import { test, expect } from "@playwright/test";
import fs from "node:fs";
import path from "node:path";

const SAMPLE_ROADMAP_ZIP = path.resolve(__dirname, "../../examples/sample-swm-roadmap.zip");

async function provisionIsolatedUser(page: any, prefix = "v2.complete") {
  const email = `${prefix}.${Date.now()}.${Math.random().toString(36).slice(2, 7)}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "V2 Completeness Tester",
    },
  });
  expect(regRes.ok()).toBeTruthy();
  const regJson = await regRes.json();
  const userId = regJson.user.id as string;

  await page.addInitScript((uid: string) => {
    window.localStorage.setItem("swms_active_user_id", uid);
  }, userId);

  return { userId, email };
}

async function importSampleRoadmap(page: any, userId: string) {
  const zipBuffer = fs.readFileSync(SAMPLE_ROADMAP_ZIP);
  const res = await page.request.post("http://127.0.0.1:5080/api/v1/courses/import-zip", {
    headers: {
      "X-Test-User-Id": userId,
    },
    multipart: {
      file: {
        name: "sample-swm-roadmap.zip",
        mimeType: "application/zip",
        buffer: zipBuffer,
      },
    },
  });
  expect(res.ok()).toBeTruthy();
  const json = await res.json();
  return json.courseId as string;
}

test.describe("V2 App Completeness & End-to-End Coherence", () => {
  test("Landing V2 theme sync, shortcuts ?, /auth?tab=register -> /app redirect, & /app <-> /workspace switcher", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1366, height: 820 });
    await page.goto("/");

    await expect(page.getByText("studxy.vn/app")).toBeVisible();
    await expect(page.getByText("© 2026")).toBeVisible();

    // Toggle theme on landing -> verify swms_theme persistence
    await page.getByRole("button", { name: "Toggle theme" }).click();
    await expect
      .poll(async () => page.evaluate(() => window.localStorage.getItem("swms_theme")))
      .toBe("dark");
    await expect(page.locator("html")).toHaveClass(/dark/);

    // Press '?' -> smoothly scrolls to #shortcuts section
    await page.keyboard.press("?");
    await expect(page.locator("#shortcuts")).toBeInViewport({ timeout: 5000 });

    // /auth?tab=register opens Register tab automatically and redirects to /app
    await page.goto("/auth?tab=register");
    await expect(page.getByTestId("auth-fullname-input")).toBeVisible();

    const uniqueEmail = `v2.auth.${Date.now()}@studxy.vn`;
    await page.getByTestId("auth-fullname-input").fill("Tester Mới V2");
    await page.getByTestId("auth-email-input").fill(uniqueEmail);
    await page.getByTestId("auth-password-input").fill("StrongPass123!");
    await page.getByTestId("auth-submit-btn").click();

    await page.waitForURL("**/app", { timeout: 15000 });
    await expect(page.locator("html")).toHaveClass(/dark/);

    // Switch /app <-> /workspace in 1 click
    await page.getByTestId("switch-to-workspace-link").click();
    await page.waitForURL("**/workspace", { timeout: 15000 });

    await page.getByTestId("user-profile-btn").click();
    await page.getByTestId("switch-to-app-v2-link").click();
    await page.waitForURL("**/app", { timeout: 15000 });
  });

  test("Course progress %, Next Lesson linking, Notebook library filter, & Course Quiz evaluation", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1366, height: 820 });
    const { userId } = await provisionIsolatedUser(page, "v2.progress");
    const courseId = await importSampleRoadmap(page, userId);

    // 1. Course Overview initially shows 0/4 and 0%
    await page.goto(`/app/courses/${courseId}`);
    await expect(page.getByText("Tiến độ: 0/4 bài")).toBeVisible({ timeout: 15000 });
    await expect(page.getByText("0%", { exact: true }).first()).toBeVisible();

    const continueBtn = page.getByTestId("course-continue-learn-btn");
    await expect(continueBtn).toContainText("Bắt đầu học");
    const lesson1Href = await continueBtn.getAttribute("href");
    expect(lesson1Href).toContain("/learn?lesson=");

    // 2. Open Learn page and mark lesson 1 completed
    await continueBtn.click();
    await page.waitForURL(`**/app/courses/${courseId}/learn**`, { timeout: 15000 });
    await expect(page.getByTestId("reading-prose-column")).toBeVisible({ timeout: 15000 });

    const completeBtn = page.getByTestId("mark-lesson-complete-btn").first();
    await expect(completeBtn).toBeVisible({ timeout: 10000 });
    const completeRespPromise = page.waitForResponse(
      (resp) => resp.url().includes("/complete") && resp.request().method() === "POST"
    );
    await completeBtn.click();
    const completeResp = await completeRespPromise;
    expect(completeResp.status()).toBe(200);

    // 3. Revisit Course Overview -> progress updated to 1/4 (25%) and Next Lesson points to Lesson 2
    await page.goto(`/app/courses/${courseId}`);
    await expect(page.getByText("Tiến độ: 1/4 bài")).toBeVisible({ timeout: 15000 });
    await expect(page.getByText("25%", { exact: true }).first()).toBeVisible();
    await expect(continueBtn).toContainText("Tiếp tục học");
    const lesson2Href = await continueBtn.getAttribute("href");
    expect(lesson2Href).toContain("/learn?lesson=");
    expect(lesson2Href).not.toBe(lesson1Href);

    // 4. Verify Library displays real progress & Notebooks tab is populated
    await page.goto("/app/library");
    await page.getByTestId("library-filter-course").click();
    await expect(page.getByText("25% hoàn thành")).toBeVisible({ timeout: 10000 });

    await page.getByTestId("library-filter-notebook").click();
    await expect(page.locator("main")).not.toContainText("Chưa có tài liệu nào");

    // 5. Course Quiz page loads & evaluates submitted item against backend
    await page.goto(`/app/courses/${courseId}/quiz`);
    const firstQuizItem = page.getByTestId("quiz-item-1");
    await expect(firstQuizItem).toBeVisible({ timeout: 15000 });

    const radio = firstQuizItem.locator('input[type="radio"]').first();
    if (await radio.isVisible().catch(() => false)) {
      await radio.check();
    } else {
      await firstQuizItem.locator("input, textarea").first().fill("FSRS v5");
    }

    const submitRespPromise = page.waitForResponse(
      (resp) => resp.url().includes("/submit") && resp.request().method() === "POST"
    );
    await firstQuizItem.getByRole("button", { name: /Kiểm tra|Nộp lại/i }).click();
    const submitResp = await submitRespPromise;
    expect([200, 202]).toContain(submitResp.status());
    await expect(firstQuizItem.getByText(/Chính xác|Chưa đúng/)).toBeVisible({ timeout: 10000 });
  });

  test("FSRS Flashcards /app/review loads real due cards, syncs ratings, and performs real server Undo", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1366, height: 820 });
    const { userId } = await provisionIsolatedUser(page, "v2.review");
    const courseId = await importSampleRoadmap(page, userId);

    await page.goto(`/app/review?courseId=${courseId}`);
    await expect(page.getByTestId("v2-review-page")).toBeVisible({ timeout: 15000 });
    await expect(page.getByTestId("review-progress-count")).toHaveText(/1 \/ 6/);

    const frontText = await page.getByTestId("review-card-front").innerText();
    expect(frontText.trim().length).toBeGreaterThan(5);
    expect(frontText).not.toContain("Câu hỏi flashcard");

    // Flip card & verify rating previews
    await page.getByTestId("review-flip-btn").click();
    await expect(page.getByTestId("review-rate-3")).toBeVisible();

    const syncRespPromise = page.waitForResponse(
      (resp) => resp.url().includes("/api/v1/flashcards/sync-reviews") && resp.request().method() === "POST"
    );
    await page.getByTestId("review-rate-3").click();
    const syncResp = await syncRespPromise;
    expect(syncResp.status()).toBe(200);
    await expect(page.getByTestId("review-progress-count")).toHaveText(/2 \/ 6/);

    // Server Undo via /undo-last-review
    const undoRespPromise = page.waitForResponse(
      (resp) => resp.url().includes("/undo-last-review") && resp.request().method() === "POST"
    );
    await page.getByTestId("review-undo-btn").click();
    const undoResp = await undoRespPromise;
    expect(undoResp.status()).toBe(200);
    await expect(page.getByTestId("review-undo-banner")).toBeVisible();
    await expect(page.getByTestId("review-progress-count")).toHaveText(/1 \/ 6/);
  });

  test("Course Studio /app/studio/[id] edits + adds chapters/lessons persistently & Ctrl+K routes directly", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1366, height: 820 });
    const { userId } = await provisionIsolatedUser(page, "v2.studio");
    const courseId = await importSampleRoadmap(page, userId);

    await page.goto(`/app/studio/${courseId}`);
    await expect(page.getByTestId("v2-studio-editor")).toBeVisible({ timeout: 15000 });
    await expect(page.getByTestId("studio-lesson-title-input")).not.toHaveValue("");

    // Add new chapter
    await page.getByTestId("studio-add-chapter-btn").click();
    await page.getByTestId("studio-new-chapter-input").fill("Chương 3: Kiểm chứng E2E");
    const addChPromise = page.waitForResponse(
      (resp) => resp.url().includes("/chapters") && resp.request().method() === "POST"
    );
    await page.getByTestId("studio-create-chapter-confirm").click();
    expect((await addChPromise).status()).toBe(200);
    await expect(page.getByRole("button", { name: /Chương 3: Kiểm chứng E2E/ })).toBeVisible();

    // Add new lesson to the newly added chapter
    await page.locator('[data-testid^="studio-add-lesson-btn-"]').last().click();
    await page.locator('[data-testid^="studio-new-lesson-input-"]').last().fill("Bài E2E Markdown Studio");
    const addLessonPromise = page.waitForResponse(
      (resp) => resp.url().includes("/lessons") && resp.request().method() === "POST"
    );
    await page.locator('[data-testid^="studio-create-lesson-confirm-"]').last().click();
    expect((await addLessonPromise).status()).toBe(200);

    // Edit contentMarkdown and Save via PUT /api/v1/lessons/{id}
    const mdPayload = "# Tài liệu E2E Studio\n\nNội dung kiểm chứng persistence trên Backend.";
    await page.getByTestId("studio-lesson-markdown-input").fill(mdPayload);
    const savePromise = page.waitForResponse(
      (resp) => resp.url().includes("/api/v1/lessons/") && resp.request().method() === "PUT"
    );
    await page.getByTestId("studio-save-lesson-btn").click();
    expect((await savePromise).status()).toBe(200);
    await expect(page.getByTestId("studio-save-banner")).toBeVisible();

    // Reload and verify persistence
    await page.reload();
    await expect(page.getByRole("button", { name: /Chương 3: Kiểm chứng E2E/ })).toBeVisible({ timeout: 15000 });
    const createdLessonBtn = page.getByRole("button", { name: "Bài E2E Markdown Studio" });
    await expect(createdLessonBtn).toBeVisible();
    await createdLessonBtn.click();
    await expect(page.getByTestId("studio-lesson-markdown-input")).toHaveValue(mdPayload);

    // Ctrl+K Command Palette in /app routes directly to /app/courses/[id]/learn?lesson=...
    await page.goto("/app");
    await page.waitForLoadState("networkidle");
    await page.keyboard.press("Control+k");
    await expect(page.getByTestId("command-palette-modal")).toBeVisible();
    await page.getByTestId("command-palette-input").fill("Bài E2E Markdown Studio");

    const lessonResult = page.getByTestId("cmd-result-lesson").first();
    await expect(lessonResult).toBeVisible({ timeout: 10000 });
    await expect(lessonResult).toContainText("Chương 3: Kiểm chứng E2E");
    await lessonResult.click();
    await page.waitForURL(`**/app/courses/${courseId}/learn?lesson=**`, { timeout: 15000 });
  });

  test("Auth Google provider panel, collapsible Demo Switcher, Landing CTA state, & Settings password change", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1366, height: 820 });

    const providersPromise = page.waitForResponse(
      (r) => r.url().includes("/api/v1/auth/providers") && r.request().method() === "GET"
    );
    await page.goto("/auth");
    const providersResp = await providersPromise;
    expect(providersResp.status()).toBe(200);
    const providersJson = await providersResp.json();

    await expect(page.getByTestId("google-signin-section")).toBeVisible();
    if (!providersJson.googleEnabled) {
      await page.getByTestId("google-signin-unconfigured-btn").click();
      await expect(page.getByTestId("google-setup-guide")).toBeVisible();
    } else {
      await expect(page.getByTestId("google-signin-button-container")).toBeVisible();
    }

    // Demo switcher starts collapsed until toggled open
    await expect(page.getByTestId("demo-user-switcher")).toBeHidden();
    await page.getByTestId("demo-switcher-toggle").click();
    await expect(page.getByTestId("demo-user-switcher")).toBeVisible();
    await expect(page.getByTestId("switch-user-default")).toBeVisible();

    // Register a new user -> change password in /app/settings -> re-login with new password
    const uniqueEmail = `v2.pwchange.${Date.now()}@studxy.vn`;
    await page.getByTestId("auth-tab-register").click();
    await page.getByTestId("auth-fullname-input").fill("Người Dùng Auth Mới");
    await page.getByTestId("auth-email-input").fill(uniqueEmail);
    await page.getByTestId("auth-password-input").fill("InitialPass123!");
    await page.getByTestId("auth-submit-btn").click();
    await page.waitForURL("**/app", { timeout: 15000 });

    // Landing page recognizes active signed-in session
    await page.goto("/");
    await expect(page.getByTestId("landing-nav-cta")).toContainText("Vào Studxy", { timeout: 10000 });

    // Change password in /app/settings
    await page.goto("/app/settings");
    await expect(page.getByTestId("settings-auth-provider-badge")).toContainText("Email");
    await page.getByTestId("toggle-password-form-btn").click();
    await page.getByTestId("settings-current-password-input").fill("InitialPass123!");
    await page.getByTestId("settings-new-password-input").fill("UpdatedPass456!");
    const chgPromise = page.waitForResponse(
      (r) => r.url().includes("/api/v1/auth/change-password") && r.request().method() === "POST"
    );
    await page.getByTestId("settings-save-password-btn").click();
    expect((await chgPromise).status()).toBe(200);
    await expect(page.getByTestId("settings-password-feedback")).toBeVisible();

    // Log out cleanly & log back in with updated password
    await page.getByTestId("settings-logout-btn").click();
    await page.waitForURL("**/auth", { timeout: 15000 });
    await page.getByTestId("auth-tab-login").click();
    await page.getByTestId("auth-email-input").fill(uniqueEmail);
    await page.getByTestId("auth-password-input").fill("UpdatedPass456!");
    await page.getByTestId("auth-submit-btn").click();
    await page.waitForURL("**/app", { timeout: 15000 });
  });
});
