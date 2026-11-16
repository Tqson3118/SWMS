# W10 — Đầu tư tăng trưởng, Index/ETF và DCA

> [!IMPORTANT]
> **Tuyên bố Phạm vi và Giới hạn Lĩnh vực (Scope và Safety Disclaimer):** Nội dung bài học chỉ phục vụ mục đích giáo dục, KHÔNG phải lời khuyên đầu tư hay khuyến nghị mua/bán bất kỳ tài sản, quỹ hay ETF nào. Mọi ví dụ đều là giả định minh họa, không mô tả mã cổ phiếu, quỹ hay ETF cụ thể. Quy định, phí, thuế, thanh khoản và dữ liệu thị trường có thể thay đổi; bạn phải tự kiểm tra lại từ nguồn chính thức (công ty quản lý quỹ, sàn giao dịch, cơ quan quản lý) và tự chịu trách nhiệm trước khi hành động.

---

## 1. Ý Nghĩa Thực Tiễn (Why It Matters)

Tuần trước (W09) bạn đã học kỷ luật đầu tư giá trị: ước lượng giá trị, so với giá, chỉ hành động khi có biên an toàn. Nhưng đó chỉ là một trường phái. Câu hỏi thực tế của người mới bắt đầu thường khác hẳn: "Mình có 120 triệu, nên dồn một lần hay chia nhỏ mua dần?" — "Nên chọn cổ phiếu tăng trưởng hay quỹ chỉ số?" — "DCA có phải là cách để luôn thắng thị trường?". Bài này trả lời ba câu hỏi đó bằng khung khái niệm và bằng chứng đã kiểm chứng, không bằng khẩu hiệu.

Ba nhóm ý tưởng sẽ được tách bạch rõ: (1) **đầu tư tăng trưởng** — chọn tài sản theo tốc độ tăng trưởng kỳ vọng; (2) **quỹ chỉ số và ETF** — mô phỏng một rổ tài sản thay vì chọn từng cổ phiếu; (3) **DCA (Dollar-Cost Averaging)** — một *cách giải ngân* theo lịch, không phải một *tài sản*. Nhầm lẫn giữa "tài sản" và "cách giải ngân" là nguồn gốc của nhiều khuyến cáo sai dạng "DCA luôn thắng". Cuối bài, worked example sẽ tính cụ thể bằng VND cho cả hai cách giải ngân trên ba kịch bản giá, kèm phí và phân tích độ nhạy.

**Kiến thức tiên quyết:** W09 — [Đầu tư giá trị: Graham và Buffett principles](./week-09-dau-tu-gia-tri.md) (để đối chiếu hai trường phái và hiểu vì sao "kỳ vọng tăng trưởng" đã được trả giá trong định giá); hữu ích thêm W04 (P/E, P/B, EPS) và W08 (đa dạng hóa, phân bổ tài sản).

---

## 2. Mục Tiêu Học Tập Đo Lường Được (Measurable Learning Objectives)

Sau khi hoàn thành bài học này, người học có năng lực:

- **`[LO-W10-01]` (Understand):** Định nghĩa và phân biệt đầu tư tăng trưởng (growth investing), đầu tư giá trị (value investing), quỹ chỉ số (index fund), ETF và DCA bằng thuật ngữ Việt-Anh đúng ngữ cảnh; giải thích vì sao "công ty tốt" và "cổ phiếu tốt (giá hợp lý)" là hai câu hỏi khác nhau, và vì sao ETF là một *hình thức* còn DCA là một *phương pháp giải ngân*.
- **`[LO-W10-02]` (Apply):** Tính số đơn vị mua được, giá vốn bình quân, giá trị cuối và tổng phí cho kế hoạch DCA và lump-sum bằng VND, với dữ liệu giả định cho trước, ghi đúng đơn vị, kỳ tính, biểu phí và quy tắc làm tròn.
- **`[LO-W10-03]` (Analyze):** Phân tích độ nhạy của kết quả DCA/lump-sum theo kịch bản giá (tăng dần, giảm rồi hồi phục, giảm sâu, đi ngang) và theo cấu trúc phí; phát hiện các kết luận quá mức dạng "DCA luôn thắng"; nêu rõ giới hạn về sai lệch theo dõi (tracking difference/error), phí và market timing.
- **`[LO-W10-04]` (Evaluate):** Phản biện một kế hoạch đầu tư định kỳ giả định (kể cả kế hoạch dùng ETF đòn bẩy) bằng bộ tiêu chí: chi phí, chỉ số tham chiếu, sai lệch theo dõi, kỷ luật lịch giải ngân, rủi ro thị trường còn tồn tại; phân định những gì bằng chứng đã kết luận và những gì chưa thể kết luận.

---

## 3. Tóm Lược Cốt Lõi và Fast-Track Core (30-45 Phút)

### 3.1. Bản Đồ Khái Niệm Nhanh (Core Summary)

Đầu tư tăng trưởng (growth investing) chọn tài sản theo tiêu chí tốc độ: tài liệu của SEC tại Investor.gov mô tả growth stock là cổ phiếu "có lợi nhuận tăng nhanh hơn mặt bằng thị trường, hiếm khi trả cổ tức, được mua với kỳ vọng tăng giá" `[W10-C01]`. Cái giá phải trả cho tốc độ đó là định giá cao và kỳ vọng đã nhúng sẵn trong giá — lịch sử cho thấy điều này không phải vé thắng miễn phí: Fama và French ghi nhận, trong giai đoạn 1975–1995, cổ phiếu giá trị (value) vượt cổ phiếu tăng trưởng (growth) ở 12 trong 13 thị trường lớn, với chênh lệch lợi nhuận trung bình 7,60%/năm giữa rổ giá trị và rổ tăng trưởng toàn cầu `[W10-C02]`. Quỹ chỉ số (index fund) và ETF đi theo hướng khác: không chọn cổ phiếu, mà mô phỏng cả một rổ theo chỉ số. ETF khác quỹ mở ở chỗ chứng chỉ quỹ được giao dịch trên sàn theo giá thị trường — giá này "có thể hơn hoặc kém" giá trị tài sản ròng (NAV), và cơ chế arbitrage thường giữ giá thị trường sát NAV cuối ngày `[W10-C03]`. DCA không phải tài sản mà là lịch giải ngân: mỗi kỳ chi một số tiền cố định, giá thấp mua được nhiều đơn vị hơn, giá cao mua ít hơn. Nhưng "mua nhiều khi giá thấp" không đồng nghĩa "kết quả cao hơn": nghiên cứu Vanguard trên dữ liệu 1976–2022 cho thấy lump-sum (dồn vốn một lần) vượt cost averaging/DCA khoảng hai phần ba số trường hợp, vì phần tiền chờ ngoài thị trường bị mất phần bù rủi ro; DCA không tạo kỳ vọng lợi nhuận cao hơn, chỉ tạm thời hạ rủi ro bằng cách giữ tiền mặt và có thể phù hợp với người cực kỳ ngại mất mát `[W10-C06]`.

### 3.2. Bảng So Sánh Đối Kháng Nhanh (Quick Comparative Table)

| Khái niệm / Mô hình | Bản chất cốt lõi | Biểu hiện thường gặp | Sai lầm phổ biến cần tránh | Nhãn bằng chứng |
| :--- | :--- | :--- | :--- | :---: |
| **Đầu tư tăng trưởng (Growth Investing)** | Chọn doanh nghiệp có tăng trưởng doanh thu/lợi nhuận kỳ vọng vượt mặt bằng; trả giá cao cho kỳ vọng đó | Chọn chỉ theo "ngành hot" không hỏi giá đã trả bao nhiêu cho kỳ vọng | Nhầm "công ty tốt" với "cổ phiếu tốt": kỳ vọng cao đã nhúng trong giá | `[W10-C01]` |
| **Đầu tư giá trị (Value Investing)** | Mua khi giá thấp hơn giá trị ước tính đủ xa (xem W09) | Định giá bề ngoài rẻ không hỏi "vì sao rẻ" | Coi "rẻ lịch sử" là vé thắng; quên rằng phần chênh có thể là phần bù rủi ro | `[W10-C02]` |
| **Quỹ chỉ số (Index Fund)** | Mô phỏng một rổ tài sản theo chỉ số tham chiếu, mua/chuộc theo NAV | Nhìn tên quỹ mà không đọc chỉ số tham chiếu và biểu phí | Cho rằng "thụ động" = không rủi ro thị trường | `[W10-C03]` `[W10-C04]` |
| **ETF (Exchange-Traded Fund)** | Chứng chỉ quỹ giao dịch trên sàn như cổ phiếu; giá thị trường có thể lệch NAV, arbitrage kéo giá về sát NAV | Coi mọi ETF giống nhau (kể cả ETF đòn bẩy) | Nhầm ETF đòn bẩy/inverse — chỉ đạt mục tiêu theo ngày — với ETF mô phỏng chỉ số thông thường | `[W10-C03]` `[W10-C05]` |
| **DCA (Dollar-Cost Averaging)** | Lịch giải ngân cố định theo kỳ, không đoán giá; giá vốn bình quân nằm giữa giá min-max | Kỳ vọng DCA luôn thắng lump-sum | Dùng DCA như "bùa may mắn"; bỏ qua rằng chờ đợi giải ngân cũng là một dạng chọn thời điểm | `[W10-C06]` `[W10-C07]` |

### 3.3. Năm Điểm Rút Ra Then Chốt (5 Key Takeaways)

1. **Growth, value, index là *chiến lược chọn tài sản*; DCA là *phương pháp giải ngân*.** Hai trục này độc lập: có thể DCA vào quỹ chỉ số, mua lump-sum cổ phiếu tăng trưởng, hoặc kết hợp tùy hoàn cảnh `[W10-C01]` `[W10-C06]`.
2. **Kỳ vọng tăng trưởng đã được trả giá trước.** Growth stock được định nghĩa bằng tốc độ kỳ vọng, không phải bằng lời hứa; lịch sử 1975–1995 cho thấy growth không tự động thắng value `[W10-C01]` `[W10-C02]`.
3. **ETF không phải một khối đồng nhất.** ETF mô phỏng chỉ số khác ETF đòn bẩy/inverse: loại sau chỉ nhắm mục tiêu theo ngày và có thể lệch xa mục tiêu khi giữ dài hạn `[W10-C03]` `[W10-C05]`.
4. **Chi phí làm giảm lợi nhuận ròng, và phần "chờ ngoài thị trường" cũng có giá.** Phí quỹ trừ trực tiếp vào lợi nhuận; với một khoản vốn sẵn có, DCA tạm thời giữ phần lớn vốn ở tiền mặt — đó là chi phí cơ hội `[W10-C04]` `[W10-C06]`.
5. **Không có kịch bản "luôn thắng".** Ví dụ Mục 6 cho thấy lump-sum thắng thị trường tăng, DCA thắng thị trường chữ V, cả hai cùng lỗ khi thị trường giảm sâu — kết quả phụ thuộc đường giá, không phụ thuộc khẩu hiệu `[W10-C07]`.

---

## 4. Mổ Xẻ Chuyên Sâu (Deep Dive: 2-3 Giờ)

### 4.1. Bản Chất Cơ Chế và Cơ Sở Khoa Học

**Đầu tư tăng trưởng và cái giá của kỳ vọng.** Investor.gov (SEC) phân loại cổ phiếu theo đặc điểm: growth stock "have earnings growing at a faster rate than the market average. They rarely pay dividends and investors buy them in the hope of capital appreciation" — lợi nhuận tăng nhanh hơn mặt bằng, hiếm trả cổ tức, mua vì kỳ vọng tăng giá; đối chiếu với value stock "have a low price-to-earnings (PE) ratio... may reflect the fact that they have fallen out of favor" `[W10-C01]`. Cơ chế rủi ro nằm ở chữ "kỳ vọng": giá growth đã chiết khấu trước một đường tăng trưởng lạc quan. Nếu kỳ vọng thành hiện thực, giá tiếp tục được bồi thường; nếu không, cả hai thành phần (tăng trưởng *và* định giá) cùng đảo chiều. Đó là lý do bài học W09 nhấn mạnh phân biệt "công ty tốt" và "cổ phiếu tốt": doanh nghiệp tuyệt vời mua ở giá đã trả quá cao cho tương lai vẫn có thể là khoản đầu tư kém.

**Bằng chứng lịch sử growth so với value — và giới hạn của nó.** Fama và French (bản làm việc đăng SSRN, sau này in trên *Journal of Finance*) tổng kết trên thị trường quốc tế: "Value stocks have higher returns than growth stocks in markets around the world. For 1975-95, the difference between the average returns on global portfolios of high and low book-to-market stocks is 7.60% per year, and value stocks outperform growth stocks in 12 of 13 major markets" `[W10-C02]`. Hai điểm cần đọc cẩn thận: (1) đây là **kết quả lịch sử** của một giai đoạn cụ thể, không phải quy luật bảo đảm; (2) chính nghiên cứu chỉ ra rằng international CAPM không giải thích được phần chênh, mà mô hình hai yếu tố có "risk factor for relative distress" (yếu tố rủi ro của doanh nghiệp khó khăn) mới bao trùm được value premium — nghĩa là phần chênh có thể là **phần bù rủi ro**, không phải tiền miễn phí. Kết luận học tập: "tăng trưởng cao" là tiêu chí có cơ sở, nhưng không phải vé tự động thắng; mua growth nghĩa là chấp nhận rủi ro kỳ vọng.

**Index fund và ETF: cấu trúc quyết định cách bạn mua.** Quỹ chỉ số theo đuổi mục tiêu mô phỏng một chỉ số tham chiếu. Quỹ mở bán/chuộc chứng chỉ theo NAV (giá trị tài sản ròng trên đơn vị); ETF thì chứng chỉ được niêm yết và giao dịch trên sàn suốt phiên. Investor Bulletin của SEC mô tả chính xác: ETF shares "are traded on a national stock exchange and at market prices that may or may not be the same as the net asset value ('NAV')" — giá thị trường có thể hơn hoặc kém NAV; giá này "is generally kept close to the ETF's end-of-day NAV because of the arbitrage function" — cơ chế arbitrage (qua Authorized Participants mua/tạo và chuộc theo lô lớn) giữ giá thị trường sát NAV `[W10-C03]`. Hệ quả thực hành: với ETF, nhà đầu tư lẻ chỉ mua/bán qua thị trường (chịu spread, thanh khoản, khớp lệnh), còn quỹ mở giao dịch theo NAV tính cuối ngày. "Thụ động" không đồng nghĩa "không rủi ro": quỹ chỉ số vẫn chịu **toàn bộ** biến động của rổ mà nó mô phỏng — nó loại rủi ro chọn sai cổ phiếu riêng lẻ, không loại rủi ro thị trường.

**Chi phí: biến số ít người chú ý nhất nhưng rõ ràng nhất.** Investor Bulletin của SEC về phí khẳng định thẳng: "Fees and expenses reduce the value of your fund's investment returns" và "A fund with higher costs must perform better than a lower-cost fund to generate the same returns for you" — quỹ phí cao phải làm tốt hơn mới chỉ *tạo được cùng* lợi nhuận `[W10-C04]`. Expense ratio (tỷ lệ chi phí hoạt động) là tổng chi phí vận hành thường niên tính bằng % tài sản ròng bình quân, trừ dần vào tài sản quỹ — nghĩa là nhà đầu tư trả phí mỗi ngày dù không thấy hóa đơn. Trong khi đó phần chi phí nằm ngoài biểu phí (chi phí giao dịch của quỹ, spread) cũng làm giảm giá trị đầu tư. Đây chính là nền tảng của **tracking difference**: lợi nhuận rổ quỹ ít khi bằng đúng lợi nhuận chỉ số, vì phí, chi phí giao dịch, chi phí giữ tiền mặt và cách tái lập rổ.

**DCA là cơ chế bình quân giá, không phải cơ chế dự đoán.** DCA cố định số tiền mỗi kỳ: số đơn vị mua được = số tiền / NAV kỳ đó; giá thấp → nhiều đơn vị, giá cao → ít đơn vị. Giá vốn bình quân = tổng tiền đã chi / tổng đơn vị, về mặt toán luôn nằm giữa giá cao nhất và thấp nhất từng kỳ mua (xem Mục 6). Câu hỏi quan trọng không phải "DCA mua rẻ hơn không" mà là "so với dồn vốn một lần, kết quả nào cao hơn?". Nghiên cứu Vanguard (2023) trên dữ liệu lịch sử và mô phỏng từ 1976–2022 kết luận: "historically, LS outperformed CA roughly two-thirds of the time" — lump-sum thắng cost averaging (tên khác của DCA) khoảng hai phần ba số trường hợp; với chỉ số toàn cầu MSCI World, lump-sum vượt CA 68% số kỳ (CA 3 tháng), trong khi cổ phiếu Mỹ vượt tiền mặt 76% số kỳ 1976–2022 `[W10-C06]`. Cơ chế được gọi tên thẳng: phần tiền chờ ngoài thị trường là "opportunity cost of lost risk premium" — chi phí cơ hội của phần bù rủi ro bị mất. Nhưng nghiên cứu cũng không bôi nhọ DCA: "Although the CA strategy won't, on average, produce higher returns, it is superior to remaining entirely in cash, and it temporarily lowers a portfolio's risk" — DCA tốt hơn giữ 100% tiền mặt, tạm thời hạ rủi ro bằng cách hạ tỷ trọng tài sản rủi ro, và phù hợp với nhà đầu tư cực kỳ ngại mất mát; Vanguard khuyến nghị nếu dùng thì giữ giai đoạn giải ngân ngắn (khoảng 3 tháng) để bớt chi phí cơ hội `[W10-C06]`. Lưu ý ranh giới: nghiên cứu xét tình huống **đã có sẵn một khoản vốn**; DCA định kỳ từ thu nhập hàng tháng (mỗi kỳ có tiền mới) là hoàn cảnh khác — khi đó "giải ngân theo lịch" là lựa chọn hiển nhiên chứ không phải một "chiến lược" cạnh tranh lump-sum.

**ETF đòn bẩy: ranh giới cần vẽ ngay từ đầu.** SEC cảnh báo rõ trong cùng bộ tài liệu: "leveraged ETFs seek to achieve performance equal to a multiple of an index after fees and expenses. These ETFs seek to achieve their investment objective on a daily basis only, potentially making them unsuitable for long-term investors" `[W10-C05]`. Cơ chế "reset theo ngày" cộng với hiệu ứng gộp (compounding) trong thị trường dao động làm hiệu suất tích lũy dài hạn lệch xa bội số tên gọi. Vì vậy DCA vào ETF đòn bẩy "để nhân đôi kết quả dài hạn" không phải là biến thể an toàn của DCA — đó là thay đổi cả tính chất rủi ro của kế hoạch.

### 4.2. Mổ Xẻ Chi Tiết Từng Thành Phần Của Mô Hình Worked Example

**Thành phần 1 — Lịch và cỡ kỳ.** DCA chỉ có nghĩa khi lịch giải ngân được cố định **trước** (số tiền, ngày mua, số kỳ). Đổi lịch theo tâm lý ("tăng tay khi thấy giá giảm") là đang nhét market timing vào trong DCA — hành vi mà nghiên cứu Vanguard cảnh báo: trì hoãn giải ngân bản thân nó là một dạng chọn thời điểm `[W10-C06]`.

**Thành phần 2 — Cấu trúc phí.** Ba lớp phí tác động khác nhau: (a) *phí giao dịch theo % giá trị lệnh* (ví dụ 0,15%) — với cùng tổng vốn, tổng phí này **không** phụ thuộc chia làm bao nhiêu lần mua; (b) *phí giao dịch cố định theo lệnh* (nếu có) — DCA nhiều lệnh sẽ tốn thêm, ví dụ phí cố định 10.000 VND/lệnh × 6 kỳ = 60.000 VND so với 10.000 VND của lump-sum; (c) *expense ratio* — tính theo thời gian nắm giữ, nên DCA (tiền vào dần) trả ít hơn lump-sum trong giai đoạn giải ngân, nhưng chênh lệch nhỏ so với tác động của đường giá `[W10-C04]` `[W10-C07]`.

**Thành phần 3 — Quy tắc làm tròn đơn vị.** Quỹ mở thường cho phép mua đơn vị lẻ và làm tròn xuống 2 chữ số thập phân; ETF giao dịch theo đơn vị nguyên và còn lệ thuộc khớp lệnh. Quy tắc làm tròn phải ghi tường minh trước khi tính, vì nó quyết định số đơn vị mỗi kỳ `[W10-C07]`.

**Thành phần 4 — Đại lượng so sánh và cách đọc kết quả.** So sánh luôn hai con số cùng chuẩn: (1) *giá trị cuối* = tổng đơn vị × NAV kỳ đánh giá; (2) *P/L* = giá trị cuối − tổng vốn. Giá vốn bình quân chỉ là thước đo trung gian; một kế hoạch DCA có thể có giá vốn bình quân thấp mà vẫn lỗ, nếu NAV cuối nằm dưới giá vốn bình quân (kịch bản C ở Mục 6).

### 4.3. Bối Cảnh Thực Tế và Ràng Buộc Đặc Thù

- **Bối cảnh Việt Nam:** cấu trúc quỹ chỉ số/ETF áp dụng được cho sản phẩm niêm yết tại HOSE/HNX và quỹ mở do công ty quản lý quỹ tại Việt Nam cung cấp; tuy nhiên biểu phí, lệ phí, thuế, chu kỳ khớp lệnh và thanh khoản phải lấy từ công bố chính thức của công ty quản lý quỹ, sàn giao dịch và cơ quan quản lý tại thời điểm sử dụng. Chi tiết tài khoản, loại lệnh, phí và thuế giao dịch tại Việt Nam là nội dung W11–W12; bài này cố tình không nêu số liệu thị trường thật để tránh tự chứng minh giả định bằng dữ liệu chưa kiểm tra.
- **Tách bạch dữ kiện - giả định - phép tính - diễn giải** (ràng buộc của môn học):

| Loại nội dung | Ví dụ trong bài | Trạng thái |
| :--- | :--- | :--- |
| Dữ kiện đã nguồn hóa | Định nghĩa growth/value stock; cấu trúc ETF và NAV; phí giảm lợi nhuận; ETF đòn bẩy theo ngày; lump-sum thắng DCA ~2/3; value premium lịch sử | Đã mở và đọc trực tiếp Investor.gov, SEC, Vanguard, SSRN (xem ledger `[W10-C01]`..`[W10-C06]`) |
| Giả định minh họa | Tổng vốn 120.000.000 VND; 6 kỳ × 20.000.000 VND; phí lệnh 0,15%; chuỗi NAV giả định 38.000–66.000 VND | Giả định phục vụ học tập, không phải dữ liệu thị trường thật |
| Phép tính | Bảng DCA vs lump-sum 3 kịch bản ở Mục 6 | Kiểm tra số học độc lập 2026-09-15 (`[W10-C07]`) |
| Diễn giải thực hành | Checklist thẩm định quỹ/ETF; quy tắc giữ lịch giải ngân | Đề xuất thực hành, không phải khuyến nghị đầu tư |

- **Ràng buộc độ bền bằng chứng:** các nguồn sơ cấp là tài liệu giáo dục của cơ quan quản lý (SEC/Investor.gov), một nghiên cứu học thuật (abstract trên SSRN) và nghiên cứu của một công ty quản lý tài sản (Vanguard, 1976–2022). Không có nghiên cứu nội địa Việt Nam về DCA so với lump-sum trong phạm vi bài; kết quả Vanguard là trung bình lịch sử của thị trường Mỹ và các thị trường lớn khác, không tự động chuyển về thị trường nào khác (giới hạn chi tiết ở Mục 7).

---

## 5. Nghiên Cứu Gốc và Mô Hình Lý Thuyết (Key Studies and Theoretical Models)

### Nguồn sơ cấp 1: Investor.gov (SEC) — định nghĩa growth stock và value stock
- **Tác giả và công bố:** SEC Office of Investor Education and Advocacy, trang "Stocks - FAQs" trên Investor.gov, https://www.investor.gov/introduction-investing/investing-basics/investment-products/stocks `[W10-C01]`.
- **Nội dung đã đối chiếu:** "Growth stocks have earnings growing at a faster rate than the market average. They rarely pay dividends and investors buy them in the hope of capital appreciation. A start-up technology company is likely to be a growth stock."; value stocks "have a low price-to-earnings (PE) ratio... their low PE ratio may reflect the fact that they have fallen out of favor with investors".
- **Ý nghĩa:** phân loại growth/value theo đặc điểm đo được (tốc độ lợi nhuận, P/E, cổ tức), không theo khẩu hiệu; nền tảng cho việc so sánh hai chiến lược.
- **Giới hạn:** mô tả khái quát cho thị trường Mỹ; là tài liệu giáo dục, không phải nghiên cứu hiệu quả chiến lược.

### Nguồn sơ cấp 2: Fama & French — value so với growth trên thị trường quốc tế
- **Tác giả và công bố:** Fama, E. F., & French, K. R. — "Value Versus Growth: The International Evidence", trang tóm tắt trên SSRN (bản làm việc đăng ký 1997; sau này in trên *The Journal of Finance* 53(6), 1998), https://papers.ssrn.com/sol3/papers.cfm?abstract_id=2358 `[W10-C02]`.
- **Nội dung đã đối chiếu (abstract):** "Value stocks have higher returns than growth stocks in markets around the world. For 1975-95, the difference between the average returns on global portfolios of high and low book-to-market stocks is 7.60% per year, and value stocks outperform growth stocks in 12 of 13 major markets. An international CAPM cannot explain the value premium, but a two-factor model that includes a risk factor for relative distress captures the value premium in international returns."
- **Ý nghĩa:** kỳ vọng tăng trưởng không tự động chuyển thành lợi nhuận cao hơn; phần chênh gắn với yếu tố rủi ro của doanh nghiệp khó khăn.
- **Giới hạn:** giai đoạn 1975–1995; trong phạm vi phiên kiểm chứng chỉ abstract được mở trực tiếp (không mở toàn văn bài in); thị trường Việt Nam không nằm trong mẫu.

### Nguồn sơ cấp 3: SEC Investor Bulletin — cấu trúc ETF
- **Tác giả và công bố:** SEC Office of Investor Education and Advocacy, "Investor Bulletin: Exchange-Traded Funds (ETFs)", https://www.sec.gov/investor/pubs/etfs.pdf `[W10-C03]`, `[W10-C05]`.
- **Nội dung đã đối chiếu:** ETF giao dịch trên sàn "at market prices that may or may not be the same as the net asset value ('NAV')"; cơ chế arbitrage giữ "market price... generally... close to the ETF's end-of-day NAV"; nhà đầu tư lẻ chỉ mua/bán qua thị trường, Authorized Participants mới mua/chuộc trực tiếp; ETF mô phỏng chỉ số là loại chủ đạo; "leveraged ETFs... seek to achieve their investment objective on a daily basis only, potentially making them unsuitable for long-term investors".
- **Ý nghĩa:** cấu trúc ETF quyết định cách mua (giá thị trường, spread, thanh khoản) khác quỹ mở (NAV); ETF không đồng nhất — cần đọc prospectus từng sản phẩm.
- **Giới hạn:** tài liệu của cơ quan quản lý Mỹ (1940 Act); mô tả cấu trúc, không đánh giá hiệu quả từng sản phẩm cụ thể.

### Nguồn sơ cấp 4: SEC Investor Bulletin — phí và chi phí quỹ
- **Tác giả và công bố:** SEC Office of Investor Education and Assistance, "Mutual Fund and ETF Fees and Expenses – Investor Bulletin" (cập nhật 2025-07-23), https://www.investor.gov/introduction-investing/general-resources/news-alerts/alerts-bulletins/investor-bulletins/mutual-fund-and-etf-fees-and-expenses-investor-bulletin `[W10-C04]`.
- **Nội dung đã đối chiếu:** "Fees and expenses reduce the value of your fund's investment returns"; "A fund with higher costs must perform better than a lower-cost fund to generate the same returns for you"; expense ratio = "Total Annual Fund Operating Expenses (Expressed as a % of the fund's average net assets)"; các chi phí ngoài biểu phí (chi phí giao dịch của quỹ) cũng làm giảm giá trị đầu tư.
- **Ý nghĩa:** chi phí là biến số kiểm soát được và tác động một chiều (giảm lợi nhuận ròng); nền tảng định nghĩa tracking difference.
- **Giới hạn:** mô tả loại phí theo quy định Mỹ; tên và mức phí cụ thể của từng quỹ/ETF phải tra prospectus của chính sản phẩm đó.

### Nguồn sơ cấp 5: Vanguard Research — DCA so với lump-sum
- **Tác giả và công bố:** Finlay, M., & Zorn, J. (2023). *Cost averaging: Invest now or temporarily hold your cash?* [Vanguard research, February 2023]. Vanguard Group. https://corporate.vanguard.com/content/dam/corp/research/pdf/cost_averaging_invest_now_or_temporarily_hold_your_cash.pdf `[W10-C06]`.
- **Nội dung đã đối chiếu:** "We find that historically, LS outperformed CA roughly two-thirds of the time"; 1976–2022 cổ phiếu Mỹ vượt tiền mặt 76% số kỳ, trái phiếu 68%; với MSCI World, lump-sum vượt CA (3 tháng) 68% số kỳ; "Although the CA strategy won't, on average, produce higher returns, it is superior to remaining entirely in cash, and it temporarily lowers a portfolio's risk"; "most investors — particularly those without significant aversion to loss — should invest a lump sum immediately"; với người rất ngại mất mát, CA có thể chấp nhận được nhưng nên "keeping a relatively short CA period, such as three months"; CA còn được gọi là dollar-cost averaging.
- **Ý nghĩa:** cơ chế chi phí cơ hội của phần tiền chờ ngoài thị trường; DCA là công cụ quản lý rủi ro tâm lý, không phải công cụ tăng kỳ vọng lợi nhuận.
- **Giới hạn:** dữ liệu 1976–2022 của các thị trường lớn (Mỹ, Anh, Úc, Canada, EU — bảng tổng hợp toàn cầu dùng MSCI World); không có dữ liệu thị trường Việt Nam; kết quả là trung bình lịch sử, không phải dự báo; nghiên cứu do một công ty quản lý tài sản có lợi ích liên quan đến chủ đề đầu tư thụ động thực hiện.

### Ghi chú nguồn liên quan
Bài nghiên cứu tiền nhiệm Shtekhman, Tasopoulos và Wimmer (2012), *"Dollar-cost averaging just means taking risk later"* (Vanguard) được Vanguard 2023 trích dẫn trong danh mục tài liệu tham khảo; trong phiên kiểm chứng này chưa mở trực tiếp bản gốc nên chỉ liệt kê như nguồn mở rộng, không gắn claim riêng.

---

## 6. Ví Dụ Giải Từng Bước (Worked Examples)

### Ví dụ 1: DCA so với lump-sum cho 120 triệu VND — 6 kỳ, 3 kịch bản giá (hoàn toàn giả định)

- **Giai đoạn 1: Bối cảnh và Đề bài (Problem Formulation).**
  Bạn (nhân vật giả định) có sẵn 120.000.000 VND và đang cân nhắc hai cách đưa vốn vào "Quỹ chỉ số Q" — quỹ tưởng tượng, không trùng sản phẩm nào thực tế: (i) **LSI** (lump-sum): mua toàn bộ 120 triệu ngay đầu tháng 1; (ii) **DCA**: chia 6 kỳ, mỗi kỳ 20 triệu, mua vào đầu mỗi tháng 1→6. Đánh giá kết quả ở đầu tháng 7, ngay sau kỳ mua thứ 6. Câu hỏi đề bài: kịch bản giá nào làm chiến lược nào thắng, và phí ảnh hưởng thế nào?

- **Bảng Giả Định (Assumptions Table) — giả định minh họa phục vụ học tập, không phải dữ liệu thực tế:**

  | Tham số | Ký hiệu | Giá trị giả định | Căn cứ thực tế và ghi chú |
  | :--- | :---: | :---: | :--- |
  | Tổng vốn | W | 120.000.000 VND | Số đề bài, giả định minh họa |
  | Số kỳ / cỡ kỳ DCA | n | 6 kỳ × 20.000.000 VND, đầu mỗi tháng | Giả định minh họa; Vanguard 2023 khuyến nghị giai đoạn giải ngân ngắn (≈3 tháng) nếu dùng CA `[W10-C06]` — 6 kỳ chọn để bảng dễ đọc |
  | Phí giao dịch mua | f | 0,15%/giá trị lệnh, khấu trừ khi mua | Giả định minh họa; biểu phí thật của từng quỹ/ETF phải tra công bố chính thức |
  | Phí quản lý quỹ | - | 0,60%/năm, đã trừ dần vào NAV | Giả định minh họa; vì đã nằm trong NAV nên không mô hình hóa riêng |
  | NAV chuỗi kịch bản A | P_A | 50.000 → 52.000 → 55.000 → 58.000 → 62.000 → 66.000 VND/đơn vị | Giả định "tăng dần" |
  | NAV chuỗi kịch bản B | P_B | 50.000 → 46.000 → 42.000 → 44.000 → 48.000 → 52.000 VND/đơn vị | Giả định "giảm rồi hồi phục" |
  | NAV chuỗi kịch bản C | P_C | 50.000 → 46.000 → 42.000 → 40.000 → 40.000 → 38.000 VND/đơn vị | Giả định "giảm sâu" (stress test) |
  | Quy tắc làm tròn | - | Số đơn vị mỗi lần mua làm tròn xuống 2 chữ số thập phân; giá trị cuối làm tròn đến đồng | Quy ước của bài học |

- **Giai đoạn 2: Phân tích và Lựa chọn Mô hình (Conceptual Deconstruction).**
  Mô hình là phép tính giải ngân trực tiếp: tiền mua thực tế mỗi kỳ = 20.000.000 × (1 − 0,0015) = **19.970.000 VND** (phí lệnh 30.000 VND); số đơn vị mua = 19.970.000 / NAV_kỳ; tổng đơn vị DCA = cộng dồn 6 kỳ; tổng đơn vị LSI = (120.000.000 × 0,9985) / NAV tháng 1 = 119.820.000 / NAV₁. Giá trị cuối = tổng đơn vị × NAV kỳ 6. Đáng chú ý về mặt cấu trúc: phí lệnh theo % của tổng vốn **không phụ thuộc** số lần mua (0,15% × 120 triệu = 180.000 VND cho cả hai cách) — vì vậy trong ví dụ này phí lệnh không tạo chênh lệch giữa hai chiến lược; điều tạo chênh lệch là **đường giá** `[W10-C07]`.

- **Giai đoạn 3: Thực hiện Từng bước (Step-by-Step Execution).**

  **Kịch bản A — NAV tăng dần (50.000 → 66.000):**

  | Kỳ | NAV (VND/đơn vị) | Tiền mua thực tế (VND) | Đơn vị mua được |
  | :---: | :---: | :---: | :---: |
  | 1 | 50.000 | 19.970.000 | 399,40 |
  | 2 | 52.000 | 19.970.000 | 384,03 |
  | 3 | 55.000 | 19.970.000 | 363,09 |
  | 4 | 58.000 | 19.970.000 | 344,31 |
  | 5 | 62.000 | 19.970.000 | 322,09 |
  | 6 | 66.000 | 19.970.000 | 302,57 |

  - DCA: tổng đơn vị **2.115,49**; giá vốn bình quân = 120.000.000 / 2.115,49 ≈ **56.724 VND/đơn vị**; giá trị cuối = 2.115,49 × 66.000 = **139.622.340 VND**; P/L = **+19.622.340 VND (+16,4%)**.
  - LSI: đơn vị = 119.820.000 / 50.000 = **2.396,40**; giá trị cuối = **158.162.400 VND**; P/L = **+38.162.400 VND (+31,8%)**.
  - **Kết quả: LSI cao hơn DCA 18.540.060 VND** — thị trường tăng một mạch thì mua sớm toàn bộ có lợi `[W10-C07]`.

  **Kịch bản B — giảm rồi hồi phục (50.000 → 42.000 → 52.000):**

  | Kỳ | NAV (VND/đơn vị) | Tiền mua thực tế (VND) | Đơn vị mua được |
  | :---: | :---: | :---: | :---: |
  | 1 | 50.000 | 19.970.000 | 399,40 |
  | 2 | 46.000 | 19.970.000 | 434,13 |
  | 3 | 42.000 | 19.970.000 | 475,47 |
  | 4 | 44.000 | 19.970.000 | 453,86 |
  | 5 | 48.000 | 19.970.000 | 416,04 |
  | 6 | 52.000 | 19.970.000 | 384,03 |

  - DCA: tổng đơn vị **2.562,93**; giá vốn bình quân ≈ **46.821 VND/đơn vị**; giá trị cuối = **133.272.360 VND**; P/L = **+13.272.360 VND (+11,1%)**.
  - LSI: 2.396,40 đơn vị; giá trị cuối = 2.396,40 × 52.000 = **124.612.800 VND**; P/L = **+4.612.800 VND (+3,8%)**.
  - **Kết quả: DCA cao hơn LSI 8.659.560 VND** — cơ chế mua nhiều đơn vị ở vùng đáy (475,47 đơn vị ở NAV 42.000) làm giá vốn bình quân thấp hơn `[W10-C07]`.

  **Kịch bản C — giảm sâu (50.000 → 38.000, stress test):**

  | Kỳ | NAV (VND/đơn vị) | Tiền mua thực tế (VND) | Đơn vị mua được |
  | :---: | :---: | :---: | :---: |
  | 1 | 50.000 | 19.970.000 | 399,40 |
  | 2 | 46.000 | 19.970.000 | 434,13 |
  | 3 | 42.000 | 19.970.000 | 475,47 |
  | 4 | 40.000 | 19.970.000 | 499,25 |
  | 5 | 40.000 | 19.970.000 | 499,25 |
  | 6 | 38.000 | 19.970.000 | 525,52 |

  - DCA: tổng đơn vị **2.833,02**; giá vốn bình quân ≈ **42.358 VND/đơn vị**; giá trị cuối = **107.654.760 VND**; P/L = **−12.345.240 VND (−10,3%)**.
  - LSI: 2.396,40 đơn vị; giá trị cuối = 2.396,40 × 38.000 = **91.063.200 VND**; P/L = **−28.936.800 VND (−24,1%)**.
  - **Kết quả: cả hai đều lỗ; DCA lỗ ít hơn** (chênh 16.591.560 VND) — nhưng lưu ý "ít lỗ hơn" vẫn là **lỗ**: DCA không loại bỏ rủi ro thị trường `[W10-C06]` `[W10-C07]`.

  **Kịch bản đối chứng D — NAV đi ngang (50.000 suốt 6 kỳ):** DCA mua đúng 399,40 × 6 = **2.396,40 đơn vị** — bằng chính xác LSI; giá trị cuối hai bên cùng 119.820.000 VND, cùng mất 180.000 VND phí. Thị trường ngang không phân biệt hai chiến lược khi phí là phần trăm giá trị lệnh `[W10-C07]`.

- **Giai đoạn 4: Đúc kết Bài học và Phân tích Độ nhạy (Takeaway và Sensitivity Analysis).**
  (1) **Kết quả là hàm của đường giá:** cùng tổng vốn, cùng phí, thứ hạng DCA/LSI lật ngược giữa kịch bản A (LSI +31,8% vs DCA +16,4%) và kịch bản B (DCA +11,1% vs LSI +3,8%) — mô hình hóa đúng kết luận của Vanguard rằng lump-sum thắng khoảng 2/3 số trường hợp trên dữ liệu dài hạn, vì thị trường có xu hướng đi lên nhiều hơn đi xuống `[W10-C06]` `[W10-C07]`. (2) **Giá vốn bình quân của DCA luôn nằm giữa giá min-max của các kỳ mua** (56.724 ∈ [50.000; 66.000]; 46.821 ∈ [42.000; 52.000]; 42.358 ∈ [38.000; 50.000]) — bình quân giá giúp "mua rẻ hơn giá trung bình" nhưng **không bảo đảm lãi**: kịch bản C có giá vốn 42.358 VND vẫn lỗ 10,3% vì NAV cuối 38.000 VND. (3) **Độ nhạy theo phí:** phí theo % giá trị lệnh không tạo chênh lệch (180.000 VND cả hai cách); nếu thay bằng phí cố định 10.000 VND/lệnh, DCA trả 60.000 VND so với 10.000 VND của LSI — chênh 50.000 VND (≈0,04% vốn), nhỏ so với tác động đường giá nhưng tường minh phải ghi. Phí quản lý 0,60%/năm: DCA giữ vốn trung bình 3,5 tháng trên cửa sổ 6 tháng so với 6 tháng của LSI, ước tính phí quản lý tương ứng ≈210.000 VND so với ≈360.000 VND — chênh lệch nhỏ, không đảo ngược kết luận nào `[W10-C04]` `[W10-C07]`. (4) **Quy tắc hành vi:** DCA phù hợp khi tiền đến theo thu nhập định kỳ hoặc khi người học tự nhận là cực kỳ ngại mất mát và dễ bỏ cuộc sau một cú giảm; nếu vốn đã sẵn có và chấp nhận rủi ro mục tiêu, bằng chứng hiện tại thiên về giải ngân sớm, giai đoạn DCA (nếu có) nên ngắn `[W10-C06]`. (5) **Giới hạn mô hình:** chưa gồm thuế, spread và thanh khoản (đặc biệt với ETF), giả định mua đúng NAV (ETF lẻ mua theo giá thị trường có thể lệch NAV `[W10-C03]`), chuỗi NAV 6 điểm là thô so với dao động thật; kết quả chỉ dùng để học cơ chế, không dùng cho tiền thật `[W10-C07]`.

### Bài tập tự luyện ngắn (giải ở phần gợi ý cuối mục)
1. Kịch bản A, nếu phí giao dịch bằng 0: số đơn vị LSI và DCA kỳ 1 là bao nhiêu, và kết luận "LSI cao hơn" có đổi chiều không?
2. Tính bằng tay giá vốn bình quân của kịch bản B (tổng tiền 120.000.000 / tổng đơn vị 2.562,93) và đối chiếu với con số 46.821 VND/đơn vị.

**Gợi ý:** (1) LSI = 120.000.000/50.000 = 2.400,00 đơn vị; DCA kỳ 1 = 400,00 đơn vị; tổng phí về 0, cả hai bên tăng kết quả nhẹ (~0,15% vốn) nhưng chênh lệch giữa hai chiến lược vẫn do đường giá — kết luận không đổi. (2) 120.000.000 / 2.562,93 ≈ 46.820,8 → làm tròn 46.821 VND/đơn vị, khớp bảng.

---

## 7. Điều Đã Sáng Tỏ và Phần Còn Bất Định (What We Know vs What Remains Uncertain)

### 7.1. Những Điều Đã Được Xác Minh Vững Chắc 🟢
- Định nghĩa growth stock/value stock được phát biểu rõ bằng đặc điểm đo được trong tài liệu chính thức của SEC: lợi nhuận tăng nhanh hơn mặt bằng, hiếm cổ tức, mua vì tăng giá; value stock có P/E thấp `[W10-C01]`.
- ETF giao dịch trên sàn theo giá thị trường có thể lệch NAV; cơ chế arbitrage thường giữ giá sát NAV cuối ngày; nhà đầu tư lẻ chỉ giao dịch qua thị trường `[W10-C03]`.
- Phí và chi phí quỹ làm giảm giá trị lợi nhuận của nhà đầu tư; quỹ phí cao phải làm tốt hơn mới chỉ bằng quỹ phí thấp; expense ratio tính theo % tài sản ròng bình quân `[W10-C04]`.
- ETF đòn bẩy chỉ nhắm mục tiêu theo ngày và "có thể không phù hợp với nhà đầu tư dài hạn" theo đúng nguyên văn SEC `[W10-C05]`.
- Trên dữ liệu 1976–2022, lump-sum vượt cost averaging/DCA khoảng hai phần ba số trường hợp; DCA không tạo kỳ vọng lợi nhuận cao hơn nhưng tạm thời hạ rủi ro, tốt hơn giữ 100% tiền mặt, và nên có giai đoạn giải ngân ngắn nếu dùng `[W10-C06]`.
- Giai đoạn 1975–1995: value vượt growth ở 12/13 thị trường lớn, chênh trung bình 7,60%/năm, và phần chênh gắn với yếu tố rủi ro "relative distress" `[W10-C02]`.

### 7.2. Những Vùng Xám Cần Cẩn Trọng 🔴 / 🟠
- **"DCA luôn thắng lump-sum"** 🔴: mâu thuẫn trực tiếp với bằng chứng đã đọc — kết quả phụ thuộc đường giá; DCA là công cụ quản lý rủi ro thời điểm và tâm lý, không phải công cụ tăng kỳ vọng lợi nhuận `[W10-C06]`.
- **Áp dụng số liệu vào thị trường Việt Nam** 🟠: nghiên cứu Vanguard không gồm dữ liệu Việt Nam; phí, thuế, thanh khoản và chu kỳ giao dịch của quỹ/ETF tại Việt Nam phải tra công bố chính thức (nội dung W11–W12); chưa có nghiên cứu DCA vs lump-sum nội địa trong phạm vi bài.
- **Value premium là kết quả lịch sử** 🟠: giai đoạn 1975–1995 không tự động lặp lại; chính mô hình của Fama-French chỉ ra phần chênh gắn với rủi ro doanh nghiệp khó khăn — mua value không phải "tiền miễn phí" `[W10-C02]`.
- **Tracking difference/error của từng sản phẩm cụ thể** 🔴: bài học chỉ định nghĩa khung (lợi nhuận quỹ lệch lợi nhuận chỉ số do phí, chi phí giao dịch, cách tái lập rổ; tracking error đo *biến động* của chênh lệch đó); con số của từng ETF phải lấy từ công bố của chính quỹ, không suy ra từ tên gọi `[W10-C03]` `[W10-C04]`.
- **"Phí bao nhiêu là thấp"** 🔴: nguồn sơ cấp không công bố một ngưỡng chuẩn; so sánh luôn trong cùng loại tài sản và cùng chỉ số tham chiếu.
- **Ngưỡng chịu mất mát của bản thân** 🟠: mức độ "ngại mất mát" nào đủ để chọn DCA là quyết định cá nhân; nghiên cứu chỉ khẳng định xu hướng theo persona, không có ngưỡng phổ quát `[W10-C06]`.

---

## 8. Sai Lầm Phổ Biến và Phản Biện Pop Literature (Common Mistakes and Myths Debunked)

| Sai lầm phổ biến / Lời khuyên thường thức | Bản chất ngụy biện / Nguy cơ thực tế | Căn cứ khoa học phản biện | Hành vi đúng đắn thay thế |
| :--- | :--- | :--- | :--- |
| **Hiểu lầm 1:** "DCA luôn thắng lump-sum, mua dần là chắc chắn có lời." | Kết quả phụ thuộc đường giá; ví dụ Mục 6 cho thấy LSI thắng khi thị trường tăng một mạch, cả hai cùng lỗ khi thị trường giảm sâu | Nghiên cứu Vanguard 2023: LS thắng ~2/3 số trường hợp 1976–2022 `[W10-C06]`; worked example `[W10-C07]` | Xác định nguồn tiền (vốn sẵn hay thu nhập định kỳ), mức độ ngại mất mát; nếu vốn sẵn và chấp nhận rủi ro, cân nhắc giải ngân sớm, giai đoạn DCA ngắn nếu dùng |
| **Hiểu lầm 2:** "ETF nào cũng như ETF nào — dùng ETF đòn bẩy 2x cho DCA dài hạn để nhân đôi kết quả." | ETF đòn bẩy/inverse chỉ đạt mục tiêu theo ngày; hiệu suất tích lũy dài hạn lệch xa bội số tên gọi | SEC: "on a daily basis only, potentially making them unsuitable for long-term investors" `[W10-C05]` | Đọc prospectus; phân loại ETF thường (mô phỏng chỉ số) và ETF đòn bẩy/inverse trước khi đưa vào kế hoạch dài hạn |
| **Hiểu lầm 3:** "Quỹ chỉ số luôn bám sát chỉ số, không cần xem phí." | Lợi nhuận quỹ lệch lợi nhuận chỉ số do expense ratio, chi phí giao dịch, tiền mặt dư — đó là tracking difference | SEC: phí giảm giá trị lợi nhuận; quỹ phí cao phải làm tốt hơn mới bằng `[W10-C04]`; giá ETF có thể lệch NAV `[W10-C03]` | So sánh expense ratio, chênh lệch so với chỉ số (tracking difference) và thanh khoản từ công bố chính thức của từng quỹ |
| **Hiểu lầm 4:** "Mua quỹ chỉ số là hết rủi ro vì đã đa dạng hóa." | Quỹ chỉ số loại rủi ro chọn cổ phiếu riêng lẻ, nhưng vẫn chịu **toàn bộ** biến động của rổ — kịch bản C ở Mục 6 lỗ 10-24% | Cấu trúc ETF chỉ mô phỏng rổ `[W10-C03]`; stress test `[W10-C07]` | Quản lý rủi ro ở tầng phân bổ tài sản (W08), không ở tầng "mua index là an toàn" |
| **Hiểu lầm 5:** "Công ty tăng trưởng hot thì cổ phiếu nào cũng đáng mua." | Kỳ vọng tăng trưởng đã nhúng trong giá; khi kỳ vọng không đạt, cả tăng trưởng lẫn định giá cùng đảo chiều; lịch sử growth không tự động thắng value | Định nghĩa growth stock `[W10-C01]`; Fama-French 1975–95 `[W10-C02]` | Hỏi "mình đang trả bao nhiêu cho kỳ vọng này" — nối về khung giá trị/giá của W09 trước khi quyết định |

---

## 9. Ôn Tập Giữa Tuần Tương Tác (Mid-Week Mini-Reviews)

### Đợt 1 (Ngày 3): Kiểm tra Nhận diện Khái niệm
<details>
<summary><b>Câu hỏi 1: Vì sao nghiên cứu Vanguard kết luận lump-sum thường thắng DCA trên dữ liệu dài hạn, và trong điều kiện nào DCA vẫn có chỗ đứng hợp lý?</b></summary>

> **Gợi ý tự kiểm tra:**
> - Cơ chế trung tâm: phần tiền chờ ngoài thị trường bị mất phần bù rủi ro (opportunity cost of lost risk premium); lịch sử 1976–2022, tài sản rủi ro vượt tiền mặt phần lớn thời gian (cổ phiếu Mỹ 76% số kỳ).
> - Số liệu cốt lõi: LS thắng CA khoảng hai phần ba số trường hợp; CA không tạo kỳ vọng lợi nhuận cao hơn.
> - Chỗ đứng của DCA: tạm thời hạ rủi ro danh mục, tốt hơn giữ 100% tiền mặt, giảm nguy cơ bỏ cuộc sau cú giảm ở người cực kỳ ngại mất mát; nếu dùng thì giai đoạn giải ngân nên ngắn (~3 tháng).
> - Ranh giới: DCA từ thu nhập định kỳ là hoàn cảnh khác — không có "lump-sum sẵn" để so sánh.
> *Đọc lại mục:* [Mục 4.1](#41-bản-chất-cơ-chế-và-cơ-sở-khoa-học) và [Mục 6](#6-ví-dụ-giải-từng-bước-worked-examples).
</details>

### Đợt 2 (Ngày 5): Kiểm tra Vận dụng Tình huống
<details>
<summary><b>Câu hỏi 2: Một người bạn khuyên: "Vốn 120 triệu nên chia 6 tháng DCA vào một ETF đòn bẩy 2x — vừa bình quân giá vừa nhân đôi lợi nhuận." Hãy phản biện kế hoạch này bằng các bằng chứng của bài học.</b></summary>

> **Gợi ý tự kiểm tra:**
> - Lớp 1 — sản phẩm: ETF đòn bẩy chỉ nhắm mục tiêu theo ngày; giữ dài hạn hiệu suất có thể lệch xa 2x do cơ chế reset và gộp; SEC cảnh báo có thể không phù hợp cho nhà đầu tư dài hạn `[W10-C05]` — "nhân đôi lợi nhuận" là giả định sai ngay từ gốc.
> - Lớp 2 — phương pháp giải ngân: DCA là quản lý rủi ro, không phải công cụ tăng lợi nhuận kỳ vọng; Vanguard: LS thắng ~2/3; nếu người bạn vốn ngại mất mát thì DCA *ETF thường* vẫn là lựa chọn có cơ sở, nhưng DCA *ETF đòn bẩy* thì không `[W10-C06]`.
> - Lớp 3 — cấu trúc sản phẩm: ETF mua qua sàn theo giá thị trường có thể lệch NAV; kiểm tra expense ratio, tracking difference, thanh khoản từ công bố chính thức `[W10-C03]` `[W10-C04]`.
> - Lớp 4 — rủi ro còn lại: ngay cả ETF thường cũng chịu toàn bộ biến động rổ (kịch bản C lỗ); đòn bẩy khuếch đại cả hai chiều.
> *Đọc lại mục:* [Mục 4.1](#41-bản-chất-cơ-chế-và-cơ-sở-khoa-học) và [Mục 8](#8-sai-lầm-phổ-biến-và-phản-biện-pop-literature-common-mistakes-and-myths-debunked).
</details>

---

## 10. Bài Tập Thực Hành và Tiêu Chí Tự Đánh Giá (Practical Exercises and Rubric)

### Hướng Dẫn Lựa Chọn Bài Tập (An Toàn và Tự Nguyện)
- **Lựa chọn A (Tự quan sát cá nhân):** trong 7 ngày tới, ghi nhật ký "lịch giải ngân" — liệt kê các khoản tiền có thể đầu tư của mình theo nguồn (vốn sẵn có, thu nhập định kỳ) và tự phân loại: khoản nào thuộc tình huống lump-sum-vs-DCA, khoản nào vốn dĩ là DCA từ thu nhập. Không cần dùng tiền thật, không cần giao dịch.
- **Lựa chọn B (Kịch bản giả định):** dùng lại ví dụ Mục 6 nhưng thay chuỗi NAV bằng chuỗi giả định tự thiết kế (6 điểm, bất kỳ hướng nào); tính tổng đơn vị, giá vốn bình quân và giá trị cuối cho cả DCA và LSI theo đúng quy tắc làm tròn; viết 1 đoạn 5 câu kết luận kèm bảng giả định. Ghi rõ tất cả là giả định minh họa.

### Tiêu Chí Tự Đánh Giá Năng Lực (Self-Assessment Rubric)

| Tiêu chí đánh giá | Chưa đạt (Emerging) | Đang phát triển (Developing) | Đạt chuẩn (Proficient) | Xuất sắc (Advanced) |
| :--- | :--- | :--- | :--- | :--- |
| **Phân biệt khái niệm** | Dùng lẫn growth/value/index/DCA. | Phân biệt được nhưng thiếu thuộc tính. | Định nghĩa đúng growth (tốc độ kỳ vọng), index/ETF (mô phỏng rổ, giao dịch trên sàn), DCA (lịch giải ngân) `[W10-C01]` `[W10-C03]`. | Giải thích được vì sao hai trục "chọn tài sản" và "cách giải ngân" độc lập và kết hợp được. |
| **Tính toán DCA/LSI** | Chỉ có kết quả không nói giả định. | Tính đúng số đơn vị nhưng sai giá vốn hoặc phí. | Đủ số đơn vị từng kỳ, giá vốn bình quân, giá trị cuối, phí; đúng đơn vị VND và quy tắc làm tròn `[W10-C07]`. | Tự phát hiện thêm tham số (phí cố định theo lệnh, thuế, spread) và định lượng tác động. |
| **Phân tích độ nhạy và giới hạn** | Kết luận "DCA luôn thắng". | Nhắc được kịch bản nhưng không lượng hóa. | Chỉ ra thứ hạng lật ngược theo đường giá; nêu tracking difference/error, phí, market timing `[W10-C06]` `[W10-C07]`. | Kết nối với giới hạn áp dụng vào thị trường Việt Nam và ranh giới bằng chứng của từng nguồn. |
| **Phản biện kế hoạch đầu tư** | Chấp nhận lời khuyên theo cảm tính. | Nhận diện được một lỗi trong kế hoạch. | Phản biện kế hoạch ETF đòn bẩy/DCA bằng bộ tiêu chí 4 lớp (sản phẩm, giải ngân, cấu trúc, rủi ro) `[W10-C05]`. | Viết được danh mục "đã kết luận/chưa thể kết luận" cho từng luận điểm của kế hoạch. |

---

## 11. Bảng Thuật Ngữ Chuẩn Hóa (Weekly Glossary)

### 11.1. Thuật ngữ song ngữ

| Thuật ngữ Tiếng Việt | Thuật ngữ Tiếng Anh | Định nghĩa khoa học cô đọng |
| :--- | :--- | :--- |
| **Đầu tư tăng trưởng** | *Growth Investing* | Chiến lược chọn tài sản theo tiêu chí tốc độ tăng trưởng doanh thu/lợi nhuận kỳ vọng vượt mặt bằng; chấp nhận định giá cao vì kỳ vọng đó. |
| **Cổ phiếu tăng trưởng** | *Growth Stock* | Cổ phiếu có lợi nhuận tăng nhanh hơn mặt bằng thị trường, hiếm khi trả cổ tức, được mua với kỳ vọng tăng giá (capital appreciation). |
| **Đầu tư giá trị** | *Value Investing* | Chiến lược mua khi giá thấp hơn giá trị nội tại ước tính đủ lớn làm đệm (xem W09); cổ phiếu giá trị thường có P/E thấp. |
| **Quỹ chỉ số** | *Index Fund* | Quỹ đầu tư theo mục tiêu mô phỏng hiệu suất một chỉ số tham chiếu thay vì chọn cổ phiếu riêng lẻ; quỹ mở giao dịch theo NAV. |
| **Quỹ hoán đổi danh mục** | *Exchange-Traded Fund (ETF)* | Chứng chỉ quỹ được niêm yết và giao dịch trên sàn như cổ phiếu; giá thị trường có thể lệch NAV, cơ chế arbitrage thường giữ giá sát NAV cuối ngày. |
| **Đầu tư thụ động** | *Passive Investing* | Tiếp cận thị trường bằng cách mô phỏng rổ tham chiếu thay vì chọn cổ phiếu/dự báo thời điểm; vẫn chịu toàn bộ biến động của rổ. |
| **Bình quân giá / đầu tư định kỳ số tiền cố định** | *Dollar-Cost Averaging (DCA)* | Giải ngân một số tiền cố định theo lịch định kỳ, không quan tâm giá; giá vốn bình quân nằm giữa giá cao nhất và thấp nhất các kỳ mua. |
| **Đầu tư một lần (dồn vốn)** | *Lump-Sum Investing (LSI)* | Đưa toàn bộ khoản vốn sẵn có vào thị trường ngay một lần; lịch sử dài hạn thường cho kết quả trung bình cao hơn cost averaging. |
| **Bình quân chi phí** | *Cost Averaging (CA)* | Tên gọi khác của DCA trong nghiên cứu Vanguard; chỉ khác chữ kỹ thuật, cùng cơ chế giải ngân theo kỳ. |
| **Giá trị tài sản ròng** | *Net Asset Value (NAV)* | Giá trị tài sản quỹ trừ nợ phải trả, chia số đơn vị lưu hành; quỹ mở mua/chuộc theo NAV, ETF giao dịch theo giá thị trường. |
| **Tỷ lệ chi phí hoạt động** | *Expense Ratio (TER)* | Tổng chi phí vận hành thường niên của quỹ tính bằng % tài sản ròng bình quân, trừ dần vào tài sản quỹ và làm giảm lợi nhuận nhà đầu tư. |
| **Sai lệch theo dõi** | *Tracking Difference* | Chênh lệch lợi nhuận giữa quỹ và chỉ số tham chiếu trong một kỳ (độ lớn của lệch); bắt nguồn từ phí, chi phí giao dịch và cách tái lập rổ. |
| **Biến động sai lệch theo dõi** | *Tracking Error* | Độ biến động (thường dùng độ lệch chuẩn) của chênh lệch lợi nhuận quỹ so với chỉ số tham chiếu — đo tính *ổn định* của sự lệch, không phải độ lớn. |

### 11.2. Viết tắt sử dụng trong bài
| Viết tắt | Đầy đủ |
| :--- | :--- |
| ETF | Exchange-Traded Fund — quỹ hoán đổi danh mục |
| DCA | Dollar-Cost Averaging — bình quân giá / đầu tư định kỳ số tiền cố định |
| CA | Cost Averaging — bình quân chi phí (tên khác của DCA) |
| LSI | Lump-Sum Investing — đầu tư một lần / dồn vốn |
| NAV | Net Asset Value — giá trị tài sản ròng |
| TER | Total Expense Ratio — tỷ lệ chi phí hoạt động |
| AP | Authorized Participant — thành viên tham gia tạo/chuộc chứng chỉ ETF trực tiếp với quỹ |
| SEC | U.S. Securities and Exchange Commission — Ủy ban Chứng khoán và Giao dịch Hoa Kỳ |

---

## 12. Danh Mục Tài Liệu Tham Khảo và Bước Kế Tiếp (References and Next Steps)

### Tài liệu tham khảo (APA-ish, gắn mã Claim; toàn bộ URL đã mở và đọc ngày 2026-09-15)
- Fama, E. F., & French, K. R. (1997). *Value versus growth: The international evidence* [Bản làm việc; sau in trên *The Journal of Finance*, 53(6), 1998]. SSRN. https://papers.ssrn.com/sol3/papers.cfm?abstract_id=2358 — `[W10-C02]` (abstract đã đọc; toàn văn bài in chưa mở đối chiếu từng trang)
- Finlay, M., & Zorn, J. (2023). *Cost averaging: Invest now or temporarily hold your cash?* [Vanguard research, February 2023]. The Vanguard Group. https://corporate.vanguard.com/content/dam/corp/research/pdf/cost_averaging_invest_now_or_temporarily_hold_your_cash.pdf — `[W10-C06]`
- Shtekhman, A., Tasopoulos, C., & Wimmer, B. (2012). *Dollar-cost averaging just means taking risk later* [Vanguard research]. The Vanguard Group. — Được Vanguard (2023) trích dẫn trong danh mục tài liệu; bản gốc chưa mở trực tiếp trong phiên này, liệt kê như nguồn mở rộng.
- U.S. Securities and Exchange Commission. (2012). *Investor Bulletin: Exchange-Traded Funds (ETFs)*. https://www.sec.gov/investor/pubs/etfs.pdf — `[W10-C03]`, `[W10-C05]`
- U.S. Securities and Exchange Commission, Office of Investor Education and Assistance. (2025). *Mutual Fund and ETF Fees and Expenses – Investor Bulletin* [Cập nhật 2025-07-23]. Investor.gov. https://www.investor.gov/introduction-investing/general-resources/news-alerts/alerts-bulletins/investor-bulletins/mutual-fund-and-etf-fees-and-expenses-investor-bulletin — `[W10-C04]`
- U.S. Securities and Exchange Commission, Office of Investor Education and Advocacy. (n.d.). *Stocks - FAQs*. Investor.gov. https://www.investor.gov/introduction-investing/investing-basics/investment-products/stocks — `[W10-C01]`
- Ví dụ định lượng `[W10-C07]`: phép tính minh họa của bài học, đã kiểm tra số học độc lập ngày 2026-09-15; không phải dữ liệu thị trường.
- Sổ cái nguồn: [Investing/quality/module-2-source-ledger.md](../../quality/module-2-source-ledger.md) — trạng thái từng claim và ngày kiểm tra.

### Bước Kế Tiếp
1. Ôn tập bộ Flashcards: `flashcards/week-10.yaml`.
2. Làm bài kiểm tra đánh giá: `assessments/quizzes/week-10-quiz.md`.
3. Đối chiếu đáp án phân tích sâu: `assessments/answer-keys/week-10-answers.md`.
4. Chuyển tiếp tuần sau: W11 — Thị trường Việt Nam: HOSE, HNX, UPCoM và VN-Index (`notes/module-3/week-11-thi-truong-viet-nam.md`), nơi các khái niệm quỹ/ETF của bài này sẽ gắn với hạ tầng giao dịch, phí và thuế thực tế tại Việt Nam.
