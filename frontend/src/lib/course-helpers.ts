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
  // Ensure psychology and communication courses are excluded from matching "khoa hoc"
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

export function isCurriculumStandardCourse(course: CourseLike): boolean {
  const rawCombined = `${course.title || ""} ${(course as { slug?: string }).slug || ""} ${course.description || ""}`;
  const norm = normalizeVi(rawCombined);
  return (
    /document-course-/.test(rawCombined) ||
    /(ielts|tieng anh|english|tieng nhat|japanese|jlpt|\bn5\b|tieng trung|chinese|\bhsk\d?\b|\.net|c#|csharp|fullstack|full-stack|dau tu|tai chinh|ky nang song|tam ly hoc|he dieu hanh|hoc may|phap luat)/.test(
      norm
    )
  );
}

export function sortCoursesSmartly<T extends CourseLike>(courses: T[]): T[] {
  return [...courses].sort((a, b) => {
    const aJunk = isTestOrJunkCourse(a);
    const bJunk = isTestOrJunkCourse(b);
    if (aJunk !== bJunk) return aJunk ? 1 : -1;

    const aStd = isCurriculumStandardCourse(a);
    const bStd = isCurriculumStandardCourse(b);
    if (aStd !== bStd) return aStd ? -1 : 1;

    const aProg = getCourseProgress(a);
    const bProg = getCourseProgress(b);
    const aInProg = aProg > 0 && aProg < 100;
    const bInProg = bProg > 0 && bProg < 100;
    if (aInProg !== bInProg) return aInProg ? -1 : 1;

    const aLessons = getCourseLessonCount(a);
    const bLessons = getCourseLessonCount(b);
    if (aLessons !== bLessons) return bLessons - aLessons;

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

export function getCourseVisualTheme(course: CourseLike): CourseVisualTheme {
  const domain = classifyCourseDomain(course);
  const lessons = Math.max(1, getCourseLessonCount(course));
  const estHours = Math.max(1, Math.round((lessons * 25) / 60));
  const norm = normalizeVi(`${course.title || ""} ${course.description || ""}`);

  let levelLabel = "Lộ trình thực chiến";
  if (/(co ban|nhap mon|nen tang|phase 0|tu con so 0|dai cuong|basics|so cap|\bn5\b|\bhsk1\b)/.test(norm)) {
    levelLabel = "Nền tảng → Vững chắc";
  } else if (/(mastery|chuyen sau|nang cao|microservices|ddd|48 tuan|28 tuan|fullstack|full-stack)/.test(norm)) {
    levelLabel = "Chuyên sâu toàn diện";
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
        estimatedHoursLabel: `~${estHours} giờ học`,
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
        estimatedHoursLabel: `~${estHours} giờ học`,
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
        estimatedHoursLabel: `~${estHours} giờ học`,
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
        estimatedHoursLabel: `~${estHours} giờ học`,
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
        estimatedHoursLabel: `~${estHours} giờ học`,
      };
    default:
      if (/(tam ly|ky nang song|giao tiep)/.test(norm)) {
        return {
          domainId: "other",
          domainLabel: "Tâm lý & Kỹ năng sống",
          accentBarClass: "from-rose-500 via-pink-500 to-amber-500",
          badgeBgClass: "bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/60 dark:text-rose-300 dark:border-rose-800",
          iconBgClass: "bg-rose-500/10 border-rose-500/20",
          iconTextClass: "text-rose-600 dark:text-rose-400",
          ringColor: "#F43F5E",
          levelLabel,
          estimatedHoursLabel: `~${estHours} giờ học`,
        };
      }
      return {
        domainId: "other",
        domainLabel: domain.label,
        accentBarClass: "from-rose-500 via-pink-500 to-amber-500",
        badgeBgClass: "bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/60 dark:text-rose-300 dark:border-rose-800",
        iconBgClass: "bg-rose-500/10 border-rose-500/20",
        iconTextClass: "text-rose-600 dark:text-rose-400",
        ringColor: "#F43F5E",
        levelLabel,
        estimatedHoursLabel: `~${estHours} giờ học`,
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
