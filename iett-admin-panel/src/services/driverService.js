import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/Drivers";

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
