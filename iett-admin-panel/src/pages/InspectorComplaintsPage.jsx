import { useCallback, useEffect, useMemo, useState } from "react";
import { decideInvestigation, finalizeInvestigation, getMyInvestigations } from "../services/inspectorService";
import "./InspectorComplaintsPage.css";

const PROCESS = { review: 1, driver: 2, final: 3, completed: 4 };
const STATUS = {
  1: "Denetimci incelemesi bekleniyor",
  2: "Şoför açıklaması bekleniyor",
  3: "Nihai karar bekleniyor",
  4: "Tamamlandı",
};
function formatDateTime(value) {
  if (!value) return "-"; const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "-" : new Intl.DateTimeFormat("tr-TR", { dateStyle: "medium", timeStyle: "short" }).format(date);
}
function Detail({ label, value, wide = false }) {
  return <div className={`inspector-complaint-detail ${wide ? "wide" : ""}`}><span>{label}</span><strong>{value || "-"}</strong></div>;
}

export default function InspectorComplaintsPage({ refreshKey = 0 }) {
  const [items, setItems] = useState([]); const [selected, setSelected] = useState(null);
  const [result, setResult] = useState(""); const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false); const [error, setError] = useState("");
  const [actionError, setActionError] = useState(""); const [success, setSuccess] = useState("");
  const [search, setSearch] = useState("");
  const load = useCallback(async () => {
    setLoading(true); setError("");
    try { const data = await getMyInvestigations(); setItems(Array.isArray(data) ? data : []); }
    catch (requestError) { setError(requestError.message || "Şikâyet incelemeleri yüklenemedi."); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => { load(); }, [load, refreshKey]);
  const filtered = useMemo(() => {
    const term = search.trim().toLocaleLowerCase("tr-TR");
    return !term ? items : items.filter((item) => [item.trackingCode, item.complaintTypeName, item.driverFullName, item.routeCode]
      .some((value) => String(value ?? "").toLocaleLowerCase("tr-TR").includes(term)));
  }, [items, search]);
  function open(item) { setSelected(item); setResult(""); setActionError(""); setSuccess(""); }
  function close() { if (!submitting) { setSelected(null); setResult(""); setActionError(""); } }
  function validateResult() {
    const trimmed = result.trim();
    if (!trimmed) { setActionError("Nihai karar zorunludur."); return null; }
    if (trimmed.length > 1000) { setActionError("Nihai karar en fazla 1000 karakter olabilir."); return null; }
    return trimmed;
  }
  async function runAction(action) {
    if (submitting || !selected) return;
    let payload = "Şikâyet şoför açıklaması için iletildi.";
    if (action !== "approve") { payload = validateResult(); if (!payload) return; }
    setSubmitting(true); setActionError("");
    try {
      if (action === "approve") await decideInvestigation(selected.id, "Approved", payload);
      else if (action === "reject") await decideInvestigation(selected.id, "Rejected", payload);
      else await finalizeInvestigation(selected.id, payload);
      await load(); setSelected(null); setResult("");
      setSuccess(action === "approve" ? "Şikâyet şoföre iletildi." : "Nihai karar kaydedildi.");
    } catch (requestError) { setActionError(requestError.message || "İşlem tamamlanamadı."); }
    finally { setSubmitting(false); }
  }
  return <div className="inspector-complaints-page">
    <div className="page-header"><div><h1>Şikâyet İncelemeleri</h1><p>Size atanan şikâyetlerin iç süreçlerini yönetin.</p></div><span className="investigation-count">{filtered.length} kayıt</span></div>
    <div className="investigation-toolbar"><input type="search" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Takip kodu, tür, şoför veya hat ara..." /></div>
    {error && <div className="alert-message">{error}</div>}{success && <div className="decision-success">{success}</div>}
    <div className="table-card inspector-investigation-card">{loading ? <div className="table-state">İncelemeler yükleniyor...</div> : filtered.length === 0 ? <div className="table-state">Gösterilecek inceleme bulunamadı.</div> :
      <div className="inspector-investigation-table"><table><thead><tr><th>Takip Kodu</th><th>Tür</th><th>Şoför</th><th>Hat</th><th>Atanma</th><th>Durum</th><th>İşlem</th></tr></thead><tbody>
        {filtered.map((item) => <tr key={item.id}><td className="investigation-tracking">{item.trackingCode}</td><td>{item.complaintTypeName}</td><td>{item.driverFullName || "Belirtilmedi"}</td><td>{item.routeCode || "Belirtilmedi"}</td><td>{formatDateTime(item.investigationCreatedDate)}</td>
          <td><span className={`investigation-status process-${item.processStatus}`}>{STATUS[item.processStatus] || "Bilinmiyor"}</span></td>
          <td><button className="investigation-detail-button" type="button" onClick={() => open(item)}>{item.processStatus === PROCESS.completed ? "Kararı Gör" : "İncele"}</button></td></tr>)}
      </tbody></table></div>}</div>
    {selected && <div className="investigation-modal-backdrop" onMouseDown={close}><div className="investigation-modal" role="dialog" aria-modal="true" onMouseDown={(e) => e.stopPropagation()}>
      <div className="investigation-modal-header"><div><h2>Şikâyet İncelemesi</h2><p>{selected.trackingCode}</p></div><button type="button" onClick={close} disabled={submitting}>×</button></div>
      <div className="investigation-detail-grid"><Detail label="Şikâyet Türü" value={selected.complaintTypeName} /><Detail label="Şoför" value={selected.driverFullName} /><Detail label="Hat" value={selected.routeCode || "Belirtilmedi"} /><Detail label="Araç" value={selected.vehicleDoorNumber} /><Detail wide label="Şikâyet Açıklaması" value={selected.complaintDescription} /></div>
      {selected.processStatus === PROCESS.driver && <section className="investigation-waiting">Şoför açıklaması bekleniyor.</section>}
      {selected.processStatus === PROCESS.final && <><section className="investigation-decision-summary"><h3>Şoför Açıklaması</h3><Detail wide label="Açıklama" value={selected.driverExplanation} /><Detail label="Açıklama Tarihi" value={formatDateTime(selected.driverExplanationDate)} /></section><DecisionEditor value={result} setValue={setResult} submitting={submitting} error={actionError} onSubmit={() => runAction("final")} buttonText="Nihai Kararı Kaydet" /></>}
      {selected.processStatus === PROCESS.review && <section className="investigation-decision-form"><p>Şikâyeti şoföre iletebilir veya gerekçeli olarak sonuçlandırabilirsiniz.</p><label htmlFor="reject-result">Ret gerekçesi / nihai karar</label><textarea id="reject-result" maxLength="1000" rows="5" value={result} disabled={submitting} onChange={(e) => { setResult(e.target.value); setActionError(""); }} /><small>{result.length}/1000 karakter</small>{actionError && <div className="decision-error">{actionError}</div>}<div className="decision-actions"><button className="decision-button decision-button--reject" disabled={submitting} onClick={() => runAction("reject")}>Reddet ve Sonuçlandır</button><button className="decision-button decision-button--approve" disabled={submitting} onClick={() => runAction("approve")}>Şoföre İlet</button></div></section>}
      {selected.processStatus === PROCESS.completed && <section className="investigation-decision-summary"><h3>Tamamlandı</h3><Detail wide label="Nihai Karar" value={selected.investigationResult} /><Detail label="Kapanış Tarihi" value={formatDateTime(selected.closedDate)} /></section>}
    </div></div>}
  </div>;
}
function DecisionEditor({ value, setValue, submitting, error, onSubmit, buttonText }) {
  return <section className="investigation-decision-form"><label htmlFor="final-result">Nihai Karar</label><textarea id="final-result" maxLength="1000" rows="5" value={value} disabled={submitting} onChange={(e) => setValue(e.target.value)} /><small>{value.length}/1000 karakter</small>{error && <div className="decision-error">{error}</div>}<div className="decision-actions"><button className="decision-button decision-button--approve" disabled={submitting} onClick={onSubmit}>{submitting ? "Kaydediliyor..." : buttonText}</button></div></section>;
}
