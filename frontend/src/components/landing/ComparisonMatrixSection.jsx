import React from "react";
import { Check, X } from "lucide-react";
export function ComparisonMatrixSection() {
    const comparisonData = [
        {
            criterion: "Dòng làm việc (Workflow)",
            traditional: "4 công cụ riêng biệt (PDF + Notion + Anki + LMS rời rạc). Phải copy/paste thủ công, phân mảnh 4 tài khoản và tiêu tốn năng lượng tinh thần.",
            swms: "Một không gian khép kín duy nhất: Đọc 68ch ↔ Ghi chép song song ↔ Ôn tập FSRS ↔ Luyện thi Formative Quiz mà không cần rời màn hình.",
            highlight: true
        },
        {
            criterion: "Bảo toàn Ngữ cảnh (Anchoring)",
            traditional: "Trích dẫn bị đứt gãy hoàn toàn; không rõ thẻ flashcard hay đoạn ghi chú bắt nguồn từ dòng nào, trang nào của tài liệu.",
            swms: "Neo ngữ cảnh vĩnh viễn với chuẩn W3C Quotes & tọa độ PDF; bấm [↗ Jump to Source] lập tức cuộn và nhấp nháy đoạn văn bản gốc.",
            highlight: true
        },
        {
            criterion: "Công thái học Đọc (Ergonomics)",
            traditional: "PDF cố định chữ nhỏ tràn màn hình hoặc web quá rộng (120ch+) gây mỏi cơ mắt sau 30 phút đọc (saccade fatigue).",
            swms: "Cột đọc công thái học 68–75ch căn giữa với lề 426px cân xứng; thanh công cụ nổi debounced 150ms tại chỗ.",
            highlight: false
        },
        {
            criterion: "Hệ thống Đánh dấu (Highlights)",
            traditional: "Màu sắc tùy hứng, không mang ý nghĩa nhận thức, dễ tạo cảm giác học vẹt và tô màu thẩm mỹ thụ động.",
            swms: "5 màu ngữ nghĩa chuẩn hóa (Khái niệm, Dẫn chứng, Nghi vấn, Tổng hợp, Lưu ý) kèm icon phân loại và đạt chuẩn WCAG AA.",
            highlight: false
        },
        {
            criterion: "Kiến trúc Ghi chú (PKM)",
            traditional: "Notion lưu từng block thành dòng DB nặng nề gây nghẽn khóa ghi; hoặc Markdown rời rạc thiếu neo ngữ cảnh.",
            swms: "Sovereign Freeform Notebook với Single Tiptap JSONB AST nén hiệu năng cao, liên kết hai chiều [[wikilinks]].",
            highlight: false
        },
        {
            criterion: "Khoa học Trí nhớ (Spaced Repetition)",
            traditional: "Thuật toán SM-2 từ năm 1987 của Anki dễ gây dồn ứ thẻ (ease hell), tốn hàng chục giờ tạo thẻ thủ công.",
            swms: "Thuật toán FSRS v5 tiên tiến (DSR 19 tham số), tính toán client-side < 5ms, hàng đợi hợp nhất theo Tag.",
            highlight: true
        },
        {
            criterion: "Đánh giá Năng lực (Assessment)",
            traditional: "Thi cử trừng phạt, trắc nghiệm thụ động lấy điểm số máy móc không có phản hồi định tính về lỗ hổng tư duy.",
            swms: "Practice Quiz định hình phi trừng phạt: Trắc nghiệm, Cloze và Bài luận tự luận có Gemini AI chấm theo rubric ngữ nghĩa chi tiết.",
            highlight: false
        }
    ];
    const competitors = [
        {
            name: "Readwise Reader",
            insight: "SWMS học tập cơ chế trích xuất in-situ, nhưng nâng cấp hơn nhờ tích hợp sẵn trình soạn thảo Notebook song song và thuật toán FSRS v5 bản địa."
        },
        {
            name: "RemNote",
            insight: "SWMS kế thừa cú pháp Cloze và phím tắt C, nhưng thay vì ép theo cây outliner gò bó, SWMS cung cấp trình soạn thảo tài liệu phong phú ProseMirror tự do."
        },
        {
            name: "Obsidian",
            insight: "SWMS kế thừa liên kết hai chiều [[wikilinks]] và quyền sở hữu dữ liệu, nhưng không bắt người dùng cài đặt hàng chục plugin cộng đồng phức tạp."
        },
        {
            name: "Notion",
            insight: "SWMS giữ trải nghiệm slash command / thân thiện nhưng từ bỏ kiến trúc lưu block thành dòng database; SWMS dùng Single JSONB AST tốc độ cao."
        },
        {
            name: "MarginNote 4",
            insight: "SWMS kế thừa tư duy thẻ là trục tri thức và tọa độ PDF, nhưng xây dựng trên nền tảng web đa nền tảng thay vì bị khóa trong hệ sinh thái Apple."
        },
        {
            name: "Heptabase",
            insight: "SWMS hướng đến sự rõ ràng trong cấu trúc tri thức, tối ưu hóa tối đa cho việc thi cử và ghi nhớ dài hạn qua FSRS thay vì chỉ tư duy bảng trắng."
        }
    ];
    return (<section id="comparison" data-testid="landing-comparison-matrix" className="w-full max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-20 box-border overflow-hidden">
      <div className="text-center max-w-3xl mx-auto mb-16">
        <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-indigo-50 dark:bg-indigo-950/60 border border-indigo-200/60 dark:border-indigo-800/60 text-xs font-semibold text-indigo-700 dark:text-indigo-300 uppercase tracking-wider mb-4">
          Bảng So sánh Giá trị Vượt trội
        </div>
        <h2 className="text-3xl sm:text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-100 mb-4">
          Tại sao SWMS Vượt trội hơn Quy trình Rời rạc?
        </h2>
        <p className="text-base sm:text-lg text-slate-600 dark:text-slate-400">
          Giảm 75% ma sát nhận thức và xóa bỏ hoàn toàn nguy cơ thất lạc trích dẫn gốc
          khi học tập chuyên sâu.
        </p>
      </div>

      {/* Comparison Table */}
      <div className="rounded-2xl border border-slate-200/90 dark:border-slate-800 bg-white dark:bg-slate-900/60 shadow-xs overflow-hidden mb-16 max-w-full">
        <div className="overflow-x-auto w-full">
          <table className="w-full text-left text-sm min-w-[640px]">
            <thead className="bg-slate-50/80 dark:bg-slate-950/80 border-b border-slate-200/80 dark:border-slate-800">
              <tr>
                <th className="py-4 px-5 font-semibold text-slate-600 dark:text-slate-300 w-1/4">
                  Tiêu chí So sánh
                </th>
                <th className="py-4 px-5 font-semibold text-slate-500 dark:text-slate-400 w-[37.5%]">
                  <div className="flex items-center gap-1.5">
                    <X className="w-4 h-4 text-rose-500 shrink-0"/>
                    <span>Quy trình Rời rạc (PDF + Notion + Anki + LMS)</span>
                  </div>
                </th>
                <th className="py-4 px-5 font-semibold text-indigo-600 dark:text-indigo-400 w-[37.5%] bg-indigo-50/40 dark:bg-indigo-950/20">
                  <div className="flex items-center gap-1.5">
                    <Check className="w-4 h-4 text-emerald-500 shrink-0"/>
                    <span>Hệ sinh thái Thống nhất SWMS</span>
                  </div>
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200/70 dark:divide-slate-800/70">
              {comparisonData.map((row, idx) => (<tr key={idx} className={`hover:bg-slate-50/50 dark:hover:bg-slate-800/40 transition-colors ${row.highlight
                ? "bg-indigo-50/15 dark:bg-indigo-950/10 font-normal"
                : ""}`}>
                  <td className="py-4 px-5 font-medium text-slate-900 dark:text-slate-100 align-top">
                    {row.criterion}
                  </td>
                  <td className="py-4 px-5 text-slate-600 dark:text-slate-400 leading-relaxed align-top">
                    {row.traditional}
                  </td>
                  <td className="py-4 px-5 text-slate-800 dark:text-slate-200 leading-relaxed bg-indigo-50/30 dark:bg-indigo-950/15 align-top">
                    {row.swms}
                  </td>
                </tr>))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Competitor Benchmark Matrix Cards */}
      <div className="space-y-6">
        <h3 className="text-xl font-bold tracking-tight text-slate-900 dark:text-slate-100 text-center">
          Đúc kết Tinh hoa từ 6 Sản phẩm Hàng đầu Thế giới
        </h3>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {competitors.map((item, idx) => (<div key={idx} className="p-5 rounded-xl border border-slate-200/80 dark:border-slate-800 bg-white/70 dark:bg-slate-900/40 hover:border-indigo-200 dark:hover:border-indigo-800 transition-colors shadow-2xs">
              <div className="text-xs font-semibold uppercase tracking-wider text-indigo-600 dark:text-indigo-400 mb-2 flex items-center justify-between">
                <span>vs. {item.name}</span>
                <span className="text-[10px] text-slate-400">Benchmark</span>
              </div>
              <p className="text-xs text-slate-600 dark:text-slate-300 leading-relaxed">
                {item.insight}
              </p>
            </div>))}
        </div>
      </div>
    </section>);
}
