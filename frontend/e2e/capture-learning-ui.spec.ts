import { test, expect } from "@playwright/test";
import path from "path";
import fs from "fs";

test.use({
  video: "on",
  viewport: { width: 1440, height: 900 }
});

test("Capture comprehensive Learning UI across Reader, Split PKM, FSRS and Quiz", async ({ page }) => {
  const screenshotsDir = path.resolve(__dirname, "../../test-results/learning-ui-screenshots");
  if (!fs.existsSync(screenshotsDir)) {
    fs.mkdirSync(screenshotsDir, { recursive: true });
  }

  // 1. Go to workspace
  await page.goto("http://127.0.0.1:3000/workspace", { waitUntil: "networkidle" });
  await page.waitForTimeout(1000);
  await expect(page.locator('[data-testid="hero-continue-learning-card"]')).toBeVisible({ timeout: 15000 });

  // 2. Click "Học Lộ trình Bài giảng" on a course card
  const startLearningBtn = page.locator('[data-testid="course-start-learning-btn"]').first();
  if (await startLearningBtn.isVisible()) {
    await startLearningBtn.click();
  } else {
    await page.locator('[data-testid="mode-tab-reading"]').click();
  }
  await page.waitForTimeout(1500);

  // Capture UI 1: Reading Mode (Mode 1 - Deep Reader with Syllabus Roadmap Bar)
  await page.screenshot({ path: path.join(screenshotsDir, "01_ui_hoc_reading_mode.png"), fullPage: false });

  // 3. Switch to Mode 2: Split PKM Workspace (15/45/40)
  const modeTabSplit = page.locator('[data-testid="mode-tab-split"]');
  await expect(modeTabSplit).toBeVisible();
  await modeTabSplit.click();
  await page.waitForTimeout(1500);
  await expect(page.locator('[data-testid="mode-2-split-container"]')).toBeVisible();

  // Capture UI 2: Split PKM Workspace
  await page.screenshot({ path: path.join(screenshotsDir, "02_ui_hoc_split_pkm_workspace.png"), fullPage: false });

  // 4. Switch to Mode 3: Active Recall FSRS Review
  const modeTabReview = page.locator('[data-testid="mode-tab-review"]');
  await expect(modeTabReview).toBeVisible();
  await modeTabReview.click();
  await page.waitForTimeout(1500);
  await expect(page.locator('[data-testid="mode-3-review-container"]')).toBeVisible();

  // Front card
  await page.screenshot({ path: path.join(screenshotsDir, "03_ui_hoc_fsrs_card_front.png"), fullPage: false });

  // Flip card
  const flipCard = page.locator('[data-testid="fsrs-3d-flip-card"]');
  if (await flipCard.isVisible()) {
    await flipCard.click();
    await page.waitForTimeout(800);
    // Back card with FSRS rating buttons
    await page.screenshot({ path: path.join(screenshotsDir, "04_ui_hoc_fsrs_card_back_rating.png"), fullPage: false });
  }

  // 5. Switch to Quiz Subtab
  const quizSubTab = page.locator('[data-testid="review-subtab-quiz"]');
  if (await quizSubTab.isVisible()) {
    await quizSubTab.click();
    await page.waitForTimeout(1500);
    // Capture UI 5: Practice Quiz Portal
    await page.screenshot({ path: path.join(screenshotsDir, "05_ui_hoc_practice_quiz_portal.png"), fullPage: false });
  }

  await page.waitForTimeout(500);
});
