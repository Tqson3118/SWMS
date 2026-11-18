const { chromium } = require('../../../../frontend/node_modules/playwright');
const path = require('path');
const fs = require('fs');

const SCREENSHOT_DIR = path.resolve(__dirname, '../../screenshots/a-newbie-mobile');
if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

async function runCoreLearningExperience() {
  console.log('=== STARTING 03: CORE LEARNING EXPERIENCE ===');
  const browser = await chromium.launch({ headless: true });

  const ts = Date.now();
  const testEmail = `qa+newbie-core-${ts}@test.local`;
  const testPassword = 'Password123!';
  const testFullName = 'Nguyễn Thực Hành Di Động';

  // Mobile Context (390x844)
  const mobileContext = await browser.newContext({
    viewport: { width: 390, height: 844 },
    isMobile: true,
    hasTouch: true,
    userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1',
  });
  const page = await mobileContext.newPage();

  console.log(`1. Navigating to landing and registering account: ${testEmail}...`);
  await page.goto('http://127.0.0.1:3000', { waitUntil: 'networkidle' });

  // Register via API from the page to obtain Bearer accessToken and set cookies
  const authData = await page.evaluate(async ({ email, password, fullName }) => {
    const res = await fetch('/api/v1/auth/register', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password, fullName }),
    });
    return await res.json();
  }, { email: testEmail, password: testPassword, fullName: testFullName });

  const accessToken = authData.accessToken;
  const userObj = authData.user || {};
  console.log('Registered successfully! User ID:', userObj.id, 'Got accessToken:', Boolean(accessToken));

  // Set localStorage session in page
  await page.evaluate(({ userObj }) => {
    window.localStorage.setItem('swms_has_refresh_cookie', '1');
    window.localStorage.setItem('swms_active_user_id', userObj.id);
    window.localStorage.setItem('swms_active_user', JSON.stringify({
      id: userObj.id,
      fullName: userObj.fullName,
      email: userObj.email,
      authProvider: 'local',
      avatarUrl: null,
      hasLocalPassword: true,
    }));
  }, { userObj });

  // 2. Commit a sample course with chapters, lessons, flashcards, quiz
  console.log('2. Creating sample course via API with Bearer token...');
  const courseData = await page.evaluate(async (token) => {
    const payload = {
      topic: 'Tiếng Anh Nhập Môn Di Động',
      title: 'Tiếng Anh Giao Tiếp Cho Người Mới',
      description: 'Khóa học thực hành tiếng Anh một tay trên thiết bị di động.',
      chapters: [
        {
          title: 'Chương 1: Chào hỏi và Làm quen',
          sortOrder: 1,
          lessons: [
            {
              title: 'Bài 1: Chào hỏi hàng ngày',
              summary: 'Các mẫu câu giao tiếp cơ bản: Hello, How are you doing today?',
              estimatedMinutes: 5,
            },
            {
              title: 'Bài 2: Giới thiệu nghề nghiệp',
              summary: 'Mẫu câu chia sẻ công việc: What do you do for a living?',
              estimatedMinutes: 7,
            },
          ],
        },
      ],
      idempotencyKey: crypto.randomUUID(),
    };

    const res = await fetch('/api/v1/courses/generate-wizard/commit', {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify(payload),
    });
    return await res.json();
  }, accessToken);

  const courseId = courseData.id || courseData.courseId;
  const firstLessonId = courseData.firstLessonId;
  const quizId = courseData.quizId;
  console.log(`Course created: ${courseId}, firstLesson: ${firstLessonId}, quizId: ${quizId}`);

  // Create an explicit due flashcard for FSRS review
  console.log('Creating due flashcards for review...');
  await page.evaluate(async ({ token, cid, lid }) => {
    const cards = [
      { frontText: 'How are you doing today?', backText: 'Hôm nay bạn thế nào? (Lời chào hỏi tự nhiên)', courseId: cid, lessonId: lid },
      { frontText: 'What do you do for a living?', backText: 'Bạn làm nghề gì để kiếm sống?', courseId: cid, lessonId: lid },
    ];
    for (const c of cards) {
      await fetch('/api/v1/flashcards', {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(c),
      });
    }
  }, { token: accessToken, cid: courseId, lid: firstLessonId });

  // 3. Test /app/courses/[id]/learn on Mobile 390x844
  console.log(`3. Navigating to /app/courses/${courseId}/learn on 390x844...`);
  await page.goto(`http://127.0.0.1:3000/app/courses/${courseId}/learn`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'learn-390-initial.png'), fullPage: false });
  console.log('Captured: learn-390-initial.png');

  // Check TOC / Outline button visibility on 390px
  const tocBtn = page.locator('button:has-text("Mục lục")').first();
  const isTocVisible390 = await tocBtn.isVisible().catch(() => false);
  console.log('Mục lục (TOC) button visible on 390px:', isTocVisible390);

  // Check Study Drawer toggle (Sổ tay & Thẻ)
  const drawerBtn = page.locator('button:has-text("Sổ tay & Thẻ"), button:has-text("Sổ tay"), button[aria-label*="sổ tay" i]').first();
  const isDrawerBtnVisible = await drawerBtn.isVisible().catch(() => false);
  console.log('Drawer Toggle button visible on 390px:', isDrawerBtnVisible);

  let drawerBox = null;
  if (isDrawerBtnVisible) {
    drawerBox = await drawerBtn.boundingBox();
    console.log('Drawer Toggle button dimensions:', drawerBox);

    console.log('Clicking drawer toggle to open Study Drawer...');
    await drawerBtn.click();
    await page.waitForTimeout(600);
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'learn-390-drawer-opened.png'), fullPage: false });
    console.log('Captured: learn-390-drawer-opened.png');

    // Type note into notebook
    const noteTitleInput = page.locator('input[placeholder*="Tiêu đề sổ tay"]').first();
    if (await noteTitleInput.isVisible().catch(() => false)) {
      await noteTitleInput.fill('Ghi chú bài 1 - Học trên Mobile 390px');
    }
    const noteTextarea = page.locator('textarea').first();
    if (await noteTextarea.isVisible().catch(() => false)) {
      await noteTextarea.fill('Cụm từ quan trọng:\n- How are you doing today? -> Pretty good!\n- Nhấn mạnh ngữ điệu tự nhiên.');
      await page.waitForTimeout(400);
      await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'learn-390-drawer-note-typed.png'), fullPage: false });
      console.log('Captured: learn-390-drawer-note-typed.png');
    }

    // Close drawer
    const closeDrawerBtn = page.locator('button[aria-label="Đóng ngăn ghi chú"]').first();
    if (await closeDrawerBtn.isVisible().catch(() => false)) {
      await closeDrawerBtn.click();
      await page.waitForTimeout(400);
    }
  }

  // Switch tabs: Bài giảng, Thực hành, Song song
  console.log('Testing Center Tabs on 390x844...');
  const tabLesson = page.locator('[data-testid="center-tab-lesson"]');
  const tabPractice = page.locator('[data-testid="center-tab-practice"]');
  const tabPdf = page.locator('[data-testid="center-tab-pdf"]');
  const tabSplit = page.locator('[data-testid="center-tab-split"]');

  console.log('Tab Bài giảng visible:', await tabLesson.isVisible().catch(() => false));
  console.log('Tab Thực hành visible:', await tabPractice.isVisible().catch(() => false));
  console.log('Tab PDF visible:', await tabPdf.isVisible().catch(() => false));
  console.log('Tab Song song visible:', await tabSplit.isVisible().catch(() => false));

  if (await tabPractice.isVisible().catch(() => false)) {
    await tabPractice.click();
    await page.waitForTimeout(600);
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'learn-390-tab-practice.png'), fullPage: false });
    console.log('Captured: learn-390-tab-practice.png');
  }

  if (await tabSplit.isVisible().catch(() => false)) {
    await tabSplit.click();
    await page.waitForTimeout(600);
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'learn-390-tab-split.png'), fullPage: false });
    console.log('Captured: learn-390-tab-split.png');
  }

  // 4. Test Learn Workspace on Tablet (768x1024)
  console.log('4. Testing Learn Workspace on Tablet 768x1024...');
  const tabletContext = await browser.newContext({
    viewport: { width: 768, height: 1024 },
    isMobile: true,
    hasTouch: true,
    userAgent: 'Mozilla/5.0 (iPad; CPU OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1',
  });
  const tabletPage = await tabletContext.newPage();
  await tabletPage.goto('http://127.0.0.1:3000', { waitUntil: 'networkidle' });
  await tabletPage.evaluate(({ userObj }) => {
    window.localStorage.setItem('swms_has_refresh_cookie', '1');
    window.localStorage.setItem('swms_active_user_id', userObj.id);
    window.localStorage.setItem('swms_active_user', JSON.stringify({
      id: userObj.id,
      fullName: userObj.fullName,
      email: userObj.email,
      authProvider: 'local',
      avatarUrl: null,
      hasLocalPassword: true,
    }));
  }, { userObj });

  await tabletPage.goto(`http://127.0.0.1:3000/app/courses/${courseId}/learn`, { waitUntil: 'networkidle' });
  await tabletPage.waitForTimeout(1000);
  await tabletPage.screenshot({ path: path.join(SCREENSHOT_DIR, 'learn-768-initial.png'), fullPage: false });
  console.log('Captured: learn-768-initial.png');

  const isTocVisible768 = await tabletPage.locator('button:has-text("Mục lục")').first().isVisible().catch(() => false);
  console.log('Mục lục (TOC) button visible on 768px tablet:', isTocVisible768);

  const tabletTabSplit = tabletPage.locator('[data-testid="center-tab-split"]');
  if (await tabletTabSplit.isVisible().catch(() => false)) {
    await tabletTabSplit.click();
    await tabletPage.waitForTimeout(600);
    await tabletPage.screenshot({ path: path.join(SCREENSHOT_DIR, 'learn-768-tab-split.png'), fullPage: false });
    console.log('Captured: learn-768-tab-split.png');
  }

  // 5. Test /app/review on Mobile 390x844 (One-handed ergonomics)
  console.log('5. Testing /app/review on 390x844...');
  await page.goto('http://127.0.0.1:3000/app/review', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'review-390-card-front.png'), fullPage: false });
  console.log('Captured: review-390-card-front.png');

  const flipBtn = page.locator('[data-testid="review-flip-btn"]');
  const isFlipVisible = await flipBtn.isVisible().catch(() => false);
  console.log('Flip button visible:', isFlipVisible);

  const ratingButtons = [];
  if (isFlipVisible) {
    const flipBox = await flipBtn.boundingBox();
    console.log('Flip button bounding box:', flipBox);

    await flipBtn.click();
    await page.waitForTimeout(500);
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'review-390-card-flipped.png'), fullPage: false });
    console.log('Captured: review-390-card-flipped.png');

    for (let i = 1; i <= 4; i++) {
      const btn = page.locator(`[data-testid="review-rate-${i}"]`);
      const visible = await btn.isVisible().catch(() => false);
      const box = await btn.boundingBox().catch(() => null);
      ratingButtons.push({
        index: i,
        visible,
        box,
        distFromScreenBottom: box ? Math.round(844 - (box.y + box.height)) : null,
      });
      console.log(`Rating button ${i}:`, box);
    }

    // Rate card with "Good" (button 3)
    console.log('Clicking rating button 3 (Good)...');
    await page.click('[data-testid="review-rate-3"]');
    await page.waitForTimeout(800);
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'review-390-after-rate.png'), fullPage: false });
    console.log('Captured: review-390-after-rate.png');
  }

  // 6. Test /app/courses/[id]/quiz on Mobile 390x844
  console.log(`6. Testing /app/courses/${courseId}/quiz on 390x844...`);
  await page.goto(`http://127.0.0.1:3000/app/courses/${courseId}/quiz`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-page.png'), fullPage: false });
  console.log('Captured: quiz-390-page.png');

  // Submit an answer if items exist
  const choiceBtn = page.locator('button[data-testid^="quiz-choice-"], label:has(input[type="radio"])').first();
  if (await choiceBtn.isVisible().catch(() => false)) {
    await choiceBtn.click();
    await page.waitForTimeout(300);
    const submitBtn = page.locator('button:has-text("Nộp bài"), button:has-text("Gửi câu trả lời"), button[type="submit"]').first();
    if (await submitBtn.isVisible().catch(() => false)) {
      await submitBtn.click();
      await page.waitForTimeout(1000);
      await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-submitted.png'), fullPage: false });
      console.log('Captured: quiz-390-submitted.png');
    }
  }

  // 7. Test /workspace (Workspace Classic) on 390x844 & 768x1024
  console.log('7. Testing /workspace (Workspace Classic) on 390x844 & 768x1024...');
  await page.goto('http://127.0.0.1:3000/workspace', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'workspace-classic-390.png'), fullPage: false });
  console.log('Captured: workspace-classic-390.png');

  // Check avatar menu dropdown
  const avatarBtn = page.locator('[data-testid="user-avatar-btn"], button[title*="Tài khoản"]').first();
  const isAvatarVisible = await avatarBtn.isVisible().catch(() => false);
  console.log('Avatar button visible on /workspace 390px:', isAvatarVisible);

  let appV2LinkInDropdown = false;
  if (isAvatarVisible) {
    await avatarBtn.click();
    await page.waitForTimeout(400);
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'workspace-classic-avatar-dropdown-390.png'), fullPage: false });
    console.log('Captured: workspace-classic-avatar-dropdown-390.png');

    const appV2Link = page.locator('[data-testid="switch-to-app-v2-link"]');
    appV2LinkInDropdown = await appV2Link.isVisible().catch(() => false);
    console.log('switch-to-app-v2-link visible in avatar dropdown:', appV2LinkInDropdown);
  }

  // Tablet workspace classic
  await tabletPage.goto('http://127.0.0.1:3000/workspace', { waitUntil: 'networkidle' });
  await tabletPage.waitForTimeout(1000);
  await tabletPage.screenshot({ path: path.join(SCREENSHOT_DIR, 'workspace-classic-768.png'), fullPage: false });
  console.log('Captured: workspace-classic-768.png');

  // Save learning experience results
  const experienceResults = {
    testEmail,
    courseId,
    firstLessonId,
    quizId,
    isTocVisible390,
    isTocVisible768,
    isDrawerBtnVisible,
    drawerBox,
    ratingButtons,
    appV2LinkInDropdown,
  };

  fs.writeFileSync(
    path.join(__dirname, 'core-learning-results.json'),
    JSON.stringify(experienceResults, null, 2),
    'utf-8'
  );

  console.log('=== COMPLETED 03: CORE LEARNING EXPERIENCE ===');
  await mobileContext.close();
  await tabletContext.close();
  await browser.close();
}

runCoreLearningExperience().catch(err => {
  console.error('Core learning experience failed:', err);
  process.exit(1);
});
