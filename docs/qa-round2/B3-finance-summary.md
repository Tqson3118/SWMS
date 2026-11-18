# Báo Cáo Hoàn Thành Nhiệm Vụ B3-FINANCE-PRACTICE

## 1. Kết quả chính

Đã xây dựng thành công module thực hành tài chính cá nhân & đầu tư chuyên sâu:
`frontend/src/components/learn/practice/FinancePracticeLab.tsx`

Component tuân thủ chuẩn props của hệ thống học tập Studxy:
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

### Tiêu chuẩn giao diện & Trải nghiệm người dùng:
- **Ngôn ngữ hiển thị:** Tiếng Việt thực tế, chuẩn mực, định dạng tiền tệ VNĐ rõ ràng (vd: `15.000.000 đ`).
- **Phong cách Clean & Calm Learning:** Hoàn toàn tương thích Light Mode & Dark Mode; tối ưu cho màn hình di động `390×844` (mọi nút bấm và thanh trượt đạt `min-h-[44px]`, biểu đồ SVG co giãn theo `viewBox`).
- **Cơ chế nhận diện thông minh (Auto-detection):** Tự động phân tích tiêu đề và nội dung bài học để chọn tab công cụ mặc định phù hợp nhất (Ngân sách / Lãi kép / Nợ / Nến & Đầu tư), đồng thời vẫn có thanh tab cho phép người học tự do chuyển đổi giữa 4 công cụ.

---

### Chi tiết 4 Công cụ thực hành tương tác đã xây dựng:

#### Công cụ 1: Bảng Chia Ngân Sách (50/30/20) & Quỹ Dự Phòng Khẩn Cấp
- **Tính năng tương tác:**
  - Nhập và điều chỉnh qua thanh trượt: **Thu nhập ròng/tháng**, **Chi tiêu thiết yếu** (nhà, ăn, điện nước), **Mong muốn cá nhân** (ăn ngoài, giải trí), **Tiết kiệm & Đầu tư**, và **Số tháng quỹ dự phòng mục tiêu (3–12 tháng)**.
  - Nút *"Áp dụng chuẩn 50/30/20 tự động"* giúp tính nhanh hạn mức lý tưởng dựa trên thu nhập.
  - Thanh tỷ lệ phân bổ tiến trình đa phân đoạn (Multi-segment progress bar) trực quan theo thời gian thực.
  - Bảng đối chiếu thực tế vs chuẩn (50% Thiết yếu · 30% Mong muốn · 20% Tiết kiệm).
  - Hệ thống cảnh báo tự động:
    - Cảnh báo dòng tiền âm (Bội chi / Thâm hụt ngân sách).
    - Cảnh báo chi phí cố định quá cao (Chi tiêu thiết yếu > 60% thu nhập).
    - Cảnh báo tích lũy mỏng (Tiết kiệm < 10% thu nhập).
  - Máy tính Quỹ dự phòng khẩn cấp: Tính số tiền mục tiêu cần tích lũy và số tháng/năm hoàn thành theo tốc độ tiết kiệm hiện tại.
  - Nút **"Lưu kế hoạch ngân sách vào Sổ tay"** (`onSaveToNotebook`) xuất định dạng Markdown chi tiết.

#### Công cụ 2: Máy Tính Lãi Kép, Lạm Phát & Mục Tiêu Tài Chính
- **Tính năng tương tác:**
  - Thanh trượt điều chỉnh: **Vốn ban đầu**, **Góp thêm mỗi tháng**, **Lãi suất kỳ vọng (%/năm)**, **Lạm phát ước tính (%/năm)**, **Thời gian đầu tư (1–35 năm)**.
  - **Biểu đồ SVG thuần 3 lớp co giãn:**
    1. Lớp cột Vốn gốc tự bỏ ra (màu xanh dương).
    2. Lớp cột Tiền lãi kép sinh lời thêm (màu xanh ngọc emerald).
    3. Đường nét đứt Sức mua thực tế sau khi trừ lạm phát (màu hổ phách amber) kèm các điểm mốc năm.
  - Tương tác rê chuột / chọn mốc năm hiển thị chi tiết số liệu tức thời.
  - Thẻ tóm tắt kết quả: Tổng tài sản danh nghĩa, Tổng vốn gốc, Tổng lãi kép, Sức mua thực và **Tỷ lệ tiền lãi gấp mấy lần tiền gốc**.
  - Tích hợp thẻ **Quy tắc 72** (ước tính số năm nhân đôi tài sản).
  - Nút **"Lưu kế hoạch lãi kép vào Sổ tay"** (`onSaveToNotebook`) và **"Tạo thẻ Flashcard Quy tắc 72"** (`onCreateFlashcardQuick`).

#### Công cụ 3: Lập Lộ Trình Trả Nợ (Quả Cầu Tuyết vs. Tuyết Lở)
- **Tính năng tương tác:**
  - Sẵn 3 khoản nợ mẫu thực tế: Thẻ tín dụng lãi cao (28%/năm), Trả góp điện thoại/xe (12%/năm), Vay cá nhân (6%/năm).
  - Cho phép người học **Thêm / Xóa khoản nợ** và điều chỉnh **Số tiền có thể trả thêm mỗi tháng**.
  - Mô phỏng trả nợ hàng tháng và so sánh song song giữa 2 phương pháp:
    - **Quả cầu tuyết (Debt Snowball):** Ưu tiên trả dứt điểm khoản nợ có dư nợ nhỏ nhất trước để tạo động lực tâm lý gạch nợ sớm.
    - **Tuyết lở (Debt Avalanche):** Ưu tiên trả khoản nợ có lãi suất cao nhất trước để tiết kiệm tối đa tiền lãi.
  - Hiển thị trực quan: Số tháng sạch nợ, tổng tiền lãi phải trả, **số tiền lãi chênh lệch tiết kiệm được** và danh sách lộ trình thứ tự gạch nợ.
  - Nút **"Lưu kế hoạch trả nợ vào Sổ tay"** (`onSaveToNotebook`).

#### Công cụ 4: Phòng Tập Đọc Biểu Đồ Nến & Tính Quản Trị Rủi Ro Đầu Tư
- **Tính năng tương tác:**
  - Tích hợp 4 kịch bản thực tế:
    1. *Nến rút chân (Bullish Pin Bar) tại vùng Hỗ trợ cứng.*
    2. *Bẫy tăng giá (Bull Trap / False Breakout) tại Kháng cự.*
    3. *Phá vỡ kháng cự bùng nổ khối lượng (Breakout with High Volume).*
    4. *Vùng đi ngang nhiễu loạn (Choppy Sideways) — Bài học kiên nhẫn đứng ngoài.*
  - **Biểu đồ nến SVG tương tác:**
    - Hiển thị vùng Kháng cự (đỏ mờ) và Hỗ trợ (xanh mờ).
    - Hiển thị các đường kẻ giá: Vào lệnh (Entry), Cắt lỗ (SL), Chốt lời (TP).
    - Thân nến, râu nến và thanh khối lượng giao dịch (Volume) từng phiên.
  - Bộ nút chọn hành động: **MUA (Long)** / **BÁN (Short)** / **ĐỨNG NGOÀI (Wait)**.
  - Nút **"Chạy tiếp 5 phiên tới"**: Mở 5 cây nến tương lai, chấm điểm đúng/sai và phân tích chi tiết diễn biến thị trường.
  - **Bộ tính Quản trị rủi ro & Định cỡ vị thế (Position Sizing):**
    - Nhập Tổng vốn, % Rủi ro tối đa/lệnh (Quy tắc 2%), Entry, SL, TP.
    - Tự động tính **Tỷ lệ Lợi nhuận/Rủi ro (R:R)** và **Số lượng cổ phiếu tối đa được phép mua** để bảo đảm nếu dính Cắt lỗ chỉ mất đúng số tiền rủi ro cho phép.
  - **3 Bài tập mini đọc nhanh chỉ số tài chính:**
    - Chỉ số P/E (Giá trên thu nhập & Cảnh giác bẫy giá trị).
    - Chỉ số ROE (Hiệu quả sử dụng vốn chủ sở hữu theo tiêu chí Warren Buffett).
    - Tỷ lệ Nợ / Vốn chủ sở hữu D/E (Đòn bẩy tài chính & Rủi ro khi lãi suất tăng).
  - Nút **"Lưu phân tích rủi ro vào Sổ tay"** (`onSaveToNotebook`) và **"Tạo thẻ Flashcard R:R & Chỉ số"** (`onCreateFlashcardQuick`).

---

## 2. Công thức tính toán chi tiết

### 1. Công thức Lãi kép (Compound Interest Future Value):
- Lãi suất hàng tháng:
  $$r_m = \frac{r_{\text{năm}}}{12 \times 100}$$
- Số tháng sau $y$ năm:
  $$m = y \times 12$$
- Tổng vốn gốc tự nộp:
  $$P_{\text{gốc}}(y) = P_0 + PMT \times m$$
- Giá trị tương lai danh nghĩa (khi $r_m > 0$):
  $$FV(y) = P_0 \times (1 + r_m)^m + PMT \times \frac{(1 + r_m)^m - 1}{r_m}$$
- Tiền lãi kép sinh ra:
  $$I(y) = FV(y) - P_{\text{gốc}}(y)$$

### 2. Sức mua thực tế sau lạm phát:
- Chiết khấu giá trị tương lai về hiện giá theo tỷ lệ lạm phát hàng năm $i$:
  $$FV_{\text{real}}(y) = \frac{FV(y)}{(1 + \frac{i}{100})^y}$$

### 3. Quy tắc 72 (Rule of 72):
- Ước tính số năm tài sản tự nhân đôi:
  $$T_{\text{double}} \approx \frac{72}{r_{\text{năm}}}$$

### 4. Thuật toán mô phỏng Quả Cầu Tuyết (Snowball) vs Tuyết Lở (Avalanche):
- Mỗi tháng $t$, với mỗi khoản nợ $k$:
  $$\text{Lãi phát sinh}_k = \text{Dư nợ}_k \times \frac{r_k}{12 \times 100}$$
- Trả số tiền tối thiểu $PMT_{\min, k}$ cho tất cả khoản nợ.
- Dồn toàn bộ số tiền thanh toán thêm + số tiền tối thiểu của các khoản đã trả dứt điểm vào khoản nợ ưu tiên:
  - **Snowball:** Chọn khoản nợ có $\min(\text{Dư nợ}_k)$.
  - **Avalanche:** Chọn khoản nợ có $\max(r_k)$.
- Quá trình lặp cho đến khi toàn bộ dư nợ về 0.

### 5. Quản trị rủi ro & Định cỡ vị thế (Risk Management & Position Sizing):
- Số tiền chịu rủi ro tối đa:
  $$\text{Risk Amount} = \text{Tổng vốn} \times \frac{\text{\% Rủi ro}}{100}$$
- Khoảng cách cắt lỗ trên 1 cổ phiếu:
  $$\Delta_{\text{SL}} = |\text{Entry} - \text{SL}|$$
- Số lượng cổ phiếu tối đa được phép mua:
  $$\text{Max Units} = \left\lfloor \frac{\text{Risk Amount}}{\Delta_{\text{SL}}} \right\rfloor$$
- Tỷ lệ Lợi nhuận / Rủi ro:
  $$R:R = \frac{|\text{TP} - \text{Entry}|}{|\text{Entry} - \text{SL}|}$$
  *(Khuyến nghị $R:R \ge 2.0$)*

---

## 3. Bằng chứng thực thi

### Lệnh kiểm tra kiểu TypeScript:
```powershell
cd frontend; npx tsc --noEmit --incremental false
```
- **Exit code:** `0`
- **Output:** Không có bất kỳ lỗi biên dịch hoặc sai lệch kiểu dữ liệu nào.

### Kiểm tra phân vùng file & an toàn repo:
```powershell
git status
```
- **File mới tạo:** `frontend/src/components/learn/practice/FinancePracticeLab.tsx`
- **File báo cáo:** `docs/qa-round2/B3-finance-summary.md`
- **Tuân thủ phân vùng:** Không chạm vào `LessonPracticeLab.tsx`, `LessonMarkdownView.tsx`, `tsconfig.tsbuildinfo` hay bất kỳ file nào khác trong `frontend/` và `backend/`.
- Không chạy `git commit`, không restart server, không để lại bất kỳ background task nào.

---

## 4. Danh sách vấn đề phát hiện

- Không có lỗi phát sinh (0 vấn đề). Module được thiết kế hoàn toàn khép kín, độc lập và tương thích 100% với props chuẩn `PracticeModuleProps`.

---

## 5. Mục Chưa kiểm chứng

1. **Tích hợp thực tế vào Workspace học tập của các khóa học Tài chính cụ thể:**
   - Component hiện tại là unit component độc lập, tuân thủ interface `PracticeModuleProps`. Việc đưa component này vào danh mục tab của các bài học cụ thể trong khóa học tài chính sẽ được thực hiện ở nhiệm vụ tích hợp tổng hợp (Task B5).
2. **Khả năng hiển thị và thao tác trên thiết bị di động iOS Safari vật lý:**
   - Mặc dù code đã được thiết kế đáp ứng kích thước `390×844` với các class Tailwind `min-h-[44px]` và SVG responsive `viewBox`, component chưa được kiểm thử trên màn hình cảm ứng Safari vật lý thực tế.
