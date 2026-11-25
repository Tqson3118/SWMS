"use client";

import React, { useState } from "react";
import Link from "next/link";
import {
  Bookmark,
  BookOpen,
  Check,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  ChevronsDownUp,
  ChevronsUpDown,
  ClipboardCheck,
  Clock,
  FlaskConical,
  HelpCircle,
  ListChecks,
  Lock,
  MessageSquare,
  Play,
  RotateCcw,
  Search,
  Send,
  Sparkles,
  Star,
  Target,
  Unlock,
  Users,
  X,
} from "lucide-react";
import { Button, Card, Badge, EmptyState } from "@/components/ui";
import { normalizeVi, stripChapterPrefix } from "@/lib/course-helpers";
import type {
  CourseChapter,
  CourseDetail,
  CourseLesson,
  CourseReviewItem,
} from "./types";

export type LessonFilter = "all" | "pending" | "completed" | "bookmarked" | "needsReview";

export interface CourseSyllabusTabProps {
  course: CourseDetail;
  courseId: string;
  isDone: (lesson: CourseLesson) => boolean;
  sequentialUnlock: boolean;
  handleToggleSequentialUnlock: () => void;
  unlockedLessonIds: Set<string>;
  nextLesson?: CourseLesson;
  expandedChapters: Set<string>;
  setExpandedChapters: React.Dispatch<React.SetStateAction<Set<string>>>;
  reviews: CourseReviewItem[];
  averageRating: number;
  handleSubmitReview: (e: React.FormEvent) => Promise<void>;
  ratingInput: number;
  setRatingInput: (val: number) => void;
  commentInput: string;
  setCommentInput: (val: string) => void;
  submittingReview: boolean;
  reviewNotice: string | null;
}

export function CourseSyllabusTab({
  course,
  courseId,
  isDone,
  sequentialUnlock,
  handleToggleSequentialUnlock,
  unlockedLessonIds,
  nextLesson,
  expandedChapters,
  setExpandedChapters,
  reviews,
  averageRating,
  handleSubmitReview,
  ratingInput,
  setRatingInput,
  commentInput,
  setCommentInput,
  submittingReview,
  reviewNotice,
}: CourseSyllabusTabProps) {
  const [searchQuery, setSearchQuery] = useState("");
  const [lessonFilter, setLessonFilter] = useState<LessonFilter>("all");

  const allLessons = course.chapters.flatMap((ch) => ch.lessons ?? []);

  const allExpanded =
    course.chapters.length > 0 &&
    course.chapters.every((ch) => expandedChapters.has(String(ch.chapterId || ch.id)));

  const toggleAllChapters = () => {
    if (allExpanded) {
      setExpandedChapters(new Set());
    } else {
      setExpandedChapters(new Set(course.chapters.map((ch) => String(ch.chapterId || ch.id))));
    }
  };

  const toggleChapter = (chId: string) => {
    setExpandedChapters((prev) => {
      const next = new Set(prev);
      if (next.has(chId)) next.delete(chId);
      else next.add(chId);
      return next;
    });
  };

  // Filter chapters and lessons
  const filteredChapters = course.chapters
    .map((ch) => {
      const matchingLessons = (ch.lessons ?? []).filter((lesson) => {
        const done = isDone(lesson);
        if (lessonFilter === "completed" && !done) return false;
        if (lessonFilter === "pending" && done) return false;
        if (lessonFilter === "bookmarked" && !lesson.isBookmarked) return false;
        if (lessonFilter === "needsReview" && !lesson.needsReview) return false;

        const q = normalizeVi(searchQuery);
        if (q && !normalizeVi(lesson.title).includes(q)) return false;
        return true;
      });
      return { ...ch, lessons: matchingLessons };
    })
    .filter((ch) => ch.lessons.length > 0 || (!searchQuery && lessonFilter === "all"));

  const learningOutcomes = Array.isArray(course.learningOutcomes) ? course.learningOutcomes : [];
  const prerequisites = Array.isArray(course.prerequisites) ? course.prerequisites : [];
  const targetAudience = course.targetAudience?.trim();

  return (
    <div className="space-y-6">
      {/* 1. Real Course Meta: Learning Outcomes, Prerequisites, Target Audience */}
      {(learningOutcomes.length > 0 || prerequisites.length > 0 || targetAudience) && (
        <Card className="p-5 sm:p-6 bg-[var(--surface-card)] space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
            {/* Learning Outcomes */}
            {learningOutcomes.length > 0 && (
              <div className="space-y-2.5">
                <h3 className="text-xs font-bold uppercase tracking-wider text-[#5E51E8] dark:text-[#A5B4FC] flex items-center gap-1.5">
                  <Target className="w-4 h-4" />
                  <span>Mục tiêu đầu ra ({learningOutcomes.length})</span>
                </h3>
                <ul className="space-y-2 text-xs text-[var(--text-secondary)]">
                  {learningOutcomes.map((item, idx) => (
                    <li key={idx} className="flex items-start gap-2">
                      <CheckCircle2 className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400 shrink-0 mt-0.5" />
                      <span className="leading-relaxed">{item}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* Prerequisites & Target Audience */}
            <div className="space-y-4">
              {prerequisites.length > 0 && (
                <div className="space-y-2">
                  <h3 className="text-xs font-bold uppercase tracking-wider text-amber-600 dark:text-amber-400 flex items-center gap-1.5">
                    <ListChecks className="w-4 h-4" />
                    <span>Điều kiện tiên quyết</span>
                  </h3>
                  <ul className="space-y-1.5 text-xs text-[var(--text-secondary)]">
                    {prerequisites.map((pre, idx) => (
                      <li key={idx} className="flex items-start gap-2">
                        <span className="w-1.5 h-1.5 rounded-full bg-amber-500 shrink-0 mt-1.5" />
                        <span className="leading-relaxed">{pre}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              )}

              {targetAudience && (
                <div className="space-y-1.5 pt-2 border-t border-[var(--border-calm)]">
                  <h4 className="text-xs font-bold uppercase tracking-wider text-[var(--text-tertiary)] flex items-center gap-1.5">
                    <Users className="w-3.5 h-3.5 text-indigo-500" />
                    <span>Đối tượng học viên</span>
                  </h4>
                  <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
                    {targetAudience}
                  </p>
                </div>
              )}
            </div>
          </div>
        </Card>
      )}

      {/* 2. Interactive Syllabus Header & Filters */}
      <div className="space-y-3">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
          <div className="flex items-center gap-2">
            <h2 className="text-base font-semibold text-[var(--text-primary)]">
              Đề cương khóa học
            </h2>
            <Badge variant="neutral" size="sm">
              {course.chapters.length} chương · {course.totalLessons} bài
            </Badge>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <button
              type="button"
              onClick={handleToggleSequentialUnlock}
              data-testid="sequential-unlock-toggle"
              className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-[8px] border text-xs font-medium transition cursor-pointer ${
                sequentialUnlock
                  ? "border-[#5E51E8]/40 bg-[#EEF0FD] dark:bg-[#1E1B4B] text-[#4F46E5] dark:text-[#A5B4FC]"
                  : "border-[var(--border-calm)] bg-[var(--surface-elevated)] text-[var(--text-secondary)] hover:text-[var(--text-primary)]"
              }`}
              title="Khi bật, cần hoàn thành bài trước để mở khóa bài tiếp theo. Có thể tắt bất cứ lúc nào."
            >
              {sequentialUnlock ? (
                <Lock className="w-3.5 h-3.5" />
              ) : (
                <Unlock className="w-3.5 h-3.5" />
              )}
              <span>Mở khóa tuần tự: {sequentialUnlock ? "Bật" : "Tắt"}</span>
            </button>

            {course.chapters.length > 0 && (
              <button
                type="button"
                onClick={toggleAllChapters}
                className="inline-flex items-center gap-1.5 text-xs font-medium text-[var(--text-secondary)] hover:text-[var(--text-primary)] transition cursor-pointer"
              >
                {allExpanded ? (
                  <>
                    <ChevronsDownUp className="w-3.5 h-3.5" />
                    <span>Thu gọn tất cả</span>
                  </>
                ) : (
                  <>
                    <ChevronsUpDown className="w-3.5 h-3.5" />
                    <span>Mở rộng tất cả</span>
                  </>
                )}
              </button>
            )}
          </div>
        </div>

        {/* Search & Quick Filters */}
        {allLessons.length > 0 && (
          <div className="flex flex-col sm:flex-row sm:items-center gap-2.5">
            <div className="relative flex-1">
              <Search className="w-4 h-4 text-[var(--text-tertiary)] absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Tìm bài học trong đề cương..."
                className="w-full h-9 pl-9 pr-8 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8] transition"
              />
              {searchQuery && (
                <button
                  type="button"
                  onClick={() => setSearchQuery("")}
                  className="absolute right-2.5 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer"
                >
                  <X className="w-3.5 h-3.5" />
                </button>
              )}
            </div>

            <div className="flex flex-wrap items-center gap-1.5">
              {(
                [
                  { id: "all", label: "Tất cả" },
                  { id: "pending", label: "Chưa hoàn thành" },
                  { id: "completed", label: "Đã xong" },
                  {
                    id: "bookmarked",
                    label: `Đánh dấu 🔖 (${course.bookmarkedLessonCount ?? 0})`,
                  },
                  {
                    id: "needsReview",
                    label: `Cần ôn 🔁 (${course.needsReviewLessonCount ?? 0})`,
                  },
                ] as { id: LessonFilter; label: string }[]
              ).map((tab) => (
                <button
                  key={tab.id}
                  type="button"
                  onClick={() => setLessonFilter(tab.id)}
                  className={`h-8 px-3 rounded-[8px] text-xs font-medium transition cursor-pointer whitespace-nowrap ${
                    lessonFilter === tab.id
                      ? "bg-[#5E51E8] text-white dark:bg-[#818CF8] dark:text-[#0F0F12]"
                      : "bg-[var(--surface-elevated)] text-[var(--text-secondary)] border border-[var(--border-calm)] hover:text-[var(--text-primary)]"
                  }`}
                >
                  {tab.label}
                </button>
              ))}
              {(lessonFilter !== "all" || searchQuery.trim().length > 0) && (
                <button
                  type="button"
                  onClick={() => {
                    setSearchQuery("");
                    setLessonFilter("all");
                  }}
                  className="inline-flex items-center gap-1 h-8 px-2.5 rounded-[8px] border border-rose-200 dark:border-rose-800/70 bg-rose-50/70 dark:bg-rose-950/30 text-xs font-medium text-rose-700 dark:text-rose-300 hover:bg-rose-100 dark:hover:bg-rose-900/40 transition cursor-pointer whitespace-nowrap"
                >
                  <X className="w-3 h-3" />
                  <span>Xóa bộ lọc</span>
                </button>
              )}
            </div>
          </div>
        )}
      </div>

      {/* 3. Chapters & Lessons Tree */}
      {filteredChapters.length > 0 ? (
        <div className="space-y-3">
          {filteredChapters.map((chapter, chIdx) => {
            const chKey = String(chapter.chapterId || chapter.id);
            const isOpen =
              searchQuery.trim().length > 0 ||
              lessonFilter !== "all" ||
              expandedChapters.has(chKey);
            const lessons = chapter.lessons ?? [];
            const origChapter =
              course.chapters.find((c) => String(c.chapterId || c.id) === chKey) ?? chapter;
            const origLessons = origChapter.lessons ?? [];
            const doneInChapter = origLessons.filter((l) => isDone(l)).length;
            const chapterProgressPct =
              origLessons.length > 0
                ? Math.round((doneInChapter * 100) / origLessons.length)
                : 0;
            const cleanChapterTitle = stripChapterPrefix(chapter.title) || chapter.title;

            return (
              <Card
                key={chKey}
                id={`chapter-card-${chKey}`}
                className="overflow-hidden"
              >
                <button
                  type="button"
                  onClick={() => toggleChapter(chKey)}
                  className="w-full px-4 py-3 bg-[var(--surface-elevated)] hover:bg-[var(--surface-elevated)]/80 flex items-center justify-between gap-3 text-left transition cursor-pointer"
                >
                  <div className="flex items-start gap-2 min-w-0">
                    {isOpen ? (
                      <ChevronDown className="w-4 h-4 text-[var(--text-tertiary)] shrink-0 mt-0.5" />
                    ) : (
                      <ChevronRight className="w-4 h-4 text-[var(--text-tertiary)] shrink-0 mt-0.5" />
                    )}
                    <h3 className="text-sm font-semibold text-[var(--text-primary)] leading-snug">
                      Chương {chIdx + 1}: {cleanChapterTitle}
                    </h3>
                  </div>
                  <div className="flex items-center gap-2.5 shrink-0">
                    <div
                      className="h-1.5 w-16 sm:w-20 rounded-full bg-slate-200 dark:bg-slate-700 overflow-hidden"
                      aria-hidden="true"
                    >
                      <div
                        className={`h-full rounded-full transition-all duration-300 ${
                          chapterProgressPct === 100 ? "bg-emerald-500" : "bg-[#5E51E8] dark:bg-[#818CF8]"
                        }`}
                        style={{ width: `${Math.min(100, Math.max(0, chapterProgressPct))}%` }}
                      />
                    </div>
                    <Badge variant="neutral" size="sm">
                      {doneInChapter}/{origLessons.length} bài
                    </Badge>
                  </div>
                </button>

                {isOpen && lessons.length > 0 && (
                  <div className="p-4 sm:p-5 border-t border-[var(--border-calm)] bg-[var(--surface-card)]">
                    <div className="relative pl-6 sm:pl-8 space-y-4 before:absolute before:left-3 sm:before:left-3.5 before:top-3 before:bottom-3 before:w-0.5 before:bg-slate-200 dark:before:bg-slate-800">
                      {lessons.map((lesson, lesIdx) => {
                        const completed = isDone(lesson);
                        const isNext = lesson.id === nextLesson?.id && !completed;
                        const isLocked =
                          sequentialUnlock && !completed && !unlockedLessonIds.has(lesson.id);
                        const mins = lesson.durationMinutes ?? lesson.estimatedMinutes ?? 15;

                        return (
                          <div
                            key={lesson.id}
                            className={`relative group flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-3.5 rounded-xl border transition shadow-2xs ${
                              isLocked
                                ? "border-[var(--border-calm)] bg-[var(--surface-elevated)]/30 opacity-80"
                                : "border-[var(--border-calm)] hover:border-[#5E51E8]/50 bg-[var(--surface-elevated)]/50 hover:bg-[var(--surface-elevated)]"
                            }`}
                          >
                            {/* Timeline Node Icon */}
                            <div
                              className={`absolute -left-6 sm:-left-8 top-4 -translate-x-1/2 w-6 h-6 rounded-full border-2 flex items-center justify-center text-[10px] font-bold shadow-xs transition ${
                                completed
                                  ? "bg-emerald-500 border-emerald-500 text-white"
                                  : isNext
                                  ? "bg-[#5E51E8] border-[#5E51E8] text-white ring-4 ring-[#5E51E8]/20 animate-pulse"
                                  : isLocked
                                  ? "bg-slate-200 dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-500"
                                  : "bg-[var(--surface-card)] border-[var(--border-calm)] text-[var(--text-tertiary)]"
                              }`}
                            >
                              {completed ? (
                                <CheckCircle2 className="w-3.5 h-3.5" />
                              ) : isNext ? (
                                <Play className="w-2.5 h-2.5 fill-current ml-0.5" />
                              ) : isLocked ? (
                                <Lock className="w-3 h-3" />
                              ) : (
                                <span>{lesIdx + 1}</span>
                              )}
                            </div>

                            {/* Lesson Info & Badges */}
                            <div className="min-w-0 flex-1">
                              <div className="flex flex-wrap items-center gap-1.5 mb-1">
                                <span
                                  className={`text-[10px] font-bold px-2 py-0.5 rounded-full border uppercase tracking-wider ${
                                    completed
                                      ? "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800"
                                      : isNext
                                      ? "bg-indigo-50 text-indigo-700 border-indigo-200 dark:bg-indigo-950/60 dark:text-indigo-300 dark:border-indigo-800"
                                      : isLocked
                                      ? "bg-slate-100 text-slate-600 border-slate-200 dark:bg-slate-800 dark:text-slate-400 dark:border-slate-700"
                                      : "bg-slate-100 text-slate-600 border-slate-200 dark:bg-slate-800 dark:text-slate-400 dark:border-slate-700"
                                  }`}
                                >
                                  {completed
                                    ? "Đã xong"
                                    : isNext
                                    ? "Tiếp theo"
                                    : isLocked
                                    ? "Khóa tuần tự"
                                    : `Bài ${lesIdx + 1}`}
                                </span>

                                {mins && (
                                  <span className="text-xs text-[var(--text-tertiary)] flex items-center gap-1">
                                    <Clock className="w-3 h-3" />
                                    {mins} phút
                                  </span>
                                )}

                                {/* Bookmarked Badge */}
                                {lesson.isBookmarked && (
                                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold bg-amber-50 text-amber-700 border border-amber-200 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-800">
                                    <Bookmark className="w-2.5 h-2.5 fill-current" />
                                    <span>Đã đánh dấu</span>
                                  </span>
                                )}

                                {/* Needs Review Badge */}
                                {lesson.needsReview && (
                                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold bg-rose-50 text-rose-700 border border-rose-200 dark:bg-rose-950/60 dark:text-rose-300 dark:border-rose-800">
                                    <RotateCcw className="w-2.5 h-2.5" />
                                    <span>Cần ôn lại</span>
                                  </span>
                                )}

                                {/* Practice Lab Badge */}
                                {(lesson.practiceActivityCount ?? 0) > 0 && (
                                  <span
                                    className={`text-[10px] font-semibold px-2 py-0.5 rounded-full border inline-flex items-center gap-1 ${
                                      (lesson.practicePassedCount ?? 0) >= (lesson.practiceActivityCount ?? 0)
                                        ? "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800"
                                        : "bg-indigo-50 text-indigo-700 border-indigo-200 dark:bg-indigo-950/60 dark:text-indigo-300 dark:border-indigo-800"
                                    }`}
                                  >
                                    <FlaskConical className="w-2.5 h-2.5" />
                                    {lesson.practicePassedCount ?? 0}/{lesson.practiceActivityCount} Lab
                                  </span>
                                )}
                              </div>

                              <Link
                                href={`/app/courses/${courseId}/learn?lessonId=${lesson.id}`}
                                title={lesson.title}
                                className="block text-sm font-semibold text-[var(--text-primary)] hover:text-[#5E51E8] dark:hover:text-[#818CF8] transition line-clamp-2 leading-snug break-words"
                              >
                                {lesson.title}
                              </Link>

                              {isLocked && (
                                <p className="text-[11px] text-amber-700 dark:text-amber-400 mt-1 flex items-center gap-1">
                                  <Lock className="w-3 h-3 shrink-0" />
                                  <span>
                                    Hoàn thành bài trước đó hoặc tắt chế độ tuần tự để mở ngay bài này.
                                  </span>
                                </p>
                              )}
                            </div>

                            {/* Action CTA Buttons */}
                            <div className="shrink-0 self-start sm:self-center pt-1 sm:pt-0 flex items-center gap-1.5">
                              {!isLocked && (
                                <>
                                  {(lesson.practiceActivityCount ?? 0) > 0 && (
                                    <Link
                                      href={`/app/practice/${courseId}/${lesson.id}`}
                                      title="Thực hành Lab cho bài học này"
                                      className="inline-flex items-center gap-1 h-8 px-2 sm:px-2.5 rounded-lg border border-indigo-200 dark:border-indigo-800 bg-indigo-50/70 dark:bg-indigo-950/40 text-xs font-semibold text-indigo-700 dark:text-indigo-300 hover:bg-indigo-100 dark:hover:bg-indigo-900 transition"
                                    >
                                      <FlaskConical className="w-3.5 h-3.5" />
                                      <span>Lab</span>
                                    </Link>
                                  )}
                                  {lesson.quizId && (
                                    <Link
                                      href={`/app/courses/${courseId}/quiz?lessonId=${lesson.id}`}
                                      title="Làm bài kiểm tra cho bài học này"
                                      className="inline-flex items-center gap-1 h-8 px-2 sm:px-2.5 rounded-lg border border-emerald-200 dark:border-emerald-800 bg-emerald-50/70 dark:bg-emerald-950/40 text-xs font-semibold text-emerald-700 dark:text-emerald-300 hover:bg-emerald-100 dark:hover:bg-emerald-900 transition"
                                    >
                                      <ClipboardCheck className="w-3.5 h-3.5" />
                                      <span>Quiz</span>
                                    </Link>
                                  )}
                                </>
                              )}

                              {isLocked ? (
                                <Button
                                  variant="outline"
                                  size="sm"
                                  onClick={handleToggleSequentialUnlock}
                                  className="whitespace-nowrap"
                                  leftIcon={<Unlock className="w-3.5 h-3.5" />}
                                >
                                  <span>Mở khóa</span>
                                </Button>
                              ) : (
                                <Link href={`/app/courses/${courseId}/learn?lessonId=${lesson.id}`}>
                                  <Button
                                    variant={isNext ? "primary" : completed ? "secondary" : "outline"}
                                    size="sm"
                                    className="whitespace-nowrap"
                                    leftIcon={
                                      completed ? (
                                        <Check className="w-3.5 h-3.5" />
                                      ) : (
                                        <Play className="w-3.5 h-3.5 fill-current" />
                                      )
                                    }
                                  >
                                    <span>{completed ? "Ôn lại" : isNext ? "Học ngay" : "Bắt đầu"}</span>
                                  </Button>
                                </Link>
                              )}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                )}
              </Card>
            );
          })}
        </div>
      ) : (
        <EmptyState
          icon={BookOpen}
          title={
            allLessons.length > 0
              ? "Không có bài giảng nào khớp bộ lọc hiện tại."
              : "Khóa học chưa có nội dung bài giảng."
          }
          action={
            (searchQuery || lessonFilter !== "all")
              ? {
                  label: "Xóa bộ lọc",
                  onClick: () => {
                    setSearchQuery("");
                    setLessonFilter("all");
                  },
                }
              : undefined
          }
        />
      )}

      {/* 4. Course Reviews Section */}
      <Card className="p-5 sm:p-6 bg-[var(--surface-card)] space-y-5">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-[var(--border-calm)] pb-4">
          <div>
            <h3 className="text-base font-bold text-[var(--text-primary)] flex items-center gap-2">
              <MessageSquare className="w-4 h-4 text-[#5E51E8] dark:text-[#A5B4FC]" />
              <span>Đánh giá &amp; Nhận xét khóa học</span>
            </h3>
            <p className="text-xs text-[var(--text-secondary)] mt-0.5">
              Chia sẻ trải nghiệm học tập của bạn về lộ trình và chất lượng bài giảng.
            </p>
          </div>
          <div className="flex items-center gap-2.5 px-3.5 py-2 rounded-xl bg-[var(--surface-elevated)] border border-[var(--border-calm)]">
            <div className="flex items-center gap-0.5">
              {[1, 2, 3, 4, 5].map((star) => (
                <Star
                  key={star}
                  className={`w-4 h-4 ${
                    star <= Math.round(averageRating || 5)
                      ? "fill-amber-400 text-amber-400"
                      : "text-slate-300 dark:text-slate-700"
                  }`}
                />
              ))}
            </div>
            <span className="text-sm font-bold text-[var(--text-primary)]">
              {averageRating > 0 ? `${averageRating}/5` : "Mới"}
            </span>
            <span className="text-xs text-[var(--text-tertiary)]">
              ({reviews.length} đánh giá)
            </span>
          </div>
        </div>

        {/* Submit Review Form */}
        <form onSubmit={handleSubmitReview} className="space-y-3">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-xs font-semibold text-[var(--text-secondary)]">
              Chọn mức điểm:
            </span>
            <div className="flex items-center gap-1">
              {[1, 2, 3, 4, 5].map((star) => (
                <button
                  key={star}
                  type="button"
                  onClick={() => setRatingInput(star)}
                  aria-label={`Đánh giá ${star} sao`}
                  className="p-1 rounded hover:bg-[var(--surface-elevated)] transition cursor-pointer"
                >
                  <Star
                    className={`w-5 h-5 ${
                      star <= ratingInput
                        ? "fill-amber-400 text-amber-400"
                        : "text-slate-300 dark:text-slate-700"
                    }`}
                  />
                </button>
              ))}
            </div>
            <span className="text-xs font-medium text-amber-600 dark:text-amber-400">
              {ratingInput} sao
            </span>
          </div>

          <div className="flex flex-col sm:flex-row gap-2.5">
            <input
              type="text"
              value={commentInput}
              onChange={(e) => setCommentInput(e.target.value)}
              placeholder="Viết nhận xét của bạn về khóa học này..."
              className="flex-1 h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8]"
            />
            <Button
              type="submit"
              variant="primary"
              size="sm"
              disabled={submittingReview}
              leftIcon={<Send className="w-3.5 h-3.5" />}
            >
              <span>{submittingReview ? "Đang gửi..." : "Gửi đánh giá"}</span>
            </Button>
          </div>
          {reviewNotice && (
            <p className="text-xs text-[#4F46E5] dark:text-[#A5B4FC] font-medium">
              {reviewNotice}
            </p>
          )}
        </form>

        {/* Reviews List */}
        {reviews.length > 0 ? (
          <div className="divide-y divide-[var(--border-calm)]">
            {reviews.map((rev) => (
              <div key={rev.id} className="py-3 first:pt-1 last:pb-0 space-y-1">
                <div className="flex items-center justify-between gap-2">
                  <span className="text-xs font-bold text-[var(--text-primary)]">
                    {rev.userName}
                  </span>
                  <div className="flex items-center gap-0.5">
                    {[1, 2, 3, 4, 5].map((s) => (
                      <Star
                        key={s}
                        className={`w-3.5 h-3.5 ${
                          s <= rev.rating
                            ? "fill-amber-400 text-amber-400"
                            : "text-slate-300 dark:text-slate-700"
                        }`}
                      />
                    ))}
                  </div>
                </div>
                <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
                  {rev.comment}
                </p>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-xs text-[var(--text-tertiary)] italic">
            Chưa có nhận xét nào. Hãy là học viên đầu tiên đánh giá khóa học này!
          </p>
        )}
      </Card>
    </div>
  );
}
