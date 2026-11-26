"use client";

import React, { useEffect, useRef, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  AlertCircle,
  ArrowRight,
  BookOpen,
  Check,
  ChevronDown,
  ChevronRight,
  Compass,
  Download,
  FilePlus,
  FileUp,
  HelpCircle,
  Loader2,
  Package,
  Pencil,
  Plus,
  Sparkles,
  Trash2,
  Upload,
  Zap,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { Button, Card } from "@/components/ui";
import { stripChapterPrefix } from "@/lib/course-helpers";
import { LEARNING_ROADMAPS, STUDY_USE_CASES } from "@/lib/learning-roadmaps";

interface PreviewLesson {
  title: string;
  summary?: string;
  estimatedMinutes?: number;
}

interface PreviewChapter {
  title: string;
  sortOrder?: number;
  lessons?: PreviewLesson[];
}

interface WizardPreviewData {
  title?: string;
  description?: string;
  chapters?: PreviewChapter[];
}

interface ClarifyQuestion {
  id: string;
  question: string;
  options: string[];
}

interface DuplicateCourseItem {
  id: string;
  title: string;
  slug: string;
  lessonCount: number;
  visibility: string;
  isSystemCourse: boolean;
  matchScore: number;
}

const TOPIC_INSPIRATIONS = [
  "Docker & Kubernetes Thực chiến",
  "IELTS Writing Task 2",
  "Tài chính Cá nhân & Quỹ dự phòng",
  "Tâm lý học Tập trung & Ghi nhớ",
] as const;

export default function NewCoursePage() {
  const router = useRouter();
  const { activeUserHeaderId } = useWorkspaceStore();

  const [uploadLoading, setUploadLoading] = useState(false);
  const [aiLoading, setAiLoading] = useState(false);
  const [importLoading, setImportLoading] = useState(false);
  const [blankLoading, setBlankLoading] = useState(false);

  // AI Generator / Wizard states
  const [aiTopic, setAiTopic] = useState("");
  const [wizardStep, setWizardStep] = useState<1 | 2 | 3>(1);
  const [clarifyLoading, setClarifyLoading] = useState(false);
  const [clarifyQuestions, setClarifyQuestions] = useState<ClarifyQuestion[]>([]);
  const [clarifyAnswers, setClarifyAnswers] = useState<Record<string, string>>({});
  const [previewData, setPreviewData] = useState<WizardPreviewData | null>(null);

  // Duplicate Check states
  const [duplicates, setDuplicates] = useState<DuplicateCourseItem[]>([]);
  const [isCheckingDuplicates, setIsCheckingDuplicates] = useState(false);
  const [duplicateBannerDismissed, setDuplicateBannerDismissed] = useState(false);
  const duplicateDebounceRef = useRef<NodeJS.Timeout | null>(null);

  // Drag & Drop Upload / Import states
  const [uploadDragActive, setUploadDragActive] = useState(false);
  const [uploadFileName, setUploadFileName] = useState<string | null>(null);
  const [importDragActive, setImportDragActive] = useState(false);
  const [importFileName, setImportFileName] = useState<string | null>(null);

  // Blank Course Form states
  const [blankTitle, setBlankTitle] = useState("");
  const [blankDescription, setBlankDescription] = useState("");
  const [blankCategory, setBlankCategory] = useState("Công nghệ");
  const [blankLevel, setBlankLevel] = useState<"foundation" | "intermediate" | "advanced">("foundation");
  const [blankOutcomes, setBlankOutcomes] = useState<string[]>([""]);

  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const uploadInputRef = useRef<HTMLInputElement>(null);
  const importInputRef = useRef<HTMLInputElement>(null);
  const aiTopicInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (typeof window === "undefined") return;
    const params = new URLSearchParams(window.location.search);
    const topicParam = params.get("topic");
    if (topicParam && topicParam.trim()) {
      setAiTopic(topicParam.trim());
      void triggerDuplicateCheck(topicParam.trim());
    }
  }, []);

  const getHeaders = () => {
    const effectiveUserId =
      activeUserHeaderId ??
      (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
    return getClientRequestHeaders(
      effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
    );
  };

  const navigateToCreatedCourse = (data: { courseId?: string; id?: string }, isEdit: boolean = false) => {
    const newId = data.courseId || data.id || "";
    if (!newId) {
      throw new Error("Máy chủ không trả về mã khóa học vừa tạo.");
    }
    if (isEdit) {
      router.push(`/app/courses/${newId}/edit`);
    } else {
      router.push(`/app/courses/${newId}`);
    }
  };

  // Duplicate Checking
  const triggerDuplicateCheck = async (queryText: string) => {
    const trimmed = queryText.trim();
    if (!trimmed || trimmed.length < 3) {
      setDuplicates([]);
      return;
    }

    setIsCheckingDuplicates(true);
    try {
      const resp = await fetch("/api/v1/courses/check-duplicates", {
        method: "POST",
        headers: {
          ...getHeaders(),
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ topic: trimmed, title: trimmed }),
      });
      if (resp.ok) {
        const data = await resp.json();
        setDuplicates(data.duplicates || []);
      }
    } catch {
      // Ignore background duplicate check failure
    } finally {
      setIsCheckingDuplicates(false);
    }
  };

  const handleAiTopicChange = (val: string) => {
    setAiTopic(val);
    setDuplicateBannerDismissed(false);
    if (previewData) setPreviewData(null);
    if (clarifyQuestions.length > 0) setClarifyQuestions([]);
    setWizardStep(1);

    if (duplicateDebounceRef.current) {
      clearTimeout(duplicateDebounceRef.current);
    }
    duplicateDebounceRef.current = setTimeout(() => {
      void triggerDuplicateCheck(val);
    }, 400);
  };

  const processDocumentFile = async (file: File) => {
    setUploadLoading(true);
    setUploadFileName(file.name);
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
      setUploadFileName(null);
      if (uploadInputRef.current) uploadInputRef.current.value = "";
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    await processDocumentFile(file);
  };

  // Step 1: AI Clarify
  const handleClarifyQuestions = async () => {
    const cleanTopic = aiTopic.trim();
    if (!cleanTopic) return;
    setClarifyLoading(true);
    setErrorMessage(null);
    try {
      const res = await fetch("/api/v1/courses/generate-wizard/clarify", {
        method: "POST",
        headers: {
          ...getHeaders(),
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ topic: cleanTopic }),
      });
      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        throw new Error(errJson.error || `Không thể tạo câu hỏi làm rõ (Mã ${res.status})`);
      }
      const data = await res.json();
      setClarifyQuestions(data.questions || []);
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : "Lỗi khi tạo câu hỏi làm rõ.");
    } finally {
      setClarifyLoading(false);
    }
  };

  // Step 2: Request Preview
  const requestPreview = async (cleanTopic: string) => {
    const previewRes = await fetch("/api/v1/courses/generate-wizard/preview", {
      method: "POST",
      headers: {
        ...getHeaders(),
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        topic: cleanTopic,
        answers: clarifyAnswers,
      }),
    });
    if (!previewRes.ok) {
      const errJson = await previewRes.json().catch(() => ({}));
      throw new Error(
        errJson.error || `Không thể sinh đề cương (Mã lỗi ${previewRes.status})`
      );
    }
    const data = await previewRes.json();
    return {
      title: data.courseTitle || data.title,
      description: data.description,
      chapters: data.chapters || [],
    } as WizardPreviewData;
  };

  const handleAiPreview = async () => {
    const cleanTopic = aiTopic.trim();
    if (!cleanTopic) return;
    setAiLoading(true);
    setErrorMessage(null);
    try {
      const preview = await requestPreview(cleanTopic);
      setPreviewData(preview);
      setWizardStep(2);
    } catch (err) {
      setErrorMessage(
        err instanceof Error ? err.message : "Không thể tạo bản xem trước đề cương."
      );
    } finally {
      setAiLoading(false);
    }
  };

  // Step 3: Commit Wizard Course
  const commitWizardCourse = async (cleanTopic: string, preview: WizardPreviewData) => {
    setWizardStep(3);
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
        chapters: (preview.chapters || []).map((ch, idx) => ({
          title: ch.title || `Chương ${idx + 1}`,
          sortOrder: ch.sortOrder ?? idx + 1,
          lessons: (ch.lessons || []).map((l, lIdx) => ({
            title: l.title || `Bài ${lIdx + 1}`,
            summary: l.summary || "",
            estimatedMinutes: l.estimatedMinutes || 10,
          })),
        })),
      }),
    });

    if (!commitRes.ok) {
      const errJson = await commitRes.json().catch(() => ({}));
      throw new Error(errJson.error || `Lưu khóa học thất bại (Mã lỗi ${commitRes.status})`);
    }

    const data = await commitRes.json();
    navigateToCreatedCourse(data);
  };

  const handleAiCreateDirect = async () => {
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

  // Method 4: Create Blank Course
  const handleCreateBlankCourse = async (e: React.FormEvent) => {
    e.preventDefault();
    const title = blankTitle.trim();
    if (!title) {
      setErrorMessage("Vui lòng nhập tên khóa học.");
      return;
    }

    setBlankLoading(true);
    setErrorMessage(null);
    try {
      const commitRes = await fetch("/api/v1/courses/generate-wizard/commit", {
        method: "POST",
        headers: {
          ...getHeaders(),
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          topic: title,
          title,
          description: blankDescription.trim() || `Khóa học tùy chỉnh: ${title}`,
          chapters: [
            {
              title: "Chương 1: Tổng quan và mở đầu",
              sortOrder: 1,
              lessons: [
                {
                  title: "Bài 1: Giới thiệu khóa học",
                  summary: "Mục tiêu và lộ trình tiếp cận.",
                  estimatedMinutes: 10,
                },
              ],
            },
          ],
        }),
      });

      if (!commitRes.ok) {
        const errJson = await commitRes.json().catch(() => ({}));
        throw new Error(errJson.error || `Khởi tạo khóa học thất bại (${commitRes.status})`);
      }

      const created = await commitRes.json();
      const newCourseId = created.id || created.courseId;

      // Update additional metadata if provided
      const filteredOutcomes = blankOutcomes.map((o) => o.trim()).filter(Boolean);
      if (newCourseId && (blankCategory || blankLevel || filteredOutcomes.length > 0)) {
        await fetch(`/api/v1/courses/${newCourseId}`, {
          method: "PUT",
          headers: {
            ...getHeaders(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            category: blankCategory,
            level: blankLevel,
            learningOutcomes: filteredOutcomes,
          }),
        }).catch(() => null);
      }

      navigateToCreatedCourse(created, true);
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : "Đã xảy ra lỗi khi tạo khóa học trống.");
    } finally {
      setBlankLoading(false);
    }
  };

  const processImportSwmFile = async (file: File) => {
    setImportLoading(true);
    setImportFileName(file.name);
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
      setImportFileName(null);
      if (importInputRef.current) importInputRef.current.value = "";
    }
  };

  const handleImportSwm = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    await processImportSwmFile(file);
  };

  const anyLoading = uploadLoading || aiLoading || importLoading || blankLoading;

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10">
      {/* Breadcrumb */}
      <nav aria-label="Breadcrumb" className="flex items-center gap-2 text-sm text-[var(--text-tertiary)] mb-5">
        <Link href="/app/courses" className="hover:text-[var(--text-primary)] transition">
          Khóa học
        </Link>
        <span>·</span>
        <span className="text-[var(--text-primary)] font-medium">Tạo khóa học mới</span>
      </nav>

      <div className="mb-6">
        <h1 className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">
          Tạo khóa học mới
        </h1>
        <p className="text-sm text-[var(--text-secondary)] mt-1.5">
          Chọn cách khởi tạo phù hợp từ trợ lý AI 3 bước, tài liệu sẵn có, khung trống cá nhân hoặc nhập gói lưu trữ.
        </p>
      </div>

      {/* Duplicate Warning Banner */}
      {duplicates.length > 0 && !duplicateBannerDismissed && (
        <div className="mb-6 rounded-xl border border-amber-500/40 bg-amber-500/10 p-4 space-y-2.5 animate-in fade-in duration-200">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-2 text-amber-700 dark:text-amber-300 font-semibold text-xs sm:text-sm">
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>Phát hiện khóa học có nội dung tương đồng trong hệ thống ({duplicates.length})</span>
            </div>
            <div className="flex items-center gap-2.5">
              <span className="hidden sm:inline text-[11px] text-amber-600/80 dark:text-amber-400/80">
                Tránh tạo trùng khóa học
              </span>
              <button
                type="button"
                onClick={() => setDuplicateBannerDismissed(true)}
                className="px-2 py-1 rounded-md border border-amber-500/30 bg-bg-primary/70 text-[11px] font-medium text-amber-800 dark:text-amber-200 hover:bg-bg-primary transition cursor-pointer"
              >
                Ẩn cảnh báo
              </button>
            </div>
          </div>
          <p className="text-xs text-text-secondary leading-relaxed">
            Hệ thống tìm thấy các khóa học tương tự với chủ đề của bạn. Bạn có thể vào học ngay hoặc xem nội dung trước khi quyết định tạo mới:
          </p>
          <div className="space-y-1.5 max-h-48 overflow-y-auto swms-thin-scrollbar pr-1">
            {duplicates.map((dup) => (
              <div
                key={dup.id}
                className="flex items-center justify-between gap-3 p-2.5 rounded-lg bg-bg-primary border border-border text-xs hover:border-amber-500/30 transition"
              >
                <div className="flex items-center gap-2 min-w-0">
                  <Link
                    href={`/app/courses/${dup.id}`}
                    className="font-semibold text-text-primary hover:text-accent hover:underline truncate"
                  >
                    {dup.title}
                  </Link>
                  <span className="text-[10px] px-1.5 py-0.5 rounded bg-bg-secondary text-text-tertiary shrink-0">
                    {dup.lessonCount} bài
                  </span>
                  {dup.isSystemCourse ? (
                    <span className="text-[10px] px-1.5 py-0.5 rounded bg-blue-500/10 text-blue-600 dark:text-blue-300 font-semibold shrink-0">
                      Hệ thống
                    </span>
                  ) : (
                    <span className="text-[10px] px-1.5 py-0.5 rounded bg-purple-500/10 text-purple-600 dark:text-purple-300 font-semibold shrink-0">
                      Cá nhân
                    </span>
                  )}
                  {dup.matchScore > 0 && (
                    <span className="text-[10px] text-amber-600 dark:text-amber-400 font-medium shrink-0">
                      Độ khớp: {Math.round(dup.matchScore)}%
                    </span>
                  )}
                </div>
                <Link
                  href={`/app/courses/${dup.id}`}
                  className="inline-flex items-center gap-1 text-xs text-accent font-medium hover:underline shrink-0"
                >
                  <span>Mở khóa học</span>
                  <ArrowRight className="w-3 h-3" />
                </Link>
              </div>
            ))}
          </div>
        </div>
      )}

      {errorMessage && (
        <div className="mb-6 rounded-[10px] border border-red-500/30 bg-red-500/5 px-4 py-3 text-sm text-red-600 dark:text-red-400 flex items-center gap-2">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        <div className="lg:col-span-7 space-y-5">
          {/* Method 1: AI Topic Generator & 3-Step Wizard */}
          <Card className="p-6 border-accent/30 shadow-xs">
            <div className="flex items-start gap-4">
              <div className="w-10 h-10 rounded-[10px] bg-accent-subtle border border-accent/30 flex items-center justify-center shrink-0">
                <Sparkles className="w-5 h-5 text-accent" />
              </div>
              <div className="flex-1 min-w-0">
                <div className="mb-1">
                  <h2 className="text-base font-semibold text-[var(--text-primary)]">
                    1. Khởi tạo lộ trình với Trợ lý AI (3 Bước)
                  </h2>
                </div>
                <p className="text-sm text-[var(--text-secondary)] mb-3">
                  Nhập chủ đề môn học hoặc kỹ năng, làm rõ mục tiêu đào tạo và tinh chỉnh đề cương trước khi lưu.
                </p>

                {/* Visual 3-Step Stepper */}
                <div
                  aria-label="Tiến trình khởi tạo AI 3 bước"
                  className="mb-4 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/70 p-3 sm:p-3.5"
                >
                  <div className="flex items-center justify-between gap-2 mb-2">
                    <span className="text-[11px] font-semibold uppercase tracking-wider text-[var(--text-tertiary)]">
                      Quy trình 3 bước
                    </span>
                    <span className="text-[11px] font-semibold px-2 py-0.5 rounded-full bg-accent-subtle text-accent dark:bg-[#818CF8]/15 dark:text-[#A5B4FC]">
                      Bước {wizardStep}/3
                    </span>
                  </div>
                  <div className="flex items-center justify-between gap-1.5 sm:gap-2">
                    {[
                      { step: 1 as const, label: "Chủ đề & Mục tiêu" },
                      { step: 2 as const, label: "Tinh chỉnh Đề cương" },
                      { step: 3 as const, label: "Khởi tạo Khóa học" },
                    ].map((item, idx) => {
                      const isCompleted = wizardStep > item.step;
                      const isActive = wizardStep === item.step;
                      return (
                        <React.Fragment key={item.step}>
                          <div className="flex items-center gap-1.5 sm:gap-2 min-w-0">
                            <span
                              className={`w-6 h-6 rounded-full border flex items-center justify-center text-[11px] font-bold shrink-0 transition-all ${
                                isCompleted
                                  ? "bg-emerald-600 border-emerald-600 text-white dark:bg-emerald-500 dark:border-emerald-500 dark:text-[#0F0F12]"
                                  : isActive
                                  ? "bg-accent border-accent text-white ring-2 ring-accent/25 dark:bg-[#818CF8] dark:border-[#818CF8] dark:text-[#0F0F12] dark:ring-[#818CF8]/30 shadow-xs"
                                  : "bg-[var(--surface-card)] border-[var(--border-calm)] text-[var(--text-tertiary)]"
                              }`}
                            >
                              {isCompleted ? <Check className="w-3.5 h-3.5" /> : item.step}
                            </span>
                            <span
                              className={`text-[11px] sm:text-xs truncate leading-tight ${
                                isCompleted
                                  ? "text-emerald-700 dark:text-emerald-300 font-medium"
                                  : isActive
                                  ? "text-accent dark:text-[#A5B4FC] font-semibold"
                                  : "text-[var(--text-tertiary)]"
                              }`}
                            >
                              {item.label}
                            </span>
                          </div>
                          {idx < 2 && (
                            <div
                              className={`h-0.5 flex-1 min-w-3 rounded-full transition-colors ${
                                wizardStep > item.step
                                  ? "bg-emerald-500 dark:bg-emerald-400"
                                  : "bg-[var(--border-calm)]"
                              }`}
                            />
                          )}
                        </React.Fragment>
                      );
                    })}
                  </div>
                </div>

                {/* Step 1: Input Topic & Clarify */}
                <div className="space-y-3">
                  <div className="relative">
                    <input
                      ref={aiTopicInputRef}
                      type="text"
                      value={aiTopic}
                      onChange={(e) => handleAiTopicChange(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === "Enter" && !anyLoading) void handleAiPreview();
                      }}
                      placeholder="Ví dụ: Thiết kế hệ thống Microservices, Tiếng Anh giao tiếp công sở, Python cơ bản..."
                      disabled={anyLoading}
                      className="w-full h-10 px-3.5 pr-8 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] dark:focus:border-[#818CF8] focus:ring-1 focus:ring-[#5E51E8] dark:focus:ring-[#818CF8]"
                    />
                    {isCheckingDuplicates && (
                      <div className="absolute right-3 top-3">
                        <Loader2 className="w-4 h-4 text-text-tertiary animate-spin" />
                      </div>
                    )}
                  </div>

                  {/* 4 Quick-Pick Topic Inspiration Chips */}
                  <div
                    data-testid="ai-topic-inspiration-chips"
                    className="flex flex-wrap items-center gap-1.5"
                  >
                    <span className="text-[11px] font-medium text-[var(--text-tertiary)] mr-0.5">
                      Gợi ý nhanh:
                    </span>
                    {TOPIC_INSPIRATIONS.map((topic) => {
                      const isSelected = aiTopic.trim() === topic;
                      return (
                        <button
                          key={topic}
                          type="button"
                          disabled={anyLoading}
                          onClick={() => {
                            handleAiTopicChange(topic);
                            aiTopicInputRef.current?.focus();
                          }}
                          className={`px-2.5 py-1 rounded-full text-[11px] font-medium border transition cursor-pointer disabled:opacity-50 ${
                            isSelected
                              ? "bg-accent text-white border-accent dark:bg-[#818CF8] dark:text-[#0F0F12] dark:border-[#818CF8] shadow-2xs"
                              : "bg-[var(--surface-elevated)] text-[var(--text-secondary)] border-[var(--border-calm)] hover:border-accent/50 hover:text-accent dark:hover:border-[#818CF8]/60 dark:hover:text-[#A5B4FC]"
                          }`}
                        >
                          {topic}
                        </button>
                      );
                    })}
                  </div>

                  {/* Clarify Questions Section */}
                  {clarifyQuestions.length > 0 && (
                    <div className="p-4 rounded-xl border border-accent/20 bg-accent-subtle/30 space-y-3 animate-in fade-in duration-150">
                      <div className="flex items-center gap-1.5 text-xs font-semibold text-accent">
                        <HelpCircle className="w-3.5 h-3.5" />
                        <span>Câu hỏi làm rõ định hướng (giúp tạo đề cương sát mục tiêu hơn)</span>
                      </div>
                      <div className="space-y-3">
                        {clarifyQuestions.map((cq) => (
                          <div key={cq.id} className="space-y-1.5">
                            <label className="text-xs font-medium text-text-primary block">
                              {cq.question}
                            </label>
                            <div className="flex flex-wrap gap-1.5">
                              {cq.options.map((opt) => {
                                const selected = clarifyAnswers[cq.id] === opt;
                                return (
                                  <button
                                    key={opt}
                                    type="button"
                                    onClick={() =>
                                      setClarifyAnswers((prev) => ({
                                        ...prev,
                                        [cq.id]: selected ? "" : opt,
                                      }))
                                    }
                                    className={`px-2.5 py-1 rounded-lg text-xs font-medium border transition cursor-pointer ${
                                      selected
                                        ? "bg-accent text-white border-accent shadow-xs dark:bg-[#818CF8] dark:text-[#0F0F12] dark:border-[#818CF8]"
                                        : "bg-bg-primary text-text-secondary border-border hover:border-accent/40"
                                    }`}
                                  >
                                    {opt}
                                  </button>
                                );
                              })}
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  <div className="flex flex-wrap items-center gap-2 pt-1">
                    <Button
                      variant="secondary"
                      disabled={anyLoading || !aiTopic.trim()}
                      isLoading={clarifyLoading}
                      onClick={handleClarifyQuestions}
                      className="shrink-0 text-xs"
                    >
                      <span>Làm rõ mục tiêu</span>
                    </Button>
                    <Button
                      variant="secondary"
                      disabled={anyLoading || !aiTopic.trim()}
                      isLoading={aiLoading && !previewData}
                      onClick={handleAiPreview}
                      className="shrink-0 text-xs"
                    >
                      <BookOpen className="w-3.5 h-3.5 mr-1" />
                      <span>Xem trước đề cương (Bước 2)</span>
                    </Button>
                    <Button
                      variant="primary"
                      disabled={anyLoading || !aiTopic.trim()}
                      isLoading={aiLoading}
                      onClick={handleAiCreateDirect}
                      className="shrink-0 text-xs bg-accent text-white hover:bg-accent-hover dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC]"
                    >
                      <span>Tạo khóa học nhanh</span>
                    </Button>
                  </div>
                </div>

                {/* Step 2: Interactive Preview & Edit Syllabus */}
                {previewData && (
                  <div className="mt-5 rounded-xl border border-accent/40 bg-[var(--surface-elevated)] p-4 space-y-3 animate-in fade-in duration-200">
                    <div className="flex items-center justify-between gap-2 border-b border-border pb-2.5">
                      <div className="flex items-center gap-2 text-xs font-semibold text-accent">
                        <BookOpen className="w-4 h-4" />
                        <span>Bước 2: Xem trước & Chỉnh sửa đề cương</span>
                      </div>
                      <span className="text-[11px] text-text-tertiary">
                        Nhấp vào tiêu đề để sửa trước khi lưu
                      </span>
                    </div>

                    <div className="space-y-2">
                      <label className="text-xs font-medium text-text-secondary block">
                        Tên khóa học dự kiến:
                      </label>
                      <input
                        type="text"
                        value={previewData.title || ""}
                        onChange={(e) =>
                          setPreviewData({ ...previewData, title: e.target.value })
                        }
                        className="w-full h-8 px-3 rounded-lg border border-border bg-bg-primary text-xs font-semibold text-text-primary focus:outline-none focus:border-accent"
                      />
                    </div>

                    <div className="space-y-2">
                      <label className="text-xs font-medium text-text-secondary block">
                        Mô tả tóm tắt:
                      </label>
                      <textarea
                        value={previewData.description || ""}
                        onChange={(e) =>
                          setPreviewData({ ...previewData, description: e.target.value })
                        }
                        rows={2}
                        className="w-full p-2.5 rounded-lg border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent resize-none"
                      />
                    </div>

                    {/* Chapters & Lessons editing list */}
                    <div className="space-y-2 max-h-72 overflow-y-auto swms-thin-scrollbar pr-1 pt-1">
                      {(previewData.chapters || []).map((ch, chIdx) => (
                        <div
                          key={chIdx}
                          className="rounded-lg bg-bg-primary p-3 border border-border space-y-2"
                        >
                          <div className="flex items-center justify-between gap-2">
                            <span className="text-[11px] font-semibold text-text-tertiary shrink-0">
                              Chương {chIdx + 1}:
                            </span>
                            <input
                              type="text"
                              value={ch.title}
                              onChange={(e) => {
                                const nextChs = [...(previewData.chapters || [])];
                                nextChs[chIdx] = { ...nextChs[chIdx], title: e.target.value };
                                setPreviewData({ ...previewData, chapters: nextChs });
                              }}
                              className="flex-1 h-7 px-2 rounded border border-border bg-bg-secondary text-xs font-medium text-text-primary focus:outline-none focus:border-accent"
                            />
                            <button
                              type="button"
                              onClick={() => {
                                const nextChs = (previewData.chapters || []).filter(
                                  (_, idx) => idx !== chIdx
                                );
                                setPreviewData({ ...previewData, chapters: nextChs });
                              }}
                              className="p-1 rounded text-text-tertiary hover:text-red-500 hover:bg-bg-secondary transition cursor-pointer"
                              title="Xóa chương này"
                            >
                              <Trash2 className="w-3.5 h-3.5" />
                            </button>
                          </div>

                          {/* Lessons list inside chapter */}
                          <div className="pl-4 space-y-1.5 border-l-2 border-border/80">
                            {(ch.lessons || []).map((les, lIdx) => (
                              <div key={lIdx} className="flex items-center gap-1.5">
                                <span className="text-[10px] text-text-tertiary shrink-0">
                                  Bài {lIdx + 1}:
                                </span>
                                <input
                                  type="text"
                                  value={les.title}
                                  onChange={(e) => {
                                    const nextChs = [...(previewData.chapters || [])];
                                    const nextLessons = [...(nextChs[chIdx].lessons || [])];
                                    nextLessons[lIdx] = {
                                      ...nextLessons[lIdx],
                                      title: e.target.value,
                                    };
                                    nextChs[chIdx] = { ...nextChs[chIdx], lessons: nextLessons };
                                    setPreviewData({ ...previewData, chapters: nextChs });
                                  }}
                                  className="flex-1 h-6 px-2 rounded border border-border/70 bg-bg-primary text-[11px] text-text-primary focus:outline-none focus:border-accent"
                                />
                                <button
                                  type="button"
                                  onClick={() => {
                                    const nextChs = [...(previewData.chapters || [])];
                                    const nextLessons = (nextChs[chIdx].lessons || []).filter(
                                      (_, idx) => idx !== lIdx
                                    );
                                    nextChs[chIdx] = { ...nextChs[chIdx], lessons: nextLessons };
                                    setPreviewData({ ...previewData, chapters: nextChs });
                                  }}
                                  className="p-0.5 text-text-tertiary hover:text-red-500 transition cursor-pointer"
                                  title="Xóa bài này"
                                >
                                  <Trash2 className="w-3 h-3" />
                                </button>
                              </div>
                            ))}

                            <button
                              type="button"
                              onClick={() => {
                                const nextChs = [...(previewData.chapters || [])];
                                const currentLessons = nextChs[chIdx].lessons || [];
                                nextChs[chIdx] = {
                                  ...nextChs[chIdx],
                                  lessons: [
                                    ...currentLessons,
                                    {
                                      title: `Bài ${currentLessons.length + 1}: Nội dung mới`,
                                      estimatedMinutes: 10,
                                    },
                                  ],
                                };
                                setPreviewData({ ...previewData, chapters: nextChs });
                              }}
                              className="inline-flex items-center gap-1 text-[11px] text-accent font-medium hover:underline pt-1 cursor-pointer"
                            >
                              <Plus className="w-3 h-3" />
                              <span>Thêm bài vào chương</span>
                            </button>
                          </div>
                        </div>
                      ))}

                      <button
                        type="button"
                        onClick={() => {
                          const currentChapters = previewData.chapters || [];
                          setPreviewData({
                            ...previewData,
                            chapters: [
                              ...currentChapters,
                              {
                                title: `Chương ${currentChapters.length + 1}: Chủ đề bổ sung`,
                                sortOrder: currentChapters.length + 1,
                                lessons: [{ title: "Bài 1: Khởi động", estimatedMinutes: 10 }],
                              },
                            ],
                          });
                        }}
                        className="w-full py-2 rounded-lg border border-dashed border-border text-center text-xs text-accent font-medium hover:bg-accent-subtle/30 transition cursor-pointer"
                      >
                        + Thêm chương mới vào đề cương
                      </button>
                    </div>

                    {/* Step 3: Commit Button */}
                    <div className="pt-2 border-t border-border flex flex-wrap items-center justify-between gap-2">
                      <button
                        type="button"
                        onClick={() => setPreviewData(null)}
                        className="text-xs text-text-tertiary hover:text-text-primary transition cursor-pointer"
                      >
                        ← Hủy đề cương &amp; nhập lại chủ đề
                      </button>
                      <Button
                        variant="primary"
                        isLoading={aiLoading}
                        onClick={() => void commitWizardCourse(aiTopic.trim(), previewData)}
                        className="text-xs bg-accent text-white hover:bg-accent-hover dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC]"
                      >
                        <Check className="w-3.5 h-3.5 mr-1" />
                        <span>Bước 3: Xác nhận &amp; Khởi tạo khóa học</span>
                      </Button>
                    </div>
                  </div>
                )}
              </div>
            </div>
          </Card>

          {/* Method 2: Blank Template (Tạo khóa học trống) */}
          <Card className="p-6">
            <div className="flex items-start gap-4">
              <div className="w-10 h-10 rounded-[10px] bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center shrink-0">
                <FilePlus className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
              </div>
              <div className="flex-1 min-w-0">
                <h2 className="text-base font-semibold text-[var(--text-primary)] mb-1">
                  2. Tạo khóa học thủ công (Trống)
                </h2>
                <p className="text-sm text-[var(--text-secondary)] mb-4">
                  Tự thiết kế cấu trúc bài học từ đầu với đầy đủ thuộc tính metadata, cấp độ và mục tiêu đầu ra.
                </p>

                <form onSubmit={handleCreateBlankCourse} className="space-y-3">
                  <div className="space-y-1">
                    <label className="text-xs font-medium text-text-secondary">
                      Tên khóa học <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      value={blankTitle}
                      onChange={(e) => setBlankTitle(e.target.value)}
                      placeholder="Ví dụ: Cẩm nang Lập trình Fullstack, Quản lý tài chính cá nhân..."
                      disabled={anyLoading}
                      required
                      className="w-full h-9 px-3 rounded-lg border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                    />
                  </div>

                  <div className="space-y-1">
                    <label className="text-xs font-medium text-text-secondary">
                      Mô tả tổng quan (tùy chọn)
                    </label>
                    <textarea
                      value={blankDescription}
                      onChange={(e) => setBlankDescription(e.target.value)}
                      placeholder="Mục đích và phạm vi kiến thức..."
                      rows={2}
                      disabled={anyLoading}
                      className="w-full p-2.5 rounded-lg border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent resize-none"
                    />
                  </div>

                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    <div className="space-y-1">
                      <label className="text-xs font-medium text-text-secondary">Lĩnh vực</label>
                      <input
                        type="text"
                        value={blankCategory}
                        onChange={(e) => setBlankCategory(e.target.value)}
                        placeholder="Công nghệ, Kinh doanh, v.v."
                        className="w-full h-8 px-2.5 rounded-lg border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                      />
                    </div>
                    <div className="space-y-1">
                      <label className="text-xs font-medium text-text-secondary">Trình độ</label>
                      <select
                        value={blankLevel}
                        onChange={(e) =>
                          setBlankLevel(e.target.value as "foundation" | "intermediate" | "advanced")
                        }
                        className="w-full h-8 px-2 rounded-lg border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                      >
                        <option value="foundation">Nền tảng</option>
                        <option value="intermediate">Trung cấp</option>
                        <option value="advanced">Chuyên sâu</option>
                      </select>
                    </div>
                  </div>

                  {/* Learning outcomes dynamic input */}
                  <div className="space-y-1.5 pt-1">
                    <label className="text-xs font-medium text-text-secondary block">
                      Mục tiêu đầu ra:
                    </label>
                    {blankOutcomes.map((outc, oIdx) => (
                      <div key={oIdx} className="flex items-center gap-1.5">
                        <input
                          type="text"
                          value={outc}
                          onChange={(e) => {
                            const next = [...blankOutcomes];
                            next[oIdx] = e.target.value;
                            setBlankOutcomes(next);
                          }}
                          placeholder={`Mục tiêu ${oIdx + 1}...`}
                          className="flex-1 h-7 px-2 rounded border border-border bg-bg-primary text-xs text-text-primary focus:outline-none focus:border-accent"
                        />
                        {blankOutcomes.length > 1 && (
                          <button
                            type="button"
                            onClick={() =>
                              setBlankOutcomes(blankOutcomes.filter((_, idx) => idx !== oIdx))
                            }
                            className="p-1 text-text-tertiary hover:text-red-500 cursor-pointer"
                          >
                            <Trash2 className="w-3 h-3" />
                          </button>
                        )}
                      </div>
                    ))}
                    <button
                      type="button"
                      onClick={() => setBlankOutcomes([...blankOutcomes, ""])}
                      className="inline-flex items-center gap-1 text-[11px] text-accent font-medium hover:underline cursor-pointer"
                    >
                      <Plus className="w-3 h-3" />
                      <span>Thêm mục tiêu</span>
                    </button>
                  </div>

                  <div className="pt-2">
                    <Button
                      variant="primary"
                      type="submit"
                      disabled={anyLoading || !blankTitle.trim()}
                      isLoading={blankLoading}
                      className="text-xs bg-accent text-white hover:bg-accent-hover dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC]"
                    >
                      <span>Khởi tạo khóa học &amp; Mở Studio soạn bài</span>
                    </Button>
                  </div>
                </form>
              </div>
            </div>
          </Card>

          {/* Method 3: Upload Document */}
          <Card className="p-6">
            <div className="flex items-start gap-4">
              <div className="w-10 h-10 rounded-[10px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0">
                <FileUp className="w-5 h-5 text-[#5E51E8] dark:text-[#818CF8]" />
              </div>
              <div className="flex-1 min-w-0">
                <h2 className="text-base font-semibold text-[var(--text-primary)] mb-1">
                  3. Tạo từ tài liệu sẵn có
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
                <div
                  onDragOver={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    if (!anyLoading) setUploadDragActive(true);
                  }}
                  onDragLeave={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    setUploadDragActive(false);
                  }}
                  onDrop={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    setUploadDragActive(false);
                    if (anyLoading) return;
                    const file = e.dataTransfer.files?.[0];
                    if (file) void processDocumentFile(file);
                  }}
                  className={`rounded-xl border-2 border-dashed p-4 transition-all duration-150 ${
                    uploadDragActive
                      ? "border-accent bg-accent-subtle/50 ring-2 ring-accent/25 dark:border-[#818CF8] dark:bg-[#818CF8]/15 dark:ring-[#818CF8]/30 scale-[1.005]"
                      : "border-[var(--border-calm)] bg-[var(--surface-elevated)]/40 hover:border-accent/50 dark:hover:border-[#818CF8]/60"
                  }`}
                >
                  <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                    <div className="min-w-0 space-y-1">
                      <p className="text-xs font-medium text-[var(--text-primary)]">
                        {uploadDragActive
                          ? "Thả tài liệu vào đây để tải lên..."
                          : "Kéo thả tài liệu PDF, Word, TXT hoặc Markdown vào đây hoặc bấm chọn tệp"}
                      </p>
                      {uploadFileName ? (
                        <p className="text-xs font-medium text-accent dark:text-[#A5B4FC] truncate">
                          Đang xử lý: {uploadFileName}
                        </p>
                      ) : (
                        <p className="text-[11px] text-[var(--text-tertiary)]">
                          Hỗ trợ: .pdf, .doc, .docx, .txt, .md, .zip, .swm
                        </p>
                      )}
                    </div>
                    <Button
                      variant="primary"
                      disabled={anyLoading}
                      isLoading={uploadLoading}
                      onClick={() => uploadInputRef.current?.click()}
                      leftIcon={<Upload className="w-4 h-4" />}
                      className="shrink-0 bg-accent text-white hover:bg-accent-hover dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC]"
                    >
                      <span>{uploadLoading ? "Đang xử lý tài liệu..." : "Chọn tệp tài liệu"}</span>
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          </Card>

          {/* Method 4: Import SWM ZIP */}
          <Card className="p-6">
            <div className="flex items-start gap-4">
              <div className="w-10 h-10 rounded-[10px] bg-[var(--surface-elevated)] border border-[var(--border-calm)] flex items-center justify-center shrink-0">
                <Package className="w-5 h-5 text-[var(--text-secondary)]" />
              </div>
              <div className="flex-1 min-w-0">
                <h2 className="text-base font-semibold text-[var(--text-primary)] mb-1">
                  4. Nhập gói khóa học (.swm / .zip)
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
                <div
                  onDragOver={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    if (!anyLoading) setImportDragActive(true);
                  }}
                  onDragLeave={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    setImportDragActive(false);
                  }}
                  onDrop={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    setImportDragActive(false);
                    if (anyLoading) return;
                    const file = e.dataTransfer.files?.[0];
                    if (file) void processImportSwmFile(file);
                  }}
                  className={`rounded-xl border-2 border-dashed p-4 space-y-3 transition-all duration-150 ${
                    importDragActive
                      ? "border-accent bg-accent-subtle/50 ring-2 ring-accent/25 dark:border-[#818CF8] dark:bg-[#818CF8]/15 dark:ring-[#818CF8]/30 scale-[1.005]"
                      : "border-[var(--border-calm)] bg-[var(--surface-elevated)]/40 hover:border-accent/50 dark:hover:border-[#818CF8]/60"
                  }`}
                >
                  <div className="min-w-0 space-y-1">
                    <p className="text-xs font-medium text-[var(--text-primary)]">
                      {importDragActive
                        ? "Thả gói .swm hoặc .zip vào đây để nhập..."
                        : "Kéo thả gói khóa học (.swm / .zip) vào đây hoặc dùng nút bên dưới"}
                    </p>
                    {importFileName && (
                      <p className="text-xs font-medium text-accent truncate">
                        Đang nhập gói: {importFileName}
                      </p>
                    )}
                  </div>
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
            </div>
          </Card>
        </div>

        {/* Right Column: 1-Click Use Cases & Roadmap Templates */}
        <aside className="lg:col-span-5 space-y-4">
          <Card className="p-5 space-y-4">
            <div className="space-y-1">
              <div className="inline-flex items-center gap-1.5 text-xs font-semibold text-[#5E51E8] dark:text-[#A5B4FC]">
                <Compass className="w-4 h-4" />
                <span>Mẫu lộ trình &amp; gợi ý khởi tạo nhanh</span>
              </div>
              <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
                Chọn một mẫu bên dưới để điền sẵn chủ đề cho trợ lý AI.
              </p>
            </div>

            <div className="space-y-2.5 max-h-[540px] overflow-y-auto swms-thin-scrollbar pr-1">
              {STUDY_USE_CASES.slice(0, 6).map((uc) => (
                <button
                  key={uc.id}
                  type="button"
                  onClick={() => {
                    handleAiTopicChange(uc.aiTemplateTopic);
                    aiTopicInputRef.current?.focus();
                  }}
                  className="w-full text-left rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 hover:border-[#5E51E8] hover:bg-[#EEF0FD]/50 dark:hover:bg-[#1E1B4B]/40 p-3 transition cursor-pointer group space-y-1"
                >
                  <div className="flex items-center justify-between gap-2">
                    <span className="text-[10px] font-semibold px-2 py-0.5 rounded bg-[var(--surface-card)] border border-[var(--border-calm)] text-[#5E51E8] dark:text-[#A5B4FC]">
                      {uc.persona}
                    </span>
                    <span className="text-[11px] font-medium text-[var(--text-tertiary)] group-hover:text-[#5E51E8] inline-flex items-center gap-1">
                      <Zap className="w-3 h-3" />
                      <span>Dùng mẫu</span>
                    </span>
                  </div>
                  <p className="text-xs font-semibold text-[var(--text-primary)] leading-snug">
                    {uc.title}
                  </p>
                  <p className="text-[11px] text-[var(--text-tertiary)] line-clamp-1">
                    Chủ đề: {uc.aiTemplateTopic}
                  </p>
                </button>
              ))}

              <div className="pt-2 border-t border-[var(--border-calm)]">
                <p className="text-[11px] font-semibold text-[var(--text-tertiary)] uppercase tracking-wider mb-2">
                  Gợi ý chặng từ {LEARNING_ROADMAPS.length} lộ trình
                </p>
                <div className="flex flex-wrap gap-1.5">
                  {LEARNING_ROADMAPS.slice(0, 4).map((rm) => (
                    <button
                      key={rm.id}
                      type="button"
                      onClick={() => {
                        handleAiTopicChange(rm.milestones[0]?.aiTopicPrompt || rm.title);
                        aiTopicInputRef.current?.focus();
                      }}
                      className="px-2.5 py-1 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-card)] hover:border-[#5E51E8] text-[11px] font-medium text-[var(--text-secondary)] hover:text-[#5E51E8] transition cursor-pointer"
                    >
                      + {rm.category}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </Card>
        </aside>
      </div>
    </div>
  );
}
