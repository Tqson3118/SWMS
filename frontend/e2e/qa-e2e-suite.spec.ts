import { test, expect } from "@playwright/test";
import path from "node:path";
import fs from "node:fs";

const SCREENSHOTS_DIR = path.resolve(__dirname, "../../test-results/qa-report/screenshots");

async function takeScreenshot(pageOrLocator: any, filename: string, options?: any) {
  if (!fs.existsSync(SCREENSHOTS_DIR)) {
    fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });
  }
  const targetPath = path.join(SCREENSHOTS_DIR, filename);
  await pageOrLocator.screenshot({ path: targetPath, ...options });
  console.log(`[EVIDENCE CAPTURED] ${filename}`);
}

test.describe("SWMS v1.0 — Comprehensive End-to-End User Flow & Defect QA Suite", () => {
  test.beforeAll(() => {
    if (!fs.existsSync(SCREENSHOTS_DIR)) {
      fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });
    }
  });

  // ==========================================================================
  // FLOW 1: Landing Page, Navigation, Modals & Theme Switching
  // ==========================================================================
  test("Flow 1: Landing Page, 9 Sections, About & Shortcuts Modals, Theme Switching", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/");

    // 1. Landing Hero
    await expect(page.getByTestId("landing-page-root")).toBeVisible();
    await expect(page.getByTestId("landing-hero-section")).toBeVisible();
    await takeScreenshot(page, "FLOW01_01_landing_hero_1080p.png");

    // 2. Comparison Matrix Section
    const compSection = page.locator("#comparison");
    await compSection.scrollIntoViewIfNeeded();
    await expect(compSection).toBeVisible();
    await takeScreenshot(page, "FLOW01_02_landing_comparison_1080p.png");

    // 3. Open About SWMS Modal (5 tabs)
    await page.getByTestId("about-swms-trigger").first().click();
    const aboutModal = page.getByTestId("about-swms-modal");
    await expect(aboutModal).toBeVisible();
    await takeScreenshot(page, "FLOW01_03_about_swms_modal.png");

    // Switch tabs inside About modal
    const hybridTab = aboutModal.locator('button:has-text("Mô hình Hybrid Persona")');
    if (await hybridTab.isVisible()) {
      await hybridTab.click();
      await page.waitForTimeout(200);
    }
    await page.getByTestId("close-about-modal").first().click();
    await expect(aboutModal).toBeHidden();

    // 4. Open Keyboard Shortcuts Modal
    await page.getByTestId("keyboard-shortcuts-trigger").first().click();
    const shortcutsModal = page.getByTestId("keyboard-shortcuts-modal");
    await expect(shortcutsModal).toBeVisible();
    await takeScreenshot(page, "FLOW01_04_keyboard_shortcuts_modal.png");
    await page.getByTestId("close-shortcuts-modal").first().click();
    await expect(shortcutsModal).toBeHidden();

    // 5. Dark Mode Toggle
    await page.getByTestId("theme-toggle-btn").first().click();
    await expect(page.locator("html")).toHaveClass(/dark/);
    await takeScreenshot(page, "FLOW01_05_landing_dark_mode.png");

    // Toggle back to light
    await page.getByTestId("theme-toggle-btn").first().click();
    await expect(page.locator("html")).not.toHaveClass(/dark/);

    // 6. Navigation to Workspace
    await page.getByTestId("landing-nav-cta").click();
    await expect(page).toHaveURL(/.*workspace/);
  });

  // ==========================================================================
  // FLOW 2: Authentication, Sessions & Token Rotation Verification
  // ==========================================================================
  test("Flow 2: Authentication, Sessions & Token Rotation (RTR) API & UI Verification", async ({
    page,
    request
  }) => {
    // 1. Direct Backend Auth Login with Seed Student
    const loginRes = await request.post("/api/v1/auth/login", {
      data: {
        email: "son.tran@studxy.vn",
        password: "Studxy@2026"
      }
    });
    expect(loginRes.status()).toBe(200);
    const loginData = await loginRes.json();
    expect(loginData.user.email).toBe("son.tran@studxy.vn");
    expect(loginData.user.fullName).toBe("Sơn Trần");
    expect(loginData.accessToken).toBeDefined();

    // 2. Direct Auth Me Check
    const meRes = await request.get("/api/v1/auth/me", {
      headers: { Authorization: `Bearer ${loginData.accessToken}` }
    });
    expect(meRes.status()).toBe(200);
    const meData = await meRes.json();
    expect(meData.streakDays).toBe(7);

    // 3. Register Duplicate Validation Check (409 Conflict)
    const dupRegRes = await request.post("/api/v1/auth/register", {
      data: {
        email: "son.tran@studxy.vn",
        password: "Password@123",
        fullName: "Fake Son Tran"
      }
    });
    expect(dupRegRes.status()).toBe(409);

    // 4. Register New Student Check
    const uniqueEmail = `test.student.${Date.now()}@studxy.vn`;
    const regRes = await request.post("/api/v1/auth/register", {
      data: {
        email: uniqueEmail,
        password: "SecurePassword@2026",
        fullName: "Học viên Kiểm thử"
      }
    });
    expect(regRes.status()).toBe(200);
    const regData = await regRes.json();
    expect(regData.user.email).toBe(uniqueEmail);
    expect(regData.personalWorkspaceCourseId).toBeDefined();

    // 5. Navigate to Workspace UI & verify session presence
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");
    await expect(page.getByTestId("global-top-header")).toBeVisible();
    await expect(page.getByTestId("streak-badge")).toContainText("7d");
    await takeScreenshot(page, "FLOW02_auth_session_verified.png");
  });

  // ==========================================================================
  // FLOW 3: Mode 0 Dashboard & Resource Management
  // ==========================================================================
  test("Flow 3: Mode 0 Personal Workspace Dashboard, Onboarding 3-Step, Quota & Upload", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // 1. Dashboard Overview
    await page.getByTestId("mode-tab-dashboard").click();
    await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
    await expect(page.getByTestId("onboarding-3step-widget")).toBeVisible();
    await takeScreenshot(page, "FLOW03_01_dashboard_onboarding_overview.png");

    // 2. Upload Document Modal
    await page.getByTestId("open-upload-pdf-btn").click();
    await expect(page.getByTestId("upload-document-modal")).toBeVisible();
    await takeScreenshot(page, "FLOW03_02_upload_document_modal.png");
    await page.locator('button:has-text("Hủy")').click();
    await expect(page.getByTestId("upload-document-modal")).toBeHidden();

    // 3. Quota Breakdown Popover
    await page.getByTestId("quota-indicator-btn").click();
    const quotaPopover = page.getByTestId("quota-breakdown-popover");
    await expect(quotaPopover).toBeVisible();
    await expect(quotaPopover).toContainText("Hạn mức tài nguyên (Gói Miễn phí)");
    await takeScreenshot(page, "FLOW03_03_quota_breakdown_popover.png");
    await page.getByTestId("quota-indicator-btn").click();
    await expect(quotaPopover).toBeHidden();
  });

  // ==========================================================================
  // FLOW 4: Mode 1 Deep Reading Surface & Semantic Highlights
  // ==========================================================================
  test("Flow 4: Mode 1 Reading Surface (68ch), 5-Color Highlights, Cloze Popover & Ghostreader AI", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // 1. Switch to Mode 1 Reading
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
    await expect(page.getByTestId("reading-prose-column")).toBeVisible();
    await takeScreenshot(page, "FLOW04_01_reading_surface_68ch.png");

    // 2. 5-Color Semantic Highlight Palette
    await expect(page.getByTestId("semantic-selection-toolbar")).toBeVisible();
    await page.locator('[data-testid="reading-prose-column"] p').first().click();
    await page.getByTestId("highlight-btn-yellow").click();
    await expect(page.getByTestId("workspace-toast")).toContainText("Đã lưu Highlight");
    await takeScreenshot(page, "FLOW04_02_semantic_highlight_active.png");

    // 3. Open Instant Cloze Popover
    await page.locator('[data-testid="reading-prose-column"] p').first().click();
    await page.getByTestId("to-flashcard-popover-trigger").click();
    const draftPopover = page.getByTestId("draft-flashcard-popover");
    await expect(draftPopover).toBeVisible();
    await expect(draftPopover).toContainText("Cloze tức thì");
    await takeScreenshot(page, "FLOW04_03_instant_cloze_popover.png");

    // 4. Ghostreader AI explanation trigger & popover
    await page.keyboard.press("Escape");
    await page.getByTestId("ai-explain-trigger").click();
    const explainPopover = page.getByTestId("ai-explanation-popover");
    await expect(explainPopover).toBeVisible();
    await takeScreenshot(page, "FLOW04_04_ghostreader_ai_assistant.png");

    // Close explain popover
    await page.keyboard.press("Escape");
    await page.waitForTimeout(200);
  });

  // ==========================================================================
  // FLOW 5: Mode 2 Split PKM Workspace & Bi-Directional Linkage
  // ==========================================================================
  test("Flow 5: Mode 2 Split PKM (15/45/40), Scroll-Sync, Jump to Source Glow, Orphan Reconciliation, 409 Keep-Both & Markdown Export", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // 1. Switch to Mode 2 Split PKM
    await page.getByTestId("mode-tab-split").click();
    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toBeVisible();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");
    await takeScreenshot(page, "FLOW05_01_split_pkm_workspace_15_45_40.png");

    // 2. Jump to Source with GSAP Glow Pulse
    const jumpBtn = page.getByTestId("jump-to-source-btn").first();
    if (await jumpBtn.isVisible()) {
      await jumpBtn.click();
      await page.waitForTimeout(400);
      await takeScreenshot(page, "FLOW05_02_jump_to_source_glow.png");
    }

    // 3. Orphan Reconciliation Drawer
    const orphanBanner = page.getByTestId("orphaned-highlight-banner");
    if (await orphanBanner.isVisible()) {
      await page.getByTestId("open-orphaned-drawer-btn").click();
      const orphanDrawer = page.getByTestId("orphaned-reconciliation-drawer");
      await expect(orphanDrawer).toBeVisible();
      await takeScreenshot(page, "FLOW05_03_orphan_reconciliation_drawer.png");
      const reanchorBtn = page.getByTestId("reconcile-reanchor-0");
      if (await reanchorBtn.isVisible()) {
        await reanchorBtn.click();
        await page.waitForTimeout(300);
      }
    }

    // 4. 409 Conflict Keep-Both Simulation
    await page.getByTestId("simulate-409-conflict-btn").click();
    await expect(page.getByTestId("workspace-toast")).toContainText("Bản sao xung đột");
    await takeScreenshot(page, "FLOW05_04_conflict_409_keep_both.png");

    // 5. Obsidian Markdown Export
    await page.getByTestId("export-markdown-btn").click();
    const mdBox = page.getByTestId("exported-markdown-box");
    await expect(mdBox).toBeVisible();
    await expect(mdBox).toContainText("SWMS Sovereign Markdown v1.0");
    await takeScreenshot(page, "FLOW05_05_obsidian_markdown_export.png");
  });

  // ==========================================================================
  // FLOW 6: Mode 3 Review Station & Practice Quiz
  // ==========================================================================
  test("Flow 6: Mode 3 FSRS v5 3D Card Flip, Rating Buttons, Completion Confetti & Practice Quiz AI Rubric", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // 1. Switch to Mode 3 Review
    await page.getByTestId("mode-tab-review").click();
    await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
    const flipCard = page.getByTestId("fsrs-3d-flip-card");
    if (await flipCard.isVisible()) {
      await takeScreenshot(page, "FLOW06_01_fsrs_review_card_front.png");

      // 2. Flip Card to Back
      await flipCard.click();
      await expect(page.getByTestId("flashcard-back-face")).toBeVisible();
      await takeScreenshot(page, "FLOW06_02_fsrs_review_card_back.png");

      // 3. Rate cards until completion
      await page.getByTestId("rate-btn-3").click(); // Rate Good
      await page.waitForTimeout(300);

      for (let i = 0; i < 4; i++) {
        if (await page.getByTestId("fsrs-3d-flip-card").isVisible()) {
          await page.getByTestId("fsrs-3d-flip-card").click();
          await page.waitForTimeout(200);
          await page.getByTestId("rate-btn-3").click();
          await page.waitForTimeout(300);
        }
      }
    }

    // Completion Celebration or Complete Card
    const completeCard = page.getByTestId("fsrs-session-complete-card");
    if (await completeCard.isVisible()) {
      await takeScreenshot(page, "FLOW06_03_fsrs_completion_celebration.png");
    }

    // 4. Practice Quiz Portal
    await page.getByTestId("review-subtab-quiz").click();
    await expect(page.getByTestId("practice-quiz-portal")).toBeVisible();
    await takeScreenshot(page, "FLOW06_04_practice_quiz_portal.png");

    // 5. Submit Essay for AI Rubric Evaluation
    const essayBtn = page.getByTestId("submit-essay-ai-btn");
    await expect(essayBtn).toBeVisible();
    await essayBtn.click();
    await expect(page.getByTestId("essay-ai-rubric-feedback")).toBeVisible({ timeout: 25000 });
    await takeScreenshot(page, "FLOW06_05_ai_rubric_evaluation.png");
  });

  // ==========================================================================
  // DEFECT HUNTING - CATEGORY 1: FUNCTIONAL BUGS
  // ==========================================================================
  test("Defect Hunt: FUNC-01 (BUG-01) Command Palette False Success on API Failure", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // Intercept /api/v1/flashcards with 500 error
    await page.route("**/api/v1/flashcards", async (route) => {
      await route.fulfill({
        status: 500,
        contentType: "application/json",
        body: JSON.stringify({ error: "Internal Database Transaction Failure" })
      });
    });

    // Open Command Palette
    await page.getByTestId("cmd-k-trigger").click();
    await expect(page.getByTestId("command-palette-modal")).toBeVisible();

    // Type text in quick capture
    const quickInput = page.getByTestId("quick-capture-input");
    await quickInput.fill("Test cloze definition with server failure");
    await page.getByTestId("quick-capture-save-btn").click();

    // Fix Verification: Error alert banner appears, preserving user input
    await expect(page.getByTestId("quick-capture-error-alert")).toBeVisible();
    await takeScreenshot(page, "FUNC_01_command_palette_false_success.png");

    await page.unroute("**/api/v1/flashcards");
  });

  test("Defect Hunt: FUNC-02 (BUG-05) Document Upload Silent Failure on Quota Exceeded (413)", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // Mock 413 Payload Too Large / Quota Exceeded for document upload
    await page.route("**/api/v1/documents/upload", async (route) => {
      await route.fulfill({
        status: 413,
        contentType: "application/json",
        body: JSON.stringify({ error: "Storage quota exceeded (50MB free tier limit)" })
      });
    });

    await page.getByTestId("open-upload-pdf-btn").click();
    const modal = page.getByTestId("upload-document-modal");
    await expect(modal).toBeVisible();

    await page.locator('input[type="text"][required]').fill("Heavy Textbook 100MB.pdf");
    await page.locator('button[type="submit"]').click();

    // Observe modal stays open with no user-facing error message or banner
    await page.waitForTimeout(600);
    await expect(modal).toBeVisible();
    await takeScreenshot(page, "FUNC_02_unhandled_upload_error_silence.png");

    await page.unroute("**/api/v1/documents/upload");
  });

  // ==========================================================================
  // DEFECT HUNTING - CATEGORY 2: UX FRICTION
  // ==========================================================================
  test("Defect Hunt: UX-01 (BUG-02) Silent Fallback When Highlighting With No Selection", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

    // Ensure clear selection
    await page.evaluate(() => window.getSelection()?.removeAllRanges());

    // Click "Tạo thẻ ghi nhớ" without selecting text
    await page.getByTestId("to-flashcard-popover-trigger").click();
    // Fix Verification: Toast warns user to select text rather than creating dummy flashcard
    await expect(page.getByTestId("workspace-toast")).toContainText("Vui lòng bôi đen");
    await takeScreenshot(page, "UX_01_silent_fallback_no_selection.png");
  });

  test("Defect Hunt: UX-02 (BUG-03) Quota Popover Lacks Outside-Click Dismissal", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // Open Quota popover
    await page.getByTestId("quota-indicator-btn").click();
    const quotaPopover = page.getByTestId("quota-breakdown-popover");
    await expect(quotaPopover).toBeVisible();

    // Click far outside on workspace reading area
    await page.mouse.click(400, 300);
    await page.waitForTimeout(300);

    // Fix Verification: Popover is now cleanly dismissed on outside click
    await expect(quotaPopover).toBeHidden();
    await takeScreenshot(page, "UX_02_quota_popover_outside_click_stuck.png");
  });

  test("Defect Hunt: UX-03 (BUG-08) Modal Lacks Focus Trap (Tab Escapes to Background)", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/");

    // Open About Modal
    await page.getByTestId("about-swms-trigger").first().click();
    const modal = page.getByTestId("about-swms-modal");
    await expect(modal).toBeVisible();

    // Press Tab multiple times to inspect if focus leaks out of modal
    for (let i = 0; i < 12; i++) {
      await page.keyboard.press("Tab");
    }

    await takeScreenshot(page, "UX_03_modal_focus_escape_leak.png");
  });

  test("Defect Hunt: UX-04 (BUG-09) Practice Quiz Empty / Completion State Guidance Gap", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await page.getByTestId("mode-tab-review").click();
    await page.getByTestId("review-subtab-quiz").click();
    await expect(page.getByTestId("practice-quiz-portal")).toBeVisible();

    // Answer MCQ & Cloze
    const choiceBtn = page.locator('button:has-text("[A]")').first();
    if (await choiceBtn.isVisible()) {
      await choiceBtn.click();
    }
    const clozeInput = page.locator('input[placeholder*="TLB"]');
    if (await clozeInput.isVisible()) {
      await clozeInput.fill("TLB");
      await page.locator('button:has-text("Kiểm tra Cloze")').click();
    }

    await takeScreenshot(page, "UX_04_quiz_empty_guidance_gap.png");
  });

  test("Defect Hunt: UX-05 Avatar Profile Has No Menu or Logout Mechanism", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // Click Avatar button
    await page.getByTestId("user-profile-btn").click();
    // Fix Verification: User profile dropdown appears with details and Logout action
    await expect(page.getByTestId("user-profile-dropdown")).toBeVisible();
    await expect(page.getByTestId("logout-btn")).toBeVisible();
    await takeScreenshot(page, "UX_05_avatar_no_menu_dropdown.png");
  });

  // ==========================================================================
  // DEFECT HUNTING - CATEGORY 3: UI LAYOUT & MISPLACEMENT DEFECTS
  // ==========================================================================
  test("Defect Hunt: LAYOUT-01 (BUG-04) Mobile Viewport (375px/390px) TopNavHeader Squeezing & Overflow", async ({
    page
  }) => {
    // Test on iPhone SE (375x667)
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto("/workspace");

    const header = page.getByTestId("global-top-header");
    await expect(header).toBeVisible();

    await takeScreenshot(page, "LAYOUT_01_mobile_375px_header_jam.png");
  });

  test("Defect Hunt: LAYOUT-02 (BUG-06) Split PKM Workspace 1280px Layout Discrepancy", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1280, height: 720 });
    await page.goto("/workspace");

    await page.getByTestId("mode-tab-split").click();
    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toBeVisible();

    await takeScreenshot(page, "LAYOUT_02_split_workspace_1280px_margins.png");
  });

  test("Defect Hunt: LAYOUT-03 (BUG-10) Semantic Highlight Badge Contrast in Dark Mode", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // Toggle Dark Mode
    await page.getByTestId("theme-toggle-btn").click();
    await expect(page.locator("html")).toHaveClass(/dark/);

    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("semantic-selection-toolbar")).toBeVisible();

    await takeScreenshot(page, "LAYOUT_03_dark_mode_contrast_badge.png");

    // Toggle back
    await page.getByTestId("theme-toggle-btn").click();
  });

  // ==========================================================================
  // DEFECT HUNTING - CATEGORY 4: ANIMATION GLITCHES
  // ==========================================================================
  test("Defect Hunt: ANIM-01 (BUG-07) FSRS 3D Card Abrupt Conditional DOM Swap", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await page.getByTestId("mode-tab-review").click();
    await page.getByTestId("review-subtab-fsrs").click();
    const flipCard = page.getByTestId("fsrs-3d-flip-card");
    if (await flipCard.isVisible()) {
      await flipCard.click();
      await takeScreenshot(page, "ANIM_01_fsrs_abrupt_dom_swap.png");
    } else {
      // If card already completed, reset to see flip
      await page.locator('button:has-text("Ôn tập lại hàng đợi")').click();
      await page.waitForTimeout(200);
      await page.getByTestId("fsrs-3d-flip-card").click();
      await takeScreenshot(page, "ANIM_01_fsrs_abrupt_dom_swap.png");
    }
  });

  test("Defect Hunt: ANIM-02 Mode Transition Rapid Switching Flicker", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // Rapidly switch between Mode 1 and Mode 2
    await page.getByTestId("mode-tab-reading").click();
    await page.waitForTimeout(50);
    await page.getByTestId("mode-tab-split").click();
    await page.waitForTimeout(50);
    await page.getByTestId("mode-tab-reading").click();

    await takeScreenshot(page, "ANIM_02_mode_transition_flicker.png");
  });
});
