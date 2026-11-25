"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import {
  AlertCircle,
  ArrowDown,
  ArrowUp,
  BookOpen,
  Check,
  ChevronDown,
  ChevronRight,
  Copy,
  Download,
  Eye,
  FileText,
  Globe,
  GripVertical,
  Info,
  Loader2,
  Lock,
  Pencil,
  Play,
  Plus,
  Save,
  Shield,
  Sparkles,
  Trash2,
  Wand2,
  X,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { LessonMarkdownView } from "@/components/reader/LessonMarkdownView";

interface Lesson {
  id: string;
  lessonId?: string;
  chapterId?: string;
  title: string;
  contentMarkdown?: string | null;
  plainText?: string | null;
  content?: string | null;
  videoUrl?: string | null;
  estimatedMinutes?: number;
  sortOrder?: number;
}

interface Chapter {
  id: string;
  chapterId?: string;
  title: string;
  sortOrder?: number;
  lessons: Lesson[];
}

interface CourseDetail {
  id?: string;
  courseId?: string;
  title: string;
  description?: string | null;
  category?: string | null;
  level?: string | null;
  learningOutcomes?: string[];
  prerequisites?: string[];
  visibility?: string;
  isPublished?: boolean;
  isSystemCourse?: boolean;
  canDelete?: boolean;
  chapters: Chapter[];
}

export default function CourseEditPage() {
  const params = useParams();
  const router = useRouter();
  const courseId = params.id as string;
  const { activeUserHeaderId } = useWorkspaceStore();

  const [course, setCourse] = useState<CourseDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [expandedChapters, setExpandedChapters] = useState<Set<string>>(new Set());
  const [selectedLesson, setSelectedLesson] = useState<Lesson | null>(null);

  // Lesson Editor states
  const [editTitle, setEditTitle] = useState("");
  const [editContent, setEditContent] = useState("");
  const [editVideoUrl, setEditVideoUrl] = useState("");
  const [editMinutes, setEditMinutes] = useState(10);
  const [editorTab, setEditorTab] = useState<"write" | "preview">("write");

  const [isSaving, setIsSaving] = useState(false);
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [errorBanner, setErrorBanner] = useState<string | null>(null);

  // Course Title & Top Actions
  const [isEditingCourseTitle, setIsEditingCourseTitle] = useState(false);
  const [tempCourseTitle, setTempCourseTitle] = useState("");
  const [isTogglingPublish, setIsTogglingPublish] = useState(false);
  const [isExporting, setIsExporting] = useState(false);

  // Course Metadata Panel states
  const [showMetaPanel, setShowMetaPanel] = useState(false);
  const [metaTitle, setMetaTitle] = useState("");
  const [metaDescription, setMetaDescription] = useState("");
  const [metaCategory, setMetaCategory] = useState("");
  const [metaLevel, setMetaLevel] = useState<"foundation" | "intermediate" | "advanced">("foundation");
  const [metaOutcomes, setMetaOutcomes] = useState<string[]>([]);
  const [metaPrerequisites, setMetaPrerequisites] = useState<string[]>([]);
  const [isSavingMeta, setIsSavingMeta] = useState(false);

  // Chapter & Lesson Management
  const [showNewChapterInput, setShowNewChapterInput] = useState(false);
  const [newChapterTitle, setNewChapterTitle] = useState("");
  const [creatingChapter, setCreatingChapter] = useState(false);

  const [addingLessonChapterId, setAddingLessonChapterId] = useState<string | null>(null);
  const [newLessonTitle, setNewLessonTitle] = useState("");
  const [creatingLesson, setCreatingLesson] = useState(false);

  const [confirmDeleteChapterId, setConfirmDeleteChapterId] = useState<string | null>(null);
  const [confirmDeleteLessonId, setConfirmDeleteLessonId] = useState<string | null>(null);

  // Reordering
  const [hasUnsavedOrder, setHasUnsavedOrder] = useState(false);
  const [isSavingOrder, setIsSavingOrder] = useState(false);
  const [draggedLessonInfo, setDraggedLessonInfo] = useState<{
    chapterId: string;
    lessonIndex: number;
  } | null>(null);

  // AI Lesson Assistant states
  const [showAiAssistant, setShowAiAssistant] = useState(false);
  const [aiAction, setAiAction] = useState<"expand" | "summarize" | "examples" | "exercise">("expand");
  const [aiPrompt, setAiPrompt] = useState("");
  const [isAiEnhancing, setIsAiEnhancing] = useState(false);
  const [aiResult, setAiResult] = useState<{
    content: string;
    usedLiveAi?: boolean;
    model?: string;
  } | null>(null);
  const [aiResultTab, setAiResultTab] = useState<"preview" | "markdown">("preview");
  const [aiCopied, setAiCopied] = useState(false);

  // Delete Course (2-Step Confirm)
  const [showDeleteCourseModal, setShowDeleteCourseModal] = useState(false);
  const [deleteConfirmStep, setDeleteConfirmStep] = useState<1 | 2>(1);
  const [isDeletingCourse, setIsDeletingCourse] = useState(false);

  const getAuthHeaders = useCallback(
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

  const selectLesson = useCallback(
    async (lesson: Lesson) => {
      const lId = lesson.lessonId || lesson.id;
      const initialContent = lesson.contentMarkdown ?? lesson.plainText ?? lesson.content ?? "";
      setSelectedLesson(lesson);
      setEditTitle(lesson.title);
      setEditContent(initialContent);
      setEditVideoUrl(lesson.videoUrl ?? "");
      setEditMinutes(lesson.estimatedMinutes ?? 10);
      setSaveMessage(null);
      setErrorBanner(null);
      setAiResult(null);
      setAiResultTab("preview");
      setAiCopied(false);

      if (!initialContent && lId) {
        try {
          const resp = await fetch(`/api/v1/lessons/${lId}`, { headers: getAuthHeaders() });
          if (resp.ok) {
            const detail = await resp.json();
            const fetchedMd = detail.contentMarkdown ?? detail.plainText ?? "";
            if (fetchedMd) setEditContent(fetchedMd);
            if (typeof detail.videoUrl === "string") setEditVideoUrl(detail.videoUrl);
            if (typeof detail.estimatedMinutes === "number") setEditMinutes(detail.estimatedMinutes);
            setSelectedLesson((prev) =>
              prev && (prev.lessonId || prev.id) === lId
                ? {
                    ...prev,
                    contentMarkdown: detail.contentMarkdown ?? prev.contentMarkdown,
                    plainText: detail.plainText ?? prev.plainText,
                    videoUrl: typeof detail.videoUrl === "string" ? detail.videoUrl : prev.videoUrl,
                    estimatedMinutes:
                      typeof detail.estimatedMinutes === "number"
                        ? detail.estimatedMinutes
                        : prev.estimatedMinutes,
                  }
                : prev
            );
          }
        } catch {
          // Keep selected lesson state cleanly
        }
      }
    },
    [getAuthHeaders]
  );

  const fetchCourse = useCallback(async () => {
    setIsLoading(true);
    setErrorBanner(null);
    try {
      const resp = await fetch(`/api/v1/courses/${courseId}?includeMarkdown=true`, {
        headers: getAuthHeaders(),
      });
      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(
          `Không thể tải thông tin khóa học (${resp.status})${errText ? `: ${errText}` : ""}`
        );
        setCourse(null);
        return;
      }
      const data: CourseDetail = await resp.json();
      setCourse(data);
      setTempCourseTitle(data.title ?? "");

      // Hydrate metadata state
      setMetaTitle(data.title || "");
      setMetaDescription(data.description || "");
      setMetaCategory(data.category || "");
      setMetaLevel((data.level as "foundation" | "intermediate" | "advanced") || "foundation");
      setMetaOutcomes(data.learningOutcomes || []);
      setMetaPrerequisites(data.prerequisites || []);

      const ids = new Set<string>();
      (data.chapters ?? []).forEach((ch: Chapter) => ids.add(ch.chapterId || ch.id));
      setExpandedChapters(ids);

      const firstLesson = data.chapters?.flatMap((c) => c.lessons ?? [])[0];
      if (firstLesson) {
        void selectLesson(firstLesson);
      }
    } catch (err) {
      setErrorBanner(
        err instanceof Error ? err.message : "Lỗi kết nối khi tải thông tin khóa học."
      );
    } finally {
      setIsLoading(false);
    }
  }, [courseId, getAuthHeaders, selectLesson]);

  useEffect(() => {
    fetchCourse();
  }, [fetchCourse]);

  const toggleChapter = (chapterId: string) => {
    setExpandedChapters((prev) => {
      const next = new Set(prev);
      if (next.has(chapterId)) next.delete(chapterId);
      else next.add(chapterId);
      return next;
    });
  };

  const handleSaveCourseTitle = async () => {
    const trimmed = tempCourseTitle.trim();
    if (!course || !trimmed) return;
    setErrorBanner(null);
    try {
      const resp = await fetch(`/api/v1/courses/${courseId}`, {
        method: "PUT",
        headers: getAuthHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({ title: trimmed }),
      });
      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(`Không thể đổi tên khóa học (${resp.status})${errText ? `: ${errText}` : ""}`);
        return;
      }
      setCourse((prev) => (prev ? { ...prev, title: trimmed } : prev));
      setMetaTitle(trimmed);
      setIsEditingCourseTitle(false);
      setSaveMessage("Đã cập nhật tên khóa học.");
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi kết nối khi đổi tên khóa học.");
    }
  };

  const handleSaveMetadata = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!course) return;
    setIsSavingMeta(true);
    setErrorBanner(null);
    try {
      const payload = {
        title: metaTitle.trim() || course.title,
        description: metaDescription.trim() || undefined,
        category: metaCategory.trim() || undefined,
        level: metaLevel,
        learningOutcomes: metaOutcomes.map((o) => o.trim()).filter(Boolean),
        prerequisites: metaPrerequisites.map((p) => p.trim()).filter(Boolean),
      };

      const resp = await fetch(`/api/v1/courses/${courseId}`, {
        method: "PUT",
        headers: getAuthHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify(payload),
      });

      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(`Không thể lưu metadata khóa học (${resp.status})${errText ? `: ${errText}` : ""}`);
        return;
      }

      setCourse((prev) =>
        prev
          ? {
              ...prev,
              title: payload.title,
              description: payload.description,
              category: payload.category,
              level: payload.level,
              learningOutcomes: payload.learningOutcomes,
              prerequisites: payload.prerequisites,
            }
          : prev
      );
      setSaveMessage("Đã lưu thông tin & metadata khóa học thành công.");
      setShowMetaPanel(false);
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi khi lưu metadata.");
    } finally {
      setIsSavingMeta(false);
    }
  };

  const handleTogglePublish = async () => {
    if (!course || isTogglingPublish) return;
    setIsTogglingPublish(true);
    setErrorBanner(null);
    try {
      const resp = await fetch(`/api/v1/courses/${courseId}/publish`, {
        method: "POST",
        headers: getAuthHeaders({ "Content-Type": "application/json" }),
      });
      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(
          `Không thể thay đổi trạng thái xuất bản (${resp.status})${errText ? `: ${errText}` : ""}`
        );
        return;
      }
      const data = await resp.json();
      setCourse((prev) =>
        prev
          ? {
              ...prev,
              visibility: data.visibility ?? prev.visibility,
              isPublished: Boolean(data.isPublished ?? data.visibility === "public"),
            }
          : prev
      );
      setSaveMessage(
        data.isPublished || data.visibility === "public"
          ? "Khóa học đã được công khai."
          : "Đã chuyển khóa học về bản nháp cá nhân."
      );
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi kết nối khi cập nhật trạng thái.");
    } finally {
      setIsTogglingPublish(false);
    }
  };

  const handleExportZip = async () => {
    if (!course || isExporting) return;
    setIsExporting(true);
    setErrorBanner(null);
    try {
      const resp = await fetch(`/api/v1/courses/${courseId}/export-zip`, {
        headers: getAuthHeaders(),
      });
      if (!resp.ok) {
        setErrorBanner(`Không thể xuất ZIP khóa học (${resp.status}).`);
        return;
      }
      const blob = await resp.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `${(course.title || "course").replace(/[^a-zA-Z0-9\u00C0-\u1EF9-_ ]+/g, "_")}.zip`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(url);
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi khi xuất ZIP.");
    } finally {
      setIsExporting(false);
    }
  };

  const handleSave = async () => {
    if (!selectedLesson) return;
    const trimmedTitle = editTitle.trim();
    if (!trimmedTitle) {
      setErrorBanner("Tiêu đề bài học không được để trống.");
      return;
    }

    setIsSaving(true);
    setSaveMessage(null);
    setErrorBanner(null);
    try {
      const lessonId = selectedLesson.lessonId || selectedLesson.id;
      const resp = await fetch(`/api/v1/lessons/${lessonId}`, {
        method: "PUT",
        headers: getAuthHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          title: trimmedTitle,
          contentMarkdown: editContent,
          plainText: editContent,
          videoUrl: editVideoUrl.trim(),
          estimatedMinutes: Number(editMinutes) || 10,
        }),
      });

      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(`Lưu bài học thất bại (${resp.status})${errText ? `: ${errText}` : ""}`);
        return;
      }

      if (course) {
        const updatedChapters = course.chapters.map((ch) => ({
          ...ch,
          lessons: ch.lessons.map((l) =>
            (l.lessonId || l.id) === lessonId
              ? {
                  ...l,
                  title: trimmedTitle,
                  contentMarkdown: editContent,
                  plainText: editContent,
                  videoUrl: editVideoUrl.trim(),
                  estimatedMinutes: Number(editMinutes) || 10,
                }
              : l
          ),
        }));
        setCourse({ ...course, chapters: updatedChapters });
        setSelectedLesson({
          ...selectedLesson,
          title: trimmedTitle,
          contentMarkdown: editContent,
          plainText: editContent,
          videoUrl: editVideoUrl.trim(),
          estimatedMinutes: Number(editMinutes) || 10,
        });
      }
      setSaveMessage("Đã lưu bài học thành công.");
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi kết nối khi lưu bài học.");
    } finally {
      setIsSaving(false);
    }
  };

  // AI Lesson Assistant Trigger
  const handleRunAiEnhance = async (actionToRun?: "expand" | "summarize" | "examples" | "exercise") => {
    if (!selectedLesson) return;
    const act = actionToRun || aiAction;
    setIsAiEnhancing(true);
    setErrorBanner(null);
    setSaveMessage(null);

    const lId = selectedLesson.lessonId || selectedLesson.id;

    try {
      const resp = await fetch(`/api/v1/lessons/${lId}/ai-enhance`, {
        method: "POST",
        headers: getAuthHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          action: act,
          prompt: aiPrompt.trim() || undefined,
          currentMarkdown: editContent,
        }),
      });

      if (!resp.ok) {
        const errJson = await resp.json().catch(() => ({}));
        throw new Error(errJson.error || `Trợ lý AI thất bại (Mã lỗi ${resp.status})`);
      }

      const data = await resp.json();
      setAiResult({
        content: data.content || "",
        usedLiveAi: Boolean(data.usedLiveAi),
        model: data.model || "Gemini 3.8",
      });
      setAiResultTab("preview");
      setAiCopied(false);
      setSaveMessage("AI đã tạo nội dung gợi ý bên dưới. Bạn có thể chèn hoặc thay thế vào bài!");
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi khi gọi Trợ lý AI.");
    } finally {
      setIsAiEnhancing(false);
    }
  };

  const handleCopyAiResult = async () => {
    if (!aiResult?.content) return;
    try {
      await navigator.clipboard.writeText(aiResult.content);
      setAiCopied(true);
      setTimeout(() => setAiCopied(false), 2000);
    } catch {
      setErrorBanner("Không thể sao chép nội dung vào clipboard.");
    }
  };

  // Delete Course
  const handleDeleteCourse = async () => {
    if (!course || isDeletingCourse) return;
    setIsDeletingCourse(true);
    setErrorBanner(null);
    try {
      const resp = await fetch(`/api/v1/courses/${courseId}`, {
        method: "DELETE",
        headers: getAuthHeaders(),
      });

      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        throw new Error(`Xóa khóa học thất bại (${resp.status})${errText ? `: ${errText}` : ""}`);
      }

      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
      router.push("/app/courses");
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi khi xóa khóa học.");
      setShowDeleteCourseModal(false);
      setDeleteConfirmStep(1);
    } finally {
      setIsDeletingCourse(false);
    }
  };

  const handleCreateChapter = async (e?: React.FormEvent) => {
    e?.preventDefault();
    if (!course || creatingChapter) return;
    const nextOrder = (course.chapters?.length ?? 0) + 1;
    const title = newChapterTitle.trim() || `Chương ${nextOrder}: Chủ đề mới`;

    setCreatingChapter(true);
    setErrorBanner(null);
    try {
      const resp = await fetch(`/api/v1/courses/${courseId}/chapters`, {
        method: "POST",
        headers: getAuthHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({ title, sortOrder: nextOrder }),
      });
      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(`Không thể thêm chương (${resp.status})${errText ? `: ${errText}` : ""}`);
        return;
      }
      const created: Chapter = await resp.json();
      if (!created.lessons) created.lessons = [];
      const chId = created.chapterId || created.id;
      setCourse((prev) => (prev ? { ...prev, chapters: [...prev.chapters, created] } : prev));
      setExpandedChapters((prev) => new Set(prev).add(chId));
      setNewChapterTitle("");
      setShowNewChapterInput(false);
      setSaveMessage(`Đã thêm "${created.title}".`);
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi kết nối khi thêm chương.");
    } finally {
      setCreatingChapter(false);
    }
  };

  const handleDeleteChapter = async (chapterId: string) => {
    if (!course) return;
    setErrorBanner(null);
    try {
      const resp = await fetch(`/api/v1/chapters/${chapterId}`, {
        method: "DELETE",
        headers: getAuthHeaders(),
      });
      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(`Không thể xóa chương (${resp.status})${errText ? `: ${errText}` : ""}`);
        return;
      }
      const deletedChapter = course.chapters.find((c) => (c.chapterId || c.id) === chapterId);
      const selectedLId = selectedLesson ? selectedLesson.lessonId || selectedLesson.id : null;
      if (selectedLId && deletedChapter?.lessons?.some((l) => (l.lessonId || l.id) === selectedLId)) {
        setSelectedLesson(null);
        setEditTitle("");
        setEditContent("");
      }
      setCourse((prev) =>
        prev
          ? { ...prev, chapters: prev.chapters.filter((c) => (c.chapterId || c.id) !== chapterId) }
          : prev
      );
      setConfirmDeleteChapterId(null);
      setSaveMessage("Đã xóa chương.");
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi kết nối khi xóa chương.");
    }
  };

  const handleCreateLesson = async (chapterId: string, e?: React.FormEvent) => {
    e?.preventDefault();
    if (!course || creatingLesson) return;
    const chapter = course.chapters.find((c) => (c.chapterId || c.id) === chapterId);
    const nextOrder = (chapter?.lessons?.length ?? 0) + 1;
    const title = newLessonTitle.trim() || `Bài ${nextOrder}: Nội dung mới`;
    const initialMd = `# ${title}\n\n## Mục tiêu bài học\n- \n\n## Nội dung chính\n`;

    setCreatingLesson(true);
    setErrorBanner(null);
    try {
      const resp = await fetch(`/api/v1/chapters/${chapterId}/lessons`, {
        method: "POST",
        headers: getAuthHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          title,
          contentMarkdown: initialMd,
          videoUrl: "",
          estimatedMinutes: 10,
          sortOrder: nextOrder,
        }),
      });
      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(`Không thể thêm bài học (${resp.status})${errText ? `: ${errText}` : ""}`);
        return;
      }
      const created: Lesson = await resp.json();
      setCourse((prev) =>
        prev
          ? {
              ...prev,
              chapters: prev.chapters.map((c) =>
                (c.chapterId || c.id) === chapterId
                  ? { ...c, lessons: [...(c.lessons ?? []), created] }
                  : c
              ),
            }
          : prev
      );
      setExpandedChapters((prev) => new Set(prev).add(chapterId));
      setNewLessonTitle("");
      setAddingLessonChapterId(null);
      void selectLesson(created);
      setSaveMessage(`Đã tạo bài "${created.title}".`);
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi kết nối khi thêm bài học.");
    } finally {
      setCreatingLesson(false);
    }
  };

  const handleSaveOrder = async (chaptersToSave?: Chapter[]) => {
    const targetChapters = chaptersToSave ?? course?.chapters;
    if (!targetChapters || isSavingOrder) return;

    setIsSavingOrder(true);
    setErrorBanner(null);
    try {
      const payload = {
        chapters: targetChapters.map((ch, idx) => ({
          id: ch.chapterId || ch.id,
          chapterId: ch.chapterId || ch.id,
          sortOrder: idx + 1,
        })),
        lessons: targetChapters.flatMap((ch) =>
          (ch.lessons ?? []).map((l, lIdx) => ({
            id: l.lessonId || l.id,
            lessonId: l.lessonId || l.id,
            chapterId: ch.chapterId || ch.id,
            sortOrder: lIdx + 1,
          }))
        ),
      };

      const resp = await fetch(`/api/v1/courses/${courseId}/reorder`, {
        method: "PUT",
        headers: getAuthHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify(payload),
      });

      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(
          `Không thể lưu thứ tự mới (${resp.status})${errText ? `: ${errText}` : ""}`
        );
        return;
      }

      setHasUnsavedOrder(false);
      setSaveMessage("Đã lưu thứ tự chương và bài học.");
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi kết nối khi lưu thứ tự.");
    } finally {
      setIsSavingOrder(false);
    }
  };

  const handleMoveChapter = (chapterIndex: number, direction: -1 | 1) => {
    if (!course) return;
    const targetIndex = chapterIndex + direction;
    if (targetIndex < 0 || targetIndex >= course.chapters.length) return;
    const nextChapters = [...course.chapters];
    const [moved] = nextChapters.splice(chapterIndex, 1);
    nextChapters.splice(targetIndex, 0, moved);
    const reindexed = nextChapters.map((ch, idx) => ({ ...ch, sortOrder: idx + 1 }));
    setCourse({ ...course, chapters: reindexed });
    setHasUnsavedOrder(true);
  };

  const handleMoveLesson = (chapterId: string, lessonIndex: number, direction: -1 | 1) => {
    if (!course) return;
    const nextChapters = course.chapters.map((ch) => {
      const chKey = ch.chapterId || ch.id;
      if (chKey !== chapterId) return ch;
      const lessons = [...(ch.lessons ?? [])];
      const targetIndex = lessonIndex + direction;
      if (targetIndex < 0 || targetIndex >= lessons.length) return ch;
      const [moved] = lessons.splice(lessonIndex, 1);
      lessons.splice(targetIndex, 0, moved);
      return {
        ...ch,
        lessons: lessons.map((l, idx) => ({ ...l, sortOrder: idx + 1 })),
      };
    });
    setCourse({ ...course, chapters: nextChapters });
    setHasUnsavedOrder(true);
  };

  const handleDropLesson = (targetChapterId: string, targetLessonIndex: number) => {
    if (!course || !draggedLessonInfo) return;
    const { chapterId: srcChapterId, lessonIndex: srcIndex } = draggedLessonInfo;
    setDraggedLessonInfo(null);
    if (srcChapterId !== targetChapterId || srcIndex === targetLessonIndex) return;

    const nextChapters = course.chapters.map((ch) => {
      const chKey = ch.chapterId || ch.id;
      if (chKey !== targetChapterId) return ch;
      const lessons = [...(ch.lessons ?? [])];
      const [moved] = lessons.splice(srcIndex, 1);
      lessons.splice(targetLessonIndex, 0, moved);
      return {
        ...ch,
        lessons: lessons.map((l, idx) => ({ ...l, sortOrder: idx + 1 })),
      };
    });
    setCourse({ ...course, chapters: nextChapters });
    setHasUnsavedOrder(true);
  };

  const handleDeleteLesson = async (chapterId: string, lessonId: string) => {
    if (!course) return;
    setErrorBanner(null);
    try {
      const resp = await fetch(`/api/v1/lessons/${lessonId}`, {
        method: "DELETE",
        headers: getAuthHeaders(),
      });
      if (!resp.ok) {
        const errText = await resp.text().catch(() => "");
        setErrorBanner(`Không thể xóa bài học (${resp.status})${errText ? `: ${errText}` : ""}`);
        return;
      }
      if (selectedLesson && (selectedLesson.lessonId || selectedLesson.id) === lessonId) {
        setSelectedLesson(null);
        setEditTitle("");
        setEditContent("");
      }
      setCourse((prev) =>
        prev
          ? {
              ...prev,
              chapters: prev.chapters.map((c) =>
                (c.chapterId || c.id) === chapterId
                  ? { ...c, lessons: c.lessons.filter((l) => (l.lessonId || l.id) !== lessonId) }
                  : c
              ),
            }
          : prev
      );
      setConfirmDeleteLessonId(null);
      setSaveMessage("Đã xóa bài học.");
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi kết nối khi xóa bài học.");
    }
  };

  useEffect(() => {
    const onKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "s") {
        if (selectedLesson && !isSaving) {
          e.preventDefault();
          void handleSave();
        }
      }
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  });

  if (isLoading) {
    return (
      <div className="h-full flex flex-col bg-bg-primary animate-pulse">
        {/* Top Bar Skeleton */}
        <div className="px-4 sm:px-6 py-3 border-b border-border bg-bg-secondary/40 flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-2">
            <div className="h-4 w-16 rounded bg-bg-secondary" />
            <div className="h-4 w-32 rounded bg-bg-secondary" />
            <div className="h-4 w-24 rounded bg-bg-secondary" />
          </div>
          <div className="flex items-center gap-2">
            <div className="h-8 w-28 rounded-md bg-bg-secondary" />
            <div className="h-8 w-24 rounded-md bg-bg-secondary" />
            <div className="h-8 w-24 rounded-md bg-bg-secondary" />
          </div>
        </div>

        {/* 2-Column Studio Skeleton */}
        <div className="flex-1 min-h-0 flex flex-col lg:flex-row">
          <aside className="w-full lg:w-80 border-b lg:border-b-0 lg:border-r border-border bg-bg-secondary/30 p-3 space-y-3">
            <div className="flex items-center justify-between">
              <div className="h-4 w-32 rounded bg-bg-secondary" />
              <div className="h-6 w-20 rounded bg-bg-secondary" />
            </div>
            {[1, 2, 3].map((item) => (
              <div key={item} className="rounded-md border border-border-subtle bg-bg-primary p-2.5 space-y-2">
                <div className="h-4 w-3/4 rounded bg-bg-secondary" />
                <div className="pl-4 space-y-1.5">
                  <div className="h-3.5 w-5/6 rounded bg-bg-secondary" />
                  <div className="h-3.5 w-2/3 rounded bg-bg-secondary" />
                </div>
              </div>
            ))}
          </aside>
          <section className="flex-1 min-w-0 p-4 sm:p-6 space-y-4">
            <div className="flex items-center justify-between border-b border-border pb-3">
              <div className="flex items-center gap-2">
                <div className="h-7 w-28 rounded bg-bg-secondary" />
                <div className="h-7 w-24 rounded bg-bg-secondary" />
              </div>
              <div className="h-8 w-28 rounded-md bg-bg-secondary" />
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-4 gap-3">
              <div className="sm:col-span-3 h-9 rounded-md bg-bg-secondary" />
              <div className="h-9 rounded-md bg-bg-secondary" />
            </div>
            <div className="h-9 w-full rounded-md bg-bg-secondary" />
            <div className="h-72 w-full rounded-md bg-bg-secondary" />
          </section>
        </div>
      </div>
    );
  }

  if (!course) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-12 text-center">
        {errorBanner && (
          <p className="text-sm text-red-600 dark:text-red-400 mb-3">{errorBanner}</p>
        )}
        <p className="text-text-secondary mb-4">Không tìm thấy khóa học.</p>
        <Link href="/app/courses" className="text-sm text-accent hover:underline">
          ← Về danh sách khóa học
        </Link>
      </div>
    );
  }

  const isPublic = Boolean(course.isPublished || course.visibility === "public");
  const isProtectedSystemCourse = Boolean(course.isSystemCourse);
  const totalChaptersCount = course.chapters?.length ?? 0;
  const totalLessonsCount = (course.chapters ?? []).reduce(
    (sum, ch) => sum + (ch.lessons?.length ?? 0),
    0
  );
  const hasUnsavedLessonChanges = Boolean(
    selectedLesson &&
      (editTitle !== (selectedLesson.title ?? "") ||
        editContent !==
          (selectedLesson.contentMarkdown ??
            selectedLesson.plainText ??
            selectedLesson.content ??
            "") ||
        editVideoUrl !== (selectedLesson.videoUrl ?? "") ||
        editMinutes !== (selectedLesson.estimatedMinutes ?? 10))
  );

  return (
    <div className="h-full flex flex-col bg-bg-primary">
      {/* Top Bar */}
      <div className="px-4 sm:px-6 py-3 border-b border-border bg-bg-secondary/40 flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2 min-w-0 flex-1">
          <nav aria-label="Breadcrumb" className="flex items-center gap-1.5 text-sm min-w-0">
            <Link
              href="/app/courses"
              className="text-text-tertiary hover:text-text-primary transition shrink-0"
            >
              Khóa học
            </Link>
            <ChevronRight className="w-3.5 h-3.5 text-text-tertiary shrink-0" />
            <Link
              href={`/app/courses/${courseId}`}
              className="text-text-secondary hover:text-text-primary transition truncate max-w-[180px] sm:max-w-[240px]"
            >
              {course.title}
            </Link>
            <ChevronRight className="w-3.5 h-3.5 text-text-tertiary shrink-0" />
            <span className="text-text-primary font-medium shrink-0">Studio Biên tập</span>
          </nav>

          {isEditingCourseTitle ? (
            <div className="flex items-center gap-1.5 ml-2">
              <input
                type="text"
                value={tempCourseTitle}
                onChange={(e) => setTempCourseTitle(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") void handleSaveCourseTitle();
                  if (e.key === "Escape") setIsEditingCourseTitle(false);
                }}
                className="h-7 px-2 rounded border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                autoFocus
              />
              <button
                type="button"
                onClick={handleSaveCourseTitle}
                className="h-7 px-2 rounded bg-accent text-white text-xs font-medium cursor-pointer"
              >
                <Check className="w-3.5 h-3.5" />
              </button>
            </div>
          ) : (
            <button
              type="button"
              onClick={() => {
                setTempCourseTitle(course.title);
                setIsEditingCourseTitle(true);
              }}
              className="p-1 rounded text-text-tertiary hover:text-text-primary hover:bg-bg-tertiary transition cursor-pointer"
              title="Đổi tên khóa học"
            >
              <Pencil className="w-3.5 h-3.5" />
            </button>
          )}

          {isProtectedSystemCourse && (
            <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-blue-500/10 text-blue-600 dark:text-blue-400 border border-blue-500/20 text-[10px] font-semibold">
              <Shield className="w-3 h-3" />
              <span>Khóa học Hệ thống</span>
            </span>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {/* Metadata Button */}
          <button
            type="button"
            onClick={() => setShowMetaPanel((v) => !v)}
            data-testid="toggle-course-metadata-btn"
            className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-md border text-xs font-medium transition cursor-pointer ${
              showMetaPanel
                ? "border-accent bg-accent text-white"
                : "border-border bg-bg-primary text-text-secondary hover:text-text-primary"
            }`}
          >
            <Info className="w-3.5 h-3.5" />
            <span>Thông tin &amp; Metadata</span>
          </button>

          <button
            type="button"
            disabled={isTogglingPublish}
            onClick={handleTogglePublish}
            className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-md border text-xs font-medium transition cursor-pointer ${
              isPublic
                ? "border-emerald-500/40 bg-emerald-500/10 text-emerald-700 dark:text-emerald-300"
                : "border-border bg-bg-primary text-text-secondary hover:text-text-primary"
            }`}
          >
            {isPublic ? <Globe className="w-3.5 h-3.5" /> : <Lock className="w-3.5 h-3.5" />}
            <span>{isPublic ? "Đang công khai" : "Bản nháp"}</span>
          </button>

          <button
            type="button"
            disabled={isExporting}
            onClick={handleExportZip}
            className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border bg-bg-primary text-xs font-medium text-text-primary hover:bg-bg-tertiary transition cursor-pointer"
          >
            <Download className="w-3.5 h-3.5" />
            <span>{isExporting ? "Đang xuất..." : "Xuất ZIP"}</span>
          </button>

          <Link
            href={`/app/courses/${courseId}/learn`}
            className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover transition"
          >
            <Play className="w-3.5 h-3.5" />
            <span>Vào học thử</span>
          </Link>

          {/* Delete Course Button */}
          {!isProtectedSystemCourse && (
            <button
              type="button"
              onClick={() => {
                setDeleteConfirmStep(1);
                setShowDeleteCourseModal(true);
              }}
              data-testid="open-delete-course-modal-btn"
              className="inline-flex items-center gap-1 h-8 px-2.5 rounded-md border border-red-500/30 text-red-600 dark:text-red-400 hover:bg-red-500/10 text-xs font-medium transition cursor-pointer"
              title="Xóa khóa học cá nhân"
            >
              <Trash2 className="w-3.5 h-3.5" />
              <span>Xóa khóa</span>
            </button>
          )}
        </div>
      </div>

      {/* Metadata Collapsible Drawer Panel */}
      {showMetaPanel && (
        <div className="border-b border-border bg-bg-secondary/70 p-4 sm:p-6 animate-in slide-in-from-top-2 duration-150">
          <form onSubmit={handleSaveMetadata} className="max-w-4xl mx-auto space-y-4">
            <div className="flex items-center justify-between gap-2 border-b border-border pb-2">
              <h3 className="text-sm font-semibold text-text-primary flex items-center gap-2">
                <Info className="w-4 h-4 text-accent" />
                <span>Thông tin &amp; Cài đặt khóa học</span>
              </h3>
              <button
                type="button"
                onClick={() => setShowMetaPanel(false)}
                className="p-1 rounded text-text-tertiary hover:text-text-primary cursor-pointer"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
              <div className="sm:col-span-2 space-y-1">
                <label className="text-xs font-medium text-text-secondary">Tiêu đề khóa học</label>
                <input
                  type="text"
                  value={metaTitle}
                  onChange={(e) => setMetaTitle(e.target.value)}
                  className="w-full h-8 px-3 rounded-md border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-medium text-text-secondary">Lĩnh vực (Category)</label>
                <input
                  type="text"
                  value={metaCategory}
                  onChange={(e) => setMetaCategory(e.target.value)}
                  placeholder="Công nghệ, Kinh tế, v.v."
                  className="w-full h-8 px-3 rounded-md border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
              <div className="sm:col-span-2 space-y-1">
                <label className="text-xs font-medium text-text-secondary">Mô tả tổng quan</label>
                <textarea
                  value={metaDescription}
                  onChange={(e) => setMetaDescription(e.target.value)}
                  rows={2}
                  className="w-full p-2 rounded-md border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent resize-none"
                />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-medium text-text-secondary">Cấp độ (Level)</label>
                <select
                  value={metaLevel}
                  onChange={(e) =>
                    setMetaLevel(e.target.value as "foundation" | "intermediate" | "advanced")
                  }
                  className="w-full h-8 px-2 rounded-md border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                >
                  <option value="foundation">Cơ bản (Foundation)</option>
                  <option value="intermediate">Trung cấp (Intermediate)</option>
                  <option value="advanced">Nâng cao (Advanced)</option>
                </select>
              </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-1">
              {/* Learning outcomes */}
              <div className="space-y-1.5">
                <div className="flex items-center justify-between">
                  <label className="text-xs font-medium text-text-secondary">
                    Mục tiêu đầu ra (Learning Outcomes)
                  </label>
                  <button
                    type="button"
                    onClick={() => setMetaOutcomes([...metaOutcomes, ""])}
                    className="text-[11px] text-accent font-medium hover:underline cursor-pointer"
                  >
                    + Thêm mục tiêu
                  </button>
                </div>
                <div className="space-y-1 max-h-32 overflow-y-auto swms-thin-scrollbar pr-1">
                  {metaOutcomes.map((outc, oIdx) => (
                    <div key={oIdx} className="flex items-center gap-1.5">
                      <input
                        type="text"
                        value={outc}
                        onChange={(e) => {
                          const next = [...metaOutcomes];
                          next[oIdx] = e.target.value;
                          setMetaOutcomes(next);
                        }}
                        placeholder={`Mục tiêu ${oIdx + 1}...`}
                        className="flex-1 h-7 px-2 rounded border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                      />
                      <button
                        type="button"
                        onClick={() =>
                          setMetaOutcomes(metaOutcomes.filter((_, idx) => idx !== oIdx))
                        }
                        className="p-1 text-text-tertiary hover:text-red-500 cursor-pointer"
                      >
                        <Trash2 className="w-3 h-3" />
                      </button>
                    </div>
                  ))}
                  {metaOutcomes.length === 0 && (
                    <p className="text-[11px] text-text-tertiary italic">Chưa có mục tiêu đầu ra.</p>
                  )}
                </div>
              </div>

              {/* Prerequisites */}
              <div className="space-y-1.5">
                <div className="flex items-center justify-between">
                  <label className="text-xs font-medium text-text-secondary">
                    Điều kiện tiên quyết (Prerequisites)
                  </label>
                  <button
                    type="button"
                    onClick={() => setMetaPrerequisites([...metaPrerequisites, ""])}
                    className="text-[11px] text-accent font-medium hover:underline cursor-pointer"
                  >
                    + Thêm điều kiện
                  </button>
                </div>
                <div className="space-y-1 max-h-32 overflow-y-auto swms-thin-scrollbar pr-1">
                  {metaPrerequisites.map((prereq, pIdx) => (
                    <div key={pIdx} className="flex items-center gap-1.5">
                      <input
                        type="text"
                        value={prereq}
                        onChange={(e) => {
                          const next = [...metaPrerequisites];
                          next[pIdx] = e.target.value;
                          setMetaPrerequisites(next);
                        }}
                        placeholder={`Điều kiện ${pIdx + 1}...`}
                        className="flex-1 h-7 px-2 rounded border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                      />
                      <button
                        type="button"
                        onClick={() =>
                          setMetaPrerequisites(metaPrerequisites.filter((_, idx) => idx !== pIdx))
                        }
                        className="p-1 text-text-tertiary hover:text-red-500 cursor-pointer"
                      >
                        <Trash2 className="w-3 h-3" />
                      </button>
                    </div>
                  ))}
                  {metaPrerequisites.length === 0 && (
                    <p className="text-[11px] text-text-tertiary italic">Chưa có điều kiện tiên quyết.</p>
                  )}
                </div>
              </div>
            </div>

            <div className="flex items-center justify-end gap-2 pt-2 border-t border-border">
              <button
                type="button"
                onClick={() => setShowMetaPanel(false)}
                className="px-3 py-1.5 rounded-md text-xs font-medium text-text-secondary hover:text-text-primary border border-border bg-bg-primary cursor-pointer"
              >
                Đóng
              </button>
              <button
                type="submit"
                disabled={isSavingMeta}
                className="px-4 py-1.5 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover transition cursor-pointer flex items-center gap-1.5"
              >
                {isSavingMeta ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Save className="w-3.5 h-3.5" />}
                <span>Lưu thay đổi metadata</span>
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Banners */}
      {(errorBanner || saveMessage) && (
        <div className="px-4 sm:px-6 pt-3">
          {errorBanner && (
            <div className="rounded-md border border-red-500/30 bg-red-500/5 px-3 py-2 text-xs text-red-600 dark:text-red-400 flex items-center justify-between">
              <div className="flex items-center gap-2">
                <AlertCircle className="w-3.5 h-3.5 shrink-0" />
                <span>{errorBanner}</span>
              </div>
              <button
                type="button"
                onClick={() => setErrorBanner(null)}
                className="p-0.5 text-red-600 dark:text-red-400 hover:opacity-80"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            </div>
          )}
          {saveMessage && !errorBanner && (
            <div className="rounded-md border border-emerald-500/30 bg-emerald-500/10 px-3 py-2 text-xs text-emerald-700 dark:text-emerald-300 flex items-center justify-between">
              <span>{saveMessage}</span>
              <button
                type="button"
                onClick={() => setSaveMessage(null)}
                className="p-0.5 text-emerald-700 dark:text-emerald-300 hover:opacity-80"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            </div>
          )}
        </div>
      )}

      {/* Main Split Editor */}
      <div className="flex-1 min-h-0 flex flex-col lg:flex-row">
        {/* Left: Curriculum Tree */}
        <aside className="w-full lg:w-80 border-b lg:border-b-0 lg:border-r border-border bg-bg-secondary/30 flex flex-col min-h-0 max-h-80 lg:max-h-none">
          <div className="px-3 py-2.5 border-b border-border flex items-center justify-between gap-2">
            <span className="text-xs font-semibold uppercase tracking-wider text-text-tertiary">
              Cấu trúc khóa học
            </span>
            <div className="flex items-center gap-2">
              <button
                type="button"
                disabled={isSavingOrder}
                onClick={() => void handleSaveOrder()}
                data-testid="save-course-order-btn"
                className={`inline-flex items-center gap-1 h-6 px-2 rounded text-[11px] font-semibold transition cursor-pointer ${
                  hasUnsavedOrder
                    ? "bg-accent text-white shadow-2xs"
                    : "border border-border bg-bg-primary text-text-secondary hover:text-text-primary"
                }`}
                title="Lưu thứ tự chương và bài học"
              >
                <Save className="w-3 h-3" />
                <span>{isSavingOrder ? "Đang lưu..." : "Lưu thứ tự"}</span>
              </button>
              <button
                type="button"
                onClick={() => setShowNewChapterInput((v) => !v)}
                className="inline-flex items-center gap-1 text-xs font-medium text-accent hover:underline cursor-pointer"
              >
                <Plus className="w-3.5 h-3.5" />
                <span>Thêm chương</span>
              </button>
            </div>
          </div>

          {showNewChapterInput && (
            <form
              onSubmit={handleCreateChapter}
              className="p-2.5 border-b border-border bg-bg-primary flex items-center gap-1.5"
            >
              <input
                type="text"
                value={newChapterTitle}
                onChange={(e) => setNewChapterTitle(e.target.value)}
                placeholder="Tên chương mới..."
                className="flex-1 h-7 px-2 rounded border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                autoFocus
              />
              <button
                type="submit"
                disabled={creatingChapter}
                className="h-7 px-2.5 rounded bg-accent text-white text-xs font-medium cursor-pointer"
              >
                {creatingChapter ? "..." : "Thêm"}
              </button>
            </form>
          )}

          <div className="flex-1 overflow-y-auto swms-thin-scrollbar p-2 space-y-1.5">
            {course.chapters.map((chapter, chIdx) => {
              const chId = chapter.chapterId || chapter.id;
              const isOpen = expandedChapters.has(chId);

              return (
                <div key={chId} className="rounded-md border border-border-subtle bg-bg-primary">
                  <div className="flex items-center justify-between gap-1 px-2 py-2">
                    <button
                      type="button"
                      onClick={() => toggleChapter(chId)}
                      className="flex items-center gap-1.5 min-w-0 flex-1 text-left cursor-pointer"
                    >
                      {isOpen ? (
                        <ChevronDown className="w-3.5 h-3.5 text-text-tertiary shrink-0" />
                      ) : (
                        <ChevronRight className="w-3.5 h-3.5 text-text-tertiary shrink-0" />
                      )}
                      <span className="text-xs font-semibold text-text-primary truncate">
                        {chIdx + 1}. {chapter.title}
                      </span>
                    </button>

                    <div className="flex items-center gap-0.5 shrink-0">
                      <button
                        type="button"
                        disabled={chIdx === 0}
                        onClick={() => handleMoveChapter(chIdx, -1)}
                        className="p-1 rounded text-text-tertiary hover:text-text-primary hover:bg-bg-secondary disabled:opacity-30 cursor-pointer"
                        title="Di chuyển chương lên"
                        aria-label="Di chuyển chương lên"
                      >
                        <ArrowUp className="w-3 h-3" />
                      </button>
                      <button
                        type="button"
                        disabled={chIdx === course.chapters.length - 1}
                        onClick={() => handleMoveChapter(chIdx, 1)}
                        className="p-1 rounded text-text-tertiary hover:text-text-primary hover:bg-bg-secondary disabled:opacity-30 cursor-pointer"
                        title="Di chuyển chương xuống"
                        aria-label="Di chuyển chương xuống"
                      >
                        <ArrowDown className="w-3 h-3" />
                      </button>
                      <button
                        type="button"
                        onClick={() =>
                          setAddingLessonChapterId((curr) => (curr === chId ? null : chId))
                        }
                        className="p-1 rounded text-text-tertiary hover:text-accent hover:bg-bg-secondary cursor-pointer"
                        title="Thêm bài học"
                      >
                        <Plus className="w-3.5 h-3.5" />
                      </button>

                      {confirmDeleteChapterId === chId ? (
                        <button
                          type="button"
                          onClick={() => handleDeleteChapter(chId)}
                          className="px-1.5 py-0.5 rounded bg-red-600 text-white text-[10px] font-medium cursor-pointer"
                        >
                          Xóa?
                        </button>
                      ) : (
                        <button
                          type="button"
                          onClick={() => setConfirmDeleteChapterId(chId)}
                          className="p-1 rounded text-text-tertiary hover:text-red-500 hover:bg-bg-secondary cursor-pointer"
                          title="Xóa chương"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      )}
                    </div>
                  </div>

                  {addingLessonChapterId === chId && (
                    <form
                      onSubmit={(e) => handleCreateLesson(chId, e)}
                      className="px-2 pb-2 flex items-center gap-1.5"
                    >
                      <input
                        type="text"
                        value={newLessonTitle}
                        onChange={(e) => setNewLessonTitle(e.target.value)}
                        placeholder="Tên bài học..."
                        className="flex-1 h-7 px-2 rounded border border-border bg-bg-secondary text-xs text-text-primary focus:outline-none focus:border-accent"
                        autoFocus
                      />
                      <button
                        type="submit"
                        disabled={creatingLesson}
                        className="h-7 px-2 rounded bg-accent text-white text-xs font-medium cursor-pointer"
                      >
                        {creatingLesson ? "..." : "Tạo"}
                      </button>
                    </form>
                  )}

                  {isOpen && (
                    <div className="border-t border-border-subtle divide-y divide-border-subtle">
                      {(chapter.lessons ?? []).map((lesson, lIdx) => {
                        const lId = lesson.lessonId || lesson.id;
                        const active =
                          selectedLesson && (selectedLesson.lessonId || selectedLesson.id) === lId;
                        const lessonCountInChapter = (chapter.lessons ?? []).length;
                        return (
                          <div
                            key={lId}
                            draggable
                            onDragStart={() =>
                              setDraggedLessonInfo({ chapterId: chId, lessonIndex: lIdx })
                            }
                            onDragOver={(e) => e.preventDefault()}
                            onDrop={() => handleDropLesson(chId, lIdx)}
                            className={`flex items-center justify-between gap-1 px-2 py-2 ${
                              active ? "bg-accent-subtle" : "hover:bg-bg-secondary"
                            }`}
                          >
                            <span
                              className="text-text-tertiary cursor-grab active:cursor-grabbing shrink-0"
                              title="Kéo thả để đổi thứ tự bài học"
                            >
                              <GripVertical className="w-3 h-3" />
                            </span>
                            <button
                              type="button"
                              onClick={() => void selectLesson(lesson)}
                              className={`flex items-center gap-1.5 min-w-0 flex-1 text-left text-xs cursor-pointer ${
                                active ? "text-accent font-medium" : "text-text-secondary"
                              }`}
                            >
                              <FileText className="w-3.5 h-3.5 shrink-0 opacity-70" />
                              <span className="truncate">{lesson.title}</span>
                            </button>

                            <div className="flex items-center gap-0.5 shrink-0">
                              <button
                                type="button"
                                disabled={lIdx === 0}
                                onClick={() => handleMoveLesson(chId, lIdx, -1)}
                                className="p-0.5 rounded text-text-tertiary hover:text-text-primary disabled:opacity-30 cursor-pointer"
                                title="Đưa bài học lên"
                                aria-label="Đưa bài học lên"
                              >
                                <ArrowUp className="w-3 h-3" />
                              </button>
                              <button
                                type="button"
                                disabled={lIdx === lessonCountInChapter - 1}
                                onClick={() => handleMoveLesson(chId, lIdx, 1)}
                                className="p-0.5 rounded text-text-tertiary hover:text-text-primary disabled:opacity-30 cursor-pointer"
                                title="Đưa bài học xuống"
                                aria-label="Đưa bài học xuống"
                              >
                                <ArrowDown className="w-3 h-3" />
                              </button>

                              {confirmDeleteLessonId === lId ? (
                                <button
                                  type="button"
                                  onClick={() => handleDeleteLesson(chId, lId)}
                                  className="px-1.5 py-0.5 rounded bg-red-600 text-white text-[10px] font-medium cursor-pointer shrink-0"
                                >
                                  Xóa?
                                </button>
                              ) : (
                                <button
                                  type="button"
                                  onClick={() => setConfirmDeleteLessonId(lId)}
                                  className="p-1 rounded text-text-tertiary hover:text-red-500 cursor-pointer shrink-0"
                                  title="Xóa bài học"
                                >
                                  <Trash2 className="w-3 h-3" />
                                </button>
                              )}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </aside>

        {/* Right: Selected Lesson Editor & AI Lesson Assistant */}
        <section className="flex-1 min-w-0 flex flex-col min-h-0">
          {selectedLesson ? (
            <>
              {/* Editor Sub-header */}
              <div className="px-4 sm:px-6 py-2.5 border-b border-border flex flex-wrap items-center justify-between gap-3 bg-bg-secondary/20">
                <div className="flex items-center gap-2">
                  <div className="flex items-center gap-1 rounded-md bg-bg-secondary p-0.5 border border-border">
                    <button
                      type="button"
                      onClick={() => setEditorTab("write")}
                      className={`h-7 px-3 rounded text-xs font-medium transition cursor-pointer ${
                        editorTab === "write"
                          ? "bg-bg-primary text-text-primary shadow-sm"
                          : "text-text-secondary hover:text-text-primary"
                      }`}
                    >
                      Soạn nội dung
                    </button>
                    <button
                      type="button"
                      onClick={() => setEditorTab("preview")}
                      className={`inline-flex items-center gap-1 h-7 px-3 rounded text-xs font-medium transition cursor-pointer ${
                        editorTab === "preview"
                          ? "bg-bg-primary text-text-primary shadow-sm"
                          : "text-text-secondary hover:text-text-primary"
                      }`}
                    >
                      <Eye className="w-3.5 h-3.5" />
                      Xem trước
                    </button>
                  </div>

                  {/* AI Assistant Toggle Button */}
                  <button
                    type="button"
                    onClick={() => setShowAiAssistant((v) => !v)}
                    data-testid="toggle-ai-lesson-assistant-btn"
                    className={`inline-flex items-center gap-1.5 h-7 px-2.5 rounded-md border text-xs font-medium transition cursor-pointer ${
                      showAiAssistant
                        ? "border-accent bg-accent text-white shadow-xs"
                        : "border-border bg-bg-primary text-accent hover:border-accent/40"
                    }`}
                  >
                    <Sparkles className="w-3.5 h-3.5" />
                    <span>Trợ lý AI bài giảng</span>
                  </button>
                </div>

                <div className="flex flex-wrap items-center gap-2">
                  {hasUnsavedLessonChanges && (
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full bg-amber-500/15 text-amber-700 dark:text-amber-300 border border-amber-500/30 text-[11px] font-medium">
                      Chưa lưu thay đổi
                    </span>
                  )}
                  <button
                    type="button"
                    disabled={isSaving}
                    onClick={handleSave}
                    className="inline-flex items-center gap-1.5 h-8 px-4 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover disabled:opacity-50 transition cursor-pointer"
                  >
                    <Save className="w-3.5 h-3.5" />
                    <span>{isSaving ? "Đang lưu..." : "Lưu bài học"}</span>
                  </button>
                </div>
              </div>

              {/* AI Lesson Assistant Panel */}
              {showAiAssistant && (
                <div className="border-b border-accent/30 bg-accent-subtle/25 p-3.5 sm:p-4 space-y-3 animate-in slide-in-from-top-1 duration-150">
                  <div className="flex items-center justify-between gap-2">
                    <div className="flex items-center gap-1.5 text-xs font-semibold text-accent">
                      <Sparkles className="w-4 h-4" />
                      <span>Trợ lý AI bài giảng (Nâng cao nội dung 1-click)</span>
                    </div>
                    <button
                      type="button"
                      onClick={() => setShowAiAssistant(false)}
                      className="p-1 rounded text-text-tertiary hover:text-text-primary cursor-pointer"
                    >
                      <X className="w-3.5 h-3.5" />
                    </button>
                  </div>

                  {/* 4 Actions */}
                  <div className="flex flex-wrap items-center gap-1.5">
                    {[
                      { key: "expand", label: "Mở rộng bài học" },
                      { key: "summarize", label: "Tóm tắt trọng tâm" },
                      { key: "examples", label: "Thêm ví dụ thực tế" },
                      { key: "exercise", label: "Tạo bài tập tự luyện" },
                    ].map((item) => (
                      <button
                        key={item.key}
                        type="button"
                        onClick={() => {
                          const action = item.key as typeof aiAction;
                          setAiAction(action);
                          void handleRunAiEnhance(action);
                        }}
                        disabled={isAiEnhancing}
                        className={`h-7 px-2.5 rounded-md text-xs font-medium border transition cursor-pointer flex items-center gap-1 ${
                          aiAction === item.key
                            ? "bg-accent text-white border-accent shadow-xs"
                            : "bg-bg-primary text-text-secondary border-border hover:border-accent/40"
                        }`}
                      >
                        <Wand2 className="w-3 h-3" />
                        <span>{item.label}</span>
                      </button>
                    ))}
                  </div>

                  {/* Custom Prompt & Trigger */}
                  <div className="flex items-center gap-2">
                    <input
                      type="text"
                      value={aiPrompt}
                      onChange={(e) => setAiPrompt(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === "Enter" && !isAiEnhancing) void handleRunAiEnhance();
                      }}
                      placeholder="Yêu cầu tùy chỉnh (ví dụ: 'thêm case study thực tế tại Việt Nam', 'viết ngắn gọn 3 gạch đầu dòng')..."
                      disabled={isAiEnhancing}
                      className="flex-1 h-8 px-3 rounded-md border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                    />
                    <button
                      type="button"
                      disabled={isAiEnhancing}
                      onClick={() => void handleRunAiEnhance()}
                      className="h-8 px-3 rounded-md bg-accent text-white text-xs font-semibold hover:bg-accent-hover disabled:opacity-50 transition cursor-pointer flex items-center gap-1 shrink-0"
                    >
                      {isAiEnhancing ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Sparkles className="w-3.5 h-3.5" />}
                      <span>{isAiEnhancing ? "Đang xử lý..." : "Thực hiện"}</span>
                    </button>
                  </div>

                  {/* AI Result Preview Box */}
                  {aiResult && (
                    <div className="mt-2 p-3 rounded-lg border border-accent/40 bg-bg-primary space-y-2.5 animate-in fade-in duration-150">
                      <div className="flex flex-wrap items-center justify-between gap-2 text-xs">
                        <div className="flex items-center gap-2">
                          <span className="font-semibold text-text-primary">Kết quả gợi ý từ AI:</span>
                          <div className="inline-flex items-center rounded-md bg-bg-secondary p-0.5 border border-border">
                            <button
                              type="button"
                              onClick={() => setAiResultTab("preview")}
                              className={`px-2 py-0.5 rounded text-[11px] font-medium transition cursor-pointer ${
                                aiResultTab === "preview"
                                  ? "bg-bg-primary text-text-primary shadow-2xs"
                                  : "text-text-secondary hover:text-text-primary"
                              }`}
                            >
                              Xem trước
                            </button>
                            <button
                              type="button"
                              onClick={() => setAiResultTab("markdown")}
                              className={`px-2 py-0.5 rounded text-[11px] font-medium transition cursor-pointer ${
                                aiResultTab === "markdown"
                                  ? "bg-bg-primary text-text-primary shadow-2xs"
                                  : "text-text-secondary hover:text-text-primary"
                              }`}
                            >
                              Mã Markdown
                            </button>
                          </div>
                        </div>
                        <div className="flex items-center gap-1.5">
                          {aiResult.model && (
                            <span className="text-[10px] px-1.5 py-0.5 rounded bg-bg-secondary text-text-tertiary">
                              {aiResult.model}
                            </span>
                          )}
                          {aiResult.usedLiveAi && (
                            <span className="text-[10px] px-1.5 py-0.5 rounded bg-emerald-500/15 text-emerald-600 font-semibold">
                              Live AI
                            </span>
                          )}
                        </div>
                      </div>

                      {aiResultTab === "preview" ? (
                        <div className="max-h-60 overflow-y-auto swms-thin-scrollbar p-3 rounded bg-bg-secondary/30 border border-border/60">
                          <LessonMarkdownView
                            markdown={aiResult.content}
                            lessonTitle={editTitle}
                            anchoredAnnotations={[]}
                            onSelectBlockText={() => {}}
                            onHighlightClick={() => {}}
                            onInsertCitation={() => {}}
                            hideInlineCitationBadges={true}
                          />
                        </div>
                      ) : (
                        <div className="max-h-60 overflow-y-auto swms-thin-scrollbar p-2.5 rounded bg-bg-secondary/40 font-mono text-xs text-text-primary whitespace-pre-wrap leading-relaxed border border-border/60">
                          {aiResult.content}
                        </div>
                      )}

                      <div className="flex flex-wrap items-center justify-end gap-2 pt-1">
                        <button
                          type="button"
                          onClick={() => setAiResult(null)}
                          className="px-2.5 py-1 rounded text-xs text-text-tertiary hover:text-text-primary cursor-pointer"
                        >
                          Bỏ qua
                        </button>
                        <button
                          type="button"
                          onClick={() => void handleCopyAiResult()}
                          className="inline-flex items-center gap-1 px-3 py-1 rounded-md border border-border bg-bg-secondary text-text-primary text-xs font-medium hover:bg-bg-tertiary transition cursor-pointer"
                        >
                          {aiCopied ? <Check className="w-3.5 h-3.5 text-emerald-500" /> : <Copy className="w-3.5 h-3.5" />}
                          <span>{aiCopied ? "Đã sao chép" : "Sao chép"}</span>
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            setEditContent((prev) => `${prev.trimEnd()}\n\n${aiResult.content}`);
                            setAiResult(null);
                            setSaveMessage("Đã chèn nội dung AI vào cuối bài. Bấm 'Lưu bài học' để cập nhật!");
                          }}
                          className="px-3 py-1 rounded-md border border-accent/40 bg-accent-subtle text-accent text-xs font-medium hover:bg-accent hover:text-white transition cursor-pointer"
                        >
                          Chèn vào cuối bài
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            setEditContent(aiResult.content);
                            setAiResult(null);
                            setSaveMessage("Đã thay thế toàn bộ nội dung bài bằng gợi ý AI. Bấm 'Lưu bài học' để commit!");
                          }}
                          className="px-3 py-1 rounded-md bg-accent text-white text-xs font-semibold hover:bg-accent-hover transition cursor-pointer"
                        >
                          Thay thế nội dung bài
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              )}

              <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-4">
                <div className="grid grid-cols-1 sm:grid-cols-4 gap-3">
                  <div className="sm:col-span-3">
                    <label className="block text-xs font-medium text-text-secondary mb-1">
                      Tiêu đề bài giảng
                    </label>
                    <input
                      type="text"
                      value={editTitle}
                      onChange={(e) => setEditTitle(e.target.value)}
                      className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-sm text-text-primary focus:outline-none focus:border-accent"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-text-secondary mb-1">
                      Thời lượng (phút)
                    </label>
                    <input
                      type="number"
                      min={1}
                      max={300}
                      value={editMinutes}
                      onChange={(e) => setEditMinutes(Number(e.target.value) || 10)}
                      className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-sm text-text-primary focus:outline-none focus:border-accent"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-medium text-text-secondary mb-1">
                    Liên kết Video bài giảng (tùy chọn)
                  </label>
                  <input
                    type="url"
                    value={editVideoUrl}
                    onChange={(e) => setEditVideoUrl(e.target.value)}
                    placeholder="https://www.youtube.com/watch?v=... hoặc URL tệp MP4"
                    className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-accent"
                  />
                </div>

                {editorTab === "write" ? (
                  <div className="space-y-2">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <label className="block text-xs font-medium text-text-secondary">
                        Nội dung bài học (Markdown)
                      </label>
                      <div className="flex flex-wrap items-center gap-1.5" data-testid="edit-practice-inserters">
                        <span className="text-[11px] text-text-tertiary mr-0.5">Chèn bài tập:</span>
                        {(
                          [
                            {
                              label: "+ Nến (Tài chính)",
                              snippet: `\n\n\`\`\`practice-finance\n{\n  "title": "Thực hành đọc nến & đặt lệnh theo bài",\n  "ticker": "VN30 / BTC",\n  "timeframe": "D1",\n  "context": "Giá hồi về vùng hỗ trợ cứng kèm nến rút chân.",\n  "supportZone": [98, 100],\n  "resistanceZone": [111, 114],\n  "defaultEntry": 101.5,\n  "defaultSl": 97,\n  "defaultTp": 111.5,\n  "bestAction": "long",\n  "explanation": "Nến rút chân tại hỗ trợ xác nhận lực cầu bắt đáy.",\n  "initialCandles": [\n    { "label": "1", "o": 108, "h": 109, "l": 105, "c": 106, "v": 45 },\n    { "label": "2", "o": 106, "h": 107, "l": 102, "c": 103, "v": 52 },\n    { "label": "3", "o": 103, "h": 104, "l": 99, "c": 100, "v": 65 },\n    { "label": "4", "o": 100, "h": 102.5, "l": 97.5, "c": 101.5, "v": 90 }\n  ],\n  "futureCandles": [\n    { "label": "5", "o": 101.5, "h": 105, "l": 101, "c": 104.5, "v": 82 },\n    { "label": "6", "o": 104.5, "h": 109, "l": 104, "c": 108.5, "v": 88 },\n    { "label": "7", "o": 108.5, "h": 112.5, "l": 108, "c": 111.8, "v": 92 }\n  ]\n}\n\`\`\`\n`,
                            },
                            {
                              label: "+ Nghe - Nói (Anh)",
                              snippet: `\n\n\`\`\`practice-english\n{\n  "topic": "Bài luyện Nghe & Nói theo giáo trình",\n  "level": "Band 6.5+",\n  "sentence": "Consistent practice builds confidence when expressing complex ideas clearly.",\n  "phoneticHint": "/kənˈsɪstənt ˈpræktɪs bɪldz ˈkɒnfɪdəns/",\n  "meaningVi": "Luyện tập đều đặn giúp xây dựng sự tự tin khi diễn đạt các ý tưởng phức tạp.",\n  "speakingPrompt": "Explain one practical example related to this lesson in 60 seconds.",\n  "sampleAnswer": "In my experience, breaking a broad goal into short daily sessions keeps motivation steady."\n}\n\`\`\`\n`,
                            },
                            {
                              label: "+ Thực hành Code",
                              snippet: `\n\n\`\`\`practice-code\n{\n  "title": "Bài tập lập trình vận dụng",\n  "lang": "typescript",\n  "prompt": "Hoàn thiện hàm xử lý dữ liệu theo yêu cầu của bài học.",\n  "starterCode": "export function solve(input: number[]): number {\\n  return 0;\\n}",\n  "solutionCode": "export function solve(input: number[]): number {\\n  return input.filter((n) => n > 0).reduce((a, b) => a + b, 0);\\n}",\n  "expectedKeywords": ["filter", "reduce"]\n}\n\`\`\`\n`,
                            },
                            {
                              label: "+ Tình huống",
                              snippet: `\n\n\`\`\`practice-case\n{\n  "title": "Tình huống vận dụng thực tế",\n  "scenario": "Đặt giả định áp dụng kiến thức bài này để xử lý một vấn đề thực tế và giải thích lý do lựa chọn.",\n  "criteria": ["Nêu được nguyên tắc chính", "Chỉ ra rủi ro/giới hạn"]\n}\n\`\`\`\n`,
                            },
                          ] as const
                        ).map((preset) => (
                          <button
                            key={preset.label}
                            type="button"
                            onClick={() =>
                              setEditContent((prev) => `${prev.trimEnd()}${preset.snippet}`)
                            }
                            className="h-7 px-2.5 rounded border border-border bg-bg-secondary text-[11px] font-medium text-text-secondary hover:text-accent hover:border-accent/50 transition cursor-pointer"
                          >
                            {preset.label}
                          </button>
                        ))}
                      </div>
                    </div>
                    <textarea
                      value={editContent}
                      onChange={(e) => setEditContent(e.target.value)}
                      rows={18}
                      className="w-full p-3 rounded-md border border-border bg-bg-primary font-mono text-xs leading-relaxed text-text-primary focus:outline-none focus:border-accent resize-y"
                      placeholder="# Tiêu đề bài học..."
                    />
                  </div>
                ) : (
                  <div className="rounded-lg border border-border bg-bg-primary p-5">
                    <LessonMarkdownView
                      markdown={editContent}
                      lessonTitle={editTitle}
                      anchoredAnnotations={[]}
                      onSelectBlockText={() => {}}
                      onHighlightClick={() => {}}
                      onInsertCitation={() => {}}
                      hideInlineCitationBadges={true}
                    />
                  </div>
                )}
              </div>
            </>
          ) : (
            <div className="flex-1 flex flex-col items-center justify-center p-8 text-center">
              <BookOpen className="w-8 h-8 text-text-tertiary mb-3" />
              <p className="text-sm font-medium text-text-primary mb-1">
                Chọn một bài học bên trái để bắt đầu chỉnh sửa
              </p>
              <p className="text-xs text-text-tertiary">
                Bạn có thể thêm chương, thêm bài mới, sử dụng Trợ lý AI và xem trước nội dung trực tiếp.
              </p>
            </div>
          )}
        </section>
      </div>

      {/* Delete Course 2-Step Confirmation Modal */}
      {showDeleteCourseModal && (
        <div
          role="dialog"
          aria-modal="true"
          className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-xs animate-in fade-in duration-150"
        >
          <div className="w-full max-w-md bg-bg-primary rounded-xl border border-red-500/40 p-5 space-y-4 shadow-2xl">
            {deleteConfirmStep === 1 ? (
              <>
                <div className="flex items-start gap-3">
                  <div className="w-10 h-10 rounded-full bg-red-500/10 text-red-600 dark:text-red-400 flex items-center justify-center shrink-0">
                    <AlertCircle className="w-5 h-5" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between gap-2">
                      <h3 className="text-base font-semibold text-text-primary">
                        Xóa khóa học? (Bước 1/2)
                      </h3>
                    </div>
                    <p className="text-xs text-text-secondary mt-1 leading-relaxed">
                      Bạn sắp xóa khóa học{" "}
                      <strong className="text-text-primary font-semibold">"{course.title}"</strong>.
                    </p>
                  </div>
                </div>

                <div className="p-3 rounded-lg bg-bg-secondary/60 border border-border text-xs text-text-secondary space-y-1">
                  <div className="flex items-center justify-between">
                    <span>Số chương sẽ bị xóa:</span>
                    <span className="font-semibold text-text-primary">{totalChaptersCount} chương</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span>Số bài học sẽ bị xóa:</span>
                    <span className="font-semibold text-text-primary">{totalLessonsCount} bài học</span>
                  </div>
                </div>

                <div className="flex flex-wrap items-center justify-end gap-2.5 pt-1">
                  <button
                    type="button"
                    disabled={isDeletingCourse}
                    onClick={() => {
                      setShowDeleteCourseModal(false);
                      setDeleteConfirmStep(1);
                    }}
                    className="px-3.5 py-1.5 rounded-md border border-border bg-bg-secondary text-xs font-medium text-text-primary hover:bg-bg-tertiary transition cursor-pointer"
                  >
                    Hủy bỏ
                  </button>
                  <button
                    type="button"
                    onClick={() => setDeleteConfirmStep(2)}
                    className="px-4 py-1.5 rounded-md bg-amber-600 text-white text-xs font-semibold hover:bg-amber-700 transition cursor-pointer"
                  >
                    Tôi hiểu rủi ro, tiếp tục
                  </button>
                </div>
              </>
            ) : (
              <>
                <div className="flex items-start gap-3">
                  <div className="w-10 h-10 rounded-full bg-red-500/15 text-red-600 dark:text-red-400 flex items-center justify-center shrink-0">
                    <Trash2 className="w-5 h-5" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <h3 className="text-base font-semibold text-text-primary">
                      Xác nhận xóa vĩnh viễn? (Bước 2/2)
                    </h3>
                    <p className="text-xs text-text-secondary mt-1 leading-relaxed">
                      Khóa học{" "}
                      <strong className="text-text-primary font-semibold">"{course.title}"</strong> cùng{" "}
                      {totalChaptersCount} chương và {totalLessonsCount} bài học sẽ bị xóa hoàn toàn.
                    </p>
                  </div>
                </div>

                <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/30 text-xs text-red-600 dark:text-red-400 font-semibold">
                  Hành động này không thể hoàn tác!
                </div>

                <div className="flex flex-wrap items-center justify-end gap-2.5 pt-1">
                  <button
                    type="button"
                    disabled={isDeletingCourse}
                    onClick={() => setDeleteConfirmStep(1)}
                    className="px-3.5 py-1.5 rounded-md border border-border bg-bg-secondary text-xs font-medium text-text-primary hover:bg-bg-tertiary transition cursor-pointer"
                  >
                    Quay lại
                  </button>
                  <button
                    type="button"
                    disabled={isDeletingCourse}
                    onClick={handleDeleteCourse}
                    data-testid="confirm-delete-course-btn"
                    className="px-4 py-1.5 rounded-md bg-red-600 text-white text-xs font-semibold hover:bg-red-700 disabled:opacity-50 transition cursor-pointer flex items-center gap-1.5"
                  >
                    {isDeletingCourse ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Trash2 className="w-3.5 h-3.5" />}
                    <span>{isDeletingCourse ? "Đang xóa..." : "Xác nhận xóa vĩnh viễn"}</span>
                  </button>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
