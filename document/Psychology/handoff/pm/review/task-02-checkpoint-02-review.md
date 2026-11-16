# Reviewer Handoff — Task 2 / Checkpoint 2

## Lệnh cho phiên chat mới

Bạn là **reviewer độc lập** của dự án khóa tự học Tâm lý học 48 tuần. Hãy review Task 2 ngay sau khi đọc handoff này. Không sửa file, không viết hộ implementer và không tin báo cáo bàn giao nếu chưa đối chiếu artifact thực tế.

## Trạng thái dự án

- Task 1 / Checkpoint 1: **APPROVED**.
- Task 2 đã được agent thực thi bàn giao và đang chờ review lần đầu.
- Chưa làm Task 3 và chưa viết Tuần 1 theo báo cáo implementer; phải kiểm chứng trạng thái file.

## Đọc bắt buộc theo thứ tự

1. `REVIEW_RULES.md`
2. `AGENTS.md`
3. `docs/superpowers/specs/2026-08-28-psychology-curriculum-design.md`, phần content contract/template
4. `implementation_plan.md`, phần Task 2
5. `source-policy.md`
6. `quality/content-audit-checklist.md`
7. `templates/lesson-template.md`
8. `templates/quiz-template.md`
9. `templates/answer-key-template.md`

Chỉ đọc thêm `quality/source-ledger.md` và nguồn gốc khi cần xác minh demo/citation.

## Artifact cần review

- `templates/lesson-template.md`
- `templates/quiz-template.md`
- `templates/answer-key-template.md`

Kiểm tra workspace để xác nhận không có thay đổi vượt phạm vi Task 2.

## Claims chính của implementer cần kiểm chứng

- Lesson template có metadata hai tầng và các section bắt buộc đúng thứ tự.
- Quiz có 8–12 câu theo tỷ lệ xấp xỉ 40% nhớ/hiểu, 40% áp dụng, 20% giới hạn bằng chứng; không lộ đáp án.
- Answer key có đáp án, giải thích đúng/sai từng lựa chọn, LO mapping, heading link và Claim ID khi cần.
- Mã `LO-WNN-*`, `Q[NN]-*`, `CNN-*` nhất quán giữa ba template.
- Demo `Q-DEMO-01` về tương quan/nhân quả có lời giải giúp học lại và dùng `C00-03` đúng phạm vi.
- Reflection tự nguyện, có case giả định và không yêu cầu thông tin nhạy cảm.
- Link tương đối và heading anchors được mô tả là hợp lệ; phải kiểm tra thực tế, không chỉ đọc tuyên bố.
- Task 1 không bị sửa; Task 3 và Tuần 1 chưa được triển khai.

## Điểm review bắt buộc

### Spec compliance

- So từng acceptance criterion của Task 2 với nội dung thật.
- Kiểm tra số lượng/thứ tự section, không dựa vào cách implementer tự đếm.
- Placeholder phải rõ là placeholder, không giống claim/citation thật.

### Scientific and source integrity

- Demo không diễn giải sai tương quan, nhân quả hoặc p-value.
- `C00-03` thực sự hỗ trợ lời giải demo.
- Template không ép mọi nghiên cứu phải có effect size/CI khi nguồn không báo cáo.
- Nhãn bằng chứng và Claim ID phù hợp `source-policy.md`.

### Learning quality

- Tầng cốt lõi được thiết kế tự đủ cho câu nhớ/hiểu.
- Mục tiêu có thể đo và mapping quiz/key có thể triển khai thật.
- Distractor instructions không khuyến khích câu mẹo.
- Answer key yêu cầu giải thích từng phương án sai và hỗ trợ học lại.

### Safety and ethics

- Không đọc vị, thao túng, tự chẩn đoán hoặc gán nhãn.
- Reflection không bắt buộc tiết lộ trauma/bệnh sử/tình dục/xung đột gia đình.
- Case giả định là lựa chọn thực sự tương đương.

### Technical integrity

- Kiểm tra Markdown, heading hierarchy và code fences/details.
- Kiểm tra link tương đối theo vị trí file đích dự kiến.
- Kiểm tra anchor mẫu không được trình bày như đã chắc chắn hoạt động nếu slug chưa tồn tại.
- Kiểm tra mapping mã giữa ba file bằng grep/đối chiếu trực tiếp.

## Lưu ý về báo cáo implementer

Implementer tự báo cáo toàn bộ 43 tiêu chí PASS, ngoại trừ `TEC-03` và `TEC-04` là N/A. Reviewer phải tự đánh giá lại. Các điểm có rủi ro cần chú ý:

1. Báo cáo nói lesson có “12 section”, nhưng acceptance criteria liệt kê 13 mục khi tách hai mini-review; xác định cấu trúc thực tế có đáp ứng ý nghĩa spec hay không.
2. Demo được đặt trong cả quiz template và answer-key template; kiểm tra quiz demo có vô tình làm lộ đáp án hoặc pattern đáp án không.
3. Các anchor mẫu được implementer thừa nhận cần cập nhật khi dùng; đánh giá xem placeholder đủ rõ và an toàn hay là lỗi Important.
4. Các tuyên bố như “định nghĩa chuẩn APA” trong glossary hoặc “5 yếu tố consent” phải không tạo claim mặc định chưa có nguồn.
5. GitHub alerts và `<details>` phải không được tuyên bố là portable Markdown nếu môi trường render mục tiêu chưa được định nghĩa.

## Cách ra finding

Mỗi finding dùng đúng format trong `REVIEW_RULES.md`:

```text
[Severity] Tiêu đề
- Vị trí: file:line hoặc heading
- Vấn đề:
- Bằng chứng:
- Tác động:
- Điều kiện pass:
```

Severity: Critical / Important / Minor.

## Verdict và hành động sau review

Chỉ dùng một verdict:

- **APPROVED**: không còn Critical/Important và evidence đầy đủ.
- **CHANGES REQUESTED**: liệt kê findings và không tạo handoff task mới.
- **BLOCKED**: nêu quyết định cụ thể cần người dùng giải quyết.

Nếu **APPROVED**:

1. Tạo handoff thực thi tiếp theo trong `handoff/pm/` với số thứ tự kế tiếp, giao **Task 3** duy nhất.
2. Handoff Task 3 phải tự đủ ngữ cảnh theo `AGENTS.md` và `REVIEW_RULES.md`.
3. Báo rõ đường dẫn handoff mới trong verdict.

Nếu **CHANGES REQUESTED** hoặc **BLOCKED**, không tạo handoff Task 3.

## Phạm vi

- Không sửa ba template trong lúc review.
- Không thực hiện Task 3.
- Không viết Tuần 1.
- Không tạo handoff tiếp theo trước verdict APPROVED.
