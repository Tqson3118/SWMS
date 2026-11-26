import type { Page } from "@playwright/test";
import { test, expect } from "./fixtures/test-base";

const RETIRED_CODING_COURSE_ID = "22222222-2222-4222-8222-222222222225";

const RETIRED_SLUGS = [
  "document-course-dotnet-react-roadmap",
  "document-course-fullstack-csharp-react-refresh",
  "lap-trinh-javascript-tu-duy-giai-quyet-van-de",
  "dau-tu-quan-tri-danh-muc-thuc-chien",
  "tam-ly-hoc-ung-dung-khoa-hoc-tri-nho",
];

const CURATED_SLUGS = [
  "doc-course-studxy-fullstack-bootcamp",
  "document-course-ielts",
  "document-course-investing",
  "document-course-personalfinance",
  "document-course-lifeskills",
  "document-course-psychology",
  "tieng-anh-giao-tiep-thuc-chien-a2-b1",
  "tieng-nhat-so-cap-n5",
  "tieng-trung-so-cap-hsk1",
  "he-dieu-hanh-bo-nho-ao",
  "document-course-ai-workflows",
  "document-course-data-sql",
  "document-course-cybersecurity",
  "document-course-deep-learning-systems",
];

async function provisionUser(page: Page): Promise<string> {
  const email = `lifecycle.${Date.now()}.${Math.random().toString(36).slice(2, 7)}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "Học viên Course Lifecycle Test",
    },
  });
  expect(regRes.ok()).toBeTruthy();
  const regJson = await regRes.json();
  const userId = regJson.user.id as string;

  await page.addInitScript((uid: string) => {
    window.localStorage.setItem("swms_active_user_id", uid);
    window.localStorage.setItem("swms_theme", "light");
  }, userId);

  return userId;
}

test.describe("Course Lifecycle & Curriculum Upgrade E2E Suite (Zero-Facade)", () => {
  test.setTimeout(120000);

  // ---------------------------------------------------------------------------
  // Pillar 1: /app/courses Catalog, Tabs, Feature Chips, Grid/List & Pin
  // ---------------------------------------------------------------------------
  test("1. Pillar 1 (/app/courses): 14 Curated, No Retired, Tabs, Filters, Grid/List, Quick Preview Drawer & Pin Persistence", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    await page.goto("/app/courses");
    await page.waitForLoadState("domcontentloaded");

    // 1A. Confirm progressive reveal renders <= 8 cards initially while badge shows >= 14 total
    const countBadge = page.locator("h1 + span").first();
    await expect(countBadge).toBeVisible({ timeout: 10000 });
    const countText = await countBadge.innerText();
    const totalCount = parseInt(countText, 10);
    expect(totalCount).toBeGreaterThanOrEqual(14);

    await expect(page.locator("[data-course-slug]").first()).toBeVisible({ timeout: 15000 });
    const initialCardCount = await page.locator("[data-course-slug]").count();
    expect(initialCardCount).toBeGreaterThan(0);
    expect(initialCardCount).toBeLessThanOrEqual(8);

    const showAllBtn = page.getByTestId("courses-show-all-btn");
    const showMoreBtn = page.getByTestId("courses-show-more-btn");
    await expect(showMoreBtn).toBeVisible({ timeout: 10000 });
    if (await showAllBtn.isVisible().catch(() => false)) {
      await showAllBtn.click();
    } else {
      await showMoreBtn.click();
    }

    // Confirm all 14 curated courses render after expanding & no retired slugs appear
    for (const slug of RETIRED_SLUGS) {
      await expect(page.locator(`[data-course-slug="${slug}"]`)).toHaveCount(0);
    }
    for (const slug of CURATED_SLUGS) {
      await expect(
        page.locator(`[data-course-slug="${slug}"]`).first(),
        `Curated course ${slug} must render on /app/courses`
      ).toBeVisible({ timeout: 15000 });
    }
    // Verify page content does not leak old titles
    const catalogText = await page.innerText("body");
    expect(catalogText).not.toContain("FullStack C# & React Refresh (28 Tuần)");
    expect(catalogText).not.toContain(".NET & React Full-Stack Roadmap");

    // 1B. Switch Collection Tabs
    const tabPinned = page.getByRole("button", { name: /Đã ghim/i });
    await tabPinned.click();
    await page.waitForTimeout(200);

    const tabMine = page.getByRole("button", { name: /Của tôi/i });
    await tabMine.click();
    await page.waitForTimeout(200);

    const tabArchived = page.getByRole("button", { name: /Đã lưu trữ/i });
    await tabArchived.click();
    await page.waitForTimeout(200);

    const tabAll = page.getByRole("button", { name: /^Tất cả/i });
    await tabAll.click();
    await page.waitForTimeout(200);

    // 1C. Filter Chips
    const labChip = page.getByRole("button", { name: /Có Lab thực hành/i });
    await labChip.click();
    await page.waitForTimeout(200);

    const audioChip = page.getByRole("button", { name: /Có Audio/i });
    await audioChip.click();
    await page.waitForTimeout(200);

    const fsrsChip = page.getByRole("button", { name: /Có Flashcard FSRS/i });
    await fsrsChip.click();
    await page.waitForTimeout(200);

    const clearFiltersBtn = page.getByRole("button", { name: /Xóa bộ lọc/i });
    if (await clearFiltersBtn.isVisible()) {
      await clearFiltersBtn.click();
      await page.waitForTimeout(200);
    }

    // 1D. Grid / List View Toggle
    const listViewBtn = page.locator('button[aria-label="Dạng danh sách"]');
    await expect(listViewBtn).toBeVisible();
    await listViewBtn.click();
    await page.waitForTimeout(200);

    const gridViewBtn = page.locator('button[aria-label="Dạng lưới"]');
    await expect(gridViewBtn).toBeVisible();
    await gridViewBtn.click();
    await page.waitForTimeout(200);

    // 1E. Quick Preview Drawer
    const previewDrawerBtn = page.locator('button[title="Xem nhanh đề cương"]').first();
    await expect(previewDrawerBtn).toBeVisible({ timeout: 10000 });
    await previewDrawerBtn.click();

    const drawerDialog = page.locator('div[role="dialog"]');
    await expect(drawerDialog).toBeVisible({ timeout: 10000 });
    await expect(
      drawerDialog.getByText(/Mục tiêu đầu ra|Điều kiện tiên quyết|Chương trình chi tiết/i).first()
    ).toBeVisible({ timeout: 10000 });

    const closeDrawerBtn = page.getByRole("button", { name: "Đóng bảng xem nhanh" });
    await expect(closeDrawerBtn).toBeVisible();
    await closeDrawerBtn.click();
    await expect(drawerDialog).toHaveCount(0);

    // 1F. Pin a Course -> Reload F5 -> Verify Persistence -> Unpin
    const cardOptionsBtn = page.locator('button[aria-label="Tùy chọn khóa học"]').first();
    await expect(cardOptionsBtn).toBeVisible();
    await cardOptionsBtn.click();

    const [pinRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/preferences") && res.request().method() === "PUT"
      ),
      page.getByRole("menuitem", { name: "Ghim khóa học" }).click(),
    ]);
    expect(pinRes.status()).toBe(200);

    await expect(page.getByText(/Đã ghim/i).first()).toBeVisible({ timeout: 10000 });

    // Reload F5
    await page.reload();
    await page.waitForLoadState("domcontentloaded");

    // Open tab "Đã ghim" and verify the pinned course appears
    await page.getByRole("button", { name: /Đã ghim/i }).click();
    await page.waitForTimeout(300);
    const pinnedOptionsBtn = page.locator('button[aria-label="Tùy chọn khóa học"]').first();
    await expect(pinnedOptionsBtn).toBeVisible({ timeout: 10000 });

    // Clean up state: unpin
    await pinnedOptionsBtn.click();
    const [unpinRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/preferences") && res.request().method() === "PUT"
      ),
      page.getByRole("menuitem", { name: "Bỏ ghim" }).click(),
    ]);
    expect(unpinRes.status()).toBe(200);
  });

  // ---------------------------------------------------------------------------
  // Test 2: Retired Course Redirect Banner & Navigation
  // ---------------------------------------------------------------------------
  test("2. Retired Course Redirect (/app/courses/22222222-2222-4222-8222-222222222225): Banner and navigation to bootcamp", async ({
    page,
    capturedErrors,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    let expectedRetired404s = 0;
    const unexpected404Urls: string[] = [];
    page.on("response", (res) => {
      if (res.status() === 404) {
        if (res.url().includes(RETIRED_CODING_COURSE_ID)) {
          expectedRetired404s += 1;
        } else {
          unexpected404Urls.push(res.url());
        }
      }
    });

    // Resolve bootcamp course ID from live API
    const coursesRes = await page.request.get("http://127.0.0.1:5080/api/v1/courses");
    expect(coursesRes.ok()).toBeTruthy();
    const courses: Array<{ id: string; title: string; slug?: string }> = await coursesRes.json();
    const bootcampCourse = courses.find(
      (c) => c.slug === "doc-course-studxy-fullstack-bootcamp" || /bootcamp/i.test(c.slug || c.title || "")
    );
    expect(bootcampCourse).toBeTruthy();

    // Navigate to retired coding course URL
    await page.goto(`/app/courses/${RETIRED_CODING_COURSE_ID}`);
    await page.waitForLoadState("domcontentloaded");

    // Confirm upgrade & redirect banner is visible
    await expect(
      page.getByText(/Khóa học đã được nâng cấp & gộp|gộp vào lộ trình chuẩn mới/i).first()
    ).toBeVisible({ timeout: 15000 });
    await expect(page.getByText(/doc-course-studxy-fullstack-bootcamp/i)).toBeVisible();

    expect(unexpected404Urls).toEqual([]);
    expect(expectedRetired404s).toBeGreaterThanOrEqual(1);

    const redirectBtn = page.getByRole("button", { name: /Chuyển tới Lộ trình Chuẩn Mới/i });
    await expect(redirectBtn).toBeVisible({ timeout: 10000 });
    await redirectBtn.click();

    // Verify cleanly redirected to valid bootcamp course Hub
    await page.waitForURL(`**/app/courses/${bootcampCourse!.id}**`, { timeout: 15000 });
    await expect(
      page.getByRole("heading", { name: /Studxy Full-Stack/i }).first()
    ).toBeVisible({ timeout: 10000 });

    // Clear only browser's built-in 404 console log matching the intentional retired-course API 404
    let remainingExpected = expectedRetired404s;
    for (let i = capturedErrors.length - 1; i >= 0 && remainingExpected > 0; i--) {
      if (capturedErrors[i].includes("status of 404")) {
        capturedErrors.splice(i, 1);
        remainingExpected -= 1;
      }
    }
  });

  // ---------------------------------------------------------------------------
  // Pillar 2: /app/courses/[id] Tabs URL Sync, Practice Lab CTA & Daily Goal DB Persistence
  // ---------------------------------------------------------------------------
  test("3. Pillar 2 (/app/courses/[id]): 4 Tabs URL Sync, Practice Lab CTA & Settings Daily Goal DB Persistence", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    // Resolve Investing course (rich practice labs + quizzes)
    const coursesRes = await page.request.get("http://127.0.0.1:5080/api/v1/courses");
    expect(coursesRes.ok()).toBeTruthy();
    const courses: Array<{ id: string; title: string; slug?: string }> = await coursesRes.json();
    const targetCourse = courses.find(
      (c) => c.slug === "document-course-investing" || /investing|đầu tư/i.test(c.slug || c.title || "")
    ) || courses[0];
    expect(targetCourse).toBeTruthy();

    await page.goto(`/app/courses/${targetCourse.id}`);
    await page.waitForLoadState("domcontentloaded");

    // 3A. Tabs synchronized with URL query params
    const practiceTabBtn = page.getByRole("tab", { name: /Thực hành & Ôn tập/i });
    await practiceTabBtn.click();
    await expect(page).toHaveURL(new RegExp(`tab=practice`));

    const assetsTabBtn = page.getByRole("tab", { name: /Tài nguyên & Ghi chú/i });
    await assetsTabBtn.click();
    await expect(page).toHaveURL(new RegExp(`tab=assets`));

    const settingsTabBtn = page.getByRole("tab", { name: /Cài đặt khóa học/i });
    await settingsTabBtn.click();
    await expect(page).toHaveURL(new RegExp(`tab=settings`));

    // Reload at settings tab and verify still on settings tab
    await page.reload();
    await page.waitForLoadState("domcontentloaded");
    await expect(page).toHaveURL(new RegExp(`tab=settings`));
    await expect(page.getByText(/Lịch học & Mục tiêu hàng ngày/i)).toBeVisible({ timeout: 10000 });

    // 3B. Verify preset chip updates dailyGoal & forecast box, then update to 4 and assert DB persistence
    const preset3Btn = page.getByRole("button", { name: /3 bài\/ngày/i });
    await expect(preset3Btn).toBeVisible({ timeout: 10000 });
    await preset3Btn.click();

    const goalInput = page.locator("form input[type='number']").first();
    await expect(goalInput).toHaveValue("3");
    await expect(page.getByText(/Dự kiến hoàn thành sau/i)).toBeVisible({ timeout: 5000 });

    await goalInput.fill("4");

    const saveGoalBtn = page.getByRole("button", { name: /Lưu lịch học/i });
    const [saveResp] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/preferences") && res.request().method() === "PUT"
      ),
      saveGoalBtn.click(),
    ]);
    expect(saveResp.status()).toBe(200);
    await expect(
      page.getByText(/Đã lưu mục tiêu học tập vào cơ sở dữ liệu thành công/i)
    ).toBeVisible({ timeout: 10000 });

    // Reload F5 and confirm value 4 remains
    await page.reload();
    await page.waitForLoadState("domcontentloaded");
    await expect(page.locator("form input[type='number']").first()).toHaveValue("4");

    // 3C. Switch to Practice Tab and confirm "Mở Lab thực hành" button & total activities > 0
    await page.getByRole("tab", { name: /Thực hành & Ôn tập/i }).click();
    await expect(page.getByRole("button", { name: /Mở Lab thực hành/i })).toBeVisible({
      timeout: 15000,
    });
    await expect(page.getByText(/bài đạt/i).first()).toBeVisible({ timeout: 10000 });
  });

  // ---------------------------------------------------------------------------
  // Pillar 3: /app/courses/[id]/learn Bookmark, Needs Review & In-Course Search Modal
  // ---------------------------------------------------------------------------
  test("4. Pillar 3 (/app/courses/[id]/learn): Bookmark, Needs Review Persistence & In-Course Search Modal", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    const coursesRes = await page.request.get("http://127.0.0.1:5080/api/v1/courses");
    expect(coursesRes.ok()).toBeTruthy();
    const courses: Array<{ id: string; title: string; slug?: string }> = await coursesRes.json();
    const targetCourse = courses.find(
      (c) => c.slug === "doc-course-studxy-fullstack-bootcamp" || /bootcamp/i.test(c.slug || c.title || "")
    ) || courses[0];

    const detailRes = await page.request.get(`http://127.0.0.1:5080/api/v1/courses/${targetCourse.id}`);
    expect(detailRes.ok()).toBeTruthy();
    const detail = await detailRes.json();
    const firstLesson = detail.chapters?.[0]?.lessons?.[0] || detail.lessons?.[0];
    expect(firstLesson).toBeTruthy();

    await page.goto(`/app/courses/${targetCourse.id}/learn?lessonId=${firstLesson.id}`);
    await page.waitForLoadState("domcontentloaded");

    const bookmarkBtn = page.getByTestId("toggle-lesson-bookmark-btn");
    const reviewBtn = page.getByTestId("toggle-lesson-needs-review-btn");
    await expect(bookmarkBtn).toBeVisible({ timeout: 15000 });
    await expect(reviewBtn).toBeVisible({ timeout: 15000 });

    // 4A. Click Bookmark & Needs Review
    const [bmRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/flags") && res.request().method() === "PUT"
      ),
      bookmarkBtn.click(),
    ]);
    expect(bmRes.status()).toBe(200);
    await expect(bookmarkBtn).toHaveAttribute("aria-pressed", "true");

    const [rvRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/flags") && res.request().method() === "PUT"
      ),
      reviewBtn.click(),
    ]);
    expect(rvRes.status()).toBe(200);
    await expect(reviewBtn).toHaveAttribute("aria-pressed", "true");

    // Verify icons on roadmap sidebar
    const sidebar = page.getByTestId("course-roadmap-sidebar");
    await expect(sidebar.locator('span[title="Đã đánh dấu"]')).toBeVisible({ timeout: 10000 });
    await expect(sidebar.locator('span[title="Cần ôn lại"]')).toBeVisible({ timeout: 10000 });

    // 4B. Reload F5 and confirm persistence & uncompleted lesson remains not completed
    await page.reload();
    await page.waitForLoadState("domcontentloaded");

    await expect(page.getByTestId("toggle-lesson-bookmark-btn")).toHaveAttribute(
      "aria-pressed",
      "true",
      { timeout: 10000 }
    );
    await expect(page.getByTestId("toggle-lesson-needs-review-btn")).toHaveAttribute(
      "aria-pressed",
      "true",
      { timeout: 10000 }
    );
    await expect(sidebar.locator('span[title="Đã đánh dấu"]')).toBeVisible({ timeout: 10000 });
    await expect(sidebar.locator('span[title="Cần ôn lại"]')).toBeVisible({ timeout: 10000 });

    // Confirm lesson is NOT falsely marked complete
    await expect(page.getByTestId("sticky-mark-lesson-complete-btn")).toBeVisible();
    await expect(sidebar.getByTestId("lesson-completed-icon")).toHaveCount(0);

    // 4C. In-Course Search Modal (Ctrl+Shift+F or search toolbar button)
    const searchModalBtn = page.getByTestId("toolbar-open-course-search-btn");
    if (await searchModalBtn.isVisible()) {
      await searchModalBtn.click();
    } else {
      await page.keyboard.press("Control+Shift+F");
    }

    const searchModalInput = page.locator("input#course-search-modal-title");
    await expect(searchModalInput).toBeVisible({ timeout: 10000 });

    // Wait for in-course search response & results
    const [searchResp] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/search?q=") && res.status() === 200,
        { timeout: 15000 }
      ),
      searchModalInput.fill("Lập trình"),
    ]);
    expect(searchResp.status()).toBe(200);

    const searchResultItem = page
      .locator("div[role='dialog'] [role='option'], div[role='dialog'] button")
      .filter({ hasText: /Bài|Chương|Lập trình/i })
      .first();
    await expect(searchResultItem).toBeVisible({ timeout: 10000 });

    // Keyboard navigation: ArrowDown then Enter to select lesson and close modal cleanly
    await page.keyboard.press("ArrowDown");
    await page.keyboard.press("Enter");

    // Modal closes
    await expect(page.locator("div[role='dialog']")).toHaveCount(0);

    // Clean up flags
    await page.getByTestId("toggle-lesson-bookmark-btn").click();
    await page.getByTestId("toggle-lesson-needs-review-btn").click();
  });

  // ---------------------------------------------------------------------------
  // Pillar 4: Lifecycle Clone -> Edit Metadata -> AI Lesson Assistant -> Delete & Duplicates Warning
  // ---------------------------------------------------------------------------
  test("5. Pillar 4 (/app/courses/new & [id]/edit): Duplicates Warning, Clone -> Edit Metadata -> AI Assistant -> Delete", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    // 5A. Duplicates Warning on /app/courses/new
    await page.goto("/app/courses/new");
    await page.waitForLoadState("domcontentloaded");

    const topicInput = page.locator("input[placeholder*='Ví dụ:']").first();
    await expect(topicInput).toBeVisible({ timeout: 15000 });

    const [dupResp] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/check-duplicates") &&
          res.request().method() === "POST" &&
          res.status() === 200
      ),
      topicInput.fill("Full-Stack"),
    ]);
    expect(dupResp.status()).toBe(200);

    // Assert duplicate warning banner appears with existing Bootcamp course link
    await expect(
      page.getByText(/Phát hiện khóa học có nội dung tương đồng trong hệ thống/i)
    ).toBeVisible({ timeout: 10000 });
    const existingCourseLink = page
      .locator("a[href*='/app/courses/']")
      .filter({ hasText: /Studxy Full-Stack.*Bootcamp|Bootcamp/i })
      .first();
    await expect(existingCourseLink).toBeVisible({ timeout: 10000 });

    // 5B. Clone a course into personal library
    await page.goto("/app/courses");
    await page.waitForLoadState("domcontentloaded");

    const firstCardOptionsBtn = page.locator('button[aria-label="Tùy chọn khóa học"]').first();
    await expect(firstCardOptionsBtn).toBeVisible({ timeout: 10000 });
    await firstCardOptionsBtn.click();

    const [cloneRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/clone") && res.request().method() === "POST"
      ),
      page.getByRole("menuitem", { name: "Nhân bản cá nhân" }).click(),
    ]);
    expect(cloneRes.status()).toBe(200);
    const clonedCourse = await cloneRes.json();
    const clonedId = (clonedCourse.id || clonedCourse.courseId) as string;
    expect(clonedId).toBeTruthy();

    // 5C. Navigate to /edit of personal clone & edit metadata
    await page.goto(`/app/courses/${clonedId}/edit`);
    await page.waitForLoadState("domcontentloaded");

    const metaToggleBtn = page.getByTestId("toggle-course-metadata-btn");
    await expect(metaToggleBtn).toBeVisible({ timeout: 15000 });
    await metaToggleBtn.click();

    const descInput = page.locator("form textarea").first();
    await expect(descInput).toBeVisible({ timeout: 5000 });
    await descInput.fill("Mô tả khóa học cá nhân tùy biến E2E Test Lifecycle.");

    const [saveMetaRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes(`/api/v1/courses/${clonedId}`) && res.request().method() === "PUT"
      ),
      page.getByRole("button", { name: /Lưu (thay đổi metadata|thông tin khóa học)/i }).click(),
    ]);
    expect(saveMetaRes.status()).toBe(200);
    await expect(
      page.getByText(/Đã lưu thông tin & metadata khóa học thành công/i)
    ).toBeVisible({ timeout: 10000 });

    // 5D. AI Lesson Assistant on a lesson
    const aiToggleBtn = page.getByTestId("toggle-ai-lesson-assistant-btn");
    await expect(aiToggleBtn).toBeVisible({ timeout: 10000 });
    await aiToggleBtn.click();

    const expandBtn = page.getByRole("button", { name: /Mở rộng bài học/i });
    await expect(expandBtn).toBeVisible({ timeout: 5000 });

    const [aiRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/ai-enhance") && res.request().method() === "POST"
      ),
      expandBtn.click(),
    ]);
    expect(aiRes.status()).toBe(200);
    await expect(page.getByText(/Kết quả gợi ý từ AI/i)).toBeVisible({ timeout: 15000 });

    // 5E. 2-Step Permanent Course Deletion
    const openDeleteModalBtn = page.getByTestId("open-delete-course-modal-btn");
    await expect(openDeleteModalBtn).toBeVisible({ timeout: 10000 });
    await openDeleteModalBtn.click();

    // Confirm step 1
    const step1ProceedBtn = page.getByRole("button", { name: /Tôi hiểu rủi ro, tiếp tục/i });
    await expect(step1ProceedBtn).toBeVisible({ timeout: 5000 });
    await step1ProceedBtn.click();

    // Confirm step 2
    const confirmDeleteBtn = page.getByTestId("confirm-delete-course-btn");
    await expect(confirmDeleteBtn).toBeVisible({ timeout: 5000 });

    const [deleteRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes(`/api/v1/courses/${clonedId}`) &&
          res.request().method() === "DELETE"
      ),
      confirmDeleteBtn.click(),
    ]);
    expect(deleteRes.status()).toBe(200);

    // Verify redirected back to /app/courses
    await page.waitForURL("**/app/courses", { timeout: 15000 });

    // Verify course is completely deleted from DB (HTTP 404)
    const verifyGetRes = await page.request.get(
      `http://127.0.0.1:5080/api/v1/courses/${clonedId}`
    );
    expect(verifyGetRes.status()).toBe(404);
  });

  // ---------------------------------------------------------------------------
  // Pillar 5: Flagship Upgrades (Mission Strip, Shortcuts Modal, Quiz Nav, Typography & Mobile Overflow)
  // ---------------------------------------------------------------------------
  test("6. Flagship Upgrades (/app, /quiz, /learn): Mission Strip, Shortcuts Modal, Quiz Navigator, Typography Width & Mobile Overflow", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    const coursesRes = await page.request.get("http://127.0.0.1:5080/api/v1/courses");
    expect(coursesRes.ok()).toBeTruthy();
    const courses: Array<{ id: string; title: string; slug?: string }> = await coursesRes.json();
    const bootcamp =
      courses.find(
        (c) =>
          c.slug === "doc-course-studxy-fullstack-bootcamp" ||
          /bootcamp/i.test(c.slug || c.title || "")
      ) || courses[0];
    expect(bootcamp).toBeTruthy();

    // 6A. /app: Today Mission Strip & Keyboard Shortcuts Modal (? / Escape)
    await page.goto("/app");
    await page.waitForLoadState("domcontentloaded");

    await expect(page.getByTestId("today-mission-strip")).toBeVisible({ timeout: 15000 });

    const neutralHeading = page.locator("h1, h2").first();
    await expect(neutralHeading).toBeVisible({ timeout: 10000 });
    await neutralHeading.click();
    await page.keyboard.press("?");

    const shortcutsModal = page.getByTestId("keyboard-shortcuts-modal");
    await expect(shortcutsModal).toBeVisible({ timeout: 10000 });
    await expect(page.getByTestId("close-shortcuts-modal")).toBeVisible();

    await page.keyboard.press("Escape");
    await expect(shortcutsModal).toHaveCount(0);

    // 6B. /app/courses/[id]/quiz: Quiz Question Navigator & Zero Horizontal Overflow
    await page.goto(`/app/courses/${bootcamp.id}/quiz`);
    await page.waitForLoadState("domcontentloaded");

    await expect(page.getByTestId("quiz-item-1")).toBeVisible({ timeout: 15000 });
    const quizNavigator = page.getByTestId("quiz-question-navigator");
    await expect(quizNavigator).toBeVisible({ timeout: 10000 });
    const navigatorBtnCount = await quizNavigator.locator("button").count();
    expect(navigatorBtnCount).toBeGreaterThanOrEqual(1);

    const quizZeroOverflow = await page.evaluate(
      () => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 2
    );
    expect(quizZeroOverflow).toBeTruthy();

    // 6C. /app/courses/[id]/learn: Typography Popover Width Toggle & Mobile Zero Overflow
    await page.goto(`/app/courses/${bootcamp.id}/learn`);
    await page.waitForLoadState("domcontentloaded");

    const proseColumn = page.getByTestId("reading-prose-column");
    await expect(proseColumn).toBeVisible({ timeout: 15000 });

    const typographyBtn = page.locator('button[aria-label="Tùy chỉnh chữ"]');
    await expect(typographyBtn).toBeVisible({ timeout: 10000 });
    await typographyBtn.click();

    const wideWidthBtn = page.getByRole("button", { name: /Rộng \(84ch\)/i });
    await expect(wideWidthBtn).toBeVisible({ timeout: 5000 });
    await wideWidthBtn.click();

    const learnWideZeroOverflow = await page.evaluate(
      () => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 2
    );
    expect(learnWideZeroOverflow).toBeTruthy();

    const standardWidthBtn = page.getByRole("button", { name: /Chuẩn \(68ch\)/i });
    if (!(await standardWidthBtn.isVisible())) {
      await typographyBtn.click();
    }
    await expect(standardWidthBtn).toBeVisible({ timeout: 5000 });
    await standardWidthBtn.click();

    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(200);
    await expect(proseColumn).toBeVisible({ timeout: 10000 });

    const mobileZeroOverflow = await page.evaluate(
      () => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 2
    );
    expect(mobileZeroOverflow).toBeTruthy();
  });

  // ---------------------------------------------------------------------------
  // Test 7: 14 Curated Courses, New Document Courses Rich Lessons & Expanded OS/EN/JA/ZH (5ch/12l)
  // ---------------------------------------------------------------------------
  test("7. Wave 4 Curated Catalog (14 Courses): New Applied Courses Load Rich Lessons & OS/EN/JA/ZH Show 5 Chapters / 12 Lessons", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    const coursesRes = await page.request.get("http://127.0.0.1:5080/api/v1/courses");
    expect(coursesRes.ok()).toBeTruthy();
    const courses: Array<{
      id: string;
      title: string;
      slug?: string;
      chapterCount?: number;
      lessonCount?: number;
    }> = await coursesRes.json();

    for (const slug of CURATED_SLUGS) {
      expect(
        courses.some((c) => c.slug === slug),
        `API /api/v1/courses must return curated slug ${slug}`
      ).toBeTruthy();
    }

    // 7A. Verify all 14 curated courses render cleanly on /app/courses (after expanding progressive reveal)
    await page.goto("/app/courses");
    await page.waitForLoadState("domcontentloaded");

    await expect(page.locator("[data-course-slug]").first()).toBeVisible({ timeout: 15000 });
    const showAll7 = page.getByTestId("courses-show-all-btn");
    const showMore7 = page.getByTestId("courses-show-more-btn");
    await expect(showMore7).toBeVisible({ timeout: 10000 });
    if (await showAll7.isVisible().catch(() => false)) {
      await showAll7.click();
    } else {
      await showMore7.click();
    }

    for (const slug of CURATED_SLUGS) {
      const card = page.locator(`[data-course-slug="${slug}"]`).first();
      await expect(card, `Card for ${slug} must render cleanly`).toBeVisible({ timeout: 15000 });
    }

    const catalogZeroOverflow = await page.evaluate(
      () => document.documentElement.scrollWidth <= document.documentElement.clientWidth + 2
    );
    expect(catalogZeroOverflow).toBeTruthy();

    // 7B. Expanded OS / EN / JA / ZH courses show 5 chapters / 12 lessons on cards & detail API
    const expandedSlugs = [
      "he-dieu-hanh-bo-nho-ao",
      "tieng-anh-giao-tiep-thuc-chien-a2-b1",
      "tieng-nhat-so-cap-n5",
      "tieng-trung-so-cap-hsk1",
    ];
    for (const slug of expandedSlugs) {
      const card = page.locator(`[data-course-slug="${slug}"]`).first();
      await expect(card).toContainText("5 chương");
      await expect(card).toContainText("12 bài");

      const matched = courses.find((c) => c.slug === slug);
      expect(matched).toBeTruthy();
      const detailRes = await page.request.get(
        `http://127.0.0.1:5080/api/v1/courses/${matched!.id}`
      );
      expect(detailRes.ok()).toBeTruthy();
      const detail = await detailRes.json();
      const chapters: Array<{ lessons?: Array<{ id: string }> }> = detail.chapters || [];
      expect(chapters.length).toBe(5);
      const totalLessons = chapters.reduce((acc, ch) => acc + (ch.lessons?.length || 0), 0);
      expect(totalLessons).toBe(12);
    }

    // 7C. Opening newly added courses loads rich lessons cleanly
    const newCourseSlugs = [
      "document-course-ai-workflows",
      "document-course-data-sql",
      "document-course-cybersecurity",
    ];
    for (const slug of newCourseSlugs) {
      const matched = courses.find((c) => c.slug === slug);
      expect(matched, `Expected course ${slug} to exist`).toBeTruthy();

      await page.goto(`/app/courses/${matched!.id}/learn`);
      await page.waitForLoadState("domcontentloaded");

      await expect(page.getByTestId("course-roadmap-sidebar")).toBeVisible({ timeout: 15000 });
      await expect(page.getByTestId("reading-prose-column")).toBeVisible({ timeout: 15000 });
      const mdView = page.getByTestId("lesson-markdown-view");
      await expect(mdView).toBeVisible({ timeout: 15000 });
      const mdText = (await mdView.innerText()).trim();
      expect(mdText.length).toBeGreaterThan(200);
    }
  });

  // ---------------------------------------------------------------------------
  // Test 8: AppliedSystemsLab Real E2E (Diagnosis + POST /api/v1/ai/evaluate-practice + FSRS Save)
  // ---------------------------------------------------------------------------
  test("8. AppliedSystemsLab (/app/practice): Diagnosis + Real POST /api/v1/ai/evaluate-practice + Grading Badge + Save FSRS", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    const coursesRes = await page.request.get("http://127.0.0.1:5080/api/v1/courses");
    expect(coursesRes.ok()).toBeTruthy();
    const courses: Array<{ id: string; title: string; slug?: string }> = await coursesRes.json();

    const preferredSlugs = [
      "document-course-ai-workflows",
      "document-course-data-sql",
      "document-course-cybersecurity",
    ];
    let targetCourse = courses.find((c) => c.slug && preferredSlugs.includes(c.slug));
    expect(targetCourse).toBeTruthy();

    // Find a lesson that has an applied practice activity in DB
    const actRes = await page.request.get(
      `http://127.0.0.1:5080/api/v1/practice/activities?courseId=${targetCourse!.id}`
    );
    expect(actRes.ok()).toBeTruthy();
    const activities: Array<{ lessonId?: string | null; type?: string }> = await actRes.json();
    const appliedAct = activities.find((a) =>
      /prompt_critique|prompt-critique|sql_reasoning|sql-reasoning|threat_spotter|threat-spotter/i.test(
        a.type || ""
      )
    );

    let lessonId = appliedAct?.lessonId || "";
    if (!lessonId) {
      const detailRes = await page.request.get(
        `http://127.0.0.1:5080/api/v1/courses/${targetCourse!.id}`
      );
      expect(detailRes.ok()).toBeTruthy();
      const detail = await detailRes.json();
      lessonId = detail.chapters?.[0]?.lessons?.[0]?.id || "";
    }
    expect(lessonId).toBeTruthy();

    await page.goto(`/app/practice/${targetCourse!.id}/${lessonId}`);
    await page.waitForLoadState("domcontentloaded");

    const appliedLab = page.getByTestId("applied-systems-lab");
    await expect(appliedLab).toBeVisible({ timeout: 20000 });

    // Select first diagnosis option & check diagnosis
    const diagOption0 = appliedLab.getByTestId("applied-diagnosis-option-0");
    await expect(diagOption0).toBeVisible({ timeout: 15000 });
    await diagOption0.check();

    const checkDiagBtn = appliedLab.getByTestId("applied-check-diagnosis-btn");
    await expect(checkDiagBtn).toBeEnabled();
    await checkDiagBtn.click();
    await expect(appliedLab.getByTestId("applied-diagnosis-feedback")).toBeVisible({
      timeout: 5000,
    });

    // Submit write-in response to real POST /api/v1/ai/evaluate-practice
    const responseInput = appliedLab.getByTestId("applied-response-input");
    await expect(responseInput).toBeVisible();
    await responseInput.fill(
      "Vai trò kỹ sư hệ thống: Xác định rõ bối cảnh nghiệp vụ, bổ sung ràng buộc JSON Schema đầu ra, tiêu chí kiểm chứng sự thật và giới hạn an toàn trước khi thực thi."
    );

    const submitEvalBtn = appliedLab.getByTestId("applied-submit-eval-btn");
    await expect(submitEvalBtn).toBeEnabled();

    const [evalRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/ai/evaluate-practice") &&
          res.request().method() === "POST",
        { timeout: 20000 }
      ),
      submitEvalBtn.click(),
    ]);
    expect(evalRes.status()).toBe(200);

    // Honest grading badge renders (either live AI or honest offline keyword badge)
    const gradingBadge = appliedLab.getByTestId("applied-grading-badge");
    await expect(gradingBadge).toBeVisible({ timeout: 15000 });
    await expect(gradingBadge).toContainText(/AI chấm trực tiếp|Chấm theo từ khóa \(offline\)/i);

    // Save FSRS flashcard via applied-save-fsrs-btn
    const saveFsrsBtn = appliedLab.getByTestId("applied-save-fsrs-btn");
    await expect(saveFsrsBtn).toBeVisible();
    await expect(saveFsrsBtn).toBeEnabled();

    const [saveCardRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/api/v1/flashcards") && res.request().method() === "POST",
        { timeout: 15000 }
      ),
      saveFsrsBtn.click(),
    ]);
    expect(saveCardRes.ok()).toBeTruthy();
    await expect(saveFsrsBtn).toContainText(/Đã lưu FSRS/i, { timeout: 10000 });
  });

  // ---------------------------------------------------------------------------
  // Test 9: Lesson Workspace Active Recall + Non-Blocking Wrong QuickCheck Save to FSRS
  // ---------------------------------------------------------------------------
  test("9. Lesson Workspace (/learn): Active Recall Evaluate + Wrong QuickCheck 1-Click Save to FSRS Without Blocking Completion", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    const coursesRes = await page.request.get("http://127.0.0.1:5080/api/v1/courses");
    expect(coursesRes.ok()).toBeTruthy();
    const courses: Array<{ id: string; title: string; slug?: string }> = await coursesRes.json();

    const targetCourse =
      courses.find((c) => c.slug === "doc-course-studxy-fullstack-bootcamp") ||
      courses.find((c) => c.slug === "document-course-investing") ||
      courses[0];
    expect(targetCourse).toBeTruthy();

    // Find a lesson with a linked quiz containing multiple_choice items
    const quizzesRes = await page.request.get(
      `http://127.0.0.1:5080/api/v1/practice-quizzes?courseId=${targetCourse!.id}`
    );
    expect(quizzesRes.ok()).toBeTruthy();
    const quizzes: Array<{ id: string; lessonId?: string | null }> = await quizzesRes.json();
    const candidateLessonIds = Array.from(
      new Set(quizzes.map((q) => q.lessonId).filter((id): id is string => Boolean(id)))
    );

    const detailRes = await page.request.get(
      `http://127.0.0.1:5080/api/v1/courses/${targetCourse!.id}`
    );
    expect(detailRes.ok()).toBeTruthy();
    const detail = await detailRes.json();
    const firstLessonId: string = detail.chapters?.[0]?.lessons?.[0]?.id || "";
    const chosenLessonId = candidateLessonIds[0] || firstLessonId;
    expect(chosenLessonId).toBeTruthy();

    await page.goto(`/app/courses/${targetCourse!.id}/learn?lessonId=${chosenLessonId}`);
    await page.waitForLoadState("domcontentloaded");

    // 9A. Active Recall evaluation & honest grading badge
    const activeRecallCard = page.getByTestId("lesson-active-recall-card");
    await expect(activeRecallCard).toBeVisible({ timeout: 15000 });
    await activeRecallCard.scrollIntoViewIfNeeded();

    const recallInput = activeRecallCard.getByTestId("active-recall-input");
    await recallInput.fill(
      "Khái niệm cốt lõi gồm kiến trúc tách lớp rõ ràng, luồng xử lý dữ liệu nhất quán, kiểm thử tự động và nguyên tắc áp dụng an toàn trong thực tế."
    );

    const recallSubmitBtn = activeRecallCard.getByTestId("active-recall-submit-btn");
    await expect(recallSubmitBtn).toBeEnabled();

    const [recallEvalRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/ai/evaluate-practice") &&
          res.request().method() === "POST",
        { timeout: 20000 }
      ),
      recallSubmitBtn.click(),
    ]);
    expect(recallEvalRes.status()).toBe(200);

    const recallBadge = activeRecallCard.getByTestId("active-recall-grading-badge");
    await expect(recallBadge).toBeVisible({ timeout: 15000 });
    await expect(recallBadge).toContainText(/AI chấm trực tiếp|Chấm theo từ khóa \(offline\)/i);

    // 9B. QuickCheck wrong answer -> 1-click quick-check-save-wrong-fsrs-btn
    const quickCheckSection = page.getByTestId("lesson-quick-check-section");
    await quickCheckSection.scrollIntoViewIfNeeded();
    await expect(
      quickCheckSection.getByText(/Đang tải câu hỏi kiểm tra nhanh/i)
    ).toHaveCount(0, { timeout: 15000 });
    await expect(quickCheckSection.getByTestId("quick-check-q-0")).toBeVisible({
      timeout: 15000,
    });

    // Try answering across available quick-check items until a wrong answer triggers the save button
    let saveWrongBtn = quickCheckSection.getByTestId("quick-check-save-wrong-fsrs-btn").first();
    for (const qIdx of [0, 1]) {
      if (await saveWrongBtn.isVisible().catch(() => false)) break;
      const qCard = quickCheckSection.getByTestId(`quick-check-q-${qIdx}`);
      if (!(await qCard.isVisible().catch(() => false))) continue;

      const optBtn = qCard.locator("button").first();
      await optBtn.click();
      await expect(quickCheckSection.getByTestId(`quick-check-feedback-${qIdx}`)).toBeVisible({
        timeout: 10000,
      });

      if (await saveWrongBtn.isVisible().catch(() => false)) break;

      // If option 0 happened to be correct on q-0, try option 1 on q-1 (or next candidate lesson)
      const q1Card = quickCheckSection.getByTestId("quick-check-q-1");
      if (qIdx === 0 && (await q1Card.isVisible().catch(() => false))) {
        await q1Card.locator("button").nth(1).click();
        await expect(quickCheckSection.getByTestId("quick-check-feedback-1")).toBeVisible({
          timeout: 10000,
        });
        break;
      }
    }

    // If both questions on the first lesson happened to match by chance, try second candidate lesson
    if (!(await saveWrongBtn.isVisible().catch(() => false)) && candidateLessonIds.length > 1) {
      await page.goto(`/app/courses/${targetCourse!.id}/learn?lessonId=${candidateLessonIds[1]}`);
      await page.waitForLoadState("domcontentloaded");
      await quickCheckSection.scrollIntoViewIfNeeded();
      for (const optIdx of [0, 1]) {
        const qCard = quickCheckSection.getByTestId(`quick-check-q-${optIdx}`);
        if (!(await qCard.isVisible({ timeout: 10000 }).catch(() => false))) continue;
        await qCard.locator("button").nth(optIdx === 0 ? 1 : 0).click();
        await expect(quickCheckSection.getByTestId(`quick-check-feedback-${optIdx}`)).toBeVisible({
          timeout: 10000,
        });
        if (await saveWrongBtn.isVisible().catch(() => false)) break;
      }
    }

    await expect(saveWrongBtn).toBeVisible({ timeout: 10000 });
    const [saveWrongRes] = await Promise.all([
      page.waitForResponse(
        (res) => res.url().includes("/api/v1/flashcards") && res.request().method() === "POST",
        { timeout: 15000 }
      ),
      saveWrongBtn.click(),
    ]);
    expect(saveWrongRes.ok()).toBeTruthy();
    await expect(saveWrongBtn).toContainText(/Đã lưu vào FSRS/i, { timeout: 10000 });

    // 9C. Verify lesson completion is NOT blocked by Active Recall or wrong QuickCheck
    const completeBtn = page.getByTestId("sticky-mark-lesson-complete-btn");
    await expect(completeBtn).toBeVisible({ timeout: 10000 });
    await expect(completeBtn).toBeEnabled();

    const [completeRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/lessons/") &&
          res.url().endsWith("/complete") &&
          res.request().method() === "POST",
        { timeout: 15000 }
      ),
      completeBtn.click(),
    ]);
    expect(completeRes.status()).toBe(200);
  });

  // ---------------------------------------------------------------------------
  // Test 10: /app/review Interleaving Toggle & Session Mastery Progress Bar
  // ---------------------------------------------------------------------------
  test("10. /app/review: Interleaving Toggle Triggers Real interleave=true & Rating Renders Session Mastery Bar", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    const userId = await provisionUser(page);

    // Seed 3 due flashcards across different source topics for isolated user
    const seedCards = [
      {
        cardType: "basic",
        sourceTitleSnapshot: "AI Workflows · Tuần 01",
        sourceQuoteSnapshot: "Prompt 5 lớp",
        frontText: "5 lớp chuẩn của một Prompt kỹ thuật gồm những thành phần nào?",
        backText: "Role, Context, Constraints, Examples, Output Schema.",
        tagNames: ["ai-workflows"],
      },
      {
        cardType: "basic",
        sourceTitleSnapshot: "Data & SQL · Tuần 05",
        sourceQuoteSnapshot: "Fan-out Join Trap",
        frontText: "Vì sao JOIN trực tiếp 2 bảng nhiều-nhiều gây sai số tổng doanh thu?",
        backText: "Gây nhân bản dòng (Fan-out); cần tổng hợp trước trong CTE theo đúng Grain.",
        tagNames: ["data-sql"],
      },
      {
        cardType: "basic",
        sourceTitleSnapshot: "An toàn số · Tuần 04",
        sourceQuoteSnapshot: "Passkeys FIDO2",
        frontText: "Passkeys (FIDO2) chống Phishing hiệu quả hơn SMS OTP nhờ cơ chế nào?",
        backText: "Ràng buộc mật mã theo tên miền gốc (Origin-bound public key cryptography).",
        tagNames: ["cybersecurity"],
      },
    ];

    for (const card of seedCards) {
      const createRes = await page.request.post("http://127.0.0.1:5080/api/v1/flashcards", {
        headers: { "X-Test-User-Id": userId },
        data: card,
      });
      expect(createRes.ok()).toBeTruthy();
    }

    await page.goto("/app/review");
    await page.waitForLoadState("domcontentloaded");

    const interleaveToggle = page.getByTestId("review-interleave-toggle");
    await expect(interleaveToggle).toBeVisible({ timeout: 15000 });

    const [interleaveRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/flashcards/due") &&
          res.url().includes("interleave=true") &&
          res.request().method() === "GET",
        { timeout: 15000 }
      ),
      interleaveToggle.click(),
    ]);
    expect(interleaveRes.status()).toBe(200);
    await expect(interleaveToggle).toContainText("Đan xen chủ đề ✓", { timeout: 10000 });

    // Flip card and rate Good (3) -> mastery bar appears
    const flipBtn = page.getByTestId("review-flip-btn");
    await expect(flipBtn).toBeVisible({ timeout: 10000 });
    await flipBtn.click();

    const rateGoodBtn = page.getByTestId("review-rate-3");
    await expect(rateGoodBtn).toBeVisible({ timeout: 10000 });

    const [syncReviewRes] = await Promise.all([
      page.waitForResponse(
        (res) =>
          res.url().includes("/api/v1/flashcards/sync-reviews") &&
          res.request().method() === "POST",
        { timeout: 15000 }
      ),
      rateGoodBtn.click(),
    ]);
    expect(syncReviewRes.status()).toBe(200);

    const masteryBar = page.getByTestId("review-session-mastery-bar");
    await expect(masteryBar).toBeVisible({ timeout: 10000 });
    await expect(masteryBar).toContainText(/Độ chắc nhớ phiên:/i);
  });
});
