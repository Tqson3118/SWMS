> [!CAUTION]
> Nội dung chỉ mang tính giáo dục, KHÔNG phải lời khuyên đầu tư.
> Mọi quyết định đầu tư là trách nhiệm cá nhân của bạn.
> Hãy tham khảo chuyên gia tài chính phù hợp trước khi đầu tư.

# W04 — Các chỉ số tài chính cơ bản: thu nhập trên mỗi cổ phần (EPS), hệ số giá–thu nhập (P/E), hệ số giá–giá trị sổ sách (P/B), ROE, ROA và D/E

## 1. Vì sao cần học
Tuần này nối W01 (tách dữ kiện — giả định — kết luận), W02 (phân biệt vốn chủ và nợ) và W03 (đọc ba báo cáo tài chính) với bước tính toán đầu tiên: **các tỷ số tài chính** — mỗi tỷ số chỉ là một phép chia lấy tử số từ báo cáo kết quả kinh doanh và mẫu số từ báo cáo cân đối kế toán (hoặc ngược lại) đã học ở W03. Mục tiêu không phải tìm "cổ phiếu rẻ", mà xây quy trình đọc tỷ số có kiểm soát: nêu rõ **tử số, mẫu số, đơn vị, kỳ tính**, nhận diện trường hợp **mẫu số bằng 0 hoặc âm**, và dừng lại ở "chưa thể kết luận" khi thiếu dữ kiện. Bài học chỉ dùng các nguồn giáo dục đã mở trực tiếp trong phiên này: hướng dẫn cho nhà đầu tư của Ủy ban Chứng khoán Hoa Kỳ (U.S. SEC), giáo trình mở OpenStax *Principles of Finance 2e* (mục 6.1, 6.4, 6.5, 6.6), trang mô tả chuẩn IAS 33 của IFRS Foundation và bảng dữ liệu theo ngành của giáo sư Aswath Damodaran (NYU Stern, dữ liệu tháng 01/2026). Đây là nguồn của Hoa Kỳ/quốc tế; cách diễn giải và chuẩn mực kế toán tại Việt Nam chưa được xác minh trong phiên này. Không phần nào là khuyến nghị cá nhân và không yêu cầu dùng tiền thật.

**Kiến thức tiên quyết:** hoàn thành W01, W02 và W03. Cần nhớ từ W03: phương trình kế toán Assets = Liabilities + Equity; vốn chủ sở hữu là phần còn lại nếu bán hết tài sản và trả hết nợ; balance sheet đo tại **thời điểm**, income statement đo theo **thời kỳ**; lợi nhuận theo dồn tích liên quan nhưng không tương đương dòng tiền.

**Fast-Track Core (30–45 phút):** đọc mục 1–4 (20–25 phút), làm Mini-review 1–2 (5–10 phút), làm quiz 8 câu (8–10 phút), rồi đối chiếu answer key. Phần đào sâu gồm mục "Biết được gì / Chưa thể kết luận gì" và hoạt động tự nguyện.

## 2. Mục tiêu học tập đo được
- [LO-W04-01] Định nghĩa thu nhập trên mỗi cổ phần (EPS), hệ số giá–thu nhập (P/E), hệ số giá–giá trị sổ sách (P/B), tỷ suất sinh lời trên vốn chủ sở hữu (ROE), tỷ suất sinh lời trên tổng tài sản (ROA) và tỷ lệ nợ trên vốn chủ sở hữu (D/E) bằng cách nêu rõ tử số, mẫu số, đơn vị và kỳ tính của từng tỷ số, dùng thuật ngữ Việt–Anh đúng ngữ cảnh.
- [LO-W04-02] Tính EPS, P/E, P/B, ROE, ROA và D/E từ bộ số liệu giả định (VND) bằng cách dùng số dư bình quân (đầu kỳ + cuối kỳ)/2 cho ROE/ROA, số cổ phiếu phổ thông bình quân gia quyền cho EPS, ghi rõ đơn vị và thời điểm/thời kỳ của từng đầu vào, và nhận ra trường hợp mẫu số bằng 0 hoặc âm để ghi "không có ý nghĩa" thay vì tiếp tục so sánh.
- [LO-W04-03] Phân tích tình huống giả định bằng cách tách ROE theo phương pháp DuPont (biên lợi nhuận × vòng quay tài sản × hệ số nhân vốn chủ), kiểm tra vai trò đòn bẩy và chất lượng lợi nhuận (khoản thu bất thường, đỉnh chu kỳ), và giải thích vì sao P/E thấp hoặc ROE cao chưa tự động là "cơ hội".
- [LO-W04-04] Phản biện một kết luận chỉ dựa trên một tỷ số bằng cách nêu giới hạn: mức tỷ số "tốt" phụ thuộc ngành, dữ liệu lịch sử không tự động dự báo tương lai, EPS có thể tăng do mua lại cổ phiếu chứ không do kinh doanh, và các tỷ số với mẫu số âm/0 cần được đọc là "chưa thể kết luận".

## 3. Khái niệm cốt lõi
### 3.1. Tỷ số tài chính: phép chia hai đại lượng đã học ở W03
Mỗi tỷ số lấy **tử số** từ một báo cáo (ví dụ lợi nhuận sau thuế từ income statement) và **mẫu số** từ báo cáo khác hoặc cùng báo cáo (ví dụ vốn chủ sở hữu từ balance sheet). Khi ghi một tỷ số bắt buộc kèm ba thông tin:
- **Đơn vị:** EPS đo bằng VND/cổ phần; P/E, P/B và D/E đo bằng "lần"; ROE và ROA đo bằng %.
- **Kỳ tính:** tử số lợi nhuận đo theo **thời kỳ** (ví dụ 12 tháng 2026); mẫu số vốn/tài sản đo tại một **thời điểm**, hoặc dùng **số bình quân** của số dư đầu kỳ và cuối kỳ (OpenStax dùng trung bình cộng hai số dư cho tổng tài sản và vốn chủ). [W04-C05]
- **Nguồn tử số/mẫu số:** từng con số lấy từ báo cáo nào, kỳ nào.

OpenStax (mục 6.1) nhóm tỷ số thành **bốn loại chính: thanh khoản (liquidity), khả năng thanh toán nợ (solvency), hiệu quả (efficiency) và sinh lời (profitability)**, đồng thời nhấn mạnh: kết quả "hấp dẫn" của một tỷ số có thể thay đổi tùy ngành; kết quả quá khứ không tự động dự báo tương lai; dữ liệu kế toán chứa nhiều ước tính và có độ linh hoạt trong áp dụng chuẩn mực, nên hai công ty tuân thủ chuẩn mực vẫn có thể trình bày thông tin khác nhau. SEC cũng khẳng định: **"nói chung, mức tỷ số mong muốn thay đổi theo ngành"** ("desirable ratios vary by industry"). [W04-C01]

### 3.2. Thu nhập trên mỗi cổ phần (earnings per share — EPS)
**Thu nhập trên mỗi cổ phần** (earnings per share, viết tắt **EPS**) đo phần lợi nhuận của kỳ gắn với mỗi cổ phiếu phổ thông. SEC mô tả rút gọn: EPS trên income statement tính bằng **tổng lợi nhuận ròng chia số cổ phiếu đang lưu hành**. OpenStax (mục 6.5) cho công thức bản cơ bản (basic) chặt hơn:

`EPS cơ bản = (Lợi nhuận ròng − Cổ tức cổ phiếu ưu đãi) / Số cổ phiếu phổ thông bình quân gia quyền trong kỳ`

- **Tử số:** lợi nhuận ròng **trừ cổ tức cổ phiếu ưu đãi** — phần lợi nhuận còn dành cho cổ đông phổ thông. (Nếu công ty không có cổ phiếu ưu đãi, tử số bằng lợi nhuận ròng — hai cách mô tả của SEC và OpenStax trùng nhau.)
- **Mẫu số:** số cổ phiếu phổ thông **bình quân gia quyền** trong kỳ, vì số cổ phiếu thay đổi khi phát hành thêm hoặc mua lại. OpenStax minh họa: đầu kỳ 600 cổ phiếu, ngày 01/04 phát hành thêm 1.000 → bình quân = 600 × 3/12 + 1.600 × 9/12 = **1.350 cổ phiếu**; nếu không gia quyền theo thời gian, phép tính sai. [W04-C02]

**EPS pha loãng (diluted EPS):** OpenStax ghi rõ công ty đại chúng phải báo cáo **cả hai con số basic và diluted trên mặt income statement**; diluted tính thêm các công cụ (chứng khoán chuyển đổi, quyền chọn, trái phiếu chuyển đổi...) có khả năng làm **giảm (pha loãng) EPS cơ bản**. Chuẩn quốc tế **IAS 33** của IFRS Foundation quy định: công ty niêm yết phải trình bày basic và diluted **với tầm quan trọng ngang nhau**; chuẩn đặt ra nguyên tắc xác định **tử số ("earnings") và mẫu số (số cổ phiếu phổ thông bình quân gia quyền trong kỳ)** cho cả hai mức; "pha loãng" được định nghĩa là mức giảm tiềm năng của EPS (hoặc tăng tiềm năng của lỗ trên mỗi cổ phần) khi giả định công cụ chuyển đổi được chuyển đổi, quyền chọn/warrant được thực hiện, hoặc cổ phiếu được phát hành khi điều kiện thỏa mãn. [W04-C02]

### 3.3. Hệ số giá–thu nhập (price-to-earnings — P/E) và hệ số giá–giá trị sổ sách (price-to-book — P/B)
**Hệ số giá trên thu nhập** (price-to-earnings, viết tắt **P/E**) liên hệ giá thị trường với EPS:

`P/E = Giá mỗi cổ phiếu / EPS`

SEC minh họa: cổ phiếu giá 20 USD, EPS 2 USD → P/E = 10/1 — "cổ phiếu đang bán với 10 lần lợi nhuận". OpenStax (công thức 6.31) dùng cùng công thức với ví dụ 69,41/3,75 = **18,51 lần** — nhà đầu tư trả 18,51 đồng giá để đổi lấy 1 đồng lợi nhuận. OpenStax nhấn mạnh hai biến thể: **trailing** (thường gọi P/E TTM — dùng EPS **trượt 12 tháng** đã báo cáo) và **forward** (dùng EPS **dự phóng** tương lai). Lưu ý cấu trúc thời gian: tử số là giá **tại một thời điểm**, mẫu số là lợi nhuận **cả một kỳ** — phải ghi rõ ngày giá và kỳ EPS. [W04-C04]

**Hệ số giá trên giá trị sổ sách** (price-to-book, viết tắt **P/B**) liên hệ giá thị trường với giá trị sổ sách trên mỗi cổ phần:

`BVPS = (Tổng vốn chủ sở hữu − Vốn ưu đãi) / Tổng số cổ phiếu lưu hành`

`P/B = Giá mỗi cổ phiếu / BVPS`

OpenStax giải thích: book value là giá trị **"trên giấy" (on paper)** suy ra từ thực hành kế toán; giá trị thị trường do **cung–cầu** quyết định; hai con số này thường khác nhau và việc so sánh chúng chỉ là một cách đọc, không phải kết luận. [W04-C04]

### 3.4. Tỷ suất sinh lời trên vốn chủ sở hữu (return on equity — ROE) và trên tổng tài sản (return on assets — ROA)
**Tỷ suất sinh lời trên tổng tài sản** (return on assets, viết tắt **ROA**) đo hiệu quả dùng toàn bộ tài sản để tạo lợi nhuận:

`ROA = Lợi nhuận ròng / Tổng tài sản bình quân`, với `Tổng tài sản bình quân = (Tổng tài sản đầu kỳ + Tổng tài sản cuối kỳ) / 2`

**Tỷ suất sinh lời trên vốn chủ sở hữu** (return on equity, viết tắt **ROE**) đo hiệu quả dùng vốn của cổ đông:

`ROE = Lợi nhuận ròng (sau cổ tức ưu đãi) / Vốn chủ sở hữu bình quân`, với `Vốn chủ bình quân = (Vốn chủ đầu kỳ + Vốn chủ cuối kỳ) / 2`

OpenStax (mục 6.6) lưu ý tử số ROE lấy lợi nhuận **sau khi đã trả cổ tức ưu đãi** — cùng logic với tử số EPS ở 3.2. [W04-C05]

**Phương pháp DuPont** tách ROE thành ba thành phần để thấy động lực:

`ROE = Biên lợi nhuận (Net Income / Net Sales) × Vòng quay tổng tài sản (Net Sales / Total Assets) × Hệ số nhân vốn chủ (Average Total Assets / Average Stockholders' Equity)`

Thành phần thứ ba — **hệ số nhân vốn chủ (equity multiplier)** — chính là thước đo **đòn bẩy**: cùng một mức lợi nhuận, công ty dùng nhiều nợ sẽ có vốn chủ nhỏ hơn và ROE bị đẩy lên, dù hiệu quả kinh doanh không đổi. OpenStax nhắc: mức ROE/ROA chấp nhận được do **chuẩn ngành** quyết định và cần đọc trong bối cảnh tổ chức, lịch sử và ngành. [W04-C05]

### 3.5. Tỷ lệ nợ trên vốn chủ sở hữu (debt-to-equity — D/E)
**Tỷ lệ nợ trên vốn chủ sở hữu** (debt-to-equity, viết tắt **D/E**) đo đòn bẩy tài chính:

`D/E = Tổng nợ phải trả / Tổng vốn chủ sở hữu`

SEC diễn giải: D/E = 2/1 nghĩa là công ty có **2 đồng nợ cho mỗi 1 đồng cổ đông đầu tư**. OpenStax (công thức 6.23) dùng cùng công thức với ví dụ 150.000/100.000 = **1,5 lần (1,5:1)** — mỗi 1 đồng vốn chủ đi kèm 1,5 đồng nợ. Biến thể trên cùng dữ liệu là **hệ số nợ trên tài sản** `Debt-to-Assets = (Nợ ngắn hạn + Nợ dài hạn) / Tổng tài sản`; OpenStax ghi chú: tỷ số này **lớn hơn 1,0** nghĩa là nợ nhiều hơn tài sản — tức **vốn chủ âm**. [W04-C06]

### 3.6. Khi mẫu số bằng 0 hoặc âm: ghi "không có ý nghĩa", không ép so sánh
- **EPS âm (công ty lỗ):** OpenStax dẫn ví dụ Amazon 1997 với EPS **−1,27 USD/cổ phần** và nhấn mạnh EPS (kể cả âm) phản ánh **hoạt động quá khứ, không dự báo tương lai**. Khi EPS âm, P/E = giá/EPS là số âm và **không dùng để so sánh mức giá**; bảng dữ liệu Damodaran (01/2026) chính vì vậy ghi **NA** cho trailing P/E của những nhóm ngành phần lớn công ty đang lỗ (ví dụ Chemical (Diversified): 100% công ty lỗ, trailing P/E = NA). [W04-C03]
- **Vốn chủ bằng 0:** BVPS = 0, nên P/B là phép chia cho 0 — **không xác định**; ROE cũng không xác định.
- **Vốn chủ âm (nợ > tài sản):** theo OpenStax 6.4, debt-to-assets > 1,0 tương ứng vốn chủ âm; khi đó D/E và P/B ra số âm và **không thể diễn giải như trường hợp dương** — ghi "NA – vốn chủ âm" thay vì đưa ra con số.
- **Quy tắc chung:** trước khi chia, kiểm tra mẫu số; nếu mẫu số bằng 0/âm hoặc tử số lợi nhuận âm, ghi rõ "không có ý nghĩa để so sánh" và nêu dữ kiện còn thiếu. [W04-C03]

## 4. Ví dụ và worked examples (giả định, đơn vị VND)
### 4.1. Dữ liệu nền — dùng lại "Công ty M" của W03 (giả định)
**Giả định của người viết:** tiếp dùng số liệu hư cấu của W03 cho năm 2026 — đơn vị VND. Balance sheet tại 31/12/2026: tổng tài sản 10.000.000.000; tổng nợ phải trả 6.000.000.000; vốn chủ sở hữu 4.000.000.000. Income statement 12 tháng 01/01–31/12/2026: doanh thu thuần 20.000.000.000; lợi nhuận sau thuế 2.625.000.000. Giả định thêm: công ty **không có cổ phiếu ưu đãi** (nên tử số EPS = lợi nhuận sau thuế), và số cổ phiếu phổ thông **không đổi trong năm**, bình quân gia quyền = **10.000.000 cổ phiếu**.

`EPS cơ bản = 2.625.000.000 / 10.000.000 = 262,5 VND/cổ phần (kỳ 12 tháng 2026)`

Kiểm tra đơn vị: tử số VND, mẫu số cổ phiếu → kết quả VND/cổ phần. [W04-C02]

### 4.2. Số cổ phiếu bình quân gia quyền (giả định thay đổi dữ kiện)
**Dữ kiện giả định mới:** thay giả định 4.1, công ty phát hành thêm 2.000.000 cổ phiếu ngày **01/04/2026**. Số cổ phiếu: 10.000.000 trong 3 tháng đầu; 12.000.000 trong 9 tháng còn lại.

`Bình quân gia quyền = 10.000.000 × 3/12 + 12.000.000 × 9/12 = 2.500.000 + 9.000.000 = 11.500.000 cổ phiếu`

`EPS cơ bản = 2.625.000.000 / 11.500.000 = 228,26 VND/cổ phần (làm tròn 2 chữ số; không làm tròn là 228,260869...)`

So với 4.1, cùng lợi nhuận nhưng EPS thấp hơn vì mẫu số lớn hơn — đúng cơ chế bình quân gia quyền của OpenStax. [W04-C02]

### 4.3. EPS pha loãng (rút gọn, giả định)
**Dữ kiện giả định:** công ty có trái phiếu chuyển đổi có thể chuyển thành **1.500.000 cổ phiếu phổ thông**. Ví dụ rút gọn (chỉ minh họa thêm mẫu số):

`EPS pha loãng (rút gọn) ≈ 2.625.000.000 / (11.500.000 + 1.500.000) = 2.625.000.000 / 13.000.000 = 201,92 VND/cổ phần`

**Lưu ý phạm vi:** đây là minh họa rút gọn — chỉ cộng thêm cổ phiếu giả định vào mẫu số. Nguyên tắc đầy đủ của IAS 33 còn có các **điều chỉnh tử số** (ví dụ chi phí lãi vay của trái phiếu chuyển đổi) và các quy tắc loại trừ; trong phiên này chỉ trang mô tả About của IAS 33 được mở, nên bài học **không** trình bày cơ chế đầy đủ đó. Điểm cần nhớ: EPS pha loãng ≤ EPS cơ bản theo định nghĩa "pha loãng" của IAS 33. [W04-C02]

### 4.4. P/E (giả định)
**Dữ kiện giả định:** giá cổ phiếu Công ty M tại phiên 31/12/2026 là 5.250 VND/cổ phần (giả định, tại một ngày).

`P/E (trailing, TTM) = 5.250 / 262,5 = 20,0 lần`

Đọc kết quả: trả 20 đồng giá cho 1 đồng lợi nhuận của 12 tháng 2026 (cách diễn giải kiểu OpenStax: nhà đầu tư bỏ 18,51 để nhận 1 — ở đây là 20). Ghi chú cấu trúc: **giá tại một ngày** chia **EPS cả năm**; nếu đổi sang P/E forward với EPS dự phóng, kết quả khác — hai loại không trộn lẫn. [W04-C04]

### 4.5. P/B (giả định)
`BVPS = 4.000.000.000 / 10.000.000 = 400 VND/cổ phần (vốn chủ tại 31/12/2026; số cổ phiếu cuối kỳ 10.000.000 — giả định)`

`P/B = 5.250 / 400 = 13,125 lần`

Book value là giá trị "trên giấy" theo cơ sở ghi nhận kế toán (W03), không phải giá trị thanh lý hay thị trường; P/B cao không tự động nghĩa là "đắt" hay "rẻ". [W04-C04]

### 4.6. ROE (giả định)
**Giả định mới của người viết** (W03 chỉ cho số dư cuối kỳ): vốn chủ tại **01/01/2026** là 3.000.000.000 VND.

`Vốn chủ bình quân = (3.000.000.000 + 4.000.000.000) / 2 = 3.500.000.000 VND`

`ROE = 2.625.000.000 / 3.500.000.000 = 0,75 = 75,0% (kỳ 12 tháng 2026; tử số là lợi nhuận sau thuế, không có cổ tức ưu đãi)`

### 4.7. ROA (giả định)
**Giả định:** tổng tài sản tại 01/01/2026 cũng là 10.000.000.000 VND (không đổi trong năm).

`Tổng tài sản bình quân = (10.000.000.000 + 10.000.000.000) / 2 = 10.000.000.000 VND`

`ROA = 2.625.000.000 / 10.000.000.000 = 0,2625 = 26,25% (kỳ 12 tháng 2026)`

### 4.8. D/E và debt-to-assets (giả định)
`D/E = 6.000.000.000 / 4.000.000.000 = 1,5 lần (1,5:1) — mỗi 1 đồng vốn chủ đi kèm 1,5 đồng nợ phải trả`

`Debt-to-Assets = 6.000.000.000 / 10.000.000.000 = 0,6 = 60% — 60% tài sản được tài trợ bằng nợ`

Cả hai dùng số dư **tại 31/12/2026** (thời điểm), đúng mẫu số balance sheet của SEC/OpenStax. [W04-C06]

### 4.9. DuPont: ROE đến từ đâu (giả định)
Dùng số liệu 4.1–4.7 cho năm 2026:

| Thành phần | Phép tính | Kết quả |
|---|---|---:|
| Biên lợi nhuận (profit margin) | 2.625.000.000 / 20.000.000.000 | 13,125% |
| Vòng quay tổng tài sản (asset turnover) | 20.000.000.000 / 10.000.000.000 | 2,0 lần |
| Hệ số nhân vốn chủ (equity multiplier) | 10.000.000.000 / 3.500.000.000 | ≈ 2,857 lần |

`ROE ≈ 13,125% × 2,0 × 2,857 ≈ 75,0%` — khớp ROE tính trực tiếp ở 4.6.

Đọc kết quả: một phần ROE cao đến từ **đòn bẩy** (equity multiplier ~2,86), không chỉ từ hiệu quả kinh doanh. Đây là lý do OpenStax tách ROE theo DuPont trước khi đánh giá. [W04-C05]

### 4.10. EPS âm và vốn chủ bằng 0/âm (giả định hai công ty khác)
**Công ty N (giả định), kỳ 12 tháng 2026:** lỗ sau thuế 1.200.000.000 VND; cổ phiếu bình quân gia quyền 8.000.000; giá tại một ngày 5.000 VND/cổ phần.

- `EPS = −1.200.000.000 / 8.000.000 = −150 VND/cổ phần`
- `P/E = 5.000 / (−150) = −33,3 lần` → **không có ý nghĩa so sánh**; cách ghi đúng là "P/E: NA – EPS âm" (Damodaran ghi NA cho các ngành phần lớn công ty lỗ). [W04-C03]
- Nếu vốn chủ bình quân của N là 2.000.000.000 VND: `ROE = −1.200.000.000 / 2.000.000.000 = −60%` — dấu trừ cho biết **lỗ**; không xếp "tốt/xấu" theo độ lớn số âm.

**Công ty P (giả định), tại 31/12/2026:** tổng tài sản 5.000.000.000 VND; tổng nợ phải trả 5.000.000.000 VND → vốn chủ = **0** → BVPS = 0 → **P/B không xác định** (chia cho 0); ROE cũng không xác định. Nếu tổng nợ là 6.000.000.000 VND → vốn chủ **−1.000.000.000 VND** (vốn chủ âm): `Debt-to-Assets = 6.000.000.000 / 5.000.000.000 = 1,2 > 1,0` — đúng cảnh báo của OpenStax 6.4; `D/E = 6.000.000.000 / (−1.000.000.000) = −6,0` — con số này **không thể** diễn giải như đòn bẩy bình thường. Ghi: "D/E: NA – vốn chủ âm". [W04-C03]

### 4.11. Phân loại bằng chứng
- **Kết quả nghiên cứu/nguồn:** SEC mô tả EPS trên income statement, P/E = giá/EPS (ví dụ 20/2 = 10 lần), D/E = tổng nợ/vốn chủ (ví dụ 2:1) và "mức tỷ số mong muốn thay đổi theo ngành"; OpenStax 6.5 cho công thức EPS cơ bản với tử số trừ cổ tức ưu đãi và mẫu số bình quân gia quyền, phân biệt basic/diluted, P/E TTM/forward, BVPS; OpenStax 6.6 cho ROA/ROE với mẫu số bình quân và DuPont; OpenStax 6.4 cho D/E, debt-to-assets và lưu ý vốn chủ âm; OpenStax 6.1 nêu bốn nhóm tỷ số và giới hạn phân tích; IAS 33 (trang About của IFRS Foundation) quy định basic + diluted song song, tử số/mẫu số và định nghĩa pha loãng; Damodaran (01/2026) cho dữ liệu P/E theo ngành và cách ghi NA.
- **Giả định minh họa:** toàn bộ số liệu "Công ty M/N/P"; giá 5.250 và 5.000 VND/cổ phần; số cổ phiếu 10/12/11,5/13 và 8 triệu; vốn chủ đầu kỳ 3 tỷ; tổng tài sản đầu kỳ 10 tỷ; lỗ 1,2 tỷ; vốn chủ bình quân 2 tỷ của N; bảng cân đối của P; việc không có cổ phiếu ưu đãi.
- **Diễn giải thực hành:** các ví dụ dùng để luyện tính toán và nhận diện trường hợp "không có ý nghĩa"; không phải phân tích doanh nghiệp thật và không phải khuyến nghị giao dịch.

## 5. Mini-review và truy xuất kiến thức
### Mini-review 1: tính EPS và P/E, ghi rõ đơn vị và kỳ
Không nhìn phần trên: Công ty X (giả định) có lợi nhuận sau thuế **800.000.000 VND cho 12 tháng 2026**, không có cổ phiếu ưu đãi, số cổ phiếu phổ thông bình quân gia quyền **4.000.000**, giá cổ phiếu tại một ngày là **1.600 VND/cổ phần**. Tính EPS và P/E, nêu rõ đơn vị và kỳ của từng đầu vào. **Tự kiểm:** EPS = 800.000.000 / 4.000.000 = **200 VND/cổ phần** (kỳ 12 tháng 2026); P/E = 1.600 / 200 = **8,0 lần** (giá tại một ngày chia EPS của 12 tháng — hai loại thời gian khác nhau, phải ghi rõ).

### Mini-review 2: ROE cao nhưng liệu có "hiệu quả"?
Công ty Y (giả định) có ROE 40% và D/E 2,5; Công ty Z cùng ngành, cùng kỳ có ROE 15% và D/E 0,5. Viết ba câu hỏi kiểm tra trước khi kết luận Y "hiệu quả hơn" Z. **Tự kiểm:** (1) tách ROE theo DuPont — ROE của Y có được đẩy bởi hệ số nhân vốn chủ (đòn bẩy) thay vì biên lợi nhuận hoặc vòng quay tài sản? (2) so sánh đã cùng ngành và cùng kỳ chưa — mức chấp nhận được do chuẩn ngành quyết định (SEC/OpenStax); (3) dữ liệu là lịch sử, không tự động dự báo tương lai (OpenStax 6.1), và còn thiếu dữ kiện nào (dòng tiền, khoản thu bất thường, EPS một lần)? Kết luận đúng: **chưa thể kết luận** "Y tốt hơn" chỉ từ hai tỷ số.

## 6. Cảnh báo & sai lầm thường gặp
- Dùng **một tỷ số** để ra quyết định mua/bán: tỷ số là phép chia hai con số kế toán — điểm bắt đầu của câu hỏi, không phải kết luận.
- So sánh khác ngành: mức tỷ số mong muốn **thay đổi theo ngành** (SEC, OpenStax 6.1); P/E của ngân hàng và P/E của công ty phần mềm không đối chiếu trực tiếp được.
- Quên **kỳ tính và đơn vị**: giá là số tại một thời điểm, lợi nhuận là số theo kỳ; P/E TTM (trượt 12 tháng) khác P/E forward (dự phóng) — ghi rõ đang dùng loại nào.
- Nhầm EPS tăng do **mua lại cổ phiếu** với hiệu quả kinh doanh tăng: OpenStax minh họa EPS tăng chỉ vì số cổ phiếu bình quân giảm.
- Nhầm ROE cao thành "quản lý giỏi": đòn bẩy (equity multiplier trong DuPont) có thể đẩy ROE lên mà hiệu quả cốt lõi không đổi.
- Tiếp tục so sánh khi **mẫu số bằng 0/âm** hoặc EPS âm: P/E, P/B, ROE, D/E mất ý nghĩa so sánh — ghi NA và nêu dữ kiện còn thiếu.
- Coi **book value** là giá trị thị trường hoặc thanh lý: book value là giá trị "trên giấy" theo thực hành kế toán (OpenStax).
- Thiên kiến xác nhận (chỉ tìm dữ kiện ủng hộ), anchoring (bám vào một con số P/E cũ) và overconfidence (nhầm biết công thức với khả năng dự đoán tương lai).

## 7. Biết được gì / Chưa thể kết luận gì
**Biết được:** định nghĩa và công thức tử số/mẫu số của EPS, P/E, P/B, ROE, ROA, D/E kèm đơn vị và kỳ tính; vì sao mẫu số EPS là số cổ phiếu **bình quân gia quyền**; phân biệt EPS cơ bản và pha loãng (theo OpenStax và trang About của IAS 33); cách lấy số bình quân (đầu kỳ + cuối kỳ)/2 cho ROE/ROA; tách ROE theo DuPont để thấy vai trò đòn bẩy; cách diễn giải D/E 1,5:1; xử lý EPS âm, vốn chủ 0/âm thành "không có ý nghĩa"; giới hạn phân tích tỷ số (ngành, lịch sử, ước tính kế toán).

**Chưa thể kết luận:** cổ phiếu "rẻ hay đắt" chỉ từ P/E hoặc P/B; hiệu quả quản trị chỉ từ ROE/ROA; mức tỷ số phù hợp cho thị trường Việt Nam hoặc ngành cụ thể tại Việt Nam (các nguồn đã mở là của Hoa Kỳ/quốc tế; đặc thù chuẩn mực kế toán Việt Nam chưa được xác minh trong phiên này); giá trị hợp lý, dự báo lợi nhuận hoặc giá cổ phiếu — bài học không tạo ra dự báo nào. Với mẫu số bằng 0/âm, kết luận đúng là "chưa đủ dữ kiện để so sánh".

## 8. Checklist thực hành
- [ ] Viết mục tiêu, thời hạn và giới hạn rủi ro.
- [ ] Ghi nguồn, thời điểm, đơn vị và trạng thái xác minh.
- [ ] Tách dữ kiện, giả định, phép tính và kết luận.
- [ ] Kiểm tra kịch bản xấu, thanh khoản, phí và thuế.
- [ ] Không dùng tiền thật chỉ vì hoàn thành bài.

## 9. Hoạt động áp dụng tự nguyện (giả định)
Dùng **chỉ số liệu giả định**: tự lập một bộ số liệu mini cho một "công ty tưởng tượng" — balance sheet đầu kỳ và cuối kỳ, income statement 12 tháng, số cổ phiếu đầu kỳ cùng một lần phát hành giữa kỳ. Sau đó: (1) tính đủ 6 tỷ số EPS, P/E (tự đặt giá một ngày), P/B, ROE, ROA, D/E, ghi rõ đơn vị, thời điểm/thời kỳ của từng đầu vào; (2) cố tình chuyển công ty sang trạng thái lỗ và viết ghi chú vì sao P/E chuyển thành "NA"; (3) tăng nợ vay nhưng giữ nguyên lợi nhuận, quan sát D/E, hệ số nhân vốn chủ và ROE đổi hướng thế nào; (4) viết ba câu "chưa thể kết luận" từ bộ tỷ số của chính mình. Không nhập số liệu doanh nghiệp thật, tài khoản cá nhân hoặc số dư thực.

## 10. Glossary
| Việt | English | Định nghĩa |
|---|---|---|
| Thu nhập trên mỗi cổ phần | Earnings per Share (EPS) | Phần lợi nhuận của kỳ gắn với mỗi cổ phiếu phổ thông; bản cơ bản lấy lợi nhuận ròng trừ cổ tức ưu đãi, chia số cổ phiếu phổ thông bình quân gia quyền (OpenStax 6.27; SEC mô tả rút gọn bằng tổng lợi nhuận chia số cổ phiếu đang lưu hành). |
| Thu nhập trên mỗi cổ phần pha loãng | Diluted EPS | EPS tính theo giả định các công cụ có thể chuyển đổi/quyền chọn được thực hiện, có khả năng làm giảm EPS cơ bản hoặc tăng lỗ trên mỗi cổ phần (OpenStax 6.5; IAS 33). |
| Số cổ phiếu bình quân gia quyền | Weighted Average Shares Outstanding | Số cổ phiếu phổ thông bình quân theo thời gian lưu hành trong kỳ; mẫu số của EPS cơ bản và diluted. |
| Hệ số giá trên thu nhập | Price-to-Earnings (P/E) | Giá mỗi cổ phiếu chia EPS; cho biết nhà đầu tư trả bao nhiêu đồng giá cho 1 đồng lợi nhuận; đo bằng "lần". |
| P/E trượt | Trailing P/E (TTM) | P/E dùng EPS của 12 tháng đã báo cáo gần nhất. |
| P/E dự phóng | Forward P/E | P/E dùng EPS dự phóng cho tương lai (OpenStax 6.5 nêu cùng lúc hai đặc tính trailing và forward). |
| Hệ số giá trên giá trị sổ sách | Price-to-Book (P/B) | Giá mỗi cổ phiếu chia giá trị sổ sách trên mỗi cổ phần (BVPS). |
| Giá trị sổ sách trên mỗi cổ phần | Book Value per Share (BVPS) | (Tổng vốn chủ − vốn ưu đãi) chia tổng số cổ phiếu lưu hành; giá trị "trên giấy" theo thực hành kế toán (OpenStax 6.33). |
| Tỷ suất sinh lời trên vốn chủ sở hữu | Return on Equity (ROE) | Lợi nhuận ròng (sau cổ tức ưu đãi) chia vốn chủ sở hữu bình quân. |
| Tỷ suất sinh lời trên tổng tài sản | Return on Assets (ROA) | Lợi nhuận ròng chia tổng tài sản bình quân — trung bình cộng số dư đầu kỳ và cuối kỳ. |
| Tỷ lệ nợ trên vốn chủ sở hữu | Debt-to-Equity (D/E) | Tổng nợ phải trả chia tổng vốn chủ sở hữu; đo mức đòn bẩy tài chính; 1,5:1 nghĩa là mỗi 1 đồng vốn chủ đi kèm 1,5 đồng nợ. |
| Hệ số nợ trên tài sản | Debt-to-Assets | (Nợ ngắn hạn + nợ dài hạn) chia tổng tài sản; lớn hơn 1,0 nghĩa là nợ nhiều hơn tài sản, tức vốn chủ âm (OpenStax 6.21–6.22). |
| Hệ số nhân vốn chủ | Equity Multiplier | Tổng tài sản bình quân chia vốn chủ bình quân; thành phần đòn bẩy trong phương pháp DuPont (OpenStax 6.48). |
| Phương pháp DuPont | DuPont Method | Tách ROE = biên lợi nhuận × vòng quay tổng tài sản × hệ số nhân vốn chủ (OpenStax 6.45), giúp thấy động lực của ROE. |
| Số dư bình quân | Average Balance | Trung bình cộng số dư đầu kỳ và cuối kỳ, dùng làm mẫu số cho ROE và ROA. |
| Không có ý nghĩa | Not Meaningful (NA) | Cách ghi khi EPS âm hoặc mẫu số bằng 0/âm khiến tỷ số mất ý nghĩa so sánh (Damodaran ghi NA cho các ngành phần lớn công ty lỗ). |

## 11. Tài liệu tham khảo đã mở
- SEC (U.S. Securities and Exchange Commission), "Beginners' Guide to Financial Statement" — các phần "Earnings Per Share or EPS" và "Financial Statement Ratios and Calculations": https://www.sec.gov/reportspubs/investor-publications/investorpubsbegfinstmtguidehtm.html (truy cập 2026-09-15).
- OpenStax, *Principles of Finance 2e*, mục 6.1 "Ratios: Condensing Information into Smaller Pieces": https://openstax.org/books/principles-finance-2e/pages/6-1-ratios-condensing-information-into-smaller-pieces (truy cập 2026-09-15).
- OpenStax, *Principles of Finance 2e*, mục 6.4 "Solvency Ratios": https://openstax.org/books/principles-finance-2e/pages/6-4-solvency-ratios (truy cập 2026-09-15).
- OpenStax, *Principles of Finance 2e*, mục 6.5 "Market Value Ratios": https://openstax.org/books/principles-finance-2e/pages/6-5-market-value-ratios (truy cập 2026-09-15).
- OpenStax, *Principles of Finance 2e*, mục 6.6 "Profitability Ratios and the DuPont Method": https://openstax.org/books/principles-finance-2e/pages/6-6-profitability-ratios-and-the-dupont-method (truy cập 2026-09-15).
- IFRS Foundation, trang About của IAS 33 *Earnings per Share*: https://www.ifrs.org/issued-standards/list-of-standards/ias-33-earnings-per-share/ (truy cập 2026-09-15).
- Aswath Damodaran (NYU Stern), "PE Ratio by Sector (US)", dữ liệu tháng 01/2026: https://pages.stern.nyu.edu/~adamodar/New_Home_Page/datafile/pedata.html (truy cập 2026-09-15).
- Source ledger: [quality/module-1-source-ledger.md](../../quality/module-1-source-ledger.md).

## 12. Next steps
- [Quiz W04](../../assessments/quizzes/week-04-quiz.md)
- [Answer key W04](../../assessments/answer-keys/week-04-answers.md)
- Sau khi nắm W04, W05 (phân tích cơ bản top-down/bottom-up) sẽ dùng các tỷ số này như một lớp đầu vào của quy trình phân tích, không phải kết luận cuối cùng.
## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

**Bài:** Tỷ số tài chính. **Đầu ra:** Làm lại EPS/P-E/ROE của Công ty M và trường hợp mẫu số âm.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| EPS | Dùng lợi nhuận thuộc cổ đông thường và số cổ phiếu bình quân đúng kỳ | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| P/E/P/B | Ghi đơn vị, giá và mẫu số, không so tỷ số khác kỳ | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| ROE/ROA | Nêu quy ước số dư bình quân theo ví dụ | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Nợ/đòn bẩy | Tách D/E và debt-to-assets, giải thích ROE cao có thể đi với nợ | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Mẫu số | Ghi không có ý nghĩa khi mẫu số0/âm theo bài; không kết luận mua/bán | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
