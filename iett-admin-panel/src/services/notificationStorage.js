const STORAGE_PREFIX = "iett:driver-notifications:v1:";
const MAX_NOTIFICATIONS = 50;
const ALLOWED_TYPES = new Set([
  "ComplaintForwarded",
  "TripAssigned",
  "PerformanceEvaluated",
]);

function getStorageKey(userId) {
  const parsedUserId = Number(userId);

  if (!Number.isInteger(parsedUserId) || parsedUserId <= 0) {
    return null;
  }

  return `${STORAGE_PREFIX}${parsedUserId}`;
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

function saveNotifications(userId, notifications) {
  const key = getStorageKey(userId);

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

export function loadNotifications(userId) {
  const key = getStorageKey(userId);

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
      saveNotifications(userId, safeNotifications);
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

export function addNotification(userId, currentNotifications, notification) {
  if (!isValidNotification(notification)) {
    return { notifications: currentNotifications, added: false };
  }

  const existing = Array.isArray(currentNotifications)
    ? currentNotifications.filter(isValidNotification)
    : [];

  if (existing.some((item) => item.id === notification.id)) {
    return { notifications: existing, added: false };
  }

  const notifications = saveNotifications(userId, [notification, ...existing]);
  return { notifications, added: true };
}

export function markNotificationRead(userId, currentNotifications, notificationId) {
  const notifications = currentNotifications.map((notification) =>
    notification.id === notificationId
      ? { ...notification, isRead: true }
      : notification,
  );

  return saveNotifications(userId, notifications);
}

export function markAllNotificationsRead(userId, currentNotifications) {
  return saveNotifications(
    userId,
    currentNotifications.map((notification) => ({
      ...notification,
      isRead: true,
    })),
  );
}
