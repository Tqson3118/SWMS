# Báo cáo rà soát roadmap

Ngày: 22/09/2026

## Kết quả

Mười bốn chặng đã được viết lại theo một dự án Sổ chi tiêu xuyên suốt, dành cho người bắt đầu lại từ nền tảng. Mỗi chặng có cách chuẩn bị, ví dụ chạy được, bài tập không đưa lời giải ngay, edge case, checkpoint, cách dùng AI và nguồn chính thức.

Các chỉnh sửa kỹ thuật chính:

- sửa các khẳng định tuyệt đối về dấu chấm phẩy, naming, decimal, ReadLine/TryParse, Big O, QuickSort, Dictionary/HashSet và debugger;
- sửa SQL Server syntax, NULL, JOIN/GROUP BY, phân trang OFFSET/FETCH, FK, transaction và SQL injection;
- sửa DI lifetime, EF Core tracking/projection/N+1, async/await, ProblemDetails, JWT ký so với mã hóa, Identity token và logging nhạy cảm;
- sửa fetch response body chỉ đọc một lần, status 4xx/5xx, CORS, AbortController, TypeScript bị erase khi build;
- sửa React state mutation, derived state, useEffect dependency/cleanup/StrictMode, Context re-render, Vite environment và cấu hình deploy;
- thêm setup LocalDB/SSMS, database API riêng ExpenseApiDb, EF migration/tool manifest, xUnit setup và README.

gen_md.py có guard để không vô tình ghi đè nội dung đã rà soát; chỉ --force-legacy mới chạy bản sinh cũ.

## Kiểm tra đã thực hiện

Các ví dụ C# cơ bản, collection/debug, class/interface/file/JSON và xUnit đã được chạy trong workspace kiểm tra riêng. Trong lần tiếp tục này, 15 snippet C# đại diện đã biên dịch thành công trên .NET 10; query SQL có LEFT JOIN/COALESCE và rollback transaction đã chạy trên database LocalDB tạm, rồi database tạm được xóa. Không sửa database người dùng.

Markdown được kiểm tra có đủ 14 file chặng, README và code fence cân bằng. Mười lăm file Word tương ứng (README và Chặng 1–14) đã được sinh lại từ Markdown hiện tại. Kiểm tra cấu trúc ZIP/python-docx cho cả 15 file đều đạt: mở được, đủ phần bắt buộc, tiêu đề khớp và không có marker lỗi. Đã thử renderer chuẩn nhưng máy LocalWorker thiếu LibreOffice; đường dự phòng Word COM bị timeout, nên chưa có bằng chứng render PNG để kết luận QA hình ảnh.

## Cập nhật 22/09/2026 – nối liền mạch Chặng 5–7

Đã xử lý blocker lớn nhất của bản review: dữ liệu từ database đến API và React giờ dùng một hợp đồng thống nhất.

- 5.md định nghĩa CreateExpenseRequest và ExpenseResponse; controller hoàn chỉnh có GET danh sách, GET theo id, POST, projection và validation.
- 5.md có đủ AddControllers, MapControllers, CORS theo đúng frontend origin và hướng dẫn chạy API ở cổng 5000.
- Migration được tách đúng vòng đời: InitialCreate chỉ tạo ở Buổi 3; thay đổi sau dùng AddExpenseCreatedAtUtc ở Buổi 5.
- 6.md và 7.md dùng cùng các field categoryId, categoryName, amount, spentOn, note; frontend có runtime parser, API_BASE_URL, POST vertical slice và router setup.
- Mẫu C# controller/DTO/model đã build thành công trên .NET 10; contract TypeScript đã qua tsc --strict.

Tám file Word đã được sinh lại sau các chỉnh sửa này. Việc kiểm tra mới xác nhận cấu trúc và nội dung đồng bộ; chưa chạy toàn bộ API thật nối LocalDB với trình duyệt React.

## Cập nhật 22/09/2026 – bổ sung Chặng 8–14

Đã bổ sung phần tiếp nối sau luồng full-stack cơ bản:

- Chặng 8: C# chuyên sâu, LINQ deferred execution, resource, async và cancellation; có bài import file có thể hủy.
- Chặng 9: composition, SOLID, dependency direction, Strategy/Factory/Adapter; có bài mở rộng importer/report mà không sửa dây chuyền.
- Chặng 10: index, execution plan, transaction/isolation, pagination, migration có dữ liệu và optimistic concurrency; có bài tái hiện 409.
- Chặng 11: authentication/authorization, ownership, resource-based policy, CORS/CSRF/XSS, rate limit và idempotency; có test User A không truy cập dữ liệu User B.
- Chặng 12: state structure, reducer, custom hook, server state, URL filter, race condition, accessibility và đo render.
- Chặng 13: unit/integration/E2E, test double, database isolation, authorization/concurrency regression và CI.
- Chặng 14: Docker, cấu hình môi trường, CI/CD, health check, structured logging, backup/restore và rollback.

Mỗi chặng mới có checkpoint, dữ liệu biên, tự chấm và Interview checkpoint. Nội dung là continuation/spec học tập có ví dụ và acceptance criteria; chưa phải bằng chứng rằng toàn bộ ứng dụng 8–14 đã được implement, deploy hoặc chạy E2E.

## Giới hạn

Phiên bản SDK/package và lệnh hosting có thể thay đổi theo máy. Roadmap cố ý dùng .NET/EF Core cùng major và dẫn về tài liệu chính thức để người học đối chiếu phiên bản cài thật. Authentication/deployment chỉ là mục tiêu học có kiểm soát; không coi checklist này là audit bảo mật production.

## Nguồn

- [Microsoft C#](https://learn.microsoft.com/dotnet/csharp/)
- [Microsoft ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
- [Microsoft EF Core](https://learn.microsoft.com/ef/core/)
- [Microsoft SQL Server](https://learn.microsoft.com/sql/)
- [MDN Web Docs](https://developer.mozilla.org/docs/Web)
- [React](https://react.dev/)
- [TypeScript](https://www.typescriptlang.org/docs/)

