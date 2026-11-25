"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  BookOpen,
  Brain,
  Target,
  ClipboardCheck,
  Search,
  Sun,
  Moon,
  MoreHorizontal,
  FileText,
  NotebookPen,
  Settings,
  LayoutGrid,
  LogOut,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { setAccessToken } from "@/lib/api-client";

interface MobileNavItem {
  href: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  testId: string;
  isActive: (pathname: string) => boolean;
}

const MOBILE_NAV_ITEMS: MobileNavItem[] = [
  {
    href: "/app",
    label: "Tổng quan",
    icon: LayoutDashboard,
    testId: "mobile-nav-app",
    isActive: (p) => p === "/app",
  },
  {
    href: "/app/courses",
    label: "Khóa học",
    icon: BookOpen,
    testId: "mobile-nav-courses",
    isActive: (p) => p.startsWith("/app/courses") && !p.includes("/quiz"),
  },
  {
    href: "/app/practice",
    label: "Lab",
    icon: Target,
    testId: "mobile-nav-practice",
    isActive: (p) => p.startsWith("/app/practice"),
  },
  {
    href: "/app/exams",
    label: "Giải đề",
    icon: ClipboardCheck,
    testId: "mobile-nav-exams",
    isActive: (p) => p.startsWith("/app/exams") || p.includes("/quiz"),
  },
  {
    href: "/app/review",
    label: "Ôn thẻ",
    icon: Brain,
    testId: "mobile-nav-review",
    isActive: (p) => p.startsWith("/app/review"),
  },
];

export function MobileTopBar() {
  const pathname = usePathname();
  const {
    darkMode,
    toggleDarkMode,
    setCommandPaletteOpen,
    setActiveUserHeaderId,
  } = useWorkspaceStore();
  const [moreOpen, setMoreOpen] = useState(false);

  useEffect(() => {
    setMoreOpen(false);
  }, [pathname]);

  useEffect(() => {
    if (!moreOpen) return;
    const handleClick = () => setMoreOpen(false);
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") setMoreOpen(false);
    };
    document.addEventListener("click", handleClick);
    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("click", handleClick);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [moreOpen]);

  const handleLogout = async () => {
    setMoreOpen(false);
    try {
      await fetch("/api/v1/auth/logout", {
        method: "POST",
        headers: getClientRequestHeaders(),
        credentials: "include",
      });
    } catch {
      // ignore logout network error
    }
    setAccessToken(null);
    if (typeof window !== "undefined") {
      window.localStorage.removeItem("swms_has_refresh_cookie");
      window.localStorage.removeItem("swms_active_user_id");
      window.localStorage.removeItem("swms_active_user");
    }
    setActiveUserHeaderId(null);
    window.location.href = "/auth";
  };

  return (
    <header
      data-testid="mobile-top-bar"
      className="md:hidden sticky top-0 z-30 h-12 px-3 border-b border-[var(--border-calm)] bg-[var(--surface-card)]/95 backdrop-blur-md flex items-center justify-between shrink-0"
    >
      <Link
        href="/app"
        className="flex items-center gap-2 min-w-0 group"
        title="Studxy — Tổng quan"
      >
        <span className="inline-flex items-center justify-center w-7 h-7 rounded-[8px] bg-[#5E51E8] dark:bg-[#818CF8] text-white dark:text-[#0F0F12] text-[11px] font-bold shrink-0 shadow-xs">
          St
        </span>
        <span className="text-sm font-bold text-[var(--text-primary)] tracking-tight truncate">
          Studxy
        </span>
      </Link>

      <div className="flex items-center gap-1 shrink-0 relative">
        <button
          type="button"
          onClick={() => setCommandPaletteOpen(true)}
          aria-label="Tìm kiếm nhanh"
          title="Tìm kiếm nhanh"
          className="w-9 h-9 flex items-center justify-center rounded-[8px] text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer"
        >
          <Search className="w-4 h-4" />
        </button>

        <button
          type="button"
          onClick={toggleDarkMode}
          aria-label={darkMode ? "Chuyển sang sáng" : "Chuyển sang tối"}
          title={darkMode ? "Chuyển sang sáng" : "Chuyển sang tối"}
          className="w-9 h-9 flex items-center justify-center rounded-[8px] text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer"
        >
          {darkMode ? (
            <Sun className="w-4 h-4 text-amber-400" />
          ) : (
            <Moon className="w-4 h-4" />
          )}
        </button>

        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            setMoreOpen((prev) => !prev);
          }}
          aria-label="Mở menu mở rộng"
          aria-expanded={moreOpen}
          aria-haspopup="true"
          className={`w-9 h-9 flex items-center justify-center rounded-[8px] transition cursor-pointer ${
            moreOpen
              ? "bg-[var(--surface-elevated)] text-[var(--text-primary)]"
              : "text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]"
          }`}
        >
          <MoreHorizontal className="w-4 h-4" />
        </button>

        {moreOpen && (
          <div
            onClick={(e) => e.stopPropagation()}
            className="absolute right-0 top-full mt-1.5 w-60 rounded-[12px] border border-[var(--border-calm)] bg-[var(--surface-card)] shadow-elevation-3 p-1.5 z-50 swms-animate-scale-in"
          >
            <div className="space-y-0.5">
              <Link
                href="/app/documents"
                onClick={() => setMoreOpen(false)}
                className={`flex items-center gap-2.5 px-2.5 min-h-[40px] rounded-[8px] text-sm transition ${
                  pathname.startsWith("/app/documents")
                    ? "bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:text-[#A5B4FC] font-medium"
                    : "text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]"
                }`}
              >
                <FileText className="w-4 h-4 shrink-0 text-[var(--text-secondary)]" />
                <span>Tài liệu</span>
              </Link>

              <Link
                href="/app/notes"
                onClick={() => setMoreOpen(false)}
                className={`flex items-center gap-2.5 px-2.5 min-h-[40px] rounded-[8px] text-sm transition ${
                  pathname.startsWith("/app/notes")
                    ? "bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:text-[#A5B4FC] font-medium"
                    : "text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]"
                }`}
              >
                <NotebookPen className="w-4 h-4 shrink-0 text-[var(--text-secondary)]" />
                <span>Sổ tay</span>
              </Link>

              <Link
                href="/app/settings"
                onClick={() => setMoreOpen(false)}
                className={`flex items-center gap-2.5 px-2.5 min-h-[40px] rounded-[8px] text-sm transition ${
                  pathname.startsWith("/app/settings")
                    ? "bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:text-[#A5B4FC] font-medium"
                    : "text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]"
                }`}
              >
                <Settings className="w-4 h-4 shrink-0 text-[var(--text-secondary)]" />
                <span>Cài đặt</span>
              </Link>

              <Link
                href="/workspace"
                onClick={() => setMoreOpen(false)}
                className="flex items-center gap-2.5 px-2.5 min-h-[40px] rounded-[8px] text-sm text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition"
              >
                <LayoutGrid className="w-4 h-4 shrink-0 text-[var(--text-secondary)]" />
                <span>Giao diện Classic Split</span>
              </Link>
            </div>

            <div className="my-1 border-t border-[var(--border-calm)]" />

            <button
              type="button"
              onClick={handleLogout}
              className="flex items-center gap-2.5 w-full px-2.5 min-h-[40px] rounded-[8px] text-sm text-rose-600 dark:text-rose-400 hover:bg-rose-50 dark:hover:bg-rose-950/30 transition cursor-pointer"
            >
              <LogOut className="w-4 h-4 shrink-0" />
              <span>Đăng xuất</span>
            </button>
          </div>
        )}
      </div>
    </header>
  );
}

export function MobileNav() {
  const pathname = usePathname();

  return (
    <nav
      data-testid="mobile-bottom-nav"
      className="md:hidden fixed bottom-0 left-0 right-0 z-50 h-14 bg-[var(--surface-card)]/95 backdrop-blur-md border-t border-[var(--border-calm)] grid grid-cols-5 items-center px-1 safe-area-bottom shadow-elevation-2"
    >
      {MOBILE_NAV_ITEMS.map((item) => {
        const Icon = item.icon;
        const active = item.isActive(pathname);
        return (
          <Link
            key={item.href}
            href={item.href}
            data-testid={item.testId}
            className={`flex flex-col items-center justify-center gap-0.5 min-w-0 min-h-[44px] h-full px-0.5 transition ${
              active
                ? "text-[var(--accent)] font-semibold"
                : "text-[var(--text-tertiary)] active:text-[var(--text-primary)]"
            }`}
          >
            <Icon className="w-4 h-4 shrink-0" />
            <span className="text-[11px] leading-tight whitespace-nowrap truncate max-w-full">
              {item.label}
            </span>
          </Link>
        );
      })}
    </nav>
  );
}
