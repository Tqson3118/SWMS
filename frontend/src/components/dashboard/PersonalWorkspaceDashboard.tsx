"use client";

import React, { useState } from "react";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";
import {
  Sparkles,
  Brain,
  Clock,
  BookOpen,
  FileText,
  PenTool,
  Target,
  ShieldCheck,
  CheckCircle2,
  Circle,
  ArrowRight,
  Zap,
  Trophy,
  Tag,
  Upload,
  FileUp,
  File,
  AlertCircle,
  Trash2,
  Columns2,
  X
} from "lucide-react";

export interface DashboardData {
  user: {
    id: string;
    fullName: string;
    email: string;
    streakDays: number;
    onboardingStep: number;
    isActivated: boolean;
  };
  reviewStation: {
    totalDue: number;
    totalCards: number;
    newCount: number;
    learningCount: number;
    reviewCount: number;
    estimatedMinutes: number;
  };
  lessons: Array<{
    id: string;
    title: string;
    slug: string;
    currentVersion: number;
    updatedAt: string;
  }>;
  documents: Array<{
    id: string;
    title: string;
    totalPages: number;
    fileSizeBytes: number;
    ocrStatus: string;
    updatedAt: string;
  }>;
  notebooks: Array<{
    id: string;
    title: string;
    version: number;
    primarySourceLessonId: string | null;
    primarySourceDocumentId: string | null;
    updatedAt: string;
  }>;
  tags: Array<{
    id: string;
    name: string;
    colorKey: string;
    cardCount: number;
  }>;
}

interface PersonalWorkspaceDashboardProps {
  data: DashboardData;
  onRefreshDashboard: () => void;
}

export function PersonalWorkspaceDashboard({
  data,
  onRefreshDashboard
}: PersonalWorkspaceDashboardProps) {
  const {
    setActiveMode,
    setActiveReviewSubTab,
    setActiveLesson,
    setActiveDocument,
    setSelectedTagFilterId
  } = useWorkspaceStore();

  const [uploadModalOpen, setUploadModalOpen] = useState(false);
  const [docTitle, setDocTitle] = useState("");
  const [docText, setDocText] = useState("");
  const [uploading, setUploading] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [fileSizeBytes, setFileSizeBytes] = useState(2450000);
  const [totalPages, setTotalPages] = useState(1);
  const [fileMimeType, setFileMimeType] = useState("application/pdf");
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const fileInputRef = React.useRef<HTMLInputElement | null>(null);

  const step = data.user.onboardingStep || 1;
  const isActivated = data.user.isActivated;

  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  const estimatePdfPages = (buffer: ArrayBuffer): number => {
    try {
      const text = new TextDecoder("latin1").decode(new Uint8Array(buffer));
      const countMatches = [...text.matchAll(/\/Count\s+(\d+)/g)];
      if (countMatches.length > 0) {
        const counts = countMatches.map((m) => parseInt(m[1], 10)).filter((n) => n > 0 && n < 50000);
        if (counts.length > 0) {
          return Math.max(...counts);
        }
      }
      const pageMatches = text.match(/\/Type\s*\/Page\b/g);
      if (pageMatches) {
        return Math.max(1, pageMatches.length);
      }
    } catch {
      // fallback
    }
    return 1;
  };

  const handleProcessFile = async (file: File) => {
    setUploadError(null);
    setSelectedFile(file);
    setFileSizeBytes(file.size);

    // Auto-fill title if empty or default
    if (!docTitle.trim() || docTitle.endsWith(".pdf") || docTitle.endsWith(".md")) {
      setDocTitle(file.name);
    }

    // Check size limit: 50MB (52,428,800 bytes)
    if (file.size > 52428800) {
      setUploadError("Dung lượng file (" + formatFileSize(file.size) + ") vượt quá hạn mức 50MB của gói Free.");
      return;
    }

    const isPdf = file.type === "application/pdf" || file.name.toLowerCase().endsWith(".pdf");
    const isMd =
      file.type === "text/markdown" ||
      file.type === "text/plain" ||
      file.name.toLowerCase().endsWith(".md") ||
      file.name.toLowerCase().endsWith(".markdown") ||
      file.name.toLowerCase().endsWith(".txt");

    if (isMd) {
      setFileMimeType("text/markdown");
      try {
        const text = await file.text();
        setDocText(text);
        const pages = Math.max(1, Math.ceil(text.length / 2500));
        setTotalPages(pages);
      } catch {
        setUploadError("Không thể đọc tệp văn bản. Vui lòng thử lại.");
      }
    } else if (isPdf) {
      setFileMimeType("application/pdf");
      try {
        const buffer = await file.arrayBuffer();
        const pages = estimatePdfPages(buffer);
        setTotalPages(pages);
        if (!docText.trim()) {
          setDocText(
            `Tài liệu học thuật "${file.name}": Đã tiếp nhận ${pages} trang sách (${formatFileSize(file.size)}). Sẵn sàng phục vụ đọc sâu và tạo thẻ ghi nhớ FSRS.`
          );
        }
      } catch {
        setTotalPages(1);
      }
    } else {
      setFileMimeType(file.type || "application/octet-stream");
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      const file = e.dataTransfer.files[0];
      handleProcessFile(file);
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handleClearSelectedFile = () => {
    setSelectedFile(null);
    setUploadError(null);
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const handleUploadDocument = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!docTitle.trim()) return;

    if (fileSizeBytes > 52428800) {
      setUploadError("Không thể tải lên: Dung lượng tài liệu vượt quá hạn mức lưu trữ 50MB của gói Free.");
      return;
    }

    setUploading(true);
    setUploadError(null);
    try {
      const resp = await fetch("/api/v1/documents/upload", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          title: docTitle.trim(),
          fileSizeBytes: Math.max(1024, fileSizeBytes),
          totalPages: Math.max(1, totalPages),
          mimeType: fileMimeType,
          extractedText:
            docText.trim() ||
            `Tài liệu học thuật "${docTitle.trim()}": Phân tích chuyên sâu về quản lý bộ nhớ ảo, Translation Lookaside Buffer (TLB), bảng trang đa cấp và tối ưu hóa hệ thống.`
        })
      });

      if (resp.ok) {
        const created = await resp.json();
        setUploadModalOpen(false);
        setDocTitle("");
        setDocText("");
        setSelectedFile(null);
        setUploadError(null);
        onRefreshDashboard();
        setActiveDocument(created.id);
        setActiveMode("reading");
      } else {
        const err = await resp.json().catch(() => ({}));
        setUploadError(
          err.error ||
            (resp.status === 413
              ? "Vượt quá hạn mức dung lượng lưu trữ 50 MB của gói Free."
              : `Tải tài liệu thất bại (Mã lỗi ${resp.status}).`)
        );
      }
    } catch {
      setUploadError("Lỗi kết nối mạng khi tải tài liệu lên. Vui lòng kiểm tra lại đường truyền.");
    } finally {
      setUploading(false);
    }
  };

  return (
    <div
      data-testid="personal-workspace-dashboard"
      className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 space-y-6"
    >
      {/* Zone 0: Welcome Header & Action Bar */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white dark:bg-slate-900 p-6 rounded-2xl border border-slate-200/80 dark:border-slate-800 shadow-sm">
        <div>
          <div className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 text-xs font-semibold mb-2">
            <ShieldCheck className="w-3.5 h-3.5" />
            <span>Không gian cá nhân chủ quyền (Sovereign Personal Workspace)</span>
          </div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-white">
            Chào mừng trở lại, {data.user.fullName}!
          </h1>
          <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-400 mt-1">
            Mục tiêu ghi nhớ dài hạn: Ôn tập ngắt quãng FSRS v5 đều đặn mỗi ngày để duy trì độ nhớ mục tiêu ≥ 90%.
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2.5 shrink-0">
          <button
            type="button"
            data-testid="open-upload-pdf-btn"
            onClick={() => setUploadModalOpen(true)}
            className="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold shadow-xs transition cursor-pointer"
          >
            <Upload className="w-3.5 h-3.5" />
            <span>Tải lên PDF / Markdown</span>
          </button>
          <button
            type="button"
            data-testid="open-split-notebook-btn"
            onClick={() => setActiveMode("split")}
            className="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 text-slate-800 dark:text-slate-100 text-xs font-semibold transition cursor-pointer"
          >
            <PenTool className="w-3.5 h-3.5" />
            <span>Mở Ghi chú Tổng hợp (Split Mode)</span>
          </button>
          <button
            type="button"
            data-testid="open-practice-quiz-btn"
            onClick={() => {
              setActiveReviewSubTab("quiz");
              setActiveMode("review");
            }}
            className="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl border border-emerald-500/40 bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-800 dark:text-emerald-300 text-xs font-semibold transition cursor-pointer"
          >
            <Target className="w-3.5 h-3.5" />
            <span>Luyện tập Practice Quiz (AI Rubric)</span>
          </button>
        </div>
      </div>

      {/* Zone 1: 3-Step Guided Onboarding Widget (Clean Card, No Rough Gradients, Sentence Case) */}
      <section
        data-testid="onboarding-3step-widget"
        className="rounded-2xl border border-slate-200/80 dark:border-slate-800 bg-white dark:bg-slate-900 p-6 shadow-sm border-l-4 border-l-indigo-600"
      >
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 pb-3 border-b border-slate-100 dark:border-slate-800">
          <div className="flex items-center gap-2">
            <Sparkles className="w-5 h-5 text-indigo-600 dark:text-indigo-400 shrink-0" />
            <h2 className="text-sm sm:text-base font-bold text-slate-900 dark:text-white">
              Bắt đầu nhanh trong 3 bước • Kích hoạt chu kỳ ôn tập FSRS
            </h2>
          </div>
          <span
            data-testid="onboarding-progress-badge"
            className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-indigo-600 text-white self-start sm:self-auto"
          >
            {isActivated ? (
              <>
                <Trophy className="w-3.5 h-3.5" />
                <span>Đã kích hoạt (3/3)</span>
              </>
            ) : (
              <span>Tiến độ: {Math.min(3, step)}/3</span>
            )}
          </span>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
          {/* Step 1 */}
          <div
            data-testid="onboarding-step-1"
            className="p-4 rounded-xl bg-slate-50/70 dark:bg-slate-950/50 border border-slate-200 dark:border-slate-800 flex flex-col justify-between"
          >
            <div>
              <div className="flex items-center gap-1.5 text-xs font-semibold text-emerald-600 dark:text-emerald-400">
                <CheckCircle2 className="w-3.5 h-3.5" />
                <span>Bước 1 (Đã sẵn sàng)</span>
              </div>
              <p className="text-xs text-slate-700 dark:text-slate-300 mt-1.5 font-medium">
                Khám phá bài đọc mẫu &ldquo;Hệ điều hành: Kiến trúc Bộ nhớ ảo & Phân trang&rdquo;
              </p>
            </div>
            <button
              type="button"
              onClick={() => {
                if (data.lessons[0]) setActiveLesson(data.lessons[0].id);
                setActiveMode("reading");
              }}
              className="mt-3.5 inline-flex items-center gap-1 text-left text-xs font-semibold text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer"
            >
              <span>Mở chế độ Đọc sâu (68ch)</span>
              <ArrowRight className="w-3.5 h-3.5" />
            </button>
          </div>

          {/* Step 2 */}
          <div
            data-testid="onboarding-step-2"
            className="p-4 rounded-xl bg-slate-50/70 dark:bg-slate-950/50 border border-slate-200 dark:border-slate-800 flex flex-col justify-between"
          >
            <div>
              <div className="flex items-center gap-1.5 text-xs font-semibold text-indigo-600 dark:text-indigo-400">
                {step >= 2 ? (
                  <>
                    <CheckCircle2 className="w-3.5 h-3.5 text-emerald-600" />
                    <span className="text-emerald-600 dark:text-emerald-400">Bước 2 (Đã tạo thẻ mẫu)</span>
                  </>
                ) : (
                  <>
                    <Circle className="w-3.5 h-3.5 text-indigo-600" />
                    <span>Bước 2: Trích xuất Cloze</span>
                  </>
                )}
              </div>
              <p className="text-xs text-slate-700 dark:text-slate-300 mt-1.5 font-medium">
                Bôi đậm khái niệm với màu Vàng & chọn &ldquo;Tạo thẻ ghi nhớ&rdquo; (Popover Cloze 0 credit)
              </p>
            </div>
            <button
              type="button"
              onClick={() => {
                if (data.lessons[0]) setActiveLesson(data.lessons[0].id);
                setActiveMode("reading");
              }}
              className="mt-3.5 inline-flex items-center gap-1 text-left text-xs font-semibold text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer"
            >
              <span>Bôi đậm & Tạo thẻ Cloze ngay</span>
              <ArrowRight className="w-3.5 h-3.5" />
            </button>
          </div>

          {/* Step 3 */}
          <div
            data-testid="onboarding-step-3"
            className="p-4 rounded-xl bg-slate-50/70 dark:bg-slate-950/50 border border-indigo-200 dark:border-indigo-900/60 flex flex-col justify-between"
          >
            <div>
              <div className="flex items-center gap-1.5 text-xs font-semibold text-amber-600 dark:text-amber-400">
                {isActivated ? (
                  <>
                    <Trophy className="w-3.5 h-3.5" />
                    <span>Bước 3 (Đã kích hoạt!)</span>
                  </>
                ) : (
                  <>
                    <Zap className="w-3.5 h-3.5" />
                    <span>Bước 3: Kích hoạt Aha! Moment</span>
                  </>
                )}
              </div>
              <p className="text-xs text-slate-700 dark:text-slate-300 mt-1.5 font-medium">
                Hoàn thành ≥ 1 lượt lật thẻ FSRS v5 và chọn mức đánh giá &ldquo;Tốt (Good)&rdquo;
              </p>
            </div>
            <button
              type="button"
              data-testid="onboarding-start-review-btn"
              onClick={() => {
                setSelectedTagFilterId(null);
                setActiveReviewSubTab("fsrs");
                setActiveMode("review");
              }}
              className="mt-3.5 inline-flex items-center justify-center gap-1.5 px-3 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold text-center cursor-pointer transition shadow-xs"
            >
              <Zap className="w-3.5 h-3.5" />
              <span>Bắt đầu phiên ôn tập (~1 phút)</span>
            </button>
          </div>
        </div>
      </section>

      {/* Main Two-Column Grid: Zone 2 (FSRS Station) + Zone 3 (Recent Materials & Tags) */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Zone 2: FSRS Active Recall Station (Spacious, Clean) */}
        <div
          data-testid="fsrs-review-station-card"
          className="lg:col-span-5 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 p-6 flex flex-col justify-between shadow-sm"
        >
          <div>
            <div className="flex items-center justify-between pb-3 border-b border-slate-100 dark:border-slate-800">
              <h3 className="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
                <Brain className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
                <span>Trạm ôn tập chủ động FSRS</span>
              </h3>
              <span className="px-2 py-0.5 rounded-md bg-emerald-500/10 text-emerald-700 dark:text-emerald-300 text-[11px] font-semibold">
                FSRS v5 (19-Param)
              </span>
            </div>

            <div className="mt-5 flex items-baseline gap-3">
              <span
                data-testid="dashboard-due-count"
                className="text-4xl font-black text-indigo-600 dark:text-indigo-400 tracking-tight"
              >
                {data.reviewStation.totalDue}
              </span>
              <span className="text-sm text-slate-600 dark:text-slate-400 font-medium">
                thẻ đến hạn hôm nay • Tổng số: {data.reviewStation.totalCards} thẻ
              </span>
            </div>

            <div className="grid grid-cols-3 gap-2.5 mt-5">
              <div className="p-3 rounded-xl bg-sky-50 dark:bg-sky-950/40 border border-sky-200/60 dark:border-sky-900">
                <div className="text-lg font-bold text-sky-700 dark:text-sky-300">
                  {data.reviewStation.newCount}
                </div>
                <div className="text-[11px] font-medium text-sky-600 dark:text-sky-400 mt-0.5">
                  Thẻ mới (New)
                </div>
              </div>
              <div className="p-3 rounded-xl bg-amber-50 dark:bg-amber-950/40 border border-amber-200/60 dark:border-amber-900">
                <div className="text-lg font-bold text-amber-700 dark:text-amber-300">
                  {data.reviewStation.learningCount}
                </div>
                <div className="text-[11px] font-medium text-amber-600 dark:text-amber-400 mt-0.5">
                  Đang học (Learn)
                </div>
              </div>
              <div className="p-3 rounded-xl bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200/60 dark:border-emerald-900">
                <div className="text-lg font-bold text-emerald-700 dark:text-emerald-300">
                  {data.reviewStation.reviewCount}
                </div>
                <div className="text-[11px] font-medium text-emerald-600 dark:text-emerald-400 mt-0.5">
                  Ôn định kỳ (Review)
                </div>
              </div>
            </div>

            <p className="text-xs text-slate-500 dark:text-slate-400 mt-5 flex items-center gap-1.5">
              <Clock className="w-3.5 h-3.5 text-slate-400 shrink-0" />
              <span>Thời gian hoàn thành dự kiến: ~{data.reviewStation.estimatedMinutes} phút • Hàng đợi hợp nhất theo Tag</span>
            </p>
          </div>

          <button
            type="button"
            data-testid="launch-unified-review-btn"
            onClick={() => {
              setSelectedTagFilterId(null);
              setActiveReviewSubTab("fsrs");
              setActiveMode("review");
            }}
            className="mt-6 w-full py-3 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs sm:text-sm font-bold shadow-sm transition cursor-pointer flex items-center justify-center gap-2"
          >
            <Brain className="w-4 h-4" />
            <span>Bắt đầu ôn tập hợp nhất ({data.reviewStation.totalDue} thẻ)</span>
          </button>
        </div>

        {/* Zone 3: Recent Materials, Notes & Tag Filters */}
        <div className="lg:col-span-7 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 p-6 shadow-sm space-y-4">
          <div className="flex items-center justify-between pb-3 border-b border-slate-100 dark:border-slate-800">
            <h3 className="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
              <BookOpen className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
              <span>Tài liệu & bài học gần đây</span>
            </h3>
            <span className="text-xs text-slate-500 dark:text-slate-400">
              {data.lessons.length + data.documents.length + data.notebooks.length} mục hoạt động
            </span>
          </div>

          <div className="divide-y divide-slate-100 dark:divide-slate-800">
            {data.lessons.map((les) => (
              <div
                key={les.id}
                className="py-3 flex items-center justify-between gap-3 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 px-2 rounded-lg transition"
              >
                <div className="min-w-0">
                  <div className="text-xs sm:text-sm font-semibold text-slate-900 dark:text-slate-100 truncate flex items-center gap-1.5">
                    <BookOpen className="w-3.5 h-3.5 text-indigo-500 shrink-0" />
                    <span>[Bài học] {les.title}</span>
                  </div>
                  <div className="text-[11px] text-slate-500 mt-0.5">
                    Phiên bản v{les.currentVersion} • Hỗ trợ tự phục hồi 3 tầng (3-Tier Self-Healing)
                  </div>
                </div>
                <div className="flex items-center gap-1.5 shrink-0">
                  <button
                    type="button"
                    onClick={() => {
                      setActiveLesson(les.id);
                      setActiveMode("reading");
                    }}
                    className="px-2.5 py-1 rounded-lg bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer"
                  >
                    Đọc sâu
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setActiveLesson(les.id);
                      setActiveMode("split");
                    }}
                    className="px-2.5 py-1 rounded-lg bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer"
                  >
                    Split PKM
                  </button>
                </div>
              </div>
            ))}

            {data.documents.map((doc) => (
              <div
                key={doc.id}
                className="py-3 flex items-center justify-between gap-3 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 px-2 rounded-lg transition"
              >
                <div className="min-w-0">
                  <div className="text-xs sm:text-sm font-semibold text-slate-900 dark:text-slate-100 truncate flex items-center gap-1.5">
                    <FileText className="w-3.5 h-3.5 text-emerald-500 shrink-0" />
                    <span>[PDF] {doc.title}</span>
                  </div>
                  <div className="text-[11px] text-slate-500 mt-0.5">
                    {doc.totalPages} trang • {(doc.fileSizeBytes / 1048576).toFixed(1)} MB • Trạng thái:{" "}
                    <span className="text-emerald-600 dark:text-emerald-400 font-semibold">
                      {doc.ocrStatus}
                    </span>
                  </div>
                </div>
                <div className="flex items-center gap-1.5 shrink-0">
                  <button
                    type="button"
                    onClick={() => {
                      setActiveDocument(doc.id);
                      setActiveMode("reading");
                    }}
                    className="px-2.5 py-1 rounded-lg bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer"
                  >
                    Mở PDF
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setActiveDocument(doc.id);
                      setActiveMode("split");
                    }}
                    className="px-2.5 py-1 rounded-lg bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer"
                  >
                    Split PKM
                  </button>
                </div>
              </div>
            ))}

            {data.notebooks.slice(0, 3).map((nb) => (
              <div
                key={nb.id}
                className="py-3 flex items-center justify-between gap-3 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 px-2 rounded-lg transition"
              >
                <div className="min-w-0">
                  <div className="text-xs sm:text-sm font-semibold text-slate-900 dark:text-slate-100 truncate flex items-center gap-1.5">
                    <PenTool className="w-3.5 h-3.5 text-amber-500 shrink-0" />
                    <span>[Ghi chú] {nb.title}</span>
                  </div>
                  <div className="text-[11px] text-slate-500 mt-0.5">
                    Phiên bản OCC v{nb.version} • Single-Document JSONB AST
                  </div>
                </div>
                <button
                  type="button"
                  onClick={() => {
                    if (nb.primarySourceLessonId) {
                      setActiveLesson(nb.primarySourceLessonId);
                    } else if (nb.primarySourceDocumentId) {
                      setActiveDocument(nb.primarySourceDocumentId);
                    }
                    setActiveMode("split");
                  }}
                  className="px-2.5 py-1 rounded-lg bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer shrink-0"
                >
                  Mở ghi chú
                </button>
              </div>
            ))}
          </div>

          {/* Orthogonal Tag Filter Bar */}
          <div className="pt-3 border-t border-slate-100 dark:border-slate-800">
            <div className="text-xs font-semibold text-slate-600 dark:text-slate-400 mb-2 flex items-center gap-1.5">
              <Tag className="w-3.5 h-3.5" />
              <span>Chủ đề ôn tập theo Tag</span>
            </div>
            <div className="flex flex-wrap gap-2">
              {data.tags.map((t) => (
                <button
                  key={t.id}
                  type="button"
                  data-testid={`dashboard-tag-${t.name}`}
                  onClick={() => {
                    setSelectedTagFilterId(t.id);
                    setActiveReviewSubTab("fsrs");
                    setActiveMode("review");
                  }}
                  className="px-3 py-1 rounded-full text-xs font-semibold bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-slate-700 dark:text-slate-200 transition cursor-pointer"
                >
                  #{t.name} ({t.cardCount})
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Upload PDF / Markdown Modal */}
      {uploadModalOpen && (
        <div
          data-testid="upload-document-modal"
          className="fixed inset-0 z-50 bg-slate-950/60 backdrop-blur-xs flex items-center justify-center p-4"
        >
          <form
            onSubmit={handleUploadDocument}
            className="w-full max-w-lg rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 shadow-xl space-y-4 max-h-[90vh] overflow-y-auto"
          >
            <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-3">
              <h3 className="text-base font-bold text-slate-900 dark:text-white flex items-center gap-2">
                <Upload className="w-4 h-4 text-indigo-600" />
                <span>Tải lên tài liệu PDF / Markdown vào Workspace</span>
              </h3>
              <button
                type="button"
                onClick={() => {
                  setUploadModalOpen(false);
                  setUploadError(null);
                }}
                className="text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 p-1 rounded cursor-pointer"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {/* Error Notification Banner (Fixes FUNC-02) */}
            {uploadError && (
              <div
                data-testid="upload-error-alert"
                className="p-3 rounded-xl bg-rose-50 dark:bg-rose-950/50 border border-rose-200 dark:border-rose-900/60 text-rose-700 dark:text-rose-300 text-xs font-semibold flex items-start gap-2 animate-in fade-in"
              >
                <AlertCircle className="w-4 h-4 shrink-0 mt-0.5 text-rose-600 dark:text-rose-400" />
                <div className="flex-1">{uploadError}</div>
              </div>
            )}

            {/* Hidden native file input */}
            <input
              type="file"
              ref={fileInputRef}
              data-testid="document-file-input"
              accept=".pdf,.md,.markdown,.txt,application/pdf,text/markdown,text/plain"
              onChange={(e) => {
                if (e.target.files && e.target.files[0]) {
                  handleProcessFile(e.target.files[0]);
                }
              }}
              className="hidden"
            />

            {/* Dropzone & File Selector Area */}
            {!selectedFile ? (
              <div
                data-testid="document-dropzone"
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
                onClick={() => fileInputRef.current?.click()}
                className={`p-6 rounded-2xl border-2 border-dashed text-center transition cursor-pointer flex flex-col items-center justify-center gap-2.5 ${
                  isDragging
                    ? "border-indigo-600 bg-indigo-50/60 dark:bg-indigo-950/40"
                    : "border-slate-300 dark:border-slate-700 hover:border-indigo-500 dark:hover:border-indigo-500 bg-slate-50/50 dark:bg-slate-950/40"
                }`}
              >
                <div className="w-10 h-10 rounded-full bg-indigo-50 dark:bg-indigo-950/80 text-indigo-600 dark:text-indigo-400 flex items-center justify-center">
                  <FileUp className="w-5 h-5" />
                </div>
                <div>
                  <p className="text-xs font-bold text-slate-800 dark:text-slate-200">
                    Kéo thả file PDF hoặc Markdown vào đây
                  </p>
                  <p className="text-[11px] text-slate-500 dark:text-slate-400 mt-0.5">
                    hoặc nhấp để chọn tệp từ máy tính của bạn
                  </p>
                </div>
                <button
                  type="button"
                  data-testid="browse-computer-file-btn"
                  onClick={(e) => {
                    e.stopPropagation();
                    fileInputRef.current?.click();
                  }}
                  className="px-3.5 py-1.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 text-xs font-semibold text-slate-700 dark:text-slate-200 transition shadow-2xs"
                >
                  Chọn file từ máy tính
                </button>
                <span className="text-[10px] text-slate-400">
                  Hỗ trợ .pdf, .md, .markdown, .txt • Tối đa 50MB (Gói Free)
                </span>
              </div>
            ) : (
              <div
                data-testid="selected-file-card"
                className="p-3.5 rounded-xl bg-slate-50 dark:bg-slate-800/60 border border-slate-200 dark:border-slate-700 flex items-center justify-between gap-3"
              >
                <div className="flex items-center gap-2.5 min-w-0">
                  <span
                    className={`px-2 py-1 rounded-md text-[10px] font-extrabold uppercase shrink-0 ${
                      fileMimeType === "application/pdf"
                        ? "bg-rose-100 dark:bg-rose-950/70 text-rose-700 dark:text-rose-300 border border-rose-200 dark:border-rose-800"
                        : "bg-emerald-100 dark:bg-emerald-950/70 text-emerald-700 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800"
                    }`}
                  >
                    {fileMimeType === "application/pdf" ? "PDF" : "MD"}
                  </span>
                  <div className="min-w-0">
                    <p className="text-xs font-bold text-slate-900 dark:text-slate-100 truncate">
                      {selectedFile.name}
                    </p>
                    <p className="text-[11px] text-slate-500">
                      {formatFileSize(fileSizeBytes)} • {totalPages} trang
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2 shrink-0">
                  <button
                    type="button"
                    onClick={() => fileInputRef.current?.click()}
                    className="text-xs font-semibold text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer"
                  >
                    Đổi file
                  </button>
                  <button
                    type="button"
                    onClick={handleClearSelectedFile}
                    title="Gỡ file"
                    className="p-1 rounded text-slate-400 hover:text-rose-600 dark:hover:text-rose-400 cursor-pointer"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            )}

            <div>
              <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                Tiêu đề tài liệu / giáo trình:
              </label>
              <input
                type="text"
                required
                value={docTitle}
                onChange={(e) => setDocTitle(e.target.value)}
                placeholder="VD: Modern Operating Systems - Andrew S. Tanenbaum.pdf"
                className="w-full px-3 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-transparent text-sm focus:ring-2 focus:ring-indigo-500 outline-none"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                Nội dung văn bản trích xuất (Text-layer / Markdown):
              </label>
              <textarea
                rows={4}
                value={docText}
                onChange={(e) => setDocText(e.target.value)}
                placeholder="Dán nội dung chương sách hoặc để trống để tạo văn bản mẫu..."
                className="w-full px-3 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-transparent text-xs focus:ring-2 focus:ring-indigo-500 outline-none"
              />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button
                type="button"
                onClick={() => {
                  setUploadModalOpen(false);
                  setUploadError(null);
                }}
                className="px-4 py-2 rounded-xl border border-slate-300 dark:border-slate-700 text-xs font-semibold cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-800 transition"
              >
                Hủy
              </button>
              <button
                type="submit"
                disabled={uploading}
                className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold cursor-pointer transition shadow-xs flex items-center gap-1.5"
              >
                {uploading ? (
                  <>
                    <Upload className="w-3.5 h-3.5 animate-bounce" />
                    <span>Đang xử lý...</span>
                  </>
                ) : (
                  <span>Nhập vào Workspace</span>
                )}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}

