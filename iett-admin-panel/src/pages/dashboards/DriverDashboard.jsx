import "./Dashboard.css";

const driverSections = [
  ["driverTasks", "📋", "Görevlerim", "Atanan görevleriniz burada gösterilecek."],
  ["driverTrips", "🚌", "Seferlerim", "Planlanan seferleriniz burada gösterilecek."],
  ["driverCertificates", "📄", "Sertifikalarım", "Sertifika bilgileriniz burada gösterilecek."],
  ["driverPerformance", "📊", "Performansım", "Performans değerlendirmeleriniz burada gösterilecek."],
  ["profile", "👤", "Profilim", "Kullanıcı ve personel bilgilerinizi görüntüleyin."],
];

function DriverDashboard({ currentUser, onNavigate }) {
  return (
    <section className="dashboard-page">
      <header className="page-header">
        <div>
          <h1>Hoş geldiniz, {currentUser?.fullName}</h1>
          <p>Rol: {currentUser?.role}</p>
        </div>
      </header>

      <div className="dashboard-grid">
        {driverSections.map(([page, icon, title, description]) => (
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

export default DriverDashboard;
