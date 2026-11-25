"use client";

import React, { Suspense, useCallback, useEffect, useRef, useState } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import {
  LogIn,
  UserPlus,
  ShieldCheck,
  ArrowRight,
  AlertCircle,
  CheckCircle2,
  BookOpen,
  Eye,
  EyeOff,
  HelpCircle,
} from "lucide-react";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { setAccessToken } from "@/lib/api-client";

interface GoogleAccountsIdApi {
  initialize: (config: {
    client_id: string;
    callback: (response: { credential?: string }) => void;
  }) => void;
  renderButton: (
    parent: HTMLElement,
    options: {
      theme?: "outline" | "filled_blue" | "filled_black";
      size?: "large" | "medium" | "small";
      shape?: "rectangular" | "pill" | "circle" | "square";
      text?: "signin_with" | "signup_with" | "continue_with" | "signin";
      width?: number;
      locale?: string;
    }
  ) => void;
}

declare global {
  interface Window {
    google?: {
      accounts?: {
        id?: GoogleAccountsIdApi;
      };
    };
  }
}

interface AuthSessionUser {
  id: string;
  fullName: string;
  email: string;
  authProvider?: string;
  avatarUrl?: string | null;
  hasLocalPassword?: boolean;
}

function resolveSafeRedirect(raw: string | null): string {
  if (raw && raw.startsWith("/") && !raw.startsWith("//")) {
    return raw;
  }
  return "/app";
}

function GoogleGlyph({ className = "w-4 h-4" }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" aria-hidden="true">
      <path
        fill="#4285F4"
        d="M23.745 12.27c0-.7-.06-1.4-.19-2.07H12v4.51h6.6c-.29 1.52-1.14 2.82-2.4 3.68v3.05h3.88c2.27-2.09 3.665-5.17 3.665-9.17Z"
      />
      <path
        fill="#34A853"
        d="M12 24c3.3 0 6.08-1.09 8.11-2.96l-3.88-3.05c-1.08.72-2.45 1.16-4.23 1.16-3.26 0-6.02-2.2-7-5.16H1.01v3.14C3.03 21.14 7.21 24 12 24Z"
      />
      <path
        fill="#FBBC05"
        d="M5 13.99c-.25-.72-.39-1.49-.39-2.29s.14-1.57.39-2.29V6.27H1.01A11.98 11.98 0 0 0 0 11.7c0 1.94.46 3.77 1.01 5.43l3.99-3.14Z"
      />
      <path
        fill="#EA4335"
        d="M12 4.75c1.8 0 3.41.62 4.68 1.83l3.51-3.51C18.07 1.19 15.3 0 12 0 7.21 0 3.03 2.86 1.01 7.13l3.99 3.14c.98-2.96 3.74-5.52 7-5.52Z"
      />
    </svg>
  );
}

function AuthPageContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { setActiveUserHeaderId } = useWorkspaceStore();

  const initialTab = searchParams.get("tab") === "register" ? "register" : "login";
  const targetDestination = resolveSafeRedirect(searchParams.get("redirect"));

  const [mode, setMode] = useState<"login" | "register">(initialTab);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [fullName, setFullName] = useState("");
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  const rawEnvGoogleClientId = (process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID ?? "").trim();
  const initialEnvGoogleClientId =
    rawEnvGoogleClientId.length >= 10 &&
    !rawEnvGoogleClientId.toUpperCase().startsWith("YOUR_") &&
    !rawEnvGoogleClientId.startsWith("<")
      ? rawEnvGoogleClientId
      : null;

  const [googleClientId, setGoogleClientId] = useState<string | null>(
    initialEnvGoogleClientId
  );
  const [googleEnabled, setGoogleEnabled] = useState(
    Boolean(initialEnvGoogleClientId)
  );
  const [googleProvidersLoaded, setGoogleProvidersLoaded] = useState(false);
  const [googleSdkError, setGoogleSdkError] = useState<string | null>(null);
  const [showGoogleSetupGuide, setShowGoogleSetupGuide] = useState(false);

  const googleBtnRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const paramTab = searchParams.get("tab");
    if (paramTab === "register" || paramTab === "login") {
      setMode(paramTab);
    }
  }, [searchParams]);

  const saveSessionAndNavigate = useCallback(
    (userObj: AuthSessionUser) => {
      if (typeof window !== "undefined") {
        window.localStorage.setItem("swms_has_refresh_cookie", "1");
        window.localStorage.setItem(
          "swms_active_user",
          JSON.stringify({
            id: userObj.id,
            fullName: userObj.fullName,
            email: userObj.email,
            authProvider: userObj.authProvider ?? "local",
            avatarUrl: userObj.avatarUrl ?? null,
            hasLocalPassword: userObj.hasLocalPassword ?? true,
          })
        );
        if (
          userObj.id &&
          userObj.id !== "11111111-1111-4111-8111-111111111111"
        ) {
          window.localStorage.setItem("swms_active_user_id", userObj.id);
          setActiveUserHeaderId(userObj.id);
        } else {
          window.localStorage.removeItem("swms_active_user_id");
          setActiveUserHeaderId(null);
        }
      }
      router.push(targetDestination);
    },
    [router, setActiveUserHeaderId, targetDestination]
  );

  const applyAuthResponse = useCallback(
    (data: Record<string, unknown>, fallbackName: string, fallbackEmail: string) => {
      const user = (data?.user ?? {}) as Record<string, unknown>;
      const userId = String(user.id ?? user.Id ?? "");
      setAccessToken(data?.accessToken ? String(data.accessToken) : null);
      saveSessionAndNavigate({
        id: userId,
        fullName: String(user.fullName ?? user.FullName ?? fallbackName),
        email: String(user.email ?? user.Email ?? fallbackEmail),
        authProvider: String(user.authProvider ?? user.AuthProvider ?? "local"),
        avatarUrl: (user.avatarUrl ?? user.AvatarUrl ?? null) as string | null,
        hasLocalPassword: Boolean(user.hasLocalPassword ?? user.HasLocalPassword ?? true),
      });
    },
    [saveSessionAndNavigate]
  );

  const handleGoogleCredential = useCallback(
    async (credential: string) => {
      setErrorMessage(null);
      setStatusMessage("Đang xác thực tài khoản Google...");
      setLoading(true);
      try {
        const resp = await fetch("/api/v1/auth/google", {
          method: "POST",
          credentials: "include",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ credential }),
        });
        if (!resp.ok) {
          const errData = await resp.json().catch(() => null);
          setStatusMessage(null);
          setErrorMessage(
            errData?.error ?? "Xác thực Google không thành công. Vui lòng thử lại."
          );
          return;
        }
        const data = (await resp.json()) as Record<string, unknown>;
        applyAuthResponse(data, "Google User", "");
      } catch {
        setStatusMessage(null);
        setErrorMessage("Không thể kết nối máy chủ để xác thực Google.");
      } finally {
        setLoading(false);
      }
    },
    [applyAuthResponse]
  );

  const handleGoogleCredentialRef = useRef(handleGoogleCredential);
  handleGoogleCredentialRef.current = handleGoogleCredential;

  useEffect(() => {
    let active = true;
    fetch("/api/v1/auth/providers", { credentials: "include" })
      .then((r) => (r.ok ? r.json() : null))
      .then((data) => {
        if (!active || !data) return;
        const enabled = Boolean(data.googleEnabled);
        const clientId = data.googleClientId ? String(data.googleClientId) : null;
        setGoogleEnabled(enabled && Boolean(clientId));
        setGoogleClientId(enabled ? clientId : null);
      })
      .catch(() => {
        // Keep env fallback if network briefly fails
      })
      .finally(() => {
        if (active) setGoogleProvidersLoaded(true);
      });
    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    if (!googleEnabled || !googleClientId || !googleBtnRef.current) return;
    let cancelled = false;

    const initGsi = () => {
      if (cancelled || !googleBtnRef.current || !window.google?.accounts?.id) return;
      try {
        window.google.accounts.id.initialize({
          client_id: googleClientId,
          callback: (res) => {
            if (res?.credential) {
              void handleGoogleCredentialRef.current(res.credential);
            } else {
              setErrorMessage("Google không trả về token đăng nhập hợp lệ.");
            }
          },
        });
        googleBtnRef.current.innerHTML = "";
        window.google.accounts.id.renderButton(googleBtnRef.current, {
          theme: "outline",
          size: "large",
          shape: "rectangular",
          text: "continue_with",
          width: 360,
          locale: "vi",
        });
        setGoogleSdkError(null);
      } catch {
        setGoogleSdkError("Không thể khởi tạo nút Đăng nhập Google cho origin này.");
      }
    };

    if (window.google?.accounts?.id) {
      initGsi();
      return () => {
        cancelled = true;
      };
    }

    const existing = document.querySelector<HTMLScriptElement>(
      'script[src="https://accounts.google.com/gsi/client"]'
    );
    if (existing) {
      existing.addEventListener("load", initGsi);
      return () => {
        cancelled = true;
        existing.removeEventListener("load", initGsi);
      };
    }

    const script = document.createElement("script");
    script.src = "https://accounts.google.com/gsi/client";
    script.async = true;
    script.defer = true;
    script.onload = initGsi;
    script.onerror = () => {
      if (!cancelled) {
        setGoogleSdkError("Không thể tải Google Identity SDK (kiểm tra kết nối Internet).");
      }
    };
    document.head.appendChild(script);

    return () => {
      cancelled = true;
    };
  }, [googleEnabled, googleClientId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setStatusMessage(null);

    const trimmedEmail = email.trim();
    if (!trimmedEmail || !password) {
      setErrorMessage("Vui lòng nhập đầy đủ Email và Mật khẩu.");
      return;
    }

    if (mode === "register") {
      if (password.length < 6 || !/[A-Za-z]/.test(password) || !/\d/.test(password)) {
        setErrorMessage("Mật khẩu phải có tối thiểu 6 ký tự và bao gồm cả chữ cái và chữ số.");
        return;
      }
    }

    setLoading(true);
    try {
      const endpoint =
        mode === "login" ? "/api/v1/auth/login" : "/api/v1/auth/register";
      const payload =
        mode === "login"
          ? { email: trimmedEmail, password }
          : {
              email: trimmedEmail,
              password,
              fullName: fullName.trim() || "Học viên SWMS",
            };

      const resp = await fetch(endpoint, {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      if (!resp.ok) {
        const errData = await resp.json().catch(() => null);
        if (resp.status === 409) {
          setErrorMessage("Email này đã được đăng ký. Vui lòng chuyển sang tab Đăng nhập.");
        } else if (resp.status === 401) {
          setErrorMessage("Email hoặc mật khẩu không chính xác.");
        } else {
          setErrorMessage(
            errData?.error ?? "Không thể xác thực tài khoản. Vui lòng thử lại."
          );
        }
        return;
      }

      const data = (await resp.json()) as Record<string, unknown>;
      applyAuthResponse(data, fullName.trim() || "Học viên SWMS", trimmedEmail);
    } catch {
      setErrorMessage("Lỗi kết nối máy chủ xác thực. Vui lòng thử lại.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      data-testid="auth-page-container"
      className="min-h-screen w-full bg-[var(--canvas-bg)] text-[var(--text-primary)] flex flex-col justify-between p-4 sm:p-6 lg:p-8"
    >
      {/* Top Bar */}
      <header className="max-w-5xl w-full mx-auto flex items-center justify-between gap-3">
        <Link
          href="/"
          className="inline-flex items-center gap-2.5 font-bold text-[#5E51E8] dark:text-[#818CF8] hover:opacity-90 transition"
        >
          <span className="inline-flex items-center justify-center w-8 h-8 rounded-[8px] bg-[#5E51E8] dark:bg-[#818CF8] text-white dark:text-[#0F0F12] text-xs font-bold shadow-xs">
            S
          </span>
          <span className="text-sm font-bold tracking-tight text-[var(--text-primary)]">
            Studxy<span className="hidden sm:inline"> · Không gian học tập thông minh</span>
          </span>
        </Link>

        <div className="flex items-center gap-2">
          <Link
            href="/app"
            data-testid="auth-go-app-v2"
            className="text-xs font-semibold text-white dark:text-[#0F0F12] bg-[#5E51E8] hover:bg-[#4F46E5] dark:bg-[#818CF8] dark:hover:bg-[#6366F1] inline-flex items-center gap-1.5 px-3.5 py-2 rounded-[10px] transition shadow-xs"
          >
            <BookOpen className="w-3.5 h-3.5" />
            <span>Vào không gian học</span>
          </Link>
          <Link
            href="/workspace"
            className="hidden sm:inline-flex text-xs font-medium text-[var(--text-secondary)] hover:text-[#5E51E8] dark:hover:text-[#818CF8] items-center gap-1.5 px-3 py-2 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] transition"
          >
            <span>Workspace Classic</span>
          </Link>
        </div>
      </header>

      {/* Main Centered Auth Content */}
      <main className="max-w-lg w-full mx-auto my-8 space-y-4">
        <section className="rounded-[16px] bg-[var(--surface-card)] border border-[var(--border-calm)] shadow-elevation-1 p-6 sm:p-8">
          <div className="mb-6">
            <div className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-[6px] bg-[#EEF0FD] dark:bg-[#1E1B4B] text-[#4F46E5] dark:text-[#A5B4FC] border border-[#5E51E8]/20 text-[11px] font-semibold mb-2">
              <ShieldCheck className="w-3.5 h-3.5" />
              <span>Không gian học tập cá nhân độc lập</span>
            </div>
            <h1 className="text-xl sm:text-2xl font-bold tracking-tight text-[var(--text-primary)]">
              {mode === "login"
                ? "Đăng nhập vào Studxy"
                : "Tạo tài khoản Studxy mới"}
            </h1>
            <p className="text-xs text-[var(--text-secondary)] mt-1">
              Mỗi tài khoản sở hữu thư viện PDF, sổ tay ghi chú, khóa học và lịch ôn tập FSRS v5 riêng biệt.
            </p>
          </div>

          {/* Mode Tabs */}
          <div className="grid grid-cols-2 gap-1.5 p-1 rounded-[10px] bg-[var(--surface-elevated)] border border-[var(--border-calm)] mb-6">
            <button
              type="button"
              data-testid="auth-tab-login"
              onClick={() => {
                setMode("login");
                setErrorMessage(null);
              }}
              className={`min-h-[40px] py-2 px-3 rounded-[8px] text-xs font-bold flex items-center justify-center gap-1.5 transition cursor-pointer ${
                mode === "login"
                  ? "bg-[#5E51E8] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] shadow-xs"
                  : "text-[var(--text-secondary)] hover:text-[var(--text-primary)]"
              }`}
            >
              <LogIn className="w-3.5 h-3.5" />
              <span>Đăng nhập</span>
            </button>
            <button
              type="button"
              data-testid="auth-tab-register"
              onClick={() => {
                setMode("register");
                setErrorMessage(null);
              }}
              className={`min-h-[40px] py-2 px-3 rounded-[8px] text-xs font-bold flex items-center justify-center gap-1.5 transition cursor-pointer ${
                mode === "register"
                  ? "bg-[#5E51E8] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] shadow-xs"
                  : "text-[var(--text-secondary)] hover:text-[var(--text-primary)]"
              }`}
            >
              <UserPlus className="w-3.5 h-3.5" />
              <span>Đăng ký tài khoản</span>
            </button>
          </div>

          {errorMessage && (
            <div
              role="alert"
              className="mb-4 p-3 rounded-[10px] bg-rose-50 dark:bg-rose-950/50 border border-rose-200 dark:border-rose-800/70 text-rose-700 dark:text-rose-300 text-xs flex items-center gap-2 font-medium"
            >
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>{errorMessage}</span>
            </div>
          )}

          {statusMessage && (
            <div className="mb-4 p-3 rounded-[10px] bg-emerald-50 dark:bg-emerald-950/50 border border-emerald-200 dark:border-emerald-800/70 text-emerald-700 dark:text-emerald-300 text-xs flex items-center gap-2 font-medium">
              <CheckCircle2 className="w-4 h-4 shrink-0" />
              <span>{statusMessage}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            {mode === "register" && (
              <div>
                <label
                  htmlFor="auth-fullname"
                  className="block text-xs font-semibold text-[var(--text-primary)] mb-1.5"
                >
                  Họ và tên hiển thị
                </label>
                <input
                  id="auth-fullname"
                  type="text"
                  data-testid="auth-fullname-input"
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  placeholder="Ví dụ: Nguyễn Minh Anh"
                  className="w-full px-3.5 py-2.5 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs text-[var(--text-primary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8]"
                />
              </div>
            )}

            <div>
              <label
                htmlFor="auth-email"
                className="block text-xs font-semibold text-[var(--text-primary)] mb-1.5"
              >
                Địa chỉ Email
              </label>
              <input
                id="auth-email"
                type="email"
                data-testid="auth-email-input"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="ten.ban@studxy.vn"
                required
                className="w-full px-3.5 py-2.5 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs text-[var(--text-primary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8]"
              />
            </div>

            <div>
              <div className="flex items-center justify-between mb-1.5">
                <label
                  htmlFor="auth-password"
                  className="block text-xs font-semibold text-[var(--text-primary)]"
                >
                  Mật khẩu bảo mật
                </label>
                {mode === "register" && (
                  <span className="text-[11px] text-[var(--text-tertiary)]">Tối thiểu 6 ký tự (gồm chữ và số)</span>
                )}
              </div>
              <div className="relative">
                <input
                  id="auth-password"
                  type={showPassword ? "text" : "password"}
                  data-testid="auth-password-input"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••••••"
                  required
                  minLength={6}
                  className="w-full px-3.5 py-2.5 pr-11 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs text-[var(--text-primary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8]"
                />
                <button
                  type="button"
                  data-testid="auth-toggle-password-btn"
                  onClick={() => setShowPassword((prev) => !prev)}
                  aria-label={showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                  className="absolute right-1 top-1/2 -translate-y-1/2 min-w-[36px] min-h-[36px] flex items-center justify-center p-1.5 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] transition cursor-pointer"
                >
                  {showPassword ? (
                    <EyeOff className="w-4 h-4" />
                  ) : (
                    <Eye className="w-4 h-4" />
                  )}
                </button>
              </div>
            </div>

            <button
              type="submit"
              data-testid="auth-submit-btn"
              disabled={loading}
              className="w-full min-h-[44px] py-2.5 px-4 rounded-[10px] bg-[#5E51E8] hover:bg-[#4F46E5] dark:bg-[#818CF8] dark:text-[#0F0F12] disabled:opacity-60 text-white text-xs font-bold shadow-xs transition flex items-center justify-center gap-2 cursor-pointer"
            >
              <span>
                {loading
                  ? "Đang xử lý..."
                  : mode === "login"
                  ? "Đăng nhập vào Studxy"
                  : "Tạo tài khoản & Khám phá"}
              </span>
              <ArrowRight className="w-4 h-4" />
            </button>
          </form>

          {/* Divider + Google Sign-In Section */}
          <div data-testid="google-signin-section" className="mt-6">
            <div className="relative flex items-center justify-center mb-4">
              <div className="border-t border-[var(--border-calm)] w-full" />
              <span className="bg-[var(--surface-card)] px-3 text-[11px] font-medium text-[var(--text-tertiary)] whitespace-nowrap">
                Hoặc tiếp tục với
              </span>
              <div className="border-t border-[var(--border-calm)] w-full" />
            </div>

            {googleEnabled && googleClientId ? (
              <div className="flex flex-col items-center gap-2">
                <div
                  ref={googleBtnRef}
                  data-testid="google-signin-button-container"
                  className="min-h-[42px] flex items-center justify-center w-full overflow-hidden"
                />
                {googleSdkError && (
                  <p className="text-[11px] text-amber-600 dark:text-amber-400 text-center">
                    {googleSdkError}
                  </p>
                )}
              </div>
            ) : (
              <div className="space-y-2.5">
                <button
                  type="button"
                  data-testid="google-signin-unconfigured-btn"
                  onClick={() => setShowGoogleSetupGuide((v) => !v)}
                  className="w-full min-h-[42px] px-4 py-2 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] hover:border-[#5E51E8] text-[var(--text-primary)] text-xs font-semibold flex items-center justify-between gap-2 transition cursor-pointer"
                >
                  <span className="inline-flex items-center gap-2">
                    <GoogleGlyph className="w-4 h-4 opacity-80" />
                    <span>
                      {googleProvidersLoaded
                        ? "Tiếp tục với Google"
                        : "Đang kiểm tra Google Sign-In..."}
                    </span>
                    {googleProvidersLoaded && (
                      <span className="px-1.5 py-0.5 rounded text-[10px] font-medium bg-zinc-100 dark:bg-zinc-800 text-zinc-600 dark:text-zinc-400 border border-zinc-200 dark:border-zinc-700">
                        Chưa cấu hình
                      </span>
                    )}
                  </span>
                  <span className="inline-flex items-center gap-1 text-[11px] text-[#5E51E8] dark:text-[#818CF8]">
                    <HelpCircle className="w-3.5 h-3.5" />
                    <span>Hướng dẫn</span>
                  </span>
                </button>

                {showGoogleSetupGuide && (
                  <div
                    data-testid="google-setup-guide"
                    className="p-3.5 rounded-[10px] bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 text-[11px] text-[var(--text-primary)] space-y-1.5 leading-relaxed"
                  >
                    <p className="font-bold text-[#4F46E5] dark:text-[#A5B4FC]">
                      Cách bật Google Sign-In thật trong 2 phút:
                    </p>
                    <ol className="list-decimal list-inside space-y-1 text-[var(--text-secondary)]">
                      <li>
                        Mở <strong>Google Cloud Console</strong> → <em>APIs &amp; Services → Credentials → Create OAuth client ID (Web application)</em>.
                      </li>
                      <li>
                        Thêm <strong>Authorized JavaScript origins</strong>: <code className="px-1 py-0.5 rounded bg-[var(--surface-card)] font-mono">https://swms-gamma.vercel.app</code>, <code className="px-1 py-0.5 rounded bg-[var(--surface-card)] font-mono">http://localhost:3000</code> và <code className="px-1 py-0.5 rounded bg-[var(--surface-card)] font-mono">http://127.0.0.1:3000</code>.
                      </li>
                      <li>
                        Cấu hình biến môi trường <code className="px-1 py-0.5 rounded bg-[var(--surface-card)] font-mono">GOOGLE_CLIENT_ID=...</code> hoặc <code className="px-1 py-0.5 rounded bg-[var(--surface-card)] font-mono">Authentication:Google:ClientId</code> trong <code className="font-mono">appsettings.json</code> rồi khởi động lại Backend.
                      </li>
                    </ol>
                  </div>
                )}
              </div>
            )}
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="max-w-5xl w-full mx-auto text-center text-[11px] text-[var(--text-tertiary)]">
        Studxy · Không gian đọc bài, ghi chú và ôn thẻ nhớ lâu
      </footer>
    </div>
  );
}

export default function AuthPage() {
  return (
    <Suspense
      fallback={
        <div className="min-h-screen w-full bg-[var(--canvas-bg)] flex items-center justify-center">
          <div className="w-6 h-6 border-2 border-[#5E51E8] border-t-transparent rounded-full animate-spin" />
        </div>
      }
    >
      <AuthPageContent />
    </Suspense>
  );
}
