# Hướng dẫn Triển khai Production — Studxy (SWMS v2.0)

Tài liệu này hướng dẫn 3 phương án triển khai chuẩn Production cho hệ thống **Studxy (SWMS)**:
1. **Cloud Split (Khuyến nghị):** **Frontend trên Vercel** + **Backend (.NET 10) trên MonsterASP.NET** + **Database trên Supabase PostgreSQL**.
2. **Docker Compose (`docker-compose.prod.yml`)** — cho VPS/Cloud với PostgreSQL 17 + pgvector & Redis 7.
3. **Windows Native (`scripts/start-prod.ps1`)** — chạy trực tiếp trên Windows Server hoặc máy cá nhân với .NET 10 + Node.js 22.

---

## 1. Phương án Khuyến nghị — Vercel (Frontend) + MonsterASP.NET (Backend) + Supabase (DB)

Kiến trúc sử dụng **Next.js BFF Rewrite** (`/api/v1/:path*` và `/health` proxy tới `BACKEND_INTERNAL_URL`), giúp trình duyệt gửi mọi request cùng nguồn gốc (Same-Origin) tới Vercel để cookie `HttpOnly; SameSite=Strict` hoạt động ổn định, không gặp lỗi chặn cookie bên thứ ba.

### Bước 1: Chuẩn bị & Đóng gói Backend cho MonsterASP.NET
1. Bảo mật cục bộ: File `backend/src/Swms.Api/appsettings.json` đã được làm sạch (không chứa mật khẩu Supabase hay Google Client ID). Khi chạy cục bộ trên máy dev, backend tự nạp file `backend/src/Swms.Api/appsettings.Development.local.json` (đã được `.gitignore` loại trừ khỏi Git).
2. Chạy script đóng gói từ thư mục gốc dự án:
   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts/publish-monsterasp.ps1
   ```
   Script sẽ tạo gói zip sạch tại `publish/studxy-backend-monsterasp.zip` (đã loại bỏ `appsettings.*.local.json`, file `.db`, `.pdb`, và đặt `startupTimeLimit="120"` trong `web.config`).

### Bước 2: Deploy Backend lên MonsterASP.NET
1. Đăng nhập **MonsterASP.NET Control Panel** → chọn Website → **File Manager** (hoặc FTP), vào thư mục gốc `wwwroot` của website.
2. Upload và giải nén toàn bộ nội dung `publish/studxy-backend-monsterasp.zip` vào `wwwroot` (sao cho `Swms.Api.dll` và `web.config` nằm ngay ở thư mục gốc `wwwroot`).
3. Vào **Websites → [Site của bạn] → Scripting**:
   - Chọn runtime **.NET 10** (InProcess).
   - Vào **Environment variables** và khai báo các biến sau:

| Tên biến | Giá trị | Ghi chú |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Chế độ Production |
| `SWMS_USE_POSTGRES` | `true` | Bật PostgreSQL |
| `ConnectionStrings__Postgres` | `Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<ref>;Password=<pass>;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20;Timeout=20;Command Timeout=120;KeepAlive=30` | Chuỗi kết nối Supabase |
| `Jwt__SigningKey` | `<chuỗi ngẫu nhiên >= 64 ký tự>` | Khóa ký JWT HMAC-SHA256 |
| `Security__AesMasterKeyBase64` | `<chuỗi 32-byte Base64>` | Tạo bằng `openssl rand -base64 32` |
| `CORS_ALLOWED_ORIGINS` | `https://<ten-app>.vercel.app` | Domain Frontend Vercel (phân tách bằng dấu `,` nếu nhiều domain) |
| `Authentication__Google__ClientId` | `<Google OAuth Client ID>` | Tùy chọn (nếu dùng Đăng nhập Google) |
| `AiProxy__BaseUrl` | `<URL OpenAI-compatible proxy>` | Tùy chọn (bỏ trống sẽ dùng fallback nội bộ) |
| `AiProxy__ApiKey` | `<API Key AI>` | Tùy chọn |
| `AiProxy__Model` | `gemini-3.8-flash` | Tùy chọn |

4. Restart App Pool / Website trên MonsterASP.NET và kiểm tra `https://<domain-backend-cua-ban>/health` phải trả về HTTP `200`:
   ```json
   {"status":"healthy","service":"SWMS Backend .NET 10 LTS"}
   ```

> [!TIP]
> Khi Supabase đã có sẵn dữ liệu (>= 14 khóa học), Backend tự động dùng chế độ **Fast cold-start** (bỏ qua đồng bộ nặng khi khởi động, chỉ kiểm tra schema nhẹ ở nền) để khởi động dưới ~2 giây và không bị IIS timeout. Nếu cần ép chạy lại đồng bộ/seed khi startup, tạm thời đặt `SWMS_FORCE_STARTUP_SEED=true`.

### Bước 3: Deploy Frontend lên Vercel
1. Push source code lên GitHub (lưu ý không commit thư mục `publish/` hay file `.env` / `appsettings.*.local.json`).
2. Trên **Vercel Dashboard → Add New Project**, import repository và cấu hình:
   - **Framework Preset:** `Next.js`
   - **Root Directory:** `frontend`
3. Thêm **Environment Variables** trên Vercel:

| Tên biến | Giá trị | Ghi chú |
| --- | --- | --- |
| `BACKEND_INTERNAL_URL` | `https://<domain-backend-cua-ban>` | URL HTTPS gốc của backend MonsterASP (**không** có dấu `/` cuối) |
| `NEXT_PUBLIC_SITE_URL` | `https://<ten-app>.vercel.app` | Dùng cho `sitemap.xml` và `robots.txt` |
| `NEXT_PUBLIC_GOOGLE_CLIENT_ID` | `<Google OAuth Client ID>` | Tùy chọn (khớp với backend) |

4. Bấm **Deploy**. Sau khi deploy xong:
   - Nếu có dùng **Google Sign-In**, vào **Google Cloud Console → Credentials → OAuth 2.0 Client IDs** và thêm `https://<ten-app>.vercel.app` vào **Authorized JavaScript origins**.
   - Cập nhật lại `CORS_ALLOWED_ORIGINS` trên MonsterASP.NET nếu bạn thêm custom domain sau này.
   - Mở `https://<ten-app>.vercel.app/health` để xác nhận Vercel đã rewrite thành công sang Backend MonsterASP.

---

## 2. Phương án B — Triển khai bằng Docker Compose (PostgreSQL 17 + Redis + Backend + Frontend)

### Yêu cầu
- Docker Engine `>= 24` và Docker Compose v2.
- Cổng mở: `3000` (Frontend Next.js) và tùy chọn `5080` (Backend API nội bộ/Reverse Proxy).

### Khởi chạy
```bash
cp .env.example .env
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

### Kiểm tra trạng thái & sức khỏe
```bash
docker compose -f docker-compose.prod.yml ps
curl -fsS http://127.0.0.1:3000/health
```

Phản hồi `/health` hợp lệ sẽ trả `HTTP 200` với `{"status":"healthy","service":"SWMS Backend .NET 10 LTS"}`.

---

## 3. Phương án C — Triển khai Trực tiếp trên Windows (Production Native)

### Yêu cầu
- `.NET 10 SDK/Runtime` (`dotnet --version`)
- `Node.js 20/22+` (`node --version`)

### Chạy 1 lệnh
Tại thư mục gốc `d:\03_Study\Studxy`, chạy PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/start-prod.ps1 -OpenBrowser
```

> [!NOTE]
> Khi chạy cục bộ không cấu hình `ConnectionStrings:Postgres` (hoặc không có file `appsettings.Development.local.json`), Backend tự động sử dụng cơ sở dữ liệu SQLite bền vững tại `swms_local_dev.db`.

---

## 4. Kiểm chứng Trước khi Deploy (Zero-Facade Verification)

Chạy kiểm chứng toàn trình tự động:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```

---

## 5. Sao lưu & Khôi phục Dữ liệu

- **Supabase PostgreSQL**: sử dụng tính năng Backup/Point-in-Time Recovery trên Supabase Dashboard hoặc `pg_dump`.
- **SQLite (`swms_local_dev.db`)**: tắt tiến trình Backend trước khi sao lưu đồng thời 3 tệp `swms_local_dev.db`, `swms_local_dev.db-wal`, `swms_local_dev.db-shm` và thư mục tệp tải lên `App_Data/storage/documents`.
- **PostgreSQL 17 (Docker)**:
  ```bash
  docker exec studxy-postgres pg_dump -U swms_user -d swms_db -Fc > backup_studxy_$(date +%Y%m%d).dump
  ```
