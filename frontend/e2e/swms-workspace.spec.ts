import { test, expect } from "@playwright/test";

const MANDATORY_VIEWPORTS = [
  { name: "1080p-100pct-2xl", width: 1920, height: 1080 },
  { name: "1080p-125pct-xl", width: 1536, height: 864 },
  { name: "1080p-150pct-lg", width: 1280, height: 720 }
];

test.describe("SWMS v1.0 — Core Study Loop & Multi-Viewport E2E Suite", () => {
  for (const vp of MANDATORY_VIEWPORTS) {
    test(`Responsive Layout & Zero Horizontal Overflow at ${vp.width}x${vp.height} (${vp.name})`, async ({
      page
    }) => {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.goto("/workspace");

      // 1. Dashboard renders with Onboarding 3-Step Checklist & FSRS Station
      await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
      await expect(page.getByTestId("onboarding-3step-widget")).toBeVisible();
      await expect(page.getByTestId("fsrs-review-station-card")).toBeVisible();

      const dashOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(dashOverflow).toBe(false);

      // 2. Switch to Mode 1: Reading Mode (68ch column + 5 Semantic Colors + Orphaned Banner)
      await page.getByTestId("mode-tab-reading").click();
      await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
      await expect(page.getByTestId("reading-prose-column")).toBeVisible();
      await expect(page.getByTestId("semantic-selection-toolbar")).toBeVisible();

      const readingOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(readingOverflow).toBe(false);

      // 3. Switch to Mode 2: Split Workspace (calc(100vw - 8px) + Citation Cards + Notebook Editor)
      await page.getByTestId("mode-tab-split").click();
      await expect(page.getByTestId("mode-2-split-container")).toBeVisible();
      await expect(page.getByTestId("split-pane-reader")).toBeVisible();
      await expect(page.getByTestId("split-pane-notebook")).toBeVisible();

      const splitOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(splitOverflow).toBe(false);

      // 4. Switch to Mode 3: Review & Practice Mode (FSRS 3D Flip Card + Rating Bar)
      await page.getByTestId("mode-tab-review").click();
      await expect(page.getByTestId("mode-3-review-container")).toBeVisible();

      const reviewOverflow = await page.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth
      );
      expect(reviewOverflow).toBe(false);
    });
  }

  test("End-to-End Core Study Loop: Quota Popover -> Draft Cloze Popover -> Orphaned Reconciliation -> 409 Keep-Both -> FSRS Review & AI Essay Rubric -> Command Palette", async ({
    page
  }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto("/workspace");

    // 1. Check Quota Breakdown Popover
    await page.getByTestId("quota-indicator-btn").click();
    await expect(page.getByTestId("quota-breakdown-popover")).toBeVisible();
    await page.getByTestId("quota-indicator-btn").click();

    // 2. Mode 1 Reading: Switch to Lesson 3.2 Reading Mode (68ch column + 5 Semantic Colors + Orphaned Banner)
    const readLessonBtn = page.getByRole("button", { name: "Đọc sâu" }).first();
    if (await readLessonBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await readLessonBtn.click();
    } else {
      await page.getByTestId("mode-tab-reading").click();
    }
    await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
    await page.evaluate(() => {
      const p = document.querySelector("article p");
      if (p) {
        const range = document.createRange();
        range.selectNodeContents(p);
        const sel = window.getSelection();
        sel?.removeAllRanges();
        sel?.addRange(range);
        p.dispatchEvent(new MouseEvent("mouseup", { bubbles: true }));
      }
    });
    await page.getByTestId("to-flashcard-popover-trigger").click();
    await expect(page.getByTestId("draft-flashcard-popover")).toBeVisible();
    await page.getByTestId("commit-draft-flashcard-btn").click();
    await expect(page.getByTestId("workspace-toast")).toContainText("FSRS v5");

    // 3. Orphaned Highlight Banner & 3-Action Reconciliation Drawer
    await expect(page.getByTestId("orphaned-highlight-banner")).toBeVisible();
    await page.getByTestId("open-orphaned-drawer-btn").click();
    await expect(page.getByTestId("orphaned-reconciliation-drawer")).toBeVisible();
    await page.getByTestId("reconcile-reanchor-0").click();

    // 4. Mode 2 Split Workspace: Interactive Drag Resizing, 409 Keep-Both Conflict Fork & Markdown Export
    await page.getByTestId("mode-tab-split").click();
    const splitContainer = page.getByTestId("mode-2-split-container");
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");

    // Drag Handle 2 far left (-600px) -> clamps Pane 2 at min 30% and Pane 3 at max 55% (15/30/55)
    const handle2 = page.getByTestId("split-resize-handle-2");
    const box2 = await handle2.boundingBox();
    expect(box2).not.toBeNull();
    await page.mouse.move(box2!.x + box2!.width / 2, box2!.y + box2!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box2!.x - 600, box2!.y + box2!.height / 2, { steps: 5 });
    await page.mouse.up();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/30/55");

    // Drag Handle 1 far right (+400px) from 15/30/55 -> cascades and clamps Pane 1 at max 25% (25/30/45)
    const handle1 = page.getByTestId("split-resize-handle-1");
    const box1 = await handle1.boundingBox();
    expect(box1).not.toBeNull();
    await page.mouse.move(box1!.x + box1!.width / 2, box1!.y + box1!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box1!.x + 400, box1!.y + box1!.height / 2, { steps: 5 });
    await page.mouse.up();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "25/30/45");

    // Drag Handle 1 far left (-500px) -> clamps Pane 1 at min 10% (10/45/45)
    const box1After = await handle1.boundingBox();
    expect(box1After).not.toBeNull();
    await page.mouse.move(box1After!.x + box1After!.width / 2, box1After!.y + box1After!.height / 2);
    await page.mouse.down();
    await page.mouse.move(box1After!.x - 500, box1After!.y + box1After!.height / 2, { steps: 5 });
    await page.mouse.up();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "10/45/45");

    // Double-click Handle 1 resets to 15/45/40
    await handle1.dblclick();
    await expect(splitContainer).toHaveAttribute("data-split-ratios", "15/45/40");

    await page.getByTestId("simulate-409-conflict-btn").click();
    await expect(page.getByTestId("workspace-toast")).toContainText("Bản sao xung đột");

    await page.getByTestId("export-markdown-btn").click();
    await expect(page.getByTestId("exported-markdown-box")).toContainText(
      "SWMS Sovereign Markdown v1.0"
    );

    // 5. Mode 3 Review & Practice: Flip 3D Card & Rate Good (3), then test Practice Quiz AI Rubric
    await page.getByTestId("mode-tab-review").click();
    await expect(page.getByTestId("fsrs-3d-flip-card")).toBeVisible();
    await page.getByTestId("fsrs-3d-flip-card").click();
    await expect(page.getByTestId("flashcard-back-face")).toBeVisible();
    await page.getByTestId("rate-btn-3").click();

    // Switch to Practice Quiz sub-tab & submit Essay for AI Rubric Evaluation
    await page.getByTestId("review-subtab-quiz").click();
    await expect(page.getByTestId("practice-quiz-portal")).toBeVisible();
    await page.getByTestId("submit-essay-ai-btn").click();
    await expect(page.getByTestId("essay-ai-rubric-feedback")).toBeVisible({ timeout: 25000 });

    // 6. Global Command Palette (Ctrl+K) Vietnamese Unaccented Search
    await page.getByTestId("cmd-k-trigger").click();
    await expect(page.getByTestId("command-palette-modal")).toBeVisible();
    await page.getByTestId("command-palette-input").fill("bo nho ao");
    await expect(page.getByTestId("cmd-result-lesson").first()).toBeVisible();
  });
});
