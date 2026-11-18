# Nhập Môn Cấu Trúc Dữ Liệu & Giải Thuật

Tài liệu học tập toàn diện về cấu trúc dữ liệu cơ bản cho sinh viên ngành Công nghệ thông tin.

## Chương 1: Mảng và Danh sách liên kết

### Bài 1.1: Tổng quan về Cấu trúc dữ liệu

Cấu trúc dữ liệu là cách tổ chức và lưu trữ dữ liệu trong máy tính sao cho có thể truy xuất và cập nhật một cách hiệu quả nhất. 

- **Độ phức tạp thời gian (Time Complexity):** Đánh giá thời gian thực thi của thuật toán qua ký hiệu Big-O.
- **Độ phức tạp không gian (Space Complexity):** Dung lượng bộ nhớ phụ trợ cần thiết.

Ví dụ: Tìm kiếm tuyến tính có độ phức tạp O(n), trong khi tìm kiếm nhị phân có độ phức tạp O(log n).

### Bài 1.2: Mảng động (Dynamic Array)

Mảng là một tập hợp các phần tử có cùng kiểu dữ liệu, được cấp phát liên tiếp trong bộ nhớ.
- Truy xuất phần tử theo chỉ số: O(1).
- Chèn hoặc xóa ở giữa: O(n).
- Mảng động (như `std::vector` trong C++ hay `List<T>` trong C#) tự động nhân đôi kích thước khi đạt ngưỡng dung lượng.

## Chương 2: Ngăn xếp và Hàng đợi

### Bài 2.1: Hoạt động của Stack & Queue

Ngăn xếp (Stack) hoạt động theo nguyên tắc LIFO (Last In, First Out).
Hàng đợi (Queue) hoạt động theo nguyên tắc FIFO (First In, First Out).

Ứng dụng thực tế:
- Stack: Quản lý call stack khi gọi hàm, hoàn tác (Undo/Redo), kiểm tra dấu ngoặc hợp lệ.
- Queue: Hàng đợi in ấn, xử lý thông điệp trong Message Broker, thuật toán BFS.
