import React from "react";
import Link from "next/link";
import { BookOpen, ShieldCheck, Keyboard, Compass, Github, ArrowRight } from "lucide-react";
export function LandingFooter({ onOpenAbout, onOpenShortcuts }) {
    return (<footer data-testid="landing-footer" className="border-t border-slate-200/80 dark:border-slate-800 bg-white/50 dark:bg-slate-950/50 py-12 w-full transition-colors">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-10">
          {/* Column 1: Brand & Mission */}
          <div className="space-y-4">
            <Link href="/" className="flex items-center gap-2.5">
              <div className="w-8 h-8 rounded-lg bg-indigo-600 text-white flex items-center justify-center shadow-xs">
                <BookOpen className="w-4 h-4 stroke-[2]"/>
              </div>
              <span className="font-bold text-base tracking-tight text-slate-900 dark:text-white">
                SWMS <span className="text-xs text-indigo-600 dark:text-indigo-400 font-medium">Studxy</span>
              </span>
            </Link>

            <p className="text-xs text-slate-600 dark:text-slate-400 leading-relaxed">
              Hệ sinh thái học tập và quản trị tri thức hợp nhất cho cá nhân chủ quyền.
              Đọc sâu 68ch, Split Workspace PKM và khoa học trí nhớ FSRS v5.
            </p>

            <div className="inline-flex items-center gap-1.5 px-2.5 py-1 text-[11px] font-mono rounded-md bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400 border border-slate-200 dark:border-slate-700">
              <ShieldCheck className="w-3.5 h-3.5 text-indigo-500"/>
              <span>Sovereign Learning v1.0 Clean</span>
            </div>
          </div>

          {/* Column 2: Product & Modes */}
          <div className="space-y-3">
            <h4 className="text-xs font-bold uppercase tracking-wider text-slate-900 dark:text-slate-100">
              Không gian Học tập
            </h4>
            <ul className="space-y-2 text-xs text-slate-600 dark:text-slate-400">
              <li>
                <Link href="/workspace" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Personal Dashboard Cá nhân
                </Link>
              </li>
              <li>
                <Link href="/workspace" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Mode 1: Đọc sâu Công thái học (68ch)
                </Link>
              </li>
              <li>
                <Link href="/workspace" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Mode 2: Chia đôi Song song (Split PKM)
                </Link>
              </li>
              <li>
                <Link href="/workspace" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Mode 3: Ôn tập Trí nhớ FSRS v5 &amp; Quiz
                </Link>
              </li>
            </ul>
          </div>

          {/* Column 3: Resources & Guides */}
          <div className="space-y-3">
            <h4 className="text-xs font-bold uppercase tracking-wider text-slate-900 dark:text-slate-100">
              Tài nguyên &amp; Hướng dẫn
            </h4>
            <ul className="space-y-2 text-xs text-slate-600 dark:text-slate-400">
              {onOpenAbout && (<li>
                  <button type="button" onClick={onOpenAbout} data-testid="about-swms-trigger" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors flex items-center gap-1.5 cursor-pointer text-left">
                    <Compass className="w-3.5 h-3.5 text-slate-400"/>
                    <span>Triết lý Sovereign &amp; Hybrid Persona</span>
                  </button>
                </li>)}
              {onOpenShortcuts && (<li>
                  <button type="button" onClick={onOpenShortcuts} data-testid="keyboard-shortcuts-trigger" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors flex items-center gap-1.5 cursor-pointer text-left">
                    <Keyboard className="w-3.5 h-3.5 text-slate-400"/>
                    <span>Sổ tay Toàn bộ Phím tắt (Phím ?)</span>
                  </button>
                </li>)}
              <li>
                <a href="#comparison" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Bảng So sánh Benchmark Đối thủ
                </a>
              </li>
              <li>
                <a href="#workflow" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Quy trình 4 Bước Liền mạch
                </a>
              </li>
            </ul>
          </div>

          {/* Column 4: Community & License */}
          <div className="space-y-3">
            <h4 className="text-xs font-bold uppercase tracking-wider text-slate-900 dark:text-slate-100">
              Mã nguồn &amp; Cộng đồng
            </h4>
            <ul className="space-y-2 text-xs text-slate-600 dark:text-slate-400">
              <li>
                <a href="https://github.com/Tqson3118/SWMS" target="_blank" rel="noopener noreferrer" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors flex items-center gap-1.5">
                  <Github className="w-3.5 h-3.5"/>
                  <span>GitHub Repository</span>
                </a>
              </li>
              <li>
                <span>Giấy phép Mã nguồn mở MIT</span>
              </li>
              <li>
                <span>Kiến trúc ADR-007, ADR-011, ADR-019</span>
              </li>
              <li className="pt-2">
                <Link href="/workspace" className="inline-flex items-center gap-1 px-3 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white font-medium text-xs transition shadow-xs">
                  <span>Vào Workspace ngay</span>
                  <ArrowRight className="w-3 h-3 stroke-[2]"/>
                </Link>
              </li>
            </ul>
          </div>
        </div>

        {/* Copyright Bar */}
        <div className="border-t border-slate-200/60 dark:border-slate-800/60 pt-6 flex flex-col sm:flex-row items-center justify-between text-xs text-slate-500 dark:text-slate-400 gap-2">
          <div>
            © 2026 SWMS (Studxy). All rights reserved.
          </div>
          <div className="flex items-center gap-3">
            <span>Next.js 15 App Router</span>
            <span>•</span>
            <span>Tailwind CSS v4</span>
            <span>•</span>
            <span>Native FSRS v5</span>
          </div>
        </div>
      </div>
    </footer>);
}
