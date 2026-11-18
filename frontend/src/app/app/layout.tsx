"use client";

import React, { useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { Sidebar } from "@/components/navigation/Sidebar";
import { MobileNav } from "@/components/navigation/MobileNav";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { CommandPaletteModal } from "@/components/command/CommandPaletteModal";
import { bootstrapAuthSession, setAccessToken } from "@/lib/api-client";

export default function AppShellLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const { darkMode, toggleDarkMode, setActiveUserHeaderId } = useWorkspaceStore();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [authReady, setAuthReady] = useState(false);
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    let mounted = true;
    bootstrapAuthSession().then((ok) => {
      if (!mounted) return;
      if (ok) {
        setAuthReady(true);
      } else {
        try {
          window.localStorage.removeItem("swms_has_refresh_cookie");
          window.localStorage.removeItem("swms_active_user");
          window.localStorage.removeItem("swms_active_user_id");
        } catch {
          // ignore
        }
        setAccessToken(null);
        setActiveUserHeaderId(null);
        router.replace(`/auth?redirect=${encodeURIComponent(pathname || "/app")}`);
      }
    });
    return () => {
      mounted = false;
    };
  }, [pathname, router, setActiveUserHeaderId]);

  // Determine if we're in an immersive view (learn/reader mode)
  const isImmersive =
    pathname.includes("/learn") || pathname.includes("/review");

  // Global keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement | null;
      if (
        target &&
        (target.tagName === "INPUT" ||
          target.tagName === "TEXTAREA" ||
          target.tagName === "SELECT" ||
          target.isContentEditable)
      ) {
        return;
      }

      // Ctrl+D — Toggle dark mode
      if ((e.ctrlKey || e.metaKey) && !e.altKey && (e.key === "d" || e.key === "D")) {
        e.preventDefault();
        toggleDarkMode();
        return;
      }

      // Ctrl+\ — Toggle sidebar
      if ((e.ctrlKey || e.metaKey) && e.key === "\\") {
        e.preventDefault();
        setSidebarCollapsed((prev) => !prev);
        return;
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [toggleDarkMode]);

  if (!authReady) {
    return (
      <div className={darkMode ? "dark" : ""}>
        <div className="h-screen w-screen flex items-center justify-center bg-bg-primary text-text-primary">
          <div className="flex flex-col items-center gap-3">
            <div className="w-7 h-7 border-2 border-accent border-t-transparent rounded-full animate-spin" />
            <span className="text-xs text-text-secondary">Đang xác thực phiên học tập...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={darkMode ? "dark" : ""}>
      <div className="h-screen w-screen overflow-hidden bg-bg-primary text-text-primary flex">
        {/* Sidebar — hidden in immersive mode on desktop, always available via toggle */}
        {!isImmersive && (
          <Sidebar
            collapsed={sidebarCollapsed}
            onToggleCollapse={() => setSidebarCollapsed((prev) => !prev)}
          />
        )}

        {/* Main content area */}
        <main
          className={`flex-1 min-w-0 h-screen overflow-y-auto overflow-x-clip transition-all duration-200 ${
            isImmersive ? "w-full" : ""
          }`}
        >
          {children}
        </main>
      </div>

      {/* Mobile bottom navigation — hidden in immersive mode */}
      {!isImmersive && <MobileNav />}

      {/* Command palette (global) */}
      <CommandPaletteModal
        onQuickCaptureCreated={() => {
          if (typeof window !== "undefined") {
            window.dispatchEvent(new Event("swms:data-mutated"));
          }
        }}
      />
    </div>
  );
}
