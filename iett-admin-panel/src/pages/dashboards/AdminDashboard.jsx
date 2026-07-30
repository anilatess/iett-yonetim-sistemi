import "./Dashboard.css";

const adminSections = [
  {
    page: "vehicles",
    icon: "🚌",
    title: "Araçlar",
    description: "Araç kayıtlarını ve durumlarını yönetin.",
  },
  {
    page: "drivers",
    icon: "👤",
    title: "Şoförler",
    description: "Şoför bilgilerini görüntüleyin.",
  },
  {
    page: "busRoutes",
    icon: "🛣️",
    title: "Hatlar",
    description: "Hatları ve hatlara bağlı durakları yönetin.",
  },
];

function AdminDashboard({ currentUser, onNavigate }) {
  return (
    <section className="dashboard-page">
      <header className="page-header">
        <div>
          <h1>Hoş geldiniz, {currentUser?.fullName}</h1>
          <p>Yönetim ekranına geçmek için bir bölüm seçin.</p>
        </div>
      </header>

      <div className="dashboard-grid">
        {adminSections.map((section) => (
          <button
            key={section.page}
            type="button"
            className="dashboard-card"
            onClick={() => onNavigate(section.page)}
          >
            <span className="dashboard-card-icon">{section.icon}</span>
            <h2>{section.title}</h2>
            <p>{section.description}</p>
          </button>
        ))}
      </div>
    </section>
  );
}

export default AdminDashboard;
