> [!CAUTION]
> Nội dung chỉ mang tính giáo dục, KHÔNG phải lời khuyên đầu tư.
> Mọi quyết định đầu tư là trách nhiệm cá nhân của bạn.
> Hãy tham khảo chuyên gia tài chính phù hợp trước khi đầu tư.

# W01 — Tiền tệ, Lạm phát, Lãi suất và Giá trị thời gian của tiền (TVM)

## 1. Vì sao cần học
Tuần này xây nền cho các tuần phân tích tài sản và doanh nghiệp: quy đổi tiền ở các thời điểm khác nhau, tách giá trị danh nghĩa khỏi sức mua, và đọc đúng một mức lãi suất. Bài học chỉ dùng tình huống giả định, không yêu cầu dùng tiền thật hay cung cấp dữ liệu tài chính cá nhân.

**Kiến thức tiên quyết:** số học phần trăm, lũy thừa cơ bản và cách đọc bảng tính.

**Fast-Track Core (30–45 phút):** đọc mục 1–4 (20–25 phút), làm Mini-review 1–2 (5–10 phút), làm quiz 8 câu (8–10 phút), rồi đối chiếu answer key. Phần đào sâu gồm Mini-review 3, mục “Biết được gì / Chưa thể kết luận gì” và hoạt động tự nguyện.

## 2. Mục tiêu học tập đo được
- [LO-W01-01] Giải thích bằng lời TVM, Present Value (PV), Future Value (FV), lãi suất danh nghĩa, lãi suất hiệu dụng và lãi suất thực.
- [LO-W01-02] Tính FV hoặc PV cho một khoản tiền duy nhất, ghi đúng đơn vị tiền, kỳ tính, lãi suất mỗi kỳ và cách ghép lãi.
- [LO-W01-03] So sánh hai tình huống giả định bằng lãi suất thực xấp xỉ, đồng thời nêu khi nào phải dùng công thức chính xác.
- [LO-W01-04] Kiểm tra một bảng tính dài hạn bằng cách tách dữ kiện, giả định, phí/thuế, rủi ro và điều chưa thể kết luận.

## 3. Khái niệm cốt lõi
### 3.1. TVM, PV và FV
**Giá trị thời gian của tiền (Time Value of Money, TVM)** là khung so sánh một khoản tiền ở các thời điểm khác nhau khi có tỷ lệ sinh lời hoặc chiết khấu phù hợp. **Giá trị hiện tại (Present Value, PV)** là giá trị quy về thời điểm bắt đầu; **giá trị tương lai (Future Value, FV)** là giá trị tại thời điểm kết thúc. [W01-C01]

Với một khoản tiền duy nhất, lãi suất theo mỗi kỳ là `r` và có `n` kỳ:

`FV = PV × (1 + r)^n`

`PV = FV / (1 + r)^n`

Trong đó `r` phải cùng kỳ với `n`: lãi suất tháng đi với số tháng, lãi suất năm đi với số năm. Công thức giả định khoản tiền được giữ nguyên, lãi được tái đầu tư theo đúng cơ chế đã nêu, và chưa tính phí, thuế hoặc dòng tiền phát sinh giữa kỳ. [W01-C02]

### 3.2. Nominal rate, EAR và real rate
**Lãi suất danh nghĩa (nominal rate)** là mức được báo theo quy ước của sản phẩm, chưa phản ánh đầy đủ tần suất ghép lãi. Nếu lãi suất danh nghĩa năm `j` được ghép `m` lần/năm, **lãi suất hiệu dụng năm (Effective Annual Rate, EAR)** là:

`EAR = (1 + j/m)^m - 1`

Giữ `j` không đổi, ghép thường xuyên hơn làm EAR cao hơn trong mô hình không có phí và các điều kiện khác không đổi. [W01-C03]

**Lãi suất thực (real interest rate)** điều chỉnh cho lạm phát. Cách tính xấp xỉ thường dùng là `real ≈ nominal - inflation`, thường được gọi là cách trình bày gần đúng theo **phương trình Fisher (Fisher equation)**. Công thức chính xác cho một kỳ là:

`1 + real = (1 + nominal) / (1 + inflation)`

Với 6% nominal và 3% inflation, xấp xỉ là 3%; công thức chính xác cho kết quả khoảng 2,91%. Ghi rõ mình đang dùng xấp xỉ hay công thức chính xác. [W01-C04]

### 3.3. Lạm phát và CPI
**Lạm phát (inflation)** là sự tăng mức giá chung theo thời gian; **sức mua (purchasing power)** của cùng một lượng tiền có thể giảm khi giá tăng. Chỉ số giá tiêu dùng (Consumer Price Index, CPI) đo thay đổi trung bình theo thời gian của giá một giỏ hàng hóa và dịch vụ tiêu dùng đại diện; CPI là mức trung bình, không phải tỷ lệ lạm phát riêng của từng người. [W01-C05]

Tách **dữ kiện đã nguồn hóa**, **giả định minh họa**, **kết quả phép tính** và **diễn giải thực hành**. Lợi suất quá khứ không tự động là dự báo tương lai.

## 4. Ví dụ và worked examples
### 4.1. Ví dụ tính FV (giả định)
**Giả định của người viết:** 10.000.000 VND, lãi suất 6%/năm, ghép lãi hàng năm, giữ 3 năm, không nộp thêm, chưa tính thuế/phí và không có rủi ro mất vốn trong mô hình.

`FV = 10.000.000 × (1 + 0,06)^3 = 11.910.160 VND` (11.910.000 VND nếu làm tròn đến nghìn đồng). Đây là kết quả của mô hình giả định, không phải cam kết lợi nhuận. [W01-C06]

### 4.2. Ví dụ tính PV (giả định)
**Dữ kiện giả định:** cần 12.100.000 VND sau 2 năm; tỷ lệ chiết khấu 10%/năm, ghép hàng năm.

`PV = 12.100.000 / (1 + 0,10)^2 = 10.000.000 VND`.

Nếu tỷ lệ chiết khấu, kỳ ghép, thời điểm nhận tiền hoặc dòng tiền thay đổi, PV cũng thay đổi. Không diễn giải PV là “giá đúng duy nhất” của một tài sản.

### 4.3. Ví dụ nominal, EAR và real (giả định)
Với nominal `j = 12%/năm`, ghép hàng tháng (`m = 12`): `EAR = (1 + 0,12/12)^12 - 1 ≈ 12,68%/năm`.

Nếu lạm phát cùng kỳ là 5%, real chính xác là `(1,1268 / 1,05) - 1 ≈ 7,31%`; cách xấp xỉ nominal trừ inflation cho kết quả `12,68% - 5% = 7,68%`. Chênh lệch cho thấy cần ghi rõ phương pháp và không làm tròn sớm.

### 4.4. Phân loại bằng chứng
- **Kết quả nghiên cứu/nguồn:** OpenStax trình bày FV/PV, tác động của ghép lãi và nominal/real trong các mục 3.4, 7.2 và 7.4.
- **Dữ kiện nguồn:** BLS mô tả CPI là mức thay đổi trung bình của giá một giỏ hàng tiêu dùng đại diện và cảnh báo CPI không nhất thiết trùng trải nghiệm của từng hộ.
- **Giả định minh họa:** các số 10 triệu, 12,1 triệu, 6%, 10%, 12% và 5% ở trên.
- **Diễn giải thực hành:** trước khi so sánh, lập bảng kỳ tính, ghép lãi, phí, thuế, thanh khoản và rủi ro; đây không phải khuyến nghị giao dịch.

## 5. Mini-review và truy xuất kiến thức
### Mini-review 1: kiểm tra công thức
Không nhìn phần trên, hãy viết công thức FV và PV, sau đó trả lời: nếu `r` là lãi suất tháng thì `n` phải đo bằng gì? **Tự kiểm:** `n` là số tháng; PV là FV chiết khấu về đầu kỳ.

### Mini-review 2: kiểm tra đơn vị
Một bảng tính dùng `8%/năm` nhưng nhập `n = 36` cho 36 tháng. Chỉ ra lỗi và sửa. **Tự kiểm:** hoặc dùng `n = 3` năm nếu ghép hàng năm, hoặc quy đổi rate/kỳ và dùng 36 tháng; không trộn hai quy ước.

### Mini-review 3: phản biện claim
Một quảng cáo nói “12%/năm chắc chắn tăng sức mua”. Viết ba câu hỏi kiểm tra. **Tự kiểm:** 12% là nominal hay EAR; lạm phát/chi phí/thuế là bao nhiêu; lợi nhuận có bảo đảm và rủi ro mất vốn/liquidity ra sao?

## 6. Cảnh báo & sai lầm thường gặp
- Nhầm nominal rate, EAR và real rate; bỏ qua tần suất ghép lãi.
- Làm tròn giữa các bước rồi dùng số đã làm tròn cho bước tiếp theo.
- Dùng CPI trung bình để khẳng định chính xác sức mua của mọi hộ gia đình.
- Dùng bảng tính với mức sinh lời cố định nhiều thập kỷ và coi kết quả là dự báo.
- Nhầm mô hình TVM của một khoản tiền duy nhất với dòng tiền định kỳ.

## 7. Biết được gì / Chưa thể kết luận gì
**Biết được:** cách quy đổi PV/FV; cách đồng bộ rate với kỳ; khác biệt nominal/EAR/real; vì sao CPI là trung bình; cách tách dữ kiện và giả định.

**Chưa thể kết luận:** tài sản sẽ sinh lời bao nhiêu; CPI cá nhân của người học; mức lãi suất/phí/thuế hiện hành của sản phẩm cụ thể; giá trị “đúng” của một khoản đầu tư chỉ từ TVM.

## 8. Checklist thực hành
- [ ] Viết mục tiêu, thời hạn và giới hạn rủi ro.
- [ ] Ghi nguồn, thời điểm, đơn vị và trạng thái xác minh.
- [ ] Tách dữ kiện, giả định, phép tính và kết luận.
- [ ] Kiểm tra kịch bản xấu, thanh khoản, phí và thuế.
- [ ] Không dùng tiền thật chỉ vì hoàn thành bài.

## 9. Hoạt động áp dụng tự nguyện
Tạo một bảng tính **chỉ với số liệu giả định** gồm PV, rate/kỳ, số kỳ, tần suất ghép lãi, FV và một cột “chưa thể kết luận”. Chạy ba kịch bản rate thấp/cơ sở/cao; không nhập thu nhập, tài sản, khoản nợ hoặc tài khoản thật.

## 10. Glossary
| Việt | English | Định nghĩa |
|---|---|---|
| Giá trị thời gian của tiền | Time Value of Money (TVM) | Nguyên lý tiền ở hiện tại có giá trị hơn cùng số tiền trong tương lai do khả năng sinh lời. |
| Giá trị hiện tại | Present Value (PV) | Giá trị quy về thời điểm hiện tại của một khoản tiền hoặc dòng tiền tương lai với tỷ lệ chiết khấu xác định. |
| Giá trị tương lai | Future Value (FV) | Giá trị của một khoản tiền hiện tại tại một thời điểm trong tương lai sau khi tính lãi kép. |
| Lãi kép | Compound Interest | Tiền lãi tính trên cả vốn gốc ban đầu lẫn lãi tích lũy của các kỳ trước đó. |
| Lãi suất thực | Real Interest Rate | Lãi suất đã điều chỉnh theo lạm phát, phản ánh sự gia tăng thực tế của sức mua (r ≈ i - π). |
| Lạm phát | Inflation | Tốc độ tăng mức giá chung của hàng hóa và dịch vụ theo thời gian, làm suy giảm sức mua tiền tệ. |
| Lãi suất danh nghĩa | Nominal Interest Rate | Lãi suất được công bố theo quy ước của sản phẩm, chưa phản ánh đầy đủ tác động của tần suất ghép lãi và lạm phát. |
| Lãi suất hiệu dụng năm | Effective Annual Rate (EAR) | Lãi suất một năm sau khi tính tần suất ghép lãi: EAR = (1 + j/m)^m - 1. |
| Chỉ số giá tiêu dùng | Consumer Price Index (CPI) | Chỉ số đo thay đổi trung bình theo thời gian của giá một giỏ hàng hóa và dịch vụ tiêu dùng đại diện. |

## 11. Tài liệu tham khảo đã mở
- OpenStax, *Principles of Finance 2e*, mục 7.2: https://openstax.org/books/principles-finance-2e/pages/7-2-time-value-of-money-tvm-basics (truy cập 2026-09-15).
- OpenStax, *Principles of Finance 2e*, mục 7.4: https://openstax.org/books/principles-finance-2e/pages/7-4-applications-of-tvm-in-finance (truy cập 2026-09-15).
- OpenStax, *Principles of Finance 2e*, mục 3.4: https://openstax.org/books/principles-finance-2e/pages/3-4-interest-rates (truy cập 2026-09-15).
- U.S. Bureau of Labor Statistics, “Consumer Price Index Frequently Asked Questions”, câu 2 và 4: https://www.bls.gov/cpi/questions-and-answers.htm (truy cập 2026-09-15).
- Source ledger: [quality/module-1-source-ledger.md](../../quality/module-1-source-ledger.md).

## 12. Next steps
- [Quiz W01](../../assessments/quizzes/week-01-quiz.md)
- [Answer key W01](../../assessments/answer-keys/week-01-answers.md)

## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

**Bài:** TVM và lãi suất. **Đầu ra:** Làm lại mục4.1–4.3, sau đó thay thời hạn3 năm thành4 năm.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| Đầu vào | Ghi vốn, kỳ ghép, thời hạn, phí/thuế bị loại trừ | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| FV/PV | Đúng công thức và phép tính: FV base11.910.160 VND, PV10.000.000 VND | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| EAR/real | Phân biệt nominal12% và EAR≈12,68%; real chính xác khoảng7,32%, không dùng phép trừ như kết quả chính xác | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Độ nhạy | FV4 năm=12.624.769,6 VND, giữ các đầu vào khác | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Kết luận | Nêu mô hình giả định và đơn vị; không hứa lợi nhuận | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
