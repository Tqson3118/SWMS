# Tuần 04 — Promises, fetch và event loop

Chặng 1; 3 bài L04-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W04-01: Theo vết resolve/reject và xử lý lỗi async đúng boundary.

- LO-W04-02: Tách lỗi HTTP/network/abort và hiển thị loading/error/empty.

- LO-W04-03: Dự đoán sync/microtask/timer order và dùng Network tab đọc response.

Tiên quyết: Đạt gate tuần 03; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-04-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w04-01"></a>

## Bài 1: Promise và async/await

LO-W04-01 · W04-C01

Promise đại diện kết quả sẽ có: pending -> fulfilled/rejected một lần. then trả Promise mới; callback trả value thì chain fulfilled, throw thì rejected. async function luôn trả Promise; await tạm dừng function async chờ settle, không khóa browser main thread đang làm công việc khác. try/catch bắt rejection tại await; gọi async không await rồi try/catch ngoài không bắt theo cách mong đợi. Promise.all chạy các operations đã khởi tạo và reject khi một input reject; không tự hủy phần còn lại. Với import cần giới hạn concurrency, không map1000 request vô hạn. finally reset loading, không nuốt lỗi rồi coi success.

<a id="lo-w04-02"></a>

## Bài 2: Fetch, HTTP và cancellation

LO-W04-02 · W04-C02

fetch resolve Response cả khi HTTP400/500; phải check response.ok. JSON parse cũng async và có thể lỗi khi body sai; đọc body một lần. DELETE204 không có JSON. CORS là browser policy do server cho origin đọc response, không thay auth; no-cors trả opaque response không cứu API. AbortController.signal hủy request client, backend vẫn có thể commit trước đó, nên không suy ra server không ghi. Request mới có thể trả trước request cũ; abort previous và/hoặc dùng sequence guard để tránh stale UI. Luôn set loading trước request, clear finally chỉ nếu còn current request. Empty array là success rỗng, khác error.

<a id="lo-w04-03"></a>

## Bài 3: Event loop và debugging network

LO-W04-03 · W04-C03

Call stack chạy synchronous code tới hết lượt. Promise reactions/continuation await vào microtask queue; timer callback vào task queue khi đủ thời gian, không đảm bảo chính xác ms. Sau task, microtasks drain trước task tiếp và cơ hội render; loop CPU nặng vẫn block dù nằm trong async function trước await. Ví dụ log A, setTimeout T0, Promise.then P, log B -> A,B,P,T trong môi trường browser đơn giản này. fetch do browser/network tiến hành, Promise callback vẫn chạy trên JS execution context. DevTools Network xem method/status/request body/response/timing; 401 khác CORS/network error. Không log bearer token khi chụp evidence.

## Ví dụ giải từng bước

1. Bài toán: tải mock expenses và không coi 404 là thành công.
2. Dùng file fixture.json trên static server với array hợp lệ.
3. Chạy:
```javascript
async function loadExpenses(url,signal){
 const r=await fetch(url,{signal});
 if(!r.ok) throw new Error("HTTP "+r.status);
 const data=await r.json();
 if(!Array.isArray(data)) throw new Error("Sai shape");
 return data;
}
const controller=new AbortController();
loadExpenses("./fixture.json",controller.signal)
 .then(xs=>console.log("count",xs.length))
 .catch(e=>console.log(e.name==="AbortError"?"Đã hủy":e.message));
console.log("A");
setTimeout(()=>console.log("T"),0);
Promise.resolve().then(()=>console.log("P"));
console.log("B");
```
4. Expected order đồng bộ A,B rồi P,T (log network count phụ thuộc thời gian). Rename URL -> HTTP404; fixture [] -> success count0; JSON sai -> parse error. Không so thứ tự count với timer.

## Đào sâu và lỗi thường gặp

await không biến CPU work thành background thread; long loop vẫn làm UI giật. Promise.all fail-fast không rollback. Network offline/CORS có thể đều hiện TypeError trong JS nên đọc Network/server logs để chẩn đoán. Abort khi POST đã commit phải tra operation ID thay tự retry không idempotency.

## Bài tập thực hành

### Bài tập 1

Tạo load state idle/loading/success/error; test file [] và URL404.

### Bài tập 2

Search A chậm 200ms, B nhanh20ms; hiển thị kết quả mới nhất.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

State loading trước await; success data sau parse/guard; [] hiện 'Chưa có khoản chi'; catch error message; finally reset. Test 404 không được render success và không spinner mãi.

### Hướng giải 2

Tăng sequence mỗi search; callback chỉ set state nếu id===current. Abort previous để giảm công việc nhưng guard vẫn cần vì response có thể đã đến. Expected cuối UI B.

## Project task

JS UI tải fixture JSON bằng fetch, có loading/error/empty, retry button và cancel. Giữ CRUD local mock; chưa gửi API giả làm production. Tag phase1-js.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w04.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Test fixture 3/[]/JSON sai/404/offline có state đúng và loading kết thúc.

- [ ] Request cũ không ghi đè request mới trong test delayed A/B.

- [ ] Giải thích A,B,P,T; không gọi await là tạo thread hay rollback request.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'Why does fetch not reject on HTTP 404?' Dùng promise, rejection, response, microtask, cancellation trong 5 câu.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Promise chaining and error handling](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Using_promises) — W04-C01; primary. HTTP: 200; section xem ledger.

- [Checking response status and canceling a request](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch) — W04-C02; primary. HTTP: 200; section xem ledger.

- [Job queue and event loop](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Execution_model) — W04-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-04-quiz.md) · [Đáp án](../../assessments/answer-keys/week-04-answers.md) · [Flashcards](../../flashcards/week-04.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-03.md)

[Tuần tiếp](../module-2/week-05.md)
