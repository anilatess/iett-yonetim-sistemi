import { useCallback, useEffect, useState } from "react";
import { getDriverDashboard } from "../../services/driverService";
import "./Dashboard.css";

function formatTime(value) {
  if (!value) return "-";
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? "-"
    : date.toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" });
}

function formatDateTime(value) {
  if (!value) return "-";
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? "-"
    : new Intl.DateTimeFormat("tr-TR", {
        day: "2-digit",
        month: "short",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      }).format(date);
}

function SectionState({ loading, error, empty, children }) {
  if (loading) return <div className="driver-section-state">Yükleniyor...</div>;
  if (error) return <div className="driver-section-state driver-section-error">Veriler görüntülenemiyor.</div>;
  if (empty) return <div className="driver-section-state">{empty}</div>;
  return children;
}

export default function DriverDashboard({ onNavigate }) {
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadDashboard = useCallback(async () => {
    try {
      setLoading(true);
      setError("");
      setDashboard(await getDriverDashboard());
    } catch (requestError) {
      setError(requestError.message || "Dashboard bilgileriniz yüklenemedi.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadDashboard();
  }, [loadDashboard]);

  const driver = dashboard?.driver;
  const trips = dashboard?.todayTrips || [];
  const complaints = (dashboard?.complaints || []).slice(0, 5);

  return (
    <section className="dashboard-page driver-dashboard-page">
      <header className="driver-profile-banner">
        {loading ? (
          <div className="driver-banner-loading">Şoför bilgileri yükleniyor...</div>
        ) : driver ? (
          <>
            <div className="driver-profile-main">
              <span className="driver-profile-eyebrow">Şoför Bilgileri</span>
              <h1>{driver.fullName}</h1>
              <span className="driver-status-badge">{driver.driverStatusName}</span>
            </div>
            <dl className="driver-profile-details">
              <div><dt>Personel No</dt><dd>{driver.personnelNumber || "-"}</dd></div>
              <div><dt>Garaj</dt><dd>{driver.garageName || "-"}</dd></div>
              <div><dt>Operatör</dt><dd>{driver.operatorName || "-"}</dd></div>
              <div><dt>Tatil Günü</dt><dd>{driver.holidayDay || "-"}</dd></div>
            </dl>
          </>
        ) : null}
      </header>

      {error && (
        <div className="driver-dashboard-alert">
          <span>{error}</span>
          <button type="button" onClick={loadDashboard}>Tekrar Dene</button>
        </div>
      )}

      <div className="driver-dashboard-columns">
        <section className="driver-dashboard-panel">
          <h2>Bugünkü Seferlerim</h2>
          <SectionState loading={loading} error={error} empty={!trips.length ? "Bugün için planlanmış seferiniz bulunmuyor." : ""}>
            <div className="driver-dashboard-list">
              {trips.map((trip) => (
                <article className="driver-trip-card" key={trip.tripId}>
                  <div className="driver-item-heading">
                    <strong>{trip.routeCode || "-"} · {trip.routeName || "-"}</strong>
                    <span className={`driver-trip-status driver-trip-status--${trip.tripStatus}`}>
                      {trip.tripStatusName || "Bilinmiyor"}
                    </span>
                  </div>
                  <div className="driver-item-meta">
                    <span>Araç {trip.vehicleDoorNumber || "-"}</span>
                    <span>{formatTime(trip.plannedDepartureDateTime)} – {formatTime(trip.plannedArrivalDateTime)}</span>
                  </div>
                </article>
              ))}
            </div>
          </SectionState>
        </section>

        <section className="driver-dashboard-panel">
          <div className="driver-panel-heading">
            <h2>Bana Gelen Şikâyetler</h2>
            {!loading && !error && dashboard?.complaints?.length > 0 && (
              <button type="button" onClick={() => onNavigate("driverComplaints")}>Tümünü Gör</button>
            )}
          </div>
          <SectionState loading={loading} error={error} empty={!complaints.length ? "Size bağlı şikâyet bulunmuyor." : ""}>
            <div className="driver-dashboard-list">
              {complaints.map((complaint) => (
                <article className="driver-complaint-card" key={complaint.complaintId}>
                  <div className="driver-item-heading">
                    <strong>{complaint.trackingCode || "Takip kodu yok"}</strong>
                    <span className="driver-complaint-status">{complaint.complaintStatusName || "Bilinmiyor"}</span>
                  </div>
                  <div className="driver-complaint-type">{complaint.complaintTypeName || "Şikâyet"}</div>
                  <p>{complaint.complaintDescription || "Açıklama bulunmuyor."}</p>
                  <div className="driver-item-meta">
                    <span>{formatDateTime(complaint.createdDate)}</span>
                    <span>Hat {complaint.routeCode || "-"} · Araç {complaint.vehicleDoorNumber || "-"}</span>
                  </div>
                </article>
              ))}
            </div>
          </SectionState>
        </section>
      </div>
    </section>
  );
}
