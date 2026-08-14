import { useEffect, useState } from "react";
import iettLogo from "../assets/iett-logo.png";
import {
  createPublicComplaint,
  getPublicComplaintTypes,
  trackPublicComplaint,
} from "../services/publicComplaintService";
import "./PublicComplaintPage.css";

function toLocalDateTimeInput(date = new Date()) {
  const pad = (value) => String(value).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

const initialForm = () => ({
  doorNumber: "", routeCode: "", complaintTypeId: "",
  incidentDateTime: toLocalDateTimeInput(), complaintDescription: "",
});

export default function PublicComplaintPage({ onBackToLogin, initialView = "create" }) {
  const [view, setView] = useState(initialView);
  const [form, setForm] = useState(initialForm);
  const [complaintTypes, setComplaintTypes] = useState([]);
  const [typesLoading, setTypesLoading] = useState(true);
  const [typesError, setTypesError] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [result, setResult] = useState(null);
  const [trackingCode, setTrackingCode] = useState("");
  const [trackingResult, setTrackingResult] = useState(null);
  const [copyMessage, setCopyMessage] = useState("");

  async function loadTypes() {
    setTypesLoading(true); setTypesError("");
    try { setComplaintTypes(await getPublicComplaintTypes()); }
    catch (requestError) { setTypesError(requestError.message || "Şikâyet türleri yüklenemedi."); }
    finally { setTypesLoading(false); }
  }

  useEffect(() => { loadTypes(); }, []);

  function validate() {
    if (typesLoading || typesError || complaintTypes.length === 0) return "Şikâyet türleri kullanılamıyor.";
    if (!form.doorNumber.trim()) return "Kapı numarası zorunludur.";
    if (!form.complaintTypeId) return "Şikâyet türü seçiniz.";
    if (!form.incidentDateTime) return "Olay tarihi ve saati zorunludur.";
    if (new Date(form.incidentDateTime) > new Date()) return "Olay tarihi ve saati gelecekte olamaz.";
    if (!form.complaintDescription.trim()) return "Şikâyet açıklaması boş olamaz.";
    return "";
  }

  async function submitComplaint(event) {
    event.preventDefault(); if (loading) return;
    const validationError = validate();
    if (validationError) { setError(validationError); return; }
    setLoading(true); setError("");
    try {
      const routeCode = form.routeCode.trim().toUpperCase();
      setResult(await createPublicComplaint({
        doorNumber: form.doorNumber.trim(),
        routeCode: routeCode || null,
        complaintTypeId: Number(form.complaintTypeId),
        incidentDateTime: `${form.incidentDateTime}:00`,
        complaintDescription: form.complaintDescription.trim(),
      }));
      setForm(initialForm());
    } catch (requestError) { setError(requestError.message || "Şikâyet oluşturulamadı."); }
    finally { setLoading(false); }
  }

  async function submitTracking(event) {
    event.preventDefault(); if (loading) return;
    const code = trackingCode.trim();
    if (!code) { setError("Takip numarası zorunludur."); return; }
    setLoading(true); setError(""); setTrackingResult(null);
    try { setTrackingResult(await trackPublicComplaint(code)); }
    catch (requestError) { setError(requestError.message || "Şikâyet sorgulanamadı."); }
    finally { setLoading(false); }
  }

  function switchView(nextView) {
    setView(nextView); setError(""); setTrackingResult(null); setResult(null);
  }

  async function copyCode() {
    try { await navigator.clipboard.writeText(result.trackingCode); setCopyMessage("Takip kodu kopyalandı."); }
    catch { setCopyMessage("Takip kodu kopyalanamadı."); }
  }

  return <div className="public-complaint-page"><main className="public-complaint-card">
    <header className="public-complaint-header"><img src={iettLogo} alt="İETT Logo" /><div>
      <h1>{view === "create" ? "Şikâyet Oluştur" : "Şikâyet Takibi"}</h1>
      <p>{view === "create" ? "Hat kodunu biliyorsanız girerek şikâyetinizi oluşturabilirsiniz." : "Takip numaranızla şikâyetinizin sonucunu sorgulayın."}</p>
    </div></header>
    <div className="public-view-tabs">
      <button type="button" className={view === "create" ? "active" : ""} onClick={() => switchView("create")}>Şikâyet Oluştur</button>
      <button type="button" className={view === "track" ? "active" : ""} onClick={() => switchView("track")}>Şikâyet Takip</button>
    </div>
    {view === "create" && (result ? <section className="public-success" aria-live="polite">
      <div className="public-success-icon">✓</div><h2>Şikâyetiniz başarıyla oluşturuldu.</h2><p>Takip kodunuzu saklayınız.</p>
      <div className="tracking-code-box"><span>Takip Kodunuz</span><strong>{result.trackingCode}</strong><button type="button" onClick={copyCode}>Kopyala</button></div>
      {copyMessage && <p className="copy-message">{copyMessage}</p>}
      <button type="button" className="new-complaint-button" onClick={() => setResult(null)}>Yeni Şikâyet Oluştur</button>
    </section> : <form className="public-complaint-form" onSubmit={submitComplaint} noValidate><div className="public-form-grid">
      <label>Kapı Numarası<input name="doorNumber" value={form.doorNumber} onChange={(e) => setForm({ ...form, doorNumber: e.target.value })} required /></label>
      <label>Hat Kodu (İsteğe bağlı)<input name="routeCode" value={form.routeCode} onChange={(e) => setForm({ ...form, routeCode: e.target.value })} /><span>Birden fazla sefer eşleşirse hat kodu istenebilir.</span></label>
      <label>Olay Tarihi ve Saati<input type="datetime-local" step="60" max={toLocalDateTimeInput()} value={form.incidentDateTime} onChange={(e) => setForm({ ...form, incidentDateTime: e.target.value })} required /></label>
      <label>Şikâyet Türü<select value={form.complaintTypeId} onChange={(e) => setForm({ ...form, complaintTypeId: e.target.value })} disabled={typesLoading || Boolean(typesError)} required>
        <option value="">{typesLoading ? "Yükleniyor..." : "Şikâyet türü seçiniz"}</option>{complaintTypes.map((type) => <option key={type.id} value={type.id}>{type.name}</option>)}</select>
        {typesError && <span className="public-types-error">{typesError}<button type="button" onClick={loadTypes}>Yeniden Dene</button></span>}</label>
      <label className="public-description">Şikâyet Açıklaması<textarea maxLength="2000" rows="6" value={form.complaintDescription} onChange={(e) => setForm({ ...form, complaintDescription: e.target.value })} required /><small>{form.complaintDescription.length}/2000 karakter</small></label>
    </div>{error && <div className="public-form-error" role="alert">{error}</div>}
    <button className="public-submit-button" disabled={loading || typesLoading || Boolean(typesError)}>{loading ? "Gönderiliyor..." : "Şikâyeti Gönder"}</button></form>)}
    {view === "track" && <section className="public-tracking"><form onSubmit={submitTracking}>
      <label>Takip Numarası<input value={trackingCode} onChange={(e) => setTrackingCode(e.target.value)} placeholder="SIK-2026-000001" required /></label>
      <button className="public-submit-button" disabled={loading}>{loading ? "Sorgulanıyor..." : "Sorgula"}</button>
    </form>{error && <div className="public-form-error" role="alert">{error}</div>}
    {trackingResult && <div className="public-tracking-result" aria-live="polite"><strong>{trackingResult.trackingCode}</strong><p>{trackingResult.status}</p>
      {trackingResult.status === "Tamamlandı" && trackingResult.finalDecision && <section><h2>Nihai Karar</h2><p>{trackingResult.finalDecision}</p></section>}</div>}</section>}
    <button type="button" className="back-login-button" onClick={onBackToLogin}>← Giriş sayfasına dön</button>
  </main></div>;
}
