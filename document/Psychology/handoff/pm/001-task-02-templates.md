# Handoff 001 — Task 2: Tạo template bài học và đánh giá

## Vai trò

Bạn là **agent thực thi** của dự án khóa tự học Tâm lý học 48 tuần. Chỉ thực hiện Task 2; reviewer ở phiên khác sẽ đánh giá. Không tự tuyên bố APPROVED.

## Trạng thái đã duyệt

- Task 1 và Checkpoint 1: **APPROVED**.
- Artifact ổn định: `source-policy.md`, `resources.md`, `quality/content-audit-checklist.md`, `quality/source-ledger.md`.
- Không sửa artifact Task 1; nếu thật sự bị cản trở, dừng và báo blocker.

## Đọc bắt buộc theo thứ tự

1. `AGENTS.md`
2. `docs/superpowers/specs/2026-08-28-psychology-curriculum-design.md`
3. `implementation_plan.md`, phần Task 2
4. `source-policy.md`
5. `quality/content-audit-checklist.md`
6. `REVIEW_RULES.md`

Nếu mâu thuẫn, dừng và hỏi người dùng.

## Nhiệm vụ duy nhất

Tạo:
- `templates/lesson-template.md`
- `templates/quiz-template.md`
- `templates/answer-key-template.md`

Không viết Tuần 1 và không làm Task 3.

## Acceptance criteria — Lesson

Có metadata: tuần, chặng, chủ đề, thời lượng hai tầng, prerequisite, mục tiêu đo được, cảnh báo an toàn khi cần.

Section theo thứ tự: ý nghĩa; mục tiêu; học nhanh; đọc sâu; nghiên cứu/mô hình; biết gì/chưa kết luận gì; hiểu lầm/pop psychology; hai mini-review; case/journal tự nguyện có lựa chọn giả định; glossary 8–15 thuật ngữ; nguồn đã xác minh; link quiz.

Chỉ dẫn rõ nơi đặt Claim ID, nhãn bằng chứng và ledger mapping; placeholder không được giả làm claim thật.

## Acceptance criteria — Quiz

- 8–12 câu: khoảng 40% nhớ/hiểu, 40% áp dụng, 20% giới hạn bằng chứng.
- Không có đáp án/gợi ý làm lộ đáp án, câu mẹo, tự chẩn đoán hoặc chẩn đoán người khác.
- Mỗi câu có mã ổn định và mapping nội bộ tới mục tiêu để answer key tái sử dụng.

## Acceptance criteria — Answer key

Mỗi câu có mã khớp quiz, đáp án, giải thích đúng, giải thích phương án nhiễu, mục tiêu được đo, link tới heading bài và Claim ID/citation khi cần.

## Kiểm thử bắt buộc

Dùng mục tiêu giả định **Phân biệt tương quan với nhân quả** để tạo một case và lời giải mẫu. Đánh dấu rõ đây là demo, không phải nội dung khóa học. Lời giải phải giúp học lại dù trả lời sai; không bịa nguồn hay số liệu.

## Quality gate

- Đối chiếu Task 2 và checklist đã duyệt.
- Kiểm tra Markdown, heading, link tương đối dự kiến và mã mục tiêu–câu hỏi–đáp án.
- Xác nhận không yêu cầu dữ liệu nhạy cảm, đọc vị hoặc tự chẩn đoán.

## Điều kiện dừng

Dừng nếu có xung đột policy/checklist, mapping không thể nhất quán hoặc cần sửa artifact Task 1.

## Gói bàn giao tại Checkpoint 2

1. Danh sách file tạo/sửa.
2. Tóm tắt ba template.
3. Mapping acceptance criteria tới file/heading/dòng.
4. Kết quả kiểm tra Markdown, link và consistency mã.
5. Checklist, gồm `N/A — lý do`.
6. Trade-off/hạn chế.
7. Xác nhận chưa làm Task 3 và chưa viết Tuần 1.

Dừng chờ reviewer; không tự tạo handoff tiếp theo.
