"use client";

import React, { useEffect, useRef, useState } from "react";
import {
  AlertCircle,
  Check,
  CheckCircle2,
  Eye,
  EyeOff,
  Lightbulb,
  Loader2,
  Mic,
  MicOff,
  RotateCcw,
  Send,
  Sliders,
  Sparkles,
  Volume2,
  XCircle,
} from "lucide-react";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import {
  DIFFICULTY_LABELS,
  GRADING_LABELS,
  evaluateClozeAnswers,
  evaluateGrammarFixAnswer,
  evaluateRoleplayOffline,
  evaluateTranslationAnswer,
  parseClozeTemplate,
  type ClozeEvaluationResult,
  type ClozePracticeItem,
  type GrammarFixEvaluationResult,
  type GrammarFixPracticeItem,
  type LanguageCode,
  type RoleplayEvaluationResult,
  type RoleplayPracticeItem,
  type TranslatePracticeItem,
  type TranslationEvaluationResult,
  type VietnamesePronunciationTip,
} from "@/lib/language-practice";

export interface AutoSaveFsrsCandidate {
  front: string;
  back: string;
  ipa?: string;
  example?: string;
}

// ============================================================================
// SHARED: DIFFICULTY BADGE & VIETNAMESE PRONUNCIATION TIPS CARD
// ============================================================================

export function DifficultyBadge({ difficulty }: { difficulty: "easy" | "medium" | "hard" }) {
  const cls =
    difficulty === "easy"
      ? "bg-emerald-500/12 text-emerald-700 dark:text-emerald-300 border-emerald-500/30"
      : difficulty === "medium"
      ? "bg-amber-500/12 text-amber-800 dark:text-amber-300 border-amber-500/30"
      : "bg-rose-500/12 text-rose-700 dark:text-rose-300 border-rose-500/30";

  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-[11px] font-bold border ${cls}`}>
      {DIFFICULTY_LABELS[difficulty]}
    </span>
  );
}

export function VietnamesePronunciationTipsCard({
  tips,
  onSpeakSlowWord,
}: {
  tips: VietnamesePronunciationTip[];
  onSpeakSlowWord: (word: string) => void;
}) {
  if (!tips || tips.length === 0) return null;

  return (
    <div
      data-testid="vietnamese-pronunciation-tips"
      className="p-4 rounded-xl bg-amber-500/10 border border-amber-500/30 space-y-3 animate-fadeIn"
    >
      <div className="flex items-center gap-2 text-amber-900 dark:text-amber-200 font-bold text-xs sm:text-sm">
        <Lightbulb className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0" />
        <span>Gợi ý lỗi phát âm hay gặp của người Việt</span>
      </div>

      <div className="space-y-2.5">
        {tips.map((tip) => (
          <div
            key={tip.id}
            className="p-3 rounded-lg bg-[var(--surface-card,#ffffff)]/90 dark:bg-zinc-900/80 border border-amber-500/20 space-y-1.5"
          >
            <div className="font-bold text-xs text-[var(--text-primary,#18181b)] dark:text-zinc-100">
              {tip.title}
            </div>
            <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-300 leading-relaxed">
              {tip.adviceVi}
            </p>
            {tip.exampleWords.length > 0 && (
              <div className="flex flex-wrap items-center gap-1.5 pt-1">
                <span className="text-[11px] font-semibold text-[var(--text-secondary,#71717a)] dark:text-zinc-400">
                  Nghe chậm từ cần chú ý:
                </span>
                {tip.exampleWords.map((word) => (
                  <button
                    key={word}
                    type="button"
                    onClick={() => onSpeakSlowWord(word)}
                    className="inline-flex items-center gap-1 px-2 py-1 rounded-md text-[11px] font-semibold bg-amber-500/15 text-amber-900 dark:text-amber-200 hover:bg-amber-500/25 transition cursor-pointer"
                    title={`Nghe đọc chậm từ "${word}"`}
                  >
                    <Volume2 className="w-3 h-3 shrink-0" />
                    <span>{word}</span>
                  </button>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

// ============================================================================
// TAB 5: CLOZE (ĐIỀN CHỖ TRỐNG TỪ TRÍ NHỚ)
// ============================================================================

export function ClozePracticeView({
  item,
  indexLabel,
  onSpeak,
  onRecordAttempt,
  onAutoSaveMistakes,
}: {
  item: ClozePracticeItem;
  indexLabel: string;
  onSpeak: (text: string, rate?: number) => void;
  onRecordAttempt: (params: {
    mode: "cloze";
    score: number;
    isPassed: boolean;
    activityKey: string;
    activityTitle: string;
    activityId?: string;
    details: Record<string, unknown>;
  }) => Promise<void>;
  onAutoSaveMistakes: (items: AutoSaveFsrsCandidate[]) => Promise<void>;
}) {
  const [inputs, setInputs] = useState<string[]>(() => item.blanks.map(() => ""));
  const [showHints, setShowHints] = useState(false);
  const [result, setResult] = useState<ClozeEvaluationResult | null>(null);

  useEffect(() => {
    setInputs(item.blanks.map(() => ""));
    setShowHints(false);
    setResult(null);
  }, [item.id, item.blanks.length]);

  const segments = parseClozeTemplate(item.template, item.blanks.length);

  const handleCheck = () => {
    const evalRes = evaluateClozeAnswers(item, inputs);
    setResult(evalRes);

    const passing = item.passingScore ?? 70;
    const isPassed = evalRes.score >= passing;

    void onRecordAttempt({
      mode: "cloze",
      score: evalRes.score,
      isPassed,
      activityKey: `cloze-${item.id}`,
      activityTitle: item.title,
      activityId: item.sourceActivityId,
      details: {
        itemId: item.id,
        inputs,
        correctCount: evalRes.correctCount,
        totalBlanks: evalRes.totalBlanks,
      },
    });

    if (!isPassed) {
      const failedBlanks = evalRes.blanks.filter((b) => !b.isCorrect);
      const candidates: AutoSaveFsrsCandidate[] = failedBlanks.slice(0, 3).map((b) => ({
        front: b.acceptedAnswers[0] || item.title,
        back: `Điền chỗ trống đúng: "${b.acceptedAnswers[0]}"\nCâu đầy đủ: ${item.fullSentence || item.template}\nNghĩa: ${item.meaningVi}${b.explanationVi ? `\nGhi chú: ${b.explanationVi}` : ""}`,
        example: item.fullSentence,
      }));
      if (candidates.length > 0) {
        void onAutoSaveMistakes(candidates);
      }
    }
  };

  const handleReset = () => {
    setInputs(item.blanks.map(() => ""));
    setResult(null);
  };

  return (
    <div
      data-testid="practice-panel-cloze"
      className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 shadow-sm p-5 sm:p-7 space-y-5"
    >
      <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
        <div className="space-y-1">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-xs font-semibold text-[var(--text-secondary,#71717a)] dark:text-zinc-400 uppercase tracking-wider">
              Điền Chỗ Trống (Cloze) · {indexLabel}
            </span>
            <DifficultyBadge difficulty={item.difficulty} />
          </div>
          <h3 className="text-base sm:text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
            {item.title}
          </h3>
          <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400">
            {item.promptVi}
          </p>
        </div>

        <button
          type="button"
          onClick={() => setShowHints((v) => !v)}
          className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl text-xs font-semibold min-h-[40px] bg-amber-500/10 text-amber-800 dark:text-amber-200 border border-amber-500/30 hover:bg-amber-500/20 transition cursor-pointer"
        >
          <Lightbulb className="w-3.5 h-3.5 text-amber-600 dark:text-amber-400" />
          <span>{showHints ? "Ẩn gợi ý" : "Gợi ý nghĩa / chữ cái đầu"}</span>
        </button>
      </div>

      {/* Inline blank inputs */}
      <div className="p-4 sm:p-5 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/70 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 leading-loose text-base sm:text-lg font-medium text-[var(--text-primary,#18181b)] dark:text-zinc-100">
        {segments.map((seg, idx) => {
          if (seg.type === "text") {
            return <span key={`txt-${idx}`}>{seg.text}</span>;
          }
          const bIdx = seg.blankIndex ?? 0;
          const blankResult = result?.blanks[bIdx];
          const spec = item.blanks[bIdx];
          const firstLetter = spec?.acceptedAnswers[0]?.[0] ?? "";
          const placeholder = showHints
            ? `${firstLetter}___`
            : `Ô #${bIdx + 1}`;

          const inputStateClass = blankResult
            ? blankResult.isCorrect
              ? "border-emerald-500 bg-emerald-500/10 text-emerald-800 dark:text-emerald-200"
              : "border-rose-500 bg-rose-500/10 text-rose-800 dark:text-rose-200"
            : "border-[#5E51E8]/50 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-[var(--text-primary,#18181b)] dark:text-zinc-100";

          return (
            <input
              key={`blank-${bIdx}`}
              type="text"
              data-testid={`cloze-input-${bIdx}`}
              value={inputs[bIdx] ?? ""}
              onChange={(e) => {
                const next = [...inputs];
                next[bIdx] = e.target.value;
                setInputs(next);
                setResult(null);
              }}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  handleCheck();
                }
              }}
              placeholder={placeholder}
              aria-label={`Ô trống số ${bIdx + 1}`}
              className={`inline-block mx-1.5 my-1 px-3 py-1 rounded-lg border-2 text-sm sm:text-base font-semibold min-w-[115px] max-w-[200px] focus:outline-none focus:ring-2 focus:ring-[#5E51E8] transition ${inputStateClass}`}
            />
          );
        })}
      </div>

      {/* Meaning & Hints box */}
      {showHints && (
        <div className="p-3.5 rounded-xl bg-amber-500/10 border border-amber-500/30 text-xs text-amber-900 dark:text-amber-200 space-y-2 animate-fadeIn">
          {item.meaningVi && (
            <p>
              <strong>Nghĩa cả câu:</strong> {item.meaningVi}
            </p>
          )}
          <div className="space-y-1">
            {item.blanks.map((b, i) => {
              const first = b.acceptedAnswers[0] ?? "";
              return (
                <div key={i} className="flex flex-wrap items-center gap-1.5">
                  <span className="font-bold">Ô #{i + 1}:</span>
                  <span className="font-mono px-1.5 py-0.5 rounded bg-amber-500/20">
                    {first ? `${first[0]}${"_".repeat(Math.max(1, first.length - 1))}` : "___"}
                  </span>
                  {b.hintVi && <span>— {b.hintVi}</span>}
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Action buttons */}
      <div className="flex flex-wrap items-center gap-2.5">
        <button
          type="button"
          data-testid="cloze-check-btn"
          onClick={handleCheck}
          className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold text-sm min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs cursor-pointer"
        >
          <CheckCircle2 className="w-4 h-4" />
          <span>Kiểm tra đáp án</span>
        </button>

        <button
          type="button"
          onClick={handleReset}
          className="inline-flex items-center gap-1.5 px-4 py-2.5 rounded-xl text-xs font-semibold min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-secondary,#52525b)] dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-700 cursor-pointer"
        >
          <RotateCcw className="w-3.5 h-3.5" />
          <span>Làm lại</span>
        </button>

        {result && item.fullSentence && (
          <button
            type="button"
            onClick={() => onSpeak(item.fullSentence!, 1.0)}
            className="inline-flex items-center gap-1.5 px-3.5 py-2.5 rounded-xl text-xs font-semibold min-h-[44px] bg-indigo-500/10 text-[#5E51E8] dark:text-[#818CF8] hover:bg-indigo-500/20 cursor-pointer ml-auto"
          >
            <Volume2 className="w-4 h-4" />
            <span>Nghe câu hoàn chỉnh</span>
          </button>
        )}
      </div>

      {/* Evaluation results */}
      {result && (
        <div
          data-testid="cloze-result-card"
          className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-3 animate-fadeIn"
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-2 font-bold text-sm">
              {result.isAllCorrect ? (
                <CheckCircle2 className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
              ) : (
                <AlertCircle className="w-5 h-5 text-amber-600 dark:text-amber-400" />
              )}
              <span className="text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                Đúng {result.correctCount}/{result.totalBlanks} ô trống
              </span>
            </div>
            <span
              className={`px-3 py-1 rounded-full text-xs font-extrabold ${
                result.score >= 70
                  ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                  : "bg-rose-500/15 text-rose-700 dark:text-rose-300"
              }`}
            >
              Điểm: {result.score}/100
            </span>
          </div>

          <div className="space-y-2">
            {result.blanks.map((b) => (
              <div
                key={b.index}
                className={`p-3 rounded-lg border text-xs space-y-1 ${
                  b.isCorrect
                    ? "bg-emerald-500/10 border-emerald-500/30 text-emerald-900 dark:text-emerald-200"
                    : "bg-rose-500/10 border-rose-500/30 text-rose-900 dark:text-rose-200"
                }`}
              >
                <div className="flex flex-wrap items-center gap-2 font-semibold">
                  {b.isCorrect ? (
                    <Check className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400" />
                  ) : (
                    <XCircle className="w-3.5 h-3.5 text-rose-600 dark:text-rose-400" />
                  )}
                  <span>Ô #{b.index + 1}:</span>
                  <span>Bạn điền: &quot;{b.userAnswer || "(trống)"}&quot;</span>
                  {!b.isCorrect && (
                    <span className="font-bold">
                      → Đáp án chấp nhận: {b.acceptedAnswers.join(" / ")}
                    </span>
                  )}
                </div>
                {b.explanationVi && <p className="opacity-90">{b.explanationVi}</p>}
              </div>
            ))}
          </div>

          {item.meaningVi && (
            <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-300 italic">
              <strong>Dịch nghĩa:</strong> {item.meaningVi}
            </p>
          )}
          {item.explanationVi && (
            <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
              <strong>Lưu ý ngữ pháp:</strong> {item.explanationVi}
            </p>
          )}
        </div>
      )}
    </div>
  );
}

// ============================================================================
// TAB 6: TRANSLATE (DỊCH VIỆT -> ANH)
// ============================================================================

export function TranslatePracticeView({
  item,
  indexLabel,
  onSpeak,
  onRecordAttempt,
  onAutoSaveMistakes,
}: {
  item: TranslatePracticeItem;
  indexLabel: string;
  onSpeak: (text: string, rate?: number) => void;
  onRecordAttempt: (params: {
    mode: "translate";
    score: number;
    isPassed: boolean;
    activityKey: string;
    activityTitle: string;
    activityId?: string;
    details: Record<string, unknown>;
  }) => Promise<void>;
  onAutoSaveMistakes: (items: AutoSaveFsrsCandidate[]) => Promise<void>;
}) {
  const [answer, setAnswer] = useState("");
  const [showKeywords, setShowKeywords] = useState(false);
  const [result, setResult] = useState<TranslationEvaluationResult | null>(null);

  useEffect(() => {
    setAnswer("");
    setShowKeywords(false);
    setResult(null);
  }, [item.id]);

  const handleCheck = () => {
    const evalRes = evaluateTranslationAnswer(item, answer);
    setResult(evalRes);

    const passing = item.passingScore ?? 70;
    const isPassed = evalRes.score >= passing;

    void onRecordAttempt({
      mode: "translate",
      score: evalRes.score,
      isPassed,
      activityKey: `translate-${item.id}`,
      activityTitle: item.title,
      activityId: item.sourceActivityId,
      details: {
        itemId: item.id,
        sourceVi: item.sourceVi,
        userAnswer: answer,
        matchedAnswer: evalRes.matchedAnswer,
        missingTokens: evalRes.missingTokens,
      },
    });

    if (!isPassed) {
      void onAutoSaveMistakes([
        {
          front: item.sourceVi,
          back: `Dịch mẫu: ${item.sampleAnswer}${item.grammarNoteVi ? `\nLưu ý: ${item.grammarNoteVi}` : ""}`,
          example: item.sampleAnswer,
        },
      ]);
    }
  };

  return (
    <div
      data-testid="practice-panel-translate"
      className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 shadow-sm p-5 sm:p-7 space-y-5"
    >
      <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
        <div className="space-y-1">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-xs font-semibold text-[var(--text-secondary,#71717a)] dark:text-zinc-400 uppercase tracking-wider">
              Dịch Việt → Anh · {indexLabel}
            </span>
            <DifficultyBadge difficulty={item.difficulty} />
          </div>
          <h3 className="text-base sm:text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
            {item.title}
          </h3>
        </div>

        {item.hintKeywords.length > 0 && (
          <button
            type="button"
            onClick={() => setShowKeywords((v) => !v)}
            className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl text-xs font-semibold min-h-[40px] bg-amber-500/10 text-amber-800 dark:text-amber-200 border border-amber-500/30 hover:bg-amber-500/20 cursor-pointer"
          >
            {showKeywords ? <EyeOff className="w-3.5 h-3.5" /> : <Eye className="w-3.5 h-3.5" />}
            <span>{showKeywords ? "Ẩn từ khóa gợi ý" : "Hiện từ khóa gợi ý"}</span>
          </button>
        )}
      </div>

      {/* Vietnamese source card */}
      <div className="p-4 sm:p-5 rounded-xl bg-indigo-500/8 border border-indigo-500/20 space-y-2">
        <span className="text-[11px] font-bold uppercase tracking-wider text-[#5E51E8] dark:text-[#818CF8]">
          Câu tiếng Việt cần dịch:
        </span>
        <p className="text-base sm:text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 leading-relaxed">
          &quot;{item.sourceVi}&quot;
        </p>

        {showKeywords && item.hintKeywords.length > 0 && (
          <div className="pt-2 flex flex-wrap items-center gap-1.5 animate-fadeIn">
            <span className="text-xs font-semibold text-amber-800 dark:text-amber-300">Từ khóa gợi ý:</span>
            {item.hintKeywords.map((kw) => (
              <span
                key={kw}
                className="px-2.5 py-1 rounded-lg text-xs font-semibold bg-amber-500/15 text-amber-900 dark:text-amber-200 border border-amber-500/30"
              >
                {kw}
              </span>
            ))}
          </div>
        )}
      </div>

      {/* User translation input */}
      <div className="space-y-3">
        <textarea
          data-testid="translate-input"
          value={answer}
          onChange={(e) => {
            setAnswer(e.target.value);
            setResult(null);
          }}
          rows={3}
          placeholder="Nhập bản dịch tiếng Anh của bạn tại đây (hỗ trợ cả dạng viết tắt như I'm, don't)..."
          className="w-full p-4 rounded-xl text-sm sm:text-base bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-[#5E51E8]"
        />

        <div className="flex flex-wrap items-center gap-2.5">
          <button
            type="button"
            data-testid="translate-check-btn"
            onClick={handleCheck}
            className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold text-sm min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs cursor-pointer"
          >
            <CheckCircle2 className="w-4 h-4" />
            <span>Kiểm tra bản dịch</span>
          </button>

          <button
            type="button"
            onClick={() => {
              setAnswer("");
              setResult(null);
            }}
            className="inline-flex items-center gap-1.5 px-4 py-2.5 rounded-xl text-xs font-semibold min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-secondary,#52525b)] dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-700 cursor-pointer"
          >
            <RotateCcw className="w-3.5 h-3.5" />
            <span>Xóa làm lại</span>
          </button>
        </div>
      </div>

      {/* Result card */}
      {result && (
        <div
          data-testid="translate-result-card"
          className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-3 animate-fadeIn"
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span className="font-bold text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100">
              {result.isExactMatch ? "Bản dịch chuẩn xác!" : "Kết quả đối chiếu bản dịch"}
            </span>
            <span
              className={`px-3 py-1 rounded-full text-xs font-extrabold ${
                result.score >= 70
                  ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                  : "bg-amber-500/15 text-amber-800 dark:text-amber-300"
              }`}
            >
              Điểm: {result.score}/100
            </span>
          </div>

          <p className="text-xs sm:text-sm text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
            {result.feedbackVi}
          </p>

          {result.missingTokens.length > 0 && (
            <div className="flex flex-wrap items-center gap-1.5 text-xs">
              <span className="font-semibold text-rose-700 dark:text-rose-300">Từ cần bổ sung/chỉnh:</span>
              {result.missingTokens.slice(0, 8).map((tok) => (
                <span
                  key={tok}
                  className="px-2 py-0.5 rounded bg-rose-500/15 text-rose-800 dark:text-rose-200 font-mono"
                >
                  {tok}
                </span>
              ))}
            </div>
          )}

          <div className="p-3.5 rounded-xl bg-emerald-500/10 border border-emerald-500/30 space-y-2">
            <div className="flex items-center justify-between gap-2">
              <span className="text-xs font-bold text-emerald-800 dark:text-emerald-300">
                Đáp án tham khảo chuẩn:
              </span>
              <button
                type="button"
                onClick={() => onSpeak(item.sampleAnswer, 1.0)}
                className="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-semibold bg-emerald-600 text-white hover:bg-emerald-700 cursor-pointer"
              >
                <Volume2 className="w-3.5 h-3.5" />
                <span>Nghe mẫu</span>
              </button>
            </div>
            <p className="text-sm font-semibold text-emerald-950 dark:text-emerald-100">
              {item.sampleAnswer}
            </p>
            {item.acceptedAnswers.length > 1 && (
              <div className="text-xs text-emerald-800 dark:text-emerald-300 space-y-0.5 pt-1 border-t border-emerald-500/20">
                <span className="font-semibold">Các cách diễn đạt khác cũng được chấp nhận:</span>
                <ul className="list-disc list-inside space-y-0.5">
                  {item.acceptedAnswers
                    .filter((a) => a !== item.sampleAnswer)
                    .slice(0, 3)
                    .map((alt, i) => (
                      <li key={i}>{alt}</li>
                    ))}
                </ul>
              </div>
            )}
          </div>

          {item.grammarNoteVi && (
            <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
              <strong>Ghi chú cấu trúc:</strong> {item.grammarNoteVi}
            </p>
          )}
        </div>
      )}
    </div>
  );
}

// ============================================================================
// TAB 7: GRAMMAR FIX (SỬA LỖI NGỮ PHÁP)
// ============================================================================

export function GrammarFixPracticeView({
  item,
  indexLabel,
  onSpeak,
  onRecordAttempt,
  onAutoSaveMistakes,
}: {
  item: GrammarFixPracticeItem;
  indexLabel: string;
  onSpeak: (text: string, rate?: number) => void;
  onRecordAttempt: (params: {
    mode: "grammar_fix";
    score: number;
    isPassed: boolean;
    activityKey: string;
    activityTitle: string;
    activityId?: string;
    details: Record<string, unknown>;
  }) => Promise<void>;
  onAutoSaveMistakes: (items: AutoSaveFsrsCandidate[]) => Promise<void>;
}) {
  const [answer, setAnswer] = useState("");
  const [showErrorHint, setShowErrorHint] = useState(false);
  const [result, setResult] = useState<GrammarFixEvaluationResult | null>(null);

  useEffect(() => {
    setAnswer("");
    setShowErrorHint(false);
    setResult(null);
  }, [item.id]);

  const handleCheck = () => {
    const evalRes = evaluateGrammarFixAnswer(item, answer);
    setResult(evalRes);

    const passing = item.passingScore ?? 70;
    const isPassed = evalRes.score >= passing;

    void onRecordAttempt({
      mode: "grammar_fix",
      score: evalRes.score,
      isPassed,
      activityKey: `grammar_fix-${item.id}`,
      activityTitle: item.title,
      activityId: item.sourceActivityId,
      details: {
        itemId: item.id,
        incorrectSentence: item.incorrectSentence,
        userAnswer: answer,
        correctedSentence: item.correctedSentence,
        isExactMatch: evalRes.isExactMatch,
      },
    });

    if (!isPassed) {
      void onAutoSaveMistakes([
        {
          front: `Sửa lỗi: ${item.incorrectSentence}`,
          back: `Câu đúng: ${item.correctedSentence}\nQuy tắc: ${item.explanationVi}`,
          example: item.correctedSentence,
        },
      ]);
    }
  };

  return (
    <div
      data-testid="practice-panel-grammar_fix"
      className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 shadow-sm p-5 sm:p-7 space-y-5"
    >
      <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
        <div className="space-y-1">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-xs font-semibold text-[var(--text-secondary,#71717a)] dark:text-zinc-400 uppercase tracking-wider">
              Sửa Lỗi Ngữ Pháp · {indexLabel}
            </span>
            <DifficultyBadge difficulty={item.difficulty} />
          </div>
          <h3 className="text-base sm:text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
            {item.title}
          </h3>
        </div>

        <button
          type="button"
          onClick={() => setShowErrorHint((v) => !v)}
          className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl text-xs font-semibold min-h-[40px] bg-amber-500/10 text-amber-800 dark:text-amber-200 border border-amber-500/30 hover:bg-amber-500/20 cursor-pointer"
        >
          <Lightbulb className="w-3.5 h-3.5 text-amber-600 dark:text-amber-400" />
          <span>{showErrorHint ? "Ẩn gợi ý loại lỗi" : "Gợi ý loại lỗi"}</span>
        </button>
      </div>

      {/* Incorrect sentence box */}
      <div className="p-4 sm:p-5 rounded-xl bg-rose-500/10 border border-rose-500/30 space-y-2">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <span className="text-[11px] font-bold uppercase tracking-wider text-rose-700 dark:text-rose-300">
            Câu chứa lỗi ngữ pháp cần sửa:
          </span>
          <button
            type="button"
            onClick={() => {
              setAnswer(item.incorrectSentence);
              setResult(null);
            }}
            className="text-xs font-semibold text-[#5E51E8] dark:text-[#818CF8] hover:underline cursor-pointer"
          >
            Chép câu này xuống để sửa nhanh
          </button>
        </div>
        <p className="text-base sm:text-lg font-bold text-rose-950 dark:text-rose-100 leading-relaxed">
          {item.incorrectSentence}
        </p>
        {item.meaningVi && (
          <p className="text-xs text-rose-900/80 dark:text-rose-200/80 italic">
            Ý muốn diễn đạt: {item.meaningVi}
          </p>
        )}
      </div>

      {showErrorHint && (
        <div className="p-3.5 rounded-xl bg-amber-500/10 border border-amber-500/30 text-xs text-amber-900 dark:text-amber-200 animate-fadeIn">
          <strong>Gợi ý loại lỗi:</strong> {item.errorTypeHintVi}
        </div>
      )}

      {/* User rewrite input */}
      <div className="space-y-3">
        <textarea
          data-testid="grammar-fix-input"
          value={answer}
          onChange={(e) => {
            setAnswer(e.target.value);
            setResult(null);
          }}
          rows={3}
          placeholder="Viết lại câu tiếng Anh đã sửa lỗi hoàn chỉnh..."
          className="w-full p-4 rounded-xl text-sm sm:text-base bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-[#5E51E8]"
        />

        <div className="flex flex-wrap items-center gap-2.5">
          <button
            type="button"
            data-testid="grammar-fix-check-btn"
            onClick={handleCheck}
            className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold text-sm min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs cursor-pointer"
          >
            <CheckCircle2 className="w-4 h-4" />
            <span>Kiểm tra sửa lỗi</span>
          </button>

          <button
            type="button"
            onClick={() => {
              setAnswer("");
              setResult(null);
            }}
            className="inline-flex items-center gap-1.5 px-4 py-2.5 rounded-xl text-xs font-semibold min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-secondary,#52525b)] dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-700 cursor-pointer"
          >
            <RotateCcw className="w-3.5 h-3.5" />
            <span>Xóa làm lại</span>
          </button>
        </div>
      </div>

      {/* Result card */}
      {result && (
        <div
          data-testid="grammar-fix-result-card"
          className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-3 animate-fadeIn"
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span className="font-bold text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100">
              {result.isExactMatch ? "Sửa lỗi chính xác 100%!" : "Đối chiếu sửa lỗi ngữ pháp"}
            </span>
            <span
              className={`px-3 py-1 rounded-full text-xs font-extrabold ${
                result.score >= 70
                  ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                  : "bg-rose-500/15 text-rose-700 dark:text-rose-300"
              }`}
            >
              Điểm: {result.score}/100
            </span>
          </div>

          <p className="text-xs sm:text-sm text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
            {result.feedbackVi}
          </p>

          <div className="p-3.5 rounded-xl bg-emerald-500/10 border border-emerald-500/30 space-y-1.5">
            <div className="flex items-center justify-between gap-2">
              <span className="text-xs font-bold text-emerald-800 dark:text-emerald-300">
                Câu đúng chuẩn ngữ pháp:
              </span>
              <button
                type="button"
                onClick={() => onSpeak(item.correctedSentence, 1.0)}
                className="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-semibold bg-emerald-600 text-white hover:bg-emerald-700 cursor-pointer"
              >
                <Volume2 className="w-3.5 h-3.5" />
                <span>Nghe câu đúng</span>
              </button>
            </div>
            <p className="text-sm font-semibold text-emerald-950 dark:text-emerald-100">
              {item.correctedSentence}
            </p>
          </div>

          <div className="p-3 rounded-lg bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-300 leading-relaxed">
            <strong className="text-[var(--text-primary,#18181b)] dark:text-zinc-100">
              Giải thích quy tắc ngữ pháp:{" "}
            </strong>
            {result.explanationVi}
          </div>
        </div>
      )}
    </div>
  );
}

// ============================================================================
// TAB 8: ROLEPLAY (NHẬP VAI HỘI THOẠI — GIỌNG NÓI HOẶC GÕ PHÍM + AI / OFFLINE)
// ============================================================================

export function RoleplayPracticeView({
  item,
  indexLabel,
  voiceLang,
  ttsSpeed,
  contextTitle,
  onSpeak,
  onRecordAttempt,
  onAutoSaveMistakes,
}: {
  item: RoleplayPracticeItem;
  indexLabel: string;
  voiceLang: LanguageCode;
  ttsSpeed: number;
  contextTitle: string;
  onSpeak: (text: string, rate?: number) => void;
  onRecordAttempt: (params: {
    mode: "roleplay";
    score: number;
    isPassed: boolean;
    activityKey: string;
    activityTitle: string;
    activityId?: string;
    details: Record<string, unknown>;
  }) => Promise<void>;
  onAutoSaveMistakes: (items: AutoSaveFsrsCandidate[]) => Promise<void>;
}) {
  const [responseText, setResponseText] = useState("");
  const [showKeywordHints, setShowKeywordHints] = useState(false);
  const [isRecording, setIsRecording] = useState(false);
  const [speechSupported, setSpeechSupported] = useState(true);
  const [micError, setMicError] = useState<string | null>(null);
  const [isEvaluating, setIsEvaluating] = useState(false);
  const [result, setResult] = useState<RoleplayEvaluationResult | null>(null);

  const recognitionRef = useRef<any>(null);

  useEffect(() => {
    if (typeof window !== "undefined") {
      const win = window as any;
      setSpeechSupported(Boolean(win.SpeechRecognition || win.webkitSpeechRecognition));
    }
  }, []);

  useEffect(() => {
    setResponseText("");
    setShowKeywordHints(false);
    setResult(null);
    setMicError(null);
    if (recognitionRef.current) {
      try {
        recognitionRef.current.stop();
      } catch {}
    }
    setIsRecording(false);
  }, [item.id]);

  useEffect(() => {
    return () => {
      if (recognitionRef.current) {
        try {
          recognitionRef.current.stop();
        } catch {}
      }
    };
  }, []);

  const handleToggleMic = () => {
    if (isRecording) {
      if (recognitionRef.current) {
        try {
          recognitionRef.current.stop();
        } catch {}
      }
      setIsRecording(false);
      return;
    }

    setMicError(null);
    const win = window as any;
    const SpeechRecognition = win.SpeechRecognition || win.webkitSpeechRecognition;
    if (!SpeechRecognition) {
      setSpeechSupported(false);
      setMicError("Trình duyệt chưa hỗ trợ Web Speech Recognition. Bạn hãy gõ câu trả lời vào ô bên dưới.");
      return;
    }

    try {
      const recognition = new SpeechRecognition();
      recognition.lang = voiceLang;
      recognition.interimResults = true;
      recognition.continuous = true;

      recognition.onresult = (event: any) => {
        let transcript = "";
        for (let i = 0; i < event.results.length; i++) {
          transcript += event.results[i][0].transcript + " ";
        }
        setResponseText(transcript.trim());
        setResult(null);
      };

      recognition.onerror = (ev: any) => {
        if (ev?.error === "not-allowed" || ev?.error === "service-not-allowed") {
          setMicError("Trình duyệt đã từ chối quyền micro. Hãy cấp quyền micro hoặc chuyển sang gõ phím.");
        } else {
          setMicError("Không nhận diện được giọng nói lúc này. Bạn có thể nói lại hoặc gõ trực tiếp.");
        }
        setIsRecording(false);
      };

      recognition.onend = () => {
        setIsRecording(false);
      };

      recognition.start();
      recognitionRef.current = recognition;
      setIsRecording(true);
    } catch {
      setMicError("Không thể khởi động nhận diện giọng nói. Bạn hãy gõ câu trả lời vào ô bên dưới.");
      setIsRecording(false);
    }
  };

  const handleSubmitRoleplay = async () => {
    const trimmed = responseText.trim();
    if (!trimmed || isEvaluating) return;

    if (isRecording && recognitionRef.current) {
      try {
        recognitionRef.current.stop();
      } catch {}
      setIsRecording(false);
    }

    setIsEvaluating(true);
    let finalResult: RoleplayEvaluationResult;

    try {
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const res = await fetch("/api/v1/ai/evaluate-practice", {
        method: "POST",
        headers,
        body: JSON.stringify({
          mode: "roleplay",
          lang: voiceLang,
          prompt: `${item.scenarioVi}\n${item.aiRole}: "${item.aiPrompt}"\nMục tiêu người học: ${item.userGoalVi}`,
          userResponse: trimmed,
          expectedKeywords: item.expectedKeywords,
          sampleAnswer: item.sampleResponses[0] || "",
          contextTitle,
        }),
      });

      if (res.ok) {
        const data = await res.json();
        const usedLive = Boolean(data.usedLiveEndpoint);
        const mode = usedLive ? "ai_live" : "keyword_offline";
        finalResult = {
          score: typeof data.score === "number" ? Math.max(0, Math.min(100, Math.round(data.score))) : 0,
          gradingMode: mode,
          usedLiveEndpoint: usedLive,
          gradingLabelVi: usedLive ? GRADING_LABELS.ai_live : GRADING_LABELS.keyword_offline,
          feedbackVi:
            typeof data.feedbackVi === "string" && data.feedbackVi.trim()
              ? data.feedbackVi.trim()
              : "Đã chấm xong phản hồi hội thoại.",
          correctedVersion:
            typeof data.correctedVersion === "string" && data.correctedVersion.trim()
              ? data.correctedVersion.trim()
              : item.sampleResponses[0] || "",
          strengths: Array.isArray(data.strengths) ? data.strengths : [],
          improvements: Array.isArray(data.improvements) ? data.improvements : [],
          matchedKeywords: Array.isArray(data.matchedKeywords) ? data.matchedKeywords : [],
          missingKeywords: Array.isArray(data.missingKeywords) ? data.missingKeywords : [],
        };
      } else {
        // Honest client-side deterministic fallback when backend returns error / 429 / offline
        finalResult = evaluateRoleplayOffline(item, trimmed);
      }
    } catch {
      finalResult = evaluateRoleplayOffline(item, trimmed);
    } finally {
      setIsEvaluating(false);
    }

    setResult(finalResult);

    const passing = item.passingScore ?? 70;
    const isPassed = finalResult.score >= passing;

    void onRecordAttempt({
      mode: "roleplay",
      score: finalResult.score,
      isPassed,
      activityKey: `roleplay-${item.id}`,
      activityTitle: item.title,
      activityId: item.sourceActivityId,
      details: {
        itemId: item.id,
        userResponse: trimmed,
        gradingMode: finalResult.gradingMode,
        usedLiveEndpoint: finalResult.usedLiveEndpoint,
        matchedKeywords: finalResult.matchedKeywords,
      },
    });

    if (!isPassed) {
      const refSentence = finalResult.correctedVersion || item.sampleResponses[0] || "";
      if (refSentence) {
        void onAutoSaveMistakes([
          {
            front: `Phản hồi tình huống: ${item.aiPrompt}`,
            back: `Mẫu trả lời tự nhiên: ${refSentence}\nTừ khóa cần nhớ: ${item.expectedKeywords.join(", ")}`,
            example: refSentence,
          },
        ]);
      }
    }
  };

  return (
    <div
      data-testid="practice-panel-roleplay"
      className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 shadow-sm p-5 sm:p-7 space-y-5"
    >
      <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
        <div className="space-y-1">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-xs font-semibold text-[var(--text-secondary,#71717a)] dark:text-zinc-400 uppercase tracking-wider">
              Nhập Vai Hội Thoại (Roleplay) · {indexLabel}
            </span>
            <DifficultyBadge difficulty={item.difficulty} />
          </div>
          <h3 className="text-base sm:text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
            {item.title}
          </h3>
          <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400">
            <strong>Bối cảnh:</strong> {item.scenarioVi}
          </p>
        </div>

        {item.expectedKeywords.length > 0 && (
          <button
            type="button"
            onClick={() => setShowKeywordHints((v) => !v)}
            className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl text-xs font-semibold min-h-[40px] bg-amber-500/10 text-amber-800 dark:text-amber-200 border border-amber-500/30 hover:bg-amber-500/20 cursor-pointer"
          >
            <Lightbulb className="w-3.5 h-3.5 text-amber-600 dark:text-amber-400" />
            <span>{showKeywordHints ? "Ẩn từ khóa gợi ý" : "Gợi ý từ khóa"}</span>
          </button>
        )}
      </div>

      {/* AI Dialogue Bubble */}
      <div className="p-4 sm:p-5 rounded-2xl bg-gradient-to-br from-[#5E51E8]/10 via-indigo-500/5 to-transparent border border-[#5E51E8]/25 space-y-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <span className="text-xs font-bold text-[#5E51E8] dark:text-[#818CF8]">
            {item.aiRole} nói:
          </span>
          <div className="flex items-center gap-1.5">
            <button
              type="button"
              onClick={() => onSpeak(item.aiPrompt, ttsSpeed)}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-[#5E51E8] text-white hover:bg-[#4F46E5] min-h-[36px] cursor-pointer"
            >
              <Volume2 className="w-3.5 h-3.5" />
              <span>Nghe lời thoại</span>
            </button>
            <button
              type="button"
              onClick={() => onSpeak(item.aiPrompt, 0.75)}
              className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg text-xs font-medium bg-[var(--surface-card,#ffffff)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 text-[var(--text-secondary,#52525b)] dark:text-zinc-300 min-h-[36px] cursor-pointer"
            >
              <Sliders className="w-3 h-3" />
              <span>0.75x</span>
            </button>
          </div>
        </div>

        <p className="text-base sm:text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 leading-relaxed">
          &quot;{item.aiPrompt}&quot;
        </p>

        {item.aiPromptVi && (
          <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400 italic">
            Dịch: {item.aiPromptVi}
          </p>
        )}

        <div className="pt-2 border-t border-[#5E51E8]/15 text-xs text-[var(--text-primary,#18181b)] dark:text-zinc-200">
          <strong>Nhiệm vụ của bạn:</strong> {item.userGoalVi}
        </div>
      </div>

      {showKeywordHints && item.expectedKeywords.length > 0 && (
        <div className="p-3.5 rounded-xl bg-amber-500/10 border border-amber-500/30 flex flex-wrap items-center gap-1.5 text-xs animate-fadeIn">
          <span className="font-bold text-amber-900 dark:text-amber-200">Từ khóa nên dùng:</span>
          {item.expectedKeywords.map((kw) => (
            <span
              key={kw}
              className="px-2.5 py-1 rounded-lg bg-amber-500/15 text-amber-900 dark:text-amber-100 font-semibold border border-amber-500/30"
            >
              {kw}
            </span>
          ))}
        </div>
      )}

      {!speechSupported && (
        <div className="p-3 rounded-xl bg-amber-500/10 border border-amber-500/30 text-xs text-amber-900 dark:text-amber-200 flex items-start gap-2">
          <AlertCircle className="w-4 h-4 shrink-0 mt-0.5 text-amber-600 dark:text-amber-400" />
          <span>
            Trình duyệt chưa hỗ trợ nhận diện giọng nói tự động (Web Speech API). Bạn hãy gõ câu trả lời trực tiếp vào ô bên dưới để chấm điểm.
          </span>
        </div>
      )}

      {micError && (
        <div className="p-3 rounded-xl bg-rose-500/10 border border-rose-500/30 text-xs text-rose-900 dark:text-rose-200 flex items-start gap-2">
          <AlertCircle className="w-4 h-4 shrink-0 mt-0.5 text-rose-600 dark:text-rose-400" />
          <span>{micError}</span>
        </div>
      )}

      {/* User Answer Controls (Voice OR Type) */}
      <div className="space-y-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <span className="text-xs font-bold text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
            Câu trả lời của bạn (nói bằng micro hoặc gõ phím):
          </span>
          <button
            type="button"
            data-testid="roleplay-mic-btn"
            onClick={handleToggleMic}
            className={`inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl text-xs font-bold min-h-[40px] transition cursor-pointer ${
              isRecording
                ? "bg-rose-600 text-white animate-pulse"
                : "bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[#5E51E8] dark:text-[#818CF8] border border-[#5E51E8]/30 hover:bg-[#5E51E8]/10"
            }`}
          >
            {isRecording ? <MicOff className="w-4 h-4" /> : <Mic className="w-4 h-4" />}
            <span>{isRecording ? "Đang nghe... Bấm để dừng" : `Nói bằng mic (${voiceLang})`}</span>
          </button>
        </div>

        <textarea
          data-testid="roleplay-input"
          value={responseText}
          onChange={(e) => {
            setResponseText(e.target.value);
            setResult(null);
          }}
          rows={3}
          placeholder="Nhập hoặc đọc câu phản hồi của bạn bằng tiếng Anh..."
          className="w-full p-4 rounded-xl text-sm sm:text-base bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-[#5E51E8]"
        />

        <div className="flex flex-wrap items-center gap-2.5">
          <button
            type="button"
            data-testid="roleplay-submit-btn"
            disabled={!responseText.trim() || isEvaluating}
            onClick={() => void handleSubmitRoleplay()}
            className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold text-sm min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs disabled:opacity-50 cursor-pointer"
          >
            {isEvaluating ? (
              <Loader2 className="w-4 h-4 animate-spin" />
            ) : (
              <Send className="w-4 h-4" />
            )}
            <span>{isEvaluating ? "Đang chấm phản hồi..." : "Gửi & Chấm điểm hội thoại"}</span>
          </button>

          <button
            type="button"
            onClick={() => {
              setResponseText("");
              setResult(null);
            }}
            className="inline-flex items-center gap-1.5 px-4 py-2.5 rounded-xl text-xs font-semibold min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-secondary,#52525b)] dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-700 cursor-pointer"
          >
            <RotateCcw className="w-3.5 h-3.5" />
            <span>Làm lại</span>
          </button>
        </div>
      </div>

      {/* Roleplay Grading Result Card with Honest Live vs Offline Badge */}
      {result && (
        <div
          data-testid="roleplay-result-card"
          className="p-4 sm:p-5 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-4 animate-fadeIn"
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex flex-wrap items-center gap-2">
              <span
                data-testid="roleplay-grading-badge"
                className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold border ${
                  result.usedLiveEndpoint
                    ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border-emerald-500/30"
                    : "bg-amber-500/15 text-amber-800 dark:text-amber-200 border-amber-500/35"
                }`}
              >
                <Sparkles className="w-3.5 h-3.5" />
                <span>{result.gradingLabelVi}</span>
              </span>
            </div>

            <span
              className={`px-3 py-1 rounded-full text-xs font-extrabold ${
                result.score >= 70
                  ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                  : "bg-amber-500/15 text-amber-800 dark:text-amber-300"
              }`}
            >
              Điểm: {result.score}/100
            </span>
          </div>

          <p className="text-xs sm:text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100 leading-relaxed">
            {result.feedbackVi}
          </p>

          {(result.strengths.length > 0 || result.improvements.length > 0) && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {result.strengths.length > 0 && (
                <div className="p-3 rounded-lg bg-emerald-500/10 border border-emerald-500/25 space-y-1">
                  <div className="text-xs font-bold text-emerald-800 dark:text-emerald-300">
                    Điểm tốt
                  </div>
                  <ul className="space-y-1 text-xs text-emerald-950 dark:text-emerald-100 list-disc list-inside">
                    {result.strengths.map((s, i) => (
                      <li key={i}>{s}</li>
                    ))}
                  </ul>
                </div>
              )}

              {result.improvements.length > 0 && (
                <div className="p-3 rounded-lg bg-amber-500/10 border border-amber-500/25 space-y-1">
                  <div className="text-xs font-bold text-amber-800 dark:text-amber-300">
                    Gợi ý cải thiện
                  </div>
                  <ul className="space-y-1 text-xs text-amber-950 dark:text-amber-100 list-disc list-inside">
                    {result.improvements.map((s, i) => (
                      <li key={i}>{s}</li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}

          {result.correctedVersion && (
            <div className="p-3.5 rounded-xl bg-indigo-500/10 border border-indigo-500/25 space-y-1.5">
              <div className="flex items-center justify-between gap-2">
                <span className="text-xs font-bold text-[#5E51E8] dark:text-[#818CF8]">
                  Phiên bản diễn đạt tự nhiên gợi ý:
                </span>
                <button
                  type="button"
                  onClick={() => onSpeak(result.correctedVersion, 1.0)}
                  className="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-semibold bg-[#5E51E8] text-white hover:bg-[#4F46E5] cursor-pointer"
                >
                  <Volume2 className="w-3.5 h-3.5" />
                  <span>Nghe mẫu</span>
                </button>
              </div>
              <p className="text-sm font-semibold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                {result.correctedVersion}
              </p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
