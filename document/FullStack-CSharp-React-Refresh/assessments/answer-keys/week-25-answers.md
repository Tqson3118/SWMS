# Đáp án tuần 25 — HTTP, rate limiting và practical design

[Quiz](../quizzes/week-25-quiz.md) · [Bài nguồn](../../notes/module-7/week-25.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q25-01 | LO-W25-01 | Analyze | Retry semantics | B | W25-C01 |
| Q25-02 | LO-W25-01 | Analyze | Privacy headers | C | W25-C01 |
| Q25-03 | LO-W25-02 | Analyze | 429 setup | D | W25-C02 |
| Q25-04 | LO-W25-02 | Analyze | Distributed quota | A | W25-C02 |
| Q25-05 | LO-W25-03 | Analyze | Capacity math | B | W25-C03 |
| Q25-06 | LO-W25-03 | Analyze | Design choice | C | W25-C03 |



## Giải thích từng phương án

### Q25-01

HTTP400invalidamount nên retry tự động không?

- **Đáp án đúng:** **B** — Không, sửa input

- **Phương án B ĐÚNG:** Validationfailure không transient.

- **A SAI:** Request này invalid không retrymagic.

- **C SAI:** Repeatedinvalidpayload không chữa lỗi.

- **D SAI:** Token không sửa amountpolicy.

---

Vị trí bài học: [LO-W25-01](../../notes/module-7/week-25.md#lo-w25-01)

Ôn [bài 1](../../notes/module-7/week-25.md) · LO-W25-01 · W25-C01.

### Q25-02

Ownerfinance report có được sharedpubliccache không?

- **Đáp án đúng:** **C** — Không, dùng private/no-store policy phù hợp

- **Phương án C ĐÚNG:** Sharedcache có thể lộ dữ liệu giữa users.

- **A SAI:** Decode token không partitioncache.

- **B SAI:** Payloadsize không securityboundary.

- **D SAI:** URLkhông đủ identityscope.

---

Vị trí bài học: [LO-W25-01](../../notes/module-7/week-25.md#lo-w25-01)

Ôn [bài 1](../../notes/module-7/week-25.md) · LO-W25-01 · W25-C01.

### Q25-03

Lab muốn rate reject429 cần gì?

- **Đáp án đúng:** **D** — Đặt RejectionStatusCode=429

- **Phương án D ĐÚNG:** Cấu hình rõ responsecontract.

- **A SAI:** Không giả định defaultstatus tương đương.

- **B SAI:** 200che errorsemantics.

- **C SAI:** Log không báo client rejection.

---

Vị trí bài học: [LO-W25-02](../../notes/module-7/week-25.md#lo-w25-02)

Ôn [bài 2](../../notes/module-7/week-25.md) · LO-W25-02 · W25-C02.

### Q25-04

Limiter memory trên 2 replicas có globalquota không?

- **Đáp án đúng:** **A** — Không, mỗi process giữ state riêng

- **Phương án A ĐÚNG:** Statein-process không shared.

- **B SAI:** Framework không tự dùng Redis.

- **C SAI:** LBroute không tự aggregatequota.

- **D SAI:** Cùng source không sharedmemory.

---

Vị trí bài học: [LO-W25-02](../../notes/module-7/week-25.md#lo-w25-02)

Ôn [bài 2](../../notes/module-7/week-25.md) · LO-W25-02 · W25-C02.

### Q25-05

100active×2req/min tương đương RPS nào?

- **Đáp án đúng:** **B** — Khoảng3.33

- **Phương án B ĐÚNG:** 200requests/min chia60seconds.

- **A SAI:** Chia thêm100 sai đơn vị.

- **C SAI:** 200 là requests/min chưa đổi đơn vị.

- **D SAI:** Nhân60 sai khi đổi min→second.

---

Vị trí bài học: [LO-W25-03](../../notes/module-7/week-25.md#lo-w25-03)

Ôn [bài 3](../../notes/module-7/week-25.md) · LO-W25-03 · W25-C03.

### Q25-06

Trước chọn microservices nên làm gì?

- **Đáp án đúng:** **C** — Làm rõ requirements/constraints và đo bottleneck

- **Phương án C ĐÚNG:** Nhu cầu/evidence dẫn architecture.

- **A SAI:** Queue không giải mọi bottleneck.

- **B SAI:** Privacy là requirement project cốt lõi.

- **D SAI:** Distributedcomplexity cần lý do.

---

Vị trí bài học: [LO-W25-03](../../notes/module-7/week-25.md#lo-w25-03)

Ôn [bài 3](../../notes/module-7/week-25.md) · LO-W25-03 · W25-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
