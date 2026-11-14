"use client";
import React, { useEffect, useState } from "react";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { Sparkles, Brain, Clock, BookOpen, FileText, PenTool, Target, ShieldCheck, CheckCircle2, Circle, ArrowRight, Zap, Trophy, Tag, Upload, FileUp, AlertCircle, Trash2, X, Flame, Layers } from "lucide-react";
export function PersonalWorkspaceDashboard({ data, onRefreshDashboard, courses: propCourses }) {
    const store = useWorkspaceStore();
    const { setActiveMode, setActiveReviewSubTab, setActiveLesson, setActiveDocument, setSelectedTagFilterId, setActiveCourseId, setActiveQuizId } = store;
    const [uploadModalOpen, setUploadModalOpen] = useState(false);
    const [docTitle, setDocTitle] = useState("");
    const [docText, setDocText] = useState("");
    const [uploading, setUploading] = useState(false);
    const [selectedFile, setSelectedFile] = useState(null);
    const [fileSizeBytes, setFileSizeBytes] = useState(2450000);
    const [totalPages, setTotalPages] = useState(1);
    const [fileMimeType, setFileMimeType] = useState("application/pdf");
    const [uploadError, setUploadError] = useState(null);
    const [isDragging, setIsDragging] = useState(false);
    const [libraryFilter, setLibraryFilter] = useState("all");
    const [creatingNotebook, setCreatingNotebook] = useState(false);
    const [generatingDocId, setGeneratingDocId] = useState(null);
    const [fetchedCourses, setFetchedCourses] = useState([]);
    const fileInputRef = React.useRef(null);
    const getAuthHeaders = () => {
        if (typeof window === "undefined")
            return {};
        const headers = {};
        const customUserId = window.localStorage.getItem("swms_active_user_id");
        if (customUserId)
            headers["X-Test-User-Id"] = customUserId;
        try {
            const rawAi = window.localStorage.getItem("swms_ai_config");
            if (rawAi) {
                const parsed = JSON.parse(rawAi);
                if (parsed.apiKey)
                    headers["X-Ai-Api-Key"] = parsed.apiKey;
                if (parsed.baseUrl)
                    headers["X-Ai-Base-Url"] = parsed.baseUrl;
                if (parsed.model)
                    headers["X-Ai-Model"] = parsed.model;
            }
        }
        catch { }
        return headers;
    };
    useEffect(() => {
        if (data.courses && data.courses.length > 0)
            return;
        let isMounted = true;
        fetch("/api/v1/courses", { headers: getAuthHeaders() })
            .then((r) => (r.ok ? r.json() : []))
            .then((list) => {
            if (isMounted && Array.isArray(list))
                setFetchedCourses(list);
        })
            .catch(() => { });
        return () => {
            isMounted = false;
        };
    }, [data.courses, data.documents.length]);
    const courses = propCourses && propCourses.length > 0
        ? propCourses
        : data.courses && data.courses.length > 0
            ? data.courses
            : fetchedCourses;
    const handleGenerateCourseFromDoc = async (docId) => {
        if (generatingDocId)
            return;
        setGeneratingDocId(docId);
        try {
            const resp = await fetch(`/api/v1/documents/${docId}/generate-course`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    ...getAuthHeaders()
                }
            });
            if (resp.ok) {
                const generated = await resp.json();
                const cid = generated.courseId ?? generated.id ?? null;
                if (cid)
                    setActiveCourseId?.(cid);
                if (generated.quizId)
                    setActiveQuizId?.(generated.quizId);
                onRefreshDashboard();
                const firstLesId = generated.firstLessonId ?? generated.chapters?.[0]?.lessons?.[0]?.id;
                if (firstLesId) {
                    setActiveLesson(firstLesId);
                }
                else {
                    setActiveDocument(docId);
                }
                setActiveMode("reading");
            }
        }
        catch { }
        finally {
            setGeneratingDocId(null);
        }
    };
    const handleCreateStandaloneNotebook = async () => {
        if (creatingNotebook)
            return;
        setCreatingNotebook(true);
        try {
            const resp = await fetch("/api/v1/notebooks", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    ...getAuthHeaders()
                },
                body: JSON.stringify({
                    title: `Ghi chú độc lập (${new Date().toLocaleDateString("vi-VN")})`
                })
            });
            if (resp.ok) {
                onRefreshDashboard();
                setLibraryFilter("notebook");
            }
            else {
                setActiveMode("split");
            }
        }
        catch {
            setActiveMode("split");
        }
        finally {
            setCreatingNotebook(false);
        }
    };
    const step = data.user.onboardingStep || 1;
    const isActivated = data.user.isActivated;
    const formatFileSize = (bytes) => {
        if (bytes < 1024)
            return `${bytes} B`;
        if (bytes < 1024 * 1024)
            return `${(bytes / 1024).toFixed(1)} KB`;
        return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
    };
    const handleProcessFile = async (file) => {
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
        const isMd = file.type === "text/markdown" ||
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
            }
            catch {
                setUploadError("Không thể đọc tệp văn bản. Vui lòng thử lại.");
            }
        }
        else if (isPdf) {
            setFileMimeType("application/pdf");
            // Multi-page PDF parsing and text extraction are handled server-side via PdfPig
            setTotalPages(1);
        }
        else {
            setFileMimeType(file.type || "application/octet-stream");
        }
    };
    const handleDrop = (e) => {
        e.preventDefault();
        setIsDragging(false);
        if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
            const file = e.dataTransfer.files[0];
            handleProcessFile(file);
        }
    };
    const handleDragOver = (e) => {
        e.preventDefault();
        setIsDragging(true);
    };
    const handleDragLeave = () => {
        setIsDragging(false);
    };
    const handleClearSelectedFile = () => {
        setSelectedFile(null);
        setUploadError(null);
        if (fileInputRef.current)
            fileInputRef.current.value = "";
    };
    const handleUploadDocument = async (e) => {
        e.preventDefault();
        if (!docTitle.trim())
            return;
        if (fileSizeBytes > 52428800) {
            setUploadError("Không thể tải lên: Dung lượng tài liệu vượt quá hạn mức lưu trữ 50MB của gói Free.");
            return;
        }
        setUploading(true);
        setUploadError(null);
        try {
            let resp;
            if (selectedFile) {
                const formData = new FormData();
                formData.append("file", selectedFile);
                formData.append("title", docTitle.trim() || selectedFile.name);
                resp = await fetch("/api/v1/documents/upload", {
                    method: "POST",
                    headers: getAuthHeaders(),
                    body: formData
                });
            }
            else {
                resp = await fetch("/api/v1/documents/upload", {
                    method: "POST",
                    headers: { "Content-Type": "application/json", ...getAuthHeaders() },
                    body: JSON.stringify({
                        title: docTitle.trim(),
                        fileSizeBytes: Math.max(1024, fileSizeBytes),
                        totalPages: Math.max(1, totalPages),
                        mimeType: fileMimeType,
                        extractedText: docText.trim()
                    })
                });
            }
            if (resp.ok) {
                const created = await resp.json();
                const genCourse = created.generatedCourse;
                const cid = genCourse?.courseId ?? genCourse?.id ?? created.courseId ?? null;
                if (cid)
                    setActiveCourseId?.(cid);
                if (genCourse?.quizId ?? created.quizId)
                    setActiveQuizId?.(genCourse?.quizId ?? created.quizId);
                setUploadModalOpen(false);
                setDocTitle("");
                setDocText("");
                setSelectedFile(null);
                setUploadError(null);
                onRefreshDashboard();
                setActiveDocument(created.id);
                setActiveMode("reading");
            }
            else {
                const err = await resp.json().catch(() => ({}));
                setUploadError(err.error ||
                    err.detail ||
                    (resp.status === 413
                        ? "Vượt quá hạn mức dung lượng lưu trữ 50 MB của gói Free."
                        : resp.status === 500
                            ? "Không thể kết nối hoặc máy chủ Backend gặp sự cố (Mã lỗi 500). Vui lòng kiểm tra máy chủ Backend đang chạy tại cổng 5080."
                            : `Tải tài liệu thất bại (Mã lỗi ${resp.status}).`));
            }
        }
        catch {
            setUploadError("Lỗi kết nối mạng khi tải tài liệu lên. Vui lòng kiểm tra lại đường truyền.");
        }
        finally {
            setUploading(false);
        }
    };
    const activeDoc = data.documents[0] || null;
    const activeLesson = data.lessons[0] || null;
    const activeTitle = activeDoc?.title || activeLesson?.title || "Chưa có bài học hoặc tài liệu";
    const activeTotalPages = activeDoc?.totalPages || 1;
    return (<div data-testid="personal-workspace-dashboard" className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 space-y-6">
      {/* Zone 0: Welcome Header & Action Bar */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-white dark:bg-slate-900 p-5 sm:p-6 rounded-2xl border border-slate-200/80 dark:border-slate-800 shadow-sm">
        <div>
          <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 text-xs font-semibold mb-2">
            <Sparkles className="w-3.5 h-3.5"/>
            <span>Bento Workspace • Tối ưu hóa việc học với FSRS Spaced Repetition</span>
          </div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-white">
            Chào mừng trở lại, {data.user.fullName}!
          </h1>
          <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-400 mt-1">
            Không gian học tập cá nhân: Đọc tài liệu, trích xuất ghi nhớ và ôn tập thông minh mỗi ngày.
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2 shrink-0">
          <button type="button" data-testid="open-upload-pdf-btn" onClick={() => setUploadModalOpen(true)} className="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs sm:text-sm font-semibold shadow-xs transition cursor-pointer min-h-[44px]">
            <Upload className="w-4 h-4"/>
            <span>Tải lên PDF / Markdown</span>
          </button>
          <button type="button" data-testid="open-split-notebook-btn" onClick={() => setActiveMode("split")} className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-800/50 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-300 text-xs font-semibold transition cursor-pointer min-h-[44px]">
            <PenTool className="w-3.5 h-3.5"/>
            <span>Ghi chú Split</span>
          </button>
          <button type="button" data-testid="open-practice-quiz-btn" onClick={() => {
            setActiveReviewSubTab("quiz");
            setActiveMode("review");
        }} className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-800/50 hover:bg-emerald-500/10 text-slate-600 dark:text-slate-300 hover:text-emerald-700 dark:hover:text-emerald-300 text-xs font-semibold transition cursor-pointer min-h-[44px]">
            <Target className="w-3.5 h-3.5 text-emerald-500"/>
            <span>Luyện Quiz</span>
          </button>
        </div>
      </div>

      {/* Ultra-sleek compact step status pill bar integrated above Bento Grid */}
      <section data-testid="onboarding-3step-widget" className="rounded-xl border border-slate-200/70 dark:border-slate-800/80 bg-white/90 dark:bg-slate-900/90 px-3.5 py-2 shadow-2xs flex flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap items-center gap-2.5">
          <span data-testid="onboarding-progress-badge" className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-indigo-600 text-white shrink-0">
            <Trophy className="w-3 h-3"/>
            <span>Chu kỳ FSRS (3/3)</span>
          </span>
          <div className="hidden sm:flex items-center gap-3 text-xs font-medium text-slate-600 dark:text-slate-400">
            <span data-testid="onboarding-step-1" className="inline-flex items-center gap-1 text-emerald-600 dark:text-emerald-400 font-semibold">
              <CheckCircle2 className="w-3.5 h-3.5"/> 1. Đọc tài liệu
            </span>
            <span data-testid="onboarding-step-2" className="inline-flex items-center gap-1 text-emerald-600 dark:text-emerald-400 font-semibold">
              <CheckCircle2 className="w-3.5 h-3.5"/> 2. Trích xuất Cloze
            </span>
            <span data-testid="onboarding-step-3" className="inline-flex items-center gap-1 text-emerald-600 dark:text-emerald-400 font-semibold">
              <CheckCircle2 className="w-3.5 h-3.5"/> 3. Ôn tập đều đặn
            </span>
          </div>
        </div>
        <button type="button" data-testid="onboarding-start-review-btn" onClick={() => {
            setSelectedTagFilterId(null);
            setActiveReviewSubTab("fsrs");
            setActiveMode("review");
        }} className="inline-flex items-center justify-center gap-1.5 px-3 py-1.5 rounded-lg bg-slate-100 hover:bg-slate-200 dark:bg-slate-800 dark:hover:bg-slate-700 text-slate-800 dark:text-slate-200 text-xs font-semibold cursor-pointer transition min-h-[38px]">
          <Zap className="w-3.5 h-3.5 text-indigo-500"/>
          <span>Ôn tập nhanh (~1 phút)</span>
        </button>
      </section>

      {/* Modern Bento Grid 4-Block Hierarchy */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-5 sm:gap-6">
        {/* Bento Block 1: Hero Continue Learning Card */}
        <div data-testid="hero-continue-learning-card" className="lg:col-span-7 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 p-6 flex flex-col justify-between shadow-sm relative overflow-hidden">
          <div>
            <div className="flex items-center justify-between pb-3 border-b border-slate-100 dark:border-slate-800">
              <div className="flex items-center gap-2">
                <span className="px-2.5 py-0.5 rounded-full bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300 text-xs font-semibold">
                  Tài liệu đang học
                </span>
                <span className="text-xs text-slate-500">
                  {activeDoc ? `PDF • ${activeTotalPages} trang` : "Bài học giáo trình"}
                </span>
              </div>
              <span className="text-xs text-slate-400">Tiếp tục phiên học</span>
            </div>
            <div className="mt-4">
              <h2 className="text-lg sm:text-xl font-bold text-slate-900 dark:text-white line-clamp-2">
                {activeTitle}
              </h2>
              <p className="text-xs sm:text-sm text-slate-500 dark:text-slate-400 mt-1.5">
                Tiếp tục từ trang gần nhất với thanh công cụ đánh dấu 5 màu và ghi chú tổng hợp song song.
              </p>
            </div>
            <div className="mt-5 space-y-2">
              <div className="flex items-center justify-between text-xs font-medium text-slate-600 dark:text-slate-400">
                <span>Tiến độ đọc: Trang 1 / {activeTotalPages}</span>
                <span>35% hoàn thành</span>
              </div>
              <div className="w-full bg-slate-100 dark:bg-slate-800 h-2.5 rounded-full overflow-hidden">
                <div className="bg-indigo-600 h-full rounded-full w-[35%] transition-all duration-300"/>
              </div>
            </div>
          </div>
          <div className="mt-6 pt-4 border-t border-slate-100 dark:border-slate-800 flex flex-wrap items-center gap-3">
            <button type="button" onClick={() => {
            if (activeDoc) {
                setActiveDocument(activeDoc.id);
            }
            else if (activeLesson) {
                setActiveLesson(activeLesson.id);
            }
            setActiveMode("reading");
        }} className="inline-flex items-center justify-center gap-2 px-5 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs sm:text-sm font-bold shadow-xs transition cursor-pointer min-h-[44px]">
              <BookOpen className="w-4 h-4"/>
              <span>Tiếp tục đọc ngay</span>
            </button>
            <button type="button" onClick={() => {
            if (activeDoc) {
                setActiveDocument(activeDoc.id);
            }
            else if (activeLesson) {
                setActiveLesson(activeLesson.id);
            }
            setActiveMode("split");
        }} className="inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-200 text-xs sm:text-sm font-semibold transition cursor-pointer min-h-[44px]">
              <PenTool className="w-4 h-4"/>
              <span>Mở Ghi chú PKM</span>
            </button>
          </div>
        </div>

        {/* Bento Block 2: FSRS Active Recall Station */}
        <div data-testid="review-station-card" className="lg:col-span-5 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 p-6 flex flex-col justify-between shadow-sm">
          <div data-testid="fsrs-review-station-card" className="flex flex-col justify-between h-full">
            <div>
              <div className="flex items-center justify-between pb-3 border-b border-slate-100 dark:border-slate-800">
                <h3 className="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
                  <Brain className="w-4 h-4 text-indigo-600 dark:text-indigo-400"/>
                  <span>Trạm ôn tập Spaced Repetition</span>
                </h3>
                <span className="px-2.5 py-0.5 rounded-md bg-emerald-500/10 text-emerald-700 dark:text-emerald-300 text-xs font-semibold">
                  FSRS v5
                </span>
              </div>

              <div className="mt-5 flex items-baseline gap-3">
                <span data-testid="dashboard-due-count" className="text-4xl sm:text-5xl font-black text-indigo-600 dark:text-indigo-400 tracking-tight">
                  {data.reviewStation.totalDue}
                </span>
                <span className="text-sm text-slate-600 dark:text-slate-400 font-medium">
                  thẻ cần ôn hôm nay • Tổng số: {data.reviewStation.totalCards} thẻ
                </span>
              </div>

              <div className="grid grid-cols-3 gap-2.5 mt-5">
                <div className="p-3 rounded-xl bg-sky-50 dark:bg-sky-950/40 border border-sky-200/60 dark:border-sky-900 text-center">
                  <div className="text-lg font-bold text-sky-700 dark:text-sky-300">
                    {data.reviewStation.newCount}
                  </div>
                  <div className="text-[11px] font-medium text-sky-600 dark:text-sky-400 mt-0.5">
                    Thẻ mới
                  </div>
                </div>
                <div className="p-3 rounded-xl bg-amber-50 dark:bg-amber-950/40 border border-amber-200/60 dark:border-amber-900 text-center">
                  <div className="text-lg font-bold text-amber-700 dark:text-amber-300">
                    {data.reviewStation.learningCount}
                  </div>
                  <div className="text-[11px] font-medium text-amber-600 dark:text-amber-400 mt-0.5">
                    Đang học
                  </div>
                </div>
                <div className="p-3 rounded-xl bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200/60 dark:border-emerald-900 text-center">
                  <div className="text-lg font-bold text-emerald-700 dark:text-emerald-300">
                    {data.reviewStation.reviewCount}
                  </div>
                  <div className="text-[11px] font-medium text-emerald-600 dark:text-emerald-400 mt-0.5">
                    Ôn định kỳ
                  </div>
                </div>
              </div>

              <p className="text-xs text-slate-500 dark:text-slate-400 mt-4 flex items-center gap-1.5">
                <Clock className="w-3.5 h-3.5 text-slate-400 shrink-0"/>
                <span>Thời gian ôn tập dự kiến: ~{data.reviewStation.estimatedMinutes || 12} phút</span>
              </p>
            </div>

            <button type="button" data-testid="launch-unified-review-btn" onClick={() => {
            setSelectedTagFilterId(null);
            setActiveReviewSubTab("fsrs");
            setActiveMode("review");
        }} className="mt-6 w-full py-3 px-4 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs sm:text-sm font-bold shadow-xs transition cursor-pointer flex items-center justify-center gap-2 min-h-[44px]">
              <Brain className="w-4 h-4"/>
              <span>Bắt đầu ôn tập ({data.reviewStation.totalDue} thẻ)</span>
            </button>
          </div>
        </div>

        {/* Bento Block 3: Quick Stats & Streak Tile */}
        <div data-testid="quick-stats-tile" className="lg:col-span-4 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 p-6 shadow-sm flex flex-col justify-between space-y-4">
          <div>
            <div className="flex items-center justify-between pb-3 border-b border-slate-100 dark:border-slate-800">
              <h3 className="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
                <Target className="w-4 h-4 text-amber-500"/>
                <span>Thống kê học tập</span>
              </h3>
              <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-amber-50 dark:bg-amber-950/40 border border-amber-200 dark:border-amber-900/60 text-amber-800 dark:text-amber-300 text-xs font-bold">
                <Flame className="w-3.5 h-3.5 text-amber-500 fill-amber-500"/>
                <span>🔥 {data.user.streakDays || 5} ngày liên tiếp</span>
              </div>
            </div>
            <div className="mt-4 space-y-3">
              <div className="p-3.5 rounded-xl bg-slate-50 dark:bg-slate-950/50 border border-slate-200/70 dark:border-slate-800">
                <div className="text-xs font-semibold text-slate-600 dark:text-slate-400">
                  Mục tiêu ghi nhớ dài hạn
                </div>
                <div className="text-lg font-bold text-slate-900 dark:text-white mt-0.5">
                  ≥ 90% khả năng lưu giữ
                </div>
                <p className="text-[11px] text-slate-500 mt-1">
                  Thuật toán FSRS v5 tự động điều chỉnh khoảng cách ôn tập theo tốc độ quên cá nhân.
                </p>
              </div>
              <div className="pt-2">
                <div className="text-xs font-semibold text-slate-600 dark:text-slate-400 mb-2 flex items-center gap-1.5">
                  <Tag className="w-3.5 h-3.5"/>
                  <span>Chủ đề ôn tập theo Tag</span>
                </div>
                <div className="flex flex-wrap gap-2">
                  {data.tags.map((t) => (<button key={t.id} type="button" data-testid={`dashboard-tag-${t.name}`} onClick={() => {
                setSelectedTagFilterId(t.id);
                setActiveReviewSubTab("fsrs");
                setActiveMode("review");
            }} className="px-3 py-1.5 rounded-full text-xs font-semibold bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-slate-700 dark:text-slate-200 transition cursor-pointer min-h-[36px]">
                      #{t.name} ({t.cardCount})
                    </button>))}
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Bento Block 4: Document & Resource Library Grid */}
        <div data-testid="document-library-grid" className="lg:col-span-8 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 p-6 shadow-sm flex flex-col justify-between">
          <div className="space-y-4">
            <div className="flex flex-wrap items-center justify-between gap-2 pb-3 border-b border-slate-100 dark:border-slate-800">
              <div className="flex items-center gap-2">
                <h3 className="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
                  <Layers className="w-4 h-4 text-indigo-600 dark:text-indigo-400"/>
                  <span>Thư viện tài liệu & học liệu</span>
                </h3>
                <span className="text-xs text-slate-500 dark:text-slate-400 font-medium">
                  ({data.documents.length + data.lessons.length + data.notebooks.length})
                </span>
              </div>

              <div className="flex flex-wrap items-center gap-1.5">
                <button type="button" data-testid="library-filter-all" onClick={() => setLibraryFilter("all")} className={`px-2.5 py-1 rounded-lg text-xs font-semibold cursor-pointer transition ${libraryFilter === "all" ? "bg-indigo-600 text-white" : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"}`}>
                  Tất cả
                </button>
                <button type="button" data-testid="library-filter-course" onClick={() => setLibraryFilter("course")} className={`px-2.5 py-1 rounded-lg text-xs font-semibold cursor-pointer transition ${libraryFilter === "course" ? "bg-indigo-600 text-white" : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"}`}>
                  Khóa học ({courses.length})
                </button>
                <button type="button" data-testid="library-filter-pdf" onClick={() => setLibraryFilter("pdf")} className={`px-2.5 py-1 rounded-lg text-xs font-semibold cursor-pointer transition ${libraryFilter === "pdf" ? "bg-indigo-600 text-white" : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"}`}>
                  PDF ({data.documents.length})
                </button>
                <button type="button" data-testid="library-filter-lesson" onClick={() => setLibraryFilter("lesson")} className={`px-2.5 py-1 rounded-lg text-xs font-semibold cursor-pointer transition ${libraryFilter === "lesson" ? "bg-indigo-600 text-white" : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"}`}>
                  Bài học ({data.lessons.length})
                </button>
                <button type="button" data-testid="library-filter-notebook" onClick={() => setLibraryFilter("notebook")} className={`px-2.5 py-1 rounded-lg text-xs font-semibold cursor-pointer transition ${libraryFilter === "notebook" ? "bg-indigo-600 text-white" : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700"}`}>
                  Ghi chú ({data.notebooks.length})
                </button>
                <button type="button" data-testid="create-standalone-notebook-btn" disabled={creatingNotebook} onClick={handleCreateStandaloneNotebook} className="px-2.5 py-1 rounded-lg border border-indigo-500/40 bg-indigo-50 dark:bg-indigo-950/60 hover:bg-indigo-600 hover:text-white text-indigo-700 dark:text-indigo-300 text-xs font-bold cursor-pointer transition">
                  {creatingNotebook ? "Đang tạo..." : "+ Tạo ghi chú mới"}
                </button>
              </div>
            </div>

            <div className="divide-y divide-slate-100 dark:divide-slate-800">
              {(libraryFilter === "all" || libraryFilter === "course") && courses.map((course) => {
                const cid = course.courseId || course.id || "";
                const chapterCount = course.chapterCount ?? course.chapters?.length ?? 0;
                const lessonCount = course.lessonCount ?? course.chapters?.reduce((acc, ch) => acc + (ch.lessons?.length ?? 0), 0) ?? 0;
                const flashcardCount = course.flashcardCount ?? 0;
                const firstLessonId = course.firstLessonId ?? course.chapters?.[0]?.lessons?.[0]?.id ?? null;
                return (<div key={`course-${cid || course.title}`} data-testid="course-card-item" className="py-4 px-3 my-2 rounded-2xl border border-indigo-500/30 bg-indigo-50/40 dark:bg-indigo-950/30 space-y-3">
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div className="min-w-0">
                        <div className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full bg-indigo-600 text-white text-[10px] font-bold mb-1">
                          <Sparkles className="w-3 h-3"/>
                          <span>{chapterCount} chương • {lessonCount} bài giảng • {flashcardCount} thẻ FSRS</span>
                        </div>
                        <h4 className="text-sm sm:text-base font-extrabold text-slate-900 dark:text-white">{course.title}</h4>
                        {course.description && (<p className="text-xs text-slate-600 dark:text-slate-400 mt-0.5 line-clamp-2">{course.description}</p>)}
                      </div>
                      <div className="flex flex-wrap items-center gap-1.5">
                        <button type="button" data-testid="course-study-lessons-btn" onClick={() => {
                        if (cid)
                            setActiveCourseId?.(cid);
                        if (firstLessonId) {
                            setActiveLesson(firstLessonId);
                        }
                        else if (course.sourceDocumentId) {
                            setActiveDocument(course.sourceDocumentId);
                        }
                        setActiveMode("reading");
                    }} className="px-2.5 py-1.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold cursor-pointer transition">
                          📖 Học Lộ trình Bài giảng
                        </button>
                        {course.sourceDocumentId && (<button type="button" data-testid="course-open-pdf-btn" onClick={() => {
                            if (cid)
                                setActiveCourseId?.(cid);
                            setActiveDocument(course.sourceDocumentId);
                            setActiveMode("reading");
                        }} className="px-2.5 py-1.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-200 text-xs font-bold cursor-pointer transition">
                            📄 Đọc PDF gốc
                          </button>)}
                        <button type="button" data-testid="course-review-flashcards-btn" onClick={() => {
                        if (cid)
                            setActiveCourseId?.(cid);
                        setSelectedTagFilterId(course.tagId ?? null);
                        setActiveReviewSubTab("fsrs");
                        setActiveMode("review");
                    }} className="px-2.5 py-1.5 rounded-xl border border-indigo-500/40 bg-indigo-50 dark:bg-indigo-950/70 hover:bg-indigo-600 hover:text-white text-indigo-700 dark:text-indigo-300 text-xs font-bold cursor-pointer transition">
                          🧠 Ôn Flashcard khóa này
                        </button>
                        <button type="button" data-testid="course-take-quiz-btn" onClick={() => {
                        if (cid)
                            setActiveCourseId?.(cid);
                        if (course.quizId)
                            setActiveQuizId?.(course.quizId);
                        setActiveReviewSubTab("quiz");
                        setActiveMode("review");
                    }} className="px-2.5 py-1.5 rounded-xl border border-emerald-500/40 bg-emerald-50 dark:bg-emerald-950/70 hover:bg-emerald-600 hover:text-white text-emerald-700 dark:text-emerald-300 text-xs font-bold cursor-pointer transition">
                          🎯 Làm Quiz khóa này
                        </button>
                      </div>
                    </div>
                    {course.chapters && course.chapters.length > 0 && (<div className="pt-2 border-t border-indigo-200/60 dark:border-indigo-900/50 space-y-2">
                        {course.chapters.map((ch) => (<div key={ch.id} className="space-y-1">
                            <div className="text-[11px] font-bold text-indigo-800 dark:text-indigo-300">{ch.title}</div>
                            <div className="flex flex-wrap gap-1.5">
                              {(ch.lessons || []).map((les) => (<button key={les.id} type="button" onClick={() => {
                                    if (cid)
                                        setActiveCourseId?.(cid);
                                    setActiveLesson(les.id);
                                    setActiveMode("reading");
                                }} className="px-2.5 py-1 rounded-lg bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 hover:border-indigo-500 text-slate-700 dark:text-slate-200 text-[11px] font-semibold cursor-pointer transition inline-flex items-center gap-1">
                                  <BookOpen className="w-3 h-3 text-indigo-600 shrink-0"/>
                                  <span className="truncate max-w-[240px]">{les.title}</span>
                                </button>))}
                            </div>
                          </div>))}
                      </div>)}
                  </div>);
            })}

              {(libraryFilter === "all" || libraryFilter === "pdf") && data.documents.map((doc) => (<div key={doc.id} className="py-3 flex items-center justify-between gap-3 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 px-2 rounded-lg transition">
                  <div className="min-w-0">
                    <div className="text-xs sm:text-sm font-semibold text-slate-900 dark:text-slate-100 truncate flex items-center gap-1.5">
                      <FileText className="w-3.5 h-3.5 text-emerald-500 shrink-0"/>
                      <span>{doc.title}</span>
                    </div>
                    <div className="text-[11px] text-slate-500 mt-0.5">
                      {doc.totalPages} trang • {formatFileSize(doc.fileSizeBytes)} •{" "}
                      <span className="text-emerald-600 dark:text-emerald-400 font-semibold">
                        {doc.ocrStatus === "completed" ? "Đã trích xuất" : doc.ocrStatus}
                      </span>
                    </div>
                  </div>
                  <div className="flex flex-wrap items-center gap-1.5 shrink-0">
                    <button type="button" data-testid="generate-course-from-doc-btn" disabled={generatingDocId === doc.id} onClick={() => handleGenerateCourseFromDoc(doc.id)} className="px-3 py-1.5 rounded-xl border border-indigo-500/40 bg-indigo-50 dark:bg-indigo-950/60 hover:bg-indigo-600 hover:text-white text-indigo-700 dark:text-indigo-300 text-xs font-bold transition cursor-pointer min-h-[40px]">
                      {generatingDocId === doc.id ? "Đang sinh..." : "✨ Sinh Khóa học"}
                    </button>
                    <button type="button" onClick={() => {
                setActiveDocument(doc.id);
                setActiveMode("reading");
            }} className="px-3 py-1.5 rounded-xl bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer min-h-[40px]">
                      Mở đọc
                    </button>
                    <button type="button" onClick={() => {
                setActiveDocument(doc.id);
                setActiveMode("split");
            }} className="px-3 py-1.5 rounded-xl bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer min-h-[40px]">
                      Split PKM
                    </button>
                  </div>
                </div>))}

              {(libraryFilter === "all" || libraryFilter === "lesson") && data.lessons.map((les) => (<div key={les.id} className="py-3 flex items-center justify-between gap-3 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 px-2 rounded-lg transition">
                  <div className="min-w-0">
                    <div className="text-xs sm:text-sm font-semibold text-slate-900 dark:text-slate-100 truncate flex items-center gap-1.5">
                      <BookOpen className="w-3.5 h-3.5 text-indigo-500 shrink-0"/>
                      <span>{les.title}</span>
                    </div>
                    <div className="text-[11px] text-slate-500 mt-0.5">
                      Bài học • Phiên bản v{les.currentVersion}
                    </div>
                  </div>
                  <div className="flex items-center gap-1.5 shrink-0">
                    <button type="button" onClick={() => {
                setActiveLesson(les.id);
                setActiveMode("reading");
            }} className="px-3 py-1.5 rounded-xl bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer min-h-[40px]">
                      Đọc sâu
                    </button>
                    <button type="button" onClick={() => {
                setActiveLesson(les.id);
                setActiveMode("split");
            }} className="px-3 py-1.5 rounded-xl bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer min-h-[40px]">
                      Split PKM
                    </button>
                  </div>
                </div>))}

              {(libraryFilter === "all" || libraryFilter === "notebook") && (libraryFilter === "notebook" ? data.notebooks : data.notebooks.slice(0, 3)).map((nb) => (<div key={nb.id} className="py-3 flex items-center justify-between gap-3 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 px-2 rounded-lg transition">
                  <div className="min-w-0">
                    <div className="text-xs sm:text-sm font-semibold text-slate-900 dark:text-slate-100 truncate flex items-center gap-1.5">
                      <PenTool className="w-3.5 h-3.5 text-amber-500 shrink-0"/>
                      <span>[Ghi chú] {nb.title}</span>
                    </div>
                    <div className="text-[11px] text-slate-500 mt-0.5">
                      Bản ghi chú tổng hợp • v{nb.version}
                    </div>
                  </div>
                  <button type="button" onClick={() => {
                if (nb.primarySourceLessonId) {
                    setActiveLesson(nb.primarySourceLessonId);
                }
                else if (nb.primarySourceDocumentId) {
                    setActiveDocument(nb.primarySourceDocumentId);
                }
                setActiveMode("split");
            }} className="px-3 py-1.5 rounded-xl bg-slate-100 dark:bg-slate-800 hover:bg-indigo-600 hover:text-white text-xs font-semibold transition cursor-pointer shrink-0 min-h-[40px]">
                    Mở ghi chú
                  </button>
                </div>))}
            </div>
          </div>
        </div>
      </div>

      {/* Upload PDF / Markdown Modal */}
      {uploadModalOpen && (<div data-testid="upload-document-modal" className="fixed inset-0 z-50 bg-slate-950/60 backdrop-blur-xs flex items-center justify-center p-4">
          <form onSubmit={handleUploadDocument} className="w-full max-w-lg rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 shadow-xl space-y-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-3">
              <div>
                <h3 className="text-base font-bold text-slate-900 dark:text-white flex items-center gap-2">
                  <Upload className="w-4 h-4 text-indigo-600"/>
                  <span>Tải lên tài liệu PDF / Markdown vào Workspace</span>
                </h3>
                <p className="text-[11px] text-indigo-600 dark:text-indigo-400 font-semibold mt-0.5">
                  Tải tài liệu &amp; Tự động Sinh Khóa học AI (Chương mục, Bài giảng, Flashcard &amp; Quiz)
                </p>
              </div>
              <button type="button" onClick={() => {
                setUploadModalOpen(false);
                setUploadError(null);
            }} className="text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 p-1 rounded cursor-pointer">
                <X className="w-4 h-4"/>
              </button>
            </div>

            {/* Error Notification Banner (Fixes FUNC-02) */}
            {uploadError && (<div data-testid="upload-error-alert" className="p-3 rounded-xl bg-rose-50 dark:bg-rose-950/50 border border-rose-200 dark:border-rose-900/60 text-rose-700 dark:text-rose-300 text-xs font-semibold flex items-start gap-2 animate-in fade-in">
                <AlertCircle className="w-4 h-4 shrink-0 mt-0.5 text-rose-600 dark:text-rose-400"/>
                <div className="flex-1">{uploadError}</div>
              </div>)}

            {/* Hidden native file input */}
            <input type="file" ref={fileInputRef} data-testid="document-file-input" accept=".pdf,.md,.markdown,.txt,application/pdf,text/markdown,text/plain" onChange={(e) => {
                if (e.target.files && e.target.files[0]) {
                    handleProcessFile(e.target.files[0]);
                }
            }} className="hidden"/>

            {/* Dropzone & File Selector Area */}
            {!selectedFile ? (<div data-testid="document-dropzone" onDragOver={handleDragOver} onDragLeave={handleDragLeave} onDrop={handleDrop} onClick={() => fileInputRef.current?.click()} className={`p-6 rounded-2xl border-2 border-dashed text-center transition cursor-pointer flex flex-col items-center justify-center gap-2.5 ${isDragging
                    ? "border-indigo-600 bg-indigo-50/60 dark:bg-indigo-950/40"
                    : "border-slate-300 dark:border-slate-700 hover:border-indigo-500 dark:hover:border-indigo-500 bg-slate-50/50 dark:bg-slate-950/40"}`}>
                <div className="w-10 h-10 rounded-full bg-indigo-50 dark:bg-indigo-950/80 text-indigo-600 dark:text-indigo-400 flex items-center justify-center">
                  <FileUp className="w-5 h-5"/>
                </div>
                <div>
                  <p className="text-xs font-bold text-slate-800 dark:text-slate-200">
                    Kéo thả file PDF hoặc Markdown vào đây
                  </p>
                  <p className="text-[11px] text-slate-500 dark:text-slate-400 mt-0.5">
                    hoặc nhấp để chọn tệp từ máy tính của bạn
                  </p>
                </div>
                <button type="button" data-testid="browse-computer-file-btn" onClick={(e) => {
                    e.stopPropagation();
                    fileInputRef.current?.click();
                }} className="px-3.5 py-1.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 text-xs font-semibold text-slate-700 dark:text-slate-200 transition shadow-2xs">
                  Chọn file từ máy tính
                </button>
                <span className="text-[10px] text-slate-400">
                  Hỗ trợ .pdf, .md, .markdown, .txt • Tối đa 50MB (Gói Free)
                </span>
              </div>) : (<div data-testid="selected-file-card" className="p-3.5 rounded-xl bg-slate-50 dark:bg-slate-800/60 border border-slate-200 dark:border-slate-700 flex items-center justify-between gap-3">
                <div className="flex items-center gap-2.5 min-w-0">
                  <span className={`px-2 py-1 rounded-md text-[10px] font-extrabold uppercase shrink-0 ${fileMimeType === "application/pdf"
                    ? "bg-rose-100 dark:bg-rose-950/70 text-rose-700 dark:text-rose-300 border border-rose-200 dark:border-rose-800"
                    : "bg-emerald-100 dark:bg-emerald-950/70 text-emerald-700 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800"}`}>
                    {fileMimeType === "application/pdf" ? "PDF" : "MD"}
                  </span>
                  <div className="min-w-0">
                    <p className="text-xs font-bold text-slate-900 dark:text-slate-100 truncate">
                      {selectedFile.name}
                    </p>
                    <p className="text-[11px] text-slate-500">
                      {formatFileSize(fileSizeBytes)} • {fileMimeType === "application/pdf" ? "Phân tích tự động trên máy chủ" : `${totalPages} trang`}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2 shrink-0">
                  <button type="button" onClick={() => fileInputRef.current?.click()} className="text-xs font-semibold text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer">
                    Đổi file
                  </button>
                  <button type="button" onClick={handleClearSelectedFile} title="Gỡ file" className="p-1 rounded text-slate-400 hover:text-rose-600 dark:hover:text-rose-400 cursor-pointer">
                    <Trash2 className="w-4 h-4"/>
                  </button>
                </div>
              </div>)}

            <div>
              <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                Tiêu đề tài liệu / giáo trình:
              </label>
              <input type="text" required value={docTitle} onChange={(e) => setDocTitle(e.target.value)} placeholder="VD: Modern Operating Systems - Andrew S. Tanenbaum.pdf" className="w-full px-3 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-transparent text-sm focus:ring-2 focus:ring-indigo-500 outline-none"/>
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                Nội dung văn bản trích xuất (Text-layer / Markdown):
              </label>
              <textarea rows={4} value={docText} onChange={(e) => setDocText(e.target.value)} placeholder="Dán nội dung chương sách hoặc để trống để tạo văn bản mẫu..." className="w-full px-3 py-2 rounded-xl border border-slate-300 dark:border-slate-700 bg-transparent text-xs focus:ring-2 focus:ring-indigo-500 outline-none"/>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => {
                setUploadModalOpen(false);
                setUploadError(null);
            }} className="px-4 py-2 rounded-xl border border-slate-300 dark:border-slate-700 text-xs font-semibold cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-800 transition">
                Hủy
              </button>
              <button type="submit" disabled={uploading} className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold cursor-pointer transition shadow-xs flex items-center gap-1.5">
                {uploading ? (<>
                    <Upload className="w-3.5 h-3.5 animate-bounce"/>
                    <span>Đang xử lý...</span>
                  </>) : (<span>Nhập vào Workspace</span>)}
              </button>
            </div>
          </form>
        </div>)}
    </div>);
}
