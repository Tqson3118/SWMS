const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../../frontend/node_modules/@playwright/test'));

const BASE_URL = process.env.SWMS_FRONTEND_URL || 'http://127.0.0.1:3000';
const API_URL = process.env.SWMS_API_URL || 'http://127.0.0.1:5080/api/v1';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../../screenshots/a-student');

if (!fs.existsSync(SCREENSHOTS_DIR)) {
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });
}

const DEFAULT_VIEWPORT = { width: 1440, height: 900 };

async function createBrowserContext(browser, options = {}) {
  const context = await browser.newContext({
    viewport: options.viewport || DEFAULT_VIEWPORT,
    isMobile: false,
    ...options
  });
  return context;
}

function attachDiagnostics(page, testName, diagnostics = { consoleErrors: [], pageErrors: [] }) {
  page.on('console', (msg) => {
    if (msg.type() === 'error') {
      const text = msg.text();
      // Ignore routine favicon or benign dev notices if any
      diagnostics.consoleErrors.push({
        test: testName,
        url: page.url(),
        text: text.slice(0, 300)
      });
    }
  });

  page.on('pageerror', (err) => {
    diagnostics.pageErrors.push({
      test: testName,
      url: page.url(),
      message: err.message,
      stack: (err.stack || '').slice(0, 400)
    });
  });

  return diagnostics;
}

async function safeScreenshot(page, filename, description = '') {
  const targetPath = path.join(SCREENSHOTS_DIR, filename);
  await page.waitForTimeout(300);
  await page.screenshot({ path: targetPath, fullPage: true });
  console.log(`[SCREENSHOT] Saved: ${filename}${description ? ` - ${description}` : ''}`);
  return targetPath;
}

async function loginDemoSonTran(page) {
  await page.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('[data-testid="demo-switcher-toggle"]', { timeout: 10000 });
  
  const defaultBtn = page.locator('[data-testid="switch-user-default"]');
  if (!(await defaultBtn.isVisible())) {
    await page.click('[data-testid="demo-switcher-toggle"]');
    await defaultBtn.waitFor({ state: 'visible', timeout: 5000 });
  }

  await defaultBtn.click();
  await page.waitForURL('**/app**', { timeout: 15000 });
  await page.waitForTimeout(800);
}

async function registerStudentUser(suffix = Date.now()) {
  const email = `qa+student-${suffix}@test.local`;
  const password = 'StudentPass123!';
  const fullName = `Sinh Viên QA ${suffix}`;

  const res = await fetch(`${API_URL}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password, fullName })
  });

  const data = await res.json();
  if (!res.ok) {
    if (res.status === 409) {
      return await loginStudentUser(email, password);
    }
    throw new Error(`Failed to register ${email}: ${res.status} ${JSON.stringify(data)}`);
  }

  return {
    email,
    password,
    fullName,
    userId: data.user.id,
    accessToken: data.accessToken,
    user: data.user
  };
}

async function loginStudentUser(email, password = 'StudentPass123!') {
  const res = await fetch(`${API_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });

  const data = await res.json();
  if (!res.ok) {
    throw new Error(`Failed to login ${email}: ${res.status} ${JSON.stringify(data)}`);
  }

  return {
    email,
    password,
    userId: data.user.id,
    accessToken: data.accessToken,
    user: data.user
  };
}

async function registerViaUi(page, timestamp = Date.now()) {
  const email = `qa+student-${timestamp}@test.local`;
  const password = 'StudentPass123!';
  const fullName = `Sinh Viên QA ${timestamp}`;

  await page.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('[data-testid="auth-tab-register"]', { timeout: 10000 });
  await page.click('[data-testid="auth-tab-register"]');
  await page.waitForTimeout(300);

  await page.fill('[data-testid="auth-fullname-input"]', fullName);
  await page.fill('[data-testid="auth-email-input"]', email);
  await page.fill('[data-testid="auth-password-input"]', password);

  await page.click('[data-testid="auth-submit-btn"]');
  await page.waitForURL('**/app**', { timeout: 15000 });
  await page.waitForTimeout(800);

  const activeUserId = await page.evaluate(() => localStorage.getItem('swms_active_user_id'));
  const activeUser = await page.evaluate(() => {
    const raw = localStorage.getItem('swms_active_user');
    return raw ? JSON.parse(raw) : null;
  });

  return {
    email,
    password,
    fullName,
    userId: activeUserId || activeUser?.id,
    user: activeUser
  };
}

async function loginViaUi(page, email, password) {
  await page.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('[data-testid="auth-email-input"]', { timeout: 10000 });

  await page.fill('[data-testid="auth-email-input"]', email);
  await page.fill('[data-testid="auth-password-input"]', password);

  await page.click('[data-testid="auth-submit-btn"]');
  await page.waitForURL('**/app**', { timeout: 15000 });
  await page.waitForTimeout(800);
}

module.exports = {
  BASE_URL,
  API_URL,
  SCREENSHOTS_DIR,
  DEFAULT_VIEWPORT,
  chromium,
  createBrowserContext,
  attachDiagnostics,
  safeScreenshot,
  loginDemoSonTran,
  registerStudentUser,
  loginStudentUser,
  registerViaUi,
  loginViaUi
};
