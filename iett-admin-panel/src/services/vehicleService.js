import { apiFetch } from "./apiClient";

const API_URL = "http://localhost:5147/api/Vehicles";

export async function getVehicles() {
  return apiFetch(API_URL);
}

export async function createVehicle(vehicle) {
  return apiFetch(API_URL, {
    method: "POST",
    body: JSON.stringify(vehicle),
  });
}

export async function updateVehicle(vehicle) {
  return apiFetch(API_URL, {
    method: "PUT",
    body: JSON.stringify(vehicle),
  });
}

export async function updateVehicleStatus(vehicleId, vehicleStatusId) {
  return apiFetch(`${API_URL}/${vehicleId}/status`, {
    method: "PATCH",
    body: JSON.stringify({ vehicleStatusId }),
  });
}

export async function deleteVehicle(id) {
  return apiFetch(`${API_URL}/${id}`, {
    method: "DELETE",
  });
}
