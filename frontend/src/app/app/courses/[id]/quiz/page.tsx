"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import {
  AlertCircle,
  ArrowLeft,
  BookOpen,
  Brain,
  CheckCircle2,
  ClipboardCheck,
  Loader2,
  Search,
  Send,
  Sparkles,
  Trophy,
  XCircle,
} from "lucide-react";
import {
  getClientRequestHeaders,
  useWorkspaceStore,
} from "@/stores/useWorkspaceStore";
import { formatCleanQuizPrompt } from "@/lib/course-helpers";

interface QuizListItem {
  id: string;
  title: string;
  lessonId?: string | null;
  documentId?: string | null;
  courseId?: string | null;
  totalItems: number;
  correctItems: number;
  status: string;
}

interface QuizPrompt {
  question?: string;
  rubricCriteria?: string[];
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
}

interface QuizItem {
  id: string;
  quizId: string;
  itemType: "multiple_choice" | "cloze" | "short_answer" | "essay" | string;
  prompt: QuizPrompt | string;
  choices?: QuizChoice[] | null;
  clozeSolution?: string | null;
  userAnswer?: { text?: string } | null;
  isCorrect?: boolean | null;
  aiRubricFeedback?: AiRubricFeedback | null;
  evaluatedAt?: string | null;
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
  aiSummaryFeedback?: string | null;
  items: QuizItem[];
}

const QUIZ_ITEM_TYPE_LABELS: Record<string, string> = {
  multiple_choice: "Trắc nghiệm",
  cloze: "Điền từ còn thiếu",
  short_answer: "Trả lời ngắn",
  essay: "Tự luận",
};

export default function CourseQuizPage() {
  const params = useParams();
  const courseId = params.id as string;
  const { activeUserHeaderId } = useWorkspaceStore();

  const [courseTitle, setCourseTitle] = useState("Khóa học");
  const [quizzes, setQuizzes] = useState<QuizListItem[]>([]);
  const [quizQuery, setQuizQuery] = useState("");
  const [selectedQuizId, setSelectedQuizId] = useState<string | null>(null);
  const [quizDetail, setQuizDetail] = useState<QuizDetail | null>(null);
  const [draftAnswers, setDraftAnswers] = useState<Record<string, string>>({});
  const [submittingIds, setSubmittingIds] = useState<Record<string, boolean>>({});
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

      if (courseResp && courseResp.ok) {
        const cData = await courseResp.json();
        if (cData?.title) setCourseTitle(cData.title);
      }

      if (quizzesResp && quizzesResp.ok) {
        const qList: QuizListItem[] = await quizzesResp.json();
        setQuizzes(Array.isArray(qList) ? qList : []);
        if (Array.isArray(qList) && qList.length > 0) {
          setSelectedQuizId(qList[0].id);
          await loadQuizDetail(qList[0].id);
        }
      }
    } catch {
      setErrorMsg("Không thể tải dữ liệu bài kiểm tra.");
    } finally {
      setIsLoading(false);
    }
  }, [courseId, buildHeaders, loadQuizDetail]);

  useEffect(() => {
    loadPageData();
  }, [loadPageData]);

  const handleSelectQuiz = async (qId: string) => {
    setSelectedQuizId(qId);
    setErrorMsg(null);
    await loadQuizDetail(qId);
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
              "Bạn đã hết hạn mức 50 AI credits. Vui lòng thử lại sau hoặc nạp thêm."
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

      // If async essay grading job is still queued/running, poll /api/v1/jobs/{id}
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

  if (isLoading) {
    return (
      <div className="max-w-[1360px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-10 min-h-[80vh] space-y-6">
        <div className="h-6 w-32 rounded bg-[var(--surface-elevated)] animate-pulse" />
        <div className="h-10 w-72 rounded-lg bg-[var(--surface-elevated)] animate-pulse" />
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 pt-2">
          <div className="lg:col-span-4 h-72 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
          <div className="lg:col-span-8 space-y-4">
            {[0, 1, 2].map((i) => (
              <div key={i} className="h-40 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
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

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-10 min-h-[80vh]">
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
        <div className="flex items-center gap-2">
          <Link
            href={`/app/courses/${courseId}/learn`}
            className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition"
          >
            <BookOpen className="w-3.5 h-3.5" />
            <span>Mở bài giảng</span>
          </Link>
          <Link
            href="/app/review"
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
              className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition"
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
          {/* Mobile Quiz Selector Dropdown */}
          {quizzes.length > 1 && (
            <div className="lg:hidden">
              <label
                htmlFor="quiz-mobile-selector"
                className="block text-xs font-medium text-text-secondary mb-1.5"
              >
                Chọn bài kiểm tra ({quizzes.length} bài)
              </label>
              <select
                id="quiz-mobile-selector"
                data-testid="quiz-mobile-selector"
                value={selectedQuizId ?? ""}
                onChange={(e) => handleSelectQuiz(e.target.value)}
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

          {/* Desktop 2-Column Sidebar: Quiz Selector + Question Navigator + Stats */}
          <aside
            data-testid="quiz-sidebar-selector"
            className="hidden lg:flex lg:col-span-4 flex-col gap-4 sticky top-4"
          >
            {/* Question Map & Score Card */}
            <div className="rounded-xl border border-border bg-bg-secondary p-4 space-y-3">
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
              <div className="rounded-xl border border-border bg-bg-secondary p-3.5 flex flex-col">
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

                <div className="max-h-[400px] overflow-y-auto swms-thin-scrollbar space-y-1 pr-0.5">
                  {quizzes
                    .filter((q) =>
                      q.title.toLowerCase().includes(quizQuery.trim().toLowerCase())
                    )
                    .map((q) => {
                      const active = selectedQuizId === q.id;
                      return (
                        <button
                          key={q.id}
                          type="button"
                          onClick={() => handleSelectQuiz(q.id)}
                          className={`w-full text-left px-3 py-2 rounded-md text-xs transition cursor-pointer flex items-center justify-between gap-2 ${
                            active
                              ? "bg-accent text-white font-medium"
                              : "bg-bg-primary text-text-secondary border border-border/60 hover:text-text-primary hover:border-border"
                          }`}
                        >
                          <span className="line-clamp-2 leading-snug">{q.title}</span>
                          <span
                            className={`text-[10px] font-semibold px-1.5 py-0.5 rounded shrink-0 ${
                              active
                                ? "bg-white/20 text-white"
                                : "bg-bg-secondary text-text-tertiary"
                            }`}
                          >
                            {q.correctItems}/{q.totalItems}
                          </span>
                        </button>
                      );
                    })}
                </div>
              </div>
            )}
          </aside>

          <div className="lg:col-span-8 space-y-6">
            {/* Quiz Summary Banner */}
            <div className="rounded-lg border border-border bg-bg-secondary p-5 flex flex-wrap items-center justify-between gap-4">
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

              <div className="flex items-center gap-3">
                <div className="px-3 py-1.5 rounded-md bg-bg-primary border border-border text-sm font-semibold text-text-primary flex items-center gap-1.5">
                  <Trophy className="w-4 h-4 text-amber-500" />
                  <span>
                    {quizDetail.correctItems}/{quizDetail.totalItems} đúng
                  </span>
                </div>
              </div>
            </div>

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
                        <h3 className="text-sm font-semibold text-text-primary leading-relaxed">
                          {questionText}
                        </h3>
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
                              {choice.text}
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
                        placeholder="Trình bày câu trả lời của bạn..."
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
                      placeholder="Nhập câu trả lời của bạn..."
                      className="w-full h-9 px-3 rounded-md border border-border bg-bg-secondary text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-accent"
                    />
                  )}

                  {/* Submit Button */}
                  <div className="flex items-center justify-between gap-3 pt-1">
                    {isEvaluated && item.clozeSolution && item.isCorrect === false ? (
                      <p className="text-xs text-text-secondary">
                        Đáp án chuẩn:{" "}
                        <span className="font-semibold text-emerald-600 dark:text-emerald-400">
                          {item.clozeSolution}
                        </span>
                      </p>
                    ) : (
                      <span />
                    )}

                    <button
                      type="button"
                      disabled={
                        isSubmitting ||
                        (item.itemType !== "essay" && isEvaluated) ||
                        !draft.trim()
                      }
                      onClick={() => handleSubmitItem(item)}
                      className="inline-flex items-center gap-1.5 h-8 px-3.5 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover disabled:opacity-50 disabled:cursor-not-allowed transition cursor-pointer"
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
                  </div>

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
    </div>
  );
}
