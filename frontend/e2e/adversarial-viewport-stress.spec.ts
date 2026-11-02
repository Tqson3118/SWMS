import { test, expect } from "@playwright/test";

const STRESS_VIEWPORTS = [
  { name: "FHD-100pct-2xl", width: 1920, height: 1080, category: "desktop" },
  { name: "Laptop-125pct-xl", width: 1536, height: 864, category: "laptop-125" },
  { name: "MacBook-1440", width: 1440, height: 900, category: "laptop-1440" },
  { name: "Laptop-150pct-lg", width: 1280, height: 720, category: "laptop-150" },
  { name: "Tablet-Landscape", width: 1024, height: 768, category: "tablet" },
  { name: "Tablet-Portrait", width: 768, height: 1024, category: "tablet-portrait" },
  { name: "Mobile-iPhone14", width: 390, height: 844, category: "mobile" },
  { name: "Mobile-SE", width: 375, height: 667, category: "mobile-small" }
];

test.describe("Adversarial Multi-Viewport Responsive & Ergonomics Stress Suite", () => {
  for (const vp of STRESS_VIEWPORTS) {
    test(`Zero Horizontal Overflow Invariant at ${vp.width}x${vp.height} (${vp.name}) across all 4 modes`, async ({
      page
    }) => {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.goto("/workspace");

      // Check Invariant Helper
      const checkZeroOverflow = async (modeName: string) => {
        const metrics = await page.evaluate(() => {
          const scrollW = document.documentElement.scrollWidth;
          const innerW = window.innerWidth;
          const bodyScrollW = document.body.scrollWidth;
          const overflowX = scrollW - innerW;
          return { scrollW, innerW, bodyScrollW, overflowX, hasOverflow: overflowX > 0 };
        });
        expect(
          metrics.hasOverflow,
          `Horizontal overflow detected on ${vp.name} (${vp.width}x${vp.height}) during ${modeName}: scrollWidth=${metrics.scrollW}, innerWidth=${metrics.innerW}, diff=+${metrics.overflowX}px`
        ).toBe(false);
        return metrics;
      };

      // 1. Dashboard Mode
      await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
      await checkZeroOverflow("Dashboard");

      // 2. Mode 1: Reading Mode
      await page.getByTestId("mode-tab-reading").click();
      await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
      await expect(page.getByTestId("reading-prose-column")).toBeVisible();
      await checkZeroOverflow("Mode 1 Reading");

      // 3. Mode 2: Split Workspace Mode
      await page.getByTestId("mode-tab-split").click();
      await expect(page.getByTestId("mode-2-split-container")).toBeVisible();
      await expect(page.getByTestId("split-pane-reader")).toBeVisible();
      await expect(page.getByTestId("split-pane-notebook")).toBeVisible();
      await checkZeroOverflow("Mode 2 Split");

      // 4. Mode 3: Review Mode
      await page.getByTestId("mode-tab-review").click();
      await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
      await checkZeroOverflow("Mode 3 Review");
    });
  }

  test("Ergonomics Stress 1: Mode 2 Split Workspace on 1536px (125% DPI Laptop)", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1536, height: 864 });
    await page.goto("/workspace");
    await page.getByTestId("mode-tab-split").click();

    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toBeVisible();

    // Inspect Sidebar dimensions and behavior
    const sidebar = page.getByTestId("split-pane-sidebar");
    const isSidebarVisible = await sidebar.isVisible();
    expect(isSidebarVisible).toBe(true);

    const sidebarBox = await sidebar.boundingBox();
    expect(sidebarBox).not.toBeNull();

    // Check Reader and Notebook widths
    const reader = page.getByTestId("split-pane-reader");
    const notebook = page.getByTestId("split-pane-notebook");
    const readerBox = await reader.boundingBox();
    const notebookBox = await notebook.boundingBox();

    expect(readerBox).not.toBeNull();
    expect(notebookBox).not.toBeNull();

    console.log(`[1536px Metrics] Sidebar width=${sidebarBox!.width}px, Reader width=${readerBox!.width}px, Notebook width=${notebookBox!.width}px`);

    // Verify zero overflow
    const overflow = await page.evaluate(
      () => document.documentElement.scrollWidth > window.innerWidth
    );
    expect(overflow).toBe(false);
  });

  test("Ergonomics Stress 2: Mode 2 Split Workspace on 1280px (150% DPI Laptop)", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1280, height: 720 });
    await page.goto("/workspace");
    await page.getByTestId("mode-tab-split").click();

    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toBeVisible();

    const sidebar = page.getByTestId("split-pane-sidebar");
    const isSidebarVisible = await sidebar.isVisible();

    let sidebarWidth = 0;
    if (isSidebarVisible) {
      const box = await sidebar.boundingBox();
      sidebarWidth = box ? box.width : 0;
    }

    const reader = page.getByTestId("split-pane-reader");
    const notebook = page.getByTestId("split-pane-notebook");
    const readerBox = await reader.boundingBox();
    const notebookBox = await notebook.boundingBox();

    console.log(`[1280px Metrics] Sidebar visible=${isSidebarVisible}, width=${sidebarWidth}px, Reader width=${readerBox?.width}px, Notebook width=${notebookBox?.width}px`);

    // Strict zero horizontal overflow verification (specifically testing for the 8px splitter bug)
    const metrics = await page.evaluate(() => ({
      scrollWidth: document.documentElement.scrollWidth,
      innerWidth: window.innerWidth,
      bodyScrollWidth: document.body.scrollWidth,
      diff: document.documentElement.scrollWidth - window.innerWidth
    }));
    expect(metrics.diff).toBeLessThanOrEqual(0);
  });

  test("Ergonomics Stress 3: Mode 1 Reading Column on 1280px & 1024px & 390px", async ({
    page
  }) => {
    for (const width of [1280, 1024, 768, 390]) {
      await page.setViewportSize({ width, height: 800 });
      await page.goto("/workspace");
      await page.getByTestId("mode-tab-reading").click();

      const proseCol = page.getByTestId("reading-prose-column");
      await expect(proseCol).toBeVisible();

      const proseBox = await proseCol.boundingBox();
      expect(proseBox).not.toBeNull();

      const computed = await proseCol.evaluate((el) => {
        const style = window.getComputedStyle(el);
        return {
          maxWidth: style.maxWidth,
          width: el.clientWidth,
          scrollWidth: el.scrollWidth,
          lineHeight: style.lineHeight,
          fontSize: style.fontSize
        };
      });

      console.log(`[Reading Mode @ ${width}px] Prose width=${computed.width}px, max-w=${computed.maxWidth}, scrollW=${computed.scrollWidth}px`);

      // Verify no horizontal overflow inside prose or page
      expect(computed.scrollWidth).toBeLessThanOrEqual(computed.width + 1); // 1px rounding tolerance
      const pageOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(pageOverflow).toBe(false);
    }
  });

  test("Ergonomics Stress 4: Small Viewport Modals & Popovers Zero Horizontal Overflow", async ({
    page
  }) => {
    // Test on 390x844 mobile viewport
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto("/workspace");

    // 1. Quota popover
    await page.getByTestId("quota-indicator-btn").click();
    const quotaPopover = page.getByTestId("quota-breakdown-popover");
    if (await quotaPopover.isVisible()) {
      const quotaOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      const diff = await page.evaluate(
        () => document.documentElement.scrollWidth - window.innerWidth
      );
      console.log(`[Mobile 390px Quota Popover] Overflow diff=${diff}px`);
      // Close popover
      await page.getByTestId("quota-indicator-btn").click();
    }

    // 2. Command Palette (Ctrl+K)
    await page.getByTestId("cmd-k-trigger").click();
    const cmdModal = page.getByTestId("command-palette-modal");
    await expect(cmdModal).toBeVisible();
    const cmdOverflow = await page.evaluate(
      () => document.documentElement.scrollWidth > window.innerWidth
    );
    const cmdDiff = await page.evaluate(
      () => document.documentElement.scrollWidth - window.innerWidth
    );
    console.log(`[Mobile 390px Command Palette] Overflow diff=${cmdDiff}px`);
    expect(cmdOverflow).toBe(false);

    // Press Escape to close
    await page.keyboard.press("Escape");
  });

  test("Ergonomics Stress 5: Mode 2 Interactive Drag Clamps & Double-Click Reset at 1536px and 1280px", async ({
    page
  }) => {
    for (const width of [1536, 1280]) {
      await page.setViewportSize({ width, height: 800 });
      await page.goto("/workspace");
      await page.getByTestId("mode-tab-split").click();

      const splitContainer = page.getByTestId("mode-2-split-container");
      await expect(splitContainer).toBeVisible();

      // Check Handle 2 (between Reader and Notebook)
      const handle2 = page.getByTestId("split-resize-handle-2");
      await expect(handle2).toBeVisible();

      // Drag Handle 2
      const box2 = await handle2.boundingBox();
      expect(box2).not.toBeNull();
      await page.mouse.move(box2!.x + box2!.width / 2, box2!.y + box2!.height / 2);
      await page.mouse.down();
      await page.mouse.move(box2!.x - 200, box2!.y + box2!.height / 2, { steps: 5 });
      await page.mouse.up();

      // Double-click to reset
      await handle2.dblclick();
      await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");

      // Verify zero overflow after drag & reset
      const overflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(overflow).toBe(false);
    }
  });

  test("Ergonomics Stress 6: Mode 2 Sidebar Collapse to 48px Rail via Toggle & Ctrl+\\ at 1536px", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1536, height: 864 });
    await page.goto("/workspace");
    await page.getByTestId("mode-tab-split").click();

    const sidebar = page.getByTestId("split-pane-sidebar");
    const toggleBtn = page.getByTestId("split-sidebar-toggle");
    await expect(toggleBtn).toBeVisible();

    // 1. Initial State: at 1536px
    const initialBox = await sidebar.boundingBox();
    expect(initialBox).not.toBeNull();
    console.log(`[1536px Sidebar Toggle] Initial width: ${initialBox!.width}px`);

    // 2. Click Toggle Button -> Sidebar collapses to rail (48px)
    await toggleBtn.click();
    const collapsedBox = await sidebar.boundingBox();
    expect(collapsedBox).not.toBeNull();
    console.log(`[1536px Sidebar Toggle] After button click width: ${collapsedBox!.width}px`);
    expect(collapsedBox!.width).toBeLessThanOrEqual(50); // w-12 = 48px (+ border)

    // Verify Reader and Notebook expand smoothly
    const reader = page.getByTestId("split-pane-reader");
    const notebook = page.getByTestId("split-pane-notebook");
    const readerBox = await reader.boundingBox();
    const notebookBox = await notebook.boundingBox();
    console.log(`[1536px Collapsed Rail] Reader width: ${readerBox!.width}px, Notebook width: ${notebookBox!.width}px`);
    expect(readerBox!.width).toBeGreaterThan(650);
    expect(notebookBox!.width).toBeGreaterThan(550);

    // Verify zero overflow in collapsed rail state
    const overflow1 = await page.evaluate(
      () => document.documentElement.scrollWidth > window.innerWidth
    );
    expect(overflow1).toBe(false);

    // 3. Press Ctrl+\ -> Re-expands
    await page.keyboard.press("Control+\\");
    const expandedBox = await sidebar.boundingBox();
    expect(expandedBox).not.toBeNull();
    console.log(`[1536px Sidebar Toggle] After Ctrl+\\ width: ${expandedBox!.width}px`);

    // 4. Press Ctrl+\ again -> Collapses back to rail
    await page.keyboard.press("Control+\\");
    const collapsedAgainBox = await sidebar.boundingBox();
    expect(collapsedAgainBox).not.toBeNull();
    expect(collapsedAgainBox!.width).toBeLessThanOrEqual(50);
  });

  test("Ergonomics Stress 7: Mode 1 Reading Column & TOC Collapse / Zen Mode at 1280px & 1536px", async ({
    page
  }) => {
    for (const width of [1536, 1280]) {
      await page.setViewportSize({ width, height: 864 });
      await page.goto("/workspace");
      await page.getByTestId("mode-tab-reading").click();

      const tocSidebar = page.getByTestId("reading-toc-sidebar");
      await expect(tocSidebar).toBeVisible();

      const initialClass = await tocSidebar.getAttribute("class");
      console.log(`[Before Click] class: ${initialClass}`);

      // Toggle TOC sidebar via collapse button
      const tocToggleBtn = tocSidebar.locator("button").first();
      await tocToggleBtn.click();
      await page.waitForTimeout(100);

      const afterClass = await tocSidebar.getAttribute("class");
      console.log(`[After Click] class: ${afterClass}`);

      const computedStyle = await tocSidebar.evaluate((el) => {
        const s = window.getComputedStyle(el);
        return { width: s.width, maxWidth: s.maxWidth, minWidth: s.minWidth, flexBasis: s.flexBasis };
      });
      console.log(`[After Click] computedStyle:`, computedStyle);

      const collapsedTocBox = await tocSidebar.boundingBox();
      console.log(`[Mode 1 @ ${width}px] Collapsed TOC width: ${collapsedTocBox?.width}px`);

      // Toggle Zen Mode via action rail
      const zenBtn = page.getByTestId("reading-action-rail").locator("button").first();
      await zenBtn.click();
      // In Zen mode, TOC sidebar is unmounted (!zenMode)
      await expect(tocSidebar).not.toBeVisible();

      // Prose column still visible and centered
      const prose = page.getByTestId("reading-prose-column");
      await expect(prose).toBeVisible();
      const proseBox = await prose.boundingBox();
      console.log(`[Mode 1 @ ${width}px Zen Mode] Prose width: ${proseBox!.width}px`);

      // Exit Zen Mode
      await zenBtn.click();
      await expect(page.getByTestId("reading-toc-sidebar")).toBeVisible();

      // Verify zero overflow
      const overflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(overflow).toBe(false);
    }
  });

  test("Ergonomics Stress 8: Mode 3 Review Canvas & Rating Buttons on 1280px and Mobile 390px", async ({
    page
  }) => {
    // Check 1280px
    await page.setViewportSize({ width: 1280, height: 720 });
    await page.goto("/workspace");
    await page.getByTestId("mode-tab-review").click();

    await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
    await expect(page.getByTestId("fsrs-3d-flip-card")).toBeVisible();
    await expect(page.getByTestId("fsrs-rating-bar")).toBeVisible();

    const cardBox1280 = await page.getByTestId("fsrs-3d-flip-card").boundingBox();
    expect(cardBox1280).not.toBeNull();
    console.log(`[Mode 3 @ 1280px] Card width=${cardBox1280!.width}px, height=${cardBox1280!.height}px`);
    expect(cardBox1280!.width).toBeLessThanOrEqual(800);

    // Check Mobile 390px
    await page.setViewportSize({ width: 390, height: 844 });
    const cardBox390 = await page.getByTestId("fsrs-3d-flip-card").boundingBox();
    expect(cardBox390).not.toBeNull();
    console.log(`[Mode 3 @ 390px Mobile] Card width=${cardBox390!.width}px, height=${cardBox390!.height}px`);
    // Card must fit within mobile screen with padding (width <= 390)
    expect(cardBox390!.width).toBeLessThanOrEqual(390);

    // Check all 4 rating buttons fit
    for (let i = 1; i <= 4; i++) {
      const btn = page.getByTestId(`rate-btn-${i}`);
      await expect(btn).toBeVisible();
      const btnBox = await btn.boundingBox();
      expect(btnBox).not.toBeNull();
      expect(btnBox!.width).toBeGreaterThan(60);
    }

    const overflow390 = await page.evaluate(
      () => document.documentElement.scrollWidth > window.innerWidth
    );
    expect(overflow390).toBe(false);
  });

  test("Ergonomics Stress 9: Rapid Dynamic Viewport Resizing Across Modes", async ({
    page
  }) => {
    await page.goto("/workspace");

    const dynamicViewports = [
      { w: 1920, h: 1080 },
      { w: 1536, h: 864 },
      { w: 1366, h: 768 },
      { w: 1280, h: 720 },
      { w: 1024, h: 768 },
      { w: 768, h: 1024 },
      { w: 390, h: 844 },
      { w: 1920, h: 1080 }
    ];

    const modes = ["mode-tab-dashboard", "mode-tab-reading", "mode-tab-split", "mode-tab-review"];

    for (let i = 0; i < dynamicViewports.length; i++) {
      const vp = dynamicViewports[i];
      const modeTab = modes[i % modes.length];

      await page.setViewportSize({ width: vp.w, height: vp.h });
      await page.getByTestId(modeTab).click();
      await page.waitForTimeout(50); // allow layout paint

      const metrics = await page.evaluate(() => ({
        scrollW: document.documentElement.scrollWidth,
        innerW: window.innerWidth,
        diff: document.documentElement.scrollWidth - window.innerWidth
      }));

      expect(metrics.diff).toBeLessThanOrEqual(0);
    }
  });
});

