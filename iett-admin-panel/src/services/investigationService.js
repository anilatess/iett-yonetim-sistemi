import { API_URL } from "../config/apiConfig";
import { apiFetch } from "./apiClient";

export function getInvestigations() {
  return apiFetch(`${API_URL}/Investigations`);
}
