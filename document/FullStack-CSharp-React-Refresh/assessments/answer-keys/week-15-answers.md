# Đáp án tuần 15 — JWT, authorization và ownership

[Quiz](../quizzes/week-15-quiz.md) · [Bài nguồn](../../notes/module-4/week-15.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q15-01 | LO-W15-01 | Analyze | JWT decode | D | W15-C01 |
| Q15-02 | LO-W15-01 | Analyze | Production auth | A | W15-C01 |
| Q15-03 | LO-W15-02 | Analyze | Ownership | B | W15-C02 |
| Q15-04 | LO-W15-02 | Analyze | Forbidden | C | W15-C02 |
| Q15-05 | LO-W15-03 | Analyze | Logout | D | W15-C03 |
| Q15-06 | LO-W15-03 | Analyze | Secrets | A | W15-C03 |



## Giải thích từng phương án

### Q15-01

DecodeJSON đủ tin user chưa?

- **Đáp án đúng:** **D** — Chưa

- **Phương án D ĐÚNG:** Signature/issuer/audience/lifetime lập trust.

- **A SAI:** Syntax không authenticity.

- **B SAI:** Attacker cũng viết claim nếu không verify.

- **C SAI:** Length không trust cryptographic.

---

Vị trí bài học: [LO-W15-01](../../notes/module-4/week-15.md#lo-w15-01)

Ôn [bài 1](../../notes/module-4/week-15.md) · LO-W15-01 · W15-C01.

### Q15-02

ProdJWT nên lấy hướng nào?

- **Đáp án đúng:** **A** — OIDC/OAuth provider,code+PKCEchoSPA

- **Phương án A ĐÚNG:** Standards SDK quản proper tokenflow.

- **B SAI:** Devkey không prodidentitysystem.

- **C SAI:** Custom insecureissuance thiếu standards.

- **D SAI:** Publicclient không giữ secret.

---

Vị trí bài học: [LO-W15-01](../../notes/module-4/week-15.md#lo-w15-01)

Ôn [bài 1](../../notes/module-4/week-15.md) · LO-W15-01 · W15-C01.

### Q15-03

Authorize tự chặn Bob đọc Alice expense chưa?

- **Đáp án đúng:** **B** — Chưa, phải query/resource ownership

- **Phương án B ĐÚNG:** Authenticated không tự biết owner.

- **A SAI:** Browserpolicy không resourcepermission.

- **C SAI:** Token không tự filter domainDB.

- **D SAI:** Clienthide bypassHTTP.

---

Vị trí bài học: [LO-W15-02](../../notes/module-4/week-15.md#lo-w15-02)

Ôn [bài 2](../../notes/module-4/week-15.md) · LO-W15-02 · W15-C02.

### Q15-04

Valid principal thiếu required policy trả gì?

- **Đáp án đúng:** **C** — 403

- **Phương án C ĐÚNG:** Forbidden là lack permission sau auth.

- **A SAI:** Không tạo resource.

- **B SAI:** Không delete/successbodyless.

- **D SAI:** Đề đãvalididentity.

---

Vị trí bài học: [LO-W15-02](../../notes/module-4/week-15.md#lo-w15-02)

Ôn [bài 2](../../notes/module-4/week-15.md) · LO-W15-02 · W15-C02.

### Q15-05

Xóa tokenlocal revoke mọiJWTserver ngay không?

- **Đáp án đúng:** **D** — Không

- **Phương án D ĐÚNG:** Lifetime/revocation server riêng.

- **A SAI:** JWT không tự sessionstore bắt buộc.

- **B SAI:** Encoding không revoke.

- **C SAI:** Rerender không đổi servertrust.

---

Vị trí bài học: [LO-W15-03](../../notes/module-4/week-15.md#lo-w15-03)

Ôn [bài 3](../../notes/module-4/week-15.md) · LO-W15-03 · W15-C03.

### Q15-06

Signingkey trongVITE_ có an toàn không?

- **Đáp án đúng:** **A** — Không

- **Phương án A ĐÚNG:** Build frontend public values.

- **B SAI:** Tên env không security boundary.

- **C SAI:** Clientvẫn tải bundleHTTPS.

- **D SAI:** Minify không encryption.

---

Vị trí bài học: [LO-W15-03](../../notes/module-4/week-15.md#lo-w15-03)

Ôn [bài 3](../../notes/module-4/week-15.md) · LO-W15-03 · W15-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
