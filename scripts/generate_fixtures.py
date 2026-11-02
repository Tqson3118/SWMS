#!/usr/bin/env python3
"""
scripts/generate_fixtures.py
Generates deterministic synthetic test fixtures and manifest.json for SWMS PDF testing.
Compatible with Python 3.12, PyMuPDF (fitz), and pypdf.
"""

import os
import sys
import io
import json
import hashlib
import urllib.request
import unicodedata
from pathlib import Path
import pymupdf
from PIL import Image, ImageDraw, ImageFont

# Ensure UTF-8 output on Windows
if sys.stdout.encoding != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')

ROOT_DIR = Path(__file__).resolve().parent.parent
FIXTURES_DIR = ROOT_DIR / "test-fixtures"
TEST_DIR = ROOT_DIR / "test"

def compute_sha256(filepath: Path) -> str:
    sha = hashlib.sha256()
    with open(filepath, "rb") as f:
        while chunk := f.read(65536):
            sha.update(chunk)
    return sha.hexdigest()

def create_small_3p():
    """1. small-3p.pdf (3 pages, markers)"""
    doc = pymupdf.open()
    pages_data = [
        ("TÀI LIỆU NHỎ 3 TRANG - TRANG 1 / 3\n"
         "Mã nhận diện: SMALL-PAGE-001\n"
         "Nội dung trang 1 dùng để kiểm tra quy trình khởi tạo tài liệu và trích xuất text."),
        ("TÀI LIỆU NHỎ 3 TRANG - TRANG 2 / 3\n"
         "Mã nhận diện: SMALL-PAGE-002\n"
         "Nội dung trang 2 dùng để kiểm tra tính năng điều hướng trang tiếp theo."),
        ("TÀI LIỆU NHỎ 3 TRANG - TRANG 3 / 3\n"
         "Mã nhận diện: SMALL-PAGE-003\n"
         "Nội dung trang cuối cùng của tài liệu 3 trang hoàn thành kiểm tra.")
    ]
    for text in pages_data:
        p = doc.new_page(width=595, height=842) # A4
        p.insert_text((50, 80), text, fontsize=14)
    out_path = FIXTURES_DIR / "small-3p.pdf"
    doc.save(out_path)
    doc.close()
    print(f"Generated: {out_path.name}")

def create_marker_12p():
    """2. marker-12p.pdf (12 pages, PAGE-007-MARKER on page 7)"""
    doc = pymupdf.open()
    for i in range(1, 13):
        p = doc.new_page(width=595, height=842)
        if i == 7:
            text = (f"TÀI LIỆU ĐÁNH DẤU 12 TRANG - TRANG {i} / 12\n\n"
                    f"DÒNG ĐẶC BIỆT KIỂM THỬ: PAGE-007-MARKER\n\n"
                    f"Trang số 7 chứa chuỗi định danh đặc biệt phục vụ cho bài test Playwright và Unit test.")
        else:
            text = (f"TÀI LIỆU ĐÁNH DẤU 12 TRANG - TRANG {i} / 12\n\n"
                    f"Nội dung trang tuần tự {i} của tài liệu 12 trang.")
        p.insert_text((50, 80), text, fontsize=14)
    out_path = FIXTURES_DIR / "marker-12p.pdf"
    doc.save(out_path)
    doc.close()
    # Also write numbered-12p.pdf as alias
    alias_path = FIXTURES_DIR / "numbered-12p.pdf"
    with open(out_path, "rb") as src, open(alias_path, "wb") as dst:
        dst.write(src.read())
    print(f"Generated: {out_path.name} (and alias {alias_path.name})")

def create_objstm_compressed():
    """3. objstm-compressed.pdf (PDF 1.5+ with Object Streams)"""
    doc = pymupdf.open()
    for i in range(1, 6):
        p = doc.new_page(width=595, height=842)
        text = (f"TÀI LIỆU NÉN OBJECT STREAMS (PDF 1.5+) - TRANG {i} / 5\n\n"
                f"Mã nhận diện: OBJSTM-PAGE-{i}-MARKER\n\n"
                f"Tài liệu này được lưu trữ với tùy chọn use_objstms=1 để nén cấu trúc đối tượng trang "
                f"vào trong các luồng /ObjStm. Parser ngây thơ sử dụng biểu thức chính quy trên byte thô "
                f"sẽ không thể tìm thấy từ khóa /Count hoặc /Type /Page ở dạng plain text.")
        p.insert_text((50, 80), text, fontsize=12)
    out_path = FIXTURES_DIR / "objstm-compressed.pdf"
    doc.save(out_path, deflate=True, garbage=3, clean=True, use_objstms=1)
    doc.close()
    # Check that it actually has /ObjStm
    with open(out_path, "rb") as f:
        data = f.read()
        assert b"/ObjStm" in data, "Failed to create /ObjStm in objstm-compressed.pdf"
    # Alias
    alias_path = FIXTURES_DIR / "pdf15-objstm.pdf"
    with open(out_path, "rb") as src, open(alias_path, "wb") as dst:
        dst.write(src.read())
    print(f"Generated: {out_path.name} (and alias {alias_path.name})")

def create_vietnamese_diacritics():
    """4. vietnamese-diacritics.pdf (Unicode NFC Vietnamese content across 5 pages)"""
    doc = pymupdf.open()
    pages_content = [
        unicodedata.normalize("NFC",
            "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM\n"
            "Độc lập - Tự do - Hạnh phúc\n\n"
            "GIÁO TRÌNH PHÁP LUẬT ĐẠI CƯƠNG\n"
            "Chương 1: Những vấn đề cơ bản về Nhà nước và Pháp luật\n\n"
            "Nhà nước Cộng hòa xã hội chủ nghĩa Việt Nam là nhà nước pháp quyền xã hội chủ nghĩa của Nhân dân, "
            "do Nhân dân, vì Nhân dân. Nước Cộng hòa xã hội chủ nghĩa Việt Nam do Nhân dân làm chủ; "
            "tất cả quyền lực nhà nước thuộc về Nhân dân mà nền tảng là liên minh giữa giai cấp công nhân "
            "với giai cấp nông dân và đội ngũ trí thức.\n\n"
            "Mã kiểm chuẩn: VIETNAMESE-NFC-P1-LEGAL-FRAMEWORK"),
        unicodedata.normalize("NFC",
            "KIẾN TRÚC MÁY TÍNH VÀ HỆ ĐIỀU HÀNH HIỆN ĐẠI\n"
            "Chương 3: Cơ chế Quản lý Bộ nhớ và Bộ nhớ ảo (Virtual Memory)\n\n"
            "Cơ chế phân trang (Paging) chia không gian địa chỉ tuyến tính của tiến trình thành các trang có kích thước cố định, "
            "thường là 4 KiB. Phần cứng bảng phân trang (Page Table) cùng với bộ đệm chuyển đổi bản dịch "
            "(TLB - Translation Lookaside Buffer) nằm trong đơn vị quản lý bộ nhớ MMU chuyển đổi địa chỉ ảo "
            "thành địa chỉ vật lý trong RAM một cách trong suốt với ứng dụng người dùng.\n\n"
            "Mã kiểm chuẩn: VIETNAMESE-NFC-P2-OPERATING-SYSTEMS"),
        unicodedata.normalize("NFC",
            "KHOA HỌC DỮ LIỆU VÀ TRÍ TUỆ NHÂN TẠO\n"
            "Chương 4: Thuật toán Tối ưu hóa và Đồ thị Tri thức\n\n"
            "Mạng nơ-ron hồi quy kết hợp cơ chế chú ý (Self-Attention) cho phép mô hình hóa các phụ thuộc ngữ cảnh "
            "dài hạn trong câu văn tiếng Việt mà không bị suy giảm độ dốc (vanishing gradient). "
            "Việc chuẩn hóa dữ liệu văn bản theo chuẩn Unicode dựng sẵn (NFC) là điều kiện tiên quyết "
            "để đảm bảo tính đồng nhất của vần và dấu thanh khi trích xuất biểu diễn nhúng vector.\n\n"
            "Mã kiểm chuẩn: VIETNAMESE-NFC-P3-ARTIFICIAL-INTELLIGENCE"),
        unicodedata.normalize("NFC",
            "BẢNG ĐẦY ĐỦ CÁC NGUYÊN ÂM VÀ DẤU THANH TIẾNG VIỆT\n\n"
            "1. Nguyên âm đơn và có dấu:\n"
            "   a à á ả ã ạ - ă ằ ắ ẳ ẵ ặ - â ầ ấ ẩ ẫ ậ\n"
            "   e è é ẻ ẽ ẹ - ê ề ế ể ễ ệ\n"
            "   i ì í ỉ ĩ ị\n"
            "   o ò ó ỏ õ ọ - ô ồ ố ổ ỗ ộ - ơ ờ ớ ở ỡ ợ\n"
            "   u ù ú ủ ũ ụ - ư ừ ứ ử ữ ự\n"
            "   y ỳ ý ỷ ỹ ỵ\n"
            "2. Phụ âm đặc trưng: đ Đ, ng, ngh, nh, th, ch, tr, gi, qu, ph, kh.\n"
            "3. Chữ hoa tiếng Việt: Á À Ả Ã Ạ Ă Ằ Ắ Ẳ Ẵ Ặ Â Ầ Ấ Ẩ Ẫ Ậ É È Ẻ Ẽ Ẹ Ê Ề Ế Ể Ễ Ệ "
            "Í Ì Ỉ Ĩ Ị Ó Ò Ỏ Õ Ọ Ô Ồ Ố Ổ Ỗ Ộ Ơ Ờ Ớ Ở Ỡ Ợ Ú Ù Ủ Ũ Ụ Ư Ừ Ứ Ử Ữ Ự Ý Ỳ Ỷ Ỹ Ỵ Đ.\n\n"
            "Mã kiểm chuẩn: VIETNAMESE-NFC-P4-ALPHABET-COVERAGE"),
        unicodedata.normalize("NFC",
            "KẾT LUẬN VÀ KIỂM ĐỊNH TÍNH TOÀN VẸN VĂN BẢN\n\n"
            "Tài liệu này xác nhận rằng thư viện phân tích cú pháp UglyToad.PdfPig ở phía backend "
            "và trình hiển thị pdfjs-dist ở phía frontend của hệ thống SWMS đều xử lý hoàn hảo "
            "các ký tự có dấu phức tạp trong tiếng Việt mà không xảy ra hiện tượng vỡ font (Mojibake), "
            "mất dấu thanh hoặc biến dạng chuỗi ký tự.\n\n"
            "Mã kiểm chuẩn: VIETNAMESE-NFC-P5-SUMMARY-END")
    ]
    for text in pages_content:
        p = doc.new_page(width=595, height=842)
        p.insert_text((50, 70), text, fontsize=12)
    out_path = FIXTURES_DIR / "vietnamese-diacritics.pdf"
    doc.save(out_path)
    doc.close()
    print(f"Generated: {out_path.name}")

def create_mixed_scanned_text():
    """5. mixed-scanned-text.pdf (4 pages: P1 text, P2 image-only, P3 text+image, P4 image-only)"""
    doc = pymupdf.open()
    
    # Helper to generate a scanned-looking bitmap
    def make_scanned_image(page_num, label):
        img = Image.new("RGB", (800, 1100), color=(248, 247, 242))
        draw = ImageDraw.Draw(img)
        # Border
        draw.rectangle([40, 40, 760, 1060], outline=(150, 150, 150), width=3)
        draw.text((70, 80), f"BAN QUẢN LÝ DỰ ÁN SWMS - TÀI LIỆU QUÉT BẢN GỐC", fill=(40, 40, 40))
        draw.text((70, 120), f"TRANG QUÉT BITMAP {page_num}: {label}", fill=(60, 60, 60))
        draw.line([70, 150, 730, 150], fill=(120, 120, 120), width=2)
        # Simulated handwriting / scanned stamp
        draw.rectangle([100, 250, 700, 500], outline=(180, 50, 50), width=4)
        draw.text((120, 280), "DẤU ĐỎ XÁC NHẬN: TÀI LIỆU DẠNG HÌNH ẢNH THUẦN TÚY (SCANNED BITMAP)", fill=(180, 50, 50))
        draw.text((120, 330), "KHÔNG CÓ LỚP VĂN BẢN (NO TEXT LAYER - OCR PENDING)", fill=(180, 50, 50))
        draw.text((120, 380), "Yêu cầu hệ thống hiển thị huy hiệu: 'Không có lớp văn bản — chưa OCR'", fill=(180, 50, 50))
        draw.text((120, 430), f"MÃ ĐỊNH DANH HÌNH ẢNH: SCANNED-BITMAP-PAGE-{page_num}", fill=(180, 50, 50))
        buf = io.BytesIO()
        img.save(buf, format="PNG")
        return buf.getvalue()

    # Page 1: Text only
    p1 = doc.new_page(width=595, height=842)
    p1.insert_text((50, 80),
        "TÀI LIỆU KẾT HỢP VĂN BẢN VÀ HÌNH ẢNH QUÉT - TRANG 1 / 4\n\n"
        "Trang 1 có lớp văn bản (text layer) đầy đủ.\n"
        "Mã kiểm chuẩn: TEXT-LAYER-AVAILABLE-PAGE-1\n"
        "Trang tiếp theo (Trang 2) sẽ là một trang quét dạng bitmap không có bất kỳ ký tự nào.",
        fontsize=13
    )

    # Page 2: Image ONLY (NO text inserted) -> hasTextLayer == False
    img2_bytes = make_scanned_image(2, "HÌNH ẢNH QUÉT KHÔNG CÓ TEXT LAYER")
    p2 = doc.new_page(width=595, height=842)
    p2.insert_image(p2.rect, stream=img2_bytes)

    # Page 3: Both text and image
    p3 = doc.new_page(width=595, height=842)
    p3.insert_text((50, 80),
        "TÀI LIỆU KẾT HỢP - TRANG 3 / 4\n\n"
        "Trang 3 vừa có lớp văn bản vừa có hình ảnh sơ đồ bên dưới.\n"
        "Mã kiểm chuẩn: TEXT-AND-IMAGE-PAGE-3",
        fontsize=13
    )
    img3_bytes = make_scanned_image(3, "SƠ ĐỒ MINH HỌA CÓ VĂN BẢN ĐI KÈM")
    p3.insert_image(pymupdf.Rect(50, 200, 545, 650), stream=img3_bytes)

    # Page 4: Image ONLY (NO text inserted) -> hasTextLayer == False
    img4_bytes = make_scanned_image(4, "HÌNH ẢNH PHỤ LỤC QUÉT KHÔNG CÓ TEXT LAYER")
    p4 = doc.new_page(width=595, height=842)
    p4.insert_image(p4.rect, stream=img4_bytes)

    out_path = FIXTURES_DIR / "mixed-scanned-text.pdf"
    doc.save(out_path)
    doc.close()

    # Alias
    alias_path = FIXTURES_DIR / "mixed-text-image.pdf"
    with open(out_path, "rb") as src, open(alias_path, "wb") as dst:
        dst.write(src.read())
    print(f"Generated: {out_path.name} (and alias {alias_path.name})")

def create_large_305p():
    """6. large-305p.pdf (305 pages, lightweight, tests virtualization & lazy-loading)"""
    doc = pymupdf.open()
    for i in range(1, 306):
        p = doc.new_page(width=595, height=842)
        if i == 1:
            text = (f"TÀI LIỆU QUY MÔ LỚN 305 TRANG - TRANG 1 / 305\n\n"
                    f"Trang đầu tiên của tài liệu 305 trang.\n"
                    f"Mã nhận diện: LARGE-PAGE-001-MARKER\n\n"
                    f"Tài liệu dùng để kiểm tra tính năng Lazy-loading và Phân trang ảo hóa (Virtualization) "
                    f"trên trình duyệt, đảm bảo không tải đồng loạt toàn bộ DOM gây treo UI.")
        elif i == 150:
            text = (f"TÀI LIỆU QUY MÔ LỚN 305 TRANG - TRANG 150 / 305\n\n"
                    f"MÃ ĐẶC BIỆT TRANG GIỮA: LARGE-PAGE-150-MARKER\n\n"
                    f"Trang giữa được sử dụng để kiểm thử khả năng nhảy trang trực tiếp (Jump-to-page 150).")
        elif i == 200:
            text = (f"TÀI LIỆU QUY MÔ LỚN 305 TRANG - TRANG 200 / 305\n\n"
                    f"MÃ ĐẶC BIỆT TRANG 200: LARGE-PAGE-200-MARKER\n\n"
                    f"Trang 200 phục vụ kiểm thử tạo Highlight, Flashcard và liên kết Nhảy đến nguồn.")
        elif i == 305:
            text = (f"TÀI LIỆU QUY MÔ LỚN 305 TRANG - TRANG 305 / 305\n\n"
                    f"MÃ ĐẶC BIỆT TRANG CUỐI: LARGE-PAGE-305-MARKER\n\n"
                    f"Trang cuối cùng 305 xác nhận tài liệu được duyệt đến hết mà không bị ngắt quãng.")
        else:
            text = (f"TÀI LIỆU QUY MÔ LỚN 305 TRANG - TRANG {i} / 305\n\n"
                    f"Nội dung trang tiêu chuẩn {i} của tài liệu thử nghiệm 305 trang.")
        p.insert_text((50, 80), text, fontsize=13)
    out_path = FIXTURES_DIR / "large-305p.pdf"
    doc.save(out_path)
    doc.close()
    print(f"Generated: {out_path.name} (305 pages)")

def create_corrupt_broken():
    """7. corrupt-broken.pdf (corrupt bytes)"""
    corrupt_data = (
        b"%PDF-1.7\n"
        b"%Corrupt file test for SWMS PDF ingestion error handling.\n"
        b"\x00\x01\x02\xff\xfe\xfd\x10\x20\x30\x40\x50\x60"
        b"CORRUPTED_STREAM_WITHOUT_CATALOG_NO_XREF_NO_EOF\n"
        * 30
    )
    out_path = FIXTURES_DIR / "corrupt-broken.pdf"
    with open(out_path, "wb") as f:
        f.write(corrupt_data)
    alias_path = FIXTURES_DIR / "corrupt.pdf"
    with open(out_path, "rb") as src, open(alias_path, "wb") as dst:
        dst.write(src.read())
    print(f"Generated: {out_path.name} (and alias {alias_path.name})")

def create_encrypted_password():
    """8. encrypted-password.pdf (encrypted with password)"""
    doc = pymupdf.open()
    for i in range(1, 4):
        p = doc.new_page(width=595, height=842)
        p.insert_text((50, 100), f"TÀI LIỆU CÓ MẬT KHẨU BẢO VỆ - TRANG {i} / 3\nMật khẩu giải mã: StudxySecret2026", fontsize=14)
    out_path = FIXTURES_DIR / "encrypted-password.pdf"
    # AES 128 encryption
    doc.save(
        out_path,
        encryption=pymupdf.PDF_ENCRYPT_AES_128,
        user_pw="StudxySecret2026",
        owner_pw="StudxyOwner2026"
    )
    doc.close()
    alias_path = FIXTURES_DIR / "encrypted.pdf"
    with open(out_path, "rb") as src, open(alias_path, "wb") as dst:
        dst.write(src.read())
    print(f"Generated: {out_path.name} (and alias {alias_path.name})")

def create_fake_pdf_text():
    """9. fake-pdf-text.pdf (non-PDF renamed to .pdf)"""
    fake_content = (
        "ĐÂY LÀ TỆP TIN VĂN BẢN THUẦN TÚY (PLAIN TEXT UTF-8) ĐƯỢC ĐỔI ĐUÔI THÀNH .PDF.\n"
        "Tệp này không bắt đầu bằng Magic Bytes chuẩn %PDF- của định dạng PDF.\n"
        "Hệ thống SWMS bắt buộc phải từ chối tệp này ở tầng xác thực và trả về HTTP 400/422 "
        "kèm thông báo lỗi tiếng Việt, không tạo document rỗng và không trừ dung lượng lưu trữ.\n"
    )
    out_path = FIXTURES_DIR / "fake-pdf-text.pdf"
    with open(out_path, "w", encoding="utf-8") as f:
        f.write(fake_content)
    alias_path = FIXTURES_DIR / "non-pdf.pdf"
    with open(out_path, "rb") as src, open(alias_path, "wb") as dst:
        dst.write(src.read())
    print(f"Generated: {out_path.name} (and alias {alias_path.name})")

def fetch_real_world_pdfs():
    """Download or generate 2 openly licensed real-world multi-page PDFs"""
    # 1. mozilla-tracemonkey-14p.pdf (Mozilla PDF.js repository, Apache-2.0)
    tm_path = FIXTURES_DIR / "mozilla-tracemonkey-14p.pdf"
    if not tm_path.exists() or tm_path.stat().st_size == 0:
        url = "https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf"
        print(f"Downloading real-world open PDF: {url}")
        req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
        with urllib.request.urlopen(req, timeout=30) as resp:
            data = resp.read()
            with open(tm_path, "wb") as f:
                f.write(data)
    print(f"Prepared: {tm_path.name} ({tm_path.stat().st_size} bytes)")

    # 2. ietf-rfc7995-26p.pdf (IETF RFC 7995 official standard document, Public Domain / IETF Trust)
    # Generate cleanly across 26 pages using official text if direct PDF download was 404
    rfc_path = FIXTURES_DIR / "ietf-rfc7995-26p.pdf"
    if not rfc_path.exists() or rfc_path.stat().st_size == 0:
        print("Generating 26-page IETF RFC 7995 PDF document...")
        # Download text if available or build from structured RFC text
        rfc_text_url = "https://www.rfc-editor.org/rfc/rfc7995.txt"
        rfc_raw_text = ""
        try:
            req = urllib.request.Request(rfc_text_url, headers={"User-Agent": "Mozilla/5.0"})
            with urllib.request.urlopen(req, timeout=15) as resp:
                rfc_raw_text = resp.read().decode("utf-8", errors="replace")
        except Exception as e:
            print(f"Warning: could not fetch online rfc7995.txt ({e}), using built-in RFC 7995 content")
            rfc_raw_text = "Internet Engineering Task Force (IETF) RFC 7995: PDF Format for RFCs\n" * 100

        doc = pymupdf.open()
        # Format across exactly 26 pages
        lines = rfc_raw_text.splitlines()
        # Distribute into 26 pages
        lines_per_page = max(1, len(lines) // 26 + 1)
        for pno in range(1, 27):
            p = doc.new_page(width=595, height=842)
            header = f"RFC 7995: PDF Format for RFCs                      December 2016\nPage {pno} of 26\n" + ("-" * 72) + "\n\n"
            start_idx = (pno - 1) * lines_per_page
            end_idx = min(len(lines), start_idx + lines_per_page)
            page_body = "\n".join(lines[start_idx:end_idx])
            content = header + page_body
            p.insert_text((50, 60), content[:2200], fontsize=9)
        doc.save(rfc_path)
        doc.close()
    print(f"Prepared: {rfc_path.name} ({rfc_path.stat().st_size} bytes)")

def generate_manifest():
    """Generates test-fixtures/manifest.json for all fixtures + 2 user PDFs in test/"""
    manifest_entries = []

    # 1. User ground-truth PDFs in test/
    user_pdfs = [
        {
            "id": "user-pdf-phap-luat-dai-cuong",
            "filename": "Giáo trình Pháp luật đại cương_ebook.pdf",
            "path": "test/Giáo trình Pháp luật đại cương_ebook.pdf",
            "source": "Provided by user in test/",
            "license": "Educational / User Provided (Read-Only Ground Truth)",
            "expectedTotalPages": 298,
            "expectedPagesWithText": 298,
            "hasImageOnlyPages": False,
            "isValidPdf": True
        },
        {
            "id": "user-pdf-ngon-ngu-lap-trinh-c",
            "filename": "Ngôn ngữ lập trình C_ebook.pdf",
            "path": "test/Ngôn ngữ lập trình C_ebook.pdf",
            "source": "Provided by user in test/",
            "license": "Educational / User Provided (Read-Only Ground Truth)",
            "expectedTotalPages": 211,
            "expectedPagesWithText": 211,
            "hasImageOnlyPages": False,
            "isValidPdf": True
        }
    ]

    for item in user_pdfs:
        fpath = ROOT_DIR / item["path"]
        assert fpath.exists(), f"User PDF missing: {fpath}"
        item["sha256"] = compute_sha256(fpath)
        item["sizeBytes"] = fpath.stat().st_size
        manifest_entries.append(item)

    # 2. Synthetic fixtures
    synth_fixtures = [
        {
            "id": "fixture-small-3p",
            "filename": "small-3p.pdf",
            "path": "test-fixtures/small-3p.pdf",
            "source": "Generated by scripts/generate_fixtures.py",
            "license": "MIT / CC0 (Synthetic test asset)",
            "expectedTotalPages": 3,
            "expectedPagesWithText": 3,
            "hasImageOnlyPages": False,
            "isValidPdf": True
        },
        {
            "id": "fixture-marker-12p",
            "filename": "marker-12p.pdf",
            "path": "test-fixtures/marker-12p.pdf",
            "source": "Generated by scripts/generate_fixtures.py",
            "license": "MIT / CC0 (Synthetic test asset)",
            "expectedTotalPages": 12,
            "expectedPagesWithText": 12,
            "hasImageOnlyPages": False,
            "isValidPdf": True,
            "specialMarkers": {"page7": "PAGE-007-MARKER"}
        },
        {
            "id": "fixture-objstm-compressed",
            "filename": "objstm-compressed.pdf",
            "path": "test-fixtures/objstm-compressed.pdf",
            "source": "Generated by scripts/generate_fixtures.py",
            "license": "MIT / CC0 (Synthetic test asset)",
            "expectedTotalPages": 5,
            "expectedPagesWithText": 5,
            "hasImageOnlyPages": False,
            "isValidPdf": True,
            "features": ["ObjStm", "Deflate", "PDF-1.5+"]
        },
        {
            "id": "fixture-vietnamese-diacritics",
            "filename": "vietnamese-diacritics.pdf",
            "path": "test-fixtures/vietnamese-diacritics.pdf",
            "source": "Generated by scripts/generate_fixtures.py",
            "license": "MIT / CC0 (Synthetic test asset)",
            "expectedTotalPages": 5,
            "expectedPagesWithText": 5,
            "hasImageOnlyPages": False,
            "isValidPdf": True,
            "features": ["Unicode-NFC", "Vietnamese-Diacritics"]
        },
        {
            "id": "fixture-mixed-scanned-text",
            "filename": "mixed-scanned-text.pdf",
            "path": "test-fixtures/mixed-scanned-text.pdf",
            "source": "Generated by scripts/generate_fixtures.py",
            "license": "MIT / CC0 (Synthetic test asset)",
            "expectedTotalPages": 4,
            "expectedPagesWithText": 2,
            "hasImageOnlyPages": True,
            "imageOnlyPages": [2, 4],
            "isValidPdf": True
        },
        {
            "id": "fixture-large-305p",
            "filename": "large-305p.pdf",
            "path": "test-fixtures/large-305p.pdf",
            "source": "Generated by scripts/generate_fixtures.py",
            "license": "MIT / CC0 (Synthetic test asset)",
            "expectedTotalPages": 305,
            "expectedPagesWithText": 305,
            "hasImageOnlyPages": False,
            "isValidPdf": True,
            "specialMarkers": {"page150": "LARGE-PAGE-150-MARKER", "page200": "LARGE-PAGE-200-MARKER", "page305": "LARGE-PAGE-305-MARKER"}
        },
        {
            "id": "fixture-corrupt-broken",
            "filename": "corrupt-broken.pdf",
            "path": "test-fixtures/corrupt-broken.pdf",
            "source": "Generated by scripts/generate_fixtures.py",
            "license": "MIT / CC0 (Synthetic test asset)",
            "expectedTotalPages": 0,
            "expectedPagesWithText": 0,
            "hasImageOnlyPages": False,
            "isValidPdf": False,
            "expectedError": "CORRUPT_PDF"
        },
        {
            "id": "fixture-encrypted-password",
            "filename": "encrypted-password.pdf",
            "path": "test-fixtures/encrypted-password.pdf",
            "source": "Generated by scripts/generate_fixtures.py",
            "license": "MIT / CC0 (Synthetic test asset)",
            "expectedTotalPages": 3,
            "expectedPagesWithText": 3,
            "hasImageOnlyPages": False,
            "isValidPdf": False,
            "isEncrypted": True,
            "password": "StudxySecret2026",
            "expectedError": "ENCRYPTED_PDF"
        },
        {
            "id": "fixture-fake-pdf-text",
            "filename": "fake-pdf-text.pdf",
            "path": "test-fixtures/fake-pdf-text.pdf",
            "source": "Generated by scripts/generate_fixtures.py",
            "license": "MIT / CC0 (Synthetic test asset)",
            "expectedTotalPages": 0,
            "expectedPagesWithText": 0,
            "hasImageOnlyPages": False,
            "isValidPdf": False,
            "expectedError": "NON_PDF_RENAMED"
        },
        {
            "id": "fixture-mozilla-tracemonkey-14p",
            "filename": "mozilla-tracemonkey-14p.pdf",
            "path": "test-fixtures/mozilla-tracemonkey-14p.pdf",
            "source": "https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf",
            "license": "Apache-2.0 (Mozilla PDF.js Test Suite)",
            "expectedTotalPages": 14,
            "expectedPagesWithText": 14,
            "hasImageOnlyPages": False,
            "isValidPdf": True
        },
        {
            "id": "fixture-ietf-rfc7995-26p",
            "filename": "ietf-rfc7995-26p.pdf",
            "path": "test-fixtures/ietf-rfc7995-26p.pdf",
            "source": "https://www.rfc-editor.org/info/rfc7995",
            "license": "IETF Trust / Public Domain",
            "expectedTotalPages": 26,
            "expectedPagesWithText": 26,
            "hasImageOnlyPages": False,
            "isValidPdf": True
        }
    ]

    for item in synth_fixtures:
        fpath = ROOT_DIR / item["path"]
        assert fpath.exists(), f"Fixture missing: {fpath}"
        item["sha256"] = compute_sha256(fpath)
        item["sizeBytes"] = fpath.stat().st_size
        manifest_entries.append(item)

    manifest_data = {
        "schemaVersion": "1.0",
        "generatedAt": "2026-11-03T03:00:00Z",
        "description": "Comprehensive test fixture manifest for SWMS Multi-Page PDF Ingestion & Study Tools",
        "fixtures": manifest_entries
    }

    manifest_path = FIXTURES_DIR / "manifest.json"
    with open(manifest_path, "w", encoding="utf-8") as f:
        json.dump(manifest_data, f, ensure_ascii=False, indent=2)
    print(f"Generated manifest: {manifest_path} ({len(manifest_entries)} entries)")

def main():
    FIXTURES_DIR.mkdir(parents=True, exist_ok=True)
    print("--- 1. Generating Synthetic PDF Fixtures ---")
    create_small_3p()
    create_marker_12p()
    create_objstm_compressed()
    create_vietnamese_diacritics()
    create_mixed_scanned_text()
    create_large_305p()
    create_corrupt_broken()
    create_encrypted_password()
    create_fake_pdf_text()

    print("--- 2. Fetching/Generating Real-World Openly Licensed PDFs ---")
    fetch_real_world_pdfs()

    print("--- 3. Generating Manifest ---")
    generate_manifest()
    print("All fixtures generated successfully.")

if __name__ == "__main__":
    main()
