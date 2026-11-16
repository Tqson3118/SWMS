# REVIEW_RULES.md — Quy tắc review khóa học Tâm lý học

## Vai trò reviewer

Reviewer đánh giá, chỉ ra bằng chứng và ra verdict; không âm thầm sửa hộ agent thực thi. Review ưu tiên tính đúng khoa học, an toàn và khả năng học hơn văn phong sở thích cá nhân.

## Verdict được phép

- **APPROVED:** Đạt spec, không còn lỗi Critical/Important và bằng chứng kiểm tra đầy đủ.
- **CHANGES REQUESTED:** Có lỗi cụ thể cần sửa; mỗi finding phải có severity, vị trí, lý do và điều kiện pass.
- **BLOCKED:** Không thể xác minh hoặc có xung đột cần người dùng quyết định.

Không dùng “gần đạt”, “ổn thôi” hoặc approve có điều kiện mơ hồ.

## Thứ tự review

### 1. Spec compliance

- Đúng task và không vượt phạm vi.
- Đủ các section/content contract bắt buộc.
- Đúng cấu trúc 48 tuần và prerequisite.
- Đã dừng đúng checkpoint.

### 2. Scientific correctness

- Claim quan trọng có citation và ledger entry.
- Citation thực sự hỗ trợ claim, không chỉ liên quan chủ đề.
- Không bịa/sai tác giả, năm, edition, DOI, URL, số liệu.
- Mức độ khẳng định tương xứng chất lượng bằng chứng.
- Tương quan/nhân quả, giới hạn mẫu, văn hóa và replication được xử lý đúng.

### 3. Learning quality

- Mục tiêu có thể đo và được quiz đo đúng.
- Tầng cốt lõi tự đủ cho kiến thức thiết yếu.
- Ví dụ giúp hiểu chứ không thay bằng chứng.
- Quiz không mẹo, không đòi kiến thức chưa dạy; answer key thực sự giải thích.
- Nội dung sâu nhưng không lặp để đủ độ dài.

### 4. Safety and ethics

- Không đọc vị, thao túng, gán nhãn hoặc tự chẩn đoán.
- Không định mệnh hóa attachment, personality test hoặc bệnh lý.
- Reflection không ép tiết lộ dữ liệu nhạy cảm.
- Nội dung consent, quan hệ nguy hiểm và sức khỏe tâm thần có guardrail phù hợp.
- Ngôn ngữ person-first và chống kỳ thị.

### 5. Documentation integrity

- Markdown render đúng; link nội bộ tồn tại và link ngoài đã kiểm tra.
- Glossary nhất quán giữa các tuần.
- Roadmap/progress/ledger khớp trạng thái thật.
- Không đánh dấu hoàn thành khi file hoặc audit còn thiếu.

## Severity

- **Critical:** Nội dung có thể gây hại; bịa nguồn/số liệu; hướng dẫn thao túng; chẩn đoán thiếu căn cứ; sai khoa học làm đảo ngược kết luận chính.
- **Important:** Thiếu nguồn cho claim chính; citation không hỗ trợ claim; thiếu phần bắt buộc; quiz sai; ngôn ngữ quá chắc chắn; link/nội dung quan trọng không dùng được.
- **Minor:** Lỗi diễn đạt, format hoặc cải thiện nhỏ không làm sai kiến thức hay trải nghiệm cốt lõi.

## Format finding

~~~text
[Severity] Tiêu đề ngắn
- Vị trí: file:line hoặc heading/câu hỏi
- Vấn đề: mô tả điều đang sai hoặc thiếu
- Bằng chứng: đối chiếu spec/nguồn/nội dung liên quan
- Tác động: vì sao ảnh hưởng người học
- Điều kiện pass: kết quả quan sát được sau khi sửa
~~~

Reviewer không viết thay lời giải hoàn chỉnh, nhưng có thể đưa ví dụ tối thiểu để làm rõ điều kiện pass.

## Evidence bắt buộc trước APPROVED

- Danh sách file thay đổi đã được đọc.
- Checklist đã đối chiếu, không chỉ tin báo cáo của implementer.
- Kiểm tra mẫu claim–source trực tiếp; với bài mẫu/clinical/relationship phải kiểm tra mọi claim có rủi ro cao.
- Kiểm tra mục tiêu–quiz–answer key mapping.
- Kiểm tra link và trạng thái file thực tế.
- Findings vòng trước đã được re-review và đánh dấu resolved từng mục.

## Nguyên tắc re-review

- Chỉ đóng finding khi điều kiện pass đã được chứng minh.
- Không coi “đã sửa” trong lời báo cáo là bằng chứng.
- Nếu bản sửa gây lỗi Critical/Important mới, mở finding mới.
- Minor có thể ghi nhận để xử lý sau; Critical/Important không được bỏ qua khi approve.
- Nếu agent và reviewer bất đồng do plan/spec mâu thuẫn, chuyển **BLOCKED** để người dùng quyết định.

## Quy trình tạo handoff sau APPROVED

Sau mỗi verdict **APPROVED**, reviewer phải tạo một file mới trong `handoff/pm/` trước khi kết thúc lượt review.

Handoff phải tự đủ ngữ cảnh; xác nhận checkpoint vừa duyệt; giao duy nhất task kế tiếp; liệt kê file phải đọc và được phép sửa; nêu acceptance criteria, quality gate, điều kiện dừng và format bàn giao theo `AGENTS.md`.

Không tạo handoff sang task mới khi verdict là **CHANGES REQUESTED** hoặc **BLOCKED**. Handoff cũ là hồ sơ bất biến; bản thay thế phải là file mới có số tăng dần.

## Handoff để mở phiên reviewer mới

Khi agent thực thi đã bàn giao một task/checkpoint nhưng chưa được review, phiên điều phối tạo một file trong `handoff/pm/review/` theo mẫu `task-NN-checkpoint-NN-review.md`.

File review handoff phải tự đủ ngữ cảnh nhưng ngắn gọn: nêu task cần review, verdict trước đó nếu có, artifact phải đọc, claim của implementer cần kiểm chứng, checklist bắt buộc, phạm vi không được sửa và output/verdict yêu cầu.

Reviewer phiên mới phải đọc file handoff rồi trực tiếp đọc artifact; không coi báo cáo implementer là bằng chứng. Reviewer không sửa hộ. Nếu APPROVED, reviewer tạo handoff thực thi task tiếp theo trong `handoff/pm/` theo quy trình hiện có. Nếu CHANGES REQUESTED/BLOCKED, không tạo handoff task mới.
