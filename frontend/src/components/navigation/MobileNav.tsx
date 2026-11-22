"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  BookOpen,
  FileText,
  NotebookPen,
  Brain,
  Search,
  Target,
} from "lucide-react";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";

interface MobileNavItem {
  href: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  matchPrefix?: boolean;
}

const MOBILE_NAV_ITEMS: MobileNavItem[] = [
  { href: "/app", label: "Hôm nay", icon: LayoutDashboard },
  { href: "/app/courses", label: "Khóa học", icon: BookOpen, matchPrefix: true },
  { href: "/app/practice", label: "Luyện tập", icon: Target, matchPrefix: true },
  { href: "/app/documents", label: "Tài liệu", icon: FileText, matchPrefix: true },
  { href: "/app/notes", label: "Sổ tay", icon: NotebookPen, matchPrefix: true },
  { href: "/app/review", label: "Ôn tập", icon: Brain, matchPrefix: true },
];

export function MobileNav() {
  const pathname = usePathname();
  const { setCommandPaletteOpen } = useWorkspaceStore();

  const isActive = (item: MobileNavItem) => {
    if (item.matchPrefix) return pathname.startsWith(item.href);
    return pathname === item.href;
  };

  return (
    <nav
      data-testid="mobile-bottom-nav"
      className="md:hidden fixed bottom-0 left-0 right-0 z-50 h-14 bg-[var(--surface-card)]/95 backdrop-blur-md border-t border-[var(--border-calm)] flex items-center justify-around px-0.5 safe-area-bottom shadow-elevation-2"
    >
      {MOBILE_NAV_ITEMS.slice(0, 3).map((item) => {
        const Icon = item.icon;
        const active = isActive(item);
        return (
          <Link
            key={item.href}
            href={item.href}
            data-testid={`mobile-nav-${item.href.split("/").pop()}`}
            className={`flex flex-col items-center justify-center gap-0.5 flex-1 min-w-0 min-h-[44px] h-full px-0.5 transition ${
              active
                ? "text-[var(--accent)] font-semibold"
                : "text-[var(--text-tertiary)] active:text-[var(--text-primary)]"
            }`}
          >
            <Icon className="w-4 h-4 shrink-0" />
            <span className="text-[10px] leading-tight whitespace-nowrap truncate max-w-full">
              {item.label}
            </span>
          </Link>
        );
      })}

      {/* Nút tìm kiếm toàn cục trên Mobile */}
      <button
        type="button"
        data-testid="mobile-search-btn"
        onClick={() => setCommandPaletteOpen(true)}
        aria-label="Tìm kiếm nhanh (Ctrl+K)"
        className="flex flex-col items-center justify-center gap-0.5 flex-1 min-w-0 min-h-[44px] h-full px-0.5 text-[var(--text-tertiary)] active:text-[var(--text-primary)] transition cursor-pointer"
      >
        <Search className="w-4 h-4 shrink-0" />
        <span className="text-[10px] leading-tight whitespace-nowrap truncate max-w-full">
          Tìm kiếm
        </span>
      </button>

      {MOBILE_NAV_ITEMS.slice(3).map((item) => {
        const Icon = item.icon;
        const active = isActive(item);
        return (
          <Link
            key={item.href}
            href={item.href}
            data-testid={`mobile-nav-${item.href.split("/").pop()}`}
            className={`flex flex-col items-center justify-center gap-0.5 flex-1 min-w-0 min-h-[44px] h-full px-0.5 transition ${
              active
                ? "text-[var(--accent)] font-semibold"
                : "text-[var(--text-tertiary)] active:text-[var(--text-primary)]"
            }`}
          >
            <Icon className="w-4 h-4 shrink-0" />
            <span className="text-[10px] leading-tight whitespace-nowrap truncate max-w-full">
              {item.label}
            </span>
          </Link>
        );
      })}
    </nav>
  );
}
