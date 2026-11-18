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

let bootstrapPromise: Promise<boolean> | null = null;

export function bootstrapAuthSession(): Promise<boolean> {
  if (bootstrapPromise) {
    return bootstrapPromise;
  }

  bootstrapPromise = (async () => {
    try {
      let token = getAccessToken();
      let hasStoredSession = false;
      if (typeof window !== "undefined") {
        try {
          hasStoredSession =
            window.localStorage.getItem("swms_has_refresh_cookie") === "1";
        } catch {
          // ignore localStorage read errors
        }
      }
      if (!token && hasStoredSession) {
        token = await refreshAccessTokenSingleFlight();
        if (!token && typeof window !== "undefined") {
          try {
            window.localStorage.removeItem("swms_has_refresh_cookie");
          } catch {
            // ignore localStorage write errors
          }
        }
      }

      const headers: Record<string, string> = {};
      if (token) {
        headers["Authorization"] = `Bearer ${token}`;
      }
      if (typeof window !== "undefined") {
        try {
          const activeUserId = window.localStorage.getItem("swms_active_user_id");
          if (activeUserId && activeUserId.trim()) {
            headers["X-Test-User-Id"] = activeUserId.trim();
          }
        } catch {
          // ignore localStorage read errors
        }
      }

      const resp = await fetch("/api/v1/auth/me", {
        method: "GET",
        credentials: "include",
        headers
      });

      if (resp.ok) {
        const user = await resp.json().catch(() => null);
        if (user && typeof window !== "undefined") {
          try {
            window.localStorage.setItem(
              "swms_active_user",
              JSON.stringify({
                id: String(user.id ?? user.Id ?? ""),
                fullName: String(user.fullName ?? user.FullName ?? ""),
                email: String(user.email ?? user.Email ?? ""),
                authProvider: String(user.authProvider ?? user.AuthProvider ?? "local"),
                avatarUrl: user.avatarUrl ?? user.AvatarUrl ?? null,
                hasLocalPassword: Boolean(user.hasLocalPassword ?? user.HasLocalPassword ?? true)
              })
            );
          } catch {
            // ignore localStorage write errors
          }
        }
        return true;
      }

      if (resp.status === 401) {
        return false;
      }
      return false;
    } catch {
      return false;
    } finally {
      bootstrapPromise = null;
    }
  })();

  return bootstrapPromise;
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
      if (!resp.ok) {
        if (resp.status === 401 && typeof window !== "undefined") {
          try {
            window.localStorage.removeItem("swms_has_refresh_cookie");
          } catch {
            // ignore localStorage write errors
          }
        }
        return null;
      }
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
