/**
 * BFF Cookie-Based Single-Flight Auth & API Client (Section 6.2, ADR-003 & ADR-016)
 */

let inMemoryAccessToken: string | null = null;

export function setAccessToken(token: string | null) {
  inMemoryAccessToken = token;
}

export function getAccessToken(): string | null {
  return inMemoryAccessToken;
}

export async function refreshAccessTokenSingleFlight(): Promise<string | null> {
  const doRefresh = async (): Promise<string | null> => {
    try {
      const resp = await fetch("/api/v1/auth/refresh", {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({})
      });
      if (!resp.ok) return null;
      const data = await resp.json();
      inMemoryAccessToken = data.accessToken ?? null;
      return inMemoryAccessToken;
    } catch {
      return null;
    }
  };

  if (typeof navigator !== "undefined" && "locks" in navigator) {
    return await navigator.locks.request("auth_token_refresh", doRefresh);
  }
  return await doRefresh();
}

export async function swmsFetch<T>(url: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers);
  if (!headers.has("Content-Type") && init?.body) {
    headers.set("Content-Type", "application/json");
  }
  if (inMemoryAccessToken) {
    headers.set("Authorization", `Bearer ${inMemoryAccessToken}`);
  }

  let resp = await fetch(url, {
    ...init,
    credentials: "include",
    headers
  });

  if (resp.status === 401) {
    const refreshed = await refreshAccessTokenSingleFlight();
    if (refreshed) {
      headers.set("Authorization", `Bearer ${refreshed}`);
      resp = await fetch(url, {
        ...init,
        credentials: "include",
        headers
      });
    }
  }

  if (!resp.ok) {
    const errText = await resp.text();
    throw new Error(errText || `HTTP ${resp.status}`);
  }

  return (await resp.json()) as T;
}
