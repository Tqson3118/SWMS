import { test, expect } from "./fixtures/test-base";
import path from "node:path";
import fs from "node:fs";

const PF_ZIP_PATH = path.resolve(__dirname, "./fixtures/personal-finance-course.zip");

test.use({
  video: "on",
  viewport: { width: 1440, height: 900 }
});

test("Upload Real Course from document/PersonalFinance and Full End-to-End Learning Experience", async ({ page }) => {
  test.setTimeout(90000);
  const screenshotsDir = path.resolve(__dirname, "../../test-results/pf-tour-screenshots");
  if (!fs.existsSync(screenshotsDir)) {
    fs.mkdirSync(screenshotsDir, { recursive: true });
  }

  // 1. Provision fresh test user
  const email = `finance.student.${Date.now()}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "Tuấn Nguyễn (Học viên Tài chính Cá nhân)"
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

  // 2. Navigate to Workspace
  await page.goto("/workspace");
  await page.waitForLoadState("domcontentloaded");
  await page.waitForTimeout(1200);

  // 3. Open Course Creation Studio & Import PersonalFinance Zip
  const openHubBtn = page.locator('[data-testid="create-course-hub-btn"]').or(page.locator('[data-testid="open-course-create-btn"]')).first();
  await expect(openHubBtn).toBeVisible({ timeout: 10000 });
  await openHubBtn.click();
  await page.waitForTimeout(500);

  // Switch to Tab 3 (Zip SWM Package)
  const zipTab = page.locator('[data-testid="tab-zip-course"]');
  await expect(zipTab).toBeVisible();
  await zipTab.click();
  await page.waitForTimeout(500);

  // Upload personal-finance-course.zip
  const zipInput = page.locator('[data-testid="swm-zip-file-input"]');
  await zipInput.setInputFiles(PF_ZIP_PATH);
  await page.waitForTimeout(800);

  // Click import
  const importBtn = page.locator('[data-testid="import-swm-zip-btn"]');
  await importBtn.click();

  // Wait for modal to process zip, parse manifest, markdown, flashcards, quizzes and close
  await expect(page.locator('[data-testid="course-create-modal"]')).toBeHidden({ timeout: 25000 });
  await page.waitForTimeout(1500);

  // 4. Switch to Dashboard to inspect the newly imported Course Card
  const dashboardTab = page.getByTestId("mode-tab-dashboard");
  await dashboardTab.click();
  await page.waitForTimeout(1500);

  // Verify course card appears
  const courseCard = page.locator('[data-testid^="course-card-"]').first();
  await expect(courseCard).toBeVisible({ timeout: 10000 });
  await expect(page.locator('[data-testid="course-card-title"]').first()).toContainText("Tài Chính Cá Nhân Thực Hành");

  // Screenshot 1: Course Dashboard showing imported Personal Finance course
  await page.screenshot({ path: path.join(screenshotsDir, "01_pf_dashboard_course_card.png"), fullPage: false });

  // 5. Start Learning Course
  const startLearningBtn = page.locator('[data-testid="course-start-learning-btn"]').first();
  await expect(startLearningBtn).toBeVisible({ timeout: 10000 });
  await startLearningBtn.click();
  await page.waitForTimeout(1500);

  // 6. Deep Reader & Syllabus Roadmap
  const roadmapPanel = page.locator('[data-testid="course-roadmap-panel"]');
  await expect(roadmapPanel).toBeVisible({ timeout: 15000 });

  // Check lesson list
  const lessonBtns = page.locator('[data-testid="course-syllabus-lesson-btn"]');
  const count = await lessonBtns.count();
  expect(count).toBeGreaterThan(0);

  // Click first lesson (Week 01 - Money Psychology)
  await lessonBtns.first().click();
  await page.waitForTimeout(1200);

  // Verify reader text is real markdown from week-01-money-psychology.md
  await expect(page.locator("body")).toContainText("Tâm Lý Tài Chính");

  // Screenshot 2: Deep Reader Module 1 Week 01
  await page.screenshot({ path: path.join(screenshotsDir, "02_pf_reader_week01_psychology.png"), fullPage: false });

  // Scroll down reader content
  await page.evaluate(() => window.scrollBy(0, 500));
  await page.waitForTimeout(1000);

  // Navigate to second lesson if available
  if (count > 1) {
    await lessonBtns.nth(1).click();
    await page.waitForTimeout(1200);
    await page.screenshot({ path: path.join(screenshotsDir, "03_pf_reader_week02_cashflow.png"), fullPage: false });
  }

  // 7. Mode 2: Split PKM Workspace
  const modeTabSplit = page.locator('[data-testid="mode-tab-split"]');
  await expect(modeTabSplit).toBeVisible();
  await modeTabSplit.click();
  await page.waitForTimeout(1500);
  await expect(page.locator('[data-testid="mode-2-split-container"]')).toBeVisible();

  // Write active reflections in Split Notebook
  const notebookEditor = page.locator('[data-testid="notebook-editor-textarea"]');
  if (await notebookEditor.isVisible()) {
    await notebookEditor.click();
    await notebookEditor.fill(
      "# 💰 Sổ tay Thực hành Tài chính Cá nhân\n\n" +
      "## 1. Bài học Tuần 01: Tâm lý tiền bạc & Thiên kiến nhận thức\n" +
      "- **Homo Economicus** là giả định phi thực tế; con người luôn có tính duy lý giới hạn (Bounded Rationality).\n" +
      "- **Ảo tưởng kiểm soát (Illusion of Control):** Tự chọn cổ phiếu hay liên tục nhìn bảng điện không làm giảm tính ngẫu nhiên.\n" +
      "- **Ngụy biện chi phí chìm (Sunk Cost):** Quá khứ không lấy lại được; quyết định tài chính phải dựa vào chi phí cơ hội tương lai.\n\n" +
      "## 2. Kế hoạch Hành động Cụ thể\n" +
      "1. Thiết lập quy tắc 48h làm nguội trước mọi quyết định mua sắm ngoài danh mục thiết yếu.\n" +
      "2. Xây dựng Quỹ dự phòng khẩn cấp tương đương 3-6 tháng chi tiêu cơ bản trước khi đầu tư mạo hiểm.\n" +
      "3. Tận dụng sức mạnh lãi kép (Compound Interest) với tầm nhìn dài hạn 10-15 năm."
    );
    await page.waitForTimeout(800);

    const saveNotebookBtn = page.locator('[data-testid="save-notebook-btn"]');
    if (await saveNotebookBtn.isVisible()) {
      await saveNotebookBtn.click();
      await page.waitForTimeout(1000);
    }
  }

  // Screenshot 4: Split PKM Workspace
  await page.screenshot({ path: path.join(screenshotsDir, "04_pf_split_pkm_workspace.png"), fullPage: false });

  // 8. Mode 3: Active Recall FSRS Review
  const modeTabReview = page.locator('[data-testid="mode-tab-review"]');
  await expect(modeTabReview).toBeVisible();
  await modeTabReview.click();
  await page.waitForTimeout(1500);
  await expect(page.locator('[data-testid="mode-3-review-container"]')).toBeVisible();

  // Screenshot 5: FSRS Flashcard Front Face
  await page.screenshot({ path: path.join(screenshotsDir, "05_pf_fsrs_card_front.png"), fullPage: false });

  // Flip Flashcard
  const flipCard = page.locator('[data-testid="fsrs-3d-flip-card"]');
  if (await flipCard.isVisible()) {
    await flipCard.click();
    await page.waitForTimeout(1000);

    // Screenshot 6: FSRS Flashcard Back Face with Rating buttons
    await page.screenshot({ path: path.join(screenshotsDir, "06_pf_fsrs_card_back.png"), fullPage: false });

    // Rate Good [3]
    const goodBtn = page.locator('[data-testid="rate-btn-3"]');
    if (await goodBtn.isVisible()) {
      await goodBtn.click();
      await page.waitForTimeout(1200);
    }
  }

  // 9. Practice Quiz Portal
  const quizSubTab = page.locator('[data-testid="review-subtab-quiz"]');
  if (await quizSubTab.isVisible()) {
    await quizSubTab.click();
    await page.waitForTimeout(1500);
    await expect(page.locator('[data-testid="practice-quiz-portal"]')).toBeVisible();

    // Screenshot 7: Practice Quiz Portal with questions from week-01-quiz.md
    await page.screenshot({ path: path.join(screenshotsDir, "07_pf_quiz_portal_questions.png"), fullPage: false });
  }

  await page.waitForTimeout(2000);
});
