"use client";

import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import type { PDFDocumentProxy, RenderTask } from "pdfjs-dist";
import {
  AlertCircle,
  ArrowLeft,
  ArrowRight,
  Award,
  BookOpen,
  Brain,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  ClipboardCheck,
  Clock,
  Download,
  FileText,
  FlaskConical,
  Highlighter,
  Layers,
  List,
  Loader2,
  Moon,
  NotebookPen,
  PanelLeft,
  Plus,
  Save,
  Sparkles,
  Sun,
  Trash2,
  Type,
  X,
  Zap,
  ZoomIn,
  ZoomOut,
} from "lucide-react";
import {
  SEMANTIC_HIGHLIGHT_KEYS,
  getClientRequestHeaders,
  useWorkspaceStore,
} from "@/stores/useWorkspaceStore";

function stripChapterPrefix(title: string): string {
  if (!title) return "";
  return title.replace(/^(?:Chương|Chapter|Phần)\s*\d+\s*[:.\-–—]\s*/i, "").trim();
}
import {
  CourseRoadmapSidebar,
  type RoadmapChapterItem,
} from "@/components/reader/CourseRoadmapSidebar";
import {
  LessonMarkdownView,
  type LessonHeadingItem,
  type MarkdownAnnotationItem,
} from "@/components/reader/LessonMarkdownView";
import dynamic from "next/dynamic";

const LessonPracticeLab = dynamic(
  () => import("@/components/learn/LessonPracticeLab").then((mod) => mod.LessonPracticeLab),
  {
    ssr: false,
    loading: () => (
      <div className="flex-1 flex items-center justify-center p-8 text-[var(--text-muted)] animate-pulse">
        Đang tải phòng thực hành...
      </div>
    )
  }
);

export interface CourseLearningWorkspaceProps {
  courseId: string;
  initialLessonId?: string | null;
  initialDocId?: string | null;
  initialNotebookId?: string | null;
  openNotesInitially?: boolean;
}

interface LessonItem {
  id: string;
  title: string;
  sortOrder?: number;
  estimatedMinutes?: number;
  durationMinutes?: number;
  startPage?: number | null;
  endPage?: number | null;
  quizId?: string | null;
  isCompleted?: boolean;
}

interface ChapterItem {
  id: string;
  chapterId?: string;
  title: string;
  sortOrder?: number;
  lessons: LessonItem[];
}

interface CourseDetailDto {
  id: string;
  title: string;
  sourceFolder?: string | null;
  quizId?: string | null;
  quizCount?: number;
  progressPercent?: number;
  totalLessons?: number;
  completedLessons?: number;
  completedLessonIds?: string[];
  chapters: ChapterItem[];
}

interface AnnotationItem {
  id: string;
  lessonId?: string | null;
  documentId?: string | null;
  blockId?: string | null;
  pageNumber?: number | null;
  exactQuote: string;
  prefixQuote?: string;
  suffixQuote?: string;
  colorKey: "yellow" | "green" | "blue" | "purple" | "rose";
  semanticLabel?: string;
  noteComment?: string | null;
  status: string;
  linkedFlashcardsCount?: number;
}

interface AstNode {
  type: string;
  attrs?: Record<string, unknown>;
  content?: AstNode[];
  text?: string;
}

interface LessonDetailDto {
  id: string;
  courseId?: string;
  title: string;
  contentMarkdown?: string | null;
  plainText?: string | null;
  contentAst?: AstNode | null;
  videoUrl?: string | null;
  estimatedMinutes?: number;
  sourceFolder?: string | null;
  sourceRelativePath?: string | null;
  sourceDocumentId?: string | null;
  startPage?: number | null;
  totalPages?: number | null;
  adjacentPrevLessonId?: string | null;
  adjacentNextLessonId?: string | null;
  linkedQuizId?: string | null;
  annotations?: AnnotationItem[];
}

interface DocumentPageDto {
  documentId: string;
  pageNumber: number;
  plainText: string;
  hasTextLayer?: boolean;
}

interface NotebookDetailDto {
  id: string;
  title: string;
  version: number;
  primarySourceLessonId?: string | null;
  primarySourceDocumentId?: string | null;
  contentAst?: AstNode;
  updatedAt?: string;
}

interface FlashcardItem {
  id: string;
  lessonId?: string | null;
  documentId?: string | null;
  frontText: string;
  backText: string;
  cardType?: string;
  sourceQuoteSnapshot?: string | null;
}

type CenterViewTab = "lesson" | "practice" | "pdf" | "split";
type RightPanelMode = "closed" | "toc" | "drawer";
type DrawerTab = "notebook" | "highlights" | "flashcards";
type PdfRenderMode = "canvas" | "text";
type DraftCardMode = "cloze" | "manual" | "ai";

const COLOR_DOT_CLASSES: Record<AnnotationItem["colorKey"], string> = {
  yellow: "bg-amber-400 border-amber-500",
  green: "bg-emerald-400 border-emerald-500",
  blue: "bg-sky-400 border-sky-500",
  purple: "bg-violet-400 border-violet-500",
  rose: "bg-rose-400 border-rose-500",
};

function toNullableUuid(val: string | null | undefined): string | null {
  if (!val) return null;
  const trimmed = val.trim();
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(trimmed)
    ? trimmed
    : null;
}

function getEmbedVideoInfo(url?: string | null): { kind: "iframe" | "video"; src: string } | null {
  if (!url || !url.trim()) return null;
  const trimmed = url.trim();
  const ytMatch = trimmed.match(
    /(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([A-Za-z0-9_-]{6,})/
  );
  if (ytMatch?.[1]) {
    return { kind: "iframe", src: `https://www.youtube.com/embed/${ytMatch[1]}` };
  }
  const vimeoMatch = trimmed.match(/vimeo\.com\/(?:video\/)?(\d+)/);
  if (vimeoMatch?.[1]) {
    return { kind: "iframe", src: `https://player.vimeo.com/video/${vimeoMatch[1]}` };
  }
  if (/^https?:\/\//i.test(trimmed) || trimmed.startsWith("/")) {
    return { kind: "video", src: trimmed };
  }
  return null;
}

function extractEditableTextFromAst(ast?: AstNode | null): string {
  if (!ast || !Array.isArray(ast.content)) return "";
  const blocks: string[] = [];
  for (const node of ast.content) {
    if (node.type === "annotationRef") continue;
    const text = (node.content ?? []).map((c) => c.text || "").join("");
    if (node.type === "heading") {
      const level = Number(node.attrs?.level ?? 1);
      if (blocks.length === 0 && level === 1) continue; // skip auto-generated heading title
      blocks.push(`${"#".repeat(Math.min(3, Math.max(1, level)))} ${text}`);
    } else if (text.trim() !== "") {
      blocks.push(text);
    }
  }
  return blocks.join("\n\n");
}

function extractCitationsFromAst(ast?: AstNode | null) {
  if (!ast || !Array.isArray(ast.content)) return [];
  return ast.content
    .filter((node) => node.type === "annotationRef")
    .map((node, idx) => ({
      key: String(node.attrs?.block_id || idx),
      quote: String(node.attrs?.source_quote || ""),
      sourceTitle: String(node.attrs?.source_title || ""),
      colorKey: String(node.attrs?.color_key || "yellow") as AnnotationItem["colorKey"],
      pageNumber: node.attrs?.page_number ? Number(node.attrs.page_number) : null,
    }));
}

function buildNotebookAst(
  title: string,
  plainText: string,
  existingAst?: AstNode | null
): AstNode {
  const existingRefs = (existingAst?.content ?? []).filter(
    (n) => n.type === "annotationRef"
  );
  const paragraphs = plainText
    .split(/\n{2,}/)
    .map((p) => p.trimEnd())
    .filter((p) => p.length > 0);

  const content: AstNode[] = [
    {
      type: "heading",
      attrs: { level: 1, block_id: crypto.randomUUID() },
      content: [{ type: "text", text: title || "Ghi chú bài học" }],
    },
    ...paragraphs.map((text) => ({
      type: "paragraph",
      attrs: { block_id: crypto.randomUUID() },
      content: [{ type: "text", text }],
    })),
    ...existingRefs,
  ];
  return { type: "doc", content };
}

export function CourseLearningWorkspace({
  courseId,
  initialLessonId,
  initialDocId,
  initialNotebookId,
  openNotesInitially = false,
}: CourseLearningWorkspaceProps) {
  const router = useRouter();
  const {
    activeUserHeaderId,
    darkMode,
    toggleDarkMode,
    serifFont,
    toggleSerifFont,
    fontSizePx,
    setFontSizePx,
    cleanReadingMode,
    toggleCleanReadingMode,
  } = useWorkspaceStore();

  const isStandalone = courseId === "standalone";

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

  // Core data states
  const [course, setCourse] = useState<CourseDetailDto | null>(null);
  const [completedIds, setCompletedIds] = useState<string[]>([]);
  const [activeLessonId, setActiveLessonId] = useState<string | null>(initialLessonId ?? null);
  const [activeDocId, setActiveDocId] = useState<string | null>(initialDocId ?? null);

  const [lesson, setLesson] = useState<LessonDetailDto | null>(null);
  const [docMeta, setDocMeta] = useState<{
    id: string;
    title: string;
    totalPages: number | null;
    annotations: AnnotationItem[];
  } | null>(null);
  const [availableDocs, setAvailableDocs] = useState<
    Array<{ id: string; title: string; totalPages?: number | null }>
  >([]);
  const [flashcards, setFlashcards] = useState<FlashcardItem[]>([]);

  // Notebook state
  const [notebook, setNotebook] = useState<NotebookDetailDto | null>(null);
  const [noteTitle, setNoteTitle] = useState("");
  const [noteBody, setNoteBody] = useState("");
  const [noteDirty, setNoteDirty] = useState(false);
  const [noteSaveStatus, setNoteSaveStatus] = useState<"idle" | "saving" | "saved" | "error">(
    "idle"
  );
  const [noteErrorMsg, setNoteErrorMsg] = useState<string | null>(null);

  // UI Layout states
  const [isInitialLoading, setIsInitialLoading] = useState(true);
  const [lessonLoading, setLessonLoading] = useState(false);
  const [pageError, setPageError] = useState<string | null>(null);
  const [toastMsg, setToastMsg] = useState<string | null>(null);

  const [roadmapOpen, setRoadmapOpen] = useState(true);
  const [centerTab, setCenterTab] = useState<CenterViewTab>(
    initialDocId && !initialLessonId ? "pdf" : "lesson"
  );
  const [rightPanel, setRightPanel] = useState<RightPanelMode>(
    openNotesInitially ? "drawer" : "closed"
  );
  const [drawerTab, setDrawerTab] = useState<DrawerTab>("notebook");
  const [headings, setHeadings] = useState<LessonHeadingItem[]>([]);
  const [typographyOpen, setTypographyOpen] = useState(false);

  // Completion states
  const [isCompleting, setIsCompleting] = useState(false);
  const [celebrationOpen, setCelebrationOpen] = useState(false);

  // Selection toolbar & interactive modals
  const [selectedText, setSelectedText] = useState("");
  const [selectedBlockId, setSelectedBlockId] = useState<string | null>(null);
  const [selectionCoords, setSelectionCoords] = useState<{ top: number; left: number } | null>(
    null
  );

  // Flashcard draft modal
  const [flashcardModalOpen, setFlashcardModalOpen] = useState(false);
  const [draftMode, setDraftMode] = useState<DraftCardMode>("cloze");
  const [draftFront, setDraftFront] = useState("");
  const [draftBack, setDraftBack] = useState("");
  const [draftTagsText, setDraftTagsText] = useState("");
  const [aiCardLoading, setAiCardLoading] = useState(false);
  const [savingCard, setSavingCard] = useState(false);

  // AI Explain modal
  const [aiExplainOpen, setAiExplainOpen] = useState(false);
  const [aiExplainLoading, setAiExplainLoading] = useState(false);
  const [aiExplanation, setAiExplanation] = useState("");
  const [aiExcerpt, setAiExcerpt] = useState("");

  // Reference Markdown modal
  const [refModal, setRefModal] = useState<{
    title: string;
    markdown: string;
    sourceFolder: string;
    relativePath: string;
  } | null>(null);

  // PDF states
  const [pdfDoc, setPdfDoc] = useState<PDFDocumentProxy | null>(null);
  const [pdfPage, setPdfPage] = useState(1);
  const [pdfJumpInput, setPdfJumpInput] = useState("1");
  const [pdfTotalPages, setPdfTotalPages] = useState<number>(1);
  const [pdfZoom, setPdfZoom] = useState(100);
  const [pdfRenderMode, setPdfRenderMode] = useState<PdfRenderMode>("canvas");
  const [currentPageData, setCurrentPageData] = useState<DocumentPageDto | null>(null);
  const [pageLoading, setPageLoading] = useState(false);
  const [pdfLoadError, setPdfLoadError] = useState<string | null>(null);

  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const renderTaskRef = useRef<RenderTask | null>(null);
  const pageCacheRef = useRef<Map<number, DocumentPageDto>>(new Map());
  const readerScrollRef = useRef<HTMLDivElement | null>(null);
  const autoSaveTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Warn user before reload/navigation if note has unsaved changes (STU-03)
  useEffect(() => {
    if (!noteDirty) return;
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      e.preventDefault();
      e.returnValue = "";
      return "";
    };
    window.addEventListener("beforeunload", handleBeforeUnload);
    return () => {
      window.removeEventListener("beforeunload", handleBeforeUnload);
    };
  }, [noteDirty]);

  const showToast = useCallback((msg: string) => {
    setToastMsg(msg);
    setTimeout(() => setToastMsg((curr) => (curr === msg ? null : curr)), 4000);
  }, []);

  // Flattened ordered lesson list
  const flatLessons = useMemo<LessonItem[]>(() => {
    if (!course?.chapters) return [];
    return course.chapters.flatMap((ch) => ch.lessons ?? []);
  }, [course]);

  const roadmapChapters = useMemo<RoadmapChapterItem[]>(() => {
    if (!course?.chapters) return [];
    return course.chapters.map((ch, cIdx) => ({
      id: ch.chapterId || ch.id,
      title: `Chương ${cIdx + 1}: ${stripChapterPrefix(ch.title)}`,
      sortOrder: ch.sortOrder,
      lessons: (ch.lessons ?? []).map((l) => ({
        id: l.id,
        title: l.title,
        sortOrder: l.sortOrder,
        estimatedMinutes: l.estimatedMinutes ?? l.durationMinutes,
        startPage: l.startPage,
        endPage: l.endPage,
        quizId: l.quizId,
      })),
    }));
  }, [course]);

  const totalLessonCount = course?.totalLessons || flatLessons.length;
  const completedCount = completedIds.length;
  const percentCompleted =
    totalLessonCount > 0 ? Math.min(100, Math.round((completedCount * 100) / totalLessonCount)) : 0;

  const activeLessonIdx = activeLessonId
    ? flatLessons.findIndex((l) => l.id === activeLessonId)
    : -1;
  const prevLessonId =
    (activeLessonIdx > 0 ? flatLessons[activeLessonIdx - 1]?.id : null) ??
    lesson?.adjacentPrevLessonId ??
    null;
  const nextLessonId =
    (activeLessonIdx >= 0 && activeLessonIdx < flatLessons.length - 1
      ? flatLessons[activeLessonIdx + 1]?.id
      : null) ??
    lesson?.adjacentNextLessonId ??
    null;

  const activeChapterTitle = useMemo(() => {
    if (!course?.chapters || !activeLessonId) return null;
    for (let cIdx = 0; cIdx < course.chapters.length; cIdx++) {
      const ch = course.chapters[cIdx];
      if ((ch.lessons ?? []).some((l) => l.id === activeLessonId)) {
        return `Chương ${cIdx + 1}: ${stripChapterPrefix(ch.title)}`;
      }
    }
    return null;
  }, [course, activeLessonId]);

  const effectiveAnnotations = useMemo<AnnotationItem[]>(() => {
    if (centerTab === "pdf" && activeDocId && docMeta?.id === activeDocId) {
      return docMeta.annotations ?? [];
    }
    return lesson?.annotations ?? docMeta?.annotations ?? [];
  }, [centerTab, activeDocId, docMeta, lesson]);

  const anchoredAnnotations = useMemo<MarkdownAnnotationItem[]>(() => {
    return effectiveAnnotations
      .filter((a) => a.status === "anchored")
      .map((a) => ({
        id: a.id,
        blockId: a.blockId,
        exactQuote: a.exactQuote,
        prefixQuote: a.prefixQuote,
        suffixQuote: a.suffixQuote,
        colorKey: a.colorKey || "yellow",
        semanticLabel: a.semanticLabel,
        status: a.status,
        linkedFlashcardsCount: a.linkedFlashcardsCount,
      }));
  }, [effectiveAnnotations]);

  // 1. Load initial course, notebook context, and resolve lesson/doc
  useEffect(() => {
    let cancelled = false;

    const init = async () => {
      setIsInitialLoading(true);
      setPageError(null);
      try {
        const headers = getHeaders();
        let loadedCourse: CourseDetailDto | null = null;

        const docsListRes = await fetch("/api/v1/documents", { headers }).catch(() => null);
        if (docsListRes && docsListRes.ok && !cancelled) {
          const docsRaw = await docsListRes.json().catch(() => []);
          if (Array.isArray(docsRaw)) {
            setAvailableDocs(
              docsRaw
                .map((d: { id?: string; documentId?: string; title?: string; fileName?: string; totalPages?: number | null }) => ({
                  id: String(d.documentId || d.id || ""),
                  title: String(d.title || d.fileName || "Tài liệu PDF"),
                  totalPages: d.totalPages ?? null,
                }))
                .filter((d) => Boolean(d.id))
            );
          }
        }

        if (!isStandalone) {
          // Auto-enroll if public / not yet enrolled (ignore if already enrolled)
          fetch(`/api/v1/courses/${courseId}/enroll`, {
            method: "POST",
            headers,
          }).catch(() => null);

          const courseRes = await fetch(`/api/v1/courses/${courseId}`, { headers });
          if (courseRes.ok) {
            const raw = await courseRes.json();
            const chaps: ChapterItem[] = raw.chapters ?? [];
            const doneIds: string[] = Array.isArray(raw.completedLessonIds)
              ? raw.completedLessonIds
              : [];
            loadedCourse = {
              id: String(raw.courseId || raw.id || courseId),
              title: String(raw.title || "Khóa học"),
              sourceFolder: raw.sourceFolder ?? null,
              quizId: raw.quizId ?? null,
              quizCount: Number(raw.quizCount ?? 0),
              progressPercent: Number(raw.progressPercent ?? 0),
              totalLessons: Number(raw.totalLessons ?? 0),
              completedLessons: Number(raw.completedLessons ?? doneIds.length),
              completedLessonIds: doneIds,
              chapters: chaps,
            };
          } else if (!initialDocId && !initialNotebookId) {
            setPageError(`Không tải được khóa học (HTTP ${courseRes.status}).`);
          }
        }

        if (cancelled) return;
        if (loadedCourse) {
          setCourse(loadedCourse);
          setCompletedIds(loadedCourse.completedLessonIds ?? []);
        }

        let resolvedLessonId = initialLessonId ?? null;
        let resolvedDocId = initialDocId ?? null;

        // If navigated from /app/notes with notebook ID & no lesson/doc, inspect notebook first
        if (initialNotebookId && !resolvedLessonId && !resolvedDocId) {
          const nbRes = await fetch(`/api/v1/notebooks/${initialNotebookId}`, { headers }).catch(
            () => null
          );
          if (nbRes && nbRes.ok) {
            const nbData: NotebookDetailDto = await nbRes.json();
            if (!cancelled) {
              setNotebook(nbData);
              setNoteTitle(nbData.title || "");
              setNoteBody(extractEditableTextFromAst(nbData.contentAst));
              setNoteDirty(false);
              if (nbData.primarySourceLessonId) resolvedLessonId = nbData.primarySourceLessonId;
              else if (nbData.primarySourceDocumentId) resolvedDocId = nbData.primarySourceDocumentId;
            }
          }
        }

        if (!resolvedLessonId && !resolvedDocId && loadedCourse) {
          const doneSet = new Set(loadedCourse.completedLessonIds ?? []);
          const all = (loadedCourse.chapters ?? []).flatMap((c) => c.lessons ?? []);
          const next = all.find((l) => !doneSet.has(l.id)) || all[0];
          if (next) resolvedLessonId = next.id;
        }

        if (cancelled) return;
        if (resolvedLessonId) setActiveLessonId(resolvedLessonId);
        if (resolvedDocId) {
          setActiveDocId(resolvedDocId);
          setCenterTab(resolvedLessonId ? "split" : "pdf");
        } else if (resolvedLessonId) {
          setCenterTab("lesson");
        }

        if (typeof window !== "undefined" && window.innerWidth < 768) {
          setRoadmapOpen(false);
        }
      } catch {
        if (!cancelled) setPageError("Không thể kết nối tới máy chủ.");
      } finally {
        if (!cancelled) setIsInitialLoading(false);
      }
    };

    void init();
    return () => {
      cancelled = true;
    };
  }, [courseId, initialLessonId, initialDocId, initialNotebookId, isStandalone, getHeaders]);

  // 2. Load lesson detail & linked notebook & flashcards when activeLessonId changes
  const loadLessonContext = useCallback(
    async (lessonId: string) => {
      setLessonLoading(true);
      setHeadings([]);
      setSelectedText("");
      setSelectionCoords(null);
      try {
        const headers = getHeaders();
        const res = await fetch(`/api/v1/lessons/${lessonId}`, { headers });
        if (!res.ok) {
          setPageError(`Không tải được nội dung bài học (HTTP ${res.status}).`);
          return;
        }
        const raw: LessonDetailDto = await res.json();
        setLesson({ ...raw, annotations: raw.annotations ?? [] });

        if (raw.sourceDocumentId) {
          setActiveDocId(raw.sourceDocumentId);
          if (raw.startPage && raw.startPage > 0) {
            setPdfPage(raw.startPage);
            setPdfJumpInput(String(raw.startPage));
          }
          if (raw.totalPages) setPdfTotalPages(raw.totalPages);
        } else if (!initialDocId) {
          setActiveDocId(null);
        }

        const nbRes = await fetch("/api/v1/notebooks/by-source", {
          method: "POST",
          headers: getHeaders({ "Content-Type": "application/json" }),
          body: JSON.stringify({ lessonId: raw.id, sourceTitle: raw.title }),
        }).catch(() => null);

        if (nbRes && nbRes.ok) {
          const nbData: NotebookDetailDto = await nbRes.json();
          setNotebook(nbData);
          setNoteTitle(nbData.title || "");
          setNoteBody(extractEditableTextFromAst(nbData.contentAst));
          setNoteDirty(false);
          setNoteSaveStatus("idle");
        }

        const fcQuery = new URLSearchParams({ includeAll: "true", lessonId: raw.id });
        const fcRes = await fetch(`/api/v1/flashcards/due?${fcQuery.toString()}`, {
          headers,
        }).catch(() => null);
        if (fcRes && fcRes.ok) {
          const cards = await fcRes.json();
          setFlashcards(Array.isArray(cards) ? cards : []);
        }

        readerScrollRef.current?.scrollTo({ top: 0, behavior: "auto" });
      } catch {
        setPageError("Không thể tải bài học.");
      } finally {
        setLessonLoading(false);
      }
    },
    [getHeaders, initialDocId]
  );

  useEffect(() => {
    if (activeLessonId) {
      void loadLessonContext(activeLessonId);
    }
  }, [activeLessonId, loadLessonContext]);

  // 3. Load document metadata + binary PDF when activeDocId changes
  useEffect(() => {
    if (!activeDocId) {
      setPdfDoc(null);
      setDocMeta(null);
      return;
    }

    let cancelled = false;
    pageCacheRef.current.clear();
    setPdfDoc(null);
    setPdfRenderMode("canvas");
    setPdfLoadError(null);

    const loadDoc = async () => {
      try {
        const headers = getHeaders();
        const docRes = await fetch(`/api/v1/documents/${activeDocId}`, { headers });
        if (docRes.ok && !cancelled) {
          const raw = await docRes.json();
          setDocMeta({
            id: String(raw.id || activeDocId),
            title: String(raw.title || "Tài liệu PDF"),
            totalPages: raw.totalPages ?? null,
            annotations: raw.annotations ?? [],
          });
          if (raw.totalPages) setPdfTotalPages(raw.totalPages);
          if (raw.initialPage) {
            pageCacheRef.current.set(1, raw.initialPage);
            setCurrentPageData(raw.initialPage);
          }

          if (!activeLessonId) {
            const nbRes = await fetch("/api/v1/notebooks/by-source", {
              method: "POST",
              headers: getHeaders({ "Content-Type": "application/json" }),
              body: JSON.stringify({ documentId: activeDocId, sourceTitle: raw.title }),
            }).catch(() => null);
            if (nbRes && nbRes.ok && !cancelled) {
              const nbData: NotebookDetailDto = await nbRes.json();
              setNotebook(nbData);
              setNoteTitle(nbData.title || "");
              setNoteBody(extractEditableTextFromAst(nbData.contentAst));
              setNoteDirty(false);
            }

            const fcRes = await fetch(
              `/api/v1/flashcards/due?includeAll=true&documentId=${activeDocId}`,
              { headers }
            ).catch(() => null);
            if (fcRes && fcRes.ok && !cancelled) {
              const cards = await fcRes.json();
              setFlashcards(Array.isArray(cards) ? cards : []);
            }
          }
        } else if (!cancelled && docRes.status === 404) {
          setPdfLoadError("Tài liệu PDF không tìm thấy hoặc đã bị xóa (HTTP 404).");
          return;
        }
      } catch {
        // Non-fatal if metadata fails while PDF stream works
      }

      try {
        const pdfjs = await import("pdfjs-dist");
        if (typeof window !== "undefined") {
          pdfjs.GlobalWorkerOptions.workerSrc = "/pdf.worker.min.mjs";
        }
        const loadingTask = pdfjs.getDocument({
          url: `/api/v1/documents/${activeDocId}/file`,
          httpHeaders: getHeaders(),
          withCredentials: true,
          cMapPacked: true,
        });
        const loadedPdf = await loadingTask.promise;
        if (!cancelled) {
          setPdfDoc(loadedPdf);
          setPdfTotalPages(loadedPdf.numPages);
          setPdfLoadError(null);
        }
      } catch (err: unknown) {
        if (!cancelled) {
          setPdfDoc(null);
          const is404 =
            err &&
            typeof err === "object" &&
            "name" in err &&
            (err as { name: string }).name === "MissingPDFException";
          setPdfLoadError(
            is404
              ? "Tệp PDF không tìm thấy trên máy chủ (HTTP 404)."
              : "Không thể tải tệp PDF. Vui lòng thử lại sau."
          );
        }
      }
    };

    void loadDoc();
    return () => {
      cancelled = true;
      if (renderTaskRef.current) {
        try {
          renderTaskRef.current.cancel();
        } catch {
          // ignore
        }
        renderTaskRef.current = null;
      }
    };
  }, [activeDocId, activeLessonId, getHeaders]);

  // 4. Render active PDF page onto canvas
  useEffect(() => {
    if (
      !pdfDoc ||
      !canvasRef.current ||
      pdfRenderMode !== "canvas" ||
      (centerTab !== "pdf" && centerTab !== "split")
    ) {
      return;
    }

    let cancelled = false;
    const render = async () => {
      if (renderTaskRef.current) {
        try {
          renderTaskRef.current.cancel();
        } catch {
          // ignore
        }
        renderTaskRef.current = null;
      }
      try {
        const safePage = Math.max(1, Math.min(pdfDoc.numPages, pdfPage));
        const page = await pdfDoc.getPage(safePage);
        if (cancelled || !canvasRef.current) return;
        const viewport = page.getViewport({ scale: (pdfZoom / 100) * 1.4 });
        const canvas = canvasRef.current;
        const ctx = canvas.getContext("2d", { willReadFrequently: true });
        if (!ctx) return;
        canvas.width = viewport.width;
        canvas.height = viewport.height;
        const task = page.render({ canvasContext: ctx, viewport });
        renderTaskRef.current = task;
        await task.promise;
        renderTaskRef.current = null;
      } catch (err: unknown) {
        const name = (err as { name?: string })?.name;
        if (name !== "RenderingCancelledException") {
          console.error("PDF page render error:", err);
        }
      }
    };

    void render();
    return () => {
      cancelled = true;
      if (renderTaskRef.current) {
        try {
          renderTaskRef.current.cancel();
        } catch {
          // ignore
        }
        renderTaskRef.current = null;
      }
    };
  }, [pdfDoc, pdfPage, pdfZoom, pdfRenderMode, centerTab]);

  // 5. Fetch PDF page extracted text
  useEffect(() => {
    if (!activeDocId || (centerTab !== "pdf" && centerTab !== "split")) return;
    setPdfJumpInput(String(pdfPage));
    const cached = pageCacheRef.current.get(pdfPage);
    if (cached) {
      setCurrentPageData(cached);
      return;
    }

    let cancelled = false;
    setPageLoading(true);
    fetch(`/api/v1/documents/${activeDocId}/pages/${pdfPage}`, { headers: getHeaders() })
      .then(async (res) => {
        if (cancelled) return;
        if (res.ok) {
          const data: DocumentPageDto = await res.json();
          pageCacheRef.current.set(pdfPage, data);
          setCurrentPageData(data);
        } else if (res.status === 404) {
          const empty: DocumentPageDto = {
            documentId: activeDocId,
            pageNumber: pdfPage,
            plainText: "",
            hasTextLayer: false,
          };
          pageCacheRef.current.set(pdfPage, empty);
          setCurrentPageData(empty);
        }
      })
      .catch(() => {
        // keep current state
      })
      .finally(() => {
        if (!cancelled) setPageLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [activeDocId, pdfPage, centerTab, getHeaders]);

  // 6. Save notebook handler (manual + auto-save)
  const saveNotebookNow = useCallback(
    async (
      customBody?: string,
      customAst?: AstNode,
      successBanner?: string
    ): Promise<boolean> => {
      if (!notebook) return false;
      setNoteSaveStatus("saving");
      setNoteErrorMsg(null);
      try {
        const bodyToUse = customBody ?? noteBody;
        const astToUse =
          customAst ?? buildNotebookAst(noteTitle || notebook.title, bodyToUse, notebook.contentAst);
        const resp = await fetch(`/api/v1/notebooks/${notebook.id}`, {
          method: "PUT",
          headers: getHeaders({ "Content-Type": "application/json" }),
          body: JSON.stringify({
            baseVersion: notebook.version,
            title: (noteTitle || notebook.title).trim(),
            contentAstJson: JSON.stringify(astToUse),
            deviceId: "studxy-web",
          }),
        });

        if (resp.status === 409) {
          const conflict = await resp.json().catch(() => ({}));
          setNoteSaveStatus("error");
          setNoteErrorMsg(
            conflict.message ||
              "Phát hiện bản ghi mới hơn trên máy chủ; hệ thống đã tạo bản sao đối chiếu."
          );
          if (conflict.canonicalNotebook) {
            setNotebook((prev) =>
              prev
                ? {
                    ...prev,
                    version: Number(conflict.serverVersion ?? prev.version),
                    contentAst: conflict.canonicalNotebook.contentAst,
                  }
                : prev
            );
          }
          return false;
        }

        if (!resp.ok) {
          setNoteSaveStatus("error");
          setNoteErrorMsg(`Không thể lưu sổ tay (HTTP ${resp.status}).`);
          return false;
        }

        const updated = await resp.json();
        setNotebook((prev) =>
          prev
            ? {
                ...prev,
                title: updated.title ?? prev.title,
                version: Number(updated.version ?? prev.version + 1),
                contentAst: updated.contentAst ?? astToUse,
                updatedAt: updated.updatedAt,
              }
            : prev
        );
        setNoteDirty(false);
        setNoteSaveStatus("saved");
        if (successBanner) showToast(successBanner);
        return true;
      } catch {
        setNoteSaveStatus("error");
        setNoteErrorMsg("Lỗi kết nối khi lưu sổ tay.");
        return false;
      }
    },
    [notebook, noteBody, noteTitle, getHeaders, showToast]
  );

  // Debounced auto-save when notebook changes
  useEffect(() => {
    if (!noteDirty || !notebook) return;
    if (autoSaveTimerRef.current) clearTimeout(autoSaveTimerRef.current);
    autoSaveTimerRef.current = setTimeout(() => {
      void saveNotebookNow();
    }, 1500);
    return () => {
      if (autoSaveTimerRef.current) clearTimeout(autoSaveTimerRef.current);
    };
  }, [noteDirty, noteBody, noteTitle, notebook, saveNotebookNow]);

  // 7. Global shortcuts (Ctrl+\ toggle roadmap, Ctrl+S save note)
  useEffect(() => {
    const onKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === "\\") {
        e.preventDefault();
        setRoadmapOpen((prev) => !prev);
      } else if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "s") {
        if (rightPanel === "drawer" && drawerTab === "notebook" && notebook) {
          e.preventDefault();
          void saveNotebookNow(undefined, undefined, "Đã lưu ghi chú.");
        }
      }
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [rightPanel, drawerTab, notebook, saveNotebookNow]);

  // Select lesson handler updating URL query cleanly
  const handleSelectLesson = useCallback(
    (lessonId: string) => {
      setActiveLessonId(lessonId);
      setCenterTab("lesson");
      if (!isStandalone) {
        router.replace(`/app/courses/${courseId}/learn?lesson=${lessonId}`, { scroll: false });
      }
    },
    [courseId, isStandalone, router]
  );

  // Mark complete & advance
  const handleMarkComplete = async () => {
    if (!activeLessonId || isCompleting) return;
    setIsCompleting(true);
    try {
      const resp = await fetch(`/api/v1/lessons/${activeLessonId}/complete`, {
        method: "POST",
        headers: getHeaders({ "Content-Type": "application/json" }),
      });

      if (!resp.ok) {
        if (resp.status === 403 && !isStandalone && courseId) {
          const enrollRes = await fetch(`/api/v1/courses/${courseId}/enroll`, {
            method: "POST",
            headers: getHeaders(),
          }).catch(() => null);
          if (enrollRes && enrollRes.ok) {
            const retryResp = await fetch(`/api/v1/lessons/${activeLessonId}/complete`, {
              method: "POST",
              headers: getHeaders({ "Content-Type": "application/json" }),
            });
            if (retryResp.ok) {
              const nextCompleted = Array.from(new Set([...completedIds, activeLessonId]));
              setCompletedIds(nextCompleted);
              window.dispatchEvent(new Event("swms:data-mutated"));
              const newPercent =
                totalLessonCount > 0 ? Math.round((nextCompleted.length * 100) / totalLessonCount) : 100;
              if (newPercent >= 100 || !nextLessonId) {
                setCelebrationOpen(true);
                showToast("Bạn đã hoàn thành toàn bộ khóa học!");
              } else {
                showToast("Đã hoàn thành bài học.");
                handleSelectLesson(nextLessonId);
              }
              return;
            }
          }
        }
        showToast(`Chưa lưu được tiến độ (HTTP ${resp.status}).`);
        return;
      }

      const nextCompleted = Array.from(new Set([...completedIds, activeLessonId]));
      setCompletedIds(nextCompleted);
      window.dispatchEvent(new Event("swms:data-mutated"));

      const newPercent =
        totalLessonCount > 0 ? Math.round((nextCompleted.length * 100) / totalLessonCount) : 100;

      if (newPercent >= 100 || !nextLessonId) {
        setCelebrationOpen(true);
        showToast("Bạn đã hoàn thành toàn bộ khóa học!");
      } else {
        showToast("Đã hoàn thành bài học.");
        handleSelectLesson(nextLessonId);
      }
    } catch {
      showToast("Lỗi kết nối khi cập nhật tiến độ.");
    } finally {
      setIsCompleting(false);
    }
  };

  // Capture text selection for contextual toolbar
  const captureSelectionInReader = () => {
    if (typeof window === "undefined") return;
    const sel = window.getSelection();
    const text = sel?.toString().trim() ?? "";
    if (text.length < 2) {
      setSelectedText("");
      setSelectionCoords(null);
      return;
    }
    setSelectedText(text);
    try {
      if (sel && sel.rangeCount > 0) {
        const rect = sel.getRangeAt(0).getBoundingClientRect();
        if (rect.width > 0 || rect.height > 0) {
          setSelectionCoords({
            top: Math.max(64, rect.top - 48),
            left: Math.min(window.innerWidth - 260, Math.max(16, rect.left)),
          });
          return;
        }
      }
    } catch {
      // Fallback below
    }
    setSelectionCoords(null);
  };

  // Reload annotations / doc / lesson cleanly
  const refreshAnnotations = useCallback(async () => {
    const headers = getHeaders();
    if (activeLessonId) {
      const res = await fetch(`/api/v1/lessons/${activeLessonId}`, { headers }).catch(() => null);
      if (res && res.ok) {
        const raw = await res.json();
        setLesson((prev) => (prev ? { ...prev, annotations: raw.annotations ?? [] } : prev));
      }
    }
    if (activeDocId) {
      const docRes = await fetch(`/api/v1/documents/${activeDocId}`, { headers }).catch(() => null);
      if (docRes && docRes.ok) {
        const raw = await docRes.json();
        setDocMeta((prev) =>
          prev ? { ...prev, annotations: raw.annotations ?? [] } : prev
        );
      }
    }
  }, [activeLessonId, activeDocId, getHeaders]);

  // Create highlight annotation
  const handleCreateHighlight = async (colorKey: AnnotationItem["colorKey"]) => {
    const quote = selectedText.trim();
    if (!quote) return;
    const isPdfActive = (centerTab === "pdf" || !lesson) && Boolean(activeDocId);
    const fullText = isPdfActive
      ? currentPageData?.plainText || ""
      : lesson?.plainText || lesson?.contentMarkdown || "";
    const startIdx = fullText.indexOf(quote);
    const startOffset = startIdx >= 0 ? startIdx : 0;
    const endOffset = startOffset + quote.length;
    const prefixQuote = fullText.slice(Math.max(0, startOffset - 32), startOffset);
    const suffixQuote = fullText.slice(endOffset, Math.min(fullText.length, endOffset + 32));

    const cfg = SEMANTIC_HIGHLIGHT_KEYS.find((k) => k.key === colorKey);
    try {
      const resp = await fetch("/api/v1/annotations", {
        method: "POST",
        headers: getHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          lessonId: isPdfActive ? null : lesson?.id,
          documentId: isPdfActive ? activeDocId : null,
          blockId: isPdfActive ? null : toNullableUuid(selectedBlockId),
          pageNumber: isPdfActive ? pdfPage : null,
          exactQuote: quote,
          originalExactQuote: quote,
          prefixQuote,
          suffixQuote,
          startOffset,
          endOffset,
          colorKey,
          semanticLabel: cfg?.label || "Đánh dấu",
          noteComment: cfg ? cfg.label : "Đánh dấu",
        }),
      });
      if (resp.ok) {
        setSelectedText("");
        setSelectionCoords(null);
        await refreshAnnotations();
        showToast("Đã lưu đánh dấu.");
      } else {
        showToast(`Không lưu được đánh dấu (HTTP ${resp.status}).`);
      }
    } catch {
      showToast("Lỗi kết nối khi tạo đánh dấu.");
    }
  };

  // Delete highlight annotation
  const handleDeleteAnnotation = async (annId: string) => {
    try {
      const resp = await fetch(`/api/v1/annotations/${annId}`, {
        method: "DELETE",
        headers: getHeaders(),
      });
      if (resp.ok) {
        await refreshAnnotations();
        showToast("Đã xóa đánh dấu.");
      }
    } catch {
      showToast("Không xóa được đánh dấu.");
    }
  };

  // Open flashcard modal
  const handleOpenFlashcardModal = (excerptOverride?: string) => {
    const excerpt = (excerptOverride ?? selectedText).trim();
    if (excerpt) {
      const words = excerpt.split(/\s+/);
      if (words.length > 3) {
        const masked = words.slice(0, Math.min(4, words.length)).join(" ");
        const rest = words.slice(Math.min(4, words.length)).join(" ");
        setDraftFront(`{{c1::${masked}}} ${rest}`.trim());
      } else {
        setDraftFront(`{{c1::${excerpt}}}`);
      }
      const sourceTitle = lesson?.title || docMeta?.title || "Bài học";
      setDraftBack(`Trích từ "${sourceTitle}": ${excerpt}`);
      setDraftMode("cloze");
    } else {
      setDraftFront("");
      setDraftBack("");
      setDraftMode("manual");
    }
    setDraftTagsText(lesson?.title ? "Bai-giang" : "Tai-lieu");
    setFlashcardModalOpen(true);
  };

  const handleGenerateAiCard = async () => {
    const excerpt =
      selectedText.trim() ||
      currentPageData?.plainText?.slice(0, 200) ||
      lesson?.plainText?.slice(0, 200) ||
      "";
    if (!excerpt) {
      showToast("Hãy bôi đen đoạn văn bản trước khi nhờ AI gợi ý thẻ.");
      return;
    }
    setDraftMode("ai");
    setAiCardLoading(true);
    try {
      const resp = await fetch("/api/v1/ai/generate-flashcard", {
        method: "POST",
        headers: getHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          excerpt,
          contextTitle: lesson?.title || docMeta?.title || "Bài học",
          actionType: "generate_qa",
        }),
      });
      if (resp.ok) {
        const data = await resp.json();
        setDraftFront(data.front || data.frontText || "");
        setDraftBack(data.back || data.backText || "");
      } else {
        showToast("Không tạo được gợi ý AI. Bạn có thể tự nhập nội dung thẻ.");
      }
    } catch {
      showToast("Lỗi kết nối AI.");
    } finally {
      setAiCardLoading(false);
    }
  };

  const handleSaveFlashcard = async () => {
    if (!draftFront.trim() || !draftBack.trim() || savingCard) return;
    setSavingCard(true);
    const isPdfActive = (centerTab === "pdf" || !lesson) && Boolean(activeDocId);
    try {
      let annotationId: string | null = null;
      const quote = selectedText.trim();
      if (quote) {
        const annResp = await fetch("/api/v1/annotations", {
          method: "POST",
          headers: getHeaders({ "Content-Type": "application/json" }),
          body: JSON.stringify({
            lessonId: isPdfActive ? null : lesson?.id,
            documentId: isPdfActive ? activeDocId : null,
            blockId: isPdfActive ? null : toNullableUuid(selectedBlockId),
            pageNumber: isPdfActive ? pdfPage : null,
            exactQuote: quote,
            originalExactQuote: quote,
            prefixQuote: "",
            suffixQuote: "",
            startOffset: 0,
            endOffset: quote.length,
            colorKey: "yellow",
            semanticLabel: "Trích dẫn thẻ ghi nhớ",
            noteComment: "Tạo từ thẻ ghi nhớ",
          }),
        });
        if (annResp.ok) {
          const createdAnn = await annResp.json();
          annotationId = createdAnn.id ?? null;
          await refreshAnnotations();
        }
      }

      const tags = draftTagsText
        .split(",")
        .map((t) => t.trim())
        .filter(Boolean);

      const resp = await fetch("/api/v1/flashcards", {
        method: "POST",
        headers: getHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          lessonId: isPdfActive ? null : lesson?.id,
          documentId: isPdfActive ? activeDocId : null,
          annotationId,
          cardType: draftMode === "cloze" ? "cloze" : draftMode === "ai" ? "ai_generated" : "basic",
          sourceTitleSnapshot: isPdfActive
            ? docMeta?.title || "Tài liệu PDF"
            : lesson?.title || "Bài học",
          sourceQuoteSnapshot: quote || draftFront.trim(),
          frontText: draftFront.trim(),
          backText: draftBack.trim(),
          tagNames: tags,
        }),
      });

      if (resp.ok) {
        const createdCard = await resp.json();
        setFlashcards((prev) => [createdCard, ...prev]);
        setFlashcardModalOpen(false);
        setSelectedText("");
        setSelectionCoords(null);
        showToast("Đã thêm thẻ ghi nhớ mới.");
      } else {
        showToast(`Lưu thẻ thất bại (HTTP ${resp.status}).`);
      }
    } catch {
      showToast("Lỗi kết nối khi lưu thẻ.");
    } finally {
      setSavingCard(false);
    }
  };

  // 1-click quick flashcard creation from floating toolbar or practice lab
  const handleCreateFlashcardQuick = async (frontInput?: string, backInput?: string) => {
    const rawFront = (frontInput ?? selectedText).trim();
    if (!rawFront) return;

    const isPdfActive = (centerTab === "pdf" || !lesson) && Boolean(activeDocId);
    const words = rawFront.split(/\s+/).filter(Boolean);
    let derivedFront = rawFront;
    let derivedBack = (backInput ?? "").trim();

    if (!derivedBack) {
      if (words.length >= 4) {
        const idx = Math.min(words.length - 2, Math.max(1, Math.floor(words.length / 2)));
        const maskedWord = words[idx].replace(/[.,;:!?()"']/g, "");
        derivedFront = words.map((w, i) => (i === idx ? "[...]" : w)).join(" ");
        derivedBack = maskedWord || rawFront;
      } else {
        derivedFront = `Ý chính trong "${lesson?.title || docMeta?.title || "bài học"}":`;
        derivedBack = rawFront;
      }
    }

    setSavingCard(true);
    try {
      const resp = await fetch("/api/v1/flashcards", {
        method: "POST",
        headers: getHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          lessonId: isPdfActive ? null : lesson?.id,
          documentId: isPdfActive ? activeDocId : null,
          annotationId: null,
          cardType: !backInput && words.length >= 4 ? "cloze" : "basic",
          sourceTitleSnapshot: isPdfActive
            ? docMeta?.title || "Tài liệu PDF"
            : lesson?.title || "Bài học",
          sourceQuoteSnapshot: rawFront,
          frontText: derivedFront,
          backText: derivedBack,
          tagNames: [],
        }),
      });
      if (resp.ok) {
        const createdCard = await resp.json();
        setFlashcards((prev) => [createdCard, ...prev]);
        setSelectedText("");
        setSelectionCoords(null);
        showToast("Đã tạo thẻ ghi nhớ 1-click.");
      } else {
        showToast(`Tạo thẻ thất bại (HTTP ${resp.status}).`);
      }
    } catch {
      showToast("Lỗi kết nối khi tạo thẻ.");
    } finally {
      setSavingCard(false);
    }
  };

  const handleSelectPdfDocument = (docId: string, nextMode?: CenterViewTab) => {
    if (!docId) return;
    setActiveDocId(docId);
    setPdfPage(1);
    setPdfJumpInput("1");
    if (nextMode) {
      setCenterTab(nextMode);
    } else if (centerTab === "lesson" || centerTab === "practice") {
      setCenterTab(hasLesson ? "split" : "pdf");
    }
  };

  // Save excerpt or annotation citation into Notebook
  const handleSaveToNotebook = async (quoteText: string) => {
    const trimmed = quoteText.trim();
    if (!trimmed) return;
    if (!notebook) {
      showToast("Đang khởi tạo sổ tay, vui lòng thử lại sau giây lát.");
      return;
    }
    const citationLine =
      trimmed.startsWith("#") || trimmed.startsWith("-") || trimmed.startsWith(">")
        ? trimmed
        : `> "${trimmed}"`;
    const nextBody = noteBody.trim() ? `${noteBody.trim()}\n\n${citationLine}` : citationLine;
    setNoteBody(nextBody);
    setRightPanel("drawer");
    setDrawerTab("notebook");
    await saveNotebookNow(nextBody, undefined, "Đã lưu trích dẫn vào sổ tay.");
    setSelectedText("");
    setSelectionCoords(null);
  };

  const handleInsertCitationRefIntoNotebook = async (ann: AnnotationItem) => {
    if (!notebook) return;
    const isPdfActive = Boolean(ann.documentId && !ann.lessonId);
    const sourceTitle = isPdfActive
      ? docMeta?.title || "Tài liệu PDF"
      : lesson?.title || "Bài học";

    const nextAst: AstNode = {
      type: "doc",
      content: [
        ...(buildNotebookAst(noteTitle || notebook.title, noteBody, notebook.contentAst).content ??
          []),
        {
          type: "annotationRef",
          attrs: {
            block_id: crypto.randomUUID(),
            ref_annotation_id: ann.id,
            ref_lesson_id: isPdfActive ? null : lesson?.id ?? null,
            ref_document_id: isPdfActive ? activeDocId ?? null : null,
            page_number: isPdfActive ? ann.pageNumber ?? pdfPage : null,
            ref_block_id: ann.blockId ?? null,
            color_key: ann.colorKey,
            source_title: sourceTitle,
            source_quote: ann.exactQuote,
          },
        },
      ],
    };

    setRightPanel("drawer");
    setDrawerTab("notebook");
    await saveNotebookNow(noteBody, nextAst, "Đã ghim trích dẫn vào sổ tay.");
  };

  // AI Explain
  const handleExplainWithAi = async () => {
    const excerpt =
      selectedText.trim() ||
      currentPageData?.plainText?.slice(0, 180) ||
      lesson?.plainText?.slice(0, 180) ||
      "";
    if (!excerpt) {
      showToast("Hãy bôi đen đoạn văn bản cần giải thích.");
      return;
    }
    setAiExcerpt(excerpt);
    setAiExplanation("");
    setAiExplainOpen(true);
    setAiExplainLoading(true);
    try {
      const resp = await fetch("/api/v1/ai/explain", {
        method: "POST",
        headers: getHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          excerpt,
          contextTitle: lesson?.title || docMeta?.title || "Bài học",
          actionType: "explain",
        }),
      });
      if (resp.ok) {
        const data = await resp.json();
        setAiExplanation(data.explanation || "Không nhận được phản hồi giải thích.");
      } else {
        setAiExplanation("Không thể kết nối AI hoặc đã hết hạn mức sử dụng hôm nay.");
      }
    } catch {
      setAiExplanation("Lỗi kết nối khi yêu cầu AI giải thích.");
    } finally {
      setAiExplainLoading(false);
    }
  };

  // Export Notebook Markdown
  const handleExportNotebookMarkdown = async () => {
    if (!notebook) return;
    try {
      const resp = await fetch(`/api/v1/notebooks/${notebook.id}/export-markdown`, {
        headers: getHeaders(),
      });
      if (!resp.ok) {
        showToast("Không xuất được tệp Markdown.");
        return;
      }
      const data = await resp.json();
      const blob = new Blob([data.markdown || noteBody], {
        type: "text/markdown;charset=utf-8",
      });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `${(noteTitle || notebook.title || "so-tay").replace(
        /[^a-zA-Z0-9\u00C0-\u1EF9-_ ]+/g,
        "_"
      )}.md`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(url);
    } catch {
      showToast("Lỗi khi xuất Markdown.");
    }
  };

  // Relative markdown link opener
  const handleOpenRelativeMarkdown = async (resolvedRelativePath: string, label: string) => {
    if (!resolvedRelativePath) return;
    const folder = lesson?.sourceFolder || course?.sourceFolder;
    const qs = new URLSearchParams({
      relativePath: resolvedRelativePath,
      ...(!isStandalone ? { courseId } : {}),
      ...(lesson?.id ? { fromLessonId: lesson.id } : {}),
      ...(folder ? { sourceFolder: folder } : {}),
    });

    try {
      const res = await fetch(`/api/v1/lessons/by-path?${qs.toString()}`, {
        headers: getHeaders(),
      });
      if (!res.ok) {
        showToast(`Không mở được liên kết: ${label}`);
        return;
      }
      const matched = await res.json();
      if (matched?.found === false) {
        showToast(`Không tìm thấy nội dung "${label}".`);
        return;
      }
      if (matched?.isAssetMarkdown && matched?.contentMarkdown) {
        setRefModal({
          title: matched.title || label,
          markdown: matched.contentMarkdown,
          sourceFolder: matched.sourceFolder || folder || "",
          relativePath: matched.sourceRelativePath || resolvedRelativePath,
        });
        return;
      }
      const targetId = matched?.lessonId || matched?.id;
      if (targetId) {
        handleSelectLesson(targetId);
      }
    } catch {
      showToast("Lỗi khi mở tài liệu tham khảo.");
    }
  };

  // Jump to highlight element inside article
  const handleJumpToHighlight = (ann: AnnotationItem) => {
    if (ann.documentId && ann.pageNumber) {
      setPdfPage(ann.pageNumber);
      if (centerTab === "lesson") setCenterTab("split");
      return;
    }
    const el = document.getElementById(`reader-highlight-${ann.id}`);
    if (el) {
      el.scrollIntoView({ behavior: "smooth", block: "center" });
    }
  };

  const hasRoadmap = roadmapChapters.length > 0;
  const hasPdf = Boolean(activeDocId);
  const hasLesson = Boolean(activeLessonId && lesson);
  const videoInfo = getEmbedVideoInfo(lesson?.videoUrl);
  const notebookCitations = useMemo(
    () => extractCitationsFromAst(notebook?.contentAst),
    [notebook?.contentAst]
  );

  if (isInitialLoading) {
    return (
      <div className="h-screen flex items-center justify-center bg-bg-primary">
        <div className="flex flex-col items-center gap-3">
          <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
          <span className="text-xs text-text-tertiary">Đang chuẩn bị không gian học tập...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col bg-bg-primary text-text-primary overflow-hidden">
      {/* Minimal Top Bar */}
      <header className="h-12 shrink-0 border-b border-border bg-bg-primary px-3 sm:px-4 flex items-center justify-between gap-2 z-20">
        <div className="flex items-center gap-2 min-w-0">
          {hasRoadmap && (
            <button
              type="button"
              onClick={() => setRoadmapOpen((prev) => !prev)}
              className={`h-8 w-8 rounded-md flex items-center justify-center transition cursor-pointer ${
                roadmapOpen
                  ? "bg-accent-subtle text-accent"
                  : "text-text-secondary hover:text-text-primary hover:bg-bg-secondary"
              }`}
              title="Ẩn/hiện lộ trình khóa học (Ctrl+\)"
            >
              <PanelLeft className="w-4 h-4" />
            </button>
          )}

          <Link
            href={isStandalone ? "/app/documents" : `/app/courses/${courseId}`}
            className="inline-flex items-center gap-1.5 h-8 px-2 rounded-md text-xs sm:text-sm font-medium text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition max-w-[180px] sm:max-w-[240px]"
          >
            <ArrowLeft className="w-3.5 h-3.5 shrink-0" />
            <span className="truncate">
              {isStandalone ? "Tài liệu" : course?.title || "Khóa học"}
            </span>
          </Link>

          {(lesson?.title || docMeta?.title) && (
            <>
              <span className="text-text-tertiary hidden md:inline">/</span>
              <span className="hidden md:inline text-xs sm:text-sm font-medium text-text-primary truncate max-w-[260px]">
                {lesson?.title || docMeta?.title}
              </span>
            </>
          )}
        </div>

        {/* Right Controls */}
        <div className="flex items-center gap-1.5 shrink-0">
          {totalLessonCount > 0 && (
            <span className="hidden sm:inline-flex items-center h-7 px-2.5 rounded-md bg-bg-secondary border border-border text-xs font-medium text-text-secondary">
              {completedCount}/{totalLessonCount} • {percentCompleted}%
            </span>
          )}

          {/* Aa Typography Popover */}
          <div className="relative">
            <button
              type="button"
              onClick={() => setTypographyOpen((v) => !v)}
              className={`h-8 px-2.5 rounded-md border text-xs font-medium inline-flex items-center gap-1 transition cursor-pointer ${
                typographyOpen
                  ? "border-accent/40 bg-accent-subtle text-accent"
                  : "border-border bg-bg-primary text-text-secondary hover:text-text-primary hover:bg-bg-secondary"
              }`}
              title="Tùy chỉnh hiển thị chữ"
            >
              <Type className="w-3.5 h-3.5" />
              <span className="hidden sm:inline">Aa</span>
            </button>

            {typographyOpen && (
              <div className="absolute right-0 mt-1.5 w-64 rounded-lg border border-border bg-bg-primary shadow-lg p-3 z-50 space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-xs text-text-secondary">Cỡ chữ</span>
                  <div className="flex items-center gap-1.5">
                    <button
                      type="button"
                      onClick={() => setFontSizePx(Math.max(13, fontSizePx - 1))}
                      className="h-7 w-7 rounded border border-border text-xs hover:bg-bg-secondary cursor-pointer"
                    >
                      A-
                    </button>
                    <span className="text-xs font-medium w-10 text-center">{fontSizePx}px</span>
                    <button
                      type="button"
                      onClick={() => setFontSizePx(Math.min(22, fontSizePx + 1))}
                      className="h-7 w-7 rounded border border-border text-xs hover:bg-bg-secondary cursor-pointer"
                    >
                      A+
                    </button>
                  </div>
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-xs text-text-secondary">Kiểu chữ</span>
                  <button
                    type="button"
                    onClick={toggleSerifFont}
                    className="h-7 px-2.5 rounded border border-border text-xs font-medium hover:bg-bg-secondary cursor-pointer"
                  >
                    {serifFont ? "Có chân (Sách)" : "Hiện đại"}
                  </button>
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-xs text-text-secondary">Giao diện</span>
                  <button
                    type="button"
                    onClick={toggleDarkMode}
                    className="inline-flex items-center gap-1.5 h-7 px-2.5 rounded border border-border text-xs font-medium hover:bg-bg-secondary cursor-pointer"
                  >
                    {darkMode ? <Sun className="w-3.5 h-3.5" /> : <Moon className="w-3.5 h-3.5" />}
                    <span>{darkMode ? "Sáng" : "Tối"}</span>
                  </button>
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-xs text-text-secondary">Lọc ký tự nhiễu</span>
                  <button
                    type="button"
                    onClick={toggleCleanReadingMode}
                    className={`h-7 px-2.5 rounded border text-xs font-medium cursor-pointer ${
                      cleanReadingMode
                        ? "border-accent/40 bg-accent-subtle text-accent"
                        : "border-border text-text-secondary"
                    }`}
                  >
                    {cleanReadingMode ? "Bật" : "Tắt"}
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* TOC toggle */}
          {headings.length > 0 && (
            <button
              type="button"
              onClick={() => setRightPanel((curr) => (curr === "toc" ? "closed" : "toc"))}
              className={`inline-flex items-center gap-1.5 h-8 px-2.5 rounded-md border text-xs font-medium transition cursor-pointer ${
                rightPanel === "toc"
                  ? "border-accent/40 bg-accent-subtle text-accent"
                  : "border-border bg-bg-primary text-text-secondary hover:text-text-primary hover:bg-bg-secondary"
              }`}
            >
              <List className="w-3.5 h-3.5" />
              <span>Mục lục</span>
            </button>
          )}

          {/* Study Drawer toggle (Notes / Highlights / Flashcards) */}
          <button
            type="button"
            onClick={() => setRightPanel((curr) => (curr === "drawer" ? "closed" : "drawer"))}
            className={`inline-flex items-center gap-1.5 h-8 px-2.5 rounded-md border text-xs font-medium transition cursor-pointer ${
              rightPanel === "drawer"
                ? "border-accent/40 bg-accent-subtle text-accent"
                : "border-border bg-bg-primary text-text-secondary hover:text-text-primary hover:bg-bg-secondary"
            }`}
          >
            <NotebookPen className="w-3.5 h-3.5" />
            <span className="hidden sm:inline">Sổ tay & Thẻ</span>
            {effectiveAnnotations.length + flashcards.length > 0 && (
              <span className="px-1.5 py-0.5 rounded bg-bg-tertiary text-[10px] font-semibold">
                {effectiveAnnotations.length + flashcards.length}
              </span>
            )}
          </button>

          {!isStandalone && (
            (course?.quizCount ?? 0) === 0 ? (
              <button
                type="button"
                disabled
                data-testid="course-quiz-btn"
                title="Khóa học chưa có bài kiểm tra"
                className="hidden sm:inline-flex items-center gap-1.5 h-8 px-2.5 rounded-md border border-border bg-bg-secondary text-xs font-medium text-text-tertiary opacity-60 cursor-not-allowed"
              >
                <ClipboardCheck className="w-3.5 h-3.5 text-text-tertiary" />
                <span>Chưa có bài kiểm tra</span>
              </button>
            ) : (
              <Link
                href={`/app/courses/${courseId}/quiz`}
                data-testid="course-quiz-btn"
                className="hidden sm:inline-flex items-center gap-1.5 h-8 px-2.5 rounded-md border border-border bg-bg-primary text-xs font-medium text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition"
              >
                <ClipboardCheck className="w-3.5 h-3.5" />
                <span>Kiểm tra</span>
              </Link>
            )
          )}
        </div>
      </header>

      {/* Main 3-column Body */}
      <div className="flex-1 min-h-0 flex relative overflow-hidden">
        {/* Left: Course Roadmap */}
        {hasRoadmap && roadmapOpen && (
          <aside
            className={`w-72 shrink-0 border-r border-[var(--border-calm)] bg-[var(--canvas-bg)]/30 h-full overflow-hidden p-3 ${
              rightPanel !== "closed" ? "hidden xl:block" : "hidden md:block"
            }`}
          >
            <CourseRoadmapSidebar
              courseTitle={course?.title || "Khóa học"}
              chapters={roadmapChapters}
              activeLessonId={activeLessonId || ""}
              completedLessonIds={completedIds}
              percentCompleted={percentCompleted}
              completedCount={completedCount}
              totalCount={totalLessonCount}
              onSelectLesson={handleSelectLesson}
            />
          </aside>
        )}

        {/* Mobile Roadmap Overlay */}
        {hasRoadmap && roadmapOpen && (
          <div className="fixed inset-0 top-12 z-40 flex md:hidden">
            <div className="w-72 max-w-[85vw] h-full bg-[var(--surface-card)] border-r border-[var(--border-calm)] shadow-elevation-3 overflow-hidden p-3">
              <CourseRoadmapSidebar
                courseTitle={course?.title || "Khóa học"}
                chapters={roadmapChapters}
                activeLessonId={activeLessonId || ""}
                completedLessonIds={completedIds}
                percentCompleted={percentCompleted}
                completedCount={completedCount}
                totalCount={totalLessonCount}
                onSelectLesson={(id) => {
                  handleSelectLesson(id);
                  setRoadmapOpen(false);
                }}
              />
            </div>
            <button
              type="button"
              onClick={() => setRoadmapOpen(false)}
              className="flex-1 bg-black/40"
              aria-label="Đóng lộ trình"
            />
          </div>
        )}

        {/* Center Canvas - Reading column never shrinks below 320px (Bug B2) */}
        <main className="flex-1 min-w-[320px] flex flex-col bg-[var(--surface-card)] overflow-hidden relative">
          {/* Workspace Switcher Bar: Bài giảng / Thực hành / Tài liệu PDF / Song song */}
          {(hasLesson || hasPdf || availableDocs.length > 0) && (
            <div className="h-11 px-4 border-b border-border-subtle bg-bg-secondary/30 flex items-center justify-between gap-2 shrink-0 overflow-x-auto swms-thin-scrollbar flex-nowrap">
              <div className="inline-flex items-center rounded-md border border-border bg-bg-primary p-0.5 flex-nowrap gap-0.5 shrink-0">
                {hasLesson && (
                  <>
                    <button
                      type="button"
                      data-testid="center-tab-lesson"
                      onClick={() => setCenterTab("lesson")}
                      className={`inline-flex items-center gap-1.5 h-7 px-2.5 rounded text-xs font-medium transition cursor-pointer ${
                        centerTab === "lesson"
                          ? "bg-accent text-white"
                          : "text-text-secondary hover:text-text-primary"
                      }`}
                    >
                      <BookOpen className="w-3.5 h-3.5" />
                      <span>Bài giảng</span>
                    </button>
                    <button
                      type="button"
                      data-testid="center-tab-practice"
                      onClick={() => setCenterTab("practice")}
                      className={`inline-flex items-center gap-1.5 h-7 px-2.5 rounded text-xs font-medium transition cursor-pointer ${
                        centerTab === "practice"
                          ? "bg-accent text-white"
                          : "text-text-secondary hover:text-text-primary"
                      }`}
                    >
                      <FlaskConical className="w-3.5 h-3.5" />
                      <span>Thực hành</span>
                    </button>
                  </>
                )}

                {(hasPdf || availableDocs.length > 0) && (
                  <>
                    <button
                      type="button"
                      data-testid="center-tab-pdf"
                      onClick={() => {
                        if (!activeDocId && availableDocs.length > 0) {
                          handleSelectPdfDocument(availableDocs[0].id, "pdf");
                        } else {
                          setCenterTab("pdf");
                        }
                      }}
                      className={`inline-flex items-center gap-1.5 h-7 px-2.5 rounded text-xs font-medium transition cursor-pointer ${
                        centerTab === "pdf"
                          ? "bg-accent text-white"
                          : "text-text-secondary hover:text-text-primary"
                      }`}
                    >
                      <FileText className="w-3.5 h-3.5" />
                      <span>Tài liệu PDF</span>
                    </button>
                    {hasLesson && (
                      <button
                        type="button"
                        data-testid="center-tab-split"
                        onClick={() => {
                          if (!activeDocId && availableDocs.length > 0) {
                            handleSelectPdfDocument(availableDocs[0].id, "split");
                          } else {
                            setCenterTab("split");
                          }
                        }}
                        className={`inline-flex items-center gap-1.5 h-7 px-2.5 rounded text-xs font-medium transition cursor-pointer ${
                          centerTab === "split"
                            ? "bg-accent text-white"
                            : "text-text-secondary hover:text-text-primary"
                        }`}
                      >
                        <Layers className="w-3.5 h-3.5" />
                        <span>Song song</span>
                      </button>
                    )}
                  </>
                )}
              </div>

              {availableDocs.length > 0 ? (
                <div className="flex items-center gap-2 shrink-0">
                  <label htmlFor="learn-pdf-selector" className="text-[11px] text-text-tertiary hidden sm:inline">
                    PDF tham chiếu:
                  </label>
                  <select
                    id="learn-pdf-selector"
                    data-testid="learn-pdf-selector"
                    value={activeDocId || ""}
                    onChange={(e) => {
                      const chosen = e.target.value;
                      if (chosen) handleSelectPdfDocument(chosen);
                    }}
                    className="h-7 max-w-[230px] rounded-md border border-border bg-bg-primary px-2 text-xs text-text-primary focus:outline-none focus:ring-1 focus:ring-accent"
                  >
                    <option value="">Chọn PDF đọc song song...</option>
                    {availableDocs.map((d) => (
                      <option key={d.id} value={d.id}>
                        {d.title}
                      </option>
                    ))}
                  </select>
                </div>
              ) : (
                docMeta?.title && (
                  <span className="text-xs text-text-tertiary truncate hidden sm:inline">
                    {docMeta.title}
                  </span>
                )
              )}
            </div>
          )}

          {/* Toast & Errors */}
          {(toastMsg || pageError) && (
            <div className="px-4 pt-2 shrink-0">
              {pageError && (
                <div className="rounded-md border border-red-500/30 bg-red-500/5 px-3 py-2 text-xs text-red-600 dark:text-red-400 flex items-center justify-between">
                  <span className="flex items-center gap-1.5">
                    <AlertCircle className="w-3.5 h-3.5" />
                    {pageError}
                  </span>
                  <button
                    type="button"
                    onClick={() => setPageError(null)}
                    className="cursor-pointer"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                </div>
              )}
              {toastMsg && (
                <div className="rounded-md border border-accent/30 bg-accent-subtle px-3 py-2 text-xs font-medium text-accent flex items-center justify-between">
                  <span>{toastMsg}</span>
                  <button
                    type="button"
                    onClick={() => setToastMsg(null)}
                    className="cursor-pointer"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                </div>
              )}
            </div>
          )}

          {/* Contextual Floating Selection Toolbar */}
          {selectedText && (
            <div
              className="fixed z-40 rounded-lg border border-border bg-bg-primary shadow-lg px-2 py-1.5 flex flex-wrap items-center gap-1.5"
              style={
                selectionCoords
                  ? { top: selectionCoords.top, left: selectionCoords.left }
                  : { bottom: 20, left: "50%", transform: "translateX(-50%)" }
              }
            >
              <div className="flex items-center gap-1 pr-1.5 border-r border-border">
                {SEMANTIC_HIGHLIGHT_KEYS.map((cfg) => (
                  <button
                    key={cfg.key}
                    type="button"
                    onClick={() => void handleCreateHighlight(cfg.key)}
                    title={`Đánh dấu: ${cfg.label}`}
                    className={`w-4 h-4 rounded-full border cursor-pointer hover:scale-110 transition ${
                      COLOR_DOT_CLASSES[cfg.key]
                    }`}
                  />
                ))}
              </div>
              <button
                type="button"
                data-testid="quick-1click-flashcard-btn"
                disabled={savingCard}
                onClick={() => void handleCreateFlashcardQuick()}
                title="Tạo ngay thẻ ghi nhớ từ đoạn đã bôi đen"
                className="h-7 px-2 rounded text-xs font-medium text-amber-600 dark:text-amber-400 hover:bg-amber-500/10 transition cursor-pointer inline-flex items-center gap-1"
              >
                <Zap className="w-3.5 h-3.5" />
                <span>Tạo thẻ 1-click</span>
              </button>
              <button
                type="button"
                onClick={() => handleOpenFlashcardModal()}
                className="h-7 px-2 rounded text-xs font-medium text-text-primary hover:bg-bg-secondary transition cursor-pointer inline-flex items-center gap-1"
              >
                <Plus className="w-3.5 h-3.5 text-accent" />
                <span>Tùy chỉnh thẻ</span>
              </button>
              <button
                type="button"
                onClick={() => void handleSaveToNotebook(selectedText)}
                className="h-7 px-2 rounded text-xs font-medium text-text-primary hover:bg-bg-secondary transition cursor-pointer inline-flex items-center gap-1"
              >
                <NotebookPen className="w-3.5 h-3.5 text-text-secondary" />
                <span>Lưu vào sổ tay</span>
              </button>
              <button
                type="button"
                onClick={() => void handleExplainWithAi()}
                className="h-7 px-2 rounded text-xs font-medium text-accent hover:bg-accent-subtle transition cursor-pointer inline-flex items-center gap-1"
              >
                <Sparkles className="w-3.5 h-3.5" />
                <span>Giải thích</span>
              </button>
              <button
                type="button"
                onClick={() => {
                  setSelectedText("");
                  setSelectionCoords(null);
                }}
                className="p-1 text-text-tertiary hover:text-text-primary cursor-pointer"
                aria-label="Đóng thanh chọn"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            </div>
          )}

          {/* Reading / Practice / Split Workspace Body */}
          <div
            className={`flex-1 min-h-0 flex ${
              centerTab === "split" ? "flex-col lg:flex-row divide-y lg:divide-y-0 lg:divide-x divide-border" : ""
            }`}
          >
            {/* Interactive Practice Lab */}
            {centerTab === "practice" && hasLesson && lesson && (
              <div className="flex-1 min-h-0 overflow-y-auto swms-thin-scrollbar">
                <LessonPracticeLab
                  courseId={courseId}
                  courseTitle={course?.title || "Khóa học"}
                  lessonId={lesson.id}
                  lessonTitle={lesson.title}
                  lessonMarkdown={lesson.contentMarkdown || ""}
                  onSaveToNotebook={handleSaveToNotebook}
                  onCreateFlashcardQuick={(front, back) => handleCreateFlashcardQuick(front, back)}
                />
              </div>
            )}

            {/* Lesson Pane */}
            {((centerTab === "lesson" || centerTab === "split") ||
              (centerTab !== "practice" && !hasPdf && availableDocs.length === 0)) && (
              <div
                ref={readerScrollRef}
                data-swms-scroll-container="learn-page"
                onMouseUp={captureSelectionInReader}
                onKeyUp={captureSelectionInReader}
                className="flex-1 min-h-0 overflow-y-auto swms-thin-scrollbar"
              >
                {lessonLoading ? (
                  <div className="flex items-center justify-center py-20">
                    <Loader2 className="w-5 h-5 animate-spin text-accent" />
                  </div>
                ) : hasLesson && lesson ? (
                  <article
                    data-testid="reading-prose-column"
                    className={`max-w-[72ch] mx-auto px-4 sm:px-8 py-8 ${
                      serifFont ? "font-serif" : ""
                    }`}
                    style={{ fontSize: `${fontSizePx}px` }}
                  >
                    {/* Clean Lesson Header */}
                    <header className="mb-6 pb-5 border-b border-border-subtle">
                      <div className="flex flex-wrap items-center gap-2 text-xs text-text-tertiary mb-2">
                        {activeChapterTitle && (
                          <span className="px-2 py-0.5 rounded bg-bg-secondary border border-border">
                            {activeChapterTitle}
                          </span>
                        )}
                        {lesson.estimatedMinutes && (
                          <span className="inline-flex items-center gap-1">
                            <Clock className="w-3.5 h-3.5" />
                            {lesson.estimatedMinutes} phút đọc
                          </span>
                        )}
                      </div>
                      <h1 className="text-2xl sm:text-3xl font-semibold tracking-tight text-text-primary leading-snug">
                        {lesson.title}
                      </h1>
                    </header>

                    {/* Video Embed */}
                    {videoInfo && (
                      <div className="mb-7 rounded-lg overflow-hidden border border-border bg-black aspect-video">
                        {videoInfo.kind === "iframe" ? (
                          <iframe
                            src={videoInfo.src}
                            title={lesson.title}
                            className="w-full h-full"
                            allowFullScreen
                          />
                        ) : (
                          <video src={videoInfo.src} controls className="w-full h-full" />
                        )}
                      </div>
                    )}

                    {/* Clean Markdown Reader */}
                    <LessonMarkdownView
                      markdown={lesson.contentMarkdown || lesson.plainText || ""}
                      lessonTitle={lesson.title}
                      sourceFolder={lesson.sourceFolder || course?.sourceFolder}
                      sourceRelativePath={lesson.sourceRelativePath}
                      cleanReadingMode={cleanReadingMode}
                      hideInlineCitationBadges={true}
                      anchoredAnnotations={anchoredAnnotations}
                      onSelectBlockText={(blockId) => {
                        setSelectedBlockId(blockId);
                        captureSelectionInReader();
                      }}
                      onHighlightClick={(quote) => {
                        setRightPanel("drawer");
                        setDrawerTab("highlights");
                        showToast(`Đang xem đánh dấu: "${quote.slice(0, 48)}..."`);
                      }}
                      onInsertCitation={(ann) => {
                        const matched = effectiveAnnotations.find((a) => a.id === ann.id);
                        if (matched) void handleInsertCitationRefIntoNotebook(matched);
                      }}
                      onOpenRelativeMarkdown={handleOpenRelativeMarkdown}
                      onHeadingsExtracted={setHeadings}
                    />

                    {/* Lesson Navigation & Completion Footer */}
                    <footer className="mt-12 pt-6 border-t border-border flex flex-wrap items-center justify-between gap-3">
                      <div>
                        {prevLessonId && (
                          <button
                            type="button"
                            onClick={() => handleSelectLesson(prevLessonId)}
                            className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-md border border-border bg-bg-secondary text-sm font-medium text-text-primary hover:bg-bg-tertiary transition cursor-pointer"
                          >
                            <ChevronLeft className="w-4 h-4" />
                            <span>Bài trước</span>
                          </button>
                        )}
                      </div>

                      <div className="flex flex-wrap items-center gap-2">
                        <button
                          type="button"
                          disabled={isCompleting}
                          onClick={handleMarkComplete}
                          data-testid="mark-lesson-complete-btn"
                          className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover disabled:opacity-60 transition cursor-pointer"
                        >
                          {isCompleting ? (
                            <Loader2 className="w-4 h-4 animate-spin" />
                          ) : (
                            <CheckCircle2 className="w-4 h-4" />
                          )}
                          <span>
                            {nextLessonId
                              ? "Đánh dấu hoàn thành & Học bài tiếp"
                              : "Hoàn thành khóa học"}
                          </span>
                        </button>

                        {nextLessonId && (
                          <button
                            type="button"
                            onClick={() => handleSelectLesson(nextLessonId)}
                            className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-md border border-border bg-bg-primary text-sm font-medium text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition cursor-pointer"
                          >
                            <span>Bài tiếp theo</span>
                            <ChevronRight className="w-4 h-4" />
                          </button>
                        )}
                      </div>
                    </footer>
                  </article>
                ) : !hasPdf ? (
                  <div className="max-w-lg mx-auto px-4 py-16 text-center">
                    <BookOpen className="w-8 h-8 text-text-tertiary mx-auto mb-3" />
                    <p className="text-sm font-medium text-text-primary mb-1">
                      Chưa có bài giảng để hiển thị
                    </p>
                    <p className="text-xs text-text-tertiary mb-4">
                      Hãy thêm bài học từ trang biên tập hoặc chọn một bài trong lộ trình.
                    </p>
                    {!isStandalone && (
                      <Link
                        href={`/app/courses/${courseId}/edit`}
                        className="inline-flex items-center h-8 px-3.5 rounded-md bg-accent text-white text-xs font-medium"
                      >
                        Biên tập khóa học
                      </Link>
                    )}
                  </div>
                ) : null}
              </div>
            )}

            {/* PDF Viewer Pane */}
            {hasPdf && (centerTab === "pdf" || centerTab === "split") && (
              <div
                onMouseUp={captureSelectionInReader}
                className="flex-1 min-h-0 flex flex-col bg-bg-secondary/20 overflow-hidden"
              >
                {/* PDF Controls Bar */}
                <div className="px-3 py-2 border-b border-border bg-bg-primary flex flex-wrap items-center justify-between gap-2 shrink-0">
                  <div className="flex items-center gap-1.5">
                    <button
                      type="button"
                      disabled={pdfPage <= 1}
                      onClick={() => setPdfPage((p) => Math.max(1, p - 1))}
                      className="h-7 w-7 rounded border border-border flex items-center justify-center text-text-secondary hover:text-text-primary disabled:opacity-40 cursor-pointer"
                      aria-label="Trang trước"
                    >
                      <ChevronLeft className="w-4 h-4" />
                    </button>
                    <form
                      onSubmit={(e) => {
                        e.preventDefault();
                        const parsed = parseInt(pdfJumpInput, 10);
                        if (!isNaN(parsed)) {
                          setPdfPage(Math.max(1, Math.min(pdfTotalPages, parsed)));
                        }
                      }}
                      className="flex items-center gap-1 text-xs"
                    >
                      <input
                        type="text"
                        value={pdfJumpInput}
                        onChange={(e) => setPdfJumpInput(e.target.value)}
                        className="w-11 h-7 text-center rounded border border-border bg-bg-secondary text-text-primary"
                      />
                      <span className="text-text-tertiary">/ {pdfTotalPages}</span>
                    </form>
                    <button
                      type="button"
                      disabled={pdfPage >= pdfTotalPages}
                      onClick={() => setPdfPage((p) => Math.min(pdfTotalPages, p + 1))}
                      className="h-7 w-7 rounded border border-border flex items-center justify-center text-text-secondary hover:text-text-primary disabled:opacity-40 cursor-pointer"
                      aria-label="Trang sau"
                    >
                      <ChevronRight className="w-4 h-4" />
                    </button>
                  </div>

                  <div className="flex items-center gap-2">
                    <div className="flex items-center gap-1">
                      <button
                        type="button"
                        onClick={() => setPdfZoom((z) => Math.max(60, z - 15))}
                        className="h-7 w-7 rounded border border-border flex items-center justify-center text-text-secondary hover:text-text-primary cursor-pointer"
                      >
                        <ZoomOut className="w-3.5 h-3.5" />
                      </button>
                      <span className="text-xs text-text-secondary w-10 text-center">
                        {pdfZoom}%
                      </span>
                      <button
                        type="button"
                        onClick={() => setPdfZoom((z) => Math.min(200, z + 15))}
                        className="h-7 w-7 rounded border border-border flex items-center justify-center text-text-secondary hover:text-text-primary cursor-pointer"
                      >
                        <ZoomIn className="w-3.5 h-3.5" />
                      </button>
                    </div>

                    <div className="inline-flex rounded border border-border bg-bg-secondary p-0.5 text-xs">
                      <button
                        type="button"
                        onClick={() => setPdfRenderMode("canvas")}
                        className={`h-6 px-2 rounded cursor-pointer ${
                          pdfRenderMode === "canvas"
                            ? "bg-bg-primary text-text-primary font-medium shadow-sm"
                            : "text-text-secondary"
                        }`}
                      >
                        Bản gốc
                      </button>
                      <button
                        type="button"
                        onClick={() => setPdfRenderMode("text")}
                        className={`h-6 px-2 rounded cursor-pointer ${
                          pdfRenderMode === "text"
                            ? "bg-bg-primary text-text-primary font-medium shadow-sm"
                            : "text-text-secondary"
                        }`}
                      >
                        Văn bản
                      </button>
                    </div>
                  </div>
                </div>

                {/* PDF Canvas or Text */}
                <div className="flex-1 overflow-auto p-4 swms-thin-scrollbar flex justify-center">
                  {pdfLoadError ? (
                    <div className="max-w-md w-full my-auto p-6 rounded-xl border border-rose-300 dark:border-rose-800 bg-rose-50 dark:bg-rose-950/30 text-center space-y-4">
                      <div className="w-10 h-10 rounded-full bg-rose-100 dark:bg-rose-900/50 text-rose-600 dark:text-rose-400 flex items-center justify-center mx-auto">
                        <AlertCircle className="w-5 h-5" />
                      </div>
                      <div>
                        <h4 className="text-sm font-semibold text-rose-800 dark:text-rose-200 mb-1">
                          Không thể hiển thị tài liệu PDF
                        </h4>
                        <p className="text-xs text-rose-700 dark:text-rose-300">
                          {pdfLoadError}
                        </p>
                      </div>
                      <div className="flex items-center justify-center gap-2.5 pt-1">
                        <button
                          type="button"
                          onClick={() => {
                            setPdfLoadError(null);
                            if (activeDocId) handleSelectPdfDocument(activeDocId);
                          }}
                          className="h-8 px-3 rounded-md bg-rose-600 hover:bg-rose-700 text-white text-xs font-medium transition cursor-pointer"
                        >
                          Thử tải lại
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            setCenterTab("lesson");
                            setActiveDocId(null);
                            setPdfLoadError(null);
                          }}
                          className="h-8 px-3 rounded-md border border-border bg-bg-primary text-text-primary text-xs font-medium hover:bg-bg-secondary transition cursor-pointer"
                        >
                          Quay lại bài giảng
                        </button>
                      </div>
                    </div>
                  ) : pdfRenderMode === "canvas" ? (
                    pdfDoc ? (
                      <canvas
                        ref={canvasRef}
                        className="shadow-md border border-border rounded bg-white max-w-full h-fit"
                      />
                    ) : (
                      <div className="py-12 text-xs text-text-tertiary flex items-center gap-2">
                        <Loader2 className="w-4 h-4 animate-spin" />
                        <span>Đang tải tệp PDF...</span>
                      </div>
                    )
                  ) : (
                    <div
                      className={`max-w-[68ch] w-full bg-bg-primary rounded-lg border border-border p-6 h-fit leading-relaxed ${
                        serifFont ? "font-serif" : ""
                      }`}
                      style={{ fontSize: `${fontSizePx}px` }}
                    >
                      {pageLoading ? (
                        <div className="flex items-center gap-2 text-xs text-text-tertiary">
                          <Loader2 className="w-4 h-4 animate-spin" />
                          <span>Đang trích xuất chữ trang {pdfPage}...</span>
                        </div>
                      ) : currentPageData?.plainText ? (
                        <p className="whitespace-pre-wrap text-text-primary">
                          {currentPageData.plainText}
                        </p>
                      ) : (
                        <p className="text-sm text-text-tertiary">
                          Trang này không có lớp văn bản trích xuất sẵn. Chuyển sang &ldquo;Bản gốc&rdquo; để xem hình ảnh trang PDF.
                        </p>
                      )}
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        </main>

        {/* Right Contextual Panel: TOC */}
        {rightPanel === "toc" && headings.length > 0 && (
          <aside className="w-64 shrink-0 border-l border-border bg-[var(--surface-card)] lg:bg-bg-secondary/20 flex flex-col fixed lg:static right-0 top-14 bottom-0 z-40 shadow-lg lg:shadow-none">
            <div className="px-4 py-3 border-b border-border flex items-center justify-between">
              <span className="text-xs font-semibold uppercase tracking-wider text-text-tertiary">
                Trong bài này
              </span>
              <button
                type="button"
                onClick={() => setRightPanel("closed")}
                className="text-text-tertiary hover:text-text-primary cursor-pointer"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            </div>
            <nav className="flex-1 overflow-y-auto swms-thin-scrollbar p-3 space-y-1">
              {headings.map((h) => (
                <button
                  key={h.id}
                  type="button"
                  onClick={() => {
                    document.getElementById(h.id)?.scrollIntoView({
                      behavior: "smooth",
                      block: "start",
                    });
                  }}
                  className="block w-full text-left text-xs text-text-secondary hover:text-accent py-1.5 rounded transition truncate cursor-pointer"
                  style={{ paddingLeft: `${Math.max(0, (h.level - 1) * 10)}px` }}
                >
                  {h.text}
                </button>
              ))}
            </nav>
          </aside>
        )}

        {/* Right Contextual Panel: 3-Tab Study Drawer (Bottom-sheet/overlay on < 768px, inline on desktop) */}
        {rightPanel === "drawer" && (
          <aside className="w-full md:w-96 shrink-0 border-l border-[var(--border-calm)] bg-[var(--surface-card)] flex flex-col h-full fixed inset-x-0 bottom-0 top-12 md:static md:top-auto z-40 shadow-elevation-2">
            {/* Drawer Tab Header */}
            <div className="px-3 py-2 border-b border-border bg-bg-secondary/30 flex items-center justify-between gap-2 shrink-0">
              <div className="flex items-center gap-1">
                {(
                  [
                    { id: "notebook", label: "Sổ tay", icon: NotebookPen },
                    {
                      id: "highlights",
                      label: `Đánh dấu (${effectiveAnnotations.length})`,
                      icon: Highlighter,
                    },
                    {
                      id: "flashcards",
                      label: `Thẻ (${flashcards.length})`,
                      icon: Brain,
                    },
                  ] as const
                ).map((tab) => {
                  const Icon = tab.icon;
                  return (
                    <button
                      key={tab.id}
                      type="button"
                      onClick={() => setDrawerTab(tab.id)}
                      className={`inline-flex items-center gap-1.5 h-7 px-2.5 rounded text-xs font-medium transition cursor-pointer ${
                        drawerTab === tab.id
                          ? "bg-bg-primary text-accent border border-border shadow-xs"
                          : "text-text-secondary hover:text-text-primary"
                      }`}
                    >
                      <Icon className="w-3.5 h-3.5" />
                      <span>{tab.label}</span>
                    </button>
                  );
                })}
              </div>

              <button
                type="button"
                onClick={() => setRightPanel("closed")}
                className="p-1 rounded text-text-tertiary hover:text-text-primary cursor-pointer"
                aria-label="Đóng ngăn ghi chú"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {/* Tab 1: Notebook */}
            {drawerTab === "notebook" && (
              <div className="flex-1 min-h-0 flex flex-col p-3.5 gap-3">
                <div className="flex items-center justify-between gap-2">
                  <input
                    type="text"
                    value={noteTitle}
                    onChange={(e) => {
                      setNoteTitle(e.target.value);
                      setNoteDirty(true);
                    }}
                    placeholder="Tiêu đề sổ tay..."
                    className="flex-1 h-8 px-2.5 rounded-md border border-border bg-bg-secondary text-sm font-medium text-text-primary focus:outline-none focus:border-accent"
                  />
                  <button
                    type="button"
                    disabled={!notebook || noteSaveStatus === "saving"}
                    onClick={() => void saveNotebookNow(undefined, undefined, "Đã lưu ghi chú.")}
                    className="inline-flex items-center gap-1 h-8 px-2.5 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover disabled:opacity-50 cursor-pointer shrink-0"
                  >
                    <Save className="w-3.5 h-3.5" />
                    <span>Lưu</span>
                  </button>
                </div>

                {noteErrorMsg && (
                  <p className="text-xs text-red-600 dark:text-red-400 bg-red-500/5 border border-red-500/20 rounded px-2.5 py-1.5">
                    {noteErrorMsg}
                  </p>
                )}

                <textarea
                  value={noteBody}
                  onChange={(e) => {
                    setNoteBody(e.target.value);
                    setNoteDirty(true);
                  }}
                  placeholder="Viết ghi chú bài học tại đây... Bạn cũng có thể bôi đen đoạn văn bên trái để lưu trích dẫn."
                  className="flex-1 min-h-[220px] w-full p-3 rounded-md border border-border bg-bg-secondary/40 text-sm leading-relaxed text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-accent resize-none swms-thin-scrollbar"
                />

                {notebookCitations.length > 0 && (
                  <div className="max-h-36 overflow-y-auto swms-thin-scrollbar space-y-1.5 pt-1 border-t border-border-subtle">
                    <p className="text-[11px] font-medium uppercase tracking-wider text-text-tertiary">
                      Trích dẫn đã ghim ({notebookCitations.length})
                    </p>
                    {notebookCitations.map((c) => (
                      <div
                        key={c.key}
                        className="text-xs rounded border border-border bg-bg-secondary/50 p-2"
                      >
                        <p className="text-text-primary italic line-clamp-2">
                          &ldquo;{c.quote}&rdquo;
                        </p>
                        {c.sourceTitle && (
                          <p className="text-[11px] text-text-tertiary mt-1">
                            {c.sourceTitle}
                            {c.pageNumber ? ` • Trang ${c.pageNumber}` : ""}
                          </p>
                        )}
                      </div>
                    ))}
                  </div>
                )}

                <div className="flex items-center justify-between text-[11px] text-text-tertiary pt-1 border-t border-border-subtle">
                  <span>
                    {noteSaveStatus === "saving"
                      ? "Đang lưu..."
                      : noteSaveStatus === "saved"
                        ? `Đã lưu • Phiên bản ${notebook?.version ?? 1}`
                        : noteDirty
                          ? "Chưa lưu thay đổi"
                          : notebook
                            ? `Phiên bản ${notebook.version}`
                            : "Đang tải sổ tay..."}
                  </span>
                  <button
                    type="button"
                    disabled={!notebook}
                    onClick={handleExportNotebookMarkdown}
                    className="inline-flex items-center gap-1 hover:text-text-primary transition cursor-pointer"
                  >
                    <Download className="w-3 h-3" />
                    <span>Xuất .md</span>
                  </button>
                </div>
              </div>
            )}

            {/* Tab 2: Highlights */}
            {drawerTab === "highlights" && (
              <div className="flex-1 overflow-y-auto swms-thin-scrollbar p-3.5 space-y-2.5">
                {effectiveAnnotations.length > 0 ? (
                  effectiveAnnotations.map((ann) => (
                    <div
                      key={ann.id}
                      className="rounded-lg border border-border bg-bg-secondary/40 p-3 space-y-2"
                    >
                      <div className="flex items-center justify-between gap-2">
                        <span className="inline-flex items-center gap-1.5 text-[11px] font-medium text-text-secondary">
                          <span
                            className={`w-2.5 h-2.5 rounded-full border ${
                              COLOR_DOT_CLASSES[ann.colorKey || "yellow"]
                            }`}
                          />
                          {ann.semanticLabel || "Đánh dấu"}
                          {ann.pageNumber ? ` • Tr. ${ann.pageNumber}` : ""}
                        </span>
                        <button
                          type="button"
                          onClick={() => void handleDeleteAnnotation(ann.id)}
                          className="p-1 rounded text-text-tertiary hover:text-red-500 cursor-pointer"
                          title="Xóa đánh dấu"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>

                      <button
                        type="button"
                        onClick={() => handleJumpToHighlight(ann)}
                        className="text-left text-xs text-text-primary hover:text-accent transition leading-relaxed block w-full cursor-pointer"
                      >
                        &ldquo;{ann.exactQuote}&rdquo;
                      </button>

                      <div className="flex items-center gap-2 pt-1">
                        <button
                          type="button"
                          onClick={() => void handleInsertCitationRefIntoNotebook(ann)}
                          className="text-[11px] font-medium text-text-secondary hover:text-accent cursor-pointer"
                        >
                          + Chèn vào sổ tay
                        </button>
                        <span className="text-text-tertiary">•</span>
                        <button
                          type="button"
                          onClick={() => handleOpenFlashcardModal(ann.exactQuote)}
                          className="text-[11px] font-medium text-text-secondary hover:text-accent cursor-pointer"
                        >
                          + Tạo thẻ
                        </button>
                      </div>
                    </div>
                  ))
                ) : (
                  <div className="py-10 text-center">
                    <Highlighter className="w-7 h-7 text-text-tertiary mx-auto mb-2" />
                    <p className="text-xs font-medium text-text-primary mb-1">
                      Chưa có đoạn đánh dấu nào
                    </p>
                    <p className="text-xs text-text-tertiary">
                      Bôi đen câu quan trọng trong bài giảng để tô màu và lưu lại tại đây.
                    </p>
                  </div>
                )}
              </div>
            )}

            {/* Tab 3: Flashcards */}
            {drawerTab === "flashcards" && (
              <div className="flex-1 min-h-0 flex flex-col p-3.5 gap-3">
                <div className="flex items-center justify-between gap-2">
                  <button
                    type="button"
                    onClick={() => handleOpenFlashcardModal("")}
                    className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover transition cursor-pointer"
                  >
                    <Plus className="w-3.5 h-3.5" />
                    <span>Tạo thẻ mới</span>
                  </button>
                  <Link
                    href={
                      isStandalone ? "/app/review" : `/app/review?courseId=${courseId}`
                    }
                    className="inline-flex items-center gap-1 text-xs font-medium text-accent hover:underline"
                  >
                    <span>Ôn tập thẻ</span>
                    <ArrowRight className="w-3.5 h-3.5" />
                  </Link>
                </div>

                <div className="flex-1 overflow-y-auto swms-thin-scrollbar space-y-2.5">
                  {flashcards.length > 0 ? (
                    flashcards.map((fc) => (
                      <div
                        key={fc.id}
                        className="rounded-lg border border-border bg-bg-secondary/40 p-3 space-y-1.5"
                      >
                        <p className="text-xs font-semibold text-text-primary">{fc.frontText}</p>
                        <p className="text-xs text-text-secondary border-t border-border-subtle pt-1.5">
                          {fc.backText}
                        </p>
                      </div>
                    ))
                  ) : (
                    <div className="py-10 text-center">
                      <Brain className="w-7 h-7 text-text-tertiary mx-auto mb-2" />
                      <p className="text-xs font-medium text-text-primary mb-1">
                        Chưa có thẻ ghi nhớ cho bài này
                      </p>
                      <p className="text-xs text-text-tertiary">
                        Bôi đen văn bản trong bài hoặc bấm &ldquo;Tạo thẻ mới&rdquo; để tự kiểm tra sau.
                      </p>
                    </div>
                  )}
                </div>
              </div>
            )}
          </aside>
        )}
      </div>

      {/* Modal 1: Flashcard Creator */}
      {flashcardModalOpen && (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center p-4">
          <div className="w-full max-w-lg rounded-xl border border-border bg-bg-primary shadow-xl p-5 space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-base font-semibold text-text-primary">Tạo thẻ ghi nhớ mới</h2>
              <button
                type="button"
                onClick={() => setFlashcardModalOpen(false)}
                className="text-text-tertiary hover:text-text-primary cursor-pointer"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            <div className="flex items-center gap-1.5">
              <button
                type="button"
                onClick={() => setDraftMode("cloze")}
                className={`h-8 px-3 rounded-md text-xs font-medium cursor-pointer border ${
                  draftMode === "cloze"
                    ? "border-accent bg-accent-subtle text-accent"
                    : "border-border text-text-secondary"
                }`}
              >
                Điền từ còn thiếu
              </button>
              <button
                type="button"
                onClick={() => setDraftMode("manual")}
                className={`h-8 px-3 rounded-md text-xs font-medium cursor-pointer border ${
                  draftMode === "manual"
                    ? "border-accent bg-accent-subtle text-accent"
                    : "border-border text-text-secondary"
                }`}
              >
                Hỏi & Đáp
              </button>
              <button
                type="button"
                disabled={aiCardLoading}
                onClick={() => void handleGenerateAiCard()}
                className={`inline-flex items-center gap-1 h-8 px-3 rounded-md text-xs font-medium cursor-pointer border ${
                  draftMode === "ai"
                    ? "border-accent bg-accent-subtle text-accent"
                    : "border-border text-text-secondary hover:text-text-primary"
                }`}
              >
                {aiCardLoading ? (
                  <Loader2 className="w-3.5 h-3.5 animate-spin" />
                ) : (
                  <Sparkles className="w-3.5 h-3.5" />
                )}
                <span>Gợi ý AI</span>
              </button>
            </div>

            <div className="space-y-3">
              <div>
                <label className="block text-xs font-medium text-text-secondary mb-1">
                  Mặt trước (Câu hỏi hoặc câu cần điền)
                </label>
                <textarea
                  rows={3}
                  value={draftFront}
                  onChange={(e) => setDraftFront(e.target.value)}
                  placeholder="Nhập câu hỏi hoặc dùng {{c1::từ cần nhớ}}..."
                  className="w-full p-2.5 rounded-md border border-border bg-bg-secondary text-sm text-text-primary focus:outline-none focus:border-accent"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-text-secondary mb-1">
                  Mặt sau (Đáp án & giải thích)
                </label>
                <textarea
                  rows={3}
                  value={draftBack}
                  onChange={(e) => setDraftBack(e.target.value)}
                  placeholder="Nhập đáp án ngắn gọn, dễ nhớ..."
                  className="w-full p-2.5 rounded-md border border-border bg-bg-secondary text-sm text-text-primary focus:outline-none focus:border-accent"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-text-secondary mb-1">
                  Nhãn phân loại (cách nhau bởi dấu phẩy)
                </label>
                <input
                  type="text"
                  value={draftTagsText}
                  onChange={(e) => setDraftTagsText(e.target.value)}
                  className="w-full h-8 px-2.5 rounded-md border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                />
              </div>
            </div>

            <div className="flex items-center justify-end gap-2 pt-2">
              <button
                type="button"
                onClick={() => setFlashcardModalOpen(false)}
                className="h-9 px-3.5 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary cursor-pointer"
              >
                Hủy
              </button>
              <button
                type="button"
                disabled={savingCard || !draftFront.trim() || !draftBack.trim()}
                onClick={() => void handleSaveFlashcard()}
                className="h-9 px-4 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover disabled:opacity-50 cursor-pointer"
              >
                {savingCard ? "Đang lưu..." : "Lưu thẻ ghi nhớ"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal 2: AI Explanation */}
      {aiExplainOpen && (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center p-4">
          <div className="w-full max-w-lg rounded-xl border border-border bg-bg-primary shadow-xl p-5 space-y-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2 text-sm font-semibold text-text-primary">
                <Sparkles className="w-4 h-4 text-accent" />
                <span>Giải thích nhanh</span>
              </div>
              <button
                type="button"
                onClick={() => setAiExplainOpen(false)}
                className="text-text-tertiary hover:text-text-primary cursor-pointer"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            <div className="rounded-md bg-bg-secondary border border-border p-3 text-xs text-text-secondary italic line-clamp-3">
              &ldquo;{aiExcerpt}&rdquo;
            </div>

            <div className="max-h-72 overflow-y-auto swms-thin-scrollbar text-sm text-text-primary leading-relaxed whitespace-pre-wrap">
              {aiExplainLoading ? (
                <div className="flex items-center gap-2 text-text-tertiary py-6 justify-center">
                  <Loader2 className="w-4 h-4 animate-spin text-accent" />
                  <span>Đang phân tích đoạn nội dung...</span>
                </div>
              ) : (
                aiExplanation
              )}
            </div>

            <div className="flex items-center justify-end gap-2">
              {aiExplanation && !aiExplainLoading && (
                <button
                  type="button"
                  onClick={() => {
                    void handleSaveToNotebook(`${aiExcerpt}\n→ Giải thích: ${aiExplanation}`);
                    setAiExplainOpen(false);
                  }}
                  className="h-8 px-3 rounded-md border border-border bg-bg-secondary text-xs font-medium text-text-primary hover:bg-bg-tertiary cursor-pointer"
                >
                  Lưu vào sổ tay
                </button>
              )}
              <button
                type="button"
                onClick={() => setAiExplainOpen(false)}
                className="h-8 px-3.5 rounded-md bg-accent text-white text-xs font-medium cursor-pointer"
              >
                Đóng
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal 3: Relative Reference Markdown */}
      {refModal && (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center p-4">
          <div className="w-full max-w-3xl max-h-[85vh] rounded-xl border border-border bg-bg-primary shadow-xl flex flex-col overflow-hidden">
            <div className="px-5 py-3.5 border-b border-border flex items-center justify-between">
              <h2 className="text-sm font-semibold text-text-primary truncate">{refModal.title}</h2>
              <button
                type="button"
                onClick={() => setRefModal(null)}
                className="text-text-tertiary hover:text-text-primary cursor-pointer"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
            <div className="flex-1 overflow-y-auto swms-thin-scrollbar p-6">
              <LessonMarkdownView
                markdown={refModal.markdown}
                lessonTitle={refModal.title}
                sourceFolder={refModal.sourceFolder}
                sourceRelativePath={refModal.relativePath}
                cleanReadingMode={cleanReadingMode}
                hideInlineCitationBadges={true}
                anchoredAnnotations={[]}
                onSelectBlockText={() => {}}
                onHighlightClick={() => {}}
                onInsertCitation={() => {}}
              />
            </div>
          </div>
        </div>
      )}

      {/* Modal 4: Course Completion Celebration */}
      {celebrationOpen && (
        <div
          className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center p-4"
          data-testid="course-celebration-modal"
        >
          <div className="w-full max-w-md rounded-xl border border-border bg-bg-primary shadow-xl p-6 text-center space-y-4">
            <div className="w-12 h-12 rounded-full bg-emerald-500/15 text-emerald-500 flex items-center justify-center mx-auto">
              <Award className="w-6 h-6" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-text-primary">
                Hoàn thành khóa học!
              </h2>
              <p className="text-sm text-text-secondary mt-1">
                Bạn đã hoàn tất toàn bộ bài giảng trong &ldquo;{course?.title || "khóa học"}&rdquo;.
              </p>
            </div>
            <div className="flex flex-col gap-2 pt-2">
              {!isStandalone && (
                <>
                  {(course?.quizCount ?? 0) === 0 ? (
                    <button
                      type="button"
                      disabled
                      title="Khóa học chưa có bài kiểm tra"
                      className="h-9 px-4 rounded-md border border-border bg-bg-secondary text-text-tertiary text-sm font-medium inline-flex items-center justify-center gap-2 opacity-60 cursor-not-allowed"
                    >
                      <ClipboardCheck className="w-4 h-4 text-text-tertiary" />
                      <span>Chưa có bài kiểm tra</span>
                    </button>
                  ) : (
                    <Link
                      href={`/app/courses/${courseId}/quiz`}
                      className="h-9 px-4 rounded-md bg-accent text-white text-sm font-medium inline-flex items-center justify-center gap-2 hover:bg-accent-hover transition"
                    >
                      <ClipboardCheck className="w-4 h-4" />
                      <span>Làm bài kiểm tra tổng kết</span>
                    </Link>
                  )}
                  <Link
                    href={`/app/review?courseId=${courseId}`}
                    data-testid="celebration-review-flashcards-btn"
                    className="h-9 px-4 rounded-md border border-border bg-bg-secondary text-text-primary text-sm font-medium inline-flex items-center justify-center gap-2 hover:bg-bg-tertiary transition"
                  >
                    <Brain className="w-4 h-4" />
                    <span>Ôn tập thẻ ghi nhớ</span>
                  </Link>
                  <Link
                    href={`/app/courses/${courseId}`}
                    data-testid="celebration-back-dashboard-btn"
                    className="h-9 px-4 rounded-md text-xs font-medium text-text-secondary hover:text-text-primary inline-flex items-center justify-center"
                  >
                    Về trang chi tiết khóa học
                  </Link>
                </>
              )}
              <button
                type="button"
                onClick={() => setCelebrationOpen(false)}
                className="text-xs text-text-tertiary hover:text-text-primary cursor-pointer pt-1"
              >
                Đóng
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
