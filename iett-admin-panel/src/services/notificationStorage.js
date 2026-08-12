const DRIVER_STORAGE_PREFIX = "iett:driver-notifications:v1:";
const ROLE_STORAGE_PREFIX = "iett:notifications:v1:";
const MAX_NOTIFICATIONS = 50;
const ALLOWED_TYPES = new Set([
  "ComplaintForwarded",
  "TripAssigned",
  "PerformanceEvaluated",
  "DriverExplanationSubmitted",
]);

function getStorageKey(userId, role = "Driver") {
  const parsedUserId = Number(userId);

  if (!Number.isInteger(parsedUserId) || parsedUserId <= 0) {
    return null;
  }

  return role === "Driver"
    ? `${DRIVER_STORAGE_PREFIX}${parsedUserId}`
    : `${ROLE_STORAGE_PREFIX}${String(role).toLowerCase()}:${parsedUserId}`;
}

function isValidNotification(notification) {
  return Boolean(
    notification &&
      typeof notification === "object" &&
      typeof notification.id === "string" &&
      notification.id.length > 0 &&
      ALLOWED_TYPES.has(notification.type) &&
      typeof notification.title === "string" &&
      typeof notification.message === "string" &&
      typeof notification.occurredAt === "string" &&
      typeof notification.isRead === "boolean" &&
      typeof notification.targetPage === "string" &&
      (typeof notification.entityId === "number" || notification.entityId === null),
  );
}

function saveNotifications(userId, role, notifications) {
  const key = getStorageKey(userId, role);

  if (!key) return [];

  const safeNotifications = notifications
    .filter(isValidNotification)
    .slice(0, MAX_NOTIFICATIONS);

  try {
    localStorage.setItem(key, JSON.stringify(safeNotifications));
  } catch {
    // Storage erişilemezse uygulama bellekteki listeyle çalışmaya devam eder.
  }

  return safeNotifications;
}

export function loadNotifications(userId, role) {
  const key = getStorageKey(userId, role);

  if (!key) return [];

  try {
    const parsed = JSON.parse(localStorage.getItem(key) || "[]");

    if (!Array.isArray(parsed)) {
      localStorage.removeItem(key);
      return [];
    }

    const safeNotifications = parsed
      .filter(isValidNotification)
      .slice(0, MAX_NOTIFICATIONS);

    if (safeNotifications.length !== parsed.length) {
      saveNotifications(userId, role, safeNotifications);
    }

    return safeNotifications;
  } catch {
    try {
      localStorage.removeItem(key);
    } catch {
      // Storage temizlenemese de bozuk veri uygulamaya aktarılmaz.
    }
    return [];
  }
}

export function addNotification(userId, role, currentNotifications, notification) {
  if (!isValidNotification(notification)) {
    return { notifications: currentNotifications, added: false };
  }

  const existing = Array.isArray(currentNotifications)
    ? currentNotifications.filter(isValidNotification)
    : [];

  if (existing.some((item) => item.id === notification.id)) {
    return { notifications: existing, added: false };
  }

  const notifications = saveNotifications(userId, role, [notification, ...existing]);
  return { notifications, added: true };
}

export function markNotificationRead(userId, role, currentNotifications, notificationId) {
  const notifications = currentNotifications.map((notification) =>
    notification.id === notificationId
      ? { ...notification, isRead: true }
      : notification,
  );

  return saveNotifications(userId, role, notifications);
}

export function markAllNotificationsRead(userId, role, currentNotifications) {
  return saveNotifications(
    userId,
    role,
    currentNotifications.map((notification) => ({
      ...notification,
      isRead: true,
    })),
  );
}
