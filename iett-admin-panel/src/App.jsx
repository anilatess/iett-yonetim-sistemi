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
import TripsPage from "./pages/TripsPage";
import TaskAssignmentPage from "./pages/TaskAssignmentPage";
import ComplaintsPage from "./pages/ComplaintsPage";
import InspectionsPage from "./pages/InspectionsPage";
import UsersPage from "./pages/UsersPage";
import InspectorTasksPage from "./pages/InspectorTasksPage";
import InspectorComplaintsPage from "./pages/InspectorComplaintsPage";
import PerformanceEvaluationPage from "./pages/PerformanceEvaluationPage";
import InvestigationHistoryPage from "./pages/InvestigationHistoryPage";
import ProfilePage from "./pages/ProfilePage";
import DriverTasksPage from "./pages/DriverTasksPage";
import DriverTripsPage from "./pages/DriverTripsPage";
import DriverCertificatesPage from "./pages/DriverCertificatesPage";
import DriverPerformancePage from "./pages/DriverPerformancePage";
import {
  getStartPage,
  isPageAllowed,
} from "./config/navigationConfig";

function restoreCurrentUser() {
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
}

function restoreSelectedRoute() {
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
}

function App() {
  const [currentUser, setCurrentUser] = useState(restoreCurrentUser);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [activePage, setActivePage] = useState(() => {
    const user = restoreCurrentUser();
    const savedPage = localStorage.getItem("activePage");

    return isPageAllowed(user?.role, savedPage)
      ? savedPage
      : getStartPage(user?.role);
  });
  const [selectedRoute, setSelectedRoute] = useState(restoreSelectedRoute);

  function handleLogin(userData) {
    const startPage = getStartPage(userData.role);

    setCurrentUser(userData);
    setActivePage(startPage);
    localStorage.setItem("currentUser", JSON.stringify(userData));
    localStorage.setItem("token", userData.token);
    localStorage.setItem("activePage", startPage);
  }

  function handleLogout() {
    setCurrentUser(null);
    setActivePage("adminDashboard");
    setSelectedRoute(null);
    localStorage.removeItem("currentUser");
    localStorage.removeItem("token");
    localStorage.removeItem("activePage");
    localStorage.removeItem("selectedRoute");
  }

  function handlePageChange(requestedPage) {
    const nextPage = isPageAllowed(currentUser?.role, requestedPage)
      ? requestedPage
      : getStartPage(currentUser?.role);

    setActivePage(nextPage);
    localStorage.setItem("activePage", nextPage);
  }

  function handleSelectRoute(route) {
    setSelectedRoute(route);
    localStorage.setItem("selectedRoute", JSON.stringify(route));
    handlePageChange("busStops");
  }

  function handleBackToRoutes() {
    handlePageChange("busRoutes");
  }

  function renderPage(page) {
    const canEdit = currentUser?.role === "Admin";
    const canManageVehicles = currentUser?.role === "Admin";
    const canChangeVehicleStatus =
      currentUser?.role === "Admin" ||
      currentUser?.role === "Inspector";

    switch (page) {
      case "adminDashboard":
        return <AdminDashboard currentUser={currentUser} onNavigate={handlePageChange} />;
      case "inspectorDashboard":
        return <InspectorDashboard currentUser={currentUser} onNavigate={handlePageChange} />;
      case "driverDashboard":
        return <DriverDashboard currentUser={currentUser} onNavigate={handlePageChange} />;
      case "vehicles":
        return (
          <VehiclesPage
            canManageVehicles={canManageVehicles}
            canChangeVehicleStatus={canChangeVehicleStatus}
          />
        );
      case "drivers":
        return <DriversPage role={currentUser?.role} />;
      case "busRoutes":
        return <BusRoutesPage canEdit={canEdit} onSelectRoute={handleSelectRoute} />;
      case "busStops":
        return <BusStopsPage selectedRoute={selectedRoute} onBack={handleBackToRoutes} />;
      case "trips":
        return <TripsPage />;
      case "taskAssignment":
        return <TaskAssignmentPage />;
      case "complaints":
        return <ComplaintsPage />;
      case "inspections":
        return <InspectionsPage />;
      case "users":
        return <UsersPage />;
      case "inspectorTasks":
        return <InspectorTasksPage />;
      case "inspectorComplaints":
        return <InspectorComplaintsPage />;
      case "performanceEvaluation":
        return <PerformanceEvaluationPage />;
      case "investigationHistory":
        return <InvestigationHistoryPage />;
      case "driverTasks":
        return <DriverTasksPage />;
      case "driverTrips":
        return <DriverTripsPage />;
      case "driverCertificates":
        return <DriverCertificatesPage />;
      case "driverPerformance":
        return <DriverPerformancePage />;
      case "profile":
        return <ProfilePage role={currentUser?.role} />;
      default:
        return null;
    }
  }

  if (!currentUser) {
    return <LoginPage onLogin={handleLogin} />;
  }

  const safeActivePage = isPageAllowed(currentUser.role, activePage)
    ? activePage
    : getStartPage(currentUser.role);

  return (
    <div className={`admin-layout ${sidebarOpen ? "" : "sidebar-closed"}`}>
      <Sidebar
        sidebarOpen={sidebarOpen}
        activePage={safeActivePage}
        setActivePage={handlePageChange}
        currentUser={currentUser}
        onLogout={handleLogout}
      />

      <main className="content">
        <button
          type="button"
          className="sidebar-toggle"
          onClick={() => setSidebarOpen((current) => !current)}
        >
          {sidebarOpen ? "←" : "→"}
        </button>

        {renderPage(safeActivePage)}
      </main>
    </div>
  );
}

export default App;
