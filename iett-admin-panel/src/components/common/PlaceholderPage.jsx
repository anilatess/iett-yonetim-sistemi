import "../../pages/dashboards/Dashboard.css";

function PlaceholderPage({ title, description, tables = [], plannedTables = [] }) {
  return (
    <section className="dashboard-page">
      <header className="page-header">
        <div>
          <h1>{title}</h1>
          <p>{description}</p>
        </div>
      </header>

      <div className="dashboard-grid">
        <article className="dashboard-card placeholder-card">
          <span className="dashboard-card-icon">🔌</span>
          <h2>Henüz veri bağlantısı yapılmadı</h2>
          <p>
            Backend servisleri hazır olduğunda bu ekran gerçek verilerle
            çalışacaktır.
          </p>
        </article>

        {tables.length > 0 && (
          <article className="dashboard-card placeholder-card">
            <span className="dashboard-card-icon">🗄️</span>
            <h2>Bağlanılacak mevcut tablolar</h2>
            <p>{tables.join(", ")}</p>
          </article>
        )}

        {plannedTables.length > 0 && (
          <article className="dashboard-card placeholder-card">
            <span className="dashboard-card-icon">🧩</span>
            <h2>Planlanan görev yapısı</h2>
            <p>{plannedTables.join(", ")}</p>
          </article>
        )}
      </div>
    </section>
  );
}

export default PlaceholderPage;
