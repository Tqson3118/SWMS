# Handoff 003 — Task 4: Vertical slice Psychology Tuần 1

## Vai trò

Bạn là **agent thực thi** của khóa tự học Tâm lý học 48 tuần. Chỉ thực hiện Task 4 — vertical slice Tuần 1 — trong phạm vi dưới đây; reviewer độc lập sẽ đánh giá tại Checkpoint 3. Không tự tuyên bố APPROVED.

## Trạng thái đã duyệt

- Task 1 / Checkpoint 1: **APPROVED** — quality gate và hệ thống nguồn.
- Task 2 / Checkpoint 2: **APPROVED** — ba template đã được re-review trực tiếp; không còn link mẫu cũ hay số liệu demo giả.
- Task 3: **APPROVED** — roadmap 48 tuần, tracker W01–W48 và glossary seed đã được đối chiếu trực tiếp.
- Task 4 / Tuần 1: **chưa bắt đầu**; đây là vertical slice bắt buộc.

## Nguyên tắc bắt buộc

- Đọc `Psychology/AGENTS.md` trước; tuân thủ strict deletion ban, research-first, ESM only nếu có code, bảo vệ `qa/evidence/` và ảnh `SIM-*.png`.
- Không dùng kiến thức nội bộ AI làm nguồn. Phải mở trực tiếp URL/DOI và đối chiếu nội dung; snippet tìm kiếm không đủ.
- Không viết Tuần 2 hoặc bất kỳ tuần nào khác. Không tạo handoff kế tiếp.
- Không dạy đọc vị, thao túng, tự chẩn đoán, gán nhãn hoặc khái quát một nghiên cứu đơn lẻ thành chân lý.
- Reflection/case là tự nguyện; luôn có hypothetical case; không yêu cầu trauma, bệnh sử, đời sống tình dục, xung đột gia đình hoặc dữ liệu định danh.

## File phải đọc theo thứ tự

1. `Psychology/AGENTS.md`
2. `Psychology/docs/superpowers/specs/2026-08-28-psychology-curriculum-design.md` — đặc biệt phần Chặng 1, content contract và tiêu chí hoàn thành.
3. `Psychology/implementation_plan.md` — Task 4 và Checkpoint 3.
4. `Psychology/source-policy.md`.
5. `Psychology/quality/content-audit-checklist.md` và `Psychology/quality/source-ledger.md`.
6. `Psychology/templates/lesson-template.md`, `quiz-template.md`, `answer-key-template.md`.
7. `Psychology/learning-roadmap.md`, `progress.md`, `glossary.md`.

## Phạm vi file được tạo/sửa

### Tạo mới

- `Psychology/notes/01-scientific-foundations/week-01-what-is-psychology.md`
- `Psychology/assessments/quizzes/week-01-quiz.md`
- `Psychology/assessments/answer-keys/week-01-answers.md`

### Cập nhật

- `Psychology/quality/source-ledger.md` — thêm các claim W01 sau khi đã mở và đối chiếu nguồn.
- `Psychology/glossary.md` — chỉ bổ sung thuật ngữ W01 nếu chưa có; không xóa seed hiện có.
- `Psychology/progress.md` — chỉ cập nhật trạng thái W01 sau khi artifact đã hoàn tất để review; không đánh dấu Hoàn thành trước verdict APPROVED.

Không sửa `source-policy.md`, templates, curriculum spec hoặc các tuần khác.

## Mục tiêu học tập bắt buộc

- `[LO-W01-01]` **Định nghĩa** tâm lý học như ngành nghiên cứu khoa học về mind và behavior, phân biệt mô tả khoa học với suy đoán đời thường.
- `[LO-W01-02]` **Trình bày và phân biệt** bốn mục tiêu khoa học: describe, explain, predict và change/modify/control; nêu rằng dự đoán không đồng nghĩa chắc chắn.
- `[LO-W01-03]` **So sánh** các góc nhìn/trường phái lịch sử và đương đại, gồm structuralism, functionalism, psychoanalytic, Gestalt, behaviorism, humanistic và cognitive, theo câu hỏi chúng nhấn mạnh và giới hạn của chúng.
- `[LO-W01-04]` **Đánh giá giới hạn** của một claim pop psychology: yêu cầu bằng chứng, bối cảnh, khả năng khái quát và phân biệt correlation với causation khi phù hợp.

## Yêu cầu bài học

- Dùng đúng 12 H2 của lesson template; không đổi thứ tự.
- Metadata ghi `W01`, Chặng 1, thời lượng hai tầng, prerequisite “Không có — khởi đầu Chặng 1”, trạng thái đang biên soạn.
- Fast Track 600–900 từ, tự đủ cho câu hỏi Nhớ/Hiểu và kết thúc 5 takeaways.
- Deep Dive mục tiêu 2.000–3.500 từ, không kéo dài bằng lặp ý.
- Hai mini-review, mỗi phần 3–5 câu retrieval practice có gợi ý trả lời.
- Key studies/models: mỗi claim có Claim ID `[C01-xx]`, một nhãn 🟢/🟡/🟠/🔴, phương pháp/phạm vi/giới hạn; không bịa effect size/CI. Nếu nguồn không báo cáo, ghi rõ.
- Có mục “Biết được gì / Chưa thể kết luận gì”, đặc biệt về giới hạn suy luận, mẫu/văn hóa và tình trạng tái lập nếu đề cập.
- Phản biện bốn hiểu lầm của Task 4: tâm lý học là đọc suy nghĩ; common sense luôn đủ; một trường phái giải thích mọi thứ; psychologist luôn là therapist. Không dùng ví dụ factual chưa xác minh.
- Dùng person-first và ngôn ngữ xác suất; không dùng một cử chỉ hoặc một nhãn để kết luận con người.

## Nghiên cứu và nguồn tối thiểu cần xác minh trực tiếp

- OpenStax, *Psychology 2e*, Section 1.1 “What Is Psychology?”: `https://openstax.org/books/psychology-2e/pages/1-1-what-is-psychology`.
- OpenStax, *Psychology 2e*, Section 1.2 “History of Psychology”: `https://openstax.org/books/psychology-2e/pages/1-2-history-of-psychology`.
- OpenStax, *Psychology 2e*, Section 1.3 “Contemporary Psychology”: `https://openstax.org/books/psychology-2e/pages/1-3-contemporary-psychology`.
- Có thể dùng thêm giáo trình Cấp 1 hoặc nguồn chuyên môn đã có trong `resources.md`, nhưng phải mở/đối chiếu đúng edition/chapter/section. Không coi HTTP 200 là đủ nếu chưa đọc claim liên quan.
- Xác minh ít nhất 3 claim quan trọng nhất; ghi URL, phạm vi xác minh và ngày kiểm tra vào ledger.

## Quiz và answer key

- Quiz đúng 10 câu: 4 Nhớ/Hiểu, 4 Áp dụng/Phân tích, 2 Đánh giá giới hạn bằng chứng.
- Mã câu `Q01-01`…`Q01-10`; mapping đầy đủ tới `[LO-W01-01]`…`[LO-W01-04]`.
- Quiz không chứa đáp án, không dùng câu hỏi tự chẩn đoán/gán nhãn/đọc vị.
- Answer key tách file; mỗi câu có đáp án, giải thích đáp án đúng, lý do từng distractor sai, LO, link heading bài học và Claim ID/citation phù hợp.
- Kiểm thử correlation/causation chỉ là hypothetical nếu dùng; không đưa số N, r, p hoặc effect size tự tạo.

## Acceptance criteria và quality gate

- Đủ ba file mới và cập nhật ledger/glossary/progress đúng phạm vi.
- 12 H2 đúng thứ tự; Fast Track, Deep Dive, glossary 8–15 thuật ngữ, hai mini-review, case tự nguyện, references và quiz links đều có.
- 100% claim năm/tác giả/kết luận/số liệu trong bài có Claim ID và dòng ledger tương ứng.
- Link nội bộ tới đúng path/anchor thật; không còn token `{{...}}` trong artifact đã xuất bản.
- Claim–source mapping, LO–quiz–answer mapping và Markdown đã được kiểm tra trực tiếp.
- Checklist 43 tiêu chí: mỗi mục PASS hoặc `N/A — lý do`; Critical/Important phải pass trước khi bàn giao.
- Không đánh dấu W01 hoàn thành trên roadmap/progress trước verdict reviewer **APPROVED**.

## Điều kiện dừng

Dừng và báo blocker nếu không thể mở/xác minh nguồn quan trọng, hai nguồn chất lượng cao mâu thuẫn ảnh hưởng kết luận, không thể đáp ứng template trong phạm vi, hoặc có nguy cơ nội dung gây hại/chẩn đoán/thao túng.

## Gói bàn giao cho reviewer

1. Danh sách file tạo/sửa và số dòng.
2. Tóm tắt mục tiêu và phần đã hoàn thành.
3. Danh sách nguồn chính, loại nguồn, URL/DOI, phạm vi và cách xác minh.
4. Claim inventory và bảng claim–source mapping; mọi `N/A — lý do`.
5. Kết quả kiểm tra Markdown/link/heading, quiz–objective mapping và answer-key mapping.
6. Điểm chưa chắc chắn, trade-off và hạn chế.
7. Không tự tuyên bố APPROVED; dừng chờ reviewer.