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
import {
  COURSE_DOMAINS,
  classifyCourseDomain,
  deduplicateCourses,
  getCourseChapterCount,
  getCourseId,
  getCourseLessonCount,
  getCourseProgress,
  normalizeVi,
  type CourseDomainId,
  type CourseLike,
} from "@/lib/course-helpers";

type StatusFilter = "all" | "in_progress" | "not_started" | "completed";

export default function CoursesCatalogPage() {
  const { activeUserHeaderId } = useWorkspaceStore();
  const [rawCourses, setRawCourses] = useState<CourseLike[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const [searchQuery, setSearchQuery] = useState("");
  const [selectedDomain, setSelectedDomain] = useState<CourseDomainId>("all");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");
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
        const dashResp = await fetch("/api/v1/workspace/dashboard", { headers }).catch(() => null);
        if (dashResp && dashResp.ok) {
          const data = await dashResp.json();
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

  const deduplicatedList = useMemo(() => deduplicateCourses(rawCourses), [rawCourses]);
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
      if (statusFilter === "in_progress") return progress > 0 && progress < 100;
      if (statusFilter === "not_started") return progress === 0;
      if (statusFilter === "completed") return progress === 100;
      return true;
    });
  }, [baseCourses, searchQuery, statusFilter]);

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
      <div className="flex items-center justify-center h-full">
        <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pb-24 md:pb-10">
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

        <Link
          href="/app/courses/new"
          className="inline-flex items-center justify-center gap-1.5 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition shrink-0"
        >
          <Plus className="w-4 h-4" />
          <span>Tạo khóa học</span>
        </Link>
      </div>

      {fetchError && (
        <div className="mb-6 rounded-lg border border-red-500/30 bg-red-500/5 px-4 py-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 text-sm text-red-600 dark:text-red-400">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{fetchError}</span>
          </div>
          <button
            type="button"
            onClick={fetchCourses}
            className="inline-flex items-center gap-1.5 text-xs font-medium px-2.5 py-1.5 rounded-md border border-border hover:bg-bg-secondary text-text-primary cursor-pointer"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            Thử lại
          </button>
        </div>
      )}

      {/* Search & Controls */}
      <div className="space-y-3 mb-6">
        <div className="flex flex-col sm:flex-row gap-2.5">
          <div className="relative flex-1">
            <Search className="w-4 h-4 text-text-tertiary absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Tìm khóa học theo tên hoặc mô tả..."
              className="w-full h-9 pl-9 pr-8 rounded-md border border-border bg-bg-primary text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-accent transition"
            />
            {searchQuery && (
              <button
                type="button"
                onClick={() => setSearchQuery("")}
                className="absolute right-2.5 top-1/2 -translate-y-1/2 text-text-tertiary hover:text-text-primary cursor-pointer"
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
              className={`inline-flex items-center justify-center gap-1.5 h-9 px-3 rounded-md border text-xs font-medium transition cursor-pointer shrink-0 ${
                hideDuplicates
                  ? "border-accent/40 bg-accent-subtle text-accent"
                  : "border-border bg-bg-secondary text-text-secondary hover:text-text-primary"
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

        {/* Status Filter Pills */}
        <div className="flex flex-wrap items-center gap-1.5">
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
              className={`h-7 px-2.5 rounded-md text-xs font-medium transition cursor-pointer ${
                statusFilter === item.id
                  ? "bg-text-primary text-bg-primary"
                  : "bg-bg-secondary text-text-secondary hover:text-text-primary border border-border"
              }`}
            >
              {item.label}
            </button>
          ))}
        </div>

        {/* Domain Filter Tabs */}
        <div className="flex items-center gap-1.5 overflow-x-auto pb-1 swms-thin-scrollbar">
          <button
            type="button"
            onClick={() => setSelectedDomain("all")}
            className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-md text-xs font-medium whitespace-nowrap transition cursor-pointer ${
              selectedDomain === "all"
                ? "bg-accent-subtle text-accent border border-accent/30"
                : "bg-bg-primary text-text-secondary border border-border hover:bg-bg-secondary"
            }`}
          >
            <span>Tất cả lĩnh vực</span>
            <span className="opacity-75">({domainCounts.all})</span>
          </button>

          {COURSE_DOMAINS.map((domain) => {
            const count = domainCounts[domain.id];
            if (count === 0 && selectedDomain !== domain.id) return null;
            return (
              <button
                key={domain.id}
                type="button"
                onClick={() => setSelectedDomain(domain.id)}
                className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-md text-xs font-medium whitespace-nowrap transition cursor-pointer ${
                  selectedDomain === domain.id
                    ? "bg-accent-subtle text-accent border border-accent/30"
                    : "bg-bg-primary text-text-secondary border border-border hover:bg-bg-secondary"
                }`}
              >
                <span>{domain.label}</span>
                <span className="opacity-75">({count})</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* Course Grid */}
      {visibleCourses.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {visibleCourses.map((course) => {
            const id = getCourseId(course);
            const progress = getCourseProgress(course);
            const lessonCount = getCourseLessonCount(course);
            const chapterCount = getCourseChapterCount(course);
            const domain = classifyCourseDomain(course);

            return (
              <div
                key={id}
                className="rounded-lg border border-border bg-bg-primary p-4 flex flex-col justify-between hover:border-accent/50 transition"
              >
                <div>
                  <div className="flex items-center justify-between gap-2 mb-2.5">
                    <span className="text-[11px] font-medium px-2 py-0.5 rounded-md bg-bg-tertiary text-text-secondary truncate">
                      {domain.label}
                    </span>
                    {progress === 100 ? (
                      <span className="inline-flex items-center gap-1 text-[11px] font-medium text-emerald-600 dark:text-emerald-400 shrink-0">
                        <CheckCircle2 className="w-3.5 h-3.5" />
                        Hoàn thành
                      </span>
                    ) : progress > 0 ? (
                      <span className="text-[11px] font-medium text-accent shrink-0">
                        {progress}%
                      </span>
                    ) : (
                      <span className="text-[11px] text-text-tertiary shrink-0">Mới</span>
                    )}
                  </div>

                  <Link
                    href={`/app/courses/${id}`}
                    className="block text-sm font-semibold text-text-primary hover:text-accent transition line-clamp-2 mb-1.5"
                  >
                    {course.title || "Khóa học"}
                  </Link>

                  {course.description ? (
                    <p className="text-xs text-text-tertiary line-clamp-2 mb-3">
                      {course.description}
                    </p>
                  ) : (
                    <p className="text-xs text-text-tertiary mb-3">
                      Lộ trình học theo chương và bài giảng tương tác.
                    </p>
                  )}
                </div>

                <div className="pt-2">
                  <div className="flex items-center gap-3 text-xs text-text-tertiary mb-2">
                    {chapterCount > 0 && (
                      <span className="inline-flex items-center gap-1">
                        <Layers className="w-3.5 h-3.5" />
                        {chapterCount} chương
                      </span>
                    )}
                    <span className="inline-flex items-center gap-1">
                      <BookOpen className="w-3.5 h-3.5" />
                      {lessonCount} bài
                    </span>
                  </div>

                  <div className="w-full h-1.5 bg-bg-tertiary rounded-full overflow-hidden mb-3.5">
                    <div
                      className={`h-full rounded-full transition-all ${
                        progress === 100 ? "bg-emerald-500" : "bg-accent"
                      }`}
                      style={{ width: `${progress}%` }}
                    />
                  </div>

                  <div className="flex items-center gap-2">
                    <Link
                      href={`/app/courses/${id}`}
                      className="flex-1 inline-flex items-center justify-center h-8 px-3 rounded-md border border-border text-xs font-medium text-text-primary hover:bg-bg-secondary transition"
                    >
                      Xem lộ trình
                    </Link>
                    <Link
                      href={`/app/courses/${id}/learn`}
                      className="flex-1 inline-flex items-center justify-center gap-1.5 h-8 px-3 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover transition"
                    >
                      <Play className="w-3 h-3" />
                      <span>{progress > 0 ? "Tiếp tục" : "Học ngay"}</span>
                    </Link>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      ) : (
        <div className="rounded-lg border border-border bg-bg-secondary p-10 text-center">
          <BookOpen className="w-8 h-8 text-text-tertiary mx-auto mb-3" />
          <p className="text-sm font-medium text-text-primary mb-1">
            Không tìm thấy khóa học phù hợp
          </p>
          <p className="text-xs text-text-tertiary mb-4">
            Thử đổi từ khóa tìm kiếm, chọn lại lĩnh vực hoặc tạo khóa học mới.
          </p>
          {(searchQuery || selectedDomain !== "all" || statusFilter !== "all") && (
            <button
              type="button"
              onClick={() => {
                setSearchQuery("");
                setSelectedDomain("all");
                setStatusFilter("all");
              }}
              className="inline-flex items-center h-8 px-3 rounded-md border border-border bg-bg-primary text-xs font-medium text-text-primary hover:bg-bg-tertiary transition cursor-pointer"
            >
              Xóa bộ lọc
            </button>
          )}
        </div>
      )}
    </div>
  );
}
