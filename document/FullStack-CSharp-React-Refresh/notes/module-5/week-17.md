# Tuần 17 — TypeScript, components và state

Chặng 5; 3 bài L17-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W17-01: Phân biệt compile-time types với runtime validation.

- LO-W17-02: Chia UI theo nhiệm vụ và dùng keys ổn định.

- LO-W17-03: Cập nhật immutable và tránh state trùng lặp.

Tiên quyết: Đạt gate tuần 16; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-17-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w17-01"></a>

## Bài 1: Kiểu dữ liệu tại boundary

LO-W17-01 · W17-C01

TypeScript bổ sung kiểm tra tĩnh cho JavaScript; kiểu bị xóa khi build. ExpenseDto mô tả id string, amountVnd number, spentOn string, version string. JSON từ fetch vẫn cần kiểm tra: unknown buộc narrow qua typeof, Array.isArray và field guards; 'as ExpenseDto' chỉ bảo compiler tin mình. Bật strict; tránh any để không mất kiểm tra null. Union status loading/error/success tạo các nhánh hợp lệ; discriminant status giúp narrow. Optional note cần quyết định undefined/null tại API boundary. Không chuyển DateOnly thành local Date rồi toISOString vì timezone có thể đổi ngày.

<a id="lo-w17-02"></a>

## Bài 2: Components, props và identity

LO-W17-02 · W17-C02

Component là hàm trả JSX từ props/state. Đặt ExpenseList, ExpenseRow, ExpenseForm ở cấp module; khai báo component trong parent tạo identity mới mỗi render. Props truyền dữ liệu xuống, callbacks truyền intent lên; không mutate props. List dùng key=id, không index khi có sort/delete vì state input có thể gắn sang item khác. JSX text được escape; không đưa note vào dangerouslySetInnerHTML. Fragment nhóm mà không thêm layout div. Type props gồm readonly data và callback có chữ ký rõ; CSS responsive từ tuần 1 vẫn dùng trong React.

<a id="lo-w17-03"></a>

## Bài 3: State và derived values

LO-W17-03 · W17-C03

State là snapshot của lần render; setState yêu cầu render mới, không sửa biến hiện tại. Khi next phụ thuộc previous, dùng functional updater setRows(old => old.filter(...)). Array/object state không push hay sửa nested field trực tiếp: tạo array/object mới cho phần thay đổi. Tổng chi tiêu derive bằng reduce từ rows thay vì lưu total riêng rồi quên đồng bộ. Chọn parent chung sở hữu filter chia sẻ; form input thường local. Chỉ thêm reducer/context khi có nhiều transitions hoặc nhu cầu chia sẻ rõ; Redux không phải điều kiện bắt đầu. Hooks gọi cùng thứ tự, ở top level component/custom hook.

## Ví dụ giải từng bước

1. Tạo client bằng npm create vite@latest web -- --template react-ts; npm install rồi npm run dev. Ghi phiên bản thực tế vào docs/toolchain.md và giữ lockfile.
2. Trong src/App.tsx dùng fixture nội bộ:
```tsx
import { useState } from 'react';
type Row = { id: string; amountVnd: number };
const initial: Row[] = [
 {id:'e1',amountVnd:50000}, {id:'e2',amountVnd:20000},
 {id:'e3',amountVnd:30000}
];
export default function App() {
 const [rows,setRows] = useState(initial);
 const total = rows.reduce((sum,r)=>sum+r.amountVnd,0);
 return <main><h1>Expenses</h1><p>Total: {total} VND</p>
 <ul>{rows.map(r=><li key={r.id}>{r.amountVnd}
 <button onClick={()=>setRows(old=>old.filter(x=>x.id!==r.id))}>
 Delete {r.id}</button></li>)}</ul></main>;
}
```
3. Ban đầu total=100000; xóa e2 còn80000. Fixture IDs e1..e3 chỉ dùng UI lab, API thực dùng GUID.
4. Chạy npm run build; đổi thứ tự list rồi xóa để kiểm keys.

## Đào sâu và lỗi thường gặp

Đừng chuyển nguyên DOM app bằng cách document.querySelector rồi sửa element trong React render. State drives view; event handler thay state. Type guard cần kiểm giá trị số nguyên an toàn và date format, không chỉ typeof number. Quy tắc giới hạn amount đến1e9 là project policy.

## Bài tập thực hành

### Bài tập 1

Viết guard amountVnd cho JSON string '50000', 50000, NaN, -1.

### Bài tập 2

Chứng minh tại sao lưu rows và total ở hai state dễ lỗi.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Number.isSafeInteger(v) && v>=1 && v<=1e9 sau typeof v==='number'. Chỉ 50000 hợp lệ; parser form string xử lý riêng trước guard DTO.

### Hướng giải 2

Xóa item rồi cập nhật rows mà quên total; UI lệch. Derive reduce từ rows, assert xóa20000 làm100000→80000.

## Project task

Thay UI DOM bằng React giữ fixture và responsive layout; thêm typed list, delete local và category filter. Chưa gọi network ở render.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w17.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Build strict TypeScript không lỗi; guard từ chối string/negative/unsafe amount.

- [ ] Seed total100000, xóa Travel còn80000; sort/delete không đổi identity input.

- [ ] Note chứa <script> hiển thị text; keyboard thao tác delete được.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Giải thích: 'Why does TypeScript not validate a server response?' Thu 2 phút với một ví dụ unknown.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Narrowing and unknown guards](https://www.typescriptlang.org/docs/handbook/2/narrowing.html) — W17-C01; primary. HTTP: 200; section xem ledger.

- [Keys and rendering lists](https://react.dev/learn/rendering-lists) — W17-C02; primary. HTTP: 200; section xem ledger.

- [Immutable array updates](https://react.dev/learn/updating-arrays-in-state) — W17-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-17-quiz.md) · [Đáp án](../../assessments/answer-keys/week-17-answers.md) · [Flashcards](../../flashcards/week-17.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](../module-4/week-16.md)

[Tuần tiếp](week-18.md)
