import { useEffect, useMemo, useState } from "react";
import "./InvestigationHistoryPage.css";

import { getMyPerformanceHistory } from "../services/inspectorService";

function formatDateTime(date) {
  if (!date) return "-";

  const parsedDate = new Date(date);

  if (Number.isNaN(parsedDate.getTime())) return "-";

  return new Intl.DateTimeFormat("tr-TR", {
    day: "2-digit",
    month: "long",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
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

export default function InvestigationHistoryPage() {
  const [performances, setPerformances] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");

  useEffect(() => {
    let isActive = true;

    async function loadPerformanceHistory() {
      try {
        const data = await getMyPerformanceHistory();

        if (isActive) {
          setPerformances(Array.isArray(data) ? data : []);
        }
      } catch (requestError) {
        if (isActive) {
          setError(
            requestError.message ||
              "Değerlendirme geçmişiniz yüklenirken bir hata oluştu.",
          );
        }
      } finally {
        if (isActive) {
          setLoading(false);
        }
      }
    }

    loadPerformanceHistory();

    return () => {
      isActive = false;
    };
  }, []);

  const filteredPerformances = useMemo(() => {
    const searchValue = search.trim().toLocaleLowerCase("tr-TR");

    if (!searchValue) return performances;

    return performances.filter((performance) =>
      [performance.driverFullName, performance.personnelNumber].some((value) =>
        String(value ?? "")
          .toLocaleLowerCase("tr-TR")
          .includes(searchValue),
      ),
    );
  }, [performances, search]);

  const averageScore = performances.length
    ? performances.reduce(
        (total, performance) => total + Number(performance.score || 0),
        0,
      ) / performances.length
    : 0;

  return (
    <div className="investigation-history-page">
      <div className="page-header">
        <div>
          <h1>Değerlendirme Geçmişim</h1>
          <p>Şoförler için kaydettiğiniz performans değerlendirmelerini görüntüleyebilirsiniz.</p>
        </div>
      </div>

      {!loading && !error && (
        <div className="history-summary-grid">
          <div className="history-summary-card">
            <span>Toplam Değerlendirme</span>
            <strong>{performances.length}</strong>
          </div>
          <div className="history-summary-card">
            <span>Ortalama Puan</span>
            <strong>
              {performances.length
                ? averageScore.toLocaleString("tr-TR", { maximumFractionDigits: 1 })
                : "-"}
            </strong>
          </div>
        </div>
      )}

      {error && <div className="alert-message">{error}</div>}

      {!loading && !error && performances.length > 0 && (
        <div className="history-toolbar">
          <input
            type="search"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Şoför adı veya personel numarası ara..."
            aria-label="Değerlendirme geçmişinde ara"
          />
          <span>{filteredPerformances.length} kayıt gösteriliyor</span>
        </div>
      )}

      <div className="table-card">
        {loading ? (
          <div className="table-state">Değerlendirme geçmişiniz yükleniyor...</div>
        ) : !error && performances.length === 0 ? (
          <div className="table-state">Henüz performans değerlendirmesi yapmadınız.</div>
        ) : !error && filteredPerformances.length === 0 ? (
          <div className="table-state">Aramanızla eşleşen değerlendirme bulunamadı.</div>
        ) : !error ? (
          <table>
            <thead>
              <tr>
                <th>Değerlendirme Tarihi</th>
                <th>Şoför</th>
                <th>Personel Numarası</th>
                <th>Puan</th>
                <th>Performans Yorumu</th>
              </tr>
            </thead>
            <tbody>
              {filteredPerformances.map((performance) => {
                const scoreCategory = getScoreCategory(performance.score);

                return (
                  <tr key={performance.id}>
                    <td>{formatDateTime(performance.evaluationDate)}</td>
                    <td className="history-driver-name">
                      {performance.driverFullName || "-"}
                    </td>
                    <td>{performance.personnelNumber || "-"}</td>
                    <td>
                      <span className={`history-score history-score--${scoreCategory.key}`}>
                        <strong>{performance.score}</strong>
                        {scoreCategory.label}
                      </span>
                    </td>
                    <td className="history-comment">
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
