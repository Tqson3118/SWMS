# Tuần 18 — Hooks, routing và API lifecycle

Chặng 5; 3 bài L18-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W18-01: Đồng bộ external API mà không tạo vòng lặp.

- LO-W18-02: Tạo navigation và deep link có thể tải lại.

- LO-W18-03: Chặn stale result và hiển thị loading/error/empty.

Tiên quyết: Đạt gate tuần 17; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-18-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w18-01"></a>

## Bài 1: Effects và lifecycle

LO-W18-01 · W18-C01

Effect chạy sau commit để đồng bộ external system. Event submit POST thuộc event handler; total derive thuộc render, không cần effect. Dependencies gồm các reactive values effect đọc; bỏ dependency để 'chỉ chạy một lần' tạo stale closure. Effect không khai báo async vì return phải là cleanup hoặc undefined; đặt async function bên trong rồi gọi. Cleanup abort/ignore request cũ trước effect mới và unmount. Strict Mode Development có chu kỳ setup-cleanup-setup để phát hiện thiếu cleanup; không tắt chỉ để giấu bug. useRef giữ controller/request id mà thay đổi không render; useMemo chỉ tối ưu khi có bằng chứng.

<a id="lo-w18-02"></a>

## Bài 2: Routes và URL state

LO-W18-02 · W18-C02

Dùng React Router declarative BrowserRouter, Routes, Route với react-router theo docs phiên bản đã khóa; routes /expenses, /budgets, /reports, /login và catch-all. Link đổi URL mà không full reload. Query string chứa filter tháng/page cho bookmark; state input tạm thời có thể local. Khi đổi from/to hoặc category, reset page về1. Refresh /expenses cần host fallback index.html cho SPA, nhưng /api route phải đến backend. Route guard giúp UX khi chưa login; server vẫn enforce authorization. Không hard-code localhost trong production; cấu hình public API base URL theo environment.

<a id="lo-w18-03"></a>

## Bài 3: Request state machine

LO-W18-03 · W18-C03

API helper kiểm response.ok vì fetch không reject chỉ do404/500; 204 trả undefined không gọi json. Loading hiển thị progress; success có items empty khác error. Union {status:'loading'|'error'|'success'} tránh các boolean mâu thuẫn. GET dựa filter cần abort và ignore để response tháng cũ không ghi đè tháng mới. Không show AbortError như lỗi cho user. Parse ProblemDetails an toàn và display message generic/field error; giữ traceId để báo lỗi. DTO không có runtime trust do TypeScript. Kiểm Number.isSafeInteger cho totals; abort network không chứng minh mutation server đã rollback.

## Ví dụ giải từng bước

1. Trong component list import useEffect/useState; state union để dữ liệu không xuất hiện ở error.
2. Core effect, apiUrl là URL GET đã chứa filter; decodeList là guard tự viết tuần17:
```tsx
useEffect(() => {
 const ac = new AbortController(); let active = true;
 setView({status:'loading'});
 async function load() {
  try {
   const r=await fetch(apiUrl,{signal:ac.signal});
   if(!r.ok) throw new Error(`HTTP ${r.status}`);
   const data=decodeList(await r.json());
   if(active) setView({status:'success',data});
  } catch(e) {
   if(active && !ac.signal.aborted)
    setView({status:'error',message:e instanceof Error?e.message:'Request failed'});
  }
 }
 void load();
 return ()=>{active=false;ac.abort();};
},[apiUrl]);
```
3. Throttle DevTools: Oct chậm, Nov nhanh; đổi filter trước Oct xong. Cuối chỉ hiển thị Nov.
4. GET Bob items=[] hiển thị empty CTA;500 hiển thị retry; không gộp thành 'no expenses'.

## Đào sâu và lỗi thường gặp

Abort giảm công việc nhưng ignore bảo vệ state khi request đã gần hoàn thành. Không thêm rows vào effect dependencies nếu effect setRows mỗi lần. Router state không cache server data. Thư viện query cache là tùy chọn sau khi đã hiểu race/keys/invalidation.

## Bài tập thực hành

### Bài tập 1

Thiết kế xử lý DELETE204 trong api helper.

### Bài tập 2

Filter mới page5 không có items nhưng có totalCount. Sửa UX.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Nếu status===204 return undefined trước json; các status khác check ok và content type. Sau delete refetch list đúng filter/page.

### Hướng giải 2

Reset page1 khi filter thay đổi; khi xóa item cuối trang, tính lại page hợp lệ rồi fetch. Empty state chỉ khi filter không có kết quả.

## Project task

Nối list/pagination/filter React tới API của tuần14–16; thêm routing và GET state machine; lưu filter URL để chia sẻ link.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w18.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Refresh deep link /expenses?month=2026-10 hoạt động; back/forward phục hồi filter.

- [ ] Test Oct/Nov responses đảo thứ tự không ghi đè mới; unmount không update state.

- [ ] Loading,500 retry,Bob empty và success có giao diện riêng; DELETE204 không JSON error.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Đọc Effects docs15 phút; nói 'A stale closure is...' và mô tả cleanup bằng sơ đồ lời nói.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Cleanup, dependencies and development remount](https://react.dev/learn/synchronizing-with-effects) — W18-C01; primary. HTTP: 200; section xem ledger.

- [BrowserRouter, Routes and nested routes](https://reactrouter.com/start/declarative/routing) — W18-C02; primary. HTTP: 200; section xem ledger.

- [Response status and cancellation](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch) — W18-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-18-quiz.md) · [Đáp án](../../assessments/answer-keys/week-18-answers.md) · [Flashcards](../../flashcards/week-18.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-17.md)

[Tuần tiếp](week-19.md)
