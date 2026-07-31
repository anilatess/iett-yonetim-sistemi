import { useEffect, useState } from "react";
import "./PerformanceEvaluationPage.css";

import { getDrivers } from "../services/driverService";
import { createDriverPerformance } from "../services/inspectorService";

const INITIAL_FORM = {
  driverId: "",
  score: "",
  performanceComment: "",
};

export default function PerformanceEvaluationPage() {
  const [drivers, setDrivers] = useState([]);
  const [formData, setFormData] = useState(INITIAL_FORM);
  const [loadingDrivers, setLoadingDrivers] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [loadError, setLoadError] = useState("");
  const [formError, setFormError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  useEffect(() => {
    let isActive = true;

    async function loadDrivers() {
      try {
        const data = await getDrivers();

        if (isActive) {
          setDrivers(Array.isArray(data) ? data : []);
        }
      } catch (requestError) {
        if (isActive) {
          setLoadError(
            requestError.message || "Şoför listesi yüklenirken bir hata oluştu.",
          );
        }
      } finally {
        if (isActive) {
          setLoadingDrivers(false);
        }
      }
    }

    loadDrivers();

    return () => {
      isActive = false;
    };
  }, []);

  function handleChange(event) {
    const { name, value } = event.target;

    setFormData((current) => ({
      ...current,
      [name]: value,
    }));
    setFormError("");
    setSuccessMessage("");
  }

  async function handleSubmit(event) {
    event.preventDefault();

    const driverId = Number(formData.driverId);
    const score = Number(formData.score);
    const comment = formData.performanceComment.trim();

    if (!Number.isInteger(driverId) || driverId < 1) {
      setFormError("Lütfen değerlendirilecek şoförü seçin.");
      return;
    }

    if (formData.score === "" || !Number.isInteger(score) || score < 0 || score > 100) {
      setFormError("Puan 0 ile 100 arasında bir tam sayı olmalıdır.");
      return;
    }

    if (comment.length > 500) {
      setFormError("Performans yorumu en fazla 500 karakter olabilir.");
      return;
    }

    try {
      setSubmitting(true);
      setFormError("");
      setSuccessMessage("");

      await createDriverPerformance({
        driverId,
        score,
        performanceComment: comment,
      });

      setFormData(INITIAL_FORM);
      setSuccessMessage("Performans değerlendirmesi başarıyla kaydedildi.");
    } catch (requestError) {
      setFormError(
        requestError.message ||
          "Performans değerlendirmesi kaydedilirken bir hata oluştu.",
      );
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="performance-evaluation-page">
      <div className="page-header">
        <div>
          <h1>Performans Değerlendirme</h1>
          <p>Şoförlerin performans puanı ve değerlendirme notunu kaydedebilirsiniz.</p>
        </div>
      </div>

      {loadError && <div className="alert-message">{loadError}</div>}

      <div className="performance-form-card">
        {loadingDrivers ? (
          <div className="performance-form-state">Şoför listesi yükleniyor...</div>
        ) : drivers.length === 0 && !loadError ? (
          <div className="performance-form-state">Değerlendirilebilecek şoför bulunamadı.</div>
        ) : !loadError ? (
          <form onSubmit={handleSubmit} noValidate>
            {formError && <div className="performance-form-message error">{formError}</div>}
            {successMessage && (
              <div className="performance-form-message success">{successMessage}</div>
            )}

            <div className="performance-form-grid">
              <div className="performance-field performance-field--wide">
                <label htmlFor="driverId">Şoför</label>
                <select
                  id="driverId"
                  name="driverId"
                  value={formData.driverId}
                  onChange={handleChange}
                  disabled={submitting}
                  required
                >
                  <option value="">Şoför seçin</option>
                  {drivers.map((driver) => (
                    <option key={driver.id} value={driver.id}>
                      {driver.fullName} — Personel No: {driver.personnelNumber}
                    </option>
                  ))}
                </select>
              </div>

              <div className="performance-field">
                <label htmlFor="score">Puan</label>
                <input
                  id="score"
                  name="score"
                  type="number"
                  min="0"
                  max="100"
                  step="1"
                  value={formData.score}
                  onChange={handleChange}
                  placeholder="0-100"
                  disabled={submitting}
                  required
                />
              </div>

              <div className="performance-field performance-field--full">
                <div className="performance-label-row">
                  <label htmlFor="performanceComment">Performans Yorumu</label>
                  <span>{formData.performanceComment.length}/500</span>
                </div>
                <textarea
                  id="performanceComment"
                  name="performanceComment"
                  rows="6"
                  maxLength="500"
                  value={formData.performanceComment}
                  onChange={handleChange}
                  placeholder="Şoförün performansıyla ilgili değerlendirmenizi yazın..."
                  disabled={submitting}
                />
              </div>
            </div>

            <div className="performance-form-actions">
              <button type="submit" disabled={submitting}>
                {submitting ? "Kaydediliyor..." : "Değerlendirmeyi Kaydet"}
              </button>
            </div>
          </form>
        ) : null}
      </div>
    </div>
  );
}
