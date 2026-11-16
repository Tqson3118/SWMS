# AGENTS.md — Quy tắc thực thi khóa học Tâm lý học

## Vai trò

- **Agent thực thi** chịu trách nhiệm nghiên cứu nguồn, viết/sửa nội dung và cung cấp bằng chứng kiểm tra.
- **Agent hiện tại (reviewer)** chỉ review đầu ra theo các quality gate; không viết hộ phần còn thiếu trong vòng review.
- Không đánh dấu task hoàn thành trước khi reviewer trả kết luận **APPROVED**.
- Nếu reviewer yêu cầu sửa, agent thực thi phải sửa đúng findings, cập nhật bằng chứng và gửi lại để re-review.

## Nguồn sự thật

Đọc theo thứ tự trước khi làm:

1. `docs/superpowers/specs/2026-08-28-psychology-curriculum-design.md` — thiết kế và phạm vi đã duyệt.
2. `implementation_plan.md` — thứ tự task, checkpoint và Definition of Done.
3. `source-policy.md` — chính sách nguồn sau khi Task 1 tạo file này.
4. `quality/content-audit-checklist.md` — quality gate sau khi Task 1 tạo file này.

Khi có mâu thuẫn, dừng và hỏi người dùng; không tự chọn văn bản thuận tiện hơn.

## Cách làm bắt buộc

1. Làm đúng thứ tự Task 1 → 12 trong `implementation_plan.md`.
2. Chỉ làm phạm vi của task hiện tại; không sinh đồng loạt 48 tuần.
3. Dừng ở mỗi checkpoint để reviewer kiểm tra.
4. Tuần 1 là vertical slice bắt buộc. Không viết Tuần 2 trước khi Tuần 1 được duyệt.
5. Mỗi tuần sau đó phải hoàn thành bài học, quiz, answer key, source ledger và audit trước khi sang tuần tiếp theo.
6. Không xóa hoặc ghi đè nội dung người dùng ngoài phạm vi task.
7. Báo rõ mọi file đã tạo/sửa, nguồn đã xác minh, lệnh/kiểm tra đã chạy và hạn chế còn lại.

## Quy tắc khoa học và nguồn

- Không bịa tên tác giả, tên bài, năm, edition, DOI, URL, số liệu, effect size hoặc trích dẫn.
- Mọi claim quan trọng phải có nguồn truy xuất được và một dòng tương ứng trong `quality/source-ledger.md`.
- Phải thật sự mở và đối chiếu URL/DOI; chỉ nhìn snippet tìm kiếm không được tính là xác minh.
- Ưu tiên theo thứ tự: giáo trình đại học có edition; systematic review/meta-analysis/guideline; cơ quan chuyên môn; nghiên cứu gốc; nguồn phổ thông.
- Self-help, blog, video và nội dung phổ thông không được làm nguồn duy nhất cho claim khoa học.
- Phân biệt rõ tương quan với nhân quả; statistical significance với practical significance.
- Nếu bằng chứng yếu, hỗn hợp, replication kém hoặc còn tranh luận, phải ghi rõ bằng một trong bốn nhãn: **Mạnh**, **Khá**, **Sơ bộ**, **Tranh luận**.
- Nêu giới hạn mẫu, văn hóa, đo lường và khả năng khái quát khi có liên quan.
- Không nâng một nghiên cứu đơn lẻ thành chân lý chung.

## Hợp đồng nội dung mỗi tuần

Mỗi bài bắt buộc có:

- Mục tiêu học tập có thể kiểm tra.
- “Học nhanh khi mệt” trong khoảng 30–45 phút.
- Bài đọc sâu, tổng khối lượng mục tiêu khoảng 2.000–3.500 từ nếu chủ đề cần.
- 8–15 thuật ngữ trong glossary.
- Nghiên cứu/mô hình tiêu biểu có nguồn và giới hạn.
- Mục **Biết được gì / Chưa thể kết luận gì**.
- Mục hiểu lầm phổ biến hoặc phản biện pop psychology khi phù hợp.
- Hai mini-review giữa tuần.
- Quiz 8–12 câu: khoảng 40% nhớ/hiểu, 40% áp dụng, 20% đánh giá giới hạn bằng chứng.
- Answer key tách file, có giải thích, lý do phương án nhiễu sai, mục tiêu được đo và link về bài.
- Case study hoặc journal tự nguyện; luôn có lựa chọn dùng tình huống giả định.
- Danh mục tài liệu tham khảo đã xác minh.

Không kéo dài bài bằng lặp ý hoặc diễn giải rỗng chỉ để đủ số từ.

## Quy tắc an toàn và đạo đức

- Không dạy “đọc vị”, phát hiện nói dối chắc chắn, thao túng, cưỡng ép hoặc khai thác điểm yếu.
- Không tự chẩn đoán hoặc chẩn đoán người khác từ một vài dấu hiệu.
- Không dùng nhãn như “toxic”, “narcissist”, “psychopath”, “bệnh” như kết luận thiếu đánh giá chuyên môn.
- Không biến attachment style, Big Five, MBTI, love languages hoặc self-screening thành bản chất cố định hay định mệnh.
- Dùng ngôn ngữ person-first, continuum và chống kỳ thị trong nội dung lâm sàng.
- Không yêu cầu người học tiết lộ trauma, bệnh sử, tình dục, xung đột gia đình hoặc dữ liệu định danh.
- Với consent: phải mô tả là tự nguyện, cụ thể, có thông tin, có thể rút lại; không suy từ im lặng.
- Với nguy cơ tức thời: hướng người đọc tới dịch vụ khẩn cấp địa phương, người tin cậy hoặc chuyên gia; không giả định quốc gia.
- Nội dung không thay thế tư vấn, chẩn đoán hoặc điều trị chuyên môn.

## Tiêu chuẩn viết

- Viết tiếng Việt tự nhiên, rõ ràng; thuật ngữ tiếng Anh đặt trong ngoặc ở lần xuất hiện đầu tiên.
- Giải thích khái niệm trước, sau đó mới dùng thuật ngữ viết tắt.
- Dùng ví dụ cụ thể nhưng không ngụy tạo case như dữ liệu nghiên cứu thật.
- Phân biệt rõ dữ kiện, diễn giải, giả thuyết và ý kiến thực hành.
- Tránh tuyệt đối hóa: “luôn luôn”, “chắc chắn”, “mọi người” nếu nguồn không hỗ trợ.
- Không viết giọng phán xét, định mệnh hóa hoặc hứa hẹn thay đổi chắc chắn.
- Markdown phải render được, heading nhất quán và link tương đối đúng.

## Gói bàn giao để review

Mỗi lần yêu cầu review, agent thực thi phải gửi:

1. Task/tuần đã làm và danh sách file thay đổi.
2. Tóm tắt mục tiêu và những gì đã hoàn thành.
3. Danh sách nguồn chính, loại nguồn và cách đã xác minh.
4. Kết quả checklist, gồm mọi mục `N/A — lý do`.
5. Cách đã kiểm tra link, Markdown, quiz–objective mapping và claim–source mapping.
6. Các điểm chưa chắc chắn, trade-off và hạn chế còn lại.
7. Không được tự tuyên bố **APPROVED**; đó là verdict của reviewer.

## Điều kiện dừng

Dừng và báo blocker nếu:

- Không thể mở/xác minh nguồn cho claim quan trọng.
- Hai nguồn chất lượng cao mâu thuẫn và ảnh hưởng kết luận bài.
- Yêu cầu mới xung đột thiết kế hoặc quy tắc an toàn.
- Không thể đáp ứng checkpoint mà không làm vượt phạm vi.
- Có nguy cơ tạo hướng dẫn gây hại, chẩn đoán hoặc thao túng.

## Quy trình handoff sau checkpoint

Sau mỗi task hoặc checkpoint được reviewer trả verdict **APPROVED**:

1. Reviewer tạo đúng một file handoff mới trong `handoff/pm/`.
2. Tên file theo mẫu `NNN-task-NN-<slug>.md`; số đầu tăng dần. Không ghi đè handoff cũ.
3. Handoff phải là prompt độc lập cho phiên chat mới, không giả định agent đã đọc lịch sử chat.
4. Handoff ghi rõ: vai trò, trạng thái đã duyệt, task kế tiếp, file phải đọc, phạm vi sửa, acceptance criteria, quality gate, điều kiện dừng và gói bàn giao.
5. Agent mới đọc `AGENTS.md` trước rồi đọc các nguồn sự thật trong handoff.
6. Agent chỉ làm task được giao, dừng đúng checkpoint và không tự tạo handoff kế tiếp.
7. Chỉ reviewer tạo handoff tiếp theo sau verdict **APPROVED**.
8. Nếu **CHANGES REQUESTED**, không tạo handoff task mới; agent hiện tại sửa findings và gửi re-review.
9. Nếu **BLOCKED**, chỉ tạo handoff sau khi người dùng giải quyết blocker.
