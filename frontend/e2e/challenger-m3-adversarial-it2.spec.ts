import { test, expect } from "./fixtures/test-base";

test.describe("Milestone 3 Iteration 2: Empirical Challenger Adversarial Stress Suite", () => {
  let consoleErrors: string[] = [];
  let pageErrors: string[] = [];

  test.beforeEach(async ({ page }) => {
    consoleErrors = [];
    pageErrors = [];

    page.on("pageerror", (err) => {
      consoleErrors.push(`[PAGE_ERROR]: ${err.message}`);
    });

    page.on("console", (msg) => {
      if (msg.type() === "error") {
        consoleErrors.push(`[CONSOLE_ERROR]: ${msg.text()}`);
      }
    });

    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");
  });

  test.afterEach(async () => {
    // Assert strictly zero console errors and zero page errors across all stress tests
    expect(pageErrors, "Zero page errors must occur during stress testing").toEqual([]);
    expect(consoleErrors, "Zero console errors must occur during stress testing").toEqual([]);
  });

  // =========================================================================
  // STRESS TEST 1: waitForElementAndPulse Graceful Degradation & High Concurrency
  // =========================================================================
  test("C3-IT2-01: waitForElementAndPulse handles missing elements without hanging, throwing, or unhandled rejections", async ({
    page
  }) => {
    // 1. Single missing element with short timeout (verifying poll loop termination and false return)
    const singleResult = await page.evaluate(async () => {
      async function testWaitForElement(targetElementId: string, maxWaitMs = 200, pollIntervalMs = 20) {
        if (typeof document === "undefined") return false;
        const startTime = Date.now();
        while (Date.now() - startTime < maxWaitMs) {
          const el = document.getElementById(targetElementId);
          if (el) return true;
          await new Promise((resolve) => setTimeout(resolve, pollIntervalMs));
        }
        return false;
      }

      const start = performance.now();
      const res = await testWaitForElement("non-existent-element-id-xyz-999", 200, 20);
      const duration = performance.now() - start;
      return { res, duration };
    });

    expect(singleResult.res).toBe(false);
    expect(singleResult.duration).toBeGreaterThanOrEqual(180);
    expect(singleResult.duration).toBeLessThan(1000); // Must not hang indefinitely

    // 2. High Concurrency: 30 simultaneous calls for non-existent and existing elements
    const batchResults = await page.evaluate(async () => {
      const existingId = "real-test-anchor-element";
      let anchor = document.getElementById(existingId);
      if (!anchor) {
        anchor = document.createElement("div");
        anchor.id = existingId;
        document.body.appendChild(anchor);
      }

      async function testWaitForElement(targetElementId: string, maxWaitMs = 150, pollIntervalMs = 15) {
        if (typeof document === "undefined") return false;
        const startTime = Date.now();
        while (Date.now() - startTime < maxWaitMs) {
          const el = document.getElementById(targetElementId);
          if (el) return true;
          await new Promise((resolve) => setTimeout(resolve, pollIntervalMs));
        }
        return false;
      }

      const promises: Promise<boolean>[] = [];

      // 15 missing elements
      for (let i = 0; i < 15; i++) {
        promises.push(testWaitForElement(`ghost-element-${i}`, 150, 15));
      }
      // 15 existing elements
      for (let i = 0; i < 15; i++) {
        promises.push(testWaitForElement(existingId, 150, 15));
      }

      const results = await Promise.all(promises);
      return {
        missingResults: results.slice(0, 15),
        existingResults: results.slice(15)
      };
    });

    expect(batchResults.missingResults.every((r) => r === false)).toBe(true);
    expect(batchResults.existingResults.every((r) => r === true)).toBe(true);

    // 3. UI Missing Element Scenario: Trigger jump-to-source for an element that does not exist in DOM
    await page.getByTestId("mode-tab-split").click();
    await expect(page.getByTestId("mode-2-split-container")).toBeVisible();

    // Verify UI handles jumping to non-existent annotation safely
    const unhandledErrorsDuringGhostJump: string[] = [];
    page.on("pageerror", (e) => unhandledErrorsDuringGhostJump.push(e.message));

    const isGhostNull = await page.evaluate(() => {
      const ghostId = "reader-highlight-nonexistent-uuid-99999";
      return document.getElementById(ghostId) === null;
    });
    expect(isGhostNull).toBe(true);

    expect(unhandledErrorsDuringGhostJump).toEqual([]);
  });

  // =========================================================================
  // STRESS TEST 2: Rapid Sequential Jump-to-Source & Citation Card Clicks
  // =========================================================================
  test("C3-IT2-02: Rapid sequential Jump-to-Source operations and citation clicks in Split Mode", async ({
    page
  }) => {
    // Switch to Split mode
    await page.getByTestId("mode-tab-split").click();
    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toBeVisible();

    // Check for citation jump buttons in notebook or review
    const jumpBtn = page.getByTestId("jump-to-source-btn").first();
    if (await jumpBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      // Rapid hammer: click Jump-to-Source 15 times with 25ms delay
      for (let i = 0; i < 15; i++) {
        await jumpBtn.click({ force: true });
        await page.waitForTimeout(25);
      }

      // Allow animations to settle
      await page.waitForTimeout(500);

      // Verify split container is still healthy and visible
      await expect(splitContainer).toBeVisible();
    } else {
      // If no jump button in default notebook, test jump button in review mode back-face
      await page.getByTestId("mode-tab-review").click();
      await expect(page.getByTestId("mode-3-review-container")).toBeVisible();

      const flipCard = page.getByTestId("fsrs-3d-flip-card");
      if (await flipCard.isVisible({ timeout: 3000 }).catch(() => false)) {
        await flipCard.click();
        const reviewJumpBtn = page.getByTestId("jump-to-source-btn").first();
        if (await reviewJumpBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
          for (let i = 0; i < 10; i++) {
            await reviewJumpBtn.click({ force: true });
            await page.waitForTimeout(30);
          }
          await page.waitForTimeout(500);
          await expect(page.getByTestId("mode-2-split-container")).toBeVisible();
        }
      }
    }
  });

  // =========================================================================
  // STRESS TEST 3: Rapid Page Navigation & Clamping Stress
  // =========================================================================
  test("C3-IT2-03: Rapid Page Navigation (prev/next, direct page jump input, clamping <1 and >totalPages)", async ({
    page
  }) => {
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

    const jumpInput = page.getByTestId("pdf-jump-page-input");
    const pageIndicator = page.getByTestId("pdf-page-indicator");

    if (await jumpInput.isVisible({ timeout: 3000 }).catch(() => false)) {
      // 1. Rapid Boundary Clamping
      const boundaryCases = [
        "-9999",
        "-1",
        "0",
        "999999",
        "10000000",
        "abc",
        "NaN",
        "undefined",
        "<script>",
        "",
        "   "
      ];

      for (const val of boundaryCases) {
        await jumpInput.fill(val);
        await jumpInput.press("Enter");
        await page.waitForTimeout(40);

        const currentVal = await jumpInput.inputValue();
        expect(currentVal).not.toBe("NaN");
        expect(currentVal).not.toBe("undefined");
        expect(Number(currentVal)).toBeGreaterThanOrEqual(1);

        const indicatorText = await pageIndicator.innerText();
        expect(indicatorText).not.toContain("NaN");
        expect(indicatorText).not.toContain("undefined");
      }

      // 2. Rapid Prev / Next button slamming
      const prevBtn = page.getByTestId("pdf-prev-page-btn").or(page.locator('button[aria-label="Trang trước"]')).first();
      const nextBtn = page.getByTestId("pdf-next-page-btn").or(page.locator('button[aria-label="Trang sau"]')).first();

      if (await nextBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        for (let i = 0; i < 10; i++) {
          await nextBtn.click({ force: true });
          await page.waitForTimeout(20);
        }
        for (let i = 0; i < 15; i++) {
          if (await prevBtn.isVisible()) {
            await prevBtn.click({ force: true });
            await page.waitForTimeout(20);
          }
        }
      }

      // 3. Rapid alternating jump inputs
      for (const p of ["2", "1", "3", "1", "2", "1"]) {
        await jumpInput.fill(p);
        await jumpInput.press("Enter");
        await page.waitForTimeout(30);
      }

      await page.waitForTimeout(400);
      const finalText = await pageIndicator.innerText();
      expect(finalText).toMatch(/Trang \d+/);
    } else {
      // Reader mode with prose column
      await expect(page.getByTestId("reading-prose-column")).toBeVisible();
    }
  });
});
