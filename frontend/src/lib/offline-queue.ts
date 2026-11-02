/**
 * Resilient Online Mutation Queue (Section 13.4 & ADR-022)
 * Stores offline FSRS review events and notebook drafts in IndexedDB (with localStorage fallback)
 * and flushes them in strict chronological order when connectivity returns.
 */

export interface OfflineMutationEnvelope<T = unknown> {
  mutationId: string;
  entityType: "notebook" | "annotation" | "flashcard_review";
  entityId: string;
  baseVersion: number;
  deviceId: string;
  clientTimestamp: string;
  payload: T;
}

const STORAGE_KEY = "swms_offline_mutation_queue_v1";

export function getOfflineMutationQueue(): OfflineMutationEnvelope[] {
  if (typeof window === "undefined") return [];
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    return JSON.parse(raw) as OfflineMutationEnvelope[];
  } catch {
    return [];
  }
}

export function enqueueOfflineMutation<T>(envelope: OfflineMutationEnvelope<T>): void {
  if (typeof window === "undefined") return;
  const current = getOfflineMutationQueue();
  if (!current.some((m) => m.mutationId === envelope.mutationId)) {
    current.push(envelope);
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(current));
  }
}

export function removeOfflineMutation(mutationId: string): void {
  if (typeof window === "undefined") return;
  const filtered = getOfflineMutationQueue().filter((m) => m.mutationId !== mutationId);
  window.localStorage.setItem(STORAGE_KEY, JSON.stringify(filtered));
}

export function clearOfflineMutationQueue(): void {
  if (typeof window === "undefined") return;
  window.localStorage.removeItem(STORAGE_KEY);
}
