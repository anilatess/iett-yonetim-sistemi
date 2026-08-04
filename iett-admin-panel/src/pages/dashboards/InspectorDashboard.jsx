import "./Dashboard.css";

const inspectorSections = [
  ["inspectorTasks", "📋", "Görevlerim", "Atanan denetim görevlerinizi görüntüleyin."],
  ["inspectorComplaints", "🔎", "Şikâyet İncelemeleri", "İncelenecek şikâyetleri görüntüleyin."],
  ["performanceEvaluation", "📊", "Performans Değerlendirme", "Şoför performanslarını değerlendirin."],
  ["investigationHistory", "🕘", "İnceleme Geçmişim", "Tamamladığınız incelemeleri görüntüleyin."],
  ["drivers", "👤", "Şoförler", "Şoför bilgilerini görüntüleyin."],
  ["vehicles", "🚌", "Araçlar", "Araçları salt okunur görüntüleyin."],
  ["busRoutes", "🛣️", "Hatlar", "Hatları ve duraklarını görüntüleyin."],
  ["trips", "🗓️", "Sefer Yönetimi", "Garajınızdaki şoförlerin planlanan seferlerini görüntüleyin ve yönetin."],
];

function InspectorDashboard({ currentUser, onNavigate }) {
  return (
    <section className="dashboard-page">
      <header className="page-header">
        <div>
          <h1>Hoş geldiniz, {currentUser?.fullName}</h1>
          <p>Rol: {currentUser?.role}</p>
        </div>
      </header>

      <div className="dashboard-grid">
        {inspectorSections.map(([page, icon, title, description]) => (
          <button className="dashboard-card" key={page} type="button" onClick={() => onNavigate(page)}>
            <span className="dashboard-card-icon">{icon}</span>
            <h2>{title}</h2>
            <p>{description}</p>
          </button>
        ))}
      </div>
    </section>
  );
}

export default InspectorDashboard;
