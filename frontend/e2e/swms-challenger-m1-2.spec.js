import { test, expect } from "@playwright/test";
/**
 * Empirical Adversarial Test Suite for Milestone 1
 * Agent: challenger_m1_2 (critic, specialist)
 * Scope: Multi-viewport responsive layout, ergonomics, zero horizontal overflow,
 *        ComparisonMatrixSection stress, VisualWorkspacePreview ergonomics,
 *        and Auxiliary Modals usability across 11 viewports (360px to 2560px).
 */
const ADVERSARIAL_VIEWPORTS = [
    { name: "2560x1440-QHD-2K", width: 2560, height: 1440, deviceType: "desktop-ultrawide" },
    { name: "1920x1080-FHD-100pct", width: 1920, height: 1080, deviceType: "desktop-standard" },
    { name: "1536x864-FHD-125pct", width: 1536, height: 864, deviceType: "laptop-125pct" },
    { name: "1366x768-HD-Global", width: 1366, height: 768, deviceType: "laptop-hd" },
    { name: "1280x720-HD-150pct", width: 1280, height: 720, deviceType: "laptop-150pct" },
    { name: "1024x768-iPad-Landscape", width: 1024, height: 768, deviceType: "tablet-landscape" },
    { name: "768x1024-iPad-Portrait", width: 768, height: 1024, deviceType: "tablet-portrait" },
    { name: "414x896-iPhone-XR-11", width: 414, height: 896, deviceType: "mobile-large" },
    { name: "390x844-iPhone-12-14", width: 390, height: 844, deviceType: "mobile-standard" },
    { name: "375x667-iPhone-SE", width: 375, height: 667, deviceType: "mobile-compact" },
    { name: "360x640-Android-Min", width: 360, height: 640, deviceType: "mobile-minimum" },
];
test.describe("Milestone 1 Adversarial Challenge: Multi-Viewport & Responsive Stress", () => {
    // =========================================================================
    // CHALLENGE 1: ZERO HORIZONTAL OVERFLOW ACROSS ALL 11 VIEWPORTS
    // =========================================================================
    for (const vp of ADVERSARIAL_VIEWPORTS) {
        test(`[Adversarial Viewport] Zero Horizontal Overflow at ${vp.width}x${vp.height} (${vp.name})`, async ({ page }) => {
            await page.setViewportSize({ width: vp.width, height: vp.height });
            await page.goto("/", { waitUntil: "domcontentloaded" });
            // Ensure primary elements are rendered
            await expect(page.getByTestId("landing-page-root")).toBeVisible();
            await expect(page.getByTestId("landing-hero-section")).toBeVisible();
            await expect(page.getByTestId("landing-comparison-matrix")).toBeVisible();
            // 1. Strict Document-Level Horizontal Overflow Check
            const overflowMetrics = await page.evaluate(() => {
                const docEl = document.documentElement;
                const body = document.body;
                const windowWidth = window.innerWidth;
                const scrollWidthDoc = docEl.scrollWidth;
                const scrollWidthBody = body.scrollWidth;
                const clientWidthDoc = docEl.clientWidth;
                return {
                    windowWidth,
                    scrollWidthDoc,
                    scrollWidthBody,
                    clientWidthDoc,
                    hasDocOverflow: scrollWidthDoc > windowWidth,
                    hasBodyOverflow: scrollWidthBody > windowWidth
                };
            });
            expect(overflowMetrics.hasDocOverflow, `Document scrollWidth (${overflowMetrics.scrollWidthDoc}px) exceeds window innerWidth (${overflowMetrics.windowWidth}px) at ${vp.name}`).toBe(false);
            expect(overflowMetrics.hasBodyOverflow, `Body scrollWidth (${overflowMetrics.scrollWidthBody}px) exceeds window innerWidth (${overflowMetrics.windowWidth}px) at ${vp.name}`).toBe(false);
            // 2. Scan every top-level section for horizontal blowout
            const overflowingElements = await page.evaluate(() => {
                const windowWidth = window.innerWidth;
                const elements = document.querySelectorAll("header, section, footer, [data-testid^='landing-']");
                const badElements = [];
                elements.forEach((el) => {
                    const rect = el.getBoundingClientRect();
                    // Allow 1px tolerance for subpixel antialiasing/rounding
                    if (rect.right > windowWidth + 1) {
                        badElements.push({
                            tag: el.tagName,
                            testId: el.getAttribute("data-testid"),
                            right: Math.round(rect.right),
                            width: Math.round(rect.width)
                        });
                    }
                });
                return badElements;
            });
            expect(overflowingElements, `Elements overflowing viewport at ${vp.name}: ${JSON.stringify(overflowingElements)}`).toEqual([]);
        });
    }
    // =========================================================================
    // CHALLENGE 2: COMPARISON MATRIX SECTION DEEP STRESS TESTING
    // =========================================================================
    test("[Adversarial Stress] Comparison Matrix Section on Narrow Mobile (360x640 & 390x844)", async ({ page }) => {
        for (const mobileWidth of [360, 390]) {
            await page.setViewportSize({ width: mobileWidth, height: 750 });
            await page.goto("/", { waitUntil: "domcontentloaded" });
            const matrixSection = page.getByTestId("landing-comparison-matrix");
            await matrixSection.scrollIntoViewIfNeeded();
            await expect(matrixSection).toBeVisible();
            // Check section bounding box does not exceed viewport width
            const sectionBox = await matrixSection.boundingBox();
            expect(sectionBox).not.toBeNull();
            expect(sectionBox.x).toBeGreaterThanOrEqual(0);
            expect(sectionBox.x + sectionBox.width).toBeLessThanOrEqual(mobileWidth + 1);
            // Inspect internal scrollable table container
            const tableAnalysis = await page.evaluate(() => {
                const matrix = document.querySelector('[data-testid="landing-comparison-matrix"]');
                if (!matrix)
                    return null;
                const scrollContainer = matrix.querySelector(".overflow-x-auto");
                const table = matrix.querySelector("table");
                if (!scrollContainer || !table)
                    return null;
                const containerRect = scrollContainer.getBoundingClientRect();
                const tableRect = table.getBoundingClientRect();
                return {
                    containerWidth: Math.round(containerRect.width),
                    containerScrollWidth: scrollContainer.scrollWidth,
                    tableWidth: Math.round(tableRect.width),
                    tableMinW640: table.classList.contains("min-w-[640px]"),
                    canScrollX: scrollContainer.scrollWidth > containerRect.width,
                    windowInnerWidth: window.innerWidth
                };
            });
            expect(tableAnalysis).not.toBeNull();
            expect(tableAnalysis.containerWidth).toBeLessThanOrEqual(mobileWidth);
            expect(tableAnalysis.tableMinW640).toBe(true);
            // At 360px and 390px, table min-width 640px must trigger internal container horizontal scrolling
            expect(tableAnalysis.canScrollX, `At ${mobileWidth}px, internal table container must scroll horizontally without breaking parent layout`).toBe(true);
            // Verify competitor benchmark cards wrap cleanly to single column without blowing out
            const competitorCardsWrap = await page.evaluate(() => {
                const matrix = document.querySelector('[data-testid="landing-comparison-matrix"]');
                if (!matrix)
                    return null;
                const cards = matrix.querySelectorAll(".grid > div");
                const windowWidth = window.innerWidth;
                const badCards = [];
                cards.forEach((c) => {
                    const rect = c.getBoundingClientRect();
                    if (rect.right > windowWidth + 1) {
                        badCards.push({ width: Math.round(rect.width), right: Math.round(rect.right) });
                    }
                });
                return {
                    cardCount: cards.length,
                    badCards
                };
            });
            expect(competitorCardsWrap.cardCount).toBe(6);
            expect(competitorCardsWrap.badCards).toEqual([]);
        }
    });
    // =========================================================================
    // CHALLENGE 3: VISUAL WORKSPACE PREVIEW ERGONOMICS ACROSS MODES (1536 & 1280)
    // =========================================================================
    test("[Adversarial Ergonomics] Visual Workspace Preview on 1536x864 (125% DPI) and 1280x720 (150% DPI)", async ({ page }) => {
        for (const [w, h, desc] of [
            [1536, 864, "1536x864 (125% DPI Laptop)"],
            [1280, 720, "1280x720 (150% DPI Laptop)"]
        ]) {
            await page.setViewportSize({ width: w, height: h });
            await page.goto("/");
            const previewRoot = page.getByTestId("landing-workspace-preview");
            await previewRoot.scrollIntoViewIfNeeded();
            await expect(previewRoot).toBeVisible();
            // --- TAB 1: READING MODE (68ch) ---
            const tab1Btn = page.getByTestId("preview-mode-tab-1");
            await tab1Btn.click();
            if (!(await page.locator(".swms-highlight-yellow").isVisible().catch(() => false))) {
                await page.waitForTimeout(300);
                await tab1Btn.click();
            }
            await expect(page.locator(".swms-highlight-yellow")).toBeVisible();
            const readingMetrics = await page.evaluate(() => {
                const readingContainer = document.querySelector(".max-w-\\[68ch\\]");
                const yellowHighlight = document.querySelector(".swms-highlight-yellow");
                const anchorBox = document.querySelector(".bg-indigo-50\\/50");
                return {
                    hasReadingContainer: !!readingContainer,
                    readingWidth: readingContainer ? readingContainer.clientWidth : 0,
                    highlightVisible: !!yellowHighlight,
                    anchorBoxVisible: !!anchorBox,
                    isZeroOverflow: document.documentElement.scrollWidth <= window.innerWidth
                };
            });
            expect(readingMetrics.hasReadingContainer).toBe(true);
            expect(readingMetrics.isZeroOverflow).toBe(true);
            // 68ch in standard typography should be approximately between 500px and 750px
            expect(readingMetrics.readingWidth).toBeGreaterThan(450);
            expect(readingMetrics.readingWidth).toBeLessThanOrEqual(760);
            // --- TAB 2: SPLIT PKM MODE (15/45/40) ---
            const tab2Btn = page.getByTestId("preview-mode-tab-2");
            await tab2Btn.click();
            await expect(page.locator(".swms-highlight-green")).toBeVisible();
            const splitMetrics = await page.evaluate(() => {
                const outlineSidebar = Array.from(document.querySelectorAll("div")).find((d) => d.textContent?.includes("Sidebar Rail 15%"));
                const readerPane = Array.from(document.querySelectorAll("div")).find((d) => d.textContent?.includes("BÀI ĐỌC LÝ THUYẾT (45%)"));
                const notebookPane = Array.from(document.querySelectorAll("div")).find((d) => d.textContent?.includes("GHI CHÚ SOVEREIGN PKM (40%)"));
                return {
                    sidebarVisible: !!outlineSidebar && window.getComputedStyle(outlineSidebar).display !== "none",
                    readerVisible: !!readerPane,
                    notebookVisible: !!notebookPane,
                    isZeroOverflow: document.documentElement.scrollWidth <= window.innerWidth
                };
            });
            expect(splitMetrics.readerVisible).toBe(true);
            expect(splitMetrics.notebookVisible).toBe(true);
            expect(splitMetrics.sidebarVisible).toBe(true);
            expect(splitMetrics.isZeroOverflow).toBe(true);
            // --- TAB 3: FSRS REVIEW MODE ---
            const tab3Btn = page.getByTestId("preview-mode-tab-3");
            await tab3Btn.click();
            await expect(page.locator("text=FSRS v5 DSR")).toBeVisible();
            const reviewMetrics = await page.evaluate(() => {
                const buttons = Array.from(document.querySelectorAll("button")).filter((b) => b.textContent?.includes("Again") || b.textContent?.includes("Easy"));
                return {
                    ratingButtonsCount: buttons.length,
                    isZeroOverflow: document.documentElement.scrollWidth <= window.innerWidth
                };
            });
            expect(reviewMetrics.ratingButtonsCount).toBe(2); // Again & Easy buttons present
            expect(reviewMetrics.isZeroOverflow).toBe(true);
        }
    });
    // =========================================================================
    // CHALLENGE 4: PREVIEW ON NARROW MOBILE (360x640)
    // =========================================================================
    test("[Adversarial Mobile] Visual Workspace Preview on 360x640 Mobile", async ({ page }) => {
        await page.setViewportSize({ width: 360, height: 640 });
        await page.goto("/", { waitUntil: "domcontentloaded" });
        const previewRoot = page.getByTestId("landing-workspace-preview");
        await previewRoot.scrollIntoViewIfNeeded();
        // Check Mode 2 Split view stacking behavior on mobile
        const tab2Btn = page.getByTestId("preview-mode-tab-2");
        await tab2Btn.click();
        const mobilePreviewAnalysis = await page.evaluate(() => {
            const preview = document.querySelector('[data-testid="landing-workspace-preview"]');
            if (!preview)
                return null;
            const rect = preview.getBoundingClientRect();
            const docScrollWidth = document.documentElement.scrollWidth;
            const windowWidth = window.innerWidth;
            return {
                previewRight: Math.round(rect.right),
                windowWidth,
                docScrollWidth,
                hasOverflow: docScrollWidth > windowWidth
            };
        });
        expect(mobilePreviewAnalysis).not.toBeNull();
        expect(mobilePreviewAnalysis.hasOverflow).toBe(false);
        expect(mobilePreviewAnalysis.previewRight).toBeLessThanOrEqual(361);
    });
    // =========================================================================
    // CHALLENGE 5: AUXILIARY MODALS STRESS ON MOBILE VIEWPORTS (360x640 & 390x844)
    // =========================================================================
    test("[Adversarial Modals] About SWMS Modal Ergonomics & Scrollability on 360x640", async ({ page }) => {
        await page.setViewportSize({ width: 360, height: 640 });
        await page.goto("/");
        // Open About Modal via visible trigger (e.g. in HeroSection or Footer)
        const aboutTrigger = page.locator('[data-testid="about-swms-trigger"]:visible').first();
        await aboutTrigger.click();
        const modal = page.getByTestId("about-swms-modal");
        if (!(await modal.isVisible().catch(() => false))) {
            await page.waitForTimeout(300);
            await aboutTrigger.click();
        }
        await expect(modal).toBeVisible();
        // 1. Check Modal does not overflow viewport horizontally
        const modalMetrics = await page.evaluate(() => {
            const modalEl = document.querySelector('[data-testid="about-swms-modal"]');
            const dialogCard = modalEl?.querySelector(".max-w-3xl");
            if (!dialogCard)
                return null;
            const rect = dialogCard.getBoundingClientRect();
            const windowWidth = window.innerWidth;
            const windowHeight = window.innerHeight;
            return {
                windowWidth,
                windowHeight,
                dialogWidth: Math.round(rect.width),
                dialogHeight: Math.round(rect.height),
                dialogRight: Math.round(rect.right),
                isOverflowingX: rect.right > windowWidth + 1,
                isOverflowingY: rect.height > windowHeight
            };
        });
        expect(modalMetrics).not.toBeNull();
        expect(modalMetrics.isOverflowingX).toBe(false);
        expect(modalMetrics.dialogWidth).toBeLessThanOrEqual(360);
        // 2. Check all 5 tabs inside About Modal are functional and click through cleanly
        const tabs = ["sovereign", "hybrid", "privacy", "open-data", "resilience"];
        for (const tabName of tabs) {
            const tabButton = modal.locator(`button:has-text("${tabName === "sovereign" ? "Cá nhân Chủ quyền" :
                tabName === "hybrid" ? "Hybrid" :
                    tabName === "privacy" ? "Riêng tư" :
                        tabName === "open-data" ? "Chống Khóa" : "Kiến trúc"}")`).first();
            if (await tabButton.isVisible()) {
                await tabButton.click();
                const tabContent = modal.locator(".overflow-y-auto");
                await expect(tabContent).toBeVisible();
            }
        }
        // 3. Close cleanly
        await page.keyboard.press("Escape");
        await expect(modal).toBeHidden();
    });
    test("[Adversarial Modals] Keyboard Shortcuts Modal Ergonomics on 360x640", async ({ page }) => {
        await page.setViewportSize({ width: 360, height: 640 });
        await page.goto("/");
        // Open Keyboard Shortcuts Modal via visible trigger (e.g. in HeroSection)
        const shortcutsTrigger = page.locator('[data-testid="keyboard-shortcuts-trigger"]:visible').first();
        await shortcutsTrigger.click();
        const modal = page.getByTestId("keyboard-shortcuts-modal");
        if (!(await modal.isVisible().catch(() => false))) {
            await page.waitForTimeout(300);
            await shortcutsTrigger.click();
        }
        await expect(modal).toBeVisible();
        // 1. Check Modal does not overflow viewport horizontally
        const shortcutsMetrics = await page.evaluate(() => {
            const modalEl = document.querySelector('[data-testid="keyboard-shortcuts-modal"]');
            const dialogCard = modalEl?.querySelector(".max-w-4xl");
            if (!dialogCard)
                return null;
            const rect = dialogCard.getBoundingClientRect();
            const windowWidth = window.innerWidth;
            const docScrollWidth = document.documentElement.scrollWidth;
            return {
                windowWidth,
                docScrollWidth,
                dialogWidth: Math.round(rect.width),
                dialogRight: Math.round(rect.right),
                hasOverflow: docScrollWidth > windowWidth
            };
        });
        expect(shortcutsMetrics).not.toBeNull();
        expect(shortcutsMetrics.hasOverflow).toBe(false);
        expect(shortcutsMetrics.dialogRight).toBeLessThanOrEqual(361);
        // 2. Test search filter input inside modal
        const searchInput = modal.locator('input[placeholder*="Tìm phím tắt"]');
        await searchInput.fill("FSRS");
        await expect(modal.locator("text=FSRS Ôn tập").first()).toBeVisible();
        // 3. Close cleanly
        await page.keyboard.press("Escape");
        await expect(modal).toBeHidden();
    });
    // =========================================================================
    // CHALLENGE 6: HEADER COMPACT ERGONOMICS ON 360px
    // =========================================================================
    test("[Adversarial Header] Landing Header Elements Compactness at 360x640", async ({ page }) => {
        await page.setViewportSize({ width: 360, height: 640 });
        await page.goto("/", { waitUntil: "domcontentloaded" });
        const header = page.getByTestId("landing-header");
        await expect(header).toBeVisible();
        const headerMetrics = await page.evaluate(() => {
            const hdr = document.querySelector('[data-testid="landing-header"]');
            if (!hdr)
                return null;
            const rect = hdr.getBoundingClientRect();
            const brand = hdr.querySelector('a[href="/"]');
            const themeBtn = hdr.querySelector('[data-testid="theme-toggle-btn"]');
            const ctaBtn = hdr.querySelector('[data-testid="landing-nav-cta"]');
            return {
                headerWidth: Math.round(rect.width),
                headerRight: Math.round(rect.right),
                hasBrand: !!brand,
                hasThemeBtn: !!themeBtn,
                hasCtaBtn: !!ctaBtn,
                windowWidth: window.innerWidth
            };
        });
        expect(headerMetrics).not.toBeNull();
        expect(headerMetrics.headerRight).toBeLessThanOrEqual(361);
        expect(headerMetrics.hasBrand).toBe(true);
        expect(headerMetrics.hasThemeBtn).toBe(true);
        expect(headerMetrics.hasCtaBtn).toBe(true);
    });
});
