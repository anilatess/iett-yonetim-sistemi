import { API_URL as API_ROOT } from "../config/apiConfig";

const API_URL = `${API_ROOT}/Auth`;

export async function login(loginData) {
  const response = await fetch(`${API_URL}/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(loginData),
  });

  const data = await response.json();

  if (!response.ok) {
    throw new Error(
      data.message || "Giriş işlemi başarısız.",
    );
  }

  return data;
}
