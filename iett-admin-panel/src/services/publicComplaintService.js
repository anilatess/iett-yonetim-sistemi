import { apiFetch } from "./apiClient";
import { API_URL as API_ROOT } from "../config/apiConfig";

const API_URL = `${API_ROOT}/PublicComplaints`;

export function getPublicComplaintTypes() {
  return apiFetch(`${API_URL}/types`);
}

export function createPublicComplaint({
  doorNumber,
  routeCode,
  complaintTypeId,
  incidentDateTime,
  complaintDescription,
}) {
  return apiFetch(API_URL, {
    method: "POST",
    body: JSON.stringify({
      doorNumber,
      routeCode,
      complaintTypeId,
      incidentDateTime,
      complaintDescription,
    }),
  });
}
