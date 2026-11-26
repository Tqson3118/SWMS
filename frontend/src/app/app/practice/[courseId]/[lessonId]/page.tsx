"use client";

import React, { Suspense, useCallback, useEffect, useMemo, useState } from "react";
import dynamic from "next/dynamic";
import Link from "next/link";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import {
  AlertCircle,
  ArrowLeft,
  Brain,
  ChevronLeft,
  ChevronRight,
  Loader2,
  Moon,
  RefreshCw,
  Sun,
  Target,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { bootstrapAuthSession, invalidateDashboardCache } from "@/lib/api-client";

const LessonPracticeLab = dynamic(
  () => import("@/components/learn/LessonPracticeLab").then((mod) => mod.LessonPracticeLab),
  {
    ssr: false,
    loading: () => (
      <div className="flex-1 flex items-center justify-center p-10 text-[var(--text-secondary)] gap-2">
        <Loader2 className="w-5 h-5 animate-spin text-[var(--accent)]" />
        <span className="text-sm">Đang tải phòng thực hành...</span>
      </div>
    ),
  }
);

interface CourseLessonNavItem {
  id: string;
  title: string;
  chapterTitle: string;
}

interface CourseDetailData {
  id: string;
  title: string;
  chapters?: Array<{
    id: string;
    title: string;
    lessons?: Array<{ id: string; title: string }>;
  }>;
}

interface LessonDetailData {
  id: string;
  courseId?: string;
  title: string;
  contentMarkdown?: string | null;
  plainText?: string | null;
}

interface AstNode {
  type: string;
  attrs?: Record<string, unknown>;
  content?: AstNode[];
  text?: string;
}

interface NotebookDetailData {
  id: string;
  title: string;
  version: number;
  contentAst?: AstNode;
}

function appendTextToNotebookAst(
  title: string,
  existingAst: AstNode | undefined,
  textToAppend: string
): AstNode {
  const baseNodes = Array.isArray(existingAst?.content) ? existingAst.content : [];
  const hasHeading = baseNodes.some((n) => n.type === "heading");
  const nextNodes: AstNode[] = hasHeading
    ? [...baseNodes]
    : [
        {
          type: "heading",
          attrs: { level: 1, block_id: crypto.randomUUID() },
          content: [{ type: "text", text: title || "Ghi chú bài học" }],
        },
        ...baseNodes,
      ];

  const blocks = textToAppend
    .trim()
    .split(/\n{2,}/)
    .map((p) => p.trim())
    .filter(Boolean)
    .map((p) => ({
      type: "paragraph",
      attrs: { block_id: crypto.randomUUID() },
      content: [{ type: "text", text: p }],
    }));

  return {
    type: "doc",
    content: [...nextNodes, ...blocks],
  };
}

function FullscreenPracticePageInner() {
  const params = useParams();
  const router = useRouter();
  const searchParams = useSearchParams();
  const { activeUserHeaderId, darkMode, toggleDarkMode } = useWorkspaceStore();

  const courseId = String(params.courseId || "");
  const lessonId = String(params.lessonId || "");
  const activityTypeParam = searchParams.get("type");
  const activityKeyParam = searchParams.get("key");

  const [course, setCourse] = useState<CourseDetailData | null>(null);
  const [lesson, setLesson] = useState<LessonDetailData | null>(null);
  const [notebook, setNotebook] = useState<NotebookDetailData | null>(null);
  const [fsrsDueCount, setFsrsDueCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const getHeaders = useCallback(
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

  const loadContext = useCallback(async (isRetry = false) => {
    if (!courseId || !lessonId) {
      setLoadError("Thiếu thông tin khóa học hoặc bài học.");
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setLoadError(null);
    try {
      // Chờ bootstrap session / token hydration hoàn tất
      await bootstrapAuthSession().catch(() => false);
      const headers = getHeaders();

      // Best-effort auto-enroll for public courses
      await fetch(`/api/v1/courses/${courseId}/enroll`, {
        method: "POST",
        headers,
      }).catch(() => null);

      let [courseResp, lessonResp] = await Promise.all([
        fetch(`/api/v1/courses/${courseId}`, { headers }),
        fetch(`/api/v1/lessons/${lessonId}`, { headers }),
      ]);

      // Nếu lần đầu nhận 401 do race condition trong lúc token đang hydrate, tự động retry an toàn 1 lần sau 350ms
      if (!isRetry && (courseResp.status === 401 || lessonResp.status === 401)) {
        await new Promise((r) => setTimeout(r, 350));
        await bootstrapAuthSession().catch(() => false);
        const retryHeaders = getHeaders();
        [courseResp, lessonResp] = await Promise.all([
          fetch(`/api/v1/courses/${courseId}`, { headers: retryHeaders }),
          fetch(`/api/v1/lessons/${lessonId}`, { headers: retryHeaders }),
        ]);
      }

      if (!courseResp.ok) {
        if (!isRetry && (courseResp.status === 0 || courseResp.status >= 500)) {
          await new Promise((r) => setTimeout(r, 300));
          return void loadContext(true);
        }
        setLoadError(`Không tải được thông tin khóa học (HTTP ${courseResp.status}).`);
        return;
      }
      if (!lessonResp.ok) {
        if (!isRetry && (lessonResp.status === 0 || lessonResp.status >= 500)) {
          await new Promise((r) => setTimeout(r, 300));
          return void loadContext(true);
        }
        setLoadError(`Không tải được thông tin bài học (HTTP ${lessonResp.status}).`);
        return;
      }

      const currentHeaders = getHeaders();
      const dueResp = await fetch(`/api/v1/flashcards/due?courseId=${encodeURIComponent(courseId)}`, {
        headers: currentHeaders,
      }).catch(() => null);

      const courseRaw = await courseResp.json();
      const lessonRaw: LessonDetailData = await lessonResp.json();

      setCourse({
        id: String(courseRaw.courseId || courseRaw.id || courseId),
        title: String(courseRaw.title || "Khóa học"),
        chapters: Array.isArray(courseRaw.chapters) ? courseRaw.chapters : [],
      });
      setLesson(lessonRaw);

      if (dueResp && dueResp.ok) {
        const dueCards = await dueResp.json().catch(() => []);
        const count = Array.isArray(dueCards)
          ? dueCards.length
          : Array.isArray(dueCards?.cards)
            ? dueCards.cards.length
            : 0;
        setFsrsDueCount(count);
      }

      const nbResp = await fetch("/api/v1/notebooks/by-source", {
        method: "POST",
        headers: getHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          lessonId: lessonRaw.id,
          sourceTitle: lessonRaw.title,
        }),
      }).catch(() => null);

      if (nbResp && nbResp.ok) {
        const nbData: NotebookDetailData = await nbResp.json();
        setNotebook(nbData);
      }
    } catch {
      if (!isRetry) {
        await new Promise((r) => setTimeout(r, 400));
        return void loadContext(true);
      }
      setLoadError("Không thể kết nối tới máy chủ để mở phòng thực hành.");
    } finally {
      setIsLoading(false);
    }
  }, [courseId, lessonId, getHeaders]);

  useEffect(() => {
    void loadContext();
  }, [loadContext]);

  const flatLessons = useMemo<CourseLessonNavItem[]>(() => {
    if (!course?.chapters) return [];
    return course.chapters.flatMap((ch) =>
      (ch.lessons ?? []).map((l) => ({
        id: l.id,
        title: l.title,
        chapterTitle: ch.title,
      }))
    );
  }, [course]);

  const currentIdx = flatLessons.findIndex((l) => l.id === lessonId);
  const prevLesson = currentIdx > 0 ? flatLessons[currentIdx - 1] : null;
  const nextLesson =
    currentIdx >= 0 && currentIdx < flatLessons.length - 1 ? flatLessons[currentIdx + 1] : null;

  const handleSaveToNotebook = useCallback(
    async (textToAppend: string) => {
      const trimmed = textToAppend.trim();
      if (!trimmed || !lesson) {
        throw new Error("Thiếu dữ liệu bài học hoặc nội dung cần lưu.");
      }

      let currentNb = notebook;
      if (!currentNb) {
        const nbResp = await fetch("/api/v1/notebooks/by-source", {
          method: "POST",
          headers: getHeaders({ "Content-Type": "application/json" }),
          body: JSON.stringify({
            lessonId: lesson.id,
            sourceTitle: lesson.title,
          }),
        });
        if (!nbResp.ok) {
          throw new Error(`Không thể mở sổ tay (HTTP ${nbResp.status}).`);
        }
        currentNb = (await nbResp.json()) as NotebookDetailData;
        setNotebook(currentNb);
      }

      const nextAst = appendTextToNotebookAst(currentNb.title, currentNb.contentAst, trimmed);
      const putResp = await fetch(`/api/v1/notebooks/${currentNb.id}`, {
        method: "PUT",
        headers: getHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          baseVersion: currentNb.version,
          title: currentNb.title,
          contentAstJson: JSON.stringify(nextAst),
          deviceId: "studxy-practice-fullscreen",
        }),
      });

      if (!putResp.ok) {
        throw new Error(`Chưa thể lưu sổ tay lúc này (HTTP ${putResp.status}).`);
      }

      const updated = await putResp.json();
      setNotebook({
        id: currentNb.id,
        title: updated.title ?? currentNb.title,
        version: Number(updated.version ?? currentNb.version + 1),
        contentAst: updated.contentAst ?? nextAst,
      });
      invalidateDashboardCache();
      if (typeof window !== "undefined") {
        window.dispatchEvent(new Event("swms:data-mutated"));
      }
    },
    [lesson, notebook, getHeaders]
  );

  const handleCreateFlashcardQuick = useCallback(
    async (front: string, back: string) => {
      const cleanFront = front.trim();
      const cleanBack = back.trim();
      if (!cleanFront || !cleanBack || !lesson) {
        throw new Error("Thiếu nội dung mặt trước/mặt sau thẻ.");
      }

      const resp = await fetch("/api/v1/flashcards", {
        method: "POST",
        headers: getHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          lessonId: lesson.id,
          documentId: null,
          annotationId: null,
          cardType: "basic",
          sourceTitleSnapshot: lesson.title,
          sourceQuoteSnapshot: cleanFront,
          frontText: cleanFront,
          backText: cleanBack,
          tagNames: [],
        }),
      });

      if (!resp.ok) {
        throw new Error(`Chưa thể tạo thẻ ghi nhớ lúc này (HTTP ${resp.status}).`);
      }

      invalidateDashboardCache();
      if (typeof window !== "undefined") {
        window.dispatchEvent(new Event("swms:data-mutated"));
      }
    },
    [lesson, getHeaders]
  );

  if (isLoading) {
    return (
      <div className="h-screen flex flex-col bg-[var(--canvas-bg)] text-[var(--text-primary)]">
        <div className="h-14 border-b border-[var(--border-calm)] bg-[var(--surface-card)] px-4 flex items-center justify-between">
          <div className="h-7 w-48 rounded bg-[var(--surface-elevated)] swms-skeleton" />
          <div className="h-7 w-32 rounded bg-[var(--surface-elevated)] swms-skeleton" />
        </div>
        <div className="flex-1 flex items-center justify-center">
          <div className="flex items-center gap-2.5 text-sm text-[var(--text-secondary)]">
            <Loader2 className="w-5 h-5 animate-spin text-[var(--accent)]" />
            <span>Đang chuẩn bị phòng thực hành toàn màn hình...</span>
          </div>
        </div>
      </div>
    );
  }

  if (loadError || !course || !lesson) {
    return (
      <div className="h-screen flex flex-col items-center justify-center bg-[var(--canvas-bg)] text-[var(--text-primary)] p-4">
        <div className="max-w-md w-full rounded-[14px] border border-rose-500/30 bg-[var(--surface-card)] p-6 text-center space-y-4 shadow-elevation-1">
          <AlertCircle className="w-8 h-8 text-rose-500 mx-auto" />
          <div className="space-y-1">
            <h1 className="text-base font-semibold text-[var(--text-primary)]">
              Không thể mở phòng thực hành
            </h1>
            <p className="text-xs text-[var(--text-secondary)]">
              {loadError || "Bài học không tồn tại hoặc bạn không có quyền truy cập."}
            </p>
          </div>
          <div className="flex flex-wrap items-center justify-center gap-2 pt-1">
            <button
              type="button"
              onClick={() => void loadContext()}
              className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-[10px] bg-[var(--accent)] text-accent-contrast text-xs font-semibold cursor-pointer"
            >
              <RefreshCw className="w-3.5 h-3.5" />
              <span>Thử lại</span>
            </button>
            <Link
              href="/app/practice"
              className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs font-medium text-[var(--text-primary)]"
            >
              <ArrowLeft className="w-3.5 h-3.5" />
              <span>Hub Luyện tập</span>
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col bg-[var(--canvas-bg)] text-[var(--text-primary)] overflow-hidden">
      {/* Compact Sticky Top Header */}
      <header className="min-h-11 sm:min-h-14 shrink-0 border-b border-[var(--border-calm)] bg-[var(--surface-card)] px-2 sm:px-5 py-1.5 sm:py-2 flex items-center justify-between gap-1.5 sm:gap-2 z-20">
        {/* Left Navigation */}
        <div className="flex items-center gap-1 sm:gap-2 min-w-0 shrink">
          <Link
            href={`/app/courses/${courseId}/learn?lesson=${lessonId}`}
            data-testid="practice-back-to-lesson-btn"
            className="inline-flex items-center gap-1 sm:gap-1.5 h-7 sm:h-8 px-2 sm:px-2.5 rounded-[8px] border border-[var(--border-calm)] bg-[var(--canvas-bg)] text-xs font-medium text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition shrink-0"
          >
            <ArrowLeft className="w-3.5 h-3.5" />
            <span className="hidden xs:inline">Quay lại bài học</span>
            <span className="xs:hidden">Bài học</span>
          </Link>

          <Link
            href="/app/practice"
            data-testid="practice-hub-link"
            className="hidden sm:inline-flex items-center gap-1.5 h-8 px-2.5 rounded-[8px] text-xs font-medium text-[var(--text-secondary)] hover:text-[var(--accent)] hover:bg-[var(--accent-subtle)] transition shrink-0"
          >
            <Target className="w-3.5 h-3.5" />
            <span>Hub Luyện tập</span>
          </Link>

          <span className="hidden lg:inline text-[var(--text-tertiary)]">/</span>
          <span
            title={course.title}
            className="hidden lg:inline text-xs font-semibold text-[var(--text-primary)] truncate max-w-[260px]"
          >
            {course.title}
          </span>
        </div>

        {/* Center/Right Lesson Switcher + FSRS + Theme */}
        <div className="flex items-center gap-1 sm:gap-2 min-w-0 shrink-0">
          {flatLessons.length > 0 && (
            <div className="flex items-center gap-0.5 sm:gap-1 min-w-0">
              <button
                type="button"
                disabled={!prevLesson}
                onClick={() => {
                  if (prevLesson) {
                    router.push(`/app/practice/${courseId}/${prevLesson.id}`);
                  }
                }}
                title={prevLesson ? `Bài trước: ${prevLesson.title}` : "Đây là bài đầu tiên"}
                aria-label="Bài học trước"
                className="h-7 w-7 sm:h-8 sm:w-8 rounded-[8px] border border-[var(--border-calm)] flex items-center justify-center text-[var(--text-secondary)] hover:text-[var(--text-primary)] disabled:opacity-40 cursor-pointer shrink-0"
              >
                <ChevronLeft className="w-3.5 h-3.5 sm:w-4 sm:h-4" />
              </button>

              {flatLessons.length > 1 && (
                <select
                  aria-label="Chuyển nhanh bài thực hành"
                  data-testid="practice-lesson-switcher"
                  value={lessonId}
                  onChange={(e) => {
                    const nextId = e.target.value;
                    if (nextId && nextId !== lessonId) {
                      router.push(`/app/practice/${courseId}/${nextId}`);
                    }
                  }}
                  className="h-7 sm:h-8 max-w-[160px] sm:max-w-[240px] min-w-0 rounded-[8px] border border-[var(--border-calm)] bg-[var(--canvas-bg)] px-1.5 sm:px-2 text-xs text-[var(--text-primary)] focus:outline-none focus:border-[var(--accent)] truncate"
                >
                  {flatLessons.map((item) => (
                    <option key={item.id} value={item.id}>
                      {item.title}
                    </option>
                  ))}
                </select>
              )}

              <button
                type="button"
                disabled={!nextLesson}
                onClick={() => {
                  if (nextLesson) {
                    router.push(`/app/practice/${courseId}/${nextLesson.id}`);
                  }
                }}
                title={nextLesson ? `Bài tiếp theo: ${nextLesson.title}` : "Đây là bài cuối cùng"}
                aria-label="Bài học tiếp theo"
                className="h-7 w-7 sm:h-8 sm:w-8 rounded-[8px] border border-[var(--border-calm)] flex items-center justify-center text-[var(--text-secondary)] hover:text-[var(--text-primary)] disabled:opacity-40 cursor-pointer shrink-0"
              >
                <ChevronRight className="w-3.5 h-3.5 sm:w-4 sm:h-4" />
              </button>
            </div>
          )}

          {fsrsDueCount > 0 && (
            <Link
              href={`/app/review?courseId=${encodeURIComponent(courseId)}`}
              className="hidden sm:inline-flex items-center gap-1.5 h-8 px-2.5 rounded-[8px] border border-[var(--accent)]/30 bg-[var(--accent-subtle)] text-xs font-semibold text-[var(--accent)] hover:opacity-90 transition"
            >
              <Brain className="w-3.5 h-3.5" />
              <span>Ôn FSRS ({fsrsDueCount})</span>
            </Link>
          )}

          <button
            type="button"
            onClick={toggleDarkMode}
            aria-label={darkMode ? "Chuyển sang giao diện sáng" : "Chuyển sang giao diện tối"}
            title={darkMode ? "Giao diện sáng" : "Giao diện tối"}
            className="h-7 w-7 sm:h-8 sm:w-8 rounded-[8px] border border-[var(--border-calm)] bg-[var(--canvas-bg)] flex items-center justify-center text-[var(--text-secondary)] hover:text-[var(--text-primary)] transition cursor-pointer"
          >
            {darkMode ? <Sun className="w-3.5 h-3.5 sm:w-4 sm:h-4 text-amber-400" /> : <Moon className="w-3.5 h-3.5 sm:w-4 sm:h-4" />}
          </button>
        </div>
      </header>

      {/* Full-Screen Practice Lab */}
      <div className="flex-1 min-h-0 flex flex-col overflow-hidden">
        <LessonPracticeLab
          isEmbedded={false}
          courseId={course.id}
          courseTitle={course.title}
          lessonId={lesson.id}
          lessonTitle={lesson.title}
          lessonMarkdown={lesson.contentMarkdown || lesson.plainText || ""}
          initialActivityType={activityTypeParam}
          initialActivityKey={activityKeyParam}
          onSaveToNotebook={handleSaveToNotebook}
          onCreateFlashcardQuick={handleCreateFlashcardQuick}
        />
      </div>
    </div>
  );
}

export default function FullscreenLessonPracticePage() {
  return (
    <Suspense
      fallback={
        <div className="flex items-center justify-center h-screen bg-[var(--canvas-bg)]">
          <div className="w-6 h-6 border-2 border-[var(--accent)] border-t-transparent rounded-full animate-spin" />
        </div>
      }
    >
      <FullscreenPracticePageInner />
    </Suspense>
  );
}
