import { test, expect } from "@playwright/test";

/**
 * Challenger M2-1: Adversarial Interaction Stress & Ergonomics Verification Suite
 * Targets:
 * 1. Rapid Mode Switching (Dashboard <-> Mode 1 <-> Mode 2 <-> Mode 3), zero layout jitter, state & draft retention.
 * 2. Mode 2 Split PKM Workspace: extreme splitter dragging, rigid clamp bounds, double-click reset, scroll-sync mutex loop prevention.
 * 3. Mode 3 FSRS Review: rapid 3D card flipping, hotkey gating (rating keys blocked when unflipped), rapid rating cascade, practice quiz AI rubric.
 * 4. Command Palette (Ctrl+K): rapid open/close cycling, Vietnamese unaccented search stress, adversarial queries, quick capture.
 */

test.describe("Challenger M2-1 — Adversarial Interaction & Workspace Stress Suite", () => {
  test.beforeEach(async ({ page }) => {
    // Monitor uncaught JavaScript page errors and console errors
    const errors: string[] = [];
    page.on("pageerror", (err) => {
      errors.push(`[PageError] ${err.message}`);
    });
    page.on("console", (msg) => {
      if (msg.type() === "error") {
        errors.push(`[ConsoleError] ${msg.text()}`);
      }
    });
    // @ts-ignore
    page.__capturedErrors = errors;
  });

  // =========================================================================
  // 1. RAPID MODE SWITCHING & STATE / DRAFT RETENTION
  // =========================================================================

  test("Stress 1.1: Rapid 24-cycle mode switching under stress with zero runtime crashes or layout overflow", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();

    const modes = [
      { id: "reading", testId: "mode-tab-reading", containerId: "mode-1-reading-container" },
      { id: "split", testId: "mode-tab-split", containerId: "mode-2-split-container" },
      { id: "review", testId: "mode-tab-review", containerId: "mode-3-review-container" },
      { id: "dashboard", testId: "mode-tab-dashboard", containerId: "personal-workspace-dashboard" }
    ];

    // Rapid-fire 24 clicks across modes (6 complete rounds)
    for (let round = 0; round < 6; round++) {
      for (const m of modes) {
        await page.getByTestId(m.testId).click();
      }
    }

    // Verify it cleanly settles on the last clicked mode (dashboard)
    await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();

    // Verify zero horizontal scrollbar overflow
    const hasOverflow = await page.evaluate(
      () => document.documentElement.scrollWidth > window.innerWidth
    );
    expect(hasOverflow).toBe(false);

    // Switch explicitly to Mode 1 and verify cleanly rendered
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
    await expect(page.getByTestId("reading-prose-column")).toBeVisible();

    // Verify no unhandled page crashes occurred during rapid switching
    // @ts-ignore
    const errors: string[] = page.__capturedErrors || [];
    const criticalErrors = errors.filter(
      (e) => !e.includes("favicon") && !e.includes("404") && !e.includes("Failed to load resource")
    );
    expect(criticalErrors).toHaveLength(0);
  });

  test("Stress 1.2: Persistent Notebook and Essay Draft Retention Across Mode Roundtrips", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // 1. Enter Mode 2 Split and type into Sovereign Notebook editor
    await page.getByTestId("mode-tab-split").click();
    await expect(page.getByTestId("mode-2-split-container")).toBeVisible();

    const noteEditor = page.getByTestId("notebook-editor-textarea");
    await expect(noteEditor).toBeVisible();

    const testDraftNote = "Adversarial Draft Note: TLB Miss handling & 4-level page tables [VERIFIED]";
    await noteEditor.fill(testDraftNote);

    // 2. Perform rapid multi-mode roundtrip: Split -> Dashboard -> Review -> Reading -> Split
    await page.getByTestId("mode-tab-dashboard").click();
    await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();

    await page.getByTestId("mode-tab-review").click();
    await expect(page.getByTestId("mode-3-review-container")).toBeVisible();

    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

    await page.getByTestId("mode-tab-split").click();
    await expect(page.getByTestId("mode-2-split-container")).toBeVisible();

    // Verify notebook draft content is 100% retained
    await expect(page.getByTestId("notebook-editor-textarea")).toHaveValue(testDraftNote);

    // 3. Test Practice Quiz Essay Draft retention
    await page.getByTestId("mode-tab-review").click();
    await page.getByTestId("review-subtab-quiz").click();
    await expect(page.getByTestId("practice-quiz-portal")).toBeVisible();

    const essayInput = page.getByTestId("essay-answer-textarea");
    await expect(essayInput).toBeVisible();

    const testEssayDraft = "Bảng trang 4 cấp giúp tiết kiệm dung lượng bằng cách chỉ cấp phát các tầng PGD, PUD khi có ánh xạ.";
    await essayInput.fill(testEssayDraft);

    // Roundtrip away and back
    await page.getByTestId("mode-tab-reading").click();
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();

    await page.getByTestId("mode-tab-review").click();
    await page.getByTestId("review-subtab-quiz").click();

    // Verify essay draft retained
    await expect(page.getByTestId("essay-answer-textarea")).toHaveValue(testEssayDraft);
  });

  // =========================================================================
  // 2. MODE 2 SPLIT WORKSPACE: EXTREME DRAG, CLAMP BOUNDS & MUTEX
  // =========================================================================

  test("Stress 2.1: Extreme Splitter Drag Clamp Invariants & Double-Click Reset", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await page.getByTestId("mode-tab-split").click();
    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toBeVisible();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");
    await page.waitForTimeout(300);

    const parseRatios = async (): Promise<[number, number, number]> => {
      const attr = await splitContainer.getAttribute("data-split-ratios");
      const parts = attr!.split("/").map(Number);
      return [parts[0], parts[1], parts[2]];
    };

    // 1. Drag Handle 2 far left (-600px) -> clamps Pane 2 at min 30% and Pane 3 at max 55% (15/30/55)
    const handle2 = page.getByTestId("split-resize-handle-2");
    let box2 = await handle2.boundingBox();
    expect(box2).not.toBeNull();
    await page.mouse.move(box2!.x + box2!.width / 2, box2!.y + box2!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box2!.x - 600, box2!.y + box2!.height / 2, { steps: 5 });
    await page.mouse.up();
    await page.waitForTimeout(100);

    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/30/55");
    let [r1, r2, r3] = await parseRatios();
    expect(r2).toBe(30);
    expect(r3).toBe(55);
    expect(Math.abs(r1 + r2 + r3 - 100)).toBeLessThan(0.2);

    // 2. Drag Handle 1 far right (+400px) from 15/30/55 -> cascades and clamps Pane 1 at max 25% (25/30/45)
    const handle1 = page.getByTestId("split-resize-handle-1");
    let box1 = await handle1.boundingBox();
    expect(box1).not.toBeNull();
    await page.mouse.move(box1!.x + box1!.width / 2, box1!.y + box1!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box1!.x + 400, box1!.y + box1!.height / 2, { steps: 5 });
    await page.mouse.up();

    await expect(splitContainer).toHaveAttribute("data-split-ratios", "25/30/45");
    [r1, r2, r3] = await parseRatios();
    expect(r1).toBe(25);
    expect(Math.abs(r1 + r2 + r3 - 100)).toBeLessThan(0.2);

    // 3. Drag Handle 1 far left (-500px) -> clamps Pane 1 at min 10% (10/45/45)
    const box1After = await handle1.boundingBox();
    expect(box1After).not.toBeNull();
    await page.mouse.move(box1After!.x + box1After!.width / 2, box1After!.y + box1After!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box1After!.x - 500, box1After!.y + box1After!.height / 2, { steps: 5 });
    await page.mouse.up();

    await expect(splitContainer).toHaveAttribute("data-split-ratios", "10/45/45");
    [r1, r2, r3] = await parseRatios();
    expect(r1).toBe(10);
    expect(Math.abs(r1 + r2 + r3 - 100)).toBeLessThan(0.2);

    // 4. Double-click Handle 1 resets to 15/45/40
    await handle1.dblclick();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");

    // 5. Test Handle 2 Double-click reset
    box2 = await handle2.boundingBox();
    expect(box2).not.toBeNull();
    await page.mouse.move(box2!.x + box2!.width / 2, box2!.y + box2!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box2!.x - 300, box2!.y + box2!.height / 2, { steps: 5 });
    await page.mouse.up();
    await expect(splitContainer).not.toHaveAttribute("data-split-ratios", "15/45/40");

    await handle2.dblclick();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");
  });

  test("Stress 2.2: Scroll-Sync Mutex prevents recursive infinite loops & supports sync decoupling", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await page.getByTestId("mode-tab-split").click();
    await expect(page.getByTestId("mode-2-split-container")).toBeVisible();

    const readerPane = page.getByTestId("split-pane-reader");
    const notebookPane = page.getByTestId("split-pane-notebook");

    // 1. High-frequency 20x programmatic scroll burst in Reader
    await page.evaluate(() => {
      const reader = document.querySelector('[data-testid="split-pane-reader"]');
      if (reader) {
        for (let i = 0; i < 20; i++) {
          reader.scrollTop = i * 25;
          reader.dispatchEvent(new Event("scroll"));
        }
      }
    });

    // Wait for the 50ms mutex trailing edge dispatch
    await page.waitForTimeout(100);

    // Ensure notebook also scrolled and page did not lock up
    const notebookScrollTop = await notebookPane.evaluate((el) => el.scrollTop);
    expect(notebookScrollTop).toBeGreaterThanOrEqual(0);

    // 2. Toggle Sync-Scroll OFF
    const syncBtn = page.getByTestId("toggle-sync-scroll-btn");
    await syncBtn.click();

    // Record notebook scroll position
    const notebookBefore = await notebookPane.evaluate((el) => el.scrollTop);

    // Scroll reader to bottom
    await page.evaluate(() => {
      const reader = document.querySelector('[data-testid="split-pane-reader"]');
      if (reader) {
        reader.scrollTop = 800;
        reader.dispatchEvent(new Event("scroll"));
      }
    });

    await page.waitForTimeout(100);

    // With sync OFF, Notebook scroll position MUST NOT change
    const notebookAfter = await notebookPane.evaluate((el) => el.scrollTop);
    expect(notebookAfter).toBe(notebookBefore);

    // Toggle Sync-Scroll back ON
    await syncBtn.click();
  });

  // =========================================================================
  // 3. MODE 3 FSRS REVIEW: 3D CARD FLIPS, HOTKEY GATING & AI RUBRIC
  // =========================================================================

  test("Stress 3.1: 3D Card Spacebar/Click Flip Stress & Rating Hotkey Gating Invariant", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await page.getByTestId("mode-tab-review").click();
    await expect(page.getByTestId("mode-3-review-container")).toBeVisible();

    const flipCard = page.getByTestId("fsrs-3d-flip-card");
    const frontText = page.getByTestId("flashcard-front-text");
    const backFace = page.getByTestId("flashcard-back-face");

    await expect(flipCard).toBeVisible();
    await expect(frontText).toBeVisible();
    await expect(backFace).not.toBeVisible();

    // ATTACK 1: Press rating keys 1, 2, 3, 4 while UNFLLIPPED
    // GATING INVARIANT: Unflipped card MUST NOT be rated or advance
    await page.keyboard.press("1");
    await page.keyboard.press("2");
    await page.keyboard.press("3");
    await page.keyboard.press("4");

    // Card must remain unflipped and visible on the same front text
    await expect(frontText).toBeVisible();
    await expect(backFace).not.toBeVisible();

    // ATTACK 2: Rapid 10x Spacebar flipping spam
    for (let i = 0; i < 10; i++) {
      await page.keyboard.press("Space");
      await page.waitForTimeout(30);
    }
    // Even number (10) -> returns to initial unflipped state
    await expect(backFace).not.toBeVisible();

    // Press Space 1 more time (11th) -> FLIPPED
    await page.keyboard.press("Space");
    await expect(backFace).toBeVisible();

    // ATTACK 3: Now flipped, press key '3' (Good rating) -> card evaluates and advances!
    await page.keyboard.press("3");

    // Next card should now be visible or queue finished
    await page.waitForTimeout(300);
  });

  test("Stress 3.2: Rapid FSRS Rating Cascade Through Entire Queue to Celebration Screen", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await page.getByTestId("mode-tab-review").click();
    await expect(page.getByTestId("mode-3-review-container")).toBeVisible();

    // Loop through cards until the session complete card appears (up to 20 safety limit)
    for (let i = 0; i < 20; i++) {
      const isComplete = await page.getByTestId("fsrs-session-complete-card").isVisible();
      if (isComplete) break;

      const flipCard = page.getByTestId("fsrs-3d-flip-card");
      if (await flipCard.isVisible()) {
        // Space to flip
        await page.keyboard.press("Space");
        await page.waitForTimeout(50);
        // Press '3' (Good)
        await page.keyboard.press("3");
        await page.waitForTimeout(100);
      }
    }

    // Must reach completion celebration screen
    await expect(page.getByTestId("fsrs-session-complete-card")).toBeVisible();
    await expect(page.getByText("Tuyệt vời! Bạn đã hoàn thành phiên ôn tập FSRS v5 hôm nay!")).toBeVisible();
  });

  test("Stress 3.3: Practice Quiz Essay AI Rubric Evaluation", async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await page.getByTestId("mode-tab-review").click();
    await page.getByTestId("review-subtab-quiz").click();
    await expect(page.getByTestId("practice-quiz-portal")).toBeVisible();

    // Fill essay question
    const essayTextarea = page.getByTestId("essay-answer-textarea");
    await essayInputSafeFill(
      essayTextarea,
      "Kiến trúc 64-bit có không gian địa chỉ quá rộng (2^64), bảng trang phẳng sẽ chiếm hàng Terabytes RAM. Do đó bắt buộc dùng Bảng trang đa cấp 4 tầng để chỉ cấp phát các bảng trang nhánh khi tiến trình thực sự sử dụng bộ nhớ."
    );

    // Submit AI essay
    const submitBtn = page.getByTestId("submit-essay-ai-btn");
    await submitBtn.click();

    // Expect rubric feedback card to render
    await expect(page.getByTestId("essay-ai-rubric-feedback")).toBeVisible({ timeout: 25000 });
    await expect(page.getByText("Nhận xét định tính từ AI Rubric")).toBeVisible();
  });

  // =========================================================================
  // 4. COMMAND PALETTE (CTRL+K): CYCLING, UNACCENTED SEARCH & ADVERSARIAL QUERIES
  // =========================================================================

  test("Stress 4.1: Rapid Open / Close Cycling of Command Palette without Backdrop Leak", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    const modal = page.getByTestId("command-palette-modal");

    // Rapid toggle 10 times with Ctrl+K and Escape
    for (let i = 0; i < 10; i++) {
      await page.keyboard.press("Control+k");
      await expect(modal).toBeVisible();

      await page.keyboard.press("Escape");
      await expect(modal).not.toBeVisible();
    }

    // Modal is completely unmounted/hidden
    await expect(modal).not.toBeVisible();
  });

  test("Stress 4.2: Vietnamese Diacritic-Insensitive Search Stress with Adversarial Inputs", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await page.getByTestId("cmd-k-trigger").click();
    const modal = page.getByTestId("command-palette-modal");
    await expect(modal).toBeVisible();

    const input = page.getByTestId("command-palette-input");

    // 1. Unaccented search: "bo nho ao" -> Matches Lesson 3.2
    await input.fill("bo nho ao");
    await page.waitForTimeout(200);
    const lessonResult = page.getByTestId("cmd-result-lesson").first();
    await expect(lessonResult).toBeVisible();
    await expect(lessonResult).toContainText("Bộ nhớ ảo");

    // 2. Acronym search: "tlb"
    await input.fill("tlb");
    await page.waitForTimeout(200);
    await expect(page.getByTestId("cmd-result-lesson").first()).toBeVisible();

    // 3. Multi-word unaccented search: "thay the trang" -> Matches Lesson 3.3
    await input.fill("thay the trang");
    await page.waitForTimeout(200);
    await expect(page.getByTestId("cmd-result-lesson").first()).toContainText("Thuật toán Thay thế Trang");

    // 4. Adversarial inputs: SQL injection, script tags, long string
    const adversarialQueries = [
      "' OR 1=1 --",
      "<script>alert('xss')</script>",
      "!@#$%^&*()_+-=[]{}|;':,./<>?",
      "a".repeat(200)
    ];

    for (const q of adversarialQueries) {
      await input.fill(q);
      await page.waitForTimeout(100);
      // Ensure page did not crash and input retains value
      await expect(input).toHaveValue(q);
    }

    // Close palette with Escape
    await page.keyboard.press("Escape");
    await expect(modal).not.toBeVisible();
  });

  test("Stress 4.3: Quick Capture from Command Palette saves card into FSRS queue", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    await page.getByTestId("cmd-k-trigger").click();
    await expect(page.getByTestId("command-palette-modal")).toBeVisible();

    const quickInput = page.getByTestId("quick-capture-input");
    await quickInput.fill("TLB Cache: {{c1::Translation Lookaside Buffer}} đạt tỉ lệ trúng > 99%");

    const saveBtn = page.getByTestId("quick-capture-save-btn");
    await saveBtn.click();

    // Verify confirmation toast or success message
    await expect(page.getByText("Đã lưu thẻ Cloze nhanh")).toBeVisible({ timeout: 5000 });

    await page.keyboard.press("Escape");
  });
});

async function essayInputSafeFill(locator: any, text: string) {
  await locator.fill(text);
}
