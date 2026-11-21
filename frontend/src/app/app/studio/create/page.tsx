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
import { Button, Card } from "@/components/ui";

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
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh]">
      {/* Header */}
      <div className="flex items-center gap-3 mb-6">
        <Link
          href="/app/studio"
          className="flex items-center justify-center w-8 h-8 rounded-[8px] hover:bg-[var(--surface-elevated)] transition text-[var(--text-secondary)]"
        >
          <ArrowLeft className="w-4 h-4" />
        </Link>
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">
            Tạo khóa học mới
          </h1>
          <p className="text-xs text-[var(--text-secondary)] mt-0.5">
            Chọn phương thức khởi tạo thuận tiện nhất cho nội dung bài giảng của bạn
          </p>
        </div>
      </div>

      {errorMessage && (
        <div
          role="alert"
          className="mb-6 rounded-[10px] border border-rose-300 bg-rose-50 dark:bg-rose-950/50 dark:border-rose-900 px-4 py-3 text-sm text-rose-800 dark:text-rose-200"
        >
          {errorMessage}
        </div>
      )}

      {/* 3 Method Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        {/* Card 1: Upload PDF / DOCX / MD */}
        <Card className="p-6 flex flex-col items-center text-center justify-between">
          <div>
            <div className="w-12 h-12 rounded-[12px] bg-[#EEF0FD] dark:bg-[#1E1B4B] flex items-center justify-center mb-4 mx-auto border border-[#5E51E8]/20">
              <Upload className="w-6 h-6 text-[#5E51E8] dark:text-[#818CF8]" />
            </div>
            <h3 className="text-base font-semibold text-[var(--text-primary)] mb-1">
              Tải tài liệu lên
            </h3>
            <p className="text-sm text-[var(--text-secondary)] mb-4">
              Upload PDF, DOCX, Markdown hoặc TXT để dựng khóa học tự động
            </p>
          </div>
          <div className="w-full">
            <Button
              variant="primary"
              className="w-full"
              isLoading={uploadLoading}
              disabled={uploadLoading}
              onClick={() => uploadInputRef.current?.click()}
              leftIcon={<Upload className="w-4 h-4" />}
            >
              <span>{uploadLoading ? "Đang xử lý..." : "Chọn file"}</span>
            </Button>
            <input
              ref={uploadInputRef}
              type="file"
              accept=".pdf,.docx,.md,.txt,.zip"
              className="hidden"
              onChange={handleFileUpload}
              disabled={uploadLoading}
            />
          </div>
        </Card>

        {/* Card 2: AI Wizard */}
        <Card className="p-6 flex flex-col items-center text-center justify-between">
          <div className="w-full">
            <div className="w-12 h-12 rounded-[12px] bg-[#EEF0FD] dark:bg-[#1E1B4B] flex items-center justify-center mb-4 mx-auto border border-[#5E51E8]/20">
              <Sparkles className="w-6 h-6 text-[#5E51E8] dark:text-[#818CF8]" />
            </div>
            <h3 className="text-base font-semibold text-[var(--text-primary)] mb-1">
              AI Wizard
            </h3>
            <p className="text-sm text-[var(--text-secondary)] mb-4">
              Tạo khóa học với trợ lý AI
            </p>
            <input
              type="text"
              value={aiTopic}
              onChange={(e) => setAiTopic(e.target.value)}
              placeholder="Nhập chủ đề..."
              className="w-full h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-[var(--text-primary)] text-sm placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8] transition mb-2"
              onKeyDown={(e) => {
                if (e.key === "Enter") handleAiWizard();
              }}
            />
          </div>
          <div className="w-full">
            <Button
              variant="primary"
              className="w-full"
              isLoading={aiLoading}
              disabled={aiLoading || !aiTopic.trim()}
              onClick={handleAiWizard}
              leftIcon={<Sparkles className="w-4 h-4" />}
            >
              <span>{aiLoading ? "Đang tạo..." : "Tạo khóa học"}</span>
            </Button>
          </div>
        </Card>

        {/* Card 3: Import SWM */}
        <Card className="p-6 flex flex-col items-center text-center justify-between">
          <div>
            <div className="w-12 h-12 rounded-[12px] bg-[var(--surface-elevated)] flex items-center justify-center mb-4 mx-auto border border-[var(--border-calm)]">
              <Package className="w-6 h-6 text-[#5E51E8] dark:text-[#818CF8]" />
            </div>
            <h3 className="text-base font-semibold text-[var(--text-primary)] mb-1">
              Import SWM
            </h3>
            <p className="text-sm text-[var(--text-secondary)] mb-4">
              Nhập từ file .zip hoặc .swm chuẩn SWM
            </p>
          </div>
          <div className="flex flex-col items-center gap-2.5 w-full">
            <Button
              variant="primary"
              className="w-full"
              isLoading={importLoading}
              disabled={importLoading}
              onClick={() => importInputRef.current?.click()}
              leftIcon={<Package className="w-4 h-4" />}
            >
              <span>{importLoading ? "Đang nhập..." : "Chọn file"}</span>
            </Button>
            <input
              ref={importInputRef}
              type="file"
              accept=".swm,.zip"
              className="hidden"
              onChange={handleImportSwm}
              disabled={importLoading}
            />
            <a
              href="/templates/lo-trinh-mau-swm.zip"
              download="lo-trinh-mau-swm.zip"
              className="text-xs font-medium text-[#5E51E8] dark:text-[#818CF8] hover:underline"
            >
              Tải gói lộ trình mẫu (.zip)
            </a>
          </div>
        </Card>
      </div>
    </div>
  );
}
