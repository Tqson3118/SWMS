"use client";

import React from "react";
import { ChevronDown, Layers, Loader2 } from "lucide-react";

export interface ShowMoreFooterProps {
  shownCount: number;
  totalCount: number;
  stepSize?: number;
  itemLabel: string;
  onShowMore: () => void;
  onShowAll?: () => void;
  isLoadingMore?: boolean;
  testIdPrefix: string;
}

/**
 * Progressive reveal footer for card/list grids: renders "Hiển thị thêm" and "Xem tất cả"
 * controls until all items in the current view are visible.
 */
export function ShowMoreFooter({
  shownCount,
  totalCount,
  stepSize = 8,
  itemLabel,
  onShowMore,
  onShowAll,
  isLoadingMore = false,
  testIdPrefix,
}: ShowMoreFooterProps) {
  if (totalCount <= 0 || shownCount >= totalCount) {
    return null;
  }

  const remaining = Math.max(0, totalCount - shownCount);
  const nextBatch = Math.min(stepSize, remaining);

  return (
    <div
      data-testid={`${testIdPrefix}-pagination-footer`}
      className="mt-6 flex flex-col sm:flex-row items-center justify-between gap-3 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] px-4 py-3 shadow-2xs"
    >
      <span className="text-xs text-[var(--text-secondary)]">
        Đang hiển thị{" "}
        <strong className="font-semibold text-[var(--text-primary)] tabular-nums">
          {shownCount}
        </strong>{" "}
        /{" "}
        <strong className="font-semibold text-[var(--text-primary)] tabular-nums">
          {totalCount}
        </strong>{" "}
        {itemLabel}
      </span>

      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          data-testid={`${testIdPrefix}-show-more-btn`}
          disabled={isLoadingMore}
          onClick={onShowMore}
          className="inline-flex items-center gap-1.5 min-h-9 px-3.5 py-1.5 rounded-lg bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC] text-xs font-semibold shadow-xs disabled:opacity-60 transition cursor-pointer"
        >
          {isLoadingMore ? (
            <Loader2 className="w-3.5 h-3.5 animate-spin" />
          ) : (
            <ChevronDown className="w-3.5 h-3.5" />
          )}
          <span>Hiển thị thêm (+{nextBatch})</span>
        </button>

        {onShowAll && remaining > nextBatch && (
          <button
            type="button"
            data-testid={`${testIdPrefix}-show-all-btn`}
            disabled={isLoadingMore}
            onClick={onShowAll}
            className="inline-flex items-center gap-1.5 min-h-9 px-3 py-1.5 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] hover:bg-[var(--surface-card)] text-[var(--text-secondary)] hover:text-[var(--text-primary)] text-xs font-semibold disabled:opacity-60 transition cursor-pointer"
          >
            <Layers className="w-3.5 h-3.5" />
            <span>Xem tất cả ({totalCount})</span>
          </button>
        )}
      </div>
    </div>
  );
}
