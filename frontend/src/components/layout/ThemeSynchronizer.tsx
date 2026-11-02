"use client";

import { useEffect } from "react";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";

/**
 * ThemeSynchronizer synchronizes the dark mode state between:
 * 1. Zustand store (useWorkspaceStore.darkMode)
 * 2. Root DOM element (document.documentElement and document.body classList 'dark')
 * 3. Browser localStorage ('swms_theme')
 * 4. Cross-tab storage events
 */
export function ThemeSynchronizer() {
  const darkMode = useWorkspaceStore((s) => s.darkMode);

  // Sync Store -> DOM & localStorage
  useEffect(() => {
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

  // Initial sync & Cross-tab listener
  useEffect(() => {
    if (typeof window === "undefined") return;

    try {
      const stored = localStorage.getItem("swms_theme");
      if (stored === "dark") {
        if (!useWorkspaceStore.getState().darkMode) {
          useWorkspaceStore.setState({ darkMode: true });
        }
        document.documentElement.classList.add("dark");
        document.body?.classList.add("dark");
      } else if (stored === "light") {
        if (useWorkspaceStore.getState().darkMode) {
          useWorkspaceStore.setState({ darkMode: false });
        }
        document.documentElement.classList.remove("dark");
        document.body?.classList.remove("dark");
      }
    } catch {
      // Ignore storage exceptions
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

  return null;
}
