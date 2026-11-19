const path = require("path");
const fs = require("fs");
const { chromium } = require(
  path.resolve(__dirname, "../../../frontend/node_modules/@playwright/test")
);

const BASE_URL = process.env.SWMS_FRONTEND_URL || "http://127.0.0.1:3000";
const API_URL = process.env.SWMS_API_URL || "http://127.0.0.1:5080/api/v1";
const SCREENSHOTS_DIR = path.resolve(
  __dirname,
  "../screenshots/e1-inline-practice"
);

if (!fs.existsSync(SCREENSHOTS_DIR)) {
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });
}

function attachDiagnostics(page, diagnostics) {
  page.on("console", (msg) => {
    if (msg.type() === "error") {
      diagnostics.consoleErrors.push({
        url: page.url(),
        text: msg.text().slice(0, 350),
      });
    }
  });
  page.on("pageerror", (err) => {
    diagnostics.pageErrors.push({
      url: page.url(),
      message: err.message,
    });
  });
  page.on("response", (res) => {
    const status = res.status();
    if (status >= 400 && !res.url().includes("favicon")) {
      diagnostics.httpErrors.push({
        status,
        url: res.url(),
      });
    }
  });
}

async function loginDemoSonTran(page) {
  await page.goto(`${BASE_URL}/auth`, { waitUntil: "networkidle" });
  await page.waitForSelector('[data-testid="demo-switcher-toggle"]', {
    timeout: 12000,
  });
  await page.waitForTimeout(600);

  const defaultBtn = page.locator('[data-testid="switch-user-default"]');
  if (!(await defaultBtn.isVisible().catch(() => false))) {
    await page.click('[data-testid="demo-switcher-toggle"]');
    try {
      await defaultBtn.waitFor({ state: "visible", timeout: 4000 });
    } catch {
      await page.waitForTimeout(600);
      await page.click('[data-testid="demo-switcher-toggle"]');
      await defaultBtn.waitFor({ state: "visible", timeout: 6000 });
    }
  }
  await defaultBtn.click();
  await page.waitForURL("**/app**", { timeout: 15000 });
  await page.waitForTimeout(1000);
}

async function checkViewportLayout(page, label, diagnostics) {
  await page.waitForTimeout(500);
  const metrics = await page.evaluate(() => {
    const docEl = document.documentElement;
    const body = document.body;
    const innerWidth = window.innerWidth;
    const scrollWidth = Math.max(
      docEl.scrollWidth,
      body ? body.scrollWidth : 0
    );
    const text = (body ? body.innerText : "") || "";
    const hasDupChapter = /Chương\s+\d+\s*:\s*Chương\s+\d+/i.test(text);
    const hasRawCloze = /\{\{c\d+::/i.test(text);
    return {
      innerWidth,
      scrollWidth,
      overflowPx: scrollWidth - innerWidth,
      hasDupChapter,
      hasRawCloze,
      bodyTextLen: text.trim().length,
    };
  });

  if (metrics.overflowPx > 2) {
    diagnostics.layoutFailures.push({
      label,
      issue: `Horizontal overflow: scrollWidth=${metrics.scrollWidth} > innerWidth=${metrics.innerWidth} (+${metrics.overflowPx}px)`,
      url: page.url(),
    });
  }
  if (metrics.hasDupChapter) {
    diagnostics.textHygieneFailures.push({
      label,
      issue: "Found duplicated chapter prefix (e.g. 'Chương 1: Chương ...')",
      url: page.url(),
    });
  }
  if (metrics.hasRawCloze) {
    diagnostics.textHygieneFailures.push({
      label,
      issue: "Found raw Cloze syntax '{{cN::' in visible page text",
      url: page.url(),
    });
  }
  return metrics;
}

async function capture(page, filename, opts = {}) {
  const target = path.join(SCREENSHOTS_DIR, filename);
  await page.waitForTimeout(350);
  await page.screenshot({
    path: target,
    fullPage: Boolean(opts.fullPage),
  });
  console.log(`[IMG] ${filename}`);
  return target;
}

async function findCourseTargetsFromDom(page) {
  return await page.evaluate(() => {
    const links = Array.from(
      document.querySelectorAll('a[href^="/app/courses/"]')
    );
    const seen = new Map();
    const uuidRegex =
      /^\/app\/courses\/([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})(?:\/|$)/i;
    for (const a of links) {
      const href = a.getAttribute("href") || "";
      const match = href.match(uuidRegex);
      if (!match) continue;
      const id = match[1];
      const text = (a.innerText || "").trim().replace(/\s+/g, " ");
      if (!seen.has(id) || text.length > seen.get(id).title.length) {
        seen.set(id, { id, title: text });
      }
    }
    return Array.from(seen.values());
  });
}

function pickByRegex(courses, regex, fallback) {
  return courses.find((c) => regex.test(c.title || "")) || fallback;
}

async function runAudit() {
  const diagnostics = {
    consoleErrors: [],
    pageErrors: [],
    httpErrors: [],
    layoutFailures: [],
    textHygieneFailures: [],
    featureAssertions: [],
    screenshots: [],
    layoutMetrics: [],
  };

  const browser = await chromium.launch({ headless: true });

  try {
    // 1. DESKTOP AUDIT (1440x900)
    const desktopCtx = await browser.newContext({
      viewport: { width: 1440, height: 900 },
    });
    const page = await desktopCtx.newPage();
    attachDiagnostics(page, diagnostics);

    // 1a. Landing page
    await page.goto(`${BASE_URL}/`, { waitUntil: "networkidle" });
    diagnostics.layoutMetrics.push({
      label: "landing-desktop",
      ...(await checkViewportLayout(page, "landing-desktop", diagnostics)),
    });
    diagnostics.screenshots.push(
      await capture(page, "01-landing-desktop.png", { fullPage: true })
    );

    // 1b. Login Demo "Sơn Trần" -> /app
    await loginDemoSonTran(page);
    await page.waitForLoadState("networkidle").catch(() => {});
    diagnostics.layoutMetrics.push({
      label: "app-dashboard-desktop",
      ...(await checkViewportLayout(page, "app-dashboard-desktop", diagnostics)),
    });
    diagnostics.screenshots.push(
      await capture(page, "02-app-dashboard-desktop.png", { fullPage: false })
    );
    await page
      .getByTestId("learning-roadmaps-showcase")
      .scrollIntoViewIfNeeded()
      .catch(() => {});
    await page.waitForTimeout(400);
    diagnostics.screenshots.push(
      await capture(page, "02b-app-roadmaps-desktop.png", { fullPage: false })
    );

    // 1c. /app/courses (extract rendered courses cleanly)
    await page.goto(`${BASE_URL}/app/courses`, { waitUntil: "networkidle" });
    await page.waitForTimeout(800);
    diagnostics.layoutMetrics.push({
      label: "courses-desktop",
      ...(await checkViewportLayout(page, "courses-desktop", diagnostics)),
    });
    diagnostics.screenshots.push(
      await capture(page, "03-courses-desktop.png", { fullPage: false })
    );

    const courses = await findCourseTargetsFromDom(page);
    console.log(`[INFO] Found ${courses.length} courses for demo user`);

    const ieltsCourse = pickByRegex(
      courses,
      /ielts|english|giao tiếp/i,
      courses[0]
    );
    const codingCourse = pickByRegex(
      courses,
      /\.net|c#|python|lập trình|microservices|fullstack|hệ điều hành/i,
      courses[0]
    );
    const financeCourse = pickByRegex(
      courses,
      /tài chính|đầu tư|kinh tế/i,
      courses[0]
    );
    const psychCourse = pickByRegex(
      courses,
      /tâm lý|kỹ năng sống/i,
      courses[0]
    );

    // 1d. /app/courses/[id]
    const detailCourse = ieltsCourse || courses[0];
    if (detailCourse) {
      await page.goto(`${BASE_URL}/app/courses/${detailCourse.id}`, {
        waitUntil: "networkidle",
      });
      diagnostics.layoutMetrics.push({
        label: "course-detail-desktop",
        ...(await checkViewportLayout(
          page,
          "course-detail-desktop",
          diagnostics
        )),
      });
      diagnostics.screenshots.push(
        await capture(page, "04-course-detail-desktop.png", { fullPage: false })
      );
    }

    // 1e. /learn across 4 domain courses + inline practice checks
    const learnTargets = [
      { key: "ielts", course: ieltsCourse, img: "05-learn-ielts-desktop.png" },
      { key: "coding", course: codingCourse, img: "06-learn-coding-desktop.png" },
      {
        key: "finance",
        course: financeCourse,
        img: "07-learn-finance-desktop.png",
      },
      { key: "psych", course: psychCourse, img: "08-learn-psych-desktop.png" },
    ];

    for (const target of learnTargets) {
      if (!target.course) continue;
      await page.goto(`${BASE_URL}/app/courses/${target.course.id}/learn`, {
        waitUntil: "networkidle",
      });
      await page.waitForTimeout(1200);

      const inlineLab = page.getByTestId("lesson-practice-lab-inline");
      const inlineVisible = await inlineLab.isVisible().catch(() => false);
      const realAssets = page.getByTestId("lesson-real-assets-panel");
      const assetsVisible = await realAssets.isVisible().catch(() => false);
      const companionRail = page.getByTestId("lesson-companion-rail");
      const companionVisible = await companionRail
        .isVisible()
        .catch(() => false);

      diagnostics.featureAssertions.push({
        courseKey: target.key,
        courseTitle: target.course.title,
        inlinePracticeVisible: inlineVisible,
        realAssetsPanelVisible: assetsVisible,
        companionRailVisible: companionVisible,
      });

      if (!inlineVisible) {
        diagnostics.layoutFailures.push({
          label: `learn-${target.key}-desktop`,
          issue:
            "Expected [data-testid='lesson-practice-lab-inline'] visible in default Lesson tab!",
          url: page.url(),
        });
      }

      diagnostics.layoutMetrics.push({
        label: `learn-${target.key}-desktop`,
        ...(await checkViewportLayout(
          page,
          `learn-${target.key}-desktop`,
          diagnostics
        )),
      });
      diagnostics.screenshots.push(
        await capture(page, target.img, { fullPage: false })
      );

      if (target.key === "ielts" && inlineVisible) {
        await inlineLab.scrollIntoViewIfNeeded();
        await page.waitForTimeout(500);
        diagnostics.screenshots.push(
          await capture(page, "09-learn-inline-practice-desktop.png", {
            fullPage: false,
          })
        );
      }

      if (target.key === "psych") {
        const speechBar = page.getByTestId("inline-speech-practice-bar");
        const speechBarCount = await speechBar.count();
        const hasIpaText = await page.evaluate(() => {
          return document.body.innerText.includes("Trạm Luyện Đọc To & Phát Âm Chuẩn IPA");
        });
        console.log(`[VERIFY-E2] Psych lesson speechBarCount: ${speechBarCount}, hasIpaText: ${hasIpaText}`);
        if (speechBarCount > 0 || hasIpaText) {
          diagnostics.layoutFailures.push({
            label: "psych-lesson-ipa-leak",
            issue: "Found unexpected 'Trạm Luyện Đọc To & Phát Âm Chuẩn IPA' on Psychology lesson!",
            url: page.url(),
          });
        }

        const isPsychLabVisible = await page.getByTestId("psychology-practice-lab").isVisible().catch(() => false);
        const isFinanceLabVisible = await page.getByTestId("finance-practice-lab").isVisible().catch(() => false);
        console.log(`[VERIFY-E2] Psych lesson isPsychLabVisible: ${isPsychLabVisible}, isFinanceLabVisible: ${isFinanceLabVisible}`);
        if (!isPsychLabVisible || isFinanceLabVisible) {
          diagnostics.layoutFailures.push({
            label: "psych-practice-lab-mismatch",
            issue: `Expected PsychologyPracticeLab visible and FinancePracticeLab hidden! isPsychLabVisible=${isPsychLabVisible}, isFinanceLabVisible=${isFinanceLabVisible}`,
            url: page.url(),
          });
        }

        if (inlineVisible) {
          await inlineLab.scrollIntoViewIfNeeded();
          await page.waitForTimeout(500);
          diagnostics.screenshots.push(
            await capture(page, "09b-learn-psych-inline-desktop.png", {
              fullPage: false,
            })
          );
          const tab4 = page.getByRole("button", {
            name: /Checklist & Tình Huống Bài Học/i,
          });
          if (await tab4.isVisible().catch(() => false)) {
            await tab4.click();
            await page.waitForTimeout(400);
            diagnostics.screenshots.push(
              await capture(page, "09b-learn-psych-tab4-desktop.png", {
                fullPage: false,
              })
            );
          }
        }
      }
    }

    // 1f. /app/courses/[id]/quiz (prefer a course with quizzes, e.g. codingCourse)
    const quizCourse = codingCourse || detailCourse;
    if (quizCourse) {
      await page.goto(`${BASE_URL}/app/courses/${quizCourse.id}/quiz`, {
        waitUntil: "networkidle",
      });
      await page.waitForTimeout(800);
      const sidebarSelectorVisible = await page
        .getByTestId("quiz-sidebar-selector")
        .isVisible()
        .catch(() => false);
      diagnostics.featureAssertions.push({
        check: "quiz-sidebar-selector",
        courseTitle: quizCourse.title,
        visible: sidebarSelectorVisible,
      });
      diagnostics.layoutMetrics.push({
        label: "quiz-desktop",
        ...(await checkViewportLayout(page, "quiz-desktop", diagnostics)),
      });
      diagnostics.screenshots.push(
        await capture(page, "10-quiz-desktop.png", { fullPage: false })
      );
    }

    // 1g. /app/review
    await page.goto(`${BASE_URL}/app/review`, { waitUntil: "networkidle" });
    await page.waitForTimeout(800);
    diagnostics.layoutMetrics.push({
      label: "review-desktop",
      ...(await checkViewportLayout(page, "review-desktop", diagnostics)),
    });
    diagnostics.screenshots.push(
      await capture(page, "11-review-desktop.png", { fullPage: false })
    );

    // 1h. /app/courses/new
    await page.goto(`${BASE_URL}/app/courses/new`, {
      waitUntil: "networkidle",
    });
    await page.waitForTimeout(600);
    diagnostics.layoutMetrics.push({
      label: "courses-new-desktop",
      ...(await checkViewportLayout(page, "courses-new-desktop", diagnostics)),
    });
    diagnostics.screenshots.push(
      await capture(page, "12-courses-new-desktop.png", { fullPage: false })
    );

    // 1i. /app/settings
    await page.goto(`${BASE_URL}/app/settings`, { waitUntil: "networkidle" });
    await page.waitForTimeout(600);
    diagnostics.layoutMetrics.push({
      label: "settings-desktop",
      ...(await checkViewportLayout(page, "settings-desktop", diagnostics)),
    });
    diagnostics.screenshots.push(
      await capture(page, "13-settings-desktop.png", { fullPage: false })
    );

    // Save storage state for mobile reuse
    const storageState = await desktopCtx.storageState();
    await desktopCtx.close();

    // 2. MOBILE AUDIT (390x844)
    const mobileCtx = await browser.newContext({
      viewport: { width: 390, height: 844 },
      isMobile: true,
      hasTouch: true,
      storageState,
    });
    const mPage = await mobileCtx.newPage();
    attachDiagnostics(mPage, diagnostics);

    const mobileRoutes = [
      { url: `${BASE_URL}/`, img: "01-landing-mobile.png", label: "landing-mobile" },
      { url: `${BASE_URL}/app`, img: "02-app-dashboard-mobile.png", label: "app-dashboard-mobile" },
      { url: `${BASE_URL}/app/courses`, img: "03-courses-mobile.png", label: "courses-mobile" },
      ...(detailCourse
        ? [
            {
              url: `${BASE_URL}/app/courses/${detailCourse.id}`,
              img: "04-course-detail-mobile.png",
              label: "course-detail-mobile",
            },
            {
              url: `${BASE_URL}/app/courses/${detailCourse.id}/learn`,
              img: "05-learn-ielts-mobile.png",
              label: "learn-ielts-mobile",
            },
            {
              url: `${BASE_URL}/app/courses/${detailCourse.id}/quiz`,
              img: "10-quiz-mobile.png",
              label: "quiz-mobile",
            },
          ]
        : []),
      { url: `${BASE_URL}/app/review`, img: "11-review-mobile.png", label: "review-mobile" },
      { url: `${BASE_URL}/app/courses/new`, img: "12-courses-new-mobile.png", label: "courses-new-mobile" },
      { url: `${BASE_URL}/app/settings`, img: "13-settings-mobile.png", label: "settings-mobile" },
    ];

    for (const r of mobileRoutes) {
      await mPage.goto(r.url, { waitUntil: "networkidle" });
      await mPage.waitForTimeout(700);
      diagnostics.layoutMetrics.push({
        label: r.label,
        ...(await checkViewportLayout(mPage, r.label, diagnostics)),
      });
      diagnostics.screenshots.push(
        await capture(mPage, r.img, { fullPage: false })
      );
    }

    await mobileCtx.close();
  } finally {
    await browser.close();
  }

  console.log("\n=== E1 VISUAL & INTERACTIVE AUDIT SUMMARY ===");
  console.log(JSON.stringify(diagnostics, null, 2));

  const fatalCount =
    diagnostics.consoleErrors.length +
    diagnostics.pageErrors.length +
    diagnostics.httpErrors.length +
    diagnostics.layoutFailures.length +
    diagnostics.textHygieneFailures.length;

  if (fatalCount > 0) {
    console.error(`\n❌ AUDIT FOUND ${fatalCount} ISSUE(S)!`);
    process.exit(1);
  }
  console.log("\n✅ ALL VISUAL & INTERACTIVE CHECKS PASSED (0 ERRORS, 0 OVERFLOW)!");
}

runAudit().catch((err) => {
  console.error("FATAL AUDIT CRASH:", err);
  process.exit(1);
});
