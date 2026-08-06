import { useEffect, useMemo, useState } from "react";
import "./TripsPage.css";

import { getTrips } from "../services/tripService";

const tripStatusNames = {
  1: "Planlandı",
  2: "Devam Ediyor",
  3: "Tamamlandı",
  4: "İptal Edildi",
};

function formatDate(value) {
  if (!value) return "-";

  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "-" : date.toLocaleDateString("tr-TR");
}

function formatTime(value) {
  if (!value) return "-";

  const [hours, minutes] = String(value).split(":");
  return hours !== undefined && minutes !== undefined
    ? `${hours.padStart(2, "0")}:${minutes.padStart(2, "0")}`
    : "-";
}

function TripsPage() {
  const [trips, setTrips] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");

  useEffect(() => {
    let isActive = true;

    async function loadTrips() {
      try {
        const data = await getTrips();
        if (isActive) setTrips(Array.isArray(data) ? data : []);
      } catch (requestError) {
        if (isActive) setError(requestError.message || "Seferler getirilemedi.");
      } finally {
        if (isActive) setLoading(false);
      }
    }

    loadTrips();
    return () => {
      isActive = false;
    };
  }, []);

  const filteredTrips = useMemo(() => {
    const searchValue = search.trim().toLocaleLowerCase("tr-TR");
    if (!searchValue) return trips;

    return trips.filter((trip) =>
      [
        trip.driverFullName,
        trip.personnelNumber,
        trip.routeCode,
        trip.routeName,
        trip.vehicleDoorNumber,
      ].some((value) =>
        String(value ?? "").toLocaleLowerCase("tr-TR").includes(searchValue),
      ),
    );
  }, [search, trips]);

  return (
    <section className="trips-page">
      <header className="page-header trips-header">
        <div>
          <h1>Seferler</h1>
          <p>Planlanan seferleri ve görevlendirilen şoförleri görüntüleyin.</p>
        </div>
        <div className="trip-count">
          {search.trim()
            ? `${filteredTrips.length} / ${trips.length} sefer gösteriliyor`
            : `Toplam ${trips.length} sefer`}
        </div>
      </header>

      <div className="trips-toolbar">
        <label className="trip-search-label" htmlFor="trip-search">Seferlerde ara</label>
        <input
          id="trip-search"
          type="search"
          className="trip-search"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Şoför, personel no, hat veya araç kapı no ara..."
        />
      </div>

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
                  <th>Sefer ID</th><th>Tarih</th><th>Şoför</th><th>Personel Numarası</th>
                  <th>Garaj</th><th>Araç Kapı Numarası</th><th>Hat Kodu</th><th>Hat Adı</th>
                  <th>Planlanan Kalkış</th><th>Planlanan Varış</th><th>Durum</th>
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
                    <td><span className="trip-route-code">{trip.routeCode || "-"}</span></td>
                    <td>{trip.routeName || "-"}</td>
                    <td>{formatTime(trip.depertureTime)}</td>
                    <td>{formatTime(trip.arrivalTime)}</td>
                    <td>
                      <span className={`trip-status trip-status-${trip.tripStatus}`}>
                        {trip.tripStatusName || tripStatusNames[trip.tripStatus] || "Bilinmiyor"}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  );
}

export default TripsPage;
