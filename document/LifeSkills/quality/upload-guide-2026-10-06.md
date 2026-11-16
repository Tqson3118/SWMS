# LifeSkills — hướng dẫn import bản phát hành 2026-10-06

Bản phát hành gồm đủ W01–W16. Mở [README](../README.md) để tự học bằng các bài, quiz, đáp án và flashcard. Bản ZIP giữ nguyên thư mục LifeSkills; giải nén vào một thư mục riêng trước khi chọn manifest. ZIP là bản đóng gói lưu chuyển, không phải định dạng đã xác nhận cho nút upload trực tiếp của Visualization.

Import từng manifest theo thứ tự manifest.yaml (W01), manifest-w02.yaml, …, manifest-w16.yaml, bằng luồng import manifest hiện có của Visualization. Mỗi manifest tham chiếu notes, assessments, flashcards và quality tương đối với thư mục LifeSkills; không chép riêng tệp manifest. Cùng subject_id roadmap:subject:lifeskills, mỗi tuần có định danh riêng và sort_order 1–16. Dùng kiểm tra/preview trước khi apply trong môi trường của bạn; việc chuẩn bị này chưa ghi khóa học hay tiến độ vào cơ sở dữ liệu thật.

Kiểm tra sau import: có đúng 16 tuần theo thứ tự, bài đọc đầy đủ, 152 câu quiz cùng 456 giải thích lựa chọn sai và 210 flashcard; không thêm bản lịch sử/backup như bài học mới. Định danh ổn định phải giữ nguyên để tránh tạo bản sao. Nếu thấy xung đột với dữ liệu đã có, xử lý qua báo cáo import trước khi apply; không xóa dữ liệu người học để vượt lỗi.

Nguồn đã được đánh giá theo claim và theo phần thân/abstract/catalog thực sự truy cập được; các bài thực hành do khóa học biên soạn được ghi như thực hành, không thành bằng chứng hiệu quả phổ quát. Nội dung giáo dục không thay thế chẩn đoán hay trị liệu. Quiz và rubric hỗ trợ tự kiểm tra, không chứng nhận năng lực cá nhân. Browser437/456 kiểm tra giao diện thật với API và dữ liệu thử cô lập; chưa kiểm tra live persistence. Cảnh báo icon ArrowRight trong modal ẩn và font công thức tiếng Việt không chặn bài; screenshot456 cho thấy câu công thức W16 đọc được.

Chỉ dùng các tệp nằm trong danh sách bản phát hành. Các bản cũ, archive, báo cáo sửa chữa vẫn được giữ trong Roadmap nhưng không được coi là bài hiện hành để import.
