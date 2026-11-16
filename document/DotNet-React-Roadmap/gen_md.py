import os

md1 = """# Chặng 1: C# Cơ bản (3-4 tuần)
**Mục tiêu**: Bắt đầu lại từ đầu, nắm chắc tư duy lập trình và cú pháp cơ bản của C#. Xây dựng nền tảng vững chắc thông qua phương pháp học: Tập trung → Hiểu → Tự nhớ lại → Thực hành → Sai → Sửa → Lặp lại cách quãng.

**Dự án xuyên suốt**: App Quản lý Chi tiêu (Bản Console)

## Tuần 1: Căn bản & Các khái niệm lõi

### Buổi 1: Chạy chương trình, Console.WriteLine

**1. Tập trung (Focused Learning):**
C# là ngôn ngữ biên dịch. Máy tính không hiểu C# trực tiếp, nên chúng ta cần trình biên dịch (.NET) dịch C# sang mã máy. Hãy làm quen với cấu trúc tối thiểu của một ứng dụng Console.

**2. Hiểu (Understanding):**
Hàm `Main` là "cánh cửa" để chương trình bắt đầu chạy.
Lệnh `Console.WriteLine("Hello World!");` ra lệnh cho máy tính in ra một dòng chữ. `Console` là cửa sổ dòng lệnh, `WriteLine` là hành động viết rồi xuống dòng.

**3. Tự nhớ lại (Active Recall):**
- Làm sao để in một dòng chữ ra màn hình? Lệnh nào được sử dụng?
- Điểm khác biệt giữa `Write` và `WriteLine` là gì? (Gợi ý: Xuống dòng).
- Tại sao phải có dấu `;` ở cuối lệnh?

**4. Thực hành (Practice):**
**Bài tập tự làm**: In ra màn hình tên của bạn, tuổi, và một câu châm ngôn bạn thích.
```csharp
Console.WriteLine("Tên: Vibe Coder");
Console.WriteLine("Tuổi: 25");
Console.WriteLine("Châm ngôn: Code ít lỗi nhiều.");
```

**5. Lỗi thường gặp (Sai và Sửa):**
- **Lỗi:** Quên dấu chấm phẩy `;` ở cuối câu.
- **Sửa:** C# bắt buộc mọi câu lệnh kết thúc bằng dấu `;`.
- **Lỗi:** Viết hoa sai (VD: `console.writeline`).
- **Sửa:** C# phân biệt hoa thường (Case-sensitive). Chữ cái đầu tiên của Class và Method luôn phải viết hoa.

### Buổi 2: Biến, int, decimal, string, phép toán

**1. Hiểu:**
Biến như những "chiếc hộp" chứa dữ liệu. Kiểu dữ liệu quyết định hộp đó chứa được loại gì. Dùng `decimal` cho tiền tệ thay vì `double` để tránh sai số (vì double lưu số dạng dấu phẩy động cơ số 2, gây ra sai số nhỏ trong tính toán thập phân).

**2. Tự nhớ lại:**
- Tại sao dùng `decimal` cho tiền bạc mà không phải `int` hay `double`?
- Khai báo một biến kiểu chuỗi (`string`) như thế nào? Cần dùng dấu ngoặc kép hay ngoặc đơn?

**3. Thực hành:**
Khai báo 3 khoản chi (tiền điện, nước, internet) và tính tổng.
```csharp
decimal tienDien = 500000m;
decimal tienNuoc = 150000m;
decimal tienInternet = 300000m;
decimal tongChi = tienDien + tienNuoc + tienInternet;
Console.WriteLine("Tổng chi: " + tongChi);
```

**4. Thử thách thêm:**
Tìm hiểu về string interpolation (`$`) để in chuỗi kết hợp biến một cách tự nhiên.
`Console.WriteLine($"Tổng chi tháng này là: {tongChi} VNĐ");`

### Buổi 3: Console.ReadLine, chuyển kiểu

**1. Hiểu:**
`Console.ReadLine()` cho phép người dùng nhập vào từ bàn phím. Mọi thứ người dùng nhập đều được C# hiểu là `string`. Muốn tính toán, phải chuyển kiểu (Convert / Parse).

**2. Thực hành:**
Nhập 3 khoản chi từ bàn phím và in ra tổng.
```csharp
Console.Write("Nhập tiền điện: ");
string inputDien = Console.ReadLine();
decimal dien = decimal.Parse(inputDien);
// Làm tương tự với nước và internet...
```

**3. Lỗi thường gặp:**
- **Lỗi:** `FormatException` khi người dùng nhập chữ (VD: "năm trăm") thay vì số.
- **Sửa:** Dùng `decimal.TryParse` thay vì `Parse` để chương trình không bị văng lỗi.

### Buổi 4: if/else, phép so sánh

**1. Hiểu:**
Sử dụng `if/else` để rẽ nhánh logic. Phép so sánh (`>`, `<`, `==`, `!=`) trả về `bool` (đúng hoặc sai).

**2. Thực hành:**
Nhập 2 khoản chi, in ra khoản nào lớn hơn, khoản nào nhỏ hơn.
**Thực tế**: Trong app quản lý chi tiêu, kiểm tra nếu khoản chi bị âm thì báo lỗi, vì không có ai chi "-50 ngàn" cả.

### Buổi 5 & 6: Tổng hợp & Lặp lại cách quãng

**1. Thực hành (Buổi 5):**
Kết hợp tất cả kiến thức: Nhập ngân sách của tháng. Nhập tổng chi. Báo dư hoặc vượt ngân sách. Nếu vượt, cảnh báo màu đỏ (gợi ý: `Console.ForegroundColor`).

**2. Spaced Repetition (Buổi 6):**
Xóa trắng file code của Buổi 5. Tự tay gõ lại toàn bộ logic từ bộ nhớ. Quên cú pháp thì tra Google, không copy-paste.

## Tuần 2: Vòng lặp, Hàm, và Cấu trúc dữ liệu

### Buổi 7 & 8: Vòng lặp for và while

**1. Hiểu:**
`for` dùng khi biết trước số lần lặp (VD: nhập 5 khoản chi). `while` dùng khi chỉ biết điều kiện dừng (VD: nhập đến khi gõ "exit").

**2. Thực hành:**
- **Buổi 7**: Nhập số lượng khoản chi N, sau đó vòng lặp N lần để nhập số tiền.
- **Buổi 8**: Nhập liên tục đến khi người dùng gõ 'exit'.

**3. Lỗi thường gặp:**
Vòng lặp vô hạn (Infinite loop) do quên tăng/giảm biến đếm (`i++`) hoặc sai điều kiện dừng trong `while`. Nếu bị kẹt, dùng tổ hợp phím Ctrl + C để dừng Console.

### Buổi 9: Hàm (Methods)

**1. Hiểu:**
Hàm giúp chia nhỏ code, dễ tái sử dụng. Hàm như một chiếc máy: Đầu vào là tham số (parameters), đầu ra là giá trị trả về (return type).

**2. Tự nhớ lại:**
- Sự khác biệt giữa tham số (parameter) và đối số (argument) là gì?
- Hàm `void` có trả về giá trị không?

**3. Thực hành:**
Viết hàm `GetValidAmount()` luôn yêu cầu người dùng nhập lại nếu nhập sai định dạng hoặc nhập số âm.
```csharp
static decimal GetValidAmount() {
    // dùng while(true) và decimal.TryParse
}
```

### Buổi 10 & 11: Array và List

**1. Hiểu:**
`Array` có kích thước cố định (Khai báo mảng 5 phần tử là cố định 5). `List<T>` có thể co giãn. Trong 90% trường hợp thực tế, người ta dùng `List`.

**2. Thực hành:**
Lưu các khoản chi vào `List<decimal>`. Dùng vòng lặp `foreach` để: Tính tổng, tìm lớn nhất, lọc các khoản vượt mức.

### Buổi 12: Debug và ôn tập

**1. Hiểu:**
Debug là kỹ năng sống còn. Đặt breakpoint (F9), chạy debug (F5), đi từng bước (F10), nhảy vào hàm (F11).
**2. Thực hành:**
Tự tạo một file code cố tình bị lỗi tính toán sai (logic error). Dùng debugger, watch variables để tìm ra biến nào đang chứa giá trị sai.
"""

md2 = """# Chặng 2: Giải bài toán và Debug (2-3 tuần)
**Mục tiêu**: Hình thành tư duy giải quyết vấn đề (problem-solving), kỹ năng đọc lỗi và cấu trúc dữ liệu nâng cao. Xây dựng tư duy phân tích hệ thống.

## 1. Tư duy phân rã (Decomposition)
**1. 🎯 Tập trung (Focused Learning):**
Đứng trước một bài toán lớn, não bộ có xu hướng bị ngợp. Phân rã bài toán là kỹ năng chia nhỏ một vấn đề phức tạp thành các bước nhỏ, có thể lập trình được. Kỹ năng này không chỉ giúp viết code dễ hơn mà còn giúp ước lượng thời gian chính xác hơn.
**2. 📖 Hiểu (Understanding):**
Ví dụ: "Làm chức năng đăng nhập".
Phân rã: (1) Nhận email/password từ user. (2) Kiểm tra email có đúng định dạng không. (3) Tìm email trong Database. (4) So sánh password. (5) Trả về thông báo thành công hoặc lỗi.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Làm sao để biết một task đã được chia đủ nhỏ?
- Tại sao phải phân rã trước khi viết code?
- Làm thế nào để áp dụng nguyên lý "Top-Down Design" vào quá trình phân rã?
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Phân rã hệ thống quản lý nhà hàng:
  - 1. Đặt bàn: check bàn trống, lưu thông tin khách.
  - 2. Gọi món: Chọn món, tính tổng tiền, gửi thông tin xuống bếp.
  - 3. Thanh toán: Áp dụng voucher, in hóa đơn.
- **Bài 2:** Phân rã bài toán máy rút tiền ATM: Xác thực thẻ, kiểm tra PIN, chọn số tiền, kiểm tra số dư, trừ tiền, nhả tiền, in biên lai.
- **Bài 3:** Phân rã hệ thống giỏ hàng e-commerce: Thêm sản phẩm, cập nhật số lượng, xóa sản phẩm, tính tổng phí vận chuyển dựa trên khu vực.
**5. ❌ Sai (Common Mistakes):**
- Bắt tay vào code ngay khi đọc đề bài mà không có kế hoạch, dẫn đến viết mã spaghetti, các thành phần dính chặt vào nhau.
- Phân rã quá to (ví dụ: Task "Xử lý thanh toán" cần chia nhỏ thành kiểm tra số dư, giao tiếp API ngân hàng, lưu log).
**6. ✅ Sửa (Fix/Debug):**
- Ép bản thân viết các gạch đầu dòng (comments) ra file code trước, sau đó mới viết code dưới mỗi comment.
- Sử dụng sơ đồ khối (Flowchart) hoặc mindmap để hình dung hệ thống.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- 3 ngày sau, thử phân rã lại bài toán "Hệ thống máy ATM rút tiền" xem có bỏ sót bước nào không (ví dụ: trừ tiền trong tài khoản, khóa thẻ nếu sai PIN 3 lần).

## 2. Dictionary và HashSet
**1. 🎯 Tập trung (Focused Learning):**
Dictionary lưu trữ dữ liệu dạng Key-Value (Khóa-Giá trị). HashSet lưu trữ tập hợp các phần tử không trùng lặp. Tốc độ truy xuất cực nhanh: O(1) nhờ cơ chế Hashing.
**2. 📖 Hiểu (Understanding):**
- `Dictionary<string, int>` giống như danh bạ điện thoại: Khóa là Tên, Giá trị là Số điện thoại.
- `HashSet<string>` giống như danh sách khách mời: Không ai được mời 2 lần.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Khi nào dùng List, khi nào dùng Dictionary?
- HashSet khác gì List?
- Điều gì xảy ra khi hai Key khác nhau tạo ra cùng một mã Hash (Hash collision)?
**4. 💻 Thực hành (Practice):**
- **Bài 1 (Đếm từ):** Cho 1 đoạn văn bản dài. Sử dụng `Dictionary<string, int>` để đếm tần suất xuất hiện của mỗi từ. (Bỏ qua dấu câu và không phân biệt hoa thường).
- **Bài 2 (Lọc trùng lặp):** Đọc một danh sách gồm 10,000 email học sinh, trong đó có nhiều email bị lặp lại. Dùng `HashSet` để lọc ra danh sách email duy nhất và in ra tổng số.
- **Bài 3 (Nhóm sinh viên):** Cho danh sách điểm sinh viên. Gom nhóm sử dụng Dictionary, trong đó Key là loại điểm (A, B, C, D) và Value là `List<string>` chứa tên các sinh viên đạt loại điểm đó.
**5. ❌ Sai (Common Mistakes):**
- Thêm một Key đã tồn tại vào Dictionary gây ra `ArgumentException`.
- Truy cập vào một Key không tồn tại bằng indexer `dict[key]` gây ra `KeyNotFoundException`.
**6. ✅ Sửa (Fix/Debug):**
- Luôn kiểm tra `if (!dict.ContainsKey(key))` trước khi thêm.
- Tốt nhất nên dùng `dict.TryGetValue(key, out var value)` để lấy dữ liệu an toàn.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Cuối tuần, làm lại bài đếm từ nhưng với biến thể: Sắp xếp các từ theo tần suất xuất hiện giảm dần.

## 3. Thuật toán cơ bản & Big O Notation
**1. 🎯 Tập trung (Focused Learning):**
Big O Notation là ngôn ngữ toán học dùng để mô tả tốc độ và hiệu suất của một thuật toán khi kích thước đầu vào (N) tăng lên rất lớn.
**2. 📖 Hiểu (Understanding):**
- Bảng so sánh Big O:
  - **O(1) - Constant:** Tra cứu Dictionary, HashSet. Rất nhanh, không đổi dù dữ liệu lớn.
  - **O(log N) - Logarithmic:** Tìm kiếm nhị phân (Binary Search). Cắt đôi dữ liệu mỗi bước.
  - **O(N) - Linear:** Duyệt mảng bằng 1 vòng lặp for.
  - **O(N log N) - Linearithmic:** Thuật toán sắp xếp QuickSort, MergeSort.
  - **O(N^2) - Quadratic:** Hai vòng lặp for lồng nhau. Rất chậm!
**3. 🧠 Tự nhớ lại (Active Recall):**
- Tại sao O(N^2) lại nguy hiểm khi xử lý dữ liệu lớn (ví dụ N = 100,000)?
- Nếu tăng N lên gấp đôi, O(N^2) sẽ tăng thời gian chạy lên bao nhiêu lần?
**4. 💻 Thực hành (Practice):**
- **Bài 1 (Bubble Sort):** Tự viết thuật toán sắp xếp nổi bọt mảng số nguyên. In ra mảng sau mỗi bước đổi chỗ.
- **Bài 2 (Đo thời gian chạy):** Sinh ra 1 mảng số ngẫu nhiên gồm 50,000 phần tử. Dùng class `Stopwatch` đo thời gian chạy của Bubble Sort tự viết so sánh với hàm `List.Sort()` có sẵn của C#.
- **Bài 3 (Binary Search):** Tự viết hàm tìm kiếm nhị phân trên một mảng đã sắp xếp. Đo thời gian so với tìm kiếm tuyến tính (dùng vòng for từ đầu đến cuối).
**5. ❌ Sai (Common Mistakes):**
- Viết 2 vòng lặp lồng nhau vô tội vạ để tìm kiếm (tạo ra O(N^2)) trong khi có thể dùng Dictionary để giảm xuống O(N).
**6. ✅ Sửa (Fix/Debug):**
- Nhận diện O(N^2): Nếu thấy `for` lồng `for`, hãy tự hỏi "Có thể đưa danh sách thứ 2 vào HashSet/Dictionary trước khi chạy vòng lặp thứ 1 không?".
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Sau 1 tuần, viết lại Bubble Sort từ đầu mà không nhìn tài liệu, cố gắng tối ưu bằng cách dừng sớm nếu mảng đã được sắp xếp.

## 4. Debugging Nâng Cao
**1. 🎯 Tập trung (Focused Learning):**
Debugging là kỹ năng quan trọng nhất của lập trình viên. Đó là quá trình điều tra, theo dõi luồng thực thi và giá trị biến để tìm ra nguyên nhân gốc rễ của lỗi.
**2. 📖 Hiểu (Understanding):**
- Breakpoint (F9): Đánh dấu điểm dừng trên IDE.
- Step Over (F10): Chạy dòng hiện tại, nhảy sang dòng tiếp theo (không đi vào bên trong hàm).
- Step Into (F11): Đi sâu vào bên trong thân hàm.
- Call Stack: Cửa sổ cho thấy thứ tự các hàm đã gọi nhau dẫn đến vị trí lỗi hiện tại.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Khác biệt giữa Step Over và Step Into?
- Cửa sổ Watch và Locals dùng để làm gì?
**4. 💻 Thực hành (Practice):**
- **Bài 1 (IndexOutOfRange):** Viết vòng lặp `for (int i = 0; i <= arr.Length; i++)`. Cố tình gây lỗi. Dùng debugger để xem biến `i` đạt giá trị nào khi chương trình văng lỗi.
- **Bài 2 (NullReference):** Tạo object `Student s = null;` sau đó gọi `s.Name`. Chạy debug, dùng Call Stack mò ngược lên để xem `s` bị null từ đâu.
- **Bài 3 (StackOverflow):** Viết hàm đệ quy tính giai thừa bị thiếu điều kiện dừng `if (n == 0)`. Khi app crash, kiểm tra cửa sổ Call Stack sẽ thấy hàng ngàn hàm giống nhau đang chờ.
- **Bài 4 (Infinite Loop):** Viết vòng `while (true)` quên lệnh `break`. Khi app treo cứng, bấm "Break All" (Pause) trong IDE để xem chương trình đang kẹt ở dòng code nào.
**5. ❌ Sai (Common Mistakes):**
- Dùng `Console.WriteLine` để in ra mọi giá trị biến để dò lỗi thay vì dùng tính năng Debug của IDE.
**6. ✅ Sửa (Fix/Debug):**
- Bắt buộc dùng Breakpoint. Học cách đặt Conditional Breakpoint (chuột phải vào điểm đỏ, chọn Condition: ví dụ chỉ dừng khi `i == 5000` để khỏi phải bấm F10 quá nhiều).
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Mỗi khi code bị lỗi ở các bài tập sau, tuyệt đối không dùng `Console.WriteLine` để debug. Bắt buộc dùng IDE Debugger.

## 5. LeetCode Easy (Luyện tư duy)
- **Bài 1: Two Sum (#1)** - Dùng vòng lặp kép O(N^2), sau đó tối ưu bằng Dictionary O(N).
- **Bài 2: Palindrome Number (#9)** - Kiểm tra một số đọc từ trái qua phải hay ngược lại đều giống nhau (không chuyển sang chuỗi).
- **Bài 3: Roman to Integer (#13)** - Ứng dụng xuất sắc của Dictionary để lưu trữ bảng map ký tự La Mã.
- **Bài 4: Valid Parentheses (#20)** - Sử dụng cấu trúc dữ liệu Stack (ngăn xếp) để kiểm tra tính hợp lệ của dấu ngoặc.
- **Bài 5: Remove Duplicates from Sorted Array (#26)** - Thao tác với mảng In-place, sử dụng hai con trỏ (Two Pointers).
- **Bài 6: Find the Index of the First Occurrence in a String (#28)** - Xử lý chuỗi, implement lại hàm `IndexOf`.
- **Bài 7: Length of Last Word (#58)** - Luyện kỹ năng split và duyệt chuỗi ngược từ cuối mảng.
- **Bài 8: Contains Duplicate (#217)** - Dùng HashSet để giải nhanh trong O(N) thời gian và bộ nhớ.
"""

md3 = """# Chặng 3: C# cho Ứng dụng nhỏ (3-4 tuần)
**Mục tiêu**: Nắm vững Lập trình hướng đối tượng (OOP), quản lý file, xử lý JSON, và làm việc với Git, Unit Test.

## 1. Lập trình Hướng đối tượng (OOP) & SOLID
**1. 🎯 Tập trung (Focused Learning):**
OOP là mô hình lập trình dựa trên việc tạo ra các thực thể (Object) mô phỏng thế giới thực, chứa dữ liệu (thuộc tính) và hành vi (phương thức). SOLID (đặc biệt là nguyên lý Single Responsibility - Đơn trách nhiệm) giúp code dễ đọc, dễ sửa và bảo trì.
**2. 📖 Hiểu (Understanding):**
- **Class vs Object**: `Class` là bản thiết kế (ví dụ: Bản vẽ ô tô). `Object` là thực thể cụ thể (ví dụ: Chiếc ô tô Toyota màu đỏ).
- **Inheritance (Kế thừa)**: Tái sử dụng code. `Manager` kế thừa `Employee`.
- **Polymorphism (Đa hình)**: Cho phép một interface đại diện cho nhiều class khác nhau. `IShape` có hàm `Draw()`. `Circle` và `Square` implement hàm đó theo cách riêng.
- **Single Responsibility Principle (SRP)**: Một class chỉ làm một việc duy nhất. Class quản lý dữ liệu không nên chứa logic in ra màn hình Console.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Tại sao phải đóng gói (Encapsulation) bằng `get/set` thay vì public các biến?
- Interface khác gì Abstract Class?
- Hậu quả nếu nhồi nhét mọi xử lý vào class `Program` là gì?
**4. 💻 Thực hành (Practice):**
- **Bài 1 (Kế thừa & Đa hình):** Tạo interface `IPaymentMethod` với phương thức `Pay(decimal amount)`. Viết các class `CreditCardPayment`, `PaypalPayment`, `CryptoPayment` implement interface này. Viết vòng lặp gọi hàm `Pay()` cho từng loại.
- **Bài 2 (Đóng gói):** Tạo class `Employee` với thuộc tính `Salary`. Đảm bảo `Salary` không bao giờ bị set thành số âm. Chỉ được phép tăng lương qua hàm `IncreaseSalary(decimal percent)`.
- **Bài 3 (SRP):** Tái cấu trúc (Refactor) ứng dụng quản lý chi tiêu: Tạo riêng một `ExpenseRepository` để lưu/đọc file, một `ExpenseService` để xử lý logic thêm/xóa, và một `ConsoleUI` để in ra màn hình.
**5. ❌ Sai (Common Mistakes):**
- Dùng Kế thừa (Inheritance) một cách bừa bãi khi không có mối quan hệ "IS-A". (Ví dụ: Class `Engine` kế thừa `Car` là sai, `Car` CHỨA `Engine` mới đúng - Composition).
**6. ✅ Sửa (Fix/Debug):**
- Ưu tiên Composition over Inheritance (Gắn kết thay vì kế thừa). Interface là giải pháp an toàn hơn kế thừa class trực tiếp.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Tuần sau, áp dụng OOP để thiết kế hệ thống Quản lý thư viện (Sách, Người mượn, Giao dịch mượn/trả).

## 2. Exceptions (Ngoại lệ)
**1. 🎯 Tập trung (Focused Learning):**
Lỗi ngoại lệ (Exception) xảy ra trong lúc chạy (Runtime) và làm ứng dụng sập (Crash). Ta cần chủ động bắt và xử lý chúng an toàn.
**2. 📖 Hiểu (Understanding):**
- `try-catch`: Đoạn code nguy hiểm nằm trong `try`, nếu có lỗi sẽ nhảy vào `catch`.
- `finally`: Khối code luôn luôn chạy dù có lỗi hay không, thường dùng để dọn dẹp tài nguyên (đóng file, ngắt kết nối database).
- `throw`: Chủ động phát tín hiệu lỗi khi dữ liệu đầu vào không hợp lệ.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Sự khác biệt giữa `throw e;` và `throw;` trong khối catch là gì?
- Tại sao khối finally lại quan trọng khi làm việc với FileStream?
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Viết chương trình máy tính bỏ túi. Bắt lỗi chia cho số 0 (`DivideByZeroException`) và lỗi nhập sai định dạng (`FormatException`).
- **Bài 2 (Custom Exception):** Tạo class `InvalidAgeException` kế thừa từ `Exception`. Trong hàm tạo người dùng, nếu tuổi < 18, chủ động `throw new InvalidAgeException()`.
- **Bài 3:** Viết một đoạn code mở file đọc dữ liệu. Bất chấp việc đọc thành công hay văng lỗi, phải luôn in ra câu "File closed." ở khối finally.
**5. ❌ Sai (Common Mistakes):**
- Swallow Exception: Bắt lỗi nhưng để trống khối catch. Dẫn đến ứng dụng chạy sai mà không có bất kỳ log lỗi nào.
- Bắt lỗi chung chung bằng `catch (Exception ex)` che giấu các lỗi bộ nhớ nguy hiểm.
**6. ✅ Sửa (Fix/Debug):**
- Luôn log lỗi ra màn hình hoặc ghi vào file log bằng thư viện như Serilog.
- Bắt các lỗi cụ thể từ hẹp đến rộng: `catch (FileNotFoundException)` trước, rồi mới đến `catch (Exception)`.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Mở lại dự án cũ, tìm xem có khối try-catch nào đang "nuốt lỗi" và thêm log vào đó.

## 3. File Handling và JSON
**1. 🎯 Tập trung (Focused Learning):**
Dữ liệu trên bộ nhớ RAM sẽ bị mất khi tắt app. Ghi dữ liệu ra ổ cứng thông qua File và sử dụng định dạng chuẩn (JSON, CSV).
**2. 📖 Hiểu (Understanding):**
- `System.IO.File`: Cung cấp hàm tiện ích đọc/ghi nội dung toàn bộ file.
- `StreamReader`/`StreamWriter`: Đọc/ghi file lớn từng dòng, tối ưu bộ nhớ.
- `System.Text.Json`: Serialize (chuyển Object C# thành chuỗi JSON) và Deserialize (chuyển JSON thành Object C#).
**3. 🧠 Tự nhớ lại (Active Recall):**
- Serialize và Deserialize có ý nghĩa gì?
- File CSV và JSON khác nhau chỗ nào về cách lưu trữ?
- Từ khóa `using` khi mở luồng File dùng để làm gì?
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Làm ứng dụng Quản lý Danh bạ (Contacts). Khi thêm mới, chương trình dùng `JsonSerializer.Serialize` để ghi list contacts vào file `contacts.json`.
- **Bài 2:** Khi mở app, kiểm tra xem `contacts.json` có tồn tại không. Nếu có, dùng `Deserialize` nạp dữ liệu lại vào List.
- **Bài 3 (Xử lý CSV):** Đọc file `employees.csv` chứa hàng ngàn nhân viên. Tính trung bình lương. Bắt buộc dùng `StreamReader.ReadLine()` trong vòng lặp while, không được dùng `File.ReadAllLines` để tiết kiệm RAM.
**5. ❌ Sai (Common Mistakes):**
- Không đóng luồng (Stream) sau khi ghi file. Lần sau mở app sẽ bị báo lỗi "The file is being used by another process".
**6. ✅ Sửa (Fix/Debug):**
- Luôn bao bọc luồng bằng lệnh `using (StreamWriter writer = new StreamWriter("file.txt")) { ... }`. Luồng sẽ tự động bị Dispose khi thoát khỏi block.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Thay đổi chuẩn ghi file của app Quản lý danh bạ từ JSON sang XML hoặc CSV để hiểu sự linh hoạt.

## 4. Unit Test Cơ Bản
**1. 🎯 Tập trung (Focused Learning):**
Viết một đoạn code để tự động kiểm tra xem đoạn code chính có chạy đúng logic không. Giúp phát hiện sớm lỗi khi mở rộng hệ thống.
**2. 📖 Hiểu (Understanding):**
- **AAA Pattern**: Arrange (chuẩn bị dữ liệu), Act (thực thi hàm), Assert (so sánh kết quả thực tế với kết quả mong muốn).
- Dùng `xUnit` framework.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Tại sao không nên viết Unit Test cho logic truy xuất Database thực?
- `Assert.Equal` khác gì `Assert.Throws`?
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Viết class `MathHelper`. Tạo project xUnit và viết các hàm test cho cộng, trừ, nhân, chia (đặc biệt test trường hợp chia 0).
- **Bài 2:** Quay lại hàm `IncreaseSalary` của bài OOP. Viết test case: Tăng 10% cho mức lương 1000 thì kết quả phải là 1100.
- **Bài 3:** Viết test case ném exception: Nếu gọi `IncreaseSalary` với số âm, phải dùng `Assert.Throws` để đảm bảo hệ thống quăng ra lỗi hợp lý.
**5. ❌ Sai (Common Mistakes):**
- Viết hàm Test phụ thuộc lẫn nhau (hàm Test B phụ thuộc kết quả của hàm Test A).
- Logic trong hàm test quá dài dòng phức tạp.
**6. ✅ Sửa (Fix/Debug):**
- Mỗi hàm test phải hoàn toàn độc lập, có thể chạy ngẫu nhiên song song không ảnh hưởng.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Thử nghiệm TDD (Test-Driven Development): Viết test báo lỗi đỏ TRƯỚC, sau đó mới viết logic C# để làm bài test xanh lại.

## 5. Git & Source Control
**1. 🎯 Tập trung (Focused Learning):**
Git là hệ thống quản lý phiên bản mã nguồn, giúp lưu vết thay đổi, phục hồi khi code hỏng và làm việc nhóm. GitHub là máy chủ đám mây lưu trữ code.
**2. 📖 Hiểu (Understanding):**
- `git add`: Đưa file vào vùng chờ (staging).
- `git commit`: Chốt một phiên bản kèm lời nhắn.
- `git branch`: Tạo nhánh tách biệt để thử nghiệm code mà không làm hỏng nhánh `main`.
- `git merge`: Gộp mã nguồn của nhánh này vào nhánh khác.
**3. 🧠 Tự nhớ lại (Active Recall):**
- `git push` và `git pull` khác nhau như thế nào?
- Merge conflict (xung đột) xảy ra khi nào?
- File `.gitignore` dùng để làm gì?
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Chạy `git init` cho dự án C# hiện tại. Tạo file `.gitignore` cho dự án Visual Studio (sử dụng mẫu trên mạng để bỏ qua thư mục `bin`, `obj`).
- **Bài 2 (Branching):** Tạo nhánh mới `feature-json-export`. Sửa đổi một số file. Commit code. Sau đó chuyển lại nhánh `main`, bạn sẽ thấy các file vừa sửa trở về nguyên trạng ban đầu.
- **Bài 3 (Merge):** Gộp nhánh `feature-json-export` vào `main`.
- **Bài 4 (Xử lý Conflict):** Cố tình sửa cùng 1 dòng code ở 2 nhánh khác nhau. Khi merge, Git sẽ báo conflict. Mở IDE và quyết định giữ lại dòng code nào.
**5. ❌ Sai (Common Mistakes):**
- Push thư mục `bin` và `obj` nặng vài trăm MB lên GitHub, làm repo rác.
**6. ✅ Sửa (Fix/Debug):**
- Đảm bảo file `.gitignore` được tạo ngay từ đầu trước lần commit đầu tiên.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Đưa tất cả các bài tập từ chặng 1 đến chặng 3 lên GitHub của riêng bạn, viết `README.md` mô tả từng dự án.
"""

md4 = """# Chặng 4: Database và SQL (2-3 tuần)
**Mục tiêu**: Nắm vững cơ sở dữ liệu quan hệ (Relational Database), SQL Server, thiết kế bảng chuẩn hóa, và các câu lệnh truy vấn nâng cao chuyên nghiệp.

## 1. Thiết kế Database & Chuẩn hóa (Normalization)
**1. 🎯 Tập trung (Focused Learning):**
Thiết kế dữ liệu tốt giúp tránh trùng lặp thông tin (Redundancy) và dị thường dữ liệu (Anomaly). Chuẩn hóa là quá trình tổ chức cấu trúc bảng khoa học.
**2. 📖 Hiểu (Understanding):**
- **1NF (Dạng chuẩn 1)**: Mỗi ô chỉ chứa 1 giá trị (atomic). Không có cột dạng mảng hoặc chuỗi phân cách bởi dấu phẩy.
- **2NF (Dạng chuẩn 2)**: Đạt 1NF và không có thuộc tính phụ thuộc một phần vào khóa chính phức hợp.
- **3NF (Dạng chuẩn 3)**: Đạt 2NF và loại bỏ sự phụ thuộc bắc cầu (ví dụ: Đã có Ngày sinh thì không lưu thêm cột Tuổi, đã lưu ID Huyện thì suy ra được Tỉnh, không cần lưu cả hai).
- **Primary Key (PK)**: Khóa chính định danh duy nhất dòng. **Foreign Key (FK)**: Khóa ngoại trỏ đến PK bảng khác tạo liên kết.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Tại sao lưu mảng số điện thoại trong 1 cột varchar là vi phạm 1NF?
- Phân biệt quan hệ 1-N (Một - Nhiều) và N-N (Nhiều - Nhiều)?
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Thiết kế ERD (Entity Relationship Diagram) cho ứng dụng Quản lý sinh viên và Lớp học (Quan hệ 1-N).
- **Bài 2:** Thiết kế hệ thống Mua hàng online: Khách hàng, Sản phẩm, Đơn hàng, Chi tiết đơn hàng. (Chi tiết đơn hàng đóng vai trò bảng trung gian giải quyết quan hệ N-N giữa Đơn hàng và Sản phẩm).
- **Bài 3:** Chuẩn hóa: Bảng `Employees` có cột `DepartmentName` và `ManagerName`. Hãy chuẩn hóa nó về 3NF bằng cách tách thành bảng `Departments`.
**5. ❌ Sai (Common Mistakes):**
- Thiết kế bảng "Phẳng" hệt như file Excel (Lưu Tên danh mục thẳng vào bảng Giao dịch thay vì lưu CategoryId). Khi đổi tên danh mục phải update hàng nghìn dòng.
**6. ✅ Sửa (Fix/Debug):**
- Luôn phân rã bảng thành các thực thể độc lập và sử dụng Khóa ngoại.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Dùng công cụ draw.io thiết kế toàn bộ database cho một trang mạng xã hội mini (Users, Posts, Comments, Likes).

## 2. SQL Cốt lõi & Lỗi thường gặp
**1. 🎯 Tập trung (Focused Learning):**
Ngôn ngữ truy vấn có cấu trúc (SQL) dùng để tương tác trực tiếp với Database. Lệnh CRUD là kiến thức sống còn.
**2. 📖 Hiểu (Understanding):**
- `SELECT` để đọc, `INSERT` tạo mới, `UPDATE` sửa, `DELETE` xóa.
- Thứ tự thực thi ngầm của SQL Engine: `FROM -> WHERE -> GROUP BY -> HAVING -> SELECT -> ORDER BY -> LIMIT`.
- `NULL` là trạng thái "không biết", không phải chuỗi rỗng và không phải số 0.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Để kiểm tra giá trị NULL trong câu WHERE, dùng dấu `=` hay `IS NULL`?
- Hậu quả của lệnh `UPDATE` hoặc `DELETE` quên kèm mệnh đề `WHERE`?
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Viết câu lệnh tạo bảng (DDL - CREATE TABLE) cho bảng `Products`.
- **Bài 2:** Thêm 5 dòng dữ liệu sản phẩm. Cập nhật giá sản phẩm có ID = 3 lên 10%.
- **Bài 3:** Lấy danh sách sản phẩm có giá lớn hơn 50,000, sắp xếp giảm dần theo giá, phân trang chỉ lấy 10 kết quả đầu tiên (sử dụng OFFSET / FETCH NEXT trong SQL Server).
**5. ❌ Sai (Common Mistakes):**
- Chạy `UPDATE`/`DELETE` mà quên bôi đen mệnh đề `WHERE` (Làm hỏng toàn bộ database thực tế).
- Lạm dụng `SELECT *` trong ứng dụng khiến lãng phí băng thông mạng.
**6. ✅ Sửa (Fix/Debug):**
- Mẹo an toàn: Luôn viết câu lệnh `SELECT` để test trước mệnh đề `WHERE`. Khi đúng kết quả, chỉ cần đổi chữ `SELECT *` thành `DELETE`.
- Luôn chỉ định rõ `SELECT Id, Name, Price`.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Chơi trò chơi SQL Murder Mystery (https://mystery.knightlab.com/) để ôn luyện truy vấn trinh thám.

## 3. JOIN, GROUP BY và Truy vấn Nâng cao
**1. 🎯 Tập trung (Focused Learning):**
Dữ liệu chuẩn hóa nằm rải rác. Ta dùng `JOIN` để gom chúng lại, và `GROUP BY` để thống kê dữ liệu.
**2. 📖 Hiểu (Understanding):**
- **INNER JOIN**: Lấy kết quả khớp nhau ở cả 2 bảng.
- **LEFT JOIN**: Lấy toàn bộ bảng bên trái, dòng nào bảng phải không có thì để trống (NULL).
- **GROUP BY**: Gom các dòng có cùng giá trị lại. Bắt buộc đi kèm hàm tổng hợp (Aggregate Functions: SUM, COUNT, MAX, MIN, AVG).
- **HAVING**: Lọc dữ liệu SAU khi đã gom nhóm. (Khác với WHERE lọc trước khi gom nhóm).
- **Subquery**: Câu truy vấn lồng bên trong câu truy vấn khác.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Khi lấy Danh sách Tất cả sinh viên và Điểm của họ (kể cả người chưa có điểm), nên dùng INNER JOIN hay LEFT JOIN?
- Lỗi "Column is invalid in the select list" xảy ra vì sao?
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Viết câu truy vấn INNER JOIN lấy `Tên đơn hàng` và `Tên khách hàng`.
- **Bài 2:** Thống kê: `GROUP BY` theo `Khu vực`, dùng `SUM` để tính tổng doanh số bán hàng của mỗi khu vực.
- **Bài 3:** Dùng `HAVING` để chỉ lấy các Khu vực có tổng doanh số lớn hơn 100 triệu.
- **Bài 4:** Dùng Subquery để tìm sản phẩm có giá đắt nhất trong bảng.
**5. ❌ Sai (Common Mistakes):**
- Cố tình đặt hàm aggregate (SUM) trong mệnh đề `WHERE` (ví dụ: `WHERE SUM(Price) > 100`). Điều này là sai cú pháp.
**6. ✅ Sửa (Fix/Debug):**
- Chuyển logic đó xuống mệnh đề `HAVING`.
- Tất cả các cột nằm trong `SELECT` (mà không nằm trong hàm tính tổng) thì ĐỀU PHẢI có mặt trong mệnh đề `GROUP BY`.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Lên HackerRank hoặc LeetCode (phần Database) giải 5 bài SQL mức độ Medium để rèn tư duy JOIN đa bảng.

## 4. Stored Procedure & Transaction
**1. 🎯 Tập trung (Focused Learning):**
Bảo vệ tính vẹn toàn dữ liệu trong các luồng phức tạp (ví dụ: chuyển tiền) và tăng bảo mật/hiệu năng.
**2. 📖 Hiểu (Understanding):**
- **Stored Procedure (SP)**: Tương tự như hàm (Function) trong lập trình. Chứa một khối lệnh SQL, được lưu sẵn trên Database, có thể nhận tham số (Parameters).
- **Transaction (ACID)**: Chế độ an toàn. Một chuỗi các lệnh SQL, hoặc là tất cả đều thành công (Commit), hoặc nếu có 1 lệnh lỗi thì toàn bộ bị hủy và quay về trạng thái cũ (Rollback). Đảm bảo tính nguyên tử (Atomicity).
**3. 🧠 Tự nhớ lại (Active Recall):**
- Tại sao chống lại SQL Injection lại là lợi thế của Stored Procedure?
- Tại sao chức năng "Chuyển tiền tài khoản A sang B" bắt buộc phải bọc trong một Transaction?
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Viết một SP `sp_CreateOrder` nhận tham số truyền vào là `CustomerId`, `ProductId`, `Quantity`. SP tự động tính tổng tiền và `INSERT` vào bảng.
- **Bài 2 (Transaction trong C#):** Viết ứng dụng console C# sử dụng `SqlConnection` và `SqlTransaction`. Thực hiện `UPDATE` trừ tiền người gửi, rồi cố ý quăng ra `Exception`. Bắt lỗi tại khối catch và gọi hàm `transaction.Rollback()`. Kiểm tra lại database xem tiền có bị mất vô lý không.
**5. ❌ Sai (Common Mistakes):**
- Mở Transaction, viết logic dài dòng xử lý bên trong nhưng quên lệnh `Commit` hoặc `Rollback`. Database sẽ bị khóa (Deadlock/Locked out).
**6. ✅ Sửa (Fix/Debug):**
- Luôn áp dụng mẫu try-catch-finally hoặc sử dụng khối `using` an toàn cho các tác vụ Transaction trong .NET.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Tự tay cài đặt môi trường SQL Server cục bộ, tạo SP lấy báo cáo tháng và gọi nó từ một chương trình C#.
"""

md5 = """# Chặng 5: HTTP và ASP.NET Core Web API (4-5 tuần)
**Mục tiêu**: Xây dựng Backend chuyên nghiệp với kiến trúc ASP.NET Core, làm chủ Entity Framework Core (EF Core), nắm vững chuẩn RESTful, xử lý lỗi tập trung, bảo mật JWT và cơ chế Dependency Injection.

## 1. Giao thức HTTP và Kiến trúc RESTful
**1. 🎯 Tập trung (Focused Learning):**
HTTP là xương sống của Web. Hiểu rõ HTTP và RESTful giúp thiết kế ra một API mà Frontend hoặc đối tác có thể dễ dàng sử dụng.
**2. 📖 Hiểu (Understanding):**
- **Methods**: `GET` (Đọc dữ liệu), `POST` (Tạo mới), `PUT` (Cập nhật toàn bộ object), `PATCH` (Cập nhật 1 vài field), `DELETE` (Xóa).
- **Status Codes**:
  - 2xx Thành công: 200 (OK), 201 (Created).
  - 4xx Lỗi từ Client (người gửi sai): 400 (Bad Request - sai data), 401 (Unauthorized - thiếu token), 403 (Forbidden - có token nhưng cấm quyền), 404 (Not Found).
  - 5xx Lỗi Server (code sập): 500 (Internal Server Error).
- **Headers & Body**: Metadata của request nằm ở Header (ví dụ Content-Type). Dữ liệu dạng JSON thường nằm ở Body.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Sự khác biệt lớn nhất giữa PUT và PATCH là gì?
- Tại sao GET không nên truyền tham số quan trọng chứa mật khẩu lên URL?
- Endpoint nào đúng chuẩn REST cho việc lấy bình luận của một bài đăng: `/api/get-comments?postId=5` hay `/api/posts/5/comments`?
**4. 💻 Thực hành (Practice):**
- Cài đặt và sử dụng Postman hoặc Insomnia.
- Thiết kế URL RESTful trên giấy cho một hệ thống Quản lý Bệnh viện (Bác sĩ, Bệnh nhân, Cuộc hẹn).
- Tạo project Web API trống, chạy thử các Controller mặc định.
**5. ❌ Sai (Common Mistakes):**
- Luôn luôn trả về Status HTTP 200 kèm theo JSON nội dung `{ "error": "Username đã tồn tại" }`. Điều này phá vỡ chuẩn HTTP.
- Sử dụng Verb trong URL (ví dụ: `/api/users/updateUser/1`).
**6. ✅ Sửa (Fix/Debug):**
- Lỗi dữ liệu thì phải trả về 400 Bad Request. Không đưa động từ vào URL, hãy dùng HTTP Methods để định nghĩa hành động.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Mỗi lần bắt gặp một website báo lỗi, hãy bật tab Network ở F12 để soi xem họ trả về mã Status code gì.

## 2. ASP.NET Core: Middleware Pipeline và Dependency Injection
**1. 🎯 Tập trung (Focused Learning):**
Request HTTP đi vào ASP.NET Core giống như nước chảy qua một đường ống (Pipeline). Các Middleware đóng vai trò là màng lọc. Dependency Injection (DI) là công cụ quản lý sự sống của các class service.
**2. 📖 Hiểu (Understanding):**
- **Middleware**: Nằm trong `Program.cs`. Chặn và xử lý request trước khi nó tới Controller (ví dụ: Logging, CORS, Authentication).
- **Dependency Injection (DI)**: Cung cấp class cần thiết thông qua Constructor thay vì dùng từ khóa `new`.
- **DI Lifetimes**:
  - `AddTransient`: Tạo một thể hiện mới toanh mỗi khi được yêu cầu.
  - `AddScoped`: Tạo mới một lần cho TỪNG Request HTTP. Các class trong cùng 1 request sẽ dùng chung.
  - `AddSingleton`: Chỉ tạo đúng một lần duy nhất kể từ khi bật Server. Toàn hệ thống dùng chung.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Tại sao kết nối Database (DbContext) luôn phải đăng ký dạng `Scoped` mà không phải `Singleton`?
- Thứ tự đăng ký Middleware có quan trọng không? (Ví dụ UseAuthorization và UseRouting).
**4. 💻 Thực hành (Practice):**
- Viết 1 Controller có hàm GET đơn giản.
- Tự xây dựng Custom Middleware: Bọc toàn bộ quá trình xử lý bằng `Stopwatch`, đo lường thời gian xử lý của từng Request và ghi ra Console.
- Tạo một `IEmailService` và cấu hình DI để tự động bơm (inject) vào Controller.
**5. ❌ Sai (Common Mistakes):**
- Đăng ký một Service chứa List data động làm `Singleton`. Hậu quả: Dữ liệu của người dùng A bị lộ sang cho người dùng B do dùng chung vùng nhớ.
**6. ✅ Sửa (Fix/Debug):**
- Scoped là lựa chọn an toàn và mặc định cho 90% các Service liên quan tới dữ liệu và Business logic.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Tự vẽ ra giấy sơ đồ Middleware Pipeline theo đúng thứ tự khai báo trong `Program.cs`.

## 3. Entity Framework Core (EF Core) Chi Tiết
**1. 🎯 Tập trung (Focused Learning):**
EF Core là ORM phổ biến nhất trong .NET. Nó chuyển đổi mã C# (LINQ) thành truy vấn SQL tương ứng.
**2. 📖 Hiểu (Understanding):**
- **DbContext & DbSet**: Cầu nối giao tiếp và đại diện cho bảng.
- **Code-First & Migrations**: Khai báo class C# trước, tạo lệnh `add migration` để sinh ra script, sau đó `update database` để đồng bộ bảng vào SQL Server.
- **Fluent API vs Data Annotations**: Data Annotations dùng attribute như `[Required]`. Fluent API mạnh hơn, viết trong hàm `OnModelCreating`.
- **Tracking**: EF Core mặc định theo dõi (track) sự thay đổi của Object lấy từ Database lên. Nếu gọi `SaveChanges()`, các giá trị thay đổi sẽ được lưu lại tự động.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Khi nào dùng `.AsNoTracking()`?
- Projection (`.Select()`) giúp tối ưu hiệu năng như thế nào?
**4. 💻 Thực hành (Practice):**
- Tạo Model `Post` và `Comment` (quan hệ 1-N).
- Viết lệnh sinh Migration và cập nhật cơ sở dữ liệu.
- Viết API thêm Post.
- Viết API lấy danh sách Post. Dùng tính năng Eager Loading (`.Include(x => x.Comments)`) để lấy cả dữ liệu bình luận.
**5. ❌ Sai (Common Mistakes - Lỗi N+1 Khét tiếng):**
- Query lấy 100 Post ra List. Trong một vòng for duyệt List, bạn gọi `item.Comments.Count()`. Hậu quả: EF Core sẽ tự động bắn 100 câu lệnh SELECT rời rạc lên Server thay vì lấy 1 lần.
**6. ✅ Sửa (Fix/Debug):**
- Xem Log Console của EF Core để phát hiện truy vấn dư thừa.
- Sử dụng `.Include()` hoặc Projection (`.Select()`) để lấy hết thông tin cần thiết trong đúng 1 lệnh truy vấn SQL. Cải thiện tốc độ lên hàng trăm lần.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Dùng thư viện Bogus tạo dữ liệu giả 50,000 bảng ghi. Viết API lấy dữ liệu bị N+1 so sánh thời gian với API đã tối ưu.

## 4. Validation, Error Handling & Async/Await
**1. 🎯 Tập trung (Focused Learning):**
Bảo vệ đầu vào với Validation, quản lý luồng bất đồng bộ tăng cường hiệu suất Server và xử lý lỗi chuyên nghiệp.
**2. 📖 Hiểu (Understanding):**
- **Async/Await**: Giải phóng Thread của server khi nó phải chờ thao tác IO (đọc Database, gọi API ngoài), giúp server chịu tải tốt hơn.
- **FluentValidation**: Viết các quy tắc kiểm tra tính đúng đắn của dữ liệu đầu vào.
- **Global Exception Handler**: Thay vì viết `try-catch` trong mọi hàm ở mọi Controller, ta đặt một màng lưới Middleware ở ngoài cùng để gom mọi lỗi về 1 nơi xử lý chung, trả ra định dạng `ProblemDetails` chuẩn hóa.
**3. 🧠 Tự nhớ lại (Active Recall):**
- `.Wait()` và `.Result` gây hậu quả gì trong ASP.NET Core? (Deadlock).
- Tại sao phải có DTO (Data Transfer Object) mà không nên trả/nhận thẳng object Model của Database?
**4. 💻 Thực hành (Practice):**
- Chuyển đổi mọi hàm Database sang Async (`ToListAsync()`, `FirstOrDefaultAsync()`).
- Thiết lập thư viện FluentValidation: Bắt buộc `Title` không được trống, `Price` phải lớn hơn 0.
- Tạo một Exception Middleware tùy chỉnh bắt các exception lạ và trả về HTTP Status 500 với body JSON gọn gàng.
**5. ❌ Sai (Common Mistakes):**
- Dùng `async void` thay vì `async Task`. (Ngoại trừ sự kiện event handler của WinForm/WPF, tuyệt đối cấm dùng trong Web API vì lỗi trong đó sẽ đánh sập cả quá trình).
**6. ✅ Sửa (Fix/Debug):**
- Toàn bộ phương thức API phải có chữ ký `public async Task<IActionResult>`.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Tìm hiểu cấu trúc chuẩn của RFC 7807 (ProblemDetails) và cấu hình `UseExceptionHandler`.

## 5. Xác thực và Phân quyền (Authentication JWT)
**1. 🎯 Tập trung (Focused Learning):**
Bảo vệ API, nhận diện người dùng đang đăng nhập thông qua JSON Web Token (JWT).
**2. 📖 Hiểu (Understanding):**
- Người dùng truyền Username/Password -> Server kiểm tra Database -> Khớp thì tạo một chuỗi JWT mã hóa bí mật kèm thông tin (Claims) -> Trả về Client.
- Client truyền JWT vào Header `Authorization: Bearer <token>` mỗi khi gọi API cần bảo vệ.
- Middleware của ASP.NET sẽ giải mã, xác nhận tính hợp lệ.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Secret Key của JWT nên được lưu ở đâu an toàn?
- Token hết hạn (Expired) thì làm sao để xử lý cho người dùng? (Refresh Token).
**4. 💻 Thực hành (Practice):**
- Cài đặt gói thư viện JWT Bearer.
- Viết API `/login`, xác thực tĩnh (tạm thời chưa kết nối DB) và cấp chuỗi Token.
- Gắn Attribute `[Authorize]` lên API lấy danh sách chi tiêu.
- Thử dùng Postman gọi API khi KHÔNG đính kèm token (sẽ nhận 401), và CÓ token để xem kết quả.
**5. ❌ Sai (Common Mistakes):**
- Đặt thời hạn JWT quá dài (10 năm). Nếu hacker lấy được Token, họ dùng được mãi mãi.
- Lưu trữ dữ liệu nhạy cảm như Mật khẩu, Số thẻ tín dụng TRONG payload của JWT (JWT chỉ được base64 encode, không bị mã hóa chống đọc).
**6. ✅ Sửa (Fix/Debug):**
- JWT chỉ chứa thông tin nhận diện ID và Role. Thời hạn nên ngắn (30 phút - 1 tiếng).
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Cấu hình ASP.NET Core Identity kết hợp cùng JWT để có giải pháp bảo mật hoàn chỉnh.
"""

md6 = """# Chặng 6: Nền tảng Frontend (3-4 tuần)
**Mục tiêu**: Xây dựng vững chắc kỹ năng HTML5/CSS3 (đặc biệt là layout), làm chủ ngôn ngữ Vanilla JavaScript (ES6+), xử lý DOM, tương tác với API backend, và làm quen với TypeScript.

## 1. HTML5 & CSS3: Semantic, Flexbox và Grid
**1. 🎯 Tập trung (Focused Learning):**
Code giao diện không chỉ là làm cho nó "trông giống" bản thiết kế, mà còn là đảm bảo cấu trúc chuẩn, SEO tốt và có thể co giãn tự nhiên trên màn hình di động (Responsive).
**2. 📖 Hiểu (Understanding):**
- **Semantic HTML**: Dùng thẻ mang ý nghĩa như `<header>`, `<footer>`, `<main>`, `<article>` thay vì lạm dụng `<div>`. Giúp Google bot hiểu cấu trúc web và hỗ trợ người khiếm thị (Accessibility).
- **Flexbox (1 chiều)**: Bố cục một hàng hoặc cột. Tuyệt vời để căn chỉnh các item nhỏ. Các thuộc tính quan trọng: `justify-content` (căn trục chính), `align-items` (căn trục vuông góc).
- **Grid (2 chiều)**: Bố cục kẻ bảng, ma trận. Cực tốt để dựng khung sườn tổng thể toàn trang (Layout lớn).
- **Responsive Design**: Sử dụng Media Queries `@media (max-width: 768px)` để thay đổi CSS cho màn hình điện thoại.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Làm thế nào để dùng Flexbox đẩy nội dung ra xa nhau nhất ở 2 đầu (VD: Logo bên trái, Menu bên phải)? (Gợi ý: `space-between`).
- Khi nào dùng Padding, khi nào dùng Margin? (Margin là khoảng cách bên ngoài, Padding là khoảng đệm bên trong viền).
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Dựng trang đích (Landing Page) đơn giản: Thanh điều hướng Navbar (Flexbox), Hero Section, Danh sách sản phẩm dạng lưới (CSS Grid 3 cột).
- **Bài 2 (Responsive):** Dùng thẻ `@media`, khi màn hình thu nhỏ xuống dưới kích thước điện thoại, lưới sản phẩm 3 cột lập tức gộp về 1 cột.
- **Bài 3:** Làm bài tập tương tác trên web: [Flexbox Froggy](https://flexboxfroggy.com/) và [Grid Garden](https://cssgridgarden.com/).
**5. ❌ Sai (Common Mistakes):**
- Dùng `float` hoặc `position: absolute` một cách mù quáng để xếp giao diện. Hậu quả là bố cục vỡ nát khi đổi kích thước trình duyệt.
**6. ✅ Sửa (Fix/Debug):**
- Hiểu tư duy "Flow" (dòng chảy tài liệu). Mặc định 95% bài toán xếp layout ngày nay nên được xử lý bằng Flexbox và Grid.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Clone (mô phỏng) lại giao diện trang chủ YouTube với Sidebar bên trái và Video Grid bên phải.

## 2. JavaScript ES6+ Cốt lõi
**1. 🎯 Tập trung (Focused Learning):**
Cú pháp JS hiện đại (ES6) được sử dụng liên tục và dày đặc trong các framework như React/Vue. Bỏ qua bước này sẽ khiến bạn không đọc hiểu code React.
**2. 📖 Hiểu (Understanding):**
- **Scope & Biến**: Bỏ `var`. Dùng `const` cho giá trị không gán lại, và `let` cho biến cần gán lại.
- **Arrow Function `() => {}`**: Viết hàm ngắn gọn. Rất khác với `function()` ở cơ chế bind `this`.
- **Destructuring**: Bóc tách dữ liệu nhanh gọn. VD: `const { name, age } = userObj;`
- **Spread Operator (`...`)**: Dùng để sao chép mảng, kết hợp object.
- **Closure**: Khái niệm nâng cao, một hàm ghi nhớ môi trường nơi nó được tạo ra (rất quan trọng khi viết hook).
**3. 🧠 Tự nhớ lại (Active Recall):**
- `.map()`, `.filter()`, `.reduce()` của mảng hoạt động khác nhau thế nào? Hàm nào biến đổi mọi phần tử?
- `const` có nghĩa là "Bất biến 100%"? (Sai, với object và array, bạn không thể gán lại bằng dấu `=` nhưng vẫn có thể dùng hàm push() hoặc thay đổi tham số con của object).
**4. 💻 Thực hành (Practice):**
- **Bài 1:** Cho mảng các giao dịch chi tiêu. Dùng `.filter()` tìm các khoản trên 1 triệu. Dùng `.map()` định dạng tiền tệ. Dùng `.reduce()` tính tổng chi phí.
- **Bài 2:** Cho Object User và Object Config. Dùng toán tử Spread để hợp nhất hai object thành một `UserProfile`.
- **Bài 3 (Closure):** Viết hàm `createCounter()` trả về một hàm con giúp tăng biến đếm cục bộ.
**5. ❌ Sai (Common Mistakes):**
- Trực tiếp chỉnh sửa (Mutate) mảng cũ bằng cách gọi hàm splice, push thay vì tạo mảng mới. Sẽ gây ra bug nghiêm trọng về state trong React sau này.
**6. ✅ Sửa (Fix/Debug):**
- Bắt đầu thói quen code Functional Programming: Luôn tạo bản sao bằng toán tử `...` trước khi chỉnh sửa mảng hoặc đối tượng.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Xem lại các bài toán cơ bản ở chặng 2 (Bubble Sort, Two Sum) và viết lại bằng JavaScript thuần.

## 3. DOM Manipulation & Sự kiện (Events)
**1. 🎯 Tập trung (Focused Learning):**
DOM (Document Object Model) là cách JS nhìn nhận cấu trúc file HTML dưới dạng một cái cây. Bạn dùng JS để thay đổi cây này.
**2. 📖 Hiểu (Understanding):**
- Truy xuất phần tử: `document.querySelector('#myBtn')`.
- Thay đổi nội dung: `element.textContent`, `element.classList.add()`.
- Tạo và xóa: `document.createElement()`, `element.remove()`.
- **Event Listener**: Bắt hành động của người dùng (click, submit, input, keydown).
- **Event Delegation**: Lắng nghe sự kiện ở thẻ cha thay vì gán sự kiện cho hàng trăm thẻ con.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Sự khác biệt giữa `innerHTML` và `textContent`? Tại sao chèn nội dung người dùng nhập bằng `innerHTML` rất nguy hiểm (Lỗi XSS)?
- Event Bubbling là hiện tượng gì?
**4. 💻 Thực hành (Practice):**
- **Dự án: Todo List (Thuần JS)**:
  - Tạo ô Input và nút Thêm. Bắt sự kiện bấm nút.
  - Lấy nội dung, tạo thẻ `<li>`, đính vào thẻ `<ul>` hiển thị danh sách.
  - Thêm một nút `[X]` nhỏ ở mỗi task để xóa (remove phần tử DOM).
- Thực hành thao tác Form: Bắt sự kiện Submit, kiểm tra xem dữ liệu có trống không, báo lỗi đỏ viền input nếu thiếu nội dung.
**5. ❌ Sai (Common Mistakes):**
- Bắt sự kiện Form Submit nhưng quên, làm trang web bị Reload toàn bộ màn hình.
**6. ✅ Sửa (Fix/Debug):**
- Luôn gọi `event.preventDefault()` ở dòng đầu tiên của hàm xử lý Submit.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Nâng cấp Todo List: Lưu trạng thái của danh sách việc cần làm vào `localStorage` của trình duyệt. Mỗi lần load lại trang, tái tạo lại DOM từ bộ nhớ này.

## 4. Fetch API & Xử lý Bất đồng bộ
**1. 🎯 Tập trung (Focused Learning):**
Giao tiếp với thế giới bên ngoài (API từ backend .NET) sử dụng tính năng bất đồng bộ để trình duyệt không bị treo đứng.
**2. 📖 Hiểu (Understanding):**
- **Promise**: Lời hứa lấy kết quả trong tương lai. Có 3 trạng thái: Pending, Fulfilled, Rejected.
- **Async/Await**: Cú pháp đường (Syntax sugar) để xử lý Promise dễ đọc như code chạy tuần tự.
- **Fetch**: Hàm chuẩn của trình duyệt để gọi API HTTP. Nhớ phải phân giải JSON 2 lần (`response.json()`).
**3. 🧠 Tự nhớ lại (Active Recall):**
- Lỗi CORS (Cross-Origin Resource Sharing) xuất phát từ đâu? Cần sửa ở Browser hay Server?
- Tại sao phải có `catch` ở cuối chuỗi Promise?
**4. 💻 Thực hành (Practice):**
- Dùng API miễn phí (VD: *jsonplaceholder.typicode.com*), fetch danh sách 10 bài viết.
- **Xử lý Loading**: Trước khi gọi `fetch`, hiển thị chữ "Đang tải dữ liệu...". Lấy được dữ liệu thì ẩn chữ tải đi và hiển thị thẻ Card giao diện.
- **Xử lý Lỗi**: Cố tình viết sai tên URL. Bắt lỗi trong khối `catch` và vẽ dòng chữ màu đỏ báo lỗi mạng ra giao diện.
- **Tích hợp Backend**: Bật Web API .NET của Chặng 5 lên. Dùng fetch gửi yêu cầu lấy danh sách chi tiêu và vẽ biểu đồ / bảng.
**5. ❌ Sai (Common Mistakes):**
- Nghĩ rằng fetch() sẽ tự văng lỗi `catch` nếu Server trả về mã 404 hoặc 500. Sự thật là fetch chỉ văng lỗi khi mạng Internet rớt.
**6. ✅ Sửa (Fix/Debug):**
- Sau khi `await fetch`, phải chủ động kiểm tra `if (!response.ok) { throw new Error(response.status) }`.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Ôn lại AbortController để tìm cách hủy (cancel) một request đang chạy ngầm nếu người dùng chuyển trang quá nhanh.

## 5. TypeScript Nhập Môn (Khuyên dùng)
**1. 🎯 Tập trung (Focused Learning):**
JavaScript là ngôn ngữ kiểu động (Dynamic typing), rất dễ nhầm lẫn. TypeScript (TS) thêm "kiểu tĩnh" (Static typing) để chặn lỗi ngay lúc bạn đang gõ code.
**2. 📖 Hiểu (Understanding):**
- Khai báo kiểu: `let name: string = "Bob"; let age: number = 20;`
- **Interface / Type Alias**: `interface User { name: string; id: number }`. Quy định cấu trúc của đối tượng một cách nghiêm ngặt.
- **Generics**: Tham số kiểu. (Ví dụ: `List<T>` trong C# rất giống Array `<T>` trong TS).
- **Union Types**: Một biến có thể là kiểu này HOẶC kiểu kia (`string | number`).
**3. 🧠 Tự nhớ lại (Active Recall):**
- Lợi ích lớn nhất khi bạn chuyển một dự án từ JS thuần sang TS là gì? (Gợi ý: Autocomplete thông minh và chống lỗi undefined tại runtime).
**4. 💻 Thực hành (Practice):**
- Chuyển dự án Todo List thuần JS sang dùng TypeScript.
- Viết interface rõ ràng cho dữ liệu trả về từ Fetch API, gán kiểu cho `response.json()` để IDE Visual Studio Code tự động gợi ý code.
"""

md7 = """# Chặng 7: React và Hoàn thiện Dự án (4-5 tuần)
**Mục tiêu**: Xây dựng SPA (Single Page Application) hiện đại, tổ chức mã nguồn bằng kiến trúc Component, quản lý State thành thạo, và triển khai toàn bộ Fullstack App (Frontend + Backend) lên Internet.

## 1. Triết lý React, JSX & Component Architecture
**1. 🎯 Tập trung (Focused Learning):**
Giao diện không còn là trang HTML khổng lồ. Nó được chẻ nhỏ thành các Component (mảnh Lego) độc lập, tái sử dụng, giúp quản lý các UI cực kỳ phức tạp.
**2. 📖 Hiểu (Understanding):**
- **JSX**: Sự pha trộn giữa HTML và JavaScript. Bạn có thể chèn biểu thức JS trực tiếp vào trong thẻ HTML bằng cặp dấu ngoặc nhọn `{}`.
- **Props**: Dữ liệu truyền từ Component Cha xuống Component Con. Giống như tham số hàm, Props chỉ được đọc, cấm chỉnh sửa.
- **Tư duy Khai báo (Declarative)**: Bạn không tự tay xóa DOM. Bạn chỉ thay đổi Dữ liệu (State). React sẽ thay bạn tính toán DOM nào cần cập nhật.
- **Container vs Presentational**: Mẫu thiết kế tách biệt phần xử lý logic (Container) và phần chỉ để vẽ giao diện nhận props (Presentational).
**3. 🧠 Tự nhớ lại (Active Recall):**
- Tại sao trong JSX phải viết `className` thay vì `class`?
- Làm sao để truyền dữ liệu từ Component con NGƯỢC LÊN Component cha? (Gợi ý: Truyền hàm callback qua props).
**4. 💻 Thực hành (Practice):**
- Xây dựng `<ProductCard />` nhận props gồm `title`, `price`, `imageUrl`. Gọi Component này 5 lần trong file `App.js` với các dữ liệu khác nhau.
- Làm chức năng Render theo điều kiện (Conditional Rendering): Dùng toán tử ba ngôi `isLoggedIn ? <UserMenu /> : <LoginButton />`.
**5. ❌ Sai (Common Mistakes):**
- Cố tình chọc thẳng vào DOM bằng `document.getElementById('text').innerHTML` để thay đổi chữ.
**6. ✅ Sửa (Fix/Debug):**
- Lên ý tưởng giao diện dựa trên State. Nếu cần thay đổi văn bản, hãy thay đổi State liên kết với văn bản đó, giao diện tự cập nhật. Chỉ dùng `useRef` cho các tình huống bắt buộc (như focus ô input, Canvas).
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Thử chia nhỏ một bản nháp giao diện Dashboard phức tạp ra giấy: xác định Component nào bọc Component nào, cái nào có thể dùng chung (Buttons, Cards).

## 2. State & useState Hook (Tổ chức dữ liệu)
**1. 🎯 Tập trung (Focused Learning):**
State là bộ nhớ cá nhân, là trái tim của Component. Thay đổi State kích hoạt quá trình Re-render của React.
**2. 📖 Hiểu (Understanding):**
- `const [count, setCount] = useState(0);`
- State hoạt động như một bức ảnh chụp nhanh (Snapshot) trong lần hiển thị hiện tại. Đừng hy vọng `count` thay đổi ngay lập tức sau lệnh `setCount`.
- Trạng thái Nâng lên (Lifting State Up): Nếu 2 Component cần chia sẻ dữ liệu cho nhau, hãy đưa State đó lên Component cha chung gần nhất của chúng.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Điều gì xảy ra khi gọi `setCount(count + 1)` 3 lần liên tiếp trong cùng 1 sự kiện Click? Kết quả tăng 1 hay tăng 3?
- Form (Controlled vs Uncontrolled Component) trong React hoạt động thế nào?
**4. 💻 Thực hành (Practice):**
- Làm một Form Đăng ký nhận 3 ô: Tên, Email, Password. Gắn 3 ô vào State (Controlled Component). In log giá trị mỗi khi thay đổi (onChange).
- Làm lại Mini-project Todo List trên môi trường React (Dùng Mảng Object làm State).
**5. ❌ Sai (Common Mistakes):**
- Sửa trực tiếp (Mutate) Object/Array trong State: `stateArray.push(newObj); setArray(stateArray);`. React sẽ không nhận diện được sự thay đổi (do tham chiếu ô nhớ không đổi) và từ chối Render.
**6. ✅ Sửa (Fix/Debug):**
- Tạo bản sao hoàn toàn mới trước khi SetState. `setArray([...stateArray, newObj]);` hoặc dùng hàm thay thế `setArray(prev => [...prev, newObj]);`.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Tự làm một game đơn giản như Tic-Tac-Toe theo tài liệu chính thức của React để thành thục kỹ năng chia sẻ mảng State phức tạp.

## 3. Quản lý Vòng đời & Side Effects (useEffect)
**1. 🎯 Tập trung (Focused Learning):**
Hook `useEffect` xử lý các công việc "bên ngoài" luồng render chính của React: Gọi API, Tương tác với DOM gốc, Hẹn giờ Timeout.
**2. 📖 Hiểu (Understanding):**
- Dependency Array (Mảng phụ thuộc) quyết định thời điểm chạy của Effect:
  - `[]`: Chạy đúng 1 lần khi Component xuất hiện (Mount). Thích hợp gọi API khởi tạo.
  - `[variableA]`: Chạy lần đầu và chạy lại mỗi khi `variableA` thay đổi.
  - Không có mảng: Chạy mỗi khi có bất kỳ render nào (Gây lag cực độ).
- **Cleanup function**: Khối lệnh `return () => {}` bên trong Effect sẽ chạy khi Component bị tháo gỡ (Unmount), chuyên dùng để hủy hẹn giờ, bỏ đăng ký sự kiện.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Tại sao người ta gọi là "Lỗi Vòng lặp Vô Hạn" (Infinite Loop) trong useEffect?
**4. 💻 Thực hành (Practice):**
- Component `<UserProfile id={5} />`. Sử dụng `useEffect` kết hợp mảng phụ thuộc `[id]` để gọi Fetch API nạp dữ liệu user mỗi khi `id` đổi.
- **Tạo Custom Hook:** Bóc tách logic gọi API phức tạp vào chung một hook riêng tên là `useFetch(url)`. Hook này xử lý luôn cả trạng thái Loading, State Error.
**5. ❌ Sai (Common Mistakes):**
- Gọi `setState()` bên trong `useEffect` thiếu mảng phụ thuộc. Component render -> Effect chạy -> Update State -> Component render -> Effect chạy -> Crash tab trình duyệt.
**6. ✅ Sửa (Fix/Debug):**
- Luôn kiểm soát chặt chẽ mảng phụ thuộc. Dùng thư viện ESLint rules của React để IDE báo đỏ khi bạn điền sai dependency.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Viết một Hook `useLocalStorage(key, initialValue)` để tự động đồng bộ State và LocalStorage trên trình duyệt mỗi khi đổi giá trị.

## 4. State Management, Router & Các tính năng Nâng cao
**1. 🎯 Tập trung (Focused Learning):**
Ứng dụng lớn cần nhiều trang, và dữ liệu phải được chia sẻ xuyên suốt (Global State) mà không cần truyền Props lằng nhằng qua từng tầng (Props Drilling).
**2. 📖 hiểu (Understanding):**
- **React Router Dom**: Quản lý đa trang. Đổi URL đổi Component mà không giật reload màn hình.
- **Context API / Redux Toolkit / Zustand**: Các công cụ quản lý State tổng quát. Lưu trữ User đăng nhập, Giỏ hàng, Chủ đề Sáng/Tối.
- **Error Boundaries**: Bọc ứng dụng, nếu một mảnh giao diện lỗi, nó không kéo sập toàn bộ app.
**3. 🧠 Tự nhớ lại (Active Recall):**
- Khi nào dùng Context API, khi nào nên cài Redux?
- Hiện tượng Props Drilling gây hại thế nào đối với code lớn?
**4. 💻 Thực hành (Practice):**
- Thêm `react-router-dom`: Tạo trang `/home`, `/about`, và trang báo lỗi `/404`. Chuyển trang bằng Component `<Link>`.
- Dùng Context API tạo `AuthContext` lưu Token JWT (từ Backend API trả về). Dựa vào Token đó làm tính năng Private Route: Nếu chưa đăng nhập mà bấm vào `/dashboard`, tự động chuyển hướng ra trang `/login`.
**5. ❌ Sai (Common Mistakes):**
- Đưa mọi thứ từ cái nhỏ nhất vào Redux/Context (ví dụ như trạng thái đang mở/đóng popup). Khi state Global đổi, toàn bộ cây Component render lại gây chậm.
**6. ✅ Sửa (Fix/Debug):**
- Tuân thủ quy tắc: State cục bộ của UI nào thì chỉ để ở Component UI đó. Chỉ đưa lên Global các dữ liệu thực sự dùng chung.
**7. 🔄 Lặp lại cách quãng (Spaced Repetition):**
- Tìm hiểu `React Query` (TanStack Query) - thư viện tối ưu hóa quá trình gọi API, cache dữ liệu và loại bỏ việc phải tự viết `useEffect` thủ công.

## 5. Triển khai (Deployment) và Hoàn thành Hệ thống
**1. 🎯 Tập trung (Focused Learning):**
Đưa dự án chạy cục bộ `localhost` thành một sản phẩm thực tế để người khác truy cập qua Internet.
**2. 📖 Hiểu (Understanding):**
- Quá trình Build (Đóng gói): Trình biên dịch nén HTML/CSS/JS/React thành tập tin tĩnh siêu nhẹ.
- **Frontend Hosting**: Triển khai file tĩnh siêu nhanh và miễn phí (Vercel, Netlify, Firebase Hosting).
- **Backend Hosting**: Triển khai ASP.NET Core API (Azure App Service, Render, Railway).
- **Database Hosting**: Thuê SQL Server trên Cloud (Azure SQL, AWS RDS, Supabase).
- Cấu hình **CORS** trên Backend cho phép Frontend (Vercel domain) truy cập.
**3. 💻 Thực hành (Practice - Checklist Dự án cuối khóa):**
- **Sản phẩm:** Một phiên bản đầy đủ của App Quản lý Chi Tiêu (hoặc Quản lý Cửa hàng).
- Backend: Cấu hình biến môi trường (`appsettings.json`) trỏ tới Connection String của Cloud DB. Đưa mã nguồn lên GitHub, kết nối Render/Railway.
- Frontend: Thay đổi tham số `API_BASE_URL` trỏ tới link Backend mới. Build React và deploy lên Vercel.
- Gửi link thành phẩm cho bạn bè trải nghiệm.
"""


# Legacy generator guard:
# The study files were reviewed and edited manually. Running this old script would
# overwrite the reviewed lessons with stale drafts. Keep the source for reference,
# but require an explicit opt-in before any write.
if __name__ == "__main__":
    import sys
    if "--force-legacy" not in sys.argv:
        raise SystemExit(
            "Legacy generator disabled: reviewed markdown would be overwritten. "
            "Use --force-legacy only if you intentionally restore the old drafts."
        )

    for i, content in enumerate([md1, md2, md3, md4, md5, md6, md7]):
        filename = f"{i+1}.md"
        with open(filename, 'w', encoding='utf-8') as f:
            f.write(content.strip())
        print(f"Created {filename}")
