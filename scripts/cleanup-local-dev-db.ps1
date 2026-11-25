param(
    [string]$DbPath = "d:\03_Study\Studxy\swms_local_dev.db",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $DbPath)) {
    Write-Error "Database file '$DbPath' does not exist."
    exit 1
}

$resolvedDbPath = (Resolve-Path -LiteralPath $DbPath).Path
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " STUDXY DATABASE CLEANUP SCRIPT (Tier 3 DB Maintenance)" -ForegroundColor Cyan
Write-Host " Target DB: $resolvedDbPath" -ForegroundColor Cyan
Write-Host " Mode: $(if ($DryRun) { '[DRY-RUN] READ-ONLY' } else { '[EXECUTE] WRITE MODE' })" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

if (-not $DryRun) {
    $backupPath = "$resolvedDbPath.bak.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Write-Host "Creating mandatory pre-cleanup backup at: $backupPath" -ForegroundColor Yellow
    Copy-Item -LiteralPath $resolvedDbPath -Destination $backupPath -Force

    $srcLen = (Get-Item -LiteralPath $resolvedDbPath).Length
    $bakLen = (Get-Item -LiteralPath $backupPath).Length

    if ($bakLen -ne $srcLen -or $bakLen -eq 0) {
        Write-Error "Backup verification failed! Source size: $srcLen, Backup size: $bakLen. Aborting cleanup immediately!"
        exit 1
    }
    Write-Host "Backup created and verified successfully ($bakLen bytes)." -ForegroundColor Green
}

$pyScript = @'
import sys
import os
import sqlite3
import re

db_path = sys.argv[1]
dry_run = sys.argv[2].lower() == 'true'

conn = sqlite3.connect(db_path)
conn.execute("PRAGMA foreign_keys = OFF;")
c = conn.cursor()

CURATED_SLUGS = {
    'doc-course-studxy-fullstack-bootcamp',
    'document-course-investing',
    'document-course-psychology',
    'document-course-personalfinance',
    'document-course-lifeskills',
    'document-course-ielts',
    'tieng-anh-giao-tiep-thuc-chien-a2-b1',
    'tieng-nhat-so-cap-n5',
    'tieng-trung-so-cap-hsk1',
    'he-dieu-hanh-bo-nho-ao'
}

PROTECTED_GUIDS = {
    '22222222-2222-4222-8222-222222222222',
    '22222222-2222-4222-8222-222222222223',
    '22222222-2222-4222-8222-222222222227',
    '22222222-2222-4222-8222-222222222228'
}

RETIRED_SLUGS = {
    'document-course-dotnet-react-roadmap',
    'document-course-fullstack-csharp-react-refresh',
    'lap-trinh-javascript-tu-duy-giai-quyet-van-de',
    'dau-tu-quan-tri-danh-muc-thuc-chien',
    'tam-ly-hoc-ung-dung-khoa-hoc-tri-nho'
}

TEST_EMAIL_PATTERNS = [
    r'@test\.local', r'@example\.com', r'@bench\.studxy\.vn',
    r'qa\+', r'qa\.', r'v2\.', r'polish\.', r'r5\.', r'studio\.test\.',
    r'e2e\.', r'probe\.', r'probe2\.', r'tour\.', r'test\.student\.',
    r'rtr\.concurrent\.', r'occ\.user\.', r'ai\.quota\.', r'race\.dup\.',
    r'autoprov\.', r'feature7\.', r'fsrs\.idem\.'
]

TEST_NAME_PATTERNS = [
    'e2e', 'tester', 'abuser', 'virtual learner', 'người mới tablet', 'người mới mobile',
    'học viên kiểm thử', 'v2 completeness tester'
]

courses = c.execute("""
    SELECT c.id, c.slug, c.title, c.instructor_id, u.email, u.full_name
    FROM courses c
    LEFT JOIN users u ON c.instructor_id = u.id
""").fetchall()

total_courses = len(courses)
protected_courses = []
to_clean_courses = []

for row in courses:
    cid, slug, title, inst_id, email, full_name = row
    cid_str = str(cid).lower()
    slug_str = (slug or '').strip().lower()
    email_str = (email or '').strip().lower()
    name_str = (full_name or '').strip().lower()
    title_str = (title or '').strip().lower()

    # Absolute protected rule: 10 curated slugs and 4 system GUIDs
    if slug_str in CURATED_SLUGS or cid_str in PROTECTED_GUIDS:
        protected_courses.append(row)
        continue

    # Absolute protected rule: system user son.tran@studxy.vn or real user personal workspaces
    is_real_user = email_str in {'son.tran@studxy.vn', 'minh.anh@studxy.vn', 'thaiquangson0311@gmail.com'}
    if is_real_user and slug_str.startswith('personal-workspace-'):
        protected_courses.append(row)
        continue

    # Targets for cleanup
    is_retired = slug_str in RETIRED_SLUGS
    is_test_email = any(re.search(pat, email_str) for pat in TEST_EMAIL_PATTERNS)
    is_test_name = any(pat in name_str for pat in TEST_NAME_PATTERNS)
    is_test_title = 'double click test' in title_str or 'kiểm thử tự động' in title_str or 'import test' in title_str

    if is_retired or is_test_email or is_test_name or is_test_title:
        to_clean_courses.append(row)
    else:
        # Keep non-test personal workspaces or custom courses by non-test users
        protected_courses.append(row)

clean_course_ids = [r[0] for r in to_clean_courses]
clean_id_set = set(clean_course_ids)

print(f"\n--- THỐNG KÊ TRƯỚC KHI DỌN DẸP ---")
print(f"Tổng số khóa học hiện tại: {total_courses}")
print(f"Số lượng khóa học được BẢO VỆ: {len(protected_courses)}")

found_curated = {r[1] for r in protected_courses if (r[1] or '').lower() in CURATED_SLUGS}
print(f"Đã xác nhận {len(found_curated)}/10 khóa chuẩn hệ thống an toàn:")
for cs in sorted(CURATED_SLUGS):
    status = "OK" if cs in found_curated else "MISSING (chưa sync trong DB này)"
    print(f"  - [{status}] {cs}")

print(f"Số lượng khóa học sẽ dọn dẹp: {len(to_clean_courses)}")
if to_clean_courses:
    print("\nMẫu tối đa 15 khóa học sẽ dọn (slug | instructor_email | title):")
    for r in to_clean_courses[:15]:
        print(f"  {r[1]} | {r[4]} | {r[2]}")

# Child record counts
def count_in_courses(table_name, col_name="course_id"):
    if not clean_course_ids:
        return 0
    placeholders = ",".join(["?"] * len(clean_course_ids))
    return c.execute(f"SELECT COUNT(*) FROM {table_name} WHERE {col_name} IN ({placeholders})", clean_course_ids).fetchone()[0]

child_chapters = count_in_courses("chapters")
child_enrollments = count_in_courses("course_enrollments")
child_reviews = count_in_courses("course_reviews")

# Find lessons belonging to chapters of these courses
if clean_course_ids:
    ph = ",".join(["?"] * len(clean_course_ids))
    lesson_rows = c.execute(f"SELECT l.id FROM lessons l JOIN chapters ch ON l.chapter_id = ch.id WHERE ch.course_id IN ({ph})", clean_course_ids).fetchall()
    clean_lesson_ids = [r[0] for r in lesson_rows]
else:
    clean_lesson_ids = []

print(f"\nSố lượng bản ghi con liên quan sẽ dọn dẹp:")
print(f"  Chapters: {child_chapters}")
print(f"  Lessons: {len(clean_lesson_ids)}")
print(f"  Course Enrollments: {child_enrollments}")
print(f"  Course Reviews: {child_reviews}")

if dry_run:
    print("\n[DRY-RUN] Chưa thay đổi database.")
    sys.exit(0)

# Perform execution with transaction
print("\nBắt đầu thực hiện dọn dẹp trong transaction (BEGIN IMMEDIATE)...")
c.execute("BEGIN IMMEDIATE;")

try:
    if clean_lesson_ids:
        les_ph = ",".join(["?"] * len(clean_lesson_ids))

        # Safely unlink references
        c.execute(f"UPDATE flashcards SET lesson_id = NULL WHERE lesson_id IN ({les_ph})", clean_lesson_ids)
        c.execute(f"UPDATE notebooks SET primary_source_lesson_id = NULL WHERE primary_source_lesson_id IN ({les_ph})", clean_lesson_ids)

        # Check if notebook_links has ref_lesson_id
        nl_cols = [r[1] for r in c.execute("PRAGMA table_info(notebook_links)").fetchall()]
        if 'ref_lesson_id' in nl_cols:
            c.execute(f"DELETE FROM notebook_links WHERE ref_lesson_id IN ({les_ph})", clean_lesson_ids)

        c.execute(f"DELETE FROM user_lesson_progress WHERE lesson_id IN ({les_ph})", clean_lesson_ids)
        c.execute(f"DELETE FROM lesson_versions WHERE lesson_id IN ({les_ph})", clean_lesson_ids)
        c.execute(f"DELETE FROM annotations WHERE lesson_id IN ({les_ph})", clean_lesson_ids)

        # Quizzes
        quiz_rows = c.execute(f"SELECT id FROM practice_quizzes WHERE lesson_id IN ({les_ph})", clean_lesson_ids).fetchall()
        clean_quiz_ids = [r[0] for r in quiz_rows]
        if clean_quiz_ids:
            qz_ph = ",".join(["?"] * len(clean_quiz_ids))
            c.execute(f"DELETE FROM practice_quiz_items WHERE quiz_id IN ({qz_ph})", clean_quiz_ids)
        c.execute(f"DELETE FROM practice_quizzes WHERE lesson_id IN ({les_ph})", clean_lesson_ids)

        # Practice Activities & Attempts
        c.execute(f"DELETE FROM practice_attempts WHERE lesson_id IN ({les_ph})", clean_lesson_ids)
        c.execute(f"DELETE FROM practice_activities WHERE lesson_id IN ({les_ph})", clean_lesson_ids)

    if clean_course_ids:
        c_ph = ",".join(["?"] * len(clean_course_ids))
        c.execute(f"DELETE FROM practice_attempts WHERE course_id IN ({c_ph})", clean_course_ids)
        c.execute(f"DELETE FROM practice_activities WHERE course_id IN ({c_ph})", clean_course_ids)
        pq_cols = [r[1] for r in c.execute("PRAGMA table_info(practice_quizzes)").fetchall()]
        if 'course_id' in pq_cols:
            c.execute(f"DELETE FROM practice_quizzes WHERE course_id IN ({c_ph})", clean_course_ids)
        c.execute(f"DELETE FROM course_reviews WHERE course_id IN ({c_ph})", clean_course_ids)
        c.execute(f"DELETE FROM course_enrollments WHERE course_id IN ({c_ph})", clean_course_ids)

        # Documents & Pages belonging to cleaned courses
        doc_rows = c.execute(f"SELECT id FROM documents WHERE course_id IN ({c_ph})", clean_course_ids).fetchall()
        clean_doc_ids = [r[0] for r in doc_rows]
        if clean_doc_ids:
            doc_ph = ",".join(["?"] * len(clean_doc_ids))
            c.execute(f"UPDATE flashcards SET document_id = NULL WHERE document_id IN ({doc_ph})", clean_doc_ids)
            c.execute(f"UPDATE notebooks SET primary_source_document_id = NULL WHERE primary_source_document_id IN ({doc_ph})", clean_doc_ids)
            nl_cols = [r[1] for r in c.execute("PRAGMA table_info(notebook_links)").fetchall()]
            if 'ref_document_id' in nl_cols:
                c.execute(f"DELETE FROM notebook_links WHERE ref_document_id IN ({doc_ph})", clean_doc_ids)
            c.execute(f"DELETE FROM annotations WHERE document_id IN ({doc_ph})", clean_doc_ids)
            doc_quiz_rows = c.execute(f"SELECT id FROM practice_quizzes WHERE document_id IN ({doc_ph})", clean_doc_ids).fetchall()
            doc_quiz_ids = [r[0] for r in doc_quiz_rows]
            if doc_quiz_ids:
                dqz_ph = ",".join(["?"] * len(doc_quiz_ids))
                c.execute(f"DELETE FROM practice_quiz_items WHERE quiz_id IN ({dqz_ph})", doc_quiz_ids)
                c.execute(f"DELETE FROM practice_quizzes WHERE id IN ({dqz_ph})", doc_quiz_ids)
            c.execute(f"DELETE FROM document_pages WHERE document_id IN ({doc_ph})", clean_doc_ids)
            c.execute(f"DELETE FROM documents WHERE course_id IN ({c_ph})", clean_course_ids)

        if clean_lesson_ids:
            les_ph = ",".join(["?"] * len(clean_lesson_ids))
            c.execute(f"DELETE FROM lessons WHERE id IN ({les_ph})", clean_lesson_ids)

        c.execute(f"DELETE FROM chapters WHERE course_id IN ({c_ph})", clean_course_ids)
        c.execute(f"DELETE FROM courses WHERE id IN ({c_ph})", clean_course_ids)

    conn.commit()
    print("Dọn dẹp thành công! Đã COMMIT thay đổi.")
except Exception as e:
    conn.rollback()
    print(f"LỖI trong quá trình dọn dẹp! Đã ROLLBACK: {e}")
    sys.exit(1)

# Verification after cleanup
remaining_courses = c.execute("SELECT COUNT(*) FROM courses").fetchone()[0]
son_tran_exists = c.execute("SELECT COUNT(*) FROM users WHERE email = 'son.tran@studxy.vn'").fetchone()[0]
print(f"\n--- THỐNG KÊ SAU KHI DỌN DẸP ---")
print(f"Tổng số khóa học còn lại: {remaining_courses}")
print(f"Xác nhận người dùng hệ thống 'son.tran@studxy.vn': {'CÒN NGUYÊN VẸN' if son_tran_exists else 'LỖI'}")
'@

$dryRunArg = if ($DryRun) { "true" } else { "false" }
$tempPyPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "studxy_cleanup_$([Guid]::NewGuid().ToString('N')).py")

try {
    [System.IO.File]::WriteAllText($tempPyPath, $pyScript, [System.Text.Encoding]::UTF8)
    & python $tempPyPath $resolvedDbPath $dryRunArg
    $pyExit = $LASTEXITCODE
    if ($pyExit -ne 0) {
        exit $pyExit
    }
}
finally {
    if (Test-Path -LiteralPath $tempPyPath) {
        Remove-Item -LiteralPath $tempPyPath -Force -ErrorAction SilentlyContinue
    }
}
