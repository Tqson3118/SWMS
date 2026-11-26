"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  AlertCircle,
  BookMarked,
  BookOpen,
  FileText,
  FolderOpen,
  RefreshCw,
  Search,
  X,
} from "lucide-react";
import {
  useWorkspaceStore,
  getClientRequestHeaders,
} from "@/stores/useWorkspaceStore";
import { fetchDashboardCached, getCachedDashboardSnapshot } from "@/lib/api-client";
import type { LibraryFilter } from "@/stores/useWorkspaceStore";
import { Button, Card, Badge, ProgressBar, EmptyState, ShowMoreFooter } from "@/components/ui";
import { deduplicateCourses, normalizeVi, sortCoursesSmartly } from "@/lib/course-helpers";

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

type LibrarySort = "recent" | "title_asc" | "progress_desc";

const getItemTitle = (item: LibraryItem): string => {
  if (item.type === "course") return item.data.title || "";
  if (item.type === "pdf") {
    return item.data.title || item.data.fileName || "";
  }
  return item.data.title || "";
};

const getItemTimestamp = (item: LibraryItem): number => {
  const raw = item.data.updatedAt || item.data.createdAt;
  if (!raw) return 0;
  const ts = Date.parse(raw);
  return Number.isFinite(ts) ? ts : 0;
};

const getCourseProgressValue = (c: CourseItem): number =>
  c.progressPercent ?? c.progress ?? 0;

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
  const [sortBy, setSortBy] = useState<LibrarySort>("recent");
  const [visibleCount, setVisibleCount] = useState(8);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const fetchData = useCallback(async (forceRefresh = false) => {
    setFetchError(null);
    const effectiveUserId =
      activeUserHeaderId ??
      (typeof window !== "undefined"
        ? window.localStorage.getItem("swms_active_user_id")
        : null);

    const applyPayload = (data: any) => {
      setCourses(sortCoursesSmartly(deduplicateCourses(data.courses ?? [])));
      setDocuments(data.documents ?? []);
      setNotebooks(data.notebooks ?? []);
      const map: Record<string, string> = {};
      for (const l of (data.lessons ?? []) as Array<{ id?: string; courseId?: string }>) {
        if (l.id && l.courseId) map[l.id] = l.courseId;
      }
      setLessonToCourse(map);
    };

    const snapshot = !forceRefresh
      ? getCachedDashboardSnapshot<any>({ compact: false, userId: effectiveUserId }) ??
        getCachedDashboardSnapshot<any>({ compact: true, userId: effectiveUserId })
      : null;

    if (snapshot) {
      applyPayload(snapshot);
      setIsLoading(false);
    } else {
      setIsLoading(true);
    }

    try {
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );
      const data = await fetchDashboardCached({
        forceRefresh,
        headers: Object.keys(headers).length > 0 ? headers : undefined
      });
      if (!data) {
        if (!snapshot) setFetchError("Không tải được dữ liệu thư viện. Vui lòng thử lại.");
        return;
      }
      applyPayload(data);
    } catch {
      if (!snapshot) setFetchError("Không thể kết nối tới máy chủ. Vui lòng thử lại.");
    } finally {
      setIsLoading(false);
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    fetchData(false);
  }, [fetchData]);

  useEffect(() => {
    setVisibleCount(8);
  }, [libraryFilter, searchQuery, sortBy]);

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

  // Filter + search + sort
  const filteredItems = useMemo(() => {
    let items = allItems;

    if (libraryFilter !== "all") {
      items = items.filter((item) => item.type === libraryFilter);
    }

    if (searchQuery.trim()) {
      const q = normalizeVi(searchQuery);
      items = items.filter((item) => {
        const title = getItemTitle(item);
        const category =
          item.type === "course" || item.type === "pdf"
            ? item.data.category || ""
            : "";
        return normalizeVi(title).includes(q) || normalizeVi(category).includes(q);
      });
    }

    const sorted = [...items];
    sorted.sort((a, b) => {
      if (sortBy === "title_asc") {
        return getItemTitle(a).localeCompare(getItemTitle(b), "vi");
      }
      if (sortBy === "progress_desc") {
        const aIsCourse = a.type === "course";
        const bIsCourse = b.type === "course";
        if (aIsCourse !== bIsCourse) {
          return aIsCourse ? -1 : 1;
        }
        if (aIsCourse && bIsCourse) {
          const progressDiff =
            getCourseProgressValue(b.data) - getCourseProgressValue(a.data);
          if (progressDiff !== 0) return progressDiff;
        }
        return getItemTimestamp(b) - getItemTimestamp(a);
      }
      return getItemTimestamp(b) - getItemTimestamp(a);
    });

    return sorted;
  }, [allItems, libraryFilter, searchQuery, sortBy]);

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
        <div className="h-10 w-48 rounded-lg swms-skeleton" />
        <div className="flex gap-4 border-b border-[var(--border-calm)] pb-2">
          {[0, 1, 2].map((i) => (
            <div key={i} className="h-7 w-28 rounded swms-skeleton" />
          ))}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[0, 1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="h-44 rounded-xl swms-skeleton" />
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
            Tổng hợp toàn bộ khóa học, tài liệu PDF và sổ tay ghi chú của bạn.
          </p>
        </div>
        <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-2 w-full sm:w-auto">
          <div className="relative w-full sm:w-64">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-[var(--text-tertiary)] pointer-events-none" />
            <input
              type="text"
              placeholder="Tìm kiếm tài liệu..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full h-9 pl-9 pr-8 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8] transition"
            />
            {searchQuery.trim().length > 0 && (
              <button
                type="button"
                onClick={() => setSearchQuery("")}
                aria-label="Xóa tìm kiếm"
                className="absolute right-2.5 top-1/2 -translate-y-1/2 p-0.5 rounded text-[var(--text-tertiary)] hover:text-[var(--text-primary)] transition cursor-pointer"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            )}
          </div>
          <select
            aria-label="Sắp xếp thư viện"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value as LibrarySort)}
            className="h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-sm text-[var(--text-primary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8] transition cursor-pointer"
          >
            <option value="recent">Mới cập nhật</option>
            <option value="title_asc">Tên A → Z</option>
            <option value="progress_desc">Tiến độ cao nhất</option>
          </select>
        </div>
      </div>

      {fetchError && (
        <div className="mb-6 rounded-xl border border-red-500/30 bg-red-500/5 px-4 py-3 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div className="flex items-center gap-2 text-sm text-red-600 dark:text-red-400">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{fetchError}</span>
          </div>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => void fetchData(true)}
            leftIcon={<RefreshCw className="w-3.5 h-3.5" />}
            className="self-start sm:self-auto shrink-0"
          >
            Thử lại
          </Button>
        </div>
      )}

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
        fetchError ? null : (
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
        )
      ) : (
        <>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredItems.slice(0, visibleCount).map((item) => {
            if (item.type === "course") {
              const course = item.data as CourseItem;
              const id = getCourseId(course);
              const progress = getProgress(course);
              return (
                <Link
                  key={`course-${id}`}
                  href={`/app/courses/${id}`}
                  className="swms-cv-card block"
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
                            Hoàn thành
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

        <ShowMoreFooter
          testIdPrefix="library"
          shownCount={Math.min(visibleCount, filteredItems.length)}
          totalCount={filteredItems.length}
          stepSize={8}
          itemLabel="mục"
          onShowMore={() => setVisibleCount((v) => v + 8)}
          onShowAll={() => setVisibleCount(filteredItems.length)}
        />
        </>
      )}
    </div>
  );
}
