import {
  normalizeVi,
  getCourseProgress,
  getCourseId,
  isRetiredCourse,
  type CourseLike,
} from "./course-helpers";

export interface RoadmapMilestone {
  id: string;
  phaseLabel: string;
  title: string;
  duration: string;
  summary: string;
  skills: string[];
  deliverable: string;
  matchKeywords: string[];
  aiTopicPrompt: string;
}

export interface LearningRoadmap {
  id: string;
  title: string;
  subtitle: string;
  category: string;
  badge: string;
  accentGradient: string;
  badgeClasses: string;
  totalDuration: string;
  targetAudience: string;
  outcome: string;
  milestones: RoadmapMilestone[];
}

export interface StudyUseCase {
  id: string;
  title: string;
  persona: string;
  painPoint: string;
  workflowSteps: string[];
  recommendedFeatures: string[];
  accentColor: string;
  aiTemplateTopic: string;
  targetRoute: string;
  ctaLabel: string;
}

export const LEARNING_ROADMAPS: LearningRoadmap[] = [
  {
    id: "roadmap-fullstack-dotnet-react",
    title: "Lộ trình Kỹ sư Full-Stack .NET 10 & React",
    subtitle: "Từ C# cốt lõi, ASP.NET Core Minimal API đến React 19, kiến trúc phân tán & vận hành thực tế",
    category: "Công nghệ & Lập trình",
    badge: "28 tuần · Thực hành dự án",
    accentGradient: "from-indigo-600 via-violet-600 to-blue-500",
    badgeClasses:
      "bg-indigo-50 text-indigo-700 border-indigo-200 dark:bg-indigo-950/60 dark:text-indigo-300 dark:border-indigo-800",
    totalDuration: "28 tuần (4 chặng)",
    targetAudience: "Sinh viên CNTT, lập trình viên Backend/Frontend chuyển sang Full-Stack",
    outcome: "Xây dựng ứng dụng Web chịu tải cao với .NET, kiểm soát đồng thời an toàn & React TypeScript",
    milestones: [
      {
        id: "fs-1",
        phaseLabel: "Chặng 1",
        title: "C# hiện đại, OOP & SQL Server",
        duration: "6 tuần",
        summary: "Nắm chắc LINQ, Async/Await, Dependency Injection và thiết kế cơ sở dữ liệu quan hệ chuẩn hóa.",
        skills: ["C# .NET 10", "Async/Await", "SQL & Indexing"],
        deliverable: "Dịch vụ quản lý dữ liệu chuẩn hóa với .NET và SQL Server",
        matchKeywords: ["c#", "dotnet", ".net", "fullstack", "full-stack", "web api", "studxy full-stack developer bootcamp", "bootcamp", "studxy full-stack"],
        aiTopicPrompt: "Lộ trình C# .NET 10, tối ưu truy vấn dữ liệu và SQL Server qua bài tập dự án",
      },
      {
        id: "fs-2",
        phaseLabel: "Chặng 2",
        title: "React 19, TypeScript & quản lý trạng thái",
        duration: "8 tuần",
        summary: "Xây dựng giao diện phản hồi nhanh, Next.js App Router, Zustand và xử lý biểu mẫu, API an toàn.",
        skills: ["React 19", "TypeScript", "Next.js 15"],
        deliverable: "Giao diện học tập tương tác đáp ứng mượt trên mọi màn hình",
        matchKeywords: ["fullstack", "react", "typescript", "web", "studxy full-stack developer bootcamp", "bootcamp", "studxy full-stack"],
        aiTopicPrompt: "React 19, Next.js App Router và TypeScript nâng cao qua dự án thực tế",
      },
      {
        id: "fs-3",
        phaseLabel: "Chặng 3",
        title: "Hệ điều hành, bộ nhớ ảo & xử lý đồng thời",
        duration: "6 tuần",
        summary: "Hiểu sâu cơ chế ánh xạ địa chỉ MMU/TLB, khóa đồng bộ đa luồng và kiểm soát xung đột dữ liệu.",
        skills: ["Hệ điều hành", "TLB / Paging", "Đồng bộ đa luồng"],
        deliverable: "Xử lý xung đột dữ liệu đồng thời an toàn và kiểm thử chịu tải",
        matchKeywords: ["he dieu hanh", "bo nho ao", "phan trang", "kien truc may tinh", "studxy full-stack developer bootcamp", "bootcamp", "studxy full-stack"],
        aiTopicPrompt: "Hệ điều hành: Bộ nhớ ảo, TLB, đa luồng và đồng bộ hóa tiến trình",
      },
      {
        id: "fs-4",
        phaseLabel: "Chặng 4",
        title: "Microservices, DDD, Event Sourcing & Docker",
        duration: "8 tuần",
        summary: "Kiến trúc hướng miền DDD, Outbox Pattern, Redis/PostgreSQL và kiểm thử đầu-cuối tự động.",
        skills: ["Microservices", "DDD & Outbox", "Docker & Kiểm thử"],
        deliverable: "Kiến trúc dịch vụ phân tán hoàn chỉnh có quy trình kiểm thử và tài liệu kiến trúc",
        matchKeywords: ["microservices", "ddd", "event sourcing", "lộ trình mẫu", "studxy full-stack developer bootcamp", "bootcamp", "studxy full-stack"],
        aiTopicPrompt: "Kiến trúc Microservices, Domain-Driven Design (DDD) và Event Sourcing",
      },
    ],
  },
  {
    id: "roadmap-ielts-mastery",
    title: "Lộ trình chinh phục IELTS 4.0 → 7.5+",
    subtitle: "Luyện đồng bộ 4 kỹ năng Listening, Reading, Speaking, Writing kết hợp Shadowing & FSRS v5",
    category: "Ngoại ngữ",
    badge: "4 chặng · Nghe nói & IPA",
    accentGradient: "from-sky-500 via-cyan-500 to-teal-500",
    badgeClasses:
      "bg-sky-50 text-sky-700 border-sky-200 dark:bg-sky-950/60 dark:text-sky-300 dark:border-sky-800",
    totalDuration: "4 giai đoạn (60+ bài nghe)",
    targetAudience: "Người bắt đầu lại từ nền tảng IPA hoặc đặt mục tiêu 7.0+ học thuật",
    outcome: "Làm chủ 1.500+ cụm từ học thuật, phát âm IPA chuẩn & phản xạ nói trôi chảy",
    milestones: [
      {
        id: "ie-1",
        phaseLabel: "Giai đoạn 0",
        title: "Phát âm IPA, cặp âm dễ nhầm & ngữ pháp cốt lõi",
        duration: "4 tuần",
        summary: "Chuẩn hóa 44 âm IPA, phân biệt cặp âm dễ nhầm và luyện đọc to hàng ngày.",
        skills: ["44 âm IPA", "Minimal Pairs", "Ngữ pháp nền"],
        deliverable: "Ghi âm và đạt trên 85% độ khớp phát âm trong trạm luyện nói",
        matchKeywords: ["ielts", "tieng anh", "english", "phat am"],
        aiTopicPrompt: "Chuẩn hóa phát âm IPA tiếng Anh, Minimal Pairs và ngữ pháp nền tảng IELTS",
      },
      {
        id: "ie-2",
        phaseLabel: "Giai đoạn 1",
        title: "Nghe chép chính tả & luyện nói đuổi tăng tốc",
        duration: "8 tuần",
        summary: "Nghe chép chính tả đoạn hội thoại thực tế, bắt nối âm và nhận diện bẫy trong IELTS Listening.",
        skills: ["Nghe chép Dictation", "Shadowing 0.75x–1x", "Paraphrase"],
        deliverable: "Hoàn thành 30 bài nghe chép chính tả đạt độ chính xác từ 90% trở lên",
        matchKeywords: ["ielts", "listening"],
        aiTopicPrompt: "IELTS Listening: Kỹ thuật nghe chép chính tả và nhận diện từ khóa",
      },
      {
        id: "ie-3",
        phaseLabel: "Giai đoạn 2",
        title: "Đọc hiểu học thuật & từ vựng FSRS v5",
        duration: "8 tuần",
        summary: "Đọc lướt và quét thông tin đoạn văn học thuật dài, biến từ vựng mới thành thẻ FSRS nhớ lâu.",
        skills: ["Skimming & Scanning", "True/False/Not Given", "Collocation FSRS"],
        deliverable: "Bộ 500+ thẻ điền khuyết từ vựng học thuật thuộc vững qua FSRS v5",
        matchKeywords: ["ielts", "reading", "vocabulary"],
        aiTopicPrompt: "IELTS Academic Reading: Chiến lược giải bài đọc và từ vựng học thuật",
      },
      {
        id: "ie-4",
        phaseLabel: "Giai đoạn 3",
        title: "Speaking Part 2–3 & Writing Task 1–2",
        duration: "8 tuần",
        summary: "Triển khai dàn ý 3 phần, ghi âm tự kiểm tra bài nói và viết luận bám sát tiêu chí chấm điểm.",
        skills: ["Speaking Cue Card", "Task 1 Biểu đồ", "Task 2 Luận"],
        deliverable: "Hoàn thiện bộ 20 bài nói mẫu & bài luận được AI đánh giá theo tiêu chí",
        matchKeywords: ["ielts", "speaking", "writing"],
        aiTopicPrompt: "IELTS Speaking & Writing Band 7.5+: Cấu trúc lập luận và từ vựng ghi điểm",
      },
    ],
  },
  {
    id: "roadmap-finance-investing",
    title: "Lộ trình Tài chính cá nhân & Đầu tư bền vững",
    subtitle: "Làm chủ dòng tiền 50/30/20, trả nợ thông minh, đọc báo cáo tài chính và đầu tư dài hạn",
    category: "Kinh tế & Quản trị",
    badge: "30 tuần · Mô phỏng số",
    accentGradient: "from-emerald-500 via-teal-500 to-lime-500",
    badgeClasses:
      "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800",
    totalDuration: "30 tuần (12 tuần cá nhân + 18 tuần đầu tư)",
    targetAudience: "Người đi làm trẻ, người mới bắt đầu tìm hiểu đầu tư và xây dựng tự do tài chính",
    outcome: "Có quỹ dự phòng 6 tháng, kế hoạch lãi kép 10 năm và quy trình phân tích cổ phiếu bài bản",
    milestones: [
      {
        id: "fn-1",
        phaseLabel: "Chặng 1",
        title: "Tâm lý tiền bạc & ngân sách 50/30/20",
        duration: "4 tuần",
        summary: "Rà soát dòng tiền cá nhân, phân bổ thiết yếu / mong muốn / tiết kiệm tự động.",
        skills: ["Ngân sách 50/30/20", "Quỹ dự phòng 6 tháng", "Kiểm soát chi tiêu"],
        deliverable: "Bảng phân bổ thu nhập & mục tiêu quỹ khẩn cấp trong Sổ tay",
        matchKeywords: ["tai chinh ca nhan", "ngan sach", "50/30/20"],
        aiTopicPrompt: "Quản trị tài chính cá nhân: Quy tắc 50/30/20 và xây dựng quỹ dự phòng",
      },
      {
        id: "fn-2",
        phaseLabel: "Chặng 2",
        title: "Chiến lược trả nợ & lãi kép chống lạm phát",
        duration: "8 tuần",
        summary: "So sánh Snowball và Avalanche, tính lãi suất thực và lập mục tiêu tích lũy hưu trí.",
        skills: ["Snowball / Avalanche", "Lãi kép & Lạm phát", "Phòng vệ rủi ro"],
        deliverable: "Kế hoạch xóa nợ & mô phỏng tăng trưởng tài sản 15 năm",
        matchKeywords: ["tai chinh ca nhan", "lai kep", "tra no"],
        aiTopicPrompt: "Chiến lược trả nợ Snowball vs Avalanche và sức mạnh lãi kép dài hạn",
      },
      {
        id: "fn-3",
        phaseLabel: "Chặng 3",
        title: "Phân tích cơ bản & đọc báo cáo tài chính",
        duration: "9 tuần",
        summary: "Đọc hiểu bảng cân đối kế toán, kết quả kinh doanh, lưu chuyển tiền tệ & P/E, ROE.",
        skills: ["Đọc BCTC", "Định giá P/E, P/B", "Biên an toàn"],
        deliverable: "Phiếu phân tích 1 doanh nghiệp niêm yết theo 5 tiêu chí chất lượng",
        matchKeywords: ["dau tu tai chinh", "dau tu", "chung khoan"],
        aiTopicPrompt: "Đầu tư chứng khoán cơ bản: Đọc báo cáo tài chính và định giá doanh nghiệp",
      },
      {
        id: "fn-4",
        phaseLabel: "Chặng 4",
        title: "Biểu đồ nến, quản trị rủi ro & danh mục",
        duration: "9 tuần",
        summary: "Đọc hành vi giá trên biểu đồ nến, đặt điểm cắt lỗ và chốt lời giữ tỷ lệ R:R từ 1:2 trở lên.",
        skills: ["Biểu đồ nến", "Quản trị R:R", "Tái cân bằng danh mục"],
        deliverable: "Nhật ký giao dịch kỷ luật & nguyên tắc phân bổ đa tài sản",
        matchKeywords: ["dau tu", "nen", "candlestick"],
        aiTopicPrompt: "Quản trị rủi ro đầu tư, tỷ lệ Risk:Reward và phân bổ danh mục bền vững",
      },
    ],
  },
  {
    id: "roadmap-ai-data-python",
    title: "Lộ trình AI, Học máy & Khoa học dữ liệu Python",
    subtitle: "Làm chủ Python, toán xác suất và giải tích, thuật toán học máy và ứng dụng LLM",
    category: "Công nghệ & Lập trình",
    badge: "16 tuần · AI & Python",
    accentGradient: "from-purple-600 via-fuchsia-500 to-pink-500",
    badgeClasses:
      "bg-purple-50 text-purple-700 border-purple-200 dark:bg-purple-950/60 dark:text-purple-300 dark:border-purple-800",
    totalDuration: "16 tuần (4 chặng)",
    targetAudience: "Lập trình viên, sinh viên định hướng AI Engineer / Data Scientist",
    outcome: "Hiểu bản chất toán học ML, huấn luyện mô hình Python & đánh giá chống rò rỉ dữ liệu",
    milestones: [
      {
        id: "ai-1",
        phaseLabel: "Chặng 1",
        title: "Lập trình Python & xử lý dữ liệu",
        duration: "4 tuần",
        summary: "Cú pháp Python hiện đại, cấu trúc dữ liệu, xử lý tệp và vector hóa dữ liệu.",
        skills: ["Python cơ bản", "List/Dict Comprehensions", "Mã nguồn sạch"],
        deliverable: "Tệp Python tự động phân tích dữ liệu học tập",
        matchKeywords: ["python", "nhap mon lap trinh python"],
        aiTopicPrompt: "Nhập môn Python và xử lý dữ liệu thực tế cho người mới",
      },
      {
        id: "ai-2",
        phaseLabel: "Chặng 2",
        title: "Học máy cơ bản (có giám sát & không giám sát)",
        duration: "4 tuần",
        summary: "Hồi quy, phân loại, phân cụm, Cross-Validation và cân bằng Bias–Variance.",
        skills: ["Hồi quy & Phân loại", "Chuẩn hóa đặc trưng", "Thước đo đánh giá"],
        deliverable: "Quy trình dự đoán với đánh giá ROC-AUC & chống rò rỉ dữ liệu",
        matchKeywords: ["hoc may", "machine learning", "deep learning"],
        aiTopicPrompt: "Học máy cơ bản: Thuật toán Supervised, Unsupervised và đánh giá mô hình",
      },
      {
        id: "ai-3",
        phaseLabel: "Chặng 3",
        title: "Toán & kiến trúc tính toán hiệu năng cao",
        duration: "4 tuần",
        summary: "Đại số tuyến tính, đạo hàm tối ưu Gradient Descent và tối ưu bộ nhớ CPU/Cache.",
        skills: ["Gradient Descent", "Xác suất thống kê", "Kiến trúc máy tính"],
        deliverable: "Bản tự diễn đạt Feynman về bản chất toán học của lan truyền ngược",
        matchKeywords: ["kien truc may tinh", "giai tich", "xac suat", "toan"],
        aiTopicPrompt: "Toán cho Machine Learning: Xác suất, Đại số tuyến tính và Gradient Descent",
      },
      {
        id: "ai-4",
        phaseLabel: "Chặng 4",
        title: "Ứng dụng LLM, RAG & đánh giá chất lượng AI",
        duration: "4 tuần",
        summary: "Thiết kế prompt có cấu trúc, trích xuất PDF theo trang & đánh giá câu trả lời theo tiêu chí.",
        skills: ["Thiết kế Prompt", "RAG & Trích dẫn", "Đánh giá theo tiêu chí"],
        deliverable: "Trợ lý học tập AI gắn trích dẫn chính xác từng trang từ tài liệu PDF",
        matchKeywords: ["ai", "llm", "hoc may"],
        aiTopicPrompt: "Xây dựng ứng dụng AI với LLM, RAG và kiểm thử chất lượng phản hồi",
      },
    ],
  },
  {
    id: "roadmap-psychology-communication",
    title: "Lộ trình Tâm lý học ứng dụng & Giao tiếp hiệu quả",
    subtitle: "Hiểu cơ chế nhận thức – cảm xúc, Giao tiếp trắc ẩn (NVC) và thương lượng tôn trọng lẫn nhau",
    category: "Chủ đề khác",
    badge: "64 tuần · Nhập vai NVC",
    accentGradient: "from-rose-500 via-pink-500 to-amber-500",
    badgeClasses:
      "bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/60 dark:text-rose-300 dark:border-rose-800",
    totalDuration: "2 khóa song hành (16 tuần + 48 tuần)",
    targetAudience: "Bất kỳ ai muốn nâng cao trí tuệ cảm xúc (EQ), làm việc nhóm và tư duy phản biện",
    outcome: "Nhận diện thiên kiến tâm lý, xử lý xung đột nơi công sở và giao tiếp giữ ranh giới tích cực",
    milestones: [
      {
        id: "ps-1",
        phaseLabel: "Chặng 1",
        title: "Tự nhận thức & lắng nghe chủ động",
        duration: "6 tuần",
        summary: "Phân biệt sự thật quan sát với phán xét cảm tính, thực hành phản hồi đồng cảm.",
        skills: ["Lắng nghe chủ động", "Quan sát khách quan", "Quản lý cảm xúc"],
        deliverable: "Hoàn thành 5 tình huống nhập vai NVC đạt điểm thấu cảm từ 9/10 trở lên",
        matchKeywords: ["ky nang song", "giao tiep"],
        aiTopicPrompt: "Kỹ năng lắng nghe chủ động và quản trị cảm xúc nơi công sở",
      },
      {
        id: "ps-2",
        phaseLabel: "Chặng 2",
        title: "Giao tiếp phi bạo lực (NVC) & góp ý xây dựng",
        duration: "10 tuần",
        summary: "Áp dụng công thức 4 bước OFNR (Quan sát – Cảm xúc – Nhu cầu – Đề nghị).",
        skills: ["Mô hình NVC 4 bước", "Từ chối khéo léo", "Thương lượng"],
        deliverable: "Bộ mẫu phản hồi cho các tình huống trễ hạn hoặc bất đồng quan điểm",
        matchKeywords: ["ky nang song", "giao tiep"],
        aiTopicPrompt: "Giao tiếp Phi Bạo Lực (NVC) và nghệ thuật góp ý không gây phòng thủ",
      },
      {
        id: "ps-3",
        phaseLabel: "Chặng 3",
        title: "Tâm lý học nhận thức & trí nhớ con người",
        duration: "24 tuần",
        summary: "Khoa học thần kinh về trí nhớ, sự chú ý, CBT và cách vượt qua đường cong quên lãng Ebbinghaus.",
        skills: ["Nhận thức & Chú ý", "CBT cơ bản", "Khoa học trí nhớ"],
        deliverable: "Áp dụng chủ động gợi nhớ & FSRS v5 vào lịch học cá nhân hàng ngày",
        matchKeywords: ["tam ly hoc", "tu hoc tam ly"],
        aiTopicPrompt: "Tâm lý học nhận thức: Trí nhớ, sự chú ý và thay đổi thói quen theo CBT",
      },
      {
        id: "ps-4",
        phaseLabel: "Chặng 4",
        title: "Tâm lý xã hội & phòng tránh thao túng tâm lý",
        duration: "24 tuần",
        summary: "Nhận diện thiên kiến nhận thức (Cognitive Biases) và kỹ năng thuyết phục lành mạnh.",
        skills: ["Thiên kiến nhận thức", "Tâm lý xã hội", "Tư duy phản biện"],
        deliverable: "Nhật ký phân tích 10 thiên kiến thường gặp trong quyết định đời sống",
        matchKeywords: ["tam ly hoc", "tam ly"],
        aiTopicPrompt: "Tâm lý học xã hội: Thiên kiến nhận thức và tư duy phản biện ứng dụng",
      },
    ],
  },
  {
    id: "roadmap-academic-law-pkm",
    title: "Lộ trình Nghiên cứu giáo trình dài & Pháp luật đại cương",
    subtitle: "Đọc tài liệu PDF 300+ trang song song Sổ tay trích dẫn, ôn tập điền khuyết & tự diễn đạt Feynman",
    category: "Pháp luật & Xã hội",
    badge: "Đọc sâu PDF · Trích dẫn",
    accentGradient: "from-amber-500 via-orange-500 to-rose-500",
    badgeClasses:
      "bg-amber-50 text-amber-800 border-amber-200 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-800",
    totalDuration: "10 tuần (nghiên cứu sâu)",
    targetAudience: "Sinh viên Luật, Kinh tế, nghiên cứu sinh đọc giáo trình dày",
    outcome: "Làm chủ kỹ năng bóc tách văn bản luật, trích dẫn trang PDF hai chiều & nhớ điều luật lâu dài",
    milestones: [
      {
        id: "lw-1",
        phaseLabel: "Chặng 1",
        title: "Nhập môn Nhà nước & Pháp luật",
        duration: "3 tuần",
        summary: "Đọc song song giáo trình PDF gần 300 trang, tô màu 5 nhóm ý (Khái niệm, Ví dụ, Câu hỏi).",
        skills: ["Đọc PDF song song", "Tô sáng 5 màu", "Hệ thống quy phạm"],
        deliverable: "Ghi chú trong Sổ tay gắn trích dẫn trang PDF chính xác",
        matchKeywords: ["phap luat", "giao trinh phap luat"],
        aiTopicPrompt: "Pháp luật đại cương: Bản chất Nhà nước, quy phạm và quan hệ pháp luật",
      },
      {
        id: "lw-2",
        phaseLabel: "Chặng 2",
        title: "Hiến pháp, Hành chính & Dân sự cốt lõi",
        duration: "3 tuần",
        summary: "Phân biệt quyền và nghĩa vụ công dân, hợp đồng dân sự và xử lý vi phạm hành chính.",
        skills: ["Hiến pháp & Hành chính", "Hợp đồng dân sự", "Tạo thẻ Cloze nhanh"],
        deliverable: "Bộ 30 thẻ điền khuyết ghi nhớ các mốc thời hiệu và điều khoản trọng tâm",
        matchKeywords: ["phap luat"],
        aiTopicPrompt: "Pháp luật Dân sự, Hành chính và phân tích tình huống hợp đồng thực tế",
      },
      {
        id: "lw-3",
        phaseLabel: "Chặng 3",
        title: "Hình sự, Lao động & Tranh chấp kinh doanh",
        duration: "2 tuần",
        summary: "Giải quyết tình huống thực tế, so sánh cấu thành tội phạm và trách nhiệm pháp lý.",
        skills: ["Tình huống pháp lý", "Tự luận đánh giá tiêu chí", "Feynman Recall"],
        deliverable: "Giải 5 tình huống pháp lý và đối chiếu bằng phương pháp Feynman",
        matchKeywords: ["phap luat"],
        aiTopicPrompt: "Phân tích tình huống Pháp luật Hình sự, Lao động và Kinh doanh",
      },
      {
        id: "lw-4",
        phaseLabel: "Chặng 4",
        title: "Tổng hợp Sổ tay Obsidian & ôn thi FSRS v5",
        duration: "2 tuần",
        summary: "Xuất toàn bộ sổ tay sang Markdown chuẩn Obsidian, tổng ôn toàn bộ thẻ đến hạn.",
        skills: ["Xuất Markdown PKM", "Tổng ôn FSRS v5", "Thi thử Quiz"],
        deliverable: "Đạt từ 85% trở lên ở bài kiểm tra tổng hợp Giáo trình Pháp luật đại cương",
        matchKeywords: ["phap luat", "tai lieu"],
        aiTopicPrompt: "Tổng ôn kiến thức Pháp luật đại cương với câu hỏi trắc nghiệm và tình huống",
      },
    ],
  },
];

export const STUDY_USE_CASES: StudyUseCase[] = [
  {
    id: "uc-pdf-to-course",
    title: "Biến giáo trình PDF 300 trang thành khóa học & thẻ nhớ",
    persona: "Sinh viên đại học / Nghiên cứu sinh",
    painPoint: "Tài liệu PDF dài hàng trăm trang, đọc xong dễ quên ngay, khó ôn tập trước kỳ thi.",
    workflowSteps: [
      "Tải tài liệu PDF lên (/app/courses/new) để bóc tách thành chương mục và bài giảng.",
      "Mở chế độ Đọc song song: bên trái xem trang PDF gốc, bên phải ghi chú & tô màu 5 nhóm ý.",
      "Bôi đen định nghĩa quan trọng rồi bấm Tạo thẻ nhanh để đưa ngay vào hàng đợi FSRS v5.",
    ],
    recommendedFeatures: ["Đọc PDF tải từng phần mượt mà", "Đọc song song", "Tạo thẻ nhanh"],
    accentColor: "indigo",
    aiTemplateTopic: "Phương pháp đọc sâu giáo trình đại học và tổng hợp thẻ ghi nhớ FSRS",
    targetRoute: "/app/documents",
    ctaLabel: "Mở thư viện PDF",
  },
  {
    id: "uc-ielts-speaking-lab",
    title: "Luyện nghe chép & phát âm chuẩn IPA ngay trong bài đọc",
    persona: "Người tự học IELTS / Tiếng Anh giao tiếp",
    painPoint: "Học từ vựng qua chữ tĩnh nên khó hình dung cách phát âm IPA chuẩn hay tự kiểm tra giọng đọc.",
    workflowSteps: [
      "Mở bất kỳ bài học IELTS nào để dùng trạm luyện đọc to ngay dưới bảng từ vựng và cặp âm.",
      "Nghe mẫu giọng Anh – Mỹ hoặc Anh – Anh ở tốc độ 0.75x hoặc 1.0x, sau đó thu âm đọc thử để kiểm tra từng từ.",
      "Cuộn xuống phòng thực hành ngay cuối bài để luyện nói đuổi, nghe chép chính tả và làm bài kiểm tra từ vựng.",
    ],
    recommendedFeatures: ["Trạm luyện IPA tại chỗ", "Shadowing & Dictation", "Ghi âm kiểm tra từng từ"],
    accentColor: "sky",
    aiTemplateTopic: "Luyện phát âm tiếng Anh chuẩn IPA và phản xạ nghe nói IELTS",
    targetRoute: "/app/courses",
    ctaLabel: "Vào học Tiếng Anh",
  },
  {
    id: "uc-coding-debug-drill",
    title: "Thực hành lập trình .NET / React & săn lỗi hệ thống",
    persona: "Lập trình viên / Sinh viên CNTT",
    painPoint: "Đọc lý thuyết suông nhưng khi làm dự án thật vẫn dễ vướng lỗi truy vấn N+1, rò rỉ bộ nhớ hay tranh chấp luồng.",
    workflowSteps: [
      "Mở bài học C# / React / Hệ điều hành và chuyển tới phần nhiệm vụ lập trình trích xuất từ bài.",
      "Thực hành điền mã còn thiếu, săn lỗi truy vấn / bộ nhớ và quan sát mô phỏng luồng xử lý.",
      "Lưu lại các mẫu mã chuẩn & bài học rút ra vào Sổ tay kỹ thuật cá nhân chỉ với một chạm.",
    ],
    recommendedFeatures: ["Điền mã tương tác", "Săn lỗi thực tế", "Mô phỏng kiến trúc"],
    accentColor: "violet",
    aiTemplateTopic: "Kỹ thuật tối ưu hiệu năng .NET Core, EF Core và xử lý lỗi thực tế",
    targetRoute: "/app/courses",
    ctaLabel: "Luyện lập trình ngay",
  },
  {
    id: "uc-finance-planner",
    title: "Lập kế hoạch tài chính 50/30/20 & đọc biểu đồ nến",
    persona: "Người đi làm trẻ / Nhà đầu tư cá nhân",
    painPoint: "Học khái niệm tài chính nhưng thiếu công cụ tính toán trực quan cho thu nhập và danh mục thực tế.",
    workflowSteps: [
      "Mở khóa Tài chính cá nhân hoặc Đầu tư tài chính để kích hoạt phòng tính toán & biểu đồ.",
      "Nhập mức thu nhập của bạn vào bảng 50/30/20, so sánh trả nợ Snowball với Avalanche và mô phỏng lãi kép.",
      "Lưu toàn bộ bản kế hoạch tài chính kèm số liệu chi tiết vào Sổ tay bài học.",
    ],
    recommendedFeatures: ["Bảng tính 50/30/20", "Mô phỏng lãi kép", "Đọc nến & Quản trị R:R"],
    accentColor: "emerald",
    aiTemplateTopic: "Lập kế hoạch tự do tài chính cá nhân và quản trị danh mục đầu tư",
    targetRoute: "/app/courses",
    ctaLabel: "Mở phòng Tài chính",
  },
  {
    id: "uc-feynman-exam-prep",
    title: "Ôn thi hiệu quả bằng kỹ thuật Feynman & đề kiểm tra đa dạng",
    persona: "Học viên ôn thi chứng chỉ / cuối kỳ",
    painPoint: "Cảm giác đã hiểu bài khi đọc lướt nhưng vào phòng thi lại khó trình bày mạch lạc.",
    workflowSteps: [
      "Dùng hộp tự trả lời ngay bên trong mỗi câu hỏi ẩn trước khi mở đáp án đối chiếu.",
      "Thử tự diễn đạt bằng lời của mình theo phương pháp Feynman để kiểm tra độ bao phủ ý chính.",
      "Làm bộ đề luyện tập đa dạng (trắc nghiệm, điền từ, tự luận được AI đánh giá theo tiêu chí) ngay cuối bài.",
    ],
    recommendedFeatures: ["Hộp tự trả lời tại chỗ", "Feynman Recall", "Đánh giá theo tiêu chí AI"],
    accentColor: "amber",
    aiTemplateTopic: "Phương pháp học chủ động Feynman và luyện đề kiểm tra hiệu quả",
    targetRoute: "/app/courses",
    ctaLabel: "Luyện đề ngay",
  },
  {
    id: "uc-psychology-nvc-roleplay",
    title: "Nhập vai xử lý tình huống giao tiếp & nhận diện thiên kiến tâm lý",
    persona: "Quản lý nhóm / Nhân sự / Sinh viên rèn kỹ năng mềm",
    painPoint: "Khó giữ bình tĩnh khi đồng đội trễ hạn, khách hàng phàn nàn hoặc đứng trước tình huống thao túng cảm xúc.",
    workflowSteps: [
      "Mở khóa Kỹ năng sống hoặc Tâm lý học để vào trạm mô phỏng hội thoại thực tế.",
      "Chọn cách phản hồi và xem phân tích theo 4 bước NVC (Quan sát – Cảm xúc – Nhu cầu – Đề nghị).",
      "Ghi lại mẫu câu giao tiếp giữ ranh giới tích cực vào Sổ tay để áp dụng ngay.",
    ],
    recommendedFeatures: ["Nhập vai phân nhánh", "Phân tích 4 bước NVC", "Nhận diện thiên kiến"],
    accentColor: "rose",
    aiTemplateTopic: "Nghệ thuật giao tiếp trắc ẩn (NVC) và xử lý xung đột nơi công sở",
    targetRoute: "/app/courses",
    ctaLabel: "Thử nhập vai ngay",
  },
  {
    id: "uc-daily-spaced-repetition",
    title: "Duy trì trí nhớ trên 90% chỉ với 15 phút ôn thẻ FSRS v5 mỗi sáng",
    persona: "Người bận rộn học nhiều lĩnh vực cùng lúc",
    painPoint: "Số thẻ đến hạn tồn đọng quá lớn dễ gây ngợp và nản lòng.",
    workflowSteps: [
      "Đặt giới hạn thẻ ôn tập hàng ngày (ví dụ 50–100 thẻ) trong Cài đặt (/app/settings) để tránh quá tải.",
      "Dùng phím tắt (Space để lật, phím 1–4 để đánh giá Quên/Khó/Tốt/Dễ, Z để hoàn tác) trên trang Ôn tập (/app/review).",
      "Theo dõi độ bền trí nhớ và biểu đồ phân bổ thẻ thuộc vững trên Bảng tổng quan.",
    ],
    recommendedFeatures: ["Thuật toán FSRS v5", "Giới hạn thẻ hằng ngày", "Phím tắt ôn tập nhanh"],
    accentColor: "purple",
    aiTemplateTopic: "Xây dựng thói quen ôn tập ngắt quãng Spaced Repetition 15 phút mỗi ngày",
    targetRoute: "/app/review",
    ctaLabel: "Mở trạm ôn tập",
  },
  {
    id: "uc-ai-curriculum-builder",
    title: "Khởi tạo lộ trình học tập cá nhân hóa trong 30 giây cùng AI",
    persona: "Người tự học muốn mở rộng kỹ năng mới",
    painPoint: "Chưa biết bắt đầu từ đâu khi tiếp cận một lĩnh vực hoàn toàn mới như Rust, System Design hay Kinh tế vĩ mô.",
    workflowSteps: [
      "Vào /app/courses/new, nhập chủ đề bạn quan tâm hoặc chọn từ các mẫu gợi ý có sẵn.",
      "Xem trước đề cương từng chương, bài học, câu hỏi kiểm tra và thẻ ghi nhớ trước khi lưu.",
      "Chỉnh sửa, thêm bớt bài giảng trong Studio (/app/studio/[id]) hoặc xuất gói bài học .swm chia sẻ cho nhóm.",
    ],
    recommendedFeatures: ["Trình tạo lộ trình AI", "Course Studio", "Xuất/Nhập gói .swm"],
    accentColor: "teal",
    aiTemplateTopic: "System Design cho ứng dụng web triệu người dùng (Caching, Sharding, Queue)",
    targetRoute: "/app/courses/new",
    ctaLabel: "Tạo lộ trình mới",
  },
  {
    id: "uc-shadowing-dictation-workplace",
    title: "Luyện nói đuổi & nghe chép tiếng Anh công sở mỗi ngày",
    persona: "Người đi làm cần giao tiếp quốc tế",
    painPoint: "Nghe hiểu ý chính nhưng còn ngập ngừng khi phản xạ hoặc thiếu tự tin phát âm trong cuộc họp trực tuyến.",
    workflowSteps: [
      "Mở khóa Tiếng Anh giao tiếp công sở (A2 → B1) để nghe và nói đuổi ở tốc độ 0.75x–1.0x.",
      "Làm bài nghe chép chính tả tình huống văn phòng để rèn khả năng nghe chi tiết và nắm cấu trúc câu.",
      "Khám phá sơ đồ từ vựng quanh từ khóa và lưu nhanh cụm từ vào thẻ ghi nhớ FSRS v5.",
    ],
    recommendedFeatures: ["Nói đuổi đa tốc độ", "Nghe chép chính tả", "Sơ đồ từ vựng", "Lưu thẻ FSRS nhanh"],
    accentColor: "sky",
    aiTemplateTopic: "Tiếng Anh giao tiếp công sở: Phản xạ Shadowing, Dictation và sơ đồ từ vựng",
    targetRoute: "/app/courses",
    ctaLabel: "Luyện nói đuổi",
  },
  {
    id: "uc-coding-worker-parsons",
    title: "Học lập trình tương tác & rèn tư duy giải thuật",
    persona: "Lập trình viên / Sinh viên CNTT rèn tư duy thuật toán",
    painPoint: "Nhiều bài tập trực tuyến chỉ chấm hình thức, thiếu môi trường chạy kiểm thử an toàn với bộ dữ liệu kiểm tra thực tế.",
    workflowSteps: [
      "Mở khóa Lập trình Full-Stack hoặc Hệ điều hành & Bộ nhớ ảo.",
      "Viết hàm và chạy thử trong môi trường tách biệt trên máy để đối chiếu qua các tình huống kiểm thử.",
      "Sắp xếp thứ tự dòng lệnh trong bài tập Parsons để rèn tư duy cấu trúc trước khi tự viết mã.",
    ],
    recommendedFeatures: ["Chạy thử mã tách biệt", "Kiểm thử tình huống", "Ghép dòng lệnh Parsons", "Săn lỗi"],
    accentColor: "violet",
    aiTemplateTopic: "Luyện tư duy thuật toán lập trình, chạy thử mã an toàn và giải bài tập Parsons",
    targetRoute: "/app/courses",
    ctaLabel: "Mở phòng chạy mã",
  },
  {
    id: "uc-investing-portfolio-sim",
    title: "Mô phỏng phân bổ danh mục đầu tư & nhận diện thiên kiến tâm lý",
    persona: "Nhà đầu tư cá nhân / Học viên tích lũy dài hạn",
    painPoint: "Dễ bị tâm lý đám đông cuốn theo khi thị trường tăng nóng hoặc hoảng loạn bán tháo lúc điều chỉnh do thiếu kế hoạch phân bổ.",
    workflowSteps: [
      "Dùng bảng tính Lãi kép & đầu tư định kỳ (DCA) để so sánh hiệu quả tích lũy so với gửi tiết kiệm ngân hàng.",
      "Phân bổ tỷ trọng các nhóm tài sản trong mục Mô phỏng danh mục và kiểm tra qua các kịch bản kinh tế.",
      "Trả lời câu hỏi Thiên kiến hành vi (FOMO, ngại thua lỗ) và lưu nguyên lý vào thẻ ghi nhớ FSRS v5.",
    ],
    recommendedFeatures: ["Bảng tính DCA & Lãi kép", "Mô phỏng danh mục", "Quiz Thiên kiến Hành vi", "Lưu FSRS"],
    accentColor: "emerald",
    aiTemplateTopic: "Mô phỏng phân bổ danh mục đầu tư đa tài sản và khắc phục thiên kiến tâm lý",
    targetRoute: "/app/courses",
    ctaLabel: "Mô phỏng danh mục",
  },
  {
    id: "uc-psychology-cognitive-labs",
    title: "Thực nghiệm nhận thức & đo dung lượng trí nhớ (Chunking)",
    persona: "Người muốn rèn luyện khả năng tập trung, trí nhớ & EQ",
    painPoint: "Đọc lý thuyết tâm lý học dễ thấy trừu tượng nếu thiếu trải nghiệm trực tiếp về giới hạn chú ý và trí nhớ ngắn hạn.",
    workflowSteps: [
      "Mở khóa Tâm lý học ứng dụng để trải nghiệm các bài tập phản xạ và nhận diện thiên kiến.",
      "Kiểm tra giới hạn trí nhớ ngắn hạn của bản thân và thử kỹ thuật gom nhóm thông tin (Chunking).",
      "Viết nhật ký tự phản chiếu theo mô hình CBT và lưu vào Sổ tay học tập cá nhân.",
    ],
    recommendedFeatures: ["Phòng thí nghiệm nhận thức", "Trí nhớ & Chunking", "Nhật ký CBT", "Giao tiếp NVC"],
    accentColor: "rose",
    aiTemplateTopic: "Thực nghiệm tâm lý học nhận thức: Dung lượng trí nhớ, kỹ thuật Chunking và mô hình CBT",
    targetRoute: "/app/courses",
    ctaLabel: "Khám phá bài thực nghiệm",
  },
];

export interface ResolvedRoadmapProgress {
  roadmap: LearningRoadmap;
  matchedCoursesCount: number;
  overallProgress: number;
  milestones: Array<{
    milestone: RoadmapMilestone;
    matchedCourse: CourseLike | null;
    matchedCourseId: string | null;
    progress: number;
  }>;
}

export function resolveRoadmapProgress(
  roadmap: LearningRoadmap,
  courses: CourseLike[]
): ResolvedRoadmapProgress {
  const matchedCourseIds = new Set<string>();
  // Filter out retired courses to never match them in roadmaps
  const validCourses = courses.filter((c) => !isRetiredCourse(c));

  const milestones = roadmap.milestones.map((milestone) => {
    let bestCourse: CourseLike | null = null;
    let bestScore = 0;

    for (const course of validCourses) {
      const hay = normalizeVi(`${course.title || ""} ${course.description || ""} ${course.category || ""}`);
      let hits = 0;
      for (const kw of milestone.matchKeywords) {
        if (hay.includes(normalizeVi(kw))) hits += 1;
      }
      if (hits > 0) {
        const prog = getCourseProgress(course);
        const score = hits * 1000 + (prog > 0 ? 100 + prog : 0);
        if (score > bestScore) {
          bestScore = score;
          bestCourse = course;
        }
      }
    }

    const matchedCourseId = bestCourse ? getCourseId(bestCourse) : null;
    if (matchedCourseId) matchedCourseIds.add(matchedCourseId);
    const progress = bestCourse ? getCourseProgress(bestCourse) : 0;

    return {
      milestone,
      matchedCourse: bestCourse,
      matchedCourseId,
      progress,
    };
  });

  const avg =
    milestones.length > 0
      ? Math.round(milestones.reduce((s, m) => s + m.progress, 0) / milestones.length)
      : 0;

  return {
    roadmap,
    matchedCoursesCount: matchedCourseIds.size,
    overallProgress: avg,
    milestones,
  };
}
