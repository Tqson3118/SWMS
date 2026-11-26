"use client";

import React, { useState } from "react";
import { Sparkles, Loader2, CheckCircle2, AlertCircle, Layers } from "lucide-react";

export interface AutoGenerateFsrsCardProps {
  lessonId: string;
  getHeaders: (extra?: Record<string, string>) => Record<string, string>;
  totalCardsCount: number;
  onCardsGenerated: (newCount: number, total: number) => void;
  onOpenFlashcardsDrawer?: () => void;
}

export function AutoGenerateFsrsCard({
  lessonId,
  getHeaders,
  totalCardsCount,
  onCardsGenerated,
  onOpenFlashcardsDrawer,
}: AutoGenerateFsrsCardProps) {
  const [isGenerating, setIsGenerating] = useState(false);
  const [lastResult, setLastResult] = useState<{ createdCount: number; totalCards: number } | null>(
    null
  );
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleGenerate = async () => {
    if (!lessonId || isGenerating) return;

    setIsGenerating(true);
    setErrorMessage(null);

    try {
      const resp = await fetch(`/api/v1/lessons/${lessonId}/auto-generate-fsrs-cards`, {
        method: "POST",
        headers: getHeaders({ "Content-Type": "application/json" }),
      });

      if (!resp.ok) {
        let msg = `Không thể tạo thẻ (HTTP ${resp.status})`;
        try {
          const errData = await resp.json();
          if (errData.detail || errData.error || errData.message) {
            msg = errData.detail || errData.error || errData.message;
          }
        } catch {
          // ignore json parse error
        }
        setErrorMessage(msg);
        return;
      }

      const data = await resp.json();
      const createdCount = Number(data.createdCount ?? 0);
      const total = Number(data.totalCards ?? (totalCardsCount + createdCount));

      setLastResult({ createdCount, totalCards: total });
      onCardsGenerated(createdCount, total);
    } catch {
      setErrorMessage("Lỗi kết nối khi tự sinh thẻ ghi nhớ FSRS. Vui lòng thử lại sau.");
    } finally {
      setIsGenerating(false);
    }
  };

  return (
    <div
      data-testid="auto-generate-fsrs-card"
      className="my-6 rounded-xl border border-indigo-200/80 dark:border-indigo-800/60 bg-[var(--surface-card)] bg-gradient-to-br from-indigo-50/60 via-[var(--surface-card)] to-purple-50/40 dark:from-indigo-950/35 dark:via-[var(--surface-card)] dark:to-purple-950/20 p-4 sm:p-5 shadow-xs font-sans"
    >
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div className="space-y-1 min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="inline-flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400">
              <Sparkles className="w-3.5 h-3.5" />
              <span>Ôn tập chủ động (FSRS v5)</span>
            </span>
            {totalCardsCount > 0 && (
              <span className="text-[11px] font-semibold text-text-secondary bg-[var(--surface-elevated)] px-2 py-0.5 rounded-full border border-[var(--border-calm)]">
                {totalCardsCount} thẻ hiện có
              </span>
            )}
          </div>
          <p className="text-xs text-text-secondary leading-relaxed">
            Tự động trích xuất các định nghĩa cốt lõi, công thức và cặp khái niệm quan trọng trong
            bài học vào thuật toán lặp lại ngắt quãng thông minh FSRS v5.
          </p>
        </div>

        <div className="flex items-center gap-2 shrink-0">
          <button
            type="button"
            disabled={isGenerating}
            onClick={handleGenerate}
            data-testid="auto-generate-fsrs-btn"
            className="inline-flex items-center justify-center gap-1.5 min-h-9 px-3.5 py-2 rounded-lg bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:hover:bg-[#A5B4FC] dark:text-[#0F0F12] text-xs font-semibold shadow-xs disabled:opacity-60 transition cursor-pointer"
          >
            {isGenerating ? (
              <>
                <Loader2 className="w-3.5 h-3.5 animate-spin" />
                <span>Đang trích xuất thẻ...</span>
              </>
            ) : (
              <>
                <Sparkles className="w-3.5 h-3.5" />
                <span>Tự sinh thẻ ghi nhớ từ bài này</span>
              </>
            )}
          </button>
        </div>
      </div>

      {lastResult && (
        <div className="mt-3.5 pt-3 border-t border-indigo-100 dark:border-indigo-900/40 flex flex-wrap items-center justify-between gap-2 text-xs">
          <div className="inline-flex items-center gap-1.5 text-emerald-600 dark:text-emerald-400 font-medium">
            <CheckCircle2 className="w-4 h-4 shrink-0" />
            <span>
              {lastResult.createdCount > 0
                ? `Đã tạo thành công ${lastResult.createdCount} thẻ mới! (Tổng cộng: ${lastResult.totalCards} thẻ)`
                : `Bài học đã có đủ thẻ ghi nhớ (${lastResult.totalCards} thẻ). Không có khái niệm mới cần bổ sung.`}
            </span>
          </div>

          {onOpenFlashcardsDrawer && (
            <button
              type="button"
              onClick={onOpenFlashcardsDrawer}
              className="inline-flex items-center gap-1 text-indigo-600 dark:text-indigo-400 hover:underline font-semibold cursor-pointer"
            >
              <Layers className="w-3.5 h-3.5" />
              <span>Xem danh sách thẻ trong Sổ tay →</span>
            </button>
          )}
        </div>
      )}

      {errorMessage && (
        <div className="mt-3.5 pt-3 border-t border-red-100 dark:border-red-900/40 flex items-center gap-1.5 text-xs text-red-600 dark:text-red-400">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}
    </div>
  );
}
