"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  AlertCircle,
  ArrowRight,
  Award,
  BookOpen,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  ClipboardCheck,
  Clock,
  FileCheck2,
  Flame,
  Layers,
  Loader2,
  Plus,
  RefreshCw,
  Search,
  Sparkles,
  Trophy,
  X,
} from "lucide-react";
import { apiFetch } from "@/lib/api-client";
import { Badge, Card, StatCard } from "@/components/ui";
import {
  deduplicateCourses,
  getCourseId,
  getCourseVisualTheme,
  normalizeVi,
  sortCoursesSmartly,
  stripChapterPrefix,
  type CourseLike,
} from "@/lib/course-helpers";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";

type ExamCategory = "final" | "chapter" | "lesson" | "custom";
type ExamCategoryFilter = "all" | ExamCategory;

interface ExamQuizDto {
  id: string;
  title: string;
  courseId?: string | null;
  courseTitle?: string | null;
  chapterId?: string | null;
  chapterTitle?: string | null;
  chapterSortOrder?: number | null;
  lessonId?: string | null;
  lessonTitle?: string | null;
  lessonSortOrder?: number | null;
  documentId?: string | null;
  category?: string | null;
  totalItems: number;
  correctItems: number;
  status: string;
  startedAt?: string | null;
  completedAt?: string | null;
}

interface PracticeStatsDto {
  totalAttempts: number;
  averageScore: number;
  passedCount: number;
  todayAttempts: number;
  streakDays: number;
}

interface OutlineChapterDto {
  id: string;
  title: string;
  sortOrder?: number;
}

function resolveQuizCategory(quiz: ExamQuizDto): ExamCategory {
  const cat = (quiz.category || "").toLowerCase().trim();
  if (cat === "final" || cat === "chapter" || cat === "lesson" || cat === "custom") {
    return cat;
  }
  const t = (quiz.title || "").trim();
  if (t.includes("[Đề Tự Tạo]")) return "custom";
  if (t.includes("[Đề Tổng Hợp")) return "final";
  if (t.includes("[Đề Chương")) return "chapter";
  return "lesson";
}

function getCategoryMeta(cat: ExamCategory): {
  label: string;
  badgeClass: string;
} {
  switch (cat) {
    case "final":
      return {
        label: "Cuối khóa",
        badgeClass:
          "bg-amber-500/15 text-amber-700 dark:text-amber-300 border-amber-500/30",
      };
    case "chapter":
      return {
        label: "Đề chương",
        badgeClass:
          "bg-purple-500/15 text-purple-700 dark:text-purple-300 border-purple-500/30",
      };
    case "custom":
      return {
        label: "Tự tạo",
        badgeClass:
          "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border-emerald-500/30",
      };
    default:
      return {
        label: "Quiz bài học",
        badgeClass:
          "bg-sky-500/10 text-sky-700 dark:text-sky-300 border-sky-500/30",
      };
  }
}

function getQuizStatusMeta(quiz: ExamQuizDto): {
  label: string;
  variant: "success" | "brand" | "neutral";
} {
  if (quiz.status === "completed") {
    return { label: "Đã nộp", variant: "success" };
  }
  if (quiz.status === "in_progress" || quiz.startedAt || quiz.correctItems > 0) {
    return { label: "Đang làm", variant: "brand" };
  }
  return { label: "Chưa làm", variant: "neutral" };
}

export default function ExamsHubPage() {
  const router = useRouter();
  const { activeUserHeaderId } = useWorkspaceStore();

  const [courses, setCourses] = useState<CourseLike[]>([]);
  const [quizzes, setQuizzes] = useState<ExamQuizDto[]>([]);
  const [stats, setStats] = useState<PracticeStatsDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const [categoryFilter, setCategoryFilter] = useState<ExamCategoryFilter>("all");
  const [selectedCourseId, setSelectedCourseId] = useState<string>("all");
  const [searchQuery, setSearchQuery] = useState("");
  const [expandedChapterKeys, setExpandedChapterKeys] = useState<Set<string>>(new Set());

  // Custom quiz generation modal state
  const [createOpen, setCreateOpen] = useState(false);
  const [genCourseId, setGenCourseId] = useState<string>("");
  const [genChapterId, setGenChapterId] = useState<string>("");
  const [genCount, setGenCount] = useState<number>(10);
  const [genItemType, setGenItemType] = useState<string>("all");
  const [genTitle, setGenTitle] = useState<string>("");
  const [generating, setGenerating] = useState(false);
  const [genError, setGenError] = useState<string | null>(null);

  const [outlineCache, setOutlineCache] = useState<Record<string, OutlineChapterDto[]>>({});
  const [loadingOutline, setLoadingOutline] = useState(false);

  const loadExamsHub = useCallback(async () => {
    setIsLoading(true);
    setFetchError(null);
    try {
      const [coursesRes, quizzesRes, statsRes] = await Promise.all([
        apiFetch<CourseLike[]>("/api/v1/courses"),
        apiFetch<ExamQuizDto[]>("/api/v1/practice-quizzes"),
        apiFetch<PracticeStatsDto>("/api/v1/practice/stats").catch(() => null),
      ]);

      const rawCourses = Array.isArray(coursesRes) ? coursesRes : [];
      const uniqueCourses = sortCoursesSmartly(deduplicateCourses(rawCourses));
      setCourses(uniqueCourses);

      const rawQuizzes = Array.isArray(quizzesRes) ? quizzesRes : [];
      setQuizzes(rawQuizzes);
      setStats(statsRes);

      if (uniqueCourses.length > 0) {
        setGenCourseId((prev) => prev || getCourseId(uniqueCourses[0]));
      }
    } catch (err) {
      const message =
        err instanceof Error && err.message
          ? err.message
          : "Không thể tải dữ liệu Ngân hàng Đề & Thi Thử từ máy chủ.";
      setFetchError(message);
    } finally {
      setIsLoading(false);
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    void loadExamsHub();
  }, [loadExamsHub]);

  useEffect(() => {
    if (typeof window === "undefined") return;
    const onMutated = () => {
      void loadExamsHub();
    };
    window.addEventListener("swms:data-mutated", onMutated);
    return () => window.removeEventListener("swms:data-mutated", onMutated);
  }, [loadExamsHub]);

  const loadCourseOutline = useCallback(
    async (courseId: string) => {
      if (!courseId || outlineCache[courseId]) return;
      setLoadingOutline(true);
      try {
        const rawOutline = await apiFetch<unknown>(`/api/v1/courses/${courseId}`);

        const chaptersRaw = Array.isArray(rawOutline)
          ? rawOutline
          : rawOutline && typeof rawOutline === "object" && Array.isArray((rawOutline as { chapters?: unknown[] }).chapters)
            ? (rawOutline as { chapters: unknown[] }).chapters
            : [];

        const mapped: OutlineChapterDto[] = chaptersRaw
          .filter((ch): ch is Record<string, unknown> => Boolean(ch && typeof ch === "object"))
          .map((ch, idx) => ({
            id: String(ch.id ?? ch.chapterId ?? ""),
            title: String(ch.title ?? `Chương ${idx + 1}`),
            sortOrder: typeof ch.sortOrder === "number" ? ch.sortOrder : idx,
          }))
          .filter((ch) => Boolean(ch.id))
          .sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0));

        setOutlineCache((prev) => ({ ...prev, [courseId]: mapped }));
      } catch {
        setOutlineCache((prev) => ({ ...prev, [courseId]: [] }));
      } finally {
        setLoadingOutline(false);
      }
    },
    [outlineCache]
  );

  useEffect(() => {
    if (createOpen && genCourseId) {
      void loadCourseOutline(genCourseId);
    }
  }, [createOpen, genCourseId, loadCourseOutline]);

  useEffect(() => {
    if (!createOpen) return;
    const prevOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !generating) {
        setCreateOpen(false);
      }
    };
    window.addEventListener("keydown", onKeyDown);
    return () => {
      document.body.style.overflow = prevOverflow;
      window.removeEventListener("keydown", onKeyDown);
    };
  }, [createOpen, generating]);

  const openCreateModalForCourse = (courseId?: string) => {
    const targetId = courseId || genCourseId || (courses[0] ? getCourseId(courses[0]) : "");
    setGenCourseId(targetId);
    setGenChapterId("");
    setGenError(null);
    setCreateOpen(true);
  };

  const handleGenerateExam = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!genCourseId || generating) return;
    setGenerating(true);
    setGenError(null);
    try {
      const created = await apiFetch<{ id: string }>(
        `/api/v1/courses/${genCourseId}/quizzes/generate`,
        {
          method: "POST",
          body: JSON.stringify({
            chapterId: genChapterId || null,
            count: genCount,
            itemType: genItemType === "all" ? null : genItemType,
            title: genTitle.trim() || null,
          }),
        }
      );

      if (typeof window !== "undefined") {
        window.dispatchEvent(new Event("swms:data-mutated"));
      }
      setCreateOpen(false);
      setGenTitle("");
      router.push(`/app/courses/${genCourseId}/quiz?quizId=${encodeURIComponent(created.id)}`);
    } catch (err) {
      setGenError(
        err instanceof Error && err.message
          ? err.message
          : "Không thể tạo đề thi tự chọn. Vui lòng thử lại."
      );
    } finally {
      setGenerating(false);
    }
  };

  // Course lookup map & available courses in quizzes
  const courseById = useMemo(() => {
    const map = new Map<string, CourseLike>();
    for (const c of courses) {
      map.set(getCourseId(c), c);
    }
    return map;
  }, [courses]);

  const courseOptions = useMemo(() => {
    const seen = new Map<string, string>();
    for (const c of courses) {
      const id = getCourseId(c);
      if (id) seen.set(id, c.title || "Khóa học");
    }
    for (const q of quizzes) {
      if (q.courseId && !seen.has(q.courseId)) {
        seen.set(q.courseId, q.courseTitle || "Khóa học");
      }
    }
    return Array.from(seen.entries()).map(([id, title]) => ({ id, title }));
  }, [courses, quizzes]);

  // Stats metrics from real API data
  const completedQuizzes = useMemo(
    () => quizzes.filter((q) => q.status === "completed"),
    [quizzes]
  );

  const inProgressCount = useMemo(
    () =>
      quizzes.filter(
        (q) => q.status !== "completed" && (q.status === "in_progress" || Boolean(q.startedAt) || q.correctItems > 0)
      ).length,
    [quizzes]
  );

  const averageCompletedPercent = useMemo(() => {
    if (completedQuizzes.length === 0) return null;
    const sumPct = completedQuizzes.reduce((acc, q) => {
      const total = Math.max(1, q.totalItems || 1);
      return acc + Math.round((q.correctItems / total) * 100);
    }, 0);
    return Math.round(sumPct / completedQuizzes.length);
  }, [completedQuizzes]);

  // Filter by course & search first (so category tabs show accurate counts for current scope)
  const courseAndSearchFiltered = useMemo(() => {
    const qNorm = normalizeVi(searchQuery);
    return quizzes.filter((quiz) => {
      if (selectedCourseId !== "all" && quiz.courseId !== selectedCourseId) {
        return false;
      }
      if (!qNorm) return true;
      const haystack = normalizeVi(
        `${quiz.title || ""} ${quiz.chapterTitle || ""} ${quiz.lessonTitle || ""} ${quiz.courseTitle || ""}`
      );
      return haystack.includes(qNorm);
    });
  }, [quizzes, selectedCourseId, searchQuery]);

  const categoryCounts = useMemo(() => {
    const counts: Record<ExamCategoryFilter, number> = {
      all: courseAndSearchFiltered.length,
      final: 0,
      chapter: 0,
      lesson: 0,
      custom: 0,
    };
    for (const q of courseAndSearchFiltered) {
      const cat = resolveQuizCategory(q);
      counts[cat] += 1;
    }
    return counts;
  }, [courseAndSearchFiltered]);

  const filteredQuizzes = useMemo(() => {
    if (categoryFilter === "all") return courseAndSearchFiltered;
    return courseAndSearchFiltered.filter((q) => resolveQuizCategory(q) === categoryFilter);
  }, [courseAndSearchFiltered, categoryFilter]);

  const hasActiveFilter =
    categoryFilter !== "all" || selectedCourseId !== "all" || searchQuery.trim().length > 0;

  // Group filtered quizzes by course -> final / custom / chapters
  const groupedCourses = useMemo(() => {
    const byCourse = new Map<string, ExamQuizDto[]>();
    for (const q of filteredQuizzes) {
      const cid = q.courseId || "uncategorized";
      const list = byCourse.get(cid);
      if (list) list.push(q);
      else byCourse.set(cid, [q]);
    }

    const orderedCourseIds: string[] = [];
    for (const c of courses) {
      const cid = getCourseId(c);
      if (byCourse.has(cid)) orderedCourseIds.push(cid);
    }
    for (const cid of byCourse.keys()) {
      if (!orderedCourseIds.includes(cid)) orderedCourseIds.push(cid);
    }

    return orderedCourseIds.map((cid) => {
      const list = byCourse.get(cid) ?? [];
      const courseMeta = courseById.get(cid);
      const courseTitle =
        courseMeta?.title || list[0]?.courseTitle || "Khóa học chưa phân loại";

      const finals: ExamQuizDto[] = [];
      const customs: ExamQuizDto[] = [];
      const chapterMap = new Map<
        string,
        {
          key: string;
          chapterTitle: string;
          sortOrder: number;
          quizzes: ExamQuizDto[];
        }
      >();

      for (const q of list) {
        const cat = resolveQuizCategory(q);
        if (cat === "final") {
          finals.push(q);
        } else if (cat === "custom") {
          customs.push(q);
        } else {
          const rawChapTitle = (q.chapterTitle || "").trim() || "Nội dung theo bài học";
          const chKey = `${cid}::${q.chapterId || rawChapTitle}`;
          const existing = chapterMap.get(chKey);
          if (existing) {
            existing.quizzes.push(q);
          } else {
            chapterMap.set(chKey, {
              key: chKey,
              chapterTitle: rawChapTitle,
              sortOrder: typeof q.chapterSortOrder === "number" ? q.chapterSortOrder : 9999,
              quizzes: [q],
            });
          }
        }
      }

      const chapters = Array.from(chapterMap.values())
        .sort((a, b) => {
          if (a.sortOrder !== b.sortOrder) return a.sortOrder - b.sortOrder;
          return a.chapterTitle.localeCompare(b.chapterTitle, "vi");
        })
        .map((ch) => ({
          ...ch,
          quizzes: [...ch.quizzes].sort((a, b) => {
            const catA = resolveQuizCategory(a) === "chapter" ? 0 : 1;
            const catB = resolveQuizCategory(b) === "chapter" ? 0 : 1;
            if (catA !== catB) return catA - catB;
            const ordA = typeof a.lessonSortOrder === "number" ? a.lessonSortOrder : 9999;
            const ordB = typeof b.lessonSortOrder === "number" ? b.lessonSortOrder : 9999;
            if (ordA !== ordB) return ordA - ordB;
            return a.title.localeCompare(b.title, "vi");
          }),
        }));

      return {
        courseId: cid,
        courseTitle,
        courseMeta,
        totalQuizzes: list.length,
        finals,
        customs,
        chapters,
      };
    });
  }, [filteredQuizzes, courses, courseById]);

  const toggleChapter = (key: string, defaultOpen: boolean) => {
    setExpandedChapterKeys((prev) => {
      const next = new Set(prev);
      const currentlyOpen = prev.has(key) ? true : defaultOpen && !prev.has(`closed:${key}`);
      if (currentlyOpen) {
        next.delete(key);
        next.add(`closed:${key}`);
      } else {
        next.delete(`closed:${key}`);
        next.add(key);
      }
      return next;
    });
  };

  const isChapterOpen = (key: string, defaultOpen: boolean) => {
    if (hasActiveFilter) return true;
    if (expandedChapterKeys.has(key)) return true;
    if (expandedChapterKeys.has(`closed:${key}`)) return false;
    return defaultOpen;
  };

  if (isLoading) {
    return (
      <div
        data-testid="exams-hub-page"
        className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 space-y-6 overflow-x-hidden"
      >
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div className="space-y-2">
            <div className="h-4 w-52 rounded-md swms-skeleton" />
            <div className="h-8 w-64 rounded-lg swms-skeleton" />
            <div className="h-4 w-96 max-w-full rounded-md swms-skeleton" />
          </div>
          <div className="flex items-center gap-2">
            <div className="h-9 w-24 rounded-[10px] swms-skeleton" />
            <div className="h-9 w-36 rounded-[10px] swms-skeleton" />
          </div>
        </div>
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3.5">
          {[0, 1, 2, 3].map((i) => (
            <div key={i} className="h-28 rounded-[14px] border border-[var(--border-calm)] swms-skeleton" />
          ))}
        </div>
        <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-3">
          <div className="h-10 w-full lg:w-[460px] rounded-[10px] swms-skeleton" />
          <div className="h-10 w-full lg:w-[420px] rounded-[10px] swms-skeleton" />
        </div>
        <div className="rounded-[16px] border border-[var(--border-calm)] bg-[var(--surface-card)] p-5 space-y-4">
          <div className="h-12 w-full rounded-lg swms-skeleton" />
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
            {[0, 1, 2].map((i) => (
              <div key={i} className="h-32 rounded-[12px] swms-skeleton" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div
      data-testid="exams-hub-page"
      className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh] space-y-7 overflow-x-hidden"
    >
      {/* Header */}
      <header className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div className="space-y-1">
          <div className="inline-flex items-center gap-1.5 text-xs font-semibold text-[var(--accent)]">
            <ClipboardCheck className="w-4 h-4" />
            <span>Hệ thống Đánh giá Năng lực &amp; Thi Thử</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-[var(--text-primary)]">
            Ngân hàng Đề &amp; Thi Thử
          </h1>
          <p className="text-sm text-[var(--text-secondary)]">
            Tổng hợp toàn bộ đề thi cuối khóa, kiểm tra chương, quiz bài học và đề tự chọn để vào phòng thi ngay.
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => void loadExamsHub()}
            className="inline-flex items-center gap-1.5 h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-medium text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            <span>Làm mới</span>
          </button>

          <button
            type="button"
            data-testid="open-create-exam-modal-btn"
            disabled={courseOptions.length === 0}
            onClick={() => openCreateModalForCourse()}
            className="inline-flex items-center gap-1.5 h-9 px-4 rounded-[10px] bg-[var(--accent)] text-white text-xs font-semibold hover:opacity-95 disabled:opacity-50 transition shadow-xs cursor-pointer"
          >
            <Plus className="w-4 h-4" />
            <span>+ Tạo Đề Tự Chọn</span>
          </button>
        </div>
      </header>

      {/* Error State (Zero-Facade) */}
      {fetchError && (
        <div
          data-testid="exams-hub-error"
          className="rounded-[14px] border border-rose-500/30 bg-rose-500/5 p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3"
        >
          <div className="flex items-center gap-2.5 text-sm text-rose-600 dark:text-rose-400">
            <AlertCircle className="w-5 h-5 shrink-0" />
            <span>{fetchError}</span>
          </div>
          <button
            type="button"
            onClick={() => void loadExamsHub()}
            className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-semibold text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer self-start sm:self-auto"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            <span>Thử lại</span>
          </button>
        </div>
      )}

      {/* 1. 4 StatCards */}
      {!fetchError && (
        <section
          data-testid="exams-hub-stats"
          className="grid grid-cols-2 lg:grid-cols-4 gap-3.5"
        >
          <StatCard
            title="Tổng số đề hiện có"
            value={quizzes.length}
            subtitle={`${groupedCourses.length} khóa học có đề`}
            icon={FileCheck2}
            variant="brand"
          />
          <StatCard
            title="Đề đã hoàn thành"
            value={completedQuizzes.length}
            subtitle={
              inProgressCount > 0
                ? `${inProgressCount} đề đang làm dở`
                : "Sẵn sàng thi thử"
            }
            icon={CheckCircle2}
            variant="success"
          />
          <StatCard
            title="Điểm TB các đề đã nộp"
            value={averageCompletedPercent !== null ? `${averageCompletedPercent}%` : "Chưa có"}
            subtitle={
              completedQuizzes.length > 0
                ? `Tính trên ${completedQuizzes.length} đề hoàn thành`
                : "Hoàn thành 1 đề để xem tỷ lệ"
            }
            icon={Trophy}
            variant="default"
          />
          <StatCard
            title="Chuỗi ngày & Lượt làm"
            value={stats ? `${stats.streakDays} ngày` : "—"}
            subtitle={stats ? `${stats.totalAttempts} lượt làm bài` : "Chưa có thống kê"}
            icon={Flame}
            variant="streak"
          />
        </section>
      )}

      {/* 2. Filter & Search Bar */}
      {!fetchError && (
        <section className="space-y-3">
          <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-3 min-w-0">
            {/* Category Tabs */}
            <div className="flex flex-nowrap overflow-x-auto no-scrollbar items-center gap-1.5 pb-1 lg:pb-0 max-w-full min-w-0">
              {(
                [
                  { id: "all", label: "Tất cả" },
                  { id: "final", label: "Thi cuối khóa" },
                  { id: "chapter", label: "Kiểm tra chương" },
                  { id: "lesson", label: "Quiz bài học" },
                  { id: "custom", label: "Đề tự tạo" },
                ] as const
              ).map((tab) => {
                const active = categoryFilter === tab.id;
                return (
                  <button
                    key={tab.id}
                    type="button"
                    data-testid={`exams-filter-${tab.id}`}
                    onClick={() => setCategoryFilter(tab.id)}
                    className={`inline-flex items-center gap-1.5 h-9 px-3 rounded-[10px] text-xs font-medium whitespace-nowrap shrink-0 transition cursor-pointer ${
                      active
                        ? "bg-[var(--accent)] text-white font-semibold shadow-xs"
                        : "bg-[var(--surface-card)] text-[var(--text-secondary)] border border-[var(--border-calm)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]"
                    }`}
                  >
                    <span>{tab.label}</span>
                    <span
                      className={`text-[11px] px-1.5 py-0.2 rounded-full font-semibold ${
                        active
                          ? "bg-white/20 text-white"
                          : "bg-[var(--surface-elevated)] text-[var(--text-tertiary)]"
                      }`}
                    >
                      {categoryCounts[tab.id]}
                    </span>
                  </button>
                );
              })}
            </div>

            {/* Course Dropdown + Search Input */}
            <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-2.5">
              <select
                aria-label="Lọc theo khóa học"
                value={selectedCourseId}
                onChange={(e) => setSelectedCourseId(e.target.value)}
                className="h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-medium text-[var(--text-primary)] focus:outline-none focus:border-[var(--accent)] sm:max-w-[240px]"
              >
                <option value="all">Tất cả khóa học ({courseOptions.length})</option>
                {courseOptions.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.title}
                  </option>
                ))}
              </select>

              <div className="relative w-full sm:w-64">
                <Search className="w-4 h-4 text-[var(--text-tertiary)] absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Tìm tên đề hoặc tên chương..."
                  aria-label="Tìm tên đề hoặc tên chương"
                  className="w-full h-9 pl-9 pr-8 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[var(--accent)]"
                />
                {searchQuery && (
                  <button
                    type="button"
                    onClick={() => setSearchQuery("")}
                    aria-label="Xóa tìm kiếm"
                    className="absolute right-2.5 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                )}
              </div>
            </div>
          </div>
        </section>
      )}

      {/* 3. Exams Grouped by Course -> Chapter */}
      {!fetchError && (
        <section className="space-y-5">
          {groupedCourses.length === 0 ? (
            <Card className="p-10 text-center space-y-3">
              <ClipboardCheck className="w-10 h-10 text-[var(--text-tertiary)] mx-auto" />
              <div className="space-y-1">
                <h2 className="text-base font-semibold text-[var(--text-primary)]">
                  {quizzes.length === 0
                    ? "Chưa có đề thi nào trong hệ thống"
                    : "Không tìm thấy đề thi phù hợp bộ lọc"}
                </h2>
                <p className="text-xs text-[var(--text-secondary)] max-w-md mx-auto">
                  {quizzes.length === 0
                    ? "Hãy tạo một đề tự chọn từ khóa học hiện có hoặc vào khóa học để sinh câu hỏi."
                    : "Thử thay đổi bộ lọc loại đề, khóa học hoặc xóa từ khóa tìm kiếm."}
                </p>
              </div>
              <div className="flex items-center justify-center gap-2 pt-2">
                {hasActiveFilter && (
                  <button
                    type="button"
                    onClick={() => {
                      setCategoryFilter("all");
                      setSelectedCourseId("all");
                      setSearchQuery("");
                    }}
                    className="h-9 px-3.5 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-semibold text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer"
                  >
                    Xóa bộ lọc
                  </button>
                )}
                {courseOptions.length > 0 && (
                  <button
                    type="button"
                    onClick={() => openCreateModalForCourse()}
                    className="inline-flex items-center gap-1.5 h-9 px-4 rounded-[10px] bg-[var(--accent)] text-white text-xs font-semibold hover:opacity-95 transition cursor-pointer"
                  >
                    <Plus className="w-3.5 h-3.5" />
                    <span>Tạo Đề Tự Chọn</span>
                  </button>
                )}
              </div>
            </Card>
          ) : (
            groupedCourses.map((courseGroup) => {
              const theme = getCourseVisualTheme(
                courseGroup.courseMeta ?? { title: courseGroup.courseTitle }
              );
              const isLargeCourse =
                courseGroup.totalQuizzes > 15 || courseGroup.chapters.length > 3;

              return (
                <Card key={courseGroup.courseId} className="overflow-hidden">
                  <div className={`h-1 w-full bg-gradient-to-r ${theme.accentBarClass}`} />
                  <div className="p-4 sm:p-5 space-y-5">
                    {/* Course Header */}
                    <div className="flex flex-col md:flex-row md:items-center justify-between gap-3 pb-3 border-b border-[var(--border-calm)]">
                      <div className="min-w-0 space-y-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <span
                            className={`inline-block text-[10px] font-semibold border rounded-md px-2 py-0.5 whitespace-nowrap shrink-0 ${theme.badgeBgClass}`}
                          >
                            {theme.domainLabel}
                          </span>
                          <Badge variant="brand" size="sm">
                            {courseGroup.totalQuizzes} đề
                          </Badge>
                        </div>
                        <h2 className="text-base sm:text-lg font-bold text-[var(--text-primary)] truncate">
                          {courseGroup.courseTitle}
                        </h2>
                      </div>

                      <div className="flex flex-wrap items-center gap-2 shrink-0">
                        <button
                          type="button"
                          onClick={() => openCreateModalForCourse(courseGroup.courseId)}
                          className="inline-flex items-center gap-1.5 h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--canvas-bg)] hover:bg-[var(--surface-elevated)] text-xs font-semibold text-[var(--text-primary)] transition cursor-pointer"
                        >
                          <Plus className="w-3.5 h-3.5 text-[var(--accent)]" />
                          <span>Tạo đề cho khóa này</span>
                        </button>

                        <Link
                          href={`/app/courses/${courseGroup.courseId}/quiz`}
                          className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-[10px] bg-[var(--accent)] text-white text-xs font-semibold hover:opacity-95 transition shadow-xs"
                        >
                          <ClipboardCheck className="w-3.5 h-3.5" />
                          <span>Mở Phòng Thi khóa học</span>
                        </Link>
                      </div>
                    </div>

                    {/* Final Exams Highlight Section */}
                    {courseGroup.finals.length > 0 && (
                      <div className="space-y-2.5">
                        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-amber-600 dark:text-amber-400">
                          <Award className="w-4 h-4" />
                          <span>Thi Tổng Hợp Cuối Khóa ({courseGroup.finals.length})</span>
                        </div>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                          {courseGroup.finals.map((quiz) => (
                            <ExamQuizCard
                              key={quiz.id}
                              quiz={quiz}
                              courseId={courseGroup.courseId}
                              highlighted
                            />
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Custom Generated Exams Section */}
                    {courseGroup.customs.length > 0 && (
                      <div className="space-y-2.5">
                        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-emerald-600 dark:text-emerald-400">
                          <Sparkles className="w-4 h-4" />
                          <span>Đề Tự Tạo ({courseGroup.customs.length})</span>
                        </div>
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                          {courseGroup.customs.map((quiz) => (
                            <ExamQuizCard
                              key={quiz.id}
                              quiz={quiz}
                              courseId={courseGroup.courseId}
                            />
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Chapter & Lesson Accordion Sections */}
                    {courseGroup.chapters.length > 0 && (
                      <div className="space-y-2.5">
                        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-[var(--text-tertiary)]">
                          <Layers className="w-4 h-4" />
                          <span>Đề theo Chương &amp; Bài học ({courseGroup.chapters.length} chương)</span>
                        </div>

                        <div className="space-y-2">
                          {courseGroup.chapters.map((chap, chapIdx) => {
                            const defaultOpen = !isLargeCourse || chapIdx === 0;
                            const open = isChapterOpen(chap.key, defaultOpen);

                            return (
                              <div
                                key={chap.key}
                                className="rounded-[12px] border border-[var(--border-calm)] bg-[var(--canvas-bg)] overflow-hidden"
                              >
                                <button
                                  type="button"
                                  onClick={() => toggleChapter(chap.key, defaultOpen)}
                                  className="w-full px-3.5 py-2.5 flex items-center justify-between gap-3 text-left hover:bg-[var(--surface-elevated)]/60 transition cursor-pointer"
                                >
                                  <div className="flex items-center gap-2 min-w-0">
                                    {open ? (
                                      <ChevronDown className="w-4 h-4 text-[var(--text-tertiary)] shrink-0" />
                                    ) : (
                                      <ChevronRight className="w-4 h-4 text-[var(--text-tertiary)] shrink-0" />
                                    )}
                                    <span className="text-xs sm:text-sm font-semibold text-[var(--text-primary)] truncate">
                                      {stripChapterPrefix(chap.chapterTitle) || chap.chapterTitle}
                                    </span>
                                  </div>
                                  <span className="text-[11px] font-semibold px-2 py-0.5 rounded-full bg-[var(--surface-card)] border border-[var(--border-calm)] text-[var(--text-secondary)] shrink-0">
                                    {chap.quizzes.length} đề
                                  </span>
                                </button>

                                {open && (
                                  <div className="p-3 pt-1 border-t border-[var(--border-calm)]/70 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2.5">
                                    {chap.quizzes.map((quiz) => (
                                      <ExamQuizCard
                                        key={quiz.id}
                                        quiz={quiz}
                                        courseId={courseGroup.courseId}
                                      />
                                    ))}
                                  </div>
                                )}
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    )}
                  </div>
                </Card>
              );
            })
          )}
        </section>
      )}

      {/* Modal: Tạo Đề Tự Chọn */}
      {createOpen && (
        <div
          className="fixed inset-0 z-50 bg-black/55 backdrop-blur-xs flex items-center justify-center p-4"
          onClick={() => {
            if (!generating) setCreateOpen(false);
          }}
        >
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="create-exam-modal-title"
            className="w-full max-w-lg max-h-[90vh] overflow-y-auto swms-thin-scrollbar rounded-[16px] border border-[var(--border-calm)] bg-[var(--surface-card)] shadow-elevation-3 p-5 space-y-4"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between gap-2 pb-2.5 border-b border-[var(--border-calm)]">
              <h3
                id="create-exam-modal-title"
                className="text-base font-bold text-[var(--text-primary)] flex items-center gap-2"
              >
                <Sparkles className="w-4 h-4 text-[var(--accent)]" />
                <span>Tạo Đề Tự Chọn</span>
              </h3>
              <button
                type="button"
                disabled={generating}
                onClick={() => setCreateOpen(false)}
                aria-label="Đóng hộp thoại tạo đề"
                className="p-1 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] disabled:opacity-40 transition cursor-pointer"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {genError && (
              <div
                role="alert"
                className="rounded-[12px] border border-rose-500/40 bg-rose-500/10 dark:bg-rose-950/35 px-3.5 py-2.5 text-xs font-medium text-rose-700 dark:text-rose-300 flex items-start gap-2.5 shadow-xs"
              >
                <AlertCircle className="w-4 h-4 shrink-0 mt-0.5" />
                <span className="leading-relaxed">{genError}</span>
              </div>
            )}

            <form onSubmit={handleGenerateExam} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-[var(--text-secondary)] mb-1.5">
                  Khóa học
                </label>
                <select
                  value={genCourseId}
                  onChange={(e) => {
                    setGenCourseId(e.target.value);
                    setGenChapterId("");
                  }}
                  required
                  className="w-full h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 text-xs text-[var(--text-primary)] focus:outline-none focus:border-[var(--accent)]"
                >
                  {courseOptions.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.title}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-[var(--text-secondary)] mb-1.5">
                  Phạm vi chương
                </label>
                <select
                  value={genChapterId}
                  onChange={(e) => setGenChapterId(e.target.value)}
                  disabled={loadingOutline}
                  className="w-full h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 text-xs text-[var(--text-primary)] focus:outline-none focus:border-[var(--accent)] disabled:opacity-60"
                >
                  <option value="">
                    {loadingOutline ? "Đang tải danh sách chương..." : "Toàn khóa học"}
                  </option>
                  {(outlineCache[genCourseId] ?? []).map((ch) => (
                    <option key={ch.id} value={ch.id}>
                      {ch.title}
                    </option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-[var(--text-secondary)] mb-1.5">
                    Số câu hỏi
                  </label>
                  <select
                    value={genCount}
                    onChange={(e) => setGenCount(Number(e.target.value))}
                    className="w-full h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 text-xs text-[var(--text-primary)] focus:outline-none focus:border-[var(--accent)]"
                  >
                    <option value={5}>5 câu (Ôn nhanh)</option>
                    <option value={10}>10 câu (Tiêu chuẩn)</option>
                    <option value={15}>15 câu (Chuyên sâu)</option>
                    <option value={20}>20 câu (Thi thử dài)</option>
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-[var(--text-secondary)] mb-1.5">
                    Dạng câu hỏi
                  </label>
                  <select
                    value={genItemType}
                    onChange={(e) => setGenItemType(e.target.value)}
                    className="w-full h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 text-xs text-[var(--text-primary)] focus:outline-none focus:border-[var(--accent)]"
                  >
                    <option value="all">Tổng hợp mọi dạng</option>
                    <option value="multiple_choice">Trắc nghiệm</option>
                    <option value="short_answer">Trả lời ngắn</option>
                    <option value="essay">Tự luận</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-[var(--text-secondary)] mb-1.5">
                  Tên đề thi (tùy chọn)
                </label>
                <input
                  type="text"
                  value={genTitle}
                  onChange={(e) => setGenTitle(e.target.value)}
                  placeholder="Ví dụ: Ôn tập trọng tâm trước thi..."
                  className="w-full h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 text-xs text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[var(--accent)]"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-3 border-t border-[var(--border-calm)]">
                <button
                  type="button"
                  disabled={generating}
                  onClick={() => setCreateOpen(false)}
                  className="h-9 px-3.5 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-medium text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] disabled:opacity-50 transition cursor-pointer"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  data-testid="confirm-create-exam-btn"
                  disabled={generating || !genCourseId}
                  className="inline-flex items-center gap-1.5 h-9 px-4 rounded-[10px] bg-[var(--accent)] text-white text-xs font-semibold hover:opacity-95 disabled:opacity-50 disabled:cursor-not-allowed transition shadow-xs cursor-pointer"
                >
                  {generating ? (
                    <Loader2 className="w-3.5 h-3.5 animate-spin" />
                  ) : (
                    <Plus className="w-3.5 h-3.5" />
                  )}
                  <span>{generating ? "Đang tạo đề..." : "Tạo & Vào phòng thi"}</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

function ExamQuizCard({
  quiz,
  courseId,
  highlighted = false,
}: {
  quiz: ExamQuizDto;
  courseId: string;
  highlighted?: boolean;
}) {
  const category = resolveQuizCategory(quiz);
  const catMeta = getCategoryMeta(category);
  const statusMeta = getQuizStatusMeta(quiz);

  return (
    <div
      className={`p-3.5 rounded-[12px] border shadow-elevation-1 hover:shadow-elevation-2 transition-all flex flex-col justify-between gap-3 ${
        highlighted
          ? "border-amber-500/35 bg-amber-500/5 dark:bg-amber-950/15"
          : "border-[var(--border-calm)] bg-[var(--surface-card)] hover:border-[var(--accent)]/50"
      }`}
    >
      <div className="space-y-2 min-w-0">
        <div className="flex flex-wrap items-center justify-between gap-1.5">
          <span
            className={`inline-flex items-center px-2 py-0.5 rounded-md border text-[10px] font-semibold whitespace-nowrap shrink-0 ${catMeta.badgeClass}`}
          >
            {catMeta.label}
          </span>

          <Badge variant={statusMeta.variant} size="sm">
            {statusMeta.label}
          </Badge>
        </div>

        <h3
          title={quiz.title}
          className="text-xs sm:text-sm font-semibold text-[var(--text-primary)] line-clamp-2 leading-snug"
        >
          {quiz.title}
        </h3>

        {quiz.lessonTitle && category === "lesson" && (
          <p className="text-[11px] text-[var(--text-tertiary)] truncate flex items-center gap-1">
            <BookOpen className="w-3 h-3 shrink-0" />
            <span className="truncate">{quiz.lessonTitle}</span>
          </p>
        )}
      </div>

      <div className="flex items-center justify-between gap-2 pt-2.5 border-t border-[var(--border-calm)]/70">
        <div className="flex items-center gap-2 text-[11px] text-[var(--text-secondary)] font-medium">
          <span className="inline-flex items-center gap-1 whitespace-nowrap">
            <Clock className="w-3 h-3 text-[var(--text-tertiary)]" />
            {quiz.totalItems} câu
          </span>
          <span>·</span>
          <span className="font-mono font-semibold text-[var(--text-primary)] whitespace-nowrap">
            {quiz.correctItems}/{quiz.totalItems}
          </span>
        </div>

        <Link
          href={`/app/courses/${courseId}/quiz?quizId=${encodeURIComponent(quiz.id)}`}
          className="inline-flex items-center gap-1 h-7 px-2.5 rounded-[8px] bg-[var(--accent)] text-white text-[11px] font-semibold hover:opacity-95 transition whitespace-nowrap shrink-0"
        >
          <span>Vào phòng thi</span>
          <ArrowRight className="w-3 h-3" />
        </Link>
      </div>
    </div>
  );
}
