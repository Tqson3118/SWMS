"use client";

import { useEffect, useRef } from "react";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";

/**
 * ThemeSynchronizer is the SINGLE SOURCE OF TRUTH for synchronizing theme between:
 * 1. Zustand store (useWorkspaceStore.darkMode)
 * 2. Root DOM element (document.documentElement and document.body classList 'dark')
 * 3. Browser localStorage ('swms_theme')
 * 4. Cross-tab storage events
 *
 * It prevents FOUC (Flash of Unstyled Content), eliminates race conditions on hydration,
 * and ensures seamless zero-layout-shift theme toggling.
 */
export function ThemeSynchronizer() {
  const darkMode = useWorkspaceStore((s) => s.darkMode);
  const isInitializedRef = useRef(false);
  const isMountedRef = useRef(false);

  // 1. Initial Mount Sync & Cross-Tab Storage Listener
  useEffect(() => {
    if (typeof window === "undefined" || typeof document === "undefined") return;

    try {
      const stored = localStorage.getItem("swms_theme");
      const root = document.documentElement;
      const body = document.body;

      if (stored === "dark") {
        if (!useWorkspaceStore.getState().darkMode) {
          useWorkspaceStore.setState({ darkMode: true });
        }
        root.classList.add("dark");
        body?.classList.add("dark");
      } else if (stored === "light") {
        if (useWorkspaceStore.getState().darkMode) {
          useWorkspaceStore.setState({ darkMode: false });
        }
        root.classList.remove("dark");
        body?.classList.remove("dark");
      } else {
        // Fallback: check DOM class from layout.tsx inline script or system preference
        const hasDarkClass = root.classList.contains("dark");
        const systemPrefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
        const initialDark = hasDarkClass || systemPrefersDark;

        if (useWorkspaceStore.getState().darkMode !== initialDark) {
          useWorkspaceStore.setState({ darkMode: initialDark });
        }
        if (initialDark) {
          root.classList.add("dark");
          body?.classList.add("dark");
          localStorage.setItem("swms_theme", "dark");
        } else {
          root.classList.remove("dark");
          body?.classList.remove("dark");
          localStorage.setItem("swms_theme", "light");
        }
      }
    } catch {
      // Ignore storage exceptions
    } finally {
      isInitializedRef.current = true;
    }

    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === "swms_theme" && e.newValue) {
        const isDark = e.newValue === "dark";
        useWorkspaceStore.setState({ darkMode: isDark });
        if (isDark) {
          document.documentElement.classList.add("dark");
          document.body?.classList.add("dark");
        } else {
          document.documentElement.classList.remove("dark");
          document.body?.classList.remove("dark");
        }
      }
    };

    window.addEventListener("storage", handleStorageChange);
    return () => window.removeEventListener("storage", handleStorageChange);
  }, []);

  // 2. React to user toggling darkMode in store (skip initial mount to prevent race condition)
  useEffect(() => {
    if (!isMountedRef.current) {
      isMountedRef.current = true;
      return;
    }
    if (typeof document === "undefined") return;

    const root = document.documentElement;
    const body = document.body;

    if (darkMode) {
      root.classList.add("dark");
      body?.classList.add("dark");
      try {
        localStorage.setItem("swms_theme", "dark");
      } catch {
        // Ignore storage exceptions
      }
    } else {
      root.classList.remove("dark");
      body?.classList.remove("dark");
      try {
        localStorage.setItem("swms_theme", "light");
      } catch {
        // Ignore storage exceptions
      }
    }
  }, [darkMode]);

  return null;
}
