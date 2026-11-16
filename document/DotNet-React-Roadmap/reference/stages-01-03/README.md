# Guided reference — Chặng 1–3

Đây là tài liệu instructor để đọc, chạy và debug một lát cắt nhỏ của Sổ chi tiêu. Nó hỗ trợ giải thích các bài trong [`1.md`](../../1.md), [`2.md`](../../2.md), [`3.md`](../../3.md); nó không thay thế yêu cầu người học viết lại từ file trống, breakpoint hay kiểm tra độc lập. Vì vậy trạng thái learner attainment vẫn là **chưa được chứng minh**.

## Chạy đúng phiên bản

SDK được ghim trong `global.json` là `10.0.401`, TFM là `net10.0`, và project test ghim `Microsoft.NET.Test.Sdk 17.12.0`, `xunit 2.9.3`, `xunit.runner.visualstudio 2.8.2`. Từ thư mục này:

```powershell
dotnet --version
dotnet test .\tests\ExpenseGuided.Tests\ExpenseGuided.Tests.csproj
dotnet run --project .\src\ExpenseGuided\ExpenseGuided.csproj
```

Expected `dotnet --version`: `10.0.401`. Expected current full-suite result: `Failed: 0, Passed: 33`; the earlier 720 two-field slice had 21 tests. Guided console flow (dữ liệu nhập được nối bằng newline) có thể chạy:

```powershell
"`nValid`n10`nNext`n1`nLast`ntext`n2" | dotnet run --project .\src\ExpenseGuided\ExpenseGuided.csproj
```

Actual verified output mới được ghi trong [validation/dotnet-reference720](../../../tasks/course-remediation-2026-09-30/validation/dotnet-reference720/console-output.txt). Flow thực tế cho thấy category rỗng và `text` bị từ chối, retry giữ category `Last`, và tổng cuối là `13`. Zero amount được kiểm tra riêng trong test.

Tiền ở reference dùng decimal invariant với dấu chấm thập phân tùy chọn (`12.5`), không nhận dấu phân cách hàng nghìn (`1,000`). Điều này giữ quy tắc nhập VNĐ nguyên đơn giản cho người mới; domain vẫn dùng `decimal` để không khóa hợp đồng Stage 4–7 vào kiểu số giao diện. Hợp đồng ngoài reference chỉ gồm `category` và `amount`.

## Hợp đồng kiến trúc để bàn giao sớm

`Expense` là domain invariant: category phải có nội dung và amount không âm. `ExpenseService` giữ danh sách và áp dụng tổng/tổng theo category (không phụ thuộc file hay console). `JsonExpenseStore` là data boundary: serialize/deserialize bằng `System.Text.Json`, validate lại sau khi đọc, và ném `InvalidDataException` khi JSON hỏng hoặc vi phạm invariant; nó không ghi đè file hỏng bằng danh sách rỗng. `ConsoleInput`, `ConsoleRunner` và `Program` chỉ là lớp hướng dẫn input/output. Runner nhận `TextReader`/`TextWriter`, retry invalid amount/category, dừng sạch khi EOF và chỉ in tổng sau đúng ba khoản hợp lệ. Stage 4–7 có thể dùng hợp đồng này làm điểm nối, nhưng reference này không chứa SQL, API hay frontend.

## Cách đọc bài và kiểm tra biên

Đọc lần lượt phần input/loop/hàm/list trong Chặng 1, phân rã và collection trong Chặng 2, rồi domain/interface/file/JSON/test trong Chặng 3. Chạy test để quan sát các trường hợp valid, zero, negative, text, empty và EOF; sau đó tự thay đổi một input và ghi expected/actual. Test kiểm tra hành vi qua reader/writer cô lập, retry loop thực tế, invariant domain, tổng chính xác, category không phân biệt hoa thường, round-trip trong thư mục tạm được giữ lại và byte file corrupt được giữ nguyên. Các test không xóa thư mục tạm.

## Nguồn kỹ thuật đã kiểm chứng

- `global.json` chọn SDK CLI và `rollForward: disable` yêu cầu bản ghim; [Microsoft Learn — global.json overview](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json?tabs=netcore3x), mục “rollForward” và “global.json and the .NET CLI”.
- `dotnet test` build và chạy test project; [Microsoft Learn — dotnet test with VSTest](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test-vstest), mục “Description”.
- Mẫu xUnit chính thức dùng `dotnet test`, `Microsoft.NET.Test.Sdk`, `xunit`; [Microsoft Learn — unit testing C# with xUnit](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit), mục “Create a test” và “The test fails”.
- JSON cần deserialize rồi validate dữ liệu ứng dụng; xem [System.Text.Json overview](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview), phần serialization/deserialization.

## DN45-1B identity/date handoff

This appended slice keeps the beginner `Expense` model unchanged and adds `DatedExpense`, `DatedExpenseService`, and `DatedJsonExpenseStore`. The domain contract is: caller supplies a positive integer `Id`, nonempty trimmed `Category`, nonnegative decimal `Amount`, and a valid `DateOnly SpentOn`. Every representable calendar date is allowed; no future/past business rule is invented. Service operations are guided examples for add, update, remove, list, duplicate-ID rejection, and year/month totals. A failed update validates the replacement before changing the previous valid value; unknown update/remove returns `false`.

The JSON boundary requires `Id`, `Category`, `Amount`, and `SpentOn` with the expected JSON types. It rejects null entries, duplicate IDs, missing fields, invalid categories, negative amounts, invalid dates, malformed JSON, and a non-array root. It raises `InvalidDataException` and leaves the original bytes untouched. The SQL/API handoff is therefore `positive-int id + category string + amount decimal + spentOn date`; normalization and persistence policy remain later-stage work. Evidence for this refinement is in [validation/dotnet-identity727](../../../tasks/course-remediation-2026-09-30/validation/dotnet-identity727/results.json). This remains instructor guidance; it does not attest a full UI, independent learner gate, SQL/API implementation, or course approval.
