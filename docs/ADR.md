# SWMS: ARCHITECTURE DECISION RECORDS (ADR)

> **Document Type**: Architecture Decision Record (ADR) Repository  
> **Format**: Markdown Architecture Decision Records (MADR) / Michael Nygard Standard  
> **Project**: Study Workspace Management System (SWMS)  
> **Repository Path**: `docs/ADR.md`  
> **Version**: 2.2.0 (Synchronized Baseline with Master Prompt v2.2.0 & Product Review Amendments)  
> **Status**: Accepted & Authoritative Baseline  
> **Scope**: Resolutions for 7 Architectural Bottlenecks (R1), 4 Open Design Decisions (R2), Workspace UI/UX Layout Architecture (R4), Feature Expansion Roadmap (R5), and Product Review Strategic Realignment (ADR-019 through ADR-022).

---

## TABLE OF CONTENTS

- [ADR-001: Standardization of ProseMirror JSON AST and Leaf-Node Inline Marks over Offset-Based Annotations](#adr-001-standardization-of-prosemirror-json-ast-and-leaf-node-inline-marks-over-offset-based-annotations) *(Amended)*
- [ADR-002: Single JSONB Document Storage with Asynchronous Link Extraction for Notebooks over Normalized Blocks](#adr-002-single-jsonb-document-storage-with-asynchronous-link-extraction-for-notebooks-over-normalized-blocks) *(Amended)*
- [ADR-003: Refresh Token Rotation (RTR) with Token Families and User Token Versioning for JWT Security](#adr-003-refresh-token-rotation-rtr-with-token-families-and-user-token-versioning-for-jwt-security)
- [ADR-004: Foreign Key Cascading Strategy and Knowledge Preservation via ON DELETE SET NULL for Flashcards](#adr-004-foreign-key-cascading-strategy-and-knowledge-preservation-via-on-delete-set-null-for-flashcards) *(Amended)*
- [ADR-005: Migration from pgvector IVFFlat to HNSW Indexing for Dynamic and Cold-Start Vector Embeddings](#adr-005-migration-from-pgvector-ivfflat-to-hnsw-indexing-for-dynamic-and-cold-start-vector-embeddings) *(Amended)*
- [ADR-006: Dedicated Exam Attempts Entity Grouping for Chapter and Course Assessments](#adr-006-dedicated-exam-attempts-entity-grouping-for-chapter-and-course-assessments) *(Amended)*
- [ADR-007: Resizable Multi-Pane Responsive Layout for Standard 1920x1080 Baseline](#adr-007-resizable-multi-pane-responsive-layout-for-standard-1920x1080-baseline) *(Amended)*
- [ADR-008: Server-Side Three-Tier Fuzzy Matching for Self-Healing Annotation Offsets on Lesson Updates](#adr-008-server-side-three-tier-fuzzy-matching-for-self-healing-annotation-offsets-on-lesson-updates) *(Amended)*
- [ADR-009: Asynchronous Background Job Processing via .NET 10 Bounded Channels and Real-Time Notification for AI Tasks](#adr-009-asynchronous-background-job-processing-via-net-10-bounded-channels-and-real-time-notification-for-ai-tasks) *(Amended)*
- [ADR-010: Hybrid Client-Side FSRS v5 Evaluation with Batch Server Synchronization](#adr-010-hybrid-client-side-fsrs-v5-evaluation-with-batch-server-synchronization) *(Amended)*
- [ADR-011: Three-Mode Adaptive Workspace UX Architecture (Reading, Split, Review) with GSAP Micro-Interactions](#adr-011-three-mode-adaptive-workspace-ux-architecture-reading-split-review-with-gsap-micro-interactions) *(Amended)*
- [ADR-012: Core PKM Feature Expansion and Phased Delivery Roadmap](#adr-012-core-pkm-feature-expansion-and-phased-delivery-roadmap) *(Amended)*
- [ADR-013: Best-of-Breed Educational Workspace Synthesis over Single-Platform Emulation](#adr-013-best-of-breed-educational-workspace-synthesis-over-single-platform-emulation) *(Amended)*
- [ADR-014: Multi-Tenant Vector Search Security, PDF Spatial Metadata, and Learner-Specific FSRS 19-Parameter Persistence](#adr-014-multi-tenant-vector-search-security-pdf-spatial-metadata-and-learner-specific-fsrs-19-parameter-persistence) *(Amended)*
- [ADR-015: User Flow, Navigation Topology, and Contextual View Interlinking Architecture](#adr-015-user-flow-navigation-topology-and-contextual-view-interlinking-architecture) *(Amended)*
- [ADR-016: BFF Cookie-Based Session Architecture and Encrypted Refresh Token Rotation](#adr-016-bff-cookie-based-session-architecture-and-encrypted-refresh-token-rotation)
- [ADR-017: Transactional Database-Backed Background Job Queue with Local In-Memory Dispatch](#adr-017-transactional-database-backed-background-job-queue-with-local-in-memory-dispatch) *(Amended)*
- [ADR-018: FSRS v5 Canonical Event Sourcing, Idempotent Reviews, and Golden Test Vectors](#adr-018-fsrs-v5-canonical-event-sourcing-idempotent-reviews-and-golden-test-vectors) *(Amended)*
- [ADR-019: Personal Workspace First and Hybrid Persona Architecture](#adr-019-personal-workspace-first-and-hybrid-persona-architecture)
- [ADR-020: Unified Review Queue with Orthogonal Tags and Elimination of Deck Entity for v1.0](#adr-020-unified-review-queue-with-orthogonal-tags-and-elimination-of-deck-entity-for-v10)
- [ADR-021: Lean MVP Scope Execution (8–10 Sprints, 1–2 Devs) and Deferral of Formal Exams to v1.x](#adr-021-lean-mvp-scope-execution-810-sprints-12-devs-and-deferral-of-formal-exams-to-v1x)
- [ADR-022: Resilient Online Strategy with "Keep Both" 409 Conflict Resolution for v1.0](#adr-022-resilient-online-strategy-with-keep-both-409-conflict-resolution-for-v10)

---

## ADR-001: Standardization of ProseMirror JSON AST and Leaf-Node Inline Marks over Offset-Based Annotations

### Status
**Amended** (Amended on 2026-10-31 by ADR-019, ADR-020, and Product Review Decisions 13 & 14)

### Context & Problem Statement
In legacy iterations of SWMS, lesson content was stored as raw plain text/markdown, while student annotations (highlights and notes) were persisted in a relational table using absolute character offsets:
$$\text{Annotation} = (\text{start\_offset}, \text{end\_offset}, \text{color}, \dots)$$
However, the front-end rich-text editor engine (Tiptap / ProseMirror) does not model text formatting as absolute document character ranges. ProseMirror employs a **Flat Inline Sequence Model**:
1. Documents consist of nested block nodes (e.g., `paragraph`, `heading`).
2. Inside each block, text is partitioned into discrete **leaf text nodes**, each carrying an array of formatting marks (`marks: []`).
3. Applying a highlight slices the text node into sibling segments:
   $$\text{TextNode}_{\text{plain}} \longrightarrow \text{TextNode}_{\text{highlighted}} \longrightarrow \text{TextNode}_{\text{plain}}$$

Maintaining absolute document offsets in the backend caused three critical defects:
- **Index Mismatch on Render**: ProseMirror positions account for block open/close tokens (costing 2 position units per block), making character indices completely out of sync with editor positions.
- **Catastrophic Offset Drift**: Any minor editorial update to a lesson shifted all subsequent offsets, corrupting existing student highlights.
- **Broken Bidirectional Linking**: The editor could not reconstruct DOM marks natively and required expensive runtime DOM slicing algorithms.

### Decision Drivers
- High-fidelity visual rendering in Tiptap without custom DOM slicing hacks.
- Resilience of student highlights when underlying lesson text is updated.
- W3C standard compliance for educational annotations and external semantic search.
- Clean separation between internal editor AST marks and external relational indexing.

### Considered Options
- **Option 1: Legacy Plain Text with Document-Level Character Offsets**:
  - *Pros*: Simple backend schema (`INT start_offset, INT end_offset`).
  - *Cons*: Total impedance mismatch with ProseMirror; severe coordinate drift; slow client rendering.
- **Option 2: Unified ProseMirror JSON AST with Leaf-Node Inline Marks & W3C TextQuoteSelector**:
  - *Pros*: 100% native Tiptap integration; zero client rendering friction; W3C Web Annotation compliance ensures searchability and self-healing resilience.
  - *Cons*: Requires migrating existing plain text lessons to ProseMirror JSON AST.

### Decision Outcome
**Chosen: Option 2**.
1. All lesson content is stored strictly as ProseMirror JSON AST in `lessons.content_ast`.
2. Highlights are represented in the AST as native `annotationMark` marks decorating leaf text nodes, storing `annotation_id`, `color_key` (superseding legacy raw hex values), and `created_at`.
3. The `annotations` relational table stores the **W3C Web Annotation Data Model (TextQuoteSelector)**:
   - `exact_quote TEXT`: The highlighted text excerpt.
   - `prefix_quote VARCHAR(100)`: 32 characters immediately preceding the excerpt.
   - `suffix_quote VARCHAR(100)`: 32 characters immediately following the excerpt.
   - `block_id UUID`: The stable UUID of the top-level block node containing the highlight.

### Authoritative References
- ProseMirror Foundation: *The Document Model: Inline Nodes, Marks, and Slice Operations* (Marijn Haverbeke).
- W3C Recommendation (23 February 2017): *Web Annotation Data Model — Section 4.3.2: TextQuoteSelector*.

### Technical Consequences
- **Positive**: Native, instantaneous rendering in Tiptap; stable anchors for AI flashcard generation; zero DOM slicing overhead.
- **Negative**: AST JSON payloads are approximately 25% larger than raw plain text.
- **Neutral**: Requires one-time data transformation for legacy lesson bodies.

### Validation & Verification Rules
- Verify that saving an annotation creates a valid record in `annotations` with non-null `exact_quote`, `prefix_quote`, `suffix_quote`, and `block_id` (for lesson text) OR `document_id`, `page_number`, and `bounding_boxes` (for PDF documents).
- Verify that the shared master document `lessons.content_ast` remains pristine in the database, while annotations render dynamically on the client via ProseMirror `DecorationSet` without cross-tenant mark pollution.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-019, ADR-020, and Product Review Decisions 13 & 14
- **Amendment Rationale**:
  1. *Highlight Color Storage (Decision 13)*: In legacy Section `Decision Outcome` item 2, raw hex color strings (`color: '#FEF08A'`) stored in AST leaf marks and the database are superseded. Storing hardcoded hex values causes severe contrast failures in Dark Mode and violates WCAG 1.4.1. Leaf-node marks now store `color_key` (`'yellow'`, `'blue'`, `'green'`, `'pink'`, `'purple'`). Colors are rendered dynamically on the client via TailwindCSS `@theme` variables (`--color-highlight-yellow`, etc.) paired with user-customizable semantic labels (e.g., "Core Definition", "Exam Alert").
  2. *Annotation Lifecycle Status (Decision 14 & Contradiction 7)*: The legacy schema representation combining `status` and `is_orphaned` boolean is unified. The column `is_orphaned` is dropped. The `status` column is defined as:
     ```sql
     status VARCHAR(30) NOT NULL DEFAULT 'anchored' 
         CHECK (status IN ('anchored', 'needs_review', 'orphaned', 'archived'))
     ```
     When an annotation loses its anchor during lesson revision, its status transitions to `'orphaned'`, triggering the non-blocking 3-action slide-over UI (Re-anchor, Keep unanchored, Delete).
- **Updated Consequences**:
  - Eliminates dark-mode contrast breakage and complies with accessibility guidelines.
  - Consolidates highlight lifecycle into a single authoritative status enum.

---

## ADR-002: Single JSONB Document Storage with Asynchronous Link Extraction for Notebooks over Normalized Blocks

### Status
**Amended** (Amended on 2026-10-31 by ADR-020, ADR-022, and Product Review Decision 3 & 12)

### Context & Problem Statement
Early designs attempted to emulate Notion's block model by normalizing notebooks into a relational table:
`notebook_blocks(id, notebook_id, block_type, content, sort_order, parent_id)`
In production, this relational approach collapses under real-world usage:
1. **Write Amplification**: Inserting a block at position 2 of a 500-block notebook forces:
   `UPDATE notebook_blocks SET sort_order = sort_order + 1 WHERE notebook_id = @id AND sort_order >= 2;`
   This locks 498 rows, floods PostgreSQL WAL, and degrades I/O.
2. **Row Lock Deadlocks (`40P01`)**: Rapid keystrokes and debounced auto-saves trigger concurrent range updates, resulting in transaction aborts.
3. **Fractional Indexing Underflow**: Using floating-point sort keys ($\text{mid} = (\text{prev} + \text{next}) / 2$) exhausts IEEE 754 double precision (53 mantissa bits) after ~53 consecutive insertions at the same position, requiring global rebalancing locks.

### Decision Drivers
- High-frequency auto-save write throughput without deadlocks.
- Native support for complex nested rich text (nested lists, code blocks, tables).
- Relational queryability for bidirectional linking (finding which notebooks cite a highlight or lesson).
- CRDT/real-time collaboration readiness.

### Considered Options
- **Option 1: Normalized Row-Per-Block Table (`notebook_blocks`) with Fractional Indexing**:
  - *Pros*: Direct SQL foreign keys per block.
  - *Cons*: Deadlocks; write amplification; fractional underflow; high CPU reconstruction latency.
- **Option 2: Single JSONB Document (`notebooks.content_ast`) with Asynchronous Auxiliary `notebook_links` Table**:
  - *Pros*: Atomic single-row update (`UPDATE notebooks SET content_ast = @ast ...`); 99% reduction in lock contention; native nested AST; fast client parse (<2ms); clean relational index for backlinks.
  - *Cons*: Requires an asynchronous extractor to maintain `notebook_links`.

### Decision Outcome
**Chosen: Option 2**.
1. Entire notebook content is persisted as a single ProseMirror JSONB document in `notebooks.content_ast`.
2. Saving a notebook is an atomic update against exactly one record.
3. **Optimistic Concurrency Control (OCC) & Conflict Resolution**:
   - Updates enforce version checking:
     ```sql
     UPDATE notebooks 
     SET content_ast = @ast, version = version + 1, updated_at = NOW() 
     WHERE id = @id AND version = @expected_version;
     ```
   - If rows affected is 0, the server returns a `409 Conflict`. The client fetches the current remote version and presents an interactive diff or non-destructive merge rather than silently overwriting changes with Last-Write-Wins.
4. **Client-Side Single-Writer Tab Locking**:
   - In multi-tab editing environments, client tabs coordinate via the `Web Locks API` (`navigator.locks.request('notebook_edit_' + id, ...)`). Only the active foreground tab possesses write authority, preventing concurrent debounced auto-save collisions.
5. **Defensive AST Rendering for Deleted Annotations**:
   - When an annotation referenced within `content_ast` (`annotationRef`) is deleted via `ON DELETE SET NULL`, the relational foreign key is cleared, but the JSONB document remains structurally valid.
   - The Tiptap `annotationRef` node extension includes a defensive fallback that renders an `[Archived / Removed Source]` badge with cached quote text, preventing client-side unhandled null exceptions.
6. Relational backlinks are extracted asynchronously into an auxiliary `notebook_links` table:
   ```sql
   CREATE TABLE notebook_links (
       id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
       notebook_id UUID NOT NULL REFERENCES notebooks(id) ON DELETE CASCADE,
       source_block_id UUID NOT NULL,
       target_type VARCHAR(30) NOT NULL,
       ref_annotation_id UUID REFERENCES annotations(id) ON DELETE CASCADE,
       ref_lesson_id UUID REFERENCES lessons(id) ON DELETE CASCADE,
       created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
   );
   ```

### Authoritative References
- Notion Engineering: *The Data Model Behind Notion’s Scalability Challenges* (Systems Architecture Papers).
- PostgreSQL 17 Documentation: *TOAST (The Oversized-Attribute Storage Technique) Compression for JSONB*.
- W3C Web Application Security Working Group: *Web Locks API Specification*.

### Technical Consequences
- **Positive**: Eliminates deadlocks; auto-save latency reduced from >350ms to $p95 < 50\text{ms}$ ($p99 < 150\text{ms}$ SLO); relational backlinks queryable via standard indexed SQL joins; immune to silent multi-tab overwrites.
- **Negative**: Relational links are eventually consistent by a few milliseconds.
- **Neutral**: Notebook storage utilizes PostgreSQL inline LZ4/TOAST compression.

### Validation & Verification Rules
- Stress-test 1,000 concurrent updates on a single notebook: confirm zero `40P01` deadlock exceptions.
- Open notebook in two browser tabs: verify that simultaneous typing is coordinated via Web Locks, and stale saves reject with 409 Conflict without data destruction.
- Verify that adding an `annotationRef` block in a notebook generates a corresponding row in `notebook_links`.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-020, ADR-022, and Product Review Decisions 3 & 12
- **Amendment Rationale**:
  1. *Conflict Resolution Strategy (Decision 12 & ADR-022)*: In Section `Decision Outcome` item 3, the mandate for an "interactive diff or non-destructive merge" on 409 Conflict is superseded for v1.0. The system adopts the **"Keep Both" Strategy**: on 409, the server saves the conflicting local payload into a companion note (`[Conflict Copy - YYYY-MM-DD] <Title>`) and loads the authoritative remote AST. Interactive 3-way AST merging is deferred to v1.x.
  2. *Document Source Linking (Decision 3)*: The auxiliary `notebook_links` table is expanded to support uploaded PDF documents in addition to lessons:
     ```sql
     ALTER TABLE notebook_links ADD COLUMN ref_document_id UUID REFERENCES documents(id) ON DELETE CASCADE;
     ```
  3. *Latency Metric Alignment (Contradiction 2)*: Technical Consequences and decision text claiming "sub-20ms auto-saves" or "<20ms" are amended to align with the formal measurable SLO in Section 13.5: Auto-Save Throughput $p95 < 50\text{ms}, p99 < 150\text{ms}$.
- **Updated Consequences**:
  - 100% data loss prevention during concurrent editing without the high engineering cost of building a full 3-way AST merge UI.
  - Multi-source citation linking for both lessons and PDF documents.
  - Performance commitments standardized around realistic, measurable SLOs ($p95 < 50\text{ms}, p99 < 150\text{ms}$).

---

## ADR-003: Refresh Token Rotation (RTR) with Token Families and User Token Versioning for JWT Security

### Status
**Accepted**

### Context & Problem Statement
Stateless JWT access tokens present severe security vulnerabilities if not paired with a secure database-backed lifecycle:
1. **Revocation Impossibility**: Compromised access tokens remain valid until expiration (e.g., 60 minutes) with no way to invalidate them upon logout or password reset.
2. **Replay Attacks**: A stolen long-lived refresh token permits an attacker to maintain persistent, undetected access indefinitely.

### Decision Drivers
- Strict compliance with RFC 6749, RFC 6819, and OWASP JWT Cheat Sheet.
- Immediate revocation capability for single sessions or all global sessions.
- Automatic breach detection when a revoked token is reused.
- Cryptographic protection against database leakage.

### Considered Options
- **Option 1: Pure Stateless JWT without Database Persistence**:
  - *Pros*: No database reads on authentication.
  - *Cons*: Cannot revoke tokens; impossible to detect token theft.
- **Option 2: Refresh Token Rotation (RTR) with Token Families and User `token_version`**:
  - *Pros*: Single-use refresh tokens; token family tracking isolates and revokes compromised sessions; `token_version` provides immediate global invalidation; tokens hashed with SHA-256.
  - *Cons*: Database write on every refresh token exchange (every 15 minutes).

### Decision Outcome
**Chosen: Option 2**.
1. Refresh tokens are persisted in `refresh_tokens`. The database stores `token_hash = SHA256(raw_token)` and groups related tokens under a `family_id` (UUID).
2. **Rotation Rule**: Presenting a valid refresh token revokes it (`is_revoked = true`, `revoked_at = NOW()`) and issues a new `(AccessToken, RefreshToken)` pair in the same family, setting `replaced_by_token_id`.
3. **Idempotent Rotation Grace Window (15–30 Seconds)**:
   - When an incoming refresh token has `is_revoked == true`:
     - If `revoked_at` is within the last 15–30 seconds AND `replaced_by_token_id` is not null: The request is classified as a legitimate cellular network retry or concurrent multi-tab expiration. The server returns the previously issued replacement token pair (`replaced_by_token_id`) idempotently.
     - If `revoked_at` exceeds the 30-second window or lacks a replacement pointer: The request is classified as an active token replay attack.
4. **Automatic Breach Detection & Alarm**:
   - If an active replay attack is detected, the backend immediately revokes **all tokens in that `family_id`** (`UPDATE refresh_tokens SET is_revoked = true WHERE family_id = @family_id`) and increments `users.token_version`.
5. **Client-Side Single-Flight Concurrency**:
   - Frontend clients implement a single-flight guard using the `Web Locks API` (`navigator.locks.request('auth_refresh', ...)`), ensuring only one tab executes the token exchange while sibling tabs await the cached outcome.
6. **Instant Distributed Invalidation via Redis Pub/Sub**:
   - `users.token_version` is embedded in the JWT access token payload (`"ver"`). Middleware checks `ver` against a distributed Redis cache. When `token_version` increments, an invalidation message is broadcast via Redis Pub/Sub to all Kestrel instances, evicting the user's cached credentials globally within <5ms.
7. **Pessimistic Row Locking (`SELECT ... FOR UPDATE`)**:
   - To eliminate concurrency race conditions when multiple tabs or rapid network retry bursts arrive at the database simultaneously before client single-flight locks engage, the verification query acquires an exclusive row-level lock in PostgreSQL:
     ```sql
     SELECT * FROM refresh_tokens WHERE token_hash = @token_hash FOR UPDATE;
     ```
   - This strictly serializes concurrent exchanges: the first transaction commits token rotation (`is_revoked = true, replaced_by_token_id = @new_id`), while subsequent transactions wait and then read the committed `is_revoked = true` state, routing cleanly into the 15–30s idempotent grace recovery branch without creating divergent token family forks or false breach alarms.

### Authoritative References
- IETF RFC 6819: *OAuth 2.0 Threat Model and Security Considerations — Section 5.2.2.3: Refresh Token Rotation*.
- OWASP Foundation: *JSON Web Token Cheat Sheet for Developers: Storage, Rotation, and Revocation*.
- Auth0 Architecture Patterns: *Refresh Token Rotation and Race Condition Recovery Windows*.

### Technical Consequences
- **Positive**: Complete defense against token replay; immediate global session invalidation; zero plain-text token exposure in DB; zero false-positive session logouts on cellular retries or multi-tab study sessions.
- **Negative**: Requires periodic cleanup of expired refresh tokens in the database.
- **Neutral**: Token refresh overhead is minimal (~5ms DB round-trip every 15 minutes).

### Validation & Verification Rules
- Test replay scenario within 15 seconds: verify server idempotently returns replacement token without terminating session.
- Test replay scenario after 35 seconds: verify server triggers breach alarm, revokes all tokens in family, and increments `users.token_version`.

---

## ADR-004: Foreign Key Cascading Strategy and Knowledge Preservation via ON DELETE SET NULL for Flashcards

### Status
**Amended** (Amended on 2026-10-31 by ADR-019, ADR-020, and Product Review Decisions 4 & 14)

### Context & Problem Statement
In an integrated LMS + PKM + SRS ecosystem, foreign key deletion rules carry existential product risks:
- **Risk 1 (Orphan Records)**: Lacking `CASCADE` on strict hierarchies leaves detached records and triggers foreign key violations (`23503`).
- **Risk 2 (Catastrophic Knowledge Loss)**: Indiscriminate `CASCADE` deletes valuable learner assets. If a student highlights a sentence, creates a flashcard from it, reviews it for 6 months, and later deletes the highlight, a cascade would permanently erase the flashcard, its FSRS stability parameters, and its entire review log history!

### Decision Drivers
- Strict compositional deletion for course curriculum hierarchies.
- Absolute preservation of student intellectual property and spaced repetition memory history.
- GDPR compliance for user account deletion.

### Considered Options
- **Option 1: Blanket `ON DELETE CASCADE` across all Foreign Keys**:
  - *Pros*: Simple DDL.
  - *Cons*: Deleting a highlight or lesson obliterates student flashcards and months of cognitive memory data.
- **Option 2: Dual Strategy Matrix (`CASCADE` for Hierarchy, `SET NULL` for Knowledge Artifacts)**:
  - *Pros*: Strict hierarchy cleanup; flashcards and review histories are permanently protected.
  - *Cons*: Requires nullable foreign key columns and UI handling for orphaned source links.

### Decision Outcome
**Chosen: Option 2**.
1. **Hierarchical Cascading (`ON DELETE CASCADE`)**:
   - `chapters.course_id -> courses.id (CASCADE)`
   - `lessons.chapter_id -> chapters.id (CASCADE)`
   - `annotations.lesson_id -> lessons.id (CASCADE)`
   - `refresh_tokens.user_id -> users.id (CASCADE)`
   - `practice_quiz_items.quiz_id -> practice_quizzes.id (CASCADE)`
   - `flashcard_reviews.flashcard_id -> flashcards.id (CASCADE)`
2. **Knowledge Preservation (`ON DELETE SET NULL`)**:
   - `flashcards.annotation_id -> annotations.id (SET NULL)`
   - `flashcards.lesson_id -> lessons.id (SET NULL)`
   - `notebooks.primary_source_lesson_id -> lessons.id (SET NULL)`
   - `notebooks.primary_source_document_id -> documents.id (SET NULL)`
   - `practice_quizzes.lesson_id -> lessons.id (SET NULL)`
   - `practice_quizzes.document_id -> documents.id (SET NULL)`
3. **Flashcard Soft-Delete Pattern (`is_archived`)**:
   - To protect the historical review log dataset necessary for training personal FSRS weights ($w_0 \dots w_{18}$), `flashcards` incorporates `is_archived BOOLEAN NOT NULL DEFAULT FALSE`.
   - When a student archives or removes a card, `is_archived` is set to `true`. Hard deletion of flashcard records with existing rows in `flashcard_reviews` is prohibited by application policy and database triggers.
4. **Denormalized Snapshotting (`source_title_snapshot`, `source_quote_snapshot`)**:
   - The `flashcards` table includes snapshot columns:
     ```sql
     source_title_snapshot TEXT,
     source_quote_snapshot TEXT,
     ```
     capturing the lesson title and highlighted quote at card creation time. If an annotation or lesson is subsequently deleted (`ON DELETE SET NULL`), the flashcard UI continues displaying full curriculum context without throwing null reference exceptions.
5. **Defensive Client AST Rendering**:
   - When an annotation is deleted, any `annotationRef` node in `notebooks.content_ast` lazy-renders an `[Archived / Removed Source]` badge with cached quote context rather than throwing a client-side error.

### Authoritative References
- E. F. Codd: *A Relational Model of Data for Large Shared Data Banks — Referential Integrity Constraints*.
- Open Spaced Repetition Principles: *Permanence of Cognitive Memory Traces in SRS Systems*.
- Martin Fowler: *Soft Deletion and Event Logging in Domain-Driven Design*.

### Technical Consequences
- **Positive**: Zero risk of accidental knowledge destruction; curriculum updates do not damage student review queues and tag-filtered study sessions; clean GDPR cascade upon user account deletion; permanent retention of FSRS training dataset.
- **Negative**: The UI must display an "Unlinked / Archived Source" badge when a flashcard's original highlight is deleted.
- **Neutral**: Nullable foreign keys require indexed lookup checks.

### Validation & Verification Rules
- Execute: `DELETE FROM annotations WHERE id = @id;`. Confirm that associated flashcards in `flashcards` still exist with `annotation_id IS NULL`, `source_title_snapshot` and `source_quote_snapshot` retain readable context, and all `flashcard_reviews` remain intact.
- Archive a flashcard: verify `is_archived = true`, that it is excluded from daily review queries, but its `flashcard_reviews` rows remain accessible for weight optimization.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-019, ADR-020, and Product Review Decisions 4 & 14
- **Amendment Rationale**:
  1. *Elimination of Decks (Decision 4 & ADR-020)*: All narrative references to "student study decks" are amended to "student review queues and tag-filtered study sessions". The platform maintains zero `decks` tables. Flashcards belong directly to `users(id)` and are tagged via `tags` and `flashcard_tags`.
  2. *Personal Workspace Root (Decision 1 & ADR-019)*: `courses` with `visibility = 'personal'` serve as the private root for independent learner assets, ensuring cascading account deletion adheres to GDPR without destroying unorganized notes.
  3. *Preservation of FSRS Datasets*: `flashcards.is_archived` and denormalized snapshots (`source_title_snapshot`, `source_quote_snapshot`) remain strictly enforced. Dropping an annotation sets `flashcards.annotation_id = NULL` via `ON DELETE SET NULL`, preserving the card, its stability parameters, and all `flashcard_reviews`.
- **Updated Consequences**:
  - Decouples flashcards from rigid deck hierarchies while preserving full cascading integrity on user deletion.
  - Maintains complete historical dataset for FSRS personalized weight calibration.

---

## ADR-005: Migration from pgvector IVFFlat to HNSW Indexing for Dynamic and Cold-Start Vector Embeddings

### Status
**Amended** (Amended on 2026-10-31 by Product Review Decision 7 and Contradiction 4)

### Context & Problem Statement
Early SWMS vector search utilized pgvector's `ivfflat` index. IVFFlat partitions vector space using $k$-means clustering during index creation. This introduces a fatal "empty table trap":
1. If `CREATE INDEX ... USING ivfflat` executes on an empty table (standard practice during initial database deployment migrations), $k$-means degenerates into arbitrary, meaningless centroids.
2. Vectors inserted post-migration are assigned to these corrupt centroids without recalculating cluster centers.
3. **Search Recall collapses to near zero (<10%)**, behaving like random retrieval unless an administrator runs `REINDEX TABLE` manually after thousands of rows are inserted.

### Decision Drivers
- Reliable semantic vector search on empty and dynamically populated tables.
- High search recall ($\ge 98\%$ theoretical unconstrained graph recall, $\ge 95\%$ production filtered search SLO) for high-dimensional embeddings (768-dim Google text-embedding-004 / 1536-dim OpenAI).
- Immunity against post-filtering recall collapse when querying specific lessons or chapters.
- Zero-maintenance indexing requiring no manual reindexing post-deployment.

### Considered Options
- **Option 1: Retain IVFFlat with Scheduled Maintenance Scripts**:
  - *Pros*: Smaller index size and faster build times on large static datasets.
  - *Cons*: Totally broken on initial deployments; high operational complexity; poor dynamic recall.
- **Option 2: Migrate to HNSW (Hierarchical Navigable Small World) with Iterative Filtering Scan & Composite Index**:
  - *Pros*: No pre-training phase required; builds dynamically as rows are added; recall $\ge 98\%$ (unconstrained); iterative index scan prevents post-filtering recall collapse; composite index fallback provides 100% exact recall on single-lesson scopes.
  - *Cons*: Higher memory (RAM) and disk footprint than IVFFlat; slightly longer build time.

### Decision Outcome
**Chosen: Option 2**.
1. All vector columns in `lesson_embeddings` and `notebook_embeddings` are indexed using **HNSW**:
   ```sql
   CREATE INDEX idx_lesson_embeddings_hnsw 
   ON lesson_embeddings 
   USING hnsw (embedding vector_cosine_ops) 
   WITH (m = 16, ef_construction = 128);
   ```
   - `m = 16`: Balances memory overhead and neighbor connectivity for 768-dim vectors.
   - `ef_construction = 128`: Ensures dense graph exploration during inserts, guaranteeing $\ge 98\%$ theoretical recall.
   - Runtime search parameter: `SET hnsw.ef_search = 100;`.
2. **Mitigating Post-Filtering Recall Collapse**:
   - *Problem*: In global HNSW graphs, selective queries (e.g. `WHERE lesson_id = 50`) can result in 0.0% recall because the global top-100 candidates may contain zero items from that single lesson.
   - *Iterative Index Scan*: Enable pgvector 0.7.0+/0.8+ iterative scan:
     ```sql
     SET hnsw.iterative_scan = 'relaxed_order';
     ```
     This instructs pgvector to dynamically traverse deeper graph layers until the limit ($k$) is satisfied by matching rows (valid pgvector 0.8+ enum values: `off`, `relaxed_order`, `strict_order`).
   - *Composite B-Tree & Query Planner Routing*:
     ```sql
     CREATE INDEX idx_lesson_embeddings_lesson_id ON lesson_embeddings(lesson_id);
     ```
     For single-lesson queries, the query planner prefers an exact vector scan across the filtered partition (`ORDER BY embedding <=> @q LIMIT 5`), while multi-lesson and curriculum searches route through the HNSW graph. This dual strategy guarantees $\ge 98\%$ unconstrained recall and $\ge 95\%$ production recall across tenant/lesson-filtered scopes.

### Authoritative References
- Andrew Kane: *pgvector: Vector similarity search for PostgreSQL — HNSW vs IVFFlat Guidelines & Iterative Index Scan in pgvector 0.7.0+*.
- Yu. A. Malkov and D. A. Yashunin (2018): *Efficient and robust approximate nearest neighbor search using Hierarchical Navigable Small World graphs* (IEEE TPAMI).

### Technical Consequences
- **Positive**: Zero recall degradation on empty table deployments; continuous index updates as students create notes; superior search accuracy; 100% immune to post-filtering recall collapse on single-lesson queries.
- **Negative**: Index consumes roughly 1.5x more RAM than IVFFlat.
- **Neutral**: Standard pgvector SQL syntax remains unchanged (`<=>` cosine distance operator).

### Validation & Verification Rules
- Create the HNSW index on an empty `lesson_embeddings` table during migration. Insert 100 chunks. Verify that vector similarity queries return expected nearest neighbors with recall matching exhaustive linear search.
- Test filtered query on 50,000 chunks partitioned across 1,000 lessons: verify that `WHERE lesson_id = @id` achieves $\ge 95\%$ recall with iterative scan enabled.

### Amendment Notes (2026-10-31)
- **Amended by**: Product Review Decision 7 and Contradiction 4
- **Amendment Rationale**:
  1. *Vector Search Recall SLO Reconciliation (Contradiction 4)*: Narrative statements claiming unconstrained "$\ge 98\%$ recall" are reconciled with the production SLO in Section 13.5: The theoretical HNSW graph recall on unpartitioned collections is $\ge 98\%$; under selective tenant/lesson filters (`WHERE user_id = @uid`), the production guaranteed SLO target is **$\ge 95\%$ recall** (top-$k=10$), achieved via pgvector iterative scan (`SET hnsw.iterative_scan = 'relaxed_order';`) and composite B-Tree index routing.
  2. *Vietnamese Full-Text Search Integration (Decision 7)*: Because PostgreSQL `tsvector` lacks native Vietnamese language dictionaries and stemmers, semantic vector search is augmented by a hybrid text search engine using PostgreSQL `unaccent` extension and `pg_trgm` (trigram matching) for diacritic-insensitive keyword search across lessons and notes.
- **Updated Consequences**:
  - Realistic and verifiable production SLO: unconstrained theoretical graph recall $\ge 98\%$, tenant-filtered search recall $\ge 95\%$.
  - First-class support for Vietnamese text search without foreign dictionary limitations.

---

## ADR-006: Dedicated Exam Attempts Entity Grouping for Chapter and Course Assessments

### Status
**Amended** (Amended on 2026-10-31 by ADR-021 and Product Review Decision 6)

### Context & Problem Statement
SWMS initially supported only isolated quiz questions embedded in individual lessons (`lesson_quizzes`). This model failed to support:
1. Formal chapter milestone exams (15–20 questions) and comprehensive course finals (50+ questions).
2. Multi-attempt tracking (comparing Attempt 1 vs Attempt 2 scores, enforcing cooldowns).
3. Session state persistence (recovering an `in_progress` exam after a network drop).
4. Hybrid grading pipelines where multiple-choice questions are auto-graded immediately, while open-ended essays are routed to asynchronous AI rubric evaluation.

### Decision Drivers
- Transactional integrity for formal academic testing.
- Comprehensive attempt lifecycle management.
- Granular response recording for AI semantic feedback.

### Considered Options
- **Option 1: Ad-hoc Quiz Log Table**:
  - *Pros*: Minimal schema additions.
  - *Cons*: Lacks session grouping; cannot calculate aggregate exam scores, enforce time limits, or compare attempts.
- **Option 2: First-Class `practice_quizzes` and `practice_quiz_items` Relational Entities**:
  - *Pros*: Complete practice session lifecycle (`in_progress -> completed -> abandoned`); supports instant self-check and qualitative AI rubric feedback; tracks start/completion timestamps and item counts.
  - *Cons*: Requires managing dedicated assessment session tables.

### Decision Outcome
**Chosen: Option 2**.
Introduce two central entities for formative assessments:
1. `practice_quizzes`: Manages the self-study quiz session, linking optionally to `lesson_id` or `document_id`, with `title`, `total_items`, `correct_items`, `status` (`in_progress`, `completed`, `abandoned`), `started_at`, `completed_at`, and `ai_summary_feedback`.
2. `practice_quiz_items`: Persists per-question responses (`user_answer` JSONB), `item_type` (`multiple_choice`, `cloze`, `short_answer`, `essay`), `prompt_ast`, `choices`, `cloze_solution`, correctness (`is_correct`), and structured `ai_rubric_feedback` JSONB containing qualitative rubric evaluation.

### Authoritative References
- IMS Global Learning Consortium: *Question and Test Interoperability (QTI) Specification v3.0*.
- Enterprise Learning Management System Design Patterns (Assessment Session State Machines).

### Technical Consequences
- **Positive**: Clean audit trail of student performance; multi-attempt support; seamless integration with AI rubric evaluation.
- **Negative**: Requires session expiration cleanup for abandoned tests.
- **Neutral**: Standard foreign key cascade from attempt to attempt items.

### Validation & Verification Rules
- Verify that completing a practice quiz transitions status to 'completed', computes correct item counts, and aggregates item-level results.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-021 and Product Review Decision 6
- **Amendment Rationale**:
  1. *Assessment Scope Realignment for v1.0 (Decision 6 & ADR-021)*: The requirement for formal proctored Chapter and Course Finals with official passing scores (`passing_score`), strict time limits, and formal instructor grading workflows is **deferred to v1.x**.
  2. *v1.0 Formative Practice Quizzes*: In SWMS v1.0, the assessment module implements `practice_quizzes` & `practice_quiz_items` exclusively for formative self-assessment with qualitative AI rubric feedback, while formal proctored exam attempts and grading tables are deferred to the v1.x backlog per ADR-021.
  3. *Qualitative AI Rubric Feedback*: Open-ended essay responses receive formative AI rubric suggestions, highlighted strengths, and missing concept recommendations, but do NOT award binding academic grades or determine course advancement. The underlying `practice_quizzes` and `practice_quiz_items` schema entities serve as the structural foundation for self-assessment.
- **Updated Consequences**:
  - Significant reduction in v1.0 implementation scope for 1–2 developers while delivering immediate formative assessment value.
  - Aligned database schema: tables `practice_quizzes` and `practice_quiz_items` provide the lightweight self-assessment foundation for v1.0, while formal proctored exam attempts and grading tables are deferred to the v1.x backlog per ADR-021.

---

## ADR-007: Resizable Multi-Pane Responsive Layout for Standard 1920x1080 Baseline

### Status
**Amended** (Amended on 2026-10-31 by Product Review Decision 8 and ADR-021)

### Context & Problem Statement
The legacy layout hardcoded pixel constraints ($580\text{px} - 650\text{px}$) across workspace containers:
- On **1920x1080 (the dominant desktop standard, >65% market share)**: The combined width of the panels ($650 + 580 = 1230\text{px}$) left **410px of wasted dead space**, constricting content and frustrating users.
- On **smaller screens (laptops 1366x768, tablets 1024px)**: The fixed width exceeded the viewport, causing severe horizontal scrollbar overflows.

### Decision Drivers
- Ergonomic reading experience on 1080p desktop baseline (optimal line length 65–75 characters).
- Fluid adaptability from large desktops down to mobile devices.
- User agency to adjust pane proportions with persistence across sessions.

### Considered Options
- **Option 1: Static CSS Media Queries with Fixed Width Containers**:
  - *Pros*: Trivial implementation.
  - *Cons*: Wastes space on 1080p; rigid; no user resizing capability.
- **Option 2: Dynamic Multi-Pane Layout via `react-resizable-panels` with Tailwind v4 Clamps**:
  - *Pros*: Fluid pixel utilization; customizable panel ratios (15/45/40); double-click reset; `localStorage` persistence; zero horizontal overflow.
  - *Cons*: Requires client-side panel state management.

### Decision Outcome
**Chosen: Option 2**.
1. Standard 1920x1080 Baseline:
   - **Sidebar**: 15% (~288px), collapsible to 48px rail with `Ctrl + \`.
   - **Reader Pane**: 45% (~864px), prose constrained to `max-w-3xl` (68ch) with symmetric padding.
   - **Notebook Pane**: 40% (~768px), fluid rich text editor.
2. **Container Sizing & Resize Handles**:
   - Total available pane width is calculated as `calc(100vw - 8px)` to account for the two 4px resize handles (`w-1`, expanding to `w-2` on hover). This completely eliminates the 8px horizontal scrollbar overflow on exact 1920px viewports.
   - Splitters support centered grip indicators, double-click reset to 15/45/40, and local storage ratio persistence.
3. **Scroll-Sync Mutex & Infinite Loop Prevention with Trailing-Edge Dispatch**:
   - To eliminate recursive ping-pong scroll events between Reader and Notebook panes while ensuring rapid flick-scrolling never drops the final resting position, a mutual exclusion lock (`isScrollingLockedRef` / `isSyncing`) with a 50ms release timer and trailing-edge dispatch via `pendingAnchorId` is enforced on all programmatic scroll sync triggers:
     ```typescript
     let isSyncing = false;
     let syncTimeout: NodeJS.Timeout | null = null;
     let pendingAnchorId: string | null = null;

     export function handleReaderScroll(targetAnchorId: string) {
       pendingAnchorId = targetAnchorId;
       if (isSyncing) return;
       isSyncing = true;
       requestAnimationFrame(() => {
         if (pendingAnchorId) {
           syncNotebookToAnchor(pendingAnchorId);
           pendingAnchorId = null;
         }
         if (syncTimeout) clearTimeout(syncTimeout);
         syncTimeout = setTimeout(() => {
           isSyncing = false;
           // Trailing edge dispatch: sync final anchor if updated during cooldown
           if (pendingAnchorId) {
             handleReaderScroll(pendingAnchorId);
           }
         }, 50);
       });
     }
     ```
4. **State Preservation Across Modes**:
   - Component unmounting during mode transitions does not purge active state. Active practice quizzes (`practice_quizzes`), form answers, and notebook drafts are stored in persistent global Zustand stores (`useQuizStore`, `useNotebookStore`), preventing accidental data loss when switching between Split Workspace and Review Mode.
5. Responsive breakpoints collapse to 2 panes on tablet landscape and tabbed single panes on mobile.

### Authoritative References
- W3C CSS Grid Layout Module Level 2: *Flexible Tracks and minmax() Sizing Rules*.
- Nielsen Norman Group: *Optimal Line Length for Reading on Desktop Screens (50–75 characters)*.
- React UI Concurrency Patterns: *Mutual Exclusion Flags in Bidirectional Event Listeners*.

### Technical Consequences
- **Positive**: Eradicates dead space on 1080p; fluid cross-device responsiveness; customizable learner ergonomics; zero horizontal overflow; zero layout thrashing or infinite scroll ping-pong.
- **Negative**: Requires careful hydration handling in Next.js App Router to avoid SSR layout mismatch.
- **Neutral**: Panel resize events are throttled to 60fps.

### Validation & Verification Rules
- Inspect layout at 1920x1080: ensure 100% viewport width is utilized without horizontal scrollbars, and double-clicking the splitter restores 15/45/40 ratio.
- Rapidly scroll Reader pane: verify Notebook scrolls smoothly in tandem without triggering reciprocal scroll events back to Reader or freezing the tab.
- Begin an exam in Review Mode, switch to Split Workspace, and return to Review Mode: confirm all unsubmitted answers are preserved.

### Amendment Notes (2026-10-31)
- **Amended by**: Product Review Decision 8 and ADR-021
- **Amendment Rationale**:
  1. *Responsive Baseline Risk & High-DPI Viewports (Decision 8)*: While the 1920x1080 baseline (15/45/40 layout) is retained, the specification explicitly recognizes that Windows laptops at 125% DPI scale yield a 1536px CSS viewport (collapsing the sidebar to a 48px icon rail), and at 150% scale yield a 1280px CSS viewport (collapsing to a 2-pane adaptive view).
  2. *Mandatory E2E Test Suite*: Formally mandates automated Playwright visual regression tests at 1920x1080 (3-pane), 1536x864 (rail + 2-pane), and 1280x720 (2-pane adaptive), verifying zero horizontal overflow and no clipped action buttons across standard display configurations.
- **Updated Consequences**:
  - Eliminates clipping and horizontal scrollbars across common scaled laptop screens while maintaining the 1080p desktop design target.
  - Automated visual regression testing ensures continuous responsive compliance.

---

## ADR-008: Server-Side Three-Tier Fuzzy Matching for Self-Healing Annotation Offsets on Lesson Updates

### Status
**Amended** (Amended on 2026-10-31 by Product Review Decision 14 and Contradiction 7)

### Context & Problem Statement
When an instructor edits or revises a lesson, previously placed student highlights risk offset misalignment. Reconciling annotations on the client when a student loads a lesson suffers from major flaws:
1. **Client Performance Degradation (UI Freeze)**: Running string alignment algorithms on low-end or mobile devices causes noticeable time-to-interactive delays.
2. **Non-Deterministic Rendering**: Differences in browser JavaScript engines or partial caches can produce inconsistent coordinate reconciliations across students.
3. **Write Storms on the Database**: If hundreds of students open an updated lesson simultaneously, their clients would calculate new coordinates and bombard the backend with concurrent update requests.

### Decision Drivers
- 100% deterministic coordinate resolution across all students.
- Zero client-side computation or rendering lag.
- Elimination of concurrent write storms.
- Clear identification and flagging of orphaned annotations.

### Considered Options
- **Option 1: Client-Side On-the-Fly Reconciliation on Page Load**:
  - *Pros*: Zero backend processing on version publish.
  - *Cons*: High client CPU load; non-deterministic; database write storms; silent annotation corruption.
- **Option 2: Server-Side 3-Tier Fuzzy Matching Executed upon Lesson Version Publication**:
  - *Pros*: Runs exactly once in the background; 100% deterministic; zero client overhead; cleanly flags unrecoverable annotations as `status = 'orphaned'` (dropping redundant `is_orphaned` boolean).
  - *Cons*: Editorial publication takes 1–3 seconds of background computation.

### Decision Outcome
**Chosen: Option 2**.
When an instructor publishes Lesson Version $V_{n+1}$, the backend executes a 3-tier self-healing and AST reconciliation algorithm:
1. **Tier 1 (Exact Match via W3C Selector)**: Searches for `prefix_quote + exact_quote + suffix_quote` in $V_{n+1}$. If found, updates offsets with 100% confidence.
2. **Tier 2 (Coordinate Shift via Myers Diff)**: Computes the edit distance diff script between $V_n$ and $V_{n+1}$ plain texts. Projects the coordinate shift vector $\Delta x$ into a local $\pm 250\text{ch}$ search window.
3. **Tier 3 (Local Fuzzy Alignment via Smith-Waterman) & SQL Quote Synchronization**:
   - If the quote was modified (e.g., minor typo fix: "proccessor cache" $\to$ "processor cache"), applies Smith-Waterman local alignment. If similarity $\ge 0.85$, offsets are automatically healed.
   - **SQL Database Quote Update**: In addition to updating character offsets and ProseMirror AST marks, the background worker updates the database record:
     ```sql
     UPDATE annotations
     SET exact_quote = @healed_exact, prefix_quote = @healed_prefix, suffix_quote = @healed_suffix, updated_at = NOW()
     WHERE id = @annotation_id;
     ```
     This prevents subsequent lesson updates ($V_{n+2}$) from perpetually searching for stale typos in Tier 1 and guarantees that flashcard generation, reader citations, and search results display the corrected text.
4. **Secondary Global Fallback**: If Tier 3 fails within the $\pm 250\text{ch}$ window (e.g., due to paragraph reordering or list conversion), the algorithm executes an unconstrained exact quote search across the entire lesson text before flagging `status = 'orphaned'`.
5. **Multi-Tenant Pristine Master AST & Client-Side DecorationSet**:
   - The shared master document `lessons.content_ast` is strictly immutable and pristine—individual student highlights are **NEVER** baked into the stored lesson JSONB, eliminating multi-tenant privacy leaks and payload bloat.
   - When version $V_{n+1}$ is published, the backend reconciliation worker updates `annotations.block_id`, `start_offset`, and `end_offset` in the student's personal `annotations` records.
   - On the client, Tiptap renders student highlights dynamically in-memory via ProseMirror `DecorationSet` (`Decoration.inline(start, end, { class: 'swms-highlight', 'data-annotation-id': id })`), ensuring 100% tenant isolation, instant rendering, and zero database bloat.

### Authoritative References
- Eugene W. Myers: *An O(ND) Difference Algorithm and Its Variations* (Algorithmica 1986).
- T. F. Smith and M. S. Waterman: *Identification of Common Molecular Subsequences* (Journal of Molecular Biology).
- ProseMirror Guide: *Decorations and View Modeling without Mutating Document State*.

### Technical Consequences
- **Positive**: Zero client lag; perfect determinism; complete protection against coordinate drift; strict multi-tenant privacy isolation; highlights render reliably in Tiptap via DecorationSet; zero payload explosion on shared lessons.
- **Negative**: Requires persisting `plain_text` alongside `content_ast` for diff processing.
- **Neutral**: Backend execution occurs asynchronously upon lesson publication.

### Validation & Verification Rules
- Test lesson revision with an inserted paragraph at line 1: confirm that downstream annotations shift offsets correctly with zero orphaned flags.
- Test modified quote with a typo fix: confirm Smith-Waterman heals coordinates if similarity $\ge 0.85$, and verify that `annotations.exact_quote`, `prefix_quote`, and `suffix_quote` in PostgreSQL are updated to reflect the new matched text.
- Inspect `lessons.content_ast`: verify that it remains pristine without student marks, while student clients receive their individual highlights and render them via `DecorationSet`.

### Amendment Notes (2026-10-31)
- **Amended by**: Product Review Decision 14 and Contradiction 7
- **Amendment Rationale**:
  1. *State Model Consolidation (Contradiction 7)*: Dropped `is_orphaned` boolean column. When Tier 3 (Smith-Waterman) and global fallback fail to locate matched text, the worker executes:
     ```sql
     UPDATE annotations SET status = 'orphaned', updated_at = NOW() WHERE id = @annotation_id;
     ```
  2. *Actionable Orphaned UI Workflow (Decision 14)*: When a student encounters an orphaned highlight, the Reader UI renders a non-blocking floating banner offering a 3-action slide-over:
     - *Action 1 (Re-anchor)*: User selects new text in the revised document to re-bind the highlight.
     - *Action 2 (Keep Unanchored)*: Preserves quote, tags, and notes as a detached card in their notebook and flashcard queue.
     - *Action 3 (Delete)*: Removes the annotation cleanly; dependent flashcards are preserved via `ON DELETE SET NULL`.
- **Updated Consequences**:
  - Replaces broken or ambiguous boolean flags with a single authoritative enum state (`status`).
  - Clear, user-driven recovery workflow for highlights that lose textual anchoring during document revisions.

---

## ADR-009: Asynchronous Background Job Processing via .NET 10 Bounded Channels and Real-Time Notification for AI Tasks

### Status
**Amended** (Amended on 2026-10-31 by ADR-017, ADR-021, Product Review Decisions 2 & 9, and Contradiction 5)

### Context & Problem Statement
Heavy AI-driven operations (multi-page PDF parsing, OCR formula extraction, semantic rubric essay evaluation) require 30 to 180 seconds to complete. Executing these operations via synchronous HTTP Request-Response with Server-Sent Events streaming suffers from severe operational hazards:
1. **Infrastructure Timeouts**: Gateways, proxies, and ALBs (Cloudflare, AWS ALB, Nginx) drop idle HTTP connections after 60 to 100 seconds, generating `504 Gateway Timeout` errors.
2. **Kestrel Thread Starvation**: Holding HTTP request threads while waiting for external LLM API calls exhausts server connection pools.
3. **No Rate-Limit Backpressure**: Uncontrolled concurrent HTTP uploads result in bursts of parallel LLM calls, triggering `429 Too Many Requests` errors.

### Decision Drivers
- Complete immunity from HTTP gateway timeouts.
- Backpressure regulation and rate-limit compliance with upstream LLM providers.
- Real-time progress transparency for end users.
- Resilience against client network disconnections.

### Considered Options
- **Option 1: Synchronous HTTP Request-Response with SSE Streaming**:
  - *Pros*: Simpler endpoint implementation.
  - *Cons*: Prone to 504 timeouts; socket exhaustion; zero resilience if the browser tab closes.
- **Option 2: Asynchronous Background Job Processing via .NET 10 Bounded `Channel<T>` + SignalR / SSE**:
  - *Pros*: Immediate `202 Accepted` response (<50ms); backpressure regulated via `BoundedChannelOptions(FullMode.Wait)`; worker concurrency bounded by `SemaphoreSlim`; real-time progress broadcast via SignalR/SSE; persistent job state in database.
  - *Cons*: Requires managing background worker infrastructure and job state transitions.

### Decision Outcome
**Chosen: Option 2**.
1. Long-running AI endpoints accept payloads, create an entry in `background_jobs`, and return `202 Accepted` with a `job_id`.
2. Work is enqueued into a singleton `Channel<AiJobMessage>` configured with `BoundedChannelOptions` (`FullMode = BoundedChannelFullMode.Wait`).
3. An `IHostedService` background worker dequeues tasks, regulates LLM concurrency via `SemaphoreSlim(5)`, updates progress (0%–100%) in `background_jobs`, and broadcasts live status to the client via SignalR Hub / SSE.

### Authoritative References
- Stephen Toub: *An Introduction to System.Threading.Channels* (Microsoft .NET Blog).
- Reactive Streams: *JVM Specification for Asynchronous Stream Processing with Non-blocking Backpressure*.

### Technical Consequences
- **Positive**: Eliminates 504 timeouts; enforces strict LLM rate limits; jobs complete even if the user navigates away or loses connection.
- **Negative**: Client must listen to a SignalR hub or poll `GET /api/v1/jobs/{id}` for completion.
- **Neutral**: Database stores transient background job logs.

### Validation & Verification Rules
- Enqueue 50 concurrent 100-page PDF parsing tasks: confirm all return `202 Accepted` within 50ms, and the system processes them with max 5 concurrent LLM calls without throwing `429` errors.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-017, ADR-021, Product Review Decisions 2 & 9, and Contradiction 5
- **Amendment Rationale**:
  1. *Durable Transactional Outbox (Contradiction 5 & ADR-017)*: In-memory `.NET 10 Channel<T>` is strictly coupled with PostgreSQL `background_jobs`. Tasks are committed to the database with `status = 'queued'` and a unique `idempotency_key` BEFORE returning `HTTP 202 Accepted`. Channel acts purely as a local wake-up dispatcher; surviving worker restarts are guaranteed via 5-minute lease expiration.
  2. *AI Quota & Cost Tracking (Decision 9)*: Endpoints check `user_quotas` before enqueuing. Upon task completion, exact token consumption is written to `ai_usage_ledger`.
  3. *Ingestion Phasing (Decision 2 & ADR-021)*: Phase 1 implements background extraction for text-layer PDFs (PDF.js); heavy OCR for scanned PDFs is deferred to Phase 3 (v1.x).
- **Updated Consequences**:
  - Full crash recovery and persistence for all background AI operations.
  - Strict daily AI usage enforcement and token tracking preventing unbounded API expenditures.
  - Accelerated Phase 1 delivery by scoping initial ingestion to text-layer PDFs.

---

## ADR-010: Hybrid Client-Side FSRS v5 Evaluation with Batch Server Synchronization

### Status
**Amended** (Amended on 2026-10-31 by ADR-020, ADR-022, Product Review Decisions 4 & 12, and Contradiction 3)

### Context & Problem Statement
FSRS v5 (Free Spaced Repetition Scheduler) is a modern memory algorithm modeling retention via Stability ($S$), Difficulty ($D$), and Retrievability ($R$). In high-frequency flashcard review sessions, learners evaluate 50–100 cards consecutively.
- If every rating button press (`Again`, `Hard`, `Good`, `Easy`) requires a synchronous API call, network latency (150–500ms) introduces an unbearable lag, destroying study flow.
- Furthermore, students frequently study during commutes or in environments with unstable internet connectivity (offline study).

### Decision Drivers
- Zero-latency interaction (<16ms, 60fps animation budget, local evaluation $p99 < 5\text{ms}$) when grading cards.
- Complete offline study capability with local persistence.
- Authoritative backend validation and global study history synchronization.
- Support for periodic personal FSRS weight optimization ($w_0 \dots w_{18}$).

### Considered Options
- **Option 1: Pure Server-Side FSRS Evaluation via Synchronous API**:
  - *Pros*: Centralized logic in C#.
  - *Cons*: 200–500ms lag per card; impossible to study offline; massive API request volume.
- **Option 2: Hybrid Client-Side `ts-fsrs` Execution with Batched Asynchronous Backend Synchronization**:
  - *Pros*: Instant local card flips; 100% offline support via IndexedDB; 95% reduction in API overhead; backend retains authoritative historical ledger and runs monthly weight optimization.
  - *Cons*: Business logic shared across TypeScript (`ts-fsrs`) and C# backend validators.

### Decision Outcome
**Chosen: Option 2**.
1. **Client Execution**: During an active review session, due cards are evaluated locally using the official `ts-fsrs` library. Parameters $S, D, R$, and next intervals are computed locally within $p99 < 5\text{ms}$ (governed by the formal Section 13.5 SLO).
2. **Local Queueing**: Review logs are staged in browser memory and backed up to IndexedDB.
3. **Batched Synchronization**: Logs are dispatched asynchronously to the backend (`POST /api/v1/flashcards/sync-reviews`) every 5 cards or upon session completion.
4. **Asymmetric Server-Authoritative Clock Validation & Monotonic Replay**:
   - *Asymmetric Clock Validation*:
     * **Upper Bound Protection**: `client_timestamp <= server_now + 10 minutes` (minor client forward drift tolerance; future-dated reviews are clamped to `server_now` or rejected, preventing artificial exponential stability inflation).
     * **Lower Bound Preservation**: `client_timestamp >= flashcard.last_review_at` AND `client_timestamp >= server_now - 30 days` (maximum offline window). Legitimate past offline study (conducted hours or days ago on flights/subways) is fully accepted with true historical timestamps, preserving elapsed intervals and scheduling accuracy, while strictly enforcing monotonic review log progression. If $t_{\text{review}} < t_{\text{last\_review}}$, reconcile via monotonic sequence replay or flag for review.
   - *Chronological Replay*: Conflicting or out-of-order review logs from multiple devices are ordered deterministically by timestamp and replayed sequentially through the canonical C# FSRS algorithm. The resulting card state ($S, D, \text{due\_at}$) is updated atomically in `flashcards`.
5. **Authoritative Ledger & Optimization**: Backend records immutable historical entries in `flashcard_reviews` and executes periodic Gradient Descent to calibrate individual learner weights ($w_0 \dots w_{18}$).

### Authoritative References
- Jarrett Ye et al. (Open Spaced Repetition): *FSRS v5: Specifications, Mathematics, and Comparison with SM-2*.
- Local-First Software Movement (Martin Kleppmann et al.): *Prioritizing Local Execution and Asynchronous Synchronization*.
- Distributed Systems Time Synchronization Patterns (Lamport Timestamps and Bounded Clock Drift).

### Technical Consequences
- **Positive**: Silky-smooth study experience ($p99 < 5\text{ms}$ local evaluation SLO, $p95 < 200\text{ms}$ batch sync SLO); offline capability; reduced server load; mathematically protected against clock drift manipulation and split-brain sync corruption.
- **Negative**: Timestamps must be validated at the backend to prevent clock skew manipulation.
- **Neutral**: Uses official `ts-fsrs` npm package directly in client React components.

### Validation & Verification Rules
- Disconnect network in browser dev tools: verify student can review 20 cards smoothly with 3D flip animations; reconnect network: verify all 20 logs sync successfully to `flashcard_reviews` with legitimate historical timestamps preserved.
- Simulate client clock skewed 30 days ahead: verify backend clamps timestamp to `server_now` (or rejects), preventing artificial stability inflation.
- Simulate legitimate offline review from 24 hours ago: verify backend accepts `client_timestamp` without clamping to `server_now`, correctly scheduling next due interval from the actual review time.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-020, ADR-022, Product Review Decisions 4 & 12, and Contradiction 3
- **Amendment Rationale**:
  1. *Unified Review Queue Execution (Decision 4 & ADR-020)*: Cards are evaluated from a single unified daily queue, with optional filtering by `tag_id` or `source_id`, completely eliminating deck selection boundaries.
  2. *Latency SLO Realignment (Contradiction 3)*: Replace marketing statements ("0ms instant card flips", "<0.1ms") with formal SLO per Section 13.5: Local evaluation $p99 < 5\text{ms}$ via `ts-fsrs`, background batch sync $p95 < 200\text{ms}$.
  3. *Resilient Online Offline Buffering (Decision 12 & ADR-022)*: Staged review logs remain buffered indefinitely in IndexedDB while offline and flush automatically with asymmetric clock verification upon reconnection.
- **Updated Consequences**:
  - Frictionless interleaved spaced repetition across all subject areas.
  - Statistically rigorous, verifiable latency SLOs ($p99 < 5\text{ms}$ local evaluation, $p95 < 200\text{ms}$ batch sync).
  - Robust offline active recall study with automatic synchronization upon network reconnection.

---

## ADR-011: Three-Mode Adaptive Workspace UX Architecture (Reading, Split, Review) with GSAP Micro-Interactions

### Status
**Amended** (Amended on 2026-10-31 by ADR-019, ADR-020, ADR-021, and Product Review Decisions 4, 5, 11, 13, 14)

### Context & Problem Statement
Educational workflows encompass three distinct cognitive phases: deep reading, note synthesis, and active recall testing. Forcing these divergent activities into a single cluttered interface causes cognitive overload, whereas splitting them into disconnected pages creates navigation friction and loss of study context.

### Decision Drivers
- Distinct visual affordances tailored to specific cognitive tasks.
- Preservation of context across mode switches.
- Delightful, high-performance micro-interactions (60–120fps) without memory leaks.
- Full keyboard accessibility (WAI-ARIA compliance).

### Considered Options
- **Option 1: Monolithic Single-View Interface with Modals**:
  - *Pros*: Fewer layouts to maintain.
  - *Cons*: Cluttered; high cognitive distraction; cramped on 1080p displays.
- **Option 2: Three-Mode Adaptive Workspace (Reading, Split, Review) Powered by GSAP `@gsap/react`**:
  - *Pros*: Purpose-built cognitive layouts; smooth animated transitions; ergonomic 1080p utilization; memorable micro-interactions (3D card flips, highlight pulse, celebration confetti).
  - *Cons*: Requires rigorous animation lifecycle management.

### Decision Outcome
**Chosen: Option 2**.
1. **Reading Mode**: Distraction-free 68ch centered column, 5-color floating selection bar, collapsible TOC, and optimal typography (`text-[1.125rem]`, `leading-[1.8]`).
2. **Split Workspace Mode**: 15/45/40 layout via `react-resizable-panels`, anchor-based synchronized scrolling, bidirectional link badges, and scroll-sync mutex flag (`isSyncing`) with 50ms cooldown and trailing-edge dispatch (`pendingAnchorId`) to eliminate infinite recursive scrolling loops without dropping final resting scroll positions. Total panel width calculates as `calc(100vw - 8px)` to prevent 8px horizontal overflow.
3. **Review & Practice Mode**: Centered 3D flip card canvas ($800\times 520\text{px}$) with FSRS rating controls, cloze testing, and semantic rubric evaluation. Active test attempts and editor drafts are preserved in persistent Zustand stores across mode transitions.
4. **Design Tokens & Components**: Standardized on TailwindCSS v4 native directive `@theme` (font families, semantic highlight colors, FSRS rating buttons, 3D shadows, radius tokens) and Shadcn/ui + Radix UI accessible component tree.
5. All micro-interactions are managed via GSAP and `@gsap/react` (`useGSAP`), guaranteeing proper animation cleanup and hardware-accelerated transforms.

### Authoritative References
- John Sweller: *Cognitive Load Theory in Educational Software Design*.
- GreenSock: *GSAP with React Guidelines and Lifecycle Management (`@gsap/react`)*.
- TailwindCSS v4 Specification: *CSS-First Configuration via `@theme` Directives*.

### Technical Consequences
- **Positive**: Fluid user experience; sharp cognitive distinction between reading, writing, and testing; high student engagement; zero memory leaks.
- **Negative**: Adds ~40KB to the frontend bundle for GSAP core and plugins.
- **Neutral**: Active mode is persisted in global Zustand state and URL query params.

### Validation & Verification Rules
- Cycle through all 3 modes 50 times in succession: verify no animation memory leaks or lingering DOM element references.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-019, ADR-020, ADR-021, and Product Review Decisions 4, 5, 11, 13, 14
- **Amendment Rationale**:
  1. *Reading Mode (Decision 13 & 14)*: Selection popover provides 5 fixed colors mapped to `color_key` tokens with user-defined semantic labels (Decision 13). Orphaned highlight banner and 3-action slide-over integrated (Decision 14).
  2. *To Flashcard Interaction (Decision 5)*: Converting highlighted text to a card always displays an in-situ draft confirmation popover. Default mode is Cloze deletion (0 AI cost); AI Q&A card generation is an optional toggle.
  3. *Review Mode (Decision 4)*: Displays the Unified Review Queue with tag filter pills instead of a deck selector.
  4. *Lean Scope Execution (Decision 11 & ADR-021)*: Simplifies GSAP animations to essential workspace transitions and 3D card flips; defers complex physics particle confetti to v1.x.
- **Updated Consequences**:
  - WCAG-compliant semantic highlighting and dark-mode safe styling.
  - Zero accidental AI billing or poor-quality flashcard creation via mandatory draft popover.
  - Streamlined frontend implementation feasible for a 1–2 developer team.

---

## ADR-012: Core PKM Feature Expansion and Phased Delivery Roadmap

### Status
**Amended** (Amended on 2026-10-31 by ADR-021 and Product Review Decisions 2, 6, 11)

### Context & Problem Statement
Benchmarking against industry leaders (Readwise Reader, RemNote, Obsidian, Notion) reveals critical feature gaps across 8 functional domains (Workspace & Layout, Reading & Highlighting, Note-taking & PKM, Spaced Repetition, Assessment, Search & Navigation, Interoperability, and Audio). 

However, attempting to build all features simultaneously risks delivery paralysis and technical debt. Furthermore, scheduling long-running AI grading before establishing background queue infrastructure creates severe dependency inversions.

### Decision Drivers
- High-impact educational value delivery in early phases.
- Strict logical technical dependency progression (infrastructure -> PKM -> background jobs -> AI -> ecosystem).
- Disciplined sprint execution (16 Sprints, 4 Phases, 8 Months).
- Elimination of dependency inversions between async queues and AI pipelines.

### Considered Options
- **Option 1: Unstructured Big-Bang Release**:
  - *Pros*: None.
  - *Cons*: Severe delivery risks; uncoordinated dependencies; unmaintainable code releases.
- **Option 2: Phased 4-Stage Architectural Roadmap with Strict Infrastructure-First Sequencing**:
  - *Pros*: Clear milestones; strict dependency ordering; risk-hedged incremental delivery; delivers `.NET 10 Channel<T>` in Sprint 9 before AI rubric essay grading in Sprint 10.
  - *Cons*: Advanced features (TTS, deep analytics) are intentionally scheduled in later phases.

### Decision Outcome
**Chosen: Option 2**.
Execute the 8-month, 16-sprint roadmap structured into 4 distinct phases with strict dependency ordering:
- **Phase 1 (Months 1-2 | Sprints 1-4)**: *MVP Hardening & Core Workspace Stability*. Resolves all 7 architectural bottlenecks, implements PostgreSQL 17 DDL with HNSW, unifies Tiptap JSON AST, builds 1920x1080 Split Workspace with scroll mutex, and integrates core FSRS v5 engine.
- **Phase 2 (Months 3-4 | Sprints 5-8)**: *Advanced PKM, Bi-directional Linking & Sync*. Command Palette (`cmd+k`), `[[Wikilinks]]`, Backlinks engine, GSAP micro-interactions, and full-fidelity Anki/Markdown export.
- **Phase 3 (Months 5-6 | Sprints 9-12)**: *Background Jobs, Multi-Tier Assessments & AI Copilot*. 
  - *Sprint 9*: .NET 10 Bounded `Channel<T>` background job infrastructure, `SemaphoreSlim(5)` throttling, and SSE progress streaming.
  - *Sprint 10*: `practice_quizzes` and `practice_quiz_items` entity models, interactive quiz/cloze UI, and AI semantic rubric essay grading pipeline (directly leveraging Sprint 9's background queue).
  - *Sprint 11*: Text-to-Speech (TTS) Karaoke Audio Reader with real-time active word highlighting and Web Speech API sync.
  - *Sprint 12*: High-throughput PDF parsing, multi-page OCR, and document ingestion pipeline running on Channel infrastructure.
- **Phase 4 (Months 7-8 | Sprints 13-16)**: *Study Analytics, Ecosystem & Offline Hardening*. 365-day GitHub streak heatmap, empirical forgetting curves, PWA offline caching with mutation queues, and v1.0 GA release.

### Authoritative References
- Agile Alliance: *Principles of Incremental Value Delivery and Architectural Enablers*.
- Competitive Benchmark Analysis (Readwise, RemNote, Obsidian, Notion, MarginNote 4, Heptabase).

### Technical Consequences
- **Positive**: Predictable engineering velocity; clear sprint acceptance criteria; manageable cognitive load for engineering teams; eliminates dependency inversions.
- **Negative**: Requires strict product discipline to avoid feature creep in early sprints.
- **Neutral**: Roadmap is reviewed at the conclusion of each 2-month phase.

### Validation & Verification Rules
- Verify completion of Phase 1 sprint deliverables before initiating Phase 2 features. Ensure each phase passes its dedicated integration test suite.
- Verify that Sprint 9 background queue is fully operational before executing Sprint 10 AI rubric evaluation jobs.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-021 and Product Review Decisions 2, 6, 11
- **Amendment Rationale**:
  - The 16-sprint, 8-month roadmap is completely superseded by the **Lean MVP Roadmap (8–10 Sprints, 4–5 Months, 1–2 Devs)** detailed in ADR-021.
  - PDF text-layer ingestion is elevated to Phase 1 (Sprints 1–3).
  - Formal Exams, OCR, TTS Karaoke, Community Shared Decks, 365-day Heatmaps, full PWA offline sync, and Anki `.apkg` export are formally transferred to the **v1.x Phased Backlog**.
- **Updated Consequences**:
  - Clear, achievable delivery trajectory for a 1–2 developer team.
  - High focus on the core reading-highlighting-notetaking-FSRS retention loop.

---

## ADR-013: Best-of-Breed Educational Workspace Synthesis over Single-Platform Emulation

### Status
**Amended** (Amended on 2026-10-31 by ADR-020, ADR-021, and Contradiction 2)

### Context & Problem Statement
Educational software tools in the current market are deeply specialized yet fundamentally isolated:
1. **Readwise Reader** excels at multi-format reading, triage, and in-context Ghostreader, but lacks synthesis note-taking and deep spaced repetition.
2. **RemNote** excels at rapid flashcard generation from bullet lists via `::`, but its rigid outliner hierarchy alienates learners who need free-form writing, tables, or essays.
3. **Obsidian** offers unmatched local-first sovereignty and bi-directional graph linking (`[[wikilinks]]`), but requires users to manually assemble brittle community plugins for PDF annotation and SRS.
4. **Notion** provides an intuitive block-based user experience, but its normalized database model causes extreme write amplification, row deadlocks, and latency on large documents, and it lacks offline capabilities.
5. **MarginNote 4** pioneers the "Cards as the Axis" paradigm and PDF bounding-box extraction, but is locked into Apple's proprietary ecosystem (macOS/iPadOS) and lacks free-form synthesis documents.
6. **Heptabase** excels at visual spatial sense-making on 2D whiteboards, but entirely lacks active recall / spaced repetition algorithms.

Attempting to clone any single platform would inherit that platform's fatal architectural flaws. SWMS required a strategic decision on architectural synthesis versus single-platform emulation.

### Decision Drivers
- Creation of a frictionless, unified study flow: Reading $\to$ Highlighting $\to$ Note Synthesis $\to$ Spaced Repetition $\to$ Assessment.
- Data sovereignty and open export formats (Anki, Markdown, PDF).
- Web-first cross-platform accessibility (Windows, macOS, Linux, Web, Mobile).
- Auto-save latency governed by $p95 < 50\text{ms}$ ($p99 < 150\text{ms}$ SLO) and zero database lock deadlocks.

### Considered Options
- **Option 1: Single-Platform Emulation (e.g., Building an "Open-Source Notion" or "Web MarginNote")**:
  - *Pros*: Clear product blueprint to copy.
  - *Cons*: Inherits severe structural flaws (e.g., Notion's block deadlock bottleneck; MarginNote's complex mind-map lock-in); fails to solve the cross-category fragmentation problem.
- **Option 2: Best-of-Breed Educational Workspace Synthesis**:
  - *Pros*: Combines the proven strengths of all 6 industry leaders into a cohesive, zero-friction workflow while deliberately re-engineering weak points using modern technologies (PostgreSQL 17 JSONB, Next.js 15, Tiptap, FSRS v5).
  - *Cons*: Requires higher architectural rigor to harmonize divergent interaction models into a single coherent system.

### Decision Outcome
**Chosen: Option 2**.
SWMS executes an authoritative, best-of-breed synthesis:
1. **From MarginNote 4**: Implement the **"Cards as the Axis"** paradigm. Highlights in Lessons and PDFs are first-class knowledge atoms projecting into reader anchors, notebook citation cards, and FSRS flashcards via stable `annotation_id` and `ON DELETE SET NULL`.
2. **From RemNote**: Implement **0-friction flashcard generation** using native Tiptap slash commands (`/flashcard`) and inline syntax, eliminating the overhead of external flashcard tools.
3. **From Readwise Reader**: Implement high-speed ingestion triage and **context-bound in-situ AI actions** (selection popup with 5 semantic highlight colors and dedicated AI prompt popovers).
4. **From Obsidian**: Implement **Bi-directional linking (`[[wikilinks]]`)**, block transclusion via `ref_block_id`, and full data sovereignty (exporting to Anki `.apkg` and standard Markdown with YAML frontmatter).
5. **From Notion**: Preserve the intuitive block feel and slash command editing on the frontend, while **strictly rejecting Notion's relational row-per-block database model** in favor of a single compressed JSONB document (`content_ast`), achieving auto-save latency governed by $p95 < 50\text{ms}$ ($p99 < 150\text{ms}$ SLO).
6. **From Heptabase**: Standardize notes on atomic, modular document structures with embedded citation cards, establishing the data foundation for future Phase 4 visual canvas expansions.

### Authoritative References
- SWMS Market & Competitive Research Report (`explorer_market_1/report.md`).
- SWMS UI/UX Specification & Design Tokens Report (`explorer_uiux_1/report.md`).
- MarginNote 4 Architecture Analysis: *Cards as the Axis of Academic Reading*.
- Jarrett Ye et al.: *Open Spaced Repetition (FSRS v5)*.

### Technical Consequences
- **Positive**: Market-defining product positioning; completely unifies the fragmented student workflow; eliminates cognitive switching costs; guarantees high database throughput and reliability.
- **Negative**: Requires strict UI consistency guidelines to prevent feature bloat across modes.
- **Neutral**: The three workspace modes (Reading, Split, Review) cleanly separate cognitive states.

### Validation & Verification Rules
- Verify that a student can read a lesson, highlight text, create a flashcard in 1 click, reference the highlight in a notebook with deep link, and review the card in FSRS mode without switching apps.
- Verify that exporting data generates both an Anki `.apkg` deck and an Obsidian-compatible Markdown vault with functional YAML frontmatter and `[[wikilinks]]`.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-020, ADR-021, and Contradiction 2
- **Amendment Rationale**:
  1. *RemNote Synthesis*: Flashcard generation uses inline cloze and draft popovers organized into a unified review queue via tags, bypassing rigid folder/deck hierarchies.
  2. *Data Sovereignty*: Full Markdown export with YAML frontmatter is delivered in v1.0; Anki `.apkg` export is deferred to v1.x.
  3. *Auto-Save SLO*: The Notion database comparison is amended: auto-save latency is governed by measurable SLO $p95 < 50\text{ms}, p99 < 150\text{ms}$ rather than unverified "sub-20ms" claims.
- **Updated Consequences**:
  - Realignment with tag-based unified spacing practice.
  - Focus on standard open Markdown data portability for v1.0.
  - Standardized performance benchmarks.

---

## ADR-014: Multi-Tenant Vector Search Security, PDF Spatial Metadata, and Learner-Specific FSRS 19-Parameter Persistence

### Status
**Amended** (Amended on 2026-10-31 by ADR-019, Product Review Decisions 1 & 2, and Contradictions 6 & 7)

### Context & Problem Statement
Auditing the v2.0.0 PostgreSQL DDL and domain specifications revealed three interrelated persistence and security defects:
1. **Multi-Tenant Vector Search Degradation**: `notebook_embeddings` lacked a `user_id` column. Searching a student's personal notes required an inner join on `notebooks`. Under pgvector HNSW, graph search traverses vectors globally prior to evaluating relational join filters. Filtering on a single user's small subset causes severe recall collapse or forces full table scans.
2. **Missing PDF Spatial Bounding-Box Schema**: While the narrative emphasized PDF.js integration and spatial bounding boxes ("Cards as the Axis"), the relational schema lacked a `documents` entity, and `annotations` only supported character offsets (`start_offset`, `end_offset`) bound to `lessons(id)`.
3. **Unpersisted FSRS Personalization Weights**: Decision J and ADR-010 mandated optimizing user-specific 19-parameter vectors ($w_0 \dots w_{18}$) via gradient descent, but provided no relational storage entity for persisting calibrated weights per user.

### Decision Drivers
- Strict multi-tenant data privacy and 100% recall on personal semantic vector search.
- First-class support for textbook PDF ingestion and spatial coordinate annotations.
- Full database persistence for personalized FSRS v5 spaced repetition weights.

### Considered Options
- **Option 1: Relational Joins and JSON Config Blobs**:
  - *Pros*: Minimizes new table definitions.
  - *Cons*: HNSW post-filtering recall collapse; poor query planning; unvalidated JSON structures for mathematical weight arrays.
- **Option 2: Dedicated Schema Remediation Entities**:
  - *Pros*: Denormalizes `user_id` onto `notebook_embeddings` with composite indexing; creates a first-class `documents` table; implements polymorphic `annotations` with spatial rect arrays; provides a dedicated `user_fsrs_parameters` entity.
  - *Cons*: Requires running the v2.0.1 migration script.

### Decision Outcome
**Chosen: Option 2**.
1. **Multi-Tenant Embeddings**:
   - Denormalize `user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE` onto `notebook_embeddings`.
   - Add a composite B-tree index `CREATE INDEX idx_notebook_embeddings_user_notebook ON notebook_embeddings(user_id, notebook_id);`. Personal searches route through this index for 100% recall.
2. **Document Management & Spatial Annotations**:
   - Introduce `documents(id, course_id, uploader_id, title, file_url, file_size_bytes, total_pages, mime_type, ocr_status VARCHAR(30) NOT NULL DEFAULT 'pending')`.
   - Make `lesson_id` and `block_id` nullable in `annotations`, and add `document_id UUID REFERENCES documents(id) ON DELETE CASCADE`, `page_number INT`, and `bounding_boxes JSONB` with constraint `chk_annotation_target`.
3. **Learner FSRS Parameters**:
   - Introduce `user_fsrs_parameters(user_id, weights FLOAT[], desired_retention, maximum_interval_days, optimized_at)`.
4. **Quiz Item Immutability & Snapshots**:
   - In `practice_quiz_items`, maintain immutable prompt and solution structures (`prompt_ast JSONB NOT NULL`, `choices JSONB`, `cloze_solution TEXT`) to ensure review fidelity even if curriculum source material evolves.

### Technical Consequences
- **Positive**: Strict tenant isolation; zero vector recall collapse; full spatial PDF annotation support; durable storage of trained FSRS memory parameters; immutable quiz histories.
- **Negative**: Requires handling polymorphic target branches in the annotation API.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-019, Product Review Decisions 1 & 2, and Contradictions 6 & 7
- **Amendment Rationale**:
  1. *Default OCR Status (Contradiction 6)*: Fixes `documents.ocr_status` default value to `'pending'` (was `'completed'`). Newly uploaded documents enter `'pending'` state until background processing initiates.
  2. *Personal Workspace Ingestion (Decision 1 & ADR-019)*: `documents.course_id` references the user's private personal container (`courses.visibility = 'personal'`), allowing self-learners to upload and annotate documents without joining institutional courses.
  3. *Annotation State (Contradiction 7)*: Confirms `annotations.status` as `'anchored' | 'needs_review' | 'orphaned' | 'archived'` with redundant `is_orphaned` boolean dropped.
- **Updated Consequences**:
  - Consistent asynchronous lifecycle tracking for uploaded documents.
  - Immediate onboarding for independent learners without mock course hierarchies.
  - Clean annotation state machine without duplicate status flags.

---

## ADR-015: User Flow, Navigation Topology, and Contextual View Interlinking Architecture

### Status
**Amended** (Amended on 2026-10-31 by ADR-019, ADR-020, and Product Review Decisions 1, 4, 8, 10)

### Context & Problem Statement
Educational platforms frequently suffer from severe user experience fragmentation:
1. **Modal Interruption Traps**: Opening full-screen modals to create flashcards or notes jolts learners out of deep reading flow.
2. **Context Loss on Cross-Referencing**: Clicking a reference link in a notebook often navigates to another page entirely, causing the student to lose their place in their synthesis notes.
3. **Split-Screen Ergonomic Friction**: Fixed-width split panes cause severe horizontal scrolling on laptops and wasted empty margins on ultra-wide displays.
4. **Synchronization Jumps**: Naive scroll synchronization between dual panes causes jarring jumps when scrolling across large sections without highlights.

### Decision Drivers
- Unbroken reading and synthesis flow (Zero-Friction In-Situ Interactions).
- Seamless bidirectional traversal between curriculum source text and personal notes.
- Optimal screen real estate utilization on standard 1080p displays with graceful responsive degradation.
- Zero loss of active work (draft notes, exam inputs) when switching modes.

### Considered Options
- **Option 1: Traditional Multi-Page Document Model with Modal Overlays**:
  - *Pros*: Familiar web development paradigm.
  - *Cons*: High cognitive load; destroys study momentum; frequent context switching.
- **Option 2: Unified Adaptive Workspace with Contextual In-Situ Linking and Scroll Mutex**:
  - *Pros*: Three dedicated cognitive modes (Reading, Split Workspace, Review/Practice); in-situ non-blocking popovers; smooth dual-pane scroll synchronization with proportional interpolation fallback; global command palette (`cmd+k`).
  - *Cons*: Requires sophisticated client-side view state orchestration via Zustand and GSAP.

### Decision Outcome
**Chosen: Option 2**.
SWMS implements a cohesive 5-tier navigation and view-linking topology:
1. **Three Primary Cognitive Modes**:
   - **Mode 1: Reading Mode**: Centered 68–75ch typography column, collapsible TOC rail (15.6%), floating tool rail, and non-blocking selection popover.
   - **Mode 2: Split Workspace Mode**: 15% Mini-Rail / 45% Reader / 40% Notebook with `react-resizable-panels`, `calc(100vw - 8px)` boundary math, and bidirectional citation cards.
   - **Mode 3: Review & Practice Mode**: 3D active recall flashcard canvas with FSRS v5 ratings and multi-tier exam assessment portal.
2. **Contextual Linking Engine**:
   - Clicking `[↗ Jump to Source]` in a Notebook citation card smoothly scrolls the Reader pane to the exact block using GSAP (`scrollToAndPulseHighlight`) without triggering reverse scroll sync.
   - Creating a flashcard from selected text occurs via an **in-situ popover (< 200ms)** directly above the selection, auto-extracting definitions via fast-path AI without modal interruption.
3. **Dual-Pane Synchronized Scrolling with Proportional Fallback**:
   - Uses `useSynchronizedScroll` with a mutual exclusion lock (`isSyncing`) and trailing-edge dispatch (`pendingAnchorId`).
   - When between two anchors, the inactive pane interpolates proportionally:
     $$\text{TargetScrollRatio} = \text{Anchor}_A + \left(\frac{\text{CurrentPos} - \text{Pos}_A}{\text{Pos}_B - \text{Pos}_A}\right) \times (\text{Anchor}_B - \text{Anchor}_A)$$
     Eliminates nearest-neighbor clamping jumps.
4. **Global Command Palette (`cmd+k`)**:
   - Instant search across lessons, uploaded PDF documents, personal notes, and flashcards using hybrid local fuzzy matching + pgvector HNSW search.
5. **Persistent State Isolation**:
   - Route state synchronizes active view parameters (`/courses/:courseSlug/lessons/:lessonSlug?mode=split&annotationId=...`).
   - Active quiz answers and unsaved notebook keystrokes are preserved in persisted Zustand stores, preventing data loss during mode transitions or accidental reloads.

### Authoritative References
- Nielsen Norman Group: *Ergonomics of Split-Pane Interfaces and Text Line Length Limits*.
- W3C Web Accessibility Initiative (WAI): *Keyboard Navigation in Split-Screen Web Applications*.
- SWMS UI/UX Specification & Micro-Interactions Blueprint (`explorer_uiux_1/report.md`).

### Technical Consequences
- **Positive**: Exceptional cognitive flow; 0 context loss; fluid 1080p utilization; eliminates scroll ping-pong loops and visual jump discontinuities; production-ready design.
- **Negative**: Requires disciplined testing of responsive layout collapse across viewports (<1440px and <1024px).
- **Neutral**: State transitions are handled client-side with minimal API round-trips.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-019, ADR-020, and Product Review Decisions 1, 4, 8, 10
- **Amendment Rationale**:
  1. *Personal Workspace First Navigation (Decision 1 & ADR-019)*: Primary routes default to `/workspace` (dashboard with onboarding checklist and recent items), `/reader` (reading & split workspace), and `/review` (unified FSRS queue with tag filter rail). Course management (`/courses/...`) is an optional secondary hierarchy.
  2. *Review Flow (Decision 4 & ADR-020)*: Review Mode navigation eliminates deck selection screens; users directly launch review sessions from the global queue or select tags/source filters.
  3. *Onboarding Flow (Decision 10)*: Initial entry loads a sample Vietnamese document with a 3-step activation checklist (Highlight text $\to$ Create cloze card $\to$ Complete first review).
- **Updated Consequences**:
  - Drastic reduction in initial onboarding drop-off via guided sample study loop.
  - Unified navigation topology prioritizing individual self-study.

---

## ADR-016: BFF Cookie-Based Session Architecture and Encrypted Refresh Token Rotation

### Status
**Accepted**

### Context & Problem Statement
The original specification stated that PostgreSQL stores only a SHA-256 hash of refresh tokens (`token_hash = SHA256(raw_token)`), yet claimed the server would return the plaintext replacement token if an incoming request fell within a 15–30s grace window. Cryptographically, a one-way hash cannot be reversed to produce the plaintext token. Furthermore, storing access and refresh tokens in JavaScript memory fails to coordinate across independent browser tabs without complex, brittle messaging, while storing tokens in LocalStorage introduces catastrophic XSS theft vectors.

### Decision Drivers
- Absolute cryptographic viability of idempotent retry recovery.
- Protection against Cross-Site Scripting (XSS) and token exfiltration.
- Frictionless multi-tab session management without memory synchronization race conditions.
- Strict RFC 6819 & OWASP compliance for Refresh Token Rotation.

### Considered Options
- **Option 1: In-Memory Client Storage with Web Locks and Plaintext Hash-Only DB**:
  - *Pros*: Simple frontend setup.
  - *Cons*: Cryptographically impossible to return replacement token from a hash; broken multi-tab experience; state lost on tab refresh.
- **Option 2: Backend-For-Frontend (BFF) HttpOnly Cookies with Server-Side Encrypted Replacement Tokens**:
  - *Pros*: Immune to JavaScript XSS exfiltration; browser handles multi-tab cookie transmission automatically; server securely decrypts `encrypted_replacement_token` using AES-256-GCM during the 15–30s grace window.
  - *Cons*: Requires CSRF protection (`SameSite=Strict` + double-submit cookie or origin validation header).

### Decision Outcome
**Chosen: Option 2**.
1. **Cookie Scoping**: Refresh tokens are transmitted exclusively via `HttpOnly`, `Secure`, `SameSite=Strict` cookies scoped to `/api/v1/auth/refresh`.
2. **Encrypted Replacement Persistence**: The `refresh_tokens` table stores `token_hash = SHA256(raw_token)` alongside `encrypted_replacement_token BYTEA`, encrypted with the backend server's AES-256-GCM master key.
3. **Idempotent Grace Recovery**: When a concurrent tab or flaky mobile connection presents an already-revoked token within 15–30s, the server decrypts and returns the existing replacement token pair, avoiding false-positive session genocide.
4. **Replay Breach Detection**: Receiving a revoked token outside the 30s grace window increments `users.token_version` and invalidates all tokens in the `family_id`.

---

## ADR-017: Transactional Database-Backed Background Job Queue with Local In-Memory Dispatch

### Status
**Amended** (Amended on 2026-10-31 by Product Review Decision 9 and Contradiction 5)

### Context & Problem Statement
Relying exclusively on .NET 10 `Channel<T>` for asynchronous AI processing (PDF OCR, semantic rubric essay evaluation) introduces severe durability defects:
1. **Crash Data Loss**: If Kestrel crashes or the pod restarts, all enqueued jobs in RAM are permanently lost.
2. **Backpressure Deadlocks**: Configuring `FullMode.Wait` blocks incoming HTTP request threads when the channel is full, violating the SLA commitment to return `202 Accepted` within 50ms.
3. **Lack of Worker Leasing & Retries**: Multi-pod environments cannot coordinate job claims or detect stalled workers.

### Decision Drivers
- 100% durability: Zero job loss across server restarts.
- Immediate non-blocking API response (`202 Accepted` with `job_id`).
- Distributed worker coordination with leasing, timeouts, and exponential backoff.
- Graceful backpressure handling (HTTP 429/503).

### Considered Options
- **Option 1: Pure In-Memory `Channel<T>`**:
  - *Pros*: Zero database I/O overhead.
  - *Cons*: Total job loss on crash; request thread blocking; untracked status.
- **Option 2: PostgreSQL Transactional Outbox Queue with In-Memory Channel Notification**:
  - *Pros*: 100% durable persistence; workers claim jobs via `SELECT ... FOR UPDATE SKIP LOCKED`; in-memory channel acts as a zero-latency local wake-up dispatcher; automatic worker crash recovery via lease expiration.
  - *Cons*: Small database write overhead per job.

### Decision Outcome
**Chosen: Option 2**.
1. **Durable Persistence**: Endpoints transactionally commit the job to `background_jobs` with status `queued` and a unique `idempotency_key` BEFORE returning `202 Accepted`.
2. **Worker Leasing Protocol**:
   Workers claim jobs using row-level locking:
   ```sql
   SELECT id, payload FROM background_jobs 
   WHERE status = 'queued' AND (lease_expires_at IS NULL OR lease_expires_at <= NOW())
   ORDER BY priority DESC, created_at ASC 
   FOR UPDATE SKIP LOCKED LIMIT 1;
   ```
   Claimed jobs are marked `leased` with `lease_expires_at = NOW() + INTERVAL '5 minutes'`. If a worker dies, orphaned jobs become eligible for reclamation upon lease expiry.
3. **Backpressure**: When active queued jobs exceed system capacity (e.g., 500 tasks), endpoints immediately return `429 Too Many Requests` or `503 Service Unavailable` with `Retry-After: 60`.

### Amendment Notes (2026-10-31)
- **Amended by**: Product Review Decision 9 and Contradiction 5
- **Amendment Rationale**:
  1. *Durable Channel Dispatch*: Reaffirm that `background_jobs` table is the authoritative persistence layer for all asynchronous AI tasks, integrated with Sprint 9 in the lean roadmap.
  2. *Pre-Enqueue Quota Check (Decision 9)*: Job insertion checks `user_quotas`. If daily limits are exceeded, request rejects with `HTTP 429 Too Many Requests`. Token consumption is recorded in `ai_usage_ledger` upon job completion.
- **Updated Consequences**:
  - Complete operational durability and multi-pod safety.
  - Built-in cost governance and prevention of runaway AI expenses.

---

## ADR-018: FSRS v5 Canonical Event Sourcing, Idempotent Reviews, and Golden Test Vectors

### Status
**Amended** (Amended on 2026-10-31 by ADR-020, ADR-022, and Product Review Decisions 4 & 12)

### Context & Problem Statement
The hybrid FSRS architecture evaluates card flips client-side via `ts-fsrs` and synchronizes to a .NET 10 backend. Without strict event schemas and mathematical parity tests:
1. Network retries duplicate review entries, artificially accelerating stability.
2. Divergent rounding, fuzz, or timezone logic between TypeScript and C# causes calculation drift.
3. Review log cascade deletion destroys empirical datasets required for 19-parameter weight calibration.

### Decision Drivers
- Strict review log idempotency on mobile retries.
- Mathematical calculation parity between frontend and backend engines.
- Permanent preservation of review logs against accidental cascade deletion.

### Considered Options
- **Option 1: Direct State Push (Client Overwrites Stability/Difficulty)**:
  - *Pros*: Simple client-server contract.
  - *Cons*: Vulnerable to clock tampering, split-brain state, and total corruption of spaced repetition history.
- **Option 2: Canonical Event Sourcing with Idempotency Keys and Golden Vectors**:
  - *Pros*: Client submits immutable events with unique `review_event_id`; database enforces `ON DELETE RESTRICT` and composite owner constraints; calculation parity verified via golden test vectors.
  - *Cons*: Requires server-side sequence replay engine for out-of-order logs.

### Decision Outcome
**Chosen: Option 2**.
1. **Pinned Algorithm**: Pinned to **FSRS v5 (19 parameters)**.
2. **Idempotent Event Submission**: Every review generated by `ts-fsrs` carries a client-generated UUID `review_event_id`. The database table `flashcard_reviews` enforces `UNIQUE(review_event_id)` and `ON DELETE RESTRICT` via composite key `(flashcard_id, user_id)`.
3. **Golden Test Suite**: A shared JSON fixture of 1,000 review sequences is executed in automated test suites for both TypeScript and C#, guaranteeing identical intervals and stability scores to 4 decimal places.
4. **Out-of-Order Reconciliation**: Offline batches with reviews older than `card.last_review_at` are replayed chronologically through the canonical C# FSRS engine to recalculate authoritative card states.

### Amendment Notes (2026-10-31)
- **Amended by**: ADR-020, ADR-022, and Product Review Decisions 4 & 12
- **Amendment Rationale**:
  1. *Deck Independence (Decision 4 & ADR-020)*: Reaffirm that `flashcard_reviews` has zero foreign key dependency on any `deck_id`; reviews are bound exclusively to `(flashcard_id, user_id)`.
  2. *Replay Engine on Unified Queues*: Replay engine operates seamlessly on unified queues: batch reconciliation processes reviews chronologically regardless of how cards were tagged or filtered during study sessions.
- **Updated Consequences**:
  - Mathematical integrity and event-sourced replay work across unified, tag-filtered, and source-filtered review flows.
  - Strict preservation of the immutable empirical review event log for FSRS optimization.

---

## ADR-019: Personal Workspace First and Hybrid Persona Architecture

### Status
**Accepted** (Supersedes institutional-only course assumptions across ADR-004, ADR-011, ADR-014, ADR-015)

### Context & Problem Statement
Early SWMS architectural baselines strictly enforced an institutional Learning Management System (LMS) hierarchy:
$$\text{Course} \longrightarrow \text{Chapter} \longrightarrow \text{Lesson}$$
In this model, curriculum documents and uploaded PDFs were required to belong to a registered course (`documents.course_id NOT NULL`), and annotations and notes were predominantly scoped to enrolled lessons.

However, market benchmarking and user analysis demonstrate that the core early adopters of modern PKM and SRS tools (such as Obsidian, Readwise Reader, and RemNote) are **self-directed learners, researchers, and knowledge workers**. Requiring an independent learner to create a dummy "Course" and "Chapter" merely to upload a textbook PDF or write a personal synthesis note creates severe onboarding friction and product mismatch. Conversely, completely eliminating courses would destroy academic institutional sales and collaborative classroom use cases.

### Decision Drivers
- Zero-friction onboarding: An independent learner must be able to sign up and start reading/annotating a PDF or note in $<30$ seconds.
- Unified data model: Support both self-directed personal study and institutional instructor-led courses without maintaining two divergent database schemas.
- Strict multi-tenant privacy: Personal documents and notes must remain strictly confidential to the individual learner, completely isolated from course instructors or institutional administrators.

### Considered Options
- **Option 1: Strictly Institutional Course LMS**:
  - *Pros*: Simple, rigid hierarchical DDL.
  - *Cons*: Total failure for self-learners; alienates independent knowledge workers.
- **Option 2: Bifurcated Dual-Engine Architecture (Separate "Personal Workspace" and "LMS Course" Services)**:
  - *Pros*: Clean separation of domain logic.
  - *Cons*: High code duplication; fragmented tables (`personal_documents` vs `course_documents`); double maintenance overhead for 1–2 developers.
- **Option 3: Hybrid Persona with Personal Workspace First (Container Generalization)**:
  - *Pros*: Unified schema where `courses` acts as a generic container with `visibility` classification (`personal`, `private`, `public`); personal workspace is provisioned automatically upon user creation; course tools are unlocked via persona toggle without schema bifurcation.
  - *Cons*: Requires UI routing to seamlessly distinguish personal versus course navigation contexts.

### Decision Outcome
**Chosen: Option 3**.
1. **Default Personal Workspace**: Every registered user is automatically provisioned with a private root container in `courses`:
   ```sql
   ALTER TABLE courses ADD COLUMN visibility VARCHAR(20) NOT NULL DEFAULT 'personal' 
       CHECK (visibility IN ('personal', 'private', 'public'));
   ```
   When an independent learner uploads a document or creates a notebook outside of an enrolled course, it is automatically assigned to their personal container (`visibility = 'personal'`, `instructor_id = user_id`).
2. **Navigation Topology**: The primary web application layout defaults to the **Personal Workspace** (`/workspace/dashboard`, `/workspace/reader`, `/workspace/review`). Instructor course authoring (`/instructor/courses`) is an expandable secondary module available to authorized personas.
3. **Privacy Isolation**: Institutional instructors and organization admins have zero read permissions over records where `courses.visibility = 'personal'`, enforced via PostgreSQL row security and API authorization filters.

### Authoritative References
- SWMS Product Review Decision 1 (`product_review_decisions.md`).
- Notion & Obsidian Information Architecture: *Personal Private Spaces vs Shared Team Workspaces*.

### Technical Consequences
- **Positive**: Eliminates onboarding friction; enables immediate self-learner activation; unifies schema across B2C and B2B tiers.
- **Negative**: Foreign key `documents.course_id` points to either personal or institutional containers, requiring descriptive join filtering.
- **Neutral**: Does not impact underlying ProseMirror AST or vector embedding formats.

### Validation & Verification Rules
- Create a new student account: verify that an initial personal container is provisioned and the user can upload a PDF and create notes without joining or creating a course.
- Verify that an instructor enrolled in an academic course cannot query or access any documents or annotations tagged with `courses.visibility = 'personal'`.

---

## ADR-020: Unified Review Queue with Orthogonal Tags and Elimination of Deck Entity for v1.0

### Status
**Accepted** (Supersedes deck-centric assumptions in ADR-004, ADR-010, ADR-011, ADR-013, ADR-018)

### Context & Problem Statement
Traditional spaced repetition software (e.g., Anki, SuperMemo) organizes flashcards into hierarchical "Decks" (folders). In practice, deck hierarchies create severe cognitive friction and architectural defects:
1. **Taxonomic Decision Fatigue**: When a student highlights a concept at the intersection of disciplines (e.g., "Mitochondrial Dysfunction in Parkinson's Disease"), they are forced to decide whether the card belongs in the "Biology", "Neurology", or "Pharmacology" deck.
2. **Sub-Optimal Cognitive Interleaving**: Reviewing cards in isolated deck silos defeats the psychological spacing effect. Modern learning science demonstrates that **interleaved retrieval practice** (mixing related topics) produces significantly higher long-term retention than blocked practice within single decks.
3. **Schema Inconsistency**: Early SWMS wireframes and RBAC matrices casually referenced "Course Decks", yet the database schema lacked a `decks` entity entirely, creating an unresolvable structural gap.

### Decision Drivers
- Zero-friction card creation: Eliminate categorization barriers when converting notes or highlights into flashcards.
- Optimal spaced repetition: Maximize retention by interleaving reviews across the user's entire knowledge base.
- Lean MVP execution: Avoid building complex nested deck trees, folder management CRUD, and deck permission hierarchies for v1.0.

### Considered Options
- **Option 1: Full Relational Decks Entity (`decks` table with parent/child hierarchies and deck sharing)**:
  - *Pros*: Familiar to traditional Anki users.
  - *Cons*: Heavy schema complexity; creates rigid knowledge silos; delays v1.0 release by 2–3 sprints.
- **Option 2: Unified Review Queue with Multi-Tagging and Source Filtering**:
  - *Pros*: Zero deck maintenance overhead; all due cards aggregate into a single daily study queue; orthogonal categorization via lightweight `tags` and `flashcard_tags`; allows ad-hoc filtering by tag or document source; aligns with FSRS interleaved spacing principles.
  - *Cons*: Anki power users must adapt to tag-based filtering instead of folder trees.

### Decision Outcome
**Chosen: Option 2**.
1. **Elimination of `decks` Entity**: No `decks` table will be created in SWMS v1.0. Flashcards belong directly to the user (`flashcards.user_id`), with optional links to `lesson_id` or `document_id`.
2. **Relational Tagging Engine**:
   ```sql
   CREATE TABLE IF NOT EXISTS tags (
       id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
       user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
       name VARCHAR(50) NOT NULL,
       color_key VARCHAR(20) NOT NULL DEFAULT 'blue' CHECK (color_key IN ('yellow', 'green', 'blue', 'purple', 'rose', 'red', 'gray')),
       created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
       UNIQUE(user_id, name)
   );

   CREATE TABLE IF NOT EXISTS flashcard_tags (
       flashcard_id UUID NOT NULL REFERENCES flashcards(id) ON DELETE CASCADE,
       tag_id UUID NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
       PRIMARY KEY (flashcard_id, tag_id)
   );
   ```
3. **Unified Review Queue API**:
   - `GET /api/v1/flashcards/due`: Returns all due cards across the user's entire knowledge base.
   - Optional query filters: `?tag_id=...` or `?document_id=...` for focused exam cramming or specific topic reviews.
4. **UI Adaptation**: The Review Mode UI replaces the traditional deck selection screen with a prominent **"Start Daily Review"** button displaying the total due count, accompanied by a tag filter rail.

### Authoritative References
- SWMS Product Review Decision 4 (`product_review_decisions.md`).
- Kornell, N. & Bjork, R. A.: *Learning concepts and categories: Is spacing the enemy of induction?* (Psychological Science, demonstrating interleaving benefits).
- RemNote Architecture: *Concept-Driven SRS over File-System Folders*.

### Technical Consequences
- **Positive**: Completely eliminates deck CRUD complexity; provides seamless one-click card creation; delivers superior cognitive retention via interleaved retrieval.
- **Negative**: Exporting to Anki `.apkg` format requires mapping tags to virtual Anki sub-decks (deferred to v1.x).
- **Neutral**: FSRS v5 scheduling algorithms operate identically on individual card states regardless of grouping.

### Validation & Verification Rules
- Verify that creating a flashcard requires only front/back content or cloze syntax, with zero prompt for a "Deck" selection.
- Execute `GET /api/v1/flashcards/due`: Confirm that due cards from disparate lessons, PDFs, and tags are returned in optimal chronological order.

---

## ADR-021: Lean MVP Scope Execution (8–10 Sprints, 1–2 Devs) and Deferral of Formal Exams to v1.x

### Status
**Accepted** (Supersedes the 16-sprint roadmap in ADR-012 and formal exam structures in ADR-006)

### Context & Problem Statement
ADR-012 previously established an ambitious 16-sprint (8-month) implementation roadmap covering enterprise assessment engines, multi-page OCR extraction, Text-to-Speech (TTS) karaoke readers, 365-day streak heatmaps, community shared decks, and full PWA offline synchronization.

Given a resource constraint of **1–2 full-stack software engineers**, executing a 16-sprint enterprise scope carries severe operational risks: delivery paralysis, architectural over-engineering, unmaintained test suites, and high vulnerability to burnout.

Furthermore, user research confirms that the existential value proposition of SWMS is the **tight core study loop**:
$$\text{Ingest (PDF/MD)} \longrightarrow \text{Read \& Highlight} \longrightarrow \text{Synthesize Note} \longrightarrow \text{FSRS Spaced Repetition}$$
Features outside this core loop (such as proctored formal exams, heavy OCR, and community sharing) add peripheral complexity without strengthening core product activation.

### Decision Drivers
- High execution feasibility for a 1–2 developer team within a 4–5 month (8–10 sprint) delivery window.
- Immediate activation and retention: prioritize features that unblock the daily reading and review loop.
- Elimination of architectural over-engineering prior to product-market fit.

### Considered Options
- **Option 1: Retain 16-Sprint Comprehensive Enterprise Scope**:
  - *Pros*: Broad feature list for institutional marketing.
  - *Cons*: High failure probability; incomplete implementations; delayed user feedback.
- **Option 2: Lean MVP Scope (8–10 Sprints) with v1.x Phased Backlog**:
  - *Pros*: Realistic velocity; high quality and test coverage; working software in production within 4 months; crystal-clear product positioning.
  - *Cons*: Requires disciplined postponement of high-effort secondary features.

### Decision Outcome
**Chosen: Option 2**.
1. **Roadmap Realignment (8–10 Sprints, 3 Phases)**:
   - **Phase 1: Core Workspace & Ingestion (Sprints 1–3)**: PostgreSQL 17 DDL with HNSW, PDF text-layer ingestion (PDF.js) + Markdown import, unified Tiptap JSON AST, 1920x1080 Split Workspace, core FSRS v5 engine.
   - **Phase 2: In-Situ Synthesis & Unified Review (Sprints 4–6)**: Personal Workspace First, 5 `color_key` highlights, Cloze-first card draft popover, Unified Review Queue with Tags, Command Palette (`cmd+k`), Freeform Notebook with backlinks.
   - **Phase 3: Copilot, Quotas & Resilient Sync (Sprints 7–9)**: Practice Quizzes with AI rubric suggestions, durable `background_jobs` queue, AI cost tracking (`ai_usage_ledger`, `user_quotas`), Resilient Online IndexedDB sync ("Keep Both" 409 resolution).
   - **Sprint 10: Production Hardening & GA**: Visual regression testing across 1920/1536/1280 viewports, performance validation, and v1.0 release.
2. **Assessment Scoping**:
   - Formal proctored Chapter/Course Exams with strict time limits, passing scores (`passing_score`), and binding instructor grades are **deferred to v1.x**.
   - SWMS v1.0 provides **Practice Quizzes**: instant self-testing quizzes generated from notes/highlights with qualitative AI rubric suggestions (no pass/fail gating).
3. **Formal v1.x Backlog Deferrals**:
   - Heavy OCR for scanned PDFs (Phase 3 v1.x).
   - Text-to-Speech (TTS) Karaoke Audio Reader.
   - Formal Academic Proctored Exams & Instructor Grading.
   - Community Shared Decks & Public Study Groups.
   - 365-day GitHub Streak Heatmaps & Radar Charts.
   - Full PWA Background Sync & Service Worker offline asset caching.
   - Anki `.apkg` binary export (Markdown export delivered in v1.0).

### Authoritative References
- SWMS Product Review Decisions 6 and 11 (`product_review_decisions.md`).
- Eric Ries: *The Lean Startup — Building the Minimum Viable Product*.

### Technical Consequences
- **Positive**: Achievable delivery timeline; high engineering morale; focus on software quality, automated testing, and rock-solid core stability.
- **Negative**: Institutional enterprise RFPs requiring proctored exams must be targeted in v1.x.
- **Neutral**: Database schema retains flexible extension points for deferred features.

### Validation & Verification Rules
- Verify that Sprint 1–10 burn-down delivers all core loop capabilities without any blocked dependencies on deferred v1.x features.
- Confirm that taking a Practice Quiz produces instant AI rubric feedback without requiring an instructor grading workflow.

---

## ADR-022: Resilient Online Strategy with "Keep Both" 409 Conflict Resolution for v1.0

### Status
**Accepted** (Supersedes 3-way AST merge and full PWA offline requirements in ADR-002, ADR-010, ADR-012)

### Context & Problem Statement
Early specifications proposed full offline PWA synchronization with an interactive 3-way visual merge interface (Base vs Local vs Server) for ProseMirror JSON AST documents when concurrent edits collide.

Implementing a non-destructive 3-way AST merge engine capable of resolving conflicting block insertions, formatting marks, citations, and table edits requires either a distributed CRDT framework (e.g., Yjs, Automerge) or an Operational Transformation (OT) engine. For a personal workspace platform built by 1–2 developers, implementing and verifying a custom 3-way JSON AST merge engine introduces immense risk and requires months of specialized effort.

Empirical data reveals that in personal single-user study environments, concurrent multi-device edit conflicts are exceedingly rare ($<0.1\%$ of saves). Investing significant engineering resources to build a complex 3-way merge UI for an ultra-rare edge case is an inefficient allocation of lean startup bandwidth.

### Decision Drivers
- Zero data loss: Under no circumstances should student synthesis notes or study logs be overwritten or discarded.
- Simplicity and reliability: Implement an intuitive, dependable conflict resolution mechanism that 1–2 developers can ship and maintain with 100% test confidence.
- Offline resilience for the high-frequency spaced repetition study loop.

### Considered Options
- **Option 1: Last-Write-Wins (LWW) via Silent Overwrite**:
  - *Pros*: Zero frontend conflict UI required.
  - *Cons*: Fatal flaw: silently destroys user work if edits occur across two disconnected tabs or devices.
- **Option 2: Interactive 3-Way ProseMirror JSON AST Diff and Merge Engine**:
  - *Pros*: Granular paragraph-level merge.
  - *Cons*: Extreme development complexity; edge cases cause AST corruption; high maintenance burden; delays launch.
- **Option 3: Resilient Online Strategy with "Keep Both" 409 Conflict Forking**:
  - *Pros*: 100% guarantee of zero data loss; simple and robust; buffers high-frequency FSRS reviews offline via IndexedDB; handles rare notebook 409 conflicts by saving the conflicting client draft as an explicit companion copy (`[Conflict Copy] <Title>`); user is notified via a non-blocking toast and retains both versions.
  - *Cons*: Leaves manual text reconciliation to the user in the rare event of a collision.

### Decision Outcome
**Chosen: Option 3**.
1. **Two-Tier Offline Architecture ("Resilient Online")**:
   - **Tier 1 (FSRS Active Recall)**: Fully offline-capable. Due flashcards cached in IndexedDB can be reviewed without internet access. Review logs accumulate in local storage and synchronize automatically via batch API upon network reconnection.
   - **Tier 2 (Rich-Text Notebooks)**: Requires online connectivity for authoritative auto-saving, but caches dirty editor drafts in IndexedDB to prevent data loss across accidental tab closures, laptop sleeps, or transient drops.
2. **"Keep Both" 409 Conflict Resolution Protocol**:
   - When saving a notebook (`PUT /api/v1/notebooks/{id}`), the client passes `expected_version`.
   - If the server detects `version > expected_version`, the server rejects the update with `HTTP 409 Conflict`.
   - **Automatic Forking & Conflict Draft Recording**: Upon receiving HTTP 409, the backend (or client fallback) logs the incoming dirty client AST into the dedicated `notebook_conflict_drafts` table with `resolution_status = 'kept_both'`, and persists a forked new note:
     - `title = '[Conflict Copy - YYYY-MM-DD HH:mm] ' || original_title`
     - `version = 1`
     - Associated links and tags are cloned.
   - The server returns HTTP 409 containing the metadata of the newly created conflict note and the current remote canonical AST.
   - The client loads the canonical server AST and displays a non-blocking notification:
     > *"A conflicting edit was detected. The latest remote version has been loaded, and your local edits were safely preserved as a new note: '[Conflict Copy]...'"*
3. **Deferral**: Interactive 3-way visual AST merging is formally deferred to v1.x.

### Authoritative References
- SWMS Product Review Decision 12 (`product_review_decisions.md`).
- Dropbox & Google Drive Sync Conflict Architecture: *Conflicted Copy Generation Patterns*.

### Technical Consequences
- **Positive**: Zero risk of catastrophic data loss; trivial implementation complexity; robust offline active recall study; eliminates weeks of AST merge debugging.
- **Negative**: Creates an extra note record in the rare event of an uncoordinated multi-device edit.
- **Neutral**: Flashcard review synchronization remains strictly deterministic via Lamport timestamps and idempotent event logs.

### Validation & Verification Rules
- Open a notebook in two independent browser sessions: edit Session A and save; then attempt to save conflicting text from Session B:
  - Verify that Session B receives HTTP 409.
  - Verify that Session B's changes are saved as a separate `[Conflict Copy]` notebook record.
  - Confirm that zero data is lost from either session.
- Review 30 flashcards in airplane mode: reconnect network: verify all 30 review events sync idempotently to `flashcard_reviews`.
