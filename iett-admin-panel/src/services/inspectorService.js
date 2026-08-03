import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/Inspectors";

export async function createDriverPerformance(performance) {
  return apiFetch(`${API_URL}/me/performances`, {
    method: "POST",
    body: JSON.stringify(performance),
  });
}

export async function getMyPerformanceHistory() {
  return apiFetch(`${API_URL}/me/performances`);
}

export async function getMyInvestigations() {
  return apiFetch(`${API_URL}/me/investigations`);
}

export async function completeInvestigation(id, investigationResult) {
  return apiFetch(`${API_URL}/me/investigations/${id}/complete`, {
    method: "PUT",
    body: JSON.stringify({ investigationResult }),
  });
}
