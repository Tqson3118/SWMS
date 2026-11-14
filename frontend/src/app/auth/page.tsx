"use client";

import React, { Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import {
  LogIn,
  UserPlus,
  Sparkles,
  ShieldCheck,
  Users,
  ArrowRight,
  AlertCircle,
  CheckCircle2,
  BookOpen,
} from "lucide-react";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";

function resolveSafeRedirect(raw: string | null): string {
  if (raw && raw.startsWith("/") && !raw.startsWith("//")) {
    return raw;
  }
  return "/app";
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
  const [fullName, setFullName] = useState("");
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  useEffect(() => {
    const paramTab = searchParams.get("tab");
    if (paramTab === "register" || paramTab === "login") {
      setMode(paramTab);
    }
  }, [searchParams]);

  const saveSessionAndNavigate = (userObj: {
    id: string;
    fullName: string;
    email: string;
    accessToken?: string;
  }) => {
    if (typeof window !== "undefined") {
      window.localStorage.setItem("swms_active_user", JSON.stringify(userObj));
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
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setStatusMessage(null);

    const trimmedEmail = email.trim();
    if (!trimmedEmail || !password) {
      setErrorMessage("Vui lòng nhập đầy đủ email và mật khẩu.");
      return;
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
              fullName: fullName.trim() || "Học viên SWMS"
            };

      const resp = await fetch(endpoint, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      });

      if (!resp.ok) {
        const errData = await resp.json().catch(() => null);
        if (resp.status === 409) {
          setErrorMessage(
            errData?.error ?? "Email này đã được đăng ký trong hệ thống."
          );
        } else if (resp.status === 401) {
          setErrorMessage("Email hoặc mật khẩu không chính xác.");
        } else {
          setErrorMessage(
            errData?.error ?? "Không thể xác thực tài khoản. Vui lòng thử lại."
          );
        }
        return;
      }

      const data = await resp.json();
      const user = data?.user ?? {};
      const userId = String(user.id ?? user.Id ?? "");
      saveSessionAndNavigate({
        id: userId,
        fullName: String(user.fullName ?? user.FullName ?? (fullName.trim() || "Học viên SWMS")),
        email: String(user.email ?? user.Email ?? trimmedEmail),
        accessToken: String(data?.accessToken ?? "")
      });
    } catch {
      setErrorMessage("Lỗi kết nối máy chủ xác thực. Vui lòng thử lại.");
    } finally {
      setLoading(false);
    }
  };

  const handleSwitchDefaultUser = () => {
    setErrorMessage(null);
    setStatusMessage("Đang chuyển về tài khoản Sơn Trần (Mặc định)...");
    if (typeof window !== "undefined") {
      window.localStorage.removeItem("swms_active_user_id");
      window.localStorage.setItem(
        "swms_active_user",
        JSON.stringify({
          id: "11111111-1111-4111-8111-111111111111",
          fullName: "Sơn Trần",
          email: "son.tran@studxy.vn"
        })
      );
    }
    setActiveUserHeaderId(null);
    router.push(targetDestination);
  };

  const handleSwitchSecondaryUser = async () => {
    setErrorMessage(null);
    setStatusMessage("Đang khởi tạo không gian độc lập cho Minh Anh...");
    setLoading(true);

    const demoEmail = "minh.anh@studxy.vn";
    const demoPassword = "MinhAnhPassword123!";
    const demoName = "Minh Anh";

    try {
      let resp = await fetch("/api/v1/auth/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          email: demoEmail,
          password: demoPassword,
          fullName: demoName
        })
      });

      if (resp.status === 409) {
        resp = await fetch("/api/v1/auth/login", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            email: demoEmail,
            password: demoPassword
          })
        });
      }

      if (!resp.ok) {
        setErrorMessage("Không thể chuyển sang tài khoản Minh Anh.");
        return;
      }

      const data = await resp.json();
      const user = data?.user ?? {};
      const userId = String(user.id ?? user.Id ?? "");
      if (typeof window !== "undefined" && userId) {
        window.localStorage.setItem("swms_active_user_id", userId);
        window.localStorage.setItem(
          "swms_active_user",
          JSON.stringify({
            id: userId,
            fullName: String(user.fullName ?? user.FullName ?? demoName),
            email: String(user.email ?? user.Email ?? demoEmail),
            accessToken: String(data?.accessToken ?? "")
          })
        );
      }
      setActiveUserHeaderId(userId || null);
      router.push(targetDestination);
    } catch {
      setErrorMessage("Không thể kết nối tới máy chủ để chuyển tài khoản.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      data-testid="auth-page-container"
      className="min-h-screen w-full bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col justify-between p-4 sm:p-6 lg:p-8"
    >
      {/* Top Bar */}
      <header className="max-w-5xl w-full mx-auto flex items-center justify-between gap-3">
        <Link
          href="/"
          className="inline-flex items-center gap-2.5 font-bold text-indigo-600 dark:text-indigo-400 hover:opacity-90 transition"
        >
          <span className="inline-flex items-center justify-center w-8 h-8 rounded-xl bg-gradient-to-br from-indigo-500 to-violet-600 text-white text-xs font-extrabold shadow-xs">
            S
          </span>
          <span className="text-sm font-extrabold tracking-tight text-slate-900 dark:text-white">
            Studxy · Không gian học tập thông minh
          </span>
        </Link>

        <div className="flex items-center gap-2">
          <Link
            href="/app"
            data-testid="auth-go-app-v2"
            className="text-xs font-semibold text-white bg-indigo-600 hover:bg-indigo-700 inline-flex items-center gap-1.5 px-3 py-2 rounded-xl transition shadow-xs"
          >
            <BookOpen className="w-3.5 h-3.5" />
            <span>Vào không gian học</span>
          </Link>
          <Link
            href="/workspace"
            className="hidden sm:inline-flex text-xs font-medium text-slate-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 items-center gap-1.5 px-3 py-2 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 transition"
          >
            <span>Workspace Classic</span>
          </Link>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-4xl w-full mx-auto my-8 grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Left Column: Login / Register Form */}
        <section className="lg:col-span-7 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm p-6 sm:p-8">
          <div className="mb-6">
            <div className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300 text-[11px] font-bold mb-2">
              <ShieldCheck className="w-3.5 h-3.5" />
              <span>Không gian học tập cá nhân độc lập</span>
            </div>
            <h1 className="text-xl sm:text-2xl font-bold tracking-tight text-slate-900 dark:text-white">
              {mode === "login"
                ? "Đăng nhập vào Studxy"
                : "Tạo tài khoản Studxy mới"}
            </h1>
            <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">
              Mỗi tài khoản sở hữu thư viện PDF, sổ tay ghi chú, khóa học và lịch ôn tập FSRS v5 riêng biệt.
            </p>
          </div>

            {/* Mode Tabs */}
            <div className="grid grid-cols-2 gap-1.5 p-1 rounded-xl bg-slate-100 dark:bg-slate-950 border border-slate-200/80 dark:border-slate-800 mb-6">
              <button
                type="button"
                data-testid="auth-tab-login"
                onClick={() => {
                  setMode("login");
                  setErrorMessage(null);
                }}
                className={`min-h-[40px] py-2 px-3 rounded-lg text-xs font-bold flex items-center justify-center gap-1.5 transition cursor-pointer ${
                  mode === "login"
                    ? "bg-indigo-600 text-white shadow-xs"
                    : "text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white"
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
                className={`min-h-[40px] py-2 px-3 rounded-lg text-xs font-bold flex items-center justify-center gap-1.5 transition cursor-pointer ${
                  mode === "register"
                    ? "bg-indigo-600 text-white shadow-xs"
                    : "text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white"
                }`}
              >
                <UserPlus className="w-3.5 h-3.5" />
                <span>Đăng ký tài khoản</span>
              </button>
            </div>

            {errorMessage && (
              <div
                role="alert"
                className="mb-4 p-3 rounded-xl bg-rose-50 dark:bg-rose-950/50 border border-rose-200 dark:border-rose-800/70 text-rose-700 dark:text-rose-300 text-xs flex items-center gap-2 font-medium"
              >
                <AlertCircle className="w-4 h-4 shrink-0" />
                <span>{errorMessage}</span>
              </div>
            )}

            {statusMessage && (
              <div className="mb-4 p-3 rounded-xl bg-emerald-50 dark:bg-emerald-950/50 border border-emerald-200 dark:border-emerald-800/70 text-emerald-700 dark:text-emerald-300 text-xs flex items-center gap-2 font-medium">
                <CheckCircle2 className="w-4 h-4 shrink-0" />
                <span>{statusMessage}</span>
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-4">
              {mode === "register" && (
                <div>
                  <label
                    htmlFor="auth-fullname"
                    className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1.5"
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
                    className="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-xs text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                </div>
              )}

              <div>
                <label
                  htmlFor="auth-email"
                  className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1.5"
                >
                  Địa chỉ Email học thuật
                </label>
                <input
                  id="auth-email"
                  type="email"
                  data-testid="auth-email-input"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="son.tran@studxy.vn"
                  required
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-xs text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              <div>
                <label
                  htmlFor="auth-password"
                  className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1.5"
                >
                  Mật khẩu bảo mật
                </label>
                <input
                  id="auth-password"
                  type="password"
                  data-testid="auth-password-input"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••••••"
                  required
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-xs text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              <button
                type="submit"
                data-testid="auth-submit-btn"
                disabled={loading}
                className="w-full min-h-[44px] py-2.5 px-4 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white text-xs font-bold shadow-xs transition flex items-center justify-center gap-2 cursor-pointer"
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
          </section>

          {/* Right Column: Multi-User Demo Switcher */}
          <aside
            data-testid="demo-user-switcher"
            className="lg:col-span-5 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm p-6 space-y-4"
          >
            <div className="flex items-center gap-2.5">
              <div className="w-9 h-9 rounded-xl bg-amber-500/10 text-amber-600 dark:text-amber-400 flex items-center justify-center shrink-0 border border-amber-500/20">
                <Users className="w-4 h-4" />
              </div>
              <div>
                <h2 className="text-sm font-bold text-slate-900 dark:text-white">
                  Chuyển nhanh tài khoản Demo
                </h2>
                <p className="text-[11px] text-slate-500 dark:text-slate-400">
                  Trải nghiệm dữ liệu học tập tách biệt 1-Click
                </p>
              </div>
            </div>

            <div className="space-y-3">
              <button
                type="button"
                data-testid="switch-user-default"
                onClick={handleSwitchDefaultUser}
                disabled={loading}
                className="w-full text-left p-3.5 rounded-xl border border-slate-200 dark:border-slate-800 hover:border-indigo-500 dark:hover:border-indigo-500 bg-slate-50/70 dark:bg-slate-950/60 hover:bg-indigo-50/30 dark:hover:bg-indigo-950/30 transition group cursor-pointer"
              >
                <div className="flex items-center justify-between mb-1">
                  <span className="text-xs font-bold text-slate-900 dark:text-white group-hover:text-indigo-600 dark:group-hover:text-indigo-400">
                    Sơn Trần (Tài khoản Mặc định - Đầy đủ dữ liệu)
                  </span>
                  <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-indigo-100 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300">
                    Mặc định
                  </span>
                </div>
                <p className="text-[11px] text-slate-500 dark:text-slate-400">
                  son.tran@studxy.vn • Sẵn sàng khóa học OSTEP, bài học Bộ nhớ ảo, thẻ FSRS và ghi chú mẫu.
                </p>
              </button>

              <button
                type="button"
                data-testid="switch-user-secondary"
                onClick={handleSwitchSecondaryUser}
                disabled={loading}
                className="w-full text-left p-3.5 rounded-xl border border-slate-200 dark:border-slate-800 hover:border-emerald-500 dark:hover:border-emerald-500 bg-slate-50/70 dark:bg-slate-950/60 hover:bg-emerald-50/30 dark:hover:bg-emerald-950/30 transition group cursor-pointer"
              >
                <div className="flex items-center justify-between mb-1">
                  <span className="text-xs font-bold text-slate-900 dark:text-white group-hover:text-emerald-600 dark:group-hover:text-emerald-400">
                    Minh Anh (Tài khoản Độc lập - Không gian mới)
                  </span>
                  <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-100 dark:bg-emerald-950 text-emerald-700 dark:text-emerald-300">
                    Độc lập
                  </span>
                </div>
                <p className="text-[11px] text-slate-500 dark:text-slate-400">
                  minh.anh@studxy.vn • Tự động tạo không gian riêng biệt, độc lập thư viện PDF và thẻ ôn tập.
                </p>
              </button>
            </div>

            <div className="p-3 rounded-xl bg-slate-100/80 dark:bg-slate-950 border border-slate-200/60 dark:border-slate-800 text-[11px] text-slate-600 dark:text-slate-400 flex items-start gap-2">
              <Sparkles className="w-4 h-4 text-indigo-500 shrink-0 mt-0.5" />
              <span>
                Mẹo: Bạn có thể chuyển đổi qua lại giữa <strong>Sơn Trần</strong> và <strong>Minh Anh</strong> bất kỳ lúc nào từ Sidebar hoặc menu Cài đặt.
              </span>
            </div>
          </aside>
        </main>

        {/* Footer */}
        <footer className="max-w-5xl w-full mx-auto text-center text-[11px] text-slate-400 dark:text-slate-600">
          Studxy · Nền tảng đọc sâu, ghi chú và ghi nhớ lâu dài với FSRS v5
        </footer>
      </div>
  );
}

export default function AuthPage() {
  return (
    <Suspense
      fallback={
        <div className="min-h-screen w-full bg-slate-50 dark:bg-slate-950 flex items-center justify-center">
          <div className="w-6 h-6 border-2 border-indigo-600 border-t-transparent rounded-full animate-spin" />
        </div>
      }
    >
      <AuthPageContent />
    </Suspense>
  );
}
