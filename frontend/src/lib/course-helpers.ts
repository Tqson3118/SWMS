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
  const rawCombined = `${course.category || ""} ${course.title || ""} ${course.description || ""}`;
  const combined = normalizeVi(rawCombined);

  // 1. Language - prioritize language so "Tiếng Anh cho Dân IT" or "Bài học Tiếng Anh" match language
  if (
    /(tieng anh|english|ielts|toeic|toefl|ngoai ngu|tieng nhat|japanese|tieng trung|chinese|tieng han|korean|ngu phap|tu vung|vocabulary|phat am|listening|reading|writing|speaking|grammar|pronunciation)/.test(
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
    /(\btoan hoc\b|\btoan cao cap\b|\btoan roi rac\b|\bgiai tich\b|\bdai so\b|\bmath\b|\bvat ly\b|\bphysics\b|\bhoa hoc\b|\bchemistry\b|\bsinh hoc\b|\bbiology\b|\bxac suat\b|\bthong ke\b|\bstatistics\b|\bkhoa hoc tu nhien\b)/.test(
      combined
    ) ||
    (/\bkhoa học\b/i.test(rawCombined) && !/\b(khóa học|khoá học)\b/i.test(rawCombined));

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
