"use client";

import React, { useState } from "react";
import Link from "next/link";
import {
  BookOpen,
  Columns2,
  Brain,
  ArrowRight,
  ExternalLink,
  Lightbulb,
  Code2,
  CircleHelp,
  Link2,
  AlertTriangle,
  RotateCw,
  Sparkles
} from "lucide-react";

type PreviewTab = "reading" | "split" | "review";

export function VisualWorkspacePreview() {
  const [activeTab, setActiveTab] = useState<PreviewTab>("split");

  return (
    <div
      data-testid="landing-workspace-preview"
      className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 mb-20 w-full"
    >
      <div className="rounded-2xl border border-slate-200/90 dark:border-slate-800 bg-white/70 dark:bg-slate-900/60 shadow-xl dark:shadow-indigo-950/10 overflow-hidden backdrop-blur-xs">
        {/* Browser Top Window Bar */}
        <div className="h-11 px-4 border-b border-slate-200/80 dark:border-slate-800 bg-slate-100/70 dark:bg-slate-900/90 flex items-center justify-between">
          {/* Window control dots */}
          <div className="flex items-center gap-1.5">
            <span className="w-2.5 h-2.5 rounded-full bg-rose-400/80 dark:bg-rose-500/80 inline-block" />
            <span className="w-2.5 h-2.5 rounded-full bg-amber-400/80 dark:bg-amber-500/80 inline-block" />
            <span className="w-2.5 h-2.5 rounded-full bg-emerald-400/80 dark:bg-emerald-500/80 inline-block" />
          </div>

          {/* Mode Switcher Tabs */}
          <div className="flex items-center gap-1 bg-slate-200/60 dark:bg-slate-800/80 p-0.5 rounded-lg">
            <button
              type="button"
              onClick={() => setActiveTab("reading")}
              data-testid="preview-mode-tab-1"
              className={`flex items-center gap-1.5 px-3 py-1 text-xs font-medium rounded-md transition-all cursor-pointer ${
                activeTab === "reading"
                  ? "bg-white dark:bg-slate-900 text-indigo-600 dark:text-indigo-400 shadow-xs"
                  : "text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white"
              }`}
            >
              <BookOpen className="w-3.5 h-3.5" />
              <span className="hidden sm:inline">Mode 1:</span> Đọc sâu (68ch)
            </button>

            <button
              type="button"
              onClick={() => setActiveTab("split")}
              data-testid="preview-mode-tab-2"
              className={`flex items-center gap-1.5 px-3 py-1 text-xs font-medium rounded-md transition-all cursor-pointer ${
                activeTab === "split"
                  ? "bg-white dark:bg-slate-900 text-indigo-600 dark:text-indigo-400 shadow-xs"
                  : "text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white"
              }`}
            >
              <Columns2 className="w-3.5 h-3.5" />
              <span className="hidden sm:inline">Mode 2:</span> Chia đôi (Vừa đọc vừa ghi)
            </button>

            <button
              type="button"
              onClick={() => setActiveTab("review")}
              data-testid="preview-mode-tab-3"
              className={`flex items-center gap-1.5 px-3 py-1 text-xs font-medium rounded-md transition-all cursor-pointer ${
                activeTab === "review"
                  ? "bg-white dark:bg-slate-900 text-indigo-600 dark:text-indigo-400 shadow-xs"
                  : "text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white"
              }`}
            >
              <Brain className="w-3.5 h-3.5" />
              <span className="hidden sm:inline">Mode 3:</span> Ôn tập FSRS
            </button>
          </div>

          {/* Fake URL */}
          <div className="hidden lg:flex items-center gap-1.5 text-[11px] font-mono text-slate-400 bg-white/50 dark:bg-slate-950/50 px-2.5 py-0.5 rounded border border-slate-200/50 dark:border-slate-800">
            <span>app.studxy.vn/workspace</span>
          </div>
        </div>

        {/* Mock View Content */}
        <div className="p-4 sm:p-6 min-h-[380px] bg-slate-50/50 dark:bg-slate-950/40 relative flex flex-col justify-center">
          {/* MODE 1 PREVIEW: Reading Mode */}
          {activeTab === "reading" && (
            <div className="max-w-[68ch] mx-auto w-full space-y-4 text-left">
              <div className="flex items-center justify-between pb-3 border-b border-slate-200 dark:border-slate-800">
                <span className="text-xs font-mono text-slate-500">
                  Chương 3 &gt; Bài 3.2: Bộ nhớ ảo &amp; TLB (Đọc sâu 68ch)
                </span>
                {/* Simulated in-situ floating selection toolbar */}
                <div className="flex items-center gap-1 px-2 py-1 rounded-lg bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs">
                  <span className="w-3 h-3 rounded-full bg-yellow-400/90 inline-block" title="Khái niệm" />
                  <span className="w-3 h-3 rounded-full bg-emerald-400/90 inline-block" title="Dẫn chứng" />
                  <span className="w-3 h-3 rounded-full bg-sky-400/90 inline-block" title="Nghi vấn" />
                  <span className="w-3 h-3 rounded-full bg-purple-400/90 inline-block" title="Tổng hợp" />
                  <span className="w-3 h-3 rounded-full bg-rose-400/90 inline-block" title="Lưu ý" />
                  <span className="text-[10px] font-mono text-slate-400 pl-1">Phím 1-5</span>
                </div>
              </div>

              <h3 className="text-xl sm:text-2xl font-bold font-serif text-slate-900 dark:text-slate-100">
                Cơ chế Chuyển đổi Địa chỉ &amp; Bảng Trang Đa cấp
              </h3>

              <p className="text-sm sm:text-base leading-relaxed text-slate-700 dark:text-slate-300 font-serif">
                Trong các hệ điều hành hiện đại, bộ nhớ ảo cung cấp một không gian địa chỉ trừu tượng, liên tục cho mỗi tiến trình.{" "}
                <span className="swms-highlight-yellow">
                  Bảng trang đa cấp (Multi-level Page Tables) giải quyết triệt để bài toán lãng phí bộ nhớ vật lý
                </span>{" "}
                bằng cách chỉ cấp phát các trang con khi thực sự cần thiết, thay vì giữ cố định toàn bộ bảng trang phẳng hàng triệu mục.
              </p>

              <div className="p-3.5 rounded-xl border border-indigo-100 dark:border-indigo-900/60 bg-indigo-50/50 dark:bg-indigo-950/20 text-xs text-indigo-900 dark:text-indigo-300">
                <span className="font-semibold">W3C TextQuoteSelector Anchor:</span> prefix: "tiến trình. ", exact: "Bảng trang đa cấp giải quyết triệt để bài toán lãng phí", suffix: " bằng cách chỉ cấp phát..."
              </div>
            </div>
          )}

          {/* MODE 2 PREVIEW: Split PKM Workspace */}
          {activeTab === "split" && (
            <div className="grid grid-cols-12 gap-3 h-[320px] text-left">
              {/* Left Pane (15%): Outline */}
              <div className="hidden md:flex md:col-span-2 rounded-xl border border-slate-200/80 dark:border-slate-800 bg-white/60 dark:bg-slate-900/40 p-3 flex-col justify-between text-xs">
                <div>
                  <div className="text-[11px] font-bold text-slate-400 uppercase tracking-wider mb-2">
                    MỤC LỤC
                  </div>
                  <div className="space-y-1.5 text-slate-600 dark:text-slate-400">
                    <div className="text-indigo-600 dark:text-indigo-400 font-semibold truncate">
                      1. Không gian ảo
                    </div>
                    <div className="truncate pl-2 font-medium">1.1 TLB Cache</div>
                    <div className="truncate pl-2">1.2 Multi-level Page</div>
                    <div className="truncate">2. Page Fault</div>
                  </div>
                </div>
                <div className="text-[10px] text-slate-400">Sidebar Rail 15%</div>
              </div>

              {/* Center Pane (45%): Reader */}
              <div className="col-span-12 md:col-span-5 rounded-xl border border-slate-200/80 dark:border-slate-800 bg-white dark:bg-slate-900/80 p-4 text-xs space-y-3 overflow-hidden flex flex-col justify-between">
                <div>
                  <div className="flex items-center justify-between pb-2 mb-2 border-b border-slate-100 dark:border-slate-800">
                    <span className="font-semibold text-slate-800 dark:text-slate-200">
                      BÀI ĐỌC LÝ THUYẾT (45%)
                    </span>
                    <span className="text-[10px] font-mono text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-950 px-1.5 py-0.5 rounded">
                      Scroll-Sync ON
                    </span>
                  </div>
                  <p className="text-slate-700 dark:text-slate-300 font-serif leading-relaxed line-clamp-4">
                    Translation Lookaside Buffer (TLB) là một bộ nhớ cache phần cứng tốc độ cao được tích hợp ngay bên trong CPU.{" "}
                    <span className="swms-highlight-green">
                      Khi có TLB hit, địa chỉ vật lý được giải quyết chỉ trong 1 chu kỳ xung nhịp
                    </span>, tránh được việc phải truy cập RAM 2 đến 4 lần.
                  </p>
                </div>
                <div className="p-2 rounded-lg bg-slate-50 dark:bg-slate-800/60 border border-slate-200/60 dark:border-slate-700/60 flex items-center justify-between text-[11px]">
                  <span>Vị trí anchor #tlb-cache</span>
                  <span className="text-indigo-600 dark:text-indigo-400 font-mono">Offset 1420</span>
                </div>
              </div>

              {/* Right Pane (40%): Notebook */}
              <div className="col-span-12 md:col-span-5 rounded-xl border border-indigo-200/80 dark:border-indigo-900/60 bg-white dark:bg-slate-900/90 p-4 text-xs space-y-3 flex flex-col justify-between shadow-xs">
                <div>
                  <div className="flex items-center justify-between pb-2 mb-2 border-b border-slate-100 dark:border-slate-800">
                    <span className="font-semibold text-indigo-700 dark:text-indigo-400 flex items-center gap-1.5">
                      <Sparkles className="w-3.5 h-3.5" />
                      SỔ TAY GHI CHÚ SONG SONG (40%)
                    </span>
                    <span className="text-[10px] text-slate-400 font-mono">Tiptap AST</span>
                  </div>

                  {/* Simulated Citation Card */}
                  <div className="p-2.5 rounded-lg border border-indigo-200/70 dark:border-indigo-800/60 bg-indigo-50/40 dark:bg-indigo-950/30 text-[11px] space-y-1">
                    <div className="flex items-center justify-between text-indigo-600 dark:text-indigo-400 font-semibold">
                      <span>Dẫn chứng trích dẫn</span>
                      <span className="font-mono text-[10px] hover:underline cursor-pointer flex items-center gap-0.5">
                        [↗ Jump to Source]
                      </span>
                    </div>
                    <p className="text-slate-600 dark:text-slate-300 italic line-clamp-2">
                      "Khi có TLB hit, địa chỉ vật lý được giải quyết chỉ trong 1 chu kỳ xung nhịp..."
                    </p>
                  </div>

                  <p className="mt-2 text-slate-700 dark:text-slate-300 leading-relaxed">
                    → Ý nghĩa: Cần tối ưu thuật toán tuần tự để tăng tính cục bộ không gian (spatial locality), tối đa hóa TLB hit rate.
                  </p>
                </div>

                <div className="flex items-center justify-between text-[10px] text-slate-400 pt-2 border-t border-slate-100 dark:border-slate-800">
                  <span>Auto-saved via Web Locks</span>
                  <span>Markdown / Wikilinks ready</span>
                </div>
              </div>
            </div>
          )}

          {/* MODE 3 PREVIEW: FSRS Spaced Repetition */}
          {activeTab === "review" && (
            <div className="max-w-md mx-auto w-full text-center space-y-4">
              <div className="p-6 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 shadow-md text-left space-y-3">
                <div className="flex items-center justify-between text-xs font-mono text-slate-400">
                  <span>Thẻ #128 • Tag: HệĐiềuHành</span>
                  <span className="text-amber-500 font-bold">FSRS v5 DSR</span>
                </div>
                <div className="text-xs text-indigo-600 dark:text-indigo-400 font-bold uppercase tracking-wider">
                  CÂU HỎI HỒI TƯỞNG
                </div>
                <p className="text-sm sm:text-base font-semibold text-slate-900 dark:text-slate-100">
                  Giải thích tại sao Bảng trang đa cấp (Multi-level Page Tables) tiết kiệm bộ nhớ hơn so với Bảng trang đơn cấp tuyến tính?
                </p>
                <div className="pt-2 border-t border-slate-100 dark:border-slate-800 text-xs text-slate-500 flex items-center justify-between">
                  <span>Bấm phím Space để lật thẻ 3D</span>
                  <span className="font-mono text-[11px]">Due: Hôm nay</span>
                </div>
              </div>

              {/* 4 FSRS rating buttons */}
              <div className="grid grid-cols-4 gap-2 text-xs">
                <button
                  type="button"
                  className="p-2 rounded-xl border border-rose-200 dark:border-rose-900/60 bg-rose-50/50 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400 font-semibold"
                >
                  <div className="text-[10px] font-mono text-slate-400">1</div>
                  <div>Again</div>
                  <div className="text-[10px] opacity-75">&lt;10m</div>
                </button>
                <button
                  type="button"
                  className="p-2 rounded-xl border border-amber-200 dark:border-amber-900/60 bg-amber-50/50 dark:bg-amber-950/30 text-amber-700 dark:text-amber-400 font-semibold"
                >
                  <div className="text-[10px] font-mono text-slate-400">2</div>
                  <div>Hard</div>
                  <div className="text-[10px] opacity-75">1.2d</div>
                </button>
                <button
                  type="button"
                  className="p-2 rounded-xl border border-emerald-200 dark:border-emerald-900/60 bg-emerald-50/50 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400 font-semibold"
                >
                  <div className="text-[10px] font-mono text-slate-400">3</div>
                  <div>Good</div>
                  <div className="text-[10px] opacity-75">3.5d</div>
                </button>
                <button
                  type="button"
                  className="p-2 rounded-xl border border-sky-200 dark:border-sky-900/60 bg-sky-50/50 dark:bg-sky-950/30 text-sky-700 dark:text-sky-400 font-semibold"
                >
                  <div className="text-[10px] font-mono text-slate-400">4</div>
                  <div>Easy</div>
                  <div className="text-[10px] opacity-75">8.2d</div>
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Footer Overlay CTA */}
        <div className="h-10 px-4 border-t border-slate-200/80 dark:border-slate-800 bg-slate-50 dark:bg-slate-900/90 flex items-center justify-between text-xs text-slate-500">
          <span>Mô phỏng Giao diện Không gian Học tập SWMS Clean Workspace v1.0</span>
          <Link
            href="/app"
            className="text-indigo-600 dark:text-indigo-400 hover:underline flex items-center font-medium"
          >
            <span>Trải nghiệm trực tiếp</span>
            <ArrowRight className="w-3 h-3 ml-1" />
          </Link>
        </div>
      </div>
    </div>
  );
}
