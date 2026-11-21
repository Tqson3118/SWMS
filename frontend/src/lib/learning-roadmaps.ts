import { normalizeVi, getCourseProgress, getCourseId, type CourseLike } from "./course-helpers";

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
    subtitle: "Từ C# cốt lõi, ASP.NET Core Minimal API đến React 19, Kiến trúc Phân tán & Production",
    category: "Công nghệ & Lập trình",
    badge: "28 Tuần · Thực chiến",
    accentGradient: "from-indigo-600 via-violet-600 to-blue-500",
    badgeClasses:
      "bg-indigo-50 text-indigo-700 border-indigo-200 dark:bg-indigo-950/60 dark:text-indigo-300 dark:border-indigo-800",
    totalDuration: "28 tuần (4 chặng)",
    targetAudience: "Sinh viên CNTT, Backend/Frontend Dev chuyển Full-Stack",
    outcome: "Xây dựng ứng dụng Web chịu tải cao với EF Core OCC, JWT RTR & React TypeScript",
    milestones: [
      {
        id: "fs-1",
        phaseLabel: "Chặng 1",
        title: "C# Hiện Đại, OOP & SQL Server",
        duration: "6 tuần",
        summary: "Nắm chắc LINQ, Async/Await, Dependency Injection và thiết kế CSDL quan hệ chuẩn hóa.",
        skills: ["C# .NET 10", "Async/Await", "SQL & Indexing"],
        deliverable: "API CRUD quản lý dữ liệu với EF Core AsNoTracking",
        matchKeywords: ["c#", "dotnet", ".net", "fullstack", "full-stack", "web api"],
        aiTopicPrompt: "Lộ trình C# .NET 10, EF Core tối ưu truy vấn và SQL Server thực chiến",
      },
      {
        id: "fs-2",
        phaseLabel: "Chặng 2",
        title: "React 19, TypeScript & State Management",
        duration: "8 tuần",
        summary: "Xây dựng UI đáp ứng nhanh, Next.js App Router, Zustand và xử lý Form/API an toàn.",
        skills: ["React 19", "TypeScript", "Next.js 15"],
        deliverable: "Dashboard học tập tương tác đa màn hình không tràn ngang",
        matchKeywords: ["fullstack", "react", "typescript", "web"],
        aiTopicPrompt: "React 19, Next.js App Router và TypeScript nâng cao qua dự án thực tế",
      },
      {
        id: "fs-3",
        phaseLabel: "Chặng 3",
        title: "Hệ điều hành, Bộ nhớ ảo & Concurrency",
        duration: "6 tuần",
        summary: "Hiểu sâu MMU/TLB, khóa đồng bộ SemaphoreSlim, Optimistic Concurrency Control.",
        skills: ["Hệ điều hành", "TLB / Paging", "Concurrency Locks"],
        deliverable: "Xử lý xung đột đồng thời HTTP 409 Keep-Both & Benchmark 30 VU",
        matchKeywords: ["he dieu hanh", "bo nho ao", "phan trang", "kien truc may tinh"],
        aiTopicPrompt: "Hệ điều hành: Bộ nhớ ảo, TLB, Đa luồng và Đồng bộ hóa tiến trình",
      },
      {
        id: "fs-4",
        phaseLabel: "Chặng 4",
        title: "Microservices, DDD, Event Sourcing & Docker",
        duration: "8 tuần",
        summary: "Kiến trúc miền DDD, Outbox Pattern, Redis/PostgreSQL và kiểm thử E2E Playwright.",
        skills: ["Microservices", "DDD & Outbox", "Docker & E2E"],
        deliverable: "Hệ thống phân tán hoàn chỉnh có CI/CD và tài liệu kiến trúc",
        matchKeywords: ["microservices", "ddd", "event sourcing", "lộ trình mẫu"],
        aiTopicPrompt: "Kiến trúc Microservices, Domain-Driven Design (DDD) và Event Sourcing",
      },
    ],
  },
  {
    id: "roadmap-ielts-mastery",
    title: "Lộ trình Chinh phục IELTS 4.0 → 7.5+",
    subtitle: "Luyện đồng bộ 4 kỹ năng Listening, Reading, Speaking, Writing kết hợp Shadowing & FSRS",
    category: "Ngoại ngữ",
    badge: "4 Phase · Audio & IPA",
    accentGradient: "from-sky-500 via-cyan-500 to-teal-500",
    badgeClasses:
      "bg-sky-50 text-sky-700 border-sky-200 dark:bg-sky-950/60 dark:text-sky-300 dark:border-sky-800",
    totalDuration: "4 giai đoạn (60+ bài nghe)",
    targetAudience: "Người mất gốc cần lấy lại nền tảng IPA hoặc mục tiêu 7.0+ học thuật",
    outcome: "Làm chủ 1.500+ collocation học thuật, phát âm IPA chuẩn & phản xạ nói trôi chảy",
    milestones: [
      {
        id: "ie-1",
        phaseLabel: "Phase 0",
        title: "Phát Âm IPA, Minimal Pairs & Ngữ Pháp Cốt Lõi",
        duration: "4 tuần",
        summary: "Chuẩn hóa 44 âm IPA, phân biệt cặp âm dễ nhầm và luyện đọc to hàng ngày.",
        skills: ["44 Âm IPA", "Minimal Pairs", "Ngữ pháp nền"],
        deliverable: "Ghi âm và đạt >85% độ khớp phát âm trong Trạm Luyện Nói",
        matchKeywords: ["ielts", "tieng anh", "english", "phat am"],
        aiTopicPrompt: "Chuẩn hóa phát âm IPA tiếng Anh, Minimal Pairs và ngữ pháp nền tảng IELTS",
      },
      {
        id: "ie-2",
        phaseLabel: "Phase 1",
        title: "Listening Dictation & Shadowing Tăng Tốc",
        duration: "8 tuần",
        summary: "Nghe chép chính tả bài WAV thực tế, bắt nối âm và từ khóa bẫy trong IELTS Listening.",
        skills: ["Nghe chép Dictation", "Shadowing 0.75x-1x", "Paraphrase"],
        deliverable: "Hoàn thành 30 bài nghe chép chính tả đạt độ chính xác >= 90%",
        matchKeywords: ["ielts", "listening"],
        aiTopicPrompt: "IELTS Listening Intensive: Kỹ thuật nghe chép chính tả và bắt từ khóa",
      },
      {
        id: "ie-3",
        phaseLabel: "Phase 2",
        title: "Academic Reading & Từ Vựng FSRS v5",
        duration: "8 tuần",
        summary: "Skimming/Scanning đoạn văn học thuật dài, biến từ vựng mới thành thẻ FSRS nhớ lâu.",
        skills: ["Skimming & Scanning", "True/False/Not Given", "Collocation FSRS"],
        deliverable: "Bộ 500+ thẻ Cloze từ vựng học thuật thuộc vững qua FSRS",
        matchKeywords: ["ielts", "reading", "vocabulary"],
        aiTopicPrompt: "IELTS Academic Reading: Chiến lược giải bài đọc và từ vựng học thuật",
      },
      {
        id: "ie-4",
        phaseLabel: "Phase 3",
        title: "Speaking Part 2-3 & Writing Task 1-2",
        duration: "8 tuần",
        summary: "Triển khai dàn ý 3 phần, thu âm tự chấm bài nói và viết luận chuẩn tiêu chí chấm điểm.",
        skills: ["Speaking Cue Card", "Task 1 Biểu đồ", "Task 2 Luận"],
        deliverable: "Hoàn thiện bộ 20 bài nói mẫu & bài luận được AI chấm Rubric",
        matchKeywords: ["ielts", "speaking", "writing"],
        aiTopicPrompt: "IELTS Speaking & Writing Band 7.5+: Cấu trúc lập luận và từ vựng ăn điểm",
      },
    ],
  },
  {
    id: "roadmap-finance-investing",
    title: "Lộ trình Tài Chính Cá Nhân & Đầu Tư Bền Vững",
    subtitle: "Làm chủ dòng tiền 50/30/20, xóa nợ thông minh, đọc BCTC và đầu tư chứng khoán dài hạn",
    category: "Kinh tế & Quản trị",
    badge: "30 Tuần · Mô phỏng số",
    accentGradient: "from-emerald-500 via-teal-500 to-lime-500",
    badgeClasses:
      "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800",
    totalDuration: "30 tuần (12w Cá nhân + 18w Đầu tư)",
    targetAudience: "Người đi làm trẻ, nhà đầu tư F0 muốn xây dựng tự do tài chính",
    outcome: "Có quỹ dự phòng 6 tháng, kế hoạch lãi kép 10 năm và quy trình lọc cổ phiếu chuẩn",
    milestones: [
      {
        id: "fn-1",
        phaseLabel: "Chặng 1",
        title: "Tâm Lý Tiền Bạc & Ngân Sách 50/30/20",
        duration: "4 tuần",
        summary: "Kiểm toán dòng tiền cá nhân, phân bổ Thiết yếu / Mong muốn / Tiết kiệm tự động.",
        skills: ["Ngân sách 50/30/20", "Quỹ dự phòng 6 tháng", "Kiểm soát chi"],
        deliverable: "Bảng phân bổ thu nhập & mục tiêu quỹ khẩn cấp trong Sổ tay",
        matchKeywords: ["tai chinh ca nhan", "ngan sach", "50/30/20"],
        aiTopicPrompt: "Quản trị tài chính cá nhân: Quy tắc 50/30/20 và xây dựng quỹ dự phòng",
      },
      {
        id: "fn-2",
        phaseLabel: "Chặng 2",
        title: "Chiến Lược Trả Nợ & Lãi Kép Chống Lạm Phát",
        duration: "8 tuần",
        summary: "So sánh Snowball vs Avalanche, tính toán lãi suất thực và mục tiêu tích lũy hưu trí.",
        skills: ["Snowball / Avalanche", "Lãi kép & Lạm phát", "Bảo hiểm rủi ro"],
        deliverable: "Kế hoạch xóa nợ & mô phỏng tăng trưởng tài sản 15 năm",
        matchKeywords: ["tai chinh ca nhan", "lai kep", "tra no"],
        aiTopicPrompt: "Chiến lược trả nợ Snowball vs Avalanche và sức mạnh lãi kép dài hạn",
      },
      {
        id: "fn-3",
        phaseLabel: "Chặng 3",
        title: "Phân Tích Cơ Bản & Đọc Báo Cáo Tài Chính",
        duration: "9 tuần",
        summary: "Đọc hiểu Bảng cân đối kế toán, Kết quả kinh doanh, Lưu chuyển tiền tệ & P/E, ROE.",
        skills: ["Đọc BCTC", "Định giá P/E, P/B", "Biên an toàn"],
        deliverable: "Phiếu phân tích 1 doanh nghiệp niêm yết theo 5 tiêu chí chất lượng",
        matchKeywords: ["dau tu tai chinh", "dau tu", "chung khoan"],
        aiTopicPrompt: "Đầu tư chứng khoán cơ bản: Đọc báo cáo tài chính và định giá doanh nghiệp",
      },
      {
        id: "fn-4",
        phaseLabel: "Chặng 4",
        title: "Biểu Đồ Nến, Quản Trị Rủi Ro & Danh Mục",
        duration: "9 tuần",
        summary: "Đọc hành vi giá trên biểu đồ nến, đặt Stop-loss/Take-profit giữ tỷ lệ R:R >= 1:2.",
        skills: ["Biểu đồ nến", "Quản trị R:R", "Tái cân bằng danh mục"],
        deliverable: "Nhật ký giao dịch kỷ luật & nguyên tắc phân bổ đa tài sản",
        matchKeywords: ["dau tu", "nen", "candlestick"],
        aiTopicPrompt: "Quản trị rủi ro đầu tư, tỷ lệ Risk:Reward và phân bổ danh mục bền vững",
      },
    ],
  },
  {
    id: "roadmap-ai-data-python",
    title: "Lộ trình AI, Học Máy & Khoa Học Dữ Liệu Python",
    subtitle: "Làm chủ Python, Toán xác suất/Giải tích, Thuật toán Học máy và Ứng dụng LLM",
    category: "Công nghệ & Lập trình",
    badge: "16 Tuần · AI & Python",
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
        title: "Lập Trình Python & Xử Lý Dữ Liệu",
        duration: "4 tuần",
        summary: "Cú pháp Python hiện đại, cấu trúc dữ liệu, xử lý tệp và vector hóa dữ liệu.",
        skills: ["Python Core", "List/Dict Comprehensions", "Clean Code"],
        deliverable: "Script Python phân tích dữ liệu học tập tự động",
        matchKeywords: ["python", "nhap mon lap trinh python"],
        aiTopicPrompt: "Nhập môn Python và xử lý dữ liệu thực tế cho người mới",
      },
      {
        id: "ai-2",
        phaseLabel: "Chặng 2",
        title: "Học Máy Cơ Bản (Supervised & Unsupervised)",
        duration: "4 tuần",
        summary: "Hồi quy, Phân loại, Phân cụm, Cross-Validation, Bias-Variance Tradeoff.",
        skills: ["Regression & Classification", "Feature Scaling", "Evaluation Metrics"],
        deliverable: "Pipeline dự đoán với đánh giá ROC-AUC & chống Data Leakage",
        matchKeywords: ["hoc may", "machine learning", "deep learning"],
        aiTopicPrompt: "Học máy cơ bản: Thuật toán Supervised, Unsupervised và đánh giá mô hình",
      },
      {
        id: "ai-3",
        phaseLabel: "Chặng 3",
        title: "Toán & Kiến Trúc Tính Toán Hiệu Năng Cao",
        duration: "4 tuần",
        summary: "Đại số tuyến tính, Đạo hàm tối ưu Gradient Descent và tối ưu bộ nhớ CPU/Cache.",
        skills: ["Gradient Descent", "Xác suất thống kê", "Kiến trúc máy tính"],
        deliverable: "Tóm tắt Feynman bản chất toán học của Backpropagation",
        matchKeywords: ["kien truc may tinh", "giai tich", "xac suat", "toan"],
        aiTopicPrompt: "Toán cho Machine Learning: Xác suất, Đại số tuyến tính và Gradient Descent",
      },
      {
        id: "ai-4",
        phaseLabel: "Chặng 4",
        title: "Ứng Dụng LLM, RAG & Đánh Giá AI Thực Chiến",
        duration: "4 tuần",
        summary: "Prompt Engineering có cấu trúc, trích xuất PDF theo trang & chấm điểm theo Rubric.",
        skills: ["LLM Prompting", "RAG & Citations", "AI Rubric Eval"],
        deliverable: "Hệ thống trợ lý học tập AI gắn trích dẫn chính xác từ PDF",
        matchKeywords: ["ai", "llm", "hoc may"],
        aiTopicPrompt: "Xây dựng ứng dụng AI với LLM, RAG và kiểm thử chất lượng phản hồi",
      },
    ],
  },
  {
    id: "roadmap-psychology-communication",
    title: "Lộ trình Tâm Lý Học Ứng Dụng & Giao Tiếp Hiệu Quả",
    subtitle: "Hiểu cơ chế nhận thức – cảm xúc, Giao tiếp Trắc ẩn (NVC) và thương lượng không bạo lực",
    category: "Chủ đề khác",
    badge: "64 Tuần · Roleplay NVC",
    accentGradient: "from-rose-500 via-pink-500 to-amber-500",
    badgeClasses:
      "bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/60 dark:text-rose-300 dark:border-rose-800",
    totalDuration: "2 khóa song hành (16w + 48w)",
    targetAudience: "Mọi người muốn nâng cao EQ, làm việc nhóm và tư duy phản biện",
    outcome: "Nhận diện bẫy tâm lý, xử lý xung đột nơi công sở và giao tiếp giữ ranh giới tích cực",
    milestones: [
      {
        id: "ps-1",
        phaseLabel: "Chặng 1",
        title: "Tự Nhận Thức & Lắng Nghe Chủ Động",
        duration: "6 tuần",
        summary: "Phân biệt sự thật quan sát vs phán xét cảm tính, thực hành phản hồi đồng cảm.",
        skills: ["Active Listening", "Quan sát khách quan", "Quản lý cảm xúc"],
        deliverable: "Hoàn thành 5 tình huống Roleplay NVC đạt điểm thấu cảm >= 9/10",
        matchKeywords: ["ky nang song", "giao tiep"],
        aiTopicPrompt: "Kỹ năng lắng nghe chủ động và quản trị cảm xúc nơi công sở",
      },
      {
        id: "ps-2",
        phaseLabel: "Chặng 2",
        title: "Giao Tiếp Phi Bạo Lực (NVC) & Góp Ý Xây Dựng",
        duration: "10 tuần",
        summary: "Áp dụng công thức 4 bước OFNR (Quan sát – Cảm xúc – Nhu cầu – Đề nghị).",
        skills: ["Mô hình NVC 4 bước", "Từ chối khéo léo", "Thương lượng"],
        deliverable: "Bộ kịch bản phản hồi mẫu cho các tình huống trễ deadline / bất đồng",
        matchKeywords: ["ky nang song", "giao tiep"],
        aiTopicPrompt: "Giao tiếp Phi Bạo Lực (NVC) và nghệ thuật góp ý không gây phòng thủ",
      },
      {
        id: "ps-3",
        phaseLabel: "Chặng 3",
        title: "Tâm Lý Học Nhận Thức & Trí Nhớ Con Người",
        duration: "24 tuần",
        summary: "Khoa học thần kinh về trí nhớ, chú ý, CBT và cách chống quên đường cong Ebbinghaus.",
        skills: ["Nhận thức & Chú ý", "CBT cơ bản", "Khoa học trí nhớ"],
        deliverable: "Áp dụng Active Recall & FSRS vào lịch học cá nhân hàng ngày",
        matchKeywords: ["tam ly hoc", "tu hoc tam ly"],
        aiTopicPrompt: "Tâm lý học nhận thức: Trí nhớ, sự chú ý và thay đổi thói quen theo CBT",
      },
      {
        id: "ps-4",
        phaseLabel: "Chặng 4",
        title: "Tâm Lý Xã Hội & Chống Thao Túng Tâm Lý",
        duration: "24 tuần",
        summary: "Phát hiện thiên kiến nhận thức (Cognitive Biases) và kỹ thuật thuyết phục lành mạnh.",
        skills: ["Cognitive Biases", "Tâm lý xã hội", "Tư duy phản biện"],
        deliverable: "Nhật ký phân tích 10 thiên kiến thường gặp trong quyết định đời sống",
        matchKeywords: ["tam ly hoc", "tam ly"],
        aiTopicPrompt: "Tâm lý học xã hội: Thiên kiến nhận thức và tư duy phản biện ứng dụng",
      },
    ],
  },
  {
    id: "roadmap-academic-law-pkm",
    title: "Lộ trình Nghiên Cứu Giáo Trình Dài & Pháp Luật Đại Cương",
    subtitle: "Đọc tài liệu PDF 300+ trang song song Sổ tay trích dẫn, ôn tập Cloze & Feynman Recall",
    category: "Pháp luật & Xã hội",
    badge: "PDF PKM · Trích dẫn",
    accentGradient: "from-amber-500 via-orange-500 to-rose-500",
    badgeClasses:
      "bg-amber-50 text-amber-800 border-amber-200 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-800",
    totalDuration: "10 tuần (Nghiên cứu sâu)",
    targetAudience: "Sinh viên Luật, Kinh tế, Nghiên cứu sinh đọc giáo trình dày",
    outcome: "Làm chủ kỹ năng bóc tách văn bản luật, trích dẫn trang PDF 2 chiều & nhớ điều luật lâu dài",
    milestones: [
      {
        id: "lw-1",
        phaseLabel: "Chặng 1",
        title: "Nhập Môn Nhà Nước & Pháp Luật",
        duration: "3 tuần",
        summary: "Đọc song song PDF giáo trình 298 trang, tô màu 5 nhóm ý (Khái niệm, Ví dụ, Câu hỏi).",
        skills: ["Đọc PDF Song Song", "Highlight 5 màu", "Hệ thống quy phạm"],
        deliverable: "Ghi chú Sổ tay gắn trích dẫn trang PDF chính xác",
        matchKeywords: ["phap luat", "giao trinh phap luat"],
        aiTopicPrompt: "Pháp luật đại cương: Bản chất Nhà nước, Quy phạm và Quan hệ pháp luật",
      },
      {
        id: "lw-2",
        phaseLabel: "Chặng 2",
        title: "Hiến Pháp, Hành Chính & Dân Sự Cốt Lõi",
        duration: "3 tuần",
        summary: "Phân biệt quyền và nghĩa vụ công dân, hợp đồng dân sự và xử lý vi phạm hành chính.",
        skills: ["Hiến pháp & Hành chính", "Hợp đồng Dân sự", "Tạo thẻ Cloze 1-click"],
        deliverable: "Bộ 30 thẻ Cloze ghi nhớ các mốc thời hiệu và điều khoản trọng tâm",
        matchKeywords: ["phap luat"],
        aiTopicPrompt: "Pháp luật Dân sự, Hành chính và phân tích tình huống hợp đồng thực tế",
      },
      {
        id: "lw-3",
        phaseLabel: "Chặng 3",
        title: "Hình Sự, Lao Động & Tranh Chấp Kinh Doanh",
        duration: "2 tuần",
        summary: "Giải quyết tình huống case study, so sánh cấu thành tội phạm và trách nhiệm pháp lý.",
        skills: ["Case Study Pháp lý", "Tự luận Rubric AI", "Feynman Recall"],
        deliverable: "Giải 5 tình huống pháp lý và đối chiếu bằng phương pháp Feynman",
        matchKeywords: ["phap luat"],
        aiTopicPrompt: "Phân tích Case Study Pháp luật Hình sự, Lao động và Kinh doanh",
      },
      {
        id: "lw-4",
        phaseLabel: "Chặng 4",
        title: "Tổng Hợp Sổ Tay Obsidian & Ôn Thi FSRS",
        duration: "2 tuần",
        summary: "Xuất toàn bộ sổ tay sang Markdown chuẩn Obsidian, tổng ôn toàn bộ thẻ đến hạn.",
        skills: ["Export Markdown PKM", "Tổng ôn FSRS v5", "Thi thử Quiz"],
        deliverable: "Đạt >= 85% bài kiểm tra tổng hợp Giáo trình Pháp luật đại cương",
        matchKeywords: ["phap luat", "tai lieu"],
        aiTopicPrompt: "Tổng ôn kiến thức Pháp luật đại cương với câu hỏi trắc nghiệm và tình huống",
      },
    ],
  },
  {
    id: "roadmap-investing-portfolio-sim",
    title: "Lộ trình Thực Chiến Đầu Tư & Quản Trị Danh Mục Đa Tài Sản",
    subtitle: "Lãi kép, tích lũy DCA, mô phỏng phân bổ danh mục mẫu và làm chủ tâm lý thị trường",
    category: "Tài chính & Đầu tư",
    badge: "18 Tuần · Portfolio Sim",
    accentGradient: "from-emerald-600 via-teal-600 to-cyan-500",
    badgeClasses:
      "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800",
    totalDuration: "18 tuần (3 chặng thực hành)",
    targetAudience: "Nhà đầu tư cá nhân, người đi làm xây dựng quỹ tự do tài chính FIRE",
    outcome: "Làm chủ máy tính DCA, phân bổ 5 lớp tài sản, stress-test danh mục và kiểm soát thiên kiến tâm lý",
    milestones: [
      {
        id: "inv-1",
        phaseLabel: "Chặng 1",
        title: "Lãi Kép, Dòng Tiền & Tích Lũy DCA Thực Chiến",
        duration: "6 tuần",
        summary: "Sức mạnh toán học của lãi kép, quy tắc 4% FIRE và kỷ luật tích lũy định kỳ bảo vệ sức mua.",
        skills: ["Lãi kép & DCA", "Quy tắc 4% FIRE", "Quản lý dòng tiền"],
        deliverable: "Bản kế hoạch tài chính cá nhân với số liệu mô phỏng DCA",
        matchKeywords: ["dau tu", "investing", "tai chinh", "lai kep", "dca", "personalfinance"],
        aiTopicPrompt: "Lộ trình Đầu tư & Quản trị Danh mục Thực chiến: Lãi kép và kỷ luật DCA",
      },
      {
        id: "inv-2",
        phaseLabel: "Chặng 2",
        title: "Xây Dựng & Tối Ưu Hóa Danh Mục Đa Tài Sản",
        duration: "6 tuần",
        summary: "Phân bổ 5 lớp tài sản (Cổ phiếu tăng trưởng, Cổ tức, Trái phiếu, Vàng, Tiền mặt) và kiểm thử Max Drawdown.",
        skills: ["Phân bổ Đa tài sản", "Max Drawdown", "Tái cân bằng"],
        deliverable: "Mô phỏng danh mục chịu đựng kịch bản suy thoái và lạm phát",
        matchKeywords: ["danh muc", "portfolio", "tai san", "dau tu"],
        aiTopicPrompt: "Xây dựng danh mục đầu tư đa tài sản và quản trị mức sụt giảm tối đa",
      },
      {
        id: "inv-3",
        phaseLabel: "Chặng 3",
        title: "Biên An Toàn, Định Giá & Tâm Lý Học Hành Vi",
        duration: "6 tuần",
        summary: "Phân tích P/E, P/B, dòng tiền tự do và khắc phục 5 thiên kiến hành vi kinh điển (FOMO, Loss Aversion).",
        skills: ["Biên an toàn", "Định giá cơ bản", "Khắc phục FOMO"],
        deliverable: "Checklist ra quyết định đầu tư và bộ thẻ FSRS thiên kiến tâm lý",
        matchKeywords: ["bien an toan", "dinh gia", "fomo", "contrarian"],
        aiTopicPrompt: "Tâm lý học hành vi nhà đầu tư và phân tích biên an toàn cổ phiếu",
      },
    ],
  },
  {
    id: "roadmap-js-problem-solving",
    title: "Lộ trình Lập Trình JavaScript & Tư Duy Giải Thuật Thực Chiến",
    subtitle: "Thực thi code trong Web Worker Sandbox, giải quyết bài toán Parsons và xử lý bất đồng bộ",
    category: "Công nghệ & Lập trình",
    badge: "16 Tuần · Web Worker",
    accentGradient: "from-amber-500 via-yellow-500 to-orange-500",
    badgeClasses:
      "bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-800",
    totalDuration: "16 tuần (3 chặng chuyên sâu)",
    targetAudience: "Lập trình viên Frontend/Fullstack muốn rèn luyện thuật toán và tư duy code sạch",
    outcome: "Viết hàm JS thuần khiết, làm chủ Web Worker sandbox, vượt qua các thử thách Parsons Problem",
    milestones: [
      {
        id: "js-1",
        phaseLabel: "Chặng 1",
        title: "Xử Lý Dữ Liệu Mảng & Tư Duy Thuật Toán",
        duration: "5 tuần",
        summary: "Biến đổi mảng với Map/Filter/Reduce, tìm kiếm nhị phân O(log n) và kỹ thuật Two Pointers.",
        skills: ["Map, Filter, Reduce", "Binary Search", "Two Pointers"],
        deliverable: "Vượt qua bài kiểm tra code JS sandbox với 100% test cases xanh",
        matchKeywords: ["javascript", "js", "mang", "two sum", "thuat toan"],
        aiTopicPrompt: "Lập trình JavaScript chuyên sâu: Xử lý mảng và thuật toán tìm kiếm",
      },
      {
        id: "js-2",
        phaseLabel: "Chặng 2",
        title: "Lập Trình Hàm, Closure & Tối Ưu Hiệu Năng",
        duration: "5 tuần",
        summary: "Cài đặt hàm Debounce, Throttle và Memoization, tìm và sửa bug rò rỉ bộ nhớ thực tế.",
        skills: ["Debounce & Throttle", "Memoization Cache", "Săn Bug JS"],
        deliverable: "Bộ thư viện tiện ích tối ưu hóa sự kiện UI tần suất cao",
        matchKeywords: ["debounce", "memoize", "throttle", "closure", "bug"],
        aiTopicPrompt: "Kỹ thuật tối ưu hiệu năng JavaScript với Debounce, Throttle và Memoize",
      },
      {
        id: "js-3",
        phaseLabel: "Chặng 3",
        title: "Bất Đồng Bộ, Pipeline & Xử Lý Lỗi Toàn Diện",
        duration: "6 tuần",
        summary: "Async/await, Promise pipeline tuần tự, cơ chế Retry exponential backoff và Deep Clone an toàn.",
        skills: ["Async Pipeline", "Promise Retry", "Deep Clone Immutability"],
        deliverable: "Pipeline xử lý dữ liệu bất đồng bộ có cơ chế tự phục hồi lỗi mạng",
        matchKeywords: ["async", "promise", "deep clone", "pipeline"],
        aiTopicPrompt: "Xử lý bất đồng bộ nâng cao trong JavaScript và cơ chế phục hồi lỗi",
      },
    ],
  },
  {
    id: "roadmap-psychology-memory-science",
    title: "Lộ trình Tâm Lý Học Nhận Thức & Khoa Học Trí Nhớ",
    subtitle: "Mini lab Stroop Test, đo Memory Span, Chunking và liệu pháp nhận thức hành vi CBT",
    category: "Tâm lý & Kỹ năng sống",
    badge: "16 Tuần · Cognitive Lab",
    accentGradient: "from-purple-600 via-pink-600 to-rose-500",
    badgeClasses:
      "bg-purple-50 text-purple-700 border-purple-200 dark:bg-purple-950/60 dark:text-purple-300 dark:border-purple-800",
    totalDuration: "16 tuần (3 chặng trải nghiệm)",
    targetAudience: "Người tự học nâng cao muốn tối ưu hóa bộ não, EQ và khả năng ghi nhớ dài hạn",
    outcome: "Đo lường thời gian phản xạ não bộ, áp dụng kỹ thuật Chunking và mô hình CBT ABC vào cuộc sống",
    milestones: [
      {
        id: "cog-1",
        phaseLabel: "Chặng 1",
        title: "Nhận Thức, Chú Ý & Can Thiệp Não Bộ",
        duration: "5 tuần",
        summary: "Mô hình Hệ thống 1 & 2 của Kahneman, trải nghiệm đo hiệu ứng Stroop ms thật và giao tiếp NVC.",
        skills: ["Hệ thống 1 & 2", "Thí nghiệm Stroop", "Giao tiếp NVC"],
        deliverable: "Báo cáo độ trễ phản xạ Stroop và bài thực hành NVC 4 bước",
        matchKeywords: ["stroop", "he thong 1", "nhan thuc", "nvc", "tam ly"],
        aiTopicPrompt: "Tâm lý học nhận thức: Hệ thống 1 và 2, hiệu ứng Stroop và giao tiếp NVC",
      },
      {
        id: "cog-2",
        phaseLabel: "Chặng 2",
        title: "Khoa Học Trí Nhớ, Chunking & Lặp Lại Ngắt Quãng",
        duration: "5 tuần",
        summary: "Đo dung lượng trí nhớ ngắn hạn (Memory Span 7±2), mở rộng bằng Chunking và Spaced Repetition.",
        skills: ["Memory Span Test", "Kỹ thuật Chunking", "Đường cong Ebbinghaus"],
        deliverable: "Thực nghiệm tăng gấp đôi dung lượng nhớ qua kỹ thuật nhóm cụm",
        matchKeywords: ["memory span", "chunking", "ebbinghaus", "tri nho"],
        aiTopicPrompt: "Khoa học trí nhớ: Kỹ thuật Chunking và thuật toán lặp lại ngắt quãng FSRS",
      },
      {
        id: "cog-3",
        phaseLabel: "Chặng 3",
        title: "Liệu Pháp CBT & Trí Tuệ Cảm Xúc Ứng Dụng",
        duration: "6 tuần",
        summary: "Tái cấu trúc nhận thức theo mô hình CBT ABC, thiết lập ranh giới cá nhân lành mạnh và tự trắc ẩn.",
        skills: ["Mô hình CBT ABC", "Thiết lập ranh giới", "Tự trắc ẩn (Self-Compassion)"],
        deliverable: "Nhật ký phản tư CBT chuyển đổi suy nghĩ tiêu cực thành hành động",
        matchKeywords: ["cbt", "ranh gioi", "tu trac an", "cam xuc"],
        aiTopicPrompt: "Liệu pháp nhận thức hành vi CBT và kỹ năng tự trắc ẩn trong đời sống",
      },
    ],
  },
];

export const STUDY_USE_CASES: StudyUseCase[] = [
  {
    id: "uc-pdf-to-course",
    title: "Biến Giáo Trình PDF 300 Trang Thành Khóa Học & Thẻ Nhớ",
    persona: "Sinh viên Đại học / Nghiên cứu sinh",
    painPoint: "Tài liệu PDF dài hàng trăm trang, đọc xong quên ngay, khó ôn tập trước kỳ thi.",
    workflowSteps: [
      "Tải tệp PDF lên (/app/courses/new) để hệ thống tự bóc tách chương mục và bài giảng.",
      "Mở chế độ Đọc Song Song: bên trái xem trang PDF gốc, bên phải ghi chú & tô màu 5 nhóm ý.",
      "Bôi đen định nghĩa quan trọng → bấm Tạo thẻ 1-click để đưa vào hàng đợi FSRS v5.",
    ],
    recommendedFeatures: ["PDF HTTP 206 Streaming", "Đọc Song Song", "Tạo thẻ 1-click"],
    accentColor: "indigo",
    aiTemplateTopic: "Phương pháp đọc sâu giáo trình đại học và tổng hợp thẻ ghi nhớ FSRS",
    targetRoute: "/app/documents",
    ctaLabel: "Mở thư viện PDF",
  },
  {
    id: "uc-ielts-speaking-lab",
    title: "Luyện Nghe Chép & Phát Âm Chuẩn IPA Ngay Trong Bài Đọc",
    persona: "Người tự học IELTS / Tiếng Anh giao tiếp",
    painPoint: "Học từ vựng toàn chữ tĩnh, không biết đọc đúng âm IPA hay tự kiểm tra phát âm.",
    workflowSteps: [
      "Mở bất kỳ bài học IELTS nào để dùng Trạm Luyện Đọc To ngay dưới bảng từ vựng/Minimal pairs.",
      "Bấm nghe mẫu giọng US / UK ở tốc độ 0.75x hoặc 1.0x, sau đó thu âm đọc thử để chấm từng từ.",
      "Cuộn xuống Phòng Thực Hành ngay cuối bài để luyện Shadowing, Dictation và làm Quiz từ vựng.",
    ],
    recommendedFeatures: ["Trạm IPA Inline", "Shadowing & Dictation", "Ghi âm chấm từng từ"],
    accentColor: "sky",
    aiTemplateTopic: "Luyện phát âm tiếng Anh chuẩn IPA và phản xạ nghe nói IELTS",
    targetRoute: "/app/courses",
    ctaLabel: "Vào học Tiếng Anh",
  },
  {
    id: "uc-coding-debug-drill",
    title: "Thực Chiến Code .NET / React & Săn Lỗi Hệ Thống Production",
    persona: "Lập trình viên / Fresher Backend-Frontend",
    painPoint: "Đọc lý thuyết lập trình suông nhưng khi vào dự án thật vẫn mắc lỗi N+1, rò rỉ bộ nhớ, race condition.",
    workflowSteps: [
      "Mở bài học C# / React / Hệ điều hành và chuyển tới phần Nhiệm vụ Code trích xuất từ bài.",
      "Thực hành chế độ Điền Code Còn Thiếu, Bug Hunter (săn lỗi SQL/EF Core/Memory Leak) và Mô phỏng luồng API.",
      "Lưu các mẫu code chuẩn & bài học rút ra vào Sổ tay kỹ thuật cá nhân chỉ với 1 cú click.",
    ],
    recommendedFeatures: ["Điền Code Tương Tác", "Bug Hunter", "Mô phỏng Kiến trúc"],
    accentColor: "violet",
    aiTemplateTopic: "Kỹ thuật tối ưu hiệu năng .NET Core, EF Core và xử lý Bug Production",
    targetRoute: "/app/courses",
    ctaLabel: "Luyện Code ngay",
  },
  {
    id: "uc-finance-planner",
    title: "Lập Kế Hoạch Tài Chính 50/30/20 & Đọc Nến Đầu Tư Thực Tế",
    persona: "Người đi làm trẻ / Nhà đầu tư cá nhân",
    painPoint: "Học khái niệm tài chính nhưng thiếu công cụ tính toán trực quan cho thu nhập và danh mục thật.",
    workflowSteps: [
      "Mở khóa Tài Chính Cá Nhân hoặc Đầu Tư Tài Chính để kích hoạt Lab Tính Toán & Biểu Đồ.",
      "Nhập thu nhập thật vào bảng 50/30/20, so sánh trả nợ Snowball vs Avalanche và mô phỏng lãi kép.",
      "Xuất toàn bộ bản kế hoạch tài chính cá nhân có số liệu chi tiết vào Sổ tay bài học.",
    ],
    recommendedFeatures: ["Máy tính 50/30/20", "Mô phỏng Lãi kép", "Đọc Nến & Quản trị R:R"],
    accentColor: "emerald",
    aiTemplateTopic: "Lập kế hoạch tự do tài chính cá nhân và quản trị danh mục đầu tư",
    targetRoute: "/app/courses",
    ctaLabel: "Mở Lab Tài chính",
  },
  {
    id: "uc-feynman-exam-prep",
    title: "Ôn Thi Cấp Tốc Bằng Kỹ Thuật Feynman & Quiz Đa Dạng",
    persona: "Học viên ôn thi chứng chỉ / cuối kỳ",
    painPoint: "Tưởng mình đã hiểu bài khi đọc lướt nhưng khi vào phòng thi lại không trình bày được.",
    workflowSteps: [
      "Sử dụng Hộp tự trả lời tại chỗ ngay bên trong mỗi câu hỏi <details> trước khi mở đáp án.",
      "Thử Tự diễn đạt Feynman từ trang trắng trong Phòng Thực Hành để kiểm tra độ bao phủ từ khóa.",
      "Làm bộ Quiz thật (Trắc nghiệm, Điền từ, Tự luận chấm Rubric AI) ngay bên dưới bài học.",
    ],
    recommendedFeatures: ["Hộp tự trả lời tại chỗ", "Feynman Recall", "Quiz Thật & AI Rubric"],
    accentColor: "amber",
    aiTemplateTopic: "Phương pháp học chủ động Feynman và luyện đề kiểm tra tốc độ cao",
    targetRoute: "/app/courses",
    ctaLabel: "Luyện Quiz ngay",
  },
  {
    id: "uc-psychology-nvc-roleplay",
    title: "Đóng Vai Xử Lý Tình Huống Giao Tiếp & Nhận Diện Bẫy Tâm Lý",
    persona: "Quản lý nhóm / Nhân sự / Sinh viên rèn kỹ năng mềm",
    painPoint: "Khó kiểm soát cảm xúc khi đồng đội trễ hạn, khách hàng phàn nàn hoặc bị thao túng tâm lý.",
    workflowSteps: [
      "Mở khóa Kỹ Năng Sống hoặc Tâm Lý Học để vào Trạm Mô Phỏng Hội Thoại Thực Tế (Roleplay).",
      "Chọn phương án phản hồi và xem phân tích 4 bước NVC (Quan sát – Cảm xúc – Nhu cầu – Đề nghị).",
      "Rút ra mẫu câu giao tiếp giữ ranh giới tích cực và lưu vào Sổ tay để áp dụng ngay.",
    ],
    recommendedFeatures: ["Roleplay Phân Nhánh", "Phân tích 4 bước NVC", "Nhận diện Thiên kiến"],
    accentColor: "rose",
    aiTemplateTopic: "Nghệ thuật giao tiếp trắc ẩn (NVC) và xử lý xung đột nơi công sở",
    targetRoute: "/app/courses",
    ctaLabel: "Thử Roleplay ngay",
  },
  {
    id: "uc-daily-spaced-repetition",
    title: "Duy Trì Trí Nhớ 90%+ Chỉ Với 15 Phút Ôn Thẻ FSRS Mỗi Sáng",
    persona: "Người bận rộn học nhiều lĩnh vực cùng lúc",
    painPoint: "Số thẻ tồn đọng quá lớn gây ngợp và dễ bỏ cuộc giữa chừng.",
    workflowSteps: [
      "Thiết lập Giới hạn thẻ ôn tập hàng ngày (ví dụ 50–100 thẻ) trong /app/settings để chống quá tải.",
      "Dùng bàn phím (Space để lật, phím 1–4 để đánh giá Quên/Khó/Tốt/Dễ, Z để hoàn tác) trên /app/review.",
      "Theo dõi Độ bền trí nhớ (Stability) và biểu đồ phân bổ thẻ thuộc vững trên Dashboard.",
    ],
    recommendedFeatures: ["Thuật toán FSRS v5", "Giới hạn thẻ chống ngợp", "Phím tắt tốc độ cao"],
    accentColor: "purple",
    aiTemplateTopic: "Xây dựng thói quen ôn tập ngắt quãng Spaced Repetition 15 phút mỗi ngày",
    targetRoute: "/app/review",
    ctaLabel: "Mở Trạm Ôn Tập",
  },
  {
    id: "uc-ai-curriculum-builder",
    title: "Khởi Tạo Lộ Trình Học Tập Cá Nhân Hóa Trong 30 Giây Với AI",
    persona: "Người tự học muốn mở rộng kỹ năng mới",
    painPoint: "Không biết bắt đầu từ đâu khi học một lĩnh vực hoàn toàn mới như Rust, System Design hay Kinh tế vĩ mô.",
    workflowSteps: [
      "Vào /app/courses/new, nhập chủ đề bất kỳ hoặc chọn từ Mẫu Use Cases có sẵn.",
      "Xem trước (Preview) đề cương từng chương, bài học, quiz và flashcard trước khi lưu.",
      "Chỉnh sửa thêm bớt bài giảng trong Studio (/app/studio/[id]) hoặc xuất gói .swm chia sẻ cho nhóm.",
    ],
    recommendedFeatures: ["AI Curriculum Wizard", "Course Studio", "Xuất/Nhập gói .swm"],
    accentColor: "teal",
    aiTemplateTopic: "System Design cho ứng dụng web triệu người dùng (Caching, Sharding, Queue)",
    targetRoute: "/app/courses/new",
    ctaLabel: "Tạo lộ trình mới",
  },
  {
    id: "uc-shadowing-dictation-workplace",
    title: "Luyện Shadowing & Dictation Tiếng Anh Đi Làm Mỗi Ngày",
    persona: "Người đi làm / Nhân viên văn phòng cần giao tiếp quốc tế",
    painPoint: "Nghe hiểu nhưng ngắc ngứ khi phản xạ, phát âm thiếu tự tin trong các buổi họp online.",
    workflowSteps: [
      "Mở khóa Tiếng Anh Giao Tiếp Thực Chiến (A2 → B1) vào trạm Shadowing nghe và nói đuổi ở tốc độ 0.75x–1.0x.",
      "Làm bài Dictation nghe chép chính tả tình huống công sở để rèn luyện đôi tai và cấu trúc câu.",
      "Khám phá sơ đồ tư duy Word Map quanh từ khóa và lưu cụm từ vào Flashcard FSRS v5 để ôn tập.",
    ],
    recommendedFeatures: ["Shadowing đa tốc độ", "Dictation nghe chép", "Sơ đồ Word Map", "Lưu FSRS 1-click"],
    accentColor: "sky",
    aiTemplateTopic: "Tiếng Anh giao tiếp công sở: Phản xạ Shadowing, Dictation và Mind Map từ vựng",
    targetRoute: "/app/courses/22222222-2222-4222-8222-222222222223/learn",
    ctaLabel: "Luyện Shadowing",
  },
  {
    id: "uc-coding-worker-parsons",
    title: "Học Code JS Thực Chiến Với Web Worker Sandbox & Parsons Problem",
    persona: "Lập trình viên / Sinh viên CNTT rèn tư duy giải thuật",
    painPoint: "Môi trường học trực tuyến chỉ chạy eval nguy hiểm hoặc chấm giả lập, không có test case thật.",
    workflowSteps: [
      "Mở khóa Lập trình JavaScript & Tư duy Giải quyết Vấn đề.",
      "Viết hàm và bấm 'Chạy kiểm thử' để Web Worker độc lập chấm điểm qua các test case thật với thời gian ms thật.",
      "Sắp xếp logic code trong bài tập Parsons Problem để rèn cấu trúc tư duy trước khi tự viết.",
    ],
    recommendedFeatures: ["Web Worker Sandbox", "Test Case Thực tế", "Parsons Problem Reorder", "Săn Bug JS"],
    accentColor: "violet",
    aiTemplateTopic: "Luyện tư duy thuật toán JavaScript, Web Worker Sandbox và giải bài tập Parsons",
    targetRoute: "/app/courses/22222222-2222-4222-8222-222222222225/learn",
    ctaLabel: "Mở Worker Sandbox",
  },
  {
    id: "uc-investing-portfolio-sim",
    title: "Mô Phỏng Phân Bổ Danh Mục Đầu Tư & Vượt Qua Thiên Kiến Tâm Lý",
    persona: "Nhà đầu tư cá nhân / Học viên tích lũy độc lập tài chính FIRE",
    painPoint: "Dễ bị FOMO khi thị trường tăng nóng hoặc hoảng loạn bán tháo ở đáy do thiếu kế hoạch phân bổ.",
    workflowSteps: [
      "Dùng Máy tính Lãi kép & DCA tương tác để so sánh sức mạnh tích lũy so với gửi tiết kiệm ngân hàng.",
      "Phân bổ tỷ trọng 5 lớp tài sản trong Mô phỏng Danh mục (Dữ liệu mẫu) và stress-test qua 3 kịch bản kinh tế.",
      "Trả lời Quiz Thiên kiến Hành vi (FOMO, Loss Aversion) và lưu nguyên lý vào Flashcard FSRS để ghi nhớ lâu dài.",
    ],
    recommendedFeatures: ["Máy tính DCA & Lãi kép", "Portfolio Simulator (Dữ liệu mẫu)", "Quiz Thiên kiến Hành vi", "Lưu FSRS"],
    accentColor: "emerald",
    aiTemplateTopic: "Mô phỏng phân bổ danh mục đầu tư đa tài sản và khắc phục thiên kiến tâm lý",
    targetRoute: "/app/courses/22222222-2222-4222-8222-222222222224/learn",
    ctaLabel: "Mô phỏng Danh mục",
  },
  {
    id: "uc-psychology-cognitive-labs",
    title: "Thí Nghiệm Nhận Thức Stroop & Đo Dung Lượng Trí Nhớ (Chunking)",
    persona: "Người tự học nâng cao / Người muốn rèn luyện não bộ & EQ",
    painPoint: "Đọc lý thuyết tâm lý học cảm giác trừu tượng, khó hình dung sự can thiệp của Hệ thống 1 và giới hạn trí nhớ.",
    workflowSteps: [
      "Tham gia Mini Lab Stroop Test: đo chính xác độ lệch mili-giây giữa chữ và màu để cảm nhận Hệ thống 1 vs Hệ thống 2.",
      "Kiểm tra Memory Span cá nhân và thực nghiệm cách nhóm cụm (Chunking) để nhân đôi khả năng nhớ.",
      "Viết nhật ký phản tư theo mô hình CBT ABC và lưu vào Sổ tay học tập cá nhân.",
    ],
    recommendedFeatures: ["Stroop Test ms thật", "Memory Span & Chunking", "Nhật ký CBT ABC", "Giao tiếp NVC"],
    accentColor: "rose",
    aiTemplateTopic: "Thực nghiệm tâm lý học nhận thức: Thí nghiệm Stroop, dung lượng trí nhớ và mô hình CBT",
    targetRoute: "/app/courses/22222222-2222-4222-8222-222222222226/learn",
    ctaLabel: "Khám phá Mini Lab",
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
  const milestones = roadmap.milestones.map((milestone) => {
    let bestCourse: CourseLike | null = null;
    let bestScore = 0;

    for (const course of courses) {
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
