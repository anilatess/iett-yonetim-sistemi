import { useEffect, useState } from "react";
import "./DriverPerformancePage.css";

import { getMyPerformances } from "../services/driverService";

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

function getScoreCategory(score) {
  if (score >= 85) {
    return { key: "successful", label: "Başarılı" };
  }

  if (score >= 70) {
    return { key: "improvable", label: "Geliştirilebilir" };
  }

  return { key: "low", label: "Düşük" };
}

export default function DriverPerformancePage({ refreshKey = 0 }) {
  const [performances, setPerformances] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let isActive = true;

    async function loadPerformances() {
      setLoading(true);
      setError("");

      try {
        const data = await getMyPerformances();

        if (isActive) {
          setPerformances(Array.isArray(data) ? data : []);
        }
      } catch (requestError) {
        if (isActive) {
          setError(
            requestError.message ||
              "Performans kayıtlarınız yüklenirken bir hata oluştu.",
          );
        }
      } finally {
        if (isActive) {
          setLoading(false);
        }
      }
    }

    loadPerformances();

    return () => {
      isActive = false;
    };
  }, [refreshKey]);

  const averageScore = performances.length
    ? performances.reduce((total, performance) => total + performance.score, 0) /
      performances.length
    : 0;

  return (
    <div className="driver-performance-page">
      <div className="page-header">
        <div>
          <h1>Performansım</h1>
          <p>Performans değerlendirmelerinizi ve puanlarınızı görüntüleyebilirsiniz.</p>
        </div>
      </div>

      {!loading && !error && performances.length > 0 && (
        <div className="performance-summary">
          <div>
            <span className="performance-summary-label">Ortalama Puan</span>
            <strong>{averageScore.toLocaleString("tr-TR", { maximumFractionDigits: 1 })}</strong>
          </div>
          <span className="performance-summary-count">
            {performances.length} değerlendirme
          </span>
        </div>
      )}

      {error && <div className="alert-message">{error}</div>}

      <div className="table-card">
        {loading ? (
          <div className="table-state">Performans kayıtlarınız yükleniyor...</div>
        ) : !error && performances.length === 0 ? (
          <div className="table-state">Performans değerlendirmeniz bulunamadı.</div>
        ) : !error ? (
          <table>
            <thead>
              <tr>
                <th>Değerlendirme Tarihi</th>
                <th>Puan</th>
                <th>Değerlendiren Denetimci</th>
                <th>Performans Yorumu</th>
              </tr>
            </thead>
            <tbody>
              {performances.map((performance) => {
                const scoreCategory = getScoreCategory(performance.score);

                return (
                  <tr key={performance.id}>
                    <td>{formatDate(performance.evaluationDate)}</td>
                    <td>
                      <span className={`score-badge score-badge--${scoreCategory.key}`}>
                        <strong>{performance.score}</strong>
                        {scoreCategory.label}
                      </span>
                    </td>
                    <td>{performance.inspectorFullName || "-"}</td>
                    <td className="performance-comment">
                      {performance.performanceComment || "Yorum bulunmuyor."}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        ) : null}
      </div>
    </div>
  );
}
