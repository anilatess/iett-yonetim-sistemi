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
  const deadlineInvestigationId = normalizeEntityId(payload.investigationId);
  const deadlineSentAt = normalizeOccurredAt(payload.sentAt);
  const deadlineType = ["FinalBusinessDay", "Overdue"].includes(payload.reminderType)
    ? payload.reminderType
    : null;
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
    DriverExplanationSubmitted: {
      entityId: normalizeEntityId(payload.investigationId),
      title: "Şoför Açıklaması Geldi",
      message: payload.message || "Şoför açıklaması gönderildi; nihai kararınız bekleniyor.",
      occurredAt: normalizeOccurredAt(payload.submittedDate),
      targetPage: "inspectorComplaints",
    },
    InvestigationDeadlineReminder:
      deadlineInvestigationId && deadlineSentAt && deadlineType
        ? {
            entityId: deadlineInvestigationId,
            title: deadlineType === "Overdue"
              ? "Süresi Aşılmış Şikâyet"
              : "Son İş Günü",
            message: typeof payload.message === "string" && payload.message.trim()
              ? payload.message
              : deadlineType === "Overdue"
                ? "Şikâyetin sonuçlandırma süresi aşılmıştır."
                : "Şikâyetin sonuçlandırılması için son iş günüdür.",
            occurredAt: deadlineSentAt,
            targetPage: "inspectorComplaints",
            id: `InvestigationDeadlineReminder:${deadlineInvestigationId}:${deadlineType}:${deadlineSentAt}`,
          }
        : null,
  };
  const configuration = configurations[type];

  if (!configuration) return null;

  return {
    id: configuration.id || (configuration.entityId
      ? `${type}:${configuration.entityId}`
      : `${type}:missing:${Date.now()}`),
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
  const hubConnectionRef = useRef(null);
  const connectionLifecycleRef = useRef(Promise.resolve());
  const [toastQueue, setToastQueue] = useState([]);
  const [complaintsRefreshKey, setComplaintsRefreshKey] = useState(0);
  const [tripsRefreshKey, setTripsRefreshKey] = useState(0);
  const [performanceRefreshKey, setPerformanceRefreshKey] = useState(0);
  const [inspectorComplaintsRefreshKey, setInspectorComplaintsRefreshKey] = useState(0);

  const closeComplaintNotification = useCallback(() => {
    setToastQueue((current) => current.slice(1));
  }, []);

  useEffect(() => {
    notificationsRef.current = [];
    setNotifications([]);
    setToastQueue([]);

    if (!(["Driver", "Inspector"].includes(currentUser?.role))) return;

    const storedNotifications = loadNotifications(currentUser.userId, currentUser.role);
    notificationsRef.current = storedNotifications;
    setNotifications(storedNotifications);
  }, [currentUser]);

  useEffect(() => {
    const token = localStorage.getItem("token");

    if (!(["Driver", "Inspector"].includes(currentUser?.role)) || !token) {
      return undefined;
    }

    const connectionUserId = Number(currentUser.userId);
    const connectionRole = currentUser.role;
    let disposed = false;
    let retryTimer = null;
    let retryAttempt = 0;
    let startInProgress = false;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(NOTIFICATION_HUB_URL, {
        accessTokenFactory: () => localStorage.getItem("token") || "",
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();
    hubConnectionRef.current = connection;

    const acceptNotification = (type, payload) => {
      const activeUser = restoreCurrentUser();

      if (
        disposed ||
        activeUser?.role !== connectionRole ||
        Number(activeUser.userId) !== connectionUserId
      ) {
        return;
      }

      const notification = normalizeNotification(type, payload);
      if (!notification) return;
      if (type === "InvestigationDeadlineReminder" && connectionRole !== "Inspector") return;

      const result = addNotification(
        connectionUserId,
        connectionRole,
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
      } else if (type === "DriverExplanationSubmitted") {
        setInspectorComplaintsRefreshKey((current) => current + 1);
      } else if (type === "InvestigationDeadlineReminder") {
        setInspectorComplaintsRefreshKey((current) => current + 1);
      }
    };

    const handleComplaintForwarded = (payload) =>
      acceptNotification("ComplaintForwarded", payload);
    const handleTripAssigned = (payload) =>
      acceptNotification("TripAssigned", payload);
    const handlePerformanceEvaluated = (payload) =>
      acceptNotification("PerformanceEvaluated", payload);
    const handleDriverExplanationSubmitted = (payload) =>
      acceptNotification("DriverExplanationSubmitted", payload);
    const handleInvestigationDeadlineReminder = (payload) =>
      acceptNotification("InvestigationDeadlineReminder", payload);

    connection.on("ComplaintForwarded", handleComplaintForwarded);
    connection.on("TripAssigned", handleTripAssigned);
    connection.on("PerformanceEvaluated", handlePerformanceEvaluated);
    connection.on("DriverExplanationSubmitted", handleDriverExplanationSubmitted);
    connection.on("InvestigationDeadlineReminder", handleInvestigationDeadlineReminder);

    const scheduleInitialRetry = () => {
      if (disposed || retryTimer) return;

      const retryDelays = [1000, 2000, 5000, 10000];
      const delay = retryDelays[Math.min(retryAttempt, retryDelays.length - 1)];
      retryAttempt += 1;
      retryTimer = window.setTimeout(() => {
        retryTimer = null;
        startConnection();
      }, delay);
    };

    const startConnection = async () => {
      if (
        disposed ||
        startInProgress ||
        connection.state !== signalR.HubConnectionState.Disconnected
      ) {
        return;
      }

      startInProgress = true;
      try {
        await connectionLifecycleRef.current;
        if (disposed) return;

        await connection.start();
        retryAttempt = 0;
        if (import.meta.env.DEV) {
          console.info("Bildirim bağlantısı kuruldu.");
        }
      } catch (error) {
        const expectedCleanupAbort = disposed && (
          error?.name === "AbortError" ||
          String(error?.message || "").includes("stopped during negotiation")
        );

        if (!expectedCleanupAbort && !disposed) {
          console.error("Bildirim bağlantısı kurulamadı.", error);
          scheduleInitialRetry();
        }
      } finally {
        startInProgress = false;
      }
    };

    startConnection();

    return () => {
      disposed = true;
      if (retryTimer) {
        window.clearTimeout(retryTimer);
        retryTimer = null;
      }
      connection.off("ComplaintForwarded", handleComplaintForwarded);
      connection.off("TripAssigned", handleTripAssigned);
      connection.off("PerformanceEvaluated", handlePerformanceEvaluated);
      connection.off("DriverExplanationSubmitted", handleDriverExplanationSubmitted);
      connection.off("InvestigationDeadlineReminder", handleInvestigationDeadlineReminder);

      const stopPromise = connection.stop().catch(() => {});
      connectionLifecycleRef.current = stopPromise;
      if (hubConnectionRef.current === connection) {
        hubConnectionRef.current = null;
      }
    };
  }, [currentUser?.role, currentUser?.userId]);

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
    if (!(["Driver", "Inspector"].includes(currentUser?.role))) return;

    const updatedNotifications = markNotificationRead(
      currentUser.userId,
      currentUser.role,
      notificationsRef.current,
      notification.id,
    );
    notificationsRef.current = updatedNotifications;
    setNotifications(updatedNotifications);
    setToastQueue((current) => current.filter((item) => item.id !== notification.id));
    handlePageChange(notification.targetPage);
  }

  function handleMarkAllNotificationsRead() {
    if (!(["Driver", "Inspector"].includes(currentUser?.role))) return;

    const updatedNotifications = markAllNotificationsRead(
      currentUser.userId,
      currentUser.role,
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
        return <InspectorComplaintsPage refreshKey={inspectorComplaintsRefreshKey} />;
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
    if (["complaint", "tracking"].includes(publicPage)) {
      return (
        <PublicComplaintPage
          initialView={publicPage === "tracking" ? "track" : "create"}
          onBackToLogin={() => setPublicPage("login")}
        />
      );
    }

    return (
      <LoginPage
        onLogin={handleLogin}
        onCreateComplaint={() => setPublicPage("complaint")}
        onTrackComplaint={() => setPublicPage("tracking")}
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

          {["Driver", "Inspector"].includes(currentUser.role) && (
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
