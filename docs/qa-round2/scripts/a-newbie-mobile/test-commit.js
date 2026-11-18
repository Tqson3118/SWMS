const { chromium } = require('../../../../frontend/node_modules/playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  await page.goto('http://127.0.0.1:3000', { waitUntil: 'networkidle' });
  
  const ts = Date.now();
  const email = `qa+test-token-${ts}@test.local`;
  const password = 'Password123!';

  // Call /api/v1/auth/register to get valid Bearer token
  const registerRes = await page.evaluate(async ({ email, password }) => {
    const res = await fetch('/api/v1/auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password, fullName: 'Test User' })
    });
    return await res.json();
  }, { email, password });

  console.log('Register response keys:', Object.keys(registerRes));
  const token = registerRes.accessToken;
  console.log('Got accessToken:', Boolean(token));

  // Now call generate-wizard/commit with Bearer token!
  const commitRes = await page.evaluate(async (token) => {
    const payload = {
      topic: 'Tiếng Anh Nhập Môn',
      title: 'Tiếng Anh Giao Tiếp Cơ Bản',
      description: 'Mô tả khóa học cho người mới',
      chapters: [
        {
          title: 'Chương 1: Chào hỏi',
          sortOrder: 1,
          lessons: [
            {
              title: 'Bài 1: Làm quen',
              summary: 'Nội dung chào hỏi cơ bản',
              estimatedMinutes: 5
            }
          ]
        }
      ]
    };
    const res = await fetch('/api/v1/courses/generate-wizard/commit', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(payload)
    });
    const data = await res.json().catch(() => null);
    return { status: res.status, ok: res.ok, data };
  }, token);

  console.log('Wizard commit result with Bearer token:', commitRes);
  await browser.close();
})();
