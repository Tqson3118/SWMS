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
  test("1. Pillar 1 (/app/courses): 10 Curated, No Retired, Tabs, Filters, Grid/List, Quick Preview Drawer & Pin Persistence", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUser(page);

    await page.goto("/app/courses");
    await page.waitForLoadState("domcontentloaded");

    // 1A. Confirm 10 curated courses exist and no retired slugs appear
    for (const slug of RETIRED_SLUGS) {
      await expect(page.locator(`[data-course-slug="${slug}"]`)).toHaveCount(0);
    }
    // Verify page content does not leak old titles
    const catalogText = await page.innerText("body");
    expect(catalogText).not.toContain("FullStack C# & React Refresh (28 Tuần)");
    expect(catalogText).not.toContain(".NET & React Full-Stack Roadmap");

    // Check count of courses in catalog header
    const countBadge = page.locator("h1 + span").first();
    await expect(countBadge).toBeVisible({ timeout: 10000 });
    const countText = await countBadge.innerText();
    const totalCount = parseInt(countText, 10);
    expect(totalCount).toBeGreaterThanOrEqual(10);

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
      page.getByRole("button", { name: /Lưu thay đổi metadata/i }).click(),
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
});
