import React from "react";
import { Keyboard, ArrowRight } from "lucide-react";
export function ShortcutsTeaserSection({ onOpenShortcuts }) {
    const shortcuts = [
        {
            combo: ["Ctrl", "K"],
            action: "Command Palette & Tìm kiếm nhanh",
            context: "Toàn cục"
        },
        {
            combo: ["Ctrl", "\\"],
            action: "Bật / Tắt Chế độ Chia đôi Split Workspace",
            context: "Workspace"
        },
        {
            combo: ["Space"],
            action: "Lật thẻ ghi nhớ 3D 180°",
            context: "Ôn tập FSRS"
        },
        {
            combo: ["1", "2", "3", "4"],
            action: "Đánh giá Again, Hard, Good, Easy",
            context: "Ôn tập FSRS"
        },
        {
            combo: ["C"],
            action: "Tạo nhanh thẻ Cloze từ đoạn bôi đen",
            context: "Chế độ đọc"
        }
    ];
    return (<section className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-16 text-center w-full">
      <div className="max-w-2xl mx-auto mb-10">
        <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider mb-3">
          <Keyboard className="w-3.5 h-3.5"/>
          <span>Học ở Tốc độ Tư duy</span>
        </div>
        <h2 className="text-2xl sm:text-3xl font-bold tracking-tight text-slate-900 dark:text-slate-100 mb-3">
          Điều khiển Toàn diện qua Hệ thống Phím tắt
        </h2>
        <p className="text-sm text-slate-600 dark:text-slate-400">
          Tối ưu hóa năng suất học tập, chuyển đổi chế độ và tương tác với thẻ ghi nhớ mà
          không cần rời tay khỏi bàn phím.
        </p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3 max-w-4xl mx-auto mb-10">
        {shortcuts.map((sc, idx) => (<div key={idx} className="p-3.5 rounded-xl border border-slate-200/80 dark:border-slate-800 bg-white dark:bg-slate-900/60 shadow-2xs flex flex-col justify-between items-center text-center">
            <div className="flex items-center justify-center gap-1 mb-2">
              {sc.combo.map((k, kIdx) => (<kbd key={kIdx} className="swms-kbd text-xs">
                  {k}
                </kbd>))}
            </div>
            <div className="text-xs font-medium text-slate-800 dark:text-slate-200 mb-1 leading-snug">
              {sc.action}
            </div>
            <span className="text-[10px] text-slate-400 dark:text-slate-500">
              {sc.context}
            </span>
          </div>))}
      </div>

      {onOpenShortcuts && (<button type="button" onClick={onOpenShortcuts} data-testid="keyboard-shortcuts-trigger" className="inline-flex items-center gap-2 px-6 py-3 rounded-xl bg-slate-900 hover:bg-slate-800 dark:bg-slate-100 dark:hover:bg-white text-white dark:text-slate-900 text-sm font-medium shadow-xs transition-all active:scale-[0.98] cursor-pointer">
          <Keyboard className="w-4 h-4"/>
          <span>Mở Toàn bộ Danh mục Phím tắt (Bấm phím ?)</span>
          <ArrowRight className="w-3.5 h-3.5 ml-1"/>
        </button>)}
    </section>);
}
