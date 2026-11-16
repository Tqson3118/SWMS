> [!CAUTION]
> Nội dung chỉ mang tính giáo dục, KHÔNG phải lời khuyên đầu tư.
> Mọi quyết định đầu tư là trách nhiệm cá nhân của bạn.
> Hãy tham khảo chuyên gia tài chính phù hợp trước khi đầu tư.

# W07 — Quản trị rủi ro, Position Sizing và Stop Loss: heuristic kiểm soát, không phải bảo đảm

## 1. Vì sao cần học
Tuần này nối W01 (tách dữ kiện — giả định — kết luận), W02 (rủi ro và các loại tài sản), W03–W04 (đọc số có đơn vị và kỳ tính) với W05 (phân tích cơ bản) và W06 (phân tích kỹ thuật, chi phí/khớp lệnh, "cảnh báo ≠ dự báo") để trả lời một câu hỏi thực dụng: **nếu luận điểm sai, bạn mất bao nhiêu — và con số đó có được kiểm soát bằng phép tính không?** Bài học không dạy "cách không bao giờ lỗ" — điều đó không tồn tại — mà dạy ba công cụ kiểm soát: **quản trị rủi ro (risk management)** qua **ngân sách rủi ro (risk budget)**, **quy mô vị thế (position sizing)** và **lệnh dừng (stop order, còn gọi là stop-loss)**; đồng thời chỉ rõ vì sao cả ba đều là **heuristic (quy tắc ngón tay cái — cách làm quy ước, không phải bảo đảm)**: lỗ thực tế có thể vượt kế hoạch vì phí, khoảng trống giá (gap), trượt giá (slippage) và mất thanh khoản (liquidity), và lệnh dừng chỉ là điểm kích hoạt (trigger) chứ không phải giá khớp bảo đảm. Nguồn chính của bài là các trang giáo dục của U.S. SEC mở trực tiếp trong phiên này: Investor.gov "What is Risk?", Investor.gov "Types of Orders", hai Investor Bulletin "Understanding Order Types" và "Stop, Stop-Limit, and Trailing Stop Orders" (cập nhật 2026-08-18), kèm bài đọc CFA Institute 2026 Curriculum *Portfolio Risk and Return: Part I* cho khẳng định về tương quan (correlation) và tài liệu SEC "Trading Basics" đã kiểm chứng từ phiên W06. Đây là nguồn Hoa Kỳ/quốc tế; loại lệnh, biên độ dao động, phí, thuế và chuẩn kích hoạt lệnh tại Việt Nam chưa được xác minh trong phiên này. Không phần nào là khuyến nghị cá nhân, không tạo dự báo và không yêu cầu dùng tiền thật.

**Kiến thức tiên quyết:** hoàn thành W01, W02, W03, W04, W05 và W06. Cần nhớ từ W04: mọi con số đều có đơn vị, kỳ tính và cửa sổ; từ W06: SEC nói giá khớp lệnh thị trường **không được bảo đảm**, giá dừng là điểm kích hoạt, và slippage + phí làm kết quả "trên giấy" xấu đi — W07 dùng đúng các khái niệm đó để đinh lượng rủi ro.

**Fast-Track Core (30–45 phút):** đọc mục 1–4 (20–25 phút), làm Mini-review 1–2 (5–10 phút), làm quiz 8 câu (8–10 phút), rồi đối chiếu answer key. Phần đào sâu gồm mục "Biết được gì / Chưa thể kết luận gì" và hoạt động tự nguyện.

## 2. Mục tiêu học tập đo được
- [LO-W07-01] Định nghĩa quản trị rủi ro (risk management), ngân sách rủi ro (risk budget), quy mô vị thế (position sizing), lệnh dừng (stop order/stop-loss), giá dừng (stop price), lệnh dừng–giới hạn (stop-limit order), tỷ lệ lợi nhuận/rủi ro (Risk/Reward Ratio — R:R) bằng tên Việt–Anh với đơn vị đúng (VND, VND/cổ phần, cổ phiếu, %), và nêu đúng vai của chúng: **heuristic kiểm soát rủi ro theo kế hoạch — không phải bảo đảm giới hạn lỗ, không phải dự báo**.
- [LO-W07-02] Tính, trên dữ liệu giả định: ngân sách rủi ro = giá trị danh mục × % rủi ro (VND); số lượng vị thế = ngân sách rủi ro / rủi ro mỗi cổ phiếu (VND/cổ phần), kèm **đệm phí/trượt giá (buffer)** và làm tròn xuống số nguyên cổ phiếu; tổng rủi ro danh mục = tổng rủi ro các vị thế (VND); R:R (lần) và kỳ vọng (expectancy, VND/cổ phần) từ bảng xác suất–kết quả giả định, ghi rõ xác suất là **giả định của người tính**.
- [LO-W07-03] Phân tích các kịch bản lỗ **vượt** kế hoạch (gap mở cửa dưới giá dừng, thanh khoản cạn làm khớp lệch hoặc khớp một phần — partial fill) và các đại lượng danh mục: mức sụt giảm tối đa (Maximum Drawdown — MDD, %), bất đối xứng phục hồi (mất x% cần lãi x/(1−x) %), và tác động của tương quan/tập trung khiến nhiều vị thế kích dừng cùng lúc.
- [LO-W07-04] Phản biện các khẳng định kiểu "stop loss bảo đảm lỗ tối đa", "quy tắc x% là tối ưu" bằng cách chỉ ra: giá dừng là trigger không phải giá khớp (SEC), stop-limit có thể không khớp, chuẩn xác định giá dừng khác nhau giữa các đầu mối, sai lầm tự ý dời stop, và rủi ro cộng dồn danh mục; ghi đúng trạng thái "biết được" và "chưa thể kết luận".

## 3. Khái niệm cốt lõi
### 3.1. Rủi ro là gì: bất định và mất tiền tiềm năng — không có bảo đảm
Investor.gov (U.S. SEC), trang "What is Risk?", định nghĩa: "In finance, risk refers to the degree of uncertainty and/or potential financial loss inherent in an investment decision" — trong tài chính, rủi ro là **mức độ bất định và/hoặc mất tiền tiềm năng** gắn với một quyết định đầu tư; và "as investment risks rise, investors seek higher returns to compensate themselves for taking such risks" — khi rủi ro tăng, nhà đầu tư đòi lợi nhuận cao hơn để bù đắp. Trang này liệt kê các loại rủi ro W07 cần: **rủi ro biến động (volatility risk)** — giá cổ phiếu dao động lên xuống kể cả khi doanh nghiệp không gặp nguy; Investor.gov nêu ví dụ dữ liệu Hoa Kỳ: cổ phiếu công ty lớn "have lost money on average about one out of every three years" — trung bình mất tiền khoảng một trong ba năm; và **rủi ro thanh khoản (liquidity risk)** — "the risk that investors won't find a market for their securities, potentially preventing them from buying or selling when they want" — rủi ro không tìm được người mua/bán khi cần. Về "bảo đảm": chứng khoán sở hữu "are not insured against loss in value" — không được bảo hiểm tổn thất giá trị (SIPC chỉ bồi hoàn chứng khoán thất lạc khi công ty môi giới phá sản, không bù mất giá). [W07-C01]

Đánh dấu vai: hai phát biểu trên là **mô tả bản chất rủi ro theo cơ quan quản lý Hoa Kỳ**, không phải công thức dự báo; ví dụ "một trong ba năm" là dữ liệu lịch sử thị trường Hoa Kỳ mà nguồn trích, không tự động lặp và không áp cho thị trường Việt Nam.

### 3.2. Ngân sách rủi ro: quy định "mất tối đa bao nhiêu" TRƯỚC KHI vào lệnh
**Quản trị rủi ro (risk management)** trong bài học này là quy trình đặt **giới hạn tổn thất bằng số** trước khi ra quyết định, rồi giữ đúng giới hạn. Khối gạch đầu tiên là **ngân sách rủi ro (risk budget)**:

`Ngân sách rủi ro (VND) = Giá trị danh mục (VND) × % rủi ro chấp nhận mỗi thương vụ`

Ví dụ quy ước phổ biến trong sách về giao dịch là 1–2% mỗi thương vụ. **Đánh dấu đúng vai:** con số % này là **heuristic** — quy ước thực hành được truyền miệng để giới hạn số lần "thua lớn" liên tiếp, **không phải con số tối ưu được nguồn chính thống nào đã mở chứng minh**, và không có nguồn đã mở trong phiên này khẳng định mức % nào "đúng". Điều duy nhất có cơ sở là nguyên lý bên dưới theo Investor.gov: rủi ro là mất tiền tiềm năng, nên việc đặt trần số tiền chịu mất là cách **định lượng** rủi ro thay vì cảm tính. Phần trăm bao nhiêu là quyết định của từng người, gắn với khả năng chịu lỗ và mục tiêu riêng — bài học không khuyến nghị mức nào. [W07-C02]

Hai tính chất cần ghi:
- Ngân sách rủi ro đo bằng **VND** (số tiền), không phải % — % chỉ là cách đặt mốc; khi danh mục tăng/giảm, số VND tương ứng cùng thay đổi.
- Ngân sách **danh mục** ≠ ngân sách một thương vụ: nếu vào 3 lệnh, tổng "rủi ro đang mở" có thể gấp 3 — mục 3.7 và 4.6 tính kịch bản cộng dồn.

### 3.3. Position sizing: công thức nối ngân sách rủi ro với khoảng giá vô hiệu hóa
**Quy mô vị thế (position sizing)** là bước tính **số lượng cổ phiếu** sao cho nếu luận điểm sai (giá chạm điểm vô hiệu hóa — thường trùng với giá dừng), tổng lỗ **theo kế hoạch** không vượt ngân sách rủi ro:

`Số lượng (cổ phiếu) = Ngân sách rủi ro (VND) / Rủi ro mỗi cổ phiếu (VND/cổ phần)`

với `Rủi ro mỗi cổ phiếu = Giá mua − Giá dừng (VND/cổ phần)` cho vị thế mua dài (long); cộng thêm **đệm (buffer)** phí giao dịch và trượt giá ước tính ở mục 3.4 nếu muốn số liệu bảo thủ hơn — và nên làm tròn **xuống** số nguyên cổ phiếu, vì không mua được cổ phiếu lẻ.

Đơn vị nhất quán: ngân sách rủi ro **VND**; rủi ro mỗi cổ phiếu **VND/cổ phần**; thương số **cổ phiếu**. Kết quả là phép chia hai số tiền → số lượng. Ba điểm đọc đúng:
- Công thức trả lời "bao nhiêu", không trả lời "mua cổ phiếu nào" hay "lệnh này có lãi không" — chất lượng tín hiệu vào/ra thuộc W05–W06.
- Khoảng giá dừng **xa** → số lượng nhỏ; khoảng dừng **gấp** (dừng sát) → số lượng lớn. Vì vậy "dừng sát để mua nhiều" tự tăng nhạy cảm với nhiễu giá ngắn hạn — SEC cảnh báo biến động trong phiên có thể kích lệnh dừng (mục 3.5).
- Khi hết ngân sách rủi ro phiên, quy ước heuristic là **dừng vào lệnh mới** — gấp thếp (double down) để "gỡ" là hành vi tăng rủi ro sau thua lỗ, phản lại chính ngân sách đã đặt. [W07-C02]

### 3.4. Phí, gap, slippage, thanh khoản: vì sao lỗ thực tế vượt kế hoạch
Công thức 3.3 tính **lỗ theo kế hoạch với giá lý tưởng**. Thực tế khớp lệnh đi qua bốn ma sát, tất cả đều làm lỗ thực tế **lớn hơn** con số kế hoạch:
- **Phí giao dịch (commission):** theo Investor.gov, nhà đầu tư thường phải trả phí khi mua/bán chứng khoán (đã mở và kiểm chứng tại W06-C05); trong ví dụ của bài, phí tính theo % giá trị giao dịch mỗi chiều — đây là giả định minh họa, biểu phí thực tế phải tra công bố chính thức.
- **Trượt giá (slippage):** chênh lệch giữa giá kỳ vọng/giá kích hoạt và giá khớp thực tế. Investor Bulletin "Understanding Order Types" (cập nhật 2026-08-18) viết về lệnh thị trường: "The price at which a market order will execute often deviates from the last-traded price or 'real time' quote due to the demand for a stock and the prices of available liquidity at the time of execution" — giá khớp thường lệch khỏi giá khớp gần nhất/báo giá thời gian thực do cầu mua và giá thanh khoản sẵn có tại thời điểm khớp.
- **Thanh khoản và khớp một phần (partial fill):** cùng bulletin: "parts of a large market order may execute at different prices due to lack of liquidity at a price at the time of execution" — một phần lệnh lớn có thể khớp ở các mức giá khác nhau do thiếu thanh khoản; và trang "What is Risk?" định nghĩa rủi ro thanh khoản là nguy cơ không tìm được người đối diện khi muốn mua/bán (3.1).
- **Khoảng trống giá (gap risk):** giá phiên sau mở xa hơn giá đóng phiên trước, nhảy qua khỏi giá dừng; khi đó lệnh dừng chỉ kích hoạt sau khi giá đã "lọt" qua và bán vào mức thấp hơn. Bài học không trích số liệu gap của sàn nào — hiện tượng được minh họa bằng dữ liệu giả định ở 4.3.

Tổng hợp thành **rủi ro mỗi cổ phiếu thực tế** (bảo thủ) = (giá mua + phí mua) − (giá khớp dừng ước tính − phí bán), đưa lại vào mẫu số của 3.3 làm **giảm** số lượng được phép mua — mục 4.2 tính con số. [W07-C03]

### 3.5. Stop, stop-limit, trailing stop: cơ chế và bốn giới hạn
Investor.gov "Types of Orders" định nghĩa: "A stop order, also referred to as a stop-loss order is an order to buy or sell a stock once the price of the stock reaches the specified price, known as the stop price. When the stop price is reached, a stop order becomes a market order" — lệnh dừng là lệnh mua/bán khi giá chạm **giá dừng (stop price)**; khi đó nó **biến thành lệnh thị trường**. "A sell stop order is entered at a stop price below the current market price. Investors generally use a sell stop order to limit a loss or protect a profit on a stock they own" — lệnh dừng bán đặt dưới giá hiện tại, mục đích chung là hạn chế lỗ hoặc bảo vệ lợi nhuận. Hai điểm đối chiếu với 3.4: lệnh thị trường "guarantees that the order will be executed, but does not guarantee the execution price" — bảo đảm khớp nhưng **không bảo đảm giá khớp**; lệnh giới hạn ngược lại "is not guaranteed to execute" — có thể không khớp. [W07-C04]

**Bốn giới hạn**, theo nguyên văn SEC:
1. **Giá dừng ≠ giá khớp bảo đảm.** Bulletin "Understanding Order Types": "The stop price is not the guaranteed execution price for a stop order. The stop price is a trigger that causes the stop order to become a market order. The execution price an investor receives for this market order can deviate significantly from the stop price due to the demand for a stock and the prices of available liquidity." — giá dừng chỉ là **điểm kích hoạt (trigger)**; giá khớp có thể lệch **xa đáng kể**. Tài liệu SEC "Trading Basics" (đã mở và kiểm chứng tại W06-C05) nêu cùng cơ chế: trong thị trường biến động nhanh, giá khớp thường lệch khỏi báo giá gần nhất.
2. **Stop-limit đổi "chắc khớp" lấy "chắc giá" — và có thể không khớp.** "A stop-limit order... combines the features of a stop order and a limit order. Once the stop price is reached, a stop-limit order becomes a limit order"; lợi ích là "the investor can control the price at which the order can be executed"; nhưng "As with all limit orders, a stop-limit order may not be executed if the stock's price moves away from the specified limit price" — nếu giá chạy qua khỏi mức giới hạn, lệnh không khớp và vị thế **vẫn mở**. Ví dụ của bulletin: lệnh dừng–giới hạn bán với stop 3,00 USD và limit 2,50 USD chỉ khớp ở 2,50 USD hoặc tốt hơn.
3. **Chuẩn kích hoạt khác nhau giữa các đầu mối.** "Different trading venues and firms have different standards for determining whether a stop price has been reached. Some use only last-sale prices to trigger a stop order, while others use quotation prices" — có nơi dùng giá khớp gần nhất, có nơi dùng báo giá; SEC khuyên hỏi chính firm mình dùng chuẩn nào. Cùng bulletin ghi thêm: "Stop, stop-limit, and trailing stop orders may not be available through all brokerage firms."
4. **Biến động trong phiên có thể kích lệnh sớm và khớp tệ hơn giá đóng.** Bulletin "Stop, Stop-Limit, and Trailing Stop Orders" (cập nhật 2026-08-18): "A stop order may be triggered by a short-term, intraday price move that results in an execution price for the stop order that is substantially worse than the stock's closing price for the day."

**Lệnh dừng trượt (trailing stop order):** stop price không cố định mà là khoảng % hoặc số tiền dưới giá thị trường; giá tăng thì mức dừng trượt theo, giá giảm thì mức dừng đứng yên. Ví dụ của SEC: mua 20 USD, đặt trailing 1 USD; giá đỉnh 24 USD → mức dừng 23 USD; bán khi giá về 23 USD, "though the execution price may deviate from 23 USD" — giá khớp vẫn có thể lệch khỏi 23 USD. Đánh dấu vai: loại lệnh này cũng thuộc nhóm trigger, chịu cùng giới hạn 1–4. Và giới hạn nguồn bắt buộc: các mô tả trên là **tài liệu giáo dục của SEC Hoa Kỳ** (bulletin ghi rõ "It is not a rule, regulation, or statement of the Securities and Exchange Commission"); loại lệnh nào khả dụng, biên độ dao động, phí và cơ chế khớp tại Việt Nam phải tra công bố chính thức trong nước — bài học không xác minh và không nêu. [W07-C04]

### 3.6. Tương quan và tập trung: ngân sách rủi ro danh mục không tự "phân tán"
CFA Institute (2026 Curriculum, bài đọc *Portfolio Risk and Return: Part I*, Level I Portfolio Management) khẳng định: "Correlations among individual assets along with risk and return are important determinants of portfolio risk" — tương quan giữa các tài sản, cùng lợi nhuận và rủi ro riêng, là yếu tố quyết định rủi ro danh mục; "The risk of a two-asset portfolio is dependent on the proportions of each asset, their standard deviations and the correlation (or covariance) between the assets' returns. As the number of assets in a portfolio increases, the correlation among asset risks becomes a more important determinant of portfolio risk"; và "Combining assets with low correlations reduces portfolio risk" — kết hợp các tài sản có tương quan thấp làm giảm rủi ro danh mục. [W07-C05]

Áp cho W07 (đọc cẩn thận, đúng vai):
- **Cộng rủi ro theo kịch bản, không theo trung bình:** ba vị thế mỗi vị thế "rủi ro 1% ngân sách" **không** tạo ra danh mục "an toàn 1%" — nếu kịch bản xấu kích cả ba dừng cùng lúc, tổng lỗ là ~3%. Câu "các vị thế cùng ngành/gắn bó thường kích dừng cùng lúc" là **suy luận minh họa** từ khẳng định tương quan của CFA (tương quan cao làm giảm tác dụng phân tán); bài học không có dữ liệu tương quan thực của mã Việt Nam nào.
- **Tập trung (concentration):** một vị thế chiếm tỷ trọng giá trị lớn của danh mục (ví dụ >20% — con số minh họa) khiến kết quả danh mục bám sát kết quả một cổ phiếu. Không nguồn đã mở công bố tỷ lệ tập trung "tối đa" nào — mọi ngưỡng kiểu "không quá x%" là quy ước cá nhân; kết luận đúng là **"chưa thể kết luận"** về con số tối ưu.
- **Phân tán thật cần tương quan thấp:** theo nguồn CFA đã mở, chỉ khi các tài sản có tương quan thấp thì rủi ro danh mục mới giảm; mua 3 mã cùng ngành là hành động "mua tên khác" chứ chưa chắc là phân tán. Phần định lượng MPT đầy đủ thuộc W08.

### 3.7. Risk/reward, expectancy, MDD và bất đối xứng phục hồi — đọc cẩn thận
**Tỷ lệ lợi nhuận/rủi ro (Risk/Reward Ratio — R:R):** `R:R = Lợi nhuận kỳ vọng mỗi cổ phiếu / Rủi ro mỗi cổ phiếu` — cả hai đều bằng **VND/cổ phần**, kết quả là **lần (không đơn vị)**. R:R 2:1 nghĩa là: **nếu kịch bản mục tiêu đạt**, lãi gấp đôi mức lỗ kế hoạch khi dừng. Đọc đúng: R:R là **tỷ lệ hai khoảng giá do chính bạn chọn** — nó không chứa xác suất đạt/không đạt, và không nói gì về khả năng thực tế.

**Kỳ vọng (expectancy):** trung bình trọng số xác suất của kết quả mỗi lệnh:

`E (VND/cổ phần) = p_thắng × Lãi mỗi cp − (1 − p_thắng) × Lỗ mỗi cp`

Điểm tựa duy nhất là **phép tính giá trị kỳ vọng chuẩn** (calculation only — định nghĩa của bài học). Ba cảnh báo bắt buộc:
- **Xác suất p là giả định của người tính.** Khác với tung xúc xắc, "xác suất thắng" của một thương vụ không quan sát được trước khi giao dịch; mọi con số p đưa vào đều là giả định khai báo, không phải dữ kiện.
- **Expectancy dương trên giấy không chứng minh chiến lược có lãi:** kết quả lịch sử có thể là sản phẩm của chọn lọc mẫu (W06: data-snooping, survivorship — "appearance of predictability"), và dạng yếu hiệu quả (W06) nghĩa là dữ liệu giá quá khứ đã nằm trong giá. Trạng thái đúng là **"chưa thể kết luận"** nếu không có kiểm định ngoài mẫu và chi phí.
- **Chi phí làm expectancy xấu đi theo cả hai nhánh** (mục 4.4 tính: một bảng giả định mất ~68% expectancy trên giấy sau phí và slippage).

**Mức sụt giảm tối đa (Maximum Drawdown — MDD):** mức giảm sâu nhất của giá trị danh mục từ đỉnh (peak) xuống đáy (trough) trước khi phục hồi: `MDD = (Đỉnh − Đáy) / Đỉnh`, đơn vị %. **Bất đối xứng phục hồi (recovery asymmetry):** sau khi mất x%, cần lãi `x/(1−x)` % trên vốn còn lại mới về ngang đỉnh — mất 20% cần +25%; mất 50% cần +100%. Đây là phép tính của bài học (calculation only) giải thích vì sao "giới hạn lỗ mỗi lệnh" quan trọng hơn "kiếm thêm mỗi lệnh": chuỗi thua lỗ sâu đẩy mức phục hồi cần thiết lên nhanh hơn tuyến tính. [W07-C05][W07-C06]

## 4. Ví dụ và worked examples (giả định, đơn vị VND/cổ phần)
### 4.1. Dữ liệu nền — "Công ty V" (hư cấu hoàn toàn)
**Giả định của người viết:** danh mục giả định **100.000.000 VND**; ngân sách rủi ro mỗi thương vụ **1% = 1.000.000 VND** (heuristic minh họa, không phải khuyến nghị); một cổ phiếu hư cấu "Công ty V": mua tại **50.000 VND/cp**, giá dừng giả định **47.500 VND/cp** (−5,00% so với giá mua), mục tiêu giả định **55.000 VND/cp** (+10,00%); phí giao dịch giả định **0,15%/chiều** tính trên giá trị giao dịch; phí làm tròn **lên** số nguyên VND/cp (quy ước bảo thủ); slippage và gap là **giả định khai báo** ở từng mục, không phải dữ liệu thị trường nào. Không con số nào trong mục 4 là dữ liệu thị trường thật. [W07-C06]

### 4.2. Tính số lượng: kiểu naïve so với có đệm phí/slippage (calculation only)
**Cách naïve (không đệm):** rủi ro mỗi cp = 50.000 − 47.500 = **2.500 VND/cp**; số lượng = 1.000.000/2.500 = **400 cổ phiếu**; giá trị vị thế = 400 × 50.000 = **20.000.000 VND** (20% danh mục — ghi chú tập trung ngay tại đây).

**Cách có đệm (bảo thủ hơn), cùng giả định 3.4:**
- Chi phí gốc mỗi cp = 50.000 + 0,15% × 50.000 = 50.000 + 75 = **50.075 VND/cp**.
- Giả định slippage khi dừng kích hoạt: khớp **47.250 VND/cp** (250 VND/cp dưới giá dừng, ≈0,53%); phí bán = 0,15% × 47.250 = 70,875 → làm tròn lên **71 VND/cp**; thu ròng mỗi cp = 47.250 − 71 = **47.179 VND/cp**.
- Rủi ro mỗi cp thực tế = 50.075 − 47.179 = **2.896 VND/cp**.
- Số lượng = 1.000.000/2.896 = 345,29… → làm tròn xuống **345 cổ phiếu**; giá trị vị thế = 345 × 50.000 = **17.250.000 VND** (17,25% danh mục).

| Kịch bản dừng (giả định) | Lỗ mỗi cp (VND/cp) | Số lượng | Tổng lỗ (VND) | So ngân sách 1.000.000 VND |
|---|---:|---:|---:|---|
| Naïve: khớp đúng 47.500, không phí | 2.500 | 400 | 1.000.000 | bằng ngân sách |
| Naïve nhưng đi qua ma sát thật (2.896/cp) | 2.896 | 400 | **1.158.400** | vượt **158.400 VND (+15,84%)** |
| Có đệm: khớp 47.250 + phí hai chiều | 2.896 | 345 | **999.120** | ≤ ngân sách ✓ |

Đọc kết quả: cùng một thế giới thực, cách naïve **vượt ngân sách 15,84%** chỉ vì bỏ qua phí và slippage; đệm đưa số lượng từ 400 xuống 345 và giữ lỗ kế hoạch 999.120 VND ≤ 1.000.000 VND. Đây là số học, không phải dự đoán giá. [W07-C02][W07-C03][W07-C06]

### 4.3. Kịch bản gap: lỗ thực tế vượt ngân sách ngay cả khi đã có đệm (calculation only)
**Giả định:** phiên kế mở cửa gap xuống **45.000 VND/cp** (dưới giá dừng 47.500) và lệnh dừng (đã biến thành lệnh thị trường) khớp tại 45.000 — bỏ qua slippage thêm để minh họa đơn giản (khai báo). Phí bán = 0,15% × 45.000 = 67,5 → làm tròn lên **68 VND/cp**; thu ròng mỗi cp = 45.000 − 68 = **44.932 VND/cp**; lỗ mỗi cp = 50.075 − 44.932 = **5.143 VND/cp**; tổng lỗ = 345 × 5.143 = **1.774.335 VND ≈ 1,77% danh mục** — vượt ngân sách 1% **khoảng 1,8 lần** dù số lượng đã tính bảo thủ. [W07-C03][W07-C04][W07-C06]

Kết luận phương pháp (không phải dự báo): lệnh dừng **giới hạn thời điểm rời vị thế**, không **bảo đảm mức lỗ tối đa** — đúng giới hạn SEC ở 3.5. Khoảng cách lỗ kế hoạch ↔ lỗ thực tế đến từ gap, slippage, phí và thanh khoản, mà không heuristic kích thước nào xóa được hoàn toàn; heuristic chỉ làm lỗ "thường xuyên hơn" nằm gần kế hoạch.

### 4.4. Risk/reward và expectancy: trên giấy so với sau chi phí (calculation only)
**Trên giấy:** reward = 55.000 − 50.000 = 5.000 VND/cp; risk = 2.500 VND/cp; **R:R = 2,0:1**.

**Sau chi phí (giả định 4.2):** nhánh thắng — giả định khớp mục tiêu tại **54.900 VND/cp** (slippage 100 VND/cp ≈ 0,18%); phí bán = 0,15% × 54.900 = 82,35 → làm tròn lên **83 VND/cp**; thu ròng = 54.817; lãi mỗi cp = 54.817 − 50.075 = **4.742 VND/cp**. Nhánh thua — lỗ mỗi cp **2.896 VND/cp** (4.2). R:R sau chi phí = 4.742/2.896 ≈ **1,64:1**.

**Kỳ vọng, với p_thắng = 40% (giả định khai báo, không phải dữ kiện):**

| Đại lượng | Công thức | Kết quả |
|---|---|---:|
| E trên giấy | 0,4 × 5.000 − 0,6 × 2.500 | **+500 VND/cp** |
| E sau chi phí | 0,4 × 4.742 − 0,6 × 2.896 = 1.896,8 − 1.737,6 | **+159,2 ≈ +159 VND/cp** |
| Mất mát do chi phí | (500 − 159,2)/500 | **≈ 68%** |

Ba lần đọc bắt buộc: (1) cả hai con số E đều xây trên p = 40% **giả định** — đổi p thành 30%: E sau chi phí = 0,3 × 4.742 − 0,7 × 2.896 = 1.422,6 − 2.027,2 = **−604,6 VND/cp**, kỳ vọng **âm**; (2) chi phí bào mòn expectancy theo cả hai nhánh, nên "R:R 2:1 trên giấy" là giới hạn trên lý tưởng; (3) E dương trong một bảng giả định **không chứng minh** chiến lược có lãi — còn thiếu xác suất thật, kiểm định ngoài mẫu và bối cảnh thị trường (W06). [W07-C05][W07-C06]

### 4.5. MDD và bất đối xứng phục hồi (calculation only)
**Giả định:** chuỗi giá trị danh mục giả định: 100,0 → 112,0 (đỉnh) → 89,6 (đáy) → 96,0 triệu VND.

- MDD = (112,0 − 89,6)/112,0 = 22,4/112,0 = **20,0%**.
- Phục hồi từ đáy về đỉnh = 22,4/89,6 = **+25,0%** — lớn hơn MDD.
- Công thức tổng quát: cần lãi `MDD/(1 − MDD)`: 0,20/0,80 = 25,0% ✓; với MDD 50% → 0,50/0,50 = **+100%** ✓ (chính là câu quiz Q07-06: mất một nửa phải kiếm gấp đôi số còn lại).

Ý nghĩa: chuỗi thua lỗ chồng lên nhau làm "chi phí phục hồi" tăng nhanh hơn tuyến tính — lý do ngân sách rủi ro mỗi lệnh nhỏ và tổng rủi ro mở bị trần hóa là **heuristic hợp lý**, dù không có mức % nào được chứng minh tối ưu. [W07-C05][W07-C06]

### 4.6. Kiểm tra danh mục: cộng rủi ro theo kịch bản, tương quan giả định, tập trung (calculation only)
**Giả định mở rộng:** sau lệnh Công ty V (345 cp, rủi ro kế hoạch 999.120 VND), cùng phiên người học còn hai vị thế giả định khác — "Công ty A" và "Công ty B" (đều hư cấu) — mỗi vị thế ngân sách rủi ro 1.000.000 VND. Ba vị thế được mô tả là cùng một nhóm ngành giả định (tương quan giả định cao, ω ≈ 0,8 — chỉ là con số khai báo minh họa, không phải dữ liệu thị trường).

- **Tổng rủi ro mở theo kịch bản xấu đồng thời:** 999.120 + 1.000.000 + 1.000.000 = **2.999.120 VND ≈ 3,0% danh mục** — gấp 3 ngân sách một lệnh. Kịch bản "ba dừng cùng lúc" là kịch bản phải xếp vào ngân sách danh mục, không phải trung bình ba lệnh.
- **Tương quan:** theo CFA (3.6), tương quan giữa tài sản là yếu tố quyết định rủi ro danh mục và chỉ tài sản tương quan thấp mới làm giảm rủi ro khi kết hợp — vì vậy "cùng ngành nên kích dừng cùng lúc" được đọc là **giả thuyết rủi ro chờ kiểm chứng**, và việc trần hóa tổng rủi ro mở là cách ứng phó heuristic.
- **Tập trung:** Công ty V chiếm 17,25% giá trị danh mục (4.2). Không nguồn đã mở công bố ngưỡng tập trung "đúng"; bài học ghi **"chưa thể kết luận"** về con số, chỉ ghi nhận cơ chế: tỷ trọng càng cao, kết quả danh mục càng phụ thuộc một cổ phiếu. [W07-C05][W07-C06]

### 4.7. Phân loại bằng chứng
- **Kết quả/nguồn (đã mở trực tiếp trong phiên này):** Investor.gov "What is Risk?" cho định nghĩa rủi ro, rủi ro biến động, rủi ro thanh khoản, dữ liệu "một trong ba năm" của cổ phiếu công ty lớn Hoa Kỳ và tính không được bảo hiểm của tổn thất giá trị; Investor.gov "Types of Orders" cho định nghĩa lệnh thị trường/giới hạn/dừng và mục đích của sell stop; Investor Bulletin "Understanding Order Types" (cập nhật 2026-08-18) cho tính không bảo đảm của giá khớp, partial fill vì thiếu thanh khoản, stop price là trigger, giới hạn stop-limit, chuẩn kích hoạt khác nhau; Investor Bulletin "Stop, Stop-Limit, and Trailing Stop Orders" (cập nhật 2026-08-18) cho cảnh báo biến động trong phiên và ví dụ trailing stop; CFA 2026 *Portfolio Risk and Return: Part I* cho các khẳng định về tương quan và rủi ro danh mục; SEC "Trading Basics" (đã mở, kiểm chứng tại W06-C05) được tái sử dụng cho cơ chế "giá dừng là trigger, giá khớp có thể lệch xa".
- **Giả định minh họa:** danh mục 100.000.000 VND, ngân sách 1%, Công ty V/A/B hư cấu, giá mua/dừng/mục tiêu 50.000/47.500/55.000 VND/cp, phí 0,15%/chiều, slippage 250/100 VND/cp, fill gap 45.000 VND/cp, p_thắng = 40% hoặc 30%, tương quan ω ≈ 0,8, chuỗi MDD 100→112→89,6→96 triệu VND.
- **Phép tính/quy ước:** công thức ngân sách rủi ro, số lượng vị thế, R:R, expectancy (giá trị kỳ vọng chuẩn), MDD và x/(1−x); quy ước làm tròn phí lên/số lượng xuống; quy ước "1–2% mỗi lệnh" và mọi ngưỡng tập trung là heuristic của thực hành, không phải phát hiện được nguồn chứng minh.

## 5. Mini-review và truy xuất kiến thức
### Mini-review 1: tính lại số lượng có đệm và kiểm tra ngân sách
Không nhìn phần trên: danh mục 100.000.000 VND, ngân sách rủi ro 1%; mua Công ty V 50.000 VND/cp, giá dừng 47.500 VND/cp; phí 0,15%/chiều (làm tròn lên), slippage giả định khớp dừng 250 VND/cp dưới giá dừng. (a) Rủi ro mỗi cp thực tế? (b) Số lượng tối đa? (c) Tổng lỗ kế hoạch và so ngân sách? **Tự kiểm:** (a) gốc 50.075; thu ròng 47.250 − 71 = 47.179; rủi ro = 50.075 − 47.179 = **2.896 VND/cp**. (b) 1.000.000/2.896 = 345,29 → **345 cổ phiếu**. (c) 345 × 2.896 = **999.120 VND ≤ 1.000.000** ✓. Ghi chú đơn vị: VND/cp cho (a), cổ phiếu cho (b), VND cho (c); không suy gì về hướng giá sau dừng.

### Mini-review 2: tự phản biện một bài đăng "stop 5% là an toàn tuyệt đối"
Một bài đăng giả định viết: "Chỉ cần đặt stop −5%, bạn KHÔNG BAO GIỜ mất quá 5% mỗi lệnh — an toàn tuyệt đối, và 3/3 lệnh của tôi đều chỉ mất đúng 5%." Liệt kê ít nhất sáu lỗi và trạng thái kết luận đúng. **Tự kiểm:** (1) "KHÔNG BAO GIỜ mất quá 5%" là ngôn ngữ **bảo đảm** — SEC: giá dừng là trigger, giá khớp "can deviate significantly from the stop price"; (2) gap mở cửa dưới giá dừng (4.3) đưa lỗ thực tế lên ~1,77% danh mục trong ví dụ giả định; (3) mất thanh khoản → partial fill hoặc không khớp; nếu đổi sang stop-limit thì "may not be executed" — vị thế vẫn mở; (4) chuẩn kích hoạt khác nhau giữa các đầu mối — "5%" đo theo giá khớp hay báo giá chưa định nghĩa; (5) "3/3 lệnh" là chọn lọc mẫu (W06: data-snooping/survivorship) và chưa trừ phí; (6) 5% giá cổ phiếu ≠ 5% danh mục nếu vị thế >100% danh mục (đòn bẩy) — và quên tổng rủi ro mở của các lệnh khác. Kết luận đúng: stop là **heuristic kiểm soát thời điểm rời vị thế**; mức lỗ tối đa **không được bảo đảm**; về "an toàn tuyệt đối" — khẳng định sai, đúng trạng thái là "lỗ có thể vượt kế hoạch"; về độ tin cậy của quy tắc 5% — **chưa thể kết luận**.

## 6. Cảnh báo & sai lầm thường gặp
- **Tự ý dời stop hoặc hủy stop khi giá chạm ngưỡng** ("hy vọng hồi"): phá vỡ chính ngân sách rủi ro đã đặt; lỗ có kiểm soát biến thành lỗ không kiểm soát. Nếu buộc phải xem xét lại, làm **trước** phiên, bằng chữ viết, không phải giữa biến động.
- **Coi stop là bảo đảm mức lỗ tối đa:** SEC — stop price là trigger, không phải giá khớp bảo đảm; gap, slippage, mất thanh khoản đều đẩy lỗ thực tế vượt kế hoạch (4.3); stop-limit đổi rủi ro "khớp tệ" lấy rủi ro "không khớp".
- **Quên đệm phí/slippage khi tính số lượng:** cách naïve trong 4.2 vượt ngân sách 15,84% chỉ vì hai ma sát nhỏ.
- **Cộng ngân sách theo lệnh, quên tổng rủi ro mở:** ba lệnh "1%" có thể là ~3% danh mục trong một kịch bản xấu đồng thời; tương quan cao giữa các vị thế làm kịch bản đồng thời khả dĩ hơn (CFA: tương quan là yếu tố quyết định rủi ro danh mục).
- **Nhầm R:R với xác suất:** R:R 2:1 không chứa p; expectancy cần p khai báo là giả định; đổi p 40%→30% làm E từ +159 xuống −605 VND/cp trong bảng 4.4.
- **Gấp thếp sau thua (revenge/double down):** dùng ngân sách của lệnh sau để "gỡ" lệnh trước; chuỗi thua đẩy mức phục hồi cần thiết theo x/(1−x) — mất 50% cần +100%.
- **Dùng đòn bẩy/margin để "đủ số lượng" mà ngân sách rủi ro không đổi:** đòn bẩy phóng đại tổn thất và thêm rủi ro gọi ký quỹ — bài học không mở nguồn về margin tại Việt Nam nên không đi vào chi tiết, chỉ ghi cảnh báo an toàn: không dùng tiền vay khi học khái niệm này.
- **Thiên kiến xác nhận — anchoring — overconfidence:** chỉ thu thập ví dụ "stop cứu mình", bám giá mua cũ khi tính dừng, nhầm hiểu công thức với khả năng thắng; thiếu ghi nhật ký (lý do vào, ngân sách, kết quả) là lỗi dữ liệu, không chỉ lỗi kỷ luật.

## 7. Biết được gì / Chưa thể kết luận gì
**Biết được:** định nghĩa rủi ro theo Investor.gov — bất định và/hoặc mất tiền tiềm năng, kèm rủi ro biến động và rủi ro thanh khoản; chứng khoán không được bảo hiểm tổn thất giá trị; ngân sách rủi ro = giá trị danh mục × % (VND) và quy mô vị thế = ngân sách / rủi ro mỗi cổ phiếu (VND/cp), nên có đệm phí + slippage và làm tròn xuống; bốn ma sát làm lỗ thực tế vượt kế hoạch — phí, slippage, gap, mất thanh khoản/partial fill — với cơ chế khớp lệnh thị trường theo SEC; cơ chế stop/stop-limit/trailing stop cùng bốn giới hạn (trigger ≠ giá khớp bảo đảm; stop-limit có thể không khớp; chuẩn kích hoạt khác nhau; biến động trong phiên có thể khớp tệ hơn giá đóng); khẳng định của CFA về tương quan: là yếu tố quyết định rủi ro danh mục, và tài sản tương quan thấp mới làm giảm rủi ro khi kết hợp; cách cộng rủi ro danh mục theo kịch bản xấu đồng thời; công thức R:R, expectancy (với p là giả định khai báo), MDD và bất đối xứng phục hồi x/(1−x); các số học giả định ở mục 4 đều kiểm tra lại được bằng tay.

**Chưa thể kết luận:** mức % rủi ro mỗi lệnh hoặc ngưỡng tập trung "tối ưu" — không nguồn đã mở chứng minh con số nào; "stop loss bảo đảm mất tối đa x%" — khẳng định sai theo tài liệu SEC đã mở (lỗ có thể vượt kế hoạch); xác suất thắng thật của một chiến lược — mọi p trong bài là giả định; expectancy dương trên giấy chứng minh chiến lược có lãi — còn thiếu xác suất thật, kiểm định ngoài mẫu, hiệu chỉnh chọn lọc và chi phí (W06); bất kỳ suy diễn nào áp thẳng tài liệu Hoa Kỳ (SEC, CFA) vào loại lệnh, biên độ dao động, phí, thuế, chuẩn kích hoạt hoặc hành vi giá tại thị trường Việt Nam — bối cảnh Việt Nam chưa được xác minh trong phiên này; hướng giá tương lai — bài học không tạo dự báo nào. Với bằng chứng chỉ là vài lệnh được chọn lọc, kết luận đúng là "chưa thể kết luận".

## 8. Checklist thực hành
- [ ] Viết mục tiêu, thời hạn và ngân sách rủi ro bằng **VND** (không chỉ %).
- [ ] Ghi nguồn, thời điểm, đơn vị (VND, VND/cp, cổ phiếu, %, lần) và trạng thái xác minh của từng con số.
- [ ] Tính số lượng vị thế có đệm phí/slippage, làm tròn xuống; kiểm tra tổng rủi ro mở theo **kịch bản xấu đồng thời**.
- [ ] Định nghĩa trước loại lệnh, giá dừng/giới hạn và chuẩn kích hoạt mình dùng — và tra công bố chính thức của đầu mối thực tế.
- [ ] Tách dữ kiện, giả định (p, slippage, tương quan), phép tính và kết luận; đánh dấu mọi kết luận chưa đủ bằng chứng là "chưa thể kết luận".
- [ ] Không dùng tiền thật, không dùng tiền vay, không đặt lệnh nào chỉ vì hoàn thành bài.

## 9. Hoạt động áp dụng tự nguyện (giả định, chỉ trên giấy)
Dùng **chỉ dữ liệu giả định tự tạo** (không tra cứu hay nhập dữ liệu thị trường thật): (1) tự đặt danh mục giả định và ngân sách rủi ro bằng VND; (2) tự viết giá mua, giá dừng, mục tiêu cho hai cổ phiếu hư cấu và tính số lượng có đệm phí/trượt giá như 4.2; (3) dựng bảng expectancy hai nhánh với p tự chọn, khai báo p là giả định, và tính cả nhánh "p giảm 10 điểm %"; (4) lập bảng tổng rủi ro mở theo kịch bản hai vị thế kích dừng cùng lúc; (5) tự phản biện: số dừng có đặt quá sát không, có quên phí không, tổng rủi ro có vượt trần danh mục không; (6) kết thúc bằng ba câu "chưa thể kết luận". Không dùng tiền thật, không đăng ký dịch vụ, không đặt lệnh nào.

## 10. Glossary
| Việt | English | Định nghĩa |
|---|---|---|
| Quản trị rủi ro | Risk Management | Quy trình đặt giới hạn tổn thất bằng số trước khi ra quyết định và giữ đúng giới hạn; trong bài học: heuristic kiểm soát, không phải bảo đảm kết quả. |
| Rủi ro | Risk | Theo Investor.gov: mức độ bất định và/hoặc mất tiền tiềm năng gắn với một quyết định đầu tư. |
| Rủi ro biến động | Volatility Risk | Rủi ro giá dao động lên xuống; Investor.gov nêu ví dụ Hoa Kỳ: cổ phiếu công ty lớn mất tiền trung bình khoảng một trong ba năm. |
| Rủi ro thanh khoản | Liquidity Risk | Theo Investor.gov: rủi ro không tìm được thị trường cho chứng khoán, khiến không mua/bán được khi muốn. |
| Ngân sách rủi ro | Risk Budget | Số tiền tối đa chấp nhận mất cho một thương vụ hoặc toàn bộ danh mục: giá trị danh mục × % rủi ro; đo bằng VND; mức % là heuristic. |
| Quy mô vị thế | Position Sizing | Số lượng cổ phiếu = ngân sách rủi ro / rủi ro mỗi cổ phiếu (VND/cp); nên có đệm phí + slippage và làm tròn xuống. |
| Rủi ro mỗi cổ phiếu | Per-Share Risk | Giá mua − giá dừng (+ đệm phí/trượt giá), đơn vị VND/cổ phần. |
| Lệnh dừng | Stop Order / Stop-Loss | Lệnh mua/bán khi giá chạm giá dừng; khi đó biến thành lệnh thị trường; dùng để hạn chế lỗ hoặc bảo vệ lợi nhuận theo mô tả SEC. |
| Giá dừng | Stop Price | Mức giá kích hoạt lệnh dừng; theo SEC là trigger, KHÔNG phải giá khớp bảo đảm. |
| Lệnh dừng–giới hạn | Stop-Limit Order | Kết hợp stop + limit: chạm giá dừng thì trở thành lệnh giới hạn; kiểm soát được giá khớp nhưng có thể không khớp nếu giá chạy qua mức giới hạn. |
| Lệnh dừng trượt | Trailing Stop Order | Lệnh dừng với mức dừng trượt theo giá theo hướng thuận; SEC ví dụ: mua 20 USD, trailing 1 USD, đỉnh 24 USD, bán tại 23 USD — giá khớp vẫn có thể lệch. |
| Khớp một phần | Partial Fill | Chỉ một phần lệnh được khớp; SEC: lệnh thị trường lớn có thể khớp ở nhiều mức giá do thiếu thanh khoản. |
| Trượt giá | Slippage | Chênh lệch giữa giá kỳ vọng/giá kích hoạt và giá khớp thực tế; thường tăng khi biến động nhanh và thanh khoản mỏng. |
| Khoảng trống giá | Gap Risk | Giá mở phiên sau nhảy vọt qua khỏi giá dừng khiến lệnh khớp ở mức thấp hơn kế hoạch; minh họa bằng dữ liệu giả định 4.3. |
| Tỷ lệ lợi nhuận/rủi ro | Risk/Reward Ratio (R:R) | Lợi nhuận kỳ vọng mỗi cp / rủi ro mỗi cp, đơn vị lần; không chứa xác suất đạt — hai khoảng giá do người học tự chọn. |
| Kỳ vọng | Expectancy | Trung bình trọng số xác suất: E = p_thắng × lãi − (1 − p_thắng) × lỗ (VND/cp); p là giả định khai báo, không quan sát được trước giao dịch. |
| Mức sụt giảm tối đa | Maximum Drawdown (MDD) | (Đỉnh − Đáy)/Đỉnh của chuỗi giá trị danh mục, đơn vị %; đo độ sâu sụt giảm trước khi phục hồi. |
| Bất đối xứng phục hồi | Recovery Asymmetry | Sau khi mất x% cần lãi x/(1−x) % trên vốn còn lại; mất 50% cần +100% — phép tính của bài học. |
| Tương quan | Correlation | Độ đo cùng biến động giữa lợi nhuận các tài sản; CFA: là yếu tố quyết định rủi ro danh mục, và tài sản tương quan thấp mới làm giảm rủi ro khi kết hợp. |
| Tập trung | Concentration | Tỷ trọng giá trị danh mục dồn vào một vị thế/nhóm gắn bó; tỷ trọng cao làm kết quả danh mục phụ thuộc một cổ phiếu; không có ngưỡng "đúng" nào được nguồn đã mở xác nhận. |
| Gấp thếp | Doubling Down | Tăng quy mô sau thua lỗ để "gỡ" — hành vi vi phạm ngân sách rủi ro, không phải chiến lược. |
| Điểm kích hoạt | Trigger | Vai của giá dừng theo SEC: mức giá khiến lệnh trở thành lệnh thị trường; không phải giá khớp bảo đảm. |
| Chưa thể kết luận | Not Yet Concluded | Trạng thái khi bằng chứng không đủ hoặc đã chọn lọc; cách ghi chuẩn thay cho mọi kết luận mua/bán/dự báo trong bài học này. |

## 11. Tài liệu tham khảo đã mở
- U.S. SEC — Investor.gov, "What is Risk?": https://www.investor.gov/introduction-investing/investing-basics/what-risk (truy cập 2026-09-15).
- U.S. SEC — Investor.gov, "Types of Orders" (mục How Stock Markets Work): https://www.investor.gov/introduction-investing/investing-basics/how-stock-markets-work/types-orders (truy cập 2026-09-15).
- U.S. SEC — Investor.gov, Investor Bulletin "Understanding Order Types – Investor Bulletin" (12/07/2017, cập nhật 18/08/2026): https://www.investor.gov/introduction-investing/general-resources/news-alerts/alerts-bulletins/investor-bulletins-14 (truy cập 2026-09-15).
- U.S. SEC — Investor.gov, Investor Bulletin "Stop, Stop-Limit, and Trailing Stop Orders – Investor Bulletin" (13/07/2017, cập nhật 18/08/2026): https://www.investor.gov/introduction-investing/general-resources/news-alerts/alerts-bulletins/investor-bulletins-15 (truy cập 2026-09-15).
- CFA Institute, "Portfolio Risk and Return: Part I" (2026 Curriculum, refresher reading, Level I Portfolio Management): https://www.cfainstitute.org/insights/professional-learning/refresher-readings/2026/portfolio-risk-return-part-1 (truy cập 2026-09-15).
- U.S. SEC, Investor Bulletin "Trading Basics: Understanding the Different Ways to Buy and Sell Stock" (Pub. No. 141): https://www.sec.gov/investor/alerts/trading101basics.pdf (đã mở và kiểm chứng trực tiếp tại phiên W06, 2026-09-15; W07 tái sử dụng trích dẫn đã ghi trong ledger W06-C05).
- Source ledger: [quality/module-2-source-ledger.md](../../quality/module-2-source-ledger.md).

## 12. Next steps
- [Quiz W07](../../assessments/quizzes/week-07-quiz.md)
- [Answer key W07](../../assessments/answer-keys/week-07-answers.md)
- Sau khi nắm W07, W08 (danh mục, asset allocation, Modern Portfolio Theory) sẽ định lượng phần "tương quan và phân tán" mà tuần này mới đặt nền: ngân sách rủi ro từng lệnh là bước trước, cấu trúc danh mục là bước sau — nhớ rằng theo CFA, tương quan giữa tài sản mới là yếu tố quyết định rủi ro danh mục khi số tài sản tăng.

## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

**Bài:** Ngân sách rủi ro. **Đầu ra:** Làm lại số lượng có đệm ở4.2 và tình huốnggap4.3.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| Ngân sách | Ghi số tiền tối đa bằngVND trước phép tính | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Position size | Dùng khoảng giá+đệm, làm tròn xuống và kiểm tra tích số | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Gap | Tính lại lỗ theo giákhớp giả định; không coi stop bảo đảm giá | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Danh mục | Cộng rủi ro mở trong kịch bản đồng thời, nêu tương quan giả định | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Kết luận | Tách expectancy với kết quả chắc chắn; không đặt lệnh hoặc vay để làm bài | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
