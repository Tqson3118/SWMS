"use client";

import React, { useEffect, useMemo, useRef, useState } from "react";
import {
  BookOpen,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  Clock,
  ChevronsUpDown,
  Search,
  Sparkles,
  X
} from "lucide-react";
import { stripChapterPrefix } from "@/lib/course-helpers";

export interface RoadmapLessonItem {
  id: string;
  title: string;
  slug?: string;
  sortOrder?: number;
  estimatedMinutes?: number;
  startPage?: number | null;
  endPage?: number | null;
  quizId?: string | null;
  isBookmarked?: boolean;
  needsReview?: boolean;
}

export interface RoadmapChapterItem {
  id: string;
  title: string;
  sortOrder?: number;
  lessons: RoadmapLessonItem[];
}

export interface CourseRoadmapSidebarProps {
  courseTitle: string;
  chapters: RoadmapChapterItem[];
  flatLessons?: RoadmapLessonItem[];
  activeLessonId: string;
  completedLessonIds: string[];
  localLessonTitles?: Record<string, string>;
  percentCompleted: number;
  completedCount: number;
  totalCount: number;
  onSelectLesson: (lessonId: string) => void;
  compact?: boolean;
  testId?: string;
}

function normalizeForSearch(input: string): string {
  return input
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase()
    .trim();
}

export function CourseRoadmapSidebar({
  courseTitle,
  chapters,
  flatLessons = [],
  activeLessonId,
  completedLessonIds,
  localLessonTitles = {},
  percentCompleted,
  completedCount,
  totalCount,
  onSelectLesson,
  compact = false,
  testId = "course-roadmap-sidebar"
}: CourseRoadmapSidebarProps) {
  const [searchQuery, setSearchQuery] = useState("");
  const [flagFilter, setFlagFilter] = useState<"all" | "bookmarked" | "review">("all");
  const [expandedChapters, setExpandedChapters] = useState<Record<string, boolean>>({});
  const activeLessonRef = useRef<HTMLButtonElement | null>(null);
  const pendingScrollLessonIdRef = useRef<string | null>(null);

  const normalizedChapters: RoadmapChapterItem[] = useMemo(() => {
    if (chapters && chapters.length > 0) {
      return chapters;
    }
    if (flatLessons && flatLessons.length > 0) {
      return [
        {
          id: "virtual-default-chapter",
          title: "Danh sách bài học",
          sortOrder: 1,
          lessons: flatLessons
        }
      ];
    }
    return [];
  }, [chapters, flatLessons]);

  const completedSet = useMemo(() => new Set(completedLessonIds), [completedLessonIds]);

  const { bookmarkedCount, needsReviewCount } = useMemo(() => {
    let bookmarked = 0;
    let review = 0;
    for (const ch of normalizedChapters) {
      for (const les of ch.lessons || []) {
        if (les.isBookmarked) bookmarked += 1;
        if (les.needsReview) review += 1;
      }
    }
    return { bookmarkedCount: bookmarked, needsReviewCount: review };
  }, [normalizedChapters]);

  // Default expand chapter containing activeLessonId (or first non-empty chapter)
  useEffect(() => {
    if (normalizedChapters.length === 0) return;
    const activeChapter =
      normalizedChapters.find((ch) =>
        (ch.lessons || []).some((l) => l.id === activeLessonId)
      ) ?? normalizedChapters.find((ch) => (ch.lessons || []).length > 0);

    if (!activeChapter) return;
    setExpandedChapters((prev) => {
      if (prev[activeChapter.id]) return prev;
      return { ...prev, [activeChapter.id]: true };
    });
  }, [activeLessonId, normalizedChapters]);

  // Mark activeLessonId for smooth scroll into view whenever it changes
  useEffect(() => {
    if (activeLessonId) {
      pendingScrollLessonIdRef.current = activeLessonId;
    }
  }, [activeLessonId]);

  // Smoothly scroll the active lesson button into view once mounted/expanded
  useEffect(() => {
    if (!activeLessonId || pendingScrollLessonIdRef.current !== activeLessonId) return;
    const el = activeLessonRef.current;
    if (!el) return;
    pendingScrollLessonIdRef.current = null;
    el.scrollIntoView({ block: "nearest", behavior: "smooth" });
  }, [activeLessonId, expandedChapters, flagFilter, normalizedChapters]);

  const normalizedQuery = normalizeForSearch(searchQuery);

  const filteredChapters = useMemo(() => {
    return normalizedChapters
      .map((ch) => {
        const chapterMatch = normalizedQuery ? normalizeForSearch(ch.title).includes(normalizedQuery) : false;
        const matchingLessons = (ch.lessons || []).filter((les) => {
          if (flagFilter === "bookmarked" && !les.isBookmarked) return false;
          if (flagFilter === "review" && !les.needsReview) return false;
          if (!normalizedQuery) return true;
          const title = localLessonTitles[les.id] || les.title || "";
          return chapterMatch || normalizeForSearch(title).includes(normalizedQuery);
        });
        return {
          ...ch,
          lessons: matchingLessons,
        };
      })
      .filter((ch) => ch.lessons.length > 0);
  }, [normalizedChapters, normalizedQuery, flagFilter, localLessonTitles]);

  const totalFilteredLessons = useMemo(
    () => filteredChapters.reduce((acc, ch) => acc + ch.lessons.length, 0),
    [filteredChapters]
  );

  const allExpanded =
    filteredChapters.length > 0 &&
    filteredChapters.every((ch) => Boolean(expandedChapters[ch.id]));

  const handleToggleExpandAll = () => {
    const nextState: Record<string, boolean> = {};
    const target = !allExpanded;
    for (const ch of normalizedChapters) {
      nextState[ch.id] = target;
    }
    setExpandedChapters(nextState);
  };

  const toggleChapter = (chapterId: string) => {
    setExpandedChapters((prev) => ({
      ...prev,
      [chapterId]: !prev[chapterId]
    }));
  };

  return (
    <div
      data-testid={testId}
      className="flex flex-col flex-1 h-full min-h-0 min-w-0 w-full space-y-2.5 text-xs"
    >
      {/* Course Mini Progress Summary */}
      <div className="rounded-lg bg-bg-primary border border-border p-2.5 space-y-1.5">
        <div className="flex items-start justify-between gap-2">
          <span
            title={courseTitle}
            className="font-semibold text-text-primary line-clamp-2 leading-snug text-[11px]"
          >
            {courseTitle}
          </span>
          <span className="shrink-0 px-1.5 py-0.5 rounded bg-accent-subtle text-accent text-[10px] font-semibold">
            {percentCompleted}%
          </span>
        </div>
        <div className="w-full h-1.5 rounded-full bg-bg-tertiary overflow-hidden">
          <div
            className={`h-full rounded-full transition-all duration-300 ${
              percentCompleted === 100 ? "bg-emerald-500" : "bg-accent"
            }`}
            style={{ width: `${percentCompleted}%` }}
          />
        </div>
        <div className="flex items-center justify-between text-[10px] text-text-tertiary font-medium">
          <span>
            {completedCount}/{totalCount} bài hoàn thành
          </span>
          <span>{normalizedChapters.length} chương</span>
        </div>
      </div>

      {/* Search & Expand/Collapse All Controls */}
      <div className="space-y-1.5">
        <div className="relative flex items-center">
          <Search className="w-3.5 h-3.5 text-text-tertiary absolute left-2.5 pointer-events-none" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            data-testid="roadmap-lesson-search-input"
            placeholder="Tìm bài học..."
            aria-label="Tìm bài học trong khóa"
            className="w-full pl-8 pr-7 py-1.5 rounded-md border border-border bg-bg-primary text-xs text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-accent"
          />
          {searchQuery && (
            <button
              type="button"
              onClick={() => setSearchQuery("")}
              aria-label="Xóa tìm kiếm bài học"
              className="absolute right-2 p-0.5 rounded text-text-tertiary hover:text-text-primary cursor-pointer"
            >
              <X className="w-3 h-3" />
            </button>
          )}
        </div>

        {/* Filter Pills: Tất cả | Đánh dấu | Cần ôn lại */}
        <div className="flex items-center gap-1 p-0.5 rounded-lg bg-bg-secondary border border-border text-[10px]">
          <button
            type="button"
            onClick={() => setFlagFilter("all")}
            data-testid="roadmap-filter-all"
            className={`flex-1 py-1 px-1 rounded text-center transition cursor-pointer font-medium ${
              flagFilter === "all"
                ? "bg-bg-primary text-text-primary shadow-xs font-semibold"
                : "text-text-tertiary hover:text-text-primary"
            }`}
          >
            Tất cả
          </button>
          <button
            type="button"
            onClick={() => setFlagFilter("bookmarked")}
            data-testid="roadmap-filter-bookmarked"
            className={`flex-1 py-1 px-1 rounded text-center transition cursor-pointer font-medium flex items-center justify-center gap-1 min-w-0 whitespace-nowrap ${
              flagFilter === "bookmarked"
                ? "bg-bg-primary text-amber-600 dark:text-amber-400 font-semibold shadow-xs"
                : "text-text-tertiary hover:text-text-primary"
            }`}
          >
            <span>🔖</span>
            <span>Đánh dấu</span>
            {bookmarkedCount > 0 && (
              <span className="px-1 rounded-full bg-amber-500/15 text-amber-700 dark:text-amber-300 text-[9px] font-bold leading-tight">
                {bookmarkedCount}
              </span>
            )}
          </button>
          <button
            type="button"
            onClick={() => setFlagFilter("review")}
            data-testid="roadmap-filter-review"
            className={`flex-1 py-1 px-1 rounded text-center transition cursor-pointer font-medium flex items-center justify-center gap-1 min-w-0 whitespace-nowrap ${
              flagFilter === "review"
                ? "bg-bg-primary text-rose-600 dark:text-rose-400 font-semibold shadow-xs"
                : "text-text-tertiary hover:text-text-primary"
            }`}
          >
            <span>🔁</span>
            <span>Cần ôn</span>
            {needsReviewCount > 0 && (
              <span className="px-1 rounded-full bg-rose-500/15 text-rose-700 dark:text-rose-300 text-[9px] font-bold leading-tight">
                {needsReviewCount}
              </span>
            )}
          </button>
        </div>

        <div className="flex items-center justify-between text-[11px] text-text-tertiary px-0.5">
          <span>
            {normalizedQuery || flagFilter !== "all"
              ? `Tìm thấy ${totalFilteredLessons} bài`
              : `${normalizedChapters.length} chương • ${totalCount} bài`}
          </span>
          {normalizedChapters.length > 1 && (
            <button
              type="button"
              data-testid="roadmap-toggle-all-chapters-btn"
              onClick={handleToggleExpandAll}
              className="inline-flex items-center gap-1 font-medium text-accent hover:underline cursor-pointer"
            >
              <ChevronsUpDown className="w-3 h-3" />
              <span>{allExpanded ? "Thu gọn" : "Mở hết"}</span>
            </button>
          )}
        </div>
      </div>

      {/* Collapsible Chapters -> Lessons Tree */}
      <div
        className={`space-y-1.5 flex-1 min-h-0 overflow-y-auto swms-thin-scrollbar pr-0.5 ${
          compact ? "lg:max-h-none max-h-[46vh]" : "lg:max-h-none max-h-[60vh]"
        }`}
      >
        {filteredChapters.length === 0 ? (
          <div className="p-4 rounded-lg border border-dashed border-border text-center text-[11px] text-text-tertiary space-y-2">
            <p>Không tìm thấy bài học phù hợp</p>
            <button
              type="button"
              onClick={() => {
                setSearchQuery("");
                setFlagFilter("all");
              }}
              className="inline-flex items-center justify-center px-2.5 py-1 rounded-md border border-border bg-bg-primary text-[11px] font-medium text-accent hover:bg-bg-secondary transition cursor-pointer"
            >
              Hiện tất cả bài
            </button>
          </div>
        ) : (
          filteredChapters.map((chapter, chIdx) => {
            const chapterLessons = chapter.lessons || [];
            const isExpanded = normalizedQuery
              ? true
              : Boolean(expandedChapters[chapter.id]);
            const chapterCompletedCount = chapterLessons.filter((l) =>
              completedSet.has(l.id)
            ).length;
            const chapterDone =
              chapterLessons.length > 0 && chapterCompletedCount === chapterLessons.length;
            const hasActiveLesson = chapterLessons.some((l) => l.id === activeLessonId);

            return (
              <div
                key={chapter.id}
                className={`rounded-lg border transition-colors ${
                  hasActiveLesson
                    ? "border-accent/40 bg-bg-primary"
                    : "border-border bg-bg-primary/60"
                }`}
              >
                <button
                  type="button"
                  data-testid={`roadmap-chapter-toggle-${chIdx}`}
                  onClick={() => toggleChapter(chapter.id)}
                  aria-expanded={isExpanded}
                  className="w-full px-2.5 py-2 flex items-start justify-between gap-2 text-left cursor-pointer hover:bg-bg-secondary rounded-lg transition"
                >
                  <div className="flex items-start gap-1.5 min-w-0">
                    {isExpanded ? (
                      <ChevronDown className="w-3.5 h-3.5 text-text-tertiary shrink-0 mt-0.5" />
                    ) : (
                      <ChevronRight className="w-3.5 h-3.5 text-text-tertiary shrink-0 mt-0.5" />
                    )}
                    <span className="font-semibold text-text-primary text-[11px] leading-snug line-clamp-2 break-words">
                      {stripChapterPrefix(chapter.title)
                        ? `Chương ${chIdx + 1}: ${stripChapterPrefix(chapter.title)}`
                        : chapter.title}
                    </span>
                  </div>
                  <span
                    className={`shrink-0 px-1.5 py-0.5 rounded text-[10px] font-semibold ${
                      chapterDone
                        ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                        : "bg-bg-tertiary text-text-secondary"
                    }`}
                  >
                    {chapterCompletedCount}/{chapterLessons.length}
                  </span>
                </button>

                {isExpanded && (
                  <div className="px-1.5 pb-1.5 pt-0.5 space-y-0.5">
                    {chapterLessons.map((les, lesIdx) => {
                      const isActive = les.id === activeLessonId;
                      const isCompleted = completedSet.has(les.id);
                      const displayTitle =
                        localLessonTitles[les.id] || les.title || `Bài ${lesIdx + 1}`;

                      return (
                        <button
                          key={les.id}
                          ref={isActive ? activeLessonRef : undefined}
                          type="button"
                          data-testid="course-syllabus-lesson-btn"
                          aria-current={isActive ? "page" : undefined}
                          onClick={() => onSelectLesson(les.id)}
                          className={`w-full min-h-[34px] px-2 py-1.5 rounded-md text-left transition cursor-pointer flex items-start justify-between gap-2 ${
                            isActive
                              ? "bg-accent text-white"
                              : "text-text-secondary hover:bg-bg-secondary hover:text-text-primary"
                          }`}
                        >
                          <div className="flex items-start gap-1.5 min-w-0">
                            {isCompleted ? (
                              <CheckCircle2
                                data-testid="lesson-completed-icon"
                                className={`w-3.5 h-3.5 shrink-0 mt-0.5 ${
                                  isActive ? "text-emerald-200" : "text-emerald-500"
                                }`}
                              />
                            ) : (
                              <BookOpen
                                className={`w-3.5 h-3.5 shrink-0 mt-0.5 ${
                                  isActive ? "text-white/80" : "text-text-tertiary"
                                }`}
                              />
                            )}
                            <span className="text-[11px] font-medium leading-snug line-clamp-2 break-words">
                              {displayTitle}
                            </span>
                          </div>

                          <div className="shrink-0 flex items-center gap-1 mt-0.5">
                            {les.isBookmarked && (
                              <span
                                title="Đã đánh dấu"
                                className={`text-[10px] ${isActive ? "text-white" : "text-amber-500"}`}
                              >
                                🔖
                              </span>
                            )}
                            {les.needsReview && (
                              <span
                                title="Cần ôn lại"
                                className={`text-[10px] ${isActive ? "text-white" : "text-rose-500"}`}
                              >
                                🔁
                              </span>
                            )}
                            {typeof les.estimatedMinutes === "number" &&
                              les.estimatedMinutes > 0 && (
                                <span
                                  className={`text-[10px] inline-flex items-center gap-0.5 ${
                                    isActive ? "text-white/80" : "text-text-tertiary"
                                  }`}
                                >
                                  <Clock className="w-2.5 h-2.5" />
                                  <span>{les.estimatedMinutes}p</span>
                                </span>
                              )}
                            {les.startPage && (
                              <span
                                className={`text-[10px] ${
                                  isActive ? "text-white/80" : "text-text-tertiary"
                                }`}
                              >
                                Tr.{les.startPage}
                              </span>
                            )}
                          </div>
                        </button>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })
        )}
      </div>

      {/* Compact Study Tip Footer (prevents empty sidebar void on short courses) */}
      {!compact && (
        <div className="rounded-lg border border-border/80 bg-bg-primary/90 p-2.5 space-y-1 shrink-0">
          <div className="flex items-center gap-1.5 text-[10px] font-semibold text-accent">
            <Sparkles className="w-3 h-3 shrink-0" />
            <span>Mẹo học sâu hiệu quả</span>
          </div>
          <p className="text-[10px] text-text-tertiary leading-relaxed">
            Bôi đen đoạn quan trọng để tạo thẻ FSRS 1-click, rồi bấm Kiểm tra nhanh hoặc Luyện bài này để mở Phòng thực hành.
          </p>
        </div>
      )}
    </div>
  );
}
