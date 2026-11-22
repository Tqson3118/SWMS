"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import {
  AlertCircle,
  Award,
  BookOpen,
  Brain,
  Check,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  ChevronsDownUp,
  ChevronsUpDown,
  ClipboardCheck,
  Clock,
  Download,
  FlaskConical,
  Layers,
  Lock,
  MessageSquare,
  Pencil,
  Play,
  RefreshCw,
  Search,
  Send,
  Sparkles,
  Star,
  Target,
  Unlock,
  X,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached } from "@/lib/api-client";
import { Button, Card, Badge, EmptyState } from "@/components/ui";
import {
  classifyCourseDomain,
  extractCourseSkillTags,
  getCourseTotalMinutes,
  getCourseVisualTheme,
  normalizeVi,
  stripChapterPrefix,
} from "@/lib/course-helpers";

interface CourseLesson {
  id: string;
  title: string;
  durationMinutes?: number;
  estimatedMinutes?: number;
  isCompleted?: boolean;
  sortOrder?: number;
  courseId?: string;
  practiceActivityCount?: number;
  practicePassedCount?: number;
}

interface CourseChapter {
  id: string;
  chapterId?: string;
  title: string;
  sortOrder?: number;
  lessons: CourseLesson[];
}

interface WeakChapterItem {
  chapterId: string;
  title: string;
  completionPercent: number;
  reason?: string;
  nextLessonId?: string;
}

interface CourseResumeState {
  lastLessonId?: string | null;
  lastLessonTitle?: string | null;
  readPercent?: number;
  totalStudiedMinutes?: number;
  lastAccessedAt?: string | null;
}

interface CourseReviewItem {
  id: string;
  userName: string;
  rating: number;
  comment: string;
  createdAt?: string | null;
}

interface CourseDetail {
  id: string;
  title: string;
  description?: string;
  category?: string;
  visibility?: string;
  isPublished?: boolean;
  progressPercent: number;
  totalLessons: number;
  completedLessons: number;
  completedLessonIds?: string[];
  totalEstimatedMinutes?: number;
  flashcardCount?: number;
  dueFlashcardCount?: number;
  masteredFlashcardCount?: number;
  practiceTotalActivities?: number;
  practicePassedActivities?: number;
  practiceAverageScore?: number;
  quizCount?: number;
  quizId?: string | null;
  weakChapters?: WeakChapterItem[];
  chapters: CourseChapter[];
}

type LessonFilter = "all" | "pending" | "completed";

function getLessonPracticeChips(domainId: string, lessonTitle: string): string[] {
  const norm = normalizeVi(lessonTitle);
  if (domainId === "language") {
    if (/nghe|listening|audio|wav|hoi thoai/.test(norm)) return ["Nghe & Chép", "Phát âm IPA", "Quiz"];
    if (/doc|reading|passage/.test(norm)) return ["Đọc hiểu", "Từ vựng FSRS", "Quiz"];
    if (/viet|writing|essay/.test(norm)) return ["Luyện viết", "Ngữ pháp"];
    return ["Luyện nghe nói", "Flashcard FSRS", "Quiz"];
  }
  if (domainId === "technology") {
    if (/database|sql|du lieu/.test(norm)) return ["SQL Lab", "Query Test", "Quiz"];
    if (/thuat toan|cau truc/.test(norm)) return ["Code Lab", "Benchmark"];
    if (/api|service|microservice/.test(norm)) return ["API Drill", "Code Lab", "Quiz"];
    return ["Code Lab", "Debug Test", "Quiz"];
  }
  if (domainId === "business") {
    if (/ngan sach|dong tien|tai chinh/.test(norm)) return ["Bảng tính FCF", "Case Study", "Quiz"];
    if (/dau tu|co phieu|tai san/.test(norm)) return ["Mô hình đầu tư", "Thực hành"];
    return ["Case Study", "Thẻ nhớ FSRS", "Quiz"];
  }
  if (domainId === "law") {
    return ["Tình huống án lệ", "Văn bản luật", "Quiz"];
  }
  if (domainId === "science") {
    return ["Giải bài tập", "Công thức", "Quiz"];
  }
  return ["Tình huống thực tế", "Thẻ nhớ FSRS", "Quiz"];
}

function getCourseLearningOutcomes(domainId: string): string[] {
  if (domainId === "language") {
    return [
      "Làm chủ 4 kỹ năng Nghe, Nói, Đọc, Viết theo lộ trình bài bản.",
      "Luyện phản xạ Nghe & Chép chính tả và phát âm chuẩn âm vị quốc tế.",
      "Ghi nhớ từ vựng dài hạn với thuật toán ngắt quãng FSRS v5.",
      "Đánh giá tiến bộ định kỳ qua hệ thống bài thi Quiz thực chiến.",
    ];
  }
  if (domainId === "technology") {
    return [
      "Nắm vững kiến trúc, tư duy thiết kế và cấu trúc hệ thống chịu tải cao.",
      "Thực hành viết mã, debug và kiểm thử trực tiếp trong Code Lab.",
      "Áp dụng Best Practices, Design Patterns và xử lý tình huống thực tế.",
      "Tự tin xây dựng sản phẩm hoàn chỉnh và đáp ứng tiêu chuẩn ngành.",
    ];
  }
  if (domainId === "business") {
    return [
      "Quản lý dòng tiền cá nhân và doanh nghiệp, thiết lập quỹ dự phòng.",
      "Thực hành phân tích báo cáo tài chính và định giá tài sản sinh lời.",
      "Xây dựng chiến lược phân bổ vốn và quản trị rủi ro đầu tư bền vững.",
      "Hệ thống hóa tư duy kinh tế và ra quyết định tài chính kỷ luật.",
    ];
  }
  if (domainId === "law") {
    return [
      "Nắm vững hệ thống quy phạm pháp luật và nguyên tắc áp dụng thực tiễn.",
      "Phân tích các tình huống tranh chấp pháp lý và án lệ điển hình.",
      "Hệ thống hóa thuật ngữ pháp lý và quy trình tố tụng chuẩn xác.",
      "Rèn luyện tư duy lập luận pháp lý sắc bén và tuân thủ chuẩn mực.",
    ];
  }
  if (domainId === "science") {
    return [
      "Hiểu sâu bản chất các định lý, công thức và nguyên lý toán học / khoa học.",
      "Rèn luyện phương pháp giải bài tập logic từ cơ bản đến phức tạp.",
      "Áp dụng tư duy phân tích định lượng vào các bài toán thực tiễn.",
      "Củng cố phản xạ giải đề qua ngân hàng câu hỏi trắc nghiệm & tự luận.",
    ];
  }
  return [
    "Hệ thống hóa toàn bộ kiến thức cốt lõi theo từng chặng rõ ràng.",
    "Rèn luyện kỹ năng thực hành tương tác qua các tình huống giả lập.",
    "Khắc sâu kiến thức dài hạn với hệ thống thẻ nhớ thông minh FSRS.",
    "Tự tin ứng dụng kiến thức vào công việc và cuộc sống hàng ngày.",
  ];
}

export default function CourseDetailPage() {
  const params = useParams();
  const courseId = params.id as string;
  const { activeUserHeaderId } = useWorkspaceStore();

  const [course, setCourse] = useState<CourseDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const [searchQuery, setSearchQuery] = useState("");
  const [lessonFilter, setLessonFilter] = useState<LessonFilter>("all");
  const [expandedChapters, setExpandedChapters] = useState<Set<string>>(new Set());
  const [dailyLessonsPace, setDailyLessonsPace] = useState<number>(2);
  const [sequentialUnlock, setSequentialUnlock] = useState<boolean>(false);

  const [resumeState, setResumeState] = useState<CourseResumeState | null>(null);
  const [reviews, setReviews] = useState<CourseReviewItem[]>([]);
  const [averageRating, setAverageRating] = useState<number>(0);
  const [ratingInput, setRatingInput] = useState<number>(5);
  const [commentInput, setCommentInput] = useState<string>("");
  const [submittingReview, setSubmittingReview] = useState<boolean>(false);
  const [reviewNotice, setReviewNotice] = useState<string | null>(null);

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

  const [downloadingGuide, setDownloadingGuide] = useState(false);

  const handleDownloadStudyGuide = async () => {
    if (!courseId || downloadingGuide) return;
    setDownloadingGuide(true);
    try {
      const resolvedUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        resolvedUserId ? { "X-Test-User-Id": resolvedUserId } : undefined
      );
      const resp = await fetch(`/api/v1/courses/${courseId}/study-guide`, { headers });
      if (!resp.ok) {
        setFetchError(`Không xuất được đề cương (HTTP ${resp.status}).`);
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
    } catch {
      setFetchError("Lỗi kết nối khi tải đề cương ôn tập.");
    } finally {
      setDownloadingGuide(false);
    }
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
    const explicitAvg = Number(
      (rawReviews as { averageRating?: unknown }).averageRating ?? 0
    );
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
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );

      const [courseResp, resumeResp, reviewsResp] = await Promise.all([
        fetch(`/api/v1/courses/${courseId}`, { headers }).catch(() => null),
        fetch(`/api/v1/courses/${courseId}/resume-state`, { headers }).catch(() => null),
        fetch(`/api/v1/courses/${courseId}/reviews`, { headers }).catch(() => null),
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
          totalLessons: total,
          completedLessons: completed,
          completedLessonIds: completedIds,
          progressPercent: progress,
          weakChapters: apiWeakChapters,
          chapters: chaps,
        };
      } else {
        const dashData = await fetchDashboardCached({ headers }).catch(() => null);
        if (dashData) {
          const allCourses = dashData.courses ?? [];
        const found = allCourses.find(
          (c: Record<string, unknown>) => (c.courseId || c.id) === courseId
        );
        if (found) {
          const chaps = (found.chapters as CourseChapter[]) ?? [];
          const total = Number(
            found.totalLessons ??
              found.lessonCount ??
              chaps.reduce((s, ch) => s + (ch.lessons?.length ?? 0), 0)
          );
          const completedIds = Array.isArray(found.completedLessonIds)
            ? (found.completedLessonIds as string[])
            : [];
          const completed = Number(found.completedLessons ?? completedIds.length);
          courseData = {
            id: String(found.courseId || found.id),
            title: String(found.title || "Khóa học"),
            description: String(found.description || ""),
            category: String(found.category || ""),
            visibility: String(found.visibility || "personal"),
            isPublished: Boolean(found.isPublished ?? found.visibility === "public"),
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
        setReviewNotice(
          `Không thể gửi đánh giá (HTTP ${resp.status})${errText ? `: ${errText}` : ""}`
        );
      }
    } catch {
      setReviewNotice("Lỗi kết nối khi gửi đánh giá.");
    } finally {
      setSubmittingReview(false);
    }
  };

  useEffect(() => {
    fetchCourse();
  }, [fetchCourse]);

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

  const weakChaptersList = useMemo<WeakChapterItem[]>(() => {
    if (!course) return [];
    if (Array.isArray(course.weakChapters) && course.weakChapters.length > 0) {
      return course.weakChapters;
    }
    const computed: WeakChapterItem[] = [];
    for (const ch of course.chapters) {
      const chKey = String(ch.chapterId || ch.id);
      const lessons = ch.lessons ?? [];
      if (lessons.length === 0) continue;
      const doneCount = lessons.filter((l) => isDone(l)).length;
      const pct = Math.round((doneCount * 100) / lessons.length);
      const firstPending = lessons.find((l) => !isDone(l));
      if (pct > 0 && pct < 100) {
        computed.push({
          chapterId: chKey,
          title: stripChapterPrefix(ch.title) || ch.title,
          completionPercent: pct,
          reason: `Đang hoàn thành dở dang (${doneCount}/${lessons.length} bài)`,
          nextLessonId: firstPending?.id,
        });
      }
    }
    if (computed.length === 0 && course.chapters.length > 0) {
      const firstIncompleteCh = course.chapters.find((ch) =>
        (ch.lessons ?? []).some((l) => !isDone(l))
      );
      if (firstIncompleteCh) {
        const lessons = firstIncompleteCh.lessons ?? [];
        const doneCount = lessons.filter((l) => isDone(l)).length;
        const pct = lessons.length > 0 ? Math.round((doneCount * 100) / lessons.length) : 0;
        const firstPending = lessons.find((l) => !isDone(l));
        computed.push({
          chapterId: String(firstIncompleteCh.chapterId || firstIncompleteCh.id),
          title: stripChapterPrefix(firstIncompleteCh.title) || firstIncompleteCh.title,
          completionPercent: pct,
          reason: "Chặng trọng tâm tiếp theo cần củng cố",
          nextLessonId: firstPending?.id,
        });
      }
    }
    return computed.slice(0, 3);
  }, [course, isDone]);

  const filteredChapters = useMemo(() => {
    if (!course) return [];
    const q = normalizeVi(searchQuery);
    return course.chapters
      .map((ch) => {
        const matchingLessons = (ch.lessons ?? []).filter((lesson) => {
          const done = isDone(lesson);
          if (lessonFilter === "completed" && !done) return false;
          if (lessonFilter === "pending" && done) return false;
          if (q && !normalizeVi(lesson.title).includes(q)) return false;
          return true;
        });
        return { ...ch, lessons: matchingLessons };
      })
      .filter((ch) => ch.lessons.length > 0 || (!q && lessonFilter === "all"));
  }, [course, searchQuery, lessonFilter, isDone]);

  const allExpanded =
    course !== null &&
    course.chapters.length > 0 &&
    course.chapters.every((ch) => expandedChapters.has(String(ch.chapterId || ch.id)));

  const toggleAllChapters = () => {
    if (!course) return;
    if (allExpanded) {
      setExpandedChapters(new Set());
    } else {
      setExpandedChapters(new Set(course.chapters.map((ch) => String(ch.chapterId || ch.id))));
    }
  };

  const toggleChapter = (chId: string) => {
    setExpandedChapters((prev) => {
      const next = new Set(prev);
      if (next.has(chId)) next.delete(chId);
      else next.add(chId);
      return next;
    });
  };

  if (isLoading) {
    return (
      <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh] space-y-6">
        <div className="h-6 w-32 rounded bg-[var(--surface-elevated)] animate-pulse" />
        <div className="h-10 w-96 rounded-lg bg-[var(--surface-elevated)] animate-pulse" />
        <div className="h-20 w-full rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
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
  const estimatedDaysToFinish =
    remainingLessons > 0 ? Math.ceil(remainingLessons / Math.max(1, dailyLessonsPace)) : 0;
  const ringRadius = 32;
  const ringCirc = 2 * Math.PI * ringRadius;
  const ringOffset = ringCirc - (Math.min(100, Math.max(0, course.progressPercent)) / 100) * ringCirc;

  const continueLearnHref = resumeLesson
    ? `/app/courses/${courseId}/learn?lesson=${resumeLesson.id}`
    : `/app/courses/${courseId}/learn`;

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-32 md:pb-10">
      {/* Breadcrumb */}
      <nav
        aria-label="Breadcrumb"
        className="flex items-center gap-2 text-sm text-[var(--text-tertiary)] mb-5 overflow-x-auto whitespace-nowrap"
      >
        <Link href="/app/courses" className="hover:text-[var(--text-primary)] transition">
          Khóa học
        </Link>
        <span>·</span>
        <span className="text-[var(--text-primary)] font-medium truncate">{course.title}</span>
      </nav>

      {/* Hero Summary Card */}
      <Card className="mb-6 bg-[var(--surface-card)] overflow-hidden p-0">
        <div className={`h-2 w-full bg-gradient-to-r ${theme.accentBarClass}`} />
        <div className="p-5 sm:p-6">
          <div className="flex flex-col md:flex-row md:items-start justify-between gap-5 mb-5">
            <div className="min-w-0 flex-1">
              <div className="flex flex-wrap items-center gap-2 mb-3">
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-md border text-xs font-semibold ${theme.badgeBgClass}`}>
                  {theme.domainLabel}
                </span>
                <Badge variant="neutral" size="sm">
                  {theme.levelLabel}
                </Badge>
                {(course.isPublished || course.visibility === "public") && (
                  <Badge variant="success" size="sm">
                    Công khai
                  </Badge>
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

              {/* Skill Pills & Stats */}
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

              {/* Core Learning Outcomes */}
              <div className="mt-4 pt-3.5 border-t border-[var(--border-calm)]/70">
                <h4 className="text-xs font-bold uppercase tracking-wider text-[var(--text-tertiary)] mb-2 flex items-center gap-1.5">
                  <Target className="w-3.5 h-3.5 text-[#5E51E8] dark:text-[#A5B4FC]" />
                  <span>Mục tiêu đầu ra cốt lõi</span>
                </h4>
                <ul className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs text-[var(--text-secondary)]">
                  {getCourseLearningOutcomes(domain.id).map((item, idx) => (
                    <li key={idx} className="flex items-start gap-2">
                      <CheckCircle2 className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400 shrink-0 mt-0.5" />
                      <span className="leading-relaxed">{item}</span>
                    </li>
                  ))}
                </ul>
              </div>
            </div>

            {/* Progress Ring Card */}
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
                  Tiến độ: {course.completedLessons}/{course.totalLessons} bài hoàn thành
                </p>
                <p className="text-[var(--text-secondary)] font-medium">
                  Thời gian học: {studiedTimeLabel}
                </p>
                <p className="text-[var(--text-tertiary)]">
                  {remainingLessons === 0
                    ? "Đã hoàn tất toàn bộ lộ trình!"
                    : `Còn ${remainingLessons} bài để về đích`}
                </p>
              </div>
            </div>
          </div>

          {/* 3-Pillar Practice Lab & FSRS Status Strip */}
          {((course.practiceTotalActivities ?? 0) > 0 || (course.flashcardCount ?? 0) > 0) && (
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-2.5 mb-4">
              <div className="rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 px-3.5 py-2.5 flex items-center justify-between gap-2">
                <div className="flex items-center gap-2 min-w-0">
                  <FlaskConical className="w-4 h-4 text-indigo-600 dark:text-indigo-400 shrink-0" />
                  <div className="min-w-0">
                    <p className="text-[11px] text-[var(--text-tertiary)] font-medium">Practice Lab</p>
                    <p className="text-xs font-bold text-[var(--text-primary)] truncate">
                      {course.practicePassedActivities ?? 0}/{course.practiceTotalActivities ?? 0} bài đạt
                    </p>
                  </div>
                </div>
                {(course.practiceAverageScore ?? 0) > 0 && (
                  <span className="text-[11px] font-semibold px-2 py-0.5 rounded-full bg-indigo-50 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-300 shrink-0">
                    TB {course.practiceAverageScore}%
                  </span>
                )}
              </div>

              <div className="rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 px-3.5 py-2.5 flex items-center justify-between gap-2">
                <div className="flex items-center gap-2 min-w-0">
                  <Brain className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0" />
                  <div className="min-w-0">
                    <p className="text-[11px] text-[var(--text-tertiary)] font-medium">Thẻ nhớ FSRS</p>
                    <p className="text-xs font-bold text-[var(--text-primary)] truncate">
                      {course.masteredFlashcardCount ?? 0}/{course.flashcardCount ?? 0} thẻ đã thuộc
                    </p>
                  </div>
                </div>
                {(course.dueFlashcardCount ?? 0) > 0 && (
                  <span className="text-[11px] font-semibold px-2 py-0.5 rounded-full bg-amber-50 text-amber-700 border border-amber-200 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-800 shrink-0">
                    {course.dueFlashcardCount} đến hạn
                  </span>
                )}
              </div>

              <div className="rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 px-3.5 py-2.5 flex items-center justify-between gap-2">
                <div className="flex items-center gap-2 min-w-0">
                  <ClipboardCheck className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
                  <div className="min-w-0">
                    <p className="text-[11px] text-[var(--text-tertiary)] font-medium">Ngân hàng Đề thi</p>
                    <p className="text-xs font-bold text-[var(--text-primary)] truncate">
                      {course.quizCount ?? 0} đề kiểm tra &amp; tổng hợp
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Next / Resume Lesson Highlight Banner */}
          {resumeLesson && (
            <div className="rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] p-4 mb-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
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
              <Link
                href={continueLearnHref}
                data-testid="course-continue-learn-btn"
              >
                <Button
                  variant="primary"
                  leftIcon={<Play className="w-4 h-4" />}
                  className="shrink-0"
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

          {/* Secondary Actions */}
          <div className="flex flex-wrap items-center gap-2">
            {!resumeLesson && (
              <Link
                href={continueLearnHref}
                data-testid="course-continue-learn-btn"
              >
                <Button
                  variant="primary"
                  leftIcon={<Play className="w-4 h-4" />}
                >
                  <span>{course.progressPercent > 0 ? "Tiếp tục học" : "Bắt đầu học"}</span>
                </Button>
              </Link>
            )}
            {(course.quizCount ?? 0) > 0 && (
              <Link
                href={`/app/courses/${courseId}/quiz`}
                data-testid="course-quiz-btn"
              >
                <Button
                  variant="secondary"
                  leftIcon={<ClipboardCheck className="w-4 h-4 text-[var(--text-secondary)]" />}
                >
                  <span>Làm bài kiểm tra ({course.quizCount})</span>
                </Button>
              </Link>
            )}
            <Link
              href={`/app/review?courseId=${courseId}`}
              data-testid="course-review-fsrs-btn"
            >
              <Button
                variant="secondary"
                leftIcon={<Brain className="w-4 h-4 text-amber-600 dark:text-amber-400" />}
              >
                <span>
                  Ôn thẻ FSRS khóa này
                  {(course.dueFlashcardCount ?? 0) > 0
                    ? ` (${course.dueFlashcardCount} đến hạn)`
                    : course.flashcardCount
                      ? ` (${course.flashcardCount})`
                      : ""}
                </span>
              </Button>
            </Link>
            <Button
              type="button"
              variant="secondary"
              disabled={downloadingGuide}
              onClick={() => void handleDownloadStudyGuide()}
              data-testid="download-study-guide-btn"
              leftIcon={<Download className="w-4 h-4 text-[var(--text-secondary)]" />}
            >
              <span>{downloadingGuide ? "Đang tạo Đề cương..." : "Tải Đề cương (.md)"}</span>
            </Button>
            <Link
              href={`/app/courses/${courseId}/certificate`}
              data-testid="course-certificate-btn"
            >
              <Button
                variant="secondary"
                leftIcon={<Award className="w-4 h-4 text-amber-600 dark:text-amber-400" />}
              >
                <span>Xem Tổng kết &amp; Chứng nhận</span>
              </Button>
            </Link>
            <Link
              href={`/app/courses/${courseId}/edit`}
            >
              <Button
                variant="secondary"
                leftIcon={<Pencil className="w-4 h-4 text-[var(--text-secondary)]" />}
              >
                <span>Biên tập khóa học</span>
              </Button>
            </Link>
          </div>
        </div>
      </Card>

      {/* Mobile Study Pace Calculator (Prominent right below Hero on mobile/tablet) */}
      <div className="lg:hidden mb-6">
        <Card className="p-4 bg-[var(--surface-card)] space-y-3">
          <div className="flex items-center justify-between gap-2">
            <div className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-[#5E51E8] dark:text-[#A5B4FC]">
              <Clock className="w-3.5 h-3.5" />
              <span>Máy tính nhịp độ hoàn thành</span>
            </div>
            <span className="text-xs font-bold text-[#5E51E8] dark:text-[#A5B4FC]">
              {estimatedDaysToFinish === 0 ? "Đã hoàn thành!" : `~${estimatedDaysToFinish} ngày`}
            </span>
          </div>
          <div className="flex items-center gap-3">
            <span className="text-xs text-[var(--text-secondary)] whitespace-nowrap">
              {dailyLessonsPace} bài/ngày
            </span>
            <input
              type="range"
              min={1}
              max={6}
              step={1}
              value={dailyLessonsPace}
              onChange={(e) => setDailyLessonsPace(Number(e.target.value))}
              aria-label="Số bài học mục tiêu mỗi ngày"
              className="flex-1 accent-[#5E51E8] cursor-pointer"
            />
          </div>
        </Card>
      </div>

      {/* Horizontal Compact Milestone Timeline */}
      {course.chapters.length > 0 && (
        <section className="mb-6">
          <div className="flex items-center justify-between mb-2.5">
            <h2 className="text-sm font-semibold text-[var(--text-primary)] flex items-center gap-1.5">
              <Target className="w-4 h-4 text-[#5E51E8] dark:text-[#A5B4FC]" />
              <span>Timeline các chặng học tập ({course.chapters.length} chặng)</span>
            </h2>
            <span className="text-xs text-[var(--text-tertiary)]">Bấm để cuộn tới chặng</span>
          </div>
          <div className="flex overflow-x-auto gap-3 pb-2 snap-x no-scrollbar">
            {course.chapters.map((ch, idx) => {
              const chKey = String(ch.chapterId || ch.id);
              const chLessons = ch.lessons ?? [];
              const chDone = chLessons.filter((l) => isDone(l)).length;
              const chPct = chLessons.length > 0 ? Math.round((chDone * 100) / chLessons.length) : 0;
              const cleanTitle = stripChapterPrefix(ch.title) || ch.title;
              return (
                <button
                  key={chKey}
                  type="button"
                  onClick={() => {
                    setExpandedChapters((prev) => new Set(prev).add(chKey));
                    document.getElementById(`chapter-card-${chKey}`)?.scrollIntoView({
                      behavior: "smooth",
                      block: "center",
                    });
                  }}
                  className={`min-w-[210px] sm:min-w-[230px] max-w-[260px] shrink-0 snap-start text-left rounded-xl border p-3.5 transition cursor-pointer flex flex-col justify-between gap-2 ${
                    chPct === 100
                      ? "border-emerald-300 dark:border-emerald-800 bg-emerald-50/40 dark:bg-emerald-950/20"
                      : chDone > 0
                        ? "border-[#5E51E8]/50 bg-[#EEF0FD]/40 dark:bg-[#1E1B4B]/40"
                        : "border-[var(--border-calm)] bg-[var(--surface-card)] hover:border-[#5E51E8]/40"
                  }`}
                >
                  <div className="flex items-center justify-between text-[11px] font-semibold">
                    <span className="text-[#5E51E8] dark:text-[#A5B4FC]">Chặng {idx + 1}</span>
                    <span className="text-[var(--text-tertiary)]">
                      {chDone}/{chLessons.length} bài
                    </span>
                  </div>
                  <p className="text-xs font-semibold text-[var(--text-primary)] line-clamp-2 leading-snug">
                    {cleanTitle}
                  </p>
                  <div className="w-full h-1.5 rounded-full bg-[var(--surface-elevated)] overflow-hidden">
                    <div
                      className={`h-full rounded-full ${chPct === 100 ? "bg-emerald-500" : "bg-[#5E51E8]"}`}
                      style={{ width: `${chPct}%` }}
                    />
                  </div>
                </button>
              );
            })}
          </div>
        </section>
      )}

      {/* Main 2-Column Body: Syllabus (8 cols) + Pace & Practice Insights (4 cols) */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Interactive Syllabus Explorer */}
        <div className="lg:col-span-8">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-4">
            <h2 className="text-base font-semibold text-[var(--text-primary)]">Đề cương khóa học</h2>
            <div className="flex flex-wrap items-center gap-3">
              <button
                type="button"
                onClick={handleToggleSequentialUnlock}
                data-testid="sequential-unlock-toggle"
                className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-[8px] border text-xs font-medium transition cursor-pointer ${
                  sequentialUnlock
                    ? "border-[#5E51E8]/40 bg-[#EEF0FD] dark:bg-[#1E1B4B] text-[#4F46E5] dark:text-[#A5B4FC]"
                    : "border-[var(--border-calm)] bg-[var(--surface-elevated)] text-[var(--text-secondary)] hover:text-[var(--text-primary)]"
                }`}
                title="Khi bật, cần hoàn thành bài trước để mở khóa bài tiếp theo. Có thể tắt bất cứ lúc nào."
              >
                {sequentialUnlock ? (
                  <Lock className="w-3.5 h-3.5" />
                ) : (
                  <Unlock className="w-3.5 h-3.5" />
                )}
                <span>Mở khóa bài học theo thứ tự: {sequentialUnlock ? "Bật" : "Tắt"}</span>
              </button>

              {course.chapters.length > 0 && (
                <button
                  type="button"
                  onClick={toggleAllChapters}
                  className="inline-flex items-center gap-1.5 text-xs font-medium text-[var(--text-secondary)] hover:text-[var(--text-primary)] transition cursor-pointer"
                >
                  {allExpanded ? (
                    <>
                      <ChevronsDownUp className="w-3.5 h-3.5" />
                      <span>Thu gọn tất cả</span>
                    </>
                  ) : (
                    <>
                      <ChevronsUpDown className="w-3.5 h-3.5" />
                      <span>Mở rộng tất cả</span>
                    </>
                  )}
                </button>
              )}
            </div>
          </div>

          {/* Search & Lesson Status Filter */}
          {allLessons.length > 0 && (
            <div className="flex flex-col sm:flex-row sm:items-center gap-2.5 mb-4">
              <div className="relative flex-1">
                <Search className="w-4 h-4 text-[var(--text-tertiary)] absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Tìm bài học trong đề cương..."
                  className="w-full h-9 pl-9 pr-8 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8] transition"
                />
                {searchQuery && (
                  <button
                    type="button"
                    onClick={() => setSearchQuery("")}
                    className="absolute right-2.5 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                )}
              </div>

              <div className="flex flex-wrap items-center gap-1.5">
                {(
                  [
                    { id: "all", label: "Tất cả" },
                    { id: "pending", label: "Chưa học" },
                    { id: "completed", label: "Đã hoàn thành" },
                  ] as { id: LessonFilter; label: string }[]
                ).map((tab) => (
                  <button
                    key={tab.id}
                    type="button"
                    onClick={() => setLessonFilter(tab.id)}
                    className={`h-8 px-3 rounded-[8px] text-xs font-medium transition cursor-pointer ${
                      lessonFilter === tab.id
                        ? "bg-[#5E51E8] text-white dark:bg-[#818CF8] dark:text-[#0F0F12]"
                        : "bg-[var(--surface-elevated)] text-[var(--text-secondary)] border border-[var(--border-calm)] hover:text-[var(--text-primary)]"
                    }`}
                  >
                    {tab.label}
                  </button>
                ))}
              </div>
            </div>
          )}

          {filteredChapters.length > 0 ? (
            <div className="space-y-3">
              {filteredChapters.map((chapter, chIdx) => {
                const chKey = String(chapter.chapterId || chapter.id);
                const isOpen =
                  searchQuery.trim().length > 0 ||
                  lessonFilter !== "all" ||
                  expandedChapters.has(chKey);
                const lessons = chapter.lessons ?? [];
                const origChapter =
                  course.chapters.find((c) => String(c.chapterId || c.id) === chKey) ?? chapter;
                const origLessons = origChapter.lessons ?? [];
                const doneInChapter = origLessons.filter((l) => isDone(l)).length;
                const cleanChapterTitle = stripChapterPrefix(chapter.title) || chapter.title;

                return (
                  <Card
                    key={chKey}
                    id={`chapter-card-${chKey}`}
                    className="overflow-hidden"
                  >
                    <button
                      type="button"
                      onClick={() => toggleChapter(chKey)}
                      className="w-full px-4 py-3 bg-[var(--surface-elevated)] hover:bg-[var(--surface-elevated)]/80 flex items-center justify-between gap-3 text-left transition cursor-pointer"
                    >
                      <div className="flex items-start gap-2 min-w-0">
                        {isOpen ? (
                          <ChevronDown className="w-4 h-4 text-[var(--text-tertiary)] shrink-0 mt-0.5" />
                        ) : (
                          <ChevronRight className="w-4 h-4 text-[var(--text-tertiary)] shrink-0 mt-0.5" />
                        )}
                        <h3 className="text-sm font-semibold text-[var(--text-primary)] leading-snug">
                          Chương {chIdx + 1}: {cleanChapterTitle}
                        </h3>
                      </div>
                      <Badge variant="neutral" size="sm">
                        {doneInChapter}/{origLessons.length} bài
                      </Badge>
                    </button>

                    {isOpen && lessons.length > 0 && (
                      <div className="p-4 sm:p-5 border-t border-[var(--border-calm)] bg-[var(--surface-card)]">
                        <div className="relative pl-6 sm:pl-8 space-y-4 before:absolute before:left-3 sm:before:left-3.5 before:top-3 before:bottom-3 before:w-0.5 before:bg-slate-200 dark:before:bg-slate-800">
                          {lessons.map((lesson, lesIdx) => {
                            const completed = isDone(lesson);
                            const isNext = lesson.id === nextLesson?.id && !completed;
                            const isLocked =
                              sequentialUnlock && !completed && !unlockedLessonIds.has(lesson.id);
                            const mins = lesson.durationMinutes ?? lesson.estimatedMinutes ?? 15;
                            const chips = getLessonPracticeChips(domain.id, lesson.title);

                            return (
                              <div
                                key={lesson.id}
                                className={`relative group flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-3.5 rounded-xl border transition shadow-2xs ${
                                  isLocked
                                    ? "border-[var(--border-calm)] bg-[var(--surface-elevated)]/30 opacity-80"
                                    : "border-[var(--border-calm)] hover:border-[#5E51E8]/50 bg-[var(--surface-elevated)]/50 hover:bg-[var(--surface-elevated)]"
                                }`}
                              >
                                {/* Timeline Node Icon Connector */}
                                <div
                                  className={`absolute -left-6 sm:-left-8 top-4 -translate-x-1/2 w-6 h-6 rounded-full border-2 flex items-center justify-center text-[10px] font-bold shadow-xs transition ${
                                    completed
                                      ? "bg-emerald-500 border-emerald-500 text-white"
                                      : isNext
                                      ? "bg-[#5E51E8] border-[#5E51E8] text-white ring-4 ring-[#5E51E8]/20 animate-pulse"
                                      : isLocked
                                      ? "bg-slate-200 dark:bg-slate-800 border-slate-300 dark:border-slate-700 text-slate-500"
                                      : "bg-[var(--surface-card)] border-[var(--border-calm)] text-[var(--text-tertiary)]"
                                  }`}
                                >
                                  {completed ? (
                                    <CheckCircle2 className="w-3.5 h-3.5" />
                                  ) : isNext ? (
                                    <Play className="w-2.5 h-2.5 fill-current ml-0.5" />
                                  ) : isLocked ? (
                                    <Lock className="w-3 h-3" />
                                  ) : (
                                    <span>{lesIdx + 1}</span>
                                  )}
                                </div>

                                {/* Lesson Info */}
                                <div className="min-w-0 flex-1">
                                  <div className="flex flex-wrap items-center gap-2 mb-1">
                                    <span
                                      className={`text-[10px] font-bold px-2 py-0.5 rounded-full border uppercase tracking-wider ${
                                        completed
                                          ? "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800"
                                          : isNext
                                          ? "bg-indigo-50 text-indigo-700 border-indigo-200 dark:bg-indigo-950/60 dark:text-indigo-300 dark:border-indigo-800"
                                          : "bg-slate-100 text-slate-600 border-slate-200 dark:bg-slate-800 dark:text-slate-400 dark:border-slate-700"
                                      }`}
                                    >
                                      {completed
                                        ? "Đã xong"
                                        : isNext
                                        ? "Tiếp theo"
                                        : isLocked
                                        ? "Đang khóa tuần tự"
                                        : `Bài ${lesIdx + 1}`}
                                    </span>
                                    {mins && (
                                      <span className="text-xs text-[var(--text-tertiary)] flex items-center gap-1">
                                        <Clock className="w-3 h-3" />
                                        {mins} phút
                                      </span>
                                    )}
                                    {(lesson.practiceActivityCount ?? 0) > 0 && (
                                      <span
                                        className={`text-[10px] font-semibold px-2 py-0.5 rounded-full border inline-flex items-center gap-1 ${
                                          (lesson.practicePassedCount ?? 0) >= (lesson.practiceActivityCount ?? 0)
                                            ? "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800"
                                            : "bg-indigo-50 text-indigo-700 border-indigo-200 dark:bg-indigo-950/60 dark:text-indigo-300 dark:border-indigo-800"
                                        }`}
                                      >
                                        <FlaskConical className="w-2.5 h-2.5" />
                                        {lesson.practicePassedCount ?? 0}/{lesson.practiceActivityCount} Lab
                                      </span>
                                    )}
                                  </div>

                                  <Link
                                    href={`/app/courses/${courseId}/learn?lesson=${lesson.id}`}
                                    title={lesson.title}
                                    className="block text-sm font-semibold text-[var(--text-primary)] hover:text-[#5E51E8] dark:hover:text-[#818CF8] transition line-clamp-2 leading-snug break-words"
                                  >
                                    {lesson.title}
                                  </Link>

                                  {isLocked && (
                                    <p className="text-[11px] text-amber-700 dark:text-amber-400 mt-1 flex items-center gap-1">
                                      <Lock className="w-3 h-3 shrink-0" />
                                      <span>
                                        Gợi ý: Hoàn thành bài trước đó hoặc tắt chế độ tuần tự để học ngay.
                                      </span>
                                    </p>
                                  )}

                                  {/* Activity Chips */}
                                  <div className="flex flex-wrap items-center gap-1.5 mt-2">
                                    {chips.map((chip, cIdx) => (
                                      <span
                                        key={cIdx}
                                        className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-semibold bg-[var(--surface-card)] border border-[var(--border-calm)] text-[var(--text-secondary)] shadow-2xs"
                                      >
                                        <Sparkles className="w-2.5 h-2.5 text-[#5E51E8] dark:text-[#A5B4FC]" />
                                        <span>{chip}</span>
                                      </span>
                                    ))}
                                  </div>
                                </div>

                                {/* Direct CTA button */}
                                <div className="shrink-0 self-start sm:self-center pt-1 sm:pt-0">
                                  {isLocked ? (
                                    <Button
                                      variant="outline"
                                      size="sm"
                                      onClick={handleToggleSequentialUnlock}
                                      className="whitespace-nowrap"
                                      leftIcon={<Unlock className="w-3.5 h-3.5" />}
                                    >
                                      <span>Mở khóa</span>
                                    </Button>
                                  ) : (
                                    <Link href={`/app/courses/${courseId}/learn?lesson=${lesson.id}`}>
                                      <Button
                                        variant={isNext ? "primary" : completed ? "secondary" : "outline"}
                                        size="sm"
                                        className="whitespace-nowrap"
                                        leftIcon={
                                          completed ? (
                                            <Check className="w-3.5 h-3.5" />
                                          ) : (
                                            <Play className="w-3.5 h-3.5 fill-current" />
                                          )
                                        }
                                      >
                                        <span>{completed ? "Ôn lại" : isNext ? "Học ngay" : "Bắt đầu"}</span>
                                      </Button>
                                    </Link>
                                  )}
                                </div>
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    )}
                  </Card>
                );
              })}
            </div>
          ) : (
            <EmptyState
              icon={BookOpen}
              title={
                allLessons.length > 0
                  ? "Không có bài giảng nào khớp bộ lọc hiện tại."
                  : "Khóa học chưa có nội dung bài giảng."
              }
            />
          )}
          {/* Course Reviews & Ratings Section */}
          <Card className="mt-8 p-5 sm:p-6 bg-[var(--surface-card)] space-y-5">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-[var(--border-calm)] pb-4">
              <div>
                <h3 className="text-base font-bold text-[var(--text-primary)] flex items-center gap-2">
                  <MessageSquare className="w-4 h-4 text-[#5E51E8] dark:text-[#A5B4FC]" />
                  <span>Đánh giá &amp; Nhận xét khóa học</span>
                </h3>
                <p className="text-xs text-[var(--text-secondary)] mt-0.5">
                  Chia sẻ trải nghiệm học tập của bạn về lộ trình và chất lượng bài giảng.
                </p>
              </div>
              <div className="flex items-center gap-2.5 px-3.5 py-2 rounded-xl bg-[var(--surface-elevated)] border border-[var(--border-calm)]">
                <div className="flex items-center gap-0.5">
                  {[1, 2, 3, 4, 5].map((star) => (
                    <Star
                      key={star}
                      className={`w-4 h-4 ${
                        star <= Math.round(averageRating || 5)
                          ? "fill-amber-400 text-amber-400"
                          : "text-slate-300 dark:text-slate-700"
                      }`}
                    />
                  ))}
                </div>
                <span className="text-sm font-bold text-[var(--text-primary)]">
                  {averageRating > 0 ? `${averageRating}/5` : "Mới"}
                </span>
                <span className="text-xs text-[var(--text-tertiary)]">
                  ({reviews.length} đánh giá)
                </span>
              </div>
            </div>

            {/* Submit Review Form */}
            <form onSubmit={handleSubmitReview} className="space-y-3">
              <div className="flex flex-wrap items-center gap-2">
                <span className="text-xs font-semibold text-[var(--text-secondary)]">
                  Chọn mức điểm:
                </span>
                <div className="flex items-center gap-1">
                  {[1, 2, 3, 4, 5].map((star) => (
                    <button
                      key={star}
                      type="button"
                      onClick={() => setRatingInput(star)}
                      aria-label={`Đánh giá ${star} sao`}
                      className="p-1 rounded hover:bg-[var(--surface-elevated)] transition cursor-pointer"
                    >
                      <Star
                        className={`w-5 h-5 ${
                          star <= ratingInput
                            ? "fill-amber-400 text-amber-400"
                            : "text-slate-300 dark:text-slate-700"
                        }`}
                      />
                    </button>
                  ))}
                </div>
                <span className="text-xs font-medium text-amber-600 dark:text-amber-400">
                  {ratingInput} sao
                </span>
              </div>

              <div className="flex flex-col sm:flex-row gap-2.5">
                <input
                  type="text"
                  value={commentInput}
                  onChange={(e) => setCommentInput(e.target.value)}
                  placeholder="Viết nhận xét của bạn về khóa học này..."
                  className="flex-1 h-9 px-3 rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8]"
                />
                <Button
                  type="submit"
                  variant="primary"
                  size="sm"
                  disabled={submittingReview}
                  leftIcon={<Send className="w-3.5 h-3.5" />}
                >
                  <span>{submittingReview ? "Đang gửi..." : "Gửi đánh giá"}</span>
                </Button>
              </div>
              {reviewNotice && (
                <p className="text-xs text-[#4F46E5] dark:text-[#A5B4FC] font-medium">
                  {reviewNotice}
                </p>
              )}
            </form>

            {/* Reviews List */}
            {reviews.length > 0 ? (
              <div className="divide-y divide-[var(--border-calm)]">
                {reviews.map((rev) => (
                  <div key={rev.id} className="py-3 first:pt-1 last:pb-0 space-y-1">
                    <div className="flex items-center justify-between gap-2">
                      <span className="text-xs font-bold text-[var(--text-primary)]">
                        {rev.userName}
                      </span>
                      <div className="flex items-center gap-0.5">
                        {[1, 2, 3, 4, 5].map((s) => (
                          <Star
                            key={s}
                            className={`w-3.5 h-3.5 ${
                              s <= rev.rating
                                ? "fill-amber-400 text-amber-400"
                                : "text-slate-300 dark:text-slate-700"
                            }`}
                          />
                        ))}
                      </div>
                    </div>
                    <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
                      {rev.comment}
                    </p>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-xs text-[var(--text-tertiary)] italic">
                Chưa có nhận xét nào. Hãy là học viên đầu tiên đánh giá khóa học này!
              </p>
            )}
          </Card>
        </div>

        {/* Right Insights & Study Pace Planner Column */}
        <aside className="lg:col-span-4 space-y-4 lg:sticky lg:top-6">
          <Card className="p-5 bg-[var(--surface-card)] space-y-4">
            <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-[#5E51E8] dark:text-[#A5B4FC]">
              <Clock className="w-4 h-4" />
              <span>Máy tính nhịp độ hoàn thành</span>
            </div>
            <div className="space-y-2">
              <div className="flex items-center justify-between text-xs">
                <span className="text-[var(--text-secondary)]">Mục tiêu mỗi ngày:</span>
                <span className="font-bold text-[var(--text-primary)]">{dailyLessonsPace} bài / ngày</span>
              </div>
              <input
                type="range"
                min={1}
                max={6}
                step={1}
                value={dailyLessonsPace}
                onChange={(e) => setDailyLessonsPace(Number(e.target.value))}
                aria-label="Số bài học mục tiêu mỗi ngày"
                className="w-full accent-[#5E51E8] cursor-pointer"
              />
              <div className="rounded-lg bg-[var(--surface-elevated)] border border-[var(--border-calm)] p-3 text-xs space-y-1">
                <p className="text-[var(--text-secondary)]">
                  Dự kiến hoàn thành phần còn lại ({remainingLessons} bài):
                </p>
                <p className="text-base font-bold text-[#5E51E8] dark:text-[#A5B4FC]">
                  {estimatedDaysToFinish === 0 ? "Đã hoàn thành!" : `${estimatedDaysToFinish} ngày`}
                </p>
              </div>
            </div>
          </Card>

          {/* Weak Chapters Review Recommendation Card */}
          {weakChaptersList.length > 0 && (
            <Card className="p-5 bg-[var(--surface-card)] border-amber-500/30 space-y-3">
              <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-amber-600 dark:text-amber-400">
                <Target className="w-4 h-4" />
                <span>Chương cần ôn tập &amp; củng cố</span>
              </div>
              <div className="space-y-2.5">
                {weakChaptersList.map((wc) => (
                  <div
                    key={wc.chapterId}
                    className="rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] p-3 space-y-2"
                  >
                    <div className="flex items-center justify-between gap-2">
                      <p className="text-xs font-bold text-[var(--text-primary)] line-clamp-1">
                        {wc.title}
                      </p>
                      <span className="text-[11px] font-semibold text-amber-600 dark:text-amber-400 shrink-0">
                        {wc.completionPercent}%
                      </span>
                    </div>
                    {wc.reason && (
                      <p className="text-[11px] text-[var(--text-secondary)]">{wc.reason}</p>
                    )}
                    <div className="flex items-center gap-2 pt-0.5">
                      <Link
                        href={
                          wc.nextLessonId
                            ? `/app/courses/${courseId}/learn?lesson=${wc.nextLessonId}`
                            : continueLearnHref
                        }
                        className="flex-1 inline-flex items-center justify-center gap-1 h-7 px-2.5 rounded-md bg-[#5E51E8] text-white text-[11px] font-semibold hover:bg-[#4F46E5] transition"
                      >
                        <Play className="w-3 h-3" />
                        <span>Học tiếp</span>
                      </Link>
                      <Link
                        href={`/app/review?courseId=${courseId}`}
                        className="flex-1 inline-flex items-center justify-center gap-1 h-7 px-2.5 rounded-md border border-[var(--border-calm)] bg-[var(--surface-card)] text-[11px] font-semibold text-[var(--text-primary)] hover:border-[#5E51E8] transition"
                      >
                        <Brain className="w-3 h-3 text-[#5E51E8]" />
                        <span>Ôn thẻ FSRS</span>
                      </Link>
                    </div>
                  </div>
                ))}
              </div>
            </Card>
          )}

          <Card className="p-5 bg-[var(--surface-card)] space-y-3">
            <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-[var(--text-tertiary)]">
              <Sparkles className="w-4 h-4 text-[#5E51E8]" />
              <span>Công cụ thực hành tích hợp</span>
            </div>
            <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
              Mỗi bài học đều đi kèm Phòng Thực Hành tương tác ngay bên dưới bài đọc: luyện nghe nói IPA, code/debug, biểu đồ tài chính, roleplay tâm lý, Quiz thật và Flashcards FSRS v5.
            </p>
            <div className="flex flex-col gap-2 pt-1">
              <Link
                href={continueLearnHref}
                className="inline-flex items-center justify-center gap-2 h-9 px-3 rounded-lg bg-[#5E51E8] hover:bg-[#4F46E5] text-white text-xs font-semibold transition"
              >
                <Play className="w-3.5 h-3.5" />
                <span>Mở bài học &amp; Phòng thực hành</span>
              </Link>
              <Link
                href={`/app/review?courseId=${courseId}`}
                className="inline-flex items-center justify-center gap-2 h-9 px-3 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] hover:border-[#5E51E8] text-xs font-semibold text-[var(--text-primary)] transition"
              >
                <Brain className="w-3.5 h-3.5 text-[#5E51E8]" />
                <span>Ôn bộ thẻ nhớ khóa này</span>
              </Link>
              <Link
                href={`/app/courses/${courseId}/certificate`}
                className="inline-flex items-center justify-center gap-2 h-9 px-3 rounded-lg border border-amber-500/30 bg-amber-500/10 hover:bg-amber-500/20 text-xs font-semibold text-amber-700 dark:text-amber-300 transition"
              >
                <Award className="w-3.5 h-3.5" />
                <span>Xem Tổng kết &amp; Chứng nhận PDF</span>
              </Link>
            </div>
          </Card>
        </aside>
      </div>

      {/* Mobile Sticky Bottom CTA Bar */}
      <div className="fixed bottom-16 inset-x-0 z-30 md:hidden bg-[var(--surface-card)]/95 backdrop-blur-md border-t border-[var(--border-calm)] px-4 py-2.5 shadow-lg flex items-center justify-between gap-3">
        <div className="min-w-0 flex-1">
          <p className="text-[10px] font-bold uppercase tracking-wider text-[#5E51E8] dark:text-[#A5B4FC]">
            Tiến độ {course.progressPercent}% ({course.completedLessons}/{course.totalLessons} bài)
          </p>
          <p className="text-xs font-semibold text-[var(--text-primary)] truncate">
            {resumeState?.lastLessonTitle || resumeLesson?.title || course.title}
          </p>
        </div>
        <Link href={continueLearnHref} className="shrink-0">
          <Button
            variant="primary"
            size="sm"
            leftIcon={<Play className="w-3.5 h-3.5 fill-current" />}
          >
            <span>{course.progressPercent > 0 ? "Tiếp tục học" : "Bắt đầu học"}</span>
          </Button>
        </Link>
      </div>
    </div>
  );
}

