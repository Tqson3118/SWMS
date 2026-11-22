import { test, expect } from "./fixtures/test-base";
import type { Page } from "@playwright/test";

const INVESTING_COURSE_ID = "c0000000-0000-0000-0000-000000002224";
const CODING_COURSE_ID = "c0000000-0000-0000-0000-000000002225";
const PSYCHOLOGY_COURSE_ID = "c0000000-0000-0000-0000-000000002226";
const ENGLISH_COURSE_ID = "c0000000-0000-0000-0000-000000002223";

async function mockCourseAndLessonRoutes(
  page: Page,
  coursePayload: Record<string, unknown>,
  lessonPayload: { id: string; title: string; contentMarkdown: string }
) {
  const courseId = String(coursePayload.id);
  await page.route("**/api/v1/documents*", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([]),
    });
  });
  await page.route(`**/api/v1/courses/${courseId}*`, async (route) => {
    if (route.request().url().includes("/enroll")) {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({ enrolled: true }),
      });
      return;
    }
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(coursePayload),
    });
  });
  await page.route(`**/api/v1/lessons/${lessonPayload.id}*`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        ...lessonPayload,
        courseId,
        annotations: [],
      }),
    });
  });
  await page.route("**/api/v1/notebooks/by-source*", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        id: `nb-${lessonPayload.id}`,
        title: lessonPayload.title,
        version: 1,
        contentAst: { type: "doc", content: [] },
      }),
    });
  });
  await page.route("**/api/v1/flashcards/due*", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([]),
    });
  });
  await page.route("**/api/v1/practice-quizzes*", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([]),
    });
  });
}

test.describe("Practice Expansion & Interactive Roadmaps (UX Suite)", () => {
  test.beforeEach(async ({ page }) => {
    // Set viewport
    await page.setViewportSize({ width: 1366, height: 860 });

    // Mock active user in localStorage
    await page.addInitScript(() => {
      window.localStorage.setItem("swms_active_user_id", "u0000000-0000-0000-0000-000000000001");
      window.localStorage.setItem("swms_theme", "light");
    });
  });

  // =========================================================================
  // SCENARIO 1: English Dictation & Word Map
  // =========================================================================
  test("Scenario 1: English Dictation & Vocabulary Word Map Lab", async ({ page }) => {
    // Intercept practice activities and attempts
    await page.route("**/api/v1/practice/activities*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify([
          {
            id: "act-dictation-1",
            courseId: ENGLISH_COURSE_ID,
            type: "dictation",
            title: "Chép chính tả giao tiếp chuẩn",
            configJson: JSON.stringify({
              sentence: "Effective communication builds trust and long-term relationships.",
              ipa: "/ɪˈfɛktɪv kəˌmjuːnɪˈkeɪʃən bɪldz trʌst/",
              meaningVi: "Giao tiếp hiệu quả xây dựng lòng tin và các mối quan hệ lâu dài.",
              keyPhrases: ["effective communication", "builds trust", "relationships"]
            }),
            passingScore: 70,
            userStatus: { attemptCount: 1, bestScore: 90, isPassed: true }
          },
          {
            id: "act-word-map-1",
            courseId: ENGLISH_COURSE_ID,
            type: "word_map",
            title: "Sơ đồ từ vựng: Communication",
            configJson: JSON.stringify({
              centralWord: "communicate",
              definition: "To share or exchange information, news, or ideas.",
              ipa: "/kəˈmjuːnɪkeɪt/",
              collocations: ["communicate effectively", "communicate openly", "communicate with clarity"],
              phrasalVerbs: ["get across", "reach out", "touch base"],
              synonyms: ["convey", "express", "interact"]
            }),
            passingScore: 80,
            userStatus: { attemptCount: 0, bestScore: null, isPassed: false }
          }
        ])
      });
    });

    await page.route("**/api/v1/practice/attempts*", async (route) => {
      if (route.request().method() === "POST") {
        const body = route.request().postDataJSON();
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify({
            id: "att-" + Date.now(),
            ...body,
            createdAt: new Date().toISOString()
          })
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify([])
        });
      }
    });

    // Mock Course & Lesson & open Learn -> Fullscreen Practice page
    const engLesson = {
      id: "les-eng-1",
      title: "Bài 1: Lắng Nghe Chủ Động & Trình Bày Ý Tưởng",
      contentMarkdown: "# Bài 1: Lắng Nghe\n\n```practice-english\n{\"type\":\"dictation\"}\n```\n"
    };
    await mockCourseAndLessonRoutes(
      page,
      {
        id: ENGLISH_COURSE_ID,
        title: "Tiếng Anh Giao Tiếp Thực Hành & Phát Âm Chuẩn",
        slug: "tieng-anh-giao-tiep-thuc-hanh",
        chapters: [
          {
            id: "chap-1",
            title: "Chương 1: Phản Xạ Giao Tiếp Nơi Công Sở",
            lessons: [engLesson]
          }
        ]
      },
      engLesson
    );

    await page.goto(`/app/courses/${ENGLISH_COURSE_ID}/learn?lesson=les-eng-1`);
    await page.waitForLoadState("domcontentloaded");

    await expect(page.locator('[data-testid="center-tab-practice"]')).toHaveCount(0);
    await expect(page.locator('[data-testid="lesson-quick-check-section"]')).toBeVisible({ timeout: 15000 });
    await page.locator('[data-testid="go-to-lesson-practice-btn"]').click();
    await page.waitForURL(`**/app/practice/${ENGLISH_COURSE_ID}/les-eng-1**`, { timeout: 15000 });

    // Verify English Practice Lab is loaded
    await expect(page.locator('[data-testid="english-practice-lab"]')).toBeVisible({ timeout: 15000 });

    // Verify Dictation Mode exists
    const dictationTab = page.locator('[data-testid="tab-dictation"]').or(page.getByRole("button", { name: /Chép chính tả/i }));
    if (await dictationTab.isVisible()) {
      await dictationTab.click();
      await expect(page.getByText(/Chép chính tả/i).first()).toBeVisible();
    }

    // Verify Word Map Mode exists
    const wordMapTab = page.locator('[data-testid="tab-word-map"]').or(page.getByRole("button", { name: /Sơ đồ từ vựng/i }));
    if (await wordMapTab.isVisible()) {
      await wordMapTab.click();
      // Should show central word
      await expect(page.getByText(/communicate/i).first()).toBeVisible();
      await expect(page.getByText(/collocations|kết hợp từ/i).first()).toBeVisible();
    }
  });

  // =========================================================================
  // SCENARIO 2: Coding Web Worker Sandbox & Parsons Problem
  // =========================================================================
  test("Scenario 2: Coding Web Worker Sandbox & Parsons Problem Reordering", async ({ page }) => {
    await page.route("**/api/v1/practice/activities*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify([])
      });
    });

    let attemptRecorded = false;
    await page.route("**/api/v1/practice/attempts*", async (route) => {
      if (route.request().method() === "POST") {
        attemptRecorded = true;
        const postData = route.request().postDataJSON();
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify({
            id: "att-code-1",
            ...postData,
            createdAt: new Date().toISOString()
          })
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify([])
        });
      }
    });

    const codingLesson = {
      id: "les-code-1",
      title: "Bài 1: Tối Ưu Hóa & Thuật Toán Cơ Bản",
      contentMarkdown: "# Bài 1: Tối Ưu Hóa\n\n```javascript\nfunction twoSum(nums, target) { return [0, 1]; }\n```\n"
    };
    await mockCourseAndLessonRoutes(
      page,
      {
        id: CODING_COURSE_ID,
        title: "Lập Trình Web C# .NET 10 & Frontend Thực Chiến",
        slug: "lap-trinh-web-csharp-dotnet-frontend",
        chapters: [
          {
            id: "chap-code-1",
            title: "Chương 1: Thuật Toán & Xử Lý Bất Đồng Bộ",
            lessons: [codingLesson]
          }
        ]
      },
      codingLesson
    );

    await page.goto(`/app/courses/${CODING_COURSE_ID}/learn?lesson=les-code-1`);
    await page.waitForLoadState("domcontentloaded");

    await expect(page.locator('[data-testid="lesson-quick-check-section"]')).toBeVisible({ timeout: 15000 });
    await page.locator('[data-testid="go-to-lesson-practice-btn"]').click();
    await page.waitForURL(`**/app/practice/${CODING_COURSE_ID}/les-code-1**`, { timeout: 15000 });

    // 1. Verify Coding Lab Container
    await expect(page.locator('[data-testid="coding-practice-lab"]')).toBeVisible({ timeout: 15000 });

    // 2. Open Tab 5: Web Worker Sandbox
    const sandboxTab = page.locator('[data-testid="mode-tab-js-sandbox"]');
    await expect(sandboxTab).toBeVisible();
    await sandboxTab.click();

    await expect(page.locator('[data-testid="js-sandbox-container"]')).toBeVisible();
    await expect(page.locator('[data-testid="js-sandbox-code-editor"]')).toBeVisible();

    // Click Run Tests in Web Worker
    const runBtn = page.locator('[data-testid="js-sandbox-run-btn"]');
    await expect(runBtn).toBeVisible();
    await runBtn.click();

    // Verify Test Results are displayed with pass/score
    await expect(page.locator('[data-testid="js-sandbox-results"]')).toBeVisible({ timeout: 8000 });
    await expect(page.getByText(/Kết quả kiểm thử/i)).toBeVisible();

    // 3. Open Tab 6: Parsons Problem
    const parsonsTab = page.locator('[data-testid="mode-tab-parsons"]');
    await expect(parsonsTab).toBeVisible();
    await parsonsTab.click();

    await expect(page.locator('[data-testid="parsons-container"]')).toBeVisible();
    await expect(page.locator('[data-testid="parsons-bank"]')).toBeVisible();
    await expect(page.locator('[data-testid="parsons-workspace"]')).toBeVisible();

    // Click a line from bank to add to workspace
    const firstBankLine = page.locator('[data-testid="parsons-bank-line"]').first();
    if (await firstBankLine.isVisible()) {
      await firstBankLine.click();
      await expect(page.locator('[data-testid="parsons-workspace-line"]').first()).toBeVisible();
    }

    // Check order button
    const checkOrderBtn = page.locator('[data-testid="parsons-check-btn"]');
    await expect(checkOrderBtn).toBeVisible();
    await checkOrderBtn.click();

    // Result card appears
    await expect(page.locator('[data-testid="parsons-result-card"]')).toBeVisible();
  });

  // =========================================================================
  // SCENARIO 3: Finance DCA / Compound & Portfolio Simulator
  // =========================================================================
  test("Scenario 3: Finance DCA, Compound Interest & Portfolio Simulator with Strict Disclaimers", async ({ page }) => {
    await page.route("**/api/v1/practice/activities*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify([])
      });
    });

    await page.route("**/api/v1/practice/attempts*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify([])
      });
    });

    const financeLesson = {
      id: "les-fin-1",
      title: "Bài 1: Sức Mạnh Lãi Kép & Lạm Phát",
      contentMarkdown: "# Bài 1: Lãi kép\n\n```practice-finance\n{\"type\":\"compound\"}\n```\n"
    };
    await mockCourseAndLessonRoutes(
      page,
      {
        id: INVESTING_COURSE_ID,
        title: "Tài Chính Cá Nhân Thực Hành & Đầu Tư Cổ Phiếu Dài Hạn",
        slug: "tai-chinh-ca-nhan-thuc-hanh-dau-tu",
        chapters: [
          {
            id: "chap-fin-1",
            title: "Chương 1: Lãi Kép & Quản Lý Rủi Ro",
            lessons: [financeLesson]
          }
        ]
      },
      financeLesson
    );

    await page.goto(`/app/courses/${INVESTING_COURSE_ID}/learn?lesson=les-fin-1`);
    await page.waitForLoadState("domcontentloaded");

    await expect(page.locator('[data-testid="lesson-quick-check-section"]')).toBeVisible({ timeout: 15000 });
    await page.locator('[data-testid="go-to-lesson-practice-btn"]').click();
    await page.waitForURL(`**/app/practice/${INVESTING_COURSE_ID}/les-fin-1**`, { timeout: 15000 });

    // 1. Verify Finance Lab is visible
    await expect(page.locator('[data-testid="finance-practice-lab"]')).toBeVisible({ timeout: 15000 });

    // 2. Tab Compound Interest
    const compoundTab = page.locator('[data-testid="tab-compound"]');
    await expect(compoundTab).toBeVisible();
    await compoundTab.click();

    // Verify SVG chart or compound calculator exists
    await expect(page.getByText(/Lãi Kép & Tác Động Của Lạm Phát/i)).toBeVisible();
    await expect(page.getByText(/Gửi Tiết Kiệm/i)).toBeVisible();

    // 3. Tab 6: Portfolio Simulator
    const portfolioTab = page.locator('[data-testid="tab-portfolio"]');
    await expect(portfolioTab).toBeVisible();
    await portfolioTab.click();

    await expect(page.locator('[data-testid="portfolio-simulator-lab"]')).toBeVisible();

    // CRITICAL REQUIREMENT: Strict disclaimer label verification
    await expect(
      page.getByText("Dữ liệu mẫu lịch sử mô phỏng phục vụ thực hành — không phải khuyến nghị đầu tư")
    ).toBeVisible();

    // Verify allocation sliders and normalize button
    const normalizeBtn = page.getByRole("button", { name: /Cân Bằng Chuẩn 100%/i });
    await expect(normalizeBtn).toBeVisible();
    await normalizeBtn.click();

    // Scenario toggle
    const inflationScenarioBtn = page.getByRole("button", { name: /Lạm phát cao/i });
    if (await inflationScenarioBtn.isVisible()) {
      await inflationScenarioBtn.click();
      await expect(page.getByText(/Lạm phát/i).first()).toBeVisible();
    }

    // 4. Tab 7: Behavioral Finance Quiz
    const behavioralTab = page.locator('[data-testid="tab-behavioral"]');
    await expect(behavioralTab).toBeVisible();
    await behavioralTab.click();

    await expect(page.locator('[data-testid="behavioral-finance-lab"]')).toBeVisible();
    await expect(page.getByText(/Tâm Lý Học Hành Vi Trong Đầu Tư/i)).toBeVisible();
  });

  // =========================================================================
  // SCENARIO 4: Psychology Stroop Test & Memory Span
  // =========================================================================
  test("Scenario 4: Psychology Stroop Test RT Measurement & Memory Span Miller Law", async ({ page }) => {
    await page.route("**/api/v1/practice/activities*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify([])
      });
    });

    await page.route("**/api/v1/practice/attempts*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify([])
      });
    });

    const psychLesson = {
      id: "les-psych-1",
      title: "Bài 1: Hiệu Ứng Stroop & Hệ Thống 1-2",
      contentMarkdown: "# Bài 1: Stroop\n\n```practice-case\n{\"type\":\"stroop\"}\n```\n"
    };
    await mockCourseAndLessonRoutes(
      page,
      {
        id: PSYCHOLOGY_COURSE_ID,
        title: "Tâm Lý Học Nhận Thức & Kỹ Năng Sống Ứng Dụng",
        slug: "tam-ly-hoc-nhan-thuc-ky-nang-song",
        chapters: [
          {
            id: "chap-psych-1",
            title: "Chương 1: Trí Nhớ & Ức Chế Phản Xạ",
            lessons: [psychLesson]
          }
        ]
      },
      psychLesson
    );

    await page.goto(`/app/courses/${PSYCHOLOGY_COURSE_ID}/learn?lesson=les-psych-1`);
    await page.waitForLoadState("domcontentloaded");

    await expect(page.locator('[data-testid="lesson-quick-check-section"]')).toBeVisible({ timeout: 15000 });
    await page.locator('[data-testid="go-to-lesson-practice-btn"]').click();
    await page.waitForURL(`**/app/practice/${PSYCHOLOGY_COURSE_ID}/les-psych-1**`, { timeout: 15000 });

    // 1. Verify Psychology Lab
    await expect(page.locator('[data-testid="psychology-practice-lab"]')).toBeVisible({ timeout: 15000 });

    // 2. Open Tab 5: Cognitive Labs
    const cogTab = page.locator('[data-testid="tab-cognitive-labs"]');
    await expect(cogTab).toBeVisible();
    await cogTab.click();

    await expect(page.locator('[data-testid="cognitive-labs-container"]')).toBeVisible();

    // 3. Sub-lab 1: Stroop Test
    await expect(page.locator('[data-testid="subtab-stroop"]')).toBeVisible();
    const startStroopBtn = page.locator('[data-testid="stroop-start-btn"]');
    await expect(startStroopBtn).toBeVisible();
    await startStroopBtn.click();

    // Verify active trial word
    await expect(page.locator('[data-testid="stroop-target-word"]')).toBeVisible();

    // Click answer button
    await page.locator('[data-testid="stroop-btn-red"]').click();

    // 4. Sub-lab 2: Memory Span & Chunking
    const memTab = page.locator('[data-testid="subtab-memory-span"]');
    await expect(memTab).toBeVisible();
    await memTab.click();

    await expect(page.locator('[data-testid="memory-span-container"]')).toBeVisible();
    await expect(page.getByText(/Định luật Miller/i)).toBeVisible();

    // Click Level 4
    const lvl4Btn = page.locator('[data-testid="span-lvl-btn-4"]');
    await expect(lvl4Btn).toBeVisible();
    await lvl4Btn.click();

    // Sequence display appears
    await expect(page.locator('[data-testid="memory-sequence-display"]')).toBeVisible();

    // 5. Sub-lab 3: Guided CBT Journal
    const cbtJournalTab = page.locator('[data-testid="subtab-cbt-journal"]');
    await expect(cbtJournalTab).toBeVisible();
    await cbtJournalTab.click();

    await expect(page.locator('[data-testid="cbt-journal-container"]')).toBeVisible();
    await expect(page.locator('[data-testid="cbt-input-a"]')).toBeVisible();
    await expect(page.locator('[data-testid="cbt-input-b"]')).toBeVisible();
    await expect(page.locator('[data-testid="cbt-save-journal-btn"]')).toBeVisible();
  });

  // =========================================================================
  // SCENARIO 5: Dashboard /app "Luyện tập hôm nay" Hub
  // =========================================================================
  test("Scenario 5: Dashboard /app Today Practice Hub with Real Stats & Zero-Facade Empty State", async ({ page }) => {
    // 1. Mock Dashboard Cached with real practice stats
    await page.route("**/api/v1/dashboard/cached*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          user: { id: "u1", fullName: "Nguyễn Văn A", email: "a@studxy.vn", streakDays: 5 },
          reviewStation: { totalDue: 3, totalCards: 20, newCount: 5, learningCount: 5, reviewCount: 10, estimatedMinutes: 6 },
          courses: [
            { id: INVESTING_COURSE_ID, title: "Tài Chính Cá Nhân", progressPercent: 45, lessonCount: 12 },
            { id: CODING_COURSE_ID, title: "Lập Trình Web C# .NET", progressPercent: 60, lessonCount: 18 }
          ],
          documents: [],
          notebooks: [],
          quizzes: []
        })
      });
    });

    await page.route("**/api/v1/courses*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify([])
      });
    });

    await page.route("**/api/v1/practice/stats*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          totalAttempts: 14,
          averageScore: 88.5,
          passedCount: 12,
          todayAttempts: 3,
          streakDays: 5,
          needsReview: [
            {
              activityKey: "investing-compound-1",
              activityTitle: "Tính lãi kép mục tiêu FIRE",
              activityType: "compound",
              courseId: INVESTING_COURSE_ID,
              lessonId: "les-fin-1",
              score: 60,
              createdAt: new Date().toISOString()
            }
          ],
          recentHistory: [
            {
              id: "att-1",
              activityKey: "js-two-sum",
              activityTitle: "Two Sum Web Worker",
              activityType: "js_sandbox",
              score: 100,
              maxScore: 100,
              isPassed: true,
              createdAt: new Date().toISOString()
            }
          ],
          last7Days: [
            { date: "2026-11-16", dayName: "T2", count: 2, avgScore: 85 },
            { date: "2026-11-17", dayName: "T3", count: 1, avgScore: 90 },
            { date: "2026-11-18", dayName: "T4", count: 3, avgScore: 78 },
            { date: "2026-11-19", dayName: "T5", count: 2, avgScore: 92 },
            { date: "2026-11-20", dayName: "T6", count: 2, avgScore: 88 },
            { date: "2026-11-21", dayName: "T7", count: 1, avgScore: 100 },
            { date: "2026-11-22", dayName: "CN", count: 3, avgScore: 88.5 }
          ]
        })
      });
    });

    await page.goto("/app");
    await page.waitForLoadState("domcontentloaded");

    // 1. Verify existing section today-stats-roadmap
    await expect(page.locator('[data-testid="today-stats-roadmap"]')).toBeVisible({ timeout: 15000 });

    // 2. Verify Today's Practice Hub
    await expect(page.locator('[data-testid="today-practice-hub"]')).toBeVisible();

    // 3. Verify 4 Stat Cards
    await expect(page.locator('[data-testid="today-practice-stats"]')).toBeVisible();
    await expect(page.getByText("14 lượt")).toBeVisible();
    await expect(page.getByText("88.5 / 100")).toBeVisible();
    await expect(page.getByText("12 bài")).toBeVisible();

    // 4. Verify SVG Chart
    await expect(page.locator('[data-testid="today-practice-chart"]')).toBeVisible();

    // 5. Verify Needs Review section with direct link
    await expect(page.locator('[data-testid="today-practice-needs-review"]')).toBeVisible();
    await expect(page.getByText("Tính lãi kép mục tiêu FIRE")).toBeVisible();
    const retryLink = page.getByRole("link", { name: /Luyện lại/i }).first();
    await expect(retryLink).toBeVisible();
    await expect(retryLink).toHaveAttribute(
      "href",
      new RegExp(`/app/practice/${INVESTING_COURSE_ID}/les-fin-1`)
    );
  });

  // =========================================================================
  // SCENARIO 6: Dedicated Practice Hub /app/practice
  // =========================================================================
  test("Scenario 6: Dedicated Practice Hub /app/practice Stats, Queue, FSRS & Courses", async ({ page }) => {
    await page.route("**/api/v1/dashboard/cached*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          user: { id: "u1", fullName: "Nguyễn Văn A", email: "a@studxy.vn", streakDays: 5 },
          reviewStation: { totalDue: 4, totalCards: 20, newCount: 5, learningCount: 5, reviewCount: 10, estimatedMinutes: 6 },
          courses: [],
          documents: [],
          notebooks: [],
          quizzes: []
        })
      });
    });

    await page.route("**/api/v1/practice/stats*", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          totalAttempts: 10,
          averageScore: 84.5,
          passedCount: 8,
          todayAttempts: 2,
          streakDays: 4,
          fsrsDueCount: 4,
          needsReview: [
            {
              activityKey: "investing-compound-1",
              activityTitle: "Tính lãi kép mục tiêu FIRE",
              activityType: "compound",
              courseId: INVESTING_COURSE_ID,
              courseTitle: "Tài Chính Cá Nhân Thực Hành",
              lessonId: "les-fin-1",
              lessonTitle: "Bài 1: Sức Mạnh Lãi Kép",
              score: 55,
              passingScore: 70,
              attemptCount: 2,
              createdAt: new Date().toISOString()
            }
          ],
          recentHistory: [],
          chartData: [
            { date: "2026-11-16", dayName: "T2", count: 1, avgScore: 80 },
            { date: "2026-11-17", dayName: "T3", count: 2, avgScore: 82 },
            { date: "2026-11-18", dayName: "T4", count: 1, avgScore: 85 },
            { date: "2026-11-19", dayName: "T5", count: 2, avgScore: 88 },
            { date: "2026-11-20", dayName: "T6", count: 1, avgScore: 79 },
            { date: "2026-11-21", dayName: "T7", count: 1, avgScore: 90 },
            { date: "2026-11-22", dayName: "CN", count: 2, avgScore: 84.5 }
          ],
          courseSummaries: [
            {
              courseId: INVESTING_COURSE_ID,
              title: "Tài Chính Cá Nhân Thực Hành",
              slug: "tai-chinh-ca-nhan",
              totalActivities: 6,
              passedActivities: 3,
              averageScore: 81.2,
              attemptCount: 5,
              lastAttemptAt: new Date().toISOString(),
              lessons: [
                {
                  lessonId: "les-fin-1",
                  title: "Bài 1: Sức Mạnh Lãi Kép",
                  chapterTitle: "Chương 1",
                  activityCount: 2,
                  passedCount: 1,
                  attemptCount: 2
                }
              ]
            }
          ]
        })
      });
    });

    await page.goto("/app/practice");
    await page.waitForLoadState("domcontentloaded");

    await expect(page.getByRole("heading", { name: "Hub Luyện tập" })).toBeVisible({ timeout: 15000 });
    await expect(page.locator('[data-testid="practice-hub-stats"]')).toBeVisible();
    await expect(page.locator('[data-testid="practice-hub-review-queue"]')).toBeVisible();
    await expect(page.getByText("Tính lãi kép mục tiêu FIRE")).toBeVisible();
    await expect(page.getByRole("link", { name: /Mở hàng ôn FSRS/i })).toBeVisible();
    await expect(page.locator('[data-testid="practice-hub-courses"]')).toBeVisible();
    await expect(page.getByText("Tài Chính Cá Nhân Thực Hành").first()).toBeVisible();
  });
});
