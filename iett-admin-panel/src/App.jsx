import { useState } from "react";
import "./App.css";

import LoginPage from "./pages/LoginPage";
import Sidebar from "./components/layout/Sidebar";
import VehiclesPage from "./pages/VehiclesPage";
import DriversPage from "./pages/DriversPage";
import BusRoutesPage from "./pages/BusRoutesPage";
import BusStopsPage from "./pages/BusStopsPage";
import AdminDashboard from "./pages/dashboards/AdminDashboard";
import DriverDashboard from "./pages/dashboards/DriverDashboard";
import InspectorDashboard from "./pages/dashboards/InspectorDashboard";

const rolePages = {
  Admin: ["adminDashboard", "vehicles", "drivers", "busRoutes", "busStops"],
  Driver: ["driverDashboard", "driverTasks"],
  Inspector: ["inspectorDashboard", "inspectorTasks", "drivers"],
};

const roleStartPages = {
  Admin: "adminDashboard",
  Driver: "driverDashboard",
  Inspector: "inspectorDashboard",
};

function getStartPage(role) {
  return roleStartPages[role] || "adminDashboard";
}

function App() {
  // Kullanıcı bilgilerini localStorage üzerinden geri yükler.
  // Böylece sayfa yenilendiğinde oturum kapanmaz.
  const [currentUser, setCurrentUser] = useState(() => {
    const savedUser = localStorage.getItem("currentUser");

    if (!savedUser) {
      return null;
    }

    try {
      return JSON.parse(savedUser);
    } catch {
      localStorage.removeItem("currentUser");
      localStorage.removeItem("token");
      return null;
    }
  });

  // Sidebar açık veya kapalı durumu
  const [sidebarOpen, setSidebarOpen] = useState(true);

  // Son açık olan sayfayı localStorage üzerinden hatırlar
  const [activePage, setActivePage] = useState(() => {
    const savedPage = localStorage.getItem("activePage");
    const allowedPages = rolePages[currentUser?.role] || [];

    return allowedPages.includes(savedPage)
      ? savedPage
      : getStartPage(currentUser?.role);
  });

  // Seçilen hat bilgisini sayfa yenilense bile korur
  const [selectedRoute, setSelectedRoute] = useState(() => {
    const savedRoute = localStorage.getItem("selectedRoute");

    if (!savedRoute) {
      return null;
    }

    try {
      return JSON.parse(savedRoute);
    } catch {
      localStorage.removeItem("selectedRoute");
      return null;
    }
  });

  // Giriş başarılı olduğunda kullanıcı ve token bilgilerini saklar
  function handleLogin(userData) {
    const startPage = getStartPage(userData.role);

    setCurrentUser(userData);
    setActivePage(startPage);

    localStorage.setItem(
      "currentUser",
      JSON.stringify(userData),
    );

    localStorage.setItem("token", userData.token);
    localStorage.setItem("activePage", startPage);
  }

  // Kullanıcı çıkış yaptığında oturum bilgilerini temizler
  function handleLogout() {
    setCurrentUser(null);
    setActivePage("adminDashboard");
    setSelectedRoute(null);

    localStorage.removeItem("currentUser");
    localStorage.removeItem("token");
    localStorage.removeItem("activePage");
    localStorage.removeItem("selectedRoute");
  }

  // Sidebar üzerinden sayfa değiştirildiğinde çalışır
  function handlePageChange(page) {
    setActivePage(page);
    localStorage.setItem("activePage", page);
  }

  // Hat seçildiğinde duraklar sayfasına geçer
  function handleSelectRoute(route) {
    setSelectedRoute(route);

    localStorage.setItem(
      "selectedRoute",
      JSON.stringify(route),
    );

    handlePageChange("busStops");
  }

  // Duraklar sayfasından hatlar sayfasına döner
  function handleBackToRoutes() {
    handlePageChange("busRoutes");
  }

  // Kullanıcı giriş yapmadıysa giriş ekranını göster
  if (!currentUser) {
    return <LoginPage onLogin={handleLogin} />;
  }

  // Kullanıcı giriş yaptıysa mevcut yönetim panelini göster
  return (
    <div
      className={`admin-layout ${
        sidebarOpen ? "" : "sidebar-closed"
      }`}
    >
      <Sidebar
        sidebarOpen={sidebarOpen}
        activePage={activePage}
        setActivePage={handlePageChange}
        currentUser={currentUser}
        onLogout={handleLogout}
      />

      <main className="content">
        <button
          type="button"
          className="sidebar-toggle"
          onClick={() =>
            setSidebarOpen((current) => !current)
          }
        >
          {sidebarOpen ? "←" : "→"}
        </button>

        {activePage === "vehicles" && (
          <VehiclesPage />
        )}

        {activePage === "adminDashboard" && (
          <AdminDashboard
            currentUser={currentUser}
            onNavigate={handlePageChange}
          />
        )}

        {activePage === "driverDashboard" && (
          <DriverDashboard currentUser={currentUser} />
        )}

        {activePage === "inspectorDashboard" && (
          <InspectorDashboard currentUser={currentUser} />
        )}

        {(activePage === "driverTasks" ||
          activePage === "inspectorTasks") && (
          <section>
            <header className="page-header">
              <div>
                <h1>Görevlerim</h1>
                <p>Görevleriniz ileride bu ekranda gösterilecek.</p>
              </div>
            </header>
          </section>
        )}

        {activePage === "drivers" && (
          <DriversPage />
        )}

        {activePage === "busRoutes" && (
          <BusRoutesPage
            onSelectRoute={handleSelectRoute}
          />
        )}

        {activePage === "busStops" && (
          <BusStopsPage
            selectedRoute={selectedRoute}
            onBack={handleBackToRoutes}
          />
        )}
      </main>
    </div>
  );
}

export default App;
