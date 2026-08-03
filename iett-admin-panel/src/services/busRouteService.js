import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/BusRoutes";

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
