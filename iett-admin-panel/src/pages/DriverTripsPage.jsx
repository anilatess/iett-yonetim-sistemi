import { useEffect, useState } from "react";
import "./DriverTripsPage.css";

import { getMyTrips } from "../services/driverService";

const STATUS_LABELS = {
  Planned: "Planlandı",
  InProgress: "Devam Ediyor",
  Completed: "Tamamlandı",
  Cancelled: "İptal Edildi",
};

function formatDate(date) {
  if (!date) return "-";

  const parsedDate = new Date(date);

  if (Number.isNaN(parsedDate.getTime())) return "-";

  return new Intl.DateTimeFormat("tr-TR", {
    day: "2-digit",
    month: "long",
    year: "numeric",
  }).format(parsedDate);
}

function formatTime(time) {
  return time ? time.slice(0, 5) : "-";
}

export default function DriverTripsPage() {
  const [trips, setTrips] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let isActive = true;

    async function loadTrips() {
      try {
        const data = await getMyTrips();

        if (isActive) {
          setTrips(Array.isArray(data) ? data : []);
        }
      } catch (requestError) {
        if (isActive) {
          setError(requestError.message || "Seferleriniz yüklenirken bir hata oluştu.");
        }
      } finally {
        if (isActive) {
          setLoading(false);
        }
      }
    }

    loadTrips();

    return () => {
      isActive = false;
    };
  }, []);

  return (
    <div className="driver-trips-page">
      <div className="page-header">
        <div>
          <h1>Seferlerim</h1>
          <p>Size atanmış seferleri görüntüleyebilirsiniz.</p>
        </div>
      </div>

      {error && <div className="alert-message">{error}</div>}

      <div className="table-card">
        {loading ? (
          <div className="table-state">Seferleriniz yükleniyor...</div>
        ) : !error && trips.length === 0 ? (
          <div className="table-state">Size atanmış sefer bulunamadı.</div>
        ) : !error ? (
          <table>
            <thead>
              <tr>
                <th>Tarih</th>
                <th>Hat</th>
                <th>Araç</th>
                <th>Kalkış Saati</th>
                <th>Varış Saati</th>
                <th>Sefer Durumu</th>
              </tr>
            </thead>
            <tbody>
              {trips.map((trip) => (
                <tr key={trip.id}>
                  <td>{formatDate(trip.tripDate)}</td>
                  <td>
                    <span className="route-code">{trip.routeCode || "-"}</span>
                    <span className="route-name">{trip.routeName || "-"}</span>
                  </td>
                  <td>{trip.vehicleDoorNumber || "-"}</td>
                  <td>{formatTime(trip.depertureTime)}</td>
                  <td>{formatTime(trip.arrivalTime)}</td>
                  <td>
                    <span className={`trip-status trip-status--${trip.tripStatus || "unknown"}`}>
                      {STATUS_LABELS[trip.tripStatus] || trip.tripStatus || "-"}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : null}
      </div>
    </div>
  );
}
