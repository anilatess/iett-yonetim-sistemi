import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/Complaints";

export function getComplaints() {
  return apiFetch(API_URL);
}

export function getComplaint(id) {
  return apiFetch(`${API_URL}/${id}`);
}
