import "./Dashboard.css";

const driverSections = [
  ["📋", "Görevlerim", "Atanan görevleriniz burada gösterilecek."],
  ["🚌", "Seferlerim", "Planlanan seferleriniz burada gösterilecek."],
  ["📄", "Sertifikalarım", "Sertifika bilgileriniz burada gösterilecek."],
];

function DriverDashboard({ currentUser }) {
  return (
    <section className="dashboard-page">
      <header className="page-header">
        <div>
          <h1>Hoş geldiniz, {currentUser?.fullName}</h1>
          <p>Rol: {currentUser?.role}</p>
        </div>
      </header>

      <div className="dashboard-grid">
        {driverSections.map(([icon, title, description]) => (
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

export default DriverDashboard;
