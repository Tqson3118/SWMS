import { classifyCourseDomain, extractCourseSkillTags, type CourseLike } from "./course-helpers.ts";

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

if (failed > 0) {
  console.error(`\n❌ ${failed} / ${testCases.length} tests failed.`);
  process.exit(1);
} else {
  console.log(`\n✅ All ${testCases.length} course classification tests passed!`);
}
