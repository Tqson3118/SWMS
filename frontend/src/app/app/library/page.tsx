"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  BookMarked,
  BookOpen,
  FileText,
  FolderOpen,
  Search,
} from "lucide-react";
import {
  useWorkspaceStore,
  getClientRequestHeaders,
} from "@/stores/useWorkspaceStore";
import type { LibraryFilter } from "@/stores/useWorkspaceStore";

interface CourseItem {
  id?: string;
  courseId?: string;
  title: string;
  category?: string;
  progressPercent?: number;
  progress?: number;
  createdAt?: string;
  updatedAt?: string;
}

interface DocumentItem {
  id?: string;
  documentId?: string;
  title: string;
  fileName?: string;
  courseId?: string;
  totalPages?: number;
  category?: string;
  createdAt?: string;
  updatedAt?: string;
}

interface NotebookItem {
  id: string;
  title: string;
  version?: number;
  primarySourceLessonId?: string | null;
  primarySourceDocumentId?: string | null;
  createdAt?: string;
  updatedAt?: string;
}

type LibraryItem =
  | { type: "course"; data: CourseItem }
  | { type: "pdf"; data: DocumentItem }
  | { type: "notebook"; data: NotebookItem };

const FILTER_TABS: { key: LibraryFilter; label: string }[] = [
  { key: "all", label: "Tất cả" },
  { key: "course", label: "Khóa học" },
  { key: "pdf", label: "PDF" },
  { key: "notebook", label: "Sổ tay" },
];

export default function LibraryPage() {
  const router = useRouter();
  const {
    activeUserHeaderId,
    libraryFilter,
    setLibraryFilter,
    setActiveDocument,
    setActiveCourseId,
    setActiveLesson,
    setActiveMode,
  } = useWorkspaceStore();
  const [courses, setCourses] = useState<CourseItem[]>([]);
  const [documents, setDocuments] = useState<DocumentItem[]>([]);
  const [notebooks, setNotebooks] = useState<NotebookItem[]>([]);
  const [lessonToCourse, setLessonToCourse] = useState<Record<string, string>>({});
  const [searchQuery, setSearchQuery] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined"
          ? window.localStorage.getItem("swms_active_user_id")
          : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );
      const resp = await fetch("/api/v1/workspace/dashboard", { headers });
      if (resp.ok) {
        const data = await resp.json();
        setCourses(data.courses ?? []);
        setDocuments(data.documents ?? []);
        setNotebooks(data.notebooks ?? []);
        const map: Record<string, string> = {};
        for (const l of (data.lessons ?? []) as Array<{ id?: string; courseId?: string }>) {
          if (l.id && l.courseId) map[l.id] = l.courseId;
        }
        setLessonToCourse(map);
      }
    } catch {
      /* silent */
    } finally {
      setIsLoading(false);
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Build unified list
  const allItems: LibraryItem[] = useMemo(() => {
    const items: LibraryItem[] = [];
    for (const c of courses) {
      items.push({ type: "course", data: c });
    }
    for (const d of documents) {
      items.push({ type: "pdf", data: d });
    }
    for (const nb of notebooks) {
      items.push({ type: "notebook", data: nb });
    }
    return items;
  }, [courses, documents, notebooks]);

  const countsByType: Record<LibraryFilter, number> = useMemo(
    () => ({
      all: allItems.length,
      course: courses.length,
      pdf: documents.length,
      lesson: 0,
      notebook: notebooks.length,
    }),
    [allItems.length, courses.length, documents.length, notebooks.length]
  );

  // Filter + search
  const filteredItems = useMemo(() => {
    let items = allItems;

    if (libraryFilter !== "all") {
      items = items.filter((item) => item.type === libraryFilter);
    }

    if (searchQuery.trim()) {
      const q = searchQuery.trim().toLowerCase();
      items = items.filter((item) => {
        const title =
          item.type === "course"
            ? item.data.title
            : item.type === "pdf"
              ? (item.data as DocumentItem).title ||
                (item.data as DocumentItem).fileName ||
                ""
              : (item.data as { title: string }).title;
        return title.toLowerCase().includes(q);
      });
    }

    return items;
  }, [allItems, libraryFilter, searchQuery]);

  const getCourseId = (c: CourseItem) => c.courseId || c.id || "";
  const getDocId = (d: DocumentItem) => d.documentId || d.id || "";
  const getProgress = (c: CourseItem) => c.progressPercent ?? c.progress ?? 0;

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return "";
    try {
      return new Date(dateStr).toLocaleDateString("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      });
    } catch {
      return "";
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pb-20 md:pb-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
        <h1 className="text-2xl font-bold text-text-primary">Thư viện</h1>
        <div className="relative w-full sm:w-72">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-text-tertiary pointer-events-none" />
          <input
            type="text"
            placeholder="Tìm kiếm tài liệu..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full h-9 pl-9 pr-3 rounded-md border border-border bg-bg-secondary text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-accent transition"
          />
        </div>
      </div>

      {/* Filter tabs */}
      <div className="flex items-center gap-1 mb-6 border-b border-border overflow-x-auto">
        {FILTER_TABS.map((tab) => (
          <button
            key={tab.key}
            type="button"
            data-testid={`library-filter-${tab.key}`}
            onClick={() => setLibraryFilter(tab.key)}
            className={`px-3 py-2 text-sm font-medium transition cursor-pointer -mb-px whitespace-nowrap flex items-center gap-1.5 ${
              libraryFilter === tab.key
                ? "text-accent border-b-2 border-accent"
                : "text-text-tertiary hover:text-text-secondary"
            }`}
          >
            <span>{tab.label}</span>
            <span className="text-[11px] px-1.5 py-0.5 rounded bg-bg-tertiary text-text-tertiary">
              {countsByType[tab.key]}
            </span>
          </button>
        ))}
      </div>

      {/* Content */}
      {filteredItems.length === 0 ? (
        /* Empty state */
        <div className="flex flex-col items-center justify-center py-20">
          <div className="w-16 h-16 rounded-lg bg-bg-tertiary flex items-center justify-center mb-4">
            <FolderOpen className="w-8 h-8 text-text-tertiary" />
          </div>
          <h3 className="text-base font-semibold text-text-primary mb-1">
            Chưa có tài liệu nào
          </h3>
          <p className="text-sm text-text-tertiary text-center max-w-xs">
            {searchQuery.trim()
              ? "Không tìm thấy tài liệu nào phù hợp với từ khóa."
              : "Hãy tạo khóa học hoặc upload PDF để bắt đầu."}
          </p>
          {!searchQuery.trim() && (
            <Link
              href="/app/studio/create"
              className="mt-4 inline-flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition"
            >
              Tạo khóa học
            </Link>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredItems.map((item) => {
            if (item.type === "course") {
              const course = item.data as CourseItem;
              const id = getCourseId(course);
              const progress = getProgress(course);
              return (
                <Link
                  key={`course-${id}`}
                  href={`/app/courses/${id}`}
                  className="group rounded-lg border border-border bg-bg-secondary p-4 hover:border-accent/50 transition"
                >
                  <div className="flex items-start gap-3 mb-3">
                    <div className="w-9 h-9 rounded-md bg-bg-tertiary flex items-center justify-center shrink-0">
                      <BookOpen className="w-4.5 h-4.5 text-text-tertiary" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <h3 className="text-sm font-medium text-text-primary group-hover:text-accent transition line-clamp-2">
                        {course.title}
                      </h3>
                      {course.category && (
                        <span className="inline-block text-xs font-medium text-text-tertiary bg-bg-tertiary rounded-md px-2 py-0.5 mt-1">
                          {course.category}
                        </span>
                      )}
                    </div>
                  </div>
                  {/* Progress bar */}
                  <div>
                    <div className="flex items-center justify-between text-xs text-text-tertiary mb-1.5">
                      <span>{progress}% hoàn thành</span>
                      {progress === 100 && (
                        <span className="text-emerald-600 dark:text-emerald-400 font-medium">
                          ✓ Xong
                        </span>
                      )}
                    </div>
                    <div className="w-full h-1 bg-bg-tertiary rounded-full overflow-hidden">
                      <div
                        className={`h-full rounded-full transition-all ${
                          progress === 100 ? "bg-emerald-500" : "bg-accent"
                        }`}
                        style={{ width: `${Math.max(0, Math.min(100, progress))}%` }}
                      />
                    </div>
                  </div>
                </Link>
              );
            }

            if (item.type === "pdf") {
              const doc = item.data as DocumentItem;
              const docId = getDocId(doc);
              const linkedCourseId =
                doc.courseId && doc.courseId !== "00000000-0000-0000-0000-000000000000"
                  ? doc.courseId
                  : null;
              return (
                <button
                  key={`pdf-${docId}`}
                  type="button"
                  onClick={() => {
                    setActiveDocument(docId, 1, doc.totalPages ?? null);
                    if (linkedCourseId) {
                      setActiveCourseId(linkedCourseId);
                      router.push(`/app/courses/${linkedCourseId}/learn?doc=${docId}`);
                    } else {
                      setActiveMode("reading");
                      router.push("/workspace");
                    }
                  }}
                  className="group text-left rounded-lg border border-border bg-bg-secondary p-4 hover:border-accent/50 transition cursor-pointer"
                >
                  <div className="flex items-start gap-3 mb-3">
                    <div className="w-9 h-9 rounded-md bg-bg-tertiary flex items-center justify-center shrink-0">
                      <FileText className="w-4.5 h-4.5 text-text-tertiary" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <h3 className="text-sm font-medium text-text-primary group-hover:text-accent transition line-clamp-2">
                        {doc.title || doc.fileName || "Tài liệu PDF"}
                      </h3>
                      {doc.category && (
                        <span className="inline-block text-xs font-medium text-text-tertiary bg-bg-tertiary rounded-md px-2 py-0.5 mt-1">
                          {doc.category}
                        </span>
                      )}
                    </div>
                  </div>
                  {(doc.createdAt || doc.updatedAt) && (
                    <p className="text-xs text-text-tertiary">
                      {formatDate(doc.updatedAt || doc.createdAt)}
                    </p>
                  )}
                </button>
              );
            }

            // notebook type
            const nb = item.data as NotebookItem;
            const nbDocCourseId = nb.primarySourceDocumentId
              ? documents.find((d) => getDocId(d) === nb.primarySourceDocumentId)?.courseId
              : undefined;
            const nbCourseId =
              (nb.primarySourceLessonId && lessonToCourse[nb.primarySourceLessonId]) ||
              (nbDocCourseId && nbDocCourseId !== "00000000-0000-0000-0000-000000000000"
                ? nbDocCourseId
                : null);

            return (
              <button
                key={`notebook-${nb.id}`}
                type="button"
                onClick={() => {
                  setActiveMode("split");
                  if (nb.primarySourceLessonId) {
                    setActiveLesson(nb.primarySourceLessonId);
                  } else if (nb.primarySourceDocumentId) {
                    setActiveDocument(nb.primarySourceDocumentId, 1, null);
                  }
                  if (nbCourseId) {
                    setActiveCourseId(nbCourseId);
                    router.push(
                      nb.primarySourceLessonId
                        ? `/app/courses/${nbCourseId}/learn?lesson=${nb.primarySourceLessonId}&split=true`
                        : `/app/courses/${nbCourseId}/learn?split=true`
                    );
                  } else {
                    router.push("/workspace");
                  }
                }}
                className="group text-left rounded-lg border border-border bg-bg-secondary p-4 hover:border-accent/50 transition cursor-pointer"
              >
                <div className="flex items-start gap-3 mb-3">
                  <div className="w-9 h-9 rounded-md bg-bg-tertiary flex items-center justify-center shrink-0">
                    <BookMarked className="w-4.5 h-4.5 text-text-tertiary" />
                  </div>
                  <div className="min-w-0 flex-1">
                    <h3 className="text-sm font-medium text-text-primary group-hover:text-accent transition line-clamp-2">
                      {nb.title}
                    </h3>
                    {nb.version != null && (
                      <span className="inline-block text-xs font-medium text-text-tertiary bg-bg-tertiary rounded-md px-2 py-0.5 mt-1">
                        Phiên bản v{nb.version}
                      </span>
                    )}
                  </div>
                </div>
                {(nb.updatedAt || nb.createdAt) && (
                  <p className="text-xs text-text-tertiary">
                    {formatDate(nb.updatedAt || nb.createdAt)}
                  </p>
                )}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
