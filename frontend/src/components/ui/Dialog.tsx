import React, { useEffect, useRef } from "react";
import { X } from "lucide-react";

export interface DialogProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  description?: string;
  maxWidth?: "sm" | "md" | "lg" | "xl" | "2xl" | "full";
  children: React.ReactNode;
  className?: string;
  showCloseButton?: boolean;
}

export function Dialog({
  isOpen,
  onClose,
  title,
  description,
  maxWidth = "lg",
  children,
  className = "",
  showCloseButton = true
}: DialogProps) {
  const dialogRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && isOpen) {
        onClose();
      }
    };
    if (isOpen) {
      document.addEventListener("keydown", handleKeyDown);
      document.body.style.overflow = "hidden";
    }
    return () => {
      document.removeEventListener("keydown", handleKeyDown);
      document.body.style.overflow = "";
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const maxWidthStyles = {
    sm: "max-w-sm",
    md: "max-w-md",
    lg: "max-w-lg",
    xl: "max-w-xl",
    "2xl": "max-w-2xl",
    full: "max-w-4xl"
  }[maxWidth];

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby={title ? "dialog-title" : undefined}
      className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 overflow-y-auto"
    >
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/40 backdrop-blur-xs transition-opacity duration-200"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Modal Dialog Content */}
      <div
        ref={dialogRef}
        className={`relative w-full ${maxWidthStyles} bg-[var(--surface-card)] border border-[var(--border-calm)] rounded-[18px] shadow-elevation-3 overflow-hidden z-10 transition-all duration-200 scale-100 ${className}`}
      >
        {(title || showCloseButton) && (
          <div className="flex items-center justify-between p-5 pb-3 border-b border-[var(--border-calm)]">
            <div>
              {title && (
                <h2
                  id="dialog-title"
                  className="text-lg font-semibold text-[var(--text-primary)] tracking-tight"
                >
                  {title}
                </h2>
              )}
              {description && (
                <p className="text-xs text-[var(--text-secondary)] mt-0.5">{description}</p>
              )}
            </div>
            {showCloseButton && (
              <button
                type="button"
                onClick={onClose}
                aria-label="Đóng cửa sổ"
                className="p-1.5 rounded-[8px] text-[var(--text-tertiary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition-colors"
              >
                <X className="w-4 h-4" />
              </button>
            )}
          </div>
        )}
        <div className="p-5">{children}</div>
      </div>
    </div>
  );
}
