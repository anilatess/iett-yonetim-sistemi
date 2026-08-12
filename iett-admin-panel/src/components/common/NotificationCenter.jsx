import { useEffect, useRef, useState } from "react";
import "./NotificationCenter.css";

function formatNotificationDate(value) {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) return "Tarih bilgisi yok";

  return new Intl.DateTimeFormat("tr-TR", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

export default function NotificationCenter({
  notifications,
  onNotificationClick,
  onMarkAllRead,
}) {
  const [open, setOpen] = useState(false);
  const containerRef = useRef(null);
  const unreadCount = notifications.filter((notification) => !notification.isRead).length;

  useEffect(() => {
    if (!open) return undefined;

    function handlePointerDown(event) {
      if (!containerRef.current?.contains(event.target)) {
        setOpen(false);
      }
    }

    function handleKeyDown(event) {
      if (event.key === "Escape") {
        setOpen(false);
      }
    }

    document.addEventListener("pointerdown", handlePointerDown);
    document.addEventListener("keydown", handleKeyDown);

    return () => {
      document.removeEventListener("pointerdown", handlePointerDown);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [open]);

  function openNotification(notification) {
    setOpen(false);
    onNotificationClick(notification);
  }

  return (
    <div className="notification-center" ref={containerRef}>
      <button
        type="button"
        className="notification-center__bell"
        aria-label={`Bildirimler${unreadCount ? `, ${unreadCount} okunmamış` : ""}`}
        aria-expanded={open}
        aria-controls="notification-panel"
        onClick={() => setOpen((current) => !current)}
      >
        <svg aria-hidden="true" viewBox="0 0 24 24" width="22" height="22">
          <path
            d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
        {unreadCount > 0 && (
          <span className="notification-center__badge" aria-hidden="true">
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <section
          id="notification-panel"
          className="notification-center__panel"
          aria-label="Bildirim merkezi"
        >
          <div className="notification-center__header">
            <div>
              <strong>Bildirimler</strong>
              <span>{unreadCount} okunmamış</span>
            </div>
            {unreadCount > 0 && (
              <button type="button" onClick={onMarkAllRead}>
                Tümünü okundu işaretle
              </button>
            )}
          </div>

          <div className="notification-center__list">
            {notifications.length === 0 ? (
              <p className="notification-center__empty">Henüz bildiriminiz bulunmuyor.</p>
            ) : (
              notifications.map((notification) => (
                <button
                  type="button"
                  key={notification.id}
                  className={`notification-center__item ${
                    notification.isRead ? "is-read" : "is-unread"
                  }`}
                  onClick={() => openNotification(notification)}
                >
                  <span className="notification-center__item-title">
                    {!notification.isRead && <span aria-label="Okunmamış bildirim" />}
                    {notification.title}
                  </span>
                  <span className="notification-center__message">{notification.message}</span>
                  <time dateTime={notification.occurredAt}>
                    {formatNotificationDate(notification.occurredAt)}
                  </time>
                </button>
              ))
            )}
          </div>
        </section>
      )}
    </div>
  );
}
