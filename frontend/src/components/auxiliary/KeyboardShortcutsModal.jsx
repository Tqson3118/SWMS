"use client";
import React, { useEffect, useState, useMemo, useRef } from "react";
import { Keyboard, Search, X } from "lucide-react";
const SHORTCUTS_DATA = [
    // Global & Navigation
    {
        id: "g1",
        category: "global",
        keys: ["Ctrl", "K"],
        description: "Mở Bảng lệnh Command Palette & Quick Capture (Tìm kiếm bài học, tài liệu, tag)",
        badge: "Phổ biến"
    },
    {
        id: "g2",
        category: "global",
        keys: ["?"],
        description: "Mở Bảng tra cứu phím tắt & Hướng dẫn luồng học tập này",
        badge: "Trợ giúp"
    },
    {
        id: "g3",
        category: "global",
        keys: ["Esc"],
        description: "Đóng bất kỳ Modal, Drawer hoặc Popover đang mở"
    },
    {
        id: "g4",
        category: "global",
        keys: ["0"],
        description: "Chuyển về Không gian cá nhân (Dashboard)"
    },
    {
        id: "g5",
        category: "global",
        keys: ["1"],
        description: "Chuyển sang Mode 1: Đọc sâu (Deep Reading 68ch)"
    },
    {
        id: "g6",
        category: "global",
        keys: ["2"],
        description: "Chuyển sang Mode 2: Chia đôi không gian (Split PKM)"
    },
    {
        id: "g7",
        category: "global",
        keys: ["3"],
        description: "Chuyển sang Mode 3: Ôn tập FSRS & Luyện tập Thực hành"
    },
    {
        id: "g8",
        category: "global",
        keys: ["Ctrl", "\\"],
        description: "Thu gọn hoặc Mở rộng Sidebar mục lục / tài nguyên"
    },
    {
        id: "g9",
        category: "global",
        keys: ["Ctrl", "D"],
        description: "Chuyển đổi giao diện Sáng / Tối (Light / Dark mode)"
    },
    // Mode 1: Reading
    {
        id: "r1",
        category: "reading",
        keys: ["j", "/", "k"],
        description: "Cuộn văn bản mượt mà xuống / lên theo nhịp đọc mắt"
    },
    {
        id: "r2",
        category: "reading",
        keys: ["1-5"],
        description: "Bôi đen và Đọc: Phím 1-5 đánh dấu 5 màu ngữ nghĩa (Khái niệm, Dẫn chứng, Nghi vấn, Tổng hợp, Lưu ý)",
        badge: "Highlight"
    },
    {
        id: "r3",
        category: "reading",
        keys: ["C"],
        description: "Tạo thẻ Cloze điền khuyết nhanh từ đoạn bôi đen (0ms, 0 AI cost)",
        badge: "0ms AI"
    },
    {
        id: "r4",
        category: "reading",
        keys: ["N"],
        description: "Trích xuất đoạn bôi đen sang Ghi chú tổng hợp và tự động mở Split Mode"
    },
    {
        id: "r5",
        category: "reading",
        keys: ["E"],
        description: "Gọi trợ lý AI giải thích nhanh thuật ngữ hoặc khái niệm"
    },
    {
        id: "r6",
        category: "reading",
        keys: ["Z"],
        description: "Bật / Tắt chế độ Zen Mode (Toàn màn hình tĩnh lặng, ẩn mọi thanh điều hướng)"
    },
    {
        id: "r7",
        category: "reading",
        keys: ["T"],
        description: "Chuyển đổi luân phiên phông chữ Serif (Source Serif 4) và Sans-serif"
    },
    // Mode 2: Split PKM
    {
        id: "s1",
        category: "split",
        keys: ["Ctrl", "\\"],
        description: "Split Mode: Thu gọn Sidebar thành thanh icon hoặc mở rộng danh mục 288px (Ctrl\\)"
    },
    {
        id: "s2",
        category: "split",
        keys: ["Double Click"],
        description: "Nhấp đúp chuột vào tay cầm Splitter để khôi phục tỷ lệ chuẩn 15% / 45% / 40%"
    },
    {
        id: "s3",
        category: "split",
        keys: ["Ctrl", "S"],
        description: "Lưu ghi chú thủ công (Hệ thống tự động lưu liên tục)"
    },
    {
        id: "s4",
        category: "split",
        keys: ["Click [↗ Jump]"],
        description: "Bấm [↗ Jump to Source]: Nhảy về dòng bài đọc gốc và kích hoạt hiệu ứng phát sáng GSAP pulse"
    },
    {
        id: "s5",
        category: "split",
        keys: ["/flashcard"],
        description: "Gõ lệnh chèn block thẻ ghi nhớ trực tiếp trong trình soạn thảo Tiptap"
    },
    {
        id: "s6",
        category: "split",
        keys: ["[["],
        description: "Gõ hai ngoặc vuông để gợi ý và chèn liên kết hai chiều (Wikilinks)"
    },
    {
        id: "s7",
        category: "split",
        keys: ["Alt", "S"],
        description: "Bật / Tắt chế độ Cuộn Đồng bộ Mutex (Scroll-Sync 50ms trailing edge)"
    },
    // Mode 3: Review & Practice
    {
        id: "v1",
        category: "review",
        keys: ["Space"],
        description: "FSRS Ôn tập: Bấm Space để lật thẻ ghi nhớ 3D 180° xem đáp án và giải thích",
        badge: "Cốt lõi"
    },
    {
        id: "v2",
        category: "review",
        keys: ["1-4"],
        description: "Phím 1-4 đánh giá FSRS: 1-Again (<10m), 2-Hard (~1.2d), 3-Good (~3.5d), 4-Easy (~8.2d)",
        badge: "FSRS v5"
    },
    {
        id: "v3",
        category: "review",
        keys: ["Z"],
        description: "Hoàn tác (Undo) lượt đánh giá thẻ vừa rồi"
    },
    {
        id: "v4",
        category: "review",
        keys: ["A", "B", "C", "D"],
        description: "Chọn nhanh phương án trong bài kiểm tra trắc nghiệm MCQ"
    },
    {
        id: "v5",
        category: "review",
        keys: ["Ctrl", "Enter"],
        description: "Nộp bài tự luận Practice Quiz để nhận phản hồi từ Gemini AI Rubric"
    }
];
export function KeyboardShortcutsModal({ isOpen, onClose, initialTab = "all" }) {
    const [activeCategory, setActiveCategory] = useState(initialTab);
    const [searchQuery, setSearchQuery] = useState("");
    const modalRef = useRef(null);
    useEffect(() => {
        setActiveCategory(initialTab);
    }, [initialTab]);
    useEffect(() => {
        const handleKeyDown = (e) => {
            if (e.key === "Escape" && isOpen) {
                onClose();
                return;
            }
            if (e.key === "Tab" && isOpen && modalRef.current) {
                const focusableElements = modalRef.current.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
                if (focusableElements.length === 0)
                    return;
                const firstEl = focusableElements[0];
                const lastEl = focusableElements[focusableElements.length - 1];
                if (e.shiftKey) {
                    if (document.activeElement === firstEl) {
                        e.preventDefault();
                        lastEl.focus();
                    }
                }
                else {
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
        }
        else {
            document.body.style.overflow = "unset";
        }
        return () => {
            document.body.style.overflow = "unset";
            window.removeEventListener("keydown", handleKeyDown);
        };
    }, [isOpen, onClose]);
    const filteredShortcuts = useMemo(() => {
        return SHORTCUTS_DATA.filter((item) => {
            const matchCat = activeCategory === "all" || item.category === activeCategory;
            if (!searchQuery.trim())
                return matchCat;
            const q = searchQuery.toLowerCase();
            const matchDesc = item.description.toLowerCase().includes(q);
            const matchKeys = item.keys.some((k) => k.toLowerCase().includes(q));
            const matchBadge = item.badge?.toLowerCase().includes(q);
            return matchCat && (matchDesc || matchKeys || matchBadge);
        });
    }, [activeCategory, searchQuery]);
    if (!isOpen)
        return null;
    return (<div data-testid="keyboard-shortcuts-modal" role="dialog" aria-modal="true" aria-labelledby="shortcuts-guide-title" className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 bg-slate-950/65 backdrop-blur-xs animate-in fade-in duration-200" onClick={(e) => {
            if (e.target === e.currentTarget)
                onClose();
        }}>
      <div ref={modalRef} className="relative w-full max-w-4xl max-h-[90vh] flex flex-col rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-2xl overflow-hidden text-slate-900 dark:text-slate-100">
        {/* Header */}
        <div className="flex items-start justify-between p-5 border-b border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-900/70">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-indigo-600/10 dark:bg-indigo-500/20 text-indigo-600 dark:text-indigo-400 flex items-center justify-center shrink-0 border border-indigo-200/50 dark:border-indigo-800/60">
              <Keyboard className="w-5 h-5"/>
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="text-[11px] font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-950/60 px-2 py-0.5 rounded-full border border-indigo-200/60 dark:border-indigo-800/60">
                  Tốc độ Tư duy
                </span>
                <span className="text-xs text-slate-400 dark:text-slate-500">•</span>
                <span className="text-xs text-slate-500 dark:text-slate-400 font-medium">Bảng Tra cứu &amp; Hướng dẫn</span>
              </div>
              <h2 id="shortcuts-guide-title" className="text-lg sm:text-xl font-bold tracking-tight text-slate-900 dark:text-slate-100 mt-0.5">
                Hướng dẫn Phím tắt &amp; Luồng Học tập Nhanh
              </h2>
            </div>
          </div>
          <button type="button" data-testid="close-shortcuts-modal" onClick={onClose} aria-label="Đóng" className="p-1.5 rounded-lg text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 transition cursor-pointer">
            <X className="w-5 h-5"/>
          </button>
        </div>

        {/* 4-Step Continuous Study Flow Banner */}
        <div className="px-5 py-3.5 bg-indigo-50/40 dark:bg-indigo-950/20 border-b border-slate-200 dark:border-slate-800">
          <div className="text-[11px] font-bold uppercase tracking-wider text-slate-500 dark:text-slate-400 mb-2">
            VÒNG LẶP HỌC TẬP KHÉP KÍN (CONTINUOUS FLOW)
          </div>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-2.5">
            <div className="flex items-center gap-2 p-2 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xs">
              <div className="w-6 h-6 rounded-lg bg-indigo-100 dark:bg-indigo-950 text-indigo-600 dark:text-indigo-400 flex items-center justify-center font-bold text-xs shrink-0">
                1
              </div>
              <div className="min-w-0">
                <div className="text-xs font-semibold truncate text-slate-900 dark:text-slate-100">Đọc sâu 68ch</div>
                <div className="text-[10px] text-slate-500 dark:text-slate-400 truncate">Công thái học</div>
              </div>
            </div>

            <div className="flex items-center gap-2 p-2 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xs">
              <div className="w-6 h-6 rounded-lg bg-amber-100 dark:bg-amber-950 text-amber-600 dark:text-amber-400 flex items-center justify-center font-bold text-xs shrink-0">
                2
              </div>
              <div className="min-w-0">
                <div className="text-xs font-semibold truncate text-slate-900 dark:text-slate-100">Bôi đen 5 màu (1-5, C)</div>
                <div className="text-[10px] text-slate-500 dark:text-slate-400 truncate">W3C TextQuotes</div>
              </div>
            </div>

            <div className="flex items-center gap-2 p-2 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xs">
              <div className="w-6 h-6 rounded-lg bg-emerald-100 dark:bg-emerald-950 text-emerald-600 dark:text-emerald-400 flex items-center justify-center font-bold text-xs shrink-0">
                3
              </div>
              <div className="min-w-0">
                <div className="text-xs font-semibold truncate text-slate-900 dark:text-slate-100">Split Chia đôi (Ctrl\)</div>
                <div className="text-[10px] text-slate-500 dark:text-slate-400 truncate">Jump to Source</div>
              </div>
            </div>

            <div className="flex items-center gap-2 p-2 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xs">
              <div className="w-6 h-6 rounded-lg bg-sky-100 dark:bg-sky-950 text-sky-600 dark:text-sky-400 flex items-center justify-center font-bold text-xs shrink-0">
                4
              </div>
              <div className="min-w-0">
                <div className="text-xs font-semibold truncate text-slate-900 dark:text-slate-100">FSRS Ôn tập (Space, 1-4)</div>
                <div className="text-[10px] text-slate-500 dark:text-slate-400 truncate">DSR 19 tham số</div>
              </div>
            </div>
          </div>
        </div>

        {/* Filter Controls & Search */}
        <div className="p-4 px-5 border-b border-slate-200 dark:border-slate-800 flex flex-col sm:flex-row items-center justify-between gap-3 bg-white dark:bg-slate-900">
          <div className="flex items-center gap-1.5 overflow-x-auto w-full sm:w-auto no-scrollbar">
            {[
            { id: "all", label: "Tất cả" },
            { id: "global", label: "Toàn cục / Nav" },
            { id: "reading", label: "Mode 1: Đọc sâu" },
            { id: "split", label: "Mode 2: Split PKM" },
            { id: "review", label: "Mode 3: Ôn tập FSRS" }
        ].map((cat) => (<button key={cat.id} type="button" onClick={() => setActiveCategory(cat.id)} className={`px-3 py-1.5 rounded-lg text-xs font-medium transition cursor-pointer whitespace-nowrap ${activeCategory === cat.id
                ? "bg-indigo-600 text-white shadow-xs"
                : "text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800"}`}>
                {cat.label}
              </button>))}
          </div>

          <div className="relative w-full sm:w-64">
            <Search className="w-3.5 h-3.5 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"/>
            <input type="text" placeholder="Tìm phím tắt hoặc tính năng..." value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} className="w-full pl-9 pr-3 py-1.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-950 text-xs text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-1 focus:ring-indigo-500"/>
          </div>
        </div>

        {/* Shortcuts List Body */}
        <div className="p-5 overflow-y-auto space-y-2 text-xs divide-y divide-slate-100 dark:divide-slate-800/60 max-h-[46vh]">
          {filteredShortcuts.length === 0 ? (<div className="py-12 text-center text-slate-400">
              Không tìm thấy phím tắt phù hợp với từ khóa "{searchQuery}"
            </div>) : (filteredShortcuts.map((item) => (<div key={item.id} className="flex items-center justify-between p-2.5 px-3.5 rounded-xl border border-slate-200/80 dark:border-slate-800 hover:bg-slate-50/80 dark:hover:bg-slate-800/40 transition gap-4">
                <div className="flex items-center gap-2 min-w-0">
                  <span className="text-slate-800 dark:text-slate-200 font-medium">
                    {item.description}
                  </span>
                  {item.badge && (<span className="px-1.5 py-0.5 rounded text-[10px] font-bold bg-indigo-50 dark:bg-indigo-950 text-indigo-600 dark:text-indigo-400 border border-indigo-200/60 dark:border-indigo-800/60 shrink-0">
                      {item.badge}
                    </span>)}
                </div>

                <div className="flex items-center gap-1 shrink-0">
                  {item.keys.map((k, idx) => (<React.Fragment key={idx}>
                      <kbd className="swms-kbd min-w-[24px]">
                        {k}
                      </kbd>
                      {idx < item.keys.length - 1 && (<span className="text-slate-400 text-[10px] font-bold">
                          +
                        </span>)}
                    </React.Fragment>))}
                </div>
              </div>)))}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between p-4 px-6 border-t border-slate-200 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-900/70 text-xs text-slate-500 dark:text-slate-400">
          <div>
            Gợi ý: Nhấn <kbd className="swms-kbd text-[10px]">?</kbd> ở bất kỳ đâu để mở lại bảng này.
          </div>
          <button type="button" data-testid="close-shortcuts-modal" onClick={onClose} className="px-4 py-1.5 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-100 dark:hover:bg-slate-700 text-xs font-semibold text-slate-700 dark:text-slate-200 transition cursor-pointer">
            Đã hiểu
          </button>
        </div>
      </div>
    </div>);
}
