Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ B3-FINANCE-PRACTICE: Xây dựng Module Thực Hành Tài Chính Cá Nhân & Đầu Tư Chuyên Sâu

## Quy tắc phân vùng chạy song song (BẮT BUỘC)
- **PHẠM VI ĐƯỢC PHÉP TẠO / SỬA:**
  - CHỈ tạo/sửa file: `frontend/src/components/learn/practice/FinancePracticeLab.tsx`
  - Và ghi báo cáo ngắn vào: `docs/qa-round2/B3-finance-summary.md`
- **TUYỆT ĐỐI KHÔNG** sửa bất kỳ file nào khác trong `frontend/` hoặc `backend/` (đặc biệt KHÔNG chạm vào `LessonPracticeLab.tsx`, `LessonMarkdownView.tsx`, `tsconfig.tsbuildinfo`).
- **KHÔNG chạy `git commit`**, **KHÔNG restart server**, **KHÔNG để lại background task**.
- Kiểm tra TypeScript bằng: `cd frontend; npx tsc --noEmit --incremental false` (exit code `0`).

## Chuẩn Props & Giao diện
Export component `FinancePracticeLab` nhận props chuẩn khớp với `LessonPracticeLab.tsx`:
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
- **Ngôn ngữ hiển thị trên UI:** Tiếng Việt dễ hiểu, thực tế với người đi làm & sinh viên (định dạng tiền tệ VNĐ rõ ràng `15.000.000 đ`).
- **Thiết kế "Clean & Calm Learning":** Tương thích Light Mode, Dark Mode, Mobile `390×844` (biểu đồ SVG co giãn `viewBox`, các thanh trượt & nút `min-h-[44px]`).

## 4 Công cụ thực hành tương tác cần xây dựng trong `FinancePracticeLab.tsx`

Tự động chọn công cụ mặc định khớp với `lessonTitle` / `courseTitle` (vd bài về ngân sách/dòng tiền mở Công cụ 1, bài về lãi kép mở Công cụ 2, bài về nợ mở Công cụ 3, bài về cổ phiếu/biểu đồ/định giá mở Công cụ 4), vẫn có thanh tab cho người học chuyển tự do cả 4 công cụ.

### Công cụ 1: Bảng Chia Ngân Sách (50/30/20) & Quỹ Dự Phòng Khẩn Cấp
- Nhập hoặc kéo thanh trượt: **Thu nhập ròng/tháng (VNĐ)**, **Chi tiêu thiết yếu** (nhà, ăn, điện nước), **Mong muốn cá nhân** (ăn ngoài, giải trí), **Tiết kiệm & Đầu tư**, và **Số tháng dự phòng mục tiêu (3–12 tháng)**.
- Hiển thị thanh tỷ lệ phân bổ trực quan theo thời gian thực + đối chiếu chuẩn **50% Thiết yếu · 30% Mong muốn · 20% Tương lai**.
- Cảnh báo ngay nếu chi phí cố định > 60% hoặc tiết kiệm < 10%, tính số tiền **Quỹ dự phòng cần tích lũy** và số tháng đạt mục tiêu theo mức tiết kiệm hiện tại.
- Nút **"Lưu kế hoạch ngân sách vào Sổ tay"** (`onSaveToNotebook`).

### Công cụ 2: Máy Tính Lãi Kép, Lạm Phát & Mục Tiêu Tài Chính
- Thanh trượt điều chỉnh: **Vốn ban đầu**, **Góp thêm mỗi tháng**, **Lãi suất kỳ vọng (%/năm)**, **Lạm phát ước tính (%/năm)**, **Thời gian (1–35 năm)**.
- Vẽ **biểu đồ cột/đường tăng trưởng bằng SVG thuần** (không cài thư viện nặng) thể hiện 3 đường/lớp rõ ràng từng năm:
  1. Tổng tiền gốc tự bỏ ra
  2. Tiền lãi kép sinh lời thêm
  3. Giá trị sức mua thực tế sau khi trừ lạm phát
- Bảng tóm tắt số tiền cuối kỳ + tỷ lệ tiền lãi gấp mấy lần tiền gốc.

### Công cụ 3: Lập Lộ Trình Trả Nợ (Quả Cầu Tuyết vs. Tuyết Lở)
- Cho sẵn 3 khoản nợ mẫu thực tế (Thẻ tín dụng lãi cao, Trả góp 0%/phí, Vay cá nhân) và cho phép người học **thêm/sửa/xóa khoản nợ** + nhập **Số tiền có thể trả thêm mỗi tháng**.
- Tự động tính toán và vẽ thanh so sánh giữa 2 chiến lược:
  - **Quả cầu tuyết (Snowball):** Trả hết khoản dư nợ nhỏ nhất trước để tạo động lực tâm lý.
  - **Tuyết lở (Avalanche):** Trả khoản có lãi suất cao nhất trước để tiết kiệm tối đa tiền lãi.
- Hiển thị rõ: số tháng sạch nợ của từng cách và **số tiền lãi chênh lệch tiết kiệm được**.

### Công cụ 4: Phòng Tập Đọc Biểu Đồ Nến & Tính Quản Trị Rủi Ro Đầu Tư
- Tích hợp **Biểu đồ nến SVG tương tác** với các kịch bản sát thực tế (Nến rút chân tại hỗ trợ, Phá vỡ kháng cự kèm volume lớn, Bẫy tăng giá Bull Trap, Vùng đi ngang Sideways nên đứng ngoài) — giữ hỗ trợ `data-testid` nếu cần hoặc cung cấp nút chọn Mua / Bán / Đứng ngoài, nhập **Tổng vốn**, **% Rủi ro tối đa/lệnh (vd 2%)**, **Giá vào (Entry)**, **Cắt lỗ (Stop-Loss)**, **Chốt lời (Take-Profit)**.
- Tự động tính **Tỷ lệ Lợi nhuận/Rủi ro (R:R)** và **Số lượng cổ phiếu/đơn vị tối đa được phép mua** để nếu chạm Stop-Loss chỉ mất đúng số tiền rủi ro cho phép.
- Bấm **"Chạy tiếp 5 phiên tới"** để mở các cây nến tương lai và chấm kết quả quyết định kèm giải thích chi tiết.
- Kèm 3 bài tập mini đọc nhanh chỉ số tài chính doanh nghiệp (`P/E`, `ROE`, `Nợ / Vốn chủ sở hữu`) và nút lưu tổng kết vào Sổ tay (`onSaveToNotebook`) / tạo thẻ ôn tập (`onCreateFlashcardQuick`).

## Kiểm chứng
1. Chạy `cd frontend; npx tsc --noEmit --incremental false` → bắt buộc exit code `0`.
2. Ghi báo cáo ngắn vào `docs/qa-round2/B3-finance-summary.md` nêu rõ 4 công cụ, công thức tính (lãi kép, lạm phát thực, snowball vs avalanche, position sizing), kết quả `tsc` và mục **Chưa kiểm chứng**.
