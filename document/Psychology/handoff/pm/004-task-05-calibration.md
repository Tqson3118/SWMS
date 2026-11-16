# Handoff 004 — Task 5: Hiệu chỉnh sau tuần mẫu (Post-Sample Calibration)

## Vai trò

Bạn là **agent thực thi** của khóa tự học Tâm lý học 48 tuần. Chỉ thực hiện **Task 5 — Hiệu chỉnh sau tuần mẫu** trong phạm vi dưới đây; reviewer độc lập sẽ đánh giá sau khi bạn bàn giao. Không tự tuyên bố APPROVED và **tuyệt đối không viết Tuần 02 (W02)**.

## Trạng thái đã duyệt

- Task 1 / Checkpoint 1: **APPROVED** — Quality gate và hệ thống chính sách nguồn.
- Task 2 / Checkpoint 2: **APPROVED** — Ba template bài học, quiz và answer key.
- Task 3: **APPROVED** — Lộ trình 48 tuần, tracker W01–W48 và glossary seed.
- Task 4 / Checkpoint 3 (Tuần 01 Vertical Slice): **APPROVED** sau vòng re-review độc lập ngày 2026-09-09. Cả 3 phát hiện cũ (TEC-07, SCC-03, TEC-04) đã hoàn toàn được giải quyết; chất lượng khoa học, sư phạm, an toàn và kỹ thuật đạt chuẩn.

## Nguyên tắc bắt buộc

- Đọc `Psychology/AGENTS.md` trước; tuân thủ nghiêm ngặt **Strict Deletion Ban**, nguyên tắc **Research-First**, ESM-only nếu có code, bảo vệ `qa/evidence/` và ảnh `SIM-*.png`.
- **Tuyệt đối không viết Tuần 02 (W02)** hoặc bất kỳ tuần nào khác trong Task 5. Task 6 mới là thời điểm triển khai W02–W08.
- Không tạo handoff kế tiếp; chỉ reviewer tạo handoff sau khi Task 5 được duyệt.
- Handoff cũ là tài liệu bất biến; không xóa hoặc ghi đè các handoff cũ (`001`, `002`, `003`).

## File phải đọc theo thứ tự

1. `Psychology/AGENTS.md`
2. `Psychology/REVIEW_RULES.md`
3. `Psychology/implementation_plan.md` — Trọng tâm phần Task 5 và Checkpoint 3 / Definition of Done.
4. `Psychology/quality/week-01-audit-report.md` — Báo cáo audit Tuần 01 đã được duyệt.
5. `Psychology/templates/lesson-template.md`, `quiz-template.md`, `answer-key-template.md`.
6. `Psychology/quality/content-audit-checklist.md`.
7. `Psychology/learning-roadmap.md`, `progress.md`, `glossary.md`.

## Phạm vi file được phép chỉnh sửa

### Được phép chỉnh sửa
- `Psychology/templates/lesson-template.md`, `quiz-template.md`, `answer-key-template.md` — Cập nhật quy ước chuẩn hóa rút ra từ tuần mẫu.
- `Psychology/quality/content-audit-checklist.md` — Cập nhật Definition of Done Phiên bản 1, ghi ngày hiệu chỉnh và làm rõ tiêu chí đo lường.
- `Psychology/learning-roadmap.md` — Cập nhật trạng thái Tuần 01 (chuyển nhãn `[Sắp biên soạn]` của W01 sang trạng thái bài mẫu đã xuất bản).
- `Psychology/progress.md` — Đánh dấu hoàn thành cho Tuần 01 sau khi đã được duyệt Checkpoint 3.
- `Psychology/notes/01-scientific-foundations/week-01-what-is-psychology.md` — Chỉ chỉnh sửa nếu cần đồng bộ với các chuẩn hiệu chỉnh của Task 5.
- `Psychology/quality/week-01-audit-report.md` — Cập nhật số liệu tự kê khai inventory nếu có chỉnh sửa Tuần 01.

### Nghiêm cấm chỉnh sửa hoặc tạo mới
- **Cấm tạo mới bất kỳ file nào thuộc Tuần 02** (`notes/.../week-02-*.md`, `quizzes/week-02-*.md`, `answer-keys/week-02-*.md`).
- Cấm tạo file audit trong `handoff/pm/`. Toàn bộ báo cáo audit thuộc thư mục `quality/`.
- Cấm xóa bất kỳ file nào trong workspace.

## Nhiệm vụ cụ thể của Task 5

1. **Chuẩn hóa quy ước heading anchor và liên kết:**
   - Đưa quy ước tạo slug anchor Unicode tiếng Việt giữ dấu vào `templates/answer-key-template.md` và `lesson-template.md` để đảm bảo tính nhất quán trên GitHub/Markdown preview cho toàn bộ 47 tuần sau.
2. **Làm rõ tiêu chí đo lường khối lượng từ (SCC-03) trong Quality Gate:**
   - Cập nhật ghi chú trong `quality/content-audit-checklist.md` về cách tính từ tiếng Việt (phân biệt âm tiết đơn lập với từ vựng khái niệm) và quy định rõ khối lượng của Deep Dive lý thuyết (Mục 4) vs khối lượng tổng các mục bổ trợ (Mục 5–7).
3. **Cập nhật Definition of Done (DoD) Phiên bản 1:**
   - Ghi nhận ngày hiệu chỉnh (2026-09-09) và phiên bản DoD v1.0 chính thức vào cuối `quality/content-audit-checklist.md`.
4. **Đồng bộ trạng thái bài mẫu trên Lộ trình và Tracker:**
   - Cập nhật `learning-roadmap.md`: gỡ nhãn `[Sắp biên soạn]` tại W01 để phản ánh bài học, quiz và đáp án đã chính thức hoàn thành; giữ nguyên nhãn `[Sắp biên soạn]` cho W02–W48.
   - Cập nhật `progress.md`: chuyển trạng thái W01 từ `—` sang `✓` (hoặc ghi nhận ngày hoàn thành 2026-09-09).
5. **Khắc phục Technical Debt quản trị tài liệu:**
   - Ghi nhận quy chuẩn: Báo cáo audit các tuần tiếp theo chỉ lưu duy nhất tại `quality/week-NN-audit-report.md`; không tạo bản sao trong `handoff/pm/`. Giữ nguyên `handoff/pm/003-task-04-week-01-audit.md` như hồ sơ lưu trữ lịch sử theo quy tắc Strict Deletion Ban.
   - Chuẩn hóa số liệu tự kê khai (lines, bytes) của audit report trong bảng inventory để phản ánh số đo thực tế sau khi file hoàn thiện.

## Acceptance Criteria và Quality Gate

- Ba template (`lesson-template.md`, `quiz-template.md`, `answer-key-template.md`) được cập nhật các quy ước chuẩn hóa để ngăn lỗi hệ thống ở 47 tuần sau.
- `quality/content-audit-checklist.md` có ghi chú DoD v1.0 và ngày hiệu chỉnh rõ ràng.
- `learning-roadmap.md` và `progress.md` đồng bộ chính xác trạng thái W01 đã hoàn thành.
- Không có bất kỳ file nào thuộc Tuần 02 được tạo ra.
- Toàn bộ liên kết tương đối nội bộ tiếp tục pass 100%, không phát sinh broken link.

## Điều kiện dừng

Dừng và báo blocker nếu:
- Phát hiện xung đột giữa template mới và thiết kế tổng thể của khóa học.
- Có yêu cầu xóa hoặc cấu trúc lại các handoff cũ đã ban hành.
- Có nhầm lẫn dẫn tới việc bắt đầu viết Tuần 02 trước khi Task 5 được review duyệt.

## Gói bàn giao cho Reviewer

1. Danh sách các file đã cập nhật trong Task 5 kèm diff tóm tắt.
2. Bản mô tả các quy ước đã được đưa vào template để bảo vệ 47 tuần sau.
3. Trích đoạn DoD v1.0 đã bổ sung vào `quality/content-audit-checklist.md`.
4. Kết quả kiểm tra liên kết nội bộ sau khi cập nhật roadmap và tracker.
5. Xác nhận không có file W02 nào được tạo.
6. Không tự tuyên bố APPROVED; dừng chờ reviewer đánh giá.
