# Kế hoạch biên soạn — FullStack C# + React Refresh
Ngày 2026-10-03. Chỉ ghi trong thư mục khóa học mới này; khóa tham chiếu chỉ đọc.

## Thiết kế đã chốt
7 chặng × 4 tuần = 28 tuần; mỗi tuần có 3 bài nhỏ = 84 bài. Tiếng Việt thống nhất; thuật ngữ và luyện tiếng Anh giữ tiếng Anh. Nhịp 12–18 giờ chuyên môn/tuần + 30–60 phút tiếng Anh mỗi ngày. Tuần là đơn vị năng lực, không hạn chót. Chẩn đoán trước học bằng quiz và bài code: >=5/6 câu đúng và vượt bài thực hành thì được giảm thời gian đọc, nhưng không bỏ gate hay tính năng.

Một dự án Sổ chi tiêu chạy xuyên suốt: HTML/JS -> domain console C# -> SQL -> ASP.NET Core/EF -> React/TS -> Docker/CI/deploy -> Redis và thiết kế có đo lường. SQL Server là đường bắt buộc; PostgreSQL chỉ đối chiếu tùy chọn. .NET 10 + EF Core 10; không trộn major package. Tiền VND nguyên: bigint/long, validation Number.isSafeInteger ở JS; API giới hạn số tiền trong miền an toàn JS. Modular monolith, không ép microservices.
Git, English, debug, test và ôn truy hồi xuyên suốt. C++, NoSQL, Vue chỉ tùy chọn.

## Đồ thị phụ thuộc
HTML/CSS -> DOM -> JS scope/array/module -> Promise/fetch -> TS -> React.
C# value/OOP -> collection/generic -> LINQ/nullable/exception -> Task/cancellation/disposal/DI.
SQL schema/constraint -> JOIN/aggregate/CTE -> plan/index/paging -> transaction.
C# + SQL + HTTP/JS fetch -> ASP.NET Core/EF -> authentication/authorization/test -> React tích hợp.
Fullstack + Git -> Docker/Linux/env/CI -> deployment/monitoring.
SQL plan + HTTP + deployment -> Redis/queue/rate limit/scaling/consistency -> interview.
Xem sơ đồ và thứ tự từng tuần tại learning-roadmap.md.

## Chặng và sản phẩm
1. W01–04 HTML/CSS/JS: sổ chi tiêu thuần JS với dữ liệu mẫu.
2. W05–08 C#: domain console, báo cáo và import có hủy.
3. W09–12 SQL: persistence, báo cáo, paging và transaction.
4. W13–16 API/EF: CRUD có validation, JWT, ownership và regression tests.
5. W17–20 React/TS: UI hoàn chỉnh, biểu đồ/budget, states và test.
6. W21–24 production: Git/Linux/env, Docker, CI, deploy/backup/rollback.
7. W25–28 design/interview: HTTP/rate limit, Redis, queues/scaling/consistency, DSA/portfolio.

## Trình tự và scope
1. Đọc SubjectTemplate, DEV, DotNet-React-Roadmap, PersonalFinance (đã đọc README/manifest và lesson/answer/card templates).
2. Tạo plan/todo/README/scaffold trước full content.
3. Tạo project-spec, setup, diagnostics, English, refresh appendix, source policy và validator.
4. Biên soạn 4 tuần/chặng; mỗi tuần core, deep dive, ví dụ giải 4 bước, bài tập có lời giải, project task, gate, 6 MCQ/đáp án (giải cả 4 lựa chọn), 6 thẻ.
5. Sau mỗi chặng chạy validator, cập nhật todo và progress; sửa lỗi trước chặng tiếp.
6. Tổng nghiệm thu: 28 tuần, nguồn chính thức, source verification, hợp đồng manifest v2 và import dry-run nếu công cụ hiện có hỗ trợ.
7. Không đánh dấu ready nếu thiếu kiểm chứng critical.

## Tasks theo thứ tự
T01 scaffold -> T02 shared spec/validator -> T03 chặng 1 -> T04 chặng 2 -> T05 chặng 3 -> T06 chặng 4 -> T07 chặng 5 -> T08 chặng 6 -> T09 chặng 7 -> T10 tổng nghiệm thu.
Mỗi task biên soạn chia thành 4 đơn vị tuần; mỗi tuần sở hữu 4 tệp học liệu. Shared manifest và quality chỉ tác giả chính ghi.
Sub-agent request: gpt-5.6-luna. LocalWorker trả policy enabled=false, maxConcurrent=0, nên không thể chạy child qua connector trong cấu hình hiện tại. Thực thi tuần tự, ghi rõ giới hạn, không tự nhận đã dùng model.

## Acceptance và verification
- 84 bài được giảng; 28 gate có dữ liệu/expected behavior, không coi tồn tại tệp là năng lực học viên.
- 168 câu/168 lời giải/168 thẻ; LO và claim map đúng; quiz tự chứa trong bài.
- Internal links tồn tại, không placeholder/accidental duplicate; IDs duy nhất.
- Nguồn primary và claim-to-source ledger ghi mục/ngày/phạm vi thật; HTTP OK không thay đọc nguồn.
- Manifest stable ID, path, hash, byte, count; contract/import được kiểm tra độc lập.
- Project task kèm mẫu dữ liệu và tiêu chí behavior; learner thực hiện sau học, không ghi đã test project khi mới biên soạn.
- Checklist chất lượng học liệu và rubric học viên riêng.

## Rủi ro
Nguồn/SDK thay đổi: kiểm tra tài liệu chính thức và lockfile. Parser web khác Markdown: đọc contract và thử dry-run, không sửa app. Target xung đột: lần đầu dùng overwrite=false; chỉ sửa file do phiên này tạo. Reference có nội dung mỏng: không copy syllabus làm lesson. Source bị chặn: ghi chưa xác minh và hạ readiness, không bịa evidence.
