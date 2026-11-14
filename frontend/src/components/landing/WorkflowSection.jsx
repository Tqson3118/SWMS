import React from "react";
import { BookOpen, Lightbulb, Columns2, Brain, ArrowRight } from "lucide-react";
export function WorkflowSection() {
    const steps = [
        {
            num: "01",
            icon: BookOpen,
            iconColor: "text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-950/50",
            title: "Nạp & Đọc sâu (68ch)",
            desc: "Tải lên tài liệu PDF hoặc giáo trình hệ thống. Văn bản được căn chỉnh trong cột đọc 68 ký tự/dòng chuẩn công thái học, loại bỏ hoàn toàn hiện tượng mỏi mắt."
        },
        {
            num: "02",
            icon: Lightbulb,
            iconColor: "text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/50",
            title: "Bôi đen Ngữ nghĩa (5 Màu)",
            desc: "Phân loại tri thức ngay tại chỗ với 5 màu ngữ nghĩa (Khái niệm, Dẫn chứng, Câu hỏi, Tổng hợp, Lưu ý). Neo văn bản chuẩn W3C tự phục hồi khi cập nhật."
        },
        {
            num: "03",
            icon: Columns2,
            iconColor: "text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-950/50",
            title: "Ghi chép Song song (PKM)",
            desc: "Nhấn Ctrl+\\ để mở không gian ghi chép 3 khung (15/45/40). Nhúng thẻ trích dẫn [↗ Jump to Source] và kích hoạt cuộn đồng bộ mutex 50ms không gián đoạn."
        },
        {
            num: "04",
            icon: Brain,
            iconColor: "text-purple-600 dark:text-purple-400 bg-purple-50 dark:bg-purple-950/50",
            title: "Khắc sâu Trí nhớ (FSRS v5)",
            desc: "Lật thẻ 3D theo chu kỳ thuật toán FSRS v5 khoa học (Again, Hard, Good, Easy) và làm bài tự luận định hình nhận đánh giá rubric sâu sắc từ Gemini AI."
        }
    ];
    return (<section id="workflow" className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-20 w-full">
      <div className="text-center max-w-3xl mx-auto mb-16">
        <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-indigo-50 dark:bg-indigo-950/60 border border-indigo-200/60 dark:border-indigo-800/60 text-xs font-semibold text-indigo-700 dark:text-indigo-300 uppercase tracking-wider mb-4">
          Dòng chảy Tri thức Khép kín
        </div>
        <h2 className="text-3xl sm:text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-100 mb-4">
          Từ Tiếp nhận Đến Khắc sâu Vĩnh viễn
        </h2>
        <p className="text-base sm:text-lg text-slate-600 dark:text-slate-400">
          Quy trình 4 bước liền mạch giúp bạn chuyển hóa thụ động đọc sách thành năng lực
          chủ động tư duy và ghi nhớ dài hạn.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-6 relative">
        {steps.map((step, idx) => {
            const IconComponent = step.icon;
            return (<div key={idx} className="relative p-6 rounded-2xl border border-slate-200/80 dark:border-slate-800 bg-white dark:bg-slate-900/60 shadow-xs flex flex-col justify-between">
              <div>
                <div className="flex items-center justify-between mb-4">
                  <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${step.iconColor}`}>
                    <IconComponent className="w-5 h-5 stroke-[1.75]"/>
                  </div>
                  <span className="font-mono text-xl font-extrabold text-slate-300 dark:text-slate-700">
                    {step.num}
                  </span>
                </div>

                <h3 className="text-base font-bold text-slate-900 dark:text-slate-100 mb-2">
                  {step.title}
                </h3>
                <p className="text-xs text-slate-600 dark:text-slate-400 leading-relaxed">
                  {step.desc}
                </p>
              </div>

              {idx < steps.length - 1 && (<div className="hidden md:block absolute -right-3 top-1/2 -translate-y-1/2 z-10 text-slate-300 dark:text-slate-700">
                  <ArrowRight className="w-6 h-6 stroke-[1.5]"/>
                </div>)}
            </div>);
        })}
      </div>
    </section>);
}
