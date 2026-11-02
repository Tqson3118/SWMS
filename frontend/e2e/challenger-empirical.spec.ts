import { test, expect } from "@playwright/test";

test.describe("Empirical Challenger Verification Suite", () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
  });

  test("EMPIRICAL-FUNC-01: Command palette false success and input wipe on API 500 error", async ({
    page
  }) => {
    await page.goto("/workspace");

    let interceptedRequestCount = 0;
    await page.route("**/api/v1/flashcards", async (route) => {
      interceptedRequestCount++;
      await route.fulfill({
        status: 500,
        contentType: "application/json",
        body: JSON.stringify({
          error: "Internal Server Error 500: Database connection crashed"
        })
      });
    });

    // 1. Open Command Palette
    await page.getByTestId("cmd-k-trigger").click();
    const modal = page.getByTestId("command-palette-modal");
    await expect(modal).toBeVisible();

    // 2. Input Cloze card text
    const quickInput = page.getByTestId("quick-capture-input");
    const testContent = "Định nghĩa Translation Lookaside Buffer: {{c1::TLB}}";
    await quickInput.fill(testContent);
    expect(await quickInput.inputValue()).toBe(testContent);

    // 3. Click save button
    await page.getByTestId("quick-capture-save-btn").click();

    // 4. Verify request was intercepted
    expect(interceptedRequestCount).toBe(1);

    // 5. EMPIRICAL VERIFICATION:
    // Despite HTTP 500 error, green success toast is displayed!
    const successToast = page.getByText("Đã lưu thẻ Cloze nhanh vào Hàng đợi Ôn tập FSRS!");
    await expect(successToast).toBeVisible();

    // The input field is wiped clean, destroying user's uncommitted work
    expect(await quickInput.inputValue()).toBe("");

    // Zero error message shown inside the modal
    const errorCount = await modal.locator(".text-rose-500, .bg-rose-500, .text-red-500, .bg-red-500, [role='alert']").count();
    expect(errorCount).toBe(0);

    const modalText = await modal.innerText();
    expect(modalText).not.toContain("Internal Server Error");
    expect(modalText).not.toContain("Lỗi");
    expect(modalText).not.toContain("Database connection crashed");

    console.log("FUNC-01 VERIFIED: False success toast displayed, input wiped, and zero error message inside modal despite HTTP 500.");
    await page.unroute("**/api/v1/flashcards");
  });

  test("EMPIRICAL-FUNC-02: Document upload silent failure on 413 Quota Exceeded", async ({
    page
  }) => {
    await page.goto("/workspace");

    let uploadIntercepted = false;
    await page.route("**/api/v1/documents/upload", async (route) => {
      uploadIntercepted = true;
      await route.fulfill({
        status: 413,
        contentType: "application/json",
        body: JSON.stringify({
          error: "Storage quota exceeded (50MB free tier limit)"
        })
      });
    });

    // 1. Open upload modal
    await page.getByTestId("open-upload-pdf-btn").click();
    const uploadModal = page.getByTestId("upload-document-modal");
    await expect(uploadModal).toBeVisible();

    // 2. Fill in document title
    const titleInput = page.locator('input[type="text"][required]');
    await titleInput.fill("OS_Deep_Dive_Over_50MB.pdf");

    // 3. Click submit
    const submitBtn = page.locator('button[type="submit"]');
    await submitBtn.click();

    // 4. Verify request reached network and failed with 413
    await page.waitForTimeout(500);
    expect(uploadIntercepted).toBe(true);

    // 5. EMPIRICAL VERIFICATION:
    // Modal is still open, submit button is active again (not loading)
    await expect(uploadModal).toBeVisible();
    await expect(submitBtn).toHaveText("Nhập vào Workspace");
    await expect(submitBtn).toBeEnabled();

    // No error message banner or text is rendered in the modal
    const modalText = await uploadModal.innerText();
    expect(modalText).not.toContain("Storage quota exceeded");
    expect(modalText).not.toContain("Lỗi");
    expect(modalText).not.toContain("413");
    expect(modalText).not.toContain("thất bại");

    console.log("FUNC-02 VERIFIED: Upload modal silently fails on HTTP 413 with zero error feedback.");
    await page.unroute("**/api/v1/documents/upload");
  });

  test("EMPIRICAL-UX-01-A: Silent fallback to hardcoded TLB string on Flashcard create without selection", async ({
    page
  }) => {
    await page.goto("/workspace");

    // Switch to Reading mode (Mode 1)
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

    // Ensure selection is empty
    await page.evaluate(() => window.getSelection()?.removeAllRanges());

    // Flashcard draft popover trigger with empty selection
    await page.getByTestId("to-flashcard-popover-trigger").click();
    const draftPopover = page.getByTestId("draft-flashcard-popover");
    await expect(draftPopover).toBeVisible();

    // Verify injected fallback text in draftFront and draftBack inputs
    const frontVal = await page.getByTestId("draft-front-input").inputValue();
    const backVal = await page.getByTestId("draft-back-input").inputValue();

    expect(frontVal).toContain("Translation Lookaside Buffer");
    expect(frontVal).toContain("{{c1::");
    expect(backVal).toContain("Translation Lookaside Buffer (TLB)");
    expect(backVal).toContain("O(1) chu kỳ");

    console.log("UX-01-A VERIFIED: Flashcard draft popover silently injects hardcoded TLB string when selection is empty.");
  });

  test("EMPIRICAL-UX-01-B: Silent fallback to hardcoded TLB string on Highlight click without selection", async ({
    page
  }) => {
    await page.goto("/workspace");

    // Switch to Reading mode (Mode 1)
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

    // Ensure selection is empty
    await page.evaluate(() => window.getSelection()?.removeAllRanges());

    let annotationPayload: any = null;
    await page.route("**/api/v1/annotations", async (route) => {
      annotationPayload = JSON.parse(route.request().postData() || "{}");
      await route.fulfill({
        status: 201,
        contentType: "application/json",
        body: JSON.stringify({
          id: "test-ann-id",
          exactQuote: annotationPayload.exactQuote,
          colorKey: "yellow"
        })
      });
    });

    // Click highlight yellow without selecting text
    await page.getByTestId("highlight-btn-yellow").click();
    await page.waitForTimeout(600);

    expect(annotationPayload).not.toBeNull();
    expect(annotationPayload.exactQuote).toBe(
      "Translation Lookaside Buffer (TLB), một bộ nhớ cache liên kết tốc độ cao"
    );

    console.log("UX-01-B VERIFIED: Highlight button silently creates annotation with hardcoded TLB string when selection is empty.");
    await page.unroute("**/api/v1/annotations");
  });

  test("EMPIRICAL-UX-02: Quota popover lacks outside-click dismissal", async ({
    page
  }) => {
    await page.goto("/workspace");

    // 1. Open Quota popover
    const quotaBtn = page.getByTestId("quota-indicator-btn");
    await quotaBtn.click();
    const popover = page.getByTestId("quota-breakdown-popover");
    await expect(popover).toBeVisible();

    // 2. Click outside: click at center of workspace dashboard (x: 500, y: 400)
    await page.mouse.click(500, 400);
    await page.waitForTimeout(300);

    // EMPIRICAL VERIFICATION: Popover is STILL visible
    await expect(popover).toBeVisible();

    // 3. Click at another outside position (x: 200, y: 200)
    await page.mouse.click(200, 200);
    await page.waitForTimeout(300);

    // Popover is STILL visible
    await expect(popover).toBeVisible();

    // 4. Only clicking inside close button closes it
    const closeBtn = popover.locator('button[title="Đóng"]');
    await closeBtn.click();
    await expect(popover).not.toBeVisible();

    console.log("UX-02 VERIFIED: Quota popover cannot be dismissed by clicking outside.");
  });
});
