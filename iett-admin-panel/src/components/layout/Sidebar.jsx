import "./Sidebar.css";
import iettLogo from "../../assets/iett-logo.png";

function Sidebar({
  sidebarOpen,
  activePage,
  setActivePage,
}) {
  return (
    <aside className="sidebar">
      <div className="sidebar-logo">
        <img src={iettLogo} alt="İETT Logo" />

        {sidebarOpen && <span>İETT Admin</span>}
      </div>

      <nav>
        <button
          type="button"
          className={`menu-item ${
            activePage === "vehicles" ? "active" : ""
          }`}
          onClick={() => setActivePage("vehicles")}
        >
          <span className="menu-icon">🚌</span>
          {sidebarOpen && <span>Araçlar</span>}
        </button>

        <button
          type="button"
          className={`menu-item ${
            activePage === "drivers" ? "active" : ""
          }`}
          onClick={() => setActivePage("drivers")}
        >
          <span className="menu-icon">👤</span>
          {sidebarOpen && <span>Şoförler</span>}
        </button>

        <button type="button" className="menu-item">
          <span className="menu-icon">🛣️</span>
          {sidebarOpen && <span>Hatlar</span>}
        </button>

        <button type="button" className="menu-item">
          <span className="menu-icon">📍</span>
          {sidebarOpen && <span>Duraklar</span>}
        </button>

        <button type="button" className="menu-item">
          <span className="menu-icon">📋</span>
          {sidebarOpen && <span>Şikâyetler</span>}
        </button>
      </nav>
    </aside>
  );
}

export default Sidebar;