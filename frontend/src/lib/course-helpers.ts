export type CourseDomainId =
  | "all"
  | "technology"
  | "law"
  | "science"
  | "language"
  | "business"
  | "other";

export interface CourseDomainMeta {
  id: Exclude<CourseDomainId, "all">;
  label: string;
}

export const COURSE_DOMAINS: CourseDomainMeta[] = [
  { id: "technology", label: "Công nghệ & Lập trình" },
  { id: "law", label: "Pháp luật & Xã hội" },
  { id: "science", label: "Khoa học & Toán" },
  { id: "language", label: "Ngoại ngữ" },
  { id: "business", label: "Kinh tế & Quản trị" },
  { id: "other", label: "Chủ đề khác" },
];

/**
 * 10 chuẩn curated courses giữ lại trong hệ thống Studxy (WP-C1, C2, C3).
 */
export const CURATED_COURSE_SLUGS = [
  "doc-course-studxy-fullstack-bootcamp",
  "document-course-ielts",
  "document-course-investing",
  "document-course-personalfinance",
  "document-course-lifeskills",
  "document-course-psychology",
  "tieng-anh-giao-tiep-thuc-chien-a2-b1",
  "tieng-nhat-so-cap-n5",
  "tieng-trung-so-cap-hsk1",
  "he-dieu-hanh-bo-nho-ao",
] as const;

/**
 * 5 slugs đã retired / gỡ bỏ khỏi thứ tự ưu tiên (Backend & Frontend Pillar 1).
 */
export const RETIRED_COURSE_SLUGS = [
  "document-course-dotnet-react-roadmap",
  "document-course-fullstack-csharp-react-refresh",
  "lap-trinh-javascript-tu-duy-giai-quyet-van-de",
  "dau-tu-quan-tri-danh-muc-thuc-chien",
  "tam-ly-hoc-ung-dung-khoa-hoc-tri-nho",
] as const;

export interface CourseSummaryChapterLesson {
  id: string;
  title: string;
  slug?: string | null;
  estimatedMinutes?: number | null;
  durationMinutes?: number | null;
  isCompleted?: boolean;
  isBookmarked?: boolean;
  needsReview?: boolean;
  practiceActivityCount?: number;
  practicePassedCount?: number;
}

export interface CourseSummaryChapter {
  id: string;
  title: string;
  sortOrder?: number;
  lessons?: CourseSummaryChapterLesson[];
}

export interface CourseLike {
  id?: string;
  courseId?: string;
  slug?: string | null;
  title?: string;
  description?: string | null;
  visibility?: string | null;
  isPublished?: boolean;
  isPinned?: boolean;
  isArchived?: boolean;
  dailyLessonGoal?: number;
  targetCompletionDate?: string | null;
  isSystemCourse?: boolean;
  canDelete?: boolean;
  category?: string | null;
  level?: string | null; // "foundation" | "intermediate" | "advanced" | ...
  learningOutcomes?: string[];
  prerequisites?: string[];
  targetAudience?: string | null;
  labLabel?: string | null;
  hasAudio?: boolean;
  bookmarkedLessonCount?: number;
  needsReviewLessonCount?: number;
  chapterCount?: number;
  lessonCount?: number;
  totalLessons?: number;
  completedLessons?: number;
  progressPercent?: number;
  progress?: number;
  totalEstimatedMinutes?: number;
  flashcardCount?: number;
  dueFlashcardCount?: number;
  masteredFlashcardCount?: number;
  practiceTotalActivities?: number;
  practicePassedActivities?: number;
  practiceAverageScore?: number;
  quizCount?: number;
  firstLessonId?: string | null;
  chapters?: Array<{
    id?: string;
    title?: string;
    sortOrder?: number;
    lessons?: unknown[];
  }>;
  createdAt?: string | null;
  updatedAt?: string | null;
  sourceFolder?: string | null;
  sourceDocumentId?: string | null;
  totalPages?: number | null;
}

export interface CourseSummary extends CourseLike {
  id: string;
  title: string;
  chapters?: CourseSummaryChapter[];
}

export function normalizeVi(text: string): string {
  return (text || "")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase()
    .replace(/\s+/g, " ")
    .trim();
}

export function getCourseId(course: CourseLike): string {
  return String(course.courseId || course.id || "");
}

export function getCourseProgress(course: CourseLike): number {
  const raw = course.progressPercent ?? course.progress ?? 0;
  const num = Number(raw);
  if (!Number.isFinite(num)) return 0;
  return Math.max(0, Math.min(100, Math.round(num)));
}

export function getCourseLessonCount(course: CourseLike): number {
  if (typeof course.totalLessons === "number" && Number.isFinite(course.totalLessons)) {
    return course.totalLessons;
  }
  if (typeof course.lessonCount === "number" && Number.isFinite(course.lessonCount)) {
    return course.lessonCount;
  }
  if (Array.isArray(course.chapters)) {
    return course.chapters.reduce(
      (sum, ch) => sum + (Array.isArray(ch.lessons) ? ch.lessons.length : 0),
      0
    );
  }
  return 0;
}

export function getCourseChapterCount(course: CourseLike): number {
  if (typeof course.chapterCount === "number" && Number.isFinite(course.chapterCount)) {
    return course.chapterCount;
  }
  if (Array.isArray(course.chapters)) {
    return course.chapters.length;
  }
  return 0;
}

export function classifyCourseDomain(course: CourseLike): CourseDomainMeta {
  const rawCombined = `${course.category || ""} ${course.title || ""} ${course.description || ""}`;
  const combined = normalizeVi(rawCombined);

  // 1. Language - prioritize language so "Tiếng Anh cho Dân IT", "Tiếng Nhật N5", "Tiếng Trung HSK1" match language
  if (
    /(tieng anh|english|ielts|toeic|toefl|ngoai ngu|tieng nhat|japanese|jlpt|\bn5\b|\bn4\b|\bn3\b|hiragana|katakana|kanji|tieng trung|chinese|\bhsk\d?\b|pinyin|hanzi|tieng han|korean|ngu phap|tu vung|vocabulary|phat am|listening|reading|writing|speaking|grammar|pronunciation)/.test(
      combined
    )
  ) {
    return { id: "language", label: "Ngoại ngữ" };
  }

  // 2. Law & Social
  if (
    /(phap luat|\bluat\b|hien phap|hinh su|dan su|hanh chinh|chinh tri|triet hoc|lich su|giao trinh phap|nha nuoc)/.test(
      combined
    )
  ) {
    return { id: "law", label: "Pháp luật & Xã hội" };
  }

  // 3. Technology & Programming - ensure short acronyms have \b word boundaries so "giai", "bai", "lai" never match "ai"
  if (
    /(lap trinh|coding|\bcode\b|react|next|typescript|javascript|python|\bjava\b|\bc#\b|dotnet|\.net|\bsql\b|database|backend|frontend|\bapi\b|\bai\b|machine learning|deep learning|\bml\b|\bllm\b|devops|cloud|docker|kubernetes|software|computer|may tinh|thuat toan|cau truc du lieu|mang may tinh|he dieu hanh|microservice|microservices|\bddd\b|event sourcing|phan tan|he thong)/.test(
      combined
    )
  ) {
    return { id: "technology", label: "Công nghệ & Lập trình" };
  }

  // 4. Science & Math - ensure \b word boundaries so "khoa hoc" (course) doesn't match "hoa hoc" (chemistry) or "khoa hoc" (science)
  const isScience =
    !/(tam ly|ky nang song|giao tiep)/.test(combined) &&
    (/(\btoan hoc\b|\btoan cao cap\b|\btoan roi rac\b|\bgiai tich\b|\bdai so\b|\bmath\b|\bvat ly\b|\bphysics\b|\bhoa hoc\b|\bchemistry\b|\bsinh hoc\b|\bbiology\b|\bxac suat\b|\bthong ke\b|\bstatistics\b|\bkhoa hoc tu nhien\b)/.test(
      combined
    ) ||
      (/\bkhoa học\b/i.test(rawCombined) && !/\b(khóa học|khoá học)\b/i.test(rawCombined)));

  if (isScience) {
    return { id: "science", label: "Khoa học & Toán" };
  }

  // 5. Business & Economics
  if (
    /(kinh te|quan tri|marketing|tai chinh|ke toan|business|management|finance|economics|startup|product|ban hang|\bsales\b)/.test(
      combined
    )
  ) {
    return { id: "business", label: "Kinh tế & Quản trị" };
  }

  return { id: "other", label: "Chủ đề khác" };
}

export function isTestOrJunkCourse(course: CourseLike): boolean {
  const text = normalizeVi(`${course.title || ""} ${course.description || ""} ${course.courseId || course.id || ""}`);
  return /(small-3p|marker-12p|large-305p|ietf-rfc7995|mixed-scanned-text|double click test|test doc|dummy|fixture)/.test(
    text
  );
}

export function isRetiredCourse(course: CourseLike): boolean {
  const slug = (course as { slug?: string | null }).slug;
  if (slug && RETIRED_COURSE_SLUGS.includes(slug as any)) return true;
  const rawCombined = `${course.title || ""} ${course.courseId || course.id || ""}`;
  const norm = normalizeVi(rawCombined);
  return (
    norm.includes("lap trinh javascript tu duy giai quyet van de") ||
    norm.includes("dau tu quan tri danh muc thuc chien") ||
    norm.includes("tam ly hoc ung dung khoa hoc tri nho") ||
    norm.includes("dotnet react roadmap") ||
    norm.includes("fullstack csharp react refresh")
  );
}

export function isCurriculumStandardCourse(course: CourseLike): boolean {
  if (isRetiredCourse(course)) return false;
  const slug = (course as { slug?: string | null }).slug;
  if (slug && CURATED_COURSE_SLUGS.includes(slug as any)) return true;
  const rawCombined = `${course.title || ""} ${slug || ""} ${course.description || ""}`;
  const norm = normalizeVi(rawCombined);
  return (
    /document-course-/.test(rawCombined) ||
    /(ielts|tieng anh|english|tieng nhat|japanese|jlpt|\bn5\b|tieng trung|chinese|\bhsk\d?\b|\.net|c#|csharp|fullstack|full-stack|dau tu|tai chinh|ky nang song|tam ly hoc|he dieu hanh|hoc may|phap luat)/.test(
      norm
    )
  );
}

/**
 * Helper chuẩn hóa CourseSummary từ CourseLike với đầy đủ fallback defaults.
 */
export function normalizeCourseSummary(raw: CourseLike): CourseSummary {
  const id = getCourseId(raw);
  const domain = classifyCourseDomain(raw);
  const lessonCount = getCourseLessonCount(raw);
  const chapterCount = getCourseChapterCount(raw);
  const progress = getCourseProgress(raw);
  const totalMinutes = getCourseTotalMinutes(raw);
  const practiceCount = Number(raw.practiceTotalActivities ?? 0);

  return {
    id,
    courseId: id,
    title: raw.title || "Khóa học",
    slug: raw.slug ?? null,
    description: raw.description ?? "",
    visibility: raw.visibility ?? "public",
    isPublished: raw.isPublished ?? true,
    isPinned: Boolean(raw.isPinned),
    isArchived: Boolean(raw.isArchived),
    dailyLessonGoal: typeof raw.dailyLessonGoal === "number" && raw.dailyLessonGoal > 0 ? raw.dailyLessonGoal : 3,
    targetCompletionDate: raw.targetCompletionDate ?? null,
    isSystemCourse: Boolean(raw.isSystemCourse),
    canDelete: Boolean(raw.canDelete),
    category: raw.category || domain.label,
    level: raw.level || "foundation",
    learningOutcomes: Array.isArray(raw.learningOutcomes) ? raw.learningOutcomes : [],
    prerequisites: Array.isArray(raw.prerequisites) ? raw.prerequisites : [],
    targetAudience: raw.targetAudience ?? "",
    labLabel: raw.labLabel || (practiceCount > 0 ? "Phòng Lab" : ""),
    hasAudio: Boolean(raw.hasAudio),
    bookmarkedLessonCount: Number(raw.bookmarkedLessonCount ?? 0),
    needsReviewLessonCount: Number(raw.needsReviewLessonCount ?? 0),
    chapterCount,
    lessonCount,
    totalLessons: lessonCount,
    completedLessons: Number(raw.completedLessons ?? 0),
    progressPercent: progress,
    progress,
    totalEstimatedMinutes: totalMinutes,
    flashcardCount: Number(raw.flashcardCount ?? 0),
    dueFlashcardCount: Number(raw.dueFlashcardCount ?? 0),
    masteredFlashcardCount: Number(raw.masteredFlashcardCount ?? 0),
    practiceTotalActivities: practiceCount,
    practicePassedActivities: Number(raw.practicePassedActivities ?? 0),
    practiceAverageScore: typeof raw.practiceAverageScore === "number" ? raw.practiceAverageScore : 0,
    quizCount: Number(raw.quizCount ?? 0),
    firstLessonId: raw.firstLessonId ?? null,
    chapters: Array.isArray(raw.chapters) ? (raw.chapters as CourseSummaryChapter[]) : [],
    createdAt: raw.createdAt ?? null,
    updatedAt: raw.updatedAt ?? null,
    sourceFolder: raw.sourceFolder ?? null,
    sourceDocumentId: raw.sourceDocumentId ?? null,
    totalPages: typeof raw.totalPages === "number" ? raw.totalPages : null,
  };
}

export interface CourseDuplicateGroup {
  normalizedTitle: string;
  title: string;
  courses: CourseSummary[];
  suggestedKeepId: string;
  duplicateIds: string[];
}

export interface DuplicateDetectionResult {
  titleDuplicateGroups: CourseDuplicateGroup[];
  emptyCourses: CourseSummary[];
  allRemovableCourseIds: string[];
  totalRemovableCount: number;
}

/**
 * Detects & groups personal duplicate courses and empty personal drafts.
 * Strictly operates ONLY on personal courses that user can delete: !c.isSystemCourse && c.canDelete.
 */
export function detectPersonalDuplicates(courses: CourseLike[]): DuplicateDetectionResult {
  const normalizedList = courses.map(normalizeCourseSummary);
  // ONLY personal deletable courses
  const personalDeletable = normalizedList.filter((c) => !c.isSystemCourse && c.canDelete);

  // 1. Empty courses: 0 lessons / old drafts
  const emptyCourses = personalDeletable.filter((c) => getCourseLessonCount(c) === 0);

  // 2. Group personal courses with duplicate normalized titles
  const titleGroupsMap = new Map<string, CourseSummary[]>();
  for (const c of personalDeletable) {
    const norm = normalizeVi(c.title || "");
    if (!norm) continue;
    const existing = titleGroupsMap.get(norm) ?? [];
    existing.push(c);
    titleGroupsMap.set(norm, existing);
  }

  const titleDuplicateGroups: CourseDuplicateGroup[] = [];
  const duplicateIdsSet = new Set<string>();

  for (const [normTitle, group] of titleGroupsMap.entries()) {
    if (group.length > 1) {
      // Suggest keeping best candidate: highest progress, most lessons, newest updatedAt
      const sorted = [...group].sort((a, b) => {
        const progA = getCourseProgress(a);
        const progB = getCourseProgress(b);
        if (progB !== progA) return progB - progA;

        const lesA = getCourseLessonCount(a);
        const lesB = getCourseLessonCount(b);
        if (lesB !== lesA) return lesB - lesA;

        const tsA = Date.parse(a.updatedAt || a.createdAt || "") || 0;
        const tsB = Date.parse(b.updatedAt || b.createdAt || "") || 0;
        return tsB - tsA;
      });

      const keep = sorted[0];
      const dupes = sorted.slice(1);
      const dupIds = dupes.map((d) => d.id);
      dupIds.forEach((id) => duplicateIdsSet.add(id));

      titleDuplicateGroups.push({
        normalizedTitle: normTitle,
        title: keep.title || normTitle,
        courses: sorted,
        suggestedKeepId: keep.id,
        duplicateIds: dupIds,
      });
    }
  }

  const allRemovable = new Set<string>();
  duplicateIdsSet.forEach((id) => allRemovable.add(id));
  emptyCourses.forEach((c) => allRemovable.add(c.id));

  return {
    titleDuplicateGroups,
    emptyCourses,
    allRemovableCourseIds: Array.from(allRemovable),
    totalRemovableCount: allRemovable.size,
  };
}

export function sortCoursesSmartly<T extends CourseLike>(courses: T[]): T[] {
  return [...courses].sort((a, b) => {
    // 1. Junk courses to bottom
    const aJunk = isTestOrJunkCourse(a);
    const bJunk = isTestOrJunkCourse(b);
    if (aJunk !== bJunk) return aJunk ? 1 : -1;

    // 2. Retired courses to bottom
    const aRetired = isRetiredCourse(a);
    const bRetired = isRetiredCourse(b);
    if (aRetired !== bRetired) return aRetired ? 1 : -1;

    // 3. Archived courses go after active courses
    const aArchived = Boolean(a.isArchived);
    const bArchived = Boolean(b.isArchived);
    if (aArchived !== bArchived) return aArchived ? 1 : -1;

    // 4. Pinned non-archived courses go first
    const aPinned = Boolean(a.isPinned && !aArchived);
    const bPinned = Boolean(b.isPinned && !bArchived);
    if (aPinned !== bPinned) return aPinned ? -1 : 1;

    // 5. Standard curriculum priority
    const aStd = isCurriculumStandardCourse(a);
    const bStd = isCurriculumStandardCourse(b);
    if (aStd !== bStd) return aStd ? -1 : 1;

    // 6. In-progress courses
    const aProg = getCourseProgress(a);
    const bProg = getCourseProgress(b);
    const aInProg = aProg > 0 && aProg < 100;
    const bInProg = bProg > 0 && bProg < 100;
    if (aInProg !== bInProg) return aInProg ? -1 : 1;

    // 7. Curated priority ordering if both in curated list
    const aSlug = (a as { slug?: string }).slug || "";
    const bSlug = (b as { slug?: string }).slug || "";
    const aCuratedIdx = CURATED_COURSE_SLUGS.indexOf(aSlug as any);
    const bCuratedIdx = CURATED_COURSE_SLUGS.indexOf(bSlug as any);
    if (aCuratedIdx !== -1 && bCuratedIdx !== -1 && aCuratedIdx !== bCuratedIdx) {
      return aCuratedIdx - bCuratedIdx;
    }

    // 8. Lesson count
    const aLessons = getCourseLessonCount(a);
    const bLessons = getCourseLessonCount(b);
    if (aLessons !== bLessons) return bLessons - aLessons;

    // 9. Progress
    if (aProg !== bProg) return bProg - aProg;

    return 0;
  });
}

/**
 * Strips repeated chapter/phase/part numbering prefixes (e.g. "Chương 1: Chương 3: ...")
 * so UI never renders duplicated prefixes.
 */
export function stripChapterPrefix(title: string): string {
  if (!title) return "";
  let result = title.trim();
  const prefixRegex =
    /^(?:Chương|Chuong|Chapter|Phần|Phan|Chặng|Chang|Phase|Giai\s*đoạn|Module|Unit|Part|Section)\s*\d+\s*[:.\-–—]\s*/i;
  let guard = 0;
  while (prefixRegex.test(result) && guard < 5) {
    const stripped = result.replace(prefixRegex, "").trim();
    if (!stripped) break;
    result = stripped;
    guard++;
  }
  return result;
}

export interface CourseVisualTheme {
  domainId: Exclude<CourseDomainId, "all">;
  domainLabel: string;
  accentBarClass: string;
  badgeBgClass: string;
  iconBgClass: string;
  iconTextClass: string;
  ringColor: string;
  levelLabel: string;
  estimatedHoursLabel: string;
}

export function getCourseTotalMinutes(course: CourseLike): number {
  if (
    typeof course.totalEstimatedMinutes === "number" &&
    Number.isFinite(course.totalEstimatedMinutes) &&
    course.totalEstimatedMinutes > 0
  ) {
    return Math.round(course.totalEstimatedMinutes);
  }

  if (Array.isArray(course.chapters) && course.chapters.length > 0) {
    let chapterMinutes = 0;
    for (const ch of course.chapters) {
      if (Array.isArray(ch.lessons)) {
        for (const rawLesson of ch.lessons) {
          if (rawLesson && typeof rawLesson === "object") {
            const lessonObj = rawLesson as {
              estimatedMinutes?: unknown;
              durationMinutes?: unknown;
            };
            const mins = Number(lessonObj.estimatedMinutes ?? lessonObj.durationMinutes ?? 0);
            if (Number.isFinite(mins) && mins > 0) {
              chapterMinutes += mins;
            }
          }
        }
      }
    }
    if (chapterMinutes > 0) {
      return Math.round(chapterMinutes);
    }
  }

  const lessons = Math.max(1, getCourseLessonCount(course));
  return lessons * 25;
}

export function getCourseVisualTheme(course: CourseLike): CourseVisualTheme {
  const domain = classifyCourseDomain(course);
  const mins = getCourseTotalMinutes(course);
  const estimatedHoursLabel =
    mins < 60 ? `${mins} phút học` : `~${Math.max(1, Math.round(mins / 60))} giờ học`;
  const norm = normalizeVi(`${course.title || ""} ${course.description || ""}`);

  let levelLabel = "Lộ trình thực chiến";
  if (course.level === "foundation" || /(co ban|nhap mon|nen tang|phase 0|tu con so 0|dai cuong|basics|so cap|\bn5\b|\bhsk1\b)/.test(norm)) {
    levelLabel = "Nền tảng → Vững chắc";
  } else if (course.level === "advanced" || /(mastery|chuyen sau|nang cao|microservices|ddd|48 tuan|28 tuan|fullstack|full-stack)/.test(norm)) {
    levelLabel = "Chuyên sâu toàn diện";
  } else if (course.level === "intermediate") {
    levelLabel = "Trung cấp thực chiến";
  }

  switch (domain.id) {
    case "technology":
      return {
        domainId: "technology",
        domainLabel: domain.label,
        accentBarClass: "from-indigo-500 via-violet-500 to-sky-500",
        badgeBgClass: "bg-indigo-50 text-indigo-700 border-indigo-200 dark:bg-indigo-950/60 dark:text-indigo-300 dark:border-indigo-800",
        iconBgClass: "bg-indigo-500/10 border-indigo-500/20",
        iconTextClass: "text-indigo-600 dark:text-indigo-400",
        ringColor: "#6366F1",
        levelLabel,
        estimatedHoursLabel,
      };
    case "language":
      return {
        domainId: "language",
        domainLabel: domain.label,
        accentBarClass: "from-sky-500 via-cyan-500 to-teal-500",
        badgeBgClass: "bg-sky-50 text-sky-700 border-sky-200 dark:bg-sky-950/60 dark:text-sky-300 dark:border-sky-800",
        iconBgClass: "bg-sky-500/10 border-sky-500/20",
        iconTextClass: "text-sky-600 dark:text-sky-400",
        ringColor: "#0EA5E9",
        levelLabel,
        estimatedHoursLabel,
      };
    case "business":
      return {
        domainId: "business",
        domainLabel: domain.label,
        accentBarClass: "from-emerald-500 via-teal-500 to-lime-500",
        badgeBgClass: "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800",
        iconBgClass: "bg-emerald-500/10 border-emerald-500/20",
        iconTextClass: "text-emerald-600 dark:text-emerald-400",
        ringColor: "#10B981",
        levelLabel,
        estimatedHoursLabel,
      };
    case "law":
      return {
        domainId: "law",
        domainLabel: domain.label,
        accentBarClass: "from-amber-500 via-orange-500 to-rose-500",
        badgeBgClass: "bg-amber-50 text-amber-800 border-amber-200 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-800",
        iconBgClass: "bg-amber-500/10 border-amber-500/20",
        iconTextClass: "text-amber-600 dark:text-amber-400",
        ringColor: "#F59E0B",
        levelLabel,
        estimatedHoursLabel,
      };
    case "science":
      return {
        domainId: "science",
        domainLabel: domain.label,
        accentBarClass: "from-purple-500 via-fuchsia-500 to-pink-500",
        badgeBgClass: "bg-purple-50 text-purple-700 border-purple-200 dark:bg-purple-950/60 dark:text-purple-300 dark:border-purple-800",
        iconBgClass: "bg-purple-500/10 border-purple-500/20",
        iconTextClass: "text-purple-600 dark:text-purple-400",
        ringColor: "#A855F7",
        levelLabel,
        estimatedHoursLabel,
      };
    default:
      return {
        domainId: "other",
        domainLabel: /(tam ly|ky nang song|giao tiep)/.test(norm) ? "Tâm lý & Kỹ năng sống" : domain.label,
        accentBarClass: "from-rose-500 via-pink-500 to-amber-500",
        badgeBgClass: "bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/60 dark:text-rose-300 dark:border-rose-800",
        iconBgClass: "bg-rose-500/10 border-rose-500/20",
        iconTextClass: "text-rose-600 dark:text-rose-400",
        ringColor: "#F43F5E",
        levelLabel,
        estimatedHoursLabel,
      };
  }
}

export function extractCourseSkillTags(course: CourseLike): string[] {
  const raw = `${course.title || ""} ${course.description || ""} ${course.category || ""}`;
  const norm = normalizeVi(raw);
  const domain = classifyCourseDomain(course);
  const tags: string[] = [];

  if (domain.id === "language") {
    const isJapanese = /(tieng nhat|japanese|jlpt|\bn5\b|hiragana|katakana)/.test(norm);
    const isChinese = /(tieng trung|chinese|\bhsk\d?\b|pinyin|hanzi)/.test(norm);
    if (isJapanese) {
      tags.push("JLPT N5");
      tags.push("Hiragana & Katakana");
      tags.push("Shadowing & Dictation");
    } else if (isChinese) {
      tags.push("HSK1 Chuẩn");
      tags.push("Pinyin & Thanh điệu");
      tags.push("Shadowing & Dictation");
    } else {
      if (/\b(ielts)\b/.test(norm)) tags.push("IELTS 4 Kỹ năng");
      if (/\b(word map|mind map|so do tu vung|tu vung|vocabulary)\b/.test(norm)) tags.push("Word Map");
      if (/\b(ipa|phat am|speaking|chuan ipa)\b/.test(norm)) tags.push("Chấm phát âm IPA");
      if (/\b(shadowing|dictation|chep chinh ta|listening|nghe)\b/.test(norm)) tags.push("Shadowing & Dictation");
    }
  } else {
    const rules: Array<[RegExp, string]> = [
      [/(\.net|c#|csharp)/, "C# / .NET 10"],
      [/\b(react|next\.js|nextjs)\b/, "React & Next.js"],
      [/\b(typescript|javascript)\b/, "TypeScript"],
      [/\b(sql|database|ef core)\b/, "SQL & EF Core"],
      [/\b(microservices|ddd|event sourcing)\b/, "Kiến trúc Phân tán"],
      [/\b(he dieu hanh|bo nho ao|tlb|phan trang)\b/, "OS & Quản lý bộ nhớ"],
      [/\b(hoc may|machine learning|deep learning)\b/, "AI & Machine Learning"],
      [/\b(python)\b/, "Python"],
      [/\b(tai chinh ca nhan|ngan sach|50\/30\/20|dong tien)\b/, "Ngân sách 50/30/20"],
      [/\b(dau tu|chung khoan|co phieu|dinh gia)\b/, "Đầu tư & Định giá"],
      [/\b(tam ly|nhan thuc|cbt)\b/, "Tâm lý học hành vi"],
      [/\b(ky nang song|giao tiep|thuyet trinh|dam phan)\b/, "Giao tiếp NVC"],
      [/\b(phap luat|hien phap|dan su|hinh su)\b/, "Pháp lý đại cương"],
    ];

    for (const [pattern, label] of rules) {
      if (pattern.test(norm) && !tags.includes(label)) {
        tags.push(label);
      }
      if (tags.length >= 3) break;
    }
  }

  if (tags.length < 3) {
    if (domain.id === "technology") tags.push("Lab Code & Debug");
    else if (domain.id === "language") {
      if (!tags.includes("Word Map")) tags.push("Word Map");
      if (tags.length < 3 && !tags.includes("Chấm phát âm IPA")) tags.push("Chấm phát âm IPA");
      if (tags.length < 3 && !tags.includes("Shadowing & Dictation")) tags.push("Shadowing & Dictation");
    }
    else if (domain.id === "business") tags.push("Biểu đồ & Tính toán");
    else if (domain.id === "law") tags.push("Trích dẫn & Case");
    else if (domain.id === "science") tags.push("Tư duy Feynman");
    else tags.push("Roleplay Tình huống");
  }

  if (tags.length < 3) {
    tags.push("Quiz & FSRS v5");
  }

  return tags.slice(0, 3);
}

/**
 * Cleans Cloze syntax {{c1::answer}} or {{c1::answer::hint}} from quiz questions
 * so learners see a clean blank instead of raw cloze markup or leaked answers.
 */
export function formatCleanQuizPrompt(
  rawQuestion: string,
  isEvaluated = false,
  solution?: string | null
): string {
  if (!rawQuestion) return "";
  return rawQuestion.replace(/\{\{c\d+::([^}:]+)(?:::([^}]+))?\}\}/g, (_m, ans, hint) => {
    if (isEvaluated) {
      return `[ ${solution?.trim() || ans.trim()} ]`;
    }
    return hint ? `[ ______ (${hint.trim()}) ]` : "[ ______ ]";
  });
}

/**
 * Deduplicates identical documents/notebooks while preserving searchability.
 */
export function deduplicateByTitle<T extends { title?: string; fileName?: string; totalPages?: number | null; pageCount?: number | null }>(
  items: T[]
): T[] {
  const seen = new Map<string, T>();
  for (const item of items) {
    const base = normalizeVi(item.title || item.fileName || "");
    const pages = item.totalPages ?? item.pageCount ?? "";
    const key = base ? `${base}::${pages}` : JSON.stringify(item);
    if (!seen.has(key)) {
      seen.set(key, item);
    }
  }
  return Array.from(seen.values());
}

/**
 * Deduplicates duplicate test/generated courses with identical normalized titles.
 * Keeps the highest-signal copy: higher progress, more lessons, newer timestamp.
 */
export function deduplicateCourses<T extends CourseLike>(courses: T[]): T[] {
  const bestByKey = new Map<string, { course: T; order: number; score: number }>();

  courses.forEach((course, idx) => {
    const normalizedTitle = normalizeVi(course.title || "");
    const id = getCourseId(course);
    const key = normalizedTitle || id || `idx-${idx}`;

    const progress = getCourseProgress(course);
    const completed = Number(course.completedLessons ?? 0);
    const lessons = getCourseLessonCount(course);
    const chapters = getCourseChapterCount(course);
    const ts = Date.parse(course.updatedAt || course.createdAt || "") || 0;

    const score =
      (progress > 0 ? 1_000_000 : 0) +
      completed * 10_000 +
      lessons * 100 +
      chapters * 10 +
      progress +
      (ts > 0 ? ts / 1e13 : 0);

    const existing = bestByKey.get(key);
    if (!existing || score > existing.score) {
      bestByKey.set(key, {
        course,
        order: existing ? existing.order : idx,
        score,
      });
    }
  });

  return Array.from(bestByKey.values())
    .sort((a, b) => a.order - b.order)
    .map((entry) => entry.course);
}
