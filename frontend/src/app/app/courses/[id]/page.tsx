"use client";

import React, { Suspense, useCallback, useEffect, useMemo, useRef, useState } from "react";
import Link from "next/link";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import {
  AlertCircle,
  Archive,
  ArrowRight,
  Award,
  BookOpen,
  Brain,
  Check,
  CheckCircle2,
  ClipboardCheck,
  Clock,
  Compass,
  Download,
  Edit3,
  ExternalLink,
  FlaskConical,
  FolderArchive,
  Layers,
  MessageSquare,
  Pin,
  Play,
  RefreshCw,
  Search,
  Settings,
  Sparkles,
  Star,
  Target,
  X,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached, invalidateDashboardCache } from "@/lib/api-client";
import { Button, Card, Badge } from "@/components/ui";
import {
  classifyCourseDomain,
  extractCourseSkillTags,
  getCourseTotalMinutes,
  getCourseVisualTheme,
} from "@/lib/course-helpers";

import type {
  CourseChapter,
  CourseDetail,
  CourseLesson,
  CourseResumeState,
  CourseReviewItem,
  WeakChapterItem,
} from "@/components/courses/types";
import { CourseSyllabusTab } from "@/components/courses/CourseSyllabusTab";
import { CoursePracticeTab } from "@/components/courses/CoursePracticeTab";
import { CourseAssetsTab } from "@/components/courses/CourseAssetsTab";
import { CourseSettingsTab } from "@/components/courses/CourseSettingsTab";
import { CourseInSearchModal } from "@/components/learn/CourseInSearchModal";

type TabKey = "syllabus" | "practice" | "assets" | "settings";
const VALID_TABS: TabKey[] = ["syllabus", "practice", "assets", "settings"];

interface ToastState {
  message: string;
  type: "success" | "error";
}

interface RetiredCourseInfo {
  error: string;
  redirectSlug: string;
  redirectCourseId?: string | null;
}

function CourseDetailPageInner() {
  const params = useParams();
  const searchParams = useSearchParams();
  const router = useRouter();

  const courseId = params.id as string;
  const { activeUserHeaderId } = useWorkspaceStore();

  // Tab State synced with URL query param ?tab= (fallback safely to "syllabus")
  const rawTab = searchParams.get("tab") as TabKey | null;
  const activeTab: TabKey = rawTab && VALID_TABS.includes(rawTab) ? rawTab : "syllabus";
  const tabButtonRefs = useRef<Record<TabKey, HTMLButtonElement | null>>({
    syllabus: null,
    practice: null,
    assets: null,
    settings: null,
  });

  const handleTabChange = (tab: TabKey) => {
    const nextParams = new URLSearchParams(searchParams.toString());
    nextParams.set("tab", tab);
    router.replace(`?${nextParams.toString()}`, { scroll: false });
  };

  const handleTabListKeyDown = (e: React.KeyboardEvent<HTMLElement>) => {
    if (e.key !== "ArrowLeft" && e.key !== "ArrowRight" && e.key !== "Home" && e.key !== "End") {
      return;
    }
    e.preventDefault();
    const currentIndex = VALID_TABS.indexOf(activeTab);
    let nextIndex = currentIndex;
    if (e.key === "ArrowRight") {
      nextIndex = (currentIndex + 1) % VALID_TABS.length;
    } else if (e.key === "ArrowLeft") {
      nextIndex = (currentIndex - 1 + VALID_TABS.length) % VALID_TABS.length;
    } else if (e.key === "Home") {
      nextIndex = 0;
    } else if (e.key === "End") {
      nextIndex = VALID_TABS.length - 1;
    }
    const nextTab = VALID_TABS[nextIndex];
    handleTabChange(nextTab);
    tabButtonRefs.current[nextTab]?.focus();
  };

  const [course, setCourse] = useState<CourseDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [retiredInfo, setRetiredInfo] = useState<RetiredCourseInfo | null>(null);

  const [expandedChapters, setExpandedChapters] = useState<Set<string>>(new Set());
  const [sequentialUnlock, setSequentialUnlock] = useState<boolean>(false);
  const [resumeState, setResumeState] = useState<CourseResumeState | null>(null);

  // Reviews
  const [reviews, setReviews] = useState<CourseReviewItem[]>([]);
  const [averageRating, setAverageRating] = useState<number>(0);
  const [ratingInput, setRatingInput] = useState<number>(5);
  const [commentInput, setCommentInput] = useState<string>("");
  const [submittingReview, setSubmittingReview] = useState<boolean>(false);
  const [reviewNotice, setReviewNotice] = useState<string | null>(null);

  // Study Guide download
  const [downloadingGuide, setDownloadingGuide] = useState(false);

  // Quick In-Course Search Modal
  const [showSearchModal, setShowSearchModal] = useState(false);

  // Quick Pin Toggle
  const [isTogglingPin, setIsTogglingPin] = useState(false);

  // Lightweight Toast
  const [toast, setToast] = useState<ToastState | null>(null);

  useEffect(() => {
    if (!toast) return;
    const timer = window.setTimeout(() => setToast(null), 3500);
    return () => window.clearTimeout(timer);
  }, [toast]);

  // Global keyboard shortcut Ctrl+Shift+F / Cmd+Shift+F for In-Course Search
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key.toLowerCase() === "f") {
        e.preventDefault();
        setShowSearchModal(true);
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, []);

  useEffect(() => {
    if (!courseId || typeof window === "undefined") return;
    const saved = window.localStorage.getItem(`swms_seq_unlock_${courseId}`);
    if (saved === "true") {
      setSequentialUnlock(true);
    } else if (saved === "false") {
      setSequentialUnlock(false);
    }
  }, [courseId]);

  const handleToggleSequentialUnlock = () => {
    setSequentialUnlock((prev) => {
      const next = !prev;
      if (typeof window !== "undefined" && courseId) {
        window.localStorage.setItem(`swms_seq_unlock_${courseId}`, String(next));
      }
      return next;
    });
  };

  const parseReviewsPayload = useCallback((rawReviews: unknown) => {
    if (!rawReviews) return;
    const listRaw: Record<string, unknown>[] = Array.isArray(rawReviews)
      ? (rawReviews as Record<string, unknown>[])
      : Array.isArray((rawReviews as { reviews?: unknown[] }).reviews)
        ? ((rawReviews as { reviews: Record<string, unknown>[] }).reviews)
        : Array.isArray((rawReviews as { items?: unknown[] }).items)
          ? ((rawReviews as { items: Record<string, unknown>[] }).items)
          : [];

    const mapped: CourseReviewItem[] = listRaw.map((item, idx) => ({
      id: String(item.id || item.reviewId || `rev-${idx}`),
      userName: String(
        item.userName ||
          item.authorName ||
          item.fullName ||
          (item.user as { fullName?: string } | undefined)?.fullName ||
          "Học viên Studxy"
      ),
      rating: Math.max(1, Math.min(5, Math.round(Number(item.rating ?? item.stars ?? 5)))),
      comment: String(item.comment ?? item.content ?? item.text ?? ""),
      createdAt: (item.createdAt as string | null | undefined) ?? null,
    }));

    setReviews(mapped);
    const explicitAvg = Number((rawReviews as { averageRating?: unknown }).averageRating ?? 0);
    if (Number.isFinite(explicitAvg) && explicitAvg > 0) {
      setAverageRating(Number(explicitAvg.toFixed(1)));
    } else if (mapped.length > 0) {
      const sum = mapped.reduce((acc, r) => acc + r.rating, 0);
      setAverageRating(Number((sum / mapped.length).toFixed(1)));
    } else {
      setAverageRating(0);
    }
  }, []);

  const fetchCourse = useCallback(async () => {
    if (!courseId) return;
    setIsLoading(true);
    setFetchError(null);
    setRetiredInfo(null);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );

      const courseResp = await fetch(`/api/v1/courses/${courseId}`, {
        headers,
        credentials: "include",
      }).catch(() => null);

      // Check 404 Retired Course First before fetching supplementary resources!
      if (courseResp && courseResp.status === 404) {
        const errorJson = await courseResp.json().catch(() => null);
        if (errorJson?.retired === true) {
          setRetiredInfo({
            error: errorJson.error || "Khóa học đã được gộp vào lộ trình chuẩn mới.",
            redirectSlug: errorJson.redirectSlug || "",
            redirectCourseId: errorJson.redirectCourseId ?? null,
          });
          setIsLoading(false);
          return;
        }
      }

      const [resumeResp, reviewsResp] = await Promise.all([
        fetch(`/api/v1/courses/${courseId}/resume-state`, { headers, credentials: "include" }).catch(() => null),
        fetch(`/api/v1/courses/${courseId}/reviews`, { headers, credentials: "include" }).catch(() => null),
      ]);

      let courseData: CourseDetail | null = null;

      if (courseResp && courseResp.ok) {
        const raw = await courseResp.json();
        const chaps: CourseChapter[] = raw.chapters ?? [];
        const flatLessons = chaps.flatMap((ch) => ch.lessons ?? []);
        const completedIds: string[] = Array.isArray(raw.completedLessonIds)
          ? raw.completedLessonIds
          : [];
        const total = Number(raw.totalLessons ?? raw.lessonCount ?? flatLessons.length);
        const completed = Number(raw.completedLessons ?? completedIds.length);
        const progress =
          raw.progressPercent != null
            ? Number(raw.progressPercent)
            : total > 0
              ? Math.round((completed * 100) / total)
              : 0;

        const apiWeakChapters: WeakChapterItem[] | undefined = Array.isArray(raw.weakChapters)
          ? raw.weakChapters.map((wc: Record<string, unknown>) => ({
              chapterId: String(wc.chapterId || wc.id || ""),
              title: String(wc.title || "Chương cần ôn tập"),
              completionPercent: Number(wc.completionPercent ?? wc.progressPercent ?? 0),
              reason: wc.reason ? String(wc.reason) : undefined,
              nextLessonId: wc.nextLessonId ? String(wc.nextLessonId) : undefined,
            }))
          : undefined;

        courseData = {
          ...raw,
          id: String(raw.courseId || raw.id || courseId),
          courseId: String(raw.courseId || raw.id || courseId),
          title: String(raw.title || "Khóa học"),
          slug: raw.slug ?? undefined,
          description: raw.description ?? "",
          visibility: raw.visibility ?? "public",
          isPublished: Boolean(raw.isPublished ?? raw.visibility === "public"),
          isPinned: Boolean(raw.isPinned),
          isArchived: Boolean(raw.isArchived),
          dailyLessonGoal: typeof raw.dailyLessonGoal === "number" && raw.dailyLessonGoal > 0 ? raw.dailyLessonGoal : 2,
          targetCompletionDate: raw.targetCompletionDate ?? null,
          isSystemCourse: Boolean(raw.isSystemCourse),
          canDelete: Boolean(raw.canDelete),
          category: raw.category ?? "",
          level: raw.level ?? "",
          learningOutcomes: Array.isArray(raw.learningOutcomes) ? raw.learningOutcomes : [],
          prerequisites: Array.isArray(raw.prerequisites) ? raw.prerequisites : [],
          targetAudience: raw.targetAudience ?? "",
          labLabel: raw.labLabel ?? "",
          hasAudio: Boolean(raw.hasAudio),
          bookmarkedLessonCount: Number(raw.bookmarkedLessonCount ?? 0),
          needsReviewLessonCount: Number(raw.needsReviewLessonCount ?? 0),
          totalLessons: total,
          completedLessons: completed,
          completedLessonIds: completedIds,
          progressPercent: progress,
          totalEstimatedMinutes: Number(raw.totalEstimatedMinutes ?? 0),
          flashcardCount: Number(raw.flashcardCount ?? 0),
          dueFlashcardCount: Number(raw.dueFlashcardCount ?? 0),
          masteredFlashcardCount: Number(raw.masteredFlashcardCount ?? 0),
          practiceTotalActivities: Number(raw.practiceTotalActivities ?? 0),
          practicePassedActivities: Number(raw.practicePassedActivities ?? 0),
          practiceAverageScore: Number(raw.practiceAverageScore ?? 0),
          quizCount: Number(raw.quizCount ?? 0),
          quizId: (raw.quizId as string | null | undefined) ?? null,
          firstLessonId: raw.firstLessonId ?? null,
          weakChapters: apiWeakChapters,
          chapters: chaps,
        };
      } else {
        // Fallback to dashboard cache if courseResp had transient error
        const dashData = await fetchDashboardCached({ headers }).catch(() => null);
        if (dashData) {
          const allCourses = dashData.courses ?? [];
          const found = allCourses.find(
            (c: Record<string, unknown>) => (c.courseId || c.id) === courseId
          );
          if (found) {
            const chaps = (found.chapters as CourseChapter[]) ?? [];
            const flatLessons = chaps.flatMap((ch) => ch.lessons ?? []);
            const completedIds = Array.isArray(found.completedLessonIds)
              ? (found.completedLessonIds as string[])
              : [];
            const total = Number(
              found.totalLessons ?? found.lessonCount ?? flatLessons.length
            );
            const completed = Number(found.completedLessons ?? completedIds.length);
            courseData = {
              id: String(found.courseId || found.id),
              courseId: String(found.courseId || found.id),
              title: String(found.title || "Khóa học"),
              description: String(found.description || ""),
              category: String(found.category || ""),
              visibility: String(found.visibility || "personal"),
              isPublished: Boolean(found.isPublished ?? found.visibility === "public"),
              isPinned: Boolean(found.isPinned),
              isArchived: Boolean(found.isArchived),
              dailyLessonGoal: typeof found.dailyLessonGoal === "number" ? found.dailyLessonGoal : 2,
              targetCompletionDate: found.targetCompletionDate ?? null,
              isSystemCourse: Boolean(found.isSystemCourse),
              canDelete: Boolean(found.canDelete),
              learningOutcomes: Array.isArray(found.learningOutcomes) ? found.learningOutcomes : [],
              prerequisites: Array.isArray(found.prerequisites) ? found.prerequisites : [],
              targetAudience: String(found.targetAudience || ""),
              labLabel: String(found.labLabel || ""),
              hasAudio: Boolean(found.hasAudio),
              progressPercent: Number(
                found.progressPercent ??
                  found.progress ??
                  (total > 0 ? Math.round((completed * 100) / total) : 0)
              ),
              totalLessons: total,
              completedLessons: completed,
              completedLessonIds: completedIds,
              flashcardCount: Number(found.flashcardCount ?? 0),
              dueFlashcardCount: Number(found.dueFlashcardCount ?? 0),
              masteredFlashcardCount: Number(found.masteredFlashcardCount ?? 0),
              practiceTotalActivities: Number(found.practiceTotalActivities ?? 0),
              practicePassedActivities: Number(found.practicePassedActivities ?? 0),
              practiceAverageScore: Number(found.practiceAverageScore ?? 0),
              quizCount: Number(found.quizCount ?? 0),
              quizId: (found.quizId as string | null | undefined) ?? null,
              chapters: chaps,
            };
          }
        }
        if (!courseData && courseResp && !courseResp.ok) {
          setFetchError(`Không tải được thông tin khóa học (HTTP ${courseResp.status}).`);
        }
      }

      // Resume State
      if (resumeResp && resumeResp.ok) {
        const rawResume = await resumeResp.json().catch(() => null);
        if (rawResume && typeof rawResume === "object") {
          const secs = Number(
            rawResume.totalTimeSpentSeconds ??
              rawResume.totalStudiedSeconds ??
              rawResume.timeSpentSeconds ??
              rawResume.studiedSeconds ??
              0
          );
          const minsRaw = Number(
            rawResume.totalStudiedMinutes ??
              rawResume.totalTimeSpentMinutes ??
              rawResume.timeSpentMinutes ??
              rawResume.studiedMinutes ??
              0
          );
          const studiedMins =
            secs > 0 ? Math.max(1, Math.round(secs / 60)) : minsRaw > 0 ? Math.round(minsRaw) : 0;

          setResumeState({
            lastLessonId:
              rawResume.lastLessonId ?? rawResume.lessonId ?? rawResume.nextLessonId ?? null,
            lastLessonTitle:
              rawResume.lastLessonTitle ?? rawResume.lessonTitle ?? rawResume.title ?? null,
            readPercent: Math.max(
              0,
              Math.min(
                100,
                Math.round(
                  Number(
                    rawResume.scrollPercent ??
                      rawResume.readPercent ??
                      rawResume.lastScrollPercent ??
                      0
                  )
                )
              )
            ),
            totalStudiedMinutes: studiedMins,
            lastAccessedAt: rawResume.lastAccessedAt ?? rawResume.updatedAt ?? null,
          });
        }
      }

      // Reviews
      if (reviewsResp && reviewsResp.ok) {
        const rawRev = await reviewsResp.json().catch(() => null);
        parseReviewsPayload(rawRev);
      }

      setCourse(courseData);
      if (courseData?.chapters) {
        setExpandedChapters(
          new Set(courseData.chapters.map((ch) => String(ch.chapterId || ch.id)))
        );
      }
    } catch {
      setFetchError("Không thể kết nối tới máy chủ.");
    } finally {
      setIsLoading(false);
    }
  }, [courseId, activeUserHeaderId, parseReviewsPayload]);

  useEffect(() => {
    fetchCourse();
  }, [fetchCourse]);

  // Quick Pin Toggle from Hero
  const handleQuickTogglePin = async () => {
    if (!course || isTogglingPin) return;
    setIsTogglingPin(true);
    try {
      const nextPin = !course.isPinned;
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders({
        "Content-Type": "application/json",
        ...(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : {}),
      });

      const resp = await fetch(`/api/v1/courses/${courseId}/preferences`, {
        method: "PUT",
        headers,
        credentials: "include",
        body: JSON.stringify({ isPinned: nextPin }),
      });
      if (resp.ok) {
        setCourse((prev) => (prev ? { ...prev, isPinned: nextPin } : prev));
        invalidateDashboardCache();
        setToast({
          message: nextPin ? "Đã ghim khóa học lên đầu danh sách." : "Đã bỏ ghim khóa học.",
          type: "success",
        });
      } else {
        setToast({
          message: `Không thể cập nhật trạng thái ghim (HTTP ${resp.status}).`,
          type: "error",
        });
      }
    } catch {
      setToast({
        message: "Lỗi kết nối khi cập nhật trạng thái ghim.",
        type: "error",
      });
    } finally {
      setIsTogglingPin(false);
    }
  };

  // Submit Review Form
  const handleSubmitReview = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!courseId || submittingReview) return;
    const trimmed = commentInput.trim();
    if (!trimmed) {
      setReviewNotice("Vui lòng nhập nhận xét của bạn về khóa học.");
      return;
    }

    setSubmittingReview(true);
    setReviewNotice(null);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders({
        "Content-Type": "application/json",
        ...(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : {}),
      });

      const resp = await fetch(`/api/v1/courses/${courseId}/reviews`, {
        method: "POST",
        headers,
        credentials: "include",
        body: JSON.stringify({
          rating: ratingInput,
          comment: trimmed,
          content: trimmed,
        }),
      });

      if (resp.ok) {
        setCommentInput("");
        setReviewNotice("Đã gửi đánh giá khóa học thành công!");
        const refreshed = await fetch(`/api/v1/courses/${courseId}/reviews`, {
          headers: getClientRequestHeaders(
            effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
          ),
          credentials: "include",
        }).catch(() => null);
        if (refreshed && refreshed.ok) {
          const data = await refreshed.json().catch(() => null);
          parseReviewsPayload(data);
        } else {
          const newItem: CourseReviewItem = {
            id: `local-${Date.now()}`,
            userName: "Bạn",
            rating: ratingInput,
            comment: trimmed,
            createdAt: new Date().toISOString(),
          };
          const nextList = [newItem, ...reviews];
          setReviews(nextList);
          const sum = nextList.reduce((acc, r) => acc + r.rating, 0);
          setAverageRating(Number((sum / nextList.length).toFixed(1)));
        }
      } else {
        const errText = await resp.text().catch(() => "");
        setReviewNotice(`Không thể gửi đánh giá (HTTP ${resp.status})${errText ? `: ${errText}` : ""}`);
      }
    } catch {
      setReviewNotice("Lỗi kết nối khi gửi đánh giá.");
    } finally {
      setSubmittingReview(false);
    }
  };

  // Download Study Guide
  const handleDownloadStudyGuide = async () => {
    if (!courseId || downloadingGuide) return;
    setDownloadingGuide(true);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );
      const resp = await fetch(`/api/v1/courses/${courseId}/study-guide`, {
        headers,
        credentials: "include",
      });
      if (!resp.ok) {
        setToast({
          message: `Không xuất được đề cương (HTTP ${resp.status}).`,
          type: "error",
        });
        return;
      }
      const data = await resp.json();
      const markdown = String(data.markdown || "");
      const fileName = String(data.fileName || "studxy-de-cuong.md");
      const blob = new Blob([markdown], { type: "text/markdown;charset=utf-8" });
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement("a");
      anchor.href = url;
      anchor.download = fileName;
      document.body.appendChild(anchor);
      anchor.click();
      document.body.removeChild(anchor);
      URL.revokeObjectURL(url);
      setToast({
        message: `Đã tải xuống Đề cương (${fileName}) thành công!`,
        type: "success",
      });
    } catch {
      setToast({
        message: "Lỗi kết nối khi tải đề cương ôn tập.",
        type: "error",
      });
    } finally {
      setDownloadingGuide(false);
    }
  };

  const completedSet = useMemo(
    () => new Set(course?.completedLessonIds ?? []),
    [course?.completedLessonIds]
  );

  const isDone = useCallback(
    (lesson: CourseLesson) => Boolean(lesson.isCompleted || completedSet.has(lesson.id)),
    [completedSet]
  );

  const allLessons = useMemo(
    () => (course?.chapters ?? []).flatMap((ch) => ch.lessons ?? []),
    [course?.chapters]
  );

  const nextLesson = useMemo(
    () => allLessons.find((l) => !isDone(l)) || allLessons[0],
    [allLessons, isDone]
  );

  const totalMinutes = useMemo(() => {
    if (!course) return 0;
    const sumLessons = allLessons.reduce(
      (sum, l) => sum + Number(l.durationMinutes ?? l.estimatedMinutes ?? 0),
      0
    );
    if (sumLessons > 0) return sumLessons;
    return getCourseTotalMinutes(course);
  }, [allLessons, course]);

  const studiedMinutesReal = useMemo(() => {
    if (resumeState?.totalStudiedMinutes && resumeState.totalStudiedMinutes > 0) {
      return resumeState.totalStudiedMinutes;
    }
    return allLessons
      .filter((l) => isDone(l))
      .reduce((sum, l) => sum + Number(l.durationMinutes ?? l.estimatedMinutes ?? 15), 0);
  }, [resumeState, allLessons, isDone]);

  const studiedTimeLabel = useMemo(() => {
    if (studiedMinutesReal <= 0) return "0 phút đã học";
    if (studiedMinutesReal < 60) return `${studiedMinutesReal} phút đã học`;
    const hours = (studiedMinutesReal / 60).toFixed(1).replace(/\.0$/, "");
    return `${hours} giờ đã học (${studiedMinutesReal} phút)`;
  }, [studiedMinutesReal]);

  const resumeLesson = useMemo(() => {
    if (resumeState?.lastLessonId) {
      const found = allLessons.find((l) => l.id === resumeState.lastLessonId);
      if (found) return found;
    }
    return nextLesson;
  }, [resumeState, allLessons, nextLesson]);

  const unlockedLessonIds = useMemo(() => {
    const allowed = new Set<string>();
    let foundFirstIncomplete = false;
    for (const lesson of allLessons) {
      if (isDone(lesson)) {
        allowed.add(lesson.id);
      } else if (!foundFirstIncomplete) {
        allowed.add(lesson.id);
        foundFirstIncomplete = true;
      }
    }
    return allowed;
  }, [allLessons, isDone]);

  // Loading Skeleton
  if (isLoading) {
    return (
      <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh] space-y-6">
        <div className="h-6 w-32 rounded bg-[var(--surface-elevated)] animate-pulse" />
        <div className="h-10 w-96 rounded-lg bg-[var(--surface-elevated)] animate-pulse" />
        <div className="h-28 w-full rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 pt-4">
          <div className="lg:col-span-8 space-y-3">
            {[0, 1, 2].map((i) => (
              <div key={i} className="h-14 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
            ))}
          </div>
          <div className="lg:col-span-4 h-64 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
        </div>
      </div>
    );
  }

  // Retired Course Redirect Banner Screen (Requirement 2)
  if (retiredInfo) {
    const destinationHref = retiredInfo.redirectCourseId
      ? `/app/courses/${retiredInfo.redirectCourseId}`
      : "/app/courses";

    return (
      <div className="max-w-2xl mx-auto px-4 py-12 sm:py-16">
        <Card className="relative overflow-hidden bg-[var(--surface-card)] border-amber-500/30 dark:border-amber-400/25 shadow-lg p-0">
          <div className="h-1.5 w-full bg-gradient-to-r from-amber-500 via-[#5E51E8] to-indigo-500" />
          <div className="p-6 sm:p-8 text-center space-y-6">
            <div className="w-16 h-16 rounded-2xl bg-amber-500/10 dark:bg-amber-500/15 border border-amber-500/25 text-amber-600 dark:text-amber-400 flex items-center justify-center mx-auto shadow-sm">
              <Compass className="w-8 h-8" />
            </div>

            <div className="space-y-3">
              <Badge variant="warning" size="md">
                Lộ trình đã được tối ưu hóa
              </Badge>
              <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-[var(--text-primary)]">
                Khóa học đã được nâng cấp &amp; gộp
              </h1>
              <p className="text-sm text-[var(--text-secondary)] leading-relaxed max-w-lg mx-auto">
                {retiredInfo.error}
              </p>
              {retiredInfo.redirectSlug && (
                <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-lg bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-xs text-[var(--text-tertiary)] font-mono">
                  <span>Lộ trình chuẩn mới:</span>
                  <span className="text-[var(--text-primary)] font-semibold">
                    {retiredInfo.redirectSlug}
                  </span>
                </div>
              )}
            </div>

            <div className="flex flex-col sm:flex-row items-center justify-center gap-3 pt-2">
              <Link href={destinationHref} className="w-full sm:w-auto">
                <Button
                  variant="primary"
                  size="lg"
                  className="w-full sm:w-auto bg-[#5E51E8] hover:bg-[#4F46E5] text-white"
                  rightIcon={<ArrowRight className="w-4 h-4" />}
                >
                  <span>Chuyển tới Lộ trình Chuẩn Mới</span>
                </Button>
              </Link>
              <Link href="/app/courses" className="w-full sm:w-auto">
                <Button variant="outline" size="lg" className="w-full sm:w-auto">
                  <span>Về Danh sách khóa học</span>
                </Button>
              </Link>
            </div>
          </div>
        </Card>
      </div>
    );
  }

  // Not Found or Error
  if (!course) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-12 text-center">
        {fetchError && (
          <div className="mb-4 rounded-lg border border-red-500/30 bg-red-500/5 px-4 py-3 text-sm text-red-600 dark:text-red-400 inline-flex items-center gap-2">
            <AlertCircle className="w-4 h-4" />
            <span>{fetchError}</span>
          </div>
        )}
        <p className="text-text-secondary mb-4">Không tìm thấy khóa học.</p>
        <div className="flex items-center justify-center gap-3">
          <button
            type="button"
            onClick={fetchCourse}
            className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-primary hover:bg-bg-secondary cursor-pointer"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            Thử lại
          </button>
          <Link href="/app/courses" className="text-sm text-accent hover:underline">
            ← Về danh sách khóa học
          </Link>
        </div>
      </div>
    );
  }

  const domain = classifyCourseDomain(course);
  const theme = getCourseVisualTheme(course);
  const skillTags = extractCourseSkillTags(course);
  const remainingLessons = Math.max(0, course.totalLessons - course.completedLessons);

  const ringRadius = 32;
  const ringCirc = 2 * Math.PI * ringRadius;
  const ringOffset = ringCirc - (Math.min(100, Math.max(0, course.progressPercent)) / 100) * ringCirc;

  const continueLearnHref = resumeLesson
    ? `/app/courses/${courseId}/learn?lesson=${resumeLesson.id}`
    : `/app/courses/${courseId}/learn`;

  const isSystemCourse = Boolean(course.isSystemCourse);

  const practiceTabBadgeCount =
    (course.practiceTotalActivities ?? 0) > 0
      ? `${course.practicePassedActivities ?? 0}/${course.practiceTotalActivities}`
      : (course.dueFlashcardCount ?? 0) + (course.quizCount ?? 0) > 0
        ? String((course.dueFlashcardCount ?? 0) + (course.quizCount ?? 0))
        : null;

  const assetsTabBadgeCount =
    (course.bookmarkedLessonCount ?? 0) + (course.needsReviewLessonCount ?? 0);

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-32 md:pb-10 space-y-6 overflow-x-hidden">
      {/* Breadcrumb */}
      <nav
        aria-label="Breadcrumb"
        className="flex items-center gap-2 text-sm text-[var(--text-tertiary)] overflow-x-auto whitespace-nowrap"
      >
        <Link href="/app/courses" className="hover:text-[var(--text-primary)] transition">
          Khóa học
        </Link>
        <span>·</span>
        <span className="text-[var(--text-primary)] font-medium truncate">{course.title}</span>
      </nav>

      {/* Hero Summary Card */}
      <Card className="bg-[var(--surface-card)] overflow-hidden p-0">
        <div className={`h-2 w-full bg-gradient-to-r ${theme.accentBarClass}`} />
        <div className="p-5 sm:p-6 space-y-5">
          <div className="flex flex-col md:flex-row md:items-start justify-between gap-5">
            <div className="min-w-0 flex-1">
              {/* Badges strip */}
              <div className="flex flex-wrap items-center gap-2 mb-3">
                <span
                  className={`inline-flex items-center px-2.5 py-0.5 rounded-md border text-xs font-semibold ${theme.badgeBgClass}`}
                >
                  {course.category || theme.domainLabel}
                </span>

                <Badge variant="neutral" size="sm">
                  {course.level || theme.levelLabel}
                </Badge>

                {isSystemCourse ? (
                  <Badge variant="brand" size="sm">
                    Chuẩn hệ thống
                  </Badge>
                ) : (
                  <Badge variant="neutral" size="sm">
                    Khóa cá nhân
                  </Badge>
                )}

                {course.isPinned && (
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-indigo-50 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-300 text-xs font-semibold">
                    <Pin className="w-3 h-3 fill-current" />
                    <span>Đã ghim</span>
                  </span>
                )}

                {course.isArchived && (
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300 text-xs font-semibold">
                    <Archive className="w-3 h-3" />
                    <span>Đã lưu trữ</span>
                  </span>
                )}

                {averageRating > 0 && (
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-amber-50 dark:bg-amber-950/50 border border-amber-200 dark:border-amber-800 text-xs font-bold text-amber-700 dark:text-amber-300">
                    <Star className="w-3.5 h-3.5 fill-amber-400 text-amber-400" />
                    <span>{averageRating}</span>
                    <span className="text-[11px] font-normal opacity-80">({reviews.length})</span>
                  </span>
                )}
              </div>

              <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-[var(--text-primary)] mb-2 leading-snug">
                {course.title}
              </h1>

              {course.description && (
                <p className="text-sm text-[var(--text-secondary)] mb-4 leading-relaxed max-w-3xl">
                  {course.description}
                </p>
              )}

              {/* Stats & Skills pills */}
              <div className="flex flex-wrap items-center gap-2 text-xs text-[var(--text-secondary)]">
                <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-[8px] bg-[var(--surface-elevated)] border border-[var(--border-calm)]">
                  <Layers className="w-3.5 h-3.5 text-[var(--text-tertiary)]" />
                  {course.chapters.length} chương
                </span>
                <span>·</span>
                <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-[8px] bg-[var(--surface-elevated)] border border-[var(--border-calm)]">
                  <BookOpen className="w-3.5 h-3.5 text-[var(--text-tertiary)]" />
                  {course.totalLessons} bài giảng
                </span>
                <span>·</span>
                <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-[8px] bg-[var(--surface-elevated)] border border-[var(--border-calm)]">
                  <Clock className="w-3.5 h-3.5 text-[var(--text-tertiary)]" />
                  {theme.estimatedHoursLabel} ({totalMinutes} phút)
                </span>
                {skillTags.map((tag) => (
                  <span
                    key={tag}
                    className="inline-flex items-center gap-1 px-2.5 py-1 rounded-[8px] bg-[#EEF0FD]/70 dark:bg-[#1E1B4B]/60 text-[#4F46E5] dark:text-[#A5B4FC] font-medium"
                  >
                    <Sparkles className="w-3 h-3" />
                    {tag}
                  </span>
                ))}
              </div>
            </div>

            {/* Progress Ring */}
            <div className="flex items-center gap-4 p-4 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 shrink-0 self-start">
              <div className="relative w-20 h-20 flex items-center justify-center shrink-0">
                <svg className="w-20 h-20 -rotate-90" viewBox="0 0 80 80">
                  <circle
                    cx="40"
                    cy="40"
                    r={ringRadius}
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="7"
                    className="text-slate-200 dark:text-slate-800"
                  />
                  <circle
                    cx="40"
                    cy="40"
                    r={ringRadius}
                    fill="none"
                    stroke={course.progressPercent === 100 ? "#10B981" : theme.ringColor}
                    strokeWidth="7"
                    strokeLinecap="round"
                    strokeDasharray={ringCirc}
                    strokeDashoffset={ringOffset}
                  />
                </svg>
                <span className="absolute text-base font-bold text-[var(--text-primary)]">
                  {course.progressPercent}%
                </span>
              </div>
              <div className="space-y-1 text-xs">
                <p className="font-semibold text-[var(--text-primary)]">
                  Tiến độ: {course.completedLessons}/{course.totalLessons} bài
                </p>
                <p className="text-[var(--text-secondary)] font-medium">
                  Thời gian học: {studiedTimeLabel}
                </p>
                <p className="text-[var(--text-tertiary)]">
                  {remainingLessons === 0
                    ? "Đã hoàn tất toàn bộ lộ trình!"
                    : `Còn ${remainingLessons} bài để về đích`}
                </p>
                <div className="pt-0.5">
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-indigo-50 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-300 border border-indigo-200/70 dark:border-indigo-800/70 text-[11px] font-semibold">
                    <Target className="w-3 h-3 shrink-0" />
                    <span>Mục tiêu: {course.dailyLessonGoal ?? 2} bài/ngày</span>
                  </span>
                </div>
              </div>
            </div>
          </div>

          {/* Resume Banner */}
          {resumeLesson && (
            <div className="rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] p-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
              <div className="min-w-0 space-y-1">
                <div className="flex flex-wrap items-center gap-2">
                  <p className="text-[11px] font-semibold uppercase tracking-wider text-[#5E51E8] dark:text-[#A5B4FC]">
                    {course.progressPercent === 100
                      ? "Ôn lại lộ trình"
                      : resumeState?.lastLessonId
                        ? "Bài học đang xem gần nhất"
                        : "Bài học tiếp theo"}
                  </p>
                  {typeof resumeState?.readPercent === "number" && resumeState.readPercent > 0 && (
                    <span className="inline-flex items-center px-2 py-0.5 rounded-full text-[11px] font-semibold bg-[#EEF0FD] dark:bg-[#1E1B4B] text-[#4F46E5] dark:text-[#A5B4FC]">
                      Đã đọc {resumeState.readPercent}%
                    </span>
                  )}
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-medium bg-[var(--surface-card)] border border-[var(--border-calm)] text-[var(--text-secondary)]">
                    <Clock className="w-3 h-3" />
                    {studiedTimeLabel}
                  </span>
                </div>
                <p className="text-sm font-semibold text-[var(--text-primary)] line-clamp-2">
                  {resumeState?.lastLessonTitle || resumeLesson.title}
                </p>
              </div>

              <Link href={continueLearnHref} data-testid="course-continue-learn-btn">
                <Button
                  variant="primary"
                  leftIcon={<Play className="w-4 h-4 fill-current" />}
                  className="shrink-0 bg-[#5E51E8] hover:bg-[#4F46E5] text-white"
                >
                  <span>
                    {course.progressPercent > 0 || Boolean(resumeState?.lastLessonId)
                      ? "Tiếp tục học"
                      : "Bắt đầu học"}
                  </span>
                </Button>
              </Link>
            </div>
          )}

          {/* Hero Quick Action Buttons */}
          <div className="flex flex-wrap items-center gap-2 pt-1 border-t border-[var(--border-calm)]/60">
            {!resumeLesson && (
              <Link href={continueLearnHref} data-testid="course-continue-learn-btn">
                <Button
                  variant="primary"
                  leftIcon={<Play className="w-4 h-4 fill-current" />}
                  className="bg-[#5E51E8] hover:bg-[#4F46E5] text-white"
                >
                  <span>{course.progressPercent > 0 ? "Tiếp tục học" : "Bắt đầu học"}</span>
                </Button>
              </Link>
            )}

            {/* Quick In-Course Search */}
            <Button
              type="button"
              variant="secondary"
              onClick={() => setShowSearchModal(true)}
              title="Tìm kiếm trong khóa (Ctrl+Shift+F)"
              leftIcon={<Search className="w-4 h-4 text-[var(--text-secondary)]" />}
            >
              <span>Tìm kiếm trong khóa</span>
            </Button>

            {/* Quiz Link */}
            <Link href={`/app/courses/${courseId}/quiz`} data-testid="course-quiz-btn">
              <Button
                variant="secondary"
                leftIcon={<ClipboardCheck className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />}
              >
                <span>
                  {(course.quizCount ?? 0) > 0
                    ? `Làm Quiz (${course.quizCount})`
                    : "Làm Quiz"}
                </span>
              </Button>
            </Link>

            {/* Personal Course Edit Studio Link */}
            {!isSystemCourse && (
              <Link href={`/app/courses/${courseId}/edit`}>
                <Button
                  variant="secondary"
                  leftIcon={<Edit3 className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />}
                >
                  <span>Biên tập khóa học</span>
                </Button>
              </Link>
            )}

            {/* Quick Pin / Unpin Button */}
            <Button
              type="button"
              variant="secondary"
              disabled={isTogglingPin}
              onClick={handleQuickTogglePin}
              leftIcon={<Pin className="w-4 h-4 text-indigo-500" />}
            >
              <span>{course.isPinned ? "Bỏ ghim" : "Ghim khóa học"}</span>
            </Button>

            {/* Study Guide Download */}
            <Button
              type="button"
              variant="secondary"
              disabled={downloadingGuide}
              onClick={() => void handleDownloadStudyGuide()}
              leftIcon={<Download className="w-4 h-4 text-[var(--text-secondary)]" />}
            >
              <span>{downloadingGuide ? "Đang tạo Đề cương..." : "Tải Đề cương (.md)"}</span>
            </Button>

            {/* Certificate */}
            <Link href={`/app/courses/${courseId}/certificate`} data-testid="course-certificate-btn">
              <Button
                variant="secondary"
                leftIcon={<Award className="w-4 h-4 text-amber-600 dark:text-amber-400" />}
              >
                <span>Xem Tổng kết &amp; Chứng nhận</span>
              </Button>
            </Link>
          </div>
        </div>
      </Card>

      {/* 4 Tabs Synchronized with URL ?tab=syllabus|practice|assets|settings */}
      <div className="border-b border-[var(--border-calm)]">
        <nav
          role="tablist"
          aria-label="Course Hub Tabs"
          onKeyDown={handleTabListKeyDown}
          className="flex items-center gap-1 sm:gap-2 overflow-x-auto no-scrollbar -mb-px"
        >
          {/* Tab 1: Syllabus */}
          <button
            ref={(el) => {
              tabButtonRefs.current.syllabus = el;
            }}
            type="button"
            role="tab"
            data-testid="course-tab-syllabus"
            aria-selected={activeTab === "syllabus"}
            tabIndex={activeTab === "syllabus" ? 0 : -1}
            onClick={() => handleTabChange("syllabus")}
            className={`inline-flex items-center gap-2 py-3 px-3.5 sm:px-4 border-b-2 text-xs sm:text-sm font-semibold whitespace-nowrap transition cursor-pointer ${
              activeTab === "syllabus"
                ? "border-[#5E51E8] text-[#5E51E8] dark:text-[#A5B4FC] dark:border-[#A5B4FC]"
                : "border-transparent text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:border-[var(--border-calm)]"
            }`}
          >
            <BookOpen className="w-4 h-4" />
            <span>Đề cương &amp; Bài học</span>
            <span className="px-1.5 py-0.2 rounded-full text-[10px] font-bold bg-[var(--surface-elevated)] border border-[var(--border-calm)]">
              {course.totalLessons}
            </span>
          </button>

          {/* Tab 2: Practice */}
          <button
            ref={(el) => {
              tabButtonRefs.current.practice = el;
            }}
            type="button"
            role="tab"
            data-testid="course-tab-practice"
            aria-selected={activeTab === "practice"}
            tabIndex={activeTab === "practice" ? 0 : -1}
            onClick={() => handleTabChange("practice")}
            className={`inline-flex items-center gap-2 py-3 px-3.5 sm:px-4 border-b-2 text-xs sm:text-sm font-semibold whitespace-nowrap transition cursor-pointer ${
              activeTab === "practice"
                ? "border-[#5E51E8] text-[#5E51E8] dark:text-[#A5B4FC] dark:border-[#A5B4FC]"
                : "border-transparent text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:border-[var(--border-calm)]"
            }`}
          >
            <FlaskConical className="w-4 h-4" />
            <span>Thực hành &amp; Ôn tập</span>
            {practiceTabBadgeCount && (
              <span className="px-1.5 py-0.2 rounded-full text-[10px] font-bold bg-indigo-50 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800">
                {practiceTabBadgeCount}
              </span>
            )}
          </button>

          {/* Tab 3: Assets */}
          <button
            ref={(el) => {
              tabButtonRefs.current.assets = el;
            }}
            type="button"
            role="tab"
            data-testid="course-tab-assets"
            aria-selected={activeTab === "assets"}
            tabIndex={activeTab === "assets" ? 0 : -1}
            onClick={() => handleTabChange("assets")}
            className={`inline-flex items-center gap-2 py-3 px-3.5 sm:px-4 border-b-2 text-xs sm:text-sm font-semibold whitespace-nowrap transition cursor-pointer ${
              activeTab === "assets"
                ? "border-[#5E51E8] text-[#5E51E8] dark:text-[#A5B4FC] dark:border-[#A5B4FC]"
                : "border-transparent text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:border-[var(--border-calm)]"
            }`}
          >
            <FolderArchive className="w-4 h-4" />
            <span>Tài nguyên &amp; Ghi chú</span>
            {assetsTabBadgeCount > 0 && (
              <span className="px-1.5 py-0.2 rounded-full text-[10px] font-bold bg-amber-50 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300 border border-amber-200 dark:border-amber-800">
                {assetsTabBadgeCount}
              </span>
            )}
          </button>

          {/* Tab 4: Settings */}
          <button
            ref={(el) => {
              tabButtonRefs.current.settings = el;
            }}
            type="button"
            role="tab"
            data-testid="course-tab-settings"
            aria-selected={activeTab === "settings"}
            tabIndex={activeTab === "settings" ? 0 : -1}
            onClick={() => handleTabChange("settings")}
            className={`inline-flex items-center gap-2 py-3 px-3.5 sm:px-4 border-b-2 text-xs sm:text-sm font-semibold whitespace-nowrap transition cursor-pointer ${
              activeTab === "settings"
                ? "border-[#5E51E8] text-[#5E51E8] dark:text-[#A5B4FC] dark:border-[#A5B4FC]"
                : "border-transparent text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:border-[var(--border-calm)]"
            }`}
          >
            <Settings className="w-4 h-4" />
            <span>Cài đặt khóa học</span>
          </button>
        </nav>
      </div>

      {/* Tab Panels */}
      <div>
        {activeTab === "syllabus" && (
          <CourseSyllabusTab
            course={course}
            courseId={courseId}
            isDone={isDone}
            sequentialUnlock={sequentialUnlock}
            handleToggleSequentialUnlock={handleToggleSequentialUnlock}
            unlockedLessonIds={unlockedLessonIds}
            nextLesson={nextLesson}
            expandedChapters={expandedChapters}
            setExpandedChapters={setExpandedChapters}
            reviews={reviews}
            averageRating={averageRating}
            handleSubmitReview={handleSubmitReview}
            ratingInput={ratingInput}
            setRatingInput={setRatingInput}
            commentInput={commentInput}
            setCommentInput={setCommentInput}
            submittingReview={submittingReview}
            reviewNotice={reviewNotice}
          />
        )}

        {activeTab === "practice" && (
          <CoursePracticeTab course={course} courseId={courseId} />
        )}

        {activeTab === "assets" && (
          <CourseAssetsTab course={course} courseId={courseId} />
        )}

        {activeTab === "settings" && (
          <CourseSettingsTab
            course={course}
            courseId={courseId}
            onCourseUpdated={(updated) => {
              setCourse((prev) => (prev ? { ...prev, ...updated } : prev));
            }}
            onProgressReset={(resetCount) => {
              fetchCourse();
            }}
          />
        )}
      </div>

      {/* Reusable In-Course Search Modal */}
      <CourseInSearchModal
        courseId={courseId}
        isOpen={showSearchModal}
        onClose={() => setShowSearchModal(false)}
        onSelectLesson={(lessonId) => router.push(`/app/courses/${courseId}/learn?lessonId=${lessonId}`)}
      />

      {/* Lightweight Toast Notification */}
      {toast && (
        <div
          role="status"
          aria-live="polite"
          className={`fixed bottom-20 md:bottom-6 right-4 sm:right-6 z-50 max-w-sm flex items-center gap-2.5 px-4 py-3 rounded-xl border shadow-lg text-xs font-medium transition-all ${
            toast.type === "error"
              ? "bg-rose-50 dark:bg-rose-950/95 border-rose-300 dark:border-rose-800 text-rose-800 dark:text-rose-200"
              : "bg-[var(--surface-card)] border-[#5E51E8]/30 text-[var(--text-primary)]"
          }`}
        >
          {toast.type === "error" ? (
            <AlertCircle className="w-4 h-4 text-rose-600 dark:text-rose-400 shrink-0" />
          ) : (
            <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
          )}
          <span className="leading-snug">{toast.message}</span>
          <button
            type="button"
            onClick={() => setToast(null)}
            aria-label="Đóng thông báo"
            className="ml-1 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer shrink-0"
          >
            <X className="w-3.5 h-3.5" />
          </button>
        </div>
      )}
    </div>
  );
}

export default function CourseDetailPage() {
  return (
    <Suspense
      fallback={
        <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 min-h-[80vh] space-y-6">
          <div className="h-6 w-32 rounded bg-[var(--surface-elevated)] animate-pulse" />
          <div className="h-10 w-96 rounded-lg bg-[var(--surface-elevated)] animate-pulse" />
          <div className="h-28 w-full rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
        </div>
      }
    >
      <CourseDetailPageInner />
    </Suspense>
  );
}
