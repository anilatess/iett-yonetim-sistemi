import { apiFetch } from "./apiClient";

const API_URL = "https://localhost:7034/api/Drivers";

export async function getDrivers() {
  return apiFetch(API_URL);
}

export async function getMyTrips() {
  return apiFetch(`${API_URL}/me/trips`);
}

export async function getMyCertificates() {
  return apiFetch(`${API_URL}/me/certificates`);
}
