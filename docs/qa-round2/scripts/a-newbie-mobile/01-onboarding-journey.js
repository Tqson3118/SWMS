const { chromium } = require('../../../../frontend/node_modules/playwright');
const path = require('path');
const fs = require('fs');

const SCREENSHOT_DIR = path.resolve(__dirname, '../../screenshots/a-newbie-mobile');
if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

async function runOnboardingJourney() {
  console.log('--- STARTING 01: ONBOARDING JOURNEY ---');
  const browser = await chromium.launch({ headless: true });
  
  // Mobile 390x844
  const context = await browser.newContext({
    viewport: { width: 390, height: 844 },
    isMobile: true,
    hasTouch: true,
    userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1',
  });
  const page = await context.newPage();

  const timestamp = Date.now();
  const testEmail = `qa+newbie-${timestamp}@test.local`;
  const testPassword = 'Password123!';
  const testFullName = 'Nguyễn Người Mới';

  const results = {
    testEmail,
    landingCtaUrl: '',
    postRegisterUrl: '',
    emptyStateAssessments: {},
    minhAnhEmptyStates: {},
    confusingTermsFound: [],
  };

  // 1. Visit landing page
  console.log('1. Navigating to http://127.0.0.1:3000/ ...');
  await page.goto('http://127.0.0.1:3000/', { waitUntil: 'networkidle' });
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '01-landing-390.png'), fullPage: false });
  console.log('Captured: 01-landing-390.png');

  // Check CTA button
  const heroCta = page.locator('[data-testid="landing-hero-cta"]');
  const heroCtaHref = await heroCta.getAttribute('href');
  console.log('Hero CTA href:', heroCtaHref);
  results.landingCtaUrl = heroCtaHref;

  // Click CTA
  console.log('Clicking landing-hero-cta...');
  await heroCta.click();
  await page.waitForLoadState('networkidle');
  console.log('Current URL after CTA click:', page.url());

  // Wait for redirect to /auth if landed on /workspace
  if (page.url().includes('/workspace')) {
    await page.waitForURL('**/auth**', { timeout: 10000 }).catch(() => {});
  }
  console.log('Auth page URL:', page.url());
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '02-auth-page-390.png'), fullPage: false });
  console.log('Captured: 02-auth-page-390.png');

  // 2. Switch to Register tab
  console.log('Switching to Register tab...');
  const registerTabBtn = page.locator('button', { hasText: 'Đăng ký' }).first();
  await registerTabBtn.click();
  await page.waitForTimeout(300);

  // Fill registration form
  console.log(`Registering account: ${testEmail}...`);
  await page.fill('#auth-fullname', testFullName);
  await page.fill('#auth-email', testEmail);
  await page.fill('#auth-password', testPassword);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '03-auth-register-form-390.png'), fullPage: false });

  // Submit registration
  await page.click('button[type="submit"]');
  console.log('Submitted registration form. Waiting for navigation...');
  await page.waitForNavigation({ timeout: 15000 }).catch(() => {});
  await page.waitForTimeout(2000);
  
  const postRegisterUrl = page.url();
  results.postRegisterUrl = postRegisterUrl;
  console.log('Post-registration URL:', postRegisterUrl);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '04-after-register-destination-390.png'), fullPage: false });

  // If redirected to /workspace instead of /app, navigate to /app
  console.log('Navigating explicitly to /app to evaluate Newbie Empty State...');
  await page.goto('http://127.0.0.1:3000/app', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '05-dashboard-empty-390.png'), fullPage: false });

  // Assess /app empty state text
  const dashboardContent = await page.textContent('body');
  results.emptyStateAssessments['/app'] = {
    title: await page.title(),
    hasWelcome: dashboardContent.includes('Bắt đầu hành trình học mới') || dashboardContent.includes('Tạo khóa học'),
    hasFirstStepsGuide: dashboardContent.includes('Bước 1') || dashboardContent.includes('1.') || dashboardContent.includes('Hướng dẫn'),
  };

  // 3. Inspect EmptyState on /app/courses
  console.log('Checking /app/courses empty state...');
  await page.goto('http://127.0.0.1:3000/app/courses', { waitUntil: 'networkidle' });
  await page.waitForTimeout(500);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '06-courses-empty-390.png'), fullPage: false });
  const coursesContent = await page.textContent('body');
  results.emptyStateAssessments['/app/courses'] = {
    hasEmptyMessage: coursesContent.includes('Không tìm thấy khóa học') || coursesContent.includes('Chưa có khóa học'),
  };

  // 4. Inspect EmptyState on /app/review
  console.log('Checking /app/review empty state...');
  await page.goto('http://127.0.0.1:3000/app/review', { waitUntil: 'networkidle' });
  await page.waitForTimeout(500);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '07-review-empty-390.png'), fullPage: false });
  const reviewContent = await page.textContent('body');
  results.emptyStateAssessments['/app/review'] = {
    hasEmptyMessage: reviewContent.includes('Không có thẻ nào') || reviewContent.includes('Hoàn thành mục tiêu') || reviewContent.includes('0 thẻ'),
  };

  // 5. Inspect EmptyState on /app/notes
  console.log('Checking /app/notes empty state...');
  await page.goto('http://127.0.0.1:3000/app/notes', { waitUntil: 'networkidle' });
  await page.waitForTimeout(500);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '08-notes-empty-390.png'), fullPage: false });
  const notesContent = await page.textContent('body');
  results.emptyStateAssessments['/app/notes'] = {
    hasEmptyMessage: notesContent.includes('Chưa có ghi chú') || notesContent.includes('Tạo ghi chú'),
  };

  // 6. Inspect EmptyState on /app/documents
  console.log('Checking /app/documents empty state...');
  await page.goto('http://127.0.0.1:3000/app/documents', { waitUntil: 'networkidle' });
  await page.waitForTimeout(500);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '09-documents-empty-390.png'), fullPage: false });
  const docsContent = await page.textContent('body');
  results.emptyStateAssessments['/app/documents'] = {
    hasEmptyMessage: docsContent.includes('Chưa có tài liệu') || docsContent.includes('Tải lên PDF'),
  };

  // 7. Check Minh Anh demo account
  console.log('Checking demo account: Minh Anh...');
  await page.goto('http://127.0.0.1:3000/auth', { waitUntil: 'networkidle' });
  await page.click('[data-testid="demo-switcher-toggle"]');
  await page.waitForSelector('[data-testid="switch-user-secondary"]');
  await page.click('[data-testid="switch-user-secondary"]');
  await page.waitForURL('**/app**', { timeout: 15000 });
  await page.waitForTimeout(1000);

  console.log('Capturing Minh Anh dashboard...');
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '10-minhanh-dashboard-390.png'), fullPage: false });
  await page.goto('http://127.0.0.1:3000/app/courses', { waitUntil: 'networkidle' });
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '11-minhanh-courses-390.png'), fullPage: false });
  await page.goto('http://127.0.0.1:3000/app/review', { waitUntil: 'networkidle' });
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '12-minhanh-review-390.png'), fullPage: false });
  await page.goto('http://127.0.0.1:3000/app/notes', { waitUntil: 'networkidle' });
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '13-minhanh-notes-390.png'), fullPage: false });
  await page.goto('http://127.0.0.1:3000/app/documents', { waitUntil: 'networkidle' });
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, '14-minhanh-documents-390.png'), fullPage: false });

  // Save results json
  fs.writeFileSync(
    path.join(__dirname, 'onboarding-results.json'),
    JSON.stringify(results, null, 2),
    'utf-8'
  );

  console.log('--- COMPLETED 01: ONBOARDING JOURNEY ---');
  await browser.close();
}

runOnboardingJourney().catch((err) => {
  console.error('Error running onboarding journey:', err);
  process.exit(1);
});
