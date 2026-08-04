import { useEffect, useMemo, useState } from "react";
import "./TripsPage.css";

import { getBusRoutes } from "../services/busRouteService";
import { getDrivers } from "../services/driverService";
import { createTrip, getTrips, updateTrip } from "../services/tripService";
import { getVehicles } from "../services/vehicleService";

const tripStatusNames = {
  1: "Planlandı",
  2: "Devam Ediyor",
  3: "Tamamlandı",
  4: "İptal Edildi",
};

const EMPTY_FORM = {
  driverId: "",
  vehicleId: "",
  routeId: "",
  tripDate: "",
  depertureTime: "",
  arrivalTime: "",
  tripStatus: "1",
};

function formatDate(value) {
  if (!value) {
    return "-";
  }

  const date = new Date(value);

  return Number.isNaN(date.getTime())
    ? "-"
    : date.toLocaleDateString("tr-TR");
}

function formatTime(value) {
  if (!value) {
    return "-";
  }

  const [hours, minutes] = String(value).split(":");
  return hours !== undefined && minutes !== undefined
    ? `${hours.padStart(2, "0")}:${minutes.padStart(2, "0")}`
    : "-";
}

function toApiTime(value) {
  return value.length === 5 ? `${value}:00` : value;
}

function toDateInput(value) {
  return value ? String(value).split("T")[0] : "";
}

function TripsPage() {
  const [trips, setTrips] = useState([]);
  const [drivers, setDrivers] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  const [busRoutes, setBusRoutes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [referenceLoading, setReferenceLoading] = useState(true);
  const [error, setError] = useState("");
  const [referenceError, setReferenceError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [search, setSearch] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingTrip, setEditingTrip] = useState(null);
  const [modalError, setModalError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState(EMPTY_FORM);

  async function loadTrips() {
    try {
      setLoading(true);
      setError("");
      const data = await getTrips();
      setTrips(Array.isArray(data) ? data : []);
    } catch (requestError) {
      setError(requestError.message || "Seferler getirilemedi.");
    } finally {
      setLoading(false);
    }
  }

  async function loadReferenceData() {
    try {
      setReferenceLoading(true);
      setReferenceError("");

      const [driverData, vehicleData, routeData] = await Promise.all([
        getDrivers(),
        getVehicles(),
        getBusRoutes(),
      ]);

      setDrivers(Array.isArray(driverData) ? driverData : []);
      setVehicles(Array.isArray(vehicleData) ? vehicleData : []);
      setBusRoutes(Array.isArray(routeData) ? routeData : []);
    } catch (requestError) {
      setReferenceError(
        requestError.message || "Sefer seçenekleri yüklenemedi.",
      );
    } finally {
      setReferenceLoading(false);
    }
  }

  useEffect(() => {
    loadTrips();
    loadReferenceData();
  }, []);

  const filteredTrips = useMemo(() => {
    const searchValue = search.trim().toLocaleLowerCase("tr-TR");

    if (!searchValue) {
      return trips;
    }

    return trips.filter((trip) =>
      [
        trip.driverFullName,
        trip.personnelNumber,
        trip.routeCode,
        trip.vehicleDoorNumber,
      ].some((value) =>
        String(value ?? "")
          .toLocaleLowerCase("tr-TR")
          .includes(searchValue),
      ),
    );
  }, [search, trips]);

  function openCreateModal() {
    setEditingTrip(null);
    setFormData(EMPTY_FORM);
    setModalError(referenceError);
    setModalOpen(true);
  }

  function openEditModal(trip) {
    setEditingTrip(trip);
    setFormData({
      driverId: String(trip.driverId),
      vehicleId: String(trip.vehicleId),
      routeId: String(trip.routeId),
      tripDate: toDateInput(trip.tripDate),
      depertureTime: formatTime(trip.depertureTime),
      arrivalTime: formatTime(trip.arrivalTime),
      tripStatus: String(trip.tripStatus),
    });
    setModalError(referenceError);
    setModalOpen(true);
  }

  function closeModal() {
    if (submitting) {
      return;
    }

    setModalOpen(false);
    setEditingTrip(null);
    setModalError("");
    setFormData(EMPTY_FORM);
  }

  function handleInputChange(event) {
    const { name, value } = event.target;

    setFormData((current) => ({
      ...current,
      [name]: value,
    }));
  }

  function validateForm() {
    if (!formData.driverId) {
      return "Şoför seçiniz.";
    }

    if (!formData.vehicleId) {
      return "Araç seçiniz.";
    }

    if (!formData.routeId) {
      return "Hat seçiniz.";
    }

    if (!formData.tripDate) {
      return "Sefer tarihi seçiniz.";
    }

    if (!formData.depertureTime) {
      return "Kalkış saati seçiniz.";
    }

    if (!formData.arrivalTime) {
      return "Varış saati seçiniz.";
    }

    if (formData.arrivalTime <= formData.depertureTime) {
      return "Varış saati kalkış saatinden sonra olmalıdır.";
    }

    if (!tripStatusNames[Number(formData.tripStatus)]) {
      return "Sefer durumu seçiniz.";
    }

    return "";
  }

  async function handleSubmit(event) {
    event.preventDefault();

    const validationError = validateForm();

    if (validationError) {
      setModalError(validationError);
      return;
    }

    const payload = {
      driverId: Number(formData.driverId),
      vehicleId: Number(formData.vehicleId),
      routeId: Number(formData.routeId),
      tripDate: formData.tripDate,
      depertureTime: toApiTime(formData.depertureTime),
      arrivalTime: toApiTime(formData.arrivalTime),
      tripStatus: Number(formData.tripStatus),
    };

    try {
      setSubmitting(true);
      setModalError("");
      setSuccessMessage("");

      if (editingTrip) {
        await updateTrip(editingTrip.id, payload);
      } else {
        await createTrip(payload);
      }

      await loadTrips();

      setModalOpen(false);
      setEditingTrip(null);
      setFormData(EMPTY_FORM);
      setSuccessMessage(
        editingTrip
          ? "Sefer başarıyla güncellendi."
          : "Sefer başarıyla planlandı.",
      );
    } catch (requestError) {
      setModalError(
        requestError.message ||
          (editingTrip
            ? "Sefer güncellenemedi."
            : "Sefer oluşturulamadı."),
      );
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="trips-page">
      <header className="page-header trips-header">
        <div>
          <h1>Seferler</h1>
          <p>Planlanan seferleri ve görevlendirilen şoförleri görüntüleyin.</p>
        </div>

        <div className="trips-header-actions">
          <div className="trip-count">
            {search.trim()
              ? `${filteredTrips.length} / ${trips.length} sefer gösteriliyor`
              : `Toplam ${trips.length} sefer`}
          </div>

          <button
            type="button"
            className="plan-trip-button"
            onClick={openCreateModal}
          >
            Yeni Sefer Planla
          </button>
        </div>
      </header>

      <div className="trips-toolbar">
        <label className="trip-search-label" htmlFor="trip-search">
          Seferlerde ara
        </label>
        <input
          id="trip-search"
          type="search"
          className="trip-search"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Şoför, personel no, hat kodu veya araç kapı no ara..."
        />
      </div>

      {successMessage && (
        <div className="trips-success">{successMessage}</div>
      )}

      {error && <div className="trips-error">{error}</div>}

      <div className="trips-table-card">
        {loading ? (
          <div className="trips-state">Seferler yükleniyor...</div>
        ) : error ? (
          <div className="trips-state">Sefer listesi görüntülenemedi.</div>
        ) : filteredTrips.length === 0 ? (
          <div className="trips-state">
            {trips.length === 0
              ? "Kayıtlı sefer bulunamadı."
              : "Arama sonucuna uygun sefer bulunamadı."}
          </div>
        ) : (
          <div className="trips-table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Sefer ID</th>
                  <th>Tarih</th>
                  <th>Şoför</th>
                  <th>Personel Numarası</th>
                  <th>Garaj</th>
                  <th>Araç Kapı Numarası</th>
                  <th>Hat Kodu</th>
                  <th>Hat Adı</th>
                  <th>Kalkış Saati</th>
                  <th>Varış Saati</th>
                  <th>Durum</th>
                  <th>İşlem</th>
                </tr>
              </thead>

              <tbody>
                {filteredTrips.map((trip) => (
                  <tr key={trip.id}>
                    <td>{trip.id}</td>
                    <td>{formatDate(trip.tripDate)}</td>
                    <td className="trip-driver-name">{trip.driverFullName || "-"}</td>
                    <td>{trip.personnelNumber || "-"}</td>
                    <td>{trip.garageName || "-"}</td>
                    <td>{trip.vehicleDoorNumber || "-"}</td>
                    <td>
                      <span className="trip-route-code">{trip.routeCode || "-"}</span>
                    </td>
                    <td>{trip.routeName || "-"}</td>
                    <td>{formatTime(trip.depertureTime)}</td>
                    <td>{formatTime(trip.arrivalTime)}</td>
                    <td>
                      <span className={`trip-status trip-status-${trip.tripStatus}`}>
                        {trip.tripStatusName || tripStatusNames[trip.tripStatus] || "Bilinmiyor"}
                      </span>
                    </td>
                    <td>
                      <button
                        type="button"
                        className="trip-edit-button"
                        onClick={() => openEditModal(trip)}
                      >
                        Düzenle
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {modalOpen && (
        <div className="trip-modal-backdrop" onMouseDown={closeModal}>
          <div
            className="trip-modal-card"
            role="dialog"
            aria-modal="true"
            aria-labelledby="trip-modal-title"
            onMouseDown={(event) => event.stopPropagation()}
          >
            <div className="trip-modal-header">
              <div>
                <h2 id="trip-modal-title">
                  {editingTrip ? "Seferi Düzenle" : "Yeni Sefer Planla"}
                </h2>
                <p>Sefer ve görevlendirme bilgilerini doldurun.</p>
              </div>

              <button
                type="button"
                className="trip-modal-close"
                onClick={closeModal}
                disabled={submitting}
                aria-label="Kapat"
              >
                ×
              </button>
            </div>

            {modalError && <div className="trip-form-error">{modalError}</div>}

            {referenceLoading ? (
              <div className="trip-form-state">Sefer seçenekleri yükleniyor...</div>
            ) : (
              <form onSubmit={handleSubmit}>
                <div className="trip-form-grid">
                  <div className="trip-form-group trip-form-wide">
                    <label htmlFor="trip-driver">Şoför</label>
                    <select id="trip-driver" name="driverId" value={formData.driverId} onChange={handleInputChange}>
                      <option value="">Şoför seçiniz</option>
                      {drivers.map((driver) => (
                        <option key={driver.id} value={driver.id}>
                          {driver.fullName} ({driver.personnelNumber})
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="trip-form-group">
                    <label htmlFor="trip-vehicle">Araç</label>
                    <select id="trip-vehicle" name="vehicleId" value={formData.vehicleId} onChange={handleInputChange}>
                      <option value="">Araç seçiniz</option>
                      {vehicles.map((vehicle) => (
                        <option key={vehicle.id} value={vehicle.id}>
                          {vehicle.doorNumber}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="trip-form-group">
                    <label htmlFor="trip-route">Hat</label>
                    <select id="trip-route" name="routeId" value={formData.routeId} onChange={handleInputChange}>
                      <option value="">Hat seçiniz</option>
                      {busRoutes.map((route) => (
                        <option key={route.id} value={route.id}>
                          {route.routeCode} - {route.routeName}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="trip-form-group trip-form-wide">
                    <label htmlFor="trip-date">Sefer tarihi</label>
                    <input id="trip-date" name="tripDate" type="date" value={formData.tripDate} onChange={handleInputChange} />
                  </div>

                  <div className="trip-form-group">
                    <label htmlFor="trip-deperture-time">Planlanan kalkış saati</label>
                    <input id="trip-deperture-time" name="depertureTime" type="time" value={formData.depertureTime} onChange={handleInputChange} />
                  </div>

                  <div className="trip-form-group">
                    <label htmlFor="trip-arrival-time">Planlanan varış saati</label>
                    <input id="trip-arrival-time" name="arrivalTime" type="time" value={formData.arrivalTime} onChange={handleInputChange} />
                  </div>

                  <div className="trip-form-group trip-form-wide">
                    <label htmlFor="trip-status">Sefer durumu</label>
                    <select id="trip-status" name="tripStatus" value={formData.tripStatus} onChange={handleInputChange}>
                      {Object.entries(tripStatusNames).map(([value, label]) => (
                        <option key={value} value={value}>{label}</option>
                      ))}
                    </select>
                  </div>
                </div>

                <div className="trip-modal-actions">
                  <button type="button" className="trip-cancel-button" onClick={closeModal} disabled={submitting}>
                    Vazgeç
                  </button>
                  <button type="submit" className="trip-save-button" disabled={submitting || Boolean(referenceError)}>
                    {submitting
                      ? editingTrip
                        ? "Kaydediliyor..."
                        : "Planlanıyor..."
                      : editingTrip
                        ? "Değişiklikleri Kaydet"
                        : "Seferi Planla"}
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      )}
    </section>
  );
}

export default TripsPage;
