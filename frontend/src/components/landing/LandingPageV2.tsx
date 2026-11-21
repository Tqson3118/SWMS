"use client";

import React, { useEffect, useRef, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  ArrowRight,
  BookOpen,
  Brain,
  ChevronRight,
  Command,
  Keyboard,
  Layers,
  Moon,
  Sparkles,
  Sun,
  X,
  Zap,
} from "lucide-react";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { bootstrapAuthSession } from "@/lib/api-client";

/* =============================================================
   Landing Page V2 — Modern SaaS / Clean & Minimal
   (Linear / Arc Browser aesthetic)
   ============================================================= */

function useScrollY() {
  const [y, setY] = useState(0);
  useEffect(() => {
    const handler = () => setY(window.scrollY);
    window.addEventListener("scroll", handler, { passive: true });
    return () => window.removeEventListener("scroll", handler);
  }, []);
  return y;
}

/* ── Header ───────────────────────────────────────────────────── */
function Header({ dark, toggleDark }: { dark: boolean; toggleDark: () => void }) {
  const scrollY = useScrollY();
  const isScrolled = scrollY > 20;
  const [signedInUser, setSignedInUser] = useState<{ fullName: string } | null>(null);

  useEffect(() => {
    let active = true;
    try {
      if (window.localStorage.getItem("swms_has_refresh_cookie") !== "1") {
        return;
      }
    } catch {
      return;
    }

    bootstrapAuthSession().then((ok) => {
      if (!active || !ok) return;
      try {
        const raw = window.localStorage.getItem("swms_active_user");
        if (raw) {
          const parsed = JSON.parse(raw) as { fullName?: string };
          setSignedInUser({ fullName: parsed.fullName || "Học viên" });
          return;
        }
      } catch {
        // ignore JSON parse error
      }
      setSignedInUser({ fullName: "Học viên" });
    });

    return () => {
      active = false;
    };
  }, []);

  return (
    <header
      data-testid="landing-header"
      className={`fixed top-0 inset-x-0 z-50 transition-all duration-300 ${
        isScrolled
          ? "bg-[var(--surface-card)]/90 backdrop-blur-lg border-b border-[var(--border-calm)] shadow-xs"
          : "bg-transparent"
      }`}
    >
      <div className="max-w-6xl mx-auto flex items-center justify-between h-14 px-6">
        {/* Brand */}
        <Link href="/" className="flex items-center gap-2 group">
          <div className="w-7 h-7 rounded-[8px] bg-[#5E51E8] dark:bg-[#818CF8] flex items-center justify-center text-white dark:text-[#0F0F12] text-xs font-bold shadow-xs">
            S
          </div>
          <span className="text-base font-bold text-[var(--text-primary)] tracking-tight">
            Studxy
          </span>
        </Link>

        {/* Right actions */}
        <div className="flex items-center gap-2">
          <button
            type="button"
            data-testid="theme-toggle-btn"
            onClick={toggleDark}
            className="p-2 rounded-[8px] hover:bg-[var(--surface-elevated)] transition text-[var(--text-secondary)] cursor-pointer"
            aria-label="Toggle theme"
          >
            {dark ? <Sun className="w-4 h-4" /> : <Moon className="w-4 h-4" />}
          </button>
          {signedInUser ? (
            <Link
              href="/app"
              data-testid="landing-nav-cta"
              className="text-sm font-medium bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] px-4 py-1.5 rounded-[10px] transition shadow-xs inline-flex items-center gap-1.5"
            >
              <span>Vào Studxy ({signedInUser.fullName})</span>
              <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          ) : (
            <>
              <Link
                href="/auth"
                data-testid="landing-login-link"
                className="text-sm font-medium text-[var(--text-secondary)] hover:text-[var(--text-primary)] transition px-3 py-1.5"
              >
                Đăng nhập
              </Link>
              <Link
                href="/auth?tab=register"
                data-testid="landing-nav-cta"
                className="text-sm font-medium bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] px-4 py-1.5 rounded-[10px] transition shadow-xs"
              >
                Bắt đầu miễn phí
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}

/* ── Hero ─────────────────────────────────────────────────────── */
function HeroSection() {
  return (
    <section data-testid="landing-hero-section" className="relative pt-32 pb-20 px-6">
      {/* Subtle gradient background */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[600px] bg-gradient-to-b from-[#5E51E8]/10 via-[#818CF8]/5 to-transparent rounded-full blur-3xl" />
      </div>

      <div className="relative max-w-3xl mx-auto text-center">
        {/* Badge */}
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-[#EEF0FD] dark:bg-[#1E1B4B] border border-[#5E51E8]/20 text-[#4F46E5] dark:text-[#A5B4FC] text-xs font-semibold mb-6">
          <Sparkles className="w-3.5 h-3.5" />
          Nền tảng học tập thế hệ mới
        </div>

        <h1 className="text-4xl sm:text-5xl md:text-6xl font-bold text-[var(--text-primary)] leading-[1.1] tracking-tight mb-5">
          Đọc sâu.{" "}
          <span className="text-[#5E51E8] dark:text-[#818CF8]">
            Ghi nhớ.
          </span>{" "}
          Làm chủ.
        </h1>

        <p className="text-lg text-[var(--text-secondary)] max-w-xl mx-auto mb-8 leading-relaxed">
          Đọc tài liệu, viết sổ tay và ôn thẻ nhớ lâu trong một nơi duy nhất. Biến mọi tài liệu thành kiến thức của riêng bạn.
        </p>

        <div className="flex flex-col sm:flex-row items-center justify-center gap-3">
          <Link
            href="/auth?tab=register"
            data-testid="landing-hero-cta"
            className="inline-flex items-center gap-2 h-11 px-6 rounded-[12px] bg-[#5E51E8] hover:bg-[#4F46E5] dark:bg-[#818CF8] dark:text-[#0F0F12] text-white text-sm font-semibold transition shadow-elevation-1"
          >
            Bắt đầu miễn phí
            <ArrowRight className="w-4 h-4" />
          </Link>
          <Link
            href="#features"
            className="inline-flex items-center gap-2 h-11 px-6 rounded-[12px] border border-[var(--border-calm)] text-sm font-medium text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition"
          >
            Khám phá tính năng
          </Link>
        </div>

        {/* Keyboard hint */}
        <div className="mt-8 flex items-center justify-center gap-2 text-xs text-[var(--text-tertiary)]">
          <Keyboard className="w-3.5 h-3.5" />
          <span>Nhấn <kbd className="px-1.5 py-0.5 rounded bg-[var(--surface-elevated)] text-[var(--text-secondary)] font-mono text-[10px] border border-[var(--border-calm)]">?</kbd> để xem phím tắt</span>
        </div>
      </div>
    </section>
  );
}

/* ── Visual Preview ───────────────────────────────────────────── */
function PreviewSection() {
  return (
    <section className="px-6 pb-24">
      <div className="max-w-5xl mx-auto">
        <div className="rounded-2xl border border-neutral-200 dark:border-neutral-800 bg-white dark:bg-neutral-900 shadow-2xl shadow-neutral-200/50 dark:shadow-black/30 overflow-hidden">
          {/* Window chrome */}
          <div className="flex items-center gap-2 px-4 py-3 border-b border-neutral-100 dark:border-neutral-800">
            <div className="flex gap-1.5">
              <div className="w-3 h-3 rounded-full bg-red-400" />
              <div className="w-3 h-3 rounded-full bg-amber-400" />
              <div className="w-3 h-3 rounded-full bg-green-400" />
            </div>
            <div className="flex-1 flex justify-center">
              <div className="px-4 py-1 rounded-md bg-neutral-50 dark:bg-neutral-800 text-xs text-neutral-400 dark:text-neutral-500 font-mono">
                studxy.vn/app
              </div>
            </div>
          </div>

          {/* Simulated workspace layout */}
          <div className="flex h-80 sm:h-96">
            {/* Sidebar mock */}
            <div className="w-48 shrink-0 border-r border-neutral-100 dark:border-neutral-800 p-3 hidden sm:block">
              <div className="space-y-1">
                {["Tổng quan", "Khóa học", "Tài liệu", "Sổ tay", "Ôn tập"].map((item, i) => (
                  <div
                    key={item}
                    className={`flex items-center gap-2 px-3 py-2 rounded-md text-xs font-medium ${
                      i === 0
                        ? "bg-neutral-100 dark:bg-neutral-800 text-neutral-900 dark:text-white"
                        : "text-neutral-500 dark:text-neutral-400"
                    }`}
                  >
                    <div className={`w-4 h-4 rounded ${i === 0 ? "bg-indigo-500" : "bg-neutral-300 dark:bg-neutral-600"}`} />
                    {item}
                  </div>
                ))}
              </div>
              <div className="mt-6 space-y-2">
                <div className="text-[10px] font-semibold text-neutral-400 dark:text-neutral-500 px-3 uppercase tracking-wider">
                  Gần đây
                </div>
                {["IELTS 7.5+ Mastery", ".NET 10 & React", "Tài Chính Cá Nhân"].map((c) => (
                  <div key={c} className="px-3 py-1.5 text-xs text-neutral-600 dark:text-neutral-300 truncate">
                    {c}
                  </div>
                ))}
              </div>
            </div>

            {/* Main content mock */}
            <div className="flex-1 p-5 sm:p-6 bg-neutral-50/40 dark:bg-neutral-900/30">
              <div className="flex items-center justify-between gap-3 mb-4">
                <div>
                  <div className="text-sm font-bold text-neutral-900 dark:text-white">
                    Chào Sơn, hôm nay học gì?
                  </div>
                  <div className="text-[11px] text-neutral-500 dark:text-neutral-400">
                    Mục tiêu tuần này: 4/5 ngày · Chuỗi 7 ngày liên tiếp
                  </div>
                </div>
                <span className="hidden sm:inline-flex items-center px-2.5 py-1 rounded-full text-[11px] font-semibold bg-indigo-50 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-300 border border-indigo-200/70 dark:border-indigo-800/50">
                  Sẵn sàng ôn tập
                </span>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div className="rounded-lg bg-gradient-to-br from-indigo-50 to-violet-50 dark:from-indigo-950/40 dark:to-violet-950/30 border border-indigo-100 dark:border-indigo-900/40 p-3.5 sm:p-4">
                  <div className="flex items-center justify-between text-[11px] font-semibold text-indigo-700 dark:text-indigo-300 mb-1">
                    <span>Đang học dở</span>
                    <span>68%</span>
                  </div>
                  <div className="text-xs sm:text-sm font-bold text-neutral-900 dark:text-white truncate">
                    .NET 10 & React FullStack
                  </div>
                  <div className="mt-2 h-1.5 w-full rounded-full bg-indigo-100 dark:bg-indigo-900/50 overflow-hidden">
                    <div className="h-full w-2/3 rounded-full bg-indigo-600" />
                  </div>
                </div>
                <div className="rounded-lg bg-gradient-to-br from-emerald-50 to-teal-50 dark:from-emerald-950/40 dark:to-teal-950/30 border border-emerald-100 dark:border-emerald-900/40 p-3.5 sm:p-4">
                  <div className="flex items-center justify-between text-[11px] font-semibold text-emerald-700 dark:text-emerald-300 mb-1">
                    <span>Ôn tập FSRS v5</span>
                    <span>Hôm nay</span>
                  </div>
                  <div className="text-xs sm:text-sm font-bold text-neutral-900 dark:text-white truncate">
                    24 thẻ đến hạn
                  </div>
                  <div className="mt-1.5 text-[11px] text-emerald-700/80 dark:text-emerald-300/80 truncate">
                    Nghe phát âm + Active Recall
                  </div>
                </div>
              </div>
              <div className="mt-3 sm:mt-4 grid grid-cols-3 gap-2.5 sm:gap-3">
                {[
                  { title: "IELTS Speaking", tag: "Nói & IPA", color: "bg-sky-500" },
                  { title: "Tài chính 101", tag: "Case study", color: "bg-amber-500" },
                  { title: "Tâm lý học", tag: "Roadmap 48T", color: "bg-rose-500" },
                ].map((card) => (
                  <div key={card.title} className="rounded-lg border border-neutral-200/70 dark:border-neutral-800 bg-white dark:bg-neutral-900 p-2.5 sm:p-3">
                    <div className="flex items-center gap-1.5 mb-1">
                      <span className={`w-2 h-2 rounded-full ${card.color}`} />
                      <span className="text-[10px] font-medium text-neutral-500 dark:text-neutral-400 truncate">
                        {card.tag}
                      </span>
                    </div>
                    <div className="text-[11px] sm:text-xs font-semibold text-neutral-900 dark:text-white truncate">
                      {card.title}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

/* ── Features / Pillars ───────────────────────────────────────── */
const PILLARS = [
  {
    icon: BookOpen,
    title: "Đọc sâu & Tô màu ý chính",
    description:
      "Đọc giáo trình và PDF dễ nhìn, tô màu 5 nhóm ý quan trọng và gắn ghi chú ngay tại đoạn đang đọc.",
    gradient: "from-blue-500 to-indigo-600",
    bg: "bg-blue-50 dark:bg-blue-950/30",
  },
  {
    icon: Layers,
    title: "Vừa đọc vừa ghi chú (Chia đôi)",
    description:
      "Chia đôi màn hình: đọc bài bên trái, viết sổ tay bên phải — không cần chuyển qua lại giữa nhiều ứng dụng.",
    gradient: "from-violet-500 to-purple-600",
    bg: "bg-violet-50 dark:bg-violet-950/30",
  },
  {
    icon: Brain,
    title: "Ôn thẻ nhớ lâu (Nhắc ôn FSRS)",
    description:
      "Tự động nhắc ôn lại đúng lúc bạn sắp quên, giúp nhớ bài lâu dài mà không cần học vẹt.",
    gradient: "from-emerald-500 to-teal-600",
    bg: "bg-emerald-50 dark:bg-emerald-950/30",
  },
  {
    icon: Sparkles,
    title: "Tạo khóa học nhanh cùng AI",
    description:
      "Tải file PDF lên hoặc nhập chủ đề để hệ thống tạo sẵn chương mục, bài học và thẻ ôn tập.",
    gradient: "from-amber-500 to-orange-600",
    bg: "bg-amber-50 dark:bg-amber-950/30",
  },
];

function FeaturesSection() {
  return (
    <section id="features" data-testid="landing-pillars-section" className="px-6 py-24">
      <div className="max-w-5xl mx-auto">
        <div className="text-center mb-16">
          <h2 className="text-3xl sm:text-4xl font-bold text-neutral-900 dark:text-white tracking-tight mb-4">
            Mọi thứ bạn cần, một nơi duy nhất
          </h2>
          <p className="text-base text-neutral-500 dark:text-neutral-400 max-w-lg mx-auto">
            Bốn trụ cột tạo nên hệ sinh thái học tập hoàn chỉnh
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {PILLARS.map((p) => (
            <div
              key={p.title}
              className={`group rounded-xl border border-neutral-200 dark:border-neutral-800 p-6 hover:border-neutral-300 dark:hover:border-neutral-700 transition-all duration-200 ${p.bg}`}
            >
              <div
                className={`w-10 h-10 rounded-xl bg-gradient-to-br ${p.gradient} flex items-center justify-center mb-4 shadow-sm`}
              >
                <p.icon className="w-5 h-5 text-white" />
              </div>
              <h3 className="text-base font-semibold text-neutral-900 dark:text-white mb-2">
                {p.title}
              </h3>
              <p className="text-sm text-neutral-600 dark:text-neutral-400 leading-relaxed">
                {p.description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

/* ── Workflow Steps ───────────────────────────────────────────── */
const STEPS = [
  {
    step: "01",
    title: "Tải tài liệu lên hoặc tạo khóa học",
    description: "Kéo thả file PDF hoặc nhập chủ đề bạn muốn học để tạo sẵn lộ trình.",
  },
  {
    step: "02",
    title: "Đọc bài & Tô màu ý chính",
    description: "Tô màu đoạn quan trọng, viết ghi chú và tạo thẻ ôn tập ngay khi đang đọc.",
  },
  {
    step: "03",
    title: "Ôn thẻ đúng lúc sắp quên",
    description: "Hệ thống tự chọn những thẻ cần ôn trong ngày. Lật thẻ và bấm mức độ nhớ của bạn.",
  },
  {
    step: "04",
    title: "Làm chủ kiến thức",
    description: "Theo dõi tiến độ, streak, và xem kiến thức tích lũy bền vững qua từng ngày.",
  },
];

function WorkflowSection() {
  return (
    <section data-testid="landing-workflow-section" className="px-6 py-24 bg-neutral-50 dark:bg-neutral-950/50">
      <div className="max-w-6xl mx-auto">
        <div className="text-center mb-12">
          <h2 className="text-3xl sm:text-4xl font-bold text-neutral-900 dark:text-white tracking-tight mb-4">
            Quy trình đơn giản
          </h2>
          <p className="text-base text-neutral-500 dark:text-neutral-400">
            Từ tài liệu thô đến kiến thức bền vững trong 4 bước
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {STEPS.map((s) => (
            <div
              key={s.step}
              className="rounded-2xl border border-neutral-200 dark:border-neutral-800 bg-white dark:bg-neutral-900 p-5 flex flex-col justify-between transition-all hover:border-indigo-300 dark:hover:border-indigo-700 hover:shadow-sm"
            >
              <div>
                <div className="w-11 h-11 rounded-xl bg-neutral-900 dark:bg-white flex items-center justify-center mb-4">
                  <span className="text-sm font-bold text-white dark:text-neutral-900 font-mono">
                    {s.step}
                  </span>
                </div>
                <h3 className="text-base font-semibold text-neutral-900 dark:text-white mb-2">
                  {s.title}
                </h3>
                <p className="text-sm text-neutral-500 dark:text-neutral-400 leading-relaxed">
                  {s.description}
                </p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

/* ── Keyboard Shortcuts Teaser ────────────────────────────────── */
function ShortcutsSection() {
  const shortcuts = [
    { keys: "Ctrl+K", label: "Tìm kiếm nhanh" },
    { keys: "Ctrl+D", label: "Dark mode" },
    { keys: "Ctrl+\\", label: "Thu gọn sidebar" },
    { keys: "Space", label: "Lật thẻ" },
    { keys: "1-4", label: "Đánh giá thẻ" },
  ];

  return (
    <section id="shortcuts" data-testid="landing-shortcuts-section" className="px-6 py-24 scroll-mt-16">
      <div className="max-w-4xl mx-auto">
        <div className="rounded-2xl border border-neutral-200 dark:border-neutral-800 bg-white dark:bg-neutral-900 p-8 sm:p-12">
          <div className="flex items-start gap-4 mb-8">
            <div className="w-10 h-10 rounded-xl bg-neutral-100 dark:bg-neutral-800 flex items-center justify-center shrink-0">
              <Command className="w-5 h-5 text-neutral-600 dark:text-neutral-400" />
            </div>
            <div>
              <h2 className="text-xl font-bold text-neutral-900 dark:text-white mb-1">
                Thiết kế cho bàn phím
              </h2>
              <p className="text-sm text-neutral-500 dark:text-neutral-400">
                Mọi thao tác đều có phím tắt. Tay bạn không cần rời bàn phím.
              </p>
            </div>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
            {shortcuts.map((s) => (
              <div
                key={s.keys}
                className="flex items-center gap-3 px-4 py-3 rounded-lg bg-neutral-50 dark:bg-neutral-800/50"
              >
                <kbd className="px-2 py-1 rounded-md bg-white dark:bg-neutral-800 border border-neutral-200 dark:border-neutral-700 text-xs font-mono text-neutral-600 dark:text-neutral-300 shadow-sm whitespace-nowrap">
                  {s.keys}
                </kbd>
                <span className="text-sm text-neutral-600 dark:text-neutral-400">
                  {s.label}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}

/* ── CTA ──────────────────────────────────────────────────────── */
function CTASection() {
  return (
    <section className="px-6 py-24">
      <div className="max-w-3xl mx-auto text-center">
        <h2 className="text-3xl sm:text-4xl font-bold text-neutral-900 dark:text-white tracking-tight mb-4">
          Sẵn sàng học hiệu quả hơn?
        </h2>
        <p className="text-base text-neutral-500 dark:text-neutral-400 mb-8">
          Bắt đầu miễn phí — không cần thẻ tín dụng.
        </p>
        <Link
          href="/auth?tab=register"
          className="inline-flex items-center gap-2 h-12 px-8 rounded-xl bg-neutral-900 dark:bg-white text-white dark:text-neutral-900 text-sm font-semibold hover:bg-neutral-800 dark:hover:bg-neutral-100 transition shadow-lg shadow-neutral-900/10 dark:shadow-white/10"
        >
          Tạo tài khoản miễn phí
          <ArrowRight className="w-4 h-4" />
        </Link>
      </div>
    </section>
  );
}

/* ── Footer ───────────────────────────────────────────────────── */
function Footer({ onOpenInfo }: { onOpenInfo: (tab: "privacy" | "terms") => void }) {
  return (
    <footer data-testid="landing-footer" className="border-t border-neutral-200 dark:border-neutral-800 px-6 py-8">
      <div className="max-w-6xl mx-auto flex flex-col sm:flex-row items-center justify-between gap-4">
        <div className="flex items-center gap-2">
          <div className="w-6 h-6 rounded-md bg-gradient-to-br from-indigo-500 to-violet-600 flex items-center justify-center text-white text-[10px] font-bold">
            S
          </div>
          <span className="text-sm font-medium text-neutral-600 dark:text-neutral-400">
            Studxy
          </span>
          <span className="text-xs text-neutral-400 dark:text-neutral-500">
            © 2026
          </span>
        </div>
        <div className="flex items-center gap-6 text-xs text-neutral-400 dark:text-neutral-500">
          <button
            type="button"
            data-testid="privacy-modal-trigger"
            onClick={() => onOpenInfo("privacy")}
            className="hover:text-neutral-600 dark:hover:text-neutral-300 transition cursor-pointer"
          >
            Chính sách bảo mật
          </button>
          <button
            type="button"
            data-testid="terms-modal-trigger"
            onClick={() => onOpenInfo("terms")}
            className="hover:text-neutral-600 dark:hover:text-neutral-300 transition cursor-pointer"
          >
            Điều khoản sử dụng
          </button>
          <Link
            href="/app"
            data-testid="landing-app-link"
            className="hover:text-neutral-600 dark:hover:text-neutral-300 transition"
          >
            Vào ứng dụng
          </Link>
        </div>
      </div>
    </footer>
  );
}

/* ── Main Landing Page ────────────────────────────────────────── */
export function LandingPageV2() {
  const router = useRouter();
  const { darkMode, toggleDarkMode } = useWorkspaceStore();
  const [infoModal, setInfoModal] = useState<"privacy" | "terms" | null>(null);
  const infoModalRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!infoModal) return;
    const prevOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    const focusables = infoModalRef.current?.querySelectorAll<HTMLElement>(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    focusables?.[0]?.focus();
    return () => {
      document.body.style.overflow = prevOverflow;
    };
  }, [infoModal]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement | null;
      if (
        target &&
        (target.tagName === "INPUT" || target.tagName === "TEXTAREA" || target.isContentEditable)
      ) {
        return;
      }
      if (infoModal && e.key === "Tab" && infoModalRef.current) {
        const focusables = Array.from(
          infoModalRef.current.querySelectorAll<HTMLElement>(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
          )
        );
        if (focusables.length > 0) {
          e.preventDefault();
          const currentIdx = focusables.indexOf(document.activeElement as HTMLElement);
          const nextIdx = e.shiftKey
            ? (currentIdx <= 0 ? focusables.length - 1 : currentIdx - 1)
            : (currentIdx + 1) % focusables.length;
          focusables[nextIdx].focus();
        }
        return;
      }
      if (e.key === "?" && !e.ctrlKey && !e.metaKey && !e.altKey) {
        e.preventDefault();
        document.getElementById("shortcuts")?.scrollIntoView({ behavior: "smooth" });
      } else if ((e.ctrlKey || e.metaKey) && e.key === "Enter") {
        e.preventDefault();
        router.push("/app");
      } else if (e.key === "Escape" && infoModal) {
        setInfoModal(null);
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [infoModal, router]);

  return (
    <div
      data-testid="landing-page-root"
      className="min-h-screen bg-[var(--canvas-bg)] text-[var(--text-primary)] antialiased"
    >
      <Header dark={darkMode} toggleDark={toggleDarkMode} />
      <HeroSection />
      <PreviewSection />
      <FeaturesSection />
      <WorkflowSection />
      <ShortcutsSection />
      <CTASection />
      <Footer onOpenInfo={(tab) => setInfoModal(tab)} />

      {infoModal && (
        <div
          className="fixed inset-0 z-50 bg-black/50 backdrop-blur-xs flex items-center justify-center p-4"
          onClick={() => setInfoModal(null)}
        >
          <div
            ref={infoModalRef}
            role="dialog"
            aria-modal="true"
            data-testid="landing-info-modal"
            className="w-full max-w-md rounded-2xl border border-neutral-200 dark:border-neutral-800 bg-white dark:bg-neutral-900 p-6 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-base font-semibold text-neutral-900 dark:text-white">
                {infoModal === "privacy" ? "Chính sách bảo mật Studxy" : "Điều khoản sử dụng Studxy"}
              </h3>
              <button
                type="button"
                data-testid="close-info-modal"
                onClick={() => setInfoModal(null)}
                className="p-1 rounded-md text-neutral-400 hover:text-neutral-700 dark:hover:text-neutral-200 transition cursor-pointer"
                aria-label="Đóng"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
            <p className="text-sm text-neutral-600 dark:text-neutral-300 leading-relaxed">
              {infoModal === "privacy"
                ? "Tài liệu PDF, ghi chú và thẻ flashcard FSRS thuộc quyền sở hữu của từng tài khoản học viên và được lưu trữ tách biệt theo người dùng. Bạn có thể xuất toàn bộ khóa học sang Markdown ZIP bất cứ lúc nào."
                : "Studxy cung cấp môi trường học tập chủ động (đọc giáo trình, trích dẫn PDF, ôn tập lặp lại ngắt quãng FSRS v5 và bài luyện tập Practice Quiz) phục vụ mục đích học tập và nghiên cứu cá nhân."}
            </p>
          </div>
        </div>
      )}
    </div>
  );
}
