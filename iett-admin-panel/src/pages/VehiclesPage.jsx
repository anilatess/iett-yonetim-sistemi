import { useEffect, useState } from "react";
import "./VehiclesPage.css";

import {
  createVehicle,
  deleteVehicle,
  getVehicles,
  updateVehicle,
} from "../services/vehicleService";

const VEHICLE_STATUS_API_URL =
  "https://localhost:7034/api/VehicleStatuses";

function VehiclesPage() {
  const [vehicles, setVehicles] = useState([]);
  const [vehicleStatuses, setVehicleStatuses] = useState([]);
  const [loading, setLoading] = useState(true);

  const [message, setMessage] = useState("");
  const [modalError, setModalError] = useState("");

  const [modalOpen, setModalOpen] = useState(false);
  const [editingVehicle, setEditingVehicle] = useState(null);

  const [formData, setFormData] = useState({
    doorNumber: "",
    vehicleStatusId: 1,
  });

  async function loadVehicles() {
    try {
      setLoading(true);
      setMessage("");

      const data = await getVehicles();
      setVehicles(data);
    } catch (error) {
      setMessage(error.message || "Araçlar getirilemedi.");
    } finally {
      setLoading(false);
    }
  }

  async function loadVehicleStatuses() {
    try {
      const response = await fetch(VEHICLE_STATUS_API_URL);

      if (!response.ok) {
        throw new Error("Araç durumları getirilemedi.");
      }

      const data = await response.json();
      setVehicleStatuses(data);
    } catch (error) {
      setMessage(
        error.message || "Araç durumları getirilemedi.",
      );
    }
  }

  useEffect(() => {
    loadVehicles();
    loadVehicleStatuses();
  }, []);

  function openCreateModal() {
    setModalError("");
    setEditingVehicle(null);

    setFormData({
      doorNumber: "",
      vehicleStatusId:
        vehicleStatuses.length > 0
          ? Number(vehicleStatuses[0].id)
          : 1,
    });

    setModalOpen(true);
  }

  function openEditModal(vehicle) {
    setModalError("");
    setEditingVehicle(vehicle);

    setFormData({
      doorNumber: vehicle.doorNumber,
      vehicleStatusId: Number(vehicle.vehicleStatusId),
    });

    setModalOpen(true);
  }

  function closeModal() {
    setModalOpen(false);
    setEditingVehicle(null);
    setModalError("");
  }

  function handleInputChange(event) {
    const { name, value } = event.target;

    setFormData((current) => ({
      ...current,
      [name]:
        name === "vehicleStatusId"
          ? Number(value)
          : value,
    }));
  }

  async function handleSubmit(event) {
    event.preventDefault();

    if (!formData.doorNumber.trim()) {
      setModalError("Kapı numarası boş bırakılamaz.");
      return;
    }

    if (!formData.vehicleStatusId) {
      setModalError("Araç durumu seçilmelidir.");
      return;
    }

    try {
      setModalError("");

      if (editingVehicle) {
        await updateVehicle({
          id: editingVehicle.id,
          doorNumber: formData.doorNumber.trim(),
          vehicleStatusId: formData.vehicleStatusId,
        });
      } else {
        await createVehicle({
          doorNumber: formData.doorNumber.trim(),
          vehicleStatusId: formData.vehicleStatusId,
        });
      }

      closeModal();
      await loadVehicles();
    } catch (error) {
      setModalError(
        error.message || "Araç kaydedilirken hata oluştu.",
      );
    }
  }

  async function handleDelete(vehicle) {
    const confirmed = window.confirm(
      `${vehicle.doorNumber} numaralı aracı silmek istediğine emin misin?`,
    );

    if (!confirmed) {
      return;
    }

    try {
      setMessage("");

      await deleteVehicle(vehicle.id);
      await loadVehicles();
    } catch (error) {
      setMessage(
        error.message || "Araç silinirken hata oluştu.",
      );
    }
  }

  async function handleQuickStatusChange(
    vehicle,
    newStatusId,
  ) {
    const previousVehicles = vehicles;

    try {
      setMessage("");

      setVehicles((currentVehicles) =>
        currentVehicles.map((item) =>
          item.id === vehicle.id
            ? {
                ...item,
                vehicleStatusId: Number(newStatusId),
              }
            : item,
        ),
      );

      await updateVehicle({
        id: vehicle.id,
        doorNumber: vehicle.doorNumber,
        vehicleStatusId: Number(newStatusId),
      });
    } catch (error) {
      setVehicles(previousVehicles);

      setMessage(
        error.message ||
          "Araç durumu değiştirilirken hata oluştu.",
      );
    }
  }

  return (
    <div className="vehicles-page">
      <div className="page-header">
        <div>
          <h1>Araç Yönetimi</h1>
          <p>
            Veritabanında kayıtlı araçları
            yönetebilirsiniz.
          </p>
        </div>

        <button
          type="button"
          className="add-button"
          onClick={openCreateModal}
        >
          Yeni Araç
        </button>
      </div>

      {message && (
        <div className="alert-message">{message}</div>
      )}

      <div className="table-card">
        {loading ? (
          <div className="table-state">
            Araçlar yükleniyor...
          </div>
        ) : vehicles.length === 0 ? (
          <div className="table-state">
            Kayıtlı araç bulunamadı.
          </div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Kapı Numarası</th>
                <th>Durum</th>
                <th>İşlemler</th>
              </tr>
            </thead>

            <tbody>
              {vehicles.map((vehicle) => (
                <tr key={vehicle.id}>
                  <td>{vehicle.id}</td>
                  <td>{vehicle.doorNumber}</td>

                  <td>
                    <select
                      className="status-select"
                      value={vehicle.vehicleStatusId}
                      onChange={(event) =>
                        handleQuickStatusChange(
                          vehicle,
                          Number(event.target.value),
                        )
                      }
                      disabled={
                        vehicleStatuses.length === 0
                      }
                    >
                      {vehicleStatuses.length === 0 ? (
                        <option value="">
                          Yükleniyor...
                        </option>
                      ) : (
                        vehicleStatuses.map((status) => (
                          <option
                            key={status.id}
                            value={status.id}
                          >
                            {status.name}
                          </option>
                        ))
                      )}
                    </select>
                  </td>

                  <td>
                    <button
                      type="button"
                      className="edit-button"
                      onClick={() => openEditModal(vehicle)}
                    >
                      Düzenle
                    </button>

                    <button
                      type="button"
                      className="delete-button"
                      onClick={() => handleDelete(vehicle)}
                    >
                      Sil
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {modalOpen && (
        <div
          className="modal-backdrop"
          onMouseDown={closeModal}
        >
          <div
            className="modal-card"
            onMouseDown={(event) =>
              event.stopPropagation()
            }
          >
            <div className="modal-header">
              <div>
                <h2>
                  {editingVehicle
                    ? "Aracı Düzenle"
                    : "Yeni Araç Ekle"}
                </h2>

                <p>Araç bilgilerini doldurun.</p>
              </div>

              <button
                type="button"
                className="modal-close"
                onClick={closeModal}
              >
                ×
              </button>
            </div>

            {modalError && (
              <div className="modal-error">
                {modalError}
              </div>
            )}

            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label htmlFor="doorNumber">
                  Kapı Numarası
                </label>

                <input
                  id="doorNumber"
                  name="doorNumber"
                  type="text"
                  value={formData.doorNumber}
                  onChange={handleInputChange}
                  placeholder="Örnek: A-4001"
                  autoFocus
                />
              </div>

              <div className="form-group">
                <label htmlFor="vehicleStatusId">
                  Araç Durumu
                </label>

                <select
                  id="vehicleStatusId"
                  name="vehicleStatusId"
                  value={formData.vehicleStatusId}
                  onChange={handleInputChange}
                  disabled={
                    vehicleStatuses.length === 0
                  }
                >
                  {vehicleStatuses.length === 0 ? (
                    <option value="">
                      Durumlar yükleniyor...
                    </option>
                  ) : (
                    vehicleStatuses.map((status) => (
                      <option
                        key={status.id}
                        value={status.id}
                      >
                        {status.name}
                      </option>
                    ))
                  )}
                </select>
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="cancel-button"
                  onClick={closeModal}
                >
                  Vazgeç
                </button>

                <button
                  type="submit"
                  className="save-button"
                  disabled={
                    vehicleStatuses.length === 0
                  }
                >
                  {editingVehicle
                    ? "Değişiklikleri Kaydet"
                    : "Aracı Kaydet"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default VehiclesPage;