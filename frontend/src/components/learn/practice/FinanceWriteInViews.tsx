"use client";

import React, { useEffect, useMemo, useState } from "react";
import {
  AlertCircle,
  BookmarkPlus,
  Calculator,
  CheckCircle2,
  FileText,
  Loader2,
  RefreshCw,
  Send,
  Sparkles,
  XCircle,
} from "lucide-react";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import {
  WRITEIN_GRADING_LABELS,
  evaluateJournalCriteriaOffline,
  evaluateNumericAnswer,
  evaluatePracticeWithAiOrOffline,
  generateFinanceCalculationProblem,
  type AiPracticeEvaluationResult,
  type DecisionJournalPromptItem,
  type FinanceCalcProblem,
  type FinanceCalcType,
  type FinanceCaseWriteInItem,
  type NumericEvaluationResult,
} from "@/lib/writein-practice";
import { saveWriteInFlashcardDeduplicated } from "./WriteInFsrsHelper";

const CALC_TYPE_OPTIONS: Array<{ id: FinanceCalcType; label: string; shortDesc: string }> = [
  { id: "compound", label: "Lãi kép", shortDesc: "FV = P(1+r)^n" },
  { id: "dca", label: "Tích lũy DCA", shortDesc: "Góp đều hàng tháng" },
  { id: "pe", label: "Định giá P/E", shortDesc: "P/E & Giá mục tiêu" },
  { id: "margin_of_safety", label: "Biên an toàn", shortDesc: "Graham Margin of Safety" },
  { id: "drawdown", label: "Drawdown & Hòa vốn", shortDesc: "% Giảm & % Tăng lại" },
];

export const DEFAULT_FINANCE_CASES: FinanceCaseWriteInItem[] = [
  {
    id: "fcase-dca-crash",
    title: "Tình huống 1: Thị trường giảm 25% khi đang tích lũy DCA quỹ chỉ số",
    scenario:
      "Bạn đang trích đều 5 triệu đ/tháng để tích lũy quỹ chỉ số VN30 cho mục tiêu 15 năm. Sau 8 tháng, thị trường chung điều chỉnh giảm 25%, danh mục đang tạm lỗ 14% và nhiều hội nhóm hô hào rút sạch tiền gửi tiết kiệm.",
    options: [
      "Bán cắt lỗ toàn bộ để gửi ngân hàng chờ thị trường tăng lại rồi mua sau",
      "Giữ vững kỷ luật tiếp tục DCA đều đặn hàng tháng vì mua được nhiều chứng chỉ quỹ hơn ở vùng giá rẻ",
      "Vay tín chấp ngân hàng lãi 18%/năm để bắt đáy tất tay một lần",
      "Ngừng theo dõi hoàn toàn và rút bớt một nửa để chi tiêu",
    ],
    correctIndex: 1,
    explanation:
      "Trong chiến lược DCA dài hạn, nhịp giảm sâu chính là cơ hội tích lũy nhiều đơn vị quỹ hơn với giá vốn trung bình thấp hơn. Vay lãi cao bắt đáy phá vỡ kỷ luật rủi ro.",
    reasonPromptVi:
      "Tự viết lập luận giải thích bằng lời: Vì sao bạn chọn phương án trên? Tác động đến giá vốn trung bình, lãi kép và tâm lý FOMO/hoảng loạn ra sao?",
    expectedKeywords: ["dca", "giá vốn", "dài hạn", "kỷ luật", "lãi kép"],
    sampleReasoningVi:
      "Với tầm nhìn 15 năm, kỷ luật DCA giúp mua thêm nhiều chứng chỉ quỹ khi giá giảm, hạ giá vốn trung bình và tận dụng lãi kép khi thị trường phục hồi mà không bị cảm xúc hoảng loạn dẫn dắt.",
    minWords: 12,
  },
  {
    id: "fcase-margin-of-safety",
    title: "Tình huống 2: Cổ phiếu đầu ngành giảm sâu về vùng chiết khấu P/E 9x",
    scenario:
      "Một doanh nghiệp tiêu dùng đầu ngành có ROE duy trì 22%/năm, dòng tiền tự do dương đều và không nợ vay. Do tin xấu vĩ mô ngắn hạn, giá giảm từ 100.000đ về 65.000đ (giá trị nội tại ước tính 100.000đ).",
    options: [
      "Bán tháo theo tin tức vĩ mô vì lo sợ giá còn giảm thêm",
      "Đánh giá biên an toàn 35%, kiểm tra lại dòng tiền cốt lõi và giải ngân từng phần có kỷ luật",
      "All-in kèm đòn bẩy margin 1:2 trong một phiên duy nhất",
      "Bỏ qua doanh nghiệp tốt để chuyển sang cổ phiếu đầu cơ đang tăng trần",
    ],
    correctIndex: 1,
    explanation:
      "Giá 65.000đ so với giá trị nội tại 100.000đ mang lại biên an toàn 35% theo Benjamin Graham. Giải ngân từng phần giúp quản trị rủi ro biến động ngắn hạn.",
    reasonPromptVi:
      "Viết ngắn gọn lý do lựa chọn của bạn, nhắc tới biên an toàn, giá trị nội tại và cách kiểm soát rủi ro:",
    expectedKeywords: ["biên an toàn", "giá trị nội tại", "dòng tiền", "rủi ro"],
    sampleReasoningVi:
      "Doanh nghiệp có nền tảng dòng tiền và ROE mạnh, trong khi giá thị trường đang chiết khấu 35% dưới giá trị nội tại tạo biên an toàn lớn. Giải ngân từng phần giúp quản trị rủi ro mà không dùng đòn bẩy quá mức.",
    minWords: 12,
  },
];

// ============================================================================
// 1. HAND CALCULATION LAB (TỰ TÍNH TAY VỚI SỐ LIỆU NGẪU NHIÊN MỖI LẦN)
// ============================================================================

export function HandCalculationLab({
  recommendedTypes,
  lessonId,
  courseTitle,
  lessonTitle,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: {
  recommendedTypes?: FinanceCalcType[];
  lessonId?: string | null;
  courseTitle: string;
  lessonTitle: string;
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
}) {
  const defaultType: FinanceCalcType =
    recommendedTypes && recommendedTypes.length > 0 ? recommendedTypes[0] : "compound";

  const [calcType, setCalcType] = useState<FinanceCalcType>(defaultType);
  const [problem, setProblem] = useState<FinanceCalcProblem>(() =>
    generateFinanceCalculationProblem(defaultType)
  );
  const [primaryInput, setPrimaryInput] = useState("");
  const [secondaryInput, setSecondaryInput] = useState("");
  const [primaryResult, setPrimaryResult] = useState<NumericEvaluationResult | null>(null);
  const [secondaryResult, setSecondaryResult] = useState<NumericEvaluationResult | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  const generateNewProblem = (nextType: FinanceCalcType) => {
    setCalcType(nextType);
    // Guarantee distinct seed from previous problem so every click creates fresh numbers
    const freshSeed =
      ((Date.now() ^ Math.floor(Math.random() * 1_000_000_000) ^ (problem.seed + 7919)) >>> 0) ||
      12345;
    setProblem(generateFinanceCalculationProblem(nextType, freshSeed));
    setPrimaryInput("");
    setSecondaryInput("");
    setPrimaryResult(null);
    setSecondaryResult(null);
  };

  const handleSaveCard = async (isAuto = false) => {
    const front = `[Tự tính Tài chính] Công thức ${problem.titleVi}`;
    const back = `**Công thức:** ${problem.formulaVi}\n\n**Ví dụ:** ${problem.promptVi}\n**Đáp án:** ${problem.expected} ${problem.unit}${
      problem.secondaryExpected !== undefined
        ? ` | Hòa vốn: ${problem.secondaryExpected} ${problem.secondaryUnit}`
        : ""
    }\n\n${problem.stepsVi.join("\n")}`;

    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["dau-tu", "tu-tinh-tay", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setToast(
        isAuto
          ? res.deduplicated
            ? "Công thức này đã có trong Flashcard FSRS để bạn ôn tập."
            : "Đã tự động lưu công thức làm sai vào Flashcard FSRS!"
          : res.deduplicated
          ? "Thẻ công thức đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu công thức vào Flashcard FSRS!"
      );
      setTimeout(() => setToast(null), 4000);
    }
  };

  const handleCheck = async () => {
    const res1 = evaluateNumericAnswer(
      primaryInput,
      problem.expected,
      problem.tolerancePercent,
      problem.toleranceAbs
    );
    setPrimaryResult(res1);

    let res2: NumericEvaluationResult | null = null;
    let combinedScore = res1.score;
    let isPassed = res1.isCorrect;

    if (problem.secondaryExpected !== undefined) {
      res2 = evaluateNumericAnswer(
        secondaryInput,
        problem.secondaryExpected,
        problem.tolerancePercent,
        problem.toleranceAbs
      );
      setSecondaryResult(res2);
      combinedScore = Math.round((res1.score + res2.score) / 2);
      isPassed = res1.isCorrect && res2.isCorrect;
    } else {
      setSecondaryResult(null);
    }

    if (onRecordAttempt) {
      await onRecordAttempt({
        activityType: "hand_calc",
        mode: "hand_calc",
        score: combinedScore,
        maxScore: 100,
        isPassed,
        activityKey: `finance-hand-calc-${problem.type}`,
        activityTitle: `${problem.titleVi} · Tự tính tay`,
        details: {
          problemType: problem.type,
          seed: problem.seed,
          userPrimary: primaryInput,
          expectedPrimary: problem.expected,
          relErrorPercent: res1.relErrorPercent,
          userSecondary: problem.secondaryExpected !== undefined ? secondaryInput : undefined,
          expectedSecondary: problem.secondaryExpected,
        },
      });
    }

    if (!isPassed) {
      await handleSaveCard(true);
    }
  };

  return (
    <div data-testid="finance-hand-calc-lab" className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 pb-4">
        <div>
          <h2 className="text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 flex items-center gap-2">
            <Calculator className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
            Tự Tính Tay Tài Chính (Active Recall · Số Liệu Ngẫu Nhiên)
          </h2>
          <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400 mt-0.5">
            Rèn phản xạ tính nhẩm &amp; bấm máy 5 bài toán cốt lõi. Mỗi lần bấm tạo đề mới sẽ sinh bộ số liệu thực tế khác nhau.
          </p>
        </div>

        <button
          type="button"
          data-testid="hand-calc-new-problem-btn"
          onClick={() => generateNewProblem(calcType)}
          className="min-h-[44px] px-4 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-semibold inline-flex items-center gap-1.5 cursor-pointer shadow-xs"
        >
          <RefreshCw className="w-4 h-4" />
          <span>Đề số ngẫu nhiên mới</span>
        </button>
      </div>

      {/* 5 Type Selector Pills */}
      <div className="grid grid-cols-2 sm:grid-cols-5 gap-2">
        {CALC_TYPE_OPTIONS.map((opt) => {
          const active = calcType === opt.id;
          return (
            <button
              key={opt.id}
              type="button"
              data-testid={`hand-calc-type-${opt.id}`}
              onClick={() => generateNewProblem(opt.id)}
              className={`min-h-[48px] p-2.5 rounded-xl border text-left transition cursor-pointer ${
                active
                  ? "border-emerald-600 bg-emerald-500/10 text-emerald-900 dark:text-emerald-200 ring-1 ring-emerald-500/40"
                  : "border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-[var(--text-secondary,#52525b)] dark:text-zinc-300 hover:border-emerald-400"
              }`}
            >
              <div className="text-xs font-bold">{opt.label}</div>
              <div className="text-[10px] opacity-75">{opt.shortDesc}</div>
            </button>
          );
        })}
      </div>

      {toast && (
        <div className="p-3 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
          <Sparkles className="w-4 h-4 shrink-0" />
          <span>{toast}</span>
        </div>
      )}

      {/* Problem Card */}
      <div className="p-5 rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-900/60 space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <span className="text-xs font-bold uppercase tracking-wider text-emerald-700 dark:text-emerald-400">
            {problem.titleVi}
          </span>
          <span className="text-[11px] font-mono text-zinc-500">
            Dung sai chấm: ±{problem.tolerancePercent}% (hoặc ±{problem.toleranceAbs})
          </span>
        </div>

        <p
          data-testid="hand-calc-prompt"
          className="text-sm sm:text-base font-medium text-[var(--text-primary,#18181b)] dark:text-zinc-100 leading-relaxed"
        >
          {problem.promptVi}
        </p>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-1">
          <div className="space-y-1.5">
            <label
              htmlFor="hand-calc-primary-input"
              className="text-xs font-semibold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block"
            >
              {problem.questionLabelVi}
            </label>
            <div className="flex items-center gap-2">
              <input
                id="hand-calc-primary-input"
                data-testid="hand-calc-primary-input"
                type="text"
                inputMode="decimal"
                value={primaryInput}
                onChange={(e) => {
                  setPrimaryInput(e.target.value);
                  setPrimaryResult(null);
                }}
                onKeyDown={(e) => {
                  if (e.key === "Enter" && problem.secondaryExpected === undefined) {
                    e.preventDefault();
                    void handleCheck();
                  }
                }}
                placeholder="Nhập con số bạn tự tính..."
                className="flex-1 min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-950 text-sm font-mono font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
              />
              <span className="px-3 py-2 rounded-xl bg-zinc-200/70 dark:bg-zinc-800 text-xs font-bold text-zinc-700 dark:text-zinc-300">
                {problem.unit}
              </span>
            </div>
          </div>

          {problem.secondaryExpected !== undefined && (
            <div className="space-y-1.5">
              <label
                htmlFor="hand-calc-secondary-input"
                className="text-xs font-semibold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block"
              >
                {problem.secondaryQuestionLabelVi}
              </label>
              <div className="flex items-center gap-2">
                <input
                  id="hand-calc-secondary-input"
                  data-testid="hand-calc-secondary-input"
                  type="text"
                  inputMode="decimal"
                  value={secondaryInput}
                  onChange={(e) => {
                    setSecondaryInput(e.target.value);
                    setSecondaryResult(null);
                  }}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      void handleCheck();
                    }
                  }}
                  placeholder="Nhập % tăng lại để hòa vốn..."
                  className="flex-1 min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-950 text-sm font-mono font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                />
                <span className="px-3 py-2 rounded-xl bg-zinc-200/70 dark:bg-zinc-800 text-xs font-bold text-zinc-700 dark:text-zinc-300">
                  {problem.secondaryUnit}
                </span>
              </div>
            </div>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-2.5 pt-2">
          <button
            type="button"
            data-testid="hand-calc-check-btn"
            disabled={!primaryInput.trim()}
            onClick={() => void handleCheck()}
            className="min-h-[44px] px-5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs sm:text-sm font-semibold inline-flex items-center gap-2 cursor-pointer shadow-xs"
          >
            <CheckCircle2 className="w-4 h-4" />
            <span>Kiểm tra đáp án tự tính</span>
          </button>

          <button
            type="button"
            onClick={() => void handleSaveCard(false)}
            className="min-h-[44px] px-3.5 py-2 rounded-xl border border-indigo-300 dark:border-indigo-700 bg-indigo-500/10 text-indigo-700 dark:text-indigo-300 text-xs font-semibold inline-flex items-center gap-1.5 cursor-pointer"
          >
            <Sparkles className="w-3.5 h-3.5" />
            <span>Lưu vào Flashcard FSRS</span>
          </button>

          <button
            type="button"
            onClick={() =>
              void onSaveToNotebook(
                `### 🧮 Tự tính tay: ${problem.titleVi}\n- **Đề bài:** ${problem.promptVi}\n- **Công thức:** \`${problem.formulaVi}\`\n- **Đáp án chuẩn:** **${problem.expected} ${problem.unit}**\n${problem.stepsVi.map((s) => `- ${s}`).join("\n")}`
              )
            }
            className="min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs font-medium text-zinc-700 dark:text-zinc-300 inline-flex items-center gap-1.5 cursor-pointer"
          >
            <BookmarkPlus className="w-3.5 h-3.5 text-emerald-500" />
            <span>Lưu Sổ tay</span>
          </button>
        </div>
      </div>

      {/* Results & Step-by-step Solution */}
      {primaryResult && (
        <div
          data-testid="hand-calc-result"
          className="p-5 rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 space-y-4"
        >
          <div className="space-y-2">
            <div
              className={`p-3.5 rounded-xl border text-xs flex items-start gap-2.5 ${
                primaryResult.isCorrect
                  ? "bg-emerald-500/10 border-emerald-500/35 text-emerald-900 dark:text-emerald-200"
                  : "bg-rose-500/10 border-rose-500/35 text-rose-900 dark:text-rose-200"
              }`}
            >
              {primaryResult.isCorrect ? (
                <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0 mt-0.5" />
              ) : (
                <XCircle className="w-4 h-4 text-rose-600 shrink-0 mt-0.5" />
              )}
              <div className="space-y-1">
                <div className="font-bold">
                  Đáp án chuẩn: {problem.expected} {problem.unit} · Sai số:{" "}
                  {primaryResult.relErrorPercent}%
                </div>
                <p>{primaryResult.feedbackVi}</p>
              </div>
            </div>

            {secondaryResult && problem.secondaryExpected !== undefined && (
              <div
                className={`p-3.5 rounded-xl border text-xs flex items-start gap-2.5 ${
                  secondaryResult.isCorrect
                    ? "bg-emerald-500/10 border-emerald-500/35 text-emerald-900 dark:text-emerald-200"
                    : "bg-rose-500/10 border-rose-500/35 text-rose-900 dark:text-rose-200"
                }`}
              >
                {secondaryResult.isCorrect ? (
                  <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0 mt-0.5" />
                ) : (
                  <XCircle className="w-4 h-4 text-rose-600 shrink-0 mt-0.5" />
                )}
                <div className="space-y-1">
                  <div className="font-bold">
                    % Tăng lại để hòa vốn chuẩn: {problem.secondaryExpected}{" "}
                    {problem.secondaryUnit} · Sai số: {secondaryResult.relErrorPercent}%
                  </div>
                  <p>{secondaryResult.feedbackVi}</p>
                </div>
              </div>
            )}
          </div>

          <div className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-950 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 space-y-2 text-xs">
            <div className="font-bold text-indigo-600 dark:text-indigo-400 font-mono">
              Công thức: {problem.formulaVi}
            </div>
            <ul className="space-y-1.5 text-[var(--text-secondary,#3f3f46)] dark:text-zinc-300">
              {problem.stepsVi.map((step, i) => (
                <li key={i} className="leading-relaxed">
                  {step}
                </li>
              ))}
            </ul>
          </div>
        </div>
      )}
    </div>
  );
}

// ============================================================================
// 2. FINANCE CASE STUDY + WRITE-IN REASONING (CHẤM AI / TỪ KHÓA OFFLINE)
// ============================================================================

export function FinanceCaseWriteInSection({
  dbCases,
  lessonId,
  courseTitle,
  lessonTitle,
  initialActivityKey,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: {
  dbCases: FinanceCaseWriteInItem[];
  lessonId?: string | null;
  courseTitle: string;
  lessonTitle: string;
  initialActivityKey?: string | null;
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
}) {
  const allCases = useMemo(() => {
    const seen = new Set<string>();
    const list: FinanceCaseWriteInItem[] = [];
    for (const c of [...dbCases, ...DEFAULT_FINANCE_CASES]) {
      if (!seen.has(c.id)) {
        seen.add(c.id);
        list.push(c);
      }
    }
    return list;
  }, [dbCases]);

  const initialIdx = useMemo(() => {
    if (!initialActivityKey) return 0;
    const idx = allCases.findIndex(
      (c) =>
        c.id === initialActivityKey ||
        `finance-case-${c.id}` === initialActivityKey ||
        c.sourceActivityId === initialActivityKey
    );
    return idx >= 0 ? idx : 0;
  }, [allCases, initialActivityKey]);

  const [selectedIdx, setSelectedIdx] = useState(initialIdx);
  useEffect(() => {
    setSelectedIdx(initialIdx);
  }, [initialIdx]);

  const activeCase = allCases[selectedIdx] || allCases[0];
  const [chosenOpt, setChosenOpt] = useState<number | null>(null);
  const [reasoning, setReasoning] = useState("");
  const [isEvaluating, setIsEvaluating] = useState(false);
  const [evalRes, setEvalRes] = useState<{
    totalScore: number;
    isPassed: boolean;
    choiceCorrect: boolean;
    aiResult: AiPracticeEvaluationResult;
  } | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  useEffect(() => {
    setChosenOpt(null);
    setReasoning("");
    setEvalRes(null);
  }, [activeCase.id]);

  const handleSaveCard = async (isAuto = false) => {
    const front = `[Tình huống Đầu tư] ${activeCase.title}`;
    const back = `**Hướng xử lý đúng:** ${activeCase.options[activeCase.correctIndex]}\n\n**Lập luận chuẩn:** ${activeCase.sampleReasoningVi || activeCase.explanation}`;
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["dau-tu", "tinh-huong", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setToast(
        isAuto
          ? res.deduplicated
            ? "Tình huống này đã có sẵn trong Flashcard FSRS."
            : "Đã tự động lưu tình huống làm sai vào Flashcard FSRS!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu tình huống vào Flashcard FSRS!"
      );
      setTimeout(() => setToast(null), 4000);
    }
  };

  const handleSubmit = async () => {
    if (chosenOpt === null || !reasoning.trim() || isEvaluating) return;
    setIsEvaluating(true);

    const headers = getClientRequestHeaders();
    const aiResult = await evaluatePracticeWithAiOrOffline({
      mode: "finance_case",
      lang: "vi-VN",
      prompt: `Tình huống đầu tư: ${activeCase.scenario}\nPhương án học viên chọn: ${activeCase.options[chosenOpt]}\nĐáp án chuẩn: ${activeCase.options[activeCase.correctIndex]}`,
      userResponse: reasoning.trim(),
      expectedKeywords: activeCase.expectedKeywords,
      sampleAnswer: activeCase.sampleReasoningVi || activeCase.explanation,
      contextTitle: `${courseTitle} · ${activeCase.title}`,
      minWords: activeCase.minWords,
      headers,
    });

    setIsEvaluating(false);

    const choiceCorrect = chosenOpt === activeCase.correctIndex;
    const totalScore = Math.max(
      0,
      Math.min(100, (choiceCorrect ? 40 : 0) + Math.round(aiResult.score * 0.6))
    );
    const passing = activeCase.passingScore ?? 70;
    const isPassed = choiceCorrect && totalScore >= passing;

    setEvalRes({ totalScore, isPassed, choiceCorrect, aiResult });

    if (onRecordAttempt) {
      await onRecordAttempt({
        activityType: "investment_case",
        mode: "investment_case",
        activityId: activeCase.sourceActivityId || null,
        score: totalScore,
        maxScore: 100,
        isPassed,
        activityKey: `finance-case-${activeCase.id}`,
        activityTitle: activeCase.title,
        details: {
          caseId: activeCase.id,
          chosenIndex: chosenOpt,
          correctIndex: activeCase.correctIndex,
          gradingMode: aiResult.gradingMode,
          usedLiveEndpoint: aiResult.usedLiveEndpoint,
          reasoningScore: aiResult.score,
        },
      });
    }

    if (!isPassed) {
      await handleSaveCard(true);
    }
  };

  return (
    <div
      data-testid="finance-case-writein-section"
      className="p-4 sm:p-5 rounded-2xl border border-purple-500/30 bg-purple-500/5 space-y-4"
    >
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 pb-3 border-b border-purple-500/20">
        <div>
          <span className="text-xs font-bold text-purple-700 dark:text-purple-300 uppercase tracking-wider">
            Tình Huống &amp; Tự Viết Lập Luận (Active Recall)
          </span>
          <h4 className="text-sm sm:text-base font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 mt-0.5">
            {activeCase.title}
          </h4>
        </div>

        <select
          data-testid="finance-case-select"
          value={selectedIdx}
          onChange={(e) => setSelectedIdx(Number(e.target.value))}
          className="min-h-[40px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs font-medium cursor-pointer shrink-0"
        >
          {allCases.map((c, idx) => (
            <option key={c.id} value={idx}>
              Tình huống #{idx + 1}: {c.title.slice(0, 48)}
            </option>
          ))}
        </select>
      </div>

      {toast && (
        <div className="p-2.5 rounded-lg bg-emerald-500/10 border border-emerald-500/30 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
          <Sparkles className="w-4 h-4 shrink-0" />
          <span>{toast}</span>
        </div>
      )}

      <p className="p-3.5 rounded-xl bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 text-xs sm:text-sm leading-relaxed text-[var(--text-primary,#18181b)] dark:text-zinc-200">
        {activeCase.scenario}
      </p>

      {/* Step 1: Choose action */}
      <div className="space-y-2">
        <span className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block">
          Bước 1: Bạn nên làm gì trong tình huống này?
        </span>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
          {activeCase.options.map((opt, idx) => {
            const selected = chosenOpt === idx;
            return (
              <button
                key={idx}
                type="button"
                data-testid={`finance-case-opt-${idx}`}
                onClick={() => {
                  setChosenOpt(idx);
                  setEvalRes(null);
                }}
                className={`min-h-[48px] p-3 rounded-xl border text-left text-xs transition cursor-pointer ${
                  selected
                    ? "border-purple-600 bg-purple-500/15 text-purple-950 dark:text-purple-100 font-semibold"
                    : "border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-zinc-700 dark:text-zinc-300 hover:border-purple-400"
                }`}
              >
                {opt}
              </button>
            );
          })}
        </div>
      </div>

      {/* Step 2: Write-in reasoning */}
      <div className="space-y-1.5">
        <label
          htmlFor="finance-case-reasoning-input"
          className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block"
        >
          Bước 2: {activeCase.reasonPromptVi}
        </label>
        <textarea
          id="finance-case-reasoning-input"
          data-testid="finance-case-reasoning-input"
          rows={3}
          value={reasoning}
          onChange={(e) => {
            setReasoning(e.target.value);
            setEvalRes(null);
          }}
          placeholder="Viết lập luận của bạn bằng tiếng Việt (nêu rõ nguyên lý tài chính, rủi ro, lợi ích dài hạn)..."
          className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-950 text-xs sm:text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-purple-500"
        />
      </div>

      <div className="flex flex-wrap items-center gap-2.5">
        <button
          type="button"
          data-testid="finance-case-submit-btn"
          disabled={chosenOpt === null || !reasoning.trim() || isEvaluating}
          onClick={() => void handleSubmit()}
          className="min-h-[44px] px-5 py-2 rounded-xl bg-purple-600 hover:bg-purple-700 disabled:opacity-50 text-white text-xs sm:text-sm font-semibold inline-flex items-center gap-2 cursor-pointer shadow-xs"
        >
          {isEvaluating ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
          <span>{isEvaluating ? "Đang chấm lập luận..." : "Kiểm tra & Chấm lập luận"}</span>
        </button>

        <button
          type="button"
          onClick={() => void handleSaveCard(false)}
          className="min-h-[44px] px-3.5 py-2 rounded-xl border border-purple-300 dark:border-purple-700 bg-purple-500/10 text-purple-800 dark:text-purple-200 text-xs font-semibold inline-flex items-center gap-1.5 cursor-pointer"
        >
          <Sparkles className="w-3.5 h-3.5" />
          <span>Lưu vào Flashcard FSRS</span>
        </button>
      </div>

      {evalRes && (
        <div
          data-testid="finance-case-result"
          className="p-4 rounded-xl bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 space-y-3 text-xs"
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span
              data-testid="finance-case-grading-badge"
              className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full font-bold border ${
                evalRes.aiResult.usedLiveEndpoint
                  ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border-emerald-500/30"
                  : "bg-amber-500/15 text-amber-800 dark:text-amber-200 border-amber-500/35"
              }`}
            >
              <Sparkles className="w-3.5 h-3.5" />
              <span>{evalRes.aiResult.gradingLabelVi}</span>
            </span>

            <span
              className={`px-3 py-1 rounded-full font-extrabold ${
                evalRes.isPassed
                  ? "bg-emerald-600 text-white"
                  : "bg-rose-600 text-white"
              }`}
            >
              Tổng điểm: {evalRes.totalScore}/100
            </span>
          </div>

          <div
            className={`p-3 rounded-lg border ${
              evalRes.choiceCorrect
                ? "bg-emerald-500/10 border-emerald-500/30 text-emerald-900 dark:text-emerald-200"
                : "bg-rose-500/10 border-rose-500/30 text-rose-900 dark:text-rose-200"
            }`}
          >
            <strong>
              {evalRes.choiceCorrect
                ? "✅ Hướng xử lý chính xác!"
                : `❌ Hướng xử lý tối ưu là: "${activeCase.options[activeCase.correctIndex]}"`}
            </strong>
          </div>

          <p className="text-[var(--text-primary,#18181b)] dark:text-zinc-200 leading-relaxed">
            {evalRes.aiResult.feedbackVi}
          </p>

          {evalRes.aiResult.matchedKeywords.length > 0 && (
            <div className="flex flex-wrap items-center gap-1.5">
              <span className="font-semibold text-emerald-700 dark:text-emerald-300">
                Từ khóa đã chạm:
              </span>
              {evalRes.aiResult.matchedKeywords.map((kw) => (
                <span
                  key={kw}
                  className="px-2 py-0.5 rounded bg-emerald-500/15 text-emerald-800 dark:text-emerald-200 font-semibold"
                >
                  {kw}
                </span>
              ))}
            </div>
          )}

          {evalRes.aiResult.missingKeywords.length > 0 && (
            <div className="flex flex-wrap items-center gap-1.5">
              <span className="font-semibold text-amber-700 dark:text-amber-300">
                Ý nên bổ sung:
              </span>
              {evalRes.aiResult.missingKeywords.map((kw) => (
                <span
                  key={kw}
                  className="px-2 py-0.5 rounded bg-amber-500/15 text-amber-800 dark:text-amber-200 font-semibold"
                >
                  {kw}
                </span>
              ))}
            </div>
          )}

          <div className="p-3 rounded-lg bg-indigo-500/10 border border-indigo-500/25 text-indigo-950 dark:text-indigo-100">
            <strong>Lập luận chuẩn tham khảo:</strong>{" "}
            {activeCase.sampleReasoningVi || activeCase.explanation}
          </div>

          <button
            type="button"
            onClick={() =>
              void onSaveToNotebook(
                `### 💼 Tình huống Đầu tư: ${activeCase.title}\n- **Quyết định:** ${activeCase.options[chosenOpt ?? 0]}\n- **Lập luận của tôi:** ${reasoning}\n- **Điểm:** ${evalRes.totalScore}/100 (${evalRes.aiResult.gradingLabelVi})`
              )
            }
            className="min-h-[38px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 text-xs font-medium inline-flex items-center gap-1.5 cursor-pointer"
          >
            <BookmarkPlus className="w-3.5 h-3.5 text-purple-500" />
            <span>Lưu phân tích vào Sổ tay</span>
          </button>
        </div>
      )}
    </div>
  );
}

// ============================================================================
// 3. DECISION JOURNAL LAB (NHẬT KÝ QUYẾT ĐỊNH ĐẦU TƯ 4 TRƯỜNG)
// ============================================================================

export function DecisionJournalLab({
  dbPrompts,
  lessonId,
  courseTitle,
  lessonTitle,
  initialActivityKey,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: {
  dbPrompts: DecisionJournalPromptItem[];
  lessonId?: string | null;
  courseTitle: string;
  lessonTitle: string;
  initialActivityKey?: string | null;
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
}) {
  const allPrompts = useMemo(() => {
    return dbPrompts.length > 0
      ? dbPrompts
      : [
          {
            id: "dj-default",
            title: "Nhật Ký Quyết Định Đầu Tư (4 Trụ Cột Kỷ Luật)",
            scenario: `Áp dụng kiến thức bài "${lessonTitle}" để viết nhật ký thẩm định cho 1 quyết định đầu tư / phân bổ vốn thực tế hoặc giả định của bạn trước khi bấm lệnh.`,
            expectedKeywords: ["biên an toàn", "dòng tiền", "rủi ro", "cắt lỗ", "kỷ luật", "dài hạn"],
            sampleThesisVi:
              "Luận điểm dựa trên tăng trưởng dòng tiền bền vững; định giá có biên an toàn tối thiểu 25%; xác định rõ kịch bản sai và mức phân bổ vốn tối đa 15%; cam kết không mua đuổi FOMO và tuân thủ kỷ luật DCA.",
          },
        ];
  }, [dbPrompts, lessonTitle]);

  const initialIdx = useMemo(() => {
    if (!initialActivityKey) return 0;
    const idx = allPrompts.findIndex(
      (p) =>
        p.id === initialActivityKey ||
        `finance-decision-journal-${p.id}` === initialActivityKey ||
        p.sourceActivityId === initialActivityKey
    );
    return idx >= 0 ? idx : 0;
  }, [allPrompts, initialActivityKey]);

  const [selectedIdx, setSelectedIdx] = useState(initialIdx);
  useEffect(() => {
    setSelectedIdx(initialIdx);
  }, [initialIdx]);

  const promptItem = allPrompts[selectedIdx] || allPrompts[0];

  const [assetName, setAssetName] = useState("");
  const [coreThesis, setCoreThesis] = useState("");
  const [valuationMos, setValuationMos] = useState("");
  const [bearCaseRisk, setBearCaseRisk] = useState("");
  const [emotionalDiscipline, setEmotionalDiscipline] = useState("");
  const [isEvaluating, setIsEvaluating] = useState(false);
  const [result, setResult] = useState<AiPracticeEvaluationResult | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  useEffect(() => {
    setAssetName("");
    setCoreThesis("");
    setValuationMos("");
    setBearCaseRisk("");
    setEmotionalDiscipline("");
    setResult(null);
  }, [promptItem.id]);

  const handleSaveCard = async (isAuto = false) => {
    const front = `[Kỷ luật Đầu tư] 4 câu hỏi bắt buộc trong Nhật ký Quyết định trước khi mua tài sản?`;
    const back =
      "1. Luận điểm cốt lõi (tại sao tài sản này tạo giá trị?)\n2. Định giá & Biên an toàn (giá hiện tại chiết khấu bao nhiêu so với giá trị nội tại?)\n3. Kịch bản sai & Quản trị rủi ro (khi nào luận điểm sai, cắt lỗ/tỷ trọng tối đa bao nhiêu?)\n4. Kỷ luật cảm xúc (tránh FOMO, Loss Aversion và neo giá quá khứ).";
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["dau-tu", "nhat-ky-quyet-dinh", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setToast(
        isAuto
          ? res.deduplicated
            ? "Checklist Nhật ký quyết định đã có sẵn trong Flashcard FSRS."
            : "Đã tự động lưu checklist 4 trụ cột vào Flashcard FSRS để ôn lại!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu checklist 4 trụ cột vào Flashcard FSRS!"
      );
      setTimeout(() => setToast(null), 4000);
    }
  };

  const handleEvaluateAndSave = async () => {
    const fields = [
      { label: "Luận điểm cốt lõi", value: coreThesis, minWords: 6, keywords: ["dòng tiền", "giá trị", "lợi thế", "tăng trưởng"] },
      { label: "Định giá & Biên an toàn", value: valuationMos, minWords: 6, keywords: ["biên an toàn", "định giá", "p/e", "chiết khấu"] },
      { label: "Kịch bản sai & Quản trị rủi ro", value: bearCaseRisk, minWords: 6, keywords: ["rủi ro", "cắt lỗ", "tỷ trọng", "kịch bản sai"] },
      { label: "Kỷ luật cảm xúc", value: emotionalDiscipline, minWords: 6, keywords: ["kỷ luật", "fomo", "cảm xúc", "dca"] },
    ];

    const combinedResponse = fields
      .map((f) => `${f.label}: ${f.value.trim() || "(chưa điền)"}`)
      .join("\n");

    if (!coreThesis.trim() && !valuationMos.trim() && !bearCaseRisk.trim() && !emotionalDiscipline.trim()) {
      setToast("Vui lòng điền ít nhất các trường lập luận trước khi chấm và lưu.");
      setTimeout(() => setToast(null), 3000);
      return;
    }

    setIsEvaluating(true);
    const headers = getClientRequestHeaders();
    const aiEval = await evaluatePracticeWithAiOrOffline({
      mode: "decision_journal",
      lang: "vi-VN",
      prompt: `Đánh giá chất lượng Nhật ký quyết định đầu tư 4 trụ cột cho tài sản "${assetName || "Danh mục cá nhân"}":\n${promptItem.scenario}`,
      userResponse: combinedResponse,
      expectedKeywords: promptItem.expectedKeywords,
      sampleAnswer: promptItem.sampleThesisVi,
      contextTitle: `${courseTitle} · Nhật ký quyết định`,
      minWords: 24,
      headers,
    });
    setIsEvaluating(false);

    // If offline, enrich with structured 4-field rubric score so empty fields are strictly penalized
    let finalEval = aiEval;
    if (!aiEval.usedLiveEndpoint) {
      finalEval = evaluateJournalCriteriaOffline(
        fields,
        promptItem.expectedKeywords,
        promptItem.sampleThesisVi
      );
      finalEval.gradingLabelVi = WRITEIN_GRADING_LABELS.keyword_offline;
    }
    setResult(finalEval);

    const passing = promptItem.passingScore ?? 70;
    const isPassed = finalEval.score >= passing;

    // Save to Notebook
    const md = `### 📓 Nhật Ký Quyết Định Đầu Tư: ${assetName || lessonTitle}
- **Bài học:** ${lessonTitle}
- **1. Luận điểm cốt lõi:** ${coreThesis || "(chưa điền)"}
- **2. Định giá & Biên an toàn:** ${valuationMos || "(chưa điền)"}
- **3. Kịch bản sai & Quản trị rủi ro:** ${bearCaseRisk || "(chưa điền)"}
- **4. Kỷ luật cảm xúc:** ${emotionalDiscipline || "(chưa điền)"}
- **Đánh giá (${finalEval.gradingLabelVi}):** ${finalEval.score}/100 — ${finalEval.feedbackVi}`;

    try {
      await onSaveToNotebook(md);
    } catch {
      // non-blocking for attempt recording
    }

    if (onRecordAttempt) {
      await onRecordAttempt({
        activityType: "decision_journal",
        mode: "decision_journal",
        activityId: promptItem.sourceActivityId || null,
        score: finalEval.score,
        maxScore: 100,
        isPassed,
        activityKey: `finance-decision-journal-${promptItem.id}`,
        activityTitle: promptItem.title,
        details: {
          assetName,
          gradingMode: finalEval.gradingMode,
          usedLiveEndpoint: finalEval.usedLiveEndpoint,
          score: finalEval.score,
        },
      });
    }

    if (!isPassed) {
      await handleSaveCard(true);
    } else {
      setToast("Đã đánh giá và lưu Nhật ký Quyết định vào Sổ tay!");
      setTimeout(() => setToast(null), 3500);
    }
  };

  return (
    <div data-testid="finance-decision-journal-lab" className="space-y-5">
      <div className="border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 pb-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 flex items-center gap-2">
            <FileText className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
            {promptItem.title}
          </h2>
          <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400 mt-1">
            {promptItem.scenario}
          </p>
        </div>

        {allPrompts.length > 1 && (
          <select
            data-testid="decision-journal-select"
            value={selectedIdx}
            onChange={(e) => setSelectedIdx(Number(e.target.value))}
            className="min-h-[40px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-950 text-xs font-medium cursor-pointer shrink-0"
          >
            {allPrompts.map((it, idx) => (
              <option key={it.id} value={idx}>
                Đề #{idx + 1}: {it.title.slice(0, 44)}
              </option>
            ))}
          </select>
        )}
      </div>

      {toast && (
        <div className="p-3 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
          <Sparkles className="w-4 h-4 shrink-0" />
          <span>{toast}</span>
        </div>
      )}

      <div className="space-y-4">
        <div>
          <label className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block mb-1">
            Tài sản / Kế hoạch đang thẩm định (tùy chọn):
          </label>
          <input
            type="text"
            data-testid="dj-asset-input"
            value={assetName}
            onChange={(e) => setAssetName(e.target.value)}
            placeholder="Ví dụ: Quỹ ETF VN30 (DCA 5 triệu/tháng) hoặc Cổ phiếu FPT..."
            className="w-full min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs sm:text-sm"
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1.5">
            <label className="text-xs font-bold text-indigo-700 dark:text-indigo-300 block">
              1. Luận điểm cốt lõi (Core Thesis)
            </label>
            <textarea
              data-testid="dj-input-thesis"
              rows={4}
              value={coreThesis}
              onChange={(e) => setCoreThesis(e.target.value)}
              placeholder="Lợi thế cạnh tranh, năng lực tạo dòng tiền và động lực tăng trưởng dài hạn là gì?"
              className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs sm:text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-bold text-emerald-700 dark:text-emerald-300 block">
              2. Định giá &amp; Biên an toàn (Valuation &amp; Margin of Safety)
            </label>
            <textarea
              data-testid="dj-input-valuation"
              rows={4}
              value={valuationMos}
              onChange={(e) => setValuationMos(e.target.value)}
              placeholder="P/E hiện tại so với trung bình? Giá mua đang chiết khấu bao nhiêu % biên an toàn?"
              className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs sm:text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-bold text-rose-700 dark:text-rose-300 block">
              3. Kịch bản sai &amp; Quản trị rủi ro (Bear Case &amp; Risk)
            </label>
            <textarea
              data-testid="dj-input-risk"
              rows={4}
              value={bearCaseRisk}
              onChange={(e) => setBearCaseRisk(e.target.value)}
              placeholder="Điều gì xảy ra nếu nhận định sai? Tỷ trọng tối đa & nguyên tắc cắt lỗ/tái cân bằng là gì?"
              className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs sm:text-sm focus:outline-none focus:ring-2 focus:ring-rose-500"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-bold text-amber-700 dark:text-amber-300 block">
              4. Kỷ luật cảm xúc (Emotional Discipline)
            </label>
            <textarea
              data-testid="dj-input-discipline"
              rows={4}
              value={emotionalDiscipline}
              onChange={(e) => setEmotionalDiscipline(e.target.value)}
              placeholder="Bạn sẽ hành động thế nào nếu giá giảm 20% tuần sau? Cách tự phòng vệ trước FOMO & hoảng loạn?"
              className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs sm:text-sm focus:outline-none focus:ring-2 focus:ring-amber-500"
            />
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2.5">
          <button
            type="button"
            data-testid="dj-submit-btn"
            disabled={isEvaluating}
            onClick={() => void handleEvaluateAndSave()}
            className="min-h-[44px] px-5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs sm:text-sm font-semibold inline-flex items-center gap-2 cursor-pointer shadow-xs"
          >
            {isEvaluating ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
            <span>{isEvaluating ? "Đang chấm lập luận..." : "Chấm lập luận & Lưu vào Sổ tay"}</span>
          </button>

          <button
            type="button"
            onClick={() => void handleSaveCard(false)}
            className="min-h-[44px] px-3.5 py-2 rounded-xl border border-indigo-300 dark:border-indigo-700 bg-indigo-500/10 text-indigo-700 dark:text-indigo-300 text-xs font-semibold inline-flex items-center gap-1.5 cursor-pointer"
          >
            <Sparkles className="w-3.5 h-3.5" />
            <span>Lưu vào Flashcard FSRS</span>
          </button>
        </div>
      </div>

      {result && (
        <div
          data-testid="dj-result-card"
          className="p-4 sm:p-5 rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-900/70 space-y-3 text-xs"
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span
              data-testid="dj-grading-badge"
              className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full font-bold border ${
                result.usedLiveEndpoint
                  ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border-emerald-500/30"
                  : "bg-amber-500/15 text-amber-800 dark:text-amber-200 border-amber-500/35"
              }`}
            >
              <Sparkles className="w-3.5 h-3.5" />
              <span>{result.gradingLabelVi}</span>
            </span>

            <span
              className={`px-3 py-1 rounded-full font-extrabold ${
                result.score >= 70 ? "bg-emerald-600 text-white" : "bg-amber-600 text-white"
              }`}
            >
              Điểm lập luận: {result.score}/100
            </span>
          </div>

          <p className="text-[var(--text-primary,#18181b)] dark:text-zinc-200 leading-relaxed">
            {result.feedbackVi}
          </p>

          {(result.strengths.length > 0 || result.improvements.length > 0) && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {result.strengths.length > 0 && (
                <div className="p-3 rounded-lg bg-emerald-500/10 border border-emerald-500/25 space-y-1">
                  <div className="font-bold text-emerald-800 dark:text-emerald-300">Điểm mạnh</div>
                  <ul className="list-disc list-inside space-y-0.5">
                    {result.strengths.map((s, i) => (
                      <li key={i}>{s}</li>
                    ))}
                  </ul>
                </div>
              )}
              {result.improvements.length > 0 && (
                <div className="p-3 rounded-lg bg-amber-500/10 border border-amber-500/25 space-y-1">
                  <div className="font-bold text-amber-800 dark:text-amber-300">Cần bổ sung</div>
                  <ul className="list-disc list-inside space-y-0.5">
                    {result.improvements.map((s, i) => (
                      <li key={i}>{s}</li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}

          {result.score < 70 && (
            <div className="flex items-center gap-2 text-amber-700 dark:text-amber-300">
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>Điểm dưới 70 — bài này đã được đưa vào hàng ôn &quot;Hôm nay cần ôn gì&quot; tại Hub Luyện tập.</span>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
