"use client";

import React, { useCallback, useEffect, useRef, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { getClientRequestHeaders, useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { bootstrapAuthSession, setAccessToken } from "@/lib/api-client";
import { TopNavHeader, type QuotaSummary } from "@/components/layout/TopNavHeader";
import {
  PersonalWorkspaceDashboard,
  type DashboardData
} from "@/components/dashboard/PersonalWorkspaceDashboard";
import { ReaderAndSplitViews } from "@/components/reader/ReaderAndSplitViews";
import { ReviewAndPracticeView } from "@/components/review/ReviewAndPracticeView";
import { CourseStudioView } from "@/components/studio/CourseStudioView";
import { CommandPaletteModal } from "@/components/command/CommandPaletteModal";
import { KeyboardShortcutsModal } from "@/components/auxiliary/KeyboardShortcutsModal";
import { animateWorkspaceTransition } from "@/lib/animations.ts";

const INITIAL_DASHBOARD_DATA: DashboardData = {
  user: {
    id: "",
    fullName: "Học viên",
    email: "",
    streakDays: 0,
    onboardingStep: 0,
    isActivated: false
  },
  reviewStation: {
    totalDue: 0,
    totalCards: 0,
    newCount: 0,
    learningCount: 0,
    reviewCount: 0,
    estimatedMinutes: 0
  },
  lessons: [],
  documents: [],
  notebooks: [],
  tags: [],
  courses: [],
  quizzes: []
};

export default function WorkspacePage() {
  const {
    activeMode,
    setActiveMode,
    darkMode,
    toggleDarkMode,
    sourceType,
    activeCourseId,
    activeDocumentId,
    activeLessonId,
    currentDocumentPage,
    commandPaletteOpen,
    aiConfigModalOpen,
    activeUserHeaderId,
    setActiveUserHeaderId
  } = useWorkspaceStore();
  const router = useRouter();
  const pathname = usePathname();
  const [authReady, setAuthReady] = useState(false);
  const [dashboard, setDashboard] = useState<DashboardData>(INITIAL_DASHBOARD_DATA);
  const [isLoadingDashboard, setIsLoadingDashboard] = useState(true);
  const [dashboardError, setDashboardError] = useState<string | null>(null);
  const [quota, setQuota] = useState<QuotaSummary | null>(null);
  const mainContainerRef = useRef<HTMLDivElement | null>(null);

  const fetchDashboardData = useCallback(async () => {
    setIsLoadingDashboard(true);
    setDashboardError(null);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined"
          ? window.localStorage.getItem("swms_active_user_id")
          : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );
      const reqHeaders = Object.keys(headers).length > 0 ? headers : undefined;
      const dashResp = await fetch("/api/v1/workspace/dashboard", { headers: reqHeaders });
      if (dashResp.ok) {
        const data = await dashResp.json();
        let resolvedCourses = Array.isArray(data.courses) ? data.courses : [];
        if (resolvedCourses.length === 0) {
          const coursesResp = await fetch("/api/v1/courses", { headers: reqHeaders }).catch(() => null);
          const extraCourses =
            coursesResp && coursesResp.ok ? await coursesResp.json().catch(() => null) : null;
          if (Array.isArray(extraCourses) && extraCourses.length > 0) {
            resolvedCourses = extraCourses;
          }
        }
        setDashboard({
          ...data,
          courses: resolvedCourses
        });
        if (data.quota) setQuota(data.quota);
      } else {
        const errBody = await dashResp.json().catch(() => null);
        setDashboardError(
          errBody?.error ||
            `Không thể tải dữ liệu không gian học tập (Mã lỗi HTTP ${dashResp.status}).`
        );
      }
    } catch {
      setDashboardError(
        "Không thể kết nối máy chủ Backend để tải dữ liệu không gian học tập."
      );
    } finally {
      setIsLoadingDashboard(false);
    }
  }, [activeUserHeaderId]);

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
        router.replace(`/auth?redirect=${encodeURIComponent(pathname || "/workspace")}`);
      }
    });
    return () => {
      mounted = false;
    };
  }, [pathname, router, setActiveUserHeaderId]);

  useEffect(() => {
    if (!authReady) return;
    fetchDashboardData();
  }, [authReady, fetchDashboardData]);

  useEffect(() => {
    animateWorkspaceTransition(mainContainerRef.current);
  }, [activeMode]);

  const [isShortcutsOpen, setIsShortcutsOpen] = useState(false);

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

      if ((e.ctrlKey || e.metaKey) && !e.altKey && (e.key === "d" || e.key === "D")) {
        e.preventDefault();
        toggleDarkMode();
        return;
      }

      if (
        isShortcutsOpen ||
        commandPaletteOpen ||
        aiConfigModalOpen ||
        document.querySelector('[role="dialog"]')
      ) {
        return;
      }

      if (e.key === "?") {
        e.preventDefault();
        setIsShortcutsOpen(true);
        return;
      }

      if (e.key === "0" && !e.ctrlKey && !e.altKey && !e.metaKey) {
        e.preventDefault();
        setActiveMode("dashboard");
        return;
      }

      if (
        (e.key === "1" || e.key === "2" || e.key === "3" || e.key === "4") &&
        !e.ctrlKey &&
        !e.metaKey
      ) {
        const hasSelectedText =
          typeof window !== "undefined" &&
          (window.getSelection()?.toString().trim().length ?? 0) >= 3;
        const canSwitchMode =
          e.altKey ||
          activeMode === "dashboard" ||
          ((activeMode === "reading" || activeMode === "split" || activeMode === "studio") && !hasSelectedText);

        if (canSwitchMode) {
          e.preventDefault();
          if (e.key === "1") setActiveMode("reading");
          if (e.key === "2") setActiveMode("split");
          if (e.key === "3") setActiveMode("review");
          if (e.key === "4") setActiveMode("studio");
        }
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isShortcutsOpen, commandPaletteOpen, aiConfigModalOpen, activeMode, setActiveMode, toggleDarkMode]);

  const coursesList = dashboard.courses ?? [];
  const activeDoc = dashboard.documents.find((d) => d.id === activeDocumentId);
  const allCourseLessons = coursesList.flatMap((c) =>
    (c.chapters ?? []).flatMap((ch) => ch.lessons ?? [])
  );
  const activeLesson =
    dashboard.lessons.find((l) => l.id === activeLessonId) ??
    allCourseLessons.find((l) => l.id === activeLessonId) ??
    dashboard.lessons[0];

  const resolvedCourseId = activeCourseId || activeLesson?.courseId || null;
  const activeCourse =
    coursesList.find((c) => (c.courseId || c.id) === resolvedCourseId) ||
    (activeDocumentId
      ? coursesList.find((c) => c.sourceDocumentId === activeDocumentId)
      : undefined);

  const breadcrumbRoot =
    activeMode === "dashboard" ? "Khóa học" : activeCourse?.title || "Khóa học";

  const activeTitle =
    activeMode === "dashboard"
      ? "Khóa học của tôi"
      : activeMode === "studio"
      ? activeCourse?.title || "Studio Khóa học"
      : activeMode === "review"
      ? "Ôn tập FSRS & Quiz"
      : sourceType === "document" && activeDoc
      ? `${activeDoc.title} · Trang ${currentDocumentPage} / ${activeDoc.totalPages}`
      : activeLesson?.title || activeCourse?.title || "Chưa chọn bài học";

  if (!authReady) {
    return (
      <div className={darkMode ? "dark" : ""}>
        <div className="min-h-screen w-full flex items-center justify-center bg-[var(--canvas-bg)] text-[var(--text-primary)]">
          <div className="flex flex-col items-center gap-3">
            <div className="w-7 h-7 border-2 border-[#5E51E8] border-t-transparent rounded-full animate-spin" />
            <span className="text-xs text-[var(--text-tertiary)]">Đang xác thực phiên làm việc...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={darkMode ? "dark" : ""}>
      <div className="min-h-screen w-full overflow-x-clip bg-[var(--canvas-bg)] text-[var(--text-primary)]">
        <TopNavHeader
          userName={dashboard.user.fullName}
          userEmail={dashboard.user.email}
          streakDays={dashboard.user.streakDays}
          breadcrumbRoot={breadcrumbRoot}
          activeTitle={activeTitle}
          dueCount={dashboard.reviewStation.totalDue}
          quota={quota}
          onOpenShortcuts={() => setIsShortcutsOpen(true)}
        />

        <main ref={mainContainerRef} className="w-full overflow-x-clip">
          {activeMode === "dashboard" && (
            <PersonalWorkspaceDashboard
              data={dashboard}
              onRefreshDashboard={fetchDashboardData}
              isLoadingDashboard={isLoadingDashboard}
              dashboardError={dashboardError}
            />
          )}

          {(activeMode === "reading" || activeMode === "split") && (
            <ReaderAndSplitViews
              mode={activeMode}
              onDataMutated={fetchDashboardData}
              documents={dashboard.documents}
              lessons={dashboard.lessons}
              {...({
                courses: dashboard.courses ?? [],
                quizzes: dashboard.quizzes ?? []
              } as Record<string, unknown>)}
            />
          )}

          {activeMode === "review" && (
            <ReviewAndPracticeView
              tags={dashboard.tags}
              onReviewSessionUpdated={fetchDashboardData}
              {...({
                courses: dashboard.courses ?? [],
                quizzes: dashboard.quizzes ?? [],
                coursesLoading: isLoadingDashboard
              } as Record<string, unknown>)}
            />
          )}

          {activeMode === "studio" && (
            <CourseStudioView onRefreshDashboard={fetchDashboardData} />
          )}
        </main>

        <CommandPaletteModal onQuickCaptureCreated={fetchDashboardData} />
        <KeyboardShortcutsModal
          isOpen={isShortcutsOpen}
          onClose={() => setIsShortcutsOpen(false)}
        />
      </div>
    </div>
  );
}
