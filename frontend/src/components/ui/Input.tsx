import React, { forwardRef } from "react";

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helperText?: string;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, helperText, leftIcon, rightIcon, className = "", id, ...props }, ref) => {
    const inputId = id || (label ? label.toLowerCase().replace(/\s+/g, "-") : undefined);

    return (
      <div className="w-full flex flex-col gap-1.5">
        {label && (
          <label
            htmlFor={inputId}
            className="text-xs font-semibold text-[var(--text-secondary)] tracking-wide"
          >
            {label}
          </label>
        )}
        <div className="relative flex items-center">
          {leftIcon && (
            <div className="absolute left-3 flex items-center pointer-events-none text-[var(--text-tertiary)]">
              {leftIcon}
            </div>
          )}
          <input
            id={inputId}
            ref={ref}
            className={`w-full h-10 px-3.5 ${leftIcon ? "pl-10" : ""} ${
              rightIcon ? "pr-10" : ""
            } bg-[var(--surface-card)] text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] border ${
              error ? "border-rose-500 focus:border-rose-500" : "border-[var(--border-calm)] focus:border-[var(--accent)]"
            } rounded-[10px] text-sm transition-all outline-none focus:ring-2 ${
              error ? "focus:ring-rose-500/20" : "focus:ring-[var(--accent)]/20"
            } disabled:opacity-50 disabled:bg-[var(--surface-elevated)] ${className}`}
            {...props}
          />
          {rightIcon && (
            <div className="absolute right-3 flex items-center text-[var(--text-tertiary)]">
              {rightIcon}
            </div>
          )}
        </div>
        {error ? (
          <p className="text-xs text-rose-500 font-medium">{error}</p>
        ) : helperText ? (
          <p className="text-xs text-[var(--text-tertiary)]">{helperText}</p>
        ) : null}
      </div>
    );
  }
);

Input.displayName = "Input";
