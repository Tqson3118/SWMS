"use client";

import React, { Suspense, useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useParams, useSearchParams } from "next/navigation";
import {
  AlertCircle,
  ArrowLeft,
  BookOpen,
  Brain,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  ClipboardCheck,
  Clock,
  Loader2,
  Plus,
  RotateCcw,
  Search,
  Send,
  Sparkles,
  Timer,
  Trophy,
  X,
  XCircle,
} from "lucide-react";
import {
  getClientRequestHeaders,
  useWorkspaceStore,
} from "@/stores/useWorkspaceStore";
import { formatCleanQuizPrompt } from "@/lib/course-helpers";
import { RichInlineMarkdown } from "@/lib/math-markdown";

interface QuizListItem {
  id: string;
  title: string;
  lessonId?: string | null;
  documentId?: string | null;
  courseId?: string | null;
  courseTitle?: string | null;
  chapterId?: string | null;
  chapterTitle?: string | null;
  chapterSortOrder?: number | null;
  lessonTitle?: string | null;
  lessonSortOrder?: number | null;
  category?: "final" | "chapter" | "lesson" | "custom" | string;
  totalItems: number;
  correctItems: number;
  status: string;
  startedAt?: string | null;
  completedAt?: string | null;
}

interface QuizPrompt {
  question?: string;
  rubricCriteria?: string[];
  explanation?: string;
}

interface QuizChoice {
  key: string;
  text: string;
}

interface AiRubricFeedback {
  summary?: string;
  strengths?: string[];
  missingConcepts?: string[];
  modelComparison?: string;
  masteryLevel?: string;
  modelUsed?: string;
  score?: number;
  feedback?: string;
  criteria?: string[];
}

interface QuizItem {
  id: string;
  quizId: string;
  itemType: "multiple_choice" | "cloze" | "short_answer" | "essay" | string;
  prompt: QuizPrompt | string;
  choices?: QuizChoice[] | null;
  clozeSolution?: string | null;
  explanation?: string | null;
  userAnswer?: { text?: string } | null;
  isCorrect?: boolean | null;
  aiRubricFeedback?: AiRubricFeedback | null;
  evaluatedAt?: string | null;
}

interface QuizAttemptHistory {
  id: string;
  score: number;
  maxScore: number;
  isPassed: boolean;
  durationSeconds?: number | null;
  detailJson?: string | null;
  createdAt: string;
}

interface QuizDetail {
  id: string;
  title: string;
  lessonId?: string | null;
  documentId?: string | null;
  courseId?: string | null;
  totalItems: number;
  correctItems: number;
  status: string;
  startedAt?: string | null;
  completedAt?: string | null;
  aiSummaryFeedback?: string | null;
  attemptHistory?: QuizAttemptHistory[];
  items: QuizItem[];
}

interface CourseChapterDto {
  id: string;
  title: string;
  sortOrder?: number;
}

interface ExamResultSummary {
  scorePercent: number;
  correctItems: number;
  totalItems: number;
  durationSeconds: number;
  status: string;
  submittedAt: string;
}

const QUIZ_ITEM_TYPE_LABELS: Record<string, string> = {
  multiple_choice: "Trắc nghiệm",
  cloze: "Điền từ còn thiếu",
  short_answer: "Trả lời ngắn",
  essay: "Tự luận",
};

type QuizCategoryFilter = "all" | "lesson" | "chapter" | "final" | "custom";
type QuizPracticeMode = "practice" | "exam";

function classifyQuizCategory(title: string): Exclude<QuizCategoryFilter, "all"> {
  const t = (title || "").trim();
  if (t.includes("[Đề Tự Tạo]")) return "custom";
  if (t.includes("[Đề Tổng Hợp")) return "final";
  if (t.includes("[Đề Chương")) return "chapter";
  return "lesson";
}

function resolveQuizCategory(q: { category?: string | null; title?: string }): Exclude<QuizCategoryFilter, "all"> {
  const cat = (q.category || "").toLowerCase();
  if (cat === "custom" || cat === "final" || cat === "chapter" || cat === "lesson") {
    return cat as Exclude<QuizCategoryFilter, "all">;
  }
  return classifyQuizCategory(q.title || "");
}

function formatTimerClock(seconds: number): string {
  const safe = Math.max(0, Math.floor(seconds));
  const m = Math.floor(safe / 60);
  const s = safe % 60;
  return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
}

function CourseQuizPageInner() {
  const params = useParams();
  const searchParams = useSearchParams();
  const courseId = params.id as string;
  const lessonIdParam = searchParams.get("lessonId");
  const quizIdParam = searchParams.get("quizId");
  const { activeUserHeaderId } = useWorkspaceStore();

  const [courseTitle, setCourseTitle] = useState("Khóa học");
  const [courseChapters, setCourseChapters] = useState<CourseChapterDto[]>([]);
  const [quizzes, setQuizzes] = useState<QuizListItem[]>([]);
  const [quizQuery, setQuizQuery] = useState("");
  const [quizCategoryFilter, setQuizCategoryFilter] = useState<QuizCategoryFilter>("all");
  const [openChapters, setOpenChapters] = useState<Record<string, boolean>>({});
  const [quizMode, setQuizMode] = useState<QuizPracticeMode>("practice");
  const [examTimeLeftSec, setExamTimeLeftSec] = useState(600);
  const [examElapsedSec, setExamElapsedSec] = useState(0);
  const [submittingAll, setSubmittingAll] = useState(false);
  const [resettingQuiz, setResettingQuiz] = useState(false);
  const [confirmResetOpen, setConfirmResetOpen] = useState(false);
  const [generateOpen, setGenerateOpen] = useState(false);
  const [genCount, setGenCount] = useState(10);
  const [genChapterId, setGenChapterId] = useState("");
  const [genItemType, setGenItemType] = useState("all");
  const [genTitle, setGenTitle] = useState("");
  const [generatingQuiz, setGeneratingQuiz] = useState(false);
  const [savingWrongIds, setSavingWrongIds] = useState<Record<string, boolean>>({});
  const [savedWrongIds, setSavedWrongIds] = useState<Record<string, boolean>>({});
  const [isSavingAllWrong, setIsSavingAllWrong] = useState(false);
  const [batchSaveSuccessMsg, setBatchSaveSuccessMsg] = useState<string | null>(null);
  const [mobileListOpen, setMobileListOpen] = useState(false);
  const [selectedQuizId, setSelectedQuizId] = useState<string | null>(null);
  const [quizDetail, setQuizDetail] = useState<QuizDetail | null>(null);
  const [draftAnswers, setDraftAnswers] = useState<Record<string, string>>({});
  const [submittingIds, setSubmittingIds] = useState<Record<string, boolean>>({});
  const [latestExamResult, setLatestExamResult] = useState<ExamResultSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const buildHeaders = useCallback(
    (extra?: Record<string, string>) => {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined"
          ? window.localStorage.getItem("swms_active_user_id")
          : null);
      return getClientRequestHeaders({
        ...(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : {}),
        ...(extra ?? {}),
      });
    },
    [activeUserHeaderId]
  );

  const loadQuizDetail = useCallback(
    async (quizId: string) => {
      try {
        const resp = await fetch(`/api/v1/practice-quizzes/${quizId}`, {
          headers: buildHeaders(),
        });
        if (!resp.ok) {
          setErrorMsg(`Không tải được chi tiết bài quiz (HTTP ${resp.status}).`);
          return;
        }
        const detail: QuizDetail = await resp.json();
        setQuizDetail(detail);
        const initialDrafts: Record<string, string> = {};
        for (const item of detail.items ?? []) {
          if (item.userAnswer?.text) {
            initialDrafts[item.id] = item.userAnswer.text;
          }
        }
        setDraftAnswers(initialDrafts);
        setExamElapsedSec(0);
        setExamTimeLeftSec(Math.max(300, (detail.items?.length ?? 5) * 90));
      } catch {
        setErrorMsg("Lỗi kết nối khi tải nội dung bài kiểm tra.");
      }
    },
    [buildHeaders]
  );

  const loadPageData = useCallback(async () => {
    if (!courseId) return;
    setIsLoading(true);
    setErrorMsg(null);
    try {
      const headers = buildHeaders();
      const [courseResp, quizzesResp] = await Promise.all([
        fetch(`/api/v1/courses/${courseId}`, { headers }).catch(() => null),
        fetch(`/api/v1/practice-quizzes?courseId=${encodeURIComponent(courseId)}`, {
          headers,
        }).catch(() => null),
      ]);

      let chaptersList: CourseChapterDto[] = [];
      if (courseResp && courseResp.ok) {
        const cData = await courseResp.json();
        if (cData?.title) setCourseTitle(cData.title);
        if (Array.isArray(cData?.chapters)) {
          chaptersList = cData.chapters.map((ch: Record<string, unknown>) => ({
            id: String(ch.id || ch.chapterId || ""),
            title: String(ch.title || "Chương"),
            sortOrder: ch.sortOrder != null ? Number(ch.sortOrder) : undefined,
          }));
        }
      }
      setCourseChapters(chaptersList);

      if (quizzesResp && quizzesResp.ok) {
        const qList: QuizListItem[] = await quizzesResp.json();
        const safeList = Array.isArray(qList) ? qList : [];
        setQuizzes(safeList);
        if (safeList.length > 0) {
          let targetQuiz: QuizListItem | undefined;
          if (quizIdParam) {
            targetQuiz = safeList.find((q) => q.id === quizIdParam);
          }
          if (!targetQuiz && lessonIdParam) {
            const matching = safeList.filter((q) => q.lessonId === lessonIdParam);
            targetQuiz =
              matching.find(
                (q) => !q.title.includes("[Đề Chương") && !q.title.includes("[Đề Tự Tạo]")
              ) || matching[0];
          }
          if (!targetQuiz) {
            targetQuiz = safeList[0];
          }
          setSelectedQuizId(targetQuiz.id);
          await loadQuizDetail(targetQuiz.id);
        }
      }
    } catch {
      setErrorMsg("Không thể tải dữ liệu bài kiểm tra.");
    } finally {
      setIsLoading(false);
    }
  }, [courseId, lessonIdParam, quizIdParam, buildHeaders, loadQuizDetail]);

  useEffect(() => {
    loadPageData();
  }, [loadPageData]);

  useEffect(() => {
    if (quizMode !== "exam" || !quizDetail || quizDetail.status === "completed") {
      return;
    }
    const timer = window.setInterval(() => {
      setExamElapsedSec((prev) => prev + 1);
      setExamTimeLeftSec((prev) => (prev > 0 ? prev - 1 : 0));
    }, 1000);
    return () => window.clearInterval(timer);
  }, [quizMode, quizDetail?.id, quizDetail?.status]);

  const handleSelectQuiz = async (qId: string) => {
    setMobileListOpen(false);
    if (qId === selectedQuizId) return;
    setSelectedQuizId(qId);
    setConfirmResetOpen(false);
    setLatestExamResult(null);
    setBatchSaveSuccessMsg(null);
    setErrorMsg(null);
    if (typeof window !== "undefined") {
      const url = new URL(window.location.href);
      url.searchParams.set("quizId", qId);
      window.history.replaceState(null, "", url.toString());
    }
    await loadQuizDetail(qId);
  };

  const startExamModeForQuiz = (itemCount: number) => {
    setQuizMode("exam");
    setExamElapsedSec(0);
    setExamTimeLeftSec(Math.max(300, itemCount * 90));
  };

  const handleResetQuiz = async () => {
    if (!quizDetail || resettingQuiz) return;
    setResettingQuiz(true);
    setErrorMsg(null);
    try {
      const resp = await fetch(`/api/v1/practice-quizzes/${quizDetail.id}/reset`, {
        method: "POST",
        headers: buildHeaders({ "Content-Type": "application/json" }),
      });
      if (!resp.ok) {
        const err = await resp.json().catch(() => null);
        setErrorMsg(err?.error ?? `Không làm lại được bài kiểm tra (HTTP ${resp.status}).`);
        return;
      }
      setDraftAnswers({});
      setSavedWrongIds({});
      setBatchSaveSuccessMsg(null);
      setLatestExamResult(null);
      setExamElapsedSec(0);
      setExamTimeLeftSec(Math.max(300, (quizDetail.items?.length ?? 5) * 90));
      setQuizzes((prev) =>
        prev.map((q) =>
          q.id === quizDetail.id ? { ...q, correctItems: 0, status: "in_progress" } : q
        )
      );
      await loadQuizDetail(quizDetail.id);
      if (typeof window !== "undefined") {
        window.dispatchEvent(new Event("swms:data-mutated"));
      }
    } catch {
      setErrorMsg("Lỗi kết nối khi làm lại bài kiểm tra.");
    } finally {
      setResettingQuiz(false);
      setConfirmResetOpen(false);
    }
  };

  const handleSubmitAllExam = async () => {
    if (!quizDetail || submittingAll) return;
    const answersPayload = (quizDetail.items ?? [])
      .map((item) => ({
        itemId: item.id,
        answer: (draftAnswers[item.id] ?? "").trim(),
      }))
      .filter((entry) => Boolean(entry.answer));

    if (answersPayload.length === 0) {
      setErrorMsg("Hãy chọn hoặc nhập ít nhất 1 câu trả lời trước khi nộp đề.");
      return;
    }

    setSubmittingAll(true);
    setErrorMsg(null);
    try {
      const resp = await fetch(`/api/v1/practice-quizzes/${quizDetail.id}/submit-all`, {
        method: "POST",
        headers: buildHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          answers: answersPayload,
          durationSeconds: examElapsedSec,
        }),
      });
      if (!resp.ok) {
        const err = await resp.json().catch(() => null);
        setErrorMsg(err?.error ?? `Không nộp được bài thi (HTTP ${resp.status}).`);
        return;
      }
      const resData = await resp.json();

      setLatestExamResult({
        scorePercent: Number(resData.scorePercent ?? 0),
        correctItems: Number(resData.correctItems ?? 0),
        totalItems: Number(resData.totalItems ?? quizDetail.items.length),
        durationSeconds: Number(resData.durationSeconds ?? examElapsedSec),
        status: String(resData.status ?? "completed"),
        submittedAt: String(resData.submittedAt ?? new Date().toISOString()),
      });

      setQuizzes((prev) =>
        prev.map((q) =>
          q.id === quizDetail.id
            ? {
                ...q,
                correctItems: Number(resData.correctItems ?? q.correctItems),
                status: String(resData.status ?? "completed"),
              }
            : q
        )
      );

      const returnedMap = new Map<string, {
        id: string;
        isCorrect?: boolean | null;
        expectedSolution?: string | null;
        explanation?: string | null;
        aiRubricFeedback?: AiRubricFeedback | null;
        evaluatedAt?: string | null;
      }>(
        Array.isArray(resData.items) ? resData.items.map((i: { id: string }) => [i.id, i]) : []
      );

      setQuizDetail((prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          correctItems: Number(resData.correctItems ?? prev.correctItems),
          status: String(resData.status ?? "completed"),
          attemptHistory: Array.isArray(resData.attemptHistory)
            ? resData.attemptHistory
            : prev.attemptHistory,
          items: prev.items.map((it) => {
            const ret = returnedMap.get(it.id);
            if (!ret) return it;
            return {
              ...it,
              isCorrect: typeof ret.isCorrect === "boolean" ? ret.isCorrect : it.isCorrect,
              clozeSolution: ret.expectedSolution ?? it.clozeSolution,
              explanation: ret.explanation ?? it.explanation,
              aiRubricFeedback: ret.aiRubricFeedback ?? it.aiRubricFeedback,
              evaluatedAt: ret.evaluatedAt ?? it.evaluatedAt ?? new Date().toISOString(),
            };
          }),
        };
      });

      await loadQuizDetail(quizDetail.id);

      if (typeof window !== "undefined") {
        window.dispatchEvent(new Event("swms:data-mutated"));
      }
    } catch {
      setErrorMsg("Lỗi kết nối khi nộp toàn bộ đề.");
    } finally {
      setSubmittingAll(false);
    }
  };

  const handleGenerateCustomQuiz = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!courseId || generatingQuiz) return;
    setGeneratingQuiz(true);
    setErrorMsg(null);
    try {
      const resp = await fetch(`/api/v1/courses/${courseId}/quizzes/generate`, {
        method: "POST",
        headers: buildHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          count: genCount,
          chapterId: genChapterId ? genChapterId : null,
          itemType: genItemType === "all" ? null : genItemType,
          title: genTitle.trim() || null,
        }),
      });
      if (!resp.ok) {
        const err = await resp.json().catch(() => null);
        setErrorMsg(err?.error ?? `Không tạo được đề luyện mới (HTTP ${resp.status}).`);
        return;
      }
      const created = await resp.json();
      const newQuizItem: QuizListItem = {
        id: String(created.id),
        title: String(created.title || "[Đề Tự Tạo]"),
        lessonId: created.lessonId ?? null,
        courseId,
        category: "custom",
        totalItems: Number(created.totalItems ?? created.itemCount ?? genCount),
        correctItems: 0,
        status: String(created.status ?? "in_progress"),
      };
      setQuizzes((prev) => [newQuizItem, ...prev]);
      setQuizCategoryFilter("all");
      setSelectedQuizId(newQuizItem.id);
      setGenerateOpen(false);
      setGenTitle("");
      setGenChapterId("");
      await loadQuizDetail(newQuizItem.id);
    } catch {
      setErrorMsg("Lỗi kết nối khi tạo đề luyện mới.");
    } finally {
      setGeneratingQuiz(false);
    }
  };

  const handleSaveWrongToFsrs = async (
    item: QuizItem,
    questionText: string
  ): Promise<boolean> => {
    if (!quizDetail || savingWrongIds[item.id] || savedWrongIds[item.id]) return false;
    setSavingWrongIds((prev) => ({ ...prev, [item.id]: true }));
    try {
      let backAnswer = (item.clozeSolution || "").trim();
      if (
        item.itemType === "multiple_choice" &&
        backAnswer &&
        Array.isArray(item.choices)
      ) {
        const matchedChoice = item.choices.find(
          (c) => c.key.toUpperCase() === backAnswer.toUpperCase()
        );
        if (matchedChoice) {
          backAnswer = `Đáp án ${matchedChoice.key}: ${matchedChoice.text}`;
        }
      }
      if (!backAnswer && item.explanation) {
        backAnswer = item.explanation;
      }
      if (!backAnswer && item.aiRubricFeedback?.summary) {
        backAnswer = item.aiRubricFeedback.summary;
      }
      if (!backAnswer) {
        backAnswer = "Xem lại phần giải thích chi tiết trong bài giảng nguồn.";
      }

      const resp = await fetch("/api/v1/flashcards", {
        method: "POST",
        headers: buildHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          lessonId: quizDetail.lessonId ?? null,
          cardType: "basic",
          sourceTitleSnapshot: quizDetail.title,
          sourceQuoteSnapshot: questionText,
          frontText: questionText,
          backText: backAnswer,
          tagNames: ["Quiz-Error"],
        }),
      });
      if (resp.ok) {
        setSavedWrongIds((prev) => ({ ...prev, [item.id]: true }));
        if (typeof window !== "undefined") {
          window.dispatchEvent(new Event("swms:data-mutated"));
        }
        return true;
      } else {
        setErrorMsg(`Không lưu được thẻ FSRS (HTTP ${resp.status}).`);
        return false;
      }
    } catch {
      setErrorMsg("Lỗi kết nối khi lưu thẻ FSRS.");
      return false;
    } finally {
      setSavingWrongIds((prev) => ({ ...prev, [item.id]: false }));
    }
  };

  const handleSaveAllWrongToFsrs = async () => {
    if (!quizDetail || isSavingAllWrong) return;
    const targets = quizDetail.items.filter(
      (it) => it.isCorrect === false && !savedWrongIds[it.id]
    );
    if (targets.length === 0) return;

    setIsSavingAllWrong(true);
    setBatchSaveSuccessMsg(null);
    setErrorMsg(null);
    let savedCount = 0;
    try {
      for (const item of targets) {
        const promptObj =
          typeof item.prompt === "string" ? { question: item.prompt } : item.prompt;
        const rawQuestionText = promptObj?.question || "Câu hỏi ôn tập";
        const cleanPrompt = formatCleanQuizPrompt(
          rawQuestionText,
          Boolean(item.evaluatedAt),
          item.clozeSolution
        );
        const ok = await handleSaveWrongToFsrs(item, cleanPrompt);
        if (ok) {
          savedCount++;
        }
      }
      if (savedCount > 0) {
        setBatchSaveSuccessMsg(`Đã lưu ${savedCount} câu sai vào bộ thẻ FSRS.`);
      }
    } finally {
      setIsSavingAllWrong(false);
    }
  };

  const handleSubmitItem = async (item: QuizItem) => {
    if (!quizDetail) return;
    const answerText = (draftAnswers[item.id] ?? "").trim();
    if (!answerText) return;

    setSubmittingIds((prev) => ({ ...prev, [item.id]: true }));
    setErrorMsg(null);

    try {
      const resp = await fetch(
        `/api/v1/practice-quizzes/${quizDetail.id}/items/${item.id}/submit`,
        {
          method: "POST",
          headers: buildHeaders({ "Content-Type": "application/json" }),
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
          setErrorMsg(
            errBody?.error || "Câu hỏi này đã được chấm trước đó, không thể nộp lại."
          );
          await loadQuizDetail(quizDetail.id);
        } else if (resp.status === 429) {
          setErrorMsg(
            errBody?.error ||
              "Bạn đã dùng hết 50 lượt AI trong ngày. Vui lòng quay lại vào ngày mai hoặc nâng cấp gói."
          );
        } else if (resp.status === 400) {
          setErrorMsg(
            errBody?.error || "Nội dung câu trả lời không hợp lệ hoặc đang để trống."
          );
        } else {
          setErrorMsg(
            errBody?.error || `Gửi câu trả lời thất bại (HTTP ${resp.status}).`
          );
        }
        return;
      }

      const result = await resp.json();

      if (result.jobId && result.jobStatus && result.jobStatus !== "completed") {
        for (let i = 0; i < 20; i++) {
          await new Promise((r) => setTimeout(r, 700));
          const jobResp = await fetch(`/api/v1/jobs/${result.jobId}`, {
            headers: buildHeaders(),
          });
          if (jobResp.ok) {
            const jobData = await jobResp.json();
            if (jobData.status === "completed" || jobData.status === "failed") {
              break;
            }
          }
        }
      }

      setQuizDetail((prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          correctItems:
            typeof result.correctItems === "number" ? result.correctItems : prev.correctItems,
          status: result.quizStatus ? String(result.quizStatus) : prev.status,
          items: prev.items.map((it) =>
            it.id === item.id
              ? {
                  ...it,
                  isCorrect:
                    typeof result.isCorrect === "boolean" ? result.isCorrect : it.isCorrect,
                  clozeSolution: result.expectedSolution ?? it.clozeSolution,
                  explanation: result.explanation ?? it.explanation,
                  evaluatedAt: result.evaluatedAt ?? new Date().toISOString(),
                }
              : it
          ),
        };
      });

      await loadQuizDetail(quizDetail.id);
      if (typeof window !== "undefined") {
        window.dispatchEvent(new Event("swms:data-mutated"));
      }
    } catch {
      setErrorMsg("Lỗi kết nối máy chủ khi nộp câu trả lời.");
    } finally {
      setSubmittingIds((prev) => ({ ...prev, [item.id]: false }));
    }
  };

  const filterCounts = useMemo(() => {
    let lesson = 0;
    let chapter = 0;
    let final = 0;
    let custom = 0;
    for (const q of quizzes) {
      const c = resolveQuizCategory(q);
      if (c === "lesson") lesson++;
      else if (c === "chapter") chapter++;
      else if (c === "final") final++;
      else if (c === "custom") custom++;
    }
    return {
      all: quizzes.length,
      lesson,
      chapter,
      final,
      custom,
    };
  }, [quizzes]);

  const filteredQuizzes = useMemo(() => {
    const query = quizQuery.trim().toLowerCase();
    return quizzes.filter((q) => {
      const matchesQuery =
        !query ||
        q.title.toLowerCase().includes(query) ||
        (q.chapterTitle && q.chapterTitle.toLowerCase().includes(query)) ||
        (q.lessonTitle && q.lessonTitle.toLowerCase().includes(query));
      if (!matchesQuery) return false;
      if (quizCategoryFilter === "all") return true;
      return resolveQuizCategory(q) === quizCategoryFilter;
    });
  }, [quizzes, quizQuery, quizCategoryFilter]);

  interface ChapterGroup {
    chapterKey: string;
    chapterTitle: string;
    chapterSortOrder: number;
    quizzes: QuizListItem[];
  }

  const { customQuizzes, finalQuizzes, chapterGroups } = useMemo(() => {
    const customList: QuizListItem[] = [];
    const finalList: QuizListItem[] = [];
    const chMap = new Map<string, ChapterGroup>();

    for (const q of filteredQuizzes) {
      const cat = resolveQuizCategory(q);
      if (cat === "custom") {
        customList.push(q);
        continue;
      }
      if (cat === "final") {
        finalList.push(q);
        continue;
      }
      const key = q.chapterId || q.chapterTitle || "default-chapter";
      const title = q.chapterTitle || "Đề theo bài học";
      const sortOrder = q.chapterSortOrder ?? 9999;
      let group = chMap.get(key);
      if (!group) {
        group = {
          chapterKey: key,
          chapterTitle: title,
          chapterSortOrder: sortOrder,
          quizzes: [],
        };
        chMap.set(key, group);
      }
      group.quizzes.push(q);
    }

    const groups = Array.from(chMap.values()).sort(
      (a, b) => a.chapterSortOrder - b.chapterSortOrder
    );

    return {
      customQuizzes: customList,
      finalQuizzes: finalList,
      chapterGroups: groups,
    };
  }, [filteredQuizzes]);

  useEffect(() => {
    if (quizQuery.trim().length > 0 || quizCategoryFilter !== "all") {
      const next: Record<string, boolean> = {};
      for (const group of chapterGroups) {
        next[group.chapterKey] = true;
      }
      setOpenChapters(next);
    } else {
      setOpenChapters((prev) => {
        const next = { ...prev };
        if (chapterGroups.length > 0 && next[chapterGroups[0].chapterKey] === undefined) {
          next[chapterGroups[0].chapterKey] = true;
        }
        if (selectedQuizId) {
          const found = chapterGroups.find((g) =>
            g.quizzes.some((q) => q.id === selectedQuizId)
          );
          if (found) {
            next[found.chapterKey] = true;
          }
        }
        return next;
      });
    }
  }, [chapterGroups, selectedQuizId, quizQuery, quizCategoryFilter]);

  const toggleChapter = (key: string) => {
    setOpenChapters((prev) => ({
      ...prev,
      [key]: !prev[key],
    }));
  };

  if (isLoading) {
    return (
      <div className="max-w-[1360px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-10 min-h-[80vh] space-y-6">
        <div className="h-6 w-32 rounded bg-[var(--surface-elevated)] swms-skeleton" />
        <div className="h-10 w-72 rounded-lg bg-[var(--surface-elevated)] swms-skeleton" />
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 pt-2">
          <div className="lg:col-span-4 h-72 rounded-xl bg-[var(--surface-elevated)] swms-skeleton" />
          <div className="lg:col-span-8 space-y-4">
            {[0, 1, 2].map((i) => (
              <div key={i} className="h-40 rounded-xl bg-[var(--surface-elevated)] swms-skeleton" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  const answeredCount = quizDetail
    ? quizDetail.items.filter((it) => Boolean(it.evaluatedAt)).length
    : 0;
  const accuracyPct =
    quizDetail && answeredCount > 0
      ? Math.round((quizDetail.correctItems * 100) / answeredCount)
      : 0;
  const unsavedWrongItems = quizDetail
    ? quizDetail.items.filter((it) => it.isCorrect === false && !savedWrongIds[it.id])
    : [];

  const displaySummary =
    latestExamResult ||
    (quizDetail?.status === "completed" &&
    quizDetail.attemptHistory &&
    quizDetail.attemptHistory.length > 0
      ? {
          scorePercent: quizDetail.attemptHistory[0].score,
          correctItems: quizDetail.correctItems,
          totalItems: quizDetail.totalItems,
          durationSeconds: quizDetail.attemptHistory[0].durationSeconds ?? 0,
          status: quizDetail.status,
          submittedAt: quizDetail.attemptHistory[0].createdAt,
        }
      : null);

  const renderQuizItemButton = (q: QuizListItem) => {
    const active = selectedQuizId === q.id;
    const isCompleted = q.status === "completed";
    return (
      <button
        key={q.id}
        type="button"
        onClick={() => void handleSelectQuiz(q.id)}
        className={`w-full text-left px-3 py-2 rounded-md text-xs transition cursor-pointer flex items-center justify-between gap-2 ${
          active
            ? "bg-accent dark:bg-[#818CF8] text-accent-contrast dark:text-[#0F0F12] font-medium shadow-xs"
            : "bg-bg-primary text-text-secondary border border-border/60 hover:text-text-primary hover:border-border"
        }`}
      >
        <div className="min-w-0 flex-1">
          <p className="line-clamp-2 leading-snug">{q.title}</p>
          {q.lessonTitle && (
            <p
              className={`text-[10px] truncate mt-0.5 ${
                active
                  ? "text-accent-contrast/80 dark:text-[#0F0F12]/80"
                  : "text-text-tertiary"
              }`}
            >
              {q.lessonTitle}
            </p>
          )}
        </div>
        <span
          className={`text-[10px] font-semibold px-1.5 py-0.5 rounded shrink-0 ${
            active
              ? "bg-white/20 dark:bg-black/15 text-accent-contrast dark:text-[#0F0F12]"
              : isCompleted
              ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-500/20"
              : "bg-bg-secondary text-text-tertiary"
          }`}
        >
          {q.correctItems}/{q.totalItems}
        </span>
      </button>
    );
  };

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-10 min-h-[80vh] overflow-x-clip">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-3 min-w-0">
          <Link
            href={`/app/courses/${courseId}`}
            className="inline-flex items-center gap-1.5 text-sm text-text-secondary hover:text-text-primary transition"
          >
            <ArrowLeft className="w-4 h-4" />
            <span>Quay lại khóa học</span>
          </Link>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            data-testid="open-generate-quiz-btn"
            onClick={() => setGenerateOpen(true)}
            className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md bg-accent dark:bg-[#818CF8] text-accent-contrast dark:text-[#0F0F12] text-xs font-medium hover:bg-accent-hover transition cursor-pointer"
          >
            <Plus className="w-3.5 h-3.5" />
            <span>Tạo đề luyện mới</span>
          </button>
          <Link
            href={`/app/courses/${courseId}/learn`}
            className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition"
          >
            <BookOpen className="w-3.5 h-3.5" />
            <span>Mở bài giảng</span>
          </Link>
          <Link
            href={`/app/review?courseId=${courseId}`}
            className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition"
          >
            <Brain className="w-3.5 h-3.5" />
            <span>Ôn thẻ ghi nhớ</span>
          </Link>
        </div>
      </div>

      <div className="mb-6">
        <p className="text-xs font-medium text-accent mb-1">{courseTitle}</p>
        <h1 className="text-2xl font-bold text-text-primary flex items-center gap-2">
          <ClipboardCheck className="w-6 h-6 text-accent" />
          <span>Kiểm tra &amp; Luyện tập</span>
        </h1>
      </div>

      {errorMsg && (
        <div
          role="alert"
          className="mb-6 p-3.5 rounded-lg border border-rose-300 dark:border-rose-800 bg-rose-50 dark:bg-rose-950/30 text-sm text-rose-700 dark:text-rose-300 flex items-center gap-2"
        >
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{errorMsg}</span>
        </div>
      )}

      {quizzes.length === 0 || !quizDetail ? (
        <div className="rounded-lg border border-border bg-bg-secondary p-10 text-center">
          <ClipboardCheck className="w-10 h-10 text-text-tertiary mx-auto mb-3" />
          <h2 className="text-base font-semibold text-text-primary mb-1">
            Khóa học này chưa có bài kiểm tra nào
          </h2>
          <p className="text-sm text-text-secondary mb-6">
            Bạn có thể học bài giảng hoặc ôn tập bằng thẻ ghi nhớ trong thời gian chờ.
          </p>
          <div className="flex items-center justify-center gap-3">
            <Link
              href={`/app/courses/${courseId}/learn`}
              className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-accent dark:bg-[#818CF8] text-accent-contrast dark:text-[#0F0F12] text-sm font-medium hover:bg-accent-hover transition"
            >
              <BookOpen className="w-4 h-4" />
              <span>Học bài giảng</span>
            </Link>
            <Link
              href="/app/review"
              className="inline-flex items-center gap-2 h-9 px-4 rounded-md border border-border text-sm font-medium text-text-primary hover:bg-bg-tertiary transition"
            >
              <Brain className="w-4 h-4" />
              <span>Ôn thẻ ghi nhớ</span>
            </Link>
          </div>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
          {/* 2-Column Sidebar: Mobile Collapsible Quiz Picker + Desktop Question Navigator + Quiz List */}
          <aside
            data-testid="quiz-sidebar-selector"
            className="lg:col-span-4 min-w-0 flex flex-col gap-3 lg:gap-4 lg:sticky lg:top-4"
          >
            {/* Question Map & Score Card (Desktop) */}
            <div className="hidden lg:block rounded-xl border border-border bg-bg-secondary p-4 space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold uppercase tracking-wider text-text-tertiary">
                  Bảng câu hỏi &amp; Tiến độ
                </span>
                <span className="text-xs font-bold text-accent">
                  {answeredCount}/{quizDetail.items.length} đã chấm
                </span>
              </div>

              <div className="grid grid-cols-3 gap-2 text-center">
                <div className="rounded-lg bg-bg-primary border border-border p-2">
                  <p className="text-[10px] text-text-tertiary">Số câu</p>
                  <p className="text-sm font-bold text-text-primary">{quizDetail.items.length}</p>
                </div>
                <div className="rounded-lg bg-bg-primary border border-border p-2">
                  <p className="text-[10px] text-text-tertiary">Đúng</p>
                  <p className="text-sm font-bold text-emerald-600 dark:text-emerald-400">
                    {quizDetail.correctItems}
                  </p>
                </div>
                <div className="rounded-lg bg-bg-primary border border-border p-2">
                  <p className="text-[10px] text-text-tertiary">Tỷ lệ</p>
                  <p className="text-sm font-bold text-accent">{accuracyPct}%</p>
                </div>
              </div>

              <div className="grid grid-cols-5 gap-1.5 pt-1">
                {quizDetail.items.map((qItem, qIdx) => {
                  const done = Boolean(qItem.evaluatedAt);
                  const ok = qItem.isCorrect === true;
                  const wrong = qItem.isCorrect === false;
                  return (
                    <button
                      key={qItem.id}
                      type="button"
                      onClick={() =>
                        document
                          .getElementById(`quiz-item-${qIdx + 1}`)
                          ?.scrollIntoView({ behavior: "smooth", block: "center" })
                      }
                      className={`h-8 rounded-md text-xs font-semibold border transition cursor-pointer ${
                        ok
                          ? "bg-emerald-500/15 border-emerald-500/40 text-emerald-700 dark:text-emerald-300"
                          : wrong
                            ? "bg-rose-500/15 border-rose-500/40 text-rose-700 dark:text-rose-300"
                            : done
                              ? "bg-accent-subtle border-accent/40 text-accent"
                              : "bg-bg-primary border-border text-text-secondary hover:border-accent"
                      }`}
                    >
                      {qIdx + 1}
                    </button>
                  );
                })}
              </div>
            </div>

            {quizzes.length > 1 && (
              <div className="lg:hidden">
                <label
                  htmlFor="quiz-mobile-selector"
                  className="block text-xs font-medium text-text-secondary mb-1.5"
                >
                  Chọn nhanh bài kiểm tra ({quizzes.length} bài)
                </label>
                <select
                  id="quiz-mobile-selector"
                  data-testid="quiz-mobile-selector"
                  value={selectedQuizId ?? ""}
                  onChange={(e) => void handleSelectQuiz(e.target.value)}
                  className="w-full h-9 rounded-md border border-border bg-bg-primary px-3 text-sm text-text-primary focus:outline-none focus:ring-1 focus:ring-accent"
                >
                  {quizzes.map((q) => (
                    <option key={q.id} value={q.id}>
                      {q.title} ({q.correctItems}/{q.totalItems} đúng)
                    </option>
                  ))}
                </select>
              </div>
            )}

            {quizzes.length > 0 && (
              <div className="flex flex-col gap-2">
                <button
                  type="button"
                  data-testid="mobile-quiz-list-toggle"
                  aria-expanded={mobileListOpen}
                  onClick={() => setMobileListOpen((prev) => !prev)}
                  className="lg:hidden w-full flex items-center justify-between gap-2 px-3.5 py-2.5 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-semibold text-text-primary cursor-pointer"
                >
                  <span className="truncate">Danh sách đề ({filteredQuizzes.length})</span>
                  {mobileListOpen ? (
                    <ChevronDown className="w-4 h-4 text-text-tertiary shrink-0" />
                  ) : (
                    <ChevronRight className="w-4 h-4 text-text-tertiary shrink-0" />
                  )}
                </button>

                <div
                  className={`${
                    mobileListOpen ? "block" : "hidden"
                  } lg:block rounded-xl border border-border bg-bg-secondary p-3.5`}
                >
                <div className="flex items-center justify-between gap-2 mb-2.5">
                  <span className="text-xs font-semibold text-text-primary">
                    Danh sách bài kiểm tra
                  </span>
                  <span className="text-[11px] font-medium px-2 py-0.5 rounded bg-bg-primary border border-border text-text-tertiary">
                    {quizzes.length} bài
                  </span>
                </div>

                <div className="relative mb-2.5">
                  <Search className="w-3.5 h-3.5 text-text-tertiary absolute left-2.5 top-1/2 -translate-y-1/2" />
                  <input
                    type="text"
                    data-testid="quiz-search-input"
                    value={quizQuery}
                    onChange={(e) => setQuizQuery(e.target.value)}
                    placeholder="Lọc bài kiểm tra..."
                    className="w-full h-8 pl-8 pr-2.5 rounded-md border border-border bg-bg-primary text-xs text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-accent"
                  />
                </div>

                {/* 5 Category Filter Chips */}
                <div className="flex flex-wrap gap-1 mb-2.5">
                  {(
                    [
                      { id: "all", label: "Tất cả" },
                      { id: "lesson", label: "Đề Bài học" },
                      { id: "chapter", label: "Đề Chương" },
                      { id: "final", label: "Cuối khóa" },
                      { id: "custom", label: "Tự tạo" },
                    ] as const
                  ).map((cat) => {
                    const count = filterCounts[cat.id] ?? 0;
                    const active = quizCategoryFilter === cat.id;
                    return (
                      <button
                        key={cat.id}
                        type="button"
                        data-testid={`quiz-filter-${cat.id}`}
                        onClick={() => setQuizCategoryFilter(cat.id)}
                        className={`px-2 py-1 rounded text-[11px] font-medium transition cursor-pointer flex items-center gap-1 ${
                          active
                            ? "bg-accent dark:bg-[#818CF8] text-accent-contrast dark:text-[#0F0F12]"
                            : "bg-bg-primary text-text-secondary border border-border/60 hover:text-text-primary"
                        }`}
                      >
                        <span>{cat.label}</span>
                        <span
                          className={`text-[10px] px-1 rounded-full ${
                            active
                              ? "bg-white/20 dark:bg-black/15 text-accent-contrast dark:text-[#0F0F12]"
                              : "bg-bg-secondary text-text-tertiary"
                          }`}
                        >
                          {count}
                        </span>
                      </button>
                    );
                  })}
                </div>

                {/* Grouped Quiz List with Accordion */}
                <div className="max-h-[460px] overflow-y-auto swms-thin-scrollbar space-y-2 pr-0.5">
                  {filteredQuizzes.length === 0 ? (
                    <p className="text-xs text-text-tertiary py-4 text-center">
                      Không có đề phù hợp bộ lọc.
                    </p>
                  ) : (
                    <>
                      {/* Custom Quizzes Section */}
                      {customQuizzes.length > 0 && (
                        <div className="space-y-1">
                          <div className="flex items-center gap-1.5 px-1 pt-1 text-[11px] font-semibold text-accent uppercase tracking-wider">
                            <Sparkles className="w-3 h-3" />
                            <span>Đề Tự Tạo ({customQuizzes.length})</span>
                          </div>
                          <div className="space-y-1">
                            {customQuizzes.map(renderQuizItemButton)}
                          </div>
                        </div>
                      )}

                      {/* Chapter Groups Accordion */}
                      {chapterGroups.length > 0 && (
                        <div className="space-y-1.5">
                          {chapterGroups.map((group) => {
                            const isOpen = Boolean(openChapters[group.chapterKey]);
                            const containsActive = group.quizzes.some(
                              (q) => q.id === selectedQuizId
                            );
                            return (
                              <div
                                key={group.chapterKey}
                                className="rounded-lg border border-border/80 bg-bg-primary/50 overflow-hidden"
                              >
                                <button
                                  type="button"
                                  onClick={() => toggleChapter(group.chapterKey)}
                                  className={`w-full px-2.5 py-2 flex items-center justify-between gap-2 text-left transition cursor-pointer hover:bg-bg-tertiary ${
                                    containsActive ? "bg-accent/5 font-semibold" : ""
                                  }`}
                                >
                                  <div className="flex items-center gap-1.5 min-w-0">
                                    {isOpen ? (
                                      <ChevronDown className="w-3.5 h-3.5 text-text-tertiary shrink-0" />
                                    ) : (
                                      <ChevronRight className="w-3.5 h-3.5 text-text-tertiary shrink-0" />
                                    )}
                                    <span className="text-xs text-text-primary truncate">
                                      {group.chapterTitle}
                                    </span>
                                  </div>
                                  <span className="text-[10px] font-medium px-1.5 py-0.5 rounded bg-bg-secondary text-text-tertiary shrink-0">
                                    {group.quizzes.length} đề
                                  </span>
                                </button>

                                {isOpen && (
                                  <div className="p-1.5 pt-0 space-y-1 border-t border-border/40">
                                    {group.quizzes.map(renderQuizItemButton)}
                                  </div>
                                )}
                              </div>
                            );
                          })}
                        </div>
                      )}

                      {/* Final Quizzes Section */}
                      {finalQuizzes.length > 0 && (
                        <div className="space-y-1">
                          <div className="flex items-center gap-1.5 px-1 pt-1 text-[11px] font-semibold text-amber-600 dark:text-amber-400 uppercase tracking-wider">
                            <Trophy className="w-3 h-3" />
                            <span>Thi Tổng Hợp Cuối Khóa ({finalQuizzes.length})</span>
                          </div>
                          <div className="space-y-1">
                            {finalQuizzes.map(renderQuizItemButton)}
                          </div>
                        </div>
                      )}
                    </>
                  )}
                </div>
              </div>
              </div>
            )}
          </aside>

          <div className="lg:col-span-8 min-w-0 space-y-6">
            {/* Quiz Summary Banner & Mode Controls */}
            <div className="rounded-lg border border-border bg-bg-secondary p-5 space-y-4">
              <div className="flex flex-wrap items-center justify-between gap-4">
                <div>
                  <h2 className="text-base font-semibold text-text-primary">
                    {quizDetail.title}
                  </h2>
                  <p className="text-xs text-text-tertiary mt-1">
                    {quizDetail.items.length} câu hỏi · Trạng thái:{" "}
                    <span className="font-medium text-text-secondary">
                      {quizDetail.status === "completed" ? "Đã hoàn thành" : "Đang làm"}
                    </span>
                  </p>
                </div>

                <div className="flex flex-wrap items-center gap-2">
                  <div className="px-3 py-1.5 rounded-md bg-bg-primary border border-border text-sm font-semibold text-text-primary flex items-center gap-1.5">
                    <Trophy className="w-4 h-4 text-amber-500" />
                    <span>
                      {quizDetail.correctItems}/{quizDetail.totalItems} đúng
                    </span>
                  </div>

                  {/* Reset Quiz Button with Safe Inline Confirmation (NO window.confirm) */}
                  <div className="inline-flex items-center gap-1.5">
                    <button
                      type="button"
                      data-testid="reset-quiz-btn"
                      disabled={resettingQuiz}
                      onClick={() => setConfirmResetOpen((prev) => !prev)}
                      className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border bg-bg-primary text-xs font-medium text-text-secondary hover:text-text-primary hover:bg-bg-tertiary disabled:opacity-50 transition cursor-pointer"
                    >
                      {resettingQuiz ? (
                        <Loader2 className="w-3.5 h-3.5 animate-spin" />
                      ) : (
                        <RotateCcw className="w-3.5 h-3.5" />
                      )}
                      <span>Làm lại đề</span>
                    </button>

                    {confirmResetOpen && (
                      <div className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md bg-rose-500/10 border border-rose-500/30 text-xs">
                        <span className="text-[11px] text-text-secondary font-medium">Đặt lại đề?</span>
                        <button
                          type="button"
                          data-testid="confirm-reset-quiz-btn"
                          disabled={resettingQuiz}
                          onClick={() => void handleResetQuiz()}
                          className="px-2 py-0.5 rounded bg-rose-600 text-white text-[11px] font-semibold hover:bg-rose-700 disabled:opacity-50 transition cursor-pointer"
                        >
                          {resettingQuiz ? "Đang đặt lại..." : "Xác nhận làm lại"}
                        </button>
                        <button
                          type="button"
                          onClick={() => setConfirmResetOpen(false)}
                          className="px-2 py-0.5 rounded border border-border bg-bg-primary text-[11px] text-text-secondary hover:text-text-primary transition cursor-pointer"
                        >
                          Hủy
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              </div>

              <div className="flex flex-wrap items-center justify-between gap-3 pt-3 border-t border-border/60">
                <div className="inline-flex items-center rounded-lg border border-border bg-bg-primary p-0.5">
                  <button
                    type="button"
                    data-testid="mode-practice-btn"
                    onClick={() => setQuizMode("practice")}
                    className={`px-3 py-1.5 rounded-md text-xs font-medium transition cursor-pointer ${
                      quizMode === "practice"
                        ? "bg-accent dark:bg-[#818CF8] text-accent-contrast dark:text-[#0F0F12] shadow-xs"
                        : "text-text-secondary hover:text-text-primary"
                    }`}
                  >
                    Luyện từng câu
                  </button>
                  <button
                    type="button"
                    data-testid="mode-exam-btn"
                    onClick={() => startExamModeForQuiz(quizDetail.items.length)}
                    className={`px-3 py-1.5 rounded-md text-xs font-medium transition cursor-pointer ${
                      quizMode === "exam"
                        ? "bg-accent dark:bg-[#818CF8] text-accent-contrast dark:text-[#0F0F12] shadow-xs"
                        : "text-text-secondary hover:text-text-primary"
                    }`}
                  >
                    Thi thử cả đề
                  </button>
                </div>

                {quizMode === "exam" && (() => {
                  const isExamActive = quizDetail.status !== "completed";
                  const timerTone =
                    isExamActive && examTimeLeftSec <= 30
                      ? "border-rose-400 dark:border-rose-700 bg-rose-50 dark:bg-rose-950/40 text-rose-700 dark:text-rose-300"
                      : isExamActive && examTimeLeftSec <= 120
                      ? "border-amber-400 dark:border-amber-700 bg-amber-50 dark:bg-amber-950/30 text-amber-700 dark:text-amber-300"
                      : "border-border bg-bg-primary text-text-primary";
                  return (
                    <div className="flex flex-wrap items-center gap-2.5">
                      <div
                        className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md border text-xs font-mono font-semibold transition-colors ${timerTone}`}
                      >
                        <Timer className="w-3.5 h-3.5" />
                        <span>{formatTimerClock(examTimeLeftSec)}</span>
                      </div>
                      <button
                        type="button"
                        data-testid="submit-all-exam-btn"
                        disabled={submittingAll || quizDetail.status === "completed"}
                        onClick={() => void handleSubmitAllExam()}
                        className="inline-flex items-center gap-1.5 h-8 px-3.5 rounded-md bg-emerald-600 text-white text-xs font-semibold hover:bg-emerald-700 disabled:opacity-50 transition cursor-pointer"
                      >
                        {submittingAll ? (
                          <Loader2 className="w-3.5 h-3.5 animate-spin" />
                        ) : (
                          <Send className="w-3.5 h-3.5" />
                        )}
                        <span>
                          {quizDetail.status === "completed"
                            ? "Đã nộp cả đề"
                            : "Nộp toàn bộ đề"}
                        </span>
                      </button>
                    </div>
                  );
                })()}
              </div>

              {(unsavedWrongItems.length > 0 || batchSaveSuccessMsg) && (
                <div className="flex flex-wrap items-center justify-between gap-2 pt-3 border-t border-border/60">
                  {unsavedWrongItems.length > 0 ? (
                    <button
                      type="button"
                      data-testid="save-all-wrong-fsrs-btn"
                      disabled={isSavingAllWrong}
                      onClick={() => void handleSaveAllWrongToFsrs()}
                      className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-amber-400/60 dark:border-amber-700 bg-amber-50 dark:bg-amber-950/40 text-xs font-semibold text-amber-800 dark:text-amber-200 hover:bg-amber-100 dark:hover:bg-amber-900/50 disabled:opacity-50 transition cursor-pointer"
                    >
                      {isSavingAllWrong ? (
                        <Loader2 className="w-3.5 h-3.5 animate-spin" />
                      ) : (
                        <Brain className="w-3.5 h-3.5" />
                      )}
                      <span>
                        {isSavingAllWrong
                          ? "Đang lưu vào FSRS..."
                          : `Lưu tất cả câu sai vào FSRS (${unsavedWrongItems.length})`}
                      </span>
                    </button>
                  ) : (
                    <div />
                  )}

                  {batchSaveSuccessMsg && (
                    <span
                      role="status"
                      className="inline-flex items-center gap-1.5 text-xs font-medium text-emerald-700 dark:text-emerald-300"
                    >
                      <CheckCircle2 className="w-3.5 h-3.5 shrink-0" />
                      <span>{batchSaveSuccessMsg}</span>
                    </span>
                  )}
                </div>
              )}
            </div>

            {/* Exam Result Summary Banner */}
            {displaySummary && (
              <div
                data-testid="exam-result-summary"
                className="rounded-xl border border-emerald-500/40 bg-emerald-500/10 p-4 sm:p-5 space-y-3"
              >
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div className="flex items-center gap-2">
                    <CheckCircle2 className="w-5 h-5 text-emerald-600 dark:text-emerald-400 shrink-0" />
                    <h3 className="text-sm sm:text-base font-bold text-text-primary">
                      Kết quả bài thi vừa nộp
                    </h3>
                  </div>
                  <span
                    className={`px-2.5 py-1 rounded-full text-xs font-bold ${
                      displaySummary.scorePercent >= 70
                        ? "bg-emerald-600 text-white"
                        : "bg-amber-600 text-white"
                    }`}
                  >
                    {displaySummary.scorePercent >= 70 ? "ĐẠT YÊU CẦU" : "CẦN ÔN LẠI"}
                  </span>
                </div>

                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 text-center">
                  <div className="rounded-lg bg-bg-primary/80 border border-border p-2.5">
                    <p className="text-[11px] text-text-tertiary">Điểm số</p>
                    <p className="text-lg font-extrabold text-accent">
                      {displaySummary.scorePercent}%
                    </p>
                  </div>
                  <div className="rounded-lg bg-bg-primary/80 border border-border p-2.5">
                    <p className="text-[11px] text-text-tertiary">Số câu đúng</p>
                    <p className="text-lg font-extrabold text-emerald-600 dark:text-emerald-400">
                      {displaySummary.correctItems}/{displaySummary.totalItems}
                    </p>
                  </div>
                  <div className="rounded-lg bg-bg-primary/80 border border-border p-2.5">
                    <p className="text-[11px] text-text-tertiary">Thời gian làm bài</p>
                    <p className="text-lg font-extrabold text-text-primary">
                      {formatTimerClock(displaySummary.durationSeconds)}
                    </p>
                  </div>
                  <div className="rounded-lg bg-bg-primary/80 border border-border p-2.5">
                    <p className="text-[11px] text-text-tertiary">Thời điểm nộp</p>
                    <p className="text-xs font-semibold text-text-secondary mt-1">
                      {new Date(displaySummary.submittedAt).toLocaleTimeString("vi-VN", {
                        hour: "2-digit",
                        minute: "2-digit",
                      })}
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Quiz Attempt History */}
            {quizDetail.attemptHistory && quizDetail.attemptHistory.length > 0 && (
              <div
                data-testid="quiz-attempt-history"
                className="rounded-lg border border-border bg-bg-secondary p-4 space-y-3"
              >
                <div className="flex items-center justify-between gap-2">
                  <h3 className="text-xs font-semibold uppercase tracking-wider text-text-tertiary flex items-center gap-1.5">
                    <Clock className="w-3.5 h-3.5 text-accent" />
                    <span>Lịch sử các lần nộp đề ({quizDetail.attemptHistory.length})</span>
                  </h3>
                </div>

                <div className="divide-y divide-border/60">
                  {quizDetail.attemptHistory.map((att, attIdx) => {
                    const attemptNumber = quizDetail.attemptHistory!.length - attIdx;
                    const formattedDate = new Date(att.createdAt).toLocaleString("vi-VN", {
                      year: "numeric",
                      month: "2-digit",
                      day: "2-digit",
                      hour: "2-digit",
                      minute: "2-digit",
                    });

                    return (
                      <div
                        key={att.id || attIdx}
                        className="py-2.5 first:pt-1 last:pb-1 flex flex-wrap items-center justify-between gap-3 text-xs"
                      >
                        <div className="flex items-center gap-2.5 min-w-0">
                          <span className="font-semibold text-text-primary">
                            Lần {attemptNumber}
                          </span>
                          <span
                            className={`px-2 py-0.5 rounded text-[11px] font-semibold ${
                              att.isPassed
                                ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border border-emerald-500/30"
                                : "bg-amber-500/15 text-amber-700 dark:text-amber-300 border border-amber-500/30"
                            }`}
                          >
                            {att.isPassed ? "Đạt" : "Cần ôn lại"}
                          </span>
                          <span className="text-text-tertiary text-[11px]">{formattedDate}</span>
                        </div>

                        <div className="flex items-center gap-3 text-text-secondary text-xs">
                          <span className="font-bold text-accent">{att.score}%</span>
                          {att.durationSeconds != null && att.durationSeconds > 0 && (
                            <span className="text-text-tertiary font-mono text-[11px]">
                              {formatTimerClock(att.durationSeconds)}
                            </span>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            )}

            {/* Question Navigator Palette */}
            {quizDetail.items.length > 0 && (
              <div
                data-testid="quiz-question-navigator"
                className="rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] p-3 space-y-2.5"
              >
                <div className="flex items-center justify-between gap-2">
                  <span className="text-xs font-semibold text-text-primary">
                    Điều hướng câu hỏi
                  </span>
                  <span className="text-[11px] font-medium text-text-tertiary">
                    {answeredCount}/{quizDetail.items.length} đã chấm
                  </span>
                </div>
                <div className="flex flex-wrap gap-1.5">
                  {quizDetail.items.map((navItem, i) => {
                    const navEvaluated =
                      Boolean(navItem.evaluatedAt) ||
                      navItem.isCorrect === true ||
                      navItem.isCorrect === false;
                    const navHasDraft = Boolean(draftAnswers[navItem.id]?.trim());
                    const pillClass = navEvaluated
                      ? navItem.isCorrect === true
                        ? "bg-emerald-500/15 border-emerald-500/40 text-emerald-700 dark:text-emerald-300"
                        : navItem.isCorrect === false
                        ? "bg-rose-500/15 border-rose-500/40 text-rose-700 dark:text-rose-300"
                        : "bg-indigo-500/15 border-indigo-500/40 text-indigo-700 dark:text-indigo-300"
                      : navHasDraft
                      ? "border-[#5E51E8]/50 bg-[#EEF0FD] dark:bg-[#1E1B4B] text-[#4338CA] dark:text-[#C7D2FE]"
                      : "bg-[var(--surface-elevated)] border-[var(--border-calm)] text-text-secondary hover:border-accent/60 hover:text-text-primary";

                    return (
                      <button
                        key={navItem.id}
                        type="button"
                        aria-label={`Đi tới câu ${i + 1}`}
                        onClick={() =>
                          document
                            .querySelector('[data-testid="quiz-item-' + (i + 1) + '"]')
                            ?.scrollIntoView({ behavior: "smooth", block: "center" })
                        }
                        className={`min-w-[2rem] h-8 px-2 rounded-lg border text-xs font-semibold transition cursor-pointer flex items-center justify-center ${pillClass}`}
                      >
                        {i + 1}
                      </button>
                    );
                  })}
                </div>
              </div>
            )}

            {/* Quiz Items */}
            <div className="space-y-4">
              {quizDetail.items.map((item, idx) => {
                const promptObj =
                  typeof item.prompt === "string"
                    ? { question: item.prompt }
                    : item.prompt;
                const rawQuestionText =
                  promptObj?.question || `Câu hỏi #${idx + 1}`;
                const rubricCriteria = promptObj?.rubricCriteria ?? [];
                const draft = draftAnswers[item.id] ?? "";
                const isSubmitting = Boolean(submittingIds[item.id]);
                const isEvaluated = Boolean(item.evaluatedAt);
                const questionText = formatCleanQuizPrompt(
                  rawQuestionText,
                  isEvaluated,
                  item.clozeSolution
                );

                return (
                  <div
                    key={item.id}
                    id={`quiz-item-${idx + 1}`}
                    data-testid={`quiz-item-${idx + 1}`}
                    className="rounded-lg border border-border bg-bg-primary p-5 space-y-4"
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <span className="inline-block text-[11px] font-semibold text-accent bg-accent-subtle px-2 py-0.5 rounded-md mb-2">
                          Câu {idx + 1} · {QUIZ_ITEM_TYPE_LABELS[item.itemType] ?? item.itemType.replace("_", " ")}
                        </span>
                        <div className="text-sm font-semibold text-text-primary leading-relaxed">
                          <RichInlineMarkdown text={questionText} />
                        </div>
                      </div>

                      {isEvaluated && item.isCorrect !== null && (
                        <span
                          className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-md text-xs font-semibold shrink-0 ${
                            item.isCorrect
                              ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                              : "bg-rose-500/15 text-rose-700 dark:text-rose-300"
                          }`}
                        >
                          {item.isCorrect ? (
                            <>
                              <CheckCircle2 className="w-3.5 h-3.5" />
                              <span>Chính xác</span>
                            </>
                          ) : (
                            <>
                              <XCircle className="w-3.5 h-3.5" />
                              <span>Chưa đúng</span>
                            </>
                          )}
                        </span>
                      )}
                    </div>

                    {/* Multiple choice options */}
                    {item.itemType === "multiple_choice" &&
                    Array.isArray(item.choices) &&
                    item.choices.length > 0 ? (
                      <div className="space-y-2">
                        {item.choices.map((choice) => {
                          const selected = draft === choice.key;
                          return (
                            <label
                              key={choice.key}
                              className={`flex items-start gap-3 p-3 rounded-md border text-sm cursor-pointer transition ${
                                selected
                                  ? "border-accent bg-accent-subtle text-text-primary"
                                  : "border-border bg-bg-secondary hover:border-accent/50 text-text-secondary"
                              }`}
                            >
                              <input
                                type="radio"
                                name={`quiz-choice-${item.id}`}
                                value={choice.key}
                                checked={selected}
                                onChange={() =>
                                  setDraftAnswers((prev) => ({
                                    ...prev,
                                    [item.id]: choice.key,
                                  }))
                                }
                                className="mt-1 accent-accent"
                              />
                              <span className="font-mono font-semibold text-xs px-1.5 py-0.5 rounded bg-bg-primary border border-border text-text-primary shrink-0">
                                {choice.key}
                              </span>
                              <span className="flex-1 text-text-primary">
                                <RichInlineMarkdown text={choice.text} />
                              </span>
                            </label>
                          );
                        })}
                      </div>
                    ) : item.itemType === "essay" ? (
                      <div className="space-y-2">
                        {rubricCriteria.length > 0 && (
                          <div className="rounded-md bg-bg-secondary border border-border p-3 text-xs text-text-secondary">
                            <p className="font-semibold text-text-primary mb-1">
                              Tiêu chí đánh giá:
                            </p>
                            <ul className="list-disc pl-4 space-y-0.5">
                              {rubricCriteria.map((c, ci) => (
                                <li key={ci}>{c}</li>
                              ))}
                            </ul>
                          </div>
                        )}
                        <textarea
                          rows={5}
                          value={draft}
                          onChange={(e) =>
                            setDraftAnswers((prev) => ({
                              ...prev,
                              [item.id]: e.target.value,
                            }))
                          }
                          onKeyDown={(e) => {
                            if (
                              (e.ctrlKey || e.metaKey) &&
                              e.key === "Enter" &&
                              quizMode === "practice" &&
                              !isEvaluated &&
                              !isSubmitting &&
                              draft.trim()
                            ) {
                              e.preventDefault();
                              void handleSubmitItem(item);
                            }
                          }}
                          placeholder="Trình bày câu trả lời của bạn... (Ctrl+Enter để nộp)"
                          className="w-full px-3 py-2 rounded-md border border-border bg-bg-secondary text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-accent"
                        />
                      </div>
                    ) : (
                      <input
                        type="text"
                        value={draft}
                        onChange={(e) =>
                          setDraftAnswers((prev) => ({
                            ...prev,
                            [item.id]: e.target.value,
                          }))
                        }
                        onKeyDown={(e) => {
                          if (
                            (e.ctrlKey || e.metaKey) &&
                            e.key === "Enter" &&
                            quizMode === "practice" &&
                            !isEvaluated &&
                            !isSubmitting &&
                            draft.trim()
                          ) {
                            e.preventDefault();
                            void handleSubmitItem(item);
                          }
                        }}
                        placeholder="Nhập câu trả lời của bạn..."
                        className="w-full h-9 px-3 rounded-md border border-border bg-bg-secondary text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-accent"
                      />
                    )}

                    {/* Submit Button & Save Wrong Question to FSRS */}
                    <div className="flex flex-wrap items-center justify-between gap-3 pt-1">
                      <div className="flex flex-wrap items-center gap-2.5">
                        {isEvaluated && item.clozeSolution && item.isCorrect === false ? (
                          <div className="text-xs text-text-secondary">
                            Đáp án chuẩn:{" "}
                            <span className="font-semibold text-emerald-600 dark:text-emerald-400">
                              <RichInlineMarkdown text={item.clozeSolution} />
                            </span>
                          </div>
                        ) : null}

                        {isEvaluated && item.isCorrect === false && (
                          <button
                            type="button"
                            data-testid={`save-wrong-fsrs-btn-${idx + 1}`}
                            disabled={
                              Boolean(savingWrongIds[item.id]) ||
                              Boolean(savedWrongIds[item.id])
                            }
                            onClick={() => void handleSaveWrongToFsrs(item, questionText)}
                            className="inline-flex items-center gap-1 h-7 px-2.5 rounded-md border border-amber-300 dark:border-amber-800 bg-amber-50 dark:bg-amber-950/30 text-[11px] font-semibold text-amber-700 dark:text-amber-300 hover:bg-amber-100 dark:hover:bg-amber-900/40 disabled:opacity-60 transition cursor-pointer"
                          >
                            <Brain className="w-3 h-3" />
                            <span>
                              {savingWrongIds[item.id]
                                ? "Đang lưu..."
                                : savedWrongIds[item.id]
                                ? "Đã lưu vào FSRS"
                                : "Lưu câu sai vào thẻ nhớ FSRS"}
                            </span>
                          </button>
                        )}
                      </div>

                      {quizMode === "practice" || isEvaluated ? (
                        <button
                          type="button"
                          disabled={
                            isSubmitting ||
                            (item.itemType !== "essay" && isEvaluated) ||
                            !draft.trim()
                          }
                          onClick={() => void handleSubmitItem(item)}
                          className="inline-flex items-center gap-1.5 h-8 px-3.5 rounded-md bg-accent dark:bg-[#818CF8] text-accent-contrast dark:text-[#0F0F12] text-xs font-medium hover:bg-accent-hover disabled:opacity-50 disabled:cursor-not-allowed transition cursor-pointer"
                        >
                          {isSubmitting ? (
                            <Loader2 className="w-3.5 h-3.5 animate-spin" />
                          ) : item.itemType !== "essay" && isEvaluated ? (
                            <CheckCircle2 className="w-3.5 h-3.5" />
                          ) : (
                            <Send className="w-3.5 h-3.5" />
                          )}
                          <span>
                            {isSubmitting
                              ? "Đang chấm..."
                              : item.itemType !== "essay" && isEvaluated
                              ? "Đã chấm"
                              : item.itemType === "essay" && isEvaluated
                              ? "Chấm lại"
                              : "Kiểm tra"}
                          </span>
                        </button>
                      ) : (
                        <span className="text-[11px] text-text-tertiary">
                          {draft.trim() ? "Đã ghi nhận đáp án nháp" : "Chưa chọn đáp án"}
                        </span>
                      )}
                    </div>

                    {/* Detailed Explanation */}
                    {(isEvaluated || quizDetail.status === "completed") && item.explanation && (
                      <div
                        data-testid={`quiz-item-explanation-${idx + 1}`}
                        className="rounded-lg border border-indigo-200 dark:border-indigo-900/60 bg-indigo-50/70 dark:bg-indigo-950/20 p-3.5 text-xs text-text-primary space-y-1.5"
                      >
                        <div className="flex items-center gap-1.5 font-semibold text-indigo-700 dark:text-indigo-300">
                          <Sparkles className="w-3.5 h-3.5 text-indigo-500 shrink-0" />
                          <span>Giải thích đáp án</span>
                        </div>
                        <div className="leading-relaxed text-text-secondary">
                          <RichInlineMarkdown text={item.explanation} />
                        </div>
                      </div>
                    )}

                    {/* AI Rubric Feedback */}
                    {item.aiRubricFeedback && (
                      <div className="rounded-md border border-accent/30 bg-accent-subtle p-4 space-y-2 text-xs">
                        <div className="flex items-center justify-between">
                          <span className="font-semibold text-accent flex items-center gap-1.5">
                            <Sparkles className="w-3.5 h-3.5" />
                            <span>Nhận xét chi tiết từ AI</span>
                          </span>
                          {item.aiRubricFeedback.masteryLevel && (
                            <span className="px-2 py-0.5 rounded bg-bg-primary border border-border font-semibold text-text-primary">
                              {item.aiRubricFeedback.masteryLevel}
                            </span>
                          )}
                        </div>
                        {item.aiRubricFeedback.summary && (
                          <p className="text-text-primary leading-relaxed">
                            {item.aiRubricFeedback.summary}
                          </p>
                        )}
                        {item.aiRubricFeedback.feedback && !item.aiRubricFeedback.summary && (
                          <p className="text-text-primary leading-relaxed">
                            {item.aiRubricFeedback.feedback}
                          </p>
                        )}
                        {item.aiRubricFeedback.strengths &&
                          item.aiRubricFeedback.strengths.length > 0 && (
                            <div>
                              <p className="font-semibold text-emerald-700 dark:text-emerald-300">
                                Điểm tốt:
                              </p>
                              <ul className="list-disc pl-4 text-text-secondary">
                                {item.aiRubricFeedback.strengths.map((s, si) => (
                                  <li key={si}>{s}</li>
                                ))}
                              </ul>
                            </div>
                          )}
                        {item.aiRubricFeedback.missingConcepts &&
                          item.aiRubricFeedback.missingConcepts.length > 0 && (
                            <div>
                              <p className="font-semibold text-amber-700 dark:text-amber-300">
                                Cần bổ sung:
                              </p>
                              <ul className="list-disc pl-4 text-text-secondary">
                                {item.aiRubricFeedback.missingConcepts.map(
                                  (m, mi) => (
                                    <li key={mi}>{m}</li>
                                  )
                                )}
                              </ul>
                            </div>
                          )}
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}

      {/* Modal: Tạo đề luyện mới */}
      {generateOpen && (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center p-4">
          <div className="w-full max-w-md rounded-xl border border-border bg-bg-primary shadow-xl p-5 space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="text-base font-semibold text-text-primary flex items-center gap-2">
                <Sparkles className="w-4 h-4 text-accent" />
                <span>Tạo đề luyện tập mới</span>
              </h3>
              <button
                type="button"
                onClick={() => setGenerateOpen(false)}
                className="text-text-tertiary hover:text-text-primary cursor-pointer"
                aria-label="Đóng hộp thoại tạo đề"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            <form onSubmit={handleGenerateCustomQuiz} className="space-y-4">
              <div>
                <label className="block text-xs font-medium text-text-secondary mb-1.5">
                  Tên đề luyện tập (tùy chọn)
                </label>
                <input
                  type="text"
                  value={genTitle}
                  onChange={(e) => setGenTitle(e.target.value)}
                  placeholder={`Ví dụ: Ôn nhanh ${courseTitle}`}
                  className="w-full h-9 px-3 rounded-md border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                />
              </div>

              {/* Phạm vi Chương */}
              <div>
                <label className="block text-xs font-medium text-text-secondary mb-1.5">
                  Phạm vi Chương
                </label>
                <select
                  value={genChapterId}
                  onChange={(e) => setGenChapterId(e.target.value)}
                  className="w-full h-9 px-2.5 rounded-md border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                >
                  <option value="">Toàn bộ khóa học</option>
                  {courseChapters.map((ch) => (
                    <option key={ch.id} value={ch.id}>
                      {ch.title}
                    </option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-medium text-text-secondary mb-1.5">
                    Số câu hỏi
                  </label>
                  <select
                    value={genCount}
                    onChange={(e) => setGenCount(Number(e.target.value))}
                    className="w-full h-9 px-2.5 rounded-md border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                  >
                    <option value={5}>5 câu (Ôn nhanh)</option>
                    <option value={10}>10 câu (Tiêu chuẩn)</option>
                    <option value={15}>15 câu (Chuyên sâu)</option>
                    <option value={20}>20 câu (Tổng ôn)</option>
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-medium text-text-secondary mb-1.5">
                    Dạng câu hỏi ưu tiên
                  </label>
                  <select
                    value={genItemType}
                    onChange={(e) => setGenItemType(e.target.value)}
                    className="w-full h-9 px-2.5 rounded-md border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                  >
                    <option value="all">Tổng hợp mọi dạng</option>
                    <option value="multiple_choice">Trắc nghiệm</option>
                    <option value="short_answer">Trả lời ngắn / Cloze</option>
                    <option value="essay">Tự luận</option>
                  </select>
                </div>
              </div>

              <div className="flex items-center justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setGenerateOpen(false)}
                  className="h-9 px-3.5 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary cursor-pointer"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  data-testid="confirm-generate-quiz-btn"
                  disabled={generatingQuiz}
                  className="inline-flex items-center gap-1.5 h-9 px-4 rounded-md bg-accent dark:bg-[#818CF8] text-accent-contrast dark:text-[#0F0F12] text-xs font-medium hover:bg-accent-hover disabled:opacity-50 cursor-pointer"
                >
                  {generatingQuiz ? (
                    <Loader2 className="w-3.5 h-3.5 animate-spin" />
                  ) : (
                    <Plus className="w-3.5 h-3.5" />
                  )}
                  <span>{generatingQuiz ? "Đang tạo đề..." : "Tạo đề ngay"}</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default function CourseQuizPage() {
  return (
    <Suspense
      fallback={
        <div className="flex items-center justify-center min-h-[60vh] bg-bg-primary">
          <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
        </div>
      }
    >
      <CourseQuizPageInner />
    </Suspense>
  );
}
