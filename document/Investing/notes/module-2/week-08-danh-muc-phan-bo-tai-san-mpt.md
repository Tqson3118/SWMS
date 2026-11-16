> [!CAUTION]
> Nội dung chỉ mang tính giáo dục, KHÔNG phải lời khuyên đầu tư.
> Mọi quyết định đầu tư là trách nhiệm cá nhân của bạn.
> Hãy tham khảo chuyên gia tài chính phù hợp trước khi đầu tư.

# W08 — Danh mục đầu tư, Asset Allocation và Modern Portfolio Theory: khung tối ưu hóa theo giả định, không phải bảo đảm

## 1. Vì sao cần học
Tuần này nối W01 (tách dữ kiện — giả định — kết luận), W02 (các loại tài sản và rủi ro), W03–W04 (đọc số có đơn vị và kỳ tính), W05–W06 (nguồn, chi phí và khớp lệnh) với W07 (ngân sách rủi ro từng lệnh) để trả lời câu hỏi cấp trên của từng lệnh: **nếu vốn được chia cho nhiều tài sản, phép tính nào mô tả lợi nhuận và rủi ro của cả tập hợp — và khung tính đó đáng tin đến đâu?** Bài học dạy bốn khối: **phân bổ tài sản (asset allocation)** và **đa dạng hóa (diversification)** theo tài liệu giáo dục của U.S. SEC; **lợi nhuận kỳ vọng, phương sai và độ lệch chuẩn của danh mục** (portfolio expected return, variance, standard deviation) cùng vai của **tương quan (correlation)** và **hiệp phương sai (covariance)**; **Thuyết danh mục hiện đại (Modern Portfolio Theory — MPT)** của Harry Markowitz với **biên hiệu quả (efficient frontier)**; và **tỷ số Sharpe (Sharpe Ratio)** làm thước đo tương đối. Cùng lúc, bài học chỉ rõ ranh giới của khung này: mọi "tối ưu" đều tính trên **đầu vào được ước lượng (estimates)** — lỗi ước lượng (estimation error) làm kết quả "tối ưu" thay đổi, mô hình không bảo đảm tương lai, phân bổ tài sản là **quyết định cá nhân** chứ không phải công thức chung, và bài học **không tính ra danh mục tối ưu cho bất kỳ ai**. Nguồn chính mở trực tiếp trong phiên này: Investor.gov (U.S. SEC) "Asset Allocation and Diversification" và mục từ "Asset Allocation", bài đọc CFA Institute 2026 Curriculum *Portfolio Risk and Return: Part I*, OpenStax *Principles of Finance* mục 15.2 và 15.4, và bài gốc Markowitz (1952) "Portfolio Selection" (Journal of Finance 7(1): 77–91) qua bản PDF toàn văn mở trực tiếp. Dữ liệu tương quan được trích từ OpenStax là dữ liệu cổ phiếu niêm yết Hoa Kỳ giai đoạn 2011–2020; **không có dữ liệu tương quan, tỷ trọng hay hành vi giá của thị trường Việt Nam nào được mở hoặc xác minh trong phiên này**. Không phần nào là khuyến nghị cá nhân, không có bảo đảm lợi nhuận hay bảo đảm "tối ưu", và không yêu cầu dùng tiền thật.

**Kiến thức tiên quyết:** hoàn thành W01, W02, W03, W04, W05, W06 và W07. Cần nhớ từ W01: tách dữ kiện — giả định — kết luận; từ W04: mọi con số đều có đơn vị, kỳ tính và cửa sổ dữ liệu; từ W06: dữ liệu quá khứ có thể đã nằm trong giá (dạng yếu hiệu quả) và chi phí làm kết quả "trên giấy" xấu đi; từ W07: CFA khẳng định tương quan giữa tài sản là yếu tố quyết định rủi ro danh mục khi số tài sản tăng, và định lượng đầy đủ phần đó được hẹn cho W08 — chính là tuần này.

**Fast-Track Core (30–45 phút):** đọc mục 1–4 (20–25 phút), làm Mini-review 1–2 (5–10 phút), làm quiz 8 câu (8–10 phút), rồi đối chiếu answer key. Phần đào sâu gồm mục "Biết được gì / Chưa thể kết luận gì" và hoạt động tự nguyện.

## 2. Mục tiêu học tập đo được
- [LO-W08-01] Định nghĩa phân bổ tài sản (asset allocation), đa dạng hóa (diversification), danh mục đầu tư (portfolio), tỷ trọng (weight), tái cân bằng (rebalancing), tương quan (correlation) và hiệp phương sai (covariance) bằng tên Việt–Anh, đúng vai theo nguồn đã mở: SEC mô tả phân bổ tài sản là **quyết định cá nhân** phụ thuộc thời hạn đầu tư (time horizon) và mức chấp nhận rủi ro (risk tolerance); đa dạng hóa là "đừng bỏ trứng vào một giỏ" theo mô tả SEC và OpenStax; tái cân bằng là đưa danh mục về tỷ trọng mục tiêu — đều **không phải khuyến nghị tỷ trọng nào và không bảo đảm kết quả**.
- [LO-W08-02] Tính, trên dữ liệu giả định với kỳ tính thống nhất (%/năm): lợi nhuận kỳ vọng danh mục `E(Rp) = w₁·E(R₁) + w₂·E(R₂)` (trung bình trọng số); phương sai danh mục hai tài sản `σp² = w₁²σ₁² + w₂²σ₂² + 2·w₁·w₂·ρ₁₂·σ₁·σ₂` (với hiệp phương sai `cov₁₂ = ρ₁₂·σ₁·σ₂`); tỷ số Sharpe `= (Rp − Rf)/σp`; số tiền tái cân bằng (VND) về tỷ trọng mục tiêu; và làm lại phép tính khi một đầu vào ước lượng đổi — ghi rõ đơn vị, kỳ tính và rằng mọi đầu vào là giả định khai báo.
- [LO-W08-03] Phân tích: vì sao σp bằng trung bình trọng số chỉ khi ρ = +1 và nhỏ hơn khi ρ < +1 (cơ chế bù trừ biến động); tách **rủi ro phi hệ thống (unsystematic/firm-specific risk)** — có thể giảm bằng đa dạng hóa — khỏi **rủi ro hệ thống (systematic risk)** — không thể đa dạng hóa; và đánh giá độ nhạy của kết quả "tối ưu" với lỗi ước lượng đầu vào E(R), σ, ρ.
- [LO-W08-04] Phản biện các khẳng định kiểu "danh mục 60/40 bảo đảm lợi nhuận", "MPT cho danh mục tối ưu hoàn hảo", "nắm giữ N mã là đủ an toàn" bằng cách chỉ ra: mô hình tối ưu hóa theo giả định và đầu vào ước lượng (Markowitz), Sharpe là thước đo **tương đối** phải so với lựa chọn khác (OpenStax), không tồn tại con số mã "chuẩn" được mọi bên thống nhất (OpenStax), và ghi đúng trạng thái "biết được" / "chưa thể kết luận".

## 3. Khái niệm cốt lõi
### 3.1. Phân bổ tài sản và đa dạng hóa: quyết định cá nhân — không phải công thức chung
Investor.gov (U.S. SEC), trang "Asset Allocation and Diversification", định nghĩa: "Asset allocation involves dividing your investments among different assets, such as stocks, bonds, and cash" — phân bổ tài sản là chia vốn đầu tư giữa các nhóm tài sản như cổ phiếu, trái phiếu và tiền mặt. Ngay sau đó, SEC nhấn mạnh: "The asset allocation decision is a personal one. The allocation that works best for you changes at different times in your life, depending on your investing timeframe or time horizon and your risk tolerance" — quyết định phân bổ **thuộc về từng người**, thay đổi theo **thời hạn đầu tư (time horizon)** — số tháng, năm hoặc thập kỷ bạn dự định đầu tư để đạt mục tiêu tài chính — và theo **mức chấp nhận rủi ro (risk tolerance)** — khả năng và sự sẵn lòng chấp nhận mất một phần hoặc toàn bộ vốn gốc để đổi lấy lợi nhuận tiềm năng cao hơn. [W08-C01]

Về đa dạng hóa, cùng trang SEC: "consider diversification — the practice of spreading money among different investments to reduce risk. Diversification is a strategy that can be neatly summed up as: 'Don't put all your eggs in one basket'" — đa dạng hóa là phân tán tiền giữa các khoản đầu tư khác nhau để giảm rủi ro, tóm gọn là "đừng bỏ hết trứng vào một giỏ". SEC nêu hai tầng: (1) phân bổ giữa các **nhóm** tài sản, vì yếu tố làm một nhóm tệ đi có thể tốt cho nhóm khác; (2) phân tán **bên trong** từng nhóm — nắm nhiều cổ phiếu/trái phiếu khác nhau và nhiều ngành khác nhau. SEC còn cảnh báo: quỹ đầu tư (mutual fund/ETF) **không tự động** tạo đa dạng hóa nếu quỹ tập trung hẹp vào một ngành, và ngay cả khi nắm nhiều quỹ, "you should check the top holdings of the funds" — cần kiểm tra các khoản nắm giữ lớn của từng quỹ để biết chúng có trùng nhau hay không. Mục từ "Asset Allocation" của Investor.gov tóm lại cùng định nghĩa: "dividing your investments among different categories, such as stocks, bonds, and cash". [W08-C01]

**Đánh dấu đúng vai:** cả hai nguồn là **tài liệu giáo dục của cơ quan quản lý Hoa Kỳ**; chúng mô tả cơ chế và cảnh báo, không gợi ý tỷ trọng nào ("60/40", "70/30"…) và không nói về thị trường, sản phẩm hay thuế tại Việt Nam. Câu hỏi "phân bổ bao nhiêu phần trăm cho nhóm nào" là quyết định cá nhân — bài học không trả lời thay.

### 3.2. Tái cân bằng: cơ chế đưa danh mục về tỷ trọng mục tiêu — có chi phí, không có bảo đảm
Cùng trang SEC, mục "Rebalancing": theo thời gian, một số khoản đầu tư tăng nhanh hơn các khoản khác, "push your holdings out of alignment with your investment goals and change the risk level of your portfolio" — làm tỷ trọng lệch khỏi mục tiêu và **thay đổi mức rủi ro của danh mục**; "To bring your portfolio back to its original asset allocation, you may need to rebalance your portfolio" — tái cân bằng (rebalancing) là hành động đưa danh mục về tỷ trọng phân bổ ban đầu. Ví dụ của SEC: bắt đầu với 60% cổ phiếu, thị trường tăng khiến tỷ trọng đó lên 80% — để về lại cơ cấu ban đầu, có thể cần **bán bớt cổ phiếu và/hoặc thêm tiền** vào nhóm tài sản khác. SEC mô tả cơ chế tâm lý: "By cutting back on current 'winners' and/or adding more current 'losers,' rebalancing forces you to buy low and sell high" — bán bớt phần đang thắng và mua thêm phần đang thua buộc hành vi "mua thấp bán cao", dù SEC thừa nhận việc này "may not be easy" — không dễ. [W08-C01]

SEC ghi nhận hai cách lịch tái cân bằng được một số chuyên gia tài chính khuyến nghị: **định kỳ** (ví dụ mỗi 6 hoặc 12 tháng) hoặc **theo ngưỡng** (khi tỷ trọng một nhóm lệch quá mức % định trước), và kết luận thực hành: "rebalancing tends to work best when done relatively infrequently" — tái cân bằng thường hoạt động tốt nhất khi thực hiện tương đối thưa. Trong quỹ ngày mục tiêu (target date fund), việc tái cân bằng do công ty quản lý quỹ thực hiện, thường thiết kế để danh mục "trở nên thận trọng hơn" khi gần ngày mục tiêu. **Giới hạn bắt buộc:** đây là mô tả giáo dục chung của SEC; tần suất, ngưỡng, phí giao dịch, thuế và loại lệnh thực tế khi tái cân bằng tại Việt Nam **chưa được xác minh trong phiên này**; mỗi lần tái cân bằng đều phát sinh ma sát (phí, chênh lệch giá khớp — W06, W07) và **không có bảo đảm** rằng "về tỷ trọng mục tiêu" là hành động tốt trong mọi kịch bản — đó là một quy ước giữ kỷ luật, không phải công thức sinh lời.

### 3.3. Lợi nhuận kỳ vọng và phương sai danh mục: công thức, đơn vị, kỳ tính
**Quy ước ký hiệu của bài học (calculation only):** danh mục hai tài sản, tỷ trọng `w₁` và `w₂` với `w₁ + w₂ = 1` (tức 100%); lợi nhuận từng tài sản `R₁, R₂` đo bằng **%/kỳ** — ví dụ %/năm; **mọi đại lượng trong cùng một phép tính phải cùng kỳ tính** (không trộn %/năm với %/tháng); kỳ tính ví dụ là năm, không phải dự báo.

**Lợi nhuận kỳ vọng danh mục** — luôn là trung bình trọng số, bất kể tương quan:

`E(Rp) = w₁·E(R₁) + w₂·E(R₂)` — đơn vị %/năm khi các E(Ri) là %/năm.

**Phương sai và độ lệch chuẩn danh mục hai tài sản** — đây là nơi tương quan vào cuộc:

`σp² = w₁²·σ₁² + w₂²·σ₂² + 2·w₁·w₂·ρ₁₂·σ₁·σ₂`

với `σ` là độ lệch chuẩn lợi nhuận (%/năm), `ρ₁₂` là hệ số tương quan giữa hai tài sản (`−1 ≤ ρ₁₂ ≤ +1`), và **hiệp phương sai (covariance)** `cov₁₂ = ρ₁₂·σ₁·σ₂` — cho phép viết lại `σp² = w₁²σ₁² + w₂²σ₂² + 2·w₁·w₂·cov₁₂`. Độ lệch chuẩn danh mục `σp = √σp²` đo bằng %/năm. CFA Institute (2026 Curriculum, *Portfolio Risk and Return: Part I*) liệt kê đúng năng lực này: "calculate and interpret the mean, variance, and covariance (or correlation) of asset returns based on historical data" và "calculate and interpret portfolio standard deviation". [W08-C02]

Ba tính chất đọc đúng vai:
- **E(Rp) không được "tối ưu hóa" thành gì khác:** với tỷ trọng cho trước, lợi nhuận kỳ vọng danh mục là trung bình trọng số; tương quan chỉ tác động lên **rủi ro** (phương sai), không làm E(Rp) vượt trung bình trọng số. Quảng cáo kiểu "kết hợp tài sản tương quan thấp để lợi nhuận cao hơn trung bình trọng số" là sai cấu trúc công thức.
- **Hai trường hợp biên:** khi `ρ = +1` (cùng chiều hoàn hảo), `σp² = (w₁σ₁ + w₂σ₂)²` → `σp` **bằng đúng trung bình trọng số** của hai độ lệch chuẩn — không có lợi ích phân tán nào. Khi `ρ < +1`, thành phần hiệp phương sai nhỏ hơn trường hợp +1 và `σp` **nhỏ hơn trung bình trọng số** — đây chính là cơ chế "gains in one stock can offset the losses in another" của OpenStax. [W08-C02]
- **Toàn bộ phép tính đứng trên đầu vào giả định:** E(R), σ, ρ trong công thức là ước lượng từ dữ liệu lịch sử hoặc giả định của người tính — mục 3.7 và 4.5 chỉ ra lỗi ước lượng làm kết quả "tối ưu" thay đổi thế nào.

### 3.4. Tương quan: con số −1 đến +1 và dữ liệu minh họa của nguồn
OpenStax (*Principles of Finance*, mục 15.2) mô tả: "a correlation coefficient describes how two variables move relative to each other. A correlation coefficient of 1 means that there is a perfect, positive correlation… while a correlation coefficient of −1 means that the two variables move exactly opposite of each other" — hệ số tương quan đo hai biến cùng biến động thế nào, từ +1 (cùng chiều hoàn hảo) đến −1 (ngược chiều hoàn hảo); và kết luận phương pháp: "Combining stocks that are not perfectly positively correlated in a portfolio decreases risk" — kết hợp cổ phiếu **không** tương quan dương hoàn hảo làm giảm rủi ro. CFA (cùng bài đọc) khẳng định ở mức danh mục: "The risk of a two-asset portfolio is dependent on the proportions of each asset, their standard deviations and the correlation (or covariance) between the assets' returns. As the number of assets in a portfolio increases, the correlation among asset risks becomes a more important determinant of portfolio risk" và "Combining assets with low correlations reduces portfolio risk". [W08-C02]

Dữ liệu minh họa mà OpenStax trích là **cổ phiếu niêm yết Hoa Kỳ, giai đoạn 2011–2020**: cặp cùng ngành hàng không Delta (DAL) — Southwest (LUV) có hệ số tương quan **0,87**; DAL — ExxonMobil (XOM, ngành năng lượng) **0,35**; DAL — CVS (y tế) **0,79**. Kèm bảng lợi nhuận hai tài sản của OpenStax: danh mục 50/50 DAL–XOM có độ lệch chuẩn **30,43%/năm** so với **51,90%/năm** của riêng DAL, trong khi trung bình lợi nhuận thấp hơn (11,03% so với 22,40% — vì XOM là tài sản lợi nhuận thấp) — minh họa cả lợi ích lẫn cái giá của phân tán. Ngược lại DAL–LUV (0,87) chỉ hạ độ lệch chuẩn từ 51,90% xuống **49,11%** vì hai hãng cùng chịu điều kiện ngành hàng không. [W08-C02]

**Giới hạn dữ liệu bắt buộc:** các con số trên là dữ liệu lịch sử Hoa Kỳ trong một cửa sổ 10 năm cụ thể mà nguồn trích; chúng minh họa cơ chế, **không** là dữ liệu Việt Nam, không tự động lặp ở giai đoạn khác, và không phải cặp tài sản nào cũng cho kết quả cùng chiều. Bài học **không có** hệ số tương quan giữa các mã, ngành hay lớp tài sản tại thị trường Việt Nam.

### 3.5. Markowitz (1952), MPT và biên hiệu quả: tối ưu hóa theo giả định — không phải bảo đảm
Bài gốc của khung này là Harry Markowitz (1952), "Portfolio Selection", *The Journal of Finance* 7(1): 77–91. Trang đầu (đã mở trực tiếp bản PDF toàn văn trong phiên) tách quá trình chọn danh mục thành **hai giai đoạn**: "The first stage starts with observation and experience and ends with beliefs about the future performances of available securities. The second stage starts with the relevant beliefs about future performances and ends with the choice of portfolio" — giai đoạn một đi từ quan sát và kinh nghiệm đến **niềm tin về hiệu suất tương lai** của các chứng khoán; giai đoạn hai đi từ niềm tin đó đến việc chọn danh mục. Markowitz từ chối quy tắc "tối đa hóa lợi nhuận chiết khấu" vì "a rule of behavior which does not imply the superiority of diversification must be rejected both as a hypothesis and as a maxim" — mọi quy tắc không hàm chứa tính vượt trội của đa dạng hóa bị bác bỏ; và chấp nhận quy tắc "expected return a desirable thing and variance of return an undesirable thing" — lợi nhuận kỳ vọng là thứ đáng muốn, **phương sai lợi nhuận** là thứ đáng tránh (quy tắc E–V: expected returns–variance of returns). [W08-C03]

Trên nền đó, CFA 2026 *Portfolio Risk and Return: Part I* mô tả kết cấu của mô hình Markowitz mà bài học cần: năng lực "describe and interpret the minimum-variance and efficient frontiers of risky assets and the global minimum-variance portfolio" — **biên phương sai tối thiểu (minimum-variance frontier)**, **danh mục phương sai tối thiểu toàn cục (global minimum-variance portfolio)** và **biên hiệu quả (efficient frontier)** — tập hợp danh mục mà tại mỗi mức rủi ro không có danh mục nào cho lợi nhuận kỳ vọng cao hơn (hoặc tại mỗi mức lợi nhuận không có danh mục nào ít rủi ro hơn). CFA cũng tóm tắt nguyên lý lựa chọn: nhà đầu tư ngại rủi ro (risk-averse) chọn theo đánh đổi rủi ro–lợi nhuận, "maximizing return for the same risk, and minimizing risk for the same return". [W08-C03]

Bốn giới hạn phải đọc cùng lúc, đúng vai:
1. **Đầu vào là niềm tin/ước lượng, không phải dữ kiện tương lai.** Chính cấu trúc hai giai đoạn của Markowitz đặt "beliefs about the future performances" làm đầu vào; bài học không có bất kỳ ước lượng hiệu suất tương lai nào được chứng minh.
2. **"Tối ưu" phụ thuộc đầu vào và phụ thuộc người.** Đổi E(R), σ hoặc ρ một chút là biên hiệu quả và danh mục "tối ưu" dịch chuyển (mục 4.5 tính minh họa); và chọn điểm nào trên biên phụ thuộc mức ngại rủi ro của từng người (CFA: đặc tính ngại rủi ro là thông tin cần thiết để chọn danh mục tối ưu). Vì vậy **không tồn tại "danh mục tối ưu chung cho mọi người"** và bài học không khuyến nghị tỷ trọng nào.
3. **Mô hình là khung mô tả lựa chọn, không phải máy dự báo.** Biên hiệu quả vẽ trên dữ liệu giả định/ước lượng; không nguồn đã mở khẳng định danh mục nằm trên biên hiệu quả lịch sử sẽ tiếp tục hiệu quả trong tương lai.
4. **Kết quả thực tế còn chịu phí, thuế và khớp lệnh** (W06–W07) — các ma sát không nằm trong công thức E–V thuần túy của bài học. [W08-C03]

### 3.6. Tỷ số Sharpe: phần thưởng trên mỗi đơn vị rủi ro — thước đo tương đối
OpenStax (*Principles of Finance*, mục 15.4) định nghĩa tỷ số Sharpe: "The Sharpe ratio is computed as a portfolio's risk premium divided by the standard deviation of the portfolio's return":

`Sharpe = (Rp − Rf) / σp`

với `Rp` là lợi nhuận danh mục, `Rf` là lợi nhuận phi rủi ro (risk-free return) — phần bù "risk premium" (phần bù rủi ro) là `Rp − Rf`, "the basic reward for bearing risk". Ví dụ của chính OpenStax: danh mục 20%/năm, Rf 3%/năm, độ lệch chuẩn 26%/năm → Sharpe = 17/26 ≈ **0,65**; và cách đọc: "The Sharpe ratio can be thought of as a reward-to-risk ratio" — tỷ lệ phần-thưởng-trên-rủi-ro, mẫu số là "units of risk". Điểm mấu chốt về vai: "Both the Sharpe and Treynor ratios are relative measures of investment performance, meaning that there is not an absolute number that indicates whether an investment performance is good or bad" — Sharpe là thước đo **tương đối**: không có con số tuyệt đối nào tự nó nói "tốt" hay "xấu"; phải so với quản trị viên khác hoặc chuẩn so sánh (benchmark). [W08-C04]

Hai cảnh báo khi dùng: (1) tử số và mẫu số phải **cùng kỳ tính** — Sharpe từ lợi nhuận %/năm và σ %/năm; trộn kỳ là lỗi đơn vị (W04); (2) Sharpe tính trên dữ liệu lịch sử là mô tả quá khứ, không phải bảo đảm; Rf lấy bao nhiêu là giả định của người tính. Bài học không xếp hạng sản phẩm nào theo Sharpe.

### 3.7. Rủi ro phi hệ thống so với hệ thống; số lượng mã: không có con số chuẩn
OpenStax mục 15.2 tách hai loại rủi ro: **rủi ro phi hệ thống (unsystematic risk, còn gọi firm-specific risk)** — rủi ro gắn với sự kiện của một công ty cụ thể, ví dụ "a company facing a product liability lawsuit, a company inventing a new product, or accounting irregularities being detected"; danh mục đa dạng hóa bảo vệ nhà đầu tư khỏi bị tác động nặng bởi loại rủi ro này ("Portfolio diversification protects you from being significantly impacted by unsystematic risk"). Và **rủi ro hệ thống (systematic risk)** — "the risk of holding the market portfolio", loại rủi ro không đi đâu được: "This systematic risk cannot be eliminated through diversification"; OpenStax nêu ví dụ tác động toàn thị trường: thay đổi chính sách tiền tệ và lãi suất của ngân hàng trung ương, sự kiện địa chính trị, bão lớn, đại dịch. Cùng mục đó, về kích thước danh mục: độ lệch chuẩn giảm nhanh khi thêm cổ phiếu thứ hai, thứ ba, rồi mức giảm dần nhỏ đi; nhưng "there is a level below which the portfolio risk does not drop, no matter how diversified the portfolio becomes" — tồn tại sàn rủi ro mà đa dạng hóa không xuyên qua được. [W08-C05]

Về câu hỏi "bao nhiêu mã là đủ", OpenStax trả lời thẳng: "How many stocks does an investor need for a portfolio to be well-diversified? There is not an exact number that all financial managers agree on" — **không tồn tại con số chính xác được mọi nhà quản trị thống nhất**; nguồn nhấn mạnh tương quan quan trọng hơn số đếm (15 mã tương quan cao cho lợi ích phân tán ít hơn 10 mã tương quan thấp) và tập trung một ngành khiến danh mục không thực sự đa dạng; OpenStax ghi nhận quan điểm mà nguồn trích rằng nhà đầu tư bình thường có thể tạo danh mục phân tán hợp lý với khoảng một chục mã, và dẫn nghiên cứu Statman (1987) qua ghi chú của nguồn. **Đúng vai:** mọi con số mã là mô tả thực hành trích trong nguồn giáo dục, **không** là ngưỡng chuẩn hóa và **không** áp được máy móc cho thị trường Việt Nam; trạng thái đúng cho "số mã tối ưu" là **chưa thể kết luận**. [W08-C05]

## 4. Ví dụ và worked examples (giả định, đơn vị %/năm và VND)
### 4.1. Dữ liệu nền — hai tài sản hư cấu (calculation only)
**Giả định của người viết:** tài sản giả định "Cổ phiếu X" với E(R) = 12%/năm, σ = 25%/năm; tài sản giả định "Trái phiếu Y" với E(R) = 5%/năm, σ = 8%/năm; tỷ trọng giả định 60% X — 40% Y (w₁ = 0,60; w₂ = 0,40); lãi suất phi rủi ro giả định Rf = 3%/năm. Mọi con số dưới đây đều là **giả định minh họa**, cùng kỳ tính một năm, **không** là dữ liệu thị trường nào (Hoa Kỳ hay Việt Nam), và không phải dự báo. Ba giá trị tương quan được tính để thấy cơ chế: ρ = +1; ρ = 0,2; ρ = −0,2 — đều là con số khai báo, không ước lượng từ dữ liệu thật. [W08-C06]

### 4.2. E(Rp) và σp với ba mức tương quan (calculation only)
**E(Rp) — giống nhau ở cả ba kịch bản vì chỉ phụ thuộc tỷ trọng:**

E(Rp) = 0,60 × 12% + 0,40 × 5% = 7,2% + 2,0% = **9,2%/năm**.

**σp — phụ thuộc ρ:** công thức `σp² = w₁²σ₁² + w₂²σ₂² + 2·w₁·w₂·ρ·σ₁·σ₂` với w₁²σ₁² = 0,36 × 625 = 225 (%²), w₂²σ₂² = 0,16 × 64 = 10,24 (%²), và thành phần hiệp phương sai 2·w₁·w₂·ρ·σ₁·σ₂ = 0,48 × ρ × 25 × 8 = 96·ρ (%²).

| Kịch bản tương quan (giả định) | Hiệp phương sai cov = ρ·σ₁·σ₂ (%²) | Số hạng 2·w₁·w₂·cov (%²) | σp² (%²) | σp (%/năm) | So với trung bình trọng số σ |
|---|---:|---:|---:|---:|---|
| ρ = +1 (cùng chiều hoàn hảo) | 1,0 × 25 × 8 = 200,00 | 0,48 × 200 = 96,00 | 225 + 10,24 + 96 = 331,24 | **18,20%** | = 0,6×25 + 0,4×8 = 18,20% — không giảm |
| ρ = 0,2 (tương quan thấp — giả định) | 0,2 × 25 × 8 = 40,00 | 0,48 × 40 = 19,20 | 225 + 10,24 + 19,20 = 254,44 | **15,95%** | thấp hơn 2,25 điểm % |
| ρ = −0,2 (tương quan âm — giả định) | −0,2 × 25 × 8 = −40,00 | 0,48 × (−40) = −19,20 | 225 + 10,24 − 19,20 = 216,04 | **14,70%** | thấp hơn 3,50 điểm % |

Tính lại từng bước cho dòng ρ = 0,2: σp² = 225 + 10,24 + 19,20 = 254,44 (%²); σp = √254,44 ≈ 15,9512 → **15,95%/năm**. Dòng ρ = −0,2: σp² = 225 + 10,24 − 19,20 = 216,04; σp = √216,04 ≈ 14,6983 → **14,70%/năm**. Kiểm tra biên ρ = +1 bằng cách khác: khi ρ = +1 thì σp = w₁σ₁ + w₂σ₂ = 0,6 × 25 + 0,4 × 8 = 15 + 3,2 = 18,20% ✓ (18,20² = 331,24). Ba lần đọc: (1) E(Rp) cố định 9,2%/năm ở mọi dòng — tương quan không đổi lợi nhuận kỳ vọng; (2) σp giảm dần khi ρ giảm từ +1 xuống âm, đúng cơ chế CFA/OpenStax; (3) dù ρ = −0,2, σp (14,70%) vẫn cao hơn σ của riêng trái phiếu Y (8%) — phân tán **giảm rủi ro so với pha trộn**, không biến danh mục thành "rủi ro thấp nhất thành phần". [W08-C02][W08-C06]

### 4.3. Tỷ số Sharpe trên dữ liệu giả định (calculation only)
Áp `Sharpe = (Rp − Rf)/σp` với Rf = 3%/năm (giả định) và các kết quả 4.2:

| Đại lượng | Phép tính | Sharpe (lần) |
|---|---|---:|
| Danh mục 60/40, ρ = 0,2 | (9,2 − 3)/15,95 = 6,2/15,95 | ≈ **0,39** |
| Chỉ Cổ phiếu X | (12 − 3)/25 = 9/25 | **0,36** |
| Chỉ Trái phiếu Y | (5 − 3)/8 = 2/8 | **0,25** |

Đọc đúng vai (theo OpenStax): Sharpe là **thước đo tương đối** — trong bảng giả định này danh mục có Sharpe cao hơn hai thành phần riêng lẻ, nhưng kết luận chỉ có nghĩa **trong tập giả định này**; đổi Rf thành 4%/năm: danh mục (9,2 − 4)/15,95 = 5,2/15,95 ≈ 0,33 — vẫn trên X (8/25 = 0,32) nhưng chênh hẹp lại. Không có ngưỡng Sharpe "đạt chuẩn" nào được nguồn đã mở xác nhận; so sánh Sharpe giữa hai đại lượng phải cùng kỳ tính và cùng dữ liệu đầu vào. [W08-C04][W08-C06]

### 4.4. Tái cân bằng: phép tính số tiền về tỷ trọng mục tiêu (calculation only)
**Giả định:** danh mục 100.000.000 VND theo mục tiêu 60% cổ phiếu X giả định — 40% trái phiếu Y giả định, tức 60.000.000 và 40.000.000 VND. Sau một kỳ: X tăng 25% → 75.000.000 VND; Y tăng 2% → 40.800.000 VND; tổng danh mục = 115.800.000 VND.

- Tỷ trọng hiện tại: X = 75.000.000/115.800.000 ≈ **64,77%**; Y = 40.800.000/115.800.000 ≈ **35,23%** — lệch khỏi mục tiêu 60/40 đúng theo cơ chế SEC (mục 3.2).
- Về lại 60/40: giá trị X mục tiêu = 0,60 × 115.800.000 = **69.480.000 VND** → **bán 5.520.000 VND cổ phiếu**; Y mục tiêu = 0,40 × 115.800.000 = **46.320.000 VND** → mua thêm 5.520.000 VND trái phiếu. Kiểm tra: 69.480.000 + 46.320.000 = 115.800.000 ✓.
- **Chi phí và giới hạn:** mỗi chiều giao dịch phát sinh phí và chênh lệch khớp (W06–W07 — mức phí tại Việt Nam chưa được xác minh trong phiên này, con số không đưa vào phép tính); SEC mô tả tái cân bằng "buộc mua thấp bán cao" nhưng không bảo đảm kết quả tốt hơn trong mọi kịch bản (giả định đối chứng: nếu X tiếp tục tăng mạnh kỳ sau, việc đã bán bớt làm danh mục hưởng lợi ít hơn việc giữ nguyên — bài học **không có nguồn** khẳng định nhánh nào thắng trung bình, ghi **chưa thể kết luận**). [W08-C01][W08-C06]

### 4.5. Lỗi ước lượng: đổi một đầu vào — kết quả "tối ưu" dịch chuyển (calculation only)
Giả sử ước lượng σ của Cổ phiếu X từ 25%/năm phải sửa thành **30%/năm** và E(R) của X sửa từ 12% xuống **9%/năm** (hai sự kiện khai báo). Giữ ρ = 0,2 và các đầu vào khác:

- E(Rp) mới = 0,60 × 9% + 0,40 × 5% = 5,4% + 2,0% = **7,4%/năm** (từ 9,2%).
- σp² mới = 0,36 × 900 + 10,24 + 0,48 × 0,2 × 30 × 8 = 324 + 10,24 + 23,04 = 357,28 (%²) → σp = √357,28 ≈ **18,90%/năm** (từ 15,95%).
- Sharpe mới = (7,4 − 3)/18,90 = 4,4/18,90 ≈ **0,23** (từ ≈ 0,39) — trong bảng giả định, danh mục bây giờ có Sharpe thấp hơn nhiều và chỉ còn nhỉnh hơn X mới (9 − 3)/30 = 0,20; hơn nữa X mới có E(R) 9% khiến "tỷ trọng 60%" có còn phù hợp hay không là câu hỏi mở.

Ý nghĩa phương pháp: hai thay đổi đầu vào vừa phải đã làm lợi nhuận kỳ vọng giảm 1,8 điểm %, rủi ro tăng 2,95 điểm % và Sharpe giảm từ ≈ 0,39 xuống ≈ 0,23. Mô hình không sai — **đầu vào sai làm mọi kết quả "tối ưu" kế thừa sai số đó** (Markowitz: giai đoạn một là "beliefs about the future performances"); vì vậy khẳng định "MPT cho danh mục tối ưu hoàn hảo" là khẳng định sai vai. Bài học không có quy trình chứng minh đầu vào nào là "đúng". [W08-C03][W08-C06]

### 4.6. Phân loại bằng chứng
- **Kết quả/nguồn (đã mở trực tiếp trong phiên này):** Investor.gov "Asset Allocation and Diversification" cho định nghĩa asset allocation, time horizon, risk tolerance, rebalancing (ví dụ 60%→80%, "buy low and sell high", tần suất tương đối thưa, target date fund) và hai tầng đa dạng hóa kèm cảnh báo quỹ hẹp; mục từ "Asset Allocation" của Investor.gov cho định nghĩa ngắn; CFA 2026 *Portfolio Risk and Return: Part I* cho năng lực tính mean/variance/covariance và portfolio standard deviation, vai trò tỷ trọng–σ–tương quan với rủi ro danh mục, "Combining assets with low correlations reduces portfolio risk", minimum-variance/efficient frontier và global minimum-variance portfolio, nguyên lý nhà đầu tư ngại rủi ro; OpenStax *Principles of Finance* mục 15.2 cho tương quan +1/−1, dữ liệu DAL–LUV 0,87 / DAL–XOM 0,35 / DAL–CVS 0,79 (Hoa Kỳ 2011–2020), bảng hai tài sản (σ 30,43% so với 51,90%), định nghĩa unsystematic/systematic risk, "no exact number" về số mã; mục 15.4 cho công thức Sharpe, ví dụ 17/26 = 0,65 và tính tương đối của thước đo; Markowitz (1952) trang đầu cho hai giai đoạn, bác bỏ quy tắc tối đa hóa lợi nhuận chiết khấu, quy tắc E–V và tính vượt trội của đa dạng hóa (bản PDF quét — hạn chế ghi tại mục 11 và ledger).
- **Giả định minh họa:** Cổ phiếu X (12%/năm; 25%/năm), Trái phiếu Y (5%/năm; 8%/năm), tỷ trọng 60/40, Rf 3%/năm hoặc 4%/năm, ρ ∈ {+1; 0,2; −0,2}, kịch bản sửa ước lượng σ_X = 30%/năm và E(R_X) = 9%/năm, danh mục tái cân bằng 100.000.000 VND với X +25%, Y +2%.
- **Phép tính/quy ước:** E(Rp), σp², cov = ρ·σ·σ, Sharpe, số tiền tái cân bằng — calculation only trên đầu vào giả định; không con số nào là dữ liệu thị trường Việt Nam hay dự báo.

## 5. Mini-review và truy xuất kiến thức
### Mini-review 1: tính lại E(Rp), σp và Sharpe (không nhìn mục 4)
Không nhìn phần trên: danh mục 60% Cổ phiếu X — 40% Trái phiếu Y; E(R_X) = 12%/năm, σ_X = 25%/năm; E(R_Y) = 5%/năm, σ_Y = 8%/năm; ρ = 0,2; Rf = 3%/năm. (a) E(Rp)? (b) σp? (c) Sharpe danh mục? **Tự kiểm:** (a) 0,6 × 12 + 0,4 × 5 = **9,2%/năm**. (b) w₁²σ₁² = 225; w₂²σ₂² = 10,24; hiệp phương sai cov = 0,2 × 25 × 8 = 40 và số hạng 2 × 0,6 × 0,4 × 40 = 19,20; σp² = 254,44 → σp ≈ **15,95%/năm**. (c) (9,2 − 3)/15,95 ≈ **0,39 lần**. Ghi chú: cả ba cùng kỳ %/năm; kết quả chỉ đúng trong tập giả định này; không suy ra lợi nhuận tương lai.

### Mini-review 2: tự phản biện một bài đăng "60/40 là đủ — an toàn tuyệt đối"
Một bài đăng giả định viết: "Chia 60/40 cổ phiếu–trái phiếu là công thức quốc dân, bảo đảm 10%/năm, không bao giờ cần xem lại, và cứ nắm 20 mã cùng ngành ngân hàng là đủ đa dạng hóa." Liệt kê ít nhất sáu lỗi và trạng thái kết luận đúng. **Tự kiểm:** (1) "bảo đảm 10%/năm" là ngôn ngữ bảo đảm — không nguồn đã mở nào bảo đảm lợi nhuận; E(Rp) 60/40 của chính ví dụ giả định 4.2 là 9,2% và là **giả định**, không phải hứa hẹn; (2) "công thức quốc dân" — SEC: phân bổ tài sản là quyết định **cá nhân** theo time horizon và risk tolerance, không có tỷ trọng chung; (3) "không bao giờ cần xem lại" — SEC mô tả chính cơ chế tỷ trọng trôi (64,77/35,23 trong 4.4) làm rủi ro danh mục đổi theo thời gian; (4) "20 mã cùng ngành ngân hàng là đủ" — OpenStax: tương quan quan trọng hơn số đếm (15 mã tương quan cao < 10 mã tương quan thấp), tập trung một ngành là không đa dạng, và **không có con số chuẩn nào được mọi bên thống nhất**; (5) bỏ qua chi phí: phí, khớp lệnh (W06–W07) và ma sát tái cân bằng không nằm trong con số "10%"; (6) nhầm lịch sử với tương lai: tương quan và lợi nhuận ước lượng từ một cửa sổ dữ liệu (4.5) — đổi đầu vào là kết quả đổi, và dữ liệu trong bài là Hoa Kỳ/giả định, không phải Việt Nam. Kết luận đúng: về "bảo đảm" — khẳng định sai; về tỷ trọng 60/40 và số mã "tối ưu" — **chưa thể kết luận**.

## 6. Cảnh báo & sai lầm thường gặp
- **Coi E(Rp) có thể "tăng" nhờ tương quan thấp:** E(Rp) luôn là trung bình trọng số; tương quan chỉ tác động lên σp. Ai hứa "pha trộn thông minh để lợi nhuận cao hơn trung bình trọng số" đã sai cấu trúc công thức.
- **Đa dạng hóa giả:** nắm nhiều mã cùng ngành hoặc nhiều quỹ trùng khoản nắm giữ lớn — SEC yêu cầu kiểm tra top holdings; OpenStax: tương quan cao triệt tiêu lợi ích phân tán kể cả khi số mã lớn.
- **Tin rằng có "số mã chuẩn":** OpenStax ghi nhận không có con số chính xác được mọi nhà quản trị thống nhất; con số trong bất kỳ bài đăng nào là quy ước/ý kiến cá nhân — trạng thái đúng là "chưa thể kết luận".
- **Quên lỗi ước lượng:** biên hiệu quả và danh mục "tối ưu" dịch chuyển khi E(R), σ, ρ sửa (4.5); dữ liệu lịch sử là cửa sổ cụ thể, không tự động lặp (W05–W06); không nguồn đã mở chứng minh tính ổn định của tương quan theo thời gian.
- **Tái cân bằng miễn phí:** mỗi lần tái cân bằng có phí và chênh lệch khớp; mức phí/thuế tại Việt Nam chưa được xác minh trong phiên; SEC ghi nhận tái cân bằng "tends to work best when done relatively infrequently" nhưng không bảo đảm nhánh thắng/thua so với giữ nguyên.
- **Đọc Sharpe như điểm tuyệt đối:** Sharpe là thước đo tương đối (OpenStax); so sánh sai kỳ tính, sai Rf giả định, hoặc giữa các giai đoạn khác nhau là lỗi so sánh; không tồn tại ngưỡng "Sharpe X là tốt" được nguồn đã mở xác nhận.
- **Nhầm rủi ro hệ thống với phi hệ thống:** đa dạng hóa giảm phần phi hệ thống, **không** xóa rủi ro hệ thống (OpenStax: chính sách tiền tệ, địa chính trị, thiên tai, đại dịch); danh mục càng đa dạng càng tiến về "rủi ro của thị trường", không về 0.
- **Áp số liệu Hoa Kỳ vào Việt Nam:** mọi dữ liệu tương quan/bảng lợi nhuận trong nguồn là cổ phiếu Hoa Kỳ 2011–2020; loại tài sản, thuế, phí, biên độ và hành vi giá tại Việt Nam chưa được xác minh trong phiên này.
- **Thiên kiến xác nhận — anchoring — overconfidence:** chỉ thu thập ví dụ "danh mục phân tán đã cứu mình", bám một bộ E(R)/σ/ρ cũ, nhầm hiểu được công thức với dự đoán đúng tương lai; thiếu ghi nhận nguồn, thời điểm, đơn vị và kỳ tính là lỗi dữ liệu, không chỉ lỗi ghi chú.

## 7. Biết được gì / Chưa thể kết luận gì
**Biết được:** định nghĩa asset allocation, time horizon, risk tolerance, diversification và rebalancing theo Investor.gov/SEC, kèm cơ chế 60%→80% và "buy low and sell high", hai tầng đa dạng hóa (giữa nhóm và trong nhóm) và cảnh báo quỹ hẹp/top holdings; công thức E(Rp) = trung bình trọng số và σp² hai tài sản với cov₁₂ = ρ₁₂·σ₁·σ₂, đơn vị %/năm, yêu cầu cùng kỳ tính; ba tính chất then chốt: E(Rp) không phụ thuộc ρ, σp bằng trung bình trọng số khi ρ = +1 và nhỏ hơn khi ρ < +1, phân tán làm σp tiến về sàn rủi ro hệ thống chứ không về 0; khung Markowitz (1952): hai giai đoạn, quy tắc E–V, tính vượt trội của đa dạng hóa, cùng kết cấu minimum-variance/efficient frontier và global minimum-variance portfolio theo CFA; vai của Sharpe = (Rp − Rf)/σp như reward-to-risk và thước đo **tương đối**; phân định unsystematic risk (giảm được bằng đa dạng hóa) và systematic risk (không đa dạng hóa được) theo OpenStax; các số học giả định ở mục 4 đều kiểm tra lại được bằng tay.

**Chưa thể kết luận:** tỷ trọng phân bổ "tối ưu" cho bất kỳ ai — SEC gọi đây là quyết định cá nhân và bài học không khuyến nghị; "danh mục 60/40 bảo đảm lợi nhuận" — khẳng định sai vai (không có bảo đảm); số lượng mã "đủ đa dạng hóa" — OpenStax: không tồn tại con số chính xác được mọi bên thống nhất; tính ổn định của tương quan theo thời gian và ở thị trường Việt Nam — không nguồn đã mở; giá trị tương quan, lợi nhuận kỳ vọng và rủi ro thật của các lớp tài sản tại Việt Nam — không có dữ liệu được mở trong phiên này; danh mục trên biên hiệu quả lịch sử có tiếp tục hiệu quả trong tương lai — không nguồn đã mở khẳng định; hướng giá tương lai — bài học không tạo dự báo nào. Với bất kỳ khẳng định "tối ưu/chắc chắn" nào ngoài các mục trên, kết luận đúng là "chưa thể kết luận" cho đến khi có bằng chứng đủ.

## 8. Checklist thực hành
- [ ] Viết mục tiêu, thời hạn đầu tư (time horizon) và mức chấp nhận rủi ro của **bản thân bằng chữ**, trước khi nhìn bất kỳ tỷ trọng mẫu nào.
- [ ] Ghi nguồn, thời điểm, đơn vị (%/năm, %², lần, VND) và kỳ tính của từng con số; các kỳ tính phải khớp nhau trước khi nhân chia.
- [ ] Tính E(Rp), σp (hai tài sản, một giá trị ρ) và Sharpe **trên dữ liệu giả định tự khai báo**; ghi rõ ρ lấy từ đâu (giả định hay dữ liệu đã mở).
- [ ] Kiểm tra đa dạng hóa thật: liệt kê ngành/nhóm của từng khoản; nếu dùng quỹ, kiểm tra các khoản nắm giữ lớn có trùng nhau không.
- [ ] Nếu mô phỏng tái cân bằng: tính cả số tiền giao dịch **và** ghi chú chi phí sẽ phát sinh; ghi "chưa thể kết luận" cho mọi so sánh nhánh thắng/thua.
- [ ] Làm lại phép tính với một đầu vào sửa đổi (4.5) để thấy độ nhạy trước khi tin bất kỳ kết quả "tối ưu" nào.
- [ ] Tách dữ kiện, giả định, phép tính và kết luận; đánh dấu mọi kết luận chưa đủ bằng chứng là "chưa thể kết luận".
- [ ] Không dùng tiền thật, không dùng tiền vay, không đặt lệnh hay rót vốn vào sản phẩm nào chỉ vì hoàn thành bài.

## 9. Hoạt động áp dụng tự nguyện (giả định, chỉ trên giấy)
Dùng **chỉ dữ liệu giả định tự tạo** (không tra cứu hay nhập dữ liệu thị trường thật): (1) tự chọn hai tài sản hư cấu A và B với E(R), σ và một tỷ trọng w tự đặt, khai báo tất cả là giả định; (2) tính E(Rp) và σp với ba giá trị ρ tự chọn trong {−1; …; +1}, lập bảng như 4.2; (3) tính Sharpe danh mục với Rf giả định, rồi tính lại với Rf đổi ±1 điểm %; (4) dựng kịch bản tái cân bằng: cho hai tài sản biến động ±x% khác nhau, tính tỷ trọng trôi và số tiền cần giao dịch để về tỷ trọng mục tiêu, ghi chú chi phí sẽ phát sinh; (5) sửa một đầu vào (E hoặc σ) và tính lại toàn bộ — ghi lại bao nhiêu kết quả đổi; (6) tự phản biện: có trộn kỳ tính không, có coi giả định là dữ kiện không, có suy ra kết luận cho thị trường Việt Nam từ dữ liệu giả định/Hoa Kỳ không; (7) kết thúc bằng ba câu "chưa thể kết luận". Không dùng tiền thật, không đăng ký dịch vụ, không đặt lệnh nào.

## 10. Glossary
| Việt | English | Định nghĩa |
|---|---|---|
| Phân bổ tài sản | Asset Allocation | Theo Investor.gov: chia vốn đầu tư giữa các nhóm tài sản (cổ phiếu, trái phiếu, tiền mặt); là quyết định cá nhân phụ thuộc thời hạn đầu tư và mức chấp nhận rủi ro. |
| Thời hạn đầu tư | Time Horizon | Số tháng, năm hoặc thập kỷ dự định đầu tư để đạt mục tiêu tài chính; thời hạn dài hơn thường gắn với mức chấp nhận biến động cao hơn theo mô tả SEC. |
| Mức chấp nhận rủi ro | Risk Tolerance | Theo SEC: khả năng và sự sẵn lòng mất một phần hoặc toàn bộ vốn gốc để đổi lấy lợi nhuận tiềm năng cao hơn. |
| Đa dạng hóa | Diversification | Phân tán tiền giữa nhiều khoản đầu tư để giảm rủi ro — "đừng bỏ hết trứng vào một giỏ"; hai tầng: giữa các nhóm tài sản và trong từng nhóm. |
| Danh mục đầu tư | Portfolio | Tập hợp các khoản đầu tư (cổ phiếu, trái phiếu…) do cùng một nhà đầu tư nắm giữ; lợi nhuận danh mục phụ thuộc từng thành phần và tương quan giữa chúng. |
| Tỷ trọng | Weight (w) | Phần vốn chia cho mỗi tài sản, w₁ + w₂ = 1 (100%); đầu vào duy nhất quyết định E(Rp) cùng với E(Ri) của các thành phần. |
| Lợi nhuận kỳ vọng danh mục | Portfolio Expected Return — E(Rp) | E(Rp) = w₁·E(R₁) + w₂·E(R₂): trung bình trọng số của lợi nhuận kỳ vọng các thành phần; đơn vị %/kỳ (ví dụ %/năm); không phụ thuộc tương quan. |
| Phương sai | Variance (σ²) | Độ đo phân tán của lợi nhuận quanh giá trị kỳ vọng; đơn vị %² khi lợi nhuận là %; phương sai danh mục gồm cả thành phần hiệp phương sai. |
| Độ lệch chuẩn | Standard Deviation (σ) | Căn bậc hai của phương sai; đo biến động lợi nhuận, đơn vị %/kỳ; OpenStax dùng σp làm mẫu số của Sharpe. |
| Hiệp phương sai | Covariance (cov₁₂) | Độ đo biến động cùng chiều của hai tài sản: cov₁₂ = ρ₁₂·σ₁·σ₂; vào phương sai danh mục qua số hạng 2·w₁·w₂·cov₁₂. |
| Hệ số tương quan | Correlation Coefficient (ρ) | Đo cùng biến động của hai biến, từ −1 (ngược chiều hoàn hảo) đến +1 (cùng chiều hoàn hảo); OpenStax: kết hợp cổ phiếu không tương quan dương hoàn hảo làm giảm rủi ro. |
| Biên phương sai tối thiểu | Minimum-Variance Frontier | Theo CFA: tập hợp danh mục hai tài sản trở lên có phương sai nhỏ nhất ứng với từng mức lợi nhuận kỳ vọng, trên dữ liệu đầu vào cho trước. |
| Danh mục phương sai tối thiểu toàn cục | Global Minimum-Variance Portfolio | Theo CFA: danh mục có phương sai nhỏ nhất trên toàn bộ biên phương sai tối thiểu. |
| Biên hiệu quả | Efficient Frontier | Theo CFA: phần biên phương sai tối thiểu gồm các danh mục cho lợi nhuận kỳ vọng cao nhất tại mỗi mức rủi ro (hoặc rủi ro thấp nhất tại mỗi mức lợi nhuận); phụ thuộc đầu vào ước lượng. |
| Thuyết danh mục hiện đại | Modern Portfolio Theory (MPT) | Khung của Markowitz (1952): chọn danh mục theo quy tắc E–V — lợi nhuận kỳ vọng đáng muốn, phương sai đáng tránh; tối ưu hóa theo giả định, không bảo đảm tương lai. |
| Tài sản phi rủi ro | Risk-free Asset (Rf) | Tài sản giả định có lợi nhuận chắc chắn trong kỳ tính; mức Rf dùng trong Sharpe là giả định của người tính. |
| Phần bù rủi ro | Risk Premium | Rp − Rf: phần lợi nhuận vượt trên mức phi rủi ro, mô tả của OpenStax là phần thưởng cơ bản cho việc chịu rủi ro. |
| Tỷ số Sharpe | Sharpe Ratio | (Rp − Rf)/σp theo OpenStax: phần thưởng trên mỗi đơn vị rủi ro tổng thể; thước đo tương đối, phải so sánh với lựa chọn khác cùng kỳ tính. |
| Rủi ro phi hệ thống | Unsystematic Risk (Firm-specific Risk) | Rủi ro gắn với một công ty cụ thể (kiện tụng sản phẩm, sự cố kế toán…); đa dạng hóa giúp giảm tác động của loại rủi ro này theo OpenStax. |
| Rủi ro hệ thống | Systematic Risk | Rủi ro của việc nắm danh mục thị trường; tác động toàn thị trường (chính sách tiền tệ, địa chính trị, thiên tai, đại dịch) và không thể xóa bằng đa dạng hóa theo OpenStax. |
| Tái cân bằng | Rebalancing | Đưa danh mục về tỷ trọng mục tiêu bằng bán phần vượt tỷ trọng và mua phần thiếu tỷ trọng; SEC mô tả cơ chế "mua thấp bán cao" và tần suất tương đối thưa; có chi phí, không bảo đảm. |
| Lỗi ước lượng | Estimation Error | Sai lệch giữa đầu vào ước lượng (E, σ, ρ) và giá trị thật; làm biên hiệu quả và danh mục "tối ưu" dịch chuyển — minh họa 4.5. |
| Chưa thể kết luận | Not Yet Concluded | Trạng thái khi bằng chứng không đủ hoặc không thuộc phạm vi nguồn đã mở; cách ghi chuẩn thay cho mọi kết luận tối ưu/bảo đảm/dự báo trong bài học này. |

## 11. Tài liệu tham khảo đã mở
- U.S. SEC — Investor.gov, "Asset Allocation and Diversification": https://www.investor.gov/introduction-investing/getting-started/asset-allocation (truy cập 2026-09-15).
- U.S. SEC — Investor.gov, Glossary mục "Asset Allocation": https://www.investor.gov/introduction-investing/investing-basics/glossary/asset-allocation (truy cập 2026-09-15).
- CFA Institute, "Portfolio Risk and Return: Part I" (2026 Curriculum, refresher reading, Level I Portfolio Management): https://www.cfainstitute.org/insights/professional-learning/refresher-readings/2026/portfolio-risk-return-part-1 (truy cập 2026-09-15; trang tóm tắt bài đọc + learning outcomes + summary — toàn văn trong Learning Ecosystem).
- OpenStax, *Principles of Finance*, mục 15.2 "Risk and Return to Multiple Assets": https://openstax.org/books/principles-finance/pages/15-2-risk-and-return-to-multiple-assets (truy cập 2026-09-15).
- OpenStax, *Principles of Finance*, mục 15.4 "Applications in Performance Measurement": https://openstax.org/books/principles-finance/pages/15-4-applications-in-performance-measurement (truy cập 2026-09-15).
- Harry Markowitz (1952), "Portfolio Selection", The Journal of Finance 7(1): 77–91, DOI: https://doi.org/10.1111/j.1540-6261.1952.tb01525.x — bản PDF toàn văn mở trực tiếp: https://www2.stat.duke.edu/courses/Fall07/sta103/markowitz.pdf và https://www.efalken.com/LowVolClassics/markowitz_JF1952.pdf (truy cập 2026-09-15; bản scan là ảnh nhị phân — phần văn bản trang đầu được trích xuất và đối chiếu trong phiên, phần trang sau chứa toán học ghi nhận ở mức tổng hợp; hạn chế chi tiết ghi trong source ledger W08-C03).
- Source ledger: [quality/module-2-source-ledger.md](../../quality/module-2-source-ledger.md).

## 12. Next steps
- [Quiz W08](../../assessments/quizzes/week-08-quiz.md)
- [Answer key W08](../../assessments/answer-keys/week-08-answers.md)
- Sau khi nắm W08, W09 (đầu tư giá trị) đi vào tầng lựa chọn từng tài sản: W07 đặt ngân sách rủi ro cho một lệnh, W08 mô tả cấu trúc của cả danh mục trên đầu vào giả định, W09 sẽ hỏi khi nào một cổ phiếu đơn lẻ được mua với mức đệm so với giá trị ước tính — nhớ rằng theo SEC, phân bổ tài sản vẫn là quyết định cá nhân và không bài học nào trong chuỗi này khuyến nghị tỷ trọng hay mã cụ thể nào.

## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

**Bài:** Phân bổ tài sản. **Đầu ra:** Làm lại E(Rp), σp, Sharpe và tái cân bằng mục4.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| Kỳ/đơn vị | Hai lợi nhuận, rủi ro vàrisk-free dùng cùng kỳ | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Danh mục | Dùng trọng số tổng 1 và hạngcovariance 2w 1w 2ρσ1σ2 | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Sharpe | Dùng lợi nhuận vượt risk-free chiaσp, không chia phương sai | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Tái cân bằng | Tính tiền dịch chuyển, ghi phí/thuế chưa đưa vào | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Độ nhạy | Đổi một đầu vào mục 4.5; không gọi danh mục tối ưu là chắc chắn | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
