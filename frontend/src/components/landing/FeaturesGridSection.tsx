import React from "react";
import {
  Search,
  Bot,
  Anchor,
  HardDrive,
  Download,
  Link as LinkIcon,
  Sparkles,
  Zap
} from "lucide-react";

export function FeaturesGridSection() {
  const extendedFeatures = [
    {
      icon: Search,
      iconColor: "text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-950/60",
      title: "Command Palette & Quick Capture (Ctrl+K)",
      desc: "Tìm kiếm toàn cục bài học, tài liệu PDF, tag và tạo nhanh thẻ flashcard hoặc ghi chú mà không cần rời tay khỏi bàn phím."
    },
    {
      icon: Bot,
      iconColor: "text-purple-600 dark:text-purple-400 bg-purple-50 dark:bg-purple-950/60",
      title: "AI Ghostreader & Formative Rubric",
      desc: "Giải thích khái niệm khó bằng ngôn ngữ trực quan in-situ và phân tích bài luận tự luận theo thang rubric nhận thức đa tầng."
    },
    {
      icon: Anchor,
      iconColor: "text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-950/60",
      title: "W3C TextQuoteSelector & Self-Healing",
      desc: "Neo giữ vết highlight bằng bộ 3 prefix, exact, suffix; thuật toán fuzzy matching tự động khôi phục tọa độ khi bài học cập nhật."
    },
    {
      icon: HardDrive,
      iconColor: "text-sky-600 dark:text-sky-400 bg-sky-50 dark:bg-sky-950/60",
      title: "Web Locks & Offline IndexedDB Queue",
      desc: "Học tập không gián đoạn khi offline. Cơ chế Web Locks và Optimistic Concurrency bảo vệ chống xung đột đa tab (409 Keep-Both)."
    },
    {
      icon: Download,
      iconColor: "text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/60",
      title: "Anti-Lockin: Xuất Markdown & Anki 1-Click",
      desc: "Xuất toàn bộ ghi chú sang Standard Markdown kèm YAML Frontmatter và xuất thẻ sang Anki .apkg bất kỳ lúc nào."
    },
    {
      icon: LinkIcon,
      iconColor: "text-rose-600 dark:text-rose-400 bg-rose-50 dark:bg-rose-950/60",
      title: "Bi-directional Wikilinks [[Liên kết 2 chiều]]",
      desc: "Kết nối tri thức đa chiều giữa các trang ghi chú và đoạn đọc; xây dựng đồ thị tri thức cá nhân bền vững."
    }
  ];

  return (
    <section
      data-testid="landing-features-grid"
      className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20 w-full"
    >
      <div className="text-center max-w-3xl mx-auto mb-16">
        <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-indigo-50 dark:bg-indigo-950/60 border border-indigo-200/60 dark:border-indigo-800/60 text-xs font-semibold text-indigo-700 dark:text-indigo-300 uppercase tracking-wider mb-4">
          Bộ Tính năng Mở rộng
        </div>
        <h2 className="text-3xl sm:text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-100 mb-4">
          Công cụ Chuyên sâu cho Người học Nghiêm túc
        </h2>
        <p className="text-base sm:text-lg text-slate-600 dark:text-slate-400">
          Từng chi tiết kiến trúc đều được tối ưu hóa để hỗ trợ các kịch bản học tập phức tạp
          và duy trì sự tĩnh lặng của tâm trí.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {extendedFeatures.map((feat, idx) => {
          const IconComp = feat.icon;
          return (
            <div
              key={idx}
              className="p-6 rounded-2xl border border-slate-200/80 dark:border-slate-800 bg-white dark:bg-slate-900/60 shadow-xs hover:border-indigo-200 dark:hover:border-indigo-900/60 transition-all flex flex-col justify-between"
            >
              <div>
                <div className={`w-11 h-11 rounded-xl flex items-center justify-center mb-5 ${feat.iconColor}`}>
                  <IconComp className="w-5 h-5 stroke-[1.75]" />
                </div>
                <h3 className="text-base font-bold text-slate-900 dark:text-slate-100 mb-2">
                  {feat.title}
                </h3>
                <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-400 leading-relaxed">
                  {feat.desc}
                </p>
              </div>
            </div>
          );
        })}
      </div>
    </section>
  );
}
