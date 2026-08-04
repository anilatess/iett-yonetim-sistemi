import { useEffect, useMemo, useState } from "react";
import "./ComplaintsPage.css";
import { getComplaint, getComplaints } from "../services/complaintService";

const show = (value) => value === null || value === undefined || value === "" ? "-" : value;
const date = (value) => value ? new Intl.DateTimeFormat("tr-TR").format(new Date(value)) : "-";
const dateTime = (value) => value ? new Intl.DateTimeFormat("tr-TR", { dateStyle: "short", timeStyle: "short" }).format(new Date(value)) : "-";
const time = (value) => value ? String(value).slice(0, 5) : "-";

function DetailItem({ label, value, wide = false }) {
  return <div className={`complaint-detail-item ${wide ? "wide" : ""}`}><span>{label}</span><strong>{show(value)}</strong></div>;
}

export default function ComplaintsPage() {
  const [complaints, setComplaints] = useState([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [detail, setDetail] = useState(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detailError, setDetailError] = useState("");

  useEffect(() => {
    (async () => {
      try { setComplaints(await getComplaints()); }
      catch (err) { setError(err.message || "Şikâyetler getirilemedi."); }
      finally { setLoading(false); }
    })();
  }, []);

  const filtered = useMemo(() => {
    const term = search.trim().toLocaleLowerCase("tr-TR");
    if (!term) return complaints;
    return complaints.filter((item) => [item.trackingCode, item.complaintTypeName, item.complaintDescription,
      item.complaintStatusName, item.routeCode, item.routeName, item.vehicleDoorNumber, item.stopCode, item.stopName]
      .some((value) => String(value ?? "").toLocaleLowerCase("tr-TR").includes(term)));
  }, [complaints, search]);

  async function openDetail(id) {
    setDetail(null); setDetailError(""); setDetailLoading(true);
    try { setDetail(await getComplaint(id)); }
    catch (err) { setDetailError(err.message || "Şikâyet detayı getirilemedi."); }
    finally { setDetailLoading(false); }
  }

  function closeDetail() { setDetail(null); setDetailError(""); setDetailLoading(false); }
  const modalOpen = detailLoading || detailError || detail;

  return <div className="complaints-page">
    <div className="page-header"><div><h1>Şikâyetler</h1><p>Vatandaş şikâyetlerini ve ilişkili kayıtları görüntüleyin.</p></div><span className="complaint-count">{filtered.length} kayıt</span></div>
    <div className="complaints-toolbar"><label htmlFor="complaint-search">Arama</label><input id="complaint-search" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Takip kodu, tür, durum, hat, araç veya durak ara..." /></div>
    {error && <div className="complaints-error">{error}</div>}
    <div className="complaints-table-card">{loading ? <div className="complaints-state">Şikâyetler yükleniyor...</div> : filtered.length === 0 ? <div className="complaints-state">{search ? "Aramanızla eşleşen şikâyet bulunamadı." : "Kayıtlı şikâyet bulunamadı."}</div> : <div className="complaints-table-wrapper"><table><thead><tr><th>Takip Kodu</th><th>Tür</th><th>Açıklama</th><th>Oluşturulma</th><th>Durum</th><th>Hat</th><th>Araç</th><th>Durak</th><th>İşlem</th></tr></thead><tbody>{filtered.map((item) => <tr key={item.id}><td className="tracking-code">{item.trackingCode}</td><td>{item.complaintTypeName}</td><td className="description-cell" title={item.complaintDescription}>{item.complaintDescription}</td><td>{dateTime(item.createdDate)}</td><td><span className={`complaint-status status-${item.complaintStatus}`}>{item.complaintStatusName}</span></td><td>{item.routeCode} · {item.routeName}</td><td>{item.vehicleDoorNumber}</td><td>{item.stopCode} · {item.stopName}</td><td><button className="complaint-detail-button" onClick={() => openDetail(item.id)}>Detay</button></td></tr>)}</tbody></table></div>}</div>
    {modalOpen && <div className="complaint-modal-backdrop" onMouseDown={closeDetail}><div className="complaint-modal" onMouseDown={(e) => e.stopPropagation()}><div className="complaint-modal-header"><div><h2>Şikâyet Detayı</h2><p>{detail?.trackingCode || "Kayıt bilgileri yükleniyor"}</p></div><button onClick={closeDetail} aria-label="Kapat">×</button></div>
      {detailLoading ? <div className="complaints-state">Şikâyet detayı yükleniyor...</div> : detailError ? <div className="complaints-error">{detailError}</div> : detail && <>
        <section><h3>Şikâyet Bilgileri</h3><div className="complaint-detail-grid"><DetailItem label="Takip Kodu" value={detail.trackingCode}/><DetailItem label="Tür" value={detail.complaintTypeName}/><DetailItem label="Şikâyet Tarihi" value={`${date(detail.complaintDate)} ${time(detail.complaintTime)}`}/><DetailItem label="Oluşturulma" value={dateTime(detail.createdDate)}/><DetailItem label="Durum" value={detail.complaintStatusName}/><DetailItem label="Hat" value={`${detail.routeCode} · ${detail.routeName}`}/><DetailItem label="Araç" value={detail.vehicleDoorNumber}/><DetailItem label="Durak" value={`${detail.stopCode} · ${detail.stopName}`}/><DetailItem wide label="Açıklama" value={detail.complaintDescription}/></div></section>
        <section><h3>Sefer ve Şoför Bilgileri</h3><div className="complaint-detail-grid"><DetailItem label="Sefer No" value={detail.tripId}/><DetailItem label="Sefer Tarihi" value={date(detail.tripDate)}/><DetailItem label="Kalkış" value={time(detail.depertureTime)}/><DetailItem label="Varış" value={time(detail.arrivalTime)}/><DetailItem label="Şoför" value={detail.driverFullName}/><DetailItem label="Personel Numarası" value={detail.driverPersonnelNumber}/></div></section>
        <section><h3>İnceleme Bilgileri</h3>{detail.investigationId ? <div className="complaint-detail-grid"><DetailItem label="Başlık" value={detail.investigationTitle}/><DetailItem label="Denetimci" value={detail.inspectorFullName}/><DetailItem label="Oluşturulma" value={dateTime(detail.investigationCreatedDate)}/><DetailItem label="Kapanış" value={dateTime(detail.investigationClosedDate)}/><DetailItem wide label="Açıklama" value={detail.investigationDescription}/><DetailItem wide label="Sonuç" value={detail.investigationResult}/></div> : <p className="no-investigation">Bu şikâyet için inceleme kaydı bulunmuyor.</p>}</section>
      </>}
    </div></div>}
  </div>;
}
