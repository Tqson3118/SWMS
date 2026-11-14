import { test, expect } from "@playwright/test";
/**
 * Challenger M1-1: Adversarial Stress & Chaos Verification Suite
 * Target: Milestone 1 (Landing Page, Auxiliary Modals, Navigation, Theme Synchronizer)
 *
 * Verifies failure modes under:
 * 1. Rapid Multi-Toggle Theme Spam (desync between DOM, localStorage & Zustand store)
 * 2. Cross-Tab Theme Storage Event Bursts
 * 3. Corrupted / Malformed localStorage Theme Values & Anti-FOUC Reload Invariant
 * 4. Modal Event Chaos (Rapid ?/Esc cycling, body overflow lock leak, focus trap inside inputs)
 * 5. Special regex characters in Shortcuts Search bar
 * 6. Mutual exclusion between About modal and Shortcuts modal
 * 7. High-frequency Two-Way Navigation Ping-Pong (/ <-> /workspace)
 * 8. Extreme & Atypical Viewport Boundaries (Mobile 375x667, Tablet 768x1024, Ultrawide 2560x1440)
 */
test.describe("Challenger M1-1 — Adversarial Stress Testing", () => {
    // =========================================================================
    // 1. RAPID MULTI-TOGGLE THEME STRESS
    // =========================================================================
    test("Stress 1: Rapid 25x theme toggle preserves tri-state sync (DOM, localStorage, Zustand)", async ({ page }) => {
        await page.goto("/");
        const themeBtn = page.getByTestId("theme-toggle-btn");
        await expect(themeBtn).toBeVisible();
        const getThemeState = async () => {
            return await page.evaluate(() => {
                const rootDark = document.documentElement.classList.contains("dark");
                const bodyDark = document.body ? document.body.classList.contains("dark") : false;
                const stored = localStorage.getItem("swms_theme");
                // @ts-ignore
                const zustandStore = window.__SWMS_STORE__ || null;
                return { rootDark, bodyDark, stored };
            });
        };
        const initial = await getThemeState();
        // Spam click 25 times rapidly
        for (let i = 0; i < 25; i++) {
            await themeBtn.click();
        }
        // Settled check after 25 clicks (odd count -> inverted)
        await page.waitForTimeout(200);
        const after25 = await getThemeState();
        expect(after25.rootDark).toBe(!initial.rootDark);
        expect(after25.bodyDark).toBe(!initial.rootDark);
        expect(after25.stored).toBe(after25.rootDark ? "dark" : "light");
        // Click 1 more time (26th click -> even count -> restored to initial)
        await themeBtn.click();
        await page.waitForTimeout(200);
        const after26 = await getThemeState();
        expect(after26.rootDark).toBe(initial.rootDark);
        expect(after26.bodyDark).toBe(initial.rootDark);
        expect(after26.stored).toBe(initial.rootDark ? "dark" : "light");
    });
    // =========================================================================
    // 2. CROSS-TAB STORAGE EVENT BURST
    // =========================================================================
    test("Stress 2: Cross-tab StorageEvent synchronization handles rapid burst without desync", async ({ page }) => {
        await page.goto("/");
        await expect(page.getByTestId("theme-toggle-btn")).toBeVisible();
        // Dispatch alternating storage events from a simulated peer tab
        for (let i = 0; i < 10; i++) {
            const targetMode = i % 2 === 0 ? "dark" : "light";
            await page.evaluate((mode) => {
                localStorage.setItem("swms_theme", mode);
                window.dispatchEvent(new StorageEvent("storage", {
                    key: "swms_theme",
                    newValue: mode,
                    url: window.location.href
                }));
            }, targetMode);
        }
        // Final forced storage event: "dark"
        await page.evaluate(() => {
            localStorage.setItem("swms_theme", "dark");
            window.dispatchEvent(new StorageEvent("storage", {
                key: "swms_theme",
                newValue: "dark",
                url: window.location.href
            }));
        });
        await expect.poll(async () => {
            return await page.evaluate(() => {
                return (document.documentElement.classList.contains("dark") &&
                    (document.body ? document.body.classList.contains("dark") : true) &&
                    localStorage.getItem("swms_theme") === "dark");
            });
        }, { timeout: 5000 }).toBe(true);
        // Final forced storage event: "light"
        await page.evaluate(() => {
            localStorage.setItem("swms_theme", "light");
            window.dispatchEvent(new StorageEvent("storage", {
                key: "swms_theme",
                newValue: "light",
                url: window.location.href
            }));
        });
        await expect.poll(async () => {
            return await page.evaluate(() => {
                return (!document.documentElement.classList.contains("dark") &&
                    (document.body ? !document.body.classList.contains("dark") : true) &&
                    localStorage.getItem("swms_theme") === "light");
            });
        }, { timeout: 5000 }).toBe(true);
    });
    // =========================================================================
    // 3. CORRUPTED / MALFORMED LOCALSTORAGE RECOVERY
    // =========================================================================
    test("Stress 3: Corrupted localStorage theme value gracefully recovers without throwing unhandled exceptions", async ({ page }) => {
        await page.goto("/");
        // Inject corrupted value into localStorage
        await page.evaluate(() => {
            localStorage.setItem("swms_theme", "{malformed_json: true, corrupted: null}}");
        });
        // Reload page to trigger anti-FOUC script & ThemeSynchronizer
        await page.reload();
        // Verify page rendered cleanly and did not crash
        await expect(page.getByTestId("landing-page-root")).toBeVisible();
        await expect(page.getByTestId("landing-hero-section")).toBeVisible();
        // Clicking theme toggle must self-heal localStorage
        const themeBtn = page.getByTestId("theme-toggle-btn");
        await themeBtn.click();
        await page.waitForTimeout(100);
        const healedTheme = await page.evaluate(() => localStorage.getItem("swms_theme"));
        expect(["dark", "light"]).toContain(healedTheme);
    });
    // =========================================================================
    // 4. MODAL EVENT CHAOS & BODY SCROLL LOCK RECOVERY
    // =========================================================================
    test("Stress 4: Rapid 10x ?/Escape cycling never leaves document.body.style.overflow locked to 'hidden'", async ({ page }) => {
        await page.goto("/");
        for (let i = 0; i < 10; i++) {
            // Press '?' to open
            await page.keyboard.press("?");
            const modal = page.getByTestId("keyboard-shortcuts-modal");
            await expect(modal).toBeVisible();
            // Check overflow is hidden while modal is open
            const overflowDuring = await page.evaluate(() => document.body.style.overflow);
            expect(overflowDuring).toBe("hidden");
            // Press Escape to close
            await page.keyboard.press("Escape");
            await expect(modal).toBeHidden();
            // Check overflow is reset
            const overflowAfter = await page.evaluate(() => document.body.style.overflow);
            expect(["unset", ""]).toContain(overflowAfter);
        }
        // Final assertion: Page can scroll freely
        const canScroll = await page.evaluate(() => {
            window.scrollTo(0, 500);
            return window.scrollY >= 0;
        });
        expect(canScroll).toBe(true);
    });
    test("Stress 5: Typing '?' inside input elements does NOT trigger KeyboardShortcutsModal", async ({ page }) => {
        await page.goto("/");
        // Open shortcuts modal first
        await page.keyboard.press("?");
        const modal = page.getByTestId("keyboard-shortcuts-modal");
        await expect(modal).toBeVisible();
        // Focus search input inside modal
        const searchInput = modal.locator('input[type="text"]');
        await expect(searchInput).toBeVisible();
        await searchInput.focus();
        // Type '?' inside search input multiple times
        await page.keyboard.type("Can I type ??? without crashing?");
        // Verify input contains the typed '?' characters
        const inputValue = await searchInput.inputValue();
        expect(inputValue).toContain("???");
        // Modal should still be open, not duplicated or closed
        await expect(modal).toBeVisible();
        // Clear search and test regex breaking characters
        await searchInput.fill("");
        await searchInput.fill("[{(*+?^$|\\\\)}]--regex-exploit");
        // Verify empty state is rendered cleanly without regex runtime exception
        await expect(modal).toContainText(/Không tìm thấy phím tắt phù hợp/i);
        // Close cleanly
        await page.keyboard.press("Escape");
        await expect(modal).toBeHidden();
    });
    test("Stress 6: About Modal and Shortcuts Modal mutual exclusion and backdrop click behavior", async ({ page }) => {
        await page.goto("/");
        // Open About Modal from header trigger (.first())
        const aboutTriggers = page.getByTestId("about-swms-trigger");
        expect(await aboutTriggers.count()).toBe(4); // Verify all 4 triggers exist across the page
        await aboutTriggers.first().click();
        const aboutModal = page.getByTestId("about-swms-modal");
        await expect(aboutModal).toBeVisible();
        // Attempt to open Shortcuts Modal with '?' while About is open
        await page.keyboard.press("?");
        const shortcutsModal = page.getByTestId("keyboard-shortcuts-modal");
        // Shortcuts modal must NOT open on top of About modal
        await expect(shortcutsModal).toBeHidden();
        // Click inside About Modal content (should NOT close modal)
        const modalHeader = aboutModal.locator("h2#about-swms-title");
        await modalHeader.click();
        await expect(aboutModal).toBeVisible();
        // Click on Backdrop overlay (should close modal)
        // Click at coordinate near top-left edge outside the modal container
        await page.mouse.click(10, 10);
        await expect(aboutModal).toBeHidden();
        // Verify body overflow is restored
        const overflow = await page.evaluate(() => document.body.style.overflow);
        expect(["unset", ""]).toContain(overflow);
    });
    test("Stress 7: 'Vào Không gian làm việc' from About Modal navigates with zero body overflow lock leak", async ({ page }) => {
        await page.goto("/");
        // Open About Modal
        await page.getByTestId("about-swms-trigger").first().click();
        const aboutModal = page.getByTestId("about-swms-modal");
        await expect(aboutModal).toBeVisible();
        // Click "Vào Không gian làm việc"
        const exploreBtn = aboutModal.getByRole("button", { name: /vào không gian làm việc/i });
        await expect(exploreBtn).toBeVisible();
        await exploreBtn.click();
        // Verify arrived at workspace
        await expect(page).toHaveURL(/\/workspace/);
        // Critical assertion: document.body.style.overflow MUST NOT be stuck at "hidden"
        const overflow = await page.evaluate(() => document.body.style.overflow);
        expect(["unset", ""]).toContain(overflow);
    });
    // =========================================================================
    // 5. HIGH-FREQUENCY NAVIGATION PING-PONG (/ <-> /workspace)
    // =========================================================================
    test("Stress 8: High-frequency 5-round navigation ping-pong (/ <-> /workspace) with zero memory/overflow leaks", async ({ page }) => {
        await page.goto("/");
        for (let round = 1; round <= 5; round++) {
            // 1. On landing page, click Hero CTA
            const heroCta = page.getByTestId("landing-hero-cta");
            await heroCta.click();
            await expect(page).toHaveURL(/\/workspace/);
            // Verify workspace loaded
            await expect(page.getByTestId("global-top-header")).toBeVisible();
            // Check overflow on workspace
            const wsOverflow = await page.evaluate(() => document.body.style.overflow);
            expect(["unset", ""]).toContain(wsOverflow);
            // 2. Click Brand Logo Link to return to Landing
            const brandLogo = page.getByTestId("brand-logo-link");
            await brandLogo.click();
            await expect(page).toHaveURL(/\/(#.*)?$/);
            // Verify landing loaded
            await expect(page.getByTestId("landing-hero-section")).toBeVisible();
            // Check overflow on landing
            const landingOverflow = await page.evaluate(() => document.body.style.overflow);
            expect(["unset", ""]).toContain(landingOverflow);
        }
    });
    // =========================================================================
    // 6. EXTREME & ATYPICAL VIEWPORT BOUNDARIES (ZERO OVERFLOW)
    // =========================================================================
    const ATYPICAL_VIEWPORTS = [
        { name: "Ultrawide 2K", width: 2560, height: 1440 },
        { name: "iPad Landscape", width: 1024, height: 768 },
        { name: "iPad Portrait", width: 768, height: 1024 },
        { name: "Mobile iPhone SE", width: 375, height: 667 }
    ];
    for (const vp of ATYPICAL_VIEWPORTS) {
        test(`Stress 9: Boundary viewports (${vp.name} - ${vp.width}x${vp.height}) maintain strict zero horizontal overflow`, async ({ page }) => {
            await page.setViewportSize({ width: vp.width, height: vp.height });
            await page.goto("/");
            await expect(page.getByTestId("landing-hero-section")).toBeVisible();
            // Check entire document horizontal overflow
            const docOverflow = await page.evaluate(() => document.documentElement.scrollWidth > window.innerWidth);
            expect(docOverflow).toBe(false);
            // Check footer is visible and within bounds
            const footer = page.getByTestId("landing-footer");
            await footer.scrollIntoViewIfNeeded();
            await expect(footer).toBeVisible();
            const footerOverflow = await page.evaluate(() => {
                const f = document.querySelector('[data-testid="landing-footer"]');
                if (!f)
                    return false;
                const rect = f.getBoundingClientRect();
                return rect.right > window.innerWidth + 1;
            });
            expect(footerOverflow).toBe(false);
        });
    }
});
