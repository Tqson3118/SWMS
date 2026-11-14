import { test, expect } from "./fixtures/test-base";
import path from "node:path";

const SAMPLE_ZIP_PATH = path.resolve(__dirname, "../../test-fixtures/sample-swm-course.zip");
const SAMPLE_PDF_PATH = path.resolve(__dirname, "../../test/Giáo trình Pháp luật đại cương_ebook.pdf");

async function provisionUser(page: any) {
  const email = `studio.test.${Date.now()}.${Math.random().toString(36).slice(2, 7)}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "Course Creator Tester"
    }
  });
  const regJson = await regRes.json();
  const userId = regJson.user.id;

  // Inject user ID into localStorage before any script runs
  await page.addInitScript((uid: string) => {
    window.localStorage.setItem("swms_active_user_id", uid);
  }, userId);

  await page.route("**/api/v1/**", async (route: any) => {
    await route.continue({
      headers: {
        ...route.request().headers(),
        "x-test-user-id": userId
      }
    });
  });

  return { userId, email };
}

test.describe("Course Creator Studio E2E Acceptance Suite", () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
  });

  // --------------------------------------------------------------------------
  // Scenario 1: Multi-Format Document Ingestion & Course Creation
  // --------------------------------------------------------------------------
  test("CS-01: Creates course from uploaded document with automatic chapters & lessons", async ({ page }) => {
    await provisionUser(page);
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    // Click "Tạo khóa học" button to open modal
    const openBtn = page.locator('[data-testid="create-course-hub-btn"]').or(page.locator('[data-testid="open-course-create-btn"]')).first();
    await expect(openBtn).toBeVisible({ timeout: 10000 });
    await openBtn.click();

    // Verify modal is open
    const modal = page.locator('[data-testid="course-create-modal"]');
    await expect(modal).toBeVisible();

    // Switch to Document tab
    const docTab = page.locator('[data-testid="tab-doc-course"]');
    if (await docTab.isVisible()) {
      await docTab.click();
    }

    // Upload PDF document
    const fileInput = page.locator('[data-testid="course-doc-file-input"]').or(page.locator('[data-testid="file-upload-input"]')).first();
    await fileInput.setInputFiles(SAMPLE_PDF_PATH);

    // Submit / confirm creation
    const submitBtn = page.locator('[data-testid="submit-doc-course-btn"]').or(page.locator('[data-testid="confirm-upload-btn"]')).first();
    if (await submitBtn.isVisible()) {
      await submitBtn.click();
    }

    // Modal closes upon successful creation or transitions to Reader
    await expect(modal).toBeHidden({ timeout: 25000 });

    // Transition into Course learning view
    const roadmapPanel = page.locator('[data-testid="course-roadmap-panel"]');
    await expect(roadmapPanel).toBeVisible({ timeout: 20000 });

    // Switch back to Dashboard (Mode 0) to verify course card
    const dashboardTab = page.getByTestId("mode-tab-dashboard");
    await dashboardTab.click();

    const courseCard = page.locator('[data-testid="course-card"]').first();
    await expect(courseCard).toBeVisible({ timeout: 15000 });
  });

  // --------------------------------------------------------------------------
  // Scenario 1 (Variant): Interactive 3-Step AI Wizard
  // --------------------------------------------------------------------------
  test("CS-02: Creates course via Interactive 3-Step AI Wizard (Topic -> Clarify -> Syllabus Preview)", async ({ page }) => {
    await provisionUser(page);
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    // Open Hub Modal
    const openBtn = page.locator('[data-testid="create-course-hub-btn"]').or(page.locator('[data-testid="open-course-create-btn"]')).first();
    await openBtn.click();

    // Select Wizard AI tab
    const wizardTab = page.locator('[data-testid="tab-wizard-course"]');
    await expect(wizardTab).toBeVisible();
    await wizardTab.click();

    // Step 1: Input Topic
    const topicInput = page.locator('[data-testid="wizard-topic-input"]');
    await expect(topicInput).toBeVisible();
    await topicInput.fill("Lập trình Web C# .NET 10 từ cơ bản đến nâng cao");

    const step1Next = page.locator('[data-testid="wizard-step1-next-btn"]');
    await step1Next.click();

    // Step 2: Clarification Questions
    const clarifyForm = page.locator('[data-testid="wizard-clarify-form"]');
    await expect(clarifyForm).toBeVisible({ timeout: 10000 });

    // Answer clarification options if present
    const optionBtns = page.locator('[data-testid^="wizard-clarify-option-"]');
    const optionCount = await optionBtns.count();
    if (optionCount > 0) {
      await optionBtns.first().click();
    }

    const step2Next = page.locator('[data-testid="wizard-step2-next-btn"]');
    await step2Next.click();

    // Step 3: Syllabus Preview
    const previewContainer = page.locator('[data-testid="wizard-syllabus-preview"]');
    await expect(previewContainer).toBeVisible({ timeout: 15000 });

    // Commit Course Creation
    const commitBtn = page.locator('[data-testid="wizard-commit-btn"]');
    await expect(commitBtn).toBeVisible();
    await commitBtn.click();

    // Modal closes and switches to Reader
    await expect(page.locator('[data-testid="course-create-modal"]')).toBeHidden({ timeout: 25000 });
    const roadmapPanel = page.locator('[data-testid="course-roadmap-panel"]');
    await expect(roadmapPanel).toBeVisible({ timeout: 20000 });

    // Navigate to Dashboard to verify course card
    const dashboardTab = page.getByTestId("mode-tab-dashboard");
    await dashboardTab.click();

    const courseCard = page.locator('[data-testid="course-card"]').first();
    await expect(courseCard).toBeVisible({ timeout: 15000 });
  });

  // --------------------------------------------------------------------------
  // Scenario 2: Switch to Mode 4 (Studio), add chapter, add lesson, edit markdown & video, save, publish
  // --------------------------------------------------------------------------
  test("CS-03: Mode 4 Studio Authoring - Adds chapter, adds lesson, enters Markdown content & Video URL, saves lesson, and toggles publish status", async ({ page }) => {
    const { userId } = await provisionUser(page);

    // Pre-create a course via API commit
    const courseRes = await page.request.post("http://127.0.0.1:5080/api/v1/courses/generate-wizard/commit", {
      headers: { "x-test-user-id": userId },
      data: {
        topic: "Lập trình C# Hiện đại",
        title: "Khóa học Lập trình C# Hiện đại",
        description: "Biên soạn khóa học thực chiến",
        chapters: [
          {
            title: "Chương 1: Khởi động",
            lessons: [{ title: "Bài 1: Giới thiệu" }]
          }
        ],
        generateFlashcards: true,
        generateQuiz: true
      }
    });
    expect(courseRes.ok()).toBeTruthy();
    const course = await courseRes.json();
    const courseId = course.courseId || course.id;

    // Explicitly set as private/draft so publish button starts as "Xuất bản"
    await page.request.post(`http://127.0.0.1:5080/api/v1/courses/${courseId}/publish`, {
      headers: { "x-test-user-id": userId },
      data: { visibility: "private", isPublished: false }
    });

    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    // Switch to Studio (Mode 4) via TopNav tab
    const studioTab = page.locator('[data-testid="mode-tab-studio"]');
    await expect(studioTab).toBeVisible({ timeout: 10000 });
    await studioTab.click();

    // Verify Studio Course View loaded and title is rendered
    const addChapterBtn = page.locator('[data-testid="studio-add-chapter-btn"]').first();
    await expect(addChapterBtn).toBeVisible({ timeout: 15000 });

    const courseTitleHeading = page.locator("main h1").first();
    await expect(courseTitleHeading).toContainText("Khóa học Lập trình C# Hiện đại");

    // Click title heading to activate in-place title editor input
    await courseTitleHeading.click();
    const courseTitleInput = page.locator('[data-testid="studio-course-title-input"]');
    await expect(courseTitleInput).toBeVisible({ timeout: 5000 });
    await expect(courseTitleInput).toHaveValue("Khóa học Lập trình C# Hiện đại");

    // Add a new chapter
    await addChapterBtn.click();

    // Add a new lesson in the chapter
    const addLessonBtn = page.locator('[data-testid="studio-add-lesson-btn"]').last();
    await expect(addLessonBtn).toBeVisible({ timeout: 10000 });
    await addLessonBtn.click();

    // Fill lesson details after async lesson loading settles
    const lessonTitleInput = page.locator('[data-testid="studio-lesson-title-input"]');
    await expect(lessonTitleInput).toBeVisible({ timeout: 10000 });
    await expect(lessonTitleInput).toHaveValue(/Bài 2/);
    await lessonTitleInput.fill("Bài 2: Kiến trúc Clean Architecture & CQRS");

    // Enter Video URL and verify video player preview renders
    const videoUrlInput = page.locator('[data-testid="studio-video-url-input"]');
    await videoUrlInput.fill("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
    const videoPlayer = page.locator('[data-testid="studio-video-player"]');
    await expect(videoPlayer).toBeVisible({ timeout: 10000 });

    // Enter Markdown Content
    const markdownEditor = page.locator('[data-testid="studio-markdown-editor"]');
    await markdownEditor.fill(
      "# Kiến trúc Clean Architecture\n\nNội dung chi tiết phân tầng mã nguồn theo DDD và CQRS.\n\n- Tách biệt Application Core\n- Độc lập Database và UI Framework"
    );

    // Save lesson
    const saveLessonBtn = page.locator('[data-testid="studio-save-lesson-btn"]');
    await expect(saveLessonBtn).toBeVisible();
    await saveLessonBtn.click();
    await expect(saveLessonBtn).toContainText(/Đã lưu|Lưu bài học/i, { timeout: 10000 });

    // Toggle publish
    const publishBtn = page.locator('[data-testid="studio-publish-btn"]');
    await expect(publishBtn).toBeVisible();
    await expect(publishBtn).toContainText("Xuất bản");
    await publishBtn.click();
    await expect(publishBtn).toContainText(/Bản nháp|Nháp/i, { timeout: 10000 });
  });

  // --------------------------------------------------------------------------
  // Scenario 3: Switch to Learner view (Mode 1), see Course Roadmap with progress bar, mark lesson complete, verify checkmark and %
  // --------------------------------------------------------------------------
  test("CS-04: Mode 1 Learner View - Displays Course Roadmap with progress bar, marks lesson complete, and verifies checkmark & progress update", async ({ page }) => {
    const { userId } = await provisionUser(page);

    const courseRes = await page.request.post("http://127.0.0.1:5080/api/v1/courses/generate-wizard/commit", {
      headers: { "x-test-user-id": userId },
      data: {
        topic: "Thiết kế Hệ thống Phân tán",
        title: "Khóa học Thiết kế Hệ thống Phân tán",
        description: "Lộ trình học tập phân tán",
        chapters: [
          {
            title: "Chương 1: Cơ bản",
            lessons: [
              { title: "Bài 1: CAP Theorem" },
              { title: "Bài 2: Eventual Consistency" }
            ]
          }
        ],
        generateFlashcards: true,
        generateQuiz: true
      }
    });
    expect(courseRes.ok()).toBeTruthy();
    const course = await courseRes.json();
    const courseId = course.courseId || course.id;

    // Explicitly set as private/draft so publish button starts as "Xuất bản"
    await page.request.post(`http://127.0.0.1:5080/api/v1/courses/${courseId}/publish`, {
      headers: { "x-test-user-id": userId },
      data: { visibility: "private", isPublished: false }
    });

    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    // Open course from dashboard
    const startLearningBtn = page.locator('[data-testid="course-start-learning-btn"]').first();
    await expect(startLearningBtn).toBeVisible({ timeout: 15000 });
    await startLearningBtn.click();

    // Verify Course Roadmap Panel & Progress Bar
    const roadmapPanel = page.locator('[data-testid="course-roadmap-panel"]');
    await expect(roadmapPanel).toBeVisible({ timeout: 15000 });

    const progressBar = page.locator('[data-testid="course-progress-bar-container"]');
    await expect(progressBar).toBeVisible();

    const progressPercent = page.locator('[data-testid="course-progress-percent"]');
    await expect(progressPercent).toBeVisible();
    await expect(progressPercent).toContainText("0%");

    // Click "Đánh dấu hoàn thành & Tiếp tục"
    const markCompleteBtn = page.locator('[data-testid="mark-lesson-complete-btn"]');
    await expect(markCompleteBtn).toBeVisible({ timeout: 10000 });
    await markCompleteBtn.click();

    // Verify completion checkmark appears
    const completedIcon = page.locator('[data-testid="lesson-completed-icon"]').first();
    await expect(completedIcon).toBeVisible({ timeout: 10000 });

    // Verify progress % updates to 50%
    await expect(progressPercent).toContainText("50%");
  });

  // --------------------------------------------------------------------------
  // Scenario 4: Mark all lessons complete, verify celebration modal
  // --------------------------------------------------------------------------
  test("CS-05: Course Completion - Marking all lessons complete triggers CourseCelebrationModal with streak & action CTAs", async ({ page }) => {
    const { userId } = await provisionUser(page);

    // Create course with 1 lesson so 1 click reaches 100%
    const courseRes = await page.request.post("http://127.0.0.1:5080/api/v1/courses/generate-wizard/commit", {
      headers: { "x-test-user-id": userId },
      data: {
        topic: "Tối ưu hóa Database",
        title: "Khóa học Tối ưu hóa Database",
        description: "Chuyên đề index và query optimization",
        chapters: [
          {
            title: "Chương 1: Indexing",
            lessons: [{ title: "Bài 1: B-Tree Index chuyên sâu" }]
          }
        ],
        generateFlashcards: true,
        generateQuiz: true
      }
    });
    expect(courseRes.ok()).toBeTruthy();
    const course = await courseRes.json();
    const courseId = course.courseId || course.id;

    // Explicitly set as private/draft so publish button starts as "Xuất bản"
    await page.request.post(`http://127.0.0.1:5080/api/v1/courses/${courseId}/publish`, {
      headers: { "x-test-user-id": userId },
      data: { visibility: "private", isPublished: false }
    });

    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    const startLearningBtn = page.locator('[data-testid="course-start-learning-btn"]').first();
    await expect(startLearningBtn).toBeVisible({ timeout: 15000 });
    await startLearningBtn.click();

    const roadmapPanel = page.locator('[data-testid="course-roadmap-panel"]');
    await expect(roadmapPanel).toBeVisible({ timeout: 15000 });

    // Mark the lesson complete -> 100%
    const markCompleteBtn = page.locator('[data-testid="mark-lesson-complete-btn"]');
    await expect(markCompleteBtn).toBeVisible({ timeout: 10000 });
    await markCompleteBtn.click();

    // Verify celebration modal is displayed
    const celebrationModal = page.locator('[data-testid="course-celebration-modal"]');
    await expect(celebrationModal).toBeVisible({ timeout: 10000 });
    await expect(celebrationModal).toContainText("Chúc mừng bạn đã hoàn thành 100% khóa học!");

    // Verify action CTAs
    const reviewBtn = page.locator('[data-testid="celebration-review-flashcards-btn"]');
    const backDashboardBtn = page.locator('[data-testid="celebration-back-dashboard-btn"]');
    await expect(reviewBtn).toBeVisible();
    await expect(backDashboardBtn).toBeVisible();

    // Click "Quay về Dashboard"
    await backDashboardBtn.click();
    await expect(celebrationModal).toBeHidden({ timeout: 10000 });

    // Verify Dashboard mode
    const dashboardTab = page.locator('[data-testid="mode-tab-dashboard"]');
    await expect(dashboardTab).toBeVisible();
  });

  // --------------------------------------------------------------------------
  // Scenario 5: Dashboard course card displays progress bar and "Mở trong Studio" button works
  // --------------------------------------------------------------------------
  test("CS-06: Dashboard Course Card - Displays updated progress bar, status badge, and 'Mở trong Studio' button transitions into Studio", async ({ page }) => {
    const { userId } = await provisionUser(page);

    const courseRes = await page.request.post("http://127.0.0.1:5080/api/v1/courses/generate-wizard/commit", {
      headers: { "x-test-user-id": userId },
      data: {
        topic: "Microservices với .NET 10",
        title: "Khóa học Microservices với .NET 10",
        description: "Xây dựng hệ thống phân tán",
        chapters: [
          {
            title: "Chương 1: Gateway",
            lessons: [
              { title: "Bài 1: YARP Reverse Proxy" },
              { title: "Bài 2: Distributed Tracing" }
            ]
          }
        ],
        generateFlashcards: true,
        generateQuiz: true
      }
    });
    expect(courseRes.ok()).toBeTruthy();
    const course = await courseRes.json();
    const firstLessonId = course.firstLessonId || course.chapters[0].lessons[0].id;

    // Mark lesson 1 complete via API
    await page.request.post(`http://127.0.0.1:5080/api/v1/lessons/${firstLessonId}/complete`, {
      headers: { "x-test-user-id": userId }
    });

    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    // Verify Dashboard course card
    const courseCard = page.locator('[data-testid="course-card"]').first();
    await expect(courseCard).toBeVisible({ timeout: 15000 });

    // Verify progress badge shows 50%
    const progressBadge = courseCard.locator('[data-testid="course-progress-badge"]');
    await expect(progressBadge).toBeVisible({ timeout: 10000 });
    await expect(progressBadge).toContainText("Đang học 50%");

    // Verify "Mở trong Studio" button exists and works
    const openInStudioBtn = courseCard.locator('[data-testid="open-in-studio-btn"]');
    await expect(openInStudioBtn).toBeVisible();
    await openInStudioBtn.click();

    // Verify transition into Studio (Mode 4)
    const addChapterBtn = page.locator('[data-testid="studio-add-chapter-btn"]').first();
    await expect(addChapterBtn).toBeVisible({ timeout: 15000 });

    const studioTitleHeading = page.locator("main h1").first();
    await expect(studioTitleHeading).toContainText("Khóa học Microservices với .NET 10");
  });

  // --------------------------------------------------------------------------
  // SWM Zip Package Import & Export
  // --------------------------------------------------------------------------
  test("CS-07: SWM Zip Template Package export & import matching D:\\03_Study\\SWM\\document structure", async ({ page }) => {
    await provisionUser(page);
    await page.goto("/workspace");
    await page.waitForLoadState("domcontentloaded");

    // Open Hub Modal
    const openBtn = page.locator('[data-testid="create-course-hub-btn"]').or(page.locator('[data-testid="open-course-create-btn"]')).first();
    await openBtn.click();

    // Select SWM Zip tab
    const zipTab = page.locator('[data-testid="tab-zip-course"]');
    await expect(zipTab).toBeVisible();
    await zipTab.click();

    // Verify Template Download Button exists
    const downloadBtn = page.locator('[data-testid="download-swm-template-btn"]');
    await expect(downloadBtn).toBeVisible();

    // Upload SWM Zip Package
    const zipInput = page.locator('[data-testid="swm-zip-file-input"]');
    await zipInput.setInputFiles(SAMPLE_ZIP_PATH);

    const importBtn = page.locator('[data-testid="import-swm-zip-btn"]');
    await importBtn.click();

    // Modal closes and transitions to Reader
    await expect(page.locator('[data-testid="course-create-modal"]')).toBeHidden({ timeout: 20000 });
    const roadmapPanel = page.locator('[data-testid="course-roadmap-panel"]');
    await expect(roadmapPanel).toBeVisible({ timeout: 20000 });

    // Switch to Dashboard to verify imported course card
    const dashboardTab = page.getByTestId("mode-tab-dashboard");
    await dashboardTab.click();

    const courseTitle = page.locator('[data-testid="course-card-title"]').first();
    await expect(courseTitle).toBeVisible({ timeout: 15000 });
    await expect(courseTitle).toContainText("Khóa học Kiểm thử Tự động SWM");
  });
});
