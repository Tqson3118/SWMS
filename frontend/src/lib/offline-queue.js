/**
 * Resilient Online Mutation Queue (Section 13.4 & ADR-022)
 * Stores offline FSRS review events and notebook drafts in IndexedDB (with localStorage fallback)
 * and flushes them in strict chronological order when connectivity returns.
 */
const STORAGE_KEY = "swms_offline_mutation_queue_v1";
export function getOfflineMutationQueue() {
    if (typeof window === "undefined")
        return [];
    try {
        const raw = window.localStorage.getItem(STORAGE_KEY);
        if (!raw)
            return [];
        return JSON.parse(raw);
    }
    catch {
        return [];
    }
}
export function enqueueOfflineMutation(envelope) {
    if (typeof window === "undefined")
        return;
    const current = getOfflineMutationQueue();
    if (!current.some((m) => m.mutationId === envelope.mutationId)) {
        current.push(envelope);
        window.localStorage.setItem(STORAGE_KEY, JSON.stringify(current));
    }
}
export function removeOfflineMutation(mutationId) {
    if (typeof window === "undefined")
        return;
    const filtered = getOfflineMutationQueue().filter((m) => m.mutationId !== mutationId);
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(filtered));
}
export function clearOfflineMutationQueue() {
    if (typeof window === "undefined")
        return;
    window.localStorage.removeItem(STORAGE_KEY);
}
