"use client";

import React, { useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { Sidebar } from "@/components/navigation/Sidebar";
import { MobileNav } from "@/components/navigation/MobileNav";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { CommandPaletteModal } from "@/components/command/CommandPaletteModal";

export default function AppShellLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const { darkMode, toggleDarkMode, setCommandPaletteOpen, commandPaletteOpen } =
    useWorkspaceStore();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const pathname = usePathname();

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

      // Ctrl+K — Command palette
      if ((e.ctrlKey || e.metaKey) && e.key === "k") {
        e.preventDefault();
        setCommandPaletteOpen(!commandPaletteOpen);
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
  }, [commandPaletteOpen, setCommandPaletteOpen, toggleDarkMode]);

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
