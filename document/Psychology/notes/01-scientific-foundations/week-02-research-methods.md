# Tuần 02: Phương pháp Nghiên cứu — Giả thuyết, Biến số, Chọn mẫu, Đo lường và Đạo đức (Research Methods)

> **Khóa học Tự học Tâm lý học 48 tuần**  
> **Chặng 1: Nền tảng khoa học** (Tuần 01 – Tuần 08)  
> **Mã bài học:** `W02` | **Trạng thái:** Đã hoàn thành  
> **Thời lượng học tập:**  
> - ⚡ **Tầng Cốt Lõi (Fast Track):** 30–45 phút (Đọc tóm lược, 5 takeaways, bảng so sánh, làm 2 mini-reviews)  
> - 🔬 **Tầng Đọc Sâu (Deep Dive):** 2–3 giờ hoặc hơn (Đọc toàn văn bài học, phân tích thiết kế thực nghiệm, phản biện ngụy khoa học, phân tích kịch bản đạo đức)  
> **Kiến thức tiên quyết (Prerequisites):** Tuần 01 — Tâm lý học là gì: phạm vi, lịch sử và các trường phái

> [!NOTE]
> **Ghi chú hiển thị Markdown:** Các khối `<details>` (gợi ý trả lời của mini-review) phụ thuộc trình xem Markdown. Trên GitHub và hầu hết các trình xem hiện đại, thẻ này hỗ trợ đóng/mở tương tác.

---

## 1. Ý nghĩa & Bối cảnh Thực tế (Why It Matters)

Hàng ngày, truyền thông và các nền tảng mạng xã hội ngập tràn những tiêu đề giật gân: *"Nghiên cứu mới chứng minh thói quen dậy sớm tăng 50% chỉ số hạnh phúc"*, hay *"Khảo sát cho thấy sử dụng mạng xã hội là nguyên nhân hủy hoại sự tập trung của giới trẻ"*. Đứng trước những thông tin như vậy, trực giác thông thường thường thúc đẩy chúng ta hoặc tin ngay nếu thông điệp phù hợp với định kiến sẵn có, hoặc hoài nghi gạt bỏ nếu nó trái ngược với trải nghiệm cá nhân. 

Tuy nhiên, tư duy khoa học không vận hành dựa trên cảm tính hay giai thoại cá nhân. Để biết một khẳng định tâm lý có đáng tin cậy hay không, chúng ta bắt buộc phải xem xét cách mà tri thức đó được tạo ra: Nhà nghiên cứu đã thu thập dữ liệu bằng phương pháp nào? Họ đã đo lường khái niệm trừu tượng như "hạnh phúc" hay "tập trung" bằng công cụ gì? Họ có kiểm soát được các yếu tố gây nhiễu bên ngoài không? Và quan trọng nhất, quy trình nghiên cứu đó có tôn trọng nhân phẩm và bảo vệ quyền lợi của con người tham gia không?

Phương pháp nghiên cứu chính là bộ quy tắc nhận thức luận giúp tâm lý học phân định giữa chân lý thực nghiệm và những ngụy biện ngụy khoa học `[C02-01]`. Tuần học này trang bị cho bạn hệ thống công cụ phương pháp luận nền tảng: từ cách hình thành một giả thuyết có tính khả bác, cách thao tác hóa khái niệm trừu tượng thành biến số đo lường được, đến các thiết kế nghiên cứu từ mô tả, cắt ngang, theo chiều dọc cho tới thực nghiệm đối chứng ngẫu nhiên — phương pháp cho phép suy luận nhân quả mạnh nhất khi các giả định thiết kế được đảm bảo. Cuối cùng, bạn sẽ thấu hiểu các nguyên tắc đạo đức chung bảo vệ con người trong nghiên cứu tâm lý học đương đại; quy định, cơ chế thẩm duyệt và ngoại lệ cụ thể thay đổi theo quốc gia và cơ sở.

> [!NOTE]
> **Thông điệp định hướng:** Một tuyên bố khoa học chỉ có giá trị khi phương pháp tạo ra nó vững chắc; nghiên cứu tâm lý học không tìm kiếm sự chắc chắn tuyệt đối mà tìm kiếm bằng chứng thực nghiệm có thể kiểm chứng, có thể bác bỏ và được bảo vệ bởi lương tri đạo đức.

---

## 2. Mục tiêu Học tập Đo lường được (Measurable Learning Objectives)

Sau khi hoàn thành bài học này, bạn sẽ có khả năng:

- **`[LO-W02-01]` (Nhớ/Hiểu):** **Định nghĩa và phân biệt** chu trình suy luận quy nạp — diễn dịch, tiêu chuẩn tính khả bác (falsifiability) của Karl Popper, và vai trò cốt lõi của định nghĩa thao tác (operational definition) đối với khả năng tái lập nghiên cứu.
- **`[LO-W02-02]` (Phân tích):** **So sánh và phân tích** ưu thế cùng giới hạn phương pháp luận của các thiết kế nghiên cứu mô tả (nghiên cứu trường hợp, quan sát tự nhiên, khảo sát) và thiết kế thời gian (cắt ngang vs theo chiều dọc, bao gồm hiệu ứng thế hệ và rụng mẫu).
- **`[LO-W02-03]` (Áp dụng/Phân tích):** **Nhận diện và thiết kế** các thành tố của một nghiên cứu thực nghiệm chuẩn tắc: biến độc lập (IV), biến phụ thuộc (DV), phân bổ ngẫu nhiên (random assignment) đối lập chọn mẫu ngẫu nhiên (random sampling), và kỹ thuật kiểm soát thiên lệch (mù đơn, mù đôi, giả dược).
- **`[LO-W02-04]` (Đánh giá):** **Đánh giá các vi phạm và tiêu chuẩn đạo đức** nghiên cứu trên người: vai trò của Hội đồng IRB, nguyên tắc đồng thuận sau khi được thông tin (informed consent), quy định đánh lạc hướng (deception) và giải thích sau nghiên cứu (debriefing), cùng ba nguyên tắc nền tảng của Báo cáo Belmont.

100% mục tiêu trên được đo lường trực tiếp thông qua [Bài kiểm tra trắc nghiệm Tuần 02](../../assessments/quizzes/week-02-quiz.md).

---

## 3. Học nhanh khi mệt (Fast-Track Core — 30–45 phút)

Trục tư duy phương pháp luận cốt lõi: **Tuyên bố (Claim) $\rightarrow$ Cách đo (Measurement) $\rightarrow$ Thiết kế (Design) $\rightarrow$ Giới hạn (Limits) $\rightarrow$ Đạo đức (Ethics)**.

### 3.1. 5 Khái niệm Cốt lõi
1. **Tuyên bố có thể kiểm chứng được (Falsifiability):** Khác với niềm tin, một giả thuyết khoa học phải chỉ rõ kết quả thực tế nào sẽ chứng minh nó sai nếu nó thực sự sai (Karl Popper). Không thể kiểm chứng một khẳng định luôn tự nhận mình đúng trong mọi trường hợp `[C02-01]`.
2. **Định nghĩa thao tác (Operational Definition):** Khái niệm tâm lý trừu tượng (như "chất lượng giấc ngủ", "stress") phải được chuyển thành chỉ số đo lường cụ thể (điểm thang đo PSQI, thời gian vào giấc ngủ tính bằng phút, nồng độ cortisol) để người khác có thể kiểm tra và tái lập độc lập `[C02-02]`.
3. **Tương quan không đồng nghĩa với Nhân quả (Correlation $\neq$ Causation):** Hai hiện tượng xảy ra cùng nhau không chứng minh A gây ra B. Có thể B gây ra A, hoặc một biến thứ ba (Confounding variable) gây ra cả hai `[C02-10]`.
4. **Thực nghiệm đối chứng ngẫu nhiên (RCT):** Chuẩn vàng để tìm nguyên nhân: Nhà nghiên cứu chủ động thay đổi **Biến độc lập (IV)** và đo lường sự thay đổi ở **Biến phụ thuộc (DV)**; sử dụng **Phân bổ ngẫu nhiên (Random Assignment)** để các biến ngoại lai có xu hướng cân bằng giữa các nhóm ở mức kỳ vọng; một mẫu cụ thể vẫn có thể lệch `[C02-05, C02-06]`.
5. **Đạo đức nghiên cứu:** Luôn đặt phẩm giá con người lên trên hết: quy tắc thông thường là người tham gia tự nguyện đồng thuận sau khi được cung cấp đầy đủ thông tin (**Informed Consent**); nếu nghiên cứu dùng đánh lạc hướng (**deception**) thì cần **Debriefing** phù hợp. Một số ngoại lệ pháp lý hoặc thể chế có điều kiện riêng và không biến thành quy tắc áp dụng cho mọi nghiên cứu `[C02-08, C02-09]`.

### 3.2. Ví dụ Xuyên suốt Có Lời giải (Worked Example): Lướt Mạng Xã Hội và Giấc Ngủ
Hãy xem một ví dụ giả định để luyện cách nhà tâm lý học trả lời câu hỏi: *"Dùng điện thoại trước khi ngủ có làm sinh viên ngủ kém hơn không?"*

- **Bước 1 — Đặt giả thuyết khả bác:**  
  *"Sinh viên lướt mạng xã hội trên 45 phút ngay trước khi ngủ sẽ có thời gian đi vào giấc ngủ lâu hơn so với sinh viên đọc sách giấy hoặc không dùng màn hình."* (Nếu kết quả đo cho thấy thời gian vào giấc ngủ của hai nhóm ngang nhau, giả thuyết bị bác bỏ).
- **Bước 2 — Thao tác hóa (Operationalize):**  
  - *Biến nguyên nhân (IV):* Thời lượng sử dụng màn hình (0 phút vs. 45 phút mạng xã hội).
  - *Biến kết quả (DV):* Độ trễ đi vào giấc ngủ (Sleep Onset Latency - tính bằng phút, ghi nhận qua thiết bị đeo theo dõi nhịp tim/giấc ngủ).
- **Bước 3 — Chọn thiết kế và nhận diện giới hạn:**
  - *Thiết kế khảo sát (Survey / Correlational):* Giả sử hỏi 500 sinh viên về thói quen và giấc ngủ và quan sát thấy mối liên hệ $r = 0.35$; con số này chỉ là dữ liệu minh họa, không phải kết quả nghiên cứu thực tế.  
    $\rightarrow$ *Giới hạn:* Chưa kết luận được điện thoại gây khó ngủ; có thể những bạn đang lo âu vì thi cử (biến thứ 3) vừa khó ngủ vừa lướt mạng để giết thời gian.
  - *Thiết kế thực nghiệm (Experiment):* Giả sử mời 80 tình nguyện viên vào phòng thí nghiệm giấc ngủ và **phân bổ ngẫu nhiên** vào Nhóm A (đọc sách giấy 45 phút trước khi tắt đèn) và Nhóm B (lướt mạng xã hội 45 phút). Phân bổ ngẫu nhiên làm các đặc điểm như lo âu hay thói quen trước đó có xu hướng cân bằng ở mức kỳ vọng; một mẫu cụ thể vẫn có thể lệch.  
    $\rightarrow$ Nếu trong ví dụ giả định Nhóm B mất trung bình 38 phút mới ngủ được so với 14 phút của Nhóm A, chênh lệch này có thể cung cấp bằng chứng phù hợp với tác động trong điều kiện thiết kế; vẫn cần xem xét đo lường, tuân thủ, cỡ mẫu, độ bất định và khả năng khái quát.
- **Bước 4 — Đạo đức:** Tình nguyện viên được quyền dừng tham gia bất kỳ lúc nào, không bị ép thức đêm quá mức, và dữ liệu giấc ngủ được bảo mật ẩn danh hoàn toàn.

### 3.3. 5 Điểm cốt lõi cần nhớ (5 Key Takeaways)
1. **Giả thuyết phải có tính khả bác:** Khẳng định không thể bị chứng minh sai trong thực tế thì không thuộc về khoa học thực nghiệm `[C02-01]`.
2. **Khái niệm trừu tượng cần được thao tác hóa:** Định nghĩa thao tác rõ ràng là điều kiện thiết yếu để đo lường và tái lập độc lập `[C02-02]`.
3. **Thực nghiệm đối chứng là chuẩn vàng nhân quả:** Thao túng IV kết hợp phân bổ ngẫu nhiên cho phép suy luận nhân quả trong phạm vi giả định thiết kế `[C02-05]`.
4. **Phân bổ ngẫu nhiên khác chọn mẫu ngẫu nhiên:** Chọn mẫu phục vụ tính đại diện dân số; phân bổ ngẫu nhiên phục vụ cân bằng biến nhiễu giữa các nhóm `[C02-06]`.
5. **Đạo đức nghiên cứu là lằn ranh đỏ:** Tri thức khoa học không bao giờ được đánh đổi bằng sự an toàn, tự do và phẩm giá con người `[C02-08, C02-09]`.

---

## 4. Bài đọc sâu (Deep Dive)

### 4.1. Bản chất nhận thức luận của nghiên cứu khoa học: Chu trình suy luận, Tính khả bác và Thao tác hóa khái niệm

Khoa học không phải là một tập hợp các chân lý bất biến được khắc trên bia đá, mà là một **tiến trình nhận thức luận tự sửa sai (self-correcting process)** liên tục. Tiến trình này vận hành thông qua một chu trình khép kín kết hợp giữa hai phương thức tư duy logic bổ trợ nhau: suy luận quy nạp (inductive reasoning) và suy luận diễn dịch (deductive reasoning) `[C02-01]` 🟢 **[Mạnh]**.

```text
       ┌────────────────────────────────────────────────────────┐
       │                   LÝ THUYẾT (Theory)                   │
       │    (Hệ thống các giả định giải thích hiện tượng)      │
       └───────────────────┬────────────────────────────────────┘
                           │
                 Suy luận DIỄN DỊCH (Deductive)
             Từ nguyên lý chung đưa ra dự đoán cụ thể
                           │
                           ▼
       ┌────────────────────────────────────────────────────────┐
       │                 GIẢ THUYẾT (Hypothesis)                │
       │           (Kiểm chứng được & Có tính khả bác)          │
       └───────────────────┬────────────────────────────────────┘
                           │
                     THỰC NGHIỆM & QUAN SÁT
                  Thu thập dữ liệu thực tế khách quan
                           │
                           ▼
       ┌────────────────────────────────────────────────────────┐
       │                   DỮ LIỆU (Empirical Data)             │
       │              (Số liệu định lượng & Định tính)          │
       └───────────────────┬────────────────────────────────────┘
                           │
                 Suy luận QUY NẠP (Inductive)
             Từ các dữ kiện cụ thể đúc kết quy luật mới
                           │
                           └────────────────────────────────────┘
```

Trong chu trình này, **Lý thuyết (Theory)** là một hệ thống các khái niệm và nguyên lý đã được phát triển nhằm giải thích một tập hợp các hiện tượng quan sát được. Từ lý thuyết tổng quát, nhà nghiên cứu áp dụng suy luận diễn dịch để đưa ra một **Giả thuyết (Hypothesis)** — một lời dự đoán cụ thể, kiểm chứng được về những gì sẽ xảy ra trong các điều kiện thực nghiệm xác định `[C02-01]`.

Tuy nhiên, làm thế nào nhận diện một giả thuyết có thể được kiểm tra thay vì một khẳng định luôn tự nhận mình đúng? Karl Popper (1963) gọi **tính khả bác (falsifiability)** là một tiêu chí triết học để thảo luận ranh giới giữa khoa học thực nghiệm và ngụy khoa học `[C02-01]`. Trong phạm vi bài học, giả thuyết cần được phát biểu sao cho có một quan sát hoặc kết quả thực nghiệm có thể cho thấy nó sai nếu nó sai; đây là điều kiện thiết kế để kiểm tra, không phải tuyên bố rằng một tiêu chí duy nhất đủ để phân định mọi khoa học theo mệnh đề “khi và chỉ khi”. Nếu một lý thuyết được cấu trúc tinh vi đến mức mọi kết quả xảy ra đều được diễn giải là "đúng như dự đoán", thì nó không tạo ra dự đoán có thể kiểm tra bằng dữ liệu `[C01-04, C02-01]`.

Để một giả thuyết khả bác có thể kiểm chứng được trong thế giới thực, tâm lý học đối mặt với thách thức nhận thức luận: làm thế nào để đo lường các khái niệm trừu tượng không nhìn thấy được? Lời giải đáp nằm ở **Định nghĩa thao tác (Operational Definition)** `[C02-02]` 🟢 **[Mạnh]**. Định nghĩa thao tác là bản đặc tả chi tiết, chính xác các quy trình, thao tác và thang đo cụ thể mà nhà nghiên cứu sử dụng để xác định và định lượng một biến số. Ví dụ:
- Khái niệm trừu tượng: *"Mức độ căng thẳng (Stress)"*.
- Định nghĩa thao tác: Có thể là (1) Điểm số trên Thang đo Căng thẳng Cảm nhận (PSS-10), (2) Nồng độ hormone cortisol đo trong nước bọt vào lúc 8h sáng tính bằng $\mu\text{g/dL}$, hoặc (3) Mức tăng nhịp tim khi thực hiện bài kiểm tra tính nhẩm áp lực thời gian.

Nếu không có định nghĩa thao tác tường minh, cộng đồng khoa học không thể biết chính xác điều gì đã được nghiên cứu, và các nhà nghiên cứu độc lập khác hoàn toàn không thể thực hiện **tái lập (replication)** — trụ cột cốt lõi của tính khách quan khoa học `[C02-02]`.

### 4.2. Bản đồ Thiết kế Nghiên cứu: Từ Mô tả Tự nhiên đến Thực nghiệm Đối chứng Ngẫu nhiên

Khi tiếp cận một câu hỏi khoa học, nhà nghiên cứu tâm lý học phải lựa chọn một thiết kế nghiên cứu phù hợp với mục tiêu, tính khả thi và đạo đức `[C02-03, C02-04, C02-05]`:

#### 1. Các Thiết kế Nghiên cứu Mô tả (Descriptive Research)
- **Nghiên cứu trường hợp (Case Study):** Tập trung đào sâu vào một cá nhân hoặc một nhóm nhỏ người có hoàn cảnh đặc biệt hiếm có. Một nghiên cứu trường hợp kinh điển mô tả H.M. (Henry Molaison), người sau một phẫu thuật vùng thái dương–trung gian để điều trị động kinh bị suy giảm nghiêm trọng khả năng hình thành ký ức khai báo mới, trong khi một số khả năng khác vẫn được bảo tồn. Nghiên cứu trường hợp mang lại dữ liệu phong phú, chi tiết mà một thực nghiệm có thể không tạo ra vì lý do đạo đức. Tuy nhiên, giới hạn lớn của nó là **khó khái quát hóa (generalizability)** sang dân số chung vì cá nhân nghiên cứu là trường hợp đặc biệt `[C02-03]` 🟢 **[Mạnh]**.
- **Quan sát tự nhiên (Naturalistic Observation):** Quan sát và ghi nhận hành vi của đối tượng trong môi trường sống tự nhiên của họ mà không có sự can thiệp từ người nghiên cứu (ví dụ: quan sát cách trẻ em chia sẻ đồ chơi trên sân trường). Điểm mạnh lớn nhất là **độ giá trị sinh thái (ecological validity)** cao — hành vi diễn ra chân thực, không gượng gạo. Giới hạn là nhà nghiên cứu thụ động trong việc can thiệp, nên khó kiểm soát nhiều biến ngoại lai xuất hiện bất ngờ, và nguy cơ mắc **thiên lệch người quan sát (observer bias)** nếu người ghi nhận vô thức chỉ chú ý những hành vi phù hợp với giả thuyết của họ `[C02-03]` 🟢 **[Mạnh]**.
- **Khảo sát (Survey):** Sử dụng bảng hỏi hoặc phỏng vấn có cấu trúc để thu thập dữ liệu tự báo cáo trên các mẫu nghiên cứu lớn. Khảo sát cho phép thu thập lượng dữ liệu khổng lồ trong thời gian ngắn với chi phí thấp. Tuy nhiên, nó chịu giới hạn nặng nề bởi **tính thiếu chính xác của tự báo cáo**: con người thường xuyên nhớ sai, hiểu nhầm câu hỏi, hoặc cố tình che giấu sự thật để tạo hình ảnh tích cực trước xã hội (**thiên lệch mong muốn xã hội / social desirability bias**) `[C02-03]` 🟢 **[Mạnh]**.

#### 2. Thiết kế Nghiên cứu theo Thời gian: Cắt ngang vs Theo chiều dọc
- **Nghiên cứu cắt ngang (Cross-sectional Research):** So sánh các nhóm người ở các lứa tuổi khác nhau tại cùng một thời điểm duy nhất (ví dụ: so sánh khả năng ghi nhớ của nhóm 20 tuổi, 40 tuổi và 60 tuổi vào năm 2026). Phương pháp này tiết kiệm chi phí và có kết quả ngay. Nhưng nhược điểm chí mạng là **Hiệu ứng thế hệ (Cohort effect)**: sự khác biệt quan sát được giữa người 20 tuổi và 60 tuổi có thể không bắt nguồn từ quá trình lão hóa sinh học tự nhiên, mà phản ánh sự khác biệt về bối cảnh văn hóa, chế độ dinh dưỡng, giáo dục và tiếp cận công nghệ của thế hệ mà họ sinh ra `[C02-04]` 🟢 **[Mạnh]**.
- **Nghiên cứu theo chiều dọc (Longitudinal Research):** Theo dõi và kiểm tra lặp đi lặp lại cùng một nhóm người qua nhiều mốc thời gian trong đời (ví dụ: theo dõi một nhóm trẻ từ lúc 5 tuổi đến khi 50 tuổi). Đây là thiết kế hữu ích để quan sát quỹ đạo phát triển thực tế và có thể giúp giảm một số vấn đề do khác biệt thế hệ, nhưng không tự động loại bỏ mọi biến gây nhiễu. Chi phí nghiên cứu có thể rất cao, đòi hỏi cam kết lâu dài, và đối mặt với nguy cơ **rụng mẫu (attrition)** nghiêm trọng — người tham gia chuyển chỗ ở, từ chối tiếp tục, hoặc qua đời, dẫn đến việc mẫu nghiên cứu cuối cùng có thể sai lệch so với ban đầu `[C02-04]` 🟢 **[Mạnh]**.

#### 3. Thiết kế Thực nghiệm (Experimental Design) — Chuẩn vàng của Quan hệ Nhân quả
Nếu các phương pháp mô tả và tương quan chỉ cho phép chúng ta trả lời câu hỏi *"Hiện tượng gì đang xảy ra?"* và *"Hai yếu tố có biến thiên cùng nhau không?"*, thì thiết kế thực nghiệm là con đường chính để suy luận về **Quan hệ nhân quả (Cause-and-effect relationship)** `[C02-10]` 🟢 **[Mạnh]**.

Cấu trúc logic của một thực nghiệm khoa học gồm:
1. **Biến độc lập (IV - Independent Variable):** Yếu tố mà nhà nghiên cứu chủ động thao túng, điều chỉnh hoặc kiểm soát nhằm tạo ra tác động (nguyên nhân giả định).
2. **Biến phụ thuộc (DV - Dependent Variable):** Yếu tố được đo lường để xem nó có bị biến đổi bởi biến độc lập hay không (kết quả quan sát).
3. **Nhóm thực nghiệm (Experimental group):** Nhóm đối tượng nhận sự can thiệp hoặc tiếp xúc với biến độc lập.
4. **Nhóm đối chứng (Control group):** Nhóm đối tượng được giữ các điều kiện liên quan tương tự nhóm thực nghiệm nhưng không nhận can thiệp, hoặc chỉ nhận **giả dược (placebo)** — một chất vô hại hoặc can thiệp giả mô phỏng điều kiện thực nghiệm để kiểm soát yếu tố tâm lý kỳ vọng `[C02-05, C02-07]`.

### 4.3. Cơ chế Kiểm soát Thực nghiệm: Phân bổ Ngẫu nhiên và Kỹ thuật Mù

Tại sao thực nghiệm thường có sức mạnh hơn trong suy luận nhân quả? Một phần câu trả lời nằm ở khả năng giảm ảnh hưởng của các **Biến gây nhiễu (Confounding Variables)** — những biến số ngoại lai không nằm trong dự tính của nhà nghiên cứu nhưng lại làm biến thiên biến phụ thuộc, tạo ra kết luận sai lệch `[C02-06]` 🟢 **[Mạnh]**. Mức độ kiểm soát thực tế vẫn phụ thuộc vào thiết kế, đo lường và tuân thủ.

Công cụ mạnh nhất để đánh bại biến gây nhiễu chính là **Phân bổ ngẫu nhiên (Random Assignment)** `[C02-06]`. Cần phân biệt rành mạch hai khái niệm dễ nhầm lẫn:
- **Chọn mẫu ngẫu nhiên (Random Sampling):** Phương pháp tuyển chọn các cá nhân từ dân số tổng thể vào mẫu nghiên cứu sao cho mỗi người trong dân số đều có xác suất được chọn ngang nhau. Mục đích: hỗ trợ tính đại diện của mẫu khi khung mẫu và tỷ lệ đáp ứng phù hợp, để đánh giá khả năng khái quát hóa ra cộng đồng.
- **Phân bổ ngẫu nhiên (Random Assignment):** Quy trình phân chia những người ĐÃ CÓ trong mẫu vào nhóm thực nghiệm hoặc nhóm đối chứng hoàn toàn ngẫu nhiên (bằng bốc thăm, tung đồng xu, hoặc thuật toán sinh số ngẫu nhiên của máy tính). Mục đích: làm các đặc tính cá nhân (như trí thông minh, tính cách, động lực, di truyền, tiền sử bệnh tật) có xu hướng cân bằng giữa hai nhóm ở mức kỳ vọng trước can thiệp; một mẫu cụ thể vẫn có thể lệch. Nếu các giả định thiết kế, đo lường và tuân thủ được đáp ứng, khác biệt ở biến phụ thuộc có thể hỗ trợ suy luận về biến độc lập, nhưng không tự động loại bỏ mọi bất định hay tạo ra kết luận phổ quát `[C02-06]`.

```text
               ┌────────────────────────────────────────────────────────┐
               │              MẪU NGHIÊN CỨU (Sample)                   │
               └──────────────────────────┬─────────────────────────────┘
                                          │
                                PHÂN BỔ NGẪU NHIÊN
                                (Random Assignment)
                                          │
                    ┌─────────────────────┴─────────────────────┐
                    ▼                                           ▼
       ┌─────────────────────────┐                 ┌─────────────────────────┐
       │   NHÓM THỰC NGHIỆM      │                 │     NHÓM ĐỐI CHỨNG      │
       │   (Experimental Group)  │                 │     (Control Group)     │
       └────────────┬────────────┘                 └────────────┬────────────┘
                    │                                           │
         Tiếp nhận Can thiệp (IV)                    Nhận Can thiệp Giả/Chuẩn
                    │                                           │
                    ▼                                           ▼
       ┌─────────────────────────┐                 ┌─────────────────────────┐
       │ Đo Biến Phụ Thuộc (DV)  │                 │ Đo Biến Phụ Thuộc (DV)  │
       └────────────┬────────────┘                 └────────────┬────────────┘
                    │                                           │
                    └─────────────────────┬─────────────────────┘
                                          │
                               SO SÁNH THỐNG KÊ
               Khác biệt phù hợp với giả thuyết = bằng chứng hỗ trợ suy luận nhân quả trong phạm vi thiết kế
```

Bên cạnh biến gây nhiễu về đặc tính cá nhân, thực nghiệm còn phải đối phó với hai nguồn thiên lệch tâm lý tinh vi:
1. **Hiệu ứng kỳ vọng của người tham gia (Placebo Effect):** Hiện tượng niềm tin hoặc sự kỳ vọng của chính người tham gia làm thay đổi hành vi hoặc sinh lý của họ, chứ không phải do tác dụng sinh hóa thực tế của can thiệp.
2. **Thiên lệch của người làm thí nghiệm (Experimenter Bias):** Xu hướng vô thức của nhà nghiên cứu cư xử khác biệt hoặc giải thích kết quả theo hướng thiên vị nhằm xác nhận giả thuyết mà họ kỳ vọng `[C02-07]` 🟢 **[Mạnh]**.

Để giảm hai nguồn thiên lệch kỳ vọng này, khoa học sử dụng các cấp độ "làm mù":
- **Thiết kế mù đơn (Single-blind study):** Người tham gia không biết mình đang ở nhóm thực nghiệm hay nhóm đối chứng (nhận thuốc thật hay thuốc giả), giúp giảm ảnh hưởng của kỳ vọng của đối tượng.
- **Thiết kế mù đôi (Double-blind study):** Cả người tham gia LẪN nhà nghiên cứu tương tác trực tiếp thu thập dữ liệu đều KHÔNG BIẾT ai thuộc nhóm nào (thông tin chỉ được mã hóa bởi một bên thứ ba độc lập và chỉ mở mã sau khi phân tích dữ liệu hoàn tất). Cách làm này có thể giảm đồng thời hai nguồn kỳ vọng trong phạm vi thiết kế; không tự động loại bỏ mọi thiên lệch `[C02-07]` 🟢 **[Mạnh]**.

---

## 5. Nghiên cứu & Mô hình Tiêu biểu (Key Studies & Theoretical Models)

<a id="51-nghiên-cứu-1-đạo-đức-nghiên-cứu-và-sự-ra-đời-của-báo-cáo-belmont-c02-08-c02-09-mạnh"></a>
### 5.1. Nghiên cứu 1: Đạo đức Nghiên cứu và Sự Ra Đời của Báo cáo Belmont `[C02-08, C02-09]` 🟢 **[Mạnh]**

- **Tác giả & Bối cảnh Lịch sử:** Ủy ban Quốc gia về Bảo vệ Đối tượng Con người trong Nghiên cứu Y sinh và Hành vi (National Commission for the Protection of Human Subjects of Biomedical and Behavioral Research, 1979), *Báo cáo Belmont: Các nguyên tắc và hướng dẫn đạo đức để bảo vệ đối tượng con người trong nghiên cứu* (Belmont Report).
- **Mục tiêu & Nguồn cơn ra đời:** Được hình thành trong bối cảnh phản ứng xã hội và thể chế trước các vi phạm nghiên cứu, trong đó có Nghiên cứu Giang mai Tuskegee (1932–1972). Theo FAQ của CDC, nhóm giang mai gồm 399 nam giới đã mắc bệnh khi được tuyển; 201 người trong nhóm đối chứng không mắc bệnh. Không có bằng chứng về việc các nhà nghiên cứu cố ý lây nhiễm cho họ. CDC cũng ghi nhận nghiên cứu không thu thập đồng thuận có hiểu biết và không cung cấp điều trị sẵn có, kể cả sau khi penicillin phổ biến `[C02-09]`.
- **Ba Nguyên tắc Đạo đức Cốt lõi của Báo cáo Belmont:**
  1. **Tôn trọng con người (Respect for Persons):** Thừa nhận quyền tự chủ của cá nhân. Mọi đối tượng có quyền tự quyết định tham gia hay không sau khi được cung cấp đầy đủ thông tin; những người bị suy giảm quyền tự chủ (trẻ em, người thiểu năng trí tuệ, tù nhân) phải được hưởng sự bảo vệ đặc biệt tăng cường `[C02-08, C02-09]`.
  2. **Lòng hướng thiện (Beneficence):** Tối đa hóa lợi ích tiềm năng và hạn chế tối đa mọi tổn hại hoặc rủi ro đối với người tham gia (nguyên tắc "Trước hết không làm hại" — *First, do no harm*). Nhà nghiên cứu phải liên tục đánh giá tỷ lệ rủi ro/lợi ích `[C02-09]`.
  3. **Công bằng (Justice):** Sự công bằng trong việc phân bổ gánh nặng và thụ hưởng thành quả nghiên cứu. Không được bóc lột các nhóm yếu thế, dễ bị tổn thương (người nghèo, người thiểu số) để chịu gánh nặng rủi ro cho những lợi ích phục vụ nhóm giàu có hơn `[C02-09]`.
- **Ý nghĩa & giới hạn thể chế hóa:** Báo cáo Belmont nêu các nguyên tắc nền tảng cho bảo vệ người tham gia và góp phần định hình thảo luận, chính sách và quy định đạo đức sau đó; không nên diễn đạt rằng báo cáo tự mình tạo ra IRB trên toàn thế giới. Trong ví dụ pháp lý của Hoa Kỳ, 45 CFR 46 yêu cầu hoặc cho phép các quy trình IRB và ngoại lệ cụ thể tùy điều khoản; §46.116(f)(3) không coi rủi ro tối thiểu đơn độc là đủ cho miễn/điều chỉnh đồng thuận, còn §46.117(c) nói riêng về miễn tài liệu hóa. Các quốc gia và cơ sở khác có thể có luật, cơ chế thẩm duyệt và ngoại lệ khác nhau `[C02-08]`.

### 5.2. Nghiên cứu 2: Thí nghiệm Nhà tù Stanford và Cuộc Tái Đánh giá Phương pháp Luận `[C02-09]` 🔴 **[Tranh luận]**

- **Tác giả & Năm xuất bản:** Philip Zimbardo (1971), Thí nghiệm Nhà tù Stanford (Stanford Prison Experiment - SPE); Le Texier, T. (2019), *Debunking the Stanford Prison Experiment*, American Psychologist, 74(7), 823–839 `[C02-09]`.
- **Thiết kế ban đầu:** Tuyển chọn 24 nam sinh viên tâm lý khỏe mạnh, phân bổ ngẫu nhiên vào hai vai: "cai ngục" hoặc "tù nhân" trong một nhà tù giả lập dưới tầng hầm đại học Stanford, dự kiến kéo dài 2 tuần nhằm kiểm định giả thuyết tình huống môi trường định hình hành vi độc ác.
- **Vấn đề đạo đức và diễn giải:** Các tường thuật lịch sử về nghiên cứu mô tả việc nghiên cứu dừng sớm và những trải nghiệm gây distress; vai trò kép của Zimbardo đặt ra câu hỏi về giám sát và thiên lệch. Những chi tiết và mức độ diễn giải cần được đọc cùng các phê bình lưu trữ, không trình bày như phép đo khách quan đã chứng minh đầy đủ `[C02-09]`.
- **Phản biện phương pháp luận hiện đại (Le Texier, 2019):** Bài phản biện và các tường thuật lưu trữ đặt nghi vấn về cách nghiên cứu được hướng dẫn và diễn giải; trong phạm vi metadata/abstract đã kiểm tra, đây là nguồn phê bình tranh luận, không phải bằng chứng để khẳng định mọi chi tiết như đã được chứng minh:
  - *Đặc tính đòi hỏi của người thí nghiệm (Demand characteristics):* Phê bình lưu trữ cho rằng hướng dẫn và bối cảnh nghiên cứu có thể đã ảnh hưởng hành vi; không nên chuyển diễn giải này thành kết luận về bản tính người tham gia.
  - *Thiên lệch tuyển chọn:* Các phê bình đặt câu hỏi liệu cách tuyển mẫu có thu hút người khác biệt hay không; mức độ ảnh hưởng cần được giữ ở trạng thái tranh luận.
- **Bài học phương pháp luận:** SPE nên được dùng như một trường hợp lịch sử gây tranh luận về an toàn, vai trò nhà nghiên cứu và diễn giải dữ liệu; không dùng nó như bằng chứng đã chứng minh bản chất con người hoặc “mất hoàn toàn tính khách quan” `[C02-09]`.

---

## 6. Biết được gì / Chưa thể kết luận gì (What We Know vs. What Remains Uncertain)

### 6.1. Những điều khoa học đã xác lập tương đối vững chắc (What We Know)

- **Thực nghiệm là một con đường mạnh để xác lập nhân quả:** Trong điều kiện phù hợp, sự kết hợp giữa **thao túng biến độc lập** và **phân bổ ngẫu nhiên** hỗ trợ suy luận quan hệ nguyên nhân - kết quả. Các thiết kế quan sát, bán thực nghiệm và phương pháp thống kê cũng có thể cung cấp bằng chứng nhân quả khi có giả định nhận dạng và kiểm tra độ nhạy phù hợp; không có phương pháp nào tự động loại bỏ mọi bất định. Mọi kết luận vẫn phụ thuộc vào giả định thiết kế, đo lường, tuân thủ và khả năng tái lập `[C02-05, C02-06]` 🟢 **[Mạnh]**.
- **Quy chuẩn đạo đức IRB và Informed Consent là nền tảng:** Việc bảo vệ quyền tự chủ, bí mật đời tư và an toàn tâm lý là trọng tâm. Trong ví dụ quy định của Hoa Kỳ (45 CFR 46), IRB chỉ có thể xem xét miễn hoặc điều chỉnh yêu cầu đồng thuận khi đáp ứng đầy đủ các điều kiện pháp lý, không phải chỉ vì nghiên cứu có rủi ro tối thiểu: §46.116(f)(3) còn yêu cầu tính không khả thi nếu không miễn/điều chỉnh, không ảnh hưởng bất lợi đến quyền và phúc lợi, và các điều kiện liên quan; §46.117(c) là ngoại lệ về **tài liệu hóa** đồng thuận, khác với miễn chính sự đồng thuận. Đây là ví dụ pháp lý của Hoa Kỳ, không phải quy tắc phổ quát cho mọi quốc gia. Nếu dùng deception vì mục tiêu phương pháp luận chính đáng, cần debriefing sớm nhất có thể `[C02-08]` 🟢 **[Mạnh]**.
- **Hiệu ứng kỳ vọng cần được kiểm soát:** Kỳ vọng của người tham gia có thể ảnh hưởng trải nghiệm và kết quả đo; vì vậy thí nghiệm can thiệp dùng nhóm đối chứng, giả dược hoặc làm mù phù hợp để tách kỳ vọng khỏi tác động của can thiệp `[C02-07]` 🟢 **[Mạnh]**.

### 6.2. Những giới hạn & Điều khoa học chưa thể kết luận (What Remains Uncertain)

- **Sự đánh đổi giữa Giá trị Nội tại (Internal Validity) và Giá trị Sinh thái (Ecological Validity):** Các thí nghiệm kiểm soát cực kỳ nghiêm ngặt trong phòng lab có thể cung cấp bằng chứng nhân quả mạnh trong phạm vi thiết kế và mẫu nghiên cứu, nhưng hành vi con người trong môi trường nhân tạo với điện cực dán trên đầu có thể khác với cách họ cư xử tự nhiên ngoài đời thực.
- **Ranh giới đạo đức của nghiên cứu thực nghiệm:** Rất nhiều câu hỏi tâm lý học hệ trọng nhất (ví dụ: tác động của sang chấn thời thơ ấu, bạo lực gia đình, hoặc sự cô lập xã hội cực đoan lên não bộ) **không thể thực nghiệm hóa** trên người trong điều kiện đạo đức chấp nhận được. Khoa học khi đó phải dựa vào dữ liệu tương quan hoặc nghiên cứu hồi cứu với các giới hạn rõ ràng.
- **Nguy cơ rụng mẫu có tính chọn lọc (Selective Attrition):** Trong các nghiên cứu theo chiều dọc kéo dài nhiều thập kỷ, những người bỏ cuộc thường không rụng ngẫu nhiên (ví dụ: những người gặp khủng hoảng tài chính, trầm cảm nặng hoặc nghiện ngập thường dễ bỏ cuộc hơn), dẫn đến việc nhóm còn lại đến cuối cùng bị méo mó theo hướng khỏe mạnh hơn thực tế.

---

## 7. Hiểu lầm phổ biến & Phản biện Pop Psychology (Common Myths & Pop Psych Debunked)

### ❌ Hiểu lầm 1: "Một thí nghiệm khoa học được công bố chứng minh lý thuyết đó đúng 100% vĩnh viễn"
- **Nguồn gốc hiểu lầm:** Xuất phát từ cách giật tít truyền thông giật gân: *"Khoa học đã chứng minh..."*. Công chúng thường lầm tưởng khoa học hoạt động theo cơ chế tìm kiếm "bằng chứng chứng minh chân lý tuyệt đối".
- **Bằng chứng khoa học thực tế:** Theo triết học khoa học của Karl Popper, dữ liệu thực nghiệm **không bao giờ có thể 'chứng minh' vĩnh viễn** một lý thuyết là đúng tuyệt đối, bởi vì chúng ta không thể quan sát toàn bộ mọi trường hợp trong tương lai `[C02-01]`. Dữ liệu chỉ có thể: (1) Bác bỏ một giả thuyết (khi kết quả đi ngược lại dự đoán), hoặc (2) **Ủng hộ / Không thể bác bỏ** giả thuyết đó ở thời điểm hiện tại. Một lý thuyết vững chắc đơn giản là một lý thuyết đã vượt qua hàng trăm cuộc kiểm tra khả bác khắt khe mà chưa bị đánh bại `[C02-01]` 🟢 **[Mạnh]**.
- **Cách tư duy đúng:** Tiếp cận tri thức khoa học với tinh thần khiêm nhường nhận thức (intellectual humility): luôn coi các phát hiện là tri thức tốt nhất hiện có trong phạm vi điều kiện kiểm tra, sẵn sàng cập nhật khi có bằng chứng mới vững chắc hơn.

### ❌ Hiểu lầm 2: "Cỡ mẫu (Sample Size) càng lớn thì nghiên cứu càng đáng tin và chắc chắn đúng"
- **Nguồn gốc hiểu lầm:** Cảm giác trực giác cho rằng khảo sát 100.000 người trên mạng internet chắc chắn có giá trị hơn một thí nghiệm chỉ làm trên 100 người trong phòng lab.
- **Bằng chứng khoa học thực tế:** Cỡ mẫu lớn không thể cứu vãn một phương pháp luận sai sót. Nếu phương pháp lấy mẫu bị thiên lệch (ví dụ: một cuộc thăm dò trực tuyến tự chọn 100.000 người trên một trang báo điện tử), mẫu đó có thể phản ánh quá mức ý kiến của những người đọc trang báo đó và không nhất thiết đại diện cho dân số chung `[C02-03, C02-06]`. Hơn nữa, khảo sát dù có rất nhiều người tham gia vẫn không tự nó chứng minh quan hệ nhân quả nếu thiếu thiết kế và giả định phù hợp. Khi mục tiêu là suy luận nhân quả, một thực nghiệm ngẫu nhiên có đối chứng được thiết kế và thực hiện tốt có thể cung cấp bằng chứng mạnh hơn một khảo sát tự nguyện; không thể gán một hệ số “cao hơn gấp bội” chung cho mọi so sánh `[C02-05, C02-06]` 🟢 **[Mạnh]**.
- **Cách tư duy đúng:** Đánh giá chất lượng của dữ liệu dựa trên **phương pháp chọn mẫu và mức độ kiểm soát thiên lệch**, chứ không chỉ nhìn vào số lượng người tham gia.

---

## 8. Ôn tập Ngắn Giữa tuần (Mid-Week Mini-Reviews)

### 8.1. Mini-Review 1 (Giữa tuần — Ngày 3)

1. **Câu hỏi 1:** Tiêu chuẩn tính khả bác (falsifiability) của Karl Popper yêu cầu điều gì ở một giả thuyết khoa học?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Giả thuyết khoa học bắt buộc phải được phát biểu sao cho về mặt lý thuyết, tồn tại một quan sát thực nghiệm cụ thể có thể chứng minh nó là sai nếu nó thực sự sai. Nếu một giả thuyết không thể bị bác bỏ bởi bất kỳ kết quả thực tế nào, nó không thuộc phạm trù khoa học thực nghiệm `[C02-01]`.
   </details>

2. **Câu hỏi 2:** Sự khác biệt cốt lõi giữa "Chọn mẫu ngẫu nhiên" (Random Sampling) và "Phân bổ ngẫu nhiên" (Random Assignment) là gì?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Chọn mẫu ngẫu nhiên là cách chọn người từ dân số vào mẫu nghiên cứu (phục vụ mục tiêu đại diện và khái quát hóa). Phân bổ ngẫu nhiên là chia những người trong mẫu vào nhóm thực nghiệm hoặc nhóm đối chứng (phục vụ mục tiêu giảm/cân bằng các biến gây nhiễu cá nhân ở mức kỳ vọng để hỗ trợ suy luận nhân quả) `[C02-06]`.
   </details>

3. **Câu hỏi 3:** Tại sao định nghĩa thao tác (operational definition) lại là điều kiện tiên quyết cho việc tái lập nghiên cứu?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Vì các khái niệm tâm lý vốn trừu tượng. Định nghĩa thao tác mô tả chính xác quy trình đo lường hoặc thao túng biến số bằng các hành động cụ thể, giúp các nhà khoa học độc lập khác có thể làm lại y hệt thí nghiệm để kiểm chứng độ tin cậy `[C02-02]`.
   </details>

---

### 8.2. Mini-Review 2 (Cuối tuần — Ngày 5)

1. **Câu hỏi 1:** Trong một thử nghiệm thuốc giải lo âu mới, tại sao cần thiết lập thiết kế mù đôi (double-blind) thay vì mù đơn?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Thiết kế mù đơn có thể giảm hiệu ứng kỳ vọng từ phía bệnh nhân. Thiết kế mù đôi có thể giảm đồng thời kỳ vọng của bệnh nhân và thiên lệch kỳ vọng của bác sĩ/nghiên cứu viên khi đánh giá triệu chứng trong phạm vi thiết kế; không tự động loại bỏ mọi thiên lệch `[C02-07]`.
   </details>

2. **Câu hỏi 2:** Báo cáo Belmont được ban hành dựa trên những sự kiện lịch sử nào và đề ra 3 nguyên tắc đạo đức cốt lõi nào?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Báo cáo Belmont ra đời năm 1979 sau sự phẫn nộ trước Nghiên cứu Giang mai Tuskegee. Ba nguyên tắc đạo đức cốt lõi gồm: (1) Tôn trọng con người (tự quyết, informed consent), (2) Lòng hướng thiện (tối đa hóa lợi ích, không gây hại), và (3) Công bằng (phân bổ công bằng gánh nặng và lợi ích) `[C02-08, C02-09]`.
   </details>

3. **Câu hỏi 3:** Một nghiên cứu theo dõi sự thay đổi nhận thức qua các lứa tuổi 20, 40 và 60 trong cùng năm 2026 đối mặt với nguy cơ phương pháp luận nào lớn nhất?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Đây là thiết kế cắt ngang (cross-sectional), nguy cơ lớn nhất là Hiệu ứng thế hệ (Cohort effect) — sự khác biệt có thể phản ánh bối cảnh lịch sử, công nghệ, dinh dưỡng của các thế hệ chứ không phải do quy luật phát triển nhận thức theo tuổi tác `[C02-04]`.
   </details>

---

## 9. Bài tập Tình huống Ứng dụng & Reflection Tự nguyện (Case Study & Voluntary Journal)

> [!IMPORTANT]
> **Nguyên tắc An toàn & Tự nguyện:** Hoạt động suy ngẫm và bài tập thực hành là **hoàn toàn tự nguyện**. Bạn không cần phải chia sẻ bất kỳ trải nghiệm cá nhân nhạy cảm nào. Bạn luôn được khuyến khích lựa chọn **Kịch bản Giả định (Lựa chọn B)** để rèn luyện tư duy phương pháp luận.

### Lựa chọn A: Nhật ký Tự quan sát Phương pháp luận (Personal Reflection — Tự nguyện)
- **Nội dung gợi ý:** Hãy nhớ lại một niềm tin hoặc thói quen cá nhân mà bạn từng áp dụng (ví dụ: nghe nhạc baroque khi học thì nhớ lâu hơn, hoặc uống nước chanh buổi sáng giúp giảm stress). 
- **Khung phân tích phương pháp luận:**
  1. *Niềm tin đó bắt nguồn từ đâu (kinh nghiệm bản thân, người quen khuyên, hay đọc trên mạng)?*
  2. *Định nghĩa thao tác của bạn cho "nhớ lâu" hoặc "giảm stress" là gì? Bạn đã tự đo lường như thế nào?*
  3. *Có những biến gây nhiễu nào khác (ví dụ: ngủ đủ giấc hơn, tâm trạng hôm đó vui vẻ) có thể đã tạo ra kết quả đó thay vì cốc nước chanh hay điệu nhạc?*
  4. *Nếu muốn kiểm định niềm tin đó bằng một thí nghiệm mù đơn trên chính mình, bạn sẽ thiết kế các điều kiện đối chứng như thế nào?*

---

### Lựa chọn B: Phân tích Kịch bản Giả định (Hypothetical Case Study — Khuyến nghị)

**Bối cảnh:**  
Công ty công nghệ EdTech X vừa phát triển một ứng dụng học tập mới trên điện thoại có tính năng "Flashcard AI thông minh". Để chứng minh hiệu quả phần mềm, giám đốc sản phẩm đã tiến hành một khảo sát: Gửi email cho 10.000 người dùng đã tải app, nhận lại 500 phản hồi tự nguyện. Kết quả cho thấy: *92% người trả lời khẳng định ứng dụng đã giúp họ cải thiện vượt bậc khả năng ghi nhớ từ vựng*. Dựa trên kết quả này, công ty chạy chiến dịch quảng cáo với khẩu hiệu: *"Khoa học đã chứng minh: Ứng dụng X giúp tăng 92% trí nhớ cho mọi học viên!"*.

**Nhiệm vụ thực hành của bạn:**
1. **Phát triển ít nhất 3 giả thuyết phương pháp luận thay thế:** Đưa ra 3 cách giải thích khoa học khác nhau giải thích tại sao kết quả khảo sát lại đạt con số 92% mà không nhất thiết do chất lượng thuật toán của ứng dụng:
   - *Giả thuyết 1 (Thiên lệch tự lựa chọn / Self-selection bias):* Chỉ những người thực sự thấy app hữu ích và có động lực học tập cao mới dành thời gian trả lời khảo sát, trong khi 9.500 người chán nản hoặc bỏ app đã không phản hồi.
   - *Giả thuyết 2 (Thiên lệch mong muốn xã hội & Trí nhớ tự báo cáo):* Người dùng tự đánh giá cảm tính về trí nhớ của mình mà không có bài kiểm tra năng lực khách quan trước và sau can thiệp.
   - *Giả thuyết 3 (Hiệu ứng kỳ vọng / Placebo effect):* Người dùng mới bỏ tiền mua gói học nên có tâm lý kỳ vọng cao, thúc đẩy họ chăm chỉ hơn trong tuần đầu tiên.
2. **Xác định các dữ kiện quan trọng còn thiếu:**
   - Cần một bài kiểm tra ghi nhớ từ vựng khách quan (định nghĩa thao tác chuẩn hóa), không dùng tự báo cáo cảm tính.
   - Cần thông tin về tỷ lệ rụng mẫu và kết quả của 9.500 người dùng không phản hồi email.
3. **Đề xuất một thiết kế thực nghiệm ngẫu nhiên có đối chứng (RCT) đạt chuẩn:**
   - Tuyển sinh viên tham gia, thực hiện **chọn mẫu ngẫu nhiên**.
   - Dùng **phân bổ ngẫu nhiên** chia thành 2 nhóm: Nhóm thực nghiệm dùng App X; nhóm so sánh dùng một app ghi chép thông thường với thời lượng và hướng dẫn tương đương. App ghi chép là **điều kiện so sánh chủ động**, không tự động là giả dược: tác dụng kỳ vọng, khác biệt giao diện và mức độ hấp dẫn vẫn có thể khác nhau.
   - Chỉ gọi là **mù đơn** nếu người học thực sự không thể nhận biết điều kiện được phân; với hai ứng dụng khác nhau, khả năng che giấu điều kiện phải được kiểm tra và có thể không khả thi. Nếu không che giấu được, ghi nhận nghiên cứu là không mù hoặc mù một phần.
   - Đo lường bằng bài kiểm tra từ vựng chuẩn hóa sau 30 ngày và định trước cách xử lý tuân thủ, rụng mẫu, khác biệt ban đầu và người đánh giá. Chênh lệch giữa nhóm có thể là bằng chứng phù hợp với tác động trong điều kiện thiết kế; diễn giải nhân quả còn phụ thuộc vào phân bổ, đo lường, các giả định và độ chắc chắn của kết quả, không tự động chứng minh nguyên nhân.

---

## 10. Bảng Thuật ngữ Tuần (Weekly Glossary — 10 thuật ngữ cốt lõi)

| Thuật ngữ Tiếng Việt | Thuật ngữ Tiếng Anh | Định nghĩa Khoa học Chuẩn tắc (APA / OpenStax) | Ngữ cảnh Sử dụng & Lưu ý |
| :--- | :--- | :--- | :--- |
| **Tính khả bác** | *Falsifiability* | Thuộc tính của một mệnh đề hoặc giả thuyết có khả năng bị dữ liệu quan sát hoặc thực nghiệm bác bỏ `[C02-01]`. | Tiêu chí triết học thường gắn với Karl Popper; dùng trong bài như một yêu cầu để thiết kế kiểm tra, không phải điều kiện duy nhất đủ cho mọi ranh giới khoa học. |
| **Định nghĩa thao tác** | *Operational Definition* | Bản mô tả chính xác các quy trình và thang đo cụ thể được dùng để đo lường hoặc thao túng một biến số trừu tượng `[C02-02]`. | Bắt buộc phải có để đảm bảo tính minh bạch và khả năng tái lập (replication). |
| **Biến độc lập** | *Independent Variable (IV)* | Biến số được nhà nghiên cứu chủ động thao túng hoặc kiểm soát trong thực nghiệm để quan sát tác động của nó `[C02-05]`. | Đóng vai trò là "nguyên nhân giả định" trong thiết kế thực nghiệm. |
| **Biến phụ thuộc** | *Dependent Variable (DV)* | Biến số được đo lường để xác định mức độ tác động do biến độc lập tạo ra `[C02-05]`. | Đóng vai trò là "kết quả" chịu sự biến thiên theo biến độc lập. |
| **Biến gây nhiễu** | *Confounding Variable* | Biến số ngoại lai nằm ngoài dự tính nhưng biến thiên cùng biến độc lập, làm sai lệch kết luận nhân quả `[C02-06]`. | Mục tiêu chính của phân bổ ngẫu nhiên là giảm/cân bằng ở mức kỳ vọng trong một mẫu cụ thể. |
| **Phân bổ ngẫu nhiên** | *Random Assignment* | Quy trình chia người tham gia vào các nhóm thực nghiệm hoặc đối chứng sao cho mỗi người có cơ hội vào mỗi nhóm như nhau `[C02-06]`. | Phân biệt với chọn mẫu ngẫu nhiên; giúp cân bằng biến gây nhiễu giữa các nhóm ở mức kỳ vọng, không bảo đảm tuyệt đối cho một mẫu cụ thể. |
| **Chọn mẫu ngẫu nhiên** | *Random Sampling* | Quy trình lựa chọn các cá nhân từ dân số vào mẫu nghiên cứu sao cho mọi thành viên đều có xác suất được chọn ngang nhau `[C02-06]`. | Quyết định tính đại diện và khả năng khái quát hóa kết quả sang cộng đồng. |
| **Thiết kế mù đôi** | *Double-blind Study* | Thiết kế nghiên cứu trong đó cả người tham gia và nghiên cứu viên tương tác trực tiếp đều không biết ai thuộc nhóm can thiệp hay đối chứng `[C02-07]`. | Có thể giúp kiểm soát đồng thời hiệu ứng kỳ vọng của người tham gia và thiên lệch kỳ vọng của người làm thí nghiệm trong phạm vi thiết kế; không tự động loại bỏ mọi thiên lệch. |
| **Hội đồng Đánh giá Thể chế** | *Institutional Review Board (IRB)* | Hội đồng chuyên môn độc lập có nhiệm vụ thẩm duyệt các đề cương nghiên cứu trên người nhằm đảm bảo an toàn đạo đức `[C02-08]`. | Theo chuẩn mực quốc tế hiện hành, nghiên cứu trên người thường phải được thẩm duyệt độc lập; tên gọi và thủ tục cụ thể khác nhau theo quốc gia. |
| **Giải thích tường minh** | *Debriefing* | Phiên làm việc ngay sau khi hoàn thành thử nghiệm để giải thích mục đích nghiên cứu và xóa bỏ lừa dối nếu có `[C02-08]`. | Chuẩn mực yêu cầu áp dụng khi nghiên cứu có sử dụng kỹ thuật đánh lạc hướng (deception). |

---

## 11. Danh mục Tài liệu Tham khảo (Verified References — APA 7th)

1. **Spielman, R. M., Jenkins, W. J., & Lovett, M. D.** (2020). *Psychology 2e* (2nd ed.). OpenStax, Rice University. Chapter 2: "Psychological Research". [https://openstax.org/books/psychology-2e/pages/2-introduction](https://openstax.org/books/psychology-2e/pages/2-introduction)
2. **National Commission for the Protection of Human Subjects of Biomedical and Behavioral Research.** (1979). *The Belmont Report: Ethical principles and guidelines for the protection of human subjects of research*. U.S. Department of Health, Education, and Welfare. [https://www.hhs.gov/ohrp/regulations-and-policy/belmont-report/index.html](https://www.hhs.gov/ohrp/regulations-and-policy/belmont-report/index.html)
3. **Le Texier, T.** (2019). Debunking the Stanford Prison Experiment. *American Psychologist*, 74(7), 823–839. [https://doi.org/10.1037/amp0000401](https://doi.org/10.1037/amp0000401)
4. **American Psychological Association.** (2017). *Ethical principles of psychologists and code of conduct* (with the 2016 amendments). [https://www.apa.org/ethics/code](https://www.apa.org/ethics/code)
5. **U.S. Department of Health and Human Services.** (2026, September 28). *45 CFR 46.116(f)(3), General requirements for informed consent* (U.S. regulatory example; accessed 2026-09-30). [https://www.ecfr.gov/current/title-45/subtitle-A/subchapter-A/part-46/subpart-A/section-46.116](https://www.ecfr.gov/current/title-45/subtitle-A/subchapter-A/part-46/subpart-A/section-46.116)
6. **U.S. Department of Health and Human Services.** (2026, September 28). *45 CFR 46.117(c), Documentation of informed consent* (U.S. regulatory example; accessed 2026-09-30). [https://www.ecfr.gov/current/title-45/subtitle-A/subchapter-A/part-46/subpart-A/section-46.117](https://www.ecfr.gov/current/title-45/subtitle-A/subchapter-A/part-46/subpart-A/section-46.117)
5. **Popper, K. R.** (1963). *Conjectures and refutations: The growth of scientific knowledge*. Routledge & Kegan Paul.

> *Xem chi tiết bảng kiểm toán nguồn và bằng chứng xác minh của tuần này tại [Sổ Cái Nguồn Khoa Học (quality/source-ledger.md)](../../quality/source-ledger.md).*

---

## 12. Đánh giá Cuối tuần & Bước tiếp theo (Weekly Quiz & Next Steps)

Sau khi hoàn thành bài đọc lý thuyết chuyên sâu, hãy chuyển sang làm bài trắc nghiệm đánh giá kiến thức tuần để củng cố các khái niệm phương pháp luận:

- 📝 **Làm bài trắc nghiệm tuần:** [Bài kiểm tra trắc nghiệm Tuần 02](../../assessments/quizzes/week-02-quiz.md) *(12 câu trắc nghiệm tình huống chuyên sâu; file đánh giá riêng — không tính là một section H2 của bài học)*.
- 🔑 **Đối chiếu đáp án & giải thích:** [Đáp án & Hướng dẫn học lại Tuần 02](../../assessments/answer-keys/week-02-answers.md) *(Chỉ mở sau khi bạn đã tự hoàn thành bài quiz)*.
- 🗺️ **Quay lại lộ trình học tập:** [Lộ trình Toàn khóa 48 tuần](../../learning-roadmap.md).



