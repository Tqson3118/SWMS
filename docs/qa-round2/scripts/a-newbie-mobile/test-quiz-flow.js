const { chromium } = require('../../../../frontend/node_modules/playwright');
const path = require('path');
const fs = require('fs');

const SCREENSHOT_DIR = path.resolve(__dirname, '../../screenshots/a-newbie-mobile');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true });
  await page.goto('http://127.0.0.1:3000', { waitUntil: 'networkidle' });

  // Register user via API
  const ts = Date.now();
  const email = `qa+quiz-user-${ts}@test.local`;
  const password = 'Password123!';
  const authData = await page.evaluate(async ({ email, password }) => {
    const res = await fetch('/api/v1/auth/register', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password, fullName: 'Học Viên Quiz' }),
    });
    return await res.json();
  }, { email, password });

  const token = authData.accessToken;
  const user = authData.user;

  // Set session in localStorage
  await page.evaluate(({ user }) => {
    window.localStorage.setItem('swms_has_refresh_cookie', '1');
    window.localStorage.setItem('swms_active_user_id', user.id);
    window.localStorage.setItem('swms_active_user', JSON.stringify(user));
  }, { user });

  // Create course with quiz
  const courseData = await page.evaluate(async (token) => {
    const payload = {
      topic: 'Tiếng Anh Nhập Môn Di Động',
      title: 'Tiếng Anh Giao Tiếp Cơ Bản',
      description: 'Khóa học thực hành tiếng Anh một tay trên thiết bị di động.',
      chapters: [
        {
          title: 'Chương 1: Chào hỏi',
          sortOrder: 1,
          lessons: [
            {
              title: 'Bài 1: Chào hỏi hàng ngày',
              summary: 'Các mẫu câu giao tiếp cơ bản: Hello, How are you doing today?',
              estimatedMinutes: 5,
            },
          ],
        },
      ],
      idempotencyKey: crypto.randomUUID(),
    };
    const res = await fetch('/api/v1/courses/generate-wizard/commit', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
      body: JSON.stringify(payload),
    });
    return await res.json();
  }, token);

  const courseId = courseData.id || courseData.courseId;
  console.log('Created course:', courseId);

  // Navigate to quiz
  await page.evaluate((cid) => {
    window.location.pathname = `/app/courses/${cid}/quiz`;
  }, courseId);

  await page.waitForTimeout(1500);
  console.log('URL at quiz:', page.url());
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-overview.png'), fullPage: false });

  // Select first radio option
  const radio = page.locator('input[type="radio"]').first();
  console.log('Radio exists:', await radio.count());
  await radio.check({ force: true });
  await page.waitForTimeout(400);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-choice-selected.png'), fullPage: false });
  console.log('Captured: quiz-390-choice-selected.png');

  // Submit button "Kiểm tra"
  const submitBtn = page.locator('button', { hasText: 'Kiểm tra' }).first();
  console.log('Submit button enabled:', await submitBtn.isEnabled());
  await submitBtn.click();
  await page.waitForTimeout(1500);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-submitted-result.png'), fullPage: false });
  console.log('Captured: quiz-390-submitted-result.png');

  await browser.close();
  console.log('Quiz test completed perfectly!');
})();
