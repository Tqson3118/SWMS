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
  LayoutGrid,
  Target,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { setAccessToken, fetchDashboardCached } from "@/lib/api-client";
import {
  deduplicateCourses,
  getCourseId,
  getCourseProgress,
  sortCoursesSmartly,
} from "@/lib/course-helpers";

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
    href: "/app/practice",
    label: "Luyện tập",
    icon: Target,
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
  const [loadingCourses, setLoadingCourses] = useState(true);
  const [dueCount, setDueCount] = useState(0);
  const [practiceReviewCount, setPracticeReviewCount] = useState(0);
  const [streakDays, setStreakDays] = useState(0);
  const [userName, setUserName] = useState("Học viên");
  const [userEmail, setUserEmail] = useState("");
  const [profileOpen, setProfileOpen] = useState(false);

  // Fetch minimal sidebar data
  const fetchSidebarData = useCallback(async () => {
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const reqHeaders = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );

      const [data, practiceStatsResp] = await Promise.all([
        fetchDashboardCached({
          headers: effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined,
        }),
        fetch("/api/v1/practice/stats", { headers: reqHeaders }).catch(() => null),
      ]);

      if (data) {
        setDueCount(data.reviewStation?.totalDue ?? 0);
        setStreakDays(data.user?.streakDays ?? 0);
        setUserName(data.user?.fullName ?? "Học viên");
        setUserEmail(data.user?.email ?? "");

        // Extract recent courses (top 4, deduplicated & smartly sorted)
        const rawCourses = Array.isArray(data.courses) ? data.courses : [];
        const uniqueCourses = sortCoursesSmartly(
          deduplicateCourses(rawCourses as Record<string, unknown>[])
        );
        setRecentCourses(
          uniqueCourses.slice(0, 4).map((c) => ({
            id: getCourseId(c),
            title: String(c.title || "Khóa học"),
            progress: getCourseProgress(c),
          }))
        );
      }

      if (practiceStatsResp && practiceStatsResp.ok) {
        const pStats = await practiceStatsResp.json().catch(() => null);
        if (pStats && Array.isArray(pStats.needsReview)) {
          setPracticeReviewCount(pStats.needsReview.length);
        }
      }
    } catch {
      // Silent fail for sidebar data
    } finally {
      setLoadingCourses(false);
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
    if (item.href === "/app/practice" && practiceReviewCount > 0) {
      return { ...item, badge: practiceReviewCount };
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
      className={`hidden md:flex flex-col h-screen border-r border-[var(--border-calm)] bg-[var(--surface-card)] shrink-0 transition-all duration-200 ${
        collapsed ? "w-[56px]" : "w-[240px]"
      }`}
    >
      {/* Brand header */}
      <div className="h-14 flex items-center justify-between px-3 border-b border-[var(--border-calm)]">
        <Link
          href="/app"
          className="flex items-center gap-2.5 min-w-0 group"
          title="Studxy — Tổng quan"
        >
          <span className="inline-flex items-center justify-center w-7 h-7 rounded-[8px] bg-[#5E51E8] dark:bg-[#818CF8] text-white dark:text-[#0F0F12] text-[11px] font-bold shrink-0 group-hover:scale-105 transition-transform shadow-xs">
            St
          </span>
          {!collapsed && (
            <span className="text-sm font-bold text-[var(--text-primary)] tracking-tight truncate">
              Studxy
            </span>
          )}
        </Link>

        <button
          type="button"
          onClick={onToggleCollapse}
          className="w-6 h-6 flex items-center justify-center rounded-[6px] text-[var(--text-tertiary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer"
          title={collapsed ? "Mở rộng sidebar (Ctrl+\\)" : "Thu gọn sidebar (Ctrl+\\)"}
          aria-label={collapsed ? "Mở rộng sidebar" : "Thu gọn sidebar"}
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
            className="w-full h-8 flex items-center gap-2 px-2.5 rounded-[8px] border border-[var(--border-calm)] bg-[var(--canvas-bg)] text-[var(--text-tertiary)] text-xs hover:border-[var(--accent)] hover:text-[var(--text-secondary)] transition cursor-pointer"
          >
            <Search className="w-3.5 h-3.5 shrink-0" />
            <span className="flex-1 text-left">Tìm kiếm...</span>
            <kbd className="swms-kbd text-[10px] px-1 py-0.5">⌘K</kbd>
          </button>
        </div>
      )}

      {collapsed && (
        <div className="px-2 pt-3 pb-1">
          <button
            type="button"
            onClick={() => setCommandPaletteOpen(true)}
            className="w-8 h-8 flex items-center justify-center rounded-[8px] text-[var(--text-tertiary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer mx-auto"
            title="Tìm kiếm (Ctrl+K)"
            aria-label="Tìm kiếm nhanh (Ctrl+K)"
          >
            <Search className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Main navigation */}
      <nav className="flex-1 overflow-y-auto px-2 py-2 space-y-1">
        {navItemsWithBadge.map((item) => {
          const Icon = item.icon;
          const active = isActive(item);
          return (
            <Link
              key={item.href}
              href={item.href}
              data-testid={`sidebar-nav-${item.href.split("/").pop()}`}
              className={`relative flex items-center gap-2.5 h-8.5 rounded-[8px] text-sm font-medium transition ${
                collapsed ? "justify-center px-0" : "px-2.5"
              } ${
                active
                  ? "bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:text-[#A5B4FC] font-semibold"
                  : "text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]"
              }`}
              title={collapsed ? item.label : undefined}
            >
              <Icon className="w-4 h-4 shrink-0" />
              {!collapsed && (
                <>
                  <span className="flex-1 truncate">{item.label}</span>
                  {item.badge && (
                    <span className="inline-flex items-center justify-center min-w-[18px] h-[18px] rounded-full text-[10px] font-bold bg-[#5E51E8] text-white px-1 shadow-xs">
                      {typeof item.badge === "number" && item.badge > 99
                        ? "99+"
                        : item.badge}
                    </span>
                  )}
                </>
              )}
              {collapsed && item.badge && (
                <span className="absolute -top-0.5 -right-0.5 w-2 h-2 rounded-full bg-[#5E51E8]" />
              )}
            </Link>
          );
        })}

        {/* Divider + Recent Courses with fixed min-h-[156px] */}
        {!collapsed && (
          <div className="min-h-[156px] flex flex-col justify-start">
            <div className="pt-4 pb-1 px-2.5">
              <span className="text-[11px] font-semibold text-[var(--text-tertiary)] uppercase tracking-wider">
                Gần đây
              </span>
            </div>
            {loadingCourses ? (
              <div className="space-y-1.5 px-2.5 py-1">
                {[0, 1, 2, 3].map((i) => (
                  <div key={i} className="h-7 w-full rounded-[6px] bg-[var(--surface-elevated)] animate-pulse" />
                ))}
              </div>
            ) : recentCourses.length > 0 ? (
              recentCourses.map((course) => (
                <Link
                  key={course.id}
                  href={`/app/courses/${course.id}`}
                  title={course.title}
                  className={`flex items-start gap-2.5 min-h-[34px] py-1.5 px-2.5 rounded-[8px] text-sm transition ${
                    pathname === `/app/courses/${course.id}`
                      ? "bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:text-[#A5B4FC] font-medium"
                      : "text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]"
                  }`}
                >
                  <BookOpen className="w-3.5 h-3.5 shrink-0 opacity-60 mt-0.5" />
                  <span className="flex-1 line-clamp-2 leading-snug text-[12px]">
                    {course.title}
                  </span>
                </Link>
              ))
            ) : null}
          </div>
        )}

        {/* Create course shortcut */}
        {!collapsed && (
          <div className="pt-2">
            <Link
              href="/app/courses/new"
              className="flex items-center gap-2.5 h-8 px-2.5 rounded-[8px] text-sm text-[var(--text-tertiary)] hover:text-[#5E51E8] hover:bg-[#EEF0FD] dark:hover:bg-[#1E1B4B] transition font-medium"
            >
              <Plus className="w-4 h-4 shrink-0" />
              <span>Tạo khóa học</span>
            </Link>
          </div>
        )}
      </nav>

      {/* Bottom section — Streak + Theme + Profile */}
      <div className="border-t border-[var(--border-calm)] px-2 py-2 space-y-1">
        {/* Streak */}
        {!collapsed && streakDays > 0 && (
          <div className="flex items-center gap-2 px-2.5 h-7 text-xs text-[var(--text-secondary)]">
            <Flame className="w-3.5 h-3.5 text-[#D97706] dark:text-[#FBBF24]" />
            <span className="font-semibold">{streakDays} ngày liên tục</span>
          </div>
        )}

        {/* Theme toggle */}
        <button
          type="button"
          onClick={toggleDarkMode}
          className={`flex items-center gap-2.5 h-8 rounded-[8px] text-sm text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition w-full cursor-pointer ${
            collapsed ? "justify-center px-0" : "px-2.5"
          }`}
          title={darkMode ? "Chuyển sang sáng" : "Chuyển sang tối"}
          aria-label={darkMode ? "Chuyển sang sáng" : "Chuyển sang tối"}
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
          className={`flex items-center gap-2.5 h-8 rounded-[8px] text-sm text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition ${
            collapsed ? "justify-center px-0" : "px-2.5"
          }`}
          title="Chế độ bàn làm việc nhiều khung trên máy tính (/workspace)"
        >
          <LayoutGrid className="w-4 h-4 shrink-0" />
          {!collapsed && <span>Giao diện Classic (Split)</span>}
        </Link>

        {/* Settings */}
        <Link
          href="/app/settings"
          className={`flex items-center gap-2.5 h-8 rounded-[8px] text-sm text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition ${
            collapsed ? "justify-center px-0" : "px-2.5"
          } ${pathname === "/app/settings" ? "bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:text-[#A5B4FC]" : ""}`}
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
            className={`flex items-center gap-2.5 h-9 rounded-[8px] text-sm text-[var(--text-secondary)] hover:bg-[var(--surface-elevated)] transition w-full cursor-pointer ${
              collapsed ? "justify-center px-0" : "px-2.5"
            }`}
            title={collapsed ? userName : undefined}
            aria-expanded={profileOpen}
            aria-haspopup="true"
          >
            <div className="w-6 h-6 rounded-[6px] bg-[#5E51E8] dark:bg-[#818CF8] text-white dark:text-[#0F0F12] text-[10px] font-bold flex items-center justify-center shrink-0">
              {userInitials}
            </div>
            {!collapsed && (
              <div className="flex-1 min-w-0 text-left">
                <div className="text-sm font-medium text-[var(--text-primary)] truncate">
                  {userName}
                </div>
              </div>
            )}
          </button>

          {/* Profile popover */}
          {profileOpen && !collapsed && (
            <div
              className="absolute bottom-full left-0 mb-1 w-full rounded-[12px] border border-[var(--border-calm)] bg-[var(--surface-card)] shadow-elevation-2 p-2 z-50"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="px-2 py-1.5 mb-1">
                <div className="text-sm font-medium text-[var(--text-primary)]">{userName}</div>
                <div className="text-xs text-[var(--text-tertiary)]">{userEmail}</div>
              </div>
              <div className="border-t border-[var(--border-calm)] pt-1">
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
                  className="flex items-center gap-2 w-full px-2 py-1.5 rounded-[8px] text-sm text-rose-600 dark:text-rose-400 hover:bg-rose-50 dark:hover:bg-rose-950/30 transition cursor-pointer"
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
