"use client";

import React, { Suspense, useCallback, useEffect, useRef, useState } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import {
  ArrowLeft,
  Brain,
  RotateCcw,
  Check,
  Clock,
  Zap,
  AlertCircle,
  BookOpen,
  Keyboard,
  Layers,
  Plus,
  Sparkles,
  Volume2,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached } from "@/lib/api-client";
import { enqueueOfflineMutation, removeOfflineMutation } from "@/lib/offline-queue";
import { getReviewCapSettings } from "@/lib/review-settings";
import {
  deduplicateCourses,
  formatCleanQuizPrompt,
  sortCoursesSmartly,
  type CourseLike,
} from "@/lib/course-helpers";

interface FlashcardTagItem {
  id: string;
  name: string;
  colorKey?: string;
}

interface IntervalPreviews {
  again: string;
  hard: string;
  good: string;
  easy: string;
}

interface DueFlashcardItem {
  id: string;
  lessonId?: string | null;
  documentId?: string | null;
  annotationId?: string | null;
  pageNumber?: number | null;
  cardType?: string | null;
  sourceTitleSnapshot?: string | null;
  sourceQuoteSnapshot?: string | null;
  frontText: string;
  backText: string;
  stability: number;
  difficulty: number;
  reps: number;
  lapses: number;
  state: string;
  lastReviewAt?: string | null;
  dueAt?: string | null;
  tags: FlashcardTagItem[];
  intervalPreviews?: IntervalPreviews;
}

interface CourseFilterOption {
  id: string;
  title: string;
}

interface ReviewHistoryEntry {
  cardId: string;
  index: number;
  wasCompleted: boolean;
}

interface ReviewQueue {
  cards: DueFlashcardItem[];
  totalDue: number;
  totalCards: number;
  newCount: number;
  learningCount: number;
  reviewCount: number;
  estimatedMinutes: number;
}

function formatFriendlyReviewError(status: number, rawText: string, defaultAction: string): string {
  let code = "";
  let message = "";
  try {
    const parsed = JSON.parse(rawText);
    code = String(parsed.error || parsed.code || "");
    message = String(parsed.message || "");
  } catch {
    code = rawText.trim();
  }

  if (code.includes("TOKEN_VERSION_REVOKED") || status === 401) {
    return "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
  }
  if (code.includes("FORBIDDEN") || status === 403) {
    return "Bạn không có quyền thực hiện thao tác này.";
  }
  if (code.includes("RATE_LIMIT") || status === 429) {
    return "Hệ thống đang bận. Vui lòng thử lại sau vài giây.";
  }
  if (message && !message.startsWith("{")) {
    return message;
  }
  return `${defaultAction} thất bại (Mã lỗi ${status}). Đã lưu vào hàng đợi offline.`;
}

function ReviewPageContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { activeUserHeaderId } = useWorkspaceStore();

  const paramCourseId = searchParams.get("courseId") ?? "";
  const paramTagId = searchParams.get("tagId") ?? "";

  const [queue, setQueue] = useState<ReviewQueue>({
    cards: [],
    totalDue: 0,
    totalCards: 0,
    newCount: 0,
    learningCount: 0,
    reviewCount: 0,
    estimatedMinutes: 0,
  });
  const [courses, setCourses] = useState<CourseFilterOption[]>([]);
  const [availableTags, setAvailableTags] = useState<FlashcardTagItem[]>([]);
  const [selectedCourseId, setSelectedCourseId] = useState<string>(paramCourseId);
  const [selectedTagId, setSelectedTagId] = useState<string>(paramTagId);
  const [practiceAll, setPracticeAll] = useState(false);

  useEffect(() => {
    setSelectedCourseId(paramCourseId);
  }, [paramCourseId]);

  useEffect(() => {
    setSelectedTagId(paramTagId);
  }, [paramTagId]);

  const [currentIndex, setCurrentIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [sessionComplete, setSessionComplete] = useState(false);
  const [reviewHistory, setReviewHistory] = useState<ReviewHistoryEntry[]>([]);
  const [errorBanner, setErrorBanner] = useState<string | null>(null);
  const [undoBanner, setUndoBanner] = useState<string | null>(null);
  const [extraSessionCap, setExtraSessionCap] = useState(0);
  const [totalBacklog, setTotalBacklog] = useState(0);

  const cardShownAtRef = useRef<number>(Date.now());

  const getAuthHeaders = useCallback(
    (extra?: Record<string, string>) => {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      return getClientRequestHeaders({
        ...(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : {}),
        ...(extra ?? {}),
      });
    },
    [activeUserHeaderId]
  );

  const fetchReviewQueue = useCallback(
    async (overridePracticeAll?: boolean) => {
      const useIncludeAll = overridePracticeAll ?? practiceAll;
      setIsLoading(true);
      setErrorBanner(null);
      try {
        const headers = getAuthHeaders();

        const params = new URLSearchParams();
        if (selectedCourseId) params.set("courseId", selectedCourseId);
        if (selectedTagId) params.set("tagId", selectedTagId);
        if (useIncludeAll) params.set("includeAll", "true");
        const queryString = params.toString();

        const [dashData, cardsResp] = await Promise.all([
          fetchDashboardCached({ headers }).catch(() => null),
          fetch(`/api/v1/flashcards/due${queryString ? `?${queryString}` : ""}`, { headers }),
        ]);

        let totalDue = 0;
        let totalCards = 0;
        let newCount = 0;
        let learningCount = 0;
        let reviewCount = 0;
        let estimatedMinutes = 0;

        if (dashData) {
          const rs = dashData.reviewStation ?? {};
          totalDue = rs.totalDue ?? 0;
          totalCards = rs.totalCards ?? 0;
          newCount = rs.newCount ?? 0;
          learningCount = rs.learningCount ?? 0;
          reviewCount = rs.reviewCount ?? 0;
          estimatedMinutes = rs.estimatedMinutes ?? Math.max(1, Math.ceil(totalDue * 0.5));

          const rawCourses = Array.isArray(dashData.courses) ? dashData.courses : [];
          const sortedCourses = sortCoursesSmartly(deduplicateCourses(rawCourses as CourseLike[]));
          setCourses(
            sortedCourses
              .map((c) => ({
                id: String(c.id ?? c.courseId ?? ""),
                title: String(c.title ?? "Khóa học"),
              }))
              .filter((c: CourseFilterOption) => Boolean(c.id))
          );
          if (Array.isArray(dashData.tags)) {
            setAvailableTags(dashData.tags);
          }
        }

        if (!cardsResp.ok) {
          const errText = await cardsResp.text().catch(() => "");
          setErrorBanner(
            formatFriendlyReviewError(cardsResp.status, errText, "Tải hàng đợi flashcard")
          );
          setQueue({
            cards: [],
            totalDue,
            totalCards,
            newCount,
            learningCount,
            reviewCount,
            estimatedMinutes,
          });
          return;
        }

        const cardsData = await cardsResp.json();
        const rawCards: DueFlashcardItem[] = Array.isArray(cardsData)
          ? cardsData
          : (cardsData.cards ?? []);

        const caps = getReviewCapSettings();
        const effectiveCap = caps.maxReviewsPerDay + extraSessionCap;
        const cards = !useIncludeAll && rawCards.length > effectiveCap
          ? rawCards.slice(0, effectiveCap)
          : rawCards;
        setTotalBacklog(Math.max(0, rawCards.length - cards.length));

        setQueue({
          cards,
          totalDue: cards.length > 0 ? cards.length : totalDue,
          totalCards: Math.max(totalCards, rawCards.length),
          newCount: newCount || cards.filter((c) => c.state === "New").length,
          learningCount:
            learningCount ||
            cards.filter((c) => c.state === "Learning" || c.state === "Relearning").length,
          reviewCount: reviewCount || cards.filter((c) => c.state === "Review").length,
          estimatedMinutes: estimatedMinutes || Math.max(1, Math.ceil(cards.length * 0.5)),
        });
        setCurrentIndex(0);
        setIsFlipped(false);
        setSessionComplete(false);
        cardShownAtRef.current = Date.now();
      } catch (err) {
        setErrorBanner(
          err instanceof Error ? err.message : "Lỗi kết nối khi tải hàng đợi ôn tập."
        );
      } finally {
        setIsLoading(false);
      }
    },
    [getAuthHeaders, practiceAll, selectedCourseId, selectedTagId, extraSessionCap]
  );

  useEffect(() => {
    fetchReviewQueue();
  }, [fetchReviewQueue]);

  useEffect(() => {
    const onMutated = (e: Event) => {
      const detail = (e as CustomEvent<{ origin?: string }> | undefined)?.detail;
      if (detail?.origin === "review") return;
      fetchReviewQueue();
    };
    window.addEventListener("swms:data-mutated", onMutated);
    return () => window.removeEventListener("swms:data-mutated", onMutated);
  }, [fetchReviewQueue]);

  const handleUndoLastReview = useCallback(async () => {
    if (reviewHistory.length === 0 || isSubmitting) return;
    const lastEntry = reviewHistory[reviewHistory.length - 1];
    setIsSubmitting(true);
    setErrorBanner(null);
    try {
      const headers = getAuthHeaders({ "Content-Type": "application/json" });
      const resp = await fetch(`/api/v1/flashcards/${lastEntry.cardId}/undo-last-review`, {
        method: "POST",
        headers,
      });
      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(
          formatFriendlyReviewError(resp.status, errText, "Hoàn tác đánh giá")
        );
        return;
      }
      setReviewHistory((prev) => prev.slice(0, -1));
      setSessionComplete(false);
      setCurrentIndex(lastEntry.index);
      setIsFlipped(false);
      cardShownAtRef.current = Date.now();
      setUndoBanner("Đã hoàn tác lượt đánh giá thẻ vừa rồi.");
      window.dispatchEvent(new CustomEvent("swms:data-mutated", { detail: { origin: "review" } }));
    } catch (err) {
      setErrorBanner(
        err instanceof Error ? err.message : "Lỗi kết nối khi hoàn tác đánh giá."
      );
    } finally {
      setIsSubmitting(false);
    }
  }, [getAuthHeaders, isSubmitting, reviewHistory]);

  const handleRate = useCallback(
    async (rating: "again" | "hard" | "good" | "easy") => {
      const card = queue.cards[currentIndex];
      if (!card || isSubmitting) return;

      setIsSubmitting(true);
      setErrorBanner(null);
      setUndoBanner(null);

      const ratingMap = { again: 1, hard: 2, good: 3, easy: 4 } as const;
      const reviewEventId = crypto.randomUUID();
      const clientTimestamp = new Date().toISOString();
      const reviewDurationMs = Math.max(100, Date.now() - cardShownAtRef.current);
      const reviewPayload = {
        reviewEventId,
        flashcardId: card.id,
        rating: ratingMap[rating],
        clientTimestamp,
        reviewDurationMs,
      };

      enqueueOfflineMutation({
        mutationId: reviewEventId,
        entityType: "flashcard_review",
        entityId: card.id,
        baseVersion: card.reps,
        deviceId: "swms-v2-web",
        clientTimestamp,
        payload: reviewPayload,
      });

      try {
        const headers = getAuthHeaders({ "Content-Type": "application/json" });
        const resp = await fetch("/api/v1/flashcards/sync-reviews", {
          method: "POST",
          headers,
          body: JSON.stringify({ reviews: [reviewPayload] }),
        });

        if (!resp.ok) {
          const errText = await resp.text().catch(() => "");
          setErrorBanner(
            formatFriendlyReviewError(resp.status, errText, "Đồng bộ đánh giá")
          );
          return;
        }

        removeOfflineMutation(reviewEventId);
        const reachedEnd = currentIndex >= queue.cards.length - 1;
        setReviewHistory((prev) => [
          ...prev,
          { cardId: card.id, index: currentIndex, wasCompleted: reachedEnd },
        ]);
        window.dispatchEvent(new CustomEvent("swms:data-mutated", { detail: { origin: "review" } }));

        if (!reachedEnd) {
          setCurrentIndex((prev) => prev + 1);
          setIsFlipped(false);
          cardShownAtRef.current = Date.now();
        } else {
          setSessionComplete(true);
        }
      } catch {
        setErrorBanner(
          "Mất kết nối — đánh giá đã được lưu vào hàng đợi offline, sẽ đồng bộ lại sau."
        );
      } finally {
        setIsSubmitting(false);
      }
    },
    [currentIndex, getAuthHeaders, isSubmitting, queue.cards]
  );

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement;
      if (
        target.tagName === "INPUT" ||
        target.tagName === "TEXTAREA" ||
        target.tagName === "SELECT" ||
        target.isContentEditable
      ) {
        return;
      }

      if (!e.ctrlKey && !e.metaKey && !e.altKey && (e.key === "z" || e.key === "Z")) {
        if (reviewHistory.length > 0) {
          e.preventDefault();
          void handleUndoLastReview();
        }
        return;
      }

      if (sessionComplete || !queue.cards[currentIndex]) return;

      if (e.key === " " || e.code === "Space") {
        e.preventDefault();
        if (!isFlipped) {
          setIsFlipped(true);
        }
      } else if (isFlipped) {
        if (e.key === "1") {
          e.preventDefault();
          void handleRate("again");
        } else if (e.key === "2") {
          e.preventDefault();
          void handleRate("hard");
        } else if (e.key === "3") {
          e.preventDefault();
          void handleRate("good");
        } else if (e.key === "4") {
          e.preventDefault();
          void handleRate("easy");
        }
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [currentIndex, handleRate, handleUndoLastReview, isFlipped, queue.cards, reviewHistory.length, sessionComplete]);

  const renderClozeText = (text: string, reveal: boolean) => {
    const regex = /\{\{c\d+::(.*?)\}\}/g;
    const parts: React.ReactNode[] = [];
    let lastIndex = 0;
    let match: RegExpExecArray | null;

    while ((match = regex.exec(text)) !== null) {
      if (match.index > lastIndex) {
        parts.push(text.slice(lastIndex, match.index));
      }
      const hiddenTerm = match[1];
      parts.push(
        <span
          key={`${match.index}-${hiddenTerm}`}
          className={`px-2 py-0.5 mx-1 rounded font-semibold ${
            reveal
              ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border border-emerald-500/40"
              : "bg-accent text-white"
          }`}
        >
          {reveal ? hiddenTerm : "[ ... ? ... ]"}
        </span>
      );
      lastIndex = regex.lastIndex;
    }

    if (lastIndex < text.length) {
      parts.push(text.slice(lastIndex));
    }
    return parts.length > 0 ? parts : text;
  };

  const currentCard = queue.cards[currentIndex];

  if (isLoading) {
    return (
      <div className="max-w-2xl mx-auto px-4 py-8 min-h-[80vh] flex items-center justify-center">
        <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  // Filter controls helper
  const hasFilters = Boolean(selectedCourseId || selectedTagId);

  // Empty / Session complete state
  if (sessionComplete || queue.cards.length === 0) {
    return (
      <div
        data-testid="v2-review-page"
        className="flex flex-col h-full"
      >
        <div className="flex flex-wrap items-center justify-between gap-2 px-4 sm:px-6 h-12 border-b border-border-subtle shrink-0">
          <Link
            href="/app"
            className="flex items-center gap-1.5 text-sm text-text-secondary hover:text-text-primary transition"
          >
            <ArrowLeft className="w-4 h-4" />
            <span>Tổng quan</span>
          </Link>

          <div className="flex items-center gap-2">
            {courses.length > 0 && (
              <select
                data-testid="review-filter-course"
                value={selectedCourseId}
                onChange={(e) => {
                  setSelectedCourseId(e.target.value);
                  setSessionComplete(false);
                }}
                className="h-8 px-2.5 rounded-md border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                aria-label="Lọc theo khóa học"
              >
                <option value="">Tất cả khóa học</option>
                {courses.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.title}
                  </option>
                ))}
              </select>
            )}
            {availableTags.length > 0 && (
              <select
                data-testid="review-filter-tag"
                value={selectedTagId}
                onChange={(e) => {
                  setSelectedTagId(e.target.value);
                  setSessionComplete(false);
                }}
                className="h-8 px-2.5 rounded-md border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                aria-label="Lọc theo thẻ tag"
              >
                <option value="">Tất cả tag</option>
                {availableTags.map((t) => (
                  <option key={t.id} value={t.id}>
                    #{t.name}
                  </option>
                ))}
              </select>
            )}
          </div>
        </div>

        <div className="flex-1 flex items-center justify-center px-4">
          <div className="text-center max-w-md">
            {errorBanner && (
              <div
                data-testid="review-error-banner"
                className="mb-4 p-3 rounded-md border border-rose-300 dark:border-rose-800 bg-rose-50 dark:bg-rose-950/30 text-rose-700 dark:text-rose-300 text-xs flex items-center gap-2 text-left"
              >
                <AlertCircle className="w-4 h-4 shrink-0" />
                <span>{errorBanner}</span>
              </div>
            )}

            <div className="w-16 h-16 rounded-2xl bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600 dark:text-emerald-400 flex items-center justify-center mx-auto mb-4">
              {sessionComplete ? <Check className="w-8 h-8" /> : <Brain className="w-8 h-8" />}
            </div>
            <h2 className="text-xl font-bold text-text-primary mb-2">
              {sessionComplete ? "Hoàn thành phiên ôn tập!" : "Không có thẻ đến hạn ôn"}
            </h2>
            <p className="text-sm text-text-secondary mb-6">
              {sessionComplete
                ? `Bạn đã ôn xong ${reviewHistory.length || queue.cards.length} thẻ trong phiên này theo giới hạn hôm nay.${
                    totalBacklog > 0 ? ` (Còn ${totalBacklog} thẻ tồn trong kho).` : ""
                  }`
                : hasFilters
                ? "Không có thẻ nào khớp với bộ lọc hiện tại đang đến hạn."
                : queue.totalCards > 0
                ? `Hiện có ${queue.totalCards} thẻ trong kho nhưng chưa đến hạn. Bạn có thể ôn tập trước hạn bất cứ lúc nào.`
                : "Hãy bôi đen đoạn văn trong bài học để tạo thẻ điền từ hoặc thẻ hỏi đáp và bắt đầu ôn tập."}
            </p>
            <div className="flex flex-wrap items-center justify-center gap-3">
              {sessionComplete && (
                <button
                  type="button"
                  data-testid="review-more-20-btn"
                  onClick={() => {
                    setExtraSessionCap((prev) => prev + 20);
                    setSessionComplete(false);
                  }}
                  className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-[#5E51E8] hover:bg-[#4F46E5] text-white text-sm font-semibold shadow-xs transition cursor-pointer"
                >
                  <Plus className="w-4 h-4" />
                  Ôn thêm 20 thẻ
                </button>
              )}
              <Link
                href="/app"
                className="inline-flex items-center gap-2 h-9 px-4 rounded-md border border-border text-text-primary text-sm font-medium hover:bg-bg-secondary transition"
              >
                <ArrowLeft className="w-4 h-4" />
                Tổng quan
              </Link>
              {sessionComplete && reviewHistory.length > 0 && (
                <button
                  type="button"
                  data-testid="review-undo-last-btn"
                  onClick={() => void handleUndoLastReview()}
                  disabled={isSubmitting}
                  className="inline-flex items-center gap-2 h-9 px-4 rounded-md border border-border text-text-primary text-sm font-medium hover:bg-bg-secondary transition cursor-pointer disabled:opacity-50"
                >
                  <RotateCcw className="w-4 h-4" />
                  Hoàn tác thẻ cuối (Z)
                </button>
              )}
              {(queue.totalCards > 0 || sessionComplete || hasFilters) && (
                <button
                  type="button"
                  data-testid="review-practice-all-btn"
                  onClick={() => {
                    setPracticeAll(true);
                    setReviewHistory([]);
                    setSessionComplete(false);
                    void fetchReviewQueue(true);
                  }}
                  className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition cursor-pointer"
                >
                  <Zap className="w-4 h-4" />
                  {sessionComplete ? "Ôn lại hàng đợi" : `Ôn trước hạn (${queue.totalCards} thẻ)`}
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Active Review — Flashcard
  const cardStateLabel =
    currentCard?.state === "New"
      ? "Thẻ mới"
      : currentCard?.state === "Learning" || currentCard?.state === "Relearning"
      ? "Đang học"
      : "Ôn lại";

  const hasClozeSyntax = Boolean(currentCard?.frontText && /\{\{c\d+::.*?\}\}/.test(currentCard.frontText));
  const previews = currentCard?.intervalPreviews ?? {
    again: "< 1 phút",
    hard: "10 phút",
    good: "1 ngày",
    easy: "4 ngày",
  };

  return (
    <div data-testid="v2-review-page" className="h-full flex flex-col">
      <h1 className="sr-only">Trạm ôn tập thẻ ghi nhớ FSRS</h1>
      {/* Minimal header + filters */}
      <div className="flex flex-wrap items-center justify-between gap-2 px-4 sm:px-6 py-2.5 border-b border-border-subtle shrink-0">
        <button
          type="button"
          onClick={() => router.back()}
          className="min-h-[36px] min-w-[36px] px-2.5 py-1.5 rounded-md inline-flex items-center gap-1.5 text-sm text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition cursor-pointer"
        >
          <ArrowLeft className="w-4 h-4" />
          <span className="hidden sm:inline">Quay lại</span>
        </button>

        {/* Progress & filters */}
        <div className="flex flex-wrap items-center gap-3">
          {courses.length > 0 && (
            <select
              data-testid="review-filter-course"
              value={selectedCourseId}
              onChange={(e) => {
                setSelectedCourseId(e.target.value);
                setReviewHistory([]);
              }}
              className="h-7 px-2 rounded border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent max-w-[160px] truncate"
              aria-label="Lọc theo khóa học"
            >
              <option value="">Tất cả khóa học</option>
              {courses.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.title}
                </option>
              ))}
            </select>
          )}
          {availableTags.length > 0 && (
            <select
              data-testid="review-filter-tag"
              value={selectedTagId}
              onChange={(e) => {
                setSelectedTagId(e.target.value);
                setReviewHistory([]);
              }}
              className="h-7 px-2 rounded border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent max-w-[130px] truncate"
              aria-label="Lọc theo tag"
            >
              <option value="">Tất cả tag</option>
              {availableTags.map((t) => (
                <option key={t.id} value={t.id}>
                  #{t.name}
                </option>
              ))}
            </select>
          )}
          <button
            type="button"
            onClick={() => {
              const next = !practiceAll;
              setPracticeAll(next);
              setReviewHistory([]);
              void fetchReviewQueue(next);
            }}
            className={`h-7 px-2.5 rounded text-xs font-medium border transition cursor-pointer ${
              practiceAll
                ? "border-accent bg-accent-subtle text-accent"
                : "border-border bg-bg-secondary text-text-secondary hover:text-text-primary"
            }`}
            title="Bao gồm cả các thẻ chưa đến hạn"
          >
            {practiceAll ? "Đang ôn tất cả" : "Chỉ thẻ đến hạn"}
          </button>

          <div className="flex items-center gap-2">
            <span data-testid="review-progress-count" className="text-xs text-text-tertiary">
              {currentIndex + 1} / {queue.cards.length}
            </span>
            <div className="w-20 h-1.5 bg-bg-tertiary rounded-full overflow-hidden">
              <div
                className="h-full bg-accent rounded-full transition-all"
                style={{ width: `${((currentIndex + 1) / queue.cards.length) * 100}%` }}
              />
            </div>
          </div>
        </div>

        {/* Undo */}
        <button
          type="button"
          data-testid="review-undo-btn"
          onClick={() => void handleUndoLastReview()}
          disabled={reviewHistory.length === 0 || isSubmitting}
          className="min-h-[36px] min-w-[36px] px-2.5 py-1.5 rounded-md inline-flex items-center gap-1.5 text-xs text-text-tertiary hover:text-text-primary hover:bg-bg-secondary disabled:opacity-30 transition cursor-pointer"
          title="Hoàn tác đánh giá vừa rồi (Z)"
        >
          <RotateCcw className="w-3.5 h-3.5" />
          <span className="hidden sm:inline">Hoàn tác (Z)</span>
        </button>
      </div>

      {/* Banners */}
      {(errorBanner || undoBanner) && (
        <div className="px-4 sm:px-6 pt-3 max-w-2xl mx-auto w-full space-y-2">
          {errorBanner && (
            <div
              data-testid="review-error-banner"
              className="p-2.5 rounded-md border border-rose-300 dark:border-rose-800 bg-rose-50 dark:bg-rose-950/30 text-rose-700 dark:text-rose-300 text-xs flex items-center gap-2"
            >
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>{errorBanner}</span>
            </div>
          )}
          {undoBanner && (
            <div
              data-testid="review-undo-banner"
              className="p-2.5 rounded-md border border-emerald-300 dark:border-emerald-800 bg-emerald-50 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-300 text-xs flex items-center gap-2"
            >
              <RotateCcw className="w-3.5 h-3.5 shrink-0" />
              <span>{undoBanner}</span>
            </div>
          )}
        </div>
      )}

      {/* Card area — 2-Column FSRS Memory Workstation */}
      <div className="flex-1 max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        <div className="lg:col-span-8 w-full">
          {/* Card */}
          <div
            className="perspective-1200 cursor-pointer"
            onClick={() => !isFlipped && setIsFlipped(true)}
          >
            <div
              className={`relative preserve-3d transition-transform duration-500 ${
                isFlipped ? "rotate-y-180" : ""
              }`}
              style={{ minHeight: "340px" }}
            >
              {/* Front */}
              <div
                data-testid="review-card-front"
                className="absolute inset-0 backface-hidden rounded-2xl border border-border bg-bg-primary p-6 sm:p-8 flex flex-col shadow-xs"
              >
                <div className="flex items-center justify-between gap-2 mb-4 flex-wrap">
                  <div className="flex flex-wrap items-center gap-1.5">
                    <span className="inline-flex items-center gap-1 text-[11px] font-medium text-accent bg-accent-subtle rounded-md px-2 py-0.5">
                      <Clock className="w-3 h-3" />
                      {cardStateLabel} · Lặp {currentCard?.reps ?? 0}
                    </span>
                    {currentCard?.tags?.slice(0, 3).map((tag) => (
                      <span
                        key={tag.id}
                        className="text-[11px] font-medium text-text-tertiary bg-bg-tertiary rounded-md px-2 py-0.5"
                      >
                        #{tag.name}
                      </span>
                    ))}
                  </div>
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={(e) => {
                        e.stopPropagation();
                        if (typeof window === "undefined" || !("speechSynthesis" in window)) return;
                        window.speechSynthesis.cancel();
                        const text = formatCleanQuizPrompt(currentCard?.frontText || "", isFlipped);
                        const utter = new SpeechSynthesisUtterance(text);
                        utter.lang = /[àáạảãâăđêôơư]/i.test(text) ? "vi-VN" : "en-US";
                        window.speechSynthesis.speak(utter);
                      }}
                      title="Đọc to câu hỏi"
                      className="inline-flex items-center gap-1 px-2 py-1 rounded-md border border-border bg-bg-secondary text-[11px] font-medium text-text-secondary hover:text-accent transition cursor-pointer"
                    >
                      <Volume2 className="w-3.5 h-3.5" />
                      <span>Nghe</span>
                    </button>
                    {currentCard?.sourceTitleSnapshot && (
                      <span className="inline-flex items-center gap-1 text-[11px] text-text-tertiary truncate max-w-[220px]">
                        <BookOpen className="w-3 h-3 shrink-0" />
                        <span className="truncate">
                          {currentCard.sourceTitleSnapshot}
                          {currentCard.pageNumber ? ` (tr.${currentCard.pageNumber})` : ""}
                        </span>
                      </span>
                    )}
                  </div>
                </div>

                <div className="flex-1 flex items-center justify-center px-2">
                  <div className="text-lg sm:text-xl text-text-primary text-center leading-relaxed whitespace-pre-wrap">
                    {currentCard?.frontText
                      ? hasClozeSyntax
                        ? renderClozeText(currentCard.frontText, false)
                        : currentCard.frontText
                      : "Câu hỏi flashcard"}
                  </div>
                </div>

                <p className="text-xs text-text-tertiary text-center mt-4">
                  Nhấn <kbd className="px-1.5 py-0.5 rounded bg-bg-tertiary border border-border text-[10px]">Space</kbd> hoặc click để lật thẻ
                </p>
              </div>

              {/* Back */}
              <div
                data-testid="review-card-back"
                className="absolute inset-0 backface-hidden rotate-y-180 rounded-2xl border border-border bg-bg-primary p-6 sm:p-8 flex flex-col shadow-xs overflow-y-auto"
              >
                <div className="flex items-center justify-between text-[11px] text-text-tertiary mb-3">
                  <span>
                    Đáp án ·{" "}
                    {currentCard && currentCard.stability > 0
                      ? `Độ nhớ ${currentCard.stability.toFixed(1)} ngày`
                      : "Thẻ mới"}
                  </span>
                  {currentCard?.cardType && (
                    <span className="px-2 py-0.5 rounded bg-bg-secondary border border-border text-[10px] font-medium text-text-secondary">
                      {currentCard.cardType === "cloze"
                        ? "Điền khuyết"
                        : currentCard.cardType === "ai_generated"
                        ? "Gợi ý AI"
                        : "Thẻ cơ bản"}
                    </span>
                  )}
                </div>

                <div className="flex-1 flex flex-col items-center justify-center gap-3 px-2">
                  {hasClozeSyntax && currentCard?.frontText && (
                    <div className="text-base sm:text-lg text-text-primary text-center leading-relaxed whitespace-pre-wrap">
                      {renderClozeText(currentCard.frontText, true)}
                    </div>
                  )}
                  {currentCard?.backText && (
                    <p
                      className={`text-center leading-relaxed whitespace-pre-wrap ${
                        hasClozeSyntax
                          ? "text-sm text-text-secondary border-t border-border-subtle pt-3 w-full"
                          : "text-base sm:text-lg text-text-primary font-medium"
                      }`}
                    >
                      {currentCard.backText}
                    </p>
                  )}
                  {currentCard?.sourceQuoteSnapshot && (
                    <blockquote className="text-xs text-text-tertiary italic border-l-2 border-accent/40 pl-3 text-left max-w-md mt-1">
                      &ldquo;{currentCard.sourceQuoteSnapshot}&rdquo;
                    </blockquote>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Rating buttons — only visible when flipped */}
          {isFlipped ? (
            <div className="mt-6 grid grid-cols-2 sm:grid-cols-4 gap-2.5">
              {([
                {
                  key: "again" as const,
                  label: "Quên",
                  interval: previews.again,
                  hotkey: "1",
                  color:
                    "hover:bg-rose-50 dark:hover:bg-rose-950/30 hover:border-rose-300 dark:hover:border-rose-800 text-rose-600 dark:text-rose-400",
                },
                {
                  key: "hard" as const,
                  label: "Khó",
                  interval: previews.hard,
                  hotkey: "2",
                  color:
                    "hover:bg-amber-50 dark:hover:bg-amber-950/30 hover:border-amber-300 dark:hover:border-amber-800 text-amber-600 dark:text-amber-400",
                },
                {
                  key: "good" as const,
                  label: "Tốt",
                  interval: previews.good,
                  hotkey: "3",
                  color:
                    "hover:bg-emerald-50 dark:hover:bg-emerald-950/30 hover:border-emerald-300 dark:hover:border-emerald-800 text-emerald-600 dark:text-emerald-400",
                },
                {
                  key: "easy" as const,
                  label: "Dễ",
                  interval: previews.easy,
                  hotkey: "4",
                  color:
                    "hover:bg-sky-50 dark:hover:bg-sky-950/30 hover:border-sky-300 dark:hover:border-sky-800 text-sky-600 dark:text-sky-400",
                },
              ]).map((btn, idx) => (
                <button
                  key={btn.key}
                  type="button"
                  data-testid={`review-rate-${idx + 1}`}
                  disabled={isSubmitting}
                  onClick={() => void handleRate(btn.key)}
                  className={`h-16 rounded-xl border border-border bg-bg-primary text-sm font-medium transition cursor-pointer flex flex-col items-center justify-center gap-0.5 disabled:opacity-50 ${btn.color}`}
                >
                  <span>{btn.label}</span>
                  <span className="text-[11px] font-normal opacity-80">{btn.interval}</span>
                  <span className="text-[10px] opacity-50">Phím {btn.hotkey}</span>
                </button>
              ))}
            </div>
          ) : (
            <div className="mt-6 flex justify-center">
              <button
                type="button"
                data-testid="review-flip-btn"
                onClick={() => setIsFlipped(true)}
                className="h-10 px-8 rounded-xl bg-accent text-white text-sm font-semibold hover:bg-accent-hover transition cursor-pointer shadow-xs"
              >
                Hiện đáp án (Space)
              </button>
            </div>
          )}
        </div>

        {/* Right Companion Column: FSRS Stats, Upcoming Queue & Shortcuts */}
        <aside className="lg:col-span-4 space-y-4">
          <div className="rounded-xl border border-border bg-bg-secondary p-4 space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold uppercase tracking-wider text-text-tertiary flex items-center gap-1.5">
                <Sparkles className="w-3.5 h-3.5 text-accent" />
                <span>Chỉ số FSRS thẻ hiện tại</span>
              </span>
              <span className="text-[11px] font-semibold px-2 py-0.5 rounded bg-accent-subtle text-accent">
                {cardStateLabel}
              </span>
            </div>

            <div className="grid grid-cols-3 gap-2 text-center">
              <div className="rounded-lg bg-bg-primary border border-border p-2.5">
                <p className="text-[10px] text-text-tertiary">Độ bền</p>
                <p className="text-sm font-bold text-text-primary">
                  {currentCard && currentCard.stability > 0
                    ? `${currentCard.stability.toFixed(1)}d`
                    : "Mới"}
                </p>
              </div>
              <div className="rounded-lg bg-bg-primary border border-border p-2.5">
                <p className="text-[10px] text-text-tertiary">Độ khó</p>
                <p className="text-sm font-bold text-text-primary">
                  {currentCard ? currentCard.difficulty.toFixed(1) : "0.0"}
                </p>
              </div>
              <div className="rounded-lg bg-bg-primary border border-border p-2.5">
                <p className="text-[10px] text-text-tertiary">Số lần lặp</p>
                <p className="text-sm font-bold text-text-primary">
                  {currentCard?.reps ?? 0}
                </p>
              </div>
            </div>

            <div className="pt-2 border-t border-border/70 flex items-center justify-between text-xs text-text-secondary">
              <span>Phiên hôm nay: {currentIndex + 1}/{queue.cards.length}</span>
              <span>{totalBacklog > 0 ? `+${totalBacklog} thẻ tồn` : "Hàng đợi tối ưu"}</span>
            </div>
          </div>

          {/* Upcoming Cards Preview */}
          <div className="rounded-xl border border-border bg-bg-secondary p-4 space-y-2.5">
            <div className="flex items-center justify-between text-xs font-semibold text-text-primary">
              <span className="inline-flex items-center gap-1.5">
                <Layers className="w-3.5 h-3.5 text-accent" />
                <span>Thẻ sắp tới trong hàng đợi</span>
              </span>
              <span className="text-[11px] text-text-tertiary">
                Còn {Math.max(0, queue.cards.length - currentIndex - 1)} thẻ
              </span>
            </div>

            {queue.cards.slice(currentIndex + 1, currentIndex + 4).length > 0 ? (
              <div className="space-y-1.5">
                {queue.cards.slice(currentIndex + 1, currentIndex + 4).map((upcoming, uIdx) => (
                  <div
                    key={upcoming.id}
                    className="rounded-lg border border-border bg-bg-primary px-3 py-2 text-xs text-text-secondary"
                  >
                    <p className="line-clamp-2 leading-relaxed">
                      <span className="font-semibold text-accent mr-1.5">#{currentIndex + uIdx + 2}</span>
                      {formatCleanQuizPrompt(upcoming.frontText || "Thẻ ghi nhớ", false)}
                    </p>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-xs text-text-tertiary">Đây là thẻ cuối cùng trong phiên ôn tập này.</p>
            )}
          </div>

          {/* Keyboard Shortcuts */}
          <div className="rounded-xl border border-border bg-bg-secondary p-4 space-y-2">
            <div className="flex items-center gap-1.5 text-xs font-semibold text-text-primary">
              <Keyboard className="w-3.5 h-3.5 text-accent" />
              <span>Phím tắt tốc độ cao</span>
            </div>
            <div className="grid grid-cols-2 gap-2 text-[11px] text-text-secondary">
              <div className="flex items-center justify-between bg-bg-primary border border-border rounded px-2.5 py-1.5">
                <span>Lật thẻ</span>
                <kbd className="font-mono text-[10px] px-1.5 py-0.5 rounded bg-bg-tertiary border border-border">Space</kbd>
              </div>
              <div className="flex items-center justify-between bg-bg-primary border border-border rounded px-2.5 py-1.5">
                <span>Đánh giá</span>
                <kbd className="font-mono text-[10px] px-1.5 py-0.5 rounded bg-bg-tertiary border border-border">1 - 4</kbd>
              </div>
              <div className="flex items-center justify-between bg-bg-primary border border-border rounded px-2.5 py-1.5 col-span-2">
                <span>Hoàn tác đánh giá vừa rồi</span>
                <kbd className="font-mono text-[10px] px-1.5 py-0.5 rounded bg-bg-tertiary border border-border">Z</kbd>
              </div>
            </div>
          </div>
        </aside>
      </div>
    </div>
  );
}

export default function ReviewPage() {
  return (
    <Suspense
      fallback={
        <div className="flex items-center justify-center h-full">
          <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
        </div>
      }
    >
      <ReviewPageContent />
    </Suspense>
  );
}
