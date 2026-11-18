# 🛡️ BÁO CÁO KIỂM THỬ BẢO MẬT & XÂM NHẬP (PENETRATION TESTING REPORT)
## Dự án: Studxy (SWMS) · Vòng 2 · 18/11/2026
**Vai trò:** Hacker Mũ Trắng (Security Penetration Tester)  
**Quy chuẩn tuân thủ:** [AGY2_RULES.md](file:///D:/03_Study/Studxy/docs/qa-round2/AGY2_RULES.md)  
**Nhiệm vụ:** [tasks/a-hacker.md](file:///D:/03_Study/Studxy/docs/qa-round2/tasks/a-hacker.md)  
**Mục tiêu kiểm thử:** Backend API (`http://127.0.0.1:5080`), Proxy BFF (`http://127.0.0.1:3000/api/v1/*`), Frontend Web App (`http://127.0.0.1:3000`)

---

## 1. KẾT QUẢ CHÍNH (EXECUTIVE SUMMARY)

Đợt kiểm thử xâm nhập chuyên sâu đã rà soát toàn diện 6 phân hệ cốt lõi: **Xác thực (Auth & JWT), Kiểm soát truy cập đối tượng (IDOR/BOLA), Tiêm mã (Injection & File Security), Cấu hình Headers & CORS, Gán quyền hàng loạt (Mass Assignment), và Lộ lọt thông tin (Information Disclosure)**.

### Thống kê kết quả kiểm thử tự động
- **Tổng số ca kiểm thử xâm nhập thực hiện:** **42 ca kiểm thử** tự động hóa trong 6 bộ kịch bản chạy lại được 100%.
- **Số ca An toàn (Pass / Secure):** **32 / 42 (76.2%)**
- **Số lỗ hổng & cảnh báo bảo mật phát hiện:** **10 vấn đề** (1 Critical, 3 High, 4 Medium, 2 Low).

### Bảng tổng hợp đánh giá bảo mật

| Phân hệ kiểm thử | Số ca test | Đánh giá | Trạng thái bảo vệ cốt lõi |
| :--- | :---: | :---: | :--- |
| **1. Auth & JWT** | 8 | ⚠️ Có lỗ hổng nghiêm trọng | Token Version và RTR Grace Window hoạt động xuất sắc, nhưng lộ khóa ký bí mật JWT cho phép giả mạo token tùy ý. Thiếu Rate Limiting. |
| **2. IDOR / BOLA** | 18 | 🛡️ **Tuyệt đối an toàn (100% Pass)** | Tất cả 18 endpoint (Khóa học, Chương mục, Bài học, Tài liệu PDF, Sổ tay ghi chú, Thẻ FSRS, Annotation, Quiz) đều chặn kẻ tấn công với HTTP 403 / 404. |
| **3. Injection & Files** | 7 | 🛡️ **Rất tốt** | Chống SQLi hoàn hảo (EF Core). Chống Path Traversal và Zip Slip triệt để. Chống SSRF bằng bộ lọc IP/DNS chặt chẽ. Đã kiểm tra chặn tệp thực thi đội lốt PDF (PdfPig 422). Nhược điểm: Lưu raw HTML trong DB. |
| **4. Headers & CORS** | 10 | ⚠️ Cần bổ sung | CORS backend đã được siết chặt về domain hợp lệ (không còn wildcard). Tuy nhiên vẫn thiếu toàn bộ HTTP Security Headers (CSP, X-Frame-Options, X-Content-Type-Options) trên cả Frontend và Backend. |
| **5. Mass Assignment** | 3 | 🛡️ **Tuyệt đối an toàn** | Sử dụng C# record DTO phân lập hoàn toàn, gán cứng `role: "student"` khi đăng ký, không thể tự leo quyền `admin` hay sửa `quota`. |
| **6. Lộ thông tin** | 5 | ⚠️ Có rủi ro | Không lộ stack trace, không lộ `.env`/`.git`. Tuy nhiên, lộ mật khẩu tài khoản demo trong bundle JS công khai và commit secret tĩnh trong `appsettings.json`. |

---

## 2. BẰNG CHỨNG THỰC NGHIỆM (REPRODUCIBLE EVIDENCE)

Toàn bộ kiểm thử được thực thi trực tiếp trên hệ thống đang chạy với 2 tài khoản thử nghiệm độc lập do kiểm thử viên tự đăng ký:
- **Tài khoản Nạn nhân (Victim):** `qa+victim-01@test.local` (ID: `85135c53-f932-4f3c-a881-d7c394b6801e`)
- **Tài khoản Kẻ tấn công (Attacker):** `qa+attacker-01@test.local` (ID: `99f6a991-886a-4e3e-ae1d-c006835c7458`)

### Mã nguồn các kịch bản kiểm thử xâm nhập (PoC Scripts)
Tất cả kịch bản được lưu trữ trong thư mục [docs/qa-round2/scripts/a-hacker/](file:///D:/03_Study/Studxy/docs/qa-round2/scripts/a-hacker/):
1. [01-auth-tests.mjs](file:///D:/03_Study/Studxy/docs/qa-round2/scripts/a-hacker/01-auth-tests.mjs): Kiểm thử Brute-force, Rate Limiting, User Enumeration, Độ mạnh mật khẩu, Giả mạo JWT, Bypass `alg: none`, Thu hồi Token qua Logout, và Replay Refresh Token (RTR).
2. [02-idor-tests.mjs](file:///D:/03_Study/Studxy/docs/qa-round2/scripts/a-hacker/02-idor-tests.mjs): Kiểm thử IDOR/BOLA trên 18 endpoint CRUD của 8 loại tài nguyên.
3. [03-injection-tests.mjs](file:///D:/03_Study/Studxy/docs/qa-round2/scripts/a-hacker/03-injection-tests.mjs): Kiểm thử SQL Injection, Path Traversal, Stored XSS, SSRF via AI Proxy, ZIP Slip, và Upload Binary độc hại.
4. [04-headers-cors-tests.mjs](file:///D:/03_Study/Studxy/docs/qa-round2/scripts/a-hacker/04-headers-cors-tests.mjs): Kiểm thử HTTP Security Headers và cấu hình CORS với Origin lạ/null.
5. [05-mass-assignment-tests.mjs](file:///D:/03_Study/Studxy/docs/qa-round2/scripts/a-hacker/05-mass-assignment-tests.mjs): Kiểm thử gửi thêm tham số `role`, `isAdmin`, `quota`, `instructorId`.
6. [06-info-disclosure-tests.mjs](file:///D:/03_Study/Studxy/docs/qa-round2/scripts/a-hacker/06-info-disclosure-tests.mjs): Kiểm thử Swagger, C# Stack Traces, `.env`/`.git` files, và quét mật khẩu cứng trong Client JS Bundle.
7. [run-all-tests.mjs](file:///D:/03_Study/Studxy/docs/qa-round2/scripts/a-hacker/run-all-tests.mjs): Trình điều phối chạy tuần tự toàn bộ 6 bộ test suite.

### Lệnh thực thi và kết quả nghiệm thu

```powershell
# Chạy toàn bộ bộ test suite bảo mật
node docs/qa-round2/scripts/a-hacker/run-all-tests.mjs
```
- **Exit code:** `0`
- **Thời gian chạy:** ~4.8 giây
- **Chi tiết đầu ra nhật ký:** Toàn bộ 6/6 kịch bản đều hoàn tất thành công và in bảng kết quả chi tiết.

---

## 3. DANH SÁCH CHI TIẾT CÁC VẤN ĐỀ BẢO MẬT (VULNERABILITIES & FINDINGS)

### 🔴 SEC-CRIT-01: Giả mạo JWT Token tùy ý do Hardcoded Secret Key
- **Mức độ:** **Critical** (CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:H/I:H/A:H — **9.8**)
- **Bước tái hiện:**
  1. Đọc tệp cấu hình [appsettings.json](file:///D:/03_Study/Studxy/backend/src/Swms.Api/appsettings.json#L18) hoặc xem code nguồn fallback tại [SecurityServices.cs:158](file:///D:/03_Study/Studxy/backend/src/Swms.Api/Infrastructure/Security/SecurityServices.cs#L158), thu được khóa ký: `"SWMS-v1-Enterprise-Jwt-HmacSha256-Secret-Key-2026-Studxy-Workspace!"`.
  2. Kẻ tấn công tạo payload JWT tùy ý với `sub: "85135c53-f932-4f3c-a881-d7c394b6801e"` (ID nạn nhân) và `role: "admin"`.
  3. Ký HMAC-SHA256 với khóa trên rồi gửi `Authorization: Bearer <forged_token>` đến `GET /api/v1/auth/me`.
  4. Chạy PoC: `node docs/qa-round2/scripts/a-hacker/01-auth-tests.mjs` (Ca `AUTH-05`).
- **Mong đợi:** Máy chủ từ chối token không do máy chủ cấp phát thật hoặc chỉ chấp nhận khóa bảo mật từ biến môi trường bí mật.
- **Thực tế:** Máy chủ trả về `HTTP 200 OK`, danh tính bị mạo danh hoàn toàn sang tài khoản nạn nhân mà không cần mật khẩu.
- **File/Dòng nghi ngờ:**
  - [backend/src/Swms.Api/appsettings.json:18](file:///D:/03_Study/Studxy/backend/src/Swms.Api/appsettings.json#L18)
  - [backend/src/Swms.Api/Infrastructure/Security/SecurityServices.cs:158](file:///D:/03_Study/Studxy/backend/src/Swms.Api/Infrastructure/Security/SecurityServices.cs#L158)
- **Đề xuất fix:** Bỏ chuỗi fallback mặc định trong mã nguồn. Khi khởi động ứng dụng trong môi trường Production, nếu không tìm thấy biến môi trường `JWT_SIGNING_KEY` hoặc độ dài < 32 ký tự, bắt buộc ném ngoại lệ dừng ứng dụng (`throw new InvalidOperationException`).

---

### 🟠 SEC-HIGH-01: Lộ mật khẩu tài khoản Demo trong tệp JavaScript tĩnh của Client
- **Mức độ:** **High** (CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:H/I:L/A:N — **8.2**)
- **Bước tái hiện:**
  1. Người dùng/kẻ tấn công truy cập trang đăng nhập `http://127.0.0.1:3000/auth`.
  2. Tải tệp chunk JavaScript tĩnh: `http://127.0.0.1:3000/_next/static/chunks/app/auth/page-1ca281af4bebc996.js`.
  3. Tìm kiếm chuỗi `"Studxy@2026"` hoặc `"MinhAnhPassword123!"`.
  4. Chạy PoC: `node docs/qa-round2/scripts/a-hacker/06-info-disclosure-tests.mjs` (Ca `INFO-04`).
- **Mong đợi:** Mật khẩu người dùng không bao giờ được hardcode hoặc gửi xuống trình duyệt client trong các file bundle tĩnh.
- **Thực tế:** Cả 2 tài khoản mẫu (Sơn Trần và Minh Anh) đều bị lộ nguyên văn mật khẩu trong bundle JavaScript client.
- **File/Dòng nghi ngờ:**
  - [frontend/src/app/auth/page.tsx:370-371](file:///D:/03_Study/Studxy/frontend/src/app/auth/page.tsx#L370-L371) (`Studxy@2026`)
  - [frontend/src/app/auth/page.tsx:396-397](file:///D:/03_Study/Studxy/frontend/src/app/auth/page.tsx#L396-L397) (`MinhAnhPassword123!`)
- **Đề xuất fix:** Thay thế luồng đăng nhập demo phía client bằng một endpoint riêng phía backend (ví dụ `POST /api/v1/auth/demo-session?profile=son-tran`), cấp phát token trực tiếp từ server mà không cần nhúng mật khẩu tĩnh vào client code.

---

### 🟠 SEC-HIGH-02: Thiếu Rate Limiting chống dò mật khẩu (Brute-Force) trên `/api/v1/auth/login`
- **Mức độ:** **High** (CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:H/I:N/A:N — **7.5**)
- **Bước tái hiện:**
  1. Gửi liên tiếp 25 yêu cầu đăng nhập sai mật khẩu tới `POST /api/v1/auth/login` trong vòng 1 giây.
  2. Chạy PoC: `node docs/qa-round2/scripts/a-hacker/01-auth-tests.mjs` (Ca `AUTH-01`).
- **Mong đợi:** Sau 5 lần thử sai, máy chủ phải trả về `HTTP 429 Too Many Requests` hoặc áp dụng cơ chế khóa tài khoản tạm thời.
- **Thực tế:** Toàn bộ 25 yêu cầu đều được xử lý và trả về `HTTP 401 Unauthorized`. Kẻ tấn công có thể thử hàng triệu mật khẩu bằng công cụ tự động.
- **File/Dòng nghi ngờ:**
  - [backend/src/Swms.Api/Features/SwmsEndpoints.cs:437-476](file:///D:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L437-L476)
- **Đề xuất fix:** Cấu hình middleware `Microsoft.AspNetCore.RateLimiting` của .NET với thuật toán Sliding Window / Token Bucket cho nhóm endpoint `/api/v1/auth/login` (tối đa 5 lần thử/phút cho mỗi IP hoặc mỗi địa chỉ email).

---

### 🟠 SEC-HIGH-03: Thiếu toàn bộ HTTP Security Headers & Nguy cơ Clickjacking (B3)
- **Mức độ:** **High** (CVSS:3.1/AV:N/AC:L/PR:N/UI:R/S:C/C:L/I:L/A:N — **7.4**)
- **Bước tái hiện:**
  1. Gửi yêu cầu HTTP GET tới Frontend (`http://127.0.0.1:3000/auth`) và Backend (`http://127.0.0.1:5080/health`).
  2. Kiểm tra danh sách header phản hồi.
  3. Chạy PoC: `node docs/qa-round2/scripts/a-hacker/04-headers-cors-tests.mjs` (Ca `HDR-01` đến `HDR-07`).
- **Mong đợi:** Phản hồi phải có các header bảo vệ: `X-Frame-Options: SAMEORIGIN`, `Content-Security-Policy`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, và loại bỏ các header lộ thông tin công nghệ (`Server`, `X-Powered-By`).
- **Thực tế:** Cả 2 server đều thiếu toàn bộ các header an ninh trên. Ứng dụng hoàn toàn có thể bị nhúng vào thẻ `<iframe>` trên trang web độc hại để thực hiện tấn công Clickjacking.
- **File/Dòng nghi ngờ:**
  - [frontend/next.config.ts](file:///D:/03_Study/Studxy/frontend/next.config.ts)
  - [backend/src/Swms.Api/Program.cs:187-210](file:///D:/03_Study/Studxy/backend/src/Swms.Api/Program.cs#L187-L210)
- **Đề xuất fix:** 
  1. Thêm cấu hình `headers()` trong `next.config.ts` để áp dụng `X-Frame-Options: SAMEORIGIN`, `X-Content-Type-Options: nosniff`, `Referrer-Policy`, và `poweredByHeader: false`.
  2. Thêm middleware bảo mật trong `Program.cs` của .NET Kestrel để gắn `X-Content-Type-Options: nosniff` và tắt server header qua `builder.WebHost.ConfigureKestrel(k => k.AddServerHeader = false)`.

---

### 🟡 SEC-MED-01: JWT Access Token lưu trữ trong `localStorage` trình duyệt (B1)
- **Mức độ:** **Medium** (CVSS:3.1/AV:N/AC:H/PR:N/UI:R/S:U/C:H/I:N/A:N — **5.9**)
- **Bước tái hiện:**
  1. Đăng nhập thành công trên giao diện web.
  2. Mở Console trình duyệt gõ `localStorage.getItem("swms_token")`.
- **Mong đợi:** Token xác thực phiên nên được quản lý qua `HttpOnly; SameSite=Strict` Cookie hoặc lưu trong bộ nhớ React state tạm thời để script JavaScript độc hại (XSS) không thể đọc trộm.
- **Thực tế:** Access token được lưu trong `localStorage`, cho phép bất kỳ mã JavaScript nào chạy trong trang đều có thể trích xuất token này.
- **File/Dòng nghi ngờ:**
  - [frontend/src/app/auth/page.tsx:280-310](file:///D:/03_Study/Studxy/frontend/src/app/auth/page.tsx#L280-L310)
- **Đề xuất fix:** Giữ Access Token trong bộ nhớ ngắn hạn của React Context/Zustand; tận dụng luồng Refresh Token HttpOnly Cookie đã có (`swms_refresh_token`) để tự động làm mới access token mỗi khi F5.

---

### 🟡 SEC-MED-02: Dò quét tài khoản người dùng (User Enumeration) qua `/api/v1/auth/register`
- **Mức độ:** **Medium** (CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:L/I:N/A:N — **5.3**)
- **Bước tái hiện:**
  1. Gửi request đăng ký với email đã tồn tại (`qa+victim-01@test.local`) -> Nhận `HTTP 409 Conflict` kèm thông báo `"Email đã tồn tại trong hệ thống."`.
  2. Gửi request đăng ký với email chưa có -> Nhận `HTTP 200 OK`.
  3. Chạy PoC: `node docs/qa-round2/scripts/a-hacker/01-auth-tests.mjs` (Ca `AUTH-02`).
- **Mong đợi:** Phản hồi đăng ký không nên để lộ trạng thái tồn tại của tài khoản cho kẻ tấn công bên ngoài.
- **Thực tế:** Kẻ tấn công có thể quét danh sách email sinh viên/học viên để xác định ai đang sử dụng hệ thống.
- **File/Dòng nghi ngờ:**
  - [backend/src/Swms.Api/Features/SwmsEndpoints.cs:393-396](file:///D:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L393-L396)
- **Đề xuất fix:** Áp dụng luồng gửi email xác nhận tiêu chuẩn (Generic response: "Nếu tài khoản hợp lệ, liên kết kích hoạt sẽ được gửi tới hòm thư") kèm rate-limiting chống quét hàng loạt.

---

### 🟡 SEC-MED-03: Chính sách mật khẩu quá yếu (Chỉ yêu cầu tối thiểu 6 ký tự)
- **Mức độ:** **Medium** (CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:L/I:N/A:N — **5.3**)
- **Bước tái hiện:**
  1. Gửi request đăng ký với mật khẩu `"123456"`.
  2. Chạy PoC: `node docs/qa-round2/scripts/a-hacker/01-auth-tests.mjs` (Ca `AUTH-04`).
- **Mong đợi:** Máy chủ từ chối các mật khẩu quá đơn giản, yêu cầu độ dài tối thiểu 8 ký tự kèm chữ hoa, chữ thường và số/ký tự đặc biệt.
- **Thực tế:** Máy chủ chấp nhận đăng ký và đổi mật khẩu thành công với `"123456"`.
- **File/Dòng nghi ngờ:**
  - [backend/src/Swms.Api/Features/SwmsEndpoints.cs:383](file:///D:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L383)
  - [backend/src/Swms.Api/Features/SwmsEndpoints.cs:605](file:///D:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L605)
- **Đề xuất fix:** Bổ sung regex kiểm tra độ phức tạp mật khẩu: tối thiểu 8 ký tự, có ít nhất 1 số và 1 ký tự đặc biệt, kiểm tra danh sách 10.000 mật khẩu phổ biến nhất.

---

### 🟡 SEC-MED-04: Lưu trữ trực tiếp mã HTML/JavaScript không qua lọc (Nguy cơ Stored XSS)
- **Mức độ:** **Medium** (CVSS:3.1/AV:N/AC:H/PR:L/UI:R/S:U/C:L/I:L/A:N — **4.6**)
- **Bước tái hiện:**
  1. Kẻ tấn công tạo sổ tay hoặc thẻ flashcard với nội dung chứa thẻ `<script>alert('XSS')</script>` hoặc `<img src=x onerror=alert(1)>`.
  2. Kiểm tra dữ liệu lưu trữ trong DB và phản hồi API.
  3. Chạy PoC: `node docs/qa-round2/scripts/a-hacker/03-injection-tests.mjs` (Ca `INJ-03`).
- **Mong đợi:** Backend nên làm sạch (sanitize) hoặc mã hóa thực thể HTML trước khi ghi vào cơ sở dữ liệu.
- **Thực tế:** Backend lưu trữ nguyên vẹn các thẻ HTML độc hại. Dù giao diện React hiện tại tự động escape qua JSX, các tính năng như xuất file PDF, xuất Markdown sang HTML của bên thứ 3 hoặc gửi email thông báo có thể bị kích hoạt XSS.
- **File/Dòng nghi ngờ:**
  - [backend/src/Swms.Api/Features/SwmsEndpoints.cs:3140-3260](file:///D:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L3140-L3260)
- **Đề xuất fix:** Tích hợp thư viện làm sạch HTML (như `Ganss.Xss.HtmlSanitizer`) ở các điểm tiếp nhận dữ liệu văn bản người dùng nhập.

---

### ⚪ SEC-LOW-01: Cho phép người dùng thường kích hoạt đồng bộ toàn bộ tài nguyên đĩa
- **Mức độ:** **Low** (CVSS:3.1/AV:N/AC:L/PR:L/UI:N/S:U/C:N/I:N/A:L — **3.1**)
- **Bước tái hiện:**
  1. Người dùng với quyền `student` gửi request `POST /api/v1/courses/sync-from-documents`.
  2. Máy chủ thực thi đồng bộ quét lại toàn bộ cây thư mục tài liệu trên ổ đĩa và so khớp DB.
- **Mong đợi:** Chỉ quản trị viên (`admin`) hoặc giảng viên (`instructor`) mới được phép kích hoạt tiến trình nặng này.
- **Thực tế:** Bất kỳ học viên nào đã đăng nhập đều có thể gọi liên tục để gây tải CPU và I/O ổ đĩa.
- **File/Dòng nghi ngờ:**
  - [backend/src/Swms.Api/Features/SwmsEndpoints.cs:1249-1254](file:///D:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L1249-L1254)
- **Đề xuất fix:** Bổ sung kiểm tra vai trò người dùng `if (user.Role != "admin") return Results.Forbid();` hoặc giới hạn rate limit 1 lần/giờ.

---

### ⚪ SEC-LOW-02: Khóa bảo mật AI Proxy và mật khẩu Postgres ghi trực tiếp trong `appsettings.json`
- **Mức độ:** **Low** (CVSS:3.1/AV:L/AC:L/PR:L/UI:N/S:U/C:L/I:N/A:N — **3.3**)
- **Bước tái hiện:**
  1. Đọc [appsettings.json](file:///D:/03_Study/Studxy/backend/src/Swms.Api/appsettings.json#L12-L33).
  2. Nhận thấy `AiProxy:ApiKey` (`sk-5a7663...`) và mật khẩu PostgreSQL (`swms_password_dev_2026`) được ghi bản rõ trong tệp cấu hình được commit lên git.
- **Mong đợi:** Mọi API key và mật khẩu kết nối cơ sở dữ liệu phải được nạp qua Environment Variables hoặc Secret Store.
- **File/Dòng nghi ngờ:**
  - [backend/src/Swms.Api/appsettings.json:12](file:///D:/03_Study/Studxy/backend/src/Swms.Api/appsettings.json#L12), [backend/src/Swms.Api/appsettings.json:33](file:///D:/03_Study/Studxy/backend/src/Swms.Api/appsettings.json#L33)
- **Đề xuất fix:** Thay thế giá trị cụ thể trong `appsettings.json` bằng chuỗi giữ chỗ rỗng `""` và truyền qua biến môi trường khi deploy.

---

## 4. CÁC ĐIỂM CHƯA KIỂM CHỨNG (UNVERIFIED SCOPE)

1. **Google OAuth 2.0 Real Provider Handshake:** Vì môi trường dev/local không cấu hình `GOOGLE_CLIENT_ID` thật, kiểm thử chỉ kiểm chứng mã nguồn giải mã ID Token ([GoogleAuthService.cs](file:///D:/03_Study/Studxy/backend/src/Swms.Api/Infrastructure/Security/GoogleAuthService.cs)), chưa thực hiện bắt tay trực tiếp với Google IDP thực tế.
2. **PostgreSQL Row-Level Locking Under 100+ Concurrent Threads:** Kiểm thử hiện tại chạy trên cơ sở dữ liệu SQLite fallback local (có SqlitePragmaConnectionInterceptor và WAL mode). Cơ chế `SELECT ... FOR UPDATE` trên PostgreSQL 17 thực tế cần môi trường Docker Postgres để kiểm chứng tải cao.
3. **Third-Party Dependency Vulnerability Tree (npm audit / dotnet list package --vulnerable):** Kiểm thử tập trung vào mã nguồn ứng dụng và API logic, chưa quét sâu cây phụ thuộc bên thứ 3.

---

## 5. GÓP Ý NHƯ NGƯỜI DÙNG (USER EXPERIENCE & PRIVACY FEEDBACK)

Đóng vai một học viên thực tế sử dụng Studxy để lưu trữ toàn bộ tài liệu học tập, ghi chú cá nhân và tiến trình học, tôi có những cảm nhận và mong muốn sau:

1. **Cảm nhận về sự riêng tư & quyền tự chủ dữ liệu (Data Sovereignty):**
   - **Rất an tâm về phân quyền IDOR:** Các ghi chú, tài liệu PDF và thẻ flashcard cá nhân được hệ thống bảo vệ rất nghiêm ngặt. Khi tôi thử dùng tài khoản khác để xem trộm bài học hoặc tải tệp tài liệu của tài khoản chính, hệ thống đều từ chối dứt khoát với mã lỗi 403 Forbidden.
   - **Lo ngại về tài khoản Demo:** Việc nhìn thấy tài khoản demo có sẵn với mật khẩu cố định khiến tôi hơi băn khoăn liệu tài khoản thật của mình có bị người khác đăng nhập nhầm hoặc truy cập vào dữ liệu không.

2. **Các tính năng an toàn & quyền riêng tư tôi mong muốn có:**
   - **Xóa tài khoản vĩnh viễn (Right to be Forgotten - GDPR):** Hiện tại chưa có nút "Xóa tài khoản của tôi" trong phần Cài đặt. Tôi muốn có quyền tự xóa toàn bộ tài khoản, tài liệu tải lên và lịch sử học tập bất cứ lúc nào.
   - **Xuất toàn bộ dữ liệu cá nhân (Takeout / Export All Data):** Studxy đã hỗ trợ xuất từng sổ tay ra Markdown (`/export-markdown`) và từng khóa học ra `.zip` rất tiện. Tôi mong muốn có 1 nút duy nhất "Xuất toàn bộ kho kiến thức của tôi" (tất cả PDF, ghi chú, thẻ FSRS) về máy tính cá nhân để lưu trữ ngoại tuyến.
   - **Xác thực 2 yếu tố (2FA / OTP):** Với một nền tảng chứa nhiều tài liệu học tập và nghiên cứu quan trọng, tôi muốn có tùy chọn bật xác thực 2 bước (Google Authenticator / TOTP) khi đăng nhập bằng mật khẩu.
   - **Xem danh sách các thiết bị & phiên đăng nhập đang hoạt động:** Dù backend đã có cơ chế Token Family rất xịn, giao diện người dùng vẫn chưa có bảng danh sách hiển thị "Các phiên đang đăng nhập" kèm nút "Đăng xuất khỏi các thiết bị khác". Tính năng này sẽ giúp người dùng an tâm tuyệt đối khi lỡ đăng nhập trên máy tính trường hoặc quán net.
