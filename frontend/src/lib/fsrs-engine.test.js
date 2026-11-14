import { evaluateFsrsV5 } from "./fsrs-engine.ts";
function assertClose(actual, expected, decimals = 4, label = "") {
    const diff = Math.abs(actual - expected);
    const tol = Math.pow(10, -decimals);
    if (diff > tol) {
        throw new Error(`Parity assertion failed (${label}): expected ${expected}, got ${actual}`);
    }
}
const nowIso = "2026-10-31T05:00:00.000Z";
const newCard = {
    stability: 0,
    difficulty: 0,
    reps: 0,
    lapses: 0,
    state: 0,
    lastReviewAt: null,
    dueAt: nowIso
};
// Golden Vector 1: Exact 4-decimal parity with C# FsrsV5Engine.cs
const r1 = evaluateFsrsV5(newCard, 1, nowIso);
const r2 = evaluateFsrsV5(newCard, 2, nowIso);
const r3 = evaluateFsrsV5(newCard, 3, nowIso);
const r4 = evaluateFsrsV5(newCard, 4, nowIso);
assertClose(r1.stability, 0.4072, 4, "Initial Stability G=1");
assertClose(r2.stability, 1.1829, 4, "Initial Stability G=2");
assertClose(r3.stability, 3.1262, 4, "Initial Stability G=3");
assertClose(r4.stability, 15.4722, 4, "Initial Stability G=4");
assertClose(r1.difficulty, 7.2102, 4, "Initial Difficulty G=1");
assertClose(r3.difficulty, 5.3146, 4, "Initial Difficulty G=3");
// Golden Vector 2: Latency SLO check (1,000 sequential evaluations, p99 < 5ms)
const latencies = [];
let state = newCard;
for (let i = 0; i < 1000; i++) {
    const rating = ((i % 4) + 1);
    const res = evaluateFsrsV5(state, rating, new Date(Date.now() + i * 86400000).toISOString());
    latencies.push(res.evaluationMs);
    state = {
        stability: res.stability,
        difficulty: res.difficulty,
        reps: res.reps,
        lapses: res.lapses,
        state: res.state,
        lastReviewAt: res.reviewedAt,
        dueAt: res.nextDueAt
    };
}
latencies.sort((a, b) => a - b);
const p99 = latencies[Math.floor(latencies.length * 0.99)];
if (p99 >= 5.0) {
    throw new Error(`FSRS v5 local evaluation p99 latency ${p99}ms exceeded 5ms SLO`);
}
console.log(`✅ FSRS v5 TypeScript <-> C# Golden Vectors Passed! (1,000 cards p99 = ${p99}ms < 5ms SLO)`);
