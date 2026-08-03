import { useEffect, useState } from "react";
import "./InspectorTasksPage.css";

import {
  completeInvestigation,
  getMyInvestigations,
} from "../services/inspectorService";

function formatDate(date) {
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

function formatTripDate(date) {
  if (!date) return "-";

  const parsedDate = new Date(date);
  if (Number.isNaN(parsedDate.getTime())) return "-";

  return new Intl.DateTimeFormat("tr-TR", {
    day: "2-digit",
    month: "long",
    year: "numeric",
  }).format(parsedDate);
}

function formatTime(time) {
  if (!time || typeof time !== "string") return "-";
  return time.slice(0, 5);
}

export default function InspectorTasksPage() {
  const [investigations, setInvestigations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedInvestigation, setSelectedInvestigation] = useState(null);
  const [investigationResult, setInvestigationResult] = useState("");
  const [formError, setFormError] = useState("");
  const [saving, setSaving] = useState(false);

  async function loadInvestigations() {
    setLoading(true);
    setError("");

    try {
      const data = await getMyInvestigations();
      setInvestigations(Array.isArray(data) ? data : []);
    } catch (requestError) {
      setError(requestError.message || "Görevleriniz yüklenirken bir hata oluştu.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadInvestigations();
  }, []);

  function openCompletionForm(investigation) {
    setSelectedInvestigation(investigation);
    setInvestigationResult("");
    setFormError("");
  }

  function closeCompletionForm() {
    if (saving) return;
    setSelectedInvestigation(null);
    setInvestigationResult("");
    setFormError("");
  }

  async function handleComplete(event) {
    event.preventDefault();
    const result = investigationResult.trim();

    if (!result) {
      setFormError("İnceleme sonucu zorunludur.");
      return;
    }

    if (result.length > 1000) {
      setFormError("İnceleme sonucu en fazla 1000 karakter olabilir.");
      return;
    }

    setSaving(true);
    setFormError("");

    try {
      await completeInvestigation(selectedInvestigation.id, result);
      setSelectedInvestigation(null);
      setInvestigationResult("");
      await loadInvestigations();
    } catch (requestError) {
      setFormError(requestError.message || "İnceleme sonuçlandırılırken bir hata oluştu.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="inspector-tasks-page">
      <div className="page-header">
        <div>
          <h1>Görevlerim</h1>
          <p>Size atanan şikâyet incelemelerini ve güncel durumlarını görüntüleyebilirsiniz.</p>
        </div>
      </div>

      {error && <div className="alert-message">{error}</div>}

      {loading ? (
        <div className="table-card task-state">Görevleriniz yükleniyor...</div>
      ) : !error && investigations.length === 0 ? (
        <div className="table-card task-state">Henüz size atanmış bir inceleme görevi bulunmuyor.</div>
      ) : !error ? (
        <div className="inspector-task-list">
          {investigations.map((investigation) => {
            const completed = Boolean(investigation.closedDate);

            return (
              <article className="inspector-task-card" key={investigation.id}>
                <div className="task-card-header">
                  <div>
                    <span className="task-tracking-code">{investigation.trackingCode || "Takip kodu yok"}</span>
                    <h2>{investigation.investigationTitle || "Başlıksız inceleme"}</h2>
                  </div>
                  <span className={`task-status task-status--${completed ? "completed" : "active"}`}>
                    {investigation.status}
                  </span>
                </div>

                <div className="task-meta">
                  <span><strong>Şikâyet türü:</strong> {investigation.complaintTypeName || "-"}</span>
                  <span><strong>Atanma tarihi:</strong> {formatDate(investigation.investigationCreatedDate)}</span>
                  <span><strong>Şikâyet tarihi:</strong> {formatDate(investigation.complaintCreatedDate)}</span>
                  <span><strong>Kapanış tarihi:</strong> {formatDate(investigation.closedDate)}</span>
                </div>

                <section className="task-trip-info" aria-label="Sefer ve araç bilgileri">
                  <h3>Sefer ve araç bilgileri</h3>
                  <div className="task-trip-grid">
                    <div>
                      <span>Şoför</span>
                      <strong>{investigation.driverFullName || "Bilgi bulunamadı"}</strong>
                      <small>Personel No: {investigation.personnelNumber || "-"}</small>
                    </div>
                    <div>
                      <span>Araç</span>
                      <strong>{investigation.vehicleDoorNumber || "Bilgi bulunamadı"}</strong>
                      <small>Kapı numarası</small>
                    </div>
                    <div>
                      <span>Hat</span>
                      <strong>{investigation.routeCode || "-"}</strong>
                      <small>{investigation.routeName || "Bilgi bulunamadı"}</small>
                    </div>
                    <div>
                      <span>Durak</span>
                      <strong>{investigation.stopCode || "-"}</strong>
                      <small>{investigation.stopName || "Bilgi bulunamadı"}</small>
                    </div>
                    <div>
                      <span>Sefer tarihi</span>
                      <strong>{formatTripDate(investigation.tripDate)}</strong>
                      <small>Sefer No: {investigation.tripId || "-"}</small>
                    </div>
                    <div>
                      <span>Hareket / Varış</span>
                      <strong>{formatTime(investigation.depertureTime)} / {formatTime(investigation.arrivalTime)}</strong>
                      <small>Saat bilgisi</small>
                    </div>
                  </div>
                </section>

                <div className="task-detail">
                  <h3>Şikâyet açıklaması</h3>
                  <p>{investigation.complaintDescription || "Açıklama bulunmuyor."}</p>
                </div>

                {investigation.investigationDescription && (
                  <div className="task-detail">
                    <h3>İnceleme bilgisi</h3>
                    <p>{investigation.investigationDescription}</p>
                  </div>
                )}

                {completed && investigation.investigationResult && (
                  <div className="task-detail task-detail--result">
                    <h3>İnceleme sonucu</h3>
                    <p>{investigation.investigationResult}</p>
                  </div>
                )}

                {!completed && (
                  <div className="task-actions">
                    <button type="button" onClick={() => openCompletionForm(investigation)}>
                      İncelemeyi Sonuçlandır
                    </button>
                  </div>
                )}
              </article>
            );
          })}
        </div>
      ) : null}

      {selectedInvestigation && (
        <div className="task-modal-overlay" role="presentation" onMouseDown={closeCompletionForm}>
          <div className="task-modal" role="dialog" aria-modal="true" aria-labelledby="completion-title" onMouseDown={(event) => event.stopPropagation()}>
            <div className="task-modal-header">
              <div>
                <span>{selectedInvestigation.trackingCode}</span>
                <h2 id="completion-title">İncelemeyi Sonuçlandır</h2>
              </div>
              <button type="button" className="task-modal-close" onClick={closeCompletionForm} disabled={saving} aria-label="Kapat">×</button>
            </div>

            <form onSubmit={handleComplete}>
              <label htmlFor="investigation-result">İnceleme sonucu</label>
              <textarea
                id="investigation-result"
                value={investigationResult}
                onChange={(event) => setInvestigationResult(event.target.value)}
                maxLength={1000}
                rows={7}
                required
                disabled={saving}
                placeholder="İnceleme sonucunu ayrıntılı olarak yazın..."
              />
              <div className="task-result-counter">{investigationResult.length}/1000</div>
              {formError && <div className="task-form-error">{formError}</div>}
              <div className="task-modal-actions">
                <button type="button" className="task-secondary-button" onClick={closeCompletionForm} disabled={saving}>Vazgeç</button>
                <button type="submit" className="task-complete-button" disabled={saving}>
                  {saving ? "Kaydediliyor..." : "Sonuçlandır"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
