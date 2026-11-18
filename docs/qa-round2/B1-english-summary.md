# Báo Cáo Hoàn Thành Nhiệm Vụ B1-ENGLISH-PRACTICE: Module Thực Hành Tiếng Anh & IELTS Chuyên Sâu (100% Miễn Phí)

## 1. Kết quả chính

Đã xây dựng thành công module thực hành tiếng Anh & IELTS chuyên sâu và tự động thích ứng theo nội dung bài học:
- **File thành phần:** `frontend/src/components/learn/practice/EnglishPracticeLab.tsx`
- **Chuẩn Props tích hợp:** Hoàn toàn tương thích và khớp chuẩn với hệ thống học tập Studxy:
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

### Tiêu chuẩn giao diện & Trải nghiệm người dùng ("Clean & Calm Learning"):
- **Ngôn ngữ hiển thị:** Tiếng Việt tự nhiên, đời thường, dễ hiểu, khích lệ và không dùng thuật ngữ nặng nề.
- **Bảng màu CSS Tokens:** Sử dụng triệt để hệ thống CSS variables (`var(--surface-card)`, `var(--surface-elevated)`, `var(--border-calm)`, `var(--text-primary)`, `var(--text-secondary)`, màu chủ đạo `#5E51E8` / `#818CF8`, success emerald, warning amber, danger rose).
- **Hỗ trợ toàn diện Dark / Light Mode & Mobile `390×844`:**
  - Nút bấm và thẻ chip đạt kích thước chạm tối thiểu `min-h-[44px]`.
  - Nút micro trung tâm nổi bật kích thước `w-16 h-16` kèm hiệu ứng ping khi đang thu âm, cực kỳ thuận tiện cho thao tác bằng một ngón tay cái trên màn hình điện thoại.
  - Bố cục responsive không bị tràn màn hình ngang (zero horizontal overflow).

---

## 2. Chi tiết 4 Chế độ thực hành chuyên sâu đã xây dựng

### Chế độ 1: Luyện Phát Âm & Nhại Giọng (Shadowing + Chấm từng từ)
- **Hiển thị câu mẫu trực quan:** Mẫu câu tiếng Anh rõ ràng, kèm phiên âm chuẩn quốc tế IPA và nghĩa tiếng Việt.
- **Nghe mẫu đa tốc độ & đa chất giọng:**
  - Nút nghe mẫu hỗ trợ tốc độ `0.75x`, `1.0x`, `1.25x`.
  - Lựa chọn giọng bản xứ linh hoạt: 🇺🇸 **Anh - Mỹ (en-US)** và 🇬🇧 **Anh - Anh (en-GB)**.
  - Tự động ưu tiên phát file âm thanh bản xứ của bài học nếu tìm thấy đường dẫn `.wav` / `.mp3`, và tự động chuyển đổi mượt sang Web Speech API `window.speechSynthesis` nếu không có file âm thanh hoặc file chưa tải được.
- **Ghi âm thật + Chấm điểm từng từ trực quan:**
  - Kết hợp đồng thời `MediaRecorder` để ghi âm âm thanh thật của người học (cho phép bấm **"Nghe lại giọng của bạn"** để đối chiếu trực tiếp với giọng mẫu) và `SpeechRecognition` / `webkitSpeechRecognition` để nhận diện văn bản theo thời gian thực.
  - Phân tích và tô màu chính xác từng từ:
    - **Xanh lá (Emerald):** Phát âm chuẩn xác.
    - **Vàng (Amber):** Gần giống (sai khác nhẹ âm cuối hoặc đuôi thì quá khứ/số nhiều/tiếp diễn, phát hiện bằng thuật toán khoảng cách Levenshtein $\le 2$).
    - **Đỏ (Rose):** Đọc sai hoặc bỏ sót — **Đặc biệt: Bấm trực tiếp vào từ màu đỏ để máy phát âm chậm riêng từ đó ở tốc độ `0.65x`!**
  - **Thao tác 1 chạm tiện ích:**
    - Nút *"Lưu từ đọc sai thành thẻ Flashcard"* (`onCreateFlashcardQuick`): Tự động gom toàn bộ các từ đọc sai trong câu kèm ngữ cảnh và nghĩa tiếng Việt để đưa vào bộ thẻ lặp lại ngắt quãng.
    - Nút *"Lưu kết quả vào Sổ tay"* (`onSaveToNotebook`): Xuất báo cáo điểm phát âm chi tiết vào sổ tay học tập.

### Chế độ 2: Nghe & Chép Chính Tả (Dictation — Gõ phím & Chạm ghép từ)
- Trạng thái ban đầu câu được ẩn đi để người học tập trung luyện nghe qua nút phát âm thanh bình thường `1.0x` và nghe chậm `0.75x`.
- Tích hợp nút xem gợi ý chữ cái đầu của từng từ (dạng `T___ u_________ l______...`) và nút hiện đáp án đầy đủ.
- Hỗ trợ chuyển đổi qua lại mượt mà giữa **2 phương thức làm bài**:
  1. **Gõ phím (Typing - Tối ưu cho Máy tính):** Ô nhập văn bản kèm nút "Kiểm tra đáp án" đối chiếu từng từ: tô xanh từ đúng, gạch ngang màu đỏ từ gõ sai, và đánh dấu màu cam cho từ bị bỏ sót kèm tỷ lệ % chính xác.
  2. **Chạm ghép từ (Word-Bank Chips - Tối ưu cho Điện thoại 390px):** Tách câu thành các thẻ từ vựng (`min-h-[44px]`), bổ sung thêm 3 từ gây nhiễu thực tế (distractors) và xáo trộn ngẫu nhiên. Người học chỉ cần dùng ngón tay cái chạm để xếp từ vào câu hoặc chạm để thu hồi từ về kho. Hệ thống phản hồi ngay tức thì khi hoàn thành 100%.

### Chế độ 3: Luyện Nói Theo Chủ Đề (Speaking Cue Card + Kiểm tra từ vựng)
- Đưa ra đề thi nói chuẩn IELTS Speaking Part 2 & 3 thích ứng theo chủ đề bài học.
- **Dàn bài định hướng:** Cung cấp 3 ý chính gợi ý giúp người học không bị bí ý tưởng.
- **Đồng hồ bấm giờ chuẩn kỳ thi:**
  - Đồng hồ **Chuẩn bị 1 phút (60 giây)** có cảnh báo đếm ngược.
  - Đồng hồ **Nói 2 phút (120 giây)** phục vụ luyện phản xạ nói liền mạch.
- **Bộ nhận diện từ vựng mục tiêu theo thời gian thực (Live Target Collocation Detection):**
  - Cung cấp 5–6 cụm từ vựng/idioms ghi điểm cao (Band 7.5+).
  - Khi người học bật mic nói (hoặc gõ dàn ý ghi chú), hệ thống tự động quét lời thoại theo thời gian thực và đánh dấu **tick xanh (✓)** ngay lập tức lên các cụm từ mà người học đã ứng dụng thành công.
- **Bài nói mẫu tham khảo (Model Answer):** Kèm bài nói mẫu Band 8.0 làm nổi bật cách liên kết ý và cấu trúc câu tự nhiên.
- **Nhận xét AI & Đánh giá tiêu chí:** Tích hợp gọi API `/api/v1/ai/explain` (hoặc phân tích rubric tự động khi ngoại tuyến) nhận xét về độ trôi chảy (Fluency), vốn từ (Lexical Resource) và ngữ pháp.
- Nút *"Lưu bài nói vào Sổ tay"* (`onSaveToNotebook`).

### Chế độ 4: Kiểm Tra Nhanh Từ Vựng & Mẫu Câu Trong Bài (Mini-Quiz)
- Tự động biên soạn 5 câu hỏi trắc nghiệm / điền từ vào chỗ trống dựa trên các mẫu câu và cụm từ vựng trích xuất từ bài học.
- Mỗi câu gồm 4 lựa chọn với kích thước chuẩn di động (`min-h-[48px]`).
- Phản hồi tức thì ngay khi chọn: Tô xanh đáp án đúng, tô đỏ đáp án sai, hiển thị hộp giải thích chi tiết bằng tiếng Việt về nghĩa và ngữ pháp.
- Bảng tổng kết điểm số cuối bài kèm nút *"Làm lại bài kiểm tra"* và nút *"Lưu kết quả vào Sổ tay"*.

---

## 3. Cách trích xuất âm thanh và câu mẫu từ `lessonMarkdown`

Hàm `extractPracticeContent(markdown, lessonTitle, courseTitle)` thực hiện phân tích tự động thích ứng qua 3 bước:

1. **Trích xuất đường dẫn Audio:**
   - Sử dụng regex tìm kiếm các mẫu đường dẫn âm thanh:
     `/(?:https?:\/\/[^\s)"]+|[A-Za-z0-9_./-]+\.(?:wav|mp3))/gi`
   - Chuẩn hóa:
     - Nếu là link tuyệt đối (`https://...` như British Council podcasts) -> Giữ nguyên.
     - Nếu là đường dẫn tương đối (vd `Phase-1/audio/cycle10-listening-authored-practice.wav`) -> Chuẩn hóa thành `/api/v1/document-assets/IELTS/${cleanPath}` để khớp với endpoint static asset proxy đã có sẵn trong backend.
2. **Trích xuất câu tiếng Anh & Từ vựng từ Markdown:**
   - Bắt các định dạng từ vựng in đậm kèm phiên âm/nghĩa: `\*\*Word\*\*\s*(\(/ipa/\))? - Nghĩa`.
   - Bắt các gạch đầu dòng chứa câu tiếng Anh trọn vẹn: `[-*•]\s*["“']?([A-Z][a-zA-Z0-9\s,;:'"–—-]{18,180}[.!?])["”']?`.
   - Bắt các đoạn hội thoại: `(?:Speaker|Man|Woman|Examiner|Candidate|[AB]): ...`.
3. **Phân loại chủ đề thông minh & Bổ sung bộ mẫu câu chuyên sâu:**
   - Phân tích từ khóa để xác định chủ đề:
     - **IELTS & Học thuật:** Nếu tiêu đề/nội dung chứa `ielts`, `listening`, `reading`, `speaking`, `academic`, v.v.
     - **Công nghệ & Tài chính:** Nếu tiêu đề/nội dung chứa `tech`, `code`, `finance`, `trading`, `crypto`, `software`, v.v.
     - **Giao tiếp đời sống & Công sở:** Dành cho các bài học giao tiếp thông dụng.
   - Nếu bài học có ít hơn 5 câu tiếng Anh, hệ thống tự động bổ sung các mẫu câu chất lượng cao thuộc đúng nhóm chủ đề đó, đảm bảo người học luôn có đủ 5 bài tập thực hành toàn diện.

---

## 4. Giới hạn trình duyệt & Giải pháp xử lý (Browser Compatibility)

| Trình duyệt | Web Speech Synthesis (Đọc mẫu) | MediaRecorder (Thu âm thật) | Web Speech Recognition (Nhận diện giọng) | Giải pháp xử lý trong EnglishPracticeLab |
| :--- | :---: | :---: | :---: | :--- |
| **Google Chrome / MS Edge** (Chromium) | ✅ Rất tốt | ✅ Hỗ trợ đầy đủ | ✅ Hỗ trợ native (`webkitSpeechRecognition`) | Nhận diện giọng nói thời gian thực mượt mà, chấm điểm ngay khi đọc. |
| **Mozilla Firefox** | ✅ Hỗ trợ tốt | ✅ Hỗ trợ đầy đủ | ⚠️ Mặc định tắt (cần config `media.webspeech.recognition`) | Tự động phát hiện trình duyệt không hỗ trợ recognition: Hiển thị banner hướng dẫn nhẹ nhàng, vẫn cho phép bấm nút Mic thu âm thật bằng `MediaRecorder` và nghe lại giọng của mình, đồng thời cung cấp ô nhập/dán văn bản để chấm điểm từng từ mà không bị chặn. |
| **Apple Safari (iOS / macOS)** | ✅ Hỗ trợ tốt | ✅ Hỗ trợ đầy đủ | ⚠️ Cần tương tác người dùng để cấp quyền | Bắt buộc người dùng chạm nút Mic rõ ràng trước khi gọi `getUserMedia` và `SpeechRecognition`, tự động giải phóng audio track khi dừng để tránh giữ mic nền. |
| **Khi mất mạng / File audio 404** | ✅ Hoạt động offline | ✅ Hoạt động offline | ⚠️ Cần kết nối nhận diện giọng | Tự động fallback sang `window.speechSynthesis` đọc máy chuẩn xác, không làm gián đoạn bài học. |

---

## 5. Bằng chứng kiểm chứng (Verification Evidence)

### 1. Kiểm tra biên dịch TypeScript (tsc):
- **Lệnh thực hiện:** `cd frontend; npx tsc --noEmit --incremental false`
- **Exit Code:** `0`
- **Kết quả:** Không có bất kỳ lỗi cú pháp hoặc kiểu dữ liệu (0 errors). Toàn bộ kiểu dữ liệu Web Speech, MediaRecorder, DOM events và props chuẩn đều được khai báo chặt chẽ.

### 2. Kiểm tra tính toàn vẹn phân vùng (File Isolation):
- Chỉ tạo mới duy nhất: `frontend/src/components/learn/practice/EnglishPracticeLab.tsx`
- Báo cáo nhiệm vụ: `docs/qa-round2/B1-english-summary.md`
- Hoàn toàn KHÔNG chỉnh sửa bất kỳ file nào khác trong `frontend/` hoặc `backend/` (không chạm vào `LessonPracticeLab.tsx`, `LessonMarkdownView.tsx`, `tsconfig.tsbuildinfo`).
- Không để lại bất kỳ tiến trình chạy nền (background task) nào.

---

## 6. Chưa kiểm chứng

1. **Kiểm thử microphone trên thiết bị vật lý thực tế:** Trình duyệt chạy trong môi trường headless / sandbox CI/CD không có thiết bị đầu vào microphone vật lý và không kết nối dịch vụ Google Speech Cloud, do đó luồng thu âm thực tế cần được người dùng trải nghiệm trên Chrome/Edge thực tế với quyền microphone được cấp.
2. **Tích hợp vào giao diện bài học chính:** Do quy tắc phân vùng song song nghiêm ngặt cấm sửa `LessonPracticeLab.tsx` hoặc `CourseLearningWorkspace.tsx`, việc nhúng component `EnglishPracticeLab` vào tab thực hành tiếng Anh chính sẽ do người điều phối (hoặc tác vụ B5) thực hiện sau khi nghiệm thu component độc lập này.
