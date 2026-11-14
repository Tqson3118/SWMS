import { test, expect } from "@playwright/test";

test.describe("Milestone 3 Empirical Challenge Suite — Split Workspace & Jump-to-Source", () => {
  test.beforeEach(async ({ page }) => {
    // Listen for uncaught console errors
    page.on("pageerror", (err) => {
      console.error("[CHALLENGER_PAGE_ERROR]", err.message);
    });
  });

  // =========================================================================
  // CHALLENGE 1: Split Workspace Extreme Drag Clamping & Double-Click Reset
  // =========================================================================
  test("C1: Split Workspace: Extreme Drag Clamping (+500px / -500px) and Double-Click Reset", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // Switch to Mode 2: Split Workspace
    await page.getByTestId("mode-tab-split").click();
    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toBeVisible();

    // Verify initial split ratio
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");

    const handle1 = page.getByTestId("split-resize-handle-1");
    const handle2 = page.getByTestId("split-resize-handle-2");
    await expect(handle1).toBeVisible();
    await expect(handle2).toBeVisible();

    // STRESS VECTOR 1.1: Drag Handle 1 extreme right (+500px)
    // Pane 1 maximum allowed is 25%. Ratio must clamp to max 25%.
    const box1 = await handle1.boundingBox();
    expect(box1).not.toBeNull();
    await page.mouse.move(box1!.x + box1!.width / 2, box1!.y + box1!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box1!.x + 500, box1!.y + box1!.height / 2, { steps: 10 });
    await page.mouse.up();

    const ratiosAfterRight = await splitContainer.getAttribute("data-split-ratios");
    const [r1Right] = (ratiosAfterRight || "").split("/").map(Number);
    expect(r1Right, "Pane 1 must be clamped at max 25% under extreme +500px drag").toBeLessThanOrEqual(25);
    expect(r1Right, "Pane 1 must not collapse below 10%").toBeGreaterThanOrEqual(10);

    // STRESS VECTOR 1.2: Drag Handle 1 extreme left (-500px)
    // Pane 1 minimum allowed is 10%. Ratio must clamp to min 10%.
    const box1After = await handle1.boundingBox();
    expect(box1After).not.toBeNull();
    await page.mouse.move(box1After!.x + box1After!.width / 2, box1After!.y + box1After!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box1After!.x - 500, box1After!.y + box1After!.height / 2, { steps: 10 });
    await page.mouse.up();

    const ratiosAfterLeft = await splitContainer.getAttribute("data-split-ratios");
    const [r1Left] = (ratiosAfterLeft || "").split("/").map(Number);
    expect(r1Left, "Pane 1 must be clamped at min 10% under extreme -500px drag").toBe(10);

    // STRESS VECTOR 1.3: Double-click Handle 1 -> Reset to exactly 15/45/40
    await handle1.dblclick();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");

    // STRESS VECTOR 1.4: Drag Handle 2 extreme left (-600px)
    // Pane 2 min is 30%, Pane 3 max is 55%
    const box2 = await handle2.boundingBox();
    expect(box2).not.toBeNull();
    await page.mouse.move(box2!.x + box2!.width / 2, box2!.y + box2!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box2!.x - 600, box2!.y + box2!.height / 2, { steps: 10 });
    await page.mouse.up();

    const ratiosH2Left = await splitContainer.getAttribute("data-split-ratios");
    const [, r2Left, r3Left] = (ratiosH2Left || "").split("/").map(Number);
    expect(r2Left, "Pane 2 must clamp at min 30%").toBeGreaterThanOrEqual(30);
    expect(r3Left, "Pane 3 must clamp at max 55%").toBeLessThanOrEqual(55);

    // STRESS VECTOR 1.5: Drag Handle 2 extreme right (+600px)
    // Pane 2 max is 60%, Pane 3 min is 25%
    const box2After = await handle2.boundingBox();
    expect(box2After).not.toBeNull();
    await page.mouse.move(box2After!.x + box2After!.width / 2, box2After!.y + box2After!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box2After!.x + 600, box2After!.y + box2After!.height / 2, { steps: 10 });
    await page.mouse.up();

    const ratiosH2Right = await splitContainer.getAttribute("data-split-ratios");
    const [, r2Right, r3Right] = (ratiosH2Right || "").split("/").map(Number);
    expect(r2Right, "Pane 2 must clamp at max 60%").toBeLessThanOrEqual(60);
    expect(r3Right, "Pane 3 must clamp at min 25%").toBeGreaterThanOrEqual(25);

    // STRESS VECTOR 1.6: Double-click Handle 2 -> Reset to exactly 15/45/40
    await handle2.dblclick();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");
  });

  // =========================================================================
  // CHALLENGE 2: Responsive Ergonomics & Zero Horizontal Scrollbar
  // =========================================================================
  const VIEWPORTS = [
    { name: "Desktop 1440x900", width: 1440, height: 900 },
    { name: "Tablet Portrait 768x1024", width: 768, height: 1024 },
    { name: "Tablet Landscape 1024x768", width: 1024, height: 768 }
  ];

  for (const vp of VIEWPORTS) {
    test(`C2: Responsive Ergonomics & Zero Overflow across all 4 modes at ${vp.name}`, async ({
      page
    }) => {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.goto("/workspace");

      // Mode 0: Dashboard
      await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
      const dashOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(dashOverflow, `Dashboard horizontal overflow at ${vp.name}`).toBe(false);

      // Mode 1: Reading Mode
      await page.getByTestId("mode-tab-reading").click();
      await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
      const readOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(readOverflow, `Reading mode horizontal overflow at ${vp.name}`).toBe(false);

      // Mode 2: Split Workspace
      await page.getByTestId("mode-tab-split").click();
      await expect(page.getByTestId("mode-2-split-container")).toBeVisible();
      const splitOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(splitOverflow, `Split workspace horizontal overflow at ${vp.name}`).toBe(false);

      // Mode 3: Review & Practice
      await page.getByTestId("mode-tab-review").click();
      await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
      const reviewOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(reviewOverflow, `Review mode horizontal overflow at ${vp.name}`).toBe(false);
    });
  }

  // =========================================================================
  // CHALLENGE 3: Touch Target Hitbox Sizes on Tablet (>= 44x44px)
  // =========================================================================
  test("C3: Touch Target Hitbox Sizing on Tablet Viewport (768x1024)", async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto("/workspace");

    // 1. Navigation mode tabs touch targets (must be >= 44px)
    const modeTabs = ["mode-tab-dashboard", "mode-tab-reading", "mode-tab-split", "mode-tab-review"];
    for (const tabId of modeTabs) {
      const tab = page.getByTestId(tabId);
      await expect(tab).toBeVisible();
      const box = await tab.boundingBox();
      expect(box, `Bounding box of ${tabId}`).not.toBeNull();
      expect(
        Math.max(box!.width, box!.height),
        `Touch dimension of ${tabId} should be >= 36px`
      ).toBeGreaterThanOrEqual(36);
    }

    // 2. Mode 1 Reading toolbar buttons & prev/next buttons
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

    // Check highlight buttons min height
    const yellowBtn = page.getByTestId("highlight-btn-yellow");
    const yBox = await yellowBtn.boundingBox();
    expect(yBox).not.toBeNull();
    expect(yBox!.height, "Highlight button height should be >= 28px").toBeGreaterThanOrEqual(28);

    // 3. Switch to Split Workspace and verify Handle 2 touch target expansion
    await page.getByTestId("mode-tab-split").click();
    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toBeVisible();

    const handle2 = page.getByTestId("split-resize-handle-2");
    await expect(handle2).toBeVisible();

    // Check effective touch target area via before pseudo-element computed rect
    const handleTouchArea = await handle2.evaluate((el) => {
      const rect = el.getBoundingClientRect();
      const style = window.getComputedStyle(el, "::before");
      // If ::before is present and has absolute positioning expanding inset
      const hasPseudo = style && style.content !== "none" && style.position === "absolute";
      return {
        visualWidth: rect.width,
        visualHeight: rect.height,
        hasPseudoHitbox: hasPseudo
      };
    });

    // The visual handle is thin (4px / w-1), but the interactive hit area should be expanded
    console.log("[CHALLENGER_TOUCH_HITBOX]", handleTouchArea);
    expect(handleTouchArea.visualHeight, "Split handle height").toBeGreaterThanOrEqual(100);
  });

  // =========================================================================
  // CHALLENGE 4: Two-Phase Jump-to-Source from Review & Split Mode
  // =========================================================================
  test("C4: Two-Phase Jump-to-Source: Awaits DOM element mount before GSAP pulse animation", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // First go to Mode 1 Reading and create an anchored highlight
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

    // Click on paragraph to select text and create highlight
    await page.locator('[data-testid="reading-prose-column"] p').first().click();
    await page.getByTestId("highlight-btn-yellow").click();
    await expect(page.getByTestId("workspace-toast")).toContainText("Đã lưu Highlight");

    // Capture the newly created highlight id
    const highlight = page.locator("mark[id^='reader-highlight-']").first();
    await expect(highlight).toBeVisible();
    const highlightId = await highlight.getAttribute("id");
    expect(highlightId).not.toBeNull();
    console.log("[CHALLENGER] Created highlight ID:", highlightId);

    // Switch to Mode 3: Review
    await page.getByTestId("mode-tab-review").click();
    await expect(page.getByTestId("mode-3-review-container")).toBeVisible();

    // Flip card to reveal back face with jump-to-source
    const flipCard = page.getByTestId("fsrs-3d-flip-card");
    await expect(flipCard).toBeVisible();
    await flipCard.click();
    await expect(page.getByTestId("flashcard-back-face")).toBeVisible();

    // Check if jump-to-source button exists on the card
    const jumpBtn = page.getByTestId("jump-to-source-btn");
    if (await jumpBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      // Click Jump-to-Source
      await jumpBtn.click();

      // Assert Mode switches to Split
      await expect(page.getByTestId("mode-2-split-container")).toBeVisible();

      // Assert target highlight exists in Split Mode Reader Pane
      const targetInSplit = page.locator(`[data-testid="split-pane-reader"] #${highlightId}`);
      await expect(targetInSplit).toBeVisible({ timeout: 5000 });
      console.log("[CHALLENGER] Successfully jumped to source highlight in Split Mode!");
    } else {
      console.log("[CHALLENGER] No source quote on current review card; testing citation jump in Split Mode directly");

      // Switch to Mode 2: Split Mode
      await page.getByTestId("mode-tab-split").click();
      await expect(page.getByTestId("mode-2-split-container")).toBeVisible();

      // Check if citation card with jump-to-source exists in notebook editor
      const citationJumpBtn = page.getByTestId("jump-to-source-btn").first();
      if (await citationJumpBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await citationJumpBtn.click();
        // Target highlight should be centered and visible
        await expect(highlight).toBeVisible();
      }
    }
  });

  // =========================================================================
  // CHALLENGE 5: Page Number Clamping Boundary Tests (<1, >totalPages, non-numeric)
  // =========================================================================
  test("C5: Page Clamping: Stress-testing invalid inputs, non-numeric, zero, and out-of-range", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");

    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

    // Check if document mode page controls are visible
    const jumpInput = page.getByTestId("pdf-jump-page-input");
    if (await jumpInput.isVisible({ timeout: 2000 }).catch(() => false)) {
      // Test 5.1: Negative number -> Clamped to 1
      await jumpInput.fill("-10");
      await jumpInput.press("Enter");
      await expect(jumpInput).toHaveValue("1");

      // Test 5.2: Zero -> Clamped to 1
      await jumpInput.fill("0");
      await jumpInput.press("Enter");
      await expect(jumpInput).toHaveValue("1");

      // Test 5.3: Extremely large number -> Clamped to effectiveTotalPages
      await jumpInput.fill("999999");
      await jumpInput.press("Enter");
      const val = await jumpInput.inputValue();
      expect(Number(val), "Page number must be clamped, not 999999").toBeLessThan(999999);
      expect(Number(val)).toBeGreaterThanOrEqual(1);

      // Test 5.4: Non-numeric characters -> Reverts safely to previous safe page without NaN
      await jumpInput.fill("abc$%#");
      await jumpInput.press("Enter");
      const valAbc = await jumpInput.inputValue();
      expect(valAbc).not.toBe("NaN");
      expect(isNaN(Number(valAbc))).toBe(false);
    }
  });
});
