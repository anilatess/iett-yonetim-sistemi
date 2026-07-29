import { useEffect, useState } from "react";
import "./App.css";
import iettLogo from "./assets/iett-logo.png";

import {
  createVehicle,
  deleteVehicle,
  getVehicles,
  updateVehicle,
} from "./services/vehicleService";

const API_BASE_URL = "https://localhost:7034/api";

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [activePage, setActivePage] = useState("vehicles");

  const [vehicles, setVehicles] = useState([]);
  const [vehicleStatuses, setVehicleStatuses] = useState([]);
  const [vehiclesLoading, setVehiclesLoading] = useState(true);

  const [drivers, setDrivers] = useState([]);
  const [driversLoading, setDriversLoading] = useState(false);
  const [driversLoaded, setDriversLoaded] = useState(false);
  const [driverSearch, setDriverSearch] = useState("");

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
      setVehiclesLoading(true);
      setMessage("");

      const data = await getVehicles();
      setVehicles(data);
    } catch (error) {
      setMessage(error.message || "Araçlar getirilemedi.");
    } finally {
      setVehiclesLoading(false);
    }
  }

  async function loadVehicleStatuses() {
    try {
      const response = await fetch(
        `${API_BASE_URL}/VehicleStatuses`,
      );

      if (!response.ok) {
        throw new Error("Araç durumları getirilemedi.");
      }

      const data = await response.json();
      setVehicleStatuses(data);
    } catch (error) {
      console.error(
        "Araç durumları alınırken hata oluştu:",
        error,
      );

      setMessage(
        error.message || "Araç durumları getirilemedi.",
      );
    }
  }

  async function loadDrivers() {
    try {
      setDriversLoading(true);
      setMessage("");

      const response = await fetch(`${API_BASE_URL}/Drivers`);

      if (!response.ok) {
        throw new Error("Şoförler getirilemedi.");
      }

      const data = await response.json();

      setDrivers(data);
      setDriversLoaded(true);
    } catch (error) {
      setMessage(error.message || "Şoförler getirilemedi.");
    } finally {
      setDriversLoading(false);
    }
  }

  useEffect(() => {
    loadVehicles();
    loadVehicleStatuses();
  }, []);

  useEffect(() => {
    if (activePage === "drivers" && !driversLoaded) {
      loadDrivers();
    }
  }, [activePage, driversLoaded]);

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

  const filteredDrivers = drivers.filter((driver) => {
    const searchValue = driverSearch
      .trim()
      .toLocaleLowerCase("tr-TR");

    if (!searchValue) {
      return true;
    }

    return [
      driver.id,
      driver.fullName,
      driver.maskedIdentityNumber,
      driver.personnelNumber,
      driver.garageName,
      driver.operatorName,
      driver.driverStatusName,
      driver.holidayDay,
    ].some((value) =>
      String(value ?? "")
        .toLocaleLowerCase("tr-TR")
        .includes(searchValue),
    );
  });

  function renderVehiclesPage() {
    return (
      <>
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

        <div className="table-card">
          {vehiclesLoading ? (
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
                        onClick={() =>
                          openEditModal(vehicle)
                        }
                      >
                        Düzenle
                      </button>

                      <button
                        type="button"
                        className="delete-button"
                        onClick={() =>
                          handleDelete(vehicle)
                        }
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
      </>
    );
  }

  function renderDriversPage() {
    return (
      <>
        <div className="page-header">
          <div>
            <h1>Şoförler</h1>
            <p>
              Sistemde kayıtlı şoför bilgilerini
              görüntüleyebilirsiniz.
            </p>
          </div>

          <div className="driver-count">
            Toplam {drivers.length} şoför
          </div>
        </div>

        <div className="drivers-toolbar">
          <input
            type="text"
            className="driver-search"
            value={driverSearch}
            onChange={(event) =>
              setDriverSearch(event.target.value)
            }
            placeholder="Şoför, personel no veya garaj ara..."
          />

          <button
            type="button"
            className="refresh-button"
            onClick={loadDrivers}
            disabled={driversLoading}
          >
            {driversLoading ? "Yenileniyor..." : "Listeyi Yenile"}
          </button>
        </div>

        <div className="table-card">
          {driversLoading ? (
            <div className="table-state">
              Şoförler yükleniyor...
            </div>
          ) : filteredDrivers.length === 0 ? (
            <div className="table-state">
              {drivers.length === 0
                ? "Kayıtlı şoför bulunamadı."
                : "Arama sonucuna uygun şoför bulunamadı."}
            </div>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Ad Soyad</th>
                  <th>TC Kimlik No</th>
                  <th>Personel No</th>
                  <th>Garaj</th>
                  <th>Operatör</th>
                  <th>Durum</th>
                  <th>Tatil Günü</th>
                </tr>
              </thead>

              <tbody>
                {filteredDrivers.map((driver) => (
                  <tr key={driver.id}>
                    <td>{driver.id}</td>

                    <td>
                      <div className="driver-name-cell">
                        <div className="driver-avatar">
                          {driver.fullName
                            ?.charAt(0)
                            .toLocaleUpperCase("tr-TR")}
                        </div>

                        <span>{driver.fullName}</span>
                      </div>
                    </td>

                    <td>
                      <span className="identity-number">
                        {driver.maskedIdentityNumber}
                      </span>
                    </td>

                    <td>{driver.personnelNumber}</td>

                    <td>{driver.garageName}</td>

                    <td>
                      <span className="operator-badge">
                        {driver.operatorName}
                      </span>
                    </td>

                    <td>
                      <span className="driver-status-badge">
                        {driver.driverStatusName}
                      </span>
                    </td>

                    <td>{driver.holidayDay}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </>
    );
  }

  return (
    <div
      className={`admin-layout ${
        sidebarOpen ? "" : "sidebar-closed"
      }`}
    >
      <aside className="sidebar">
        <div className="sidebar-logo">
          <img src={iettLogo} alt="İETT Logo" />

          {sidebarOpen && <span>İETT Admin</span>}
        </div>

        <nav>
          <button
            type="button"
            className={`menu-item ${
              activePage === "vehicles" ? "active" : ""
            }`}
            onClick={() => {
              setActivePage("vehicles");
              setMessage("");
            }}
          >
            <span className="menu-icon">🚌</span>
            {sidebarOpen && <span>Araçlar</span>}
          </button>

          <button
            type="button"
            className={`menu-item ${
              activePage === "drivers" ? "active" : ""
            }`}
            onClick={() => {
              setActivePage("drivers");
              setMessage("");
            }}
          >
            <span className="menu-icon">👤</span>
            {sidebarOpen && <span>Şoförler</span>}
          </button>

          <button type="button" className="menu-item">
            <span className="menu-icon">🛣️</span>
            {sidebarOpen && <span>Hatlar</span>}
          </button>

          <button type="button" className="menu-item">
            <span className="menu-icon">📍</span>
            {sidebarOpen && <span>Duraklar</span>}
          </button>

          <button type="button" className="menu-item">
            <span className="menu-icon">📋</span>
            {sidebarOpen && <span>Şikâyetler</span>}
          </button>
        </nav>
      </aside>

      <main className="content">
        <button
          type="button"
          className="sidebar-toggle"
          onClick={() =>
            setSidebarOpen((current) => !current)
          }
        >
          {sidebarOpen ? "←" : "→"}
        </button>

        {message && (
          <div className="alert-message">{message}</div>
        )}

        {activePage === "vehicles"
          ? renderVehiclesPage()
          : renderDriversPage()}
      </main>

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

export default App;