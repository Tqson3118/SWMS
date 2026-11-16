# Kế hoạch triển khai lộ trình tự học Tâm lý học 48 tuần

> **Dành cho agent thực thi:** Triển khai tuần tự và dừng ở từng checkpoint. Không sinh đồng loạt 48 bài trước khi tuần mẫu được người học duyệt.

**Mục tiêu:** Tạo khóa tự học bằng tiếng Việt có nền tảng khoa học, bài đọc sâu, quiz có giải thích và nội dung ứng dụng để hiểu bản thân, người khác, tình cảm và quan hệ.

**Kiến trúc:** `learning-roadmap.md` điều hướng sáu module. Mỗi tuần dùng một content contract hai tầng; quiz và đáp án tách file. Chính sách nguồn, source ledger và audit checklist là quality gate bắt buộc.

**Đặc tả:** [Thiết kế lộ trình 48 tuần](docs/superpowers/specs/2026-08-28-psychology-curriculum-design.md)

## Ràng buộc toàn cục

- Đúng 48 tuần, sáu chặng và thứ tự trong đặc tả.
- Mỗi tuần có tầng cốt lõi 30–45 phút và phần đọc sâu để tổng thời gian đạt 2–3 giờ hoặc hơn.
- Bài chính khoảng 2.000–3.500 từ, glossary 8–15 thuật ngữ, hai mini-review, quiz 8–12 câu, đáp án giải thích và case/journal tự nguyện.
- Tầng cốt lõi phải đủ để trả lời câu hỏi kiến thức thiết yếu.
- Không dạy đọc vị, thao túng, tự chẩn đoán hoặc chẩn đoán người khác.
- Reflection luôn cho phép dùng case giả định; không yêu cầu tiết lộ thông tin nhạy cảm.
- Không bịa tác giả, năm, DOI, URL, số liệu hoặc trích dẫn. Claim quan trọng phải truy được tới nguồn.
- Bằng chứng yếu, hỗn hợp hoặc tranh luận phải ghi rõ; self-help không làm nguồn duy nhất.
- Quiz đo kiến thức và áp dụng, không tạo điểm “tính cách”, “trí tuệ” hoặc “bệnh lý”.
- Workspace hiện không phải Git repository; dùng checkpoint thay commit.

## Sơ đồ đầu ra

~~~text
Psychology/
├── learning-roadmap.md
├── source-policy.md
├── resources.md
├── glossary.md
├── progress.md
├── templates/
│   ├── lesson-template.md
│   ├── quiz-template.md
│   └── answer-key-template.md
├── quality/
│   ├── content-audit-checklist.md
│   ├── source-ledger.md
│   └── final-audit-report.md
├── notes/
│   ├── 01-scientific-foundations/       # tuần 01–08
│   ├── 02-understanding-self/            # tuần 09–16
│   ├── 03-understanding-others/          # tuần 17–24
│   ├── 04-love-relationships/            # tuần 25–34
│   ├── 05-development-mental-health/     # tuần 35–42
│   └── 06-integration/                   # tuần 43–48
└── assessments/
    ├── quizzes/week-NN-quiz.md
    ├── answer-keys/week-NN-answers.md
    ├── milestone-checks/stage-01.md ... stage-06.md
    ├── final-exam.md
    └── final-answer-key.md
~~~

---

## Task 1 — Xây quality gate và hệ thống nguồn

**Files:** Tạo `source-policy.md`, `resources.md`, `quality/content-audit-checklist.md`, `quality/source-ledger.md`.

- [ ] Viết phân cấp nguồn: giáo trình có edition → systematic review/meta-analysis/guideline → tổ chức chuyên môn → nghiên cứu gốc → nguồn phổ thông.
- [ ] Định nghĩa bốn nhãn dùng xuyên khóa: **Mạnh**, **Khá**, **Sơ bộ**, **Tranh luận**, kèm điều kiện sử dụng từng nhãn.
- [ ] Chọn giáo trình xương sống sau khi xác minh tên, edition, năm, URL chính thức và mục lục bao phủ chương trình.
- [ ] Tạo ledger với các cột: tuần, claim ID, claim, citation, loại nguồn, nhãn bằng chứng, URL/DOI, ngày mở kiểm tra, giới hạn.
- [ ] Tạo checklist pass/fail cho claim–source mapping, số liệu, tương quan/nhân quả, mẫu/văn hóa, kỳ thị, tự chẩn đoán, mục tiêu–quiz, Markdown và link.
- [ ] Rà soát: không còn tiêu chí mơ hồ như “dùng nguồn uy tín”; mỗi rule phải nêu điều kiện pass.

**Checkpoint 1:** Duyệt `source-policy.md` trước khi viết bài.

## Task 2 — Tạo template bài học và đánh giá

**Files:** Tạo ba file trong `templates/`.

- [ ] Lesson template có metadata: tuần, chặng, thời lượng hai tầng, prerequisite, mục tiêu và cảnh báo nếu cần.
- [ ] Section theo đúng thứ tự: ý nghĩa → mục tiêu → học nhanh → đọc sâu → nghiên cứu/mô hình → biết gì/chưa kết luận gì → hiểu lầm → hai mini-review → case/journal → glossary → nguồn → quiz.
- [ ] Quiz template có 8–12 câu: khoảng 40% nhớ/hiểu, 40% áp dụng, 20% đánh giá giới hạn bằng chứng.
- [ ] Answer key ghi đáp án, giải thích, lý do phương án nhiễu sai, mục tiêu được đo và link về heading bài học.
- [ ] Kiểm thử template với case “tương quan có đồng nghĩa nhân quả không”; lời giải phải giúp học lại dù trả lời sai.

**Checkpoint 2:** Duyệt ba template.

## Task 3 — Cập nhật điều hướng và tracker

**Files:** Sửa `learning-roadmap.md`; tạo `progress.md`, `glossary.md`.

- [ ] Thay roadmap 24 tuần bằng đúng 48 tuần/sáu chặng của đặc tả, dùng link tương đối và tên file ổn định.
- [ ] Hướng dẫn ba chế độ: mệt chỉ học cốt lõi; bình thường học đầy đủ + quiz; có hứng làm phần mở rộng/case.
- [ ] Nói rõ journal không bắt buộc để hoàn thành tuần.
- [ ] Tracker mỗi tuần có: cốt lõi, đọc sâu, mini-review, quiz, ôn lại; không đặt deadline mặc định.
- [ ] Seed glossary: hypothesis, operational definition, correlation, causation, reliability, validity, effect size, bias, replication, evidence.
- [ ] Link chưa có bài phải hiển thị “sắp biên soạn”, không giả vờ nội dung đã tồn tại.

## Task 4 — Viết vertical slice Tuần 1

**Files:** Tạo bài tuần 1, quiz, answer key; cập nhật ledger và glossary.

- [ ] Chốt bốn mục tiêu: định nghĩa tâm lý học khoa học; phân biệt mô tả/giải thích/dự đoán/can thiệp; so sánh góc nhìn chính; nhận ra claim thiếu bằng chứng.
- [ ] Lập claim inventory và xác minh nguồn trước khi viết mốc năm, số liệu hay kết luận nghiên cứu.
- [ ] Viết “Học nhanh khi mệt” 600–900 từ, kết thúc bằng năm takeaway.
- [ ] Viết phần sâu về mục tiêu khoa học, cấp độ giải thích, văn hóa/bối cảnh và psychology–psychiatry–counseling.
- [ ] Phản biện bốn hiểu lầm: tâm lý học là đọc suy nghĩ; common sense đủ dùng; một lý thuyết giải thích tất cả; psychologist luôn là therapist.
- [ ] Tạo hai mini-review, quiz 10 câu (4 nhớ/hiểu, 4 áp dụng, 2 giới hạn claim) và answer key đầy đủ.
- [ ] Cập nhật ledger/glossary rồi chạy toàn bộ audit; mục không áp dụng phải ghi `N/A — lý do`.

**Checkpoint 3 — bắt buộc:** Người học duyệt giọng văn, độ dài, độ khó, độ thú vị và quiz trước Tuần 2.

## Task 5 — Hiệu chỉnh sau tuần mẫu

- [ ] Chuyển phản hồi chủ quan thành tiêu chí đo được, ví dụ thêm ví dụ sau mỗi khái niệm hoặc giới hạn độ dài section.
- [ ] Sửa template trước để ngăn lỗi hệ thống lặp ở 47 tuần sau.
- [ ] Sửa tuần 1, chạy lại audit và xin duyệt.
- [ ] Ghi ngày và Definition of Done phiên bản 1 vào audit checklist.

## Task 6 — Chặng 1: Nền tảng khoa học, Tuần 2–8

- [ ] Tuần 2: thiết kế nghiên cứu và đạo đức.
- [ ] Tuần 3: tương quan, nhân quả, reliability, validity và replication.
- [ ] Tuần 4: não bộ, gene và môi trường.
- [ ] Tuần 5: cảm giác và tri giác.
- [ ] Tuần 6: chú ý, ý thức và giấc ngủ.
- [ ] Tuần 7: học tập và điều kiện hóa.
- [ ] Tuần 8: trí nhớ, ký ức sai và ôn chặng.
- [ ] Hoàn thành bài + quiz + key + ledger + audit của từng tuần trước tuần tiếp theo.
- [ ] Tạo `stage-01.md`: knowledge check, case đánh giá claim và reflection tự nguyện.
- [ ] Audit chéo thuật ngữ, link, độ khó và prerequisite toàn chặng.

**Checkpoint 4:** Làm milestone và hiệu chỉnh trước Chặng 2.

## Task 7 — Chặng 2: Hiểu bản thân, Tuần 9–16

- [ ] Viết lần lượt: cảm xúc; động lực; Big Five; self-concept/self-esteem/identity; giá trị; thói quen; bias/điểm mù; hồ sơ tự hiểu.
- [ ] Big Five được dạy theo continuum; test online và MBTI không dùng để kết luận bản chất.
- [ ] Không biến identity, self-esteem, giá trị, thói quen hoặc bias thành nhãn cố định.
- [ ] Hồ sơ tuần 16 gồm quan sát, bằng chứng, giả thuyết thay thế và mức tự tin; không có mục chẩn đoán.
- [ ] Mỗi tuần hoàn tất quiz/key/ledger/audit; tạo `stage-02.md`.

**Checkpoint 5:** Review tính hữu ích và an toàn của hồ sơ tự hiểu.

## Task 8 — Chặng 3: Hiểu người khác, Tuần 17–24

- [ ] Viết lần lượt: social cognition; attribution; empathy/perspective-taking; giao tiếp; thái độ/thuyết phục; nhóm/quyền lực; định kiến/giúp đỡ/xung đột; case tổng hợp.
- [ ] Phân tích hành vi luôn nêu ít nhất ba giả thuyết và dữ kiện còn thiếu.
- [ ] Không có tín hiệu phi lời đơn lẻ nào được mô tả là bằng chứng chắc chắn của nói dối, yêu thích hoặc tính cách.
- [ ] Phân biệt ảnh hưởng có đạo đức với thao túng/cưỡng ép.
- [ ] Case tuần 24 có bối cảnh văn hóa và chấp nhận bất định hợp lý.
- [ ] Hoàn tất quiz/key/ledger/audit từng tuần và `stage-03.md`.

**Checkpoint 6:** Thử một case mới chưa có trong bài.

## Task 9 — Chặng 4: Tình cảm và quan hệ, Tuần 25–34

- [ ] Viết lần lượt: attachment; hấp dẫn; intimacy/commitment; phản biện love languages; giao tiếp nhu cầu; boundary/consent; conflict/repair; ghen/lòng tin; healthy patterns/warning signs; chia tay/phục hồi.
- [ ] Attachment không được dùng như nhãn cố định hoặc công cụ đoán tương lai.
- [ ] Love languages là case đánh giá bằng chứng, không phải công thức tương hợp.
- [ ] Consent được định nghĩa là tự nguyện, cụ thể, có thông tin, có thể rút lại; không suy từ im lặng.
- [ ] Warning signs tập trung hành vi, tần suất, mức độ, bối cảnh và an toàn; không chẩn đoán đối tác.
- [ ] Không hứa thời gian phục hồi chuẩn sau chia tay; chỉ dẫn tìm hỗ trợ khi mất an toàn/suy sụp nghiêm trọng.
- [ ] Hoàn tất quiz/key/ledger/audit từng tuần và `stage-04.md`.

**Checkpoint 7:** Review riêng consent, red flags, an toàn và ngôn ngữ gán nhãn.

## Task 10 — Chặng 5: Phát triển và sức khỏe tâm thần, Tuần 35–42

- [ ] Viết lần lượt: phát triển suốt đời; stress/coping; wellbeing; khái niệm rối loạn; anxiety/depression/OCD; trauma/bipolar/psychosis; personality disorders; trị liệu dựa trên bằng chứng.
- [ ] Phân biệt trải nghiệm bình thường, distress, impairment và chẩn đoán chuyên môn.
- [ ] Dùng person-first language, continuum và bối cảnh văn hóa.
- [ ] Không biến self-screening thành chẩn đoán; mọi thang đo nêu mục đích và giới hạn.
- [ ] So sánh trị liệu theo vấn đề, bằng chứng, giới hạn và vai trò therapist–psychologist–psychiatrist.
- [ ] Hướng dẫn khẩn cấp không giả định quốc gia: khi nguy cơ tức thời, liên hệ dịch vụ khẩn cấp địa phương, người tin cậy hoặc chuyên gia.
- [ ] Hoàn tất quiz/key/ledger/audit từng tuần và `stage-05.md`.

**Checkpoint 8:** Audit chuyên biệt nguồn lâm sàng và chống kỳ thị.

## Task 11 — Chặng 6 và đánh giá cuối, Tuần 43–48

- [ ] Viết lần lượt: quyết định; học tập/công việc; giao tiếp khó; quan hệ lành mạnh; kế hoạch phát triển; final/portfolio.
- [ ] Giao tiếp khó cho phép chuẩn bị, dừng và ưu tiên an toàn; không ép đối thoại trong quan hệ nguy hiểm.
- [ ] Kế hoạch cá nhân có hành vi, baseline, cue, cách đo, ngày review và phương án điều chỉnh.
- [ ] Final exam phủ sáu chặng, ưu tiên transfer và đánh giá bằng chứng hơn thuộc tên tác giả.
- [ ] Case mở trong answer key ghi các đáp án hợp lý, bất định và dữ kiện thiếu.
- [ ] Portfolio cho phép ẩn/xóa reflection riêng tư và dùng case giả định.
- [ ] Hoàn tất quiz/key/ledger/audit, `stage-06.md`, final exam/key và `portfolio-guide.md`.

**Checkpoint 9:** Hoàn thành final và chọn hướng học tiếp.

## Task 12 — Audit phát hành

- [ ] Inventory phải có: 48 bài, 48 quiz, 48 answer key, sáu milestone, final exam/key, portfolio và file quản trị.
- [ ] Kiểm tra toàn bộ link nội bộ; không còn link tới file không tồn tại.
- [ ] Mở lại URL/DOI quan trọng và lưu ngày kiểm tra trong ledger.
- [ ] Lấy mẫu ít nhất năm claim mỗi chặng để đối chiếu nguồn; một claim fail thì audit toàn tuần đó.
- [ ] Mỗi mục tiêu được đo ít nhất một lần; không câu hỏi nào đòi kiến thức chưa dạy.
- [ ] Review thủ công “toxic”, “narcissist”, “bệnh”, “bình thường”, “đọc vị”, “luôn luôn”, “chắc chắn”.
- [ ] Ghi `quality/final-audit-report.md`: inventory, link check, claim sample, lỗi đã sửa, giới hạn còn lại và ngày audit.
- [ ] Chỉ đánh dấu roadmap “Hoàn thành” khi tất cả quality gate pass.

## Definition of Done

- Đủ 48 tuần và sáu milestone theo thiết kế.
- Bài học đọc độc lập, không buộc xem video/podcast.
- Nội dung giúp hiểu bản thân, người khác và quan hệ nhưng không đọc vị/chẩn đoán.
- Mọi tuần qua content audit; mọi chặng qua cross-week audit.
- Citation, số liệu và nguồn truy xuất được; giới hạn bằng chứng được nói trung thực.
- Roadmap, tracker, glossary, quiz, answer key và links nhất quán.
- Tuần mẫu đã được người học duyệt trước khi nhân rộng.
