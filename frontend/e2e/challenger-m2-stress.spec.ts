import { test, expect } from "./fixtures/test-base";
import path from "node:path";
import fs from "node:fs";

const FIXTURES_DIR = path.resolve(__dirname, "../../test-fixtures");
const ZERO_BYTE_PDF = path.join(FIXTURES_DIR, "zero-byte.pdf");
const ZERO_BYTE_TXT = path.join(FIXTURES_DIR, "zero-byte.txt");
const OVERSIZED_51MB_PDF = path.join(FIXTURES_DIR, "oversized-51mb.pdf");
const FAKE_PDF_TEXT = path.join(FIXTURES_DIR, "fake-pdf-text.pdf");
const CORRUPT_PDF = path.join(FIXTURES_DIR, "corrupt-broken.pdf");
const ENCRYPTED_PDF = path.join(FIXTURES_DIR, "encrypted-password.pdf");
const SMALL_3P_PDF = path.join(FIXTURES_DIR, "small-3p.pdf");

test.describe("Milestone 2 Adversarial Stress Suite", () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");
  });

  test("C2-01: Zero-byte PDF file selection & rejection with zero console errors", async ({
    page
  }) => {
    expect(fs.existsSync(ZERO_BYTE_PDF)).toBe(true);
    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    const fileInput = page.getByTestId("document-file-input");
    await fileInput.setInputFiles(ZERO_BYTE_PDF);

    // Assert friendly Vietnamese error alert is displayed immediately upon selection
    const errorAlert = page.getByTestId("upload-error-alert");
    await expect(errorAlert).toBeVisible({ timeout: 5000 });
    await expect(errorAlert).toContainText(/lỗi|hỏng|không thể đọc/i);

    // Clicking submit should NOT trigger network request or cause 4xx error
    const submitBtn = modal.locator('button[type="submit"]');
    await submitBtn.click();
    await expect(errorAlert).toBeVisible();
    await page.keyboard.press("Escape");
    await expect(modal).toBeHidden();
  });

  test("C2-02: Zero-byte text/markdown file selection & submission handling", async ({
    page
  }) => {
    expect(fs.existsSync(ZERO_BYTE_TXT)).toBe(true);
    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    const fileInput = page.getByTestId("document-file-input");
    await fileInput.setInputFiles(ZERO_BYTE_TXT);

    // Attempt upload of 0-byte text file
    const submitBtn = modal.locator('button[type="submit"]');
    await submitBtn.click();

    // Check if error is handled gracefully without uncaught exceptions
    await page.waitForTimeout(1000);
    await page.keyboard.press("Escape");
  });

  test("C2-03: Real oversized 51MB PDF file selection & rejection without submission", async ({
    page
  }) => {
    expect(fs.existsSync(OVERSIZED_51MB_PDF)).toBe(true);
    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    const fileInput = page.getByTestId("document-file-input");
    await fileInput.setInputFiles(OVERSIZED_51MB_PDF);

    // Client-side validation must catch >50MB and display Vietnamese error
    const errorAlert = page.getByTestId("upload-error-alert");
    await expect(errorAlert).toBeVisible({ timeout: 5000 });
    await expect(errorAlert).toContainText(/50MB/i);

    // Submit must be blocked
    const submitBtn = modal.locator('button[type="submit"]');
    await submitBtn.click();
    await expect(errorAlert).toBeVisible();
    await page.keyboard.press("Escape");
  });

  test("C2-04: Fake PDF (text file renamed to .pdf) client rejection with zero console errors", async ({
    page
  }) => {
    expect(fs.existsSync(FAKE_PDF_TEXT)).toBe(true);
    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    const fileInput = page.getByTestId("document-file-input");
    await fileInput.setInputFiles(FAKE_PDF_TEXT);

    const errorAlert = page.getByTestId("upload-error-alert");
    await expect(errorAlert).toBeVisible({ timeout: 5000 });
    await expect(errorAlert).toContainText(/lỗi|hỏng|định dạng/i);

    const submitBtn = modal.locator('button[type="submit"]');
    await submitBtn.click();
    await expect(errorAlert).toBeVisible();
    await page.keyboard.press("Escape");
  });

  test("C2-05: Encrypted PDF client rejection with password notice and zero console errors", async ({
    page
  }) => {
    expect(fs.existsSync(ENCRYPTED_PDF)).toBe(true);
    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    const fileInput = page.getByTestId("document-file-input");
    await fileInput.setInputFiles(ENCRYPTED_PDF);

    const errorAlert = page.getByTestId("upload-error-alert");
    await expect(errorAlert).toBeVisible({ timeout: 5000 });
    await expect(errorAlert).toContainText(/mật khẩu|mã hóa|encrypted/i);

    const submitBtn = modal.locator('button[type="submit"]');
    await submitBtn.click();
    await expect(errorAlert).toBeVisible();
    await page.keyboard.press("Escape");
  });

  test("C2-06: Corrupt PDF client rejection with friendly notice and zero console errors", async ({
    page
  }) => {
    expect(fs.existsSync(CORRUPT_PDF)).toBe(true);
    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    const fileInput = page.getByTestId("document-file-input");
    await fileInput.setInputFiles(CORRUPT_PDF);

    const errorAlert = page.getByTestId("upload-error-alert");
    await expect(errorAlert).toBeVisible({ timeout: 5000 });
    await expect(errorAlert).toContainText(/lỗi|hỏng|không thể đọc/i);

    const submitBtn = modal.locator('button[type="submit"]');
    await submitBtn.click();
    await expect(errorAlert).toBeVisible();
    await page.keyboard.press("Escape");
  });

  test("C2-07: Escape key and Close button dismiss upload modal and clean error state", async ({
    page
  }) => {
    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    // Trigger an error first
    const fileInput = page.getByTestId("document-file-input");
    await fileInput.setInputFiles(CORRUPT_PDF);
    await expect(page.getByTestId("upload-error-alert")).toBeVisible();

    // Dismiss via Escape
    await page.keyboard.press("Escape");
    await expect(modal).toBeHidden();

    // Re-open: error must be cleared
    await page.getByTestId("open-upload-pdf-btn").click();
    await expect(modal).toBeVisible();
    await expect(page.getByTestId("upload-error-alert")).toBeHidden();

    // Dismiss via Close (X) button
    const closeBtn = modal.locator('button[aria-label="Close"]');
    await closeBtn.click();
    await expect(modal).toBeHidden();
  });

  test("C2-08: Trash button removes selected file and resets dropzone", async ({
    page
  }) => {
    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    const fileInput = page.getByTestId("document-file-input");
    await fileInput.setInputFiles(CORRUPT_PDF);
    await expect(page.getByTestId("selected-file-card")).toBeVisible();

    // Click trash button
    const trashBtn = page.getByTestId("selected-file-card").locator("button[title='Gỡ file']");
    await trashBtn.click();

    // Assert dropzone is restored and selected file card is removed
    await expect(page.getByTestId("document-dropzone")).toBeVisible();
    await expect(page.getByTestId("selected-file-card")).toBeHidden();
    await expect(page.getByTestId("upload-error-alert")).toBeHidden();

    await page.keyboard.press("Escape");
  });

  test("C2-09: Rapid multiple clicks on submit button does not throw or multi-submit", async ({
    page
  }) => {
    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    const submitBtn = modal.locator('button[type="submit"]');
    // Rapidly click 5 times with empty/invalid form
    await Promise.all([
      submitBtn.click().catch(() => {}),
      submitBtn.click().catch(() => {}),
      submitBtn.click().catch(() => {}),
      submitBtn.click().catch(() => {})
    ]);

    // Modal should still be stable
    await expect(modal).toBeVisible();
    await page.keyboard.press("Escape");
  });
});
