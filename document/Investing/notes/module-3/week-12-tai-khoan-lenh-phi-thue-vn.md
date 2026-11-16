> [!IMPORTANT]
> **Tuyên bố phạm vi và giới hạn:** Bài học chỉ phục vụ giáo dục, không phải lời khuyên đầu tư, thuế hay pháp lý; không khuyến nghị công ty chứng khoán, mã chứng khoán, loại lệnh hoặc mức vốn nào. Quy định, lịch giao dịch, biểu phí và thuế có thể thay đổi. Trước khi đặt lệnh, hãy đọc quy chế hiện hành của sở giao dịch, văn bản thuế và biểu phí của chính công ty chứng khoán tại ngày thực hiện.

# W12 — Tài khoản, lệnh, phí và thuế tại Việt Nam

## 1. Ý Nghĩa Thực Tiễn (Why It Matters)
W11 giúp bạn đặt giao dịch vào đúng hạ tầng HOSE/HNX/UPCoM và hiểu T+2. W12 chuyển từ “thị trường vận hành thế nào” sang “một lệnh của tôi đi qua hệ thống ra sao, chi phí ròng là gì, và tiền nhận về có thể khác giá trị khớp thế nào?”. Người mới thường nhìn thấy nút Mua/Bán nhưng bỏ qua bốn điểm: tài khoản phải được mở và bảo vệ đúng; loại lệnh quyết định mức kiểm soát giá; phí hiện rõ và chi phí ẩn cùng bào mòn kết quả; thuế bán và thuế cổ tức không phụ thuộc vào cảm giác lời/lỗ.

**Kiến thức tiên quyết:** W11 — [Tổng quan TTCK Việt Nam](week-11-thi-truong-viet-nam.md), đặc biệt phân biệt sàn với VSDC, giá tham chiếu và T+2.

## 2. Mục Tiêu Học Tập Đo Lường Được (Measurable Learning Objectives)
Sau khi hoàn thành bài, người học có thể:
- **`[LO-W12-01]` (Understand):** phân biệt tài khoản giao dịch, tài khoản lưu ký, LO, ATO, ATC và lệnh thị trường/MTL; mô tả điều kiện hiệu lực và rủi ro giá của từng loại.
- **`[LO-W12-02]` (Apply):** lập bảng chi phí một giao dịch giả định bằng VND, tách phí môi giới, thuế bán, spread, slippage và chi phí tùy chọn; ghi rõ cơ sở, kỳ tính và giả định.
- **`[LO-W12-03]` (Analyze):** phân biệt và tính đúng trong phạm vi đã kiểm tra cho cá nhân cư trú: chuyển nhượng chứng khoán thông thường, cổ tức tiền mặt và cổ tức bằng cổ phiếu; ghi ngày hiệu lực, loại thu nhập và giới hạn áp dụng. Giữ ví dụ TT111/2013 và TT92/2015 ở trạng thái lịch sử/minh họa.
- **`[LO-W12-04]` (Evaluate):** áp dụng checklist trước lệnh gồm mục tiêu, thanh khoản, loại lệnh, chi phí, thuế, bảo mật và bằng chứng; phản biện tuyên bố “zero-fee” hoặc “lệnh thị trường luôn có giá tốt nhất”.

## 3. Tóm Lược Cốt Lõi và Fast Track (30–45 Phút)
### 3.1. Bản đồ khái niệm nhanh
Mở tài khoản tạo quyền truy cập, không tạo lợi nhuận bảo đảm. LO đặt giới hạn giá: mua ở giá chỉ định hoặc thấp hơn, bán ở giá chỉ định hoặc cao hơn. ATO tham gia khớp định kỳ mở cửa; ATC tham gia khớp định kỳ đóng cửa. Lệnh thị trường ưu tiên khớp với giá đối ứng tốt nhất đang có; trên HOSE tài liệu order types dùng MTL cho lệnh thị trường, phần còn lại có thể chuyển thành LO theo quy tắc. ATO/ATC chưa khớp hết sẽ bị hủy sau phiên tương ứng `[W12-C01]`.

Chi phí toàn phần có thể viết thành: **chi phí ròng = phí dịch vụ + thuế + spread + slippage + chi phí vốn/chi phí cơ hội (nếu có)**. Thông tư 128/2018/TT-BTC quy định mức giá tối đa/khung cho dịch vụ; công ty chứng khoán quyết định mức cụ thể trong khung và phải công bố biểu phí của mình `[W12-C02]`. Vì vậy “phí môi giới 0%” không đồng nghĩa giao dịch không có chi phí.

Trong **khung lịch sử/minh họa** của bài, TT111/2013 và TT92/2015 được dùng để minh họa **0,1% trên giá chuyển nhượng từng lần bán** và **5% trên cổ tức tiền mặt**; phần này vẫn đóng nhãn lịch sử và không thay thế quy tắc hiện hành `[W12-C03]`. Quy tắc hiện hành được tách riêng ở mục 4.4 dưới đây.

### 3.2. Bảng so sánh nhanh
| Thành phần | Cơ chế | Câu hỏi kiểm tra trước lệnh |
|---|---|---|
| Tài khoản/eKYC | Xác lập danh tính, hợp đồng và quyền sử dụng dịch vụ | Đơn vị được cấp phép? điều khoản và kênh liên hệ chính thức? |
| LO | Kiểm soát giá, có thể không khớp | Giá tối đa/tối thiểu chấp nhận là gì? |
| ATO/ATC | Khớp định kỳ mở/đóng, không cam kết giá trước | Phần chưa khớp sẽ xử lý ra sao? |
| Lệnh thị trường/MTL | Ưu tiên tốc độ, có thể ăn nhiều mức giá | Sổ lệnh có đủ sâu và spread có hẹp không? |
| Phí | Phí công bố theo nhà cung cấp và dịch vụ | Biểu phí hiện hành, mức tối thiểu, phí ứng trước/margin? |
| Thuế | Khoản khấu trừ theo loại thu nhập | Cơ sở tính là giá bán hay cổ tức? |

### 3.3. Năm điểm rút ra
1. Tài khoản là hạ tầng truy cập; không phải bảo hiểm vốn.
2. LO ưu tiên kiểm soát giá; lệnh thị trường ưu tiên khả năng khớp.
3. ATO/ATC là lệnh theo phiên định kỳ, không phải giá mở/đóng được bảo đảm trước.
4. Mức phí cụ thể là của nhà cung cấp; trong ví dụ lịch sử, thuế bán 0,1% được đặt trên giá chuyển nhượng, nhưng không được coi đó là mức hiện hành năm 2026.
5. Một lệnh chỉ nên được đặt sau khi đã kiểm tra thanh khoản, chi phí, thuế, bảo mật và kịch bản khớp một phần.

## 4. Mổ Xẻ Chuyên Sâu (Deep Dive: 2–3 Giờ)
### 4.1. Tài khoản, eKYC và quyền kiểm soát
Quy trình eKYC thường thu thập thông tin nhận dạng và tạo hợp đồng dịch vụ điện tử; chi tiết phụ thuộc tổ chức cung cấp. Người học phải xác minh tên pháp nhân, kênh ứng dụng/website, điều khoản ủy quyền, tài khoản ngân hàng liên kết, cơ chế rút tiền và thông báo giao dịch. Không gửi mật khẩu, mã OTP hoặc Smart OTP cho môi giới. 2FA giảm rủi ro chiếm quyền nhưng không loại bỏ lừa đảo, thiết bị nhiễm mã độc hoặc lệnh đặt nhầm.

### 4.2. Lệnh và sổ lệnh
Theo tài liệu loại lệnh HOSE đã đối chiếu, LO mua/bán tại giá chỉ định hoặc tốt hơn; ATO dùng trong khớp định kỳ mở cửa và phần chưa khớp tự động hủy; ATC dùng trong khớp định kỳ đóng cửa và phần chưa khớp hết hạn; MTL mua tại giá bán thấp nhất hoặc bán tại giá mua cao nhất hiện có, phần còn lại chuyển thành LO theo quy tắc `[W12-C01]`. HNX có các tên MTL, MOK và MAK theo quy chế tương ứng; không nên suy ra thời gian và hành vi của mọi lệnh từ một sàn sang sàn khác.

Lệnh thị trường trên sổ mỏng có thể khớp nhiều mức giá. “Giá tốt nhất đang có” chỉ là mức đầu tiên trong sổ tại thời điểm hệ thống xử lý, không phải giá tốt nhất cả ngày. LO đổi lại quyền kiểm soát giá bằng nguy cơ không khớp hoặc khớp một phần.

### 4.3. Phí, thuế và chi phí ẩn
Thông tư 128/2018/TT-BTC có hiệu lực 15/02/2019 và nêu mức giá dịch vụ môi giới cổ phiếu/chứng chỉ quỹ/chứng quyền tối đa 0,5% giá trị giao dịch; công ty chứng khoán quyết định mức cụ thể `[W12-C02]`. Con số tối đa này không phải mức mọi khách hàng đều phải trả. Ngoài phí môi giới, cần đọc phí lưu ký, ứng trước tiền bán, margin, chuyển khoản, dữ liệu, phí nền tảng và mức tối thiểu nếu có.

Thuế chuyển nhượng và thuế cổ tức là lớp khác với phí. **Khung dưới đây chỉ là lịch sử/minh họa**, dựa trên TT111/2013/TT-BTC và TT92/2015/TT-BTC; các mức 0,1% và 5% **không được đánh dấu là hiện hành hoặc đã xác minh cho năm 2026** `[W12-C03]`. Muốn áp dụng thực tế phải đọc đúng điều khoản thuế và điều khoản chuyển tiếp đang có hiệu lực tại ngày giao dịch. Các nguồn hiện hành đã được đọc trong phạm vi cần cho bài: Luật 109/2025/QH15, Luật 09/2026/QH16 và 112/VBHN-VPQH (ban hành 20/05/2026) làm khung Luật Thuế TNCN; Nghị định 253/2026/NĐ-CP (ban hành 30/06/2026, hiệu lực 01/07/2026) có Điều 52 và 54; Thông tư 87/2026/TT-BTC (ban hành 30/06/2026, hiệu lực 01/07/2026) hướng dẫn thi hành. Các quy tắc dưới đây chỉ áp dụng cho phạm vi cá nhân cư trú và loại thu nhập được nêu, không suy rộng cho ESOP, chứng khoán phái sinh, sản phẩm hay tình trạng cư trú khác `[W12-C05][W12-C06][W12-C07][W12-C08]`.

## 5. Ví Dụ Giải Từng Bước (Worked Example)

### 4.4. Current 2026 resident-individual tax scope

Phần này cập nhật **đúng phạm vi cá nhân cư trú** theo các điều khoản đã đọc của Nghị định 253/2026/NĐ-CP và Thông tư 87/2026/TT-BTC, áp dụng từ **01/07/2026** cho các quy tắc được nêu. Đây là bài tập đọc nguồn, không phải tư vấn hồ sơ cá nhân.

- **Cổ tức/lợi tức đầu tư vốn bằng tiền:** Điều 52 Nghị định 253/2026/NĐ-CP quy định thuế suất **5%** trong phạm vi khoản chi trả bằng tiền. Ví dụ giả định nhận 10.000.000 VND tiền mặt: khoản thuế minh họa theo điều khoản này là 500.000 VND, trước khi xem xét chứng từ, thời điểm và điều kiện hồ sơ thực tế.
- **Chuyển nhượng chứng khoán thông thường:** Điều 54 Nghị định 253/2026/NĐ-CP quy định **0,1% trên giá chuyển nhượng từng lần** trong phạm vi chứng khoán thông thường của cá nhân cư trú. Ví dụ giả định giá chuyển nhượng 20.000.000 VND: khoản thuế theo công thức nguồn là 20.000 VND; đây không phải thuế tính trên lãi/lỗ theo giá vốn.
- **Cổ tức bằng cổ phiếu/cổ phiếu phát hành từ vốn chủ sở hữu:** tại thời điểm nhận, chưa nộp thuế thu nhập đầu tư vốn; khi chuyển nhượng, khoản thu nhập đầu tư vốn chưa nộp và thu nhập chuyển nhượng chứng khoán phải được xử lý theo các khoản tương ứng của Điều 52 và Điều 54. Bài không mở rộng quy tắc này sang ESOP, quyền mua, phái sinh hoặc sản phẩm khác.

**Không dùng lẫn ba trường hợp:** tiền mặt là khoản chi trả đầu tư vốn; bán chứng khoán thông thường là chuyển nhượng; cổ tức bằng cổ phiếu có thời điểm xử lý khác. Luôn ghi ngày hiệu lực, tư cách cá nhân cư trú, loại chứng khoán, loại phân phối và điều khoản nguồn trước khi tính.

**Nguồn và giới hạn:** [Nghị định 253/2026/NĐ-CP](https://datafiles.chinhphu.vn/cpp/files/vbpq/2026/7/253m-ndcp.signed.pdf), Điều 52 và 54, ban hành 30/06/2026, hiệu lực 01/07/2026; [Thông tư 87/2026/TT-BTC](https://vanban.chinhphu.vn/?docid=218772&pageid=27160&typegroupid=6), ban hành 30/06/2026, hiệu lực 01/07/2026; [112/VBHN-VPQH](https://vanban.chinhphu.vn/?docid=218215&pageid=27160), ban hành 20/05/2026. Các nguồn này không xác nhận nghĩa vụ của mọi nhà đầu tư, không bao phủ hồ sơ ngoại lệ và không thay thế việc kiểm tra văn bản tại ngày thực hiện.

### Ví dụ: một lệnh bán giả định, tính bằng VND
**Giả định minh họa lịch sử, không phải quy tắc hiện hành năm 2026, báo giá hay biểu phí thực tế:** nhà đầu tư bán 1.000 cổ phiếu giả định. Giá khớp bình quân là 20.000 VND/cổ phiếu, tổng giá trị bán 20.000.000 VND. Công ty chứng khoán thu phí môi giới giả định 0,15%; **giả định không có minimum/fixed fee, margin, phí lưu ký hoặc phí ứng trước**. Theo khung lịch sử của bài, dùng mức 0,1% trên giá trị bán. Không tính giá vốn để xác định thuế bán. Trước khi áp dụng, phải kiểm tra luật 2026 và điều khoản chuyển tiếp.

| Khoản | Công thức | VND |
|---|---:|---:|
| Giá trị bán | 1.000 × 20.000 | 20.000.000 |
| Phí môi giới giả định | 20.000.000 × 0,15% | 30.000 |
| Thuế chuyển nhượng | 20.000.000 × 0,1% | 20.000 |
| Tiền ròng trước khoản khác | 20.000.000 − 30.000 − 20.000 | **19.950.000** |

**Sensitivity/check minimum fee:** ví dụ trên giả định không có minimum/fixed fee. Nếu biểu phí hiện hành có minimum fee 50.000 VND, phí phải trả là `max(30.000, 50.000) = 50.000 VND`; tiền ròng theo **khung lịch sử minh họa** là `20.000.000 − 50.000 − 20.000 = 19.930.000 VND`. Đây chỉ là kiểm tra độ nhạy, không xác nhận minimum fee của nhà cung cấp hay mức thuế hiện hành.

Nếu nhà đầu tư bán với giá vốn 21.000 VND/cổ phiếu, lỗ kinh tế trước các khoản khác là `(20.000 − 21.000) × 1.000 = −1.000.000 VND`; trong **ví dụ lịch sử** khoản tính theo giá chuyển nhượng vẫn là 20.000 VND. Nếu mua/bán qua spread hoặc lệnh thị trường làm giá thực tế kém hơn 20.000 VND, chi phí ròng tăng thêm; nếu biểu phí thật khác 0,15% hoặc có minimum fee, phải thay số giả định. Nếu dùng cho giao dịch 2026, dừng phép tính và đọc văn bản thuế cùng chuyển tiếp hiện hành.

**Kiểm tra độ nhạy:** phí 0,30% thay vì 0,15% làm phí tăng từ 30.000 lên 60.000 VND; giá khớp bình quân 19.800 thay vì 20.000 làm giá trị bán giảm 200.000 VND trước khi tính phí và khoản thuế lịch sử minh họa. Bài học là tách giá khớp, phí, thuế và chi phí ẩn thay vì gộp thành “lãi/lỗ”.

## 6. Quy Trình Thực Hành Trước Khi Đặt Lệnh
1. Xác nhận mục tiêu, thời hạn, số tiền tối đa và điều kiện không giao dịch.
2. Kiểm tra đúng tài khoản, mã, sàn, sức mua/sức bán và trạng thái thanh toán T+2 của W11.
3. Chọn LO/ATO/ATC/MTL dựa trên mục tiêu kiểm soát giá hay tốc độ; ghi giá tối đa/tối thiểu nếu dùng LO.
4. Mở biểu phí hiện hành: phí giao dịch, mức tối thiểu, margin, ứng trước, lưu ký và chuyển tiền.
5. Ước lượng thuế và spread; với lệnh lớn so với thanh khoản, mô phỏng khớp từng mức giá.
6. Kiểm tra lại màn hình xác nhận, không chia sẻ mật khẩu/OTP, lưu xác nhận và đối chiếu sao kê.

## 7. Điều Đã Sáng Tỏ và Phần Còn Bất Định
### 7.1. Đã xác minh
- HOSE công bố LO, ATO, ATC và MTL cùng cơ chế hiệu lực/hủy/chuyển phần còn lại `[W12-C01]`.
- Thông tư 128/2018/TT-BTC là văn bản của Bộ Tài chính về giá dịch vụ chứng khoán, hiệu lực từ 15/02/2019; mức cụ thể do tổ chức cung cấp dịch vụ quyết định trong khung `[W12-C02]`.
- TT111/2013/TT-BTC và TT92/2015/TT-BTC được dùng như khung **lịch sử/minh họa**, không phải xác nhận mức thuế hiện hành năm 2026 `[W12-C03]`.
- Các nguồn hiện hành W12-C05–C08 đã được đọc trong phạm vi cá nhân cư trú, cổ tức tiền mặt/cổ phiếu và chuyển nhượng chứng khoán thông thường; các trường hợp ngoài phạm vi này vẫn pending và phải kiểm tra văn bản tương ứng.
- Các phép tính trong Ví dụ 1 là phép tính minh họa có thể kiểm tra độc lập `[W12-C04]`.

### 7.2. Còn bất định hoặc cần tra lại
- Giờ giao dịch, tên lệnh và chi tiết sửa/hủy có thể thay đổi theo sàn, hệ thống và quy chế hiện hành; không dùng giờ trong tài liệu cũ như dữ kiện vĩnh viễn.
- Mức phí thực trả phụ thuộc công ty chứng khoán, gói dịch vụ, khách hàng và thời điểm; Thông tư 128 không phải biểu phí cá nhân của mọi công ty.
- Thuế cần phân loại tình trạng cư trú, loại chứng khoán, loại thu nhập, văn bản đang có hiệu lực và điều khoản chuyển tiếp; bài học không xử lý mọi ngoại lệ và không xác nhận 0,1%/5% cho năm 2026.
- Spread, slippage, khớp một phần, mất kết nối và rủi ro an ninh không thể suy ra chỉ từ thuế suất hoặc phí niêm yết.

## 8. Sai Lầm Phổ Biến và Phản Biện
| Nhận định | Vì sao sai | Thay thế bằng |
|---|---|---|
| “ATO chắc chắn mua được ở giá mở cửa tôi muốn.” | ATO tham gia cơ chế xác định giá mở cửa, không phải LO có mức giá cam kết. | Đọc quy tắc phiên và chấp nhận phần không khớp. |
| “Market order luôn có giá tốt nhất.” | Chỉ mức đối ứng đầu tiên có thể tốt; sổ mỏng gây trượt qua nhiều mức. | Kiểm tra độ sâu, spread và cân nhắc LO. |
| “Zero-fee nghĩa là không có chi phí.” | Thuế, spread, slippage, margin và phí khác vẫn có thể tồn tại. | Đọc bảng chi phí toàn phần. |
| “Lỗ thì không có thuế bán.” | Thuế chuyển nhượng dùng giá bán theo tỷ lệ trong ví dụ, không dùng lãi/lỗ. | Tách thuế khỏi P/L kinh tế. |

## 9. Ôn Tập Giữa Tuần Tương Tác
<details>
<summary>Thẻ tự kiểm tra 1 — chọn lệnh</summary>

Bạn muốn không mua cao hơn 20.000 VND nhưng chấp nhận không khớp. Lựa chọn phù hợp về nguyên tắc là LO mua với giá tối đa 20.000 VND, sau đó kiểm tra hiệu lực và đơn vị lệnh của sàn.
</details>

<details>
<summary>Thẻ tự kiểm tra 2 — thuế và phí</summary>

Với giá trị bán giả định 20.000.000 VND, **trong khung lịch sử minh họa** 0,1% là 20.000 VND. Phí môi giới 0,15% giả định là 30.000 VND, nhưng minimum/fixed fee bị giả định bằng 0. Hai khoản này khác cơ sở và khác bản chất; giao dịch 2026 phải kiểm tra lại văn bản và biểu phí.
</details>

## 10. Bài Tập Ứng Dụng
Lập một bảng cho giao dịch giả định trị giá 50.000.000 VND với ba kịch bản phí môi giới 0%, 0,15% và 0,30%; **khung thuế lịch sử minh họa** 0,1%; minimum/fixed fee lần lượt 0 và 50.000 VND; spread 0,20%; slippage 0%, 0,30% và 1%. Ghi rõ khoản nào là giả định lịch sử, khoản nào cần kiểm tra luật 2026 và điều khoản chuyển tiếp, khoản nào là giả định thị trường. Sau đó viết 120–150 từ phản biện câu “zero-fee là miễn phí”. Không dùng tiền thật để làm bài.

## 11. Glossary Song Ngữ
| Việt | English | Định nghĩa |
|---|---|---|
| Lệnh giới hạn | Limit Order (LO) | Lệnh mua/bán tại giá chỉ định hoặc tốt hơn. |
| Lệnh mở cửa | At-the-Open (ATO) | Lệnh dùng trong khớp định kỳ xác định giá mở cửa; phần chưa khớp bị hủy theo quy tắc. |
| Lệnh đóng cửa | At-the-Close (ATC) | Lệnh dùng trong khớp định kỳ xác định giá đóng cửa; phần chưa khớp hết hạn theo quy tắc. |
| Lệnh thị trường giới hạn | Market-to-Limit (MTL) | Lệnh ưu tiên khớp giá đối ứng hiện có; phần còn lại có thể chuyển thành LO theo quy chế. |
| Chênh lệch giá mua-bán | Bid-Ask Spread | Khoảng cách giữa giá mua cao nhất và giá bán thấp nhất đang hiển thị. |
| Trượt giá | Slippage | Chênh giữa giá kỳ vọng và giá khớp thực tế. |
| Thuế chuyển nhượng | Securities Transfer Tax | Trong ví dụ **lịch sử/minh họa** dựa trên TT111/2013 và TT92/2015: dùng 0,1% trên giá chuyển nhượng mỗi lần bán; không phải mức hiện hành 2026. |
| Thuế đầu tư vốn | Tax on Capital Investment Income | Trong ví dụ **lịch sử/minh họa**: dùng 5% trên cổ tức tiền mặt; không phải mức hiện hành 2026. |

## 12. Nguồn, Tuyên Bố Giới Hạn và Điều Hướng
**Nguồn đã mở và đọc:** HOSE, “Order types”, https://staticfile.hsx.vn/Uploads/UploadDocuments/2372213/6.Order%20types.pdf; Công báo Chính phủ, Thông tư 111/2013/TT-BTC, https://congbao.chinhphu.vn/van-ban/thong-tu-so-111-2013-tt-btc-7822/4293.htm; CSDL VBPL Bộ Tài chính, https://vbpl.vn/botaichinh/Pages/vbpq-thuoctinh-hopnhat.aspx?ItemID=139757; Công báo Chính phủ, Thông tư 92/2015/TT-BTC, https://congbao.chinhphu.vn/van-ban/thong-tu-so-92-2015-tt-btc-15620.htm; Công báo Chính phủ, Thông tư 128/2018/TT-BTC, https://congbao.chinhphu.vn/van-ban/thong-tu-so-128-2018-tt-btc-28190/25383.htm. Ngày kiểm tra: **2026-09-16**. Các văn bản 2025/2026 trong phạm vi W12-C05–C08 đã được mở/đọc ở các điều khoản được trích dẫn cho cá nhân cư trú, cổ tức bằng tiền/cổ phiếu và chuyển nhượng chứng khoán thông thường. Bài không suy rộng sang điều khoản chuyển tiếp, ngoại lệ, ESOP, phái sinh, sản phẩm khác hoặc tình trạng cư trú khác; các trường hợp đó vẫn cần kiểm tra văn bản hiện hành tương ứng.

**Disclaimer:** Nguồn luật có thể được sửa đổi, thay thế hoặc áp dụng khác theo tư cách người nộp thuế và sản phẩm. Hãy kiểm tra văn bản gốc/cơ sở dữ liệu chính thức và hỏi chuyên gia đủ điều kiện trước quyết định thực tế.

**Điều hướng:** [Quiz W12](../../assessments/quizzes/week-12-quiz.md) · [Answer key W12](../../assessments/answer-keys/week-12-answers.md) · [Flashcards W12](../../flashcards/week-12.yaml) · [Source ledger](../../quality/module-3-source-ledger.md)

## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

**Bài:** Chi phí giao dịch. **Đầu ra:** Làm bảng 50 triệu VND ở mục 10. Ghi cách mô phỏng spread/slippage và tránh tính hai lần.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| Phạm vi | Ghi rõ thuế là giả định lịch sử; cần tra luật hiện hành trước khi áp dụng | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Phí | 0/0,15/0,30% tương ứng 0/75.000/150.000 VND; minimum 50.000 dùng max, không cộng nhầm | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Chi phí khác | Giả định thuế 0,1%=50.000; spread 0,20%=100.000; slippage 0/0,30/1%=0/150.000/500.000 VND | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Tính ròng | Tách giá trị, phí và ảnh hưởng thực thi; không trừ spread/slippage nữa nếu đã nằm trong giá khớp | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Phản biện | 120–150 từ giải thích zero broker fee vẫn có chi phí; không tuyên bố các số là phí thực tế | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
