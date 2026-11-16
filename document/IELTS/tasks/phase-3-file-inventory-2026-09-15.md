# Inventory file cho chuẩn bị IELTS Phase 3

> Ngày lập: 2026-09-15  
> Mục đích: manifest bàn giao cho agent lập Phase 3; không tạo bài học trong file này.  
> Quy tắc ownership: chỉ inventory; không ghi đè các file root, handoff, roadmap hoặc mã nguồn ngoài IELTS.

## 1. Bảng file hiện có và vai trò

| Khu vực/file | Trạng thái thực tế | Vai trò Phase 3 | Hành động | Ưu tiên | Agent phù hợp |
|---|---|---|---|---|---|
| `IELTS/README.md` | Có; mô tả Phase 3–5 đang chờ | UI/entry point và status tracker | Chỉ cập nhật sau khi batch được tạo/review; không sửa trong task chuẩn bị | P2 | Tracker/release agent |
| `IELTS/roadmap.md` | Có; có Phase 0–2 và log EF SET | Theo dõi ngày, test, reader, lỗi | Tái sử dụng format; không ghi đè; thêm Phase 3 chỉ sau approval | P2 | Tracker/release agent |
| `IELTS/implementation_plan_new.md` | **Thiếu** dù được README/HANDOFF gọi là nguồn sự thật | Nguồn phạm vi chính | Blocker; owner phải phục hồi/xác nhận trước biên soạn | P0 | Owner/planning agent |
| `IELTS/implementation_plan.md` | Có nhưng tự ghi lỗi thời | Chỉ đối chiếu lịch sử, không phải source of truth | Không sửa và không dùng làm căn cứ duy nhất | P2 | Reviewer |
| `IELTS/HANDOFF.md` | Có; nêu pattern 14 batch, Phase 3 roadmap | Contract về batch, naming, quality gates | Reuse có điều kiện; không cập nhật trạng thái trong task này | P0 | Planning/reviewer agent |
| `IELTS/HANDOFF-REVIEW.md` | Có; 7 quality checks | Audit contract | Reuse cho audit Phase 3 sau khi tạo | P1 | QA/reviewer agent |
| `IELTS/Resources/anki-guide.md` | Có nhưng claim CSV không nhất quán | Schema thẻ, spaced repetition, overload handling | Tái sử dụng; cần owner sửa claim trước khi dùng làm source tuyệt đối | P1 | Learning-design/reviewer agent |
| `IELTS/Resources/anki-master-vocabulary.csv` | Có; không nên coi là master đầy đủ P3 | Backup/tra cứu tùy chọn | Không cập nhật trừ khi có yêu cầu riêng; P3 dùng tag/deck workflow | P2 | Anki/content agent |
| `IELTS/Resources/graded-readers.md` | Có; Level 4–5 gắn với Phase 3 | Chọn level, cách đọc, fallback miễn phí | Tái sử dụng; không cần sửa để tạo batch | P1 | Learning-design agent |
| `IELTS/Resources/ai-conversation-guide.md` | Có; prompt nói và sửa lỗi | Speaking practice và feedback | Tái sử dụng; P3 cần prompt B1/B2 riêng trong batch | P1 | Speaking-content agent |
| `IELTS/Resources/study-tips.md` | Có; maintenance và feedback habits | Chế độ thiếu thời gian, motivation, sửa output | Tái sử dụng; không phụ thuộc các claim phần trăm như bằng chứng khoa học | P2 | Learning-design/reviewer agent |
| `IELTS/Resources/youtube-channels.md` | Có; nguồn nghe miễn phí và fallback | Listening source discovery | Tái sử dụng; kiểm tra link/search query từng file mới | P1 | Listening-content agent |
| `IELTS/Resources/psychology-vocabulary.md` | Có; index concept Phase 0–1 | Tránh lặp và nối concept | Tái sử dụng như index; mở rộng chỉ khi owner yêu cầu | P2 | Psychology-content reviewer |
| `IELTS/Phase-0/` | Có | Prerequisite lịch sử, không sửa | Không sửa | P2 | Không cần |
| `IELTS/Phase-1/` | Có, gồm `error-log.md` | Kỹ năng nền và error-log candidate | Chỉ đọc/tham chiếu; không sửa trong Phase 3 prep | P0 | Không sửa |
| `IELTS/Phase-2/` | Có, 4 tháng và các thư mục kỹ năng | Mẫu format gần nhất, prerequisite | Đọc/đối chiếu; không lặp nguyên content; không sửa trong prep | P0 | Reviewer/content planner |

## 2. File Phase 3 cần tạo sau khi đóng blocker

Mỗi tháng P3-M1 đến P3-M4 dự kiến giữ format HANDOFF, tổng danh nghĩa 40 file/tháng:

| Khu vực | Mỗi tháng | Quy ước/ghi chú |
|---|---:|---|
| `daily-guide-monthN.md` | 1 | Tạo trước; source of truth cho lịch và filenames |
| `grammar/` | 5 | `NN-topic.md`; ba phần A/B/C và đáp án |
| `vocabulary/` | 8 | 3 file part1/2/3, 4 cycle files, 1 review; khoảng 140–160 core words/tháng nhưng phải đếm thật |
| `reading/` | 8 | 3 bài đầu + 5 `cycleN-reading.md`; bài 500–800 từ, câu hỏi và đáp án |
| `listening/` | 8 | 3 day files + 5 cycle files; nguồn miễn phí, shadowing, dictation, fallback |
| `speaking/` | 1 | `speaking-prompts-monthN.md`, 24 ngày, prompt feedback và test |
| `writing/` | 2 | kickoff day file + `writing-prompts-monthN.md`, essay progression và checklist |
| `tests/` | 7 | 6 cycle/total tests + `test-01-retry.md`; numbering liên tục trong Phase 3 |

### Backlog file cụ thể theo tháng

| Batch | Grammar | Theme | Đầu ra cần kiểm tra | Phụ thuộc |
|---|---|---|---|---|
| P3-M1 | Relative Clauses | Personality Deep Dive, Big Five, critique MBTI | 500–800-word reading; 200–250-word essay; 2-minute talk | P0 decisions, Phase 2 baseline, psychology concept review |
| P3-M2 | Reported Speech + reporting verbs | Communication & Language, NVC, framing | Paraphrase/attribution; speaking explanation; listening nuance | P3-M1 filenames/schema; approved thresholds |
| P3-M3 | Third/Mixed Conditionals + Wish/If only | Regret & Counterfactual Thinking | Counterfactual essay; speaking cause/effect; scheduled retest only if due | P3-M2; EF SET ownership/schedule |
| P3-M4 | Gerunds & Infinitives + review | The Good Life, flow, meaning | Integrated finale across four skills; transition evidence | P3-M3; finale rubric and phase gate |

## 3. File cần cập nhật sau này, không thuộc task này

| File | Khi nào cập nhật | Điều kiện |
|---|---|---|
| `README.md` | Sau Phase 3 batch/review | Số file thực tế đã đếm; status không nói “đã tạo” trước khi có đủ batch |
| `roadmap.md` | Sau từng monthly batch hoặc theo ownership được xác nhận | Có bảng tháng, test score fields, reader log và retest note; không xóa dữ liệu cũ |
| `implementation_plan_new.md` | Khi file được phục hồi và owner cho phép | Ghi batch đã tạo, không tạo kế hoạch mới trái nguồn sự thật |
| `HANDOFF.md` | Sau mỗi batch theo quy ước hiện tại | Cập nhật số file thật và batch kế tiếp; không dùng task prep để giả trạng thái |
| `HANDOFF-REVIEW.md` | Sau audit Phase 3 | Chỉ ghi kết quả review thật, không ghi pass khi blocker chưa đóng |
| `Resources/anki-master-vocabulary.csv` | Chỉ khi người dùng yêu cầu | Export/format phải được kiểm tra; không thêm tay gây sai backup |

## 4. File có thể tái sử dụng

| File/nhóm | Cách tái sử dụng | Giới hạn |
|---|---|---|
| Phase 2 daily guides | Bắt chước điều phối 24 ngày và slot | Không copy mục tiêu A2/B1 hoặc tên file cũ |
| Phase 2 grammar | Giữ layout A/B/C, exercise, answer key | Nội dung P3 phải là grammar mới và có retrieval grammar cũ |
| Phase 2 vocabulary | Giữ bảng IPA/part of speech/meaning/example | Tránh lặp từ; tăng collocation nhưng kiểm soát tải |
| Phase 2 reading | Giữ psychology essays, questions, `details` answer | Tăng 500–800 từ; fact-check concept và ghi giới hạn |
| Phase 2 listening | Giữ 2 lượt sub, shadowing, 3 câu dictation, % | Kiểm tra nguồn và luôn có fallback |
| Phase 2 speaking/writing | Giữ daily table, recording, AI correction, checklist | Nâng output lên essay 200–250 và talk 2 phút |
| Resources guides | Giữ Anki, reader, AI feedback và maintenance behavior | Không biến mẹo/claim cũ thành bằng chứng band |

## 5. File không nên sửa trong giai đoạn chuẩn bị

| File/area | Lý do |
|---|---|
| `IELTS/README.md` | User yêu cầu không ghi đè tracker cũ trong task chuẩn bị; status hiện tại cần giữ nguyên |
| `IELTS/roadmap.md` | Dữ liệu tiến độ/ownership cá nhân; không thêm Phase 3 chưa biên soạn |
| `IELTS/HANDOFF.md` | Là handoff cũ cần bảo toàn; không cập nhật giả trạng thái |
| `IELTS/HANDOFF-REVIEW.md` | Review log cũ; không ghi Phase 3 pass khi chưa có file |
| `IELTS/implementation_plan.md` | Được đánh dấu lỗi thời; không sửa để “vá” việc thiếu plan mới |
| `IELTS/Phase-0/`, `Phase-1/`, `Phase-2/` | Baseline/reference; không refactor hoặc lặp nội dung trong prep |
| `IELTS/Resources/*` | Dùng tham chiếu; các bất nhất phải đưa vào blocker, không âm thầm sửa |
| `Psychology/`, `Investing/`, `LifeSkills/`, `PersonalFinance/` | Ngoài phạm vi tuyệt đối |
| Mã nguồn Visualization | Ngoài phạm vi tuyệt đối |
| `IELTS/Phase-4/`, `IELTS/Phase-5/` | Chưa được tạo trong task này |

## 6. Quan hệ phụ thuộc

```text
implementation_plan_new.md (phục hồi/xác nhận)
        |
        +--> owner decisions: track IELTS vs general English, thresholds, error-log, baseline
        |          |
        |          +--> Phase 2 inventory/quality reconciliation
        |                         |
        |                         +--> P3-M1 guide/schema
        |                                      |
        |                                      +--> P3-M1 content + tests + audit
        |                                                   |
        |                                                   +--> P3-M2 --> P3-M3 --> P3-M4 finale
        |
        +--> resources/format references (Anki, readers, AI, listening fallbacks)
```

Không được bỏ qua dependency source-of-truth và baseline bằng cách bắt đầu viết hàng loạt. Mỗi monthly batch phải pass audit trước khi làm batch sau nếu owner muốn phát hiện lỗi schema sớm.

## 7. Ưu tiên và phân công agent

| Ưu tiên | Công việc | Agent phù hợp | Đầu ra |
|---|---|---|---|
| P0 | Phục hồi/xác nhận plan mới, quyết định scope và ownership | Planning/owner agent | Decision record, không code |
| P0 | Baseline Phase 3 và reconcile Phase 2 count/threshold | Assessment/reviewer agent | Baseline + discrepancy report |
| P1 | Thiết kế schema/naming P3-M1 và kiểm tra dependency | Content planner / API-interface style reviewer | Approved monthly manifest |
| P1 | Biên soạn grammar + vocabulary | Grammar/content agent | 13 file/tháng theo schema |
| P1 | Biên soạn reading + concept fact-check | Reading + psychology reviewer | 8 file/tháng, concept-safe |
| P1 | Biên soạn listening + fallback | Listening agent | 8 file/tháng, source checked |
| P1 | Biên soạn speaking + writing | Output-skills agent | 3 file/tháng, rubric/feedback |
| P1 | Biên soạn tests/retry/answers | Assessment agent | 7 file/tháng, scoreable |
| P1 | Chạy quality gates và human-eye review | QA/reviewer agent | Gate report, open issues |
| P2 | Cập nhật tracker sau approval | Tracker/release agent | README/roadmap/handoff consistent |

## 8. Checklist bàn giao cho agent biên soạn

- [ ] Đọc `phase-3-spec-2026-09-15.md` và xác nhận mọi P0 đã đóng.
- [ ] Không tạo Phase 4–5, không sửa các project ngoài IELTS và không sửa Visualization.
- [ ] Dùng Phase 2 làm format gần nhất nhưng kiểm tra số file thực tế, không tin claim README/HANDOFF mù quáng.
- [ ] Viết daily guide trước các file nội dung.
- [ ] Giữ tên file tương đối nhất quán và kiểm tra tất cả backtick references.
- [ ] Đếm vocabulary thật trước khi ghi claim.
- [ ] Tạo đáp án/rubric cùng test/grammar, không để placeholder.
- [ ] Có Anki retrieval, spaced repetition, input/output balance và maintenance path mỗi tháng.
- [ ] Không tuyên bố band IELTS hoặc “Phase 3 ready” nếu thiếu evidence/baseline/finale.
- [ ] Sau batch: chạy CJK, count, references, answers, source/fallback và human-eye review.

## 9. Trạng thái inventory

**Phase 3 chưa sẵn sàng để biên soạn hàng loạt.** Lý do trực tiếp là thiếu `implementation_plan_new.md` và thiếu dữ liệu xác nhận baseline/ownership/threshold. Inventory này chỉ mô tả công việc và phụ thuộc; không phải lệnh tạo Phase 3.
