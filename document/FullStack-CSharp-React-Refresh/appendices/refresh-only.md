# C++, NoSQL, Vue — tùy chọn, 3 buổi × 60–90 phút
Không gate core; không mở dự án thứ hai.
## C++: đọc code và ownership
Value copy, reference alias, pointer nullable; RAII hủy resource khi hết scope; vector/map; iterator invalidation sau thay đổi container. So sánh RAII với using/IDisposable C#: thời điểm dispose tường minh khác GC. Đọc hàm truyền const std::vector<int>&, chỉ ra không copy container; viết function sum bằng int64_t; input [] ->0, [1,2]->3. Giải thích raw pointer không tự nói ai sở hữu memory. Không học template metaprogramming trước job.
Nguồn: https://isocpp.org/get-started .
## NoSQL: đọc schema và truy vấn
Document embed khi dữ liệu thường đọc cùng và kích thước có giới hạn; reference khi quan hệ lớn/nhiều phía. "Schema flexible" không có nghĩa bỏ validation/index/transaction. Ví dụ expense document chứa categoryName snapshot: đổi category tên cũ có cập nhật không? Quyết định snapshot historical hoặc lookup current, ghi tradeoff. Không chuyển SQL sang MongoDB chỉ để thêm stack.
Nguồn: https://www.mongodb.com/docs/manual/data-modeling/ .
## Vue: đọc project cũ
ref reactive value dùng .value trong script; computed cho derived state; watch cho side effects; component props và emits boundary; không sửa prop trực tiếp. Đọc một component list expense, tìm state/filter/event; ánh xạ computed -> tính toán render/useMemo khi cần, watch -> effect có cleanup. Không coi lifecycle React/Vue giống từng dòng.
Nguồn: https://vuejs.org/guide/essentials/reactivity-fundamentals.html .
## Tự kiểm tra
Giải thích được đoạn cũ, sửa một lỗi nhỏ và chạy expected/actual; dừng sau 3 buổi nếu không có dự án maintenance cần thêm. Các bài này không thay C#/SQL/React core.
