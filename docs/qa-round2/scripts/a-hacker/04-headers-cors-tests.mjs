// docs/qa-round2/scripts/a-hacker/04-headers-cors-tests.mjs
// Test Suite 4: HTTP Security Headers & CORS Policy

const BACKEND_URL = 'http://127.0.0.1:5080';
const FRONTEND_URL = 'http://127.0.0.1:3000';

const results = [];

function record(testId, name, status, details) {
  results.push({ testId, name, status, details });
  console.log(`[${status}] ${testId} - ${name}`);
  if (details) console.log(`   Details: ${details}`);
}

async function run() {
  console.log('=== STARTING TEST SUITE 4: HTTP HEADERS & CORS ===\n');

  // 4.1 Backend (Port 5080) Headers
  console.log('--- 4.1 Backend (5080) Security Headers ---');
  const beRes = await fetch(`${BACKEND_URL}/health`);
  const beHeaders = Object.fromEntries(beRes.headers.entries());

  if (!beHeaders['x-content-type-options']) {
    record('HDR-01', 'Backend missing X-Content-Type-Options', 'VULNERABLE',
      'X-Content-Type-Options header is absent on backend response.');
  } else {
    record('HDR-01', 'Backend X-Content-Type-Options', 'SECURE', beHeaders['x-content-type-options']);
  }

  if (!beHeaders['content-security-policy']) {
    record('HDR-02', 'Backend missing Content-Security-Policy', 'VULNERABLE',
      'Content-Security-Policy is absent on backend.');
  } else {
    record('HDR-02', 'Backend Content-Security-Policy', 'SECURE', 'CSP header present.');
  }

  if (beHeaders['server']) {
    record('HDR-03', 'Backend Server Banner Information Disclosure', 'VULNERABLE',
      `Server banner leaked: "${beHeaders['server']}". Attackers identify Kestrel web server.`);
  } else {
    record('HDR-03', 'Backend Server Banner Information Disclosure', 'SECURE', 'Server header stripped.');
  }

  // 4.2 Frontend (Port 3000) Headers
  console.log('\n--- 4.2 Frontend (3000) Security Headers ---');
  const feRes = await fetch(`${FRONTEND_URL}/auth`);
  const feHeaders = Object.fromEntries(feRes.headers.entries());

  if (!feHeaders['x-frame-options'] && !feHeaders['content-security-policy']?.includes('frame-ancestors')) {
    record('HDR-04', 'Frontend missing Clickjacking Protection (X-Frame-Options/CSP frame-ancestors)', 'VULNERABLE',
      'Neither X-Frame-Options nor CSP frame-ancestors is present on Frontend pages. Application can be framed in an iframe for Clickjacking attacks.');
  } else {
    record('HDR-04', 'Frontend Clickjacking Protection', 'SECURE', 'Clickjacking protection present.');
  }

  if (!feHeaders['content-security-policy']) {
    record('HDR-05', 'Frontend missing Content-Security-Policy (CSP)', 'VULNERABLE',
      'CSP header is absent on frontend pages. XSS payloads have unrestricted execution and exfiltration scope.');
  } else {
    record('HDR-05', 'Frontend Content-Security-Policy (CSP)', 'SECURE', 'CSP header present.');
  }

  if (!feHeaders['x-content-type-options']) {
    record('HDR-06', 'Frontend missing X-Content-Type-Options', 'VULNERABLE',
      'X-Content-Type-Options header is absent on frontend.');
  } else {
    record('HDR-06', 'Frontend X-Content-Type-Options', 'SECURE', feHeaders['x-content-type-options']);
  }

  if (feHeaders['x-powered-by']) {
    record('HDR-07', 'Frontend X-Powered-By Banner Leak', 'VULNERABLE',
      `Leaking technology stack: "${feHeaders['x-powered-by']}".`);
  } else {
    record('HDR-07', 'Frontend X-Powered-By Banner Leak', 'SECURE', 'X-Powered-By stripped.');
  }

  // 4.3 CORS Configuration on Backend
  console.log('\n--- 4.3 CORS Policy Verification ---');
  // Test with Evil Origin
  const evilCorsRes = await fetch(`${BACKEND_URL}/api/v1/auth/providers`, {
    headers: { Origin: 'https://evil-attacker.com' }
  });
  const evilAcao = evilCorsRes.headers.get('access-control-allow-origin');
  const evilAcac = evilCorsRes.headers.get('access-control-allow-credentials');

  if (evilAcao === '*' || evilAcao === 'https://evil-attacker.com') {
    record('CORS-01', 'Backend CORS Wildcard / Arbitrary Origin Reflection', 'VULNERABLE',
      `Backend reflected or allowed evil origin! ACAO: ${evilAcao}, ACAC: ${evilAcac}`);
  } else {
    record('CORS-01', 'Backend CORS Policy against Evil Origin', 'SECURE',
      'Backend correctly omitted Access-Control-Allow-Origin for untrusted origin (https://evil-attacker.com).');
  }

  // Test with Valid Origin
  const validCorsRes = await fetch(`${BACKEND_URL}/api/v1/auth/providers`, {
    headers: { Origin: 'http://localhost:3000' }
  });
  const validAcao = validCorsRes.headers.get('access-control-allow-origin');
  const validAcac = validCorsRes.headers.get('access-control-allow-credentials');

  if (validAcao === 'http://localhost:3000' && validAcac === 'true') {
    record('CORS-02', 'Backend Whitelist Origin with Credentials', 'SECURE',
      'Backend properly allows whitelisted origin http://localhost:3000 with Allow-Credentials: true.');
  } else {
    record('CORS-02', 'Backend Whitelist Origin with Credentials', 'INFO',
      `ACAO: ${validAcao}, ACAC: ${validAcac}`);
  }

  // Test null origin
  const nullCorsRes = await fetch(`${BACKEND_URL}/api/v1/auth/providers`, {
    headers: { Origin: 'null' }
  });
  const nullAcao = nullCorsRes.headers.get('access-control-allow-origin');
  if (nullAcao === 'null' || nullAcao === '*') {
    record('CORS-03', 'Backend CORS Null Origin Bypass', 'VULNERABLE',
      `Backend permitted null origin: ACAO = ${nullAcao}`);
  } else {
    record('CORS-03', 'Backend CORS Null Origin Bypass', 'SECURE',
      'Backend correctly denied null origin.');
  }

  console.log('\n=== TEST SUITE 4 SUMMARY ===');
  console.table(results);
}

run().catch(console.error);
