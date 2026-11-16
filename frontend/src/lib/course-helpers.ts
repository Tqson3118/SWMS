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

export interface CourseLike {
  id?: string;
  courseId?: string;
  title?: string;
  description?: string | null;
  category?: string | null;
  progressPercent?: number;
  progress?: number;
  totalLessons?: number;
  completedLessons?: number;
  chapterCount?: number;
  chapters?: { lessons?: unknown[] }[];
  createdAt?: string | null;
  updatedAt?: string | null;
  visibility?: string | null;
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
  const combined = normalizeVi(
    `${course.category || ""} ${course.title || ""} ${course.description || ""}`
  );

  if (
    /(phap luat|luat|hien phap|hinh su|dan su|hanh chinh|chinh tri|triet hoc|xa hoi|lich su|giao trinh phap|nha nuoc)/.test(
      combined
    )
  ) {
    return { id: "law", label: "Pháp luật & Xã hội" };
  }

  if (
    /(lap trinh|code|coding|react|next|typescript|javascript|python|java|c#|dotnet|\.net|sql|database|backend|frontend|api|ai|machine learning|deep learning|llm|devops|cloud|docker|kubernetes|software|computer|it|data|thuat toan|cau truc du lieu|mang may tinh|he dieu hanh)/.test(
      combined
    )
  ) {
    return { id: "technology", label: "Công nghệ & Lập trình" };
  }

  if (
    /(toan|math|vat ly|physics|hoa hoc|chemistry|sinh hoc|biology|xac suat|thong ke|statistics|giai tich|dai so|khoa hoc)/.test(
      combined
    )
  ) {
    return { id: "science", label: "Khoa học & Toán" };
  }

  if (
    /(tieng anh|english|ielts|toeic|toefl|ngoai ngu|tieng nhat|japanese|tieng trung|chinese|tieng han|korean|ngu phap|tu vung|vocabulary)/.test(
      combined
    )
  ) {
    return { id: "language", label: "Ngoại ngữ" };
  }

  if (
    /(kinh te|quan tri|marketing|tai chinh|ke toan|business|management|finance|economics|startup|product|ban hang|sales)/.test(
      combined
    )
  ) {
    return { id: "business", label: "Kinh tế & Quản trị" };
  }

  return { id: "other", label: "Chủ đề khác" };
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
