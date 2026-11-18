"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import dynamic from "next/dynamic";
import {
  Award,
  BookOpen,
  Brain,
  CandlestickChart,
  Check,
  CheckCircle2,
  Code2,
  Headphones,
  Layers,
  Loader2,
  NotebookPen,
  Plus,
  RotateCcw,
  Sparkles,
  Users,
} from "lucide-react";
import { getClientRequestHeaders, useWorkspaceStore } from "@/stores/useWorkspaceStore";

export interface LessonPracticeLabProps {
  courseId: string;
  courseTitle: string;
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  onSaveToNotebook: (textToAppend: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
}

type PracticeMode = "english" | "code" | "finance" | "recall";

function PracticeLabLoading({ label }: { label: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 px-4 space-y-3">
      <Loader2 className="w-8 h-8 text-accent animate-spin" />
      <p className="text-sm font-medium text-text-secondary">{label}</p>
    </div>
  );
}

const EnglishPracticeLab = dynamic(() => import("./practice/EnglishPracticeLab"), {
  loading: () => <PracticeLabLoading label="Đang tải phòng thực hành Tiếng Anh · Nghe & Nói..." />,
});

const CodingPracticeLab = dynamic(() => import("./practice/CodingPracticeLab"), {
  loading: () => <PracticeLabLoading label="Đang tải phòng thực hành Lập trình · Code & Săn lỗi..." />,
});

const FinancePracticeLab = dynamic(() => import("./practice/FinancePracticeLab"), {
  loading: () => <PracticeLabLoading label="Đang tải phòng thực hành Tài chính · Tính toán & Biểu đồ..." />,
});

const PsychologyLifeSkillsLab = dynamic(() => import("./practice/PsychologyLifeSkillsLab"), {
  loading: () => <PracticeLabLoading label="Đang tải phòng thực hành Kỹ năng & Tình huống..." />,
});

function stripDiacritics(input: string): string {
  return input
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase();
}

function detectPreferredMode(courseTitle: string, lessonTitle: string, md: string): PracticeMode {
  if (/```practice-finance\b/i.test(md)) return "finance";
  if (/```practice-english\b/i.test(md)) return "english";
  if (/```practice-code\b/i.test(md)) return "code";
  if (/```practice-case\b/i.test(md)) return "recall";

  const combined = stripDiacritics(`${courseTitle} ${lessonTitle} ${md.slice(0, 1500)}`);
  if (
    /\b(tai chinh|dau tu|chung khoan|investing|finance|nen|candlestick|co phieu|dong tien|danh muc|trai phieu|ngan sach|lai kep|tra no|snowball|avalanche)\b/.test(
      combined
    )
  ) {
    return "finance";
  }
  if (
    /\b(ielts|tieng anh|english|listening|speaking|vocabulary|grammar|phat am|reading|writing|shadowing|dictation|pronunciation)\b/.test(
      combined
    )
  ) {
    return "english";
  }
  if (
    /\b(\.net|c#|csharp|react|typescript|javascript|sql|api|docker|ef core|backend|frontend|fullstack|git|code|lap trinh|bug|debug|tlb|eat|memory|hoc may|machine learning|kien truc may tinh|computer architecture|he dieu hanh|bo nho ao|phan trang|microservices|ddd|event sourcing|python|fastapi|nodejs|kubernetes|algorithm|thuat toan|cau truc du lieu)\b/.test(
      combined
    ) ||
    /```(csharp|cs|ts|tsx|js|jsx|sql|python|py|bash|json)\b/i.test(md)
  ) {
    return "code";
  }
  return "recall";
}

export interface ConceptCheckCard {
  id: string;
  front: string;
  back: string;
}

function extractConceptCards(md: string, lessonTitle: string, fallbackTerms: string[]): ConceptCheckCard[] {
  const cards: ConceptCheckCard[] = [];
  const seenFronts = new Set<string>();

  // 1. Definition patterns: **Thuật ngữ**: giải thích hoặc - **Thuật ngữ**: giải thích
  const defRegex = /(?:^|\n)(?:[-*]\s*)?\*\*([^*\n:]{2,50})\*\*\s*[:–—\-]\s*([^\n]{8,240})/g;
  let match: RegExpExecArray | null;
  while ((match = defRegex.exec(md)) !== null) {
    const front = match[1].replace(/[:`[\]]/g, "").trim();
    const back = match[2].trim();
    const norm = stripDiacritics(front);
    if (
      front.length >= 2 &&
      back.length >= 8 &&
      !seenFronts.has(norm) &&
      !norm.startsWith("lo-") &&
      !norm.startsWith("c0")
    ) {
      seenFronts.add(norm);
      cards.push({ id: `def-${cards.length}`, front, back });
    }
    if (cards.length >= 8) break;
  }

  // 2. Headings ## / ### followed by explanation paragraph
  if (cards.length < 4) {
    const headingBlockRegex = /^#{2,3}\s+([^\n]{3,60})\r?\n+([^#\n][^\n]{15,250})/gm;
    while ((match = headingBlockRegex.exec(md)) !== null) {
      const heading = match[1].replace(/[*`#]/g, "").trim();
      const content = match[2].trim();
      const norm = stripDiacritics(heading);
      if (
        heading.length >= 3 &&
        content.length >= 15 &&
        !seenFronts.has(norm) &&
        !/muc luc|tham khao|bai tiep|gioi thieu|loi mo dau|tong ket/i.test(norm)
      ) {
        seenFronts.add(norm);
        cards.push({ id: `hd-${cards.length}`, front: heading, back: content });
      }
      if (cards.length >= 8) break;
    }
  }

  // 3. Fallback: key terms
  if (cards.length < 3 && fallbackTerms.length > 0) {
    for (const term of fallbackTerms) {
      const norm = stripDiacritics(term);
      if (!seenFronts.has(norm)) {
        seenFronts.add(norm);
        cards.push({
          id: `kt-${cards.length}`,
          front: term,
          back: `Khái niệm cốt lõi cần ghi nhớ và giải thích trong bài học "${lessonTitle}".`,
        });
      }
      if (cards.length >= 6) break;
    }
  }

  return cards;
}

function extractKeyTerms(md: string, lessonTitle: string): string[] {
  const terms = new Set<string>();
  const boldRegex = /\*\*([^*\n]{3,45})\*\*/g;
  let m: RegExpExecArray | null;
  while ((m = boldRegex.exec(md)) !== null) {
    const clean = m[1].replace(/[:`[\]]/g, "").trim();
    if (clean.length >= 3 && clean.length <= 42 && !clean.startsWith("LO-")) {
      terms.add(clean);
    }
    if (terms.size >= 8) break;
  }

  if (terms.size < 4) {
    const headingRegex = /^#{1,3}\s+(.+)$/gm;
    while ((m = headingRegex.exec(md)) !== null) {
      const clean = m[1].replace(/[*`#]/g, "").trim();
      if (clean.length >= 4 && clean.length <= 45) terms.add(clean);
      if (terms.size >= 8) break;
    }
  }

  if (terms.size === 0 && lessonTitle) {
    for (const part of lessonTitle.split(/[\s—–-]+/).filter((p) => p.length > 3)) {
      terms.add(part);
      if (terms.size >= 5) break;
    }
  }

  return Array.from(terms).slice(0, 7);
}

export function LessonPracticeLab(props: LessonPracticeLabProps) {
  const { courseTitle, lessonTitle, lessonMarkdown, onSaveToNotebook, onCreateFlashcardQuick } = props;
  const { activeUserHeaderId } = useWorkspaceStore();

  const defaultMode = useMemo(
    () => detectPreferredMode(courseTitle, lessonTitle, lessonMarkdown),
    [courseTitle, lessonTitle, lessonMarkdown]
  );

  const [mode, setMode] = useState<PracticeMode>(defaultMode);
  useEffect(() => {
    setMode(defaultMode);
  }, [defaultMode, lessonTitle]);

  const [labMessage, setLabMessage] = useState<string | null>(null);
  const notify = useCallback((msg: string) => {
    setLabMessage(msg);
    setTimeout(() => setLabMessage((c) => (c === msg ? null : c)), 4000);
  }, []);

  const handleSaveToNotebookWrapped = useCallback(
    async (textToAppend: string) => {
      try {
        await onSaveToNotebook(textToAppend);
        notify("Đã lưu vào Sổ tay bài học!");
      } catch {
        notify("Không thể lưu vào Sổ tay lúc này.");
      }
    },
    [onSaveToNotebook, notify]
  );

  const handleCreateFlashcardQuickWrapped = useCallback(
    async (front: string, back: string) => {
      if (!onCreateFlashcardQuick) return;
      try {
        await onCreateFlashcardQuick(front, back);
        notify("Đã tạo thẻ Flashcard ôn tập!");
      } catch {
        notify("Không thể tạo thẻ Flashcard lúc này.");
      }
    },
    [onCreateFlashcardQuick, notify]
  );

  const subLabProps = useMemo(
    () => ({
      ...props,
      onSaveToNotebook: handleSaveToNotebookWrapped,
      onCreateFlashcardQuick: handleCreateFlashcardQuickWrapped,
    }),
    [props, handleSaveToNotebookWrapped, handleCreateFlashcardQuickWrapped]
  );

  // Sub-tab for "recall" mode: either Psychology/Life Skills simulation OR Feynman Active Recall
  const isPsychologyContext = useMemo(() => {
    const combined = stripDiacritics(`${courseTitle} ${lessonTitle} ${lessonMarkdown.slice(0, 1200)}`);
    return /\b(tam ly|ky nang|giao tiep|thao tung|nhan thuc|cbt|life skills|psychology|cam xuc|stress|tu duy|thuyet trinh|dam phan)\b/.test(
      combined
    );
  }, [courseTitle, lessonTitle, lessonMarkdown]);

  const [recallSubMode, setRecallSubMode] = useState<"psychology" | "feynman">(
    isPsychologyContext ? "psychology" : "feynman"
  );

  useEffect(() => {
    setRecallSubMode(isPsychologyContext ? "psychology" : "feynman");
  }, [isPsychologyContext, lessonTitle]);

  // Feynman Active Recall States
  const keyTerms = useMemo(
    () => extractKeyTerms(lessonMarkdown, lessonTitle),
    [lessonMarkdown, lessonTitle]
  );
  const [recallText, setRecallText] = useState("");
  const [recallChecked, setRecallChecked] = useState(false);
  const [recallAiFeedback, setRecallAiFeedback] = useState("");
  const [recallAiLoading, setRecallAiLoading] = useState(false);

  // Concept Quick Self-Check Cards
  const conceptCards = useMemo(
    () => extractConceptCards(lessonMarkdown, lessonTitle, keyTerms),
    [lessonMarkdown, lessonTitle, keyTerms]
  );
  const [flippedCards, setFlippedCards] = useState<Record<string, boolean>>({});
  const [savedCardIds, setSavedCardIds] = useState<Record<string, boolean>>({});
  const [savingCardId, setSavingCardId] = useState<string | null>(null);

  const toggleFlipCard = (cardId: string) => {
    setFlippedCards((prev) => ({ ...prev, [cardId]: !prev[cardId] }));
  };

  const handleSaveCardToFlashcard = async (card: ConceptCheckCard) => {
    if (savedCardIds[card.id] || savingCardId) return;
    setSavingCardId(card.id);
    try {
      await handleCreateFlashcardQuickWrapped(card.front, card.back);
      setSavedCardIds((prev) => ({ ...prev, [card.id]: true }));
    } finally {
      setSavingCardId(null);
    }
  };

  const recallCoverage = useMemo(() => {
    const normInput = stripDiacritics(recallText);
    const covered: string[] = [];
    const missed: string[] = [];
    for (const term of keyTerms) {
      const normTerm = stripDiacritics(term);
      const tokens = normTerm.split(/\s+/).filter((t) => t.length >= 2);
      const hit =
        normInput.includes(normTerm) ||
        (tokens.length > 0 &&
          tokens.filter((t) => normInput.includes(t)).length >= Math.ceil(tokens.length * 0.6));
      if (hit) covered.push(term);
      else missed.push(term);
    }
    const percent = keyTerms.length > 0 ? Math.round((covered.length * 100) / keyTerms.length) : 0;
    return { covered, missed, percent };
  }, [keyTerms, recallText]);

  const handleRecallAiCheck = async () => {
    if (!recallText.trim()) {
      notify("Hãy viết tóm tắt bằng lời của bạn trước khi nhờ AI đối chiếu.");
      return;
    }
    setRecallAiLoading(true);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders({
        "Content-Type": "application/json",
        ...(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : {}),
      });
      const resp = await fetch("/api/v1/ai/explain", {
        method: "POST",
        headers,
        body: JSON.stringify({
          excerpt: `Tự diễn đạt bài "${lessonTitle}": ${recallText.trim().slice(0, 600)}`,
          contextTitle: lessonTitle,
          actionType: "explain",
        }),
      });
      if (resp.ok) {
        const data = await resp.json();
        setRecallAiFeedback(data.explanation || "");
      } else {
        setRecallAiFeedback(
          "Không gọi được AI lúc này. Hãy xem danh sách ý cốt lõi bên trên để bổ sung."
        );
      }
    } catch {
      setRecallAiFeedback("Lỗi kết nối khi gọi AI nhận xét.");
    } finally {
      setRecallAiLoading(false);
    }
  };

  const PRACTICE_MODES = [
    { id: "english" as const, label: "Tiếng Anh · Nghe & Nói", icon: Headphones },
    { id: "code" as const, label: "Lập trình · Code & Săn lỗi", icon: Code2 },
    { id: "finance" as const, label: "Tài chính · Tính toán & Biểu đồ", icon: CandlestickChart },
    { id: "recall" as const, label: "Kỹ năng & Tình huống", icon: Brain },
  ];

  return (
    <div
      data-testid="lesson-practice-lab"
      className="flex-1 overflow-y-auto swms-thin-scrollbar px-3 sm:px-6 py-5"
    >
      <div className="max-w-5xl mx-auto space-y-4">
        {/* Practice Header + 4 Mode Switchers */}
        <div className="flex flex-wrap items-center justify-between gap-3 pb-3 border-b border-border">
          <div>
            <p className="text-xs font-medium text-accent mb-0.5">Phòng thực hành chuyên sâu theo bài học</p>
            <h2 className="text-base sm:text-lg font-semibold text-text-primary">{lessonTitle}</h2>
          </div>

          <div className="inline-flex flex-wrap items-center gap-1 rounded-xl border border-border bg-bg-secondary p-1">
            {PRACTICE_MODES.map((m) => {
              const Icon = m.icon;
              const isActive = mode === m.id;
              return (
                <button
                  key={m.id}
                  type="button"
                  data-testid={`practice-mode-${m.id}`}
                  onClick={() => setMode(m.id)}
                  className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-lg text-xs font-medium transition cursor-pointer ${
                    isActive
                      ? "bg-accent text-white shadow-xs font-semibold"
                      : "text-text-secondary hover:text-text-primary hover:bg-bg-primary"
                  }`}
                >
                  <Icon className="w-3.5 h-3.5 shrink-0" />
                  <span>{m.label}</span>
                </button>
              );
            })}
          </div>
        </div>

        {labMessage && (
          <div
            data-testid="practice-lab-notification"
            className="rounded-lg border border-accent/30 bg-accent-subtle px-3.5 py-2.5 text-xs font-medium text-accent flex items-center gap-2 shadow-xs"
          >
            <CheckCircle2 className="w-4 h-4 shrink-0 text-accent" />
            <span>{labMessage}</span>
          </div>
        )}

        {/* ==================== 1. ENGLISH PRACTICE LAB ==================== */}
        {mode === "english" && <EnglishPracticeLab {...subLabProps} />}

        {/* ==================== 2. CODING PRACTICE LAB ==================== */}
        {mode === "code" && <CodingPracticeLab {...subLabProps} />}

        {/* ==================== 3. FINANCE PRACTICE LAB ==================== */}
        {mode === "finance" && <FinancePracticeLab {...subLabProps} />}

        {/* ==================== 4. PSYCHOLOGY & LIFE SKILLS / FEYNMAN RECALL ==================== */}
        {mode === "recall" && (
          <div className="space-y-4">
            {/* Sub-tab switcher to toggle between interactive psychology lab and Feynman self-explanation */}
            <div className="flex flex-wrap items-center justify-between gap-2 bg-bg-secondary/70 p-1.5 rounded-xl border border-border">
              <div className="flex items-center gap-1">
                <button
                  type="button"
                  onClick={() => setRecallSubMode("psychology")}
                  className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition cursor-pointer ${
                    recallSubMode === "psychology"
                      ? "bg-bg-primary text-text-primary shadow-xs font-semibold border border-border"
                      : "text-text-secondary hover:text-text-primary"
                  }`}
                >
                  <Users className="w-3.5 h-3.5 text-accent" />
                  <span>Mô phỏng tình huống & Tâm lý</span>
                </button>
                <button
                  type="button"
                  onClick={() => setRecallSubMode("feynman")}
                  className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition cursor-pointer ${
                    recallSubMode === "feynman"
                      ? "bg-bg-primary text-text-primary shadow-xs font-semibold border border-border"
                      : "text-text-secondary hover:text-text-primary"
                  }`}
                >
                  <Award className="w-3.5 h-3.5 text-accent" />
                  <span>Tự diễn đạt Feynman (Ghi nhớ ý chính)</span>
                </button>
              </div>
              <span className="text-[11px] text-text-tertiary px-2 hidden sm:inline">
                {recallSubMode === "psychology"
                  ? "Thực hành phản hồi & nhận diện bẫy tâm lý"
                  : "Kiểm tra độ hiểu sâu bằng cách tự giải thích"}
              </span>
            </div>

            {recallSubMode === "psychology" ? (
              <PsychologyLifeSkillsLab {...subLabProps} />
            ) : (
              <div className="space-y-4">
                <div className="rounded-xl border border-border bg-bg-primary p-5 space-y-4">
                  <div className="space-y-1">
                    <span className="text-xs font-semibold text-accent inline-flex items-center gap-1.5">
                      <Award className="w-4 h-4" />
                      Tự diễn đạt lại từ trang trắng (Phương pháp Feynman)
                    </span>
                    <p className="text-xs text-text-secondary leading-relaxed">
                      Không nhìn lại bài giảng &quot;{lessonTitle}&quot;, hãy giải thích ngắn gọn những ý quan trọng nhất như thể bạn đang giảng lại cho một người mới bắt đầu.
                    </p>
                  </div>

                  <textarea
                    rows={7}
                    data-testid="practice-feynman-input"
                    value={recallText}
                    onChange={(e) => {
                      setRecallText(e.target.value);
                      setRecallChecked(false);
                    }}
                    placeholder="Gõ phần tự tóm tắt, công thức hoặc lập luận tình huống của bạn tại đây..."
                    className="w-full p-3.5 rounded-lg border border-border bg-bg-secondary text-sm text-text-primary leading-relaxed focus:outline-none focus:border-accent"
                  />

                  <div className="flex flex-wrap items-center gap-2">
                    <button
                      type="button"
                      data-testid="practice-feynman-submit-btn"
                      onClick={() => setRecallChecked(true)}
                      className="inline-flex items-center gap-1.5 h-8 px-4 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover transition cursor-pointer"
                    >
                      <BookOpen className="w-3.5 h-3.5" />
                      <span>Đối chiếu ý cốt lõi</span>
                    </button>
                    <button
                      type="button"
                      disabled={recallAiLoading}
                      onClick={() => void handleRecallAiCheck()}
                      className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border bg-bg-secondary text-xs font-medium text-text-primary hover:bg-bg-tertiary transition cursor-pointer"
                    >
                      <Sparkles className="w-3.5 h-3.5 text-accent" />
                      <span>{recallAiLoading ? "Đang chấm..." : "AI góp ý lỗ hổng"}</span>
                    </button>
                    <button
                      type="button"
                      disabled={!recallText.trim()}
                      onClick={() =>
                        void handleSaveToNotebookWrapped(
                          `### 🧠 Tự diễn đạt (${lessonTitle})\n${recallText.trim()}\n${
                            recallAiFeedback ? `\n> Góp ý AI: ${recallAiFeedback}` : ""
                          }`
                        )
                      }
                      className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary disabled:opacity-50 cursor-pointer"
                    >
                      <NotebookPen className="w-3.5 h-3.5" />
                      <span>Lưu vào Sổ tay</span>
                    </button>
                  </div>

                  {recallChecked && (
                    <div className="rounded-lg border border-border bg-bg-secondary/50 p-3.5 space-y-2 text-xs">
                      <p className="font-semibold text-text-primary">
                        Độ bao phủ khái niệm: {recallCoverage.percent}% ({recallCoverage.covered.length}/
                        {keyTerms.length})
                      </p>
                      <div className="flex flex-wrap gap-1.5">
                        {recallCoverage.covered.map((t) => (
                          <span
                            key={t}
                            className="px-2 py-0.5 rounded bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 font-medium"
                          >
                            ✓ {t}
                          </span>
                        ))}
                        {recallCoverage.missed.map((t) => (
                          <span
                            key={t}
                            className="px-2 py-0.5 rounded bg-amber-500/15 text-amber-700 dark:text-amber-300 font-medium"
                          >
                            Cần bổ sung: {t}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}

                  {recallAiFeedback && (
                    <div className="rounded-lg border border-border bg-bg-secondary/40 p-3 text-xs text-text-primary whitespace-pre-wrap leading-relaxed">
                      {recallAiFeedback}
                    </div>
                  )}
                </div>

                {/* Concept Quick Self-Check Cards */}
                {conceptCards.length > 0 && (
                  <div className="rounded-xl border border-border bg-bg-primary p-5 space-y-4">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <div className="space-y-0.5">
                        <span className="text-xs font-semibold text-accent inline-flex items-center gap-1.5">
                          <Layers className="w-4 h-4" />
                          Thẻ Tự Kiểm Tra Khái Niệm (Lật để nhớ nhanh)
                        </span>
                        <p className="text-xs text-text-secondary">
                          Chạm vào thẻ để lật xem định nghĩa cốt lõi, hoặc bấm nút lưu thành thẻ Flashcard ôn tập Spaced Repetition.
                        </p>
                      </div>
                      <span className="text-[11px] font-medium text-text-tertiary bg-bg-secondary px-2.5 py-1 rounded-full border border-border">
                        {conceptCards.length} thẻ khái niệm
                      </span>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                      {conceptCards.map((card) => {
                        const isFlipped = Boolean(flippedCards[card.id]);
                        const isSaved = Boolean(savedCardIds[card.id]);
                        const isSaving = savingCardId === card.id;

                        return (
                          <div
                            key={card.id}
                            className={`rounded-lg border transition-all flex flex-col justify-between p-3.5 min-h-[145px] text-xs ${
                              isFlipped
                                ? "border-accent/40 bg-accent-subtle/30"
                                : "border-border bg-bg-secondary/60 hover:border-accent/30"
                            }`}
                          >
                            <div
                              onClick={() => toggleFlipCard(card.id)}
                              className="cursor-pointer select-none space-y-2 flex-1"
                            >
                              <div className="flex items-center justify-between text-[11px] text-text-tertiary">
                                <span className="font-semibold uppercase tracking-wider text-[10px] text-accent">
                                  {isFlipped ? "Định nghĩa / Giải thích" : "Khái niệm cốt lõi"}
                                </span>
                                <span className="inline-flex items-center gap-1 text-text-secondary hover:text-accent">
                                  <RotateCcw className="w-3 h-3" />
                                  <span>{isFlipped ? "Mặt trước" : "Lật xem"}</span>
                                </span>
                              </div>

                              <p
                                className={`leading-relaxed ${
                                  isFlipped
                                    ? "text-text-secondary font-normal"
                                    : "text-text-primary font-medium"
                                }`}
                              >
                                {isFlipped ? card.back : card.front}
                              </p>
                            </div>

                            <div className="pt-3 mt-2 border-t border-border/60 flex items-center justify-between gap-2">
                              <button
                                type="button"
                                onClick={() => toggleFlipCard(card.id)}
                                className="text-[11px] text-text-secondary hover:text-text-primary underline cursor-pointer"
                              >
                                {isFlipped ? "← Xem từ khóa" : "Lật thẻ →"}
                              </button>

                              {onCreateFlashcardQuick && (
                                <button
                                  type="button"
                                  disabled={isSaved || isSaving}
                                  onClick={() => void handleSaveCardToFlashcard(card)}
                                  className={`inline-flex items-center gap-1 h-7 px-2.5 rounded text-[11px] font-medium transition cursor-pointer ${
                                    isSaved
                                      ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 font-semibold cursor-default"
                                      : "bg-accent text-white hover:bg-accent-hover disabled:opacity-50"
                                  }`}
                                >
                                  {isSaved ? (
                                    <>
                                      <Check className="w-3 h-3" />
                                      <span>✓ Đã lưu</span>
                                    </>
                                  ) : isSaving ? (
                                    <>
                                      <Loader2 className="w-3 h-3 animate-spin" />
                                      <span>Đang lưu...</span>
                                    </>
                                  ) : (
                                    <>
                                      <Plus className="w-3 h-3" />
                                      <span>Lưu thành thẻ Flashcard</span>
                                    </>
                                  )}
                                </button>
                              )}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export default LessonPracticeLab;
