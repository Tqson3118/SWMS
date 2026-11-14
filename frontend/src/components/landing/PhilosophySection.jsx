import React from "react";
import { ShieldCheck, Lock, Download, Compass, ArrowRight } from "lucide-react";
export function PhilosophySection({ onOpenAbout }) {
    return (<section id="philosophy" className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-20 w-full">
      <div className="rounded-3xl border border-slate-200/90 dark:border-slate-800 bg-white dark:bg-slate-900/60 p-8 sm:p-12 shadow-sm relative overflow-hidden backdrop-blur-xs">
        {/* Subtle decorative glow */}
        <div className="absolute top-0 right-0 w-72 h-72 bg-indigo-500/5 rounded-full blur-3xl pointer-events-none"/>

        <div className="relative z-10">
          <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-indigo-50 dark:bg-indigo-950/60 border border-indigo-200/60 dark:border-indigo-800/60 text-xs font-semibold text-indigo-700 dark:text-indigo-300 uppercase tracking-wider mb-4">
            Triết lý Phát triển
          </div>

          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-100 mb-4">
            Học tập Cá nhân Chủ quyền (Sovereign Learning)
          </h2>

          <p className="text-base sm:text-lg text-slate-600 dark:text-slate-300 mb-10 max-w-2xl leading-relaxed">
            Tri thức của bạn thuộc về bạn — Vĩnh viễn, Riêng tư và Độc lập. SWMS được
            xây dựng trên nền tảng tôn trọng quyền sở hữu dữ liệu tuyệt đối của người học.
          </p>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10">
            {/* Principle 1 */}
            <div className="p-5 rounded-2xl bg-slate-50/70 dark:bg-slate-800/40 border border-slate-200/60 dark:border-slate-700/60">
              <div className="w-9 h-9 rounded-lg bg-indigo-100 dark:bg-indigo-900/60 text-indigo-600 dark:text-indigo-400 flex items-center justify-center mb-4">
                <ShieldCheck className="w-5 h-5 stroke-[2]"/>
              </div>
              <h3 className="text-sm font-bold text-slate-900 dark:text-slate-100 mb-2">
                Quyền sở hữu Tối cao
              </h3>
              <p className="text-xs text-slate-600 dark:text-slate-400 leading-relaxed">
                Người học là chủ thể trung tâm. Mọi tài liệu PDF cá nhân tải lên, ghi chép
                và tiến trình học tập được bảo mật tuyệt đối, không chia sẻ với trường lớp.
              </p>
            </div>

            {/* Principle 2 */}
            <div className="p-5 rounded-2xl bg-slate-50/70 dark:bg-slate-800/40 border border-slate-200/60 dark:border-slate-700/60">
              <div className="w-9 h-9 rounded-lg bg-rose-100 dark:bg-rose-900/60 text-rose-600 dark:text-rose-400 flex items-center justify-center mb-4">
                <Lock className="w-5 h-5 stroke-[2]"/>
              </div>
              <h3 className="text-sm font-bold text-slate-900 dark:text-slate-100 mb-2">
                Không Khóa Dữ liệu
              </h3>
              <p className="text-xs text-slate-600 dark:text-slate-400 leading-relaxed">
                Xuất toàn bộ ghi chú ra file Markdown chuẩn (kèm Frontmatter) và xuất thẻ
                flashcard sang Anki bất kỳ lúc nào. Không bao giờ bị ép phụ thuộc vào nền
                tảng.
              </p>
            </div>

            {/* Principle 3 */}
            <div className="p-5 rounded-2xl bg-slate-50/70 dark:bg-slate-800/40 border border-slate-200/60 dark:border-slate-700/60">
              <div className="w-9 h-9 rounded-lg bg-emerald-100 dark:bg-emerald-900/60 text-emerald-600 dark:text-emerald-400 flex items-center justify-center mb-4">
                <Download className="w-5 h-5 stroke-[2]"/>
              </div>
              <h3 className="text-sm font-bold text-slate-900 dark:text-slate-100 mb-2">
                Kiến trúc Ngoại tuyến
              </h3>
              <p className="text-xs text-slate-600 dark:text-slate-400 leading-relaxed">
                Tiếp tục học tập và ôn thẻ FSRS bình thường khi mất kết nối Internet. Cơ
                chế Web Locks và hàng đợi IndexedDB bảo vệ bạn khỏi mất mát dữ liệu.
              </p>
            </div>
          </div>

          {onOpenAbout && (<div className="flex items-center justify-start">
              <button type="button" onClick={onOpenAbout} data-testid="about-swms-trigger" className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl border border-indigo-200 dark:border-indigo-800 bg-indigo-50/50 dark:bg-indigo-950/40 text-indigo-700 dark:text-indigo-300 text-sm font-medium hover:bg-indigo-100/50 dark:hover:bg-indigo-900/40 transition-colors shadow-2xs cursor-pointer">
                <Compass className="w-4 h-4 text-indigo-600 dark:text-indigo-400"/>
                <span>Tìm hiểu về Triết lý &amp; Mô hình Hybrid Persona</span>
                <ArrowRight className="w-3.5 h-3.5 ml-1"/>
              </button>
            </div>)}
        </div>
      </div>
    </section>);
}
