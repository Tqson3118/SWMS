import React from "react";
import Link from "next/link";
import {
  ShieldCheck,
  ArrowRight,
  Keyboard,
  Compass,
  Zap,
  FileText,
  Lock
} from "lucide-react";

interface HeroSectionProps {
  onOpenShortcuts?: () => void;
  onOpenAbout?: () => void;
}

export function HeroSection({ onOpenShortcuts, onOpenAbout }: HeroSectionProps) {
  return (
    <section
      data-testid="landing-hero-section"
      className="pt-16 pb-12 sm:pt-24 sm:pb-16 text-center max-w-5xl mx-auto px-4 sm:px-6 w-full"
    >
      {/* Eyebrow Badge */}
      <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-indigo-50/80 dark:bg-indigo-950/50 border border-indigo-200/60 dark:border-indigo-800/60 text-xs font-medium text-indigo-700 dark:text-indigo-300 mb-6">
        <ShieldCheck className="w-3.5 h-3.5 text-indigo-600 dark:text-indigo-400" />
        <span>Không gian Học tập &amp; Quản trị Tri thức Cá nhân Chủ quyền</span>
      </div>

      {/* Main Heading */}
      <h1 className="text-4xl sm:text-5xl lg:text-6xl font-extrabold tracking-tight text-slate-900 dark:text-slate-50 leading-[1.15] mb-6">
        Hệ sinh thái Học tập Thống nhất{" "}
        <span className="text-indigo-600 dark:text-indigo-400">
          Cho Cá nhân Chủ quyền
        </span>
      </h1>

      {/* Subheading */}
      <p className="text-lg sm:text-xl text-slate-600 dark:text-slate-300 max-w-3xl mx-auto leading-relaxed mb-8 font-normal">
        Hợp nhất Đọc sâu chuẩn công thái học (68ch), Ghi chép song song (PKM Split), và
        Khoa học trí nhớ dài hạn (FSRS v5) trong một không gian duy nhất — Loại bỏ hoàn
        toàn sự phân mảnh công cụ và nguy cơ thất lạc ngữ cảnh.
      </p>

      {/* Action Buttons */}
      <div className="flex flex-wrap items-center justify-center gap-4 mb-10">
        <Link
          href="/app"
          data-testid="landing-hero-cta"
          className="group inline-flex items-center justify-center px-6 py-3.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-medium text-base shadow-xs hover:shadow-sm transition-all active:scale-[0.98] cursor-pointer"
        >
          <span>Bắt đầu học ngay — Miễn phí</span>
          <ArrowRight className="w-4 h-4 ml-2 group-hover:translate-x-0.5 transition-transform stroke-[2]" />
        </Link>

        {onOpenShortcuts && (
          <button
            type="button"
            onClick={onOpenShortcuts}
            data-testid="keyboard-shortcuts-trigger"
            className="inline-flex items-center justify-center px-5 py-3.5 rounded-xl border border-slate-300/80 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-slate-50 dark:hover:bg-slate-800/80 text-slate-700 dark:text-slate-200 text-sm font-medium transition-all shadow-xs cursor-pointer"
          >
            <Keyboard className="w-4 h-4 mr-2 text-slate-500 dark:text-slate-400" />
            <span>Xem Phím tắt &amp; Luồng học</span>
          </button>
        )}

        {onOpenAbout && (
          <button
            type="button"
            onClick={onOpenAbout}
            data-testid="about-swms-trigger"
            className="inline-flex items-center text-sm font-medium text-slate-500 dark:text-slate-400 hover:text-slate-800 dark:hover:text-slate-200 transition-colors px-2 py-1 cursor-pointer"
          >
            <Compass className="w-4 h-4 mr-1.5 text-indigo-500" />
            <span className="underline decoration-slate-300 dark:decoration-slate-700 underline-offset-4 hover:decoration-indigo-500">
              Khám phá Triết lý Sovereign
            </span>
          </button>
        )}
      </div>

      {/* Trust Highlights Grid */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 pt-8 border-t border-slate-200/60 dark:border-slate-800/60 text-left max-w-4xl mx-auto">
        <div className="flex items-start gap-2.5">
          <div className="p-1.5 rounded-md bg-amber-50 dark:bg-amber-950/40 text-amber-600 dark:text-amber-400 shrink-0">
            <Zap className="w-4 h-4 stroke-[2]" />
          </div>
          <div>
            <div className="text-xs font-semibold text-slate-800 dark:text-slate-200">
              FSRS v5 Engine
            </div>
            <div className="text-[11px] text-slate-500 dark:text-slate-400">
              Client-side p99 &lt; 5ms
            </div>
          </div>
        </div>

        <div className="flex items-start gap-2.5">
          <div className="p-1.5 rounded-md bg-emerald-50 dark:bg-emerald-950/40 text-emerald-600 dark:text-emerald-400 shrink-0">
            <FileText className="w-4 h-4 stroke-[2]" />
          </div>
          <div>
            <div className="text-xs font-semibold text-slate-800 dark:text-slate-200">
              Cột đọc 68ch
            </div>
            <div className="text-[11px] text-slate-500 dark:text-slate-400">
              Công thái học chống mỏi mắt
            </div>
          </div>
        </div>

        <div className="flex items-start gap-2.5">
          <div className="p-1.5 rounded-md bg-indigo-50 dark:bg-indigo-950/40 text-indigo-600 dark:text-indigo-400 shrink-0">
            <ShieldCheck className="w-4 h-4 stroke-[2]" />
          </div>
          <div>
            <div className="text-xs font-semibold text-slate-800 dark:text-slate-200">
              Sovereign Privacy
            </div>
            <div className="text-[11px] text-slate-500 dark:text-slate-400">
              Riêng tư cá nhân 100%
            </div>
          </div>
        </div>

        <div className="flex items-start gap-2.5">
          <div className="p-1.5 rounded-md bg-rose-50 dark:bg-rose-950/40 text-rose-600 dark:text-rose-400 shrink-0">
            <Lock className="w-4 h-4 stroke-[2]" />
          </div>
          <div>
            <div className="text-xs font-semibold text-slate-800 dark:text-slate-200">
              Anti-Vendor Lock
            </div>
            <div className="text-[11px] text-slate-500 dark:text-slate-400">
              Xuất Markdown &amp; Anki tự do
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
