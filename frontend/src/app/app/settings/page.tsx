"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  User,
  LogOut,
  Bot,
  Brain,
  Key,
  Globe,
  Cpu,
  Save,
  Check,
  Sun,
  Moon,
  Type,
  ALargeSmall,
  Keyboard,
  Info,
  ExternalLink,
  ShieldCheck,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { Button, Card, Badge } from "@/components/ui";
import { setAccessToken, fetchDashboardCached } from "@/lib/api-client";
import {
  getReviewCapSettings,
  saveReviewCapSettings,
  type ReviewCapSettings,
} from "@/lib/review-settings";

interface UserInfo {
  id: string;
  fullName: string;
  email: string;
  authProvider?: string;
  avatarUrl?: string | null;
  hasLocalPassword?: boolean;
}

interface AiConfig {
  apiKey: string;
  baseUrl: string;
  model: string;
}

const DEFAULT_AI_CONFIG: AiConfig = {
  apiKey: "",
  baseUrl: "https://generativelanguage.googleapis.com/v1beta/openai",
  model: "gemini-2.5-flash",
};

const SHORTCUTS = [
  { keys: "Ctrl+K", description: "Tìm kiếm nhanh" },
  { keys: "Ctrl+D", description: "Chế độ tối" },
  { keys: "Ctrl+\\", description: "Thu gọn sidebar" },
  { keys: "Space", description: "Lật thẻ (Ôn tập)" },
  { keys: "1-4", description: "Đánh giá thẻ" },
  { keys: "Z", description: "Hoàn tác" },
];

export default function SettingsPage() {
  const router = useRouter();
  const {
    activeUserHeaderId,
    setActiveUserHeaderId,
    darkMode,
    toggleDarkMode,
    serifFont,
    toggleSerifFont,
    fontSizePx,
    setFontSizePx,
  } = useWorkspaceStore();

  const [user, setUser] = useState<UserInfo>({ id: "", fullName: "", email: "" });
  const [isLoading, setIsLoading] = useState(true);
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  // Password form state
  const [showPasswordForm, setShowPasswordForm] = useState(false);
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [passwordFeedback, setPasswordFeedback] = useState<{ type: "ok" | "err"; message: string } | null>(null);
  const [isUpdatingPassword, setIsUpdatingPassword] = useState(false);

  // AI Config state
  const [aiConfig, setAiConfig] = useState<AiConfig>(DEFAULT_AI_CONFIG);
  const [showSavedToast, setShowSavedToast] = useState(false);

  // Daily Review Cap state
  const [reviewCaps, setReviewCaps] = useState<ReviewCapSettings>(getReviewCapSettings);
  const [showCapSavedToast, setShowCapSavedToast] = useState(false);

  const handleSaveReviewCaps = (e: React.FormEvent) => {
    e.preventDefault();
    saveReviewCapSettings(reviewCaps);
    setShowCapSavedToast(true);
    setTimeout(() => setShowCapSavedToast(false), 2500);
  };

  // Fetch user data
  const fetchUser = useCallback(async () => {
    setIsLoading(true);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );
      const [meResp, dashData] = await Promise.all([
        fetch("/api/v1/auth/me", { credentials: "include", headers }).catch(() => null),
        fetchDashboardCached({ headers }).catch(() => null),
      ]);
      const meData = meResp && meResp.ok ? await meResp.json().catch(() => null) : null;
      const baseUser = dashData?.user ?? {};
      if (meData || dashData) {
        setUser({
          id: String(meData?.id ?? baseUser.id ?? ""),
          fullName: String(meData?.fullName ?? baseUser.fullName ?? ""),
          email: String(meData?.email ?? baseUser.email ?? ""),
          authProvider: meData?.authProvider ?? "local",
          avatarUrl: meData?.avatarUrl ?? null,
          hasLocalPassword: meData?.hasLocalPassword ?? true,
        });
      }
    } catch {
      /* silent */
    } finally {
      setIsLoading(false);
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    fetchUser();
  }, [fetchUser]);

  // Load AI config from localStorage
  useEffect(() => {
    try {
      const raw = window.localStorage.getItem("swms_ai_config");
      if (raw) {
        const parsed = JSON.parse(raw);
        setAiConfig({
          apiKey: parsed.apiKey ?? DEFAULT_AI_CONFIG.apiKey,
          baseUrl: parsed.baseUrl ?? DEFAULT_AI_CONFIG.baseUrl,
          model: parsed.model ?? DEFAULT_AI_CONFIG.model,
        });
      }
    } catch {
      /* ignore */
    }
  }, []);

  const handleSaveAiConfig = () => {
    window.localStorage.setItem(
      "swms_ai_config",
      JSON.stringify({
        apiKey: aiConfig.apiKey,
        baseUrl: aiConfig.baseUrl,
        model: aiConfig.model,
      })
    );
    setShowSavedToast(true);
    setTimeout(() => setShowSavedToast(false), 2000);
  };

  const handleLogout = async () => {
    setIsLoggingOut(true);
    try {
      await fetch("/api/v1/auth/logout", {
        method: "POST",
        headers: getClientRequestHeaders(),
        credentials: "include",
      });
    } catch {
      /* silent */
    }
    setAccessToken(null);
    setActiveUserHeaderId(null);
    try {
      window.localStorage.removeItem("swms_has_refresh_cookie");
      window.localStorage.removeItem("swms_active_user_id");
      window.localStorage.removeItem("swms_active_user");
    } catch {
      /* silent */
    }
    router.push("/auth");
  };

  // Avatar initials
  const initials = user.fullName
    ? user.fullName
        .split(" ")
        .map((w) => w[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : "?";

  if (isLoading) {
    return (
      <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-8 min-h-[80vh] space-y-6">
        <div className="space-y-2">
          <div className="h-8 w-36 rounded-lg swms-skeleton" />
          <div className="h-4 w-80 rounded-md swms-skeleton" />
        </div>
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          <div className="lg:col-span-7 space-y-6">
            {[0, 1, 2].map((i) => (
              <div key={i} className="h-48 rounded-xl swms-skeleton" />
            ))}
          </div>
          <div className="lg:col-span-5 space-y-6">
            {[0, 1, 2].map((i) => (
              <div key={i} className="h-44 rounded-xl swms-skeleton" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-8 min-h-[80vh]">
      <div className="mb-6">
        <h1 className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">Cài đặt</h1>
        <p className="text-sm text-[var(--text-secondary)] mt-1">
          Quản lý tài khoản, mô hình AI cá nhân, giới hạn ôn tập FSRS và tùy biến giao diện đọc.
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        <div className="lg:col-span-7 space-y-6">
      {/* Section 1: Tài khoản & Bảo mật */}
      <Card className="p-6 bg-[var(--surface-card)] border-[var(--border-calm)]">
        <div className="flex items-start gap-3 mb-5">
          <div className="w-8 h-8 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0">
            <User className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
          </div>
          <div>
            <h2 className="text-base font-semibold text-[var(--text-primary)]">
              Tài khoản &amp; Bảo mật
            </h2>
            <p className="text-xs text-[var(--text-tertiary)] mt-0.5">
              Thông tin định danh, phương thức đăng nhập và mật khẩu cá nhân.
            </p>
          </div>
        </div>
        <div className="flex flex-wrap items-center gap-4">
          {user.avatarUrl ? (
            <img
              src={user.avatarUrl}
              alt={user.fullName || "Avatar"}
              referrerPolicy="no-referrer"
              className="w-12 h-12 rounded-full object-cover border border-[var(--border-calm)] shrink-0"
            />
          ) : (
            <div className="w-12 h-12 rounded-full bg-[#5E51E8] dark:bg-[#818CF8] flex items-center justify-center text-white dark:text-[#0F0F12] font-semibold text-sm shrink-0">
              {initials}
            </div>
          )}
          <div className="flex-1 min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <p className="text-sm font-medium text-[var(--text-primary)] truncate">
                {user.fullName || "Chưa có tên"}
              </p>
              <span
                data-testid="settings-auth-provider-badge"
                className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-[var(--text-secondary)]"
              >
                <ShieldCheck className="w-3 h-3 text-[#5E51E8] dark:text-[#818CF8]" />
                {user.authProvider === "local+google"
                  ? "Google + Email"
                  : user.authProvider === "google"
                  ? "Google"
                  : "Email & Mật khẩu"}
              </span>
            </div>
            <p className="text-sm text-[var(--text-secondary)] truncate">
              {user.email || "Chưa có email"}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <button
              type="button"
              data-testid="toggle-password-form-btn"
              onClick={() => {
                setShowPasswordForm((v) => !v);
                setPasswordFeedback(null);
              }}
              className="flex items-center gap-1.5 h-9 px-3 rounded-[8px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-[var(--text-primary)] text-xs font-medium hover:border-[#5E51E8]/50 transition cursor-pointer"
            >
              <Key className="w-3.5 h-3.5" />
              <span>{user.hasLocalPassword === false ? "Đặt mật khẩu" : "Đổi mật khẩu"}</span>
            </button>
            <button
              type="button"
              data-testid="settings-logout-btn"
              onClick={handleLogout}
              disabled={isLoggingOut}
              className="flex items-center gap-2 h-9 px-4 rounded-[8px] border border-red-300 dark:border-red-700 text-red-600 dark:text-red-400 text-sm font-medium hover:bg-red-50 dark:hover:bg-red-950/30 transition disabled:opacity-50 cursor-pointer"
            >
              <LogOut className="w-4 h-4" />
              <span>{isLoggingOut ? "Đang xuất..." : "Đăng xuất"}</span>
            </button>
          </div>
        </div>

        {showPasswordForm && (
          <form
            data-testid="change-password-section"
            onSubmit={async (e) => {
              e.preventDefault();
              setPasswordFeedback(null);
              if (newPassword.length < 6) {
                setPasswordFeedback({ type: "err", message: "Mật khẩu mới phải tối thiểu 6 ký tự." });
                return;
              }
              setIsUpdatingPassword(true);
              try {
                const effectiveUserId =
                  activeUserHeaderId ??
                  (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
                const resp = await fetch("/api/v1/auth/change-password", {
                  method: "POST",
                  credentials: "include",
                  headers: getClientRequestHeaders({
                    "Content-Type": "application/json",
                    ...(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : {}),
                  }),
                  body: JSON.stringify({
                    currentPassword: user.hasLocalPassword === false ? null : currentPassword,
                    newPassword,
                  }),
                });
                const body = await resp.json().catch(() => null);
                if (!resp.ok) {
                  setPasswordFeedback({
                    type: "err",
                    message: body?.error || "Không thể cập nhật mật khẩu.",
                  });
                  return;
                }
                if (body?.accessToken) {
                  setAccessToken(String(body.accessToken));
                }
                if (body?.user) {
                  setUser((prev) => ({
                    ...prev,
                    authProvider: body.user.authProvider ?? prev.authProvider,
                    hasLocalPassword: body.user.hasLocalPassword ?? true,
                  }));
                }
                setCurrentPassword("");
                setNewPassword("");
                setPasswordFeedback({
                  type: "ok",
                  message: body?.message || "Đã cập nhật mật khẩu thành công.",
                });
              } catch {
                setPasswordFeedback({ type: "err", message: "Lỗi kết nối máy chủ." });
              } finally {
                setIsUpdatingPassword(false);
              }
            }}
            className="mt-4 pt-4 border-t border-border space-y-3"
          >
            {user.hasLocalPassword !== false && (
              <div>
                <label className="block text-xs font-medium text-text-secondary mb-1">
                  Mật khẩu hiện tại
                </label>
                <input
                  type="password"
                  data-testid="settings-current-password-input"
                  value={currentPassword}
                  onChange={(e) => setCurrentPassword(e.target.value)}
                  required
                  className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-accent"
                />
              </div>
            )}
            <div>
              <label className="block text-xs font-medium text-text-secondary mb-1">
                {user.hasLocalPassword === false
                  ? "Mật khẩu mới (để đăng nhập bằng Email)"
                  : "Mật khẩu mới (tối thiểu 6 ký tự)"}
              </label>
              <input
                type="password"
                data-testid="settings-new-password-input"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
                minLength={6}
                className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-accent"
              />
            </div>
            {passwordFeedback && (
              <p
                data-testid="settings-password-feedback"
                className={`text-xs font-medium ${
                  passwordFeedback.type === "ok"
                    ? "text-emerald-600 dark:text-emerald-400"
                    : "text-rose-600 dark:text-rose-400"
                }`}
              >
                {passwordFeedback.message}
              </p>
            )}
            <button
              type="submit"
              data-testid="settings-save-password-btn"
              disabled={isUpdatingPassword}
              className="h-9 px-4 rounded-md bg-accent text-white text-xs font-semibold hover:opacity-90 transition disabled:opacity-50 cursor-pointer"
            >
              {isUpdatingPassword ? "Đang lưu..." : "Lưu mật khẩu"}
            </button>
          </form>
        )}
      </Card>

      {/* Section 2: Cấu hình AI */}
      <Card className="p-6 bg-[var(--surface-card)] border-[var(--border-calm)]">
        <div className="flex items-start gap-3 mb-5">
          <div className="w-8 h-8 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0">
            <Bot className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
          </div>
          <div>
            <h2 className="text-base font-semibold text-[var(--text-primary)]">
              Cấu hình AI
            </h2>
            <p className="text-xs text-[var(--text-tertiary)] mt-0.5">
              Tùy chỉnh khóa API và mô hình AI cá nhân (lưu cục bộ trên trình duyệt).
            </p>
          </div>
        </div>
        <div className="space-y-4">
          {/* API Key */}
          <div>
            <label className="flex items-center gap-1.5 text-sm font-medium text-[var(--text-secondary)] mb-1.5">
              <Key className="w-3.5 h-3.5" />
              API Key
            </label>
            <input
              type="password"
              value={aiConfig.apiKey}
              onChange={(e) => setAiConfig((c) => ({ ...c, apiKey: e.target.value }))}
              placeholder="Nhập API key..."
              className="w-full h-9 px-3 rounded-[8px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:ring-2 focus:ring-[#5E51E8]/40 focus:border-[#5E51E8] transition"
            />
          </div>

          {/* Base URL */}
          <div>
            <label className="flex items-center gap-1.5 text-sm font-medium text-[var(--text-secondary)] mb-1.5">
              <Globe className="w-3.5 h-3.5" />
              Base URL
            </label>
            <input
              type="text"
              value={aiConfig.baseUrl}
              onChange={(e) => setAiConfig((c) => ({ ...c, baseUrl: e.target.value }))}
              placeholder={DEFAULT_AI_CONFIG.baseUrl}
              className="w-full h-9 px-3 rounded-[8px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:ring-2 focus:ring-[#5E51E8]/40 focus:border-[#5E51E8] transition"
            />
          </div>

          {/* Model */}
          <div>
            <label className="flex items-center gap-1.5 text-sm font-medium text-[var(--text-secondary)] mb-1.5">
              <Cpu className="w-3.5 h-3.5" />
              Model
            </label>
            <input
              type="text"
              value={aiConfig.model}
              onChange={(e) => setAiConfig((c) => ({ ...c, model: e.target.value }))}
              placeholder={DEFAULT_AI_CONFIG.model}
              className="w-full h-9 px-3 rounded-[8px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:ring-2 focus:ring-[#5E51E8]/40 focus:border-[#5E51E8] transition"
            />
          </div>

          {/* Save button + toast */}
          <div className="flex flex-wrap items-center gap-3 pt-1">
            <button
              type="button"
              onClick={handleSaveAiConfig}
              className="flex items-center gap-2 h-9 px-4 rounded-[8px] bg-[#5E51E8] hover:bg-[#4F46E5] dark:bg-[#818CF8] dark:hover:bg-[#6366F1] text-white dark:text-[#0F0F12] text-sm font-medium transition cursor-pointer"
            >
              <Save className="w-4 h-4" />
              Lưu cấu hình
            </button>
            {showSavedToast && (
              <span
                role="status"
                className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full border border-emerald-500/30 bg-emerald-500/10 text-xs text-emerald-700 dark:text-emerald-300 font-semibold shadow-xs"
              >
                <Check className="w-3.5 h-3.5" />
                Đã lưu cấu hình AI
              </span>
            )}
          </div>
        </div>
      </Card>

      {/* Section 2b: Mục tiêu Ôn tập FSRS */}
      <Card className="p-6 bg-[var(--surface-card)] border-[var(--border-calm)]">
        <div className="flex items-start gap-3 mb-5">
          <div className="w-8 h-8 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0">
            <Brain className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
          </div>
          <div>
            <h2 className="text-base font-semibold text-[var(--text-primary)]">
              Mục tiêu Ôn tập FSRS
            </h2>
            <p className="text-xs text-[var(--text-tertiary)] mt-0.5">
              Giới hạn số thẻ mỗi ngày để duy trì nhịp độ ghi nhớ bền vững, tránh quá tải.
            </p>
          </div>
        </div>
        <form onSubmit={handleSaveReviewCaps} className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="flex items-center gap-1.5 text-sm font-medium text-[var(--text-secondary)] mb-1.5">
                Số thẻ ôn tập tối đa / ngày
              </label>
              <input
                data-testid="settings-review-cap-input"
                type="number"
                min={10}
                max={500}
                value={reviewCaps.maxReviewsPerDay}
                onChange={(e) =>
                  setReviewCaps((c) => ({
                    ...c,
                    maxReviewsPerDay: Number(e.target.value) || 100,
                  }))
                }
                className="w-full h-9 px-3 rounded-[8px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-sm text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[#5E51E8]/40 focus:border-[#5E51E8]"
              />
              <p className="text-[11px] text-[var(--text-tertiary)] mt-1">
                Mặc định: 100 thẻ. Giúp chống quá tải nhận thức khi ôn thi.
              </p>
            </div>

            <div>
              <label className="flex items-center gap-1.5 text-sm font-medium text-[var(--text-secondary)] mb-1.5">
                Số thẻ mới tối đa / ngày
              </label>
              <input
                data-testid="settings-new-cap-input"
                type="number"
                min={5}
                max={100}
                value={reviewCaps.maxNewPerDay}
                onChange={(e) =>
                  setReviewCaps((c) => ({
                    ...c,
                    maxNewPerDay: Number(e.target.value) || 20,
                  }))
                }
                className="w-full h-9 px-3 rounded-[8px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-sm text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[#5E51E8]/40 focus:border-[#5E51E8]"
              />
              <p className="text-[11px] text-[var(--text-tertiary)] mt-1">
                Mặc định: 20 thẻ mới. Điều phối nhịp độ tiếp thu kiến thức mới.
              </p>
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-3 pt-1">
            <button
              type="submit"
              data-testid="settings-save-review-caps-btn"
              className="flex items-center gap-2 h-9 px-4 rounded-[8px] bg-[#5E51E8] hover:bg-[#4F46E5] dark:bg-[#818CF8] dark:hover:bg-[#6366F1] text-white dark:text-[#0F0F12] text-sm font-medium transition cursor-pointer"
            >
              <Save className="w-4 h-4" />
              Lưu giới hạn ôn tập
            </button>
            {showCapSavedToast && (
              <span
                role="status"
                className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full border border-emerald-500/30 bg-emerald-500/10 text-xs text-emerald-700 dark:text-emerald-300 font-semibold shadow-xs"
              >
                <Check className="w-3.5 h-3.5" />
                Đã lưu mục tiêu FSRS
              </span>
            )}
          </div>
        </form>
      </Card>
        </div>

        <div className="lg:col-span-5 space-y-6">
      {/* Section 3: Giao diện & Phông chữ đọc */}
      <Card className="p-6 bg-[var(--surface-card)] border-[var(--border-calm)]">
        <div className="flex items-start gap-3 mb-5">
          <div className="w-8 h-8 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0">
            <Sun className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
          </div>
          <div>
            <h2 className="text-base font-semibold text-[var(--text-primary)]">
              Giao diện &amp; Phông chữ đọc
            </h2>
            <p className="text-xs text-[var(--text-tertiary)] mt-0.5">
              Tối ưu hóa trải nghiệm đọc giáo trình và giảm mỏi mắt.
            </p>
          </div>
        </div>
        <div className="space-y-5">
          {/* Dark mode toggle */}
          <div className="flex items-center justify-between gap-3">
            <div className="flex items-center gap-3 min-w-0">
              {darkMode ? <Moon className="w-4 h-4 text-[var(--text-secondary)] shrink-0" /> : <Sun className="w-4 h-4 text-[var(--text-secondary)] shrink-0" />}
              <div>
                <p className="text-sm font-medium text-[var(--text-primary)]">Chế độ tối</p>
                <p className="text-xs text-[var(--text-tertiary)]">
                  {darkMode ? "Đang bật" : "Đang tắt"}
                </p>
              </div>
            </div>
            <button
              type="button"
              role="switch"
              aria-checked={darkMode}
              aria-label="Bật hoặc tắt chế độ tối"
              onClick={toggleDarkMode}
              className={`relative w-11 h-6 rounded-full transition-colors cursor-pointer shrink-0 ${
                darkMode ? "bg-[#5E51E8] dark:bg-[#818CF8]" : "bg-[var(--surface-elevated)] border border-[var(--border-calm)]"
              }`}
            >
              <span
                className={`absolute top-0.5 left-0.5 w-5 h-5 rounded-full bg-white shadow transition-transform ${
                  darkMode ? "translate-x-5" : "translate-x-0"
                }`}
              />
            </button>
          </div>

          {/* Serif / Sans toggle */}
          <div className="flex items-center justify-between gap-3">
            <div className="flex items-center gap-3 min-w-0">
              <Type className="w-4 h-4 text-[var(--text-secondary)] shrink-0" />
              <div>
                <p className="text-sm font-medium text-[var(--text-primary)]">Phông chữ Serif</p>
                <p className="text-xs text-[var(--text-tertiary)]">
                  {serifFont ? "Serif (có chân)" : "Sans-serif (không chân)"}
                </p>
              </div>
            </div>
            <button
              type="button"
              role="switch"
              aria-checked={serifFont}
              aria-label="Bật hoặc tắt phông chữ Serif"
              onClick={toggleSerifFont}
              className={`relative w-11 h-6 rounded-full transition-colors cursor-pointer shrink-0 ${
                serifFont ? "bg-[#5E51E8] dark:bg-[#818CF8]" : "bg-[var(--surface-elevated)] border border-[var(--border-calm)]"
              }`}
            >
              <span
                className={`absolute top-0.5 left-0.5 w-5 h-5 rounded-full bg-white shadow transition-transform ${
                  serifFont ? "translate-x-5" : "translate-x-0"
                }`}
              />
            </button>
          </div>

          {/* Font size slider */}
          <div className="pt-1 border-t border-[var(--border-calm)]">
            <div className="flex items-center justify-between gap-3 mb-3">
              <div className="flex items-center gap-3">
                <ALargeSmall className="w-4 h-4 text-[var(--text-secondary)]" />
                <p className="text-sm font-medium text-[var(--text-primary)]">Cỡ chữ đọc</p>
              </div>
              <Badge variant="neutral" size="sm">{fontSizePx}px</Badge>
            </div>
            <div className="flex items-center gap-3">
              <span className="text-xs text-[var(--text-tertiary)] w-7 text-right">12</span>
              <input
                type="range"
                min={12}
                max={20}
                step={1}
                value={fontSizePx}
                onChange={(e) => setFontSizePx(Number(e.target.value))}
                className="flex-1 h-1.5 accent-[#5E51E8] cursor-pointer"
              />
              <span className="text-xs text-[var(--text-tertiary)] w-7">20</span>
            </div>
          </div>
        </div>
      </Card>

      {/* Section 4: Phím tắt & Tiện ích */}
      <Card className="p-6 bg-[var(--surface-card)] border-[var(--border-calm)]">
        <div className="flex items-start gap-3 mb-4">
          <div className="w-8 h-8 rounded-[8px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 flex items-center justify-center shrink-0">
            <Keyboard className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
          </div>
          <div>
            <h2 className="text-base font-semibold text-[var(--text-primary)]">
              Phím tắt &amp; Tiện ích
            </h2>
            <p className="text-xs text-[var(--text-tertiary)] mt-0.5">
              Thao tác nhanh bằng bàn phím trong toàn bộ không gian học.
            </p>
          </div>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--border-calm)]">
                <th className="text-left py-2 pr-4 font-medium text-[var(--text-secondary)]">
                  Phím tắt
                </th>
                <th className="text-left py-2 font-medium text-[var(--text-secondary)]">
                  Chức năng
                </th>
              </tr>
            </thead>
            <tbody>
              {SHORTCUTS.map((s) => (
                <tr key={s.keys} className="border-b border-[var(--border-calm)]/60 last:border-0">
                  <td className="py-2.5 pr-4">
                    <kbd className="inline-block text-xs font-mono px-2 py-1 rounded-md bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-[var(--text-primary)]">
                      {s.keys}
                    </kbd>
                  </td>
                  <td className="py-2.5 text-[var(--text-secondary)]">{s.description}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Section 5: Thông tin ứng dụng */}
      <Card className="p-6 bg-[var(--surface-card)] border-[var(--border-calm)]">
        <h2 className="text-base font-semibold text-[var(--text-primary)] flex items-center gap-2 mb-4">
          <Info className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
          Thông tin ứng dụng
        </h2>
        <div className="space-y-2.5">
          <div className="flex items-center justify-between">
            <span className="text-sm text-[var(--text-secondary)]">Ứng dụng</span>
            <span className="text-sm font-medium text-[var(--text-primary)]">Studxy</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-[var(--text-secondary)]">Phiên bản</span>
            <span className="text-sm font-medium text-[var(--text-primary)]">2.0 — UI Redesign</span>
          </div>
          <div className="flex items-center justify-between pt-2 border-t border-[var(--border-calm)]">
            <span className="text-sm text-[var(--text-secondary)]">Không gian làm việc khác</span>
            <Link
              href="/workspace"
              data-testid="settings-workspace-classic-link"
              className="flex items-center gap-1.5 text-sm font-medium text-[#5E51E8] dark:text-[#818CF8] hover:underline"
            >
              Mở Workspace Classic
              <ExternalLink className="w-3.5 h-3.5" />
            </Link>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-[var(--text-secondary)]">Tài khoản &amp; Phiên đăng nhập</span>
            <Link
              href="/auth"
              className="flex items-center gap-1.5 text-sm font-medium text-[#5E51E8] dark:text-[#818CF8] hover:underline"
            >
              Quản lý tài khoản
              <ExternalLink className="w-3.5 h-3.5" />
            </Link>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-[var(--text-secondary)]">Xuất bản &amp; Quản trị khóa học</span>
            <Link
              href="/app/studio"
              className="flex items-center gap-1.5 text-sm font-medium text-[#5E51E8] dark:text-[#818CF8] hover:underline"
            >
              Mở Course Studio
              <ExternalLink className="w-3.5 h-3.5" />
            </Link>
          </div>
        </div>
      </Card>
        </div>
      </div>
    </div>
  );
}
