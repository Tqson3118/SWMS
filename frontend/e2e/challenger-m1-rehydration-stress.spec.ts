import { test, expect } from "./fixtures/test-base";

/**
 * Empirical Challenger M1-IT2 Suite:
 * Deep Stress Testing for Remediated Theme Synchronization & Storage Integrity
 *
 * Requirements:
 * 1. Rehydration Stress: Browser refresh with dark mode stored in localStorage.
 *    Confirm `.dark` remains on <html> and <body> and is NEVER stripped (verified via MutationObserver).
 * 2. Rapid Theme Toggle Stress: Rapid theme switches with zero desync across DOM, state, and localStorage.
 * 3. Zero Console Error Verification: Zero unhandled console errors, zero hydration mismatches, zero 404s.
 */

test.describe("Empirical Challenger M1-IT2 — Theme Synchronization & Rehydration Stress", () => {
  test.beforeEach(async ({ page }) => {
    // Clear storage before each test
    await page.goto("about:blank");
  });

  // =========================================================================
  // 1. REHYDRATION STRESS: MUTATIONOBSERVER VERIFICATION
  // =========================================================================

  test("Rehydration Stress 1: Dark mode in localStorage is NEVER stripped from <html> or <body> during page load and rehydration", async ({
    page
  }) => {
    // Install MutationObserver BEFORE any HTML is parsed
    await page.addInitScript(() => {
      localStorage.setItem("swms_theme", "dark");
      (window as any).__THEME_MUTATIONS__ = [];

      const observer = new MutationObserver((mutations) => {
        for (const m of mutations) {
          if (m.type === "attributes" && m.attributeName === "class") {
            const el = m.target as HTMLElement;
            if (el.tagName === "HTML" || el.tagName === "BODY") {
              (window as any).__THEME_MUTATIONS__.push({
                time: performance.now(),
                target: el.tagName,
                classList: el.className,
                hasDark: el.classList.contains("dark")
              });
            }
          }
        }
      });

      // Observe entire document tree for class attribute changes
      observer.observe(document, { attributes: true, subtree: true, attributeFilter: ["class"] });
    });

    // 1. Navigate to /workspace
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");
    await page.waitForTimeout(300);

    // Assert DOM states
    const rootHasDark = await page.evaluate(() => document.documentElement.classList.contains("dark"));
    const bodyHasDark = await page.evaluate(() => document.body ? document.body.classList.contains("dark") : false);
    const storedTheme = await page.evaluate(() => localStorage.getItem("swms_theme"));

    expect(rootHasDark, "<html> must have .dark class on load").toBe(true);
    expect(bodyHasDark, "<body> must have .dark class on load").toBe(true);
    expect(storedTheme, "localStorage must remain 'dark'").toBe("dark");

    // Check MutationObserver logs: was 'dark' ever stripped?
    const mutations = await page.evaluate(() => (window as any).__THEME_MUTATIONS__);
    const strippedEvents = mutations.filter((m: any) => m.target === "HTML" && !m.hasDark);
    expect(strippedEvents, "<html> .dark class must NEVER be removed during rehydration").toEqual([]);

    // 2. Perform 3 consecutive hard reloads (F5 spam stress)
    for (let reloadIdx = 1; reloadIdx <= 3; reloadIdx++) {
      await page.reload();
      await page.waitForLoadState("domcontentloaded");
      await page.waitForTimeout(200);

      const reloadRootDark = await page.evaluate(() => document.documentElement.classList.contains("dark"));
      const reloadBodyDark = await page.evaluate(() => document.body ? document.body.classList.contains("dark") : false);
      const reloadStored = await page.evaluate(() => localStorage.getItem("swms_theme"));

      expect(reloadRootDark, `Reload #${reloadIdx}: <html> must keep .dark`).toBe(true);
      expect(reloadBodyDark, `Reload #${reloadIdx}: <body> must keep .dark`).toBe(true);
      expect(reloadStored, `Reload #${reloadIdx}: localStorage must keep 'dark'`).toBe("dark");

      const reloadMutations = await page.evaluate(() => (window as any).__THEME_MUTATIONS__);
      const reloadStripped = reloadMutations.filter((m: any) => m.target === "HTML" && !m.hasDark);
      expect(reloadStripped, `Reload #${reloadIdx}: .dark was stripped during rehydration`).toEqual([]);
    }
  });

  test("Rehydration Stress 2: Light mode in localStorage preserves light state without unwanted .dark flash", async ({
    page
  }) => {
    await page.addInitScript(() => {
      localStorage.setItem("swms_theme", "light");
      (window as any).__THEME_MUTATIONS__ = [];

      const observer = new MutationObserver((mutations) => {
        for (const m of mutations) {
          if (m.type === "attributes" && m.attributeName === "class") {
            const el = m.target as HTMLElement;
            if (el.tagName === "HTML" || el.tagName === "BODY") {
              (window as any).__THEME_MUTATIONS__.push({
                target: el.tagName,
                hasDark: el.classList.contains("dark")
              });
            }
          }
        }
      });
      observer.observe(document, { attributes: true, subtree: true, attributeFilter: ["class"] });
    });

    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");
    await page.waitForTimeout(300);

    const rootHasDark = await page.evaluate(() => document.documentElement.classList.contains("dark"));
    const storedTheme = await page.evaluate(() => localStorage.getItem("swms_theme"));

    expect(rootHasDark).toBe(false);
    expect(storedTheme).toBe("light");

    const unwantedDarkMutations = await page.evaluate(() =>
      ((window as any).__THEME_MUTATIONS__ || []).filter((m: any) => m.target === "HTML" && m.hasDark)
    );
    expect(unwantedDarkMutations, "<html> must NEVER briefly receive .dark when stored theme is light").toEqual([]);
  });

  // =========================================================================
  // 2. RAPID THEME TOGGLE STRESS
  // =========================================================================

  test("Rapid Toggle Stress 1: 30 consecutive rapid clicks maintain 100% tri-state synchronization", async ({
    page
  }) => {
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    const themeToggleBtn = page.getByTestId("theme-toggle-btn");
    await expect(themeToggleBtn).toBeVisible();

    const getFullState = async () => {
      return await page.evaluate(() => {
        const rootDark = document.documentElement.classList.contains("dark");
        const bodyDark = document.body ? document.body.classList.contains("dark") : false;
        const stored = localStorage.getItem("swms_theme");
        return { rootDark, bodyDark, stored };
      });
    };

    const initial = await getFullState();

    // Rapid toggle burst: 30 clicks with minimal 15ms interval
    for (let i = 1; i <= 30; i++) {
      await themeToggleBtn.click({ delay: 15 });
    }

    // Wait for event loop to settle
    await page.waitForTimeout(300);

    const settledState = await getFullState();

    // 30 is even -> should restore to exact initial state
    expect(settledState.rootDark).toBe(initial.rootDark);
    expect(settledState.bodyDark).toBe(initial.rootDark);
    expect(settledState.stored).toBe(initial.rootDark ? "dark" : "light");

    // Click 1 more time -> 31 is odd -> must invert cleanly
    await themeToggleBtn.click();
    await page.waitForTimeout(200);

    const invertedState = await getFullState();
    expect(invertedState.rootDark).toBe(!initial.rootDark);
    expect(invertedState.bodyDark).toBe(!initial.rootDark);
    expect(invertedState.stored).toBe(!initial.rootDark ? "dark" : "light");
  });

  test("Rapid Toggle Stress 2: Cross-tab storage race condition resilience under rapid local toggling", async ({
    page
  }) => {
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    const themeToggleBtn = page.getByTestId("theme-toggle-btn");
    await expect(themeToggleBtn).toBeVisible();

    // Concurrently trigger storage events while user clicks toggle
    for (let i = 0; i < 5; i++) {
      await themeToggleBtn.click();
      await page.evaluate((iter) => {
        const simulatedExternalMode = iter % 2 === 0 ? "dark" : "light";
        window.dispatchEvent(
          new StorageEvent("storage", {
            key: "swms_theme",
            newValue: simulatedExternalMode,
            url: window.location.href
          })
        );
      }, i);
    }

    await page.waitForTimeout(300);

    // Final decisive toggle: Dark Mode
    await page.evaluate(() => {
      localStorage.setItem("swms_theme", "dark");
      window.dispatchEvent(
        new StorageEvent("storage", {
          key: "swms_theme",
          newValue: "dark",
          url: window.location.href
        })
      );
    });

    await expect.poll(async () => {
      return await page.evaluate(() => {
        return (
          document.documentElement.classList.contains("dark") &&
          (document.body ? document.body.classList.contains("dark") : true) &&
          localStorage.getItem("swms_theme") === "dark"
        );
      });
    }, { timeout: 4000 }).toBe(true);
  });

  // =========================================================================
  // 3. ZERO CONSOLE ERROR & ASSET INTEGRITY VERIFICATION
  // =========================================================================

  test("Zero Console Error Verification: Full navigation flow across landing and workspace has zero errors and valid favicon", async ({
    page,
    request
  }) => {
    // 1. Verify /favicon.ico returns HTTP 200 OK
    const faviconResp = await request.get("/favicon.ico");
    expect(faviconResp.status()).toBe(200);
    const contentType = faviconResp.headers()["content-type"];
    expect(contentType).toMatch(/image\/(x-icon|vnd\.microsoft\.icon)/);

    // 2. Navigate from / to /workspace
    await page.goto("/");
    await page.waitForLoadState("domcontentloaded");
    await expect(page.getByTestId("landing-hero-section")).toBeVisible();

    // 3. Toggle theme on landing page
    const landingThemeBtn = page.getByTestId("theme-toggle-btn");
    await landingThemeBtn.click();
    await page.waitForTimeout(150);
    await landingThemeBtn.click();
    await page.waitForTimeout(150);

    // 4. Navigate to workspace
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");
    await expect(page.getByTestId("global-top-header")).toBeVisible();

    // 5. Switch between all workspace modes
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

    await page.getByTestId("mode-tab-split").click();
    await expect(page.getByTestId("mode-2-split-container")).toBeVisible();

    await page.getByTestId("mode-tab-review").click();
    await expect(page.getByTestId("mode-3-review-container")).toBeVisible();

    await page.getByTestId("mode-tab-dashboard").click();
    await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();

    // Note: capturedErrors fixture will automatically assert 0 errors during teardown
  });
});
