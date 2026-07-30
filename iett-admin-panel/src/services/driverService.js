import { apiFetch } from "./apiClient";

const API_URL = "https://localhost:7034/api/Drivers";

export async function getDrivers() {
  return apiFetch(API_URL);
}
