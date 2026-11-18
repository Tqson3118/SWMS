"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  BookOpen,
  Brain,
  FileText,
  NotebookPen,
  Settings,
  Search,
  ChevronLeft,
  ChevronRight,
  Plus,
  LogOut,
  Moon,
  Sun,
  Flame,
  User,
  LayoutGrid,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { setAccessToken } from "@/lib/api-client";
import { deduplicateCourses, getCourseId, getCourseProgress } from "@/lib/course-helpers";

interface SidebarProps {
  collapsed: boolean;
  onToggleCollapse: () => void;
}

interface NavItem {
  href: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  badge?: number | string;
  matchPrefix?: boolean;
}

interface RecentCourse {
  id: string;
  title: string;
  progress: number;
}

const NAV_ITEMS: NavItem[] = [
  {
    href: "/app",
    label: "Hôm nay",
    icon: LayoutDashboard,
  },
  {
    href: "/app/courses",
    label: "Khóa học",
    icon: BookOpen,
    matchPrefix: true,
  },
  {
    href: "/app/documents",
    label: "Tài liệu",
    icon: FileText,
    matchPrefix: true,
  },
  {
    href: "/app/notes",
    label: "Sổ tay",
    icon: NotebookPen,
    matchPrefix: true,
  },
  {
    href: "/app/review",
    label: "Ôn tập",
    icon: Brain,
    matchPrefix: true,
  },
];

export function Sidebar({ collapsed, onToggleCollapse }: SidebarProps) {
  const pathname = usePathname();
  const {
    darkMode,
    toggleDarkMode,
    setCommandPaletteOpen,
    setActiveUserHeaderId,
    activeUserHeaderId,
  } = useWorkspaceStore();

  const [recentCourses, setRecentCourses] = useState<RecentCourse[]>([]);
  const [dueCount, setDueCount] = useState(0);
  const [streakDays, setStreakDays] = useState(0);
  const [userName, setUserName] = useState("Học viên");
  const [userEmail, setUserEmail] = useState("");
  const [profileOpen, setProfileOpen] = useState(false);

  // Fetch minimal sidebar data
  const fetchSidebarData = useCallback(async () => {
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined"
          ? window.localStorage.getItem("swms_active_user_id")
          : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );
      const resp = await fetch("/api/v1/workspace/dashboard", { headers });
      if (resp.ok) {
        const data = await resp.json();
        setDueCount(data.reviewStation?.totalDue ?? 0);
        setStreakDays(data.user?.streakDays ?? 0);
        setUserName(data.user?.fullName ?? "Học viên");
        setUserEmail(data.user?.email ?? "");

        // Extract recent courses (top 4, deduplicated)
        const rawCourses = Array.isArray(data.courses) ? data.courses : [];
        const uniqueCourses = deduplicateCourses(
          rawCourses as Record<string, unknown>[]
        );
        setRecentCourses(
          uniqueCourses.slice(0, 4).map((c) => ({
            id: getCourseId(c),
            title: String(c.title || "Khóa học"),
            progress: getCourseProgress(c),
          }))
        );
      }
    } catch {
      // Silent fail for sidebar data
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    fetchSidebarData();
  }, [fetchSidebarData, pathname]);

  useEffect(() => {
    if (typeof window === "undefined") return;
    const onMutated = () => {
      fetchSidebarData();
    };
    window.addEventListener("swms:data-mutated", onMutated);
    return () => window.removeEventListener("swms:data-mutated", onMutated);
  }, [fetchSidebarData]);

  const isActive = (item: NavItem) => {
    if (item.matchPrefix) return pathname.startsWith(item.href);
    return pathname === item.href;
  };

  const navItemsWithBadge = NAV_ITEMS.map((item) => {
    if (item.href === "/app/review" && dueCount > 0) {
      return { ...item, badge: dueCount };
    }
    return item;
  });

  // Close profile popover on outside click
  useEffect(() => {
    if (!profileOpen) return;
    const handleClick = () => setProfileOpen(false);
    document.addEventListener("click", handleClick);
    return () => document.removeEventListener("click", handleClick);
  }, [profileOpen]);

  const userInitials =
    userName
      .trim()
      .split(/\s+/)
      .map((part) => part[0]?.toUpperCase() ?? "")
      .join("")
      .slice(0, 2) || "HV";

  return (
    <aside
      data-testid="app-sidebar"
      className={`hidden md:flex flex-col h-screen border-r border-border bg-bg-tertiary shrink-0 transition-all duration-200 ${
        collapsed ? "w-[52px]" : "w-[240px]"
      }`}
    >
      {/* Brand header */}
      <div className="h-14 flex items-center justify-between px-3 border-b border-border-subtle">
        <Link
          href="/app"
          className="flex items-center gap-2.5 min-w-0 group"
          title="Studxy — Tổng quan"
        >
          <span className="inline-flex items-center justify-center w-7 h-7 rounded-md bg-accent text-white text-[11px] font-bold shrink-0 group-hover:scale-105 transition-transform">
            St
          </span>
          {!collapsed && (
            <span className="text-sm font-bold text-text-primary truncate">
              Studxy
            </span>
          )}
        </Link>

        <button
          type="button"
          onClick={onToggleCollapse}
          className="w-6 h-6 flex items-center justify-center rounded-md text-text-tertiary hover:text-text-primary hover:bg-bg-secondary transition cursor-pointer"
          title={collapsed ? "Mở rộng sidebar (Ctrl+\\)" : "Thu gọn sidebar (Ctrl+\\)"}
        >
          {collapsed ? (
            <ChevronRight className="w-3.5 h-3.5" />
          ) : (
            <ChevronLeft className="w-3.5 h-3.5" />
          )}
        </button>
      </div>

      {/* Search trigger */}
      {!collapsed && (
        <div className="px-3 pt-3 pb-1">
          <button
            type="button"
            onClick={() => setCommandPaletteOpen(true)}
            className="w-full h-8 flex items-center gap-2 px-2.5 rounded-md border border-border bg-bg-secondary text-text-tertiary text-xs hover:border-border hover:text-text-secondary transition cursor-pointer"
          >
            <Search className="w-3.5 h-3.5 shrink-0" />
            <span className="flex-1 text-left">Tìm kiếm...</span>
            <kbd className="text-[10px] font-mono px-1 py-0.5 rounded bg-bg-tertiary border border-border text-text-tertiary">
              ⌘K
            </kbd>
          </button>
        </div>
      )}

      {collapsed && (
        <div className="px-2 pt-3 pb-1">
          <button
            type="button"
            onClick={() => setCommandPaletteOpen(true)}
            className="w-8 h-8 flex items-center justify-center rounded-md text-text-tertiary hover:text-text-primary hover:bg-bg-secondary transition cursor-pointer mx-auto"
            title="Tìm kiếm (Ctrl+K)"
          >
            <Search className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Main navigation */}
      <nav className="flex-1 overflow-y-auto px-2 py-2 space-y-0.5">
        {navItemsWithBadge.map((item) => {
          const Icon = item.icon;
          const active = isActive(item);
          return (
            <Link
              key={item.href}
              href={item.href}
              data-testid={`sidebar-nav-${item.href.split("/").pop()}`}
              className={`flex items-center gap-2.5 h-8 rounded-md text-sm font-medium transition ${
                collapsed ? "justify-center px-0" : "px-2.5"
              } ${
                active
                  ? "bg-accent-subtle text-accent"
                  : "text-text-secondary hover:text-text-primary hover:bg-bg-secondary"
              }`}
              title={collapsed ? item.label : undefined}
            >
              <Icon className="w-4 h-4 shrink-0" />
              {!collapsed && (
                <>
                  <span className="flex-1 truncate">{item.label}</span>
                  {item.badge && (
                    <span className="inline-flex items-center justify-center min-w-[18px] h-[18px] rounded-md text-[10px] font-bold bg-accent text-white px-1">
                      {typeof item.badge === "number" && item.badge > 99
                        ? "99+"
                        : item.badge}
                    </span>
                  )}
                </>
              )}
              {collapsed && item.badge && (
                <span className="absolute -top-0.5 -right-0.5 w-2 h-2 rounded-full bg-accent" />
              )}
            </Link>
          );
        })}

        {/* Divider + Recent Courses */}
        {!collapsed && recentCourses.length > 0 && (
          <>
            <div className="pt-4 pb-1 px-2.5">
              <span className="text-[11px] font-semibold text-text-tertiary uppercase tracking-wider">
                Gần đây
              </span>
            </div>
            {recentCourses.map((course) => (
              <Link
                key={course.id}
                href={`/app/courses/${course.id}`}
                className={`flex items-center gap-2.5 h-8 px-2.5 rounded-md text-sm transition ${
                  pathname === `/app/courses/${course.id}`
                    ? "bg-accent-subtle text-accent font-medium"
                    : "text-text-secondary hover:text-text-primary hover:bg-bg-secondary"
                }`}
              >
                <BookOpen className="w-3.5 h-3.5 shrink-0 opacity-60" />
                <span className="flex-1 truncate text-[13px]">
                  {course.title}
                </span>
              </Link>
            ))}
          </>
        )}

        {/* Create course shortcut */}
        {!collapsed && (
          <div className="pt-2">
            <Link
              href="/app/courses/new"
              className="flex items-center gap-2.5 h-8 px-2.5 rounded-md text-sm text-text-tertiary hover:text-accent hover:bg-accent-subtle transition font-medium"
            >
              <Plus className="w-4 h-4 shrink-0" />
              <span>Tạo khóa học</span>
            </Link>
          </div>
        )}
      </nav>

      {/* Bottom section — Streak + Theme + Profile */}
      <div className="border-t border-border-subtle px-2 py-2 space-y-1">
        {/* Streak */}
        {!collapsed && streakDays > 0 && (
          <div className="flex items-center gap-2 px-2.5 h-7 text-xs text-text-secondary">
            <Flame className="w-3.5 h-3.5 text-amber-500" />
            <span className="font-medium">{streakDays} ngày liên tục</span>
          </div>
        )}

        {/* Theme toggle */}
        <button
          type="button"
          onClick={toggleDarkMode}
          className={`flex items-center gap-2.5 h-8 rounded-md text-sm text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition w-full cursor-pointer ${
            collapsed ? "justify-center px-0" : "px-2.5"
          }`}
          title={darkMode ? "Chuyển sang sáng" : "Chuyển sang tối"}
        >
          {darkMode ? (
            <Sun className="w-4 h-4 text-amber-400" />
          ) : (
            <Moon className="w-4 h-4" />
          )}
          {!collapsed && <span>{darkMode ? "Giao diện sáng" : "Giao diện tối"}</span>}
        </button>

        {/* Classic Workspace switcher */}
        <Link
          href="/workspace"
          data-testid="switch-to-workspace-link"
          className={`flex items-center gap-2.5 h-8 rounded-md text-sm text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition ${
            collapsed ? "justify-center px-0" : "px-2.5"
          }`}
          title={collapsed ? "Workspace Classic (/workspace)" : "Mở Workspace Classic (/workspace)"}
        >
          <LayoutGrid className="w-4 h-4 shrink-0" />
          {!collapsed && <span className="truncate">Workspace Classic</span>}
        </Link>

        {/* Footer Bottom: Settings & User Profile */}

        {/* Settings */}
        <Link
          href="/app/settings"
          className={`flex items-center gap-2.5 h-8 rounded-md text-sm text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition ${
            collapsed ? "justify-center px-0" : "px-2.5"
          } ${pathname === "/app/settings" ? "bg-accent-subtle text-accent" : ""}`}
          title={collapsed ? "Cài đặt" : undefined}
        >
          <Settings className="w-4 h-4 shrink-0" />
          {!collapsed && <span>Cài đặt</span>}
        </Link>

        {/* User profile */}
        <div className="relative">
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              setProfileOpen((prev) => !prev);
            }}
            className={`flex items-center gap-2.5 h-9 rounded-md text-sm text-text-secondary hover:bg-bg-secondary transition w-full cursor-pointer ${
              collapsed ? "justify-center px-0" : "px-2.5"
            }`}
            title={collapsed ? userName : undefined}
          >
            <div className="w-6 h-6 rounded-md bg-accent text-white text-[10px] font-bold flex items-center justify-center shrink-0">
              {userInitials}
            </div>
            {!collapsed && (
              <div className="flex-1 min-w-0 text-left">
                <div className="text-sm font-medium text-text-primary truncate">
                  {userName}
                </div>
              </div>
            )}
          </button>

          {/* Profile popover */}
          {profileOpen && !collapsed && (
            <div
              className="absolute bottom-full left-0 mb-1 w-full rounded-lg border border-border bg-bg-primary shadow-lg p-2 z-50"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="px-2 py-1.5 mb-1">
                <div className="text-sm font-medium text-text-primary">{userName}</div>
                <div className="text-xs text-text-tertiary">{userEmail}</div>
              </div>
              <div className="border-t border-border-subtle pt-1">
                <button
                  type="button"
                  onClick={async () => {
                    try {
                      await fetch("/api/v1/auth/logout", {
                        method: "POST",
                        headers: getClientRequestHeaders(),
                        credentials: "include",
                      });
                    } catch {}
                    setAccessToken(null);
                    if (typeof window !== "undefined") {
                      window.localStorage.removeItem("swms_has_refresh_cookie");
                      window.localStorage.removeItem("swms_active_user_id");
                      window.localStorage.removeItem("swms_active_user");
                    }
                    setActiveUserHeaderId(null);
                    window.location.href = "/auth";
                  }}
                  className="flex items-center gap-2 w-full px-2 py-1.5 rounded-md text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-950/30 transition cursor-pointer"
                >
                  <LogOut className="w-3.5 h-3.5" />
                  <span>Đăng xuất</span>
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </aside>
  );
}
