# Roadmap học .NET và React từ nền tảng

Tài liệu này dành cho người đã từng dùng AI để tạo ứng dụng nhưng muốn tự đọc, viết, debug và giải thích code. Mục tiêu cuối là một Sổ chi tiêu cá nhân có frontend React + TypeScript, ASP.NET Core API, SQL Server, kiểm thử có hệ thống và quy trình triển khai/vận hành cơ bản.

## Cách đi qua roadmap

Học theo thứ tự 1 đến 14. Chặng 1–7 đưa cụ đến một ứng dụng full-stack chạy được; Chặng 8–14 nâng ứng dụng đó thành code dễ mở rộng, có kiểm thử, bảo mật và vận hành được. Mỗi chặng có thời lượng tham khảo; chỉ đi tiếp khi đạt checkpoint, không chạy theo số tuần. Với 60–90 phút một buổi:

1. Đọc một khái niệm nhỏ và tự nói lại.
2. Gõ ví dụ, chạy với input bình thường và edge case.
3. Làm bài tập từ file trống, chỉ tra cú pháp khi cần.
4. Ghi input, output mong đợi và lỗi đã sửa.
5. Viết lại bài cũ sau một ngày.

Mọi code từ AI đều phải được bạn chạy, đọc từng dòng và viết lại một biến thể. Khi hỏi AI, hãy gửi code đã thử, lỗi nguyên văn và expected output; yêu cầu gợi ý trước khi nhận lời giải.

## Các chặng

| File | Trọng tâm | Sản phẩm/checkpoint |
|---|---|---|
| [1.md](1.md) | C# và console | Tự viết sổ chi tiêu từ file trống, validation và debugger |
| [2.md](2.md) | Phân rã, collection, Big O | Tổng theo danh mục, HashSet/Dictionary, bài tìm kiếm |
| [3.md](3.md) | Class, file, JSON, Git, test | App console có domain model, lưu JSON, unit test |
| [4.md](4.md) | SQL Server | Categories/Expenses, JOIN, GROUP BY, transaction |
| [5.md](5.md) | HTTP, ASP.NET Core, EF Core | API CRUD, validation, ProblemDetails, logging, auth |
| [6.md](6.md) | HTML, CSS, JavaScript, TypeScript | Trang frontend thuần gọi được API |
| [7.md](7.md) | React, state, effect, deploy | Ứng dụng fullstack hoàn chỉnh |
| [8.md](8.md) | C# chuyên sâu, async, cancellation | Import file có thể hủy, hiểu execution/state |
| [9.md](9.md) | Thiết kế code, SOLID, patterns | Thêm importer/report mới mà ít sửa code cũ |
| [10.md](10.md) | SQL/EF Core sâu, index, concurrency | Đo query, migration có dữ liệu, xử lý 409 |
| [11.md](11.md) | Multi-user API, authorization, security | User chỉ truy cập dữ liệu của mình, chống retry trùng |
| [12.md](12.md) | React/TypeScript state, cache, race, a11y | Filter qua URL, search không stale, UI có lỗi rõ |
| [13.md](13.md) | Unit, integration, E2E, CI | Có bằng chứng test cho flow và regression |
| [14.md](14.md) | Docker, CI/CD, health, logs, backup, rollback | Deploy, điều tra lỗi và phục hồi được |

## Nhịp học từ Chặng 8 đến 14

Từ Chặng 8, mỗi chặng đều có bốn đầu ra: code/project slice, dữ liệu biên, checkpoint và Interview checkpoint. Cụ không học Interview thành một bộ tách rời; sau khi làm xong bài thực hành, hãy tự trả lời câu hỏi, đọc code và debug trong thời gian giới hạn.

Mạch chính là:

1. C# hiểu sâu để viết async/import an toàn.
2. Thiết kế boundary để mở rộng importer/report.
3. SQL/EF giữ query nhanh và dữ liệu đúng.
4. API bảo vệ dữ liệu theo user.
5. React quản lý state/request không stale.
6. Test chứng minh behavior và regression.
7. Deploy/vận hành để biết hệ thống chạy thật ra sao.

## Sau Chặng 14

Chỉ mở rộng sang background jobs, Redis, message queue, SignalR hoặc system design khi monolith hiện tại đã có nhu cầu và cụ đo được vấn đề. Microservices, Kubernetes và DDD sâu không phải checkpoint bắt buộc trước khi cụ vận hành tốt một ứng dụng đơn khối.

## Chuẩn bị

Cài .NET SDK LTS, Node.js LTS, Git và một editor có hỗ trợ C#. Có thể dùng SQL Server LocalDB trên Windows cho các bài SQL. Mỗi project nên có README và commit nhỏ sau một trạng thái chạy được.

Roadmap dùng ví dụ SQL Server và .NET 10 trong phần API mẫu. Nếu SDK của bạn khác major version, giữ các package .NET/EF Core cùng major và đọc thông báo restore thay vì trộn ngẫu nhiên phiên bản.

## Quy tắc an toàn dữ liệu

Chỉ dùng dữ liệu học tập. Không đưa password, token, connection string thật hoặc dữ liệu người dùng vào prompt hay repository. Không chạy UPDATE/DELETE thiếu WHERE trên database có dữ liệu cần giữ. Không lưu secret trong VITE_ frontend environment vì giá trị đó đi vào bundle trình duyệt.

## Nguồn chính thức

- [C# và .NET](https://learn.microsoft.com/dotnet/csharp/)
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
- [EF Core](https://learn.microsoft.com/ef/core/)
- [SQL Server T-SQL](https://learn.microsoft.com/sql/t-sql/)
- [MDN Web Docs](https://developer.mozilla.org/docs/Web)
- [React Learn](https://react.dev/learn)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)

Ngày rà soát nội dung: 22/09/2026. Các lệnh cài đặt và phiên bản thư viện có thể thay đổi; khi chạy, ưu tiên tài liệu chính thức của phiên bản đang cài.

