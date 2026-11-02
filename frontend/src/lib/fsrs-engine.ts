/**
 * Canonical Client-Side FSRS v5 Engine (19 parameters)
 * Synchronized to 4 decimal places with Backend C# FsrsV5Engine.cs (Decision J & ADR-018)
 * Target Local Evaluation Latency SLO: p99 < 5ms (Section 13.5)
 */

export interface FsrsCardState {
  stability: number;
  difficulty: number;
  reps: number;
  lapses: number;
  state: number; // 0=New, 1=Learning, 2=Review, 3=Relearning
  lastReviewAt: string | null;
  dueAt: string;
}

export interface FsrsScheduleOutput {
  stability: number;
  difficulty: number;
  retrievability: number;
  scheduledDays: number;
  elapsedDays: number;
  reps: number;
  lapses: number;
  state: number;
  reviewedAt: string;
  nextDueAt: string;
  evaluationMs: number;
}

export const DEFAULT_FSRS_V5_WEIGHTS: readonly number[] = [
  0.4072, 1.1829, 3.1262, 15.4722, 7.2102,
  0.5316, 1.0651, 0.0234, 1.616, 0.1544,
  1.0824, 1.9813, 0.0953, 0.2975, 2.2042,
  0.2407, 2.9466, 0.5034, 0.6567
];

const DECAY = -0.5;
const FACTOR = Math.pow(0.9, 1.0 / DECAY) - 1.0; // 19 / 81

function round4(val: number): number {
  const sign = val < 0 ? -1 : 1;
  return (sign * Math.round(Math.abs(val) * 10000)) / 10000;
}

function clamp(val: number, min: number, max: number): number {
  return Math.min(Math.max(val, min), max);
}

export function initialStability(w: readonly number[], rating: number): number {
  return w[rating - 1];
}

export function initialDifficulty(w: readonly number[], rating: number): number {
  const d0 = w[4] - Math.exp(w[5] * (rating - 1.0)) + 1.0;
  return clamp(d0, 1.0, 10.0);
}

export function nextDifficulty(w: readonly number[], d: number, rating: number): number {
  const deltaD = -w[6] * (rating - 3.0);
  const dPrime = d + deltaD * ((10.0 - d) / 9.0);
  const d0Easy = initialDifficulty(w, 4);
  const meanReverted = w[7] * d0Easy + (1.0 - w[7]) * dPrime;
  return clamp(meanReverted, 1.0, 10.0);
}

export function forgettingCurve(elapsedDays: number, stability: number): number {
  if (stability <= 0.0) return 0.0;
  return Math.pow(1.0 + FACTOR * (elapsedDays / stability), DECAY);
}

export function shortTermStability(w: readonly number[], s: number, rating: number): number {
  return s * Math.exp(w[17] * (rating - 3.0 + w[18]));
}

export function nextRecallStability(
  w: readonly number[],
  d: number,
  s: number,
  r: number,
  rating: number
): number {
  const hardPenalty = rating === 2 ? w[15] : 1.0;
  const easyBonus = rating === 4 ? w[16] : 1.0;
  const inc =
    Math.exp(w[8]) *
    (11.0 - d) *
    Math.pow(s, -w[9]) *
    (Math.exp(w[10] * (1.0 - r)) - 1.0) *
    hardPenalty *
    easyBonus;
  return s * (1.0 + inc);
}

export function nextForgetStability(
  w: readonly number[],
  d: number,
  s: number,
  r: number
): number {
  const sf =
    w[11] *
    Math.pow(d, -w[12]) *
    (Math.pow(s + 1.0, w[13]) - 1.0) *
    Math.exp(w[14] * (1.0 - r));
  return Math.min(s, sf);
}

export function nextIntervalDays(
  s: number,
  desiredRetention = 0.9,
  maxInterval = 36500
): number {
  const raw = (s / FACTOR) * (Math.pow(desiredRetention, 1.0 / DECAY) - 1.0);
  return clamp(raw, 1.0, maxInterval);
}

export function evaluateFsrsV5(
  card: FsrsCardState,
  rating: 1 | 2 | 3 | 4,
  reviewTimeIso: string = new Date().toISOString(),
  customWeights?: readonly number[],
  desiredRetention = 0.9,
  maxIntervalDays = 36500
): FsrsScheduleOutput {
  const tStart = typeof performance !== "undefined" ? performance.now() : 0;
  const w = customWeights && customWeights.length === 19 ? customWeights : DEFAULT_FSRS_V5_WEIGHTS;
  const reviewMs = new Date(reviewTimeIso).getTime();

  let elapsedDays = 0.0;
  if (card.lastReviewAt && card.state !== 0) {
    const lastMs = new Date(card.lastReviewAt).getTime();
    elapsedDays = Math.max(0.0, (reviewMs - lastMs) / 86400000.0);
  }

  let newS: number;
  let newD: number;
  let retrievability: number;
  let newState: number;
  let newLapses = card.lapses;
  const newReps = card.reps + 1;

  if (card.state === 0 || card.stability <= 0.0) {
    newS = initialStability(w, rating);
    newD = initialDifficulty(w, rating);
    retrievability = 1.0;
    newState = rating === 1 ? 1 : 2;
  } else {
    retrievability = forgettingCurve(elapsedDays, card.stability);
    newD = nextDifficulty(w, card.difficulty, rating);

    if (elapsedDays < 1.0 && (card.state === 1 || card.state === 3)) {
      newS = shortTermStability(w, card.stability, rating);
    } else if (rating === 1) {
      newS = nextForgetStability(w, newD, card.stability, retrievability);
    } else {
      newS = nextRecallStability(w, newD, card.stability, retrievability, rating);
    }

    if (rating === 1) {
      newLapses++;
      newState = card.state === 2 ? 3 : 1;
    } else {
      newState = 2;
    }
  }

  newS = round4(clamp(newS, 0.01, maxIntervalDays));
  newD = round4(clamp(newD, 1.0, 10.0));
  retrievability = round4(retrievability);

  let scheduledDays: number;
  if (rating === 1) {
    scheduledDays = round4(Math.max(0.0069, nextIntervalDays(newS, desiredRetention, maxIntervalDays) * 0.1));
  } else {
    scheduledDays = round4(nextIntervalDays(newS, desiredRetention, maxIntervalDays));
  }

  const nextDueMs =
    rating === 1
      ? reviewMs + 10 * 60 * 1000
      : reviewMs + Math.max(1, Math.round(scheduledDays)) * 86400000;

  const tEnd = typeof performance !== "undefined" ? performance.now() : 0;

  return {
    stability: newS,
    difficulty: newD,
    retrievability,
    scheduledDays,
    elapsedDays: round4(elapsedDays),
    reps: newReps,
    lapses: newLapses,
    state: newState,
    reviewedAt: new Date(reviewMs).toISOString(),
    nextDueAt: new Date(nextDueMs).toISOString(),
    evaluationMs: round4(tEnd - tStart)
  };
}
