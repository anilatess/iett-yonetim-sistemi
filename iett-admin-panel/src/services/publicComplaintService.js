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
  const payload = {
    doorNumber,
    complaintTypeId,
    incidentDateTime,
    complaintDescription,
  };
  if (routeCode) payload.routeCode = routeCode;

  return apiFetch(API_URL, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function trackPublicComplaint(trackingCode) {
  return apiFetch(`${API_URL}/${encodeURIComponent(trackingCode)}`);
}
