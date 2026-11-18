// docs/qa-round2/scripts/a-hacker/05-mass-assignment-tests.mjs
// Test Suite 5: Mass Assignment Vulnerability Testing

const BASE_URL = 'http://127.0.0.1:5080';
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
  const data = await res.json();
  return data.accessToken;
}

async function run() {
  console.log('=== STARTING TEST SUITE 5: MASS ASSIGNMENT ===\n');

  // 5.1 Mass Assignment on Registration (Attempting Privilege Escalation)
  console.log('--- 5.1 Registration Mass Assignment (Role: Admin) ---');
  const massAssignEmail = `qa+massassign-${Date.now()}@test.local`;
  const regRes = await fetch(`${BASE_URL}/api/v1/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email: massAssignEmail,
      password: 'StrongPassword@123',
      fullName: 'Mass Assign Tester',
      role: 'admin',
      isAdmin: true,
      tier: 'enterprise',
      tokenVersion: 99,
      streakDays: 365,
      dailyAiCreditLimit: 10000
    })
  });
  const regData = await regRes.json();

  if (regData?.user?.role === 'admin' || regData?.user?.streakDays === 365) {
    record('MASS-01', 'Privilege Escalation via Register Mass Assignment', 'VULNERABLE',
      `User registered with elevated role "${regData.user.role}" or injected attributes!`);
  } else if (regData?.user?.role === 'student' && regData?.user?.streakDays === 1) {
    record('MASS-01', 'Privilege Escalation via Register Mass Assignment', 'SECURE',
      `Server bound only defined DTO fields (Email, Password, FullName) and explicitly hardcoded role to "student" (streakDays=1).`);
  } else {
    record('MASS-01', 'Privilege Escalation via Register Mass Assignment', 'INFO',
      JSON.stringify(regData));
  }

  // 5.2 Mass Assignment on Onboarding
  console.log('\n--- 5.2 Onboarding Mass Assignment ---');
  const token = await login(ATTACKER_EMAIL, ATTACKER_PASS);
  const onbRes = await fetch(`${BASE_URL}/api/v1/workspace/onboarding`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      step: 2,
      role: 'admin',
      isAdmin: true,
      streakDays: 999
    })
  });
  const onbData = await onbRes.json();

  const meRes = await fetch(`${BASE_URL}/api/v1/auth/me`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  const meData = await meRes.json();

  if (meData?.role === 'admin' || meData?.streakDays === 999) {
    record('MASS-02', 'Onboarding Mass Assignment Attribute Injection', 'VULNERABLE',
      `Elevated role or modified streakDays: role=${meData.role}, streak=${meData.streakDays}`);
  } else {
    record('MASS-02', 'Onboarding Mass Assignment Attribute Injection', 'SECURE',
      `Injected role/isAdmin/streakDays ignored. User role remains "${meData.role}".`);
  }

  // 5.3 Mass Assignment on Course Creation / Ownership Transfer
  console.log('\n--- 5.3 Course Creation Ownership / Visibility Injection ---');
  const courseRes = await fetch(`${BASE_URL}/api/v1/courses/generate-wizard/commit`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      topic: 'Mass Assign Course',
      title: 'Course Mass Assign Test',
      instructorId: '11111111-1111-4111-8111-111111111111', // Try spoofing Son Tran
      visibility: 'public',
      isPublished: true,
      chapters: [
        {
          title: 'Ch 1',
          sortOrder: 1,
          lessons: [{ title: 'Les 1', summary: 'Summary', estimatedMinutes: 5 }]
        }
      ]
    })
  });
  const courseData = await courseRes.json();

  // Verify who actually owns the created course
  const checkCourseRes = await fetch(`${BASE_URL}/api/v1/courses/${courseData.id || courseData.courseId}`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  const checkCourseData = await checkCourseRes.json();

  // In CourseWizardAndImportService: course.InstructorId = userId (resolved from JWT)
  if (checkCourseData?.instructorId === '11111111-1111-4111-8111-111111111111') {
    record('MASS-03', 'Course Ownership Spoofing (instructorId injection)', 'VULNERABLE',
      'Attacker successfully set course instructor to victim!');
  } else {
    record('MASS-03', 'Course Ownership Spoofing (instructorId injection)', 'SECURE',
      'Course instructorId bound strictly to authenticated caller UserId resolved from JWT sub claim.');
  }

  // Cleanup
  if (courseData?.id || courseData?.courseId) {
    await fetch(`${BASE_URL}/api/v1/courses/${courseData.id || courseData.courseId}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  console.log('\n=== TEST SUITE 5 SUMMARY ===');
  console.table(results);
}

run().catch(console.error);
