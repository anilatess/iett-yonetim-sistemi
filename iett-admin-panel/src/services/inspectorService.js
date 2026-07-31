import { apiFetch } from "./apiClient";

const API_URL = "https://localhost:7034/api/Inspectors";

export async function createDriverPerformance(performance) {
  return apiFetch(`${API_URL}/me/performances`, {
    method: "POST",
    body: JSON.stringify(performance),
  });
}

export async function getMyPerformanceHistory() {
  return apiFetch(`${API_URL}/me/performances`);
}
