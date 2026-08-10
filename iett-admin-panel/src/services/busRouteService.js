import { apiFetch } from "./apiClient";
import { API_URL as API_ROOT } from "../config/apiConfig";

const API_URL = `${API_ROOT}/BusRoutes`;

export async function getBusRoutes() {
  return apiFetch(API_URL);
}

export async function createBusRoute(busRoute) {
  return apiFetch(API_URL, {
    method: "POST",
    body: JSON.stringify(busRoute),
  });
}

export async function updateBusRoute(busRoute) {
  return apiFetch(API_URL, {
    method: "PUT",
    body: JSON.stringify(busRoute),
  });
}

export async function deleteBusRoute(id) {
  return apiFetch(`${API_URL}/${id}`, {
    method: "DELETE",
  });
}

export async function getBusRouteStops(routeId) {
  return apiFetch(
    `${API_URL}/${routeId}/stops`,
  );
}
