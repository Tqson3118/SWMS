"use client";

import React, { useEffect, useMemo, useState } from "react";
import {
  BookOpen,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  Clock,
  ChevronsUpDown,
  Search,
  X
} from "lucide-react";

export interface RoadmapLessonItem {
  id: string;
  title: string;
  slug?: string;
  sortOrder?: number;
  estimatedMinutes?: number;
  startPage?: number | null;
  endPage?: number | null;
  quizId?: string | null;
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
  compact = false
}: CourseRoadmapSidebarProps) {
  const [searchQuery, setSearchQuery] = useState("");
  const [expandedChapters, setExpandedChapters] = useState<Record<string, boolean>>({});

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

  const normalizedQuery = normalizeForSearch(searchQuery);

  const filteredChapters = useMemo(() => {
    if (!normalizedQuery) return normalizedChapters;
    return normalizedChapters
      .map((ch) => {
        const chapterMatch = normalizeForSearch(ch.title).includes(normalizedQuery);
        const matchingLessons = (ch.lessons || []).filter((les) => {
          const title = localLessonTitles[les.id] || les.title || "";
          return chapterMatch || normalizeForSearch(title).includes(normalizedQuery);
        });
        return {
          ...ch,
          lessons: matchingLessons
        };
      })
      .filter((ch) => ch.lessons.length > 0);
  }, [normalizedChapters, normalizedQuery, localLessonTitles]);

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
      data-testid="course-roadmap-sidebar"
      className="flex flex-col flex-1 min-h-0 min-w-0 w-full space-y-2.5 text-xs"
    >
      {/* Course Mini Progress Summary */}
      <div className="rounded-xl bg-indigo-50/70 dark:bg-indigo-950/40 border border-indigo-200/70 dark:border-indigo-900/60 p-2.5 space-y-1.5">
        <div className="flex items-center justify-between gap-2">
          <span className="font-extrabold text-indigo-950 dark:text-indigo-100 truncate text-[11px]">
            {courseTitle}
          </span>
          <span className="shrink-0 px-1.5 py-0.5 rounded-md bg-indigo-600 text-white text-[10px] font-extrabold">
            {percentCompleted}%
          </span>
        </div>
        <div className="w-full h-1.5 rounded-full bg-indigo-200/70 dark:bg-indigo-950 overflow-hidden">
          <div
            className={`h-full rounded-full transition-all duration-300 ${
              percentCompleted === 100
                ? "bg-emerald-500"
                : "bg-gradient-to-r from-indigo-500 to-emerald-500"
            }`}
            style={{ width: `${percentCompleted}%` }}
          />
        </div>
        <div className="flex items-center justify-between text-[10px] text-slate-600 dark:text-slate-400 font-medium">
          <span>
            {completedCount}/{totalCount} bài hoàn thành
          </span>
          <span>{normalizedChapters.length} chương</span>
        </div>
      </div>

      {/* Search & Expand/Collapse All Controls */}
      <div className="space-y-1.5">
        <div className="relative flex items-center">
          <Search className="w-3.5 h-3.5 text-slate-400 absolute left-2.5 pointer-events-none" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            data-testid="roadmap-lesson-search-input"
            placeholder="Tìm bài học..."
            aria-label="Tìm bài học trong khóa"
            className="w-full pl-8 pr-7 py-1.5 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-950 text-xs text-slate-800 dark:text-slate-200 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
          {searchQuery && (
            <button
              type="button"
              onClick={() => setSearchQuery("")}
              aria-label="Xóa tìm kiếm bài học"
              className="absolute right-2 p-0.5 rounded text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 cursor-pointer"
            >
              <X className="w-3 h-3" />
            </button>
          )}
        </div>

        <div className="flex items-center justify-between text-[11px] text-slate-500 dark:text-slate-400 px-0.5">
          <span>
            {normalizedQuery
              ? `Tìm thấy ${totalFilteredLessons} bài`
              : `${normalizedChapters.length} chương • ${totalCount} bài`}
          </span>
          {normalizedChapters.length > 1 && (
            <button
              type="button"
              data-testid="roadmap-toggle-all-chapters-btn"
              onClick={handleToggleExpandAll}
              className="inline-flex items-center gap-1 font-semibold text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer"
            >
              <ChevronsUpDown className="w-3 h-3" />
              <span>{allExpanded ? "Thu gọn" : "Mở hết"}</span>
            </button>
          )}
        </div>
      </div>

      {/* Collapsible Chapters -> Lessons Tree */}
      <div
        className={`space-y-2 flex-1 min-h-0 overflow-y-auto swms-thin-scrollbar pr-0.5 ${
          compact ? "lg:max-h-none max-h-[46vh]" : "lg:max-h-none max-h-[60vh]"
        }`}
      >
        {filteredChapters.length === 0 ? (
          <div className="p-3 rounded-xl border border-dashed border-slate-200 dark:border-slate-800 text-center text-[11px] text-slate-500 dark:text-slate-400">
            Không tìm thấy bài học phù hợp
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
                className={`rounded-xl border transition-colors ${
                  hasActiveLesson
                    ? "border-indigo-300/80 dark:border-indigo-800/80 bg-indigo-50/30 dark:bg-indigo-950/20"
                    : "border-slate-200/80 dark:border-slate-800 bg-white dark:bg-slate-900/60"
                }`}
              >
                <button
                  type="button"
                  data-testid={`roadmap-chapter-toggle-${chIdx}`}
                  onClick={() => toggleChapter(chapter.id)}
                  aria-expanded={isExpanded}
                  className="w-full px-2.5 py-2 flex items-start justify-between gap-2 text-left cursor-pointer hover:bg-slate-50/80 dark:hover:bg-slate-800/50 rounded-xl transition"
                >
                  <div className="flex items-start gap-1.5 min-w-0">
                    {isExpanded ? (
                      <ChevronDown className="w-3.5 h-3.5 text-slate-500 shrink-0 mt-0.5" />
                    ) : (
                      <ChevronRight className="w-3.5 h-3.5 text-slate-500 shrink-0 mt-0.5" />
                    )}
                    <span className="font-bold text-slate-800 dark:text-slate-200 text-[11px] leading-snug line-clamp-2 break-words">
                      {chapter.title}
                    </span>
                  </div>
                  <span
                    className={`shrink-0 px-1.5 py-0.5 rounded-md text-[10px] font-bold ${
                      chapterDone
                        ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-300"
                        : "bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300"
                    }`}
                  >
                    {chapterCompletedCount}/{chapterLessons.length}
                  </span>
                </button>

                {isExpanded && (
                  <div className="px-1.5 pb-1.5 pt-0.5 space-y-1">
                    {chapterLessons.map((les, lesIdx) => {
                      const isActive = les.id === activeLessonId;
                      const isCompleted = completedSet.has(les.id);
                      const displayTitle =
                        localLessonTitles[les.id] || les.title || `Bài ${lesIdx + 1}`;

                      return (
                        <button
                          key={les.id}
                          type="button"
                          data-testid="course-syllabus-lesson-btn"
                          aria-current={isActive ? "page" : undefined}
                          onClick={() => onSelectLesson(les.id)}
                          className={`w-full min-h-[36px] px-2 py-1.5 rounded-lg text-left transition cursor-pointer flex items-start justify-between gap-2 ${
                            isActive
                              ? "bg-indigo-600 text-white shadow-xs"
                              : "text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800/80"
                          }`}
                        >
                          <div className="flex items-start gap-1.5 min-w-0">
                            {isCompleted ? (
                              <CheckCircle2
                                data-testid="lesson-completed-icon"
                                className={`w-3.5 h-3.5 shrink-0 mt-0.5 ${
                                  isActive ? "text-emerald-300" : "text-emerald-500"
                                }`}
                              />
                            ) : (
                              <BookOpen
                                className={`w-3.5 h-3.5 shrink-0 mt-0.5 ${
                                  isActive ? "text-indigo-100" : "text-slate-400"
                                }`}
                              />
                            )}
                            <span className="text-[11px] font-semibold leading-snug line-clamp-2 break-words">
                              {displayTitle}
                            </span>
                          </div>

                          <div className="shrink-0 flex items-center gap-1 mt-0.5">
                            {typeof les.estimatedMinutes === "number" &&
                              les.estimatedMinutes > 0 && (
                                <span
                                  className={`text-[10px] inline-flex items-center gap-0.5 ${
                                    isActive
                                      ? "text-indigo-100"
                                      : "text-slate-400 dark:text-slate-500"
                                  }`}
                                >
                                  <Clock className="w-2.5 h-2.5" />
                                  <span>{les.estimatedMinutes}p</span>
                                </span>
                              )}
                            {les.startPage && (
                              <span
                                className={`text-[10px] ${
                                  isActive
                                    ? "text-indigo-100"
                                    : "text-slate-400 dark:text-slate-500"
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
    </div>
  );
}
