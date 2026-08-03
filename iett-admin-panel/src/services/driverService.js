import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/Drivers";

export async function getDrivers() {
  return apiFetch(API_URL);
}

export async function getMyTrips() {
  return apiFetch(`${API_URL}/me/trips`);
}

export async function getMyCertificates() {
  return apiFetch(`${API_URL}/me/certificates`);
}

export async function getMyPerformances() {
  return apiFetch(`${API_URL}/me/performances`);
}
