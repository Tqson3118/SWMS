# Upload và import

7 chặng /28 tuần /84 bài nhỏ. Mỗi tuần là một unit:1 tài liệu có3 bài,6 quiz questions,6 đáp án đúng và18 giải thích sai,6 flashcards,1 ledger.

## Format và navigation

Markdown là bản nguồn. HTML cùng tên được render cho importer; lesson_theory trong manifest trỏ HTML. Không đưa Markdown thô vào HTML sanitizer vì generics C#/JSX có thể bị cắt. Quiz/answers giữ Markdown, cards/manifest giữ YAML.

Mỗi tuần dùng module key phase-N-week-NN riêng để tránh trùng key nhưng title khác. Trong bộ tài liệu vẫn có7 thư mục chặng; UI importer non-psychology hiện tạo28 thư mục tuần. Navigation của app theo units; relative links trong tài liệu tham chiếu bộ nguồn đã giải nén, không giả định app đã publish các file đó thành routes.

## Kiểm tra trước upload

Chạy từ thư mục khóa với Python3.11+:
```text
python -m pip install -r tools/requirements.txt
python tools/check_sources.py
python tools/build_course.py
python tools/validate_course.py
python tools/validate_import.py 1
```

Lặp validate_import cho2..7. Script dùng DLL Visualization hiện có theo absolute path bên trong script. Nếu DLL được chuyển, cập nhật constant trong khóa, không sửa ứng dụng Visualization.

Dry-run/static validate/export-preview offline đã kiểm đủ28 units, không ghi DB hoặc mở network sockets. Script so full lesson text, prompts/options/keys,correct feedback/3wrong feedback,concepts,anchors,cards/claims và citations đúng tuần; không chỉ so counts.

## Package

Giữ28 manifest và cấu trúc notes HTML+MD,assessments,flashcards,quality/ledger-wNN.md,shared docs. Global source-ledger.md là index; manifest trỏ ledger riêng để citations không lẫn tuần.

Sửa nội dung phải chạy build và checks để hashes/byte counts khớp. Không đổi stable IDs khi cập nhật cùng unit. tools/package_course.py tạo ZIP trong quality, loại source-cache,runtime bin/obj,preview payloads và chính ZIP. Giữ validation reports/source metadata trong package.

Upload/import database thật chưa được thực hiện. Học liệu và các gates học viên không chứng minh project đã được triển khai, cũng không thay acceptance của app import đích. Xem quality/progress.md cho readiness và giới hạn thực tế.
