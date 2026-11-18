"use client";

import React, { useCallback, useState, useEffect, useRef } from "react";
import { getClientRequestHeaders, useWorkspaceStore } from "@/stores/useWorkspaceStore";
import {
  Sparkles,
  Brain,
  Clock,
  BookOpen,
  FileText,
  PenTool,
  Target,
  ShieldCheck,
  CheckCircle2,
  ArrowRight,
  Zap,
  Trophy,
  Tag,
  Upload,
  FileUp,
  AlertCircle,
  Trash2,
  X,
  Flame,
  Layers,
  ChevronRight,
  RefreshCw
} from "lucide-react";
import { CourseCreateModal } from "./CourseCreateModal";
import {
  CourseCatalogGrid,
  resolveNextCourseLesson,
  resolveCourseDomain,
  type CourseChapterSummaryDto,
  type CourseLessonSummaryDto,
  type CourseProgressInfo,
  type CourseSummaryDto
} from "./CourseCatalogGrid";

export type {
  CourseLessonSummaryDto,
  CourseChapterSummaryDto,
  CourseSummaryDto,
  CourseProgressInfo
};

export interface PracticeQuizSummaryDto {
  id: string;
  title: string;
  documentId?: string | null;
  lessonId?: string | null;
  totalItems: number;
  correctItems: number;
  status: string;
  startedAt?: string;
}

export interface DashboardData {
  user: {
    id: string;
    fullName: string;
    email: string;
    streakDays: number;
    onboardingStep: number;
    isActivated: boolean;
  };
  reviewStation: {
    totalDue: number;
    totalCards: number;
    newCount: number;
    learningCount: number;
    reviewCount: number;
    estimatedMinutes: number;
  };
  lessons: Array<{
    id: string;
    title: string;
    slug: string;
    currentVersion: number;
    courseId?: string | null;
    chapterId?: string | null;
    sourceDocumentId?: string | null;
    startPage?: number | null;
    endPage?: number | null;
    updatedAt: string;
  }>;
  documents: Array<{
    id: string;
    title: string;
    totalPages: number;
    fileSizeBytes: number;
    ocrStatus: string;
    updatedAt: string;
  }>;
  notebooks: Array<{
    id: string;
    title: string;
    version: number;
    primarySourceLessonId: string | null;
    primarySourceDocumentId: string | null;
    updatedAt: string;
  }>;
  tags: Array<{
    id: string;
    name: string;
    colorKey: string;
    cardCount: number;
  }>;
  courses?: CourseSummaryDto[];
  quizzes?: PracticeQuizSummaryDto[];
}

interface PersonalWorkspaceDashboardProps {
  data: DashboardData;
  onRefreshDashboard: () => Promise<void> | void;
  courses?: CourseSummaryDto[];
  quizzes?: PracticeQuizSummaryDto[];
  isLoadingDashboard?: boolean;
  dashboardError?: string | null;
}

export function PersonalWorkspaceDashboard({
  data,
  onRefreshDashboard,
  courses: propCourses,
  isLoadingDashboard = false,
  dashboardError = null
}: PersonalWorkspaceDashboardProps) {
  const {
    activeCourseId,
    activeLessonId,
    activeDocumentId,
    sourceType,
    setActiveMode,
    setActiveReviewSubTab,
    setActiveLesson,
    setActiveDocument,
    setSelectedTagFilterId,
    setActiveCourseId,
    setActiveQuizId,
    setEditingCourseId
  } = useWorkspaceStore();

  const [uploadModalOpen, setUploadModalOpen] = useState(false);
  const [courseCreateModalOpen, setCourseCreateModalOpen] = useState(false);
  const [docTitle, setDocTitle] = useState("");
  const [docText, setDocText] = useState("");
  const [uploading, setUploading] = useState(false);
  const [uploadStep, setUploadStep] = useState<"idle" | "uploading" | "extracting" | "completed">("idle");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [fileSizeBytes, setFileSizeBytes] = useState(2450000);
  const [totalPages, setTotalPages] = useState(1);
  const [fileMimeType, setFileMimeType] = useState("application/pdf");
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [libraryFilter, setLibraryFilter] = useState<"all" | "course" | "pdf" | "lesson" | "notebook">("all");
  const [creatingNotebook, setCreatingNotebook] = useState(false);
  const [generatingDocId, setGeneratingDocId] = useState<string | null>(null);
  const hasPreloadedCourses = Boolean(
    (propCourses && propCourses.length > 0) ||
      (data.courses && data.courses.length > 0)
  );
  const [fetchedCourses, setFetchedCourses] = useState<CourseSummaryDto[]>([]);
  const [loadingCourses, setLoadingCourses] = useState<boolean>(
    !hasPreloadedCourses
  );
  const [coursesError, setCoursesError] = useState<string | null>(null);
  const [showAllLessons, setShowAllLessons] = useState(false);
  const [showAllDocs, setShowAllDocs] = useState(false);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const getAuthHeaders = useCallback((): Record<string, string> => {
    return getClientRequestHeaders();
  }, []);

  const fetchCourses = useCallback(async () => {
    setLoadingCourses(true);
    setCoursesError(null);
    try {
      const resp = await fetch("/api/v1/courses", { headers: getAuthHeaders() });
      if (!resp.ok) {
        const errBody = await resp.json().catch(() => null);
        setCoursesError(
          errBody?.error || `Không thể tải danh sách khóa học (Mã lỗi HTTP ${resp.status}).`
        );
        return;
      }
      const list = await resp.json();
      if (Array.isArray(list)) {
        setFetchedCourses(list);
      } else {
        setCoursesError("Dữ liệu khóa học trả về từ máy chủ không đúng định dạng.");
      }
    } catch {
      setCoursesError("Không thể kết nối máy chủ để tải danh sách khóa học.");
    } finally {
      setLoadingCourses(false);
    }
  }, [getAuthHeaders]);

  useEffect(() => {
    setFetchedCourses([]);
  }, [data.user.id]);

  useEffect(() => {
    if (isLoadingDashboard || hasPreloadedCourses) {
      if (hasPreloadedCourses) setLoadingCourses(false);
      return;
    }
    void fetchCourses();
  }, [isLoadingDashboard, hasPreloadedCourses, fetchCourses, data.documents.length]);

  const handleRefreshAll = useCallback(async () => {
    await Promise.all([Promise.resolve(onRefreshDashboard()), fetchCourses()]);
  }, [onRefreshDashboard, fetchCourses]);

  // Prefer fresh /api/v1/courses records first while preserving backend ordering
  const rawCourses: CourseSummaryDto[] = [
    ...fetchedCourses,
    ...(propCourses || []),
    ...(data.courses || [])
  ];
  const coursesMap = new Map<string, CourseSummaryDto>();
  for (const c of rawCourses) {
    const key = c.id || c.courseId || c.slug;
    if (key && !coursesMap.has(key)) {
      coursesMap.set(key, c);
    }
  }
  const courses: CourseSummaryDto[] = Array.from(coursesMap.values());
  const courseIdsKey = courses.map((c) => c.courseId || c.id || "").join(",");

  const [courseProgressMap, setCourseProgressMap] = useState<
    Record<string, CourseProgressInfo>
  >({});

  useEffect(() => {
    let isMounted = true;
    const fetchProgress = async () => {
      const entries = await Promise.all(
        courses.map(async (course) => {
          const cid = course.courseId || course.id;
          if (!cid) return null;
          try {
            const res = await fetch(`/api/v1/courses/${cid}/progress`, {
              headers: getAuthHeaders()
            });
            if (!res.ok) return null;
            const prog = await res.json();
            const info: CourseProgressInfo = {
              percentCompleted: Number(prog.percentCompleted) || 0,
              completedLessons: Number(prog.completedLessons) || 0,
              totalLessons: Number(prog.totalLessons) || (course.lessonCount ?? 0),
              completedLessonIds: Array.isArray(prog.completedLessonIds)
                ? prog.completedLessonIds
                : []
            };
            return [cid, info] as const;
          } catch {
            return null;
          }
        })
      );

      if (!isMounted) return;
      const updates: Record<string, CourseProgressInfo> = {};
      for (const entry of entries) {
        if (entry) {
          updates[entry[0]] = entry[1];
        }
      }
      if (Object.keys(updates).length > 0) {
        setCourseProgressMap((prev) => ({ ...prev, ...updates }));
      }
    };

    if (courses.length > 0) {
      void fetchProgress();
    }

    return () => {
      isMounted = false;
    };
  }, [courseIdsKey, data.lessons.length, activeCourseId, getAuthHeaders]);

  const handleGenerateCourseFromDoc = async (docId: string) => {
    if (generatingDocId) return;
    setGeneratingDocId(docId);
    try {
      const resp = await fetch(`/api/v1/documents/${docId}/generate-course`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...getAuthHeaders()
        }
      });
      if (resp.ok) {
        const generated: CourseSummaryDto = await resp.json();
        const cid = generated.courseId ?? generated.id ?? null;
        if (cid) setActiveCourseId?.(cid);
        if (generated.quizId) setActiveQuizId?.(generated.quizId);
        await handleRefreshAll();
        const firstLesId = generated.firstLessonId ?? generated.chapters?.[0]?.lessons?.[0]?.id;
        if (firstLesId) {
          setActiveLesson(firstLesId);
        } else {
          setActiveDocument(docId);
        }
        setActiveMode("reading");
      }
    } catch {
      // Fallback
    } finally {
      setGeneratingDocId(null);
    }
  };

  const handleCreateStandaloneNotebook = async () => {
    if (creatingNotebook) return;
    setCreatingNotebook(true);
    try {
      const resp = await fetch("/api/v1/notebooks", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...getAuthHeaders()
        },
        body: JSON.stringify({
          title: `Ghi chú độc lập (${new Date().toLocaleDateString("vi-VN")})`
        })
      });
      if (resp.ok) {
        onRefreshDashboard();
        setLibraryFilter("notebook");
      } else {
        setActiveMode("split");
      }
    } catch {
      setActiveMode("split");
    } finally {
      setCreatingNotebook(false);
    }
  };

  // Close modal on Escape key
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && uploadModalOpen) {
        setUploadModalOpen(false);
        setUploadError(null);
        setUploadStep("idle");
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [uploadModalOpen]);

  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  // Client-side PDF structure inspection for fast, friendly error handling
  const validatePdfFile = async (file: File): Promise<{ valid: boolean; error?: string }> => {
    // 0. Check for empty / zero-byte file
    if (file.size === 0) {
      return {
        valid: false,
        error: "Lỗi: Tệp tin rỗng (0 bytes). Vui lòng chọn một tệp có nội dung hợp lệ."
      };
    }

    // 1. Check size limit: 50MB (52,428,800 bytes)
    if (file.size > 52428800) {
      return {
        valid: false,
        error: `Dung lượng tệp (${formatFileSize(file.size)}) vượt quá hạn mức tối đa 50MB của gói Free. Vui lòng chọn tệp nhỏ hơn.`
      };
    }

    const isPdf = file.type === "application/pdf" || file.name.toLowerCase().endsWith(".pdf");
    if (!isPdf) return { valid: true };

    try {
      const buffer = await file.arrayBuffer();
      const bytes = new Uint8Array(buffer);
      if (bytes.length < 32) {
        return {
          valid: false,
          error: "Lỗi: Tệp bị hỏng hoặc không thể đọc được. Tệp PDF không hợp lệ hoặc quá ngắn."
        };
      }

      const head = new TextDecoder("latin1").decode(bytes.subarray(0, Math.min(2048, bytes.length)));
      const tail = new TextDecoder("latin1").decode(bytes.subarray(Math.max(0, bytes.length - 2048)));
      const sample = bytes.length < 5000000 ? new TextDecoder("latin1").decode(bytes) : head + "\n" + tail;

      // Check %PDF- header
      if (!head.includes("%PDF-")) {
        return {
          valid: false,
          error: "Lỗi: Tệp bị hỏng hoặc không đúng định dạng PDF hợp lệ."
        };
      }

      // Check %%EOF trailer
      if (!tail.includes("%%EOF")) {
        return {
          valid: false,
          error: "Lỗi: Tệp bị hỏng hoặc không thể đọc được. Thiếu đánh dấu kết thúc tệp (EOF) hợp lệ."
        };
      }

      // Check Catalog / Root object
      if (!sample.includes("/Catalog") && !sample.includes("/Root")) {
        return {
          valid: false,
          error: "Lỗi: Tệp bị hỏng hoặc không thể đọc được. Không tìm thấy mục lục (Catalog/Root) trong tệp."
        };
      }

      // Check /Encrypt dictionary
      if (sample.includes("/Encrypt")) {
        return {
          valid: false,
          error: "Lỗi: Tệp được bảo vệ bằng mật khẩu hoặc đã bị mã hóa (encrypted). Vui lòng mở khóa trước khi tải lên."
        };
      }

      return { valid: true };
    } catch {
      return {
        valid: false,
        error: "Lỗi: Không thể đọc dữ liệu tệp. Tệp bị hỏng hoặc không thể đọc."
      };
    }
  };

  const handleProcessFile = async (file: File) => {
    setUploadError(null);
    setSelectedFile(file);
    setFileSizeBytes(file.size);

    // Auto-fill title if empty or default
    if (!docTitle.trim() || docTitle.endsWith(".pdf") || docTitle.endsWith(".md")) {
      setDocTitle(file.name);
    }

    const validation = await validatePdfFile(file);
    if (!validation.valid && validation.error) {
      setUploadError(validation.error);
      return;
    }

    const isPdf = file.type === "application/pdf" || file.name.toLowerCase().endsWith(".pdf");
    const isMd =
      file.type === "text/markdown" ||
      file.type === "text/plain" ||
      file.name.toLowerCase().endsWith(".md") ||
      file.name.toLowerCase().endsWith(".markdown") ||
      file.name.toLowerCase().endsWith(".txt");

    if (isMd) {
      setFileMimeType("text/markdown");
      try {
        const text = await file.text();
        setDocText(text);
        const pages = Math.max(1, Math.ceil(text.length / 2500));
        setTotalPages(pages);
      } catch {
        setUploadError("Lỗi: Không thể đọc tệp văn bản. Vui lòng thử lại.");
      }
    } else if (isPdf) {
      setFileMimeType("application/pdf");
      setTotalPages(1);
    } else {
      setFileMimeType(file.type || "application/octet-stream");
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      const file = e.dataTransfer.files[0];
      handleProcessFile(file);
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handleClearSelectedFile = () => {
    setSelectedFile(null);
    setUploadError(null);
    setUploadStep("idle");
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const handleUploadDocument = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!docTitle.trim()) return;

    if (fileSizeBytes > 52428800) {
      setUploadError("Lỗi: Không thể tải lên: Dung lượng tệp vượt quá hạn mức tối đa 50MB của gói Free.");
      return;
    }

    // Pre-validate selected file before network fetch to prevent raw 422 browser console errors
    if (selectedFile) {
      const validation = await validatePdfFile(selectedFile);
      if (!validation.valid && validation.error) {
        setUploadError(validation.error);
        return;
      }
    }

    setUploading(true);
    setUploadStep("uploading");
    setUploadError(null);

    try {
      let resp: Response;
      if (selectedFile) {
        const formData = new FormData();
        formData.append("file", selectedFile);
        formData.append("title", docTitle.trim() || selectedFile.name);

        setUploadStep("extracting");
        resp = await fetch("/api/v1/documents/upload", {
          method: "POST",
          headers: getAuthHeaders(),
          body: formData
        });
      } else {
        resp = await fetch("/api/v1/documents/upload", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            ...getAuthHeaders()
          },
          body: JSON.stringify({
            title: docTitle.trim(),
            fileSizeBytes: Math.max(1024, fileSizeBytes),
            totalPages: Math.max(1, totalPages),
            mimeType: fileMimeType,
            extractedText: docText.trim()
          })
        });
      }

      if (resp.ok) {
        setUploadStep("completed");
        const created = await resp.json();
        const genCourse = created.generatedCourse;
        const cid = genCourse?.courseId ?? genCourse?.id ?? created.courseId ?? null;
        if (cid) setActiveCourseId?.(cid);
        if (genCourse?.quizId ?? created.quizId) {
          setActiveQuizId?.(genCourse?.quizId ?? created.quizId);
        }
        setUploadModalOpen(false);
        setDocTitle("");
        setDocText("");
        setSelectedFile(null);
        setUploadError(null);
        setUploadStep("idle");
        onRefreshDashboard();
        setActiveDocument(created.id, 1, created.totalPages ?? null);
        setActiveMode("reading");
      } else {
        const err = await resp.json().catch(() => ({}));
        setUploadError(
          err.error ||
            err.detail ||
            (resp.status === 413
              ? "Lỗi: Vượt quá hạn mức dung lượng lưu trữ 50 MB của gói Free."
              : resp.status === 422
              ? "Lỗi: Tệp bị hỏng hoặc không thể đọc được."
              : resp.status === 500
              ? "Lỗi: Không thể kết nối hoặc máy chủ Backend gặp sự cố (500)."
              : `Lỗi: Tải tài liệu thất bại (Mã lỗi ${resp.status}).`)
        );
      }
    } catch {
      setUploadError("Lỗi kết nối mạng khi tải tài liệu lên. Vui lòng kiểm tra lại đường truyền.");
    } finally {
      setUploading(false);
      setUploadStep("idle");
    }
  };

  // Determine active item for Hero Continue Learning Bento Block (no hardcoded percentages)
  const activeDoc =
    data.documents.find((d) => d.id === activeDocumentId) ||
    data.documents[0] ||
    null;
  const activeLesson =
    data.lessons.find((l) => l.id === activeLessonId) ||
    data.lessons[0] ||
    null;

  const heroCourse =
    courses.find((c) => (c.courseId || c.id) === activeCourseId) ||
    courses.find((c) => {
      const cid = c.courseId || c.id || "";
      const p = courseProgressMap[cid]?.percentCompleted ?? 0;
      return p > 0 && p < 100;
    }) ||
    courses[0] ||
    null;

  const heroCourseId = heroCourse ? heroCourse.courseId || heroCourse.id || "" : "";
  const heroCourseProg = heroCourseId ? courseProgressMap[heroCourseId] : undefined;
  const heroCourseLessonsTotal =
    heroCourseProg?.totalLessons ??
    heroCourse?.lessonCount ??
    heroCourse?.chapters?.reduce((acc, ch) => acc + (ch.lessons?.length ?? 0), 0) ??
    0;
  const heroCourseCompleted = heroCourseProg?.completedLessons ?? 0;
  const heroCoursePercent = Math.max(
    0,
    Math.min(100, heroCourseProg?.percentCompleted ?? 0)
  );
  const heroNextLesson = heroCourse
    ? resolveNextCourseLesson(heroCourse, heroCourseProg?.completedLessonIds)
    : { id: null, title: null, allCompleted: false };
  const heroCourseDomain = heroCourse ? resolveCourseDomain(heroCourse) : null;
  const preferHeroDoc =
    sourceType === "document" && Boolean(activeDocumentId && activeDoc);
  const usingCourseHero = Boolean(heroCourse && !preferHeroDoc);

  const activeTitle = usingCourseHero
    ? heroCourse!.title
    : activeDoc?.title || activeLesson?.title || "Chưa có bài học hoặc tài liệu";
  const activeTotalPages = activeDoc?.totalPages || 1;
  const docPercent = activeDoc
    ? Math.min(100, Math.round((1 / Math.max(1, activeTotalPages)) * 100))
    : 0;
  const heroProgressPercent = usingCourseHero ? heroCoursePercent : docPercent;

  const handleLaunchHeroTarget = (targetMode: "reading" | "split") => {
    if (usingCourseHero && heroCourse) {
      if (heroCourseId) setActiveCourseId(heroCourseId);
      const nextId = heroNextLesson.id ?? heroCourse.firstLessonId;
      if (nextId) {
        setActiveLesson(nextId);
      } else if (heroCourse.sourceDocumentId) {
        setActiveDocument(heroCourse.sourceDocumentId);
      }
      setActiveMode(targetMode);
      return;
    }

    if (activeDoc && (preferHeroDoc || !activeLesson)) {
      setActiveDocument(activeDoc.id);
    } else if (activeLesson) {
      setActiveLesson(activeLesson.id);
    }
    setActiveMode(targetMode);
  };

  return (
    <div
      data-testid="personal-workspace-dashboard"
      className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 space-y-6"
    >
      {dashboardError && (
        <div
          data-testid="dashboard-error-alert"
          role="alert"
          className="p-4 rounded-2xl bg-rose-50 dark:bg-rose-950/50 border border-rose-200 dark:border-rose-800 text-rose-800 dark:text-rose-200 text-xs sm:text-sm flex flex-wrap items-center justify-between gap-3"
        >
          <div className="flex items-center gap-2.5 font-semibold">
            <AlertCircle className="w-4 h-4 shrink-0 text-rose-600 dark:text-rose-400" />
            <span>{dashboardError}</span>
          </div>
          <button
            type="button"
            onClick={() => void handleRefreshAll()}
            className="min-h-[40px] px-3.5 py-1.5 rounded-xl bg-rose-600 hover:bg-rose-700 text-white text-xs font-bold inline-flex items-center gap-1.5 cursor-pointer transition"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            <span>Thử lại</span>
          </button>
        </div>
      )}
      {/* Zone 0: Header Greeting & Primary Action Bar */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white dark:bg-slate-900 p-5 sm:p-6 rounded-2xl border border-slate-200/80 dark:border-slate-800 shadow-sm">
        <div>
          <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 text-xs font-semibold mb-2">
            <Sparkles className="w-3.5 h-3.5" />
            <span>Bento Workspace • Tối ưu hóa việc học với FSRS Spaced Repetition</span>
          </div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-white">
            Chào mừng trở lại, {data.user.fullName}!
          </h1>
          <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-400 mt-1">
            Không gian học tập cá nhân: Đọc tài liệu, trích xuất ghi nhớ và ôn tập thông minh mỗi ngày.
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2 shrink-0">
          <button
            type="button"
            data-testid="create-course-hub-btn"
            onClick={() => setCourseCreateModalOpen(true)}
            className="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs sm:text-sm font-semibold shadow-xs transition cursor-pointer min-h-[44px]"
          >
            <Sparkles className="w-4 h-4 text-amber-300" />
            <span>Tạo Khóa học Mới</span>
          </button>
          <button
            type="button"
            data-testid="open-upload-pdf-btn"
            onClick={() => setUploadModalOpen(true)}
            className="inline-flex items-center gap-2 px-3 py-2.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-200 text-xs sm:text-sm font-semibold transition cursor-pointer min-h-[44px]"
          >
            <Upload className="w-4 h-4" />
            <span>Tải PDF lẻ</span>
          </button>
          <button
            type="button"
            data-testid="open-split-notebook-btn"
            onClick={() => setActiveMode("split")}
            className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-800/50 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-300 text-xs font-semibold transition cursor-pointer min-h-[44px]"
          >
            <PenTool className="w-3.5 h-3.5" />
            <span>Ghi chú Split</span>
          </button>
          <button
            type="button"
            data-testid="open-practice-quiz-btn"
            onClick={() => {
              setActiveReviewSubTab("quiz");
              setActiveMode("review");
            }}
            className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-800/50 hover:bg-emerald-500/10 text-slate-600 dark:text-slate-300 hover:text-emerald-700 dark:hover:text-emerald-300 text-xs font-semibold transition cursor-pointer min-h-[44px]"
          >
            <Target className="w-3.5 h-3.5 text-emerald-500" />
            <span>Luyện Quiz</span>
          </button>
        </div>
      </div>

      {/* Ultra-sleek compact step status pill bar integrated above Bento Grid */}
      <section
        data-testid="onboarding-3step-widget"
        className="rounded-xl border border-slate-200/70 dark:border-slate-800/80 bg-white/90 dark:bg-slate-900/90 px-3.5 py-2 shadow-2xs flex flex-wrap items-center justify-between gap-2"
      >
        <div className="flex flex-wrap items-center gap-2.5">
          <span
            data-testid="onboarding-progress-badge"
            className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-indigo-600 text-white shrink-0"
          >
            <Trophy className="w-3 h-3" />
            <span>Chu kỳ FSRS (3/3)</span>
          </span>
          <div className="hidden sm:flex items-center gap-3 text-xs font-medium text-slate-600 dark:text-slate-400">
            <span data-testid="onboarding-step-1" className="inline-flex items-center gap-1 text-emerald-600 dark:text-emerald-400 font-semibold">
              <CheckCircle2 className="w-3.5 h-3.5" /> 1. Đọc tài liệu
            </span>
            <span data-testid="onboarding-step-2" className="inline-flex items-center gap-1 text-emerald-600 dark:text-emerald-400 font-semibold">
              <CheckCircle2 className="w-3.5 h-3.5" /> 2. Trích xuất Cloze
            </span>
            <span data-testid="onboarding-step-3" className="inline-flex items-center gap-1 text-emerald-600 dark:text-emerald-400 font-semibold">
              <CheckCircle2 className="w-3.5 h-3.5" /> 3. Ôn tập đều đặn
            </span>
          </div>
        </div>
        <button
          type="button"
          data-testid="onboarding-start-review-btn"
          onClick={() => {
            setSelectedTagFilterId(null);
            setActiveReviewSubTab("fsrs");
            setActiveMode("review");
          }}
          className="inline-flex items-center justify-center gap-1.5 px-3 py-2 rounded-lg bg-slate-100 hover:bg-slate-200 dark:bg-slate-800 dark:hover:bg-slate-700 text-slate-800 dark:text-slate-200 text-xs font-semibold cursor-pointer transition min-h-[42px]"
        >
          <Zap className="w-3.5 h-3.5 text-indigo-500" />
          <span>Ôn tập nhanh (~1 phút)</span>
        </button>
      </section>

      {/* Top Row: Hero Continue Learning + FSRS Review Station */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-5 sm:gap-6">
        {/* Bento Block 1: Hero Continue Learning Card (lg:col-span-7) */}
        <div
          data-testid="hero-continue-learning-card"
          className="lg:col-span-7 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 p-6 flex flex-col justify-between shadow-sm relative overflow-hidden"
        >
          <div>
            <div className="flex items-center justify-between gap-2 pb-3 border-b border-slate-100 dark:border-slate-800">
              <div className="flex flex-wrap items-center gap-2">
                <span className="px-2.5 py-0.5 rounded-full bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300 text-xs font-semibold">
                  {usingCourseHero ? "Khóa học trọng tâm" : "Tài liệu đang học"}
                </span>
                <span className="text-xs text-slate-600 dark:text-slate-400">
                  {usingCourseHero && heroCourseDomain
                    ? `${heroCourseDomain.badgeLabel} • ${heroCourseLessonsTotal} bài giảng`
                    : activeDoc
                    ? `PDF • ${activeTotalPages} trang`
                    : "Bài học giáo trình"}
                </span>
              </div>
              <span className="text-xs text-slate-500 dark:text-slate-400 shrink-0">
                Tiếp tục phiên học
              </span>
            </div>

            <div className="mt-4">
              <h2 className="text-lg sm:text-xl font-bold text-slate-900 dark:text-white line-clamp-2">
                {activeTitle}
              </h2>
              <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-400 mt-1.5 line-clamp-2">
                {usingCourseHero && heroNextLesson.title
                  ? `Bài tiếp theo: ${heroNextLesson.title}`
                  : usingCourseHero && heroCourse?.description
                  ? heroCourse.description
                  : "Tiếp tục đọc với bảng mục lục tương tác, đánh dấu 5 màu và ghi chú tổng hợp PKM song song."}
              </p>
            </div>

            <div className="mt-5 space-y-2">
              <div className="flex items-center justify-between text-xs font-medium text-slate-600 dark:text-slate-400">
                <span>
                  {usingCourseHero
                    ? `Tiến độ khóa học: ${heroCourseCompleted} / ${heroCourseLessonsTotal} bài`
                    : activeDoc
                    ? `Tiến độ đọc: Trang 1 / ${activeTotalPages}`
                    : "Tiến độ học tập"}
                </span>
                <span className="font-bold text-slate-800 dark:text-slate-200">
                  {heroProgressPercent}% hoàn thành
                </span>
              </div>
              <div className="w-full bg-slate-100 dark:bg-slate-800 h-2.5 rounded-full overflow-hidden">
                <div
                  className="bg-indigo-600 h-full rounded-full transition-all duration-300"
                  style={{ width: `${heroProgressPercent}%` }}
                />
              </div>
            </div>
          </div>

          <div className="mt-6 pt-4 border-t border-slate-100 dark:border-slate-800 flex flex-wrap items-center gap-3">
            {/* The first button inside hero card is the 1-click CTA for reading mode */}
            <button
              type="button"
              onClick={() => handleLaunchHeroTarget("reading")}
              className="inline-flex items-center justify-center gap-2 px-5 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs sm:text-sm font-bold shadow-xs transition cursor-pointer min-h-[44px]"
            >
              <BookOpen className="w-4 h-4" />
              <span>
                {usingCourseHero
                  ? heroCoursePercent > 0
                    ? "Tiếp tục học ngay"
                    : "Vào học ngay"
                  : "Tiếp tục đọc ngay"}
              </span>
            </button>
            <button
              type="button"
              onClick={() => handleLaunchHeroTarget("split")}
              className="inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-200 text-xs sm:text-sm font-semibold transition cursor-pointer min-h-[44px]"
            >
              <PenTool className="w-4 h-4" />
              <span>Mở Ghi chú PKM</span>
            </button>
          </div>
        </div>

        {/* Bento Block 2: Review Station Bento Card (lg:col-span-5) */}
        <div
          data-testid="review-station-card"
          className="lg:col-span-5 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 p-6 flex flex-col justify-between shadow-sm"
        >
          <div data-testid="fsrs-review-station-card" className="flex flex-col justify-between h-full">
            <div>
              <div className="flex items-center justify-between pb-3 border-b border-slate-100 dark:border-slate-800">
                <h3 className="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
                  <Brain className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
                  <span>Trạm ôn tập Spaced Repetition</span>
                </h3>
                <span className="px-2.5 py-0.5 rounded-md bg-emerald-500/10 text-emerald-700 dark:text-emerald-300 text-xs font-semibold">
                  FSRS v5
                </span>
              </div>

              <div className="mt-5 flex items-baseline gap-3">
                <span
                  data-testid="dashboard-due-count"
                  className="text-4xl sm:text-5xl font-black text-indigo-600 dark:text-indigo-400 tracking-tight"
                >
                  {data.reviewStation.totalDue}
                </span>
                <span className="text-sm text-slate-600 dark:text-slate-400 font-medium">
                  thẻ cần ôn hôm nay • Tổng số: {data.reviewStation.totalCards} thẻ
                </span>
              </div>

              <div className="grid grid-cols-3 gap-2.5 mt-5">
                <div className="p-3 rounded-xl bg-sky-50 dark:bg-sky-950/40 border border-sky-200/60 dark:border-sky-900 text-center">
                  <div className="text-lg font-bold text-sky-700 dark:text-sky-300">
                    {data.reviewStation.newCount}
                  </div>
                  <div className="text-[11px] font-medium text-sky-600 dark:text-sky-400 mt-0.5">
                    Thẻ mới
                  </div>
                </div>
                <div className="p-3 rounded-xl bg-amber-50 dark:bg-amber-950/40 border border-amber-200/60 dark:border-amber-900 text-center">
                  <div className="text-lg font-bold text-amber-700 dark:text-amber-300">
                    {data.reviewStation.learningCount}
                  </div>
                  <div className="text-[11px] font-medium text-amber-600 dark:text-amber-400 mt-0.5">
                    Đang học
                  </div>
                </div>
                <div className="p-3 rounded-xl bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200/60 dark:border-emerald-900 text-center">
                  <div className="text-lg font-bold text-emerald-700 dark:text-emerald-300">
                    {data.reviewStation.reviewCount}
                  </div>
                  <div className="text-[11px] font-medium text-emerald-600 dark:text-emerald-400 mt-0.5">
                    Ôn định kỳ
                  </div>
                </div>
              </div>

              <p className="text-xs text-slate-500 dark:text-slate-400 mt-4 flex items-center gap-1.5">
                <Clock className="w-3.5 h-3.5 text-slate-400 shrink-0" />
                <span>Thời gian ôn tập dự kiến: ~{data.reviewStation.estimatedMinutes ?? 0} phút</span>
              </p>
            </div>

            <button
              type="button"
              data-testid="launch-unified-review-btn"
              onClick={() => {
                setSelectedTagFilterId(null);
                setActiveReviewSubTab("fsrs");
                setActiveMode("review");
              }}
              className="mt-6 w-full py-3 px-4 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs sm:text-sm font-bold shadow-xs transition cursor-pointer flex items-center justify-center gap-2 min-h-[44px]"
            >
              <Brain className="w-4 h-4" />
              <span>Bắt đầu ôn tập ({data.reviewStation.totalDue} thẻ)</span>
            </button>
          </div>
        </div>
      </div>

      {/* Primary Centerpiece: Khóa học của tôi Catalog Grid */}
      <CourseCatalogGrid
        courses={courses}
        courseProgressMap={courseProgressMap}
        isLoading={loadingCourses || isLoadingDashboard}
        errorMessage={coursesError}
        onRefresh={handleRefreshAll}
        onOpenCreateModal={() => setCourseCreateModalOpen(true)}
      />

      {/* Secondary Bento Row: Quick Stats & Resource Library */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-5 sm:gap-6">
        {/* Bento Block 3: Quick Stats & Streak Tile (lg:col-span-4) */}
        <div
          data-testid="quick-stats-tile"
          className="lg:col-span-4 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 p-6 shadow-sm flex flex-col justify-between space-y-4"
        >
          <div>
            <div className="flex items-center justify-between pb-3 border-b border-slate-100 dark:border-slate-800">
              <h3 className="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
                <Target className="w-4 h-4 text-amber-500" />
                <span>Thống kê học tập</span>
              </h3>
              <div
                className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-amber-50 dark:bg-amber-950/40 border border-amber-200 dark:border-amber-900/60 text-amber-800 dark:text-amber-300 text-xs font-bold"
              >
                <Flame className="w-3.5 h-3.5 text-amber-500 fill-amber-500" />
                <span>{data.user.streakDays ?? 0} ngày liên tiếp</span>
              </div>
            </div>

            <div className="mt-4 space-y-3">
              <div className="p-3.5 rounded-xl bg-slate-50 dark:bg-slate-950/50 border border-slate-200/70 dark:border-slate-800">
                <div className="text-xs font-semibold text-slate-600 dark:text-slate-400">
                  Mục tiêu ghi nhớ dài hạn
                </div>
                <div className="text-lg font-bold text-slate-900 dark:text-white mt-0.5">
                  ≥ 90% khả năng lưu giữ
                </div>
                <p className="text-[11px] text-slate-500 mt-1">
                  Thuật toán FSRS v5 tự động điều chỉnh khoảng cách ôn tập theo tốc độ quên cá nhân.
                </p>
              </div>

              {/* Tag Topic Filters */}
              <div className="pt-2">
                <div className="text-xs font-semibold text-slate-600 dark:text-slate-400 mb-2 flex items-center gap-1.5">
                  <Tag className="w-3.5 h-3.5" />
                  <span>Chủ đề ôn tập theo Tag</span>
                </div>
                <div className="flex flex-wrap gap-2">
                  {data.tags.map((t) => (
                    <button
                      key={t.id}
                      type="button"
                      data-testid={`dashboard-tag-${t.name}`}
                      onClick={() => {
                        setSelectedTagFilterId(t.id);
                        setActiveReviewSubTab("fsrs");
                        setActiveMode("review");
                      }}
                      className="px-3 py-1.5 rounded-full text-xs font-semibold bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-slate-700 dark:text-slate-200 transition cursor-pointer min-h-[36px]"
                    >
                      #{t.name} ({t.cardCount})
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Bento Block 4: Document & Resource Library Grid (lg:col-span-8) */}
        <div
          data-testid="document-library-grid"
          className="lg:col-span-8 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 p-6 shadow-sm flex flex-col justify-between"
        >
          <div className="space-y-4">
            <div className="flex flex-wrap items-center justify-between gap-2 pb-3 border-b border-slate-100 dark:border-slate-800">
              <div className="flex items-center gap-2">
                <h3 className="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
                  <Layers className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
                  <span>Thư viện tài liệu & học liệu</span>
                </h3>
                <span className="text-xs text-slate-500 dark:text-slate-400 font-medium">
                  ({courses.length + data.documents.length + data.lessons.length + data.notebooks.length})
                </span>
              </div>

              <div className="flex flex-wrap items-center gap-1.5">
                <button
                  type="button"
                  data-testid="library-filter-all"
                  onClick={() => setLibraryFilter("all")}
                  className={`px-3 py-2 min-h-[42px] rounded-lg text-xs font-semibold cursor-pointer transition inline-flex items-center justify-center ${
                    libraryFilter === "all"
                      ? "bg-indigo-600 text-white"
                      : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"
                  }`}
                >
                  Tất cả
                </button>
                <button
                  type="button"
                  data-testid="library-filter-course"
                  onClick={() => setLibraryFilter("course")}
                  className={`px-3 py-2 min-h-[42px] rounded-lg text-xs font-semibold cursor-pointer transition inline-flex items-center justify-center ${
                    libraryFilter === "course"
                      ? "bg-indigo-600 text-white"
                      : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"
                  }`}
                >
                  Khóa học ({courses.length})
                </button>
                <button
                  type="button"
                  data-testid="library-filter-pdf"
                  onClick={() => setLibraryFilter("pdf")}
                  className={`px-3 py-2 min-h-[42px] rounded-lg text-xs font-semibold cursor-pointer transition inline-flex items-center justify-center ${
                    libraryFilter === "pdf"
                      ? "bg-indigo-600 text-white"
                      : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"
                  }`}
                >
                  PDF ({data.documents.length})
                </button>
                <button
                  type="button"
                  data-testid="library-filter-lesson"
                  onClick={() => setLibraryFilter("lesson")}
                  className={`px-3 py-2 min-h-[42px] rounded-lg text-xs font-semibold cursor-pointer transition inline-flex items-center justify-center ${
                    libraryFilter === "lesson"
                      ? "bg-indigo-600 text-white"
                      : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"
                  }`}
                >
                  Bài học ({data.lessons.length})
                </button>
                <button
                  type="button"
                  data-testid="library-filter-notebook"
                  onClick={() => setLibraryFilter("notebook")}
                  className={`px-3 py-2 min-h-[42px] rounded-lg text-xs font-semibold cursor-pointer transition inline-flex items-center justify-center ${
                    libraryFilter === "notebook"
                      ? "bg-indigo-600 text-white"
                      : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"
                  }`}
                >
                  Ghi chú ({data.notebooks.length})
                </button>
                <button
                  type="button"
                  data-testid="create-standalone-notebook-btn"
                  disabled={creatingNotebook}
                  onClick={handleCreateStandaloneNotebook}
                  className="px-3 py-2 min-h-[42px] rounded-lg border border-indigo-500/40 bg-indigo-50 dark:bg-indigo-950/60 hover:bg-indigo-600 hover:text-white text-indigo-700 dark:text-indigo-300 text-xs font-bold cursor-pointer transition inline-flex items-center justify-center"
                >
                  {creatingNotebook ? "Đang tạo..." : "+ Tạo ghi chú mới"}
                </button>
                <button
                  type="button"
                  data-testid="open-course-create-btn"
                  onClick={() => setCourseCreateModalOpen(true)}
                  className="px-3 py-2 min-h-[42px] rounded-lg border border-indigo-600 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer transition shadow-2xs inline-flex items-center justify-center gap-1"
                >
                  <Sparkles className="w-3 h-3 text-amber-300" />
                  <span>+ Tạo khóa học</span>
                </button>
              </div>
            </div>

            {courses.length === 0 && data.documents.length === 0 && data.lessons.length === 0 && data.notebooks.length === 0 ? (
              <div className="py-12 text-center space-y-3">
                <div className="w-12 h-12 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-400 flex items-center justify-center mx-auto">
                  <BookOpen className="w-6 h-6" />
                </div>
                <div className="text-sm font-bold text-slate-700 dark:text-slate-300">
                  Chưa có tài liệu nào trong thư viện
                </div>
                <p className="text-xs text-slate-500 max-w-sm mx-auto">
                  Tải lên tài liệu PDF hoặc giáo trình Markdown để bắt đầu đọc và ghi nhớ kiến thức.
                </p>
              </div>
            ) : (
              <div className="divide-y divide-slate-100 dark:divide-slate-800">
                {/* Compact Course Index when filtered explicitly to "course" */}
                {libraryFilter === "course" &&
                  courses.map((course) => {
                    const cid = course.courseId || course.id || "";
                    const chapterCount = course.chapterCount ?? course.chapters?.length ?? 0;
                    const lessonCount =
                      course.lessonCount ??
                      course.chapters?.reduce((acc, ch) => acc + (ch.lessons?.length ?? 0), 0) ??
                      0;
                    const prog = courseProgressMap[cid] || {
                      percentCompleted: 0,
                      completedLessons: 0,
                      totalLessons: lessonCount,
                      completedLessonIds: []
                    };
                    const nextLesson = resolveNextCourseLesson(course, prog.completedLessonIds);
                    const percent = Math.max(0, Math.min(100, prog.percentCompleted));
                    const domain = resolveCourseDomain(course);

                    return (
                      <div
                        key={`lib-course-${cid || course.title}`}
                        data-testid="course-card-item"
                        className="py-3 flex flex-wrap items-center justify-between gap-3 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 px-2 rounded-xl transition"
                      >
                        <div className="min-w-0 flex-1">
                          <div className="flex flex-wrap items-center gap-2">
                            <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold border ${domain.badgeClass}`}>
                              {domain.badgeLabel}
                            </span>
                            <span className="text-xs sm:text-sm font-bold text-slate-900 dark:text-slate-100 truncate">
                              {course.title}
                            </span>
                          </div>
                          <div className="text-[11px] text-slate-500 dark:text-slate-400 mt-1 flex flex-wrap items-center gap-2">
                            <span>{chapterCount} chương</span>
                            <span>•</span>
                            <span>{lessonCount} bài giảng</span>
                            <span>•</span>
                            <span className="font-semibold text-indigo-600 dark:text-indigo-400">
                              {percent}% hoàn thành
                            </span>
                          </div>
                        </div>

                        <div className="flex items-center gap-1.5 shrink-0">
                          <button
                            type="button"
                            onClick={() => {
                              if (cid) setActiveCourseId(cid);
                              const targetLessonId = nextLesson.id ?? course.firstLessonId;
                              if (targetLessonId) {
                                setActiveLesson(targetLessonId);
                              } else if (course.sourceDocumentId) {
                                setActiveDocument(course.sourceDocumentId);
                              }
                              setActiveMode("reading");
                            }}
                            className="px-3 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold transition cursor-pointer min-h-[42px]"
                          >
                            Vào học
                          </button>
                          <button
                            type="button"
                            onClick={() => {
                              if (cid) {
                                setActiveCourseId(cid);
                                setEditingCourseId(cid);
                              }
                              setActiveMode("studio");
                            }}
                            className="px-3 py-2 rounded-xl border border-purple-500/40 bg-purple-50 dark:bg-purple-950/70 hover:bg-purple-600 hover:text-white text-purple-700 dark:text-purple-300 text-xs font-bold transition cursor-pointer min-h-[42px]"
                          >
                            Studio
                          </button>
                        </div>
                      </div>
                    );
                  })}

                {/* Documents List */}
                {(libraryFilter === "all" || libraryFilter === "pdf") &&
                  (libraryFilter === "pdf" || showAllDocs
                    ? data.documents
                    : data.documents.slice(0, 6)
                  ).map((doc) => (
                    <div
                      key={doc.id}
                      className="py-3 flex items-center justify-between gap-3 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 px-2 rounded-xl transition"
                    >
                      <div className="min-w-0">
                        <div className="text-xs sm:text-sm font-semibold text-slate-900 dark:text-slate-100 truncate flex items-center gap-1.5">
                          <FileText className="w-3.5 h-3.5 text-emerald-500 shrink-0" />
                          <span>{doc.title}</span>
                        </div>
                        <div className="text-[11px] text-slate-500 mt-0.5 flex flex-wrap items-center gap-2">
                          <span>{doc.totalPages} trang</span>
                          <span>•</span>
                          <span>{formatFileSize(doc.fileSizeBytes)}</span>
                          <span>•</span>
                          <span className="text-emerald-600 dark:text-emerald-400 font-semibold">
                            {doc.ocrStatus === "completed" ? "Đã trích xuất" : doc.ocrStatus}
                          </span>
                        </div>
                      </div>
                      <div className="flex flex-wrap items-center gap-1.5 shrink-0">
                        <button
                          type="button"
                          data-testid="generate-course-from-doc-btn"
                          disabled={generatingDocId === doc.id}
                          onClick={() => handleGenerateCourseFromDoc(doc.id)}
                          className="px-3 py-2 rounded-xl border border-indigo-500/40 bg-indigo-50 dark:bg-indigo-950/60 hover:bg-indigo-600 hover:text-white text-indigo-700 dark:text-indigo-300 text-xs font-bold transition cursor-pointer min-h-[42px]"
                        >
                          {generatingDocId === doc.id ? "Đang sinh..." : "✨ Sinh Khóa học"}
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            setActiveDocument(doc.id, 1, doc.totalPages ?? null);
                            setActiveMode("reading");
                          }}
                          className="px-3 py-2 rounded-xl bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer min-h-[42px]"
                        >
                          Mở đọc
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            setActiveDocument(doc.id, 1, doc.totalPages ?? null);
                            setActiveMode("split");
                          }}
                          className="px-3 py-2 rounded-xl bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer min-h-[42px]"
                        >
                          Split PKM
                        </button>
                      </div>
                    </div>
                  ))}
                {libraryFilter === "all" && data.documents.length > 6 && (
                  <div className="py-2 text-center">
                    <button
                      type="button"
                      onClick={() => setShowAllDocs((v) => !v)}
                      className="text-xs font-semibold text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer px-4 py-2 min-h-[42px] inline-flex items-center justify-center rounded-xl"
                    >
                      {showAllDocs
                        ? "Thu gọn danh sách PDF"
                        : `Xem thêm ${data.documents.length - 6} tài liệu PDF`}
                    </button>
                  </div>
                )}

                {/* Lessons List (capped cleanly so large synced catalogs stay fast) */}
                {(libraryFilter === "all" || libraryFilter === "lesson") &&
                  (() => {
                    const lessonLimit = libraryFilter === "lesson" ? 12 : 6;
                    const visibleLessons = showAllLessons
                      ? data.lessons
                      : data.lessons.slice(0, lessonLimit);
                    return (
                      <>
                        {visibleLessons.map((les) => (
                          <div
                            key={les.id}
                            className="py-3 flex items-center justify-between gap-3 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 px-2 rounded-xl transition"
                          >
                            <div className="min-w-0">
                              <div className="text-xs sm:text-sm font-semibold text-slate-900 dark:text-slate-100 truncate flex items-center gap-1.5">
                                <BookOpen className="w-3.5 h-3.5 text-indigo-500 shrink-0" />
                                <span>{les.title}</span>
                              </div>
                              <div className="text-[11px] text-slate-500 mt-0.5">
                                Bài học • Phiên bản v{les.currentVersion}
                              </div>
                            </div>
                            <div className="flex items-center gap-1.5 shrink-0">
                              <button
                                type="button"
                                onClick={() => {
                                  if (les.courseId) setActiveCourseId(les.courseId);
                                  setActiveLesson(les.id);
                                  setActiveMode("reading");
                                }}
                                className="px-3 py-2 rounded-xl bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer min-h-[42px]"
                              >
                                Đọc sâu
                              </button>
                              <button
                                type="button"
                                onClick={() => {
                                  if (les.courseId) setActiveCourseId(les.courseId);
                                  setActiveLesson(les.id);
                                  setActiveMode("split");
                                }}
                                className="px-3 py-2 rounded-xl bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer min-h-[42px]"
                              >
                                Split PKM
                              </button>
                            </div>
                          </div>
                        ))}
                        {data.lessons.length > lessonLimit && (
                          <div className="py-2 text-center">
                            <button
                              type="button"
                              onClick={() => setShowAllLessons((v) => !v)}
                              className="text-xs font-semibold text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer px-4 py-2 min-h-[42px] inline-flex items-center justify-center rounded-xl"
                            >
                              {showAllLessons
                                ? "Thu gọn danh sách bài học"
                                : `Xem thêm ${data.lessons.length - lessonLimit} bài học khác`}
                            </button>
                          </div>
                        )}
                      </>
                    );
                  })()}

                {/* Notebooks List */}
                {(libraryFilter === "all" || libraryFilter === "notebook") &&
                  (libraryFilter === "notebook" ? data.notebooks : data.notebooks.slice(0, 4)).map((nb) => (
                    <div
                      key={nb.id}
                      className="py-3 flex items-center justify-between gap-3 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 px-2 rounded-xl transition"
                    >
                      <div className="min-w-0">
                        <div className="text-xs sm:text-sm font-semibold text-slate-900 dark:text-slate-100 truncate flex items-center gap-1.5">
                          <PenTool className="w-3.5 h-3.5 text-amber-500 shrink-0" />
                          <span>[Ghi chú] {nb.title}</span>
                        </div>
                        <div className="text-[11px] text-slate-500 mt-0.5">
                          Bản ghi chú tổng hợp • v{nb.version}
                        </div>
                      </div>
                      <button
                        type="button"
                        onClick={() => {
                          if (nb.primarySourceLessonId) {
                            setActiveLesson(nb.primarySourceLessonId);
                          } else if (nb.primarySourceDocumentId) {
                            setActiveDocument(nb.primarySourceDocumentId);
                          }
                          setActiveMode("split");
                        }}
                        className="px-3 py-2 rounded-xl bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer shrink-0 min-h-[42px]"
                      >
                        Mở ghi chú
                      </button>
                    </div>
                  ))}

                {libraryFilter === "all" &&
                  data.documents.length === 0 &&
                  data.lessons.length === 0 &&
                  data.notebooks.length === 0 &&
                  courses.length > 0 && (
                    <div className="py-8 text-center text-xs text-slate-500 dark:text-slate-400">
                      Các khóa học đang hiển thị ở mục &ldquo;Khóa học của tôi&rdquo; phía trên. Chọn tab &ldquo;Khóa học ({courses.length})&rdquo; hoặc tải PDF / tạo ghi chú mới.
                    </div>
                  )}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Upload Document Modal (Triggered by open-upload-pdf-btn) */}
      {uploadModalOpen && (
        <div
          data-testid="upload-document-modal"
          className="fixed inset-0 z-50 bg-slate-950/60 backdrop-blur-xs flex items-center justify-center p-4 animate-in fade-in"
        >
          <form
            onSubmit={handleUploadDocument}
            className="w-full max-w-lg rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 shadow-xl space-y-4 max-h-[90vh] overflow-y-auto"
          >
            <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-3">
              <div>
                <h3 className="text-base font-bold text-slate-900 dark:text-white flex items-center gap-2">
                  <Upload className="w-4 h-4 text-indigo-600" />
                  <span>Tải lên tài liệu học tập (PDF / Markdown)</span>
                </h3>
                <p className="text-[11px] text-indigo-600 dark:text-indigo-400 font-semibold mt-0.5">
                  Tải tài liệu &amp; Tự động Sinh Khóa học AI (Chương mục, Bài giảng, Flashcard &amp; Quiz)
                </p>
              </div>
              <button
                type="button"
                aria-label="Close"
                onClick={() => {
                  setUploadModalOpen(false);
                  setUploadError(null);
                  setUploadStep("idle");
                }}
                className="text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 p-2 rounded-lg cursor-pointer min-h-[40px] min-w-[40px] flex items-center justify-center"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {/* Error Notification Alert Banner */}
            {uploadError && (
              <div
                data-testid="upload-error-alert"
                className="p-3.5 rounded-xl bg-rose-50 dark:bg-rose-950/50 border border-rose-200 dark:border-rose-900/60 text-rose-700 dark:text-rose-300 text-xs font-semibold flex items-start gap-2.5 animate-in fade-in"
              >
                <AlertCircle className="w-4 h-4 shrink-0 mt-0.5 text-rose-600 dark:text-rose-400" />
                <div className="flex-1 leading-relaxed">{uploadError}</div>
              </div>
            )}

            {/* Hidden native file input */}
            <input
              type="file"
              ref={fileInputRef}
              data-testid="document-file-input"
              accept=".pdf,.md,.markdown,.txt,application/pdf,text/markdown,text/plain"
              onChange={(e) => {
                if (e.target.files && e.target.files[0]) {
                  handleProcessFile(e.target.files[0]);
                }
              }}
              className="hidden"
            />

            {/* Dropzone & File Selector Area */}
            {!selectedFile ? (
              <div
                data-testid="document-dropzone"
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
                onClick={() => fileInputRef.current?.click()}
                className={`p-6 rounded-2xl border-2 border-dashed text-center transition cursor-pointer flex flex-col items-center justify-center gap-2.5 ${
                  isDragging
                    ? "border-indigo-600 bg-indigo-50/60 dark:bg-indigo-950/40"
                    : "border-slate-300 dark:border-slate-700 hover:border-indigo-500 dark:hover:border-indigo-500 bg-slate-50/50 dark:bg-slate-950/40"
                }`}
              >
                <div className="w-12 h-12 rounded-full bg-indigo-50 dark:bg-indigo-950/80 text-indigo-600 dark:text-indigo-400 flex items-center justify-center">
                  <FileUp className="w-6 h-6" />
                </div>
                <div>
                  <p className="text-xs sm:text-sm font-bold text-slate-800 dark:text-slate-200">
                    Kéo thả file PDF hoặc Markdown vào đây
                  </p>
                  <p className="text-[11px] text-slate-500 dark:text-slate-400 mt-0.5">
                    hoặc nhấp để chọn tệp từ máy tính của bạn
                  </p>
                </div>
                <button
                  type="button"
                  data-testid="browse-computer-file-btn"
                  onClick={(e) => {
                    e.stopPropagation();
                    fileInputRef.current?.click();
                  }}
                  className="px-4 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 text-xs font-semibold text-slate-700 dark:text-slate-200 transition shadow-2xs min-h-[40px]"
                >
                  Chọn file từ máy tính
                </button>
                <span className="text-[11px] text-slate-500 dark:text-slate-400 font-medium">
                  Hạn mức tối đa 50MB (Gói Free) • Hỗ trợ .pdf, .md, .txt
                </span>
              </div>
            ) : (
              <div
                data-testid="selected-file-card"
                className="p-3.5 rounded-xl bg-slate-50 dark:bg-slate-800/60 border border-slate-200 dark:border-slate-700 flex items-center justify-between gap-3"
              >
                <div className="flex items-center gap-2.5 min-w-0">
                  <span
                    className={`px-2 py-1 rounded-md text-[10px] font-extrabold uppercase shrink-0 ${
                      fileMimeType === "application/pdf"
                        ? "bg-rose-100 dark:bg-rose-950/70 text-rose-700 dark:text-rose-300 border border-rose-200 dark:border-rose-800"
                        : "bg-emerald-100 dark:bg-emerald-950/70 text-emerald-700 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800"
                    }`}
                  >
                    {fileMimeType === "application/pdf" ? "PDF" : "MD"}
                  </span>
                  <div className="min-w-0">
                    <p className="text-xs font-bold text-slate-900 dark:text-slate-100 truncate">
                      {selectedFile.name}
                    </p>
                    <p className="text-[11px] text-slate-500">
                      {formatFileSize(fileSizeBytes)} • {fileMimeType === "application/pdf" ? "Tự động phân tích PdfPig" : `${totalPages} trang`}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2 shrink-0">
                  <button
                    type="button"
                    onClick={() => fileInputRef.current?.click()}
                    className="text-xs font-semibold text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer min-h-[40px] px-2 flex items-center"
                  >
                    Đổi file
                  </button>
                  <button
                    type="button"
                    onClick={handleClearSelectedFile}
                    title="Gỡ file"
                    className="p-2 rounded-lg text-slate-400 hover:text-rose-600 dark:hover:text-rose-400 cursor-pointer min-h-[40px] min-w-[40px] flex items-center justify-center"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            )}

            {/* Multi-step progress indicator when uploading */}
            {uploading && (
              <div className="space-y-2 p-3.5 rounded-xl bg-indigo-50/70 dark:bg-indigo-950/40 border border-indigo-200 dark:border-indigo-900/60 animate-in fade-in">
                <div className="flex items-center justify-between text-xs font-semibold text-indigo-700 dark:text-indigo-300">
                  <span>
                    {uploadStep === "extracting"
                      ? "Đang phân tích cấu trúc PdfPig & trích xuất trang..."
                      : uploadStep === "completed"
                      ? "Hoàn tất xử lý tài liệu!"
                      : "Đang gửi dữ liệu lên máy chủ..."}
                  </span>
                  <span>{uploadStep === "extracting" ? "75%" : uploadStep === "completed" ? "100%" : "30%"}</span>
                </div>
                <div className="w-full bg-indigo-200/60 dark:bg-indigo-900/60 h-2 rounded-full overflow-hidden">
                  <div
                    className={`h-full bg-indigo-600 transition-all duration-300 ${
                      uploadStep === "extracting"
                        ? "w-3/4"
                        : uploadStep === "completed"
                        ? "w-full"
                        : "w-1/3"
                    }`}
                  />
                </div>
              </div>
            )}

            <div>
              <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                Tiêu đề tài liệu / giáo trình:
              </label>
              <input
                type="text"
                required
                value={docTitle}
                onChange={(e) => setDocTitle(e.target.value)}
                placeholder="VD: Modern Operating Systems - Andrew S. Tanenbaum.pdf"
                className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-transparent text-sm focus:ring-2 focus:ring-indigo-500 outline-none"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                Nội dung văn bản trích xuất (Text-layer / Markdown):
              </label>
              <textarea
                rows={3}
                value={docText}
                onChange={(e) => setDocText(e.target.value)}
                placeholder="Dán nội dung chương sách hoặc để trống để tạo văn bản mẫu..."
                className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-transparent text-xs focus:ring-2 focus:ring-indigo-500 outline-none"
              />
            </div>
            <div className="flex justify-end items-center gap-2.5 pt-2">
              <button
                type="button"
                onClick={() => {
                  setUploadModalOpen(false);
                  setUploadError(null);
                  setUploadStep("idle");
                }}
                className="px-4 py-2.5 rounded-xl border border-slate-300 dark:border-slate-700 text-xs font-semibold cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-800 transition min-h-[44px]"
              >
                Hủy
              </button>
              <button
                type="submit"
                disabled={uploading}
                className="px-5 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs sm:text-sm font-semibold cursor-pointer transition shadow-xs flex items-center gap-2 min-h-[44px]"
              >
                {uploading ? (
                  <>
                    <Upload className="w-4 h-4 animate-bounce" />
                    <span>Đang xử lý...</span>
                  </>
                ) : (
                  <span>Nhập vào Workspace</span>
                )}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Course Creator Studio Unified Hub Modal */}
      <CourseCreateModal
        isOpen={courseCreateModalOpen}
        onClose={() => setCourseCreateModalOpen(false)}
        onSuccess={() => {
          void handleRefreshAll();
        }}
      />
    </div>
  );
}
