import { test, expect } from "@playwright/test";
import path from "node:path";

const SAMPLE_ROADMAP_ZIP = path.resolve(__dirname, "../../examples/sample-swm-roadmap.zip");

async function provisionIsolatedUser(page: any) {
  const email = `polish.verify.${Date.now()}.${Math.random().toString(36).slice(2, 7)}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "UI Polish Verifier"
    }
  });
  expect(regRes.ok()).toBeTruthy();
  const regJson = await regRes.json();
  const userId = regJson.user.id;

  await page.addInitScript((uid: string) => {
    window.localStorage.setItem("swms_active_user_id", uid);
  }, userId);

  return { userId, email };
}

test.describe("UI/UX Polish, Clean Mode, DOCX Tables, Sticky & Sample Roadmap E2E", () => {
  test("Verifies Clean Mode, <details> accordion, single left scrollbar, sticky right TOC & scroll-to-top on Psychology", async ({
    page
  }) => {
    await provisionIsolatedUser(page);
    await page.setViewportSize({ width: 1536, height: 864 });

    // Open Psychology course in /workspace (filter strictly by card title, not IELTS description)
    await page.goto("/workspace");
    const psychCard = page
      .getByTestId("course-card")
      .filter({ has: page.getByTestId("course-card-title").filter({ hasText: /Tự học Tâm lý/i }) })
      .first();
    await expect(psychCard).toBeVisible({ timeout: 15000 });
    await psychCard.getByTestId("course-start-learning-btn").click();

    const prose = page.getByTestId("reading-prose-column");
    await expect(prose).toBeVisible({ timeout: 15000 });

    // Expand Chapter 1 in sidebar (or expand all) and open Tuần 01
    const sidebar = page.getByTestId("course-roadmap-sidebar");
    await expect(sidebar).toBeVisible();
    const expandAllBtn = sidebar.getByRole("button", { name: /Mở hết/i });
    if (await expandAllBtn.isVisible()) {
      await expandAllBtn.click();
    }
    const week01Btn = sidebar.getByRole("button", { name: /Tuần 01:/i }).first();
    await expect(week01Btn).toBeVisible({ timeout: 10000 });
    await week01Btn.click();

    await expect(prose).toContainText("Tuần 01", { timeout: 10000 });

    // 1. Clean Reading Mode hides audit metadata by default
    const cleanToggle = page.getByTestId("toggle-clean-reading-mode");
    await expect(cleanToggle).toContainText("Đọc tinh gọn");

    const cleanText = await prose.innerText();
    expect(cleanText).not.toContain("[C01-01]");
    expect(cleanText).not.toContain("[C03-08]");
    expect(cleanText).not.toContain("[LO-W01-01]");
    expect(cleanText).not.toContain("[Mạnh]");
    expect(cleanText).not.toContain("Ghi chú hiển thị Markdown");

    // Toggle Clean Reading Mode OFF -> source tags appear, then back ON
    await cleanToggle.click();
    await expect(cleanToggle).toContainText("Hiện mã nguồn");
    await expect(prose).toContainText("[C01-01]");
    await expect(prose).toContainText("[LO-W01-01]");
    await cleanToggle.click();
    await expect(cleanToggle).toContainText("Đọc tinh gọn");
    expect(await prose.innerText()).not.toContain("[C01-01]");

    // 2. Native interactive <details> mini-reviews
    const detailsItems = page.getByTestId("lesson-details-accordion");
    const detailsCount = await detailsItems.count();
    expect(detailsCount).toBeGreaterThanOrEqual(4);
    const firstDetails = detailsItems.first();
    expect(await firstDetails.evaluate((el) => (el as HTMLDetailsElement).open)).toBe(false);
    await firstDetails.locator("summary").click();
    expect(await firstDetails.evaluate((el) => (el as HTMLDetailsElement).open)).toBe(true);
    await expect(firstDetails).toContainText("tâm trí");

    // 3. Single scrollbar on left sidebar (outer aside must be overflow-hidden)
    const leftAside = page.getByTestId("reading-toc-sidebar");
    await expect(leftAside).toBeVisible();
    const leftOverflowY = await leftAside.evaluate((el) => window.getComputedStyle(el).overflowY);
    expect(leftOverflowY).toBe("hidden");

    // 4. Sticky right TOC panel + action rail stay pinned while scrolling down
    const rightToc = page.getByTestId("reading-on-this-page-toc");
    await expect(rightToc).toBeVisible();

    await page.evaluate(() => window.scrollTo(0, 1400));
    await page.waitForTimeout(150);
    const scrollBeforeNext = await page.evaluate(() => window.scrollY);
    expect(scrollBeforeNext).toBeGreaterThan(800);

    const leftBoxAfterScroll = await leftAside.boundingBox();
    const rightBoxAfterScroll = await rightToc.boundingBox();
    expect(leftBoxAfterScroll).not.toBeNull();
    expect(rightBoxAfterScroll).not.toBeNull();
    expect(leftBoxAfterScroll!.y).toBeLessThan(150);
    expect(rightBoxAfterScroll!.y).toBeLessThan(150);

    // 5. Scroll to bottom, click Complete & Continue -> auto-scrolls to top of Tuần 02
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await page.waitForTimeout(150);
    const completeBtn = prose.getByRole("button", { name: /Đánh dấu hoàn thành/i }).first();
    await completeBtn.click();

    await expect(prose).toContainText(/Tuần 02/i, { timeout: 10000 });
    await page.waitForTimeout(250);
    const scrollAfterNext = await page.evaluate(() => window.scrollY);
    expect(scrollAfterNext).toBeLessThanOrEqual(20);
  });

  test("Verifies LEARNING_PATH.md renders GFM table on Studxy Full-Stack Bootcamp and /app/courses/[id]/learn is scrollable", async ({
    page
  }) => {
    await provisionIsolatedUser(page);
    await page.setViewportSize({ width: 1440, height: 900 });

    const coursesRes = await page.request.get("http://127.0.0.1:5080/api/v1/courses");
    expect(coursesRes.ok()).toBeTruthy();
    const courses: Array<{ id: string; title: string; slug?: string }> = await coursesRes.json();
    const bootcampCourse = courses.find(
      (c) => c.slug === "doc-course-studxy-fullstack-bootcamp" || /Studxy Full-Stack/i.test(c.title)
    );
    expect(bootcampCourse).toBeTruthy();

    const detailRes = await page.request.get(`http://127.0.0.1:5080/api/v1/courses/${bootcampCourse!.id}`);
    expect(detailRes.ok()).toBeTruthy();
    const detail = await detailRes.json();
    const allLessons: Array<{ id: string; title: string }> =
      detail.chapters?.flatMap((ch: { lessons?: Array<{ id: string; title: string }> }) => ch.lessons ?? []) ??
      detail.lessons ??
      [];
    const learningPathLesson = allLessons.find((l) =>
      /Hướng Dẫn Lộ Trình|Lộ Trình & Phương Pháp|LEARNING_PATH/i.test(l.title)
    );
    expect(learningPathLesson).toBeTruthy();

    await page.goto(`/app/courses/${bootcampCourse!.id}/learn`);
    const prose = page.getByTestId("reading-prose-column");
    await expect(prose).toBeVisible({ timeout: 15000 });

    const sidebar = page.getByTestId("course-roadmap-sidebar");
    await expect(sidebar).toBeVisible({ timeout: 10000 });
    const learningPathBtn = sidebar.getByRole("button", { name: learningPathLesson!.title }).first();
    await expect(learningPathBtn).toBeVisible({ timeout: 10000 });
    await learningPathBtn.click();

    const table = prose.locator("table").first();
    await expect(table).toBeVisible({ timeout: 10000 });
    await expect(table).toContainText("Tiêu Chí So Sánh");
    await expect(table).toContainText("Full-time Track");

    // Verify /app/courses/[id]/learn container actually scrolls (no overflow-hidden lock)
    const scrollContainer = page.locator('[data-swms-scroll-container="learn-page"]');
    await expect(scrollContainer).toBeVisible();
    const scrollCheck = await scrollContainer.evaluate((el) => {
      const prev = el.scrollTop;
      el.scrollTop = 400;
      const after = el.scrollTop;
      el.scrollTop = prev;
      return { scrollHeight: el.scrollHeight, clientHeight: el.clientHeight, after };
    });
    expect(scrollCheck.scrollHeight).toBeGreaterThan(scrollCheck.clientHeight);
    expect(scrollCheck.after).toBeGreaterThan(0);

    const overflow = await page.evaluate(() => document.documentElement.scrollWidth > window.innerWidth);
    expect(overflow).toBe(false);
  });

  test("Verifies static sample ZIP download & real UI import via /app/studio/create", async ({
    page
  }) => {
    await provisionIsolatedUser(page);
    await page.setViewportSize({ width: 1440, height: 900 });

    const zipDownloadRes = await page.request.get("/templates/lo-trinh-mau-swm.zip");
    expect(zipDownloadRes.status()).toBe(200);
    const zipBuffer = await zipDownloadRes.body();
    expect(zipBuffer.length).toBeGreaterThan(1000);

    await page.goto("/app/studio/create");
    await expect(page.getByRole("heading", { name: /Tạo khóa học mới/i })).toBeVisible();
    await expect(page.getByRole("link", { name: /Tải gói lộ trình mẫu/i })).toBeVisible();

    const importInput = page.locator('input[accept=".swm,.zip"]');
    await importInput.setInputFiles(SAMPLE_ROADMAP_ZIP);

    await page.waitForURL(/\/app\/studio\/[0-9a-f-]+/i, { timeout: 15000 });
    await expect(page.locator("body")).toContainText("Lộ trình Mẫu Chuẩn SWM", { timeout: 10000 });
  });
});
