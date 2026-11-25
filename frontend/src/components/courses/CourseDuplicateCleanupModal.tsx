"use client";

import React, { useEffect, useMemo, useRef, useState } from "react";
import {
  X,
  Trash2,
  AlertTriangle,
  CheckCircle2,
  Layers,
  Clock,
  Sparkles,
  RefreshCw,
  Copy,
  FileQuestion,
  ShieldCheck,
  Check,
} from "lucide-react";
import {
  type CourseSummary,
  detectPersonalDuplicates,
  getCourseLessonCount,
  getCourseProgress,
} from "@/lib/course-helpers";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";

interface CourseDuplicateCleanupModalProps {
  isOpen: boolean;
  onClose: () => void;
  courses: CourseSummary[];
  onCleanupSuccess: (deletedCount: number) => void;
}

export function CourseDuplicateCleanupModal({
  isOpen,
  onClose,
  courses,
  onCleanupSuccess,
}: CourseDuplicateCleanupModalProps) {
  const detectionResult = useMemo(() => detectPersonalDuplicates(courses), [courses]);

  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [isCleaning, setIsCleaning] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successReport, setSuccessReport] = useState<{
    deletedCount: number;
    deletedIds: string[];
    skippedIds: string[];
  } | null>(null);

  const modalDialogRef = useRef<HTMLDivElement | null>(null);
  const closeButtonRef = useRef<HTMLButtonElement | null>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);
  const onCloseRef = useRef(onClose);
  const isCleaningRef = useRef(isCleaning);
  onCloseRef.current = onClose;
  isCleaningRef.current = isCleaning;

  // Initialize selected IDs with all recommended removable IDs when opened
  useEffect(() => {
    if (isOpen) {
      setSelectedIds(new Set(detectionResult.allRemovableCourseIds));
      setErrorMessage(null);
      setSuccessReport(null);
    }
  }, [isOpen, detectionResult.allRemovableCourseIds]);

  // Body scroll lock, focus trap & ESC key
  useEffect(() => {
    if (!isOpen) return;

    previousFocusRef.current = document.activeElement as HTMLElement | null;
    const prevOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    const focusTimer = window.setTimeout(() => {
      closeButtonRef.current?.focus();
    }, 0);

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !isCleaningRef.current) {
        onCloseRef.current();
        return;
      }

      if (e.key === "Tab" && modalDialogRef.current) {
        const focusable = Array.from(
          modalDialogRef.current.querySelectorAll<HTMLElement>(
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
          if (activeEl === firstEl || !modalDialogRef.current.contains(activeEl)) {
            e.preventDefault();
            lastEl.focus();
          }
        } else {
          if (activeEl === lastEl || !modalDialogRef.current.contains(activeEl)) {
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

  if (!isOpen) return null;

  const toggleSelect = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const handleSelectAll = () => {
    setSelectedIds(new Set(detectionResult.allRemovableCourseIds));
  };

  const handleDeselectAll = () => {
    setSelectedIds(new Set());
  };

  const handlePerformCleanup = async () => {
    if (selectedIds.size === 0 || isCleaning) return;
    setIsCleaning(true);
    setErrorMessage(null);

    try {
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const courseIds = Array.from(selectedIds);

      const resp = await fetch("/api/v1/courses/cleanup-duplicates", {
        method: "POST",
        headers,
        body: JSON.stringify({ courseIds }),
      });

      if (!resp.ok) {
        const text = await resp.text().catch(() => "");
        throw new Error(text || `Lỗi dọn dẹp khóa học (HTTP ${resp.status}).`);
      }

      const result = await resp.json();
      const count = Number(result.deletedCount ?? courseIds.length);
      setSuccessReport({
        deletedCount: count,
        deletedIds: Array.isArray(result.deletedIds) ? result.deletedIds : courseIds,
        skippedIds: Array.isArray(result.skippedIds) ? result.skippedIds : [],
      });

      invalidateDashboardCache();
      onCleanupSuccess(count);
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : "Đã xảy ra lỗi khi dọn dẹp.");
    } finally {
      setIsCleaning(false);
    }
  };

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="cleanup-modal-title"
      className="fixed inset-0 z-50 overflow-y-auto"
    >
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/60 backdrop-blur-xs transition-opacity animate-in fade-in duration-200"
        onClick={() => !isCleaning && onClose()}
        aria-hidden="true"
      />

      {/* Modal Container */}
      <div className="flex min-h-full items-center justify-center p-3 sm:p-6">
        <div
          ref={modalDialogRef}
          className="relative w-full max-w-2xl bg-[var(--surface-card)] rounded-2xl border border-[var(--border-calm)] shadow-2xl overflow-hidden flex flex-col justify-between max-h-[88vh] sm:max-h-[85vh] animate-in zoom-in-95 duration-200"
        >
          {/* Modal Header */}
          <div className="px-4 sm:px-6 py-4 border-b border-[var(--border-calm)] flex items-start sm:items-center justify-between gap-3 bg-[var(--surface-elevated)]/50">
            <div className="flex items-start sm:items-center gap-2.5 min-w-0">
              <div className="w-8 h-8 rounded-xl bg-amber-500/10 border border-amber-500/20 flex items-center justify-center text-amber-600 dark:text-amber-400 shrink-0">
                <Trash2 className="w-4 h-4" />
              </div>
              <div className="min-w-0">
                <h2 id="cleanup-modal-title" className="text-sm sm:text-base font-bold text-[var(--text-primary)] break-words">
                  Dọn dẹp khóa học cá nhân trùng lặp &amp; rỗng
                </h2>
                <p className="text-xs text-[var(--text-secondary)]">
                  Chỉ rà soát các khóa cá nhân bạn sở hữu. Khóa chuẩn hệ thống luôn được bảo vệ tuyệt đối.
                </p>
              </div>
            </div>

            <button
              ref={closeButtonRef}
              type="button"
              onClick={onClose}
              disabled={isCleaning}
              className="p-1.5 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition disabled:opacity-50 cursor-pointer shrink-0"
              aria-label="Đóng cửa sổ"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Modal Body */}
          <div className="p-4 sm:p-6 overflow-y-auto space-y-5 flex-1">
            {/* Success Report View */}
            {successReport ? (
              <div className="py-8 text-center space-y-4">
                <div className="w-12 h-12 rounded-full bg-emerald-500/15 border border-emerald-500/30 text-emerald-600 dark:text-emerald-400 flex items-center justify-center mx-auto">
                  <Check className="w-6 h-6" />
                </div>
                <div className="space-y-1">
                  <h3 className="text-base font-bold text-[var(--text-primary)]">
                    Đã dọn dẹp thành công {successReport.deletedCount} khóa học cá nhân!
                  </h3>
                  <p className="text-xs text-[var(--text-secondary)] max-w-md mx-auto">
                    Thư viện của bạn đã được cập nhật sạch sẽ và không còn các bản sao trùng lặp thừa.
                  </p>
                </div>
                <button
                  type="button"
                  onClick={onClose}
                  className="px-5 py-2 rounded-xl bg-[#5E51E8] text-white text-xs font-semibold hover:bg-[#4F46E5] transition cursor-pointer"
                >
                  Hoàn tất &amp; Đóng
                </button>
              </div>
            ) : detectionResult.totalRemovableCount === 0 ? (
              <div className="py-10 text-center space-y-3">
                <div className="w-12 h-12 rounded-full bg-emerald-500/10 border border-emerald-500/20 text-emerald-600 dark:text-emerald-400 flex items-center justify-center mx-auto">
                  <ShieldCheck className="w-6 h-6" />
                </div>
                <div className="space-y-1">
                  <h3 className="text-sm font-bold text-[var(--text-primary)]">
                    Thư viện cá nhân của bạn hoàn toàn gọn gàng!
                  </h3>
                  <p className="text-xs text-[var(--text-secondary)] max-w-sm mx-auto">
                    Không phát hiện thấy bản sao trùng lặp nào hay khóa nháp rỗng cần dọn dẹp.
                  </p>
                </div>
              </div>
            ) : (
              <>
                {/* Error Banner */}
                {errorMessage && (
                  <div className="rounded-xl border border-red-500/30 bg-red-500/5 px-4 py-3 flex items-center gap-2.5 text-xs text-red-600 dark:text-red-400">
                    <AlertTriangle className="w-4 h-4 shrink-0" />
                    <span>{errorMessage}</span>
                  </div>
                )}

                {/* Quick Selection Actions Bar */}
                <div className="flex flex-wrap items-center justify-between gap-2.5 pb-2 border-b border-[var(--border-calm)]">
                  <span className="text-xs font-semibold text-[var(--text-secondary)]">
                    Phát hiện{" "}
                    <strong className="text-[var(--text-primary)]">
                      {detectionResult.totalRemovableCount} khóa có thể dọn
                    </strong>{" "}
                    (Đang chọn {selectedIds.size} khóa)
                  </span>

                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={handleSelectAll}
                      className="px-2.5 py-1 rounded-lg text-xs font-medium text-[#5E51E8] hover:bg-[#EEF0FD] dark:hover:bg-[#1E1B4B] transition cursor-pointer"
                    >
                      Chọn tất cả ({detectionResult.totalRemovableCount})
                    </button>
                    <span className="text-[var(--border-calm)]">|</span>
                    <button
                      type="button"
                      onClick={handleDeselectAll}
                      className="px-2.5 py-1 rounded-lg text-xs font-medium text-[var(--text-secondary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer"
                    >
                      Bỏ chọn
                    </button>
                  </div>
                </div>

                {/* Section 1: Title Duplicate Groups */}
                {detectionResult.titleDuplicateGroups.length > 0 && (
                  <div className="space-y-4">
                    <div className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                      <Copy className="w-3.5 h-3.5 text-[#5E51E8]" />
                      <span>
                        Khóa học trùng tên ({detectionResult.titleDuplicateGroups.length} nhóm)
                      </span>
                    </div>

                    <div className="space-y-4">
                      {detectionResult.titleDuplicateGroups.map((group, gIdx) => (
                        <div
                          key={group.normalizedTitle || gIdx}
                          className="rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/30 p-3.5 space-y-2.5"
                        >
                          <div className="flex items-center justify-between gap-2">
                            <span className="text-xs font-bold text-[var(--text-primary)] truncate">
                              {group.title}
                            </span>
                            <span className="text-[11px] text-[var(--text-tertiary)] shrink-0">
                              {group.courses.length} bản sao
                            </span>
                          </div>

                          <div className="space-y-2">
                            {group.courses.map((c) => {
                              const isKeep = c.id === group.suggestedKeepId;
                              const isChecked = selectedIds.has(c.id);
                              const progress = getCourseProgress(c);
                              const lessons = getCourseLessonCount(c);

                              return (
                                <div
                                  key={c.id}
                                  className={`p-2.5 rounded-lg border text-xs flex items-center justify-between gap-3 transition ${
                                    isKeep
                                      ? "border-2 border-emerald-500/75 dark:border-emerald-500/65 bg-emerald-50/60 dark:bg-emerald-950/30 shadow-2xs"
                                      : isChecked
                                      ? "border-rose-300 dark:border-rose-700/80 bg-rose-50/50 dark:bg-rose-950/25"
                                      : "border-[var(--border-calm)] bg-[var(--surface-card)]"
                                  }`}
                                >
                                  <div className="flex items-start sm:items-center gap-2.5 min-w-0 flex-1">
                                    {isKeep ? (
                                      <span className="w-4 h-4 mt-0.5 sm:mt-0 rounded-full bg-emerald-500 text-white flex items-center justify-center shrink-0">
                                        <Check className="w-3 h-3" />
                                      </span>
                                    ) : (
                                      <input
                                        type="checkbox"
                                        checked={isChecked}
                                        onChange={() => toggleSelect(c.id)}
                                        className="w-4 h-4 mt-0.5 sm:mt-0 rounded text-[#5E51E8] focus:ring-[#5E51E8] cursor-pointer shrink-0"
                                        aria-label={`Chọn xóa bản sao ${c.title}`}
                                      />
                                    )}

                                    <div className="min-w-0 flex-1">
                                      <div className="flex flex-wrap items-center gap-1.5">
                                        <span className="font-semibold text-[var(--text-primary)] truncate max-w-full">
                                          {c.title}
                                        </span>
                                        {isKeep ? (
                                          <span className="px-1.5 py-0.5 rounded text-[10px] font-bold bg-emerald-600 text-white shrink-0">
                                            Gợi ý giữ lại
                                          </span>
                                        ) : isChecked ? (
                                          <span className="px-1.5 py-0.5 rounded text-[10px] font-bold bg-rose-100 text-rose-700 border border-rose-200 dark:bg-rose-950/70 dark:text-rose-300 dark:border-rose-800 shrink-0">
                                            Sẽ xóa
                                          </span>
                                        ) : null}
                                      </div>
                                      <div className="flex flex-wrap items-center gap-x-2 gap-y-0.5 text-[11px] text-[var(--text-tertiary)] mt-0.5">
                                        <span>{lessons} bài học</span>
                                        <span>·</span>
                                        <span>Tiến độ {progress}%</span>
                                        {c.updatedAt && (
                                          <>
                                            <span>·</span>
                                            <span>
                                              Cập nhật: {new Date(c.updatedAt).toLocaleDateString("vi-VN")}
                                            </span>
                                          </>
                                        )}
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              );
                            })}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* Section 2: Empty Personal Courses */}
                {detectionResult.emptyCourses.length > 0 && (
                  <div className="space-y-3 pt-2">
                    <div className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                      <FileQuestion className="w-3.5 h-3.5 text-amber-500" />
                      <span>
                        Khóa học cá nhân rỗng / nháp cũ ({detectionResult.emptyCourses.length} khóa)
                      </span>
                    </div>

                    <div className="space-y-2">
                      {detectionResult.emptyCourses.map((c) => {
                        const isChecked = selectedIds.has(c.id);
                        return (
                          <div
                            key={c.id}
                            className={`p-2.5 rounded-lg border text-xs flex items-center justify-between gap-3 transition ${
                              isChecked
                                ? "border-rose-300 dark:border-rose-700/80 bg-rose-50/50 dark:bg-rose-950/25"
                                : "border-[var(--border-calm)] bg-[var(--surface-card)]"
                            }`}
                          >
                            <div className="flex items-start sm:items-center gap-2.5 min-w-0 flex-1">
                              <input
                                type="checkbox"
                                checked={isChecked}
                                onChange={() => toggleSelect(c.id)}
                                className="w-4 h-4 mt-0.5 sm:mt-0 rounded text-[#5E51E8] focus:ring-[#5E51E8] cursor-pointer shrink-0"
                                aria-label={`Chọn xóa khóa rỗng ${c.title}`}
                              />

                              <div className="min-w-0 flex-1">
                                <div className="flex flex-wrap items-center gap-1.5">
                                  <span className="font-semibold text-[var(--text-primary)] truncate max-w-full">
                                    {c.title}
                                  </span>
                                  {isChecked && (
                                    <span className="px-1.5 py-0.5 rounded text-[10px] font-bold bg-rose-100 text-rose-700 border border-rose-200 dark:bg-rose-950/70 dark:text-rose-300 dark:border-rose-800 shrink-0">
                                      Sẽ xóa
                                    </span>
                                  )}
                                </div>
                                <span className="text-[11px] text-[var(--text-tertiary)] block mt-0.5">
                                  0 bài học · Tạo lúc:{" "}
                                  {c.createdAt
                                    ? new Date(c.createdAt).toLocaleDateString("vi-VN")
                                    : "Chưa rõ"}
                                </span>
                              </div>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                )}
              </>
            )}
          </div>

          {/* Modal Footer */}
          {!successReport && detectionResult.totalRemovableCount > 0 && (
            <div className="px-4 sm:px-6 py-4 border-t border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
              <span className="text-xs text-[var(--text-secondary)]">
                {selectedIds.size > 0
                  ? `Sẽ xóa vĩnh viễn ${selectedIds.size} khóa học cá nhân đã chọn.`
                  : "Chưa chọn khóa nào để xóa."}
              </span>

              <div className="flex flex-col-reverse sm:flex-row items-stretch sm:items-center gap-2.5 w-full sm:w-auto">
                <button
                  type="button"
                  onClick={onClose}
                  disabled={isCleaning}
                  className="px-4 py-2 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-semibold text-[var(--text-secondary)] hover:bg-[var(--surface-elevated)] transition disabled:opacity-50 cursor-pointer text-center"
                >
                  Hủy bỏ
                </button>

                <button
                  type="button"
                  onClick={handlePerformCleanup}
                  disabled={selectedIds.size === 0 || isCleaning}
                  className="px-4 py-2 rounded-xl bg-red-600 hover:bg-red-700 text-white text-xs font-semibold inline-flex items-center justify-center gap-1.5 shadow-xs transition disabled:opacity-50 cursor-pointer"
                >
                  {isCleaning ? (
                    <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                  ) : (
                    <Trash2 className="w-3.5 h-3.5" />
                  )}
                  <span>Xóa {selectedIds.size} khóa đã chọn</span>
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
