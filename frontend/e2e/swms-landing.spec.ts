import { test, expect } from "@playwright/test";

/**
 * SWMS v1.0 — Public Landing Page & Auxiliary Views E2E Test Suite (Dual Track)
 * 
 * Covers Requirements R1–R4 from ORIGINAL_REQUEST.md & PROJECT.md:
 * - Tier 1: Feature Coverage (Landing Page, Hero CTA, Comparison Matrix, About Modal, Shortcuts Guide, Theme Toggle)
 * - Tier 2: Boundary & Responsive Layout (1920x1080, 1536x864, 1280x720 with Zero Horizontal Overflow)
 * - Tier 3: Cross-Feature & State Preservation (Two-Way Routing & Theme Continuity)
 * - Tier 4: Real-World Learner Journey (Continuous Study Loop Onboarding Flow)
 */

const MANDATORY_VIEWPORTS = [
  { name: "1080p-100pct-2xl", width: 1920, height: 1080 },
  { name: "1080p-125pct-xl", width: 1536, height: 864 },
  { name: "1080p-150pct-lg", width: 1280, height: 720 }
];

test.describe("SWMS v1.0 — Landing Page & Auxiliary Views (Tiers 1-4)", () => {

  // =========================================================================
  // TIER 1: FEATURE COVERAGE (OPAQUE-BOX CONTRACT VERIFICATION)
  // =========================================================================

  test("Tier 1: Landing Page renders complete ecosystem sections (Hero, Pillars, Comparison, Features, Footer)", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/");

    // 1. Root & Header Check
    const root = page.getByTestId("landing-page-root").or(page.locator("main")).first();
    await expect(root).toBeVisible();

    const header = page.getByTestId("landing-header").or(page.locator("header")).first();
    await expect(header).toBeVisible();
    await expect(header).toContainText(/SWMS/i);

    // 2. Hero Section Check
    const hero = page.getByTestId("landing-hero-section").first();
    await expect(hero).toBeVisible();
    await expect(hero).toContainText(/Học tập|Workspace|Tri thức|Không gian|Chủ quyền/i);

    // 3. Primary CTA Button
    const heroCta = page
      .getByTestId("landing-hero-cta")
      .or(page.getByRole("link", { name: /bắt đầu học ngay|vào workspace|vào không gian làm việc|khám phá/i }))
      .or(page.getByRole("button", { name: /bắt đầu học ngay|vào workspace|vào không gian làm việc|khám phá/i }))
      .first();
    await expect(heroCta).toBeVisible();

    // 4. Core Pillars Section (Deep Reader 68ch, Split PKM, FSRS v5, Formative Practice)
    const pillars = page.getByTestId("landing-pillars-section").first();
    await expect(pillars).toBeVisible();
    await expect(pillars).toContainText(/68ch|Reader|Đọc sâu/i);
    await expect(pillars).toContainText(/Split|Chia đôi|PKM/i);
    await expect(pillars).toContainText(/FSRS/i);

    // 5. Value Comparison Matrix (SWMS vs Fragmented Tools)
    const comparison = page.getByTestId("landing-comparison-matrix").first();
    await expect(comparison).toBeVisible();
    await expect(comparison).toContainText(/Rời rạc|Notion|Anki|PDF|LMS|So sánh/i);

    // 6. Features Grid Section
    const featuresGrid = page.getByTestId("landing-features-grid").first();
    await expect(featuresGrid).toBeVisible();

    // 7. Landing Footer
    const footer = page.getByTestId("landing-footer").or(page.locator("footer")).first();
    await expect(footer).toBeVisible();
    await expect(footer).toContainText(/SWMS|Studxy/i);
  });

  test("Tier 1: Primary CTA navigates from Landing Page to Workspace and Logo navigates back", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/");

    // 1. Click Primary CTA on Hero
    const heroCta = page
      .getByTestId("landing-hero-cta")
      .or(page.getByRole("link", { name: /bắt đầu học ngay|vào workspace|vào không gian làm việc/i }))
      .first();
    await heroCta.click();

    // 2. Verify seamless transition to /workspace
    await expect(page).toHaveURL(/\/workspace/);
    await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();

    // 3. Click SWMS Brand Logo on TopNav in Workspace to navigate back to /
    const brandLogo = page
      .getByTestId("brand-logo-link")
      .or(page.locator('header a[href="/"]'))
      .or(page.getByRole("link", { name: /SWMS/i }))
      .first();
    await brandLogo.click();

    // 4. Verify return to Landing Page
    await expect(page).toHaveURL(/\/(#.*)?$/);
    const hero = page.getByTestId("landing-hero-section").first();
    await expect(hero).toBeVisible();
  });

  test("Tier 1: About SWMS Modal opens, displays Sovereign Personal Workspace philosophy, and closes cleanly", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/");

    // 1. Trigger About Modal from Landing Header or Footer
    const aboutTrigger = page
      .getByTestId("about-swms-trigger")
      .or(page.getByRole("button", { name: /về swms|triết lý|about/i }))
      .or(page.getByRole("link", { name: /về swms|triết lý|about/i }))
      .first();
    await aboutTrigger.click();

    // 2. Verify About Modal is visible with Sovereign Philosophy content
    const aboutModal = page.getByTestId("about-swms-modal");
    await expect(aboutModal).toBeVisible();
    await expect(aboutModal).toContainText(/Sovereign|Chủ quyền|Cá nhân|Hybrid/i);

    // 3. Close via close button or Escape key
    const closeBtn = page
      .getByTestId("close-about-modal")
      .or(aboutModal.getByRole("button", { name: /đóng|close|✕/i }))
      .first();

    if (await closeBtn.isVisible()) {
      await closeBtn.click();
    } else {
      await page.keyboard.press("Escape");
    }

    await expect(aboutModal).toBeHidden();
  });

  test("Tier 1: Keyboard Shortcuts Guide opens, displays 4 context categories, and closes cleanly", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/");

    // 1. Trigger Keyboard Shortcuts Modal
    const shortcutsTrigger = page
      .getByTestId("keyboard-shortcuts-trigger")
      .or(page.getByRole("button", { name: /phím tắt|shortcuts/i }))
      .or(page.getByRole("link", { name: /phím tắt|shortcuts/i }))
      .first();
    await shortcutsTrigger.click();

    // 2. Verify Shortcuts Modal is visible
    const shortcutsModal = page.getByTestId("keyboard-shortcuts-modal");
    await expect(shortcutsModal).toBeVisible();

    // 3. Verify presence of all 4 key contexts (Navigation, Reading, Split, Review)
    await expect(shortcutsModal).toContainText(/Ctrl\+K|Command Palette|Tìm kiếm/i);
    await expect(shortcutsModal).toContainText(/Đọc|Highlight|Bôi đen|1-5|C/i);
    await expect(shortcutsModal).toContainText(/Split|Chia đôi|Ctrl\\|Jump/i);
    await expect(shortcutsModal).toContainText(/FSRS|Ôn tập|Space|1-4/i);

    // 4. Close via close button or Escape key
    const closeBtn = page
      .getByTestId("close-shortcuts-modal")
      .or(shortcutsModal.getByRole("button", { name: /đóng|close|✕/i }))
      .first();

    if (await closeBtn.isVisible()) {
      await closeBtn.click();
    } else {
      await page.keyboard.press("Escape");
    }

    await expect(shortcutsModal).toBeHidden();
  });

  test("Tier 1: Dark/Light Mode toggle operates and synchronizes on root document", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/");

    const themeToggle = page
      .getByTestId("theme-toggle-btn")
      .or(page.getByRole("button", { name: /chuyển đổi giao diện sáng\/tối|dark mode|light mode|theme/i }))
      .first();
    await expect(themeToggle).toBeVisible();

    // Initial theme check
    const isInitiallyDark = await page.evaluate(() =>
      document.documentElement.classList.contains("dark") ||
      document.body.classList.contains("dark")
    );

    // Toggle theme
    await themeToggle.click();

    // Verify root class toggled
    const isDarkAfterToggle = await page.evaluate(() =>
      document.documentElement.classList.contains("dark") ||
      document.body.classList.contains("dark")
    );
    expect(isDarkAfterToggle).toBe(!isInitiallyDark);

    // Verify localStorage theme token is set
    const savedTheme = await page.evaluate(() => localStorage.getItem("swms_theme"));
    expect(savedTheme).not.toBeNull();
  });

  // =========================================================================
  // TIER 2: BOUNDARY & MULTI-VIEWPORT RESPONSIVE (ZERO HORIZONTAL OVERFLOW)
  // =========================================================================

  for (const vp of MANDATORY_VIEWPORTS) {
    test(`Tier 2: Responsive Layout & Zero Horizontal Overflow at ${vp.width}x${vp.height} (${vp.name})`, async ({
      page
    }) => {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.goto("/");

      // Verify essential sections render
      await expect(page.getByTestId("landing-hero-section").first()).toBeVisible();
      await expect(page.getByTestId("landing-comparison-matrix").first()).toBeVisible();

      // Strict Zero Horizontal Overflow Verification across the entire page
      const hasHorizontalOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(hasHorizontalOverflow).toBe(false);

      // Verify comparison table does not exceed viewport width
      const matrixOverflow = await page.evaluate(() => {
        const el = document.querySelector('[data-testid="landing-comparison-matrix"]');
        if (!el) return false;
        const rect = el.getBoundingClientRect();
        return rect.right > window.innerWidth + 1;
      });
      expect(matrixOverflow).toBe(false);

      // Verify Modal responsiveness at this viewport
      const shortcutsTrigger = page
        .getByTestId("keyboard-shortcuts-trigger")
        .or(page.getByRole("button", { name: /phím tắt|shortcuts/i }))
        .first();

      if (await shortcutsTrigger.isVisible()) {
        await shortcutsTrigger.click();
        const shortcutsModal = page.getByTestId("keyboard-shortcuts-modal");
        await expect(shortcutsModal).toBeVisible();

        const modalOverflow = await page.evaluate(
          () => document.documentElement.scrollWidth > window.innerWidth
        );
        expect(modalOverflow).toBe(false);

        await page.keyboard.press("Escape");
        await expect(shortcutsModal).toBeHidden();
      }
    });
  }

  // =========================================================================
  // TIER 3: CROSS-FEATURE & STATE PRESERVATION
  // =========================================================================

  test("Tier 3: Theme Continuity & State Preservation during Navigation (/ -> /workspace -> /)", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/");

    // 1. Force Dark Mode on Landing Page
    const themeToggle = page
      .getByTestId("theme-toggle-btn")
      .or(page.getByRole("button", { name: /chuyển đổi giao diện sáng\/tối|dark mode|light mode|theme/i }))
      .first();

    const isDark = await page.evaluate(() =>
      document.documentElement.classList.contains("dark") ||
      document.body.classList.contains("dark")
    );
    if (!isDark) {
      await themeToggle.click();
    }

    // Verify Dark Mode is active on Landing Page
    const isDarkOnLanding = await page.evaluate(() =>
      document.documentElement.classList.contains("dark") ||
      document.body.classList.contains("dark")
    );
    expect(isDarkOnLanding).toBe(true);

    // 2. Navigate to /workspace via Primary CTA
    const heroCta = page
      .getByTestId("landing-hero-cta")
      .or(page.getByRole("link", { name: /bắt đầu học ngay|vào workspace|vào không gian làm việc/i }))
      .first();
    await heroCta.click();
    await expect(page).toHaveURL(/\/workspace/);

    // 3. Verify Dark Mode persists inside /workspace
    const isDarkInWorkspace = await page.evaluate(() =>
      document.documentElement.classList.contains("dark") ||
      document.body.classList.contains("dark") ||
      document.querySelector(".dark") !== null
    );
    expect(isDarkInWorkspace).toBe(true);

    // 4. Return to Landing Page via Brand Logo
    const brandLogo = page
      .getByTestId("brand-logo-link")
      .or(page.locator('header a[href="/"]'))
      .or(page.getByRole("link", { name: /SWMS/i }))
      .first();
    await brandLogo.click();
    await expect(page).toHaveURL(/\/(#.*)?$/);

    // 5. Verify Dark Mode remains active on Landing Page
    const isDarkAfterReturn = await page.evaluate(() =>
      document.documentElement.classList.contains("dark") ||
      document.body.classList.contains("dark")
    );
    expect(isDarkAfterReturn).toBe(true);

    // 6. Verify zero open dialog/modal leak
    await expect(page.getByTestId("about-swms-modal")).toBeHidden();
    await expect(page.getByTestId("keyboard-shortcuts-modal")).toBeHidden();
  });

  // =========================================================================
  // TIER 4: REAL-WORLD LEARNER JOURNEY (FULL ONBOARDING WORKFLOW)
  // =========================================================================

  test("Tier 4: Complete Learner Journey (Landing -> Value Discovery -> Workspace -> Reading -> Shortcuts -> Return)", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });

    // Step 1: Learner lands on public homepage
    await page.goto("/");
    await expect(page.getByTestId("landing-hero-section").first()).toBeVisible();

    // Step 2: Learner reads value proposition and inspects comparison table
    const comparison = page.getByTestId("landing-comparison-matrix").first();
    await comparison.scrollIntoViewIfNeeded();
    await expect(comparison).toBeVisible();

    // Step 3: Learner opens About modal to understand Sovereign Workspace philosophy
    const aboutTrigger = page
      .getByTestId("about-swms-trigger")
      .or(page.getByRole("button", { name: /về swms|triết lý|about/i }))
      .first();
    if (await aboutTrigger.isVisible()) {
      await aboutTrigger.click();
      const aboutModal = page.getByTestId("about-swms-modal");
      await expect(aboutModal).toBeVisible();
      await page.keyboard.press("Escape");
      await expect(aboutModal).toBeHidden();
    }

    // Step 4: Learner clicks Primary CTA to enter personal workspace
    const heroCta = page
      .getByTestId("landing-hero-cta")
      .or(page.getByRole("link", { name: /bắt đầu học ngay|vào workspace|vào không gian làm việc/i }))
      .first();
    await heroCta.click();
    await expect(page).toHaveURL(/\/workspace/);

    // Step 5: In Workspace Dashboard, learner opens Mode 1 Reading Mode
    await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
    await expect(page.getByTestId("reading-prose-column")).toBeVisible();

    // Step 6: In Workspace, learner opens Keyboard Shortcuts guide to learn hotkeys
    const shortcutsBtn = page
      .getByTestId("keyboard-shortcuts-trigger")
      .or(page.getByTestId("help-shortcuts-btn"))
      .or(page.getByRole("button", { name: /phím tắt|shortcuts|\?/i }))
      .first();

    if (await shortcutsBtn.isVisible()) {
      await shortcutsBtn.click();
      const shortcutsModal = page.getByTestId("keyboard-shortcuts-modal");
      await expect(shortcutsModal).toBeVisible();
      await page.keyboard.press("Escape");
      await expect(shortcutsModal).toBeHidden();
    }

    // Step 7: Learner returns to Landing page via Brand Logo link
    const brandLogo = page
      .getByTestId("brand-logo-link")
      .or(page.locator('header a[href="/"]'))
      .or(page.getByRole("link", { name: /SWMS/i }))
      .first();
    await brandLogo.click();
    await expect(page).toHaveURL(/\/(#.*)?$/);
    await expect(page.getByTestId("landing-hero-section").first()).toBeVisible();

    // Step 8: Verify zero page horizontal overflow upon return
    const finalOverflow = await page.evaluate(
      () => document.documentElement.scrollWidth > window.innerWidth
    );
    expect(finalOverflow).toBe(false);
  });
});
