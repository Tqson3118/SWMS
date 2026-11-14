import { test, expect } from "./fixtures/test-base";
import { assertZeroHorizontalOverflow } from "./fixtures/ergonomics-helpers";
import path from "path";
import fs from "fs";

test.describe("Challenger M2 R2: Responsive Reflow & Boundary Stress Suite", () => {
  const VIEWPORTS = [
    { name: "1920x1080 Full HD Desktop", width: 1920, height: 1080 },
    { name: "1536x864 Standard Laptop", width: 1536, height: 864 },
    { name: "1440x900 Desktop / MacBook", width: 1440, height: 900 },
    { name: "1280x720 Compact Desktop", width: 1280, height: 720 },
    { name: "1024x768 Tablet Landscape", width: 1024, height: 768 },
    { name: "768x1024 Tablet Portrait", width: 768, height: 1024 },
    { name: "640x960 Small Tablet / Mobile Boundary", width: 640, height: 960 }
  ];

  for (const vp of VIEWPORTS) {
    test(`C2-VIEWPORT-${vp.width}: Bento Grid reflow and zero horizontal overflow at ${vp.name}`, async ({
      page
    }) => {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.goto("/workspace");
      await page.getByTestId("mode-tab-dashboard").click();

      // Ensure Bento workspace is loaded
      const dashboard = page.getByTestId("personal-workspace-dashboard");
      await expect(dashboard).toBeVisible();

      // Assert zero horizontal overflow
      await assertZeroHorizontalOverflow(page);

      // Verify all 4 Bento Grid blocks are rendered
      const heroCard = page.getByText("Tài liệu đang học").first();
      await expect(heroCard).toBeVisible();

      const reviewStation = page.getByTestId("fsrs-review-station-card");
      await expect(reviewStation).toBeVisible();

      const statsHeading = page.getByText("Thống kê học tập");
      await expect(statsHeading).toBeVisible();

      const libHeading = page.getByText("Thư viện tài liệu & học liệu");
      await expect(libHeading).toBeVisible();

      // Verify bounding boxes fit within viewport width
      const dashBox = await dashboard.boundingBox();
      expect(dashBox).not.toBeNull();
      if (dashBox) {
        expect(dashBox.width).toBeLessThanOrEqual(vp.width);
      }
    });
  }

  test("C2-INTERACT-01: Rapid modal open and close cycles via Escape and Cancel button", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");
    await page.getByTestId("mode-tab-dashboard").click();

    const openBtn = page.getByTestId("open-upload-pdf-btn");
    const modal = page.getByTestId("upload-document-modal");

    // Cycle 1: Click open, press Escape immediately
    for (let i = 0; i < 5; i++) {
      await openBtn.click();
      await expect(modal).toBeVisible();
      await page.keyboard.press("Escape");
      await expect(modal).toBeHidden();
    }

    // Cycle 2: Click open, click Cancel button
    for (let i = 0; i < 3; i++) {
      await openBtn.click();
      await expect(modal).toBeVisible();
      const cancelBtn = modal.locator('button:has-text("Hủy")');
      await cancelBtn.click();
      await expect(modal).toBeHidden();
    }

    // Cycle 3: Click open, click Close icon button (X)
    for (let i = 0; i < 3; i++) {
      await openBtn.click();
      await expect(modal).toBeVisible();
      const closeIconBtn = modal.locator('button[aria-label="Close"]');
      await closeIconBtn.click();
      await expect(modal).toBeHidden();
    }
  });

  test("C2-INTERACT-02: Rapid Mode Switching stress test between Dashboard, Reading, Split, and Review", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");

    const tabDashboard = page.getByTestId("mode-tab-dashboard");
    const tabReading = page.getByTestId("mode-tab-reading");
    const tabSplit = page.getByTestId("mode-tab-split");
    const tabReview = page.getByTestId("mode-tab-review");

    // Rapid round-robin switching 4 cycles
    for (let cycle = 0; cycle < 4; cycle++) {
      await tabReading.click();
      await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

      await tabSplit.click();
      await expect(page.getByTestId("mode-2-split-container")).toBeVisible();

      await tabReview.click();
      await expect(page.getByTestId("mode-3-review-container")).toBeVisible();

      await tabDashboard.click();
      await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
    }

    // Assert zero layout overflow after rapid navigation
    await assertZeroHorizontalOverflow(page);
  });

  test("C2-INTERACT-03: Rapid Drag-and-Drop enter and leave stress test on dropzone", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");
    await page.getByTestId("mode-tab-dashboard").click();

    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    const dropzone = page.getByTestId("document-dropzone");
    await expect(dropzone).toBeVisible();

    // Rapid drag enter and leave simulation
    for (let i = 0; i < 8; i++) {
      await dropzone.dispatchEvent("dragover");
      await dropzone.dispatchEvent("dragleave");
    }

    // After rapid drag stress, verify dropzone is still responsive and functional
    await dropzone.dispatchEvent("dragover");
    await expect(dropzone).toHaveClass(/border-indigo-600/);
    await dropzone.dispatchEvent("dragleave");
    await expect(dropzone).not.toHaveClass(/border-indigo-600/);

    await page.keyboard.press("Escape");
    await expect(modal).toBeHidden();
  });

  test("C2-TOUCH-01: Touch target bounding box audit on all Dashboard interactive elements", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");
    await page.getByTestId("mode-tab-dashboard").click();

    // Check Header Action Buttons
    const primaryHeaderButtons = [
      page.getByTestId("open-upload-pdf-btn"),
      page.getByTestId("open-split-notebook-btn"),
      page.getByTestId("open-practice-quiz-btn")
    ];

    for (const btn of primaryHeaderButtons) {
      await expect(btn).toBeVisible();
      const box = await btn.boundingBox();
      expect(box).not.toBeNull();
      if (box) {
        expect(box.height, `Header button height ${box.height}px >= 40px`).toBeGreaterThanOrEqual(40);
        expect(box.width, `Header button width ${box.width}px >= 40px`).toBeGreaterThanOrEqual(40);
      }
    }

    // Check Onboarding quick review button
    const onboardingBtn = page.getByTestId("onboarding-start-review-btn");
    await expect(onboardingBtn).toBeVisible();
    const obBox = await onboardingBtn.boundingBox();
    expect(obBox).not.toBeNull();
    if (obBox) {
      expect(obBox.height, `Onboarding button height ${obBox.height}px >= 40px`).toBeGreaterThanOrEqual(40);
      expect(obBox.width, `Onboarding button width ${obBox.width}px >= 40px`).toBeGreaterThanOrEqual(40);
    }

    // Check Hero card primary CTA
    const heroCard = page.locator(".lg\\:col-span-7").first();
    const heroPrimaryBtn = heroCard.locator("button").first();
    await expect(heroPrimaryBtn).toBeVisible();
    const heroBox = await heroPrimaryBtn.boundingBox();
    expect(heroBox).not.toBeNull();
    if (heroBox) {
      expect(heroBox.height, `Hero CTA height ${heroBox.height}px >= 40px`).toBeGreaterThanOrEqual(40);
      expect(heroBox.width, `Hero CTA width ${heroBox.width}px >= 40px`).toBeGreaterThanOrEqual(40);
    }

    // Check Review Station CTA
    const reviewBtn = page.getByTestId("launch-unified-review-btn");
    await expect(reviewBtn).toBeVisible();
    const rBox = await reviewBtn.boundingBox();
    expect(rBox).not.toBeNull();
    if (rBox) {
      expect(rBox.height, `Review CTA height ${rBox.height}px >= 40px`).toBeGreaterThanOrEqual(40);
      expect(rBox.width, `Review CTA width ${rBox.width}px >= 40px`).toBeGreaterThanOrEqual(40);
    }

    // Check Document Library action buttons
    const libButtons = page.locator(".lg\\:col-span-8 button");
    const count = await libButtons.count();
    expect(count).toBeGreaterThan(0);
    for (let i = 0; i < Math.min(count, 6); i++) {
      const btn = libButtons.nth(i);
      const bBox = await btn.boundingBox();
      expect(bBox).not.toBeNull();
      if (bBox) {
        expect(bBox.height, `Library action button [${i}] height ${bBox.height}px >= 38px`).toBeGreaterThanOrEqual(38);
        expect(bBox.width, `Library action button [${i}] width ${bBox.width}px >= 40px`).toBeGreaterThanOrEqual(40);
      }
    }

    // Check Upload Modal interactive targets
    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    const closeBtn = modal.locator('button[aria-label="Close"]');
    const closeBox = await closeBtn.boundingBox();
    expect(closeBox).not.toBeNull();
    if (closeBox) {
      expect(closeBox.height, `Modal close button height ${closeBox.height}px >= 40px`).toBeGreaterThanOrEqual(40);
      expect(closeBox.width, `Modal close button width ${closeBox.width}px >= 40px`).toBeGreaterThanOrEqual(40);
    }

    const browseBtn = modal.getByTestId("browse-computer-file-btn");
    const browseBox = await browseBtn.boundingBox();
    expect(browseBox).not.toBeNull();
    if (browseBox) {
      expect(browseBox.height, `Browse file button height ${browseBox.height}px >= 40px`).toBeGreaterThanOrEqual(40);
      expect(browseBox.width, `Browse file button width ${browseBox.width}px >= 40px`).toBeGreaterThanOrEqual(40);
    }

    const cancelBtn = modal.locator('button:has-text("Hủy")');
    const cancelBox = await cancelBtn.boundingBox();
    expect(cancelBox).not.toBeNull();
    if (cancelBox) {
      expect(cancelBox.height, `Cancel button height ${cancelBox.height}px >= 40px`).toBeGreaterThanOrEqual(40);
      expect(cancelBox.width, `Cancel button width ${cancelBox.width}px >= 40px`).toBeGreaterThanOrEqual(40);
    }

    const submitBtn = modal.locator('button[type="submit"]');
    const submitBox = await submitBtn.boundingBox();
    expect(submitBox).not.toBeNull();
    if (submitBox) {
      expect(submitBox.height, `Submit button height ${submitBox.height}px >= 40px`).toBeGreaterThanOrEqual(40);
      expect(submitBox.width, `Submit button width ${submitBox.width}px >= 40px`).toBeGreaterThanOrEqual(40);
    }

    await page.keyboard.press("Escape");
    await expect(modal).toBeHidden();
  });
});
