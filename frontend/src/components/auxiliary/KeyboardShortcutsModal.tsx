"use client";

import React, { useEffect, useState, useMemo, useRef } from "react";
import { Keyboard, Search, X } from "lucide-react";

export interface KeyboardShortcutsModalProps {
  isOpen: boolean;
  onClose: () => void;
  initialTab?: "all" | "global" | "reading" | "split" | "review";
}

type ShortcutCategory = "all" | "global" | "reading" | "split" | "review";

interface ShortcutItem {
  id: string;
  category: "global" | "reading" | "split" | "review";
  keys: string[];
  description: string;
  badge?: string;
}

function normalizeVietnamese(str: string): string {
  return str
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase();
}

const SHORTCUTS_DATA: ShortcutItem[] = [
  // Global & Navigation
  {
    id: "g1",
    category: "global",
    keys: ["Ctrl", "K"],
    description: "Mở bảng lệnh tìm kiếm & tạo thẻ ghi nhớ nhanh",
    badge: "Phổ biến"
  },
  {
    id: "g2",
    category: "global",
    keys: ["?"],
    description: "Mở hoặc đóng bảng tra cứu phím tắt",
    badge: "Trợ giúp"
  },
  {
    id: "g3",
    category: "global",
    keys: ["Esc"],
    description: "Đóng hộp thoại hoặc bảng đang mở"
  },
  {
    id: "g4",
    category: "global",
    keys: ["Ctrl", "D"],
    description: "Chuyển đổi giao diện sáng / tối"
  },
  {
    id: "g5",
    category: "global",
    keys: ["Ctrl", "\\"],
    description: "Thu gọn hoặc mở rộng thanh điều hướng"
  },
  {
    id: "g6",
    category: "global",
    keys: ["/"],
    description: "Đưa con trỏ vào ô tìm kiếm trên trang Khóa học"
  },

  // Learning & Reading
  {
    id: "r1",
    category: "reading",
    keys: ["Ctrl", "Shift", "F"],
    description: "Tìm kiếm toàn văn trong khóa học hiện tại",
    badge: "Tìm kiếm"
  },
  {
    id: "r2",
    category: "reading",
    keys: ["Alt", "← / →"],
    description: "Chuyển nhanh sang bài học trước hoặc bài học tiếp theo"
  },
  {
    id: "r3",
    category: "reading",
    keys: ["Alt", "Z"],
    description: "Bật hoặc tắt chế độ đọc tập trung",
    badge: "Tập trung"
  },

  // Studio & Editor (category id "split" kept for backwards compatibility)
  {
    id: "s1",
    category: "split",
    keys: ["Ctrl", "S"],
    description: "Lưu thay đổi bài giảng trong trình biên tập",
    badge: "Biên tập"
  },
  {
    id: "s2",
    category: "split",
    keys: ["Ctrl", "Enter"],
    description: "Nộp câu trả lời tự luận hoặc trả lời ngắn trong bài kiểm tra",
    badge: "Kiểm tra"
  },

  // Review & Practice
  {
    id: "v1",
    category: "review",
    keys: ["Space"],
    description: "Lật thẻ ghi nhớ FSRS để xem đáp án",
    badge: "Cốt lõi"
  },
  {
    id: "v2",
    category: "review",
    keys: ["1 - 4"],
    description: "Đánh giá mức độ nhớ thẻ FSRS (1: Quên, 2: Khó, 3: Tốt, 4: Dễ)",
    badge: "FSRS"
  },
  {
    id: "v3",
    category: "review",
    keys: ["Z"],
    description: "Hoàn tác lượt đánh giá thẻ FSRS vừa rồi"
  }
];

export function KeyboardShortcutsModal({
  isOpen,
  onClose,
  initialTab = "all"
}: KeyboardShortcutsModalProps) {
  const [activeCategory, setActiveCategory] = useState<ShortcutCategory>(initialTab);
  const [searchQuery, setSearchQuery] = useState("");
  const modalRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    setActiveCategory(initialTab);
  }, [initialTab]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && isOpen) {
        e.preventDefault();
        e.stopPropagation();
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
      window.addEventListener("keydown", handleKeyDown, { capture: true });
      document.addEventListener("keydown", handleKeyDown, { capture: true });
      requestAnimationFrame(() => {
        if (modalRef.current) {
          modalRef.current.focus();
        }
      });
    } else {
      document.body.style.overflow = "unset";
    }
    return () => {
      document.body.style.overflow = "unset";
      window.removeEventListener("keydown", handleKeyDown, { capture: true });
      document.removeEventListener("keydown", handleKeyDown, { capture: true });
    };
  }, [isOpen, onClose]);

  const filteredShortcuts = useMemo(() => {
    const q = normalizeVietnamese(searchQuery.trim());
    return SHORTCUTS_DATA.filter((item) => {
      const matchCat =
        activeCategory === "all" || item.category === activeCategory;
      if (!q) return matchCat;
      const matchDesc = normalizeVietnamese(item.description).includes(q);
      const matchKeys = item.keys.some((k) => normalizeVietnamese(k).includes(q));
      const matchBadge = item.badge
        ? normalizeVietnamese(item.badge).includes(q)
        : false;
      return matchCat && (matchDesc || matchKeys || matchBadge);
    });
  }, [activeCategory, searchQuery]);

  if (!isOpen) return null;

  return (
    <div
      data-testid="keyboard-shortcuts-modal"
      role="dialog"
      aria-modal="true"
      aria-labelledby="shortcuts-guide-title"
      className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 bg-black/55 backdrop-blur-xs swms-animate-fade-in"
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <div
        ref={modalRef}
        tabIndex={-1}
        className="relative w-full max-w-4xl max-h-[90vh] flex flex-col rounded-2xl bg-[var(--surface-card)] border border-[var(--border-calm)] shadow-elevation-3 overflow-hidden text-[var(--text-primary)] outline-none swms-animate-scale-in"
      >
        {/* Header */}
        <div className="flex items-start justify-between p-5 border-b border-[var(--border-calm)] bg-[var(--surface-elevated)]/60">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-[var(--accent-subtle)] text-[var(--accent-text)] flex items-center justify-center shrink-0 border border-[var(--accent)]/20">
              <Keyboard className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="text-[11px] font-bold uppercase tracking-wider text-[var(--accent-text)] bg-[var(--accent-subtle)] px-2 py-0.5 rounded-full border border-[var(--accent)]/20">
                  Học nhanh bằng phím
                </span>
                <span className="text-xs text-[var(--text-tertiary)]">•</span>
                <span className="text-xs text-[var(--text-secondary)] font-medium">
                  Bảng tra cứu &amp; hướng dẫn
                </span>
              </div>
              <h2
                id="shortcuts-guide-title"
                className="text-lg sm:text-xl font-bold tracking-tight text-[var(--text-primary)] mt-0.5"
              >
                Hướng dẫn phím tắt &amp; luồng học tập nhanh
              </h2>
            </div>
          </div>
          <button
            type="button"
            data-testid="close-shortcuts-modal"
            onClick={onClose}
            aria-label="Đóng"
            className="p-1.5 rounded-lg text-[var(--text-tertiary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition cursor-pointer"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* 4-Step Continuous Study Flow Banner */}
        <div className="px-5 py-3.5 bg-[var(--accent-subtle)]/35 border-b border-[var(--border-calm)]">
          <div className="text-[11px] font-bold uppercase tracking-wider text-[var(--text-secondary)] mb-2">
            VÒNG LẶP HỌC TẬP HIỆU QUẢ
          </div>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-2.5">
            <div className="flex items-center gap-2 p-2 rounded-xl bg-[var(--surface-card)] border border-[var(--border-calm)] shadow-elevation-1">
              <div className="w-6 h-6 rounded-lg bg-[var(--accent-subtle)] text-[var(--accent-text)] flex items-center justify-center font-bold text-xs shrink-0">
                1
              </div>
              <div className="min-w-0">
                <div className="text-xs font-semibold truncate text-[var(--text-primary)]">
                  Đọc tập trung (Alt+Z)
                </div>
                <div className="text-[10px] text-[var(--text-secondary)] truncate">
                  Khung đọc chuẩn 68 ký tự
                </div>
              </div>
            </div>

            <div className="flex items-center gap-2 p-2 rounded-xl bg-[var(--surface-card)] border border-[var(--border-calm)] shadow-elevation-1">
              <div className="w-6 h-6 rounded-lg bg-[var(--streak-subtle)] text-[var(--streak-accent)] flex items-center justify-center font-bold text-xs shrink-0">
                2
              </div>
              <div className="min-w-0">
                <div className="text-xs font-semibold truncate text-[var(--text-primary)]">
                  Tìm &amp; Tạo thẻ (Ctrl+K)
                </div>
                <div className="text-[10px] text-[var(--text-secondary)] truncate">
                  Tạo thẻ ghi nhớ tức thì
                </div>
              </div>
            </div>

            <div className="flex items-center gap-2 p-2 rounded-xl bg-[var(--surface-card)] border border-[var(--border-calm)] shadow-elevation-1">
              <div className="w-6 h-6 rounded-lg bg-[var(--success-subtle)] text-[var(--success-accent)] flex items-center justify-center font-bold text-xs shrink-0">
                3
              </div>
              <div className="min-w-0">
                <div className="text-xs font-semibold truncate text-[var(--text-primary)]">
                  Ghi chú &amp; Biên tập (Ctrl+S)
                </div>
                <div className="text-[10px] text-[var(--text-secondary)] truncate">
                  Lưu nhanh bài giảng
                </div>
              </div>
            </div>

            <div className="flex items-center gap-2 p-2 rounded-xl bg-[var(--surface-card)] border border-[var(--border-calm)] shadow-elevation-1">
              <div className="w-6 h-6 rounded-lg bg-[var(--accent-subtle)] text-[var(--accent-text)] flex items-center justify-center font-bold text-xs shrink-0">
                4
              </div>
              <div className="min-w-0">
                <div className="text-xs font-semibold truncate text-[var(--text-primary)]">
                  FSRS Ôn tập (Space, 1-4)
                </div>
                <div className="text-[10px] text-[var(--text-secondary)] truncate">
                  Lặp lại ngắt quãng
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Filter Controls & Search */}
        <div className="p-4 px-5 border-b border-[var(--border-calm)] flex flex-col sm:flex-row items-center justify-between gap-3 bg-[var(--surface-card)]">
          <div className="flex items-center gap-1.5 overflow-x-auto w-full sm:w-auto no-scrollbar">
            {(
              [
                { id: "all", label: "Tất cả" },
                { id: "global", label: "Toàn cục" },
                { id: "reading", label: "Học & Đọc" },
                { id: "split", label: "Ghi chú & Studio" },
                { id: "review", label: "Ôn tập & Luyện tập" }
              ] as const
            ).map((cat) => (
              <button
                key={cat.id}
                type="button"
                onClick={() => setActiveCategory(cat.id)}
                className={`px-3 py-1.5 rounded-lg text-xs font-medium transition cursor-pointer whitespace-nowrap ${
                  activeCategory === cat.id
                    ? "bg-[var(--accent)] text-white dark:text-[#0F0F12] shadow-xs font-semibold"
                    : "text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]"
                }`}
              >
                {cat.label}
              </button>
            ))}
          </div>

          <div className="relative w-full sm:w-64">
            <Search className="w-3.5 h-3.5 absolute left-3 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)]" />
            <input
              type="text"
              placeholder="Tìm phím tắt hoặc tính năng..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-3 py-1.5 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:ring-1 focus:ring-[var(--ring)]"
            />
          </div>
        </div>

        {/* Shortcuts List Body */}
        <div className="p-5 overflow-y-auto space-y-2 text-xs max-h-[46vh] swms-thin-scrollbar">
          {filteredShortcuts.length === 0 ? (
            <div className="py-12 text-center text-[var(--text-tertiary)]">
              Không tìm thấy phím tắt phù hợp với từ khóa &ldquo;{searchQuery}&rdquo;
            </div>
          ) : (
            filteredShortcuts.map((item) => (
              <div
                key={item.id}
                className="flex items-center justify-between p-2.5 px-3.5 rounded-xl border border-[var(--border-calm)] hover:bg-[var(--surface-elevated)]/70 transition gap-4"
              >
                <div className="flex items-center gap-2 min-w-0">
                  <span className="text-[var(--text-primary)] font-medium">
                    {item.description}
                  </span>
                  {item.badge && (
                    <span className="px-1.5 py-0.5 rounded text-[10px] font-bold bg-[var(--accent-subtle)] text-[var(--accent-text)] border border-[var(--accent)]/20 shrink-0">
                      {item.badge}
                    </span>
                  )}
                </div>

                <div className="flex items-center gap-1 shrink-0">
                  {item.keys.map((k, idx) => (
                    <React.Fragment key={idx}>
                      <kbd className="swms-kbd min-w-[24px]">
                        {k}
                      </kbd>
                      {idx < item.keys.length - 1 && (
                        <span className="text-[var(--text-tertiary)] text-[10px] font-bold">
                          +
                        </span>
                      )}
                    </React.Fragment>
                  ))}
                </div>
              </div>
            ))
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between p-4 px-6 border-t border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 text-xs text-[var(--text-secondary)]">
          <div>
            Gợi ý: Nhấn <kbd className="swms-kbd text-[10px]">?</kbd> ở bất kỳ đâu để mở lại bảng này.
          </div>
          <button
            type="button"
            data-testid="dismiss-shortcuts-modal"
            onClick={onClose}
            className="px-4 py-1.5 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] hover:bg-[var(--surface-elevated)] text-xs font-semibold text-[var(--text-primary)] transition cursor-pointer"
          >
            Đã hiểu
          </button>
        </div>
      </div>
    </div>
  );
}
