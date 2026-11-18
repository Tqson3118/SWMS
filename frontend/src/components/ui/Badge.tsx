import React from "react";

export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: "neutral" | "brand" | "streak" | "success" | "warning" | "danger";
  size?: "sm" | "md";
}

export function Badge({
  variant = "neutral",
  size = "md",
  className = "",
  children,
  ...props
}: BadgeProps) {
  const sizeStyles = {
    sm: "px-2 py-0.5 text-[11px] font-medium leading-none rounded-[5px]",
    md: "px-2.5 py-1 text-xs font-medium leading-none rounded-[6px]"
  }[size];

  const variantStyles = {
    neutral:
      "bg-[var(--surface-elevated)] text-[var(--text-secondary)] border border-[var(--border-calm)]",
    brand:
      "bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:text-[#A5B4FC] border border-[#5E51E8]/20",
    streak:
      "bg-[#FEF3C7] text-[#B45309] dark:bg-[#451A03] dark:text-[#FBBF24] border border-[#F59E0B]/20",
    success:
      "bg-[#D1FAE5] text-[#047857] dark:bg-[#064E3B] dark:text-[#34D399] border border-[#10B981]/20",
    warning:
      "bg-amber-50 text-amber-800 dark:bg-amber-950/60 dark:text-amber-300 border border-amber-200 dark:border-amber-800/40",
    danger:
      "bg-rose-50 text-rose-700 dark:bg-rose-950/60 dark:text-rose-300 border border-rose-200 dark:border-rose-800/40"
  }[variant];

  return (
    <span
      className={`inline-flex items-center gap-1.5 transition-colors select-none ${sizeStyles} ${variantStyles} ${className}`}
      {...props}
    >
      {children}
    </span>
  );
}
