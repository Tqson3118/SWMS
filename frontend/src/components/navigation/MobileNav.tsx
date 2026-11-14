"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  FolderOpen,
  Brain,
  GraduationCap,
  Settings,
} from "lucide-react";

interface MobileNavItem {
  href: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  matchPrefix?: boolean;
}

const MOBILE_NAV_ITEMS: MobileNavItem[] = [
  { href: "/app", label: "Tổng quan", icon: LayoutDashboard },
  { href: "/app/library", label: "Thư viện", icon: FolderOpen, matchPrefix: true },
  { href: "/app/review", label: "Ôn tập", icon: Brain },
  { href: "/app/studio", label: "Studio", icon: GraduationCap, matchPrefix: true },
  { href: "/app/settings", label: "Cài đặt", icon: Settings },
];

export function MobileNav() {
  const pathname = usePathname();

  const isActive = (item: MobileNavItem) => {
    if (item.matchPrefix) return pathname.startsWith(item.href);
    return pathname === item.href;
  };

  return (
    <nav
      data-testid="mobile-bottom-nav"
      className="md:hidden fixed bottom-0 left-0 right-0 z-50 h-14 bg-bg-primary/95 backdrop-blur-sm border-t border-border flex items-center justify-around px-1 safe-area-bottom"
    >
      {MOBILE_NAV_ITEMS.map((item) => {
        const Icon = item.icon;
        const active = isActive(item);
        return (
          <Link
            key={item.href}
            href={item.href}
            className={`flex flex-col items-center justify-center gap-0.5 flex-1 h-full transition ${
              active
                ? "text-accent"
                : "text-text-tertiary active:text-text-primary"
            }`}
          >
            <Icon className="w-5 h-5" />
            <span className="text-[10px] font-medium">{item.label}</span>
          </Link>
        );
      })}
    </nav>
  );
}
