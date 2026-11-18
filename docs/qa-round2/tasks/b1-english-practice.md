Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ B1-ENGLISH-PRACTICE: Xây dựng Module Thực Hành Tiếng Anh & IELTS Chuyên Sâu (100% Miễn Phí)

## Quy tắc phân vùng chạy song song (BẮT BUỘC)
- **PHẠM VI ĐƯỢC PHÉP TẠO / SỬA:**
  - CHỈ tạo/sửa file: `frontend/src/components/learn/practice/EnglishPracticeLab.tsx`
  - Và ghi báo cáo ngắn vào: `docs/qa-round2/B1-english-summary.md`
- **TUYỆT ĐỐI KHÔNG** sửa bất kỳ file nào khác trong `frontend/` hoặc `backend/` (đặc biệt KHÔNG chạm vào `LessonPracticeLab.tsx`, `LessonMarkdownView.tsx`, `tsconfig.tsbuildinfo`).
- **KHÔNG chạy `git commit`**, **KHÔNG restart server**, **KHÔNG để lại background task**.
- Kiểm tra TypeScript bằng: `cd frontend; npx tsc --noEmit --incremental false` (exit code `0`).

## Chuẩn Props & Giao diện
Export component `EnglishPracticeLab` nhận props chuẩn khớp với `LessonPracticeLab.tsx`:
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
- **Ngôn ngữ hiển thị trên UI:** Tiếng Việt tự nhiên, đời thường, dễ hiểu (không dùng từ học thuật nặng nề).
- **Thiết kế "Clean & Calm Learning":** Dùng CSS tokens (`var(--surface-card)`, `var(--surface-elevated)`, `var(--border-calm)`, `var(--text-primary)`, `var(--text-secondary)`, accent `#5E51E8` / `#818CF8`, success emerald, warning amber, danger rose). Hỗ trợ đầy đủ Light Mode, Dark Mode, và **Mobile `390×844`** (nút bấm chạm tối thiểu `min-h-[44px]`, nút mic tối thiểu `h-12`, dễ thao tác một tay).

## 4 Chế độ thực hành chuyên sâu cần xây dựng trong `EnglishPracticeLab.tsx`

### Dữ liệu tự động thích ứng theo bài học
- Viết hàm trích xuất thông minh từ `lessonMarkdown` & `lessonTitle`:
  1. Tìm các file audio `.wav` / `.mp3` được nhắc tới trong bài học (ví dụ pattern `([A-Za-z0-9_/-]+\.(?:wav|mp3))`) để phát qua `/api/v1/document-assets/IELTS/${relativePath}` (kèm fallback mượt sang Web Speech API `window.speechSynthesis` nếu không có file audio hoặc file chưa tải được).
  2. Trích xuất các câu tiếng Anh thực tế, cặp từ vựng/IPA/nghĩa tiếng Việt từ markdown bài học (nếu bài ít câu tiếng Anh thì bổ sung bộ 5 câu mẫu theo đúng chủ đề bài học: Giao tiếp hằng ngày, Nghe IELTS, Thảo luận công nghệ/tài chính).

### Chế độ 1: Luyện Phát Âm & Nhại Giọng (Shadowing + Chấm từng từ)
- Hiển thị câu tiếng Anh rõ ràng, phiên âm IPA (nếu có), nghĩa tiếng Việt, và nút **Nghe mẫu**:
  - Chọn tốc độ đọc (`0.75x`, `1.0x`, `1.25x`) và giọng (`Anh - Anh` / `Anh - Mỹ`).
  - Phát được cả file audio bài học (nếu tìm thấy link `.wav`) hoặc Web Speech Synthesis.
- **Ghi âm thật + Chấm điểm từng từ:**
  - Dùng `MediaRecorder` thu lại âm thanh thật của người học (để phát lại nút **"Nghe lại giọng của bạn"** so sánh với giọng mẫu) song song với `SpeechRecognition` / `webkitSpeechRecognition` (`lang = "en-US"` hoặc `"en-GB"`) để nhận diện lời nói (tự động xử lý khi trình duyệt không hỗ trợ SpeechRecognition bằng thông báo rõ ràng + cho phép dùng tính năng tự kiểm tra / gõ hoặc nghe lại bản ghi âm).
  - Sau khi đọc xong, chấm độ khớp từng từ và tô màu trực quan:
    - **Xanh lá:** phát âm chuẩn.
    - **Vàng:** gần giống (sai khác nhẹ âm cuối/nguyên âm).
    - **Đỏ:** đọc sai hoặc bỏ sót — bấm trực tiếp vào từ màu đỏ để nghe máy đọc chậm riêng từ đó!
  - Có nút 1 chạm: **"Lưu từ đọc sai thành thẻ ôn tập"** (gọi `onCreateFlashcardQuick`) và **"Lưu kết quả vào Sổ tay"** (gọi `onSaveToNotebook`).

### Chế độ 2: Nghe & Chép Chính Tả (Dictation — Gõ phím & Chạm ghép từ)
- Nghe câu mẫu ở trạng thái ẩn chữ (chỉ hiện sau khi kiểm tra hoặc bấm "Xem gợi ý"), có nút nghe chậm `0.75x` và gợi ý chữ cái đầu.
- Hỗ trợ chuyển qua lại **2 cách làm bài**:
  1. **Gõ câu nghe được (Phù hợp máy tính):** Gõ vào ô nhập → bấm Kiểm tra → hiển thị so sánh từng từ (xanh = đúng, đỏ gạch ngang = gõ sai, cam = bị nghe thiếu).
  2. **Chạm ghép từ (Word-Bank Chips — Tối ưu cho điện thoại 390px):** Tách các từ trong câu + thêm 2–3 từ gây nhiễu, xáo trộn thành các thẻ chip (`min-h-[42px]`) để người học chạm bằng ngón tay cái xếp thành câu đúng, chạm lại để bỏ chọn.

### Chế độ 3: Luyện Nói Theo Chủ Đề (Speaking Cue Card + Kiểm tra từ vựng đã dùng)
- Hiển thị 1 câu hỏi Speaking theo chủ đề bài học + gợi ý 3 ý chính + 5–6 cụm từ vựng hay nên dùng.
- Đồng hồ bấm giờ: **Chuẩn bị 1 phút** / **Nói 2 phút**.
- Khi người học bật mic nói (hoặc gõ dàn ý nháp), tự động nhận diện xem người học đã dùng được những cụm từ gợi ý nào và đánh dấu **tick xanh** vào từ đó!
- Kèm bài nói mẫu có đánh dấu cấu trúc câu hay và nút lưu ghi chú luyện nói vào Sổ tay.

### Chế độ 4: Kiểm Tra Nhanh Từ Vựng & Mẫu Câu Trong Bài (Mini-Quiz)
- Tự tạo 4–5 câu hỏi trắc nghiệm ngắn / điền từ vào chỗ trống dựa trên câu & từ vựng trích từ bài học, chấm điểm ngay và giải thích nghĩa tiếng Việt từng lựa chọn.

## Kiểm chứng
1. Chạy `cd frontend; npx tsc --noEmit --incremental false` → bắt buộc exit code `0`.
2. Ghi báo cáo ngắn vào `docs/qa-round2/B1-english-summary.md` nêu rõ: các chế độ đã xây dựng, cách trích xuất audio/câu từ `lessonMarkdown`, giới hạn trình duyệt (SpeechRecognition trên Chrome/Edge vs Firefox), kết quả `tsc` và mục **Chưa kiểm chứng**.
