# Báo Cáo Hoàn Thành Nhiệm Vụ B4-PSYCH-LIFESKILLS-PRACTICE

## 1. Kết quả chính

Đã xây dựng thành công component thực hành chuyên sâu:
`frontend/src/components/learn/practice/PsychologyLifeSkillsLab.tsx`

Component tuân thủ chuẩn props giao tiếp với hệ thống học tập Studxy:
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

Giao diện áp dụng phong cách thiết kế **Clean & Calm Learning**, hỗ trợ 100% Light Mode & Dark Mode, tối ưu màn hình di động `390×844` (tap target tối thiểu `min-h-[44px]`, bong bóng chat phản hồi mềm mại).

### Chi tiết 3 Chế độ thực hành chuyên sâu:

#### Chế độ 1: Mô Phỏng Hội Thoại Thực Tế (Interactive Roleplay)
- Xây dựng 4 kịch bản thực tế phân nhánh sâu (2–3 lượt qua lại, mỗi lượt có 3–4 lựa chọn phản ứng):
  1. **Góp ý đồng đội trễ hạn mà không gây phòng thủ:** Xử lý tình huống 22h đêm trước ngày thuyết trình đồng đội chưa gửi slide; chuyển hóa từ công kích/né tránh sang Giao tiếp Trắc ẩn (NVC).
  2. **Từ chối lời nhờ vả quá sức mà vẫn giữ trọn tình cảm & ranh giới:** Chị đồng nghiệp thân nhờ làm hộ báo cáo chiều thứ Sáu khi bản thân đang gánh 2 deadline gấp; công thức đồng cảm + từ chối trung thực + đề xuất giải pháp thay thế.
  3. **Xử lý khi nhận lời chê bai gay gắt:** Phản hồi khi bị thành viên nhóm phản biện lớn tiếng chê bai ý tưởng trong cuộc họp; áp dụng kỹ thuật Fogging, tách biệt giá trị bản thân khỏi cảm xúc tức thời để xoay chuyển về dữ liệu thực tế.
  4. **Lắng nghe người thân đang stress:** Lắng nghe thấu cảm em gái/người yêu đi làm về kiệt sức; tránh cạm bẫy Toxic Positivity (giảng giải đạo lý) và Fixing Mode (vội vàng đưa giải pháp), thay vào đó thực hành Holding Space (giữ không gian an toàn).
- Mỗi lượt phản hồi của người học đều hiển thị:
  - Phản ứng thực tế và cảm xúc của đối phương.
  - Chấm điểm 2 tiêu chí trực quan: **Sự tôn trọng/Ranh giới** (thang điểm 10) & **Mức độ đồng cảm** (thang điểm 10).
  - Phân tích chi tiết theo khung 4 bước NVC: *Nêu sự việc khách quan → Nói cảm xúc thật → Nêu nhu cầu cốt lõi → Đưa đề nghị hành động cụ thể*.
  - Nút **"Lưu bài học hội thoại vào Sổ tay"** (`onSaveToNotebook`) và **"Tạo thẻ ghi nhớ câu nói mẫu"** (`onCreateFlashcardQuick`).

#### Chế độ 2: Nhận Diện Bẫy Tâm Lý Hằng Ngày (Cognitive Bias Detector)
- Bộ 8 tình huống thực tế về các bẫy nhận thức phổ biến trong đời sống:
  1. *Thiên kiến xác nhận (Confirmation Bias):* Chỉ tìm đọc bài khen ngợi chế độ ăn Keto, bác bỏ nghiên cứu y khoa cảnh báo rủi ro.
  2. *Ngụy biện chi phí chìm (Sunk Cost Fallacy):* Cắn răng học tiếp ngành không hợp vì tiếc 3 năm đại học đã bỏ ra.
  3. *Hiệu ứng mỏ neo (Anchoring Effect):* Áo niêm yết 2.500.000đ gạch giá còn 990.000đ tạo cảm giác hời dù giá trị thực thấp hơn.
  4. *Hiệu ứng Dunning-Kruger:* Đọc 3 bài viết chứng khoán đã tự tin all-in tiền tiết kiệm vào mã rủi ro.
  5. *Tâm lý bầy đàn & Bằng chứng xã hội (Bandwagon / Social Proof):* Xếp hàng 2 tiếng dưới nắng mua trà sữa hot trend dù không khát.
  6. *Tư duy Nhanh (Hệ thống 1) vs Tư duy Chậm (Hệ thống 2):* Phản xạ bốc hỏa gõ phím cãi nhau ngay khi thấy comment khiêu khích trên mạng xã hội.
  7. *Hiệu ứng vầng hào quang (Halo Effect):* Vội vàng tin tưởng ký hợp đồng đầu tư tiền tỷ chỉ vì đối tác ăn mặc vest sang trọng, bảnh bao.
  8. *Ảo tưởng trong suốt (Illusion of Transparency):* Lầm tưởng rằng cả hội trường 50 người đều nhìn thấu sự run rẩy bên trong của mình khi thuyết trình.
- Sau khi chọn đáp án:
  - Phân tích cơ chế tiến hóa não bộ (khoa học thần kinh / Neuroscience & Hạch hạnh nhân Amygdala).
  - Cung cấp **1 mẹo hành động ngay (Actionable Mind-Hack)**: Kỹ thuật Luật sư của Ác quỷ, Phép thử Số Không Thuần Khiết, Nhổ mỏ neo, Quy tắc 10 giây thở sâu...
  - Nút tạo thẻ ghi nhớ nhanh (`onCreateFlashcardQuick`) và lưu sổ tay (`onSaveToNotebook`).

#### Chế độ 3: Gỡ Rối Suy Nghĩ CBT (Interactive Reflection Card)
- Công cụ tương tác 5 bước CBT (Liệu pháp Nhận thức - Hành vi):
  1. *Bước 1: Sự việc vừa xảy ra là gì?* (Ghi lại sự thật khách quan như máy quay ghi hình, không phán xét/suy diễn).
  2. *Bước 2: Suy nghĩ tự động đầu tiên nảy ra trong đầu?* + Bộ chip chọn cảm xúc (10 cảm xúc) + Slider đo mức độ căng thẳng (`1–10`) kèm màu sắc trực quan.
  3. *Bước 3: Cân nhắc bằng chứng* (2 cột so sánh: Bằng chứng ủng hộ vs Bằng chứng phản bác/Sự thật ngược lại).
  4. *Bước 4: Góc nhìn trắc ẩn* ("Nếu người bạn thân nhất gặp chuyện này, bạn sẽ khuyên họ thế nào?").
  5. *Bước 5: 1 việc nhỏ làm được ngay trong 5 phút tới* (Hành động vi mô lấy lại quyền kiểm soát).
- Kèm sẵn **3 kịch bản mẫu** (Bị sếp phê bình slide, Tin nhắn 2 tiếng chưa được trả lời, Điểm thi thử thấp hơn kỳ vọng) để người học tham khảo chỉ với 1 click.
- Nút **"Xuất & Lưu bản gỡ rối vào Sổ tay"** (`onSaveToNotebook`) với cấu trúc Markdown chỉn chu, chuyên nghiệp.

---

## 2. Bằng chứng thực thi

### Lệnh kiểm tra kiểu TypeScript:
```powershell
cd frontend; npx tsc --noEmit --incremental false
```
- **Exit code:** `0`
- **Output:** Không có lỗi cú pháp hoặc sai lệch kiểu dữ liệu nào.

### Kiểm tra phân vùng file & an toàn repo:
```powershell
git status
```
- Chỉ có thư mục mới `frontend/src/components/learn/practice/PsychologyLifeSkillsLab.tsx` được tạo.
- Không chạm vào `LessonPracticeLab.tsx`, `LessonMarkdownView.tsx`, `tsconfig.tsbuildinfo` hay bất kỳ file nào khác trong `frontend/` và `backend/`.
- Không chạy `git commit`, không restart server, không để lại bất kỳ tiến trình nền nào.

---

## 3. Danh sách vấn đề phát hiện

- Không phát hiện lỗi (0 vấn đề phát sinh). Component hoạt động độc lập, tự chủ về dữ liệu và tương thích hoàn toàn với hệ thống hiện tại.

---

## 4. Mục Chưa kiểm chứng

1. **Giao tiếp đầu cuối với Backend khi tích hợp vào router/workspace:**
   - Component hiện tại là unit component độc lập, tuân thủ interface `PracticeModuleProps`. Việc gắn component vào tab hiển thị thực tế của khóa học Tâm Lý Học / Kỹ Năng Sống phụ thuộc vào nhiệm vụ tích hợp tổng hợp (ví dụ Task B5).
2. **Khả năng hiển thị trên thiết bị di động vật lý iOS/Android:**
   - Component đã được định dạng chuẩn với responsive classes của Tailwind (`min-h-[44px]`, `grid-cols-1 sm:grid-cols-2`, `p-3 sm:p-5`), tuy nhiên chưa kiểm thử thực tế trên màn hình Safari iOS thật.
