namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;

/// <summary>
/// Safe theory content upgrade for 18 core expansion lessons (Coding, Investing, Psychology).
/// Uses marker &lt;!-- studxy-writein-v2 --&gt; and strictly upgrades ONLY when CurrentVersion == 1.
/// NEVER overwrites user-edited lessons (CurrentVersion &gt; 1).
/// </summary>
public static class WriteInTheoryExpansion
{
    public const string UpgradeMarker = "<!-- studxy-writein-v2 -->";

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
            // 1. CODING LESSONS (1.1 .. 3.2)
            // =========================================================================
            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson11Id,
                "Bài 1.1: Biến Đổi & Lọc Dữ Liệu Nâng Cao (Map, Filter, Reduce)",
                "bien-doi-va-loc-du-lieu-mang",
                25,
                @"# Bài 1.1: Biến Đổi & Lọc Dữ Liệu Nâng Cao (Map, Filter, Reduce)

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững bản chất của Lập trình hàm (Functional Programming) trong JavaScript: tính thuần khiết (pure function) và tính bất biến (immutability).
- Làm chủ bộ ba phương thức mảng nền tảng: `map()`, `filter()`, `reduce()`, hiểu rõ cơ chế cấp phát bộ nhớ và giá trị trả về.
- Xây dựng các pipeline xử lý dữ liệu phức tạp (nhóm đối tượng, tính tổng ròng, chuyển đổi schema DTO) mà không làm đột biến (mutate) mảng gốc.
- Nhận diện các lỗi runtime phổ biến như thiếu giá trị khởi tạo (initialValue) trong `reduce`, dẫn đến crash khi mảng rỗng.

---

## 2. Khái niệm & Mô hình Cốt lõi

### 2.1. Tính Bất Biến (Immutability)
Trong kiến trúc ứng dụng hiện đại (React state, Redux, Clean Architecture), đột biến trực tiếp mảng đầu vào (`arr.sort()`, `arr.push()`) là nguồn gốc hàng đầu gây bug re-render và side-effect khó truy vết. Các hàm `map`, `filter`, `reduce` luôn trả về một giá trị/mảng mới, giữ nguyên vẹn dữ liệu gốc.

### 2.2. Cơ chế Hoạt động của Bộ Ba Phương thức
1. **`filter(predicate)`**:
   - Duyệt qua từng phần tử, chỉ giữ lại phần tử có `predicate(item, index, array) === true`.
   - Độ dài mảng kết quả luôn $\le$ mảng ban đầu; kiểu phần tử không đổi.
2. **`map(transform)`**:
   - Ánh xạ 1-1 từng phần tử thành một giá trị mới qua `transform(item, index, array)`.
   - Độ dài mảng giữ nguyên 100%; kiểu phần tử có thể thay đổi hoàn toàn.
3. **`reduce(accumulator, initialValue)`**:
   - Thu gọn toàn bộ mảng thành một giá trị duy nhất (số, chuỗi, object gom nhóm, hoặc map phẳng).
   - **Quy tắc vàng:** Luôn truyền tham số thứ hai `initialValue` (ví dụ `0`, `[]`, `{}`) để đảm bảo an toàn kiểu dữ liệu khi mảng rỗng.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu

```javascript
// Danh sách giao dịch ví điện tử
const transactions = [
  { id: 'tx-1', type: 'income', amount: 5000000, status: 'completed' },
  { id: 'tx-2', type: 'expense', amount: 1200000, status: 'completed' },
  { id: 'tx-3', type: 'income', amount: 3000000, status: 'pending' },
  { id: 'tx-4', type: 'expense', amount: 800000, status: 'completed' }
];

// Pipeline: Lọc đơn hoàn tất -> Gom nhóm theo loại thu/chi
const summary = transactions
  .filter(tx => tx.status === 'completed')
  .reduce((acc, tx) => {
    acc[tx.type] = (acc[tx.type] || 0) + tx.amount;
    acc.netBalance += (tx.type === 'income' ? tx.amount : -tx.amount);
    return acc;
  }, { income: 0, expense: 0, netBalance: 0 });

console.log(summary);
// Kết quả: { income: 5000000, expense: 2000000, netBalance: 3000000 }
```

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Bỏ qua `initialValue` khi tính tổng object: `arr.reduce((s, o) => s + o.val)` | Khi mảng có 1 phần tử hoặc rỗng, trả về object hoặc ném `TypeError` | Luôn chỉ định giá trị khởi tạo: `.reduce((s, o) => s + o.val, 0)` |
| Nhầm lẫn giữa `filter()` và `find()` | `find()` chỉ lấy 1 phần tử đầu tiên thỏa điều kiện, `filter()` trả về mảng | Dùng `filter()` khi cần xử lý danh sách, `find()` khi tìm bản ghi độc nhất |
| Đột biến mảng bên trong callback: `arr.map(x => { x.checked = true; return x; })` | Thay đổi thuộc tính của object trong mảng gốc trong bộ nhớ heap | Trả về shallow copy của object: `arr.map(x => ({ ...x, checked: true }))` |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Mảng gốc có được bảo toàn nguyên vẹn sau hàm xử lý không?
- [ ] Tất cả các lệnh `reduce` đã có tham số `initialValue` hợp lệ chưa?
- [ ] Hàm callback truyền vào `map`/`filter` có phải là hàm thuần khiết (không gọi API hay mutate state ngoài)?
- [ ] Trường hợp mảng rỗng `[]` đã được test case kiểm tra mà không crash chưa?

<!-- studxy-writein-v2 -->",
                "Biến đổi và lọc mảng với map, filter, reduce chuẩn Functional Programming.",
                "Bảo toàn tính bất biến và triệt tiêu lỗi runtime khi mảng rỗng."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson12Id,
                "Bài 1.2: Thuật Toán Tìm Kiếm & Kỹ Thuật Hai Con Trỏ (Two Pointers)",
                "thuat-toan-tim-kiem-va-two-pointers",
                25,
                @"# Bài 1.2: Thuật Toán Tìm Kiếm & Kỹ Thuật Hai Con Trỏ (Two Pointers)

## 1. Mục tiêu bài học (Learning Objectives)
- Phân biệt độ phức tạp thời gian giữa Tìm kiếm tuyến tính $O(n)$ và Tìm kiếm nhị phân $O(\log n)$.
- Cài đặt thành thạo thuật toán Binary Search trên mảng đã sắp xếp và xử lý an toàn điều kiện dừng vòng lặp.
- Vận dụng kỹ thuật Hai con trỏ (Two Pointers): con trỏ đối xứng hai đầu (left - right) và con trỏ chạy nhanh - chậm (fast - slow).
- Tối ưu hóa bộ nhớ phụ về $O(1)$ khi giải các bài toán đảo mảng, loại bỏ trùng lặp và tìm cặp số mục tiêu (Two Sum II).

---

## 2. Khái niệm & Mô hình Cốt lõi

### 2.1. Tìm kiếm Nhị phân (Binary Search)
- **Điều kiện tiên quyết:** Mảng phải được sắp xếp theo thứ tự (tăng dần hoặc giảm dần).
- **Nguyên lý chia để trị:** Tại mỗi bước, so sánh phần tử ở giữa `mid` với `target`. Nếu `target < arr[mid]`, thu hẹp không gian tìm kiếm sang nửa trái (`right = mid - 1`); ngược lại sang nửa phải (`left = mid + 1`).
- **Phòng chống tràn số (Integer Overflow):** Thay vì viết `Math.floor((left + right) / 2)`, trong các ngôn ngữ có kiểu int cố định nên dùng `left + Math.floor((right - left) / 2)`.

### 2.2. Kỹ thuật Hai Con Trỏ (Two Pointers Pattern)
Thay vì dùng 2 vòng lặp lồng nhau với độ phức tạp $O(n^2)$, kỹ thuật 2 con trỏ duyệt mảng theo hướng hội tụ hoặc song hành giúp giảm độ phức tạp về $O(n)$:
1. **Con trỏ hai đầu (Opposite Direction):** Một con trỏ ở đầu mảng (`left = 0`), một con trỏ ở cuối mảng (`right = n - 1`), tiến dần vào giữa (ví dụ: Two Sum trên mảng đã sort, Palindrome check).
2. **Con trỏ nhanh - chậm (Fast & Slow):** Hai con trỏ cùng xuất phát từ đầu mảng với tốc độ khác nhau (ví dụ: phát hiện chu trình Floyd, xóa phần tử trùng lặp in-place).

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu

```javascript
// Bài toán: Tìm hai số trong mảng đã sắp xếp có tổng bằng target (Two Sum II)
function twoSumSorted(numbers, target) {
  let left = 0;
  let right = numbers.length - 1;

  while (left < right) {
    const currentSum = numbers[left] + numbers[right];
    if (currentSum === target) {
      return [left, right]; // Tìm thấy cặp chỉ số
    } else if (currentSum < target) {
      left++; // Tổng còn nhỏ -> Tăng con trỏ bên trái
    } else {
      right--; // Tổng quá lớn -> Giảm con trỏ bên phải
    }
  }

  return []; // Không tìm thấy
}

console.log(twoSumSorted([2, 7, 11, 15], 9)); // Output: [0, 1]
```

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Quên kiểm tra mảng đã sắp xếp trước khi chạy Binary Search | Kết quả tìm kiếm ngẫu nhiên, bỏ sót phần tử tồn tại | Luôn đảm bảo mảng đã được sort hoặc sort trước khi áp dụng |
| Điều kiện dừng `left < right` thay vì `left <= right` trong Binary Search | Bỏ sót phần tử cuối cùng khi `left === right` | Dùng `while (left <= right)` cho tìm kiếm chính xác vị trí |
| Cập nhật con trỏ `left = mid` hoặc `right = mid` | Gây vòng lặp vô tận (infinite loop) khi `left` và `right` kề nhau | Luôn bước nhảy: `left = mid + 1` hoặc `right = mid - 1` |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Dữ liệu đầu vào đã được sắp xếp trước khi gọi Binary Search chưa?
- [ ] Điều kiện biên mảng rỗng `[]` hoặc mảng 1 phần tử đã được kiểm tra chưa?
- [ ] Thuật toán đạt độ phức tạp không gian $O(1)$ không cần cấp phát thêm mảng phụ?

<!-- studxy-writein-v2 -->",
                "Thuật toán tìm kiếm nhị phân O(log n) và kỹ thuật hai con trỏ tối ưu O(1) bộ nhớ.",
                "Giải quyết bài toán Two Sum và đảo mảng in-place."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson21Id,
                "Bài 2.1: Kỹ Thuật Debounce & Throttle Xử Lý Sự Kiện Tần Suất Cao",
                "debounce-va-throttle-su-kien",
                25,
                @"# Bài 2.1: Kỹ Thuật Debounce & Throttle Xử Lý Sự Kiện Tần Suất Cao

## 1. Mục tiêu bài học (Learning Objectives)
- Phân biệt triệt để cơ chế vận hành và ca sử dụng thực tế giữa **Debounce** và **Throttle**.
- Làm chủ kỹ thuật Closure và quản lý định thời qua `setTimeout` / `clearTimeout` trong JavaScript.
- Bảo vệ ứng dụng web khỏi hiện tượng nghẽn UI thread (Jank) và quá tải API backend khi người dùng tương tác liên tục.
- Xử lý dọn dẹp (cleanup) timer khi component unmount để ngăn ngừa rò rỉ bộ nhớ (Memory Leak).

---

## 2. Khái niệm & Mô hình Cốt lõi

```
Sự kiện phát ra:  |||||||||||||||||---------------
Debounce (300ms): -----------------------------[Thực thi 1 lần cuối]
Throttle (300ms): [Lần 1]-----[Lần 2]-----[Lần 3]--------
```

### 2.1. Debounce (Gom nhóm sự kiện)
- **Cơ chế:** Trì hoãn việc gọi hàm cho đến khi người dùng **ngừng hoàn toàn** phát sinh sự kiện trong một khoảng thời gian `delay` nhất định. Mỗi khi sự kiện mới xuất hiện, bộ đếm timer cũ bị hủy bỏ và đếm lại từ đầu.
- **Kịch bản điển hình:** Ô tìm kiếm tức thì (Search Autocomplete), lưu bản nháp tự động (Auto-save draft), kiểm tra tính khả dụng của username khi gõ.

### 2.2. Throttle (Tiết chế tần suất)
- **Cơ chế:** Đảm bảo hàm chỉ được thực thi **tối đa 1 lần** trong mỗi chu kỳ thời gian `limit`, bất kể người dùng kích hoạt sự kiện bao nhiêu lần.
- **Kịch bản điển hình:** Sự kiện cuộn trang (`window.onscroll`), kéo giãn kích thước cửa sổ (`resize`), cập nhật tọa độ chuột (`mousemove`), nút bấm chống spam click liên hoàn.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu

```javascript
// Cài đặt Debounce chuẩn chỉnh hỗ trợ arguments và ngữ cảnh this
function debounce(fn, delay) {
  let timerId = null;

  const debounced = function(...args) {
    if (timerId !== null) {
      clearTimeout(timerId); // Hủy timer cũ nếu có tương tác mới
    }
    timerId = setTimeout(() => {
      fn.apply(this, args);
      timerId = null;
    }, delay);
  };

  // Hỗ trợ hàm hủy thủ công (cleanup)
  debounced.cancel = () => {
    if (timerId !== null) {
      clearTimeout(timerId);
      timerId = null;
    }
  };

  return debounced;
}
```

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Khởi tạo lại hàm debounce bên trong body component React | Mỗi lần re-render tạo một timer độc lập, mất tác dụng debounce | Bọc trong `useCallback` hoặc `useRef` để giữ nguyên instance qua các lần render |
| Quên gọi `clearTimeout` khi component unmount | Callback thực thi trên component đã unmount, gây rò rỉ bộ nhớ | Thêm return cleanup function trong `useEffect`: `return () => debounced.cancel()` |
| Nhầm lẫn giữa Debounce và Throttle khi làm tính năng Infinite Scroll | Debounce khiến trang phải dừng cuộn hẳn mới tải tiếp, gây cảm giác giật cục | Dùng Throttle để nạp nội dung mượt mà đều đặn theo chu kỳ cuộn |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Đã chọn đúng giải pháp: Debounce (chờ người dùng dừng) hay Throttle (đều đặn theo nhịp)?
- [ ] Biến `timerId` đã được lưu trữ an toàn trong Closure hoặc Ref?
- [ ] Có hàm `.cancel()` để dọn dẹp khi đổi màn hình hoặc hủy thao tác không?

<!-- studxy-writein-v2 -->",
                "Phân biệt và cài đặt Debounce vs Throttle với Closure và timer cleanup.",
                "Tối ưu hiệu năng UI và chống nghẽn server dưới tải sự kiện dày đặc."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson22Id,
                "Bài 2.2: Memoization & Cache Kết Quả Hàm Tính Toán Phức Tạp",
                "memoization-va-cache-ket-qua-ham",
                25,
                @"# Bài 2.2: Memoization & Cache Kết Quả Hàm Tính Toán Phức Tạp

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững khái niệm Hàm thuần khiết (Pure Functions) và điều kiện để áp dụng kỹ thuật Memoization.
- Tự tay xây dựng hàm bậc cao `memoize()` hỗ trợ đa tham số, lưu trữ cache trong `Map` hoặc `WeakMap`.
- Nhận diện và xử lý triệt để bài toán Trùng lặp khóa cache (Key Collision) khi tham số là Object.
- Cân bằng chi phí bộ nhớ RAM (Memory Footprint) với chi phí thời gian xử lý CPU (Time Complexity).

---

## 2. Khái niệm & Mô hình Cốt lõi

### 2.1. Bản chất của Memoization
Memoization là kỹ thuật tối ưu hóa trong đó hàm ghi nhớ kết quả của các lần tính toán trước đó dựa trên tham số đầu vào. Khi hàm được gọi lại với cùng một bộ tham số, kết quả lưu sẵn trong bộ nhớ đệm (Cache) được trả về ngay lập tức với độ phức tạp $O(1)$, bỏ qua toàn bộ khối tính toán đắt đỏ.

### 2.2. Điều kiện Tiên quyết
- **Hàm phải thuần khiết (Pure Function):** Với cùng đầu vào $x$, hàm luôn trả về cùng đầu ra $y$ và không tạo ra side-effect (không phụ thuộc biến toàn cục, thời gian hiện tại `Date.now()`, hay số ngẫu nhiên `Math.random()`).
- Chi phí tính toán của hàm gốc phải lớn hơn chi phí serialize và tra cứu key trong cache.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu

```javascript
// Hàm memoize mạnh mẽ với bộ tạo key an toàn
function memoize(fn, customKeyGenerator) {
  const cache = new Map();

  return function(...args) {
    // Tạo cache key an toàn: dùng hàm tự chọn hoặc serialize JSON
    const key = customKeyGenerator ? customKeyGenerator(...args) : JSON.stringify(args);

    if (cache.has(key)) {
      return cache.get(key); // Cache Hit: Trả về kết quả tức thì O(1)
    }

    const result = fn.apply(this, args); // Cache Miss: Tính toán
    cache.set(key, result);
    return result;
  };
}

// Ứng dụng: Tính số Fibonacci đệ quy
const fib = memoize((n) => {
  if (n <= 1) return n;
  return fib(n - 1) + fib(n - 2);
});

console.log(fib(40)); // Tính toán trong vài mili-giây thay vì vài chục giây!
```

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Dùng `arg.toString()` làm key cache | Các object khác nhau như `{ a: 1 }` và `{ b: 2 }` đều biến thành `""[object Object]""`, gây trả về sai kết quả | Dùng `JSON.stringify()` hoặc `WeakMap` phân biệt theo tham chiếu object |
| Cache phình to vô hạn theo thời gian (Unbounded Cache) | Gây tràn bộ nhớ RAM (OutOfMemory) khi ứng dụng chạy lâu dài | Áp dụng chiến lược dọn dẹp bộ nhớ: LRU Cache (Least Recently Used) hoặc giới hạn maxSize |
| Memoize hàm có side-effect hoặc hàm bất thuần khiết | Hàm không cập nhật dữ liệu mới khi môi trường bên ngoài thay đổi | Tuyệt đối không memoize hàm gọi API hoặc hàm phụ thuộc biến mutable ngoài |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Hàm được memoize có phải là hàm thuần khiết 100%?
- [ ] Chiến lược tạo key cache có an toàn với tham số object/array không?
- [ ] Bộ nhớ cache có cơ chế dọn dẹp hoặc giới hạn số lượng phần tử không?

<!-- studxy-writein-v2 -->",
                "Kỹ thuật Memoization tối ưu hóa thời gian tính toán O(1) qua cache Map.",
                "Quản lý bộ nhớ đệm và phòng chống xung đột khóa cache."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson31Id,
                "Bài 3.1: Async/Await, Promise Pipeline & Retry Cơ Chế Tự Phục Hồi",
                "async-await-promise-pipeline-retry",
                30,
                @"# Bài 3.1: Async/Await, Promise Pipeline & Retry Cơ Chế Tự Phục Hồi

## 1. Mục tiêu bài học (Learning Objectives)
- Thấu hiểu kiến trúc Vòng lặp sự kiện (Event Loop), phân biệt Call Stack, Microtask Queue (`Promise.then`) và Macrotask Queue (`setTimeout`).
- Xây dựng Pipeline thực thi danh sách Promise: chạy tuần tự (Sequential) vs chạy song song có kiểm soát giới hạn tải (`Promise.all` / Pool limit).
- Cài đặt cơ chế Tự phục hồi lỗi (Self-Healing) với thuật toán Exponential Backoff và Jitter cho các kết nối mạng không ổn định.
- Sử dụng chuẩn `Promise.allSettled()` để tổng hợp trạng thái thành công/thất bại mà không bị ngắt quãng nửa chừng.

---

## 2. Khái niệm & Mô hình Cốt lõi

### 2.1. Thứ tự Ưu tiên trong Event Loop
1. **Call Stack:** Thực thi toàn bộ mã đồng bộ hiện tại.
2. **Microtask Queue:** Ngay khi Call Stack trống, toàn bộ microtasks (`Promise.then`, `queueMicrotask`, `process.nextTick`) được xả sạch trước.
3. **Macrotask Queue:** Event Loop lấy 1 tác vụ macrotask (`setTimeout`, `setInterval`, I/O sự kiện UI) đưa vào Call Stack, sau đó lặp lại chu kỳ.

### 2.2. Cơ chế Retry với Exponential Backoff & Jitter
Khi API gặp sự cố tạm thời (mã lỗi 429 Too Many Requests, 503 Service Unavailable), việc retry liên tục ngay lập tức sẽ làm sập máy chủ.
- **Exponential Backoff:** Thời gian chờ giữa các lần thử tăng theo cấp số nhân: $T = \text{baseDelay} \times 2^{\text{attempt}}$.
- **Jitter (Độ trễ ngẫu nhiên):** Thêm một khoảng ngẫu nhiên nhỏ $\pm \Delta$ để phân tán các request, tránh hiện tượng hàng nghìn client cùng retry đúng 1 thời điểm (Thundering Herd Problem).

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu

```javascript
// Hàm tự phục hồi: Retry với Exponential Backoff
async function retryWithBackoff(fn, maxRetries = 3, baseDelay = 300) {
  let attempt = 0;
  while (true) {
    try {
      return await fn(); // Thực thi tác vụ bất đồng bộ
    } catch (error) {
      attempt++;
      if (attempt > maxRetries) {
        throw new Error(`Đã thử ${maxRetries} lần nhưng thất bại: ${error.message}`);
      }
      // Tính độ trễ cấp số nhân kèm Jitter ngẫu nhiên
      const delay = baseDelay * Math.pow(2, attempt - 1) + Math.random() * 100;
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }
}
```

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Dùng `forEach` với async/await: `arr.forEach(async x => await api(x))` | `forEach` không chờ Promise giải quyết, code chạy không đồng bộ ngoài tầm kiểm soát | Dùng vòng lặp `for...of` cho tuần tự, hoặc `Promise.all(arr.map(...))` cho song song |
| Dùng `Promise.all` khi có 1 task lỗi nhỏ không nghiêm trọng | Toàn bộ các task thành công khác bị từ chối và mất trắng kết quả (Fail-Fast) | Dùng `Promise.allSettled()` để nhận diện chi tiết kết quả từng task độc lập |
| Bỏ quên `try/catch` xung quanh câu lệnh `await` | Unhandled Promise Rejection làm crash tiến trình Node.js hoặc treo UI | Luôn bọc trong khối `try...catch...finally` hoặc xử lý lỗi tập trung |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Thứ tự thực thi microtasks và macrotasks đã được tính toán chính xác chưa?
- [ ] Các tác vụ gọi API không ổn định đã có cơ chế Retry Backoff và Timeout bảo vệ chưa?
- [ ] Các lỗi mạng có được thông báo rõ ràng cho người dùng thay vì nuốt lỗi âm thầm?

<!-- studxy-writein-v2 -->",
                "Làm chủ Event Loop, Promise Pipeline và thuật toán Retry Exponential Backoff.",
                "Cơ chế tự phục hồi kết nối mạng và xử lý lỗi bất đồng bộ toàn diện."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.CodingLesson32Id,
                "Bài 3.2: Deep Clone, Tính Bất Biến & Xử Lý Lỗi Toàn Diện",
                "deep-clone-immutability-va-xu-ly-loi",
                25,
                @"# Bài 3.2: Deep Clone, Tính Bất Biến & Xử Lý Lỗi Toàn Diện

## 1. Mục tiêu bài học (Learning Objectives)
- Phân biệt bản chất bộ nhớ Heap giữa Sao chép nông (Shallow Copy) và Sao chép sâu (Deep Clone).
- Tự cài đặt hàm đệ quy Deep Clone an toàn xử lý được Array, Object, Date, RegExp và Vòng lặp tham chiếu (Circular Reference).
- Sử dụng thành thạo API chuẩn hiện đại `structuredClone()` và nắm vững các giới hạn của nó (hàm, DOM node, symbol).
- Xây dựng kiến trúc xử lý ngoại lệ theo tầng (Layered Error Handling) với các Custom Error Class có định danh rõ ràng.

---

## 2. Khái niệm & Mô hình Cốt lõi

### 2.1. Phân biệt Shallow Copy vs Deep Clone
- **Shallow Copy (`{ ...obj }`, `[...arr]`, `Object.assign()`):** Chỉ tạo bản sao mới cho thuộc tính ở tầng thứ nhất (level 1). Nếu object chứa object/array lồng nhau, chúng vẫn trỏ cùng địa chỉ ô nhớ trong Heap. Mọi thay đổi ở tầng con sẽ làm biến đổi đối tượng gốc!
- **Deep Clone:** Sao chép đệ quy mọi tầng cấu trúc dữ liệu, tạo ra thực thể hoàn toàn độc lập trong bộ nhớ.

### 2.2. Vấn đề Vòng lặp Tham chiếu (Circular Reference)
Khi `obj.self = obj`, hàm đệ quy thông thường sẽ gọi vô tận dẫn tới lỗi `RangeError: Maximum call stack size exceeded`. Giải pháp là dùng một `WeakMap` ghi nhớ các object đã từng duyệt qua; nếu gặp lại object cũ thì lập tức trả về tham chiếu đã clone.

---

## 3. Ví dụ Thực tế Minh họa Chuyên sâu

```javascript
// Hàm Deep Clone toàn diện hỗ trợ Circular References & Types
function safeDeepClone(value, hash = new WeakMap()) {
  if (value === null || typeof value !== 'object') return value;
  if (value instanceof Date) return new Date(value.getTime());
  if (value instanceof RegExp) return new RegExp(value.source, value.flags);

  // Xử lý vòng lặp tham chiếu
  if (hash.has(value)) return hash.get(value);

  const clone = Array.isArray(value) ? [] : {};
  hash.set(value, clone);

  for (const key of Object.keys(value)) {
    clone[key] = safeDeepClone(value[key], hash);
  }

  return clone;
}

// Thử nghiệm
const original = { name: 'Studxy', config: { theme: 'dark' } };
const duplicated = safeDeepClone(original);
duplicated.config.theme = 'light';
console.log(original.config.theme); // Vẫn là 'dark' - Tính bất biến được bảo toàn!
```

---

## 4. Sai lầm Hay gặp & Cách Khắc phục

| Sai lầm phổ biến | Hậu quả thực tế | Cách khắc phục chuẩn mực |
|---|---|---|
| Dùng `JSON.parse(JSON.stringify(obj))` để deep clone | Làm mất kiểu `Date` (biến thành string), mất `undefined`, `NaN`, `RegExp`, ném lỗi nếu có vòng lặp tham chiếu | Dùng `structuredClone()` có sẵn trong trình duyệt hiện đại hoặc hàm clone đệ quy chuyên dụng |
| Dùng toán tử spread `[...matrix]` để clone ma trận 2 chiều | Các hàng bên trong vẫn là tham chiếu chung, sửa ô `matrix[0][0]` sẽ ảnh hưởng cả bản sao | Clone từng hàng: `matrix.map(row => [...row])` |
| Bắt lỗi chung chung bằng `catch (e) {}` mà không rethrow hoặc ghi log | Che giấu lỗi hệ thống, dẫn đến bug ngầm cực kỳ khó xác định nguyên nhân | Luôn phân loại lỗi theo `instanceof` và chỉ xử lý ngoại lệ có thể phục hồi |

---

## 5. Checklist Áp dụng Thực chiến
- [ ] Các đối tượng lồng nhau có bị đột biến ngoài ý muốn sau khi sửa đổi không?
- [ ] Đã kiểm tra trường hợp dữ liệu có `Date`, `null`, `undefined` hoặc tham chiếu vòng chưa?
- [ ] Lỗi được phân loại bằng Custom Error Class chuẩn mực thay vì ném chuỗi thuần túy?

<!-- studxy-writein-v2 -->",
                "Sao chép sâu Deep Clone an toàn với Circular References và WeakMap.",
                "Bảo toàn tính bất biến cấu trúc dữ liệu và xử lý ngoại lệ theo tầng."
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

<!-- studxy-writein-v2 -->",
                "Lãi kép và chiến lược DCA trung bình giá loại bỏ rủi ro tâm lý chọn thời điểm.",
                "Công thức toán học hàm mũ và bảo vệ sức mua trước lạm phát."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson12Id,
                "Bài 1.2: Kế Hoạch Tự Do Tài Chính (FIRE) & Quản Lý Dòng Tiền",
                "ke-hoach-tu-do-tai-chinh-fire-va-dong-tien",
                25,
                @"# Bài 1.2: Kế Hoạch Tự Do Tài Chính (FIRE) & Quản Lý Dòng Tiền

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

<!-- studxy-writein-v2 -->",
                "Kế hoạch tự do tài chính FIRE, nghiên cứu Trinity và quy tắc 4%.",
                "Tối ưu hóa tỷ lệ tiết kiệm ròng và dứt điểm các khoản nợ lãi cao."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson21Id,
                "Bài 2.1: Phân Bổ Tài Sản & Tương Quan Giữa Các Lớp Tài Sản",
                "phan-bo-tai-san-va-tuong-quan",
                25,
                @"# Bài 2.1: Phân Bổ Tài Sản & Tương Quan Giữa Các Lớp Tài Sản

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

<!-- studxy-writein-v2 -->",
                "Lý thuyết danh mục hiện đại Harry Markowitz và hệ số tương quan đa tài sản.",
                "Thiết kế danh mục All-Weather phòng thủ khủng hoảng và kiểm soát biến động."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson22Id,
                "Bài 2.2: Quản Trị Rủi Ro & Mức Sụt Giảm Tối Đa (Max Drawdown)",
                "quan-tri-rui-ro-va-max-drawdown",
                25,
                @"# Bài 2.2: Quản Trị Rủi Ro & Mức Sụt Giảm Tối Đa (Max Drawdown)

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

<!-- studxy-writein-v2 -->",
                "Đo lường mức sụt giảm tối đa Max Drawdown và toán học hòa vốn bất đối xứng.",
                "Quy trình tái cân bằng danh mục và kiểm thử áp lực Stress Testing."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson31Id,
                "Bài 3.1: Biên An Toàn & Định Giá Doanh Nghiệp Cơ Bản",
                "bien-an-toan-va-dinh-gia-doanh-nghiep",
                30,
                @"# Bài 3.1: Biên An Toàn & Định Giá Doanh Nghiệp Cơ Bản

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

<!-- studxy-writein-v2 -->",
                "Nguyên lý Biên an toàn Benjamin Graham và các chỉ số định giá P/E, ROE, FCF.",
                "Tránh bẫy giá trị và thẩm định con hào kinh tế của doanh nghiệp."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.InvestingLesson32Id,
                "Bài 3.2: Tâm Lý Học Hành Vi & Quản Trị Cảm Xúc Trong Đầu Tư",
                "tam-ly-hoc-hanh-vi-va-quan-tri-cam-xuc",
                25,
                @"# Bài 3.2: Tâm Lý Học Hành Vi & Quản Trị Cảm Xúc Trong Đầu Tư

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

<!-- studxy-writein-v2 -->",
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

<!-- studxy-writein-v2 -->",
                "Mô hình hai hệ thống tư duy Kahneman và hiệu ứng can thiệp Stroop.",
                "Giao tiếp phi bạo lực NVC 4 bước và kỹ thuật kích hoạt Hệ thống 2."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson12Id,
                "Bài 1.2: Ảo Tưởng Trí Nhớ & Thiên Kiến Xác Nhận (Confirmation Bias)",
                "ao-tuong-tri-nho-va-thien-kien-xac-nhan",
                25,
                @"# Bài 1.2: Ảo Tưởng Trí Nhớ & Thiên Kiến Xác Nhận (Confirmation Bias)

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

<!-- studxy-writein-v2 -->",
                "Ảo tưởng trí nhớ và thiên kiến xác nhận cản trở tư duy phản biện.",
                "Phương pháp bác bỏ giả thuyết Karl Popper và kỹ thuật Steelman."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson21Id,
                "Bài 2.1: Dung Lượng Trí Nhớ Ngắn Hạn & Sức Mạnh Kỹ Thuật Chunking",
                "dung-luong-tri-nho-va-ky-thuat-chunking",
                25,
                @"# Bài 2.1: Dung Lượng Trí Nhớ Ngắn Hạn & Sức Mạnh Kỹ Thuật Chunking

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

<!-- studxy-writein-v2 -->",
                "Quy luật George Miller 7 ± 2 và kỹ thuật gom cụm Chunking mở rộng bộ nhớ.",
                "Tối ưu dung lượng Working Memory và nguyên tắc thẻ nhớ nguyên tử FSRS."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson22Id,
                "Bài 2.2: Đường Cong Quên Lãng Ebbinghaus & Tái Hiện Chủ Động",
                "duong-cong-ebbinghaus-va-tai-hien-chu-dong",
                25,
                @"# Bài 2.2: Đường Cong Quên Lãng Ebbinghaus & Tái Hiện Chủ Động

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

<!-- studxy-writein-v2 -->",
                "Đường cong quên lãng Ebbinghaus và phương pháp Retrieval Practice.",
                "Tái hiện chủ động thay thế đọc lại thụ động, củng cố synap thần kinh dài hạn."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson31Id,
                "Bài 3.1: Mô Hình ABC Nhận Thức & Thách Thức Suy Nghĩ Tự Động",
                "mo-hinh-cbt-abc-va-tai-cau-truc-nhan-thuc",
                30,
                @"# Bài 3.1: Mô Hình ABC Nhận Thức & Thách Thức Suy Nghĩ Tự Động

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

<!-- studxy-writein-v2 -->",
                "Liệu pháp nhận thức hành vi CBT, mô hình ABCDE của Ellis và Beck.",
                "Nhận diện 4 lỗi nhận thức kinh điển và tái cấu trúc suy nghĩ cân bằng."
            ),

            new LessonTheoryMeta(
                SeedDataInitializer.PsychologyLesson32Id,
                "Bài 3.2: Thiết Lập Ranh Giới Cá Nhân & Tự Trắc Ẩn (Self-Compassion)",
                "thiet-lap-ranh-gioi-va-tu-trac-an",
                25,
                @"# Bài 3.2: Thiết Lập Ranh Giới Cá Nhân & Tự Trắc Ẩn (Self-Compassion)

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

<!-- studxy-writein-v2 -->",
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
            if (existing != null)
            {
                // Safe idempotency: ONLY upgrade if CurrentVersion == 1 and marker is not yet present.
                // If user modified lesson (CurrentVersion > 1) or it is already upgraded, DO NOT overwrite!
                if (existing.CurrentVersion == 1 && !(existing.ContentMarkdown ?? string.Empty).Contains(UpgradeMarker))
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
