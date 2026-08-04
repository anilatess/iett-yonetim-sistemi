import { useState } from "react";
import iettLogo from "../assets/iett-logo.png";
import { createPublicComplaint } from "../services/publicComplaintService";
import "./PublicComplaintPage.css";

const initialForm = {
  complaintTypeId: "",
  routeId: "",
  vehicleId: "",
  stopId: "",
  tripId: "",
  complaintDate: "",
  complaintTime: "",
  complaintDescription: "",
};

function PublicComplaintPage({ onBackToLogin }) {
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);
  const [copyMessage, setCopyMessage] = useState("");

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((current) => ({ ...current, [name]: value }));
  }

  function validate() {
    const requiredIds = ["complaintTypeId", "routeId", "vehicleId", "stopId"];
    if (requiredIds.some((name) => !form[name] || Number(form[name]) < 1)) {
      return "Şikâyet türü, hat, araç ve durak için geçerli bir ID giriniz.";
    }
    if (form.tripId && Number(form.tripId) < 1) return "Sefer ID değeri geçersiz.";
    if (!form.complaintDate) return "Şikâyet tarihi zorunludur.";
    if (!form.complaintTime) return "Şikâyet saati zorunludur.";
    if (!form.complaintDescription.trim()) return "Şikâyet açıklaması boş olamaz.";
    if (form.complaintDescription.trim().length > 2000) return "Şikâyet açıklaması en fazla 2000 karakter olabilir.";
    return "";
  }

  async function handleSubmit(event) {
    event.preventDefault();
    const validationError = validate();
    if (validationError) { setError(validationError); return; }

    setError("");
    setLoading(true);
    try {
      const response = await createPublicComplaint({
        complaintTypeId: Number(form.complaintTypeId),
        routeId: Number(form.routeId),
        vehicleId: Number(form.vehicleId),
        stopId: Number(form.stopId),
        tripId: form.tripId ? Number(form.tripId) : null,
        complaintDate: form.complaintDate,
        complaintTime: `${form.complaintTime}:00`,
        complaintDescription: form.complaintDescription.trim(),
      });
      setResult(response);
      setForm(initialForm);
      setCopyMessage("");
    } catch (requestError) {
      setError(requestError.message || "Şikâyet oluşturulamadı. Lütfen tekrar deneyiniz.");
    } finally {
      setLoading(false);
    }
  }

  async function copyTrackingCode() {
    try {
      await navigator.clipboard.writeText(result.trackingCode);
      setCopyMessage("Takip kodu kopyalandı.");
    } catch {
      setCopyMessage("Takip kodu kopyalanamadı. Lütfen elle kopyalayınız.");
    }
  }

  const createdDate = result?.createdDate
    ? new Intl.DateTimeFormat("tr-TR", { dateStyle: "medium", timeStyle: "short" }).format(new Date(result.createdDate))
    : "-";

  return (
    <div className="public-complaint-page">
      <main className="public-complaint-card">
        <header className="public-complaint-header">
          <img src={iettLogo} alt="İETT Logo" />
          <div><h1>Şikâyet Oluştur</h1><p>Yaşadığınız olayı İETT’ye bildirin.</p></div>
        </header>

        {result ? (
          <section className="public-success" aria-live="polite">
            <div className="public-success-icon">✓</div>
            <h2>Şikâyetiniz başarıyla oluşturuldu.</h2>
            <p>Takip kodunuzu saklayınız.</p>
            <div className="tracking-code-box">
              <span>Takip Kodunuz</span>
              <strong>{result.trackingCode}</strong>
              <button type="button" onClick={copyTrackingCode}>Kopyala</button>
            </div>
            {copyMessage && <p className="copy-message">{copyMessage}</p>}
            <dl className="public-result-details">
              <div><dt>Durum</dt><dd>{result.complaintStatusName}</dd></div>
              <div><dt>Oluşturulma</dt><dd>{createdDate}</dd></div>
            </dl>
            <button type="button" className="new-complaint-button" onClick={() => setResult(null)}>Yeni Şikâyet Oluştur</button>
          </section>
        ) : (
          <form className="public-complaint-form" onSubmit={handleSubmit} noValidate>
            <div className="public-info">Referans listeleri anonim erişime açık olmadığı için ilgili kayıtların ID değerlerini giriniz.</div>
            <div className="public-form-grid">
              <label>Şikâyet Türü ID <input name="complaintTypeId" type="number" min="1" value={form.complaintTypeId} onChange={handleChange} required /></label>
              <label>Hat ID <input name="routeId" type="number" min="1" value={form.routeId} onChange={handleChange} required /></label>
              <label>Araç ID <input name="vehicleId" type="number" min="1" value={form.vehicleId} onChange={handleChange} required /></label>
              <label>Durak ID <input name="stopId" type="number" min="1" value={form.stopId} onChange={handleChange} required /></label>
              <label>Sefer ID <span>(opsiyonel)</span><input name="tripId" type="number" min="1" value={form.tripId} onChange={handleChange} /></label>
              <label>Şikâyet Tarihi <input name="complaintDate" type="date" value={form.complaintDate} onChange={handleChange} required /></label>
              <label>Şikâyet Saati <input name="complaintTime" type="time" value={form.complaintTime} onChange={handleChange} required /></label>
              <label className="public-description">Şikâyet Açıklaması <textarea name="complaintDescription" maxLength="2000" rows="6" value={form.complaintDescription} onChange={handleChange} required /><small>{form.complaintDescription.length}/2000 karakter</small></label>
            </div>
            {error && <div className="public-form-error" role="alert">{error}</div>}
            <button className="public-submit-button" type="submit" disabled={loading}>{loading ? "Şikâyet gönderiliyor..." : "Şikâyeti Gönder"}</button>
          </form>
        )}

        <button type="button" className="back-login-button" onClick={onBackToLogin}>← Giriş sayfasına dön</button>
      </main>
    </div>
  );
}

export default PublicComplaintPage;
