"use client";

import React from "react";
import Link from "next/link";
import {
  ChevronLeft,
  ChevronRight,
  CheckCircle2,
  ClipboardCheck,
  Loader2,
  Clock,
  BookOpen,
  FlaskConical,
  Zap,
} from "lucide-react";

export interface LearningStickyBottomBarProps {
  prevLessonId?: string | null;
  prevLessonTitle?: string | null;
  onSelectPrevLesson?: () => void;

  nextLessonId?: string | null;
  nextLessonTitle?: string | null;
  onSelectNextLesson?: () => void;

  scrollPercent: number;
  timeSpentFormatted: string;

  isCompleting: boolean;
  isCompleted?: boolean;
  onMarkComplete: () => void;

  practiceHref?: string | null;
  quizHref?: string | null;
  onOpenQuickCheck?: () => void;
}

export function LearningStickyBottomBar({
  prevLessonId,
  prevLessonTitle,
  onSelectPrevLesson,
  nextLessonId,
  nextLessonTitle,
  onSelectNextLesson,
  scrollPercent,
  timeSpentFormatted,
  isCompleting,
  isCompleted = false,
  onMarkComplete,
  practiceHref,
  quizHref,
  onOpenQuickCheck,
}: LearningStickyBottomBarProps) {
  const clampedScrollPercent = Math.min(100, Math.max(0, scrollPercent));

  return (
    <div
      data-testid="learning-sticky-bottom-bar"
      className="sticky bottom-0 z-30 w-full max-w-full overflow-hidden border-t border-border bg-bg-primary/95 backdrop-blur-md shadow-md"
    >
      {/* Top Hairline Reading Progress (visible on both mobile & desktop) */}
      <div
        aria-hidden="true"
        className="h-[2px] w-full bg-[var(--border-subtle)] overflow-hidden"
      >
        <div
          className="h-full bg-accent transition-all duration-150"
          style={{ width: `${clampedScrollPercent}%` }}
        />
      </div>

      <div className="w-full min-w-0 px-2 sm:px-4 py-2 flex items-center justify-between gap-1 sm:gap-2 text-xs">
        {/* Left: Previous Lesson Button */}
        <div className="flex items-center shrink-0">
          {prevLessonId ? (
            <button
              type="button"
              onClick={onSelectPrevLesson}
              title={prevLessonTitle ? `Bài trước: ${prevLessonTitle}` : "Quay lại bài trước"}
              className="inline-flex items-center gap-1 h-8 px-2 sm:px-3 rounded-lg border border-border bg-bg-secondary text-text-primary hover:bg-bg-tertiary transition cursor-pointer font-medium whitespace-nowrap"
            >
              <ChevronLeft className="w-4 h-4 shrink-0" />
              <span className="hidden sm:inline">Bài trước</span>
            </button>
          ) : (
            <div className="w-2 sm:w-8" />
          )}
        </div>

        {/* Center: Reading progress & Real study time */}
        <div className="hidden sm:inline-flex items-center gap-1 sm:gap-2 px-2 py-1 rounded-lg bg-bg-secondary border border-border text-[11px] sm:text-xs font-medium text-text-secondary whitespace-nowrap min-w-0 shrink-0">
          <span className="inline-flex items-center gap-1 text-accent font-semibold">
            <BookOpen className="w-3.5 h-3.5 shrink-0" />
            <span>{clampedScrollPercent}%</span>
            <span className="hidden md:inline text-text-tertiary">đã đọc</span>
          </span>
          <span className="text-text-tertiary">·</span>
          <span className="inline-flex items-center gap-1 text-text-primary font-mono">
            <Clock className="w-3.5 h-3.5 shrink-0 text-text-tertiary" />
            <span>{timeSpentFormatted}</span>
          </span>
        </div>

        {/* Right Controls: Practice shortcut & Mark Complete & Next Lesson */}
        <div className="flex items-center gap-1 sm:gap-2 shrink-0 ml-auto min-w-0">
          {/* Practice shortcut */}
          {practiceHref ? (
            <Link
              href={practiceHref}
              title="Mở phòng thực hành cho bài học này"
              className="inline-flex items-center gap-1 h-8 px-2 sm:px-2.5 rounded-lg border border-indigo-200 dark:border-indigo-800 bg-indigo-50/70 dark:bg-indigo-950/40 text-[11px] sm:text-xs font-semibold text-indigo-700 dark:text-indigo-300 hover:bg-indigo-100 dark:hover:bg-indigo-900 transition shrink-0 whitespace-nowrap"
            >
              <FlaskConical className="w-3.5 h-3.5 shrink-0" />
              <span>Lab</span>
            </Link>
          ) : onOpenQuickCheck ? (
            <button
              type="button"
              onClick={onOpenQuickCheck}
              title="Cuộn tới câu hỏi kiểm tra nhanh"
              className="inline-flex items-center gap-1 h-8 px-2 sm:px-2.5 rounded-lg border border-indigo-200 dark:border-indigo-800 bg-indigo-50/70 dark:bg-indigo-950/40 text-[11px] sm:text-xs font-semibold text-indigo-700 dark:text-indigo-300 hover:bg-indigo-100 dark:hover:bg-indigo-900 transition shrink-0 cursor-pointer whitespace-nowrap"
            >
              <Zap className="w-3.5 h-3.5 shrink-0" />
              <span className="hidden sm:inline">Kiểm tra</span>
              <span className="sm:hidden">Đề</span>
            </button>
          ) : null}

          {/* Quiz shortcut */}
          {quizHref && (
            <Link
              href={quizHref}
              data-testid="sticky-quiz-link"
              title="Làm bài kiểm tra / giải đề cho bài học này"
              className="inline-flex items-center gap-1 h-8 px-2 sm:px-2.5 rounded-lg border border-emerald-200 dark:border-emerald-800 bg-emerald-50/70 dark:bg-emerald-950/40 text-[11px] sm:text-xs font-semibold text-emerald-700 dark:text-emerald-300 hover:bg-emerald-100 dark:hover:bg-emerald-900 transition shrink-0 whitespace-nowrap"
            >
              <ClipboardCheck className="w-3.5 h-3.5 shrink-0" />
              <span className="hidden sm:inline">Giải đề</span>
              <span className="sm:hidden">Đề</span>
            </Link>
          )}

          {/* Main Complete button */}
          <button
            type="button"
            disabled={isCompleting}
            onClick={onMarkComplete}
            data-testid="sticky-mark-lesson-complete-btn"
            className="inline-flex items-center gap-1 sm:gap-1.5 h-8 px-2 sm:px-3 rounded-lg bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:hover:bg-[#A5B4FC] dark:text-[#0F0F12] text-[11px] sm:text-xs font-semibold disabled:opacity-60 transition cursor-pointer shadow-xs shrink-0 whitespace-nowrap"
          >
            {isCompleting ? (
              <Loader2 className="w-3.5 h-3.5 animate-spin shrink-0" />
            ) : (
              <CheckCircle2 className="w-3.5 h-3.5 shrink-0" />
            )}
            <span>
              {nextLessonId ? (
                isCompleted ? (
                  <>
                    <span className="sm:hidden">Tiếp →</span>
                    <span className="hidden sm:inline">Bài tiếp →</span>
                  </>
                ) : (
                  <>
                    <span className="sm:hidden">Xong & Tiếp →</span>
                    <span className="hidden sm:inline">Hoàn thành & Bài tiếp →</span>
                  </>
                )
              ) : isCompleted ? (
                <>
                  <span className="sm:hidden">Xong</span>
                  <span className="hidden sm:inline">Đã xong</span>
                </>
              ) : (
                <>
                  <span className="sm:hidden">Xong</span>
                  <span className="hidden sm:inline">Đánh dấu hoàn thành</span>
                </>
              )}
            </span>
          </button>

          {/* Next Lesson arrow button */}
          {nextLessonId && (
            <button
              type="button"
              onClick={onSelectNextLesson}
              title={nextLessonTitle ? `Bài tiếp: ${nextLessonTitle}` : "Chuyển sang bài sau"}
              className="inline-flex items-center gap-1 h-8 px-2 sm:px-3 rounded-lg border border-border bg-bg-secondary text-text-primary hover:bg-bg-tertiary transition cursor-pointer font-medium shrink-0 whitespace-nowrap"
            >
              <span className="hidden sm:inline">Bài sau</span>
              <ChevronRight className="w-4 h-4 shrink-0" />
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
