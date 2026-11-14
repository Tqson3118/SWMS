"use client";

import React, { useMemo, useState } from "react";
import {
  BookOpen,
  Brain,
  CheckCircle2,
  Code2,
  Compass,
  FileText,
  GraduationCap,
  Layers,
  Languages,
  PenTool,
  Play,
  RefreshCw,
  Search,
  Sparkles,
  Target,
  TrendingUp,
  AlertCircle,
  X
} from "lucide-react";
import { getClientRequestHeaders, useWorkspaceStore } from "@/stores/useWorkspaceStore";

export interface CourseLessonSummaryDto {
  id: string;
  title: string;
  slug?: string;
  sortOrder?: number;
  currentVersion?: number;
  courseId?: string | null;
  chapterId?: string | null;
  sourceDocumentId?: string | null;
  sourceFolder?: string | null;
  sourceRelativePath?: string | null;
  quizId?: string | null;
  startPage?: number | null;
  endPage?: number | null;
  estimatedMinutes?: number | null;
}

export interface CourseChapterSummaryDto {
  id: string;
  title: string;
  sortOrder?: number;
  lessons: CourseLessonSummaryDto[];
}

export interface CourseSummaryDto {
  id?: string;
  courseId?: string;
  title: string;
  slug?: string;
  description?: string;
  isPublished?: boolean;
  sourceFolder?: string | null;
  category?: string | null;
  sourceDocumentId?: string | null;
  totalPages?: number;
  chapterCount?: number;
  lessonCount?: number;
  flashcardCount?: number;
  quizCount?: number;
  lessonQuizMap?: Record<string, string> | null;
  tagId?: string | null;
  tagName?: string | null;
  quizId?: string | null;
  notebookId?: string | null;
  firstLessonId?: string | null;
  firstLessonTitle?: string | null;
  updatedAt?: string;
  chapters?: CourseChapterSummaryDto[];
}

export interface CourseProgressInfo {
  percentCompleted: number;
  completedLessons: number;
  totalLessons: number;
  completedLessonIds?: string[];
}

type CategoryKey = "all" | "programming" | "language" | "finance" | "psychology";

interface CategoryPillConfig {
  key: CategoryKey;
  label: string;
  testId: string;
}

const CATEGORY_PILLS: CategoryPillConfig[] = [
  { key: "all", label: "Tất cả", testId: "category-pill-all" },
  { key: "programming", label: "Lập trình", testId: "category-pill-programming" },
  { key: "language", label: "Ngoại ngữ", testId: "category-pill-language" },
  { key: "finance", label: "Tài chính", testId: "category-pill-finance" },
  { key: "psychology", label: "Tâm lý & Kỹ năng", testId: "category-pill-psychology" }
];

interface DomainStyle {
  filterKey: Exclude<CategoryKey, "all"> | "general";
  badgeLabel: string;
  accentBarClass: string;
  badgeClass: string;
  iconBgClass: string;
  Icon: React.ComponentType<{ className?: string }>;
}

export function normalizeVietnamese(input: string): string {
  return input
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase()
    .trim();
}

export function resolveCourseDomain(course: CourseSummaryDto): DomainStyle {
  const folder = normalizeVietnamese(course.sourceFolder || "");
  const cat = normalizeVietnamese(course.category || "");
  const slug = normalizeVietnamese(course.slug || "");
  const title = normalizeVietnamese(course.title || "");
  const combined = `${folder} ${cat} ${slug} ${title}`;

  if (
    folder === "dotnet-react-roadmap" ||
    folder === "fullstack-csharp-react-refresh" ||
    cat.includes("lap trinh") ||
    cat.includes("full-stack") ||
    cat.includes("fullstack") ||
    combined.includes("dotnet") ||
    combined.includes(".net") ||
    combined.includes("csharp") ||
    combined.includes("c#") ||
    combined.includes("react") ||
    combined.includes("microservices") ||
    combined.includes("phan tan") ||
    combined.includes("database") ||
    combined.includes("he dieu hanh") ||
    combined.includes("lap trinh")
  ) {
    return {
      filterKey: "programming",
      badgeLabel: course.category || "Lập trình Full-Stack",
      accentBarClass: "bg-indigo-600 dark:bg-indigo-500",
      badgeClass:
        "bg-indigo-50 text-indigo-800 border-indigo-200 dark:bg-indigo-950/70 dark:text-indigo-200 dark:border-indigo-800/80",
      iconBgClass: "bg-indigo-50 dark:bg-indigo-950/70 text-indigo-600 dark:text-indigo-300",
      Icon: Code2
    };
  }

  if (
    folder === "ielts" ||
    cat.includes("ngoai ngu") ||
    cat.includes("ielts") ||
    combined.includes("ielts") ||
    combined.includes("tieng anh") ||
    combined.includes("english")
  ) {
    return {
      filterKey: "language",
      badgeLabel: course.category || "Ngoại ngữ IELTS",
      accentBarClass: "bg-sky-600 dark:bg-sky-500",
      badgeClass:
        "bg-sky-50 text-sky-800 border-sky-200 dark:bg-sky-950/70 dark:text-sky-200 dark:border-sky-800/80",
      iconBgClass: "bg-sky-50 dark:bg-sky-950/70 text-sky-600 dark:text-sky-300",
      Icon: Languages
    };
  }

  if (
    folder === "investing" ||
    folder === "personalfinance" ||
    cat.includes("tai chinh") ||
    cat.includes("dau tu") ||
    combined.includes("tai chinh") ||
    combined.includes("dau tu") ||
    combined.includes("investing") ||
    combined.includes("personal finance")
  ) {
    return {
      filterKey: "finance",
      badgeLabel: course.category || "Tài chính & Đầu tư",
      accentBarClass: "bg-emerald-600 dark:bg-emerald-500",
      badgeClass:
        "bg-emerald-50 text-emerald-800 border-emerald-200 dark:bg-emerald-950/70 dark:text-emerald-200 dark:border-emerald-800/80",
      iconBgClass: "bg-emerald-50 dark:bg-emerald-950/70 text-emerald-600 dark:text-emerald-300",
      Icon: TrendingUp
    };
  }

  if (
    folder === "psychology" ||
    folder === "lifeskills" ||
    cat.includes("tam ly") ||
    cat.includes("ky nang") ||
    combined.includes("tam ly") ||
    combined.includes("psychology") ||
    combined.includes("ky nang song") ||
    combined.includes("lifeskills") ||
    combined.includes("giao tiep")
  ) {
    return {
      filterKey: "psychology",
      badgeLabel: course.category || "Tâm lý & Kỹ năng sống",
      accentBarClass: "bg-amber-600 dark:bg-amber-500",
      badgeClass:
        "bg-amber-50 text-amber-900 border-amber-200 dark:bg-amber-950/70 dark:text-amber-200 dark:border-amber-800/80",
      iconBgClass: "bg-amber-50 dark:bg-amber-950/70 text-amber-700 dark:text-amber-300",
      Icon: Compass
    };
  }

  return {
    filterKey: "general",
    badgeLabel: course.category || "Khóa học cá nhân",
    accentBarClass: "bg-violet-600 dark:bg-violet-500",
    badgeClass:
      "bg-violet-50 text-violet-800 border-violet-200 dark:bg-violet-950/70 dark:text-violet-200 dark:border-violet-800/80",
    iconBgClass: "bg-violet-50 dark:bg-violet-950/70 text-violet-600 dark:text-violet-300",
    Icon: GraduationCap
  };
}

export function resolveNextCourseLesson(
  course: CourseSummaryDto,
  completedLessonIds?: string[]
): { id: string | null; title: string | null; allCompleted: boolean } {
  const orderedLessons = (course.chapters ?? [])
    .slice()
    .sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0))
    .flatMap((ch) =>
      (ch.lessons ?? []).slice().sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0))
    );

  if (orderedLessons.length === 0) {
    return {
      id: course.firstLessonId ?? null,
      title: course.firstLessonTitle ?? null,
      allCompleted: false
    };
  }

  const completedSet = new Set(completedLessonIds ?? []);
  const nextUncompleted = orderedLessons.find((l) => !completedSet.has(l.id));
  if (nextUncompleted) {
    return {
      id: nextUncompleted.id,
      title: nextUncompleted.title,
      allCompleted: false
    };
  }

  return {
    id: orderedLessons[0].id,
    title: orderedLessons[0].title,
    allCompleted: completedSet.size > 0 && completedSet.size >= orderedLessons.length
  };
}

interface CourseCatalogGridProps {
  courses: CourseSummaryDto[];
  courseProgressMap: Record<string, CourseProgressInfo>;
  isLoading: boolean;
  errorMessage: string | null;
  onRefresh: () => Promise<void> | void;
  onOpenCreateModal: () => void;
}

export function CourseCatalogGrid({
  courses,
  courseProgressMap,
  isLoading,
  errorMessage,
  onRefresh,
  onOpenCreateModal
}: CourseCatalogGridProps) {
  const {
    setActiveCourseId,
    setActiveLesson,
    setActiveDocument,
    setActiveMode,
    setActiveReviewSubTab,
    setSelectedTagFilterId,
    setActiveQuizId,
    setEditingCourseId,
    courseSearchQuery,
    setCourseSearchQuery,
    selectedCategory,
    setSelectedCategory
  } = useWorkspaceStore();

  const [syncing, setSyncing] = useState(false);
  const [syncNotice, setSyncNotice] = useState<{
    type: "success" | "error";
    text: string;
  } | null>(null);

  const selectedPillKey = useMemo<CategoryKey>(() => {
    const found = CATEGORY_PILLS.find((p) => p.label === selectedCategory);
    return found ? found.key : "all";
  }, [selectedCategory]);

  const categoryCounts = useMemo(() => {
    const counts: Record<CategoryKey, number> = {
      all: courses.length,
      programming: 0,
      language: 0,
      finance: 0,
      psychology: 0
    };
    for (const c of courses) {
      const domain = resolveCourseDomain(c);
      if (domain.filterKey !== "general") {
        counts[domain.filterKey] += 1;
      }
    }
    return counts;
  }, [courses]);

  const filteredCourses = useMemo(() => {
    const q = normalizeVietnamese(courseSearchQuery);
    return courses.filter((course) => {
      const domain = resolveCourseDomain(course);
      if (selectedPillKey !== "all" && domain.filterKey !== selectedPillKey) {
        return false;
      }
      if (!q) return true;
      const searchable = normalizeVietnamese(
        [
          course.title,
          course.description ?? "",
          course.sourceFolder ?? "",
          course.category ?? "",
          domain.badgeLabel,
          course.tagName ?? ""
        ].join(" ")
      );
      return searchable.includes(q);
    });
  }, [courses, courseSearchQuery, selectedPillKey]);

  const handleSyncFromDocuments = async () => {
    if (syncing) return;
    setSyncing(true);
    setSyncNotice(null);
    try {
      const resp = await fetch("/api/v1/courses/sync-from-documents", {
        method: "POST",
        headers: getClientRequestHeaders()
      });
      if (!resp.ok) {
        const errBody = await resp.json().catch(() => null);
        throw new Error(
          errBody?.error || `Đồng bộ thất bại (Mã lỗi HTTP ${resp.status}).`
        );
      }
      const data = await resp.json().catch(() => ({}));
      const syncedCount =
        data?.syncedCourses ??
        data?.coursesSynced ??
        data?.syncedCoursesCount ??
        data?.totalCourses ??
        (Array.isArray(data?.courses) ? data.courses.length : undefined);
      const lessonsSynced =
        typeof data?.createdLessons === "number" || typeof data?.updatedLessons === "number"
          ? (Number(data?.createdLessons) || 0) + (Number(data?.updatedLessons) || 0)
          : data?.lessonsSynced ?? data?.totalLessons;
      const quizzesSynced =
        typeof data?.createdQuizzes === "number" || typeof data?.updatedQuizzes === "number"
          ? (Number(data?.createdQuizzes) || 0) + (Number(data?.updatedQuizzes) || 0)
          : undefined;
      const flashcardsCreated =
        typeof data?.createdFlashcards === "number" && data.createdFlashcards > 0
          ? data.createdFlashcards
          : undefined;

      await onRefresh();

      const details = [
        typeof syncedCount === "number" ? `${syncedCount} khóa học` : null,
        typeof lessonsSynced === "number" && lessonsSynced > 0 ? `${lessonsSynced} bài học` : null,
        typeof quizzesSynced === "number" && quizzesSynced > 0 ? `${quizzesSynced} quiz` : null,
        typeof flashcardsCreated === "number" ? `+${flashcardsCreated} flashcard` : null
      ]
        .filter(Boolean)
        .join(" • ");

      setSyncNotice({
        type: "success",
        text: details
          ? `Đã đồng bộ từ thư mục document/: ${details}.`
          : data?.message || "Đã đồng bộ thành công toàn bộ khóa học từ document/."
      });
      setTimeout(() => {
        setSyncNotice((prev) => (prev?.type === "success" ? null : prev));
      }, 4500);
    } catch (err: unknown) {
      const msg =
        err instanceof Error
          ? err.message
          : "Không thể kết nối máy chủ để đồng bộ thư mục document/.";
      setSyncNotice({ type: "error", text: msg });
    } finally {
      setSyncing(false);
    }
  };

  return (
    <section
      data-testid="course-catalog-section"
      aria-labelledby="course-catalog-heading"
      className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 shadow-sm p-5 sm:p-6 space-y-5"
    >
      {/* Header Row */}
      <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
        <div className="space-y-1">
          <div className="flex flex-wrap items-center gap-2.5">
            <h2
              id="course-catalog-heading"
              className="text-lg sm:text-xl font-extrabold tracking-tight text-slate-900 dark:text-white flex items-center gap-2"
            >
              <GraduationCap className="w-5 h-5 text-indigo-600 dark:text-indigo-400 shrink-0" />
              <span>Khóa học của tôi</span>
            </h2>
            <span className="px-2.5 py-0.5 rounded-full text-xs font-bold bg-indigo-50 text-indigo-800 dark:bg-indigo-950/70 dark:text-indigo-300 border border-indigo-200/80 dark:border-indigo-800/70">
              {courses.length} khóa học
            </span>
          </div>
          <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-400">
            Lộ trình học tập hợp nhất từ kho tài liệu{" "}
            <code className="font-mono text-[11px] px-1.5 py-0.5 rounded bg-slate-100 dark:bg-slate-800 text-slate-800 dark:text-slate-200">
              document/
            </code>{" "}
            kèm Flashcard FSRS &amp; Quiz thực hành.
          </p>
        </div>

        {/* Search + Sync Controls */}
        <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-2.5">
          <div className="relative flex-1 sm:w-80">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none" />
            <input
              type="text"
              data-testid="course-search-input"
              value={courseSearchQuery}
              onChange={(e) => setCourseSearchQuery(e.target.value)}
              placeholder="Tìm khóa học (FullStack, IELTS...)"
              aria-label="Tìm kiếm khóa học"
              className="w-full min-h-[44px] pl-9 pr-9 py-2 rounded-xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-950 text-xs sm:text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 transition"
            />
            {courseSearchQuery && (
              <button
                type="button"
                onClick={() => setCourseSearchQuery("")}
                aria-label="Xóa từ khóa tìm kiếm"
                className="absolute right-2 top-1/2 -translate-y-1/2 p-1.5 rounded-lg text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 cursor-pointer"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            )}
          </div>

          <button
            type="button"
            data-testid="sync-documents-btn"
            onClick={handleSyncFromDocuments}
            disabled={syncing}
            className="min-h-[44px] px-4 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 disabled:opacity-60 text-slate-800 dark:text-slate-100 text-xs sm:text-sm font-semibold inline-flex items-center justify-center gap-2 cursor-pointer transition shrink-0"
          >
            <RefreshCw className={`w-4 h-4 text-indigo-600 dark:text-indigo-400 ${syncing ? "animate-spin" : ""}`} />
            <span>{syncing ? "Đang đồng bộ..." : "Đồng bộ document/"}</span>
          </button>
        </div>
      </div>

      {/* Category Filter Pills */}
      <div
        role="group"
        aria-label="Lọc khóa học theo lĩnh vực"
        className="flex flex-wrap items-center gap-2 border-b border-slate-100 dark:border-slate-800/80 pb-4"
      >
        {CATEGORY_PILLS.map((pill) => {
          const isSelected = selectedPillKey === pill.key;
          const count = categoryCounts[pill.key];
          return (
            <button
              key={pill.key}
              type="button"
              data-testid={pill.testId}
              aria-pressed={isSelected}
              onClick={() => setSelectedCategory(pill.label)}
              className={`min-h-[44px] px-3.5 py-1.5 rounded-full text-xs font-bold inline-flex items-center gap-1.5 transition cursor-pointer border ${
                isSelected
                  ? "bg-indigo-600 border-indigo-600 text-white shadow-xs"
                  : "bg-slate-50 dark:bg-slate-800/80 border-slate-200 dark:border-slate-700 text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800"
              }`}
            >
              <span>{pill.label}</span>
              <span
                className={`px-1.5 py-0.2 rounded-full text-[10px] font-extrabold ${
                  isSelected
                    ? "bg-white/20 text-white"
                    : "bg-slate-200/80 dark:bg-slate-700 text-slate-700 dark:text-slate-300"
                }`}
              >
                {count}
              </span>
            </button>
          );
        })}
      </div>

      {/* Sync Toast / Feedback */}
      {syncNotice && (
        <div
          data-testid="catalog-sync-toast"
          role="status"
          className={`p-3.5 rounded-xl border text-xs sm:text-sm font-semibold flex items-center justify-between gap-3 ${
            syncNotice.type === "success"
              ? "bg-emerald-50 dark:bg-emerald-950/50 border-emerald-200 dark:border-emerald-800 text-emerald-800 dark:text-emerald-200"
              : "bg-rose-50 dark:bg-rose-950/50 border-rose-200 dark:border-rose-800 text-rose-800 dark:text-rose-200"
          }`}
        >
          <div className="flex items-center gap-2">
            {syncNotice.type === "success" ? (
              <CheckCircle2 className="w-4 h-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
            ) : (
              <AlertCircle className="w-4 h-4 shrink-0 text-rose-600 dark:text-rose-400" />
            )}
            <span>{syncNotice.text}</span>
          </div>
          <button
            type="button"
            onClick={() => setSyncNotice(null)}
            aria-label="Đóng thông báo"
            className="p-1 rounded-lg hover:bg-black/5 dark:hover:bg-white/10 cursor-pointer"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Backend Error Alert */}
      {errorMessage && (
        <div
          data-testid="courses-error-alert"
          role="alert"
          className="p-4 rounded-xl bg-rose-50 dark:bg-rose-950/40 border border-rose-200 dark:border-rose-800/80 text-rose-800 dark:text-rose-200 text-xs sm:text-sm flex flex-wrap items-center justify-between gap-3"
        >
          <div className="flex items-center gap-2.5 font-semibold">
            <AlertCircle className="w-4 h-4 shrink-0 text-rose-600 dark:text-rose-400" />
            <span>{errorMessage}</span>
          </div>
          <button
            type="button"
            onClick={() => onRefresh()}
            className="min-h-[40px] px-3.5 py-1.5 rounded-lg bg-rose-600 hover:bg-rose-700 text-white text-xs font-bold cursor-pointer transition"
          >
            Tải lại danh sách
          </button>
        </div>
      )}

      {/* Loading Skeletons */}
      {isLoading && courses.length === 0 ? (
        <div
          data-testid="courses-loading-skeleton"
          className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5"
        >
          {[0, 1, 2].map((idx) => (
            <div
              key={idx}
              className="rounded-2xl border border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-950/50 p-5 space-y-4 animate-pulse"
            >
              <div className="flex items-center justify-between">
                <div className="h-5 w-28 rounded-full bg-slate-200 dark:bg-slate-800" />
                <div className="h-5 w-20 rounded-full bg-slate-200 dark:bg-slate-800" />
              </div>
              <div className="h-6 w-3/4 rounded-lg bg-slate-200 dark:bg-slate-800" />
              <div className="h-10 w-full rounded-lg bg-slate-200/80 dark:bg-slate-800/80" />
              <div className="h-2.5 w-full rounded-full bg-slate-200 dark:bg-slate-800" />
              <div className="h-11 w-full rounded-xl bg-slate-200 dark:bg-slate-800" />
            </div>
          ))}
        </div>
      ) : courses.length === 0 && !errorMessage ? (
        /* Empty Catalog State */
        <div className="py-12 px-4 text-center rounded-2xl border border-dashed border-slate-300 dark:border-slate-700 bg-slate-50/50 dark:bg-slate-950/40 space-y-4">
          <div className="w-12 h-12 rounded-2xl bg-indigo-50 dark:bg-indigo-950/70 text-indigo-600 dark:text-indigo-400 flex items-center justify-center mx-auto">
            <BookOpen className="w-6 h-6" />
          </div>
          <div className="space-y-1">
            <h3 className="text-sm sm:text-base font-bold text-slate-900 dark:text-white">
              Chưa có khóa học nào trong danh mục
            </h3>
            <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-400 max-w-md mx-auto">
              Nhấn &ldquo;Đồng bộ document/&rdquo; để nhập toàn bộ giáo trình từ thư mục gốc hoặc tạo khóa học mới từ PDF/AI Wizard.
            </p>
          </div>
          <div className="flex flex-wrap items-center justify-center gap-2.5">
            <button
              type="button"
              onClick={handleSyncFromDocuments}
              disabled={syncing}
              className="min-h-[44px] px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs sm:text-sm font-bold inline-flex items-center gap-2 cursor-pointer transition"
            >
              <RefreshCw className={`w-4 h-4 ${syncing ? "animate-spin" : ""}`} />
              <span>Đồng bộ document/ ngay</span>
            </button>
            <button
              type="button"
              onClick={onOpenCreateModal}
              className="min-h-[44px] px-4 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 text-slate-800 dark:text-slate-200 text-xs sm:text-sm font-semibold inline-flex items-center gap-2 cursor-pointer transition"
            >
              <Sparkles className="w-4 h-4 text-indigo-500" />
              <span>Tạo khóa học mới</span>
            </button>
          </div>
        </div>
      ) : filteredCourses.length === 0 ? (
        /* Filter/Search No Match */
        <div className="py-10 px-4 text-center rounded-2xl border border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-950/30 space-y-3">
          <div className="text-sm font-bold text-slate-800 dark:text-slate-200">
            Không tìm thấy khóa học phù hợp
          </div>
          <p className="text-xs text-slate-600 dark:text-slate-400">
            Thử đổi từ khóa tìm kiếm hoặc quay về danh mục &ldquo;Tất cả&rdquo;.
          </p>
          <button
            type="button"
            onClick={() => {
              setCourseSearchQuery("");
              setSelectedCategory("Tất cả");
            }}
            className="min-h-[44px] px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer transition"
          >
            Xóa bộ lọc
          </button>
        </div>
      ) : (
        /* Spacious Responsive Card Grid */
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5">
          {filteredCourses.map((course) => {
            const cid = course.courseId || course.id || "";
            const domain = resolveCourseDomain(course);
            const DomainIcon = domain.Icon;

            const chapterCount = course.chapterCount ?? course.chapters?.length ?? 0;
            const computedLessonCount =
              course.chapters?.reduce((acc, ch) => acc + (ch.lessons?.length ?? 0), 0) ?? 0;
            const lessonCount = course.lessonCount ?? computedLessonCount;
            const flashcardCount = course.flashcardCount ?? 0;
            const quizCount = course.quizCount ?? (course.quizId ? 1 : 0);

            const prog = courseProgressMap[cid] || {
              percentCompleted: 0,
              completedLessons: 0,
              totalLessons: lessonCount,
              completedLessonIds: []
            };
            const percent = Math.max(0, Math.min(100, prog.percentCompleted));
            const totalLessonsDisplay = prog.totalLessons || lessonCount;

            const nextLesson = resolveNextCourseLesson(course, prog.completedLessonIds);

            let statusBadgeLabel = "Chưa học (0%)";
            let statusBadgeClass =
              "bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300 border border-slate-200 dark:border-slate-700";

            if (percent >= 100) {
              statusBadgeLabel = "Đã hoàn thành";
              statusBadgeClass =
                "bg-emerald-100 text-emerald-800 dark:bg-emerald-950/80 dark:text-emerald-300 border border-emerald-300 dark:border-emerald-800";
            } else if (percent > 0) {
              statusBadgeLabel = `Đang học ${percent}%`;
              statusBadgeClass =
                "bg-indigo-100 text-indigo-800 dark:bg-indigo-950/80 dark:text-indigo-200 border border-indigo-300 dark:border-indigo-800";
            }

            const primaryCtaLabel =
              percent >= 100
                ? "Ôn lại bài học"
                : percent > 0
                ? "Tiếp tục học"
                : "Vào học ngay";

            return (
              <article
                key={`course-${cid || course.slug || course.title}`}
                data-testid="course-card"
                data-course-id={cid}
                data-course-slug={course.slug || ""}
                className="group relative rounded-2xl border border-slate-200/90 dark:border-slate-800 bg-white dark:bg-slate-900/95 shadow-2xs hover:shadow-md hover:border-indigo-300 dark:hover:border-indigo-700/80 transition-all overflow-hidden flex flex-col justify-between"
              >
                {/* Top Domain Accent Bar */}
                <div className={`h-1.5 w-full ${domain.accentBarClass}`} />

                <div className="p-5 flex-1 flex flex-col justify-between space-y-4">
                  {/* Top Meta: Domain Badge + Progress Status */}
                  <div className="space-y-3">
                    <div className="flex items-center justify-between gap-2">
                      <span
                        className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-bold border ${domain.badgeClass}`}
                      >
                        <DomainIcon className="w-3.5 h-3.5 shrink-0" />
                        <span className="truncate max-w-[165px]">{domain.badgeLabel}</span>
                      </span>

                      <span
                        data-testid="course-progress-badge"
                        className={`px-2.5 py-1 rounded-full text-[11px] font-bold shrink-0 ${statusBadgeClass}`}
                      >
                        {statusBadgeLabel}
                      </span>
                    </div>

                    {/* Course Title & Description */}
                    <div>
                      <h3
                        data-testid="course-card-title"
                        className="text-base sm:text-lg font-extrabold text-slate-900 dark:text-white leading-snug line-clamp-2 group-hover:text-indigo-700 dark:group-hover:text-indigo-300 transition-colors"
                      >
                        {course.title}
                      </h3>
                      {course.description && (
                        <p className="text-xs text-slate-600 dark:text-slate-400 mt-1.5 line-clamp-2 leading-relaxed">
                          {course.description}
                        </p>
                      )}
                    </div>

                    {/* Curriculum Metrics */}
                    <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5 text-[11px] font-semibold text-slate-600 dark:text-slate-300">
                      <span className="inline-flex items-center gap-1">
                        <Layers className="w-3.5 h-3.5 text-slate-400" />
                        <span>{chapterCount} chương</span>
                      </span>
                      <span className="inline-flex items-center gap-1">
                        <BookOpen className="w-3.5 h-3.5 text-indigo-500" />
                        <span>{lessonCount} bài</span>
                      </span>
                      <span className="inline-flex items-center gap-1">
                        <Brain className="w-3.5 h-3.5 text-sky-500" />
                        <span>{flashcardCount} thẻ</span>
                      </span>
                      <span className="inline-flex items-center gap-1">
                        <Target className="w-3.5 h-3.5 text-emerald-500" />
                        <span>{quizCount} quiz</span>
                      </span>
                    </div>

                    {/* Next Lesson Preview */}
                    {nextLesson.title && (
                      <div className="px-3 py-2 rounded-xl bg-slate-50 dark:bg-slate-950/60 border border-slate-200/70 dark:border-slate-800 text-xs">
                        <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 dark:text-slate-400">
                          {nextLesson.allCompleted ? "Đã hoàn tất • Ôn tập từ" : "Bài tiếp theo"}
                        </div>
                        <div
                          className="font-semibold text-slate-800 dark:text-slate-200 truncate mt-0.5"
                          title={nextLesson.title}
                        >
                          {nextLesson.title}
                        </div>
                      </div>
                    )}
                  </div>

                  {/* Progress & Actions Footer */}
                  <div className="space-y-3 pt-2 border-t border-slate-100 dark:border-slate-800/80">
                    {/* Progress Bar */}
                    <div className="space-y-1.5">
                      <div className="flex items-center justify-between text-xs">
                        <span className="font-semibold text-slate-600 dark:text-slate-400">
                          Tiến độ học tập
                        </span>
                        <span
                          data-testid="course-progress-percent"
                          className="font-bold text-slate-800 dark:text-slate-200"
                        >
                          {prog.completedLessons}/{totalLessonsDisplay} bài ({percent}%)
                        </span>
                      </div>
                      <div
                        data-testid="course-card-progress-bar-container"
                        role="progressbar"
                        aria-label={`Tiến độ khóa học ${course.title}`}
                        aria-valuenow={percent}
                        aria-valuemin={0}
                        aria-valuemax={100}
                        className="w-full bg-slate-200 dark:bg-slate-800 h-2 rounded-full overflow-hidden"
                      >
                        <div
                          data-testid="course-card-progress-bar-fill"
                          className={`h-full rounded-full transition-all duration-300 ${
                            percent >= 100
                              ? "bg-emerald-500"
                              : "bg-gradient-to-r from-indigo-500 to-indigo-600"
                          }`}
                          style={{ width: `${percent}%` }}
                        />
                      </div>
                    </div>

                    {/* Primary 1-Click Launch CTA */}
                    <button
                      type="button"
                      data-testid="course-start-learning-btn"
                      onClick={() => {
                        if (cid) setActiveCourseId(cid);
                        const targetLessonId = nextLesson.id ?? course.firstLessonId;
                        if (targetLessonId) {
                          setActiveLesson(targetLessonId);
                        } else if (course.sourceDocumentId) {
                          setActiveDocument(course.sourceDocumentId);
                        }
                        setActiveMode("reading");
                      }}
                      className="w-full min-h-[44px] px-4 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs sm:text-sm font-bold shadow-2xs transition cursor-pointer flex items-center justify-center gap-2"
                    >
                      <Play className="w-4 h-4 fill-current" />
                      <span>{primaryCtaLabel}</span>
                    </button>

                    {/* Compact Secondary Actions Row */}
                    <div
                      className={`grid gap-2 ${
                        course.sourceDocumentId ? "grid-cols-4" : "grid-cols-3"
                      }`}
                    >
                      <button
                        type="button"
                        data-testid="course-review-flashcards-btn"
                        title="Ôn Flashcard khóa này"
                        onClick={() => {
                          if (cid) setActiveCourseId(cid);
                          setSelectedTagFilterId(course.tagId ?? null);
                          setActiveReviewSubTab("fsrs");
                          setActiveMode("review");
                        }}
                        className="min-h-[44px] px-2 py-2 rounded-xl border border-indigo-200 dark:border-indigo-800/70 bg-indigo-50/70 dark:bg-indigo-950/50 hover:bg-indigo-600 hover:text-white text-indigo-800 dark:text-indigo-200 text-xs font-bold cursor-pointer transition flex items-center justify-center gap-1"
                      >
                        <Brain className="w-3.5 h-3.5 shrink-0" />
                        <span className="truncate">Thẻ</span>
                      </button>

                      <button
                        type="button"
                        data-testid="course-take-quiz-btn"
                        title="Làm Quiz khóa này"
                        onClick={() => {
                          if (cid) setActiveCourseId(cid);
                          const mappedQuizId =
                            course.quizId ??
                            (course.lessonQuizMap
                              ? Object.values(course.lessonQuizMap)[0]
                              : undefined) ??
                            null;
                          setActiveQuizId(mappedQuizId);
                          setActiveReviewSubTab("quiz");
                          setActiveMode("review");
                        }}
                        className="min-h-[44px] px-2 py-2 rounded-xl border border-emerald-200 dark:border-emerald-800/70 bg-emerald-50/70 dark:bg-emerald-950/50 hover:bg-emerald-600 hover:text-white text-emerald-800 dark:text-emerald-200 text-xs font-bold cursor-pointer transition flex items-center justify-center gap-1"
                      >
                        <Target className="w-3.5 h-3.5 shrink-0" />
                        <span className="truncate">Quiz</span>
                      </button>

                      <button
                        type="button"
                        data-testid="open-in-studio-btn"
                        title="Mở trong Studio"
                        onClick={() => {
                          if (cid) {
                            setActiveCourseId(cid);
                            setEditingCourseId(cid);
                          }
                          setActiveMode("studio");
                        }}
                        className="min-h-[44px] px-2 py-2 rounded-xl border border-purple-200 dark:border-purple-800/70 bg-purple-50/70 dark:bg-purple-950/50 hover:bg-purple-600 hover:text-white text-purple-800 dark:text-purple-200 text-xs font-bold cursor-pointer transition flex items-center justify-center gap-1"
                      >
                        <PenTool className="w-3.5 h-3.5 shrink-0" />
                        <span className="truncate">Studio</span>
                      </button>

                      {course.sourceDocumentId && (
                        <button
                          type="button"
                          data-testid="course-open-pdf-btn"
                          title="Đọc PDF gốc"
                          onClick={() => {
                            if (cid) setActiveCourseId(cid);
                            setActiveDocument(course.sourceDocumentId!);
                            setActiveMode("reading");
                          }}
                          className="min-h-[44px] px-2 py-2 rounded-xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800/80 hover:bg-slate-200 dark:hover:bg-slate-700 text-slate-800 dark:text-slate-200 text-xs font-bold cursor-pointer transition flex items-center justify-center gap-1"
                        >
                          <FileText className="w-3.5 h-3.5 shrink-0" />
                          <span className="truncate">PDF</span>
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </section>
  );
}
