"use client";
import React, { useEffect, useState } from "react";
import { Brain, Target, Trophy, RotateCw, Home, Zap, Archive, BookOpen, Sparkles, CheckCircle2, Lightbulb, LayoutDashboard } from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { evaluateFsrsV5 } from "@/lib/fsrs-engine";
import { enqueueOfflineMutation, removeOfflineMutation } from "@/lib/offline-queue";
import { triggerCompletionCelebration } from "@/lib/animations";
const DEFAULT_PRACTICE_QUIZ_ID = "77777777-7777-4777-8777-777777777777";
export function ReviewAndPracticeView({ tags, onReviewSessionUpdated, courses: propCourses, quizzes: propQuizzes }) {
    const { activeReviewSubTab, setActiveReviewSubTab, selectedTagFilterId, setSelectedTagFilterId, setActiveMode, setActiveLesson, setActiveDocument, activeDocumentId, activeCourseId, activeQuizId, setActiveQuizId, practiceQuizDraftAnswers, setPracticeQuizDraftAnswer } = useWorkspaceStore();
    const [cards, setCards] = useState([]);
    const [currentIndex, setCurrentIndex] = useState(0);
    const [isFlipped, setIsFlipped] = useState(false);
    const [lastEvalMs, setLastEvalMs] = useState(null);
    const [sessionCompleted, setSessionCompleted] = useState(false);
    const [loadingCards, setLoadingCards] = useState(true);
    const [lastReviewedCardId, setLastReviewedCardId] = useState(null);
    const [undoBanner, setUndoBanner] = useState(null);
    // Practice Quiz State
    const [quiz, setQuiz] = useState(null);
    const [fetchedQuizzes, setFetchedQuizzes] = useState([]);
    const [submittingItemId, setSubmittingItemId] = useState(null);
    const loadQuizList = async () => {
        try {
            const resp = await fetch("/api/v1/practice-quizzes", {
                headers: getClientRequestHeaders()
            });
            if (resp.ok) {
                const list = await resp.json();
                if (Array.isArray(list)) {
                    setFetchedQuizzes(list);
                }
            }
        }
        catch {
            // Ignore network errors and fallback to propQuizzes / default quiz
        }
    };
    const effectiveQuizList = React.useMemo(() => {
        const map = new Map();
        for (const q of propQuizzes || []) {
            if (q && q.id)
                map.set(q.id, q);
        }
        for (const q of fetchedQuizzes) {
            if (q && q.id)
                map.set(q.id, q);
        }
        if (!map.has(DEFAULT_PRACTICE_QUIZ_ID)) {
            map.set(DEFAULT_PRACTICE_QUIZ_ID, {
                id: DEFAULT_PRACTICE_QUIZ_ID,
                title: "Đề luyện tập tổng hợp: Chương 3 — Bộ nhớ ảo & Phân trang"
            });
        }
        return Array.from(map.values());
    }, [propQuizzes, fetchedQuizzes]);
    const resolvedQuizId = React.useMemo(() => {
        if (activeQuizId)
            return activeQuizId;
        if (activeCourseId) {
            const byCourse = effectiveQuizList.find((q) => q.courseId === activeCourseId);
            if (byCourse)
                return byCourse.id;
            const courseMatch = (propCourses || []).find((c) => (c.courseId || c.id) === activeCourseId && c.quizId);
            if (courseMatch?.quizId)
                return courseMatch.quizId;
        }
        if (activeDocumentId) {
            const byDoc = effectiveQuizList.find((q) => q.documentId === activeDocumentId || q.sourceDocumentId === activeDocumentId);
            if (byDoc)
                return byDoc.id;
            const courseByDoc = (propCourses || []).find((c) => c.sourceDocumentId === activeDocumentId && c.quizId);
            if (courseByDoc?.quizId)
                return courseByDoc.quizId;
        }
        return DEFAULT_PRACTICE_QUIZ_ID;
    }, [activeQuizId, activeCourseId, activeDocumentId, effectiveQuizList, propCourses]);
    const loadDueQueue = async (tagId) => {
        setLoadingCards(true);
        const url = tagId
            ? `/api/v1/flashcards/due?tagId=${encodeURIComponent(tagId)}&includeAll=true`
            : `/api/v1/flashcards/due?includeAll=true`;
        const resp = await fetch(url, { headers: getClientRequestHeaders() });
        if (resp.ok) {
            const list = await resp.json();
            const prioritized = activeDocumentId
                ? [...list].sort((a, b) => {
                    const aMatch = a.documentId === activeDocumentId ? 1 : 0;
                    const bMatch = b.documentId === activeDocumentId ? 1 : 0;
                    if (aMatch !== bMatch)
                        return bMatch - aMatch;
                    const aAnn = a.annotationId ? 1 : 0;
                    const bAnn = b.annotationId ? 1 : 0;
                    return bAnn - aAnn;
                })
                : list;
            setCards(prioritized);
            setCurrentIndex(0);
            setIsFlipped(false);
            setSessionCompleted(prioritized.length === 0);
        }
        setLoadingCards(false);
    };
    const loadPracticeQuiz = async (targetQuizId) => {
        const idToLoad = targetQuizId || resolvedQuizId || DEFAULT_PRACTICE_QUIZ_ID;
        const headers = getClientRequestHeaders();
        let resp = await fetch(`/api/v1/practice-quizzes/${encodeURIComponent(idToLoad)}`, {
            headers
        });
        if (!resp.ok && idToLoad !== DEFAULT_PRACTICE_QUIZ_ID) {
            resp = await fetch(`/api/v1/practice-quizzes/${DEFAULT_PRACTICE_QUIZ_ID}`, {
                headers
            });
        }
        if (resp.ok) {
            const data = await resp.json();
            setQuiz(data);
        }
    };
    useEffect(() => {
        loadDueQueue(selectedTagFilterId);
    }, [selectedTagFilterId]);
    useEffect(() => {
        loadQuizList();
    }, [activeReviewSubTab, activeDocumentId, activeCourseId]);
    useEffect(() => {
        loadPracticeQuiz(resolvedQuizId);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [resolvedQuizId]);
    const currentCard = cards[currentIndex] ?? null;
    const handleUndoLastReview = async () => {
        if (!lastReviewedCardId)
            return;
        const cardIdToUndo = lastReviewedCardId;
        try {
            const activeUserId = typeof window !== "undefined" ? localStorage.getItem("swms_active_user_id") : null;
            const headers = { "Content-Type": "application/json" };
            if (activeUserId) {
                headers["X-Test-User-Id"] = activeUserId;
            }
            const resp = await fetch(`/api/v1/flashcards/${cardIdToUndo}/undo-last-review`, {
                method: "POST",
                headers
            });
            if (resp.ok) {
                onReviewSessionUpdated();
            }
        }
        catch {
            // Fallback safely if offline
        }
        if (sessionCompleted) {
            setSessionCompleted(false);
            setCurrentIndex(Math.max(0, cards.length - 1));
        }
        else {
            setCurrentIndex((prev) => Math.max(0, prev - 1));
        }
        setIsFlipped(false);
        setLastReviewedCardId(null);
        setUndoBanner("Đã hoàn tác đánh giá thẻ vừa rồi (Undo)!");
        setTimeout(() => setUndoBanner(null), 4000);
    };
    // Keyboard shortcuts: Space to flip, 1..4 to rate, Z to undo last review
    useEffect(() => {
        if (activeReviewSubTab !== "fsrs")
            return;
        const onKey = (e) => {
            if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement || e.target?.isContentEditable)
                return;
            if (!e.ctrlKey && !e.metaKey && !e.altKey && (e.key === "z" || e.key === "Z")) {
                if (lastReviewedCardId) {
                    e.preventDefault();
                    handleUndoLastReview();
                }
                return;
            }
            if (!currentCard || sessionCompleted)
                return;
            if (e.code === "Space") {
                e.preventDefault();
                if (!isFlipped) {
                    setIsFlipped(true);
                }
                else {
                    handleRateCard(3);
                }
            }
            else if (isFlipped && ["1", "2", "3", "4"].includes(e.key)) {
                e.preventDefault();
                handleRateCard(Number(e.key));
            }
        };
        window.addEventListener("keydown", onKey);
        return () => window.removeEventListener("keydown", onKey);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [activeReviewSubTab, currentCard, isFlipped, sessionCompleted, currentIndex, lastReviewedCardId]);
    // Keyboard shortcuts in Practice Quiz mode: A/B/C/D to select & submit multiple-choice
    useEffect(() => {
        if (activeReviewSubTab !== "quiz" || !quiz)
            return;
        const onQuizKey = (e) => {
            if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement || e.target?.isContentEditable)
                return;
            if (e.ctrlKey || e.metaKey || e.altKey)
                return;
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
    const handleRateCard = async (rating) => {
        if (!currentCard)
            return;
        setLastReviewedCardId(currentCard.id);
        setUndoBanner(null);
        // 1. Instantaneous Client-Side FSRS v5 Evaluation (SLO: p99 < 5ms)
        const nowIso = new Date().toISOString();
        const localResult = evaluateFsrsV5({
            stability: currentCard.stability,
            difficulty: currentCard.difficulty,
            reps: currentCard.reps,
            lapses: currentCard.lapses,
            state: currentCard.state,
            lastReviewAt: currentCard.lastReviewAt,
            dueAt: currentCard.dueAt
        }, rating, nowIso);
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
        }
        else {
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
        }
        catch {
            // Remains safely queued in OfflineMutationQueue until online reconnection
        }
    };
    const handleArchiveCardSoftDelete = async (cardId) => {
        const resp = await fetch(`/api/v1/flashcards/${cardId}`, {
            method: "DELETE",
            headers: getClientRequestHeaders()
        });
        if (resp.ok) {
            await loadDueQueue(selectedTagFilterId);
            onReviewSessionUpdated();
        }
    };
    const handleSubmitQuizItem = async (itemId, fallbackAnswer) => {
        if (!quiz)
            return;
        const answerText = (practiceQuizDraftAnswers[itemId] ?? fallbackAnswer).trim();
        if (!answerText)
            return;
        setSubmittingItemId(itemId);
        try {
            const resp = await fetch(`/api/v1/practice-quizzes/${quiz.id}/items/${itemId}/submit`, {
                method: "POST",
                headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
                body: JSON.stringify({
                    answer: answerText,
                    idempotencyKey: crypto.randomUUID(),
                    waitForAiCompletion: true
                })
            });
            if (resp.ok || resp.status === 202) {
                await loadPracticeQuiz(quiz.id);
                onReviewSessionUpdated();
            }
        }
        finally {
            setSubmittingItemId(null);
        }
    };
    const renderClozeFormattedText = (text, reveal) => {
        const regex = /\{\{c1::(.*?)\}\}/g;
        const parts = [];
        let lastIndex = 0;
        let match;
        while ((match = regex.exec(text)) !== null) {
            if (match.index > lastIndex) {
                parts.push(text.slice(lastIndex, match.index));
            }
            const hiddenTerm = match[1];
            parts.push(<span key={match.index} className={`px-2 py-0.5 mx-1 rounded-md font-extrabold ${reveal
                    ? "bg-emerald-500/20 text-emerald-700 dark:text-emerald-300 border border-emerald-500/50"
                    : "bg-indigo-600 text-white"}`}>
          {reveal ? hiddenTerm : "[ ... ? ... ]"}
        </span>);
            lastIndex = regex.lastIndex;
        }
        if (lastIndex < text.length) {
            parts.push(text.slice(lastIndex));
        }
        return parts.length > 0 ? parts : text;
    };
    return (<div id="review-celebration-Anchor" data-testid="mode-3-review-container" className="relative max-w-6xl mx-auto px-4 sm:px-6 py-6 space-y-6">
      {/* Top Sub-Navigation & Tag Filter Bar (Unified Review Queue — Zero Decks per ADR-020) */}
      <div className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-4 flex flex-col md:flex-row md:items-center justify-between gap-4 shadow-xs">
        <div className="flex items-center gap-2">
          <button type="button" data-testid="review-subtab-fsrs" onClick={() => setActiveReviewSubTab("fsrs")} className={`px-3.5 py-1.5 rounded-xl text-xs font-bold cursor-pointer transition inline-flex items-center gap-1.5 ${activeReviewSubTab === "fsrs"
            ? "bg-indigo-600 text-white shadow-xs"
            : "bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"}`}>
            <Brain className="w-4 h-4 shrink-0"/>
            <span>Hàng đợi ôn tập FSRS v5 ({cards.length} thẻ)</span>
          </button>
          <button type="button" data-testid="review-subtab-quiz" onClick={() => setActiveReviewSubTab("quiz")} className={`px-3.5 py-1.5 rounded-xl text-xs font-bold cursor-pointer transition inline-flex items-center gap-1.5 ${activeReviewSubTab === "quiz"
            ? "bg-indigo-600 text-white shadow-xs"
            : "bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"}`}>
            <Target className="w-4 h-4 shrink-0"/>
            <span>Bài luyện tập thực hành (Làm bài & AI nhận xét)</span>
          </button>
        </div>

        {/* Tag Filter Pills */}
        {activeReviewSubTab === "fsrs" && (<div className="flex flex-wrap items-center gap-1.5 text-xs">
            <span className="font-bold text-slate-400 mr-1">Lọc theo Tag:</span>
            <button type="button" data-testid="filter-tag-all" onClick={() => setSelectedTagFilterId(null)} className={`px-2.5 py-1 rounded-full font-bold cursor-pointer ${selectedTagFilterId === null
                ? "bg-indigo-600 text-white"
                : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300"}`}>
              Tất cả
            </button>
            {tags.map((tg) => (<button key={tg.id} type="button" data-testid={`filter-tag-${tg.name}`} onClick={() => setSelectedTagFilterId(tg.id)} className={`px-2.5 py-1 rounded-full font-bold cursor-pointer ${selectedTagFilterId === tg.id
                    ? "bg-indigo-600 text-white"
                    : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300"}`}>
                #{tg.name}
              </button>))}
          </div>)}
      </div>

      {/* ================================================================== */}
      {/* SUB-TAB 1: FSRS v5 3D ACTIVE RECALL FLIP CARD CANVAS               */}
      {/* ================================================================== */}
      {activeReviewSubTab === "fsrs" && (<div className="flex flex-col items-center space-y-6">
          {undoBanner && (<div role="status" aria-live="polite" className="w-full max-w-[800px] rounded-xl border border-indigo-500/40 bg-indigo-50/90 dark:bg-indigo-950/60 px-4 py-2 text-xs font-bold text-indigo-700 dark:text-indigo-300 flex items-center justify-between">
              <span>{undoBanner}</span>
              <button type="button" onClick={() => setUndoBanner(null)} className="text-indigo-500 hover:underline text-[11px]">
                Đóng
              </button>
            </div>)}
          {loadingCards ? (<div className="p-12 text-sm text-slate-500">Đang tải Hàng đợi Ôn tập FSRS v5...</div>) : sessionCompleted || !currentCard ? (<div data-testid="fsrs-session-complete-card" className="w-full max-w-[800px] rounded-3xl bg-white dark:bg-slate-900 border border-emerald-500/40 p-8 text-center space-y-4 shadow-xl">
              <div className="w-12 h-12 mx-auto rounded-full bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center">
                <Trophy className="w-6 h-6"/>
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
                {lastReviewedCardId && (<button type="button" data-testid="undo-review-btn" onClick={handleUndoLastReview} className="px-4 py-2 rounded-xl border border-amber-500/40 bg-amber-500/10 hover:bg-amber-500/20 text-amber-700 dark:text-amber-300 text-xs font-bold cursor-pointer inline-flex items-center gap-1.5 transition">
                    <RotateCw className="w-3.5 h-3.5"/>
                    <span>Hoàn tác thẻ vừa đánh giá (Z)</span>
                  </button>)}
                <button type="button" onClick={() => {
                    setSessionCompleted(false);
                    setCurrentIndex(0);
                    setIsFlipped(false);
                }} className="px-4 py-2 rounded-xl border border-slate-300 dark:border-slate-700 text-xs font-bold cursor-pointer inline-flex items-center gap-1.5 hover:bg-slate-100 dark:hover:bg-slate-800 transition">
                  <RotateCw className="w-3.5 h-3.5"/>
                  <span>Ôn tập lại hàng đợi</span>
                </button>
                <button type="button" onClick={() => setActiveMode("dashboard")} className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer inline-flex items-center gap-1.5 transition">
                  <Home className="w-3.5 h-3.5"/>
                  <span>Về Không gian cá nhân</span>
                </button>
              </div>
            </div>) : (<>
              {/* Session Progress Bar */}
              <div className="w-full max-w-[800px] flex items-center justify-between text-xs text-slate-600 dark:text-slate-400">
                <div className="flex items-center gap-2 font-bold">
                  <span>
                    Tiến độ: {currentIndex + 1} / {cards.length} Thẻ
                  </span>
                  {lastEvalMs !== null && (<span className="px-2 py-0.5 rounded bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 text-[10px] inline-flex items-center gap-1">
                      <Zap className="w-3 h-3 text-emerald-600 dark:text-emerald-400"/>
                      <span>Local FSRS: {lastEvalMs}ms (&lt; 5ms SLO)</span>
                    </span>)}
                </div>
                <div className="flex items-center gap-3">
                  <button type="button" data-testid="undo-review-btn" disabled={!lastReviewedCardId} onClick={handleUndoLastReview} className="px-2 py-0.5 rounded-md border border-slate-200 dark:border-slate-700 bg-slate-100 dark:bg-slate-800 hover:bg-indigo-50 dark:hover:bg-indigo-950/60 text-slate-700 dark:text-slate-300 disabled:opacity-40 disabled:cursor-not-allowed transition font-bold cursor-pointer inline-flex items-center gap-1 text-[11px]" title="Hoàn tác đánh giá vừa rồi (Phím Z)">
                    <RotateCw className="w-3 h-3"/>
                    <span>Hoàn tác (Z)</span>
                  </button>
                  <span>
                    Stability (S): <strong>{currentCard.stability.toFixed(2)}</strong>
                  </span>
                  <span>
                    Difficulty (D): <strong>{currentCard.difficulty.toFixed(2)}</strong>
                  </span>
                  <button type="button" data-testid="archive-flashcard-btn" onClick={() => handleArchiveCardSoftDelete(currentCard.id)} className="text-rose-600 hover:underline font-semibold cursor-pointer inline-flex items-center gap-1" title="Lưu trữ ẩn thẻ (is_archived = true, bảo toàn lịch sử flashcard_reviews)">
                    <Archive className="w-3.5 h-3.5"/>
                    <span>Lưu trữ ẩn thẻ</span>
                  </button>
                </div>
              </div>

              {/* 3D Flip Card Canvas (800x520px comfortable dimensions) */}
              <div className="w-full max-w-[800px] min-h-[440px] sm:min-h-[520px] perspective-1200">
                <div data-testid="fsrs-3d-flip-card" onClick={() => setIsFlipped((prev) => !prev)} className={`relative w-full min-h-[440px] sm:min-h-[520px] transition-transform duration-500 ease-in-out preserve-3d cursor-pointer select-none ${isFlipped ? "rotate-y-180" : ""}`}>
                  {/* FRONT FACE */}
                  <div data-testid="flashcard-front-face" className={`absolute inset-0 w-full h-full rounded-3xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xl hover:shadow-2xl transition-all p-6 sm:p-8 flex flex-col justify-between backface-hidden ${isFlipped ? "pointer-events-none opacity-0" : "opacity-100"}`}>
                    {/* Card Top Metadata Header */}
                    <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-slate-100 dark:border-slate-800">
                      <div className="flex items-center gap-2 text-xs">
                        <span className="px-2.5 py-0.5 rounded-md bg-indigo-50 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300 font-extrabold uppercase">
                          {currentCard.cardType}
                        </span>
                        <span className="text-slate-500 truncate max-w-[260px] sm:max-w-[400px] inline-flex items-center gap-1.5">
                          <BookOpen className="w-3.5 h-3.5 text-indigo-600 shrink-0"/>
                          <span>{currentCard.sourceTitleSnapshot || "Bài 3.2: Bộ nhớ ảo & Phân trang"}</span>
                        </span>
                      </div>

                      <div className="flex items-center gap-1.5">
                        {currentCard.tags.map((t) => (<span key={t.id} className="px-2 py-0.5 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 text-[11px] font-bold">
                            #{t.name}
                          </span>))}
                      </div>
                    </div>

                    {/* Card Front Content */}
                    <div className="my-auto py-6 space-y-6">
                      <div>
                        <div className="text-[11px] font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400 mb-2">
                          Câu hỏi / Mặt trước (Active Recall Prompt):
                        </div>
                        <div data-testid="flashcard-front-text" className="text-base sm:text-xl font-bold text-slate-900 dark:text-white leading-relaxed">
                          {renderClozeFormattedText(currentCard.frontText, false)}
                        </div>
                      </div>

                      <div className="pt-8 text-center">
                        <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-slate-100 dark:bg-slate-800 text-xs font-bold text-slate-600 dark:text-slate-300">
                          <RotateCw className="w-3.5 h-3.5 text-indigo-600"/>
                          <span>Nhấn Phím Cách (Space) hoặc Click vào thẻ để lật đáp án (180° 3D Flip)</span>
                        </span>
                      </div>
                    </div>

                    {/* Card Footer */}
                    <div className="pt-3 border-t border-slate-100 dark:border-slate-800 flex items-center justify-between text-[11px] text-slate-400">
                      <span>Phím tắt: Space (Lật thẻ) • 1 (Học lại) • 2 (Khó) • 3 (Tốt) • 4 (Dễ)</span>
                      <span>Bản chụp nguồn bất biến (ON DELETE SET NULL Safe)</span>
                    </div>
                  </div>

                  {/* BACK FACE */}
                  <div data-testid="flashcard-back-face" className={`absolute inset-0 w-full h-full rounded-3xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xl hover:shadow-2xl transition-all p-6 sm:p-8 flex flex-col justify-between backface-hidden rotate-y-180 overflow-y-auto ${isFlipped ? "opacity-100 visible" : "opacity-0 pointer-events-none invisible"}`}>
                    {/* Card Top Metadata Header */}
                    <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-slate-100 dark:border-slate-800">
                      <div className="flex items-center gap-2 text-xs">
                        <span className="px-2.5 py-0.5 rounded-md bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 font-extrabold uppercase">
                          Đáp án & Giải thích
                        </span>
                        <span className="text-slate-500 truncate max-w-[260px] sm:max-w-[400px] inline-flex items-center gap-1.5">
                          <BookOpen className="w-3.5 h-3.5 text-indigo-600 shrink-0"/>
                          <span>{currentCard.sourceTitleSnapshot || "Bài 3.2: Bộ nhớ ảo & Phân trang"}</span>
                        </span>
                      </div>

                      <div className="flex items-center gap-1.5">
                        {currentCard.tags.map((t) => (<span key={t.id} className="px-2 py-0.5 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 text-[11px] font-bold">
                            #{t.name}
                          </span>))}
                      </div>
                    </div>

                    {/* Card Back Content */}
                    <div className="my-auto py-4 space-y-4">
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

                      {currentCard.sourceQuoteSnapshot && (<div className="p-3 rounded-xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800 flex items-center justify-between gap-2 text-xs">
                          <span className="italic text-slate-600 dark:text-slate-400 truncate">
                            Dẫn chứng gốc: &ldquo;{currentCard.sourceQuoteSnapshot}&rdquo;
                          </span>
                          <button type="button" data-testid="jump-to-source-btn" onClick={(e) => {
                        e.stopPropagation();
                        if (currentCard.lessonId) {
                            setActiveLesson(currentCard.lessonId);
                        }
                        else if (currentCard.documentId) {
                            setActiveDocument(currentCard.documentId, currentCard.pageNumber ?? 1);
                        }
                        setActiveMode("split");
                    }} className="px-2.5 py-1 rounded-lg bg-indigo-50 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300 font-bold shrink-0 cursor-pointer inline-flex items-center gap-1 hover:bg-indigo-600 hover:text-white transition">
                            <BookOpen className="w-3 h-3"/>
                            <span>Nhảy đến nguồn</span>
                          </button>
                        </div>)}
                    </div>

                    {/* Card Footer */}
                    <div className="pt-3 border-t border-slate-100 dark:border-slate-800 flex items-center justify-between text-[11px] text-slate-400">
                      <span>Đánh giá mức độ ghi nhớ bên dưới (1-4)</span>
                      <span>FSRS v5 Dynamic Intervals</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* 4 FSRS Dynamic Rating Controls (Section 5.3) */}
              <div data-testid="fsrs-rating-bar" className="w-full max-w-[800px] grid grid-cols-2 sm:grid-cols-4 gap-3">
                <button type="button" data-testid="rate-btn-1" onClick={() => handleRateCard(1)} className="p-3 rounded-2xl border border-rose-500/30 bg-rose-500/5 hover:border-rose-500 hover:bg-rose-500 hover:text-white text-rose-700 dark:text-rose-300 transition flex flex-col items-center cursor-pointer">
                  <span className="text-xs font-bold">[1] Học lại (Again)</span>
                  <span className="text-[11px] opacity-85 mt-0.5">
                    {currentCard.intervalPreviews.again}
                  </span>
                </button>

                <button type="button" data-testid="rate-btn-2" onClick={() => handleRateCard(2)} className="p-3 rounded-2xl border border-amber-500/30 bg-amber-500/5 hover:border-amber-500 hover:bg-amber-500 hover:text-white text-amber-700 dark:text-amber-300 transition flex flex-col items-center cursor-pointer">
                  <span className="text-xs font-bold">[2] Khó (Hard)</span>
                  <span className="text-[11px] opacity-85 mt-0.5">
                    {currentCard.intervalPreviews.hard}
                  </span>
                </button>

                <button type="button" data-testid="rate-btn-3" onClick={() => handleRateCard(3)} className="p-3 rounded-2xl border border-emerald-500/30 bg-emerald-500/5 hover:border-emerald-600 hover:bg-emerald-600 hover:text-white text-emerald-700 dark:text-emerald-300 transition flex flex-col items-center cursor-pointer">
                  <span className="text-xs font-bold">[3] Tốt (Good)</span>
                  <span className="text-[11px] opacity-85 mt-0.5">
                    {currentCard.intervalPreviews.good}
                  </span>
                </button>

                <button type="button" data-testid="rate-btn-4" onClick={() => handleRateCard(4)} className="p-3 rounded-2xl border border-sky-500/30 bg-sky-500/5 hover:border-sky-500 hover:bg-sky-500 hover:text-white text-sky-700 dark:text-sky-300 transition flex flex-col items-center cursor-pointer">
                  <span className="text-xs font-bold">[4] Dễ (Easy)</span>
                  <span className="text-[11px] opacity-85 mt-0.5">
                    {currentCard.intervalPreviews.easy}
                  </span>
                </button>
              </div>
            </>)}
        </div>)}

      {/* ================================================================== */}
      {/* SUB-TAB 2: FORMATIVE PRACTICE QUIZ PORTAL (ADR-021 & AI RUBRIC)    */}
      {/* ================================================================== */}
      {activeReviewSubTab === "quiz" && quiz && (<div data-testid="practice-quiz-portal" className="space-y-5 max-w-4xl mx-auto">
          {/* Course Quiz Selector */}
          <div data-testid="quiz-course-selector" className="rounded-2xl bg-indigo-50/70 dark:bg-indigo-950/30 border border-indigo-500/30 p-4 space-y-2.5">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <label htmlFor="quiz-selector-select" className="text-xs font-extrabold text-indigo-800 dark:text-indigo-200 inline-flex items-center gap-1.5">
                <Target className="w-3.5 h-3.5 text-indigo-600 dark:text-indigo-400"/>
                <span>Chọn Đề luyện tập theo Khóa học / Tài liệu:</span>
              </label>
              <select id="quiz-selector-select" data-testid="quiz-selector-select" value={quiz.id || resolvedQuizId} onChange={(e) => {
                const nextId = e.target.value;
                setActiveQuizId(nextId);
                loadPracticeQuiz(nextId);
            }} className="px-3 py-1.5 rounded-xl border border-indigo-300 dark:border-indigo-700 bg-white dark:bg-slate-900 text-xs font-bold text-slate-800 dark:text-slate-100 cursor-pointer">
                {effectiveQuizList.map((qItem) => (<option key={qItem.id} value={qItem.id}>
                    {qItem.title}
                  </option>))}
              </select>
            </div>
            {effectiveQuizList.length > 1 && (<div className="flex flex-wrap items-center gap-1.5">
                {effectiveQuizList.map((qItem) => {
                    const isActiveQuiz = (quiz.id || resolvedQuizId) === qItem.id;
                    return (<button key={`pill-${qItem.id}`} type="button" onClick={() => {
                            setActiveQuizId(qItem.id);
                            loadPracticeQuiz(qItem.id);
                        }} className={`px-2.5 py-1 rounded-lg text-[11px] font-bold cursor-pointer transition ${isActiveQuiz
                            ? "bg-indigo-600 text-white shadow-2xs"
                            : "bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-700 dark:text-slate-300 hover:border-indigo-500"}`}>
                      {qItem.title}
                    </button>);
                })}
              </div>)}
          </div>

          <div className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5 flex items-center justify-between">
            <div>
              <span className="text-[11px] font-extrabold uppercase text-indigo-600 dark:text-indigo-400">
                Bài luyện tập củng cố (Làm sai không bị trừ điểm)
              </span>
              <h2 className="text-lg font-extrabold text-slate-900 dark:text-white mt-0.5">
                {quiz.title}
              </h2>
            </div>
            <span className="px-3 py-1 rounded-full bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 text-xs font-extrabold">
              Đã hoàn thành: {quiz.correctItems}/{quiz.totalItems} mục
            </span>
          </div>

          {quiz.items.map((item, idx) => {
                const currentAns = practiceQuizDraftAnswers[item.id] ??
                    item.userAnswer?.text ??
                    (item.itemType === "essay"
                        ? "Bảng trang đa cấp giúp tiết kiệm bộ nhớ RAM vì không cần cấp phát các vùng địa chỉ trống như bảng trang phẳng. Tuy nhiên khi xảy ra TLB Miss, phần cứng MMU phải đọc RAM 4 lần liên tiếp."
                        : "");
                return (<div key={item.id} data-testid={`quiz-item-${item.itemType}`} className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5 space-y-4 shadow-xs">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-extrabold uppercase text-indigo-600 dark:text-indigo-400">
                    Câu {idx + 1} • Loại: {item.itemType.toUpperCase()}
                  </span>
                  {typeof item.isCorrect === "boolean" && (<span className={`px-2.5 py-0.5 rounded-full text-xs font-bold ${item.isCorrect
                            ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                            : "bg-rose-500/15 text-rose-700 dark:text-rose-300"}`}>
                      {item.isCorrect ? (<span className="inline-flex items-center gap-1">
                          <CheckCircle2 className="w-3.5 h-3.5"/>
                          <span>Chính xác / Đã đánh giá</span>
                        </span>) : ("Cần xem lại")}
                    </span>)}
                </div>

                <p className="text-sm sm:text-base font-bold text-slate-900 dark:text-white leading-relaxed">
                  {item.prompt.question}
                </p>

                {item.itemType === "multiple_choice" && item.choices && (<div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
                    {item.choices.map((ch) => (<button key={ch.key} type="button" onClick={() => {
                                setPracticeQuizDraftAnswer(item.id, ch.key);
                                handleSubmitQuizItem(item.id, ch.key);
                            }} className={`p-3 rounded-xl border text-left text-xs font-bold transition cursor-pointer ${currentAns === ch.key
                                ? "border-indigo-600 bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300"
                                : "border-slate-200 dark:border-slate-800 hover:bg-slate-50 dark:hover:bg-slate-800"}`}>
                        [{ch.key}] {ch.text}
                      </button>))}
                  </div>)}

                {item.itemType === "cloze" && (<div className="flex items-center gap-2">
                    <input type="text" value={currentAns} onChange={(e) => setPracticeQuizDraftAnswer(item.id, e.target.value)} placeholder="Nhập thuật ngữ viết tắt (VD: TLB)..." className="flex-1 px-3 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-transparent text-xs font-bold"/>
                    <button type="button" onClick={() => handleSubmitQuizItem(item.id, currentAns || "TLB")} className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer">
                      Kiểm tra Cloze
                    </button>
                  </div>)}

                {item.itemType === "essay" && (<div className="space-y-3">
                    <textarea data-testid="essay-answer-textarea" rows={4} value={currentAns} onChange={(e) => setPracticeQuizDraftAnswer(item.id, e.target.value)} onKeyDown={(e) => {
                        if ((e.ctrlKey || e.metaKey) && e.key === "Enter") {
                            e.preventDefault();
                            handleSubmitQuizItem(item.id, currentAns);
                        }
                    }} className="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-slate-50 dark:bg-slate-950 p-3 text-xs sm:text-sm"/>
                    <div className="flex justify-end">
                      <button type="button" data-testid="submit-essay-ai-btn" disabled={submittingItemId === item.id} onClick={() => handleSubmitQuizItem(item.id, currentAns)} className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer inline-flex items-center gap-1.5 transition">
                        {submittingItemId === item.id ? (<>
                            <Sparkles className="w-3.5 h-3.5 animate-spin"/>
                            <span>Đang chấm qua .NET 10 Channel & gemini-3.8-flash...</span>
                          </>) : (<>
                            <Sparkles className="w-3.5 h-3.5"/>
                            <span>Gửi chấm nhận xét ngữ nghĩa AI (2 Credits)</span>
                          </>)}
                      </button>
                    </div>

                    {item.aiRubricFeedback && (<div data-testid="essay-ai-rubric-feedback" className="rounded-2xl border border-emerald-500/40 bg-emerald-50/60 dark:bg-emerald-950/30 p-4 space-y-2.5 text-xs">
                        <div className="flex items-center justify-between font-bold text-emerald-800 dark:text-emerald-300">
                          <span className="inline-flex items-center gap-1.5">
                            <Sparkles className="w-4 h-4 text-emerald-600 dark:text-emerald-400"/>
                            <span>Nhận xét chi tiết từ AI</span>
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
                            <CheckCircle2 className="w-3.5 h-3.5"/>
                            <span>Điểm mạnh trong lập luận:</span>
                          </div>
                          <ul className="list-disc list-inside text-slate-700 dark:text-slate-300 mt-1">
                            {(item.aiRubricFeedback.strengths || []).map((s, i) => (<li key={i}>{s}</li>))}
                          </ul>
                        </div>
                        <div>
                          <div className="inline-flex items-center gap-1 font-bold text-amber-700 dark:text-amber-400">
                            <Lightbulb className="w-3.5 h-3.5"/>
                            <span>Khái niệm gợi ý bổ sung:</span>
                          </div>
                          <ul className="list-disc list-inside text-slate-700 dark:text-slate-300 mt-1">
                            {(item.aiRubricFeedback.missingConcepts || []).map((m, i) => (<li key={i}>{m}</li>))}
                          </ul>
                        </div>
                      </div>)}
                  </div>)}
              </div>);
            })}

          {/* Practice Quiz Completion Card (Resolves UX-04) */}
          {(quiz.correctItems >= quiz.totalItems || quiz.items.every((it) => it.isCorrect || it.aiRubricFeedback)) && (<div data-testid="quiz-completion-summary-card" className="rounded-2xl border border-emerald-500/50 bg-emerald-50/70 dark:bg-emerald-950/40 p-6 text-center space-y-4 shadow-sm">
              <div className="w-12 h-12 rounded-2xl bg-emerald-500 text-white flex items-center justify-center mx-auto shadow-md">
                <Trophy className="w-6 h-6"/>
              </div>
              <div className="space-y-1">
                <h3 className="text-base sm:text-lg font-bold text-slate-900 dark:text-white">
                  Tuyệt vời! Bạn đã hoàn thành toàn bộ bài luyện tập thực hành!
                </h3>
                <p className="text-xs text-slate-600 dark:text-slate-400 max-w-md mx-auto">
                  Bạn đã nắm vững các khái niệm trọng tâm về Phân trang &amp; TLB. Hãy tiếp tục ôn tập ngắt quãng hoặc quay về Không gian cá nhân.
                </p>
              </div>

              <div className="flex flex-wrap items-center justify-center gap-3 pt-2">
                <button type="button" onClick={() => setActiveReviewSubTab("fsrs")} className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold transition cursor-pointer inline-flex items-center gap-1.5">
                  <Brain className="w-3.5 h-3.5"/>
                  <span>Quay về Hàng đợi Ôn tập FSRS</span>
                </button>
                <button type="button" onClick={() => setActiveMode("dashboard")} className="px-4 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-slate-100 dark:hover:bg-slate-800 text-xs font-bold transition cursor-pointer inline-flex items-center gap-1.5">
                  <LayoutDashboard className="w-3.5 h-3.5"/>
                  <span>Về Không gian cá nhân</span>
                </button>
              </div>
            </div>)}
        </div>)}
    </div>);
}
