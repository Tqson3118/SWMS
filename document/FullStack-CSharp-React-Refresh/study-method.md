# Cách học: chẩn đoán → sửa gap → áp dụng → kiểm chứng

Lịch tham khảo 12–18 giờ kỹ thuật mỗi tuần, cộng 30–60 phút English/ngày. Nếu ít thời gian hơn, kéo dài tuần cho đến khi đạt gate; không bỏ bài vì lịch hết.

1. Làm quiz trước, ghi độ tự tin từng câu. Không tra đáp án.
2. Tự viết ví dụ bằng input khác. >=5/6 và practice độc lập đúng: ôn 30–60 phút rồi chuyển sang project/deep dive. Thấp hơn: học đúng mục LO của câu sai, thực hiện ví dụ, tạo phản ví dụ.
3. Làm hai bài tập trước khi đọc hướng giải. Lưu expected/actual và giải thích vì sao.
4. Project feature phải dùng lại fixture/contract, không xây một app mới mỗi tuần.
5. Ghi docs/evidence/wNN.md trong repo học viên: commit, command, input, expected, actual, test/log/ảnh, lỗi và cách sửa. Không có secrets.
6. Quiz >=5/6 và mọi behavior checkbox mới đạt tuần. Rubric từng hàng >=2; lỗi ownership/auth/atomicity không được bù bằng điểm lý thuyết.
7. Flashcards ôn sau 1/3/7/14 ngày; thẻ sai cần gắn lại LO và viết một code example.

## Sổ theo dõi gaps

| LO | Chẩn đoán | Hiểu lầm | Bài sửa | Bằng chứng độc lập | Ngày kiểm lại |
|---|---|---|---|---|---|
| LO-W17-01 | Cast không validate JSON | Tin server shape | Guard unknown | Test string/negative rejected | Sau 24h |

Đây là ví dụ đã điền, không phải kết quả học viên. Tạo hàng của mình trong repo.

## DSA song song

Từ chặng C#, 2–3 buổi/tuần, mỗi buổi 25–35 phút. Tổng bắt buộc 16 bài: frequency map, TwoSum, duplicate detection, anagram count, stack brackets, queue simulation, binary search, lower bound, merge sorted arrays, two pointers pair sum, remove duplicates, sliding window max sum, longest unique substring, BFS shortest unweighted path, sorting comparison, category aggregation. Mỗi bài ghi brute force, cải tiến, edge cases, complexity và tests.

Advanced DP/competitive programming là tùy chọn. W28 kiểm lại hai bài mới trong 45 phút thay vì học dồn cả DSA.

