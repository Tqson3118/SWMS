# Tuần 19 — Forms, authentication và edit conflicts

Chặng 5; 3 bài L19-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W19-01: Giữ input string, parse rõ và map field errors.

- LO-W19-02: Dùng provider code flow PKCE và bearer đúng audience.

- LO-W19-03: Xóa cache theo user và xử lý401/403/404 đúng.

Tiên quyết: Đạt gate tuần 18; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-19-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w19-01"></a>

## Bài 1: Controlled forms và validation

LO-W19-01 · W19-C01

Form control value luôn string khi người dùng đang gõ, kể cả amount. Không dùng Number('') vì ra0 rồi đánh mất trạng thái trống. Submit trim, kiểm digits rồi Number.isSafeInteger/range; server vẫn validate cùng policy. Label htmlFor nối id, lỗi bằng aria-describedby và summary focus khi fail. Disable nút khi submitting để tránh double click; idempotency vẫn cần cho import/retry. POST chỉ gửi writable fields, không UserId; PUT gửi đủ fields và version Base64 từ record đã đọc. Khi409 giữ input và cho reload/so sánh; không âm thầm overwrite phiên bản mới.

<a id="lo-w19-02"></a>

## Bài 2: SPA authentication

LO-W19-02 · W19-C02

Development dùng dotnet user-jwts theo API lab tuần15; chỉ dán token test vào memory của tab để học401/403. Production dùng OIDC/OAuth provider và client SDK được hỗ trợ: đăng ký public SPA client, redirect URI chính xác, authorization code với PKCE, scopes API. Không đặt client secret trong SPA. SDK quản lý login callback/state/nonce và token renewal; không tự viết protocol từ đầu. API nhận access token chứ không id_token; validate signature/issuer/audience/lifetime và ánh xạ issuer+subject sang internal UserId. Decode payload client chỉ phục vụ hiển thị, không chứng minh token hợp lệ.

<a id="lo-w19-03"></a>

## Bài 3: Session UX và ownership

LO-W19-03 · W19-C03

Auth provider/context giữ trạng thái loading/authenticated/anonymous cho UI. Authenticated principal không có quyền trên resource của người khác: API query scope theo UserId cho404. 401 đưa UX đăng nhập lại theo SDK nhưng không tự lặp vô hạn;403 giải thích thiếu policy. Logout hủy requests và bỏ in-memory user data/cache; query cache keys chứa identity để Alice/Bob không chia sẻ. Course không lưu access token dài hạn trong localStorage; XSS có thể đọc storage. Nếu chọn BFF cookie thay SPA bearer thì thiết kế CSRF/SameSite riêng, không trộn hai flow tùy tiện. Login và authorization cần test độc lập.

## Ví dụ giải từng bước

1. Controlled amount bắt đầu ''. Tại submit:
```ts
function parseAmount(text:string):number {
 const s=text.trim();
 if(!/^\d+$/.test(s)) throw new Error('Enter whole VND');
 const n=Number(s);
 if(!Number.isSafeInteger(n)||n<1||n>1_000_000_000)
  throw new Error('Amount out of range');
 return n;
}
```
2. Thử '', '1.5', '-1', '50000'; chỉ50000 pass. Keep note/date inputs khi server400.
3. Alice mở e1 versionV1 ở hai tab; tabA PUT trả V2; tabB PUT V1 nhận409, giữ form.
4. Bob gọi e1 nhận404 dù ID và JSON đúng. Production auth triển khai bằng SDK provider được chọn trong ADR; dev token không dùng deployment.

## Đào sâu và lỗi thường gặp

Auth token không là chỗ lưu finance data. Không log token và không copy vào screenshot. Submit retry sau timeout chưa biết commit: import dùng idempotency key, CRUD tạo cần UX reconcile hoặc bổ sung contract idempotency có thiết kế rõ.

## Bài tập thực hành

### Bài tập 1

Map validation errors amountVnd/spentOn từ ProblemDetails vào form.

### Bài tập 2

Alice logout rồi Bob login thấy Alice total trong cache. Sửa và test.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Dùng allowlist field names; render text, aria-describedby; errors lạ vào summary. Giữ input và traceId; không render raw HTML.

### Hướng giải 2

Clear cached data on logout/user change, cancel/ignore pending Alice responses; key theo internal user identity. Test delayed Alice response sau loginBob không xuất hiện.

## Project task

Hoàn tất create/edit/delete form, conflict recovery và login/logout. Dev token ở memory; thiết kế production provider registration và owner-scoped requests.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w19.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Form chặn empty/decimal/unsafe; server400 giữ input và focus error.

- [ ] Hai-tab PUT cho409 khi version cũ; user phải reload/compare trước retry.

- [ ] No token trên repo/log/storage; Alice/Bob chuyển session không chia sẻ dữ liệu.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Đóng vai phỏng vấn: 'Authentication versus authorization'; giải thích code flow PKCE và token audience trong120 giây.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Controlled input and labels](https://react.dev/reference/react-dom/components/input) — W19-C01; primary. HTTP: 200; section xem ledger.

- [Access token validation and OAuth/OIDC](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0) — W19-C02; primary. HTTP: 200; section xem ledger.

- [Storage scope and behavior](https://developer.mozilla.org/en-US/docs/Web/API/Storage) — W19-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-19-quiz.md) · [Đáp án](../../assessments/answer-keys/week-19-answers.md) · [Flashcards](../../flashcards/week-19.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-18.md)

[Tuần tiếp](week-20.md)
