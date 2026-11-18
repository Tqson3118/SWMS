"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { BookOpen, Sun, Moon, ArrowRight, Keyboard, Compass } from "lucide-react";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";

interface LandingHeaderProps {
  onOpenShortcuts?: () => void;
  onOpenAbout?: () => void;
}

export function LandingHeader({ onOpenShortcuts, onOpenAbout }: LandingHeaderProps) {
  const { darkMode, toggleDarkMode } = useWorkspaceStore();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  const handleToggleTheme = () => {
    toggleDarkMode();
    if (typeof document !== "undefined") {
      const isDark = !darkMode;
      if (isDark) {
        document.documentElement.classList.add("dark");
        document.body?.classList.add("dark");
      } else {
        document.documentElement.classList.remove("dark");
        document.body?.classList.remove("dark");
      }
      try {
        localStorage.setItem("swms_theme", isDark ? "dark" : "light");
      } catch {
        // Ignore storage exceptions
      }
    }
  };

  return (
    <header
      data-testid="landing-header"
      className="sticky top-0 z-40 w-full h-16 bg-white/80 dark:bg-slate-950/80 backdrop-blur-md border-b border-slate-200/80 dark:border-slate-800 transition-colors"
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-full flex items-center justify-between">
        {/* Brand identity */}
        <Link href="/" className="flex items-center gap-2.5 group cursor-pointer">
          <div className="w-9 h-9 rounded-xl bg-indigo-600 text-white flex items-center justify-center shadow-xs group-hover:scale-105 transition-transform">
            <BookOpen className="w-5 h-5 stroke-[2]" />
          </div>
          <div className="flex flex-col">
            <div className="flex items-center gap-1.5">
              <span className="font-bold text-base tracking-tight text-slate-900 dark:text-white">
                SWMS
              </span>
              <span className="text-xs text-indigo-600 dark:text-indigo-400 font-medium">
                Studxy
              </span>
              <span className="hidden sm:inline-flex px-1.5 py-0.2 text-[10px] font-medium tracking-wide rounded-full bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 border border-indigo-200/60 dark:border-indigo-800/60">
                v1.0 Clean
              </span>
            </div>
            <span className="text-[10px] text-slate-500 dark:text-slate-400 hidden sm:inline">
              Sovereign Personal Workspace
            </span>
          </div>
        </Link>

        {/* Navigation links (Desktop) */}
        <nav className="hidden md:flex items-center gap-6 text-sm font-medium text-slate-600 dark:text-slate-300">
          {onOpenAbout && (
            <button
              type="button"
              onClick={onOpenAbout}
              data-testid="about-swms-trigger"
              className="inline-flex items-center gap-1 hover:text-slate-900 dark:hover:text-white transition-colors cursor-pointer"
            >
              <Compass className="w-3.5 h-3.5 text-slate-400" />
              <span>Về SWMS</span>
            </button>
          )}
          <a
            href="#features"
            className="hover:text-slate-900 dark:hover:text-white transition-colors"
          >
            Tính năng
          </a>
          <a
            href="#comparison"
            className="hover:text-slate-900 dark:hover:text-white transition-colors"
          >
            So sánh
          </a>
          <a
            href="#workflow"
            className="hover:text-slate-900 dark:hover:text-white transition-colors"
          >
            Quy trình
          </a>
          <a
            href="#philosophy"
            className="hover:text-slate-900 dark:hover:text-white transition-colors"
          >
            Tư tưởng
          </a>

          {onOpenShortcuts && (
            <button
              onClick={onOpenShortcuts}
              data-testid="keyboard-shortcuts-trigger"
              className="inline-flex items-center gap-1.5 hover:text-slate-900 dark:hover:text-white transition-colors cursor-pointer"
            >
              <Keyboard className="w-3.5 h-3.5 text-slate-400" />
              <span>Phím tắt</span>
              <kbd className="swms-kbd text-[10px]">
                ?
              </kbd>
            </button>
          )}
        </nav>

        {/* Actions (Right) */}
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={handleToggleTheme}
            data-testid="theme-toggle-btn"
            aria-label="Chuyển đổi giao diện Sáng / Tối"
            className="p-2 rounded-lg text-slate-600 dark:text-slate-300 hover:bg-slate-200/50 dark:hover:bg-slate-800/50 transition-colors cursor-pointer"
          >
            {mounted && darkMode ? (
              <Sun className="w-4 h-4 text-amber-400 stroke-[2]" />
            ) : (
              <Moon className="w-4 h-4 text-slate-500 stroke-[2]" />
            )}
          </button>

          <Link
            href="/app"
            data-testid="landing-nav-cta"
            className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-medium rounded-lg bg-indigo-600 hover:bg-indigo-700 active:scale-[0.98] text-white shadow-xs transition-all cursor-pointer"
          >
            <span>Vào App</span>
            <ArrowRight className="w-3.5 h-3.5 stroke-[2]" />
          </Link>
        </div>
      </div>
    </header>
  );
}
