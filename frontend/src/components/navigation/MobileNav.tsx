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
      className="md:hidden fixed bottom-0 left-0 right-0 z-50 h-14 bg-[var(--surface-card)]/95 backdrop-blur-md border-t border-[var(--border-calm)] flex items-center justify-around px-1 safe-area-bottom shadow-elevation-2"
    >
      {MOBILE_NAV_ITEMS.slice(0, 2).map((item) => {
        const Icon = item.icon;
        const active = isActive(item);
        return (
          <Link
            key={item.href}
            href={item.href}
            className={`flex flex-col items-center justify-center gap-0.5 flex-1 h-full transition ${
              active
                ? "text-[var(--accent)] font-semibold"
                : "text-[var(--text-tertiary)] active:text-[var(--text-primary)]"
            }`}
          >
            <Icon className="w-5 h-5" />
            <span className="text-[10px]">{item.label}</span>
          </Link>
        );
      })}

      {/* Nút tìm kiếm toàn cục trên Mobile */}
      <button
        type="button"
        data-testid="mobile-search-btn"
        onClick={() => setCommandPaletteOpen(true)}
        aria-label="Tìm kiếm nhanh (Ctrl+K)"
        className="flex flex-col items-center justify-center gap-0.5 flex-1 h-full text-[var(--text-tertiary)] active:text-[var(--text-primary)] transition cursor-pointer"
      >
        <Search className="w-5 h-5" />
        <span className="text-[10px]">Tìm kiếm</span>
      </button>

      {MOBILE_NAV_ITEMS.slice(2).map((item) => {
        const Icon = item.icon;
        const active = isActive(item);
        return (
          <Link
            key={item.href}
            href={item.href}
            className={`flex flex-col items-center justify-center gap-0.5 flex-1 h-full transition ${
              active
                ? "text-[var(--accent)] font-semibold"
                : "text-[var(--text-tertiary)] active:text-[var(--text-primary)]"
            }`}
          >
            <Icon className="w-5 h-5" />
            <span className="text-[10px]">{item.label}</span>
          </Link>
        );
      })}
    </nav>
  );
}
