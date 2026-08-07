import { useCallback, useEffect, useMemo, useState } from "react";
import "./InspectorComplaintsPage.css";

import {
  decideInvestigation,
  getMyInvestigations,
} from "../services/inspectorService";

function formatDateTime(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return new Intl.DateTimeFormat("tr-TR", {
    day: "2-digit",
    month: "long",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

function Detail({ label, value, wide = false }) {
  return (
    <div className={`inspector-complaint-detail ${wide ? "wide" : ""}`}>
      <span>{label}</span>
      <strong>{value || "-"}</strong>
    </div>
  );
}

export default function InspectorComplaintsPage() {
  const [investigations, setInvestigations] = useState([]);
  const [selected, setSelected] = useState(null);
  const [result, setResult] = useState("");
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [actionError, setActionError] = useState("");
  const [search, setSearch] = useState("");

  const loadInvestigations = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const data = await getMyInvestigations();
      setInvestigations(Array.isArray(data) ? data : []);
      return Array.isArray(data) ? data : [];
    } catch (requestError) {
      setError(requestError.message || "Şikâyet incelemeleri yüklenemedi.");
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadInvestigations();
  }, [loadInvestigations]);

  const filtered = useMemo(() => {
    const term = search.trim().toLocaleLowerCase("tr-TR");
    if (!term) return investigations;
    return investigations.filter((item) =>
      [item.trackingCode, item.complaintTypeName, item.driverFullName, item.routeCode]
        .some((value) => String(value ?? "").toLocaleLowerCase("tr-TR").includes(term)),
    );
  }, [investigations, search]);

  function openInvestigation(item) {
    setSelected(item);
    setResult(item.closedDate ? item.investigationResult || "" : "");
    setActionError("");
  }

  function closeModal() {
    if (submitting) return;
    setSelected(null);
    setResult("");
    setActionError("");
  }

  async function submitDecision(decision) {
    const trimmedResult = result.trim();
    if (!trimmedResult) {
      setActionError("Karar açıklaması zorunludur.");
      return;
    }
    if (trimmedResult.length > 1000) {
      setActionError("Karar açıklaması en fazla 1000 karakter olabilir.");
      return;
    }

    const confirmation = decision === "Approved"
      ? "Bu şikâyet ilgili şoföre iletilecek. Devam etmek istiyor musunuz?"
      : "Bu şikâyet reddedilecek ve şoföre gönderilmeyecek. Devam etmek istiyor musunuz?";
    if (!window.confirm(confirmation)) return;

    setSubmitting(true);
    setActionError("");
    try {
      await decideInvestigation(selected.id, decision, trimmedResult);
      await loadInvestigations();
      closeModal();
      setSelected(null);
    } catch (requestError) {
      setActionError(requestError.message || "Karar kaydedilemedi. Lütfen tekrar deneyin.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="inspector-complaints-page">
      <div className="page-header">
        <div><h1>Şikâyet İncelemeleri</h1><p>Size atanan şikâyetleri inceleyip şoföre iletin veya reddedin.</p></div>
        <span className="investigation-count">{filtered.length} kayıt</span>
      </div>

      <div className="investigation-toolbar">
        <input type="search" value={search} onChange={(event) => setSearch(event.target.value)}
          placeholder="Takip kodu, tür, şoför veya hat ara..." aria-label="Şikâyet incelemelerinde ara" />
      </div>
      {error && <div className="alert-message">{error}</div>}

      <div className="table-card inspector-investigation-card">
        {loading ? <div className="table-state">İncelemeler yükleniyor...</div>
          : !error && filtered.length === 0 ? <div className="table-state">Gösterilecek inceleme bulunamadı.</div>
            : !error && <div className="inspector-investigation-table"><table><thead><tr>
              <th>Takip Kodu</th><th>Şikâyet Türü</th><th>Şoför</th><th>Hat</th><th>Atanma Tarihi</th><th>Durum</th><th>İşlem</th>
            </tr></thead><tbody>{filtered.map((item) => <tr key={item.id}>
              <td className="investigation-tracking">{item.trackingCode}</td><td>{item.complaintTypeName}</td>
              <td>{item.driverFullName || "-"}</td><td>{item.routeCode || "-"}</td><td>{formatDateTime(item.investigationCreatedDate)}</td>
              <td><span className={`investigation-status ${item.closedDate ? `status-${item.complaintStatus}` : "status-open"}`}>
                {item.closedDate ? item.complaintStatusName : "Denetimci İncelemesinde"}</span></td>
              <td><button type="button" className="investigation-detail-button" onClick={() => openInvestigation(item)}>
                {item.closedDate ? "Kararı Gör" : "İncele"}</button></td>
            </tr>)}</tbody></table></div>}
      </div>

      {selected && <div className="investigation-modal-backdrop" onMouseDown={closeModal}>
        <div className="investigation-modal" role="dialog" aria-modal="true" aria-labelledby="investigation-title"
          onMouseDown={(event) => event.stopPropagation()}>
          <div className="investigation-modal-header"><div><h2 id="investigation-title">Şikâyet İncelemesi</h2><p>{selected.trackingCode}</p></div>
            <button type="button" onClick={closeModal} disabled={submitting} aria-label="Kapat">×</button></div>
          <div className="investigation-detail-grid">
            <Detail label="Şikâyet Türü" value={selected.complaintTypeName} /><Detail label="Şoför" value={selected.driverFullName} />
            <Detail label="Hat" value={[selected.routeCode, selected.routeName].filter(Boolean).join(" · ")} />
            <Detail label="Araç" value={selected.vehicleDoorNumber} /><Detail label="Durak" value={selected.stopName} />
            <Detail wide label="Şikâyet Açıklaması" value={selected.complaintDescription} />
          </div>

          {selected.closedDate ? <section className="investigation-decision-summary">
            <h3>{selected.complaintStatusName}</h3><Detail wide label="Karar Açıklaması" value={selected.investigationResult} />
            <Detail label="Kapanış Tarihi" value={formatDateTime(selected.closedDate)} />
          </section> : <section className="investigation-decision-form">
            <label htmlFor="decision-result">Karar Açıklaması <span>*</span></label>
            <textarea id="decision-result" rows="6" maxLength="1000" value={result} disabled={submitting}
              onChange={(event) => { setResult(event.target.value); setActionError(""); }} />
            <small>{result.length}/1000 karakter</small>
            {actionError && <div className="decision-error" role="alert">{actionError}</div>}
            <div className="decision-actions">
              <button type="button" className="decision-button decision-button--reject" disabled={submitting}
                onClick={() => submitDecision("Rejected")}>{submitting ? "İşlem yapılıyor..." : "Reddet"}</button>
              <button type="button" className="decision-button decision-button--approve" disabled={submitting}
                onClick={() => submitDecision("Approved")}>{submitting ? "İşlem yapılıyor..." : "Şoföre İlet"}</button>
            </div>
          </section>}
        </div>
      </div>}
    </div>
  );
}
