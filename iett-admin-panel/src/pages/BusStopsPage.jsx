import { useEffect, useState } from "react";
import { getBusRouteStops } from "../services/busRouteService";
import "./VehiclesPage.css";

function BusStopsPage({ selectedRoute, onBack }) {
  const [routeDetails, setRouteDetails] = useState(null);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState("");

  useEffect(() => {
    async function loadStops() {
      if (!selectedRoute?.id) {
        setMessage("Seçilen hat bulunamadı.");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setMessage("");

        const data = await getBusRouteStops(
          selectedRoute.id,
        );

        setRouteDetails(data);
      } catch (error) {
        setMessage(
          error.message ||
            "Hattın durakları getirilemedi.",
        );
      } finally {
        setLoading(false);
      }
    }

    loadStops();
  }, [selectedRoute]);

  return (
    <div className="vehicles-page">
      <div className="page-header">
        <div>
          <button
            type="button"
            className="edit-button"
            onClick={onBack}
          >
            ← Hatlara Dön
          </button>

          <h1>
            {selectedRoute?.routeCode || "Hat"} Durakları
          </h1>

          <p>
            {selectedRoute?.routeName ||
              "Seçilen hattın durakları"}
          </p>
        </div>
      </div>

      {message && (
        <div className="alert-message">{message}</div>
      )}

      <div className="table-card">
        {loading ? (
          <div className="table-state">
            Duraklar yükleniyor...
          </div>
        ) : !routeDetails?.stops?.length ? (
          <div className="table-state">
            Bu hatta bağlı durak bulunamadı.
          </div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Sıra</th>
                <th>Durak Kodu</th>
                <th>Durak Adı</th>
                <th>Konum Açıklaması</th>
              </tr>
            </thead>

            <tbody>
              {routeDetails.stops.map((stop) => (
                <tr key={stop.stopId}>
                  <td>{stop.stopOrder}</td>
                  <td>{stop.stopCode}</td>
                  <td>{stop.stopName}</td>
                  <td>
                    {stop.locationDescription || "-"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

export default BusStopsPage;