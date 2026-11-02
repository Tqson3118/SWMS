#!/usr/bin/env python3
"""
scripts/generate_oracle.py
Extracts independent ground truth using PyMuPDF (fitz) into test-fixtures/oracle.json.
This oracle serves as the authoritative source of truth for both C# backend integration tests
and Playwright frontend E2E acceptance tests.
"""

import sys
import json
import hashlib
import unicodedata
from pathlib import Path
import pymupdf

# Ensure UTF-8 output on Windows
if sys.stdout.encoding != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')

ROOT_DIR = Path(__file__).resolve().parent.parent
MANIFEST_PATH = ROOT_DIR / "test-fixtures" / "manifest.json"
ORACLE_PATH = ROOT_DIR / "test-fixtures" / "oracle.json"

def compute_sha256(filepath: Path) -> str:
    sha = hashlib.sha256()
    with open(filepath, "rb") as f:
        while chunk := f.read(65536):
            sha.update(chunk)
    return sha.hexdigest()

def extract_oracle_for_file(rel_path: str, meta: dict) -> dict:
    fpath = ROOT_DIR / rel_path
    if not fpath.exists():
        raise FileNotFoundError(f"File not found: {fpath}")

    sha256 = compute_sha256(fpath)
    file_size = fpath.stat().st_size

    # Handle non-valid PDFs (corrupt, encrypted, fake)
    if not meta.get("isValidPdf", True):
        return {
            "path": rel_path.replace("\\", "/"),
            "filename": fpath.name,
            "sha256": sha256,
            "sizeBytes": file_size,
            "isValidPdf": False,
            "expectedError": meta.get("expectedError", "INVALID_PDF"),
            "isEncrypted": meta.get("isEncrypted", False),
            "totalPages": 0,
            "pagesWithText": 0,
            "pages": {}
        }

    try:
        doc = pymupdf.open(fpath)
    except Exception as e:
        return {
            "path": rel_path.replace("\\", "/"),
            "filename": fpath.name,
            "sha256": sha256,
            "sizeBytes": file_size,
            "isValidPdf": False,
            "expectedError": str(e),
            "totalPages": 0,
            "pagesWithText": 0,
            "pages": {}
        }

    total_pages = len(doc)
    pages_dict = {}
    pages_with_text = 0

    for idx, page in enumerate(doc):
        page_num = idx + 1
        raw_text = page.get_text()
        nfc_text = unicodedata.normalize("NFC", raw_text)
        stripped = nfc_text.strip()
        has_text = len(stripped) > 0
        if has_text:
            pages_with_text += 1

        words = [w for w in nfc_text.split() if w]
        word_count = len(words)
        char_count = len(nfc_text)

        # Snippets for assertions
        cleaned_single_line = " ".join(nfc_text.split())
        first_50 = cleaned_single_line[:50]
        first_100 = cleaned_single_line[:100]

        # Extract sample quote if available
        sample_quote = ""
        if len(words) >= 5:
            sample_quote = " ".join(words[:12])
        elif words:
            sample_quote = " ".join(words)

        pages_dict[str(page_num)] = {
            "pageNumber": page_num,
            "hasTextLayer": has_text,
            "charCount": char_count,
            "characterCount": char_count,
            "wordCount": word_count,
            "first50Chars": first_50,
            "first100Chars": first_100,
            "sampleQuote": sample_quote
        }

    doc.close()

    return {
        "path": rel_path.replace("\\", "/"),
        "filename": fpath.name,
        "sha256": sha256,
        "sizeBytes": file_size,
        "isValidPdf": True,
        "totalPages": total_pages,
        "pagesWithText": pages_with_text,
        "hasImageOnlyPages": (pages_with_text < total_pages),
        "pages": pages_dict
    }

def main():
    if not MANIFEST_PATH.exists():
        print(f"Error: {MANIFEST_PATH} does not exist. Run scripts/generate_fixtures.py first.")
        sys.exit(1)

    with open(MANIFEST_PATH, "r", encoding="utf-8") as f:
        manifest = json.load(f)

    oracle = {
        "schemaVersion": "1.0",
        "generatedAt": "2026-11-03T03:00:00Z",
        "generator": "scripts/generate_oracle.py (PyMuPDF)",
        "documents": {}
    }

    print("Generating independent oracle from manifest fixtures...")
    for item in manifest.get("fixtures", []):
        rel_path = item["path"]
        print(f"  Extracting ground truth for: {rel_path} ...", end="", flush=True)
        doc_oracle = extract_oracle_for_file(rel_path, item)
        # Store under both relative path and id for convenient lookup
        normalized_key = rel_path.replace("\\", "/")
        oracle["documents"][normalized_key] = doc_oracle
        # Also store top-level key for direct path indexing as expected by tests
        oracle[normalized_key] = doc_oracle
        print(f" Done ({doc_oracle['totalPages']} pages, {doc_oracle['pagesWithText']} with text)")

    with open(ORACLE_PATH, "w", encoding="utf-8") as f:
        json.dump(oracle, f, ensure_ascii=False, indent=2)

    print(f"\nOracle written to: {ORACLE_PATH} ({len(oracle['documents'])} documents indexed)")

if __name__ == "__main__":
    main()
