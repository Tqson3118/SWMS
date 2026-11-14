import { useCallback, useEffect, useRef } from "react";
/**
 * Single-Writer Tab Coordination via Web Locks API (navigator.locks) + BroadcastChannel
 * and 409 "Keep Both" Conflict Fork Handler (Decision B, ADR-002 & ADR-022).
 */
export function useNotebookAutoSave(notebookId, onRemoteVersionUpdate, onConflictForkCreated) {
    const channelRef = useRef(null);
    useEffect(() => {
        if (typeof window === "undefined" || typeof BroadcastChannel === "undefined" || !notebookId) {
            return;
        }
        const ch = new BroadcastChannel("swms_notebook_sync");
        channelRef.current = ch;
        ch.onmessage = (ev) => {
            if (ev.data?.notebookId === notebookId && ev.data?.version) {
                onRemoteVersionUpdate?.(ev.data.version, ev.data.contentAst);
            }
        };
        return () => {
            ch.close();
            channelRef.current = null;
        };
    }, [notebookId, onRemoteVersionUpdate]);
    const saveNotebookWithSingleWriterLock = useCallback(async (params) => {
        const executeSave = async () => {
            try {
                const resp = await fetch(`/api/v1/notebooks/${params.id}`, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        mutationId: crypto.randomUUID(),
                        baseVersion: params.baseVersion,
                        deviceId: "swms-web-tab",
                        title: params.title,
                        contentAstJson: JSON.stringify(params.contentAst)
                    })
                });
                if (resp.status === 409) {
                    const conflictData = await resp.json();
                    onConflictForkCreated?.({
                        message: conflictData.message ||
                            "Phát hiện xung đột nội dung: Đã lưu bản sao riêng để bạn đối chiếu.",
                        serverVersion: conflictData.serverVersion,
                        canonicalContentAst: conflictData.canonicalNotebook?.contentAst,
                        conflictCopyTitle: conflictData.conflictCopyNotebook?.title,
                        conflictCopyId: conflictData.conflictCopyNotebook?.id
                    });
                    return { status: "conflict_kept_both", version: conflictData.serverVersion };
                }
                if (resp.ok) {
                    const saved = await resp.json();
                    channelRef.current?.postMessage({
                        notebookId: params.id,
                        version: saved.version,
                        contentAst: saved.contentAst
                    });
                    return { status: "saved", version: saved.version };
                }
                return { status: "error" };
            }
            catch {
                return { status: "error" };
            }
        };
        if (typeof navigator !== "undefined" && "locks" in navigator) {
            return await navigator.locks.request(`notebook_autosave_${params.id}`, { ifAvailable: true }, async (lock) => {
                if (!lock) {
                    return { status: "deferred" };
                }
                return await executeSave();
            });
        }
        return await executeSave();
    }, [onConflictForkCreated]);
    return { saveNotebookWithSingleWriterLock };
}
