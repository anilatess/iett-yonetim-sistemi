const configuredApiUrl = import.meta.env.VITE_API_BASE_URL?.trim();
const defaultApiUrl = import.meta.env.DEV
  ? "http://localhost:5147"
  : window.location.origin;

export const API_BASE_URL = (configuredApiUrl || defaultApiUrl)
  .replace(/\/+$/, "");

export const API_URL = `${API_BASE_URL}/api`;
export const NOTIFICATION_HUB_URL = `${API_BASE_URL}/hubs/notifications`;
