import React from "react";
import { LucideIcon } from "lucide-react";

export interface StatCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon?: LucideIcon | React.ReactNode;
  trend?: {
    value: string | number;
    isPositive?: boolean;
    label?: string;
  };
  variant?: "default" | "streak" | "brand" | "success";
  className?: string;
}

export function StatCard({
  title,
  value,
  subtitle,
  icon: Icon,
  trend,
  variant = "default",
  className = ""
}: StatCardProps) {
  const variantStyles = {
    default: "border-[var(--border-calm)] hover:border-[var(--accent)]/50",
    streak: "border-amber-200/80 dark:border-amber-900/50 hover:border-amber-400",
    brand: "border-[#5E51E8]/30 dark:border-[#818CF8]/30 hover:border-[#5E51E8]",
    success: "border-emerald-200/80 dark:border-emerald-900/50 hover:border-emerald-400"
  }[variant];

  const iconBgStyles = {
    default: "bg-[var(--surface-elevated)] text-[var(--text-secondary)]",
    streak: "bg-amber-50 text-[#D97706] dark:bg-amber-950/60 dark:text-[#FBBF24]",
    brand: "bg-[#EEF0FD] text-[#5E51E8] dark:bg-[#1E1B4B] dark:text-[#818CF8]",
    success: "bg-emerald-50 text-[#059669] dark:bg-emerald-950/60 dark:text-[#34D399]"
  }[variant];

  const renderIcon = () => {
    if (!Icon) return null;
    if (typeof Icon === "function") {
      const LucideComp = Icon as LucideIcon;
      return <LucideComp className="w-5 h-5 shrink-0" />;
    }
    return Icon;
  };

  return (
    <div
      className={`bg-[var(--surface-card)] border rounded-[14px] p-5 shadow-elevation-1 transition-all duration-200 flex flex-col justify-between ${variantStyles} ${className}`}
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-xs font-medium text-[var(--text-secondary)] tracking-wide">{title}</p>
          <p className="text-2xl font-bold text-[var(--text-primary)] mt-1.5 tracking-tight">
            {value}
          </p>
        </div>
        {Icon && (
          <div className={`p-2.5 rounded-[10px] shrink-0 transition-transform ${iconBgStyles}`}>
            {renderIcon()}
          </div>
        )}
      </div>

      {(subtitle || trend) && (
        <div className="mt-3 pt-2.5 border-t border-[var(--border-calm)] flex items-center justify-between text-xs">
          {subtitle && <span className="text-[var(--text-tertiary)]">{subtitle}</span>}
          {trend && (
            <span
              className={`font-semibold ml-auto flex items-center gap-1 ${
                trend.isPositive !== false
                  ? "text-emerald-600 dark:text-emerald-400"
                  : "text-rose-600 dark:text-rose-400"
              }`}
            >
              {trend.isPositive !== false ? "+" : ""}
              {trend.value} {trend.label || ""}
            </span>
          )}
        </div>
      )}
    </div>
  );
}
