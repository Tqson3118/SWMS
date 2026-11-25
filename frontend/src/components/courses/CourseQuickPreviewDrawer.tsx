"use client";

import React, { useEffect, useRef, useState } from "react";
import Link from "next/link";
import {
  X,
  BookOpen,
  Layers,
  Clock,
  Sparkles,
  Brain,
  CheckCircle2,
  Copy,
  ExternalLink,
  Play,
  Pin,
  Archive,
  AlertCircle,
  ChevronDown,
  ChevronUp,
  Bookmark,
  Check,
  RefreshCw,
  Target,
  FileQuestion,
  UserCheck,
} from "lucide-react";
import {
  type CourseSummary,
  type CourseSummaryChapter,
  normalizeCourseSummary,
  getCourseTotalMinutes,
  stripChapterPrefix,
} from "@/lib/course-helpers";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";

interface CourseQuickPreviewDrawerProps {
  course: CourseSummary | null;
  isOpen: boolean;
  onClose: () => void;
  onCloneSuccess?: (clonedCourse: CourseSummary) => void;
}

export function CourseQuickPreviewDrawer({
  course: initialCourse,
  isOpen,
  onClose,
  onCloneSuccess,
}: CourseQuickPreviewDrawerProps) {
  const [course, setCourse] = useState<CourseSummary | null>(initialCourse);
  const [isLoadingDetails, setIsLoadingDetails] = useState(false);
  const [expandedChapterIds, setExpandedChapterIds] = useState<Record<string, boolean>>({});
  const [isCloning, setIsCloning] = useState(false);
  const [cloneError, setCloneError] = useState<string | null>(null);
  const [cloneSuccessMsg, setCloneSuccessMsg] = useState<string | null>(null);

  const drawerPanelRef = useRef<HTMLDivElement | null>(null);
  const closeButtonRef = useRef<HTMLButtonElement | null>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;

  // Sync state when initialCourse changes
  useEffect(() => {
    if (initialCourse) {
      setCourse(normalizeCourseSummary(initialCourse));
      setCloneError(null);
      setCloneSuccessMsg(null);
    }
  }, [initialCourse]);

  // Fetch full details (chapters & lessons) if needed
  useEffect(() => {
    if (!isOpen || !initialCourse?.id) return;

    const courseId = initialCourse.id;
    const hasFullChapters =
      Array.isArray(initialCourse.chapters) &&
      initialCourse.chapters.length > 0 &&
      initialCourse.chapters.some((ch) => Array.isArray(ch.lessons) && ch.lessons.length > 0);

    if (hasFullChapters) {
      // Initialize accordion: open first 2 chapters
      const initOpen: Record<string, boolean> = {};
      initialCourse.chapters?.forEach((ch, idx) => {
        initOpen[ch.id] = idx < 2;
      });
      setExpandedChapterIds(initOpen);
      return;
    }

    let isMounted = true;
    setIsLoadingDetails(true);

    const headers = getClientRequestHeaders();
    fetch(`/api/v1/courses/${courseId}`, { headers })
      .then(async (resp) => {
        if (!resp.ok) {
          throw new Error(`HTTP ${resp.status}`);
        }
        return resp.json();
      })
      .then((data) => {
        if (!isMounted || !data) return;
        const normalized = normalizeCourseSummary(data);
        setCourse(normalized);
        const initOpen: Record<string, boolean> = {};
        normalized.chapters?.forEach((ch, idx) => {
          initOpen[ch.id] = idx < 2;
        });
        setExpandedChapterIds(initOpen);
      })
      .catch(() => {
        // Fallback: keep existing data
      })
      .finally(() => {
        if (isMounted) setIsLoadingDetails(false);
      });

    return () => {
      isMounted = false;
    };
  }, [isOpen, initialCourse?.id]);

  // Body scroll lock, focus trap & ESC key handling
  useEffect(() => {
    if (!isOpen) return;

    previousFocusRef.current = document.activeElement as HTMLElement | null;
    const prevOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    const focusTimer = window.setTimeout(() => {
      closeButtonRef.current?.focus();
    }, 0);

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onCloseRef.current();
        return;
      }

      if (e.key === "Tab" && drawerPanelRef.current) {
        const focusable = Array.from(
          drawerPanelRef.current.querySelectorAll<HTMLElement>(
            'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])'
          )
        ).filter((el) => el.offsetParent !== null || el.getClientRects().length > 0);

        if (focusable.length === 0) {
          e.preventDefault();
          return;
        }

        const firstEl = focusable[0];
        const lastEl = focusable[focusable.length - 1];
        const activeEl = document.activeElement as HTMLElement | null;

        if (e.shiftKey) {
          if (activeEl === firstEl || !drawerPanelRef.current.contains(activeEl)) {
            e.preventDefault();
            lastEl.focus();
          }
        } else {
          if (activeEl === lastEl || !drawerPanelRef.current.contains(activeEl)) {
            e.preventDefault();
            firstEl.focus();
          }
        }
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => {
      window.clearTimeout(focusTimer);
      window.removeEventListener("keydown", handleKeyDown);
      document.body.style.overflow = prevOverflow;
      previousFocusRef.current?.focus?.();
    };
  }, [isOpen]);

  if (!isOpen || !course) return null;

  const toggleChapter = (chapterId: string) => {
    setExpandedChapterIds((prev) => ({
      ...prev,
      [chapterId]: !prev[chapterId],
    }));
  };

  const handleClone = async () => {
    if (!course.id || isCloning) return;
    setIsCloning(true);
    setCloneError(null);
    setCloneSuccessMsg(null);

    try {
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const resp = await fetch(`/api/v1/courses/${course.id}/clone`, {
        method: "POST",
        headers,
      });

      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        throw new Error(errText || `Nhân bản thất bại (HTTP ${resp.status}).`);
      }

      const cloned = await resp.json();
      invalidateDashboardCache();
      const normCloned = normalizeCourseSummary(cloned);
      setCloneSuccessMsg(`Đã nhân bản "${normCloned.title}" vào kho cá nhân.`);

      if (onCloneSuccess) {
        onCloneSuccess(normCloned);
      }
    } catch (err) {
      setCloneError(err instanceof Error ? err.message : "Không thể nhân bản khóa học.");
    } finally {
      setIsCloning(false);
    }
  };

  const totalMinutes = getCourseTotalMinutes(course);
  const hoursLabel =
    totalMinutes < 60 ? `${totalMinutes} phút` : `~${Math.round(totalMinutes / 60)} giờ học`;

  const progressPercent = Number(course.progressPercent ?? 0);
  const completedLessons = Number(course.completedLessons ?? 0);
  const totalLessons = Number(course.lessonCount ?? 0);
  const hasProgress = progressPercent > 0 || completedLessons > 0;

  const chapters: CourseSummaryChapter[] = Array.isArray(course.chapters) ? course.chapters : [];

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="preview-drawer-title"
      className="fixed inset-0 z-50 overflow-hidden"
    >
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/60 backdrop-blur-xs transition-opacity animate-in fade-in duration-200"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Slide-in Panel from right */}
      <div className="fixed inset-y-0 right-0 w-full sm:max-w-xl flex">
        <div
          ref={drawerPanelRef}
          className="w-full bg-[var(--surface-card)] border-l border-[var(--border-calm)] shadow-2xl flex flex-col justify-between overflow-hidden animate-in slide-in-from-right duration-250"
        >
          {/* Header */}
          <div className="px-6 py-4 border-b border-[var(--border-calm)] flex items-start justify-between gap-4 bg-[var(--surface-elevated)]/50">
            <div className="space-y-1.5 min-w-0 flex-1">
              <div className="flex flex-wrap items-center gap-1.5">
                <span className="inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-semibold bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:text-[#A5B4FC] border border-[#5E51E8]/20">
                  {course.category || "Tổng hợp"}
                </span>

                <span className="inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-semibold bg-[var(--surface-elevated)] text-[var(--text-secondary)] border border-[var(--border-calm)]">
                  {course.level === "advanced"
                    ? "Chuyên sâu"
                    : course.level === "intermediate"
                    ? "Trung cấp"
                    : "Nền tảng"}
                </span>

                {course.isSystemCourse ? (
                  <span className="inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800">
                    Hệ thống
                  </span>
                ) : (
                  <span className="inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-semibold bg-purple-50 text-purple-700 border border-purple-200 dark:bg-purple-950/40 dark:text-purple-300 dark:border-purple-800">
                    Cá nhân
                  </span>
                )}

                {course.isPinned && (
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[11px] font-semibold bg-amber-50 text-amber-700 border border-amber-200 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800">
                    <Pin className="w-3 h-3 fill-current" />
                    Đã ghim
                  </span>
                )}

                {course.isArchived && (
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[11px] font-semibold bg-gray-100 text-gray-700 border border-gray-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700">
                    <Archive className="w-3 h-3" />
                    Đã lưu trữ
                  </span>
                )}
              </div>

              <h2
                id="preview-drawer-title"
                className="text-lg font-bold text-[var(--text-primary)] leading-snug break-words"
              >
                {course.title}
              </h2>
            </div>

            <button
              ref={closeButtonRef}
              type="button"
              onClick={onClose}
              className="p-1.5 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer shrink-0"
              aria-label="Đóng bảng xem nhanh"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Drawer Body - Scrollable */}
          <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-6">
            {/* Compact Progress Bar when learner has started */}
            {hasProgress && (
              <div className="rounded-xl border border-[#5E51E8]/20 bg-[#EEF0FD]/50 dark:bg-[#1E1B4B]/30 p-3.5 space-y-2">
                <div className="flex items-center justify-between gap-2 text-xs font-semibold">
                  <span className="text-[var(--text-primary)]">
                    Tiến độ: {completedLessons}/{totalLessons} bài ({progressPercent}%)
                  </span>
                  <span className="text-[11px] font-bold text-[#5E51E8] dark:text-[#A5B4FC]">
                    {progressPercent >= 100 ? "Hoàn thành" : "Đang học"}
                  </span>
                </div>
                <div className="h-2 w-full rounded-full bg-[var(--surface-elevated)] overflow-hidden border border-[var(--border-calm)]">
                  <div
                    className={`h-full rounded-full transition-all duration-300 ${
                      progressPercent >= 100 ? "bg-emerald-500" : "bg-[#5E51E8]"
                    }`}
                    style={{ width: `${Math.min(100, Math.max(0, progressPercent))}%` }}
                  />
                </div>
              </div>
            )}
            {/* Clone alerts */}
            {cloneSuccessMsg && (
              <div className="rounded-lg border border-emerald-500/30 bg-emerald-50/50 dark:bg-emerald-950/20 px-3.5 py-2.5 flex items-center gap-2 text-xs font-medium text-emerald-700 dark:text-emerald-300">
                <Check className="w-4 h-4 shrink-0 text-emerald-600" />
                <span>{cloneSuccessMsg}</span>
              </div>
            )}
            {cloneError && (
              <div className="rounded-lg border border-red-500/30 bg-red-50/50 dark:bg-red-950/20 px-3.5 py-2.5 flex items-center gap-2 text-xs font-medium text-red-600 dark:text-red-400">
                <AlertCircle className="w-4 h-4 shrink-0" />
                <span>{cloneError}</span>
              </div>
            )}

            {/* Description & Target Audience */}
            <div className="space-y-2">
              <p className="text-sm text-[var(--text-secondary)] leading-relaxed">
                {course.description || "Khóa học có hệ thống bài giảng tương tác, phòng thực hành và thẻ nhớ FSRS."}
              </p>

              {course.targetAudience && (
                <div className="flex items-center gap-2 text-xs text-[var(--text-tertiary)]">
                  <UserCheck className="w-3.5 h-3.5 text-[#5E51E8] shrink-0" />
                  <span>
                    <strong className="text-[var(--text-secondary)] font-medium">Đối tượng: </strong>
                    {course.targetAudience}
                  </span>
                </div>
              )}
            </div>

            {/* Metric Tiles Grid */}
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-2.5">
              <div className="p-3 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60">
                <div className="flex items-center gap-1.5 text-xs text-[var(--text-tertiary)] mb-1">
                  <Layers className="w-3.5 h-3.5 text-[#5E51E8]" />
                  <span>Chương mục</span>
                </div>
                <div className="text-base font-bold text-[var(--text-primary)]">
                  {course.chapterCount ?? 0} chương
                </div>
              </div>

              <div className="p-3 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60">
                <div className="flex items-center gap-1.5 text-xs text-[var(--text-tertiary)] mb-1">
                  <BookOpen className="w-3.5 h-3.5 text-[#5E51E8]" />
                  <span>Bài học</span>
                </div>
                <div className="text-base font-bold text-[var(--text-primary)]">
                  {course.lessonCount ?? 0} bài
                </div>
              </div>

              <div className="p-3 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60">
                <div className="flex items-center gap-1.5 text-xs text-[var(--text-tertiary)] mb-1">
                  <Clock className="w-3.5 h-3.5 text-[#5E51E8]" />
                  <span>Thời lượng</span>
                </div>
                <div className="text-base font-bold text-[var(--text-primary)]">
                  {hoursLabel}
                </div>
              </div>

              <div className="p-3 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60">
                <div className="flex items-center gap-1.5 text-xs text-[var(--text-tertiary)] mb-1">
                  <Sparkles className="w-3.5 h-3.5 text-purple-500" />
                  <span>Phòng Lab</span>
                </div>
                <div className="text-base font-bold text-[var(--text-primary)] truncate">
                  {course.practiceTotalActivities ?? 0} bài tập
                </div>
              </div>

              <div className="p-3 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60">
                <div className="flex items-center gap-1.5 text-xs text-[var(--text-tertiary)] mb-1">
                  <FileQuestion className="w-3.5 h-3.5 text-amber-500" />
                  <span>Quiz test</span>
                </div>
                <div className="text-base font-bold text-[var(--text-primary)]">
                  {course.quizCount ?? 0} bộ đề
                </div>
              </div>

              <div className="p-3 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60">
                <div className="flex items-center gap-1.5 text-xs text-[var(--text-tertiary)] mb-1">
                  <Brain className="w-3.5 h-3.5 text-emerald-500" />
                  <span>Thẻ FSRS</span>
                </div>
                <div className="text-base font-bold text-[var(--text-primary)]">
                  {course.flashcardCount ?? 0} thẻ
                </div>
              </div>
            </div>

            {/* Learning Outcomes */}
            {Array.isArray(course.learningOutcomes) && course.learningOutcomes.length > 0 && (
              <div className="space-y-2">
                <h3 className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] flex items-center gap-1.5">
                  <Target className="w-3.5 h-3.5 text-[#5E51E8]" />
                  <span>Mục tiêu đầu ra</span>
                </h3>
                <ul className="space-y-1.5">
                  {course.learningOutcomes.map((outcome, idx) => (
                    <li
                      key={idx}
                      className="text-xs text-[var(--text-secondary)] flex items-start gap-2 leading-relaxed"
                    >
                      <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500 shrink-0 mt-0.5" />
                      <span>{outcome}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* Prerequisites */}
            {Array.isArray(course.prerequisites) && course.prerequisites.length > 0 && (
              <div className="space-y-2">
                <h3 className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] flex items-center gap-1.5">
                  <CheckCircle2 className="w-3.5 h-3.5 text-amber-500" />
                  <span>Điều kiện tiên quyết</span>
                </h3>
                <ul className="space-y-1.5">
                  {course.prerequisites.map((req, idx) => (
                    <li
                      key={idx}
                      className="text-xs text-[var(--text-secondary)] flex items-start gap-2 leading-relaxed"
                    >
                      <span className="w-1.5 h-1.5 rounded-full bg-amber-500 shrink-0 mt-1.5" />
                      <span>{req}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* Chapter & Lesson Accordion Preview */}
            <div className="space-y-2.5">
              <div className="flex items-center justify-between">
                <h3 className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] flex items-center gap-1.5">
                  <Layers className="w-3.5 h-3.5 text-[#5E51E8]" />
                  <span>Chương trình chi tiết ({chapters.length} chương)</span>
                </h3>
                {isLoadingDetails && (
                  <span className="text-[11px] text-[var(--text-tertiary)] flex items-center gap-1">
                    <RefreshCw className="w-3 h-3 animate-spin" />
                    Đang tải...
                  </span>
                )}
              </div>

              {chapters.length > 0 ? (
                <div className="space-y-2">
                  {chapters.map((ch, chIdx) => {
                    const isExpanded = Boolean(expandedChapterIds[ch.id]);
                    const lessons = Array.isArray(ch.lessons) ? ch.lessons : [];
                    return (
                      <div
                        key={ch.id || chIdx}
                        className="rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/40 overflow-hidden"
                      >
                        <button
                          type="button"
                          onClick={() => toggleChapter(ch.id)}
                          className="w-full px-3.5 py-2.5 text-left flex items-center justify-between gap-2 hover:bg-[var(--surface-elevated)] transition cursor-pointer"
                        >
                          <div className="min-w-0 flex items-center gap-2">
                            <span className="w-5 h-5 rounded-full bg-[#EEF0FD] dark:bg-[#1E1B4B] text-[#4F46E5] dark:text-[#A5B4FC] text-[11px] font-bold flex items-center justify-center shrink-0">
                              {chIdx + 1}
                            </span>
                            <span className="text-xs font-semibold text-[var(--text-primary)] truncate">
                              {stripChapterPrefix(ch.title)}
                            </span>
                            <span className="text-[11px] text-[var(--text-tertiary)] shrink-0">
                              ({lessons.length} bài)
                            </span>
                          </div>
                          {isExpanded ? (
                            <ChevronUp className="w-4 h-4 text-[var(--text-tertiary)] shrink-0" />
                          ) : (
                            <ChevronDown className="w-4 h-4 text-[var(--text-tertiary)] shrink-0" />
                          )}
                        </button>

                        {isExpanded && lessons.length > 0 && (
                          <div className="border-t border-[var(--border-calm)] divide-y divide-[var(--border-calm)]/60 bg-[var(--surface-card)]">
                            {lessons.map((lesson, lIdx) => (
                              <div
                                key={lesson.id || lIdx}
                                className="px-3.5 py-2 flex items-center justify-between gap-2 hover:bg-[var(--surface-elevated)]/50 transition group"
                              >
                                <div className="min-w-0 flex items-center gap-2">
                                  {lesson.isCompleted ? (
                                    <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500 shrink-0" />
                                  ) : (
                                    <span className="w-1.5 h-1.5 rounded-full bg-[var(--text-tertiary)] shrink-0" />
                                  )}
                                  <span className="text-xs text-[var(--text-primary)] truncate">
                                    {lesson.title}
                                  </span>
                                  {lesson.isBookmarked && (
                                    <Bookmark className="w-3 h-3 text-amber-500 shrink-0 fill-current" />
                                  )}
                                </div>

                                <div className="flex items-center gap-2 shrink-0">
                                  {lesson.estimatedMinutes ? (
                                    <span className="text-[11px] text-[var(--text-tertiary)]">
                                      {lesson.estimatedMinutes}p
                                    </span>
                                  ) : null}
                                  <Link
                                    href={`/app/courses/${course.id}/learn?lessonId=${lesson.id}`}
                                    onClick={onClose}
                                    className="opacity-100 sm:opacity-0 sm:group-hover:opacity-100 sm:group-focus-within:opacity-100 px-2 py-0.5 rounded text-[11px] font-semibold text-[#5E51E8] hover:bg-[#EEF0FD] dark:hover:bg-[#1E1B4B] transition inline-flex items-center gap-1"
                                  >
                                    <span>Học</span>
                                    <Play className="w-2.5 h-2.5 fill-current" />
                                  </Link>
                                </div>
                              </div>
                            ))}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              ) : (
                <div className="py-6 text-center text-xs text-[var(--text-tertiary)] rounded-xl border border-dashed border-[var(--border-calm)]">
                  Chưa có chi tiết bài học nào cho khóa này.
                </div>
              )}
            </div>
          </div>

          {/* Drawer Footer Actions */}
          <div className="p-4 sm:px-6 border-t border-[var(--border-calm)] bg-[var(--surface-elevated)]/80 shadow-[0_-4px_16px_rgba(0,0,0,0.06)] dark:shadow-[0_-4px_16px_rgba(0,0,0,0.28)] flex flex-col sm:flex-row items-center gap-2.5">
            <Link
              href={`/app/courses/${course.id}/learn`}
              onClick={onClose}
              className="w-full sm:flex-1 h-9 px-4 rounded-xl bg-[#5E51E8] hover:bg-[#4F46E5] text-white text-xs font-semibold inline-flex items-center justify-center gap-1.5 shadow-xs transition"
            >
              <Play className="w-3.5 h-3.5 fill-current" />
              <span>{hasProgress ? "Tiếp tục học" : "Vào học ngay"}</span>
            </Link>

            <Link
              href={`/app/courses/${course.id}`}
              onClick={onClose}
              className="w-full sm:flex-1 h-9 px-4 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] hover:bg-[var(--surface-elevated)] text-xs font-semibold text-[var(--text-primary)] inline-flex items-center justify-center gap-1.5 transition"
            >
              <ExternalLink className="w-3.5 h-3.5" />
              <span>Mở Course Hub</span>
            </Link>

            <button
              type="button"
              onClick={handleClone}
              disabled={isCloning}
              className="w-full sm:w-auto h-9 px-3 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] hover:bg-[var(--surface-elevated)] text-xs font-semibold text-[var(--text-secondary)] hover:text-[var(--text-primary)] inline-flex items-center justify-center gap-1.5 transition disabled:opacity-50 cursor-pointer shrink-0"
              title="Nhân bản khóa học về kho cá nhân để chỉnh sửa"
            >
              {isCloning ? (
                <RefreshCw className="w-3.5 h-3.5 animate-spin" />
              ) : (
                <Copy className="w-3.5 h-3.5" />
              )}
              <span>Nhân bản</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
