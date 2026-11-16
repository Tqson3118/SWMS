# Đáp án tuần 26 — Redis, cache-aside và consistency

[Quiz](../quizzes/week-26-quiz.md) · [Bài nguồn](../../notes/module-7/week-26.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q26-01 | LO-W26-01 | Analyze | Truth source | C | W26-C01 |
| Q26-02 | LO-W26-01 | Analyze | Key scope | D | W26-C01 |
| Q26-03 | LO-W26-02 | Analyze | Write order | A | W26-C02 |
| Q26-04 | LO-W26-02 | Analyze | DEL race | B | W26-C02 |
| Q26-05 | LO-W26-03 | Analyze | Cache outage | C | W26-C03 |
| Q26-06 | LO-W26-03 | Analyze | Stampede | D | W26-C03 |



## Giải thích từng phương án

### Q26-01

Authoritative expenseamount nằm đâu?

- **Đáp án đúng:** **C** — SQL database có constraints/transactions

- **Phương án C ĐÚNG:** Durableledger cần invariant và transaction.

- **A SAI:** Browserstate không authoritative server.

- **B SAI:** Chart là presentation output.

- **D SAI:** Cache có thể evict/expire/mất.

---

Vị trí bài học: [LO-W26-01](../../notes/module-7/week-26.md#lo-w26-01)

Ôn [bài 1](../../notes/module-7/week-26.md) · LO-W26-01 · W26-C01.

### Q26-02

Monthlycachekey cần phần nào?

- **Đáp án đúng:** **D** — Schema/user/month/filter

- **Phương án D ĐÚNG:** Đủ identity/shape tránh trộn dữ liệu.

- **A SAI:** Globalkeylộ ownerdata.

- **B SAI:** Monthkhông phân biệt principal.

- **C SAI:** Client không được chọn scopeowner.

---

Vị trí bài học: [LO-W26-01](../../notes/module-7/week-26.md#lo-w26-01)

Ôn [bài 1](../../notes/module-7/week-26.md) · LO-W26-01 · W26-C01.

### Q26-03

Invalidatecache vào thời điểm nào?

- **Đáp án đúng:** **A** — Sau SQLcommit rồi bỏ affectedkeys

- **Phương án A ĐÚNG:** Tránh readerfillold khi DBchưa đổi.

- **B SAI:** Invalidatebeforecommit dễ refillold.

- **C SAI:** Cache stale không bounded nếu thiếuTTL.

- **D SAI:** Authphải enforce trước workowner.

---

Vị trí bài học: [LO-W26-02](../../notes/module-7/week-26.md#lo-w26-02)

Ôn [bài 2](../../notes/module-7/week-26.md) · LO-W26-02 · W26-C02.

### Q26-04

DELsaucommit bảo đảm zerostale không?

- **Đáp án đúng:** **B** — Không, late reader có thể SETold

- **Phương án B ĐÚNG:** In-flightreader không atomic vớidelete.

- **A SAI:** Await không khóa distributedread/write.

- **C SAI:** Delete không chặn reader đang chạy.

- **D SAI:** TTL chỉ boundsstaleness.

---

Vị trí bài học: [LO-W26-02](../../notes/module-7/week-26.md#lo-w26-02)

Ôn [bài 2](../../notes/module-7/week-26.md) · LO-W26-02 · W26-C02.

### Q26-05

Redisoptionaldown nên xử lý sao?

- **Đáp án đúng:** **C** — Fallback SQL và metric/log bounded

- **Phương án C ĐÚNG:** Cache là optimization có degrade path.

- **A SAI:** Zero giả làm sai finance data.

- **B SAI:** Unboundedretry gây latency/resourceexhaustion.

- **D SAI:** Optionaldependency không ledgerboundary.

---

Vị trí bài học: [LO-W26-03](../../notes/module-7/week-26.md#lo-w26-03)

Ôn [bài 3](../../notes/module-7/week-26.md) · LO-W26-03 · W26-C03.

### Q26-06

Nhiều miss cùngkey gây gì?

- **Đáp án đúng:** **D** — Nhiều SQLreads đồng thời

- **Phương án D ĐÚNG:** Cachemiss không tự coalesce work.

- **A SAI:** Cacheaside không tự distributedlock.

- **B SAI:** Authrenewal khác reportcache.

- **C SAI:** Coldkey có thể tăng DBload.

---

Vị trí bài học: [LO-W26-03](../../notes/module-7/week-26.md#lo-w26-03)

Ôn [bài 3](../../notes/module-7/week-26.md) · LO-W26-03 · W26-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
