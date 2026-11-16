# Ghi chú đối chiếu nguồn — 2026-10-03

Nguồn được fetch với URL cuối, ngày,HTTP status/body hash trong source-check.json. Claim IDs là nhóm chủ đề gắn bài/LO; chúng không chứng minh người học đã chạy project. Ledgers từng tuần chứa citations của đúng tuần,global ledger là index.

## Phạm vi đã đọc/đối chiếu

Đối chiếu các đoạn kỹ thuật liên quan và API contracts trong primary docs; không tuyên bố đọc toàn bộ mọi trang từ đầu đến cuối. Không coi HTTP200 là semantic approval. Các ví dụ và policy project do tác giả tự thiết kế.

| Chặng | Các cơ chế đã đối chiếu | Giới hạn |
|---|---|---|
| HTML/JS | label association;box model/responsive;lexical environment;array copy;safe integer;delegation/modules/storage;Promise/fetch/event loop |320/768/1280 vàkeyboard là gate học viên,không phải test browser đã chạy |
| C# | value/reference copy;OOP;collection/constraints/delegates;LINQ deferred;nullable compile-time;exception boundaries;Task/cooperative cancellation/DI lifetime |using/disposal mẫu file và async tested;không claim toàn domain app hoàn tất |
| SQL | PK/FK/UNIQUE/CHECK;normalization;JOIN/HAVING;CTE không materialize;index/actual plan;N+1;transaction/concurrency |schema/workload/index là thiết kế project,không có benchmark production bịa |
| API/EF | composite keys;migrations/validation;middleware/ProblemDetails;JWT standards/dev user-jwts;resource auth;transaction/retry |OIDC provider đăng ký thật là bài học viên;không tự tạo password/token issuer production |
| React/TS | narrowing;keys/immutable state;effect cleanup/StrictMode;Router routes;controlled inputs;reducers;user-oriented tests |cast không runtime validation;amount/budget arithmetic là policy author;client guard không thay server auth |
| Production | staged changes/config providers;GNU chmod trên Ubuntu official;Docker stages/DNS/readiness;CI real relational provider;health/migration deployment |cloud price/version phải đọc lại khi làm;Docker/cloud/backup learner chưa được thực hiện bởi tác giả |
| Design/interview |HTTP semantics;limiter algorithms;cache-aside stale limits;Redis SET TTL;queue/outbox;trusted proxy;dictionary/binary search;STAR(R) |RPS,TTL,RPO/RTO và16 DSA problems là giả định/định mức luyện của khóa,không guarantee/job-market stats |

## Sửa sau đối chiếu

- Thay URL collection-class bị404 và GNU permission URL cũ.
- GNU site có timeout không ổn định; dùng Ubuntu official manpage của GNU chmod,nội dung symbolic/octal permission bits đã đối chiếu.
- W16 LO2 dùng EF transactions cho atomic batch;testing-strategy vẫn ởW23.
- W28 LO2 dùng Microsoft Careers interview tips cho reasoning/STAR;binary search vẫn là nguồn hỗ trợ DSA.
- Cache DEL sau commit vẫn có late-reader race;TTL không chứng minh zero stale.
- Outbox nguồn Cosmos chỉ minh họa nguyên lý;project SQL dùng transaction/constraints,không copy Cosmos APIs.
- Action references trong workflow là mẫu major tags;release cần pin revision/digest theo inventory.

## Trạng thái import và runtime

Manifest source_status giữ in_review và course Draft trong importer;không tự publish/approve qua việc kiểm URL. Runtime reports chỉ những ví dụ cụ thể đã chạy. Project acceptance checklist mô tả hành vi học viên phải đạt và không được đánh dấu đạt thay học viên.
