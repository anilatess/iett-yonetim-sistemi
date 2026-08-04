import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/PublicComplaints";

export function createPublicComplaint(payload) {
  return apiFetch(API_URL, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}
