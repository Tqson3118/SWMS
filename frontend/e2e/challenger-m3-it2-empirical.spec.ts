import { test, expect } from "./fixtures/test-base";
import { assertZeroHorizontalOverflow } from "./fixtures/ergonomics-helpers";

test.describe("Milestone 3 Iteration 2 Adversarial Challenge Suite", () => {
  // =========================================================================
  // 1. MODULE RESOLUTION ROBUSTNESS
  // =========================================================================
  test("M3-CHALLENGE-01: Module resolution robustness - @/lib/animations vs @/lib/animations.ts", async ({
    page
  }) => {
    // 1.0 Test dynamic import of '../src/lib/animations' (extensionless resolution)
    const modExtensionless = await import("../src/lib/animations");
    expect(typeof modExtensionless.animateWorkspaceTransition).toBe("function");
    expect(typeof modExtensionless.scrollToAndPulseHighlight).toBe("function");
    expect(typeof modExtensionless.waitForElementAndPulse).toBe("function");
    expect(typeof modExtensionless.triggerCompletionCelebration).toBe("function");
    expect(typeof modExtensionless.default).toBe("object");
    expect(typeof modExtensionless.default.waitForElementAndPulse).toBe("function");

    // 1.1 Test dynamic import of '../src/lib/animations.ts' (explicit TS)
    const modWithExt = await import("../src/lib/animations.ts");
    expect(typeof modWithExt.animateWorkspaceTransition).toBe("function");
    expect(typeof modWithExt.scrollToAndPulseHighlight).toBe("function");
    expect(typeof modWithExt.waitForElementAndPulse).toBe("function");
    expect(typeof modWithExt.triggerCompletionCelebration).toBe("function");
    expect(typeof modWithExt.default).toBe("object");
    expect(typeof modWithExt.default.waitForElementAndPulse).toBe("function");

    // 1.2 Test dynamic import of '../src/lib/animations.js' (explicit JS)
    const modJs = await import("../src/lib/animations.js");
    expect(typeof modJs.animateWorkspaceTransition).toBe("function");
    expect(typeof modJs.scrollToAndPulseHighlight).toBe("function");
    expect(typeof modJs.waitForElementAndPulse).toBe("function");
    expect(typeof modJs.triggerCompletionCelebration).toBe("function");
    expect(typeof modJs.default).toBe("object");
    expect(typeof modJs.default.waitForElementAndPulse).toBe("function");

    // 1.3 Verify waitForElementAndPulse runs without TypeError in Node context
    const nodeResult = await modWithExt.waitForElementAndPulse("non-existent-id", 50, 10);
    expect(nodeResult).toBe(false);

    // 1.4 Verify in browser context: navigate to /workspace and execute animation
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    const splitTab = page.getByTestId("mode-tab-split");
    await expect(splitTab).toBeVisible({ timeout: 15000 });
    await splitTab.click();

    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toBeVisible({ timeout: 15000 });

    // Test Jump-to-Source in Split Mode does not trigger TypeError
    const jumpBtn = page.getByTestId("jump-to-source-btn").first();
    if (await jumpBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await jumpBtn.click();
      await page.waitForTimeout(400);
    }
  });

  // =========================================================================
  // 2. SPLIT WORKSPACE DRAG CLAMPING & DOUBLE-CLICK RESET
  // =========================================================================
  test("M3-CHALLENGE-02: Split pane drag clamping beyond min/max & double-click reset", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    const splitTab = page.getByTestId("mode-tab-split");
    await expect(splitTab).toBeVisible({ timeout: 15000 });
    await splitTab.click();

    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toBeVisible({ timeout: 15000 });

    // Initial check: default ratios should be 15/45/40
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");

    const handle1 = page.getByTestId("split-resize-handle-1");
    const handle2 = page.getByTestId("split-resize-handle-2");
    await expect(handle1).toBeVisible({ timeout: 10000 });
    await expect(handle2).toBeVisible({ timeout: 10000 });

    // -----------------------------------------------------------------------
    // Stress Vector 2.1: Handle 1 Drag Extreme Left (-600px)
    // Pane 1 must clamp at minimum 10% (cannot collapse to 0% or negative)
    // -----------------------------------------------------------------------
    const box1 = await handle1.boundingBox();
    expect(box1).not.toBeNull();
    await page.mouse.move(box1!.x + box1!.width / 2, box1!.y + box1!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box1!.x - 600, box1!.y + box1!.height / 2, { steps: 10 });
    await page.mouse.up();

    let ratiosAttr = await splitContainer.getAttribute("data-split-ratios");
    let [r1, r2, r3] = (ratiosAttr || "").split("/").map(Number);
    expect(r1, "Handle 1 drag left: Pane 1 must clamp to min 10%").toBe(10);
    expect(r1 + r2 + r3, "Total ratio sum must equal 100%").toBeCloseTo(100, 1);
    expect(r2, "Pane 2 must be within [30, 60]").toBeGreaterThanOrEqual(30);
    expect(r2, "Pane 2 must be within [30, 60]").toBeLessThanOrEqual(60);
    expect(r3, "Pane 3 must be within [25, 55]").toBeGreaterThanOrEqual(25);
    expect(r3, "Pane 3 must be within [25, 55]").toBeLessThanOrEqual(55);

    // -----------------------------------------------------------------------
    // Stress Vector 2.2: Handle 1 Drag Extreme Right (+600px)
    // Pane 1 must clamp at maximum 25% (cannot monopolize the workspace)
    // -----------------------------------------------------------------------
    const box1After = await handle1.boundingBox();
    expect(box1After).not.toBeNull();
    await page.mouse.move(box1After!.x + box1After!.width / 2, box1After!.y + box1After!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box1After!.x + 600, box1After!.y + box1After!.height / 2, { steps: 10 });
    await page.mouse.up();

    ratiosAttr = await splitContainer.getAttribute("data-split-ratios");
    [r1, r2, r3] = (ratiosAttr || "").split("/").map(Number);
    expect(r1, "Handle 1 drag right: Pane 1 must clamp to max 25%").toBe(25);
    expect(r1 + r2 + r3, "Total ratio sum must equal 100%").toBeCloseTo(100, 1);
    expect(r2, "Pane 2 must be within [30, 60]").toBeGreaterThanOrEqual(30);
    expect(r2, "Pane 2 must be within [30, 60]").toBeLessThanOrEqual(60);
    expect(r3, "Pane 3 must be within [25, 55]").toBeGreaterThanOrEqual(25);
    expect(r3, "Pane 3 must be within [25, 55]").toBeLessThanOrEqual(55);

    // -----------------------------------------------------------------------
    // Stress Vector 2.3: Double Click Handle 1 -> Reset to exactly [15, 45, 40]
    // -----------------------------------------------------------------------
    await handle1.dblclick();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");
    await expect(handle1).toHaveAttribute("aria-valuenow", "15");
    await expect(handle2).toHaveAttribute("aria-valuenow", "45");

    // -----------------------------------------------------------------------
    // Stress Vector 2.4: Handle 2 Drag Extreme Left (-800px)
    // Pane 2 must clamp at minimum 30%, Pane 3 must clamp at max 55%
    // -----------------------------------------------------------------------
    const box2 = await handle2.boundingBox();
    expect(box2).not.toBeNull();
    await page.mouse.move(box2!.x + box2!.width / 2, box2!.y + box2!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box2!.x - 800, box2!.y + box2!.height / 2, { steps: 10 });
    await page.mouse.up();

    ratiosAttr = await splitContainer.getAttribute("data-split-ratios");
    [r1, r2, r3] = (ratiosAttr || "").split("/").map(Number);
    expect(r2, "Handle 2 drag left: Pane 2 must clamp to min 30%").toBe(30);
    expect(r3, "Handle 2 drag left: Pane 3 must clamp to max 55% or complement").toBeLessThanOrEqual(55);
    expect(r1 + r2 + r3, "Total ratio sum must equal 100%").toBeCloseTo(100, 1);

    // -----------------------------------------------------------------------
    // Stress Vector 2.5: Handle 2 Drag Extreme Right (+800px)
    // Pane 2 must clamp at maximum 60%, Pane 3 must clamp at min 25%
    // -----------------------------------------------------------------------
    const box2After = await handle2.boundingBox();
    expect(box2After).not.toBeNull();
    await page.mouse.move(box2After!.x + box2After!.width / 2, box2After!.y + box2After!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box2After!.x + 800, box2After!.y + box2After!.height / 2, { steps: 10 });
    await page.mouse.up();

    ratiosAttr = await splitContainer.getAttribute("data-split-ratios");
    [r1, r2, r3] = (ratiosAttr || "").split("/").map(Number);
    expect(r2, "Handle 2 drag right: Pane 2 must clamp to max 60%").toBe(60);
    expect(r3, "Handle 2 drag right: Pane 3 must clamp to min 25%").toBe(25);
    expect(r1 + r2 + r3, "Total ratio sum must equal 100%").toBeCloseTo(100, 1);

    // -----------------------------------------------------------------------
    // Stress Vector 2.6: Double Click Handle 2 -> Reset to exactly [15, 45, 40]
    // -----------------------------------------------------------------------
    await handle2.dblclick();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");
    await expect(handle1).toHaveAttribute("aria-valuenow", "15");
    await expect(handle2).toHaveAttribute("aria-valuenow", "45");
  });

  // =========================================================================
  // 3. ZERO HORIZONTAL OVERFLOW AT TABLET (768px, 1024px) & DESKTOP (1440px)
  // =========================================================================
  const VIEWPORT_CONFIGS = [
    { name: "Desktop (1440x900)", width: 1440, height: 900, isTablet: false },
    { name: "Tablet Landscape (1024x768)", width: 1024, height: 768, isTablet: true },
    { name: "Tablet Portrait (768x1024)", width: 768, height: 1024, isTablet: true }
  ];

  for (const vp of VIEWPORT_CONFIGS) {
    test(`M3-CHALLENGE-03: Zero horizontal overflow at ${vp.name}`, async ({ page }) => {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.goto("/workspace");
      await page.waitForLoadState("domcontentloaded");

      const splitTab = page.getByTestId("mode-tab-split");
      await expect(splitTab).toBeVisible({ timeout: 15000 });
      await splitTab.click();

      const splitContainer = page.getByTestId("mode-2-split-container");
      await expect(splitContainer).toBeVisible({ timeout: 15000 });

      // Verify zero horizontal overflow at document root
      await assertZeroHorizontalOverflow(page);

      // Verify split container itself does not cause scroll
      const containerBox = await splitContainer.boundingBox();
      expect(containerBox).not.toBeNull();
      expect(
        containerBox!.width,
        `Container width (${containerBox!.width}px) must not exceed viewport width (${vp.width}px)`
      ).toBeLessThanOrEqual(vp.width);

      const handle2 = page.getByTestId("split-resize-handle-2");
      await expect(handle2).toBeVisible({ timeout: 10000 });

      if (vp.isTablet) {
        // On tablet viewports (<1280px), Pane 1 sidebar and Handle 1 should be hidden
        const handle1 = page.getByTestId("split-resize-handle-1");
        await expect(handle1).toBeHidden();
        const sidebar = page.getByTestId("split-pane-sidebar");
        await expect(sidebar).toBeHidden();

        // Drag handle 2 on tablet to extreme bounds and assert zero overflow
        const h2Box = await handle2.boundingBox();
        expect(h2Box).not.toBeNull();
        await page.mouse.move(h2Box!.x + h2Box!.width / 2, h2Box!.y + h2Box!.height / 2);
        await page.mouse.down();
        await page.mouse.move(h2Box!.x + 300, h2Box!.y + h2Box!.height / 2, { steps: 5 });
        await page.mouse.up();

        await assertZeroHorizontalOverflow(page);

        // Reset via double-click on tablet
        await handle2.dblclick();
        await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");
        await assertZeroHorizontalOverflow(page);
      } else {
        // On desktop (1440px), Pane 1 sidebar and Handle 1 should be visible
        const handle1 = page.getByTestId("split-resize-handle-1");
        await expect(handle1).toBeVisible();
        const sidebar = page.getByTestId("split-pane-sidebar");
        await expect(sidebar).toBeVisible();
      }
    });
  }
});
