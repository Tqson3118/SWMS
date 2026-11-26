import {
  CURATED_COURSE_SLUGS,
  classifyCourseDomain,
  extractCourseSkillTags,
  isCurriculumStandardCourse,
  sortCoursesSmartly,
  type CourseLike,
} from "./course-helpers.ts";

interface TestCase {
  course: CourseLike;
  expectedDomainId: string;
  expectedLabel: string;
}

const testCases: TestCase[] = [
  // 1. Real courses from swms_local_dev.db
  {
    course: {
      title: "IELTS Mastery Roadmap (Phase 0 – Phase 3)",
      description: "Lộ trình 4 giai đoạn Listening, Reading, Writing, Speaking hơn 60 bài nghe WAV.",
    },
    expectedDomainId: "language",
    expectedLabel: "Ngoại ngữ",
  },
  {
    course: {
      title: "Hệ điều hành: Kiến trúc Bộ nhớ ảo & Phân trang",
      description: "Khóa học cốt lõi về Quản lý bộ nhớ, Phần cứng phân trang (MMU/TLB), Bảng trang đa cấp.",
    },
    expectedDomainId: "technology",
    expectedLabel: "Công nghệ & Lập trình",
  },
  {
    course: {
      title: "Tài Chính Cá Nhân Thực Hành (12 Tuần)",
      description: "12 tuần làm chủ tâm lý tiền bạc, dòng tiền, ngân sách, quỹ dự phòng, bảo hiểm.",
    },
    expectedDomainId: "business",
    expectedLabel: "Kinh tế & Quản trị",
  },
  {
    course: {
      title: "Khóa học: Giáo trình Pháp luật đại cương",
      description: "Lộ trình học tập tự động tổng hợp từ tài liệu Giáo trình Pháp luật đại cương (298 trang).",
    },
    expectedDomainId: "law",
    expectedLabel: "Pháp luật & Xã hội",
  },
  {
    course: {
      title: "Khóa học: Lập trình Web C# .NET 10 từ cơ bản đến nâng cao",
      description: "Lộ trình học tập cá nhân hóa về Lập trình Web C# .NET 10 từ cơ bản đến nâng cao.",
    },
    expectedDomainId: "technology",
    expectedLabel: "Công nghệ & Lập trình",
  },
  {
    course: {
      title: "Khóa học: Kiến trúc Microservices, DDD & Event Sourcing",
      description: "Xây dựng hệ thống phân tán chịu tải cao.",
    },
    expectedDomainId: "technology",
    expectedLabel: "Công nghệ & Lập trình",
  },
  {
    course: {
      title: "Khóa học: Học máy cơ bản",
      description: "Nhập môn Machine Learning và Deep Learning cơ bản.",
    },
    expectedDomainId: "technology",
    expectedLabel: "Công nghệ & Lập trình",
  },
  {
    course: {
      title: "Khóa học: Kiến trúc Máy tính",
      description: "Cấu trúc phần cứng máy tính và tập lệnh xử lý.",
    },
    expectedDomainId: "technology",
    expectedLabel: "Công nghệ & Lập trình",
  },
  {
    course: {
      title: "Khóa học: Nhập môn Lập trình Python",
      description: "Học lập trình Python từ con số 0.",
    },
    expectedDomainId: "technology",
    expectedLabel: "Công nghệ & Lập trình",
  },
  {
    course: {
      title: "Kỹ Năng Sống & Giao Tiếp Hiệu Quả (16 Tuần)",
      description: "Rèn luyện kỹ năng mềm và giao tiếp trong cuộc sống.",
    },
    expectedDomainId: "other",
    expectedLabel: "Chủ đề khác",
  },
  {
    course: {
      title: "Tự Học Tâm Lý Học 48 Tuần",
      description: "Giáo trình tâm lý học hành vi và nhận thức.",
    },
    expectedDomainId: "other",
    expectedLabel: "Chủ đề khác",
  },
  // 2. Extra domain verification cases
  {
    course: {
      title: "Giải Tích 1: Giới Hạn, Đạo Hàm & Tích Phân",
      description: "Giáo trình bài giảng giải tích toán học cao cấp cho sinh viên.",
    },
    expectedDomainId: "science",
    expectedLabel: "Khoa học & Toán",
  },
  {
    course: {
      title: "Tiếng Anh Giao Tiếp Hàng Ngày & Phát Âm Chuẩn",
      description: "Khóa học ngữ pháp, từ vựng và luyện phát âm chuẩn quốc tế.",
    },
    expectedDomainId: "language",
    expectedLabel: "Ngoại ngữ",
  },
  {
    course: {
      title: "Bài học Từ Vựng Tiếng Anh Chuyên Ngành",
      description: "Tổng hợp các bài học trau dồi từ vựng thông dụng.",
    },
    expectedDomainId: "language",
    expectedLabel: "Ngoại ngữ",
  },
  {
    course: {
      title: "Lý Thuyết Xác Suất & Thống Kê Toán Học",
      description: "Cơ sở xác suất thống kê đại cương ứng dụng thực tế.",
    },
    expectedDomainId: "science",
    expectedLabel: "Khoa học & Toán",
  },
  {
    course: {
      title: "Quản Trị Tài Chính Doanh Nghiệp & Đầu Tư Cá Nhân",
      description: "Kiến thức tài chính, đầu tư, quản lý dòng tiền và kinh tế thị trường.",
    },
    expectedDomainId: "business",
    expectedLabel: "Kinh tế & Quản trị",
  },
  {
    course: {
      title: "Tiếng Nhật Sơ Cấp (N5)",
      description: "Khóa học tiếng Nhật nền tảng JLPT N5: Hiragana, Katakana, giao tiếp đời sống.",
    },
    expectedDomainId: "language",
    expectedLabel: "Ngoại ngữ",
  },
  {
    course: {
      title: "Tiếng Trung Sơ Cấp (HSK1)",
      description: "Khóa học tiếng Trung giao tiếp chuẩn HSK1: Pinyin, 4 thanh điệu và chữ Hán cơ bản.",
    },
    expectedDomainId: "language",
    expectedLabel: "Ngoại ngữ",
  },
  // 3. Wave 4 curated descriptions regression cases
  {
    course: {
      title: "Đầu Tư Tài Chính Nền Tảng (18 Tuần)",
      category: "Tài chính & Đầu tư",
      description:
        "Chương trình 18 tuần xây dựng nền tảng đầu tư bài bản: từ giá trị thời gian của tiền, phân tích báo cáo tài chính và định giá doanh nghiệp đến quản trị danh mục và kỷ luật hành vi.",
    },
    expectedDomainId: "business",
    expectedLabel: "Kinh tế & Quản trị",
  },
  {
    course: {
      title: "Kỹ Năng Sống & Giao Tiếp Hiệu Quả (16 Tuần)",
      category: "Tâm lý & Kỹ năng sống",
      description:
        "Hành trình 16 tuần rèn luyện tự nhận thức, trí tuệ cảm xúc, lắng nghe thấu cảm, Giao tiếp Phi Bạo Lực (NVC), tư duy phản biện và quản trị năng lượng cá nhân.",
    },
    expectedDomainId: "other",
    expectedLabel: "Chủ đề khác",
  },
  {
    course: {
      title: "Tự Học Tâm Lý Học 48 Tuần",
      category: "Tâm lý & Kỹ năng sống",
      description:
        "Chương trình 48 tuần khám phá tâm lý học hệ thống: từ khoa học thần kinh, nhận thức và cảm xúc đến tâm lý xã hội, gắn bó tình cảm và chăm sóc sức khỏe tinh thần.",
    },
    expectedDomainId: "other",
    expectedLabel: "Chủ đề khác",
  },
  // 4. Wave 3 Applied Systems & Deep Learning curated courses
  {
    course: {
      slug: "document-course-ai-workflows",
      title: "AI Ứng Dụng & Tự Động Hóa Workflow Thực Chiến (2026)",
      description:
        "Lộ trình 12 tuần thực chiến làm chủ Prompt Engineering chuẩn kỹ thuật, Structured Outputs, RAG cơ bản và thiết kế Workflow Agent an toàn.",
    },
    expectedDomainId: "technology",
    expectedLabel: "Công nghệ & Lập trình",
  },
  {
    course: {
      slug: "document-course-data-sql",
      title: "Tư Duy Dữ Liệu, SQL & Ra Quyết Định Bằng Số Liệu",
      description:
        "Khóa học 12 tuần từ tư duy đặt câu hỏi kinh doanh, SQL phân tích (JOIN, CTE, Window Functions), Cohort/Funnel đến thiết kế Dashboard.",
    },
    expectedDomainId: "technology",
    expectedLabel: "Công nghệ & Lập trình",
  },
  {
    course: {
      slug: "document-course-cybersecurity",
      title: "An Toàn Thông Tin, Quyền Riêng Tư & Phòng Vệ Lừa Đảo Số",
      description:
        "Cẩm nang 10 tuần phòng vệ an toàn số thực chiến tại Việt Nam: nhận diện Phishing/Quishing/Deepfake/APK giả mạo, Passkeys/MFA chuẩn FIDO2.",
    },
    expectedDomainId: "technology",
    expectedLabel: "Công nghệ & Lập trình",
  },
  {
    course: {
      slug: "document-course-deep-learning-systems",
      title: "Học Sâu, Tư Duy Hệ Thống & Quản Trị Hiệu Suất Thời AI",
      description:
        "10 tuần làm chủ khoa học trí nhớ dài hạn (Active Recall, FSRS, Feynman, Interleaving), tư duy hệ thống (Stocks/Flows, MECE) và Deep Work.",
    },
    expectedDomainId: "technology",
    expectedLabel: "Công nghệ & Lập trình",
  },
];

let failed = 0;
for (const tc of testCases) {
  const result = classifyCourseDomain(tc.course);
  if (result.id !== tc.expectedDomainId) {
    console.error(
      `FAIL: "${tc.course.title}" -> expected "${tc.expectedDomainId}", got "${result.id}"`
    );
    failed++;
  } else {
    console.log(
      `PASS: [${result.id}] "${tc.course.title}"`
    );
  }
}

// Verify Japanese & Chinese skill tags are tailored to N5 / HSK1 (not English IPA)
const jaTags = extractCourseSkillTags({ title: "Tiếng Nhật Sơ Cấp (N5)", description: "JLPT N5 Hiragana Katakana" });
if (!jaTags.includes("JLPT N5") || jaTags.includes("Chấm phát âm IPA")) {
  console.error(`FAIL: Unexpected Japanese skill tags: ${jaTags.join(", ")}`);
  failed++;
} else {
  console.log(`PASS: Japanese skill tags -> ${jaTags.join(", ")}`);
}

const zhTags = extractCourseSkillTags({ title: "Tiếng Trung Sơ Cấp (HSK1)", description: "HSK1 Pinyin Thanh điệu" });
if (!zhTags.includes("HSK1 Chuẩn") || zhTags.includes("Chấm phát âm IPA")) {
  console.error(`FAIL: Unexpected Chinese skill tags: ${zhTags.join(", ")}`);
  failed++;
} else {
  console.log(`PASS: Chinese skill tags -> ${zhTags.join(", ")}`);
}

// Verify 14 curated slugs & curriculum standard recognition
if (CURATED_COURSE_SLUGS.length !== 14) {
  console.error(`FAIL: Expected 14 curated slugs, got ${CURATED_COURSE_SLUGS.length}`);
  failed++;
} else {
  console.log(`PASS: CURATED_COURSE_SLUGS has ${CURATED_COURSE_SLUGS.length} slugs`);
}

const newSlugs = [
  "document-course-ai-workflows",
  "document-course-data-sql",
  "document-course-cybersecurity",
  "document-course-deep-learning-systems",
];
for (const slug of newSlugs) {
  if (!CURATED_COURSE_SLUGS.includes(slug as any)) {
    console.error(`FAIL: Missing curated slug "${slug}"`);
    failed++;
  }
  if (!isCurriculumStandardCourse({ slug, title: slug })) {
    console.error(`FAIL: isCurriculumStandardCourse returned false for "${slug}"`);
    failed++;
  }
}

const aiTags = extractCourseSkillTags({
  slug: "document-course-ai-workflows",
  title: "AI Ứng Dụng & Tự Động Hóa Workflow Thực Chiến (2026)",
  description: "Prompt Engineering, Structured Outputs, RAG và Workflow Agent.",
});
if (!aiTags.includes("AI Workflows")) {
  console.error(`FAIL: Expected 'AI Workflows' tag, got: ${aiTags.join(", ")}`);
  failed++;
} else {
  console.log(`PASS: AI Workflows skill tags -> ${aiTags.join(", ")}`);
}

const sqlTags = extractCourseSkillTags({
  slug: "document-course-data-sql",
  title: "Tư Duy Dữ Liệu, SQL & Ra Quyết Định Bằng Số Liệu",
  description: "SQL phân tích, Cohort, Funnel, Window Functions và A/B Testing.",
});
if (!sqlTags.includes("SQL & Analytics")) {
  console.error(`FAIL: Expected 'SQL & Analytics' tag, got: ${sqlTags.join(", ")}`);
  failed++;
} else {
  console.log(`PASS: Data SQL skill tags -> ${sqlTags.join(", ")}`);
}

const cyberTags = extractCourseSkillTags({
  slug: "document-course-cybersecurity",
  title: "An Toàn Thông Tin, Quyền Riêng Tư & Phòng Vệ Lừa Đảo Số",
  description: "Nhận diện Phishing, Quishing, Passkeys FIDO2 và ứng cứu tài khoản.",
});
if (!cyberTags.includes("An toàn số")) {
  console.error(`FAIL: Expected 'An toàn số' tag, got: ${cyberTags.join(", ")}`);
  failed++;
} else {
  console.log(`PASS: Cybersecurity skill tags -> ${cyberTags.join(", ")}`);
}

const dlTags = extractCourseSkillTags({
  slug: "document-course-deep-learning-systems",
  title: "Học Sâu, Tư Duy Hệ Thống & Quản Trị Hiệu Suất Thời AI",
  description: "Active Recall, FSRS, Feynman, Stocks/Flows và Deep Work.",
});
if (!dlTags.includes("Học sâu")) {
  console.error(`FAIL: Expected 'Học sâu' tag, got: ${dlTags.join(", ")}`);
  failed++;
} else {
  console.log(`PASS: Deep Learning Systems skill tags -> ${dlTags.join(", ")}`);
}

const sorted = sortCoursesSmartly([
  { id: "custom", title: "Khóa học cá nhân khác", lessonCount: 5 },
  { id: "ai", slug: "document-course-ai-workflows", title: "AI Workflows", lessonCount: 12 },
  { id: "ielts", slug: "document-course-ielts", title: "IELTS", lessonCount: 20 },
]);
if (sorted[0].id !== "ielts" || sorted[1].id !== "ai" || sorted[2].id !== "custom") {
  console.error(`FAIL: Unexpected sortCoursesSmartly order: ${sorted.map((s) => s.id).join(", ")}`);
  failed++;
} else {
  console.log("PASS: sortCoursesSmartly preserves curated priority ordering");
}

if (failed > 0) {
  console.error(`\n❌ ${failed} tests failed.`);
  process.exit(1);
} else {
  console.log(`\n✅ All ${testCases.length} course classification + 14 curated slug tests passed!`);
}
