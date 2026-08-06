import { useCallback, useEffect, useMemo, useState } from "react";
import "./InspectorCertificatesPage.css";
import {
  approveInspectorCertificate,
  getInspectorCertificates,
  rejectInspectorCertificate,
} from "../services/inspectorService";

const API_ORIGIN = "http://localhost:5147";
const STATUS_NAMES = { 1: "İnceleniyor", 2: "Onaylandı", 3: "Reddedildi" };

function formatDate(value) {
  if (!value) return "-";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "-" : date.toLocaleDateString("tr-TR");
}

function fileUrl(value) {
  if (!value) return "";
  try { return new URL(value, API_ORIGIN).href; } catch { return ""; }
}

export default function InspectorCertificatesPage() {
  const [certificates, setCertificates] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [typeFilter, setTypeFilter] = useState("");
  const [processingId, setProcessingId] = useState(null);
  const [rejecting, setRejecting] = useState(null);
  const [reason, setReason] = useState("");
  const [rejectError, setRejectError] = useState("");

  const loadCertificates = useCallback(async () => {
    try {
      setLoading(true);
      setError("");
      const data = await getInspectorCertificates();
      setCertificates(Array.isArray(data) ? data : []);
    } catch (requestError) {
      setError(requestError.message || "Sertifikalar yüklenemedi.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadCertificates(); }, [loadCertificates]);

  const types = useMemo(() => [...new Set(certificates.map((item) => item.certificateType).filter(Boolean))].sort((a, b) => a.localeCompare(b, "tr")), [certificates]);
  const filtered = useMemo(() => {
    const text = search.trim().toLocaleLowerCase("tr-TR");
    return certificates.filter((item) => {
      const matchesText = !text || [item.driverFullName, item.personnelNumber].some((value) => String(value || "").toLocaleLowerCase("tr-TR").includes(text));
      return matchesText && (!statusFilter || Number(item.approvalStatus) === Number(statusFilter)) && (!typeFilter || item.certificateType === typeFilter);
    });
  }, [certificates, search, statusFilter, typeFilter]);

  const counts = certificates.reduce((result, item) => ({ ...result, [item.approvalStatus]: (result[item.approvalStatus] || 0) + 1 }), {});

  async function approve(certificate) {
    if (!window.confirm("Bu sertifikayı onaylamak istediğinize emin misiniz?")) return;
    try {
      setProcessingId(certificate.certificateId);
      setSuccess("");
      await approveInspectorCertificate(certificate.certificateId);
      await loadCertificates();
      setSuccess("Sertifika onaylandı.");
    } catch (requestError) {
      setError(requestError.message || "Sertifika onaylanamadı.");
    } finally { setProcessingId(null); }
  }

  async function reject(event) {
    event.preventDefault();
    const trimmed = reason.trim();
    if (!trimmed) { setRejectError("Ret nedeni zorunludur."); return; }
    if (trimmed.length > 500) { setRejectError("Ret nedeni en fazla 500 karakter olabilir."); return; }
    try {
      setProcessingId(rejecting.certificateId);
      setRejectError("");
      await rejectInspectorCertificate(rejecting.certificateId, { rejectionReason: trimmed });
      setRejecting(null);
      setReason("");
      await loadCertificates();
      setSuccess("Sertifika reddedildi.");
    } catch (requestError) {
      setRejectError(requestError.message || "Sertifika reddedilemedi.");
    } finally { setProcessingId(null); }
  }

  return <div className="inspector-certificates-page">
    <header className="page-header"><div><h1>Şoför Sertifikaları</h1><p>Garajınızdaki şoförlerin sertifikalarını inceleyin.</p></div></header>
    <div className="certificate-review-summary"><span>Toplam <strong>{certificates.length}</strong></span><span>İnceleniyor <strong>{counts[1] || 0}</strong></span><span>Onaylandı <strong>{counts[2] || 0}</strong></span><span>Reddedildi <strong>{counts[3] || 0}</strong></span></div>
    <div className="certificate-review-filters"><label>Şoför ara<input type="search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Ad veya personel numarası" /></label><label>Onay durumu<select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}><option value="">Tümü</option>{Object.entries(STATUS_NAMES).map(([value, name]) => <option key={value} value={value}>{name}</option>)}</select></label><label>Sertifika türü<select value={typeFilter} onChange={(event) => setTypeFilter(event.target.value)}><option value="">Tümü</option>{types.map((type) => <option key={type}>{type}</option>)}</select></label><button type="button" onClick={() => { setSearch(""); setStatusFilter(""); setTypeFilter(""); }}>Filtreleri Temizle</button></div>
    {error && <div className="alert-message">{error}</div>}{success && <div className="certificate-review-success">{success}</div>}
    <div className="table-card">{loading ? <div className="table-state">Sertifikalar yükleniyor...</div> : filtered.length === 0 ? <div className="table-state">Filtrelere uygun sertifika bulunamadı.</div> : <table><thead><tr><th>Şoför</th><th>Personel No</th><th>Tür</th><th>Numara</th><th>Düzenlenme</th><th>Son Geçerlilik</th><th>Onay Durumu</th><th>Dosya</th><th>İşlemler</th></tr></thead><tbody>{filtered.map((item) => { const url = fileUrl(item.fileUrl); return <tr key={item.certificateId}><td className="review-driver">{item.driverFullName}</td><td>{item.personnelNumber}</td><td>{item.certificateType || "-"}</td><td>{item.certificateNumber}</td><td>{formatDate(item.issueDate)}</td><td>{formatDate(item.expiryDate)}</td><td><span className={`approval-badge approval-badge--${item.approvalStatus}`}>{item.approvalStatusName || STATUS_NAMES[item.approvalStatus]}</span>{item.rejectionReason && <small className="rejection-note">{item.rejectionReason}</small>}</td><td>{url ? <a href={url} target="_blank" rel="noopener noreferrer">Dosyayı Aç</a> : "-"}</td><td>{Number(item.approvalStatus) === 1 ? <div className="certificate-review-actions"><button type="button" disabled={processingId === item.certificateId} onClick={() => approve(item)}>Onayla</button><button type="button" disabled={processingId === item.certificateId} onClick={() => { setRejecting(item); setReason(""); setRejectError(""); }}>Reddet</button></div> : <span className="reviewed-label">İşlem tamamlandı</span>}</td></tr>; })}</tbody></table>}</div>
    {rejecting && <div className="review-modal-backdrop" onMouseDown={() => !processingId && setRejecting(null)}><form className="review-reject-modal" onSubmit={reject} onMouseDown={(event) => event.stopPropagation()}><h2>Sertifikayı Reddet</h2><p>{rejecting.driverFullName} · {rejecting.certificateNumber}</p>{rejectError && <div className="certificate-form-error">{rejectError}</div>}<label>Ret nedeni<textarea rows="5" maxLength="500" value={reason} onChange={(event) => { setReason(event.target.value); setRejectError(""); }} required /></label><small>{reason.length}/500</small><div><button type="button" disabled={Boolean(processingId)} onClick={() => setRejecting(null)}>Vazgeç</button><button type="submit" disabled={Boolean(processingId)}>{processingId ? "Gönderiliyor..." : "Reddet"}</button></div></form></div>}
  </div>;
}
