# Bảng Kiểm Tra Chất Lượng Nội Dung (Content Audit Checklist)

> **Mục đích:** Là Quality Gate chính thức gồm **51 tiêu chí kiểm tra bắt buộc** (43 tiêu chí nền tảng + 8 tiêu chí chống manh mối hình thức & chất lượng trắc nghiệm) để đánh giá từng tuần bài học, quiz, answer key, milestone check và toàn bộ khóa học.  
> **Nguyên tắc:** Mỗi tiêu chí phải có điều kiện PASS khách quan, định lượng và đo lường được. Không sử dụng các tiêu chí cảm tính như "viết hay", "nguồn uy tín". Mọi mục không áp dụng phải ghi rõ `N/A — lý do`.

---

## 1. Hệ thống Phân loại Mức độ Nghiêm trọng (Severity Levels)

- 🔴 **Critical (Nghiêm trọng):** Vi phạm an toàn (dạy thao túng, đọc vị, hướng dẫn tự chẩn đoán, kỳ thị); bịa đặt nguồn, số liệu, DOI, tác giả; sai lệch kiến thức khoa học làm đảo ngược bản chất vấn đề. **Bắt buộc từ chối duyệt (CHANGES REQUESTED hoặc BLOCKED).**
- 🟡 **Important (Quan trọng):** Thiếu nguồn cho claim chính; trích dẫn không khớp nội dung gốc; thiếu các mục bắt buộc trong Content Contract; ma trận quiz mất cân đối; link nội bộ hỏng; ngôn ngữ quá chắc chắn thiếu căn cứ; gán nhãn bằng chứng sai quy chuẩn. **Phải sửa trước khi APPROVED.**
- 🟢 **Minor (Nhỏ):** Lỗi chính tả, định dạng bảng chưa tối ưu, câu từ có thể trau chuốt thêm nhưng không làm sai lệch kiến thức hoặc trải nghiệm học. **Có thể ghi nhận để hoàn thiện.**

---

## 2. Tiêu chuẩn Đánh giá Chi tiết (Detailed Audit Matrix — 51 Tiêu chí)

### Nhóm 1: Cấu trúc & Hợp đồng Nội dung (Spec & Content Contract — 10 tiêu chí)

| Mã | Tên tiêu chí | Định nghĩa vi phạm | Điều kiện PASS (Chuẩn bắt buộc) | Phương pháp kiểm tra | Severity |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **SCC-01** | Header Metadata | Thiếu thời lượng, mục tiêu, hoặc chặng. | File có đủ: Tuần, Chặng, Thời lượng 2 tầng (Cốt lõi 30–45m / Đọc sâu 2–3h), Prerequisites, 3–5 Mục tiêu đo được. | Kiểm tra đầu file bài học. | 🟡 Important |
| **SCC-02** | Thứ tự Section | Các mục bị đảo lộn, thiếu mục bắt buộc. | Đủ 12 mục theo đúng thứ tự: Ý nghĩa → Mục tiêu → Học nhanh khi mệt → Bài đọc sâu → Nghiên cứu/Mô hình tiêu biểu → Biết gì/Chưa kết luận gì → Hiểu lầm & Phản biện → 2 Mini-reviews → Case/Journal → Glossary → Nguồn → Link Quiz. | Đối chiếu cây thư mục heading. | 🟡 Important |
| **SCC-03** | Khối lượng từ | Quá ngắn (<1500 từ) hoặc bôi chữ rỗng ý. | Tầng "Học nhanh khi mệt" đạt 600–900 từ (kết thúc 5 takeaways). Toàn bài đọc sâu lý thuyết (Mục 4) đạt 2.000–3.500 từ, súc tích, không lặp ý. (Quy ước kỹ thuật: PowerShell `Measure-Object -Word` đếm âm tiết đơn lập cách nhau bởi khoảng trắng. Khung 2.000–3.500 từ áp dụng cho khối lý thuyết Deep Dive Mục 4; các phần bổ trợ thực hành, case study, glossary Mục 5–10 được tính riêng để không làm loãng lý thuyết cốt lõi). | Đếm số từ thực tế. | 🟡 Important |
| **SCC-04** | Glossary Tuần | Thiếu số lượng hoặc thiếu thuật ngữ gốc. | Có 8–15 thuật ngữ định nghĩa rõ ràng; có thuật ngữ tiếng Anh gốc trong ngoặc đơn ở lần đầu xuất hiện. | Đếm và đối chiếu danh sách glossary. | 🟡 Important |
| **SCC-05** | Hai Mini-reviews | Thiếu câu hỏi ôn tập giữa tuần. | Có đủ 2 phần mini-review (mỗi phần 3–5 câu retrieval practice / active recall kèm đáp án gợi mở). | Kiểm tra section Mini-review trong bài. | 🟡 Important |
| **SCC-06** | File Quiz Tách biệt | Gộp chung quiz và đáp án vào bài đọc. | Quiz nằm ở `assessments/quizzes/week-NN-quiz.md`, gồm 8–12 câu hỏi trắc nghiệm hoàn chỉnh. | Kiểm tra đường dẫn file quiz. | 🔴 Critical |
| **SCC-07** | File Answer Key | Thiếu giải thích sai, thiếu link bài. | Answer key nằm ở `assessments/answer-keys/week-NN-answers.md`, có giải thích đáp án đúng, lý do phương án nhiễu sai, mục tiêu đo, và link ngược về heading bài học. | Đối chiếu từng câu hỏi trong key. | 🔴 Critical |
| **SCC-08** | Tự nguyện Reflection | Bắt buộc chia sẻ đời tư, ép buộc format. | Mục Case study / Journal nêu rõ tính tự nguyện; luôn có sẵn 1 kịch bản nhân vật giả định (hypothetical case) để thực hành. | Soát nội dung bài tập reflection. | 🔴 Critical |
| **SCC-09** | Tự đủ của Tầng cốt lõi | Người đọc nhanh không đủ thông tin làm quiz. | Phần "Học nhanh khi mệt" chứa đầy đủ thông tin để trả lời 100% các câu hỏi kiểm tra kiến thức thiết yếu (nhớ/hiểu). | Đối chiếu câu hỏi quiz với phần đọc nhanh. | 🟡 Important |
| **SCC-10** | Cấu trúc 48 tuần | Thay đổi thứ tự hoặc xóa bỏ tuần trong spec. | Đúng tên tuần, chặng và mục tiêu đã duyệt trong đặc tả 48 tuần (`docs/superpowers/specs/...`). | Đối chiếu với curriculum spec. | 🔴 Critical |

---

### Nhóm 2: Độ chính xác Khoa học & Xác minh Nguồn (Scientific Rigor & Sources — 10 tiêu chí)

| Mã | Tên tiêu chí | Định nghĩa vi phạm | Điều kiện PASS (Chuẩn bắt buộc) | Phương pháp kiểm tra | Severity |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **SRC-01** | Claim-Source Mapping | Claim trong bài không có trong Ledger. | 100% mốc năm, tên nhà khoa học, số liệu, kết luận thực nghiệm đều có mã Claim ID và 1 dòng tương ứng trong `quality/source-ledger.md`. | So khớp Claim ID giữa bài học và Ledger. | 🔴 Critical |
| **SRC-02** | Tính xác thực Nguồn | Bịa tác giả, năm, DOI, URL hoặc số liệu. | Tất cả URL/DOI và thông tin trích dẫn đã được mở kiểm tra thực tế, đối chiếu chính xác với văn bản gốc. | Kiểm tra ngẫu nhiên $\ge 5$ nguồn/tuần. | 🔴 Critical |
| **SRC-03** | Phân cấp Nguồn | Dùng sách Pop Psych làm căn cứ duy nhất. | Kiến thức cốt lõi dựa trên Cấp 1 (Giáo trình ĐH/OER), Cấp 2 (Meta-analysis), Cấp 3 (APA/WHO/NIMH). Cấp 5 chỉ dùng phản biện. | Kiểm tra cột "Loại nguồn" trong Ledger. | 🔴 Critical |
| **SRC-04** | Gán nhãn Bằng chứng | Khẳng định tuyệt đối điều đang tranh cãi; nhầm effect size với độ chắc chắn. | Mọi mô hình/kết luận đều gắn 1 trong 4 nhãn: **[Mạnh]**, **[Khá]**, **[Sơ bộ]**, **[Tranh luận]** (hoặc **[Chưa đánh giá]** nếu thiếu dữ kiện) dựa trên tổng thể bằng chứng; tách biệt độ chắc chắn bằng chứng khỏi độ lớn hiệu ứng. | Soát nhãn trong các section chính. | 🟡 Important |
| **SRC-05** | Tương quan vs Nhân quả | Dùng từ "gây ra" cho nghiên cứu tương quan. | Nghiên cứu tương quan chỉ dùng: "liên hệ với", "đi kèm", "dự báo". Tuyệt đối không dùng "dẫn đến", "gây ra" nếu không có RCT/thực nghiệm. | Tìm kiếm từ khóa "gây ra", "dẫn đến". | 🔴 Critical |
| **SRC-06** | Thống kê vs Thực tiễn | Diễn giải sai $p$-value; tự bịa effect size/CI hoặc dùng $p$-value đơn độc để tuyên bố tầm quan trọng. | Định nghĩa đúng $p$-value theo xác suất có điều kiện $P(\text{Data} \mid H_0)$; không đồng nhất $p < .05$ với ý nghĩa thực tiễn; khi nguồn gốc có báo cáo effect size và khoảng tin cậy (CI) thì bài học phải ghi lại đầy đủ; nếu nguồn gốc không báo cáo thì ghi rõ "nguồn không báo cáo", tuyệt đối không tự ý ngoại suy hoặc bịa đặt số liệu. | Soát các số liệu định lượng và diễn giải trong bài. | 🟡 Important |
| **SRC-07** | Mẫu & WEIRD Bias | Khái quát hóa mẫu sinh viên phương Tây. | Ghi nhận rõ giới hạn về văn hóa, độ tuổi, bối cảnh khi thảo luận các nghiên cứu thực hiện trên mẫu WEIRD. | Kiểm tra mục "Giới hạn" và "Chưa thể kết luận". | 🟡 Important |
| **SRC-08** | Khủng hoảng Tái lập | Trình bày hiệu ứng không tái lập như chân lý. | Các nghiên cứu kinh điển gặp vấn đề tái lập (như priming quá mức, ego depletion hữu hạn) phải gắn nhãn **[Tranh luận]** và nêu rõ kết quả Open Science. | Đối chiếu danh mục Replication Crisis. | 🔴 Critical |
| **SRC-09** | Tránh Tuyệt đối hóa | Dùng "luôn luôn", "chắc chắn", "mọi người". | Văn phong khoa học thể hiện tính xác suất, điều kiện kích hoạt và sự đa dạng cá nhân. | Quét các từ ngữ khẳng định tuyệt đối. | 🟡 Important |
| **SRC-10** | Trích dẫn Chi tiết | Chỉ ghi tên tác giả chung chung không rõ sách. | Trích dẫn giáo trình Cấp 1 phải có: Tên tác giả, Năm, Tên sách, Phiên bản (Edition), Chương hoặc Mục. | Soát danh mục Tài liệu tham khảo. | 🟡 Important |

---

### Nhóm 3: An toàn, Đạo đức & Chống kỳ thị (Safety & Ethics — 8 tiêu chí)

| Mã | Tên tiêu chí | Định nghĩa vi phạm | Điều kiện PASS (Chuẩn bắt buộc) | Phương pháp kiểm tra | Severity |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **SAF-01** | Cấm Đọc vị & Thao túng | Dạy thủ thuật "nhìn thấu tâm can", ép buộc. | Ngôn ngữ cơ thể và hành vi được dạy dưới dạng đa nghĩa; yêu cầu đưa ra ít nhất 3 giả thuyết; không dạy dark psychology hay thao túng. | Soát nội dung giao tiếp và nhận thức xã hội. | 🔴 Critical |
| **SAF-02** | Cấm Tự Chẩn đoán | Hướng dẫn người học tự kết luận bệnh lý. | Phân biệt rõ distress thông thường với tiêu chuẩn chẩn đoán lâm sàng; ghi rõ disclaimer: nội dung không thay thế chẩn đoán chuyên môn. | Kiểm tra các bài Chặng 2 và Chặng 5. | 🔴 Critical |
| **SAF-03** | Chống Dán nhãn Độc hại | Dùng bừa bãi các từ "toxic", "narcissist". | Không dùng "kẻ thao túng", "narcissist", "bệnh hoạn" như nhãn định danh; tập trung vào hành vi cụ thể, bối cảnh và mức độ an toàn. | Tìm kiếm từ khóa "toxic", "narcissist". | 🔴 Critical |
| **SAF-04** | Person-First Language | Dùng ngôn ngữ gán nhãn ("người tâm thần"). | Sử dụng ngôn ngữ tôn trọng con người trước (*Person-First Language*): "người trải qua trầm cảm", "người có phong cách gắn bó lo âu". | Quét ngôn ngữ trong toàn bộ bài học. | 🟡 Important |
| **SAF-05** | Tính liên tục (Continuum) | Coi tính cách / tâm lý là nhị nguyên trắng/đen. | Big Five, gắn bó và sức khỏe tâm thần được tiếp cận theo dải liên tục (spectrum/continuum) và tương tác sinh - tâm - xã hội. | Soát bài Tính cách và Tâm bệnh học. | 🟡 Important |
| **SAF-06** | Định nghĩa Consent | Cho rằng im lặng là đồng ý, thiếu ranh giới. | Consent được định nghĩa đúng 5 yếu tố: Tự nguyện, Có thể rút lại, Có thông tin, Rõ ràng/Nhiệt tình, Cụ thể (không suy đoán từ im lặng). | Kiểm tra bài học về Quan hệ & Ranh giới. | 🔴 Critical |
| **SAF-07** | Bảo mật Đời tư | Yêu cầu kể lại sang chấn, xung đột gia đình. | Bài tập reflection không đòi hỏi người học khai báo thông tin nhạy cảm, bệnh sử, quan hệ tình dục; luôn có case giả định. | Kiểm tra phần hướng dẫn Journal/Case. | 🔴 Critical |
| **SAF-08** | Hướng dẫn Khẩn cấp | Chỉ đưa số điện thoại của một quốc gia cụ thể. | Khuyến cáo khẩn cấp được viết trung lập: liên hệ cơ sở y tế địa phương, đường dây hỗ trợ khủng hoảng tại khu vực sở tại hoặc chuyên gia tâm lý có phép. | Soát disclaimer và hướng dẫn khẩn cấp. | 🟡 Important |

---

### Nhóm 4: Chất lượng Sư phạm & Ma trận Đánh giá (Pedagogy & Assessment — 8 tiêu chí)

| Mã | Tên tiêu chí | Định nghĩa vi phạm | Điều kiện PASS (Chuẩn bắt buộc) | Phương pháp kiểm tra | Severity |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **PED-01** | Mục tiêu Đo được | Mục tiêu chung chung ("hiểu về tâm lý"). | Mục tiêu dùng động từ hành động theo thang Bloom (nhận diện, phân biệt, phân tích, đánh giá giới hạn). | Soát 3–5 bullets mục tiêu đầu bài. | 🟡 Important |
| **PED-02** | Ma trận Quiz | Quiz toàn câu hỏi ghi nhớ vặt hoặc toàn mẹo. | Phân bổ tỷ lệ chuẩn: ~40% Nhớ/Hiểu (cốt lõi), ~40% Áp dụng tình huống, ~20% Đánh giá giới hạn bằng chứng/phương pháp. | Phân loại từng câu hỏi trong Quiz. | 🟡 Important |
| **PED-03** | Độ phủ Mục tiêu | Có mục tiêu bài học không được kiểm tra. | 100% mục tiêu học tập của tuần đều có ít nhất 1 câu hỏi quiz tương ứng đo lường trực tiếp. | Lập bảng ma trận Mục tiêu – Câu hỏi Quiz. | 🟡 Important |
| **PED-04** | Công bằng Đánh giá | Câu hỏi đòi hỏi kiến thức ngoài bài học. | Tất cả câu hỏi đều có thể trả lời được dựa hoàn toàn vào nội dung bài học đã cung cấp; không có câu hỏi bẫy chữ vô nghĩa. | Đối chiếu từng câu hỏi với nội dung bài. | 🟡 Important |
| **PED-05** | Phương án Nhiễu (Distractors) | Phương án sai ngớ ngẩn, không có tính sư phạm. | Phương án sai phản ánh đúng các hiểu lầm thực tế (pop psych) hoặc lỗi tư duy trực giác phổ biến để giúp người học nhận diện điểm mù. | Soát các phương án B, C, D của từng câu. | 🟡 Important |
| **PED-06** | Tính Sư phạm của Key | Chỉ ghi "A đúng" mà không giải thích. | Answer key phân tích chi tiết: vì sao đáp án đúng, vì sao các phương án khác sai, trỏ về mục kiến thức trong bài để ôn lại. | Đọc toàn bộ file Answer Key. | 🔴 Critical |
| **PED-07** | Đa Giả thuyết Phân tích | Phân tích tình huống chỉ đưa ra 1 kết luận. | Các bài tập ứng dụng tình huống luôn yêu cầu học viên đưa ra ít nhất 3 giả thuyết thay thế và xác định các dữ kiện còn thiếu. | Soát phần hướng dẫn phân tích case. | 🟡 Important |
| **PED-08** | Không Đổi Điểm thành Bệnh | Dùng điểm quiz để phán xét tính cách/tâm thần. | Quiz chỉ đo mức độ nắm vững kiến thức; tuyệt đối không tạo thang điểm "mức độ rối loạn", "loại tính cách", "chỉ số thông minh". | Kiểm tra cách tính điểm và giải thích kết quả. | 🔴 Critical |

---

### Nhóm 5: Tính toàn vẹn Kỹ thuật & Markdown (Technical Integrity — 7 tiêu chí)

| Mã | Tên tiêu chí | Định nghĩa vi phạm | Điều kiện PASS (Chuẩn bắt buộc) | Phương pháp kiểm tra | Severity |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **TEC-01** | Chuẩn cú pháp Markdown | Lỗi vỡ layout, heading lộn xộn. | Render chuẩn trên trình xem Markdown; sử dụng đúng thứ bậc heading (H1 → H2 → H3); bảng biểu và alert box hiển thị đẹp mắt. | Preview Markdown rendering. | 🟢 Minor |
| **TEC-02** | Liên kết Nội bộ (Links) | Link tương đối bị hỏng (404/broken). | 100% link tương đối giữa bài học, quiz, answer key, roadmap, glossary và source ledger trỏ đúng file và đúng anchor tồn tại. | Kiểm tra cú pháp và sự tồn tại của file đích. | 🔴 Critical |
| **TEC-03** | Đồng bộ File Quản trị | Viết bài mới nhưng không cập nhật ledger/glossary. | Khi tạo hoặc sửa tuần học, các thuật ngữ mới được cập nhật vào `glossary.md`, nguồn mới vào `quality/source-ledger.md`, tiến độ vào `progress.md`. | Đối chiếu diff các file quản trị. | 🟡 Important |
| **TEC-04** | Trạng thái Link Chưa có | Đặt link đến bài chưa viết như thể đã có. | Các bài chưa viết phải được đánh dấu rõ "[Sắp biên soạn]" trên roadmap, không tạo link rỗng gây hiểu lầm. | Soát `learning-roadmap.md`. | 🟡 Important |
| **TEC-05** | Quy ước Đặt tên File | Đặt tên file tùy tiện, sai chuẩn. | Đặt tên nhất quán: `notes/0X-tên-chặng/week-NN-tên-chủ-đề.md`, `assessments/quizzes/week-NN-quiz.md`, `assessments/answer-keys/week-NN-answers.md`. | Kiểm tra cây thư mục. | 🟡 Important |
| **TEC-06** | Định dạng Trích dẫn APA | Trích dẫn nguồn lộn xộn, thiếu cấu trúc. | Danh mục tài liệu tham khảo cuối bài tuân thủ cấu trúc chuẩn APA 7th edition, có kèm link hoặc DOI. | Soát format trích dẫn cuối bài. | 🟢 Minor |
| **TEC-07** | Báo cáo Audit Đầy đủ | Nộp bài review thiếu bằng chứng checklist. | Bản nộp review ghi rõ trạng thái của từng tiêu chí (PASS / FAIL / N/A kèm lý do cụ thể). | Soát gói bàn giao review của agent. | 🔴 Critical |

---

### Nhóm 6: Chống Manh mối Hình thức & Chất lượng Trắc nghiệm (Anti-Cue & Quiz Rigor — 8 tiêu chí)

| Mã | Tên tiêu chí | Định nghĩa vi phạm | Điều kiện PASS (Chuẩn bắt buộc) | Phương pháp kiểm tra | Severity |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **CUE-01** | Cân bằng Độ dài & Định dạng | Đáp án đúng dài/ngắn vượt trội hoặc format khác biệt. | Cả 4 phương án A, B, C, D trong cùng câu có độ dài tương đương (độ lệch ký tự giữa dài nhất và ngắn nhất $\le 25\%$). Cấm đáp án đúng luôn dài nhất. | Đếm số ký tự/từ của từng phương án. | 🟡 Important |
| **CUE-02** | Cân xứng Thuật ngữ Song ngữ | Đáp án đúng là lựa chọn duy nhất có thuật ngữ tiếng Anh. | Thuật ngữ tiếng Anh gốc trong ngoặc đơn nếu có phải nằm trên dẫn đề (stem) hoặc xuất hiện đồng đều ở cả 4 lựa chọn A, B, C, D. | Quét ngoặc đơn tiếng Anh trong các phương án. | 🟡 Important |
| **CUE-03** | Phương án Nhiễu Hợp lý & Cùng Cấp độ | Distractor ngớ ngẩn, phi lý, lệch cấp độ khái niệm. | 100% phương án sai là hiểu lầm/ngụy biện thực tế có vẻ hợp lý và cùng cấp độ khái niệm với đáp án đúng (cùng trường phái, cùng chuyên ngành, cùng cấp bậc). | Thẩm định nội dung sư phạm từng distractor. | 🟡 Important |
| **CUE-04** | Chống Stem-Keyword Echo | Từ khóa đặc trưng của stem lặp lại đơn độc ở đáp án đúng. | Không có từ khóa hiếm/đặc trưng nào từ câu hỏi chỉ xuất hiện duy nhất ở đáp án đúng để người học bắt từ khóa đoán mò. | So khớp từ khóa giữa stem và 4 lựa chọn. | 🟡 Important |
| **CUE-05** | Chống Mẫu "Dài & Thận trọng Nhất" | Đáp án đúng vừa dài nhất vừa dùng ngôn ngữ thận trọng đối lập cực đoan. | Tuyệt đối không để xảy ra mô hình kép: đáp án đúng thận trọng ("có xu hướng", "có thể") trong khi cả 3 đáp án sai cực đoan ("luôn luôn", "chắc chắn 100%"). | Rà soát tương quan giữa độ dài và giọng văn. | 🔴 Critical |
| **CUE-06** | Đánh giá Độc lập Mù Manh mối (Blind Cue Review) | Người đọc chỉ nhìn 4 lựa chọn (không đọc stem) đoán được đáp án. | Reviewer độc lập đọc 4 lựa chọn mà không đọc câu hỏi; tỷ lệ nhận diện đáp án đúng không được vượt trội xác suất ngẫu nhiên (25%). | Thử nghiệm Blind Cue Review độc lập. | 🔴 Critical |
| **CUE-07** | Ma trận Phủ Khái niệm (Concept Coverage) | Chỉ đo LO chung chung, bỏ sót các cụm khái niệm cốt lõi. | Có ma trận đối chiếu câu hỏi kiểm tra phủ đủ 100% các cụm khái niệm thành phần chi tiết của tuần (Tuần 01: 12 cụm khái niệm). | Đối chiếu bảng Concept Coverage Matrix. | 🟡 Important |
| **CUE-08** | Cách ly Siêu dữ liệu Khỏi Người học | Lộ Bloom level, LO tag, ma trận, section labels trên view quiz. | File quiz của người học chỉ hiển thị mã câu `Q[NN]-XX`; toàn bộ Bloom, LO, ma trận, tỷ lệ phần trăm được chuyển sang Answer Key và Audit. | Soát file `assessments/quizzes/week-NN-quiz.md`. | 🔴 Critical |

---

## 3. Quy trình Quyết định Kết luận Audit (Verdict Decision Rules)

Khi tiến hành Review chất lượng cho một tuần học hoặc một đợt bàn giao:

```text
[Kiểm tra 6 Nhóm Checklist (51 tiêu chí)]
        │
        ├──► Có bất kỳ lỗi CRITICAL nào? ───────► KẾT LUẬN: CHANGES REQUESTED / BLOCKED
        │
        ├──► Có lỗi IMPORTANT chưa sửa? ────────► KẾT LUẬN: CHANGES REQUESTED
        │
        ├──► Không thể xác minh nguồn/xung đột? ─► KẾT LUẬN: BLOCKED (Hỏi Người dùng)
        │
        └──► 100% tiêu chí PASS hoặc N/A hợp lệ, 
             chỉ còn Minor không ảnh hưởng? ───► KẾT LUẬN: APPROVED
```

---

## 4. Definition of Done Phiên bản 1 (DoD v1.0 — Hiệu chỉnh 2026-09-09)

Một tuần học chỉ được đóng (Closed / Complete) khi thỏa mãn trọn vẹn 8 điều kiện tiên quyết:
1. **Bài học hoàn tất:** Đủ 12 section H2, đạt chuẩn khối lượng từ hai tầng (Fast-Track 600–900 từ, Deep Dive 2.000–3.500 từ), không bịa đặt nguồn, không vi phạm an toàn.
2. **Quiz 12 câu sạch metadata:** Đủ 12 câu, interleave thứ tự, sạch hoàn toàn Bloom/LO/matrix trên giao diện người học, phân bổ đáp án cân bằng (3A, 3B, 3C, 3D).
3. **Pass 100% Cue Audit:** 12/12 câu vượt qua kiểm toán mù (Blind Cue Review), không có câu nào lộ đáp án qua độ dài, format, song ngữ hay ngôn ngữ thận trọng.
4. **Phủ trọn vẹn Concept Matrix:** 12/12 cụm khái niệm cốt lõi đều được đo lường trực tiếp.
5. **Answer Key hoàn chỉnh:** Bảng Quick Matrix 12 câu có cột Concept Tag; giải thích sâu sắc lý do đúng và lý do sai của từng distractor; link heading bài học Unicode chuẩn xác; 100% Claim ID truy xuất được trong ledger.
6. **Báo cáo Audit minh bạch:** Ghi nhận đủ 51 tiêu chí (43 cũ + 8 CUE); đính chính mọi sai lệch lịch sử.
7. **Template & Quy ước chuẩn hóa:** Áp dụng thống nhất slug anchor Unicode tiếng Việt giữ dấu và chuẩn cách ly metadata.
8. **Người học nghiệm thu:** Được Người học trực tiếp làm thử và xác nhận APPROVED tại Checkpoint tương ứng.
