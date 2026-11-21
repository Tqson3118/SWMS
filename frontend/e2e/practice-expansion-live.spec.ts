import type { Page } from "@playwright/test";
import { test, expect } from "./fixtures/test-base";

const ENGLISH_COURSE_ID = "22222222-2222-4222-8222-222222222223";
const INVESTING_COURSE_ID = "22222222-2222-4222-8222-222222222224";
const CODING_COURSE_ID = "22222222-2222-4222-8222-222222222225";
const PSYCHOLOGY_COURSE_ID = "22222222-2222-4222-8222-222222222226";

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

test.describe("Practice Expansion Live End-to-End Suite (NO MOCKS)", () => {
  test.setTimeout(120000);

  test("Comprehensive Real E2E: English Dictation, Coding Sandbox/Parsons, Finance Portfolio, Psychology Stroop, and Dashboard Live Stats", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });

    // Step 0: Provision real user on live backend (SQLite)
    const userId = await provisionUser(page);
    expect(userId).toBeTruthy();

    // -----------------------------------------------------------------------
    // SCENARIO 1: English Course (Dictation & Word Map)
    // -----------------------------------------------------------------------
    await page.goto(`/app/courses/${ENGLISH_COURSE_ID}/learn`);
    await page.waitForLoadState("domcontentloaded");

    const practiceTabBtn = page.getByTestId("center-tab-practice");
    await expect(practiceTabBtn).toBeVisible({ timeout: 15000 });
    await practiceTabBtn.click();

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
    // SCENARIO 2: Coding Course (JS Web Worker Sandbox & Parsons Problem)
    // -----------------------------------------------------------------------
    await page.goto(`/app/courses/${CODING_COURSE_ID}/learn`);
    await page.waitForLoadState("domcontentloaded");

    const codingPracticeBtn = page.getByTestId("center-tab-practice");
    await expect(codingPracticeBtn).toBeVisible({ timeout: 15000 });
    await codingPracticeBtn.click();

    await expect(page.getByTestId("coding-practice-lab")).toBeVisible({ timeout: 15000 });

    // 2A. JS Sandbox: run code in Web Worker
    await page.getByTestId("mode-tab-js-sandbox").click();
    await expect(page.getByTestId("js-sandbox-container")).toBeVisible({ timeout: 10000 });

    await page.getByTestId("js-sandbox-run-btn").click();
    await expect(page.getByTestId("js-sandbox-results")).toBeVisible({ timeout: 15000 });

    // 2B. Parsons Problem: line assembly with no key errors
    await page.getByTestId("mode-tab-parsons").click();
    await expect(page.getByTestId("parsons-container")).toBeVisible({ timeout: 10000 });

    const bankLine = page.getByTestId("parsons-bank-line").first();
    await expect(bankLine).toBeVisible({ timeout: 10000 });
    await bankLine.click();

    await page.getByTestId("parsons-check-btn").click();
    await expect(page.getByTestId("parsons-result-card")).toBeVisible({ timeout: 10000 });

    // -----------------------------------------------------------------------
    // SCENARIO 3: Finance Course (Compound & Portfolio Simulator)
    // -----------------------------------------------------------------------
    await page.goto(`/app/courses/${INVESTING_COURSE_ID}/learn`);
    await page.waitForLoadState("domcontentloaded");

    const financePracticeBtn = page.getByTestId("center-tab-practice");
    await expect(financePracticeBtn).toBeVisible({ timeout: 15000 });
    await financePracticeBtn.click();

    await expect(page.getByTestId("finance-practice-lab")).toBeVisible({ timeout: 15000 });

    // 3A. Compound Calculator
    await page.getByTestId("tab-compound").click();
    await expect(page.getByText(/Lãi Kép|DCA/i).first()).toBeVisible({ timeout: 10000 });

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
    await page.goto(`/app/courses/${PSYCHOLOGY_COURSE_ID}/learn`);
    await page.waitForLoadState("domcontentloaded");

    const psychPracticeBtn = page.getByTestId("center-tab-practice");
    await expect(psychPracticeBtn).toBeVisible({ timeout: 15000 });
    await psychPracticeBtn.click();

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
  });
});
