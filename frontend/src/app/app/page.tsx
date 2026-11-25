"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import {
  AlertCircle,
  ArrowRight,
  BarChart3,
  BookOpen,
  Brain,
  CheckCircle2,
  ClipboardCheck,
  Clock,
  FileText,
  Flame,
  NotebookPen,
  Play,
  Plus,
  RefreshCw,
  Search,
  TrendingUp,
  Target,
  Sparkles,
  Pin,
  Archive,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached, fetchPracticeStatsCached } from "@/lib/api-client";
import {
  classifyCourseDomain,
  deduplicateCourses,
  extractCourseSkillTags,
  getCourseId,
  getCourseLessonCount,
  getCourseProgress,
  getCourseVisualTheme,
  normalizeCourseSummary,
  sortCoursesSmartly,
  type CourseLike,
  type CourseSummary,
} from "@/lib/course-helpers";
import { buildPracticeLessonHref } from "@/lib/practice-helpers";
import { LearningRoadmapsShowcase } from "@/components/dashboard/LearningRoadmapsShowcase";
import {
  getReviewCapSettings,
  type ReviewCapSettings,
} from "@/lib/review-settings";
import { EmptyState, Card, StatCard, ProgressBar, Badge } from "@/components/ui";

interface PracticeStatsData {
  totalAttempts: number;
  averageScore: number;
  passedCount: number;
  todayAttempts: number;
  streakDays: number;
  needsReview: Array<{
    activityKey: string;
    activityTitle: string;
    activityType: string;
    courseId: string;
    lessonId?: string | null;
    score: number;
    createdAt: string;
  }>;
  recentHistory: Array<{
    id: string;
    activityKey: string;
    activityTitle: string;
    activityType: string;
    score: number;
    maxScore: number;
    isPassed: boolean;
    createdAt: string;
  }>;
  last7Days: Array<{
    date: string;
    dayName: string;
    count: number;
    avgScore: number;
  }>;
}

interface DashboardUser {
  id: string;
  fullName: string;
  email: string;
  streakDays: number;
}

interface ReviewStation {
  totalDue: number;
  totalCards: number;
  newCount: number;
  learningCount: number;
  reviewCount: number;
  estimatedMinutes: number;
}

interface NextLessonInfo {
  id: string;
  title: string;
  chapterTitle?: string;
  minutes?: number;
}

interface DashboardQuizItem {
  id: string;
  title: string;
  courseId?: string | null;
  totalItems: number;
  correctItems: number;
  status: string;
  completedAt?: string | null;
}

interface CourseDetailLesson {
  id: string;
  title: string;
  isCompleted?: boolean;
  durationMinutes?: number;
  estimatedMinutes?: number;
}

interface CourseDetailChapter {
  id: string;
  title: string;
  lessons?: CourseDetailLesson[];
}

interface ContinueCourseMeta {
  courseId: string;
  dailyLessonGoal: number;
  targetCompletionDate?: string | null;
  totalLessons: number;
  completedLessons: number;
}

export default function DashboardPage() {
  const { activeUserHeaderId, setCommandPaletteOpen } = useWorkspaceStore();
  const [user, setUser] = useState<DashboardUser>({
    id: "",
    fullName: "Học viên",
    email: "",
    streakDays: 0,
  });
  const [reviewStation, setReviewStation] = useState<ReviewStation>({
    totalDue: 0,
    totalCards: 0,
    newCount: 0,
    learningCount: 0,
    reviewCount: 0,
    estimatedMinutes: 0,
  });
  const [courses, setCourses] = useState<CourseLike[]>([]);
  const [archivedCoursesCount, setArchivedCoursesCount] = useState(0);
  const [quizzes, setQuizzes] = useState<DashboardQuizItem[]>([]);
  const [documentsCount, setDocumentsCount] = useState(0);
  const [notebooksCount, setNotebooksCount] = useState(0);
  const [practiceStats, setPracticeStats] = useState<PracticeStatsData | null>(null);
  const [practiceStatsError, setPracticeStatsError] = useState<string | null>(null);
  const [nextLessonInfo, setNextLessonInfo] = useState<NextLessonInfo | null>(null);
  const [continueCourseMeta, setContinueCourseMeta] = useState<ContinueCourseMeta | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    setFetchError(null);
    setPracticeStatsError(null);
    setNextLessonInfo(null);
    setContinueCourseMeta(null);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );
      const cachedReqOpts = effectiveUserId
        ? { headers: { "X-Test-User-Id": effectiveUserId } }
        : undefined;
      const [data, pStats] = await Promise.all([
        fetchDashboardCached(cachedReqOpts),
        fetchPracticeStatsCached(cachedReqOpts),
      ]);

      if (!data) {
        setFetchError("Không tải được dữ liệu bảng điều khiển.");
        return;
      }
      setUser(data.user ?? { id: "", fullName: "Học viên", email: "", streakDays: 0 });
      setReviewStation(
        data.reviewStation ?? {
          totalDue: 0,
          totalCards: 0,
          newCount: 0,
          learningCount: 0,
          reviewCount: 0,
          estimatedMinutes: 0,
        }
      );
      setDocumentsCount(Array.isArray(data.documents) ? data.documents.length : 0);
      setNotebooksCount(Array.isArray(data.notebooks) ? data.notebooks.length : 0);
      setQuizzes(Array.isArray(data.quizzes) ? data.quizzes : []);

      if (pStats && typeof pStats.totalAttempts === "number") {
        setPracticeStats(pStats);
        setPracticeStatsError(null);
      } else {
        setPracticeStatsError("Không thể tải dữ liệu thống kê luyện tập từ máy chủ.");
      }

      let rawCourses: CourseLike[] =
        Array.isArray(data.courses) && data.courses.length > 0 ? data.courses : [];
      if (rawCourses.length === 0) {
        const coursesResp = await fetch("/api/v1/courses", { headers }).catch(() => null);
        const extraCourses =
          coursesResp && coursesResp.ok ? await coursesResp.json().catch(() => null) : null;
        if (Array.isArray(extraCourses) && extraCourses.length > 0) {
          rawCourses = extraCourses;
        }
      }

      const normalizedAll = rawCourses.map(normalizeCourseSummary);
      const archived = normalizedAll.filter((c) => Boolean(c.isArchived));
      const active = normalizedAll.filter((c) => !c.isArchived);
      setArchivedCoursesCount(archived.length);

      const unique = sortCoursesSmartly(deduplicateCourses(active));
      setCourses(unique);

      const activeCourse =
        unique.find((c) => Boolean(c.isPinned) && !c.isArchived && getCourseProgress(c) < 100) ||
        unique.find((c) => Boolean(c.isPinned) && !c.isArchived) ||
        unique.find((c) => {
          const p = getCourseProgress(c);
          return p > 0 && p < 100;
        }) ||
        unique.find((c) => getCourseProgress(c) < 100) ||
        unique[0];

      const applyNextLessonFromCourse = (
        courseDetail: {
          id?: string;
          completedLessonIds?: string[];
          chapters?: CourseDetailChapter[];
          dailyLessonGoal?: number | null;
          targetCompletionDate?: string | null;
          totalLessons?: number | null;
          completedLessons?: number | null;
        },
        fallbackCourse?: CourseLike
      ) => {
        const completedSet = new Set<string>(courseDetail.completedLessonIds ?? []);
        const chapters: CourseDetailChapter[] = Array.isArray(courseDetail.chapters)
          ? courseDetail.chapters
          : [];
        const flattened = chapters.flatMap((ch) =>
          (ch.lessons ?? []).map((lesson) => ({
            lesson,
            chapterTitle: ch.title,
          }))
        );
        const candidate =
          flattened.find(
            ({ lesson }) => !lesson.isCompleted && !completedSet.has(lesson.id)
          ) || flattened[0];

        if (candidate) {
          setNextLessonInfo({
            id: candidate.lesson.id,
            title: candidate.lesson.title,
            chapterTitle: candidate.chapterTitle,
            minutes: candidate.lesson.durationMinutes ?? candidate.lesson.estimatedMinutes,
          });
        }

        const resolvedId =
          courseDetail.id || (fallbackCourse ? getCourseId(fallbackCourse) : "");
        if (resolvedId) {
          const fallbackRaw = (fallbackCourse ?? {}) as {
            dailyLessonGoal?: number | null;
            targetCompletionDate?: string | null;
            totalLessons?: number | null;
            completedLessons?: number | null;
          };
          const completedFromChapters = flattened.filter(
            ({ lesson }) => Boolean(lesson.isCompleted) || completedSet.has(lesson.id)
          ).length;
          const totalLessons =
            Number(courseDetail.totalLessons ?? fallbackRaw.totalLessons ?? 0) ||
            flattened.length ||
            (fallbackCourse ? getCourseLessonCount(fallbackCourse) : 0);
          const completedLessons =
            Number(
              courseDetail.completedLessons ??
                fallbackRaw.completedLessons ??
                completedFromChapters
            ) || 0;
          const dailyLessonGoal = Number(
            courseDetail.dailyLessonGoal ?? fallbackRaw.dailyLessonGoal ?? 0
          );
          const targetCompletionDate =
            courseDetail.targetCompletionDate ?? fallbackRaw.targetCompletionDate ?? null;

          setContinueCourseMeta({
            courseId: resolvedId,
            dailyLessonGoal: Number.isFinite(dailyLessonGoal) ? dailyLessonGoal : 0,
            targetCompletionDate,
            totalLessons,
            completedLessons,
          });
        }
      };

      const activeCourseChapters = (activeCourse as { chapters?: CourseDetailChapter[] } | undefined)?.chapters;
      const activeId = activeCourse ? getCourseId(activeCourse) : "";
      const hasEmbeddedGoal =
        (activeCourse as { dailyLessonGoal?: unknown } | undefined)?.dailyLessonGoal !== undefined;
      if (Array.isArray(activeCourseChapters) && activeCourseChapters.length > 0) {
        applyNextLessonFromCourse(
          {
            id: activeId,
            completedLessonIds: (activeCourse as { completedLessonIds?: string[] }).completedLessonIds,
            chapters: activeCourseChapters,
            dailyLessonGoal: (activeCourse as { dailyLessonGoal?: number | null }).dailyLessonGoal,
            targetCompletionDate: (activeCourse as { targetCompletionDate?: string | null })
              .targetCompletionDate,
            totalLessons: (activeCourse as { totalLessons?: number | null }).totalLessons,
            completedLessons: (activeCourse as { completedLessons?: number | null }).completedLessons,
          },
          activeCourse
        );
      }
      if (activeId && (!Array.isArray(activeCourseChapters) || activeCourseChapters.length === 0 || !hasEmbeddedGoal)) {
        void fetch(`/api/v1/courses/${activeId}`, { headers })
          .then((resp) => (resp.ok ? resp.json() : null))
          .then((detail) => {
            if (detail) applyNextLessonFromCourse(detail, activeCourse);
          })
          .catch(() => null);
      }
    } catch {
      setFetchError("Không thể kết nối tới máy chủ. Vui lòng thử lại.");
    } finally {
      setIsLoading(false);
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const [reviewCaps, setReviewCaps] = useState<ReviewCapSettings>(getReviewCapSettings);

  useEffect(() => {
    const handleCapsUpdate = (e: Event) => {
      const detail = (e as CustomEvent<ReviewCapSettings>).detail;
      if (detail) setReviewCaps(detail);
    };
    window.addEventListener("studxy:review-caps-updated", handleCapsUpdate);
    return () => window.removeEventListener("studxy:review-caps-updated", handleCapsUpdate);
  }, []);

  const totalDue = reviewStation.totalDue;
  const cappedDue = Math.min(totalDue, reviewCaps.maxReviewsPerDay);
  const backlogCount = Math.max(0, totalDue - cappedDue);
  const estimatedMins = Math.max(1, Math.round(cappedDue * 0.5));

  // Lịch sử kiểm tra: chỉ hiển thị bài đã nộp (hoàn tất)
  const submittedQuizzes = quizzes.filter(
    (q) => q.status === "completed" || q.status === "submitted"
  );
  const recentQuizzes = submittedQuizzes.slice(0, 3);

  const continueCourse =
    courses.find((c) => Boolean(c.isPinned) && !c.isArchived && getCourseProgress(c) < 100) ||
    courses.find((c) => Boolean(c.isPinned) && !c.isArchived) ||
    courses.find((c) => {
      const p = getCourseProgress(c);
      return p > 0 && p < 100;
    }) ||
    courses.find((c) => getCourseProgress(c) < 100) ||
    courses[0];

  const continueCourseId = continueCourse ? getCourseId(continueCourse) : "";
  const continueProgress = continueCourse ? getCourseProgress(continueCourse) : 0;
  const continueLearnHref = continueCourseId
    ? nextLessonInfo
      ? `/app/courses/${continueCourseId}/learn?lesson=${nextLessonInfo.id}`
      : `/app/courses/${continueCourseId}/learn`
    : "/app/courses/new";

  const activeMeta =
    continueCourseMeta && continueCourseMeta.courseId === continueCourseId
      ? continueCourseMeta
      : null;
  const continueDailyGoal =
    activeMeta?.dailyLessonGoal ??
    Number((continueCourse as { dailyLessonGoal?: number } | undefined)?.dailyLessonGoal ?? 0);
  const continueTargetDate =
    activeMeta?.targetCompletionDate ??
    (continueCourse as { targetCompletionDate?: string | null } | undefined)?.targetCompletionDate ??
    null;
  const continueTotalLessons =
    activeMeta?.totalLessons ?? (continueCourse ? getCourseLessonCount(continueCourse) : 0);
  const continueCompletedLessons =
    activeMeta?.completedLessons ??
    Number((continueCourse as { completedLessons?: number } | undefined)?.completedLessons ?? 0);
  const continueRemaining = Math.max(0, continueTotalLessons - continueCompletedLessons);

  let continueForecastText: string | null = null;
  if (continueRemaining > 0 && continueDailyGoal > 0) {
    const daysNeeded = Math.ceil(continueRemaining / continueDailyGoal);
    const parsedTarget = continueTargetDate ? new Date(continueTargetDate) : null;
    const targetDateObj =
      parsedTarget && !Number.isNaN(parsedTarget.getTime())
        ? parsedTarget
        : new Date(Date.now() + daysNeeded * 86_400_000);
    const dd = String(targetDateObj.getDate()).padStart(2, "0");
    const mm = String(targetDateObj.getMonth() + 1).padStart(2, "0");
    const yyyy = targetDateObj.getFullYear();
    continueForecastText = `Dự kiến xong: ${dd}/${mm}/${yyyy} (~${daysNeeded} ngày)`;
  }

  const avgCourseProgress =
    courses.length > 0
      ? Math.round(
          courses.reduce((sum, c) => sum + getCourseProgress(c), 0) / courses.length
        )
      : 0;
  const totalLessonsAll = courses.reduce((sum, c) => sum + getCourseLessonCount(c), 0);
  const totalCompletedAll = courses.reduce(
    (sum, c) => sum + Number((c as { completedLessons?: number }).completedLessons ?? 0),
    0
  );
  const totalCardsSafe = Math.max(0, reviewStation.totalCards);
  const masteryPercent =
    totalCardsSafe > 0
      ? Math.min(
          100,
          Math.round(
            ((reviewStation.reviewCount + reviewStation.learningCount * 0.5) / totalCardsSafe) * 100
          )
        )
      : 0;
  const streakFilledCount = Math.min(7, Math.max(0, user.streakDays));

  if (isLoading) {
    return (
      <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh] space-y-8">
        {/* Header skeleton */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div className="space-y-2">
            <div className="h-8 w-64 max-w-full rounded-[8px] swms-skeleton" />
            <div className="h-4 w-48 max-w-full rounded-[6px] swms-skeleton" />
          </div>
          <div className="flex items-center gap-2">
            <div className="hidden sm:block h-9 w-32 rounded-[8px] swms-skeleton" />
            <div className="h-9 w-32 rounded-[8px] swms-skeleton" />
          </div>
        </div>

        {/* 2 Focus cards skeleton */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {[0, 1].map((i) => (
            <div
              key={i}
              className="rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] shadow-elevation-1 overflow-hidden p-5 space-y-4"
            >
              <div className="flex items-center justify-between gap-2">
                <div className="h-4 w-32 rounded-[6px] swms-skeleton" />
                <div className="h-5 w-20 rounded-[6px] swms-skeleton" />
              </div>
              <div className="space-y-2">
                <div className="h-6 w-3/4 rounded-[6px] swms-skeleton" />
                <div className="h-4 w-1/2 rounded-[6px] swms-skeleton" />
              </div>
              <div className="h-2 w-full rounded-full swms-skeleton" />
              <div className="h-9 w-32 rounded-[8px] swms-skeleton" />
            </div>
          ))}
        </div>

        {/* Practice hub skeleton */}
        <div className="rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] shadow-elevation-1 p-5 space-y-5">
          <div className="flex items-center justify-between gap-2 border-b border-[var(--border-calm)] pb-3">
            <div className="h-5 w-60 max-w-full rounded-[6px] swms-skeleton" />
            <div className="h-4 w-24 rounded-[6px] swms-skeleton" />
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
            {[0, 1, 2, 3].map((i) => (
              <div key={i} className="h-20 rounded-lg swms-skeleton" />
            ))}
          </div>
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-4">
            <div className="lg:col-span-7 h-44 rounded-lg swms-skeleton" />
            <div className="lg:col-span-5 h-44 rounded-lg swms-skeleton" />
          </div>
        </div>

        {/* 4 Recent course cards skeleton */}
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="h-5 w-40 rounded-[6px] swms-skeleton" />
            <div className="h-4 w-28 rounded-[6px] swms-skeleton" />
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
            {[0, 1, 2, 3].map((i) => (
              <div
                key={i}
                className="min-h-[176px] rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] p-4 flex flex-col justify-between gap-3"
              >
                <div className="space-y-2.5">
                  <div className="h-4 w-20 rounded-[5px] swms-skeleton" />
                  <div className="h-5 w-full rounded-[6px] swms-skeleton" />
                  <div className="h-4 w-2/3 rounded-[6px] swms-skeleton" />
                </div>
                <div className="space-y-2">
                  <div className="h-3 w-full rounded-[4px] swms-skeleton" />
                  <div className="h-1.5 w-full rounded-full swms-skeleton" />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh]">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-text-primary">
            Chào bạn, hôm nay học gì?
          </h1>
          <div className="flex items-center gap-3 mt-1.5 text-sm text-text-secondary">
            {user.streakDays > 0 ? (
              <span className="inline-flex items-center gap-1.5 text-amber-600 dark:text-amber-400 font-medium">
                <Flame className="w-4 h-4" />
                {user.streakDays} ngày học liên tiếp
              </span>
            ) : (
              <span>Tập trung từng bài một, giữ nhịp học mỗi ngày.</span>
            )}
          </div>
        </div>

        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => setCommandPaletteOpen(true)}
            className="hidden sm:inline-flex items-center gap-2 h-9 px-3 rounded-md border border-border bg-bg-secondary text-text-tertiary text-sm hover:text-text-secondary transition cursor-pointer"
          >
            <Search className="w-4 h-4" />
            <span>Tìm nhanh...</span>
            <kbd className="text-[10px] font-mono px-1.5 py-0.5 rounded bg-bg-tertiary border border-border text-text-tertiary ml-1">
              ⌘K
            </kbd>
          </button>
          <Link
            href="/app/courses/new"
            className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-md bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC] text-sm font-medium transition shadow-xs"
          >
            <Plus className="w-4 h-4" />
            <span>Tạo khóa học</span>
          </Link>
        </div>
      </div>

      {fetchError && (
        <div className="mb-6 rounded-lg border border-red-500/30 bg-red-500/5 px-4 py-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 text-sm text-red-600 dark:text-red-400">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{fetchError}</span>
          </div>
          <button
            type="button"
            onClick={fetchData}
            className="inline-flex items-center gap-1.5 text-xs font-medium px-2.5 py-1.5 rounded-md border border-border hover:bg-bg-secondary text-text-primary cursor-pointer"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            Thử lại
          </button>
        </div>
      )}

      {/* Focus Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-10">
        {/* Card 1: Continue Course Roadmap */}
        <div className="rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] shadow-elevation-1 hover:shadow-elevation-2 transition-all overflow-hidden flex flex-col">
          <div className="h-1 w-full bg-gradient-to-r from-[#5E51E8] via-indigo-500 to-violet-500" />
          <div className="p-5 flex-1 flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between gap-2 mb-3 flex-wrap">
                <div className="flex items-center gap-2 text-xs font-medium uppercase tracking-wider text-text-tertiary">
                  <span className="p-1.5 rounded-lg bg-[var(--accent-subtle)] text-[#5E51E8] dark:text-[#A5B4FC]">
                    <BookOpen className="w-3.5 h-3.5" />
                  </span>
                  <span>Tiếp tục lộ trình</span>
                </div>
                {continueCourse && (
                  <div className="flex items-center gap-1.5">
                    {Boolean((continueCourse as { isPinned?: boolean }).isPinned) && (
                      <span className="inline-flex items-center gap-1 text-[11px] font-semibold text-amber-700 dark:text-amber-300 bg-amber-50 dark:bg-amber-950/40 border border-amber-200 dark:border-amber-800 px-2 py-0.5 rounded-md">
                        <Pin className="w-3 h-3 fill-current text-amber-500" />
                        <span>Đã ghim</span>
                      </span>
                    )}
                    <span className="text-xs px-2 py-0.5 rounded-md bg-bg-tertiary text-text-secondary">
                      {classifyCourseDomain(continueCourse).label}
                    </span>
                  </div>
                )}
              </div>

              {continueCourse ? (
                <>
                  <h2 className="text-base font-semibold text-text-primary mb-1 line-clamp-2">
                    {continueCourse.title}
                  </h2>
                  {nextLessonInfo ? (
                    <p className="text-sm text-text-secondary mb-3 line-clamp-1">
                      Bài tiếp theo: <span className="text-text-primary font-medium">{nextLessonInfo.title}</span>
                      {nextLessonInfo.minutes ? ` (${nextLessonInfo.minutes}p)` : ""}
                    </p>
                  ) : (
                    <p className="text-sm text-text-tertiary mb-3">
                      {getCourseLessonCount(continueCourse)} bài giảng trong lộ trình
                    </p>
                  )}

                  {(continueDailyGoal > 0 || continueForecastText) && (
                    <div className="flex flex-wrap items-center gap-2 mb-3.5">
                      {continueDailyGoal > 0 && (
                        <Badge variant="brand" size="sm">
                          <Target className="w-3 h-3" />
                          <span>Mục tiêu: {continueDailyGoal} bài/ngày</span>
                        </Badge>
                      )}
                      {continueForecastText && (
                        <span className="inline-flex items-center gap-1 text-[11px] font-medium text-[var(--text-secondary)] bg-[var(--surface-elevated)] border border-[var(--border-calm)] px-2 py-0.5 rounded-[5px]">
                          <Clock className="w-3 h-3 text-[var(--text-tertiary)]" />
                          <span>{continueForecastText}</span>
                        </span>
                      )}
                    </div>
                  )}

                  <div className="mb-4">
                    <div className="flex items-center justify-between text-xs text-text-tertiary mb-1.5">
                      <span>Tiến độ khóa học</span>
                      <span className="font-medium text-text-secondary">{continueProgress}%</span>
                    </div>
                    <div className="w-full h-1.5 bg-bg-tertiary rounded-full overflow-hidden">
                      <div
                        className={`h-full rounded-full transition-all ${
                          continueProgress === 100 ? "bg-emerald-500" : "bg-[#5E51E8] dark:bg-[#818CF8]"
                        }`}
                        style={{ width: `${continueProgress}%` }}
                      />
                    </div>
                  </div>
                </>
              ) : (
                <div className="py-4">
                  <h2 className="text-base font-semibold text-text-primary mb-1">
                    Bắt đầu hành trình học mới
                  </h2>
                  <p className="text-sm text-text-secondary">
                    Tạo khóa học từ tài liệu PDF hoặc để AI phác thảo lộ trình theo mục tiêu của bạn.
                  </p>
                </div>
              )}
            </div>

            <div className="flex items-center justify-between gap-3 pt-1">
              {continueCourse ? (
                <>
                  <Link
                    href={continueLearnHref}
                    className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC] text-sm font-medium transition shadow-xs"
                  >
                    <Play className="w-3.5 h-3.5" />
                    <span>{continueProgress > 0 ? "Học tiếp" : "Bắt đầu học"}</span>
                  </Link>
                  <Link
                    href={`/app/courses/${continueCourseId}`}
                    className="text-xs font-medium text-text-secondary hover:text-accent transition"
                  >
                    Xem đề cương →
                  </Link>
                </>
              ) : (
                <Link
                  href="/app/courses/new"
                  className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC] text-sm font-medium transition shadow-xs"
                >
                  <Plus className="w-4 h-4" />
                  <span>Tạo khóa học đầu tiên</span>
                </Link>
              )}
            </div>
          </div>
        </div>

        {/* Card 2: Today's Memory Review */}
        <div className="rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] shadow-elevation-1 hover:shadow-elevation-2 transition-all overflow-hidden flex flex-col">
          <div className="h-1 w-full bg-gradient-to-r from-emerald-500 via-teal-500 to-cyan-500" />
          <div className="p-5 flex-1 flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between gap-2 mb-3">
                <div className="flex items-center gap-2 text-xs font-medium uppercase tracking-wider text-text-tertiary">
                  <span className="p-1.5 rounded-lg bg-[var(--accent-subtle)] text-[#5E51E8] dark:text-[#A5B4FC]">
                    <Brain className="w-3.5 h-3.5" />
                  </span>
                  <span>Ôn tập ghi nhớ</span>
                </div>
                <span className="text-xs text-text-tertiary">
                  {reviewStation.totalCards} thẻ đang lưu
                </span>
              </div>

              <div className="flex items-baseline gap-2 mb-1 flex-wrap">
                <span className="text-2xl sm:text-3xl font-bold tracking-tight text-text-primary">
                  Hôm nay: {cappedDue} thẻ
                </span>
                <span className="text-sm font-medium text-text-secondary">
                  (~{estimatedMins} phút)
                </span>
              </div>

              <div className="flex flex-wrap items-center gap-2 mb-4 text-xs">
                <span className="text-text-tertiary">
                  {backlogCount > 0 ? `còn ${backlogCount} thẻ tồn` : "Đã lên lịch tối ưu"}
                </span>
                <span className="text-text-tertiary">·</span>
                <span className="text-text-tertiary">Giới hạn {reviewCaps.maxReviewsPerDay} thẻ/ngày</span>
              </div>
            </div>

            <div className="pt-1">
              <Link
                href="/app/review"
                className={`inline-flex items-center gap-2 h-9 px-4 rounded-md text-sm font-medium transition ${
                  cappedDue > 0
                    ? "bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC] shadow-xs"
                    : "border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-[var(--text-primary)] hover:bg-[var(--border-calm)]"
                }`}
              >
                <span>{cappedDue > 0 ? "Bắt đầu ôn tập" : "Mở phòng ôn tập"}</span>
                <ArrowRight className="w-3.5 h-3.5" />
              </Link>
            </div>
          </div>
        </div>
      </div>

      {/* Learning Statistics & Personalized Roadmap */}
      <section data-testid="today-stats-roadmap" className="mb-10">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-text-primary">
            Luyện tập hôm nay &amp; Thống kê học tập cá nhân hóa
          </h2>
          <span className="text-xs text-text-tertiary">
            Cập nhật trực tiếp từ Practice Engine &amp; FSRS
          </span>
        </div>

        {/* TODAY'S PRACTICE HUB */}
        <div data-testid="today-practice-hub" className="mb-6 rounded-xl border border-border bg-bg-secondary p-5 space-y-5">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 border-b border-border pb-3">
            <div className="flex items-center gap-2">
              <span className="p-1.5 rounded-lg bg-accent-subtle text-accent">
                <Target className="w-4 h-4" />
              </span>
              <div>
                <h3 className="text-sm font-bold text-text-primary">
                  Hub Luyện Tập Hôm Nay (Practice &amp; Recall Hub)
                </h3>
                <p className="text-xs text-text-tertiary">
                  Theo dõi chuỗi thực hành, phản hồi Web Worker, mô phỏng tài chính &amp; bài tập nhận thức
                </p>
              </div>
            </div>
            <Link
              href="/app/practice"
              className="text-xs font-semibold text-accent hover:underline inline-flex items-center gap-1 self-start sm:self-auto"
            >
              <span>Mở Hub Luyện tập</span>
              <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          </div>

          {/* ZERO-FACADE: IF practiceStats fails with network/server error, show error banner with retry */}
          {practiceStatsError ? (
            <div
              data-testid="today-practice-error"
              className="p-4 rounded-lg border border-red-500/30 bg-red-500/5 text-sm text-red-600 dark:text-red-400 flex flex-col sm:flex-row sm:items-center justify-between gap-3"
            >
              <div className="flex items-center gap-2">
                <AlertCircle className="w-4 h-4 shrink-0" />
                <span>{practiceStatsError}</span>
              </div>
              <button
                type="button"
                onClick={fetchData}
                className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-primary hover:bg-bg-secondary cursor-pointer self-start sm:self-auto shrink-0"
              >
                <RefreshCw className="w-3.5 h-3.5" />
                Thử lại
              </button>
            </div>
          ) : (
            <>
              {/* 4 QUICK STAT TILES */}
              <div data-testid="today-practice-stats" className="grid grid-cols-2 sm:grid-cols-4 gap-3">
            <div className="p-3 rounded-lg border border-border bg-bg-primary">
              <span className="text-[11px] text-text-tertiary flex items-center gap-1">
                <Flame className="w-3.5 h-3.5 text-amber-500" />
                Chuỗi luyện tập
              </span>
              <p className="text-lg font-bold text-amber-600 dark:text-amber-400 mt-1">
                {practiceStats?.streakDays ?? user.streakDays} ngày
              </p>
            </div>
            <div className="p-3 rounded-lg border border-border bg-bg-primary">
              <span className="text-[11px] text-text-tertiary flex items-center gap-1">
                <Target className="w-3.5 h-3.5 text-indigo-500" />
                Tổng lượt thực hành
              </span>
              <p className="text-lg font-bold text-text-primary mt-1">
                {practiceStats?.totalAttempts ?? 0} lượt
              </p>
            </div>
            <div className="p-3 rounded-lg border border-border bg-bg-primary">
              <span className="text-[11px] text-text-tertiary flex items-center gap-1">
                <Sparkles className="w-3.5 h-3.5 text-purple-500" />
                Điểm trung bình
              </span>
              <p className="text-lg font-bold text-purple-600 dark:text-purple-400 mt-1">
                {practiceStats?.averageScore ?? 0} / 100
              </p>
            </div>
            <div className="p-3 rounded-lg border border-border bg-bg-primary">
              <span className="text-[11px] text-text-tertiary flex items-center gap-1">
                <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" />
                Lượt đạt chuẩn
              </span>
              <p className="text-lg font-bold text-emerald-600 dark:text-emerald-400 mt-1">
                {practiceStats?.passedCount ?? 0} bài
              </p>
            </div>
          </div>

          {/* ZERO-FACADE: IF totalAttempts === 0, RENDER FRIENDLY EMPTY STATE */}
          {(!practiceStats || practiceStats.totalAttempts === 0) ? (
            <div data-testid="today-practice-empty-state" className="p-6 rounded-lg border border-dashed border-border bg-bg-primary text-center space-y-3">
              <div className="text-2xl">🎯</div>
              <h4 className="text-xs sm:text-sm font-semibold text-text-primary">
                Bạn chưa có lượt luyện tập tương tác nào
              </h4>
              <p className="text-xs text-text-tertiary max-w-md mx-auto">
                Hãy mở Hub <strong>Luyện tập</strong> hoặc chọn bài học trong khóa học của bạn để thực hành Sandbox Web Worker, mô phỏng danh mục tài chính hoặc bài tập nhận thức Stroop.
              </p>
              <div>
                <Link
                  href="/app/practice"
                  className="px-4 py-2 rounded-lg bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC] text-xs font-semibold inline-flex items-center gap-1.5 transition shadow-xs"
                >
                  <span>Bắt đầu luyện tập ngay</span>
                  <ArrowRight className="w-3.5 h-3.5" />
                </Link>
              </div>
            </div>
          ) : (
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-4">
              {/* REAL SVG SCORE PROGRESS CHART (7 COLS) */}
              <div data-testid="today-practice-chart" className="lg:col-span-7 rounded-lg border border-border bg-bg-primary p-4 space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-bold text-text-primary flex items-center gap-1.5">
                    <BarChart3 className="w-3.5 h-3.5 text-accent" />
                    Biểu đồ điểm số 7 ngày gần nhất (Dữ liệu thật)
                  </span>
                  <span className="text-[11px] text-text-tertiary">
                    {practiceStats.todayAttempts} lượt hôm nay
                  </span>
                </div>

                {/* SVG CHART */}
                <div className="w-full h-36">
                  <svg viewBox="0 0 420 120" className="w-full h-full">
                    {/* Grid lines */}
                    <line x1="30" y1="20" x2="410" y2="20" stroke="currentColor" strokeOpacity="0.1" strokeDasharray="3 3" />
                    <line x1="30" y1="60" x2="410" y2="60" stroke="currentColor" strokeOpacity="0.1" strokeDasharray="3 3" />
                    <line x1="30" y1="100" x2="410" y2="100" stroke="currentColor" strokeOpacity="0.1" />

                    <text x="5" y="24" fontSize="9" fill="currentColor" opacity="0.4">100</text>
                    <text x="5" y="64" fontSize="9" fill="currentColor" opacity="0.4">50</text>
                    <text x="15" y="104" fontSize="9" fill="currentColor" opacity="0.4">0</text>

                    {/* Bars / Points for last 7 days */}
                    {practiceStats.last7Days && practiceStats.last7Days.map((d, idx) => {
                      const x = 50 + idx * 52;
                      const barH = (d.avgScore / 100) * 80;
                      const y = 100 - barH;
                      const hasAttempts = d.count > 0;

                      return (
                        <g key={d.date}>
                          {hasAttempts ? (
                            <>
                              <rect
                                x={x - 12}
                                y={y}
                                width={24}
                                height={barH}
                                rx="4"
                                fill="#8b5cf6"
                                fillOpacity="0.8"
                              />
                              <text x={x} y={Math.max(16, y - 4)} textAnchor="middle" fontSize="9" fontWeight="bold" fill="#8b5cf6">
                                {Math.round(d.avgScore)}
                              </text>
                            </>
                          ) : (
                            <circle cx={x} cy="100" r="2.5" fill="currentColor" opacity="0.2" />
                          )}
                          <text x={x} y="115" textAnchor="middle" fontSize="9" fill="currentColor" opacity="0.6">
                            {d.dayName}
                          </text>
                        </g>
                      );
                    })}
                  </svg>
                </div>
              </div>

              {/* REVIEW HUB: ITEMS NEEDING PRACTICE (5 COLS) */}
              <div data-testid="today-practice-needs-review" className="lg:col-span-5 rounded-lg border border-border bg-bg-primary p-4 space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-bold text-text-primary flex items-center gap-1.5">
                    <Clock className="w-3.5 h-3.5 text-amber-500" />
                    Cần Luyện Tập Lại (Điểm &lt; 70)
                  </span>
                  <span className="text-[11px] font-semibold text-amber-600 dark:text-amber-400">
                    {practiceStats.needsReview.length} bài
                  </span>
                </div>

                {practiceStats.needsReview.length === 0 ? (
                  <div className="py-6 text-center space-y-1">
                    <CheckCircle2 className="w-5 h-5 text-emerald-500 mx-auto" />
                    <p className="text-xs font-semibold text-emerald-700 dark:text-emerald-300">
                      Tất cả các bài luyện tập gần đây đều đạt chuẩn!
                    </p>
                    <p className="text-[11px] text-text-tertiary">
                      Tiếp tục duy trì phong độ và thử thách các bài tập mới.
                    </p>
                  </div>
                ) : (
                  <div className="space-y-2 max-h-40 overflow-y-auto pr-1">
                    {practiceStats.needsReview.map((item, idx) => (
                      <div
                        key={item.activityKey || idx}
                        className="p-2.5 rounded-lg border border-amber-200 dark:border-amber-800/60 bg-amber-50/50 dark:bg-amber-950/20 text-xs flex items-center justify-between gap-2"
                      >
                        <div className="min-w-0 flex-1">
                          <p className="font-semibold text-text-primary truncate">
                            {item.activityTitle}
                          </p>
                          <span className="text-[11px] text-amber-700 dark:text-amber-300 font-mono">
                            Điểm gần nhất: {Math.round(item.score)}/100
                          </span>
                        </div>
                        <Link
                          href={buildPracticeLessonHref(
                            item.courseId,
                            item.lessonId,
                            item.activityType,
                            item.activityKey
                          )}
                          className="px-2.5 py-1 rounded bg-amber-600 hover:bg-amber-500 text-white text-[11px] font-bold shrink-0 transition"
                        >
                          Luyện lại &rarr;
                        </Link>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}
            </>
          )}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          {/* Card 1: Streak & Overall Course Pace */}
          <div className="rounded-lg border border-border bg-bg-secondary p-5 flex flex-col justify-between space-y-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2 text-xs font-medium uppercase tracking-wider text-text-tertiary">
                <TrendingUp className="w-3.5 h-3.5 text-amber-500" />
                <span>Nhịp độ học tập</span>
              </div>
              <span className="text-xs font-semibold text-amber-600 dark:text-amber-400">
                {user.streakDays} ngày
              </span>
            </div>

            <div>
              <div className="flex items-baseline justify-between mb-2">
                <p className="text-2xl font-bold text-text-primary">{avgCourseProgress}%</p>
                <span className="text-xs text-text-tertiary">
                  {totalCompletedAll}/{totalLessonsAll} bài hoàn thành
                </span>
              </div>
              <div className="w-full h-2 rounded-full bg-bg-tertiary overflow-hidden mb-3">
                <div
                  className="h-full rounded-full bg-accent transition-all"
                  style={{ width: `${Math.min(100, Math.max(2, avgCourseProgress))}%` }}
                />
              </div>
              <p className="text-[11px] text-text-tertiary mb-2">
                Chuỗi mục tiêu 7 ngày gần nhất ({streakFilledCount}/7 ngày đạt chuỗi):
              </p>
              <div className="grid grid-cols-7 gap-1.5">
                {["T2", "T3", "T4", "T5", "T6", "T7", "CN"].map((dayLabel, idx) => {
                  const active = idx < streakFilledCount;
                  return (
                    <div
                      key={dayLabel}
                      className={`py-1.5 rounded text-center text-[10px] font-medium border ${
                        active
                          ? "bg-amber-500/15 border-amber-500/30 text-amber-700 dark:text-amber-300"
                          : "bg-bg-primary border-border text-text-tertiary"
                      }`}
                    >
                      {dayLabel}
                    </div>
                  );
                })}
              </div>
            </div>
          </div>

          {/* Card 2: Flashcard Retention Breakdown */}
          <div className="rounded-lg border border-border bg-bg-secondary p-5 flex flex-col justify-between space-y-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2 text-xs font-medium uppercase tracking-wider text-text-tertiary">
                <BarChart3 className="w-3.5 h-3.5 text-emerald-500" />
                <span>Tỷ lệ thuộc thẻ</span>
              </div>
              <Link
                href="/app/review"
                className="text-xs font-medium text-accent hover:underline"
              >
                {reviewStation.totalDue} cần ôn
              </Link>
            </div>

            <div>
              <div className="flex items-baseline justify-between mb-2">
                <p className="text-2xl font-bold text-text-primary">{masteryPercent}%</p>
                <span className="text-xs text-text-tertiary">
                  Tổng {totalCardsSafe} thẻ ghi nhớ
                </span>
              </div>

              <div className="w-full h-2.5 rounded-full bg-bg-tertiary overflow-hidden flex mb-3">
                {totalCardsSafe > 0 && (
                  <>
                    <div
                      className="h-full bg-emerald-500"
                      style={{
                        width: `${Math.round((reviewStation.reviewCount / totalCardsSafe) * 100)}%`,
                      }}
                      title={`Thuộc vững: ${reviewStation.reviewCount}`}
                    />
                    <div
                      className="h-full bg-amber-500"
                      style={{
                        width: `${Math.round((reviewStation.learningCount / totalCardsSafe) * 100)}%`,
                      }}
                      title={`Đang học: ${reviewStation.learningCount}`}
                    />
                    <div
                      className="h-full bg-sky-500"
                      style={{
                        width: `${Math.round((reviewStation.newCount / totalCardsSafe) * 100)}%`,
                      }}
                      title={`Thẻ mới: ${reviewStation.newCount}`}
                    />
                  </>
                )}
              </div>

              <div className="grid grid-cols-3 gap-2 text-center">
                <div className="rounded-md bg-bg-primary border border-border px-2 py-1.5">
                  <p className="text-[10px] text-text-tertiary">Thuộc vững</p>
                  <p className="text-xs font-semibold text-emerald-600 dark:text-emerald-400">
                    {reviewStation.reviewCount}
                  </p>
                </div>
                <div className="rounded-md bg-bg-primary border border-border px-2 py-1.5">
                  <p className="text-[10px] text-text-tertiary">Đang củng cố</p>
                  <p className="text-xs font-semibold text-amber-600 dark:text-amber-400">
                    {reviewStation.learningCount}
                  </p>
                </div>
                <div className="rounded-md bg-bg-primary border border-border px-2 py-1.5">
                  <p className="text-[10px] text-text-tertiary">Thẻ mới</p>
                  <p className="text-xs font-semibold text-sky-600 dark:text-sky-400">
                    {reviewStation.newCount}
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Card 3: Quiz History & Personalized Recommendation */}
          <div className="rounded-lg border border-border bg-bg-secondary p-5 flex flex-col justify-between space-y-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2 text-xs font-medium uppercase tracking-wider text-text-tertiary">
                <ClipboardCheck className="w-3.5 h-3.5 text-accent" />
                <span>Lịch sử kiểm tra</span>
              </div>
              <span className="text-xs text-text-tertiary">{quizzes.length} bài</span>
            </div>

            {recentQuizzes.length > 0 ? (
              <div className="space-y-2">
                {recentQuizzes.map((q) => (
                  <Link
                    key={q.id}
                    href={q.courseId ? `/app/courses/${q.courseId}/quiz` : "/app/courses"}
                    className="flex items-center justify-between gap-2 rounded-md border border-border bg-bg-primary px-2.5 py-2 hover:border-accent/40 transition"
                  >
                    <span className="text-xs font-medium text-text-primary truncate">
                      {q.title}
                    </span>
                    <span className="text-[11px] font-semibold px-2 py-0.5 rounded shrink-0 bg-emerald-500/15 text-emerald-700 dark:text-emerald-300">
                      {q.correctItems}/{q.totalItems}
                    </span>
                  </Link>
                ))}
              </div>
            ) : (
              <EmptyState
                title="Chưa có bài kiểm tra đã nộp"
                description="Làm bài kiểm tra ngắn sau mỗi bài học để củng cố trí nhớ dài hạn."
                action={{
                  label: "Chọn khóa học",
                  href: "/app/courses",
                  testId: "dashboard-empty-quizzes-cta",
                }}
                className="py-4"
              />
            )}

            <div className="rounded-md border border-accent/25 bg-accent-subtle/60 px-3 py-2 flex items-start gap-2">
              <CheckCircle2 className="w-3.5 h-3.5 text-accent shrink-0 mt-0.5" />
              <div className="text-[11px] text-text-secondary leading-relaxed">
                <span className="font-semibold text-text-primary">Gợi ý bước tiếp: </span>
                {reviewStation.totalDue > 0 ? (
                  <>
                    Ôn {reviewStation.totalDue} thẻ đến hạn (~{reviewStation.estimatedMinutes} phút) trước khi học bài mới.
                  </>
                ) : nextLessonInfo ? (
                  <>
                    Hoàn thành bài &ldquo;{nextLessonInfo.title}&rdquo; và luyện trong Hub Thực hành.
                  </>
                ) : (
                  <>Chọn một khóa học bên dưới để củng cố kiến thức qua bài kiểm tra.</>
                )}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Recent Courses (max 4 deduplicated) */}
      <div className="mb-10">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <h2 className="text-base font-semibold text-text-primary">Khóa học của bạn</h2>
            <span className="text-xs font-medium px-2 py-0.5 rounded-md bg-bg-tertiary text-text-tertiary">
              {courses.length}
            </span>
          </div>
          <div className="flex items-center gap-3">
            {archivedCoursesCount > 0 && (
              <Link
                href="/app/courses?tab=archived"
                className="text-xs text-text-tertiary hover:text-accent transition inline-flex items-center gap-1"
                title="Xem các khóa học đã lưu trữ"
              >
                <Archive className="w-3.5 h-3.5" />
                <span>Đã lưu trữ ({archivedCoursesCount})</span>
              </Link>
            )}
            <Link
              href="/app/courses"
              className="text-sm font-medium text-text-secondary hover:text-accent transition inline-flex items-center gap-1"
            >
              <span>Xem tất cả khóa học</span>
              <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          </div>
        </div>

        {courses.length > 0 ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5">
            {courses.slice(0, 4).map((course) => {
              const id = getCourseId(course);
              const progress = getCourseProgress(course);
              const lessonCount = getCourseLessonCount(course);
              const theme = getCourseVisualTheme(course);
              const skillTags = extractCourseSkillTags(course);
              const isPinned = Boolean((course as { isPinned?: boolean }).isPinned);
              return (
                <Link
                  key={id}
                  href={`/app/courses/${id}`}
                  className="group rounded-xl border border-border bg-bg-primary hover:border-accent/60 hover:shadow-sm transition overflow-hidden flex flex-col justify-between min-h-[176px]"
                >
                  <div className={`h-1.5 w-full bg-gradient-to-r ${theme.accentBarClass}`} />
                  <div className="p-4 flex-1 flex flex-col justify-between gap-3">
                    <div>
                      <div className="flex items-center justify-between gap-1.5 mb-2">
                        <div className="flex items-center gap-1">
                          <span className={`inline-block text-[10px] font-semibold border rounded-md px-2 py-0.5 ${theme.badgeBgClass}`}>
                            {theme.domainLabel}
                          </span>
                          {isPinned && (
                            <span className="inline-flex items-center gap-0.5 text-[10px] font-semibold text-amber-700 dark:text-amber-300 bg-amber-50 dark:bg-amber-950/40 border border-amber-200 dark:border-amber-800 px-1.5 py-0.2 rounded">
                              <Pin className="w-2.5 h-2.5 fill-current text-amber-500" />
                              <span>Ghim</span>
                            </span>
                          )}
                        </div>
                        <span className="text-[11px] text-text-tertiary shrink-0">
                          {theme.estimatedHoursLabel}
                        </span>
                      </div>
                      <h3 className="text-sm font-semibold text-text-primary group-hover:text-accent transition line-clamp-2 leading-snug mb-2">
                        {course.title || "Khóa học"}
                      </h3>
                      <div className="flex flex-wrap gap-1">
                        {skillTags.slice(0, 2).map((tag) => (
                          <span
                            key={tag}
                            className="px-1.5 py-0.5 rounded bg-bg-secondary border border-border text-[10px] font-medium text-text-secondary"
                          >
                            {tag}
                          </span>
                        ))}
                      </div>
                    </div>

                    <div>
                      <div className="flex items-center justify-between text-xs text-text-tertiary mb-1.5">
                        <span>{lessonCount > 0 ? `${lessonCount} bài giảng` : "Chưa có bài"}</span>
                        <span
                          className={
                            progress === 100
                              ? "text-emerald-600 dark:text-emerald-400 font-semibold"
                              : "font-semibold text-text-primary"
                          }
                        >
                          {progress === 100 ? "Hoàn thành" : `${progress}%`}
                        </span>
                      </div>
                      <div className="w-full h-1.5 bg-bg-tertiary rounded-full overflow-hidden">
                        <div
                          className={`h-full rounded-full transition-all ${
                            progress === 100 ? "bg-emerald-500" : "bg-accent"
                          }`}
                          style={{ width: `${progress}%` }}
                        />
                      </div>
                    </div>
                  </div>
                </Link>
              );
            })}
          </div>
        ) : (
          <div className="rounded-lg border border-dashed border-border bg-bg-secondary p-8 text-center">
            <BookOpen className="w-7 h-7 text-text-tertiary mx-auto mb-2.5" />
            <p className="text-sm text-text-secondary mb-3">
              Bạn chưa có khóa học nào trong thư viện.
            </p>
            <Link
              href="/app/courses/new"
              className="inline-flex items-center gap-1.5 h-9 px-4 rounded-md bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC] text-sm font-medium transition shadow-xs"
            >
              <Plus className="w-4 h-4" />
              <span>Tạo khóa học mới</span>
            </Link>
          </div>
        )}
      </div>

      {/* Multi-Stage Learning Roadmaps & Real-World Use Cases */}
      <div className="mb-10">
        <LearningRoadmapsShowcase courses={courses} compact={true} />
      </div>

      {/* Workspace Shortcuts (Level 2 entry points) */}
      <div>
        <h2 className="text-xs font-semibold uppercase tracking-wider text-text-tertiary mb-3">
          Không gian học tập
        </h2>
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
          <Link
            href="/app/courses"
            className="rounded-lg border border-border bg-bg-secondary p-4 hover:border-accent/50 transition group"
          >
            <BookOpen className="w-4 h-4 text-text-tertiary group-hover:text-accent transition mb-2.5" />
            <p className="text-sm font-semibold text-text-primary">Khóa học</p>
            <p className="text-xs text-text-tertiary mt-0.5">{courses.length} khóa học</p>
          </Link>

          <Link
            href="/app/documents"
            className="rounded-lg border border-border bg-bg-secondary p-4 hover:border-accent/50 transition group"
          >
            <FileText className="w-4 h-4 text-text-tertiary group-hover:text-accent transition mb-2.5" />
            <p className="text-sm font-semibold text-text-primary">Tài liệu PDF</p>
            <p className="text-xs text-text-tertiary mt-0.5">{documentsCount} tài liệu</p>
          </Link>

          <Link
            href="/app/notes"
            className="rounded-lg border border-border bg-bg-secondary p-4 hover:border-accent/50 transition group"
          >
            <NotebookPen className="w-4 h-4 text-text-tertiary group-hover:text-accent transition mb-2.5" />
            <p className="text-sm font-semibold text-text-primary">Sổ tay</p>
            <p className="text-xs text-text-tertiary mt-0.5">{notebooksCount} sổ tay</p>
          </Link>

          <Link
            href="/app/review"
            className="rounded-lg border border-border bg-bg-secondary p-4 hover:border-accent/50 transition group"
          >
            <Brain className="w-4 h-4 text-text-tertiary group-hover:text-accent transition mb-2.5" />
            <p className="text-sm font-semibold text-text-primary">Ôn tập</p>
            <p className="text-xs text-text-tertiary mt-0.5">
              {reviewStation.totalDue} thẻ cần ôn
            </p>
          </Link>
        </div>
      </div>
    </div>
  );
}

