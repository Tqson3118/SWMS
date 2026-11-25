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

const FOCUSABLE_SELECTOR =
  'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])';

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
  const previousActiveElementRef = useRef<HTMLElement | null>(null);
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;

  useEffect(() => {
    if (!isOpen) return;

    previousActiveElementRef.current = document.activeElement as HTMLElement | null;
    document.body.style.overflow = "hidden";

    const getFocusableElements = () => {
      const container = dialogRef.current;
      if (!container) return [];
      return Array.from(container.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR)).filter(
        (el) => !el.hasAttribute("disabled") && el.getClientRects().length > 0
      );
    };

    const rafId = window.requestAnimationFrame(() => {
      const focusables = getFocusableElements();
      if (focusables.length > 0) {
        focusables[0].focus();
      } else {
        dialogRef.current?.focus();
      }
    });

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onCloseRef.current();
        return;
      }

      if (e.key === "Tab") {
        const focusables = getFocusableElements();
        if (focusables.length === 0) {
          e.preventDefault();
          dialogRef.current?.focus();
          return;
        }

        const first = focusables[0];
        const last = focusables[focusables.length - 1];
        const activeEl = document.activeElement as HTMLElement | null;
        const isInside = Boolean(activeEl && dialogRef.current?.contains(activeEl));

        if (e.shiftKey) {
          if (!isInside || activeEl === first) {
            e.preventDefault();
            last.focus();
          }
        } else if (!isInside || activeEl === last) {
          e.preventDefault();
          first.focus();
        }
      }
    };

    document.addEventListener("keydown", handleKeyDown);

    return () => {
      window.cancelAnimationFrame(rafId);
      document.removeEventListener("keydown", handleKeyDown);
      document.body.style.overflow = "";
      const prevEl = previousActiveElementRef.current;
      if (prevEl && prevEl.isConnected) {
        prevEl.focus();
      }
      previousActiveElementRef.current = null;
    };
  }, [isOpen]);

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
      aria-describedby={description ? "dialog-description" : undefined}
      className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 overflow-hidden"
    >
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/45 backdrop-blur-xs swms-animate-fade-in"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Modal Dialog Content */}
      <div
        ref={dialogRef}
        tabIndex={-1}
        className={`relative w-full ${maxWidthStyles} max-h-[90vh] flex flex-col bg-[var(--surface-card)] border border-[var(--border-calm)] rounded-[18px] shadow-elevation-3 overflow-hidden z-10 swms-animate-scale-in focus:outline-none ${className}`}
      >
        {(title || showCloseButton) && (
          <div className="shrink-0 flex items-center justify-between gap-3 p-5 pb-3 border-b border-[var(--border-calm)]">
            <div className="min-w-0">
              {title && (
                <h2
                  id="dialog-title"
                  className="text-lg font-semibold text-[var(--text-primary)] tracking-tight"
                >
                  {title}
                </h2>
              )}
              {description && (
                <p id="dialog-description" className="text-xs text-[var(--text-secondary)] mt-0.5">
                  {description}
                </p>
              )}
            </div>
            {showCloseButton && (
              <button
                type="button"
                onClick={onClose}
                aria-label="Đóng cửa sổ"
                className="shrink-0 p-1.5 rounded-[8px] text-[var(--text-tertiary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)] transition-colors cursor-pointer"
              >
                <X className="w-4 h-4" />
              </button>
            )}
          </div>
        )}
        <div className="p-5 overflow-y-auto swms-thin-scrollbar flex-1 min-h-0">{children}</div>
      </div>
    </div>
  );
}
