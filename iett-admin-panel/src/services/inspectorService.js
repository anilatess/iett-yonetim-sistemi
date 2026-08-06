import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/Inspectors";

export function getInspectorDashboard() {
  return apiFetch(`${API_URL}/me/dashboard`);
}

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

export const getInspectorDrivers = () => apiFetch(`${API_URL}/me/drivers`);
export const getInspectorTrips = () => apiFetch(`${API_URL}/me/trips`);

export function createInspectorTrip(payload) {
  return apiFetch(`${API_URL}/me/trips`, { method: "POST", body: JSON.stringify(payload) });
}

export function updateInspectorTrip(tripId, payload) {
  return apiFetch(`${API_URL}/me/trips/${tripId}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function cancelInspectorTrip(tripId) {
  return apiFetch(`${API_URL}/me/trips/${tripId}/cancel`, { method: "PUT" });
}

export const getInspectorCertificates = () =>
  apiFetch(`${API_URL}/me/certificates`);

export const approveInspectorCertificate = (certificateId) =>
  apiFetch(`${API_URL}/me/certificates/${certificateId}/approve`, {
    method: "PUT",
  });

export const rejectInspectorCertificate = (certificateId, payload) =>
  apiFetch(`${API_URL}/me/certificates/${certificateId}/reject`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
