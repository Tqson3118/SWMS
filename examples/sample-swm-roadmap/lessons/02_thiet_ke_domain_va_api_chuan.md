# Bài 2: Thiết kế Domain Model, Validation & HTTP Contract

Một API dễ bảo trì bắt đầu từ **ràng buộc nghiệp vụ nằm ngay trong Domain** thay vì rải rác câu lệnh `if` khắp Controller hay Endpoint.

## 1. Bảng phân biệt mã phản hồi HTTP cốt lõi

| Tình huống | Mã HTTP chuẩn | Ý nghĩa cho Frontend | Cách xử lý UI khuyến nghị |
| --- | --- | --- | --- |
| Payload thiếu trường, sai định dạng | `400 Bad Request` | Lỗi đầu vào từ biểu mẫu | Hiển thị rõ lỗi ngay dưới trường nhập liệu |
| Tài nguyên không tồn tại hoặc không thuộc quyền sở hữu | `404 Not Found` | ID sai hoặc đã bị xóa | Điều hướng về danh sách hoặc hiện trạng thái trống |
| Dữ liệu bị người/tab khác cập nhật trước đó | `409 Conflict` | Xung đột phiên bản (`RowVersion`) | Tải lại bản mới nhất rồi mời người dùng gộp thay đổi |
| Lỗi quy tắc nghiệp vụ hợp lệ cú pháp nhưng sai logic | `422 Unprocessable Entity` | Vi phạm invariant (ví dụ vượt hạn mức) | Hiện thông báo nguyên nhân + gợi ý điều chỉnh |

## 2. Ví dụ C#: Ràng buộc bất biến trong Entity

```csharp
public sealed class StudyGoal
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public int TargetMinutesPerDay { get; private set; }

    public StudyGoal(string title, int targetMinutesPerDay)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề mục tiêu không được rỗng.", nameof(title));

        if (targetMinutesPerDay is < 5 or > 480)
            throw new ArgumentOutOfRangeException(nameof(targetMinutesPerDay), "Thời lượng học mỗi ngày phải từ 5 đến 480 phút.");

        Id = Guid.NewGuid();
        Title = title.Trim();
        TargetMinutesPerDay = targetMinutesPerDay;
    }
}
```

> [!TIP]
> Với validation đầu vào từ HTTP Request, trả về **Result object hoặc ProblemDetails** thay vì ném `Exception` cho mọi sai sót nhập liệu thông thường để log hệ thống chỉ ghi nhận lỗi thực sự bất thường.

## 3. Tự kiểm tra hiểu bài

<details>
<summary>👉 Tình huống: Người dùng bấm "Lưu ghi chú" cùng lúc trên hai tab trình duyệt khác nhau. Backend nên trả mã nào và Frontend phản ứng ra sao?</summary>

**Phân tích đề xuất:**

- Backend kiểm tra `Version` / `ETag`: nếu phiên bản client gửi lên nhỏ hơn phiên bản hiện tại trong database, trả **`409 Conflict`** kèm phiên bản mới nhất trên server.
- Frontend tuyệt đối không âm thầm ghi đè bản của tab kia; cần báo xung đột và giữ bản nháp hiện tại để người dùng đối chiếu.
</details>

## 4. Bài tập đối chiếu nhanh

1. **Câu hỏi 1:** Điểm khác biệt thực tế giữa `400 Bad Request` và `422 Unprocessable Entity` là gì?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>

   **Đáp án gợi ý:** `400` thường báo sai cú pháp JSON hoặc thiếu kiểu dữ liệu bắt buộc ở tầng transport/binding; `422` báo cấu trúc JSON hoàn toàn hợp lệ nhưng vi phạm **quy tắc nghiệp vụ** (ví dụ ngày kết thúc nhỏ hơn ngày bắt đầu).
   </details>

2. **Câu hỏi 2:** Vì sao setter của thuộc tính trong Domain Entity nên để `private set`?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>

   **Đáp án gợi ý:** Để mọi thay đổi trạng thái bắt buộc phải đi qua phương thức có kiểm tra invariant, tránh việc code bên ngoài sửa lẻ một trường làm hỏng tính nhất quán của đối tượng.
   </details>
