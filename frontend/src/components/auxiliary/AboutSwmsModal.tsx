"use client";

import React, { useEffect, useState, useRef } from "react";
import {
  ShieldCheck,
  Lock,
  Layers,
  BookOpen,
  Cpu,
  Sparkles,
  CheckCircle2,
  ArrowRight,
  X,
  FileCode,
  RotateCw,
  HardDrive,
  Users
} from "lucide-react";

export interface AboutSwmsModalProps {
  isOpen: boolean;
  onClose: () => void;
  onExploreWorkspace?: () => void;
}

type TabKey = "sovereign" | "hybrid" | "privacy" | "open-data" | "resilience";

export function AboutSwmsModal({
  isOpen,
  onClose,
  onExploreWorkspace
}: AboutSwmsModalProps) {
  const [activeTab, setActiveTab] = useState<TabKey>("sovereign");
  const modalRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && isOpen) {
        onClose();
        return;
      }
      if (e.key === "Tab" && isOpen && modalRef.current) {
        const focusableElements = modalRef.current.querySelectorAll<HTMLElement>(
          'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        if (focusableElements.length === 0) return;
        const firstEl = focusableElements[0];
        const lastEl = focusableElements[focusableElements.length - 1];

        if (e.shiftKey) {
          if (document.activeElement === firstEl) {
            e.preventDefault();
            lastEl.focus();
          }
        } else {
          if (document.activeElement === lastEl) {
            e.preventDefault();
            firstEl.focus();
          }
        }
      }
    };
    if (isOpen) {
      document.body.style.overflow = "hidden";
      window.addEventListener("keydown", handleKeyDown);
    } else {
      document.body.style.overflow = "unset";
    }
    return () => {
      document.body.style.overflow = "unset";
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return (
    <div
      data-testid="about-swms-modal"
      role="dialog"
      aria-modal="true"
      aria-labelledby="about-swms-title"
      className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 bg-slate-950/65 backdrop-blur-xs animate-in fade-in duration-200"
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <div
        ref={modalRef}
        className="relative w-full max-w-3xl max-h-[88vh] flex flex-col rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-2xl overflow-hidden transition-all text-slate-900 dark:text-slate-100"
      >
        {/* Header */}
        <div className="flex items-start justify-between p-5 border-b border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-900/70">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-indigo-600/10 dark:bg-indigo-500/20 text-indigo-600 dark:text-indigo-400 flex items-center justify-center shrink-0 border border-indigo-200/50 dark:border-indigo-800/60">
              <ShieldCheck className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="text-[11px] font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-950/60 px-2 py-0.5 rounded-full border border-indigo-200/60 dark:border-indigo-800/60">
                  Triết lý Sản phẩm &amp; Kiến trúc
                </span>
                <span className="text-xs text-slate-400 dark:text-slate-500">·</span>
                <span className="text-xs text-slate-500 dark:text-slate-400 font-medium">ADR-019 Baseline</span>
              </div>
              <h2
                id="about-swms-title"
                className="text-lg sm:text-xl font-bold tracking-tight text-slate-900 dark:text-slate-100 mt-0.5"
              >
                Về Studxy — Không gian Học tập Chủ quyền Cá nhân
              </h2>
            </div>
          </div>
          <button
            type="button"
            data-testid="close-about-modal"
            onClick={onClose}
            aria-label="Đóng"
            className="p-1.5 rounded-lg text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 transition cursor-pointer"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Navigation Tabs */}
        <div className="flex border-b border-slate-200 dark:border-slate-800 px-5 bg-slate-50/40 dark:bg-slate-900/40 overflow-x-auto no-scrollbar">
          <button
            type="button"
            onClick={() => setActiveTab("sovereign")}
            className={`flex items-center gap-1.5 py-3 px-3 text-xs font-semibold border-b-2 transition cursor-pointer whitespace-nowrap ${
              activeTab === "sovereign"
                ? "border-indigo-600 text-indigo-600 dark:text-indigo-400"
                : "border-transparent text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200"
            }`}
          >
            <ShieldCheck className="w-3.5 h-3.5" />
            <span>Cá nhân Chủ quyền</span>
          </button>

          <button
            type="button"
            onClick={() => setActiveTab("hybrid")}
            className={`flex items-center gap-1.5 py-3 px-3 text-xs font-semibold border-b-2 transition cursor-pointer whitespace-nowrap ${
              activeTab === "hybrid"
                ? "border-indigo-600 text-indigo-600 dark:text-indigo-400"
                : "border-transparent text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200"
            }`}
          >
            <Users className="w-3.5 h-3.5" />
            <span>Mô hình Hybrid Persona</span>
          </button>

          <button
            type="button"
            onClick={() => setActiveTab("privacy")}
            className={`flex items-center gap-1.5 py-3 px-3 text-xs font-semibold border-b-2 transition cursor-pointer whitespace-nowrap ${
              activeTab === "privacy"
                ? "border-indigo-600 text-indigo-600 dark:text-indigo-400"
                : "border-transparent text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200"
            }`}
          >
            <Lock className="w-3.5 h-3.5" />
            <span>Quyền Riêng tư Độc lập</span>
          </button>

          <button
            type="button"
            onClick={() => setActiveTab("open-data")}
            className={`flex items-center gap-1.5 py-3 px-3 text-xs font-semibold border-b-2 transition cursor-pointer whitespace-nowrap ${
              activeTab === "open-data"
                ? "border-indigo-600 text-indigo-600 dark:text-indigo-400"
                : "border-transparent text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200"
            }`}
          >
            <FileCode className="w-3.5 h-3.5" />
            <span>Chống Khóa Dữ liệu</span>
          </button>

          <button
            type="button"
            onClick={() => setActiveTab("resilience")}
            className={`flex items-center gap-1.5 py-3 px-3 text-xs font-semibold border-b-2 transition cursor-pointer whitespace-nowrap ${
              activeTab === "resilience"
                ? "border-indigo-600 text-indigo-600 dark:text-indigo-400"
                : "border-transparent text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200"
            }`}
          >
            <RotateCw className="w-3.5 h-3.5" />
            <span>Kiến trúc Ngoại tuyến</span>
          </button>
        </div>

        {/* Tab Content Body */}
        <div className="p-6 overflow-y-auto space-y-4 text-sm leading-relaxed text-slate-700 dark:text-slate-300">
          {activeTab === "sovereign" && (
            <div className="space-y-4 animate-in fade-in duration-150">
              <div className="p-4 rounded-xl bg-indigo-50/70 dark:bg-indigo-950/30 border border-indigo-100 dark:border-indigo-900/50">
                <h4 className="font-bold text-indigo-950 dark:text-indigo-200 text-base mb-1 flex items-center gap-2">
                  <ShieldCheck className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
                  Triết lý Sovereign Personal Workspace
                </h4>
                <p className="text-xs sm:text-sm text-indigo-900/90 dark:text-indigo-300">
                  Tại SWMS, chúng tôi tin rằng người học là trung tâm làm chủ tri thức. Toàn bộ tài liệu PDF tải lên, bài giảng cá nhân, đánh dấu ngữ nghĩa và hàng đợi thẻ ôn tập FSRS là tài sản vĩnh viễn thuộc quyền sở hữu của bạn.
                </p>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-3.5 pt-1">
                <div className="p-3.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
                  <div className="font-semibold text-slate-900 dark:text-slate-100 text-xs mb-1 flex items-center gap-1.5">
                    <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" />
                    Không gian Học tập Độc lập
                  </div>
                  <p className="text-xs text-slate-600 dark:text-slate-400 leading-normal">
                    Không giống như LMS truyền thống đóng cửa khi hết học kỳ, SWMS duy trì dòng chảy tri thức của bạn trọn đời qua mọi giai đoạn sự nghiệp.
                  </p>
                </div>

                <div className="p-3.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
                  <div className="font-semibold text-slate-900 dark:text-slate-100 text-xs mb-1 flex items-center gap-1.5">
                    <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" />
                    Bảo toàn Ngữ cảnh Gốc
                  </div>
                  <p className="text-xs text-slate-600 dark:text-slate-400 leading-normal">
                    Mọi trích dẫn ghi chú hay thẻ flashcard đều gắn chặt với tọa độ PDF và W3C TextQuotes, cho phép nhảy ngược về văn bản gốc với 1 cú click.
                  </p>
                </div>
              </div>
            </div>
          )}

          {activeTab === "hybrid" && (
            <div className="space-y-4 animate-in fade-in duration-150">
              <div className="p-4 rounded-xl bg-purple-50/70 dark:bg-purple-950/30 border border-purple-100 dark:border-purple-900/50">
                <h4 className="font-bold text-purple-950 dark:text-purple-200 text-base mb-1 flex items-center gap-2">
                  <Users className="w-4 h-4 text-purple-600 dark:text-purple-400" />
                  Mô hình Hybrid Persona (ADR-019)
                </h4>
                <p className="text-xs sm:text-sm text-purple-900/90 dark:text-purple-300">
                  Dung hòa hoàn hảo giữa hai thế giới: Tự do học tập của cá nhân độc lập và kỷ luật học thuật của sinh viên tham gia khóa học chính quy.
                </p>
              </div>

              <div className="space-y-2 text-xs">
                <div className="flex items-start gap-3 p-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
                  <span className="w-6 h-6 rounded-lg bg-indigo-100 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300 flex items-center justify-center font-bold text-xs shrink-0">
                    1
                  </span>
                  <div>
                    <span className="font-bold text-slate-900 dark:text-slate-100">Học viên Tự do (Autonomous Self-Learner):</span>
                    <p className="text-slate-600 dark:text-slate-400 mt-0.5">
                      Khởi tạo kho khóa học cá nhân (visibility = 'personal'), tải giáo trình PDF tùy ý, tạo ghi chú và thẻ Cloze không giới hạn.
                    </p>
                  </div>
                </div>

                <div className="flex items-start gap-3 p-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
                  <span className="w-6 h-6 rounded-lg bg-purple-100 dark:bg-purple-950 text-purple-700 dark:text-purple-300 flex items-center justify-center font-bold text-xs shrink-0">
                    2
                  </span>
                  <div>
                    <span className="font-bold text-slate-900 dark:text-slate-100">Sinh viên Theo Khóa (Academic Cohort Student):</span>
                    <p className="text-slate-600 dark:text-slate-400 mt-0.5">
                      Tham gia lớp học của giảng viên, nhận tài liệu chính quy và làm bài kiểm tra Formative Practice Quiz với AI Rubric.
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}

          {activeTab === "privacy" && (
            <div className="space-y-4 animate-in fade-in duration-150">
              <div className="p-4 rounded-xl bg-amber-50/70 dark:bg-amber-950/30 border border-amber-100 dark:border-amber-900/50">
                <h4 className="font-bold text-amber-950 dark:text-amber-200 text-base mb-1 flex items-center gap-2">
                  <Lock className="w-4 h-4 text-amber-600 dark:text-amber-400" />
                  Quyền Riêng tư Dữ liệu Tuyệt đối
                </h4>
                <p className="text-xs sm:text-sm text-amber-900/90 dark:text-amber-300">
                  Bức tường bảo mật độc lập ngăn chặn mọi hành vi xâm phạm quyền riêng tư của người học.
                </p>
              </div>

              <ul className="space-y-2.5 text-xs text-slate-600 dark:text-slate-400">
                <li className="flex items-start gap-2">
                  <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" />
                  <span>
                    <strong className="text-slate-900 dark:text-slate-100">Không Giám sát Ghi chú:</strong> Giảng viên và nhà trường tuyệt đối không thể xem ghi chú cá nhân, trích dẫn hay thẻ ghi nhớ của bạn trong bất kỳ hoàn cảnh nào.
                  </span>
                </li>
                <li className="flex items-start gap-2">
                  <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" />
                  <span>
                    <strong className="text-slate-900 dark:text-slate-100">Mã hóa Dữ liệu Học tập:</strong> Toàn bộ dữ liệu ghi chú JSON AST và lịch sử FSRS được mã hóa an toàn trên đường truyền và tại nơi lưu trữ.
                  </span>
                </li>
                <li className="flex items-start gap-2">
                  <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" />
                  <span>
                    <strong className="text-slate-900 dark:text-slate-100">Không Bán Dữ liệu:</strong> Tri thức và hành vi học tập của bạn không bao giờ bị sử dụng cho mục đích quảng cáo thương mại.
                  </span>
                </li>
              </ul>
            </div>
          )}

          {activeTab === "open-data" && (
            <div className="space-y-4 animate-in fade-in duration-150">
              <div className="p-4 rounded-xl bg-emerald-50/70 dark:bg-emerald-950/30 border border-emerald-100 dark:border-emerald-900/50">
                <h4 className="font-bold text-emerald-950 dark:text-emerald-200 text-base mb-1 flex items-center gap-2">
                  <FileCode className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
                  Cam kết Chống Khóa Nền tảng (Anti-Vendor Lock-in)
                </h4>
                <p className="text-xs sm:text-sm text-emerald-900/90 dark:text-emerald-300">
                  Dữ liệu của bạn là của bạn. Bạn có thể xuất toàn bộ tài sản học tập ra các định dạng chuẩn bất kỳ lúc nào.
                </p>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-xs">
                <div className="p-3.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
                  <div className="font-bold text-slate-900 dark:text-slate-100 mb-1">Standard Markdown (.md)</div>
                  <p className="text-slate-600 dark:text-slate-400">
                    Xuất ghi chú sang Markdown tương thích 100% với Obsidian, Logseq, Notion với đầy đủ YAML Frontmatter và anchor links.
                  </p>
                </div>
                <div className="p-3.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
                  <div className="font-bold text-slate-900 dark:text-slate-100 mb-1">Anki Package (.apkg)</div>
                  <p className="text-slate-600 dark:text-slate-400">
                    Xuất toàn bộ flashcards kèm tham số FSRS sang Anki để tiếp tục ôn tập trên thiết bị di động mà không mất lịch sử ghi nhớ.
                  </p>
                </div>
              </div>
            </div>
          )}

          {activeTab === "resilience" && (
            <div className="space-y-4 animate-in fade-in duration-150">
              <div className="p-4 rounded-xl bg-sky-50/70 dark:bg-sky-950/30 border border-sky-100 dark:border-sky-900/50">
                <h4 className="font-bold text-sky-950 dark:text-sky-200 text-base mb-1 flex items-center gap-2">
                  <RotateCw className="w-4 h-4 text-sky-600 dark:text-sky-400" />
                  Kiến trúc Ngoại tuyến Kiên cường (Offline-First)
                </h4>
                <p className="text-xs sm:text-sm text-sky-900/90 dark:text-sky-300">
                  Học tập liên tục ngay cả khi mất mạng. Thuật toán FSRS v5 chạy trực tiếp trên client với độ trễ phản hồi p99 &lt; 5ms.
                </p>
              </div>

              <div className="space-y-2 text-xs text-slate-600 dark:text-slate-400">
                <div className="p-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
                  <div className="font-bold text-slate-900 dark:text-slate-100 mb-0.5">Web Locks API &amp; Optimistic Concurrency</div>
                  <p>Bảo vệ ghi chép khỏi xung đột khi mở nhiều tab trình duyệt cùng lúc; tự động giải quyết xung đột 409 theo cơ chế "Keep-Both".</p>
                </div>
                <div className="p-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
                  <div className="font-bold text-slate-900 dark:text-slate-100 mb-0.5">Hàng đợi Ngoại tuyến IndexedDB</div>
                  <p>Lưu đệm toàn bộ thao tác đánh dấu và đánh giá ôn tập khi mất kết nối; tự động đẩy lên server khi có mạng trở lại.</p>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between p-4 px-6 border-t border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-900/70 text-xs">
          <span className="text-slate-500 dark:text-slate-400 hidden sm:inline">
            SWMS v1.0 • Chuẩn Clean Workspace
          </span>
          <div className="flex items-center gap-2 w-full sm:w-auto justify-end">
            <button
              type="button"
              data-testid="close-about-modal"
              onClick={onClose}
              className="px-4 py-2 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-100 dark:hover:bg-slate-700 font-semibold text-slate-700 dark:text-slate-200 transition cursor-pointer"
            >
              Đóng
            </button>
            {onExploreWorkspace && (
              <button
                type="button"
                onClick={() => {
                  onClose();
                  onExploreWorkspace();
                }}
                className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-semibold flex items-center gap-1.5 transition cursor-pointer shadow-xs"
              >
                <span>Vào Không gian làm việc</span>
                <ArrowRight className="w-3.5 h-3.5" />
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
