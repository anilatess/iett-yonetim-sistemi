import { useCallback, useEffect, useState } from "react";
import "./DriverCertificatesPage.css";

import { getMyCertificates, uploadDriverCertificate } from "../services/driverService";
import { API_BASE_URL } from "../config/apiConfig";

const API_ORIGIN = API_BASE_URL;
const MAX_FILE_SIZE = 5 * 1024 * 1024;
const ALLOWED_TYPES = ["application/pdf", "image/jpeg", "image/png"];
const EMPTY_FORM = { certificateType: "", certificateNumber: "", issueDate: "", expiryDate: "", file: null };
const STATUS_LABELS = { Expired: "Süresi Doldu", ExpiringSoon: "Süresi Yaklaşıyor", Valid: "Geçerli" };
const APPROVAL_LABELS = { 1: "İnceleniyor", 2: "Onaylandı", 3: "Reddedildi" };

function formatDate(value) {
  if (!value) return "-";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "-" : new Intl.DateTimeFormat("tr-TR", { day: "2-digit", month: "long", year: "numeric" }).format(date);
}

function formatRemainingDays(value) {
  const days = Number(value);
  if (!Number.isFinite(days)) return "-";
  if (days < 0) return `${Math.abs(days)} gün önce doldu`;
  if (days === 0) return "Bugün sona eriyor";
  return `${days} gün kaldı`;
}

function getFileUrl(fileUrl) {
  if (!fileUrl) return "";
  try { return new URL(fileUrl, API_ORIGIN).href; } catch { return ""; }
}

export default function DriverCertificatesPage() {
  const [certificates, setCertificates] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const loadCertificates = useCallback(async () => {
    try {
      setLoading(true);
      setError("");
      const data = await getMyCertificates();
      setCertificates(Array.isArray(data) ? data : []);
    } catch (requestError) {
      setError(requestError.message || "Sertifikalarınız yüklenirken bir hata oluştu.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadCertificates(); }, [loadCertificates]);

  function closeModal() {
    if (submitting) return;
    setModalOpen(false);
    setForm(EMPTY_FORM);
    setFormError("");
  }

  function handleChange(event) {
    const { name, value, files } = event.target;
    setForm((current) => ({ ...current, [name]: files ? files[0] || null : value }));
    setFormError("");
  }

  function validateForm() {
    if (!form.certificateType.trim()) return "Sertifika türü zorunludur.";
    if (form.certificateType.trim().length > 100) return "Sertifika türü en fazla 100 karakter olabilir.";
    if (!form.certificateNumber.trim()) return "Sertifika numarası zorunludur.";
    if (form.certificateNumber.trim().length > 100) return "Sertifika numarası en fazla 100 karakter olabilir.";
    if (!form.issueDate || !form.expiryDate) return "Düzenlenme ve son geçerlilik tarihleri zorunludur.";
    if (form.expiryDate < form.issueDate) return "Son geçerlilik tarihi düzenlenme tarihinden önce olamaz.";
    if (!form.file) return "Sertifika dosyası zorunludur.";
    if (!ALLOWED_TYPES.includes(form.file.type) || !/\.(pdf|jpe?g|png)$/i.test(form.file.name)) return "Yalnızca PDF, JPG, JPEG veya PNG dosyası seçebilirsiniz.";
    if (form.file.size > MAX_FILE_SIZE) return "Dosya boyutu en fazla 5 MB olabilir.";
    return "";
  }

  async function handleSubmit(event) {
    event.preventDefault();
    const validationError = validateForm();
    if (validationError) { setFormError(validationError); return; }

    const data = new FormData();
    data.append("CertificateType", form.certificateType.trim());
    data.append("CertificateNumber", form.certificateNumber.trim());
    data.append("IssueDate", form.issueDate);
    data.append("ExpiryDate", form.expiryDate);
    data.append("File", form.file);

    try {
      setSubmitting(true);
      setFormError("");
      await uploadDriverCertificate(data);
      await loadCertificates();
      setModalOpen(false);
      setForm(EMPTY_FORM);
      setSuccess("Sertifikanız incelemeye gönderildi.");
    } catch (requestError) {
      setFormError(requestError.message || "Sertifika yüklenemedi.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="driver-certificates-page">
      <div className="page-header certificates-header">
        <div><h1>Sertifikalarım</h1><p>Sertifikalarınızı yükleyebilir ve geçerlilik durumlarını görüntüleyebilirsiniz.</p></div>
        <button type="button" className="certificate-upload-button" onClick={() => { setSuccess(""); setModalOpen(true); }}>Yeni Sertifika Yükle</button>
      </div>

      {error && <div className="alert-message">{error}</div>}
      {success && <div className="certificate-success">{success}</div>}

      <div className="table-card">
        {loading ? <div className="table-state">Sertifikalarınız yükleniyor...</div> : !error && certificates.length === 0 ? <div className="table-state">Kayıtlı sertifikanız bulunamadı.</div> : !error ? (
          <table><thead><tr><th>Sertifika Türü</th><th>Sertifika Numarası</th><th>Düzenlenme Tarihi</th><th>Son Geçerlilik</th><th>Kalan Gün</th><th>Onay Durumu</th><th>Tarih Durumu</th><th>Dosya</th></tr></thead>
            <tbody>{certificates.map((certificate) => {
              const fileUrl = getFileUrl(certificate.fileUrl);
              return <tr key={certificate.id}><td>{certificate.certificateType || "-"}</td><td className="certificate-number">{certificate.certificateNumber || "-"}</td><td>{formatDate(certificate.issueDate)}</td><td>{formatDate(certificate.expiryDate)}</td><td>{formatRemainingDays(certificate.remainingDays)}</td><td><span className={`certificate-approval certificate-approval--${certificate.approvalStatus}`}>{certificate.approvalStatusName || APPROVAL_LABELS[certificate.approvalStatus] || "-"}</span>{Number(certificate.approvalStatus) === 3 && certificate.rejectionReason && <small className="driver-rejection-reason">{certificate.rejectionReason}</small>}</td><td>{Number(certificate.approvalStatus) === 2 ? <span className={`certificate-status certificate-status--${certificate.status || "unknown"}`}>{STATUS_LABELS[certificate.status] || certificate.status || "-"}</span> : <span className="inactive-certificate-label">Aktif değil</span>}</td><td>{fileUrl ? <a className="certificate-file-link" href={fileUrl} target="_blank" rel="noopener noreferrer">Dosyayı Aç</a> : "-"}</td></tr>;
            })}</tbody></table>
        ) : null}
      </div>

      {modalOpen && <div className="certificate-modal-backdrop" onMouseDown={closeModal}><div className="certificate-upload-modal" role="dialog" aria-modal="true" aria-labelledby="certificate-modal-title" onMouseDown={(event) => event.stopPropagation()}>
        <div className="certificate-modal-header"><div><h2 id="certificate-modal-title">Yeni Sertifika Yükle</h2><p>PDF, JPG, JPEG veya PNG · En fazla 5 MB</p></div><button type="button" onClick={closeModal} disabled={submitting} aria-label="Kapat">×</button></div>
        {formError && <div className="certificate-form-error">{formError}</div>}
        <form onSubmit={handleSubmit}><div className="certificate-form-grid">
          <label>Sertifika türü<input name="certificateType" value={form.certificateType} onChange={handleChange} maxLength="100" disabled={submitting} required /></label>
          <label>Sertifika numarası<input name="certificateNumber" value={form.certificateNumber} onChange={handleChange} maxLength="100" disabled={submitting} required /></label>
          <label>Düzenlenme tarihi<input name="issueDate" type="date" value={form.issueDate} onChange={handleChange} disabled={submitting} required /></label>
          <label>Son geçerlilik tarihi<input name="expiryDate" type="date" value={form.expiryDate} min={form.issueDate || undefined} onChange={handleChange} disabled={submitting} required /></label>
          <label className="certificate-file-field">Sertifika dosyası<input name="file" type="file" accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png" onChange={handleChange} disabled={submitting} required /><span>{form.file ? form.file.name : "Dosya seçilmedi"}</span></label>
        </div><div className="certificate-form-actions"><button type="button" onClick={closeModal} disabled={submitting}>Vazgeç</button><button type="submit" disabled={submitting}>{submitting ? "Yükleniyor..." : "Sertifikayı Yükle"}</button></div></form>
      </div></div>}
    </div>
  );
}
