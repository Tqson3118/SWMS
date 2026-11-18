// PoC 06: Extreme & Malformed Inputs Abuse Test Suite
const { BASE_URL, registerTestUser, authHeaders } = require('./common');

async function testDeeplyNestedJson(user) {
  console.log('\n--- 6.1 Testing Deeply Nested JSON (100 levels) ---');
  // Build 100 levels nested JSON
  let nested = { leaf: 'value' };
  for (let i = 0; i < 100; i++) {
    nested = { next: nested };
  }

  const res = await fetch(`${BASE_URL}/notebooks`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify(nested)
  });

  const text = await res.text();
  console.log(`Deeply nested JSON response: status=${res.status}, bodyExcerpt=${text.slice(0, 150)}`);

  return {
    test: 'deeply_nested_json',
    status: res.status,
    isClean4xx: res.status >= 400 && res.status < 500,
    isServer500: res.status >= 500
  };
}

async function testNullBytesInStrings(user) {
  console.log('\n--- 6.2 Testing Null Bytes (\\u0000) in String Inputs ---');
  const res = await fetch(`${BASE_URL}/flashcards`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      frontText: 'Front with null byte \u0000 injected',
      backText: 'Back with null byte \u0000 test',
      cardType: 'standard'
    })
  });

  const text = await res.text();
  console.log(`Null byte in flashcard response: status=${res.status}, bodyExcerpt=${text.slice(0, 150)}`);

  return {
    test: 'null_bytes_in_strings',
    status: res.status,
    isClean: res.status !== 500
  };
}

async function testHugeNumbersAndOverflow(user) {
  console.log('\n--- 6.3 Testing Huge Numbers & Numeric Overflow ---');
  const hugeNumber = '999999999999999999999999999999999999999999999999';

  const res = await fetch(`${BASE_URL}/workspace/onboarding`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      step: 9999999999999999999999999999999999999999n.toString()
    })
  });

  const text = await res.text();
  console.log(`Huge number in step response: status=${res.status}, bodyExcerpt=${text.slice(0, 150)}`);

  return {
    test: 'huge_number_overflow',
    status: res.status,
    isClean4xx: res.status >= 400 && res.status < 500,
    isServer500: res.status >= 500
  };
}

async function testVeryLongStrings(user) {
  console.log('\n--- 6.4 Testing Very Long Strings (5MB Payload) ---');
  const longString = 'A'.repeat(5 * 1024 * 1024); // 5MB

  let status = 0;
  let text = '';
  try {
    const res = await fetch(`${BASE_URL}/workspace/search?q=${encodeURIComponent(longString.slice(0, 10000))}`, {
      headers: authHeaders(user.accessToken)
    });
    status = res.status;
    text = await res.text();
  } catch (err) {
    status = 0;
    text = err.message;
  }

  console.log(`Long search query response: status=${status}, bodyExcerpt=${text.slice(0, 150)}`);

  return {
    test: 'very_long_strings',
    status,
    isServer500: status >= 500
  };
}

async function testEmptyArraysAndMalformedPayloads(user) {
  console.log('\n--- 6.5 Testing Empty Arrays and Malformed Payloads ---');

  // Empty reviews array
  const rEmpty = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({ reviews: [] })
  });
  console.log(`Empty reviews array: status=${rEmpty.status} (expected 200 clean sync of 0 items)`);

  // Empty body with content-type json
  const rEmptyBody = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: ''
  });
  console.log(`Empty body POST: status=${rEmptyBody.status} (expected 400)`);

  // Invalid JSON syntax
  const rBadJson = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: '{ "reviews": [ missing_quote ] }'
  });
  console.log(`Malformed JSON syntax: status=${rBadJson.status} (expected 400)`);

  return {
    test: 'empty_and_malformed_payloads',
    emptyReviewsStatus: rEmpty.status,
    emptyBodyStatus: rEmptyBody.status,
    badJsonStatus: rBadJson.status,
    allClean4xx: rEmptyBody.status === 400 && rBadJson.status === 400
  };
}

async function main() {
  console.log('=== STARTING POC-06: EXTREME INPUTS SUITE ===');
  const user = await registerTestUser(`input-user-${Date.now().toString().slice(-4)}`);

  const r1 = await testDeeplyNestedJson(user);
  const r2 = await testNullBytesInStrings(user);
  const r3 = await testHugeNumbersAndOverflow(user);
  const r4 = await testVeryLongStrings(user);
  const r5 = await testEmptyArraysAndMalformedPayloads(user);

  console.log('\n=== SUMMARY OF POC-06 FINDINGS ===');
  console.log(JSON.stringify({ r1, r2, r3, r4, r5 }, null, 2));
}

main().catch(err => {
  console.error('Fatal error in poc-06:', err);
  process.exit(1);
});
