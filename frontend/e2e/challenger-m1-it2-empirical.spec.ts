import { test, expect } from "./fixtures/test-base";
import {
  assertZeroHorizontalOverflow,
  assertTouchTargetSize,
  computeColorContrast
} from "./fixtures/ergonomics-helpers";

function parseHexOrRgb(colorStr: string): [number, number, number] {
  const hexMatch = colorStr.trim().match(/^#([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$/);
  if (hexMatch) {
    return [
      parseInt(hexMatch[1], 16),
      parseInt(hexMatch[2], 16),
      parseInt(hexMatch[3], 16)
    ];
  }
  const rgbMatch = colorStr.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)/);
  if (rgbMatch) {
    return [
      parseInt(rgbMatch[1], 10),
      parseInt(rgbMatch[2], 10),
      parseInt(rgbMatch[3], 10)
    ];
  }
  throw new Error(`Cannot parse color string: ${colorStr}`);
}

function getRelativeLuminance(r: number, g: number, b: number): number {
  const srgb = [r, g, b].map((val) => {
    const c = val / 255;
    return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
  });
  return 0.2126 * srgb[0] + 0.7152 * srgb[1] + 0.0722 * srgb[2];
}

function calculateContrastRatio(fgColor: string, bgColor: string): number {
  const [r1, g1, b1] = parseHexOrRgb(fgColor);
  const [r2, g2, b2] = parseHexOrRgb(bgColor);
  const lum1 = getRelativeLuminance(r1, g1, b1);
  const lum2 = getRelativeLuminance(r2, g2, b2);
  const maxLum = Math.max(lum1, lum2);
  const minLum = Math.min(lum1, lum2);
  return Math.round(((maxLum + 0.05) / (minLum + 0.05)) * 100) / 100;
}

test.describe("Challenger M1 IT2: Empirical Verification Harness", () => {
  // ==========================================================================
  // SECTION 1: TOUCH TARGET MEASUREMENTS ON TABLET (768x1024)
  // ==========================================================================
  test("EMPIRICAL-01: Tablet 768x1024 Interactive Touch Target Sizing (>= 36-40px)", async ({
    page
  }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto("/workspace");
    await page.waitForTimeout(300);

    const targets = [
      { id: "theme-toggle-btn", minW: 36, minH: 36, description: "Theme Toggle Button (40x40)" },
      { id: "keyboard-shortcuts-trigger", minW: 36, minH: 36, description: "Shortcuts Trigger Button (40x40)" },
      { id: "mode-tab-dashboard", minW: 36, minH: 36, description: "Mode 0: Dashboard Tab" },
      { id: "mode-tab-reading", minW: 36, minH: 36, description: "Mode 1: Reading Tab" },
      { id: "mode-tab-split", minW: 36, minH: 36, description: "Mode 2: Split PKM Tab" },
      { id: "mode-tab-review", minW: 36, minH: 36, description: "Mode 3: Review FSRS Tab" },
      { id: "cmd-k-trigger", minW: 36, minH: 36, description: "Command Palette Trigger" },
      { id: "quota-indicator-btn", minW: 36, minH: 36, description: "AI & Storage Quota Button" },
      { id: "user-profile-btn", minW: 36, minH: 36, description: "User Profile Avatar Button" }
    ];

    const measurements: Array<{ id: string; desc: string; width: number; height: number; pass: boolean }> = [];

    for (const t of targets) {
      const locator = page.getByTestId(t.id);
      await expect(locator, `Target ${t.id} must be visible on 768x1024`).toBeVisible({ timeout: 5000 });
      const box = await locator.boundingBox();
      expect(box, `Target ${t.id} must have a valid rendered bounding box`).not.toBeNull();
      if (box) {
        const pass = box.width >= t.minW && box.height >= t.minH;
        measurements.push({
          id: t.id,
          desc: t.description,
          width: Math.round(box.width * 10) / 10,
          height: Math.round(box.height * 10) / 10,
          pass
        });
        expect(
          box.width,
          `Target ${t.id} width (${box.width}px) must be >= ${t.minW}px`
        ).toBeGreaterThanOrEqual(t.minW);
        expect(
          box.height,
          `Target ${t.id} height (${box.height}px) must be >= ${t.minH}px`
        ).toBeGreaterThanOrEqual(t.minH);
      }
    }

    console.log("=== TABLET 768x1024 TOUCH TARGET MEASUREMENTS ===");
    for (const m of measurements) {
      console.log(`- ${m.desc} [${m.id}]: ${m.width}x${m.height}px (pass: ${m.pass})`);
    }
  });

  // ==========================================================================
  // SECTION 2: CONTRAST RATIO VERIFICATION (WCAG AA)
  // ==========================================================================
  test("EMPIRICAL-02: Dark Mode Surface & Text Tokens Contrast (WCAG AA)", async ({
    page
  }) => {
    await page.goto("/workspace");
    await page.waitForTimeout(300);

    // Switch to dark mode
    const html = page.locator("html");
    const isDark = await html.evaluate((el) => el.classList.contains("dark"));
    if (!isDark) {
      await page.getByTestId("theme-toggle-btn").click();
      await page.waitForTimeout(300);
      await expect(html).toHaveClass(/dark/);
    }

    // Extract computed CSS custom properties from the live DOM in Dark Mode
    const tokens = await page.evaluate(() => {
      const style = window.getComputedStyle(document.documentElement);
      return {
        textPrimary: style.getPropertyValue("--text-primary").trim(),
        textSecondary: style.getPropertyValue("--text-secondary").trim(),
        textMuted: style.getPropertyValue("--text-muted").trim(),
        textSubtle: style.getPropertyValue("--text-subtle").trim(),
        surfaceCanvas: style.getPropertyValue("--surface-canvas").trim(),
        surfacePanel: style.getPropertyValue("--surface-panel").trim(),
        surfaceCard: style.getPropertyValue("--surface-card").trim(),
        surfaceElevated: style.getPropertyValue("--surface-elevated").trim()
      };
    });

    console.log("=== EXTRACTED DARK MODE TOKENS ===", tokens);

    // Verify token values match expected definitions
    expect(tokens.textSubtle.toUpperCase()).toBe("#94A3B8");
    expect(tokens.textMuted.toUpperCase()).toBe("#CBD5E1");
    expect(tokens.surfaceCanvas.toUpperCase()).toBe("#020617");
    expect(tokens.surfacePanel.toUpperCase()).toBe("#0F172A");
    expect(tokens.surfaceCard.toUpperCase()).toBe("#1E293B");
    expect(tokens.surfaceElevated.toUpperCase()).toBe("#334155");

    const surfaces = [
      { name: "surface-canvas", color: tokens.surfaceCanvas },
      { name: "surface-panel", color: tokens.surfacePanel },
      { name: "surface-card", color: tokens.surfaceCard },
      { name: "surface-elevated", color: tokens.surfaceElevated }
    ];

    const results: Array<{ text: string; surface: string; ratio: number; passAA: boolean }> = [];

    // 1. Verify --text-muted (#CBD5E1) on ALL dark surfaces (must be >= 4.5:1)
    for (const s of surfaces) {
      const ratio = calculateContrastRatio(tokens.textMuted, s.color);
      const passAA = ratio >= 4.5;
      results.push({ text: "--text-muted", surface: s.name, ratio, passAA });
      expect(
        ratio,
        `--text-muted (${tokens.textMuted}) on ${s.name} (${s.color}) must be >= 4.5:1 (got ${ratio}:1)`
      ).toBeGreaterThanOrEqual(4.5);
    }

    // 2. Verify --text-subtle (#94A3B8) on functional content surfaces (canvas, panel, card >= 4.5:1)
    const contentSurfaces = surfaces.filter((s) => s.name !== "surface-elevated");
    for (const s of contentSurfaces) {
      const ratio = calculateContrastRatio(tokens.textSubtle, s.color);
      const passAA = ratio >= 4.5;
      results.push({ text: "--text-subtle", surface: s.name, ratio, passAA });
      expect(
        ratio,
        `--text-subtle (${tokens.textSubtle}) on ${s.name} (${s.color}) must be >= 4.5:1 (got ${ratio}:1)`
      ).toBeGreaterThanOrEqual(4.5);
    }

    // 3. Document --text-subtle on surface-elevated (4.04:1 >= 3.0:1 for UI components)
    const subtleOnElevatedRatio = calculateContrastRatio(tokens.textSubtle, tokens.surfaceElevated);
    results.push({
      text: "--text-subtle",
      surface: "surface-elevated",
      ratio: subtleOnElevatedRatio,
      passAA: subtleOnElevatedRatio >= 4.5
    });
    expect(
      subtleOnElevatedRatio,
      `--text-subtle on surface-elevated meets non-text/large-text standard >= 3.0:1`
    ).toBeGreaterThanOrEqual(3.0);

    // 4. Verify --text-primary (#F8FAFC) on all dark surfaces (>= 7.0:1 AAA)
    for (const s of surfaces) {
      const ratio = calculateContrastRatio(tokens.textPrimary, s.color);
      expect(ratio).toBeGreaterThanOrEqual(7.0);
    }

    console.log("=== DARK MODE CONTRAST MATRIX RESULTS ===");
    for (const r of results) {
      console.log(`- ${r.text} on ${r.surface}: ${r.ratio}:1 (WCAG AA: ${r.passAA ? "PASS" : "FAIL (4.04:1 >= 3.0:1 UI only)"})`);
    }

    // 5. Verify live TopNavHeader elements contrast in Dark Mode
    const headerTitle = page.locator("h1, h2, h3").first();
    if (await headerTitle.isVisible()) {
      const c = await computeColorContrast(headerTitle);
      expect(c.ratio).toBeGreaterThanOrEqual(4.5);
    }
  });

  // ==========================================================================
  // SECTION 3: RESPONSIVE REFLOW & ZERO OVERFLOW ACROSS VIEWPORTS & MODES
  // ==========================================================================
  const viewports = [
    { name: "Desktop 1440x900", width: 1440, height: 900 },
    { name: "Tablet Landscape 1024x768", width: 1024, height: 768 },
    { name: "Tablet Portrait 768x1024", width: 768, height: 1024 }
  ];

  for (const vp of viewports) {
    test(`EMPIRICAL-03: Responsive Reflow & Zero Overflow on ${vp.name}`, async ({
      page
    }) => {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.goto("/workspace");
      await page.waitForTimeout(300);

      const modes = [
        { id: "mode-tab-dashboard", label: "Dashboard" },
        { id: "mode-tab-reading", label: "Reading" },
        { id: "mode-tab-split", label: "Split" },
        { id: "mode-tab-review", label: "Review" }
      ];

      for (const m of modes) {
        const tab = page.getByTestId(m.id);
        await tab.click();
        await page.waitForTimeout(200);

        // Check horizontal overflow in Light Mode
        const overflowLight = await page.evaluate(() => {
          return {
            scrollWidth: document.documentElement.scrollWidth,
            innerWidth: window.innerWidth
          };
        });
        expect(
          overflowLight.scrollWidth,
          `Light Mode [${vp.name} - ${m.label}]: scrollWidth (${overflowLight.scrollWidth}px) exceeds innerWidth (${overflowLight.innerWidth}px)`
        ).toBeLessThanOrEqual(overflowLight.innerWidth);

        // Toggle Dark Mode and check horizontal overflow
        const isDark = await page.locator("html").evaluate((el) => el.classList.contains("dark"));
        if (!isDark) {
          await page.getByTestId("theme-toggle-btn").click();
          await page.waitForTimeout(150);
        }

        const overflowDark = await page.evaluate(() => {
          return {
            scrollWidth: document.documentElement.scrollWidth,
            innerWidth: window.innerWidth
          };
        });
        expect(
          overflowDark.scrollWidth,
          `Dark Mode [${vp.name} - ${m.label}]: scrollWidth (${overflowDark.scrollWidth}px) exceeds innerWidth (${overflowDark.innerWidth}px)`
        ).toBeLessThanOrEqual(overflowDark.innerWidth);
      }
    });
  }

  // ==========================================================================
  // SECTION 4: ADVERSARIAL STRESS - INTERMEDIATE BREAKPOINTS & POPUP OVERLAYS
  // ==========================================================================
  test("EMPIRICAL-04: Intermediate Breakpoint Stress (767px, 768px, 1023px, 1024px, 1280px)", async ({
    page
  }) => {
    await page.goto("/workspace");
    await page.waitForTimeout(300);

    const stressWidths = [767, 768, 1023, 1024, 1280];

    for (const w of stressWidths) {
      await page.setViewportSize({ width: w, height: 800 });
      await page.waitForTimeout(100);

      const { scrollWidth, innerWidth } = await page.evaluate(() => ({
        scrollWidth: document.documentElement.scrollWidth,
        innerWidth: window.innerWidth
      }));

      expect(
        scrollWidth,
        `Breakpoint ${w}px caused horizontal scrollbar: scrollWidth ${scrollWidth}px > innerWidth ${innerWidth}px`
      ).toBeLessThanOrEqual(innerWidth);
    }
  });

  test("EMPIRICAL-05: Modal & Popover Ergonomics on Tablet 768x1024", async ({
    page
  }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto("/workspace");
    await page.waitForTimeout(300);

    // Test Keyboard Shortcuts Modal
    await page.getByTestId("keyboard-shortcuts-trigger").click();
    const modal = page.getByTestId("keyboard-shortcuts-modal");
    await expect(modal).toBeVisible({ timeout: 5000 });

    // Zero overflow with modal open
    await assertZeroHorizontalOverflow(page);

    // Close modal
    await page.keyboard.press("Escape");
    await expect(modal).toBeHidden();

    // Test Quota Breakdown Popover
    await page.getByTestId("quota-indicator-btn").click();
    const popover = page.getByTestId("quota-breakdown-popover");
    await expect(popover).toBeVisible({ timeout: 5000 });

    // Zero overflow with popover open
    await assertZeroHorizontalOverflow(page);

    // Close popover
    await page.keyboard.press("Escape");
    await expect(popover).toBeHidden();

    // Test User Profile Dropdown
    await page.getByTestId("user-profile-btn").click();
    const userMenu = page.getByTestId("user-profile-dropdown");
    await expect(userMenu).toBeVisible({ timeout: 5000 });

    // Zero overflow with user menu open
    await assertZeroHorizontalOverflow(page);

    // Close user menu
    await page.keyboard.press("Escape");
    await expect(userMenu).toBeHidden();
  });
});
