/**
 * BFF Cookie-Based Single-Flight Auth & API Client (Section 6.2, ADR-003 & ADR-016)
 */

let inMemoryAccessToken: string | null = null;
let isPageUnloading = false;
let unloadListenerRegistered = false;

if (typeof window !== "undefined" && !unloadListenerRegistered) {
  unloadListenerRegistered = true;
  const markUnloading = () => {
    isPageUnloading = true;
  };
  window.addEventListener("beforeunload", markUnloading);
  window.addEventListener("pagehide", markUnloading);
}

export type AuthBootstrapStatus = "authenticated" | "unauthenticated" | "aborted";

export function setAccessToken(token: string | null) {
  inMemoryAccessToken = token;
}

export function getAccessToken(): string | null {
  return inMemoryAccessToken;
}

let bootstrapPromise: Promise<AuthBootstrapStatus> | null = null;

export function bootstrapAuthSessionDetailed(): Promise<AuthBootstrapStatus> {
  if (isPageUnloading) {
    return Promise.resolve("aborted");
  }
  if (bootstrapPromise) {
    return bootstrapPromise;
  }

  bootstrapPromise = (async (): Promise<AuthBootstrapStatus> => {
    try {
      if (isPageUnloading) return "aborted";
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
        if (isPageUnloading) return "aborted";
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

      if (isPageUnloading) return "aborted";

      if (resp.ok) {
        const user = await resp.json().catch(() => null);
        if (isPageUnloading) return "aborted";
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
        return "authenticated";
      }

      if (resp.status === 401 || resp.status === 403) {
        return "unauthenticated";
      }
      return "aborted";
    } catch {
      return "aborted";
    } finally {
      bootstrapPromise = null;
    }
  })();

  return bootstrapPromise;
}

export async function bootstrapAuthSession(): Promise<boolean> {
  const status = await bootstrapAuthSessionDetailed();
  return status === "authenticated";
}

export async function refreshAccessTokenSingleFlight(): Promise<string | null> {
  const doRefresh = async (): Promise<string | null> => {
    try {
      if (isPageUnloading) return null;
      const resp = await fetch("/api/v1/auth/refresh", {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({})
      });
      if (isPageUnloading) return null;
      if (!resp.ok) {
        if (resp.status === 401 && !isPageUnloading && typeof window !== "undefined") {
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

export async function apiFetch<T = unknown>(url: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers);
  if (typeof window !== "undefined" && !headers.has("X-Test-User-Id")) {
    try {
      const activeUserId = window.localStorage.getItem("swms_active_user_id");
      if (activeUserId && activeUserId.trim()) {
        headers.set("X-Test-User-Id", activeUserId.trim());
      }
    } catch {
      // ignore localStorage read errors
    }
  }
  return swmsFetch<T>(url, {
    ...init,
    headers,
  });
}

interface DashboardCacheEntry<T = unknown> {
  data: T;
  timestamp: number;
}

const DASHBOARD_CACHE_TTL_MS = 25_000;
const SESSION_SNAPSHOT_TTL_MS = 5 * 60_000;
const DASH_SNAPSHOT_PREFIX = "swms_dash_snap_";
const COURSES_SNAPSHOT_PREFIX = "swms_courses_snap_";

const dashboardCache = new Map<string, DashboardCacheEntry<any>>();
const dashboardInFlight = new Map<string, Promise<any>>();
const practiceStatsCache = new Map<string, DashboardCacheEntry<any>>();
const practiceStatsInFlight = new Map<string, Promise<any>>();

let clientCacheGeneration = 0;

function resolveActiveUserCacheKey(userId?: string | null): string {
  if (userId && userId.trim()) return userId.trim();
  if (typeof window !== "undefined") {
    try {
      const stored = window.localStorage.getItem("swms_active_user_id");
      if (stored && stored.trim()) return stored.trim();
    } catch {
      // ignore
    }
  }
  return "default_user";
}

function clearSessionSnapshots(userId?: string): void {
  if (typeof window === "undefined") return;
  try {
    const keysToRemove: string[] = [];
    for (let i = 0; i < window.sessionStorage.length; i++) {
      const k = window.sessionStorage.key(i);
      if (!k) continue;
      if (userId) {
        if (
          k.startsWith(`${DASH_SNAPSHOT_PREFIX}${userId}_`) ||
          k === `${COURSES_SNAPSHOT_PREFIX}${userId}`
        ) {
          keysToRemove.push(k);
        }
      } else if (k.startsWith(DASH_SNAPSHOT_PREFIX) || k.startsWith(COURSES_SNAPSHOT_PREFIX)) {
        keysToRemove.push(k);
      }
    }
    for (const k of keysToRemove) {
      window.sessionStorage.removeItem(k);
    }
  } catch {
    // ignore sessionStorage errors
  }
}

export function getCachedDashboardSnapshot<T = any>(options?: {
  compact?: boolean;
  userId?: string | null;
}): T | null {
  if (typeof window === "undefined") return null;
  const userKey = resolveActiveUserCacheKey(options?.userId);
  const mode = options?.compact ? "compact" : "full";
  const snapKey = `${DASH_SNAPSHOT_PREFIX}${userKey}_${mode}`;
  try {
    const raw = window.sessionStorage.getItem(snapKey);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as { ts?: number; data?: T };
    if (!parsed || typeof parsed.ts !== "number" || Date.now() - parsed.ts > SESSION_SNAPSHOT_TTL_MS) {
      window.sessionStorage.removeItem(snapKey);
      return null;
    }
    return parsed.data ?? null;
  } catch {
    return null;
  }
}

function writeDashboardSnapshot(userKey: string, compact: boolean, data: unknown): void {
  if (typeof window === "undefined") return;
  const mode = compact ? "compact" : "full";
  try {
    window.sessionStorage.setItem(
      `${DASH_SNAPSHOT_PREFIX}${userKey}_${mode}`,
      JSON.stringify({ ts: Date.now(), data })
    );
  } catch {
    // ignore quota errors
  }
}

export interface CoursesPagePayload<T = any> {
  items: T[];
  totalCount: number;
  offset: number;
  limit: number;
  hasMore: boolean;
}

export function getCachedCoursesSnapshot<T = any>(userId?: string | null): {
  items: T[];
  totalCount: number;
} | null {
  if (typeof window === "undefined") return null;
  const userKey = resolveActiveUserCacheKey(userId);
  const snapKey = `${COURSES_SNAPSHOT_PREFIX}${userKey}`;
  try {
    const raw = window.sessionStorage.getItem(snapKey);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as { ts?: number; items?: T[]; totalCount?: number };
    if (
      !parsed ||
      typeof parsed.ts !== "number" ||
      Date.now() - parsed.ts > SESSION_SNAPSHOT_TTL_MS ||
      !Array.isArray(parsed.items)
    ) {
      window.sessionStorage.removeItem(snapKey);
      return null;
    }
    return {
      items: parsed.items,
      totalCount: typeof parsed.totalCount === "number" ? parsed.totalCount : parsed.items.length,
    };
  } catch {
    return null;
  }
}

export function setCachedCoursesSnapshot<T = any>(
  items: T[],
  totalCount: number,
  userId?: string | null
): void {
  if (typeof window === "undefined") return;
  const userKey = resolveActiveUserCacheKey(userId);
  try {
    window.sessionStorage.setItem(
      `${COURSES_SNAPSHOT_PREFIX}${userKey}`,
      JSON.stringify({ ts: Date.now(), items, totalCount })
    );
  } catch {
    // ignore quota errors
  }
}

export async function fetchCoursesPage<T = any>(options?: {
  limit?: number;
  offset?: number;
  compact?: boolean;
  headers?: Record<string, string>;
}): Promise<CoursesPagePayload<T>> {
  const headers = buildDashboardHeaders(options?.headers);
  const limit = Math.max(1, options?.limit ?? 8);
  const offset = Math.max(0, options?.offset ?? 0);
  const compact = options?.compact ?? true;
  const url = `/api/v1/courses?limit=${limit}&offset=${offset}&compact=${compact ? "true" : "false"}`;

  let resp = await fetch(url, { credentials: "include", headers });
  if (resp.status === 401) {
    const refreshed = await refreshAccessTokenSingleFlight();
    if (refreshed) {
      headers["Authorization"] = `Bearer ${refreshed}`;
      resp = await fetch(url, { credentials: "include", headers });
    }
  }
  if (!resp.ok) {
    throw new Error(`HTTP ${resp.status}`);
  }
  const payload = await resp.json();
  if (Array.isArray(payload)) {
    const slice = payload.slice(offset, offset + limit);
    return {
      items: slice,
      totalCount: payload.length,
      offset,
      limit,
      hasMore: offset + slice.length < payload.length,
    };
  }
  const items = Array.isArray(payload?.items) ? (payload.items as T[]) : [];
  const totalCount =
    typeof payload?.totalCount === "number" ? payload.totalCount : offset + items.length;
  return {
    items,
    totalCount,
    offset: typeof payload?.offset === "number" ? payload.offset : offset,
    limit: typeof payload?.limit === "number" ? payload.limit : limit,
    hasMore:
      typeof payload?.hasMore === "boolean"
        ? payload.hasMore
        : offset + items.length < totalCount,
  };
}

export function invalidateDashboardCache(userId?: string): void {
  clientCacheGeneration++;
  clearSessionSnapshots(userId);
  if (userId) {
    for (const key of Array.from(dashboardCache.keys())) {
      if (key === userId || key.startsWith(`${userId}:`)) {
        dashboardCache.delete(key);
      }
    }
    for (const key of Array.from(dashboardInFlight.keys())) {
      if (key === userId || key.startsWith(`${userId}:`)) {
        dashboardInFlight.delete(key);
      }
    }
    practiceStatsCache.delete(userId);
    practiceStatsInFlight.delete(userId);
  } else {
    dashboardCache.clear();
    practiceStatsCache.clear();
    dashboardInFlight.clear();
    practiceStatsInFlight.clear();
  }
}

if (typeof window !== "undefined") {
  window.addEventListener("swms:data-mutated", () => {
    invalidateDashboardCache();
  });
}

function buildDashboardHeaders(extraHeaders?: Record<string, string>): Record<string, string> {
  const headers: Record<string, string> = { ...(extraHeaders ?? {}) };
  if (inMemoryAccessToken && !headers["Authorization"]) {
    headers["Authorization"] = `Bearer ${inMemoryAccessToken}`;
  }
  if (typeof window !== "undefined") {
    try {
      const activeUserId = window.localStorage.getItem("swms_active_user_id");
      if (activeUserId && activeUserId.trim() && !headers["X-Test-User-Id"]) {
        headers["X-Test-User-Id"] = activeUserId.trim();
      }
    } catch {
      // ignore
    }
  }
  return headers;
}

export async function fetchDashboardCached<T = any>(options?: {
  forceRefresh?: boolean;
  compact?: boolean;
  headers?: Record<string, string>;
}): Promise<T> {
  const headers = buildDashboardHeaders(options?.headers);
  const userKey = headers["X-Test-User-Id"] || "default_user";
  const isCompact = Boolean(options?.compact);
  const cacheKey = `${userKey}:${isCompact ? "compact" : "full"}`;
  const endpointUrl = isCompact
    ? "/api/v1/workspace/dashboard?compact=true"
    : "/api/v1/workspace/dashboard";

  if (!options?.forceRefresh) {
    const cached = dashboardCache.get(cacheKey);
    if (cached && Date.now() - cached.timestamp < DASHBOARD_CACHE_TTL_MS) {
      return cached.data as T;
    }
    const inFlight = dashboardInFlight.get(cacheKey);
    if (inFlight) {
      return inFlight as Promise<T>;
    }
  }

  const reqGen = clientCacheGeneration;
  const fetchPromise = (async () => {
    try {
      let resp = await fetch(endpointUrl, {
        credentials: "include",
        headers
      });

      if (resp.status === 401) {
        const refreshed = await refreshAccessTokenSingleFlight();
        if (refreshed) {
          headers["Authorization"] = `Bearer ${refreshed}`;
          resp = await fetch(endpointUrl, {
            credentials: "include",
            headers
          });
        }
      }

      if (!resp.ok) {
        throw new Error(`HTTP ${resp.status}`);
      }
      const data = await resp.json();
      if (reqGen === clientCacheGeneration) {
        dashboardCache.set(cacheKey, { data, timestamp: Date.now() });
        writeDashboardSnapshot(userKey, isCompact, data);
      }
      return data as T;
    } finally {
      dashboardInFlight.delete(cacheKey);
    }
  })();

  dashboardInFlight.set(cacheKey, fetchPromise);
  return fetchPromise;
}

export async function fetchPracticeStatsCached<T = any>(options?: {
  forceRefresh?: boolean;
  headers?: Record<string, string>;
}): Promise<T | null> {
  const headers = buildDashboardHeaders(options?.headers);
  const cacheKey = headers["X-Test-User-Id"] || "default_user";

  if (!options?.forceRefresh) {
    const cached = practiceStatsCache.get(cacheKey);
    if (cached && Date.now() - cached.timestamp < DASHBOARD_CACHE_TTL_MS) {
      return cached.data as T;
    }
    const inFlight = practiceStatsInFlight.get(cacheKey);
    if (inFlight) {
      return inFlight as Promise<T | null>;
    }
  }

  const reqGen = clientCacheGeneration;
  const fetchPromise = (async () => {
    try {
      let resp = await fetch("/api/v1/practice/stats", {
        credentials: "include",
        headers
      });

      if (resp.status === 401) {
        const refreshed = await refreshAccessTokenSingleFlight();
        if (refreshed) {
          headers["Authorization"] = `Bearer ${refreshed}`;
          resp = await fetch("/api/v1/practice/stats", {
            credentials: "include",
            headers
          });
        }
      }

      if (!resp.ok) {
        return null;
      }
      const data = await resp.json().catch(() => null);
      if (data && reqGen === clientCacheGeneration) {
        practiceStatsCache.set(cacheKey, { data, timestamp: Date.now() });
      }
      return data as T | null;
    } catch {
      return null;
    } finally {
      practiceStatsInFlight.delete(cacheKey);
    }
  })();

  practiceStatsInFlight.set(cacheKey, fetchPromise);
  return fetchPromise;
}

