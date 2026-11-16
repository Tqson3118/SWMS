# Hướng dẫn Anki cho các bộ thẻ IELTS

Dùng thẻ để thử nhớ câu trả lời trước khi xem mặt sau rồi ghi nhận mức nhớ của mình. Lịch ôn hỗ trợ phân bố lần luyện; tài liệu này không hứa nhớ lâu gấp3–5 lần, không chứng nhận năng lực từ một tỷ lệ thống kê. Khung15–20 phút là kế hoạch có thể điều chỉnh, không bảo đảm ôn hết trong thời gian ấy.

## Cài và chọn bộ thẻ

Mở [trang Anki chính thức](https://apps.ankiweb.net/) để chọn nền tảng và kiểm tra thông tin tải, giá hoặc yêu cầu hiện tại. Chọn deck riêng cho tháng đang học; chưa cần mua ứng dụng hay thêm tất cả thẻ của các phase cùng lúc. Tài liệu này không thay đổi deck/tiến độ hiện có.

Master CSV trong Resources hiện có461rows dữ liệu và461Front khác nhau; các mô tả40/~500 cũ không phải inventory hiện hành. Đây không phải chứng nhận đủ từ cho toàn Phase1 hoặc backup deck cá nhân. Hai thẻ scared/afraid từng có cùng Front nay dùng câu điền từ với gợi ý chữ đầu; cả Back/Tags và461notes được giữ. Vẫn kiểm tra duplicate settings trước nhập vào deck cá nhân; không xóa hay gộp thẻ tự động. Bộ Phase3 Month1 có120notes riêng, gồm24 từ C1 và24 mỗi C2–5, C6 không từ mới. Không nhập một deck C1 thứ hai. Ví dụ mặt trước của bộ này là tiếng Anh, không phải tiếng Việt; giữ đúng hướng đang dùng rồi giải thích nghĩa và thử câu riêng.

## Định dạng import đã đối chiếu với tài liệu chính thức

[Anki text import](https://docs.ankiweb.net/manual/importing/text-files): file UTF8, chọn delimiter và ánh xạ fields đúng. HTMLfields cho phép linebreak trong Back. Header dạng comment khai báo separator/columns; dòng Front,Back thông thường có thể trở thành note. Duplicate matching dựa trên firstfield theo phạm vi/type; kiểm tra tùy chọn trước nhập vào deck có sẵn.

Với Month1, dùng [file Anki120notes](../Phase-3/flashcards/personality-month1-120-anki.txt). File này có các header điều khiển và120rows dữ liệu; bản CSV gốc được giữ để đối chiếu. Trong cửa sổ import, chọn Basic, Front→Front và Back→Back. Xem preview, bảo đảm thẻ đầu là trait, không phải Front; kiểm tra IPA và linebreak. Nếu phiên bản không nhận header, dừng và tra tài liệu phiên bản, không nhập mù rồi xóa deck.

Xem báo cáo imported/updated/duplicate thực tế. Nó có thể khác120new nếu deck đã có thẻ; so sánh tổng120notes nguồn và tùy chọn đang dùng. Tài liệu mới chỉ kiểm tra cấu trúc file, chưa chạy import trong Anki trên thiết bị của bạn.

## Buổi luyện và điều chỉnh tải

Thử nhớ nghĩa/câu trước khi mở Back. Nếu sai hoặc không nhớ, ghi nhận Again; nếu nhớ đúng, chọn mức phù hợp với nỗ lực vừa trải qua. Không dự đoán chính xác phút/ngày gặp lại vì còn phụ thuộc scheduler và thiết lập.

Theo bài hiện hành, mỗi ngày mới có8từ; C6 ôn. Bắt đầu tải phù hợp rồi ghi thời gian thực và mức mệt. Nếu còn nhiều review, giảm hoặc tạm dừng newcards, chia buổi và xử lý lỗi từng thẻ. Không có số due-card cố định sau2–3ngày nghỉ, không coi150reviews hoặc85–90% là chuẩn sức khỏe bắt buộc.

[Deck options](https://docs.ankiweb.net/manual/deck-options) mô tả giới hạn new/review và burying. Các giá trị8newcards/ngày hay một khung thời gian là lựa chọn luyện của giáo trình, không mặc định hoặc bảo đảm khoa học của Anki. Reviewlimit có thể che việc còn tồn đọng; xem thông tin thực trong app trước tăng tải.

## Sync và backup

[Sync](https://docs.ankiweb.net/manual/syncing) trao đổi dữ liệu giữa thiết bị và AnkiWeb; nó không phải bản backup riêng. Xác nhận sync thành công theo app đang dùng, nhất là trước và sau khi đổi thiết bị; không hứa tự động chạy khi mở mọi app. Khi cần bản sao độc lập, dùng quy trình export/backup của ứng dụng và giữ file ở nơi riêng. Nếu app đề nghị chọn chiều thay thế collection hoặc báo xung đột, dừng để kiểm tra bản sao, không chọn reset/delete cho nhanh.

## Ghi chú nguồn và giới hạn

Đối chiếu bốn trang chính thức trên ngày2026-10-08. Hướng dẫn workflow/tải học là đề xuất tự luyện; bỏ các định lượng hiệu quả không có nguồn. Không có claim đã chạy Anki UI, nhập vào deck cá nhân hay sửa trạng thái learner. YAML dành riêng cho native Visualization; TXT dành cho Anki; CSV là nguồn Front/Back giữ nguyên.
