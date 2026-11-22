"use client";

import React, { useEffect, useMemo, useState } from "react";
import {
  AlertTriangle,
  BookmarkPlus,
  CheckCircle2,
  Code2,
  Cpu,
  Eye,
  EyeOff,
  HelpCircle,
  Loader2,
  Play,
  RotateCcw,
  Sparkles,
  Terminal,
  Wrench,
  XCircle,
} from "lucide-react";
import {
  evaluateBugFixStatic,
  evaluatePredictOutput,
  type BugFixStaticEvaluationResult,
  type BugFixWriteInItem,
  type PredictOutputEvaluationResult,
  type PredictOutputItem,
} from "@/lib/writein-practice";
import {
  runJsFunctionInWorker,
  runJsPredictOutputInWorker,
  saveWriteInFlashcardDeduplicated,
} from "./WriteInFsrsHelper";

export const DEFAULT_PREDICT_OUTPUT_ITEMS: PredictOutputItem[] = [
  {
    id: "pred-js-map-filter-reduce",
    title: "1. Pipeline filter → map → reduce & Tính bất biến",
    difficulty: "Dễ",
    description: "Dự đoán chính xác 2 dòng mà console.log sẽ in ra (mỗi dòng trên 1 hàng).",
    code: `const nums = [1, 2, 3, 4, 5];
const total = nums
  .filter((n) => n % 2 === 0)
  .map((n) => n * 10)
  .reduce((acc, n) => acc + n, 0);
console.log(total);
console.log(nums.length);`,
    expectedOutput: "60\n5",
    hint: "filter giữ lại các số chẵn [2, 4], map nhân 10 thành [20, 40], reduce cộng tổng. Mảng gốc nums không bị thay đổi.",
    explanation:
      "filter([1,2,3,4,5]) trả về [2, 4], map(n => n * 10) thành [20, 40], reduce cộng lại ra 60. Cả 3 phương thức đều bất biến nên nums.length vẫn bằng 5.",
  },
  {
    id: "pred-js-reference-vs-spread",
    title: "2. Shallow Copy với Spread & Tham chiếu Object lồng nhau",
    difficulty: "Trung bình",
    description: "Dự đoán output khi sửa thuộc tính lồng bên trong object sau khi sao chép nông bằng {...user}.",
    code: `const user = { name: "An", meta: { score: 10 } };
const copy = { ...user };
copy.name = "Bình";
copy.meta.score = 99;
console.log(user.name);
console.log(user.meta.score);`,
    expectedOutput: "An\n99",
    hint: "Spread {...user} tạo bản sao mới cho tầng 1 (name), nhưng tầng 2 (meta) vẫn trỏ cùng tham chiếu bộ nhớ.",
    explanation:
      "Gán copy.name = 'Bình' không ảnh hưởng user.name (vẫn là An). Nhưng copy.meta và user.meta cùng trỏ đến một object trong heap nên user.meta.score đổi thành 99.",
  },
  {
    id: "pred-js-event-loop-order",
    title: "3. Event Loop: Đồng bộ vs Microtask (Promise) vs Macrotask (setTimeout)",
    difficulty: "Nâng cao",
    description: "Dự đoán thứ tự 3 dòng console.log khi có Promise.resolve().then và setTimeout 0ms.",
    code: `console.log("A");
Promise.resolve().then(() => console.log("B"));
console.log("C");`,
    expectedOutput: "A\nC\nB",
    hint: "Call Stack chạy hết lệnh đồng bộ trước khi xả hàng đợi Microtask (Promise.then).",
    explanation:
      "Lệnh đồng bộ 'A' và 'C' chạy ngay trong Call Stack. Sau đó Event Loop mới lấy callback trong Microtask Queue để in ra 'B'.",
  },
];

export const DEFAULT_JS_BUG_FIX_ITEMS: BugFixWriteInItem[] = [
  {
    id: "bugfix-js-reduce-initial",
    title: "JS Bug Fix: Tính tổng đơn hàng bị lỗi khi mảng rỗng hoặc thiếu giá trị khởi tạo",
    language: "javascript",
    categoryTag: "JavaScript Array & Reduce",
    context:
      "Hàm sumCompleted(orders) cần cộng thuộc tính amount của các đơn có status === 'paid'. Bản hiện tại trả về sai kiểu (object/NaN) hoặc crash khi mảng rỗng.",
    buggyCode: `function sumCompleted(orders) {
  return orders
    .filter((o) => o.status === "paid")
    .reduce((sum, o) => sum + o.amount);
}`,
    bugLineNumber: 4,
    bugBriefDescription: "Dòng 4: Thiếu giá trị khởi tạo 0 trong reduce((sum, o) => ..., 0)",
    rootCauseAnalysis:
      "Khi không truyền initialValue = 0, reduce lấy phần tử đầu tiên (là object { amount, status }) làm biến tích lũy sum, dẫn đến '[object Object]300', và ném TypeError nếu mảng lọc rỗng.",
    realWorldImpact:
      "Trang báo cáo doanh thu bị trắng màn hình mỗi khi người dùng lọc ngày không có đơn hàng.",
    cleanFixedCode: `function sumCompleted(orders) {
  return orders
    .filter((o) => o.status === "paid")
    .reduce((sum, o) => sum + o.amount, 0);
}`,
    requiredTokens: ["reduce", ", 0", "o.amount"],
    functionName: "sumCompleted",
    testCases: [
      {
        name: "Đơn hỗn hợp paid & pending",
        input: [
          [
            { amount: 200, status: "paid" },
            { amount: 500, status: "pending" },
            { amount: 350, status: "paid" },
          ],
        ],
        expected: 550,
      },
      {
        name: "Không có đơn nào paid (mảng sau filter rỗng)",
        input: [[{ amount: 100, status: "pending" }]],
        expected: 0,
      },
      {
        name: "Mảng đầu vào rỗng []",
        input: [[]],
        expected: 0,
      },
    ],
    explanation:
      "Luôn truyền giá trị khởi tạo (ví dụ 0) làm tham số thứ 2 của Array.prototype.reduce khi cộng thuộc tính của object hoặc xử lý mảng có thể rỗng.",
  },
  {
    id: "bugfix-js-immutability-sort",
    title: "JS Bug Fix: Sắp xếp điểm số tăng dần sai thứ tự số học & đột biến mảng gốc",
    language: "javascript",
    categoryTag: "JavaScript Immutability & Sort",
    context:
      "Hàm sortScoresAsc(scores) cần trả về mảng mới được sắp xếp tăng dần theo giá trị số (ví dụ [10, 2, 30] -> [2, 10, 30]). Hiện tại JS sort mặc định theo chuỗi khiến 10 đứng trước 2!",
    buggyCode: `function sortScoresAsc(scores) {
  return scores.sort();
}`,
    bugLineNumber: 2,
    bugBriefDescription: "Dòng 2: Array.sort() không có comparator nên sắp theo mã Unicode chuỗi ('10' < '2') và đột biến mảng gốc.",
    rootCauseAnalysis:
      "Mặc định scores.sort() chuyển phần tử sang chuỗi UTF-16 nên '10' đứng trước '2'. Ngoài ra cần copy mảng `[...scores]` để tránh mutate dữ liệu đầu vào.",
    realWorldImpact:
      "Bảng xếp hạng học viên hiển thị sai thứ tự điểm (100 đứng trước 20) và gây lỗi React state mutation.",
    cleanFixedCode: `function sortScoresAsc(scores) {
  return [...scores].sort((a, b) => a - b);
}`,
    requiredTokens: ["a - b", "sort"],
    functionName: "sortScoresAsc",
    testCases: [
      { name: "Số có 1 & 2 chữ số: [10, 2, 30, 5]", input: [[10, 2, 30, 5]], expected: [2, 5, 10, 30] },
      { name: "Số âm & 0: [3, -1, 0, -10]", input: [[3, -1, 0, -10]], expected: [-10, -1, 0, 3] },
      { name: "Mảng rỗng []", input: [[]], expected: [] },
    ],
    explanation:
      "Truyền hàm so sánh `(a, b) => a - b` để sắp xếp số tăng dần và dùng `[...scores]` (hoặc `scores.slice()`) để bảo toàn tính bất biến.",
  },
];

// ============================================================================
// PREDICT OUTPUT LAB (WEB WORKER SANDBOX)
// ============================================================================

export function PredictOutputLab({
  dbItems,
  lessonId,
  courseTitle,
  lessonTitle,
  initialActivityKey,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: {
  dbItems: PredictOutputItem[];
  lessonId?: string | null;
  courseTitle: string;
  lessonTitle: string;
  initialActivityKey?: string | null;
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
}) {
  const allItems = useMemo(() => {
    const seen = new Set<string>();
    const merged: PredictOutputItem[] = [];
    for (const item of [...dbItems, ...DEFAULT_PREDICT_OUTPUT_ITEMS]) {
      if (!seen.has(item.id)) {
        seen.add(item.id);
        merged.push(item);
      }
    }
    return merged;
  }, [dbItems]);

  const initialIdx = useMemo(() => {
    if (!initialActivityKey) return 0;
    const found = allItems.findIndex(
      (x) =>
        x.id === initialActivityKey ||
        `predict-output-${x.id}` === initialActivityKey ||
        x.sourceActivityId === initialActivityKey
    );
    return found >= 0 ? found : 0;
  }, [allItems, initialActivityKey]);

  const [selectedIdx, setSelectedIdx] = useState(initialIdx);
  useEffect(() => {
    setSelectedIdx(initialIdx);
  }, [initialIdx]);

  const activeItem = allItems[selectedIdx] || allItems[0];

  const [prediction, setPrediction] = useState("");
  const [showHint, setShowHint] = useState(false);
  const [isRunning, setIsRunning] = useState(false);
  const [workerError, setWorkerError] = useState<string | null>(null);
  const [actualOutput, setActualOutput] = useState<string | null>(null);
  const [execMs, setExecMs] = useState<number | null>(null);
  const [evalResult, setEvalResult] = useState<PredictOutputEvaluationResult | null>(null);
  const [fsrsToast, setFsrsToast] = useState<string | null>(null);

  useEffect(() => {
    setPrediction("");
    setShowHint(false);
    setWorkerError(null);
    setActualOutput(null);
    setExecMs(null);
    setEvalResult(null);
  }, [activeItem.id]);

  const handleSaveCard = async (isAuto = false) => {
    const front = `[Đoán Output JS] ${activeItem.title}\nĐoạn code sau in ra gì?\n${activeItem.code.slice(0, 260)}`;
    const back = `**Output đúng:**\n${actualOutput ?? activeItem.expectedOutput}\n\n**Giải thích:**\n${activeItem.explanation}`;
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["lap-trinh", "predict-output", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setFsrsToast(
        isAuto
          ? res.deduplicated
            ? "Bài này đã nằm sẵn trong bộ Flashcard FSRS để ôn lại."
            : "Tự động lưu lỗi vào Flashcard FSRS để ôn lại sau!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu bài tập vào Flashcard FSRS!"
      );
      setTimeout(() => setFsrsToast(null), 4000);
    }
  };

  const handleCheckPrediction = async () => {
    if (!prediction.trim() || isRunning) return;
    setIsRunning(true);
    setWorkerError(null);

    const runRes = await runJsPredictOutputInWorker({
      code: activeItem.code,
      timeLimitMs: activeItem.timeLimitMs ?? 2000,
    });

    setIsRunning(false);
    setExecMs(runRes.executionTimeMs);

    if (!runRes.ok) {
      setWorkerError(runRes.error || "Không thể thực thi mã nguồn trong Web Worker.");
      return;
    }

    const realOut = runRes.output;
    setActualOutput(realOut);

    const comparison = evaluatePredictOutput(
      prediction,
      realOut,
      activeItem.acceptedOutputs
    );
    setEvalResult(comparison);

    const passing = activeItem.passingScore ?? 70;
    const isPassed = comparison.isCorrect || comparison.score >= passing;

    if (onRecordAttempt) {
      await onRecordAttempt({
        mode: "predict_output",
        activityId: activeItem.sourceActivityId || null,
        score: comparison.score,
        maxScore: 100,
        isPassed,
        activityKey: `predict-output-${activeItem.id}`,
        activityTitle: `${activeItem.title} · Đoán Output`,
        durationSeconds: Math.max(1, Math.round(runRes.executionTimeMs / 1000)),
        details: {
          itemId: activeItem.id,
          userPrediction: prediction,
          actualWorkerOutput: realOut,
          matchedLines: comparison.matchedLines,
          totalExpectedLines: comparison.totalExpectedLines,
        },
      });
    }

    if (!isPassed) {
      await handleSaveCard(true);
    }
  };

  return (
    <div data-testid="predict-output-container" className="p-4 sm:p-6 space-y-5">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
        <div>
          <span className="text-xs font-semibold text-indigo-600 dark:text-indigo-400 inline-flex items-center gap-1.5">
            <Cpu className="w-3.5 h-3.5" />
            Active Recall · Chạy kiểm chứng thực tế bằng Web Worker
          </span>
          <h3 className="text-base sm:text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 mt-0.5">
            {activeItem.title}
          </h3>
          <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400 mt-0.5">
            {activeItem.description}
          </p>
        </div>

        <div className="flex items-center gap-2 shrink-0">
          <select
            data-testid="predict-output-select"
            value={selectedIdx}
            onChange={(e) => setSelectedIdx(Number(e.target.value))}
            className="min-h-[40px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs font-medium text-[var(--text-primary,#18181b)] dark:text-zinc-100 cursor-pointer"
          >
            {allItems.map((it, idx) => (
              <option key={it.id} value={idx}>
                [{it.difficulty}] {it.title}
              </option>
            ))}
          </select>
        </div>
      </div>

      {fsrsToast && (
        <div className="p-3 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-800 dark:text-emerald-200 text-xs font-medium flex items-center gap-2">
          <Sparkles className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
          <span>{fsrsToast}</span>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-5">
        {/* Code block */}
        <div className="lg:col-span-7 space-y-3">
          <div className="rounded-xl border border-zinc-800 bg-zinc-950 text-zinc-100 overflow-hidden shadow-sm">
            <div className="px-4 py-2 bg-zinc-900 border-b border-zinc-800 flex items-center justify-between text-xs text-zinc-400">
              <span className="font-mono flex items-center gap-1.5">
                <Terminal className="w-3.5 h-3.5 text-indigo-400" />
                snippet.js (Chỉ đọc — hãy tự chạy nhẩm trong đầu trước!)
              </span>
              <span className="px-2 py-0.5 rounded-full bg-zinc-800 text-[11px] text-zinc-300">
                {activeItem.difficulty}
              </span>
            </div>
            <div className="p-4 font-mono text-xs sm:text-sm overflow-x-auto space-y-1">
              {activeItem.code.split("\n").map((line, idx) => (
                <div key={idx} className="flex items-start gap-3 leading-relaxed">
                  <span className="w-5 text-right text-zinc-600 select-none text-xs shrink-0">
                    {idx + 1}
                  </span>
                  <pre className="whitespace-pre text-emerald-300">{line}</pre>
                </div>
              ))}
            </div>
          </div>

          {activeItem.hint && (
            <div>
              <button
                type="button"
                onClick={() => setShowHint((v) => !v)}
                className="text-xs font-semibold text-indigo-600 dark:text-indigo-400 inline-flex items-center gap-1.5 hover:underline cursor-pointer"
              >
                <HelpCircle className="w-3.5 h-3.5" />
                <span>{showHint ? "Ẩn gợi ý tư duy" : "Xem gợi ý tư duy"}</span>
              </button>
              {showHint && (
                <div className="mt-2 p-3 rounded-xl bg-amber-500/10 border border-amber-500/30 text-xs text-amber-900 dark:text-amber-200">
                  💡 {activeItem.hint}
                </div>
              )}
            </div>
          )}
        </div>

        {/* Prediction input */}
        <div className="lg:col-span-5 space-y-3">
          <label
            htmlFor="predict-output-input"
            className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block"
          >
            Nhập Output bạn dự đoán (mỗi lần console.log một dòng):
          </label>
          <textarea
            id="predict-output-input"
            data-testid="predict-output-input"
            rows={6}
            value={prediction}
            onChange={(e) => {
              setPrediction(e.target.value);
              setEvalResult(null);
            }}
            placeholder={"Ví dụ:\n60\n5"}
            spellCheck={false}
            className="w-full p-3.5 rounded-xl font-mono text-xs sm:text-sm bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-900 border border-[var(--border-calm,#d4d4d8)] dark:border-zinc-700 text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />

          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              data-testid="predict-output-check-btn"
              disabled={!prediction.trim() || isRunning}
              onClick={() => void handleCheckPrediction()}
              className="min-h-[44px] px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs sm:text-sm font-semibold inline-flex items-center gap-2 transition cursor-pointer shadow-xs"
            >
              {isRunning ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : (
                <Play className="w-4 h-4" />
              )}
              <span>{isRunning ? "Đang chạy Web Worker..." : "Chạy thực tế & Kiểm tra"}</span>
            </button>

            <button
              type="button"
              onClick={() => {
                setPrediction("");
                setEvalResult(null);
                setActualOutput(null);
                setWorkerError(null);
              }}
              className="min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs font-medium text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800 inline-flex items-center gap-1.5 cursor-pointer"
            >
              <RotateCcw className="w-3.5 h-3.5" />
              <span>Làm lại</span>
            </button>
          </div>
        </div>
      </div>

      {workerError && (
        <div className="p-4 rounded-xl bg-rose-500/10 border border-rose-500/30 text-rose-800 dark:text-rose-200 text-xs space-y-1">
          <div className="font-bold flex items-center gap-1.5">
            <AlertTriangle className="w-4 h-4" />
            <span>Lỗi Web Worker Sandbox:</span>
          </div>
          <p className="font-mono">{workerError}</p>
        </div>
      )}

      {evalResult && actualOutput !== null && (
        <div
          data-testid="predict-output-result"
          className={`p-4 sm:p-5 rounded-xl border space-y-4 ${
            evalResult.isCorrect
              ? "bg-emerald-500/10 border-emerald-500/35"
              : "bg-amber-500/10 border-amber-500/35"
          }`}
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-2 font-bold text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100">
              {evalResult.isCorrect ? (
                <CheckCircle2 className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
              ) : (
                <XCircle className="w-5 h-5 text-amber-600 dark:text-amber-400" />
              )}
              <span>{evalResult.feedbackVi}</span>
            </div>
            <div className="flex items-center gap-2">
              {execMs !== null && (
                <span className="text-[11px] font-mono text-zinc-500 dark:text-zinc-400">
                  Worker: {execMs}ms
                </span>
              )}
              <span
                className={`px-3 py-1 rounded-full text-xs font-extrabold ${
                  evalResult.score >= 70
                    ? "bg-emerald-600 text-white"
                    : "bg-rose-600 text-white"
                }`}
              >
                {evalResult.score}/100 điểm
              </span>
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs font-mono">
            <div className="p-3 rounded-lg bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 space-y-1">
              <div className="font-sans font-bold text-zinc-600 dark:text-zinc-400">
                Dự đoán của bạn:
              </div>
              <pre className="whitespace-pre-wrap text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                {prediction.trim() || "(trống)"}
              </pre>
            </div>
            <div className="p-3 rounded-lg bg-zinc-950 text-emerald-300 border border-zinc-800 space-y-1">
              <div className="font-sans font-bold text-emerald-400">
                Output thực tế (Web Worker):
              </div>
              <pre className="whitespace-pre-wrap">{actualOutput || "(không in ra dòng nào)"}</pre>
            </div>
          </div>

          <div className="p-3.5 rounded-lg bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 text-xs text-[var(--text-secondary,#3f3f46)] dark:text-zinc-300 leading-relaxed">
            <strong className="text-[var(--text-primary,#18181b)] dark:text-zinc-100">
              Giải thích từng bước:{" "}
            </strong>
            {activeItem.explanation}
          </div>

          <div className="flex flex-wrap items-center gap-2 pt-1">
            <button
              type="button"
              data-testid="predict-output-save-fsrs-btn"
              onClick={() => void handleSaveCard(false)}
              className="min-h-[40px] px-3.5 py-1.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold inline-flex items-center gap-1.5 cursor-pointer"
            >
              <Sparkles className="w-3.5 h-3.5" />
              <span>Lưu vào Flashcard FSRS</span>
            </button>
            <button
              type="button"
              onClick={() =>
                void onSaveToNotebook(
                  `### 💻 Đoán Output JS: ${activeItem.title}\n\`\`\`javascript\n${activeItem.code}\n\`\`\`\n- **Output thực tế:** \`${actualOutput}\`\n- **Giải thích:** ${activeItem.explanation}`
                )
              }
              className="min-h-[40px] px-3.5 py-1.5 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs font-medium text-zinc-700 dark:text-zinc-300 inline-flex items-center gap-1.5 cursor-pointer"
            >
              <BookmarkPlus className="w-3.5 h-3.5 text-indigo-500" />
              <span>Lưu vào Sổ tay</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

// ============================================================================
// BUG FIX WRITE-IN PANEL (DIRECT CODE EDIT + WEB WORKER OR STATIC CHECK)
// ============================================================================

export function BugFixWriteInPanel({
  items,
  lessonId,
  courseTitle,
  lessonTitle,
  initialActivityKey,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: {
  items: BugFixWriteInItem[];
  lessonId?: string | null;
  courseTitle: string;
  lessonTitle: string;
  initialActivityKey?: string | null;
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
}) {
  const allItems = useMemo(() => {
    const seen = new Set<string>();
    const merged: BugFixWriteInItem[] = [];
    for (const item of [...items, ...DEFAULT_JS_BUG_FIX_ITEMS]) {
      if (!seen.has(item.id)) {
        seen.add(item.id);
        merged.push(item);
      }
    }
    return merged;
  }, [items]);

  const initialIdx = useMemo(() => {
    if (!initialActivityKey) return 0;
    const found = allItems.findIndex(
      (x) =>
        x.id === initialActivityKey ||
        `bug-fix-${x.id}` === initialActivityKey ||
        x.sourceActivityId === initialActivityKey
    );
    return found >= 0 ? found : 0;
  }, [allItems, initialActivityKey]);

  const [selectedIdx, setSelectedIdx] = useState(initialIdx);
  useEffect(() => {
    setSelectedIdx(initialIdx);
  }, [initialIdx]);

  const activeItem = allItems[selectedIdx] || allItems[0];
  const canRunWorker =
    (activeItem.language === "javascript" || activeItem.language === "js") &&
    Array.isArray(activeItem.testCases) &&
    activeItem.testCases.length > 0 &&
    Boolean(activeItem.functionName);

  const [editedCode, setEditedCode] = useState(activeItem.buggyCode);
  const [isRunning, setIsRunning] = useState(false);
  const [workerResults, setWorkerResults] = useState<Array<{
    name: string;
    passed: boolean;
    input: any[];
    actual: any;
    expected: any;
    error?: string;
  }> | null>(null);
  const [workerError, setWorkerError] = useState<string | null>(null);
  const [staticResult, setStaticResult] = useState<BugFixStaticEvaluationResult | null>(null);
  const [showSolution, setShowSolution] = useState(false);
  const [fsrsToast, setFsrsToast] = useState<string | null>(null);

  useEffect(() => {
    setEditedCode(activeItem.buggyCode);
    setWorkerResults(null);
    setWorkerError(null);
    setStaticResult(null);
    setShowSolution(false);
  }, [activeItem.id, activeItem.buggyCode]);

  const handleSaveCard = async (isAuto = false) => {
    const front = `[Sửa Bug Write-in] ${activeItem.title}\nLỗi gốc nằm ở đâu và sửa thế nào?`;
    const back = `**Nguyên nhân:** ${activeItem.explanation}\n\n**Bản sửa chuẩn:**\n\`\`\`${activeItem.language}\n${activeItem.cleanFixedCode}\n\`\`\``;
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["lap-trinh", "bug-fix", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setFsrsToast(
        isAuto
          ? res.deduplicated
            ? "Bài sửa bug này đã có trong Flashcard FSRS."
            : "Đã tự động lưu lỗi sửa bug vào Flashcard FSRS để ôn lại!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu bài sửa bug vào Flashcard FSRS!"
      );
      setTimeout(() => setFsrsToast(null), 4000);
    }
  };

  const handleCheckBugFix = async () => {
    setWorkerError(null);
    setWorkerResults(null);
    setStaticResult(null);

    let score = 0;
    let isPassed = false;
    let details: Record<string, unknown> = { itemId: activeItem.id, language: activeItem.language };

    if (canRunWorker) {
      setIsRunning(true);
      const runRes = await runJsFunctionInWorker({
        code: editedCode,
        functionName: activeItem.functionName!,
        testCases: activeItem.testCases!,
        timeLimitMs: 2000,
      });
      setIsRunning(false);

      if (!runRes.ok) {
        setWorkerError(runRes.error || "Mã sửa lỗi phát sinh ngoại lệ khi chạy Web Worker.");
        score = 0;
        isPassed = false;
        details = { ...details, mode: "web_worker", error: runRes.error };
      } else {
        const resList = runRes.results || [];
        setWorkerResults(resList);
        const passedCount = resList.filter((r) => r.passed).length;
        score = resList.length > 0 ? Math.round((passedCount / resList.length) * 100) : 0;
        isPassed = resList.length > 0 && passedCount === resList.length;
        details = {
          ...details,
          mode: "web_worker",
          passedCount,
          totalCount: resList.length,
          executionTimeMs: runRes.executionTimeMs,
        };
      }
    } else {
      const st = evaluateBugFixStatic(editedCode, activeItem);
      setStaticResult(st);
      score = st.score;
      isPassed = st.isPassed;
      details = {
        ...details,
        mode: "static_normalized",
        missingRequired: st.missingRequired,
        remainingForbidden: st.remainingForbidden,
      };
    }

    if (onRecordAttempt) {
      await onRecordAttempt({
        mode: "bug_fix",
        activityId: activeItem.sourceActivityId || null,
        score,
        maxScore: 100,
        isPassed,
        activityKey: `bug-fix-${activeItem.id}`,
        activityTitle: `${activeItem.title} · Sửa Bug Write-in`,
        details,
      });
    }

    if (!isPassed) {
      await handleSaveCard(true);
    }
  };

  return (
    <div
      data-testid="bug-fix-writein-panel"
      className="p-4 sm:p-5 rounded-2xl border border-amber-500/35 bg-amber-500/5 space-y-4"
    >
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-amber-500/20">
        <div>
          <span className="text-xs font-bold text-amber-700 dark:text-amber-300 inline-flex items-center gap-1.5">
            <Wrench className="w-3.5 h-3.5" />
            Sửa Bug Trực Tiếp (Write-in Code Repair)
            {canRunWorker ? " · Kiểm thử thật trên Web Worker" : " · Chấm chuẩn hóa cú pháp"}
          </span>
          <h4 className="text-sm sm:text-base font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 mt-0.5">
            {activeItem.title}
          </h4>
          <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400 mt-0.5">
            {activeItem.context}
          </p>
        </div>

        <select
          data-testid="bug-fix-writein-select"
          value={selectedIdx}
          onChange={(e) => setSelectedIdx(Number(e.target.value))}
          className="min-h-[40px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs font-medium text-[var(--text-primary,#18181b)] dark:text-zinc-100 cursor-pointer shrink-0"
        >
          {allItems.map((it, idx) => (
            <option key={it.id} value={idx}>
              [{it.language.toUpperCase()}] {it.title}
            </option>
          ))}
        </select>
      </div>

      {fsrsToast && (
        <div className="p-2.5 rounded-lg bg-emerald-500/10 border border-emerald-500/30 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
          <Sparkles className="w-4 h-4 shrink-0" />
          <span>{fsrsToast}</span>
        </div>
      )}

      <div className="rounded-xl border border-zinc-800 bg-zinc-950 overflow-hidden">
        <div className="px-4 py-2 bg-zinc-900 border-b border-zinc-800 flex items-center justify-between text-xs text-zinc-400">
          <span className="font-mono flex items-center gap-1.5">
            <Code2 className="w-3.5 h-3.5 text-amber-400" />
            Sửa trực tiếp đoạn mã lỗi bên dưới ({activeItem.language})
          </span>
          <button
            type="button"
            onClick={() => {
              setEditedCode(activeItem.buggyCode);
              setWorkerResults(null);
              setWorkerError(null);
              setStaticResult(null);
            }}
            className="text-[11px] text-zinc-300 hover:text-white inline-flex items-center gap-1 cursor-pointer"
          >
            <RotateCcw className="w-3 h-3" />
            Khôi phục bản lỗi
          </button>
        </div>
        <textarea
          data-testid="bug-fix-writein-editor"
          rows={9}
          value={editedCode}
          onChange={(e) => setEditedCode(e.target.value)}
          spellCheck={false}
          className="w-full p-4 font-mono text-xs sm:text-sm text-amber-200 bg-zinc-950 focus:outline-none leading-relaxed resize-y"
        />
      </div>

      <div className="flex flex-wrap items-center gap-2.5">
        <button
          type="button"
          data-testid="bug-fix-writein-check-btn"
          disabled={isRunning || !editedCode.trim()}
          onClick={() => void handleCheckBugFix()}
          className="min-h-[44px] px-4 py-2 rounded-xl bg-amber-600 hover:bg-amber-700 disabled:opacity-50 text-white text-xs sm:text-sm font-semibold inline-flex items-center gap-2 cursor-pointer shadow-xs"
        >
          {isRunning ? <Loader2 className="w-4 h-4 animate-spin" /> : <Play className="w-4 h-4" />}
          <span>{canRunWorker ? "Chạy Test Case & Kiểm Tra Bản Sửa" : "Kiểm Tra Bản Sửa"}</span>
        </button>

        <button
          type="button"
          onClick={() => setShowSolution((v) => !v)}
          className="min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs font-medium text-zinc-700 dark:text-zinc-300 inline-flex items-center gap-1.5 cursor-pointer"
        >
          {showSolution ? <EyeOff className="w-3.5 h-3.5" /> : <Eye className="w-3.5 h-3.5" />}
          <span>{showSolution ? "Ẩn đáp án chuẩn" : "Xem đáp án chuẩn"}</span>
        </button>

        <button
          type="button"
          onClick={() => void handleSaveCard(false)}
          className="min-h-[44px] px-3.5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold inline-flex items-center gap-1.5 cursor-pointer"
        >
          <Sparkles className="w-3.5 h-3.5" />
          <span>Lưu vào Flashcard FSRS</span>
        </button>

        <button
          type="button"
          onClick={() =>
            void onSaveToNotebook(
              `### 🛠️ Sửa Bug Write-in: ${activeItem.title}\n\`\`\`${activeItem.language}\n${editedCode}\n\`\`\`\n- **Giải thích:** ${activeItem.explanation}`
            )
          }
          className="min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-xs font-medium text-zinc-700 dark:text-zinc-300 inline-flex items-center gap-1.5 cursor-pointer"
        >
          <BookmarkPlus className="w-3.5 h-3.5 text-amber-500" />
          <span>Lưu Sổ tay</span>
        </button>
      </div>

      {workerError && (
        <div className="p-3.5 rounded-xl bg-rose-500/10 border border-rose-500/30 text-rose-800 dark:text-rose-200 text-xs font-mono">
          {workerError}
        </div>
      )}

      {workerResults && (
        <div
          data-testid="bug-fix-worker-results"
          className="p-4 rounded-xl bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 space-y-2.5 text-xs"
        >
          <div className="flex items-center justify-between font-bold">
            <span>
              Kết quả chạy Web Worker: {workerResults.filter((r) => r.passed).length}/
              {workerResults.length} test đạt
            </span>
            <span
              className={`px-2.5 py-0.5 rounded-full ${
                workerResults.every((r) => r.passed)
                  ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                  : "bg-rose-500/15 text-rose-700 dark:text-rose-300"
              }`}
            >
              {Math.round(
                (workerResults.filter((r) => r.passed).length /
                  Math.max(1, workerResults.length)) *
                  100
              )}
              /100 điểm
            </span>
          </div>
          {workerResults.map((tr, i) => (
            <div
              key={i}
              className={`p-2.5 rounded-lg border space-y-1 ${
                tr.passed
                  ? "bg-emerald-500/10 border-emerald-500/30"
                  : "bg-rose-500/10 border-rose-500/30"
              }`}
            >
              <div className="flex items-center justify-between font-semibold">
                <span className="flex items-center gap-1.5">
                  {tr.passed ? (
                    <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
                  ) : (
                    <XCircle className="w-4 h-4 text-rose-600 dark:text-rose-400" />
                  )}
                  <span>{tr.name}</span>
                </span>
                <span className="font-mono">{tr.passed ? "PASS" : "FAIL"}</span>
              </div>
              <div className="font-mono text-[11px] opacity-90 pl-5 space-y-0.5">
                <div>Input: {JSON.stringify(tr.input)}</div>
                <div>Expected: {JSON.stringify(tr.expected)}</div>
                <div>Actual: {JSON.stringify(tr.actual)}</div>
                {tr.error && <div className="text-rose-600 dark:text-rose-400">Lỗi: {tr.error}</div>}
              </div>
            </div>
          ))}
        </div>
      )}

      {staticResult && (
        <div
          data-testid="bug-fix-static-results"
          className={`p-4 rounded-xl border text-xs space-y-2 ${
            staticResult.isPassed
              ? "bg-emerald-500/10 border-emerald-500/35 text-emerald-900 dark:text-emerald-200"
              : "bg-rose-500/10 border-rose-500/35 text-rose-900 dark:text-rose-200"
          }`}
        >
          <div className="flex items-center justify-between font-bold">
            <span>{staticResult.feedbackVi}</span>
            <span>{staticResult.score}/100 điểm</span>
          </div>
          <p className="opacity-90">{activeItem.explanation}</p>
        </div>
      )}

      {showSolution && activeItem.cleanFixedCode && (
        <div className="rounded-xl border border-emerald-700/60 bg-zinc-950 text-emerald-300 p-4 font-mono text-xs overflow-x-auto space-y-2">
          <div className="font-sans font-bold text-emerald-400">Đáp án chuẩn Production:</div>
          <pre className="whitespace-pre">{activeItem.cleanFixedCode}</pre>
          <p className="font-sans text-zinc-300 pt-2 border-t border-zinc-800">
            {activeItem.explanation}
          </p>
        </div>
      )}
    </div>
  );
}
