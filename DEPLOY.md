# Hướng dẫn Triển khai Production — Studxy (SWMS v2.0)

Tài liệu này mô tả hai phương án triển khai chuẩn Production cho hệ thống **Studxy (SWMS)**:
1. **Docker Compose (`docker-compose.prod.yml`)** — khuyến nghị cho VPS/Cloud với PostgreSQL 17 + pgvector & Redis 7.
2. **Windows Native (`scripts/start-prod.ps1`)** — triển khai trực tiếp trên máy Windows Server hoặc máy cá nhân với .NET 10 + Node.js 22.

---

## 1. Cấu hình Biến môi trường

Sao chép mẫu `.env.example` thành `.env` tại thư mục gốc dự án và thay thế toàn bộ giá trị bí mật trước khi triển khai thật:

```bash
cp .env.example .env
```

Các biến quan trọng bắt buộc phải thay khi lên Production:
- `POSTGRES_PASSWORD`: mật khẩu cơ sở dữ liệu PostgreSQL 17.
- `JWT_SIGNING_KEY`: chuỗi bí mật HMAC-SHA256 dài tối thiểu 64 ký tự.
- `AES_MASTER_KEY_BASE64`: khóa AES-256-GCM 32 byte mã hóa Base64 (`openssl rand -base64 32`).
- `CORS_ALLOWED_ORIGINS`: danh sách domain Frontend thực tế, phân tách bằng dấu phẩy (ví dụ `https://studxy.vn,https://www.studxy.vn`).
- `BACKEND_INTERNAL_URL`: địa chỉ nội bộ để Next.js BFF rewrite `/api/v1/*` và `/health` tới Backend (`http://backend:5080` trong Docker hoặc `http://127.0.0.1:5080` trên máy đơn).
- `NEXT_PUBLIC_SITE_URL`: domain công khai cho `sitemap.xml` và `robots.txt`.
- `AI_PROXY_BASE_URL`, `AI_PROXY_API_KEY`, `AI_PROXY_MODEL`: cấu hình dịch vụ LLM thượng nguồn (nếu không bật proxy, hệ thống tự fallback sang phản hồi học thuật tất định mà không làm sập luồng học).

---

## 2. Phương án A — Triển khai bằng Docker Compose (PostgreSQL 17 + Redis + Backend + Frontend)

### Yêu cầu
- Docker Engine `>= 24` và Docker Compose v2.
- Cổng mở: `3000` (Frontend Next.js) và tùy chọn `5080` (Backend API nội bộ/Reverse Proxy).

### Khởi chạy
```bash
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

### Kiểm tra trạng thái & sức khỏe
```bash
docker compose -f docker-compose.prod.yml ps
curl -fsS http://127.0.0.1:3000/health
```

Phản hồi `/health` hợp lệ sẽ trả `HTTP 200` kèm `"databaseMode": "PostgreSQL 17 + pgvector"`.

---

## 3. Phương án B — Triển khai Trực tiếp trên Windows (Production Native)

### Yêu cầu
- `.NET 10 SDK/Runtime` (`dotnet --version`)
- `Node.js 20/22+` (`node --version`)

### Chạy 1 lệnh
Tại thư mục gốc `d:\03_Study\Studxy`, chạy PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/start-prod.ps1 -OpenBrowser
```

Script tự động:
1. Giải phóng cổng `5080` và `3000`.
2. Biên dịch Backend ở chế độ `Release` (`backend/Swms.Api.sln`).
3. Biên dịch Frontend Next.js Production (`npx next build`).
4. Khởi chạy `Swms.Api.dll` trên `http://127.0.0.1:5080` và `next start --port 3000`.
5. Kiểm tra `/health` cả trực tiếp và qua proxy Next.js trước khi mở `http://127.0.0.1:3000/app`.

> [!NOTE]
> Khi không có PostgreSQL chạy trên cổng `5432`, Backend tự động sử dụng cơ sở dữ liệu SQLite bền vững tại `swms_local_dev.db` kèm toàn bộ giáo trình mẫu từ thư mục `document/`.

---

## 4. Kiểm chứng Trước khi Deploy (Zero-Facade Verification)

Chạy kiểm chứng toàn trình tự động (Backend xUnit + Multi-User Benchmark 30 VUs + Playwright E2E trên hệ thống thật, không mock):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```

Kỳ vọng: kết thúc với `STATUS: ALL CHECKS PASSED (EXIT CODE 0)`.

---

## 5. Sao lưu & Khôi phục Dữ liệu

- **SQLite (`swms_local_dev.db`)**: tắt tiến trình Backend trước khi sao lưu đồng thời 3 tệp `swms_local_dev.db`, `swms_local_dev.db-wal`, `swms_local_dev.db-shm` (hoặc chạy `VACUUM INTO` khi đang online) và thư mục tệp tải lên `App_Data/storage/documents`.
- **PostgreSQL 17 (Docker)**:
  ```bash
  docker exec studxy-postgres pg_dump -U swms_user -d swms_db -Fc > backup_studxy_$(date +%Y%m%d).dump
  ```
