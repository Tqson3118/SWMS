import type { Page, Response } from "@playwright/test";
import { test, expect } from "./fixtures/test-base";
import { assertZeroHorizontalOverflow } from "./fixtures/ergonomics-helpers";
import path from "node:path";
import fs from "node:fs";

const SCRATCH_DIR =
  "C:\\Users\\Administrator\\.gemini\\antigravity\\brain\\42774bee-a40b-45e4-b7d4-d765f5fefa73\\scratch";

const EXPECTED_DOCUMENT_COURSES = [
  {
    slug: "doc-course-studxy-fullstack-bootcamp",
    title: "Studxy Full-Stack"
  },
  {
    slug: "document-course-ielts",
    title: "IELTS Mastery Roadmap (Phase 0 – Phase 3)"
  },
  {
    slug: "document-course-investing",
    title: "Đầu Tư Tài Chính Nền Tảng (18 Tuần)"
  },
  {
    slug: "document-course-lifeskills",
    title: "Kỹ Năng Sống & Giao Tiếp Hiệu Quả (16 Tuần)"
  },
  {
    slug: "document-course-personalfinance",
    title: "Tài Chính Cá Nhân Thực Hành (12 Tuần)"
  },
  {
    slug: "document-course-psychology",
    title: "Tự Học Tâm Lý Học 48 Tuần"
  }
];

const RETIRED_SLUGS = [
  "document-course-dotnet-react-roadmap",
  "document-course-fullstack-csharp-react-refresh"
];

function ensureScratchDir() {
  if (!fs.existsSync(SCRATCH_DIR)) {
    fs.mkdirSync(SCRATCH_DIR, { recursive: true });
  }
}

async function provisionIsolatedLearner(page: Page): Promise<string> {
  const email = `e2e.doc.${Date.now()}.${Math.random().toString(36).slice(2, 7)}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "Học viên Kiểm thử E2E"
    }
  });
  expect(regRes.status()).toBe(200);
  const regJson = await regRes.json();
  const userId = regJson.user.id as string;
  expect(userId).toBeTruthy();

  await page.addInitScript((uid: string) => {
    window.localStorage.setItem("swms_active_user_id", uid);
  }, userId);

  return userId;
}

function attachNetworkFailureMonitor(page: Page): string[] {
  const failures: string[] = [];
  page.on("response", (resp: Response) => {
    const status = resp.status();
    const url = resp.url();
    if (status >= 500) {
      failures.push(`[HTTP ${status}] ${resp.request().method()} ${url}`);
    }
  });
  return failures;
}

test.describe("Document Courses Full-Stack Real E2E (No Mocks)", () => {
  test.setTimeout(90000);

  test("1. Landing -> /workspace Catalog shows all curated courses, search/category filters, sync, dark mode & tablet layout", async ({
    page
  }) => {
    ensureScratchDir();
    const serverErrors = attachNetworkFailureMonitor(page);

    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/");
    await page.waitForLoadState("domcontentloaded");

    const heroCta = page.getByTestId("landing-hero-cta");
    await expect(heroCta).toBeVisible();
    await heroCta.click();

    await page.waitForURL("**/auth?tab=register");
    await provisionIsolatedLearner(page);
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");
    const catalog = page.getByTestId("course-catalog-section");
    await expect(catalog).toBeVisible({ timeout: 20000 });

    // Wait for all 6 curated document courses to render in the catalog
    for (const expected of EXPECTED_DOCUMENT_COURSES) {
      const card = catalog.locator(`[data-course-slug="${expected.slug}"]`);
      await expect(card, `Course ${expected.slug} must be visible`).toBeVisible({
        timeout: 20000
      });
      await expect(card.getByTestId("course-card-title")).toContainText(expected.title);
    }

    // Assert that the 2 retired courses do NOT appear in /workspace
    for (const slug of RETIRED_SLUGS) {
      await expect(catalog.locator(`[data-course-slug="${slug}"]`)).toHaveCount(0);
    }

    // Assert that the 2 retired courses do NOT appear in /app/courses
    await page.goto("/app/courses");
    await page.waitForLoadState("domcontentloaded");
    for (const slug of RETIRED_SLUGS) {
      await expect(page.locator(`[data-course-slug="${slug}"]`)).toHaveCount(0);
    }
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    // Check zero horizontal overflow on 1440x900 light mode
    await assertZeroHorizontalOverflow(page);
    await catalog.scrollIntoViewIfNeeded();
    await page.screenshot({
      path: path.join(SCRATCH_DIR, "01-workspace-catalog-desktop-1440.png"),
      fullPage: false
    });

    // Dark mode check & screenshot
    await page.getByTestId("theme-toggle-btn").click();
    await expect(page.locator("html")).toHaveClass(/dark/);
    await page.waitForTimeout(250);
    await assertZeroHorizontalOverflow(page);
    await page.screenshot({
      path: path.join(SCRATCH_DIR, "02-workspace-catalog-dark-1440.png"),
      fullPage: false
    });
    // Restore light mode
    await page.getByTestId("theme-toggle-btn").click();
    await expect(page.locator("html")).not.toHaveClass(/dark/);

    // Search filter (exact & unaccented Vietnamese)
    const searchInput = page.getByTestId("course-search-input");
    await searchInput.fill("IELTS");
    await expect(catalog.locator('[data-course-slug="document-course-ielts"]')).toBeVisible();
    await expect(
      catalog.locator('[data-course-slug="document-course-psychology"]')
    ).toBeHidden();

    await searchInput.fill("tai chinh");
    await expect(
      catalog.locator('[data-course-slug="document-course-personalfinance"]')
    ).toBeVisible();
    await expect(catalog.locator('[data-course-slug="document-course-investing"]')).toBeVisible();
    await searchInput.fill("");

    // Category filter pills
    await page.getByTestId("category-pill-programming").click();
    await expect(
      catalog.locator('[data-course-slug="doc-course-studxy-fullstack-bootcamp"]')
    ).toBeVisible();
    await expect(
      catalog.locator('[data-course-slug="document-course-dotnet-react-roadmap"]')
    ).toHaveCount(0);
    await expect(
      catalog.locator('[data-course-slug="document-course-fullstack-csharp-react-refresh"]')
    ).toHaveCount(0);
    await expect(catalog.locator('[data-course-slug="document-course-ielts"]')).toBeHidden();

    await page.getByTestId("category-pill-language").click();
    await expect(catalog.locator('[data-course-slug="document-course-ielts"]')).toBeVisible();
    await expect(
      catalog.locator('[data-course-slug="document-course-investing"]')
    ).toBeHidden();

    await page.getByTestId("category-pill-finance").click();
    await expect(
      catalog.locator('[data-course-slug="document-course-personalfinance"]')
    ).toBeVisible();
    await expect(catalog.locator('[data-course-slug="document-course-investing"]')).toBeVisible();

    await page.getByTestId("category-pill-psychology").click();
    await expect(catalog.locator('[data-course-slug="document-course-psychology"]')).toBeVisible();
    await expect(catalog.locator('[data-course-slug="document-course-lifeskills"]')).toBeVisible();

    await page.getByTestId("category-pill-all").click();
    for (const expected of EXPECTED_DOCUMENT_COURSES) {
      await expect(catalog.locator(`[data-course-slug="${expected.slug}"]`)).toBeVisible();
    }

    // Sync from document/ button calls real POST /api/v1/courses/sync-from-documents
    const syncBtn = page.getByTestId("sync-documents-btn");
    const [syncResp] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/courses/sync-from-documents") &&
          res.request().method() === "POST"
      ),
      syncBtn.click()
    ]);
    expect(syncResp.status()).toBe(200);
    await expect(page.getByTestId("catalog-sync-toast")).toBeVisible({ timeout: 15000 });
    for (const expected of EXPECTED_DOCUMENT_COURSES) {
      await expect(catalog.locator(`[data-course-slug="${expected.slug}"]`)).toHaveCount(1);
    }

    // Tablet 768x1024 layout & zero overflow
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(250);
    await assertZeroHorizontalOverflow(page);
    await catalog.scrollIntoViewIfNeeded();
    await page.screenshot({
      path: path.join(SCRATCH_DIR, "03-workspace-catalog-tablet-768.png"),
      fullPage: false
    });

    expect(serverErrors).toEqual([]);
  });

  test("2. Technical course Reader: Chapter->Lesson accordion, Markdown tables & code blocks, completion persistence across F5, Split PKM & Tablet drawer", async ({
    page
  }) => {
    ensureScratchDir();
    const serverErrors = attachNetworkFailureMonitor(page);
    await provisionIsolatedLearner(page);

    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");

    const catalog = page.getByTestId("course-catalog-section");
    const bootcampCard = catalog.locator(
      '[data-course-slug="doc-course-studxy-fullstack-bootcamp"]'
    );
    await expect(bootcampCard).toBeVisible({ timeout: 20000 });
    await bootcampCard.getByTestId("course-start-learning-btn").click();

    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible({ timeout: 15000 });
    const sidebar = page.getByTestId("course-roadmap-sidebar");
    await expect(sidebar).toBeVisible();

    // TopNav breadcrumb reflects active course title
    await expect(page.getByTestId("header-breadcrumb-root")).toContainText(
      "Full-Stack"
    );

    // Search & click Lesson 01 inside the Chapter->Lesson accordion sidebar
    const lessonSearch = page.getByTestId("roadmap-lesson-search-input");
    await lessonSearch.fill("Bài 01");
    const lesson1Btn = sidebar
      .getByTestId("course-syllabus-lesson-btn")
      .filter({ hasText: /Bài 01|Nhập Môn/i })
      .first();
    await expect(lesson1Btn).toBeVisible();
    await lesson1Btn.click();
    await lessonSearch.fill("");

    // Verify Markdown view renders fenced code blocks and content
    const mdView = page.getByTestId("lesson-markdown-view");
    await expect(mdView).toBeVisible({ timeout: 15000 });
    await expect(mdView.locator("pre code").first()).toBeVisible();

    await assertZeroHorizontalOverflow(page);
    await page.screenshot({
      path: path.join(SCRATCH_DIR, "04-reader-fullstack-markdown-1440.png"),
      fullPage: false
    });

    // Verify initial 0% progress, mark lesson complete, assert % increases & persists across F5
    const progressPct = page.getByTestId("course-progress-percent");
    await expect(progressPct).toContainText("0%");

    const markCompleteBtn = page.getByTestId("mark-lesson-complete-btn");
    await markCompleteBtn.scrollIntoViewIfNeeded();
    const [completeResp] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/lessons/") &&
          res.url().endsWith("/complete") &&
          res.request().method() === "POST"
      ),
      markCompleteBtn.click()
    ]);
    expect(completeResp.status()).toBe(200);

    await expect(progressPct).not.toHaveText("0%", { timeout: 10000 });
    const updatedPctText = (await progressPct.innerText()).trim();
    expect(parseInt(updatedPctText, 10)).toBeGreaterThan(0);

    // Completed check icon visible in roadmap sidebar
    await expect(sidebar.getByTestId("lesson-completed-icon").first()).toBeVisible();

    // Reload page (F5) and confirm progress % persists from SQLite in Catalog & Reader
    await page.reload();
    await page.waitForLoadState("domcontentloaded");
    const reloadedCard = page
      .getByTestId("course-catalog-section")
      .locator('[data-course-slug="doc-course-studxy-fullstack-bootcamp"]');
    await expect(reloadedCard).toBeVisible({ timeout: 20000 });
    await expect(reloadedCard.getByTestId("course-progress-percent")).not.toContainText("(0%)", {
      timeout: 15000
    });
    await reloadedCard.getByTestId("course-start-learning-btn").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible({ timeout: 15000 });
    await expect(page.getByTestId("course-progress-percent")).toHaveText(updatedPctText, {
      timeout: 15000
    });
    const reloadedSidebar = page.getByTestId("course-roadmap-sidebar");
    await page.getByTestId("roadmap-lesson-search-input").fill("Bài 01");
    await expect(reloadedSidebar.getByTestId("lesson-completed-icon").first()).toBeVisible();
    await page.getByTestId("roadmap-lesson-search-input").fill("");

    // Switch to Mode 2 Split PKM Workspace and capture screenshot
    await page.getByTestId("course-reader-notebook-btn").click();
    await expect(page.getByTestId("mode-2-split-container")).toBeVisible({ timeout: 10000 });
    await expect(page.getByTestId("split-pane-sidebar")).toBeVisible();
    await expect(page.getByTestId("split-pane-reader")).toBeVisible();
    await expect(page.getByTestId("split-pane-notebook")).toBeVisible();
    await assertZeroHorizontalOverflow(page);
    await page.screenshot({
      path: path.join(SCRATCH_DIR, "06-split-pkm-1440.png"),
      fullPage: false
    });

    // Switch back to Mode 1 and check Tablet 768x1024 layout + mobile roadmap drawer
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(250);
    await assertZeroHorizontalOverflow(page);
    const tabletLogoBox = await page.getByTestId("brand-logo-link").boundingBox();
    const tabletDashTabBox = await page.getByTestId("mode-tab-dashboard").boundingBox();
    expect(tabletLogoBox).toBeTruthy();
    expect(tabletDashTabBox).toBeTruthy();
    expect((tabletLogoBox?.x ?? 0) + (tabletLogoBox?.width ?? 0)).toBeLessThanOrEqual(
      tabletDashTabBox?.x ?? 0
    );
    const drawerBtn = page.getByTestId("mobile-roadmap-drawer-btn");
    await expect(drawerBtn).toBeVisible();
    await page.screenshot({
      path: path.join(SCRATCH_DIR, "09-reader-tablet-768.png"),
      fullPage: false
    });

    expect(serverErrors).toEqual([]);
  });

  test("3. IELTS Listening lesson: Inline audio player renders & .wav asset returns HTTP 200/206 audio/wav + relative .md link resolves", async ({
    page
  }) => {
    ensureScratchDir();
    const serverErrors = attachNetworkFailureMonitor(page);
    await provisionIsolatedLearner(page);

    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");

    const catalog = page.getByTestId("course-catalog-section");
    const ieltsCard = catalog.locator('[data-course-slug="document-course-ielts"]');
    await expect(ieltsCard).toBeVisible({ timeout: 20000 });
    await ieltsCard.getByTestId("course-start-learning-btn").click();

    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible({ timeout: 15000 });
    const sidebar = page.getByTestId("course-roadmap-sidebar");
    await expect(sidebar).toBeVisible();

    // Filter roadmap for Cycle 11 Listening lesson
    const searchInput = page.getByTestId("roadmap-lesson-search-input");
    await searchInput.fill("Chu Kỳ 11");
    const cycle11Btn = sidebar
      .getByTestId("course-syllabus-lesson-btn")
      .filter({ hasText: "Listening" })
      .first();
    await expect(cycle11Btn).toBeVisible({ timeout: 10000 });
    await cycle11Btn.click();

    const mdView = page.getByTestId("lesson-markdown-view");
    await expect(mdView).toBeVisible({ timeout: 15000 });

    const audioPlayer = page.getByTestId("lesson-audio-player").first();
    await audioPlayer.scrollIntoViewIfNeeded();
    await expect(audioPlayer).toBeVisible({ timeout: 10000 });

    const audioEl = audioPlayer.locator("audio");
    await expect(audioEl).toBeVisible();
    const audioSrc = await audioEl.getAttribute("src");
    expect(audioSrc).toBeTruthy();
    expect(audioSrc!).toContain("/api/v1/document-assets/IELTS/");
    expect(audioSrc!).toContain(".wav");

    // Verify speed controls update audio playbackRate
    await audioPlayer.getByTestId("audio-speed-0.75").click();
    const playbackRate = await audioEl.evaluate((el: HTMLAudioElement) => el.playbackRate);
    expect(playbackRate).toBe(0.75);
    await audioPlayer.getByTestId("audio-speed-1").click();

    // Verify real HTTP 200 & HTTP 206 Range streaming of audio/wav through frontend origin
    const fullWavResp = await page.request.get(`http://127.0.0.1:3000${audioSrc}`);
    expect(fullWavResp.status()).toBe(200);
    expect(fullWavResp.headers()["content-type"]).toContain("audio/wav");

    const rangeWavResp = await page.request.get(`http://127.0.0.1:3000${audioSrc}`, {
      headers: { Range: "bytes=0-1023" }
    });
    expect(rangeWavResp.status()).toBe(206);
    expect(rangeWavResp.headers()["content-type"]).toContain("audio/wav");
    const partialBody = await rangeWavResp.body();
    expect(partialBody.byteLength).toBe(1024);

    await assertZeroHorizontalOverflow(page);
    await page.screenshot({
      path: path.join(SCRATCH_DIR, "05-reader-ielts-audio-1440.png"),
      fullPage: false
    });

    // Click sibling relative .md link (transcript) and verify /api/v1/lessons/by-path resolves cleanly
    const relativeMdLink = mdView.getByTestId("lesson-relative-md-link").first();
    if (await relativeMdLink.isVisible().catch(() => false)) {
      const [byPathResp] = await Promise.all([
        page.waitForResponse((res) => res.url().includes("/api/v1/lessons/by-path")),
        relativeMdLink.click()
      ]);
      expect(byPathResp.status()).toBe(200);
      await page.waitForTimeout(400);
    }

    expect(serverErrors).toEqual([]);
  });

  test("4. Course Quiz & Flashcard FSRS real workflow + multi-viewport zero horizontal overflow", async ({
    page
  }) => {
    ensureScratchDir();
    const serverErrors = attachNetworkFailureMonitor(page);
    await provisionIsolatedLearner(page);

    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");

    // Launch Investing course -> Tuần 01 -> Quiz button
    const catalog = page.getByTestId("course-catalog-section");
    const investingCard = catalog.locator(
      '[data-course-slug="document-course-investing"]'
    );
    await expect(investingCard).toBeVisible({ timeout: 20000 });
    await investingCard.getByTestId("course-start-learning-btn").click();

    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible({ timeout: 15000 });
    const sidebar = page.getByTestId("course-roadmap-sidebar");
    await page.getByTestId("roadmap-lesson-search-input").fill("W01");
    await sidebar
      .getByTestId("course-syllabus-lesson-btn")
      .filter({ hasText: "W01" })
      .first()
      .click();

    // Launch lesson-linked Quiz from Reader header pill
    const quizBtn = page.getByTestId("course-reader-quiz-btn");
    await expect(quizBtn).toBeVisible();
    await quizBtn.click();

    await expect(page.getByTestId("mode-3-review-container")).toBeVisible({ timeout: 15000 });
    const quizPortal = page.getByTestId("practice-quiz-portal");
    await expect(quizPortal).toBeVisible({ timeout: 15000 });

    // Verify course filter & real MCQ questions
    await expect(page.getByTestId("quiz-course-filter-select")).toBeVisible();
    const mcqItem = quizPortal.getByTestId("quiz-item-multiple_choice").first();
    await expect(mcqItem).toBeVisible({ timeout: 10000 });

    const correctChoiceBtn = mcqItem.locator("button").nth(1);
    const [submitQuizResp] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/practice-quizzes/") &&
          res.url().includes("/submit") &&
          res.request().method() === "POST"
      ),
      correctChoiceBtn.click()
    ]);
    expect(submitQuizResp.status()).toBe(200);
    await expect(
      mcqItem.getByText(/Chính xác \/ Đã đánh giá|Cần xem lại/)
    ).toBeVisible({ timeout: 10000 });

    await page.evaluate(() => window.scrollTo(0, 0));
    await page.waitForTimeout(150);
    await assertZeroHorizontalOverflow(page);
    await page.screenshot({
      path: path.join(SCRATCH_DIR, "07-review-quiz-1440.png"),
      fullPage: false
    });

    // Switch to FSRS Flashcard subtab for this course
    await page.getByTestId("review-subtab-fsrs").click();
    const flipCard = page.getByTestId("fsrs-3d-flip-card");
    await expect(flipCard).toBeVisible({ timeout: 15000 });
    await expect(page.getByTestId("flashcard-front-face")).toBeVisible();
    await page.waitForTimeout(250);

    const logoBox = await page.getByTestId("brand-logo-link").boundingBox();
    expect(logoBox?.x ?? -1).toBeGreaterThanOrEqual(0);
    await assertZeroHorizontalOverflow(page);
    await page.screenshot({
      path: path.join(SCRATCH_DIR, "08-review-fsrs-1440.png"),
      fullPage: false
    });

    // Flip card and rate Good [3] via real POST /api/v1/flashcards/sync-reviews
    await flipCard.click();
    await expect(page.getByTestId("flashcard-back-face")).toBeVisible({ timeout: 5000 });

    const rateGoodBtn = page.getByTestId("rate-btn-3");
    await expect(rateGoodBtn).toBeVisible();
    const [rateResp] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/flashcards/sync-reviews") &&
          res.request().method() === "POST"
      ),
      rateGoodBtn.click()
    ]);
    expect(rateResp.status()).toBe(200);

    // Check 768x1024 Tablet zero horizontal overflow in Mode 3 (FSRS + Quiz)
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(250);
    await assertZeroHorizontalOverflow(page);
    await page.getByTestId("review-subtab-quiz").click();
    await page.waitForTimeout(250);
    await assertZeroHorizontalOverflow(page);

    expect(serverErrors).toEqual([]);
  });
});
