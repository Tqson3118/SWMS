# Project: SWMS Multi-Page PDF Ingestion, Streaming & Study Tools

## Architecture
- **Backend (.NET 10 Minimal API at `backend/src/Swms.Api`)**:
  - Ingestion: `POST /api/v1/documents/upload` accepts `multipart/form-data` with binary PDF payload (and legacy JSON fallback for error simulation). Uses `UglyToad.PdfPig` (0.1.16) for server-side parsing.
  - Binary Storage & Streaming: Local file storage (`Storage:LocalStoragePath`). `GET /api/v1/documents/{id}/file` serves `application/pdf` with `enableRangeProcessing: true` (HTTP 206) for efficient chunked browser streaming.
  - Relational Schema: `DocumentPage` entity mapped to `document_pages` table in `SwmsDbContext` and `database/schema.sql` (compatible with `SwmsArchitectureTests.cs`). Fields: `DocumentId`, `PageNumber`, `PlainText` (Unicode NFC), `HasTextLayer`, character/word counts.
  - Per-Page Retrieval: `GET /api/v1/documents/{id}/pages` and `GET /api/v1/documents/{id}/pages/{pageNumber}`.
  - Annotation Integrity: Enforce `chk_annotation_target_exclusive` on `POST /api/v1/annotations` (`DocumentId != null && PageNumber > 0 && LessonId == null && BlockId == null`).
- **Frontend (Next.js 15 at `frontend`)**:
  - Ingestion UI: `PersonalWorkspaceDashboard.tsx` removes `estimatePdfPages()` and fake text generation; uploads real binary via `FormData`.
  - PDF Rendering Engine: `pdfjs-dist@^4.10.38` with offline worker in `frontend/public/pdf.worker.min.mjs`.
  - Dual-Layer Reader: `ReaderAndSplitViews.tsx` renders visual PDF canvas from `GET /api/v1/documents/{id}/file` and per-page text layer from `GET /api/v1/documents/{id}/pages/{pageNumber}` for text selection and annotations.
  - Navigation & Virtualization: Previous/next buttons, numeric page input, keyboard navigation, `"Trang X / Y"` indicator (`data-testid="pdf-page-indicator"`), lazy loading up to 300 pages.
  - No Text Layer Badge: Badge `"Không có lớp văn bản — chưa OCR"` (`data-testid="no-text-layer-badge"`) when `HasTextLayer === false`.
  - Page-Aware Annotations: Fix `anchoredAnnotations.filter` to check `ann.documentId === activeDocumentId && ann.pageNumber === currentPage` for documents. Real prefix/suffix quotes and offsets.
  - Page-Aware Flashcards & Citations: Store `pageNumber` and `ref_document_id`.
  - Jump-to-Source: "Nhảy đến nguồn" (`data-testid="jump-to-source-btn"`) in Mode 2 Notebook and Mode 3 Review switches to document, jumps reader to page $N$, and pulses the highlight.
- **Testing & Automated Verification**:
  - Fixtures & Manifest: `test-fixtures/` with `manifest.json`, generated synthetic PDFs, and 2 real user PDFs in `test/`.
  - Independent Oracle: Python script (`scripts/generate_oracle.py`) using `pymupdf` to generate `test-fixtures/oracle.json`.
  - Backend Tests: Unit tests for `PdfPigExtractionService` and integration tests with `WebApplicationFactory`.
  - Acceptance E2E Spec: `frontend/e2e/pdf-multipage-e2e.spec.ts` driving real Chrome with real files, canvas pixel assertions, highlight persistence, and jump-to-source.
  - Verification Runner: `scripts/verify.ps1` with health checks, log collection in `test-results/verification/<timestamp>/`, and 2 consecutive zero exit codes.

## Feature Inventory
| # | Feature | Description | Milestone | Source |
|---|---------|-------------|-----------|--------|
| 1 | Git Baseline & Clean Repo | Run `git init`, configure `.gitignore`, commit baseline before any code changes | M0 | Survey |
| 2 | Deterministic Fixtures & Manifest | Create `scripts/generate_fixtures.py`, `test-fixtures/manifest.json`, small multi-page, compressed, diacritics, scanned, corrupt, encrypted | M0 | Survey |
| 3 | Independent Oracle Script | Create `scripts/generate_oracle.py` producing `test-fixtures/oracle.json` with ground truth page counts & snippets | M0 | Survey |
| 4 | PdfPig Integration & Service | Add `UglyToad.PdfPig` 0.1.16 to `backend/src/Swms.Api/Swms.Api.csproj`, build `PdfExtractionService` | M1 | Survey |
| 5 | Relational DocumentPage Schema | Add `DocumentPage` entity, update `SwmsDbContext`, update `database/schema.sql` and pass `SwmsArchitectureTests.cs` | M1 | Survey |
| 6 | Binary Storage & Streaming Endpoint | Save uploaded PDF to configurable local folder; serve via `GET /api/v1/documents/{id}/file` with range processing (HTTP 206) | M1 | Survey |
| 7 | Multipart Upload Endpoint | Update `POST /api/v1/documents/upload` to handle `multipart/form-data` with binary PDF parsing, keep legacy JSON fallback | M1 | Survey |
| 8 | Per-Page Endpoints | Implement `GET /api/v1/documents/{id}/pages` and `GET /api/v1/documents/{id}/pages/{pageNumber}` | M1 | Survey |
| 9 | Quota & Error Integrity | Return HTTP 400/422 with clear Vietnamese messages on corrupt/encrypted/non-PDF files; 0 quota deducted; no fake AST | M1 | Survey |
| 10 | Backend Tests & WebApplicationFactory | Unit tests for PdfPig service and integration tests for upload, streaming, pages, and error handling | M1 | Survey |
| 11 | Frontend pdfjs-dist & Offline Worker | Install `pdfjs-dist` in `frontend/package.json`, configure offline worker in `frontend/public/` | M2 | Survey |
| 12 | Frontend Real Upload Flow | Refactor `PersonalWorkspaceDashboard.tsx` to send `FormData` with real file, delete `estimatePdfPages` and fake text | M2 | Survey |
| 13 | Dual-Layer PDF Reader | Render PDF canvas from `/file` endpoint and extracted text layer from `/pages/{pageNumber}` in Mode 1 & Mode 2 | M2 | Survey |
| 14 | Page Navigation & Virtualization | Add prev/next, page input, `"Trang X / Y"` indicator (`data-testid="pdf-page-indicator"`), lazy load pages up to 300 | M2 | Survey |
| 15 | No Text Layer Badge | Render visual canvas and display badge `"Không có lớp văn bản — chưa OCR"` (`data-testid="no-text-layer-badge"`) when `HasTextLayer == false` | M2 | Survey |
| 16 | Dynamic Header, TOC & Resource Tree | Dynamic reader header, dynamic reading progress `Math.round(currentPage/totalPages*100)%`, dynamic Mode 2 document tree | M2 | Survey |
| 17 | Annotation Constraint & Filter Fix | Fix `ReaderAndSplitViews.tsx` filter to `ann.documentId === activeDocumentId && ann.pageNumber === currentPage`; real quotes and offsets | M3 | Survey |
| 18 | Page-Aware Flashcards & Citations | Store `pageNumber: N` and `ref_document_id` in flashcards and notebook citations; eliminate hardcoded MMU/TLB quotes | M3 | Survey |
| 19 | "Nhảy đến nguồn" Jump-to-Source | Implement `data-testid="jump-to-source-btn"` in Mode 2 and Mode 3 to switch document, navigate reader to page $N$, and pulse quote | M3 | Survey |
| 20 | Red-Before-Green Baseline Evidence | Run `pdf-multipage-e2e.spec.ts` against baseline commit, capture `test-results/verification/baseline-red.log` proving detection of facade | M4 | Survey |
| 21 | Playwright Real Acceptance E2E Spec | Implement `frontend/e2e/pdf-multipage-e2e.spec.ts` (strictly NO MOCKS, real `setInputFiles`, canvas pixel check, page 200 highlight & flashcard) | M4 | Survey |
| 22 | Automated Verification Runner | Create `scripts/verify.ps1`, verify 2 consecutive clean runs with exit code 0 | M4 | Survey |
| 23 | Regression Verification & Handover | Pass all existing architecture tests and positive E2E specs; deliver independent verification sign-off and Vietnamese handover report | M4 | Survey |

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M0 | Git Baseline & Test Fixtures/Oracle Setup | Git baseline commit, `generate_fixtures.py`, `generate_oracle.py`, `test-fixtures/manifest.json`, `oracle.json` | none | PLANNED |
| M1 | Backend PDF Ingestion, Streaming & Page Storage | PdfPig integration, `DocumentPage` schema & DDL, `multipart/form-data` upload, `/file` streaming, `/pages` endpoints, error handling, backend tests | M0 | PLANNED |
| M2 | Frontend Dual-Layer PDF Reader & Navigation | `pdfjs-dist` setup, real upload, dual canvas+text rendering, `"Trang X / Y"`, no-text-layer badge, dynamic TOC/tree | M1 | PLANNED |
| M3 | Page-Aware Annotations, Flashcards & Jump-to-Source | Annotation filter fix (`pageNumber`), real quote selection, flashcards/citations with `pageNumber`, "Nhảy đến nguồn" direct page jump & pulse | M2 | PLANNED |
| M4 | Dual-Track E2E & Full Verification | Red-before-green log, `pdf-multipage-e2e.spec.ts`, `scripts/verify.ps1` 2x exit 0, regression test pass, independent audit & handover report | M3 | PLANNED |

## Interface Contracts
### Client ↔ Ingestion Endpoint
- `POST /api/v1/documents/upload`
  - Content-Type: `multipart/form-data`
  - Form Fields: `file: File`, `title?: string`, `workspaceId?: string`
  - Response (HTTP 200):
    ```json
    {
      "id": "guid",
      "title": "string",
      "totalPages": 298,
      "fileSizeBytes": 4991174,
      "status": "completed",
      "ocrStatus": "not_applicable",
      "createdAt": "iso-date",
      "pages": [
        { "pageNumber": 1, "hasTextLayer": true, "characterCount": 420 }
      ]
    }
    ```
  - Error Response (HTTP 400/422):
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
      "title": "Tệp PDF không hợp lệ",
      "status": 400,
      "detail": "Tệp tin bị hỏng hoặc có mật khẩu bảo vệ..."
    }
    ```

### Client ↔ Binary Streaming Endpoint
- `GET /api/v1/documents/{id}/file`
  - Headers: `Range: bytes=0-65535` (optional)
  - Response: HTTP 200 / HTTP 206 Partial Content
  - Content-Type: `application/pdf`
  - Headers: `Accept-Ranges: bytes`, `Content-Length`, `Content-Range`

### Client ↔ Per-Page Data Endpoint
- `GET /api/v1/documents/{id}/pages/{pageNumber}`
  - Response (HTTP 200):
    ```json
    {
      "documentId": "guid",
      "pageNumber": 150,
      "plainText": "string content in Unicode NFC",
      "hasTextLayer": true,
      "characterCount": 1240,
      "wordCount": 195
    }
    ```

### Client ↔ Annotation Endpoint
- `POST /api/v1/annotations`
  - Request Body:
    ```json
    {
      "documentId": "guid",
      "pageNumber": 200,
      "lessonId": null,
      "blockId": null,
      "exactQuote": "string",
      "prefixQuote": "string",
      "suffixQuote": "string",
      "color": "yellow",
      "startOffset": 12,
      "endOffset": 45
    }
    ```
  - Validation: Must satisfy `chk_annotation_target_exclusive`.

## Code Layout
- `backend/src/Swms.Api/`
  - `Domain/Entities/SwmsEntities.cs`: Add `DocumentPage` entity
  - `Infrastructure/Database/SwmsDbContext.cs`: Map `DocumentPage` to `document_pages` table
  - `Features/Pdf/PdfExtractionService.cs`: UglyToad.PdfPig extraction service
  - `Features/SwmsEndpoints.cs`: Update upload, binary streaming `/file`, `/pages` endpoints
- `database/schema.sql`: Add `document_pages` table DDL
- `frontend/`
  - `package.json`: Add `pdfjs-dist@^4.10.38`
  - `public/pdf.worker.min.mjs`: Offline worker script
  - `src/components/dashboard/PersonalWorkspaceDashboard.tsx`: Multipart upload, remove fake text
  - `src/components/reader/ReaderAndSplitViews.tsx`: Dual-layer viewer, navigation, page-aware annotations
  - `src/components/review/ReviewAndPracticeView.tsx`: "Nhảy đến nguồn" jump-to-source
  - `src/store/useWorkspaceStore.ts`: Dynamic active title & current document page
- `scripts/`
  - `generate_fixtures.py`: Synthetic test fixture generator
  - `generate_oracle.py`: Ground-truth oracle generator using PyMuPDF
  - `verify.ps1`: One-command automated build, run, and test verification
- `test-fixtures/`: Fixtures and manifest
- `frontend/e2e/pdf-multipage-e2e.spec.ts`: Real Chrome E2E acceptance tests
