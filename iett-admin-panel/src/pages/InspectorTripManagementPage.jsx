import { useEffect, useMemo, useState } from "react";
import "./TripsPage.css";
import "./InspectorTripManagementPage.css";
import { getBusRoutes } from "../services/busRouteService";
import { cancelInspectorTrip, createInspectorTrip, getInspectorDrivers, getInspectorTrips, updateInspectorTrip } from "../services/inspectorService";
import { getVehicles } from "../services/vehicleService";

const EMPTY_FORM = { driverId: "", vehicleId: "", routeId: "", plannedDepartureDateTime: "", plannedArrivalDateTime: "" };
const STATUS_NAMES = { 1: "Planlandı", 2: "Devam Ediyor", 3: "Tamamlandı", 4: "İptal Edildi" };

function getStatus(trip) {
  const number = Number(trip.tripStatus);
  if (STATUS_NAMES[number]) return number;
  const name = String(trip.tripStatusName || "").toLocaleLowerCase("tr-TR");
  return Number(Object.entries(STATUS_NAMES).find(([, value]) => value.toLocaleLowerCase("tr-TR") === name)?.[0] || 0);
}

function formatDateTime(value) {
  const date = new Date(value);
  return !value || Number.isNaN(date.getTime()) ? "-" : date.toLocaleString("tr-TR", { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit" });
}

function toInputDateTime(value) {
  const match = String(value || "").match(/^(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2})/);
  return match ? `${match[1]}T${match[2]}` : "";
}

const datePart = (value) => String(value || "").slice(0, 10);

export default function InspectorTripManagementPage() {
  const [trips, setTrips] = useState([]);
  const [drivers, setDrivers] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  const [routes, setRoutes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [referencesLoading, setReferencesLoading] = useState(true);
  const [error, setError] = useState("");
  const [referenceError, setReferenceError] = useState("");
  const [success, setSuccess] = useState("");
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [dateFilter, setDateFilter] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingTrip, setEditingTrip] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [cancellingId, setCancellingId] = useState(null);

  async function loadTrips() {
    try {
      setLoading(true); setError("");
      const data = await getInspectorTrips();
      setTrips(Array.isArray(data) ? data : []);
    } catch (requestError) {
      setError(requestError.message || "Görevler yüklenirken bir hata oluştu.");
    } finally { setLoading(false); }
  }

  async function loadReferences() {
    try {
      setReferencesLoading(true); setReferenceError("");
      const [driverData, vehicleData, routeData] = await Promise.all([getInspectorDrivers(), getVehicles(), getBusRoutes()]);
      setDrivers(Array.isArray(driverData) ? driverData : []);
      setVehicles((Array.isArray(vehicleData) ? vehicleData : []).filter((vehicle) => Number(vehicle.vehicleStatusId) === 1));
      setRoutes(Array.isArray(routeData) ? routeData : []);
    } catch (requestError) {
      setReferenceError(requestError.message || "Form seçenekleri yüklenemedi.");
    } finally { setReferencesLoading(false); }
  }

  useEffect(() => { loadTrips(); loadReferences(); }, []);

  const summary = useMemo(() => ({
    total: trips.length,
    planned: trips.filter((trip) => getStatus(trip) === 1).length,
    inProgress: trips.filter((trip) => getStatus(trip) === 2).length,
    completed: trips.filter((trip) => getStatus(trip) === 3).length,
    cancelled: trips.filter((trip) => getStatus(trip) === 4).length,
  }), [trips]);

  const filteredTrips = useMemo(() => {
    const query = search.trim().toLocaleLowerCase("tr-TR");
    return trips.filter((trip) => {
      const matchesSearch = !query || [trip.driverFullName, trip.personnelNumber].some((value) => String(value || "").toLocaleLowerCase("tr-TR").includes(query));
      return matchesSearch && (!statusFilter || getStatus(trip) === Number(statusFilter)) && (!dateFilter || datePart(trip.plannedDepartureDateTime) === dateFilter);
    });
  }, [trips, search, statusFilter, dateFilter]);

  function openCreateModal() {
    setEditingTrip(null); setForm(EMPTY_FORM); setFormError(referenceError); setModalOpen(true);
  }

  function openEditModal(trip) {
    const driver = drivers.find((item) => item.personnelNumber === trip.personnelNumber);
    const vehicle = vehicles.find((item) => item.doorNumber === trip.vehicleDoorNumber);
    const route = routes.find((item) => item.routeCode === trip.routeCode);
    setEditingTrip(trip);
    setForm({ driverId: driver ? String(driver.id) : "", vehicleId: vehicle ? String(vehicle.id) : "", routeId: route ? String(route.id) : "", plannedDepartureDateTime: toInputDateTime(trip.plannedDepartureDateTime), plannedArrivalDateTime: toInputDateTime(trip.plannedArrivalDateTime) });
    setFormError(driver && vehicle && route ? referenceError : "Mevcut görev seçenekleri eşleştirilemedi.");
    setModalOpen(true);
  }

  function closeModal() {
    if (submitting) return;
    setModalOpen(false); setEditingTrip(null); setForm(EMPTY_FORM); setFormError("");
  }

  function validateForm() {
    if (Object.values(form).some((value) => !value)) return "Tüm alanları doldurunuz.";
    const departure = new Date(form.plannedDepartureDateTime);
    const arrival = new Date(form.plannedArrivalDateTime);
    if (departure >= arrival) return "Kalkış zamanı varış zamanından önce olmalıdır.";
    if (departure < new Date()) return "Kalkış zamanı geçmişte olamaz.";
    if (datePart(form.plannedDepartureDateTime) !== datePart(form.plannedArrivalDateTime)) return "Kalkış ve varış aynı gün içinde olmalıdır.";
    return "";
  }

  async function handleSubmit(event) {
    event.preventDefault();
    const validationError = validateForm();
    if (validationError) { setFormError(validationError); return; }
    const payload = { driverId: Number(form.driverId), vehicleId: Number(form.vehicleId), routeId: Number(form.routeId), plannedDepartureDateTime: form.plannedDepartureDateTime, plannedArrivalDateTime: form.plannedArrivalDateTime };
    try {
      setSubmitting(true); setFormError(""); setSuccess("");
      if (editingTrip) await updateInspectorTrip(editingTrip.tripId, payload);
      else await createInspectorTrip(payload);
      await loadTrips();
      setModalOpen(false); setForm(EMPTY_FORM);
      setSuccess(editingTrip ? "Görev başarıyla güncellendi." : "Yeni görev başarıyla atandı.");
      setEditingTrip(null);
    } catch (requestError) {
      setFormError(requestError.message || "Görev kaydedilemedi.");
    } finally { setSubmitting(false); }
  }

  async function handleCancel(trip) {
    if (!window.confirm("Bu görevi iptal etmek istediğinize emin misiniz?")) return;
    try {
      setCancellingId(trip.tripId); setError(""); setSuccess("");
      await cancelInspectorTrip(trip.tripId); await loadTrips(); setSuccess("Görev başarıyla iptal edildi.");
    } catch (requestError) {
      setError(requestError.message || "Görev iptal edilemedi.");
    } finally { setCancellingId(null); }
  }

  return <section className="trips-page inspector-trip-page">
    <header className="page-header trips-header"><div><h1>Şoför Görev Yönetimi</h1><p>Garajınızdaki şoförlerin görevlerini planlayın ve yönetin.</p></div><button type="button" className="plan-trip-button" onClick={openCreateModal}>Yeni Görev Ata</button></header>
    <div className="trip-summary-grid"><Summary label="Toplam görev" value={summary.total} /><Summary label="Planlanan" value={summary.planned} tone="blue" /><Summary label="Devam eden" value={summary.inProgress} tone="amber" /><Summary label="Tamamlanan" value={summary.completed} tone="green" /><Summary label="İptal edilen" value={summary.cancelled} tone="red" /></div>
    <div className="inspector-trip-filters"><label>Şoför ara<input type="search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Ad veya personel numarası" /></label><label>Sefer durumu<select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}><option value="">Tümü</option>{Object.entries(STATUS_NAMES).map(([id, name]) => <option key={id} value={id}>{name}</option>)}</select></label><label>Tarih<input type="date" value={dateFilter} onChange={(event) => setDateFilter(event.target.value)} /></label><button type="button" className="clear-trip-filters" onClick={() => { setSearch(""); setStatusFilter(""); setDateFilter(""); }}>Filtreleri Temizle</button></div>
    {success && <div className="trips-success">{success}</div>}{error && <div className="trips-error">{error}</div>}
    <div className="trips-table-card">{loading ? <div className="trips-state">Görevler yükleniyor...</div> : error ? <div className="trips-state">Görev listesi görüntülenemedi.</div> : !filteredTrips.length ? <div className="trips-state">{trips.length ? "Filtrelere uygun görev bulunamadı." : "Henüz atanmış görev bulunmuyor."}</div> : <TripTable trips={filteredTrips} cancellingId={cancellingId} onEdit={openEditModal} onCancel={handleCancel} />}</div>
    {modalOpen && <TripModal editing={Boolean(editingTrip)} form={form} setForm={setForm} error={formError} loading={referencesLoading} disabled={submitting || Boolean(referenceError)} submitting={submitting} drivers={drivers} vehicles={vehicles} routes={routes} onClose={closeModal} onSubmit={handleSubmit} />}
  </section>;
}

function Summary({ label, value, tone = "default" }) { return <article className={`trip-summary-card summary-${tone}`}><span>{label}</span><strong>{value}</strong></article>; }

function TripTable({ trips, cancellingId, onEdit, onCancel }) {
  return <div className="trips-table-wrapper"><table className="inspector-trip-table"><thead><tr><th>Şoför</th><th>Personel No</th><th>Araç</th><th>Hat Kodu</th><th>Hat Adı</th><th>Planlanan Kalkış</th><th>Planlanan Varış</th><th>Durum</th><th>İşlemler</th></tr></thead><tbody>{trips.map((trip) => { const status = getStatus(trip); return <tr key={trip.tripId}><td className="trip-driver-name">{trip.driverFullName || "-"}</td><td>{trip.personnelNumber || "-"}</td><td>{trip.vehicleDoorNumber || "-"}</td><td><span className="trip-route-code">{trip.routeCode || "-"}</span></td><td>{trip.routeName || "-"}</td><td>{formatDateTime(trip.plannedDepartureDateTime)}</td><td>{formatDateTime(trip.plannedArrivalDateTime)}</td><td><span className={`trip-status trip-status-${status}`}>{trip.tripStatusName || STATUS_NAMES[status] || "Bilinmiyor"}</span></td><td>{status === 1 ? <div className="trip-row-actions"><button type="button" className="trip-edit-button" onClick={() => onEdit(trip)}>Düzenle</button><button type="button" className="trip-cancel-action" disabled={cancellingId === trip.tripId} onClick={() => onCancel(trip)}>{cancellingId === trip.tripId ? "İptal ediliyor..." : "İptal Et"}</button></div> : <span className="no-trip-action">İşlem yok</span>}</td></tr>; })}</tbody></table></div>;
}

function TripModal({ editing, form, setForm, error, loading, disabled, submitting, drivers, vehicles, routes, onClose, onSubmit }) {
  const change = (event) => setForm({ ...form, [event.target.name]: event.target.value });
  return <div className="trip-modal-backdrop" onMouseDown={onClose}><div className="trip-modal-card" role="dialog" aria-modal="true" onMouseDown={(event) => event.stopPropagation()}><div className="trip-modal-header"><div><h2>{editing ? "Görevi Düzenle" : "Yeni Görev Ata"}</h2><p>Şoför, araç, hat ve planlanan zamanları seçin.</p></div><button type="button" className="trip-modal-close" onClick={onClose} disabled={submitting}>×</button></div>{error && <div className="trip-form-error">{error}</div>}{loading ? <div className="trip-form-state">Form seçenekleri yükleniyor...</div> : <form onSubmit={onSubmit}><div className="trip-form-grid"><Field label="Şoför"><select name="driverId" value={form.driverId} onChange={change} required><option value="">Şoför seçiniz</option>{drivers.map((item) => <option key={item.id} value={item.id}>{item.fullName} ({item.personnelNumber})</option>)}</select></Field><Field label="Aktif araç"><select name="vehicleId" value={form.vehicleId} onChange={change} required><option value="">Araç seçiniz</option>{vehicles.map((item) => <option key={item.id} value={item.id}>{item.doorNumber}</option>)}</select></Field><Field label="Hat"><select name="routeId" value={form.routeId} onChange={change} required><option value="">Hat seçiniz</option>{routes.map((item) => <option key={item.id} value={item.id}>{item.routeCode} - {item.routeName}</option>)}</select></Field><Field label="Planlanan kalkış"><input name="plannedDepartureDateTime" type="datetime-local" value={form.plannedDepartureDateTime} onChange={change} required /></Field><Field label="Planlanan varış"><input name="plannedArrivalDateTime" type="datetime-local" value={form.plannedArrivalDateTime} onChange={change} required /></Field></div><div className="trip-modal-actions"><button type="button" className="trip-cancel-button" onClick={onClose} disabled={submitting}>Vazgeç</button><button type="submit" className="trip-save-button" disabled={disabled}>{submitting ? "Kaydediliyor..." : editing ? "Değişiklikleri Kaydet" : "Görevi Ata"}</button></div></form>}</div></div>;
}

function Field({ label, children }) { return <div className="trip-form-group"><label>{label}</label>{children}</div>; }
