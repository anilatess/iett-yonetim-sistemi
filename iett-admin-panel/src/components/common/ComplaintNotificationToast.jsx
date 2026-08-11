import { useEffect } from "react";
import "./ComplaintNotificationToast.css";

export default function ComplaintNotificationToast({ notification, onClose, onOpen }) {
  useEffect(() => {
    if (!notification) return undefined;

    const timeoutId = window.setTimeout(onClose, 7000);
    return () => window.clearTimeout(timeoutId);
  }, [notification, onClose]);

  if (!notification) return null;

  return (
    <div className="complaint-notification" role="status" aria-live="polite">
      <button
        type="button"
        className="complaint-notification__body"
        onClick={onOpen}
      >
        <span className="complaint-notification__eyebrow">Yeni bildirim</span>
        <strong>{notification.title}</strong>
        <span>{notification.message}</span>
      </button>
      <button
        type="button"
        className="complaint-notification__close"
        onClick={onClose}
        aria-label="Bildirimi kapat"
      >
        ×
      </button>
    </div>
  );
}
