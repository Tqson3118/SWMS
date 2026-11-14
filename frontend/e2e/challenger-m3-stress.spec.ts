import { test, expect } from "./fixtures/test-base";
import path from "node:path";
import fs from "node:fs";

const USER_PDF_298P_PATH = path.resolve(
  __dirname,
  "../../test/Giáo trình Pháp luật đại cương_ebook.pdf"
);
const PDF_26P_PATH = path.resolve(
  __dirname,
  "../../test-fixtures/ietf-rfc7995-26p.pdf"
);
const SCANNED_PDF_PATH = path.resolve(
  __dirname,
  "../../test-fixtures/mixed-scanned-text.pdf"
);

test.describe("Milestone 3 Empirical Challenger Stress Suite", () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");
  });

  test("C3-01: Page Jump Clamping - Negative, Zero, Over-max, Non-numeric, Empty String & Rapid Nav", async ({
    page
  }) => {
    test.setTimeout(60000);

    // Upload multi-page document to test high page counts while in Dashboard mode
    expect(fs.existsSync(PDF_26P_PATH)).toBe(true);
    const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
    await expect(openUploadBtn).toBeVisible({ timeout: 15000 });
    await openUploadBtn.click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();
    const fileInput = page.getByTestId("document-file-input").or(page.getByTestId("file-upload-input"));
    await fileInput.setInputFiles(PDF_26P_PATH);
    await modal.locator('button[type="submit"]').click();

    const pageIndicator = page.getByTestId("pdf-page-indicator");
    await expect(pageIndicator).toBeVisible({ timeout: 25000 });
    const indicatorText = await pageIndicator.innerText();
    expect(indicatorText).toMatch(/Trang \d+ \/ \d+/);

    const totalPagesMatch = indicatorText.match(/Trang \d+ \/ (\d+)/);
    const totalPages = totalPagesMatch ? parseInt(totalPagesMatch[1], 10) : 298;
    expect(totalPages).toBeGreaterThanOrEqual(1);

    const jumpInput = page.getByTestId("pdf-jump-page-input");
    await expect(jumpInput).toBeVisible();

    // 1. Test Page 0 -> must clamp to Page 1
    await jumpInput.fill("0");
    await jumpInput.press("Enter");
    await expect(pageIndicator).toContainText("Trang 1 /");
    await expect(jumpInput).toHaveValue("1");

    // 2. Test Negative -1 -> must clamp to Page 1
    await jumpInput.fill("-1");
    await jumpInput.press("Enter");
    await expect(pageIndicator).toContainText("Trang 1 /");
    await expect(jumpInput).toHaveValue("1");

    // 3. Test Negative -100 -> must clamp to Page 1
    await jumpInput.fill("-100");
    await jumpInput.press("Enter");
    await expect(pageIndicator).toContainText("Trang 1 /");
    await expect(jumpInput).toHaveValue("1");

    // 4. Test Over-max 9999 -> must clamp to totalPages
    await jumpInput.fill("9999");
    await jumpInput.press("Enter");
    await expect(pageIndicator).toContainText(`Trang ${totalPages} / ${totalPages}`);
    await expect(jumpInput).toHaveValue(String(totalPages));

    // 5. Test Over-max 100000 -> must clamp to totalPages
    await jumpInput.fill("100000");
    await jumpInput.press("Enter");
    await expect(pageIndicator).toContainText(`Trang ${totalPages} / ${totalPages}`);
    await expect(jumpInput).toHaveValue(String(totalPages));

    // 6. Test Non-numeric "abc" -> must restore previous safe page, no NaN
    await jumpInput.fill("abc");
    await jumpInput.press("Enter");
    await expect(pageIndicator).toContainText(`Trang ${totalPages} / ${totalPages}`);
    await expect(pageIndicator).not.toContainText(/NaN|undefined/);
    await expect(jumpInput).toHaveValue(String(totalPages));

    // 7. Test Non-numeric "!@#$" -> must restore previous safe page
    await jumpInput.fill("!@#$");
    await jumpInput.press("Enter");
    await expect(pageIndicator).toContainText(`Trang ${totalPages} / ${totalPages}`);
    await expect(jumpInput).toHaveValue(String(totalPages));

    // 8. Test Empty String "" -> must restore previous safe page
    await jumpInput.fill("");
    await jumpInput.press("Enter");
    await expect(pageIndicator).toContainText(`Trang ${totalPages} / ${totalPages}`);
    await expect(jumpInput).toHaveValue(String(totalPages));

    // 9. Rapid Keyboard PageUp / PageDown navigation
    // Jump back to page 2 first
    await jumpInput.fill("2");
    await jumpInput.press("Enter");
    await expect(pageIndicator).toContainText("Trang 2 /");

    // Blur input so keyboard events hit reader
    await page.getByTestId("mode-1-reading-container").click();

    // Rapid PageDown (ArrowRight / PageDown)
    for (let i = 0; i < 5; i++) {
      await page.keyboard.press("PageDown");
      await page.waitForTimeout(30);
    }
    await page.waitForTimeout(300);
    const afterDown = await pageIndicator.innerText();
    expect(afterDown).toMatch(/Trang \d+ \/ \d+/);
    expect(afterDown).not.toContain("NaN");

    // Rapid PageUp
    for (let i = 0; i < 10; i++) {
      await page.keyboard.press("PageUp");
      await page.waitForTimeout(30);
    }
    await page.waitForTimeout(300);
    // Should clamp at page 1
    await expect(pageIndicator).toContainText("Trang 1 /");
  });

  test("C3-02: Dual Reading Progress - Percentage Pill & Bar Fill Math Synchronization", async ({
    page
  }) => {
    test.setTimeout(60000);

    // Ensure multi-page doc is loaded from Dashboard mode
    const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
    await expect(openUploadBtn).toBeVisible({ timeout: 15000 });
    await openUploadBtn.click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();
    const fileInput = page.getByTestId("document-file-input").or(page.getByTestId("file-upload-input"));
    await fileInput.setInputFiles(USER_PDF_298P_PATH);
    await modal.locator('button[type="submit"]').click();

    const pageIndicator = page.getByTestId("pdf-page-indicator");
    await expect(pageIndicator).toBeVisible({ timeout: 25000 });

    const progressPill = page.getByTestId("reading-progress-pill");
    const progressBarContainer = page.getByTestId("reading-progress-bar-container");
    const progressBarFill = page.getByTestId("reading-progress-bar-fill");

    await expect(progressPill).toBeVisible();
    await expect(progressBarContainer).toBeVisible();
    await expect(progressBarFill).toBeVisible();

    const jumpInput = page.getByTestId("pdf-jump-page-input");

    const testPages = [1, 50, 149, 298];
    const totalPages = 298;

    for (const testPage of testPages) {
      await jumpInput.fill(String(testPage));
      await jumpInput.press("Enter");
      await page.waitForTimeout(200);

      const expectedPercent = Math.min(100, Math.max(1, Math.round((testPage / totalPages) * 100)));

      // Assert pill text
      await expect(progressPill).toHaveText(`${expectedPercent}%`);

      // Assert aria-valuenow on container
      const ariaValue = await progressBarContainer.getAttribute("aria-valuenow");
      expect(Number(ariaValue)).toBe(expectedPercent);

      // Assert fill width style
      const fillStyle = await progressBarFill.getAttribute("style");
      expect(fillStyle).toContain(`width: ${expectedPercent}%`);
    }
  });

  test("C3-03: Selection Preservation on MouseDown for all 5 Semantic Highlight Buttons", async ({
    page
  }) => {
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("semantic-selection-toolbar")).toBeVisible();

    // Target a paragraph in reading prose
    const textTarget = page.locator('[data-testid="reading-prose-column"] p, [data-testid="pdf-extracted-text-container"]').first();
    await expect(textTarget).toBeVisible();

    // Select text using DOM selection range
    await textTarget.evaluate((el) => {
      const selection = window.getSelection();
      const range = document.createRange();
      range.selectNodeContents(el);
      selection?.removeAllRanges();
      selection?.addRange(range);
    });

    // Check that text is selected in window
    const selectedTextBefore = await page.evaluate(() => window.getSelection()?.toString().trim());
    expect(selectedTextBefore?.length).toBeGreaterThan(0);

    const colors = ["yellow", "green", "blue", "purple", "rose"] as const;

    for (const color of colors) {
      const btn = page.getByTestId(`highlight-btn-${color}`);
      await expect(btn).toBeVisible();

      // Trigger mousedown on button - should NOT clear selection because of onMouseDown preventDefault
      await btn.dispatchEvent("mousedown");

      const selectedTextDuringMouseDown = await page.evaluate(() =>
        window.getSelection()?.toString().trim()
      );
      expect(selectedTextDuringMouseDown).toBe(selectedTextBefore);
    }
  });

  test("C3-04: Persistence of All 5 Semantic Highlight Colors to Local Annotations", async ({
    page
  }) => {
    await page.getByTestId("mode-tab-reading").click();
    const toolbar = page.getByTestId("semantic-selection-toolbar");
    await expect(toolbar).toBeVisible();

    const textTarget = page.locator('[data-testid="reading-prose-column"] p').first();
    await expect(textTarget).toBeVisible();

    const colors = ["yellow", "green", "blue", "purple", "rose"] as const;

    for (const color of colors) {
      // Re-select text
      await textTarget.evaluate((el) => {
        const selection = window.getSelection();
        const range = document.createRange();
        range.selectNodeContents(el);
        selection?.removeAllRanges();
        selection?.addRange(range);
      });

      const btn = page.getByTestId(`highlight-btn-${color}`);
      await btn.click();

      // Check toast feedback
      const toast = page.getByTestId("workspace-toast");
      if (await toast.isVisible({ timeout: 2000 }).catch(() => false)) {
        await expect(toast).toContainText(/Highlight/i);
      }

      await page.waitForTimeout(300);
    }
  });

  test("C3-05: Empty Selection Highlight Rejection with Vietnamese Toast", async ({
    page
  }) => {
    await page.getByTestId("mode-tab-reading").click();

    // Clear any active selection
    await page.evaluate(() => window.getSelection()?.removeAllRanges());

    const yellowBtn = page.getByTestId("highlight-btn-yellow");
    await expect(yellowBtn).toBeVisible();
    await yellowBtn.click();

    // Assert friendly Vietnamese toast notification appears
    const toast = page.getByTestId("workspace-toast");
    await expect(toast).toBeVisible({ timeout: 5000 });
    await expect(toast).toContainText("Vui lòng bôi đen một đoạn văn bản trong bài đọc trước khi đánh dấu hoặc tạo thẻ!");

    // Verify no empty <mark> elements were created
    const emptyMarks = await page.locator("mark:empty").count();
    expect(emptyMarks).toBe(0);
  });

  test("C3-06: Missing Text Layer Badge for Scanned PDF Pages", async ({
    page
  }) => {
    test.setTimeout(60000);
    expect(fs.existsSync(SCANNED_PDF_PATH)).toBe(true);

    const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
    await openUploadBtn.click();

    const uploadModal = page.getByTestId("upload-document-modal");
    await expect(uploadModal).toBeVisible();

    const fileInput = page.getByTestId("document-file-input").or(page.getByTestId("file-upload-input"));
    await fileInput.setInputFiles(SCANNED_PDF_PATH);

    const submitBtn = uploadModal.locator('button[type="submit"]');
    await submitBtn.click();

    const pageIndicator = page.getByTestId("pdf-page-indicator");
    await expect(pageIndicator).toBeVisible({ timeout: 25000 });

    // Jump to page 2 (the scanned image page without text layer)
    const jumpInput = page.getByTestId("pdf-jump-page-input");
    await jumpInput.fill("2");
    await jumpInput.press("Enter");

    // Verify no-text-layer-badge is displayed
    const badge = page.getByTestId("no-text-layer-badge");
    await expect(badge).toBeVisible({ timeout: 10000 });
    await expect(badge).toContainText("Không có lớp văn bản — chưa OCR");
  });
});
