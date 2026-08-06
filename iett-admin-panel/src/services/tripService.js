import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/Trips";

export async function getTrips() {
  return apiFetch(API_URL);
}
