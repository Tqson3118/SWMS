"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import {
  AlertCircle,
  BookOpen,
  CheckCircle2,
  Eye,
  EyeOff,
  Layers,
  Play,
  Plus,
  RefreshCw,
  Search,
  X,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached } from "@/lib/api-client";
import {
  Button,
  Card,
  Badge,
  ProgressBar,
  EmptyState,
} from "@/components/ui";
import {
  COURSE_DOMAINS,
  classifyCourseDomain,
  deduplicateCourses,
  extractCourseSkillTags,
  getCourseChapterCount,
  getCourseId,
  getCourseLessonCount,
  getCourseProgress,
  getCourseVisualTheme,
  normalizeVi,
  sortCoursesSmartly,
  type CourseDomainId,
  type CourseLike,
} from "@/lib/course-helpers";
import { LearningRoadmapsShowcase } from "@/components/dashboard/LearningRoadmapsShowcase";
import { CourseCardGraphicBanner } from "@/components/dashboard/CourseCardGraphicBanner";
import { Sparkles, Clock, Target, Filter } from "lucide-react";

type StatusFilter = "all" | "in_progress" | "not_started" | "completed";
type LevelFilter = "all" | "foundation" | "advanced";

export default function CoursesCatalogPage() {
  const { activeUserHeaderId } = useWorkspaceStore();
  const [rawCourses, setRawCourses] = useState<CourseLike[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const [searchQuery, setSearchQuery] = useState("");
  const [selectedDomain, setSelectedDomain] = useState<CourseDomainId>("all");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");
  const [levelFilter, setLevelFilter] = useState<LevelFilter>("all");
  const [hideDuplicates, setHideDuplicates] = useState(true);

  const fetchCourses = useCallback(async () => {
    setIsLoading(true);
    setFetchError(null);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );

      const resp = await fetch("/api/v1/courses", { headers });
      if (resp.ok) {
        const list = await resp.json();
        setRawCourses(Array.isArray(list) ? list : []);
      } else {
        const data = await fetchDashboardCached({ headers }).catch(() => null);
        if (data && data.courses) {
          setRawCourses(Array.isArray(data.courses) ? data.courses : []);
        } else {
          setFetchError(`Không tải được danh sách khóa học (HTTP ${resp.status}).`);
        }
      }
    } catch {
      setFetchError("Không thể kết nối tới máy chủ.");
    } finally {
      setIsLoading(false);
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    fetchCourses();
  }, [fetchCourses]);

  const deduplicatedList = useMemo(() => sortCoursesSmartly(deduplicateCourses(rawCourses)), [rawCourses]);
  const baseCourses = hideDuplicates ? deduplicatedList : rawCourses;
  const hiddenDuplicatesCount = Math.max(0, rawCourses.length - deduplicatedList.length);

  const searchAndStatusFiltered = useMemo(() => {
    const q = normalizeVi(searchQuery);
    return baseCourses.filter((course) => {
      if (q) {
        const hay = normalizeVi(
          `${course.title || ""} ${course.description || ""} ${course.category || ""}`
        );
        if (!hay.includes(q)) return false;
      }

      const progress = getCourseProgress(course);
      if (statusFilter === "in_progress" && !(progress > 0 && progress < 100)) return false;
      if (statusFilter === "not_started" && progress !== 0) return false;
      if (statusFilter === "completed" && progress !== 100) return false;

      if (levelFilter !== "all") {
        const theme = getCourseVisualTheme(course);
        if (levelFilter === "foundation" && !theme.levelLabel.includes("Nền tảng")) return false;
        if (levelFilter === "advanced" && !theme.levelLabel.includes("Chuyên sâu")) return false;
      }

      return true;
    });
  }, [baseCourses, searchQuery, statusFilter, levelFilter]);

  const domainCounts = useMemo(() => {
    const counts: Record<CourseDomainId, number> = {
      all: searchAndStatusFiltered.length,
      technology: 0,
      law: 0,
      science: 0,
      language: 0,
      business: 0,
      other: 0,
    };
    for (const course of searchAndStatusFiltered) {
      const domain = classifyCourseDomain(course);
      counts[domain.id] += 1;
    }
    return counts;
  }, [searchAndStatusFiltered]);

  const visibleCourses = useMemo(() => {
    if (selectedDomain === "all") return searchAndStatusFiltered;
    return searchAndStatusFiltered.filter(
      (course) => classifyCourseDomain(course).id === selectedDomain
    );
  }, [searchAndStatusFiltered, selectedDomain]);

  if (isLoading) {
    return (
      <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh] space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div className="h-10 w-48 rounded-lg bg-[var(--surface-elevated)] animate-pulse" />
          <div className="h-9 w-36 rounded-lg bg-[var(--surface-elevated)] animate-pulse" />
        </div>
        <div className="h-11 w-full rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
        <div className="flex flex-wrap gap-2">
          {[0, 1, 2, 3, 4].map((i) => (
            <div key={i} className="h-9 w-24 rounded-lg bg-[var(--surface-elevated)] animate-pulse" />
          ))}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {[0, 1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="h-56 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh]">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-text-primary">
            Khóa học
          </h1>
          <p className="text-sm text-text-secondary mt-1">
            {hideDuplicates && hiddenDuplicatesCount > 0
              ? `Hiển thị ${baseCourses.length} khóa học tiêu biểu (đã ẩn ${hiddenDuplicatesCount} bản sao trùng lặp).`
              : `Tổng cộng ${baseCourses.length} khóa học trong thư viện của bạn.`}
          </p>
        </div>

        <Link href="/app/courses/new">
          <Button
            variant="primary"
            leftIcon={<Plus className="w-4 h-4" />}
            aria-label="Tạo khóa học mới"
          >
            <span>Tạo khóa học</span>
          </Button>
        </Link>
      </div>

      {fetchError && (
        <div className="mb-6 rounded-[10px] border border-red-500/30 bg-red-500/5 px-4 py-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 text-sm text-red-600 dark:text-red-400">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{fetchError}</span>
          </div>
          <Button
            variant="secondary"
            size="sm"
            onClick={fetchCourses}
            leftIcon={<RefreshCw className="w-3.5 h-3.5" />}
          >
            Thử lại
          </Button>
        </div>
      )}

      {/* Search & Controls */}
      <div className="space-y-3 mb-6">
        <div className="flex flex-col sm:flex-row gap-2.5">
          <div className="relative flex-1">
            <Search className="w-4 h-4 text-[var(--text-tertiary)] absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Tìm khóa học theo tên hoặc mô tả..."
              className="w-full h-9 pl-9 pr-8 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8] transition"
            />
            {searchQuery && (
              <button
                type="button"
                onClick={() => setSearchQuery("")}
                className="absolute right-2.5 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer"
                aria-label="Xóa từ khóa"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            )}
          </div>

          {hiddenDuplicatesCount > 0 && (
            <button
              type="button"
              onClick={() => setHideDuplicates((prev) => !prev)}
              className={`inline-flex items-center justify-center gap-1.5 h-9 px-3 rounded-[10px] border text-xs font-medium transition cursor-pointer shrink-0 ${
                hideDuplicates
                  ? "border-[#5E51E8]/40 bg-[#EEF0FD] dark:bg-[#1E1B4B] text-[#4F46E5] dark:text-[#A5B4FC]"
                  : "border-[var(--border-calm)] bg-[var(--surface-elevated)] text-[var(--text-secondary)] hover:text-[var(--text-primary)]"
              }`}
            >
              {hideDuplicates ? (
                <EyeOff className="w-3.5 h-3.5" />
              ) : (
                <Eye className="w-3.5 h-3.5" />
              )}
              <span>
                {hideDuplicates ? "Đang ẩn bản sao trùng lặp" : `Hiện cả bản sao (${rawCourses.length})`}
              </span>
            </button>
          )}
        </div>

        {/* Status & Level Filter Pills */}
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="flex flex-wrap items-center gap-1.5">
            <span className="text-xs font-semibold text-[var(--text-tertiary)] mr-1">Trạng thái:</span>
            {(
              [
                { id: "all", label: "Tất cả trạng thái" },
                { id: "in_progress", label: "Đang học" },
                { id: "not_started", label: "Chưa học" },
                { id: "completed", label: "Hoàn thành" },
              ] as { id: StatusFilter; label: string }[]
            ).map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => setStatusFilter(item.id)}
                className={`h-8 min-h-[32px] px-3 rounded-[8px] text-xs font-medium transition cursor-pointer ${
                  statusFilter === item.id
                    ? "bg-[#5E51E8] text-white dark:bg-[#818CF8] dark:text-[#0F0F12]"
                    : "bg-[var(--surface-elevated)] text-[var(--text-secondary)] hover:text-[var(--text-primary)] border border-[var(--border-calm)]"
                }`}
              >
                {item.label}
              </button>
            ))}
          </div>

          <div className="flex flex-wrap items-center gap-1.5">
            <span className="text-xs font-semibold text-[var(--text-tertiary)] mr-1">Trình độ:</span>
            {(
              [
                { id: "all", label: "Tất cả" },
                { id: "foundation", label: "Nền tảng" },
                { id: "advanced", label: "Chuyên sâu" },
              ] as { id: LevelFilter; label: string }[]
            ).map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => setLevelFilter(item.id)}
                className={`h-8 min-h-[32px] px-3 rounded-[8px] text-xs font-medium transition cursor-pointer ${
                  levelFilter === item.id
                    ? "bg-[#5E51E8] text-white dark:bg-[#818CF8] dark:text-[#0F0F12]"
                    : "bg-[var(--surface-elevated)] text-[var(--text-secondary)] hover:text-[var(--text-primary)] border border-[var(--border-calm)]"
                }`}
              >
                {item.label}
              </button>
            ))}
          </div>
        </div>

        {/* Domain Filter Tabs */}
        <div className="flex flex-wrap items-center gap-2 pb-1">
          <button
            type="button"
            onClick={() => setSelectedDomain("all")}
            className={`inline-flex items-center gap-1.5 h-9 min-h-[36px] px-3 rounded-[8px] text-xs font-medium whitespace-nowrap transition cursor-pointer ${
              selectedDomain === "all"
                ? "bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:text-[#A5B4FC] border border-[#5E51E8]/30 font-semibold"
                : "bg-[var(--surface-card)] text-[var(--text-secondary)] border border-[var(--border-calm)] hover:bg-[var(--surface-elevated)]"
            }`}
          >
            <span>Tất cả lĩnh vực</span>
            <span className="opacity-75">({domainCounts.all})</span>
          </button>

          {COURSE_DOMAINS.map((domain) => {
            const count = domainCounts[domain.id];
            if (count === 0 && selectedDomain !== domain.id) return null;
            const displayLabel = domain.id === "other" ? "Tâm lý & Kỹ năng sống" : domain.label;
            return (
              <button
                key={domain.id}
                type="button"
                onClick={() => setSelectedDomain(domain.id)}
                className={`inline-flex items-center gap-1.5 h-9 min-h-[36px] px-3 rounded-[8px] text-xs font-medium whitespace-nowrap transition cursor-pointer ${
                  selectedDomain === domain.id
                    ? "bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:text-[#A5B4FC] border border-[#5E51E8]/30 font-semibold"
                    : "bg-[var(--surface-card)] text-[var(--text-secondary)] border border-[var(--border-calm)] hover:bg-[var(--surface-elevated)]"
                }`}
              >
                <span>{displayLabel}</span>
                <span className="opacity-75">({count})</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* Course Grid */}
      {visibleCourses.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 gap-6">
          {visibleCourses.map((course) => {
            const id = getCourseId(course);
            const progress = getCourseProgress(course);
            const lessonCount = getCourseLessonCount(course);
            const chapterCount = getCourseChapterCount(course);
            const theme = getCourseVisualTheme(course);
            const skillTags = extractCourseSkillTags(course);

            const primaryCtaLabel =
              progress === 100 ? "Ôn lại bài" : progress > 0 ? "Tiếp tục học" : "Bắt đầu học";

            return (
              <Card
                key={id}
                hoverable
                className="overflow-hidden flex flex-col justify-between p-0 shadow-xs hover:shadow-md transition-all duration-200"
              >
                {/* Visual Thematic Header Banner */}
                <CourseCardGraphicBanner
                  domainId={theme.domainId}
                  domainLabel={theme.domainLabel}
                  progressPercent={progress}
                  levelLabel={theme.levelLabel}
                />

                <div className="p-5 flex-1 flex flex-col justify-between gap-4">
                  <div className="min-w-0">
                    <Link
                      href={`/app/courses/${id}`}
                      title={course.title || "Khóa học"}
                      className="block text-base font-bold text-[var(--text-primary)] hover:text-[#5E51E8] dark:hover:text-[#818CF8] transition line-clamp-2 leading-snug mb-1.5 break-words min-w-0"
                    >
                      {course.title || "Khóa học"}
                    </Link>

                    <p
                      title={course.description || ""}
                      className="text-xs text-[var(--text-secondary)] line-clamp-2 leading-relaxed mb-3 break-words"
                    >
                      {course.description ||
                        "Lộ trình học theo chương, bài giảng tương tác, phòng thực hành và thẻ nhớ FSRS."}
                    </p>

                    <div className="flex flex-wrap gap-1.5">
                      {skillTags.slice(0, 3).map((tag) => (
                        <span
                          key={tag}
                          className="inline-flex items-center px-2 py-0.5 rounded-md bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-[10px] font-medium text-[var(--text-secondary)] truncate max-w-[180px]"
                          title={tag}
                        >
                          {tag}
                        </span>
                      ))}
                    </div>
                  </div>

                  <div className="pt-3 border-t border-[var(--border-calm)] space-y-3">
                    <div className="flex flex-wrap items-center justify-between gap-2 text-xs text-[var(--text-tertiary)]">
                      <div className="flex items-center gap-1.5 flex-wrap">
                        {chapterCount > 0 && (
                          <>
                            <span className="inline-flex items-center gap-1 font-medium">
                              <Layers className="w-3.5 h-3.5" />
                              {chapterCount} chương
                            </span>
                            <span>·</span>
                          </>
                        )}
                        <span className="inline-flex items-center gap-1 font-medium">
                          <BookOpen className="w-3.5 h-3.5" />
                          {lessonCount} bài
                        </span>
                        <span>·</span>
                        <span className="inline-flex items-center gap-1 font-medium">
                          <Clock className="w-3.5 h-3.5" />
                          {theme.estimatedHoursLabel}
                        </span>
                      </div>
                      <span className="inline-flex items-center gap-1 text-[11px] font-bold text-[#5E51E8] dark:text-[#A5B4FC]">
                        <Sparkles className="w-3 h-3" />
                        Lab &amp; Quiz
                      </span>
                    </div>

                    <div className="space-y-1">
                      <div className="flex items-center justify-between text-xs font-semibold text-[var(--text-secondary)]">
                        <span>Tiến độ</span>
                        <span className="text-[var(--text-primary)] font-bold">{progress}%</span>
                      </div>
                      <ProgressBar
                        value={progress}
                        color={progress === 100 ? "success" : "brand"}
                        size="sm"
                      />
                    </div>

                    <div className="flex items-center gap-2 pt-1">
                      <Link href={`/app/courses/${id}`} className="flex-1">
                        <Button variant="secondary" size="sm" className="w-full">
                          Xem lộ trình
                        </Button>
                      </Link>
                      <Link href={`/app/courses/${id}/learn`} className="flex-1">
                        <Button
                          variant="primary"
                          size="sm"
                          className="w-full"
                          leftIcon={<Play className="w-3 h-3 fill-current" />}
                        >
                          {primaryCtaLabel}
                        </Button>
                      </Link>
                    </div>
                  </div>
                </div>
              </Card>
            );
          })}
        </div>
      ) : baseCourses.length === 0 ? (
        <EmptyState
          icon={BookOpen}
          title="Chào mừng bạn đến với Studxy!"
          description="Bạn chưa có khóa học nào trong danh sách. Hãy tạo khóa học đầu tiên để bắt đầu học tập và ghi nhớ có hệ thống."
          action={{
            label: "Tạo khóa học đầu tiên",
            href: "/app/courses/new",
            icon: <Plus className="w-4 h-4" />,
          }}
        />
      ) : (
        <EmptyState
          icon={BookOpen}
          title="Không tìm thấy khóa học phù hợp"
          description="Thử đổi từ khóa tìm kiếm, chọn lại lĩnh vực hoặc tạo khóa học mới."
          action={
            searchQuery || selectedDomain !== "all" || statusFilter !== "all"
              ? {
                  label: "Xóa bộ lọc",
                  onClick: () => {
                    setSearchQuery("");
                    setSelectedDomain("all");
                    setStatusFilter("all");
                  },
                }
              : {
                  label: "Tạo khóa học mới",
                  href: "/app/courses/new",
                  icon: <Plus className="w-4 h-4" />,
                }
          }
        />
      )}

      {/* Roadmaps & Use Cases Explorer */}
      <div className="mt-10">
        <LearningRoadmapsShowcase courses={baseCourses} compact={true} />
      </div>
    </div>
  );
}
