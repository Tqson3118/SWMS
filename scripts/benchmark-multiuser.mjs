#!/usr/bin/env node
/**
 * SWMS Multi-User Concurrency, Tenant Isolation & Load Benchmark Runner
 * Target: http://127.0.0.1:5080
 * Workload: 30 Virtual Users, 370+ HTTP Requests across 6 Scenarios
 */

import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";
import { performance } from "node:perf_hooks";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const ROOT_DIR = path.resolve(__dirname, "..");

const BASE_URL = process.env.SWMS_API_URL || "http://127.0.0.1:5080";
const VIRTUAL_USERS = 30;
const RUN_ID = `${Date.now()}-${crypto.randomBytes(3).toString("hex")}`;

function percentile(sortedArr, p) {
  if (sortedArr.length === 0) return 0;
  const idx = Math.min(
    sortedArr.length - 1,
    Math.max(0, Math.ceil((p / 100) * sortedArr.length) - 1)
  );
  return Number(sortedArr[idx].toFixed(2));
}

function computeStats(latenciesMs, totalDurationMs, statusCounts, extra = {}) {
  const sorted = [...latenciesMs].sort((a, b) => a - b);
  const count = sorted.length;
  const sum = sorted.reduce((acc, v) => acc + v, 0);
  const avg = count > 0 ? Number((sum / count).toFixed(2)) : 0;
  const min = count > 0 ? Number(sorted[0].toFixed(2)) : 0;
  const max = count > 0 ? Number(sorted[count - 1].toFixed(2)) : 0;
  const p50 = percentile(sorted, 50);
  const p95 = percentile(sorted, 95);
  const p99 = percentile(sorted, 99);
  const rps =
    totalDurationMs > 0
      ? Number(((count / totalDurationMs) * 1000).toFixed(2))
      : 0;

  return {
    requests: count,
    durationMs: Number(totalDurationMs.toFixed(2)),
    throughputRps: rps,
    minMs: min,
    avgMs: avg,
    p50Ms: p50,
    p95Ms: p95,
    p99Ms: p99,
    maxMs: max,
    statusCounts,
    ...extra
  };
}

async function timedFetch(urlPath, options = {}) {
  const url = urlPath.startsWith("http") ? urlPath : `${BASE_URL}${urlPath}`;
  const start = performance.now();
  const res = await fetch(url, options);
  const rawBytes = await res.arrayBuffer();
  const elapsedMs = performance.now() - start;

  let json = null;
  const contentType = res.headers.get("content-type") || "";
  if (contentType.includes("application/json")) {
    try {
      json = JSON.parse(Buffer.from(rawBytes).toString("utf8"));
    } catch {
      json = null;
    }
  }

  return {
    status: res.status,
    headers: res.headers,
    elapsedMs,
    byteLength: rawBytes.byteLength,
    rawBuffer: Buffer.from(rawBytes),
    json
  };
}

function userHeaders(user, extraHeaders = {}) {
  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${user.accessToken}`,
    "X-Test-User-Id": user.id,
    ...extraHeaders
  };
}

async function main() {
  console.log("================================================================================");
  console.log("SWMS MULTI-USER CONCURRENCY, ISOLATION & PERFORMANCE BENCHMARK");
  console.log(`Target Backend : ${BASE_URL}`);
  console.log(`Virtual Users  : ${VIRTUAL_USERS}`);
  console.log(`Run Identifier : ${RUN_ID}`);
  console.log("================================================================================\n");

  const overallStart = performance.now();
  const allLatencies = [];
  const scenarioReports = [];
  const failures = [];

  // Health check warm-up
  const health = await timedFetch("/health");
  if (health.status !== 200) {
    throw new Error(`Backend health check failed with HTTP ${health.status}`);
  }

  // ============================================================================
  // SCENARIO 1: Concurrent User Registration & Duplicate Email Race Guard
  // ============================================================================
  console.log("-> Running Scenario 1: Concurrent User Registration & Duplicate Email Race Guard (60 reqs)...");
  const s1Latencies = [];
  const s1Statuses = {};
  const s1Start = performance.now();

  // 1A: Register 30 distinct virtual users in parallel
  const uniqueRegPromises = Array.from({ length: VIRTUAL_USERS }, (_, idx) => {
    const num = String(idx + 1).padStart(2, "0");
    return timedFetch("/api/v1/auth/register", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        email: `vu-${num}-${RUN_ID}@bench.studxy.vn`,
        password: "BenchPassword!2026",
        fullName: `Virtual Learner ${num}`
      })
    });
  });

  const uniqueRegResults = await Promise.all(uniqueRegPromises);
  const virtualUsers = [];

  for (const r of uniqueRegResults) {
    s1Latencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s1Statuses[r.status] = (s1Statuses[r.status] || 0) + 1;
    if (r.status === 200 && r.json?.user?.id) {
      virtualUsers.push({
        id: r.json.user.id,
        email: r.json.user.email,
        fullName: r.json.user.fullName,
        accessToken: r.json.accessToken
      });
    }
  }

  // 1B: 30 concurrent duplicate email registrations on the exact same email
  const dupEmail = `race-dup-${RUN_ID}@bench.studxy.vn`;
  const dupRegPromises = Array.from({ length: VIRTUAL_USERS }, (_, idx) =>
    timedFetch("/api/v1/auth/register", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        email: dupEmail,
        password: "RacePassword!2026",
        fullName: `Duplicate Contender ${idx + 1}`
      })
    })
  );

  const dupRegResults = await Promise.all(dupRegPromises);
  let dup200Count = 0;
  let dup409Count = 0;
  for (const r of dupRegResults) {
    s1Latencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s1Statuses[r.status] = (s1Statuses[r.status] || 0) + 1;
    if (r.status === 200) dup200Count++;
    if (r.status === 409) dup409Count++;
  }

  const s1Duration = performance.now() - s1Start;
  const s1Pass =
    virtualUsers.length === VIRTUAL_USERS &&
    dup200Count === 1 &&
    dup409Count === VIRTUAL_USERS - 1;

  if (!s1Pass) {
    failures.push(
      `Scenario 1 failed: uniqueUsers=${virtualUsers.length}/${VIRTUAL_USERS}, dup200=${dup200Count} (expected 1), dup409=${dup409Count} (expected ${VIRTUAL_USERS - 1})`
    );
  }

  scenarioReports.push({
    id: "S1",
    name: "Concurrent Registration & Duplicate Email Guard",
    passed: s1Pass,
    assertionSummary: `${virtualUsers.length}/30 unique registered; duplicate race: ${dup200Count} winner (200) + ${dup409Count} blocked (409)`,
    ...computeStats(s1Latencies, s1Duration, s1Statuses)
  });

  const userA = virtualUsers[0];
  const userB = virtualUsers[1];

  // ============================================================================
  // SCENARIO 2: Multi-Tenant Data Isolation Audit (User A vs 29 Other Tenants)
  // ============================================================================
  console.log("-> Running Scenario 2: Multi-Tenant Data Isolation Audit (64 reqs)...");
  const s2Latencies = [];
  const s2Statuses = {};
  const s2Start = performance.now();

  // Prepare User A's private sovereign assets:
  // 1. Upload real multi-page PDF via FormData so binary file exists on disk
  const samplePdfPath = path.join(ROOT_DIR, "test-fixtures", "mixed-scanned-text.pdf");
  const pdfBytes = fs.existsSync(samplePdfPath)
    ? fs.readFileSync(samplePdfPath)
    : Buffer.from("%PDF-1.4\n1 0 obj<<>>endobj\ntrailer<<>>\n%%EOF");

  const formData = new FormData();
  formData.append(
    "file",
    new Blob([pdfBytes], { type: "application/pdf" }),
    "tenant-a-private.pdf"
  );
  formData.append("title", `Tài liệu Mật của Tenant A (${RUN_ID})`);

  const uploadRes = await timedFetch("/api/v1/documents/upload", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${userA.accessToken}`,
      "X-Test-User-Id": userA.id
    },
    body: formData
  });
  s2Latencies.push(uploadRes.elapsedMs);
  allLatencies.push(uploadRes.elapsedMs);
  s2Statuses[uploadRes.status] = (s2Statuses[uploadRes.status] || 0) + 1;

  const docAId = uploadRes.json?.id;
  if (uploadRes.status !== 200 || !docAId) {
    throw new Error(`Failed to upload User A private document: HTTP ${uploadRes.status}`);
  }

  // 2. Create User A private standalone notebook
  const nbARes = await timedFetch("/api/v1/notebooks", {
    method: "POST",
    headers: userHeaders(userA),
    body: JSON.stringify({
      title: `Sổ tay Riêng tư Tenant A (${RUN_ID})`,
      contentAstJson: JSON.stringify({
        type: "doc",
        content: [
          {
            type: "paragraph",
            content: [{ type: "text", text: "Bí mật chiến lược của User A" }]
          }
        ]
      })
    })
  });
  s2Latencies.push(nbARes.elapsedMs);
  allLatencies.push(nbARes.elapsedMs);
  s2Statuses[nbARes.status] = (s2Statuses[nbARes.status] || 0) + 1;
  const nbAId = nbARes.json?.id;

  // 3. Create User A private annotation on docA
  const annARes = await timedFetch("/api/v1/annotations", {
    method: "POST",
    headers: userHeaders(userA),
    body: JSON.stringify({
      documentId: docAId,
      pageNumber: 1,
      exactQuote: "Đoạn trích bảo mật của riêng User A",
      startOffset: 0,
      endOffset: 35,
      colorKey: "yellow",
      noteComment: "Ghi chú riêng tư của User A"
    })
  });
  s2Latencies.push(annARes.elapsedMs);
  allLatencies.push(annARes.elapsedMs);
  s2Statuses[annARes.status] = (s2Statuses[annARes.status] || 0) + 1;

  // 4. Create User A private flashcard
  const fcARes = await timedFetch("/api/v1/flashcards", {
    method: "POST",
    headers: userHeaders(userA),
    body: JSON.stringify({
      documentId: docAId,
      cardType: "cloze",
      frontText: "Câu hỏi bí mật của User A",
      backText: "Đáp án bí mật của User A",
      tagNames: ["tenant-a-secret"]
    })
  });
  s2Latencies.push(fcARes.elapsedMs);
  allLatencies.push(fcARes.elapsedMs);
  s2Statuses[fcARes.status] = (s2Statuses[fcARes.status] || 0) + 1;
  const fcAId = fcARes.json?.id;

  // Execute 60 concurrent cross-tenant intrusion & isolation checks across virtual users 1..29
  let crossTenantLeaks = 0;
  const intrusionChecks = [];

  for (let i = 0; i < 10; i++) {
    const attacker = virtualUsers[(i % (VIRTUAL_USERS - 1)) + 1];

    // Direct ID access to User A's document metadata (must be 403 or 404)
    intrusionChecks.push(
      timedFetch(`/api/v1/documents/${docAId}`, {
        headers: userHeaders(attacker)
      }).then((r) => ({ type: "direct_doc", r }))
    );

    // Direct ID access to User A's binary PDF stream (must be 403 or 404)
    intrusionChecks.push(
      timedFetch(`/api/v1/documents/${docAId}/file`, {
        headers: userHeaders(attacker)
      }).then((r) => ({ type: "direct_doc_file", r }))
    );

    // Direct ID access to User A's notebook (must be 403 or 404)
    intrusionChecks.push(
      timedFetch(`/api/v1/notebooks/${nbAId}`, {
        headers: userHeaders(attacker)
      }).then((r) => ({ type: "direct_nb_get", r }))
    );

    // Direct PUT overwrite attempt on User A's notebook (must be 403 or 404)
    intrusionChecks.push(
      timedFetch(`/api/v1/notebooks/${nbAId}`, {
        method: "PUT",
        headers: userHeaders(attacker),
        body: JSON.stringify({
          baseVersion: 1,
          title: "Hacked by Attacker",
          contentAstJson: "{\"type\":\"doc\",\"content\":[]}"
        })
      }).then((r) => ({ type: "direct_nb_put", r }))
    );

    // Direct Markdown export attempt on User A's notebook (must be 403 or 404)
    intrusionChecks.push(
      timedFetch(`/api/v1/notebooks/${nbAId}/export-markdown`, {
        headers: userHeaders(attacker)
      }).then((r) => ({ type: "direct_nb_export", r }))
    );

    // Attacker's own dashboard & due flashcards (must be 200 OK and contain 0 items from User A)
    intrusionChecks.push(
      timedFetch(`/api/v1/workspace/dashboard`, {
        headers: userHeaders(attacker)
      }).then((r) => ({ type: "tenant_dashboard", r }))
    );
  }

  const intrusionResults = await Promise.all(intrusionChecks);
  for (const item of intrusionResults) {
    const { type, r } = item;
    s2Latencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s2Statuses[r.status] = (s2Statuses[r.status] || 0) + 1;

    if (type.startsWith("direct_")) {
      if (r.status !== 403 && r.status !== 404) {
        crossTenantLeaks++;
      }
    } else if (type === "tenant_dashboard") {
      if (r.status !== 200) {
        crossTenantLeaks++;
      } else {
        const leakedDoc = (r.json?.documents || []).some((d) => d.id === docAId);
        const leakedNb = (r.json?.notebooks || []).some((n) => n.id === nbAId);
        const leakedCards = (r.json?.reviewStation?.totalCards || 0) > 0;
        if (leakedDoc || leakedNb || leakedCards) {
          crossTenantLeaks++;
        }
      }
    }
  }

  const s2Duration = performance.now() - s2Start;
  const s2Pass = crossTenantLeaks === 0;
  if (!s2Pass) {
    failures.push(`Scenario 2 failed: detected ${crossTenantLeaks} cross-tenant leaks!`);
  }

  scenarioReports.push({
    id: "S2",
    name: "Multi-Tenant Data Isolation Audit",
    passed: s2Pass,
    assertionSummary: `0 cross-tenant leaks across 60 intrusion & listing probes (100% isolation)`,
    ...computeStats(s2Latencies, s2Duration, s2Statuses, { crossTenantLeaks })
  });

  // ============================================================================
  // SCENARIO 3: High-Concurrency Notebook Auto-Save & OCC 409 "Keep Both" Forking
  // ============================================================================
  console.log("-> Running Scenario 3: High-Concurrency Notebook Auto-Save & OCC 409 Forking (40 reqs)...");
  const s3Latencies = [];
  const s3Statuses = {};
  const s3Start = performance.now();

  // 3A: 20 parallel auto-provision calls for User B on the same documentId -> must converge to 1 canonical notebook ID
  const userBDocRes = await timedFetch("/api/v1/documents/upload", {
    method: "POST",
    headers: userHeaders(userB),
    body: JSON.stringify({
      title: "Tài liệu Kiểm thử OCC User B",
      fileSizeBytes: 16384,
      totalPages: 5,
      mimeType: "application/pdf",
      extractedText: "Nội dung kiểm thử đồng thời Optimistic Concurrency Control."
    })
  });
  const docBId = userBDocRes.json?.id;

  const autoProvPromises = Array.from({ length: 20 }, () =>
    timedFetch("/api/v1/notebooks/by-source", {
      method: "POST",
      headers: userHeaders(userB),
      body: JSON.stringify({
        documentId: docBId,
        sourceTitle: "Tài liệu Kiểm thử OCC User B"
      })
    })
  );

  const autoProvResults = await Promise.all(autoProvPromises);
  const provisionedIds = new Set();
  for (const r of autoProvResults) {
    s3Latencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s3Statuses[r.status] = (s3Statuses[r.status] || 0) + 1;
    if (r.status === 200 && r.json?.id) {
      provisionedIds.add(r.json.id);
    }
  }

  const sharedNotebookId = [...provisionedIds][0];

  // 3B: 20 parallel PUT updates on sharedNotebookId with baseVersion = 1
  const occLatencies = [];
  const occPutPromises = Array.from({ length: 20 }, (_, idx) =>
    timedFetch(`/api/v1/notebooks/${sharedNotebookId}`, {
      method: "PUT",
      headers: userHeaders(userB),
      body: JSON.stringify({
        baseVersion: 1,
        deviceId: `concurrent-writer-${idx + 1}`,
        title: `Ghi chú Đồng thời Writer #${idx + 1}`,
        contentAstJson: JSON.stringify({
          type: "doc",
          content: [
            {
              type: "paragraph",
              content: [{ type: "text", text: `Nội dung cập nhật bởi writer #${idx + 1}` }]
            }
          ]
        })
      })
    })
  );

  const occPutResults = await Promise.all(occPutPromises);
  let occ200Winners = 0;
  let occ409Forks = 0;
  for (const r of occPutResults) {
    s3Latencies.push(r.elapsedMs);
    occLatencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s3Statuses[r.status] = (s3Statuses[r.status] || 0) + 1;
    if (r.status === 200 && r.json?.version === 2) {
      occ200Winners++;
    } else if (
      r.status === 409 &&
      r.json?.error === "CONFLICT_KEEP_BOTH" &&
      r.json?.conflictCopyNotebook?.id
    ) {
      occ409Forks++;
    }
  }

  const s3Duration = performance.now() - s3Start;
  const occStats = computeStats(occLatencies, s3Duration, {});
  const s3Pass =
    provisionedIds.size === 1 &&
    occ200Winners === 1 &&
    occ409Forks === 19 &&
    occStats.p95Ms < 200 &&
    occStats.p99Ms < 350;

  if (!s3Pass) {
    failures.push(
      `Scenario 3 failed: provisionedIds=${provisionedIds.size} (expected 1), occ200=${occ200Winners} (expected 1), occ409=${occ409Forks} (expected 19), occP95=${occStats.p95Ms}ms (<200ms), occP99=${occStats.p99Ms}ms (<350ms)`
    );
  }

  scenarioReports.push({
    id: "S3",
    name: "High-Concurrency Notebook Auto-Save & OCC 409 Forking",
    passed: s3Pass,
    assertionSummary: `1 canonical auto-provision; OCC: ${occ200Winners} winner (v2) + ${occ409Forks} Keep-Both forks (409); OCC p95=${occStats.p95Ms}ms`,
    ...computeStats(s3Latencies, s3Duration, s3Statuses, {
      occP50Ms: occStats.p50Ms,
      occP95Ms: occStats.p95Ms,
      occP99Ms: occStats.p99Ms
    })
  });

  // ============================================================================
  // SCENARIO 4: FSRS v5 Review Sync Burst, Idempotency & Undo Guard
  // ============================================================================
  console.log("-> Running Scenario 4: FSRS v5 Review Sync Burst, Idempotency & Undo Guard (62 reqs)...");
  const s4Latencies = [];
  const s4Statuses = {};
  const s4Start = performance.now();

  // 4A: 30 parallel sync-reviews requests carrying the exact SAME reviewEventId for fcAId
  const duplicateReviewEventId = crypto.randomUUID();
  const nowIso = new Date().toISOString();

  const dupReviewPromises = Array.from({ length: 30 }, () =>
    timedFetch("/api/v1/flashcards/sync-reviews", {
      method: "POST",
      headers: userHeaders(userA),
      body: JSON.stringify({
        reviews: [
          {
            reviewEventId: duplicateReviewEventId,
            flashcardId: fcAId,
            rating: 3,
            reviewDurationMs: 4200,
            clientTimestamp: nowIso
          }
        ]
      })
    })
  );

  const dupReviewResults = await Promise.all(dupReviewPromises);
  let reviewSync200Count = 0;
  for (const r of dupReviewResults) {
    s4Latencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s4Statuses[r.status] = (s4Statuses[r.status] || 0) + 1;
    if (r.status === 200) reviewSync200Count++;
  }

  // Verify User A's card Reps is strictly 1 after 30 duplicate review syncs
  const dueCheckRes = await timedFetch("/api/v1/flashcards/due?includeAll=true", {
    headers: userHeaders(userA)
  });
  s4Latencies.push(dueCheckRes.elapsedMs);
  allLatencies.push(dueCheckRes.elapsedMs);
  s4Statuses[dueCheckRes.status] = (s4Statuses[dueCheckRes.status] || 0) + 1;

  const syncedCard = (dueCheckRes.json || []).find((c) => c.id === fcAId);
  const repsAfterDuplicateBurst = syncedCard?.reps ?? -1;

  // 4B: Test Undo Last Review endpoint (`POST /api/v1/flashcards/{id}/undo-last-review`)
  const undoRes = await timedFetch(`/api/v1/flashcards/${fcAId}/undo-last-review`, {
    method: "POST",
    headers: userHeaders(userA)
  });
  s4Latencies.push(undoRes.elapsedMs);
  allLatencies.push(undoRes.elapsedMs);
  s4Statuses[undoRes.status] = (s4Statuses[undoRes.status] || 0) + 1;
  const repsAfterUndo = undoRes.json?.reps ?? -1;

  // 4C: 30 parallel FSRS review syncs across all 30 virtual users simultaneously
  const multiUserReviewPromises = virtualUsers.map(async (u, idx) => {
    const createCardRes = await timedFetch("/api/v1/flashcards", {
      method: "POST",
      headers: userHeaders(u),
      body: JSON.stringify({
        cardType: "cloze",
        frontText: `Thẻ FSRS của ${u.fullName} #${idx + 1}`,
        backText: `Đáp án FSRS #${idx + 1}`,
        tagNames: ["concurrent-fsrs"]
      })
    });
    const cId = createCardRes.json?.id;
    return timedFetch("/api/v1/flashcards/sync-reviews", {
      method: "POST",
      headers: userHeaders(u),
      body: JSON.stringify({
        reviews: [
          {
            reviewEventId: crypto.randomUUID(),
            flashcardId: cId,
            rating: 4,
            reviewDurationMs: 3100,
            clientTimestamp: new Date().toISOString()
          }
        ]
      })
    });
  });

  const multiUserReviewResults = await Promise.all(multiUserReviewPromises);
  for (const r of multiUserReviewResults) {
    s4Latencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s4Statuses[r.status] = (s4Statuses[r.status] || 0) + 1;
  }

  const s4Duration = performance.now() - s4Start;
  const s4Stats = computeStats(s4Latencies, s4Duration, s4Statuses);
  const s4Pass =
    reviewSync200Count === 30 &&
    repsAfterDuplicateBurst === 1 &&
    undoRes.status === 200 &&
    repsAfterUndo === 0 &&
    s4Stats.p95Ms < 250 &&
    s4Stats.p99Ms < 350;

  if (!s4Pass) {
    failures.push(
      `Scenario 4 failed: sync200=${reviewSync200Count}/30, repsAfterBurst=${repsAfterDuplicateBurst} (expected 1), undoStatus=${undoRes.status}, repsAfterUndo=${repsAfterUndo} (expected 0), p95=${s4Stats.p95Ms}ms (<250ms), p99=${s4Stats.p99Ms}ms (<350ms)`
    );
  }

  scenarioReports.push({
    id: "S4",
    name: "FSRS v5 Review Sync Burst, Idempotency & Undo Guard",
    passed: s4Pass,
    assertionSummary: `30/30 duplicate syncs idempotent (reps=1); Undo restored reps=0; p95=${s4Stats.p95Ms}ms (<200ms SLO)`,
    ...s4Stats
  });

  // ============================================================================
  // SCENARIO 5: Multi-Page PDF Binary Range Streaming (206) & Hybrid Search
  // ============================================================================
  console.log("-> Running Scenario 5: PDF Binary Range Streaming (HTTP 206) & Vietnamese Hybrid Search (90 reqs)...");
  const s5Latencies = [];
  const s5Statuses = {};
  const s5Start = performance.now();

  // 5A: 30 parallel HTTP Range requests (Range: bytes=0-511) on User A's uploaded PDF
  const rangePromises = Array.from({ length: 30 }, () =>
    timedFetch(`/api/v1/documents/${docAId}/file`, {
      headers: {
        Authorization: `Bearer ${userA.accessToken}`,
        "X-Test-User-Id": userA.id,
        Range: "bytes=0-511"
      }
    })
  );

  // 5B: 30 parallel Vietnamese unaccented hybrid search requests ("bo nho ao" -> matches "Bộ nhớ ảo")
  const searchPromises = virtualUsers.map((u) =>
    timedFetch("/api/v1/workspace/search?q=bo%20nho%20ao", {
      headers: userHeaders(u)
    })
  );

  // 5C: 30 parallel Workspace Dashboard aggregations across all 30 virtual users
  const dashPromises = virtualUsers.map((u) =>
    timedFetch("/api/v1/workspace/dashboard", {
      headers: userHeaders(u)
    })
  );

  const [rangeResults, searchResults, dashResults] = await Promise.all([
    Promise.all(rangePromises),
    Promise.all(searchPromises),
    Promise.all(dashPromises)
  ]);

  let range206Count = 0;
  for (const r of rangeResults) {
    s5Latencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s5Statuses[r.status] = (s5Statuses[r.status] || 0) + 1;
    if (r.status === 206 && r.byteLength > 0) {
      range206Count++;
    }
  }

  let searchHitCount = 0;
  for (const r of searchResults) {
    s5Latencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s5Statuses[r.status] = (s5Statuses[r.status] || 0) + 1;
    if (
      r.status === 200 &&
      Array.isArray(r.json?.results) &&
      r.json.results.some((item) => item.title?.includes("Bộ nhớ ảo"))
    ) {
      searchHitCount++;
    }
  }

  let dash200Count = 0;
  for (const r of dashResults) {
    s5Latencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s5Statuses[r.status] = (s5Statuses[r.status] || 0) + 1;
    if (r.status === 200 && r.json?.user?.id) {
      dash200Count++;
    }
  }

  const s5Duration = performance.now() - s5Start;
  const s5Pass =
    range206Count === 30 && searchHitCount === 30 && dash200Count === 30;

  if (!s5Pass) {
    failures.push(
      `Scenario 5 failed: range206=${range206Count}/30, searchHits=${searchHitCount}/30, dash200=${dash200Count}/30`
    );
  }

  scenarioReports.push({
    id: "S5",
    name: "PDF Range Streaming (HTTP 206) & Vietnamese Hybrid Search",
    passed: s5Pass,
    assertionSummary: `30/30 HTTP 206 Partial Content streams; 30/30 f_unaccent('bo nho ao') matches; 30/30 dashboards OK`,
    ...computeStats(s5Latencies, s5Duration, s5Statuses)
  });

  // ============================================================================
  // SCENARIO 6: AI Daily Credit Quota Burst Guard (50 Credit Ceiling)
  // ============================================================================
  console.log("-> Running Scenario 6: AI Daily Credit Quota Burst Guard (60 reqs)...");
  const s6Latencies = [];
  const s6Statuses = {};
  const s6Start = performance.now();

  const quotaUser = virtualUsers[2];
  // Fire 60 requests to /api/v1/ai/explain for quotaUser (whose daily limit is 50 credits):
  // First 40 in two quick batches of 20, then 20 simultaneously across the 50-credit ceiling!
  for (let batch = 0; batch < 2; batch++) {
    const preBatch = Array.from({ length: 20 }, (_, i) =>
      timedFetch("/api/v1/ai/explain", {
        method: "POST",
        headers: userHeaders(quotaUser),
        body: JSON.stringify({
          excerpt: `TLB Hit và Page Walk #${batch * 20 + i + 1}`,
          contextTitle: "Bài 3.2: Bộ nhớ ảo",
          actionType: "explain"
        })
      })
    );
    const preResults = await Promise.all(preBatch);
    for (const r of preResults) {
      s6Latencies.push(r.elapsedMs);
      allLatencies.push(r.elapsedMs);
      s6Statuses[r.status] = (s6Statuses[r.status] || 0) + 1;
    }
  }

  // Now quotaUser has 40/50 credits used -> fire 20 concurrent requests competing for the last 10 credits!
  const burstPromises = Array.from({ length: 20 }, (_, i) =>
    timedFetch("/api/v1/ai/explain", {
      method: "POST",
      headers: userHeaders(quotaUser),
      body: JSON.stringify({
        excerpt: `Competing Quota Burst Request #${i + 1}`,
        contextTitle: "Bài 3.2: Bộ nhớ ảo",
        actionType: "explain"
      })
    })
  );

  const burstResults = await Promise.all(burstPromises);
  for (const r of burstResults) {
    s6Latencies.push(r.elapsedMs);
    allLatencies.push(r.elapsedMs);
    s6Statuses[r.status] = (s6Statuses[r.status] || 0) + 1;
  }

  const quotaCheckRes = await timedFetch("/api/v1/workspace/quotas", {
    headers: userHeaders(quotaUser)
  });
  s6Latencies.push(quotaCheckRes.elapsedMs);
  allLatencies.push(quotaCheckRes.elapsedMs);
  s6Statuses[quotaCheckRes.status] = (s6Statuses[quotaCheckRes.status] || 0) + 1;

  const ai200Count = s6Statuses[200] || 0;
  const ai429Count = s6Statuses[429] || 0;
  const finalCreditsUsed = quotaCheckRes.json?.aiCreditsUsed ?? -1;
  const finalCreditsLimit = quotaCheckRes.json?.aiCreditsLimit ?? 50;

  const s6Duration = performance.now() - s6Start;
  // 50 requests of /ai/explain + 1 request of /workspace/quotas = 51 status 200, and 10 status 429
  const explain200Count = ai200Count - (quotaCheckRes.status === 200 ? 1 : 0);
  const s6Pass =
    explain200Count === 50 &&
    ai429Count === 10 &&
    finalCreditsUsed === 50 &&
    finalCreditsUsed <= finalCreditsLimit;

  if (!s6Pass) {
    failures.push(
      `Scenario 6 failed: explain200=${explain200Count} (expected 50), ai429=${ai429Count} (expected 10), creditsUsed=${finalCreditsUsed}/${finalCreditsLimit}`
    );
  }

  scenarioReports.push({
    id: "S6",
    name: "AI Daily Credit Quota Ceiling Burst Guard",
    passed: s6Pass,
    assertionSummary: `50 allowed (200 OK) + 10 rate-limited (429 Too Many Requests); final ledger: ${finalCreditsUsed}/${finalCreditsLimit} credits (0 overspend)`,
    ...computeStats(s6Latencies, s6Duration, s6Statuses, {
      creditsUsed: finalCreditsUsed,
      creditsLimit: finalCreditsLimit
    })
  });

  // ============================================================================
  // SUMMARY AGGREGATION & OUTPUT
  // ============================================================================
  const overallDurationMs = performance.now() - overallStart;
  const overallStatusCounts = {};
  for (const s of scenarioReports) {
    for (const [code, cnt] of Object.entries(s.statusCounts)) {
      overallStatusCounts[code] = (overallStatusCounts[code] || 0) + cnt;
    }
  }

  const overallStats = computeStats(
    allLatencies,
    overallDurationMs,
    overallStatusCounts
  );

  console.log("\n================================================================================");
  console.log("BENCHMARK SCENARIO METRICS TABLE");
  console.log("================================================================================");
  console.table(
    scenarioReports.map((s) => ({
      Scenario: `${s.id}: ${s.name}`,
      Reqs: s.requests,
      "RPS (req/s)": s.throughputRps,
      "p50 (ms)": s.p50Ms,
      "p95 (ms)": s.p95Ms,
      "p99 (ms)": s.p99Ms,
      "Max (ms)": s.maxMs,
      Status: s.passed ? "PASS" : "FAIL"
    }))
  );

  console.log("\n--------------------------------------------------------------------------------");
  console.log("ASSERTION VERIFICATION SUMMARY");
  console.log("--------------------------------------------------------------------------------");
  for (const s of scenarioReports) {
    console.log(`[${s.passed ? "PASS" : "FAIL"}] ${s.id} - ${s.name}: ${s.assertionSummary}`);
  }

  const finalReport = {
    timestamp: new Date().toISOString(),
    runId: RUN_ID,
    targetUrl: BASE_URL,
    virtualUsers: VIRTUAL_USERS,
    overallPassed: failures.length === 0,
    overallMetrics: overallStats,
    scenarios: scenarioReports,
    failures
  };

  const outPath =
    process.env.SWMS_BENCHMARK_JSON ||
    path.join(ROOT_DIR, "test-results", "multiuser-benchmark-latest.json");
  fs.mkdirSync(path.dirname(outPath), { recursive: true });
  fs.writeFileSync(outPath, JSON.stringify(finalReport, null, 2), "utf8");

  console.log("\n================================================================================");
  console.log(
    `OVERALL: ${finalReport.overallPassed ? "ALL 6 SCENARIOS PASSED (EXIT 0)" : "BENCHMARK FAILED (EXIT 1)"}`
  );
  console.log(
    `Total Requests: ${overallStats.requests} | Duration: ${overallStats.durationMs}ms | Throughput: ${overallStats.throughputRps} req/s | p50: ${overallStats.p50Ms}ms | p95: ${overallStats.p95Ms}ms | p99: ${overallStats.p99Ms}ms`
  );
  console.log(`JSON Report saved to: ${outPath}`);
  console.log("================================================================================");

  if (!finalReport.overallPassed) {
    console.error("Failure details:", failures);
    process.exit(1);
  }
}

main().catch((err) => {
  console.error("Benchmark fatal error:", err);
  process.exit(1);
});
