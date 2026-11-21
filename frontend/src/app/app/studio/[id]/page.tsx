"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import {
  ArrowLeft,
  BookOpen,
  ChevronDown,
  ChevronRight,
  Download,
  Eye,
  FileText,
  Globe,
  Lock,
  Plus,
  Save,
  Trash2,
  AlertCircle,
  Check,
  Pencil,
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
  category?: string;
  visibility?: string;
  isPublished?: boolean;
  chapters: Chapter[];
}

export default function EditCoursePage() {
  const params = useParams();
  const courseId = params.id as string;
  const { activeUserHeaderId } = useWorkspaceStore();

  const [course, setCourse] = useState<CourseDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [expandedChapters, setExpandedChapters] = useState<Set<string>>(new Set());
  const [selectedLesson, setSelectedLesson] = useState<Lesson | null>(null);

  const [editTitle, setEditTitle] = useState("");
  const [editContent, setEditContent] = useState("");
  const [editVideoUrl, setEditVideoUrl] = useState("");
  const [editMinutes, setEditMinutes] = useState(10);
  const [editorTab, setEditorTab] = useState<"write" | "preview">("write");

  const [isSaving, setIsSaving] = useState(false);
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [errorBanner, setErrorBanner] = useState<string | null>(null);

  // Course metadata controls
  const [isEditingCourseTitle, setIsEditingCourseTitle] = useState(false);
  const [tempCourseTitle, setTempCourseTitle] = useState("");
  const [isTogglingPublish, setIsTogglingPublish] = useState(false);
  const [isExporting, setIsExporting] = useState(false);

  // Inline create/delete states (no native alert/prompt/confirm blocking)
  const [showNewChapterInput, setShowNewChapterInput] = useState(false);
  const [newChapterTitle, setNewChapterTitle] = useState("");
  const [creatingChapter, setCreatingChapter] = useState(false);

  const [addingLessonChapterId, setAddingLessonChapterId] = useState<string | null>(null);
  const [newLessonTitle, setNewLessonTitle] = useState("");
  const [creatingLesson, setCreatingLesson] = useState(false);

  const [confirmDeleteChapterId, setConfirmDeleteChapterId] = useState<string | null>(null);
  const [confirmDeleteLessonId, setConfirmDeleteLessonId] = useState<string | null>(null);

  const getAuthHeaders = useCallback(
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

      if (!initialContent && lId) {
        try {
          const resp = await fetch(`/api/v1/lessons/${lId}`, { headers: getAuthHeaders() });
          if (resp.ok) {
            const detail = await resp.json();
            const fetchedMd = detail.contentMarkdown ?? detail.plainText ?? "";
            if (fetchedMd) setEditContent(fetchedMd);
            if (typeof detail.videoUrl === "string") setEditVideoUrl(detail.videoUrl);
            if (typeof detail.estimatedMinutes === "number") setEditMinutes(detail.estimatedMinutes);
          }
        } catch {
          // Keep current state cleanly
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
      if (next.has(chapterId)) {
        next.delete(chapterId);
      } else {
        next.add(chapterId);
      }
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
      setIsEditingCourseTitle(false);
      setSaveMessage("Đã cập nhật tên khóa học.");
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi kết nối khi đổi tên khóa học.");
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
        setErrorBanner(`Không thể thay đổi trạng thái xuất bản (${resp.status})${errText ? `: ${errText}` : ""}`);
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
          ? "Khóa học đã được xuất bản."
          : "Đã chuyển khóa học về bản nháp cá nhân."
      );
      window.dispatchEvent(new CustomEvent("swms:data-mutated"));
    } catch (err) {
      setErrorBanner(err instanceof Error ? err.message : "Lỗi kết nối khi cập nhật trạng thái xuất bản.");
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
        setErrorBanner(
          `Lưu bài học thất bại (${resp.status})${errText ? `: ${errText}` : ""}`
        );
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
      setCourse((prev) =>
        prev ? { ...prev, chapters: [...prev.chapters, created] } : prev
      );
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
      if (
        selectedLId &&
        deletedChapter?.lessons?.some((l) => (l.lessonId || l.id) === selectedLId)
      ) {
        setSelectedLesson(null);
        setEditTitle("");
        setEditContent("");
      }
      setCourse((prev) =>
        prev
          ? {
              ...prev,
              chapters: prev.chapters.filter((c) => (c.chapterId || c.id) !== chapterId),
            }
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
                  ? {
                      ...c,
                      lessons: c.lessons.filter((l) => (l.lessonId || l.id) !== lessonId),
                    }
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

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (!course) {
    return (
      <div className="flex flex-col items-center justify-center h-full text-center">
        <p className="text-text-secondary text-sm">Không tìm thấy khóa học.</p>
        <Link href="/app/studio" className="text-accent text-sm mt-2 hover:underline">
          ← Quay lại Studio
        </Link>
      </div>
    );
  }

  return (
    <div data-testid="v2-studio-editor" className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh]">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-3 min-w-0 flex-1">
          <Link
            href="/app/studio"
            className="flex items-center justify-center w-8 h-8 rounded-md hover:bg-bg-tertiary transition text-text-secondary shrink-0"
            title="Quay lại Studio"
          >
            <ArrowLeft className="w-4 h-4" />
          </Link>

          {isEditingCourseTitle ? (
            <form
              onSubmit={(e) => {
                e.preventDefault();
                void handleSaveCourseTitle();
              }}
              className="flex items-center gap-2 flex-1 max-w-md"
            >
              <input
                type="text"
                value={tempCourseTitle}
                onChange={(e) => setTempCourseTitle(e.target.value)}
                data-testid="studio-course-title-input"
                className="flex-1 h-9 px-3 rounded-md border border-accent bg-bg-primary text-sm font-semibold text-text-primary focus:outline-none"
                autoFocus
              />
              <button
                type="submit"
                className="h-9 px-3 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover transition cursor-pointer"
              >
                Lưu
              </button>
              <button
                type="button"
                onClick={() => {
                  setIsEditingCourseTitle(false);
                  setTempCourseTitle(course.title);
                }}
                className="h-9 px-2.5 rounded-md border border-border text-xs text-text-secondary hover:text-text-primary transition cursor-pointer"
              >
                Hủy
              </button>
            </form>
          ) : (
            <div className="flex items-center gap-2 min-w-0">
              <h1 className="text-xl sm:text-2xl font-bold text-text-primary truncate">
                {course.title}
              </h1>
              <span className="hidden sm:inline-flex px-2 py-0.5 rounded text-[11px] font-medium bg-bg-secondary text-text-secondary border border-border shrink-0">
                Lộ trình &amp; Thẻ nhớ FSRS
              </span>
              <button
                type="button"
                onClick={() => {
                  setTempCourseTitle(course.title);
                  setIsEditingCourseTitle(true);
                }}
                className="p-1.5 rounded-md text-text-tertiary hover:text-text-primary hover:bg-bg-tertiary transition cursor-pointer shrink-0"
                title="Đổi tên khóa học"
              >
                <Pencil className="w-3.5 h-3.5" />
              </button>
            </div>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            data-testid="studio-publish-toggle-btn"
            onClick={() => void handleTogglePublish()}
            disabled={isTogglingPublish}
            className={`inline-flex items-center gap-1.5 h-9 px-3.5 rounded-md border text-xs sm:text-sm font-medium transition cursor-pointer disabled:opacity-50 ${
              course.isPublished || course.visibility === "public"
                ? "border-emerald-300 dark:border-emerald-800 bg-emerald-50 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-300"
                : "border-border bg-bg-secondary text-text-secondary hover:text-text-primary"
            }`}
          >
            {course.isPublished || course.visibility === "public" ? (
              <>
                <Globe className="w-3.5 h-3.5" />
                <span>Đã xuất bản</span>
              </>
            ) : (
              <>
                <Lock className="w-3.5 h-3.5" />
                <span>Bản nháp · Xuất bản</span>
              </>
            )}
          </button>

          <button
            type="button"
            data-testid="studio-export-zip-btn"
            onClick={() => void handleExportZip()}
            disabled={isExporting}
            className="inline-flex items-center gap-1.5 h-9 px-3 rounded-md border border-border bg-bg-secondary text-text-primary text-xs sm:text-sm font-medium hover:bg-bg-tertiary transition cursor-pointer disabled:opacity-50"
            title="Xuất khóa học dạng Markdown ZIP"
          >
            <Download className="w-3.5 h-3.5" />
            <span className="hidden sm:inline">{isExporting ? "Đang xuất..." : "Xuất ZIP"}</span>
          </button>

          <Link
            href={`/app/courses/${courseId}/learn`}
            data-testid="studio-preview-learn-btn"
            className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-md border border-border bg-bg-secondary text-text-primary text-xs sm:text-sm font-medium hover:bg-bg-tertiary transition shrink-0"
          >
            <Eye className="w-4 h-4" />
            <span className="hidden sm:inline">Học thử</span>
          </Link>
        </div>
      </div>

      {/* Feedback banners */}
      {(errorBanner || saveMessage) && (
        <div className="mb-5 space-y-2">
          {errorBanner && (
            <div
              data-testid="studio-error-banner"
              className="p-3 rounded-md border border-rose-300 dark:border-rose-800 bg-rose-50 dark:bg-rose-950/30 text-rose-700 dark:text-rose-300 text-xs flex items-center gap-2"
            >
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>{errorBanner}</span>
            </div>
          )}
          {saveMessage && (
            <div
              data-testid="studio-save-banner"
              className="p-3 rounded-md border border-emerald-300 dark:border-emerald-800 bg-emerald-50 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-300 text-xs flex items-center gap-2"
            >
              <Check className="w-4 h-4 shrink-0" />
              <span>{saveMessage}</span>
            </div>
          )}
        </div>
      )}

      {/* Main Content: Chapter tree + Editor */}
      <div className="flex flex-col lg:flex-row gap-6">
        {/* Left: Chapter/Lesson Tree (45%) */}
        <div className="lg:w-5/12">
          <div className="rounded-lg border border-border bg-bg-secondary">
            {/* Tree header */}
            <div className="flex items-center justify-between p-4 border-b border-border">
              <h2 className="text-base font-semibold text-text-primary">
                Nội dung khóa học
              </h2>
              <button
                type="button"
                data-testid="studio-add-chapter-btn"
                onClick={() => {
                  setShowNewChapterInput((prev) => !prev);
                  setNewChapterTitle("");
                }}
                className="inline-flex items-center gap-1.5 text-sm text-accent hover:underline transition cursor-pointer"
              >
                <Plus className="w-3.5 h-3.5" />
                <span>Thêm chương</span>
              </button>
            </div>

            {showNewChapterInput && (
              <form
                onSubmit={(e) => void handleCreateChapter(e)}
                className="p-3 border-b border-border bg-bg-primary/60 flex items-center gap-2"
              >
                <input
                  type="text"
                  data-testid="studio-new-chapter-input"
                  value={newChapterTitle}
                  onChange={(e) => setNewChapterTitle(e.target.value)}
                  placeholder={`Chương ${(course.chapters?.length ?? 0) + 1}: Tên chương...`}
                  className="flex-1 h-8 px-2.5 rounded border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                  autoFocus
                />
                <button
                  type="submit"
                  data-testid="studio-create-chapter-confirm"
                  disabled={creatingChapter}
                  className="h-8 px-3 rounded bg-accent text-white text-xs font-medium hover:bg-accent-hover transition cursor-pointer disabled:opacity-50"
                >
                  {creatingChapter ? "Đang tạo..." : "Tạo"}
                </button>
                <button
                  type="button"
                  onClick={() => setShowNewChapterInput(false)}
                  className="h-8 px-2 rounded border border-border text-xs text-text-secondary hover:text-text-primary transition cursor-pointer"
                >
                  Hủy
                </button>
              </form>
            )}

            {/* Chapters */}
            {course.chapters.length === 0 ? (
              <div className="p-6 text-center text-sm text-text-tertiary">
                Chưa có chương nào. Nhấn &ldquo;Thêm chương&rdquo; để bắt đầu.
              </div>
            ) : (
              <div className="divide-y divide-border">
                {course.chapters.map((chapter) => {
                  const chId = chapter.chapterId || chapter.id;
                  const isExpanded = expandedChapters.has(chId);
                  return (
                    <div key={chId}>
                      {/* Chapter header */}
                      <div className="flex items-center justify-between pr-3 hover:bg-bg-tertiary/50 transition">
                        <button
                          type="button"
                          onClick={() => toggleChapter(chId)}
                          className="flex-1 min-w-0 flex items-center gap-2 p-3.5 text-left cursor-pointer"
                        >
                          {isExpanded ? (
                            <ChevronDown className="w-4 h-4 text-text-tertiary shrink-0" />
                          ) : (
                            <ChevronRight className="w-4 h-4 text-text-tertiary shrink-0" />
                          )}
                          <BookOpen className="w-4 h-4 text-text-secondary shrink-0" />
                          <span className="text-sm font-medium text-text-primary truncate flex-1">
                            {chapter.title}
                          </span>
                          <span className="text-xs text-text-tertiary shrink-0">
                            {chapter.lessons.length} bài
                          </span>
                        </button>

                        {confirmDeleteChapterId === chId ? (
                          <div className="flex items-center gap-1 shrink-0">
                            <button
                              type="button"
                              onClick={() => void handleDeleteChapter(chId)}
                              className="px-2 py-1 rounded bg-rose-600 text-white text-[11px] font-medium cursor-pointer"
                            >
                              Xóa
                            </button>
                            <button
                              type="button"
                              onClick={() => setConfirmDeleteChapterId(null)}
                              className="px-1.5 py-1 rounded border border-border text-[11px] text-text-secondary cursor-pointer"
                            >
                              Hủy
                            </button>
                          </div>
                        ) : (
                          <button
                            type="button"
                            onClick={() => setConfirmDeleteChapterId(chId)}
                            className="p-1.5 rounded text-text-tertiary hover:text-rose-500 transition cursor-pointer shrink-0"
                            title="Xóa chương"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        )}
                      </div>

                      {/* Lessons */}
                      {isExpanded && (
                        <div className="pb-2">
                          {chapter.lessons.map((lesson) => {
                            const lId = lesson.lessonId || lesson.id;
                            const isSelected =
                              selectedLesson &&
                              (selectedLesson.lessonId || selectedLesson.id) === lId;
                            return (
                              <div
                                key={lId}
                                className={`flex items-center justify-between pl-10 pr-3 py-1 transition ${
                                  isSelected
                                    ? "bg-accent-subtle text-accent"
                                    : "hover:bg-bg-tertiary/50 text-text-secondary"
                                }`}
                              >
                                <button
                                  type="button"
                                  data-testid={`studio-lesson-item-${lId}`}
                                  onClick={() => void selectLesson(lesson)}
                                  className="flex-1 min-w-0 flex items-center gap-2 py-1.5 text-left cursor-pointer"
                                >
                                  <FileText className="w-3.5 h-3.5 shrink-0" />
                                  <span className="text-sm truncate">{lesson.title}</span>
                                </button>

                                {confirmDeleteLessonId === lId ? (
                                  <div className="flex items-center gap-1 shrink-0">
                                    <button
                                      type="button"
                                      onClick={() => void handleDeleteLesson(chId, lId)}
                                      className="px-2 py-0.5 rounded bg-rose-600 text-white text-[11px] font-medium cursor-pointer"
                                    >
                                      Xóa
                                    </button>
                                    <button
                                      type="button"
                                      onClick={() => setConfirmDeleteLessonId(null)}
                                      className="px-1.5 py-0.5 rounded border border-border text-[11px] text-text-secondary cursor-pointer"
                                    >
                                      Hủy
                                    </button>
                                  </div>
                                ) : (
                                  <button
                                    type="button"
                                    onClick={() => setConfirmDeleteLessonId(lId)}
                                    className="p-1 rounded text-text-tertiary hover:text-rose-500 transition cursor-pointer shrink-0"
                                    title="Xóa bài học"
                                  >
                                    <Trash2 className="w-3 h-3" />
                                  </button>
                                )}
                              </div>
                            );
                          })}

                          {/* Add lesson inline form or trigger */}
                          {addingLessonChapterId === chId ? (
                            <form
                              onSubmit={(e) => void handleCreateLesson(chId, e)}
                              className="mx-3 mt-1.5 p-2 rounded border border-border bg-bg-primary flex items-center gap-1.5"
                            >
                              <input
                                type="text"
                                data-testid={`studio-new-lesson-input-${chId}`}
                                value={newLessonTitle}
                                onChange={(e) => setNewLessonTitle(e.target.value)}
                                placeholder="Tiêu đề bài học mới..."
                                className="flex-1 h-7 px-2 rounded border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                                autoFocus
                              />
                              <button
                                type="submit"
                                data-testid={`studio-create-lesson-confirm-${chId}`}
                                disabled={creatingLesson}
                                className="h-7 px-2.5 rounded bg-accent text-white text-xs font-medium hover:bg-accent-hover transition cursor-pointer disabled:opacity-50"
                              >
                                Tạo
                              </button>
                              <button
                                type="button"
                                onClick={() => setAddingLessonChapterId(null)}
                                className="h-7 px-2 rounded border border-border text-xs text-text-secondary cursor-pointer"
                              >
                                Hủy
                              </button>
                            </form>
                          ) : (
                            <button
                              type="button"
                              data-testid={`studio-add-lesson-btn-${chId}`}
                              onClick={() => {
                                setAddingLessonChapterId(chId);
                                setNewLessonTitle("");
                              }}
                              className="w-full flex items-center gap-2 pl-10 pr-4 py-2 text-left text-xs text-text-tertiary hover:text-accent transition cursor-pointer"
                            >
                              <Plus className="w-3 h-3" />
                              <span>Thêm bài học</span>
                            </button>
                          )}
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>

        {/* Right: Editor Panel (55%) */}
        <div className="lg:w-7/12">
          <div className="rounded-lg border border-border bg-bg-secondary p-5 sticky top-6">
            {selectedLesson ? (
              <div className="space-y-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <h3 className="text-base font-semibold text-text-primary">
                    Chỉnh sửa bài học
                  </h3>
                  <div className="inline-flex rounded-md border border-border bg-bg-primary p-0.5 text-xs">
                    <button
                      type="button"
                      onClick={() => setEditorTab("write")}
                      className={`px-3 py-1 rounded transition cursor-pointer ${
                        editorTab === "write"
                          ? "bg-accent text-white font-medium"
                          : "text-text-secondary hover:text-text-primary"
                      }`}
                    >
                      Soạn Markdown
                    </button>
                    <button
                      type="button"
                      onClick={() => setEditorTab("preview")}
                      className={`px-3 py-1 rounded transition cursor-pointer ${
                        editorTab === "preview"
                          ? "bg-accent text-white font-medium"
                          : "text-text-secondary hover:text-text-primary"
                      }`}
                    >
                      Xem trước
                    </button>
                  </div>
                </div>

                {/* Title + Minutes */}
                <div className="grid grid-cols-1 sm:grid-cols-4 gap-3">
                  <div className="sm:col-span-3">
                    <label className="block text-xs font-medium text-text-secondary mb-1.5">
                      Tiêu đề bài học
                    </label>
                    <input
                      type="text"
                      data-testid="studio-lesson-title-input"
                      value={editTitle}
                      onChange={(e) => setEditTitle(e.target.value)}
                      className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-text-primary text-sm focus:outline-none focus:border-accent transition"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-text-secondary mb-1.5">
                      Thời lượng (phút)
                    </label>
                    <input
                      type="number"
                      min={1}
                      max={240}
                      value={editMinutes}
                      onChange={(e) => setEditMinutes(Number(e.target.value) || 1)}
                      className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-text-primary text-sm focus:outline-none focus:border-accent transition"
                    />
                  </div>
                </div>

                {/* Video URL */}
                <div>
                  <label className="block text-xs font-medium text-text-secondary mb-1.5">
                    Video đính kèm (URL YouTube / MP4 — tùy chọn)
                  </label>
                  <input
                    type="url"
                    value={editVideoUrl}
                    onChange={(e) => setEditVideoUrl(e.target.value)}
                    placeholder="https://www.youtube.com/watch?v=..."
                    className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-text-primary text-sm placeholder:text-text-tertiary focus:outline-none focus:border-accent transition"
                  />
                </div>

                {/* Markdown editor or Live Preview */}
                <div>
                  <label className="block text-xs font-medium text-text-secondary mb-1.5">
                    {editorTab === "write" ? "Nội dung Markdown" : "Xem trước hiển thị"}
                  </label>
                  {editorTab === "write" ? (
                    <textarea
                      data-testid="studio-lesson-markdown-input"
                      value={editContent}
                      onChange={(e) => setEditContent(e.target.value)}
                      rows={15}
                      placeholder="# Tiêu đề bài học&#10;&#10;Nhập nội dung Markdown tại đây..."
                      className="w-full px-3 py-2.5 rounded-md border border-border bg-bg-primary text-text-primary font-mono text-xs leading-relaxed placeholder:text-text-tertiary focus:outline-none focus:border-accent transition resize-y"
                    />
                  ) : (
                    <div className="min-h-[300px] max-h-[460px] overflow-y-auto rounded-md border border-border bg-bg-primary p-4">
                      <LessonMarkdownView
                        markdown={editContent || "*Chưa có nội dung Markdown.*"}
                        lessonTitle={editTitle}
                        cleanReadingMode={true}
                        anchoredAnnotations={[]}
                        onSelectBlockText={() => {}}
                        onHighlightClick={() => {}}
                        onInsertCitation={() => {}}
                      />
                    </div>
                  )}
                </div>

                {/* Save button */}
                <div className="flex items-center justify-between pt-2 border-t border-border-subtle">
                  <span className="text-xs text-text-tertiary">
                    ID: {selectedLesson.lessonId || selectedLesson.id}
                  </span>
                  <button
                    type="button"
                    data-testid="studio-save-lesson-btn"
                    onClick={() => void handleSave()}
                    disabled={isSaving}
                    className={`inline-flex items-center gap-2 h-9 px-4 rounded-md text-sm font-medium transition cursor-pointer ${
                      isSaving
                        ? "bg-bg-tertiary text-text-tertiary cursor-not-allowed"
                        : "bg-accent text-white hover:bg-accent-hover"
                    }`}
                  >
                    {isSaving ? (
                      <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                    ) : (
                      <Save className="w-4 h-4" />
                    )}
                    <span>{isSaving ? "Đang lưu..." : "Lưu thay đổi"}</span>
                  </button>
                </div>
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center py-14 text-center">
                <FileText className="w-8 h-8 text-text-tertiary mb-3" />
                <p className="text-sm text-text-secondary">
                  Chọn một bài học bên trái để chỉnh sửa nội dung Markdown
                </p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
