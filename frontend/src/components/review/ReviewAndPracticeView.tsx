"use client";

import React, { useEffect, useState } from "react";
import {
  Brain,
  Target,
  Trophy,
  RotateCw,
  Home,
  Zap,
  Archive,
  BookOpen,
  Sparkles,
  CheckCircle2,
  Lightbulb,
  LayoutDashboard
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { evaluateFsrsV5 } from "@/lib/fsrs-engine";
import { enqueueOfflineMutation, removeOfflineMutation } from "@/lib/offline-queue";
import { triggerCompletionCelebration, waitForElementAndPulse } from "@/lib/animations.ts";

interface DueFlashcardItem {
  id: string;
  courseId?: string | null;
  lessonId?: string;
  documentId?: string;
  pageNumber?: number;
  annotationId?: string;
  cardType: string;
  sourceTitleSnapshot?: string;
  sourceQuoteSnapshot?: string;
  frontText: string;
  backText: string;
  stability: number;
  difficulty: number;
  reps: number;
  lapses: number;
  state: number;
  lastReviewAt: string | null;
  dueAt: string;
  tags: Array<{ id: string; name: string; colorKey: string }>;
  intervalPreviews: {
    again: string;
    hard: string;
    good: string;
    easy: string;
  };
}

interface TagOption {
  id: string;
  name: string;
  colorKey: string;
}

interface PracticeQuizItemDto {
  id: string;
  quizId: string;
  itemType: "multiple_choice" | "cloze" | "short_answer" | "essay";
  prompt: { question: string; rubricCriteria?: string[] };
  choices?: Array<{ key: string; text: string }>;
  clozeSolution?: string;
  userAnswer?: { text: string };
  isCorrect?: boolean;
  aiRubricFeedback?: {
    summary: string;
    strengths: string[];
    missingConcepts: string[];
    modelComparison: string;
    masteryLevel: string;
    modelUsed?: string;
  };
}

interface PracticeQuizDto {
  id: string;
  title: string;
  courseId?: string | null;
  documentId?: string | null;
  sourceDocumentId?: string | null;
  lessonId?: string | null;
  status: string;
  totalItems: number;
  correctItems: number;
  items: PracticeQuizItemDto[];
}

export interface PracticeQuizListItemDto {
  id: string;
  title: string;
  status?: string;
  totalItems?: number;
  correctItems?: number;
  documentId?: string | null;
  sourceDocumentId?: string | null;
  lessonId?: string | null;
  courseId?: string | null;
}

interface ReviewCourseOption {
  id?: string;
  courseId?: string;
  title: string;
  sourceFolder?: string | null;
  sourceDocumentId?: string | null;
  quizId?: string | null;
  lessonQuizMap?: Record<string, string> | null;
  tagId?: string | null;
  tagName?: string | null;
  lessonIds?: string[];
  lessons?: Array<{ id: string; title?: string; quizId?: string | null }>;
  chapters?: Array<{
    id: string;
    lessons?: Array<{ id: string; title?: string; quizId?: string | null }>;
  }>;
}

interface ReviewAndPracticeViewProps {
  tags: TagOption[];
  onReviewSessionUpdated: () => void;
  courses?: ReviewCourseOption[];
  quizzes?: PracticeQuizListItemDto[];
  coursesLoading?: boolean;
}

const DEFAULT_PRACTICE_QUIZ_ID = "77777777-7777-4777-8777-777777777777";
const DEFAULT_DEMO_USER_ID = "11111111-1111-4111-8111-111111111111";

function isDefaultDemoUser(): boolean {
  if (typeof window === "undefined") return true;
  try {
    const activeUserId = window.localStorage.getItem("swms_active_user_id");
    if (activeUserId && activeUserId.trim()) {
      return activeUserId.trim() === DEFAULT_DEMO_USER_ID;
    }
    const rawUser = window.localStorage.getItem("swms_active_user");
    if (rawUser) {
      const parsed = JSON.parse(rawUser) as { id?: string };
      if (parsed?.id) {
        return parsed.id === DEFAULT_DEMO_USER_ID;
      }
    }
  } catch {
    // ignore localStorage parse errors
  }
  return true;
}

export function ReviewAndPracticeView({
  tags,
  onReviewSessionUpdated,
  courses: propCourses,
  quizzes: propQuizzes,
  coursesLoading = false
}: ReviewAndPracticeViewProps) {
  const {
    activeReviewSubTab,
    setActiveReviewSubTab,
    selectedTagFilterId,
    setSelectedTagFilterId,
    setActiveMode,
    setActiveLesson,
    setActiveDocument,
    activeDocumentId,
    activeLessonId,
    activeCourseId,
    setActiveCourseId,
    activeQuizId,
    setActiveQuizId,
    practiceQuizDraftAnswers,
    setPracticeQuizDraftAnswer
  } = useWorkspaceStore();

  const [cards, setCards] = useState<DueFlashcardItem[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);
  const [lastEvalMs, setLastEvalMs] = useState<number | null>(null);
  const [sessionCompleted, setSessionCompleted] = useState(false);
  const [loadingCards, setLoadingCards] = useState(true);
  const [lastReviewedCardId, setLastReviewedCardId] = useState<string | null>(null);
  const [undoBanner, setUndoBanner] = useState<string | null>(null);
  const [fetchedCourses, setFetchedCourses] = useState<ReviewCourseOption[]>([]);
  const [selectedCourseFilterId, setSelectedCourseFilterId] = useState<string>(
    activeCourseId || "all"
  );
  const [onlyCurrentLessonCards, setOnlyCurrentLessonCards] = useState(false);
  const [showAllTags, setShowAllTags] = useState(false);
  const [quizSubmitError, setQuizSubmitError] = useState<string | null>(null);

  // Sync course filter automatically when workspace activeCourseId changes from Reader/Catalog
  useEffect(() => {
    if (activeCourseId) {
      setSelectedCourseFilterId(activeCourseId);
    }
  }, [activeCourseId]);

  useEffect(() => {
    const hasCoursesFromProps =
      propCourses &&
      propCourses.length > 0 &&
      (!activeCourseId ||
        propCourses.some((c) => (c.courseId || c.id) === activeCourseId));
    if (hasCoursesFromProps || coursesLoading) {
      return;
    }

    const controller = new AbortController();
    fetch("/api/v1/courses", {
      headers: getClientRequestHeaders(),
      signal: controller.signal
    })
      .then((r) => (r.ok ? r.json() : []))
      .then((list) => {
        if (!controller.signal.aborted && Array.isArray(list)) {
          setFetchedCourses(list);
        }
      })
      .catch(() => {});
    return () => {
      controller.abort();
    };
  }, [activeReviewSubTab, activeCourseId, propCourses, coursesLoading]);

  const effectiveCourses = React.useMemo<ReviewCourseOption[]>(() => {
    return propCourses && propCourses.length > 0 ? propCourses : fetchedCourses;
  }, [propCourses, fetchedCourses]);

  const selectedCourseObj = React.useMemo(() => {
    if (!selectedCourseFilterId || selectedCourseFilterId === "all") return null;
    return (
      effectiveCourses.find(
        (c) => (c.courseId || c.id) === selectedCourseFilterId
      ) ?? null
    );
  }, [effectiveCourses, selectedCourseFilterId]);

  const selectedCourseLessonIds = React.useMemo(() => {
    const ids = new Set<string>();
    if (!selectedCourseObj) return ids;
    for (const id of selectedCourseObj.lessonIds || []) ids.add(id);
    for (const l of selectedCourseObj.lessons || []) ids.add(l.id);
    for (const ch of selectedCourseObj.chapters || []) {
      for (const l of ch.lessons || []) ids.add(l.id);
    }
    return ids;
  }, [selectedCourseObj]);

  // Practice Quiz State
  const [quiz, setQuiz] = useState<PracticeQuizDto | null>(null);
  const [fetchedQuizzes, setFetchedQuizzes] = useState<PracticeQuizListItemDto[]>([]);
  const [loadingQuizList, setLoadingQuizList] = useState(true);
  const [submittingItemId, setSubmittingItemId] = useState<string | null>(null);

  const loadQuizList = async (courseIdOverride?: string) => {
    setLoadingQuizList(true);
    try {
      const targetCourseId =
        courseIdOverride !== undefined ? courseIdOverride : selectedCourseFilterId;
      const headers = getClientRequestHeaders();
      const [allResp, courseResp] = await Promise.all([
        fetch("/api/v1/practice-quizzes", { headers }),
        targetCourseId && targetCourseId !== "all"
          ? fetch(
              `/api/v1/practice-quizzes?courseId=${encodeURIComponent(targetCourseId)}`,
              { headers }
            )
          : Promise.resolve(null)
      ]);

      const mergedMap = new Map<string, PracticeQuizListItemDto>();
      if (allResp.ok) {
        const list = await allResp.json();
        if (Array.isArray(list)) {
          for (const q of list) {
            if (q && q.id) mergedMap.set(q.id, q);
          }
        }
      }
      if (courseResp && courseResp.ok) {
        const courseList = await courseResp.json();
        if (Array.isArray(courseList)) {
          for (const q of courseList) {
            if (q && q.id) {
              mergedMap.set(q.id, {
                ...q,
                courseId: q.courseId || targetCourseId
              });
            }
          }
        }
      }
      setFetchedQuizzes(Array.from(mergedMap.values()));
    } catch {
      // Ignore network errors and fallback to propQuizzes / default quiz
    } finally {
      setLoadingQuizList(false);
    }
  };

  const effectiveQuizList: PracticeQuizListItemDto[] = React.useMemo(() => {
    const map = new Map<string, PracticeQuizListItemDto>();
    for (const q of propQuizzes || []) {
      if (q && q.id) map.set(q.id, q);
    }
    for (const q of fetchedQuizzes) {
      if (q && q.id) map.set(q.id, q);
    }
    if (quiz && quiz.id && !map.has(quiz.id)) {
      map.set(quiz.id, {
        id: quiz.id,
        title: quiz.title,
        courseId: quiz.courseId ?? null,
        documentId: quiz.documentId ?? null,
        sourceDocumentId: quiz.sourceDocumentId ?? null,
        lessonId: quiz.lessonId ?? null,
        status: quiz.status,
        totalItems: quiz.totalItems,
        correctItems: quiz.correctItems
      });
    }
    if (isDefaultDemoUser() && !map.has(DEFAULT_PRACTICE_QUIZ_ID)) {
      map.set(DEFAULT_PRACTICE_QUIZ_ID, {
        id: DEFAULT_PRACTICE_QUIZ_ID,
        title: "Đề luyện tập tổng hợp: Chương 3 — Bộ nhớ ảo & Phân trang"
      });
    }
    return Array.from(map.values());
  }, [propQuizzes, fetchedQuizzes, quiz]);

  const resolvedQuizId = React.useMemo(() => {
    if (activeQuizId) return activeQuizId;
    const courseFilterTarget =
      selectedCourseFilterId !== "all" ? selectedCourseFilterId : activeCourseId;
    if (courseFilterTarget) {
      const byCourse = effectiveQuizList.find((q) => q.courseId === courseFilterTarget);
      if (byCourse) return byCourse.id;
      const courseMatch = effectiveCourses.find(
        (c) => (c.courseId || c.id) === courseFilterTarget && c.quizId
      );
      if (courseMatch?.quizId) return courseMatch.quizId;
    }
    if (activeDocumentId) {
      const byDoc = effectiveQuizList.find(
        (q) => q.documentId === activeDocumentId || q.sourceDocumentId === activeDocumentId
      );
      if (byDoc) return byDoc.id;
      const courseByDoc = effectiveCourses.find(
        (c) => c.sourceDocumentId === activeDocumentId && c.quizId
      );
      if (courseByDoc?.quizId) return courseByDoc.quizId;
    }
    return isDefaultDemoUser() ? DEFAULT_PRACTICE_QUIZ_ID : (effectiveQuizList[0]?.id ?? null);
  }, [
    activeQuizId,
    selectedCourseFilterId,
    activeCourseId,
    activeDocumentId,
    effectiveQuizList,
    effectiveCourses
  ]);

  const filteredQuizList = React.useMemo(() => {
    if (!selectedCourseFilterId || selectedCourseFilterId === "all") {
      return effectiveQuizList;
    }
    const matches = effectiveQuizList.filter((q) => {
      if (q.courseId && q.courseId === selectedCourseFilterId) return true;
      if (selectedCourseObj?.quizId && q.id === selectedCourseObj.quizId) return true;
      if (
        selectedCourseObj?.lessonQuizMap &&
        Object.values(selectedCourseObj.lessonQuizMap).includes(q.id)
      ) {
        return true;
      }
      if (
        selectedCourseObj?.sourceDocumentId &&
        (q.sourceDocumentId === selectedCourseObj.sourceDocumentId ||
          q.documentId === selectedCourseObj.sourceDocumentId)
      ) {
        return true;
      }
      if (q.lessonId && selectedCourseLessonIds.has(q.lessonId)) return true;
      return false;
    });
    return matches;
  }, [
    effectiveQuizList,
    selectedCourseFilterId,
    selectedCourseObj,
    selectedCourseLessonIds
  ]);

  const selectedCourseObjRef = React.useRef(selectedCourseObj);
  selectedCourseObjRef.current = selectedCourseObj;
  const selectedCourseLessonIdsRef = React.useRef(selectedCourseLessonIds);
  selectedCourseLessonIdsRef.current = selectedCourseLessonIds;
  const lastQueueFilterKeyRef = React.useRef<string>("");
  const dueQueueReqSeqRef = React.useRef(0);
  const dueQueueAbortRef = React.useRef<AbortController | null>(null);

  useEffect(() => {
    return () => {
      dueQueueAbortRef.current?.abort();
    };
  }, []);

  const loadDueQueue = async (
    tagId: string | null,
    courseFilterId: string = selectedCourseFilterId,
    lessonOnly: boolean = onlyCurrentLessonCards,
    resetPosition = false
  ) => {
    const filterKey = `${tagId || ""}|${courseFilterId}|${lessonOnly}|${
      lessonOnly ? activeLessonId : ""
    }`;
    const filterChanged = lastQueueFilterKeyRef.current !== filterKey;
    if (filterChanged || resetPosition) {
      setLoadingCards(true);
    }

    dueQueueAbortRef.current?.abort();
    const controller = new AbortController();
    dueQueueAbortRef.current = controller;
    const reqSeq = ++dueQueueReqSeqRef.current;
    const params = new URLSearchParams({ includeAll: "true" });
    if (tagId) params.set("tagId", tagId);
    if (courseFilterId && courseFilterId !== "all") {
      params.set("courseId", courseFilterId);
    }
    if (lessonOnly && activeLessonId) {
      params.set("lessonId", activeLessonId);
    }

    try {
      const resp = await fetch(`/api/v1/flashcards/due?${params.toString()}`, {
        headers: getClientRequestHeaders(),
        signal: controller.signal
      });
      if (controller.signal.aborted || reqSeq !== dueQueueReqSeqRef.current) return;

      if (resp.ok) {
        let list: DueFlashcardItem[] = await resp.json();
        if (reqSeq !== dueQueueReqSeqRef.current) return;

        const courseObj = selectedCourseObjRef.current;
        const courseLessonIds = selectedCourseLessonIdsRef.current;
        // Backend supports ?courseId= natively; apply client-side guard only when courseId metadata is explicitly present
        if (courseFilterId && courseFilterId !== "all" && courseObj) {
          const hasCourseField = list.some((c) => Boolean(c.courseId));
          if (hasCourseField) {
            list = list.filter(
              (c) =>
                c.courseId === courseFilterId ||
                (c.lessonId && courseLessonIds.has(c.lessonId)) ||
                (courseObj.sourceDocumentId &&
                  c.documentId === courseObj.sourceDocumentId)
            );
          }
        }

        if (lessonOnly && activeLessonId) {
          list = list.filter((c) => c.lessonId === activeLessonId);
        }

        const prioritized = activeDocumentId
          ? [...list].sort((a, b) => {
              const aMatch = a.documentId === activeDocumentId ? 1 : 0;
              const bMatch = b.documentId === activeDocumentId ? 1 : 0;
              if (aMatch !== bMatch) return bMatch - aMatch;
              const aAnn = a.annotationId ? 1 : 0;
              const bAnn = b.annotationId ? 1 : 0;
              return bAnn - aAnn;
            })
          : list;
        setCards(prioritized);
        if (filterChanged || resetPosition) {
          lastQueueFilterKeyRef.current = filterKey;
          setCurrentIndex(0);
          setIsFlipped(false);
        } else {
          setCurrentIndex((prev) =>
            prioritized.length === 0 ? 0 : Math.min(prev, prioritized.length - 1)
          );
        }
        setSessionCompleted(prioritized.length === 0);
      }
    } catch (err) {
      if ((err as { name?: string })?.name === "AbortError") {
        return;
      }
      console.error("Failed to load due flashcards:", err);
    } finally {
      if (!controller.signal.aborted && reqSeq === dueQueueReqSeqRef.current) {
        setLoadingCards(false);
      }
    }
  };

  const loadPracticeQuiz = async (targetQuizId?: string | null) => {
    const idToLoad =
      targetQuizId || resolvedQuizId || (isDefaultDemoUser() ? DEFAULT_PRACTICE_QUIZ_ID : null);
    if (!idToLoad) {
      setQuiz(null);
      return;
    }
    const headers = getClientRequestHeaders();
    let resp = await fetch(`/api/v1/practice-quizzes/${encodeURIComponent(idToLoad)}`, {
      headers,
      credentials: "include"
    });
    if (!resp.ok && isDefaultDemoUser() && idToLoad !== DEFAULT_PRACTICE_QUIZ_ID) {
      resp = await fetch(`/api/v1/practice-quizzes/${DEFAULT_PRACTICE_QUIZ_ID}`, {
        headers,
        credentials: "include"
      });
    }
    if (resp.ok) {
      const data: PracticeQuizDto = await resp.json();
      setQuiz(data);
      setQuizSubmitError(null);
    } else {
      setQuiz(null);
    }
  };

  const activeLessonFilterDep = onlyCurrentLessonCards ? activeLessonId : "";
  useEffect(() => {
    loadDueQueue(selectedTagFilterId, selectedCourseFilterId, onlyCurrentLessonCards);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    selectedTagFilterId,
    selectedCourseFilterId,
    onlyCurrentLessonCards,
    activeLessonFilterDep
  ]);

  useEffect(() => {
    loadQuizList(selectedCourseFilterId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeReviewSubTab, activeDocumentId, activeCourseId, selectedCourseFilterId]);

  useEffect(() => {
    loadPracticeQuiz(resolvedQuizId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [resolvedQuizId]);

  const currentCard = cards[currentIndex] ?? null;

  const handleUndoLastReview = async () => {
    if (!lastReviewedCardId) return;
    const cardIdToUndo = lastReviewedCardId;
    try {
      const resp = await fetch(`/api/v1/flashcards/${cardIdToUndo}/undo-last-review`, {
        method: "POST",
        headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
        credentials: "include"
      });
      if (resp.ok) {
        onReviewSessionUpdated();
      }
    } catch {
      // Fallback safely if offline
    }
    if (sessionCompleted) {
      setSessionCompleted(false);
      setCurrentIndex(Math.max(0, cards.length - 1));
    } else {
      setCurrentIndex((prev) => Math.max(0, prev - 1));
    }
    setIsFlipped(false);
    setLastReviewedCardId(null);
    setUndoBanner("Đã hoàn tác đánh giá thẻ vừa rồi (Undo)!");
    setTimeout(() => setUndoBanner(null), 4000);
  };

  // Keyboard shortcuts: Space to flip (front), Space or 1..4 to rate (back), Z to undo last review
  useEffect(() => {
    if (activeReviewSubTab !== "fsrs") return;
    const onKey = (e: KeyboardEvent) => {
      if (
        e.target instanceof HTMLInputElement ||
        e.target instanceof HTMLTextAreaElement ||
        (e.target as HTMLElement)?.isContentEditable
      ) {
        return;
      }
      if (!e.ctrlKey && !e.metaKey && !e.altKey && (e.key === "z" || e.key === "Z")) {
        if (lastReviewedCardId) {
          e.preventDefault();
          handleUndoLastReview();
        }
        return;
      }
      if (!currentCard || sessionCompleted) return;
      if (e.code === "Space") {
        e.preventDefault();
        if (!isFlipped) {
          setIsFlipped(true);
        } else {
          handleRateCard(3); // Spacebar dual-action on back face submits default rating: Good
        }
      } else if (isFlipped && ["1", "2", "3", "4"].includes(e.key)) {
        e.preventDefault();
        handleRateCard(Number(e.key) as 1 | 2 | 3 | 4);
      }
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeReviewSubTab, currentCard, isFlipped, sessionCompleted, currentIndex, lastReviewedCardId]);

  // Keyboard shortcuts in Practice Quiz mode: A/B/C/D to select & submit multiple-choice
  useEffect(() => {
    if (activeReviewSubTab !== "quiz" || !quiz) return;
    const onQuizKey = (e: KeyboardEvent) => {
      if (
        e.target instanceof HTMLInputElement ||
        e.target instanceof HTMLTextAreaElement ||
        (e.target as HTMLElement)?.isContentEditable
      ) {
        return;
      }
      if (e.ctrlKey || e.metaKey || e.altKey) return;
      const upper = e.key.toUpperCase();
      if (["A", "B", "C", "D"].includes(upper)) {
        const mcItem = quiz.items.find((it) => it.itemType === "multiple_choice");
        if (mcItem) {
          e.preventDefault();
          setPracticeQuizDraftAnswer(mcItem.id, upper);
          handleSubmitQuizItem(mcItem.id, upper);
        }
      }
    };
    window.addEventListener("keydown", onQuizKey);
    return () => window.removeEventListener("keydown", onQuizKey);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeReviewSubTab, quiz]);

  const handleRateCard = async (rating: 1 | 2 | 3 | 4) => {
    if (!currentCard || !isFlipped) return;

    setLastReviewedCardId(currentCard.id);
    setUndoBanner(null);

    // 1. Instantaneous Client-Side FSRS v5 Evaluation (SLO: p99 < 5ms)
    const nowIso = new Date().toISOString();
    const localResult = evaluateFsrsV5(
      {
        stability: currentCard.stability,
        difficulty: currentCard.difficulty,
        reps: currentCard.reps,
        lapses: currentCard.lapses,
        state: currentCard.state,
        lastReviewAt: currentCard.lastReviewAt,
        dueAt: currentCard.dueAt
      },
      rating,
      nowIso
    );
    setLastEvalMs(localResult.evaluationMs);

    const reviewEventId = crypto.randomUUID();
    const reviewPayload = {
      reviewEventId,
      flashcardId: currentCard.id,
      rating,
      clientTimestamp: nowIso,
      reviewDurationMs: 2800
    };

    // 2. Stage into Resilient Online Mutation Queue (IndexedDB/LocalStorage)
    enqueueOfflineMutation({
      mutationId: reviewEventId,
      entityType: "flashcard_review",
      entityId: currentCard.id,
      baseVersion: 1,
      deviceId: "swms-web-client",
      clientTimestamp: nowIso,
      payload: reviewPayload
    });

    // Advance UI immediately without blocking on HTTP round-trip!
    if (currentIndex + 1 < cards.length) {
      setIsFlipped(false);
      setCurrentIndex((prev) => prev + 1);
    } else {
      setSessionCompleted(true);
      setTimeout(() => triggerCompletionCelebration("review-celebration-Anchor"), 60);
    }

    // 3. Asynchronous Idempotent Server Sync
    try {
      const resp = await fetch("/api/v1/flashcards/sync-reviews", {
        method: "POST",
        headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({ reviews: [reviewPayload] })
      });
      if (resp.ok) {
        removeOfflineMutation(reviewEventId);
        onReviewSessionUpdated();
      }
    } catch {
      // Remains safely queued in OfflineMutationQueue until online reconnection
    }
  };

  const handleArchiveCardSoftDelete = async (cardId: string) => {
    const resp = await fetch(`/api/v1/flashcards/${cardId}`, {
      method: "DELETE",
      headers: getClientRequestHeaders()
    });
    if (resp.ok) {
      await loadDueQueue(selectedTagFilterId, selectedCourseFilterId, onlyCurrentLessonCards, true);
      onReviewSessionUpdated();
    }
  };

  const handleSubmitQuizItem = async (itemId: string, fallbackAnswer: string) => {
    if (!quiz) return;
    const targetItem = quiz.items.find((it) => it.id === itemId);
    if (
      targetItem &&
      targetItem.itemType !== "essay" &&
      targetItem.isCorrect !== null &&
      targetItem.isCorrect !== undefined
    ) {
      setQuizSubmitError("Câu hỏi này đã được chấm trước đó, không thể nộp lại.");
      return;
    }
    const answerText = (practiceQuizDraftAnswers[itemId] ?? fallbackAnswer).trim();
    if (!answerText) {
      setQuizSubmitError("Vui lòng nhập câu trả lời trước khi nộp.");
      return;
    }

    setSubmittingItemId(itemId);
    setQuizSubmitError(null);
    try {
      const resp = await fetch(`/api/v1/practice-quizzes/${quiz.id}/items/${itemId}/submit`, {
        method: "POST",
        headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
        credentials: "include",
        body: JSON.stringify({
          answer: answerText,
          idempotencyKey: crypto.randomUUID(),
          waitForAiCompletion: true
        })
      });
      if (resp.ok || resp.status === 202) {
        await loadPracticeQuiz(quiz.id);
        onReviewSessionUpdated();
      } else {
        const errBody = await resp.json().catch(() => null);
        if (resp.status === 409) {
          setQuizSubmitError(
            errBody?.error || "Câu hỏi này đã được chấm trước đó, không thể nộp lại."
          );
          await loadPracticeQuiz(quiz.id);
        } else if (resp.status === 429) {
          setQuizSubmitError(
            errBody?.error ||
              "Bạn đã hết hạn mức 50 AI credits. Vui lòng thử lại sau hoặc nạp thêm."
          );
        } else if (resp.status === 400) {
          setQuizSubmitError(
            errBody?.error || "Nội dung câu trả lời không hợp lệ hoặc đang để trống."
          );
        } else {
          setQuizSubmitError(
            errBody?.error || `Gửi câu trả lời thất bại (HTTP ${resp.status}).`
          );
        }
      }
    } catch {
      setQuizSubmitError("Lỗi kết nối máy chủ khi nộp câu trả lời.");
    } finally {
      setSubmittingItemId(null);
    }
  };

  const renderClozeFormattedText = (text: string, reveal: boolean) => {
    const regex = /\{\{c1::(.*?)\}\}/g;
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
          key={match.index}
          className={`px-2 py-0.5 mx-1 rounded-md font-extrabold ${
            reveal
              ? "bg-emerald-500/20 text-emerald-700 dark:text-emerald-300 border border-emerald-500/50"
              : "bg-indigo-600 text-white"
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

  return (
    <div
      id="review-celebration-Anchor"
      data-testid="mode-3-review-container"
      className="relative max-w-6xl mx-auto px-4 sm:px-6 py-6 space-y-6"
    >
      {/* Top Sub-Navigation, Course & Tag Filter Bar (Unified Review Queue — Zero Decks per ADR-020) */}
      <div className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-4 flex flex-col gap-3.5 shadow-xs">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              data-testid="review-subtab-fsrs"
              data-subtab="flashcards"
              aria-label="Hàng đợi ôn tập Flashcards FSRS"
              onClick={() => setActiveReviewSubTab("fsrs")}
              className={`px-3.5 py-1.5 rounded-xl text-xs font-bold cursor-pointer transition inline-flex items-center gap-1.5 ${
                activeReviewSubTab === "fsrs"
                  ? "bg-indigo-600 text-white shadow-xs"
                  : "bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"
              }`}
            >
              <Brain className="w-4 h-4 shrink-0" />
              <span>Hàng đợi ôn tập FSRS v5 ({cards.length} thẻ)</span>
            </button>
            <button
              type="button"
              data-testid="review-subtab-quiz"
              data-subtab="quiz"
              aria-label="Bài luyện tập thực hành"
              onClick={() => setActiveReviewSubTab("quiz")}
              className={`px-3.5 py-1.5 rounded-xl text-xs font-bold cursor-pointer transition inline-flex items-center gap-1.5 ${
                activeReviewSubTab === "quiz"
                  ? "bg-indigo-600 text-white shadow-xs"
                  : "bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"
              }`}
            >
              <Target className="w-4 h-4 shrink-0" />
              <span>Bài luyện tập thực hành (Practice Quiz & AI Rubric)</span>
            </button>
          </div>

          {activeReviewSubTab === "fsrs" && effectiveCourses.length > 0 && (
            <div className="flex flex-wrap items-center gap-2 text-xs">
              <label
                htmlFor="flashcard-course-filter-select"
                className="font-bold text-slate-500 dark:text-slate-400"
              >
                Khóa học:
              </label>
              <select
                id="flashcard-course-filter-select"
                data-testid="flashcard-course-filter-select"
                value={selectedCourseFilterId}
                onChange={(e) => {
                  const val = e.target.value;
                  setSelectedCourseFilterId(val);
                  setActiveCourseId(val === "all" ? null : val);
                  if (val === "all") setOnlyCurrentLessonCards(false);
                }}
                className="px-2.5 py-1.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-xs font-bold text-slate-800 dark:text-slate-100 max-w-[260px] cursor-pointer"
              >
                <option value="all">Tất cả khóa học</option>
                {effectiveCourses.map((c) => {
                  const cid = c.courseId || c.id || "";
                  if (!cid) return null;
                  return (
                    <option key={cid} value={cid}>
                      {c.title}
                    </option>
                  );
                })}
              </select>

              {selectedCourseFilterId !== "all" &&
                activeLessonId &&
                selectedCourseLessonIds.has(activeLessonId) && (
                  <button
                    type="button"
                    data-testid="flashcard-lesson-only-toggle"
                    onClick={() => setOnlyCurrentLessonCards((v) => !v)}
                    className={`px-2.5 py-1.5 rounded-xl border text-[11px] font-bold cursor-pointer transition ${
                      onlyCurrentLessonCards
                        ? "border-indigo-500 bg-indigo-50 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300"
                        : "border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-300"
                    }`}
                  >
                    {onlyCurrentLessonCards ? "✓ Chỉ bài đang học" : "Chỉ bài đang học"}
                  </button>
                )}
            </div>
          )}
        </div>

        {/* Tag Filter Pills */}
        {activeReviewSubTab === "fsrs" && (
          <div className="flex flex-wrap items-center gap-1.5 text-xs pt-2 border-t border-slate-100 dark:border-slate-800">
            <span className="font-bold text-slate-400 mr-1">Lọc theo Tag:</span>
            <button
              type="button"
              data-testid="filter-tag-all"
              onClick={() => setSelectedTagFilterId(null)}
              className={`px-2.5 py-1 rounded-full font-bold cursor-pointer ${
                selectedTagFilterId === null
                  ? "bg-indigo-600 text-white"
                  : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300"
              }`}
            >
              Tất cả
            </button>
            {(showAllTags
              ? tags
              : tags.filter((tg, i) => i < 8 || tg.id === selectedTagFilterId)
            ).map((tg) => (
              <button
                key={tg.id}
                type="button"
                data-testid={`filter-tag-${tg.name}`}
                onClick={() => setSelectedTagFilterId(tg.id)}
                className={`px-2.5 py-1 rounded-full font-bold cursor-pointer ${
                  selectedTagFilterId === tg.id
                    ? "bg-indigo-600 text-white"
                    : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300"
                }`}
              >
                #{tg.name}
              </button>
            ))}
            {tags.length > 8 && (
              <button
                type="button"
                onClick={() => setShowAllTags((v) => !v)}
                className="px-2.5 py-1 rounded-full border border-slate-200 dark:border-slate-700 text-[11px] font-semibold text-slate-500 hover:text-indigo-600 cursor-pointer"
              >
                {showAllTags ? "Thu gọn" : `+${tags.length - 8} tag khác`}
              </button>
            )}
          </div>
        )}
      </div>

      {/* ================================================================== */}
      {/* SUB-TAB 1: FSRS v5 3D ACTIVE RECALL FLIP CARD CANVAS               */}
      {/* ================================================================== */}
      {activeReviewSubTab === "fsrs" && (
        <div className="flex flex-col items-center space-y-6">
          {undoBanner && (
            <div
              role="status"
              aria-live="polite"
              className="w-full max-w-[800px] rounded-xl border border-indigo-500/40 bg-indigo-50/90 dark:bg-indigo-950/60 px-4 py-2 text-xs font-bold text-indigo-700 dark:text-indigo-300 flex items-center justify-between"
            >
              <span>{undoBanner}</span>
              <button
                type="button"
                onClick={() => setUndoBanner(null)}
                className="text-indigo-500 hover:underline text-[11px]"
              >
                Đóng
              </button>
            </div>
          )}
          {loadingCards ? (
            <div className="p-12 text-sm text-slate-500">Đang tải Hàng đợi Ôn tập FSRS v5...</div>
          ) : sessionCompleted || !currentCard ? (
            <div
              data-testid="fsrs-session-complete-card"
              className="w-full max-w-[800px] rounded-3xl bg-white dark:bg-slate-900 border border-emerald-500/40 p-8 text-center space-y-4 shadow-xl"
            >
              <div className="w-12 h-12 mx-auto rounded-full bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center">
                <Trophy className="w-6 h-6" />
              </div>
              <h2 className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-white">
                Tuyệt vời! Bạn đã hoàn thành phiên ôn tập FSRS v5 hôm nay!
              </h2>
              <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-400 max-w-lg mx-auto">
                Tất cả lịch sử đánh giá đã được đồng bộ bất đồng bộ qua sự kiện bất biến{" "}
                <code className="font-mono text-indigo-600">review_event_id</code> và kích hoạt mốc{" "}
                <strong>Activation Milestone</strong>.
              </p>
              <div className="flex flex-wrap justify-center gap-3 pt-2">
                {(selectedCourseFilterId !== "all" || selectedTagFilterId !== null || onlyCurrentLessonCards) && (
                  <button
                    type="button"
                    data-testid="reset-flashcard-filters-btn"
                    onClick={() => {
                      setSelectedCourseFilterId("all");
                      setActiveCourseId(null);
                      setSelectedTagFilterId(null);
                      setOnlyCurrentLessonCards(false);
                    }}
                    className="px-4 py-2 rounded-xl border border-indigo-400/50 bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 text-xs font-bold cursor-pointer inline-flex items-center gap-1.5 hover:bg-indigo-100 dark:hover:bg-indigo-900 transition"
                  >
                    <span>Xem tất cả khóa học & Tag</span>
                  </button>
                )}
                {lastReviewedCardId && (
                  <button
                    type="button"
                    data-testid="undo-review-btn"
                    onClick={handleUndoLastReview}
                    className="px-4 py-2 rounded-xl border border-amber-500/40 bg-amber-500/10 hover:bg-amber-500/20 text-amber-700 dark:text-amber-300 text-xs font-bold cursor-pointer inline-flex items-center gap-1.5 transition"
                  >
                    <RotateCw className="w-3.5 h-3.5" />
                    <span>Hoàn tác thẻ vừa đánh giá (Z)</span>
                  </button>
                )}
                <button
                  type="button"
                  onClick={() => {
                    setSessionCompleted(false);
                    setCurrentIndex(0);
                    setIsFlipped(false);
                  }}
                  className="px-4 py-2 rounded-xl border border-slate-300 dark:border-slate-700 text-xs font-bold cursor-pointer inline-flex items-center gap-1.5 hover:bg-slate-100 dark:hover:bg-slate-800 transition"
                >
                  <RotateCw className="w-3.5 h-3.5" />
                  <span>Ôn tập lại hàng đợi</span>
                </button>
                <button
                  type="button"
                  onClick={() => setActiveMode("dashboard")}
                  className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer inline-flex items-center gap-1.5 transition"
                >
                  <Home className="w-3.5 h-3.5" />
                  <span>Về Không gian cá nhân</span>
                </button>
              </div>
            </div>
          ) : (
            <>
              {/* Review Queue Progress Track & Distraction-Free Header */}
              <div className="w-full max-w-[800px] space-y-2">
                <div className="w-full bg-slate-200 dark:bg-slate-800 h-2 rounded-full overflow-hidden">
                  <div
                    className="bg-indigo-600 h-full rounded-full transition-all duration-300"
                    style={{
                      width: `${cards.length > 0 ? Math.round(((currentIndex + 1) / cards.length) * 100) : 0}%`
                    }}
                  />
                </div>
                <div className="flex items-center justify-between text-xs text-slate-600 dark:text-slate-400">
                  <div className="flex items-center gap-2 font-bold">
                    <span>
                      Tiến độ: {currentIndex + 1} / {cards.length} Thẻ ({cards.length - currentIndex - 1 > 0 ? `${cards.length - currentIndex - 1} thẻ còn lại` : "Thẻ cuối cùng"})
                    </span>
                  </div>
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      data-testid="undo-review-btn"
                      disabled={!lastReviewedCardId}
                      onClick={handleUndoLastReview}
                      className="px-2 py-0.5 rounded-md border border-slate-200 dark:border-slate-700 bg-slate-100 dark:bg-slate-800 hover:bg-indigo-50 dark:hover:bg-indigo-950/60 text-slate-700 dark:text-slate-300 disabled:opacity-40 disabled:cursor-not-allowed transition font-bold cursor-pointer inline-flex items-center gap-1 text-[11px]"
                      title="Hoàn tác đánh giá vừa rồi (Phím Z)"
                    >
                      <RotateCw className="w-3 h-3" />
                      <span>Hoàn tác (Z)</span>
                    </button>
                    <span className="px-2 py-0.5 rounded-md bg-slate-100 dark:bg-slate-800 text-[11px] font-medium text-slate-600 dark:text-slate-400">
                      Độ bền: {currentCard.stability.toFixed(1)}d
                    </span>
                    <span className="px-2 py-0.5 rounded-md bg-slate-100 dark:bg-slate-800 text-[11px] font-medium text-slate-600 dark:text-slate-400">
                      Độ khó: {currentCard.difficulty.toFixed(1)}
                    </span>
                    <button
                      type="button"
                      data-testid="archive-flashcard-btn"
                      onClick={() => handleArchiveCardSoftDelete(currentCard.id)}
                      className="text-slate-400 hover:text-rose-600 dark:hover:text-rose-400 transition font-medium cursor-pointer inline-flex items-center gap-1 text-[11px] ml-1"
                      title="Lưu trữ thẻ ôn tập"
                    >
                      <Archive className="w-3.5 h-3.5" />
                      <span>Lưu trữ</span>
                    </button>
                  </div>
                </div>
              </div>

              {/* 3D Flip Card Canvas (800x520px comfortable dimensions) */}
              <div className="w-full max-w-[800px] min-h-[440px] sm:min-h-[520px] perspective-1200">
                <div
                  data-testid="fsrs-3d-flip-card"
                  onClick={() => setIsFlipped((prev) => !prev)}
                  className={`relative w-full min-h-[440px] sm:min-h-[520px] transition-transform duration-350 ease-out preserve-3d cursor-pointer select-none will-change-transform ${
                    isFlipped ? "rotate-y-180" : ""
                  }`}
                >
                  {/* FRONT FACE */}
                  <div
                    data-testid="flashcard-front-face"
                    className={`absolute inset-0 w-full h-full rounded-3xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xl p-6 sm:p-8 flex flex-col justify-between backface-hidden ${
                      isFlipped ? "pointer-events-none invisible" : "visible"
                    }`}
                  >
                    {/* Card Top Metadata Header */}
                    <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-slate-100 dark:border-slate-800">
                      <div className="flex items-center gap-2 text-xs">
                        <span className="px-2.5 py-0.5 rounded-md bg-indigo-50 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300 font-extrabold uppercase">
                          {currentCard.cardType}
                        </span>
                        <span className="text-slate-500 truncate max-w-[260px] sm:max-w-[400px] inline-flex items-center gap-1.5">
                          <BookOpen className="w-3.5 h-3.5 text-indigo-600 shrink-0" />
                          <span>{currentCard.sourceTitleSnapshot || (currentCard.documentId ? "Tài liệu PDF" : "Bài học")}</span>
                        </span>
                      </div>

                      <div className="flex items-center gap-1.5">
                        {currentCard.tags.map((t) => (
                          <span
                            key={t.id}
                            className="px-2 py-0.5 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 text-[11px] font-bold"
                          >
                            #{t.name}
                          </span>
                        ))}
                      </div>
                    </div>

                    {/* Card Front Content */}
                    <div className="my-auto py-6 space-y-6">
                      <div>
                        <div className="text-[11px] font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400 mb-2">
                          Câu hỏi / Mặt trước (Active Recall Prompt):
                        </div>
                        <div
                          data-testid="flashcard-front-text"
                          className="text-base sm:text-xl font-bold text-slate-900 dark:text-white leading-relaxed"
                        >
                          {renderClozeFormattedText(currentCard.frontText, false)}
                        </div>
                      </div>

                      <div className="pt-8 text-center">
                        <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-slate-100 dark:bg-slate-800 text-xs font-bold text-slate-600 dark:text-slate-300">
                          <RotateCw className="w-3.5 h-3.5 text-indigo-600" />
                          <span>Nhấn Phím Cách (Space) hoặc Click vào thẻ để lật đáp án (180° 3D Flip)</span>
                        </span>
                      </div>
                    </div>

                    {/* Card Footer */}
                    <div className="pt-3 border-t border-slate-100 dark:border-slate-800 flex items-center justify-between text-[11px] text-slate-400">
                      <span>Phím tắt: Space (Lật thẻ) • 1 (Học lại) • 2 (Khó) • 3 (Tốt) • 4 (Dễ)</span>
                      <span>Trích dẫn ngữ cảnh nguyên bản</span>
                    </div>
                  </div>

                  {/* BACK FACE */}
                  <div
                    data-testid="flashcard-back-face"
                    className={`absolute inset-0 w-full h-full rounded-3xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xl p-6 sm:p-8 flex flex-col justify-between backface-hidden rotate-y-180 overflow-hidden ${
                      isFlipped ? "visible" : "pointer-events-none invisible"
                    }`}
                  >
                    {/* Card Top Metadata Header */}
                    <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-slate-100 dark:border-slate-800">
                      <div className="flex items-center gap-2 text-xs">
                        <span className="px-2.5 py-0.5 rounded-md bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 font-extrabold uppercase">
                          Đáp án & Giải thích
                        </span>
                        <span className="text-slate-500 truncate max-w-[260px] sm:max-w-[400px] inline-flex items-center gap-1.5">
                          <BookOpen className="w-3.5 h-3.5 text-indigo-600 shrink-0" />
                          <span>{currentCard.sourceTitleSnapshot || (currentCard.documentId ? "Tài liệu PDF" : "Bài học")}</span>
                        </span>
                      </div>

                      <div className="flex items-center gap-1.5">
                        {currentCard.tags.map((t) => (
                          <span
                            key={t.id}
                            className="px-2 py-0.5 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 text-[11px] font-bold"
                          >
                            #{t.name}
                          </span>
                        ))}
                      </div>
                    </div>

                    {/* Card Back Content (Scrollable inner container without flattening 3D context) */}
                    <div className="my-auto py-4 space-y-4 flex-1 overflow-y-auto pr-1">
                      <div>
                        <div className="text-[11px] font-bold uppercase tracking-wider text-slate-500 dark:text-slate-400 mb-1">
                          Nội dung hoàn chỉnh:
                        </div>
                        <div className="text-sm font-semibold text-slate-700 dark:text-slate-300">
                          {renderClozeFormattedText(currentCard.frontText, true)}
                        </div>
                      </div>

                      <div className="space-y-2">
                        <div className="text-[11px] font-bold uppercase tracking-wider text-emerald-600 dark:text-emerald-400">
                          Giải thích chi tiết (Mặt sau):
                        </div>
                        <p className="text-sm sm:text-base text-slate-800 dark:text-slate-200 leading-relaxed whitespace-pre-line">
                          {currentCard.backText}
                        </p>
                      </div>

                      {currentCard.sourceQuoteSnapshot && (
                        <div className="p-3 rounded-xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800 flex items-center justify-between gap-2 text-xs">
                          <span className="italic text-slate-600 dark:text-slate-400 truncate">
                            Dẫn chứng gốc: &ldquo;{currentCard.sourceQuoteSnapshot}&rdquo;
                          </span>
                          <button
                            type="button"
                            data-testid="jump-to-source-btn"
                            onClick={async (e) => {
                              e.stopPropagation();
                              if (currentCard.lessonId) {
                                setActiveLesson(currentCard.lessonId);
                              } else if (currentCard.documentId) {
                                setActiveDocument(
                                  currentCard.documentId,
                                  currentCard.pageNumber ?? 1
                                );
                              }
                              setActiveMode("split");
                              if (currentCard.annotationId) {
                                await waitForElementAndPulse(`reader-highlight-${currentCard.annotationId}`);
                              }
                            }}
                            className="px-2.5 py-1 rounded-lg bg-indigo-50 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300 font-bold shrink-0 cursor-pointer inline-flex items-center gap-1 hover:bg-indigo-600 hover:text-white transition"
                          >
                            <BookOpen className="w-3.5 h-3.5" />
                            <span>Nhảy đến nguồn</span>
                          </button>
                        </div>
                      )}
                    </div>

                    {/* Card Footer */}
                    <div className="pt-3 border-t border-slate-100 dark:border-slate-800 flex items-center justify-between text-[11px] text-slate-400">
                      <span>Đánh giá mức độ ghi nhớ (1-4 hoặc Space để chọn Tốt)</span>
                      <span>Khoảng cách ngắt quãng FSRS v5</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* 4 FSRS Dynamic Rating Controls (Gated: active only on back face) */}
              <div
                data-testid="fsrs-rating-bar"
                className={`w-full max-w-[800px] transition-all duration-300 ${
                  isFlipped
                    ? "grid grid-cols-2 sm:grid-cols-4 gap-3 opacity-100 visible pointer-events-auto"
                    : "opacity-0 invisible pointer-events-none h-0 overflow-hidden"
                }`}
              >
                <button
                  type="button"
                  data-testid="rate-btn-1"
                  disabled={!isFlipped}
                  onClick={() => handleRateCard(1)}
                  className="p-3 rounded-2xl border border-rose-500/30 bg-rose-500/5 hover:border-rose-500 hover:bg-rose-500 hover:text-white text-rose-700 dark:text-rose-300 transition flex flex-col items-center cursor-pointer disabled:cursor-not-allowed"
                >
                  <span className="text-xs font-bold">[1] Học lại (Again)</span>
                  <span className="text-[11px] opacity-85 mt-0.5">
                    {currentCard.intervalPreviews?.again || "< 10m"}
                  </span>
                </button>

                <button
                  type="button"
                  data-testid="rate-btn-2"
                  disabled={!isFlipped}
                  onClick={() => handleRateCard(2)}
                  className="p-3 rounded-2xl border border-amber-500/30 bg-amber-500/5 hover:border-amber-500 hover:bg-amber-500 hover:text-white text-amber-700 dark:text-amber-300 transition flex flex-col items-center cursor-pointer disabled:cursor-not-allowed"
                >
                  <span className="text-xs font-bold">[2] Khó (Hard)</span>
                  <span className="text-[11px] opacity-85 mt-0.5">
                    {currentCard.intervalPreviews?.hard || "1.2d"}
                  </span>
                </button>

                <button
                  type="button"
                  data-testid="rate-btn-3"
                  disabled={!isFlipped}
                  onClick={() => handleRateCard(3)}
                  className="p-3 rounded-2xl border border-emerald-500/30 bg-emerald-500/5 hover:border-emerald-600 hover:bg-emerald-600 hover:text-white text-emerald-700 dark:text-emerald-300 transition flex flex-col items-center cursor-pointer disabled:cursor-not-allowed"
                >
                  <span className="text-xs font-bold">[3] Tốt (Good)</span>
                  <span className="text-[11px] opacity-85 mt-0.5">
                    {currentCard.intervalPreviews?.good || "3.5d"}
                  </span>
                </button>

                <button
                  type="button"
                  data-testid="rate-btn-4"
                  disabled={!isFlipped}
                  onClick={() => handleRateCard(4)}
                  className="p-3 rounded-2xl border border-sky-500/30 bg-sky-500/5 hover:border-sky-500 hover:bg-sky-500 hover:text-white text-sky-700 dark:text-sky-300 transition flex flex-col items-center cursor-pointer disabled:cursor-not-allowed"
                >
                  <span className="text-xs font-bold">[4] Dễ (Easy)</span>
                  <span className="text-[11px] opacity-85 mt-0.5">
                    {currentCard.intervalPreviews?.easy || "7.0d"}
                  </span>
                </button>
              </div>

              {!isFlipped && (
                <div
                  data-testid="fsrs-rating-bar-placeholder"
                  className="w-full max-w-[800px] text-center py-2 text-xs text-slate-400 dark:text-slate-500"
                >
                  <span>Nhấn Phím Cách (Space) hoặc bấm vào thẻ để lật xem đáp án trước khi đánh giá</span>
                </div>
              )}
            </>
          )}
        </div>
      )}

      {/* ================================================================== */}
      {/* SUB-TAB 2: FORMATIVE PRACTICE QUIZ PORTAL (ADR-021 & AI RUBRIC)    */}
      {/* ================================================================== */}
      {activeReviewSubTab === "quiz" && quiz && (
        <div
          data-testid="practice-quiz-portal"
          className="space-y-5 max-w-4xl mx-auto"
        >
          {/* Course Quiz Selector */}
          <div
            data-testid="quiz-course-selector"
            className="rounded-2xl bg-indigo-50/70 dark:bg-indigo-950/30 border border-indigo-500/30 p-4 space-y-3"
          >
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2.5">
              <div className="flex flex-wrap items-center gap-2">
                <label
                  htmlFor="quiz-course-filter-select"
                  className="text-xs font-extrabold text-indigo-800 dark:text-indigo-200 inline-flex items-center gap-1.5"
                >
                  <Target className="w-3.5 h-3.5 text-indigo-600 dark:text-indigo-400" />
                  <span>Khóa học:</span>
                </label>
                <select
                  id="quiz-course-filter-select"
                  data-testid="quiz-course-filter-select"
                  value={selectedCourseFilterId}
                  onChange={(e) => {
                    const nextCourseId = e.target.value;
                    setSelectedCourseFilterId(nextCourseId);
                    setActiveCourseId(nextCourseId === "all" ? null : nextCourseId);
                    if (nextCourseId !== "all") {
                      const nextCourse = effectiveCourses.find(
                        (c) => (c.courseId || c.id) === nextCourseId
                      );
                    const matched = effectiveQuizList.find(
                      (q) =>
                        q.courseId === nextCourseId ||
                        (nextCourse?.quizId && q.id === nextCourse.quizId) ||
                        (nextCourse?.lessonQuizMap &&
                          Object.values(nextCourse.lessonQuizMap).includes(q.id)) ||
                        (nextCourse?.sourceDocumentId &&
                          (q.sourceDocumentId === nextCourse.sourceDocumentId ||
                            q.documentId === nextCourse.sourceDocumentId))
                    );
                    const fallbackId =
                      matched?.id ||
                      nextCourse?.quizId ||
                      (nextCourse?.lessonQuizMap
                        ? Object.values(nextCourse.lessonQuizMap)[0]
                        : undefined);
                    if (fallbackId) {
                      setActiveQuizId(fallbackId);
                      loadPracticeQuiz(fallbackId);
                    } else {
                      setActiveQuizId(null);
                    }
                  }
                }}
                className="px-2.5 py-1.5 rounded-xl border border-indigo-300 dark:border-indigo-700 bg-white dark:bg-slate-900 text-xs font-bold text-slate-800 dark:text-slate-100 max-w-[260px] cursor-pointer"
              >
                <option value="all">
                  Tất cả khóa học ({effectiveQuizList.length} đề)
                </option>
                {effectiveCourses.map((c) => {
                  const cid = c.courseId || c.id || "";
                  if (!cid) return null;
                  return (
                    <option key={cid} value={cid}>
                      {c.title}
                    </option>
                  );
                })}
              </select>
            </div>

            {(() => {
              const displayedQuizList =
                filteredQuizList.length > 0 ? filteredQuizList : effectiveQuizList;
              return (
                <div className="flex flex-wrap items-center gap-2 min-w-0">
                  <label
                    htmlFor="quiz-selector-select"
                    className="text-xs font-bold text-slate-600 dark:text-slate-300 shrink-0"
                  >
                    Đề luyện tập ({displayedQuizList.length}):
                  </label>
                  <select
                    id="quiz-selector-select"
                    data-testid="quiz-selector-select"
                    value={quiz.id || resolvedQuizId}
                    onChange={(e) => {
                      const nextId = e.target.value;
                      setActiveQuizId(nextId);
                      loadPracticeQuiz(nextId);
                    }}
                    className="px-3 py-1.5 rounded-xl border border-indigo-300 dark:border-indigo-700 bg-white dark:bg-slate-900 text-xs font-bold text-slate-800 dark:text-slate-100 cursor-pointer max-w-full sm:max-w-[340px]"
                  >
                    {displayedQuizList.map((qItem) => (
                      <option key={qItem.id} value={qItem.id}>
                        {qItem.title}
                      </option>
                    ))}
                  </select>
                </div>
              );
            })()}
          </div>

          {!loadingQuizList &&
            selectedCourseFilterId !== "all" &&
            filteredQuizList.length === 0 &&
            !(
              quiz &&
              quiz.id !== DEFAULT_PRACTICE_QUIZ_ID &&
              (quiz.courseId === selectedCourseFilterId ||
                selectedCourseObj?.quizId === quiz.id)
            ) && (
              <div className="flex flex-wrap items-center justify-between gap-2 rounded-xl bg-amber-50 dark:bg-amber-950/40 border border-amber-300/60 px-3 py-2 text-[11px] text-amber-800 dark:text-amber-200">
                <span>Khóa học này chưa có đề Quiz riêng — đang hiển thị danh sách tổng hợp.</span>
                <button
                  type="button"
                  onClick={() => {
                    setSelectedCourseFilterId("all");
                    setActiveCourseId(null);
                  }}
                  className="font-bold underline cursor-pointer"
                >
                  Xem tất cả đề
                </button>
              </div>
            )}

            {filteredQuizList.length > 1 && filteredQuizList.length <= 12 && (
              <div className="flex flex-wrap items-center gap-1.5">
                {filteredQuizList.map((qItem) => {
                  const isActiveQuiz = (quiz.id || resolvedQuizId) === qItem.id;
                  const isCurrentLessonQuiz = Boolean(
                    activeLessonId && qItem.lessonId === activeLessonId
                  );
                  return (
                    <button
                      key={`pill-${qItem.id}`}
                      type="button"
                      onClick={() => {
                        setActiveQuizId(qItem.id);
                        loadPracticeQuiz(qItem.id);
                      }}
                      className={`px-2.5 py-1 rounded-lg text-[11px] font-bold cursor-pointer transition inline-flex items-center gap-1 ${
                        isActiveQuiz
                          ? "bg-indigo-600 text-white shadow-2xs"
                          : "bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-700 dark:text-slate-300 hover:border-indigo-500"
                      }`}
                    >
                      {isCurrentLessonQuiz && (
                        <span className="text-[10px] opacity-80">★</span>
                      )}
                      <span className="truncate max-w-[260px]">{qItem.title}</span>
                    </button>
                  );
                })}
              </div>
            )}
          </div>

          <div className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5 flex items-center justify-between">
            <div>
              <span className="text-[11px] font-extrabold uppercase text-indigo-600 dark:text-indigo-400">
                Đánh giá Quá trình (Formative Practice Quiz • Không tính điểm GPA áp lực theo ADR-021)
              </span>
              <h2 className="text-lg font-extrabold text-slate-900 dark:text-white mt-0.5">
                {quiz.title}
              </h2>
            </div>
            <span className="px-3 py-1 rounded-full bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 text-xs font-extrabold">
              Đã hoàn thành: {quiz.correctItems}/{quiz.totalItems} mục
            </span>
          </div>

          {quizSubmitError && (
            <div className="rounded-xl border border-rose-300 dark:border-rose-800 bg-rose-50 dark:bg-rose-950/40 px-4 py-2.5 text-xs font-semibold text-rose-700 dark:text-rose-300">
              {quizSubmitError}
            </div>
          )}

          {quiz.items.map((item, idx) => {
            const isItemEvaluated = item.isCorrect !== null && item.isCorrect !== undefined;
            const currentAns =
              practiceQuizDraftAnswers[item.id] ??
              item.userAnswer?.text ??
              (item.itemType === "essay" && quiz.id === DEFAULT_PRACTICE_QUIZ_ID
                ? "Bảng trang đa cấp giúp tiết kiệm bộ nhớ RAM vì không cần cấp phát các vùng địa chỉ trống như bảng trang phẳng. Tuy nhiên khi xảy ra TLB Miss, phần cứng MMU phải đọc RAM 4 lần liên tiếp."
                : "");

            return (
              <div
                key={item.id}
                data-testid={`quiz-item-${item.itemType}`}
                className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5 space-y-4 shadow-xs"
              >
                <div className="flex items-center justify-between">
                  <span className="text-xs font-extrabold uppercase text-indigo-600 dark:text-indigo-400">
                    Câu {idx + 1} • Loại: {item.itemType.toUpperCase()}
                  </span>
                  {typeof item.isCorrect === "boolean" && (
                    <span
                      className={`px-2.5 py-0.5 rounded-full text-xs font-bold ${
                        item.isCorrect
                          ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                          : "bg-rose-500/15 text-rose-700 dark:text-rose-300"
                      }`}
                    >
                      {item.isCorrect ? (
                        <span className="inline-flex items-center gap-1">
                          <CheckCircle2 className="w-3.5 h-3.5" />
                          <span>Chính xác / Đã đánh giá</span>
                        </span>
                      ) : (
                        "Cần xem lại"
                      )}
                    </span>
                  )}
                </div>

                <p className="text-sm sm:text-base font-bold text-slate-900 dark:text-white leading-relaxed">
                  {item.prompt.question}
                </p>

                {item.itemType === "multiple_choice" && item.choices && (
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
                    {item.choices.map((ch) => (
                      <button
                        key={ch.key}
                        type="button"
                        disabled={submittingItemId === item.id || isItemEvaluated}
                        onClick={() => {
                          setPracticeQuizDraftAnswer(item.id, ch.key);
                          handleSubmitQuizItem(item.id, ch.key);
                        }}
                        className={`p-3 rounded-xl border text-left text-xs font-bold transition cursor-pointer disabled:opacity-60 disabled:cursor-not-allowed ${
                          currentAns === ch.key
                            ? "border-indigo-600 bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300"
                            : "border-slate-200 dark:border-slate-800 hover:bg-slate-50 dark:hover:bg-slate-800"
                        }`}
                      >
                        [{ch.key}] {ch.text}
                      </button>
                    ))}
                  </div>
                )}

                {item.itemType === "cloze" && (
                  <div className="flex items-center gap-2">
                    <input
                      type="text"
                      data-testid="cloze-answer-input"
                      value={currentAns}
                      onChange={(e) => setPracticeQuizDraftAnswer(item.id, e.target.value)}
                      placeholder="Nhập thuật ngữ viết tắt (VD: TLB)..."
                      className="flex-1 px-3 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-transparent text-xs font-bold"
                    />
                    <button
                      type="button"
                      data-testid="submit-cloze-btn"
                      disabled={submittingItemId === item.id || isItemEvaluated}
                      onClick={() => handleSubmitQuizItem(item.id, currentAns || "TLB")}
                      className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed text-white text-xs font-bold cursor-pointer"
                    >
                      {isItemEvaluated ? "Đã chấm" : "Kiểm tra Cloze"}
                    </button>
                  </div>
                )}

                {item.itemType === "short_answer" && (
                  <div className="flex items-center gap-2">
                    <input
                      type="text"
                      data-testid="short-answer-input"
                      value={currentAns}
                      onChange={(e) => setPracticeQuizDraftAnswer(item.id, e.target.value)}
                      placeholder="Nhập câu trả lời ngắn..."
                      className="flex-1 px-3 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-transparent text-xs font-bold"
                    />
                    <button
                      type="button"
                      data-testid="submit-short-answer-btn"
                      disabled={submittingItemId === item.id || isItemEvaluated || !currentAns.trim()}
                      onClick={() => handleSubmitQuizItem(item.id, currentAns)}
                      className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed text-white text-xs font-bold cursor-pointer"
                    >
                      {isItemEvaluated ? "Đã chấm" : "Gửi câu trả lời"}
                    </button>
                  </div>
                )}

                {item.itemType === "essay" && (
                  <div className="space-y-3">
                    <textarea
                      data-testid="essay-answer-textarea"
                      rows={4}
                      value={currentAns}
                      onChange={(e) => setPracticeQuizDraftAnswer(item.id, e.target.value)}
                      onKeyDown={(e) => {
                        if ((e.ctrlKey || e.metaKey) && e.key === "Enter") {
                          e.preventDefault();
                          handleSubmitQuizItem(item.id, currentAns);
                        }
                      }}
                      className="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-slate-50 dark:bg-slate-950 p-3 text-xs sm:text-sm"
                    />
                    <div className="flex justify-end">
                      <button
                        type="button"
                        data-testid="submit-essay-ai-btn"
                        disabled={submittingItemId === item.id || !currentAns.trim()}
                        onClick={() => handleSubmitQuizItem(item.id, currentAns)}
                        className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed text-white text-xs font-bold cursor-pointer inline-flex items-center gap-1.5 transition"
                      >
                        {submittingItemId === item.id ? (
                          <>
                            <Sparkles className="w-3.5 h-3.5 animate-spin" />
                            <span>Đang chấm qua .NET 10 Channel & gemini-3.8-flash...</span>
                          </>
                        ) : (
                          <>
                            <Sparkles className="w-3.5 h-3.5" />
                            <span>
                              {isItemEvaluated
                                ? "Gửi chấm lại AI (2 Credits)"
                                : "Gửi chấm nhận xét ngữ nghĩa AI (2 Credits)"}
                            </span>
                          </>
                        )}
                      </button>
                    </div>

                    {item.aiRubricFeedback && (
                      <div
                        data-testid="essay-ai-rubric-feedback"
                        className="rounded-2xl border border-emerald-500/40 bg-emerald-50/60 dark:bg-emerald-950/30 p-4 space-y-2.5 text-xs"
                      >
                        <div className="flex items-center justify-between font-bold text-emerald-800 dark:text-emerald-300">
                          <span className="inline-flex items-center gap-1.5">
                            <Sparkles className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
                            <span>Nhận xét định tính từ AI Rubric (gemini-3.8-flash)</span>
                          </span>
                          <span className="px-2 py-0.5 rounded bg-emerald-600 text-white font-semibold">
                            Mức độ: {item.aiRubricFeedback.masteryLevel}
                          </span>
                        </div>
                        <p className="text-slate-800 dark:text-slate-200 font-medium">
                          {item.aiRubricFeedback.summary}
                        </p>
                        <div>
                          <div className="inline-flex items-center gap-1 font-bold text-emerald-700 dark:text-emerald-400">
                            <CheckCircle2 className="w-3.5 h-3.5" />
                            <span>Điểm mạnh trong lập luận:</span>
                          </div>
                          <ul className="list-disc list-inside text-slate-700 dark:text-slate-300 mt-1">
                            {(item.aiRubricFeedback.strengths || []).map((s, i) => (
                              <li key={i}>{s}</li>
                            ))}
                          </ul>
                        </div>
                        <div>
                          <div className="inline-flex items-center gap-1 font-bold text-amber-700 dark:text-amber-400">
                            <Lightbulb className="w-3.5 h-3.5" />
                            <span>Khái niệm gợi ý bổ sung:</span>
                          </div>
                          <ul className="list-disc list-inside text-slate-700 dark:text-slate-300 mt-1">
                            {(item.aiRubricFeedback.missingConcepts || []).map((m, i) => (
                              <li key={i}>{m}</li>
                            ))}
                          </ul>
                        </div>
                        {item.aiRubricFeedback.modelComparison && (
                          <div
                            data-testid="essay-rubric-model-comparison"
                            className="p-3.5 rounded-xl bg-white/80 dark:bg-slate-900/80 border border-emerald-500/30 text-xs space-y-1 mt-2"
                          >
                            <div className="font-bold text-emerald-800 dark:text-emerald-300 inline-flex items-center gap-1.5">
                              <BookOpen className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400" />
                              <span>Đối chiếu đáp án mẫu (Model Comparison):</span>
                            </div>
                            <p className="text-slate-700 dark:text-slate-300 leading-relaxed italic">
                              &ldquo;{item.aiRubricFeedback.modelComparison}&rdquo;
                            </p>
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                )}
              </div>
            );
          })}

          {/* Practice Quiz Completion Card (Resolves UX-04) */}
          {(quiz.correctItems >= quiz.totalItems || quiz.items.every((it) => it.isCorrect || it.aiRubricFeedback)) && (
            <div
              data-testid="quiz-completion-summary-card"
              className="rounded-2xl border border-emerald-500/50 bg-emerald-50/70 dark:bg-emerald-950/40 p-6 text-center space-y-4 shadow-sm"
            >
              <div className="w-12 h-12 rounded-2xl bg-emerald-500 text-white flex items-center justify-center mx-auto shadow-md">
                <Trophy className="w-6 h-6" />
              </div>
              <div className="space-y-1">
                <h3 className="text-base sm:text-lg font-bold text-slate-900 dark:text-white">
                  Tuyệt vời! Bạn đã hoàn thành toàn bộ bài luyện tập thực hành!
                </h3>
                <p className="text-xs text-slate-600 dark:text-slate-400 max-w-md mx-auto">
                  Bạn đã hoàn thành các câu hỏi trong &ldquo;{quiz.title}&rdquo;. Hãy tiếp tục ôn tập ngắt quãng hoặc quay về Không gian cá nhân.
                </p>
              </div>

              <div className="flex flex-wrap items-center justify-center gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setActiveReviewSubTab("fsrs")}
                  className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold transition cursor-pointer inline-flex items-center gap-1.5"
                >
                  <Brain className="w-3.5 h-3.5" />
                  <span>Quay về Hàng đợi Ôn tập FSRS</span>
                </button>
                <button
                  type="button"
                  onClick={() => setActiveMode("dashboard")}
                  className="px-4 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-slate-100 dark:hover:bg-slate-800 text-xs font-bold transition cursor-pointer inline-flex items-center gap-1.5"
                >
                  <LayoutDashboard className="w-3.5 h-3.5" />
                  <span>Về Không gian cá nhân</span>
                </button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
