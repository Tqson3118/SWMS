"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import {
  AlertCircle,
  ArrowRight,
  BarChart3,
  BookOpen,
  Brain,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  ChevronUp,
  ClipboardCheck,
  Clock,
  Flame,
  Layers,
  Play,
  RefreshCw,
  Search,
  Sparkles,
  Target,
  X,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { Badge, Card, ProgressBar, StatCard } from "@/components/ui";
import {
  getCourseVisualTheme,
  normalizeVi,
  stripChapterPrefix,
} from "@/lib/course-helpers";
import {
  buildPracticeLessonHref,
  getPracticeActivityTypeLabel,
} from "@/lib/practice-helpers";

interface NeedsReviewItem {
  activityKey: string;
  activityTitle: string;
  activityType: string;
  courseId: string;
  courseTitle?: string | null;
  lessonId?: string | null;
  lessonTitle?: string | null;
  score: number;
  maxScore?: number;
  passingScore?: number;
  attemptCount?: number;
  createdAt: string;
}

interface ChartDayPoint {
  date: string;
  dayName: string;
  count: number;
  avgScore: number;
}

interface PracticeLessonSummary {
  lessonId: string;
  title: string;
  chapterTitle: string;
  activityCount: number;
  passedCount: number;
  attemptCount?: number;
  hasQuiz?: boolean;
  quizId?: string | null;
}

interface PracticeCourseSummary {
  courseId: string;
  title: string;
  slug: string;
  totalActivities: number;
  passedActivities: number;
  averageScore: number;
  attemptCount?: number;
  lastAttemptAt?: string | null;
  quizCount?: number;
  lessons: PracticeLessonSummary[];
}

interface PracticeHubStats {
  totalAttempts: number;
  averageScore: number;
  passedCount: number;
  todayAttempts: number;
  streakDays: number;
  fsrsDueCount?: number;
  needsReview: NeedsReviewItem[];
  chartData?: ChartDayPoint[];
  last7Days?: ChartDayPoint[];
  courseSummaries?: PracticeCourseSummary[];
}

type CoursePracticeFilter = "all" | "in_progress" | "not_started";

function formatShortDateVi(iso?: string | null): string {
  if (!iso) return "Chưa luyện";
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return "Chưa luyện";
  return d.toLocaleDateString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

export default function PracticeHubPage() {
  const { activeUserHeaderId } = useWorkspaceStore();
  const [stats, setStats] = useState<PracticeHubStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const [searchQuery, setSearchQuery] = useState("");
  const [courseFilter, setCourseFilter] = useState<CoursePracticeFilter>("all");
  const [expandedCourseIds, setExpandedCourseIds] = useState<Set<string>>(new Set());
  const [expandedChapterKeys, setExpandedChapterKeys] = useState<Set<string>>(new Set());
  const [selectedDayIdx, setSelectedDayIdx] = useState<number | null>(null);

  const fetchHubStats = useCallback(async () => {
    setIsLoading(true);
    setFetchError(null);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );

      const resp = await fetch("/api/v1/practice/stats", { headers });
      if (!resp.ok) {
        setFetchError(`Không thể tải dữ liệu luyện tập từ máy chủ (HTTP ${resp.status}).`);
        return;
      }

      const data = await resp.json().catch(() => null);
      if (!data || typeof data.totalAttempts !== "number") {
        setFetchError("Dữ liệu thống kê luyện tập trả về không hợp lệ.");
        return;
      }

      setStats(data as PracticeHubStats);

      const summaries = Array.isArray(data.courseSummaries)
        ? (data.courseSummaries as PracticeCourseSummary[])
        : [];
      if (summaries.length > 0) {
        setExpandedCourseIds((prev) => {
          if (prev.size > 0) return prev;
          const firstWithLessons = summaries.find((c) => c.lessons?.length > 0);
          return firstWithLessons ? new Set([firstWithLessons.courseId]) : new Set();
        });
      }
    } catch {
      setFetchError("Không thể kết nối tới máy chủ để tải Hub Luyện tập.");
    } finally {
      setIsLoading(false);
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    void fetchHubStats();
  }, [fetchHubStats]);

  useEffect(() => {
    if (typeof window === "undefined") return;
    const onMutated = () => {
      void fetchHubStats();
    };
    window.addEventListener("swms:data-mutated", onMutated);
    return () => window.removeEventListener("swms:data-mutated", onMutated);
  }, [fetchHubStats]);

  const chartPoints = useMemo<ChartDayPoint[]>(() => {
    if (!stats) return [];
    return stats.chartData ?? stats.last7Days ?? [];
  }, [stats]);

  const courseSummaries = useMemo<PracticeCourseSummary[]>(() => {
    return Array.isArray(stats?.courseSummaries) ? stats.courseSummaries : [];
  }, [stats]);

  const firstLessonByCourseId = useMemo(() => {
    const map = new Map<string, string>();
    for (const c of courseSummaries) {
      if (c.lessons && c.lessons.length > 0) {
        map.set(c.courseId, c.lessons[0].lessonId);
      }
    }
    return map;
  }, [courseSummaries]);

  const filteredCourses = useMemo(() => {
    const q = normalizeVi(searchQuery);
    return courseSummaries.filter((course) => {
      const attempted = (course.attemptCount ?? 0) > 0 || course.passedActivities > 0;
      if (courseFilter === "in_progress" && !attempted) return false;
      if (courseFilter === "not_started" && attempted) return false;

      if (!q) return true;
      const inCourseTitle = normalizeVi(course.title).includes(q);
      const inLessons = (course.lessons ?? []).some(
        (l) => normalizeVi(l.title).includes(q) || normalizeVi(l.chapterTitle).includes(q)
      );
      return inCourseTitle || inLessons;
    });
  }, [courseSummaries, searchQuery, courseFilter]);

  const toggleCourseExpanded = (courseId: string) => {
    setExpandedCourseIds((prev) => {
      const next = new Set(prev);
      if (next.has(courseId)) next.delete(courseId);
      else next.add(courseId);
      return next;
    });
  };

  const toggleChapterExpanded = (key: string, defaultOpen: boolean) => {
    setExpandedChapterKeys((prev) => {
      const next = new Set(prev);
      const isOpenNow = prev.has(key) ? true : defaultOpen && !prev.has(`closed:${key}`);
      if (isOpenNow) {
        next.delete(key);
        next.add(`closed:${key}`);
      } else {
        next.delete(`closed:${key}`);
        next.add(key);
      }
      return next;
    });
  };

  const isChapterExpanded = (key: string, defaultOpen: boolean) => {
    if (searchQuery.trim().length > 0) return true;
    if (expandedChapterKeys.has(key)) return true;
    if (expandedChapterKeys.has(`closed:${key}`)) return false;
    return defaultOpen;
  };

  if (isLoading) {
    return (
      <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div className="space-y-2">
            <div className="h-4 w-48 rounded-md swms-skeleton" />
            <div className="h-8 w-56 rounded-lg swms-skeleton" />
            <div className="h-4 w-80 max-w-full rounded-md swms-skeleton" />
          </div>
          <div className="flex items-center gap-2">
            <div className="h-9 w-36 rounded-[10px] swms-skeleton" />
            <div className="h-9 w-24 rounded-[10px] swms-skeleton" />
            <div className="h-9 w-32 rounded-[10px] swms-skeleton" />
          </div>
        </div>
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3.5">
          {[0, 1, 2, 3].map((i) => (
            <div key={i} className="h-28 rounded-[14px] border border-[var(--border-calm)] swms-skeleton" />
          ))}
        </div>
        <div className="h-52 rounded-[14px] border border-[var(--border-calm)] swms-skeleton" />
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-4">
          <div className="lg:col-span-8 h-60 rounded-[14px] border border-[var(--border-calm)] swms-skeleton" />
          <div className="lg:col-span-4 h-60 rounded-[14px] border border-[var(--border-calm)] swms-skeleton" />
        </div>
        <div className="h-64 rounded-[14px] border border-[var(--border-calm)] swms-skeleton" />
      </div>
    );
  }

  const fsrsDueCount = stats?.fsrsDueCount ?? 0;
  const needsReview = stats?.needsReview ?? [];
  const totalAttempts = stats?.totalAttempts ?? 0;
  const passedCount = stats?.passedCount ?? 0;

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh] space-y-8">
      {/* Page Header */}
      <header className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div className="space-y-1">
          <div className="inline-flex items-center gap-1.5 text-xs font-semibold text-[var(--accent)]">
            <Target className="w-4 h-4" />
            <span>Trung tâm thực hành &amp; phản xạ chủ động</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-[var(--text-primary)]">
            Hub Luyện tập
          </h1>
          <p className="text-sm text-[var(--text-secondary)]">
            Thực hành theo từng bài học, ôn lại các câu chưa đạt và luyện thẻ FSRS hằng ngày.
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <Link
            href="/app/exams"
            className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-[10px] border border-purple-500/30 bg-purple-500/10 hover:bg-purple-500/20 text-purple-700 dark:text-purple-300 text-xs font-semibold transition shadow-xs"
          >
            <ClipboardCheck className="w-4 h-4" />
            <span>Ngân hàng Đề &amp; Thi thử</span>
          </Link>
          <button
            type="button"
            onClick={() => void fetchHubStats()}
            className="inline-flex items-center gap-1.5 h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-medium text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            <span>Làm mới</span>
          </button>
          <Link
            href="/app/review"
            className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-[10px] bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC] text-xs font-semibold transition shadow-xs"
          >
            <Brain className="w-4 h-4" />
            <span>Ôn thẻ FSRS ({fsrsDueCount})</span>
          </Link>
        </div>
      </header>

      {/* Error State (Zero-Facade) */}
      {fetchError && (
        <div
          data-testid="practice-hub-error"
          className="rounded-[14px] border border-rose-500/30 bg-rose-500/5 p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3"
        >
          <div className="flex items-center gap-2.5 text-sm text-rose-600 dark:text-rose-400">
            <AlertCircle className="w-5 h-5 shrink-0" />
            <span>{fetchError}</span>
          </div>
          <button
            type="button"
            onClick={() => void fetchHubStats()}
            className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-semibold text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer self-start sm:self-auto"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            <span>Thử lại</span>
          </button>
        </div>
      )}

      {/* 1. Today's Stats & 7-Day Progress Chart */}
      {!fetchError && stats && (
        <section data-testid="practice-hub-stats" className="space-y-4">
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-3.5">
            <StatCard
              title="Điểm trung bình"
              value={`${stats.averageScore} / 100`}
              subtitle={totalAttempts > 0 ? `Trên ${totalAttempts} lượt luyện` : "Chưa có lượt chấm"}
              icon={Sparkles}
              variant="brand"
            />
            <StatCard
              title="Lượt luyện hôm nay"
              value={`${stats.todayAttempts} lượt`}
              subtitle={`Tổng cộng ${totalAttempts} lượt`}
              icon={Target}
              variant="default"
            />
            <StatCard
              title="Chuỗi ngày luyện tập"
              value={`${stats.streakDays} ngày`}
              subtitle={stats.streakDays > 0 ? "Giữ nhịp đều mỗi ngày" : "Luyện 1 bài để khởi động"}
              icon={Flame}
              variant="streak"
            />
            <StatCard
              title="Số bài đã đạt"
              value={`${passedCount} / ${totalAttempts}`}
              subtitle={
                totalAttempts > 0
                  ? `Tỷ lệ đạt ${Math.round((passedCount / totalAttempts) * 100)}%`
                  : "Chưa có lượt đạt"
              }
              icon={CheckCircle2}
              variant="success"
            />
          </div>

          <Card className="p-4 sm:p-5 bg-[var(--surface-card)] border-[var(--border-calm)] shadow-elevation-1">
            <div className="flex flex-wrap items-center justify-between gap-2 mb-4">
              <div className="flex items-center gap-2 min-w-0">
                <BarChart3 className="w-4 h-4 text-[var(--accent)] shrink-0" />
                <h2 className="text-sm font-semibold text-[var(--text-primary)] truncate">
                  Biểu đồ tiến độ 7 ngày gần nhất
                </h2>
              </div>
              <span
                data-testid="practice-chart-readout"
                aria-live="polite"
                className="text-xs text-[var(--text-tertiary)]"
              >
                {selectedDayIdx !== null && chartPoints[selectedDayIdx]
                  ? `Ngày ${chartPoints[selectedDayIdx].date} · ${chartPoints[selectedDayIdx].count} lượt · Điểm TB ${Math.round(chartPoints[selectedDayIdx].avgScore)}/100`
                  : `${stats.todayAttempts} lượt hôm nay · Cập nhật từ lịch sử học tập`}
              </span>
            </div>

            {totalAttempts === 0 ? (
              <div className="py-6 px-4 text-center rounded-[12px] border border-dashed border-[var(--border-calm)] bg-[var(--surface-elevated)]/40">
                <p className="text-sm font-medium text-[var(--text-primary)]">
                  Chưa có hoạt động luyện tập nào trong 7 ngày qua
                </p>
                <p className="text-xs text-[var(--text-tertiary)] mt-1">
                  Hãy chọn một bài học bên dưới để bắt đầu thực hành và theo dõi tiến độ của bạn.
                </p>
              </div>
            ) : (
              <div className="w-full h-40 overflow-hidden">
                <svg
                  viewBox="0 0 490 130"
                  preserveAspectRatio="xMidYMid meet"
                  className="w-full h-full max-w-full"
                  role="img"
                  aria-label="Biểu đồ điểm luyện tập 7 ngày gần nhất"
                >
                  <defs>
                    <linearGradient id="practiceHubBarGrad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#6366F1" />
                      <stop offset="100%" stopColor="#8B5CF6" stopOpacity="0.82" />
                    </linearGradient>
                  </defs>
                  <line x1="34" y1="18" x2="474" y2="18" stroke="currentColor" strokeOpacity="0.12" strokeDasharray="3 3" />
                  <line x1="34" y1="58" x2="474" y2="58" stroke="currentColor" strokeOpacity="0.12" strokeDasharray="3 3" />
                  <line x1="34" y1="98" x2="474" y2="98" stroke="currentColor" strokeOpacity="0.16" />

                  <text x="6" y="22" fontSize="10" fill="currentColor" opacity="0.5">100</text>
                  <text x="12" y="62" fontSize="10" fill="currentColor" opacity="0.5">50</text>
                  <text x="18" y="102" fontSize="10" fill="currentColor" opacity="0.5">0</text>

                  {chartPoints.map((d, idx) => {
                    const x = 64 + idx * 62;
                    const clampedAvg = Math.max(0, Math.min(100, d.avgScore));
                    const barH = (clampedAvg / 100) * 80;
                    const y = 98 - barH;
                    const hasAttempts = d.count > 0;
                    const isSelected = selectedDayIdx === idx;
                    const isDimmed = selectedDayIdx !== null && !isSelected;

                    return (
                      <g
                        key={d.date}
                        tabIndex={0}
                        role="button"
                        aria-label={`Ngày ${d.date} · ${d.count} lượt · Điểm TB ${Math.round(d.avgScore)}/100`}
                        className="cursor-pointer focus:outline-none"
                        onMouseEnter={() => setSelectedDayIdx(idx)}
                        onMouseLeave={() => setSelectedDayIdx(null)}
                        onFocus={() => setSelectedDayIdx(idx)}
                        onBlur={() => setSelectedDayIdx(null)}
                        onClick={() => setSelectedDayIdx((prev) => (prev === idx ? null : idx))}
                        onKeyDown={(e) => {
                          if (e.key === "Enter" || e.key === " ") {
                            e.preventDefault();
                            setSelectedDayIdx((prev) => (prev === idx ? null : idx));
                          }
                        }}
                      >
                        <title>{`${d.date}: ${d.count} lượt, TB ${d.avgScore}/100`}</title>
                        <rect
                          x={x - 24}
                          y={8}
                          width={48}
                          height={120}
                          fill="transparent"
                        />
                        {hasAttempts ? (
                          <>
                            <rect
                              x={x - 14}
                              y={y}
                              width={28}
                              height={Math.max(4, barH)}
                              rx="5"
                              fill="url(#practiceHubBarGrad)"
                              fillOpacity={isDimmed ? 0.45 : 1}
                              stroke={isSelected ? "var(--accent)" : "none"}
                              strokeWidth={isSelected ? 1.5 : 0}
                            />
                            <text
                              x={x}
                              y={Math.max(12, y - 5)}
                              textAnchor="middle"
                              fontSize="10"
                              fontWeight="600"
                              fill="var(--accent)"
                              opacity={isDimmed ? 0.55 : 1}
                            >
                              {Math.round(d.avgScore)}
                            </text>
                          </>
                        ) : (
                          <circle
                            cx={x}
                            cy="98"
                            r={isSelected ? 4.5 : 3}
                            fill="currentColor"
                            opacity={isSelected ? 0.6 : 0.22}
                          />
                        )}
                        <text x={x} y="114" textAnchor="middle" fontSize="10" fill="currentColor" opacity={isSelected ? 1 : 0.68} fontWeight={isSelected ? "700" : "400"}>
                          {d.dayName}
                        </text>
                        <text x={x} y="126" textAnchor="middle" fontSize="8.5" fill="currentColor" opacity="0.45">
                          {hasAttempts ? `${d.count} lượt` : ""}
                        </text>
                      </g>
                    );
                  })}
                </svg>
              </div>
            )}
          </Card>
        </section>
      )}

      {/* Compact Banner -> /app/exams */}
      {!fetchError && stats && (
        <div className="rounded-[14px] border border-purple-500/25 bg-purple-500/5 dark:bg-purple-950/20 p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div className="flex items-center gap-3 min-w-0">
            <div className="w-9 h-9 rounded-[10px] bg-purple-500/15 text-purple-600 dark:text-purple-300 flex items-center justify-center shrink-0">
              <ClipboardCheck className="w-5 h-5" />
            </div>
            <p className="text-xs sm:text-sm font-medium text-[var(--text-primary)]">
              Cần luyện trắc nghiệm theo chương hoặc thi thử tổng hợp cuối khóa?
            </p>
          </div>
          <Link
            href="/app/exams"
            data-testid="practice-go-exams-banner-link"
            className="inline-flex items-center justify-center gap-1.5 h-9 px-3.5 rounded-[10px] bg-purple-600 hover:bg-purple-700 text-white text-xs font-semibold shrink-0 transition shadow-xs"
          >
            <span>Mở Ngân hàng Đề &amp; Thi Thử</span>
            <ArrowRight className="w-3.5 h-3.5" />
          </Link>
        </div>
      )}

      {/* 2. What to Review Today: Active Recall Queue + FSRS */}
      {!fetchError && stats && (
        <section data-testid="practice-hub-review-queue" className="space-y-3">
          <div className="flex items-center justify-between gap-2">
            <h2 className="text-lg font-semibold text-[var(--text-primary)]">
              Hôm nay cần ôn gì
            </h2>
            <span className="text-xs text-[var(--text-tertiary)]">
              Tự động cập nhật khi bạn làm lại đạt chuẩn
            </span>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-12 gap-4">
            {/* Active Recall Queue */}
            <Card className="lg:col-span-8 p-4 sm:p-5 bg-[var(--surface-card)] border-[var(--border-calm)] shadow-elevation-1 flex flex-col justify-between gap-4 min-w-0">
              <div className="flex flex-wrap items-center justify-between gap-2 pb-3 border-b border-[var(--border-calm)]">
                <div className="flex items-center gap-2 min-w-0">
                  <Clock className="w-4 h-4 text-amber-500 shrink-0" />
                  <h3 className="text-sm font-semibold text-[var(--text-primary)] truncate">
                    Hàng đợi ôn câu sai &amp; bài cần củng cố (Active Recall)
                  </h3>
                </div>
                <Badge variant={needsReview.length > 0 ? "warning" : "success"} size="sm">
                  {needsReview.length} mục cần ôn
                </Badge>
              </div>

              {needsReview.length === 0 ? (
                <div className="py-8 px-4 text-center rounded-[12px] border border-dashed border-[var(--border-calm)] bg-[var(--surface-elevated)]/40 space-y-2">
                  <CheckCircle2 className="w-7 h-7 text-emerald-500 mx-auto" />
                  <p className="text-sm font-semibold text-[var(--text-primary)]">
                    Chưa có bài sai nào đang chờ ôn lại
                  </p>
                  <p className="text-xs text-[var(--text-secondary)] max-w-md mx-auto">
                    Mọi bài bạn đã luyện đều đạt chuẩn, hoặc hãy chọn một khóa học bên dưới để bắt đầu thử thách mới.
                  </p>
                </div>
              ) : (
                <div className="space-y-2.5 max-h-[340px] overflow-y-auto swms-thin-scrollbar pr-1">
                  {needsReview.map((item) => {
                    const targetLessonId =
                      item.lessonId || firstLessonByCourseId.get(item.courseId) || null;
                    const retryHref = targetLessonId
                      ? buildPracticeLessonHref(
                          item.courseId,
                          targetLessonId,
                          item.activityType,
                          item.activityKey
                        )
                      : `/app/courses/${item.courseId}/learn`;

                    return (
                      <div
                        key={`${item.courseId}-${item.activityKey}`}
                        className="p-3.5 rounded-[12px] border border-amber-500/25 dark:border-amber-500/20 bg-amber-50/50 dark:bg-amber-950/15 flex flex-col sm:flex-row sm:items-center justify-between gap-3"
                      >
                        <div className="min-w-0 space-y-1">
                          <div className="flex flex-wrap items-center gap-1.5">
                            <Badge variant="warning" size="sm">
                              {getPracticeActivityTypeLabel(item.activityType)}
                            </Badge>
                            {item.courseTitle && (
                              <span className="text-[11px] font-medium text-[var(--text-secondary)] truncate max-w-full sm:max-w-[260px]">
                                {item.courseTitle}
                              </span>
                            )}
                          </div>
                          <p className="text-sm font-semibold text-[var(--text-primary)] truncate">
                            {item.activityTitle}
                          </p>
                          <div className="flex flex-wrap items-center gap-2 text-xs text-[var(--text-tertiary)]">
                            {item.lessonTitle && (
                              <span className="truncate max-w-full sm:max-w-[260px]">{item.lessonTitle}</span>
                            )}
                            <span>·</span>
                            <span className="font-mono font-semibold text-amber-700 dark:text-amber-300">
                              Điểm gần nhất: {Math.round(item.score)}/{item.maxScore ?? 100}
                            </span>
                            {typeof item.attemptCount === "number" && (
                              <>
                                <span>·</span>
                                <span>{item.attemptCount} lượt thử</span>
                              </>
                            )}
                          </div>
                        </div>

                        <Link
                          href={retryHref}
                          className="inline-flex items-center justify-center gap-1.5 h-9 px-3.5 rounded-[10px] bg-amber-600 hover:bg-amber-500 text-white text-xs font-semibold shrink-0 transition shadow-xs"
                        >
                          <span>Ôn lại ngay</span>
                          <ArrowRight className="w-3.5 h-3.5" />
                        </Link>
                      </div>
                    );
                  })}
                </div>
              )}
            </Card>

            {/* FSRS Review Card */}
            <Card className="lg:col-span-4 p-4 sm:p-5 bg-[var(--surface-card)] border-[var(--border-calm)] shadow-elevation-1 flex flex-col justify-between gap-5 min-w-0">
              <div className="space-y-3">
                <div className="flex items-center justify-between gap-2">
                  <div className="flex items-center gap-2 text-xs font-semibold uppercase tracking-wider text-[var(--text-tertiary)]">
                    <Brain className="w-4 h-4 text-[var(--accent)]" />
                    <span>Ôn thẻ ghi nhớ FSRS</span>
                  </div>
                  <Badge variant={fsrsDueCount > 0 ? "brand" : "neutral"} size="sm">
                    {fsrsDueCount > 0 ? "Đến hạn" : "Đã xong"}
                  </Badge>
                </div>

                <div>
                  <p className="text-3xl font-bold text-[var(--text-primary)] tracking-tight">
                    {fsrsDueCount} thẻ
                  </p>
                  <p className="text-xs text-[var(--text-secondary)] mt-1 leading-relaxed">
                    {fsrsDueCount > 0
                      ? `Bạn có ${fsrsDueCount} thẻ FSRS đang đến hạn ôn tập hôm nay để khóa chặt trí nhớ dài hạn.`
                      : "Hiện không có thẻ nào quá hạn. Bạn có thể mở hàng ôn bất cứ lúc nào để ôn trước hạn."}
                  </p>
                </div>
              </div>

              <Link
                href="/app/review"
                className="inline-flex items-center justify-center gap-2 h-10 px-4 rounded-[10px] bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC] text-xs font-semibold transition shadow-xs"
              >
                <span>Mở hàng ôn FSRS</span>
                <ArrowRight className="w-3.5 h-3.5" />
              </Link>
            </Card>
          </div>
        </section>
      )}

      {/* 3. Courses & Lessons Picker */}
      {!fetchError && stats && (
        <section data-testid="practice-hub-courses" className="space-y-4">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            <div>
              <h2 className="text-lg font-semibold text-[var(--text-primary)]">
                Chọn khóa học &amp; bài học để luyện tập &amp; giải đề
              </h2>
              <p className="text-xs text-[var(--text-secondary)] mt-0.5">
                Chọn trực tiếp một bài học để mở phòng thực hành, hoặc chọn Giải đề &amp; Thi thử để vào phòng thi.
              </p>
            </div>

            <div className="flex flex-wrap items-center gap-2 w-full sm:w-auto min-w-0">
              <div className="relative w-full sm:w-64">
                <Search className="w-4 h-4 text-[var(--text-tertiary)] absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Tìm khóa học hoặc bài học..."
                  aria-label="Tìm khóa học hoặc bài học để luyện tập"
                  className="w-full h-9 pl-9 pr-8 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[var(--accent)]"
                />
                {searchQuery && (
                  <button
                    type="button"
                    onClick={() => setSearchQuery("")}
                    aria-label="Xóa từ khóa tìm kiếm"
                    className="absolute right-2.5 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                )}
              </div>

              <div className="inline-flex max-w-full overflow-x-auto no-scrollbar rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] p-0.5 text-xs">
                {(
                  [
                    { id: "all", label: "Tất cả" },
                    { id: "in_progress", label: "Đã luyện" },
                    { id: "not_started", label: "Chưa luyện" },
                  ] as const
                ).map((tab) => (
                  <button
                    key={tab.id}
                    type="button"
                    onClick={() => setCourseFilter(tab.id)}
                    className={`h-8 px-2.5 rounded-[8px] font-medium whitespace-nowrap transition cursor-pointer ${
                      courseFilter === tab.id
                        ? "bg-[#5E51E8] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] shadow-xs"
                        : "text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]/60"
                    }`}
                  >
                    {tab.label}
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Quick Quiz Shortcut Suggestion */}
          {courseSummaries.length > 0 && (
            <div className="flex flex-wrap items-center gap-2 p-3 rounded-[12px] border border-[var(--border-calm)] bg-[var(--surface-card)] shadow-elevation-1 text-xs">
              <span className="inline-flex items-center gap-1.5 font-semibold text-[var(--text-secondary)]">
                <ClipboardCheck className="w-4 h-4 text-purple-600 dark:text-purple-400" />
                <span>Vào nhanh phòng Giải đề:</span>
              </span>
              <div className="flex flex-wrap items-center gap-1.5 min-w-0">
                {courseSummaries.map((c) => (
                  <Link
                    key={`hub-quiz-link-${c.courseId}`}
                    href={`/app/courses/${c.courseId}/quiz`}
                    className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-[8px] border border-purple-500/20 bg-purple-500/5 hover:bg-purple-500/15 text-purple-700 dark:text-purple-300 font-medium transition max-w-full"
                  >
                    <span className="truncate max-w-[160px]">{c.title}</span>
                    {(c.quizCount ?? 0) > 0 && (
                      <span className="text-[10px] font-bold px-1.5 py-0.2 rounded-full bg-purple-500/20 shrink-0">
                        {c.quizCount} đề
                      </span>
                    )}
                  </Link>
                ))}
              </div>
            </div>
          )}

          {filteredCourses.length === 0 ? (
            <Card className="p-8 text-center bg-[var(--surface-card)] border-[var(--border-calm)] shadow-elevation-1 space-y-3">
              <div className="w-12 h-12 rounded-2xl bg-[var(--surface-elevated)] text-[var(--text-tertiary)] flex items-center justify-center mx-auto">
                <BookOpen className="w-6 h-6" />
              </div>
              <div className="space-y-1">
                <p className="text-sm font-semibold text-[var(--text-primary)]">
                  Không tìm thấy khóa học phù hợp
                </p>
                <p className="text-xs text-[var(--text-secondary)] max-w-md mx-auto">
                  {searchQuery.trim() || courseFilter !== "all"
                    ? "Thử xóa bộ lọc hoặc thay đổi từ khóa tìm kiếm để xem lại danh sách khóa học."
                    : "Chưa có khóa học nào trong hệ thống. Hãy tạo khóa học mới trong mục Khóa học."}
                </p>
              </div>
              {(searchQuery.trim() || courseFilter !== "all") && (
                <div className="pt-1">
                  <button
                    type="button"
                    onClick={() => {
                      setSearchQuery("");
                      setCourseFilter("all");
                    }}
                    className="inline-flex items-center gap-1.5 h-9 px-4 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] hover:bg-[var(--surface-card)] text-xs font-semibold text-[var(--text-primary)] transition cursor-pointer"
                  >
                    <X className="w-3.5 h-3.5" />
                    <span>Xóa bộ lọc</span>
                  </button>
                </div>
              )}
            </Card>
          ) : (
            <div className="space-y-3.5">
              {filteredCourses.map((course) => {
                const theme = getCourseVisualTheme({ title: course.title });
                const isExpanded =
                  Boolean(searchQuery.trim()) || expandedCourseIds.has(course.courseId);
                const lessons = course.lessons ?? [];
                const nextLesson =
                  lessons.find((l) => l.activityCount > 0 && l.passedCount < l.activityCount) ||
                  lessons[0];
                const pct =
                  course.totalActivities > 0
                    ? Math.round((course.passedActivities / course.totalActivities) * 100)
                    : 0;

                return (
                  <Card
                    key={course.courseId}
                    className="overflow-hidden border-[var(--border-calm)] bg-[var(--surface-card)] shadow-elevation-1 hover:shadow-elevation-2 transition-shadow"
                  >
                    <div className={`h-1 w-full bg-gradient-to-r ${theme.accentBarClass}`} />
                    <div className="p-4 sm:p-5 space-y-4">
                      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                        <div className="min-w-0 flex-1 space-y-2">
                          <div className="flex flex-wrap items-center gap-2">
                            <span
                              className={`inline-block text-[10px] font-semibold border rounded-md px-2 py-0.5 ${theme.badgeBgClass}`}
                            >
                              {theme.domainLabel}
                            </span>
                            <span className="text-xs text-[var(--text-tertiary)] whitespace-nowrap">
                              {lessons.length} bài học
                            </span>
                            <span className="inline-flex items-center gap-1 text-[10px] font-semibold border border-purple-500/30 bg-purple-500/10 text-purple-600 dark:text-purple-400 rounded-md px-2 py-0.5 whitespace-nowrap">
                              <ClipboardCheck className="w-3 h-3" />
                              <span>{course.quizCount ?? 0} đề thi</span>
                            </span>
                            {(course.attemptCount ?? 0) > 0 && (
                              <span className="text-xs text-[var(--text-secondary)] font-medium">
                                · TB {course.averageScore}/100 · Gần nhất:{" "}
                                {formatShortDateVi(course.lastAttemptAt)}
                              </span>
                            )}
                          </div>

                          <h3 className="text-base font-semibold text-[var(--text-primary)]">
                            {course.title}
                          </h3>

                          <div className="max-w-md space-y-1">
                            <div className="flex items-center justify-between text-xs">
                              <span className="text-[var(--text-secondary)]">
                                {course.totalActivities > 0
                                  ? `${course.passedActivities} / ${course.totalActivities} hoạt động đạt`
                                  : "Luyện tập tự do theo bài giảng"}
                              </span>
                              {course.totalActivities > 0 && (
                                <span className="font-semibold text-[var(--text-primary)]">
                                  {pct}%
                                </span>
                              )}
                            </div>
                            {course.totalActivities > 0 && (
                              <ProgressBar value={course.passedActivities} max={course.totalActivities} size="sm" />
                            )}
                          </div>
                        </div>

                        <div className="flex flex-wrap items-center gap-2 shrink-0">
                          {nextLesson && (
                            <Link
                              href={buildPracticeLessonHref(course.courseId, nextLesson.lessonId)}
                              className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-[10px] bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC] text-xs font-semibold transition"
                            >
                              <Play className="w-3.5 h-3.5" />
                              <span>Luyện bài tiếp</span>
                            </Link>
                          )}

                          <Link
                            href={`/app/courses/${course.courseId}/quiz`}
                            className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-[10px] border border-purple-500/30 bg-purple-500/10 hover:bg-purple-500/20 text-purple-700 dark:text-purple-300 text-xs font-semibold transition"
                          >
                            <ClipboardCheck className="w-3.5 h-3.5" />
                            <span>
                              Phòng thi &amp; Đề{(course.quizCount ?? 0) > 0 ? ` (${course.quizCount})` : ""}
                            </span>
                          </Link>

                          {lessons.length > 0 && (
                            <button
                              type="button"
                              onClick={() => toggleCourseExpanded(course.courseId)}
                              className="inline-flex items-center gap-1.5 h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--canvas-bg)] text-xs font-medium text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer"
                            >
                              <span>{isExpanded ? "Thu gọn" : `Chọn bài (${lessons.length})`}</span>
                              {isExpanded ? (
                                <ChevronUp className="w-3.5 h-3.5" />
                              ) : (
                                <ChevronDown className="w-3.5 h-3.5" />
                              )}
                            </button>
                          )}
                        </div>
                      </div>

                      {/* Expandable Lessons (grouped by chapter when multi-chapter or >15 lessons) */}
                      {isExpanded && lessons.length > 0 && (() => {
                        const visibleLessons = lessons.filter(
                          (lesson) =>
                            !searchQuery.trim() ||
                            normalizeVi(course.title).includes(normalizeVi(searchQuery)) ||
                            normalizeVi(lesson.title).includes(normalizeVi(searchQuery)) ||
                            normalizeVi(lesson.chapterTitle).includes(normalizeVi(searchQuery))
                        );

                        const chapterGroupsMap = new Map<string, PracticeLessonSummary[]>();
                        for (const lesson of visibleLessons) {
                          const chName = (lesson.chapterTitle || "").trim() || "Bài học";
                          const list = chapterGroupsMap.get(chName);
                          if (list) list.push(lesson);
                          else chapterGroupsMap.set(chName, [lesson]);
                        }
                        const chapterGroups = Array.from(chapterGroupsMap.entries());
                        const useChapterAccordions =
                          chapterGroups.length > 1 || lessons.length > 15;

                        const renderLessonCard = (lesson: PracticeLessonSummary) => {
                          const doneAll =
                            lesson.activityCount > 0 && lesson.passedCount >= lesson.activityCount;
                          const lessonPct =
                            lesson.activityCount > 0
                              ? Math.round((lesson.passedCount / lesson.activityCount) * 100)
                              : 0;

                          return (
                            <div
                              key={lesson.lessonId}
                              className="group p-3.5 rounded-[12px] border border-[var(--border-calm)] bg-[var(--surface-elevated)]/35 hover:border-[var(--accent)]/60 hover:bg-[var(--surface-elevated)]/70 transition flex flex-col justify-between gap-2.5 shadow-xs"
                            >
                              <Link
                                href={buildPracticeLessonHref(course.courseId, lesson.lessonId)}
                                className="flex items-start justify-between gap-2 min-w-0"
                                title={`Mở phòng thực hành: ${lesson.title}`}
                              >
                                <div className="min-w-0 space-y-1 flex-1">
                                  <p className="text-[11px] font-medium text-[var(--text-tertiary)] truncate">
                                    {stripChapterPrefix(lesson.chapterTitle)}
                                  </p>
                                  <p className="text-xs font-semibold text-[var(--text-primary)] group-hover:text-[var(--accent)] truncate">
                                    {lesson.title}
                                  </p>
                                  {lesson.activityCount > 0 ? (
                                    <div className="space-y-1 pt-0.5">
                                      <div className="flex items-center justify-between text-[11px]">
                                        <span className="text-[var(--text-secondary)]">
                                          {lesson.passedCount}/{lesson.activityCount} đạt
                                        </span>
                                        <span
                                          className={`font-semibold tabular-nums ${
                                            doneAll
                                              ? "text-emerald-600 dark:text-emerald-400"
                                              : "text-[var(--text-primary)]"
                                          }`}
                                        >
                                          {lessonPct}%
                                        </span>
                                      </div>
                                      <ProgressBar
                                        value={lesson.passedCount}
                                        max={lesson.activityCount}
                                        size="sm"
                                      />
                                    </div>
                                  ) : (
                                    <p className="text-[11px] text-[var(--text-secondary)]">
                                      Thực hành tương tác
                                    </p>
                                  )}
                                </div>
                                <span
                                  className={`shrink-0 inline-flex items-center justify-center w-7 h-7 rounded-full border transition mt-0.5 ${
                                    doneAll
                                      ? "border-emerald-500/40 bg-emerald-500/15 text-emerald-600 dark:text-emerald-400"
                                      : "border-[var(--border-calm)] bg-[var(--surface-card)] text-[var(--text-secondary)] group-hover:text-[var(--accent)] group-hover:border-[var(--accent)]"
                                  }`}
                                >
                                  {doneAll ? (
                                    <CheckCircle2 className="w-3.5 h-3.5" />
                                  ) : (
                                    <ArrowRight className="w-3.5 h-3.5" />
                                  )}
                                </span>
                              </Link>

                              <div className="flex items-center justify-between gap-1.5 pt-2 border-t border-[var(--border-calm)]/70 text-[11px]">
                                <Link
                                  href={buildPracticeLessonHref(course.courseId, lesson.lessonId)}
                                  className="inline-flex items-center gap-1 font-medium text-[var(--accent)] hover:underline"
                                >
                                  <Play className="w-3 h-3" />
                                  <span>Thực hành Lab</span>
                                </Link>

                                <Link
                                  href={`/app/courses/${course.courseId}/quiz?lessonId=${lesson.lessonId}`}
                                  className="inline-flex items-center gap-1 px-2 py-0.5 rounded-[6px] border border-purple-500/30 bg-purple-500/10 text-purple-700 dark:text-purple-300 font-semibold hover:bg-purple-500/20 transition"
                                  title={`Giải đề thi & trắc nghiệm cho bài: ${lesson.title}`}
                                >
                                  <ClipboardCheck className="w-3 h-3" />
                                  <span>Giải đề</span>
                                </Link>
                              </div>
                            </div>
                          );
                        };

                        if (!useChapterAccordions) {
                          return (
                            <div className="pt-3 border-t border-[var(--border-calm)] grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2.5">
                              {visibleLessons.map(renderLessonCard)}
                            </div>
                          );
                        }

                        return (
                          <div className="pt-3 border-t border-[var(--border-calm)] space-y-2.5">
                            {chapterGroups.map(([chapterTitle, chapterLessons], chIdx) => {
                              const chKey = `${course.courseId}::${chapterTitle}`;
                              const defaultOpen = chIdx === 0;
                              const open = isChapterExpanded(chKey, defaultOpen);
                              const chTotalAct = chapterLessons.reduce((acc, l) => acc + l.activityCount, 0);
                              const chPassedAct = chapterLessons.reduce((acc, l) => acc + l.passedCount, 0);
                              const chPct = chTotalAct > 0 ? Math.round((chPassedAct / chTotalAct) * 100) : null;

                              return (
                                <div
                                  key={chKey}
                                  className="rounded-[12px] border border-[var(--border-calm)] bg-[var(--surface-card)] overflow-hidden"
                                >
                                  <button
                                    type="button"
                                    onClick={() => toggleChapterExpanded(chKey, defaultOpen)}
                                    className="w-full px-3.5 py-2.5 flex items-center justify-between gap-3 text-left hover:bg-[var(--surface-elevated)]/60 transition cursor-pointer"
                                  >
                                    <div className="flex items-center gap-2 min-w-0">
                                      {open ? (
                                        <ChevronDown className="w-4 h-4 text-[var(--text-tertiary)] shrink-0" />
                                      ) : (
                                        <ChevronRight className="w-4 h-4 text-[var(--text-tertiary)] shrink-0" />
                                      )}
                                      <Layers className="w-3.5 h-3.5 text-[var(--accent)] shrink-0" />
                                      <span className="text-xs sm:text-sm font-semibold text-[var(--text-primary)] truncate">
                                        {stripChapterPrefix(chapterTitle) || chapterTitle}
                                      </span>
                                    </div>
                                    <div className="flex items-center gap-2 shrink-0">
                                      {chPct !== null && (
                                        <span className="hidden sm:inline-flex items-center gap-1.5 text-[11px] font-semibold text-[var(--text-secondary)] tabular-nums">
                                          <span>{chPassedAct}/{chTotalAct}</span>
                                          <span className="text-[var(--accent)]">({chPct}%)</span>
                                        </span>
                                      )}
                                      <span className="text-[11px] font-semibold px-2 py-0.5 rounded-full bg-[var(--surface-elevated)] text-[var(--text-secondary)]">
                                        {chapterLessons.length} bài
                                      </span>
                                    </div>
                                  </button>

                                  {open && (
                                    <div className="p-3 pt-1 border-t border-[var(--border-calm)]/70 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2.5">
                                      {chapterLessons.map(renderLessonCard)}
                                    </div>
                                  )}
                                </div>
                              );
                            })}
                          </div>
                        );
                      })()}
                    </div>
                  </Card>
                );
              })}
            </div>
          )}
        </section>
      )}
    </div>
  );
}
