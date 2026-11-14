import { test, expect } from "@playwright/test";
/**
 * Challenger M2-2: Empirical Verification Suite for QA Report UI Layout & Animation Defects
 * Targets:
 * - LAYOUT-01: TopNavHeader mobile (375px/390px) cramping, element clipping & horizontal overflow.
 * - LAYOUT-02: Split workspace container calc(100vw - 8px) margin mismatch and pane ratio gap at 1280px.
 * - ANIM-01: FSRS 3D Card flip implementation — conditional DOM mount vs CSS 3D preserve-3d rotation.
 * - ANIM-02: Rapid mode transition flicker caused by uncancelled concurrent GSAP tweens.
 */
test.describe("Challenger M2-2 — Empirical Verification of UI Layout & Animation Defects", () => {
    // =========================================================================
    // TASK 1: EMPIRICAL VERIFICATION OF LAYOUT-01
    // =========================================================================
    test("Empirical LAYOUT-01: TopNavHeader mobile 375px & 390px viewport overflow and element squeezing", async ({ page }) => {
        // 1. Test at iPhone SE (375px)
        await page.setViewportSize({ width: 375, height: 667 });
        await page.goto("/workspace");
        await page.waitForLoadState("domcontentloaded");
        const header = page.getByTestId("global-top-header");
        await expect(header).toBeVisible();
        const metrics375 = await page.evaluate(() => {
            const docEl = document.documentElement;
            const hdr = document.querySelector('[data-testid="global-top-header"]');
            const hdrBox = hdr.getBoundingClientRect();
            const nav = hdr.querySelector("nav");
            const cmdK = document.querySelector('[data-testid="cmd-k-trigger"]');
            const cmdKBox = cmdK?.getBoundingClientRect();
            const quotaBtn = document.querySelector('[data-testid="quota-indicator-btn"]');
            const quotaBox = quotaBtn?.getBoundingClientRect();
            const themeBtn = document.querySelector('[data-testid="theme-toggle-btn"]');
            const themeBox = themeBtn?.getBoundingClientRect();
            return {
                viewportWidth: window.innerWidth,
                docScrollWidth: docEl.scrollWidth,
                docClientWidth: docEl.clientWidth,
                headerScrollWidth: hdr.scrollWidth,
                headerClientWidth: hdr.clientWidth,
                headerBoundingRight: hdrBox.right,
                navScrollWidth: nav?.scrollWidth,
                navClientWidth: nav?.clientWidth,
                cmdKBoundingRight: cmdKBox?.right,
                quotaBoundingRight: quotaBox?.right,
                themeBoundingRight: themeBox?.right,
                hasHorizontalOverflow: docEl.scrollWidth > window.innerWidth,
                hasHeaderOverflow: hdr.scrollWidth > hdr.clientWidth
            };
        });
        console.log("[EMPIRICAL 375px METRICS]", JSON.stringify(metrics375, null, 2));
        // 2. Test at iPhone 14/15 standard (390px)
        await page.setViewportSize({ width: 390, height: 844 });
        await page.waitForTimeout(100);
        const metrics390 = await page.evaluate(() => {
            const hdr = document.querySelector('[data-testid="global-top-header"]');
            return {
                viewportWidth: window.innerWidth,
                headerScrollWidth: hdr.scrollWidth,
                headerClientWidth: hdr.clientWidth,
                hasHeaderOverflow: hdr.scrollWidth > hdr.clientWidth
            };
        });
        console.log("[EMPIRICAL 390px METRICS]", JSON.stringify(metrics390, null, 2));
        // EMPIRICAL VERIFICATION FINDINGS FOR LAYOUT-01:
        // A. Header scrollWidth is 644px, far exceeding the 375px client width (>260px excess)
        expect(metrics375.headerScrollWidth).toBeGreaterThan(600);
        expect(metrics375.headerClientWidth).toBe(375);
        expect(metrics375.hasHeaderOverflow).toBe(true);
        // B. Critical controls are pushed off-screen beyond the 375px viewport boundary:
        // Cmd+K is at ~428px (>375px)
        expect(metrics375.cmdKBoundingRight).toBeGreaterThan(375);
        // Quota button is at ~528px (>375px)
        expect(metrics375.quotaBoundingRight).toBeGreaterThan(375);
        // Theme toggle is at ~600px (>375px)
        expect(metrics375.themeBoundingRight).toBeGreaterThan(375);
        // C. At 390px, header still overflows by over 250px
        expect(metrics390.hasHeaderOverflow).toBe(true);
    });
    // =========================================================================
    // TASK 2: EMPIRICAL VERIFICATION OF LAYOUT-02
    // =========================================================================
    test("Empirical LAYOUT-02: Split workspace calc(100vw - 8px) container margin mismatch at 1280px", async ({ page }) => {
        await page.setViewportSize({ width: 1280, height: 720 });
        await page.goto("/workspace");
        await page.waitForLoadState("domcontentloaded");
        await page.getByTestId("mode-tab-split").click();
        const splitContainer = page.getByTestId("mode-2-split-container");
        await expect(splitContainer).toBeVisible();
        const metrics1280 = await page.evaluate(() => {
            const hdr = document.querySelector('[data-testid="global-top-header"]');
            const hdrBox = hdr.getBoundingClientRect();
            const split = document.querySelector('[data-testid="mode-2-split-container"]');
            const splitBox = split.getBoundingClientRect();
            const computedStyle = window.getComputedStyle(split);
            const sidebar = document.querySelector('[data-testid="split-pane-sidebar"]');
            const reader = document.querySelector('[data-testid="split-pane-reader"]');
            const notebook = document.querySelector('[data-testid="split-pane-notebook"]');
            return {
                windowInnerWidth: window.innerWidth,
                headerWidth: hdrBox.width,
                headerLeft: hdrBox.left,
                headerRight: hdrBox.right,
                splitInlineWidth: split.style.width,
                splitComputedWidth: computedStyle.width,
                splitBoundingLeft: splitBox.left,
                splitBoundingRight: splitBox.right,
                splitBoundingWidth: splitBox.width,
                splitMarginLeft: computedStyle.marginLeft,
                splitMarginRight: computedStyle.marginRight,
                gapLeftToEdge: splitBox.left,
                gapRightToEdge: window.innerWidth - splitBox.right,
                sidebarWidth: sidebar?.getBoundingClientRect().width,
                readerWidth: reader?.getBoundingClientRect().width,
                notebookWidth: notebook?.getBoundingClientRect().width
            };
        });
        console.log("[EMPIRICAL 1280px METRICS]", JSON.stringify(metrics1280, null, 2));
        // EMPIRICAL VERIFICATION FINDINGS FOR LAYOUT-02:
        // A. Inline style sets fixed viewport formula calc(100vw - 8px)
        expect(metrics1280.splitInlineWidth).toContain("100vw");
        expect(metrics1280.splitInlineWidth).toContain("8px");
        // B. Header spans full viewport width (1280px edge-to-edge, left = 0)
        expect(metrics1280.headerWidth).toBe(1280);
        expect(metrics1280.headerLeft).toBe(0);
        // C. Container has an artificial gutter/gap on left and right (> 4px up to 10.36px with scrollbar)
        expect(metrics1280.gapLeftToEdge).toBeGreaterThan(3);
        expect(metrics1280.gapRightToEdge).toBeGreaterThan(3);
        // D. Split container width does NOT match header width
        expect(metrics1280.splitBoundingWidth).toBeLessThan(metrics1280.headerWidth);
    });
    // =========================================================================
    // TASK 3: EMPIRICAL VERIFICATION OF ANIM-01
    // =========================================================================
    test("Empirical ANIM-01: FSRS 3D Card flip DOM replacement vs CSS 3D transform verification", async ({ page }) => {
        await page.setViewportSize({ width: 1920, height: 1080 });
        await page.goto("/workspace");
        await page.waitForLoadState("domcontentloaded");
        await page.getByTestId("mode-tab-review").click();
        await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
        const flipCard = page.getByTestId("fsrs-3d-flip-card");
        const sessionComplete = page.getByTestId("fsrs-session-complete-card");
        // If already complete, click to restart review
        if (await sessionComplete.isVisible()) {
            await page.locator('button:has-text("Ôn tập lại hàng đợi")').click();
        }
        await expect(flipCard).toBeVisible();
        // 1. Check DOM structure and CSS styles BEFORE flip
        const beforeState = await page.evaluate(() => {
            const card = document.querySelector('[data-testid="fsrs-3d-flip-card"]');
            const front = document.querySelector('[data-testid="flashcard-front-text"]');
            const back = document.querySelector('[data-testid="flashcard-back-face"]');
            const computed = window.getComputedStyle(card);
            return {
                cardHasPerspective: computed.perspective !== "none",
                cardTransformStyle: computed.transformStyle,
                cardTransform: computed.transform,
                frontExistsInDom: !!front,
                backExistsInDom: !!back,
                cardHeight: card.getBoundingClientRect().height
            };
        });
        console.log("[ANIM-01 BEFORE FLIP]", JSON.stringify(beforeState, null, 2));
        // 2. Flip card (click or Space)
        await flipCard.click();
        await page.waitForTimeout(200);
        // 3. Check DOM structure and CSS styles AFTER flip
        const afterState = await page.evaluate(() => {
            const card = document.querySelector('[data-testid="fsrs-3d-flip-card"]');
            const front = document.querySelector('[data-testid="flashcard-front-text"]');
            const back = document.querySelector('[data-testid="flashcard-back-face"]');
            const computed = window.getComputedStyle(card);
            return {
                cardHasPerspective: computed.perspective !== "none",
                cardTransformStyle: computed.transformStyle,
                cardTransform: computed.transform,
                frontExistsInDom: !!front,
                backExistsInDom: !!back,
                cardHeight: card.getBoundingClientRect().height
            };
        });
        console.log("[ANIM-01 AFTER FLIP]", JSON.stringify(afterState, null, 2));
        // EMPIRICAL VERDICT PROOFS:
        // A. Back face was NOT in DOM initially, then conditionally mounted into DOM
        expect(beforeState.backExistsInDom).toBe(false);
        expect(afterState.backExistsInDom).toBe(true);
        // B. Front face REMAINS in DOM (not hidden behind 3D rotation)
        expect(afterState.frontExistsInDom).toBe(true);
        // C. CSS transform on card is "none" both before and after (NO 3D rotateY(180deg))
        expect(beforeState.cardTransform).toBe("none");
        expect(afterState.cardTransform).toBe("none");
        // D. Card does NOT have preserve-3d
        expect(beforeState.cardTransformStyle).toBe("flat");
        expect(afterState.cardTransformStyle).toBe("flat");
        // E. Card height expands (layout shift) to accommodate newly mounted back face
        expect(afterState.cardHeight).toBeGreaterThan(beforeState.cardHeight);
    });
    // =========================================================================
    // TASK 4: EMPIRICAL VERIFICATION OF ANIM-02
    // =========================================================================
    test("Empirical ANIM-02: Rapid mode transition flicker caused by uncancelled concurrent GSAP tweens", async ({ page }) => {
        await page.setViewportSize({ width: 1920, height: 1080 });
        await page.goto("/workspace");
        await page.waitForLoadState("domcontentloaded");
        // Measure opacity jumping during rapid switching
        // Tween duration is 260ms (0.26s) starting at opacity: 0.55
        const flickerLog = await page.evaluate(async () => {
            const readings = [];
            const mainEl = document.querySelector("main");
            const record = (step) => {
                const style = window.getComputedStyle(mainEl);
                readings.push({
                    step,
                    opacity: style.opacity,
                    transform: style.transform,
                    timestamp: performance.now()
                });
            };
            record("initial-mode-0");
            // Switch to Mode 1
            const tabReading = document.querySelector('[data-testid="mode-tab-reading"]');
            tabReading?.click();
            record("switched-to-mode-1");
            // Wait 180ms (into the accelerating phase of the 260ms tween)
            await new Promise((r) => setTimeout(r, 180));
            record("midway-mode-1-at-180ms");
            // Rapidly switch to Mode 2 at 180ms
            const tabSplit = document.querySelector('[data-testid="mode-tab-split"]');
            tabSplit?.click();
            record("switched-to-mode-2-immediately-after-click");
            // Wait 25ms to allow GSAP fromTo to re-apply
            await new Promise((r) => setTimeout(r, 25));
            record("restarted-mode-2-at-25ms");
            // Wait 180ms
            await new Promise((r) => setTimeout(r, 180));
            record("midway-mode-2-at-180ms");
            // Rapidly switch back to Mode 1 at 180ms
            tabReading?.click();
            record("switched-back-to-mode-1");
            await new Promise((r) => setTimeout(r, 25));
            record("restarted-mode-1-at-25ms");
            // Let animation settle
            await new Promise((r) => setTimeout(r, 350));
            record("settled");
            return readings;
        });
        console.log("[ANIM-02 FLICKER LOG]", JSON.stringify(flickerLog, null, 2));
        const midway1 = parseFloat(flickerLog.find((r) => r.step === "midway-mode-1-at-180ms")?.opacity || "0");
        const restart2 = parseFloat(flickerLog.find((r) => r.step === "restarted-mode-2-at-25ms")?.opacity || "1");
        console.log(`[ANIM-02 ANALYSIS] Midway opacity was ${midway1}, suddenly reset to ${restart2}`);
        // Mid-animation opacity has risen significantly above 0.55
        expect(midway1).toBeGreaterThan(0.60);
        // Restart resets opacity back to 0.55
        expect(restart2).toBeLessThanOrEqual(0.65);
        // Delta drop demonstrates the abrupt flicker jump
        expect(midway1 - restart2).toBeGreaterThan(0.05);
    });
});
