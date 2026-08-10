import { useEffect, useState } from "react";
import "./VehiclesPage.css";
import { createVehicle, deleteVehicle, getVehicles, updateVehicle, updateVehicleStatus } from "../services/vehicleService";
import { apiFetch } from "../services/apiClient";
import { API_URL } from "../config/apiConfig";

const STATUS_URL = `${API_URL}/VehicleStatuses`;
const MODELS = ["OTOKAR/KENT 290LF", "KARSAN/AVANCITY S PLUS"];
const MIN_YEAR = 1980;
const MAX_YEAR = new Date().getFullYear() + 1;
const emptyForm = { doorNumber: "", licensePlate: "", model: "", productionYear: "", capacity: "", vehicleStatusId: 1 };

function VehiclesPage({ canManageVehicles = false, canChangeVehicleStatus = false }) {
  const [vehicles, setVehicles] = useState([]);
  const [statuses, setStatuses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState("");
  const [modalError, setModalError] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);

  async function loadVehicles() {
    try { setLoading(true); setMessage(""); setVehicles(await getVehicles()); }
    catch (error) { setMessage(error.message || "Araçlar getirilemedi."); }
    finally { setLoading(false); }
  }
  async function loadStatuses() {
    try { setStatuses(await apiFetch(STATUS_URL)); }
    catch (error) { setMessage(error.message || "Araç durumları getirilemedi."); }
  }
  useEffect(() => { loadVehicles(); loadStatuses(); }, []);

  function openCreate() {
    setEditing(null); setModalError("");
    setForm({ ...emptyForm, vehicleStatusId: Number(statuses[0]?.id || 1) }); setModalOpen(true);
  }
  function openEdit(vehicle) {
    setEditing(vehicle); setModalError("");
    setForm({ doorNumber: vehicle.doorNumber, licensePlate: vehicle.licensePlate, model: vehicle.model, productionYear: String(vehicle.productionYear), capacity: String(vehicle.capacity), vehicleStatusId: Number(vehicle.vehicleStatusId) });
    setModalOpen(true);
  }
  function closeModal() { if (!submitting) { setModalOpen(false); setEditing(null); setModalError(""); } }
  function change({ target: { name, value } }) { setForm((old) => ({ ...old, [name]: name === "vehicleStatusId" ? Number(value) : value })); }

  function validationError() {
    if (!form.doorNumber.trim()) return "Kapı numarası boş bırakılamaz.";
    if (!form.licensePlate.trim()) return "Plaka boş bırakılamaz.";
    if (!form.model.trim()) return "Model boş bırakılamaz.";
    const capacity = Number(form.capacity);
    if (!Number.isInteger(capacity) || capacity < 1 || capacity > 300) return "Kapasite 1 ile 300 arasında olmalıdır.";
    const year = Number(form.productionYear);
    if (!Number.isInteger(year) || year < MIN_YEAR || year > MAX_YEAR) return `Üretim yılı ${MIN_YEAR} ile ${MAX_YEAR} arasında olmalıdır.`;
    if (!form.vehicleStatusId) return "Araç durumu seçilmelidir.";
    return "";
  }
  async function submit(event) {
    event.preventDefault(); const error = validationError();
    if (error) { setModalError(error); return; }
    const payload = { doorNumber: form.doorNumber.trim(), licensePlate: form.licensePlate.trim(), model: form.model.trim(), productionYear: Number(form.productionYear), capacity: Number(form.capacity), vehicleStatusId: Number(form.vehicleStatusId) };
    try {
      setSubmitting(true); setModalError("");
      if (editing) await updateVehicle({ id: editing.id, ...payload }); else await createVehicle(payload);
      setModalOpen(false); setEditing(null); await loadVehicles();
    } catch (submitError) { setModalError(submitError.message || "Araç kaydedilirken hata oluştu."); }
    finally { setSubmitting(false); }
  }
  async function remove(vehicle) {
    if (!window.confirm(`${vehicle.doorNumber} numaralı aracı silmek istediğinize emin misiniz?`)) return;
    try { await deleteVehicle(vehicle.id); await loadVehicles(); }
    catch (error) { setMessage(error.message || "Araç silinirken hata oluştu."); }
  }
  async function changeStatus(vehicle, value) {
    const id = Number(value); if (id === Number(vehicle.vehicleStatusId)) return;
    const name = statuses.find((status) => Number(status.id) === id)?.name || "seçilen durum";
    if (!window.confirm(`${vehicle.doorNumber} numaralı aracın durumunu "${name}" olarak değiştirmek istediğinize emin misiniz?`)) return;
    const old = vehicles;
    try { setVehicles((items) => items.map((item) => item.id === vehicle.id ? { ...item, vehicleStatusId: id } : item)); await updateVehicleStatus(vehicle.id, id); }
    catch (error) { setVehicles(old); setMessage(error.message || "Araç durumu değiştirilemedi."); }
  }
  const statusName = (id) => statuses.find((status) => Number(status.id) === Number(id))?.name || "-";

  return <div className="vehicles-page">
    <div className="page-header"><div><h1>{canManageVehicles ? "Araç Yönetimi" : "Araçlar"}</h1><p>Kayıtlı araç bilgilerini görüntüleyebilirsiniz.</p></div>{canManageVehicles && <button className="add-button" type="button" onClick={openCreate}>Yeni Araç</button>}</div>
    {message && <div className="alert-message">{message}</div>}
    <div className="table-card vehicles-table-wrapper">{loading ? <div className="table-state">Araçlar yükleniyor...</div> : vehicles.length === 0 ? <div className="table-state">Kayıtlı araç bulunamadı.</div> :
      <table><thead><tr><th>Kapı Numarası</th><th>Plaka</th><th>Model</th><th>Üretim Yılı</th><th>Kapasite</th><th>Durum</th>{canManageVehicles && <th>İşlemler</th>}</tr></thead><tbody>{vehicles.map((vehicle) =>
        <tr key={vehicle.id}><td>{vehicle.doorNumber}</td><td>{vehicle.licensePlate}</td><td>{vehicle.model}</td><td>{vehicle.productionYear}</td><td>{vehicle.capacity}</td><td>{canChangeVehicleStatus ? <select className="status-select" value={vehicle.vehicleStatusId} onChange={(event) => changeStatus(vehicle, event.target.value)} disabled={!statuses.length}>{statuses.map((status) => <option key={status.id} value={status.id}>{status.name}</option>)}</select> : <span className="status-badge">{statusName(vehicle.vehicleStatusId)}</span>}</td>{canManageVehicles && <td className="vehicle-actions"><button className="edit-button" type="button" onClick={() => openEdit(vehicle)}>Düzenle</button><button className="delete-button" type="button" onClick={() => remove(vehicle)}>Sil</button></td>}</tr>)}</tbody></table>}
    </div>
    {canManageVehicles && modalOpen && <div className="modal-backdrop" onMouseDown={closeModal}><div className="modal-card vehicle-modal" onMouseDown={(event) => event.stopPropagation()}><div className="modal-header"><div><h2>{editing ? "Aracı Düzenle" : "Yeni Araç Ekle"}</h2><p>Araç bilgilerini doldurun.</p></div><button className="modal-close" type="button" onClick={closeModal} disabled={submitting}>×</button></div>{modalError && <div className="modal-error">{modalError}</div>}
      <form onSubmit={submit}><div className="vehicle-form-grid">
        <Field label="Kapı Numarası"><input name="doorNumber" value={form.doorNumber} onChange={change} placeholder="A-4001" required autoFocus /></Field>
        <Field label="Plaka"><input name="licensePlate" value={form.licensePlate} onChange={change} maxLength="20" placeholder="34 IET 101" required /></Field>
        <Field label="Model" wide><input name="model" list="vehicle-models" value={form.model} onChange={change} maxLength="150" required /><datalist id="vehicle-models">{MODELS.map((model) => <option key={model} value={model} />)}</datalist></Field>
        <Field label="Üretim Yılı"><input name="productionYear" type="number" min={MIN_YEAR} max={MAX_YEAR} value={form.productionYear} onChange={change} required /></Field>
        <Field label="Kapasite"><input name="capacity" type="number" min="1" max="300" value={form.capacity} onChange={change} required /></Field>
        <Field label="Araç Durumu" wide><select name="vehicleStatusId" value={form.vehicleStatusId} onChange={change} disabled={!statuses.length} required>{statuses.map((status) => <option key={status.id} value={status.id}>{status.name}</option>)}</select></Field>
      </div><div className="modal-actions"><button className="cancel-button" type="button" onClick={closeModal} disabled={submitting}>Vazgeç</button><button className="save-button" type="submit" disabled={submitting || !statuses.length}>{submitting ? "Kaydediliyor..." : editing ? "Değişiklikleri Kaydet" : "Aracı Kaydet"}</button></div></form>
    </div></div>}
  </div>;
}

function Field({ label, wide = false, children }) { return <div className={`form-group${wide ? " vehicle-form-wide" : ""}`}><label>{label}</label>{children}</div>; }
export default VehiclesPage;
