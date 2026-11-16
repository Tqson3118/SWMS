# Đáp Án & Hướng Dẫn Học Lại Tuần 03: Tương quan, Nhân quả, Độ tin cậy, Tái lập và Cách đọc một Tuyên bố Khoa học

> **Khóa học Tự học Tâm lý học 48 tuần**  
> **Chặng 1: Nền tảng khoa học** (Tuần 01 – Tuần 08)  
> **Bài kiểm tra liên kết:** [Bài kiểm tra trắc nghiệm Tuần 03](../quizzes/week-03-quiz.md)  
> **Bài học liên kết:** [Bài đọc Tuần 03: Tương quan, Nhân quả, Độ tin cậy và Tái lập](../../notes/01-scientific-foundations/week-03-correlation-causation.md)  
> **Mục đích tài liệu:** Cung cấp đáp án chuẩn xác, phân tích chi tiết bản chất đúng/sai của từng phương án, chỉ ra bẫy tư duy thống kê và điều hướng vị trí ôn tập cụ thể trong bài đọc.

---

## Bảng Tra Cứu Nhanh Đáp Án (Quick Answer Matrix)

| Câu hỏi | Mã mục tiêu (LO) | Cấp độ nhận thức | Cụm khái niệm (Concept Cluster) | Đáp án | Vị trí bài học cần ôn tập | Claim ID & Nguồn |
| :---: | :---: | :---: | :--- | :---: | :--- | :--- |
| `Q03-01` | `[LO-W03-01]` | Hiểu / Phân tích | Cụm 01: Hệ số tương quan Pearson | **D** | [Mục 4.1. Toán học của Hệ số Tương quan](../../notes/01-scientific-foundations/week-03-correlation-causation.md#41-toán-học-của-hệ-số-tương-quan-ý-nghĩa-của-r-đám-mây-điểm-và-ranh-giới-tuyến-tính) | `[C03-01]`, `[C03-02]` (Spielman et al., 2020) |
| `Q03-02` | `[LO-W03-01]` | Phân tích / So sánh | Cụm 02: Độ mạnh vs Chiều hướng | **D** | [Mục 4.1. Toán học của Hệ số Tương quan](../../notes/01-scientific-foundations/week-03-correlation-causation.md#41-toán-học-của-hệ-số-tương-quan-ý-nghĩa-của-r-đám-mây-điểm-và-ranh-giới-tuyến-tính) | `[C03-01]`, `[C03-02]` (Spielman et al., 2020) |
| `Q03-03` | `[LO-W03-02]` | Áp dụng / Phân tích | Cụm 03: Tương quan vs Nhân quả & Biến thứ ba | **C** | [Mục 4.2. Hai Cạm bẫy Phương pháp luận](../../notes/01-scientific-foundations/week-03-correlation-causation.md#42-hai-cạm-bẫy-phương-pháp-luận-khiến-tương-quan-không-thể-suy-ra-nhân-quả) | `[C03-03]` (OpenStax, 2020; Bernal et al., 2017; VanderWeele, 2019) |
| `Q03-04` | `[LO-W03-02]` | Áp dụng / Phân tích | Cụm 04: Vấn đề chiều hướng | **C** | [Mục 4.2. Hai Cạm bẫy Phương pháp luận](../../notes/01-scientific-foundations/week-03-correlation-causation.md#42-hai-cạm-bẫy-phương-pháp-luận-khiến-tương-quan-không-thể-suy-ra-nhân-quả) | `[C03-03]` (Spielman et al., 2020) |
| `Q03-05` | `[LO-W03-02]` | Hiểu / Phân tích | Cụm 05: Tương quan ảo tưởng & Thiên kiến xác nhận | **B** | [Mục 4.2. Hai Cạm bẫy Phương pháp luận](../../notes/01-scientific-foundations/week-03-correlation-causation.md#42-hai-cạm-bẫy-phương-pháp-luận-khiến-tương-quan-không-thể-suy-ra-nhân-quả) | `[C03-04]` (Spielman et al., 2020) |
| `Q03-06` | `[LO-W03-03]` | Áp dụng / Phân tích | Cụm 06: Độ tin cậy vs Độ giá trị | **B** | [Mục 4.3. Thẩm định Đo lường Tâm lý học](../../notes/01-scientific-foundations/week-03-correlation-causation.md#43-thẩm-định-đo-lường-tâm-lý-học-độ-tin-cậy-vs-độ-giá-trị) | `[C03-05]`, `[C03-06]` (Spielman et al., 2020) |
| `Q03-07` | `[LO-W03-03]` | Áp dụng / Đánh giá | Cụm 07: Mối quan hệ biện chứng Tin cậy - Giá trị | **A** | [Mục 4.3. Thẩm định Đo lường Tâm lý học](../../notes/01-scientific-foundations/week-03-correlation-causation.md#43-thẩm-định-đo-lường-tâm-lý-học-độ-tin-cậy-vs-độ-giá-trị) | `[C03-05]`, `[C03-06]` (Spielman et al., 2020) |
| `Q03-08` | `[LO-W03-04]` | Hiểu / Đánh giá | Cụm 08: Ý nghĩa p-value & Tuyên bố ASA | **A** | [Mục 5.1. Tuyên bố ASA về p-values](../../notes/01-scientific-foundations/week-03-correlation-causation.md#51-tuyên-bố-tiêu-biểu-1-tuyên-bố-của-hiệp-hội-thống-kê-hoa-kỳ-asa-về-p-values-và-kích-thước-hiệu-ứng-c03-07-mạnh) | `[C03-07]` (Wasserstein & Lazar, 2016) |
| `Q03-09` | `[LO-W03-04]` | Nhớ / Hiểu | Cụm 09: Khủng hoảng tái lập & OSC 2015 | **A** | [Mục 5.2. Đại Dự án Tái lập Tâm lý học](../../notes/01-scientific-foundations/week-03-correlation-causation.md#52-nghiên-cứu-tiêu-biểu-2-đại-dự-án-tái-lập-tâm-lý-học-và-phong-trào-khoa-học-mở-c03-08-khá) | `[C03-08]` (Open Science Collaboration, 2015) |
| `Q03-10` | `[LO-W03-04]` | Hiểu / Áp dụng | Cụm 10: Khoa học Mở & Đăng ký trước | **B** | [Mục 5.2. Đại Dự án Tái lập Tâm lý học](../../notes/01-scientific-foundations/week-03-correlation-causation.md#52-nghiên-cứu-tiêu-biểu-2-đại-dự-án-tái-lập-tâm-lý-học-và-phong-trào-khoa-học-mở-c03-08-khá) | `[C03-08]` (Nosek et al., 2018) |
| `Q03-11` | `[LO-W03-01]` | Đánh giá | Cụm 11: Giới hạn mô hình và dự đoán cá nhân | **C** | [Mục 4.1. Toán học của Hệ số Tương quan](../../notes/01-scientific-foundations/week-03-correlation-causation.md#41-toán-học-của-hệ-số-tương-quan-ý-nghĩa-của-r-đám-mây-điểm-và-ranh-giới-tuyến-tính) | `[C03-01]`, `[C03-02]` (Spielman et al., 2020) |
| `Q03-12` | `[LO-W03-03]` | Đánh giá | Cụm 12: Giới hạn alpha và cấu trúc thang đo | **D** | [Mục 4.3. Thẩm định Đo lường Tâm lý học](../../notes/01-scientific-foundations/week-03-correlation-causation.md#43-thẩm-định-đo-lường-tâm-lý-học-độ-tin-cậy-vs-độ-giá-trị) | `[C03-05]`, `[C03-06]` (Tavakol & Dennick, 2011; OpenStax, 2020) |

---

## Hướng Dẫn Giải Chi Tiết & Phân Tích Sư Phạm

### Câu `Q03-01`

- **Đáp án đúng:** **D — $X$ tăng đi cùng $Y$ giảm; liên hệ khá mạnh.**
- **Vì sao đáp án này đúng:** Hệ số tương quan Pearson $r=-0.76$ có hai thành tố: dấu âm ($-$) chỉ ra mối tương quan nghịch (một biến tăng thì biến kia có xu hướng giảm), và độ lớn tuyệt đối $|-0.76|=0.76$ cho thấy liên hệ tuyến tính khá mạnh trong dữ liệu này. Các mốc diễn giải như $|r|\ge0.50$ chỉ là heuristic phụ thuộc lĩnh vực, thang đo và mẫu, không phải phân loại phổ quát.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án A SAI:** A đảo chiều liên hệ; stem cho dấu âm, không phải cùng chiều.
  - ❌ **Phương án B SAI:** B giữ chiều đúng nhưng hạ độ mạnh xuống yếu, trái với độ lớn khá mạnh trong stem.
  - ❌ **Phương án C SAI:** C đảo chiều và đồng thời hạ độ mạnh, trái với cả hai đặc điểm của stem.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-01]` Hệ số tương quan Pearson — Cụm 01.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 4.1. Toán học của Hệ số Tương quan](../../notes/01-scientific-foundations/week-03-correlation-causation.md#41-toán-học-của-hệ-số-tương-quan-ý-nghĩa-của-r-đám-mây-điểm-và-ranh-giới-tuyến-tính).
- **Căn cứ khoa học:** `[C03-01]`, `[C03-02]` — Spielman et al. (2020), Section 2.3.

---

### Câu `Q03-02`

- **Đáp án đúng:** **D — Báo cáo độ mạnh: liên hệ thứ nhất yếu hơn liên hệ thứ hai; đây là liên hệ quan sát, không tự suy ra nhân quả.**
- **Vì sao đáp án này đúng:** Khi đánh giá độ mạnh của mối quan hệ tuyến tính, chúng ta so sánh giá trị tuyệt đối $|r|$. Ở đây, $|r_2| = |-0.72| = 0.72$, lớn hơn nhiều so với $|r_1| = |+0.35| = 0.35$. Vì vậy, liên hệ tuyến tính thứ hai trong mẫu mạnh hơn; cả hai vẫn là liên hệ quan sát, không tự đảm bảo dự báo chính xác cho từng cá nhân hay xác lập nhân quả.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án C SAI:** C xếp đúng thứ tự độ mạnh nhưng đổi dấu của $r_2$ thành dương; stem ghi $r_2=-0.72$, nên profile dấu của C không khớp dữ kiện. Giới hạn quan sát trong C đúng nhưng không sửa lỗi này.
  - ❌ **Phương án B SAI:** B nói hai liên hệ ngang nhau, trái với việc $|r_2|=.72$ lớn hơn $|r_1|=.35$.
  - ❌ **Phương án A SAI:** A đảo thứ tự độ mạnh; liên hệ thứ nhất không mạnh hơn liên hệ thứ hai.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-01]` Độ mạnh vs Chiều hướng — Cụm 02.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 4.1. Toán học của Hệ số Tương quan](../../notes/01-scientific-foundations/week-03-correlation-causation.md#41-toán-học-của-hệ-số-tương-quan-ý-nghĩa-của-r-đám-mây-điểm-và-ranh-giới-tuyến-tính).
- **Căn cứ khoa học:** `[C03-01]`, `[C03-02]` — Spielman et al. (2020), Section 2.3.

---

### Câu `Q03-03`

- **Đáp án đúng:** **C — Nếu chênh lệch giảm nhưng còn dư, yếu tố đo có thể góp phần vào liên hệ.**
- **Vì sao đáp án này đúng:** Liên hệ giảm rõ nhưng còn dư sau khi điều chỉnh tập thể dục và hút thuốc, phù hợp với việc các yếu tố đó góp phần vào liên hệ. Phần còn lại có thể do yếu tố khác hoặc sai số; nghiên cứu quan sát chưa chứng minh trà xanh gây giảm bệnh tim.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án A SAI:** Stem không cho chênh lệch tăng; A sẽ phù hợp với một bộ dữ liệu mà điều chỉnh làm lộ liên hệ mạnh hơn.
  - ❌ **Phương án B SAI:** Stem không cho chênh lệch giữ nguyên; B sẽ phù hợp với một bộ dữ liệu mà điều chỉnh hầu như không đổi liên hệ.
  - ❌ **Phương án D SAI:** Stem cho chênh lệch còn dư; D chỉ phù hợp nếu chênh lệch về 0, và ngay cả khi đó cũng không chứng minh nhân quả.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-02]` Tương quan vs Nhân quả & Biến thứ ba — Cụm 03.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 4.2. Hai Cạm bẫy Phương pháp luận](../../notes/01-scientific-foundations/week-03-correlation-causation.md#42-hai-cạm-bẫy-phương-pháp-luận-khiến-tương-quan-không-thể-suy-ra-nhân-quả).
- **Căn cứ khoa học:** `[C03-03]` — OpenStax Psychology 2e, Section 2.3; Bernal, Cummins, & Gasparrini (2017) for bounded adjustment/ITS cautions; VanderWeele (2019) for confounder-selection limits. Myers et al. (2024) is not used as a verified support source here.

---

### Câu `Q03-04`

- **Đáp án đúng:** **C — Nếu dự báo thuận trội hơn dự báo ngược, dữ liệu này ủng hộ chiều thuận nhưng chưa chứng minh nhân quả.**
- **Vì sao đáp án này đúng:** Tự ti T1 dự báo cô lập T2 sau khi tính đến cô lập ban đầu, còn chiều ngược yếu hơn; vì vậy hồ sơ cho thấy dự báo thuận trội hơn dự báo ngược. Thiết kế vẫn quan sát và còn khả năng nhiễu chưa đo, nên C giữ đúng giới hạn: không tự chứng minh nhân quả.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án A SAI:** Stem không cho đường ngược trội; A sẽ phù hợp nếu hệ số ngược lớn hơn trong một bộ dữ liệu khác.
  - ❌ **Phương án B SAI:** Stem không cho hai đường ngang nhau; B sẽ phù hợp nếu hai hệ số chéo trễ tương đương.
  - ❌ **Phương án D SAI:** Stem cho đường thuận còn lại; D sẽ phù hợp nếu cả hai đường mất sau kiểm soát, và vẫn không chứng minh nhân quả.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-02]` Vấn đề chiều hướng — Cụm 04.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 4.2. Hai Cạm bẫy Phương pháp luận](../../notes/01-scientific-foundations/week-03-correlation-causation.md#42-hai-cạm-bẫy-phương-pháp-luận-khiến-tương-quan-không-thể-suy-ra-nhân-quả).
- **Căn cứ khoa học:** `[C03-03]` — Spielman et al. (2020), Section 2.3.

---

### Câu `Q03-05`

- **Đáp án đúng:** **B — Hồ sơ không khác biệt; y tá chỉ chọn đêm có ca và bỏ qua đêm yên tĩnh.**
- **Vì sao đáp án này đúng:** Hồ sơ 10 năm không cho thấy khác biệt, nhưng y tá chỉ chọn các đêm phù hợp với niềm tin và bỏ qua các đêm yên tĩnh. Mẫu quan sát này mô tả tương quan ảo tưởng được củng cố bởi thiên kiến xác nhận, chứ không phải một quy luật từ dữ liệu.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án C SAI:** Stem mô tả bỏ qua các đêm yên tĩnh, còn C nói y tá có đối chiếu thêm chúng.
  - ❌ **Phương án A SAI:** Hồ sơ 10 năm không cho thấy số ca cao hơn.
  - ❌ **Phương án D SAI:** Stem mô tả việc chọn có xác nhận các đêm nổi bật và bỏ qua đêm yên tĩnh, không phải kiểm tra toàn bộ các tầng.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-02]` Tương quan ảo tưởng & Thiên kiến xác nhận — Cụm 05.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 4.2. Hai Cạm bẫy Phương pháp luận](../../notes/01-scientific-foundations/week-03-correlation-causation.md#42-hai-cạm-bẫy-phương-pháp-luận-khiến-tương-quan-không-thể-suy-ra-nhân-quả).
- **Căn cứ khoa học:** `[C03-04]` — Spielman et al. (2020), Section 2.3.

---

### Câu `Q03-06`

- **Đáp án đúng:** **B — Đồng thuận cao giữa người chấm; liên hệ với chỉ báo tiêu chuẩn yếu.**
- **Vì sao đáp án này đúng:** Ba chuyên gia cho điểm gần như giống nhau, cung cấp bằng chứng về độ tin cậy giữa người chấm. Tương quan yếu với chỉ báo hành vi đã định trước lại hạn chế bằng chứng về độ giá trị tiêu chuẩn; hai loại bằng chứng này trả lời câu hỏi khác nhau.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án A SAI:** Stem cho đồng thuận cao, không thấp.
  - ❌ **Phương án C SAI:** Stem cho liên hệ tiêu chuẩn yếu, không mạnh.
  - ❌ **Phương án D SAI:** Stem cho đồng thuận cao và liên hệ yếu, không phải cả hai thấp.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-03]` Độ tin cậy vs Độ giá trị — Cụm 06.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 4.3. Thẩm định Đo lường Tâm lý học](../../notes/01-scientific-foundations/week-03-correlation-causation.md#43-thẩm-định-đo-lường-tâm-lý-học-độ-tin-cậy-vs-độ-giá-trị).
- **Căn cứ khoa học:** `[C03-05]`, `[C03-06]` — Spielman et al. (2020), Section 2.3.

---

### Câu `Q03-07`

- **Đáp án đúng:** **A — Test-retest cao, lab mạnh, nhật ký yếu.**
- **Vì sao đáp án này đúng:** Điểm ổn định qua ba tuần cung cấp bằng chứng test-retest; tương quan mạnh với bài kiểm tra trong phòng thí nghiệm là bằng chứng giá trị trong bối cảnh đó. Tương quan yếu với nhật ký đời sống cho thấy không nên mở rộng kết luận sang hành vi tự nhiên mà chưa có bằng chứng giá trị sinh thái.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án B SAI:** Stem cho test-retest cao và nhật ký yếu, trái với cả hai điểm của B.
  - ❌ **Phương án C SAI:** Stem cho lab mạnh và nhật ký yếu, trái với C.
  - ❌ **Phương án D SAI:** Stem cho test-retest cao và lab mạnh, không phải hai kết quả thấp.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-03]` Mối quan hệ biện chứng Tin cậy - Giá trị — Cụm 07.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 4.3. Thẩm định Đo lường Tâm lý học](../../notes/01-scientific-foundations/week-03-correlation-causation.md#43-thẩm-định-đo-lường-tâm-lý-học-độ-tin-cậy-vs-độ-giá-trị).
- **Căn cứ khoa học:** `[C03-05]`, `[C03-06]` — Spielman et al. (2020), Section 2.3.

---

### Câu `Q03-08`
- **Đáp án đúng:** **A — Xác suất $T$ không lớn hơn $t_{obs}$ dưới $H_0$ và quy trình đã định.**
- **Vì sao đáp án này đúng:** Với giả thuyết định hướng là can thiệp làm giảm điểm số, p-value dùng đuôi trái của phân phối $T$. Theo ASA, đây là xác suất, giả sử $H_0$ và quy trình đã định đúng, quan sát được thống kê nhỏ bằng hoặc nhỏ hơn trị số đã thấy; không phải xác suất $H_0$ đúng hay kết quả tương lai.
  - ❌ **Phương án B SAI:** B dùng trị tuyệt đối và gộp hai đuôi, không theo kiểm định giảm một phía.
  - ❌ **Phương án C SAI:** C lấy vùng $T$ không nhỏ hơn trị quan sát, ngược hướng giả thuyết giảm điểm.
  - ❌ **Phương án D SAI:** D lấy vùng trị tuyệt đối không lớn hơn trị quan sát, không phải tail event theo hướng đã định.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-04]` Ý nghĩa p-value & Tuyên bố ASA — Cụm 08.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 3.1. Tóm lược Cốt lõi](../../notes/01-scientific-foundations/week-03-correlation-causation.md#31-tóm-lược-cốt-lõi-core-summary), rồi [Mục 5.1. Tuyên bố ASA về p-values](../../notes/01-scientific-foundations/week-03-correlation-causation.md#51-tuyên-bố-tiêu-biểu-1-tuyên-bố-của-hiệp-hội-thống-kê-hoa-kỳ-asa-về-p-values-và-kích-thước-hiệu-ứng-c03-07-mạnh).
- **Căn cứ khoa học:** `[C03-07]` — Wasserstein & Lazar (2016) cho diễn giải p-value; Illowsky & Dean (2023), *Introductory Statistics 2e*, Sections 9.3 và 9.5 cho ký hiệu phân phối, giả định và xác định đuôi kiểm định (không dùng công thức t cụ thể).

---

### Câu `Q03-09`

- **Đáp án đúng:** **A — Mục tiêu ý nghĩa thống kê: 36% đạt tiêu chí; hiệu ứng trung bình giảm `.403` xuống `.197`.**
- **Vì sao đáp án này đúng:** OSC (2015) tái lập 100 nghiên cứu với 270 tác giả/cộng tác viên. Báo cáo nhiều tiêu chí: 36% đạt ý nghĩa thống kê, hiệu ứng trung bình giảm từ $r=.403$ xuống $r=.197$, 39% được đánh giá chủ quan là tái lập, và 47% hiệu ứng gốc nằm trong khoảng tin cậy 95% của hiệu ứng tái lập. Đây là các chỉ báo khác nhau trong mẫu dự án, không phải một tỷ lệ phổ quát cho mọi lĩnh vực hay mọi thất bại tái lập.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án B SAI:** B sẽ đúng nếu stem hỏi tỷ lệ hiệu ứng gốc nằm trong khoảng tin cậy, nhưng stem hỏi tỷ lệ tái lập đạt ý nghĩa thống kê.
  - ❌ **Phương án C SAI:** C sẽ đúng nếu stem hỏi đánh giá chủ quan, nhưng không trả lời tiêu chí ý nghĩa thống kê được nêu.
  - ❌ **Phương án D SAI:** 97% là tỷ lệ đạt ý nghĩa thống kê ở nghiên cứu gốc; D sẽ phù hợp với câu hỏi về nghiên cứu gốc, không phải các lần tái lập.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-04]` Khủng hoảng tái lập & OSC 2015 — Cụm 09.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 5.2. Đại Dự án Tái lập Tâm lý học](../../notes/01-scientific-foundations/week-03-correlation-causation.md#52-nghiên-cứu-tiêu-biểu-2-đại-dự-án-tái-lập-tâm-lý-học-và-phong-trào-khoa-học-mở-c03-08-khá).
- **Căn cứ khoa học:** `[C03-08]` — Open Science Collaboration (2015).

---

### Câu `Q03-10`

- **Đáp án đúng:** **B — Nếu 1 khóa trước thu thập, 2 thêm sau khi thấy dữ liệu, lần lượt xác nhận rồi khám phá.**
- **Vì sao đáp án này đúng:** Việc đăng ký trước dữ liệu xác định kế hoạch xác nhận. Phân tích mới được thêm sau khi thấy dữ liệu cần được gắn nhãn khám phá, dù có thể tạo giả thuyết hữu ích; B gắn đúng hai nhãn theo trình tự thời gian.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án A SAI:** A sẽ phù hợp nếu cả hai phân tích đã khóa trước thu thập; stem nói phân tích 2 thêm sau khi thấy kết quả.
  - ❌ **Phương án C SAI:** C sẽ phù hợp nếu cả hai được thêm sau khi thấy dữ liệu; stem nói phân tích 1 đã khóa trước.
  - ❌ **Phương án D SAI:** D sẽ phù hợp nếu phân tích 2 được sửa trước khi thấy kết quả; stem đặt việc thêm nó sau khi dữ liệu đã mở.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-04]` Khoa học Mở & Đăng ký trước — Cụm 10.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 5.2. Đại Dự án Tái lập Tâm lý học](../../notes/01-scientific-foundations/week-03-correlation-causation.md#52-nghiên-cứu-tiêu-biểu-2-đại-dự-án-tái-lập-tâm-lý-học-và-phong-trào-khoa-học-mở-c03-08-khá).
- **Căn cứ khoa học:** `[C03-08]` — Nosek et al. (2018).

---

### Câu `Q03-11`

- **Đáp án đúng:** **C — Nếu có điểm ảnh hưởng, phần dư cong, khoảng rộng, cần kiểm tra giả định và bất định trước dự đoán cá nhân.**
- **Vì sao đáp án này đúng:** $r=.70$ mô tả liên hệ trong mẫu, nhưng hai điểm ảnh hưởng, phần dư cong và khoảng dự đoán rộng cảnh báo về giả định mô hình và độ chính xác khi dự đoán cá nhân. C khớp cả ba dấu hiệu và giữ cùng giới hạn suy luận.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án A SAI:** Stem ghi nhận điểm ảnh hưởng, phần dư cong và khoảng rộng, nên không khớp với hồ sơ A.
  - ❌ **Phương án B SAI:** Stem có phần dư cong và khoảng rộng, không phải phần dư thẳng và khoảng hẹp của B.
  - ❌ **Phương án D SAI:** Stem có điểm ảnh hưởng, trong khi D giả định không có; D chỉ khớp với hai chẩn đoán còn lại.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-01]` Giới hạn mô hình và dự đoán cá nhân — Cụm 11.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 4.1. Toán học của Hệ số Tương quan](../../notes/01-scientific-foundations/week-03-correlation-causation.md#41-toán-học-của-hệ-số-tương-quan-ý-nghĩa-của-r-đám-mây-điểm-và-ranh-giới-tuyến-tính).
- **Căn cứ khoa học:** `[C03-01]`, `[C03-02]` — Spielman et al. (2020), Section 2.3.

---

### Câu `Q03-12`

- **Đáp án đúng:** **D — Nếu alpha .82 và phân tích nhân tố phù hợp hai nhân tố được báo cáo, hai bằng chứng này được ghi nhận; tiêu chí độc lập vẫn chưa được xác nhận.**
- **Vì sao đáp án này đúng:** Alpha cung cấp bằng chứng về nhất quán nội bộ, còn phân tích nhân tố cung cấp bằng chứng cấu trúc trong mẫu và mô hình đã chọn; hai kết quả trả lời các câu hỏi khác nhau và không tự chứng minh giá trị tiêu chí. Stem báo cáo đúng hai bằng chứng này, nhưng chưa kiểm tra chỉ báo tiêu chí độc lập, nên D là profile phù hợp và vẫn thận trọng.
- **Phân tích vì sao TỪNG phương án khác SAI:**
  - ❌ **Phương án A SAI:** A thay phân tích nhân tố bằng liên hệ tiêu chí; stem chưa kiểm tra tiêu chí và có bằng chứng cấu trúc thay thế.
  - ❌ **Phương án B SAI:** B thay phân tích nhân tố bằng ổn định theo thời gian; stem không báo cáo lần đo lặp lại và vẫn có bằng chứng cấu trúc.
  - ❌ **Phương án C SAI:** C đặt liên hệ tiêu chí cùng phân tích nhân tố nhưng bỏ qua alpha; stem có alpha và chưa kiểm tra tiêu chí.
- **Mục tiêu học tập đo lường & Cụm khái niệm:** `[LO-W03-03]` Giới hạn alpha và cấu trúc thang đo — Cụm 12.
- **Vị trí bài học để ôn tập:** [Bài đọc Tuần 03 — Mục 4.3. Thẩm định Đo lường Tâm lý học](../../notes/01-scientific-foundations/week-03-correlation-causation.md#43-thẩm-định-đo-lường-tâm-lý-học-độ-tin-cậy-vs-độ-giá-trị).
- **Căn cứ khoa học:** `[C03-05]`, `[C03-06]` — Tavakol & Dennick (2011), *Making sense of Cronbach’s alpha*, pp. 53–54 (alpha, dimensionality, factor analysis); OpenStax Psychology 2e, Section 2.3 (reliability/validity distinction).
