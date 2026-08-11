import { useCallback, useEffect, useRef, useState } from "react";
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
import NotificationCenter from "./components/common/NotificationCenter";
import { NOTIFICATION_HUB_URL } from "./config/apiConfig";
import {
  addNotification,
  loadNotifications,
  markAllNotificationsRead,
  markNotificationRead,
} from "./services/notificationStorage";
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

function normalizeEntityId(value) {
  const entityId = Number(value);
  return Number.isInteger(entityId) && entityId > 0 ? entityId : null;
}

function normalizeOccurredAt(value) {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "" : date.toISOString();
}

function normalizeNotification(type, payload = {}) {
  const configurations = {
    ComplaintForwarded: {
      entityId: normalizeEntityId(payload.complaintId),
      title: "Yeni Şikâyet",
      message: payload.message || "Bir şikâyet size iletildi.",
      occurredAt: normalizeOccurredAt(payload.approvedDate),
      targetPage: "driverComplaints",
    },
    TripAssigned: {
      entityId: normalizeEntityId(payload.tripId),
      title: "Yeni Sefer Görevi",
      message: payload.message || "Yeni bir sefer görevi atandı.",
      occurredAt: normalizeOccurredAt(payload.plannedDepartureDateTime),
      targetPage: "driverTrips",
    },
    PerformanceEvaluated: {
      entityId: normalizeEntityId(payload.performanceId),
      title: "Yeni Performans Değerlendirmesi",
      message: payload.message || "Yeni bir performans değerlendirmeniz var.",
      occurredAt: normalizeOccurredAt(payload.evaluationDate),
      targetPage: "driverPerformance",
    },
  };
  const configuration = configurations[type];

  if (!configuration) return null;

  return {
    id: configuration.entityId
      ? `${type}:${configuration.entityId}`
      : `${type}:missing:${Date.now()}`,
    type,
    title: configuration.title,
    message: String(configuration.message),
    occurredAt: configuration.occurredAt,
    isRead: false,
    targetPage: configuration.targetPage,
    entityId: configuration.entityId,
  };
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
  const [notifications, setNotifications] = useState([]);
  const notificationsRef = useRef([]);
  const [toastQueue, setToastQueue] = useState([]);
  const [complaintsRefreshKey, setComplaintsRefreshKey] = useState(0);
  const [tripsRefreshKey, setTripsRefreshKey] = useState(0);
  const [performanceRefreshKey, setPerformanceRefreshKey] = useState(0);

  const closeComplaintNotification = useCallback(() => {
    setToastQueue((current) => current.slice(1));
  }, []);

  useEffect(() => {
    notificationsRef.current = [];
    setNotifications([]);
    setToastQueue([]);

    if (currentUser?.role !== "Driver") return;

    const storedNotifications = loadNotifications(currentUser.userId);
    notificationsRef.current = storedNotifications;
    setNotifications(storedNotifications);
  }, [currentUser]);

  useEffect(() => {
    const token = localStorage.getItem("token");

    if (currentUser?.role !== "Driver" || !token) {
      return undefined;
    }

    const connectionUserId = Number(currentUser.userId);
    let isActive = true;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(NOTIFICATION_HUB_URL, {
        accessTokenFactory: () => localStorage.getItem("token") || "",
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    const acceptNotification = (type, payload) => {
      const activeUser = restoreCurrentUser();

      if (
        !isActive ||
        activeUser?.role !== "Driver" ||
        Number(activeUser.userId) !== connectionUserId
      ) {
        return;
      }

      const notification = normalizeNotification(type, payload);
      if (!notification) return;

      const result = addNotification(
        connectionUserId,
        notificationsRef.current,
        notification,
      );

      if (!result.added) return;

      notificationsRef.current = result.notifications;
      setNotifications(result.notifications);
      setToastQueue((current) => [...current, notification]);

      if (type === "ComplaintForwarded") {
        setComplaintsRefreshKey((current) => current + 1);
      } else if (type === "TripAssigned") {
        setTripsRefreshKey((current) => current + 1);
      } else if (type === "PerformanceEvaluated") {
        setPerformanceRefreshKey((current) => current + 1);
      }
    };

    const handleComplaintForwarded = (payload) =>
      acceptNotification("ComplaintForwarded", payload);
    const handleTripAssigned = (payload) =>
      acceptNotification("TripAssigned", payload);
    const handlePerformanceEvaluated = (payload) =>
      acceptNotification("PerformanceEvaluated", payload);

    connection.on("ComplaintForwarded", handleComplaintForwarded);
    connection.on("TripAssigned", handleTripAssigned);
    connection.on("PerformanceEvaluated", handlePerformanceEvaluated);
    connection.start().catch((error) => {
      console.error("Bildirim bağlantısı kurulamadı.", error);
    });

    return () => {
      isActive = false;
      connection.off("ComplaintForwarded", handleComplaintForwarded);
      connection.off("TripAssigned", handleTripAssigned);
      connection.off("PerformanceEvaluated", handlePerformanceEvaluated);
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
    notificationsRef.current = [];
    setNotifications([]);
    setToastQueue([]);
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

  function handleNotificationOpen(notification) {
    if (currentUser?.role !== "Driver") return;

    const updatedNotifications = markNotificationRead(
      currentUser.userId,
      notificationsRef.current,
      notification.id,
    );
    notificationsRef.current = updatedNotifications;
    setNotifications(updatedNotifications);
    setToastQueue((current) => current.filter((item) => item.id !== notification.id));
    handlePageChange(notification.targetPage);
  }

  function handleMarkAllNotificationsRead() {
    if (currentUser?.role !== "Driver") return;

    const updatedNotifications = markAllNotificationsRead(
      currentUser.userId,
      notificationsRef.current,
    );
    notificationsRef.current = updatedNotifications;
    setNotifications(updatedNotifications);
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
        return <DriverTripsPage refreshKey={tripsRefreshKey} />;
      case "driverComplaints":
        return <DriverComplaintsPage refreshKey={complaintsRefreshKey} />;
      case "driverCertificates":
        return <DriverCertificatesPage />;
      case "driverPerformance":
        return <DriverPerformancePage refreshKey={performanceRefreshKey} />;
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
        <div className="app-toolbar">
          <button
            type="button"
            className="sidebar-toggle"
            onClick={() => setSidebarOpen((current) => !current)}
          >
            {sidebarOpen ? "←" : "→"}
          </button>

          {currentUser.role === "Driver" && (
            <NotificationCenter
              notifications={notifications}
              onNotificationClick={handleNotificationOpen}
              onMarkAllRead={handleMarkAllNotificationsRead}
            />
          )}
        </div>

        {renderPage(safeActivePage)}
      </main>

      <ComplaintNotificationToast
        notification={toastQueue[0] || null}
        onClose={closeComplaintNotification}
        onOpen={() => toastQueue[0] && handleNotificationOpen(toastQueue[0])}
      />
    </div>
  );
}

export default App;
