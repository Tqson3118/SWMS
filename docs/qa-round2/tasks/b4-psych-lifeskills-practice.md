Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ B4-PSYCH-LIFESKILLS-PRACTICE: Xây dựng Module Thực Hành Tâm Lý Học & Kỹ Năng Sống Chuyên Sâu

## Quy tắc phân vùng chạy song song (BẮT BUỘC)
- **PHẠM VI ĐƯỢC PHÉP TẠO / SỬA:**
  - CHỈ tạo/sửa file: `frontend/src/components/learn/practice/PsychologyLifeSkillsLab.tsx`
  - Và ghi báo cáo ngắn vào: `docs/qa-round2/B4-psych-lifeskills-summary.md`
- **TUYỆT ĐỐI KHÔNG** sửa bất kỳ file nào khác trong `frontend/` hoặc `backend/` (đặc biệt KHÔNG chạm vào `LessonPracticeLab.tsx`, `LessonMarkdownView.tsx`, `tsconfig.tsbuildinfo`).
- **KHÔNG chạy `git commit`**, **KHÔNG restart server**, **KHÔNG để lại background task**.
- Kiểm tra TypeScript bằng: `cd frontend; npx tsc --noEmit --incremental false` (exit code `0`).

## Chuẩn Props & Giao diện
Export component `PsychologyLifeSkillsLab` nhận props chuẩn khớp với `LessonPracticeLab.tsx`:
```ts
export interface PracticeModuleProps {
  courseId: string;
  courseTitle: string;
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  onSaveToNotebook: (textToAppend: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
}
```
- **Ngôn ngữ hiển thị trên UI:** Tiếng Việt gần gũi, thấu cảm, thực tế, dễ áp dụng ngay vào đời sống (tránh văn phong lý thuyết khô cứng).
- **Thiết kế "Clean & Calm Learning":** Tương thích Light Mode, Dark Mode, Mobile `390×844` (tap target `min-h-[44px]`, bong bóng chat gọn đẹp).

## 3 Chế độ thực hành chuyên sâu cần xây dựng trong `PsychologyLifeSkillsLab.tsx`

### Chế độ 1: Mô Phỏng Hội Thoại Thực Tế (Interactive Roleplay)
- Xây dựng 4–5 kịch bản hội thoại phân nhánh (2–3 lượt qua lại cho mỗi tình huống) bám sát khóa `Kỹ Năng Sống & Giao Tiếp Hiệu Quả` và `Tâm Lý Học`:
  1. **Góp ý khi bạn cùng nhóm / đồng nghiệp trễ hạn** mà không gây phòng thủ.
  2. **Từ chối lời nhờ vả quá sức** một cách lịch sự nhưng giữ vững ranh giới.
  3. **Xử lý khi nhận lời chê bai gay gắt** (tách biệt cảm xúc tức thời để phản hồi bình tĩnh).
  4. **Lắng nghe người thân đang stress** (lắng nghe đồng cảm thay vì vội vàng giảng giải/khuyên bảo).
- Ở mỗi lượt, người học chọn 1 trong 3–4 câu trả lời → giao diện chat hiển thị **phản ứng thực tế của đối phương**, chấm điểm 2 tiêu chí (**Sự tôn trọng/Ranh giới** & **Mức độ đồng cảm**), và giải thích theo 4 bước giao tiếp rõ ràng: *Nêu sự việc → Nói cảm xúc thật → Nêu nhu cầu → Đưa đề nghị cụ thể*.

### Chế độ 2: Nhận Diện Bẫy Tâm Lý Hằng Ngày (Cognitive Bias Detector)
- Bộ 6–8 tình huống thực tế về cách bộ não hay đánh lừa chúng ta:
  - *Thiên kiến xác nhận* (chỉ tìm thông tin hợp ý mình), *Ngụy biện chi phí chìm* (tiếc tiền/công sức đã bỏ ra nên cố đâm lao), *Hiệu ứng mỏ neo* (bị giá niêm yết đầu tiên chi phối), *Hiệu ứng Dunning-Kruger*, *Áp lực đám đông*, *Tư duy Nhanh (Hệ thống 1) vs Tư duy Chậm (Hệ thống 2)*.
- Người học đọc tình huống → chọn bẫy tâm lý đang diễn ra → xem phân tích cơ chế não bộ và **1 mẹo hành động ngay** để không sập bẫy lần sau. Có nút **"Tạo thẻ ghi nhớ bẫy tâm lý này"** (`onCreateFlashcardQuick`).

### Chế độ 3: Gỡ Rối Suy Nghĩ & Cân Bằng Cảm Xúc (Interactive Reflection Card)
- Công cụ thực hành CBT / Tự phản chiếu 5 bước để người học tự áp dụng cho vấn đề của chính mình ngay lúc đang học:
  1. **Sự việc vừa xảy ra là gì?** (chỉ ghi lại sự thật khách quan, không phán xét)
  2. **Suy nghĩ đầu tiên nảy ra trong đầu?** + Chọn nhanh cảm xúc & mức độ căng thẳng (`1–10`)
  3. **Bằng chứng ủng hộ vs. Bằng chứng phản bác suy nghĩ đó?**
  4. **Nếu một người bạn thân gặp chuyện này, bạn sẽ khuyên họ thế nào?** (Góc nhìn bình tĩnh hơn)
  5. **1 việc nhỏ làm được ngay trong 5 phút tới**
- Kèm nút **"Xuất & Lưu bản gỡ rối vào Sổ tay"** (`onSaveToNotebook`) với định dạng Markdown sạch đẹp.

## Kiểm chứng
1. Chạy `cd frontend; npx tsc --noEmit --incremental false` → bắt buộc exit code `0`.
2. Ghi báo cáo ngắn vào `docs/qa-round2/B4-psych-lifeskills-summary.md` nêu rõ 3 chế độ, danh sách kịch bản, kết quả `tsc` và mục **Chưa kiểm chứng**.
