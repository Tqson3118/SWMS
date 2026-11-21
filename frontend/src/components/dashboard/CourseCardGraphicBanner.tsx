"use client";

import React from "react";
import {
  Atom,
  Brain,
  CheckCircle2,
  Code2,
  Compass,
  Cpu,
  GraduationCap,
  Languages,
  Scale,
  Sparkles,
  TrendingUp,
} from "lucide-react";
import type { CourseDomainId } from "@/lib/course-helpers";

interface CourseCardGraphicBannerProps {
  domainId: CourseDomainId;
  domainLabel: string;
  progressPercent: number;
  levelLabel?: string;
  className?: string;
}

export function CourseCardGraphicBanner({
  domainId,
  domainLabel,
  progressPercent,
  levelLabel,
  className = "",
}: CourseCardGraphicBannerProps) {
  const isCompleted = progressPercent >= 100;
  const isInProgress = progressPercent > 0 && progressPercent < 100;

  // Domain-specific graphical themes
  switch (domainId) {
    case "technology":
      return (
        <div
          className={`relative h-24 w-full overflow-hidden bg-gradient-to-br from-indigo-600 via-indigo-700 to-violet-800 text-white p-3.5 flex flex-col justify-between ${className}`}
        >
          {/* Subtle SVG Grid & Circuit Vector Pattern */}
          <svg
            className="absolute inset-0 w-full h-full opacity-15 pointer-events-none"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 300 100"
            preserveAspectRatio="none"
          >
            <defs>
              <pattern id="tech-grid" width="20" height="20" patternUnits="userSpaceOnUse">
                <path d="M 20 0 L 0 0 0 20" fill="none" stroke="currentColor" strokeWidth="0.75" />
              </pattern>
            </defs>
            <rect width="300" height="100" fill="url(#tech-grid)" />
            <path
              d="M 10 80 Q 80 20 160 50 T 290 20"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeDasharray="4,4"
            />
            <circle cx="160" cy="50" r="4" fill="currentColor" />
            <circle cx="240" cy="35" r="3" fill="currentColor" />
          </svg>

          {/* Banner Meta Row */}
          <div className="relative z-10 flex items-center justify-between gap-2">
            <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/15 backdrop-blur-md text-white border border-white/20">
              <Code2 className="w-3.5 h-3.5 shrink-0 text-indigo-200" />
              <span>{domainLabel}</span>
            </span>

            {isCompleted ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-500/90 text-white shadow-xs">
                <CheckCircle2 className="w-3 h-3" />
                <span>Hoàn thành</span>
              </span>
            ) : isInProgress ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/20 backdrop-blur-md text-white border border-white/25">
                <span className="w-1.5 h-1.5 rounded-full bg-amber-300 animate-pulse" />
                <span>{progressPercent}%</span>
              </span>
            ) : levelLabel ? (
              <span className="px-2 py-0.5 rounded-md text-[10px] font-medium bg-black/20 text-white/85">
                {levelLabel}
              </span>
            ) : null}
          </div>

          {/* Bottom Vector Deco */}
          <div className="relative z-10 flex items-center justify-end">
            <div className="p-2 rounded-xl bg-white/10 backdrop-blur-xs border border-white/15 shadow-inner">
              <Cpu className="w-5 h-5 text-indigo-100" />
            </div>
          </div>
        </div>
      );

    case "language":
      return (
        <div
          className={`relative h-24 w-full overflow-hidden bg-gradient-to-br from-sky-600 via-sky-700 to-teal-800 text-white p-3.5 flex flex-col justify-between ${className}`}
        >
          {/* Subtle SVG Soundwave / Global Vector Pattern */}
          <svg
            className="absolute inset-0 w-full h-full opacity-15 pointer-events-none"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 300 100"
            preserveAspectRatio="none"
          >
            <circle cx="260" cy="50" r="45" fill="none" stroke="currentColor" strokeWidth="1" />
            <circle cx="260" cy="50" r="30" fill="none" stroke="currentColor" strokeWidth="1" />
            <circle cx="260" cy="50" r="15" fill="none" stroke="currentColor" strokeWidth="1" />
            <path
              d="M 20 50 Q 50 10 90 50 T 170 50 T 240 50"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            />
          </svg>

          {/* Banner Meta Row */}
          <div className="relative z-10 flex items-center justify-between gap-2">
            <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/15 backdrop-blur-md text-white border border-white/20">
              <Languages className="w-3.5 h-3.5 shrink-0 text-sky-200" />
              <span>{domainLabel}</span>
            </span>

            {isCompleted ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-500/90 text-white shadow-xs">
                <CheckCircle2 className="w-3 h-3" />
                <span>Hoàn thành</span>
              </span>
            ) : isInProgress ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/20 backdrop-blur-md text-white border border-white/25">
                <span className="w-1.5 h-1.5 rounded-full bg-amber-300 animate-pulse" />
                <span>{progressPercent}%</span>
              </span>
            ) : levelLabel ? (
              <span className="px-2 py-0.5 rounded-md text-[10px] font-medium bg-black/20 text-white/85">
                {levelLabel}
              </span>
            ) : null}
          </div>

          {/* Bottom Vector Deco */}
          <div className="relative z-10 flex items-center justify-end">
            <div className="p-2 rounded-xl bg-white/10 backdrop-blur-xs border border-white/15 shadow-inner">
              <Languages className="w-5 h-5 text-sky-100" />
            </div>
          </div>
        </div>
      );

    case "business":
      return (
        <div
          className={`relative h-24 w-full overflow-hidden bg-gradient-to-br from-emerald-600 via-teal-700 to-green-800 text-white p-3.5 flex flex-col justify-between ${className}`}
        >
          {/* Subtle SVG Graph / Growth Vector Pattern */}
          <svg
            className="absolute inset-0 w-full h-full opacity-15 pointer-events-none"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 300 100"
            preserveAspectRatio="none"
          >
            <polyline
              points="10,80 60,65 110,70 170,40 230,45 285,15"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
            />
            <line x1="10" y1="90" x2="290" y2="90" stroke="currentColor" strokeWidth="1" />
            <rect x="50" y="65" width="12" height="25" fill="currentColor" opacity="0.3" />
            <rect x="100" y="70" width="12" height="20" fill="currentColor" opacity="0.3" />
            <rect x="160" y="40" width="12" height="50" fill="currentColor" opacity="0.3" />
            <rect x="220" y="45" width="12" height="45" fill="currentColor" opacity="0.3" />
          </svg>

          {/* Banner Meta Row */}
          <div className="relative z-10 flex items-center justify-between gap-2">
            <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/15 backdrop-blur-md text-white border border-white/20">
              <TrendingUp className="w-3.5 h-3.5 shrink-0 text-emerald-200" />
              <span>{domainLabel}</span>
            </span>

            {isCompleted ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-500/90 text-white shadow-xs">
                <CheckCircle2 className="w-3 h-3" />
                <span>Hoàn thành</span>
              </span>
            ) : isInProgress ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/20 backdrop-blur-md text-white border border-white/25">
                <span className="w-1.5 h-1.5 rounded-full bg-amber-300 animate-pulse" />
                <span>{progressPercent}%</span>
              </span>
            ) : levelLabel ? (
              <span className="px-2 py-0.5 rounded-md text-[10px] font-medium bg-black/20 text-white/85">
                {levelLabel}
              </span>
            ) : null}
          </div>

          {/* Bottom Vector Deco */}
          <div className="relative z-10 flex items-center justify-end">
            <div className="p-2 rounded-xl bg-white/10 backdrop-blur-xs border border-white/15 shadow-inner">
              <TrendingUp className="w-5 h-5 text-emerald-100" />
            </div>
          </div>
        </div>
      );

    case "law":
      return (
        <div
          className={`relative h-24 w-full overflow-hidden bg-gradient-to-br from-amber-600 via-orange-700 to-amber-900 text-white p-3.5 flex flex-col justify-between ${className}`}
        >
          {/* Subtle SVG Pillars / Scales Vector Pattern */}
          <svg
            className="absolute inset-0 w-full h-full opacity-15 pointer-events-none"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 300 100"
            preserveAspectRatio="none"
          >
            <line x1="20" y1="20" x2="280" y2="20" stroke="currentColor" strokeWidth="3" />
            <line x1="20" y1="85" x2="280" y2="85" stroke="currentColor" strokeWidth="3" />
            <rect x="50" y="23" width="16" height="60" fill="currentColor" />
            <rect x="110" y="23" width="16" height="60" fill="currentColor" />
            <rect x="170" y="23" width="16" height="60" fill="currentColor" />
            <rect x="230" y="23" width="16" height="60" fill="currentColor" />
          </svg>

          {/* Banner Meta Row */}
          <div className="relative z-10 flex items-center justify-between gap-2">
            <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/15 backdrop-blur-md text-white border border-white/20">
              <Scale className="w-3.5 h-3.5 shrink-0 text-amber-200" />
              <span>{domainLabel}</span>
            </span>

            {isCompleted ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-500/90 text-white shadow-xs">
                <CheckCircle2 className="w-3 h-3" />
                <span>Hoàn thành</span>
              </span>
            ) : isInProgress ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/20 backdrop-blur-md text-white border border-white/25">
                <span className="w-1.5 h-1.5 rounded-full bg-amber-300 animate-pulse" />
                <span>{progressPercent}%</span>
              </span>
            ) : levelLabel ? (
              <span className="px-2 py-0.5 rounded-md text-[10px] font-medium bg-black/20 text-white/85">
                {levelLabel}
              </span>
            ) : null}
          </div>

          {/* Bottom Vector Deco */}
          <div className="relative z-10 flex items-center justify-end">
            <div className="p-2 rounded-xl bg-white/10 backdrop-blur-xs border border-white/15 shadow-inner">
              <Scale className="w-5 h-5 text-amber-100" />
            </div>
          </div>
        </div>
      );

    case "science":
      return (
        <div
          className={`relative h-24 w-full overflow-hidden bg-gradient-to-br from-purple-600 via-fuchsia-700 to-indigo-900 text-white p-3.5 flex flex-col justify-between ${className}`}
        >
          {/* Subtle SVG Orbital Vector Pattern */}
          <svg
            className="absolute inset-0 w-full h-full opacity-15 pointer-events-none"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 300 100"
            preserveAspectRatio="none"
          >
            <ellipse cx="240" cy="50" rx="55" ry="20" fill="none" stroke="currentColor" strokeWidth="1.5" transform="rotate(-25 240 50)" />
            <ellipse cx="240" cy="50" rx="55" ry="20" fill="none" stroke="currentColor" strokeWidth="1.5" transform="rotate(25 240 50)" />
            <ellipse cx="240" cy="50" rx="55" ry="20" fill="none" stroke="currentColor" strokeWidth="1.5" transform="rotate(90 240 50)" />
            <circle cx="240" cy="50" r="5" fill="currentColor" />
          </svg>

          {/* Banner Meta Row */}
          <div className="relative z-10 flex items-center justify-between gap-2">
            <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/15 backdrop-blur-md text-white border border-white/20">
              <Atom className="w-3.5 h-3.5 shrink-0 text-purple-200" />
              <span>{domainLabel}</span>
            </span>

            {isCompleted ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-500/90 text-white shadow-xs">
                <CheckCircle2 className="w-3 h-3" />
                <span>Hoàn thành</span>
              </span>
            ) : isInProgress ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/20 backdrop-blur-md text-white border border-white/25">
                <span className="w-1.5 h-1.5 rounded-full bg-amber-300 animate-pulse" />
                <span>{progressPercent}%</span>
              </span>
            ) : levelLabel ? (
              <span className="px-2 py-0.5 rounded-md text-[10px] font-medium bg-black/20 text-white/85">
                {levelLabel}
              </span>
            ) : null}
          </div>

          {/* Bottom Vector Deco */}
          <div className="relative z-10 flex items-center justify-end">
            <div className="p-2 rounded-xl bg-white/10 backdrop-blur-xs border border-white/15 shadow-inner">
              <Atom className="w-5 h-5 text-purple-100" />
            </div>
          </div>
        </div>
      );

    default: {
      const isPsychology = domainLabel.includes("Tâm lý") || domainLabel.includes("Kỹ năng");
      const DomainIcon = isPsychology ? Brain : Compass;
      return (
        <div
          className={`relative h-24 w-full overflow-hidden bg-gradient-to-br from-rose-600 via-pink-700 to-amber-800 text-white p-3.5 flex flex-col justify-between ${className}`}
        >
          {/* Subtle SVG Compass / Mind Vector Pattern */}
          <svg
            className="absolute inset-0 w-full h-full opacity-15 pointer-events-none"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 300 100"
            preserveAspectRatio="none"
          >
            <circle cx="250" cy="50" r="35" fill="none" stroke="currentColor" strokeWidth="1" strokeDasharray="3,3" />
            <polygon points="250,25 257,50 250,75 243,50" fill="currentColor" opacity="0.3" />
            <polygon points="225,50 250,57 275,50 250,43" fill="currentColor" opacity="0.3" />
          </svg>

          {/* Banner Meta Row */}
          <div className="relative z-10 flex items-center justify-between gap-2">
            <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/15 backdrop-blur-md text-white border border-white/20">
              <DomainIcon className="w-3.5 h-3.5 shrink-0 text-rose-200" />
              <span>{domainLabel}</span>
            </span>

            {isCompleted ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-500/90 text-white shadow-xs">
                <CheckCircle2 className="w-3 h-3" />
                <span>Hoàn thành</span>
              </span>
            ) : isInProgress ? (
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-white/20 backdrop-blur-md text-white border border-white/25">
                <span className="w-1.5 h-1.5 rounded-full bg-amber-300 animate-pulse" />
                <span>{progressPercent}%</span>
              </span>
            ) : levelLabel ? (
              <span className="px-2 py-0.5 rounded-md text-[10px] font-medium bg-black/20 text-white/85">
                {levelLabel}
              </span>
            ) : null}
          </div>

          {/* Bottom Vector Deco */}
          <div className="relative z-10 flex items-center justify-end">
            <div className="p-2 rounded-xl bg-white/10 backdrop-blur-xs border border-white/15 shadow-inner">
              <DomainIcon className="w-5 h-5 text-rose-100" />
            </div>
          </div>
        </div>
      );
    }
  }
}
