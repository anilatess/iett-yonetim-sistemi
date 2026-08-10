import { apiFetch } from "./apiClient";
import { API_URL as API_ROOT } from "../config/apiConfig";

const API_URL = `${API_ROOT}/Trips`;

export async function getTrips() {
  return apiFetch(API_URL);
}
