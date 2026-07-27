const API_URL = "https://localhost:7034/api/Vehicles";

async function handleResponse(response) {
  if (response.ok) {
    if (response.status === 204) {
      return null;
    }

    return response.json();
  }

  const responseText = await response.text();

  let message = `İşlem başarısız oldu. HTTP ${response.status}`;

  if (responseText) {
    try {
      const errorData = JSON.parse(responseText);

      message =
        errorData.message ||
        errorData.title ||
        responseText;
    } catch {
      message = responseText;
    }
  }

  throw new Error(message);
}

export async function getVehicles() {
  const response = await fetch(API_URL);
  return handleResponse(response);
}

export async function createVehicle(vehicle) {
  const response = await fetch(API_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(vehicle),
  });

  return handleResponse(response);
}

export async function updateVehicle(vehicle) {
  const response = await fetch(API_URL, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(vehicle),
  });

  return handleResponse(response);
}

export async function deleteVehicle(id) {
  const response = await fetch(`${API_URL}/${id}`, {
    method: "DELETE",
  });

  return handleResponse(response);
}