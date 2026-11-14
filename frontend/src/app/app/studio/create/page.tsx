"use client";

import React, { useRef, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  ArrowLeft,
  Loader2,
  Package,
  Sparkles,
  Upload,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";

export default function CreateCoursePage() {
  const router = useRouter();
  const { activeUserHeaderId } = useWorkspaceStore();

  const [uploadLoading, setUploadLoading] = useState(false);
  const [aiLoading, setAiLoading] = useState(false);
  const [importLoading, setImportLoading] = useState(false);
  const [aiTopic, setAiTopic] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const uploadInputRef = useRef<HTMLInputElement>(null);
  const importInputRef = useRef<HTMLInputElement>(null);

  const getHeaders = () => {
    const effectiveUserId =
      activeUserHeaderId ??
      (typeof window !== "undefined"
        ? window.localStorage.getItem("swms_active_user_id")
        : null);
    return getClientRequestHeaders(
      effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
    );
  };

  const navigateToCreatedCourse = (data: { courseId?: string; id?: string }) => {
    const newId = data.courseId || data.id || "";
    if (!newId) {
      throw new Error("Máy chủ không trả về mã khóa học vừa tạo.");
    }
    router.push(`/app/studio/${newId}`);
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
          throw new Error(errJson.error || `Sinh khóa học từ tài liệu thất bại (Mã lỗi ${genRes.status})`);
        }
        createdCourse = await genRes.json();
      }

      navigateToCreatedCourse(createdCourse || {});
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : "Đã xảy ra lỗi khi tải tài liệu lên.");
    } finally {
      setUploadLoading(false);
      if (uploadInputRef.current) uploadInputRef.current.value = "";
    }
  };

  const handleAiWizard = async () => {
    if (!aiTopic.trim()) return;
    setAiLoading(true);
    setErrorMessage(null);
    try {
      const cleanTopic = aiTopic.trim();
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
        throw new Error(errJson.error || `Không thể sinh dàn ý AI (Mã lỗi ${previewRes.status})`);
      }

      const preview = await previewRes.json();
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
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : "Đã xảy ra lỗi khi khởi tạo khóa học AI.");
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
      setErrorMessage(err instanceof Error ? err.message : "Đã xảy ra lỗi khi nhập gói khóa học SWM.");
    } finally {
      setImportLoading(false);
      if (importInputRef.current) importInputRef.current.value = "";
    }
  };

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pb-20 md:pb-8">
      {/* Header */}
      <div className="flex items-center gap-3 mb-6">
        <Link
          href="/app/studio"
          className="flex items-center justify-center w-8 h-8 rounded-md hover:bg-bg-tertiary transition text-text-secondary"
        >
          <ArrowLeft className="w-4 h-4" />
        </Link>
        <h1 className="text-2xl font-bold text-text-primary">
          Tạo khóa học mới
        </h1>
      </div>

      {errorMessage && (
        <div
          role="alert"
          className="mb-6 rounded-lg border border-rose-300 bg-rose-50 dark:bg-rose-950/50 dark:border-rose-900 px-4 py-3 text-sm text-rose-800 dark:text-rose-200"
        >
          {errorMessage}
        </div>
      )}

      {/* 3 Method Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        {/* Card 1: Upload PDF / DOCX / MD */}
        <div className="rounded-lg border border-border bg-bg-secondary p-6 flex flex-col items-center text-center">
          <div className="w-12 h-12 rounded-lg bg-bg-tertiary flex items-center justify-center mb-4">
            <Upload className="w-6 h-6 text-accent" />
          </div>
          <h3 className="text-base font-semibold text-text-primary mb-1">
            Tải tài liệu lên
          </h3>
          <p className="text-sm text-text-secondary mb-4">
            Upload PDF, DOCX, Markdown hoặc TXT để dựng khóa học tự động
          </p>
          <label
            className={`inline-flex items-center gap-2 h-9 px-4 rounded-md text-sm font-medium transition cursor-pointer ${
              uploadLoading
                ? "bg-bg-tertiary text-text-tertiary cursor-not-allowed"
                : "bg-accent text-white hover:bg-accent-hover"
            }`}
          >
            {uploadLoading ? (
              <Loader2 className="w-4 h-4 animate-spin" />
            ) : (
              <Upload className="w-4 h-4" />
            )}
            <span>{uploadLoading ? "Đang xử lý..." : "Chọn file"}</span>
            <input
              ref={uploadInputRef}
              type="file"
              accept=".pdf,.docx,.md,.txt,.zip"
              className="hidden"
              onChange={handleFileUpload}
              disabled={uploadLoading}
            />
          </label>
        </div>

        {/* Card 2: AI Wizard */}
        <div className="rounded-lg border border-border bg-bg-secondary p-6 flex flex-col items-center text-center">
          <div className="w-12 h-12 rounded-lg bg-bg-tertiary flex items-center justify-center mb-4">
            <Sparkles className="w-6 h-6 text-accent" />
          </div>
          <h3 className="text-base font-semibold text-text-primary mb-1">
            AI Wizard
          </h3>
          <p className="text-sm text-text-secondary mb-4">
            Tạo khóa học với trợ lý AI
          </p>
          <div className="w-full space-y-2">
            <input
              type="text"
              value={aiTopic}
              onChange={(e) => setAiTopic(e.target.value)}
              placeholder="Nhập chủ đề..."
              className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-text-primary text-sm placeholder:text-text-tertiary focus:outline-none focus:border-accent transition"
              onKeyDown={(e) => {
                if (e.key === "Enter") handleAiWizard();
              }}
            />
            <button
              type="button"
              onClick={handleAiWizard}
              disabled={aiLoading || !aiTopic.trim()}
              className={`w-full inline-flex items-center justify-center gap-2 h-9 rounded-md text-sm font-medium transition cursor-pointer ${
                aiLoading || !aiTopic.trim()
                  ? "bg-bg-tertiary text-text-tertiary cursor-not-allowed"
                  : "bg-accent text-white hover:bg-accent-hover"
              }`}
            >
              {aiLoading ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : (
                <Sparkles className="w-4 h-4" />
              )}
              <span>{aiLoading ? "Đang tạo..." : "Tạo khóa học"}</span>
            </button>
          </div>
        </div>

        {/* Card 3: Import SWM */}
        <div className="rounded-lg border border-border bg-bg-secondary p-6 flex flex-col items-center text-center">
          <div className="w-12 h-12 rounded-lg bg-bg-tertiary flex items-center justify-center mb-4">
            <Package className="w-6 h-6 text-accent" />
          </div>
          <h3 className="text-base font-semibold text-text-primary mb-1">
            Import SWM
          </h3>
          <p className="text-sm text-text-secondary mb-4">
            Nhập từ file .zip hoặc .swm chuẩn SWM
          </p>
          <div className="flex flex-col items-center gap-2.5 w-full">
            <label
              className={`inline-flex items-center gap-2 h-9 px-4 rounded-md text-sm font-medium transition cursor-pointer ${
                importLoading
                  ? "bg-bg-tertiary text-text-tertiary cursor-not-allowed"
                  : "bg-accent text-white hover:bg-accent-hover"
              }`}
            >
              {importLoading ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : (
                <Package className="w-4 h-4" />
              )}
              <span>{importLoading ? "Đang nhập..." : "Chọn file"}</span>
              <input
                ref={importInputRef}
                type="file"
                accept=".swm,.zip"
                className="hidden"
                onChange={handleImportSwm}
                disabled={importLoading}
              />
            </label>
            <a
              href="/templates/lo-trinh-mau-swm.zip"
              download="lo-trinh-mau-swm.zip"
              className="text-xs font-medium text-accent hover:underline"
            >
              Tải gói lộ trình mẫu (.zip)
            </a>
          </div>
        </div>
      </div>
    </div>
  );
}
