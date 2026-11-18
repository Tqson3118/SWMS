// Common test helpers for A-ABUSER QA suite
const BASE_URL = process.env.SWMS_API_URL || 'http://127.0.0.1:5080/api/v1';

async function registerTestUser(suffix, role = 'abuser') {
  const email = `qa+${role}-${suffix}@test.local`;
  const password = 'Password123!';
  const fullName = `QA ${role.toUpperCase()} ${suffix}`;

  const res = await fetch(`${BASE_URL}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password, fullName })
  });

  const data = await res.json();
  if (!res.ok) {
    // If user already exists, try logging in
    if (res.status === 409) {
      return await loginTestUser(email, password);
    }
    throw new Error(`Failed to register ${email}: ${res.status} ${JSON.stringify(data)}`);
  }

  return {
    email,
    password,
    userId: data.user.id,
    accessToken: data.accessToken,
    personalWorkspaceCourseId: data.personalWorkspaceCourseId,
    user: data.user
  };
}

async function loginTestUser(email, password = 'Password123!') {
  const res = await fetch(`${BASE_URL}/auth/login`, {
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
    personalWorkspaceCourseId: data.personalWorkspaceCourseId,
    user: data.user
  };
}

function authHeaders(token, extra = {}) {
  return {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`,
    ...extra
  };
}

module.exports = {
  BASE_URL,
  registerTestUser,
  loginTestUser,
  authHeaders
};
