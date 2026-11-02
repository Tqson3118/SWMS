"use client";

import React, { useEffect, useRef, useState } from "react";
import * as pdfjsLib from "pdfjs-dist";
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
  X
} from "lucide-react";
import {
  SEMANTIC_HIGHLIGHT_KEYS,
  useWorkspaceStore,
  type SemanticHighlightConfig
} from "@/stores/useWorkspaceStore";
import { useSynchronizedScroll } from "@/hooks/useSynchronizedScroll";
import { useNotebookAutoSave } from "@/hooks/useNotebookAutoSave";
import { scrollToAndPulseHighlight } from "@/lib/animations";

// Configure PDF.js offline worker without external CDN
if (typeof window !== "undefined") {
  pdfjsLib.GlobalWorkerOptions.workerSrc = "/pdf.worker.min.mjs";
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
  updatedAt?: string;
}

interface ReaderAndSplitViewsProps {
  mode: "reading" | "split";
  onDataMutated: () => void;
  documents?: DocumentSummary[];
  lessons?: LessonSummary[];
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
  lessons
}: ReaderAndSplitViewsProps) {
  const {
    activeLessonId,
    activeDocumentId,
    sourceType,
    currentDocumentPage,
    setCurrentDocumentPage,
    setActiveMode,
    setActiveLesson,
    setActiveDocument,
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
    orphanedDrawerOpen,
    setOrphanedDrawerOpen,
    dismissedOrphanBanner,
    setDismissedOrphanBanner,
    notebookDraftTextBySource,
    setNotebookDraftText
  } = useWorkspaceStore();

  const effectiveLessons = (lessons && lessons.length > 0)
    ? lessons
    : [
        { id: "44444444-4444-4444-8444-444444444444", title: "3.2 Bộ nhớ ảo & TLB", slug: "3-2-bo-nho-ao-tlb", currentVersion: 1 },
        { id: "55555555-5555-5555-8555-555555555555", title: "3.3 Thay thế trang LRU", slug: "3-3-thay-the-trang-lru", currentVersion: 1 }
      ];

  const docList = [...(documents || [])];
  if (
    activeDocumentId === "66666666-6666-6666-8666-666666666666" &&
    !docList.some((d) => d.id === "66666666-6666-6666-8666-666666666666")
  ) {
    docList.push({
      id: "66666666-6666-6666-8666-666666666666",
      title: "OSTEP Paging.pdf",
      totalPages: 18,
      fileSizeBytes: 1240000,
      ocrStatus: "completed"
    });
  }

  const activeDoc = docList.find((d) => d.id === activeDocumentId);

  const [lesson, setLesson] = useState<LessonDetail | null>(null);
  const [notebook, setNotebook] = useState<NotebookDetail | null>(null);
  const [selectedText, setSelectedText] = useState<string>("");
  const [selectedBlockId, setSelectedBlockId] = useState<string>("");
  const [selectedColorKey, setSelectedColorKey] = useState<SemanticHighlightConfig["key"]>("yellow");

  // PDF Viewer & Dual-Layer Reader states
  const [currentPageData, setCurrentPageData] = useState<DocumentPageDto | null>(null);
  const [pageLoading, setPageLoading] = useState(false);
  const [pdfDoc, setPdfDoc] = useState<pdfjsLib.PDFDocumentProxy | null>(null);
  const [pdfTotalPages, setPdfTotalPages] = useState<number>(1);
  const [jumpPageInput, setJumpPageInput] = useState<string>("1");
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const currentRenderTaskRef = useRef<pdfjsLib.RenderTask | null>(null);
  const pageCacheRef = useRef<Map<number, DocumentPageDto>>(new Map());

  // Draft Flashcard Confirmation Popover state (Section 5.9 & Decision 5)
  const [draftPopoverOpen, setDraftPopoverOpen] = useState(false);
  const [draftMode, setDraftMode] = useState<"cloze" | "ai">("cloze");
  const [draftFront, setDraftFront] = useState("");
  const [draftBack, setDraftBack] = useState("");
  const [draftTags, setDraftTags] = useState<string[]>(["OS-Paging", "TLB"]);
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

    const res = await fetch(endpoint);
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
        headers: { "Content-Type": "application/json" },
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
    loadLessonAndNotebook();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeLessonId, activeDocumentId, sourceType]);

  // Load PDF Document Proxy when activeDocumentId changes in document mode
  useEffect(() => {
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    if (!isDocMode || !activeDocumentId) {
      setPdfDoc(null);
      return;
    }

    let isMounted = true;
    pageCacheRef.current.clear();

    const loadPdf = async () => {
      try {
        const loadingTask = pdfjsLib.getDocument({
          url: `/api/v1/documents/${activeDocumentId}/file`,
          cMapPacked: true
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
  }, [pdfDoc, currentDocumentPage, activeDocumentId, sourceType, mode]);

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
        const res = await fetch(`/api/v1/documents/${activeDocumentId}/pages/${pageNum}`);
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
    const isDocMode = sourceType === "document" && Boolean(activeDocumentId);
    if (!isDocMode) return;

    const totalPages = pdfTotalPages || lesson?.totalPages || 1;

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
        setCurrentDocumentPage(Math.max(1, currentDocumentPage - 1));
      } else if (e.key === "ArrowRight" || e.key === "PageDown") {
        e.preventDefault();
        setCurrentDocumentPage(Math.min(totalPages, currentDocumentPage + 1));
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [activeDocumentId, currentDocumentPage, pdfTotalPages, lesson?.totalPages, setCurrentDocumentPage, sourceType]);

  const handleJumpPage = (e: React.FormEvent | React.KeyboardEvent) => {
    if ("key" in e && e.key !== "Enter") return;
    e.preventDefault();
    const pageNum = parseInt(jumpPageInput, 10);
    const totalPages = pdfTotalPages || lesson?.totalPages || 1;
    if (!isNaN(pageNum)) {
      const clamped = Math.max(1, Math.min(totalPages, pageNum));
      setCurrentDocumentPage(clamped);
      setJumpPageInput(String(clamped));
    }
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
    } else if (!selectedText) {
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
    const fullText = isDocMode ? (currentPageData?.plainText || "") : (lesson.plainText || "");
    const startIdx = fullText.indexOf(quoteToSave);
    const startOffset = startIdx >= 0 ? startIdx : 0;
    const endOffset = startOffset + quoteToSave.length;
    const prefixQuote = fullText.slice(Math.max(0, startOffset - 32), startOffset);
    const suffixQuote = fullText.slice(endOffset, Math.min(fullText.length, endOffset + 32));

    const firstParagraphBlock = lesson.contentAst?.content?.find((b) => b.type === "paragraph");
    const defaultBlockId =
      firstParagraphBlock?.attrs?.block_id || "aaaaaaa4-4444-4444-8444-444444444444";
    const blockIdToUse = isDocMode ? null : (selectedBlockId || defaultBlockId);

    setSelectedColorKey(colorKey);
    const cfg = SEMANTIC_HIGHLIGHT_KEYS.find((c) => c.key === colorKey)!;

    const resp = await fetch("/api/v1/annotations", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
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
    const excerpt = (excerptOverride ?? selectedText).trim();
    if (!excerpt) {
      showToast("Vui lòng bôi đen một đoạn văn bản trong bài đọc trước khi đánh dấu hoặc tạo thẻ!");
      return;
    }
    setSelectedText(excerpt);
    setDraftMode("cloze");
    // Instant Cloze generation in 0ms (0 AI credits)
    const words = excerpt.split(" ");
    const targetPhrase = words.slice(0, Math.min(4, words.length)).join(" ");
    const rest = words.slice(Math.min(4, words.length)).join(" ");
    setDraftFront(
      `Trong cơ chế bộ nhớ ảo: {{c1::${targetPhrase}}} ${rest}`
    );
    setDraftBack(
      `Trích dẫn gốc từ ${lesson?.title ?? "Bài 3.2"}: "${excerpt}"`
    );
    setDraftPopoverOpen(true);
  };

  const handleSwitchDraftToAiMode = async () => {
    setDraftMode("ai");
    setAiDraftLoading(true);
    try {
      const resp = await fetch("/api/v1/ai/generate-flashcard", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          excerpt: selectedText || "Translation Lookaside Buffer (TLB)",
          contextTitle: lesson?.title,
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
    const targetAnn =
      anchoredAnnotations.find((a) => a.exactQuote === (selectedText || draftFront)) ??
      anchoredAnnotations[0] ??
      null;
    const resp = await fetch("/api/v1/flashcards", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        lessonId: isDocMode ? null : lesson?.id,
        documentId: isDocMode ? (activeDocumentId || lesson?.id) : null,
        annotationId: targetAnn?.id ?? null,
        cardType: draftMode === "cloze" ? "cloze" : "ai_generated",
        sourceTitleSnapshot: isDocMode
          ? (activeDoc?.title || lesson?.title || "Tài liệu PDF")
          : (lesson?.title || "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp"),
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
    const excerpt =
      selectedText.trim() ||
      "Bảng trang đa cấp 4 tầng (PGD -> PUD -> PMD -> PTE) và TLB Hit Ratio";
    setAiExplainOpen(true);
    setAiExplainLoading(true);
    try {
      const resp = await fetch("/api/v1/ai/explain", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          excerpt,
          contextTitle: lesson?.title,
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
    if (!notebook || !lesson) return;
    const updatedContent: AstNode[] = [
      ...(notebook.contentAst?.content || []),
      {
        type: "annotationRef",
        attrs: {
          block_id: crypto.randomUUID(),
          ref_annotation_id: ann.id,
          ref_lesson_id: lesson.id,
          ref_block_id: ann.blockId,
          color_key: ann.colorKey,
          source_title: lesson.title,
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

  const handleReconcileOrphan = async (
    annId: string,
    action: "reanchor" | "keep_unanchored" | "delete"
  ) => {
    const resp = await fetch(`/api/v1/annotations/${annId}/reconcile`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
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
    // Deliberately send stale baseVersion = 0 to trigger 409 Conflict "Keep Both" Fork (ADR-022)
    await saveNotebookWithSingleWriterLock({
      id: notebook.id,
      baseVersion: 0,
      title: notebook.title,
      contentAst: notebook.contentAst
    });
    onDataMutated();
  };

  const handleExportMarkdown = async () => {
    if (!notebook) return;
    const resp = await fetch(`/api/v1/notebooks/${notebook.id}/export-markdown`);
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
      showToast(`Đã lưu tự động (OCC v${res.version}) qua Web Locks API!`);
    }
  };

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
          className={`${colorCfg.cssClass} px-1 py-0.5 rounded-xs font-medium cursor-pointer transition`}
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
      (a) => a.documentId === activeDocumentId && (a.pageNumber ?? 1) === currentDocumentPage
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
          data-page={item.ann.pageNumber ?? currentDocumentPage}
          className={`${colorCfg.cssClass} px-1 py-0.5 rounded-xs font-medium cursor-pointer transition`}
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

    return (
      <div className="space-y-5">
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
        className="sticky top-2 z-30 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white/95 dark:bg-slate-900/95 backdrop-blur-md p-2.5 shadow-lg flex flex-wrap items-center justify-between gap-2"
      >
        <div className="flex flex-wrap items-center gap-1.5">
          {SEMANTIC_HIGHLIGHT_KEYS.map((cfg) => (
            <button
              key={cfg.key}
              type="button"
              data-testid={`highlight-btn-${cfg.key}`}
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
        </div>

        <div className="flex items-center gap-1.5">
          <button
            type="button"
            data-testid="to-flashcard-popover-trigger"
            onClick={() => handleOpenDraftFlashcardPopover()}
            className="px-2.5 py-1 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer transition inline-flex items-center gap-1.5"
          >
            <Zap className="w-3.5 h-3.5" />
            <span>Tạo thẻ ghi nhớ</span>
          </button>
          <button
            type="button"
            data-testid="ai-explain-trigger"
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
              (*) Cloze tức thì (Mặc định • 0 AI Credit)
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
              <span>Trợ lý học thuật AI (gemini-3.8-flash In-Situ Ghostreader)</span>
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

      {/* Ergonomic 68ch Prose Reading Surface */}
      <article
        data-testid="reading-prose-column"
        style={{ fontSize: `${fontSizePx}px`, lineHeight: 1.8 }}
        className={`mx-auto max-w-[68ch] bg-white dark:bg-slate-900 rounded-2xl border border-slate-200/80 dark:border-slate-800 p-6 sm:p-8 shadow-xs ${
          serifFont ? "font-serif" : "font-sans"
        }`}
      >
        <div className="flex items-center justify-between gap-2 pb-4 mb-6 border-b border-slate-100 dark:border-slate-800 font-sans">
          <div>
            <span className="text-[11px] font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400">
              {isDocMode
                ? `Tài liệu PDF • Trang ${currentDocumentPage} / ${totalPages}`
                : `Chương 3: Quản lý Bộ nhớ • Phiên bản v${lesson?.currentVersion ?? 2}`}
            </span>
            <h1 className="text-xl sm:text-2xl font-extrabold text-slate-900 dark:text-white mt-0.5">
              {isDocMode
                ? (activeDoc?.title || lesson?.title || "Tài liệu PDF")
                : (lesson?.title ?? "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp")}
            </h1>
          </div>
        </div>

        {isDocMode ? (
          <div className="space-y-6 font-sans">
            {/* Page Navigation & Controls Bar */}
            <div
              data-testid="pdf-page-navigation-bar"
              className="flex flex-wrap items-center justify-between gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-950/70 border border-slate-200 dark:border-slate-800"
            >
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  data-testid="pdf-prev-page-btn"
                  disabled={currentDocumentPage <= 1}
                  onClick={() => setCurrentDocumentPage(Math.max(1, currentDocumentPage - 1))}
                  className="p-1.5 rounded-lg border border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 disabled:opacity-40 disabled:cursor-not-allowed transition cursor-pointer"
                  title="Trang trước (← / PageUp)"
                >
                  <ChevronLeft className="w-4 h-4" />
                </button>
                <div className="flex items-center gap-1.5 text-xs">
                  <span data-testid="pdf-page-indicator" className="font-bold text-slate-900 dark:text-slate-100">
                    Trang {currentDocumentPage} / {totalPages}
                  </span>
                </div>
                <button
                  type="button"
                  data-testid="pdf-next-page-btn"
                  disabled={currentDocumentPage >= totalPages}
                  onClick={() => setCurrentDocumentPage(Math.min(totalPages, currentDocumentPage + 1))}
                  className="p-1.5 rounded-lg border border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 disabled:opacity-40 disabled:cursor-not-allowed transition cursor-pointer"
                  title="Trang sau (→ / PageDown)"
                >
                  <ChevronRight className="w-4 h-4" />
                </button>
              </div>

              <div className="flex items-center gap-2">
                <span className="text-[11px] text-slate-500 font-medium">Nhảy trang:</span>
                <input
                  type="number"
                  min={1}
                  max={totalPages}
                  value={jumpPageInput}
                  onChange={(e) => setJumpPageInput(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") handleJumpPage(e);
                  }}
                  data-testid="pdf-jump-page-input"
                  className="w-16 px-2 py-1 text-center font-bold text-xs rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 focus:ring-2 focus:ring-indigo-500 outline-none"
                  aria-label="Nhập số trang"
                />
                <button
                  type="button"
                  onClick={handleJumpPage}
                  className="px-2.5 py-1 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold transition cursor-pointer"
                >
                  Đi
                </button>
              </div>
            </div>

            {/* Visual Canvas Layer */}
            <div className="pdf-canvas-container flex flex-col items-center justify-center bg-slate-100 dark:bg-slate-950 p-3 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-inner overflow-auto">
              <div className="text-[11px] font-semibold text-slate-500 dark:text-slate-400 mb-2 w-full flex items-center justify-between">
                <span className="flex items-center gap-1.5">
                  <FileText className="w-3.5 h-3.5 text-indigo-500" />
                  <span>Bản in trực quan PDF (Trang {currentDocumentPage})</span>
                </span>
                {pdfDoc && (
                  <span className="text-[10px] text-slate-400">
                    Khổ trang tự nhiên • Kết xuất qua PDF.js
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
            <div className="space-y-3 font-sans">
              <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-2">
                <span className="text-xs font-bold text-slate-700 dark:text-slate-300 uppercase tracking-wider flex items-center gap-1.5">
                  <BookOpen className="w-3.5 h-3.5 text-indigo-600" />
                  <span>Lớp văn bản trích xuất (Trang {currentDocumentPage})</span>
                </span>
                {currentPageData && (
                  <span className="text-[11px] text-slate-400">
                    {currentPageData.characterCount ?? currentPageData.charCount ?? currentPageData.plainText?.length ?? 0} ký tự • {currentPageData.wordCount ?? 0} từ
                  </span>
                )}
              </div>

              {pageLoading ? (
                <div className="py-6 text-center text-xs text-indigo-600 dark:text-indigo-400 animate-pulse font-semibold">
                  Đang nạp lớp văn bản trang {currentDocumentPage}...
                </div>
              ) : currentPageData?.hasTextLayer === false || !currentPageData?.plainText?.trim() ? (
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
                  {renderPdfTextWithHighlights(currentPageData.plainText)}
                </div>
              )}
            </div>
          </div>
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
      </article>
    </div>
    );
  };

  return (
    <div className="relative w-full overflow-x-hidden">
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
                sidebarCollapsed ? "w-12" : "w-64 xl:w-72"
              } rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-3.5 sticky top-20`}
            >
              <div className="flex items-center justify-between mb-3">
                {!sidebarCollapsed && (
                  <span className="text-xs font-bold uppercase tracking-wider text-slate-500">
                    {sourceType === "document" && activeDocumentId ? "Điều hướng trang" : "Mục lục bài học"}
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
                sourceType === "document" && activeDocumentId ? (
                  <div className="space-y-1.5 text-xs">
                    <div className="max-h-[60vh] overflow-y-auto pr-1 space-y-1">
                      {generatePageJumps(
                        pdfTotalPages || lesson?.totalPages || 1,
                        currentDocumentPage
                      ).map((p) => (
                        <button
                          key={p}
                          type="button"
                          onClick={() => setCurrentDocumentPage(p)}
                          className={`w-full text-left pl-2.5 py-1.5 rounded-lg font-semibold transition cursor-pointer flex items-center justify-between ${
                            p === currentDocumentPage
                              ? "border-l-2 border-indigo-600 bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300 font-bold"
                              : "border-l-2 border-transparent text-slate-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 hover:bg-slate-50 dark:hover:bg-slate-800/50"
                          }`}
                        >
                          <span>Trang {p}</span>
                          {p === 1 && <span className="text-[10px] text-slate-400 font-normal">Đầu</span>}
                          {p === (pdfTotalPages || lesson?.totalPages || 1) && (
                            <span className="text-[10px] text-slate-400 font-normal">Cuối</span>
                          )}
                        </button>
                      ))}
                    </div>

                    <div className="pt-4 mt-4 border-t border-slate-100 dark:border-slate-800">
                      <div className="flex justify-between text-[11px] font-semibold text-slate-500 mb-1">
                        <span>Tiến độ đọc</span>
                        <span>
                          {Math.round(
                            (currentDocumentPage / (pdfTotalPages || lesson?.totalPages || 1)) * 100
                          )}
                          %
                        </span>
                      </div>
                      <div className="w-full h-1.5 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
                        <div
                          style={{
                            width: `${Math.round(
                              (currentDocumentPage / (pdfTotalPages || lesson?.totalPages || 1)) * 100
                            )}%`
                          }}
                          className="h-full bg-indigo-600 rounded-full transition-all duration-300"
                        />
                      </div>
                    </div>
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

          {/* Center 68ch Reading Column */}
          <div className="flex-1 min-w-0">{renderReaderColumn()}</div>

          {/* Right Ergonomic Floating Action Rail */}
          <aside
            data-testid="reading-action-rail"
            className="hidden md:flex flex-col gap-2 shrink-0 w-12 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-1.5 sticky top-20 items-center shadow-xs"
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
              onClick={() => setActiveMode("split")}
              className="w-9 h-9 rounded-xl bg-indigo-50 dark:bg-indigo-950 text-indigo-600 dark:text-indigo-400 text-xs font-bold cursor-pointer flex items-center justify-center"
              title="Chuyển sang Split Workspace (Mode 2)"
            >
              <Columns2 className="w-4 h-4" />
            </button>
          </aside>
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
          {/* Pane 1: Curriculum & Resource Tree (15% on 2xl, 48px rail on xl, collapsible drawer on lg) */}
          <aside
            data-testid="split-pane-sidebar"
            style={{
              flexBasis: sidebarCollapsed ? "48px" : `${splitRatios[0]}%`
            }}
            className={`hidden xl:flex flex-col shrink-0 border-r border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 overflow-y-auto ${
              sidebarCollapsed ? "w-12 max-w-[48px]" : "xl:max-w-[48px] 2xl:max-w-[25%]"
            }`}
          >
            {/* On 1536px (xl) defaults to compact 48px icon rail; on >=1920px (2xl) full 15% tree unless collapsed */}
            <div className="p-2.5 flex items-center justify-between border-b border-slate-100 dark:border-slate-800">
              <span
                className={`text-xs font-bold uppercase text-slate-500 truncate ${
                  sidebarCollapsed ? "hidden" : "hidden 2xl:inline"
                }`}
              >
                Cây học liệu ({splitRatios[0]}%)
              </span>
              <button
                type="button"
                data-testid="split-sidebar-toggle"
                onClick={toggleSidebar}
                className="p-1 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-xs font-bold mx-auto 2xl:mx-0 cursor-pointer flex items-center justify-center"
                title="Thu gọn / Mở rộng Sidebar (Ctrl+\)"
              >
                <BookOpen className="w-4 h-4 text-slate-600 dark:text-slate-300" />
              </button>
            </div>

            {!sidebarCollapsed && (
              <div className="hidden 2xl:block p-3 space-y-4 text-xs">
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

          {/* Resize Handle 1 (4px width, interactive drag 10-25% / 30-60%, double-click resets to 15/45/40) */}
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
            className="hidden 2xl:flex w-1 hover:w-2 bg-slate-200/80 dark:bg-slate-800 hover:bg-indigo-500 transition-colors cursor-col-resize items-center justify-center shrink-0"
          >
            <span className="w-0.5 h-6 rounded-full bg-slate-400/70 pointer-events-none" />
          </div>

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
            className="w-1 hover:w-2 bg-slate-200/80 dark:bg-slate-800 hover:bg-indigo-500 transition-colors cursor-col-resize flex items-center justify-center shrink-0"
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
                      OCC v{notebook?.version ?? 1} • Web Locks Active
                    </span>
                    <button
                      type="button"
                      data-testid="toggle-sync-scroll-btn"
                      onClick={toggleSyncScroll}
                      className={`px-2 py-0.5 rounded text-[10px] font-bold cursor-pointer inline-flex items-center gap-1 ${
                        syncScrollEnabled
                          ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                          : "bg-slate-100 dark:bg-slate-800 text-slate-500"
                      }`}
                    >
                      <Link2 className="w-3 h-3" />
                      <span>Đồng bộ cuộn (Mutex 50ms): {syncScrollEnabled ? "Bật" : "Tắt"}</span>
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
                    className="px-2.5 py-1 rounded-lg border border-amber-500/40 bg-amber-500/10 hover:bg-amber-500/20 text-amber-800 dark:text-amber-300 text-[11px] font-bold cursor-pointer inline-flex items-center gap-1"
                    title="Kiểm thử xung đột 409 Keep-Both (ADR-022)"
                  >
                    <Zap className="w-3 h-3 text-amber-600" />
                    <span>Test 409 Keep-Both</span>
                  </button>
                  <button
                    type="button"
                    data-testid="export-markdown-btn"
                    onClick={handleExportMarkdown}
                    className="px-2.5 py-1 rounded-lg border border-slate-300 dark:border-slate-700 hover:bg-slate-100 dark:hover:bg-slate-800 text-[11px] font-bold cursor-pointer inline-flex items-center gap-1"
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
                              onClick={() =>
                                scrollToAndPulseHighlight(`reader-highlight-${refAnnId}`)
                              }
                              className="px-2 py-0.5 rounded bg-indigo-600 text-white font-bold hover:bg-indigo-700 transition cursor-pointer shrink-0 inline-flex items-center gap-1"
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
                <div className="flex items-center justify-between text-xs">
                  <span className="font-bold text-slate-600 dark:text-slate-400 inline-flex items-center gap-1.5">
                    <PenTool className="w-3.5 h-3.5 text-indigo-600 shrink-0" />
                    <span>Soạn thảo ghi chú tổng hợp (Hỗ trợ [[Wikilinks]] & /flashcard):</span>
                  </span>
                  <span className="text-[11px] text-emerald-600 dark:text-emerald-400 font-semibold">
                    Đã đồng bộ IndexedDB Draft
                  </span>
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
                    Lưu vào JSONB AST (OCC)
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
    </div>
  );
}
