"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import {
  AlertCircle,
  Bookmark,
  BookOpen,
  Brain,
  CheckCircle2,
  ChevronRight,
  ClipboardCheck,
  FileText,
  FolderArchive,
  Highlighter,
  NotebookPen,
  Quote,
  RefreshCw,
  RotateCcw,
  Sparkles,
} from "lucide-react";
import { Button, Card, Badge, EmptyState } from "@/components/ui";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import type { CourseDetail, WorkspaceAssetsData } from "./types";

export interface CourseAssetsTabProps {
  course: CourseDetail;
  courseId: string;
}

export function CourseAssetsTab({ course, courseId }: CourseAssetsTabProps) {
  const { activeUserHeaderId } = useWorkspaceStore();

  const [assets, setAssets] = useState<WorkspaceAssetsData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAssets = useCallback(async () => {
    if (!courseId) return;
    setIsLoading(true);
    setError(null);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );

      const resp = await fetch(`/api/v1/courses/${courseId}/workspace-assets`, {
        headers,
        credentials: "include",
      });

      if (!resp.ok) {
        const errJson = await resp.json().catch(() => null);
        const errMsg = errJson?.error || `Không tải được tài nguyên khóa học (HTTP ${resp.status})`;
        setError(errMsg);
        return;
      }

      const data: WorkspaceAssetsData = await resp.json();
      setAssets(data);
    } catch {
      setError("Không thể kết nối tới máy chủ để tải tài nguyên khóa học.");
    } finally {
      setIsLoading(false);
    }
  }, [courseId, activeUserHeaderId]);

  useEffect(() => {
    fetchAssets();
  }, [fetchAssets]);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="h-28 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="h-48 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
          <div className="h-48 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <Card className="p-6 bg-[var(--surface-card)] text-center space-y-4">
        <div className="inline-flex items-center gap-2 text-sm text-red-600 dark:text-red-400 bg-red-500/10 px-4 py-2 rounded-lg border border-red-500/20">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
        <div>
          <Button
            variant="secondary"
            size="sm"
            onClick={fetchAssets}
            leftIcon={<RefreshCw className="w-3.5 h-3.5" />}
          >
            Tải lại tài nguyên
          </Button>
        </div>
      </Card>
    );
  }

  const bookmarked = assets?.bookmarkedLessons ?? [];
  const needsReview = assets?.needsReviewLessons ?? [];
  const notebooks = assets?.notebooks ?? [];
  const annotations = assets?.annotations ?? [];
  const flashcards = assets?.flashcards?.items ?? [];
  const quizzes = assets?.quizzes ?? [];

  const totalFlashcardsCount = assets?.flashcards?.total ?? flashcards.length;

  const totalAssetsCount =
    bookmarked.length +
    needsReview.length +
    notebooks.length +
    annotations.length +
    flashcards.length +
    quizzes.length;

  const scrollToSection = (sectionId: string) => {
    if (typeof document === "undefined") return;
    document.getElementById(sectionId)?.scrollIntoView({ behavior: "smooth", block: "start" });
  };

  const summaryPills = [
    {
      label: "Đánh dấu",
      count: bookmarked.length,
      targetId: "assets-flags",
      icon: Bookmark,
      colorClass: "text-amber-600 dark:text-amber-400 bg-amber-500/10 border-amber-500/20",
    },
    {
      label: "Cần ôn",
      count: needsReview.length,
      targetId: "assets-flags",
      icon: RotateCcw,
      colorClass: "text-rose-600 dark:text-rose-400 bg-rose-500/10 border-rose-500/20",
    },
    {
      label: "Sổ tay",
      count: notebooks.length,
      targetId: "assets-notebooks",
      icon: NotebookPen,
      colorClass: "text-indigo-600 dark:text-indigo-400 bg-indigo-500/10 border-indigo-500/20",
    },
    {
      label: "Highlight",
      count: annotations.length,
      targetId: "assets-annotations",
      icon: Highlighter,
      colorClass: "text-amber-600 dark:text-amber-400 bg-amber-500/10 border-amber-500/20",
    },
    {
      label: "Flashcards",
      count: totalFlashcardsCount,
      targetId: "assets-flashcards-quizzes",
      icon: Brain,
      colorClass: "text-orange-600 dark:text-orange-400 bg-orange-500/10 border-orange-500/20",
    },
    {
      label: "Quiz",
      count: quizzes.length,
      targetId: "assets-flashcards-quizzes",
      icon: ClipboardCheck,
      colorClass: "text-emerald-600 dark:text-emerald-400 bg-emerald-500/10 border-emerald-500/20",
    },
  ];

  return (
    <div className="space-y-6">
      {/* Header bar */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h2 className="text-base font-bold text-[var(--text-primary)] flex items-center gap-2">
            <FolderArchive className="w-4 h-4 text-[#5E51E8] dark:text-[#A5B4FC]" />
            <span>Tài nguyên &amp; Không gian ghi chú gắn với khóa học</span>
          </h2>
          <p className="text-xs text-[var(--text-secondary)] mt-0.5">
            Tổng hợp toàn bộ đánh dấu, sổ tay, trích dẫn highlight, thẻ FSRS và đề kiểm tra của bạn.
          </p>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={fetchAssets}
          leftIcon={<RefreshCw className="w-3.5 h-3.5" />}
          className="self-start sm:self-auto shrink-0"
        >
          Làm mới
        </Button>
      </div>

      {/* 6 Summary Counter Pills */}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-2.5">
        {summaryPills.map((pill) => {
          const Icon = pill.icon;
          return (
            <button
              key={pill.label}
              type="button"
              onClick={() => scrollToSection(pill.targetId)}
              className="flex items-center justify-between gap-2 px-3 py-2.5 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] hover:border-[#5E51E8]/50 hover:bg-[var(--surface-elevated)] transition text-left cursor-pointer"
            >
              <div className="flex items-center gap-2 min-w-0">
                <span className={`w-7 h-7 rounded-lg border flex items-center justify-center shrink-0 ${pill.colorClass}`}>
                  <Icon className="w-3.5 h-3.5" />
                </span>
                <span className="text-xs font-medium text-[var(--text-secondary)] truncate">
                  {pill.label}
                </span>
              </div>
              <span className="text-sm font-bold text-[var(--text-primary)] shrink-0">
                {pill.count}
              </span>
            </button>
          );
        })}
      </div>

      {totalAssetsCount === 0 && (
        <EmptyState
          icon={FolderArchive}
          title="Chưa có tài nguyên nào được tạo trong khóa học này"
          description="Trong quá trình học, bạn có thể đánh dấu bài quan trọng (🔖), đánh dấu cần ôn lại (🔁), highlight trích dẫn hoặc tạo sổ tay ghi chú để hiển thị tại đây."
          action={{
            label: "Vào học bài đầu tiên",
            href: `/app/courses/${courseId}/learn`,
          }}
        />
      )}

      {/* 1. Bookmarked & Needs Review Lessons */}
      {(bookmarked.length > 0 || needsReview.length > 0) && (
        <div id="assets-flags" className="grid grid-cols-1 md:grid-cols-2 gap-4 scroll-mt-6">
          {/* Bookmarked Lessons */}
          <Card className="p-4 sm:p-5 bg-[var(--surface-card)] space-y-3">
            <div className="flex items-center justify-between border-b border-[var(--border-calm)] pb-2.5">
              <h3 className="text-xs font-bold uppercase tracking-wider text-amber-600 dark:text-amber-400 flex items-center gap-1.5">
                <Bookmark className="w-4 h-4 fill-amber-500/20" />
                <span>Bài học đã đánh dấu ({bookmarked.length})</span>
              </h3>
              <Badge variant="neutral" size="sm">🔖 Bookmark</Badge>
            </div>

            {bookmarked.length > 0 ? (
              <div className="divide-y divide-[var(--border-calm)]">
                {bookmarked.map((l) => (
                  <Link
                    key={l.id}
                    href={`/app/courses/${courseId}/learn?lessonId=${l.id}`}
                    className="py-2.5 first:pt-1 last:pb-1 flex items-center justify-between gap-3 group hover:text-[#5E51E8] transition"
                  >
                    <span className="text-xs font-medium text-[var(--text-primary)] group-hover:text-[#5E51E8] line-clamp-1">
                      {l.title}
                    </span>
                    <span className="text-[11px] text-[var(--text-tertiary)] shrink-0 inline-flex items-center gap-0.5">
                      Vào bài <ChevronRight className="w-3 h-3" />
                    </span>
                  </Link>
                ))}
              </div>
            ) : (
              <p className="text-xs text-[var(--text-tertiary)] italic">
                Chưa có bài học nào được đánh dấu.
              </p>
            )}
          </Card>

          {/* Needs Review Lessons */}
          <Card className="p-4 sm:p-5 bg-[var(--surface-card)] space-y-3">
            <div className="flex items-center justify-between border-b border-[var(--border-calm)] pb-2.5">
              <h3 className="text-xs font-bold uppercase tracking-wider text-rose-600 dark:text-rose-400 flex items-center gap-1.5">
                <RotateCcw className="w-4 h-4" />
                <span>Bài học cần ôn lại ({needsReview.length})</span>
              </h3>
              <Badge variant="neutral" size="sm">🔁 Review</Badge>
            </div>

            {needsReview.length > 0 ? (
              <div className="divide-y divide-[var(--border-calm)]">
                {needsReview.map((l) => (
                  <Link
                    key={l.id}
                    href={`/app/courses/${courseId}/learn?lessonId=${l.id}`}
                    className="py-2.5 first:pt-1 last:pb-1 flex items-center justify-between gap-3 group hover:text-rose-600 transition"
                  >
                    <span className="text-xs font-medium text-[var(--text-primary)] group-hover:text-rose-600 line-clamp-1">
                      {l.title}
                    </span>
                    <span className="text-[11px] text-[var(--text-tertiary)] shrink-0 inline-flex items-center gap-0.5">
                      Ôn ngay <ChevronRight className="w-3 h-3" />
                    </span>
                  </Link>
                ))}
              </div>
            ) : (
              <p className="text-xs text-[var(--text-tertiary)] italic">
                Chưa có bài học nào được đánh dấu cần ôn lại.
              </p>
            )}
          </Card>
        </div>
      )}

      {/* 2. Sổ tay & Ghi chú (Notebooks) */}
      <Card id="assets-notebooks" className="p-5 bg-[var(--surface-card)] space-y-4 scroll-mt-6">
        <div className="flex items-center justify-between border-b border-[var(--border-calm)] pb-3">
          <div className="flex items-center gap-2">
            <NotebookPen className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
            <h3 className="text-sm font-bold text-[var(--text-primary)]">
              Sổ tay &amp; Ghi chú học tập ({notebooks.length})
            </h3>
          </div>
          <Link
            href="/app/notes"
            className="text-xs text-[#5E51E8] dark:text-[#A5B4FC] hover:underline"
          >
            Quản lý tất cả sổ tay →
          </Link>
        </div>

        {notebooks.length > 0 ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
            {notebooks.map((nb) => {
              const targetHref = nb.primarySourceLessonId
                ? `/app/courses/${courseId}/learn?lessonId=${nb.primarySourceLessonId}&notebook=${nb.id}&notes=open`
                : `/app/courses/${courseId}/learn?notebook=${nb.id}&notes=open`;

              const updatedDate = nb.updatedAt
                ? new Date(nb.updatedAt).toLocaleDateString("vi-VN")
                : "";

              return (
                <Link
                  key={nb.id}
                  href={targetHref}
                  className="p-3.5 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)] hover:border-indigo-500/50 hover:bg-[var(--surface-elevated)]/80 transition flex flex-col justify-between gap-2.5 group"
                >
                  <div className="space-y-1">
                    <p className="text-xs font-bold text-[var(--text-primary)] group-hover:text-indigo-600 dark:group-hover:text-indigo-400 line-clamp-2">
                      {nb.title || "Sổ tay chưa đặt tên"}
                    </p>
                    {updatedDate && (
                      <p className="text-[10px] text-[var(--text-tertiary)]">
                        Cập nhật: {updatedDate}
                      </p>
                    )}
                  </div>
                  <div className="flex items-center justify-between text-[11px] text-indigo-600 dark:text-indigo-400 font-medium pt-1 border-t border-[var(--border-calm)]/40">
                    <span>Mở ghi chú trong bài</span>
                    <ChevronRight className="w-3 h-3" />
                  </div>
                </Link>
              );
            })}
          </div>
        ) : (
          <p className="text-xs text-[var(--text-tertiary)] italic">
            Chưa có sổ tay ghi chú nào gắn với khóa học này. Hãy mở bài học bất kỳ và bấm biểu tượng Sổ tay bên thanh công cụ để tạo ghi chú.
          </p>
        )}
      </Card>

      {/* 3. Highlights / Annotations */}
      <Card id="assets-annotations" className="p-5 bg-[var(--surface-card)] space-y-4 scroll-mt-6">
        <div className="flex items-center justify-between border-b border-[var(--border-calm)] pb-3">
          <div className="flex items-center gap-2">
            <Highlighter className="w-4 h-4 text-amber-500" />
            <h3 className="text-sm font-bold text-[var(--text-primary)]">
              Trích dẫn Highlight &amp; Ghi chú lề ({annotations.length})
            </h3>
          </div>
        </div>

        {annotations.length > 0 ? (
          <div className="space-y-3">
            {annotations.map((ann) => {
              const lessonHref = ann.lessonId
                ? `/app/courses/${courseId}/learn?lessonId=${ann.lessonId}`
                : `/app/courses/${courseId}/learn`;

              return (
                <div
                  key={ann.id}
                  className="p-3.5 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)] space-y-2"
                >
                  {ann.exactQuote && (
                    <div className="flex items-start gap-2 text-xs italic text-[var(--text-secondary)] border-l-2 border-amber-500 pl-3 py-0.5">
                      <Quote className="w-3.5 h-3.5 text-amber-500 shrink-0 mt-0.5" />
                      <span>&ldquo;{ann.exactQuote}&rdquo;</span>
                    </div>
                  )}

                  {ann.noteComment && (
                    <p className="text-xs font-medium text-[var(--text-primary)] pl-5">
                      {ann.noteComment}
                    </p>
                  )}

                  <div className="flex items-center justify-between pt-1 border-t border-[var(--border-calm)]/40 text-[11px]">
                    <span className="text-[var(--text-tertiary)]">
                      {ann.semanticLabel ? `Nhãn: ${ann.semanticLabel}` : "Highlight ghi chú"}
                    </span>
                    <Link
                      href={lessonHref}
                      className="text-[#5E51E8] dark:text-[#A5B4FC] hover:underline inline-flex items-center gap-0.5 font-medium"
                    >
                      <span>Xem trong bài học</span>
                      <ChevronRight className="w-3 h-3" />
                    </Link>
                  </div>
                </div>
              );
            })}
          </div>
        ) : (
          <p className="text-xs text-[var(--text-tertiary)] italic">
            Chưa có trích dẫn highlight nào. Bạn có thể bôi đen văn bản trong bài giảng để tạo highlight và ghi chú tức thì.
          </p>
        )}
      </Card>

      {/* 4. Flashcards FSRS & Practice Quizzes Snapshot */}
      <div id="assets-flashcards-quizzes" className="grid grid-cols-1 md:grid-cols-2 gap-5 scroll-mt-6">
        {/* Flashcards Preview */}
        <Card className="p-5 bg-[var(--surface-card)] space-y-3">
          <div className="flex items-center justify-between border-b border-[var(--border-calm)] pb-2.5">
            <h3 className="text-xs font-bold uppercase tracking-wider text-amber-600 dark:text-amber-400 flex items-center gap-1.5">
              <Brain className="w-4 h-4" />
              <span>Thẻ nhớ FSRS ({assets?.flashcards?.total ?? 0})</span>
            </h3>
            <Link
              href={`/app/review?courseId=${courseId}`}
              className="text-xs text-amber-600 dark:text-amber-400 hover:underline"
            >
              Luyện tập ngay →
            </Link>
          </div>

          {flashcards.length > 0 ? (
            <div className="space-y-2">
              {flashcards.slice(0, 5).map((fc) => (
                <div
                  key={fc.id}
                  className="p-2.5 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs space-y-1"
                >
                  <p className="font-semibold text-[var(--text-primary)] line-clamp-1">
                    {fc.frontText || fc.sourceTitleSnapshot || "Thẻ ghi nhớ"}
                  </p>
                  {fc.backText && (
                    <p className="text-[var(--text-secondary)] line-clamp-1 text-[11px]">
                      {fc.backText}
                    </p>
                  )}
                </div>
              ))}
              {flashcards.length > 5 && (
                <p className="text-center text-[11px] text-[var(--text-tertiary)] pt-1">
                  ...và còn {flashcards.length - 5} thẻ khác trong bộ nhớ.
                </p>
              )}
            </div>
          ) : (
            <p className="text-xs text-[var(--text-tertiary)] italic">
              Chưa có thẻ nhớ nào được tạo cho khóa học này.
            </p>
          )}
        </Card>

        {/* Quizzes Snapshot */}
        <Card className="p-5 bg-[var(--surface-card)] space-y-3">
          <div className="flex items-center justify-between border-b border-[var(--border-calm)] pb-2.5">
            <h3 className="text-xs font-bold uppercase tracking-wider text-emerald-600 dark:text-emerald-400 flex items-center gap-1.5">
              <ClipboardCheck className="w-4 h-4" />
              <span>Bài kiểm tra Quiz ({quizzes.length})</span>
            </h3>
            <Link
              href={`/app/courses/${courseId}/quiz`}
              className="text-xs text-emerald-600 dark:text-emerald-400 hover:underline"
            >
              Vào làm Quiz →
            </Link>
          </div>

          {quizzes.length > 0 ? (
            <div className="space-y-2">
              {quizzes.slice(0, 5).map((q) => (
                <div
                  key={q.id}
                  className="p-2.5 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs flex items-center justify-between gap-2"
                >
                  <div className="space-y-0.5 min-w-0">
                    <p className="font-semibold text-[var(--text-primary)] truncate">
                      {q.title || "Bài kiểm tra"}
                    </p>
                    <p className="text-[11px] text-[var(--text-tertiary)]">
                      {q.totalItems} câu hỏi · Trạng thái: {q.status === "completed" ? "Đã làm" : "Chưa làm"}
                    </p>
                  </div>
                  <Link
                    href={`/app/courses/${courseId}/quiz?quizId=${q.id}`}
                    className="shrink-0 text-emerald-600 dark:text-emerald-400 hover:underline text-xs font-semibold"
                  >
                    Bắt đầu
                  </Link>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-xs text-[var(--text-tertiary)] italic">
              Chưa có bài kiểm tra trắc nghiệm nào.
            </p>
          )}
        </Card>
      </div>
    </div>
  );
}
