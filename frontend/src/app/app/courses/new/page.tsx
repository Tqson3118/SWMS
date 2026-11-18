"use client";

import React, { useRef, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  AlertCircle,
  BookOpen,
  ChevronRight,
  FileUp,
  Loader2,
  Package,
  Download,
  Sparkles,
  Upload,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { Button, Card } from "@/components/ui";

interface PreviewChapter {
  title: string;
  lessons?: { title: string; summary?: string }[];
}

interface WizardPreviewData {
  title?: string;
  description?: string;
  chapters?: PreviewChapter[];
}

export default function NewCoursePage() {
  const router = useRouter();
  const { activeUserHeaderId } = useWorkspaceStore();

  const [uploadLoading, setUploadLoading] = useState(false);
  const [aiLoading, setAiLoading] = useState(false);
  const [importLoading, setImportLoading] = useState(false);
  const [aiTopic, setAiTopic] = useState("");
  const [previewData, setPreviewData] = useState<WizardPreviewData | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const uploadInputRef = useRef<HTMLInputElement>(null);
  const importInputRef = useRef<HTMLInputElement>(null);

  const getHeaders = () => {
    const effectiveUserId =
      activeUserHeaderId ??
      (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
    return getClientRequestHeaders(
      effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
    );
  };

  const navigateToCreatedCourse = (data: { courseId?: string; id?: string }) => {
    const newId = data.courseId || data.id || "";
    if (!newId) {
      throw new Error("Máy chủ không trả về mã khóa học vừa tạo.");
    }
    router.push(`/app/courses/${newId}`);
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploadLoading(true);
    setErrorMessage(null);
    try {
      const formData = new FormData();
      formData.append("file", file);

      if (file.name.toLowerCase().endsWith(".zip") || file.name.toLowerCase().endsWith(".swm")) {
        const importRes = await fetch("/api/v1/courses/import-zip", {
          method: "POST",
          headers: getHeaders(),
          body: formData,
        });
        if (!importRes.ok) {
          const errJson = await importRes.json().catch(() => ({}));
          throw new Error(errJson.error || `Nhập gói ZIP thất bại (Mã lỗi ${importRes.status})`);
        }
        const data = await importRes.json();
        navigateToCreatedCourse(data);
        return;
      }

      formData.append("title", file.name.replace(/\.[^/.]+$/, ""));
      const uploadRes = await fetch("/api/v1/documents/upload", {
        method: "POST",
        headers: getHeaders(),
        body: formData,
      });

      if (!uploadRes.ok) {
        const errJson = await uploadRes.json().catch(() => ({}));
        throw new Error(errJson.error || `Tải tài liệu thất bại (Mã lỗi ${uploadRes.status})`);
      }

      const uploadData = await uploadRes.json();
      let createdCourse = uploadData.generatedCourse;

      if (!createdCourse && uploadData.documentId) {
        const genRes = await fetch(`/api/v1/documents/${uploadData.documentId}/generate-course`, {
          method: "POST",
          headers: getHeaders(),
        });
        if (!genRes.ok) {
          const errJson = await genRes.json().catch(() => ({}));
          throw new Error(
            errJson.error || `Sinh khóa học từ tài liệu thất bại (Mã lỗi ${genRes.status})`
          );
        }
        createdCourse = await genRes.json();
      }

      navigateToCreatedCourse(createdCourse || {});
    } catch (err) {
      setErrorMessage(
        err instanceof Error ? err.message : "Đã xảy ra lỗi khi tải tài liệu lên."
      );
    } finally {
      setUploadLoading(false);
      if (uploadInputRef.current) uploadInputRef.current.value = "";
    }
  };

  const requestPreview = async (cleanTopic: string) => {
    const previewRes = await fetch("/api/v1/courses/generate-wizard/preview", {
      method: "POST",
      headers: {
        ...getHeaders(),
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        topic: cleanTopic,
        answers: {},
      }),
    });
    if (!previewRes.ok) {
      const errJson = await previewRes.json().catch(() => ({}));
      throw new Error(
        errJson.error || `Không thể sinh đề cương (Mã lỗi ${previewRes.status})`
      );
    }
    return (await previewRes.json()) as WizardPreviewData;
  };

  const commitWizardCourse = async (cleanTopic: string, preview: WizardPreviewData) => {
    const commitRes = await fetch("/api/v1/courses/generate-wizard/commit", {
      method: "POST",
      headers: {
        ...getHeaders(),
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        topic: cleanTopic,
        title: preview.title || `Khóa học: ${cleanTopic}`,
        description: preview.description || `Lộ trình học tập ${cleanTopic}`,
        chapters: preview.chapters || [],
        generateFlashcards: true,
        generateQuiz: true,
      }),
    });

    if (!commitRes.ok) {
      const errJson = await commitRes.json().catch(() => ({}));
      throw new Error(errJson.error || `Lưu khóa học thất bại (Mã lỗi ${commitRes.status})`);
    }

    const data = await commitRes.json();
    navigateToCreatedCourse(data);
  };

  const handleAiPreview = async () => {
    const cleanTopic = aiTopic.trim();
    if (!cleanTopic) return;
    setAiLoading(true);
    setErrorMessage(null);
    try {
      const preview = await requestPreview(cleanTopic);
      setPreviewData(preview);
    } catch (err) {
      setErrorMessage(
        err instanceof Error ? err.message : "Không thể tạo bản xem trước đề cương."
      );
    } finally {
      setAiLoading(false);
    }
  };

  const handleAiCreate = async () => {
    const cleanTopic = aiTopic.trim();
    if (!cleanTopic) return;
    setAiLoading(true);
    setErrorMessage(null);
    try {
      const preview = previewData || (await requestPreview(cleanTopic));
      await commitWizardCourse(cleanTopic, preview);
    } catch (err) {
      setErrorMessage(
        err instanceof Error ? err.message : "Đã xảy ra lỗi khi khởi tạo khóa học."
      );
    } finally {
      setAiLoading(false);
    }
  };

  const handleImportSwm = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setImportLoading(true);
    setErrorMessage(null);
    try {
      const formData = new FormData();
      formData.append("file", file);

      const res = await fetch("/api/v1/courses/import-zip", {
        method: "POST",
        headers: getHeaders(),
        body: formData,
      });

      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        throw new Error(errJson.error || `Nhập gói SWM/ZIP thất bại (Mã lỗi ${res.status})`);
      }

      const data = await res.json();
      navigateToCreatedCourse(data);
    } catch (err) {
      setErrorMessage(
        err instanceof Error ? err.message : "Đã xảy ra lỗi khi nhập gói khóa học."
      );
    } finally {
      setImportLoading(false);
      if (importInputRef.current) importInputRef.current.value = "";
    }
  };

  const anyLoading = uploadLoading || aiLoading || importLoading;

  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pb-24 md:pb-10">
      {/* Breadcrumb */}
      <nav aria-label="Breadcrumb" className="flex items-center gap-2 text-sm text-[var(--text-tertiary)] mb-6">
        <Link href="/app/courses" className="hover:text-[var(--text-primary)] transition">
          Khóa học
        </Link>
        <span>·</span>
        <span className="text-[var(--text-primary)] font-medium">Tạo khóa học mới</span>
      </nav>

      <div className="mb-8">
        <h1 className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">
          Tạo khóa học mới
        </h1>
        <p className="text-sm text-[var(--text-secondary)] mt-1.5">
          Chọn cách khởi tạo phù hợp từ tài liệu sẵn có, gợi ý theo chủ đề hoặc nhập gói lưu trữ.
        </p>
      </div>

      {errorMessage && (
        <div className="mb-6 rounded-[10px] border border-red-500/30 bg-red-500/5 px-4 py-3 text-sm text-red-600 dark:text-red-400 flex items-center gap-2">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      <div className="space-y-4">
        {/* Method 1: Upload Document */}
        <Card className="p-6">
          <div className="flex items-start gap-4">
            <div className="w-10 h-10 rounded-[10px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0">
              <FileUp className="w-5 h-5 text-[#5E51E8] dark:text-[#818CF8]" />
            </div>
            <div className="flex-1 min-w-0">
              <h2 className="text-base font-semibold text-[var(--text-primary)] mb-1">
                1. Tạo từ tài liệu sẵn có
              </h2>
              <p className="text-sm text-[var(--text-secondary)] mb-4">
                Tải lên giáo trình PDF, tài liệu Word, TXT hoặc Markdown để tự động chia chương và bài học.
              </p>

              <input
                ref={uploadInputRef}
                type="file"
                accept=".pdf,.doc,.docx,.txt,.md,.zip,.swm"
                onChange={handleFileUpload}
                className="hidden"
                disabled={anyLoading}
              />
              <Button
                variant="primary"
                disabled={anyLoading}
                isLoading={uploadLoading}
                onClick={() => uploadInputRef.current?.click()}
                leftIcon={<Upload className="w-4 h-4" />}
              >
                <span>{uploadLoading ? "Đang xử lý tài liệu..." : "Chọn tệp tài liệu"}</span>
              </Button>
            </div>
          </div>
        </Card>

        {/* Method 2: AI Topic Generator */}
        <Card className="p-6">
          <div className="flex items-start gap-4">
            <div className="w-10 h-10 rounded-[10px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0">
              <Sparkles className="w-5 h-5 text-[#5E51E8] dark:text-[#818CF8]" />
            </div>
            <div className="flex-1 min-w-0">
              <h2 className="text-base font-semibold text-[var(--text-primary)] mb-1">
                2. Tạo lộ trình theo chủ đề với AI
              </h2>
              <p className="text-sm text-[var(--text-secondary)] mb-4">
                Nhập tên môn học, kỹ năng hoặc mục tiêu bạn muốn chinh phục để hệ thống thiết kế đề cương chi tiết.
              </p>

              <div className="flex flex-col sm:flex-row gap-2">
                <input
                  type="text"
                  value={aiTopic}
                  onChange={(e) => {
                    setAiTopic(e.target.value);
                    if (previewData) setPreviewData(null);
                  }}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") handleAiCreate();
                  }}
                  placeholder="Ví dụ: Thiết kế hệ thống Backend, Tiếng Anh giao tiếp..."
                  disabled={anyLoading}
                  className="flex-1 h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8]"
                />
                <div className="flex items-center gap-2">
                  <Button
                    variant="secondary"
                    disabled={anyLoading || !aiTopic.trim()}
                    onClick={handleAiPreview}
                    className="shrink-0"
                  >
                    Xem trước
                  </Button>
                  <Button
                    variant="primary"
                    disabled={anyLoading || !aiTopic.trim()}
                    isLoading={aiLoading}
                    onClick={handleAiCreate}
                    className="shrink-0"
                  >
                    <span>{previewData ? "Lưu & Mở khóa học" : "Tạo khóa học"}</span>
                  </Button>
                </div>
              </div>

              {previewData && (
                <div className="mt-4 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] p-4">
                  <div className="flex items-center gap-2 text-xs font-semibold text-[#5E51E8] dark:text-[#818CF8] mb-1">
                    <BookOpen className="w-3.5 h-3.5" />
                    <span>Bản xem trước đề cương</span>
                  </div>
                  <h3 className="text-sm font-semibold text-[var(--text-primary)] mb-1">
                    {previewData.title || aiTopic}
                  </h3>
                  {previewData.description && (
                    <p className="text-xs text-[var(--text-secondary)] mb-3">{previewData.description}</p>
                  )}
                  {Array.isArray(previewData.chapters) && previewData.chapters.length > 0 && (
                    <div className="space-y-2 max-h-56 overflow-y-auto swms-thin-scrollbar pr-1">
                      {previewData.chapters.map((ch, idx) => (
                        <div key={idx} className="text-xs rounded-[8px] bg-[var(--surface-card)] p-2.5 border border-[var(--border-calm)]">
                          <p className="font-semibold text-[var(--text-primary)] mb-1">
                            Chương {idx + 1}: {ch.title}
                          </p>
                          {Array.isArray(ch.lessons) && ch.lessons.length > 0 && (
                            <ul className="list-disc list-inside text-[var(--text-secondary)] space-y-0.5">
                              {ch.lessons.map((l, lIdx) => (
                                <li key={lIdx}>{l.title}</li>
                              ))}
                            </ul>
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        </Card>

        {/* Method 3: Import SWM ZIP */}
        <Card className="p-6">
          <div className="flex items-start gap-4">
            <div className="w-10 h-10 rounded-[10px] bg-[var(--surface-elevated)] border border-[var(--border-calm)] flex items-center justify-center shrink-0">
              <Package className="w-5 h-5 text-[var(--text-secondary)]" />
            </div>
            <div className="flex-1 min-w-0">
              <h2 className="text-base font-semibold text-[var(--text-primary)] mb-1">
                3. Nhập gói khóa học (.swm / .zip)
              </h2>
              <p className="text-sm text-[var(--text-secondary)] mb-4">
                Khôi phục khóa học đã xuất trước đó hoặc nhập giáo trình do người khác chia sẻ.
              </p>

              <input
                ref={importInputRef}
                type="file"
                accept=".zip,.swm"
                onChange={handleImportSwm}
                className="hidden"
                disabled={anyLoading}
              />
              <div className="flex flex-wrap items-center gap-2.5">
                <Button
                  variant="secondary"
                  disabled={anyLoading}
                  isLoading={importLoading}
                  onClick={() => importInputRef.current?.click()}
                  leftIcon={<Package className="w-4 h-4" />}
                >
                  <span>{importLoading ? "Đang nhập gói..." : "Chọn tệp SWM / ZIP"}</span>
                </Button>
                <a
                  href="/templates/lo-trinh-mau-swm.zip"
                  download="lo-trinh-mau-swm.zip"
                  data-testid="download-swm-template-btn"
                  className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-medium text-[var(--text-secondary)] hover:text-[#5E51E8] hover:border-[#5E51E8]/40 transition"
                >
                  <Download className="w-3.5 h-3.5" />
                  <span>Tải gói lộ trình mẫu (.zip)</span>
                </a>
              </div>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
}
