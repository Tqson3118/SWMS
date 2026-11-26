"use client";

import React from "react";
import {
  Atom,
  BookOpen,
  Brain,
  CheckCircle2,
  Code2,
  Compass,
  Cpu,
  Headphones,
  Languages,
  Layers,
  Scale,
  Sparkles,
  TrendingUp,
  Wallet,
} from "lucide-react";
import { normalizeVi, type CourseDomainId } from "@/lib/course-helpers";

interface CourseCardGraphicBannerProps {
  domainId: CourseDomainId;
  domainLabel: string;
  progressPercent: number;
  levelLabel?: string;
  courseTitle?: string;
  courseSlug?: string;
  className?: string;
}

interface BannerThemeConfig {
  gradientClass: string;
  Icon: React.ComponentType<{ className?: string }>;
  iconTintClass: string;
  accentBadge?: string;
  heroGlyph?: string;
  patternType:
    | "ielts"
    | "japanese"
    | "chinese"
    | "english"
    | "dotnet"
    | "js"
    | "tech"
    | "candlestick"
    | "finance"
    | "law"
    | "science"
    | "psychology"
    | "lifeskills";
}

function resolveCourseBannerTheme(
  domainId: CourseDomainId,
  domainLabel: string,
  courseTitle?: string,
  courseSlug?: string
): BannerThemeConfig {
  const norm = normalizeVi(`${courseTitle || ""} ${courseSlug || ""} ${domainLabel || ""}`);

  if (/\bielts\b/.test(norm)) {
    return {
      gradientClass: "from-blue-600 via-sky-700 to-cyan-800",
      Icon: Headphones,
      iconTintClass: "text-sky-100",
      accentBadge: "IELTS 4 Kỹ năng",
      patternType: "ielts",
    };
  }

  if (/(tieng nhat|japanese|jlpt|\bn5\b|hiragana|katakana)/.test(norm)) {
    return {
      gradientClass: "from-rose-600 via-pink-600 to-red-800",
      Icon: Languages,
      iconTintClass: "text-rose-100",
      accentBadge: "JLPT N5",
      heroGlyph: "あ",
      patternType: "japanese",
    };
  }

  if (/(tieng trung|chinese|\bhsk\d?\b|pinyin|hanzi)/.test(norm)) {
    return {
      gradientClass: "from-red-600 via-amber-600 to-orange-800",
      Icon: Languages,
      iconTintClass: "text-amber-100",
      accentBadge: "HSK 1",
      heroGlyph: "汉",
      patternType: "chinese",
    };
  }

  if (/(tieng anh|english|\ba2\b|\bb1\b|giao tiep tieng anh)/.test(norm)) {
    return {
      gradientClass: "from-teal-600 via-cyan-700 to-emerald-800",
      Icon: BookOpen,
      iconTintClass: "text-teal-100",
      accentBadge: "A2 → B1",
      patternType: "english",
    };
  }

  if (/(\.net|dotnet|c#|csharp|react|fullstack|full-stack)/.test(norm)) {
    return {
      gradientClass: "from-indigo-600 via-violet-700 to-purple-900",
      Icon: Layers,
      iconTintClass: "text-indigo-100",
      accentBadge: ".NET · React",
      patternType: "dotnet",
    };
  }

  if (/(javascript|typescript|\bjs\b|\bts\b)/.test(norm)) {
    return {
      gradientClass: "from-amber-500 via-orange-600 to-indigo-900",
      Icon: Code2,
      iconTintClass: "text-amber-100",
      accentBadge: "JS / TS",
      heroGlyph: "{ }",
      patternType: "js",
    };
  }

  if (/(dau tu|chung khoan|co phieu|dinh gia)/.test(norm)) {
    return {
      gradientClass: "from-emerald-600 via-green-700 to-teal-900",
      Icon: TrendingUp,
      iconTintClass: "text-emerald-100",
      accentBadge: "Biểu đồ & Định giá",
      patternType: "candlestick",
    };
  }

  if (/(tai chinh ca nhan|ngan sach|50\/30\/20|dong tien|tai chinh)/.test(norm)) {
    return {
      gradientClass: "from-teal-600 via-emerald-700 to-cyan-900",
      Icon: Wallet,
      iconTintClass: "text-teal-100",
      accentBadge: "Ngân sách 50/30/20",
      patternType: "finance",
    };
  }

  if (/(tam ly)/.test(norm)) {
    return {
      gradientClass: "from-fuchsia-600 via-purple-700 to-rose-800",
      Icon: Brain,
      iconTintClass: "text-fuchsia-100",
      accentBadge: "Tâm lý học",
      patternType: "psychology",
    };
  }

  if (/(ky nang song|giao tiep)/.test(norm)) {
    return {
      gradientClass: "from-orange-500 via-amber-600 to-rose-700",
      Icon: Sparkles,
      iconTintClass: "text-amber-100",
      accentBadge: "Kỹ năng & Giao tiếp",
      patternType: "lifeskills",
    };
  }

  switch (domainId) {
    case "technology":
      return {
        gradientClass: "from-indigo-600 via-indigo-700 to-violet-800",
        Icon: Cpu,
        iconTintClass: "text-indigo-100",
        patternType: "tech",
      };
    case "language":
      return {
        gradientClass: "from-sky-600 via-sky-700 to-teal-800",
        Icon: Languages,
        iconTintClass: "text-sky-100",
        patternType: "english",
      };
    case "business":
      return {
        gradientClass: "from-emerald-600 via-teal-700 to-green-800",
        Icon: TrendingUp,
        iconTintClass: "text-emerald-100",
        patternType: "candlestick",
      };
    case "law":
      return {
        gradientClass: "from-amber-600 via-orange-700 to-amber-900",
        Icon: Scale,
        iconTintClass: "text-amber-100",
        patternType: "law",
      };
    case "science":
      return {
        gradientClass: "from-purple-600 via-fuchsia-700 to-indigo-900",
        Icon: Atom,
        iconTintClass: "text-purple-100",
        patternType: "science",
      };
    default:
      return {
        gradientClass: "from-rose-600 via-pink-700 to-amber-800",
        Icon: Compass,
        iconTintClass: "text-rose-100",
        patternType: "lifeskills",
      };
  }
}

export function CourseCardGraphicBanner({
  domainId,
  domainLabel,
  progressPercent,
  levelLabel,
  courseTitle,
  courseSlug,
  className = "",
}: CourseCardGraphicBannerProps) {
  const isCompleted = progressPercent >= 100;
  const isInProgress = progressPercent > 0 && progressPercent < 100;
  const config = resolveCourseBannerTheme(domainId, domainLabel, courseTitle, courseSlug);
  const { Icon } = config;

  return (
    <div
      className={`group/banner relative h-24 w-full overflow-hidden bg-gradient-to-br ${config.gradientClass} text-white p-3.5 flex flex-col justify-between shadow-inner ${className}`}
    >
      {/* Subtle Gradient Depth & Contrast Scrim (Light + Dark mode) */}
      <div
        aria-hidden="true"
        className="absolute inset-0 pointer-events-none bg-[radial-gradient(circle_at_top_right,rgba(255,255,255,0.22),transparent_58%)] dark:bg-[radial-gradient(circle_at_top_right,rgba(255,255,255,0.14),transparent_58%)]"
      />
      <div
        aria-hidden="true"
        className="absolute inset-0 pointer-events-none bg-gradient-to-t from-black/30 via-transparent to-black/15 dark:from-black/45 dark:via-black/10 dark:to-black/25"
      />

      {/* Course-specific SVG Pattern */}
      <svg
        aria-hidden="true"
        className="absolute inset-0 w-full h-full opacity-20 group-hover/banner:opacity-30 transition-opacity duration-300 ease-out motion-reduce:transition-none pointer-events-none"
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 300 100"
        preserveAspectRatio="none"
      >
        {config.patternType === "candlestick" && (
          <>
            <line x1="10" y1="88" x2="290" y2="88" stroke="currentColor" strokeWidth="1" />
            <line x1="56" y1="50" x2="56" y2="85" stroke="currentColor" strokeWidth="1.5" />
            <rect x="50" y="58" width="12" height="20" fill="currentColor" rx="2" />
            <line x1="106" y1="35" x2="106" y2="78" stroke="currentColor" strokeWidth="1.5" />
            <rect x="100" y="45" width="12" height="24" fill="currentColor" opacity="0.5" rx="2" />
            <line x1="166" y1="22" x2="166" y2="68" stroke="currentColor" strokeWidth="1.5" />
            <rect x="160" y="30" width="12" height="28" fill="currentColor" rx="2" />
            <line x1="226" y1="12" x2="226" y2="55" stroke="currentColor" strokeWidth="1.5" />
            <rect x="220" y="18" width="12" height="26" fill="currentColor" rx="2" />
            <polyline
              points="15,78 65,62 120,66 175,38 235,25 285,12"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.2"
            />
          </>
        )}

        {config.patternType === "finance" && (
          <>
            <circle cx="245" cy="52" r="36" fill="none" stroke="currentColor" strokeWidth="5" opacity="0.35" />
            <path d="M 245 16 A 36 36 0 0 1 281 52 L 245 52 Z" fill="currentColor" opacity="0.45" />
            <path d="M 20 75 Q 80 35 150 55 T 275 25" fill="none" stroke="currentColor" strokeWidth="2" />
          </>
        )}

        {(config.patternType === "dotnet" || config.patternType === "tech" || config.patternType === "js") && (
          <>
            <path
              d="M 10 80 Q 80 20 160 50 T 290 20"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeDasharray="4,4"
            />
            <circle cx="160" cy="50" r="4" fill="currentColor" />
            <circle cx="235" cy="35" r="3.5" fill="currentColor" />
            <rect x="195" y="18" width="38" height="24" rx="4" fill="none" stroke="currentColor" strokeWidth="1.5" />
            <rect x="215" y="34" width="38" height="24" rx="4" fill="none" stroke="currentColor" strokeWidth="1.5" />
          </>
        )}

        {(config.patternType === "ielts" || config.patternType === "english") && (
          <>
            <circle cx="255" cy="50" r="42" fill="none" stroke="currentColor" strokeWidth="1.2" />
            <circle cx="255" cy="50" r="28" fill="none" stroke="currentColor" strokeWidth="1.2" />
            <path
              d="M 15 55 Q 45 20 75 55 T 135 55 T 195 55 T 255 55"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            />
          </>
        )}

        {(config.patternType === "japanese" || config.patternType === "chinese") && (
          <>
            <circle cx="242" cy="50" r="34" fill="currentColor" opacity="0.28" />
            <circle cx="195" cy="24" r="6" fill="currentColor" opacity="0.35" />
            <circle cx="178" cy="42" r="4" fill="currentColor" opacity="0.25" />
            <path d="M 15 72 Q 95 32 185 62" fill="none" stroke="currentColor" strokeWidth="1.75" />
          </>
        )}

        {config.patternType === "law" && (
          <>
            <line x1="20" y1="20" x2="280" y2="20" stroke="currentColor" strokeWidth="3" />
            <line x1="20" y1="85" x2="280" y2="85" stroke="currentColor" strokeWidth="3" />
            <rect x="55" y="23" width="16" height="60" fill="currentColor" />
            <rect x="115" y="23" width="16" height="60" fill="currentColor" />
            <rect x="175" y="23" width="16" height="60" fill="currentColor" />
            <rect x="235" y="23" width="16" height="60" fill="currentColor" />
          </>
        )}

        {config.patternType === "science" && (
          <>
            <ellipse cx="240" cy="50" rx="55" ry="20" fill="none" stroke="currentColor" strokeWidth="1.5" transform="rotate(-25 240 50)" />
            <ellipse cx="240" cy="50" rx="55" ry="20" fill="none" stroke="currentColor" strokeWidth="1.5" transform="rotate(25 240 50)" />
            <circle cx="240" cy="50" r="5" fill="currentColor" />
          </>
        )}

        {(config.patternType === "psychology" || config.patternType === "lifeskills") && (
          <>
            <circle cx="248" cy="50" r="36" fill="none" stroke="currentColor" strokeWidth="1.5" strokeDasharray="4,3" />
            <circle cx="248" cy="50" r="22" fill="none" stroke="currentColor" strokeWidth="1.2" />
            <path d="M 20 68 Q 95 25 170 52 T 275 36" fill="none" stroke="currentColor" strokeWidth="2" />
          </>
        )}
      </svg>

      {/* Banner Meta Row */}
      <div className="relative z-10 flex items-center justify-between gap-1.5 min-w-0">
        <span
          title={domainLabel}
          className="inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-[11px] sm:text-xs font-bold bg-slate-950/35 dark:bg-slate-950/50 backdrop-blur-md text-white border border-white/30 shadow-xs min-w-0 max-w-[65%]"
        >
          <Icon className={`w-3.5 h-3.5 shrink-0 ${config.iconTintClass}`} />
          <span className="truncate">{domainLabel}</span>
        </span>

        {isCompleted ? (
          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] sm:text-xs font-bold bg-emerald-600/95 dark:bg-emerald-500/95 text-white border border-emerald-300/40 shadow-xs whitespace-nowrap shrink-0">
            <CheckCircle2 className="w-3.5 h-3.5 shrink-0" />
            <span>Hoàn thành</span>
          </span>
        ) : isInProgress ? (
          <span className="inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-[11px] sm:text-xs font-bold bg-slate-950/45 dark:bg-slate-950/65 backdrop-blur-md text-white border border-white/35 shadow-xs whitespace-nowrap shrink-0">
            <span className="w-1.5 h-1.5 rounded-full bg-amber-300 ring-2 ring-amber-300/30 shrink-0" />
            <span>{progressPercent}%</span>
          </span>
        ) : levelLabel ? (
          <span
            title={levelLabel}
            className="px-2 py-0.5 rounded-md text-[11px] font-semibold bg-slate-950/45 dark:bg-slate-950/65 backdrop-blur-md text-white border border-white/30 shadow-xs truncate max-w-[46%]"
          >
            {levelLabel}
          </span>
        ) : null}
      </div>

      {/* Bottom Course Signature Badge & Icon */}
      <div className="relative z-10 flex items-center justify-between gap-2">
        {config.accentBadge ? (
          <span className="inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-bold tracking-wide bg-slate-950/40 dark:bg-slate-950/55 text-white border border-white/25 backdrop-blur-xs shadow-2xs whitespace-nowrap shrink-0">
            {config.accentBadge}
          </span>
        ) : (
          <span />
        )}

        <div className="flex items-center gap-1.5 transition-transform duration-300 ease-out group-hover/banner:scale-105 group-hover/banner:-translate-y-0.5 motion-reduce:transition-none motion-reduce:transform-none">
          {config.heroGlyph ? (
            <div className="w-9 h-9 rounded-xl bg-white/15 dark:bg-white/10 backdrop-blur-sm border border-white/30 shadow-sm flex items-center justify-center text-base font-black text-white select-none">
              {config.heroGlyph}
            </div>
          ) : (
            <div className="p-2 rounded-xl bg-white/15 dark:bg-white/10 backdrop-blur-sm border border-white/25 shadow-sm">
              <Icon className={`w-5 h-5 ${config.iconTintClass}`} />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
