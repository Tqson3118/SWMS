import os
import sys

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

notes_dir = r"\\?\D:\03_Study\Roadmap\Investing\notes"
canonical_modules = {"module-1", "module-2", "module-3", "module-4"}

redirect_content = """# ⚠️ LEGACY / REFERENCE DIRECTORY — CANONICAL REDIRECT

> [!WARNING] **LEGACY / NON-CANONICAL CONTENT — DO NOT USE AS PRIMARY SOURCE**
> Thư mục này chứa bản thảo tham khảo lịch sử (legacy reference).
> Hệ thống học liệu chính thức (canonical source of truth) của khóa học Đầu tư 18 tuần được tổ chức chuẩn hóa tại:
> - [Module 1: Nền tảng](../../notes/module-1/) (Tuần 01 – 04)
> - [Module 2: Định giá](../../notes/module-2/) (Tuần 05 – 10)
> - [Module 3: Thị trường VN](../../notes/module-3/) (Tuần 11 – 14)
> - [Module 4: Danh mục & Tâm lý](../../notes/module-4/) (Tuần 15 – 18)
>
> Vui lòng tra cứu và học tập theo các bài học canonical trong `notes/module-{1..4}/week-*.md`.
"""

entries = os.listdir(notes_dir)
misconception_dirs = [
    os.path.join(notes_dir, d)
    for d in entries
    if d.startswith("module-") and d not in canonical_modules
]

print(f"Total misconception directories found across Investing: {len(misconception_dirs)}")

count = 0
for dir_path in misconception_dirs:
    readme_path = os.path.join(dir_path, "README.md")
    try:
        with open(readme_path, "w", encoding="utf-8") as f:
            f.write(redirect_content)
        count += 1
    except Exception as e:
        print(f"Error writing to {dir_path}: {e}")

print(f"Successfully marked {count}/{len(misconception_dirs)} legacy directories with canonical redirect notices.")
