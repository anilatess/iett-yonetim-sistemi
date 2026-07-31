import { useEffect, useState } from "react";
import "./DriverCertificatesPage.css";

import { getMyCertificates } from "../services/driverService";

const STATUS_LABELS = {
  Expired: "Süresi Doldu",
  ExpiringSoon: "Süresi Yaklaşıyor",
  Valid: "Geçerli",
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

function formatRemainingDays(remainingDays) {
  const days = Number(remainingDays);

  if (!Number.isFinite(days)) return "-";
  if (days < 0) return `${Math.abs(days)} gün önce doldu`;
  if (days === 0) return "Bugün sona eriyor";

  return `${days} gün kaldı`;
}

export default function DriverCertificatesPage() {
  const [certificates, setCertificates] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let isActive = true;

    async function loadCertificates() {
      try {
        const data = await getMyCertificates();

        if (isActive) {
          setCertificates(Array.isArray(data) ? data : []);
        }
      } catch (requestError) {
        if (isActive) {
          setError(
            requestError.message ||
              "Sertifikalarınız yüklenirken bir hata oluştu.",
          );
        }
      } finally {
        if (isActive) {
          setLoading(false);
        }
      }
    }

    loadCertificates();

    return () => {
      isActive = false;
    };
  }, []);

  return (
    <div className="driver-certificates-page">
      <div className="page-header">
        <div>
          <h1>Sertifikalarım</h1>
          <p>Sertifikalarınızı ve geçerlilik durumlarını görüntüleyebilirsiniz.</p>
        </div>
      </div>

      {error && <div className="alert-message">{error}</div>}

      <div className="table-card">
        {loading ? (
          <div className="table-state">Sertifikalarınız yükleniyor...</div>
        ) : !error && certificates.length === 0 ? (
          <div className="table-state">Kayıtlı sertifikanız bulunamadı.</div>
        ) : !error ? (
          <table>
            <thead>
              <tr>
                <th>Sertifika Numarası</th>
                <th>Son Geçerlilik Tarihi</th>
                <th>Kalan Gün</th>
                <th>Durum</th>
              </tr>
            </thead>
            <tbody>
              {certificates.map((certificate) => (
                <tr key={certificate.id}>
                  <td className="certificate-number">
                    {certificate.certificateNumber || "-"}
                  </td>
                  <td>{formatDate(certificate.expiryDate)}</td>
                  <td>{formatRemainingDays(certificate.remainingDays)}</td>
                  <td>
                    <span
                      className={`certificate-status certificate-status--${certificate.status || "unknown"}`}
                    >
                      {STATUS_LABELS[certificate.status] || certificate.status || "-"}
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
