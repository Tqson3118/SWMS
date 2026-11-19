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
      <div className="max-w-[1320px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-8 min-h-[80vh] space-y-6">
        <div className="h-10 w-40 rounded-lg bg-[var(--surface-elevated)] animate-pulse" />
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          <div className="lg:col-span-7 space-y-4">
            {[0, 1].map((i) => (
              <div key={i} className="h-44 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
            ))}
          </div>
          <div className="lg:col-span-5 space-y-4">
            {[0, 1].map((i) => (
              <div key={i} className="h-44 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-[1320px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-8 min-h-[80vh]">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-text-primary">Cài đặt</h1>
        <p className="text-sm text-text-secondary mt-1">
          Quản lý tài khoản, mô hình AI cá nhân, giới hạn ôn tập FSRS và tùy biến giao diện đọc.
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        <div className="lg:col-span-7 space-y-6">
      {/* Section 1: Tài khoản */}
      <Card className="p-6 bg-[var(--surface-card)]">
        <h2 className="text-base font-semibold text-text-primary flex items-center gap-2 mb-4">
          <User className="w-4 h-4" />
          Tài khoản
        </h2>
        <div className="flex flex-wrap items-center gap-4">
          {user.avatarUrl ? (
            <img
              src={user.avatarUrl}
              alt={user.fullName || "Avatar"}
              referrerPolicy="no-referrer"
              className="w-12 h-12 rounded-full object-cover border border-border shrink-0"
            />
          ) : (
            <div className="w-12 h-12 rounded-full bg-accent flex items-center justify-center text-white font-semibold text-sm shrink-0">
              {initials}
            </div>
          )}
          <div className="flex-1 min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <p className="text-sm font-medium text-text-primary truncate">
                {user.fullName || "Chưa có tên"}
              </p>
              <span
                data-testid="settings-auth-provider-badge"
                className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold bg-bg-tertiary border border-border text-text-secondary"
              >
                <ShieldCheck className="w-3 h-3 text-accent" />
                {user.authProvider === "local+google"
                  ? "Google + Email"
                  : user.authProvider === "google"
                  ? "Google"
                  : "Email & Mật khẩu"}
              </span>
            </div>
            <p className="text-sm text-text-secondary truncate">
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
              className="flex items-center gap-1.5 h-9 px-3 rounded-md border border-border bg-bg-primary text-text-primary text-xs font-medium hover:bg-bg-tertiary transition cursor-pointer"
            >
              <Key className="w-3.5 h-3.5" />
              <span>{user.hasLocalPassword === false ? "Đặt mật khẩu" : "Đổi mật khẩu"}</span>
            </button>
            <button
              type="button"
              data-testid="settings-logout-btn"
              onClick={handleLogout}
              disabled={isLoggingOut}
              className="flex items-center gap-2 h-9 px-4 rounded-md border border-red-300 dark:border-red-700 text-red-600 dark:text-red-400 text-sm font-medium hover:bg-red-50 dark:hover:bg-red-950/30 transition disabled:opacity-50 cursor-pointer"
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
      <Card className="p-6 bg-[var(--surface-card)]">
        <h2 className="text-base font-semibold text-text-primary flex items-center gap-2 mb-4">
          <Bot className="w-4 h-4" />
          Cấu hình AI
        </h2>
        <div className="space-y-4">
          {/* API Key */}
          <div>
            <label className="flex items-center gap-1.5 text-sm font-medium text-text-secondary mb-1.5">
              <Key className="w-3.5 h-3.5" />
              API Key
            </label>
            <input
              type="password"
              value={aiConfig.apiKey}
              onChange={(e) => setAiConfig((c) => ({ ...c, apiKey: e.target.value }))}
              placeholder="Nhập API key..."
              className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:ring-2 focus:ring-accent/40 focus:border-accent transition"
            />
          </div>

          {/* Base URL */}
          <div>
            <label className="flex items-center gap-1.5 text-sm font-medium text-text-secondary mb-1.5">
              <Globe className="w-3.5 h-3.5" />
              Base URL
            </label>
            <input
              type="text"
              value={aiConfig.baseUrl}
              onChange={(e) => setAiConfig((c) => ({ ...c, baseUrl: e.target.value }))}
              placeholder={DEFAULT_AI_CONFIG.baseUrl}
              className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:ring-2 focus:ring-accent/40 focus:border-accent transition"
            />
          </div>

          {/* Model */}
          <div>
            <label className="flex items-center gap-1.5 text-sm font-medium text-text-secondary mb-1.5">
              <Cpu className="w-3.5 h-3.5" />
              Model
            </label>
            <input
              type="text"
              value={aiConfig.model}
              onChange={(e) => setAiConfig((c) => ({ ...c, model: e.target.value }))}
              placeholder={DEFAULT_AI_CONFIG.model}
              className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:ring-2 focus:ring-accent/40 focus:border-accent transition"
            />
          </div>

          {/* Save button + toast */}
          <div className="flex items-center gap-3 pt-1">
            <button
              type="button"
              onClick={handleSaveAiConfig}
              className="flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition cursor-pointer"
            >
              <Save className="w-4 h-4" />
              Lưu cấu hình
            </button>
            {showSavedToast && (
              <span className="flex items-center gap-1.5 text-sm text-emerald-600 dark:text-emerald-400 font-medium animate-in fade-in">
                <Check className="w-4 h-4" />
                Đã lưu ✓
              </span>
            )}
          </div>
        </div>
      </Card>

      {/* Section 2b: Giới hạn ôn tập hàng ngày (Daily Review Cap) */}
      <Card className="p-6 bg-[var(--surface-card)]">
        <h2 className="text-base font-semibold text-text-primary flex items-center gap-2 mb-4">
          <Brain className="w-4 h-4 text-accent" />
          Giới hạn ôn tập hàng ngày (Daily Review Cap)
        </h2>
        <form onSubmit={handleSaveReviewCaps} className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="flex items-center gap-1.5 text-sm font-medium text-text-secondary mb-1.5">
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
                className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-accent"
              />
              <p className="text-[11px] text-text-tertiary mt-1">
                Mặc định: 100 thẻ. Giúp chống quá tải nhận thức khi ôn thi.
              </p>
            </div>

            <div>
              <label className="flex items-center gap-1.5 text-sm font-medium text-text-secondary mb-1.5">
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
                className="w-full h-9 px-3 rounded-md border border-border bg-bg-primary text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-accent"
              />
              <p className="text-[11px] text-text-tertiary mt-1">
                Mặc định: 20 thẻ mới. Điều phối nhịp độ tiếp thu kiến thức mới.
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3 pt-1">
            <button
              type="submit"
              data-testid="settings-save-review-caps-btn"
              className="flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition cursor-pointer"
            >
              <Save className="w-4 h-4" />
              Lưu giới hạn ôn tập
            </button>
            {showCapSavedToast && (
              <span className="flex items-center gap-1.5 text-sm text-emerald-600 dark:text-emerald-400 font-medium animate-in fade-in">
                <Check className="w-4 h-4" />
                Đã lưu ✓
              </span>
            )}
          </div>
        </form>
      </Card>
        </div>

        <div className="lg:col-span-5 space-y-6">
      {/* Section 3: Giao diện */}
      <Card className="p-6 bg-[var(--surface-card)]">
        <h2 className="text-base font-semibold text-text-primary flex items-center gap-2 mb-4">
          <Sun className="w-4 h-4" />
          Giao diện
        </h2>
        <div className="space-y-5">
          {/* Dark mode toggle */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              {darkMode ? <Moon className="w-4 h-4 text-text-secondary" /> : <Sun className="w-4 h-4 text-text-secondary" />}
              <div>
                <p className="text-sm font-medium text-text-primary">Chế độ tối</p>
                <p className="text-xs text-text-tertiary">
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
              className={`relative w-11 h-6 rounded-full transition-colors cursor-pointer ${
                darkMode ? "bg-accent" : "bg-bg-tertiary border border-border"
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
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <Type className="w-4 h-4 text-text-secondary" />
              <div>
                <p className="text-sm font-medium text-text-primary">Phông chữ Serif</p>
                <p className="text-xs text-text-tertiary">
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
              className={`relative w-11 h-6 rounded-full transition-colors cursor-pointer ${
                serifFont ? "bg-accent" : "bg-bg-tertiary border border-border"
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
          <div>
            <div className="flex items-center gap-3 mb-3">
              <ALargeSmall className="w-4 h-4 text-text-secondary" />
              <div>
                <p className="text-sm font-medium text-text-primary">Cỡ chữ</p>
                <p className="text-xs text-text-tertiary">{fontSizePx}px</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <span className="text-xs text-text-tertiary w-7 text-right">12</span>
              <input
                type="range"
                min={12}
                max={20}
                step={1}
                value={fontSizePx}
                onChange={(e) => setFontSizePx(Number(e.target.value))}
                className="flex-1 h-1.5 accent-accent cursor-pointer"
              />
              <span className="text-xs text-text-tertiary w-7">20</span>
            </div>
          </div>
        </div>
      </Card>

      {/* Section 4: Phím tắt */}
      <Card className="p-6 bg-[var(--surface-card)]">
        <h2 className="text-base font-semibold text-text-primary flex items-center gap-2 mb-4">
          <Keyboard className="w-4 h-4" />
          Phím tắt
        </h2>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium text-text-secondary">
                  Phím tắt
                </th>
                <th className="text-left py-2 font-medium text-text-secondary">
                  Chức năng
                </th>
              </tr>
            </thead>
            <tbody>
              {SHORTCUTS.map((s) => (
                <tr key={s.keys} className="border-b border-border-subtle last:border-0">
                  <td className="py-2.5 pr-4">
                    <kbd className="inline-block text-xs font-mono px-2 py-1 rounded-md bg-bg-tertiary border border-border text-text-primary">
                      {s.keys}
                    </kbd>
                  </td>
                  <td className="py-2.5 text-text-secondary">{s.description}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Section 5: Thông tin ứng dụng */}
      <Card className="p-6 bg-[var(--surface-card)]">
        <h2 className="text-base font-semibold text-text-primary flex items-center gap-2 mb-4">
          <Info className="w-4 h-4" />
          Thông tin ứng dụng
        </h2>
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-sm text-text-secondary">Ứng dụng</span>
            <span className="text-sm font-medium text-text-primary">Studxy</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-text-secondary">Phiên bản</span>
            <span className="text-sm font-medium text-text-primary">2.0 — UI Redesign</span>
          </div>
          <div className="flex items-center justify-between pt-2">
            <span className="text-sm text-text-secondary">Không gian làm việc khác</span>
            <Link
              href="/workspace"
              data-testid="settings-workspace-classic-link"
              className="flex items-center gap-1.5 text-sm font-medium text-accent hover:underline"
            >
              Mở Workspace Classic
              <ExternalLink className="w-3.5 h-3.5" />
            </Link>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-text-secondary">Tài khoản &amp; Phiên đăng nhập</span>
            <Link
              href="/auth"
              className="flex items-center gap-1.5 text-sm font-medium text-accent hover:underline"
            >
              Quản lý tài khoản
              <ExternalLink className="w-3.5 h-3.5" />
            </Link>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-text-secondary">Xuất bản &amp; Quản trị khóa học</span>
            <Link
              href="/app/studio"
              className="flex items-center gap-1.5 text-sm font-medium text-accent hover:underline"
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
