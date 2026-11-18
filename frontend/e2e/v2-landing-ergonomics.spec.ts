import { test, expect } from "./fixtures/test-base";
import { assertZeroHorizontalOverflow } from "./fixtures/ergonomics-helpers";

const VIEWPORTS = [
  { name: "2560x1440-QHD-2K", width: 2560, height: 1440 },
  { name: "1920x1080-FHD-100pct", width: 1920, height: 1080 },
  { name: "1536x864-FHD-125pct", width: 1536, height: 864 },
  { name: "1366x768-HD-Global", width: 1366, height: 768 },
  { name: "1280x720-HD-150pct", width: 1280, height: 720 },
  { name: "1024x768-iPad-Landscape", width: 1024, height: 768 },
  { name: "768x1024-iPad-Portrait", width: 768, height: 1024 },
  { name: "414x896-iPhone-XR-11", width: 414, height: 896 },
  { name: "390x844-iPhone-12-14", width: 390, height: 844 },
  { name: "375x667-iPhone-SE", width: 375, height: 667 },
  { name: "360x640-Android-Min", width: 360, height: 640 },
];

test.describe("Studxy V2 Landing Page — Ergonomics, Theme Sync & Multi-Viewport Stress", () => {
  test("Renders all V2 Landing sections, navigation CTAs, & info modals with Escape/backdrop close", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/", { waitUntil: "networkidle" });
    await page.evaluate(
      () => new Promise<void>((r) => requestAnimationFrame(() => requestAnimationFrame(() => r())))
    );

    await expect(page.getByTestId("landing-page-root")).toBeVisible();
    await expect(page.getByTestId("landing-header")).toBeVisible();
    await expect(page.getByTestId("landing-hero-section")).toBeVisible();
    await expect(page.getByTestId("landing-pillars-section")).toBeVisible();
    await expect(page.getByTestId("landing-workflow-section")).toBeVisible();
    await expect(page.getByTestId("landing-shortcuts-section")).toBeVisible();
    await expect(page.getByTestId("landing-footer")).toBeVisible();

    // '?' scrolls smoothly to #shortcuts
    await page.keyboard.press("?");
    await expect(page.locator("#shortcuts")).toBeInViewport({ timeout: 5000 });

    // Open Privacy modal -> verify body overflow lock -> close with Escape -> verify restored
    await page.getByTestId("privacy-modal-trigger").click();
    const modal = page.getByTestId("landing-info-modal");
    await expect(modal).toBeVisible();
    expect(await page.evaluate(() => document.body.style.overflow)).toBe("hidden");

    await page.keyboard.press("Escape");
    await expect(modal).toBeHidden();
    expect(await page.evaluate(() => document.body.style.overflow)).not.toBe("hidden");

    // Open Terms modal -> close via button
    await page.getByTestId("terms-modal-trigger").click();
    await expect(modal).toBeVisible();
    await page.getByTestId("close-info-modal").click();
    await expect(modal).toBeHidden();
  });

  test("Rapid 25x theme toggle preserves DOM/localStorage sync & corrupted swms_theme recovers cleanly", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await page.addInitScript(() => {
      window.localStorage.setItem("swms_theme", "{corrupted_json_value!!!");
    });

    await page.goto("/");
    const toggleBtn = page.getByTestId("theme-toggle-btn");
    await expect(toggleBtn).toBeVisible();

    for (let i = 0; i < 25; i++) {
      await toggleBtn.click();
    }

    await expect(page.locator("html")).toHaveClass(/dark/);
    await expect
      .poll(async () => page.evaluate(() => window.localStorage.getItem("swms_theme")))
      .toBe("dark");

    // Toggle once more -> back to light
    await toggleBtn.click();
    await expect(page.locator("html")).not.toHaveClass(/dark/);
    await expect
      .poll(async () => page.evaluate(() => window.localStorage.getItem("swms_theme")))
      .toBe("light");
  });

  for (const vp of VIEWPORTS) {
    test(`Zero Horizontal Overflow at ${vp.name} (${vp.width}x${vp.height})`, async ({ page }) => {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.goto("/");
      await expect(page.getByTestId("landing-hero-section")).toBeVisible();
      await assertZeroHorizontalOverflow(page);

      // Scroll through sections and verify zero overflow throughout
      await page.getByTestId("landing-footer").scrollIntoViewIfNeeded();
      await assertZeroHorizontalOverflow(page);
    });
  }
});
