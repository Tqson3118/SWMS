import { test, expect } from "@playwright/test";
import path from "node:path";
import fs from "node:fs";

const SCREENSHOTS_DIR = path.resolve(__dirname, "../../test-results/screenshots/pdf");

test("Capture Visual Verification Evidence for Multi-Page PDF Experience", async ({ page }) => {
  test.setTimeout(180000);
  if (!fs.existsSync(SCREENSHOTS_DIR)) {
    fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });
  }

  await page.setViewportSize({ width: 1920, height: 1080 });
  await page.goto("/workspace");
  await expect(page).toHaveURL(/.*workspace/);
  await page.waitForTimeout(1000);

  // 1. Dashboard before upload
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, "01_dashboard_before_upload.png") });

  // 2. Open upload modal & select 298-page file
  const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
  await expect(openUploadBtn).toBeVisible({ timeout: 15000 });
  await openUploadBtn.click();

  const uploadModal = page.getByTestId("upload-document-modal");
  await expect(uploadModal).toBeVisible();

  const userPdfPath = path.resolve(__dirname, "../../test/Giáo trình Pháp luật đại cương_ebook.pdf");
  const fileInput = page.getByTestId("document-file-input");
  await fileInput.setInputFiles(userPdfPath);
  await page.waitForTimeout(500);

  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, "02_upload_modal_file_selected.png") });

  // 3. Submit upload and enter reader
  const submitBtn = uploadModal.locator('button[type="submit"]');
  await submitBtn.click();

  const pageIndicator = page.getByTestId("pdf-page-indicator");
  await expect(pageIndicator).toBeVisible({ timeout: 25000 });
  await expect(pageIndicator).toHaveText(/Trang 1 \/ 298/, { timeout: 15000 });

  // Wait for canvas to render pixels
  const canvas = page.locator("canvas.pdf-canvas").first();
  await expect(canvas).toBeVisible({ timeout: 15000 });
  await page.waitForTimeout(2000);

  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, "03_reader_page_1_of_298.png") });

  // 4. Jump to Page 150
  const pageInput = page.getByTestId("pdf-jump-page-input");
  await pageInput.fill("150");
  await pageInput.press("Enter");
  await expect(pageIndicator).toHaveText(/Trang 150 \/ 298/, { timeout: 10000 });
  await page.waitForTimeout(1500);

  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, "04_reader_page_150_of_298.png") });

  // 5. Jump to Page 298 (Last Page)
  await pageInput.fill("298");
  await pageInput.press("Enter");
  await expect(pageIndicator).toHaveText(/Trang 298 \/ 298/, { timeout: 10000 });
  await page.waitForTimeout(1500);

  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, "05_reader_page_298_of_298.png") });

  // 6. Jump to Page 200 & create yellow highlight
  await pageInput.fill("200");
  await pageInput.press("Enter");
  await expect(pageIndicator).toHaveText(/Trang 200 \/ 298/, { timeout: 10000 });
  await page.waitForTimeout(1500);

  const textContainer = page.getByTestId("pdf-extracted-text-container");
  await expect(textContainer).toBeVisible({ timeout: 10000 });

  await page.evaluate(() => {
    const container = document.querySelector('[data-testid="pdf-extracted-text-container"]');
    if (container && container.firstChild) {
      const range = document.createRange();
      range.setStart(container.firstChild, 10);
      range.setEnd(container.firstChild, 80);
      const sel = window.getSelection();
      sel?.removeAllRanges();
      sel?.addRange(range);
      container.dispatchEvent(new MouseEvent("mouseup", { bubbles: true }));
    }
  });

  const yellowBtn = page.getByTestId("highlight-btn-yellow");
  await expect(yellowBtn).toBeVisible({ timeout: 5000 });
  await yellowBtn.click();

  const page200Mark = page.locator("mark[data-page='200']");
  await expect(page200Mark.first()).toBeVisible({ timeout: 5000 });
  await page.waitForTimeout(500);

  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, "06_page_200_highlight_created.png") });

  // 7. Create flashcard from highlight
  await page200Mark.first().click();
  const toFlashcardBtn = page.getByTestId("to-flashcard-popover-trigger");
  await expect(toFlashcardBtn).toBeVisible();
  await toFlashcardBtn.click();

  const commitFlashcardBtn = page.getByTestId("commit-draft-flashcard-btn");
  await expect(commitFlashcardBtn).toBeVisible();
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, "07_flashcard_creation_popover.png") });

  await commitFlashcardBtn.click();
  await page.waitForTimeout(1000);

  // 8. Go to Review Mode and test Jump to Source
  const reviewModeBtn = page.getByRole("button", { name: "Ôn tập" }).first();
  if (await reviewModeBtn.isVisible().catch(() => false)) {
    await reviewModeBtn.click();
    await page.waitForTimeout(1000);
    await page.screenshot({ path: path.join(SCREENSHOTS_DIR, "08_review_mode_with_flashcard.png") });

    const jumpBtn = page.getByRole("button", { name: "Nhảy đến nguồn" }).first();
    if (await jumpBtn.isVisible().catch(() => false)) {
      await jumpBtn.click();
      await page.waitForTimeout(1500);
      await expect(pageIndicator).toHaveText(/Trang 200 \/ 298/);
      await page.screenshot({ path: path.join(SCREENSHOTS_DIR, "09_jumped_back_to_page_200.png") });
    }
  }
});
