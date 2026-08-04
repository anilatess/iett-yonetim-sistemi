import { useEffect, useState } from "react";
import "./DriverComplaintsPage.css";

import { getMyComplaints } from "../services/driverService";

function formatDate(value) {
  if (!value) return "-";

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) return "-";

  return new Intl.DateTimeFormat("tr-TR", {
    day: "2-digit",
    month: "long",
    year: "numeric",
  }).format(date);
}

function formatDateTime(value) {
  if (!value) return "-";

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) return "-";

  return new Intl.DateTimeFormat("tr-TR", {
    day: "2-digit",
    month: "long",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

function formatTime(value) {
  return value ? String(value).slice(0, 5) : "-";
}

export default function DriverComplaintsPage() {
  const [complaints, setComplaints] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let isActive = true;

    async function loadComplaints() {
      try {
        const data = await getMyComplaints();

        if (isActive) {
          setComplaints(Array.isArray(data) ? data : []);
        }
      } catch (requestError) {
        if (isActive) {
          setError(
            requestError.message ||
              "Şikâyetleriniz yüklenirken bir hata oluştu.",
          );
        }
      } finally {
        if (isActive) {
          setLoading(false);
        }
      }
    }

    loadComplaints();

    return () => {
      isActive = false;
    };
  }, []);

  return (
    <div className="driver-complaints-page">
      <div className="page-header">
        <div>
          <h1>Şikâyetlerim</h1>
          <p>Seferlerinizle ilişkilendirilen şikâyetleri görüntüleyebilirsiniz.</p>
        </div>
      </div>

      {error && <div className="alert-message">{error}</div>}

      <div className="table-card driver-complaints-card">
        {loading ? (
          <div className="table-state">Şikâyetleriniz yükleniyor...</div>
        ) : !error && complaints.length === 0 ? (
          <div className="table-state">Size ait şikâyet bulunamadı.</div>
        ) : !error ? (
          <div className="driver-complaints-table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Takip Kodu</th>
                  <th>Şikâyet Türü</th>
                  <th>Açıklama</th>
                  <th>Şikâyet Tarihi</th>
                  <th>Durum</th>
                  <th>Hat</th>
                  <th>Araç</th>
                  <th>Durak</th>
                  <th>Sefer Tarihi</th>
                  <th>Kalkış</th>
                  <th>Varış</th>
                  <th>İnceleme Sonucu</th>
                  <th>Kapanış Tarihi</th>
                </tr>
              </thead>

              <tbody>
                {complaints.map((complaint) => (
                  <tr key={complaint.id}>
                    <td>
                      <span className="complaint-tracking-code">
                        {complaint.trackingCode || "-"}
                      </span>
                    </td>
                    <td>{complaint.complaintTypeName || "-"}</td>
                    <td>
                      <div className="driver-complaint-description">
                        {complaint.complaintDescription || "-"}
                      </div>
                    </td>
                    <td>{formatDateTime(complaint.complaintCreatedDate)}</td>
                    <td>
                      <span className={`complaint-status complaint-status--${complaint.complaintStatus}`}>
                        {complaint.complaintStatusName || "-"}
                      </span>
                    </td>
                    <td>
                      <span className="complaint-reference-code">
                        {complaint.routeCode || "-"}
                      </span>
                      <span className="complaint-reference-name">
                        {complaint.routeName || "-"}
                      </span>
                    </td>
                    <td>{complaint.vehicleDoorNumber || "-"}</td>
                    <td>
                      <span className="complaint-reference-code">
                        {complaint.stopCode || "-"}
                      </span>
                      <span className="complaint-reference-name">
                        {complaint.stopName || "-"}
                      </span>
                    </td>
                    <td>{formatDate(complaint.tripDate)}</td>
                    <td>{formatTime(complaint.depertureTime)}</td>
                    <td>{formatTime(complaint.arrivalTime)}</td>
                    <td>
                      <div className="investigation-result">
                        {complaint.investigationResult ||
                          "Henüz sonuçlandırılmadı"}
                      </div>
                    </td>
                    <td>{formatDateTime(complaint.investigationClosedDate)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : null}
      </div>
    </div>
  );
}
