"use client";

import React, { useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { Sidebar } from "@/components/navigation/Sidebar";
import { MobileNav, MobileTopBar } from "@/components/navigation/MobileNav";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { CommandPaletteModal } from "@/components/command/CommandPaletteModal";
import { KeyboardShortcutsModal } from "@/components/auxiliary/KeyboardShortcutsModal";
import { bootstrapAuthSessionDetailed, setAccessToken } from "@/lib/api-client";

export default function AppShellLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const { darkMode, toggleDarkMode, setActiveUserHeaderId } = useWorkspaceStore();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [authReady, setAuthReady] = useState(false);
  const [shortcutsOpen, setShortcutsOpen] = useState(false);
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    let mounted = true;
    bootstrapAuthSessionDetailed().then((status) => {
      if (!mounted || status === "aborted") return;
      if (status === "authenticated") {
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

  // Determine if we're in an immersive view (learn/reader mode, review, quiz, or full-screen practice)
  const isImmersive =
    pathname.includes("/learn") ||
    pathname.includes("/review") ||
    pathname.includes("/quiz") ||
    /^\/app\/practice\/[^/]+\/[^/]+/.test(pathname);

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

      // ? — Toggle Keyboard Shortcuts Modal
      if (!e.ctrlKey && !e.metaKey && !e.altKey && e.key === "?") {
        e.preventDefault();
        setShortcutsOpen((prev) => !prev);
        return;
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [toggleDarkMode]);

  useEffect(() => {
    const handleOpenShortcuts = () => setShortcutsOpen(true);
    window.addEventListener("studxy:open-shortcuts", handleOpenShortcuts);
    return () => window.removeEventListener("studxy:open-shortcuts", handleOpenShortcuts);
  }, []);

  if (!authReady) {
    return (
      <div className={darkMode ? "dark" : ""}>
        <div className="h-screen w-full overflow-hidden bg-[var(--canvas-bg)] text-[var(--text-primary)] flex">
          {/* Layout preserving skeleton to keep CLS < 0.1 */}
          {!isImmersive && (
            <div className="hidden md:flex flex-col h-screen w-[240px] border-r border-[var(--border-calm)] bg-[var(--surface-card)] p-3 space-y-4 shrink-0">
              <div className="flex items-center gap-2.5 h-10 border-b border-[var(--border-calm)] pb-3">
                <div className="w-7 h-7 rounded-[8px] swms-skeleton" />
                <div className="w-20 h-4 rounded-[6px] swms-skeleton" />
              </div>
              <div className="space-y-2 pt-2">
                {[...Array(5)].map((_, i) => (
                  <div key={i} className="h-8.5 rounded-[8px] swms-skeleton" />
                ))}
              </div>
            </div>
          )}
          <main className="flex-1 min-w-0 h-screen p-6 overflow-hidden">
            <div className="max-w-5xl mx-auto space-y-6">
              <div className="h-8 w-48 rounded-[8px] swms-skeleton" />
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {[...Array(3)].map((_, i) => (
                  <div key={i} className="h-28 rounded-[14px] swms-skeleton" />
                ))}
              </div>
              <div className="h-64 rounded-[14px] swms-skeleton" />
            </div>
          </main>
        </div>
      </div>
    );
  }

  return (
    <div className={darkMode ? "dark" : ""}>
      <div className="h-screen w-full overflow-hidden bg-[var(--canvas-bg)] text-[var(--text-primary)] flex">
        {/* Sidebar — hidden in immersive mode on desktop, always available via toggle */}
        {!isImmersive && (
          <Sidebar
            collapsed={sidebarCollapsed}
            onToggleCollapse={() => setSidebarCollapsed((prev) => !prev)}
          />
        )}

        {/* Right column: mobile utility bar + smooth scrollable main area */}
        <div className="flex-1 min-w-0 h-screen flex flex-col overflow-hidden">
          {!isImmersive && <MobileTopBar />}
          <main
            className={`flex-1 min-w-0 overflow-y-auto overflow-x-clip transition-all duration-200 ${
              isImmersive ? "w-full" : ""
            }`}
          >
            {children}
          </main>
        </div>
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

      {/* Keyboard shortcuts guide (global) */}
      <KeyboardShortcutsModal
        isOpen={shortcutsOpen}
        onClose={() => setShortcutsOpen(false)}
      />
    </div>
  );
}
