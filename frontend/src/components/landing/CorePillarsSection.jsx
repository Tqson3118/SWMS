import React from "react";
import { BookOpen, Columns2, Brain, Sparkles, CheckCircle2 } from "lucide-react";
export function CorePillarsSection() {
    return (<section id="features" data-testid="landing-pillars-section" className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20 w-full">
      <div className="text-center max-w-3xl mx-auto mb-16">
        <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-indigo-50 dark:bg-indigo-950/60 border border-indigo-200/60 dark:border-indigo-800/60 text-xs font-semibold text-indigo-700 dark:text-indigo-300 uppercase tracking-wider mb-4">
          4 Trụ cột Hệ sinh thái
        </div>
        <h2 className="text-3xl sm:text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-100 mb-4">
          Thiết kế cho Trải nghiệm Học tập Sâu sắc &amp; Liền mạch
        </h2>
        <p className="text-base sm:text-lg text-slate-600 dark:text-slate-400">
          Xóa bỏ hoàn toàn ma sát chuyển đổi giữa các ứng dụng đọc tài liệu, ghi chép
          phân mảnh và ôn thi riêng biệt.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Pillar 1: Deep Reader 68ch */}
        <div className="rounded-2xl border border-slate-200/80 dark:border-slate-800 bg-white dark:bg-slate-900/60 p-8 shadow-xs hover:shadow-sm transition-all flex flex-col justify-between">
          <div>
            <div className="w-12 h-12 rounded-xl bg-indigo-50 dark:bg-indigo-950/50 border border-indigo-100 dark:border-indigo-900 text-indigo-600 dark:text-indigo-400 flex items-center justify-center mb-6">
              <BookOpen className="w-6 h-6 stroke-[1.75]"/>
            </div>

            <div className="text-xs font-semibold uppercase tracking-wider text-indigo-600 dark:text-indigo-400 mb-1">
              Trụ cột 1 • Công thái học Đọc hiểu
            </div>
            <h3 className="text-xl font-bold text-slate-900 dark:text-slate-100 mb-3">
              Đọc sâu Công thái học (Deep Reader 68ch) &amp; Đánh dấu Ngữ nghĩa
            </h3>
            <p className="text-sm text-slate-600 dark:text-slate-400 leading-relaxed mb-6">
              Cột đọc tối ưu 68 ký tự/dòng (<code>68ch</code>) với lề đối xứng cân đối,
              triệt tiêu hiện tượng mỏi cơ mắt khi học tập và nghiên cứu tài liệu dài trong
              nhiều giờ liên tục.
            </p>

            <ul className="space-y-3 text-xs sm:text-sm text-slate-700 dark:text-slate-300">
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    5 Màu ngữ nghĩa cố định:
                  </span>{" "}
                  Khái niệm cốt lõi, Dẫn chứng, Câu hỏi nghi vấn, Ý tưởng tổng hợp và Lưu ý
                  quan trọng đạt chuẩn tương phản WCAG AA.
                </div>
              </li>
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    W3C TextQuoteSelector:
                  </span>{" "}
                  Neo văn bản bằng bộ 3 <code>prefix</code>, <code>exact</code>,{" "}
                  <code>suffix</code>, tự động phục hồi (self-healing) khi bài học cập
                  nhật.
                </div>
              </li>
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    Thanh công cụ nổi In-situ:
                  </span>{" "}
                  Xuất hiện debounced 150ms tại chỗ, bôi màu và tạo thẻ ghi nhớ trực tiếp
                  ngay trên đoạn chữ.
                </div>
              </li>
            </ul>
          </div>
        </div>

        {/* Pillar 2: Split Workspace PKM */}
        <div className="rounded-2xl border border-slate-200/80 dark:border-slate-800 bg-white dark:bg-slate-900/60 p-8 shadow-xs hover:shadow-sm transition-all flex flex-col justify-between">
          <div>
            <div className="w-12 h-12 rounded-xl bg-emerald-50 dark:bg-emerald-950/50 border border-emerald-100 dark:border-emerald-900 text-emerald-600 dark:text-emerald-400 flex items-center justify-center mb-6">
              <Columns2 className="w-6 h-6 stroke-[1.75]"/>
            </div>

            <div className="text-xs font-semibold uppercase tracking-wider text-emerald-600 dark:text-emerald-400 mb-1">
              Trụ cột 2 • Không gian Song song
            </div>
            <h3 className="text-xl font-bold text-slate-900 dark:text-slate-100 mb-3">
              Không gian Chia đôi (Split Workspace PKM) &amp; Dẫn chứng 2 Chiều
            </h3>
            <p className="text-sm text-slate-600 dark:text-slate-400 leading-relaxed mb-6">
              Bố cục 3 khung chuẩn hóa (15% Mục lục / 45% Bài đọc / 40% Trình soạn thảo
              Tiptap ProseMirror) theo ADR-007, co giãn linh hoạt bằng splitter, bảo đảm
              zero scroll ngang.
            </p>

            <ul className="space-y-3 text-xs sm:text-sm text-slate-700 dark:text-slate-300">
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    Thẻ Dẫn chứng Hai chiều:
                  </span>{" "}
                  Nhúng trích dẫn vào ghi chú; bấm <code>[↗ Jump to Source]</code> lập tức
                  cuộn bài đọc và nhấp nháy highlight bằng GSAP.
                </div>
              </li>
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    Cuộn Đồng bộ Mutex 50ms:
                  </span>{" "}
                  Đồng bộ vị trí đọc và ghi chép có cơ chế khóa chống vòng lặp đệ quy, tự
                  động nội suy mượt mà qua các đoạn không có anchor.
                </div>
              </li>
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    Tự động tạo Ghi chú (Auto-Provisioning):
                  </span>{" "}
                  Mở bài học là tự động mở trang ghi chép gắn kết, tự động lưu với Web
                  Locks chống mất dữ liệu khi mở nhiều tab.
                </div>
              </li>
            </ul>
          </div>
        </div>

        {/* Pillar 3: Native FSRS v5 */}
        <div className="rounded-2xl border border-slate-200/80 dark:border-slate-800 bg-white dark:bg-slate-900/60 p-8 shadow-xs hover:shadow-sm transition-all flex flex-col justify-between">
          <div>
            <div className="w-12 h-12 rounded-xl bg-amber-50 dark:bg-amber-950/50 border border-amber-100 dark:border-amber-900 text-amber-600 dark:text-amber-400 flex items-center justify-center mb-6">
              <Brain className="w-6 h-6 stroke-[1.75]"/>
            </div>

            <div className="text-xs font-semibold uppercase tracking-wider text-amber-600 dark:text-amber-400 mb-1">
              Trụ cột 3 • Khoa học Trí nhớ
            </div>
            <h3 className="text-xl font-bold text-slate-900 dark:text-slate-100 mb-3">
              Thuật toán FSRS v5 &amp; Hàng đợi Hợp nhất
            </h3>
            <p className="text-sm text-slate-600 dark:text-slate-400 leading-relaxed mb-6">
              Ứng dụng thuật toán Spaced Repetition thế hệ mới FSRS v5 với mô hình DSR 19
              tham số, loại bỏ hoàn toàn hiện tượng dồn ứ thẻ (ease hell) của thuật toán
              SM-2 cổ điển 40 năm tuổi.
            </p>

            <ul className="space-y-3 text-xs sm:text-sm text-slate-700 dark:text-slate-300">
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    Phản hồi Tức thì Client-side:
                  </span>{" "}
                  Tính toán stability và difficulty trực tiếp trong trình duyệt với tốc độ
                  phản hồi p99 &lt; 5ms.
                </div>
              </li>
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    Khung Thẻ Lật 3D 180°:
                  </span>{" "}
                  Thẻ 800x520 với hiệu ứng GSAP spring easing, điều khiển bằng phím Space
                  và 4 nút đánh giá Again / Hard / Good / Easy.
                </div>
              </li>
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    Hàng đợi Ôn tập Hợp nhất (Unified Queue):
                  </span>{" "}
                  Toàn bộ thẻ đến hạn được gom vào một dòng chảy duy nhất, hỗ trợ lọc theo
                  Tag linh hoạt mà không bị trói buộc vào cấu trúc Decks.
                </div>
              </li>
            </ul>
          </div>
        </div>

        {/* Pillar 4: Formative AI Practice */}
        <div className="rounded-2xl border border-slate-200/80 dark:border-slate-800 bg-white dark:bg-slate-900/60 p-8 shadow-xs hover:shadow-sm transition-all flex flex-col justify-between">
          <div>
            <div className="w-12 h-12 rounded-xl bg-purple-50 dark:bg-purple-950/50 border border-purple-100 dark:border-purple-900 text-purple-600 dark:text-purple-400 flex items-center justify-center mb-6">
              <Sparkles className="w-6 h-6 stroke-[1.75]"/>
            </div>

            <div className="text-xs font-semibold uppercase tracking-wider text-purple-600 dark:text-purple-400 mb-1">
              Trụ cột 4 • Luyện tập Định hình
            </div>
            <h3 className="text-xl font-bold text-slate-900 dark:text-slate-100 mb-3">
              Formative Practice Quiz &amp; AI Semantic Rubric
            </h3>
            <p className="text-sm text-slate-600 dark:text-slate-400 leading-relaxed mb-6">
              Cổng tự kiểm tra năng lực phi trừng phạt (zero exam anxiety), biến bài tập
              thành công cụ chẩn đoán và khắc sâu kiến thức.
            </p>

            <ul className="space-y-3 text-xs sm:text-sm text-slate-700 dark:text-slate-300">
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    Trắc nghiệm &amp; Điền khuyết Tức thì:
                  </span>{" "}
                  Làm câu hỏi MCQ (phím A-D) và thẻ Cloze tức thì (0ms, 0 AI cost) để kích
                  hoạt hồi tưởng tích cực.
                </div>
              </li>
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    Đánh giá Tự luận qua AI Rubric:
                  </span>{" "}
                  Mô hình Gemini AI chấm bài luận định tính theo thang rubric đa tiêu chí,
                  chỉ rõ luận điểm còn thiếu và gợi ý tài liệu ôn tập.
                </div>
              </li>
              <li className="flex items-start gap-2.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5"/>
                <div>
                  <span className="font-semibold text-slate-900 dark:text-slate-100">
                    Bảo toàn Bản nháp Bền vững:
                  </span>{" "}
                  Câu trả lời nháp được lưu trữ trong Zustand store, không bị mất bài làm
                  khi chuyển đổi giữa các chế độ trong workspace.
                </div>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </section>);
}
