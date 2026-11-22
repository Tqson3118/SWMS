"use client";

import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";

export interface SaveWriteInFsrsParams {
  front: string;
  back: string;
  lessonId?: string | null;
  sourceTitle: string;
  tags: string[];
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
}

/**
 * Persists a Flashcard FSRS card via POST /api/v1/flashcards with `deduplicateByFront: true`
 * to avoid duplicate cards during manual saves or auto-saves on failed write-in attempts.
 */
export async function saveWriteInFlashcardDeduplicated(
  params: SaveWriteInFsrsParams
): Promise<{ ok: boolean; deduplicated: boolean }> {
  const cleanFront = (params.front || "").trim();
  const cleanBack = (params.back || "").trim();
  if (!cleanFront || !cleanBack) {
    return { ok: false, deduplicated: false };
  }

  try {
    const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
    const res = await fetch("/api/v1/flashcards", {
      method: "POST",
      headers,
      body: JSON.stringify({
        lessonId: params.lessonId || null,
        documentId: null,
        annotationId: null,
        cardType: "standard",
        sourceTitleSnapshot: params.sourceTitle,
        sourceQuoteSnapshot: cleanFront.slice(0, 240),
        frontText: cleanFront,
        backText: cleanBack,
        tagNames: params.tags,
        deduplicateByFront: true,
      }),
    });

    if (res.ok) {
      const deduplicated = res.headers.get("X-Swms-Flashcard-Deduplicated") === "true";
      invalidateDashboardCache();
      if (typeof window !== "undefined") {
        window.dispatchEvent(new Event("swms:data-mutated"));
      }
      return { ok: true, deduplicated };
    }

    if (params.onCreateFlashcardQuick) {
      await params.onCreateFlashcardQuick(cleanFront, cleanBack);
      return { ok: true, deduplicated: false };
    }
    return { ok: false, deduplicated: false };
  } catch {
    if (params.onCreateFlashcardQuick) {
      try {
        await params.onCreateFlashcardQuick(cleanFront, cleanBack);
        return { ok: true, deduplicated: false };
      } catch {
        return { ok: false, deduplicated: false };
      }
    }
    return { ok: false, deduplicated: false };
  }
}

/**
 * Executes arbitrary JS snippets inside an isolated Blob Web Worker with a hard timeout
 * to capture console output (predict_output) or run function test cases (js_sandbox / bug_fix).
 */
export function runJsFunctionInWorker(params: {
  code: string;
  functionName: string;
  testCases: Array<{ name: string; input: any[]; expected: any }>;
  timeLimitMs?: number;
}): Promise<{
  ok: boolean;
  results?: Array<{
    name: string;
    passed: boolean;
    input: any[];
    actual: any;
    expected: any;
    error?: string;
  }>;
  logs: string[];
  error?: string;
  executionTimeMs: number;
}> {
  const timeoutMs = params.timeLimitMs ?? 2000;
  const startTime = typeof performance !== "undefined" ? performance.now() : Date.now();

  return new Promise((resolve) => {
    if (typeof Worker === "undefined" || typeof Blob === "undefined") {
      resolve({
        ok: false,
        logs: [],
        error: "Trình duyệt hiện tại không hỗ trợ Web Worker để chạy mã JavaScript.",
        executionTimeMs: 0,
      });
      return;
    }

    const safeFnName = (params.functionName || "solution").replace(/[^A-Za-z0-9_$]/g, "");
    const workerScript = `
self.onmessage = function(e) {
  const { code, functionName, testCases } = e.data;
  const capturedLogs = [];

  const formatVal = (v) => {
    if (typeof v === 'object' && v !== null) {
      try { return JSON.stringify(v); } catch { return String(v); }
    }
    return String(v);
  };

  const customConsole = {
    log: function(...args) { capturedLogs.push(args.map(formatVal).join(' ')); },
    warn: function(...args) { capturedLogs.push('[CẢNH BÁO] ' + args.map(formatVal).join(' ')); },
    error: function(...args) { capturedLogs.push('[LỖI] ' + args.map(formatVal).join(' ')); }
  };

  const deepEqual = (a, b) => {
    if (a === b) return true;
    if (Number.isNaN(a) && Number.isNaN(b)) return true;
    if (a === null || b === null || typeof a !== 'object' || typeof b !== 'object') return false;
    if (Array.isArray(a) !== Array.isArray(b)) return false;
    if (Array.isArray(a)) {
      if (a.length !== b.length) return false;
      for (let i = 0; i < a.length; i++) {
        if (!deepEqual(a[i], b[i])) return false;
      }
      return true;
    }
    const ka = Object.keys(a).sort();
    const kb = Object.keys(b).sort();
    if (ka.length !== kb.length) return false;
    for (let i = 0; i < ka.length; i++) {
      if (ka[i] !== kb[i] || !deepEqual(a[ka[i]], b[kb[i]])) return false;
    }
    return true;
  };

  try {
    const resolver = functionName
      ? 'typeof ' + functionName + ' === "function" ? ' + functionName + ' : '
      : '';
    const wrapped = new Function(
      'console',
      code + ';\\nreturn (' + resolver +
        '(typeof twoSum === "function" ? twoSum : ' +
        '(typeof deepClone === "function" ? deepClone : ' +
        '(typeof simulateLRU === "function" ? simulateLRU : ' +
        '(typeof solution === "function" ? solution : null)))));'
    );
    const targetFn = wrapped(customConsole);

    if (typeof targetFn !== 'function') {
      self.postMessage({
        type: 'ERROR',
        error: 'Không tìm thấy hàm "' + (functionName || 'solution') + '" trong mã nguồn. Hãy giữ đúng tên hàm theo đề bài.',
        logs: capturedLogs
      });
      return;
    }

    const results = [];
    for (const tc of testCases) {
      try {
        const clonedArgs = JSON.parse(JSON.stringify(tc.input));
        let actual = targetFn(...clonedArgs);
        let passed = false;
        if (tc.expected === 'function' && typeof actual === 'function') {
          passed = true;
          actual = 'function';
        } else {
          passed = deepEqual(actual, tc.expected);
        }
        results.push({
          name: tc.name,
          passed,
          input: tc.input,
          actual,
          expected: tc.expected
        });
      } catch (tcErr) {
        results.push({
          name: tc.name,
          passed: false,
          input: tc.input,
          actual: null,
          expected: tc.expected,
          error: tcErr && tcErr.message ? tcErr.message : String(tcErr)
        });
      }
    }

    self.postMessage({ type: 'SUCCESS', results, logs: capturedLogs });
  } catch (parseErr) {
    self.postMessage({
      type: 'ERROR',
      error: parseErr && parseErr.message ? parseErr.message : String(parseErr),
      logs: capturedLogs
    });
  }
};
`;

    const blob = new Blob([workerScript], { type: "application/javascript" });
    const blobUrl = URL.createObjectURL(blob);
    let worker: Worker | null = null;
    let settled = false;

    const finish = (payload: any) => {
      if (settled) return;
      settled = true;
      if (worker) {
        try {
          worker.terminate();
        } catch {}
      }
      URL.revokeObjectURL(blobUrl);
      const endTime = typeof performance !== "undefined" ? performance.now() : Date.now();
      resolve({
        ...payload,
        executionTimeMs: Math.max(1, Math.round(endTime - startTime)),
      });
    };

    const timer = setTimeout(() => {
      finish({
        ok: false,
        logs: [],
        error: `⏱️ QUÁ THỜI GIAN (${timeoutMs}ms): Mã thực thi vượt giới hạn! Hãy kiểm tra vòng lặp vô tận hoặc đệ quy không có điều kiện dừng.`,
      });
    }, timeoutMs);

    try {
      worker = new Worker(blobUrl);
      worker.onmessage = (event) => {
        clearTimeout(timer);
        const data = event.data;
        if (data?.type === "SUCCESS") {
          finish({ ok: true, results: data.results || [], logs: data.logs || [] });
        } else {
          finish({
            ok: false,
            logs: data?.logs || [],
            error: data?.error || "Lỗi thực thi trong Web Worker.",
          });
        }
      };
      worker.onerror = (err) => {
        clearTimeout(timer);
        finish({
          ok: false,
          logs: [],
          error: `Lỗi Worker Runtime: ${err.message || "Không xác định"}`,
        });
      };
      worker.postMessage({
        code: params.code,
        functionName: safeFnName,
        testCases: params.testCases,
      });
    } catch (err: any) {
      clearTimeout(timer);
      finish({
        ok: false,
        logs: [],
        error: `Không thể khởi tạo Web Worker: ${err?.message || String(err)}`,
      });
    }
  });
}

/**
 * Executes a JavaScript snippet inside an isolated Web Worker with timeout
 * and returns the exact captured console.log lines (for Predict Output mode).
 */
export function runJsPredictOutputInWorker(params: {
  code: string;
  timeLimitMs?: number;
}): Promise<{
  ok: boolean;
  output: string;
  logs: string[];
  error?: string;
  executionTimeMs: number;
}> {
  const timeoutMs = params.timeLimitMs ?? 2000;
  const startTime = typeof performance !== "undefined" ? performance.now() : Date.now();

  return new Promise((resolve) => {
    if (typeof Worker === "undefined" || typeof Blob === "undefined") {
      resolve({
        ok: false,
        output: "",
        logs: [],
        error: "Trình duyệt hiện tại không hỗ trợ Web Worker.",
        executionTimeMs: 0,
      });
      return;
    }

    const workerScript = `
self.onmessage = async function(e) {
  const { code } = e.data;
  const logs = [];
  const fmt = (v) => {
    if (v === undefined) return 'undefined';
    if (v === null) return 'null';
    if (typeof v === 'object') {
      try { return JSON.stringify(v); } catch { return String(v); }
    }
    return String(v);
  };
  const customConsole = {
    log: (...args) => logs.push(args.map(fmt).join(' ')),
    warn: (...args) => logs.push(args.map(fmt).join(' ')),
    error: (...args) => logs.push(args.map(fmt).join(' '))
  };
  try {
    const fn = new Function('console', '"use strict";\\n' + code);
    const maybePromise = fn(customConsole);
    if (maybePromise && typeof maybePromise.then === 'function') {
      await maybePromise;
    }
    // Wait one short macrotask tick so microtasks (Promise.resolve().then) flush in order
    setTimeout(() => {
      self.postMessage({ type: 'SUCCESS', logs });
    }, 30);
  } catch (err) {
    self.postMessage({
      type: 'ERROR',
      error: err && err.message ? err.message : String(err),
      logs
    });
  }
};
`;

    const blob = new Blob([workerScript], { type: "application/javascript" });
    const blobUrl = URL.createObjectURL(blob);
    let worker: Worker | null = null;
    let settled = false;

    const finish = (payload: any) => {
      if (settled) return;
      settled = true;
      if (worker) {
        try {
          worker.terminate();
        } catch {}
      }
      URL.revokeObjectURL(blobUrl);
      const endTime = typeof performance !== "undefined" ? performance.now() : Date.now();
      resolve({
        ...payload,
        executionTimeMs: Math.max(1, Math.round(endTime - startTime)),
      });
    };

    const timer = setTimeout(() => {
      finish({
        ok: false,
        output: "",
        logs: [],
        error: `⏱️ Quá thời gian thực thi (${timeoutMs}ms) trong Web Worker.`,
      });
    }, timeoutMs);

    try {
      worker = new Worker(blobUrl);
      worker.onmessage = (event) => {
        clearTimeout(timer);
        const data = event.data;
        if (data?.type === "SUCCESS") {
          const logs: string[] = Array.isArray(data.logs) ? data.logs : [];
          finish({ ok: true, output: logs.join("\n"), logs });
        } else {
          const logs: string[] = Array.isArray(data?.logs) ? data.logs : [];
          finish({
            ok: false,
            output: logs.join("\n"),
            logs,
            error: data?.error || "Lỗi thực thi đoạn mã JS.",
          });
        }
      };
      worker.onerror = (err) => {
        clearTimeout(timer);
        finish({
          ok: false,
          output: "",
          logs: [],
          error: `Lỗi Worker Runtime: ${err.message || "Không xác định"}`,
        });
      };
      worker.postMessage({ code: params.code });
    } catch (err: any) {
      clearTimeout(timer);
      finish({
        ok: false,
        output: "",
        logs: [],
        error: `Không thể khởi chạy Web Worker: ${err?.message || String(err)}`,
      });
    }
  });
}
