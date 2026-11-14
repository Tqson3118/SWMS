import { test, expect } from "./fixtures/test-base";
import path from "node:path";
import fs from "node:fs";

const SAMPLE_ZIP_PATH = path.resolve(__dirname, "../../test-fixtures/sample-swm-course.zip");

test.use({
  video: "on",
  viewport: { width: 1440, height: 900 }
});

test("Comprehensive Deep-Dive Tour: From Course Creation to Reader, Split PKM, FSRS and Quiz", async ({ page }) => {
  const screenshotsDir = path.resolve(__dirname, "../../test-results/deep-tour-screenshots");
  if (!fs.existsSync(screenshotsDir)) {
    fs.mkdirSync(screenshotsDir, { recursive: true });
  }

  // 1. Provision fresh user
  const email = `tour.${Date.now()}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "Sơn Trần (Học viên Chuyên sâu)"
    }
  });
  const regJson = await regRes.json();
  const userId = regJson.user.id;

  await page.addInitScript((uid: string) => {
    window.localStorage.setItem("swms_active_user_id", uid);
  }, userId);

  await page.route("**/api/v1/**", async (route: any) => {
    await route.continue({
      headers: {
        ...route.request().headers(),
        "x-test-user-id": userId
      }
    });
  });

  // 2. Open Workspace Dashboard
  await page.goto("/workspace");
  await page.waitForLoadState("domcontentloaded");
  await page.waitForTimeout(1000);

  // 3. Import SWM Zip Course
  const openBtn = page.locator('[data-testid="create-course-hub-btn"]').or(page.locator('[data-testid="open-course-create-btn"]')).first();
  await expect(openBtn).toBeVisible({ timeout: 10000 });
  await openBtn.click();
  await page.waitForTimeout(500);

  const zipTab = page.locator('[data-testid="tab-zip-course"]');
  await expect(zipTab).toBeVisible();
  await zipTab.click();
  await page.waitForTimeout(400);

  const zipInput = page.locator('[data-testid="swm-zip-file-input"]');
  await zipInput.setInputFiles(SAMPLE_ZIP_PATH);
  await page.waitForTimeout(500);

  const importBtn = page.locator('[data-testid="import-swm-zip-btn"]');
  await importBtn.click();

  // Wait for modal to complete and close
  await expect(page.locator('[data-testid="course-create-modal"]')).toBeHidden({ timeout: 20000 });
  await page.waitForTimeout(1000);

  // Switch to Dashboard to inspect the newly imported course card
  const dashboardTab = page.getByTestId("mode-tab-dashboard");
  await dashboardTab.click();
  await page.waitForTimeout(1200);

  // Screenshot 1: Dashboard with newly imported course card
  await page.screenshot({ path: path.join(screenshotsDir, "01_deep_dashboard_course_card.png"), fullPage: false });

  // 4. Click "Học Lộ trình Bài giảng"
  const startLearningBtn = page.locator('[data-testid="course-start-learning-btn"]').first();
  await expect(startLearningBtn).toBeVisible({ timeout: 10000 });
  await startLearningBtn.click();
  await page.waitForTimeout(1500);

  // Mode 1: Reader & Syllabus Roadmap
  const roadmapPanel = page.locator('[data-testid="course-roadmap-panel"]');
  await expect(roadmapPanel).toBeVisible({ timeout: 15000 });

  // Switch between lessons in the syllabus
  const lessonBtns = page.locator('[data-testid="course-syllabus-lesson-btn"]');
  const lessonCount = await lessonBtns.count();
  if (lessonCount > 1) {
    await lessonBtns.nth(1).click();
    await page.waitForTimeout(1000);
  }

  // In-Place editing of lesson title
  const editTitleBtn = page.locator('[data-testid="edit-lesson-title-btn"]').first();
  if (await editTitleBtn.isVisible()) {
    await editTitleBtn.click();
    await page.waitForTimeout(400);
    const titleInput = page.locator('[data-testid="lesson-title-input"]');
    await titleInput.fill("Bài giảng Chuyên sâu: Kiến trúc Phần mềm & Kiểm thử");
    await page.waitForTimeout(300);
    await page.locator('[data-testid="save-lesson-title-btn"]').click();
    await page.waitForTimeout(800);
  }

  // Screenshot 2: Deep Reader with Syllabus Bar & In-Place Editing
  await page.screenshot({ path: path.join(screenshotsDir, "02_deep_reader_syllabus_roadmap.png"), fullPage: false });

  // 5. Switch to Mode 2: Split PKM Workspace (15/45/40)
  const modeTabSplit = page.locator('[data-testid="mode-tab-split"]');
  await expect(modeTabSplit).toBeVisible();
  await modeTabSplit.click();
  await page.waitForTimeout(1500);
  await expect(page.locator('[data-testid="mode-2-split-container"]')).toBeVisible();

  // Type in Markdown Notebook
  const notebookEditor = page.locator('[data-testid="notebook-editor-textarea"]');
  if (await notebookEditor.isVisible()) {
    await notebookEditor.click();
    await notebookEditor.fill(
      "# 📓 Sổ tay Ghi chép Khóa học\n\n" +
      "## 1. Nguyên tắc Cốt lõi\n" +
      "- Kiến trúc Modular Monolith kết hợp .NET 10 Minimal API và Next.js 15.\n" +
      "- Kiểm soát đồng thời lạc quan OCC với chiến lược `409 CONFLICT_KEEP_BOTH`.\n\n" +
      "## 2. Liên kết Trí thức\n" +
      "- Khái niệm: [[Sovereign Learning Workspace]]\n" +
      "- Thuật toán nhớ ngắt quãng: [[FSRS v5 Algorithm]]\n\n" +
      "## 3. Thẻ Ghi nhớ Nhanh\n" +
      "- Mặt trước: FSRS v5 tối ưu hơn SM-2 ở điểm nào?\n" +
      "- Mặt sau: Đo lường độ trôi dạt ký ức theo 3 biến Stability, Difficulty và Retrievability."
    );
    await page.waitForTimeout(800);

    const saveNotebookBtn = page.locator('[data-testid="save-notebook-btn"]');
    if (await saveNotebookBtn.isVisible()) {
      await saveNotebookBtn.click();
      await page.waitForTimeout(1000);
    }
  }

  // Screenshot 3: Split PKM Workspace with active notes & 3 columns
  await page.screenshot({ path: path.join(screenshotsDir, "03_deep_split_pkm_active_notes.png"), fullPage: false });

  // 6. Switch to Mode 3: Active Recall FSRS Review
  const modeTabReview = page.locator('[data-testid="mode-tab-review"]');
  await expect(modeTabReview).toBeVisible();
  await modeTabReview.click();
  await page.waitForTimeout(1500);
  await expect(page.locator('[data-testid="mode-3-review-container"]')).toBeVisible();

  // Screenshot 4: Active Recall Card (Front Face)
  await page.screenshot({ path: path.join(screenshotsDir, "04_deep_fsrs_review_front.png"), fullPage: false });

  // Flip card
  const flipCard = page.locator('[data-testid="fsrs-3d-flip-card"]');
  if (await flipCard.isVisible()) {
    await flipCard.click();
    await page.waitForTimeout(1000);

    // Screenshot 5: Flipped Card (Back Face with FSRS 4 Rating Buttons)
    await page.screenshot({ path: path.join(screenshotsDir, "05_deep_fsrs_review_back_rating.png"), fullPage: false });

    // Click "Good [3]" rating button
    const goodBtn = page.locator('[data-testid="rate-btn-3"]');
    if (await goodBtn.isVisible()) {
      await goodBtn.click();
      await page.waitForTimeout(1200);
    }
  }

  // 7. Switch to Quiz Subtab
  const quizSubTab = page.locator('[data-testid="review-subtab-quiz"]');
  if (await quizSubTab.isVisible()) {
    await quizSubTab.click();
    await page.waitForTimeout(1500);
    await expect(page.locator('[data-testid="practice-quiz-portal"]')).toBeVisible();

    // Screenshot 6: Formative Practice Quiz Portal
    await page.screenshot({ path: path.join(screenshotsDir, "06_deep_quiz_portal_questions.png"), fullPage: false });
  }

  await page.waitForTimeout(1000);
});
