"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import {
  AlertCircle,
  BookOpen,
  FileText,
  NotebookPen,
  RefreshCw,
  Search,
  X,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached } from "@/lib/api-client";
import { Button, Card, Badge, EmptyState } from "@/components/ui";
import { getCourseId, normalizeVi, type CourseLike } from "@/lib/course-helpers";

interface NotebookItem {
  id: string;
  title: string;
  version?: number;
  primarySourceLessonId?: string | null;
  primarySourceDocumentId?: string | null;
  createdAt?: string | null;
  updatedAt?: string | null;
}

interface LessonLookupItem {
  id?: string;
  title?: string;
  courseId?: string;
}

interface DocLookupItem {
  id?: string;
  documentId?: string;
  title?: string;
  fileName?: string;
  courseId?: string;
}

const ZERO_GUID = "00000000-0000-0000-0000-000000000000";

function formatDate(iso?: string | null): string {
  if (!iso) return "";
  try {
    return new Date(iso).toLocaleDateString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  } catch {
    return "";
  }
}

export default function NotesPage() {
  const router = useRouter();
  const { activeUserHeaderId, setActiveCourseId, setActiveLesson, setActiveDocument } =
    useWorkspaceStore();

  const [notebooks, setNotebooks] = useState<NotebookItem[]>([]);
  const [lessonMap, setLessonMap] = useState<Record<string, LessonLookupItem>>({});
  const [docMap, setDocMap] = useState<Record<string, DocLookupItem>>({});
  const [courseMap, setCourseMap] = useState<Record<string, string>>({});
  const [searchQuery, setSearchQuery] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const fetchNotes = useCallback(async () => {
    setIsLoading(true);
    setFetchError(null);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );

      const data = await fetchDashboardCached({
        headers: effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      });
      if (!data) {
        setFetchError("Không tải được danh sách sổ tay.");
        return;
      }
      setNotebooks(Array.isArray(data.notebooks) ? data.notebooks : []);

      const nextLessons: Record<string, LessonLookupItem> = {};
      for (const l of (data.lessons ?? []) as LessonLookupItem[]) {
        if (l.id) nextLessons[l.id] = l;
      }
      setLessonMap(nextLessons);

      const nextDocs: Record<string, DocLookupItem> = {};
      for (const d of (data.documents ?? []) as DocLookupItem[]) {
        const id = String(d.documentId || d.id || "");
        if (id) nextDocs[id] = d;
      }
      setDocMap(nextDocs);

      const nextCourses: Record<string, string> = {};
      for (const c of (data.courses ?? []) as CourseLike[]) {
        const id = getCourseId(c);
        if (id && c.title) nextCourses[id] = c.title;
      }
      setCourseMap(nextCourses);
    } catch {
      setFetchError("Không thể kết nối tới máy chủ.");
    } finally {
      setIsLoading(false);
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    fetchNotes();
  }, [fetchNotes]);

  const resolveContext = (nb: NotebookItem) => {
    const lesson = nb.primarySourceLessonId ? lessonMap[nb.primarySourceLessonId] : undefined;
    const doc = nb.primarySourceDocumentId ? docMap[nb.primarySourceDocumentId] : undefined;

    const rawCourseId =
      lesson?.courseId ||
      (doc?.courseId && doc.courseId !== ZERO_GUID ? doc.courseId : undefined) ||
      "";
    const courseId = rawCourseId && rawCourseId !== ZERO_GUID ? rawCourseId : "standalone";
    const courseTitle = rawCourseId ? courseMap[rawCourseId] : undefined;

    const sourceLabel = lesson?.title
      ? `Bài: ${lesson.title}`
      : doc?.title || doc?.fileName
        ? `Tài liệu: ${doc.title || doc.fileName}`
        : courseTitle || "Ghi chú cá nhân";

    return {
      courseId,
      lessonId: nb.primarySourceLessonId || undefined,
      docId: nb.primarySourceDocumentId || undefined,
      sourceLabel,
      courseTitle,
      isDocumentSource: Boolean(!lesson && doc),
    };
  };

  const handleOpenNotebook = (nb: NotebookItem) => {
    const ctx = resolveContext(nb);
    if (ctx.courseId !== "standalone") {
      setActiveCourseId(ctx.courseId);
    }
    if (ctx.lessonId) {
      setActiveLesson(ctx.lessonId);
    } else if (ctx.docId) {
      setActiveDocument(ctx.docId, 1, null);
    }

    const params = new URLSearchParams();
    if (ctx.lessonId) params.set("lesson", ctx.lessonId);
    if (ctx.docId) params.set("doc", ctx.docId);
    params.set("notebook", nb.id);
    params.set("notes", "open");
    router.push(`/app/courses/${ctx.courseId}/learn?${params.toString()}`);
  };

  const filteredNotebooks = useMemo(() => {
    const q = normalizeVi(searchQuery);
    if (!q) return notebooks;
    return notebooks.filter((nb) => {
      const ctx = resolveContext(nb);
      const hay = normalizeVi(`${nb.title || ""} ${ctx.sourceLabel} ${ctx.courseTitle || ""}`);
      return hay.includes(q);
    });
  }, [notebooks, searchQuery, lessonMap, docMap, courseMap]);

  if (isLoading) {
    return (
      <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh] space-y-6">
        <div className="space-y-2">
          <div className="h-8 w-36 rounded-lg swms-skeleton" />
          <div className="h-4 w-72 rounded-md swms-skeleton" />
        </div>
        <div className="h-10 w-full rounded-[10px] swms-skeleton" />
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[0, 1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="h-40 rounded-xl swms-skeleton" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh]">
      <div className="mb-6">
        <h1 className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">
          Sổ tay
        </h1>
        <p className="text-sm text-[var(--text-secondary)] mt-1">
          {notebooks.length} sổ tay ghi chú gắn với bài giảng và tài liệu của bạn.
        </p>
      </div>

      {fetchError && (
        <div className="mb-6 rounded-[10px] border border-red-500/30 bg-red-500/5 px-4 py-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 text-sm text-red-600 dark:text-red-400">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{fetchError}</span>
          </div>
          <Button
            variant="secondary"
            size="sm"
            onClick={fetchNotes}
            leftIcon={<RefreshCw className="w-3.5 h-3.5" />}
          >
            Thử lại
          </Button>
        </div>
      )}

      {/* Search bar & count */}
      <div className="flex flex-col sm:flex-row sm:items-center gap-3 mb-6">
        <div className="relative flex-1">
          <Search className="w-4 h-4 text-[var(--text-tertiary)] absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Tìm sổ tay theo tiêu đề, bài giảng hoặc khóa học..."
            className="w-full h-9 pl-9 pr-8 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8] transition"
          />
          {searchQuery && (
            <button
              type="button"
              onClick={() => setSearchQuery("")}
              aria-label="Xóa lọc"
              className="absolute right-2.5 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer"
            >
              <X className="w-3.5 h-3.5" />
            </button>
          )}
        </div>
        <div className="flex items-center gap-2 shrink-0">
          <Badge variant="neutral" size="sm">
            {searchQuery
              ? `${filteredNotebooks.length} / ${notebooks.length} sổ tay`
              : `${notebooks.length} sổ tay`}
          </Badge>
        </div>
      </div>

      {/* Notebooks Grid */}
      {filteredNotebooks.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredNotebooks.map((nb) => {
            const ctx = resolveContext(nb);
            const dateLabel = formatDate(nb.updatedAt || nb.createdAt);

            return (
              <Card
                key={nb.id}
                hoverable
                className="group p-5 flex flex-col justify-between transition-all duration-200 hover:-translate-y-0.5 hover:border-[#5E51E8]/40 dark:hover:border-[#818CF8]/40 hover:shadow-md"
              >
                <div>
                  <div className="flex items-start gap-3 mb-3">
                    <div className="w-9 h-9 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0 transition-transform duration-200 group-hover:scale-105">
                      <NotebookPen className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <h2 className="text-sm font-semibold text-[var(--text-primary)] line-clamp-2 group-hover:text-[#5E51E8] dark:group-hover:text-[#818CF8] transition-colors">
                        {nb.title || "Sổ tay ghi chú"}
                      </h2>
                      {nb.version != null && (
                        <Badge variant="neutral" size="sm" className="mt-1">
                          Phiên bản {nb.version}
                        </Badge>
                      )}
                    </div>
                  </div>

                  <div className="flex items-center gap-1.5 text-xs text-[var(--text-secondary)] bg-[var(--surface-elevated)] rounded-[8px] px-2.5 py-1.5 mb-3">
                    {ctx.isDocumentSource ? (
                      <FileText className="w-3.5 h-3.5 text-[var(--text-tertiary)] shrink-0" />
                    ) : (
                      <BookOpen className="w-3.5 h-3.5 text-[var(--text-tertiary)] shrink-0" />
                    )}
                    <span className="truncate">{ctx.sourceLabel}</span>
                  </div>
                </div>

                <div className="flex items-center justify-between gap-2 pt-3 border-t border-[var(--border-calm)]">
                  <span className="text-xs text-[var(--text-tertiary)]">
                    {dateLabel ? `Cập nhật ${dateLabel}` : ""}
                  </span>
                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={() => handleOpenNotebook(nb)}
                  >
                    Mở ghi chú
                  </Button>
                </div>
              </Card>
            );
          })}
        </div>
      ) : (
        <EmptyState
          icon={NotebookPen}
          title={searchQuery ? "Không tìm thấy sổ tay phù hợp" : "Chưa có sổ tay ghi chú nào"}
          description={
            searchQuery
              ? `Không có sổ tay nào khớp với "${searchQuery}". Thử từ khóa khác hoặc xóa bộ lọc.`
              : "Sổ tay sẽ tự động được tạo và đồng bộ khi bạn ghi chú song song trong lúc học bài giảng hoặc đọc tài liệu PDF."
          }
          action={
            searchQuery
              ? {
                  label: "Xóa lọc",
                  onClick: () => setSearchQuery(""),
                }
              : {
                  label: "Mở bài học để ghi chú",
                  href: "/app/courses",
                  icon: <BookOpen className="w-4 h-4" />,
                }
          }
        />
      )}
    </div>
  );
}
