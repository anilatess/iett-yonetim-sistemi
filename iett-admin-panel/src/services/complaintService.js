import { apiFetch } from "./apiClient";
import { API_URL as API_ROOT } from "../config/apiConfig";

const API_URL = `${API_ROOT}/Complaints`;

export function getComplaints() {
  return apiFetch(API_URL);
}

export function getComplaint(id) {
  return apiFetch(`${API_URL}/${id}`);
}
