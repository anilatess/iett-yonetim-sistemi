import { useEffect, useMemo, useState } from "react";
import "./DriversPage.css";

import { getDriverCertificates, getDrivers } from "../services/driverService";

const certificateStatuses = {
  Valid: "Geçerli",
  ExpiringSoon: "Yakında sona erecek",
  Expired: "Süresi dolmuş",
};

function DriversPage({ role }) {
  const [drivers, setDrivers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState("");
  const [search, setSearch] = useState("");
  const [selectedDriver, setSelectedDriver] = useState(null);
  const [certificates, setCertificates] = useState([]);
  const [certificatesLoading, setCertificatesLoading] = useState(false);
  const [certificatesError, setCertificatesError] = useState("");

  async function showCertificates(driver) {
    setSelectedDriver(driver);
    setCertificates([]);
    setCertificatesError("");
    setCertificatesLoading(true);

    try {
      setCertificates(await getDriverCertificates(driver.id));
    } catch (error) {
      setCertificatesError(
        error.message || "Sertifikalar yüklenirken bir hata oluştu.",
      );
    } finally {
      setCertificatesLoading(false);
    }
  }

  function closeCertificates() {
    setSelectedDriver(null);
    setCertificates([]);
    setCertificatesError("");
  }

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
                <th>İşlem</th>
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
                  <td>
                    {(role === "Admin" || role === "Inspector") && (
                      <button type="button" className="certificate-button" onClick={() => showCertificates(driver)}>
                        Sertifikaları Gör
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {selectedDriver && (
        <div className="certificate-modal-backdrop" role="presentation" onMouseDown={closeCertificates}>
          <div className="certificate-modal" role="dialog" aria-modal="true" aria-labelledby="certificate-modal-title" onMouseDown={(event) => event.stopPropagation()}>
            <div className="certificate-modal-header">
              <div>
                <h2 id="certificate-modal-title">Sertifikalar</h2>
                <p>{selectedDriver.fullName}</p>
              </div>
              <button type="button" className="certificate-modal-close" onClick={closeCertificates} aria-label="Kapat">×</button>
            </div>

            {certificatesLoading ? (
              <div className="certificate-state">Sertifikalar yükleniyor...</div>
            ) : certificatesError ? (
              <div className="certificate-error">{certificatesError}</div>
            ) : certificates.length === 0 ? (
              <div className="certificate-state">Bu şoföre ait sertifika kaydı bulunamadı.</div>
            ) : (
              <div className="certificate-table-wrapper">
                <table>
                  <thead><tr><th>Sertifika Numarası</th><th>Son Geçerlilik Tarihi</th><th>Kalan Gün</th><th>Durum</th></tr></thead>
                  <tbody>
                    {certificates.map((certificate) => (
                      <tr key={certificate.id}>
                        <td>{certificate.certificateNumber}</td>
                        <td>{new Date(certificate.expiryDate).toLocaleDateString("tr-TR")}</td>
                        <td>{certificate.remainingDays}</td>
                        <td><span className={`certificate-status ${certificate.status}`}>{certificateStatuses[certificate.status] || certificate.status}</span></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default DriversPage;
