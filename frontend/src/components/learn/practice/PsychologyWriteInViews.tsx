"use client";

import React, { useEffect, useMemo, useState } from "react";
import {
  Award,
  BookmarkPlus,
  Brain,
  CheckCircle2,
  Compass,
  Eye,
  EyeOff,
  Heart,
  Loader2,
  RotateCcw,
  Send,
  Sparkles,
  XCircle,
  Zap,
} from "lucide-react";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import {
  WRITEIN_GRADING_LABELS,
  evaluateBiasSpotterAnswer,
  evaluateCbtExercise,
  evaluateFeynmanOffline,
  evaluateJournalCriteriaOffline,
  evaluatePracticeWithAiOrOffline,
  type AiPracticeEvaluationResult,
  type ApplyJournalPromptItem,
  type BiasSpotterEvaluationResult,
  type BiasSpotterItem,
  type CbtExerciseEvaluationResult,
  type CbtExerciseItem,
  type FeynmanEvaluationResult,
  type FeynmanPromptItem,
} from "@/lib/writein-practice";
import { saveWriteInFlashcardDeduplicated } from "./WriteInFsrsHelper";

export const DEFAULT_BIAS_SPOTTER_ITEMS: BiasSpotterItem[] = [
  {
    id: "psych-bias-confirmation",
    title: "Tình huống 1: Chỉ tìm bài viết đồng tình với quan điểm có sẵn",
    contextTag: "Tư duy phản biện & Học tập",
    scenario:
      "Minh tin rằng học thức khuya hiệu quả hơn dậy sớm. Khi tìm tài liệu, Minh chỉ đọc và lưu lại những bài blog ca ngợi 'cú đêm sáng tạo' và bỏ qua hoàn toàn các nghiên cứu khoa học thần kinh chỉ ra tác hại thiếu ngủ lên trí nhớ dài hạn.",
    canonicalNameVi: "Thiên kiến xác nhận (Confirmation Bias)",
    acceptedNames: [
      "Thiên kiến xác nhận",
      "thien kien xac nhan",
      "Confirmation Bias",
      "confirmation",
      "xác nhận",
    ],
    reasonKeywords: ["thông tin", "quan điểm", "bỏ qua", "củng cố", "niềm tin"],
    minReasonWords: 8,
    sampleExplanation:
      "Minh đang chủ động tìm kiếm thông tin củng cố niềm tin có sẵn và phớt lờ bằng chứng trái chiều — biểu hiện điển hình của Thiên kiến xác nhận.",
    counterMeasureVi:
      "Luôn áp dụng kỹ thuật Steelman: chủ động tìm ít nhất 2 lập luận mạnh nhất phản bác giả thuyết ban đầu của mình.",
  },
  {
    id: "psych-bias-sunk-cost",
    title: "Tình huống 2: Cố theo tiếp khóa học / dự án không phù hợp vì tiếc công sức cũ",
    contextTag: "Ra quyết định & Quản lý thời gian",
    scenario:
      "Lan đã đóng 12 triệu và học 2 tháng một khóa học thiết kế nhưng nhận ra hoàn toàn không phù hợp định hướng nghề nghiệp. Tuy nhiên Lan vẫn ép bản thân mất thêm 4 tháng nữa vì nghĩ 'bỏ giữa chừng thì uổng 12 triệu đã đóng'.",
    canonicalNameVi: "Ảo tưởng chi phí chìm (Sunk Cost Fallacy)",
    acceptedNames: [
      "Ảo tưởng chi phí chìm",
      "Chi phí chìm",
      "Thiên kiến chi phí chìm",
      "Sunk Cost Fallacy",
      "Sunk Cost",
      "chi phi chim",
    ],
    reasonKeywords: ["chi phí", "quá khứ", "không thể thu hồi", "tiếc", "tương lai"],
    minReasonWords: 8,
    sampleExplanation:
      "Khoản 12 triệu và 2 tháng đã chi là chi phí chìm không thể lấy lại; việc ra quyết định tương lai dựa trên sự tiếc nuối quá khứ khiến Lan mất thêm 4 tháng quý giá.",
    counterMeasureVi:
      "Tự hỏi: 'Nếu hôm nay mình bắt đầu từ con số 0 và chưa tốn đồng nào, mình có chọn tiếp tục việc này không?'",
  },
  {
    id: "psych-bias-anchoring",
    title: "Tình huống 3: Bị chi phối bởi mức giá gốc gạch ngang đầu tiên",
    contextTag: "Đời sống & Tiêu dùng",
    scenario:
      "Một chiếc tai nghe niêm yết giá gốc 4.000.000đ đang giảm giá chỉ còn 1.990.000đ. Dù không thật sự cần và chưa so sánh với các mẫu cùng phân khúc giá 1.500.000đ, Nam vẫn mua ngay vì cảm thấy mình 'lời tới 2 triệu'.",
    canonicalNameVi: "Hiệu ứng mỏ neo (Anchoring Bias)",
    acceptedNames: [
      "Hiệu ứng mỏ neo",
      "Thiên kiến mỏ neo",
      "Mỏ neo",
      "Anchoring Bias",
      "Anchoring",
      "mo neo",
    ],
    reasonKeywords: ["con số đầu tiên", "mỏ neo", "giá gốc", "tham chiếu", "so sánh"],
    minReasonWords: 8,
    sampleExplanation:
      "Não bộ của Nam bị neo vào con số 4 triệu đầu tiên làm mốc tham chiếu nên đánh giá mức 1,99 triệu là rẻ mà không đối chiếu giá trị thực.",
    counterMeasureVi:
      "Tạm dừng 24 giờ và tự khảo sát giá trị sử dụng thực tế độc lập với con số niêm yết đầu tiên.",
  },
];

export const DEFAULT_FEYNMAN_ITEMS: FeynmanPromptItem[] = [
  {
    id: "psych-feynman-system12",
    conceptTitle: "Hệ thống 1 & Hệ thống 2 (Daniel Kahneman)",
    promptVi:
      "Không nhìn lại tài liệu, hãy tự định nghĩa bằng lời của bạn sự khác nhau giữa Hệ thống 1 và Hệ thống 2 trong tư duy, kèm 1 ví dụ đời thường cụ thể.",
    requiredKeywords: [
      "hệ thống 1|system 1",
      "hệ thống 2|system 2",
      "tự động|nhanh",
      "nỗ lực|chậm|logic",
      "ví dụ",
    ],
    minWords: 18,
    sampleAnswer:
      "Hệ thống 1 là cơ chế xử lý nhanh, tự động và theo cảm tính (ví dụ nhận diện ngay nét mặt giận dữ hay đọc chữ quen thuộc). Hệ thống 2 hoạt động chậm hơn, đòi hỏi sự tập trung, nỗ lực và phân tích logic (ví dụ nhẩm phép tính 17 × 24 hoặc kiểm chứng màu mực trong thí nghiệm Stroop).",
    everydayExampleHintVi:
      "Gợi ý ví dụ: lái xe trên đường quen thuộc (Hệ thống 1) vs lùi xe vào bãi đỗ chật hẹp (Hệ thống 2).",
  },
  {
    id: "psych-feynman-stroop",
    conceptTitle: "Hiệu ứng Stroop (Stroop Interference Effect)",
    promptVi:
      "Hãy giải thích Hiệu ứng Stroop cho một người bạn chưa từng học tâm lý học hiểu được nguyên nhân vì sao ta phản xạ chậm lại khi tên màu và màu mực xung đột.",
    requiredKeywords: ["màu", "đọc", "tự động", "xung đột|nhiễu", "phản xạ|chậm"],
    minWords: 15,
    sampleAnswer:
      "Hiệu ứng Stroop xảy ra khi bạn nhìn chữ 'ĐỎ' nhưng được tô bằng mực màu xanh dương và phải gọi tên màu mực. Vì kỹ năng đọc chữ đã trở nên quá tự động ở Hệ thống 1, não bộ phải tốn thêm nỗ lực của Hệ thống 2 để ức chế xung đột, làm thời gian phản xạ chậm lại rõ rệt.",
    everydayExampleHintVi:
      "Hãy mô tả cụ thể ví dụ chữ 'XANH' in bằng mực màu đỏ.",
  },
];

export const DEFAULT_CBT_EXERCISES: CbtExerciseItem[] = [
  {
    id: "psych-cbt-ex-catastrophizing",
    title: "Bài tập CBT 1: Bị góp ý sai sót trong buổi thuyết trình",
    scenario:
      "Trong buổi họp nhóm, trưởng nhóm chỉ ra 2 số liệu trên slide của bạn chưa khớp với bảng gốc và yêu cầu chỉnh lại trước chiều nay. Ngay lập tức bạn cảm thấy nóng mặt, tim đập nhanh và nghĩ rằng cả nhóm đang coi thường năng lực của mình.",
    sampleAutomaticThought:
      "Mình thật tệ hại, chắc chắn trưởng nhóm và mọi người nghĩ mình bất tài và sắp loại mình khỏi dự án.",
    canonicalDistortionVi: "Trầm trọng hóa / Đọc suy nghĩ (Catastrophizing & Mind Reading)",
    acceptedDistortions: [
      "Trầm trọng hóa",
      "Thảm họa hóa",
      "Đọc suy nghĩ",
      "Catastrophizing",
      "Mind Reading",
      "tram trong hoa",
      "doc suy nghi",
      "tham hoa hoa",
      "khái quát hóa quá mức",
      "overgeneralization",
    ],
    distortionChoicesHint: [
      "Trầm trọng hóa (Catastrophizing)",
      "Đọc suy nghĩ (Mind Reading)",
      "Tư duy trắng - đen (All-or-Nothing)",
      "Cá nhân hóa (Personalization)",
    ],
    balancedKeywords: ["bằng chứng", "sửa", "sai sót", "không có nghĩa", "học hỏi|cải thiện"],
    minBalancedWords: 12,
    sampleBalancedThought:
      "Sự thật là chỉ có 2 số liệu cần cập nhật lại trong 15 phút; không có bằng chứng nào cho thấy cả nhóm coi thường mình. Một sai sót kỹ thuật nhỏ không có nghĩa là mình bất tài, đây là dịp rà soát checklist tốt hơn.",
    explanationVi:
      "Suy diễn ý nghĩ của người khác không có bằng chứng là 'Đọc suy nghĩ', còn phóng đại lỗi nhỏ thành bị loại khỏi dự án là 'Trầm trọng hóa'.",
  },
  {
    id: "psych-cbt-ex-allornothing",
    title: "Bài tập CBT 2: Lỡ bỏ 1 ngày trong chuỗi ôn tập 30 ngày",
    scenario:
      "Bạn đặt mục tiêu học tiếng Anh 30 phút mỗi ngày trong 30 ngày. Đến ngày thứ 14 bạn bị ốm phải nghỉ 1 buổi và nảy sinh ý định bỏ luôn kế hoạch vì thấy 'đã hỏng hết cả chuỗi rồi'.",
    sampleAutomaticThought:
      "Mình đã đứt chuỗi 1 ngày rồi, coi như thất bại hoàn toàn, có học tiếp cũng vô nghĩa.",
    canonicalDistortionVi: "Tư duy trắng - đen / Tất cả hoặc không có gì (All-or-Nothing Thinking)",
    acceptedDistortions: [
      "Tư duy trắng đen",
      "Tư duy trắng - đen",
      "Tất cả hoặc không có gì",
      "All-or-Nothing",
      "Black and white",
      "tu duy trang den",
      "tat ca hoac khong co gi",
      "cực đoan",
    ],
    distortionChoicesHint: [
      "Tư duy trắng - đen (All-or-Nothing)",
      "Lọc tiêu cực (Mental Filter)",
      "Đọc suy nghĩ (Mind Reading)",
    ],
    balancedKeywords: ["13 ngày|tiến bộ", "nghỉ", "tiếp tục", "không phải thất bại", "kiên trì|dài hạn"],
    minBalancedWords: 12,
    sampleBalancedThought:
      "Mình đã duy trì tốt 13 ngày liên tiếp — nghỉ 1 ngày vì sức khỏe không phải thất bại và không xóa sạch tiến bộ trước đó. Ngày mai tiếp tục lại sẽ vẫn đạt 29/30 ngày rất đáng tự hào.",
    explanationVi:
      "Tư duy trắng - đen khiến bạn coi mọi kết quả dưới 100% hoàn hảo đều là số 0 tròn trĩnh, dễ dẫn đến bỏ cuộc.",
  },
];

// ============================================================================
// 1. BIAS SPOTTER WRITE-IN LAB
// ============================================================================

export function BiasSpotterWriteInLab({
  dbItems,
  lessonId,
  courseTitle,
  lessonTitle,
  initialActivityKey,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: {
  dbItems: BiasSpotterItem[];
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
    const list: BiasSpotterItem[] = [];
    for (const item of [...dbItems, ...DEFAULT_BIAS_SPOTTER_ITEMS]) {
      if (!seen.has(item.id)) {
        seen.add(item.id);
        list.push(item);
      }
    }
    return list;
  }, [dbItems]);

  const initialIdx = useMemo(() => {
    if (!initialActivityKey) return 0;
    const idx = allItems.findIndex(
      (x) =>
        x.id === initialActivityKey ||
        `bias-spotter-${x.id}` === initialActivityKey ||
        x.sourceActivityId === initialActivityKey
    );
    return idx >= 0 ? idx : 0;
  }, [allItems, initialActivityKey]);

  const [selectedIdx, setSelectedIdx] = useState(initialIdx);
  useEffect(() => {
    setSelectedIdx(initialIdx);
  }, [initialIdx]);

  const activeItem = allItems[selectedIdx] || allItems[0];
  const [biasNameInput, setBiasNameInput] = useState("");
  const [reasonInput, setReasonInput] = useState("");
  const [isEvaluating, setIsEvaluating] = useState(false);
  const [result, setResult] = useState<BiasSpotterEvaluationResult | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  useEffect(() => {
    setBiasNameInput("");
    setReasonInput("");
    setResult(null);
  }, [activeItem.id]);

  const handleSaveCard = async (isAuto = false) => {
    const front =
      activeItem.flashcardFront ||
      `[Nhận diện Thiên kiến] ${activeItem.canonicalNameVi} biểu hiện ra sao?`;
    const back =
      activeItem.flashcardBack ||
      `**Tình huống:** ${activeItem.scenario}\n\n**Giải thích:** ${activeItem.sampleExplanation}\n\n**Cách hóa giải:** ${activeItem.counterMeasureVi}`;
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["tam-ly-hoc", "thien-kien", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setToast(
        isAuto
          ? res.deduplicated
            ? "Thẻ thiên kiến này đã có sẵn trong Flashcard FSRS."
            : "Đã tự động lưu thiên kiến làm sai vào Flashcard FSRS!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu thiên kiến vào Flashcard FSRS!"
      );
      setTimeout(() => setToast(null), 4000);
    }
  };

  const handleSubmit = async () => {
    if (!biasNameInput.trim() || !reasonInput.trim() || isEvaluating) return;
    setIsEvaluating(true);

    const headers = getClientRequestHeaders();
    const aiReasonEval = await evaluatePracticeWithAiOrOffline({
      mode: "bias_spotter",
      lang: "vi-VN",
      prompt: `Tình huống tâm lý: ${activeItem.scenario}\nKhái niệm/Thiên kiến chuẩn: ${activeItem.canonicalNameVi}\nNgười học gõ tên: ${biasNameInput}\nHãy đánh giá lý do giải thích của người học.`,
      userResponse: reasonInput.trim(),
      expectedKeywords: activeItem.reasonKeywords,
      sampleAnswer: activeItem.sampleExplanation,
      contextTitle: `${courseTitle} · ${activeItem.title}`,
      minWords: activeItem.minReasonWords ?? 8,
      headers,
    });
    setIsEvaluating(false);

    const evaluated = evaluateBiasSpotterAnswer(
      activeItem,
      biasNameInput,
      reasonInput,
      aiReasonEval
    );
    setResult(evaluated);

    if (onRecordAttempt) {
      await onRecordAttempt({
        mode: "bias_spotter",
        activityId: activeItem.sourceActivityId || null,
        score: evaluated.score,
        maxScore: 100,
        isPassed: evaluated.isPassed,
        activityKey: `bias-spotter-${activeItem.id}`,
        activityTitle: `${activeItem.title} · Nhận diện Thiên kiến`,
        details: {
          itemId: activeItem.id,
          userBiasName: biasNameInput,
          nameMatched: evaluated.nameMatched,
          gradingMode: evaluated.gradingMode,
          usedLiveEndpoint: evaluated.usedLiveEndpoint,
        },
      });
    }

    if (!evaluated.isPassed) {
      await handleSaveCard(true);
    }
  };

  return (
    <div data-testid="psych-bias-spotter-container" className="space-y-5 animate-in fade-in duration-200">
      <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 p-5 space-y-4 shadow-sm">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
          <div>
            <span className="text-xs font-bold text-amber-700 dark:text-amber-300 inline-flex items-center gap-1.5">
              <Zap className="w-3.5 h-3.5" />
              Nhận diện Thiên kiến (Active Recall · Tự gõ không chọn trắc nghiệm)
            </span>
            <h3 className="text-base font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 mt-0.5">
              {activeItem.title}
            </h3>
          </div>

          <select
            data-testid="bias-spotter-select"
            value={selectedIdx}
            onChange={(e) => setSelectedIdx(Number(e.target.value))}
            className="min-h-[40px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-950 text-xs font-medium cursor-pointer shrink-0"
          >
            {allItems.map((it, idx) => (
              <option key={it.id} value={idx}>
                Tình huống #{idx + 1}: {it.title.slice(0, 44)}
              </option>
            ))}
          </select>
        </div>

        {toast && (
          <div className="p-3 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
            <Sparkles className="w-4 h-4 shrink-0" />
            <span>{toast}</span>
          </div>
        )}

        <div className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/70 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-1.5">
          <span className="text-[11px] font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400">
            Ngữ cảnh: {activeItem.contextTag}
          </span>
          <p className="text-sm leading-relaxed text-[var(--text-primary,#18181b)] dark:text-zinc-100">
            {activeItem.scenario}
          </p>
        </div>

        <div className="space-y-3">
          <div className="space-y-1">
            <label
              htmlFor="bias-spotter-name-input"
              className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block"
            >
              1. Tự gõ tên thiên kiến / khái niệm tâm lý (hỗ trợ tiếng Việt có dấu, không dấu hoặc tiếng Anh):
            </label>
            <input
              id="bias-spotter-name-input"
              data-testid="bias-spotter-name-input"
              type="text"
              value={biasNameInput}
              onChange={(e) => {
                setBiasNameInput(e.target.value);
                setResult(null);
              }}
              placeholder="Ví dụ: Thiên kiến xác nhận hoặc Confirmation Bias..."
              className="w-full min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-950 text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="space-y-1">
            <label
              htmlFor="bias-spotter-reason-input"
              className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block"
            >
              2. Viết lý do ngắn: Chi tiết nào trong tình huống chứng minh đây là thiên kiến đó?
            </label>
            <textarea
              id="bias-spotter-reason-input"
              data-testid="bias-spotter-reason-input"
              rows={3}
              value={reasonInput}
              onChange={(e) => {
                setReasonInput(e.target.value);
                setResult(null);
              }}
              placeholder="Giải thích bằng lời của bạn..."
              className="w-full p-3.5 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-950 text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="flex flex-wrap items-center gap-2.5">
            <button
              type="button"
              data-testid="bias-spotter-submit-btn"
              disabled={!biasNameInput.trim() || !reasonInput.trim() || isEvaluating}
              onClick={() => void handleSubmit()}
              className="min-h-[44px] px-5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs sm:text-sm font-semibold inline-flex items-center gap-2 cursor-pointer shadow-xs"
            >
              {isEvaluating ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
              <span>{isEvaluating ? "Đang chấm bài..." : "Kiểm tra & Chấm giải thích"}</span>
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
            data-testid="bias-spotter-result"
            className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-3 text-xs"
          >
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span
                data-testid="bias-spotter-grading-badge"
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
                  result.isPassed ? "bg-emerald-600 text-white" : "bg-rose-600 text-white"
                }`}
              >
                Điểm: {result.score}/100
              </span>
            </div>

            <div className="flex items-start gap-2 font-semibold text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100">
              {result.nameMatched ? (
                <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0 mt-0.5" />
              ) : (
                <XCircle className="w-5 h-5 text-rose-600 shrink-0 mt-0.5" />
              )}
              <span>{result.feedbackVi}</span>
            </div>

            <p className="text-[var(--text-secondary,#3f3f46)] dark:text-zinc-300 leading-relaxed">
              {result.reasonEval.feedbackVi}
            </p>

            <div className="p-3 rounded-lg bg-emerald-500/10 border border-emerald-500/25 space-y-1 text-emerald-950 dark:text-emerald-100">
              <div>
                <strong>Khái niệm chuẩn:</strong> {activeItem.canonicalNameVi}
              </div>
              <div>
                <strong>Giải thích mẫu:</strong> {activeItem.sampleExplanation}
              </div>
              {activeItem.counterMeasureVi && (
                <div>
                  <strong>Cách hóa giải (Gỡ bẫy tư duy):</strong> {activeItem.counterMeasureVi}
                </div>
              )}
            </div>

            <button
              type="button"
              onClick={() =>
                void onSaveToNotebook(
                  `### ⚡ Nhận diện Thiên kiến: ${activeItem.canonicalNameVi}\n- **Tình huống:** ${activeItem.scenario}\n- **Câu trả lời của tôi:** ${biasNameInput} — ${reasonInput}\n- **Cách hóa giải:** ${activeItem.counterMeasureVi}`
                )
              }
              className="min-h-[38px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 text-xs font-medium inline-flex items-center gap-1.5 cursor-pointer"
            >
              <BookmarkPlus className="w-3.5 h-3.5 text-indigo-500" />
              <span>Lưu vào Sổ tay</span>
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

// ============================================================================
// 2. FEYNMAN SELF-DEFINITION WRITE-IN LAB
// ============================================================================

export function FeynmanWriteInLab({
  dbItems,
  lessonId,
  courseTitle,
  lessonTitle,
  initialActivityKey,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: {
  dbItems: FeynmanPromptItem[];
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
    const list: FeynmanPromptItem[] = [];
    for (const item of [...dbItems, ...DEFAULT_FEYNMAN_ITEMS]) {
      if (!seen.has(item.id)) {
        seen.add(item.id);
        list.push(item);
      }
    }
    return list;
  }, [dbItems]);

  const initialIdx = useMemo(() => {
    if (!initialActivityKey) return 0;
    const idx = allItems.findIndex(
      (x) =>
        x.id === initialActivityKey ||
        `feynman-${x.id}` === initialActivityKey ||
        x.sourceActivityId === initialActivityKey
    );
    return idx >= 0 ? idx : 0;
  }, [allItems, initialActivityKey]);

  const [selectedIdx, setSelectedIdx] = useState(initialIdx);
  useEffect(() => {
    setSelectedIdx(initialIdx);
  }, [initialIdx]);

  const activeItem = allItems[selectedIdx] || allItems[0];
  const [explanationText, setExplanationText] = useState("");
  const [showSample, setShowSample] = useState(false);
  const [isEvaluating, setIsEvaluating] = useState(false);
  const [result, setResult] = useState<FeynmanEvaluationResult | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  useEffect(() => {
    setExplanationText("");
    setShowSample(false);
    setResult(null);
  }, [activeItem.id]);

  const handleSaveCard = async (isAuto = false) => {
    const front = `[Feynman] Tự định nghĩa & nêu ví dụ đời thường cho: ${activeItem.conceptTitle}`;
    const back = `${activeItem.sampleAnswer}\n\n**Từ khóa cốt lõi:** ${activeItem.requiredKeywords
      .map((k) => k.split("|")[0])
      .join(", ")}`;
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["tam-ly-hoc", "feynman", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setToast(
        isAuto
          ? res.deduplicated
            ? "Khái niệm Feynman này đã nằm trong Flashcard FSRS."
            : "Đã tự động lưu khái niệm còn thiếu ý vào Flashcard FSRS!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu khái niệm vào Flashcard FSRS!"
      );
      setTimeout(() => setToast(null), 4000);
    }
  };

  const handleCheck = async () => {
    if (!explanationText.trim() || isEvaluating) return;
    setIsEvaluating(true);

    const offlineEval = evaluateFeynmanOffline(activeItem, explanationText);
    const headers = getClientRequestHeaders();
    const aiEval = await evaluatePracticeWithAiOrOffline({
      mode: "feynman",
      lang: "vi-VN",
      prompt: `Khái niệm cần giải thích theo kỹ thuật Feynman: ${activeItem.conceptTitle}\nYêu cầu: ${activeItem.promptVi}`,
      userResponse: explanationText.trim(),
      expectedKeywords: activeItem.requiredKeywords.map((k) => k.split("|")[0]),
      sampleAnswer: activeItem.sampleAnswer,
      contextTitle: `${courseTitle} · Feynman`,
      minWords: activeItem.minWords,
      headers,
    });
    setIsEvaluating(false);

    const finalRes: FeynmanEvaluationResult = aiEval.usedLiveEndpoint
      ? {
          ...aiEval,
          matchedKeywords: offlineEval.matchedKeywords,
          missingKeywords: offlineEval.missingKeywords,
          wordCount: offlineEval.wordCount,
          minWords: offlineEval.minWords,
          meetsMinLength: offlineEval.meetsMinLength,
          isPassed: aiEval.score >= (activeItem.passingScore ?? 70),
        }
      : offlineEval;

    setResult(finalRes);

    if (onRecordAttempt) {
      await onRecordAttempt({
        mode: "feynman",
        activityId: activeItem.sourceActivityId || null,
        score: finalRes.score,
        maxScore: 100,
        isPassed: finalRes.isPassed,
        activityKey: `feynman-${activeItem.id}`,
        activityTitle: `${activeItem.conceptTitle} · Feynman`,
        details: {
          itemId: activeItem.id,
          wordCount: finalRes.wordCount,
          matchedKeywords: finalRes.matchedKeywords,
          missingKeywords: finalRes.missingKeywords,
          gradingMode: finalRes.gradingMode,
          usedLiveEndpoint: finalRes.usedLiveEndpoint,
        },
      });
    }

    if (!finalRes.isPassed) {
      await handleSaveCard(true);
    }
  };

  return (
    <div data-testid="psych-feynman-container" className="space-y-5 animate-in fade-in duration-200">
      <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 p-5 space-y-4 shadow-sm">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
          <div>
            <span className="text-xs font-bold text-indigo-600 dark:text-indigo-400 inline-flex items-center gap-1.5">
              <Award className="w-3.5 h-3.5" />
              Feynman Tự Định Nghĩa &amp; Ví Dụ Đời Thường
            </span>
            <h3 className="text-base font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 mt-0.5">
              {activeItem.conceptTitle}
            </h3>
          </div>

          <select
            data-testid="feynman-concept-select"
            value={selectedIdx}
            onChange={(e) => setSelectedIdx(Number(e.target.value))}
            className="min-h-[40px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-950 text-xs font-medium cursor-pointer shrink-0"
          >
            {allItems.map((it, idx) => (
              <option key={it.id} value={idx}>
                Khái niệm #{idx + 1}: {it.conceptTitle}
              </option>
            ))}
          </select>
        </div>

        {toast && (
          <div className="p-3 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
            <Sparkles className="w-4 h-4 shrink-0" />
            <span>{toast}</span>
          </div>
        )}

        <p className="text-xs sm:text-sm text-[var(--text-secondary,#3f3f46)] dark:text-zinc-300 leading-relaxed">
          {activeItem.promptVi}
        </p>

        {activeItem.everydayExampleHintVi && (
          <div className="text-xs text-indigo-700 dark:text-indigo-300 bg-indigo-500/10 border border-indigo-500/20 p-2.5 rounded-lg">
            Ví dụ gợi ý: {activeItem.everydayExampleHintVi}
          </div>
        )}

        <textarea
          data-testid="psych-feynman-textarea"
          rows={5}
          value={explanationText}
          onChange={(e) => {
            setExplanationText(e.target.value);
            setResult(null);
          }}
          placeholder={`Tự viết định nghĩa bằng ngôn từ đơn giản của bạn và thêm 1 ví dụ đời thường (tối thiểu khoảng ${activeItem.minWords} từ)...`}
          className="w-full p-3.5 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-950 text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
        />

        <div className="flex flex-wrap items-center gap-2.5">
          <button
            type="button"
            data-testid="psych-feynman-submit-btn"
            disabled={!explanationText.trim() || isEvaluating}
            onClick={() => void handleCheck()}
            className="min-h-[44px] px-5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs sm:text-sm font-semibold inline-flex items-center gap-2 cursor-pointer shadow-xs"
          >
            {isEvaluating ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
            <span>{isEvaluating ? "Đang chấm bài..." : "Chấm độ hiểu Feynman"}</span>
          </button>

          <button
            type="button"
            onClick={() => setShowSample((v) => !v)}
            className="min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 text-xs font-medium inline-flex items-center gap-1.5 cursor-pointer"
          >
            {showSample ? <EyeOff className="w-3.5 h-3.5" /> : <Eye className="w-3.5 h-3.5" />}
            <span>{showSample ? "Ẩn mẫu chuẩn" : "Xem giải thích mẫu"}</span>
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

        {showSample && (
          <div className="p-3.5 rounded-xl bg-emerald-500/10 border border-emerald-500/25 text-xs text-emerald-950 dark:text-emerald-100 leading-relaxed">
            <strong>Giải thích mẫu Feynman:</strong> {activeItem.sampleAnswer}
          </div>
        )}

        {result && (
          <div
            data-testid="psych-feynman-result"
            className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-3 text-xs"
          >
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span
                data-testid="psych-feynman-grading-badge"
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
                  result.isPassed ? "bg-emerald-600 text-white" : "bg-amber-600 text-white"
                }`}
              >
                Điểm: {result.score}/100 ({result.wordCount}/{result.minWords} từ)
              </span>
            </div>

            <p className="text-[var(--text-primary,#18181b)] dark:text-zinc-100 leading-relaxed">
              {result.feedbackVi}
            </p>

            <div className="space-y-1.5">
              {result.matchedKeywords.length > 0 && (
                <div className="flex flex-wrap items-center gap-1.5">
                  <span className="font-bold text-emerald-700 dark:text-emerald-300">
                    Từ khóa đã chạm:
                  </span>
                  {result.matchedKeywords.map((kw) => (
                    <span
                      key={kw}
                      className="px-2 py-0.5 rounded bg-emerald-500/15 text-emerald-800 dark:text-emerald-200 font-semibold"
                    >
                      ✓ {kw}
                    </span>
                  ))}
                </div>
              )}
              {result.missingKeywords.length > 0 && (
                <div className="flex flex-wrap items-center gap-1.5">
                  <span className="font-bold text-amber-700 dark:text-amber-300">
                    Từ khóa còn thiếu:
                  </span>
                  {result.missingKeywords.map((kw) => (
                    <span
                      key={kw}
                      className="px-2 py-0.5 rounded bg-amber-500/15 text-amber-800 dark:text-amber-200 font-semibold"
                    >
                      + {kw}
                    </span>
                  ))}
                </div>
              )}
            </div>

            <button
              type="button"
              onClick={() =>
                void onSaveToNotebook(
                  `### 🧠 Feynman Tự định nghĩa: ${activeItem.conceptTitle}\n${explanationText}\n- **Điểm (${result.gradingLabelVi}):** ${result.score}/100`
                )
              }
              className="min-h-[38px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 text-xs font-medium inline-flex items-center gap-1.5 cursor-pointer"
            >
              <BookmarkPlus className="w-3.5 h-3.5 text-indigo-500" />
              <span>Lưu bản tự định nghĩa vào Sổ tay</span>
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

// ============================================================================
// 3. GUIDED CBT EXERCISE WITH SAMPLE SCENARIO (CHẤM LỖI NHẬN THỨC + TÁI CẤU TRÚC)
// ============================================================================

export function CbtExerciseWriteInLab({
  dbItems,
  lessonId,
  courseTitle,
  lessonTitle,
  initialActivityKey,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: {
  dbItems: CbtExerciseItem[];
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
    const list: CbtExerciseItem[] = [];
    for (const item of [...dbItems, ...DEFAULT_CBT_EXERCISES]) {
      if (!seen.has(item.id)) {
        seen.add(item.id);
        list.push(item);
      }
    }
    return list;
  }, [dbItems]);

  const initialIdx = useMemo(() => {
    if (!initialActivityKey) return 0;
    const idx = allItems.findIndex(
      (x) =>
        x.id === initialActivityKey ||
        `cbt-exercise-${x.id}` === initialActivityKey ||
        x.sourceActivityId === initialActivityKey
    );
    return idx >= 0 ? idx : 0;
  }, [allItems, initialActivityKey]);

  const [selectedIdx, setSelectedIdx] = useState(initialIdx);
  useEffect(() => {
    setSelectedIdx(initialIdx);
  }, [initialIdx]);

  const activeItem = allItems[selectedIdx] || allItems[0];
  const [autoThought, setAutoThought] = useState("");
  const [distortionInput, setDistortionInput] = useState("");
  const [balancedThought, setBalancedThought] = useState("");
  const [showHints, setShowHints] = useState(false);
  const [isEvaluating, setIsEvaluating] = useState(false);
  const [result, setResult] = useState<CbtExerciseEvaluationResult | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  useEffect(() => {
    setAutoThought("");
    setDistortionInput("");
    setBalancedThought("");
    setShowHints(false);
    setResult(null);
  }, [activeItem.id]);

  const handleSaveCard = async (isAuto = false) => {
    const front = `[CBT Tái cấu trúc] ${activeItem.title} — Lỗi nhận thức là gì và cách viết lại cân bằng?`;
    const back = `**Lỗi nhận thức:** ${activeItem.canonicalDistortionVi}\n\n**Suy nghĩ cân bằng mẫu:** ${activeItem.sampleBalancedThought}\n\n**Giải thích:** ${activeItem.explanationVi}`;
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["tam-ly-hoc", "cbt", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setToast(
        isAuto
          ? res.deduplicated
            ? "Bài tập CBT này đã có sẵn trong Flashcard FSRS."
            : "Đã tự động lưu bài tập CBT cần ôn vào Flashcard FSRS!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu bài tập CBT vào Flashcard FSRS!"
      );
      setTimeout(() => setToast(null), 4000);
    }
  };

  const handleSubmit = async () => {
    if (!autoThought.trim() || !distortionInput.trim() || !balancedThought.trim() || isEvaluating) {
      return;
    }
    setIsEvaluating(true);

    const headers = getClientRequestHeaders();
    const aiBalancedEval = await evaluatePracticeWithAiOrOffline({
      mode: "cbt_exercise",
      lang: "vi-VN",
      prompt: `Tình huống CBT: ${activeItem.scenario}\nSuy nghĩ tự động học viên ghi: ${autoThought}\nLỗi nhận thức chuẩn: ${activeItem.canonicalDistortionVi}\nHãy chấm câu suy nghĩ cân bằng viết lại của học viên.`,
      userResponse: balancedThought.trim(),
      expectedKeywords: activeItem.balancedKeywords,
      sampleAnswer: activeItem.sampleBalancedThought,
      contextTitle: `${courseTitle} · ${activeItem.title}`,
      minWords: activeItem.minBalancedWords ?? 10,
      headers,
    });
    setIsEvaluating(false);

    const evalRes = evaluateCbtExercise(
      activeItem,
      {
        automaticThought: autoThought,
        distortionName: distortionInput,
        balancedThought,
      },
      aiBalancedEval
    );
    setResult(evalRes);

    if (onRecordAttempt) {
      await onRecordAttempt({
        mode: "cbt_exercise",
        activityId: activeItem.sourceActivityId || null,
        score: evalRes.score,
        maxScore: 100,
        isPassed: evalRes.isPassed,
        activityKey: `cbt-exercise-${activeItem.id}`,
        activityTitle: activeItem.title,
        details: {
          itemId: activeItem.id,
          distortionMatched: evalRes.distortionMatched,
          gradingMode: evalRes.gradingMode,
          usedLiveEndpoint: evalRes.usedLiveEndpoint,
        },
      });
    }

    if (!evalRes.isPassed) {
      await handleSaveCard(true);
    }
  };

  return (
    <div data-testid="psych-cbt-exercise-container" className="space-y-5 animate-in fade-in duration-200">
      <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 p-5 space-y-4 shadow-sm">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
          <div>
            <span className="text-xs font-bold text-purple-600 dark:text-purple-400 inline-flex items-center gap-1.5">
              <Heart className="w-3.5 h-3.5" />
              Bài Tập CBT Tình Huống Mẫu (Chấm Lỗi Nhận Thức &amp; Tái Cấu Trúc)
            </span>
            <h3 className="text-base font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 mt-0.5">
              {activeItem.title}
            </h3>
          </div>

          <select
            data-testid="cbt-exercise-select"
            value={selectedIdx}
            onChange={(e) => setSelectedIdx(Number(e.target.value))}
            className="min-h-[40px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-950 text-xs font-medium cursor-pointer shrink-0"
          >
            {allItems.map((it, idx) => (
              <option key={it.id} value={idx}>
                Bài tập #{idx + 1}: {it.title.slice(0, 44)}
              </option>
            ))}
          </select>
        </div>

        {toast && (
          <div className="p-3 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
            <Sparkles className="w-4 h-4 shrink-0" />
            <span>{toast}</span>
          </div>
        )}

        <div className="p-4 rounded-xl bg-purple-500/10 border border-purple-500/25 text-xs sm:text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100 leading-relaxed">
          <strong>Tình huống mẫu:</strong> {activeItem.scenario}
        </div>

        <div className="space-y-3">
          <div className="space-y-1">
            <label className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block">
              Bước 1: Ghi lại Suy nghĩ tự động tiêu cực (Automatic Thought) dễ nảy ra trong tình huống trên:
            </label>
            <textarea
              data-testid="cbt-ex-auto-input"
              rows={2}
              value={autoThought}
              onChange={(e) => {
                setAutoThought(e.target.value);
                setResult(null);
              }}
              placeholder="Ví dụ: Mình thất bại rồi, mọi người chắc chắn nghĩ mình kém cỏi..."
              className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-950 text-xs sm:text-sm"
            />
          </div>

          <div className="space-y-1.5">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <label className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200">
                Bước 2: Tự gõ tên Lỗi nhận thức (Cognitive Distortion) đang chi phối:
              </label>
              {activeItem.distortionChoicesHint.length > 0 && (
                <button
                  type="button"
                  onClick={() => setShowHints((v) => !v)}
                  className="text-xs font-semibold text-purple-600 dark:text-purple-400 hover:underline cursor-pointer"
                >
                  {showHints ? "Ẩn gợi ý danh sách lỗi" : "Gợi ý các nhóm lỗi nhận thức"}
                </button>
              )}
            </div>

            {showHints && activeItem.distortionChoicesHint.length > 0 && (
              <div className="flex flex-wrap gap-1.5">
                {activeItem.distortionChoicesHint.map((h) => (
                  <button
                    key={h}
                    type="button"
                    onClick={() => setDistortionInput(h)}
                    className="px-2.5 py-1 rounded-lg bg-purple-500/10 border border-purple-500/25 text-[11px] font-medium text-purple-800 dark:text-purple-200 hover:bg-purple-500/20 cursor-pointer"
                  >
                    {h}
                  </button>
                ))}
              </div>
            )}

            <input
              data-testid="cbt-ex-distortion-input"
              type="text"
              value={distortionInput}
              onChange={(e) => {
                setDistortionInput(e.target.value);
                setResult(null);
              }}
              placeholder="Ví dụ: Trầm trọng hóa, Đọc suy nghĩ, Tư duy trắng - đen..."
              className="w-full min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-950 text-xs sm:text-sm"
            />
          </div>

          <div className="space-y-1">
            <label className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block">
              Bước 3: Viết lại Suy nghĩ cân bằng &amp; thực tế (Balanced Reframing):
            </label>
            <textarea
              data-testid="cbt-ex-balanced-input"
              rows={3}
              value={balancedThought}
              onChange={(e) => {
                setBalancedThought(e.target.value);
                setResult(null);
              }}
              placeholder="Dựa trên bằng chứng khách quan, góc nhìn công bằng và hữu ích hơn là gì?"
              className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-950 text-xs sm:text-sm"
            />
          </div>

          <div className="flex flex-wrap items-center gap-2.5">
            <button
              type="button"
              data-testid="cbt-ex-submit-btn"
              disabled={
                !autoThought.trim() ||
                !distortionInput.trim() ||
                !balancedThought.trim() ||
                isEvaluating
              }
              onClick={() => void handleSubmit()}
              className="min-h-[44px] px-5 py-2 rounded-xl bg-purple-600 hover:bg-purple-700 disabled:opacity-50 text-white text-xs sm:text-sm font-semibold inline-flex items-center gap-2 cursor-pointer shadow-xs"
            >
              {isEvaluating ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
              <span>{isEvaluating ? "Đang chấm bài tập CBT..." : "Chấm bài tập CBT"}</span>
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
        </div>

        {result && (
          <div
            data-testid="cbt-ex-result"
            className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-3 text-xs"
          >
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span
                data-testid="cbt-ex-grading-badge"
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
                  result.isPassed ? "bg-emerald-600 text-white" : "bg-rose-600 text-white"
                }`}
              >
                Điểm: {result.score}/100
              </span>
            </div>

            <div className="flex items-start gap-2 font-semibold text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100">
              {result.distortionMatched ? (
                <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0 mt-0.5" />
              ) : (
                <XCircle className="w-5 h-5 text-rose-600 shrink-0 mt-0.5" />
              )}
              <span>{result.feedbackVi}</span>
            </div>

            <p className="text-[var(--text-secondary,#3f3f46)] dark:text-zinc-300">
              {result.balancedEval.feedbackVi}
            </p>

            <div className="p-3 rounded-lg bg-emerald-500/10 border border-emerald-500/25 space-y-1 text-emerald-950 dark:text-emerald-100">
              <div>
                <strong>Lỗi nhận thức chuẩn:</strong> {activeItem.canonicalDistortionVi}
              </div>
              <div>
                <strong>Suy nghĩ cân bằng mẫu:</strong> {activeItem.sampleBalancedThought}
              </div>
              <div>
                <strong>Giải thích:</strong> {activeItem.explanationVi}
              </div>
            </div>

            <button
              type="button"
              onClick={() =>
                void onSaveToNotebook(
                  `### 💜 Bài tập CBT: ${activeItem.title}\n- **Suy nghĩ tự động:** ${autoThought}\n- **Lỗi nhận thức:** ${distortionInput} (Chuẩn: ${activeItem.canonicalDistortionVi})\n- **Suy nghĩ cân bằng:** ${balancedThought}\n- **Điểm:** ${result.score}/100 (${result.gradingLabelVi})`
                )
              }
              className="min-h-[38px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 text-xs font-medium inline-flex items-center gap-1.5 cursor-pointer"
            >
              <BookmarkPlus className="w-3.5 h-3.5 text-purple-500" />
              <span>Lưu bài tập CBT vào Sổ tay</span>
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

// ============================================================================
// 4. APPLY JOURNAL WRITE-IN LAB (NHẬT KÝ ÁP DỤNG & CÂU GIAO TIẾP NVC)
// ============================================================================

export function ApplyJournalWriteInLab({
  dbPrompts,
  lessonId,
  courseTitle,
  lessonTitle,
  initialActivityKey,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: {
  dbPrompts: ApplyJournalPromptItem[];
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
            id: "apply-nvc-default",
            title: "Nhật Ký Áp Dụng Thực Tế & Soạn Lời Thoại NVC",
            promptVi: `Chọn một tình huống thật bạn sắp gặp trong tuần này để áp dụng bài "${lessonTitle}". Viết rõ kế hoạch hành động hoặc câu giao tiếp theo 4 bước NVC (Quan sát - Cảm xúc - Nhu cầu - Đề nghị).`,
            expectedKeywords: ["quan sát", "cảm xúc", "nhu cầu", "đề nghị", "hành động"],
            samplePlanVi:
              "Tình huống: Đồng nghiệp gửi file trễ hạn. Câu NVC: 'Khi mình nhận báo cáo lúc 22h tối (quan sát), mình thấy khá lo lắng (cảm xúc) vì cần thời gian rà kỹ trước buổi họp sáng (nhu cầu). Tuần tới bạn có thể gửi bản nháp trước 17h được không (đề nghị)?'",
          },
        ];
  }, [dbPrompts, lessonTitle]);

  const initialIdx = useMemo(() => {
    if (!initialActivityKey) return 0;
    const idx = allPrompts.findIndex(
      (p) =>
        p.id === initialActivityKey ||
        `apply-journal-${p.id}` === initialActivityKey ||
        p.sourceActivityId === initialActivityKey
    );
    return idx >= 0 ? idx : 0;
  }, [allPrompts, initialActivityKey]);

  const [selectedIdx, setSelectedIdx] = useState(initialIdx);
  useEffect(() => {
    setSelectedIdx(initialIdx);
  }, [initialIdx]);

  const promptItem = allPrompts[selectedIdx] || allPrompts[0];

  const [realSituation, setRealSituation] = useState("");
  const [nvcScript, setNvcScript] = useState("");
  const [concreteAction, setConcreteAction] = useState("");
  const [isEvaluating, setIsEvaluating] = useState(false);
  const [result, setResult] = useState<AiPracticeEvaluationResult | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  useEffect(() => {
    setRealSituation("");
    setNvcScript("");
    setConcreteAction("");
    setResult(null);
  }, [promptItem.id]);

  const handleSaveCard = async (isAuto = false) => {
    const front = `[Áp dụng Tâm lý / NVC] 4 bước giao tiếp phi bạo lực (NVC) là gì?`;
    const back =
      "1. Quan sát khách quan (O): nêu sự kiện như camera, không dán nhãn.\n2. Cảm xúc thật (F): 'Mình cảm thấy lo lắng/buồn...' thay vì đổ lỗi.\n3. Nhu cầu cốt lõi (N): nêu giá trị bạn cần.\n4. Đề nghị cụ thể (R): hành động rõ ràng, khả thi.";
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["tam-ly-hoc", "nvc", "nhat-ky-ap-dung", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setToast(
        isAuto
          ? res.deduplicated
            ? "Khung 4 bước NVC đã có sẵn trong Flashcard FSRS."
            : "Đã tự động lưu khung 4 bước NVC vào Flashcard FSRS để ôn tập!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu khung áp dụng vào Flashcard FSRS!"
      );
      setTimeout(() => setToast(null), 4000);
    }
  };

  const handleSubmit = async () => {
    if (!realSituation.trim() && !nvcScript.trim() && !concreteAction.trim()) {
      setToast("Hãy nhập tình huống và kế hoạch áp dụng trước khi đánh giá.");
      setTimeout(() => setToast(null), 3000);
      return;
    }

    const fields = [
      { label: "Tình huống thực tế của bản thân", value: realSituation, minWords: 6 },
      {
        label: "Câu giao tiếp NVC / Cách ứng xử mới",
        value: nvcScript,
        minWords: 8,
        keywords: ["cảm thấy|cảm xúc", "cần|nhu cầu", "có thể|đề nghị"],
      },
      {
        label: "Bước hành động cụ thể trong 24–48 giờ tới",
        value: concreteAction,
        minWords: 6,
      },
    ];

    const combined = fields.map((f) => `${f.label}: ${f.value.trim()}`).join("\n");

    setIsEvaluating(true);
    const headers = getClientRequestHeaders();
    const aiEval = await evaluatePracticeWithAiOrOffline({
      mode: "apply_journal",
      lang: "vi-VN",
      prompt: `${promptItem.title}\n${promptItem.promptVi}`,
      userResponse: combined,
      expectedKeywords: promptItem.expectedKeywords,
      sampleAnswer: promptItem.samplePlanVi,
      contextTitle: `${courseTitle} · Nhật ký áp dụng`,
      minWords: 20,
      headers,
    });
    setIsEvaluating(false);

    let finalRes = aiEval;
    if (!aiEval.usedLiveEndpoint) {
      finalRes = evaluateJournalCriteriaOffline(
        fields,
        promptItem.expectedKeywords,
        promptItem.samplePlanVi
      );
      finalRes.gradingLabelVi = WRITEIN_GRADING_LABELS.keyword_offline;
    }
    setResult(finalRes);

    const passing = promptItem.passingScore ?? 70;
    const isPassed = finalRes.score >= passing;

    const md = `### 🌱 Nhật Ký Áp Dụng: ${lessonTitle}
- **1. Tình huống thật của bản thân:** ${realSituation || "(chưa điền)"}
- **2. Câu giao tiếp NVC / Phản hồi mới:** ${nvcScript || "(chưa điền)"}
- **3. Hành động cam kết trong 24–48h:** ${concreteAction || "(chưa điền)"}
- **Đánh giá (${finalRes.gradingLabelVi}):** ${finalRes.score}/100 — ${finalRes.feedbackVi}`;

    try {
      await onSaveToNotebook(md);
    } catch {
      // non-blocking
    }

    if (onRecordAttempt) {
      await onRecordAttempt({
        mode: "apply_journal",
        activityId: promptItem.sourceActivityId || null,
        score: finalRes.score,
        maxScore: 100,
        isPassed,
        activityKey: `apply-journal-${promptItem.id}`,
        activityTitle: promptItem.title,
        details: {
          gradingMode: finalRes.gradingMode,
          usedLiveEndpoint: finalRes.usedLiveEndpoint,
          score: finalRes.score,
        },
      });
    }

    if (!isPassed) {
      await handleSaveCard(true);
    } else {
      setToast("Đã đánh giá và lưu Nhật ký áp dụng vào Sổ tay!");
      setTimeout(() => setToast(null), 3500);
    }
  };

  return (
    <div data-testid="psych-apply-journal-container" className="space-y-5 animate-in fade-in duration-200">
      <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 p-5 space-y-4 shadow-sm">
        <div className="pb-3 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div>
            <span className="text-xs font-bold text-emerald-700 dark:text-emerald-400 inline-flex items-center gap-1.5">
              <Compass className="w-3.5 h-3.5" />
              Nhật Ký Áp Dụng Đời Thực &amp; Luyện Câu NVC
            </span>
            <h3 className="text-base font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 mt-0.5">
              {promptItem.title}
            </h3>
            <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400 mt-1">
              {promptItem.promptVi}
            </p>
          </div>

          {allPrompts.length > 1 && (
            <select
              data-testid="apply-journal-select"
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

        <div className="space-y-3">
          <div className="space-y-1">
            <label className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block">
              1. Tình huống thật bạn muốn áp dụng (Quan sát khách quan không phán xét):
            </label>
            <textarea
              data-testid="apply-journal-situation"
              rows={2}
              value={realSituation}
              onChange={(e) => setRealSituation(e.target.value)}
              placeholder="Ví dụ: Trong buổi họp thứ Tư tới khi cần góp ý về tiến độ chưa khớp lịch..."
              className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-950 text-xs sm:text-sm"
            />
          </div>

          <div className="space-y-1">
            <label className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block">
              2. Viết nguyên văn câu giao tiếp NVC (Quan sát + Cảm xúc + Nhu cầu + Đề nghị) hoặc phản xạ mới:
            </label>
            <textarea
              data-testid="apply-journal-nvc"
              rows={3}
              value={nvcScript}
              onChange={(e) => setNvcScript(e.target.value)}
              placeholder="Khi [sự kiện quan sát], mình cảm thấy [cảm xúc] vì mình cần [nhu cầu]. Bạn có thể [đề nghị cụ thể] được không?"
              className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-950 text-xs sm:text-sm"
            />
          </div>

          <div className="space-y-1">
            <label className="text-xs font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-200 block">
              3. Kế hoạch hành động cụ thể &amp; dấu hiệu đo lường trong 24–48 giờ tới:
            </label>
            <textarea
              data-testid="apply-journal-action"
              rows={2}
              value={concreteAction}
              onChange={(e) => setConcreteAction(e.target.value)}
              placeholder="Ví dụ: Dành 3 phút hít thở sâu trước buổi họp, kích hoạt Hệ thống 2 và mở lời bằng câu NVC đã soạn..."
              className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-[var(--surface-elevated,#fafafa)] dark:bg-zinc-950 text-xs sm:text-sm"
            />
          </div>

          <div className="flex flex-wrap items-center gap-2.5">
            <button
              type="button"
              data-testid="apply-journal-submit-btn"
              disabled={isEvaluating}
              onClick={() => void handleSubmit()}
              className="min-h-[44px] px-5 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white text-xs sm:text-sm font-semibold inline-flex items-center gap-2 cursor-pointer shadow-xs"
            >
              {isEvaluating ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
              <span>{isEvaluating ? "Đang đánh giá..." : "Đánh giá tiêu chí & Lưu vào Sổ tay"}</span>
            </button>

            <button
              type="button"
              onClick={() => {
                setRealSituation("");
                setNvcScript("");
                setConcreteAction("");
                setResult(null);
              }}
              className="min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 text-xs font-medium inline-flex items-center gap-1.5 cursor-pointer"
            >
              <RotateCcw className="w-3.5 h-3.5" />
              <span>Làm lại</span>
            </button>

            <button
              type="button"
              onClick={() => void handleSaveCard(false)}
              className="min-h-[44px] px-3.5 py-2 rounded-xl border border-emerald-300 dark:border-emerald-700 bg-emerald-500/10 text-emerald-800 dark:text-emerald-200 text-xs font-semibold inline-flex items-center gap-1.5 cursor-pointer"
            >
              <Sparkles className="w-3.5 h-3.5" />
              <span>Lưu vào Flashcard FSRS</span>
            </button>
          </div>
        </div>

        {result && (
          <div
            data-testid="apply-journal-result"
            className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-3 text-xs"
          >
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span
                data-testid="apply-journal-grading-badge"
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
                Điểm tiêu chí: {result.score}/100
              </span>
            </div>

            <p className="text-[var(--text-primary,#18181b)] dark:text-zinc-100 leading-relaxed">
              {result.feedbackVi}
            </p>

            {result.improvements.length > 0 && (
              <ul className="list-disc list-inside space-y-0.5 text-amber-800 dark:text-amber-200">
                {result.improvements.map((imp, i) => (
                  <li key={i}>{imp}</li>
                ))}
              </ul>
            )}

            {promptItem.samplePlanVi && (
              <div className="p-3 rounded-lg bg-indigo-500/10 border border-indigo-500/25 text-indigo-950 dark:text-indigo-100">
                <strong>Mẫu áp dụng NVC tham khảo:</strong> {promptItem.samplePlanVi}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
