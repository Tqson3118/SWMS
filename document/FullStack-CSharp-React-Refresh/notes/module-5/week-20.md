# Tuần 20 — Dashboard, accessibility và frontend tests

Chặng 5; 3 bài L20-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W20-01: Tính totals và trạng thái budget đúng contract.

- LO-W20-02: Chọn state owner và dùng reducer khi có nhu cầu.

- LO-W20-03: Kiểm hành vi loading/empty/error/auth bằng roles.

Tiên quyết: Đạt gate tuần 19; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-20-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w20-01"></a>

## Bài 1: Budget dashboard

LO-W20-01 · W20-C01

Monthly dashboard dùng report API server đã scope owner, grouped và filtered, không sum chỉ trang expenses hiện tại. Alice100000,Food80000,Travel20000; Food budget70000 nên vượt 10000,14.29%. Tỷ lệ hiển thị làm tròn 2 decimal; giữ phép tính gốc cho logic. Limit0 và spent>0 biểu thị 'no budget/over limit' theo label đã chọn, không chia0 hay Infinity. Missing budget khác zero budget; viết rõ contract optional. Tables có caption/headers; màu không là dấu hiệu duy nhất, thêm text excess. Số VND format bằng Intl.NumberFormat nhưng payload vẫn number nguyên.

<a id="lo-w20-02"></a>

## Bài 2: State management tối thiểu

LO-W20-02 · W20-C02

Giữ UI state local; filter share qua URL; auth qua context; server data có fetching/caching lifecycle riêng. Không copy report vào nhiều contexts rồi giữ nhiều total không đồng bộ. Reducer hữu ích khi form workflow có transitions loading/saving/conflict/done; pure reducer nhận state+action trả next. Context truyền dependency/state, không tự cache hay tránh rerenders. Memoization/Redux/query library là tùy chọn, cần ADR ghi pain point và đơn giản hóa. Invalidate/refetch expenses/report/budgets sau write thành công; optimistic update chỉ thêm khi đã thiết kế rollback/version conflict.

<a id="lo-w20-03"></a>

## Bài 3: User-focused testing và accessibility

LO-W20-03 · W20-C03

Component tests dùng React Testing Library truy role/name/label như người dùng. Mock network ở boundary để test delay,500,400,409,204 chứ không mock mọi hook; integration/browser smoke test gọi API thật fixture Alice/Bob ở môi trường test. Vitest với jsdom và jest-dom setup là lựa chọn; khóa versions thực tế trong lockfile. Test behavior, không snapshot toàn DOM dễ nhiễu. Tab order, focus error, contrast và mobile320px cần manual checks vì automated accessibility không chứng minh mọi thứ. Screen reader status announce aria-live polite; không đọc lại toàn table mỗi refresh.

## Ví dụ giải từng bước

1. Test expected report bằng fixture tổng100000,Food80000,limit70000.
2. Pure function:
```ts
export function budgetStatus(spent:number,limit:number|null) {
 if(limit===null) return {label:'No budget',excess:0,percent:null};
 const excess=Math.max(0,spent-limit);
 return {label:excess?'Over budget':'Within budget',excess,
 percent:limit===0?null:excess/limit*100};
}
```
3. Vitest assertions expect(budgetStatus(80000,70000).excess).toBe(10000); percent closeTo14.285714; zero returns null.
4. Component integration mock500 then retry200; queryByRole table absent duringerror, getByRole button name Retry works. Test module src/budgetStatus.test.ts; install vitest/dev script rồi run.

## Đào sâu và lỗi thường gặp

Missing budget với spent không trả 'free spending' như khuyến nghị tài chính; course chỉ theo dõi dữ liệu. Không dùng page subtotal làm monthly total. Test login/logout và stale response quan trọng hơn đạt tỷ lệ coverage tùy ý.

## Bài tập thực hành

### Bài tập 1

Viết 4 testcases cho budget zero/missing/equal/over.

### Bài tập 2

Lập test UI retry và empty riêng.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

(20000,0)=>excess20000 percentnull; (20000,null)=>No budget; (70000,70000)=>0; (80000,70000)=>10000/14.29%. Assert label tương ứng.

### Hướng giải 2

Mock500→show alert+Retry, click→200items; mock200items[]→empty CTA không alert. Gọi bằng role/name, không class selector.

## Project task

Thêm dashboard/monthly budgets, responsive shell và bộ frontend behavior tests. Chuẩn bị screenshot/video demo Alice/Bob, lỗi/retry và conflict.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w20.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Dashboard100000/80000/20000; excess10000/14.29%,zero không Infinity.

- [ ] Test loading/empty/error/retry/400/409/204 và session switch pass.

- [ ] 320px không horizontal overflow; mọi form dùng keyboard và focus error được.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Demo dashboard trong3 phút bằng English; mô tả một failing UI test và cách sửa.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Formatting numeric values](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/NumberFormat) — W20-C01; primary. HTTP: 200; section xem ledger.

- [Pure reducers and actions](https://react.dev/learn/extracting-state-logic-into-a-reducer) — W20-C02; primary. HTTP: 200; section xem ledger.

- [Tests that resemble user interaction](https://testing-library.com/docs/react-testing-library/intro/) — W20-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-20-quiz.md) · [Đáp án](../../assessments/answer-keys/week-20-answers.md) · [Flashcards](../../flashcards/week-20.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-19.md)

[Tuần tiếp](../module-6/week-21.md)
