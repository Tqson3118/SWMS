import { useRef, useCallback } from "react";

/**
 * Scroll-Sync Mutex with 50ms Cooldown, Trailing-Edge Dispatch (pendingAnchorId),
 * and Proportional Interpolation Fallback (Decision G, ADR-007 & ADR-015).
 */
export function useSynchronizedScroll(syncEnabled: boolean) {
  const isSyncingRef = useRef<boolean>(false);
  const syncTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const pendingAnchorIdRef = useRef<string | null>(null);

  const syncScrollContainerProportional = useCallback(
    (sourceEl: HTMLElement | null, targetEl: HTMLElement | null) => {
      if (!syncEnabled || !sourceEl || !targetEl) return;
      if (isSyncingRef.current) return;

      isSyncingRef.current = true;
      requestAnimationFrame(() => {
        const maxSourceScroll = Math.max(1, sourceEl.scrollHeight - sourceEl.clientHeight);
        const maxTargetScroll = Math.max(0, targetEl.scrollHeight - targetEl.clientHeight);
        const ratio = Math.min(1, Math.max(0, sourceEl.scrollTop / maxSourceScroll));
        targetEl.scrollTop = ratio * maxTargetScroll;

        if (syncTimeoutRef.current) clearTimeout(syncTimeoutRef.current);
        syncTimeoutRef.current = setTimeout(() => {
          isSyncingRef.current = false;
        }, 50);
      });
    },
    [syncEnabled]
  );

  const handleAnchorScroll = useCallback(
    (targetAnchorElementId: string) => {
      if (!syncEnabled || typeof document === "undefined") return;
      pendingAnchorIdRef.current = targetAnchorElementId;
      if (isSyncingRef.current) return;

      isSyncingRef.current = true;
      requestAnimationFrame(() => {
        const anchorToSync = pendingAnchorIdRef.current;
        if (anchorToSync) {
          const el = document.getElementById(anchorToSync);
          if (el) {
            el.scrollIntoView({ behavior: "smooth", block: "nearest" });
          }
          pendingAnchorIdRef.current = null;
        }

        if (syncTimeoutRef.current) clearTimeout(syncTimeoutRef.current);
        syncTimeoutRef.current = setTimeout(() => {
          isSyncingRef.current = false;
          if (pendingAnchorIdRef.current) {
            const trailingId = pendingAnchorIdRef.current;
            pendingAnchorIdRef.current = null;
            handleAnchorScroll(trailingId);
          }
        }, 50);
      });
    },
    [syncEnabled]
  );

  return {
    syncScrollContainerProportional,
    handleAnchorScroll,
    isSyncingRef
  };
}
