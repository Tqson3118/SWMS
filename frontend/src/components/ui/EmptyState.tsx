import React from "react";
import Link from "next/link";
import { LucideIcon, Sparkles } from "lucide-react";
import { Button } from "./Button";

export interface EmptyStateProps {
  icon?: LucideIcon | React.ReactNode;
  title: string;
  description?: string;
  action?: {
    label: string;
    onClick?: () => void;
    href?: string;
    testId?: string;
    icon?: React.ReactNode;
  };
  secondaryAction?: {
    label: string;
    onClick?: () => void;
    href?: string;
    testId?: string;
  };
  className?: string;
}

export function EmptyState({
  icon: Icon,
  title,
  description,
  action,
  secondaryAction,
  className = ""
}: EmptyStateProps) {
  const renderIcon = () => {
    if (!Icon) {
      return <Sparkles className="w-8 h-8 text-[#5E51E8] dark:text-[#818CF8]" />;
    }
    if (React.isValidElement(Icon)) {
      return Icon;
    }
    const Comp = Icon as React.ComponentType<{ className?: string }>;
    return <Comp className="w-8 h-8 text-[#5E51E8] dark:text-[#818CF8]" />;
  };

  return (
    <div
      className={`flex flex-col items-center justify-center text-center p-8 sm:p-12 border border-dashed border-[var(--border-calm)] rounded-[16px] bg-[var(--surface-card)]/50 ${className}`}
    >
      <div className="w-16 h-16 rounded-full bg-[#EEF0FD] dark:bg-[#1E1B4B] flex items-center justify-center mb-4 shadow-xs">
        {renderIcon()}
      </div>
      <h3 className="text-base font-semibold text-[var(--text-primary)] max-w-sm tracking-tight">
        {title}
      </h3>
      {description && (
        <p className="text-sm text-[var(--text-secondary)] max-w-md mt-1.5 leading-relaxed">
          {description}
        </p>
      )}

      {(action || secondaryAction) && (
        <div className="flex flex-wrap items-center justify-center gap-3 mt-6">
          {action && (
            action.href ? (
              <Link href={action.href}>
                <Button
                  data-testid={action.testId}
                  leftIcon={action.icon}
                  variant="primary"
                >
                  {action.label}
                </Button>
              </Link>
            ) : (
              <Button
                data-testid={action.testId}
                onClick={action.onClick}
                leftIcon={action.icon}
                variant="primary"
              >
                {action.label}
              </Button>
            )
          )}
          {secondaryAction && (
            secondaryAction.href ? (
              <Link href={secondaryAction.href}>
                <Button data-testid={secondaryAction.testId} variant="outline">
                  {secondaryAction.label}
                </Button>
              </Link>
            ) : (
              <Button
                data-testid={secondaryAction.testId}
                onClick={secondaryAction.onClick}
                variant="outline"
              >
                {secondaryAction.label}
              </Button>
            )
          )}
        </div>
      )}
    </div>
  );
}
