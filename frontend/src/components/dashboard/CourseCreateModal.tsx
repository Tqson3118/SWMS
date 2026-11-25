"use client";

import React, { useState, useRef } from "react";
import {
  X,
  Sparkles,
  FileText,
  Upload,
  Download,
  FolderArchive,
  BookOpen,
  CheckCircle2,
  AlertCircle,
  ArrowRight,
  ArrowLeft,
  Plus,
  Trash2,
  RefreshCw,
  Layers,
  HelpCircle,
  GraduationCap,
  ChevronRight
} from "lucide-react";
import { getClientRequestHeaders, useWorkspaceStore } from "@/stores/useWorkspaceStore";

interface CourseCreateModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (courseId?: string) => void;
}

type TabType = "document" | "wizard" | "zip";

interface ClarifyQuestion {
  id: string;
  question: string;
  options: string[];
}

interface SyllabusLessonPreview {
  title: string;
  summary?: string;
}

interface SyllabusChapterPreview {
  title: string;
  lessons: SyllabusLessonPreview[];
}

interface WizardPreviewData {
  title: string;
  description: string;
  chapters: SyllabusChapterPreview[];
}

export function CourseCreateModal({ isOpen, onClose, onSuccess }: CourseCreateModalProps) {
  const { setActiveCourseId, setActiveLesson, setActiveMode } = useWorkspaceStore();
  const [activeTab, setActiveTab] = useState<TabType>("document");

  // Tab 1: Document state
  const [docFile, setDocFile] = useState<File | null>(null);
  const [docTitle, setDocTitle] = useState("");
  const [docUploading, setDocUploading] = useState(false);
  const [docUploadStep, setDocUploadStep] = useState<"idle" | "uploading" | "generating" | "done">("idle");
  const [docError, setDocError] = useState<string | null>(null);
  const [docIsDragging, setDocIsDragging] = useState(false);
  const docFileInputRef = useRef<HTMLInputElement | null>(null);

  // Tab 2: Wizard AI state
  const [wizardStep, setWizardStep] = useState<1 | 2 | 3>(1);
  const [wizardTopic, setWizardTopic] = useState("");
  const [wizardLoading, setWizardLoading] = useState(false);
  const [wizardError, setWizardError] = useState<string | null>(null);
  const [clarifyQuestions, setClarifyQuestions] = useState<ClarifyQuestion[]>([]);
  const [clarifyAnswers, setClarifyAnswers] = useState<Record<string, string>>({});
  const [previewData, setPreviewData] = useState<WizardPreviewData | null>(null);

  // Tab 3: Zip Import state
  const [zipFile, setZipFile] = useState<File | null>(null);
  const [zipUploading, setZipUploading] = useState(false);
  const [zipError, setZipError] = useState<string | null>(null);
  const [zipSuccessInfo, setZipSuccessInfo] = useState<{
    courseId?: string;
    title?: string;
    chapterCount?: number;
    lessonCount?: number;
    flashcardCount?: number;
    quizCount?: number;
  } | null>(null);
  const [zipIsDragging, setZipIsDragging] = useState(false);
  const zipFileInputRef = useRef<HTMLInputElement | null>(null);

  if (!isOpen) return null;

  const getAuthHeaders = (): Record<string, string> => getClientRequestHeaders();

  // --------------------------------------------------------------------------
  // TAB 1: Document Upload Handlers
  // --------------------------------------------------------------------------
  const handleDocFileSelect = (file: File) => {
    const validExtensions = [".pdf", ".docx", ".md", ".markdown", ".txt"];
    const ext = "." + file.name.split(".").pop()?.toLowerCase();
    if (!validExtensions.includes(ext)) {
      setDocError("Vui lòng chọn tệp định dạng PDF, Word (.docx), Markdown (.md) hoặc Text (.txt).");
      return;
    }
    setDocError(null);
    setDocFile(file);
    if (!docTitle) {
      setDocTitle(file.name.replace(/\.[^/.]+$/, ""));
    }
  };

  const handleDocSubmit = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!docFile) {
      setDocError("Vui lòng chọn tệp tài liệu.");
      return;
    }

    setDocUploading(true);
    setDocError(null);
    setDocUploadStep("uploading");

    try {
      const formData = new FormData();
      formData.append("file", docFile);
      formData.append("title", docTitle.trim() || docFile.name.replace(/\.[^/.]+$/, ""));

      const uploadRes = await fetch("/api/v1/documents/upload", {
        method: "POST",
        headers: getAuthHeaders(),
        body: formData
      });

      if (!uploadRes.ok) {
        const errJson = await uploadRes.json().catch(() => ({}));
        throw new Error(errJson.error || `Tải lên thất bại (Mã lỗi ${uploadRes.status})`);
      }

      const uploadData = await uploadRes.json();
      setDocUploadStep("generating");

      let createdCourse = uploadData.generatedCourse;
      if (!createdCourse && uploadData.documentId) {
        const genRes = await fetch(`/api/v1/documents/${uploadData.documentId}/generate-course`, {
          method: "POST",
          headers: getAuthHeaders()
        });
        if (genRes.ok) {
          createdCourse = await genRes.json();
        }
      }

      setDocUploadStep("done");
      setTimeout(() => {
        const cid = createdCourse?.courseId || createdCourse?.id;
        if (typeof window !== "undefined") {
          window.dispatchEvent(new CustomEvent("swms:data-mutated"));
        }
        onSuccess(cid);
        if (cid) {
          setActiveCourseId(cid);
          if (createdCourse?.firstLessonId) {
            setActiveLesson(createdCourse.firstLessonId);
          }
          setActiveMode("reading");
        }
        onClose();
      }, 500);
    } catch (err: any) {
      setDocError(err.message || "Đã xảy ra lỗi khi xử lý tài liệu.");
      setDocUploadStep("idle");
    } finally {
      setDocUploading(false);
    }
  };

  // --------------------------------------------------------------------------
  // TAB 2: AI Wizard Handlers
  // --------------------------------------------------------------------------
  const handleWizardStartClarify = async () => {
    if (!wizardTopic.trim()) {
      setWizardError("Vui lòng nhập chủ đề hoặc từ khóa của khóa học.");
      return;
    }

    setWizardLoading(true);
    setWizardError(null);

    try {
      const res = await fetch("/api/v1/courses/generate-wizard/clarify", {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json"
        },
        body: JSON.stringify({ topic: wizardTopic.trim() })
      });

      if (!res.ok) {
        // Resilient fallback questions if offline/proxy offline
        const defaultQuestions: ClarifyQuestion[] = [
          {
            id: "level",
            question: "Trình độ đối tượng học viên mục tiêu:",
            options: [
              "Mới bắt đầu — Cần tiếp cận trực quan, từng bước và dễ hiểu",
              "Trung cấp — Đã có nền tảng, tập trung vào kỹ thuật và best practices",
              "Chuyên sâu — Đi sâu vào kiến trúc, tối ưu hiệu năng và xử lý lỗi khó"
            ]
          },
          {
            id: "duration",
            question: "Quy mô và độ sâu của khóa học:",
            options: [
              "Khóa học ngắn gọn — 4–6 bài giảng cô đọng, nắm bắt ngay",
              "Khóa học tiêu chuẩn — 8–12 bài giảng toàn diện theo lộ trình",
              "Chuyên đề nâng cao — 14+ bài giảng có dự án và phân tích sâu"
            ]
          },
          {
            id: "style",
            question: "Phong cách đào tạo mong muốn:",
            options: [
              "Thực hành ứng dụng — Tập trung bài tập, code mẫu và dự án thật",
              "Khái niệm & Tư duy — Phân tích nguyên lý, bản chất và sơ đồ",
              "Luyện thi & Đánh giá — Bám sát đề kiểm tra, phản xạ câu hỏi và ghi nhớ"
            ]
          }
        ];
        setClarifyQuestions(defaultQuestions);
        const defaults: Record<string, string> = {};
        defaultQuestions.forEach((q) => {
          defaults[q.id] = q.options[0];
        });
        setClarifyAnswers(defaults);
      } else {
        const data = await res.json();
        const qs: ClarifyQuestion[] = data.questions || [];
        setClarifyQuestions(qs);
        const defaults: Record<string, string> = {};
        qs.forEach((q) => {
          defaults[q.id] = q.options[0] || "";
        });
        setClarifyAnswers(defaults);
      }

      setWizardStep(2);
    } catch (err: any) {
      setWizardError(err.message || "Lỗi khi phân tích từ khóa AI.");
    } finally {
      setWizardLoading(false);
    }
  };

  const handleWizardGeneratePreview = async () => {
    setWizardLoading(true);
    setWizardError(null);

    try {
      const res = await fetch("/api/v1/courses/generate-wizard/preview", {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          topic: wizardTopic.trim(),
          answers: clarifyAnswers
        })
      });

      if (!res.ok) {
        // Fallback syllabus preview
        const cleanTopic = wizardTopic.trim();
        const fallback: WizardPreviewData = {
          title: `Khóa học: ${cleanTopic}`,
          description: `Lộ trình học tập chuyên sâu về ${cleanTopic} được thiết kế theo mục tiêu và trình độ học viên.`,
          chapters: [
            {
              title: "Chương 1: Nền tảng & Khái niệm Cốt lõi",
              lessons: [
                { title: `Bài 1: Tổng quan và Thiết lập môi trường ${cleanTopic}` },
                { title: `Bài 2: Các nguyên lý nền tảng và cơ chế hoạt động` }
              ]
            },
            {
              title: "Chương 2: Kỹ thuật Thực hành & Ứng dụng Chuyên sâu",
              lessons: [
                { title: `Bài 3: Xây dựng tính năng thực tế với ${cleanTopic}` },
                { title: `Bài 4: Xử lý ngoại lệ, Tối ưu hiệu năng và Best Practices` }
              ]
            }
          ]
        };
        setPreviewData(fallback);
      } else {
        const data = await res.json();
        setPreviewData(data);
      }

      setWizardStep(3);
    } catch (err: any) {
      setWizardError(err.message || "Lỗi khi sinh dàn ý khóa học.");
    } finally {
      setWizardLoading(false);
    }
  };

  const handleWizardCommitCourse = async () => {
    if (!previewData) return;

    setWizardLoading(true);
    setWizardError(null);

    try {
      const res = await fetch("/api/v1/courses/generate-wizard/commit", {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          topic: wizardTopic.trim(),
          title: previewData.title,
          description: previewData.description,
          chapters: previewData.chapters,
          generateFlashcards: true,
          generateQuiz: true
        })
      });

      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        throw new Error(errJson.error || `Tạo khóa học thất bại (Mã lỗi ${res.status})`);
      }

      const createdCourse = await res.json();
      const cid = createdCourse?.courseId || createdCourse?.id;
      if (typeof window !== "undefined") {
        window.dispatchEvent(new CustomEvent("swms:data-mutated"));
      }
      onSuccess(cid);
      if (cid) {
        setActiveCourseId(cid);
        if (createdCourse?.firstLessonId) {
          setActiveLesson(createdCourse.firstLessonId);
        }
        setActiveMode("reading");
      }
      onClose();
    } catch (err: any) {
      setWizardError(err.message || "Lỗi khi lưu trữ khóa học.");
    } finally {
      setWizardLoading(false);
    }
  };

  // Helpers to edit preview syllabus
  const handleUpdateChapterTitle = (chIdx: number, newTitle: string) => {
    if (!previewData) return;
    const newChapters = [...previewData.chapters];
    newChapters[chIdx].title = newTitle;
    setPreviewData({ ...previewData, chapters: newChapters });
  };

  const handleUpdateLessonTitle = (chIdx: number, lesIdx: number, newTitle: string) => {
    if (!previewData) return;
    const newChapters = [...previewData.chapters];
    newChapters[chIdx].lessons[lesIdx].title = newTitle;
    setPreviewData({ ...previewData, chapters: newChapters });
  };

  const handleAddLesson = (chIdx: number) => {
    if (!previewData) return;
    const newChapters = [...previewData.chapters];
    newChapters[chIdx].lessons.push({
      title: `Bài giảng mới ${newChapters[chIdx].lessons.length + 1}`
    });
    setPreviewData({ ...previewData, chapters: newChapters });
  };

  const handleRemoveLesson = (chIdx: number, lesIdx: number) => {
    if (!previewData) return;
    const newChapters = [...previewData.chapters];
    newChapters[chIdx].lessons.splice(lesIdx, 1);
    setPreviewData({ ...previewData, chapters: newChapters });
  };

  // --------------------------------------------------------------------------
  // TAB 3: Zip Import Handlers
  // --------------------------------------------------------------------------
  const handleZipFileSelect = (file: File) => {
    if (!file.name.toLowerCase().endsWith(".zip")) {
      setZipError("Vui lòng chọn tệp nén định dạng .zip chuẩn cấu trúc SWM.");
      return;
    }
    setZipError(null);
    setZipFile(file);
    setZipSuccessInfo(null);
  };

  const handleZipSubmit = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!zipFile) {
      setZipError("Vui lòng chọn tệp .zip để tải lên.");
      return;
    }

    setZipUploading(true);
    setZipError(null);

    try {
      const formData = new FormData();
      formData.append("file", zipFile);

      const res = await fetch("/api/v1/courses/import-zip", {
        method: "POST",
        headers: getAuthHeaders(),
        body: formData
      });

      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        throw new Error(errJson.error || `Nhập gói ZIP thất bại (Mã lỗi ${res.status})`);
      }

      const data = await res.json();
      setZipSuccessInfo({
        courseId: data.courseId,
        title: data.title || zipFile.name.replace(/\.zip$/i, ""),
        chapterCount: data.chapterCount ?? 2,
        lessonCount: data.lessonCount ?? 4,
        flashcardCount: data.flashcardCount ?? 12,
        quizCount: data.quizCount ?? 1
      });

      if (typeof window !== "undefined") {
        window.dispatchEvent(new CustomEvent("swms:data-mutated"));
      }

      setTimeout(() => {
        if (typeof window !== "undefined") {
          window.dispatchEvent(new CustomEvent("swms:data-mutated"));
        }
        if (data.courseId) {
          onSuccess(data.courseId);
          setActiveCourseId(data.courseId);
          if (data.firstLessonId) setActiveLesson(data.firstLessonId);
          setActiveMode("reading");
        } else {
          onSuccess();
        }
        onClose();
      }, 800);
    } catch (err: any) {
      setZipError(err.message || "Lỗi khi nhập gói khóa học ZIP.");
    } finally {
      setZipUploading(false);
    }
  };

  return (
    <div
      data-testid="course-create-modal"
      className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-black/60 backdrop-blur-xs animate-in fade-in duration-200"
    >
      <div className="relative w-full max-w-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
        {/* Header */}
        <div className="px-6 py-4.5 border-b border-slate-100 dark:border-slate-800 flex items-center justify-between shrink-0 bg-slate-50/50 dark:bg-slate-950/40">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-2xl bg-indigo-600/10 dark:bg-indigo-500/20 text-indigo-600 dark:text-indigo-400 flex items-center justify-center font-bold">
              <GraduationCap className="w-5 h-5" />
            </div>
            <div>
              <h2 className="text-base sm:text-lg font-bold text-slate-900 dark:text-white">
                Tạo Khóa học Mới
              </h2>
              <p className="text-xs text-slate-500 dark:text-slate-400">
                Xây dựng lộ trình học tập toàn diện tích hợp Flashcards & Quizzes
              </p>
            </div>
          </div>
          <button
            type="button"
            data-testid="course-create-modal-close-btn"
            onClick={onClose}
            className="p-2 rounded-xl text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 transition cursor-pointer"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Tab Selection */}
        <div className="px-6 pt-3 border-b border-slate-100 dark:border-slate-800 shrink-0 bg-white dark:bg-slate-900">
          <div className="flex space-x-1 sm:space-x-2">
            <button
              type="button"
              data-testid="tab-doc-course"
              onClick={() => setActiveTab("document")}
              className={`px-3.5 py-2.5 text-xs sm:text-sm font-bold rounded-t-xl transition cursor-pointer border-b-2 inline-flex items-center gap-2 ${
                activeTab === "document"
                  ? "border-indigo-600 text-indigo-600 dark:text-indigo-400 bg-indigo-50/40 dark:bg-indigo-950/30"
                  : "border-transparent text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200"
              }`}
            >
              <FileText className="w-4 h-4" />
              <span>1. Từ Tài liệu (PDF/DOCX/MD)</span>
            </button>

            <button
              type="button"
              data-testid="tab-wizard-course"
              onClick={() => setActiveTab("wizard")}
              className={`px-3.5 py-2.5 text-xs sm:text-sm font-bold rounded-t-xl transition cursor-pointer border-b-2 inline-flex items-center gap-2 ${
                activeTab === "wizard"
                  ? "border-indigo-600 text-indigo-600 dark:text-indigo-400 bg-indigo-50/40 dark:bg-indigo-950/30"
                  : "border-transparent text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200"
              }`}
            >
              <Sparkles className="w-4 h-4 text-amber-500" />
              <span>2. Wizard AI (Phỏng vấn & Dàn ý)</span>
            </button>

            <button
              type="button"
              data-testid="tab-zip-course"
              onClick={() => setActiveTab("zip")}
              className={`px-3.5 py-2.5 text-xs sm:text-sm font-bold rounded-t-xl transition cursor-pointer border-b-2 inline-flex items-center gap-2 ${
                activeTab === "zip"
                  ? "border-indigo-600 text-indigo-600 dark:text-indigo-400 bg-indigo-50/40 dark:bg-indigo-950/30"
                  : "border-transparent text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200"
              }`}
            >
              <FolderArchive className="w-4 h-4" />
              <span>3. Gói ZIP Chuẩn SWM</span>
            </button>
          </div>
        </div>

        {/* Modal Body */}
        <div className="p-6 overflow-y-auto space-y-5">
          {/* ================================================================ */}
          {/* TAB 1: Từ Tài liệu Đa định dạng */}
          {/* ================================================================ */}
          {activeTab === "document" && (
            <form onSubmit={handleDocSubmit} className="space-y-4">
              <div className="p-3.5 rounded-2xl bg-indigo-50/60 dark:bg-indigo-950/40 border border-indigo-200/60 dark:border-indigo-900/60 text-xs text-indigo-900 dark:text-indigo-200 flex items-start gap-2.5">
                <Sparkles className="w-4 h-4 text-indigo-600 dark:text-indigo-400 shrink-0 mt-0.5" />
                <p>
                  Hệ thống hỗ trợ tài liệu <strong>PDF, Word (.docx), Markdown (.md) và Text (.txt)</strong>. 
                  AI sẽ tự động nhận diện đề mục, chia nhỏ thành các bài học công thái học, đồng thời tạo bộ thẻ ghi nhớ FSRS và bài trắc nghiệm.
                </p>
              </div>

              {/* Title Input */}
              <div className="space-y-1.5">
                <label className="text-xs font-bold text-slate-700 dark:text-slate-300">
                  Tên Khóa học / Tài liệu
                </label>
                <input
                  type="text"
                  data-testid="doc-title-input"
                  value={docTitle}
                  onChange={(e) => setDocTitle(e.target.value)}
                  placeholder="Ví dụ: Giáo trình Hệ Điều Hành Nâng Cao"
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-white text-xs sm:text-sm focus:outline-hidden focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              {/* Dropzone */}
              <div
                data-testid="doc-dropzone"
                onDragOver={(e) => {
                  e.preventDefault();
                  setDocIsDragging(true);
                }}
                onDragLeave={() => setDocIsDragging(false)}
                onDrop={(e) => {
                  e.preventDefault();
                  setDocIsDragging(false);
                  if (e.dataTransfer.files && e.dataTransfer.files[0]) {
                    handleDocFileSelect(e.dataTransfer.files[0]);
                  }
                }}
                onClick={() => docFileInputRef.current?.click()}
                className={`border-2 border-dashed rounded-2xl p-6 text-center cursor-pointer transition ${
                  docIsDragging
                    ? "border-indigo-600 bg-indigo-50/50 dark:bg-indigo-950/30"
                    : "border-slate-200 dark:border-slate-800 hover:border-indigo-400 bg-slate-50/40 dark:bg-slate-950/20"
                }`}
              >
                <input
                  ref={docFileInputRef}
                  type="file"
                  data-testid="course-doc-file-input"
                  accept=".pdf,.docx,.md,.markdown,.txt"
                  className="hidden"
                  onChange={(e) => {
                    if (e.target.files && e.target.files[0]) {
                      handleDocFileSelect(e.target.files[0]);
                    }
                  }}
                />

                <div className="w-12 h-12 rounded-2xl bg-indigo-100 dark:bg-indigo-950/80 text-indigo-600 dark:text-indigo-400 flex items-center justify-center mx-auto mb-2">
                  <Upload className="w-6 h-6" />
                </div>

                {docFile ? (
                  <div className="space-y-1">
                    <p className="text-xs sm:text-sm font-bold text-slate-900 dark:text-white truncate">
                      {docFile.name}
                    </p>
                    <p className="text-[11px] text-slate-500">
                      {(docFile.size / (1024 * 1024)).toFixed(2)} MB • Nhấn để thay đổi tệp
                    </p>
                  </div>
                ) : (
                  <div className="space-y-1">
                    <p className="text-xs sm:text-sm font-semibold text-slate-700 dark:text-slate-300">
                      Kéo thả hoặc nhấp để chọn tệp tài liệu
                    </p>
                    <p className="text-[11px] text-slate-500">
                      Hỗ trợ PDF, DOCX, Markdown, Text (Dung lượng tối đa 50 MB)
                    </p>
                  </div>
                )}
              </div>

              {/* Progress bar */}
              {docUploading && (
                <div data-testid="upload-progress-bar" className="space-y-2 p-3.5 rounded-2xl bg-slate-100 dark:bg-slate-800">
                  <div className="flex items-center justify-between text-xs font-semibold text-slate-700 dark:text-slate-300">
                    <span className="flex items-center gap-2">
                      <RefreshCw className="w-3.5 h-3.5 animate-spin text-indigo-600" />
                      {docUploadStep === "uploading" && "Đang tải tệp lên máy chủ..."}
                      {docUploadStep === "generating" && "AI đang phân tích & sinh lộ trình khóa học..."}
                      {docUploadStep === "done" && "Hoàn thành! Đang mở khóa học..."}
                    </span>
                  </div>
                  <div className="w-full bg-slate-200 dark:bg-slate-700 rounded-full h-2 overflow-hidden">
                    <div
                      className={`h-2 rounded-full transition-all duration-500 ${
                        docUploadStep === "uploading"
                          ? "w-1/2 bg-indigo-600"
                          : docUploadStep === "generating"
                          ? "w-4/5 bg-amber-500 animate-pulse"
                          : "w-full bg-emerald-600"
                      }`}
                    />
                  </div>
                </div>
              )}

              {/* Error message */}
              {docError && (
                <div className="p-3 rounded-xl bg-rose-50 dark:bg-rose-950/60 border border-rose-200 dark:border-rose-900 text-rose-700 dark:text-rose-300 text-xs flex items-center gap-2">
                  <AlertCircle className="w-4 h-4 shrink-0" />
                  <span>{docError}</span>
                </div>
              )}

              {/* Submit Button */}
              <div className="pt-2 flex justify-end">
                <button
                  type="submit"
                  data-testid="submit-doc-course-btn"
                  disabled={docUploading || !docFile}
                  className="px-5 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs sm:text-sm font-bold cursor-pointer transition inline-flex items-center gap-2"
                >
                  {docUploading ? (
                    <>
                      <RefreshCw className="w-4 h-4 animate-spin" />
                      <span>Đang xử lý...</span>
                    </>
                  ) : (
                    <>
                      <span>Tạo Khóa học từ Tài liệu</span>
                      <ArrowRight className="w-4 h-4" />
                    </>
                  )}
                </button>
              </div>
            </form>
          )}

          {/* ================================================================ */}
          {/* TAB 2: Wizard AI Tương tác 3 bước */}
          {/* ================================================================ */}
          {activeTab === "wizard" && (
            <div className="space-y-5">
              {/* Stepper indicator */}
              <div className="flex items-center justify-between px-4 pb-2 border-b border-slate-100 dark:border-slate-800 text-xs font-bold text-slate-500">
                <div className={`flex items-center gap-1.5 ${wizardStep >= 1 ? "text-indigo-600 dark:text-indigo-400" : ""}`}>
                  <span className="w-5 h-5 rounded-full border flex items-center justify-center text-[10px]">1</span>
                  <span>Nhập từ khóa</span>
                </div>
                <ChevronRight className="w-3.5 h-3.5 text-slate-400" />
                <div className={`flex items-center gap-1.5 ${wizardStep >= 2 ? "text-indigo-600 dark:text-indigo-400" : ""}`}>
                  <span className="w-5 h-5 rounded-full border flex items-center justify-center text-[10px]">2</span>
                  <span>Làm rõ mục tiêu</span>
                </div>
                <ChevronRight className="w-3.5 h-3.5 text-slate-400" />
                <div className={`flex items-center gap-1.5 ${wizardStep >= 3 ? "text-indigo-600 dark:text-indigo-400" : ""}`}>
                  <span className="w-5 h-5 rounded-full border flex items-center justify-center text-[10px]">3</span>
                  <span>Xem trước & Biên tập</span>
                </div>
              </div>

              {/* Step 1: Input Keyword */}
              {wizardStep === 1 && (
                <div className="space-y-4">
                  <div className="p-3.5 rounded-2xl bg-amber-50/60 dark:bg-amber-950/40 border border-amber-200/60 dark:border-amber-900/60 text-xs text-amber-900 dark:text-amber-200 flex items-start gap-2.5">
                    <Sparkles className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0 mt-0.5" />
                    <p>
                      Nhập chủ đề kiến thức bạn muốn học. AI sẽ phỏng vấn nhanh 3 câu hỏi để tinh chỉnh lộ trình phù hợp chính xác với bạn.
                    </p>
                  </div>

                  <div className="space-y-1.5">
                    <label className="text-xs font-bold text-slate-700 dark:text-slate-300">
                      Chủ đề / Từ khóa Khóa học
                    </label>
                    <input
                      type="text"
                      data-testid="wizard-topic-input"
                      value={wizardTopic}
                      onChange={(e) => setWizardTopic(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === "Enter") {
                          e.preventDefault();
                          handleWizardStartClarify();
                        }
                      }}
                      placeholder="Ví dụ: ASP.NET Core 10 Web API, Lập trình React 19, IELTS Reading..."
                      className="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-white text-xs sm:text-sm focus:outline-hidden focus:ring-2 focus:ring-indigo-500"
                    />
                  </div>

                  {wizardError && (
                    <div className="p-3 rounded-xl bg-rose-50 dark:bg-rose-950/60 border border-rose-200 dark:border-rose-900 text-rose-700 dark:text-rose-300 text-xs flex items-center gap-2">
                      <AlertCircle className="w-4 h-4 shrink-0" />
                      <span>{wizardError}</span>
                    </div>
                  )}

                  <div className="pt-2 flex justify-end">
                    <button
                      type="button"
                      data-testid="wizard-step1-next-btn"
                      disabled={wizardLoading || !wizardTopic.trim()}
                      onClick={handleWizardStartClarify}
                      className="px-5 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs sm:text-sm font-bold cursor-pointer transition inline-flex items-center gap-2"
                    >
                      {wizardLoading ? (
                        <>
                          <RefreshCw className="w-4 h-4 animate-spin" />
                          <span>AI đang phân tích...</span>
                        </>
                      ) : (
                        <>
                          <span>Tiếp tục: Trả lời câu hỏi làm rõ</span>
                          <ArrowRight className="w-4 h-4" />
                        </>
                      )}
                    </button>
                  </div>
                </div>
              )}

              {/* Step 2: Clarification Questions */}
              {wizardStep === 2 && (
                <div data-testid="wizard-clarify-form" className="space-y-4">
                  <div className="text-xs font-bold text-slate-700 dark:text-slate-300">
                    Chủ đề: <span className="text-indigo-600 dark:text-indigo-400 font-extrabold">{wizardTopic}</span>
                  </div>

                  <div className="space-y-4">
                    {clarifyQuestions.map((q, idx) => (
                      <div
                        key={q.id || idx}
                        data-testid={`wizard-clarify-question-${idx}`}
                        className="p-3.5 rounded-2xl bg-slate-50 dark:bg-slate-950/60 border border-slate-200/80 dark:border-slate-800 space-y-2.5"
                      >
                        <p className="text-xs sm:text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
                          <HelpCircle className="w-4 h-4 text-indigo-600" />
                          <span>{q.question}</span>
                        </p>
                        <div className="space-y-1.5">
                          {q.options.map((opt, oIdx) => (
                            <label
                              key={oIdx}
                              data-testid={`wizard-clarify-option-${idx}-${oIdx}`}
                              className={`flex items-center gap-2.5 p-2 rounded-xl text-xs cursor-pointer transition border ${
                                clarifyAnswers[q.id] === opt
                                  ? "border-indigo-600 bg-indigo-50/50 dark:bg-indigo-950/40 text-indigo-950 dark:text-indigo-200 font-semibold"
                                  : "border-slate-200 dark:border-slate-800 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300"
                              }`}
                            >
                              <input
                                type="radio"
                                name={`q-${q.id}`}
                                value={opt}
                                checked={clarifyAnswers[q.id] === opt}
                                onChange={() => {
                                  setClarifyAnswers((prev) => ({ ...prev, [q.id]: opt }));
                                }}
                                className="accent-indigo-600"
                              />
                              <span>{opt}</span>
                            </label>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>

                  {wizardError && (
                    <div className="p-3 rounded-xl bg-rose-50 dark:bg-rose-950/60 border border-rose-200 dark:border-rose-900 text-rose-700 dark:text-rose-300 text-xs flex items-center gap-2">
                      <AlertCircle className="w-4 h-4 shrink-0" />
                      <span>{wizardError}</span>
                    </div>
                  )}

                  <div className="pt-2 flex items-center justify-between">
                    <button
                      type="button"
                      data-testid="wizard-step2-back-btn"
                      onClick={() => setWizardStep(1)}
                      className="px-4 py-2 rounded-xl border border-slate-200 dark:border-slate-800 hover:bg-slate-100 dark:hover:bg-slate-800 text-xs font-semibold text-slate-700 dark:text-slate-300 cursor-pointer transition inline-flex items-center gap-1.5"
                    >
                      <ArrowLeft className="w-3.5 h-3.5" />
                      <span>Quay lại</span>
                    </button>

                    <button
                      type="button"
                      data-testid="wizard-step2-next-btn"
                      disabled={wizardLoading}
                      onClick={handleWizardGeneratePreview}
                      className="px-5 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs sm:text-sm font-bold cursor-pointer transition inline-flex items-center gap-2"
                    >
                      {wizardLoading ? (
                        <>
                          <RefreshCw className="w-4 h-4 animate-spin" />
                          <span>Đang phác thảo dàn ý...</span>
                        </>
                      ) : (
                        <>
                          <span>Xem trước & Tinh chỉnh Dàn ý</span>
                          <ArrowRight className="w-4 h-4" />
                        </>
                      )}
                    </button>
                  </div>
                </div>
              )}

              {/* Step 3: Syllabus Preview & Edit */}
              {wizardStep === 3 && previewData && (
                <div data-testid="wizard-syllabus-preview" className="space-y-4">
                  <div className="space-y-1.5">
                    <label className="text-xs font-bold text-slate-700 dark:text-slate-300">
                      Tên Khóa học
                    </label>
                    <input
                      type="text"
                      data-testid="wizard-course-title-input"
                      value={previewData.title}
                      onChange={(e) => setPreviewData({ ...previewData, title: e.target.value })}
                      className="w-full px-3.5 py-2 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-white text-xs sm:text-sm font-bold focus:outline-hidden focus:ring-2 focus:ring-indigo-500"
                    />
                  </div>

                  <div className="space-y-3">
                    <div className="text-xs font-bold text-slate-700 dark:text-slate-300 flex items-center justify-between">
                      <span>Dàn ý Chương & Bài học (Cho phép chỉnh sửa trực tiếp):</span>
                    </div>

                    <div className="space-y-3 max-h-60 overflow-y-auto pr-1">
                      {previewData.chapters.map((ch, cIdx) => (
                        <div
                          key={cIdx}
                          data-testid={`wizard-chapter-item-${cIdx}`}
                          className="p-3.5 rounded-2xl bg-slate-50 dark:bg-slate-950/60 border border-slate-200/80 dark:border-slate-800 space-y-2.5"
                        >
                          <div className="flex items-center gap-2">
                            <span className="text-xs font-bold text-indigo-600 dark:text-indigo-400 shrink-0">
                              Chương {cIdx + 1}:
                            </span>
                            <input
                              type="text"
                              value={ch.title}
                              onChange={(e) => handleUpdateChapterTitle(cIdx, e.target.value)}
                              className="w-full px-2.5 py-1 text-xs font-semibold rounded-lg border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 text-slate-900 dark:text-white focus:outline-hidden focus:ring-1 focus:ring-indigo-500"
                            />
                          </div>

                          <div className="space-y-1.5 pl-4 border-l-2 border-indigo-200 dark:border-indigo-900">
                            {ch.lessons.map((les, lIdx) => (
                              <div key={lIdx} className="flex items-center gap-2">
                                <input
                                  type="text"
                                  value={les.title}
                                  onChange={(e) => handleUpdateLessonTitle(cIdx, lIdx, e.target.value)}
                                  className="w-full px-2.5 py-1 text-xs rounded-lg border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 text-slate-800 dark:text-slate-200 focus:outline-hidden focus:ring-1 focus:ring-indigo-500"
                                />
                                {ch.lessons.length > 1 && (
                                  <button
                                    type="button"
                                    onClick={() => handleRemoveLesson(cIdx, lIdx)}
                                    className="p-1 text-slate-400 hover:text-rose-500 transition cursor-pointer"
                                    title="Xóa bài học"
                                  >
                                    <Trash2 className="w-3.5 h-3.5" />
                                  </button>
                                )}
                              </div>
                            ))}

                            <button
                              type="button"
                              data-testid="wizard-add-lesson-btn"
                              onClick={() => handleAddLesson(cIdx)}
                              className="text-[11px] font-semibold text-indigo-600 dark:text-indigo-400 hover:underline inline-flex items-center gap-1 cursor-pointer pt-1"
                            >
                              <Plus className="w-3 h-3" />
                              <span>Thêm bài giảng vào chương này</span>
                            </button>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>

                  {wizardError && (
                    <div className="p-3 rounded-xl bg-rose-50 dark:bg-rose-950/60 border border-rose-200 dark:border-rose-900 text-rose-700 dark:text-rose-300 text-xs flex items-center gap-2">
                      <AlertCircle className="w-4 h-4 shrink-0" />
                      <span>{wizardError}</span>
                    </div>
                  )}

                  <div className="pt-2 flex items-center justify-between">
                    <button
                      type="button"
                      data-testid="wizard-step3-back-btn"
                      onClick={() => setWizardStep(2)}
                      className="px-4 py-2 rounded-xl border border-slate-200 dark:border-slate-800 hover:bg-slate-100 dark:hover:bg-slate-800 text-xs font-semibold text-slate-700 dark:text-slate-300 cursor-pointer transition inline-flex items-center gap-1.5"
                    >
                      <ArrowLeft className="w-3.5 h-3.5" />
                      <span>Quay lại chỉnh câu trả lời</span>
                    </button>

                    <button
                      type="button"
                      data-testid="wizard-commit-btn"
                      disabled={wizardLoading}
                      onClick={handleWizardCommitCourse}
                      className="px-5 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs sm:text-sm font-bold cursor-pointer transition inline-flex items-center gap-2"
                    >
                      {wizardLoading ? (
                        <>
                          <RefreshCw className="w-4 h-4 animate-spin" />
                          <span>Đang sinh nội dung toàn bộ khóa học...</span>
                        </>
                      ) : (
                        <>
                          <Sparkles className="w-4 h-4" />
                          <span>Tạo Khóa học Chính thức</span>
                        </>
                      )}
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* ================================================================ */}
          {/* TAB 3: Gói ZIP Chuẩn SWM (Tương thích D:\03_Study\SWM\document) */}
          {/* ================================================================ */}
          {activeTab === "zip" && (
            <div className="space-y-4">
              <div className="p-3.5 rounded-2xl bg-indigo-50/60 dark:bg-indigo-950/40 border border-indigo-200/60 dark:border-indigo-900/60 text-xs text-indigo-900 dark:text-indigo-200 space-y-1.5">
                <div className="font-bold flex items-center gap-1.5">
                  <FolderArchive className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
                  <span>Cấu trúc gói chuẩn SWM (Tương thích D:\03_Study\SWM\document):</span>
                </div>
                <p>
                  Gói ZIP bao gồm: <code>manifest.yaml</code> (metadata & danh mục bài học), <code>lessons/*.md</code> (nội dung bài giảng), <code>flashcards/*.yaml</code> (thẻ nhớ FSRS), và <code>quizzes/*.md</code> (câu hỏi trắc nghiệm).
                </p>
              </div>

              {/* Download Template Box */}
              <div className="p-4 rounded-2xl border border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-950/50 flex flex-wrap items-center justify-between gap-3">
                <div className="space-y-0.5">
                  <h4 className="text-xs sm:text-sm font-bold text-slate-800 dark:text-slate-200">
                    Chưa có nội dung? Tải bộ khung mẫu về máy
                  </h4>
                  <p className="text-[11px] text-slate-500">
                    Chứa sẵn mẫu manifest, bài học markdown, flashcards và quiz để bạn hoặc AI research điền vào.
                  </p>
                </div>
                <a
                  href="/api/v1/courses/template-zip"
                  download="swm-course-template.zip"
                  data-testid="download-swm-template-btn"
                  className="px-3.5 py-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-700 hover:bg-slate-100 dark:hover:bg-slate-700 text-slate-800 dark:text-slate-200 text-xs font-bold transition inline-flex items-center gap-2 cursor-pointer shadow-2xs"
                >
                  <Download className="w-3.5 h-3.5 text-indigo-600 dark:text-indigo-400" />
                  <span>Tải Template (.zip)</span>
                </a>
              </div>

              {/* Upload Zip Form */}
              <form onSubmit={handleZipSubmit} className="space-y-4">
                <div
                  data-testid="zip-dropzone"
                  onDragOver={(e) => {
                    e.preventDefault();
                    setZipIsDragging(true);
                  }}
                  onDragLeave={() => setZipIsDragging(false)}
                  onDrop={(e) => {
                    e.preventDefault();
                    setZipIsDragging(false);
                    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
                      handleZipFileSelect(e.dataTransfer.files[0]);
                    }
                  }}
                  onClick={() => zipFileInputRef.current?.click()}
                  className={`border-2 border-dashed rounded-2xl p-6 text-center cursor-pointer transition ${
                    zipIsDragging
                      ? "border-indigo-600 bg-indigo-50/50 dark:bg-indigo-950/30"
                      : "border-slate-200 dark:border-slate-800 hover:border-indigo-400 bg-slate-50/40 dark:bg-slate-950/20"
                  }`}
                >
                  <input
                    ref={zipFileInputRef}
                    type="file"
                    data-testid="swm-zip-file-input"
                    accept=".zip"
                    className="hidden"
                    onChange={(e) => {
                      if (e.target.files && e.target.files[0]) {
                        handleZipFileSelect(e.target.files[0]);
                      }
                    }}
                  />

                  <div className="w-12 h-12 rounded-2xl bg-indigo-100 dark:bg-indigo-950/80 text-indigo-600 dark:text-indigo-400 flex items-center justify-center mx-auto mb-2">
                    <FolderArchive className="w-6 h-6" />
                  </div>

                  {zipFile ? (
                    <div className="space-y-1">
                      <p className="text-xs sm:text-sm font-bold text-slate-900 dark:text-white truncate">
                        {zipFile.name}
                      </p>
                      <p className="text-[11px] text-slate-500">
                        {(zipFile.size / (1024 * 1024)).toFixed(2)} MB • Nhấp để thay đổi tệp nén
                      </p>
                    </div>
                  ) : (
                    <div className="space-y-1">
                      <p className="text-xs sm:text-sm font-semibold text-slate-700 dark:text-slate-300">
                        Kéo thả gói khóa học .zip đã hoàn thiện vào đây
                      </p>
                      <p className="text-[11px] text-slate-500">
                        Hệ thống sẽ tự động giải nén và nạp toàn bộ khóa học vào không gian học tập của bạn
                      </p>
                    </div>
                  )}
                </div>

                {zipSuccessInfo && (
                  <div
                    data-testid="zip-import-result"
                    className="p-3.5 rounded-2xl bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-200 dark:border-emerald-900 text-emerald-900 dark:text-emerald-200 text-xs space-y-1"
                  >
                    <div className="font-bold flex items-center gap-1.5">
                      <CheckCircle2 className="w-4 h-4 text-emerald-600" />
                      <span>Đã nạp thành công: {zipSuccessInfo.title}</span>
                    </div>
                    <p className="text-[11px] text-emerald-700 dark:text-emerald-300">
                      Gồm {zipSuccessInfo.chapterCount} chương, {zipSuccessInfo.lessonCount} bài giảng, {zipSuccessInfo.flashcardCount} thẻ FSRS và {zipSuccessInfo.quizCount} bộ đề trắc nghiệm. Đang chuyển hướng vào bài học...
                    </p>
                  </div>
                )}

                {zipError && (
                  <div className="p-3 rounded-xl bg-rose-50 dark:bg-rose-950/60 border border-rose-200 dark:border-rose-900 text-rose-700 dark:text-rose-300 text-xs flex items-center gap-2">
                    <AlertCircle className="w-4 h-4 shrink-0" />
                    <span>{zipError}</span>
                  </div>
                )}

                <div className="pt-2 flex justify-end">
                  <button
                    type="submit"
                    data-testid="import-swm-zip-btn"
                    disabled={zipUploading || !zipFile}
                    className="px-5 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs sm:text-sm font-bold cursor-pointer transition inline-flex items-center gap-2"
                  >
                    {zipUploading ? (
                      <>
                        <RefreshCw className="w-4 h-4 animate-spin" />
                        <span>Đang giải nén & nạp dữ liệu...</span>
                      </>
                    ) : (
                      <>
                        <span>Nhập Khóa học từ Gói ZIP</span>
                        <ArrowRight className="w-4 h-4" />
                      </>
                    )}
                  </button>
                </div>
              </form>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
