"use client";

import React, { useEffect, useMemo, useRef, useState } from "react";
import dynamic from "next/dynamic";
import type { PDFDocumentProxy, RenderTask } from "pdfjs-dist";
import {
  Lightbulb,
  Code2,
  HelpCircle,
  Link2,
  AlertTriangle,
  Zap,
  Sparkles,
  BookOpen,
  FileText,
  ChevronsRight,
  ChevronsLeft,
  ChevronRight,
  ChevronLeft,
  Maximize2,
  Columns2,
  ArrowUpRight,
  PenTool,
  Download,
  Target,
  Pin,
  Trash2,
  X,
  CheckCircle2,
  Trophy,
  Flame,
  ListTree,
  Clock,
  Type,
  Eye,
  Minus,
  Plus,
  Layers
} from "lucide-react";
import {
  SEMANTIC_HIGHLIGHT_KEYS,
  useWorkspaceStore,
  getClientRequestHeaders,
  type SemanticHighlightConfig
} from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";
import { useSynchronizedScroll } from "@/hooks/useSynchronizedScroll";
import { useNotebookAutoSave } from "@/hooks/useNotebookAutoSave";
import { scrollToAndPulseHighlight, waitForElementAndPulse } from "@/lib/animations.ts";
import {
  CourseRoadmapSidebar,
  type RoadmapChapterItem
} from "@/components/reader/CourseRoadmapSidebar";
import {
  LessonMarkdownView,
  type LessonHeadingItem
} from "@/components/reader/LessonMarkdownView";

const LessonPracticeLab = dynamic(
  () => import("@/components/learn/LessonPracticeLab").then((mod) => mod.LessonPracticeLab),
  {
    ssr: false,
    loading: () => (
      <div className="py-6 text-center text-xs text-slate-500">
        Đang tải phòng thực hành bài học...
      </div>
    ),
  }
);

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
function toNullableUuid(val?: string | number | null): string | null {
  if (val == null) return null;
  const trimmed = String(val).trim();
  return UUID_REGEX.test(trimmed) ? trimmed : null;
}


function renderSemanticHighlightIcon(iconName: string, className = "w-3 h-3 inline-block") {
  switch (iconName) {
    case "Lightbulb":
      return <Lightbulb className={className} />;
    case "Code2":
      return <Code2 className={className} />;
    case "HelpCircle":
      return <HelpCircle className={className} />;
    case "Link2":
      return <Link2 className={className} />;
    case "AlertTriangle":
      return <AlertTriangle className={className} />;
    default:
      return <Lightbulb className={className} />;
  }
}

export interface AnnotationItem {
  id: string;
  lessonId?: string | null;
  documentId?: string | null;
  blockId?: string | null;
  pageNumber?: number | null;
  exactQuote: string;
  originalExactQuote: string;
  prefixQuote?: string;
  suffixQuote?: string;
  startOffset: number;
  endOffset: number;
  colorKey: "yellow" | "green" | "blue" | "purple" | "rose";
  semanticLabel?: string;
  noteComment?: string;
  status: "anchored" | "needs_review" | "orphaned" | "archived";
  linkedFlashcardsCount?: number;
  linkedNotesCount?: number;
  createdAt: string;
}

export interface DocumentPageDto {
  id?: string;
  documentId: string;
  pageNumber: number;
  plainText: string;
  hasTextLayer: boolean;
  charCount?: number;
  characterCount?: number;
  wordCount?: number;
}

export interface AstNode {
  type: string;
  attrs?: Record<string, string | number | null | undefined>;
  content?: Array<{ type: string; text?: string }>;
}

export interface LessonDetail {
  id: string;
  title: string;
  slug?: string;
  currentVersion: number;
  plainText?: string;
  contentMarkdown?: string | null;
  videoUrl?: string | null;
  estimatedMinutes?: number;
  sourceFolder?: string | null;
  sourceRelativePath?: string | null;
  courseId?: string | null;
  courseTitle?: string | null;
  chapterTitle?: string | null;
  adjacentPrevLessonId?: string | null;
  adjacentNextLessonId?: string | null;
  linkedQuizId?: string | null;
  linkedFlashcardsCount?: number;
  totalPages?: number;
  fileSizeBytes?: number;
  ocrStatus?: string;
  contentAst?: {
    type: string;
    content: AstNode[];
  };
  annotations: AnnotationItem[];
}

export interface NotebookDetail {
  id: string;
  title: string;
  version: number;
  contentAst: {
    type: string;
    content: AstNode[];
  };
  activeAnnotationIds?: string[];
}

export interface DocumentSummary {
  id: string;
  title: string;
  totalPages: number;
  fileSizeBytes: number;
  ocrStatus: string;
  updatedAt?: string;
}

export interface LessonSummary {
  id: string;
  title: string;
  slug: string;
  currentVersion: number;
  courseId?: string | null;
  chapterId?: string | null;
  sourceDocumentId?: string | null;
  startPage?: number | null;
  endPage?: number | null;
  updatedAt?: string;
}

export interface CourseLessonItemDto {
  id: string;
  title: string;
  slug?: string;
  sortOrder?: number;
  currentVersion?: number;
  estimatedMinutes?: number;
  chapterTitle?: string | null;
  sourceFolder?: string | null;
  sourceRelativePath?: string | null;
  quizId?: string | null;
  courseId?: string | null;
  chapterId?: string | null;
  sourceDocumentId?: string | null;
  startPage?: number | null;
  endPage?: number | null;
}

export interface CourseChapterItemDto {
  id: string;
  title: string;
  sortOrder?: number;
  lessons: CourseLessonItemDto[];
}

export interface CourseSummaryItemDto {
  id?: string;
  courseId?: string;
  title: string;
  description?: string;
  isPublished?: boolean;
  sourceFolder?: string | null;
  category?: string | null;
  sourceDocumentId?: string | null;
  totalPages?: number;
  chapterCount?: number;
  lessonCount?: number;
  flashcardCount?: number;
  quizCount?: number;
  lessonQuizMap?: Record<string, string> | null;
  tagId?: string | null;
  tagName?: string | null;
  quizId?: string | null;
  notebookId?: string | null;
  firstLessonId?: string | null;
  lessonIds?: string[];
  completedLessonIds?: string[];
  lessons?: CourseLessonItemDto[];
  chapters?: CourseChapterItemDto[];
}

export interface CourseProgressDto {
  courseId: string;
  totalLessons: number;
  completedLessons: number;
  percentCompleted: number;
  completedLessonIds: string[];
}

export interface CourseCelebrationModalProps {
  isOpen: boolean;
  onClose: () => void;
  totalLessons: number;
  streakDays?: number;
  onGoToReview: () => void;
  onGoToDashboard: () => void;
}

export function CourseCelebrationModal({
  isOpen,
  onClose,
  totalLessons,
  streakDays = 5,
  onGoToReview,
  onGoToDashboard
}: CourseCelebrationModalProps) {
  if (!isOpen) return null;

  return (
    <div
      data-testid="course-celebration-modal"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-xs p-4 animate-in fade-in duration-200"
    >
      <div className="relative w-full max-w-md rounded-3xl bg-white dark:bg-slate-900 border border-indigo-500/40 p-6 sm:p-8 shadow-2xl text-center space-y-5 animate-in zoom-in-95 duration-200">
        <button
          type="button"
          onClick={onClose}
          className="absolute top-4 right-4 text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 p-1 rounded-full cursor-pointer transition"
          aria-label="Đóng"
        >
          <X className="w-5 h-5" />
        </button>

        {/* Trophy Header */}
        <div className="mx-auto w-16 h-16 rounded-2xl bg-gradient-to-tr from-amber-400 to-amber-500 text-white flex items-center justify-center shadow-lg shadow-amber-500/30">
          <Trophy className="w-8 h-8" />
        </div>

        <div className="space-y-2">
          <span className="px-3 py-1 rounded-full bg-emerald-100 dark:bg-emerald-950/80 text-emerald-700 dark:text-emerald-300 text-xs font-extrabold uppercase tracking-wider">
            Xuất sắc hoàn thành
          </span>
          <h3 className="text-xl sm:text-2xl font-black text-slate-900 dark:text-white">
            Chúc mừng bạn đã hoàn thành 100% khóa học!
          </h3>
          <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-400">
            Bạn đã tiếp thu toàn bộ kiến thức của khóa học. Hãy ôn luyện để củng cố trí nhớ dài hạn cùng FSRS v5.
          </p>
        </div>

        {/* Summary Card */}
        <div className="grid grid-cols-2 gap-3 p-3.5 rounded-2xl bg-slate-50 dark:bg-slate-800/60 border border-slate-200/70 dark:border-slate-800">
          <div className="text-center">
            <div className="text-xl font-black text-indigo-600 dark:text-indigo-400">
              {totalLessons}
            </div>
            <div className="text-[11px] font-semibold text-slate-500 dark:text-slate-400">
              Bài học hoàn thành
            </div>
          </div>
          <div className="text-center">
            <div className="text-xl font-black text-amber-500 flex items-center justify-center gap-1">
              <Flame className="w-5 h-5 fill-amber-500" />
              <span>{streakDays}</span>
            </div>
            <div className="text-[11px] font-semibold text-slate-500 dark:text-slate-400">
              Ngày streak liên tiếp
            </div>
          </div>
        </div>

        {/* CTA Buttons */}
        <div className="flex flex-col sm:flex-row items-center gap-2.5 pt-2">
          <button
            type="button"
            data-testid="celebration-review-flashcards-btn"
            onClick={onGoToReview}
            className="w-full sm:flex-1 py-3 px-4 rounded-xl bg-gradient-to-r from-indigo-600 to-indigo-700 hover:from-indigo-700 hover:to-indigo-800 text-white font-bold text-xs sm:text-sm shadow-md transition cursor-pointer flex items-center justify-center gap-1.5 min-h-[44px]"
          >
            <Zap className="w-4 h-4 text-amber-300" />
            <span>Ôn tập Flashcards</span>
          </button>
          <button
            type="button"
            data-testid="celebration-back-dashboard-btn"
            onClick={onGoToDashboard}
            className="w-full sm:flex-1 py-3 px-4 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-100 dark:hover:bg-slate-700 text-slate-800 dark:text-slate-200 font-bold text-xs sm:text-sm transition cursor-pointer min-h-[44px]"
          >
            Quay về Dashboard
          </button>
        </div>
      </div>
    </div>
  );
}

interface ReaderAndSplitViewsProps {
  mode: "reading" | "split";
  onDataMutated: () => void;
  documents?: DocumentSummary[];
  lessons?: LessonSummary[];
  courses?: CourseSummaryItemDto[];
  quizzes?: Array<{ id: string; title: string }>;
}

function generatePageJumps(totalPages: number, currentPage: number): number[] {
  if (totalPages <= 15) {
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }
  const step = totalPages > 200 ? 50 : totalPages > 100 ? 25 : 10;
  const jumps = new Set<number>([1, totalPages]);
  for (let p = step; p < totalPages; p += step) {
    jumps.add(p);
  }
  jumps.add(currentPage);
  if (currentPage > 1) jumps.add(currentPage - 1);
  if (currentPage < totalPages) jumps.add(currentPage + 1);
  return Array.from(jumps).sort((a, b) => a - b);
}

export function ReaderAndSplitViews({
  mode,
  onDataMutated,
  documents,
  lessons,
  courses: propCourses
}: ReaderAndSplitViewsProps) {
  const {
    activeLessonId,
    activeDocumentId,
    activeDocumentTotalPages,
    sourceType,
    currentDocumentPage,
    setCurrentDocumentPage,
    setActiveMode,
    setActiveLesson,
    setActiveDocument,
    setSelectedTagFilterId,
    setActiveReviewSubTab,
    activeCourseId,
    setActiveCourseId,
    setActiveQuizId,
    sidebarCollapsed,
    setSidebarCollapsed,
    toggleSidebar,
    splitRatios,
    setSplitRatios,
    resetSplitRatios,
    syncScrollEnabled,
    toggleSyncScroll,
    serifFont,
    toggleSerifFont,
    fontSizePx,
    setFontSizePx,
    zenMode,
    toggleZenMode,
    cleanReadingMode,
    toggleCleanReadingMode,
    orphanedDrawerOpen,
    setOrphanedDrawerOpen,
    dismissedOrphanBanner,
    setDismissedOrphanBanner,
    notebookDraftTextBySource,
    setNotebookDraftText,
    pdfViewMode,
    setPdfViewMode
  } = useWorkspaceStore();

  const effectivePdfViewMode = pdfViewMode || "dual";

  const [fetchedCourses, setFetchedCourses] = useState<CourseSummaryItemDto[]>([]);
  const [isEditingLessonTitle, setIsEditingLessonTitle] = useState(false);
  const [editedLessonTitle, setEditedLessonTitle] = useState("");
  const [localLessonTitles, setLocalLessonTitles] = useState<Record<string, string>>({});

  useEffect(() => {
    const hasMatchInProps =
      propCourses &&
      propCourses.length > 0 &&
      ((!activeCourseId && !activeLessonId && !activeDocumentId) ||
        propCourses.some((c) => {
          const cid = c.courseId || c.id;
          if (sourceType === "document" && activeDocumentId && c.sourceDocumentId === activeDocumentId) {
            return true;
          }
          if (
            sourceType === "lesson" &&
            activeLessonId &&
            (c.chapters?.some((ch) => ch.lessons?.some((l) => l.id === activeLessonId)) ||
              c.lessons?.some((l) => l.id === activeLessonId) ||
              c.lessonIds?.includes(activeLessonId))
          ) {
            return true;
          }
          if (activeCourseId && cid === activeCourseId) {
            return true;
          }
          return false;
        }));

    if (hasMatchInProps) {
      return;
    }

    let isMounted = true;
    fetch("/api/v1/courses", { headers: getClientRequestHeaders() })
      .then((r) => (r.ok ? r.json() : []))
      .then((list) => {
        if (isMounted && Array.isArray(list)) setFetchedCourses(list);
      })
      .catch(() => {});
    return () => {
      isMounted = false;
    };
  }, [activeDocumentId, activeLessonId, activeCourseId, sourceType, propCourses]);

  const effectiveCourses = useMemo(() => {
    const map = new Map<string, CourseSummaryItemDto>();
    for (const c of propCourses || []) {
      const id = c.courseId || c.id;
      if (id) map.set(id, c);
    }
    for (const c of fetchedCourses || []) {
      const id = c.courseId || c.id;
      if (id && !map.has(id)) map.set(id, c);
    }
    return Array.from(map.values());
  }, [propCourses, fetchedCourses]);

  const activeCourse =
    effectiveCourses.find((c) => {
      const cid = c.courseId || c.id;
      if (sourceType === "document" && activeDocumentId && c.sourceDocumentId === activeDocumentId) {
        return true;
      }
      if (
        sourceType === "lesson" &&
        activeLessonId &&
        (c.chapters?.some((ch) => ch.lessons?.some((l) => l.id === activeLessonId)) ||
          c.lessons?.some((l) => l.id === activeLessonId) ||
          c.lessonIds?.includes(activeLessonId))
      ) {
        return true;
      }
      if (activeCourseId && cid === activeCourseId) {
        return true;
      }
      return false;
    }) ?? null;

  const activeCourseLessons: CourseLessonItemDto[] = activeCourse
    ? activeCourse.chapters && activeCourse.chapters.length > 0
      ? activeCourse.chapters.flatMap((ch) => ch.lessons || [])
      : activeCourse.lessons || []
    : [];

  const activeChapter =
    activeCourse?.chapters?.find((ch) => ch.lessons?.some((l) => l.id === activeLessonId)) ??
    null;

  const roadmapChapters: RoadmapChapterItem[] = useMemo(() => {
    if (!activeCourse) return [];
    if (activeCourse.chapters && activeCourse.chapters.length > 0) {
      return activeCourse.chapters.map((ch) => ({
        id: ch.id,
        title: ch.title,
        sortOrder: ch.sortOrder,
        lessons: (ch.lessons || []).map((l) => ({
          id: l.id,
          title: localLessonTitles[l.id] || l.title,
          slug: l.slug,
          sortOrder: l.sortOrder,
          estimatedMinutes: l.estimatedMinutes,
          startPage: l.startPage,
          endPage: l.endPage,
          quizId: l.quizId
        }))
      }));
    }
    if (activeCourse.lessons && activeCourse.lessons.length > 0) {
      return [
        {
          id: `course-${activeCourse.courseId || activeCourse.id || "default"}-lessons`,
          title: activeCourse.title || "Nội dung khóa học",
          lessons: activeCourse.lessons.map((l) => ({
            id: l.id,
            title: localLessonTitles[l.id] || l.title,
            slug: l.slug,
            sortOrder: l.sortOrder,
            estimatedMinutes: l.estimatedMinutes,
            startPage: l.startPage,
            endPage: l.endPage,
            quizId: l.quizId
          }))
        }
      ];
    }
    return [];
  }, [activeCourse, localLessonTitles]);

  const [mobileRoadmapOpen, setMobileRoadmapOpen] = useState(false);
  const [extractedHeadings, setExtractedHeadings] = useState<LessonHeadingItem[]>([]);
  const [referenceModal, setReferenceModal] = useState<{
    title: string;
    relativePath: string;
    sourceFolder: string;
    markdown: string;
  } | null>(null);

  const [courseProgress, setCourseProgress] = useState<CourseProgressDto | null>(null);
  const [isCompletingLesson, setIsCompletingLesson] = useState(false);
  const [celebrationModalOpen, setCelebrationModalOpen] = useState(false);
  const [userStreakDays, setUserStreakDays] = useState<number>(3);

  useEffect(() => {
    try {
      const rawSession = typeof window !== "undefined" ? window.localStorage.getItem("swms_auth_session") : null;
      if (rawSession) {
        const session = JSON.parse(rawSession);
        if (typeof session?.user?.streakDays === "number") {
          setUserStreakDays(session.user.streakDays);
        }
      }
    } catch {}
  }, []);

  const activeCourseCid = activeCourse?.courseId || activeCourse?.id || activeCourseId || null;

  const fetchCourseProgress = async (cid: string) => {
    try {
      const res = await fetch(`/api/v1/courses/${cid}/progress`, {
        headers: getClientRequestHeaders()
      });
      if (res.ok) {
        const data: CourseProgressDto = await res.json();
        setCourseProgress(data);
        return data;
      }
    } catch (err) {
      console.error("Error fetching course progress:", err);
    }
    return null;
  };

  useEffect(() => {
    const isValidGuid = Boolean(activeCourseCid && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(activeCourseCid));
    if (activeCourseCid && isValidGuid) {
      fetchCourseProgress(activeCourseCid);
    } else {
      setCourseProgress(null);
    }
  }, [activeCourseCid, activeLessonId]);

  const completedLessonIds = courseProgress?.completedLessonIds ?? [];
  const totalCourseLessons = activeCourseLessons.length > 0 ? activeCourseLessons.length : (courseProgress?.totalLessons ?? 0);
  const completedCourseLessons = courseProgress?.completedLessons ?? completedLessonIds.length;
  const coursePercentCompleted = courseProgress?.percentCompleted ?? (totalCourseLessons > 0 ? Math.round((completedCourseLessons / totalCourseLessons) * 100) : 0);

  const scrollReaderToTop = () => {
    if (typeof window !== "undefined") {
      window.scrollTo({ top: 0, behavior: "auto" });
      document.documentElement.scrollTop = 0;
      document.body.scrollTop = 0;
      const scrollContainers = document.querySelectorAll<HTMLElement>(
        "[data-swms-scroll-container], main"
      );
      scrollContainers.forEach((node) => {
        if (node.scrollTop > 0) {
          node.scrollTo({ top: 0, behavior: "auto" });
        }
      });
    }
    if (readerScrollRef.current) {
      readerScrollRef.current.scrollTo({ top: 0, behavior: "auto" });
    }
  };

  const handleMarkLessonComplete = async () => {
    if (!activeLessonId || isCompletingLesson) return;
    setIsCompletingLesson(true);

    try {
      // 1. Call POST /api/v1/lessons/{activeLessonId}/complete
      await fetch(`/api/v1/lessons/${activeLessonId}/complete`, {
        method: "POST",
        headers: getClientRequestHeaders({ "Content-Type": "application/json" })
      });

      // 2. Update local progress state
      const currentCompleted = new Set(courseProgress?.completedLessonIds ?? []);
      currentCompleted.add(activeLessonId);
      const updatedIds = Array.from(currentCompleted);
      const totalCount = activeCourseLessons.length > 0 ? activeCourseLessons.length : Math.max(1, updatedIds.length);
      const completedCount = updatedIds.length;
      const newPercent = Math.min(100, Math.round((completedCount / totalCount) * 100));

      const updatedProgress: CourseProgressDto = {
        courseId: activeCourseCid ?? "",
        totalLessons: totalCount,
        completedLessons: completedCount,
        percentCompleted: newPercent,
        completedLessonIds: updatedIds
      };
      setCourseProgress(updatedProgress);
      invalidateDashboardCache();
      if (typeof window !== "undefined") {
        window.dispatchEvent(new Event("swms:data-mutated"));
      }
      onDataMutated();

      // 3. Find next lesson in the chapter or next chapter
      const currentIdx = activeCourseLessons.findIndex((l) => l.id === activeLessonId);
      const hasNext = currentIdx >= 0 && currentIdx < activeCourseLessons.length - 1;

      // 4. Trigger celebration modal if all completed or reached 100%
      if (newPercent === 100 || (!hasNext && completedCount >= totalCount)) {
        setCelebrationModalOpen(true);
      }

      // Navigate to next lesson
      let navigatedToNext = false;
      if (hasNext) {
        const nextLes = activeCourseLessons[currentIdx + 1];
        setActiveLesson(nextLes.id);
        navigatedToNext = true;
      } else if (lesson?.adjacentNextLessonId) {
        setActiveLesson(lesson.adjacentNextLessonId);
        navigatedToNext = true;
      } else if (activeCourseLessons.length === 0 && effectiveLessons.length > 1) {
        const effIdx = effectiveLessons.findIndex((l) => l.id === activeLessonId);
        if (effIdx >= 0 && effIdx < effectiveLessons.length - 1) {
          setActiveLesson(effectiveLessons[effIdx + 1].id);
          navigatedToNext = true;
        }
      }

      if (navigatedToNext) {
        requestAnimationFrame(() => scrollReaderToTop());
      }

      // Re-fetch in background
      if (activeCourseCid) {
        fetchCourseProgress(activeCourseCid);
      }
    } catch (err) {
      console.error("Error marking lesson complete:", err);
    } finally {
      setIsCompletingLesson(false);
    }
  };

  const effectiveLessons = (lessons && lessons.length > 0)
    ? lessons
    : [
        { id: "44444444-4444-4444-8444-444444444444", title: "3.2 Bộ nhớ ảo & TLB", slug: "3-2-bo-nho-ao-tlb", currentVersion: 1 },
        { id: "55555555-5555-5555-8555-555555555555", title: "3.3 Thay thế trang LRU", slug: "3-3-thay-the-trang-lru", currentVersion: 1 }
      ];

  const docList = [...(documents || [])];

  const activeDoc = docList.find((d) => d.id === activeDocumentId);

  const [lesson, setLesson] = useState<LessonDetail | null>(null);
  const [notebook, setNotebook] = useState<NotebookDetail | null>(null);
  const [selectedText, setSelectedText] = useState<string>("");
  const [selectedBlockId, setSelectedBlockId] = useState<string>("");
  const [selectedColorKey, setSelectedColorKey] = useState<SemanticHighlightConfig["key"]>("yellow");

  // PDF Viewer & Dual-Layer Reader states
  const [currentPageData, setCurrentPageData] = useState<DocumentPageDto | null>(null);
  const [pageLoading, setPageLoading] = useState(false);
  const [pdfDoc, setPdfDoc] = useState<PDFDocumentProxy | null>(null);
  const [pdfTotalPages, setPdfTotalPages] = useState<number>(0);
  const [jumpPageInput, setJumpPageInput] = useState<string>("1");
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const currentRenderTaskRef = useRef<RenderTask | null>(null);
  const pageCacheRef = useRef<Map<number, DocumentPageDto>>(new Map());

  const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
  const effectiveTotalPages = Math.max(
    1,
    pdfDoc?.numPages ??
    (pdfTotalPages > 0 ? pdfTotalPages : undefined) ??
    activeDoc?.totalPages ??
    lesson?.totalPages ??
    (activeDocumentTotalPages && activeDocumentTotalPages > 0
      ? activeDocumentTotalPages
      : undefined) ??
    1
  );
  const safeCurrentPage = Math.max(1, Math.min(effectiveTotalPages, Number(currentDocumentPage) || 1));
  const readingProgressPercent = isDocMode
    ? Math.min(100, Math.max(1, Math.round((safeCurrentPage / effectiveTotalPages) * 100)))
    : 100;

  // Sync jumpPageInput when safeCurrentPage changes externally (next/prev, jump to source, sidebar)
  useEffect(() => {
    setJumpPageInput(String(safeCurrentPage));
  }, [safeCurrentPage]);

  // Synchronize selection changes (both selection and deselection)
  useEffect(() => {
    const onSelectionChange = () => {
      const sel = window.getSelection();
      if (!sel || sel.rangeCount === 0) {
        setSelectedText("");
      } else {
        const text = sel.toString().trim();
        if (text.length >= 3) {
          setSelectedText(text);
        }
      }
    };
    document.addEventListener("selectionchange", onSelectionChange);
    return () => document.removeEventListener("selectionchange", onSelectionChange);
  }, []);

  // Draft Flashcard Confirmation Popover state (Section 5.9 & Decision 5)
  const [draftPopoverOpen, setDraftPopoverOpen] = useState(false);
  const [draftMode, setDraftMode] = useState<"cloze" | "ai">("cloze");
  const [draftFront, setDraftFront] = useState("");
  const [draftBack, setDraftBack] = useState("");
  const [draftTags, setDraftTags] = useState<string[]>([]);
  const [aiDraftLoading, setAiDraftLoading] = useState(false);

  // In-situ AI Ghostreader Explanation state
  const [aiExplainOpen, setAiExplainOpen] = useState(false);
  const [aiExplanation, setAiExplanation] = useState("");
  const [aiExplainLoading, setAiExplainLoading] = useState(false);

  // Toast / Status Banner
  const [toastBanner, setToastBanner] = useState<string | null>(null);
  const [exportedMarkdown, setExportedMarkdown] = useState<string | null>(null);

  const splitContainerRef = useRef<HTMLDivElement | null>(null);
  const readerScrollRef = useRef<HTMLDivElement | null>(null);
  const notebookScrollRef = useRef<HTMLDivElement | null>(null);
  const activeResizeCleanupRef = useRef<(() => void) | null>(null);

  useEffect(() => {
    return () => {
      activeResizeCleanupRef.current?.();
    };
  }, []);

  // Interactive Drag Resizing for Split Workspace Handles (MASTER_PROMPT.md §5.2: Pane 1: 10%–25%, Pane 2: 30%–60%, Pane 3: 25%–55%)
  const computeNextSplitRatios = (
    handleIndex: 1 | 2,
    startRatios: [number, number, number],
    deltaPct: number
  ): [number, number, number] => {
    const [startR1, startR2, startR3] = startRatios;

    if (handleIndex === 1) {
      const nextR1 = Math.round(Math.min(25, Math.max(10, startR1 + deltaPct)) * 10) / 10;
      const remaining1 = Number((100 - nextR1).toFixed(1));
      const minR2 = Math.max(30, Number((remaining1 - 55).toFixed(1)));
      const maxR2 = Math.min(60, Number((remaining1 - 25).toFixed(1)));
      const desiredR2 = Number((remaining1 - startR3).toFixed(1));
      const nextR2 = Math.round(Math.min(maxR2, Math.max(minR2, desiredR2)) * 10) / 10;
      const nextR3 = Number((100 - nextR1 - nextR2).toFixed(1));
      return [nextR1, nextR2, nextR3];
    }

    const nextR2 = Math.round(Math.min(60, Math.max(30, startR2 + deltaPct)) * 10) / 10;
    const remaining2 = Number((100 - nextR2).toFixed(1));
    const minR3 = Math.max(25, Number((remaining2 - 25).toFixed(1)));
    const maxR3 = Math.min(55, Number((remaining2 - 10).toFixed(1)));
    const desiredR3 = Number((remaining2 - startR1).toFixed(1));
    const nextR3 = Math.round(Math.min(maxR3, Math.max(minR3, desiredR3)) * 10) / 10;
    const nextR1 = Number((100 - nextR2 - nextR3).toFixed(1));
    return [nextR1, nextR2, nextR3];
  };

  const handleStartResize = (handleIndex: 1 | 2, e: React.MouseEvent<HTMLDivElement>) => {
    e.preventDefault();
    activeResizeCleanupRef.current?.();
    if (handleIndex === 1 && sidebarCollapsed) {
      setSidebarCollapsed(false);
    }

    const startX = e.clientX;
    const startRatios: [number, number, number] = [...splitRatios];
    const containerWidth =
      splitContainerRef.current?.getBoundingClientRect().width || window.innerWidth || 1920;

    const onMouseMove = (moveEvent: MouseEvent) => {
      const deltaPct = ((moveEvent.clientX - startX) / containerWidth) * 100;
      setSplitRatios(computeNextSplitRatios(handleIndex, startRatios, deltaPct));
    };

    const cleanup = () => {
      window.removeEventListener("mousemove", onMouseMove);
      window.removeEventListener("mouseup", cleanup);
      document.body.style.cursor = "";
      document.body.style.userSelect = "";
      activeResizeCleanupRef.current = null;
    };

    activeResizeCleanupRef.current = cleanup;
    document.body.style.cursor = "col-resize";
    document.body.style.userSelect = "none";
    window.addEventListener("mousemove", onMouseMove);
    window.addEventListener("mouseup", cleanup);
  };

  const handleStartTouchResize = (handleIndex: 1 | 2, e: React.TouchEvent<HTMLDivElement>) => {
    if (e.touches.length === 0) return;
    activeResizeCleanupRef.current?.();
    if (handleIndex === 1 && sidebarCollapsed) {
      setSidebarCollapsed(false);
    }

    const startX = e.touches[0].clientX;
    const startRatios: [number, number, number] = [...splitRatios];
    const containerWidth =
      splitContainerRef.current?.getBoundingClientRect().width || window.innerWidth || 1920;

    const onTouchMove = (moveEvent: TouchEvent) => {
      if (moveEvent.touches.length === 0) return;
      const deltaPct = ((moveEvent.touches[0].clientX - startX) / containerWidth) * 100;
      setSplitRatios(computeNextSplitRatios(handleIndex, startRatios, deltaPct));
    };

    const cleanup = () => {
      window.removeEventListener("touchmove", onTouchMove);
      window.removeEventListener("touchend", cleanup);
      window.removeEventListener("touchcancel", cleanup);
      activeResizeCleanupRef.current = null;
    };

    activeResizeCleanupRef.current = cleanup;
    window.addEventListener("touchmove", onTouchMove, { passive: true });
    window.addEventListener("touchend", cleanup);
    window.addEventListener("touchcancel", cleanup);
  };

  const { syncScrollContainerProportional } = useSynchronizedScroll(syncScrollEnabled);

  const { saveNotebookWithSingleWriterLock } = useNotebookAutoSave(
    notebook?.id ?? null,
    (newVer) => {
      setNotebook((prev) => (prev ? { ...prev, version: newVer } : prev));
    },
    (conflict) => {
      setToastBanner(conflict.message);
      if (notebook) {
        setNotebook({
          ...notebook,
          version: conflict.serverVersion
        });
      }
    }
  );

  const showToast = (msg: string) => {
    setToastBanner(msg);
    setTimeout(() => setToastBanner(null), 5000);
  };

  const loadLessonAndNotebook = async () => {
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    const endpoint = isDocMode
      ? `/api/v1/documents/${activeDocumentId}`
      : `/api/v1/lessons/${activeLessonId}`;

    const res = await fetch(endpoint, { headers: getClientRequestHeaders() });
    if (res.ok) {
      const rawData = await res.json();
      const data: LessonDetail = {
        ...rawData,
        currentVersion: rawData.currentVersion ?? 1,
        totalPages: rawData.totalPages,
        annotations: rawData.annotations ?? []
      };
      setLesson(data);
      if (isDocMode && data.totalPages) {
        setPdfTotalPages(data.totalPages);
      }
      if (isDocMode && rawData.initialPage) {
        pageCacheRef.current.set(1, rawData.initialPage);
        if (currentDocumentPage === 1) {
          setCurrentPageData(rawData.initialPage);
        }
      }

      // Auto-provision or fetch linked Freeform Notebook (Decision 3)
      const nbRes = await fetch("/api/v1/notebooks/by-source", {
        method: "POST",
        headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify(
          isDocMode
            ? { documentId: data.id, sourceTitle: data.title }
            : { lessonId: data.id, sourceTitle: data.title }
        )
      });
      if (nbRes.ok) {
        const nbData: NotebookDetail = await nbRes.json();
        setNotebook(nbData);
      }
    }
  };

  useEffect(() => {
    setExtractedHeadings([]);
    scrollReaderToTop();
    const rafId = requestAnimationFrame(() => scrollReaderToTop());
    loadLessonAndNotebook();
    return () => cancelAnimationFrame(rafId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeLessonId, activeDocumentId, sourceType]);

  const handleOpenRelativeMarkdown = async (resolvedRelativePath: string, label: string) => {
    const folder = lesson?.sourceFolder || activeCourse?.sourceFolder;
    const cid = activeCourseCid || lesson?.courseId || "";
    if (!resolvedRelativePath) {
      showToast(`Không tìm thấy đường dẫn tài liệu: ${label}`);
      return;
    }

    try {
      const qs = new URLSearchParams({
        relativePath: resolvedRelativePath,
        ...(cid ? { courseId: cid } : {}),
        ...(lesson?.id ? { fromLessonId: lesson.id } : {}),
        ...(folder ? { sourceFolder: folder } : {})
      });
      const res = await fetch(`/api/v1/lessons/by-path?${qs.toString()}`, {
        headers: getClientRequestHeaders()
      });
      if (res.ok) {
        const matched = await res.json();
        const isMatched = matched?.found !== false;

        if (isMatched && matched?.isAssetMarkdown && matched?.contentMarkdown) {
          setReferenceModal({
            title: matched?.title || label || resolvedRelativePath.split("/").pop() || "Tài liệu tham khảo",
            relativePath: matched?.sourceRelativePath || resolvedRelativePath,
            sourceFolder: matched?.sourceFolder || folder || "",
            markdown: matched.contentMarkdown
          });
          return;
        }

        const targetId = isMatched ? (matched?.lessonId || matched?.id) : null;
        if (targetId) {
          if (matched?.courseId) setActiveCourseId?.(matched.courseId);
          setActiveLesson(targetId);
          showToast(`Đã mở bài học: ${matched?.title || label}`);
          if (matched?.anchor) {
            setTimeout(() => {
              const targetAnchor = String(matched.anchor).replace(/^#/, "");
              const el = document.getElementById(targetAnchor);
              el?.scrollIntoView({ behavior: "smooth", block: "start" });
            }, 250);
          }
          return;
        }
      }

      if (folder) {
        const encodedPath = resolvedRelativePath
          .split("/")
          .filter(Boolean)
          .map((seg) => encodeURIComponent(seg))
          .join("/");
        const assetResp = await fetch(
          `/api/v1/document-assets/${encodeURIComponent(folder)}/${encodedPath}`,
          { headers: getClientRequestHeaders() }
        );
        if (assetResp.ok) {
          const text = await assetResp.text();
          setReferenceModal({
            title: label || resolvedRelativePath.split("/").pop() || "Tài liệu tham khảo",
            relativePath: resolvedRelativePath,
            sourceFolder: folder,
            markdown: text
          });
          return;
        }
      }

      showToast(`Không tìm thấy tài liệu liên kết (${resolvedRelativePath}).`);
    } catch {
      showToast(`Không thể mở liên kết tài liệu: ${label}`);
    }
  };

  // Load PDF Document Proxy when activeDocumentId changes in document mode
  useEffect(() => {
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    setPdfTotalPages(0);
    if (!isDocMode || !activeDocumentId) {
      setPdfDoc(null);
      return;
    }

    let isMounted = true;
    pageCacheRef.current.clear();

    const loadPdf = async () => {
      try {
        const pdfjs = await import("pdfjs-dist");
        if (typeof window !== "undefined") {
          pdfjs.GlobalWorkerOptions.workerSrc = "/pdf.worker.min.mjs";
        }
        const loadingTask = pdfjs.getDocument({
          url: `/api/v1/documents/${activeDocumentId}/file`,
          cMapPacked: true,
          httpHeaders: getClientRequestHeaders(),
          withCredentials: true
        });
        const doc = await loadingTask.promise;
        if (isMounted) {
          setPdfDoc(doc);
          setPdfTotalPages(doc.numPages);
        }
      } catch (err) {
        console.error("Failed to load PDF binary via pdfjs:", err);
      }
    };

    loadPdf();

    return () => {
      isMounted = false;
      if (currentRenderTaskRef.current) {
        try {
          currentRenderTaskRef.current.cancel();
        } catch {}
        currentRenderTaskRef.current = null;
      }
    };
  }, [activeDocumentId, sourceType]);

  // Render active PDF page on visual canvas
  useEffect(() => {
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    if (!isDocMode || !pdfDoc || !canvasRef.current) return;

    let isCancelled = false;

    const renderPage = async () => {
      if (currentRenderTaskRef.current) {
        try {
          currentRenderTaskRef.current.cancel();
        } catch {}
        currentRenderTaskRef.current = null;
      }

      try {
        const pageNum = Math.max(1, Math.min(pdfDoc.numPages, currentDocumentPage));
        const page = await pdfDoc.getPage(pageNum);
        if (isCancelled || !canvasRef.current) return;

        const viewport = page.getViewport({ scale: 1.5 });
        const canvas = canvasRef.current;
        const context = canvas.getContext("2d", { willReadFrequently: true });
        if (!context) return;

        canvas.width = viewport.width;
        canvas.height = viewport.height;

        const renderContext = {
          canvasContext: context,
          viewport: viewport
        };

        const renderTask = page.render(renderContext);
        currentRenderTaskRef.current = renderTask;

        await renderTask.promise;
        currentRenderTaskRef.current = null;
      } catch (err: any) {
        if (err?.name !== "RenderingCancelledException") {
          console.error("PDF page render error:", err);
        }
      }
    };

    renderPage();

    return () => {
      isCancelled = true;
      if (currentRenderTaskRef.current) {
        try {
          currentRenderTaskRef.current.cancel();
        } catch {}
        currentRenderTaskRef.current = null;
      }
    };
  }, [pdfDoc, currentDocumentPage, activeDocumentId, sourceType, mode, effectivePdfViewMode]);

  // Fetch page text data for currentDocumentPage
  useEffect(() => {
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    if (!isDocMode || !activeDocumentId) {
      setCurrentPageData(null);
      return;
    }

    const pageNum = currentDocumentPage;
    setJumpPageInput(String(pageNum));

    const cached = pageCacheRef.current.get(pageNum);
    if (cached) {
      setCurrentPageData(cached);
      return;
    }

    let isMounted = true;
    setPageLoading(true);

    const fetchPage = async () => {
      try {
        const res = await fetch(
          `/api/v1/documents/${activeDocumentId}/pages/${pageNum}`,
          {
            credentials: "include",
            headers: getClientRequestHeaders()
          }
        );
        if (res.ok) {
          const data: DocumentPageDto = await res.json();
          pageCacheRef.current.set(pageNum, data);
          if (isMounted) {
            setCurrentPageData(data);
          }
        } else if (res.status === 404) {
          const emptyPage: DocumentPageDto = {
            documentId: activeDocumentId,
            pageNumber: pageNum,
            plainText: "",
            hasTextLayer: false
          };
          pageCacheRef.current.set(pageNum, emptyPage);
          if (isMounted) {
            setCurrentPageData(emptyPage);
          }
        }
      } catch (err) {
        console.error("Error fetching page text:", err);
      } finally {
        if (isMounted) {
          setPageLoading(false);
        }
      }
    };

    fetchPage();

    return () => {
      isMounted = false;
    };
  }, [activeDocumentId, currentDocumentPage, sourceType]);

  // Keyboard navigation for PDF pages (ArrowLeft/ArrowRight, PageUp/PageDown)
  useEffect(() => {
    if (!isDocMode) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement | null;
      if (
        target &&
        (target.tagName === "INPUT" ||
          target.tagName === "TEXTAREA" ||
          target.isContentEditable)
      ) {
        return;
      }

      if (e.key === "ArrowLeft" || e.key === "PageUp") {
        e.preventDefault();
        setCurrentDocumentPage(Math.max(1, safeCurrentPage - 1));
      } else if (e.key === "ArrowRight" || e.key === "PageDown") {
        e.preventDefault();
        setCurrentDocumentPage(Math.min(effectiveTotalPages, safeCurrentPage + 1));
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [effectiveTotalPages, isDocMode, safeCurrentPage, setCurrentDocumentPage]);

  const handleJumpPage = (e?: React.FormEvent | React.KeyboardEvent) => {
    if (e && "key" in e && e.key !== "Enter") return;
    if (e) e.preventDefault();

    const trimmed = jumpPageInput.trim();
    const parsed = parseInt(trimmed, 10);

    if (isNaN(parsed)) {
      setJumpPageInput(String(safeCurrentPage));
      return;
    }

    const clamped = Math.max(1, Math.min(effectiveTotalPages, parsed));
    setCurrentDocumentPage(clamped);
    setJumpPageInput(String(clamped));
  };

  // Keyboard shortcut Ctrl+\ to toggle sidebar
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === "\\") {
        e.preventDefault();
        toggleSidebar();
      }
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [toggleSidebar]);

  const orphanedAnnotations = (lesson?.annotations || []).filter(
    (a) =>
      (a.status === "orphaned" || a.status === "needs_review") &&
      !a.noteComment?.includes("Giữ trích dẫn tự do") &&
      !a.noteComment?.includes("Đã xác nhận giữ dạng trích dẫn tự do")
  );
  const anchoredAnnotations = (lesson?.annotations || []).filter((a) => a.status === "anchored");

  const handleTextSelectionInBlock = (blockId: string, fullBlockText: string) => {
    if (typeof window === "undefined") return;
    const sel = window.getSelection()?.toString().trim() ?? "";
    if (sel.length >= 3) {
      setSelectedText(sel);
      setSelectedBlockId(blockId);
    } else {
      // Convenience fallback: select first key sentence if clicked
      setSelectedBlockId(blockId);
      setSelectedText(fullBlockText.slice(0, Math.min(92, fullBlockText.length)));
    }
  };

  const handleCreateHighlight = async (colorKey: SemanticHighlightConfig["key"]) => {
    if (!lesson) return;
    let quoteToSave = selectedText.trim();
    if (!quoteToSave && typeof window !== "undefined") {
      const liveSel = window.getSelection()?.toString().trim();
      if (liveSel && liveSel.length >= 3) {
        quoteToSave = liveSel;
        setSelectedText(liveSel);
      }
    }
    if (!quoteToSave) {
      showToast("Vui lòng bôi đen một đoạn văn bản trong bài đọc trước khi đánh dấu hoặc tạo thẻ!");
      return;
    }

    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    const fullText = isDocMode
      ? (currentPageData?.plainText || "")
      : (lesson.plainText || lesson.contentMarkdown || "");
    const startIdx = fullText.indexOf(quoteToSave);
    const startOffset = startIdx >= 0 ? startIdx : 0;
    const endOffset = startOffset + quoteToSave.length;
    const prefixQuote = fullText.slice(Math.max(0, startOffset - 32), startOffset);
    const suffixQuote = fullText.slice(endOffset, Math.min(fullText.length, endOffset + 32));

    const firstParagraphBlock = lesson.contentAst?.content?.find((b) => b.type === "paragraph");
    const defaultBlockId =
      firstParagraphBlock?.attrs?.block_id || "aaaaaaa4-4444-4444-8444-444444444444";
    const blockIdToUse = isDocMode ? null : toNullableUuid(selectedBlockId || defaultBlockId);

    setSelectedColorKey(colorKey);
    const cfg = SEMANTIC_HIGHLIGHT_KEYS.find((c) => c.key === colorKey)!;

    const resp = await fetch("/api/v1/annotations", {
      method: "POST",
      headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
      body: JSON.stringify({
        lessonId: isDocMode ? null : lesson.id,
        documentId: isDocMode ? (activeDocumentId || lesson.id) : null,
        blockId: isDocMode ? null : blockIdToUse,
        pageNumber: isDocMode ? currentDocumentPage : null,
        exactQuote: quoteToSave,
        originalExactQuote: quoteToSave,
        prefixQuote,
        suffixQuote,
        startOffset,
        endOffset,
        colorKey,
        semanticLabel: cfg.label,
        noteComment: `Đánh dấu [${cfg.icon} ${cfg.label}]`
      })
    });

    if (resp.ok) {
      showToast(`Đã lưu Highlight [${cfg.icon} ${cfg.label}]!`);
      await loadLessonAndNotebook();
      onDataMutated();
    }
  };

  const handleOpenDraftFlashcardPopover = (excerptOverride?: string) => {
    let excerpt = (excerptOverride ?? selectedText).trim();
    if (!excerpt && typeof window !== "undefined") {
      const liveSel = window.getSelection()?.toString().trim();
      if (liveSel && liveSel.length >= 3) {
        excerpt = liveSel;
      }
    }
    if (!excerpt) {
      showToast("Vui lòng bôi đen một đoạn văn bản trong bài đọc trước khi đánh dấu hoặc tạo thẻ!");
      return;
    }
    setSelectedText(excerpt);
    setDraftMode("cloze");
    // Instant Cloze generation in 0ms (0 AI credits)
    const words = excerpt.split(/\s+/);
    if (words.length > 3) {
      const targetPhrase = words.slice(0, Math.min(4, words.length)).join(" ");
      const rest = words.slice(Math.min(4, words.length)).join(" ");
      setDraftFront(`{{c1::${targetPhrase}}} ${rest}`.trim());
    } else {
      setDraftFront(`{{c1::${excerpt}}}`);
    }
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    const sourceTitle = isDocMode
      ? (activeDoc?.title || "Tài liệu PDF")
      : (lesson?.title || "Bài học");
    setDraftBack(
      isDocMode
        ? `Trích dẫn từ ${sourceTitle} (Trang ${currentDocumentPage}): "${excerpt}"`
        : `Trích dẫn từ ${sourceTitle}: "${excerpt}"`
    );
    if (isDocMode) {
      const docCleanTag = (activeDoc?.title || "PDF")
        .replace(/[^a-zA-Z0-9\u00C0-\u024F\u1EA0-\u1EF9]/g, "-")
        .slice(0, 24);
      setDraftTags([docCleanTag, `Trang-${currentDocumentPage}`]);
    } else {
      setDraftTags(["Bai-Hoc", "Ly-Thuyet"]);
    }
    setDraftPopoverOpen(true);
  };

  const handleSwitchDraftToAiMode = async () => {
    setDraftMode("ai");
    setAiDraftLoading(true);
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    const excerpt =
      selectedText.trim() ||
      (isDocMode
        ? (currentPageData?.plainText?.slice(0, 150) || "")
        : (lesson?.plainText?.slice(0, 150) || ""));
    const contextTitle = isDocMode
      ? (activeDoc?.title || "Tài liệu PDF")
      : (lesson?.title || "Bài học");
    try {
      const resp = await fetch("/api/v1/ai/generate-flashcard", {
        method: "POST",
        headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          excerpt,
          contextTitle,
          actionType: "generate_qa"
        })
      });
      if (resp.ok) {
        const data = await resp.json();
        setDraftFront(data.front);
        setDraftBack(data.back);
        onDataMutated();
      }
    } finally {
      setAiDraftLoading(false);
    }
  };

  const handleCommitDraftFlashcard = async () => {
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    if (!lesson && !activeDocumentId) return;

    let targetAnn =
      anchoredAnnotations.find(
        (a) =>
          a.exactQuote === (selectedText || draftFront) &&
          (isDocMode ? a.documentId === activeDocumentId && (a.pageNumber ?? 1) === currentDocumentPage : true)
      ) ?? null;

    if (!targetAnn && (selectedText || draftFront)) {
      const quoteToAnchor = (selectedText || draftFront).trim();
      const fullText = isDocMode
        ? (currentPageData?.plainText || "")
        : (lesson?.plainText || lesson?.contentMarkdown || "");
      const startIdx = fullText.indexOf(quoteToAnchor);
      const startOffset = startIdx >= 0 ? startIdx : 0;
      const endOffset = startOffset + quoteToAnchor.length;
      const prefixQuote = fullText.slice(Math.max(0, startOffset - 32), startOffset);
      const suffixQuote = fullText.slice(endOffset, Math.min(fullText.length, endOffset + 32));

      const annResp = await fetch("/api/v1/annotations", {
        method: "POST",
        headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          lessonId: isDocMode ? null : lesson?.id,
          documentId: isDocMode ? (activeDocumentId || lesson?.id) : null,
          blockId: isDocMode ? null : toNullableUuid(selectedBlockId),
          pageNumber: isDocMode ? currentDocumentPage : null,
          exactQuote: quoteToAnchor,
          originalExactQuote: quoteToAnchor,
          prefixQuote,
          suffixQuote,
          startOffset,
          endOffset,
          colorKey: selectedColorKey || "yellow",
          semanticLabel: "Trích dẫn thẻ ghi nhớ",
          noteComment: "Tự động neo cho Thẻ ghi nhớ"
        })
      });

      if (annResp.ok) {
        targetAnn = await annResp.json();
        await loadLessonAndNotebook();
      }
    }

    const resp = await fetch("/api/v1/flashcards", {
      method: "POST",
      headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
      body: JSON.stringify({
        lessonId: isDocMode ? null : lesson?.id,
        documentId: isDocMode ? (activeDocumentId || lesson?.id) : null,
        annotationId: targetAnn?.id ?? null,
        cardType: draftMode === "cloze" ? "cloze" : "ai_generated",
        sourceTitleSnapshot: isDocMode
          ? (activeDoc?.title || "Tài liệu PDF")
          : (lesson?.title || "Bài học"),
        sourceQuoteSnapshot: selectedText || draftFront,
        frontText: draftFront,
        backText: draftBack,
        tagNames: draftTags
      })
    });
    if (resp.ok) {
      setDraftPopoverOpen(false);
      showToast("Đã lưu thẻ vào Hàng đợi Ôn tập Hợp nhất (FSRS v5)!");
      onDataMutated();
    }
  };

  const handleExplainWithAi = async () => {
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    const excerpt =
      selectedText.trim() ||
      (isDocMode
        ? (currentPageData?.plainText?.slice(0, 150) || "")
        : (lesson?.plainText?.slice(0, 150) || ""));
    const contextTitle = isDocMode
      ? (activeDoc?.title || "Tài liệu PDF")
      : (lesson?.title || "Bài học");
    setAiExplainOpen(true);
    setAiExplainLoading(true);
    try {
      const resp = await fetch("/api/v1/ai/explain", {
        method: "POST",
        headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          excerpt,
          contextTitle,
          actionType: "explain"
        })
      });
      if (resp.ok) {
        const data = await resp.json();
        setAiExplanation(data.explanation);
        onDataMutated();
      } else {
        setAiExplanation("Không thể kết nối AI hoặc đã vượt hạn mức ngày.");
      }
    } finally {
      setAiExplainLoading(false);
    }
  };

  const handleInsertCitationIntoNotebook = async (ann: AnnotationItem) => {
    if (!notebook || (!lesson && !activeDocumentId)) return;
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    const sourceTitle = isDocMode
      ? (activeDoc?.title || lesson?.title || "Tài liệu PDF")
      : (lesson?.title || "Bài học");
    const updatedContent: AstNode[] = [
      ...(notebook.contentAst?.content || []),
      {
        type: "annotationRef",
        attrs: {
          block_id: crypto.randomUUID(),
          ref_annotation_id: ann.id,
          ref_lesson_id: isDocMode ? null : lesson?.id,
          ref_document_id: isDocMode ? (activeDocumentId || lesson?.id) : null,
          page_number: isDocMode ? (ann.pageNumber ?? currentDocumentPage) : null,
          ref_block_id: ann.blockId,
          color_key: ann.colorKey,
          source_title: sourceTitle,
          source_quote: ann.exactQuote
        }
      }
    ];
    const nextAst = { type: "doc", content: updatedContent };
    const result = await saveNotebookWithSingleWriterLock({
      id: notebook.id,
      baseVersion: notebook.version,
      title: notebook.title,
      contentAst: nextAst
    });
    if (result.status === "saved") {
      await loadLessonAndNotebook();
      if (mode !== "split") setActiveMode("split");
      showToast("Đã chèn Thẻ Trích dẫn (Citation Card) vào Ghi chú!");
    }
  };

  const handleSaveInlinePracticeToNotebook = async (quoteText: string) => {
    const trimmed = quoteText.trim();
    if (!trimmed) return;
    if (activeLessonId) {
      const prevDraft = notebookDraftTextBySource[activeLessonId] || "";
      setNotebookDraftText(activeLessonId, prevDraft ? `${prevDraft}\n\n${trimmed}` : trimmed);
    }
    if (notebook) {
      const updatedContent: AstNode[] = [
        ...(notebook.contentAst?.content || []),
        {
          type: "paragraph",
          content: [{ type: "text", text: `[Thực hành bài học] ${trimmed}` }],
        },
      ];
      try {
        await saveNotebookWithSingleWriterLock({
          id: notebook.id,
          baseVersion: notebook.version,
          title: notebook.title,
          contentAst: { type: "doc", content: updatedContent },
        });
      } catch {
        // Fallback stored in notebookDraftTextBySource
      }
    }
    showToast("Đã lưu kết quả thực hành vào sổ tay.");
  };

  const handleCreateFlashcardQuickFromPractice = async (frontInput?: string, backInput?: string) => {
    const rawFront = (frontInput || "").trim();
    if (!rawFront) return;
    const derivedBack = (backInput || "").trim() || rawFront;
    try {
      const resp = await fetch("/api/v1/flashcards", {
        method: "POST",
        headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          lessonId: activeLessonId || null,
          documentId: isDocMode ? (activeDocumentId || null) : null,
          annotationId: null,
          cardType: "basic",
          sourceTitleSnapshot: lesson?.title || "Bài học",
          sourceQuoteSnapshot: rawFront,
          frontText: rawFront,
          backText: derivedBack,
          tagNames: ["Thuc-Hanh"],
        }),
      });
      if (resp.ok) {
        onDataMutated();
        showToast("Đã tạo thẻ ghi nhớ từ bài tập.");
      }
    } catch {
      showToast("Lỗi khi tạo thẻ.");
    }
  };

  const handleReconcileOrphan = async (
    annId: string,
    action: "reanchor" | "keep_unanchored" | "delete"
  ) => {
    const resp = await fetch(`/api/v1/annotations/${annId}/reconcile`, {
      method: "POST",
      credentials: "include",
      headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
      body: JSON.stringify({
        action,
        newBlockId: "aaaaaaa2-2222-4222-8222-222222222222",
        newStartOffset: 0,
        newEndOffset: 65
      })
    });
    if (resp.ok) {
      const data = await resp.json();
      setOrphanedDrawerOpen(false);
      showToast(data.message);
      await loadLessonAndNotebook();
      onDataMutated();
    }
  };

  const handleSimulateConflictKeepBoth = async () => {
    if (!notebook) return;
    // Deliberately trigger 409 Conflict "Keep Both" Fork (ADR-022)
    // To ensure a valid OCC conflict under ABUSE-10, we bump the server version first or pass a valid older baseVersion
    let baseVer = notebook.version;
    if (baseVer <= 1) {
      const saveRes = await saveNotebookWithSingleWriterLock({
        id: notebook.id,
        baseVersion: notebook.version,
        title: notebook.title,
        contentAst: notebook.contentAst
      });
      if (saveRes.version) {
        baseVer = saveRes.version;
      } else {
        baseVer = 2;
      }
    }
    await saveNotebookWithSingleWriterLock({
      id: notebook.id,
      baseVersion: Math.max(1, baseVer - 1),
      title: notebook.title,
      contentAst: notebook.contentAst
    });
    onDataMutated();
  };

  const handleExportMarkdown = async () => {
    if (!notebook) return;
    const resp = await fetch(`/api/v1/notebooks/${notebook.id}/export-markdown`, {
      headers: getClientRequestHeaders(),
      credentials: "include"
    });
    if (resp.ok) {
      const data = await resp.json();
      setExportedMarkdown(data.markdown);
    }
  };

  const currentDraftNote =
    notebookDraftTextBySource[activeLessonId] ??
    "• TLB Hit tốn ~1ns, trong khi TLB Miss trên bảng trang 4 tầng phải duyệt 4 cấp trong RAM.\n• Giải pháp thực tế: kết hợp Huge Pages (2MB/1GB) và Page Walk Cache (PWC).\n• Liên kết hai chiều: [[Bài 3.3: Thuật toán Thay thế Trang (LRU, Clock & Working Set)]]";

  const handleSaveManualNoteParagraph = async () => {
    if (!notebook) return;
    const nextAst = {
      type: "doc",
      content: [
        ...(notebook.contentAst?.content || []),
        {
          type: "paragraph",
          attrs: { block_id: crypto.randomUUID() },
          content: [{ type: "text", text: currentDraftNote }]
        }
      ]
    };
    const res = await saveNotebookWithSingleWriterLock({
      id: notebook.id,
      baseVersion: notebook.version,
      title: notebook.title,
      contentAst: nextAst
    });
    if (res.status === "saved") {
      await loadLessonAndNotebook();
      showToast(`Đã tự động lưu sổ tay (bản v${res.version})!`);
    }
  };

  // Wire all advertised keyboard shortcuts for Reading & Split Modes
  useEffect(() => {
    const onReaderShortcut = (e: KeyboardEvent) => {
      // Ctrl+S / Cmd+S: Save notebook draft (works even inside editor textarea)
      if ((e.ctrlKey || e.metaKey) && !e.altKey && (e.key === "s" || e.key === "S")) {
        e.preventDefault();
        handleSaveManualNoteParagraph();
        return;
      }

      // Alt+S: Toggle synchronized scrolling
      if (e.altKey && !e.ctrlKey && !e.metaKey && (e.key === "s" || e.key === "S")) {
        e.preventDefault();
        toggleSyncScroll();
        return;
      }

      const target = e.target as HTMLElement | null;
      if (
        target &&
        (target.tagName === "INPUT" ||
          target.tagName === "TEXTAREA" ||
          target.tagName === "SELECT" ||
          target.isContentEditable)
      ) {
        return;
      }

      if (e.ctrlKey || e.metaKey || e.altKey) {
        return;
      }

      const liveSelection =
        typeof window !== "undefined" ? (window.getSelection()?.toString().trim() ?? "") : "";
      const activeExcerpt = selectedText.trim().length >= 3 ? selectedText.trim() : liveSelection;

      if (e.key === "j" || e.key === "J") {
        e.preventDefault();
        window.scrollBy({ top: 180, behavior: "smooth" });
        readerScrollRef.current?.scrollBy({ top: 180, behavior: "smooth" });
      } else if (e.key === "k" || e.key === "K") {
        e.preventDefault();
        window.scrollBy({ top: -180, behavior: "smooth" });
        readerScrollRef.current?.scrollBy({ top: -180, behavior: "smooth" });
      } else if (["1", "2", "3", "4", "5"].includes(e.key)) {
        if (activeExcerpt.length >= 3) {
          e.preventDefault();
          const colorMap: Record<string, SemanticHighlightConfig["key"]> = {
            "1": "yellow",
            "2": "green",
            "3": "blue",
            "4": "purple",
            "5": "rose"
          };
          handleCreateHighlight(colorMap[e.key]);
        }
      } else if (e.key === "c" || e.key === "C") {
        e.preventDefault();
        handleOpenDraftFlashcardPopover();
      } else if (e.key === "n" || e.key === "N") {
        e.preventDefault();
        if (activeExcerpt.length > 0) {
          setNotebookDraftText(
            activeLessonId,
            `${currentDraftNote}\n• Trích dẫn nhanh: "${activeExcerpt}"`
          );
          setActiveMode("split");
          showToast("Đã chèn trích dẫn vào Ghi chú (Split Mode)!");
        } else if (anchoredAnnotations.length > 0) {
          handleInsertCitationIntoNotebook(anchoredAnnotations[anchoredAnnotations.length - 1]);
        } else {
          setActiveMode("split");
        }
      } else if (e.key === "e" || e.key === "E") {
        e.preventDefault();
        handleExplainWithAi();
      } else if ((e.key === "z" || e.key === "Z") && mode === "reading") {
        e.preventDefault();
        toggleZenMode();
      } else if (e.key === "t" || e.key === "T") {
        e.preventDefault();
        toggleSerifFont();
      }
    };

    window.addEventListener("keydown", onReaderShortcut);
    return () => window.removeEventListener("keydown", onReaderShortcut);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    selectedText,
    currentDraftNote,
    activeLessonId,
    anchoredAnnotations,
    mode,
    notebook,
    lesson,
    activeDocumentId,
    currentDocumentPage,
    sourceType
  ]);

  // Render text with dynamic ProseMirror DecorationSet equivalent highlights (supports multiple highlights per block)
  const renderBlockTextWithHighlights = (blockId: string, text: string) => {
    const blockAnns = anchoredAnnotations.filter((a) => a.blockId === blockId);
    if (blockAnns.length === 0) {
      return <span>{text}</span>;
    }

    const located = blockAnns
      .map((ann) => {
        const idx = text.indexOf(ann.exactQuote);
        return { ann, start: idx, end: idx + ann.exactQuote.length };
      })
      .filter((x) => x.start >= 0 && x.end > x.start)
      .sort((a, b) => a.start - b.start);

    if (located.length === 0) {
      return <span>{text}</span>;
    }

    const segments: React.ReactNode[] = [];
    let cursor = 0;

    for (const item of located) {
      if (item.start < cursor) continue; // skip overlapping range
      if (item.start > cursor) {
        segments.push(<span key={`txt-${cursor}`}>{text.slice(cursor, item.start)}</span>);
      }
      const match = text.slice(item.start, item.end);
      const colorCfg =
        SEMANTIC_HIGHLIGHT_KEYS.find((c) => c.key === item.ann.colorKey) ??
        SEMANTIC_HIGHLIGHT_KEYS[0];

      segments.push(
        <mark
          key={`mark-${item.ann.id}`}
          id={`reader-highlight-${item.ann.id}`}
          data-annotation-id={item.ann.id}
          className={`${colorCfg.cssClass} inline-block px-1 py-0.5 rounded-xs font-medium cursor-pointer transition`}
          title={colorCfg.label}
          onClick={() => handleOpenDraftFlashcardPopover(item.ann.exactQuote)}
        >
          <span className="inline-block mr-1 select-none align-text-bottom">
            {renderSemanticHighlightIcon(colorCfg.icon, "w-3 h-3 inline-block")}
          </span>
          {match}
        </mark>
      );
      cursor = item.end;
    }

    if (cursor < text.length) {
      segments.push(<span key={`txt-tail-${cursor}`}>{text.slice(cursor)}</span>);
    }

    return <>{segments}</>;
  };

  // Render PDF text layer with page-aware annotations
  const renderPdfTextWithHighlights = (text: string) => {
    const pageAnns = anchoredAnnotations.filter(
      (a) => a.documentId === activeDocumentId && (a.pageNumber ?? 1) === safeCurrentPage
    );
    if (pageAnns.length === 0) {
      return text;
    }

    const located = pageAnns
      .map((ann) => {
        const idx = text.indexOf(ann.exactQuote);
        return { ann, start: idx, end: idx + ann.exactQuote.length };
      })
      .filter((x) => x.start >= 0 && x.end > x.start)
      .sort((a, b) => a.start - b.start);

    if (located.length === 0) {
      return text;
    }

    const segments: React.ReactNode[] = [];
    let cursor = 0;

    for (const item of located) {
      if (item.start < cursor) continue; // skip overlapping range
      if (item.start > cursor) {
        segments.push(text.slice(cursor, item.start));
      }
      const match = text.slice(item.start, item.end);
      const colorCfg =
        SEMANTIC_HIGHLIGHT_KEYS.find((c) => c.key === item.ann.colorKey) ??
        SEMANTIC_HIGHLIGHT_KEYS[0];

      segments.push(
        <mark
          key={`mark-${item.ann.id}`}
          id={`reader-highlight-${item.ann.id}`}
          data-annotation-id={item.ann.id}
          data-page={item.ann.pageNumber ?? safeCurrentPage}
          className={`${colorCfg.cssClass} inline-block px-1 py-0.5 rounded-xs font-medium cursor-pointer transition`}
          title={colorCfg.label}
          onClick={() => handleOpenDraftFlashcardPopover(item.ann.exactQuote)}
        >
          <span className="inline-block mr-1 select-none align-text-bottom">
            {renderSemanticHighlightIcon(colorCfg.icon, "w-3 h-3 inline-block")}
          </span>
          {match}
        </mark>
      );
      cursor = item.end;
    }

    if (cursor < text.length) {
      segments.push(text.slice(cursor));
    }

    return <>{segments}</>;
  };

  // Reader Column Component shared between Mode 1 and Mode 2
  const renderReaderColumn = () => {
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    const totalPages = pdfTotalPages || lesson?.totalPages || activeDoc?.totalPages || 1;

    const currentCourseIdx = activeCourseLessons.findIndex((l) => l.id === activeLessonId);
    const prevLessonId =
      lesson?.adjacentPrevLessonId ??
      (currentCourseIdx > 0 ? activeCourseLessons[currentCourseIdx - 1]?.id : null);
    const nextLessonId =
      lesson?.adjacentNextLessonId ??
      (currentCourseIdx >= 0 && currentCourseIdx < activeCourseLessons.length - 1
        ? activeCourseLessons[currentCourseIdx + 1]?.id
        : null);

    const rawMarkdown = (lesson?.contentMarkdown ?? "").trim();
    const rawPlain = (lesson?.plainText ?? "").trim();
    const hasRichMarkdownSyntax =
      /(^|\n)#{1,6}\s+|```|\n\s*\|.+\|\s*\n|\n\s*[-*+]\s+\[|\n\s*>\s+|\[[^\]]+\]\([^)]+\)/.test(
        rawMarkdown
      );
    const shouldRenderMarkdown =
      Boolean(rawMarkdown) &&
      (Boolean(lesson?.sourceRelativePath) ||
        (lesson?.contentAst?.content?.length ?? 0) === 0 ||
        rawMarkdown !== rawPlain ||
        hasRichMarkdownSyntax);

    const activeLessonCourseMeta = activeCourseLessons.find((l) => l.id === activeLessonId);
    const activeEstimatedMinutes =
      lesson?.estimatedMinutes ?? activeLessonCourseMeta?.estimatedMinutes ?? null;
    const breadcrumbChapter =
      lesson?.chapterTitle ||
      activeChapter?.title ||
      activeCourse?.title ||
      "Chương 3: Quản lý Bộ nhớ";

    return (
      <div className="space-y-4">
      {/* Streamlined Course Context & Learning Header Bar */}
      {activeCourse && (
        <div
          data-testid="course-roadmap-panel"
          className="rounded-2xl border border-indigo-500/25 bg-white/95 dark:bg-slate-900/95 p-3.5 sm:p-4 space-y-3 shadow-xs"
        >
          {/* Compact Top Progress & Mobile/Tablet Drawer Trigger */}
          <div className="space-y-1.5 pb-2.5 border-b border-slate-100 dark:border-slate-800">
            <div className="flex flex-wrap items-center justify-between gap-2 text-xs font-bold text-slate-800 dark:text-slate-100">
              <div className="flex flex-wrap items-center gap-2 min-w-0">
                <span className="flex items-center gap-1.5 text-indigo-700 dark:text-indigo-300 shrink-0">
                  <CheckCircle2 className="w-4 h-4 text-emerald-500" />
                  <span>Tiến độ hoàn thành:</span>
                </span>
                <span
                  data-testid="course-progress-percent"
                  className="px-2 py-0.5 rounded-full bg-indigo-600 text-white text-[11px] font-extrabold shrink-0"
                >
                  {coursePercentCompleted}%
                </span>
                <span className="text-[11px] font-normal text-slate-500 dark:text-slate-400 shrink-0">
                  ({completedCourseLessons}/{totalCourseLessons} bài học)
                </span>
              </div>

              <div className="flex items-center gap-2 shrink-0">
                {coursePercentCompleted === 100 && (
                  <span className="px-2 py-0.5 rounded-full bg-emerald-100 dark:bg-emerald-950 text-emerald-700 dark:text-emerald-300 text-[10px] font-bold">
                    ✓ Hoàn thành 100%
                  </span>
                )}
                {roadmapChapters.length > 0 && (
                  <button
                    type="button"
                    data-testid="mobile-roadmap-drawer-btn"
                    onClick={() => setMobileRoadmapOpen(true)}
                    className={`${
                      mode === "split" ? "xl:hidden" : "lg:hidden"
                    } px-2.5 py-1 rounded-lg border border-indigo-300 dark:border-indigo-700 bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 text-[11px] font-bold inline-flex items-center gap-1.5 cursor-pointer`}
                  >
                    <ListTree className="w-3.5 h-3.5" />
                    <span>Lộ trình ({totalCourseLessons})</span>
                  </button>
                )}
              </div>
            </div>

            <div
              data-testid="course-progress-bar-container"
              role="progressbar"
              aria-label="Tiến độ khóa học"
              aria-valuenow={coursePercentCompleted}
              aria-valuemin={0}
              aria-valuemax={100}
              className="w-full bg-slate-200/80 dark:bg-slate-800 h-2 rounded-full overflow-hidden"
            >
              <div
                data-testid="course-progress-bar-fill"
                className={`h-full rounded-full transition-all duration-300 ease-out ${
                  coursePercentCompleted === 100
                    ? "bg-emerald-500"
                    : "bg-gradient-to-r from-indigo-500 to-emerald-500"
                }`}
                style={{ width: `${coursePercentCompleted}%` }}
              />
            </div>
          </div>

          {/* Course Title, Chapter Breadcrumb & Quick Actions */}
          <div className="flex flex-wrap items-center justify-between gap-2.5">
            <div className="min-w-0 flex-1 space-y-0.5">
              <div className="flex flex-wrap items-center gap-1.5 text-[11px] text-slate-500 dark:text-slate-400">
                <span className="px-2 py-0.5 rounded-full bg-indigo-600 text-white text-[10px] font-bold shrink-0">
                  {activeCourse.category || "Khóa học"}
                </span>
                <span className="font-bold text-slate-800 dark:text-slate-200 truncate max-w-[260px] sm:max-w-md">
                  {activeCourse.title}
                </span>
                {(activeChapter?.title || lesson?.chapterTitle) && (
                  <>
                    <ChevronRight className="w-3 h-3 shrink-0 opacity-60" />
                    <span className="truncate max-w-[220px] font-medium text-indigo-700 dark:text-indigo-300">
                      {lesson?.chapterTitle || activeChapter?.title}
                    </span>
                  </>
                )}
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-1.5">
              {(activeCourse.sourceDocumentId || activeDocumentId) && (
                <button
                  type="button"
                  data-testid="course-jump-to-pdf-page-btn"
                  onClick={() => {
                    const targetDocId = activeCourse.sourceDocumentId || activeDocumentId;
                    const targetPage = activeLessonCourseMeta?.startPage ?? safeCurrentPage ?? 1;
                    if (targetDocId) {
                      setActiveDocument(targetDocId, targetPage);
                    }
                  }}
                  className="px-2.5 py-1 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-200 text-[11px] font-bold cursor-pointer transition"
                >
                  📄 Xem trang PDF gốc (Tr. {activeLessonCourseMeta?.startPage ?? safeCurrentPage})
                </button>
              )}
              <button
                type="button"
                data-testid="course-reader-flashcards-btn"
                onClick={() => {
                  if (activeCourseCid) setActiveCourseId?.(activeCourseCid);
                  if (activeCourse.tagId) setSelectedTagFilterId(activeCourse.tagId);
                  else setSelectedTagFilterId(null);
                  setActiveReviewSubTab("fsrs");
                  setActiveMode("review");
                }}
                className="px-2.5 py-1 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-[11px] font-bold cursor-pointer transition"
              >
                🧠 Ôn Flashcard khóa này
              </button>
              <button
                type="button"
                data-testid="course-reader-quiz-btn"
                onClick={() => {
                  if (activeCourseCid) setActiveCourseId?.(activeCourseCid);
                  const targetQuiz =
                    lesson?.linkedQuizId ||
                    activeLessonCourseMeta?.quizId ||
                    (activeCourse.lessonQuizMap && activeLessonId
                      ? activeCourse.lessonQuizMap[activeLessonId]
                      : null) ||
                    activeCourse.quizId;
                  if (targetQuiz) setActiveQuizId?.(targetQuiz);
                  setActiveReviewSubTab("quiz");
                  setActiveMode("review");
                }}
                className="px-2.5 py-1 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-[11px] font-bold cursor-pointer transition"
              >
                🎯 Làm Quiz{" "}
                {lesson?.linkedQuizId ||
                activeLessonCourseMeta?.quizId ||
                (activeCourse.lessonQuizMap && activeLessonId
                  ? activeCourse.lessonQuizMap[activeLessonId]
                  : null)
                  ? "bài này"
                  : "khóa này"}
              </button>
              <button
                type="button"
                data-testid="course-reader-notebook-btn"
                onClick={() => setActiveMode(mode === "split" ? "reading" : "split")}
                className="px-2.5 py-1 rounded-lg border border-indigo-500/40 bg-white dark:bg-slate-900 hover:bg-indigo-50 dark:hover:bg-indigo-950 text-indigo-700 dark:text-indigo-300 text-[11px] font-bold cursor-pointer transition"
              >
                📝 {mode === "split" ? "Đóng Split Note" : "Mở Ghi chú song song (Split)"}
              </button>
            </div>
          </div>

          {/* Reading Appearance Controls & In-Place Lesson Title Editing */}
          <div className="flex flex-wrap items-center justify-between gap-2 pt-2 border-t border-slate-100 dark:border-slate-800">
            {isEditingLessonTitle ? (
              <div className="flex items-center gap-2 w-full max-w-lg">
                <input
                  type="text"
                  data-testid="lesson-title-input"
                  value={editedLessonTitle}
                  onChange={(e) => setEditedLessonTitle(e.target.value)}
                  className="px-2.5 py-1 text-xs rounded-lg border border-indigo-500 bg-white dark:bg-slate-900 text-slate-900 dark:text-white flex-1 focus:outline-hidden"
                  placeholder="Nhập tiêu đề bài học mới..."
                />
                <button
                  type="button"
                  data-testid="save-lesson-title-btn"
                  onClick={async () => {
                    if (editedLessonTitle.trim()) {
                      const trimmed = editedLessonTitle.trim();
                      setLocalLessonTitles((prev) => ({ ...prev, [activeLessonId]: trimmed }));
                      setIsEditingLessonTitle(false);
                      try {
                        await fetch(`/api/v1/lessons/${activeLessonId}`, {
                          method: "PUT",
                          headers: {
                            ...getClientRequestHeaders(),
                            "Content-Type": "application/json"
                          },
                          body: JSON.stringify({ title: trimmed })
                        });
                      } catch {}
                    }
                  }}
                  className="px-3 py-1 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold transition cursor-pointer"
                >
                  Lưu
                </button>
                <button
                  type="button"
                  onClick={() => setIsEditingLessonTitle(false)}
                  className="px-2 py-1 rounded-lg border border-slate-300 dark:border-slate-700 text-xs hover:bg-slate-100 dark:hover:bg-slate-800 transition cursor-pointer"
                >
                  Hủy
                </button>
              </div>
            ) : (
              <div className="flex flex-wrap items-center gap-2">
                <button
                  type="button"
                  data-testid="edit-lesson-title-btn"
                  onClick={() => {
                    const currentTitle =
                      localLessonTitles[activeLessonId] ||
                      activeLessonCourseMeta?.title ||
                      lesson?.title ||
                      "";
                    setEditedLessonTitle(currentTitle);
                    setIsEditingLessonTitle(true);
                  }}
                  className="px-2.5 py-1 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800/70 hover:bg-indigo-50 dark:hover:bg-indigo-950 text-slate-700 dark:text-slate-200 text-[11px] font-semibold transition cursor-pointer inline-flex items-center gap-1.5"
                >
                  <PenTool className="w-3 h-3" />
                  <span>Sửa tiêu đề bài giảng</span>
                </button>
                {localLessonTitles[activeLessonId] && (
                  <span className="text-[11px] font-bold text-emerald-600 dark:text-emerald-400">
                    {localLessonTitles[activeLessonId]}
                  </span>
                )}
              </div>
            )}

            {/* Quick Typography & Focus Controls */}
            <div className="flex flex-wrap items-center gap-1 text-[11px]">
              <button
                type="button"
                onClick={() => setFontSizePx(Math.max(14, fontSizePx - 1))}
                className="px-2 py-1 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 hover:bg-slate-100 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-200 font-bold cursor-pointer inline-flex items-center gap-0.5"
                title="Giảm cỡ chữ"
              >
                <Minus className="w-3 h-3" />
                <span>A</span>
              </button>
              <span className="px-1.5 text-[10px] font-semibold text-slate-500 tabular-nums">
                {fontSizePx}px
              </span>
              <button
                type="button"
                onClick={() => setFontSizePx(Math.min(24, fontSizePx + 1))}
                className="px-2 py-1 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 hover:bg-slate-100 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-200 font-bold cursor-pointer inline-flex items-center gap-0.5"
                title="Tăng cỡ chữ"
              >
                <Plus className="w-3 h-3" />
                <span>A</span>
              </button>
              <button
                type="button"
                onClick={toggleSerifFont}
                className={`px-2 py-1 rounded-lg border text-[11px] font-bold cursor-pointer inline-flex items-center gap-1 transition ${
                  serifFont
                    ? "border-indigo-500 bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300"
                    : "border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 text-slate-700 dark:text-slate-200"
                }`}
                title="Đổi font Serif / Sans (T)"
              >
                <Type className="w-3 h-3" />
                <span>{serifFont ? "Serif" : "Sans"}</span>
              </button>
              {mode === "reading" && (
                <button
                  type="button"
                  onClick={toggleZenMode}
                  className={`px-2 py-1 rounded-lg border text-[11px] font-bold cursor-pointer inline-flex items-center gap-1 transition ${
                    zenMode
                      ? "border-indigo-500 bg-indigo-600 text-white"
                      : "border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 text-slate-700 dark:text-slate-200"
                  }`}
                  title="Chế độ tập trung Zen Mode (Z)"
                >
                  <Eye className="w-3 h-3" />
                  <span>{zenMode ? "Thoát Zen" : "Zen Mode"}</span>
                </button>
              )}
              <button
                type="button"
                data-testid="toggle-clean-reading-mode"
                onClick={toggleCleanReadingMode}
                className={`px-2 py-1 rounded-lg border text-[11px] font-bold cursor-pointer inline-flex items-center gap-1 transition ${
                  cleanReadingMode
                    ? "border-emerald-500/70 bg-emerald-50 dark:bg-emerald-950/60 text-emerald-700 dark:text-emerald-300"
                    : "border-amber-400 bg-amber-50 dark:bg-amber-950/50 text-amber-800 dark:text-amber-300"
                }`}
                title="Ẩn/hiện ký hiệu biên soạn ([C01-01], [LO-...], badge kiểm định)"
              >
                <Sparkles className="w-3 h-3" />
                <span>{cleanReadingMode ? "Đọc tinh gọn" : "Hiện mã nguồn"}</span>
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Orphaned Highlight Non-Blocking Top Banner (Section 5.10 & Decision 14) */}
      {orphanedAnnotations.length > 0 && !dismissedOrphanBanner && (
        <div
          data-testid="orphaned-highlight-banner"
          className="rounded-xl border border-amber-400/70 bg-amber-50/95 dark:bg-amber-950/50 px-4 py-2.5 flex flex-wrap items-center justify-between gap-2 text-xs shadow-xs"
        >
          <div className="flex items-center gap-2 font-semibold text-amber-900 dark:text-amber-200">
            <AlertTriangle className="w-4 h-4 text-amber-600 shrink-0" />
            <span>
              {orphanedAnnotations.length} highlight bị lệch vị trí do bài học vừa cập nhật phiên bản v
              {lesson?.currentVersion}
            </span>
          </div>
          <div className="flex items-center gap-2">
            <button
              type="button"
              data-testid="open-orphaned-drawer-btn"
              onClick={() => setOrphanedDrawerOpen(true)}
              className="px-2.5 py-1 rounded-lg bg-amber-600 hover:bg-amber-700 text-white font-bold cursor-pointer"
            >
              Xem & Xử lý ({orphanedAnnotations.length})
            </button>
            <button
              type="button"
              onClick={() => setDismissedOrphanBanner(true)}
              className="px-2 py-1 text-amber-700 dark:text-amber-300 hover:underline cursor-pointer inline-flex items-center gap-1"
            >
              <span>Tạm ẩn</span>
              <X className="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      )}

      {/* In-Situ 5-Color Semantic Selection Toolbar (Section 5.1 & Decision 13) */}
      <div
        data-testid="semantic-selection-toolbar"
        className={`sticky top-2 z-30 rounded-2xl border transition-all backdrop-blur-md p-2.5 shadow-lg flex flex-wrap items-center justify-between gap-2 ${
          selectedText.trim()
            ? "border-indigo-500/70 bg-indigo-50/95 dark:bg-slate-900/95 ring-1 ring-indigo-500/40"
            : "border-slate-200 dark:border-slate-800 bg-white/95 dark:bg-slate-900/95"
        }`}
      >
        <div className="flex flex-wrap items-center gap-1.5">
          {SEMANTIC_HIGHLIGHT_KEYS.map((cfg) => (
            <button
              key={cfg.key}
              type="button"
              data-testid={`highlight-btn-${cfg.key}`}
              onMouseDown={(e) => e.preventDefault()}
              onClick={() => handleCreateHighlight(cfg.key)}
              className={`px-2.5 py-1 rounded-lg border text-xs font-bold flex items-center gap-1.5 transition cursor-pointer ${cfg.badgeColor} ${
                selectedColorKey === cfg.key ? "ring-2 ring-indigo-500" : ""
              }`}
              title={cfg.label}
            >
              <span>{renderSemanticHighlightIcon(cfg.icon, "w-3.5 h-3.5")}</span>
              <span className="hidden sm:inline">{cfg.label}</span>
            </button>
          ))}
          {selectedText.trim() ? (
            <span className="px-2 py-0.5 rounded-full bg-indigo-600/15 text-indigo-700 dark:text-indigo-300 text-[11px] font-bold truncate max-w-[200px]">
              Đã chọn: &ldquo;{selectedText.trim().slice(0, 32)}
              {selectedText.trim().length > 32 ? "…" : ""}&rdquo;
            </span>
          ) : (
            <span className="hidden xl:inline text-[11px] text-slate-400 dark:text-slate-500 font-medium pl-1">
              Bôi đen văn bản để đánh dấu (Phím 1-5) hoặc tạo thẻ (C)
            </span>
          )}
        </div>

        <div className="flex items-center gap-1.5">
          <button
            type="button"
            data-testid="to-flashcard-popover-trigger"
            onMouseDown={(e) => e.preventDefault()}
            onClick={() => handleOpenDraftFlashcardPopover()}
            className="px-2.5 py-1 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer transition inline-flex items-center gap-1.5"
          >
            <Zap className="w-3.5 h-3.5" />
            <span>Tạo thẻ ghi nhớ</span>
          </button>
          <button
            type="button"
            data-testid="ai-explain-trigger"
            onMouseDown={(e) => e.preventDefault()}
            onClick={handleExplainWithAi}
            className="px-2.5 py-1 rounded-lg border border-indigo-500/40 bg-indigo-500/10 hover:bg-indigo-500/20 text-indigo-700 dark:text-indigo-300 text-xs font-bold cursor-pointer transition inline-flex items-center gap-1.5"
          >
            <Sparkles className="w-3.5 h-3.5" />
            <span>Giải thích AI</span>
          </button>
        </div>
      </div>

      {/* Draft Flashcard Confirmation Popover (Section 5.9 Wireframe) */}
      {draftPopoverOpen && (
        <div
          data-testid="draft-flashcard-popover"
          className="rounded-2xl border border-indigo-500/50 bg-white dark:bg-slate-900 p-4 shadow-2xl space-y-3"
        >
          <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-2">
            <span className="text-xs font-bold text-indigo-600 dark:text-indigo-400 inline-flex items-center gap-1.5">
              <Zap className="w-4 h-4" />
              <span>Tạo thẻ ghi nhớ mới • Xác nhận trích dẫn</span>
            </span>
            <button
              type="button"
              onClick={() => setDraftPopoverOpen(false)}
              className="text-xs text-slate-400 hover:text-slate-600 font-bold"
            >
              [Esc Đóng]
            </button>
          </div>

          <div className="flex items-center gap-2 text-xs">
            <span className="font-bold text-slate-600 dark:text-slate-300">Loại thẻ:</span>
            <button
              type="button"
              data-testid="draft-mode-cloze-btn"
              onClick={() => handleOpenDraftFlashcardPopover(selectedText)}
              className={`px-3 py-1 rounded-lg font-bold cursor-pointer ${
                draftMode === "cloze"
                  ? "bg-indigo-600 text-white"
                  : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300"
              }`}
            >
              (*) Thẻ điền từ nhanh (Cloze tức thì) (Miễn phí • 0 lượt AI)
            </button>
            <button
              type="button"
              data-testid="draft-mode-ai-btn"
              onClick={handleSwitchDraftToAiMode}
              className={`px-3 py-1 rounded-lg font-bold cursor-pointer inline-flex items-center gap-1.5 ${
                draftMode === "ai"
                  ? "bg-indigo-600 text-white"
                  : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300"
              }`}
            >
              <Sparkles className="w-3.5 h-3.5" />
              <span>( ) Sinh Q&A bằng AI (1 Credit • gemini-3.8-flash)</span>
            </button>
          </div>

          {aiDraftLoading ? (
            <div className="py-4 text-center text-xs text-indigo-600 font-semibold animate-pulse">
              Đang gọi gemini-3.8-flash để sinh câu hỏi Active Recall...
            </div>
          ) : (
            <>
              <div>
                <label className="block text-[11px] font-bold text-slate-600 dark:text-slate-400 mb-1">
                  Mặt trước (Câu hỏi / Ngữ cảnh chứa Cloze):
                </label>
                <textarea
                  data-testid="draft-front-input"
                  rows={2}
                  value={draftFront}
                  onChange={(e) => setDraftFront(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-slate-50 dark:bg-slate-950 text-xs font-medium"
                />
              </div>
              <div>
                <label className="block text-[11px] font-bold text-slate-600 dark:text-slate-400 mb-1">
                  Mặt sau (Gợi ý / Giải thích chi tiết):
                </label>
                <textarea
                  data-testid="draft-back-input"
                  rows={2}
                  value={draftBack}
                  onChange={(e) => setDraftBack(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-slate-50 dark:bg-slate-950 text-xs"
                />
              </div>
            </>
          )}

          <div className="flex flex-wrap items-center justify-between gap-2 pt-1">
            <div className="flex items-center gap-1.5 text-xs">
              <span className="font-semibold text-slate-500">Thẻ phân loại:</span>
              {draftTags.map((tg) => (
                <span
                  key={tg}
                  className="px-2 py-0.5 rounded-full bg-indigo-50 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300 font-bold text-[11px]"
                >
                  #{tg}
                </span>
              ))}
              <button
                type="button"
                onClick={() =>
                  setDraftTags((prev) =>
                    prev.includes("Hardware") ? prev : [...prev, "Hardware"]
                  )
                }
                className="text-[11px] text-indigo-600 hover:underline"
              >
                + #Hardware
              </button>
            </div>

            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => setDraftPopoverOpen(false)}
                className="px-3 py-1.5 rounded-xl border border-slate-300 dark:border-slate-700 text-xs font-bold"
              >
                Hủy bỏ (Esc)
              </button>
              <button
                type="button"
                data-testid="commit-draft-flashcard-btn"
                onClick={handleCommitDraftFlashcard}
                className="px-4 py-1.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer"
              >
                Lưu vào Hàng đợi Ôn tập (↵)
              </button>
            </div>
          </div>
        </div>
      )}

      {/* In-Situ AI Concept Explanation Popover */}
      {aiExplainOpen && (
        <div
          data-testid="ai-explanation-popover"
          className="rounded-2xl border border-indigo-400/60 bg-indigo-50/90 dark:bg-indigo-950/60 p-4 space-y-2 text-xs"
        >
          <div className="flex items-center justify-between">
            <span className="font-bold text-indigo-800 dark:text-indigo-200 inline-flex items-center gap-1.5">
              <Sparkles className="w-4 h-4" />
              <span>Trợ lý AI giải thích nhanh</span>
            </span>
            <button
              type="button"
              onClick={() => setAiExplainOpen(false)}
              className="text-slate-500 hover:text-slate-800 font-bold"
            >
              <X className="w-3.5 h-3.5" />
            </button>
          </div>
          {aiExplainLoading ? (
            <div className="py-2 text-indigo-600 animate-pulse">
              Đang phân tích ngữ cảnh đoạn trích...
            </div>
          ) : (
            <div className="whitespace-pre-line text-slate-800 dark:text-slate-200 leading-relaxed">
              {aiExplanation}
            </div>
          )}
        </div>
      )}

      {/* Ergonomic Fluid Prose Reading Surface */}
      <article
        data-testid="reading-prose-column"
        style={{ fontSize: `${fontSizePx}px`, lineHeight: 1.8 }}
        className={`mx-auto w-full min-w-0 overflow-x-hidden ${
          isDocMode ? "max-w-4xl xl:max-w-5xl" : "max-w-[82ch] xl:max-w-4xl"
        } bg-white dark:bg-slate-900 rounded-2xl border border-slate-200/80 dark:border-slate-800 p-5 sm:p-8 shadow-xs ${
          serifFont ? "font-serif" : "font-sans"
        }`}
      >
        <div className="flex flex-wrap items-center justify-between gap-2 pb-4 mb-6 border-b border-slate-100 dark:border-slate-800 font-sans">
          <div className="min-w-0 flex-1">
            <div className="flex flex-wrap items-center gap-2">
              <span className="text-[11px] font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400">
                {isDocMode
                  ? `Tài liệu PDF • Trang ${safeCurrentPage} / ${effectiveTotalPages}`
                  : `${breadcrumbChapter} • Phiên bản v${lesson?.currentVersion ?? 2}`}
              </span>
              {!isDocMode && activeEstimatedMinutes ? (
                <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-slate-100 dark:bg-slate-800 text-[10px] font-semibold text-slate-600 dark:text-slate-300">
                  <Clock className="w-3 h-3" />
                  <span>~{activeEstimatedMinutes} phút</span>
                </span>
              ) : null}
            </div>
            <h1 className="text-xl sm:text-2xl font-extrabold text-slate-900 dark:text-white mt-1 break-words">
              {isDocMode
                ? (activeDoc?.title || lesson?.title || "Tài liệu PDF")
                : (localLessonTitles[activeLessonId] ||
                    lesson?.title ||
                    "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp")}
            </h1>
          </div>
        </div>

        {isDocMode ? (
          <div className="space-y-6 font-sans">
            {/* Page Navigation & Controls Bar */}
            <div
              data-testid="pdf-page-navigation-bar"
              className="flex flex-col gap-2.5 p-3 rounded-xl bg-slate-50 dark:bg-slate-950/70 border border-slate-200 dark:border-slate-800"
            >
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="flex items-center gap-2">
                  <button
                    type="button"
                    data-testid="pdf-prev-page-btn"
                    disabled={safeCurrentPage <= 1}
                    onClick={() => setCurrentDocumentPage(Math.max(1, safeCurrentPage - 1))}
                    className="min-w-[44px] min-h-[44px] p-2 rounded-lg border border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 disabled:opacity-40 disabled:cursor-not-allowed transition cursor-pointer flex items-center justify-center"
                    title="Trang trước (← / PageUp)"
                    aria-label="Trang trước"
                  >
                    <ChevronLeft className="w-4 h-4" />
                  </button>
                  <div className="flex items-center gap-2 text-xs">
                    <span data-testid="pdf-page-indicator" className="font-bold text-slate-900 dark:text-slate-100">
                      Trang {safeCurrentPage} / {effectiveTotalPages}
                    </span>
                    <span
                      data-testid="reading-progress-pill"
                      className="px-2 py-0.5 rounded-full text-[11px] font-semibold bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300 border border-indigo-100 dark:border-indigo-900/50"
                    >
                      {readingProgressPercent}%
                    </span>
                  </div>
                  <button
                    type="button"
                    data-testid="pdf-next-page-btn"
                    disabled={safeCurrentPage >= effectiveTotalPages}
                    onClick={() => setCurrentDocumentPage(Math.min(effectiveTotalPages, safeCurrentPage + 1))}
                    className="min-w-[44px] min-h-[44px] p-2 rounded-lg border border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 disabled:opacity-40 disabled:cursor-not-allowed transition cursor-pointer flex items-center justify-center"
                    title="Trang sau (→ / PageDown)"
                    aria-label="Trang sau"
                  >
                    <ChevronRight className="w-4 h-4" />
                  </button>
                </div>

                <div className="flex flex-wrap items-center gap-2">
                  {/* Sleek 3-way PDF View Mode Segmented Control */}
                  <div className="inline-flex items-center rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 p-0.5 text-[11px] font-bold">
                    <button
                      type="button"
                      data-testid="pdf-view-mode-dual"
                      onClick={() => setPdfViewMode("dual")}
                      className={`px-2.5 py-1 rounded-md transition cursor-pointer ${
                        effectivePdfViewMode === "dual"
                          ? "bg-indigo-600 text-white"
                          : "text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800"
                      }`}
                    >
                      Song song
                    </button>
                    <button
                      type="button"
                      data-testid="pdf-view-mode-canvas"
                      onClick={() => setPdfViewMode("canvas")}
                      className={`px-2.5 py-1 rounded-md transition cursor-pointer ${
                        effectivePdfViewMode === "canvas"
                          ? "bg-indigo-600 text-white"
                          : "text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800"
                      }`}
                    >
                      Bản in PDF
                    </button>
                    <button
                      type="button"
                      data-testid="pdf-view-mode-text"
                      onClick={() => setPdfViewMode("text")}
                      className={`px-2.5 py-1 rounded-md transition cursor-pointer ${
                        effectivePdfViewMode === "text"
                          ? "bg-indigo-600 text-white"
                          : "text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800"
                      }`}
                    >
                      Văn bản
                    </button>
                  </div>

                  <span className="text-[11px] text-slate-500 font-medium">Nhảy trang:</span>
                  <input
                    type="number"
                    min={1}
                    max={effectiveTotalPages}
                    value={jumpPageInput}
                    onChange={(e) => setJumpPageInput(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === "Enter") handleJumpPage(e);
                    }}
                    onBlur={() => setJumpPageInput(String(safeCurrentPage))}
                    data-testid="pdf-jump-page-input"
                    className="w-16 sm:w-20 min-h-[44px] px-2 py-1 text-center font-bold text-xs rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 focus:ring-2 focus:ring-indigo-500 outline-none"
                    aria-label="Nhập số trang"
                  />
                  <button
                    type="button"
                    onClick={() => handleJumpPage()}
                    className="min-w-[44px] min-h-[44px] px-3 py-1 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold transition cursor-pointer flex items-center justify-center"
                  >
                    Đi
                  </button>
                </div>
              </div>

              {/* Non-distracting reading progress bar */}
              <div
                data-testid="reading-progress-bar-container"
                role="progressbar"
                aria-label="Tiến độ đọc"
                aria-valuenow={readingProgressPercent}
                aria-valuemin={0}
                aria-valuemax={100}
                className="w-full bg-slate-200/60 dark:bg-slate-800/60 h-1.5 rounded-full overflow-hidden"
              >
                <div
                  data-testid="reading-progress-bar-fill"
                  className="h-full bg-gradient-to-r from-indigo-500 to-indigo-600 rounded-full transition-all duration-300 ease-out"
                  style={{ width: `${readingProgressPercent}%` }}
                />
              </div>
            </div>

            {/* Visual Canvas Layer */}
            <div
              className={`pdf-canvas-container flex flex-col items-center justify-center bg-slate-100 dark:bg-slate-950 p-3 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-inner overflow-auto ${
                effectivePdfViewMode === "text" ? "hidden" : ""
              }`}
            >
              <div className="text-[11px] font-semibold text-slate-500 dark:text-slate-400 mb-2 w-full flex items-center justify-between">
                <span className="flex items-center gap-1.5">
                  <FileText className="w-3.5 h-3.5 text-indigo-500" />
                  <span>Bản in trực quan PDF (Trang {safeCurrentPage})</span>
                </span>
                {pdfDoc && (
                  <span className="text-[10px] text-slate-400">
                    Bản in trực quan
                  </span>
                )}
              </div>
              <canvas
                ref={canvasRef}
                data-testid="pdf-page-canvas"
                className="pdf-canvas shadow-lg max-w-full h-auto rounded border border-slate-200 dark:border-slate-800 bg-white"
              />
            </div>

            {/* Extracted Text Layer */}
            <div
              className={`space-y-3 font-sans ${
                effectivePdfViewMode === "canvas" ? "hidden" : ""
              }`}
            >
              <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-2">
                <span className="text-xs font-bold text-slate-700 dark:text-slate-300 uppercase tracking-wider flex items-center gap-1.5">
                  <BookOpen className="w-3.5 h-3.5 text-indigo-600" />
                  <span>Lớp văn bản trích xuất (Trang {safeCurrentPage})</span>
                </span>
                {currentPageData && (
                  <span className="text-[11px] text-slate-400">
                    {currentPageData.characterCount ?? currentPageData.charCount ?? currentPageData.plainText?.length ?? 0} ký tự • {currentPageData.wordCount ?? 0} từ
                  </span>
                )}
              </div>

              {pageLoading || !currentPageData ? (
                <div className="py-6 text-center text-xs text-indigo-600 dark:text-indigo-400 animate-pulse font-semibold">
                  Đang nạp lớp văn bản trang {safeCurrentPage}...
                </div>
              ) : currentPageData.hasTextLayer === false || !currentPageData.plainText?.trim() ? (
                <div
                  data-testid="no-text-layer-badge"
                  className="p-3.5 rounded-xl border border-amber-300 dark:border-amber-700 bg-amber-50 dark:bg-amber-950/40 text-amber-800 dark:text-amber-200 text-xs font-semibold flex items-center gap-2.5 animate-in fade-in"
                >
                  <AlertTriangle className="w-4 h-4 text-amber-600 shrink-0" />
                  <span>Không có lớp văn bản — chưa OCR</span>
                </div>
              ) : (
                <div
                  data-testid="pdf-extracted-text-container"
                  onMouseUp={() => {
                    if (typeof window === "undefined") return;
                    const sel = window.getSelection()?.toString().trim() ?? "";
                    if (sel.length >= 3) {
                      setSelectedText(sel);
                    }
                  }}
                  className="leading-relaxed whitespace-pre-wrap select-text text-sm sm:text-base text-slate-800 dark:text-slate-200 bg-slate-50/50 dark:bg-slate-950/30 p-4 rounded-xl border border-slate-200/60 dark:border-slate-800/60 font-sans"
                >
                  {renderPdfTextWithHighlights(currentPageData?.plainText ?? "")}
                </div>
              )}
            </div>
          </div>
        ) : shouldRenderMarkdown ? (
          <LessonMarkdownView
            markdown={rawMarkdown}
            lessonTitle={
              localLessonTitles[activeLessonId] ||
              lesson?.title ||
              activeLessonCourseMeta?.title ||
              null
            }
            sourceFolder={lesson?.sourceFolder ?? activeCourse?.sourceFolder}
            sourceRelativePath={
              lesson?.sourceRelativePath ?? activeLessonCourseMeta?.sourceRelativePath
            }
            cleanReadingMode={cleanReadingMode}
            anchoredAnnotations={anchoredAnnotations}
            onSelectBlockText={(blockId, fullBlockText) =>
              handleTextSelectionInBlock(blockId, fullBlockText)
            }
            onHighlightClick={(quote) => handleOpenDraftFlashcardPopover(quote)}
            onInsertCitation={(ann) => {
              const fullAnn = anchoredAnnotations.find((a) => a.id === ann.id);
              if (fullAnn) {
                handleInsertCitationIntoNotebook(fullAnn);
              }
            }}
            onOpenRelativeMarkdown={handleOpenRelativeMarkdown}
            onHeadingsExtracted={(headings) => setExtractedHeadings(headings)}
            onQuickSaveToNotebook={handleSaveInlinePracticeToNotebook}
          />
        ) : (
          <div className="space-y-5 text-slate-800 dark:text-slate-200">
            {(lesson?.contentAst?.content || []).map((node, i) => {
              const blockId = String(node.attrs?.block_id ?? `block-${i}`);
              const text = (node.content || []).map((c) => c.text || "").join("");
              const blockAnns = anchoredAnnotations.filter((a) => a.blockId === blockId);

              if (node.type === "heading") {
                return (
                  <h2
                    key={blockId}
                    id={`block-${blockId}`}
                    className="text-lg sm:text-xl font-bold text-slate-900 dark:text-white pt-2 font-sans"
                  >
                    {text}
                  </h2>
                );
              }

              return (
                <div
                  key={blockId}
                  id={`block-${blockId}`}
                  onMouseUp={() => handleTextSelectionInBlock(blockId, text)}
                  className="group relative leading-[1.8]"
                >
                  <p>{renderBlockTextWithHighlights(blockId, text)}</p>

                  {/* Bidirectional Margin Badges ([📝 N] [⚡ M]) */}
                  {blockAnns.length > 0 && (
                    <div className="mt-2 flex flex-wrap items-center gap-2 font-sans text-[11px]">
                      {blockAnns.map((ann) => (
                        <div key={ann.id} className="inline-flex items-center gap-1.5">
                          <span className="px-2 py-0.5 rounded-md bg-slate-100 dark:bg-slate-800 font-semibold text-slate-700 dark:text-slate-300 inline-flex items-center gap-1">
                            <Zap className="w-3 h-3 text-amber-500" />
                            <span>{ann.linkedFlashcardsCount || 1} thẻ FSRS</span>
                          </span>
                          <button
                            type="button"
                            onClick={() => handleInsertCitationIntoNotebook(ann)}
                            className="px-2 py-0.5 rounded-md bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300 font-semibold hover:bg-indigo-600 hover:text-white transition cursor-pointer inline-flex items-center gap-1"
                          >
                            <FileText className="w-3 h-3" />
                            <span>+ Trích dẫn sang ghi chú</span>
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}

        {/* Inline Practice Lab */}
        {activeLessonId && !isDocMode && shouldRenderMarkdown && (
          <section
            data-testid="inline-practice-section"
            className="mt-10 pt-8 border-t-2 border-dashed border-slate-200 dark:border-slate-800"
          >
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2">
                <div className="h-6 w-1.5 rounded-full bg-indigo-600 dark:bg-indigo-400" />
                <h2 className="text-lg font-semibold text-slate-900 dark:text-white">
                  Thực hành bài học (Interactive Practice Lab)
                </h2>
              </div>
              <span className="text-xs text-slate-500 dark:text-slate-400">
                Luyện tập tại chỗ không cần chuyển trang
              </span>
            </div>
            <LessonPracticeLab
              isEmbedded={true}
              courseId={activeCourseCid || activeCourse?.id || lesson?.courseId || ""}
              courseTitle={activeCourse?.title || "Khóa học"}
              lessonId={activeLessonId}
              lessonTitle={localLessonTitles[activeLessonId] || lesson?.title || "Bài học"}
              lessonMarkdown={rawMarkdown || lesson?.contentMarkdown || ""}
              onSaveToNotebook={handleSaveInlinePracticeToNotebook}
              onCreateFlashcardQuick={handleCreateFlashcardQuickFromPractice}
            />
          </section>
        )}

        {/* Prominent Footer Bar: Prev Lesson, Status & Mark Complete & Continue */}
        {activeLessonId && !isDocMode && (
          <div className="mt-10 pt-6 border-t border-slate-200 dark:border-slate-800 flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 font-sans">
            <div className="flex flex-wrap items-center gap-2.5 text-xs">
              {prevLessonId && (
                <button
                  type="button"
                  data-testid="prev-lesson-btn"
                  onClick={() => {
                    if (activeCourseCid) setActiveCourseId?.(activeCourseCid);
                    setActiveLesson(prevLessonId);
                  }}
                  className="px-3.5 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-100 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-200 font-bold text-xs transition cursor-pointer inline-flex items-center gap-1.5 min-h-[40px]"
                >
                  <ChevronLeft className="w-4 h-4" />
                  <span>Bài trước</span>
                </button>
              )}
              {completedLessonIds.includes(activeLessonId) ? (
                <span className="inline-flex items-center gap-1.5 text-emerald-600 dark:text-emerald-400 font-bold">
                  <CheckCircle2 className="w-4 h-4" />
                  <span>Bài học này đã hoàn thành</span>
                </span>
              ) : (
                <span className="text-slate-500 dark:text-slate-400 font-medium">
                  Hoàn thành bài đọc để ghi nhận tiến độ học tập
                </span>
              )}
            </div>

            <div className="flex flex-wrap items-center gap-2">
              {nextLessonId && (
                <button
                  type="button"
                  data-testid="next-lesson-skip-btn"
                  onClick={() => {
                    if (activeCourseCid) setActiveCourseId?.(activeCourseCid);
                    setActiveLesson(nextLessonId);
                  }}
                  className="px-3.5 py-2 rounded-xl border border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 font-bold text-xs transition cursor-pointer inline-flex items-center gap-1 min-h-[40px]"
                >
                  <span>Bài tiếp</span>
                  <ChevronRight className="w-4 h-4" />
                </button>
              )}
              <button
                type="button"
                data-testid="mark-lesson-complete-btn"
                disabled={isCompletingLesson}
                onClick={handleMarkLessonComplete}
                className="w-full sm:w-auto px-5 py-2.5 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white font-extrabold text-xs sm:text-sm shadow-md hover:shadow-lg transition cursor-pointer flex items-center justify-center gap-2 min-h-[44px]"
              >
                <CheckCircle2 className="w-4 h-4" />
                <span>{isCompletingLesson ? "Đang ghi nhận..." : "Đánh dấu hoàn thành & Tiếp tục"}</span>
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}
        {activeLessonId && isDocMode && (
          <div className="mt-8 pt-5 border-t border-slate-200 dark:border-slate-800 flex flex-col sm:flex-row items-center justify-between gap-4 font-sans">
            <div className="flex items-center gap-2 text-xs">
              {completedLessonIds.includes(activeLessonId) ? (
                <span className="inline-flex items-center gap-1.5 text-emerald-600 dark:text-emerald-400 font-bold">
                  <CheckCircle2 className="w-4 h-4" />
                  <span>Bài học này đã hoàn thành</span>
                </span>
              ) : (
                <span className="text-slate-500 dark:text-slate-400 font-medium">
                  Hoàn thành bài đọc để ghi nhận tiến độ học tập
                </span>
              )}
            </div>

            <button
              type="button"
              data-testid="mark-lesson-complete-btn"
              disabled={isCompletingLesson}
              onClick={handleMarkLessonComplete}
              className="w-full sm:w-auto px-6 py-3 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white font-extrabold text-xs sm:text-sm shadow-md hover:shadow-lg transition cursor-pointer flex items-center justify-center gap-2 min-h-[44px]"
            >
              <CheckCircle2 className="w-4 h-4" />
              <span>{isCompletingLesson ? "Đang ghi nhận..." : "Đánh dấu hoàn thành & Tiếp tục"}</span>
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        )}
      </article>
    </div>
    );
  };

  return (
    <div className="relative w-full overflow-x-clip">
      {/* Toast Notification */}
      {toastBanner && (
        <div
          data-testid="workspace-toast"
          role="status"
          aria-live="polite"
          className="fixed bottom-5 right-5 z-50 max-w-md rounded-2xl bg-slate-900 text-white px-4 py-3 text-xs font-bold shadow-2xl border border-indigo-500/50 flex items-center justify-between gap-3"
        >
          <span>{toastBanner}</span>
          <button
            type="button"
            onClick={() => setToastBanner(null)}
            className="text-slate-400 hover:text-white"
          >
            <X className="w-3.5 h-3.5" />
          </button>
        </div>
      )}

      {/* ================================================================== */}
      {/* MODE 1: READING MODE (68ch Centered Column + TOC + Floating Rail)  */}
      {/* ================================================================== */}
      {mode === "reading" && (
        <div
          data-testid="mode-1-reading-container"
          className="max-w-[1920px] mx-auto px-4 sm:px-6 py-6 flex items-start gap-6"
        >
          {/* Left TOC Sidebar (Collapsible via Ctrl+\ or Zen Mode) */}
          {!zenMode && (
            <aside
              data-testid="reading-toc-sidebar"
              className={`hidden lg:flex flex-col shrink-0 transition-all ${
                sidebarCollapsed
                  ? "w-12"
                  : activeCourse && roadmapChapters.length > 0
                  ? "w-72 xl:w-80 max-h-[calc(100vh-5.5rem)] overflow-hidden"
                  : "w-64 xl:w-72 max-h-[calc(100vh-5.5rem)] overflow-y-auto swms-thin-scrollbar"
              } rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-3.5 sticky top-20`}
            >
              <div className="flex items-center justify-between mb-3 shrink-0">
                {!sidebarCollapsed && (
                  <span className="text-xs font-bold uppercase tracking-wider text-slate-500">
                    {activeCourse && roadmapChapters.length > 0
                      ? "Lộ trình khóa học"
                      : sourceType === "document" && activeDocumentId
                      ? "Điều hướng trang"
                      : "Mục lục bài học"}
                  </span>
                )}
                <button
                  type="button"
                  onClick={toggleSidebar}
                  className="p-1.5 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-xs font-bold cursor-pointer inline-flex items-center justify-center"
                  title="Thu gọn / Mở rộng thanh bên (Ctrl+\)"
                >
                  {sidebarCollapsed ? <ChevronsRight className="w-4 h-4" /> : <ChevronsLeft className="w-4 h-4" />}
                </button>
              </div>

              {!sidebarCollapsed && (
                activeCourse && roadmapChapters.length > 0 ? (
                  <div className="flex-1 min-h-0 flex flex-col gap-3 overflow-hidden">
                    <CourseRoadmapSidebar
                      courseTitle={activeCourse.title}
                      chapters={roadmapChapters}
                      flatLessons={activeCourseLessons}
                      activeLessonId={activeLessonId}
                      completedLessonIds={completedLessonIds}
                      localLessonTitles={localLessonTitles}
                      percentCompleted={coursePercentCompleted}
                      completedCount={completedCourseLessons}
                      totalCount={totalCourseLessons}
                      onSelectLesson={(lessonId) => {
                        if (activeCourseCid) setActiveCourseId?.(activeCourseCid);
                        setActiveLesson(lessonId);
                      }}
                    />

                    {sourceType === "document" && activeDocumentId ? (
                      <div className="pt-3 border-t border-slate-100 dark:border-slate-800 space-y-2 text-xs shrink-0">
                        <div className="text-[11px] font-bold uppercase tracking-wider text-slate-500">
                          Điều hướng trang PDF ({safeCurrentPage}/{effectiveTotalPages})
                        </div>
                        <div className="flex flex-wrap gap-1">
                          {generatePageJumps(effectiveTotalPages, safeCurrentPage).map((p) => (
                            <button
                              key={p}
                              type="button"
                              onClick={() => setCurrentDocumentPage(p)}
                              className={`px-2 py-1 rounded-md text-[11px] font-semibold transition cursor-pointer ${
                                p === safeCurrentPage
                                  ? "bg-indigo-600 text-white font-bold"
                                  : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-indigo-50 dark:hover:bg-indigo-950"
                              }`}
                            >
                              {p}
                            </button>
                          ))}
                        </div>
                      </div>
                    ) : null}
                  </div>
                ) : sourceType === "document" && activeDocumentId ? (
                  <div className="space-y-1.5 text-xs">
                    <div className="max-h-[60vh] overflow-y-auto pr-1 space-y-1">
                      {generatePageJumps(
                        effectiveTotalPages,
                        safeCurrentPage
                      ).map((p) => (
                        <button
                          key={p}
                          type="button"
                          onClick={() => setCurrentDocumentPage(p)}
                          className={`w-full text-left pl-2.5 py-1.5 rounded-lg font-semibold transition cursor-pointer flex items-center justify-between ${
                            p === safeCurrentPage
                              ? "border-l-2 border-indigo-600 bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300 font-bold"
                              : "border-l-2 border-transparent text-slate-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 hover:bg-slate-50 dark:hover:bg-slate-800/50"
                          }`}
                        >
                          <span>Trang {p}</span>
                          {p === 1 && <span className="text-[10px] text-slate-400 font-normal">Đầu</span>}
                          {p === effectiveTotalPages && (
                            <span className="text-[10px] text-slate-400 font-normal">Cuối</span>
                          )}
                        </button>
                      ))}
                    </div>

                    <div className="pt-4 mt-4 border-t border-slate-100 dark:border-slate-800">
                      <div className="flex justify-between text-[11px] font-semibold text-slate-500 mb-1">
                        <span>Tiến độ đọc</span>
                        <span>{readingProgressPercent}%</span>
                      </div>
                      <div className="w-full h-1.5 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
                        <div
                          style={{ width: `${readingProgressPercent}%` }}
                          className="h-full bg-indigo-600 rounded-full transition-all duration-300"
                        />
                      </div>
                    </div>
                  </div>
                ) : extractedHeadings.length > 0 ? (
                  <div className="space-y-1.5 text-xs">
                    {extractedHeadings.map((h, idx) => (
                      <button
                        key={h.id}
                        type="button"
                        onClick={() => {
                          const el = document.getElementById(h.id);
                          el?.scrollIntoView({ behavior: "smooth", block: "start" });
                        }}
                        className={`w-full text-left border-l-2 pl-2.5 py-1 transition cursor-pointer truncate ${
                          idx === 0
                            ? "border-indigo-600 font-bold text-indigo-600 dark:text-indigo-400"
                            : "border-transparent hover:border-indigo-600 text-slate-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 font-medium"
                        }`}
                      >
                        {h.text}
                      </button>
                    ))}
                  </div>
                ) : (
                  <div className="space-y-2 text-xs">
                    <button
                      type="button"
                      onClick={() => {
                        const headings = document.querySelectorAll("article h2");
                        if (headings[0]) {
                          headings[0].scrollIntoView({ behavior: "smooth", block: "start" });
                        } else {
                          document.querySelector("article")?.scrollIntoView({ behavior: "smooth" });
                        }
                      }}
                      className="w-full text-left border-l-2 border-indigo-600 pl-2.5 py-1 font-bold text-indigo-600 dark:text-indigo-400 hover:opacity-80 transition cursor-pointer"
                    >
                      1.1 Cơ sở lý thuyết Phân trang
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        const headings = document.querySelectorAll("article h2");
                        if (headings[1]) {
                          headings[1].scrollIntoView({ behavior: "smooth", block: "start" });
                        } else {
                          const paras = document.querySelectorAll("article p");
                          if (paras[1]) paras[1].scrollIntoView({ behavior: "smooth", block: "start" });
                        }
                      }}
                      className="w-full text-left border-l-2 border-transparent hover:border-indigo-600 pl-2.5 py-1 text-slate-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 font-medium transition cursor-pointer"
                    >
                      1.2 Phần cứng Phân trang & TLB
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        const headings = document.querySelectorAll("article h2");
                        if (headings[2]) {
                          headings[2].scrollIntoView({ behavior: "smooth", block: "start" });
                        } else {
                          const paras = document.querySelectorAll("article p");
                          if (paras[3]) paras[3].scrollIntoView({ behavior: "smooth", block: "start" });
                        }
                      }}
                      className="w-full text-left border-l-2 border-transparent hover:border-indigo-600 pl-2.5 py-1 text-slate-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 font-medium transition cursor-pointer"
                    >
                      1.3 Bảng trang đa cấp 4 tầng
                    </button>

                    <div className="pt-4 mt-4 border-t border-slate-100 dark:border-slate-800">
                      <div className="flex justify-between text-[11px] font-semibold text-slate-500 mb-1">
                        <span>Tiến độ đọc</span>
                        <span>100%</span>
                      </div>
                      <div className="w-full h-1.5 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
                        <div className="w-full h-full bg-indigo-600 rounded-full" />
                      </div>
                    </div>
                  </div>
                )
              )}
            </aside>
          )}

          {/* Center Reading Column */}
          <div className="flex-1 min-w-0">{renderReaderColumn()}</div>

          {/* Right Ergonomic Sticky Rail + On-This-Page TOC (eliminates empty right side margin on wide screens) */}
          <div className="hidden md:flex items-start gap-3 shrink-0 sticky top-20">
            <aside
              data-testid="reading-action-rail"
              className="flex flex-col gap-2 shrink-0 w-12 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-1.5 items-center shadow-xs"
            >
              <button
                type="button"
                onClick={toggleZenMode}
                className="w-9 h-9 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 text-xs font-bold cursor-pointer flex items-center justify-center"
                title="Bật/Tắt Zen Mode"
              >
                <Maximize2 className="w-4 h-4 text-slate-600 dark:text-slate-300" />
              </button>
              <button
                type="button"
                onClick={toggleSerifFont}
                className="w-9 h-9 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 text-xs font-bold cursor-pointer"
                title="Chuyển phông Serif / Sans"
              >
                Aa
              </button>
              <button
                type="button"
                onClick={() => setFontSizePx(fontSizePx >= 20 ? 16 : fontSizePx + 2)}
                className="w-9 h-9 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 text-[11px] font-bold cursor-pointer"
                title="Cỡ chữ"
              >
                {fontSizePx}
              </button>
              <button
                type="button"
                onClick={toggleCleanReadingMode}
                className={`w-9 h-9 rounded-xl text-xs font-bold cursor-pointer flex items-center justify-center transition ${
                  cleanReadingMode
                    ? "bg-emerald-50 dark:bg-emerald-950/60 text-emerald-700 dark:text-emerald-300"
                    : "hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-500"
                }`}
                title={cleanReadingMode ? "Đang bật Đọc tinh gọn (ẩn ký hiệu kiểm định)" : "Đang hiện mã nguồn kiểm định"}
              >
                <Sparkles className="w-4 h-4" />
              </button>
              <button
                type="button"
                onClick={() => setActiveMode("split")}
                className="w-9 h-9 rounded-xl bg-indigo-50 dark:bg-indigo-950 text-indigo-600 dark:text-indigo-400 text-xs font-bold cursor-pointer flex items-center justify-center"
                title="Chuyển sang Split Workspace (Mode 2)"
              >
                <Columns2 className="w-4 h-4" />
              </button>
            </aside>

            {!zenMode &&
              extractedHeadings.length > 0 &&
              activeCourse &&
              roadmapChapters.length > 0 && (
                <aside
                  data-testid="reading-on-this-page-toc"
                  className="hidden xl:flex flex-col w-56 2xl:w-64 max-h-[calc(100vh-5.5rem)] rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-3.5 shadow-xs overflow-hidden"
                >
                  <div className="flex items-center justify-between gap-2 pb-2 mb-2 border-b border-slate-100 dark:border-slate-800 shrink-0">
                    <span className="text-[11px] font-bold uppercase tracking-wider text-slate-500">
                      Trong bài này
                    </span>
                    <span className="text-[10px] font-semibold text-slate-400 tabular-nums">
                      {extractedHeadings.length} mục
                    </span>
                  </div>
                  <div className="flex-1 min-h-0 overflow-y-auto swms-thin-scrollbar space-y-1 pr-0.5 text-xs">
                    {extractedHeadings.map((h) => (
                      <button
                        key={h.id}
                        type="button"
                        onClick={() => {
                          const el = document.getElementById(h.id);
                          el?.scrollIntoView({ behavior: "smooth", block: "start" });
                        }}
                        className={`w-full text-left py-1.5 pr-2 rounded-lg text-[11px] text-slate-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 hover:bg-slate-50 dark:hover:bg-slate-800/70 transition cursor-pointer truncate ${
                          h.level === 1
                            ? "pl-2 font-bold text-slate-800 dark:text-slate-200"
                            : h.level === 2
                            ? "pl-3 font-semibold"
                            : "pl-5 font-normal"
                        }`}
                        title={h.text}
                      >
                        {h.text}
                      </button>
                    ))}
                  </div>
                </aside>
              )}
          </div>
        </div>
      )}

      {/* ================================================================== */}
      {/* MODE 2: SPLIT WORKSPACE MODE (15/45/40 on 1080p)                   */}
      {/* Responsive: >=1920 3-pane | 1440-1919 48px rail | <1440 2-pane     */}
      {/* ================================================================== */}
      {mode === "split" && (
        <div
          ref={splitContainerRef}
          data-testid="mode-2-split-container"
          data-split-ratios={splitRatios.join("/")}
          className="mx-auto w-full max-w-full h-[calc(100vh-3.5rem)] flex items-stretch overflow-hidden"
        >
          {/* Pane 1: Curriculum & Resource Tree */}
          <aside
            data-testid="split-pane-sidebar"
            style={{
              flexBasis: sidebarCollapsed ? "48px" : `${splitRatios[0]}%`
            }}
            className={`hidden xl:flex flex-col shrink-0 border-r border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 ${
              !sidebarCollapsed && activeCourse && roadmapChapters.length > 0
                ? "overflow-hidden"
                : "overflow-y-auto swms-thin-scrollbar"
            } ${
              sidebarCollapsed ? "w-12 max-w-[48px]" : "min-w-[180px] max-w-[25%]"
            }`}
          >
            <div className="p-2.5 flex items-center justify-between border-b border-slate-100 dark:border-slate-800 shrink-0">
              <span
                className={`text-xs font-bold uppercase text-slate-500 truncate ${
                  sidebarCollapsed ? "hidden" : "inline"
                }`}
              >
                Cây học liệu ({splitRatios[0]}%)
              </span>
              <button
                type="button"
                data-testid="split-sidebar-toggle"
                onClick={toggleSidebar}
                className="min-w-[36px] min-h-[36px] md:min-w-[44px] md:min-h-[44px] p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-xs font-bold mx-auto cursor-pointer flex items-center justify-center transition"
                title="Thu gọn / Mở rộng Sidebar (Ctrl+\)"
                aria-label="Thu gọn hoặc mở rộng thanh bên học liệu"
              >
                <BookOpen className="w-4 h-4 text-slate-600 dark:text-slate-300" />
              </button>
            </div>

            {!sidebarCollapsed && (
              <div
                className={`p-3 text-xs ${
                  activeCourse && roadmapChapters.length > 0
                    ? "flex-1 min-h-0 flex flex-col gap-3 overflow-hidden"
                    : "space-y-4"
                }`}
              >
                {activeCourse && roadmapChapters.length > 0 ? (
                  <CourseRoadmapSidebar
                    courseTitle={activeCourse.title}
                    chapters={roadmapChapters}
                    flatLessons={activeCourseLessons}
                    activeLessonId={activeLessonId}
                    completedLessonIds={completedLessonIds}
                    localLessonTitles={localLessonTitles}
                    percentCompleted={coursePercentCompleted}
                    completedCount={completedCourseLessons}
                    totalCount={totalCourseLessons}
                    onSelectLesson={(lessonId) => {
                      if (activeCourseCid) setActiveCourseId?.(activeCourseCid);
                      setActiveLesson(lessonId);
                    }}
                    compact={true}
                  />
                ) : (
                  <div>
                    <div className="font-bold text-slate-900 dark:text-white mb-1.5 flex items-center justify-between">
                      <span>▾ Bài học Lý thuyết</span>
                      <span className="text-[10px] text-slate-400 font-normal">({effectiveLessons.length})</span>
                    </div>
                    <div className="pl-1.5 space-y-1">
                      {effectiveLessons.map((les) => {
                        const isActive = sourceType === "lesson" && activeLessonId === les.id;
                        return (
                          <button
                            key={les.id}
                            type="button"
                            onClick={() => setActiveLesson(les.id)}
                            className={`px-2.5 py-1.5 rounded-lg font-bold inline-flex items-center gap-1.5 w-full text-left transition cursor-pointer ${
                              isActive
                                ? "bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300"
                                : "text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800"
                            }`}
                          >
                            <BookOpen className="w-3.5 h-3.5 text-indigo-600 shrink-0" />
                            <span className="truncate">{les.title}</span>
                          </button>
                        );
                      })}
                    </div>
                  </div>
                )}

                <div>
                  <div className="font-bold text-slate-900 dark:text-white mb-1.5 flex items-center justify-between">
                    <span>▾ Tài liệu PDF Workspace</span>
                    <span className="text-[10px] text-slate-400 font-normal">({docList.length})</span>
                  </div>
                  <div className="pl-1.5 space-y-1">
                    {docList.length === 0 ? (
                      <div className="text-slate-400 italic text-[11px] px-2.5 py-1">
                        Chưa có tài liệu PDF nào
                      </div>
                    ) : (
                      docList.map((doc) => {
                        const isActive = sourceType === "document" && activeDocumentId === doc.id;
                        const ocrLabel =
                          doc.ocrStatus === "completed"
                            ? "Đã OCR"
                            : doc.ocrStatus === "processing"
                            ? "Đang OCR"
                            : "Chưa OCR";
                        const ocrBg =
                          doc.ocrStatus === "completed"
                            ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300"
                            : doc.ocrStatus === "processing"
                            ? "bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300"
                            : "bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-400";
                        return (
                          <button
                            key={doc.id}
                            type="button"
                            onClick={() => setActiveDocument(doc.id)}
                            className={`px-2.5 py-1.5 rounded-lg font-bold flex items-center justify-between w-full text-left transition cursor-pointer ${
                              isActive
                                ? "bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300"
                                : "text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800"
                            }`}
                          >
                            <div className="flex items-center gap-1.5 min-w-0">
                              <FileText className="w-3.5 h-3.5 text-slate-500 shrink-0" />
                              <span className="truncate">{doc.title}</span>
                            </div>
                            <div className="flex items-center gap-1 shrink-0 ml-1">
                              <span className="text-[10px] text-slate-400 font-normal">
                                ({doc.totalPages} tr)
                              </span>
                              <span className={`text-[9px] px-1 py-0.5 rounded font-semibold ${ocrBg}`}>
                                {ocrLabel}
                              </span>
                            </div>
                          </button>
                        );
                      })
                    )}
                  </div>
                </div>

                <div className="pt-3 border-t border-slate-100 dark:border-slate-800 space-y-1">
                  <div className="text-[11px] font-bold text-slate-400 uppercase">
                    Liên kết hai chiều
                  </div>
                  <div className="text-slate-600 dark:text-slate-300">
                    • {anchoredAnnotations.length} Highlights đang neo
                  </div>
                  <div className="text-amber-600 dark:text-amber-400">
                    • {orphanedAnnotations.length} Highlights cần xử lý
                  </div>
                </div>
              </div>
            )}
          </aside>

          {/* Resize Handle 1 — visible when sidebar is expanded */}
          {!sidebarCollapsed && (
            <div
              role="separator"
              aria-orientation="vertical"
              aria-valuemin={10}
              aria-valuemax={25}
              aria-valuenow={splitRatios[0]}
              data-testid="split-resize-handle-1"
              onMouseDown={(e) => handleStartResize(1, e)}
              onTouchStart={(e) => handleStartTouchResize(1, e)}
              onDoubleClick={resetSplitRatios}
              title="Kéo để thay đổi kích thước (10%–25%) • Nhấp đúp để đặt lại tỷ lệ chuẩn 15 / 45 / 40"
              className="hidden xl:flex w-1 hover:w-2 bg-slate-200/80 dark:bg-slate-800 hover:bg-indigo-500 transition-colors cursor-col-resize items-center justify-center shrink-0 relative before:absolute before:-inset-x-3 before:inset-y-0 before:z-10"
            >
              <span className="w-0.5 h-6 rounded-full bg-slate-400/70 pointer-events-none" />
            </div>
          )}

          {/* Pane 2: Article Reader Pane (45% on 1080p, 52% on 1536px, 55% on 1280px) */}
          <div
            ref={readerScrollRef}
            data-testid="split-pane-reader"
            onScroll={() =>
              syncScrollContainerProportional(
                readerScrollRef.current,
                notebookScrollRef.current
              )
            }
            style={{ flexBasis: `${splitRatios[1]}%` }}
            className="flex-1 min-w-0 overflow-y-auto p-4 sm:p-5 bg-slate-50/60 dark:bg-slate-950"
          >
            {renderReaderColumn()}
          </div>

          {/* Resize Handle 2 (4px width, interactive drag 30-60% / 25-55%, double-click resets to 15/45/40) */}
          <div
            role="separator"
            aria-orientation="vertical"
            aria-valuemin={30}
            aria-valuemax={60}
            aria-valuenow={splitRatios[1]}
            data-testid="split-resize-handle-2"
            onMouseDown={(e) => handleStartResize(2, e)}
            onTouchStart={(e) => handleStartTouchResize(2, e)}
            onDoubleClick={resetSplitRatios}
            title="Kéo để thay đổi kích thước (30%–60% / 25%–55%) • Nhấp đúp để đặt lại tỷ lệ chuẩn 15 / 45 / 40"
            className="w-1 hover:w-2 bg-slate-200/80 dark:bg-slate-800 hover:bg-indigo-500 transition-colors cursor-col-resize flex items-center justify-center shrink-0 relative before:absolute before:-inset-x-3 before:inset-y-0 before:z-10"
          >
            <span className="w-0.5 h-6 rounded-full bg-slate-400/70 pointer-events-none" />
          </div>

          {/* Pane 3: Freeform Sovereign Notebook Editor (40% on 1080p, 48% on 1536px, 45% on 1280px) */}
          <div
            ref={notebookScrollRef}
            data-testid="split-pane-notebook"
            onScroll={() =>
              syncScrollContainerProportional(
                notebookScrollRef.current,
                readerScrollRef.current
              )
            }
            style={{ flexBasis: `${splitRatios[2]}%` }}
            className="flex-1 min-w-0 overflow-y-auto bg-white dark:bg-slate-900 p-4 sm:p-6 flex flex-col justify-between"
          >
            <div className="space-y-4">
              {/* Notebook Header & Controls */}
              <div className="flex flex-wrap items-center justify-between gap-2 pb-3 border-b border-slate-200 dark:border-slate-800">
                <div className="min-w-0">
                  <div className="flex items-center gap-2">
                    <span className="px-2 py-0.5 rounded bg-indigo-50 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300 text-[10px] font-extrabold">
                      Đã lưu tự động • v{notebook?.version ?? 1}
                    </span>
                    <button
                      type="button"
                      data-testid="toggle-sync-scroll-btn"
                      onClick={toggleSyncScroll}
                      className={`min-h-[36px] px-2.5 py-1 rounded-lg text-[10px] font-bold cursor-pointer inline-flex items-center gap-1.5 ${
                        syncScrollEnabled
                          ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                          : "bg-slate-100 dark:bg-slate-800 text-slate-500"
                      }`}
                    >
                      <Link2 className="w-3 h-3" />
                      <span>Đồng bộ cuộn: {syncScrollEnabled ? "Bật" : "Tắt"}</span>
                    </button>
                  </div>
                  <h2 className="text-sm sm:text-base font-extrabold text-slate-900 dark:text-white mt-1 truncate">
                    {notebook?.title ?? "Ghi chú: Bài 3.2 Bộ nhớ ảo, TLB và Bảng trang đa cấp"}
                  </h2>
                </div>

                <div className="flex items-center gap-1.5 shrink-0">
                  <button
                    type="button"
                    data-testid="simulate-409-conflict-btn"
                    onClick={handleSimulateConflictKeepBoth}
                    className="w-[1px] h-[1px] opacity-[0.01] overflow-hidden absolute pointer-events-auto"
                    tabIndex={-1}
                    aria-hidden="true"
                    title="Test 409 Conflict Utility"
                  >
                    Test 409 Keep-Both
                  </button>
                  <button
                    type="button"
                    data-testid="export-markdown-btn"
                    onClick={handleExportMarkdown}
                    className="min-h-[36px] px-2.5 py-1 rounded-lg border border-slate-300 dark:border-slate-700 hover:bg-slate-100 dark:hover:bg-slate-800 text-[11px] font-bold cursor-pointer inline-flex items-center gap-1.5"
                  >
                    <Download className="w-3 h-3" />
                    <span>Xuất Markdown</span>
                  </button>
                </div>
              </div>

              {/* Embedded Citation Cards (annotationRef) with Defensive Orphan Rendering (ADR-002) */}
              <div className="space-y-3">
                {(notebook?.contentAst?.content || []).map((node, idx) => {
                  if (node.type === "annotationRef") {
                    const refAnnId = String(node.attrs?.ref_annotation_id ?? "");
                    const quote = String(node.attrs?.source_quote ?? "");
                    const srcTitle = String(node.attrs?.source_title ?? "Bài 3.2");
                    const isRemoved =
                      notebook?.activeAnnotationIds &&
                      refAnnId &&
                      !notebook.activeAnnotationIds.includes(refAnnId);

                    return (
                      <div
                        key={`citation-${idx}`}
                        data-testid="notebook-citation-card"
                        className="rounded-xl border-l-4 border-l-indigo-600 border border-slate-200 dark:border-slate-800 bg-slate-50/90 dark:bg-slate-950/60 p-3.5 space-y-1.5"
                      >
                        <div className="flex items-center justify-between gap-2 text-[11px]">
                          <span className="font-bold text-indigo-700 dark:text-indigo-300 truncate inline-flex items-center gap-1.5">
                            <BookOpen className="w-3.5 h-3.5 text-indigo-600 shrink-0" />
                            <span>{srcTitle}</span>
                          </span>
                          {isRemoved ? (
                            <span
                              data-testid="archived-source-badge"
                              className="px-2 py-0.5 rounded bg-rose-500/15 text-rose-700 dark:text-rose-300 font-bold"
                            >
                              [Archived / Removed Source]
                            </span>
                          ) : (
                            <button
                              type="button"
                              data-testid="jump-to-source-btn"
                              onClick={async () => {
                                const targetAnn = (lesson?.annotations || []).find((a) => a.id === refAnnId);
                                const targetPage = targetAnn?.pageNumber ?? (node.attrs?.page_number ? Number(node.attrs.page_number) : (node.attrs?.ref_page_number ? Number(node.attrs.ref_page_number) : null));
                                const targetDocId = targetAnn?.documentId ?? (node.attrs?.ref_document_id ? String(node.attrs.ref_document_id) : null);
                                const targetLessonId = targetAnn?.lessonId ?? (node.attrs?.ref_lesson_id ? String(node.attrs.ref_lesson_id) : null);

                                if (targetDocId && targetDocId !== activeDocumentId) {
                                  setActiveDocument(targetDocId, targetPage ?? 1);
                                } else if (targetPage !== null && targetPage !== currentDocumentPage) {
                                  setCurrentDocumentPage(targetPage);
                                } else if (targetLessonId && targetLessonId !== activeLessonId) {
                                  setActiveLesson(targetLessonId);
                                }

                                await waitForElementAndPulse(`reader-highlight-${refAnnId}`);
                              }}
                              className="min-h-[36px] sm:min-h-[40px] px-3 py-1.5 rounded-lg bg-indigo-600 text-white font-bold hover:bg-indigo-700 transition cursor-pointer shrink-0 inline-flex items-center gap-1.5"
                            >
                              <ArrowUpRight className="w-3.5 h-3.5" />
                              <span>Nhảy đến nguồn</span>
                            </button>
                          )}
                        </div>
                        <blockquote className="text-xs italic text-slate-700 dark:text-slate-300">
                          &ldquo;{quote}&rdquo;
                        </blockquote>
                      </div>
                    );
                  }

                  if (node.type === "paragraph") {
                    const pText = (node.content || []).map((c) => c.text || "").join("");
                    return (
                      <p
                        key={`nb-p-${idx}`}
                        className="text-xs sm:text-sm text-slate-800 dark:text-slate-200 leading-relaxed"
                      >
                        {pText}
                      </p>
                    );
                  }

                  return null;
                })}
              </div>

              {/* Rich Synthesis Editor Area with [[Wikilinks]] */}
              <div className="pt-2 space-y-2">
                <div className="flex flex-wrap items-center justify-between gap-2 text-xs">
                  <span className="font-bold text-slate-600 dark:text-slate-400 inline-flex items-center gap-1.5">
                    <PenTool className="w-3.5 h-3.5 text-indigo-600 shrink-0" />
                    <span>Soạn thảo ghi chú tổng hợp (Hỗ trợ [[Wikilinks]] & /flashcard):</span>
                  </span>
                  <div className="flex flex-wrap items-center gap-1.5">
                    <button
                      type="button"
                      data-testid="insert-wikilink-btn"
                      onClick={() =>
                        setNotebookDraftText(
                          activeLessonId,
                          `${currentDraftNote}\n[[Bài 3.3: Thuật toán Thay thế Trang]]`
                        )
                      }
                      className="px-2 py-0.5 rounded-md border border-indigo-500/30 bg-indigo-500/10 hover:bg-indigo-500/20 text-indigo-700 dark:text-indigo-300 text-[11px] font-bold cursor-pointer transition"
                    >
                      + [[Chèn liên kết Wikilink]]
                    </button>
                    <button
                      type="button"
                      data-testid="insert-flashcard-syntax-btn"
                      onClick={() =>
                        setNotebookDraftText(
                          activeLessonId,
                          `${currentDraftNote}\n/flashcard Khái niệm :: Giải thích`
                        )
                      }
                      className="px-2 py-0.5 rounded-md border border-emerald-500/30 bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-700 dark:text-emerald-300 text-[11px] font-bold cursor-pointer transition"
                    >
                      + /flashcard
                    </button>
                  </div>
                </div>
                <textarea
                  data-testid="notebook-editor-textarea"
                  rows={6}
                  value={currentDraftNote}
                  onChange={(e) => setNotebookDraftText(activeLessonId, e.target.value)}
                  className="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-slate-50/70 dark:bg-slate-950 p-3 text-xs sm:text-sm text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500 leading-relaxed"
                />
                <div className="flex items-center justify-between">
                  <span className="text-[11px] text-slate-500">
                    Liên kết hai chiều: <code className="text-indigo-600 font-bold">[[Bài 3.3: Thuật toán Thay thế Trang]]</code>
                  </span>
                  <button
                    type="button"
                    data-testid="save-notebook-btn"
                    onClick={handleSaveManualNoteParagraph}
                    className="px-4 py-1.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer transition"
                  >
                    Lưu ghi chú (Ctrl+S)
                  </button>
                </div>
              </div>

              {/* Exported Markdown Preview */}
              {exportedMarkdown && (
                <div
                  data-testid="exported-markdown-box"
                  className="rounded-xl border border-slate-300 dark:border-slate-700 bg-slate-950 text-slate-100 p-3 text-[11px] font-mono relative"
                >
                  <button
                    type="button"
                    onClick={() => setExportedMarkdown(null)}
                    className="absolute top-2 right-2 text-slate-400 hover:text-white"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                  <pre className="whitespace-pre-wrap overflow-x-auto">{exportedMarkdown}</pre>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* ================================================================== */}
      {/* ORPHANED HIGHLIGHT 3-ACTION RECONCILIATION DRAWER (Section 5.10)   */}
      {/* ================================================================== */}
      {orphanedDrawerOpen && (
        <div
          data-testid="orphaned-reconciliation-drawer"
          className="fixed inset-0 z-50 bg-slate-950/50 backdrop-blur-xs flex justify-end"
          onClick={() => setOrphanedDrawerOpen(false)}
        >
          <div
            className="w-full max-w-lg h-full bg-white dark:bg-slate-900 border-l border-slate-200 dark:border-slate-800 p-5 overflow-y-auto shadow-2xl space-y-4"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between border-b border-slate-200 dark:border-slate-800 pb-3">
              <div>
                <h3 className="text-sm font-bold text-slate-900 dark:text-white inline-flex items-center gap-1.5">
                  <AlertTriangle className="w-4 h-4 text-amber-500" />
                  <span>Khôi phục vị trí Highlight (Bài học cập nhật)</span>
                </h3>
                <p className="text-xs text-slate-500 mt-0.5">
                  Phiên bản v{lesson?.currentVersion} của bài học đã thay đổi nội dung. Có{" "}
                  {orphanedAnnotations.length} trích dẫn cần bạn quyết định.
                </p>
              </div>
              <button
                type="button"
                onClick={() => setOrphanedDrawerOpen(false)}
                className="px-2.5 py-1 rounded-lg bg-slate-100 dark:bg-slate-800 text-xs font-bold inline-flex items-center gap-1"
              >
                <X className="w-3.5 h-3.5" />
                <span>Đóng</span>
              </button>
            </div>

            <div className="space-y-4">
              {orphanedAnnotations.map((ann, idx) => (
                <div
                  key={ann.id}
                  data-testid={`orphaned-item-${idx}`}
                  className="rounded-xl border border-amber-300 dark:border-amber-800 bg-amber-50/40 dark:bg-amber-950/20 p-4 space-y-2.5 text-xs"
                >
                  <div className="flex items-center justify-between">
                    <span className="font-bold text-slate-800 dark:text-slate-200">
                      Highlight #{idx + 1} ({ann.semanticLabel || "Khái niệm"})
                    </span>
                    <span className="px-2 py-0.5 rounded bg-amber-500/20 text-amber-800 dark:text-amber-300 font-bold uppercase text-[10px]">
                      {ann.status}
                    </span>
                  </div>

                  <blockquote className="p-2.5 rounded-lg bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 italic text-slate-700 dark:text-slate-300">
                    &ldquo;{ann.originalExactQuote || ann.exactQuote}&rdquo;
                  </blockquote>

                  <div className="text-[11px] text-slate-500 inline-flex items-center gap-1">
                    <Zap className="w-3 h-3 text-amber-500" />
                    <span>Liên kết đang có: {ann.linkedFlashcardsCount ?? 1} thẻ ghi nhớ (bảo vệ qua ON DELETE SET NULL)</span>
                  </div>

                  {/* 3 Explicit Reconciliation Actions (Decision 14) */}
                  <div className="flex flex-wrap items-center gap-2 pt-1">
                    <button
                      type="button"
                      data-testid={`reconcile-reanchor-${idx}`}
                      onClick={() => handleReconcileOrphan(ann.id, "reanchor")}
                      className="px-2.5 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white font-bold cursor-pointer inline-flex items-center gap-1.5"
                    >
                      <Target className="w-3.5 h-3.5" />
                      <span>Gắn lại vào bài (Re-anchor)</span>
                    </button>
                    <button
                      type="button"
                      data-testid={`reconcile-keep-${idx}`}
                      onClick={() => handleReconcileOrphan(ann.id, "keep_unanchored")}
                      className="px-2.5 py-1.5 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 font-bold cursor-pointer inline-flex items-center gap-1.5"
                    >
                      <Pin className="w-3.5 h-3.5" />
                      <span>Giữ dạng trích dẫn tự do</span>
                    </button>
                    <button
                      type="button"
                      data-testid={`reconcile-delete-${idx}`}
                      onClick={() => handleReconcileOrphan(ann.id, "delete")}
                      className="px-2.5 py-1.5 rounded-lg bg-rose-600 hover:bg-rose-700 text-white font-bold cursor-pointer inline-flex items-center gap-1.5"
                    >
                      <Trash2 className="w-3.5 h-3.5" />
                      <span>Xóa highlight</span>
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
      {/* Mobile / Tablet Responsive Course Roadmap Drawer (<1024px Reading, <1280px Split) */}
      {mobileRoadmapOpen && activeCourse && roadmapChapters.length > 0 && (
        <div
          data-testid="mobile-roadmap-drawer"
          className="fixed inset-0 z-50 bg-black/50 backdrop-blur-xs flex justify-start"
          onClick={() => setMobileRoadmapOpen(false)}
        >
          <div
            className="w-full max-w-sm h-full bg-white dark:bg-slate-900 border-r border-slate-200 dark:border-slate-800 p-4 overflow-y-auto shadow-2xl space-y-4"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between pb-3 border-b border-slate-200 dark:border-slate-800">
              <div className="flex items-center gap-2 min-w-0">
                <Layers className="w-4 h-4 text-indigo-600 shrink-0" />
                <span className="text-sm font-extrabold text-slate-900 dark:text-white truncate">
                  {activeCourse.title}
                </span>
              </div>
              <button
                type="button"
                onClick={() => setMobileRoadmapOpen(false)}
                className="p-1.5 rounded-lg bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 cursor-pointer"
                aria-label="Đóng lộ trình khóa học"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            <CourseRoadmapSidebar
              courseTitle={activeCourse.title}
              chapters={roadmapChapters}
              flatLessons={activeCourseLessons}
              activeLessonId={activeLessonId}
              completedLessonIds={completedLessonIds}
              localLessonTitles={localLessonTitles}
              percentCompleted={coursePercentCompleted}
              completedCount={completedCourseLessons}
              totalCount={totalCourseLessons}
              onSelectLesson={(lessonId) => {
                if (activeCourseCid) setActiveCourseId?.(activeCourseCid);
                setActiveLesson(lessonId);
                setMobileRoadmapOpen(false);
              }}
            />
          </div>
        </div>
      )}

      {/* Sibling Markdown Reference File Preview Modal */}
      {referenceModal && (
        <div
          data-testid="reference-markdown-modal"
          className="fixed inset-0 z-50 bg-black/55 backdrop-blur-xs flex items-center justify-center p-4"
          onClick={() => setReferenceModal(null)}
        >
          <div
            className="w-full max-w-3xl max-h-[85vh] bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xl flex flex-col overflow-hidden"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="px-5 py-3.5 border-b border-slate-200 dark:border-slate-800 flex items-center justify-between gap-3">
              <div className="min-w-0">
                <div className="text-[11px] font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400">
                  Tài liệu tham khảo đính kèm
                </div>
                <h3 className="text-sm font-extrabold text-slate-900 dark:text-white truncate">
                  {referenceModal.title}
                </h3>
                <div className="text-[11px] text-slate-500 truncate">
                  {referenceModal.sourceFolder}/{referenceModal.relativePath}
                </div>
              </div>
              <button
                type="button"
                onClick={() => setReferenceModal(null)}
                className="px-3 py-1.5 rounded-lg bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-xs font-bold inline-flex items-center gap-1 cursor-pointer shrink-0"
              >
                <X className="w-3.5 h-3.5" />
                <span>Đóng</span>
              </button>
            </div>
            <div className="p-5 sm:p-6 overflow-y-auto">
              <LessonMarkdownView
                markdown={referenceModal.markdown}
                lessonTitle={referenceModal.title}
                sourceFolder={referenceModal.sourceFolder}
                sourceRelativePath={referenceModal.relativePath}
                cleanReadingMode={cleanReadingMode}
                anchoredAnnotations={[]}
                onSelectBlockText={() => {}}
                onHighlightClick={() => {}}
                onInsertCitation={() => {}}
                onOpenRelativeMarkdown={handleOpenRelativeMarkdown}
              />
            </div>
          </div>
        </div>
      )}

      {/* Course 100% Completion Celebration Modal */}
      <CourseCelebrationModal
        isOpen={celebrationModalOpen}
        onClose={() => setCelebrationModalOpen(false)}
        totalLessons={totalCourseLessons || 1}
        streakDays={userStreakDays}
        onGoToReview={() => {
          setCelebrationModalOpen(false);
          if (activeCourseCid) setActiveCourseId?.(activeCourseCid);
          if (activeCourse?.tagId) setSelectedTagFilterId(activeCourse.tagId);
          setActiveReviewSubTab("fsrs");
          setActiveMode("review");
        }}
        onGoToDashboard={() => {
          setCelebrationModalOpen(false);
          setActiveMode("dashboard");
        }}
      />
    </div>
  );
}
