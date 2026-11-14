import { test, expect } from "@playwright/test";
import path from "path";
import fs from "fs";

test.use({
  video: "on",
  viewport: { width: 1440, height: 900 }
});

test("Capture full video and showcase screenshots for Course Creator Studio", async ({ page }) => {
  const screenshotsDir = path.resolve(__dirname, "../../test-results/showcase-screenshots");
  if (!fs.existsSync(screenshotsDir)) {
    fs.mkdirSync(screenshotsDir, { recursive: true });
  }

  // 1. Navigate to Workspace Dashboard
  await page.goto("http://127.0.0.1:3000/workspace", { waitUntil: "networkidle" });
  await page.waitForTimeout(1000);
  await expect(page.locator('[data-testid="hero-continue-learning-card"]')).toBeVisible({ timeout: 15000 });

  // Screenshot 1: Dashboard Overview & Create Course CTA
  await page.screenshot({ path: path.join(screenshotsDir, "01_dashboard_course_hub.png"), fullPage: false });

  // 2. Open Course Creator Hub Modal
  const createBtn = page.locator('[data-testid="create-course-hub-btn"]');
  if (await createBtn.isVisible()) {
    await createBtn.click();
    await page.waitForTimeout(500);

    const modal = page.locator('[data-testid="course-create-modal"]');
    await expect(modal).toBeVisible();

    // Screenshot 2: Tab 1 - Multi-Format Document Ingestion
    await page.screenshot({ path: path.join(screenshotsDir, "02_tab1_multiformat_documents.png"), fullPage: false });

    // Navigate to Tab 2 - Interactive AI Wizard
    await page.locator('[data-testid="tab-wizard-course"]').click();
    await page.waitForTimeout(500);
    await page.locator('[data-testid="wizard-topic-input"]').fill("Khóa học Lập trình Web C# .NET 10 từ cơ bản đến nâng cao");
    await page.waitForTimeout(400);

    // Screenshot 3: Tab 2 - Step 1: Topic Input
    await page.screenshot({ path: path.join(screenshotsDir, "03_tab2_wizard_step1_topic.png"), fullPage: false });

    // Step 2: Clarification questions
    await page.locator('[data-testid="wizard-step1-next-btn"]').click();
    await page.waitForTimeout(600);
    if (await page.locator('[data-testid="wizard-clarify-form"]').isVisible()) {
      // Screenshot 4: Tab 2 - Step 2: AI Clarification Interview
      await page.screenshot({ path: path.join(screenshotsDir, "04_tab2_wizard_step2_clarify.png"), fullPage: false });
      await page.locator('[data-testid="wizard-step2-next-btn"]').click();
      await page.waitForTimeout(800);
    }

    if (await page.locator('[data-testid="wizard-syllabus-preview"]').isVisible()) {
      // Screenshot 5: Tab 2 - Step 3: Editable Syllabus Preview
      await page.screenshot({ path: path.join(screenshotsDir, "05_tab2_wizard_step3_syllabus.png"), fullPage: false });
    }

    // Navigate to Tab 3 - SWM Zip Package Import/Export
    await page.locator('[data-testid="tab-zip-course"]').click();
    await page.waitForTimeout(500);

    // Screenshot 6: Tab 3 - SWM Template Zip Package
    await page.screenshot({ path: path.join(screenshotsDir, "06_tab3_swm_zip_package.png"), fullPage: false });

    // Close modal cleanly via close button
    const closeBtn = page.locator('[data-testid="course-create-modal-close-btn"]');
    if (await closeBtn.isVisible()) {
      await closeBtn.click();
    } else {
      await page.keyboard.press("Escape");
    }
    await expect(modal).toBeHidden({ timeout: 5000 });
    await page.waitForTimeout(500);
  }

  // 3. Navigate to Mode 4 - Course Creator Studio
  const studioTab = page.locator('[data-testid="mode-tab-studio"]');
  if (await studioTab.isVisible()) {
    await studioTab.click();
    await page.waitForTimeout(1000);

    // Screenshot 7: Mode 4 - Course Creator Studio with Curriculum Tree & Editor
    await page.screenshot({ path: path.join(screenshotsDir, "07_mode4_course_creator_studio.png"), fullPage: false });

    // Type video url to preview embedded video player
    const videoInput = page.locator('[data-testid="studio-video-url-input"]');
    if (await videoInput.isVisible()) {
      await videoInput.fill("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
      await page.waitForTimeout(600);
      // Screenshot 8: Mode 4 - Video Embed & Markdown Live Preview
      await page.screenshot({ path: path.join(screenshotsDir, "08_mode4_studio_video_markdown.png"), fullPage: false });
    }
  }

  // 4. Switch to Mode 1 - Reader & Learner View with Roadmap Progression
  const modeTab1 = page.locator('[data-testid="mode-tab-1"]');
  if (await modeTab1.isVisible()) {
    await modeTab1.click();
    await page.waitForTimeout(1000);

    // Screenshot 9: Dual-Layer Reader & Syllabus Roadmap with Progression
    await page.screenshot({ path: path.join(screenshotsDir, "09_reader_syllabus_progression.png"), fullPage: false });

    // Click mark lesson complete button if available
    const markCompleteBtn = page.locator('[data-testid="mark-lesson-complete-btn"]');
    if (await markCompleteBtn.isVisible()) {
      await markCompleteBtn.click();
      await page.waitForTimeout(800);
      // Screenshot 10: Progress updated & Checkmark visible
      await page.screenshot({ path: path.join(screenshotsDir, "10_lesson_completed_progress_update.png"), fullPage: false });
    }
  }

  // 5. Navigate to Mode 3 - Active Recall Review & Quiz
  const modeTab3 = page.locator('[data-testid="mode-tab-3"]');
  if (await modeTab3.isVisible()) {
    await modeTab3.click();
    await page.waitForTimeout(1000);

    // Screenshot 11: FSRS Active Recall & Practice Quiz
    await page.screenshot({ path: path.join(screenshotsDir, "11_fsrs_review_quiz_station.png"), fullPage: false });
  }

  // Finalize video recording
  const video = page.video();
  await page.waitForTimeout(500);
  await page.close();

  if (video) {
    const videoDir = path.resolve(__dirname, "../../test-results/showcase-video");
    if (!fs.existsSync(videoDir)) {
      fs.mkdirSync(videoDir, { recursive: true });
    }
    const targetVideoPath = path.join(videoDir, "studxy-course-platform-tour.webm");
    await video.saveAs(targetVideoPath);
  }
});
