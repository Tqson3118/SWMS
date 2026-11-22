"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import dynamic from "next/dynamic";
import Link from "next/link";
import {
  AlertCircle,
  Award,
  BookOpen,
  Brain,
  CandlestickChart,
  Check,
  CheckCircle2,
  ClipboardCheck,
  Code2,
  Headphones,
  Layers,
  Loader2,
  NotebookPen,
  Plus,
  RotateCcw,
  Send,
  Sparkles,
  Trophy,
  Users,
  XCircle,
} from "lucide-react";
import { getClientRequestHeaders, useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";
import { formatCleanQuizPrompt } from "@/lib/course-helpers";
import { detectCourseLanguageFromText } from "@/lib/language-practice";
import { resolvePracticeSelection } from "@/lib/practice-helpers";

export interface LessonPracticeLabProps {
  courseId: string;
  courseTitle: string;
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  onSaveToNotebook: (textToAppend: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  isEmbedded?: boolean;
  initialActivityType?: string | null;
  initialActivityKey?: string | null;
}

type PracticeMode = "english" | "code" | "finance" | "recall";

interface QuizSummaryItem {
  id: string;
  title: string;
  lessonId?: string | null;
  documentId?: string | null;
  courseId?: string | null;
  totalItems: number;
  correctItems: number;
  status: string;
}

interface QuizChoiceItem {
  key: string;
  text: string;
}

interface QuizQuestionItem {
  id: string;
  quizId: string;
  itemType: string;
  prompt: { question?: string; rubricCriteria?: string[] } | string;
  choices?: QuizChoiceItem[] | null;
  clozeSolution?: string | null;
  userAnswer?: { text?: string } | null;
  isCorrect?: boolean | null;
  aiRubricFeedback?: {
    summary?: string;
    strengths?: string[];
    missingConcepts?: string[];
    masteryLevel?: string;
  } | null;
  evaluatedAt?: string | null;
}

interface QuizDetailData {
  id: string;
  title: string;
  lessonId?: string | null;
  totalItems: number;
  correctItems: number;
  status: string;
  items: QuizQuestionItem[];
}

interface RealFlashcardItem {
  id: string;
  lessonId?: string | null;
  sourceTitleSnapshot?: string | null;
  frontText: string;
  backText: string;
  cardType?: string | null;
  state?: string;
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function PracticeLabLoading({ label }: { label: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-12 px-4 space-y-3">
      <Loader2 className="w-7 h-7 text-accent animate-spin" />
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
  return (input || "")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase();
}

function extractUnitTokens(title: string): string[] {
  const norm = stripDiacritics(title);
  const tokens: string[] = [];
  const patterns = [
    /\bw0*(\d+)\b/g,
    /\b(?:tuan|week|bai|chang|phase|chuong|module)\s*0*(\d+)\b/g,
  ];
  for (const pat of patterns) {
    let m: RegExpExecArray | null;
    while ((m = pat.exec(norm)) !== null) {
      tokens.push(m[1]);
    }
  }
  return Array.from(new Set(tokens));
}

function detectPreferredMode(courseTitle: string, lessonTitle: string, md: string): PracticeMode {
  if (/```practice-finance\b/i.test(md)) return "finance";
  if (/```practice-english\b/i.test(md)) return "english";
  if (/```practice-code\b/i.test(md)) return "code";
  if (/```practice-case\b/i.test(md)) return "recall";

  const titleNorm = stripDiacritics(`${courseTitle} ${lessonTitle}`);
  // 1. Prioritize Languages (English, Japanese N5, Chinese HSK1) so language courses always open Language Practice Lab
  if (
    /\b(ielts|tieng anh|english|tieng nhat|japanese|jlpt|n5|n4|n3|hiragana|katakana|kanji|tieng trung|chinese|hsk|hsk1|hsk2|pinyin|hanzi|listening|speaking|vocabulary|grammar|phat am|shadowing|dictation|pronunciation|ipa)\b/.test(
      titleNorm
    ) ||
    /[\u3040-\u30ff]/.test(`${courseTitle} ${lessonTitle}`)
  ) {
    return "english";
  }
  if (
    /\b(tam ly|ky nang|giao tiep|cam xuc|cbt|life skills|psychology|eq|nvc|self-awareness|attachment)\b/.test(
      titleNorm
    )
  ) {
    return "recall";
  }
  if (
    /\b(tai chinh|dau tu|chung khoan|investing|finance|bieu do nen|candlestick|co phieu|ngan sach|lai kep|tra no|snowball|avalanche|personal finance|thue thu nhap)\b/.test(
      titleNorm
    )
  ) {
    return "finance";
  }
  if (
    /\b(\.net|c#|csharp|react|typescript|javascript|sql|api|docker|ef core|backend|frontend|fullstack|git|code|lap trinh|he dieu hanh|bo nho ao|phan trang|hoc may|machine learning|kien truc may tinh|microservices|python|fastapi|nodejs|kubernetes|algorithm|thuat toan|cau truc du lieu)\b/.test(
      titleNorm
    )
  ) {
    return "code";
  }

  // Fallback sang xét md.slice(0, 1500)
  const mdNorm = stripDiacritics(md.slice(0, 1500));
  if (
    /\b(ielts|tieng anh|english|tieng nhat|japanese|jlpt|n5|hiragana|katakana|kanji|tieng trung|chinese|hsk|hsk1|pinyin|hanzi|listening|speaking|vocabulary|grammar|phat am|shadowing|dictation|pronunciation|ipa)\b/.test(
      mdNorm
    ) ||
    /[\u3040-\u30ff]/.test(md.slice(0, 1500))
  ) {
    return "english";
  }
  if (
    /\b(tam ly|ky nang|giao tiep|cam xuc|cbt|life skills|psychology|eq|nvc|self-awareness|attachment)\b/.test(
      mdNorm
    )
  ) {
    return "recall";
  }
  if (
    /\b(tai chinh|dau tu|chung khoan|investing|finance|bieu do nen|nen nhat|candlestick|co phieu|dong tien|danh muc dau tu|trai phieu|ngan sach|lai kep|tra no|snowball|avalanche|personal finance|thue thu nhap)\b/.test(
      mdNorm
    )
  ) {
    return "finance";
  }
  if (
    /\b(\.net|c#|csharp|react|typescript|javascript|sql|api|docker|ef core|backend|frontend|fullstack|git|code|lap trinh|bug|debug|tlb|eat|memory|hoc may|machine learning|kien truc may tinh|computer architecture|he dieu hanh|bo nho ao|phan trang|microservices|ddd|event sourcing|python|fastapi|nodejs|kubernetes|algorithm|thuat toan|cau truc du lieu)\b/.test(
      mdNorm
    ) ||
    /```(csharp|cs|ts|tsx|js|jsx|sql|python|py|bash)\b/i.test(md)
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

function cleanMarkdownForPractice(md: string): string {
  if (!md) return "";
  return md
    .replace(/\[(?:C\d+-\d+|LO-[A-Z0-9-]+|Mạnh|Trung\s*bình|Yếu)\]/gi, "")
    .replace(/^\s*[-*]?\s*Ghi chú hiển thị Markdown[^\n]*$/gim, "")
    .replace(/[ \t]{2,}/g, " ")
    .trim();
}

const QUIZ_TYPE_LABELS: Record<string, string> = {
  multiple_choice: "Trắc nghiệm",
  cloze: "Điền khuyết",
  short_answer: "Trả lời ngắn",
  essay: "Tự luận",
};

export function LessonPracticeLab(props: LessonPracticeLabProps) {
  const {
    courseId,
    courseTitle,
    lessonId,
    lessonTitle,
    lessonMarkdown,
    onSaveToNotebook,
    onCreateFlashcardQuick,
    isEmbedded = false,
    initialActivityType,
    initialActivityKey,
  } = props;
  const { activeUserHeaderId } = useWorkspaceStore();

  const defaultMode = useMemo(
    () => detectPreferredMode(courseTitle, lessonTitle, lessonMarkdown),
    [courseTitle, lessonTitle, lessonMarkdown]
  );

  const resolvedTarget = useMemo(
    () =>
      resolvePracticeSelection(
        initialActivityType,
        initialActivityKey,
        courseTitle,
        lessonTitle,
        lessonMarkdown
      ),
    [initialActivityType, initialActivityKey, courseTitle, lessonTitle, lessonMarkdown]
  );

  const [mode, setMode] = useState<PracticeMode>(resolvedTarget?.labMode ?? defaultMode);
  useEffect(() => {
    setMode(resolvedTarget?.labMode ?? defaultMode);
  }, [resolvedTarget, defaultMode, lessonTitle]);

  const [labMessage, setLabMessage] = useState<string | null>(null);
  const notify = useCallback((msg: string) => {
    setLabMessage(msg);
    setTimeout(() => setLabMessage((c) => (c === msg ? null : c)), 4000);
  }, []);

  const buildAuthHeaders = useCallback(
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

  // =========================================================================
  // REAL DB QUIZZES & FLASHCARDS FOR CURRENT LESSON / COURSE
  // =========================================================================
  const [courseQuizzes, setCourseQuizzes] = useState<QuizSummaryItem[]>([]);
  const [selectedQuizId, setSelectedQuizId] = useState<string>("");
  const [quizDetail, setQuizDetail] = useState<QuizDetailData | null>(null);
  const [quizIsDirectMatch, setQuizIsDirectMatch] = useState(false);
  const [quizDrafts, setQuizDrafts] = useState<Record<string, string>>({});
  const [quizSubmitting, setQuizSubmitting] = useState<Record<string, boolean>>({});
  const [quizError, setQuizError] = useState<string | null>(null);
  const [lessonFlashcards, setLessonFlashcards] = useState<RealFlashcardItem[]>([]);
  const [flippedRealCards, setFlippedRealCards] = useState<Record<string, boolean>>({});
  const [loadingLessonAssets, setLoadingLessonAssets] = useState(false);
  const [activeAssetPanel, setActiveAssetPanel] = useState<"quiz" | "flashcards">("quiz");

  const loadQuizDetail = useCallback(
    async (qId: string) => {
      if (!qId) {
        setQuizDetail(null);
        return;
      }
      try {
        const res = await fetch(`/api/v1/practice-quizzes/${qId}`, {
          headers: buildAuthHeaders(),
        });
        if (!res.ok) return;
        const detail: QuizDetailData = await res.json();
        setQuizDetail(detail);
        const drafts: Record<string, string> = {};
        for (const item of detail.items ?? []) {
          if (item.userAnswer?.text) drafts[item.id] = item.userAnswer.text;
        }
        setQuizDrafts(drafts);
      } catch {
        // silent fallback
      }
    },
    [buildAuthHeaders]
  );

  useEffect(() => {
    let active = true;
    const isValidCourseGuid = UUID_REGEX.test((courseId || "").trim());
    if (!isValidCourseGuid) {
      setCourseQuizzes([]);
      setQuizDetail(null);
      setLessonFlashcards([]);
      return;
    }

    const fetchLessonAssets = async () => {
      setLoadingLessonAssets(true);
      setQuizError(null);
      try {
        const headers = buildAuthHeaders();
        const [quizzesRes, cardsRes] = await Promise.all([
          fetch(`/api/v1/practice-quizzes?courseId=${encodeURIComponent(courseId)}`, {
            headers,
          }).catch(() => null),
          fetch(`/api/v1/flashcards/due?courseId=${encodeURIComponent(courseId)}&includeAll=true`, {
            headers,
          }).catch(() => null),
        ]);

        if (!active) return;

        if (quizzesRes && quizzesRes.ok) {
          const list: QuizSummaryItem[] = await quizzesRes.json();
          const safeList = Array.isArray(list) ? list : [];
          setCourseQuizzes(safeList);

          const lessonTokens = extractUnitTokens(lessonTitle);
          const normLesson = stripDiacritics(lessonTitle);

          const directById = lessonId
            ? safeList.find((q) => q.lessonId && q.lessonId === lessonId)
            : undefined;
          const directByTitle = safeList.find((q) => {
            const qt = stripDiacritics(q.title || "");
            if (normLesson && qt.includes(normLesson.slice(0, 18))) return true;
            if (lessonTokens.length > 0) {
              const qTokens = extractUnitTokens(q.title || "");
              return lessonTokens.some((t) => qTokens.includes(t));
            }
            return false;
          });

          const chosen = directById || directByTitle || safeList[0] || null;
          setQuizIsDirectMatch(Boolean(directById || directByTitle));
          if (chosen) {
            setSelectedQuizId(chosen.id);
            await loadQuizDetail(chosen.id);
          } else {
            setSelectedQuizId("");
            setQuizDetail(null);
          }
        }

        if (cardsRes && cardsRes.ok) {
          const cardsPayload = await cardsRes.json();
          const allCards: RealFlashcardItem[] = Array.isArray(cardsPayload)
            ? cardsPayload
            : Array.isArray(cardsPayload?.cards)
              ? cardsPayload.cards
              : [];

          const lessonTokens = extractUnitTokens(lessonTitle);
          const matchedCards = allCards.filter((c) => {
            if (lessonId && c.lessonId === lessonId) return true;
            if (c.sourceTitleSnapshot && lessonTokens.length > 0) {
              const cTokens = extractUnitTokens(c.sourceTitleSnapshot);
              if (lessonTokens.some((t) => cTokens.includes(t))) return true;
            }
            return false;
          });

          const finalCards = (matchedCards.length > 0 ? matchedCards : allCards).slice(0, 12);
          setLessonFlashcards(finalCards);
        }
      } catch {
        // silent
      } finally {
        if (active) setLoadingLessonAssets(false);
      }
    };

    void fetchLessonAssets();
    return () => {
      active = false;
    };
  }, [courseId, lessonId, lessonTitle, buildAuthHeaders, loadQuizDetail]);

  const handleSubmitRealQuizItem = async (item: QuizQuestionItem) => {
    if (!quizDetail) return;
    const answerText = (quizDrafts[item.id] ?? "").trim();
    if (!answerText) return;

    setQuizSubmitting((prev) => ({ ...prev, [item.id]: true }));
    setQuizError(null);
    try {
      const resp = await fetch(
        `/api/v1/practice-quizzes/${quizDetail.id}/items/${item.id}/submit`,
        {
          method: "POST",
          headers: buildAuthHeaders({ "Content-Type": "application/json" }),
          credentials: "include",
          body: JSON.stringify({
            answer: answerText,
            idempotencyKey: crypto.randomUUID(),
            waitForAiCompletion: true,
          }),
        }
      );

      if (!resp.ok) {
        const errBody = await resp.json().catch(() => null);
        if (resp.status === 409) {
          setQuizError(errBody?.error || "Câu hỏi này đã được chấm trước đó.");
          await loadQuizDetail(quizDetail.id);
        } else if (resp.status === 429) {
          setQuizError(errBody?.error || "Hết hạn mức AI credits hôm nay.");
        } else {
          setQuizError(errBody?.error || `Không thể nộp câu trả lời (HTTP ${resp.status}).`);
        }
        return;
      }

      const result = await resp.json();
      if (result.jobId && result.jobStatus && result.jobStatus !== "completed") {
        for (let i = 0; i < 16; i++) {
          await new Promise((r) => setTimeout(r, 600));
          const jobResp = await fetch(`/api/v1/jobs/${result.jobId}`, {
            headers: buildAuthHeaders(),
          });
          if (jobResp.ok) {
            const jobData = await jobResp.json();
            if (jobData.status === "completed" || jobData.status === "failed") break;
          }
        }
      }

      await loadQuizDetail(quizDetail.id);
      invalidateDashboardCache();
      if (typeof window !== "undefined") {
        window.dispatchEvent(new Event("swms:data-mutated"));
      }
      notify(result.isCorrect ? "Chính xác! Đã ghi nhận kết quả." : "Đã chấm xong câu hỏi.");
    } catch {
      setQuizError("Lỗi kết nối khi chấm câu hỏi.");
    } finally {
      setQuizSubmitting((prev) => ({ ...prev, [item.id]: false }));
    }
  };

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

  const cleanMarkdown = useMemo(
    () => cleanMarkdownForPractice(lessonMarkdown),
    [lessonMarkdown]
  );

  const subLabProps = useMemo(
    () => ({
      ...props,
      lessonMarkdown: cleanMarkdown,
      onSaveToNotebook: handleSaveToNotebookWrapped,
      onCreateFlashcardQuick: handleCreateFlashcardQuickWrapped,
    }),
    [props, cleanMarkdown, handleSaveToNotebookWrapped, handleCreateFlashcardQuickWrapped]
  );

  const isPsychologyContext = useMemo(() => {
    const combined = stripDiacritics(`${courseTitle} ${lessonTitle} ${cleanMarkdown.slice(0, 1200)}`);
    return /\b(tam ly|ky nang|giao tiep|thao tung|nhan thuc|cbt|life skills|psychology|cam xuc|stress|tu duy|thuyet trinh|dam phan)\b/.test(
      combined
    );
  }, [courseTitle, lessonTitle, cleanMarkdown]);

  const [recallSubMode, setRecallSubMode] = useState<"psychology" | "feynman">(
    resolvedTarget?.recallSubMode ?? (isPsychologyContext ? "psychology" : "feynman")
  );

  useEffect(() => {
    setRecallSubMode(
      resolvedTarget?.recallSubMode ?? (isPsychologyContext ? "psychology" : "feynman")
    );
  }, [resolvedTarget, isPsychologyContext, lessonTitle]);

  const keyTerms = useMemo(
    () => extractKeyTerms(cleanMarkdown, lessonTitle),
    [cleanMarkdown, lessonTitle]
  );
  const [recallText, setRecallText] = useState("");
  const [recallChecked, setRecallChecked] = useState(false);
  const [recallAiFeedback, setRecallAiFeedback] = useState("");
  const [recallAiLoading, setRecallAiLoading] = useState(false);

  const conceptCards = useMemo(
    () => extractConceptCards(cleanMarkdown, lessonTitle, keyTerms),
    [cleanMarkdown, lessonTitle, keyTerms]
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
      const resp = await fetch("/api/v1/ai/explain", {
        method: "POST",
        headers: buildAuthHeaders({ "Content-Type": "application/json" }),
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

  const detectedLanguage = useMemo(
    () => detectCourseLanguageFromText(courseTitle, lessonTitle, lessonMarkdown),
    [courseTitle, lessonTitle, lessonMarkdown]
  );

  const languageModeLabel =
    detectedLanguage === "ja-JP"
      ? "Tiếng Nhật (N5) · Nghe & Nói"
      : detectedLanguage === "zh-CN"
      ? "Tiếng Trung (HSK1) · Nghe & Nói"
      : "Tiếng Anh · Nghe & Nói";

  const PRACTICE_MODES = [
    { id: "english" as const, label: languageModeLabel, icon: Headphones },
    { id: "code" as const, label: "Lập trình · Code & Săn lỗi", icon: Code2 },
    { id: "finance" as const, label: "Tài chính · Tính toán & Biểu đồ", icon: CandlestickChart },
    { id: "recall" as const, label: "Kỹ năng & Tình huống", icon: Brain },
  ];

  const hasRealAssets = Boolean(quizDetail && quizDetail.items?.length > 0) || lessonFlashcards.length > 0;

  return (
    <div
      data-testid={isEmbedded ? "lesson-practice-lab-inline" : "lesson-practice-lab"}
      className={
        isEmbedded
          ? "w-full pt-8 mt-8 border-t-2 border-dashed border-border space-y-5 font-sans"
          : "flex-1 overflow-y-auto swms-thin-scrollbar px-3 sm:px-6 py-5"
      }
    >
      <div className={isEmbedded ? "space-y-5" : "max-w-5xl mx-auto space-y-5"}>
        {/* Practice Header + 4 Mode Switchers */}
        <div className="flex flex-wrap items-center justify-between gap-3 pb-3 border-b border-border">
          <div>
            <p className="text-xs font-semibold text-accent mb-0.5 flex items-center gap-1.5">
              <Sparkles className="w-3.5 h-3.5" />
              <span>
                {isEmbedded
                  ? "Thực hành ngay cho bài học này"
                  : mode === "english" && detectedLanguage === "ja-JP"
                  ? "Phòng thực hành Tiếng Nhật (N5)"
                  : mode === "english" && detectedLanguage === "zh-CN"
                  ? "Phòng thực hành Tiếng Trung (HSK1)"
                  : "Phòng thực hành chuyên sâu theo bài học"}
              </span>
            </p>
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

        {/* ==================== REAL DB QUIZ & FLASHCARDS OF LESSON ==================== */}
        {(loadingLessonAssets || hasRealAssets) && (
          <section
            data-testid="lesson-real-assets-panel"
            className="rounded-xl border border-indigo-200/80 dark:border-indigo-900/60 bg-gradient-to-br from-indigo-50/40 via-bg-primary to-sky-50/30 dark:from-indigo-950/20 dark:via-bg-primary dark:to-slate-900/50 p-4 sm:p-5 space-y-4"
          >
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div className="space-y-0.5">
                <div className="flex flex-wrap items-center gap-2">
                  <span className="inline-flex items-center gap-1.5 text-xs font-bold text-accent">
                    <ClipboardCheck className="w-4 h-4" />
                    <span>Bài Kiểm Tra &amp; Thẻ Ghi Nhớ Của Bài Học</span>
                  </span>
                  {quizDetail && (
                    <span className="px-2 py-0.5 rounded-full text-[10px] font-semibold bg-bg-secondary border border-border text-text-secondary">
                      {quizIsDirectMatch ? "Khớp bài đang học" : "Thuộc giáo trình khóa"}
                    </span>
                  )}
                </div>
                <p className="text-xs text-text-secondary">
                  Luyện trực tiếp bộ câu hỏi trắc nghiệm / điền từ thật và thẻ ghi nhớ FSRS gắn với khóa học.
                </p>
              </div>

              <div className="flex flex-wrap items-center gap-1.5">
                {quizDetail && quizDetail.items?.length > 0 && (
                  <button
                    type="button"
                    onClick={() => setActiveAssetPanel("quiz")}
                    className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-lg text-xs font-semibold transition cursor-pointer ${
                      activeAssetPanel === "quiz"
                        ? "bg-accent text-white shadow-xs"
                        : "bg-bg-secondary text-text-secondary border border-border hover:text-text-primary"
                    }`}
                  >
                    <Trophy className="w-3.5 h-3.5" />
                    <span>Quiz thật ({quizDetail.items.length} câu)</span>
                  </button>
                )}
                {lessonFlashcards.length > 0 && (
                  <button
                    type="button"
                    onClick={() => setActiveAssetPanel("flashcards")}
                    className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-lg text-xs font-semibold transition cursor-pointer ${
                      activeAssetPanel === "flashcards"
                        ? "bg-accent text-white shadow-xs"
                        : "bg-bg-secondary text-text-secondary border border-border hover:text-text-primary"
                    }`}
                  >
                    <Brain className="w-3.5 h-3.5" />
                    <span>Thẻ nhớ ({lessonFlashcards.length})</span>
                  </button>
                )}
                {UUID_REGEX.test((courseId || "").trim()) && (
                  <Link
                    href={`/app/review?courseId=${courseId}`}
                    className="inline-flex items-center gap-1 h-8 px-3 rounded-lg border border-border bg-bg-primary text-xs font-medium text-text-primary hover:border-accent transition"
                  >
                    <span>Ôn chuyên sâu toàn khóa →</span>
                  </Link>
                )}
              </div>
            </div>

            {loadingLessonAssets && !hasRealAssets ? (
              <div className="flex items-center gap-2 text-xs text-text-secondary py-2">
                <Loader2 className="w-4 h-4 animate-spin text-accent" />
                <span>Đang tải câu hỏi kiểm tra và thẻ ghi nhớ thật của bài học...</span>
              </div>
            ) : activeAssetPanel === "quiz" && quizDetail && quizDetail.items?.length > 0 ? (
              <div className="space-y-3">
                {courseQuizzes.length > 1 && (
                  <div className="flex flex-wrap items-center justify-between gap-2 pb-2 border-b border-border/60">
                    <span className="text-xs font-semibold text-text-primary">
                      {quizDetail.title} ({quizDetail.correctItems}/{quizDetail.totalItems} đúng)
                    </span>
                    <select
                      aria-label="Chọn bộ câu hỏi của khóa"
                      value={selectedQuizId}
                      onChange={(e) => {
                        const nextId = e.target.value;
                        setSelectedQuizId(nextId);
                        void loadQuizDetail(nextId);
                      }}
                      className="h-8 max-w-xs rounded-md border border-border bg-bg-primary px-2.5 text-xs text-text-primary focus:outline-none focus:border-accent"
                    >
                      {courseQuizzes.map((q) => (
                        <option key={q.id} value={q.id}>
                          {q.title} ({q.correctItems}/{q.totalItems})
                        </option>
                      ))}
                    </select>
                  </div>
                )}

                {quizError && (
                  <div className="rounded-lg border border-rose-300 dark:border-rose-800 bg-rose-50 dark:bg-rose-950/30 px-3 py-2 text-xs text-rose-700 dark:text-rose-300 flex items-center gap-2">
                    <AlertCircle className="w-3.5 h-3.5 shrink-0" />
                    <span>{quizError}</span>
                  </div>
                )}

                <div className="grid grid-cols-1 gap-3">
                  {quizDetail.items.slice(0, 6).map((item, idx) => {
                    const promptObj =
                      typeof item.prompt === "string" ? { question: item.prompt } : item.prompt;
                    const rawQ = promptObj?.question || `Câu hỏi #${idx + 1}`;
                    const evaluated = Boolean(item.evaluatedAt);
                    const cleanQ = formatCleanQuizPrompt(rawQ, evaluated, item.clozeSolution);
                    const draft = quizDrafts[item.id] ?? "";
                    const isSub = Boolean(quizSubmitting[item.id]);

                    return (
                      <div
                        key={item.id}
                        className="rounded-xl border border-border bg-bg-primary p-4 space-y-3"
                      >
                        <div className="flex items-start justify-between gap-2">
                          <div className="space-y-1">
                            <span className="inline-block text-[10px] font-semibold px-2 py-0.5 rounded bg-accent-subtle text-accent">
                              Câu {idx + 1} · {QUIZ_TYPE_LABELS[item.itemType] ?? item.itemType}
                            </span>
                            <p className="text-sm font-medium text-text-primary leading-relaxed">
                              {cleanQ}
                            </p>
                          </div>
                          {evaluated && item.isCorrect !== null && (
                            <span
                              className={`inline-flex items-center gap-1 px-2 py-0.5 rounded text-[11px] font-semibold shrink-0 ${
                                item.isCorrect
                                  ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                                  : "bg-rose-500/15 text-rose-700 dark:text-rose-300"
                              }`}
                            >
                              {item.isCorrect ? (
                                <>
                                  <CheckCircle2 className="w-3 h-3" />
                                  <span>Đúng</span>
                                </>
                              ) : (
                                <>
                                  <XCircle className="w-3 h-3" />
                                  <span>Chưa đúng</span>
                                </>
                              )}
                            </span>
                          )}
                        </div>

                        {item.itemType === "multiple_choice" &&
                        Array.isArray(item.choices) &&
                        item.choices.length > 0 ? (
                          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                            {item.choices.map((ch) => {
                              const selected = draft === ch.key;
                              return (
                                <label
                                  key={ch.key}
                                  className={`flex items-start gap-2.5 p-2.5 rounded-lg border text-xs cursor-pointer transition ${
                                    selected
                                      ? "border-accent bg-accent-subtle text-text-primary"
                                      : "border-border bg-bg-secondary/60 hover:border-accent/50 text-text-secondary"
                                  }`}
                                >
                                  <input
                                    type="radio"
                                    name={`inline-quiz-${item.id}`}
                                    value={ch.key}
                                    checked={selected}
                                    disabled={evaluated}
                                    onChange={() =>
                                      setQuizDrafts((prev) => ({ ...prev, [item.id]: ch.key }))
                                    }
                                    className="mt-0.5 accent-accent"
                                  />
                                  <span className="font-mono font-bold text-[11px] px-1.5 py-0.5 rounded bg-bg-primary border border-border shrink-0">
                                    {ch.key}
                                  </span>
                                  <span className="text-text-primary leading-snug">{ch.text}</span>
                                </label>
                              );
                            })}
                          </div>
                        ) : item.itemType === "essay" ? (
                          <textarea
                            rows={2}
                            value={draft}
                            onChange={(e) =>
                              setQuizDrafts((prev) => ({ ...prev, [item.id]: e.target.value }))
                            }
                            placeholder="Nhập câu trả lời tự luận ngắn gọn..."
                            className="w-full p-2.5 rounded-lg border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                          />
                        ) : (
                          <input
                            type="text"
                            value={draft}
                            disabled={evaluated}
                            onChange={(e) =>
                              setQuizDrafts((prev) => ({ ...prev, [item.id]: e.target.value }))
                            }
                            placeholder="Điền đáp án của bạn..."
                            className="w-full h-9 px-3 rounded-lg border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                          />
                        )}

                        <div className="flex flex-wrap items-center justify-between gap-2">
                          {evaluated && item.clozeSolution ? (
                            <span className="text-xs text-text-secondary">
                              Đáp án chuẩn:{" "}
                              <strong className="text-emerald-600 dark:text-emerald-400">
                                {item.clozeSolution}
                              </strong>
                            </span>
                          ) : (
                            <span />
                          )}
                          <button
                            type="button"
                            disabled={isSub || (item.itemType !== "essay" && evaluated) || !draft.trim()}
                            onClick={() => void handleSubmitRealQuizItem(item)}
                            className="inline-flex items-center gap-1.5 h-8 px-3 rounded-lg bg-accent hover:bg-accent-hover disabled:opacity-50 text-white text-xs font-semibold transition cursor-pointer"
                          >
                            {isSub ? (
                              <Loader2 className="w-3.5 h-3.5 animate-spin" />
                            ) : (
                              <Send className="w-3.5 h-3.5" />
                            )}
                            <span>
                              {isSub
                                ? "Đang chấm..."
                                : item.itemType !== "essay" && evaluated
                                  ? "Đã chấm"
                                  : "Kiểm tra câu này"}
                            </span>
                          </button>
                        </div>

                        {item.aiRubricFeedback?.summary && (
                          <div className="rounded-lg border border-accent/25 bg-accent-subtle/60 p-3 text-xs text-text-primary">
                            <span className="font-semibold text-accent">Nhận xét AI: </span>
                            {item.aiRubricFeedback.summary}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            ) : lessonFlashcards.length > 0 ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                {lessonFlashcards.map((card) => {
                  const flipped = Boolean(flippedRealCards[card.id]);
                  const cleanFront = formatCleanQuizPrompt(card.frontText, flipped);
                  return (
                    <button
                      key={card.id}
                      type="button"
                      onClick={() =>
                        setFlippedRealCards((prev) => ({ ...prev, [card.id]: !prev[card.id] }))
                      }
                      className={`text-left rounded-xl border p-4 min-h-[132px] flex flex-col justify-between transition cursor-pointer ${
                        flipped
                          ? "border-accent bg-accent-subtle/40"
                          : "border-border bg-bg-primary hover:border-accent/50"
                      }`}
                    >
                      <div className="space-y-1.5">
                        <div className="flex items-center justify-between text-[10px] font-semibold uppercase tracking-wider text-text-tertiary">
                          <span className="text-accent">{flipped ? "Đáp án / Giải thích" : "Thẻ FSRS"}</span>
                          <span>{card.state || "Ôn tập"}</span>
                        </div>
                        <p className="text-xs sm:text-sm font-medium text-text-primary leading-relaxed">
                          {flipped ? card.backText || cleanFront : cleanFront}
                        </p>
                      </div>
                      <span className="text-[11px] text-accent font-medium mt-3 inline-flex items-center gap-1">
                        <RotateCcw className="w-3 h-3" />
                        <span>{flipped ? "Xem câu hỏi" : "Chạm để lật đáp án"}</span>
                      </span>
                    </button>
                  );
                })}
              </div>
            ) : null}
          </section>
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
            <div className="flex flex-wrap items-center justify-between gap-2 bg-bg-secondary/70 p-1.5 rounded-xl border border-border">
              <div className="flex flex-wrap items-center gap-1">
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
                  <span>Mô phỏng tình huống &amp; Tâm lý</span>
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
                    rows={6}
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
