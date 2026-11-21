"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import {
  AlertCircle,
  BookOpen,
  Brain,
  Check,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  ChevronsDownUp,
  ChevronsUpDown,
  Circle,
  ClipboardCheck,
  Clock,
  Layers,
  Pencil,
  Play,
  RefreshCw,
  Search,
  Sparkles,
  Target,
  X,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached } from "@/lib/api-client";
import { Button, Card, Badge, ProgressBar, EmptyState } from "@/components/ui";
import {
  classifyCourseDomain,
  extractCourseSkillTags,
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
}

interface CourseChapter {
  id: string;
  chapterId?: string;
  title: string;
  sortOrder?: number;
  lessons: CourseLesson[];
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
  flashcardCount?: number;
  quizCount?: number;
  quizId?: string | null;
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

      const [courseResp, dashData] = await Promise.all([
        fetch(`/api/v1/courses/${courseId}`, { headers }).catch(() => null),
        fetchDashboardCached({ headers }).catch(() => null),
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
        courseData = {
          ...raw,
          id: String(raw.courseId || raw.id || courseId),
          totalLessons: total,
          completedLessons: completed,
          completedLessonIds: completedIds,
          progressPercent: progress,
          chapters: chaps,
        };
      } else if (dashData) {
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
            quizCount: Number(found.quizCount ?? 0),
            quizId: (found.quizId as string | null | undefined) ?? null,
            chapters: chaps,
          };
        }
      } else if (courseResp && !courseResp.ok) {
        setFetchError(`Không tải được thông tin khóa học (HTTP ${courseResp.status}).`);
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
  }, [courseId, activeUserHeaderId]);

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

  const totalMinutes = useMemo(
    () =>
      allLessons.reduce(
        (sum, l) => sum + Number(l.durationMinutes ?? l.estimatedMinutes ?? 10),
        0
      ),
    [allLessons]
  );

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

  const continueLearnHref = nextLesson
    ? `/app/courses/${courseId}/learn?lesson=${nextLesson.id}`
    : `/app/courses/${courseId}/learn`;

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10">
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
                  {totalMinutes} phút
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
                    className="text-border"
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
                <p className="text-[var(--text-tertiary)]">
                  {remainingLessons === 0
                    ? "Đã hoàn tất toàn bộ lộ trình!"
                    : `Còn ${remainingLessons} bài để về đích`}
                </p>
              </div>
            </div>
          </div>

          {/* Overall Progress Bar */}
          <div className="mb-5">
            <ProgressBar
              value={course.progressPercent}
              color={course.progressPercent === 100 ? "success" : "brand"}
              size="md"
            />
          </div>

          {/* Next Lesson Highlight Banner */}
          {nextLesson && (
            <div className="rounded-[10px] border border-[var(--border-calm)] bg-[var(--surface-elevated)] p-4 mb-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
              <div className="min-w-0">
                <p className="text-[11px] font-semibold uppercase tracking-wider text-[var(--text-tertiary)]">
                  {course.progressPercent === 100 ? "Ôn lại lộ trình" : "Bài học tiếp theo"}
                </p>
                <p className="text-sm font-semibold text-[var(--text-primary)] line-clamp-2 mt-0.5">
                  {nextLesson.title}
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
                  <span>{course.progressPercent > 0 ? "Tiếp tục học" : "Bắt đầu học"}</span>
                </Button>
              </Link>
            </div>
          )}

          {/* Secondary Actions */}
          <div className="flex flex-wrap items-center gap-2">
            {!nextLesson && (
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
            {course.quizCount === 0 ? (
              <button
                type="button"
                disabled
                data-testid="course-quiz-btn"
                title="Khóa học chưa có bài kiểm tra"
                className="inline-flex items-center gap-1.5 h-9 px-3 rounded-md border border-border bg-bg-secondary text-xs font-medium text-text-tertiary opacity-60 cursor-not-allowed"
              >
                <ClipboardCheck className="w-4 h-4 text-text-tertiary" />
                <span>Chưa có bài kiểm tra</span>
              </button>
            ) : (
              <Link
                href={`/app/courses/${courseId}/quiz`}
                data-testid="course-quiz-btn"
              >
                <Button
                  variant="secondary"
                  leftIcon={<ClipboardCheck className="w-4 h-4 text-[var(--text-secondary)]" />}
                >
                  <span>Làm bài kiểm tra{course.quizCount ? ` (${course.quizCount})` : ""}</span>
                </Button>
              </Link>
            )}
            <Link
              href={`/app/review?courseId=${courseId}`}
            >
              <Button
                variant="secondary"
                leftIcon={<Brain className="w-4 h-4 text-[var(--text-secondary)]" />}
              >
                <span>Ôn thẻ ghi nhớ{course.flashcardCount ? ` (${course.flashcardCount})` : ""}</span>
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

      {/* Visual Milestone Stepper */}
      {course.chapters.length > 0 && (
        <section className="mb-6">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-sm font-semibold text-[var(--text-primary)] flex items-center gap-1.5">
              <Target className="w-4 h-4 text-[#5E51E8] dark:text-[#A5B4FC]" />
              <span>Bản đồ các chặng học tập ({course.chapters.length} chặng)</span>
            </h2>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
            {course.chapters.slice(0, 8).map((ch, idx) => {
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
                  className={`text-left rounded-xl border p-3.5 transition cursor-pointer flex flex-col justify-between gap-2 ${
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
            {course.chapters.length > 0 && (
              <button
                type="button"
                onClick={toggleAllChapters}
                className="inline-flex items-center gap-1.5 text-xs font-medium text-[var(--text-secondary)] hover:text-[var(--text-primary)] transition cursor-pointer self-start sm:self-auto"
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
                            const mins = lesson.durationMinutes ?? lesson.estimatedMinutes ?? 15;
                            const chips = getLessonPracticeChips(domain.id, lesson.title);

                            return (
                              <div
                                key={lesson.id}
                                className="relative group flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-3.5 rounded-xl border border-[var(--border-calm)] hover:border-[#5E51E8]/50 bg-[var(--surface-elevated)]/50 hover:bg-[var(--surface-elevated)] transition shadow-2xs"
                              >
                                {/* Timeline Node Icon Connector */}
                                <div
                                  className={`absolute -left-6 sm:-left-8 top-4 -translate-x-1/2 w-6 h-6 rounded-full border-2 flex items-center justify-center text-[10px] font-bold shadow-xs transition ${
                                    completed
                                      ? "bg-emerald-500 border-emerald-500 text-white"
                                      : isNext
                                      ? "bg-[#5E51E8] border-[#5E51E8] text-white ring-4 ring-[#5E51E8]/20 animate-pulse"
                                      : "bg-[var(--surface-card)] border-[var(--border-calm)] text-[var(--text-tertiary)]"
                                  }`}
                                >
                                  {completed ? (
                                    <CheckCircle2 className="w-3.5 h-3.5" />
                                  ) : isNext ? (
                                    <Play className="w-2.5 h-2.5 fill-current ml-0.5" />
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
                                      {completed ? "Đã xong" : isNext ? "Tiếp theo" : `Bài ${lesIdx + 1}`}
                                    </span>
                                    {mins && (
                                      <span className="text-xs text-[var(--text-tertiary)] flex items-center gap-1">
                                        <Clock className="w-3 h-3" />
                                        {mins} phút
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
            </div>
          </Card>
        </aside>
      </div>
    </div>
  );
}

