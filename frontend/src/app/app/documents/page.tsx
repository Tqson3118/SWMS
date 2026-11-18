"use client";

import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import {
  AlertCircle,
  BookOpen,
  FileText,
  Loader2,
  RefreshCw,
  Search,
  Upload,
  X,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached } from "@/lib/api-client";
import { Button, Card, Badge, EmptyState } from "@/components/ui";
import { getCourseId, normalizeVi, type CourseLike } from "@/lib/course-helpers";

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

  const handleUploadPdf = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
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

  const filteredDocuments = useMemo(() => {
    const q = normalizeVi(searchQuery);
    if (!q) return documents;
    return documents.filter((doc) => {
      const text = normalizeVi(
        `${doc.title || ""} ${doc.fileName || ""} ${doc.category || ""}`
      );
      return text.includes(q);
    });
  }, [documents, searchQuery]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pb-24 md:pb-10">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">
            Tài liệu
          </h1>
          <p className="text-sm text-[var(--text-secondary)] mt-1">
            {documents.length} tài liệu PDF dùng cho việc đọc và trích dẫn kiến thức.
          </p>
        </div>

        <div>
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

      {/* Search bar */}
      <div className="relative mb-6">
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
            className="absolute right-2.5 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer"
          >
            <X className="w-3.5 h-3.5" />
          </button>
        )}
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
                className="p-5 flex flex-col justify-between"
              >
                <div>
                  <div className="flex items-start gap-3 mb-3">
                    <div className="w-9 h-9 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0">
                      <FileText className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <h2 className="text-sm font-semibold text-[var(--text-primary)] line-clamp-2">
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
              ? "Thử tìm bằng từ khóa khác."
              : "Tải tệp PDF lên để đọc, đánh dấu ý chính và ghi chú song song."
          }
          action={
            searchQuery
              ? {
                  label: "Xóa tìm kiếm",
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
