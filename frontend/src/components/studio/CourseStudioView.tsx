"use client";

import React, { useState, useEffect, useCallback, useMemo } from "react";
import {
  GraduationCap,
  Plus,
  Trash2,
  Save,
  Share2,
  Download,
  Video,
  Play,
  Eye,
  FileText,
  Check,
  Clock,
  Globe,
  Lock,
  Columns,
  ChevronRight,
  ChevronDown,
  Sparkles,
  Copy,
  AlertCircle,
  ExternalLink,
  RefreshCw,
  FolderPlus,
  BookOpen
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";

export interface StudioLesson {
  id: string;
  chapterId: string;
  courseId?: string;
  title: string;
  slug?: string;
  sortOrder: number;
  contentMarkdown?: string;
  plainText?: string;
  videoUrl?: string;
  estimatedMinutes?: number;
  currentVersion?: number;
}

export interface StudioChapter {
  id: string;
  courseId: string;
  title: string;
  sortOrder: number;
  lessons: StudioLesson[];
}

export interface StudioCourse {
  id: string;
  title: string;
  slug?: string;
  description?: string;
  visibility?: string;
  isPublished?: boolean;
  chapterCount?: number;
  lessonCount?: number;
  chapters: StudioChapter[];
  sourceDocumentId?: string | null;
  updatedAt?: string;
}

// Helper to parse YouTube, Vimeo or MP4 video URLs into an embeddable or direct stream URL
export function parseVideoUrl(url: string | undefined): {
  type: "youtube" | "vimeo" | "direct" | "unknown";
  embedUrl: string;
} | null {
  if (!url || !url.trim()) return null;
  const clean = url.trim();

  // YouTube detection
  const ytMatch = clean.match(
    /(?:youtube\.com\/(?:watch\?.*v=|embed\/|shorts\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})/i
  );
  if (ytMatch && ytMatch[1]) {
    return {
      type: "youtube",
      embedUrl: `https://www.youtube.com/embed/${ytMatch[1]}`
    };
  }

  // Vimeo detection
  const vimeoMatch = clean.match(/vimeo\.com\/(?:video\/)?([0-9]+)/i);
  if (vimeoMatch && vimeoMatch[1]) {
    return {
      type: "vimeo",
      embedUrl: `https://player.vimeo.com/video/${vimeoMatch[1]}`
    };
  }

  // Direct MP4 / WebM / Ogg video
  if (
    /\.(mp4|webm|ogg|mov)($|\?)/i.test(clean) ||
    clean.startsWith("blob:") ||
    clean.startsWith("data:video")
  ) {
    return {
      type: "direct",
      embedUrl: clean
    };
  }

  // Fallback for valid HTTP links
  if (clean.startsWith("http://") || clean.startsWith("https://")) {
    return {
      type: "direct",
      embedUrl: clean
    };
  }

  return null;
}

// Lightweight Markdown Renderer supporting headers, bold, code, lists, tables, blockquotes
function MarkdownRenderer({ content }: { content: string }) {
  if (!content || !content.trim()) {
    return (
      <div className="text-slate-400 dark:text-slate-500 italic text-sm p-4">
        (Nội dung bài học chưa có văn bản. Hãy nhập nội dung Markdown ở khung bên trái...)
      </div>
    );
  }

  const lines = content.split("\n");
  const renderedElements: React.ReactNode[] = [];
  let inCodeBlock = false;
  let codeBlockContent: string[] = [];
  let codeBlockLang = "";

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];

    // Code block toggle
    if (line.trim().startsWith("```")) {
      if (inCodeBlock) {
        // End code block
        renderedElements.push(
          <pre
            key={`code-${i}`}
            className="my-3 p-3.5 rounded-xl bg-slate-900 text-slate-100 font-mono text-xs overflow-x-auto border border-slate-800"
          >
            <code>{codeBlockContent.join("\n")}</code>
          </pre>
        );
        inCodeBlock = false;
        codeBlockContent = [];
      } else {
        inCodeBlock = true;
        codeBlockLang = line.trim().replace(/^```/, "").trim();
      }
      continue;
    }

    if (inCodeBlock) {
      codeBlockContent.push(line);
      continue;
    }

    // Headings
    if (line.startsWith("# ")) {
      renderedElements.push(
        <h1
          key={`h1-${i}`}
          className="text-2xl font-bold text-slate-900 dark:text-slate-100 mt-6 mb-3 pb-2 border-b border-slate-200 dark:border-slate-800"
        >
          {formatInline(line.slice(2))}
        </h1>
      );
      continue;
    }
    if (line.startsWith("## ")) {
      renderedElements.push(
        <h2
          key={`h2-${i}`}
          className="text-xl font-bold text-slate-900 dark:text-slate-100 mt-5 mb-2.5"
        >
          {formatInline(line.slice(3))}
        </h2>
      );
      continue;
    }
    if (line.startsWith("### ")) {
      renderedElements.push(
        <h3
          key={`h3-${i}`}
          className="text-lg font-semibold text-slate-800 dark:text-slate-200 mt-4 mb-2"
        >
          {formatInline(line.slice(4))}
        </h3>
      );
      continue;
    }
    if (line.startsWith("#### ")) {
      renderedElements.push(
        <h4
          key={`h4-${i}`}
          className="text-base font-semibold text-slate-800 dark:text-slate-200 mt-3 mb-1.5"
        >
          {formatInline(line.slice(5))}
        </h4>
      );
      continue;
    }

    // Blockquote
    if (line.startsWith("> ")) {
      renderedElements.push(
        <blockquote
          key={`quote-${i}`}
          className="my-2.5 pl-4 border-l-4 border-indigo-500 bg-indigo-50/50 dark:bg-indigo-950/20 py-2 pr-3 rounded-r-lg text-sm text-slate-700 dark:text-slate-300 italic"
        >
          {formatInline(line.slice(2))}
        </blockquote>
      );
      continue;
    }

    // Unordered List
    if (line.trim().startsWith("- ") || line.trim().startsWith("* ")) {
      const bulletText = line.trim().slice(2);
      renderedElements.push(
        <li
          key={`li-${i}`}
          className="ml-5 list-disc text-sm text-slate-700 dark:text-slate-300 my-1 leading-relaxed"
        >
          {formatInline(bulletText)}
        </li>
      );
      continue;
    }

    // Ordered List
    const numMatch = line.trim().match(/^(\d+)\.\s+(.*)$/);
    if (numMatch) {
      renderedElements.push(
        <li
          key={`num-li-${i}`}
          className="ml-5 list-decimal text-sm text-slate-700 dark:text-slate-300 my-1 leading-relaxed"
        >
          {formatInline(numMatch[2])}
        </li>
      );
      continue;
    }

    // Horizontal Rule
    if (line.trim() === "---" || line.trim() === "***") {
      renderedElements.push(
        <hr key={`hr-${i}`} className="my-5 border-slate-200 dark:border-slate-800" />
      );
      continue;
    }

    // Blank line
    if (!line.trim()) {
      renderedElements.push(<div key={`blank-${i}`} className="h-2" />);
      continue;
    }

    // Normal Paragraph
    renderedElements.push(
      <p
        key={`p-${i}`}
        className="text-sm text-slate-700 dark:text-slate-300 my-2 leading-relaxed"
      >
        {formatInline(line)}
      </p>
    );
  }

  return <div className="space-y-1">{renderedElements}</div>;
}

// Inline formatting parser for bold, italic, inline code, links
function formatInline(text: string): React.ReactNode {
  // Simple regex parser for inline tokens
  const parts = text.split(/(`[^`]+`|\*\*[^*]+\*\*|\*[^*]+\*|\[[^\]]+\]\([^)]+\))/g);
  return parts.map((part, idx) => {
    if (part.startsWith("`") && part.endsWith("`") && part.length > 2) {
      return (
        <code
          key={idx}
          className="px-1.5 py-0.5 rounded-md bg-slate-100 dark:bg-slate-800 text-indigo-600 dark:text-indigo-400 font-mono text-xs font-semibold"
        >
          {part.slice(1, -1)}
        </code>
      );
    }
    if (part.startsWith("**") && part.endsWith("**") && part.length > 4) {
      return <strong key={idx} className="font-bold text-slate-900 dark:text-slate-100">{part.slice(2, -2)}</strong>;
    }
    if (part.startsWith("*") && part.endsWith("*") && part.length > 2) {
      return <em key={idx} className="italic">{part.slice(1, -1)}</em>;
    }
    const linkMatch = part.match(/^\[([^\]]+)\]\(([^)]+)\)$/);
    if (linkMatch) {
      return (
        <a
          key={idx}
          href={linkMatch[2]}
          target="_blank"
          rel="noreferrer"
          className="text-indigo-600 dark:text-indigo-400 underline hover:text-indigo-700 font-medium inline-flex items-center gap-0.5"
        >
          {linkMatch[1]}
        </a>
      );
    }
    return part;
  });
}

export function CourseStudioView({ onRefreshDashboard }: { onRefreshDashboard?: () => void }) {
  const {
    editingCourseId,
    setEditingCourseId,
    editingLessonId,
    setEditingLessonId,
    activeCourseId,
    setActiveCourseId,
    setActiveLesson,
    setActiveMode
  } = useWorkspaceStore();

  const [courseList, setCourseList] = useState<StudioCourse[]>([]);
  const [course, setCourse] = useState<StudioCourse | null>(null);
  const [loadingCourse, setLoadingCourse] = useState<boolean>(true);

  // Lesson editing state
  const [lessonTitle, setLessonTitle] = useState("");
  const [lessonMinutes, setLessonMinutes] = useState(10);
  const [videoUrl, setVideoUrl] = useState("");
  const [markdownContent, setMarkdownContent] = useState("");
  const [editorViewMode, setEditorViewMode] = useState<"split" | "edit" | "preview">("split");
  const [savingLesson, setSavingLesson] = useState(false);
  const [saveSuccessNotice, setSaveSuccessNotice] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [shareCopiedNotice, setShareCopiedNotice] = useState(false);
  const [publishToggling, setPublishToggling] = useState(false);
  const [exportingZip, setExportingZip] = useState(false);
  const [expandedChapters, setExpandedChapters] = useState<Record<string, boolean>>({});

  // Title editing state
  const [isEditingCourseTitle, setIsEditingCourseTitle] = useState(false);
  const [tempCourseTitle, setTempCourseTitle] = useState("");

  // Load all available courses
  const fetchCourses = useCallback(async () => {
    try {
      setLoadingCourse(true);
      const headers = getClientRequestHeaders();
      const res = await fetch("/api/v1/courses", { headers });
      if (res.ok) {
        const data = await res.json();
        const list: StudioCourse[] = Array.isArray(data) ? data : data.courses ?? [];
        setCourseList(list);

        // Determine which course to pick: editingCourseId > activeCourseId > list[0]
        const targetId = editingCourseId || activeCourseId || (list[0]?.id ?? null);
        if (targetId) {
          fetchCourseDetails(targetId);
        } else {
          setLoadingCourse(false);
        }
      } else {
        setLoadingCourse(false);
      }
    } catch (err) {
      console.error("Failed to fetch courses for Studio:", err);
      setLoadingCourse(false);
    }
  }, [editingCourseId, activeCourseId]);

  // Load full course details
  const fetchCourseDetails = async (courseId: string) => {
    try {
      setLoadingCourse(true);
      const headers = getClientRequestHeaders();
      const res = await fetch(`/api/v1/courses/${courseId}`, { headers });
      if (res.ok) {
        const data: StudioCourse = await res.json();
        setCourse(data);
        setTempCourseTitle(data.title || "Khóa học chưa đặt tên");
        setEditingCourseId(data.id);
        setActiveCourseId(data.id);

        // Auto-expand all chapters
        const exp: Record<string, boolean> = {};
        data.chapters?.forEach((ch) => {
          exp[ch.id] = true;
        });
        setExpandedChapters(exp);

        // Auto-select lesson: if editingLessonId is in this course, keep it, else pick first lesson
        const allLessons = data.chapters?.flatMap((ch) => ch.lessons) || [];
        if (allLessons.length > 0) {
          const matched = allLessons.find((l) => l.id === editingLessonId);
          if (matched) {
            loadLessonDetails(matched.id, matched);
          } else {
            loadLessonDetails(allLessons[0].id, allLessons[0]);
          }
        } else {
          setEditingLessonId(null);
          clearLessonEditor();
        }
      }
    } catch (err) {
      console.error("Failed to load course details:", err);
    } finally {
      setLoadingCourse(false);
    }
  };

  const clearLessonEditor = () => {
    setLessonTitle("");
    setLessonMinutes(10);
    setVideoUrl("");
    setMarkdownContent("");
  };

  // Load full lesson data
  const loadLessonDetails = async (lessonId: string, fallbackLesson?: StudioLesson) => {
    setEditingLessonId(lessonId);
    if (fallbackLesson) {
      setLessonTitle(fallbackLesson.title || "");
      setLessonMinutes(fallbackLesson.estimatedMinutes || 10);
      setVideoUrl(fallbackLesson.videoUrl || "");
      setMarkdownContent(
        fallbackLesson.contentMarkdown || fallbackLesson.plainText || "# " + fallbackLesson.title + "\n\nNội dung bài học..."
      );
    }

    try {
      const headers = getClientRequestHeaders();
      const res = await fetch(`/api/v1/lessons/${lessonId}`, { headers });
      if (res.ok) {
        const data = await res.json();
        setLessonTitle(data.title || fallbackLesson?.title || "");
        if (typeof data.estimatedMinutes === "number") {
          setLessonMinutes(data.estimatedMinutes);
        }
        if (typeof data.videoUrl === "string") {
          setVideoUrl(data.videoUrl);
        }
        if (typeof data.contentMarkdown === "string" && data.contentMarkdown) {
          setMarkdownContent(data.contentMarkdown);
        } else if (typeof data.plainText === "string" && data.plainText) {
          setMarkdownContent(data.plainText);
        }
      }
    } catch (err) {
      console.warn("Could not fetch remote lesson details, using local state:", err);
    }
  };

  useEffect(() => {
    fetchCourses();
  }, [fetchCourses]);

  // Handle course title update
  const handleSaveCourseTitle = async () => {
    if (!course || !tempCourseTitle.trim()) return;
    try {
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const res = await fetch(`/api/v1/courses/${course.id}`, {
        method: "PUT",
        headers,
        body: JSON.stringify({ title: tempCourseTitle.trim() })
      });
      if (res.ok) {
        setCourse((prev) => (prev ? { ...prev, title: tempCourseTitle.trim() } : prev));
        setIsEditingCourseTitle(false);
        onRefreshDashboard?.();
      }
    } catch (err) {
      console.error("Failed to update course title:", err);
      // Optimistic update
      setCourse((prev) => (prev ? { ...prev, title: tempCourseTitle.trim() } : prev));
      setIsEditingCourseTitle(false);
    }
  };

  // Handle Publish / Unpublish Toggle
  const handleTogglePublish = async () => {
    if (!course) return;
    try {
      setPublishToggling(true);
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const res = await fetch(`/api/v1/courses/${course.id}/publish`, {
        method: "POST",
        headers
      });

      const nextVisibility = course.visibility === "public" ? "draft" : "public";
      if (res.ok) {
        const data = await res.json();
        const updatedVisibility = data.visibility || nextVisibility;
        setCourse((prev) =>
          prev
            ? {
                ...prev,
                visibility: updatedVisibility,
                isPublished: updatedVisibility === "public"
              }
            : prev
        );
      } else {
        // Optimistic fallback
        setCourse((prev) =>
          prev
            ? {
                ...prev,
                visibility: nextVisibility,
                isPublished: nextVisibility === "public"
              }
            : prev
        );
      }
      onRefreshDashboard?.();
    } catch (err) {
      console.error("Failed to toggle publish status:", err);
      // Optimistic fallback
      const nextVisibility = course.visibility === "public" ? "draft" : "public";
      setCourse((prev) =>
        prev
          ? {
              ...prev,
              visibility: nextVisibility,
              isPublished: nextVisibility === "public"
            }
          : prev
      );
    } finally {
      setPublishToggling(false);
    }
  };

  // Handle Share link copy
  const handleCopyShareLink = () => {
    if (!course) return;
    const shareUrl =
      typeof window !== "undefined"
        ? `${window.location.origin}/workspace?courseId=${course.id}`
        : `/workspace?courseId=${course.id}`;

    if (navigator?.clipboard?.writeText) {
      navigator.clipboard.writeText(shareUrl);
      setShareCopiedNotice(true);
      setTimeout(() => setShareCopiedNotice(false), 3000);
    }
  };

  // Handle Export SWM ZIP
  const handleExportZip = async () => {
    if (!course) return;
    try {
      setExportingZip(true);
      const headers = getClientRequestHeaders();
      const res = await fetch(`/api/v1/courses/${course.id}/export-zip`, { headers });
      if (res.ok) {
        const blob = await res.blob();
        const downloadUrl = window.URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = downloadUrl;
        const filename = `${course.slug || "course"}-${course.id.slice(0, 8)}.zip`;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(downloadUrl);
      } else {
        alert("Chưa thể xuất file ZIP lúc này. Vui lòng thử lại sau.");
      }
    } catch (err) {
      console.error("Export zip error:", err);
      alert("Đã xảy ra lỗi khi tạo gói ZIP khóa học.");
    } finally {
      setExportingZip(false);
    }
  };

  // Add Chapter
  const handleAddChapter = async () => {
    if (!course) return;
    const newSortOrder = (course.chapters?.length || 0) + 1;
    const defaultTitle = `Chương ${newSortOrder}: Chủ đề mới`;

    try {
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const res = await fetch(`/api/v1/courses/${course.id}/chapters`, {
        method: "POST",
        headers,
        body: JSON.stringify({ title: defaultTitle, sortOrder: newSortOrder })
      });

      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        alert(errJson.error || "Không thể tạo chương mới trên máy chủ.");
        return;
      }

      const createdChapter: StudioChapter = await res.json();
      if (!createdChapter.lessons) createdChapter.lessons = [];

      setCourse((prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          chapters: [...(prev.chapters || []), createdChapter]
        };
      });
      setExpandedChapters((prev) => ({ ...prev, [createdChapter.id]: true }));
    } catch (err: any) {
      console.error("Failed to add chapter:", err);
      alert(err.message || "Lỗi khi kết nối đến máy chủ.");
    }
  };

  // Delete Chapter
  const handleDeleteChapter = async (chapterId: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (!confirm("Bạn có chắc chắn muốn xóa chương này cùng các bài học bên trong?")) return;
    try {
      const headers = getClientRequestHeaders();
      const res = await fetch(`/api/v1/chapters/${chapterId}`, {
        method: "DELETE",
        headers
      });

      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        alert(errJson.error || "Không thể xóa chương mục trên máy chủ.");
        return;
      }

      setCourse((prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          chapters: prev.chapters.filter((ch) => ch.id !== chapterId)
        };
      });

      // If active lesson was in this chapter, select another
      const remainingLessons = course?.chapters
        .filter((ch) => ch.id !== chapterId)
        .flatMap((ch) => ch.lessons) || [];
      if (remainingLessons.length > 0) {
        loadLessonDetails(remainingLessons[0].id, remainingLessons[0]);
      } else {
        setEditingLessonId(null);
        clearLessonEditor();
      }
      onRefreshDashboard?.();
    } catch (err: any) {
      console.error("Failed to delete chapter:", err);
      alert(err.message || "Lỗi khi kết nối đến máy chủ.");
    }
  };

  // Delete Lesson
  const handleDeleteLesson = async (lessonId: string, chapterId: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (!confirm("Bạn có chắc chắn muốn xóa bài học này khỏi giáo trình?")) return;
    try {
      const headers = getClientRequestHeaders();
      const res = await fetch(`/api/v1/lessons/${lessonId}`, {
        method: "DELETE",
        headers
      });

      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        alert(errJson.error || "Không thể xóa bài học trên máy chủ.");
        return;
      }

      setCourse((prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          chapters: prev.chapters.map((ch) =>
            ch.id === chapterId
              ? { ...ch, lessons: ch.lessons.filter((l) => l.id !== lessonId) }
              : ch
          )
        };
      });

      if (editingLessonId === lessonId) {
        setEditingLessonId(null);
        clearLessonEditor();
      }
      onRefreshDashboard?.();
    } catch (err: any) {
      console.error("Failed to delete lesson:", err);
      alert(err.message || "Lỗi khi kết nối đến máy chủ.");
    }
  };

  // Add Lesson
  const handleAddLesson = async (chapterId: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (!course) return;
    const targetChapter = course.chapters.find((ch) => ch.id === chapterId);
    const newSortOrder = (targetChapter?.lessons?.length || 0) + 1;
    const defaultTitle = `Bài ${newSortOrder}: Nội dung mới`;
    const defaultMarkdown = `# ${defaultTitle}\n\nChào mừng bạn đến với bài học này.\n\n### Mục tiêu bài học:\n- Nắm vững kiến thức nền tảng\n- Áp dụng vào thực hành thực tế`;

    try {
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const res = await fetch(`/api/v1/chapters/${chapterId}/lessons`, {
        method: "POST",
        headers,
        body: JSON.stringify({
          title: defaultTitle,
          contentMarkdown: defaultMarkdown,
          videoUrl: "",
          estimatedMinutes: 10,
          sortOrder: newSortOrder
        })
      });

      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        alert(errJson.error || "Không thể tạo bài học mới trên máy chủ.");
        return;
      }

      const createdLesson: StudioLesson = await res.json();

      setCourse((prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          chapters: prev.chapters.map((ch) =>
            ch.id === chapterId
              ? { ...ch, lessons: [...(ch.lessons || []), createdLesson] }
              : ch
          )
        };
      });

      // Select newly created lesson in editor
      loadLessonDetails(createdLesson.id, createdLesson);
      onRefreshDashboard?.();
    } catch (err: any) {
      console.error("Failed to add lesson:", err);
      alert(err.message || "Lỗi khi kết nối đến máy chủ.");
    }
  };

  // Save Lesson changes
  const handleSaveLesson = async () => {
    if (!editingLessonId || !course) return;
    try {
      setSavingLesson(true);
      setSaveError(null);
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const payload = {
        title: lessonTitle.trim(),
        contentMarkdown: markdownContent,
        videoUrl: videoUrl.trim(),
        estimatedMinutes: Number(lessonMinutes) || 10,
        plainText: markdownContent,
        contentAstJson: JSON.stringify({
          type: "doc",
          content: [
            {
              type: "paragraph",
              content: [{ type: "text", text: markdownContent }]
            }
          ]
        })
      };

      const res = await fetch(`/api/v1/lessons/${editingLessonId}`, {
        method: "PUT",
        headers,
        body: JSON.stringify(payload)
      });

      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        throw new Error(errJson.error || `Lỗi máy chủ: HTTP ${res.status}`);
      }

      // Update in local course state
      setCourse((prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          chapters: prev.chapters.map((ch) => ({
            ...ch,
            lessons: ch.lessons.map((l) =>
              l.id === editingLessonId
                ? {
                    ...l,
                    title: lessonTitle.trim(),
                    contentMarkdown: markdownContent,
                    videoUrl: videoUrl.trim(),
                    estimatedMinutes: Number(lessonMinutes) || 10
                  }
                : l
            )
          }))
        };
      });

      setSaveSuccessNotice(true);
      setTimeout(() => setSaveSuccessNotice(false), 2500);
      onRefreshDashboard?.();
    } catch (err: any) {
      console.error("Failed to save lesson:", err);
      setSaveError(err.message || "Không thể lưu bài học.");
    } finally {
      setSavingLesson(false);
    }
  };

  // Keyboard shortcut Ctrl+S inside editor
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && (e.key === "s" || e.key === "S")) {
        e.preventDefault();
        handleSaveLesson();
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [editingLessonId, lessonTitle, markdownContent, videoUrl, lessonMinutes, course]);

  // Video parsing
  const videoParsed = useMemo(() => parseVideoUrl(videoUrl), [videoUrl]);

  const isPublic = course?.visibility === "public" || course?.isPublished === true;

  return (
    <div className="flex flex-col h-[calc(100vh-3.5rem)] w-full bg-slate-50 dark:bg-slate-950 overflow-hidden text-slate-800 dark:text-slate-100">
      {/* ------------------------------------------------------------- */}
      {/* 1. TOP STUDIO HEADER BAR */}
      {/* ------------------------------------------------------------- */}
      <header className="h-14 shrink-0 border-b border-slate-200/80 dark:border-slate-800 bg-white/95 dark:bg-slate-900/95 backdrop-blur-md px-4 sm:px-6 flex items-center justify-between gap-4 z-20">
        {/* Left: Course Switcher & Title Editor */}
        <div className="flex items-center gap-3 min-w-0">
          <button
            type="button"
            onClick={() => setActiveMode("dashboard")}
            className="flex items-center gap-1.5 text-xs font-semibold text-slate-500 hover:text-indigo-600 dark:text-slate-400 dark:hover:text-indigo-400 p-1.5 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 transition cursor-pointer shrink-0"
            title="Quay lại Không gian cá nhân"
          >
            <BookOpen className="w-4 h-4 text-indigo-500" />
            <span className="hidden md:inline">Dashboard</span>
          </button>

          <span className="text-slate-300 dark:text-slate-700 hidden sm:inline">/</span>

          {/* Course Selector Dropdown if multiple courses */}
          {courseList.length > 1 && (
            <select
              value={course?.id || ""}
              onChange={(e) => fetchCourseDetails(e.target.value)}
              className="text-xs bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg px-2 py-1 text-slate-700 dark:text-slate-200 font-medium focus:outline-none focus:ring-1 focus:ring-indigo-500 max-w-[140px] sm:max-w-[180px] truncate"
              title="Chuyển khóa học khác"
            >
              {courseList.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.title}
                </option>
              ))}
            </select>
          )}

          {/* Course Title Editor */}
          {isEditingCourseTitle ? (
            <div className="flex items-center gap-1.5">
              <input
                type="text"
                data-testid="studio-course-title-input"
                value={tempCourseTitle}
                onChange={(e) => setTempCourseTitle(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") handleSaveCourseTitle();
                  if (e.key === "Escape") setIsEditingCourseTitle(false);
                }}
                className="text-xs sm:text-sm font-bold bg-white dark:bg-slate-950 border border-indigo-500 rounded-lg px-2 py-1 text-slate-900 dark:text-white focus:outline-none ring-2 ring-indigo-500/20"
                autoFocus
              />
              <button
                type="button"
                onClick={handleSaveCourseTitle}
                className="p-1 rounded bg-indigo-600 text-white text-xs hover:bg-indigo-700"
                title="Lưu tên khóa học"
              >
                <Check className="w-3.5 h-3.5" />
              </button>
            </div>
          ) : (
            <div
              onClick={() => {
                setTempCourseTitle(course?.title || "");
                setIsEditingCourseTitle(true);
              }}
              className="flex items-center gap-1.5 cursor-pointer group"
              title="Nhấn để đổi tên khóa học"
            >
              <h1 className="text-xs sm:text-sm font-bold text-slate-900 dark:text-white truncate max-w-[200px] sm:max-w-[340px] group-hover:text-indigo-600 transition">
                {course?.title || (loadingCourse ? "Đang tải khóa học..." : "Studio Khóa học")}
              </h1>
              <span className="text-[10px] text-slate-400 group-hover:text-indigo-500 hidden sm:inline">
                ✎
              </span>
            </div>
          )}

          {/* Status Badge */}
          {course && (
            <span
              className={`text-[10px] font-bold px-2 py-0.5 rounded-full shrink-0 ${
                isPublic
                  ? "bg-emerald-100 text-emerald-800 dark:bg-emerald-950/80 dark:text-emerald-300 border border-emerald-300 dark:border-emerald-700"
                  : "bg-amber-100 text-amber-800 dark:bg-amber-950/80 dark:text-amber-300 border border-amber-300 dark:border-amber-700"
              }`}
            >
              {isPublic ? "Công khai" : "Bản nháp"}
            </span>
          )}
        </div>

        {/* Right: Actions (Publish, Share, Export ZIP, Learner Preview) */}
        <div className="flex items-center gap-2 shrink-0">
          {/* Publish Toggle Button */}
          <button
            type="button"
            data-testid="studio-publish-btn"
            onClick={handleTogglePublish}
            disabled={publishToggling || !course}
            className={`min-h-[34px] px-3 py-1.5 rounded-xl text-xs font-bold flex items-center gap-1.5 transition cursor-pointer shadow-2xs ${
              isPublic
                ? "bg-amber-50 hover:bg-amber-100 text-amber-800 border border-amber-300 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800"
                : "bg-emerald-600 hover:bg-emerald-700 text-white shadow-emerald-500/20"
            }`}
            title={isPublic ? "Chuyển khóa học về Bản nháp riêng tư" : "Xuất bản khóa học để học viên có thể học"}
          >
            {isPublic ? (
              <>
                <Lock className="w-3.5 h-3.5" />
                <span className="hidden sm:inline">Bản nháp</span>
                <span className="sm:hidden">Nháp</span>
              </>
            ) : (
              <>
                <Globe className="w-3.5 h-3.5" />
                <span>Xuất bản</span>
              </>
            )}
          </button>

          {/* Copy Share Link Button */}
          <button
            type="button"
            data-testid="studio-share-btn"
            onClick={handleCopyShareLink}
            disabled={!course}
            className="min-h-[34px] px-2.5 sm:px-3 py-1.5 rounded-xl border border-slate-200 dark:border-slate-800 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300 text-xs font-semibold flex items-center gap-1.5 transition cursor-pointer"
            title="Sao chép link chia sẻ khóa học"
          >
            {shareCopiedNotice ? (
              <>
                <Check className="w-3.5 h-3.5 text-emerald-500" />
                <span className="text-emerald-600 dark:text-emerald-400 font-bold">Đã chép!</span>
              </>
            ) : (
              <>
                <Share2 className="w-3.5 h-3.5 text-indigo-500" />
                <span className="hidden md:inline">Sao chép link</span>
              </>
            )}
          </button>

          {/* Export SWM ZIP Button */}
          <button
            type="button"
            data-testid="studio-export-zip-btn"
            onClick={handleExportZip}
            disabled={exportingZip || !course}
            className="min-h-[34px] px-2.5 sm:px-3 py-1.5 rounded-xl border border-slate-200 dark:border-slate-800 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300 text-xs font-semibold flex items-center gap-1.5 transition cursor-pointer"
            title="Đóng gói và tải xuống toàn bộ khóa học dạng ZIP chuẩn SWM"
          >
            <Download className="w-3.5 h-3.5 text-slate-500" />
            <span className="hidden lg:inline">{exportingZip ? "Đang xuất..." : "Xuất gói ZIP"}</span>
            <span className="lg:hidden">ZIP</span>
          </button>

          {/* Switch to Learner View */}
          <button
            type="button"
            onClick={() => {
              if (course) {
                setActiveCourseId(course.id);
                if (editingLessonId) setActiveLesson(editingLessonId);
              }
              setActiveMode("reading");
            }}
            className="min-h-[34px] px-2.5 sm:px-3 py-1.5 rounded-xl bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-300 text-xs font-semibold flex items-center gap-1.5 transition cursor-pointer"
            title="Xem bài học này dưới giao diện người học (Mode 1)"
          >
            <Eye className="w-3.5 h-3.5 text-indigo-500" />
            <span className="hidden xl:inline">Xem học viên</span>
          </button>
        </div>
      </header>

      {/* ------------------------------------------------------------- */}
      {/* 2. MAIN 2-PANEL STUDIO WORKSPACE */}
      {/* ------------------------------------------------------------- */}
      <div className="flex-1 flex overflow-hidden">
        {/* ========================================================= */}
        {/* LEFT PANEL: Curriculum Tree Builder */}
        {/* ========================================================= */}
        <aside className="w-72 sm:w-80 md:w-88 shrink-0 border-r border-slate-200/80 dark:border-slate-800 bg-white/70 dark:bg-slate-900/70 flex flex-col h-full overflow-hidden">
          {/* Tree Builder Header */}
          <div className="p-3.5 border-b border-slate-200/80 dark:border-slate-800 flex items-center justify-between gap-2 shrink-0 bg-slate-50/50 dark:bg-slate-950/30">
            <div className="flex items-center gap-2">
              <GraduationCap className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
              <span className="text-xs font-bold text-slate-900 dark:text-white uppercase tracking-wider">
                Cây Giáo trình
              </span>
            </div>
            <button
              type="button"
              data-testid="studio-add-chapter-btn"
              onClick={handleAddChapter}
              className="px-2.5 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold flex items-center gap-1 shadow-2xs transition cursor-pointer"
              title="Thêm một chương mới vào giáo trình"
            >
              <Plus className="w-3.5 h-3.5" />
              <span>+ Thêm Chương</span>
            </button>
          </div>

          {/* Chapter & Lesson List */}
          <div className="flex-1 overflow-y-auto p-3 space-y-3">
            {loadingCourse && (
              <div className="p-4 text-center text-xs text-slate-400 space-y-2">
                <RefreshCw className="w-5 h-5 mx-auto animate-spin text-indigo-500" />
                <p>Đang nạp cấu trúc bài giảng...</p>
              </div>
            )}

            {!loadingCourse && (!course || !course.chapters || course.chapters.length === 0) && (
              <div className="p-6 text-center text-xs text-slate-400 border border-dashed border-slate-300 dark:border-slate-800 rounded-2xl space-y-3 my-4">
                <FolderPlus className="w-8 h-8 mx-auto text-indigo-400/80" />
                <p className="font-semibold text-slate-600 dark:text-slate-300">
                  Chưa có chương học nào trong khóa học này.
                </p>
                <button
                  type="button"
                  data-testid="studio-add-chapter-btn"
                  onClick={handleAddChapter}
                  className="px-3 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold inline-flex items-center gap-1.5 shadow-sm"
                >
                  <Plus className="w-3.5 h-3.5" />
                  <span>+ Thêm Chương Đầu Tiên</span>
                </button>
              </div>
            )}

            {course?.chapters?.map((chapter, chIdx) => {
              const isExpanded = expandedChapters[chapter.id] !== false;
              return (
                <div
                  key={chapter.id}
                  className="rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900/90 shadow-2xs overflow-hidden transition"
                >
                  {/* Chapter Header */}
                  <div
                    onClick={() =>
                      setExpandedChapters((prev) => ({
                        ...prev,
                        [chapter.id]: !isExpanded
                      }))
                    }
                    className="p-2.5 bg-slate-50/80 dark:bg-slate-800/60 flex items-center justify-between gap-2 cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-800 transition select-none"
                  >
                    <div className="flex items-center gap-1.5 min-w-0">
                      {isExpanded ? (
                        <ChevronDown className="w-3.5 h-3.5 text-slate-400 shrink-0" />
                      ) : (
                        <ChevronRight className="w-3.5 h-3.5 text-slate-400 shrink-0" />
                      )}
                      <span className="text-xs font-bold text-slate-900 dark:text-slate-100 truncate">
                        {chapter.title}
                      </span>
                    </div>

                    <div className="flex items-center gap-1 shrink-0">
                      {/* Add Lesson to Chapter */}
                      <button
                        type="button"
                        data-testid="studio-add-lesson-btn"
                        onClick={(e) => handleAddLesson(chapter.id, e)}
                        className="p-1 rounded-md text-slate-500 hover:text-indigo-600 hover:bg-slate-200 dark:hover:bg-slate-700 transition"
                        title="Thêm bài học mới vào chương này"
                      >
                        <Plus className="w-3.5 h-3.5" />
                      </button>

                      {/* Delete Chapter */}
                      <button
                        type="button"
                        data-testid="studio-delete-chapter-btn"
                        onClick={(e) => handleDeleteChapter(chapter.id, e)}
                        className="p-1 rounded-md text-slate-400 hover:text-rose-600 hover:bg-rose-50 dark:hover:bg-rose-950/40 transition"
                        title="Xóa chương này"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>

                  {/* Lessons list inside chapter */}
                  {isExpanded && (
                    <div className="p-1.5 space-y-1">
                      {chapter.lessons && chapter.lessons.length > 0 ? (
                        chapter.lessons.map((lesson) => {
                          const isSelected = editingLessonId === lesson.id;
                          return (
                            <div
                              key={lesson.id}
                              onClick={() => loadLessonDetails(lesson.id, lesson)}
                              className={`p-2 rounded-lg text-xs flex items-center justify-between gap-2 cursor-pointer transition ${
                                isSelected
                                  ? "bg-indigo-600 text-white font-semibold shadow-sm"
                                  : "text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800"
                              }`}
                            >
                              <div className="flex items-center gap-2 min-w-0">
                                {lesson.videoUrl ? (
                                  <Video
                                    className={`w-3.5 h-3.5 shrink-0 ${
                                      isSelected ? "text-indigo-200" : "text-amber-500"
                                    }`}
                                  />
                                ) : (
                                  <FileText
                                    className={`w-3.5 h-3.5 shrink-0 ${
                                      isSelected ? "text-indigo-200" : "text-slate-400"
                                    }`}
                                  />
                                )}
                                <span className="truncate">{lesson.title}</span>
                              </div>

                              <div className="flex items-center gap-1.5 shrink-0">
                                {typeof lesson.estimatedMinutes === "number" && (
                                  <span
                                    className={`text-[10px] px-1.5 py-0.2 rounded-md ${
                                      isSelected
                                        ? "bg-indigo-700/80 text-white"
                                        : "bg-slate-100 dark:bg-slate-800 text-slate-500"
                                    }`}
                                  >
                                    {lesson.estimatedMinutes}m
                                  </span>
                                )}
                                <button
                                  type="button"
                                  data-testid="studio-delete-lesson-btn"
                                  onClick={(e) => handleDeleteLesson(lesson.id, chapter.id, e)}
                                  className={`p-1 rounded-md transition cursor-pointer ${
                                    isSelected
                                      ? "text-indigo-200 hover:text-white hover:bg-indigo-700"
                                      : "text-slate-400 hover:text-rose-600 hover:bg-rose-50 dark:hover:bg-rose-950/40"
                                  }`}
                                  title="Xóa bài học này"
                                >
                                  <Trash2 className="w-3.5 h-3.5" />
                                </button>
                              </div>
                            </div>
                          );
                        })
                      ) : (
                        <div className="p-3 text-center text-[11px] text-slate-400 italic">
                          Chưa có bài học nào. Bấm{" "}
                          <span
                            onClick={(e) => handleAddLesson(chapter.id, e)}
                            className="text-indigo-600 dark:text-indigo-400 font-bold cursor-pointer underline"
                          >
                            + Thêm Bài
                          </span>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </aside>

        {/* ========================================================= */}
        {/* CENTER / RIGHT PANEL: Multi-Modal Lesson Editor */}
        {/* ========================================================= */}
        <main className="flex-1 flex flex-col h-full overflow-hidden bg-white dark:bg-slate-950">
          {editingLessonId ? (
            <div className="flex-1 flex flex-col h-full overflow-hidden">
              {/* Lesson Control & Header Bar */}
              <div className="p-4 border-b border-slate-200/80 dark:border-slate-800 bg-white/95 dark:bg-slate-900/95 flex flex-wrap items-center justify-between gap-3 shrink-0">
                <div className="flex-1 min-w-[240px] flex items-center gap-3">
                  <div className="flex-1">
                    <label className="block text-[11px] font-bold text-slate-500 uppercase tracking-wider mb-1">
                      Tiêu đề Bài học
                    </label>
                    <input
                      type="text"
                      data-testid="studio-lesson-title-input"
                      value={lessonTitle}
                      onChange={(e) => setLessonTitle(e.target.value)}
                      placeholder="Nhập tiêu đề bài học..."
                      className="w-full px-3 py-1.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-sm font-bold text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                  </div>

                  <div className="w-28 shrink-0">
                    <label className="block text-[11px] font-bold text-slate-500 uppercase tracking-wider mb-1">
                      Thời lượng
                    </label>
                    <div className="relative flex items-center">
                      <input
                        type="number"
                        data-testid="studio-lesson-minutes-input"
                        min="1"
                        max="300"
                        value={lessonMinutes}
                        onChange={(e) => setLessonMinutes(Number(e.target.value))}
                        className="w-full px-2.5 py-1.5 pr-8 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-sm font-semibold text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 text-center"
                      />
                      <span className="absolute right-2 text-xs text-slate-400 font-medium pointer-events-none">
                        phút
                      </span>
                    </div>
                  </div>
                </div>

                {/* Save Lesson Button */}
                <div className="flex items-center gap-2 shrink-0">
                  {saveSuccessNotice && (
                    <div className="px-2.5 py-1 rounded-lg bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-300 dark:border-emerald-800 text-emerald-700 dark:text-emerald-300 text-xs font-semibold flex items-center gap-1.5 animate-in fade-in">
                      <Check className="w-3.5 h-3.5" />
                      <span>Đã lưu thành công!</span>
                    </div>
                  )}

                  {saveError && (
                    <div className="px-2.5 py-1 rounded-lg bg-rose-50 dark:bg-rose-950/60 border border-rose-300 dark:border-rose-800 text-rose-700 dark:text-rose-300 text-xs font-semibold flex items-center gap-1.5 animate-in fade-in">
                      <AlertCircle className="w-3.5 h-3.5" />
                      <span>{saveError}</span>
                    </div>
                  )}

                  <button
                    type="button"
                    data-testid="studio-save-lesson-btn"
                    onClick={handleSaveLesson}
                    disabled={savingLesson}
                    className="min-h-[38px] px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold text-xs flex items-center gap-2 shadow-sm transition cursor-pointer"
                    title="Lưu các thay đổi của bài học này (Ctrl+S)"
                  >
                    <Save className="w-4 h-4" />
                    <span>{savingLesson ? "Đang lưu..." : "Lưu Bài học"}</span>
                    <kbd className="hidden sm:inline px-1 py-0.2 text-[9px] font-mono bg-indigo-700 text-indigo-100 rounded">
                      Ctrl+S
                    </kbd>
                  </button>
                </div>
              </div>

              {/* Scrollable Content Body */}
              <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-6">
                {/* --------------------------------------------------- */}
                {/* A. MULTI-MODAL VIDEO URL & REAL-TIME PREVIEW PLAYER */}
                {/* --------------------------------------------------- */}
                <div className="rounded-2xl border border-slate-200 dark:border-slate-800 bg-slate-50/60 dark:bg-slate-900/40 p-4 space-y-3">
                  <div className="flex items-center justify-between">
                    <label className="text-xs font-bold text-slate-800 dark:text-slate-200 flex items-center gap-1.5">
                      <Video className="w-4 h-4 text-amber-500" />
                      <span>Đa phương tiện: Video Bài giảng (YouTube / Vimeo / MP4)</span>
                    </label>
                    <span className="text-[11px] text-slate-400">Xem trước tức thì</span>
                  </div>

                  <input
                    type="url"
                    data-testid="studio-video-url-input"
                    value={videoUrl}
                    onChange={(e) => setVideoUrl(e.target.value)}
                    placeholder="Dán đường dẫn YouTube (https://www.youtube.com/watch?v=...), Vimeo hoặc link file .mp4..."
                    className="w-full px-3.5 py-2 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-950 text-xs text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />

                  {/* Video Player Preview */}
                  {videoParsed ? (
                    <div
                      data-testid="studio-video-player"
                      className="w-full aspect-video max-h-[360px] rounded-2xl overflow-hidden bg-black shadow-lg border border-slate-800 flex items-center justify-center relative"
                    >
                      {videoParsed.type === "youtube" || videoParsed.type === "vimeo" ? (
                        <iframe
                          src={videoParsed.embedUrl}
                          title="Trình phát video xem trước bài giảng"
                          className="w-full h-full border-0"
                          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                          allowFullScreen
                        />
                      ) : (
                        <video
                          src={videoParsed.embedUrl}
                          controls
                          className="w-full h-full object-contain"
                        >
                          Trình duyệt của bạn không hỗ trợ thẻ video.
                        </video>
                      )}
                    </div>
                  ) : (
                    <div className="p-4 rounded-xl border border-dashed border-slate-300 dark:border-slate-800 text-center text-xs text-slate-400 space-y-1">
                      <Play className="w-5 h-5 mx-auto text-slate-400" />
                      <p>Nhập liên kết video ở trên để kích hoạt Trình phát đa phương tiện xem trước.</p>
                    </div>
                  )}
                </div>

                {/* --------------------------------------------------- */}
                {/* B. MARKDOWN CONTENT EDITOR & LIVE PREVIEW TOGGLE */}
                {/* --------------------------------------------------- */}
                <div className="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900/60 overflow-hidden flex flex-col shadow-2xs">
                  {/* Markdown Editor Toolbar */}
                  <div className="p-3 border-b border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-950/40 flex items-center justify-between gap-2">
                    <div className="flex items-center gap-2">
                      <FileText className="w-4 h-4 text-indigo-500" />
                      <span className="text-xs font-bold text-slate-800 dark:text-slate-200">
                        Nội dung Bài học (Định dạng Markdown)
                      </span>
                    </div>

                    {/* View Mode Toggle: Edit / Split / Preview */}
                    <div className="flex items-center bg-slate-200/80 dark:bg-slate-800 p-0.5 rounded-lg text-xs font-semibold">
                      <button
                        type="button"
                        onClick={() => setEditorViewMode("edit")}
                        className={`px-2.5 py-1 rounded-md transition ${
                          editorViewMode === "edit"
                            ? "bg-white dark:bg-slate-700 text-indigo-600 dark:text-white shadow-2xs"
                            : "text-slate-600 dark:text-slate-400 hover:text-slate-900"
                        }`}
                        title="Chỉ hiển thị khung soạn thảo"
                      >
                        Chỉ soạn
                      </button>
                      <button
                        type="button"
                        onClick={() => setEditorViewMode("split")}
                        className={`px-2.5 py-1 rounded-md transition flex items-center gap-1 ${
                          editorViewMode === "split"
                            ? "bg-white dark:bg-slate-700 text-indigo-600 dark:text-white shadow-2xs"
                            : "text-slate-600 dark:text-slate-400 hover:text-slate-900"
                        }`}
                        title="Chia đôi màn hình: Soạn thảo & Xem trước trực tiếp"
                      >
                        <Columns className="w-3.5 h-3.5" />
                        <span>Chia đôi</span>
                      </button>
                      <button
                        type="button"
                        onClick={() => setEditorViewMode("preview")}
                        className={`px-2.5 py-1 rounded-md transition flex items-center gap-1 ${
                          editorViewMode === "preview"
                            ? "bg-white dark:bg-slate-700 text-indigo-600 dark:text-white shadow-2xs"
                            : "text-slate-600 dark:text-slate-400 hover:text-slate-900"
                        }`}
                        title="Chỉ hiển thị văn bản kết quả sau khi render"
                      >
                        <Eye className="w-3.5 h-3.5" />
                        <span>Xem trước</span>
                      </button>
                    </div>
                  </div>

                  {/* Split Editor / Preview Area */}
                  <div className="grid grid-cols-1 md:grid-cols-2 divide-y md:divide-y-0 md:divide-x divide-slate-200 dark:divide-slate-800 min-h-[380px]">
                    {/* Left/Top: Textarea Editor */}
                    {(editorViewMode === "edit" || editorViewMode === "split") && (
                      <div
                        className={`p-3.5 flex flex-col ${
                          editorViewMode === "edit" ? "md:col-span-2" : ""
                        }`}
                      >
                        <textarea
                          data-testid="studio-markdown-editor"
                          value={markdownContent}
                          onChange={(e) => setMarkdownContent(e.target.value)}
                          placeholder="Soạn nội dung bài học bằng cú pháp Markdown (# Tiêu đề, **in đậm**, - gạch đầu dòng, code block)..."
                          className="w-full flex-1 min-h-[320px] p-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-950 font-mono text-xs text-slate-800 dark:text-slate-200 leading-relaxed resize-y focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                      </div>
                    )}

                    {/* Right/Bottom: Live Markdown Preview */}
                    {(editorViewMode === "preview" || editorViewMode === "split") && (
                      <div
                        data-testid="studio-markdown-preview"
                        className={`p-5 overflow-y-auto max-h-[500px] bg-slate-50/30 dark:bg-slate-950/20 ${
                          editorViewMode === "preview" ? "md:col-span-2" : ""
                        }`}
                      >
                        <div className="text-[11px] font-bold text-slate-400 uppercase tracking-wider mb-2 flex items-center gap-1.5">
                          <Eye className="w-3.5 h-3.5 text-indigo-500" />
                          <span>Bản xem trước trực tiếp (Live Preview)</span>
                        </div>
                        <MarkdownRenderer content={markdownContent} />
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </div>
          ) : (
            <div className="flex-1 flex flex-col items-center justify-center p-8 text-center text-slate-400 space-y-4">
              <GraduationCap className="w-16 h-16 text-indigo-500/40" />
              <div className="max-w-md space-y-1">
                <h3 className="text-base font-bold text-slate-800 dark:text-slate-200">
                  Chọn hoặc tạo một bài học để bắt đầu
                </h3>
                <p className="text-xs text-slate-500 leading-relaxed">
                  Chọn một bài học từ danh sách bên trái hoặc nhấn{" "}
                  <span className="font-semibold text-indigo-600 dark:text-indigo-400">
                    "+ Thêm Bài"
                  </span>{" "}
                  để soạn thảo nội dung bài giảng đa phương tiện (Video URL, Markdown, Thời lượng).
                </p>
              </div>
            </div>
          )}
        </main>
      </div>
    </div>
  );
}
