import type { Page } from "@playwright/test";
import { test, expect } from "./fixtures/test-base";

const ENGLISH_COURSE_ID = "22222222-2222-4222-8222-222222222223";
const OS_COURSE_ID = "22222222-2222-4222-8222-222222222222";
const JAPANESE_COURSE_ID = "22222222-2222-4222-8222-222222222227";
const CHINESE_COURSE_ID = "22222222-2222-4222-8222-222222222228";

async function provisionUser(page: Page): Promise<string> {
  const email = `practice.live.${Date.now()}.${Math.random().toString(36).slice(2, 7)}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "Học viên Live Practice Test",
    },
  });
  expect(regRes.ok()).toBeTruthy();
  const regJson = await regRes.json();
  const userId = regJson.user.id as string;

  await page.addInitScript((uid: string) => {
    window.localStorage.setItem("swms_active_user_id", uid);
    window.localStorage.setItem("swms_theme", "light");
  }, userId);

  return userId;
}

async function resolveCuratedCourses(page: Page) {
  const coursesRes = await page.request.get("http://127.0.0.1:5080/api/v1/courses");
  expect(coursesRes.ok()).toBeTruthy();
  const courses: Array<{ id: string; title: string; slug?: string }> = await coursesRes.json();

  const bootcamp = courses.find(
    (c) => c.slug === "doc-course-studxy-fullstack-bootcamp" || /bootcamp/i.test(c.slug || c.title || "")
  );
  expect(bootcamp, "doc-course-studxy-fullstack-bootcamp must be found").toBeTruthy();

  const investing = courses.find(
    (c) => c.slug === "document-course-investing" || /investing|đầu tư/i.test(c.slug || c.title || "")
  );
  expect(investing, "document-course-investing must be found").toBeTruthy();

  const psychology = courses.find(
    (c) => c.slug === "document-course-psychology" || /psychology|tâm lý/i.test(c.slug || c.title || "")
  );
  expect(psychology, "document-course-psychology must be found").toBeTruthy();

  const english = courses.find(
    (c) => c.id === ENGLISH_COURSE_ID || /tieng-anh|english/i.test(c.slug || c.title || "")
  );
  expect(english, "English course must be found").toBeTruthy();

  const os = courses.find(
    (c) => c.id === OS_COURSE_ID || /he-dieu-hanh/i.test(c.slug || c.title || "")
  );
  expect(os, "OS course must be found").toBeTruthy();

  const japanese = courses.find(
    (c) => c.id === JAPANESE_COURSE_ID || /tieng-nhat|n5/i.test(c.slug || c.title || "")
  );
  expect(japanese, "Japanese N5 course must be found").toBeTruthy();

  const chinese = courses.find(
    (c) => c.id === CHINESE_COURSE_ID || /tieng-trung|hsk1/i.test(c.slug || c.title || "")
  );
  expect(chinese, "Chinese HSK1 course must be found").toBeTruthy();

  return {
    bootcampId: bootcamp!.id,
    investingId: investing!.id,
    psychologyId: psychology!.id,
    englishId: english!.id,
    osId: os!.id,
    japaneseId: japanese!.id,
    chineseId: chinese!.id,
  };
}

async function findPracticeLessonId(page: Page, courseId: string, slugHint?: string): Promise<string> {
  const detailRes = await page.request.get(`http://127.0.0.1:5080/api/v1/courses/${courseId}`);
  expect(detailRes.ok()).toBeTruthy();
  const detail = await detailRes.json();
  const allLessons: Array<{ id: string; title: string; slug?: string; practiceActivityCount?: number }> =
    detail.chapters?.flatMap((ch: { lessons?: Array<{ id: string; title: string; slug?: string; practiceActivityCount?: number }> }) => ch.lessons || []) ||
    detail.lessons ||
    [];

  if (slugHint) {
    const hintMatch = allLessons.find(
      (l) => l.slug?.includes(slugHint) && (l.practiceActivityCount ?? 0) > 0
    );
    if (hintMatch) return hintMatch.id;
  }

  const practiceMatch = allLessons.find((l) => (l.practiceActivityCount ?? 0) > 0);
  if (practiceMatch) return practiceMatch.id;

  return allLessons[0]?.id || "";
}

test.describe("Practice Expansion Live End-to-End Suite (NO MOCKS)", () => {
  test.setTimeout(120000);

  test("Comprehensive Real E2E: English Dictation, Coding Sandbox/Parsons, Finance Portfolio, Psychology Stroop, and Dashboard Live Stats", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });

    // Step 0: Provision real user on live backend (SQLite)
    const userId = await provisionUser(page);
    expect(userId).toBeTruthy();

    // Step 0.1: Resolve active course IDs from live API
    const {
      bootcampId,
      investingId,
      psychologyId,
      englishId,
      osId,
      japaneseId,
      chineseId,
    } = await resolveCuratedCourses(page);

    expect(bootcampId).toBeTruthy();
    expect(investingId).toBeTruthy();
    expect(psychologyId).toBeTruthy();
    expect(englishId).toBe(ENGLISH_COURSE_ID);
    expect(osId).toBe(OS_COURSE_ID);
    expect(japaneseId).toBe(JAPANESE_COURSE_ID);
    expect(chineseId).toBe(CHINESE_COURSE_ID);

    // -----------------------------------------------------------------------
    // SCENARIO 1: English Course (Dictation & Word Map) via Fullscreen Practice
    // -----------------------------------------------------------------------
    const englishLessonId = await findPracticeLessonId(page, englishId);
    await page.goto(`/app/courses/${englishId}/learn${englishLessonId ? `?lessonId=${englishLessonId}` : ""}`);
    await page.waitForLoadState("domcontentloaded");

    await expect(page.getByTestId("center-tab-practice")).toHaveCount(0);
    await expect(page.getByTestId("lesson-quick-check-section")).toBeVisible({ timeout: 15000 });
    const engGoPracticeBtn = page.getByTestId("go-to-lesson-practice-btn");
    await expect(engGoPracticeBtn).toBeVisible({ timeout: 15000 });
    await engGoPracticeBtn.click();
    await page.waitForURL(`**/app/practice/${englishId}/**`, { timeout: 15000 });
    await expect(page.getByTestId("practice-back-to-lesson-btn")).toBeVisible({ timeout: 10000 });

    await expect(page.getByTestId("english-practice-lab")).toBeVisible({ timeout: 15000 });

    // 1A. Dictation exercise
    await page.getByTestId("practice-tab-dictation").click();
    const typeBtn = page.getByRole("button", { name: /Gõ phím/i });
    if (await typeBtn.isVisible()) {
      await typeBtn.click();
    }

    const dictationInput = page.locator("textarea");
    await expect(dictationInput).toBeVisible({ timeout: 10000 });
    await dictationInput.fill("Effective communication builds trust and long term relationships.");

    const [dictationAttemptRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/practice/attempts") &&
          res.request().method() === "POST" &&
          res.status() === 200,
        { timeout: 15000 }
      ),
      page.getByRole("button", { name: "Kiểm tra đáp án" }).click(),
    ]);
    expect(dictationAttemptRes.status()).toBe(200);

    // 1B. Word Map
    await page.getByTestId("practice-tab-word_map").click();
    await expect(page.getByText(/Sơ Đồ Từ Vựng|Word Map|Từ cốt lõi/i).first()).toBeVisible({
      timeout: 10000,
    });

    // -----------------------------------------------------------------------
    // SCENARIO 2: Bootcamp Course (JS Web Worker Sandbox & Parsons Problem)
    // -----------------------------------------------------------------------
    const bootcampLessonId = await findPracticeLessonId(page, bootcampId, "phase-3");
    await page.goto(`/app/courses/${bootcampId}/learn?lessonId=${bootcampLessonId}`);
    await page.waitForLoadState("domcontentloaded");

    const codingPracticeBtn = page.getByTestId("go-to-lesson-practice-btn");
    await expect(codingPracticeBtn).toBeVisible({ timeout: 15000 });
    await codingPracticeBtn.click();
    await page.waitForURL(`**/app/practice/${bootcampId}/**`, { timeout: 15000 });

    await expect(page.getByTestId("coding-practice-lab")).toBeVisible({ timeout: 15000 });

    // 2A. JS Sandbox: run code in Web Worker
    const jsSandboxTab = page.getByTestId("mode-tab-js-sandbox");
    if (await jsSandboxTab.isVisible()) {
      await jsSandboxTab.click();
      await expect(page.getByTestId("js-sandbox-container")).toBeVisible({ timeout: 10000 });

      await page.getByTestId("js-sandbox-run-btn").click();
      await expect(page.getByTestId("js-sandbox-results")).toBeVisible({ timeout: 15000 });
    }

    // 2B. Parsons Problem: line assembly with no key errors
    const parsonsTab = page.getByTestId("mode-tab-parsons");
    if (await parsonsTab.isVisible()) {
      await parsonsTab.click();
      await expect(page.getByTestId("parsons-container")).toBeVisible({ timeout: 10000 });

      const bankLine = page.getByTestId("parsons-bank-line").first();
      await expect(bankLine).toBeVisible({ timeout: 10000 });
      await bankLine.click();

      await page.getByTestId("parsons-check-btn").click();
      await expect(page.getByTestId("parsons-result-card")).toBeVisible({ timeout: 10000 });
    }

    // -----------------------------------------------------------------------
    // SCENARIO 3: Finance Course (Compound & Portfolio Simulator)
    // -----------------------------------------------------------------------
    const investingLessonId = await findPracticeLessonId(page, investingId, "module-");
    await page.goto(`/app/courses/${investingId}/learn?lessonId=${investingLessonId}`);
    await page.waitForLoadState("domcontentloaded");

    const financePracticeBtn = page.getByTestId("go-to-lesson-practice-btn");
    await expect(financePracticeBtn).toBeVisible({ timeout: 15000 });
    await financePracticeBtn.click();
    await page.waitForURL(`**/app/practice/${investingId}/**`, { timeout: 15000 });

    await expect(page.getByTestId("finance-practice-lab")).toBeVisible({ timeout: 15000 });

    // 3A. Compound Calculator
    await page.getByTestId("tab-compound").click();
    await expect(page.getByTestId("finance-practice-lab").getByText(/Lãi Kép|DCA/i).first()).toBeVisible({ timeout: 10000 });

    // 3B. Portfolio Simulator
    await page.getByTestId("tab-portfolio").click();
    await expect(
      page.getByText("Dữ liệu mẫu lịch sử mô phỏng phục vụ thực hành — không phải khuyến nghị đầu tư")
    ).toBeVisible({ timeout: 10000 });

    const [portfolioAttemptRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/practice/attempts") &&
          res.request().method() === "POST" &&
          res.status() === 200,
        { timeout: 15000 }
      ),
      page.getByTestId("portfolio-record-btn").click(),
    ]);
    expect(portfolioAttemptRes.status()).toBe(200);

    // -----------------------------------------------------------------------
    // SCENARIO 4: Psychology Course (Cognitive Labs - Stroop Test)
    // -----------------------------------------------------------------------
    const psychologyLessonId = await findPracticeLessonId(page, psychologyId, "notes-");
    await page.goto(`/app/courses/${psychologyId}/learn?lessonId=${psychologyLessonId}`);
    await page.waitForLoadState("domcontentloaded");

    const psychPracticeBtn = page.getByTestId("go-to-lesson-practice-btn");
    await expect(psychPracticeBtn).toBeVisible({ timeout: 15000 });
    await psychPracticeBtn.click();
    await page.waitForURL(`**/app/practice/${psychologyId}/**`, { timeout: 15000 });

    await expect(page.getByTestId("psychology-practice-lab")).toBeVisible({ timeout: 15000 });

    await page.getByTestId("tab-cognitive-labs").click();
    await expect(page.getByTestId("stroop-test-container")).toBeVisible({ timeout: 10000 });

    // Start Stroop test (10 trials, no microphone needed)
    await page.getByTestId("stroop-start-btn").click();

    for (let i = 0; i < 10; i++) {
      const redBtn = page.getByTestId("stroop-btn-red");
      await redBtn.waitFor({ state: "visible", timeout: 5000 });
      await redBtn.click();
      await page.waitForTimeout(60);
    }

    await expect(page.getByTestId("stroop-results-card")).toBeVisible({ timeout: 10000 });

    // -----------------------------------------------------------------------
    // SCENARIO 5: Navigate to /app & Confirm Practice Stats > 0
    // -----------------------------------------------------------------------
    const [dashboardStatsRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/api/v1/practice/stats") && res.status() === 200,
        { timeout: 15000 }
      ),
      page.goto("/app"),
    ]);
    expect(dashboardStatsRes.status()).toBe(200);

    await expect(page.getByTestId("today-practice-stats")).toBeVisible({ timeout: 15000 });
    await expect(page.getByTestId("today-practice-error")).not.toBeVisible();
    await expect(page.getByTestId("today-practice-empty-state")).not.toBeVisible();

    // Verify total attempts tile shows at least 1 lượt (in fact >= 2 real attempts saved!)
    const statsContainer = page.getByTestId("today-practice-stats");
    await expect(statsContainer.getByText(/[1-9]\d*\s*lượt/)).toBeVisible({ timeout: 10000 });

    // -----------------------------------------------------------------------
    // SCENARIO 6: Navigate to dedicated Hub /app/practice & verify real data
    // -----------------------------------------------------------------------
    const [hubStatsRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/api/v1/practice/stats") && res.status() === 200,
        { timeout: 15000 }
      ),
      page.goto("/app/practice"),
    ]);
    expect(hubStatsRes.status()).toBe(200);

    await expect(page.getByRole("heading", { name: "Hub Luyện tập" })).toBeVisible({ timeout: 15000 });
    await expect(page.getByTestId("practice-hub-error")).toHaveCount(0);
    await expect(page.getByTestId("practice-hub-stats")).toBeVisible({ timeout: 10000 });
    await expect(page.getByTestId("practice-hub-stats").getByText(/[1-9]\d*/).first()).toBeVisible();
    await expect(page.getByTestId("practice-hub-review-queue")).toBeVisible({ timeout: 10000 });
    await expect(page.getByTestId("practice-hub-courses")).toBeVisible({ timeout: 10000 });
  });
});
