"use client";

import React, { useState, useEffect, useRef } from "react";
import Link from "next/link";
import {
  BookOpen,
  Search,
  Bot,
  HardDrive,
  Flame,
  Sun,
  Moon,
  Keyboard,
  Columns2,
  Brain,
  LayoutDashboard,
  Sparkles,
  X,
  ChevronRight,
  LogOut,
  Users,
  GraduationCap
} from "lucide-react";
import { getClientRequestHeaders, useWorkspaceStore, type WorkspaceMode } from "@/stores/useWorkspaceStore";
import { setAccessToken } from "@/lib/api-client";

export interface QuotaSummary {
  tier: string;
  aiCreditsUsed: number;
  aiCreditsLimit: number;
  aiTokensUsed: number;
  aiTokensLimit: number;
  storageBytesUsed: number;
  storageBytesLimit: number;
  storageMbUsed: number;
  storageMbLimit: number;
  documentCount: number;
  instantClozeCreatedFree: number;
  breakdown: {
    qaGenerationCredits: number;
    explainConceptCredits: number;
    essayFeedbackCredits: number;
  };
}

interface TopNavHeaderProps {
  userName: string;
  userEmail?: string;
  streakDays: number;
  breadcrumbRoot?: string;
  activeTitle: string;
  dueCount: number;
  quota: QuotaSummary | null;
  onOpenShortcuts?: () => void;
}

export function TopNavHeader({
  userName,
  userEmail = "son.tran@studxy.vn",
  streakDays,
  breadcrumbRoot = "Khóa học",
  activeTitle,
  dueCount,
  quota,
  onOpenShortcuts
}: TopNavHeaderProps) {
  const {
    activeMode,
    setActiveMode,
    darkMode,
    toggleDarkMode,
    setCommandPaletteOpen,
    setActiveUserHeaderId,
    aiConfigModalOpen,
    setAiConfigModalOpen
  } = useWorkspaceStore();

  const [aiApiKey, setAiApiKey] = useState("");
  const [aiBaseUrl, setAiBaseUrl] = useState(
    "https://generativelanguage.googleapis.com/v1beta/openai"
  );
  const [aiModel, setAiModel] = useState("gemini-2.5-flash");
  const [aiSavedNotice, setAiSavedNotice] = useState(false);

  useEffect(() => {
    if (!aiConfigModalOpen || typeof window === "undefined") return;
    try {
      const raw = window.localStorage.getItem("swms_ai_config");
      if (raw) {
        const parsed = JSON.parse(raw);
        if (typeof parsed.apiKey === "string") setAiApiKey(parsed.apiKey);
        if (typeof parsed.baseUrl === "string" && parsed.baseUrl)
          setAiBaseUrl(parsed.baseUrl);
        if (typeof parsed.model === "string" && parsed.model)
          setAiModel(parsed.model);
      }
    } catch {
      // Ignore storage parse error
    }
  }, [aiConfigModalOpen]);

  const userInitials =
    userName
      .trim()
      .split(/\s+/)
      .map((part) => part[0]?.toUpperCase() ?? "")
      .join("")
      .slice(0, 2) || "ST";

  const [quotaOpen, setQuotaOpen] = useState(false);
  const [avatarMenuOpen, setAvatarMenuOpen] = useState(false);
  const quotaRef = useRef<HTMLDivElement | null>(null);
  const avatarRef = useRef<HTMLDivElement | null>(null);

  // Outside-click & Escape dismissal for Quota Popover & Avatar Menu (Fixes UX-02 & UX-05)
  useEffect(() => {
    if (!quotaOpen && !avatarMenuOpen) return;
    const handleOutsideClick = (e: MouseEvent) => {
      const target = e.target as Node;
      if (quotaOpen && quotaRef.current && !quotaRef.current.contains(target)) {
        setQuotaOpen(false);
      }
      if (avatarMenuOpen && avatarRef.current && !avatarRef.current.contains(target)) {
        setAvatarMenuOpen(false);
      }
    };
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        setQuotaOpen(false);
        setAvatarMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", handleOutsideClick);
    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("mousedown", handleOutsideClick);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [quotaOpen, avatarMenuOpen]);

  // ThemeSynchronizer is the sole source of truth for HTML class & localStorage synchronization.

  const creditsUsed = quota?.aiCreditsUsed ?? 14;
  const creditsLimit = quota?.aiCreditsLimit ?? 50;
  const storageMbUsed = quota?.storageMbUsed ?? 18.4;
  const storageMbLimit = quota?.storageMbLimit ?? 50.0;
  const creditRatio = Math.min(100, Math.round((creditsUsed / Math.max(1, creditsLimit)) * 100));
  const storageRatio = Math.min(100, Math.round((storageMbUsed / Math.max(1, storageMbLimit)) * 100));

  const quotaColorClass =
    creditRatio >= 90
      ? "border-rose-500/60 bg-rose-500/10 text-rose-700 dark:text-rose-300"
      : creditRatio >= 70
      ? "border-amber-500/60 bg-amber-500/10 text-amber-700 dark:text-amber-300"
      : "border-emerald-500/50 bg-emerald-500/10 text-emerald-800 dark:text-emerald-300";

  const modes: {
    id: WorkspaceMode;
    label: string;
    shortLabel: string;
    fullTitle: string;
    icon: React.ComponentType<{ className?: string }>;
    hotkey: string;
    badge?: number;
  }[] = [
    { id: "dashboard", label: "Tổng quan", shortLabel: "Tổng quan", fullTitle: "Không gian cá nhân", icon: LayoutDashboard, hotkey: "0" },
    { id: "reading", label: "Đọc sâu", shortLabel: "Đọc", fullTitle: "Mode 1: Đọc sâu", icon: BookOpen, hotkey: "1" },
    { id: "split", label: "Split PKM", shortLabel: "Split", fullTitle: "Mode 2: Chia đôi PKM", icon: Columns2, hotkey: "2" },
    { id: "review", label: "Ôn tập FSRS", shortLabel: "Ôn tập", fullTitle: "Mode 3: Ôn tập FSRS", icon: Brain, hotkey: "3", badge: dueCount },
    { id: "studio", label: "Studio", shortLabel: "Studio", fullTitle: "Mode 4: Studio Khóa học", icon: GraduationCap, hotkey: "4" }
  ];

  return (
    <header
      data-testid="global-top-header"
      className="sticky top-0 z-50 h-14 w-full border-b border-slate-200/80 dark:border-slate-800 bg-white/95 dark:bg-slate-950/95 backdrop-blur-md px-1.5 sm:px-2.5 lg:px-3 xl:px-5 flex items-center justify-between gap-1 sm:gap-1.5 lg:gap-2"
    >
      {/* Left: Brand & Breadcrumb (never shrinks below logo on mobile/tablet) */}
      <div className="flex items-center gap-1.5 lg:gap-2 shrink-0 lg:shrink lg:flex-1 min-w-0 relative z-10">
        <Link
          href="/"
          data-testid="brand-logo-link"
          className="relative z-10 flex items-center gap-2 font-bold tracking-tight text-[#5E51E8] dark:text-[#818CF8] shrink-0 focus:outline-none focus:ring-2 focus:ring-[#5E51E8] rounded-md px-1 sm:px-1.5 py-1 cursor-pointer group"
          title="Về Trang chủ Studxy"
        >
          <span className="inline-flex items-center justify-center w-7 h-7 rounded-[8px] bg-[#5E51E8] dark:bg-[#818CF8] text-white dark:text-[#0F0F12] text-xs font-extrabold shadow-sm group-hover:scale-105 transition-transform">
            St
          </span>
          <span className="text-sm font-extrabold hidden lg:inline">Studxy</span>
        </Link>

        {/* Nút quay về /app rõ ràng (MOB-05) */}
        <Link
          href="/app"
          data-testid="header-back-to-app-btn"
          className="inline-flex items-center gap-1 px-2 py-1 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] text-[#4F46E5] dark:text-[#A5B4FC] border border-[#5E51E8]/20 text-xs font-semibold hover:opacity-90 transition shrink-0"
          title="Chuyển sang không gian học tập mới (/app)"
        >
          <BookOpen className="w-3.5 h-3.5 shrink-0" />
          <span className="text-[11px] font-semibold">Vào App</span>
        </Link>

        <span className="text-[var(--border-calm)] hidden lg:inline shrink-0">/</span>

        <div className="hidden lg:flex items-center gap-1.5 text-xs text-[var(--text-secondary)] min-w-0 flex-1 overflow-hidden">
          <button
            type="button"
            data-testid="header-breadcrumb-root"
            onClick={() => setActiveMode("dashboard")}
            className="font-medium text-[var(--text-secondary)] hover:text-[#5E51E8] dark:hover:text-[#818CF8] truncate max-w-[110px] xl:max-w-[140px] 2xl:max-w-[180px] shrink cursor-pointer"
            title={`Về Không gian cá nhân (${breadcrumbRoot})`}
          >
            {breadcrumbRoot}
          </button>
          <ChevronRight className="w-3 h-3 text-[var(--text-tertiary)] shrink-0" />
          <span
            data-testid="header-breadcrumb-current"
            className="font-semibold text-[var(--text-primary)] truncate min-w-0 flex-1"
            title={activeTitle}
          >
            {activeTitle}
          </span>
        </div>
      </div>

      {/* Center: Mode Switcher + Command Palette */}
      <div className="flex items-center gap-1 lg:gap-1.5 shrink-0">
        <nav
          aria-label="Chế độ không gian học tập"
          className="flex items-center bg-slate-100 dark:bg-slate-900 p-0.5 sm:p-1 rounded-xl border border-slate-200/70 dark:border-slate-800"
        >
          {modes.map((m) => {
            const isActive = activeMode === m.id;
            const Icon = m.icon;
            return (
              <button
                key={m.id}
                type="button"
                data-testid={`mode-tab-${m.id}`}
                onClick={() => setActiveMode(m.id)}
                className={`min-w-[36px] min-h-[38px] px-1.5 lg:px-2 xl:px-3 py-1.5 rounded-lg text-xs font-semibold transition-all flex items-center justify-center gap-1 xl:gap-1.5 cursor-pointer whitespace-nowrap focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 ${
                  isActive
                    ? "bg-indigo-600 text-white shadow-sm"
                    : "text-slate-700 dark:text-slate-300 hover:bg-slate-200/70 dark:hover:bg-slate-800"
                }`}
                title={`Chuyển sang ${m.fullTitle} (Phím ${m.hotkey})`}
              >
                <Icon className="w-3.5 h-3.5 shrink-0" />
                <span className="hidden xl:inline">{m.label}</span>
                <span className="hidden sm:inline xl:hidden">{m.shortLabel}</span>
                {typeof m.badge === "number" && m.badge > 0 && (
                  <span
                    className={`px-1 sm:px-1.5 py-0.2 rounded-full text-[10px] font-bold ${
                      isActive
                        ? "bg-white text-indigo-700"
                        : "bg-indigo-100 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300"
                    }`}
                  >
                    {m.badge}
                  </span>
                )}
              </button>
            );
          })}
        </nav>

        <button
          type="button"
          data-testid="cmd-k-trigger"
          onClick={() => setCommandPaletteOpen(true)}
          className="min-w-[36px] min-h-[38px] flex items-center justify-center gap-1.5 px-2 xl:px-3 py-1.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-900 hover:bg-slate-100 dark:hover:bg-slate-800 text-xs text-slate-700 dark:text-slate-300 transition cursor-pointer shrink-0 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500"
          title="Mở bảng lệnh & tìm kiếm nhanh (Ctrl+K / Cmd+K)"
        >
          <Search className="w-3.5 h-3.5 text-slate-400" />
          <span className="hidden xl:inline font-medium">Tìm nhanh</span>
          <kbd className="hidden xl:inline px-1.5 py-0.5 text-[10px] font-mono bg-white dark:bg-slate-800 rounded border border-slate-300 dark:border-slate-700 text-slate-600 dark:text-slate-300">
            Ctrl+K
          </kbd>
        </button>
      </div>

      {/* Right: AI & Storage Quota Indicator + Streak + Theme + Avatar */}
      <div className="flex items-center gap-1 lg:gap-1.5 shrink-0 relative">
        <div ref={quotaRef} className="relative">
          <button
            type="button"
            data-testid="quota-indicator-btn"
            onClick={() => setQuotaOpen((prev) => !prev)}
            className={`min-w-[36px] min-h-[38px] px-1.5 lg:px-2.5 xl:px-3 py-1.5 rounded-xl border text-xs font-semibold flex items-center gap-1 lg:gap-1.5 cursor-pointer whitespace-nowrap transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 ${quotaColorClass}`}
          >
            <Sparkles className="w-3.5 h-3.5 shrink-0" />
            <span className="hidden lg:inline">AI: {creditsUsed}/{creditsLimit}</span>
            <span className="lg:hidden">AI:{creditsUsed}</span>
            <span className="hidden 2xl:inline text-slate-400">|</span>
            <HardDrive className="w-3.5 h-3.5 hidden 2xl:inline shrink-0" />
            <span className="hidden 2xl:inline">{storageMbUsed}/{storageMbLimit} MB</span>
          </button>

          {quotaOpen && (
            <div
              data-testid="quota-breakdown-popover"
              className="absolute right-0 mt-2 w-80 sm:w-96 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-lg p-4 z-50 text-xs text-slate-700 dark:text-slate-200"
            >
              <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-2.5 mb-3">
                <span className="font-bold text-slate-900 dark:text-white">
                  Hạn mức tài nguyên (Gói Miễn phí)
                </span>
                <button
                  type="button"
                  onClick={() => setQuotaOpen(false)}
                  className="text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 p-0.5 rounded cursor-pointer"
                  title="Đóng"
                >
                  <X className="w-4 h-4" />
                </button>
              </div>

              <div className="space-y-3">
                <div>
                  <div className="flex justify-between font-semibold mb-1">
                    <span className="flex items-center gap-1">
                      <Sparkles className="w-3.5 h-3.5 text-indigo-500" />
                      <span>Lượt dùng AI hôm nay</span>
                    </span>
                    <span>
                      {creditsUsed} / {creditsLimit} lượt (Còn {Math.max(0, creditsLimit - creditsUsed)})
                    </span>
                  </div>
                  <div className="w-full h-2.5 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-indigo-600 rounded-full transition-all"
                      style={{ width: `${creditRatio}%` }}
                    />
                  </div>
                  <ul className="mt-2 space-y-1 text-[11px] text-slate-600 dark:text-slate-400">
                    <li className="flex justify-between">
                      <span>• Sinh thẻ Q&A bằng AI:</span>
                      <span className="font-semibold">{quota?.breakdown.qaGenerationCredits ?? 8} credits</span>
                    </li>
                    <li className="flex justify-between">
                      <span>• Giải thích thuật ngữ bằng AI:</span>
                      <span className="font-semibold">{quota?.breakdown.explainConceptCredits ?? 4} credits</span>
                    </li>
                    <li className="flex justify-between">
                      <span>• Chấm điểm bài luận Practice:</span>
                      <span className="font-semibold">{quota?.breakdown.essayFeedbackCredits ?? 2} credits</span>
                    </li>
                    <li className="flex justify-between text-emerald-600 dark:text-emerald-400 font-medium">
                      <span>• Tạo thẻ Cloze tức thì ({quota?.instantClozeCreatedFree ?? 2} thẻ):</span>
                      <span>Miễn phí vĩnh viễn (0 credit)</span>
                    </li>
                  </ul>
                </div>

                <div className="border-t border-slate-100 dark:border-slate-800 pt-2.5">
                  <div className="flex justify-between font-semibold mb-1">
                    <span className="flex items-center gap-1">
                      <HardDrive className="w-3.5 h-3.5 text-emerald-500" />
                      <span>Lưu trữ tài liệu học tập (PDF)</span>
                    </span>
                    <span>
                      {storageMbUsed} / {storageMbLimit} MB
                    </span>
                  </div>
                  <div className="w-full h-2 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-emerald-500 rounded-full transition-all"
                      style={{ width: `${storageRatio}%` }}
                    />
                  </div>
                  <p className="mt-1.5 text-[11px] text-slate-500">
                    Đang lưu trữ {quota?.documentCount ?? 1} tài liệu PDF học thuật · Tự động làm mới hạn mức AI lúc 00:00 UTC.
                  </p>
                </div>
              </div>
            </div>
          )}
        </div>

        <div
          data-testid="streak-badge"
          className="hidden lg:flex items-center gap-1.5 px-2.5 py-1 rounded-xl bg-amber-500/15 border border-amber-500/40 text-amber-800 dark:text-amber-300 text-xs font-bold whitespace-nowrap"
          title="Chuỗi ngày ôn tập FSRS liên tục"
        >
          <Flame className="w-3.5 h-3.5 text-amber-500" />
          <span>{streakDays}d</span>
        </div>

        {onOpenShortcuts && (
          <button
            type="button"
            data-testid="keyboard-shortcuts-trigger"
            onClick={onOpenShortcuts}
            aria-label="Phím tắt và trợ giúp"
            title="Xem hướng dẫn phím tắt (?)"
            className="hidden sm:flex w-9 h-9 xl:w-10 xl:h-10 items-center justify-center p-2 rounded-xl border border-slate-200 dark:border-slate-800 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300 text-xs cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 transition-colors"
          >
            <Keyboard className="w-4 h-4" />
          </button>
        )}

        <button
          type="button"
          data-testid="topnav-create-course-btn"
          aria-label="Tạo khóa học mới"
          onClick={() => {
            setActiveMode("dashboard");
            if (typeof window !== "undefined") {
              setTimeout(() => {
                const btn = document.querySelector('[data-testid="create-course-hub-btn"]') as HTMLButtonElement | null;
                btn?.click();
              }, 50);
            }
          }}
          className="hidden sm:flex min-w-[36px] min-h-[36px] px-2 lg:px-2.5 py-1.5 rounded-[10px] bg-[#5E51E8] hover:bg-[#4F46E5] text-white text-xs font-bold items-center justify-center gap-1.5 cursor-pointer shadow-xs whitespace-nowrap transition"
          title="Tạo khóa học mới"
        >
          <Sparkles className="w-3.5 h-3.5 text-amber-300 shrink-0" />
          <span className="hidden xl:inline">+ Tạo khóa học</span>
        </button>

        <button
          type="button"
          data-testid="open-ai-config-btn"
          onClick={() => {
            setAiSavedNotice(false);
            setAiConfigModalOpen(true);
          }}
          aria-label="Cấu hình khóa API AI"
          title="Cấu hình khóa API AI (Gemini / OpenAI Compatible)"
          className="hidden sm:flex min-w-[36px] min-h-[36px] px-2 lg:px-2.5 py-1.5 rounded-xl border border-slate-200 dark:border-slate-800 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300 text-xs font-semibold items-center justify-center gap-1 cursor-pointer whitespace-nowrap focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 transition-colors"
        >
          <Bot className="w-3.5 h-3.5 text-indigo-600 dark:text-indigo-400 shrink-0" />
          <span className="hidden 2xl:inline">AI Key</span>
        </button>

        <button
          type="button"
          data-testid="theme-toggle-btn"
          onClick={toggleDarkMode}
          aria-label="Chuyển đổi giao diện Sáng/Tối"
          title="Chuyển đổi giao diện Sáng/Tối"
          className="w-9 h-9 xl:w-10 xl:h-10 flex items-center justify-center p-1.5 sm:p-2 rounded-xl border border-slate-200 dark:border-slate-800 hover:bg-slate-100 dark:hover:bg-slate-800 text-xs cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 transition-colors"
        >
          {darkMode ? (
            <Sun className="w-4 h-4 text-amber-400 stroke-[2]" />
          ) : (
            <Moon className="w-4 h-4 text-slate-700 dark:text-slate-300 stroke-[2]" />
          )}
        </button>

        <div ref={avatarRef} className="relative flex items-center pl-0.5">
          <button
            type="button"
            data-testid="user-profile-btn"
            onClick={() => setAvatarMenuOpen((prev) => !prev)}
            className="min-w-[36px] min-h-[36px] flex items-center justify-center gap-1.5 p-1 rounded-full hover:ring-2 hover:ring-indigo-500/50 transition cursor-pointer"
            title="Hồ sơ người dùng & Tài khoản"
            aria-expanded={avatarMenuOpen}
            aria-haspopup="true"
          >
            <div className="w-7 h-7 rounded-full bg-indigo-600 text-white text-xs font-bold flex items-center justify-center">
              {userInitials}
            </div>
            <span className="text-xs font-semibold text-slate-800 dark:text-slate-200 hidden 2xl:inline">
              {userName}
            </span>
          </button>

          {avatarMenuOpen && (
            <div
              data-testid="user-profile-dropdown"
              className="absolute right-0 top-full mt-2 w-64 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xl p-3 z-50 text-xs text-slate-700 dark:text-slate-200 animate-in fade-in zoom-in-95 duration-100"
            >
              {/* User Info Header */}
              <div className="flex items-center gap-3 pb-3 border-b border-slate-100 dark:border-slate-800">
                <div className="w-9 h-9 rounded-full bg-indigo-600 text-white text-sm font-bold flex items-center justify-center shrink-0">
                  {userInitials}
                </div>
                <div className="min-w-0">
                  <div className="font-bold text-slate-900 dark:text-white truncate">{userName}</div>
                  <div className="text-[11px] text-slate-500 truncate">{userEmail}</div>
                  <div className="inline-block mt-0.5 px-1.5 py-0.2 rounded-md bg-indigo-50 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300 text-[10px] font-semibold">
                    Gói Miễn Phí (50MB / 50 AI)
                  </div>
                </div>
              </div>

              {/* Menu Links */}
              <div className="py-2 space-y-1">
                <Link
                  href="/app"
                  data-testid="switch-to-app-v2-link"
                  onClick={() => setAvatarMenuOpen(false)}
                  className="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg hover:bg-indigo-50 dark:hover:bg-indigo-950/40 text-indigo-600 dark:text-indigo-400 font-semibold text-left transition cursor-pointer"
                >
                  <Sparkles className="w-3.5 h-3.5" />
                  <span>Giao diện mới (/app)</span>
                </Link>
                <button
                  type="button"
                  onClick={() => {
                    setActiveMode("dashboard");
                    setAvatarMenuOpen(false);
                  }}
                  className="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-left transition cursor-pointer"
                >
                  <LayoutDashboard className="w-3.5 h-3.5 text-slate-500" />
                  <span>Không gian cá nhân</span>
                </button>
                {onOpenShortcuts && (
                  <button
                    type="button"
                    onClick={() => {
                      onOpenShortcuts();
                      setAvatarMenuOpen(false);
                    }}
                    className="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-left transition cursor-pointer"
                  >
                    <Keyboard className="w-3.5 h-3.5 text-slate-500" />
                    <span>Bảng phím tắt (?)</span>
                  </button>
                )}
                <button
                  type="button"
                  onClick={() => {
                    setAvatarMenuOpen(false);
                    setAiSavedNotice(false);
                    setAiConfigModalOpen(true);
                  }}
                  className="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-left transition cursor-pointer"
                >
                  <Bot className="w-3.5 h-3.5 text-indigo-500" />
                  <span>Cấu hình AI Key (⚙️ AI Key)</span>
                </button>
                <button
                  type="button"
                  data-testid="switch-account-btn"
                  onClick={() => {
                    setAvatarMenuOpen(false);
                    window.location.href = "/auth";
                  }}
                  className="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 text-left transition cursor-pointer"
                >
                  <Users className="w-3.5 h-3.5 text-indigo-500" />
                  <span>Chuyển đổi tài khoản (/auth)</span>
                </button>
              </div>

              {/* Logout Action */}
              <div className="pt-2 border-t border-slate-100 dark:border-slate-800">
                <button
                  type="button"
                  data-testid="logout-btn"
                  onClick={async () => {
                    try {
                      await fetch("/api/v1/auth/logout", {
                        method: "POST",
                        headers: getClientRequestHeaders(),
                        credentials: "include"
                      });
                    } catch {}
                    setAccessToken(null);
                    try {
                      window.localStorage.removeItem("swms_has_refresh_cookie");
                      window.localStorage.removeItem("swms_active_user_id");
                      window.localStorage.removeItem("swms_active_user");
                    } catch {}
                    setActiveUserHeaderId(null);
                    window.location.href = "/auth";
                  }}
                  className="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-rose-600 dark:text-rose-400 hover:bg-rose-50 dark:hover:bg-rose-950/40 text-left transition cursor-pointer font-semibold"
                >
                  <LogOut className="w-3.5 h-3.5" />
                  <span>Đăng xuất</span>
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      {aiConfigModalOpen && (
        <div
          data-testid="ai-config-modal"
          role="dialog"
          aria-modal="true"
          aria-labelledby="ai-config-modal-title"
          className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/60 backdrop-blur-xs"
          onClick={(e) => {
            if (e.target === e.currentTarget) setAiConfigModalOpen(false);
          }}
        >
          <div className="w-full max-w-md rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-2xl p-5 text-xs text-slate-800 dark:text-slate-100 space-y-4">
            <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-3">
              <div className="flex items-center gap-2">
                <Bot className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
                <h2 id="ai-config-modal-title" className="text-sm font-bold text-slate-900 dark:text-white">
                  Cấu hình AI Sinh Khóa học &amp; Giải thích (BYOK)
                </h2>
              </div>
              <button
                type="button"
                onClick={() => setAiConfigModalOpen(false)}
                className="p-1 rounded-lg text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 cursor-pointer"
                aria-label="Đóng"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            <div className="space-y-3">
              <div>
                <label className="block font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  API Key (Gemini / OpenAI Compatible)
                </label>
                <input
                  type="password"
                  data-testid="ai-api-key-input"
                  value={aiApiKey}
                  onChange={(e) => setAiApiKey(e.target.value)}
                  placeholder="AIzaSy... hoặc sk-..."
                  className="w-full px-3 py-2 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-xs text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              <div>
                <label className="block font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Base URL (Endpoint tương thích OpenAI)
                </label>
                <input
                  type="text"
                  data-testid="ai-base-url-input"
                  value={aiBaseUrl}
                  onChange={(e) => setAiBaseUrl(e.target.value)}
                  placeholder="https://generativelanguage.googleapis.com/v1beta/openai"
                  className="w-full px-3 py-2 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-xs text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              <div>
                <label className="block font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Model AI
                </label>
                <input
                  type="text"
                  data-testid="ai-model-input"
                  value={aiModel}
                  onChange={(e) => setAiModel(e.target.value)}
                  placeholder="gemini-2.5-flash"
                  className="w-full px-3 py-2 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-xs text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>
            </div>

            {aiSavedNotice && (
              <div className="p-2.5 rounded-xl bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-200 dark:border-emerald-800 text-emerald-700 dark:text-emerald-300 font-semibold">
                Đã lưu cấu hình AI vào trình duyệt (swms_ai_config).
              </div>
            )}

            <div className="flex items-center justify-end gap-2 pt-2 border-t border-slate-100 dark:border-slate-800">
              <button
                type="button"
                onClick={() => setAiConfigModalOpen(false)}
                className="px-3 py-2 rounded-xl border border-slate-200 dark:border-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 font-semibold cursor-pointer"
              >
                Đóng
              </button>
              <button
                type="button"
                data-testid="save-ai-config-btn"
                onClick={() => {
                  if (typeof window !== "undefined") {
                    window.localStorage.setItem(
                      "swms_ai_config",
                      JSON.stringify({
                        apiKey: aiApiKey.trim(),
                        baseUrl: aiBaseUrl.trim(),
                        model: aiModel.trim() || "gemini-2.5-flash"
                      })
                    );
                  }
                  setAiSavedNotice(true);
                  setTimeout(() => {
                    setAiConfigModalOpen(false);
                  }, 300);
                }}
                className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold cursor-pointer"
              >
                Lưu cấu hình AI
              </button>
            </div>
          </div>
        </div>
      )}
    </header>
  );
}

