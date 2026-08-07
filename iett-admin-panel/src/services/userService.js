import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/Users";

export function getUsers() {
  return apiFetch(API_URL);
}
