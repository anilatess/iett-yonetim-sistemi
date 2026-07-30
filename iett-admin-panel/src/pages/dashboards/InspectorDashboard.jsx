import "./Dashboard.css";

const inspectorSections = [
  ["📋", "Görevlerim", "Atanan denetim görevleriniz burada gösterilecek."],
  ["🔎", "Şikâyet İncelemeleri", "İncelenecek şikâyetler burada gösterilecek."],
  ["📊", "Performans Değerlendirmeleri", "Şoför değerlendirmeleri burada gösterilecek."],
];

function InspectorDashboard({ currentUser }) {
  return (
    <section className="dashboard-page">
      <header className="page-header">
        <div>
          <h1>Hoş geldiniz, {currentUser?.fullName}</h1>
          <p>Rol: {currentUser?.role}</p>
        </div>
      </header>

      <div className="dashboard-grid">
        {inspectorSections.map(([icon, title, description]) => (
          <article className="dashboard-card" key={title}>
            <span className="dashboard-card-icon">{icon}</span>
            <h2>{title}</h2>
            <p>{description}</p>
          </article>
        ))}
      </div>
    </section>
  );
}

export default InspectorDashboard;
