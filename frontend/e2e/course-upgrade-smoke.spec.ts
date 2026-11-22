import type { Page, Response } from "@playwright/test";
import { test, expect } from "./fixtures/test-base";

const ACADEMIC_OS_COURSE_ID = "22222222-2222-4222-8222-222222222222";

async function provisionIsolatedLearner(page: Page): Promise<string> {
  const email = `e2e.upgrade.${Date.now()}.${Math.random().toString(36).slice(2, 6)}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:3000/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "Học viên Upgrade E2E",
    },
  });
  expect(regRes.status()).toBe(200);
  const regJson = await regRes.json();
  const userId = regJson.user.id as string;
  expect(userId).toBeTruthy();

  await page.addInitScript(
    ({ uid }) => {
      window.localStorage.setItem("swms_active_user_id", uid);
      window.localStorage.setItem("swms_has_refresh_cookie", "1");
    },
    { uid: userId }
  );

  return userId;
}

function attachServerErrors(page: Page): string[] {
  const errors: string[] = [];
  page.on("response", (resp: Response) => {
    if (resp.status() >= 500) {
      errors.push(`[HTTP ${resp.status()}] ${resp.request().method()} ${resp.url()}`);
    }
  });
  return errors;
}

test.describe("Upgraded Courses & Features Real E2E", () => {
  test("Course Detail, AI Tutor, Exam Mode / Reset / Custom Quiz, and Certificate Transcript work end-to-end", async ({
    page,
  }) => {
    const serverErrors = attachServerErrors(page);
    await provisionIsolatedLearner(page);

    // 1. Course Detail: OS course shows Practice Lab, FSRS CTA, Study Guide download, 6 lessons
    await page.goto(`/app/courses/${ACADEMIC_OS_COURSE_ID}`);
    await expect(page.getByTestId("course-review-fsrs-btn")).toBeVisible({ timeout: 15000 });
    await expect(page.getByTestId("download-study-guide-btn")).toBeVisible();
    await expect(page.getByText(/Practice Lab/i).first()).toBeVisible();

    const [guideResp, download] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes(`/api/v1/courses/${ACADEMIC_OS_COURSE_ID}/study-guide`) &&
          res.request().method() === "GET"
      ),
      page.waitForEvent("download"),
      page.getByTestId("download-study-guide-btn").click(),
    ]);
    expect(guideResp.status()).toBe(200);
    expect(download.suggestedFilename()).toContain("-de-cuong.md");

    // 2. Certificate Page: 4-Dimensional Academic Transcript renders
    await page.goto(`/app/courses/${ACADEMIC_OS_COURSE_ID}/certificate`);
    const transcript = page.getByTestId("certificate-academic-transcript");
    await expect(transcript).toBeVisible({ timeout: 15000 });
    await expect(transcript.getByText("Practice Lab")).toBeVisible();
    await expect(transcript.getByText("Trí nhớ FSRS v5")).toBeVisible();

    // 3. Quiz Page: Custom Quiz Generation + Thi thử cả đề (/submit-all) + Làm lại đề (/reset)
    await page.goto(`/app/courses/${ACADEMIC_OS_COURSE_ID}/quiz`);
    await expect(page.getByTestId("open-generate-quiz-btn")).toBeVisible({ timeout: 15000 });

    await page.getByTestId("open-generate-quiz-btn").click();
    const [genResp] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes(`/api/v1/courses/${ACADEMIC_OS_COURSE_ID}/quizzes/generate`) &&
          res.request().method() === "POST"
      ),
      page.getByTestId("confirm-generate-quiz-btn").click(),
    ]);
    expect(genResp.status()).toBe(201);

    await page.getByTestId("mode-exam-btn").click();
    await expect(page.getByTestId("submit-all-exam-btn")).toBeVisible();

    // Fill first question (whether MCQ radio or short answer input)
    const firstItem = page.getByTestId("quiz-item-1");
    await expect(firstItem).toBeVisible();
    const radioCount = await firstItem.locator('input[type="radio"]').count();
    if (radioCount > 0) {
      await firstItem.locator('input[type="radio"]').first().check();
    } else {
      await firstItem.locator("input, textarea").first().fill("Phân trang và quản lý bộ nhớ ảo");
    }

    const [submitAllResp] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/submit-all") && res.request().method() === "POST"
      ),
      page.getByTestId("submit-all-exam-btn").click(),
    ]);
    expect(submitAllResp.status()).toBe(200);

    const [resetResp] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/reset") && res.request().method() === "POST"
      ),
      page.getByTestId("reset-quiz-btn").click(),
    ]);
    expect(resetResp.status()).toBe(200);

    // 4. Learn Workspace: open Study Drawer -> AI Gia sư drawer tab & tutor-chat endpoint
    await page.goto(`/app/courses/${ACADEMIC_OS_COURSE_ID}/learn`);
    const drawerToggle = page.getByRole("button", { name: /Sổ tay & Thẻ/i });
    await expect(drawerToggle).toBeVisible({ timeout: 15000 });
    await drawerToggle.click();

    const aiTutorTab = page.getByRole("button", { name: /AI Gia sư/i });
    await expect(aiTutorTab).toBeVisible({ timeout: 5000 });
    await aiTutorTab.click();

    await expect(page.getByTestId("ai-tutor-panel")).toBeVisible();
    await page.getByTestId("ai-tutor-input").fill("Tóm tắt ý chính bài này");
    const [tutorResp] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/api/v1/ai/tutor-chat") && res.request().method() === "POST"
      ),
      page.getByTestId("ai-tutor-send-btn").click(),
    ]);
    expect(tutorResp.status()).toBe(200);
    const saveTutorCardBtn = page.getByRole("button", { name: /Tạo thẻ FSRS/i }).first();
    await expect(saveTutorCardBtn).toBeVisible();
    const [saveCardResp] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/api/v1/flashcards") && res.request().method() === "POST"
      ),
      saveTutorCardBtn.click(),
    ]);
    expect([200, 201]).toContain(saveCardResp.status());

    expect(serverErrors).toEqual([]);
  });
});
