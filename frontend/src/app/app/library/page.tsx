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
import { fetchDashboardCached } from "@/lib/api-client";
import type { LibraryFilter } from "@/stores/useWorkspaceStore";
import { Button, Card, Badge, ProgressBar, EmptyState } from "@/components/ui";
import { deduplicateCourses, sortCoursesSmartly } from "@/lib/course-helpers";

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
      const data = await fetchDashboardCached({
        headers: effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      });
      if (data) {
        setCourses(sortCoursesSmartly(deduplicateCourses(data.courses ?? [])));
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
      <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-8 min-h-[80vh] space-y-6">
        <div className="h-10 w-48 rounded-lg bg-[var(--surface-elevated)] animate-pulse" />
        <div className="flex gap-4 border-b border-[var(--border-calm)] pb-2">
          {[0, 1, 2].map((i) => (
            <div key={i} className="h-7 w-28 rounded bg-[var(--surface-elevated)] animate-pulse" />
          ))}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[0, 1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="h-44 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-8 min-h-[80vh]">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">Thư viện</h1>
          <p className="text-sm text-[var(--text-secondary)] mt-1">
            Tổng hợp toàn bộ khóa học, tài liệu đọc và sổ tay ghi chú của bạn
          </p>
        </div>
        <div className="relative w-full sm:w-72">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-[var(--text-tertiary)] pointer-events-none" />
          <input
            type="text"
            placeholder="Tìm kiếm tài liệu..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full h-9 pl-9 pr-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8] transition"
          />
        </div>
      </div>

      {/* Filter tabs */}
      <div className="flex flex-wrap sm:flex-nowrap items-center gap-1 sm:gap-1.5 mb-6 border-b border-[var(--border-calm)] max-w-full overflow-x-auto no-scrollbar">
        {FILTER_TABS.map((tab) => (
          <button
            key={tab.key}
            type="button"
            data-testid={`library-filter-${tab.key}`}
            onClick={() => setLibraryFilter(tab.key)}
            className={`px-2.5 sm:px-3.5 py-2 text-xs sm:text-sm font-medium transition cursor-pointer -mb-px whitespace-nowrap flex items-center gap-1 sm:gap-1.5 shrink-0 ${
              libraryFilter === tab.key
                ? "text-[#5E51E8] dark:text-[#818CF8] border-b-2 border-[#5E51E8] dark:border-[#818CF8] font-semibold"
                : "text-[var(--text-tertiary)] hover:text-[var(--text-secondary)]"
            }`}
          >
            <span>{tab.label}</span>
            <span className="text-[10px] sm:text-[11px] px-1.5 py-0.5 rounded-[5px] bg-[var(--surface-elevated)] text-[var(--text-secondary)]">
              {countsByType[tab.key]}
            </span>
          </button>
        ))}
      </div>

      {/* Content */}
      {filteredItems.length === 0 ? (
        <EmptyState
          icon={FolderOpen}
          title={searchQuery.trim() ? "Không tìm thấy tài liệu phù hợp" : "Chưa có tài liệu nào"}
          description={
            searchQuery.trim()
              ? "Thử tìm bằng từ khóa khác hoặc chuyển sang danh mục khác."
              : "Hãy tạo khóa học hoặc tải tài liệu PDF lên để bắt đầu học tập."
          }
          action={
            searchQuery.trim()
              ? {
                  label: "Xóa tìm kiếm",
                  onClick: () => setSearchQuery(""),
                }
              : {
                  label: "Tạo khóa học mới",
                  href: "/app/courses/new",
                }
          }
        />
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
                  className="block"
                >
                  <Card hoverable className="p-4 flex flex-col justify-between h-full">
                    <div>
                      <div className="flex items-start gap-3 mb-3">
                        <div className="w-9 h-9 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] flex items-center justify-center shrink-0 border border-[#5E51E8]/20">
                          <BookOpen className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
                        </div>
                        <div className="min-w-0 flex-1">
                          <h3 className="text-sm font-semibold text-[var(--text-primary)] hover:text-[#5E51E8] transition line-clamp-2">
                            {course.title}
                          </h3>
                          {course.category && (
                            <Badge variant="neutral" size="sm" className="mt-1">
                              {course.category}
                            </Badge>
                          )}
                        </div>
                      </div>
                    </div>
                    {/* Progress bar */}
                    <div className="pt-2">
                      <div className="flex items-center justify-between text-xs text-[var(--text-tertiary)] mb-1.5">
                        <span>{progress}% hoàn thành</span>
                        {progress === 100 && (
                          <Badge variant="success" size="sm">
                            ✓ Xong
                          </Badge>
                        )}
                      </div>
                      <ProgressBar
                        value={progress}
                        color={progress === 100 ? "success" : "brand"}
                        size="sm"
                      />
                    </div>
                  </Card>
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
                <div
                  key={`pdf-${docId}`}
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
                  className="cursor-pointer"
                >
                  <Card hoverable className="p-4 flex flex-col justify-between h-full">
                    <div>
                      <div className="flex items-start gap-3 mb-3">
                        <div className="w-9 h-9 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] flex items-center justify-center shrink-0 border border-[#5E51E8]/20">
                          <FileText className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
                        </div>
                        <div className="min-w-0 flex-1">
                          <h3 className="text-sm font-semibold text-[var(--text-primary)] hover:text-[#5E51E8] transition line-clamp-2">
                            {doc.title || doc.fileName || "Tài liệu PDF"}
                          </h3>
                          {doc.category && (
                            <Badge variant="neutral" size="sm" className="mt-1">
                              {doc.category}
                            </Badge>
                          )}
                        </div>
                      </div>
                    </div>
                    {(doc.createdAt || doc.updatedAt) && (
                      <p className="text-xs text-[var(--text-tertiary)] pt-2 border-t border-[var(--border-calm)]">
                        {formatDate(doc.updatedAt || doc.createdAt)}
                      </p>
                    )}
                  </Card>
                </div>
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
              <div
                key={`notebook-${nb.id}`}
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
                className="cursor-pointer"
              >
                <Card hoverable className="p-4 flex flex-col justify-between h-full">
                  <div>
                    <div className="flex items-start gap-3 mb-3">
                      <div className="w-9 h-9 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] flex items-center justify-center shrink-0 border border-[#5E51E8]/20">
                        <BookMarked className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
                      </div>
                      <div className="min-w-0 flex-1">
                        <h3 className="text-sm font-semibold text-[var(--text-primary)] hover:text-[#5E51E8] transition line-clamp-2">
                          {nb.title}
                        </h3>
                        {nb.version != null && (
                          <Badge variant="neutral" size="sm" className="mt-1">
                            Phiên bản v{nb.version}
                          </Badge>
                        )}
                      </div>
                    </div>
                  </div>
                  {(nb.updatedAt || nb.createdAt) && (
                    <p className="text-xs text-[var(--text-tertiary)] pt-2 border-t border-[var(--border-calm)]">
                      {formatDate(nb.updatedAt || nb.createdAt)}
                    </p>
                  )}
                </Card>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
