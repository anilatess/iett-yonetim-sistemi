import { apiFetch } from "./apiClient";
import { API_URL as API_ROOT } from "../config/apiConfig";

const API_URL = `${API_ROOT}/Users`;

export function getUsers() {
  return apiFetch(API_URL);
}
