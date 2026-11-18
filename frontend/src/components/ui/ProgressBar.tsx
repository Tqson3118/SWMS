import React from "react";

export interface ProgressBarProps {
  value: number;
  max?: number;
  size?: "sm" | "md" | "lg";
  showLabel?: boolean;
  color?: "brand" | "success" | "streak";
  className?: string;
  ariaLabel?: string;
}

export function ProgressBar({
  value,
  max = 100,
  size = "md",
  showLabel = false,
  color = "brand",
  className = "",
  ariaLabel = "Tiến độ học tập"
}: ProgressBarProps) {
  const percentage = Math.min(100, Math.max(0, Math.round((value / max) * 100)));
  const isComplete = percentage >= 100;

  const heightStyles = {
    sm: "h-1.5",
    md: "h-2.5",
    lg: "h-3.5"
  }[size];

  const barColor = isComplete
    ? "bg-[#059669] dark:bg-[#34D399]"
    : {
        brand: "bg-[#5E51E8] dark:bg-[#818CF8]",
        success: "bg-[#059669] dark:bg-[#34D399]",
        streak: "bg-[#D97706] dark:bg-[#F59E0B]"
      }[color];

  return (
    <div className={`w-full flex flex-col gap-1.5 ${className}`}>
      {showLabel && (
        <div className="flex justify-between items-center text-xs">
          <span className="text-[var(--text-secondary)] font-medium">{ariaLabel}</span>
          <span className="font-semibold text-[var(--text-primary)]">{percentage}%</span>
        </div>
      )}
      <div
        role="progressbar"
        aria-valuenow={percentage}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={ariaLabel}
        className={`w-full ${heightStyles} bg-[var(--surface-elevated)] rounded-full overflow-hidden`}
      >
        <div
          className={`h-full ${barColor} rounded-full transition-all duration-300 ease-out`}
          style={{ width: `${percentage}%` }}
        />
      </div>
    </div>
  );
}
