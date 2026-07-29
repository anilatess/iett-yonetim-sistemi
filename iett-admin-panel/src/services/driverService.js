const API_URL = "https://localhost:7034/api/Drivers";

export async function getDrivers() {
  const response = await fetch(API_URL);

  if (!response.ok) {
    throw new Error("Şoförler getirilemedi.");
  }

  return response.json();
}