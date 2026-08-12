import { useCallback, useEffect, useState } from "react";
import { getMyComplaints, submitComplaintExplanation } from "../services/driverService";
import "./DriverComplaintsPage.css";

const PROCESS = { awaitingDriver: 2, awaitingFinal: 3, completed: 4 };
function formatDateTime(value) {
  if (!value) return "-"; const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "-" : new Intl.DateTimeFormat("tr-TR", { dateStyle: "medium", timeStyle: "short" }).format(date);
}
function processLabel(status) {
  if (status === PROCESS.awaitingDriver) return "Şoför açıklaması bekleniyor";
  if (status === PROCESS.awaitingFinal) return "Denetimci kararı bekleniyor";
  if (status === PROCESS.completed) return "Tamamlandı";
  return "Bilinmiyor";
}
export default function DriverComplaintsPage({ refreshKey = 0 }) {
  const [complaints, setComplaints] = useState([]); const [loading, setLoading] = useState(true);
  const [error, setError] = useState(""); const [success, setSuccess] = useState("");
  const [selected, setSelected] = useState(null); const [explanation, setExplanation] = useState("");
  const [submitting, setSubmitting] = useState(false); const [actionError, setActionError] = useState("");
  const load = useCallback(async () => {
    setLoading(true); setError("");
    try { const data = await getMyComplaints(); setComplaints(Array.isArray(data) ? data : []); }
    catch (requestError) { setError(requestError.message || "Şikâyetleriniz yüklenemedi."); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { load(); }, [load, refreshKey]);
  function close() { if (!submitting) { setSelected(null); setExplanation(""); setActionError(""); } }
  async function submit(event) {
    event.preventDefault(); if (submitting || !selected) return;
    const value = explanation.trim();
    if (!value) { setActionError("Açıklama zorunludur."); return; }
    if (value.length > 1000) { setActionError("Açıklama en fazla 1000 karakter olabilir."); return; }
    setSubmitting(true); setActionError("");
    try { await submitComplaintExplanation(selected.investigationId, value); await load(); close(); setSelected(null); setSuccess("Açıklamanız denetimciye gönderildi."); }
    catch (requestError) { setActionError(requestError.message || "Açıklama gönderilemedi."); }
    finally { setSubmitting(false); }
  }
  return <div className="driver-complaints-page"><div className="page-header"><div><h1>Şikâyetlerim</h1><p>Size iletilen şikâyetleri görüntüleyip açıklamanızı gönderin.</p></div></div>
    {error && <div className="alert-message">{error}</div>}{success && <div className="driver-action-success">{success}</div>}
    <div className="table-card driver-complaints-card">{loading ? <div className="table-state">Şikâyetleriniz yükleniyor...</div> : complaints.length === 0 ? <div className="table-state">Size ait şikâyet bulunamadı.</div> :
      <div className="driver-complaints-table-wrapper"><table><thead><tr><th>Takip Kodu</th><th>Şikâyet Türü</th><th>Açıklama</th><th>Olay Tarihi</th><th>Hat</th><th>Araç</th><th>Durum</th><th>İşlem</th></tr></thead><tbody>
        {complaints.map((item) => <tr key={item.id}><td><span className="complaint-tracking-code">{item.trackingCode}</span></td><td>{item.complaintTypeName}</td><td><div className="driver-complaint-description">{item.complaintDescription}</div></td><td>{formatDateTime(`${String(item.complaintDate).slice(0, 10)}T${item.complaintTime}`)}</td><td>{item.routeCode || "Belirtilmedi"}</td><td>{item.vehicleDoorNumber || "-"}</td><td><span className={`complaint-status process-${item.processStatus}`}>{processLabel(item.processStatus)}</span></td><td>
          {item.processStatus === PROCESS.awaitingDriver ? <button className="driver-explanation-button" type="button" onClick={() => { setSelected(item); setExplanation(""); setActionError(""); }}>Açıklama Yaz</button> : "-"}
        </td></tr>)}
      </tbody></table></div>}</div>
    {selected && <div className="driver-modal-backdrop" onMouseDown={close}><div className="driver-modal" role="dialog" aria-modal="true" onMouseDown={(e) => e.stopPropagation()}>
      <header><div><h2>Şoför Açıklaması</h2><p>{selected.trackingCode}</p></div><button type="button" onClick={close} disabled={submitting}>×</button></header>
      <dl><div><dt>Şikâyet Türü</dt><dd>{selected.complaintTypeName}</dd></div><div><dt>Hat</dt><dd>{selected.routeCode || "Belirtilmedi"}</dd></div><div className="wide"><dt>Şikâyet Açıklaması</dt><dd>{selected.complaintDescription}</dd></div></dl>
      <form onSubmit={submit}><label htmlFor="driver-explanation">Açıklamanız</label><textarea id="driver-explanation" rows="6" maxLength="1000" value={explanation} disabled={submitting} onChange={(e) => { setExplanation(e.target.value); setActionError(""); }} required /><small>{explanation.length}/1000 karakter</small>{actionError && <div className="decision-error">{actionError}</div>}<button className="driver-explanation-submit" disabled={submitting}>{submitting ? "Gönderiliyor..." : "Denetimciye Gönder"}</button></form>
    </div></div>}
  </div>;
}
