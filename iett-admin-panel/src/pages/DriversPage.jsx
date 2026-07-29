import { useEffect, useMemo, useState } from "react";
import "./DriversPage.css";

import { getDrivers } from "../services/driverService";

function DriversPage() {
  const [drivers, setDrivers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState("");
  const [search, setSearch] = useState("");

  async function loadDrivers() {
    try {
      setLoading(true);
      setMessage("");

      const data = await getDrivers();
      setDrivers(data);
    } catch (error) {
      setMessage(error.message || "Şoförler getirilemedi.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadDrivers();
  }, []);

  const filteredDrivers = useMemo(() => {
    const searchValue = search
      .trim()
      .toLocaleLowerCase("tr-TR");

    if (!searchValue) {
      return drivers;
    }

    return drivers.filter((driver) =>
      [
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
      ),
    );
  }, [drivers, search]);

  return (
    <div className="drivers-page">
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

      {message && (
        <div className="alert-message">{message}</div>
      )}

      <div className="drivers-toolbar">
        <input
          type="text"
          className="driver-search"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Şoför, personel no veya garaj ara..."
        />

        <button
          type="button"
          className="refresh-button"
          onClick={loadDrivers}
          disabled={loading}
        >
          {loading ? "Yenileniyor..." : "Listeyi Yenile"}
        </button>
      </div>

      <div className="table-card">
        {loading ? (
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
    </div>
  );
}

export default DriversPage;