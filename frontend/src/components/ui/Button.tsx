import React, { forwardRef } from "react";
import { Loader2 } from "lucide-react";

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "outline" | "ghost" | "streak" | "danger";
  size?: "sm" | "md" | "lg";
  isLoading?: boolean;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = "primary",
      size = "md",
      isLoading = false,
      leftIcon,
      rightIcon,
      children,
      className = "",
      disabled,
      ...props
    },
    ref
  ) => {
    const baseStyles =
      "inline-flex items-center justify-center font-medium transition-all duration-150 ease-out select-none focus-visible:outline-2 focus-visible:outline-offset-2 active:scale-[0.98] disabled:opacity-50 disabled:pointer-events-none disabled:active:scale-100";

    const sizeStyles = {
      sm: "h-8 px-3 text-xs rounded-[6px] gap-1.5",
      md: "h-9 px-4 text-sm rounded-[10px] gap-2",
      lg: "h-11 px-5 text-base rounded-[12px] gap-2.5"
    }[size];

    const variantStyles = {
      primary:
        "bg-[#5E51E8] text-white hover:bg-[#4F46E5] active:bg-[#4338CA] dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#6366F1] shadow-xs hover:shadow-elevation-1",
      secondary:
        "bg-[var(--surface-elevated)] text-[var(--text-primary)] hover:bg-[var(--border-calm)] border border-[var(--border-calm)]",
      outline:
        "bg-transparent border border-[var(--border-calm)] text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] hover:border-[var(--text-tertiary)]",
      ghost:
        "bg-transparent text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]",
      streak:
        "bg-[#D97706] text-white hover:bg-[#B45309] dark:bg-[#F59E0B] dark:text-[#0F0F12] shadow-xs",
      danger:
        "bg-rose-600 text-white hover:bg-rose-700 dark:bg-rose-500 dark:text-white shadow-xs"
    }[variant];

    return (
      <button
        ref={ref}
        disabled={disabled || isLoading}
        className={`${baseStyles} ${sizeStyles} ${variantStyles} ${className}`}
        {...props}
      >
        {isLoading ? (
          <Loader2 className="w-4 h-4 animate-spin shrink-0" />
        ) : (
          leftIcon && <span className="shrink-0">{leftIcon}</span>
        )}
        {children}
        {!isLoading && rightIcon && <span className="shrink-0">{rightIcon}</span>}
      </button>
    );
  }
);

Button.displayName = "Button";
