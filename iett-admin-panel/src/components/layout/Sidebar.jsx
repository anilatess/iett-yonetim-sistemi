import "./Sidebar.css";
import iettLogo from "../../assets/iett-logo.png";
import { getRoleNavigation } from "../../config/navigationConfig";

function Sidebar({
  sidebarOpen,
  activePage,
  setActivePage,
  currentUser,
  onLogout,
}) {
  const navigation = getRoleNavigation(currentUser?.role);
  const menuItems = navigation?.menuItems || [];

  return (
    <aside className="sidebar">
      <div className="sidebar-logo">
        <img src={iettLogo} alt="İETT Logo" />

        {sidebarOpen && <span>{navigation?.title || "İETT Panel"}</span>}
      </div>

      <nav className="sidebar-menu">
        {menuItems.map(([page, icon, label]) => (
          <button
            key={page}
            type="button"
            className={`menu-item ${
              activePage === page ? "active" : ""
            }`}
            onClick={() => setActivePage(page)}
          >
            <span className="menu-icon">{icon}</span>
            {sidebarOpen && <span>{label}</span>}
          </button>
        ))}
      </nav>

      <div className="sidebar-user">
        {sidebarOpen && (
          <div className="sidebar-user-info">
            <strong>{currentUser?.fullName}</strong>
            <span>{currentUser?.role}</span>
          </div>
        )}

        <button
          type="button"
          className="logout-button"
          onClick={onLogout}
          title="Çıkış Yap"
        >
          <span className="menu-icon">🚪</span>
          {sidebarOpen && <span>Çıkış Yap</span>}
        </button>
      </div>
    </aside>
  );
}

export default Sidebar;
