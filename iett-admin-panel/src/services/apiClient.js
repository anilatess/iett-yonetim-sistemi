export async function apiFetch(url, options = {}) {
  const token = localStorage.getItem("token");
  const headers = new Headers(options.headers || {});
  const hasBody = options.body !== undefined && options.body !== null;

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  if (hasBody && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(url, {
    ...options,
    headers,
  });

  if (response.status === 401) {
    localStorage.removeItem("currentUser");
    localStorage.removeItem("token");
    localStorage.removeItem("activePage");
    localStorage.removeItem("selectedRoute");

    window.location.reload();
    throw new Error("Oturum süreniz doldu. Lütfen tekrar giriş yapın.");
  }

  if (response.status === 204) {
    return null;
  }

  const responseText = await response.text();
  let data = null;

  if (responseText) {
    try {
      data = JSON.parse(responseText);
    } catch {
      data = responseText;
    }
  }

  if (!response.ok) {
    const message =
      (typeof data === "object" &&
        data !== null &&
        (data.message || data.title)) ||
      (typeof data === "string" && data) ||
      `İşlem başarısız oldu. HTTP ${response.status}`;

    throw new Error(message);
  }

  return data;
}
