import { useEffect, useState } from "react";
import iettLogo from "../assets/iett-logo.png";
import { createPublicComplaint, getPublicComplaintTypes } from "../services/publicComplaintService";
import "./PublicComplaintPage.css";

function toLocalDateTimeInput(date = new Date()) {
  const pad = (value) => String(value).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function createInitialForm() {
  return {
    doorNumber: "",
    routeCode: "",
    complaintTypeId: "",
    incidentDateTime: toLocalDateTimeInput(),
    complaintDescription: "",
  };
}

function PublicComplaintPage({ onBackToLogin }) {
  const [form, setForm] = useState(createInitialForm);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);
  const [copyMessage, setCopyMessage] = useState("");
  const [complaintTypes, setComplaintTypes] = useState([]);
  const [typesLoading, setTypesLoading] = useState(true);
  const [typesError, setTypesError] = useState("");

  async function loadComplaintTypes() {
    setTypesLoading(true);
    setTypesError("");

    try {
      const response = await getPublicComplaintTypes();
      setComplaintTypes(Array.isArray(response) ? response : []);
    } catch (requestError) {
      setComplaintTypes([]);
      setTypesError(requestError.message || "Şikâyet türleri yüklenemedi. Lütfen tekrar deneyiniz.");
    } finally {
      setTypesLoading(false);
    }
  }

  useEffect(() => {
    loadComplaintTypes();
  }, []);

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((current) => ({ ...current, [name]: value }));
  }

  function validate() {
    if (typesLoading) return "Şikâyet türleri henüz yükleniyor.";
    if (typesError) return "Şikâyet türleri yüklenemedi. Lütfen yeniden deneyiniz.";
    if (complaintTypes.length === 0) return "Şu anda kullanılabilir şikâyet türü bulunmuyor.";
    if (!form.doorNumber.trim()) return "Kapı numarası zorunludur.";
    if (!form.routeCode.trim()) return "Hat kodu zorunludur.";
    if (!form.complaintTypeId || Number(form.complaintTypeId) < 1) return "Şikâyet türü seçiniz.";
    if (!form.incidentDateTime) return "Olay tarihi ve saati zorunludur.";
    if (new Date(form.incidentDateTime) > new Date()) return "Olay tarihi ve saati gelecekte olamaz.";
    if (!form.complaintDescription.trim()) return "Şikâyet açıklaması boş olamaz.";
    if (form.complaintDescription.trim().length > 2000) return "Şikâyet açıklaması en fazla 2000 karakter olabilir.";
    return "";
  }

  async function handleSubmit(event) {
    event.preventDefault();
    if (loading) return;

    const validationError = validate();
    if (validationError) {
      setError(validationError);
      return;
    }

    setError("");
    setLoading(true);

    try {
      const response = await createPublicComplaint({
        doorNumber: form.doorNumber.trim(),
        routeCode: form.routeCode.trim().toUpperCase(),
        complaintTypeId: Number(form.complaintTypeId),
        incidentDateTime: `${form.incidentDateTime}:00`,
        complaintDescription: form.complaintDescription.trim(),
      });

      setResult(response);
      setForm(createInitialForm());
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
          <div>
            <h1>Şikâyet Oluştur</h1>
            <p>Kapı numarası ve hat kodunu girerek şikâyetinizi kolayca oluşturabilirsiniz.</p>
          </div>
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
            <div className="public-form-grid">
              <label>
                Kapı Numarası
                <input name="doorNumber" type="text" placeholder="A-1232" value={form.doorNumber} onChange={handleChange} required />
                <span>Otobüsün üzerinde yazan kapı numarasını giriniz.</span>
              </label>
              <label>
                Hat Kodu
                <input name="routeCode" type="text" placeholder="500T" value={form.routeCode} onChange={handleChange} required />
                <span>Örneğin: 500T, 15A, 19A</span>
              </label>
              <label>
                Olay Tarihi ve Saati
                <input name="incidentDateTime" type="datetime-local" step="60" max={toLocalDateTimeInput()} value={form.incidentDateTime} onChange={handleChange} required />
              </label>
              <label>
                Şikâyet Türü
                <select name="complaintTypeId" value={form.complaintTypeId} onChange={handleChange} disabled={typesLoading || Boolean(typesError) || complaintTypes.length === 0} required>
                  <option value="">
                    {typesLoading ? "Şikâyet türleri yükleniyor..." : "Şikâyet türü seçiniz"}
                  </option>
                  {complaintTypes.map((type) => <option key={type.id} value={type.id}>{type.name}</option>)}
                </select>
                {!typesLoading && !typesError && complaintTypes.length === 0 && (
                  <span className="public-types-message">Şu anda kullanılabilir şikâyet türü bulunmuyor.</span>
                )}
                {typesError && (
                  <span className="public-types-error">
                    {typesError}
                    <button type="button" onClick={loadComplaintTypes}>Yeniden Dene</button>
                  </span>
                )}
              </label>
              <label className="public-description">
                Şikâyet Açıklaması
                <textarea name="complaintDescription" maxLength="2000" rows="6" value={form.complaintDescription} onChange={handleChange} required />
                <small>{form.complaintDescription.length}/2000 karakter</small>
              </label>
            </div>
            {error && <div className="public-form-error" role="alert">{error}</div>}
            <button className="public-submit-button" type="submit" disabled={loading || typesLoading || Boolean(typesError) || complaintTypes.length === 0}>
              {loading ? "Şikâyet gönderiliyor..." : "Şikâyeti Gönder"}
            </button>
          </form>
        )}

        <button type="button" className="back-login-button" onClick={onBackToLogin}>← Giriş sayfasına dön</button>
      </main>
    </div>
  );
}

export default PublicComplaintPage;
