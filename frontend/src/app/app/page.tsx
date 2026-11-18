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
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached } from "@/lib/api-client";
import {
  classifyCourseDomain,
  deduplicateCourses,
  getCourseId,
  getCourseLessonCount,
  getCourseProgress,
  type CourseLike,
} from "@/lib/course-helpers";
import {
  getVietnameseGreetingName,
  getReviewCapSettings,
  type ReviewCapSettings,
} from "@/lib/review-settings";
import { EmptyState, Card, StatCard, ProgressBar, Badge } from "@/components/ui";

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
  const [quizzes, setQuizzes] = useState<DashboardQuizItem[]>([]);
  const [documentsCount, setDocumentsCount] = useState(0);
  const [notebooksCount, setNotebooksCount] = useState(0);
  const [nextLessonInfo, setNextLessonInfo] = useState<NextLessonInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    setFetchError(null);
    setNextLessonInfo(null);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );
      const [data, coursesResp] = await Promise.all([
        fetchDashboardCached({
          headers: effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
        }),
        fetch("/api/v1/courses", { headers }).catch(() => null),
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

      const extraCourses =
        coursesResp && coursesResp.ok ? await coursesResp.json().catch(() => null) : null;
      const rawCourses: CourseLike[] =
        Array.isArray(extraCourses) && extraCourses.length > 0
          ? extraCourses
          : Array.isArray(data.courses)
            ? data.courses
            : [];

      const unique = deduplicateCourses(rawCourses);
      setCourses(unique);

      const activeCourse =
        unique.find((c) => {
          const p = getCourseProgress(c);
          return p > 0 && p < 100;
        }) ||
        unique.find((c) => getCourseProgress(c) < 100) ||
        unique[0];

      const activeId = activeCourse ? getCourseId(activeCourse) : "";
      if (activeId) {
        const detailResp = await fetch(`/api/v1/courses/${activeId}`, { headers }).catch(() => null);
        if (detailResp && detailResp.ok) {
          const detail = await detailResp.json();
          const completedSet = new Set<string>(detail.completedLessonIds ?? []);
          const chapters: CourseDetailChapter[] = Array.isArray(detail.chapters)
            ? detail.chapters
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
        }
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

  const firstName = getVietnameseGreetingName(user.fullName);

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
      <div className="flex items-center justify-center h-full">
        <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pb-24 md:pb-10">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-text-primary">
            Chào {firstName}, hôm nay học gì?
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
            className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition"
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
        <div className="rounded-lg border border-border bg-bg-secondary p-5 flex flex-col justify-between">
          <div>
            <div className="flex items-center justify-between gap-2 mb-3">
              <div className="flex items-center gap-2 text-xs font-medium uppercase tracking-wider text-text-tertiary">
                <BookOpen className="w-3.5 h-3.5 text-accent" />
                <span>Tiếp tục lộ trình</span>
              </div>
              {continueCourse && (
                <span className="text-xs px-2 py-0.5 rounded-md bg-bg-tertiary text-text-secondary">
                  {classifyCourseDomain(continueCourse).label}
                </span>
              )}
            </div>

            {continueCourse ? (
              <>
                <h2 className="text-base font-semibold text-text-primary mb-1 line-clamp-2">
                  {continueCourse.title}
                </h2>
                {nextLessonInfo ? (
                  <p className="text-sm text-text-secondary mb-4 line-clamp-1">
                    Bài tiếp theo: <span className="text-text-primary font-medium">{nextLessonInfo.title}</span>
                    {nextLessonInfo.minutes ? ` (${nextLessonInfo.minutes}p)` : ""}
                  </p>
                ) : (
                  <p className="text-sm text-text-tertiary mb-4">
                    {getCourseLessonCount(continueCourse)} bài giảng trong lộ trình
                  </p>
                )}

                <div className="mb-4">
                  <div className="flex items-center justify-between text-xs text-text-tertiary mb-1.5">
                    <span>Tiến độ khóa học</span>
                    <span className="font-medium text-text-secondary">{continueProgress}%</span>
                  </div>
                  <div className="w-full h-1.5 bg-bg-tertiary rounded-full overflow-hidden">
                    <div
                      className={`h-full rounded-full transition-all ${
                        continueProgress === 100 ? "bg-emerald-500" : "bg-accent"
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
                  className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition"
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
                className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition"
              >
                <Plus className="w-4 h-4" />
                <span>Tạo khóa học đầu tiên</span>
              </Link>
            )}
          </div>
        </div>

        {/* Card 2: Today's Memory Review */}
        <div className="rounded-lg border border-border bg-bg-secondary p-5 flex flex-col justify-between">
          <div>
            <div className="flex items-center justify-between gap-2 mb-3">
              <div className="flex items-center gap-2 text-xs font-medium uppercase tracking-wider text-text-tertiary">
                <Brain className="w-3.5 h-3.5 text-accent" />
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

            <div className="flex items-center gap-2 mb-4 text-xs">
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
                  ? "bg-accent text-white hover:bg-accent-hover shadow-xs"
                  : "border border-border bg-bg-primary text-text-primary hover:bg-bg-tertiary"
              }`}
            >
              <span>{cappedDue > 0 ? "Bắt đầu ôn tập" : "Mở phòng ôn tập"}</span>
              <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          </div>
        </div>
      </div>

      {/* Learning Statistics & Personalized Roadmap */}
      <section data-testid="today-stats-roadmap" className="mb-10">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-text-primary">
            Thống kê học tập &amp; Lộ trình cá nhân hóa
          </h2>
          <span className="text-xs text-text-tertiary">
            Cập nhật trực tiếp từ tiến độ của bạn
          </span>
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
                    Hoàn thành bài &ldquo;{nextLessonInfo.title}&rdquo; và thử tab Thực hành.
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
          <Link
            href="/app/courses"
            className="text-sm font-medium text-text-secondary hover:text-accent transition inline-flex items-center gap-1"
          >
            <span>Xem tất cả khóa học</span>
            <ArrowRight className="w-3.5 h-3.5" />
          </Link>
        </div>

        {courses.length > 0 ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
            {courses.slice(0, 4).map((course) => {
              const id = getCourseId(course);
              const progress = getCourseProgress(course);
              const lessonCount = getCourseLessonCount(course);
              const domain = classifyCourseDomain(course);
              return (
                <Link
                  key={id}
                  href={`/app/courses/${id}`}
                  className="group rounded-lg border border-border bg-bg-primary p-4 hover:border-accent/60 transition flex flex-col justify-between min-h-[144px]"
                >
                  <div>
                    <span className="inline-block text-[11px] font-medium text-text-tertiary bg-bg-tertiary rounded-md px-2 py-0.5 mb-2.5">
                      {domain.label}
                    </span>
                    <h3 className="text-sm font-semibold text-text-primary group-hover:text-accent transition line-clamp-2">
                      {course.title || "Khóa học"}
                    </h3>
                  </div>

                  <div className="mt-4">
                    <div className="flex items-center justify-between text-xs text-text-tertiary mb-1.5">
                      <span>{lessonCount > 0 ? `${lessonCount} bài` : "Chưa có bài"}</span>
                      <span
                        className={
                          progress === 100
                            ? "text-emerald-600 dark:text-emerald-400 font-medium"
                            : "font-medium text-text-secondary"
                        }
                      >
                        {progress === 100 ? "Hoàn thành" : `${progress}%`}
                      </span>
                    </div>
                    <div className="w-full h-1 bg-bg-tertiary rounded-full overflow-hidden">
                      <div
                        className={`h-full rounded-full transition-all ${
                          progress === 100 ? "bg-emerald-500" : "bg-accent"
                        }`}
                        style={{ width: `${progress}%` }}
                      />
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
              className="inline-flex items-center gap-1.5 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition"
            >
              <Plus className="w-4 h-4" />
              <span>Tạo khóa học mới</span>
            </Link>
          </div>
        )}
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

