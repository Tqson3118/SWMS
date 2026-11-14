"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  User,
  LogOut,
  Bot,
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
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";

interface UserInfo {
  id: string;
  fullName: string;
  email: string;
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

  // AI Config state
  const [aiConfig, setAiConfig] = useState<AiConfig>(DEFAULT_AI_CONFIG);
  const [showSavedToast, setShowSavedToast] = useState(false);

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
      const resp = await fetch("/api/v1/workspace/dashboard", { headers });
      if (resp.ok) {
        const data = await resp.json();
        setUser(data.user ?? { id: "", fullName: "", email: "" });
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
      await fetch("/api/v1/auth/logout", { method: "POST" });
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
      <div className="flex items-center justify-center h-full">
        <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pb-20 md:pb-8">
      <h1 className="text-2xl font-bold text-text-primary mb-8">Cài đặt</h1>

      {/* Section 1: Tài khoản */}
      <section className="bg-bg-secondary rounded-lg border border-border p-6 mb-6">
        <h2 className="text-base font-semibold text-text-primary flex items-center gap-2 mb-4">
          <User className="w-4 h-4" />
          Tài khoản
        </h2>
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-full bg-accent flex items-center justify-center text-white font-semibold text-sm shrink-0">
            {initials}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-text-primary truncate">
              {user.fullName || "Chưa có tên"}
            </p>
            <p className="text-sm text-text-secondary truncate">
              {user.email || "Chưa có email"}
            </p>
          </div>
          <button
            type="button"
            onClick={handleLogout}
            disabled={isLoggingOut}
            className="flex items-center gap-2 h-9 px-4 rounded-md border border-red-300 dark:border-red-700 text-red-600 dark:text-red-400 text-sm font-medium hover:bg-red-50 dark:hover:bg-red-950/30 transition disabled:opacity-50 cursor-pointer"
          >
            <LogOut className="w-4 h-4" />
            <span>{isLoggingOut ? "Đang xuất..." : "Đăng xuất"}</span>
          </button>
        </div>
      </section>

      {/* Section 2: Cấu hình AI */}
      <section className="bg-bg-secondary rounded-lg border border-border p-6 mb-6">
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
      </section>

      {/* Section 3: Giao diện */}
      <section className="bg-bg-secondary rounded-lg border border-border p-6 mb-6">
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
      </section>

      {/* Section 4: Phím tắt */}
      <section className="bg-bg-secondary rounded-lg border border-border p-6 mb-6">
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
      </section>

      {/* Section 5: Thông tin ứng dụng */}
      <section className="bg-bg-secondary rounded-lg border border-border p-6 mb-6">
        <h2 className="text-base font-semibold text-text-primary flex items-center gap-2 mb-4">
          <Info className="w-4 h-4" />
          Thông tin ứng dụng
        </h2>
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-sm text-text-secondary">Ứng dụng</span>
            <span className="text-sm font-medium text-text-primary">Studxy (SWMS)</span>
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
      </section>
    </div>
  );
}
