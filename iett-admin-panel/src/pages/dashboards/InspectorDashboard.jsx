import { useEffect, useState } from "react";
import { getInspectorDashboard } from "../../services/inspectorService";
import "./Dashboard.css";

function formatTime(value) {
  if (!value) return "-";
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? "-"
    : date.toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" });
}

function DashboardState({ loading, error, empty, children }) {
  if (loading) return <div className="inspector-section-state">Yükleniyor...</div>;
  if (error) return <div className="inspector-section-state inspector-section-error">{error}</div>;
  if (empty) return <div className="inspector-section-state">{empty}</div>;
  return children;
}

function TripList({ trips }) {
  return (
    <div className="inspector-dashboard-list">
      {trips.map((trip) => (
        <article className="inspector-trip-item" key={trip.tripId}>
          <div className="inspector-item-heading">
            <strong>Araç {trip.vehicleDoorNumber || "-"}</strong>
            <span className={`inspector-status inspector-status--trip-${trip.tripStatus}`}>
              {trip.tripStatusName || "Bilinmiyor"}
            </span>
          </div>
          <div className="inspector-item-primary">{trip.driverFullName || "-"}</div>
          <div className="inspector-item-meta">
            <span>Personel No: {trip.personnelNumber || "-"}</span>
            <span>Hat: {trip.routeCode || "-"} · {trip.routeName || "-"}</span>
            <span>{formatTime(trip.plannedDepartureDateTime)} – {formatTime(trip.plannedArrivalDateTime)}</span>
          </div>
        </article>
      ))}
    </div>
  );
}

function ComplaintList({ complaints }) {
  return (
    <div className="inspector-dashboard-list">
      {complaints.map((complaint) => (
        <article className="inspector-complaint-item" key={complaint.complaintId}>
          <div className="inspector-item-heading">
            <strong>{complaint.trackingCode || "Takip kodu yok"}</strong>
            <span className="inspector-status">{complaint.statusName || "Bilinmiyor"}</span>
          </div>
          <div className="inspector-item-primary">{complaint.complaintTypeName || "Şikâyet"}</div>
          <p className="inspector-complaint-description">{complaint.complaintDescription || "Açıklama bulunmuyor."}</p>
          <div className="inspector-item-meta">
            <span>{formatTime(complaint.createdDate)}</span>
            {complaint.driverFullName && <span>{complaint.driverFullName}{complaint.personnelNumber ? ` · ${complaint.personnelNumber}` : ""}</span>}
            {(complaint.routeCode || complaint.vehicleDoorNumber) && (
              <span>Hat {complaint.routeCode || "-"} · Araç {complaint.vehicleDoorNumber || "-"}</span>
            )}
          </div>
        </article>
      ))}
    </div>
  );
}

export default function InspectorDashboard({ currentUser }) {
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let isActive = true;

    async function loadDashboard() {
      try {
        const data = await getInspectorDashboard();
        if (isActive) setDashboard(data);
      } catch (requestError) {
        if (isActive) setError(requestError.message || "Dashboard verileri yüklenemedi.");
      } finally {
        if (isActive) setLoading(false);
      }
    }

    loadDashboard();
    return () => {
      isActive = false;
    };
  }, []);

  const activeTrips = dashboard?.activeTrips || [];
  const cancelledTrips = dashboard?.cancelledTripsToday || [];
  const complaints = dashboard?.complaintsToday || [];

  return (
    <section className="dashboard-page inspector-dashboard-page">
      <header className="inspector-garage-banner">
        <div>
          <span className="inspector-garage-eyebrow">Bağlı Garaj</span>
          <h1>{loading ? "Garaj bilgisi yükleniyor..." : dashboard?.garage?.garageName || "Garaj bilgisi bulunamadı"}</h1>
          {currentUser?.fullName && <p>Denetimci: {currentUser.fullName}</p>}
        </div>
        {!loading && !error && dashboard?.garage && (
          <div className="inspector-driver-count">
            <strong>{dashboard.garage.totalDriverCount}</strong>
            <span>Bağlı şoför</span>
          </div>
        )}
      </header>

      {error && <div className="inspector-dashboard-alert">{error}</div>}

      <div className="inspector-dashboard-columns">
        <div className="inspector-dashboard-left">
          <section className="inspector-dashboard-panel">
            <h2>Aktif Görevdeki Araçlar</h2>
            <DashboardState loading={loading} error={error} empty={!activeTrips.length ? "Şu anda aktif görev bulunmuyor." : ""}>
              <TripList trips={activeTrips} />
            </DashboardState>
          </section>

          <section className="inspector-dashboard-panel">
            <h2>Bugünün İptal Edilen Görevleri</h2>
            <DashboardState loading={loading} error={error} empty={!cancelledTrips.length ? "Bugün iptal edilmiş görev bulunmuyor." : ""}>
              <TripList trips={cancelledTrips} />
            </DashboardState>
          </section>
        </div>

        <section className="inspector-dashboard-panel inspector-complaints-panel">
          <h2>Bugünün Şikâyetleri</h2>
          <DashboardState loading={loading} error={error} empty={!complaints.length ? "Bugün garajınızla ilişkili şikâyet bulunmuyor." : ""}>
            <ComplaintList complaints={complaints} />
          </DashboardState>
        </section>
      </div>
    </section>
  );
}
