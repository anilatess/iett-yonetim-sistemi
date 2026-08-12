import { apiFetch } from "./apiClient";
import { API_URL as API_ROOT } from "../config/apiConfig";

const API_URL = `${API_ROOT}/Drivers`;

export function getDriverDashboard() {
  return apiFetch(`${API_URL}/me/dashboard`);
}

export async function getDrivers() {
  return apiFetch(API_URL);
}

export async function getMyTrips() {
  return apiFetch(`${API_URL}/me/trips`);
}

export async function getMyCertificates() {
  return apiFetch(`${API_URL}/me/certificates`);
}

export function uploadDriverCertificate(formData) {
  return apiFetch(`${API_URL}/me/certificates`, {
    method: "POST",
    body: formData,
  });
}

export async function getDriverCertificates(driverId) {
  return apiFetch(`${API_URL}/${driverId}/certificates`);
}

export async function getMyPerformances() {
  return apiFetch(`${API_URL}/me/performances`);
}

export async function getMyComplaints() {
  return apiFetch(`${API_URL}/me/complaints`);
}

export function submitComplaintExplanation(investigationId, explanation) {
  return apiFetch(`${API_URL}/me/investigations/${investigationId}/explanation`, {
    method: "POST",
    body: JSON.stringify({ explanation }),
  });
}
