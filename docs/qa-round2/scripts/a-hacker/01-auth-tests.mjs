// docs/qa-round2/scripts/a-hacker/01-auth-tests.mjs
// Test Suite 1: Authentication & Authorization Controls
import crypto from 'node:crypto';

const BASE_URL = 'http://127.0.0.1:5080';
const VICTIM_EMAIL = 'qa+victim-01@test.local';
const VICTIM_PASS = 'VictimPass@123';
const ATTACKER_EMAIL = 'qa+attacker-01@test.local';
const ATTACKER_PASS = 'AttackerPass@123';

const results = [];

function record(testId, name, status, details) {
  results.push({ testId, name, status, details });
  console.log(`[${status}] ${testId} - ${name}`);
  if (details) console.log(`   Details: ${details}`);
}

async function login(email, password) {
  const res = await fetch(`${BASE_URL}/api/v1/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
  const data = await res.json().catch(() => null);
  return { status: res.status, headers: res.headers, data };
}

async function run() {
  console.log('=== STARTING TEST SUITE 1: AUTHENTICATION ===\n');

  // 1.1 Brute-force / Rate Limiting Test
  console.log('--- 1.1 Brute-Force & Rate Limiting on Login ---');
  let rateLimited = false;
  let rapidAttempts = 25;
  let statusCodes = [];
  for (let i = 0; i < rapidAttempts; i++) {
    const res = await fetch(`${BASE_URL}/api/v1/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: VICTIM_EMAIL, password: `WrongPassword_${i}` })
    });
    statusCodes.push(res.status);
    if (res.status === 429) {
      rateLimited = true;
      break;
    }
  }
  if (!rateLimited) {
    record('AUTH-01', 'Rate Limiting on /api/v1/auth/login', 'VULNERABLE', 
      `Sent ${rapidAttempts} consecutive failed login attempts; all returned HTTP ${statusCodes[0]}. No 429 Too Many Requests was returned.`);
  } else {
    record('AUTH-01', 'Rate Limiting on /api/v1/auth/login', 'SECURE', 'Server throttled with 429.');
  }

  // 1.2 User Enumeration Test
  console.log('\n--- 1.2 User Enumeration ---');
  // Compare /login response for existing vs non-existing
  const loginExisting = await login(VICTIM_EMAIL, 'wrong_pass');
  const loginNonExisting = await login('qa+nonexistent-user-999@test.local', 'wrong_pass');
  const loginLeaks = loginExisting.status !== loginNonExisting.status || 
                     JSON.stringify(loginExisting.data) !== JSON.stringify(loginNonExisting.data);

  // Compare /register response for existing vs non-existing
  const regExistingRes = await fetch(`${BASE_URL}/api/v1/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: VICTIM_EMAIL, password: 'AnyPassword@123', fullName: 'Victim' })
  });
  const regExistingData = await regExistingRes.json().catch(() => null);

  const regFreshEmail = `qa+enum-check-${Date.now()}@test.local`;
  const regFreshRes = await fetch(`${BASE_URL}/api/v1/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: regFreshEmail, password: 'AnyPassword@123', fullName: 'Fresh' })
  });
  const regFreshData = await regFreshRes.json().catch(() => null);

  if (regExistingRes.status === 409 && regFreshRes.status === 200) {
    record('AUTH-02', 'User Enumeration via /api/v1/auth/register', 'VULNERABLE',
      `Registration returns HTTP 409 Conflict with message "${regExistingData?.error}" for existing emails, but HTTP 200 for new emails. Allows complete user base enumeration.`);
  } else {
    record('AUTH-02', 'User Enumeration via /api/v1/auth/register', 'SECURE', 'Registration does not differentiate existing users.');
  }

  if (loginLeaks) {
    record('AUTH-03', 'User Enumeration via /api/v1/auth/login', 'VULNERABLE',
      `Login responses differ between valid email (${loginExisting.status}) and invalid email (${loginNonExisting.status}).`);
  } else {
    record('AUTH-03', 'User Enumeration via /api/v1/auth/login', 'SECURE',
      `Login returns uniform HTTP ${loginExisting.status} with empty body for both existing and non-existing accounts.`);
  }

  // 1.3 Password Strength / Complexity Policy
  console.log('\n--- 1.3 Password Strength Policy ---');
  const weakPasswordEmail = `qa+weakpass-${Date.now()}@test.local`;
  const weakPassRes = await fetch(`${BASE_URL}/api/v1/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: weakPasswordEmail, password: '123456', fullName: 'Weak Pass' })
  });
  const weakPassData = await weakPassRes.json().catch(() => null);
  if (weakPassRes.status === 200) {
    record('AUTH-04', 'Weak Password Policy (Trivial 6-character accepted)', 'VULNERABLE',
      `Accepted trivially weak password '123456' without complexity, character-class, or dictionary check.`);
  } else {
    record('AUTH-04', 'Weak Password Policy', 'SECURE', `Rejected weak password: ${JSON.stringify(weakPassData)}`);
  }

  // 1.4 Hardcoded JWT Secret Key / Token Forgery Test
  console.log('\n--- 1.4 JWT Forgery via Hardcoded Secret Key ---');
  // From appsettings.json: "SWMS-v1-Enterprise-Jwt-HmacSha256-Secret-Key-2026-Studxy-Workspace!"
  const KNOWN_SECRET = 'SWMS-v1-Enterprise-Jwt-HmacSha256-Secret-Key-2026-Studxy-Workspace!';
  
  // Victim's user ID from login
  const victimLogin = await login(VICTIM_EMAIL, VICTIM_PASS);
  const victimId = victimLogin.data?.user?.id;
  const victimTokenVer = victimLogin.data?.user?.tokenVersion || 1;

  function base64UrlEncode(str) {
    return Buffer.from(str)
      .toString('base64')
      .replace(/=/g, '')
      .replace(/\+/g, '-')
      .replace(/\//g, '_');
  }

  function createJwt(header, payload, secret) {
    const encodedHeader = base64UrlEncode(JSON.stringify(header));
    const encodedPayload = base64UrlEncode(JSON.stringify(payload));
    const dataToSign = `${encodedHeader}.${encodedPayload}`;
    const hmac = crypto.createHmac('sha256', secret);
    hmac.update(dataToSign);
    const signature = hmac.digest('base64')
      .replace(/=/g, '')
      .replace(/\+/g, '-')
      .replace(/\//g, '_');
    return `${dataToSign}.${signature}`;
  }

  // Craft forged token impersonating victim as admin
  const forgedPayload = {
    sub: victimId,
    email: VICTIM_EMAIL,
    name: 'Victim Forged',
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'admin',
    ver: String(victimTokenVer),
    exp: Math.floor(Date.now() / 1000) + 3600,
    iss: 'https://api.studxy.vn',
    aud: 'https://app.studxy.vn'
  };

  const forgedToken = createJwt({ alg: 'HS256', typ: 'JWT' }, forgedPayload, KNOWN_SECRET);
  const forgedRes = await fetch(`${BASE_URL}/api/v1/auth/me`, {
    headers: { Authorization: `Bearer ${forgedToken}` }
  });
  const forgedData = await forgedRes.json().catch(() => null);

  if (forgedRes.status === 200 && forgedData?.id === victimId) {
    record('AUTH-05', 'CRITICAL: Arbitrary JWT Forgery via Hardcoded Secret', 'VULNERABLE',
      `Successfully forged a valid JWT for victim (${victimId}) with admin role using the hardcoded signing key in appsettings.json. Server returned 200 OK.`);
  } else {
    record('AUTH-05', 'Arbitrary JWT Forgery via Hardcoded Secret', 'SECURE',
      `Server rejected forged token: status ${forgedRes.status}`);
  }

  // 1.5 JWT alg: "none" Bypass Test
  console.log('\n--- 1.5 JWT alg: none Bypass ---');
  const algNoneToken = `${base64UrlEncode(JSON.stringify({ alg: 'none', typ: 'JWT' }))}.${base64UrlEncode(JSON.stringify(forgedPayload))}.`;
  const algNoneRes = await fetch(`${BASE_URL}/api/v1/auth/me`, {
    headers: { Authorization: `Bearer ${algNoneToken}` }
  });
  if (algNoneRes.status === 200) {
    record('AUTH-06', 'JWT alg: none Signature Bypass', 'VULNERABLE', 'Server accepted unsigned JWT with alg: none!');
  } else {
    record('AUTH-06', 'JWT alg: none Signature Bypass', 'SECURE', `Server rejected alg: none with status ${algNoneRes.status}.`);
  }

  // 1.6 Token Revocation on Logout
  console.log('\n--- 1.6 Token Invalidation After Logout ---');
  const freshLogin = await login(VICTIM_EMAIL, VICTIM_PASS);
  const freshAccessToken = freshLogin.data.accessToken;

  // Verify access token works
  const preLogout = await fetch(`${BASE_URL}/api/v1/auth/me`, {
    headers: { Authorization: `Bearer ${freshAccessToken}` }
  });
  // Now logout
  const logoutRes = await fetch(`${BASE_URL}/api/v1/auth/logout`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${freshAccessToken}` }
  });

  // Try using same access token post logout
  const postLogout = await fetch(`${BASE_URL}/api/v1/auth/me`, {
    headers: { Authorization: `Bearer ${freshAccessToken}` }
  });
  const postLogoutData = await postLogout.json().catch(() => null);

  if (postLogout.status === 401 && postLogoutData?.error === 'TOKEN_VERSION_REVOKED') {
    record('AUTH-07', 'Access Token Revocation on Logout', 'SECURE',
      'Token version check immediately invalidated access token upon logout (HTTP 401 TOKEN_VERSION_REVOKED).');
  } else if (postLogout.status === 200) {
    record('AUTH-07', 'Access Token Revocation on Logout', 'VULNERABLE',
      'Access token remains valid after logout!');
  } else {
    record('AUTH-07', 'Access Token Revocation on Logout', 'INFO',
      `Returned HTTP ${postLogout.status}: ${JSON.stringify(postLogoutData)}`);
  }

  // 1.7 Refresh Token Rotation (RTR) & Replay Breach Alarm
  console.log('\n--- 1.7 Refresh Token Rotation & Replay Attack ---');
  // Login again to get a fresh refresh token
  const rtrLogin = await login(VICTIM_EMAIL, VICTIM_PASS);
  const initialRefresh = rtrLogin.data.refreshToken;

  // 1st rotation: exchange initialRefresh for token2
  const rotate1 = await fetch(`${BASE_URL}/api/v1/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: initialRefresh })
  });
  const rotate1Data = await rotate1.json().catch(() => null);
  const token2 = rotate1Data?.refreshToken;

  // 2nd call: replay initialRefresh (should trigger 30s idempotent grace recovery)
  const replayGrace = await fetch(`${BASE_URL}/api/v1/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: initialRefresh })
  });
  const replayGraceData = await replayGrace.json().catch(() => null);

  const isGraceRecovered = replayGraceData?.graceWindowRecovered === true;
  if (isGraceRecovered) {
    record('AUTH-08', 'RTR 30s Grace Window Idempotency', 'SECURE',
      'Replaying recently rotated refresh token within grace window successfully returned recovered replacement token.');
  } else {
    record('AUTH-08', 'RTR 30s Grace Window Idempotency', 'INFO',
      `Replay within grace window result: status ${replayGrace.status}`);
  }

  console.log('\n=== TEST SUITE 1 SUMMARY ===');
  console.table(results);
}

run().catch(console.error);
