"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import dynamic from "next/dynamic";
import {
  Award,
  BookOpen,
  Brain,
  CandlestickChart,
  CheckCircle2,
  Code2,
  Headphones,
  Loader2,
  NotebookPen,
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
    /\b(\.net|c#|csharp|react|typescript|javascript|sql|api|docker|ef core|backend|frontend|fullstack|git|code|lap trinh|bug|debug|tlb|eat|memory)\b/.test(
      combined
    ) ||
    /```(csharp|cs|ts|tsx|js|jsx|sql|python|py|bash|json)\b/i.test(md)
  ) {
    return "code";
  }
  return "recall";
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
  const { courseTitle, lessonTitle, lessonMarkdown, onSaveToNotebook } = props;
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

  // Sub-tab for "recall" mode: either Psychology/Life Skills simulation OR Feynman Active Recall
  const isPsychologyContext = useMemo(() => {
    const combined = stripDiacritics(`${courseTitle} ${lessonTitle} ${lessonMarkdown.slice(0, 1200)}`);
    return /\b(tam ly|ky nang|giao tiep|thao tung|nhan thuc|cbt|life skills|psychology|cam xuc|stress|tu duy|thuyet trinh|dam phan)\b/.test(
      combined
    );
  }, [courseTitle, lessonTitle, lessonMarkdown]);

  const [recallSubMode, setRecallSubMode] = useState<"psychology" | "feynman">(
    isPsychologyContext ? "psychology" : "psychology"
  );

  // Feynman Active Recall States
  const keyTerms = useMemo(
    () => extractKeyTerms(lessonMarkdown, lessonTitle),
    [lessonMarkdown, lessonTitle]
  );
  const [recallText, setRecallText] = useState("");
  const [recallChecked, setRecallChecked] = useState(false);
  const [recallAiFeedback, setRecallAiFeedback] = useState("");
  const [recallAiLoading, setRecallAiLoading] = useState(false);

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
          <div className="rounded-lg border border-accent/30 bg-accent-subtle px-3.5 py-2 text-xs font-medium text-accent">
            {labMessage}
          </div>
        )}

        {/* ==================== 1. ENGLISH PRACTICE LAB ==================== */}
        {mode === "english" && <EnglishPracticeLab {...props} />}

        {/* ==================== 2. CODING PRACTICE LAB ==================== */}
        {mode === "code" && <CodingPracticeLab {...props} />}

        {/* ==================== 3. FINANCE PRACTICE LAB ==================== */}
        {mode === "finance" && <FinancePracticeLab {...props} />}

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
              <PsychologyLifeSkillsLab {...props} />
            ) : (
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
                      void onSaveToNotebook(
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
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export default LessonPracticeLab;
