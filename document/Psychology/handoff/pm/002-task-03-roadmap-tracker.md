# Handoff 002 — Task 3: Cập nhật điều hướng và tracker

## Vai trò

Bạn là agent thực thi của khóa tự học Tâm lý học 48 tuần. Chỉ thực hiện Task 3 trong phạm vi dưới đây; reviewer độc lập sẽ đánh giá. Không tự tuyên bố APPROVED.

## Trạng thái đã duyệt

- Task 1 / Checkpoint 1: APPROVED — quality gate và hệ thống nguồn đã được duyệt.
- Task 2 / Checkpoint 2: APPROVED — ba template đã được re-review trực tiếp.
- Task 4 / Tuần 1: chưa bắt đầu.

## Nguyên tắc bắt buộc

- Đọc Psychology/AGENTS.md trước; tuân thủ strict deletion ban, research-first, bảo vệ qa/evidence/ và ảnh SIM-*.png.
- Nếu có code, chỉ dùng ESM import; task này không cần code.
- Không sửa Task 1 artifacts, ba template Task 2, source ledger hay source policy.
- Không viết Tuần 1, không tạo handoff kế tiếp.

## File phải đọc theo thứ tự

1. Psychology/AGENTS.md
2. Psychology/docs/superpowers/specs/2026-08-28-psychology-curriculum-design.md
3. Psychology/implementation_plan.md — Task 3
4. Psychology/learning-roadmap.md — bản hiện tại
5. Psychology/templates/lesson-template.md, quiz-template.md, answer-key-template.md — chỉ để đồng bộ link/section, không sửa
6. Psychology/source-policy.md và Psychology/quality/content-audit-checklist.md — quy tắc link/tracker/glossary

## Nhiệm vụ duy nhất và phạm vi file

### Sửa
- Psychology/learning-roadmap.md

### Tạo mới
- Psychology/progress.md
- Psychology/glossary.md

Không sửa hoặc tạo file Psychology nào khác.

## Acceptance criteria đo được

### learning-roadmap.md

- Có đúng 48 tuần thuộc đúng 6 chặng, theo curriculum spec. Phân bổ chính xác là 8+8+8+10+8+6 vì spec quy định Chặng 4 có 10 tuần.
- Chặng 1 W01–W08: Nền tảng khoa học; Chặng 2 W09–W16: Hiểu bản thân; Chặng 3 W17–W24: Hiểu người khác; Chặng 4 W25–W34: Tình cảm và các mối quan hệ; Chặng 5 W35–W42: Phát triển và sức khỏe tâm thần; Chặng 6 W43–W48: Ứng dụng tích hợp.
- Không còn tiêu đề “24 tuần” hoặc link bài không tồn tại được trình bày như đã hoàn thành.
- Bài chưa có phải hiện [Sắp biên soạn] ở trạng thái link, nhưng giữ đường dẫn ổn định theo spec.
- Có hướng dẫn 3 chế độ học: Mệt (chỉ Fast Track/Core), Bình thường (đầy đủ + hai mini-review + quiz), Có hứng (Deep Dive, case/journal tự nguyện và đọc mở rộng).
- Ghi rõ journal/case reflection là tự nguyện, luôn có lựa chọn hypothetical case.

### progress.md

- Có tracker cho đủ W01–W48; mỗi tuần có 5 trạng thái/cột: Core, Deep Dive, Mini-Review, Quiz, Review lại.
- Không đặt deadline mặc định; chỉ để ô trạng thái hoặc ngày người học tự ghi.
- Phân biệt trạng thái Chưa bắt đầu, Đang học, Hoàn thành, Ôn lại.

### glossary.md

- Có ít nhất 10 thuật ngữ seed, mỗi thuật ngữ có tiếng Việt, English term, định nghĩa ngắn, và lưu ý tránh nhầm.
- Tối thiểu gồm: hypothesis, operational definition, correlation, causation, reliability, validity, effect size, bias, replication, evidence.
- Không thêm claim nghiên cứu mới hoặc nguồn giả; nếu định nghĩa cần nguồn cụ thể, ghi nguồn đã có trong resources/ledger hoặc đánh dấu cần xác minh.

## Quality gate trước khi bàn giao

- Đếm đúng tuần/chặng và đối chiếu tên tuần với spec.
- Kiểm tra link Markdown: bài chưa có không được trỏ tới file đã tồn tại giả; dùng nhãn [Sắp biên soạn].
- Đồng bộ tên tuần/link giữa roadmap và progress.
- Kiểm tra 3 chế độ học và journal tự nguyện hiện rõ.
- Kiểm tra glossary có đủ 10 thuật ngữ và không dùng nguồn nội bộ AI làm citation.
- Không chạm file ngoài phạm vi; không xóa, di chuyển hoặc ghi đè tài sản được bảo vệ.
- Ghi mọi mục không áp dụng là N/A — lý do trong gói bàn giao.

## Điều kiện dừng

Dừng và báo blocker nếu spec/roadmap hiện tại mâu thuẫn không thể giải quyết mà không hỏi người dùng; không tự chuyển sang 24 tuần, Task 4 hoặc chủ đề khác.

## Gói bàn giao cho reviewer

1. Danh sách file sửa/tạo và số dòng.
2. Bảng đối chiếu 48 tuần/6 chặng với spec; nêu rõ phân bổ thực tế 8+8+8+10+8+6.
3. Bằng chứng 3 chế độ học, journal tự nguyện, link [Sắp biên soạn] và tracker 5 cột.
4. Bảng 10+ glossary terms.
5. Kết quả kiểm tra Markdown, link, và phạm vi file.
6. Các hạn chế/điểm chưa chắc chắn; mọi mục N/A — lý do.
7. Không tự tuyên bố APPROVED; dừng chờ reviewer.