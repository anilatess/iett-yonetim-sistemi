import { useCallback, useEffect, useMemo, useState } from "react";
import "./InspectionsPage.css";

import { getInvestigations } from "../services/investigationService";

function formatDateTime(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return new Intl.DateTimeFormat("tr-TR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function formatTrip(item) {
  if (!item.tripId) return "Belirtilmedi";
  const date = item.tripDate
    ? new Intl.DateTimeFormat("tr-TR", { dateStyle: "medium" })
      .format(new Date(item.tripDate))
    : null;
  const hours = [item.depertureTime, item.arrivalTime]
    .filter(Boolean)
    .map((value) => String(value).slice(0, 5))
    .join(" – ");
  return [`#${item.tripId}`, date, hours].filter(Boolean).join(" · ");
}

function Detail({ label, value, wide = false }) {
  return (
    <div className={`admin-investigation-detail ${wide ? "wide" : ""}`}>
      <span>{label}</span>
      <strong>{value || "-"}</strong>
    </div>
  );
}

export default function InspectionsPage() {
  const [investigations, setInvestigations] = useState([]);
  const [selected, setSelected] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [decisionFilter, setDecisionFilter] = useState("");
  const [inspectorFilter, setInspectorFilter] = useState("");

  const loadInvestigations = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const data = await getInvestigations();
      setInvestigations(Array.isArray(data) ? data : []);
    } catch (requestError) {
      setError(requestError.message || "Denetimler yüklenemedi.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadInvestigations();
  }, [loadInvestigations]);

  const inspectors = useMemo(() => {
    const unique = new Map();
    investigations.forEach((item) => {
      if (!unique.has(item.inspectorId)) {
        unique.set(item.inspectorId, item.inspectorFullName);
      }
    });
    return [...unique.entries()]
      .map(([id, name]) => ({ id, name }))
      .sort((left, right) => left.name.localeCompare(right.name, "tr-TR"));
  }, [investigations]);

  const summary = useMemo(() => ({
    total: investigations.length,
    open: investigations.filter((item) => item.status === "Devam Ediyor").length,
    closed: investigations.filter((item) => item.status === "Tamamlandı").length,
    forwarded: investigations.filter((item) => item.decision === "Şoföre İletildi").length,
    rejected: investigations.filter((item) => item.decision === "Reddedildi").length,
  }), [investigations]);

  const filtered = useMemo(() => {
    const term = search.trim().toLocaleLowerCase("tr-TR");
    return investigations.filter((item) => {
      const matchesSearch = !term || [
        item.inspectorFullName,
        item.trackingCode,
        item.driverFullName,
        item.driverPersonnelNumber,
        item.vehicleDoorNumber,
        item.routeCode,
        item.complaintTypeName,
      ].some((value) => String(value ?? "")
        .toLocaleLowerCase("tr-TR").includes(term));
      return matchesSearch
        && (!statusFilter || item.status === statusFilter)
        && (!decisionFilter || item.decision === decisionFilter)
        && (!inspectorFilter || String(item.inspectorId) === inspectorFilter);
    });
  }, [investigations, search, statusFilter, decisionFilter, inspectorFilter]);

  return (
    <section className="admin-investigations-page">
      <header className="page-header">
        <div>
          <h1>Denetimler</h1>
          <p>Tüm denetimcilerin devam eden ve tamamlanan şikâyet incelemeleri</p>
        </div>
        <span className="admin-investigation-count">{filtered.length} kayıt</span>
      </header>

      <div className="admin-investigation-summary">
        <article><span>Toplam denetim</span><strong>{summary.total}</strong></article>
        <article><span>Devam eden</span><strong>{summary.open}</strong></article>
        <article><span>Tamamlanan</span><strong>{summary.closed}</strong></article>
        <article><span>Şoföre iletilen</span><strong>{summary.forwarded}</strong></article>
        <article><span>Reddedilen</span><strong>{summary.rejected}</strong></article>
      </div>

      <div className="admin-investigation-toolbar">
        <input type="search" value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Denetimci, takip kodu, şoför, araç, hat veya tür ara..."
          aria-label="Denetimlerde ara" />
        <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
          <option value="">Tüm durumlar</option>
          <option>Devam Ediyor</option><option>Tamamlandı</option>
        </select>
        <select value={decisionFilter} onChange={(event) => setDecisionFilter(event.target.value)}>
          <option value="">Tüm kararlar</option>
          <option>Bekliyor</option><option>Şoföre İletildi</option><option>Reddedildi</option>
        </select>
        <select value={inspectorFilter} onChange={(event) => setInspectorFilter(event.target.value)}>
          <option value="">Tüm denetimciler</option>
          {inspectors.map((inspector) => <option key={inspector.id} value={inspector.id}>{inspector.name}</option>)}
        </select>
      </div>

      {error && <div className="admin-investigation-error" role="alert">
        <span>{error}</span><button type="button" onClick={loadInvestigations}>Yeniden Dene</button>
      </div>}

      <div className="admin-investigation-table-card">
        {loading ? <div className="admin-investigation-state">Denetimler yükleniyor...</div>
          : !error && investigations.length === 0
            ? <div className="admin-investigation-state">Henüz denetim kaydı bulunmuyor.</div>
            : !error && filtered.length === 0
              ? <div className="admin-investigation-state">Filtrelere uygun kayıt bulunamadı.</div>
              : !error && <div className="admin-investigation-table-wrap"><table>
                <thead><tr><th>Denetimci</th><th>Takip Kodu</th><th>Şikâyet Türü</th><th>Şoför</th>
                  <th>Araç</th><th>Hat</th><th>Atanma</th><th>Tamamlanma</th><th>Durum</th><th>Karar</th><th></th></tr></thead>
                <tbody>{filtered.map((item) => <tr key={item.investigationId}>
                  <td>{item.inspectorFullName}</td><td className="admin-investigation-tracking">{item.trackingCode}</td>
                  <td>{item.complaintTypeName}</td><td>{item.driverFullName || "Belirtilmedi"}</td>
                  <td>{item.vehicleDoorNumber || "-"}</td><td>{item.routeCode || "-"}</td>
                  <td>{formatDateTime(item.createdDate)}</td><td>{formatDateTime(item.closedDate)}</td>
                  <td><span className={`admin-badge status-${item.status === "Devam Ediyor" ? "open" : "closed"}`}>{item.status}</span></td>
                  <td><span className={`admin-badge decision-${item.decision === "Şoföre İletildi" ? "forwarded" : item.decision === "Reddedildi" ? "rejected" : "pending"}`}>{item.decision}</span></td>
                  <td><button type="button" className="admin-investigation-detail-button" onClick={() => setSelected(item)}>Detay</button></td>
                </tr>)}</tbody>
              </table></div>}
      </div>

      {selected && <div className="admin-investigation-modal-backdrop" onMouseDown={() => setSelected(null)}>
        <div className="admin-investigation-modal" role="dialog" aria-modal="true" aria-labelledby="admin-investigation-title"
          onMouseDown={(event) => event.stopPropagation()}>
          <header><div><h2 id="admin-investigation-title">{selected.investigationTitle || "Denetim Detayı"}</h2>
            <p>{selected.trackingCode}</p></div><button type="button" onClick={() => setSelected(null)} aria-label="Kapat">×</button></header>
          <div className="admin-investigation-detail-grid">
            <Detail label="Denetimci" value={selected.inspectorFullName} />
            <Detail label="Şoför" value={[selected.driverFullName, selected.driverPersonnelNumber].filter(Boolean).join(" · ")} />
            <Detail label="Araç / Kapı No" value={selected.vehicleDoorNumber} />
            <Detail label="Hat" value={[selected.routeCode, selected.routeName].filter(Boolean).join(" · ")} />
            <Detail label="Durak" value={[selected.stopCode, selected.stopName].filter(Boolean).join(" · ")} />
            <Detail label="Sefer" value={formatTrip(selected)} />
            <Detail label="Atanma Tarihi" value={formatDateTime(selected.createdDate)} />
            <Detail label="Tamamlanma Tarihi" value={formatDateTime(selected.closedDate)} />
            <Detail wide label="Şikâyet Açıklaması" value={selected.complaintDescription} />
            <Detail wide label="İnceleme Açıklaması" value={selected.investigationDescription} />
            <Detail wide label="Denetim Sonucu" value={selected.investigationResult || "Henüz sonuçlandırılmadı."} />
          </div>
        </div>
      </div>}
    </section>
  );
}
