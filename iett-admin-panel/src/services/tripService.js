import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/Trips";

export async function getTrips() {
  return apiFetch(API_URL);
}

export async function createTrip(payload) {
  return apiFetch(API_URL, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}
