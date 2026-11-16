# Tuần 03: Tương quan, Nhân quả, Độ tin cậy, Tái lập và Cách đọc một Tuyên bố Khoa học (Correlation, Causation, Reliability & Replication)

> **Khóa học Tự học Tâm lý học 48 tuần**  
> **Chặng 1: Nền tảng khoa học** (Tuần 01 – Tuần 08)  
> **Mã bài học:** `W03` | **Trạng thái:** Đang sửa theo review ngày 2026-10-01
> **Thời lượng học tập:**  
> - ⚡ **Tầng Cốt Lõi (Fast Track):** 30–45 phút (Đọc tóm lược, 5 takeaways, bảng so sánh, làm 2 mini-reviews)  
> - 🔬 **Tầng Đọc Sâu (Deep Dive):** 2–3 giờ hoặc hơn (Đọc toàn văn bài học, phân tích toán thống kê hệ số r, giải mã p-value theo tuyên bố ASA, tìm hiểu khủng hoảng tái lập)  
> **Kiến thức tiên quyết (Prerequisites):** Tuần 02 — Phương pháp nghiên cứu: giả thuyết, biến, mẫu, đo lường và đạo đức

> [!NOTE]
> **Ghi chú hiển thị Markdown:** Các khối `<details>` (gợi ý trả lời của mini-review) phụ thuộc trình xem Markdown. Trên GitHub và hầu hết các trình xem hiện đại, thẻ này hỗ trợ đóng/mở tương tác.

---

## 1. Ý nghĩa & Bối cảnh Thực tế (Why It Matters)

Trong đời sống thường ngày, bộ não con người là một cỗ máy tìm kiếm quy luật bẩm sinh. Khi thấy hai hiện tượng thường xuyên xuất hiện cùng nhau — chẳng hạn như "những người uống cà phê nhiều có tỷ lệ mắc bệnh tim cao hơn", hay "trẻ chơi trò chơi điện tử bạo lực có xu hướng cáu kỉnh hơn" — trực giác của chúng ta lập tức nhảy đến kết luận rằng: *cái này chắc chắn gây ra cái kia*. 

Tuy nhiên, sai lầm nhận thức phổ biến và nguy hiểm nhất trong tư duy đời thường lẫn truyền thông đại chúng chính là đánh đồng **Mối liên hệ tương quan (Correlation)** với **Mối quan hệ nhân quả (Causation)** `[C03-03]`. Tương quan chỉ cho biết hai sự việc có xu hướng biến thiên cùng nhau, hoàn toàn không có nghĩa là sự việc này sinh ra sự việc kia. Việc thiếu vắng tư duy phân biệt tương quan - nhân quả khiến con người dễ dàng rơi vào các bẫy ngụy khoa học, tin vào những mối tương quan ảo tưởng, lãng phí thời gian và tiền bạc vào những phương pháp can thiệp vô căn cứ.

Hơn thế nữa, để trở thành một người học tâm lý học có tư duy phản biện độc lập, bạn cần hiểu sâu về công cụ đo lường: Một bài trắc nghiệm tính cách có **độ tin cậy (reliability)** ổn định qua thời gian nhưng có thể hoàn toàn thiếu **độ giá trị (validity)** — tức là không đo lường đúng bản chất tâm lý mà nó tự xưng `[C03-05, C03-06]`. Bạn cũng sẽ được trang bị góc nhìn khoa học chuẩn xác về chỉ số thống kê $p$-value theo cảnh báo chính thức của Hiệp hội Thống kê Hoa Kỳ (ASA), và thấu hiểu cuộc khủng hoảng tái lập (replication crisis) đang thúc đẩy phong trào Khoa học Mở (Open Science) cải cách mạnh mẽ tâm lý học đương đại `[C03-07, C03-08]`.

> [!NOTE]
> **Thông điệp định hướng:** Nhìn thấy mối liên hệ là khởi đầu của sự tò mò; kiểm chứng mối quan hệ nhân quả và thẩm định công cụ đo lường là bổn phận của tư duy khoa học chân chính.

---

## 2. Mục tiêu Học tập Đo lường được (Measurable Learning Objectives)

Sau khi hoàn thành bài học này, bạn sẽ có khả năng:

- **`[LO-W03-01]` (Nhớ/Hiểu):** **Định nghĩa và giải thích ý nghĩa toán học** của hệ số tương quan Pearson ($r$), phân biệt chiều hướng (thuận vs nghịch) và độ mạnh ($|r|$), khẳng định giá trị tương quan nghịch không đồng nghĩa với không có mối liên hệ.
- **`[LO-W03-02]` (Phân tích):** **Phân tích bản chất ngụy biện** khi đồng nhất tương quan với nhân quả; chỉ ra và giải thích cơ chế của hai trở ngại phương pháp luận cốt lõi: Vấn đề biến thứ ba (third variable problem) và Vấn đề chiều hướng tác động (directionality problem).
- **`[LO-W03-03]` (Áp dụng/Phân tích):** **Phân biệt và đánh giá mối quan hệ** giữa Độ tin cậy (Reliability: test-retest, inter-rater, internal consistency) và Độ giá trị (Validity: construct, ecological, external); chứng minh vì sao độ tin cậy là điều kiện cần nhưng không đủ cho độ giá trị.
- **`[LO-W03-04]` (Đánh giá):** **Đánh giá phản biện các công bố khoa học**: hiểu đúng ý nghĩa và giới hạn của ngưỡng thống kê $p < .05$, xem xét kích thước hiệu ứng và độ bất định trong bối cảnh theo ASA; phân biệt các chỉ báo tái lập khác nhau và các cơ chế hệ thống được đề xuất cùng giải pháp của phong trào Khoa học Mở (preregistration, open science).

100% mục tiêu trên được đo lường trực tiếp thông qua [Bài kiểm tra trắc nghiệm Tuần 03](../../assessments/quizzes/week-03-quiz.md).

---

## 3. Học nhanh khi mệt (Fast-Track Core — 30–45 phút)

### 3.1. Tóm lược Cốt lõi (Core Summary)
- **Hệ số Tương quan Pearson ($r$):** Chỉ số mô tả độ mạnh và chiều hướng của mối quan hệ tuyến tính, từ $-1.00$ đến $+1.00$ `[C03-01]`. Dấu chỉ chiều hướng; $|r|$ chỉ độ mạnh. $r = 0$ nghĩa là không có quan hệ tuyến tính `[C03-01]`.
- **Tương quan Thuận vs Tương quan Nghịch:**
  - *Tương quan thuận ($r > 0$):* Hai biến cùng tăng hoặc cùng giảm (ví dụ: giờ học và điểm số).
  - *Tương quan nghịch ($r < 0$):* Một biến tăng thì biến kia có xu hướng giảm. $r=-.85$ là liên hệ tuyến tính nghịch mạnh, không phải “không liên quan”; độ mạnh này không tự bảo đảm dự báo chính xác cho từng cá nhân `[C03-02]`.
- **Tương quan KHÔNG LÀ Nhân quả (Correlation $\neq$ Causation):** Hai biến tương quan chặt chẽ không chứng minh nhân quả do hai rào cản:
  1. *Vấn đề biến thứ ba (Third-variable problem):* Một yếu tố thứ ba $C$ có thể liên hệ với cả $A$ và $B$, tạo ra hoặc làm lệch tương quan (ví dụ: nắng nóng có thể làm tăng cả doanh số bán kem và việc đi bơi) `[C03-03]`.
  2. *Vấn đề chiều hướng (Directionality problem):* Không xác định được $A$ gây ra $B$ hay $B$ gây ra $A$ `[C03-03]`.
- **Tương quan Ảo tưởng (Illusory Correlation):** Tin mối liên hệ mạnh hơn hoặc rõ hơn dữ liệu hỗ trợ `[C03-04]`.
- **Độ tin cậy vs Độ giá trị:**
  - *Độ tin cậy (Reliability):* Tính nhất quán, ổn định qua các lần đo lặp lại (test-retest, inter-rater, internal consistency) `[C03-05]`.
  - *Độ giá trị (Validity):* Mức độ thang đo đo lường chính xác đúng khái niệm cần đo (construct, ecological, external validity) `[C03-06]`.
  - *Nguyên lý vàng:* Độ tin cậy là điều kiện cần nhưng KHÔNG ĐỦ cho độ giá trị. Một công cụ sai lệch có thể ổn định 100% nhưng hoàn toàn vô giá trị `[C03-05, C03-06]`.
- **Đọc một $p$-value:** Trước dữ liệu, đặt $H_0$, chọn thống kê $T$ và hướng thay thế. Sau khi có $t_{obs}$, $p$ là xác suất, **giả sử $H_0$ và quy trình đúng**, thấy $T$ lệch ít nhất xa như $t_{obs}$ theo hướng đã định, dựa trên phân phối lấy mẫu dưới $H_0$. Kiểm định $t$ thường cần mẫu ngẫu nhiên/độc lập và dữ liệu gần chuẩn khi cỡ mẫu nhỏ `[C03-07]`. “Giảm” dùng đuôi trái: $P(T\le t_{obs}\mid H_0,\text{quy trình})$; “tăng” dùng đuôi phải; “khác” dùng hai đuôi. $p$ không phải xác suất $H_0$ đúng hay thước đo độ lớn hiệu ứng; ASA yêu cầu xem bối cảnh, độ lớn và độ bất định `[C03-07]`.
- **Khủng hoảng Tái lập & Khoa học Mở:** OSC tái lập 100 nghiên cứu: 97% gốc nhưng 36% tái lập đạt ý nghĩa; hiệu ứng trung bình giảm $r=.403$ xuống $r=.197$; 39% đánh giá chủ quan tái lập, 47% hiệu ứng gốc nằm trong CI 95% tái lập. Các tiêu chí khác nhau trong mẫu dự án, không phải tỷ lệ phổ quát `[C03-08]`. Preregistration, Registered Reports và chia sẻ dữ liệu tăng minh bạch; thiên lệch xuất bản, p-hacking và HARKing có thể góp phần.

---

### 3.2. Bảng So sánh Nhanh: Độ tin cậy vs Độ giá trị trong Đo lường Tâm lý học

| Tiêu chí | Độ tin cậy (*Reliability*) | Độ giá trị (*Validity*) |
| :--- | :--- | :--- |
| **Định nghĩa** | Tính nhất quán, ổn định và lặp lại của phép đo `[C03-05]` | Tính chính xác của công cụ trong việc đo đúng khái niệm `[C03-06]` |
| **Câu hỏi cốt lõi** | *"Nếu đo lại vào ngày mai, kết quả có giống vậy không?"* | *"Công cụ này có thực sự đo đúng thứ nó tuyên bố đo không?"* |
| **Các dạng chính** | Test-retest, Inter-rater, Internal consistency (Cronbach's $\alpha$) | Construct validity, Ecological validity, External validity |
| **Quan hệ** | Điều kiện CẦN; không có tin cậy thì không có giá trị | Không đảm bảo giá trị tạo ra từ độ tin cậy đơn thuần |
| **Ví dụ minh họa** | Chiếc cân luôn cân thừa 5kg một cách ổn định | Thước dây đo chu vi đầu không đo được trí tuệ |

---

### 3.3. 5 Điểm cốt lõi cần nhớ (5 Key Takeaways)
1. **Tương quan không phải nhân quả:** Dù $|r| = 0.99$, dữ liệu thuần tương quan vẫn không đủ để suy diễn quan hệ nhân quả `[C03-03]`.
2. **Dấu của $r$ chỉ chiều hướng, không chỉ độ mạnh:** Tương quan nghịch $r = -0.80$ mạnh hơn nhiều so với tương quan thuận $r = +0.20$ `[C03-01, C03-02]`.
3. **Tìm các giải thích thay thế:** Trước phát hiện tương quan giật gân, hãy hỏi yếu tố thứ ba nào có thể góp phần vào cả hai biến `[C03-03]`.
4. **Nhất quán không đồng nghĩa chính xác:** Một phép đo có thể cho kết quả ổn định 100% nhưng hoàn toàn vô giá trị khoa học `[C03-05, C03-06]`.
5. **$p < .05$ không đo lường tầm quan trọng thực tế:** Phải xem xét kích thước hiệu ứng (effect size) và khoảng tin cậy để đánh giá giá trị thực tiễn `[C03-07]`.

---

## 4. Bài đọc sâu (Deep Dive)

### 4.1. Toán học của Hệ số Tương quan: Ý nghĩa của $r$, Đám mây điểm và Ranh giới Tuyến tính

Để định lượng mối quan hệ giữa hai biến số liên tục, nhà thống kê học Karl Pearson đã phát triển **Hệ số tương quan Pearson ($r$)** `[C03-01]` 🟢 **[Mạnh]**. Về mặt toán học, hệ số $r$ đo lường mức độ mà các điểm dữ liệu của hai biến số cùng dao động quanh giá trị trung bình của chúng theo một đường thẳng (quan hệ tuyến tính):

$$r = \frac{\sum (X - \bar{X})(Y - \bar{Y})}{\sqrt{\sum (X - \bar{X})^2 \sum (Y - \bar{Y})^2}}$$

Hệ số $r$ luôn bị chặn trong khoảng số học: $-1.00 \le r \le +1.00$. Hai thuộc tính toán học này quyết định cách chúng ta diễn giải dữ liệu `[C03-01, C03-02]`:
1. **Dấu đại số ($+$ hoặc $-$):** Thể hiện **chiều hướng** của mối liên hệ.
   - Dấu dương ($+$): Tương quan thuận — Khi $X$ tăng thì $Y$ có xu hướng tăng; khi $X$ giảm thì $Y$ có xu hướng giảm. Đồ thị đám mây điểm (scatter plot) dốc lên từ trái qua phải.
   - Dấu âm ($-$): Tương quan nghịch — Khi $X$ tăng thì $Y$ có xu hướng giảm. Đồ thị đám mây điểm dốc xuống từ trái qua phải.
2. **Giá trị tuyệt đối ($|r|$):** Thể hiện **độ mạnh** của mối liên hệ tuyến tính.
   - $|r| = 1.00$: Mối quan hệ tuyến tính hoàn hảo (tất cả các điểm nằm chính xác trên một đường thẳng).
   - $|r| = 0$: Hoàn toàn không có mối quan hệ tuyến tính.
   - Một số tài liệu dùng các mốc gần $|r|=.10$, $.30$, $.50$ để minh họa yếu, vừa và mạnh; đây chỉ là heuristic phụ thuộc lĩnh vực, thang đo, mẫu và mục tiêu phân tích, không phải phân loại phổ quát.

```text
  Tương quan thuận mạnh       Tương quan nghịch mạnh          Không có tương quan
        (r ≈ +0.85)                 (r ≈ -0.85)                     (r ≈ 0.00)
    Y                           Y                           Y
    ▲        *                  ▲   *                       ▲       *    *
    │      * *                  │     *                     │   *     *    *
    │    * *                    │       * *                 │     *   *  *
    │  * *                      │         * *               │   *   *    *
    │* *                        │           * *             │     *    *
    └──────────► X              └──────────► X              └──────────► X
```

> [!WARNING]
> **Cảnh báo toán thống kê:** Hệ số tương quan Pearson chỉ phản ánh **quan hệ tuyến tính (linear relationship)**. Nếu hai biến có mối quan hệ phi tuyến hình chữ U ngược (chẳng hạn như Định luật Yerkes-Dodson: mức độ hưng phấn vừa phải tối ưu hóa hiệu suất làm việc, nhưng hưng phấn quá thấp hoặc quá cao đều làm giảm hiệu suất), hệ số $r$ tính ra có thể gần bằng 0 dù hai biến có mối liên hệ mật thiết. Do đó, nhà nghiên cứu bắt buộc phải luôn quan sát biểu đồ đám mây điểm (scatter plot) trước khi tính toán $r$.

### 4.2. Hai Cạm bẫy Phương pháp luận Khiến Tương quan Không Thể Suy ra Nhân quả

Khẩu hiệu *"Tương quan không đồng nghĩa với nhân quả"* (Correlation does not equal causation) là nguyên tắc cảnh báo cốt lõi trong giảng dạy phương pháp nghiên cứu `[C03-03]` 🟢 **[Mạnh]**. Khi hai biến số $A$ và $B$ có tương quan, dữ liệu tương quan đơn độc chưa xác định được cơ chế nhân quả; có thể tồn tại ít nhất ba khả năng cạnh tranh:
1. $A$ là nguyên nhân sinh ra $B$ ($A \rightarrow B$).
2. $B$ là nguyên nhân sinh ra $A$ ($B \rightarrow A$).
3. Một biến số thứ ba chưa đo lường $C$ là nguyên nhân sinh ra cả $A$ lẫn $B$ ($C \rightarrow A$ và $C \rightarrow B$).

Sự bất lực của dữ liệu tương quan trong việc xác lập nhân quả bắt nguồn từ hai rào cản phương pháp luận:

```text
       1. VẤN ĐỀ CHIỀU HƯỚNG TÁC ĐỘNG          2. VẤN ĐỀ BIẾN THỨ BA (Confounding)
          (Directionality Problem)                    (Third-Variable Problem)

                 A ?──────? B                                    C (Biến thứ 3)
                                                               ┌─┴─┐
              A ──────► B  (A gây ra B)                       ▼     ▼
                        hoặc                                  A     B
              B ──────► A  (B gây ra A)                 (A và B chỉ cùng biến thiên)
```

#### 1. Vấn đề Chiều hướng Tác động (Directionality Problem)
Khi phát hiện mối tương quan giữa hai biến số thu thập tại cùng một thời điểm, nhà nghiên cứu không có căn cứ thời gian để xác định biến nào xảy ra trước và tác động lên biến nào. Ví dụ: Các nghiên cứu khảo sát nhận thấy mối tương quan thuận mạnh mẽ giữa *Lòng tự trắc ẩn (Self-compassion)* và *Mức độ hạnh phúc*. Liệu việc rèn luyện lòng tự trắc ẩn giúp con người cảm thấy hạnh phúc hơn ($A \rightarrow B$), hay chính vì một người vốn đang có cuộc sống hạnh phúc, bình yên nên họ mới có tâm trí bao dung và tự trắc ẩn với bản thân ($B \rightarrow A$)? Khảo sát tương quan hoàn toàn bất lực trước câu hỏi này `[C03-03]`.

#### 2. Vấn đề Biến thứ ba (Third-Variable Problem / Confounding)
Nghiêm trọng hơn nữa, $A$ và $B$ có thể hoàn toàn không có mối liên hệ nhân quả trực tiếp nào với nhau; chúng tương quan với nhau đơn thuần vì cùng chịu sự chi phối của một **biến số thứ ba ngoại lai (confounding variable)**. 
- *Ví dụ minh họa:* Tương quan giữa *doanh số bán kem* và *số vụ đuối nước* có thể được giải thích bằng *nhiệt độ mùa hè* ($C$): trời nóng có thể làm tăng cả việc mua kem ($C \rightarrow A$) và đi bơi ($C \rightarrow B$). Đây là giả thuyết về biến thứ ba để kiểm tra, không phải cơ chế đã được xác lập bởi chính tương quan này `[C03-03]`.

Khi một người nhận thấy hoặc tin rằng mối liên hệ giữa hai biến mạnh hơn, khác hướng, hoặc rõ ràng hơn dữ liệu thực tế hỗ trợ, hiện tượng **Tương quan ảo tưởng (Illusory Correlation)** có thể xuất hiện `[C03-04]` 🟡 **[Khá]**. Fiedler (2000) mô tả miền khái niệm này trong các quan sát tuần tự về kích thích: độ lệch có hệ thống khỏi mối tương quan được trình bày khách quan, từ phóng đại/giảm nhẹ đến đảo chiều hoặc bóp méo quan hệ. Đây là bài tổng quan lý thuyết, nên không biến nó thành một cơ chế đơn lẻ hay quy luật cho mọi cách diễn giải bảng dữ liệu. Bộ não con người dễ bị ảnh hưởng bởi những sự kiện nổi bật, kịch tính (ví dụ: một bác sĩ sản khoa nhớ mãi những đêm trăng tròn bận rộn đỡ đẻ và tin rằng "trăng tròn làm tăng tỷ lệ sinh nở"). Niềm tin sai lệch này có thể được củng cố bởi **Thiên kiến xác nhận (Confirmation bias)**: người đó chú ý và ghi nhớ các lần trùng hợp, đồng thời bỏ qua các lần không phù hợp `[C03-04]`.

#### Các hướng mở rộng cần đọc thận trọng
Trong thực tế, khi RCT không khả thi, các thiết kế dọc, tự nhiên, bán thực nghiệm hoặc mô hình thống kê có thể đóng góp bằng chứng nếu giả định và giới hạn được nêu rõ. Phần dưới đây là bản đồ khái niệm để đặt câu hỏi, không phải hướng dẫn kết luận nhân quả `[C03-03]`.
1. **Kiểm soát Thống kê bằng Hồi quy Tuyến tính Bội (Multiple Regression) và Tương quan Riêng (Partial Correlation):**
   - Thay vì chỉ đo lường tương quan đơn biến giữa hai yếu tố, nhà nghiên cứu thu thập dữ liệu về hàng loạt biến số thứ ba tiềm năng (như thu nhập gia đình, học vấn cha mẹ, chỉ số IQ ban đầu).
   - Hồi quy bội có thể điều chỉnh các biến đã đo theo mô hình giả định; lựa chọn biến cần dựa vào hiểu biết nhân quả và thứ tự thời gian. Điều chỉnh mọi biến có thể gây hại nếu biến là trung gian hoặc collider; hồi quy không bảo đảm đã loại hết nhiễu chưa đo, sai số đo, lựa chọn mẫu hay sai lệch mô hình. Vì vậy kết quả còn lại chỉ là bằng chứng hỗ trợ có điều kiện, không chứng minh cơ chế trực tiếp.
2. **Thiết kế Bảng Dọc Chéo Trễ (Cross-Lagged Panel Design):**
   - Để giải quyết bài toán chiều hướng tác động (*Directionality problem*), nhà nghiên cứu đo lường đồng thời cả hai biến số $X$ và $Y$ tại ít nhất hai thời điểm cách nhau trong thời gian ($T_1$ và $T_2$) trên cùng một nhóm đối tượng.
   - So sánh các đường chéo trễ có thể cho biết biến nào đi trước về thời gian trong mô hình đã chỉ định; nó không tự chứng minh chiều nhân quả. Khi các cấu trúc có đặc tính ổn định theo thời gian, các hệ số trễ có thể không đại diện cho quan hệ trong cùng cá nhân và dẫn đến kết luận nhân quả sai; đây là cảnh báo từ phần tóm tắt của Hamaker và cộng sự (2015), không phải công thức phân tích đầy đủ.
3. **Thiết kế Chuỗi Thời gian Ngắt quãng (Interrupted Time Series Design):**
   - Theo dõi biến phụ thuộc qua nhiều mốc trước và sau một can thiệp có thời điểm xác định có thể cho thấy thay đổi về mức độ hoặc độ dốc quanh thời điểm đó. Thiết kế cần chuỗi quan sát trước/sau đủ phù hợp với câu hỏi và giả định về xu thế phản sự kiện; xu thế tự nhiên, thay đổi đồng thời, mùa vụ và các yếu tố gây nhiễu biến thiên theo thời gian vẫn có thể giải thích một phần thay đổi. Bernal và cộng sự (2017) nhấn mạnh các điều kiện và giới hạn này `[C03-03]`.

### 4.3. Thẩm định Đo lường Tâm lý học: Độ tin cậy vs Độ giá trị

Trong khoa học tâm lý học (Psychometrics), việc chế tạo và kiểm định các công cụ đo lường (bài test, bảng hỏi, thang đo) bắt buộc phải trải qua việc đánh giá nghiêm ngặt hai phẩm chất cốt tử: Độ tin cậy (Reliability) và Độ giá trị (Validity) `[C03-05, C03-06]` 🟢 **[Mạnh]**.

#### 1. Các dạng Độ tin cậy (Reliability)
Độ tin cậy phản ánh tính nhất quán và khả năng lặp lại của phép đo:
- **Độ tin cậy kiểm tra - kiểm tra lại (Test-retest reliability):** Được ước lượng từ nhiều người và điều kiện đo tương đương, không kết luận chỉ từ một người. Một chênh lệch điểm IQ giữa hai ngày có thể phản ánh thay đổi thật, điều kiện làm bài, sai số đo hoặc độ ổn định thấp; cần dữ liệu nhóm và khoảng bất định để đánh giá.
- **Độ tin cậy giữa các người đánh giá (Inter-rater reliability):** Mức độ đồng thuận và nhất quán giữa hai hoặc nhiều quan sát viên độc lập khi cùng cho điểm hoặc đánh giá một hành vi.
- **Tính nhất quán nội tại (Internal consistency):** Mức độ các mục trong thang đo liên hệ nhất quán trong mẫu và điều kiện cụ thể, thường được tóm tắt bằng Cronbach's alpha. Các ngưỡng thường được báo cáo khoảng $\alpha=.70$–$.95$ tùy bối cảnh; alpha phụ thuộc số mục, mức liên hệ giữa mục và mẫu, không tự chứng minh đơn hướng, độ giá trị hay chất lượng cho mọi mục đo `[C03-05]`.

#### Alpha, phân tích nhân tố và bằng chứng cấu trúc
Cronbach's alpha là chỉ số **nhất quán nội tại** của các mục trong một mẫu; nó không phải phép kiểm tra số chiều của thang đo. **Phân tích nhân tố (factor analysis)** là một họ phương pháp dùng mẫu hình hiệp phương sai/tương quan giữa các mục để ước lượng những chiều hoặc nhân tố tiềm ẩn có thể tổ chức các mục. Kết quả “phù hợp với hai nhân tố” vì vậy là bằng chứng cấu trúc trong mẫu và mô hình đã chọn, không tự khẳng định một cấu trúc cố định trong mọi mẫu hay chứng minh thang đo có giá trị tiêu chí.

Đọc hai loại bằng chứng này như hai câu hỏi khác nhau: alpha hỏi các mục có nhất quán với nhau trong dữ liệu này không; phân tích nhân tố hỏi cấu trúc chiều tiềm ẩn nào có thể giải thích tương quan giữa các mục. Alpha cao không chứng minh đơn hướng, và **không có ngưỡng alpha phổ quát** để thay thế đánh giá cấu trúc hoặc giá trị. Content grouping (nhóm mục theo nội dung do người thiết kế đặt ra) có thể giúp hình thành giả thuyết, nhưng chỉ bản thân việc chia nhóm không phải bằng chứng nhân tố. Cần nêu rõ mẫu, mô hình và mức độ bất định khi diễn giải; một kết quả cấu trúc thăm dò không thay thế kiểm định trên dữ liệu độc lập `[C03-05, C03-06]`.

#### 2. Các dạng Độ giá trị (Validity)
Độ giá trị phản ánh mức độ thang đo thực sự đo lường đúng bản chất khái niệm tâm lý mà nó dự định đo:
- **Độ giá trị khái niệm (Construct validity):** Thang đo có thực sự đo lường đúng cấu trúc lý thuyết trừu tượng cần đo hay không (ví dụ: Thang đo Trầm cảm Beck BDI-II có phản ánh đúng các tiêu chuẩn lâm sàng của trầm cảm không, hay lại đo sang trạng thái lo âu tạm thời?).
- **Độ giá trị sinh thái (Ecological validity):** Kết quả đo lường trong phòng thí nghiệm có phản ánh đúng hành vi diễn ra trong môi trường thực tế hàng ngày hay không.
- **Độ giá trị ngoại tại (External validity):** Mức độ mà kết quả nghiên cứu từ mẫu có thể khái quát hóa sang các quần thể dân số và bối cảnh văn hóa khác.

#### 3. Mối quan hệ biện chứng giữa Độ tin cậy và Độ giá trị
Hình tượng tấm bia bắn cung kinh điển mô tả trọn vẹn mối quan hệ này:
1. *Không tin cậy, không giá trị:* Các mũi tên bắn rải rác khắp nơi, không chụm lại và không trúng tâm.
2. *Tin cậy cao, nhưng KHÔNG có giá trị:* Tất cả các mũi tên bắn chụm lại thành một cụm cực kỳ sát nhau ở góc ngoài cùng mép bia. Kết quả vô cùng ổn định, nhất quán nhưng bắn sai mục tiêu! (Ví dụ: Dùng thước dây đo chu vi vòng đầu để suy ra trí thông minh — kết quả lần nào đo cũng trùng khớp hoàn hảo nhưng hoàn toàn không đo được trí thông minh).
3. *Vừa tin cậy, vừa có giá trị:* Tất cả các mũi tên bắn chụm lại chính xác vào tâm hồng tâm `[C03-05, C03-06]`.

```text
  Không tin cậy, Không giá trị       Tin cậy nhưng KHÔNG giá trị       Vừa tin cậy, Vừa giá trị
        ┌───────────────┐                 ┌───────────────┐                 ┌───────────────┐
        │   *           │                 │           *** │                 │               │
        │       *   *   │                 │           *** │                 │      ***      │
        │     *       * │                 │           *** │                 │      ***      │
        │   *     *     │                 │               │                 │               │
        └───────────────┘                 └───────────────┘                 └───────────────┘
```

> **Nguyên lý nhận thức luận:** Độ tin cậy là điều kiện tiên quyết cần thiết, nhưng không bao giờ là điều kiện đủ để bảo đảm độ giá trị. Một công cụ đo lường vô nghĩa vẫn có thể đạt độ tin cậy tuyệt đối `[C03-05, C03-06]`.

---

## 5. Nghiên cứu & Mô hình Tiêu biểu (Key Studies & Theoretical Models)

### 5.1. Tuyên bố Tiêu biểu 1: Tuyên bố của Hiệp hội Thống kê Hoa Kỳ (ASA) về $p$-values và Kích thước Hiệu ứng `[C03-07]` 🟢 **[Mạnh]**

- **Tác giả & Bối cảnh Xuất bản:** Wasserstein, R. L., & Lazar, N. A. (2016). *The ASA statement on p-values: Context, process, and purpose*. The American Statistician, 70(2), 129–133 `[C03-07]`.
- **Phạm vi kết luận của ASA:** ASA (2016) cảnh báo rằng việc dùng ngưỡng cơ học $p < .05$ như một cánh cổng nhị phân cho kết luận khoa học là không đủ; diễn giải cần đặt trong bối cảnh mô hình, quy trình phân tích, độ lớn hiệu ứng, độ bất định và tính minh bạch. Đoạn này không quy một lịch sử nhân quả định lượng cho toàn bộ thực hành khoa học.
- **Sáu Nguyên tắc Chuẩn tắc do ASA Xác lập:**
  1. *Giá trị $p$ có thể chỉ ra mức độ không tương thích giữa dữ liệu và một mô hình thống kê xác định (thường là giả thuyết không $H_0$).*
  2. *Giá trị $p$ KHÔNG đo lường xác suất mà giả thuyết nghiên cứu là đúng, cũng KHÔNG đo xác suất dữ liệu được tạo ra do ngẫu nhiên đơn thuần.*
  3. *Các kết luận khoa học và quyết định chính sách không bao giờ được phép chỉ dựa vào việc một giá trị $p$ có vượt qua một ngưỡng tùy tiện (như $.05$) hay không.*
  4. *Suy luận thống kê đòi hỏi sự minh bạch toàn diện; không được phép "chọn lọc dữ liệu đẹp" hay chỉ báo cáo các phép thử có $p < .05$ (chống cherry-picking).*
  5. *Giá trị $p$, hoặc ý nghĩa thống kê, KHÔNG đo lường độ lớn của một hiệu ứng (effect size) hay tầm quan trọng thực tiễn của một kết quả.* Một mẫu nghiên cứu cực lớn ($N = 100.000$) có thể tạo ra $p < .001$ cho một khác biệt vô cùng nhỏ nhặt, hoàn toàn không có ý nghĩa lâm sàng đời thực.
  6. *Đơn độc một giá trị $p$ không cung cấp thước đo tốt về bằng chứng liên quan đến một mô hình hay giả thuyết.*
- **Khuyến nghị thực hành:** ASA khuyến khích diễn giải kết quả trong bối cảnh, báo cáo đầy đủ và minh bạch, đồng thời cân nhắc các cách biểu diễn độ lớn hiệu ứng và độ bất định phù hợp (ví dụ effect size, khoảng tin cậy hoặc khoảng dự báo). ASA không áp đặt một công thức effect size kèm CI duy nhất; đây là cách áp dụng minh họa, không phải quy tắc bắt buộc `[C03-07]`.

### 5.2. Nghiên cứu Tiêu biểu 2: Đại Dự án Tái lập Tâm lý học và Phong trào Khoa học Mở `[C03-08]` 🟡 **[Khá]**

- **Tác giả & Công bố:** Open Science Collaboration. (2015). *Estimating the reproducibility of psychological science*. Science, 349(6251), aac4716; Nosek, B. A., et al. (2018). *The preregistration revolution*. PNAS, 115(11), 2600–2606 `[C03-08]`. Bản PDF OSC được mở để đối chiếu là [bản sao truy cập được](https://gwern.net/doc/statistics/bias/2015-opensciencecollaboration.pdf), phần kết quả và bảng tiêu chí trong PDF; bài Nosek được mở toàn thân qua [PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC5856500/) để đối chiếu các phần phương pháp và giới hạn. DOI Science bị máy chủ nhà xuất bản từ chối trong phiên này.
- **Phương pháp & Mẫu nghiên cứu:** 270 nhà nghiên cứu trên toàn thế giới hợp tác tái lập trực tiếp 100 nghiên cứu thực nghiệm được xuất bản trên ba tạp chí tâm lý học hàng đầu thế giới trong năm 2008.
- **Kết quả cần đọc theo nhiều tiêu chí:** OSC tái lập 100 nghiên cứu với 270 tác giả/cộng tác viên; 97% nghiên cứu gốc có kết quả ý nghĩa thống kê, còn 36% tái lập đạt ý nghĩa thống kê. Hiệu ứng trung bình giảm từ chính xác $r = .403$ xuống $r = .197$. Các tiêu chí khác cho kết quả khác: 39% được đánh giá chủ quan là tái lập và 47% hiệu ứng gốc nằm trong khoảng tin cậy 95% của hiệu ứng tái lập. Đây là ước lượng của một mẫu nghiên cứu tâm lý học được chọn trong dự án, không phải một tỷ lệ phổ quát cho mọi lĩnh vực hoặc mọi thất bại tái lập `[C03-08]`.
- **Các cơ chế hệ thống được đề xuất/có thể góp phần:**
  - *Thiên lệch xuất bản (Publication bias / File-drawer effect):* Có thể làm các kết quả không đạt ngưỡng ít xuất hiện hơn trong tài liệu; không nên diễn đạt thành mọi tạp chí đều từ chối kết quả không có ý nghĩa hoặc mọi kết quả không có ý nghĩa đều là hiệu ứng âm.
  - *Đào bới dữ liệu (p-hacking):* Việc thử nhiều cách phân tích hoặc thay đổi tiêu chí sau khi xem dữ liệu có thể làm tăng kết quả dương tính giả.
  - *HARKing (Hypothesizing After the Results are Known):* Trình bày giả thuyết sau khi biết dữ liệu như thể đã định trước có thể làm giảm tính minh bạch. OSC không xác định một nguyên nhân duy nhất hay nguyên nhân phổ quát cho từng thất bại tái lập.
- **Giải pháp Cách mạng của Khoa học Mở (Open Science):**
   - **Đăng ký trước (Preregistration):** Nhà nghiên cứu ghi lại và, khi phù hợp, đăng công khai kế hoạch trước khi biết kết quả; kế hoạch có thể nêu giả thuyết, cỡ mẫu và phương pháp phân tích. Đây là công cụ tăng minh bạch, không bảo đảm tự động ngăn p-hacking hay HARKing.
  - **Báo cáo đã đăng ký (Registered Reports):** Sau khi Stage 1 được chấp nhận về nguyên tắc (in-principle acceptance), Stage 2 được xem xét theo việc tuân thủ giao thức đã đăng ký, các kiểm tra chất lượng và cách báo cáo; kết quả không phải căn cứ để từ chối, nhưng preregistration đơn lẻ không tạo ra cam kết xuất bản `[C03-08]`.

---

## 6. Biết được gì / Chưa thể kết luận gì (What We Know vs. What Remains Uncertain)

### 6.1. Những điều khoa học đã xác lập tương đối vững chắc (What We Know)

- **Tương quan không tự chứng minh nhân quả:** Bất kể hệ số $r$ cao đến đâu hay đồ thị thẳng hàng đến mức nào, dữ liệu tương quan đơn độc không xác lập quan hệ nhân quả. Thí nghiệm ngẫu nhiên là một đường bằng chứng mạnh; các thiết kế dọc, tự nhiên, bán thực nghiệm, hồi quy và ITS cũng có thể đóng góp nếu giả định và giới hạn được nêu rõ `[C03-03]` 🟢 **[Mạnh]**. Mục 4.2 chỉ cung cấp bản đồ khái niệm có điều kiện, không phải công thức kết luận nhân quả.
- **Độ tin cậy là điều kiện cần nhưng không đủ cho độ giá trị:** Một thang đo không ổn định thì không thể chính xác, nhưng một thang đo ổn định 100% vẫn có thể đo sai lệch hoàn toàn khái niệm cần đo `[C03-05, C03-06]` 🟢 **[Mạnh]**.
- **Giá trị $p$ không phản ánh kích thước hiệu ứng:** Một kết quả đạt $p < .001$ không đồng nghĩa với việc phát hiện đó có ý nghĩa lớn trong đời sống; nên báo cáo độ lớn hiệu ứng và độ bất định phù hợp với thiết kế, nhưng ASA không quy định một công thức duy nhất `[C03-07]` 🟢 **[Mạnh]**.

### 6.2. Những giới hạn & Điều khoa học chưa thể kết luận (What Remains Uncertain)

- **Nguyên nhân đa yếu tố phức tạp:** Trong thế giới thực, hầu hết các hiện tượng tâm lý (như trầm cảm, tự tử, thành công học tập) đều chịu sự tác động đồng thời của hàng trăm biến số tương tác phi tuyến tính phức tạp; việc cô lập một biến số đơn lẻ trong phòng thí nghiệm đôi khi làm mất đi bức tranh tương tác toàn diện.
- **Giới hạn của các nỗ lực tái lập:** Một nghiên cứu không tái lập được không tự động có nghĩa là nghiên cứu gốc là gian lận; sự thất bại khi tái lập có thể xuất phát từ sự khác biệt tinh vi về bối cảnh văn hóa, thế hệ người tham gia, hoặc sai lệch trong quy trình thao tác của nhóm nghiên cứu mới.

---

## 7. Hiểu lầm phổ biến & Phản biện Pop Psychology (Common Myths & Pop Psych Debunked)

### ❌ Hiểu lầm 1: "Hệ số tương quan âm tính nghĩa là hai yếu tố không liên quan gì đến nhau hoặc liên hệ rất yếu"
- **Nguồn gốc hiểu lầm:** Nhầm lẫn từ ngữ đời thường giữa khái niệm "tiêu cực/âm tính" với "vô giá trị/yếu ớt".
- **Bằng chứng khoa học thực tế:** Dấu âm trong hệ số Pearson chỉ phản ánh chiều hướng ngược nhau của sự biến thiên (nghịch biến), còn $|r|$ mô tả độ mạnh của liên hệ tuyến tính trong mẫu `[C03-01, C03-02]`. Một hệ số $r = -0.85$ cho thấy liên hệ tuyến tính nghịch mạnh trong dữ liệu quan sát; không thể từ độ lớn này một mình bảo đảm độ chính xác dự báo cho từng cá nhân, quan hệ nhân quả, hay khả năng áp dụng ngoài mẫu. Độ chính xác thực tế còn phụ thuộc thang đo, phần dư, bối cảnh và đánh giá trên dữ liệu giữ lại (held-out) khi phù hợp. Ví dụ số giờ thức khuya và điểm thi vì thế chỉ minh họa chiều hướng liên hệ, không chứng minh thức khuya làm điểm thi giảm `[C03-02]` 🟢 **[Mạnh]**.
- **Cách tư duy đúng:** Tách bạch rạch ròi giữa **Dấu** (chỉ chiều hướng biến thiên) và **Độ lớn tuyệt đối $|r|$** (chỉ độ mạnh của liên kết).

### ❌ Hiểu lầm 2: "Một nghiên cứu đạt $p < .05$ chứng minh phương pháp can thiệp đó tạo ra tác động kỳ diệu ngoài đời thực"
- **Nguồn gốc hiểu lầm:** Báo chí truyền thông giật tít: *"Nghiên cứu có ý nghĩa thống kê chứng minh liệu pháp X hiệu quả"*.
- **Bằng chứng khoa học thực tế:** Khái niệm "ý nghĩa thống kê" (statistical significance) không đồng nghĩa với "tầm quan trọng thực tiễn" (practical/clinical significance) `[C03-07]`. Ví dụ giảm 0.1 điểm trên thang 100 là **tình huống giả định**; ý nghĩa thực tiễn không thể kết luận chỉ từ con số đó mà cần biết thang đo, ngưỡng thay đổi có ý nghĩa, độ bất định và bối cảnh người học `[C03-07]` 🟢 **[Mạnh]**.
- **Cách tư duy đúng:** Hỏi kết quả có tương thích với mô hình không, độ lớn ước lượng và độ bất định là gì, thang đo có ý nghĩa ra sao, và bằng chứng thiết kế có đủ cho mục tiêu suy luận không.

---

## 8. Ôn tập Ngắn Giữa tuần (Mid-Week Mini-Reviews)

### 8.1. Mini-Review 1 (Giữa tuần — Ngày 3)

1. **Câu hỏi 1:** Khi một nghiên cứu báo cáo hệ số tương quan giữa mức độ lo âu và chất lượng giấc ngủ là $r = -0.72$, điều này có nghĩa là gì về chiều hướng và độ mạnh?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Dấu âm chỉ ra mối tương quan nghịch (người có mức độ lo âu càng cao thì chất lượng giấc ngủ càng thấp). Độ lớn tuyệt đối $|-0.72| = 0.72$ cho thấy liên hệ tuyến tính khá mạnh trong ví dụ; các mốc phân loại chỉ là heuristic phụ thuộc bối cảnh `[C03-01, C03-02]`.
   </details>

2. **Câu hỏi 2:** Tại sao sự tồn tại của "Biến thứ ba" lại ngăn cản chúng ta kết luận quan hệ nhân quả từ một mối tương quan?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Vì một yếu tố thứ ba có thể liên hệ với cả hai biến và tạo ra hoặc làm lệch tương quan quan sát được; ví dụ thời tiết nóng có thể góp phần làm tăng cả việc mua kem và đi bơi `[C03-03]`.
   </details>

3. **Câu hỏi 3:** Phân biệt "Vấn đề chiều hướng" (Directionality problem) trong nghiên cứu tương quan bằng một ví dụ thực tế?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Khi phát hiện tương quan giữa chơi game bạo lực và hành vi hung hăng, chúng ta không thể biết việc chơi game bạo lực khiến trẻ trở nên hung hăng ($A \rightarrow B$), hay chính những đứa trẻ vốn có tính khí hung hăng từ trước đã chủ động tìm đến các trò chơi bạo lực ($B \rightarrow A$) `[C03-03]`.
   </details>

---

### 8.2. Mini-Review 2 (Cuối tuần — Ngày 5)

1. **Câu hỏi 1:** Tại sao một thang đo có độ tin cậy hoàn hảo vẫn có thể hoàn toàn vô giá trị?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Vì độ tin cậy chỉ đo tính nhất quán, lặp lại của công cụ chứ không đo tính đúng đắn. Một bài test trắc tinh tử vi có thể cho bạn kết quả tính cách giống hệt nhau mỗi lần làm (rất tin cậy), nhưng nó hoàn toàn không đo đúng cấu trúc nhân cách khoa học (vô giá trị) `[C03-05, C03-06]`.
   </details>

2. **Câu hỏi 2:** Tuyên bố của ASA năm 2016 cảnh báo điều gì về việc sử dụng máy móc ngưỡng $p < .05$?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** ASA cảnh báo rằng giá trị $p$ không đo lường độ lớn hiệu ứng hay tầm quan trọng thực tiễn, và việc coi $p < .05$ như một ranh giới nhị phân cứng nhắc để quyết định chân lý khoa học là sai lầm phương pháp luận nghiêm trọng `[C03-07]`.
   </details>

3. **Câu hỏi 3:** Sáng kiến "Báo cáo đã đăng ký" (Registered Reports) của phong trào Khoa học Mở giúp triệt tiêu thiên lệch xuất bản như thế nào?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>
   
   **Đáp án gợi ý:** Stage 1 được bình duyệt trước khi thu thập dữ liệu và có thể nhận chấp thuận về nguyên tắc. Ở Stage 2, nhóm phải cho thấy đã tuân thủ giao thức, vượt qua các kiểm tra chất lượng và diễn giải hợp lý; kết quả dương tính hay âm tính không tự nó quyết định xuất bản, còn preregistration đơn lẻ không bảo đảm xuất bản `[C03-08]`.
   </details>

---

## 9. Bài tập Tình huống Ứng dụng & Reflection Tự nguyện (Case Study & Voluntary Journal)

> [!IMPORTANT]
> **Nguyên tắc An toàn & Tự nguyện:** Hoạt động suy ngẫm và bài tập thực hành là **hoàn toàn tự nguyện**. Bạn không cần phải chia sẻ bất kỳ thông tin cá nhân nhạy cảm nào. Bạn luôn có thể lựa chọn **Kịch bản Giả định (Lựa chọn B)** để rèn luyện kỹ năng phân tích bằng chứng.

### Lựa chọn A: Nhật ký Nhận diện Tương quan Ảo tưởng Cá nhân (Personal Reflection — Tự nguyện)
- **Nội dung gợi ý:** Hãy suy ngẫm về một niềm tin tâm linh hoặc thói quen may rủi mà bạn hoặc người quen từng có (ví dụ: mặc chiếc áo màu đỏ thì hôm đó đi thi sẽ gặp may, hoặc mỗi khi rửa xe thì trời lại đổ mưa).
- **Khung phân tích phản biện:**
  1. *Hai sự kiện nào đã được liên kết với nhau trong tâm trí bạn?*
  2. *Sự kiện nổi bật nào đã khiến não bộ bạn chú ý và ghi nhớ mối liên hệ đó?*
  3. *Thiên kiến xác nhận (Confirmation bias) đã vận hành ra sao: Bạn đã ghi nhớ những lần nào và vô thức lãng quên những lần nào?*
  4. *Nếu lập một bảng thống kê $2 \times 2$ (Áo đỏ + Thi đỗ, Áo đỏ + Thi trượt, Áo khác + Thi đỗ, Áo khác + Thi trượt), bạn dự đoán hệ số $r$ thực tế sẽ là bao nhiêu?*

---

### Lựa chọn B: Phân tích Kịch bản Giả định (Hypothetical Case Study — Khuyến nghị)

**Bối cảnh:**  
Một bài báo trên chuyên trang sức khỏe trực tuyến giật tít chấn động: *"Phát hiện gây sốc: Ăn nhiều sô-cô-la giúp tăng chỉ số IQ và đạt giải Nobel!"*. Bài viết trích dẫn một nghiên cứu thực nghiệm ghi nhận một mối tương quan thuận khá mạnh trong ví dụ ($r = +0.79, p < .001$) giữa lượng tiêu thụ sô-cô-la bình quân đầu người của một quốc gia và số lượng giải Nobel bình quân trên 10 triệu dân của quốc gia đó (Thụy Sĩ đứng đầu ở cả hai bảng xếp hạng). Tác giả bài báo kết luận: *"Các bậc phụ huynh muốn con thông minh đạt giải thưởng quốc tế nên cho trẻ ăn sô-cô-la đen mỗi ngày"*.

**Nhiệm vụ thực hành của bạn:**
1. **Phát hiện và phân tích lỗi ngụy biện phương pháp luận cốt lõi:**
   - Tác giả bài báo đã phạm lỗi đồng nhất mối tương quan thống kê cấp quốc gia ($r = +0.79$) với quan hệ nhân quả cá nhân ($X$ ăn sô-cô-la gây ra $Y$ thông minh).
2. **Phát triển ít nhất 3 giả thuyết về Biến thứ ba (Confounding Variables):**
   - *Biến thứ ba 1 (Mức độ phát triển kinh tế / GDP bình quân đầu người):* Các quốc gia giàu có có thu nhập cao nên người dân chi nhiều tiền mua đồ ăn xa xỉ như sô-cô-la, đồng thời ngân sách quốc gia đầu tư khổng lồ cho các phòng thí nghiệm nghiên cứu khoa học đỉnh cao, dẫn tới nhiều giải Nobel.
   - *Biến thứ ba 2 (Chất lượng hệ thống giáo dục đại học và viện nghiên cứu):* Bối cảnh địa lý Tây Âu có truyền thống sản xuất kẹo sô-cô-la lâu đời và cũng là nơi đặt trụ sở của các trường đại học nghiên cứu lâu đời nhất thế giới.
   - *Biến thứ ba 3 (Khí hậu và văn hóa ẩm thực):* Các nước xứ lạnh tiêu thụ nhiều calo từ đồ ngọt và có điều kiện cơ sở hạ tầng học thuật tập trung trong nhà.
3. **Chỉ ra nguy cơ ngụy biện sinh thái (Ecological Fallacy):**
   - Dữ liệu ở cấp độ vĩ mô toàn quốc gia (mức tiêu thụ bình quân) không thể suy diễn cơ học cho cấp độ vi mô của từng cá nhân (không có bằng chứng nào cho thấy bản thân các nhà khoa học đạt giải Nobel ăn nhiều sô-cô-la hơn người bình thường).
4. **Đề xuất khuyến nghị khoa học:**
   - Không nên đưa lời khuyên can thiệp cá nhân từ tương quan cấp quốc gia. Nếu câu hỏi nhân quả được nghiên cứu tiếp, một RCT hoặc thiết kế có kiểm soát phù hợp cần nêu rõ giả định, đo lường và giới hạn; mù đôi không phải điều kiện duy nhất cho mọi câu hỏi.

---

## 10. Bảng Thuật ngữ Tuần (Weekly Glossary — 10 thuật ngữ cốt lõi)

| Thuật ngữ Tiếng Việt | Thuật ngữ Tiếng Anh | Định nghĩa Khoa học Chuẩn tắc (APA / OpenStax) | Ngữ cảnh Sử dụng & Lưu ý |
| :--- | :--- | :--- | :--- |
| **Hệ số tương quan** | *Correlation Coefficient ($r$)* | Chỉ số thống kê đo lường độ mạnh và chiều hướng của mối liên hệ tuyến tính giữa hai biến số, giá trị từ $-1.00$ đến $+1.00$ `[C03-01]`. | Chỉ áp dụng cho quan hệ tuyến tính; không phản ánh quan hệ nhân quả. |
| **Tương quan thuận** | *Positive Correlation* | Mối quan hệ trong đó hai biến số cùng biến thiên cùng chiều (cùng tăng hoặc cùng giảm) `[C03-02]`. | Đồ thị đám mây điểm dốc lên; hệ số $r > 0$. |
| **Tương quan nghịch** | *Negative Correlation* | Mối quan hệ trong đó hai biến số biến thiên ngược chiều nhau (biến này tăng thì biến kia giảm) `[C03-02]`. | Đồ thị dốc xuống; dấu âm không có nghĩa là mối liên hệ yếu. |
| **Vấn đề biến thứ ba** | *Third-variable Problem* | Trở ngại khi một yếu tố thứ ba có thể liên hệ với cả hai biến và tạo ra hoặc làm lệch tương quan quan sát được `[C03-03]`. | Gợi ý một giải thích thay thế; cần dữ liệu và giả định để đánh giá. |
| **Vấn đề chiều hướng** | *Directionality Problem* | Sự bất lực trong việc xác định biến nào là nguyên nhân xảy ra trước và tác động lên biến kia `[C03-03]`. | Xuất hiện phổ biến trong các nghiên cứu cắt ngang đồng thời. |
| **Tương quan ảo tưởng** | *Illusory Correlation* | Nhận thức sai lệch khi tin rằng mối liên hệ giữa hai biến mạnh hơn, khác hướng, hoặc rõ ràng hơn dữ liệu thực tế hỗ trợ `[C03-04]`. | Có thể được duy trì bởi các sự kiện nổi bật và thiên kiến xác nhận. |
| **Độ tin cậy** | *Reliability* | Tính nhất quán, ổn định và lặp lại được của một công cụ đo lường qua nhiều lần kiểm tra `[C03-05]`. | Điều kiện cần nhưng không đủ cho độ giá trị. |
| **Độ giá trị** | *Validity* | Mức độ chính xác mà một công cụ nghiên cứu thực sự đo lường đúng khái niệm cần đo `[C03-06]`. | Phân biệt với độ tin cậy; phản ánh tính đúng đắn bản chất. |
| **Kích thước hiệu ứng** | *Effect Size* | Chỉ số định lượng mô tả độ lớn của hiện tượng hoặc khác biệt giữa các nhóm (như Cohen's $d$, $r^2$) `[C03-07]`. | Đọc cùng bối cảnh, độ bất định và ý nghĩa thực tiễn; không xếp hạng máy móc với $p$. |
| **Đăng ký trước** | *Preregistration* | Ghi lại kế hoạch nghiên cứu trước khi biết kết quả và có thể công khai kế hoạch đó `[C03-08]`. | Tăng minh bạch; không bảo đảm tự động ngăn p-hacking, HARKing hoặc thất bại tái lập. |

---

## 11. Danh mục Tài liệu Tham khảo và Phạm vi Truy cập (APA 7th)

1. **Spielman, R. M., Jenkins, W. J., & Lovett, M. D.** (2020). *Psychology 2e* (2nd ed.). OpenStax, Rice University. Section 2.3: "Analyzing Findings". [https://openstax.org/books/psychology-2e/pages/2-3-analyzing-findings](https://openstax.org/books/psychology-2e/pages/2-3-analyzing-findings)
2. **Wasserstein, R. L., & Lazar, N. A.** (2016). The ASA statement on p-values: Context, process, and purpose. *The American Statistician*, 70(2), 129–133. [DOI](https://doi.org/10.1080/00031305.2016.1154108) · [bản PDF toàn văn đã mở](https://www.stat.berkeley.edu/~aldous/Real_World/ASA_statement.pdf). Bản PDF ASA chính thức [tóm tắt sáu nguyên tắc](https://www.amstat.org/asa/files/pdfs/P-ValueStatement.pdf) cũng đã được mở; không coi bản tóm tắt này là toàn văn bài tạp chí.
- **Illowsky, B., & Dean, S.** (2023). *Introductory Statistics 2e*. OpenStax. Sections 9.3, “Probability Distribution Needed for Hypothesis Testing,” and 9.5, “Additional Information and Full Hypothesis Test Examples.” [Section 9.3](https://openstax.org/books/introductory-statistics-2e/pages/9-3-probability-distribution-needed-for-hypothesis-testing) · [Section 9.5](https://openstax.org/books/introductory-statistics-2e/pages/9-5-additional-information-and-full-hypothesis-test-examples). Dùng cho ký hiệu nhập môn, giả định t và cách xác định đuôi; không dùng công thức t cụ thể.
3. **Open Science Collaboration.** (2015). Estimating the reproducibility of psychological science. *Science*, 349(6251), aac4716. [DOI](https://doi.org/10.1126/science.aac4716) · [bản PDF đã mở để đối chiếu kết quả](https://gwern.net/doc/statistics/bias/2015-opensciencecollaboration.pdf). DOI/publisher full text không mở được trong phiên này; các số liệu OSC được giới hạn ở bản PDF truy cập được và mẫu dự án.
4. **Nosek, B. A., Ebersole, C. R., DeHaven, A. C., & Mellor, D. T.** (2018). The preregistration revolution. *Proceedings of the National Academy of Sciences (PNAS)*, 115(11), 2600–2606. [https://doi.org/10.1073/pnas.1708274114](https://doi.org/10.1073/pnas.1708274114)
5. **Tavakol, M., & Dennick, R.** (2011). Making sense of Cronbach's alpha. *International Journal of Medical Education, 2*, 53–55. [DOI](https://doi.org/10.5116/ijme.4dfb.8dfd) · [PMC record/full text](https://pmc.ncbi.nlm.nih.gov/articles/PMC4205511/). The opened record supports sample-specific reliability, item-count/item-correlation influences, and the limits of alpha for unidimensionality; its numerical error-variance illustration is not used here.
6. **Myers, D. G., & DeWall, C. N.** (2024). *Psychology* (14th ed.). Worth Publishers. Chapter 2: "Research Methods". Full text was not opened in this repair; advanced methods previously attributed to this citation are source-pending.
7. **Bernal, J. L., Cummins, S., & Gasparrini, A.** (2017). Interrupted time series regression for the evaluation of public health interventions: A tutorial. *International Journal of Epidemiology, 46*(1), 348–355. [Europe PMC full-text XML](https://www.ebi.ac.uk/europepmc/webservices/rest/PMC5407170/fullTextXML). Retrieved and parsed full text is recorded in the validation artifact; the lesson uses only its design requirements, time-varying-confounder cautions, and summary limits.
8. **VanderWeele, T. J.** (2019). Principles of confounder selection. *European Journal of Epidemiology, 34*, 211–219. [Europe PMC full-text XML](https://www.ebi.ac.uk/europepmc/webservices/rest/PMC6447501/fullTextXML). Retrieved and parsed full text is recorded in the validation artifact; the lesson uses only the conditional selection and timing cautions.
9. **Hamaker, E. L., Kuiper, R. M., & Grasman, R. P. P. P.** (2015). A critique of the cross-lagged panel model. *Psychological Methods, 20*(1), 102–116. [University record and abstract](https://research-portal.uu.nl/en/publications/a-critique-of-the-cross-lagged-panel-model) · [DOI](https://doi.org/10.1037/a0038889). The institutional full PDF returned HTTP 403 in this review; the lesson makes only the abstract-level stable-trait caution explicit.
10. **Fiedler, K.** (2000). Illusory correlations: A simple associative algorithm provides a convergent account of seemingly divergent paradigms. *Review of General Psychology, 4*(1), 25–58. [Author-hosted review](https://acs.ist.psu.edu/misc/dirk-files/Papers/ICPapers/2000_Fiedler_RevofGEnPSychhtml.html). The opened review's domain definition is limited to systematic deviation from objectively presented correlations in sequential observations; it is not treated as a single-experiment estimate or universal mechanism.

> *Xem chi tiết bảng kiểm toán nguồn và bằng chứng xác minh của tuần này tại [Sổ Cái Nguồn Khoa Học (quality/source-ledger.md)](../../quality/source-ledger.md).*

---

## 12. Đánh giá Cuối tuần & Bước tiếp theo (Weekly Quiz & Next Steps)

Sau khi hoàn thành bài đọc lý thuyết chuyên sâu, hãy chuyển sang làm bài trắc nghiệm đánh giá kiến thức tuần để củng cố năng lực tư duy thống kê và phương pháp luận:

- 📝 **Làm bài trắc nghiệm tuần:** [Bài kiểm tra trắc nghiệm Tuần 03](../../assessments/quizzes/week-03-quiz.md) *(12 câu trắc nghiệm tình huống chuyên sâu; file đánh giá riêng — không tính là một section H2 của bài học)*.
- 🔑 **Đối chiếu đáp án & giải thích:** [Đáp án & Hướng dẫn học lại Tuần 03](../../assessments/answer-keys/week-03-answers.md) *(Chỉ mở sau khi bạn đã tự hoàn thành bài quiz)*.
- 🗺️ **Quay lại lộ trình học tập:** [Lộ trình Toàn khóa 48 tuần](../../learning-roadmap.md).

## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

Bản bổ sung cần reviewer đối chiếu nguồn/an toàn; không thay verdict lịch sử. Không chấm mức bệnh, tính cách hoặc đời tư.

**Bài:** Tương quan và nhân quả. **Đầu ra:** Phân tích bài báo sô-cô-la/Nobel ở mục 9.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| Mô tả | Tách dữ liệu tương quan cấp quốc gia và kết luận nhân quả cá nhân | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Phương pháp | Nêu biến thứ ba và chiều hướng tác động như vấn đề chưa loại trừ | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Giả thuyết | Có ba giả thuyết thay thế và dữ kiện cần để kiểm tra | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Cấp phân tích | Không suy kết quả trung bình quốc gia thành tác dụng cho cá nhân | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Kết luận | Không khuyên can thiệp dinh dưỡng dựa vào tương quan | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
