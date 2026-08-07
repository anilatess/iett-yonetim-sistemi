import { useCallback, useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import "./App.css";

import LoginPage from "./pages/LoginPage";
import PublicComplaintPage from "./pages/PublicComplaintPage";
import Sidebar from "./components/layout/Sidebar";
import VehiclesPage from "./pages/VehiclesPage";
import DriversPage from "./pages/DriversPage";
import BusRoutesPage from "./pages/BusRoutesPage";
import BusStopsPage from "./pages/BusStopsPage";
import AdminDashboard from "./pages/dashboards/AdminDashboard";
import DriverDashboard from "./pages/dashboards/DriverDashboard";
import InspectorDashboard from "./pages/dashboards/InspectorDashboard";
import TripsPage from "./pages/TripsPage";
import InspectorTripManagementPage from "./pages/InspectorTripManagementPage";
import TaskAssignmentPage from "./pages/TaskAssignmentPage";
import ComplaintsPage from "./pages/ComplaintsPage";
import InspectionsPage from "./pages/InspectionsPage";
import UsersPage from "./pages/UsersPage";
import InspectorComplaintsPage from "./pages/InspectorComplaintsPage";
import InspectorCertificatesPage from "./pages/InspectorCertificatesPage";
import PerformanceEvaluationPage from "./pages/PerformanceEvaluationPage";
import InvestigationHistoryPage from "./pages/InvestigationHistoryPage";
import DriverTripsPage from "./pages/DriverTripsPage";
import DriverCertificatesPage from "./pages/DriverCertificatesPage";
import DriverPerformancePage from "./pages/DriverPerformancePage";
import DriverComplaintsPage from "./pages/DriverComplaintsPage";
import ComplaintNotificationToast from "./components/common/ComplaintNotificationToast";
import { API_BASE_URL } from "./config/apiConfig";
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
  const [publicPage, setPublicPage] = useState("login");
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [activePage, setActivePage] = useState(() => {
    const user = restoreCurrentUser();
    const savedPage = localStorage.getItem("activePage");

    return isPageAllowed(user?.role, savedPage)
      ? savedPage
      : getStartPage(user?.role);
  });
  const [selectedRoute, setSelectedRoute] = useState(restoreSelectedRoute);
  const [complaintNotification, setComplaintNotification] = useState(null);
  const [complaintsRefreshKey, setComplaintsRefreshKey] = useState(0);

  const closeComplaintNotification = useCallback(() => {
    setComplaintNotification(null);
  }, []);

  useEffect(() => {
    const token = localStorage.getItem("token");

    if (currentUser?.role !== "Driver" || !token) {
      return undefined;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/notifications`, {
        accessTokenFactory: () => localStorage.getItem("token") || "",
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    const handleComplaintForwarded = (notification) => {
      setComplaintNotification(notification);
      setComplaintsRefreshKey((current) => current + 1);
    };

    connection.on("ComplaintForwarded", handleComplaintForwarded);
    connection.start().catch((error) => {
      console.error("Bildirim bağlantısı kurulamadı.", error);
    });

    return () => {
      connection.off("ComplaintForwarded", handleComplaintForwarded);
      connection.stop().catch(() => {});
    };
  }, [currentUser]);

  function handleLogin(userData) {
    const startPage = getStartPage(userData.role);

    setCurrentUser(userData);
    setActivePage(startPage);
    localStorage.setItem("currentUser", JSON.stringify(userData));
    localStorage.setItem("token", userData.token);
    localStorage.setItem("activePage", startPage);
  }

  function handleLogout() {
    setComplaintNotification(null);
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
        return currentUser?.role === "Inspector"
          ? <InspectorTripManagementPage />
          : <TripsPage />;
      case "taskAssignment":
        return <TaskAssignmentPage />;
      case "complaints":
        return <ComplaintsPage />;
      case "inspections":
        return <InspectionsPage />;
      case "users":
        return <UsersPage />;
      case "inspectorComplaints":
        return <InspectorComplaintsPage />;
      case "inspectorCertificates":
        return <InspectorCertificatesPage />;
      case "performanceEvaluation":
        return <PerformanceEvaluationPage />;
      case "investigationHistory":
        return <InvestigationHistoryPage />;
      case "driverTrips":
        return <DriverTripsPage />;
      case "driverComplaints":
        return <DriverComplaintsPage refreshKey={complaintsRefreshKey} />;
      case "driverCertificates":
        return <DriverCertificatesPage />;
      case "driverPerformance":
        return <DriverPerformancePage />;
      default:
        return null;
    }
  }

  if (!currentUser) {
    if (publicPage === "complaint") {
      return <PublicComplaintPage onBackToLogin={() => setPublicPage("login")} />;
    }

    return (
      <LoginPage
        onLogin={handleLogin}
        onCreateComplaint={() => setPublicPage("complaint")}
      />
    );
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

      <ComplaintNotificationToast
        notification={complaintNotification}
        onClose={closeComplaintNotification}
        onOpen={() => {
          closeComplaintNotification();
          handlePageChange("driverComplaints");
        }}
      />
    </div>
  );
}

export default App;
