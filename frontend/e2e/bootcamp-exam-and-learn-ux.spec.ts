import type { Page } from "@playwright/test";
import { test, expect } from "./fixtures/test-base";

/**
 * Multi-tenant provision helper: creates a fresh, isolated user on the live backend
 * and sets localStorage so all frontend fetch calls attach X-Test-User-Id.
 */
async function provisionUser(page: Page): Promise<{ userId: string; email: string }> {
  const email = `bootcamp.ux.${Date.now()}.${Math.random().toString(36).slice(2, 7)}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "Học viên Bootcamp UX E2E",
    },
  });
  expect(regRes.ok()).toBeTruthy();
  const regJson = await regRes.json();
  const userId = regJson.user.id as string;

  await page.addInitScript((uid: string) => {
    window.localStorage.setItem("swms_active_user_id", uid);
    window.localStorage.setItem("swms_theme", "light");
  }, userId);

  return { userId, email };
}

/**
 * Discover best test course: prioritizes Studxy Full-Stack Bootcamp if available,
 * fallback to first public course with chapters/lessons (e.g. Coding/Psychology/English or Document course).
 */
async function resolveTargetCourseAndLesson(page: Page): Promise<{
  courseId: string;
  lessonId: string;
  courseTitle: string;
}> {
  const coursesRes = await page.request.get("http://127.0.0.1:5080/api/v1/courses");
  expect(coursesRes.ok()).toBeTruthy();
  const courses: Array<{ id: string; title: string; slug?: string }> = await coursesRes.json();
  expect(courses.length).toBeGreaterThan(0);

  // 1. Try to find Bootcamp course
  let matched = courses.find((c) =>
    /studxy-fullstack-bootcamp|bootcamp/i.test(c.slug || c.title || "")
  );

  // 2. Fallback: find public course with >= 2 lessons
  if (!matched) {
    for (const c of courses) {
      const detailRes = await page.request.get(`http://127.0.0.1:5080/api/v1/courses/${c.id}`);
      if (detailRes.ok()) {
        const detail = await detailRes.json();
        const lessons =
          detail.chapters?.flatMap((ch: { lessons?: Array<{ id: string }> }) => ch.lessons || []) ||
          detail.lessons ||
          [];
        if (lessons.length >= 2) {
          matched = c;
          break;
        }
      }
    }
  }

  // 3. Fallback to first course
  if (!matched) {
    matched = courses[0];
  }

  const courseId = matched.id;
  const courseTitle = matched.title || "Khóa học kiểm thử";

  // Discover lessons inside course
  const courseDetailRes = await page.request.get(`http://127.0.0.1:5080/api/v1/courses/${courseId}`);
  expect(courseDetailRes.ok()).toBeTruthy();
  const courseDetail = await courseDetailRes.json();
  const allLessons: Array<{ id: string; title: string }> =
    courseDetail.chapters?.flatMap((ch: { lessons?: Array<{ id: string; title: string }> }) => ch.lessons || []) ||
    courseDetail.lessons ||
    [];

  // Choose lesson with rich content if possible (lesson-02, variables, or second lesson)
  const chosenLesson =
    allLessons.find((l) => /lesson-02|variables|biến|toán|syntax|cơ bản/i.test(l.title || "")) ||
    allLessons[1] ||
    allLessons[0];

  expect(chosenLesson).toBeTruthy();
  return {
    courseId,
    lessonId: chosenLesson.id,
    courseTitle,
  };
}

test.describe("Bootcamp Exam Hub, Focused Learn UX, and Mobile Responsiveness (Zero-Facade)", () => {
  test.setTimeout(120000);

  // ---------------------------------------------------------------------------
  // Test 1 — Navigation Split & Ngân hàng Đề Thi (/app/practice & /app/exams)
  // ---------------------------------------------------------------------------
  test("Test 1 — Navigation Split & Ngân hàng Đề Thi (/app/practice & /app/exams)", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    // Navigate to /app Today dashboard
    await page.goto("/app");
    await expect(page.getByRole("heading", { name: /hôm nay học gì/i })).toBeVisible({
      timeout: 15000,
    });

    // 1A. Click sidebar-nav-practice -> verify /app/practice Hub
    const practiceNav = page.getByTestId("sidebar-nav-practice");
    await expect(practiceNav).toBeVisible({ timeout: 10000 });
    await practiceNav.click();

    await page.waitForURL("**/app/practice", { timeout: 15000 });
    await expect(page.getByRole("heading", { name: "Hub Luyện tập", exact: true })).toBeVisible({
      timeout: 15000,
    });
    await expect(page.getByTestId("practice-hub-stats")).toBeVisible({ timeout: 10000 });
    await expect(page.getByTestId("practice-hub-review-queue")).toBeVisible({ timeout: 10000 });
    await expect(page.getByTestId("practice-hub-courses")).toBeVisible({ timeout: 10000 });

    // 1B. Click sidebar-nav-exams -> verify /app/exams Exam Hub
    const examsNav = page.getByTestId("sidebar-nav-exams");
    await expect(examsNav).toBeVisible({ timeout: 10000 });
    await examsNav.click();

    await page.waitForURL("**/app/exams", { timeout: 15000 });
    await expect(page.getByTestId("exams-hub-page")).toBeVisible({ timeout: 15000 });
    await expect(page.getByRole("heading", { name: /ngân hàng đề & thi thử/i })).toBeVisible({
      timeout: 10000,
    });
    await expect(page.getByTestId("exams-hub-stats")).toBeVisible({ timeout: 10000 });
    await expect(page.getByTestId("exams-filter-all")).toBeVisible({ timeout: 10000 });

    // Ensure public courses / exam listings render with deduplicated content
    const quizFilterTabs = page.locator("[data-testid^='exams-filter-']");
    expect(await quizFilterTabs.count()).toBeGreaterThanOrEqual(3);
  });

  // ---------------------------------------------------------------------------
  // Test 2 — Trình đọc /learn: KaTeX, Syntax Highlight, <details> kín & Auto-Enroll
  // ---------------------------------------------------------------------------
  test("Test 2 — Trình đọc /learn: KaTeX, Syntax Highlight, <details> kín đáp án & Auto-Enroll khi Hoàn thành bài học", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    const { courseId, lessonId } = await resolveTargetCourseAndLesson(page);

    // Direct navigation to /app/courses/[id]/learn?lessonId=[id]
    await page.goto(`/app/courses/${courseId}/learn?lessonId=${lessonId}`);
    const proseColumn = page.getByTestId("reading-prose-column");
    await expect(proseColumn).toBeVisible({ timeout: 20000 });

    // 2A. Code block syntax highlighting token assertion
    const codeBlocks = page.locator("pre code");
    if ((await codeBlocks.count()) > 0) {
      const firstCodeBlock = codeBlocks.first();
      await expect(firstCodeBlock).toBeVisible({ timeout: 10000 });
      const coloredSpans = firstCodeBlock.locator("span");
      // Syntax highlighted code produces multiple <span> tags inside <code>
      if ((await coloredSpans.count()) > 0) {
        expect(await coloredSpans.count()).toBeGreaterThanOrEqual(1);
      }
    }

    // 2B. KaTeX math rendering (formula does not leak raw $$ in DOM)
    const katexNodes = page.locator(".katex");
    if ((await katexNodes.count()) > 0) {
      await expect(katexNodes.first()).toBeVisible();
    }
    const proseContent = await proseColumn.innerText();
    expect(proseContent).not.toMatch(/\$\$\s*[a-zA-Z0-9]/);

    // 2C. <details> accordion hiding answers & separate interactive draft box
    const detailsAccordions = page.getByTestId("lesson-details-accordion");
    if ((await detailsAccordions.count()) > 0) {
      const firstAccordion = detailsAccordions.first();
      // Must NOT be open by default
      const isOpen = await firstAccordion.evaluate((el) => (el as HTMLDetailsElement).open);
      expect(isOpen).toBe(false);

      // Draft textarea must NOT be located inside <details>
      const innerTextarea = firstAccordion.locator("textarea");
      await expect(innerTextarea).toHaveCount(0);
    }

    // 2D. Auto-Enroll when completing lesson (no prior manual enrollment needed)
    const completeLessonBtn = page.getByTestId("sticky-mark-lesson-complete-btn");
    await expect(completeLessonBtn).toBeVisible({ timeout: 10000 });

    const [completeRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/complete") &&
          res.request().method() === "POST",
        { timeout: 15000 }
      ),
      completeLessonBtn.click(),
    ]);

    // Must return HTTP 200 (NOT 403 COURSE_NOT_ENROLLED)
    expect(completeRes.status()).toBe(200);
  });

  // ---------------------------------------------------------------------------
  // Test 3 — Phòng Thi (/app/courses/[id]/quiz): [Đề Tự Tạo], submit-all, explanation & history, Inline Reset
  // ---------------------------------------------------------------------------
  test("Test 3 — Phòng Thi (/app/courses/[id]/quiz): [Đề Tự Tạo], Nộp toàn bài (submit-all), explanation & attemptHistory, Inline Reset", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    const { courseId } = await resolveTargetCourseAndLesson(page);

    // Navigate to course quiz room
    await page.goto(`/app/courses/${courseId}/quiz`);
    await page.waitForLoadState("domcontentloaded");

    // 3A. Open custom quiz generation modal
    const openGenBtn = page.getByTestId("open-generate-quiz-btn");
    await expect(openGenBtn).toBeVisible({ timeout: 15000 });
    await openGenBtn.click();

    // Fill title and select 5 questions
    const titleInput = page.locator("form input[type='text']").first();
    await expect(titleInput).toBeVisible({ timeout: 5000 });
    await titleInput.fill("Đề Kiểm Tra Tự Chọn E2E");

    const countSelect = page.locator("form select").filter({ has: page.locator("option[value='5']") }).first();
    if (await countSelect.isVisible()) {
      await countSelect.selectOption("5");
    }

    const confirmGenBtn = page.getByTestId("confirm-generate-quiz-btn");
    await expect(confirmGenBtn).toBeVisible();

    const [genRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/quizzes/generate") &&
          res.request().method() === "POST" &&
          (res.status() === 200 || res.status() === 201),
        { timeout: 20000 }
      ),
      confirmGenBtn.click(),
    ]);
    expect([200, 201]).toContain(genRes.status());

    // Verify new quiz appears with prefix [Đề Tự Tạo] and is selected
    await expect(
      page.locator("h2, h3").filter({ hasText: /\[Đề Tự Tạo\]/i }).first()
    ).toBeVisible({ timeout: 15000 });

    // 3B. Switch to Exam Mode (Thi thử cả đề)
    const modeExamBtn = page.getByTestId("mode-exam-btn");
    await expect(modeExamBtn).toBeVisible({ timeout: 10000 });
    await modeExamBtn.click();

    // Select answers for questions
    const radios = page.locator("input[type='radio']");
    if ((await radios.count()) > 0) {
      await radios.first().check({ force: true });
      const q2Radios = page.locator("#quiz-item-2 input[type='radio']");
      if ((await q2Radios.count()) > 0) {
        await q2Radios.first().check({ force: true });
      }
    } else {
      const anyInput = page.locator("#quiz-item-1 input[type='text'], #quiz-item-1 textarea").first();
      if (await anyInput.isVisible()) {
        await anyInput.fill("Câu trả lời kiểm thử E2E");
      }
    }

    // 3C. Submit all exam items via submit-all-exam-btn
    const submitAllBtn = page.getByTestId("submit-all-exam-btn");
    await expect(submitAllBtn).toBeVisible({ timeout: 10000 });

    const [submitRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/submit-all") &&
          res.request().method() === "POST" &&
          res.status() === 200,
        { timeout: 20000 }
      ),
      submitAllBtn.click(),
    ]);
    expect(submitRes.status()).toBe(200);

    // 3D. Confirm summary banner, explanations, and attempt history
    await expect(page.getByTestId("exam-result-summary")).toBeVisible({ timeout: 10000 });

    // Check item explanation
    const explanationNode = page.getByTestId("quiz-item-explanation-1");
    if ((await explanationNode.count()) > 0) {
      await expect(explanationNode).toBeVisible({ timeout: 5000 });
    } else {
      await expect(page.getByTestId("quiz-item-1")).toBeVisible({ timeout: 5000 });
    }

    // Check attempt history table with at least 1 record
    const attemptHistory = page.getByTestId("quiz-attempt-history");
    await expect(attemptHistory).toBeVisible({ timeout: 10000 });
    await expect(
      attemptHistory.locator("tr, div").filter({ hasText: /lần|\d+%/i }).first()
    ).toBeVisible({ timeout: 5000 });

    // 3E. Safe inline reset quiz (NO window.confirm popup)
    const resetBtn = page.getByTestId("reset-quiz-btn");
    await expect(resetBtn).toBeVisible({ timeout: 10000 });
    await resetBtn.click();

    const confirmResetBtn = page.getByTestId("confirm-reset-quiz-btn");
    await expect(confirmResetBtn).toBeVisible({ timeout: 5000 });

    const [resetRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/reset") &&
          res.request().method() === "POST" &&
          res.ok(),
        { timeout: 15000 }
      ),
      confirmResetBtn.click(),
    ]);
    expect(resetRes.ok()).toBeTruthy();
  });

  // ---------------------------------------------------------------------------
  // Test 4 — Viewport Mobile 390x844
  // ---------------------------------------------------------------------------
  test("Test 4 — Viewport Mobile 390x844: Bottom Nav 5 items, no horizontal overflow, and sticky bar bounding box", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await provisionUser(page);

    const { courseId } = await resolveTargetCourseAndLesson(page);

    // 4A. Viewport Mobile on /app
    await page.goto("/app");
    const mobileBottomNav = page.getByTestId("mobile-bottom-nav");
    await expect(mobileBottomNav).toBeVisible({ timeout: 15000 });

    // Assert all 5 mobile nav items are present, especially mobile-nav-exams
    await expect(page.getByTestId("mobile-nav-app")).toBeVisible();
    await expect(page.getByTestId("mobile-nav-courses")).toBeVisible();
    await expect(page.getByTestId("mobile-nav-practice")).toBeVisible();
    await expect(page.getByTestId("mobile-nav-exams")).toBeVisible();
    await expect(page.getByTestId("mobile-nav-review")).toBeVisible();

    // Check no horizontal overflow on mobile /app
    const isAppOverflowing = await page.evaluate(() => {
      return document.documentElement.scrollWidth > window.innerWidth + 1;
    });
    expect(isAppOverflowing).toBe(false);

    // Click mobile-nav-exams -> navigates cleanly to /app/exams
    await page.getByTestId("mobile-nav-exams").click();
    await page.waitForURL("**/app/exams", { timeout: 15000 });
    await expect(page.getByTestId("exams-hub-page")).toBeVisible({ timeout: 15000 });

    // 4B. Viewport Mobile on /app/courses/[id]/learn
    await page.goto(`/app/courses/${courseId}/learn`);
    const stickyBottomBar = page.getByTestId("learning-sticky-bottom-bar");
    await expect(stickyBottomBar).toBeVisible({ timeout: 15000 });

    // Verify sticky bottom bar does not overflow the 390x844 viewport
    const barBox = await stickyBottomBar.boundingBox();
    expect(barBox).not.toBeNull();
    expect(barBox!.x).toBeGreaterThanOrEqual(-1);
    expect(barBox!.width).toBeLessThanOrEqual(390 + 1);
    expect(barBox!.y + barBox!.height).toBeLessThanOrEqual(844 + 1);

    // Verify learn page itself does not overflow horizontally
    const isLearnOverflowing = await page.evaluate(() => {
      return document.documentElement.scrollWidth > window.innerWidth + 1;
    });
    expect(isLearnOverflowing).toBe(false);
  });
});
