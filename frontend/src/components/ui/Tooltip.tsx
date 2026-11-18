import React, { useState } from "react";

export interface TooltipProps {
  content: React.ReactNode;
  shortcut?: string;
  side?: "top" | "bottom" | "left" | "right";
  children: React.ReactElement;
  className?: string;
}

export function Tooltip({
  content,
  shortcut,
  side = "top",
  children,
  className = ""
}: TooltipProps) {
  const [isVisible, setIsVisible] = useState(false);

  const sideStyles = {
    top: "bottom-full left-1/2 -translate-x-1/2 mb-2",
    bottom: "top-full left-1/2 -translate-x-1/2 mt-2",
    left: "right-full top-1/2 -translate-y-1/2 mr-2",
    right: "left-full top-1/2 -translate-y-1/2 ml-2"
  }[side];

  return (
    <div
      className="relative inline-flex"
      onMouseEnter={() => setIsVisible(true)}
      onMouseLeave={() => setIsVisible(false)}
      onFocus={() => setIsVisible(true)}
      onBlur={() => setIsVisible(false)}
    >
      {children}
      {isVisible && (
        <div
          role="tooltip"
          className={`absolute z-50 pointer-events-none px-2.5 py-1 text-xs font-medium text-[var(--text-primary)] bg-[var(--surface-card)] border border-[var(--border-calm)] rounded-[6px] shadow-elevation-2 flex items-center gap-1.5 whitespace-nowrap animate-in fade-in-0 duration-150 ${sideStyles} ${className}`}
        >
          <span>{content}</span>
          {shortcut && (
            <kbd className="swms-kbd text-[10px] px-1 py-0.5 ml-1">{shortcut}</kbd>
          )}
        </div>
      )}
    </div>
  );
}
