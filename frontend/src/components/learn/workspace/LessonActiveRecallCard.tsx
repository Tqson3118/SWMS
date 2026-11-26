"use client";

import React, { useEffect, useMemo, useState } from "react";
import {
  AlertCircle,
  BookmarkPlus,
  Brain,
  CheckCircle2,
  ChevronDown,
  ChevronUp,
  Loader2,
  Send,
  Sparkles,
} from "lucide-react";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import {
  evaluatePracticeWithAiOrOffline,
  type AiPracticeEvaluationResult,
} from "@/lib/writein-practice";
import { saveWriteInFlashcardDeduplicated } from "@/components/learn/practice/WriteInFsrsHelper";

export interface LessonActiveRecallCardProps {
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  keyTerms?: string[];
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
}

/**
 * Extracts concise key terms from lesson markdown (headings, bold phrases, inline code)
 * when explicit `keyTerms` are not provided.
 */
function deriveKeywordsFromMarkdown(
  lessonMarkdown: string,
  lessonTitle: string,
  explicitTerms?: string[]
): string[] {
  const cleanedExplicit = (explicitTerms ?? [])
    .map((t) => t.trim())
    .filter((t) => t.length >= 2);
  if (cleanedExplicit.length > 0) {
    return Array.from(new Set(cleanedExplicit)).slice(0, 8);
  }

  const candidates: string[] = [];
  const seenLower = new Set<string>();
  const addCandidate = (raw: string) => {
    const cleaned = raw
      .replace(/[*_`#[\]()]/g, " ")
      .replace(/^\d+[.)]\s*/, "")
      .replace(/\s+/g, " ")
      .trim();
    if (cleaned.length < 3 || cleaned.length > 48) return;
    const key = cleaned.toLowerCase();
    if (seenLower.has(key)) return;
    seenLower.add(key);
    candidates.push(cleaned);
  };

  if (lessonMarkdown) {
    const headingRegex = /^#{2,3}\s+(.+)$/gm;
    let match: RegExpExecArray | null;
    while ((match = headingRegex.exec(lessonMarkdown)) !== null) {
      addCandidate(match[1]);
      if (candidates.length >= 5) break;
    }

    const boldRegex = /\*\*([^*]{3,42})\*\*/g;
    while ((match = boldRegex.exec(lessonMarkdown)) !== null) {
      addCandidate(match[1]);
      if (candidates.length >= 8) break;
    }

    if (candidates.length < 4) {
      const codeRegex = /`([^`\n]{2,32})`/g;
      while ((match = codeRegex.exec(lessonMarkdown)) !== null) {
        addCandidate(match[1]);
        if (candidates.length >= 8) break;
      }
    }
  }

  if (candidates.length === 0 && lessonTitle) {
    lessonTitle
      .split(/[:\-–—,|]/)
      .map((part) => part.trim())
      .filter((part) => part.length >= 3)
      .forEach(addCandidate);
  }

  return candidates.slice(0, 8);
}

/**
 * Extracts the first meaningful summary paragraph or bullet block from lesson markdown
 * to serve as a reference answer for Active Recall grading and FSRS card generation.
 */
function extractKeySummaryFromMarkdown(lessonMarkdown: string, lessonTitle: string): string {
  if (!lessonMarkdown || !lessonMarkdown.trim()) {
    return `Tóm tắt trọng tâm bài "${lessonTitle}".`;
  }

  const stripped = lessonMarkdown
    .replace(/```[\s\S]*?```/g, " ")
    .replace(/!\[[^\]]*\]\([^)]*\)/g, " ");

  const blocks = stripped
    .split(/\r?\n\r?\n/)
    .map((b) => b.trim())
    .filter((b) => {
      if (!b) return false;
      if (/^#{1,6}\s/.test(b)) return false;
      if (/^\|.*\|/.test(b)) return false;
      if (/^[-*_]{3,}$/.test(b)) return false;
      return b.replace(/[#>*`\-_\s]/g, "").length >= 25;
    });

  if (blocks.length > 0) {
    const first = blocks[0]
      .replace(/\[([^\]]+)\]\([^)]+\)/g, "$1")
      .replace(/[*_`>#]/g, "")
      .replace(/\s+/g, " ")
      .trim();
    return first.slice(0, 420);
  }

  return `Tóm tắt trọng tâm bài "${lessonTitle}".`;
}

export function LessonActiveRecallCard({
  lessonId,
  lessonTitle,
  lessonMarkdown,
  keyTerms,
  onCreateFlashcardQuick,
}: LessonActiveRecallCardProps) {
  const [isExpanded, setIsExpanded] = useState(true);
  const [userResponse, setUserResponse] = useState("");
  const [isEvaluating, setIsEvaluating] = useState(false);
  const [result, setResult] = useState<AiPracticeEvaluationResult | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [fsrsStatus, setFsrsStatus] = useState<"idle" | "saving" | "saved" | "deduped" | "error">(
    "idle"
  );

  // Reset recall state when navigating to a different lesson
  useEffect(() => {
    setUserResponse("");
    setResult(null);
    setErrorMsg(null);
    setFsrsStatus("idle");
  }, [lessonId, lessonTitle]);

  const expectedKeywords = useMemo(
    () => deriveKeywordsFromMarkdown(lessonMarkdown, lessonTitle, keyTerms),
    [lessonMarkdown, lessonTitle, keyTerms]
  );

  const sampleAnswer = useMemo(
    () => extractKeySummaryFromMarkdown(lessonMarkdown, lessonTitle),
    [lessonMarkdown, lessonTitle]
  );

  const handleSubmit = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    const trimmed = userResponse.trim();
    if (!trimmed) {
      setErrorMsg("Hãy tự viết 2–3 ý chính bạn nhớ được trước khi kiểm tra.");
      return;
    }
    if (isEvaluating) return;

    setIsEvaluating(true);
    setErrorMsg(null);
    setFsrsStatus("idle");

    try {
      const evaluation = await evaluatePracticeWithAiOrOffline({
        mode: "active_recall",
        lang: "vi-VN",
        prompt: lessonTitle,
        expectedKeywords,
        sampleAnswer,
        userResponse: trimmed,
        contextTitle: lessonTitle,
        minWords: 8,
        headers: getClientRequestHeaders(),
      });
      setResult(evaluation);
    } catch {
      setErrorMsg("Không thể đánh giá lúc này. Vui lòng thử lại sau.");
    } finally {
      setIsEvaluating(false);
    }
  };

  const handleSaveWeaknessToFsrs = async () => {
    if (!result || fsrsStatus === "saving" || fsrsStatus === "saved" || fsrsStatus === "deduped") {
      return;
    }

    setFsrsStatus("saving");
    const missingList = result.missingKeywords.slice(0, 5);
    const front =
      missingList.length > 0
        ? `[Gợi nhớ · ${lessonTitle}] Giải thích các khái niệm trọng tâm còn sót: ${missingList.join(", ")}`
        : `[Gợi nhớ · ${lessonTitle}] Nêu 2–3 ý cốt lõi quan trọng nhất của bài học này?`;

    const backParts: string[] = [];
    if (result.correctedVersion) {
      backParts.push(`Tóm tắt chuẩn:\n${result.correctedVersion}`);
    } else if (sampleAnswer) {
      backParts.push(`Ý chính:\n${sampleAnswer}`);
    }
    if (missingList.length > 0) {
      backParts.push(`Từ khóa cần nhớ: ${missingList.join(", ")}`);
    }
    if (result.improvements.length > 0) {
      backParts.push(`Lưu ý bổ sung:\n- ${result.improvements.join("\n- ")}`);
    }

    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back: backParts.join("\n\n") || sampleAnswer,
      lessonId: lessonId ?? null,
      sourceTitle: lessonTitle,
      tags: ["active-recall"],
      onCreateFlashcardQuick,
    });

    if (res.ok) {
      setFsrsStatus(res.deduplicated ? "deduped" : "saved");
    } else {
      setFsrsStatus("error");
    }
  };

  return (
    <section
      data-testid="lesson-active-recall-card"
      className="mt-10 rounded-2xl border border-violet-200/80 dark:border-violet-800/60 bg-[var(--surface-card)] bg-gradient-to-br from-violet-50/50 via-[var(--surface-card)] to-indigo-50/40 dark:from-violet-950/30 dark:via-[var(--surface-card)] dark:to-indigo-950/20 p-5 sm:p-6 shadow-xs font-sans"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="space-y-1 min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="inline-flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-violet-700 dark:text-violet-300">
              <Brain className="w-4 h-4 shrink-0" />
              <span>Gợi nhớ chủ động (Active Recall)</span>
            </span>
            <span className="text-[11px] font-medium px-2 py-0.5 rounded-full bg-[var(--surface-elevated)] text-text-secondary border border-[var(--border-calm)]">
              Không bắt buộc · Giúp nhớ lâu x2
            </span>
          </div>
          <p className="text-xs sm:text-sm text-text-secondary leading-relaxed">
            Không nhìn lại ghi chú — hãy thử tự diễn đạt <strong>2–3 ý cốt lõi</strong> bạn vừa học từ bài{" "}
            <span className="font-medium text-text-primary">&ldquo;{lessonTitle}&rdquo;</span>.
          </p>
        </div>

        <button
          type="button"
          onClick={() => setIsExpanded((v) => !v)}
          className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs font-medium text-text-secondary hover:text-text-primary transition cursor-pointer shrink-0"
          aria-expanded={isExpanded}
        >
          {isExpanded ? (
            <>
              <span>Thu gọn</span>
              <ChevronUp className="w-3.5 h-3.5" />
            </>
          ) : (
            <>
              <span>Mở rộng</span>
              <ChevronDown className="w-3.5 h-3.5" />
            </>
          )}
        </button>
      </div>

      {isExpanded && (
        <form onSubmit={handleSubmit} className="mt-4 space-y-3.5">
          <textarea
            data-testid="active-recall-input"
            value={userResponse}
            onChange={(e) => {
              setUserResponse(e.target.value);
              if (errorMsg) setErrorMsg(null);
            }}
            rows={3}
            placeholder="Ví dụ: 1) Khái niệm cốt lõi là... 2) Cơ chế hoạt động gồm... 3) Lưu ý quan trọng khi áp dụng..."
            className="w-full rounded-xl border border-border bg-bg-primary px-3.5 py-2.5 text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-[#5E51E8] dark:focus:border-[#818CF8] resize-y min-h-[88px]"
          />

          {errorMsg && (
            <div className="flex items-center gap-1.5 text-xs text-rose-600 dark:text-rose-400">
              <AlertCircle className="w-3.5 h-3.5 shrink-0" />
              <span>{errorMsg}</span>
            </div>
          )}

          <div className="flex flex-wrap items-center justify-between gap-3">
            <span className="text-[11px] text-text-tertiary">
              {expectedKeywords.length > 0
                ? `Hệ thống đối chiếu theo ${expectedKeywords.length} khái niệm trọng tâm của bài.`
                : "Hệ thống sẽ đánh giá độ mạch lạc và ý chính bạn tự nhớ lại."}
            </span>

            <button
              type="submit"
              data-testid="active-recall-submit-btn"
              disabled={isEvaluating || !userResponse.trim()}
              className="inline-flex items-center justify-center gap-1.5 min-h-9 px-4 py-2 rounded-xl bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:hover:bg-[#A5B4FC] dark:text-[#0F0F12] text-xs font-semibold shadow-xs disabled:opacity-50 transition cursor-pointer"
            >
              {isEvaluating ? (
                <>
                  <Loader2 className="w-3.5 h-3.5 animate-spin" />
                  <span>Đang chấm gợi nhớ...</span>
                </>
              ) : (
                <>
                  <Send className="w-3.5 h-3.5" />
                  <span>Kiểm tra mức độ nhớ</span>
                </>
              )}
            </button>
          </div>

          {result && (
            <div className="mt-4 pt-4 border-t border-violet-200/70 dark:border-violet-800/40 space-y-3">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div className="flex flex-wrap items-center gap-2">
                  <span
                    className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold tabular-nums ${
                      result.score >= 75
                        ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border border-emerald-500/30"
                        : result.score >= 50
                        ? "bg-amber-500/15 text-amber-700 dark:text-amber-300 border border-amber-500/30"
                        : "bg-rose-500/15 text-rose-700 dark:text-rose-300 border border-rose-500/30"
                    }`}
                  >
                    Điểm gợi nhớ: {result.score}/100
                  </span>

                  <span
                    data-testid="active-recall-grading-badge"
                    className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-semibold border ${
                      result.usedLiveEndpoint
                        ? "bg-indigo-500/10 text-indigo-700 dark:text-indigo-300 border-indigo-500/30"
                        : "bg-[var(--surface-elevated)] text-text-secondary border-[var(--border-calm)]"
                    }`}
                  >
                    <Sparkles className="w-3 h-3 shrink-0" />
                    <span>{result.gradingLabelVi}</span>
                  </span>
                </div>

                <button
                  type="button"
                  data-testid="active-recall-save-fsrs-btn"
                  disabled={
                    fsrsStatus === "saving" || fsrsStatus === "saved" || fsrsStatus === "deduped"
                  }
                  onClick={() => void handleSaveWeaknessToFsrs()}
                  className={`inline-flex items-center gap-1.5 min-h-8 px-3 py-1.5 rounded-lg border text-xs font-semibold transition ${
                    fsrsStatus === "saved" || fsrsStatus === "deduped"
                      ? "bg-emerald-500/15 text-emerald-700 border-emerald-500/35 dark:text-emerald-300 cursor-default"
                      : "border-violet-300 dark:border-violet-700 bg-violet-500/10 hover:bg-violet-500/20 text-violet-700 dark:text-violet-300 disabled:opacity-60 cursor-pointer"
                  }`}
                >
                  {fsrsStatus === "saving" ? (
                    <>
                      <Loader2 className="w-3.5 h-3.5 animate-spin" />
                      <span>Đang lưu...</span>
                    </>
                  ) : fsrsStatus === "saved" ? (
                    <>
                      <CheckCircle2 className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400" />
                      <span>Đã lưu vào FSRS ✓</span>
                    </>
                  ) : fsrsStatus === "deduped" ? (
                    <>
                      <CheckCircle2 className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400" />
                      <span>Thẻ đã có trong FSRS ✓</span>
                    </>
                  ) : (
                    <>
                      <BookmarkPlus className="w-3.5 h-3.5" />
                      <span>Lưu điểm yếu thành Thẻ FSRS</span>
                    </>
                  )}
                </button>
              </div>

              <p className="text-xs sm:text-sm text-text-primary leading-relaxed bg-bg-primary/70 rounded-xl border border-border px-3.5 py-2.5">
                {result.feedbackVi}
              </p>

              {(result.matchedKeywords.length > 0 || result.missingKeywords.length > 0) && (
                <div className="space-y-2">
                  {result.matchedKeywords.length > 0 && (
                    <div className="flex flex-wrap items-center gap-1.5">
                      <span className="text-[11px] font-semibold text-emerald-700 dark:text-emerald-300 mr-1">
                        Đã nhớ:
                      </span>
                      {result.matchedKeywords.map((kw) => (
                        <span
                          key={`matched-${kw}`}
                          className="inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-medium bg-emerald-500/10 text-emerald-700 dark:text-emerald-300 border border-emerald-500/25"
                        >
                          ✓ {kw}
                        </span>
                      ))}
                    </div>
                  )}

                  {result.missingKeywords.length > 0 && (
                    <div className="flex flex-wrap items-center gap-1.5">
                      <span className="text-[11px] font-semibold text-amber-700 dark:text-amber-300 mr-1">
                        Cần bổ sung:
                      </span>
                      {result.missingKeywords.map((kw) => (
                        <span
                          key={`missing-${kw}`}
                          className="inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-medium bg-amber-500/10 text-amber-800 dark:text-amber-300 border border-amber-500/25"
                        >
                          + {kw}
                        </span>
                      ))}
                    </div>
                  )}
                </div>
              )}

              {result.correctedVersion && (
                <div className="rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 px-3.5 py-2.5 text-xs text-text-secondary space-y-1">
                  <p className="font-semibold text-text-primary">Ý chính gợi ý đối chiếu:</p>
                  <p className="leading-relaxed">{result.correctedVersion}</p>
                </div>
              )}

              {fsrsStatus === "error" && (
                <p className="text-xs text-rose-600 dark:text-rose-400">
                  Không thể lưu thẻ FSRS lúc này. Vui lòng thử lại.
                </p>
              )}
            </div>
          )}
        </form>
      )}
    </section>
  );
}
