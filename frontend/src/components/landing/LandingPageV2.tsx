"use client";

import React, { useEffect, useState } from "react";
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

  return (
    <header
      className={`fixed top-0 inset-x-0 z-50 transition-all duration-300 ${
        isScrolled
          ? "bg-white/80 dark:bg-neutral-950/80 backdrop-blur-lg border-b border-neutral-200 dark:border-neutral-800"
          : "bg-transparent"
      }`}
    >
      <div className="max-w-6xl mx-auto flex items-center justify-between h-14 px-6">
        {/* Brand */}
        <Link href="/" className="flex items-center gap-2 group">
          <div className="w-7 h-7 rounded-lg bg-gradient-to-br from-indigo-500 to-violet-600 flex items-center justify-center text-white text-xs font-bold shadow-sm">
            S
          </div>
          <span className="text-base font-semibold text-neutral-900 dark:text-white tracking-tight">
            Studxy
          </span>
        </Link>

        {/* Right actions */}
        <div className="flex items-center gap-2">
          <button
            onClick={toggleDark}
            className="p-2 rounded-lg hover:bg-neutral-100 dark:hover:bg-neutral-800 transition text-neutral-500 dark:text-neutral-400"
            aria-label="Toggle theme"
          >
            {dark ? <Sun className="w-4 h-4" /> : <Moon className="w-4 h-4" />}
          </button>
          <Link
            href="/auth"
            className="text-sm font-medium text-neutral-600 dark:text-neutral-300 hover:text-neutral-900 dark:hover:text-white transition px-3 py-1.5"
          >
            Đăng nhập
          </Link>
          <Link
            href="/auth?tab=register"
            className="text-sm font-medium bg-neutral-900 dark:bg-white text-white dark:text-neutral-900 px-4 py-1.5 rounded-lg hover:bg-neutral-800 dark:hover:bg-neutral-100 transition shadow-sm"
          >
            Bắt đầu miễn phí
          </Link>
        </div>
      </div>
    </header>
  );
}

/* ── Hero ─────────────────────────────────────────────────────── */
function HeroSection() {
  return (
    <section className="relative pt-32 pb-20 px-6">
      {/* Subtle gradient background */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[600px] bg-gradient-to-b from-indigo-100/50 via-violet-50/30 to-transparent dark:from-indigo-950/30 dark:via-violet-950/20 dark:to-transparent rounded-full blur-3xl" />
      </div>

      <div className="relative max-w-3xl mx-auto text-center">
        {/* Badge */}
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-indigo-50 dark:bg-indigo-950/50 border border-indigo-100 dark:border-indigo-900/50 text-indigo-600 dark:text-indigo-400 text-xs font-medium mb-6">
          <Sparkles className="w-3.5 h-3.5" />
          Nền tảng học tập thế hệ mới
        </div>

        <h1 className="text-4xl sm:text-5xl md:text-6xl font-bold text-neutral-900 dark:text-white leading-[1.1] tracking-tight mb-5">
          Đọc sâu.{" "}
          <span className="bg-gradient-to-r from-indigo-500 to-violet-500 bg-clip-text text-transparent">
            Ghi nhớ.
          </span>{" "}
          Làm chủ.
        </h1>

        <p className="text-lg text-neutral-600 dark:text-neutral-400 max-w-xl mx-auto mb-8 leading-relaxed">
          Hợp nhất Deep Reader, PKM Notebook và thuật toán FSRS v5 trong một
          không gian duy nhất. Biến mọi tài liệu thành kiến thức bền vững.
        </p>

        <div className="flex flex-col sm:flex-row items-center justify-center gap-3">
          <Link
            href="/auth?tab=register"
            className="inline-flex items-center gap-2 h-11 px-6 rounded-xl bg-neutral-900 dark:bg-white text-white dark:text-neutral-900 text-sm font-semibold hover:bg-neutral-800 dark:hover:bg-neutral-100 transition shadow-lg shadow-neutral-900/10 dark:shadow-white/10"
          >
            Bắt đầu miễn phí
            <ArrowRight className="w-4 h-4" />
          </Link>
          <Link
            href="#features"
            className="inline-flex items-center gap-2 h-11 px-6 rounded-xl border border-neutral-200 dark:border-neutral-700 text-sm font-medium text-neutral-700 dark:text-neutral-300 hover:bg-neutral-50 dark:hover:bg-neutral-800 transition"
          >
            Khám phá tính năng
          </Link>
        </div>

        {/* Keyboard hint */}
        <div className="mt-8 flex items-center justify-center gap-2 text-xs text-neutral-400 dark:text-neutral-500">
          <Keyboard className="w-3.5 h-3.5" />
          <span>Nhấn <kbd className="px-1.5 py-0.5 rounded bg-neutral-100 dark:bg-neutral-800 text-neutral-600 dark:text-neutral-400 font-mono text-[10px]">?</kbd> để xem phím tắt</span>
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
                {["Tổng quan", "Thư viện", "Ôn tập", "Studio"].map((item, i) => (
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
                {["Calculus II", "Triết học 101", "React Patterns"].map((c) => (
                  <div key={c} className="px-3 py-1.5 text-xs text-neutral-500 dark:text-neutral-400 truncate">
                    {c}
                  </div>
                ))}
              </div>
            </div>

            {/* Main content mock */}
            <div className="flex-1 p-6">
              <div className="h-4 w-40 bg-neutral-200 dark:bg-neutral-700 rounded mb-3" />
              <div className="h-3 w-64 bg-neutral-100 dark:bg-neutral-800 rounded mb-6" />
              <div className="grid grid-cols-2 gap-3">
                <div className="rounded-lg bg-gradient-to-br from-indigo-50 to-violet-50 dark:from-indigo-950/30 dark:to-violet-950/30 border border-indigo-100 dark:border-indigo-900/30 p-4 h-24">
                  <div className="h-3 w-20 bg-indigo-200 dark:bg-indigo-800 rounded mb-2" />
                  <div className="h-2 w-32 bg-indigo-100 dark:bg-indigo-900/50 rounded" />
                </div>
                <div className="rounded-lg bg-gradient-to-br from-emerald-50 to-teal-50 dark:from-emerald-950/30 dark:to-teal-950/30 border border-emerald-100 dark:border-emerald-900/30 p-4 h-24">
                  <div className="h-3 w-16 bg-emerald-200 dark:bg-emerald-800 rounded mb-2" />
                  <div className="h-2 w-28 bg-emerald-100 dark:bg-emerald-900/50 rounded" />
                </div>
              </div>
              <div className="mt-4 grid grid-cols-3 gap-3">
                {[1, 2, 3].map((i) => (
                  <div key={i} className="rounded-lg border border-neutral-100 dark:border-neutral-800 p-3 h-20">
                    <div className="h-3 w-16 bg-neutral-200 dark:bg-neutral-700 rounded mb-2" />
                    <div className="h-2 w-full bg-neutral-100 dark:bg-neutral-800 rounded" />
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
    title: "Deep Reader",
    description:
      "Đọc PDF ergonomic với layout 68ch, semantic highlighting 5 màu, annotation và ghi chú liên kết.",
    gradient: "from-blue-500 to-indigo-600",
    bg: "bg-blue-50 dark:bg-blue-950/30",
  },
  {
    icon: Layers,
    title: "Split PKM",
    description:
      "Chia đôi màn hình — đọc bên trái, ghi chú bên phải. Đồng bộ cuộn và liên kết annotation.",
    gradient: "from-violet-500 to-purple-600",
    bg: "bg-violet-50 dark:bg-violet-950/30",
  },
  {
    icon: Brain,
    title: "FSRS v5 Spaced Repetition",
    description:
      "Thuật toán trí nhớ thế hệ mới. Tự động lập lịch ôn tập, tối ưu hóa khoảng cách lặp lại.",
    gradient: "from-emerald-500 to-teal-600",
    bg: "bg-emerald-50 dark:bg-emerald-950/30",
  },
  {
    icon: Sparkles,
    title: "Course Studio + AI",
    description:
      "Tạo khóa học từ PDF, AI Wizard hoặc import. Quản lý curriculum, bài giảng và flashcard.",
    gradient: "from-amber-500 to-orange-600",
    bg: "bg-amber-50 dark:bg-amber-950/30",
  },
];

function FeaturesSection() {
  return (
    <section id="features" className="px-6 py-24">
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
    title: "Upload hoặc tạo khóa học",
    description: "Kéo thả PDF vào Studio, hoặc để AI Wizard tạo curriculum từ chủ đề.",
  },
  {
    step: "02",
    title: "Đọc & Highlight",
    description: "Đọc với layout ergonomic, highlight semantic 5 màu, ghi chú và tạo flashcard ngay trên trang.",
  },
  {
    step: "03",
    title: "Ôn tập thông minh",
    description: "FSRS v5 tính toán thời điểm tối ưu để ôn lại. Lật thẻ, đánh giá, xây dựng trí nhớ dài hạn.",
  },
  {
    step: "04",
    title: "Làm chủ kiến thức",
    description: "Theo dõi tiến độ, streak, và xem kiến thức phát triển qua từng ngày.",
  },
];

function WorkflowSection() {
  return (
    <section className="px-6 py-24 bg-neutral-50 dark:bg-neutral-950/50">
      <div className="max-w-4xl mx-auto">
        <div className="text-center mb-16">
          <h2 className="text-3xl sm:text-4xl font-bold text-neutral-900 dark:text-white tracking-tight mb-4">
            Quy trình đơn giản
          </h2>
          <p className="text-base text-neutral-500 dark:text-neutral-400">
            Từ tài liệu thô đến kiến thức bền vững trong 4 bước
          </p>
        </div>

        <div className="space-y-6">
          {STEPS.map((s, i) => (
            <div
              key={s.step}
              className="flex items-start gap-5 group"
            >
              <div className="shrink-0 w-12 h-12 rounded-xl bg-neutral-900 dark:bg-white flex items-center justify-center">
                <span className="text-sm font-bold text-white dark:text-neutral-900 font-mono">
                  {s.step}
                </span>
              </div>
              <div className="pt-1">
                <h3 className="text-base font-semibold text-neutral-900 dark:text-white mb-1">
                  {s.title}
                </h3>
                <p className="text-sm text-neutral-500 dark:text-neutral-400 leading-relaxed">
                  {s.description}
                </p>
              </div>
              {i < STEPS.length - 1 && (
                <div className="hidden" /> /* Divider handled by space-y */
              )}
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
    <section id="shortcuts" className="px-6 py-24 scroll-mt-16">
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
    <footer className="border-t border-neutral-200 dark:border-neutral-800 px-6 py-8">
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
            onClick={() => onOpenInfo("privacy")}
            className="hover:text-neutral-600 dark:hover:text-neutral-300 transition cursor-pointer"
          >
            Chính sách bảo mật
          </button>
          <button
            type="button"
            onClick={() => onOpenInfo("terms")}
            className="hover:text-neutral-600 dark:hover:text-neutral-300 transition cursor-pointer"
          >
            Điều khoản sử dụng
          </button>
          <Link
            href="/app"
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

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement | null;
      if (
        target &&
        (target.tagName === "INPUT" || target.tagName === "TEXTAREA" || target.isContentEditable)
      ) {
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
    <div className="min-h-screen bg-white dark:bg-neutral-950 text-neutral-900 dark:text-neutral-100 antialiased">
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
            className="w-full max-w-md rounded-2xl border border-neutral-200 dark:border-neutral-800 bg-white dark:bg-neutral-900 p-6 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-base font-semibold text-neutral-900 dark:text-white">
                {infoModal === "privacy" ? "Chính sách bảo mật Studxy" : "Điều khoản sử dụng Studxy"}
              </h3>
              <button
                type="button"
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
