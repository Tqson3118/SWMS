namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;

/// <summary>
/// Safe theory content upgrade for 18 core expansion lessons (Coding, Investing, Psychology).
/// Uses marker &lt;!-- studxy-writein-v2 --&gt; (and &lt;!-- studxy-writein-v3 --&gt; for Coding JS)
/// and strictly upgrades ONLY when CurrentVersion == 1.
/// NEVER overwrites user-edited lessons (CurrentVersion &gt; 1).
/// </summary>
public static class WriteInTheoryExpansion
{
    public const string UpgradeMarker = "<!-- studxy-writein-v2 -->";
    public const string CodingUpgradeMarkerV3 = "<!-- studxy-writein-v3 -->";

    public record LessonTheoryMeta(
        Guid LessonId,
        string Title,
        string Slug,
        int EstimatedMinutes,
        string Markdown,
        string Summary1,
        string Summary2
    );

    public static List<LessonTheoryMeta> GetUpgradedLessons()
    {
        return new List<LessonTheoryMeta>
        {
            // =========================================================================
            // 1. CODING LESSONS (1.1 .. 3.2) — v3 Beginner-Friendly Deep Expansion
            // =========================================================================
            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson11Id,
                "Bài 1.1: Biến Đổi & Lọc Dữ Liệu Nâng Cao (Map, Filter, Reduce)",
                "bien-doi-va-loc-du-lieu-mang",
                40,
                @"# Bài 1.1: Biến Đổi & Lọc Dữ Liệu Nâng Cao (Map, Filter, Reduce)

> [!TIP]
> Nếu bạn là người mới bắt đầu học JavaScript, đừng lo lắng khi thấy các từ khóa như `map`, `filter`, `reduce` hay `Immutability`. Hãy đọc chậm **Phần 0** để nắm chắc cách biến và vòng lặp hoạt động trước khi bước sang cách viết hiện đại.

---

## 🌱 Phần 0 — Nhập môn từ con số 0 (Dành cho người mới hoàn toàn)

### 0.1. Biến, Mảng `[]` và Chỉ số `0, 1, 2` là gì?
- **Biến (`let`, `const`)** giống như một chiếc hộp có dán nhãn để cất giữ dữ liệu. Dùng `const` khi bạn không muốn gán lại chiếc hộp sang giá trị khác, dùng `let` khi giá trị trong hộp sẽ thay đổi (ví dụ biến đếm `i` hoặc biến tổng `sum`).
- **Mảng (Array)** được viết bằng cặp dấu ngoặc vuông `[...]`, giống như một **dãy tủ đánh số từ 0**:
  ```javascript
  const scores = [7, 9, 5];
  // Chỉ số (index): 0 -> 7 | 1 -> 9 | 2 -> 5
  // Độ dài mảng: scores.length === 3
  ```
  > [!IMPORTANT]
  > Máy tính đếm vị trí từ **0**, không phải từ 1! Phần tử đầu tiên là `scores[0]`, phần tử cuối cùng là `scores[scores.length - 1]`.

### 0.2. Hàm mũi tên (`x => ...`) cho người mới
Thay vì viết dài dòng:
```javascript
function double(x) {
  return x * 2;
}
```
JavaScript hiện đại cho phép viết gọn thành **hàm mũi tên (Arrow Function)**:
```javascript
const double = (x) => x * 2;
// Đọc là: ""Nhận đầu vào x, trả về x * 2""
```
Khi truyền vào hàm khác (làm callback), ta viết trực tiếp `x => x * 2` cho ngắn gọn và dễ đọc.

### 0.3. Từ vòng lặp `for` truyền thống đến `map` / `filter` / `reduce`
Giả sử ta có mảng `[10, 20, 30]` và muốn tính tổng tất cả các số. Bằng vòng lặp `for` truyền thống:
```javascript
const nums = [10, 20, 30];
let sum = 0; // Khởi tạo hộp chứa tổng ban đầu = 0

for (let i = 0; i < nums.length; i++) {
  sum = sum + nums[i]; // Lấy phần tử thứ i cộng vào sum
}
console.log(sum); // 60
```

#### Bảng theo dõi từng bước (Step-by-step Trace Table) & Sơ đồ ASCII
```
Mảng nums: [ 10,  20,  30 ]
Chỉ số i :    0    1    2
              │    │    └─ Lượt 3: sum = 30 + 30 = 60
              │    └────── Lượt 2: sum = 10 + 20 = 30
              └─────────── Lượt 1: sum =  0 + 10 = 10
```

| Bước lặp | Chỉ số `i` | Điều kiện `i < 3` | Giá trị `nums[i]` | `sum` trước cộng | `sum` sau cộng |
|---|---|---|---|---|---|
| Khởi tạo | `0` | Chưa chạy | - | `0` | `0` |
| Lượt 1 | `0` | `0 < 3` (Đúng) | `10` | `0` | `0 + 10 = 10` |
| Lượt 2 | `1` | `1 < 3` (Đúng) | `20` | `10` | `10 + 20 = 30` |
| Lượt 3 | `2` | `2 < 3` (Đúng) | `30` | `30` | `30 + 30 = 60` |
| Kết thúc | `3` | `3 < 3` (Sai -> Dừng) | - | `60` | Trả về `60` |

Tương tự, nếu muốn **nhân đôi từng phần tử** hay **lọc số lớn hơn 15**, bằng vòng `for` bạn luôn phải: (1) tạo mảng rỗng `const result = []`, (2) tự viết `for (let i = 0; ...)`, (3) tự `result.push(...)`. Bộ ba `map`, `filter`, `reduce` sinh ra để làm thay 3 bước lặp nhàm chán này một cách an toàn và rõ ý định hơn!

---

## 📘 Phần 1 & 2 — Lý thuyết cốt lõi đi từ Dễ đến Khó

### 1.1. Vì sao cần Tính Bất Biến (Immutability) trong dự án thực tế?
Khi bạn làm ứng dụng React, Vue hay Backend API:
- Nếu một hàm nhận vào mảng `cartItems` và sửa trực tiếp nó (`cartItems.push(...)` hoặc `cartItems.sort()`), mọi màn hình khác đang dùng chung biến đó sẽ bị thay đổi dữ liệu ngầm mà không hề hay biết!
- Đặc biệt trong React, giao diện **chỉ vẽ lại (re-render)** khi nhận được một **mảng/đối tượng mới** ở địa chỉ ô nhớ mới. Nếu bạn sửa mảng cũ tại chỗ, giao diện sẽ đứng yên không cập nhật.
- **Giải pháp:** Các hàm `filter`, `map`, `reduce` luôn giữ nguyên mảng ban đầu và trả về kết quả mới.

### 1.2. Cơ chế Bộ Ba Phương thức Mảng (Đi từ Dễ đến Khó)
1. **Bước 1 — `filter(predicate)` (Cái rây lọc dữ liệu):**
   - Duyệt từng phần tử; nếu hàm kiểm tra trả về `true`, phần tử được giữ lại trong mảng mới.
   - Độ dài mảng kết quả **nhỏ hơn hoặc bằng** mảng gốc; hình dạng phần tử không đổi.
   - Ví dụ tư duy: `[1, 2, 3, 4].filter(n => n % 2 === 0)` $\rightarrow$ `[2, 4]`.
2. **Bước 2 — `map(transform)` (Dây chuyền biến đổi 1-1):**
   - Biến đổi từng phần tử thành một giá trị mới theo công thức bạn cung cấp.
   - Độ dài mảng kết quả **luôn bằng đúng 100%** mảng gốc.
   - Ví dụ tư duy: `[1, 2, 3].map(n => n * 10)` $\rightarrow$ `[10, 20, 30]`.
3. **Bước 3 — `reduce((acc, item) => ..., initialValue)` (Máy nén dữ liệu đa năng):**
   - Nén cả mảng thành **1 kết quả duy nhất**: có thể là tổng tiền (số), chuỗi HTML, hay một object thống kê.
   - `acc` (accumulator) là chiếc hộp tích lũy kết quả qua từng lượt.
   - **Quy tắc vàng sống còn:** Luôn truyền `initialValue` (ví dụ `0`, `[]`, `{}`) ở tham số thứ 2 để chương trình không bị sập khi gặp mảng rỗng `[]`.

### 2.1. Nối chuỗi Pipeline trong thực tế (Chaining)
Trong dự án thật, dữ liệu từ API thường cần: **Lọc bỏ rác $\rightarrow$ Biến đổi định dạng $\rightarrow$ Tổng hợp số liệu**.
```
[Mảng gốc] ──(.filter)──> [Mảng sạch] ──(.map)──> [Mảng đã đổi dạng] ──(.reduce)──> [1 Kết quả]
```
Mỗi bước không làm hỏng dữ liệu của bước trước, giúp code dễ đọc như tiếng Anh tự nhiên.

---

## 🔍 Phần 3 — 3 Ví dụ Mẫu Giải Chậm Từng Dòng (Worked Examples)

### Ví dụ 1 (Mức Dễ - Làm quen): Nhân đôi điểm thưởng học viên
**Bài toán:** Cho mảng điểm `[5, 8, 10]`, tạo mảng mới với mỗi điểm được nhân đôi.
```javascript
const points = [5, 8, 10];              // Dòng 1: Mảng gốc gồm 3 phần tử
const doubled = points.map((p) => {     // Dòng 2: map đi qua từng phần tử, đặt tên tạm là p
  return p * 2;                         // Dòng 3: Trả về giá trị mới p * 2 cho vị trí tương ứng
});                                     // Dòng 4: Kết thúc map, gom các giá trị mới thành mảng
console.log(doubled);                   // Dòng 5: In ra [10, 16, 20]; mảng points vẫn là [5, 8, 10]
```

### Ví dụ 2 (Mức Trung bình - Bài toán thực tế): Tính tổng tiền giỏ hàng còn hàng
**Bài toán:** Trong giỏ hàng có sản phẩm hết hàng (`inStock: false`). Chỉ tính tổng tiền các món còn hàng sau khi nhân số lượng.
```javascript
const cart = [
  { name: 'Bàn phím cơ', price: 1200000, qty: 1, inStock: true },
  { name: 'Chuột gaming', price: 650000, qty: 2, inStock: false },
  { name: 'Lót chuột', price: 150000, qty: 2, inStock: true }
];

// Bước 1: Lọc lấy món còn hàng -> còn Bàn phím cơ và Lót chuột
const availableItems = cart.filter((item) => item.inStock === true);

// Bước 2: Biến đổi mỗi món thành thành tiền (price * qty) -> [1200000, 300000]
const subtotals = availableItems.map((item) => item.price * item.qty);

// Bước 3: Cộng dồn từ mốc khởi tạo 0 -> 1500000
const totalPayment = subtotals.reduce((sum, money) => sum + money, 0);

console.log(totalPayment); // 1500000
```
**Trace ngắn:** `availableItems` có 2 món $\rightarrow$ `subtotals` là `[1200000, 300000]` $\rightarrow$ `reduce` lượt 1: `0 + 1200000 = 1200000`; lượt 2: `1200000 + 300000 = 1500000`.

### Ví dụ 3 (Mức Nâng cao + Bẫy hay sai): Mảng rỗng, `undefined` & Đột biến ngầm
Xem xét đoạn code chứa **3 lỗi kinh điển** mà người mới hay mắc:
```javascript
// CODE LỖI CẦN TRÁNH:
function buggySummary(orders) {
  // BẪY 1: .sort() sửa thẳng mảng orders gốc!
  const sorted = orders.sort((a, b) => b.amount - a.amount);

  // BẪY 2: Khi orders = [], thiếu initialValue 0 -> văng TypeError: Reduce of empty array!
  // Hoặc khi orders = [{ amount: 500 }], reduce trả về nguyên OBJECT { amount: 500 } chứ không phải số 500!
  const total = sorted.reduce((acc, o) => acc + o.amount);
  return total;
}

// CODE CHUẨN AN TOÀN:
function safeSummary(orders) {
  if (!Array.isArray(orders) || orders.length === 0) return { topAmount: 0, total: 0 };

  // Copy mảng bằng [...orders] trước khi sort để bảo toàn mảng gốc
  const sorted = [...orders].sort((a, b) => b.amount - a.amount);
  // Luôn truyền initialValue = 0
  const total = sorted.reduce((acc, o) => acc + (o.amount || 0), 0);

  return { topAmount: sorted[0].amount, total };
}
```
> [!WARNING]
> **Bẫy `find()` vs `filter()`:** Nếu bạn dùng `orders.find(o => o.id === 99)` mà không thấy, kết quả là `undefined`. Đọc `result.amount` ngay sau đó sẽ crash (`Cannot read properties of undefined`). Luôn kiểm tra `if (!found)` trước khi dùng!

---

## 📝 Phần 4 — Bộ Bài Tập Tự Luyện & Đề Tự Giải

### A. 4 Bài tập tự viết code (Từ Rất Dễ đến Vận dụng)

1. **Bài 1 (Rất Dễ):** Viết hàm `getEvenNumbers(nums)` nhận vào mảng số nguyên và trả về mảng chỉ chứa các số chẵn (chia hết cho 2).
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Số chẵn có phép chia lấy dư `n % 2 === 0`. Hãy dùng phương thức `.filter(...)`.

```javascript
function getEvenNumbers(nums) {
  return nums.filter((n) => n % 2 === 0);
}
// Kiểm tra: getEvenNumbers([1, 2, 3, 4, 6]) -> [2, 4, 6]
// Mảng rỗng: getEvenNumbers([]) -> []
```
</details>

2. **Bài 2 (Dễ):** Viết hàm `formatUserTags(names)` biến mảng tên `['An', 'Bình']` thành mảng chuỗi có tiền tố `['@An', '@Bình']`.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Mỗi phần tử giữ nguyên vị trí nhưng thay đổi hình dạng $\rightarrow$ dùng `.map(...)`.

```javascript
function formatUserTags(names) {
  return names.map((name) => '@' + name);
}
// Kiểm tra: formatUserTags(['An', 'Bình']) -> ['@An', '@Bình']
```
</details>

3. **Bài 3 (Trung bình):** Cho danh sách sinh viên `[{ name: 'Lan', score: 8 }, { name: 'Huy', score: 4 }, { name: 'Mai', score: 9 }]`. Viết hàm `getPassedNames(students)` trả về mảng **tên** của các bạn có `score >= 5`.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Kết hợp 2 bước: bước 1 dùng `filter` giữ người đạt `score >= 5`, bước 2 dùng `map` chỉ lấy `.name`.

```javascript
function getPassedNames(students) {
  return students
    .filter((s) => s.score >= 5)
    .map((s) => s.name);
}
// Kiểm tra: trả về ['Lan', 'Mai']
```
</details>

4. **Bài 4 (Vận dụng):** Cho mảng giao dịch `[{ category: 'food', amount: 40 }, { category: 'book', amount: 100 }, { category: 'food', amount: 60 }]`. Viết hàm `sumByCategory(txs)` gom nhóm tổng tiền theo từng `category` thành `{ food: 100, book: 100 }`.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Dùng `reduce` với `initialValue` là `{}`. Với mỗi phần tử, lấy giá trị cũ `(acc[tx.category] || 0)` cộng thêm `tx.amount`.

```javascript
function sumByCategory(txs) {
  return txs.reduce((acc, tx) => {
    acc[tx.category] = (acc[tx.category] || 0) + tx.amount;
    return acc;
  }, {});
}
```
</details>

### B. 3 Câu hỏi tự kiểm tra tư duy

1. **Câu hỏi 1:** Điều gì xảy ra khi chạy `[].reduce((acc, x) => acc + x)` (không có tham số thứ 2)?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** JavaScript ném lỗi `TypeError: Reduce of empty array with no initial value` khiến ứng dụng dừng đột ngột. **Cách sửa:** Luôn truyền `0` làm tham số thứ 2: `[].reduce((acc, x) => acc + x, 0)` để trả về `0` an toàn.
</details>

2. **Câu hỏi 2:** Tại sao `const res = arr.map(item => { item.done = true; return item; })` vẫn bị coi là vi phạm tính bất biến?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Tuy `.map()` tạo mảng mới, nhưng bên trong callback bạn sửa trực tiếp thuộc tính `.done` trên chính object gốc trong bộ nhớ Heap. Do đó mảng `arr` ban đầu cũng bị biến đổi theo! **Cách sửa:** Trả về object mới bằng spread `{ ...item, done: true }`.
</details>

3. **Câu hỏi 3:** Khi nào nên dùng `forEach` thay vì `map`, và khi nào tuyệt đối dùng `map`?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** `forEach` luôn trả về `undefined`, chỉ dùng khi bạn muốn thực hiện hành động phụ (như in log ra màn hình). Khi bạn cần tạo ra một mảng dữ liệu mới từ mảng cũ, luôn dùng `map`.
</details>

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Nhập môn biến, mảng, vòng lặp for đến map/filter/reduce chuẩn Immutability.",
                "Trace từng bước, 3 ví dụ chậm và bộ đề tự giải phòng bẫy mảng rỗng."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson12Id,
                "Bài 1.2: Thuật Toán Tìm Kiếm & Kỹ Thuật Hai Con Trỏ (Two Pointers)",
                "thuat-toan-tim-kiem-va-two-pointers",
                40,
                @"# Bài 1.2: Thuật Toán Tìm Kiếm & Kỹ Thuật Hai Con Trỏ (Two Pointers)

> [!TIP]
> Nghe tên ""Tìm kiếm nhị phân"" hay ""Hai con trỏ"" có vẻ hàn lâm, nhưng thực chất chúng chỉ là 2 biến đếm vị trí (`left`, `right`) di chuyển thông minh trên mảng. Hãy đi từng bước từ **Phần 0** để hình dung rõ ràng nhất!

---

## 🌱 Phần 0 — Nhập môn từ con số 0 (Dành cho người mới hoàn toàn)

### 0.1. Con trỏ trong JavaScript chỉ là ""Ngón tay chỉ vào số thứ tự""
Đừng nhầm với con trỏ bộ nhớ phức tạp trong ngôn ngữ C/C++. Trong JavaScript, khi nói **hai con trỏ (Two Pointers)**, nghĩa là bạn chỉ tạo **2 biến số nguyên** để nhớ vị trí trong mảng:
```javascript
const arr = [10, 20, 30, 40, 50];
let left = 0;               // Ngón tay trái đặt ở đầu mảng (giá trị 10)
let right = arr.length - 1; // Ngón tay phải đặt ở cuối mảng (giá trị 50)
```

### 0.2. Tìm tuần tự (lật từng trang) vs Tìm nhị phân (chia đôi cuốn sách)
Hãy tưởng tượng bạn cần tìm từ **""Mèo""** trong cuốn từ điển 1.000 trang đã xếp theo thứ tự A $\rightarrow$ Z:
1. **Tìm kiếm tuyến tính (Linear Search):** Mở từ trang 1, lật từng trang 2, 3, 4... Nếu từ ở trang 999, bạn mất 999 bước ($O(n)$).
2. **Tìm kiếm nhị phân (Binary Search):** Mở ngay **giữa sách (trang 500)**. Nếu thấy chữ ""K"", biết chắc chữ ""Mèo"" nằm ở nửa sau (trang 501–1000) nên **vứt bỏ hoàn toàn nửa đầu**! Lần 2 mở giữa nửa sau (trang 750)... Chỉ mất tối đa $\approx 10$ lần mở sách cho 1.000 trang, và chỉ **20 lần** cho **1.000.000 bản ghi** ($O(\log n)$)!

### 0.3. Phép lấy vị trí giữa `Math.floor`
Nếu `left = 0` và `right = 3`, phép chia `(0 + 3) / 2` ra `1.5`, nhưng chỉ số mảng phải là số nguyên! Vì vậy ta dùng `Math.floor((left + right) / 2)` để làm tròn xuống thành `1`.

#### Bảng theo dõi từng bước (Trace Table) — Tìm số `12` trong `[2, 5, 8, 12, 19]`
```
Chỉ số :    0    1    2    3    4
Mảng   : [  2,   5,   8,  12,  19 ]
Lượt 1 :  left=0      mid=2      right=4   --> arr[2] = 8 < 12  ==> bỏ nửa trái, left = mid + 1 = 3
Lượt 2 :                  left=3, mid=3, right=4 --> arr[3] = 12 === 12 ==> TÌM THẤY tại vị trí 3!
```

| Lượt lặp | `left` | `right` | `mid = Math.floor((l+r)/2)` | `arr[mid]` | So sánh với `target = 12` | Hành động tiếp theo |
|---|---|---|---|---|---|---|
| Lượt 1 | `0` | `4` | `2` | `8` | `8 < 12` (Nhỏ hơn) | Bỏ `0..2`, gán `left = 2 + 1 = 3` |
| Lượt 2 | `3` | `4` | `3` | `12` | `12 === 12` (Khớp!) | Trả về `3` và thoát hàm |

---

## 📘 Phần 1 & 2 — Lý thuyết cốt lõi đi từ Dễ đến Khó

### 1.1. Tìm kiếm Nhị phân (Binary Search) — Quy tắc bắt buộc
- **Điều kiện tiên quyết 1:** Mảng đầu vào **bắt buộc phải được sắp xếp** (tăng dần hoặc giảm dần). Nếu mảng lộn xộn `[9, 2, 7]`, Binary Search sẽ cho kết quả sai hoàn toàn.
- **Điều kiện dừng vòng lặp:** Dùng `while (left <= right)` (có dấu `=`). Lý do: khi không gian thu hẹp còn đúng 1 phần tử (`left === right`), ta vẫn phải kiểm tra nốt phần tử cuối cùng đó.
- **Bước nhảy an toàn:** Luôn cập nhật `left = mid + 1` hoặc `right = mid - 1` (không bao giờ gán `left = mid`), vì `arr[mid]` đã được kiểm tra rồi, giữ lại `mid` sẽ khiến vòng lặp quay vô tận.
- **Ứng dụng thực tế:** Tìm log lỗi theo mốc thời gian `timestamp` trong hàng triệu dòng log, tìm phiên bản gây lỗi phần mềm (`git bisect`), tìm khoảng giá sản phẩm trên sàn thương mại điện tử.

### 2.1. Kỹ thuật Hai Con Trỏ (Two Pointers Pattern)
Nếu bạn dùng 2 vòng `for` lồng nhau (`for i` bên trong có `for j`) để tìm cặp số, 10.000 phần tử sẽ tốn $10.000 \times 10.000 = 100.000.000$ phép tính ($O(n^2)$) khiến trình duyệt bị treo! Kỹ thuật Hai Con Trỏ giảm xuống chỉ **1 lần duyệt ($O(n)$)** và **không tốn thêm mảng phụ ($O(1)$ bộ nhớ)**:

1. **Mẫu A — Hai con trỏ đối xứng hai đầu (Opposite Direction):**
   - `left = 0` đi sang phải, `right = n - 1` đi sang trái, dừng khi `left >= right` (`while (left < right)`).
   - Dùng cho: Đảo ngược mảng tại chỗ, kiểm tra chuỗi đối xứng (Palindrome), tìm cặp số có tổng bằng `target` trên mảng đã sắp xếp (Two Sum II).
2. **Mẫu B — Hai con trỏ cùng chiều Nhanh - Chậm (Fast & Slow / Read & Write):**
   - `read` chạy nhanh phía trước để dò tìm phần tử hợp lệ; `write` chạy chậm phía sau để ghi đè phần tử sạch.
   - Dùng cho: Xóa phần tử trùng lặp trên mảng đã sắp xếp mà không cần tạo mảng mới hay `Set`.

---

## 🔍 Phần 3 — 3 Ví dụ Mẫu Giải Chậm Từng Dòng (Worked Examples)

### Ví dụ 1 (Mức Dễ - Làm quen): Đảo ngược mảng bằng hai con trỏ
**Bài toán:** Đảo ngược mảng `[1, 2, 3, 4]` trực tiếp mà không dùng `.reverse()`.
```javascript
function reverseArray(arr) {
  let left = 0;                   // Dòng 1: Đặt con trỏ trái ở phần tử đầu
  let right = arr.length - 1;     // Dòng 2: Đặt con trỏ phải ở phần tử cuối
  while (left < right) {          // Dòng 3: Chạy khi hai ngón tay chưa chạm nhau
    const temp = arr[left];       // Dòng 4: Cất tạm giá trị bên trái
    arr[left] = arr[right];       // Dòng 5: Chép giá trị bên phải sang trái
    arr[right] = temp;            // Dòng 6: Đưa giá trị tạm sang phải
    left++;                       // Dòng 7: Tiến ngón trái vào giữa
    right--;                      // Dòng 8: Lùi ngón phải vào giữa
  }
  return arr;                     // Dòng 9: Trả về [4, 3, 2, 1]
}
```

### Ví dụ 2 (Mức Trung bình - Bài toán thực tế): Tìm 2 món hàng đúng ngân sách
**Bài toán:** Bạn có voucher đúng `target = 100` nghìn và danh sách giá đã sắp xếp tăng dần `[20, 35, 65, 80, 95]`. Tìm vị trí 2 món có tổng đúng bằng `100`.
```javascript
function findTwoProducts(prices, target) {
  let left = 0;
  let right = prices.length - 1;

  while (left < right) {
    const sum = prices[left] + prices[right];
    if (sum === target) {
      return [left, right]; // Khớp ngân sách!
    }
    if (sum < target) {
      // Tổng đang nhỏ hơn 100 -> Cần giá lớn hơn -> Nhích con trỏ trái sang phải
      left++;
    } else {
      // Tổng đang lớn hơn 100 -> Cần giá nhỏ hơn -> Lùi con trỏ phải sang trái
      right--;
    }
  }
  return [-1, -1]; // Không có cặp nào phù hợp
}
// Trace: [20 + 95 = 115 > 100 -> right--] -> [20 + 80 = 100 === 100 -> trả về [0, 3]]!
```

### Ví dụ 3 (Mức Nâng cao + Bẫy hay sai): Bẫy Vòng Lặp Vô Hạn & Off-by-One
```javascript
// BẪY 1: Vòng lặp vô tận trong Binary Search!
// Khi arr = [10, 20], target = 20: left = 0, right = 1
// mid = Math.floor((0 + 1) / 2) = 0. arr[0] = 10 < 20.
// Nếu viết nhầm: left = mid (left vẫn là 0!) -> Lượt sau mid lại là 0 -> TREO VĨNH VIỄN!
// Sửa đúng: left = mid + 1;

// BẪY 2: Thiếu dấu '=' trong Binary Search (lỗi Off-by-One)
// Với mảng 1 phần tử [7], target = 7: left = 0, right = 0.
// Nếu viết while (left < right) -> 0 < 0 là SAI -> bỏ qua không kiểm tra arr[0] -> trả về -1!
// Sửa đúng: while (left <= right)

// BẪY 3: Mảng rỗng [] hoặc null
function safeBinarySearch(arr, target) {
  if (!Array.isArray(arr) || arr.length === 0) return -1;
  let left = 0;
  let right = arr.length - 1;
  while (left <= right) {
    const mid = left + Math.floor((right - left) / 2); // Tránh tràn số ở ngôn ngữ khác
    if (arr[mid] === target) return mid;
    if (arr[mid] < target) left = mid + 1;
    else right = mid - 1;
  }
  return -1;
}
```

---

## 📝 Phần 4 — Bộ Bài Tập Tự Luyện & Đề Tự Giải

### A. 4 Bài tập tự viết code (Từ Rất Dễ đến Vận dụng)

1. **Bài 1 (Rất Dễ):** Viết hàm tìm kiếm tuyến tính `findFirstIndex(arr, target)` bằng vòng lặp `for` thông thường, trả về chỉ số đầu tiên tìm thấy hoặc `-1`.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Duyệt `i` từ `0` đến `arr.length - 1`. Nếu `arr[i] === target` thì `return i;` ngay lập tức. Hết vòng lặp không thấy thì `return -1;`.

```javascript
function findFirstIndex(arr, target) {
  for (let i = 0; i < arr.length; i++) {
    if (arr[i] === target) return i;
  }
  return -1;
}
```
</details>

2. **Bài 2 (Dễ):** Viết hàm `isSymmetricNumbers(nums)` kiểm tra xem mảng số có đối xứng từ hai đầu hay không (ví dụ `[1, 2, 3, 2, 1]` $\rightarrow$ `true`, `[1, 2, 3]` $\rightarrow$ `false`).
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Đặt `left = 0`, `right = nums.length - 1`. Dùng `while (left < right)`, nếu `nums[left] !== nums[right]` thì trả về `false`.

```javascript
function isSymmetricNumbers(nums) {
  let left = 0;
  let right = nums.length - 1;
  while (left < right) {
    if (nums[left] !== nums[right]) return false;
    left++;
    right--;
  }
  return true;
}
```
</details>

3. **Bài 3 (Trung bình):** Viết hàm `countPairsLessThan(sortedNums, limit)` đếm xem có bao nhiêu cặp phần tử `(i, j)` trong mảng đã sắp xếp có tổng nhỏ hơn `limit` với độ phức tạp $O(n)$.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Nếu `sortedNums[left] + sortedNums[right] < limit`, vì mảng tăng dần nên mọi phần tử nằm giữa `left` và `right` ghép với `left` cũng đều nhỏ hơn `limit`! Có ngay `(right - left)` cặp hợp lệ, rồi tăng `left++`.

```javascript
function countPairsLessThan(sortedNums, limit) {
  let left = 0;
  let right = sortedNums.length - 1;
  let count = 0;
  while (left < right) {
    if (sortedNums[left] + sortedNums[right] < limit) {
      count += right - left;
      left++;
    } else {
      right--;
    }
  }
  return count;
}
```
</details>

4. **Bài 4 (Vận dụng):** Cho mảng tăng dần có phần tử trùng `[1, 1, 2, 2, 2, 3]`. Viết hàm `removeDuplicatesInPlace(nums)` loại bỏ trùng lặp tại chỗ bằng con trỏ `read` và `write`, trả về số lượng phần tử độc nhất.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Nếu mảng rỗng trả về `0`. Khởi tạo `write = 1`. Cho `read` chạy từ `1` tới cuối, nếu `nums[read] !== nums[read - 1]` thì chép `nums[write] = nums[read]` và tăng `write++`.

```javascript
function removeDuplicatesInPlace(nums) {
  if (nums.length === 0) return 0;
  let write = 1;
  for (let read = 1; read < nums.length; read++) {
    if (nums[read] !== nums[read - 1]) {
      nums[write] = nums[read];
      write++;
    }
  }
  return write;
}
```
</details>

### B. 3 Câu hỏi tự kiểm tra tư duy

1. **Câu hỏi 1:** Nếu chạy Binary Search trên mảng `[10, 3, 25, 8]` (chưa sắp xếp) để tìm `3`, điều gì sẽ xảy ra?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** `mid = 1` (tùy độ dài; ở đây `left=0, right=3, mid=1 -> arr[1]=3` tình cờ trúng), nhưng nếu tìm `25`: `arr[1] = 3 < 25` đi sang phải `left=2, mid=2 -> arr[2]=25`. Tuy nhiên nếu tìm `10`: `arr[1] = 3 < 10`, thuật toán sẽ bỏ nửa trái (nơi chứa số `10`!) và báo không tìm thấy. Binary Search **bắt buộc** mảng phải có thứ tự đơn điệu.
</details>

2. **Câu hỏi 2:** Vì sao thuật toán đảo mảng dùng `while (left < right)` còn Binary Search dùng `while (left <= right)`?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Khi đảo mảng và gặp nhau tại tâm (`left === right`), phần tử ở chính giữa không cần đổi chỗ với chính nó nên dùng `<`. Còn Binary Search phải kiểm tra xem phần tử cuối cùng tại `left === right` có bằng `target` không nên phải dùng `<=`.
</details>

3. **Câu hỏi 3:** Một mảng có 1 tỷ phần tử đã sắp xếp. Binary Search mất tối đa khoảng bao nhiêu bước so sánh?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Vì $2^{30} \approx 1.07 \times 10^9$, thuật toán chỉ cần tối đa khoảng **30 lần chia đôi** để tìm ra kết quả trong 1 tỷ phần tử, thay vì 1 tỷ bước như vòng lặp thường!
</details>

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Nhập môn chỉ số mảng, tìm kiếm nhị phân O(log n) và kỹ thuật Hai Con Trỏ O(1).",
                "Bảng trace từng bước, ví dụ thực tế và phòng tránh vòng lặp vô hạn."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson21Id,
                "Bài 2.1: Kỹ Thuật Debounce & Throttle Xử Lý Sự Kiện Tần Suất Cao",
                "debounce-va-throttle-su-kien",
                40,
                @"# Bài 2.1: Kỹ Thuật Debounce & Throttle Xử Lý Sự Kiện Tần Suất Cao

> [!TIP]
> Để hiểu được `Debounce` và `Throttle`, bạn chỉ cần nắm 3 viên gạch nền tảng: **Hàm nhận hàm làm tham số (Callback)**, **Đồng hồ hẹn giờ `setTimeout`**, và **Chiếc ba lô trí nhớ (Closure)**. Phần 0 sẽ giải thích thật chậm cả 3 khái niệm này!

---

## 🌱 Phần 0 — Nhập môn từ con số 0 (Dành cho người mới hoàn toàn)

### 0.1. Bài toán thực tế: Tại sao gõ phím nhanh lại làm lag web?
Khi bạn gõ chữ `""laptop""` vào thanh tìm kiếm Google/Shopee (6 ký tự trong 1 giây):
- Nếu cứ mỗi lần gõ 1 chữ lại gọi API ngay lập tức, trình duyệt sẽ bắn **6 request** liên tiếp (`""l""`, `""la""`, `""lap""`, `""lapt""`, `""lapto""`, `""laptop""`).
- 5 request đầu hoàn toàn lãng phí, thậm chí kết quả của `""la""` về chậm hơn `""laptop""` còn làm nhảy sai danh sách gợi ý!

### 0.2. Đồng hồ hẹn giờ `setTimeout` & Nút hủy `clearTimeout`
Trong JavaScript:
- `const id = setTimeout(hamChaySau, 300)` giống như bạn **đặt báo thức sau 300ms** (0.3 giây) sẽ chạy `hamChaySau`. Nó trả về một mã vé `id`.
- `clearTimeout(id)` giống như bạn **tắt báo thức trước khi nó kịp reo**. Nếu bạn gọi `clearTimeout(id)` ở phút chót, `hamChaySau` sẽ **không bao giờ chạy**!

### 0.3. Closure (Bao đóng) — ""Chiếc ba lô bí mật của hàm""
Thông thường, biến tạo bên trong hàm sẽ biến mất khi hàm chạy xong. Nhưng nếu hàm cha trả về một hàm con, và hàm con có dùng biến của hàm cha, thì JavaScript giữ biến đó lại trong một ""chiếc ba lô"" gắn theo hàm con:
```javascript
function makeCounter() {
  let count = 0; // Biến nằm trong ba lô Closure
  return function() {
    count = count + 1;
    return count;
  };
}
const clickBtn = makeCounter();
console.log(clickBtn()); // 1
console.log(clickBtn()); // 2 -> Biến count vẫn nhớ giá trị cũ!
```
Chính nhờ Closure này mà `debounce` và `throttle` nhớ được biến `timerId` qua nhiều lần người dùng gõ phím!

#### Sơ đồ dòng thời gian ASCII & Bảng Trace `debounce(search, 300)`
```
Thời gian (ms): 0ms        100ms       200ms                     500ms
Người dùng    : Gõ ""j""     Gõ ""ja""    Gõ ""jav""                  (ngừng gõ)
Hành động     : Đặt T1     Hủy T1      Hủy T2                    T3 reo chuông!
                           Đặt T2      Đặt T3                    ==> GỌI search(""jav"") 1 LẦN!
```

| Thời điểm | Sự kiện gõ | `timerId` cũ trước xử lý | Hành động với Timer | Kết quả gọi `search()` |
|---|---|---|---|---|
| `0ms` | Gõ `""j""` | `null` | Đặt báo thức `T1` (sẽ chạy ở `300ms`) | Chưa gọi |
| `100ms` | Gõ `""ja""` | `T1` | `clearTimeout(T1)` $\rightarrow$ Đặt `T2` (`400ms`) | Chưa gọi |
| `200ms` | Gõ `""jav""` | `T2` | `clearTimeout(T2)` $\rightarrow$ Đặt `T3` (`500ms`) | Chưa gọi |
| `500ms` | Im lặng đủ `300ms` | `T3` | Báo thức `T3` kích hoạt | Gọi `search(""jav"")` 1 lần! |

---

## 📘 Phần 1 & 2 — Lý thuyết cốt lõi đi từ Dễ đến Khó

### 1.1. Debounce (Gom nhóm — ""Chờ nói hết câu mới trả lời"")
- **Nguyên lý:** Mỗi khi sự kiện mới tới, ta hủy timer cũ đang đếm dở và bắt đầu đếm lại từ đầu `delay` ms. Hàm gốc chỉ chạy **1 lần duy nhất** sau khi người dùng **ngừng thao tác** đủ khoảng thời gian `delay`.
- **Dùng trong dự án thực tế khi nào?**
  - Thanh tìm kiếm Live Search / Autocomplete (chờ người dùng gõ xong từ khóa mới gọi API).
  - Tự động lưu nháp bài viết (Auto-save draft sau khi ngừng gõ 1 giây).
  - Kiểm tra email/username trùng lặp khi điền form đăng ký.

### 1.2. Throttle (Tiết chế — ""Mỗi chu kỳ chỉ mở cửa 1 lần"")
```
Sự kiện liên tục: |||||||||||||||||||||||
Debounce (300ms): -----------------------[Chạy 1 lần cuối]
Throttle (300ms): [Lần 1]-----[Lần 2]-----[Lần 3]--------
```
- **Nguyên lý:** Dù sự kiện xảy ra 100 lần mỗi giây, hàm gốc chỉ được chạy **tối đa 1 lần trong mỗi khoảng `limit` ms** theo nhịp đều đặn.
- **Dùng trong dự án thực tế khi nào?**
  - Sự kiện cuộn trang `window.onscroll` để hiện nút ""Lên đầu trang"" hoặc tải thêm bài (Infinite Scroll).
  - Kéo thả chuột (`mousemove`), thay đổi kích thước cửa sổ (`resize`).
  - Nút ""Thanh toán"" hoặc ""Gửi đơn"" để chống người dùng bấm đúp liên hoàn tạo nhiều đơn trùng.

### 2.1. Bảo toàn tham số `...args`, ngữ cảnh `this` và `.cancel()`
1. **`...args` (Rest Parameters):** Gom toàn bộ tham số được truyền vào (như `event`, `keyword`, `page`) thành một mảng.
2. **`fn.apply(this, args)`:** Gọi hàm `fn` với đúng đối tượng `this` ban đầu (ví dụ thẻ `<input>`) và trải toàn bộ mảng `args` thành các tham số rời.
3. **Phương thức `.cancel()`:** Khi người dùng chuyển sang trang khác (component unmount trong React), timer đang chờ có thể chạy nhầm trên màn hình đã đóng, gây lỗi hoặc rò rỉ bộ nhớ (Memory Leak). Luôn gắn thêm `.cancel()` để gọi `clearTimeout` khi dọn dẹp!

---

## 🔍 Phần 3 — 3 Ví dụ Mẫu Giải Chậm Từng Dòng (Worked Examples)

### Ví dụ 1 (Mức Dễ - Làm quen): Hàm kiểm soát chỉ chạy 1 lần trong chu kỳ bằng cờ khóa
**Bài toán:** Viết phiên bản `throttle` tối giản bằng một biến cờ `isLocked` (boolean).
```javascript
function simpleThrottle(fn, limit) {
  let isLocked = false;                   // Dòng 1: Ban đầu cửa đang mở (chưa bị khóa)
  return function(...args) {              // Dòng 2: Trả về hàm bọc nhận mọi tham số
    if (isLocked) return;                 // Dòng 3: Nếu đang khóa thì bỏ qua luôn, thoát hàm
    fn.apply(this, args);                 // Dòng 4: Thực thi hàm gốc ngay lập tức
    isLocked = true;                      // Dòng 5: Khóa cửa lại trong thời gian chờ
    setTimeout(() => {                    // Dòng 6: Hẹn giờ sau limit ms sẽ mở lại cửa
      isLocked = false;
    }, limit);
  };
}
```

### Ví dụ 2 (Mức Trung bình - Bài toán thực tế): Hàm `debounce` chuẩn chỉnh có `.cancel()`
```javascript
function debounce(fn, delay) {
  let timerId = null;

  const debounced = function(...args) {
    // Nếu còn hẹn giờ cũ chưa chạy thì hủy bỏ ngay
    if (timerId !== null) {
      clearTimeout(timerId);
    }
    // Đặt lại hẹn giờ mới từ đầu
    timerId = setTimeout(() => {
      fn.apply(this, args);
      timerId = null; // Đặt lại trạng thái sạch sau khi đã chạy
    }, delay);
  };

  // Hàm dọn dẹp thủ công để dùng trong cleanup (VD: useEffect return)
  debounced.cancel = function() {
    if (timerId !== null) {
      clearTimeout(timerId);
      timerId = null;
    }
  };

  return debounced;
}
```

### Ví dụ 3 (Mức Nâng cao + Bẫy hay sai): 3 Lỗi làm Debounce/Throttle vô tác dụng
```javascript
// BẪY 1: Quên clearTimeout(timer)
function brokenDebounce(fn, delay) {
  let timer;
  return function(...args) {
    // LỖI: Không hủy timer cũ -> Gõ 5 chữ sẽ gọi API đủ 5 lần sau 300ms (chỉ bị trễ chứ không gom!)
    timer = setTimeout(() => fn.apply(this, args), delay);
  };
}

// BẪY 2: Gọi fn() quên truyền ...args và this
function lostArgsDebounce(fn, delay) {
  let timer;
  return function(...args) {
    clearTimeout(timer);
    // LỖI: Gọi fn() rỗng -> từ khóa tìm kiếm biến thành undefined!
    timer = setTimeout(() => fn(), delay);
  };
}

// BẪY 3 (Trong React): Khởi tạo debounce trực tiếp mỗi lần render
// function SearchBox() {
//   // SAI: Mỗi lần state đổi, component vẽ lại tạo ra một biến timerId mới hoàn toàn -> mất tác dụng!
//   const handleSearch = debounce((q) => fetchApi(q), 300);
//   // ĐÚNG: Bọc bằng useMemo / useRef và gọi handleSearch.cancel() trong cleanup useEffect!
// }
```

---

## 📝 Phần 4 — Bộ Bài Tập Tự Luyện & Đề Tự Giải

### A. 4 Bài tập tự viết code (Từ Rất Dễ đến Vận dụng)

1. **Bài 1 (Rất Dễ):** Viết hàm `createPrefixLogger(prefix)` sử dụng Closure để trả về một hàm nhận `message` và trả về chuỗi `""[prefix] message""`.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Biến `prefix` là tham số của hàm ngoài, hàm bên trong chỉ cần ghép chuỗi `prefix` và `message`.

```javascript
function createPrefixLogger(prefix) {
  return function(message) {
    return `[${prefix}] ${message}`;
  };
}
// const infoLog = createPrefixLogger('API');
// infoLog('Đã tải xong') -> '[API] Đã tải xong'
```
</details>

2. **Bài 2 (Dễ):** Viết hàm `once(fn)` dùng Closure đảm bảo hàm `fn` chỉ được phép chạy **đúng 1 lần duy nhất** trong suốt vòng đời (các lần gọi sau trả về kết quả lần đầu hoặc không chạy lại).
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Tạo 2 biến trong closure: `let called = false;` và `let savedResult;`.

```javascript
function once(fn) {
  let called = false;
  let savedResult;
  return function(...args) {
    if (!called) {
      called = true;
      savedResult = fn.apply(this, args);
    }
    return savedResult;
  };
}
```
</details>

3. **Bài 3 (Trung bình):** Viết hàm `throttleTimestamp(fn, limit)` dùng `Date.now()` (không dùng `setTimeout`) để đảm bảo khoảng cách giữa 2 lần chạy `fn` ít nhất là `limit` ms.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Lưu `let lastTime = 0;`. Mỗi lần gọi, lấy `const now = Date.now();`. Nếu `now - lastTime >= limit` thì cập nhật `lastTime = now` rồi chạy `fn.apply(this, args)`.

```javascript
function throttleTimestamp(fn, limit) {
  let lastTime = 0;
  return function(...args) {
    const now = Date.now();
    if (now - lastTime >= limit) {
      lastTime = now;
      return fn.apply(this, args);
    }
  };
}
```
</details>

4. **Bài 4 (Vận dụng):** Viết hàm `debounceImmediate(fn, delay)` có thêm tính năng: nếu đang không có timer nào chờ, thực thi ngay lập tức ở lần bấm đầu tiên (Leading Edge), nhưng nếu người dùng bấm tiếp liên tục trong lúc chưa hết `delay` thì chặn lại và chỉ đặt lại thời gian khóa.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Kiểm tra `const shouldCallNow = timerId === null;` trước khi `clearTimeout` và đặt timer mới để reset `timerId = null`.

```javascript
function debounceImmediate(fn, delay) {
  let timerId = null;
  return function(...args) {
    const shouldCallNow = timerId === null;
    if (timerId !== null) clearTimeout(timerId);
    timerId = setTimeout(() => {
      timerId = null;
    }, delay);
    if (shouldCallNow) {
      fn.apply(this, args);
    }
  };
}
```
</details>

### B. 3 Câu hỏi tự kiểm tra tư duy

1. **Câu hỏi 1:** Nếu người dùng cuộn chuột liên tục không ngừng trong 10 giây, chức năng ""tải thêm bài khi cuộn"" nên dùng Debounce hay Throttle? Vì sao?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Phải dùng **Throttle**! Nếu dùng Debounce, trang web sẽ đứng im hoàn toàn suốt 10 giây vì người dùng chưa ngừng cuộn, tạo cảm giác giật lag. Throttle giúp kiểm tra vị trí cuộn đều đặn mỗi 200–300ms để tải nội dung mượt mà.
</details>

2. **Câu hỏi 2:** Tại sao dòng `const t = setTimeout(() => { val = 10; }, 50); clearTimeout(t); console.log(val);` in ra `0` (giá trị cũ) thay vì `10`?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Vì `clearTimeout(t)` đã hủy lệnh hẹn giờ trước khi hết 50ms nên hàm callback `{ val = 10; }` bị loại bỏ hoàn toàn, không bao giờ được thực thi.
</details>

3. **Câu hỏi 3:** Tại sao phải viết `debounced.cancel()` và gọi nó khi người dùng đóng modal hoặc chuyển trang?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Để hủy bỏ bộ đếm thời gian đang chạy ngầm. Nếu không hủy, callback vẫn kích hoạt sau khi màn hình đã bị hủy, gây lỗi cập nhật state trên component không còn tồn tại và rò rỉ bộ nhớ (Memory Leak).
</details>

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Nhập môn Closure, setTimeout/clearTimeout và phân biệt Debounce vs Throttle.",
                "Bảo toàn ngữ cảnh this/args, dọn dẹp timer chống Memory Leak."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson22Id,
                "Bài 2.2: Memoization & Cache Kết Quả Hàm Tính Toán Phức Tạp",
                "memoization-va-cache-ket-qua-ham",
                40,
                @"# Bài 2.2: Memoization & Cache Kết Quả Hàm Tính Toán Phức Tạp

> [!TIP]
> Nghe chữ **Memoization** tưởng khó, nhưng thực ra đây chính là thói quen **""ghi nháp kết quả để lần sau khỏi tính lại""**. Phần 0 sẽ giúp bạn làm quen với sổ tay `Map` và cơ chế tra cứu chỉ trong vài phút!

---

## 🌱 Phần 0 — Nhập môn từ con số 0 (Dành cho người mới hoàn toàn)

### 0.1. Bài toán đời thường: Cô thủ thư ghi nháp
Giả sử có người hỏi bạn: *""$12345 \times 6789$ bằng bao nhiêu?""*. Bạn mất 30 giây nhẩm ra kết quả rồi ghi vào cuốn sổ nháp. Một phút sau, có người khác hỏi lại đúng phép tính đó — bạn chỉ cần **lật sổ đọc ra đáp án trong 1 giây** mà không phải nhân lại!
- Cuốn sổ nháp đó trong lập trình gọi là **Cache (Bộ nhớ đệm)**.
- Kỹ thuật gắn cuốn sổ nháp tự động vào một hàm gọi là **Memoization** (xuất phát từ *memo* = ghi chú).

### 0.2. Làm quen cấu trúc dữ liệu `Map` trong JavaScript
Để làm sổ nháp, JavaScript có sẵn đối tượng `Map` với 3 thao tác cơ bản dễ nhớ như đếm 1-2-3:
```javascript
const cache = new Map();       // Tạo cuốn sổ nháp rỗng

cache.has('key1');             // 1. Hỏi: ""Trong sổ có lưu 'key1' chưa?"" -> true / false
cache.set('key1', 999);        // 2. Ghi: Lưu cặp (khóa 'key1', giá trị 999) vào sổ
cache.get('key1');             // 3. Đọc: Lấy giá trị của 'key1' -> trả về 999
// Kiểm tra số lượng mục đang lưu: cache.size
```
**Tại sao dùng `Map` thay vì Object `{}`?** `Map` phân biệt rõ khóa số `1` với chuỗi `'1'`, không bị xung đột với các tên thuộc tính có sẵn của Object như `'toString'` hay `'constructor'`, và giữ nguyên thứ tự thêm vào để dễ xóa mục cũ nhất!

#### Bảng theo dõi từng bước (Trace Table) — Hàm bình phương có Cache
```
Gọi 1: memoSquare(4) -> cache.has(4) = false (Cache Miss!) -> Tính 4*4 = 16 -> Ghi {4 => 16} -> callCount = 1
Gọi 2: memoSquare(4) -> cache.has(4) = true  (Cache Hit!)  -> Đọc ngay 16  -> Không tính lại -> callCount = 1
Gọi 3: memoSquare(5) -> cache.has(5) = false (Cache Miss!) -> Tính 5*5 = 25 -> Ghi {4=>16, 5=>25} -> callCount = 2
```

| Lần gọi | Đầu vào `n` | Đã có trong `cache`? | Trạng thái | Hàm tính gốc có chạy? | `callCount` | Nội dung `cache` sau lệnh |
|---|---|---|---|---|---|---|
| Lượt 1 | `memoSquare(4)` | Không (`false`) | **Cache Miss** | Có (`4 * 4 = 16`) | `1` | `{ 4 => 16 }` |
| Lượt 2 | `memoSquare(4)` | Có (`true`) | **Cache Hit** | Không (trả ngay) | `1` | `{ 4 => 16 }` |
| Lượt 3 | `memoSquare(5)` | Không (`false`) | **Cache Miss** | Có (`5 * 5 = 25`) | `2` | `{ 4 => 16, 5 => 25 }` |

---

## 📘 Phần 1 & 2 — Lý thuyết cốt lõi đi từ Dễ đến Khó

### 1.1. Điều kiện bắt buộc: Hàm Thuần Khiết (Pure Function)
Bạn **CHỈ ĐƯỢC PHÉP** memoize khi hàm là **Hàm Thuần Khiết**:
1. **Cùng đầu vào luôn cho đúng 1 đầu ra:** Ví dụ `add(2, 3)` hôm nay hay ngày mai đều luôn là `5`.
2. **Không phụ thuộc yếu tố bên ngoài biến đổi:** Không dùng `Date.now()`, `Math.random()`, không đọc biến toàn cục hay gọi API mạng đổi liên tục bên trong hàm được memoize.

### 1.2. Cách tạo khóa cache an toàn cho nhiều tham số (`Key Generation`)
Một hàm thường nhận nhiều tham số (như `calcShipping(weight, distance, isVip)`), hoặc nhận tham số là Object (`{ city: 'HCM' }`).
- Nếu ghép bằng `args.toString()` hoặc `String(obj)`: hai object khác nhau `{ a: 1 }` và `{ b: 2 }` đều biến thành chuỗi `""[object Object]""`! Kết quả là gọi dữ liệu của B lại trả nhầm đáp án của A (**Key Collision**).
- **Giải pháp chuẩn:** Dùng `JSON.stringify(args)` để biến toàn bộ danh sách tham số thành một chuỗi duy nhất (ví dụ `'[2,15,true]'` hoặc `'[{""a"":1}]'`).

### 2.1. Sức mạnh tối ưu thuật toán & Quản lý RAM
- **Biến thuật toán hàm mũ $O(2^n)$ thành tuyến tính $O(n)$:** Tính số Fibonacci `fib(40)` bằng đệ quy thuần mất hàng trăm triệu phép gọi hàm (treo máy); thêm `Map` cache vào chỉ mất đúng 40 bước trong chưa tới 1 mili-giây!
- **Giới hạn bộ nhớ (Cache Eviction):** Nếu ứng dụng chạy liên tục cả tuần, `Map` không giới hạn sẽ phình to mãi gây tràn RAM (**OutOfMemory**). Trong thực tế, ta kiểm tra `if (cache.size >= maxSize)` để xóa khóa cũ nhất (`cache.keys().next().value`) theo chiến lược FIFO hoặc LRU.

---

## 🔍 Phần 3 — 3 Ví dụ Mẫu Giải Chậm Từng Dòng (Worked Examples)

### Ví dụ 1 (Mức Dễ - Làm quen): Cache hàm tính giai thừa đơn giản
```javascript
function createMemoSquare() {
  const cache = new Map();              // Dòng 1: Tạo sổ tay lưu kết quả trong Closure
  return function(n) {                  // Dòng 2: Hàm nhận tham số n
    if (cache.has(n)) {                 // Dòng 3: Kiểm tra n đã từng tính chưa
      return cache.get(n);              // Dòng 4: Có rồi -> Trả luôn không cần nhân
    }
    const result = n * n;               // Dòng 5: Chưa có -> Thực hiện phép tính
    cache.set(n, result);               // Dòng 6: Ghi kết quả vào sổ cho lần sau
    return result;                      // Dòng 7: Trả kết quả về cho người gọi
  };
}
```

### Ví dụ 2 (Mức Trung bình - Bài toán thực tế): Hàm `memoize` đa tham số cho tính phí vận chuyển
```javascript
function memoize(fn, maxSize = 100) {
  const cache = new Map();

  return function(...args) {
    // Biến mọi tham số (số, chuỗi, object) thành chuỗi khóa JSON duy nhất
    const key = JSON.stringify(args);

    if (cache.has(key)) {
      return cache.get(key); // Cache Hit: lấy ngay với O(1)
    }

    const result = fn.apply(this, args); // Cache Miss: chạy hàm tính toán thật

    // Bảo vệ RAM: Nếu đầy bộ nhớ đệm thì xóa phần tử cũ nhất
    if (cache.size >= maxSize) {
      const oldestKey = cache.keys().next().value;
      cache.delete(oldestKey);
    }

    cache.set(key, result);
    return result;
  };
}

// Ứng dụng tính cước phức tạp
const calcShippingFee = memoize((weightKg, distanceKm, express) => {
  const base = weightKg * 5000 + distanceKm * 1200;
  return express ? Math.round(base * 1.5) : base;
});

console.log(calcShippingFee(2, 10, true)); // Tính toán lần 1 -> 33000
console.log(calcShippingFee(2, 10, true)); // Trả tức thì từ Cache -> 33000
```

### Ví dụ 3 (Mức Nâng cao + Bẫy hay sai): 3 Bẫy Kinh Điển Khi Dùng Cache
```javascript
// BẪY 1: Kiểm tra tồn tại bằng `if (cache[key])` khi kết quả hợp lệ là số 0 hoặc false!
function buggyFalsyCache(fn) {
  const cache = {};
  return function(x) {
    // LỖI: Nếu fn(0) trả về 0 (là falsy), if (cache[0]) ra false -> Lần nào cũng bị Cache Miss!
    if (cache[x]) return cache[x];
    return (cache[x] = fn(x));
  };
}
// SỬA ĐÚNG: Luôn dùng `cache.has(x)` với Map (hoặc `x in cache` với Object).

// BẪY 2: Trả về tham chiếu trực tiếp của mảng/object đang nằm trong Cache
const listCache = new Map();
function getTagsSafe(category) {
  if (listCache.has(category)) {
    // SỬA ĐÚNG: Trả về bản sao [...arr] để bên ngoài gọi .push() không làm hỏng Cache!
    return [...listCache.get(category)];
  }
  const tags = ['js', 'web'];
  listCache.set(category, tags);
  return [...tags];
}

// BẪY 3: Quên gọi cache.set() hoặc dùng obj.toString() làm key
// Dùng obj.toString() khiến { id: 1 } và { id: 2 } đều thành ""[object Object]"" -> trả về nhầm dữ liệu!
```

---

## 📝 Phần 4 — Bộ Bài Tập Tự Luyện & Đề Tự Giải

### A. 4 Bài tập tự viết code (Từ Rất Dễ đến Vận dụng)

1. **Bài 1 (Rất Dễ):** Viết hàm `lookupScore(scoreMap, studentName)` nhận vào một `Map` điểm và tên học viên; nếu có trong `Map` trả về điểm, nếu không có trả về `-1`.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Lưu ý điểm `0` cũng là điểm hợp lệ! Phải dùng `scoreMap.has(studentName)` thay vì `scoreMap.get(studentName) || -1`.

```javascript
function lookupScore(scoreMap, studentName) {
  if (scoreMap.has(studentName)) {
    return scoreMap.get(studentName);
  }
  return -1;
}
```
</details>

2. **Bài 2 (Dễ):** Viết hàm `memoizeSingleArg(fn)` lưu cache cho hàm nhận đúng 1 tham số nguyên thủy (`number` hoặc `string`) bằng `new Map()`.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Khởi tạo `const cache = new Map();` bên ngoài hàm con trả về, kiểm tra `cache.has(x)`, tính `const val = fn(x); cache.set(x, val);`.

```javascript
function memoizeSingleArg(fn) {
  const cache = new Map();
  return function(x) {
    if (cache.has(x)) return cache.get(x);
    const val = fn(x);
    cache.set(x, val);
    return val;
  };
}
```
</details>

3. **Bài 3 (Trung bình):** Cài đặt hàm tính số Fibonacci `fibMemo(n)` với `Map` cache bên trong để `fibMemo(45)` chạy tức thì mà không bị treo trình duyệt.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Với `n <= 1` trả về `n`. Trước khi đệ quy `fib(n - 1) + fib(n - 2)`, kiểm tra `cache.has(n)` và lưu lại sau khi tính xong.

```javascript
function createFibMemo() {
  const cache = new Map();
  function fib(n) {
    if (n <= 1) return n;
    if (cache.has(n)) return cache.get(n);
    const res = fib(n - 1) + fib(n - 2);
    cache.set(n, res);
    return res;
  }
  return fib;
}
```
</details>

4. **Bài 4 (Vận dụng):** Viết hàm `createLruMemoize(fn, capacity)` kết hợp Memoization với LRU Cache (Least Recently Used): khi `cache.has(key)` trúng, xóa key đó rồi `set` lại vào cuối `Map` để đánh dấu là vừa dùng gần nhất; khi vượt `capacity`, xóa phần tử đầu tiên.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** `Map` trong JS duy trì thứ tự chèn. Khi cache hit: `const v = cache.get(key); cache.delete(key); cache.set(key, v); return v;`.

```javascript
function createLruMemoize(fn, capacity = 50) {
  const cache = new Map();
  return function(...args) {
    const key = JSON.stringify(args);
    if (cache.has(key)) {
      const val = cache.get(key);
      cache.delete(key);
      cache.set(key, val);
      return val;
    }
    const result = fn.apply(this, args);
    if (cache.size >= capacity) {
      const oldestKey = cache.keys().next().value;
      cache.delete(oldestKey);
    }
    cache.set(key, result);
    return result;
  };
}
```
</details>

### B. 3 Câu hỏi tự kiểm tra tư duy

1. **Câu hỏi 1:** Có nên áp dụng `memoize()` cho hàm `getCurrentExchangeRate(currency)` (gọi API lấy tỷ giá USD mỗi giây) không?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** **Không nên** dùng `memoize` vĩnh viễn! Vì tỷ giá thay đổi theo thời gian (đây không phải Pure Function). Nếu cache vĩnh viễn theo mã `'USD'`, người dùng sẽ luôn nhận tỷ giá cũ của ngày hôm qua. Với dữ liệu thay đổi theo thời gian, phải dùng Cache có thời hạn hết hạn (TTL).
</details>

2. **Câu hỏi 2:** Vì sao câu lệnh `({ a: 1 }).toString() === ({ b: 2 }).toString()` trong JavaScript lại trả về `true`?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Vì mặc định mọi plain object khi gọi `.toString()` đều trả về cùng một chuỗi `""[object Object]""`. Do đó tuyệt đối không dùng `.toString()` làm cache key cho object!
</details>

3. **Câu hỏi 3:** Nếu hàm gốc trả về giá trị hợp lệ là `0` (ví dụ `indexOf`), lỗi gì xảy ra nếu ta kiểm tra `if (cache[key])` thay vì `if (cache.has(key))`?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Số `0` trong JavaScript bị ép kiểu thành `false` (falsy). Vì vậy `if (cache[key])` sẽ tưởng rằng chưa có trong cache và bắt hàm tính toán lại mỗi lần gọi. Dùng `cache.has(key)` luôn kiểm tra sự tồn tại của khóa, bất kể giá trị là `0`, `false`, `""` hay `null`.
</details>

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Nhập môn Map (has/get/set), hàm thuần khiết và kỹ thuật Memoization tối ưu O(1).",
                "Phòng tránh Key Collision, đột biến dữ liệu cache và tràn bộ nhớ RAM."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson31Id,
                "Bài 3.1: Async/Await, Promise Pipeline & Retry Cơ Chế Tự Phục Hồi",
                "async-await-promise-pipeline-retry",
                45,
                @"# Bài 3.1: Async/Await, Promise Pipeline & Retry Cơ Chế Tự Phục Hồi

> [!TIP]
> Lập trình bất đồng bộ (`Promise`, `async/await`, `Event Loop`) là rào cản lớn nhất với người mới học JS. Đừng vội nhớ thuộc lòng lý thuyết — hãy bắt đầu từ hình ảnh **quán trà sữa và thẻ rung** ở **Phần 0**!

---

## 🌱 Phần 0 — Nhập môn từ con số 0 (Dành cho người mới hoàn toàn)

### 0.1. Đồng bộ (Sync) vs Bất đồng bộ (Async)
- **Đồng bộ (Synchronous):** Bạn đứng xếp hàng mua trà sữa. Phải đợi pha chế xong ly của bạn (mất 5 phút) thì nhân viên mới nhận đơn người tiếp theo. Trong phần mềm, nếu việc tải API mất 2 giây mà chạy đồng bộ, toàn bộ nút bấm và cuộn trang sẽ **đóng băng hoàn toàn**.
- **Bất đồng bộ (Asynchronous):** Nhân viên đưa bạn một **chiếc thẻ rung (Promise)** rồi tiếp tục phục vụ khách khác. Trong lúc chờ, màn hình web vẫn cuộn mượt mà. Khi nước xong, thẻ rung lên — bạn quay lại lấy kết quả!

### 0.2. `Promise` (Giấy hẹn tương lai) có đúng 3 trạng thái
1. **`Pending` (Đang chờ):** Thẻ chưa rung, yêu cầu mạng đang bay trên đường truyền.
2. **`Fulfilled` (Thành công):** Quầy pha xong, trả về dữ liệu (ví dụ danh sách sản phẩm).
3. **`Rejected` (Thất bại):** Hết nguyên liệu hoặc mất mạng, trả về đối tượng lỗi (`Error`).

### 0.3. `async` / `await` — Viết code bất đồng bộ dễ như đọc truyện
Thay vì lồng nhiều `.then().then()` rối mắt, ES2017 giới thiệu từ khóa `async` và `await`:
```javascript
async function loadUserProfile() {        // Gắn async để báo hàm này có thao tác chờ
  try {
    const user = await fetchUserApi();    // Tạm dừng Ở ĐÂY chờ Promise xong (không treo web!)
    console.log('Xin chào:', user.name);  // Chạy tiếp sau khi đã có biến user
  } catch (err) {
    console.error('Lỗi mạng:', err);      // Nhảy vào đây nếu Promise bị Rejected
  }
}
```

### 0.4. Hình dung Event Loop (Vòng lặp sự kiện) cho người mới
JavaScript chỉ có 1 luồng thực thi chính, điều phối bởi 3 hàng đợi ưu tiên từ cao xuống thấp:
```
1. Call Stack (Bàn làm việc chính)      -> Chạy hết 100% code đồng bộ trước!
2. Microtask Queue (Hàng ưu tiên VIP)   -> Xả sạch mọi Promise.then / phần sau lệnh await!
3. Macrotask Queue (Hàng đợi thường)    -> Lấy từng tác vụ setTimeout / sự kiện UI chạy sau cùng!
```

#### Bảng theo dõi từng bước (Trace Table)
```javascript
console.log('1');
setTimeout(() => console.log('2'), 0);
Promise.resolve().then(() => console.log('3'));
console.log('4');
```
| Bước | Call Stack đang chạy | Microtask Queue (`Promise`) | Macrotask Queue (`setTimeout`) | In ra màn hình |
|---|---|---|---|---|
| Lượt 1 | `console.log('1')` | Trống | Trống | `1` |
| Lượt 2 | `setTimeout(..., 0)` | Trống | `[callback in '2']` | `1` |
| Lượt 3 | `Promise.resolve().then(...)` | `[callback in '3']` | `[callback in '2']` | `1` |
| Lượt 4 | `console.log('4')` | `[callback in '3']` | `[callback in '2']` | `1`, `4` |
| Lượt 5 | Trống $\rightarrow$ Lấy Microtask VIP! | Đang chạy in `'3'` | `[callback in '2']` | `1`, `4`, `3` |
| Lượt 6 | Trống $\rightarrow$ Lấy Macrotask! | Trống | Đang chạy in `'2'` | `1`, `4`, `3`, `2` |

---

## 📘 Phần 1 & 2 — Lý thuyết cốt lõi đi từ Dễ đến Khó

### 1.1. Chạy tuần tự (`for...of`) vs Chạy song song (`Promise.all` / `Promise.allSettled`)
Giả sử bạn cần tải 3 API, mỗi API mất 1 giây:
1. **Chạy tuần tự (`for (const id of ids) { await fetch(id); }`):** Xong cái 1 mới gọi cái 2 $\rightarrow$ Tổng mất **3 giây**. Chỉ dùng khi bước sau phụ thuộc kết quả bước trước.
2. **Chạy song song với `Promise.all([p1, p2, p3])`:** Bắn cả 3 cùng lúc $\rightarrow$ Chỉ mất **1 giây**! Nhưng lưu ý: nếu **chỉ 1 task bị lỗi**, toàn bộ `Promise.all` ném lỗi ngay lập tức và vứt bỏ kết quả của 2 task thành công kia (**Fail-Fast**).
3. **Chạy song song an toàn với `Promise.allSettled([p1, p2, p3])`:** Chờ tất cả hoàn thành bất kể thành công hay lỗi! Mỗi phần tử trả về `{ status: 'fulfilled', value }` hoặc `{ status: 'rejected', reason }`. Đây là lựa chọn chuẩn trong dashboard tải nhiều widget độc lập.

### 2.1. Cơ chế Tự Phục Hồi: Retry với Exponential Backoff & Jitter
Khi máy chủ quá tải tạm thời (lỗi HTTP 429 hoặc 503), nếu code thử gọi lại liên tục ngay lập tức 10 lần trong 10ms, bạn sẽ vô tình làm sập hẳn máy chủ!
- **Exponential Backoff (Lùi cấp số nhân):** Tăng thời gian chờ gấp đôi sau mỗi lần thất bại: `delay = baseDelay * Math.pow(2, attempt - 1)` (ví dụ: 300ms $\rightarrow$ 600ms $\rightarrow$ 1200ms).
- **Jitter (Độ trễ ngẫu nhiên):** Cộng thêm một số ngẫu nhiên nhỏ `+ Math.random() * 100` để 1.000 người dùng bị rớt mạng không cùng thử lại vào đúng một tích tắc (tránh bão **Thundering Herd**).

---

## 🔍 Phần 3 — 3 Ví dụ Mẫu Giải Chậm Từng Dòng (Worked Examples)

### Ví dụ 1 (Mức Dễ - Làm quen): Hàm `sleep(ms)` tạo khoảng nghỉ bất đồng bộ
```javascript
function sleep(ms) {                      // Dòng 1: Nhận số mili-giây cần chờ
  return new Promise((resolve) => {       // Dòng 2: Trả về một Promise mới
    setTimeout(() => resolve(), ms);      // Dòng 3: Sau ms mili-giây, gọi resolve() báo hoàn tất
  });
}

async function countdown() {
  console.log('Chuẩn bị...');
  await sleep(1000);                      // Chờ 1 giây không làm đơ giao diện
  console.log('Bắt đầu!');
}
```

### Ví dụ 2 (Mức Trung bình - Bài toán thực tế): Tải nhiều hồ sơ không chết chùm bằng `Promise.allSettled`
```javascript
async function loadProfilesSafely(promises) {
  // Chờ tất cả các Promise hoàn tất dù có cái thành công, có cái lỗi
  const settled = await Promise.allSettled(promises);

  const succeeded = settled
    .filter((item) => item.status === 'fulfilled')
    .map((item) => item.value);

  const failedCount = settled.filter((item) => item.status === 'rejected').length;

  return { succeeded, failedCount };
}
```

### Ví dụ 3 (Mức Nâng cao + Bẫy hay sai): `retryWithBackoff` & Bẫy `forEach(async ...)`
```javascript
// BẪY KINH ĐIỂN: Dùng array.forEach với async/await!
async function buggyLoadAll(urls, fetchFn) {
  const results = [];
  // LỖI: forEach KHÔNG CHỜ các hàm callback async bên trong!
  urls.forEach(async (url) => {
    const data = await fetchFn(url);
    results.push(data);
  });
  return results; // LUÔN TRẢ VỀ MẢNG RỖNG [] trước khi dữ liệu kịp tải về!
}

// GIẢI PHÁP CHUẨN: Dùng vòng lặp `for...of` cho tuần tự, và bọc Retry có giới hạn
async function retryWithBackoff(taskFn, maxRetries = 3, baseDelay = 300) {
  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      return await taskFn();
    } catch (error) {
      if (attempt === maxRetries) {
        throw new Error(`Thất bại sau ${maxRetries + 1} lần thử: ${error.message}`);
      }
      const jitter = Math.random() * 100;
      const delay = baseDelay * Math.pow(2, attempt) + jitter;
      await sleep(delay);
    }
  }
}
```
> [!CAUTION]
> **Bẫy quên `try/catch` quanh `await`:** Nếu một lệnh `await` bị lỗi mà không có `try/catch`, Promise trả về sẽ bị Rejected không có người hứng (**Unhandled Promise Rejection**), có thể làm sập tiến trình Node.js hoặc treo thanh loading trên giao diện mãi mãi!

---

## 📝 Phần 4 — Bộ Bài Tập Tự Luyện & Đề Tự Giải

### A. 4 Bài tập tự viết code (Từ Rất Dễ đến Vận dụng)

1. **Bài 1 (Rất Dễ):** Viết hàm `safeRunAsync(asyncFn, fallbackValue)` gọi `await asyncFn()`, nếu thành công trả về kết quả, nếu lỗi thì trả về `fallbackValue` (không làm crash chương trình).
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Bọc `return await asyncFn();` trong khối `try { ... } catch { return fallbackValue; }`.

```javascript
async function safeRunAsync(asyncFn, fallbackValue) {
  try {
    return await asyncFn();
  } catch (err) {
    return fallbackValue;
  }
}
```
</details>

2. **Bài 2 (Dễ):** Viết hàm `runSequentially(tasks)` nhận mảng các hàm trả về Promise `tasks` (ví dụ `[() => fetchA(), () => fetchB()]`) và thực thi lần lượt từng cái bằng `for...of`, trả về mảng kết quả đúng thứ tự.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Tuyệt đối không dùng `.forEach(async ...)`! Tạo mảng `results = []` rồi lặp `for (const task of tasks)`.

```javascript
async function runSequentially(tasks) {
  const results = [];
  for (const task of tasks) {
    const res = await task();
    results.push(res);
  }
  return results;
}
```
</details>

3. **Bài 3 (Trung bình):** Viết hàm `withTimeout(promise, ms)` trả về kết quả của `promise` nếu nó xong trước `ms` mili-giây; nếu quá giờ thì reject với thông báo `new Error('Timeout')`.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Tạo một `timeoutPromise` chỉ gọi `reject` sau `ms` ms, rồi dùng `Promise.race([promise, timeoutPromise])`.

```javascript
function withTimeout(promise, ms) {
  const timeout = new Promise((_, reject) => {
    setTimeout(() => reject(new Error('Timeout')), ms);
  });
  return Promise.race([promise, timeout]);
}
```
</details>

4. **Bài 4 (Vận dụng):** Viết hàm `retrySimple(taskFn, maxAttempts)` thử gọi tối đa `maxAttempts` lần; nếu thành công trả kết quả ngay, nếu thử hết vẫn lỗi thì ném lỗi cuối cùng ra ngoài.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Dùng vòng `for (let i = 0; i < maxAttempts; i++)`, trong `catch (err)` kiểm tra `if (i === maxAttempts - 1) throw err;`.

```javascript
async function retrySimple(taskFn, maxAttempts) {
  for (let i = 0; i < maxAttempts; i++) {
    try {
      return await taskFn();
    } catch (err) {
      if (i === maxAttempts - 1) throw err;
    }
  }
}
```
</details>

### B. 3 Câu hỏi tự kiểm tra tư duy

1. **Câu hỏi 1:** Đoạn code sau in ra theo thứ tự nào: `async function f() { console.log(1); await Promise.resolve(); console.log(2); } f(); console.log(3);`?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Thứ tự là **`1` $\rightarrow$ `3` $\rightarrow$ `2`**. Code trong `f()` chạy đồng bộ cho tới trước chữ `await` nên in `1` ngay. Gặp `await`, phần phía sau (`console.log(2)`) bị đẩy vào **Microtask Queue**, nhường Call Stack in tiếp `3`, rồi mới quay lại chạy `2`.
</details>

2. **Câu hỏi 2:** Vì sao có `Promise.all` rồi vẫn cần `Promise.allSettled`?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** `Promise.all` dừng ngay khi 1 promise thất bại, làm mất dữ liệu các promise đã thành công. `Promise.allSettled` chờ toàn bộ hoàn tất để ta hiển thị những phần tải được và chỉ báo lỗi đúng phần bị hỏng.
</details>

3. **Câu hỏi 3:** Nếu thiếu từ khóa `reject` bên trong `new Promise((resolve, reject) => ...)` khi có lỗi xảy ra, điều gì xảy ra với lệnh `await`?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Promise mãi mãi kẹt ở trạng thái `Pending`, khiến câu lệnh `await` chờ vô tận (treo logic) và khối `catch` không bao giờ chạy tới.
</details>

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Nhập môn Sync vs Async, Promise 3 trạng thái, Async/Await và Event Loop.",
                "Pipeline song song Promise.allSettled và thuật toán Retry Exponential Backoff."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson32Id,
                "Bài 3.2: Deep Clone, Tính Bất Biến & Xử Lý Lỗi Toàn Diện",
                "deep-clone-immutability-va-xu-ly-loi",
                40,
                @"# Bài 3.2: Deep Clone, Tính Bất Biến & Xử Lý Lỗi Toàn Diện

> [!TIP]
> Bạn từng gặp cảnh ""sửa biến `copy` mà biến `original` tự dưng đổi theo"" chưa? Đó là do cách JavaScript lưu trữ Object trong bộ nhớ! **Phần 0** sẽ giải mã bí ẩn này bằng hình ảnh **photo giấy vs đánh thêm chìa khóa phòng**.

---

## 🌱 Phần 0 — Nhập môn từ con số 0 (Dành cho người mới hoàn toàn)

### 0.1. Kiểu Nguyên Thủy vs Kiểu Tham Chiếu trong bộ nhớ
JavaScript chia dữ liệu thành 2 nhóm hoàn toàn khác nhau khi gán bằng dấu `=`:
1. **Kiểu nguyên thủy (`number`, `string`, `boolean`, `null`, `undefined`):** Giống như **photo một tờ giấy**.
   ```javascript
   let a = 10;
   let b = a;  // Photo ra tờ giấy mới mang số 10
   b = 99;     // Sửa tờ b thành 99 -> tờ a vẫn là 10!
   ```
2. **Kiểu tham chiếu (`Object {}`, `Array []`):** Giống như **đánh thêm một chiếc chìa khóa mở chung 1 căn phòng trong Heap**.
   ```javascript
   const userA = { name: 'An', age: 20 };
   const userB = userA; // KHÔNG tạo object mới! userB chỉ là chìa khóa thứ 2 mở chung phòng!
   userB.age = 99;
   console.log(userA.age); // 99!! userA bị sửa lây!
   ```

### 0.2. Sao chép nông (`{ ...obj }`, `[...arr]`) vs Sao chép sâu (Deep Clone)
Toán tử ba chấm `...` (Spread) chỉ tách riêng **tầng 1 (tầng ngoài cùng)**. Nếu bên trong object lại lồng thêm một object hoặc mảng con (tầng 2), các tầng con vẫn dùng chung chìa khóa!
```
userA: { name: 'An', profile: ──┐
                                ▼
                         [ Ô nhớ Heap: { score: 10 } ]  <-- CHUNG NHAU!
                                ▲
copy : { name: 'An', profile: ──┘   (khi dùng { ...userA })
```

#### Bảng Trace từng bước: Spread Nông vs Deep Clone
Giả sử `const orig = { name: 'An', profile: { score: 10 } };`

| Bước thực hiện | Nếu dùng Spread Nông `copy = { ...orig }` | Nếu dùng `deepClone(orig)` | `orig.profile.score` cuối cùng |
|---|---|---|---|
| Bước 1: Tạo bản sao | `copy` là vỏ mới, nhưng `copy.profile === orig.profile` | `copy` và `copy.profile` đều là ô nhớ mới độc lập | `10` |
| Bước 2: `copy.name = 'Bình'` | Chỉ đổi `copy.name`, `orig.name` vẫn là `'An'` | Chỉ đổi `copy.name`, `orig.name` vẫn là `'An'` | `10` |
| Bước 3: `copy.profile.score = 99` | Sửa thẳng vào ô nhớ chung! `orig.profile.score` thành `99`! | Chỉ sửa bản sao! `orig.profile.score` vẫn là `10`! | Nông: `99` / Sâu: `10` |

### 0.3. Đệ quy (Recursion) là gì?
**Đệ quy** là kỹ thuật hàm **tự gọi lại chính nó** để xử lý các tầng lồng nhau không biết trước độ sâu (tầng 1 $\rightarrow$ tầng 2 $\rightarrow$ tầng 3...). Để không chạy vô tận làm tràn ngăn xếp gọi hàm (`Maximum call stack size exceeded`), mọi hàm đệ quy **bắt buộc có điều kiện dừng** ngay dòng đầu tiên (ví dụ: nếu là số/chuỗi/`null` thì trả về ngay!).

---

## 📘 Phần 1 & 2 — Lý thuyết cốt lõi đi từ Dễ đến Khó

### 1.1. Hai cách Deep Clone chuẩn trong thực tế
1. **Cách 1 — Hàm dựng sẵn hiện đại `structuredClone(obj)`:**
   - Có sẵn trên Chrome/Firefox/Edge/Node.js hiện đại.
   - Sao chép sâu chuẩn chỉnh `Object`, `Array`, `Date`, `RegExp`, `Map`, `Set` và xử lý luôn tham chiếu vòng.
   - **Giới hạn:** Sẽ ném lỗi `DataCloneError` nếu trong object có chứa hàm (`function`) hoặc phần tử DOM HTML.
2. **Cách 2 — Tự viết hàm đệ quy `safeDeepClone(value, seen = new WeakMap())`:**
   - Dùng `WeakMap` làm sổ ghi nhớ những object đã từng đi qua. Nếu gặp vòng lặp tham chiếu (ví dụ `node.self = node`), trả về ngay bản sao đã lưu trong `WeakMap` để tránh đệ quy vô tận.

### 2.1. Xử lý lỗi theo tầng (Layered Error Handling)
Khi xây dựng hệ thống thật, đừng bao giờ viết `catch (e) {}` rỗng (nuốt lỗi âm thầm) hoặc chỉ `throw 'Lỗi rồi'` (ném chuỗi thuần mất dấu vết Stack Trace):
- **Tạo Custom Error Class:** Kế thừa từ `Error` (`class ValidationError extends Error`), gọi `super(message)` ở dòng đầu tiên trong `constructor`, đặt `this.name = 'ValidationError'`.
- **Phân loại bằng `instanceof`:** Dùng `if (err instanceof ValidationError)` để trả về HTTP 400 (lỗi người dùng nhập sai), còn các lỗi hệ thống không lường trước thì `throw err` ra tầng trên ghi log!
- **Khối `finally`:** Luôn chạy dù thành công hay thất bại — dùng để đóng kết nối DB hoặc tắt trạng thái loading.

---

## 🔍 Phần 3 — 3 Ví dụ Mẫu Giải Chậm Từng Dòng (Worked Examples)

### Ví dụ 1 (Mức Dễ - Làm quen): Cập nhật Object 1 tầng bất biến
```javascript
const user = { id: 1, name: 'Minh', role: 'student' }; // Dòng 1: Object 1 tầng không lồng nhau
const promoted = { ...user, role: 'mentor' };           // Dòng 2: Chép hết thuộc tính, ghi đè role mới
console.log(user.role);                                 // Dòng 3: 'student' (dữ liệu gốc nguyên vẹn)
console.log(promoted.role);                             // Dòng 4: 'mentor'
```

### Ví dụ 2 (Mức Trung bình - Bài toán thực tế): Sao chép sâu cấu hình ứng dụng nhiều tầng
```javascript
function cloneConfig(value) {
  // Điều kiện dừng đệ quy: nhớ kiểm tra value === null TRƯỚC typeof!
  if (value === null || typeof value !== 'object') {
    return value;
  }
  // Nếu là mảng -> tạo mảng mới và gọi đệ quy cho từng phần tử
  if (Array.isArray(value)) {
    return value.map((item) => cloneConfig(item));
  }
  // Nếu là object thuần -> duyệt từng key và gọi đệ quy
  const copy = {};
  for (const key of Object.keys(value)) {
    copy[key] = cloneConfig(value[key]);
  }
  return copy;
}
```

### Ví dụ 3 (Mức Nâng cao + Bẫy hay sai): Tham chiếu vòng, `typeof null` & Bẫy `JSON.stringify`
```javascript
// BẪY 1: Quên kiểm tra `value === null`!
// Trong JS, `typeof null` trả về `'object'` (lỗi lịch sử của ngôn ngữ)!
// Nếu quên check `value === null`, lệnh `Object.keys(null)` sẽ văng TypeError ngay!

// BẪY 2: Lạm dụng `JSON.parse(JSON.stringify(obj))`
// -> Làm biến mất thuộc tính có giá trị `undefined` hoặc hàm
// -> Biến kiểu `Date` thành chuỗi string (không gọi được .getFullYear() nữa!)
// -> Văng lỗi crash ngay nếu object có vòng lặp tham chiếu (obj.self = obj)!

// GIẢI PHÁP CHUẨN: Hàm Deep Clone an toàn với WeakMap & giữ Date/RegExp
function safeDeepClone(value, seen = new WeakMap()) {
  if (value === null || typeof value !== 'object') return value;
  if (value instanceof Date) return new Date(value.getTime());
  if (value instanceof RegExp) return new RegExp(value.source, value.flags);

  // Chặn vòng lặp tham chiếu (Circular Reference)
  if (seen.has(value)) return seen.get(value);

  const clone = Array.isArray(value) ? [] : {};
  seen.set(value, clone); // Ghi nhớ trước khi đệ quy sâu hơn!

  for (const key of Object.keys(value)) {
    clone[key] = safeDeepClone(value[key], seen);
  }
  return clone;
}
```

---

## 📝 Phần 4 — Bộ Bài Tập Tự Luyện & Đề Tự Giải

### A. 4 Bài tập tự viết code (Từ Rất Dễ đến Vận dụng)

1. **Bài 1 (Rất Dễ):** Viết hàm `cloneAndAddTag(tags, newTag)` nhận mảng chuỗi `tags` và trả về một mảng mới có thêm `newTag` ở cuối mà không làm thay đổi mảng `tags` ban đầu.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Không dùng `tags.push(newTag)`! Hãy dùng toán tử Spread `[...tags, newTag]`.

```javascript
function cloneAndAddTag(tags, newTag) {
  return [...tags, newTag];
}
```
</details>

2. **Bài 2 (Dễ):** Viết hàm `cloneMatrix(matrix)` sao chép sâu một mảng 2 chiều các số (ví dụ `[[1, 2], [3, 4]]`) để khi sửa `copy[0][0] = 99` thì `matrix[0][0]` vẫn giữ nguyên.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** `[...matrix]` chỉ copy tầng ngoài. Cần dùng `.map(row => [...row])` để copy cả từng hàng bên trong!

```javascript
function cloneMatrix(matrix) {
  return matrix.map((row) => [...row]);
}
```
</details>

3. **Bài 3 (Trung bình):** Viết hàm `deepEqual(a, b)` kiểm tra xem 2 giá trị hoặc 2 object lồng nhau có giống nhau hoàn toàn về cấu trúc và dữ liệu hay không.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Nếu `a === b` trả `true`. Kiểm tra `null` hoặc khác `'object'` trả `false`. So sánh độ dài `Object.keys(a)` và `Object.keys(b)`, rồi gọi đệ quy cho từng key.

```javascript
function deepEqual(a, b) {
  if (a === b) return true;
  if (a === null || typeof a !== 'object' || b === null || typeof b !== 'object') return false;
  if (Array.isArray(a) !== Array.isArray(b)) return false;
  const keysA = Object.keys(a);
  const keysB = Object.keys(b);
  if (keysA.length !== keysB.length) return false;
  for (const key of keysA) {
    if (!Object.prototype.hasOwnProperty.call(b, key) || !deepEqual(a[key], b[key])) {
      return false;
    }
  }
  return true;
}
```
</details>

4. **Bài 4 (Vận dụng):** Tạo Custom Error `class QuotaExceededError extends Error` nhận `(message, limit)` và viết hàm `processRequest(used, limit)` ném lỗi này khi `used >= limit`; ở ngoài dùng `try/catch` kiểm tra bằng `instanceof`.
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Gợi ý:** Trong `constructor` của class con kế thừa `Error`, bắt buộc gọi `super(message)` trước khi gán `this.name` và `this.limit`.

```javascript
class QuotaExceededError extends Error {
  constructor(message, limit) {
    super(message);
    this.name = 'QuotaExceededError';
    this.limit = limit;
  }
}

function processRequest(used, limit) {
  if (used >= limit) {
    throw new QuotaExceededError('Đã vượt hạn mức sử dụng', limit);
  }
  return 'OK';
}
```
</details>

### B. 3 Câu hỏi tự kiểm tra tư duy

1. **Câu hỏi 1:** Vì sao `typeof null === 'object'` lại nguy hiểm khi viết hàm `deepClone`?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Nếu chỉ kiểm tra `if (typeof val !== 'object') return val;`, thì giá trị `null` sẽ lọt qua và đi vào lệnh `Object.keys(null)`, làm chương trình crash ngay lập tức với `TypeError: Cannot convert undefined or null to object`. Luôn kiểm tra `if (val === null || typeof val !== 'object')`.
</details>

2. **Câu hỏi 2:** Tại sao lệnh `seen.set(value, clone)` phải đặt **trước** vòng lặp gọi đệ quy `safeDeepClone(value[key], seen)`?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** Nếu đặt sau vòng lặp đệ quy, khi gặp `value.self = value`, lúc gọi đệ quy xuống thuộc tính `self`, `value` vẫn chưa có mặt trong `seen`! Do đó phải đăng ký vào `seen` ngay trước khi duyệt các thuộc tính con.
</details>

3. **Câu hỏi 3:** Nếu muốn phân biệt `ValidationError` với lỗi mạng bất ngờ trong khối `catch (err)`, dùng `typeof err === 'ValidationError'` có chạy đúng không?
<details>
<summary>Xem gợi ý & Lời giải chi tiết</summary>

**Trả lời:** **Không!** `typeof err` luôn trả về `'object'` cho mọi đối tượng lỗi. Để kiểm tra chính xác lớp lỗi kế thừa, phải dùng toán tử `err instanceof ValidationError`.
</details>

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Nhập môn kiểu tham chiếu Heap, Shallow vs Deep Clone và đệ quy có điểm dừng.",
                "Xử lý Circular Reference bằng WeakMap, structuredClone và Custom Error Class."
            ),

            // =========================================================================
            // 2. INVESTING LESSONS (1.1 .. 3.2)
            // =========================================================================
            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson11Id,
                "Bài 1.1: Sức Mạnh Lãi Kép & Chiến Lược DCA Định Kỳ",
                "suc-manh-lai-kep-va-chien-luoc-dca",
                25,
                @"# Bài 1.1: Sức Mạnh Lãi Kép & Chiến Lược DCA Định Kỳ

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Bạn không cần vốn lớn để bắt đầu. Chỉ cần gửi đều **2 triệu/tháng** trong 20 năm với lãi suất 10%/năm: tổng vốn bỏ ra chỉ là **480 triệu**, nhưng nhờ lãi mẹ đẻ lãi con (Lãi kép), tài sản cuối kỳ vượt ngưỡng **1,5 tỷ đồng**!

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững công thức toán học và nguyên lý tăng trưởng theo hàm mũ của Lãi kép trong tích lũy tài sản dài hạn.
- Hiểu rõ chiến lược Dollar-Cost Averaging (DCA - Trung bình giá) để triệt tiêu hoàn toàn áp lực phán đoán thời điểm thị trường (Market Timing).
- Phân biệt tác động của Lợi suất danh nghĩa và Lợi suất thực tế sau khi khấu trừ tỷ lệ Lạm phát.
- Xây dựng kỷ luật trích quỹ đầu tư tự động ngay khi có thu nhập đầu tháng (Pay Yourself First).

---

## 2. Khái niệm & Công thức Cốt lõi

### 2.1. Công thức Lãi Kép & Đóng Góp Định Kỳ (DCA)
$$FV = PV \times (1 + r)^n + PMT \times \frac{(1 + r)^n - 1}{r}$$
- $FV$: Giá trị tài sản tích lũy trong tương lai.
- $PV$: Vốn khởi điểm ban đầu.
- $PMT$: Số tiền tích lũy đều đặn mỗi chu kỳ (hàng tháng/hàng năm).
- $r$: Tỷ suất sinh lời bình quân mỗi chu kỳ.
- $n$: Tổng số chu kỳ tích lũy.

### 2.2. Lợi suất Thực tế vs Lạm phát
$$\text{Lợi suất thực tế} \approx \text{Lợi suất danh nghĩa} - \text{Tỷ lệ lạm phát}$$
Nếu bạn gửi tiết kiệm lãi suất 6%/năm nhưng lạm phát thực tế là 4%/năm, sức mua tài sản của bạn chỉ tăng trưởng thực 2%/năm. Nắm giữ tiền mặt thuần túy trong dài hạn đồng nghĩa với việc chấp nhận tài sản bị bào mòn mỗi năm.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
Hai người bạn An và Bình cùng đặt mục tiêu tích lũy hưu trí:
- **An:** Bắt đầu từ năm 22 tuổi, tích lũy đều đặn 3 triệu đ/tháng với lợi suất 10%/năm trong 10 năm (đến 32 tuổi), sau đó ngừng đóng tiền và để lãi kép tự vận hành đến năm 60 tuổi.
- **Bình:** Đến năm 32 tuổi mới bắt đầu, đóng đều 3 triệu đ/tháng liên tục suốt 28 năm (đến 60 tuổi).
- **Kết quả:** Dù An chỉ bỏ ra tổng vốn 360 triệu đồng còn Bình bỏ ra hơn 1 tỷ đồng, tài sản cuối cùng của An ở tuổi 60 vẫn vượt trội hơn Bình nhờ **yếu tố thời gian và sức mạnh lũy tiến của lãi kép**!

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Cố gắng bắt đáy và bán đỉnh (Market Timing) | Bỏ lỡ những phiên tăng mạnh nhất của thị trường, mua đỉnh bán đáy theo cảm xúc | Thực hiện DCA kỷ luật vào một ngày cố định hàng tháng bất kể tin tức thị trường |
| Chờ có số tiền lớn mới bắt đầu đầu tư | Lãng phí những năm tháng tuổi trẻ quý giá nhất - đòn bẩy lớn nhất của lãi kép | Bắt đầu ngay với số tiền nhỏ (500k - 1 triệu/tháng) để kích hoạt lãi kép sớm |
| Rút vốn giữa chừng để tiêu xài ngắn hạn | Ngắt quãng chu kỳ tăng trưởng mũ của lãi kép, quay lại vạch xuất phát | Tách bạch rõ Quỹ khẩn cấp 6 tháng chi phí riêng biệt với Danh mục đầu tư dài hạn |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Đã thiết lập lệnh chuyển khoản DCA tự động vào ngày nhận lương hàng tháng chưa?
- [ ] Quỹ dự phòng khẩn cấp (3-6 tháng chi phí thiết yếu) đã được tách riêng chưa?
- [ ] Mục tiêu tích lũy có tầm nhìn tối thiểu 5–15 năm không bị ảnh hưởng bởi biến động ngắn hạn?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Lãi kép và chiến lược DCA trung bình giá loại bỏ rủi ro tâm lý chọn thời điểm.",
                "Công thức toán học hàm mũ và bảo vệ sức mua trước lạm phát."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson12Id,
                "Bài 1.2: Kế Hoạch Tự Do Tài Chính (FIRE) & Quản Lý Dòng Tiền",
                "ke-hoach-tu-do-tai-chinh-fire-va-dong-tien",
                25,
                @"# Bài 1.2: Kế Hoạch Tự Do Tài Chính (FIRE) & Quản Lý Dòng Tiền

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Tự do tài chính không có nghĩa là trở thành tỷ phú, mà là khoảnh khắc **thu nhập thụ động từ danh mục đầu tư >= chi phí sống cơ bản**, cho phép bạn làm việc vì đam mê thay vì áp lực cơm áo!

## 1. Mục tiêu bài học (Learning Objectives)
- Thấu hiểu mô hình FIRE (Financial Independence, Retire Early) và Nghiên cứu Trinity (Trinity Study).
- Nắm vững Quy tắc 4% và công thức tính Con số Tự do Tài chính mục tiêu: $Tài\ sản = 25 \times Chi\ tiêu\ năm$.
- Xây dựng Bảng cân đối dòng tiền cá nhân (Thu nhập ròng - Chi phí thiết yếu - Mong muốn - Tích lũy).
- Xóa bỏ triệt để các khoản nợ xấu lãi suất cao (nợ thẻ tín dụng, vay tiêu dùng) bằng phương pháp Quả cầu tuyết hoặc Tuyết lở.

---

## 2. Khái niệm & Công thức Cốt lõi

### 2.1. Quy tắc 4% (Nghiên cứu Trinity)
Quy tắc 4% chỉ ra rằng nếu bạn sở hữu danh mục đầu tư đa dạng hóa (ví dụ: 60% cổ phiếu + 40% trái phiếu), bạn có thể rút **4% tổng giá trị danh mục** trong năm đầu tiên và điều chỉnh theo lạm phát ở các năm tiếp theo mà xác suất cạn vốn trong vòng 30 năm là dưới 5%.
$$\text{Số tiền FIRE mục tiêu} = \frac{\text{Chi tiêu hàng năm}}{4\%} = \text{Chi tiêu hàng năm} \times 25$$

### 2.2. Tỷ lệ Tiết kiệm Ròng (Savings Rate)
Thời gian để bạn đạt tự do tài chính không phụ thuộc vào việc bạn kiếm được bao nhiêu tiền, mà phụ thuộc vào **tỷ lệ phần trăm thu nhập bạn tiết kiệm và đầu tư được**:
- Tiết kiệm 10%: Cần khoảng 51 năm làm việc.
- Tiết kiệm 30%: Cần khoảng 28 năm.
- Tiết kiệm 50%: Chỉ cần khoảng 17 năm để độc lập tài chính hoàn toàn!

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
Gia đình chị Hà có chi tiêu thiết yếu và sinh hoạt là 20 triệu đ/tháng (240 triệu đ/năm):
1. **Con số FIRE mục tiêu:** $240\ \text{triệu} \times 25 = 6\ \text{tỷ đồng}$.
2. **Kế hoạch hành động:**
   - Với mức thu nhập 35 triệu đ/tháng, tối ưu chi tiêu giữ ở mức 20 triệu, tỷ lệ tiết kiệm đạt $15 / 35 \approx 43\%$.
   - DCA đều đặn 15 triệu đ/tháng vào quỹ chỉ số với lợi suất kỳ vọng 10%/năm.
   - Sau khoảng 15–16 năm, danh mục tích lũy đạt mốc 6 tỷ đồng, mang lại dòng tiền an toàn 240 triệu đ/năm mà không cần đi làm thuê.

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Lạm phát lối sống (Lifestyle Inflation) | Thu nhập tăng bao nhiêu thì chi tiêu tăng bấy nhiêu, không bao giờ tích lũy được | Cố định mức chi tiêu cơ bản, dành ít nhất 50% số tiền tăng lương đưa thẳng vào quỹ đầu tư |
| Giữ nợ thẻ tín dụng 25–30%/năm trong khi cố đi lướt sóng | Lãi vay bào mòn toàn bộ lợi nhuận, rủi ro vỡ nợ cao | Trả dứt điểm nợ thẻ tín dụng trước vì đó là tỷ suất sinh lời phi rủi ro 25–30% chắc chắn |
| Đặt mục tiêu FIRE quá xa vời không khả thi | Nhanh chóng nản lòng và từ bỏ kỷ luật tài chính | Chia nhỏ thành các cột mốc vi mô: FIRE 3 tháng -> 1 năm -> 5 năm -> Tự do hoàn toàn |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Đã tính chính xác chi tiêu hàng năm và Con số FIRE mục tiêu của bản thân chưa?
- [ ] Đã trả sạch toàn bộ các khoản nợ lãi suất cao trên 12%/năm chưa?
- [ ] Tỷ lệ tiết kiệm và đầu tư hàng tháng có đạt tối thiểu 20% thu nhập ròng không?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Kế hoạch tự do tài chính FIRE, nghiên cứu Trinity và quy tắc 4%.",
                "Tối ưu hóa tỷ lệ tiết kiệm ròng và dứt điểm các khoản nợ lãi cao."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson21Id,
                "Bài 2.1: Phân Bổ Tài Sản & Tương Quan Giữa Các Lớp Tài Sản",
                "phan-bo-tai-san-va-tuong-quan",
                25,
                @"# Bài 2.1: Phân Bổ Tài Sản & Tương Quan Giữa Các Lớp Tài Sản

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Đừng bao giờ bỏ tất cả trứng vào một giỏ — và cũng đừng bỏ vào 10 giỏ cùng nằm trên một chiếc xe tải! Phân bổ tài sản đúng là kết hợp các lớp tài sản chuyển động **ngược chiều nhau** khi bão kinh tế ập đến.

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững Lý thuyết Danh mục Hiện đại (Modern Portfolio Theory - MPT) của Harry Markowitz (giải Nobel Kinh tế).
- Hiểu sâu khái niệm Hệ số tương quan (Correlation Coefficient $r$) giữa các lớp tài sản (-1 đến +1).
- Thiết lập cơ cấu phân bổ danh mục cân bằng đa tài sản (Cổ phiếu, Trái phiếu, Vàng, Tiền mặt).
- Nhận thức rõ nguyên lý: Phân bổ tài sản quyết định hơn 90% mức độ biến động và hiệu quả danh mục dài hạn.

---

## 2. Khái niệm & Mô hình Cốt lõi

### 2.1. Hệ số Tương quan và Biên giới Hiệu quả (Efficient Frontier)
- **Hệ số tương quan ($r = +1$):** Hai tài sản biến động cùng chiều hoàn hảo (không có tác dụng giảm rủi ro).
- **Hệ số tương quan ($r \le 0$):** Khi một tài sản giảm giá, tài sản kia giữ giá hoặc tăng giá, giúp triệt tiêu độ biến động danh mục mà không làm suy giảm lợi nhuận kỳ vọng dài hạn.

### 2.2. Đặc tính 4 Lớp Tài sản Cốt lõi
1. **Cổ phiếu (Stocks):** Động cơ sinh lời chính, bảo vệ khỏi lạm phát dài hạn nhưng biến động mạnh ngắn hạn.
2. **Trái phiếu (Bonds):** Dòng tiền trả lãi định kỳ ổn định, đóng vai trò tấm đệm phòng thủ khi kinh tế suy thoái.
3. **Vàng (Gold):** Nơi trú ẩn an toàn trước khủng hoảng địa chính trị và lạm phát phi mã ($r$ thấp với cổ phiếu).
4. **Tiền mặt / Tiết kiệm ngắn hạn:** Đảm bảo thanh khoản tức thì để bắt đáy khi thị trường hoảng loạn.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
Mô hình Danh mục Mọi Thời tiết (All-Weather Portfolio) của Ray Dalio:
- **30% Cổ phiếu tăng trưởng:** Hưởng lợi khi nền kinh tế tăng trưởng mạnh.
- **40% Trái phiếu dài hạn:** Tăng giá mạnh khi lãi suất giảm và kinh tế suy thoái.
- **15% Trái phiếu trung hạn:** Duy trì thanh khoản và giảm sốc danh mục.
- **7.5% Vàng + 7.5% Hàng hóa:** Phòng thủ lạm phát tăng cao bất ngờ.
- **Kết quả:** Trong các cuộc khủng hoảng tài chính lịch sử, danh mục này chỉ giảm nhẹ dưới 5–10%, trong khi thị trường chứng khoán giảm tới 40–50%!

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| ""Đa dạng hóa giả tạo"": Mua 10 mã cổ phiếu bất động sản khác nhau | Cùng một ngành nên cùng giảm sàn khi chính sách tín dụng thắt chặt | Phân bổ chéo qua nhiều ngành nghề và các lớp tài sản có tương quan âm |
| Dồn 100% tài sản vào cổ phiếu khi tuổi đã cao | Khi thị trường sụt giảm 40% sát tuổi hưu, không có đủ thời gian để chờ phục hồi | Tuân thủ quy tắc ngón tay cái: Tỷ trọng cổ phiếu $\approx 110 - \text{Độ tuổi}$ |
| Bỏ qua tài sản phòng thủ vì chê lãi suất thấp | Danh mục rơi tự do khi khủng hoảng ập đến, dẫn đến bán tháo hoảng loạn ngay đáy | Luôn duy trì tỷ trọng tối thiểu 15–20% cho trái phiếu và tiền mặt dự phòng |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Danh mục hiện tại đã có ít nhất 2 lớp tài sản có hệ số tương quan thấp hoặc âm chưa?
- [ ] Tỷ trọng phân bổ có phù hợp với độ tuổi và khẩu vị chịu đựng rủi ro cá nhân không?
- [ ] Có kế hoạch tái cân bằng định kỳ hàng năm khi tỷ trọng tài sản bị lệch quá 5% không?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Lý thuyết danh mục hiện đại Harry Markowitz và hệ số tương quan đa tài sản.",
                "Thiết kế danh mục All-Weather phòng thủ khủng hoảng và kiểm soát biến động."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson22Id,
                "Bài 2.2: Quản Trị Rủi Ro & Mức Sụt Giảm Tối Đa (Max Drawdown)",
                "quan-tri-rui-ro-va-max-drawdown",
                25,
                @"# Bài 2.2: Quản Trị Rủi Ro & Mức Sụt Giảm Tối Đa (Max Drawdown)

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Nếu tài khoản 100 triệu bị lỗ **50%** còn 50 triệu, bạn phải lời tới **100%** (tăng gấp đôi từ 50 triệu) mới về lại điểm hòa vốn! Bảo vệ vốn luôn quan trọng hơn việc tìm kiếm siêu lợi nhuận nhất thời.

## 1. Mục tiêu bài học (Learning Objectives)
- Đo lường chính xác các chỉ số rủi ro danh mục: Mức sụt giảm tối đa (Max Drawdown - MDD), Độ lệch chuẩn (Volatility) và Tỷ số Sharpe.
- Thấu hiểu định luật toán học về sự bất đối xứng giữa Tỷ lệ Lỗ và Tỷ lệ Lời cần thiết để hòa vốn.
- Thực hành Kiểm thử áp lực danh mục (Stress Testing) qua các kịch bản khủng hoảng lịch sử.
- Thiết lập kỷ luật Tái cân bằng danh mục định kỳ (Portfolio Rebalancing) bán cao mua thấp tự động.

---

## 2. Khái niệm & Công thức Cốt lõi

### 2.1. Mức Sụt Giảm Tối Đa (Max Drawdown)
$$MDD = \frac{\text{Giá trị Đáy} - \text{Giá trị Đỉnh}}{\text{Giá trị Đỉnh}} \times 100\%$$
MDD phản ánh nỗi đau tâm lý lớn nhất mà nhà đầu tư phải trải qua trong một giai đoạn nắm giữ tài sản.

### 2.2. Sự Bất Đối Xứng của Khoản Lỗ (Math of Loss)
$$\text{Mức tăng để hòa vốn} = \frac{L}{1 - L}$$
- Lỗ **10%** $\rightarrow$ Cần tăng **11.1%** để hòa vốn.
- Lỗ **20%** $\rightarrow$ Cần tăng **25.0%** để hòa vốn.
- Lỗ **50%** $\rightarrow$ Cần tăng **100.0%** (nhân đôi tài sản) chỉ để về lại điểm xuất phát!
- Lỗ **80%** $\rightarrow$ Cần tăng **400.0%** (gấp 5 lần) để gỡ gạc!
> Do đó, bảo toàn vốn và giới hạn Max Drawdown là mục tiêu tối thượng trong quản trị danh mục.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
Kỷ luật Tái cân bằng (Rebalancing) theo tỷ trọng mục tiêu 60% Cổ phiếu / 40% Trái phiếu:
- Sau một năm tăng trưởng nóng, giá cổ phiếu tăng vọt làm cơ cấu lệch thành 75% Cổ phiếu / 25% Trái phiếu.
- **Hành động tái cân bằng:** Bán bớt cổ phiếu ở vùng giá cao để chuyển 15% sang mua trái phiếu đưa về mức 60/40.
- **Hành vi vô thức tích cực:** Nhà đầu tư đã chủ động **hiện thực hóa lợi nhuận khi tài sản tăng nóng** và gom mua thêm tài sản an toàn giá rẻ một cách hoàn toàn tự động và kỷ luật, loại bỏ cảm xúc tham lam.

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Gồng lỗ không giới hạn với hy vọng giá sẽ hồi phục | Khoản lỗ 50% triệt tiêu năng lượng tinh thần và bào mòn vốn liếng vĩnh viễn | Luôn xác định trước ngưỡng cắt lỗ tối đa hoặc biên an toàn trước khi vào lệnh |
| Bỏ qua kiểm thử áp lực kịch bản xấu nhất (Stress Test) | Hoảng loạn bán tháo ngay đáy khi thị trường sụt giảm mạnh bất ngờ | Kiểm tra trước khả năng chịu đựng của bản thân nếu danh mục sụt giảm 25–35% |
| Tái cân bằng quá thường xuyên theo ngày/tuần | Tốn kém phí giao dịch và thuế chuyển nhượng, làm giảm hiệu suất ròng | Chỉ tái cân bằng theo chu kỳ 6–12 tháng hoặc khi tỷ trọng lệch vượt ngưỡng dải biên độ $\pm 5\%$ |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Max Drawdown danh mục của bạn có nằm trong ngưỡng chịu đựng tâm lý ($\le 20\%$) không?
- [ ] Đã có kế hoạch tái cân bằng danh mục theo chu kỳ cố định chưa?
- [ ] Có khoản vay ký quỹ (margin) nguy hiểm nào đang đe dọa bị giải chấp (force sell) không?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Đo lường mức sụt giảm tối đa Max Drawdown và toán học hòa vốn bất đối xứng.",
                "Quy trình tái cân bằng danh mục và kiểm thử áp lực Stress Testing."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson31Id,
                "Bài 3.1: Biên An Toàn & Định Giá Doanh Nghiệp Cơ Bản",
                "bien-an-toan-va-dinh-gia-doanh-nghiep",
                30,
                @"# Bài 3.1: Biên An Toàn & Định Giá Doanh Nghiệp Cơ Bản

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Khi xây một cây cầu chịu được xe tải 30 tấn, kỹ sư luôn treo biển giới hạn **10 tấn**. Khoảng đệm 20 tấn dư ra đó chính là **Biên an toàn (Margin of Safety)** — nguyên tắc vàng giúp nhà đầu tư giá trị sống sót qua mọi biến cố!

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững triết lý Biên an toàn (Margin of Safety - MoS) của Benjamin Graham và Warren Buffett.
- Đọc hiểu và phân tích nhanh các chỉ số định giá tài chính cốt lõi: P/E, P/B, ROE, Nợ ròng / EBITDA và Free Cash Flow.
- Phân biệt rõ ranh giới giữa Biến động giá thị trường (Price) và Giá trị nội tại thực của doanh nghiệp (Value).
- Thiết lập checklist thẩm định doanh nghiệp chất lượng cao sở hữu con hào kinh tế bền vững (Economic Moat).

---

## 2. Khái niệm & Công thức Cốt lõi

### 2.1. Khái niệm Biên An Toàn (Margin of Safety)
Biên an toàn là khoảng chiết khấu giữa Giá thị trường của cổ phiếu so với Giá trị nội tại ước tính thận trọng của doanh nghiệp:
$$\text{Biên an toàn (\%)} = \frac{\text{Giá trị nội tại} - \text{Giá thị trường}}{\text{Giá trị nội tại}} \times 100\%$$
- Biên an toàn bảo vệ nhà đầu tư khỏi: (1) Sai sót trong các giả định tương lai; (2) Rủi ro biến động kinh tế vĩ mô bất ngờ; (3) Vận đen thị trường.

### 2.2. Các Bộ Chỉ số Định Giá Cơ Bản
1. **P/E (Price-to-Earnings):** Giá thị trường trên lợi nhuận mỗi cổ phiếu. Phản ánh số năm thu hồi vốn nếu lợi nhuận giữ nguyên.
2. **P/B (Price-to-Book):** Giá thị trường trên giá trị sổ sách của tài sản ròng.
3. **ROE (Return on Equity):** Lợi nhuận trên vốn chủ sở hữu. Doanh nghiệp xuất sắc thường duy trì $ROE > 15–20\%$ bền vững nhiều năm.
4. **FCF (Free Cash Flow - Dòng tiền tự do):** Dòng tiền từ hoạt động kinh doanh trừ chi phí vốn (CapEx). Tiền thật sau khi đã tái đầu tư.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
Phân tích cổ phiếu Công ty Bán lẻ Đầu ngành XYZ:
- Giá trị nội tại tính theo phương pháp chiết khấu dòng tiền thận trọng: 100.000 đ/cổ phiếu.
- Thị trường hoảng loạn vì tin tức vĩ mô ngắn hạn khiến giá cổ phiếu giảm về 70.000 đ/cổ phiếu.
- **Biên an toàn hiện tại:** $(100.000 - 70.000) / 100.000 = 30\%$.
- Doanh nghiệp không có nợ vay ngắn hạn, biên lợi nhuận ròng ổn định, tiền mặt dồi dào.
- **Quyết định đầu tư:** Mức chiết khấu 30% đem lại biên an toàn rất vững chắc để giải ngân tích lũy từng phần.

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Cổ phiếu rẻ là cổ phiếu có P/E thấp (""Bẫy giá trị"" - Value Trap) | Mua phải doanh nghiệp đang trên đà phá sản hoặc ngành nghề suy thoái vĩnh viễn | Kết hợp P/E với tăng trưởng doanh thu, chất lượng tài sản và triển vọng con hào kinh tế |
| Đánh đồng Giá cả biến động hàng ngày với Giá trị doanh nghiệp | Lo sợ bán tháo khi giá giảm trong khi hoạt động kinh doanh của công ty vẫn tăng trưởng tốt | Nhớ lời Warren Buffett: ""Giá là thứ bạn trả, giá trị là thứ bạn nhận được"" |
| Không trừ chi phí vốn CapEx khi nhìn lợi nhuận kế toán | Lợi nhuận ghi nhận trên giấy tờ cao nhưng doanh nghiệp không có tiền mặt trả cổ tức | Luôn kiểm tra kỹ Báo cáo Lưu chuyển Tiền tệ và Dòng tiền Tự do (FCF) |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Doanh nghiệp có duy trì $ROE > 15\%$ liên tục trong ít nhất 3–5 năm qua không?
- [ ] Tỷ lệ Nợ vay / Vốn chủ sở hữu có nằm trong ngưỡng an toàn lành mạnh ($< 1.0$) không?
- [ ] Mức giá mua hiện tại có biên an toàn tối thiểu 20–30% so với giá trị nội tại không?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Nguyên lý Biên an toàn Benjamin Graham và các chỉ số định giá P/E, ROE, FCF.",
                "Tránh bẫy giá trị và thẩm định con hào kinh tế của doanh nghiệp."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson32Id,
                "Bài 3.2: Tâm Lý Học Hành Vi & Quản Trị Cảm Xúc Trong Đầu Tư",
                "tam-ly-hoc-hanh-vi-va-quan-tri-cam-xuc",
                25,
                @"# Bài 3.2: Tâm Lý Học Hành Vi & Quản Trị Cảm Xúc Trong Đầu Tư

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Kẻ thù lớn nhất của nhà đầu tư không nằm trên bảng điện tử mà nằm trong chính gương soi! Kiểm soát được nỗi sợ FOMO khi thị trường tăng nóng và hoảng loạn khi thị trường sập đã giúp bạn chiến thắng 80% đám đông.

## 1. Mục tiêu bài học (Learning Objectives)
- Thấu hiểu các phát hiện then chốt của Tài chính Hành vi (Behavioral Finance).
- Nhận diện 5 thiên kiến tâm lý kinh điển: FOMO, Loss Aversion, Anchoring, Sunk Cost Fallacy và Tâm lý Bầy đàn (Herd Mentality).
- Rèn luyện Tư duy Ngược dòng (Contrarian Thinking) dựa trên dữ liệu khách quan thay vì cảm xúc đám đông.
- Xây dựng Bản quy tắc Checklist trước khi bấm lệnh giao dịch và ghi chép Nhật ký Quyết định Đầu tư 4 trụ cột.

---

## 2. Khái niệm & Mô hình Cốt lõi

```
[Thị trường Tăng Nóng] --> FOMO / Hưng phấn --> Mua đuổi ở Đỉnh
         ^                                             |
         |                                             v
Lãi kép Kỷ luật                                 [Thị trường Điều chỉnh]
         ^                                             |
         |                                             v
Gom mua DCA Rẻ <--- Hoảng loạn / Tháo chạy <--- Nỗi đau Sợ thua lỗ
```

### 2.1. Năm Thiên kiến Tâm lý Nhà Đầu tư Cần Khắc Phục
1. **FOMO (Fear of Missing Out):** Nỗi sợ bỏ lỡ cơ hội khi thấy người khác khoe lãi, dẫn đến mua đuổi tài sản đã tăng giá quá nóng.
2. **Loss Aversion (Sợ thua lỗ):** Nỗi đau mất 10 triệu đồng lớn gấp 2–2.5 lần niềm vui kiếm được 10 triệu đồng, khiến nhà đầu tư chối bỏ thực tế và ôm giữ tài sản mục nát.
3. **Anchoring (Hiệu ứng Mỏ neo):** Bị neo chặt vào mức giá mua trong quá khứ và quyết tâm gồng lỗ cho tới khi hòa vốn mới bán, dù triển vọng doanh nghiệp đã thay đổi hoàn toàn.
4. **Sunk Cost Fallacy (Ảo tưởng chi phí chìm):** Cố đấm ăn xôi ném thêm tiền vào một vị thế sai lầm vì tiếc số tiền và thời gian đã bỏ ra trước đó.
5. **Herd Mentality (Tâm lý Bầy đàn):** Cảm thấy an tâm giả tạo khi làm giống số đông trên các hội nhóm diễn đàn thay vì tự mình thẩm định độc lập.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
Năm 2022, khi thị trường chứng khoán toàn cầu giảm sâu hơn 30%:
- **Nhà đầu tư theo tâm lý đám đông:** Hoảng loạn bán sạch toàn bộ danh mục ngay vùng đáy do bị chi phối bởi truyền thông bi quan và thiên kiến sợ thua lỗ.
- **Nhà đầu tư ngược dòng có kỷ luật:** Kiểm tra lại luận điểm đầu tư, nhận thấy doanh nghiệp vẫn tạo dòng tiền dồi dào, biên an toàn mở rộng lên mức kỷ lục 40%. Họ tiếp tục duy trì lệnh DCA định kỳ đều đặn, mua gom được lượng lớn cổ phiếu giá rẻ và đạt lợi nhuận bùng nổ khi chu kỳ hồi phục bắt đầu.

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Mở bảng điện tử theo dõi từng giây trong phiên | Cảm xúc bị cuốn theo nhịp giật giá xanh đỏ, dẫn đến giao dịch bốc đồng liên tục | Giới hạn xem bảng giá tối đa 1 lần/ngày hoặc chỉ kiểm tra vào cuối tuần |
| Mua bán theo tin đồn rỉ tai trên các hội nhóm mạng xã hội | Trở thành người đổ vỏ thanh khoản cuối cùng cho các đội lái giá | Luôn đặt câu hỏi: ""Nếu tin này thật sự tốt, tại sao họ lại chia sẻ miễn phí cho mình?"" |
| Không ghi lại lý do tại sao mua và khi nào sẽ bán | Khi thị trường biến động, không có căn cứ để đánh giá đúng sai, hành động theo bản năng sợ hãi | Bắt buộc viết Nhật ký Quyết định Đầu tư 4 trụ cột trước khi giải ngân |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Quyết định mua này có đang bị thúc đẩy bởi lòng tham hay nỗi sợ FOMO không?
- [ ] Nếu giá giảm thêm 20% vào ngày mai, bạn có vui vẻ mua thêm không? (Nếu không, bạn chưa hiểu giá trị tài sản).
- [ ] Đã ghi chép đầy đủ 4 trụ cột (Luận điểm, Định giá, Kịch bản sai, Kỷ luật cảm xúc) vào Nhật ký Quyết định chưa?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Tài chính hành vi, khắc phục 5 thiên kiến tâm lý kinh điển của nhà đầu tư.",
                "Tư duy ngược dòng Contrarian và Nhật ký quyết định đầu tư 4 trụ cột."
            ),

            // =========================================================================
            // 3. PSYCHOLOGY LESSONS (1.1 .. 3.2)
            // =========================================================================
            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson11Id,
                "Bài 1.1: Xử Lý Thông Tin Tự Động vs Có Chủ Đích (Hệ Thống 1 & 2)",
                "he-thong-1-va-2-nhan-thuc",
                25,
                @"# Bài 1.1: Xử Lý Thông Tin Tự Động vs Có Chủ Đích (Hệ Thống 1 & 2)

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Não bộ bạn có 2 ""nhân viên"": **Hệ thống 1** phản xạ chớp nhoáng (tránh xe lao tới, tức giận ngay khi bị góp ý) và **Hệ thống 2** điềm tĩnh phân tích (tính toán, đối chiếu dữ kiện). Làm chủ cảm xúc chính là tập thói quen **dừng 5 giây** để Hệ thống 2 kịp lên tiếng!

## 1. Mục tiêu bài học (Learning Objectives)
- Thấu hiểu mô hình Hai hệ thống nhận thức của Daniel Kahneman (Hệ thống 1: Nhanh, trực giác; Hệ thống 2: Chậm, logic).
- Giải mã cơ chế nhiễu phản xạ qua Thí nghiệm Hiệu ứng Stroop (Stroop Interference Effect).
- Áp dụng nguyên lý Giao tiếp phi bạo lực (Nonviolent Communication - NVC) 4 bước: Quan sát - Cảm xúc - Nhu cầu - Đề nghị.
- Tự rèn luyện kỹ thuật ""Dừng 5 giây"" để kích hoạt Hệ thống 2 trước các phản hồi dễ gây xung đột.

---

## 2. Khái niệm & Mô hình Cốt lõi

| Đặc điểm so sánh | Hệ thống 1 (Nhanh & Tự động) | Hệ thống 2 (Chậm & Nỗ lực) |
|---|---|---|
| **Tốc độ xử lý** | Tức thì, tính bằng phần trăm giây | Chậm chạp, đòi hỏi sự tập trung tuần tự |
| **Mức độ tiêu hao năng lượng** | Rất thấp, chạy nền liên tục | Rất cao, tiêu thụ nhiều glucose não bộ |
| **Cơ chế vận hành** | Theo thói quen, cảm xúc, định kiến | Phân tích logic, tính toán, kiểm chứng |
| **Ví dụ điển hình** | Né tránh vật thể bay tới, đọc chữ quen | Tính nhẩm $17 \times 24$, so sánh hợp đồng pháp lý |

### 2.1. Hiệu Ứng Stroop
Khi từ ngữ ""ĐỎ"" được in bằng mực màu Xanh dương, thời gian phản xạ để đọc màu mực bị chậm lại rõ rệt. Nguyên nhân là do kỹ năng đọc chữ của Hệ thống 1 diễn ra tự động và nhanh hơn việc gọi tên màu sắc, tạo ra xung đột nhận thức mà Hệ thống 2 phải nỗ lực can thiệp để ức chế.

### 2.2. Khung Giao Tiếp NVC 4 Bước (Marshall Rosenberg)
1. **Quan sát khách quan (Observation):** Nêu sự kiện cụ thể như một camera ghi lại, không phán xét, không phóng đại.
2. **Cảm xúc chân thật (Feeling):** Nói rõ cảm xúc nội tại của bạn (lo lắng, thất vọng, bất an) thay vì dùng từ ngụy cảm xúc để buộc tội.
3. **Nhu cầu cốt lõi (Need):** Bày tỏ giá trị hoặc nhu cầu chung bạn đang cần (sự an tâm, tôn trọng tiến độ, hợp tác).
4. **Đề nghị cụ thể (Request):** Đưa ra yêu cầu hành động khả thi, rõ ràng về thời gian và mang tính xây dựng.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
Tình huống đồng đội nộp phần việc trễ hạn:
- **Phản xạ tự động Hệ thống 1 (Gây phòng thủ):** *""Lúc nào cậu cũng vô trách nhiệm và làm chậm trễ cả nhóm!""* $\rightarrow$ Đối phương lập tức kích hoạt cơ chế tự vệ (Fight-or-Flight) và phản pháo.
- **Phản hồi có chủ đích Hệ thống 2 qua NVC:** *""Mình thấy bản slide được gửi lúc 23h30 thay vì 18h như nhóm đã thống nhất (Quan sát). Mình cảm thấy lo lắng (Cảm xúc) vì sáng mai 8h nhóm phải thuyết trình và cần thời gian ráp bài chỉn chu (Nhu cầu). Cậu có thể gửi ngay dàn ý các ý chính trong 20 phút nữa để mình hỗ trợ format không (Đề nghị)?""*

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Trộn lẫn Phán xét vào Quan sát: *""Cậu lại lười biếng rồi""* | Người nghe cảm thấy bị tấn công nhân phẩm và lập tức dựng rào cản phòng vệ | Chỉ nêu số liệu, mốc giờ và hành vi thực tế có thể kiểm chứng khách quan |
| Dùng ngụy cảm xúc để đổ lỗi: *""Tôi cảm thấy bị coi thường""* | Thực chất đây là suy diễn hành vi của người khác chứ không phải nói cảm xúc của mình | Bày tỏ cảm xúc thật từ chính mình: *""Tôi cảm thấy buồn và bất an...""* |
| Đưa ra yêu cầu mơ hồ: *""Hãy làm việc chuyên nghiệp hơn đi""* | Đối phương không biết cụ thể cần thay đổi điều gì để đáp ứng | Đưa ra đề nghị hành động cụ thể, đo lường được: *""Gửi báo cáo trước 17h thứ Sáu""* |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Bạn đã tạm dừng ít nhất 5 giây để Hệ thống 2 kịp khởi động trước khi phản hồi căng thẳng chưa?
- [ ] Lời nói của bạn có đang tách bạch rõ ràng giữa Sự kiện thực tế và Phán xét cá nhân không?
- [ ] Câu giao tiếp có đầy đủ 4 thành tố NVC (Quan sát - Cảm xúc - Nhu cầu - Đề nghị) không?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Mô hình hai hệ thống tư duy Kahneman và hiệu ứng can thiệp Stroop.",
                "Giao tiếp phi bạo lực NVC 4 bước và kỹ thuật kích hoạt Hệ thống 2."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson12Id,
                "Bài 1.2: Ảo Tưởng Trí Nhớ & Thiên Kiến Xác Nhận (Confirmation Bias)",
                "ao-tuong-tri-nho-va-thien-kien-xac-nhan",
                25,
                @"# Bài 1.2: Ảo Tưởng Trí Nhớ & Thiên Kiến Xác Nhận (Confirmation Bias)

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Trí nhớ con người không hoạt động như camera quay phim lưu nguyên bản, mà giống như một ""biên tập viên"" tự lắp ghép chi tiết mới mỗi lần nhớ lại. Vì vậy tự hỏi *""Dữ liệu nào chứng minh mình có thể đang sai?""* là liều vắc-xin tốt nhất chống mù quáng!

## 1. Mục tiêu bài học (Learning Objectives)
- Thấu hiểu cơ chế Tái tạo ký ức (Memory Reconstruction): Não bộ không phải máy ghi hình mà tự điền khuyết bằng định kiến.
- Nhận diện biểu hiện của Thiên kiến Xác nhận (Confirmation Bias) trong học tập, công việc và tranh biện xã hội.
- Rèn luyện phương pháp Bác bỏ giả thuyết (Falsification Method) của Karl Popper.
- Sử dụng kỹ thuật Lập luận Tượng Thép (Steelman Argument) để nâng cao năng lực tư duy phản biện đa chiều.

---

## 2. Khái niệm & Mô hình Cốt lõi

### 2.1. Bản chất của Thiên Kiến Xác Nhận
Thiên kiến xác nhận là xu hướng tâm lý tìm kiếm, diễn giải và ghi nhớ có chọn lọc những thông tin củng cố niềm tin hoặc giả định có sẵn của bản thân, đồng thời phớt lờ, coi nhẹ hoặc chủ động bác bỏ các dữ kiện trái chiều.
- **Hệ quả nguy hiểm:** Càng thu thập thông tin một chiều, niềm tin mù quáng càng được củng cố (Echo Chamber / Buồng vang thông tin).

### 2.2. Kỹ thuật Phản biện ""Steelman"" (Đối lập với Strawman)
- **Rơm (Strawman):** Bóp méo hoặc làm suy yếu lập luận của đối phương để dễ dàng đập tan.
- **Thép (Steelman):** Chủ động xây dựng lại lập luận của đối phương ở phiên bản mạnh mẽ, chặt chẽ và thuyết phục nhất có thể, sau đó mới tìm điểm mù. Đây là cảnh giới cao nhất của tư duy phản biện khoa học.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
Trong tranh luận về giải pháp kiến trúc phần mềm giữa Microservices và Monolith:
- **Kỹ sư bị chi phối bởi Thiên kiến xác nhận:** Chỉ tìm kiếm các bài viết ca ngợi Microservices của Netflix, bỏ qua hoàn toàn các bài báo cáo chi phí vận hành tăng vọt và độ phức tạp phân tán từ các công ty có quy mô tương đương mình.
- **Kỹ sư áp dụng phương pháp Bác bỏ:** Chủ động tự hỏi: *""Những điều kiện hoặc số liệu tải thực tế nào sẽ chứng minh việc chuyển sang Microservices là một thảm họa tốn kém cho công ty?""* Nhờ vậy, nhóm tránh được quyết định đầu tư lãng phí hàng trăm nghìn USD.

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Chỉ đặt câu hỏi xác nhận: *""Điều này có đúng không?""* | Não bộ lập tức quét tìm các bằng chứng khẳng định có sẵn | Đổi câu hỏi: *""Dữ liệu khách quan nào có thể chứng minh mình đang sai?""* |
| Xem tranh luận là cuộc chiến thắng - thua của cái tôi | Cố thủ bảo vệ quan điểm ban đầu dù sự thật đã chứng minh ngược lại | Tách cái tôi ra khỏi giả thuyết: Thay đổi quan điểm khi có dữ liệu mới là dấu hiệu của trí tuệ |
| Quá tin tưởng vào sự chính xác của ký ức bản thân | Tự tin tuyệt đối về một chi tiết trong quá khứ mà thực chất đã bị bóp méo | Luôn đối chiếu với văn bản, email và biên bản ghi chép thực tế |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Bạn đã tìm kiếm ít nhất 2 lập luận mạnh nhất phản bác niềm tin hiện tại của mình chưa?
- [ ] Bạn có thể tóm tắt lại quan điểm của người bất đồng với bạn khiến chính họ cũng gật đầu đồng ý không?
- [ ] Quyết định quan trọng có được dựa trên bằng chứng dữ liệu khách quan thay vì cảm tính không?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Ảo tưởng trí nhớ và thiên kiến xác nhận cản trở tư duy phản biện.",
                "Phương pháp bác bỏ giả thuyết Karl Popper và kỹ thuật Steelman."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson21Id,
                "Bài 2.1: Dung Lượng Trí Nhớ Ngắn Hạn & Sức Mạnh Kỹ Thuật Chunking",
                "dung-luong-tri-nho-va-ky-thuat-chunking",
                25,
                @"# Bài 2.1: Dung Lượng Trí Nhớ Ngắn Hạn & Sức Mạnh Kỹ Thuật Chunking

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Hãy thử nhớ dãy `0 9 1 2 3 4 5 6 7 8` (rất khó vì có 10 số lẻ), nhưng khi gom thành `0912 - 345 - 678` thì nhớ ngay trong 2 giây! Đó chính là **Chunking** — bí quyết giúp bộ não ghi nhớ gấp 3 lần giới hạn tự nhiên.

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững Quy luật kỳ diệu của George Miller: Giới hạn dung lượng Bộ nhớ làm việc (Working Memory) trong $7 \pm 2$ đơn vị (chunks).
- Hiểu cơ chế gom cụm thông tin có nghĩa (Chunking) để vượt qua giới hạn vật lý của não bộ.
- Đo lường Digit Span / Memory Span cá nhân và kiểm chứng hiệu quả mở rộng ghi nhớ qua bài test tương tác.
- Tích hợp nguyên lý Chunking vào cấu trúc thẻ Flashcard FSRS v5 để tối ưu hóa việc học sâu.

---

## 2. Khái niệm & Mô hình Cốt lõi

### 2.1. Giới hạn của Working Memory (Bộ nhớ Làm việc)
Working Memory giống như thanh RAM của máy tính: dung lượng cực kỳ hạn chế và thông tin bị xóa rất nhanh nếu không được xử lý tích cực. Nghiên cứu kinh điển của George Miller (1956) và các nghiên cứu thần kinh học hiện đại chỉ ra não bộ con người chỉ có thể giữ cùng lúc khoảng 4 đến 7 đơn vị thông tin độc lập.

### 2.2. Kỹ thuật Chunking (Gom Cụm Thông Tin)
Chunking là quá trình tổ chức các mẩu thông tin rời rạc thành các khối (chunks) lớn hơn, có ý nghĩa và liên kết logic với kiến thức sẵn có trong Trí nhớ dài hạn (Long-term Memory).
- Nhớ chuỗi số ngẫu nhiên: `0 9 1 2 3 4 5 6 7 8` (10 đơn vị rời rạc $\rightarrow$ quá tải RAM).
- Gom cụm quen thuộc: `[0912] [345] [678]` (3 cụm có nghĩa $\rightarrow$ ghi nhớ tức thì không tốn sức!).

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
So sánh giữa Kiện tướng Cờ vua và Người mới chơi:
- Khi cho nhìn thoáng qua một thế cờ đang diễn ra trong 5 giây, kiện tướng có thể nhớ chính xác vị trí của hầu hết quân cờ, trong khi người mới chỉ nhớ được 4–5 quân.
- Tuy nhiên, nếu sắp xếp các quân cờ **ngẫu nhiên phi lý trí** trên bàn cờ, khả năng ghi nhớ của kiện tướng tụt xuống bằng đúng người mới!
- **Bài học:** Kiện tướng không có bộ nhớ siêu phàm hơn, mà họ đã lưu trữ hàng nghìn ""khối cấu trúc chiến thuật (chunks)"" trong trí nhớ dài hạn, giúp não bộ nhận diện một cụm 6–7 quân cờ như một đơn vị duy nhất!

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Nhồi nhét một lượng lớn khái niệm rời rạc cùng lúc | Quá tải nhận thức (Cognitive Overload), nhanh chóng quên sạch sau 1 ngày | Chia nhỏ kiến thức thành từng khối vi mô 3–5 khái niệm liên quan logic |
| Thẻ Flashcard chứa quá nhiều ý trong một mặt (Over-packed card) | Khó xác định chính xác mình nhớ ý nào và quên ý nào, phá hỏng thuật toán FSRS | Áp dụng Quy tắc Nguyên tử hóa (Atomic Card): Mỗi thẻ flashcard chỉ kiểm tra 1 ý cốt lõi duy nhất |
| Cố gắng học thuộc lòng chuỗi ký tự không có liên tưởng | Não bộ tiêu tốn năng lượng gấp nhiều lần mà không tạo được synap bền vững | Gắn cụm thông tin mới với hình ảnh đời thường hoặc câu chuyện có ý nghĩa cá nhân |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Thông tin cần học đã được phân nhóm thành các cụm nhỏ từ 3 đến 5 đơn vị chưa?
- [ ] Mỗi thẻ Flashcard tạo ra có tuân thủ tính nguyên tử (1 thẻ = 1 khái niệm) không?
- [ ] Các khối kiến thức mới đã được liên kết với nền tảng kinh nghiệm có sẵn chưa?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Quy luật George Miller 7 ± 2 và kỹ thuật gom cụm Chunking mở rộng bộ nhớ.",
                "Tối ưu dung lượng Working Memory và nguyên tắc thẻ nhớ nguyên tử FSRS."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson22Id,
                "Bài 2.2: Đường Cong Quên Lãng Ebbinghaus & Tái Hiện Chủ Động",
                "duong-cong-ebbinghaus-va-tai-hien-chu-dong",
                25,
                @"# Bài 2.2: Đường Cong Quên Lãng Ebbinghaus & Tái Hiện Chủ Động

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Đọc lại sách giáo khoa 5 lần không bằng **gấp sách lại và tự nhớ ra 3 ý chính một lần**! Cảm giác hơi căng não khi cố nhớ lại chính là tín hiệu hóa sinh báo cho não bộ khóa chặt ký ức đó vào trí nhớ dài hạn.

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững quy luật Đường cong Quên lãng (Forgetting Curve) của Hermann Ebbinghaus: hơn 50% kiến thức bị lãng quên trong vòng 24 giờ.
- Phân biệt sự vượt trội giữa Tái hiện Chủ động (Retrieval Practice) và Đọc lại thụ động (Passive Review).
- Hiểu cơ chế củng cố synap thần kinh qua Lặp lại ngắt quãng (Spaced Repetition) và mô hình FSRS v5.
- Thiết lập thói quen kiểm tra trí nhớ trước khi nhìn lại tài liệu (Testing Effect).

---

## 2. Khái niệm & Mô hình Cốt lõi

### 2.1. Đường Cong Quên Lãng Ebbinghaus
Khi tiếp nhận thông tin mới mà không có sự củng cố, tốc độ lãng quên diễn ra nhanh nhất ngay trong những giờ đầu tiên:
- Sau 20 phút: Quên khoảng 40%.
- Sau 24 giờ: Quên hơn 50–70%.
- Sau 30 ngày: Chỉ còn sót lại dưới 10–20% nếu không được ôn tập ngắt quãng.

### 2.2. Sức Mạnh của Retrieval Practice (Hiệu Ứng Kiểm Tra)
- **Đọc lại thụ động (Passive Reading):** Tạo ra **Ảo tưởng về năng lực (Illusion of Competence)**. Mắt nhìn thấy quen thuộc khiến não lầm tưởng rằng mình đã hiểu sâu, nhưng khi vào phòng thi/thực tế thì không thể nhớ ra.
- **Tái hiện chủ động (Active Recall / Retrieval):** Ép não bộ phải nỗ lực truy xuất thông tin từ bộ nhớ sâu mà không nhìn tài liệu. Chính **nỗ lực truy xuất này** mới là tín hiệu hóa sinh kích thích tế bào thần kinh củng cố liên kết synap bền vững.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
Nghiên cứu kinh điển của Roediger & Karpicke (2006) trên hai nhóm sinh viên:
- **Nhóm 1 (Học dồn & Đọc lại):** Đọc bài 4 lần liên tiếp.
- **Nhóm 2 (Active Recall):** Đọc bài 1 lần, sau đó làm 3 bài tự kiểm tra nhớ lại mà không mở tài liệu.
- **Kết quả kiểm tra sau 1 tuần:** Nhóm 2 ghi nhớ **nhiều hơn gấp đôi** so với Nhóm 1, dù thời gian tiếp xúc với tài liệu ít hơn hẳn!

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Dùng bút dạ quang highlight vàng rực cả trang sách | Chỉ làm tăng độ quen mắt thụ động, không kích hoạt cơ chế truy xuất của não | Đóng sách lại, tự lấy giấy trắng viết ra 3 ý chính vừa đọc được (Brain Dump) |
| Học dồn thâu đêm trước ngày thi (Cramming) | Thông tin chỉ nằm tạm bợ ở trí nhớ ngắn hạn và biến mất sạch sau khi thi xong | Rải đều các khoảng ôn tập ngắn 10–15 phút theo lịch ngắt quãng FSRS |
| Nhìn đáp án ngay khi thấy hơi khó nhớ | Tước đoạt cơ hội nỗ lực thần kinh củng cố synap | Dành ít nhất 10–15 giây cố gắng tự nhớ lại trước khi bấm lật thẻ đáp án |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Bạn đã tự kiểm tra nhớ lại bằng lời của mình trước khi mở tài liệu xem đáp án chưa?
- [ ] Lịch ôn tập có được ngắt quãng theo các mốc thời gian tăng dần (1 ngày, 3 ngày, 7 ngày, 30 ngày) không?
- [ ] Đã chuyển các khái niệm cốt lõi thành câu hỏi Active Recall trong Studxy chưa?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Đường cong quên lãng Ebbinghaus và phương pháp Retrieval Practice.",
                "Tái hiện chủ động thay thế đọc lại thụ động, củng cố synap thần kinh dài hạn."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson31Id,
                "Bài 3.1: Mô Hình ABC Nhận Thức & Thách Thức Suy Nghĩ Tự Động",
                "mo-hinh-cbt-abc-va-tai-cau-truc-nhan-thuc",
                30,
                @"# Bài 3.1: Mô Hình ABC Nhận Thức & Thách Thức Suy Nghĩ Tự Động

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Hai người cùng gặp 1 cơn mưa (Sự kiện A): người thứ nhất nghĩ *""Hôm nay xui tận mạng""* (B) nên bực bội; người thứ hai nghĩ *""Không khí mát mẻ hẳn để đọc sách""* nên thư thái. Sự kiện giống nhau, **câu chuyện bạn kể trong đầu mới quyết định cảm xúc**!

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững Mô hình ABC trong Liệu pháp Nhận thức Hành vi (Cognitive Behavioral Therapy - CBT) của Albert Ellis và Aaron Beck.
- Nhận diện các Bẫy Nhận thức / Lỗi Tư duy (Cognitive Distortions): Đọc suy nghĩ, Trầm trọng hóa, Tư duy Trắng - Đen, Cá nhân hóa.
- Tự viết Nhật ký Tái cấu trúc Nhận thức (Cognitive Restructuring) 5 cột để thách thức suy nghĩ tiêu cực tự động.
- Chuyển hóa cảm xúc lo âu, tự ti thành hành vi mang tính hành động cân bằng và lành mạnh.

---

## 2. Khái niệm & Mô hình Cốt lõi

```
[A: Sự kiện kích hoạt] ---> [B: Niềm tin / Suy nghĩ tự động] ---> [C: Cảm xúc & Hành vi hệ quả]
                                      |
                           (D: Tranh biện & Thách thức)
                                      |
                           (E: Suy nghĩ cân bằng mới)
```

- **A (Activating Event):** Sự kiện khách quan xảy ra trong thực tế (không chứa định kiến hay suy diễn).
- **B (Belief / Automatic Thought):** Ý nghĩ tự động bật lên trong đầu để diễn giải sự kiện A.
- **C (Consequence):** Phản ứng cảm xúc (lo âu, tức giận) và hành vi (trì hoãn, né tránh) bắt nguồn trực tiếp từ niềm tin B.
> **Chân lý cốt lõi của CBT:** Không phải sự kiện A làm bạn tổn thương, mà chính cách bạn diễn giải sự kiện đó qua lăng kính B mới quyết định cảm xúc C của bạn!

### 2.1. Bốn Lỗi Nhận Thức Thường Gặp Nhất
1. **Đọc suy nghĩ (Mind Reading):** Tự quả quyết rằng người khác đang coi thường hoặc chê bai mình dù không có bằng chứng khách quan.
2. **Trầm trọng hóa / Thảm họa hóa (Catastrophizing):** Phóng đại một sai sót nhỏ thành kịch bản tồi tệ nhất không thể cứu vãn.
3. **Tư duy Trắng - Đen (All-or-Nothing Thinking):** Nhìn nhận kết quả theo hai thái cực cực đoan: hoặc là hoàn hảo 100%, hoặc là thất bại hoàn toàn.
4. **Dán nhãn (Labeling):** Gắn cho bản thân hoặc người khác một nhãn tiêu cực cố định (*""Mình là kẻ bất tài""* thay vì *""Mình vừa làm hỏng bài test này""*).

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
- **A (Sự kiện):** Bạn gửi đề xuất cải tiến lên nhóm chat công việc lúc 10h sáng; đến 14h chiều có 4 người đã xem nhưng chưa ai phản hồi.
- **B (Suy nghĩ tự động tiêu cực):** *""Chắc chắn mọi người thấy ý tưởng của mình ngớ ngẩn và đang coi thường mình (Đọc suy nghĩ). Dự án này coi như hỏng rồi (Trầm trọng hóa).""*
- **C (Hệ quả):** Tim đập nhanh, căng thẳng, muốn xóa tin nhắn và không muốn đi họp.
- **D (Tranh biện & Bằng chứng):** *""Có dữ kiện nào khẳng định mọi người chê không? Không, 10h–12h là giờ cao điểm xử lý task gấp, 12h–13h30 nghỉ trưa. Trưởng nhóm từng khen góc nhìn sáng tạo tuần trước.""*
- **E (Suy nghĩ cân bằng mới):** *""Mọi người đang bận xử lý việc ưu tiên hơn. Mình sẽ nhắn tin nhắc nhẹ lịch góp ý vào 16h chiều nay.""* $\rightarrow$ Cảm xúc bình an trở lại, hành động chuyên nghiệp.

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Đồng nhất cảm xúc với sự thật: *""Mình cảm thấy bất an nghĩa là mình sắp thất bại""* | Cảm xúc bị biến thành bằng chứng giả tạo, làm trầm trọng thêm sự lo âu | Nhớ rằng cảm xúc là phản ứng của cơ thể, không phải sự thật khách quan |
| Ép bản thân ""suy nghĩ tích cực độc hại"" (Toxic Positivity) gượng ép | Đè nén cảm xúc thật, gây ức chế tâm lý ngầm bùng phát mạnh hơn | Hướng tới **Suy nghĩ Cân bằng & Thực tế** (Balanced Thought) dựa trên dữ kiện |
| Xem lỗi nhận thức là khuyết tật cá nhân | Tự trách bản thân vì có suy nghĩ tiêu cực | Chấp nhận suy nghĩ tự động là thói quen tiến hóa của não bộ; quan trọng là nhận diện và tái cấu trúc |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Bạn đã tách biệt rõ ràng Sự kiện thực tế (A) khỏi Diễn giải chủ quan (B) chưa?
- [ ] Bằng chứng khách quan nào ủng hộ và bằng chứng nào phản bác suy nghĩ tự động đó?
- [ ] Lời thoại cân bằng mới có giúp bạn bình tâm và hành động hiệu quả hơn không?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Liệu pháp nhận thức hành vi CBT, mô hình ABCDE của Ellis và Beck.",
                "Nhận diện 4 lỗi nhận thức kinh điển và tái cấu trúc suy nghĩ cân bằng."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson32Id,
                "Bài 3.2: Thiết Lập Ranh Giới Cá Nhân & Tự Trắc Ẩn (Self-Compassion)",
                "thiet-lap-ranh-gioi-va-tu-trac-an",
                25,
                @"# Bài 3.2: Thiết Lập Ranh Giới Cá Nhân & Tự Trắc Ẩn (Self-Compassion)

> [!TIP]
> **🌱 Phần 0 — Khởi động trực giác cho người mới:** Khi một người bạn thân vừa làm hỏng bài kiểm tra, bạn sẽ an ủi họ ra sao? Hãy dùng chính sự tử tế đó nói với bản thân mình thay vì chỉ trích cay nghiệt — đó là nền tảng của sức bền tinh thần dài hạn.

## 1. Mục tiêu bài học (Learning Objectives)
- Xây dựng Ranh giới Cá nhân lành mạnh (Healthy Boundaries) trong các mối quan hệ công việc và đời sống.
- Nắm vững công thức Nói ""Không"" khéo léo, tự tin, tôn trọng đối phương mà không cảm thấy tội lỗi.
- Thực hành mô hình Tự trắc ẩn (Self-Compassion) 3 yếu tố của Tiến sĩ Kristin Neff.
- Thay thế tiếng nói tự chỉ trích cay nghiệt bên trong (Inner Critic) bằng tiếng nói người bạn thông thái bao dung.

---

## 2. Khái niệm & Mô hình Cốt lõi

### 2.1. Ba Yếu Tố của Tự Trắc Ẩn (Kristin Neff)
1. **Sự tử tế với bản thân (Self-Kindness) vs Tự phán xét:** Đối xử với chính mình bằng sự ấm áp, thấu hiểu khi gặp thất bại, thay vì sỉ nhục và trừng phạt bản thân.
2. **Nhân tính chung (Common Humanity) vs Sự cô lập:** Nhận thức rằng vấp ngã, đau khổ và khiếm khuyết là một phần tất yếu của trải nghiệm làm người; bạn không hề đơn độc hay dị biệt.
3. **Chánh niệm (Mindfulness) vs Đồng hóa quá mức:** Quan sát nỗi đau và cảm xúc tiêu cực với tâm thế khách quan, không né tránh nhưng cũng không phóng đại làm thảm họa.

### 2.2. Nghệ Thuật Thiết Lập Ranh Giới Lành Mạnh
Nói ""Không"" với những điều vượt quá năng lượng và giá trị ưu tiên của bạn chính là nói ""Có"" với sức khỏe tinh thần và cam kết chất lượng của chính bạn. Một lời từ chối rõ ràng và lịch thiệp luôn tử tế hơn một sự đồng ý miễn cưỡng để rồi làm hời hợt hoặc ôm sự ấm ức trong lòng.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu
Khi một đồng nghiệp nhờ bạn làm giúp phần việc ngoài phạm vi vào tối muộn khi bạn đang cần ôn thi:
- **Phản ứng thụ động né tránh:** Miễn cưỡng đồng ý vì sợ bị đánh giá là ích kỷ, sau đó thức trắng đêm làm trong bực bội và oán trách đối phương.
- **Phản ứng hung hăng công kích:** *""Sao lúc nào cũng đùn việc cho người khác thế, tự đi mà làm!""* $\rightarrow$ Đổ vỡ mối quan hệ.
- **Ranh giới lành mạnh quyết đoán (Assertive Boundary):** *""Cảm ơn bạn đã tin tưởng mình. Tuy nhiên tối nay mình đang tập trung toàn bộ thời gian cho đồ án tốt nghiệp đến hạn nộp sáng mai nên không thể nhận hỗ trợ được. Chúc dự án của bạn diễn ra thuận lợi nhé!""* $\rightarrow$ Giữ vững ranh giới cá nhân mà vẫn tôn trọng đối phương.

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Nhầm lẫn Tự trắc ẩn với Sự tự thương hại (Self-Pity) | Chìm đắm trong vai nạn nhân và than thân trách phận | Tự trắc ẩn kết nối với nhân tính chung để nhìn nhận vấn đề khách quan và hành động sửa đổi |
| Đánh đồng Tự trắc ẩn với Sự buông thả bản thân (Self-Indulgence) | Dùng làm cái cớ để lười biếng và không nỗ lực vươn lên | Tự trắc ẩn tạo động lực xuất phát từ tình yêu thương chứ không phải từ nỗi sợ thất bại |
| Giải thích dài dòng và xin lỗi rối rít khi từ chối | Khiến đối phương cảm thấy ranh giới của bạn lỏng lẻo và tiếp tục ép bạn nhượng bộ | Trình bày lý do ngắn gọn, dứt khoát và lịch thiệp; không cần biện minh quá mức |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Bạn có đang đối xử với bản thân dịu dàng như cách bạn đối xử với một người bạn thân đang đau buồn không?
- [ ] Lời từ chối của bạn có rõ ràng, ngắn gọn và tôn trọng ranh giới của cả hai bên không?
- [ ] Khi gặp thất bại, bạn có nhắc nhở mình rằng đây là trải nghiệm làm người chung của nhân loại không?

<!-- studxy-writein-v2 -->
<!-- studxy-writein-v3 -->",
                "Thiết lập ranh giới cá nhân lành mạnh và kỹ năng nói không quyết đoán.",
                "Mô hình tự trắc ẩn 3 yếu tố Kristin Neff thay thế tiếng nói tự chỉ trích."
            )
        };
    }

    public static async Task EnsureLessonsUpgradedAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var metas = GetUpgradedLessons();
        foreach (var meta in metas)
        {
            var existing = await db.Lessons.FirstOrDefaultAsync(l => l.Id == meta.LessonId);
            if (existing != null && existing.CurrentVersion == 1)
            {
                var currentMd = existing.ContentMarkdown ?? string.Empty;
                var isV3Meta = meta.Markdown.Contains(CodingUpgradeMarkerV3, StringComparison.Ordinal);
                var needsUpgrade = isV3Meta
                    ? !currentMd.Contains(CodingUpgradeMarkerV3, StringComparison.Ordinal)
                    : !currentMd.Contains(UpgradeMarker, StringComparison.Ordinal);

                // Safe idempotency: ONLY upgrade if CurrentVersion == 1 and target marker is not yet present.
                // Upgrades both v1 -> v3 and v2 -> v3 for Coding JS without overwriting user edits (CurrentVersion > 1).
                if (needsUpgrade)
                {
                    existing.ContentMarkdown = meta.Markdown;
                    existing.PlainText = meta.Markdown;
                    existing.EstimatedMinutes = meta.EstimatedMinutes;
                    existing.ContentAstJson = BuildLessonAst(existing.Title, meta.Summary1, meta.Summary2);
                    existing.UpdatedAt = now;
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static string BuildLessonAst(string title, string p1, string p2)
    {
        var ast = new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 1, block_id = Guid.NewGuid().ToString() },
                    content = new object[] { new { type = "text", text = title } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new object[] { new { type = "text", text = p1 } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new object[] { new { type = "text", text = p2 } }
                }
            }
        };

        return JsonSerializer.Serialize(ast);
    }
}
