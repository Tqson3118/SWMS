-- ============================================================================
-- SWMS DATABASE DEFINITION DDL (POSTGRESQL 17 + PGVECTOR)
-- AUTHORITATIVE PRODUCTION SCHEMA WITH STRICT INTEGRITY CONSTRAINTS
-- SYNCHRONIZED WITH MASTER_PROMPT.MD v2.2.0 & ADR-001 THROUGH ADR-022
-- ============================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "vector";
CREATE EXTENSION IF NOT EXISTS "unaccent";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ----------------------------------------------------------------------------
-- IMMUTABLE UNACCENT WRAPPER FOR VIETNAMESE TRIGRAM INDEXING
-- In PostgreSQL, unaccent() is STABLE. An IMMUTABLE wrapper is required to build
-- functional GIN trigram indexes for fast Vietnamese search & autocomplete.
-- ----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION f_unaccent(text)
  RETURNS text AS $$
  SELECT public.unaccent('public.unaccent', $1)
$$ LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT;

-- ----------------------------------------------------------------------------
-- 1. USERS & AUTHENTICATION (JWT REFRESH TOKEN ROTATION & REVOCATION)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(150) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'student' CHECK (role IN ('student', 'instructor', 'admin')),
    token_version INT NOT NULL DEFAULT 1,
    streak_days INT NOT NULL DEFAULT 0,
    onboarding_step INT NOT NULL DEFAULT 1 CHECK (onboarding_step BETWEEN 1 AND 4),
    is_activated BOOLEAN NOT NULL DEFAULT FALSE,
    activated_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash VARCHAR(64) NOT NULL UNIQUE,
    family_id UUID NOT NULL,
    is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
    replaced_by_token_id UUID REFERENCES refresh_tokens(id) ON DELETE SET NULL,
    encrypted_replacement_token BYTEA, -- AES-256-GCM encrypted replacement token for 15-30s grace window (ADR-016)
    device_info TEXT,
    ip_address VARCHAR(45),
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    revoked_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_refresh_tokens_hash ON refresh_tokens(token_hash);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user ON refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_family ON refresh_tokens(family_id);

-- ----------------------------------------------------------------------------
-- 1.1 LEARNER-SPECIFIC FSRS v5 19-PARAMETER WEIGHTS & PREFERENCES
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_fsrs_parameters (
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    weights FLOAT[] NOT NULL DEFAULT '{0.4072, 1.1829, 3.1262, 15.4722, 7.2102, 0.5316, 1.0651, 0.0234, 1.616, 0.1544, 1.0824, 1.9813, 0.0953, 0.2975, 2.2042, 0.2407, 2.9466, 0.5034, 0.6567}',
    desired_retention FLOAT NOT NULL DEFAULT 0.90 CHECK (desired_retention BETWEEN 0.70 AND 0.99),
    maximum_interval_days INT NOT NULL DEFAULT 36500 CHECK (maximum_interval_days > 0),
    optimized_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ----------------------------------------------------------------------------
-- 1.2 AI COST MANAGEMENT & USAGE QUOTAS (FREEMIUM SAFEGUARDS)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_quotas (
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    tier VARCHAR(30) NOT NULL DEFAULT 'free' CHECK (tier IN ('free', 'pro', 'institution')),
    daily_ai_token_limit INT NOT NULL DEFAULT 50000 CHECK (daily_ai_token_limit >= 0),
    daily_ai_credit_limit INT NOT NULL DEFAULT 50 CHECK (daily_ai_credit_limit >= 0),
    storage_limit_bytes BIGINT NOT NULL DEFAULT 52428800 CHECK (storage_limit_bytes >= 0), -- 50 MB free tier per Section 5.11
    current_storage_bytes BIGINT NOT NULL DEFAULT 0 CHECK (current_storage_bytes >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS ai_usage_ledger (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    feature VARCHAR(50) NOT NULL CHECK (feature IN ('cloze_generation', 'qa_generation', 'explain_concept', 'essay_feedback', 'practice_quiz_gen', 'semantic_search', 'ocr_parsing')),
    credits_charged INT NOT NULL DEFAULT 1 CHECK (credits_charged >= 0),
    model VARCHAR(100) NOT NULL,
    prompt_tokens INT NOT NULL DEFAULT 0 CHECK (prompt_tokens >= 0),
    completion_tokens INT NOT NULL DEFAULT 0 CHECK (completion_tokens >= 0),
    total_tokens INT NOT NULL DEFAULT 0 CHECK (total_tokens >= 0),
    cost_usd NUMERIC(10, 6) NOT NULL DEFAULT 0.000000,
    context_ref JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_ai_usage_ledger_user_date ON ai_usage_ledger(user_id, created_at);
CREATE INDEX IF NOT EXISTS idx_ai_usage_ledger_feature ON ai_usage_ledger(feature);

-- ----------------------------------------------------------------------------
-- 2. COURSE HIERARCHY, PERSONAL WORKSPACE & VERSIONING
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS courses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    instructor_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL UNIQUE,
    description TEXT,
    visibility VARCHAR(30) NOT NULL DEFAULT 'personal' CHECK (visibility IN ('personal', 'private', 'public')),
    is_published BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_courses_instructor ON courses(instructor_id);
CREATE INDEX IF NOT EXISTS idx_courses_visibility ON courses(visibility);
CREATE INDEX IF NOT EXISTS idx_courses_title_trgm ON courses USING gin (f_unaccent(title) gin_trgm_ops);

CREATE TABLE IF NOT EXISTS course_enrollments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    course_id UUID NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    role VARCHAR(50) NOT NULL DEFAULT 'student' CHECK (role IN ('student', 'teaching_assistant', 'auditor')),
    enrolled_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, course_id)
);

CREATE INDEX IF NOT EXISTS idx_course_enrollments_user ON course_enrollments(user_id);
CREATE INDEX IF NOT EXISTS idx_course_enrollments_course ON course_enrollments(course_id);

CREATE TABLE IF NOT EXISTS chapters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    course_id UUID NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    sort_order INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_chapters_course ON chapters(course_id, sort_order);

CREATE TABLE IF NOT EXISTS lessons (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    chapter_id UUID NOT NULL REFERENCES chapters(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL,
    sort_order INT NOT NULL DEFAULT 0,
    current_version INT NOT NULL DEFAULT 1,
    content_ast JSONB NOT NULL,
    plain_text TEXT NOT NULL,
    is_published BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(chapter_id, slug)
);

CREATE INDEX IF NOT EXISTS idx_lessons_chapter ON lessons(chapter_id, sort_order);
CREATE INDEX IF NOT EXISTS idx_lessons_title_trgm ON lessons USING gin (f_unaccent(title) gin_trgm_ops);

CREATE TABLE IF NOT EXISTS lesson_versions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    lesson_id UUID NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    version_number INT NOT NULL,
    content_ast JSONB NOT NULL,
    plain_text TEXT NOT NULL,
    published_by UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    published_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    change_summary TEXT,
    UNIQUE(lesson_id, version_number)
);

CREATE INDEX IF NOT EXISTS idx_lesson_versions_lesson ON lesson_versions(lesson_id, version_number);

-- ----------------------------------------------------------------------------
-- 2.1 DOCUMENTS & PDF TEXTBOOK MANAGEMENT
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    course_id UUID NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    uploader_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    file_url TEXT NOT NULL,
    file_size_bytes BIGINT NOT NULL CHECK (file_size_bytes > 0),
    total_pages INT NOT NULL DEFAULT 1 CHECK (total_pages > 0),
    mime_type VARCHAR(100) NOT NULL DEFAULT 'application/pdf',
    ocr_status VARCHAR(30) NOT NULL DEFAULT 'pending' CHECK (ocr_status IN ('pending', 'processing', 'completed', 'failed')),
    content_ast JSONB,
    plain_text TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_documents_course ON documents(course_id);
CREATE INDEX IF NOT EXISTS idx_documents_uploader ON documents(uploader_id);
CREATE INDEX IF NOT EXISTS idx_documents_title_trgm ON documents USING gin (f_unaccent(title) gin_trgm_ops);

-- ----------------------------------------------------------------------------
-- 3. ANNOTATIONS (POLYMORPHIC & STRICTLY EXCLUSIVE TARGETING)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS annotations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    -- Strictly exclusive polymorphic parent target: Lesson Text Block XOR Document PDF Page
    lesson_id UUID REFERENCES lessons(id) ON DELETE CASCADE,
    block_id UUID,
    document_id UUID REFERENCES documents(id) ON DELETE CASCADE,
    page_number INT CHECK (page_number IS NULL OR page_number > 0),
    bounding_boxes JSONB, -- Array of spatial rects: [{"x": 0.1, "y": 0.2, "width": 0.5, "height": 0.05}]
    
    exact_quote TEXT NOT NULL,
    original_exact_quote TEXT NOT NULL,
    prefix_quote VARCHAR(100),
    suffix_quote VARCHAR(100),
    start_offset INT NOT NULL DEFAULT 0,
    end_offset INT NOT NULL DEFAULT 0,
    color_key VARCHAR(20) NOT NULL DEFAULT 'yellow' CHECK (color_key IN ('yellow', 'green', 'blue', 'purple', 'rose', 'red')),
    semantic_label VARCHAR(100),
    note_comment TEXT,
    status VARCHAR(30) NOT NULL DEFAULT 'anchored' CHECK (status IN ('anchored', 'needs_review', 'orphaned', 'archived')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_annotation_target_exclusive CHECK (
        (lesson_id IS NOT NULL AND block_id IS NOT NULL AND document_id IS NULL AND page_number IS NULL) OR 
        (document_id IS NOT NULL AND page_number IS NOT NULL AND lesson_id IS NULL AND block_id IS NULL)
    ),
    CONSTRAINT chk_annotation_offsets CHECK (end_offset >= start_offset)
);

CREATE INDEX IF NOT EXISTS idx_annotations_lesson_user ON annotations(lesson_id, user_id);
CREATE INDEX IF NOT EXISTS idx_annotations_document_user ON annotations(document_id, user_id, page_number);
CREATE INDEX IF NOT EXISTS idx_annotations_block ON annotations(block_id);
CREATE INDEX IF NOT EXISTS idx_annotations_status ON annotations(user_id, status);

-- ----------------------------------------------------------------------------
-- 4. NOTEBOOKS (SINGLE JSONB) & M:N NOTEBOOK LINKS
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS notebooks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL DEFAULT 'Untitled Notebook',
    primary_source_lesson_id UUID REFERENCES lessons(id) ON DELETE SET NULL,
    primary_source_document_id UUID REFERENCES documents(id) ON DELETE SET NULL,
    content_ast JSONB NOT NULL DEFAULT '{"type":"doc","content":[]}'::jsonb,
    version INT NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_notebook_primary_source CHECK (
        (primary_source_lesson_id IS NULL OR primary_source_document_id IS NULL)
    )
);

CREATE INDEX IF NOT EXISTS idx_notebooks_user ON notebooks(user_id);
CREATE INDEX IF NOT EXISTS idx_notebooks_primary_lesson ON notebooks(primary_source_lesson_id) WHERE primary_source_lesson_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_notebooks_primary_document ON notebooks(primary_source_document_id) WHERE primary_source_document_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_notebooks_title_trgm ON notebooks USING gin (f_unaccent(title) gin_trgm_ops);

CREATE TABLE IF NOT EXISTS notebook_links (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    notebook_id UUID NOT NULL REFERENCES notebooks(id) ON DELETE CASCADE,
    source_block_id UUID NOT NULL,
    target_type VARCHAR(30) NOT NULL CHECK (target_type IN ('annotation', 'lesson', 'document')),
    ref_annotation_id UUID REFERENCES annotations(id) ON DELETE CASCADE,
    ref_lesson_id UUID REFERENCES lessons(id) ON DELETE CASCADE,
    ref_document_id UUID REFERENCES documents(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_notebook_links_target CHECK (
        (target_type = 'annotation' AND ref_annotation_id IS NOT NULL) OR
        (target_type = 'lesson' AND ref_lesson_id IS NOT NULL) OR
        (target_type = 'document' AND ref_document_id IS NOT NULL)
    )
);

CREATE INDEX IF NOT EXISTS idx_notebook_links_notebook ON notebook_links(notebook_id);
CREATE INDEX IF NOT EXISTS idx_notebook_links_ref_ann ON notebook_links(ref_annotation_id) WHERE ref_annotation_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_notebook_links_ref_les ON notebook_links(ref_lesson_id) WHERE ref_lesson_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_notebook_links_ref_doc ON notebook_links(ref_document_id) WHERE ref_document_id IS NOT NULL;

-- ----------------------------------------------------------------------------
-- 4.1 OFFLINE CONFLICT DRAFTS (KEEP-BOTH RECONCILIATION PER ADR-022)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS notebook_conflict_drafts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    notebook_id UUID NOT NULL REFERENCES notebooks(id) ON DELETE CASCADE,
    forked_notebook_id UUID REFERENCES notebooks(id) ON DELETE SET NULL,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    device_id VARCHAR(100),
    base_version INT NOT NULL,
    server_version INT NOT NULL,
    client_content_ast JSONB NOT NULL,
    resolution_status VARCHAR(30) NOT NULL DEFAULT 'kept_both' CHECK (resolution_status IN ('pending', 'kept_both', 'resolved_local', 'resolved_server')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    resolved_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_notebook_conflicts_user ON notebook_conflict_drafts(user_id, resolution_status);

-- ----------------------------------------------------------------------------
-- 5. SPACED REPETITION (TAG-BASED UNIFIED QUEUE & FSRS v5)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(50) NOT NULL,
    color_key VARCHAR(20) NOT NULL DEFAULT 'blue' CHECK (color_key IN ('yellow', 'green', 'blue', 'purple', 'rose', 'red', 'gray')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, name)
);

CREATE INDEX IF NOT EXISTS idx_tags_user ON tags(user_id);
CREATE INDEX IF NOT EXISTS idx_tags_name_trgm ON tags USING gin (f_unaccent(name) gin_trgm_ops);

CREATE TABLE IF NOT EXISTS flashcards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    lesson_id UUID REFERENCES lessons(id) ON DELETE SET NULL,
    document_id UUID REFERENCES documents(id) ON DELETE SET NULL,
    annotation_id UUID REFERENCES annotations(id) ON DELETE SET NULL,
    card_type VARCHAR(30) NOT NULL DEFAULT 'standard' CHECK (card_type IN ('standard', 'cloze', 'ai_generated')),
    source_title_snapshot TEXT,
    source_quote_snapshot TEXT,
    front_ast JSONB NOT NULL,
    back_ast JSONB NOT NULL,
    stability FLOAT NOT NULL DEFAULT 0.0,
    difficulty FLOAT NOT NULL DEFAULT 0.0,
    reps INT NOT NULL DEFAULT 0,
    lapses INT NOT NULL DEFAULT 0,
    state INT NOT NULL DEFAULT 0 CHECK (state BETWEEN 0 AND 3), -- 0=New, 1=Learning, 2=Review, 3=Relearning
    is_archived BOOLEAN NOT NULL DEFAULT FALSE,
    last_review_at TIMESTAMPTZ,
    due_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(id, user_id)
);

CREATE INDEX IF NOT EXISTS idx_flashcards_user_due ON flashcards(user_id, due_at) WHERE is_archived = FALSE;
CREATE INDEX IF NOT EXISTS idx_flashcards_lesson ON flashcards(lesson_id);
CREATE INDEX IF NOT EXISTS idx_flashcards_document ON flashcards(document_id);
CREATE INDEX IF NOT EXISTS idx_flashcards_annotation ON flashcards(annotation_id);
CREATE INDEX IF NOT EXISTS idx_flashcards_archived ON flashcards(user_id, is_archived);

CREATE TABLE IF NOT EXISTS flashcard_tags (
    flashcard_id UUID NOT NULL REFERENCES flashcards(id) ON DELETE CASCADE,
    tag_id UUID NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (flashcard_id, tag_id)
);

CREATE INDEX IF NOT EXISTS idx_flashcard_tags_tag ON flashcard_tags(tag_id);

CREATE TABLE IF NOT EXISTS flashcard_reviews (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    review_event_id UUID UNIQUE NOT NULL,
    flashcard_id UUID NOT NULL,
    user_id UUID NOT NULL,
    rating INT NOT NULL CHECK (rating BETWEEN 1 AND 4),
    review_duration_ms INT CHECK (review_duration_ms IS NULL OR review_duration_ms >= 0),
    scheduled_days FLOAT NOT NULL,
    elapsed_days FLOAT NOT NULL,
    last_stability FLOAT NOT NULL,
    new_stability FLOAT NOT NULL,
    last_difficulty FLOAT NOT NULL,
    new_difficulty FLOAT NOT NULL,
    reviewed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_flashcard_reviews_owner FOREIGN KEY (flashcard_id, user_id) 
        REFERENCES flashcards(id, user_id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_flashcard_reviews_card ON flashcard_reviews(flashcard_id, reviewed_at);
CREATE INDEX IF NOT EXISTS idx_flashcard_reviews_user ON flashcard_reviews(user_id, reviewed_at);
CREATE INDEX IF NOT EXISTS idx_flashcard_reviews_event ON flashcard_reviews(review_event_id);

-- ----------------------------------------------------------------------------
-- 6. FORMATIVE PRACTICE QUIZZES (SELF-STUDY WITH AI RUBRIC FEEDBACK)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS practice_quizzes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL DEFAULT 'Practice Quiz',
    lesson_id UUID REFERENCES lessons(id) ON DELETE SET NULL,
    document_id UUID REFERENCES documents(id) ON DELETE SET NULL,
    total_items INT NOT NULL DEFAULT 0 CHECK (total_items >= 0),
    correct_items INT NOT NULL DEFAULT 0 CHECK (correct_items >= 0),
    status VARCHAR(30) NOT NULL DEFAULT 'in_progress' CHECK (status IN ('in_progress', 'completed', 'abandoned')),
    started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ,
    ai_summary_feedback TEXT
);

CREATE INDEX IF NOT EXISTS idx_practice_quizzes_user ON practice_quizzes(user_id, started_at DESC);
CREATE INDEX IF NOT EXISTS idx_practice_quizzes_lesson ON practice_quizzes(lesson_id);
CREATE INDEX IF NOT EXISTS idx_practice_quizzes_document ON practice_quizzes(document_id);

CREATE TABLE IF NOT EXISTS practice_quiz_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    quiz_id UUID NOT NULL REFERENCES practice_quizzes(id) ON DELETE CASCADE,
    item_type VARCHAR(30) NOT NULL CHECK (item_type IN ('multiple_choice', 'cloze', 'short_answer', 'essay')),
    prompt_ast JSONB NOT NULL,
    choices JSONB,
    cloze_solution TEXT,
    user_answer JSONB,
    is_correct BOOLEAN,
    ai_rubric_feedback JSONB,
    evaluated_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_practice_quiz_items_quiz ON practice_quiz_items(quiz_id);

-- ----------------------------------------------------------------------------
-- 7. TRANSACTIONAL DURABLE BACKGROUND JOBS (POSTGRESQL LEASE QUEUE)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS background_jobs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    idempotency_key UUID UNIQUE NOT NULL,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    job_type VARCHAR(50) NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'queued' CHECK (status IN ('queued', 'leased', 'running', 'completed', 'failed', 'cancelled')),
    priority INT NOT NULL DEFAULT 0,
    retry_count INT NOT NULL DEFAULT 0,
    max_retries INT NOT NULL DEFAULT 3,
    lease_expires_at TIMESTAMPTZ,
    leased_by_worker_id VARCHAR(100),
    progress_percent INT NOT NULL DEFAULT 0 CHECK (progress_percent BETWEEN 0 AND 100),
    payload JSONB NOT NULL,
    result JSONB,
    error_message TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_background_jobs_queue ON background_jobs(status, priority DESC, created_at ASC);
CREATE INDEX IF NOT EXISTS idx_background_jobs_user_status ON background_jobs(user_id, status);
CREATE INDEX IF NOT EXISTS idx_background_jobs_idempotency ON background_jobs(idempotency_key);

-- ----------------------------------------------------------------------------
-- 8. PGVECTOR HNSW EMBEDDINGS (M = 16, EF_CONSTRUCTION = 128)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS lesson_embeddings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    lesson_id UUID NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    chunk_index INT NOT NULL,
    content_chunk TEXT NOT NULL,
    embedding vector(768) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_lesson_embeddings_hnsw 
ON lesson_embeddings 
USING hnsw (embedding vector_cosine_ops) 
WITH (m = 16, ef_construction = 128);

CREATE INDEX IF NOT EXISTS idx_lesson_embeddings_lesson_id 
ON lesson_embeddings(lesson_id);

CREATE TABLE IF NOT EXISTS document_embeddings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_id UUID NOT NULL REFERENCES documents(id) ON DELETE CASCADE,
    page_number INT NOT NULL CHECK (page_number > 0),
    chunk_index INT NOT NULL,
    content_chunk TEXT NOT NULL,
    embedding vector(768) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_document_embeddings_hnsw 
ON document_embeddings 
USING hnsw (embedding vector_cosine_ops) 
WITH (m = 16, ef_construction = 128);

CREATE INDEX IF NOT EXISTS idx_document_embeddings_doc 
ON document_embeddings(document_id, page_number);

CREATE TABLE IF NOT EXISTS notebook_embeddings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    notebook_id UUID NOT NULL REFERENCES notebooks(id) ON DELETE CASCADE,
    block_id UUID NOT NULL,
    content_chunk TEXT NOT NULL,
    embedding vector(768) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_notebook_embeddings_user_notebook 
ON notebook_embeddings(user_id, notebook_id);

CREATE INDEX IF NOT EXISTS idx_notebook_embeddings_hnsw 
ON notebook_embeddings 
USING hnsw (embedding vector_cosine_ops) 
WITH (m = 16, ef_construction = 128);
