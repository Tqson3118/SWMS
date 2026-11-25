"use client";

import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import {
  AlertCircle,
  BookOpen,
  FileText,
  RefreshCw,
  Search,
  Upload,
  X,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached } from "@/lib/api-client";
import { Button, Card, Badge, EmptyState } from "@/components/ui";
import { deduplicateByTitle, getCourseId, normalizeVi, type CourseLike } from "@/lib/course-helpers";

interface DocumentItem {
  id?: string;
  documentId?: string;
  title?: string;
  fileName?: string;
  courseId?: string | null;
  totalPages?: number | null;
  pageCount?: number | null;
  category?: string | null;
  createdAt?: string | null;
  updatedAt?: string | null;
}

const ZERO_GUID = "00000000-0000-0000-0000-000000000000";

function getDocumentId(doc: DocumentItem): string {
  return String(doc.documentId || doc.id || "");
}

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

export default function DocumentsPage() {
  const router = useRouter();
  const { activeUserHeaderId, setActiveDocument, setActiveCourseId } = useWorkspaceStore();

  const [documents, setDocuments] = useState<DocumentItem[]>([]);
  const [courseTitleById, setCourseTitleById] = useState<Record<string, string>>({});
  const [searchQuery, setSearchQuery] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isUploading, setIsUploading] = useState(false);
  const [isDraggingOver, setIsDraggingOver] = useState(false);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [uploadError, setUploadError] = useState<string | null>(null);

  const fileInputRef = useRef<HTMLInputElement>(null);

  const getHeaders = useCallback(() => {
    const effectiveUserId =
      activeUserHeaderId ??
      (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
    return getClientRequestHeaders(
      effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
    );
  }, [activeUserHeaderId]);

  const fetchDocuments = useCallback(async () => {
    setIsLoading(true);
    setFetchError(null);
    try {
      const headers = getHeaders();
      const [docsResp, dashData] = await Promise.all([
        fetch("/api/v1/documents", { headers }).catch(() => null),
        fetchDashboardCached({ headers }).catch(() => ({})),
      ]);

      const docsList = docsResp && docsResp.ok ? await docsResp.json().catch(() => null) : null;

      const resolvedDocs: DocumentItem[] =
        Array.isArray(docsList) && docsList.length > 0
          ? docsList
          : Array.isArray(dashData?.documents)
            ? dashData.documents
            : [];
      setDocuments(resolvedDocs);

      const courseMap: Record<string, string> = {};
      for (const c of (dashData.courses ?? []) as CourseLike[]) {
        const cid = getCourseId(c);
        if (cid && c.title) courseMap[cid] = c.title;
      }
      setCourseTitleById(courseMap);
    } catch {
      setFetchError("Không thể kết nối tới máy chủ.");
    } finally {
      setIsLoading(false);
    }
  }, [getHeaders]);

  useEffect(() => {
    fetchDocuments();
  }, [fetchDocuments]);

  const uploadFile = async (file: File) => {
    const ext = file.name.split(".").pop()?.toLowerCase() ?? "";
    if (!["pdf", "txt", "md", "docx"].includes(ext) && file.type !== "application/pdf") {
      setUploadError("Vui lòng chọn tệp định dạng PDF, TXT, MD hoặc DOCX.");
      return;
    }
    setIsUploading(true);
    setUploadError(null);
    try {
      const formData = new FormData();
      formData.append("file", file);
      formData.append("title", file.name.replace(/\.[^/.]+$/, ""));

      const resp = await fetch("/api/v1/documents/upload", {
        method: "POST",
        headers: getHeaders(),
        body: formData,
      });

      if (!resp.ok) {
        const err = await resp.json().catch(() => ({}));
        throw new Error(err.error || `Tải tài liệu lên thất bại (HTTP ${resp.status})`);
      }

      await fetchDocuments();
    } catch (err) {
      setUploadError(
        err instanceof Error ? err.message : "Đã xảy ra lỗi khi tải tài liệu."
      );
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  };

  const handleUploadPdf = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    await uploadFile(file);
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    if (!isDraggingOver) setIsDraggingOver(true);
  };

  const handleDragLeave = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    if (!e.currentTarget.contains(e.relatedTarget as Node)) {
      setIsDraggingOver(false);
    }
  };

  const handleDrop = async (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDraggingOver(false);
    const file = e.dataTransfer?.files?.[0];
    if (!file) return;
    await uploadFile(file);
  };

  const handleOpenDocument = (doc: DocumentItem) => {
    const docId = getDocumentId(doc);
    if (!docId) return;
    const pages = doc.totalPages ?? doc.pageCount ?? null;
    const validCourseId =
      doc.courseId && doc.courseId !== ZERO_GUID ? doc.courseId : "standalone";
    setActiveDocument(docId, 1, pages);
    if (validCourseId !== "standalone") {
      setActiveCourseId(validCourseId);
    }
    router.push(`/app/courses/${validCourseId}/learn?doc=${docId}`);
  };

  const uniqueDocuments = useMemo(() => deduplicateByTitle(documents), [documents]);

  const filteredDocuments = useMemo(() => {
    const q = normalizeVi(searchQuery);
    const sourceList = q ? documents : uniqueDocuments;
    if (!q) return sourceList;
    return sourceList.filter((doc) => {
      const text = normalizeVi(
        `${doc.title || ""} ${doc.fileName || ""} ${doc.category || ""}`
      );
      return text.includes(q);
    });
  }, [documents, uniqueDocuments, searchQuery]);

  if (isLoading) {
    return (
      <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh] space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div className="space-y-2">
            <div className="h-8 w-36 rounded-lg swms-skeleton" />
            <div className="h-4 w-72 rounded-md swms-skeleton" />
          </div>
          <div className="h-9 w-40 rounded-lg swms-skeleton" />
        </div>
        <div className="h-10 w-full rounded-[10px] swms-skeleton" />
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[0, 1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="h-44 rounded-xl swms-skeleton" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
      className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh]"
    >
      {/* Header + Dropzone */}
      <div
        className={`flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6 rounded-xl transition-all duration-200 ${
          isDraggingOver
            ? "p-4 -mx-4 border-2 border-dashed border-[#5E51E8] dark:border-[#818CF8] bg-[#EEF0FD]/70 dark:bg-[#1E1B4B]/50"
            : ""
        }`}
      >
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">
            Tài liệu
          </h1>
          <p className="text-sm text-[var(--text-secondary)] mt-1">
            {isDraggingOver
              ? "Thả tệp PDF vào đây để tải lên thư viện tài liệu..."
              : `${uniqueDocuments.length} tài liệu PDF dùng cho việc đọc và trích dẫn kiến thức${
                  documents.length > uniqueDocuments.length
                    ? ` (đã gom ${documents.length - uniqueDocuments.length} bản sao trùng).`
                    : ". Kéo thả tệp PDF vào đây để tải nhanh."
                }`}
          </p>
        </div>

        <div className="shrink-0">
          <input
            ref={fileInputRef}
            type="file"
            accept=".pdf,.txt,.md,.docx"
            onChange={handleUploadPdf}
            className="hidden"
            disabled={isUploading}
          />
          <Button
            variant="primary"
            isLoading={isUploading}
            disabled={isUploading}
            onClick={() => fileInputRef.current?.click()}
            leftIcon={<Upload className="w-4 h-4" />}
            aria-label="Tải tài liệu lên"
          >
            <span>{isUploading ? "Đang tải lên..." : "Tải tài liệu lên"}</span>
          </Button>
        </div>
      </div>

      {(fetchError || uploadError) && (
        <div className="mb-6 rounded-[10px] border border-red-500/30 bg-red-500/5 px-4 py-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 text-sm text-red-600 dark:text-red-400">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{uploadError || fetchError}</span>
          </div>
          {fetchError && (
            <Button
              variant="secondary"
              size="sm"
              onClick={fetchDocuments}
              leftIcon={<RefreshCw className="w-3.5 h-3.5" />}
            >
              Thử lại
            </Button>
          )}
        </div>
      )}

      {/* Search bar + Filtered counter */}
      <div className="flex flex-col sm:flex-row sm:items-center gap-3 mb-6">
        <div className="relative flex-1">
          <Search className="w-4 h-4 text-[var(--text-tertiary)] absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Tìm tài liệu theo tiêu đề hoặc tên tệp..."
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
              ? `${filteredDocuments.length} / ${documents.length} tài liệu`
              : `${filteredDocuments.length} tài liệu`}
          </Badge>
        </div>
      </div>

      {/* Document Grid */}
      {filteredDocuments.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredDocuments.map((doc) => {
            const docId = getDocumentId(doc);
            const pages = doc.totalPages ?? doc.pageCount;
            const courseName =
              doc.courseId && doc.courseId !== ZERO_GUID ? courseTitleById[doc.courseId] : "";
            const dateLabel = formatDate(doc.updatedAt || doc.createdAt);

            return (
              <Card
                key={docId}
                hoverable
                className="group p-5 flex flex-col justify-between transition-all duration-200 hover:-translate-y-0.5 hover:border-[#5E51E8]/40 dark:hover:border-[#818CF8]/40 hover:shadow-md"
              >
                <div>
                  <div className="flex items-start gap-3 mb-3">
                    <div className="w-9 h-9 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0 transition-transform duration-200 group-hover:scale-105">
                      <FileText className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <h2 className="text-sm font-semibold text-[var(--text-primary)] line-clamp-2 group-hover:text-[#5E51E8] dark:group-hover:text-[#818CF8] transition-colors">
                        {doc.title || doc.fileName || "Tài liệu PDF"}
                      </h2>
                      {doc.fileName && doc.title && doc.fileName !== doc.title && (
                        <p className="text-xs text-[var(--text-tertiary)] truncate mt-0.5">
                          {doc.fileName}
                        </p>
                      )}
                    </div>
                  </div>

                  {courseName && (
                    <div className="flex items-center gap-1.5 text-xs text-[var(--text-secondary)] bg-[var(--surface-elevated)] rounded-[8px] px-2.5 py-1.5 mb-3">
                      <BookOpen className="w-3.5 h-3.5 text-[var(--text-tertiary)] shrink-0" />
                      <span className="truncate">{courseName}</span>
                    </div>
                  )}
                </div>

                <div className="flex items-center justify-between gap-2 pt-3 border-t border-[var(--border-calm)]">
                  <div className="text-xs text-[var(--text-tertiary)] flex items-center gap-1.5">
                    {pages ? <span>{pages} trang</span> : null}
                    {pages && dateLabel ? <span>·</span> : null}
                    {dateLabel ? <span>{dateLabel}</span> : null}
                  </div>
                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={() => handleOpenDocument(doc)}
                  >
                    Mở đọc
                  </Button>
                </div>
              </Card>
            );
          })}
        </div>
      ) : (
        <EmptyState
          icon={FileText}
          title={searchQuery ? "Không tìm thấy tài liệu phù hợp" : "Chưa có tài liệu nào"}
          description={
            searchQuery
              ? `Không có tài liệu nào khớp với "${searchQuery}". Thử từ khóa khác hoặc xóa bộ lọc.`
              : "Kéo thả tệp PDF vào trang hoặc bấm nút bên dưới để đọc, đánh dấu ý chính và ghi chú song song."
          }
          action={
            searchQuery
              ? {
                  label: "Xóa lọc",
                  onClick: () => setSearchQuery(""),
                }
              : {
                  label: "Tải tài liệu lên",
                  onClick: () => fileInputRef.current?.click(),
                  icon: <Upload className="w-4 h-4" />,
                }
          }
        />
      )}
    </div>
  );
}
