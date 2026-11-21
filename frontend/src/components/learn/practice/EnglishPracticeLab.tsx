"use client";

import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  Volume2,
  Mic,
  MicOff,
  Play,
  Pause,
  RotateCcw,
  CheckCircle2,
  XCircle,
  AlertCircle,
  Sparkles,
  BookOpen,
  NotebookPen,
  Check,
  ChevronRight,
  ChevronLeft,
  HelpCircle,
  Timer,
  Layers,
  Headphones,
  MessageSquare,
  Award,
  FileText,
  Lightbulb,
  Eye,
  EyeOff,
  VolumeX,
  BookmarkPlus,
  RefreshCw,
  Send,
  Sliders,
  Type,
  Grid
} from "lucide-react";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";
import EnglishWordMapView, { type WordMapData } from "./EnglishWordMapView";

export interface PracticeModuleProps {
  courseId: string;
  courseTitle: string;
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  onSaveToNotebook: (textToAppend: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
}

export type EnglishPracticeLabProps = PracticeModuleProps;

type PracticeMode = "shadowing" | "dictation" | "speaking" | "word_map" | "quiz";
type VoiceAccent = "en-US" | "en-GB";

interface ExtractedDrill {
  id: string;
  topic: string;
  sentence: string;
  ipa: string;
  meaningVi: string;
  audioUrl: string | null;
  speakingPrompt: string;
  speakingOutline: string[];
  sampleAnswer: string;
  keyPhrases: string[];
  wordMapData?: WordMapData | null;
}

interface QuizQuestion {
  id: string;
  question: string;
  options: string[];
  correctAnswer: string;
  explanation: string;
}

// ----------------- DEFAULT TOPIC-AWARE DRILL PACKS -----------------
const DEFAULT_IELTS_DRILLS: ExtractedDrill[] = [
  {
    id: "ielts-1",
    topic: "Nghe IELTS & Môi Trường Học Thuật",
    sentence: "The university library provides free access to academic journals and digital archives.",
    ipa: "/ðə ˌjuːnɪˈvɜːsəti ˈlaɪbrəri prəˈvaɪdz friː ˈækses tuː ˌækəˈdemɪk ˈdʒɜːnlz ænd ˈdɪdʒɪtl ˈɑːkaɪvz/",
    meaningVi: "Thư viện trường đại học cung cấp quyền truy cập miễn phí vào các tạp chí học thuật và kho lưu trữ số.",
    audioUrl: null,
    speakingPrompt: "Describe an educational facility or library that you found particularly helpful during your studies.",
    speakingOutline: [
      "Tên cơ sở vật chất hoặc thư viện và nơi tọa lạc",
      "Những dịch vụ và nguồn tài liệu nổi bật nhất bạn từng sử dụng",
      "Lý do nơi này tạo cảm hứng học tập và cải thiện kết quả của bạn"
    ],
    sampleAnswer: "During my academic journey, the central university library played an indispensable role. Not only does it house cutting-edge research facilities, but it also provides a quiet study sanctuary. Utilizing its digital archives significantly streamlined my research workflow and boosted my academic output.",
    keyPhrases: ["academic journals", "digital archives", "cutting-edge research", "streamline workflow", "indispensable role", "study sanctuary"]
  },
  {
    id: "ielts-2",
    topic: "Môi trường & Biến đổi Khí hậu",
    sentence: "Renewable energy adoption has accelerated significantly over the past decade.",
    ipa: "/rɪˈnjuːəbl ˈenədʒi əˈdɒpʃn hæz əkˈseləreɪtɪd sɪɡˈnɪfɪkəntli ˈəʊvə ðə pɑːst ˈdekeɪd/",
    meaningVi: "Việc ứng dụng năng lượng tái tạo đã tăng tốc đáng kể trong thập kỷ vừa qua.",
    audioUrl: null,
    speakingPrompt: "Discuss an environmental initiative that you believe could make a substantial difference in your hometown.",
    speakingOutline: [
      "Sáng kiến môi trường đó là gì (năng lượng mặt trời, phân loại rác, xe điện)",
      "Những thách thức khi triển khai trong cộng đồng",
      "Tác động tích cực lâu dài đối với chất lượng cuộc sống người dân"
    ],
    sampleAnswer: "In my opinion, expanding renewable energy infrastructure is a game changer for sustainable urban growth. Although initial capital expenditure might be high, the long-term environmental and economic dividends far outweigh the initial costs.",
    keyPhrases: ["renewable energy", "accelerated significantly", "game changer", "sustainable growth", "capital expenditure", "long-term dividends"]
  },
  {
    id: "ielts-3",
    topic: "Đời Sống Giảng Đường & Sinh Viên",
    sentence: "Students are strongly encouraged to collaborate on cross-disciplinary research projects.",
    ipa: "/ˈstjuːdnts ɑː ˈstrɒŋli ɪnˈkʌrɪdʒd tuː kəˈlæbəreɪt ɒn krɒs ˈdɪsəplɪnəri rɪˈsɜːtʃ ˈprɒdʒekts/",
    meaningVi: "Sinh viên được khuyến khích mạnh mẽ hợp tác trong các dự án nghiên cứu liên ngành.",
    audioUrl: null,
    speakingPrompt: "Talk about a successful group project or teamwork experience you participated in.",
    speakingOutline: [
      "Dự án nói về chủ đề gì và nhóm gồm bao nhiêu thành viên",
      "Cách phân chia công việc và giải quyết bất đồng ý kiến",
      "Kết quả đạt được và bài học về tinh thần đồng đội"
    ],
    sampleAnswer: "Working on cross-disciplinary projects taught me the importance of active listening and collaborative problem-solving. By pooling diverse perspectives, our team successfully delivered an innovative solution that exceeded expectations.",
    keyPhrases: ["strongly encouraged", "cross-disciplinary", "diverse perspectives", "collaborative problem-solving", "innovative solution", "exceeded expectations"]
  },
  {
    id: "ielts-4",
    topic: "Phương Pháp Nghiên Cứu",
    sentence: "Quantitative research methodologies require rigorous statistical validation to avoid bias.",
    ipa: "/ˈkwɒntɪtətɪv rɪˈsɜːtʃ ˌmeθəˈdɒlədʒiz rɪˈkwaɪə ˈrɪɡərəs stəˈtɪstɪkl ˌvælɪˈdeɪʃn tuː əˈvɔɪd ˈbaɪəs/",
    meaningVi: "Các phương pháp nghiên cứu định lượng đòi hỏi sự kiểm định thống kê nghiêm ngặt để tránh sai lệch.",
    audioUrl: null,
    speakingPrompt: "Describe an analytical skill or method you learned that helped you solve complex problems.",
    speakingOutline: [
      "Kỹ năng hoặc phương pháp phân tích đó là gì",
      "Bạn đã rèn luyện và áp dụng nó như thế nào",
      "Tại sao sự chính xác và tư duy phản biện lại quan trọng"
    ],
    sampleAnswer: "Applying rigorous statistical validation enabled me to identify underlying trends without falling into confirmation bias. This analytical mindset has been crucial for making sound, data-backed decisions.",
    keyPhrases: ["quantitative research", "rigorous validation", "statistical significance", "confirmation bias", "analytical mindset", "data-backed decisions"]
  },
  {
    id: "ielts-5",
    topic: "Giao Tiếp Học Thuật & Thuyết Trình",
    sentence: "Presenting complex ideas concisely is an essential skill for both academic and professional success.",
    ipa: "/prɪˈzentɪŋ ˈkɒmpleks aɪˈdɪəz kənˈsaɪsli ɪz ən ɪˈsenʃl skɪl fɔː bəʊθ ˌækəˈdemɪk ænd prəˈfeʃənl səkˈses/",
    meaningVi: "Trình bày những ý tưởng phức tạp một cách cô đọng là một kỹ năng thiết yếu cho cả thành công học thuật lẫn công việc.",
    audioUrl: null,
    speakingPrompt: "Describe a presentation or speech that made a strong positive impression on you.",
    speakingOutline: [
      "Ai là người thuyết trình và chủ đề là gì",
      "Cách người đó sử dụng ngôn ngữ và dẫn dắt người nghe",
      "Điều gì làm bạn nhớ mãi và áp dụng vào bản thân"
    ],
    sampleAnswer: "The speaker had a remarkable ability to present complex concepts concisely. Instead of relying on technical jargon, they used relatable analogies and clear storytelling, which kept the entire audience thoroughly engaged.",
    keyPhrases: ["presenting complex ideas", "concisely and clearly", "essential skill", "relatable analogies", "audience engagement", "mastering storytelling"]
  }
];

const DEFAULT_TECH_FINANCE_DRILLS: ExtractedDrill[] = [
  {
    id: "tech-1",
    topic: "Công Nghệ & Trí Tuệ Nhân Tạo",
    sentence: "Artificial intelligence is fundamentally transforming algorithmic trading and risk mitigation.",
    ipa: "/ˌɑːtɪˈfɪʃl ɪnˈtelɪdʒəns ɪz ˌfʌndəˈmentəli trænsˈfɔːmɪŋ ˌælɡəˈrɪðmɪk ˈtreɪdɪŋ ænd rɪsk ˌmɪtɪˈɡeɪʃn/",
    meaningVi: "Trí tuệ nhân tạo đang thay đổi căn bản hoạt động giao dịch theo thuật toán và giảm thiểu rủi ro.",
    audioUrl: null,
    speakingPrompt: "Describe a software tool or technological solution that revolutionized your daily productivity.",
    speakingOutline: [
      "Công cụ công nghệ đó là gì và bạn biết đến nó từ khi nào",
      "Cách bạn tích hợp nó vào quy trình công việc hằng ngày",
      "Lợi ích lớn nhất về hiệu suất và tiết kiệm thời gian mà nó đem lại"
    ],
    sampleAnswer: "Integrating modern software tools has been a genuine game changer for my workflow. It drastically minimizes repetitive overhead, allowing me to focus on strategic thinking and high-impact execution.",
    keyPhrases: ["artificial intelligence", "algorithmic trading", "risk mitigation", "productivity boost", "game changer", "streamline operations"]
  },
  {
    id: "tech-2",
    topic: "Khởi Nghiệp & Đầu Tư Tài Chính",
    sentence: "The startup secured five million dollars in seed funding from prominent venture capitalists.",
    ipa: "/ðə ˈstɑːtʌp sɪˈkjʊəd faɪv ˈmɪljən ˈdɒləz ɪn siːd ˈfʌndɪŋ frɒm ˈprɒmɪnənt ˈventʃə ˈkæpɪtəlɪsts/",
    meaningVi: "Công ty khởi nghiệp đã huy động thành công 5 triệu đô la trong vòng gọi vốn hạt giống từ các nhà đầu tư mạo hiểm hàng đầu.",
    audioUrl: null,
    speakingPrompt: "Discuss an entrepreneurial business or investment idea that you find highly promising.",
    speakingOutline: [
      "Mô hình kinh doanh hoặc cơ hội đầu tư đó thuộc lĩnh vực nào",
      "Yếu tố tạo nên lợi thế cạnh tranh cốt lõi",
      "Chiến lược quản trị vốn và giảm thiểu rủi ro"
    ],
    sampleAnswer: "Securing capital in a competitive market requires a rock-solid value proposition. Startups that prioritize unit economics and product-market fit are far more likely to generate sustainable returns for their investors.",
    keyPhrases: ["seed funding", "venture capitalists", "value proposition", "unit economics", "product-market fit", "competitive advantage"]
  },
  {
    id: "tech-3",
    topic: "Kiến Trúc Đám Mây & Phần Mềm",
    sentence: "Cloud infrastructure offers scalable and cost-effective solutions for modern distributed systems.",
    ipa: "/klaʊd ˈɪnfrəstrʌktʃə ˈɒfəz ˈskeɪləbl ænd kɒst ɪˈfektɪv səˈluːʃnz fɔː ˈmɒdn dɪˈstrɪbjuːtɪd ˈsɪstəmz/",
    meaningVi: "Hạ tầng đám mây cung cấp các giải pháp có khả năng mở rộng linh hoạt và tiết kiệm chi phí cho hệ thống phân tán hiện đại.",
    audioUrl: null,
    speakingPrompt: "Talk about an important technical challenge you encountered and how you solved it.",
    speakingOutline: [
      "Hệ thống hoặc bài toán gặp trục trặc gì (hiệu năng, chi phí, nghẽn mạng)",
      "Giải pháp kiến trúc nào được lựa chọn để giải quyết",
      "Kết quả thu được về độ ổn định và chi phí vận hành"
    ],
    sampleAnswer: "Migrating to a scalable cloud infrastructure eliminated our performance bottlenecks. Not only did it improve uptime, but it also delivered substantial cost-effective benefits through dynamic auto-scaling.",
    keyPhrases: ["cloud infrastructure", "scalable solution", "cost-effective", "distributed systems", "performance bottleneck", "dynamic auto-scaling"]
  },
  {
    id: "tech-4",
    topic: "Quản Trị Danh Mục Đầu Tư",
    sentence: "Diversification remains a cornerstone strategy for managing market volatility and preserving capital.",
    ipa: "/daɪˌvɜːsɪfɪˈkeɪʃn rɪˈmeɪnz ə ˈkɔːnəstəʊn ˈstrætədʒi fɔː ˈmænɪdʒɪŋ ˈmɑːkɪt ˌvɒləˈtɪləti ænd prɪˈzɜːvɪŋ ˈkæpɪtl/",
    meaningVi: "Đa dạng hóa danh mục vẫn là chiến lược nền tảng để kiểm soát biến động thị trường và bảo toàn vốn.",
    audioUrl: null,
    speakingPrompt: "Share your view on how an individual investor should handle emotional discipline during market downturns.",
    speakingOutline: [
      "Tác động của tâm lý sợ hãi hoặc tham lam đối với quyết định đầu tư",
      "Tầm quan trọng của kỷ luật và chiến lược phân bổ tài sản",
      "Lời khuyên thiết thực để giữ vững bình tĩnh trong giai đoạn khó khăn"
    ],
    sampleAnswer: "Portfolio diversification is indispensable when navigating market volatility. Maintaining emotional discipline and adhering strictly to risk parameters prevents catastrophic drawdowns during severe corrections.",
    keyPhrases: ["diversification", "cornerstone strategy", "market volatility", "preserving capital", "emotional discipline", "risk parameters"]
  },
  {
    id: "tech-5",
    topic: "Phát Triển Sản Phẩm Nhanh (Agile)",
    sentence: "Our engineering team adopted agile methodologies to shorten release cycles and enhance feedback loops.",
    ipa: "/ˈaʊər ˌendʒɪˈnɪərɪŋ tiːm əˈdɒptɪd ˈædʒaɪl ˌmeθəˈdɒlədʒiz tuː ˈʃɔːtn rɪˈliːs ˈsaɪklz ænd ɪnˈhɑːns ˈfiːdbæk luːps/",
    meaningVi: "Đội ngũ kỹ thuật của chúng tôi đã áp dụng các phương pháp Agile để rút ngắn chu kỳ phát hành và tăng cường vòng phản hồi người dùng.",
    audioUrl: null,
    speakingPrompt: "Describe an effective workflow methodology that helped your team work more harmoniously.",
    speakingOutline: [
      "Quy trình hoặc phương pháp làm việc được áp dụng (Scrum, Kanban, Sprint)",
      "Những cải thiện trực tiếp về tiến độ và sự minh bạch",
      "Cách duy trì năng lượng và động lực làm việc cho các thành viên"
    ],
    sampleAnswer: "Transitioning to agile development was truly transformative. Shorter iteration cycles allowed us to validate product hypotheses rapidly while fostering transparent collaboration across all stakeholders.",
    keyPhrases: ["engineering team", "agile methodologies", "release cycles", "feedback loops", "iteration cycles", "transparent collaboration"]
  }
];

const DEFAULT_DAILY_DRILLS: ExtractedDrill[] = [
  {
    id: "daily-1",
    topic: "Giao Tiếp Công Sở Hằng Ngày",
    sentence: "Could you please give me a quick update on the project milestones before our team sync?",
    ipa: "/kʊd juː pliːz ɡɪv miː ə kwɪk ˌʌpˈdeɪt ɒn ðə ˈprɒdʒekt ˈmaɪlstəʊnz bɪˈfɔːr ˈaʊə tiːm sɪŋk/",
    meaningVi: "Bạn có thể vui lòng cập nhật nhanh các mốc quan trọng của dự án trước buổi họp nhóm được không?",
    audioUrl: null,
    speakingPrompt: "Describe how you prepare for and conduct an effective update meeting with colleagues.",
    speakingOutline: [
      "Mục tiêu chính của cuộc họp cập nhật công việc",
      "Cách sắp xếp thông tin ngắn gọn, tập trung vào trọng tâm",
      "Cách thống nhất phương án xử lý các đầu việc bị chậm"
    ],
    sampleAnswer: "In professional communication, brevity is essential. Before any team sync, I prioritize key milestones and flag potential blockers so that everyone leaves with crystal-clear action items.",
    keyPhrases: ["quick update", "project milestones", "team sync", "brevity is essential", "flag potential blockers", "crystal-clear action items"]
  },
  {
    id: "daily-2",
    topic: "Thương Lượng & Lịch Hẹn",
    sentence: "I would really appreciate it if we could reschedule our meeting to Thursday afternoon.",
    ipa: "/aɪ wʊd ˈrɪəli əˈpriːʃieɪt ɪt ɪf wiː kʊd ˌriːˈʃedjuːl ˈaʊə ˈmiːtɪŋ tuː ˈθɜːzdeɪ ˌɑːftəˈnuːn/",
    meaningVi: "Tôi rất cảm kích nếu chúng ta có thể dời lịch họp sang chiều thứ Năm.",
    audioUrl: null,
    speakingPrompt: "Talk about a time you had to renegotiate a schedule or deadline politely.",
    speakingOutline: [
      "Lý do đột xuất khiến bạn phải thay đổi lịch trình",
      "Cách bạn mở lời để thể hiện sự tôn trọng thời gian của đối phương",
      "Giải pháp bù đắp hoặc phương án thay thế bạn đưa ra"
    ],
    sampleAnswer: "Polite scheduling requests rely on clear explanations and proactive alternatives. By acknowledging the inconvenience and offering flexible slots, we can maintain smooth working relationships without friction.",
    keyPhrases: ["really appreciate", "reschedule meeting", "polite request", "proactive alternatives", "acknowledge inconvenience", "smooth relationships"]
  },
  {
    id: "daily-3",
    topic: "Đưa Ra Ý Kiến & Đóng Góp",
    sentence: "From my perspective, focusing on user experience will give us a strong competitive edge.",
    ipa: "/frɒm maɪ pəˈspektɪv ˈfəʊkəsɪŋ ɒn ˈjuːzər ɪkˈspɪəriəns wɪl ɡɪv ʌs ə strɒŋ kəmˈpetətɪv edʒ/",
    meaningVi: "Theo quan điểm của tôi, việc tập trung vào trải nghiệm người dùng sẽ mang lại lợi thế cạnh tranh vượt trội.",
    audioUrl: null,
    speakingPrompt: "Explain why user experience is critical when designing any modern product or service.",
    speakingOutline: [
      "Một sản phẩm có trải nghiệm người dùng tuyệt vời mà bạn yêu thích",
      "Những sai lầm thường gặp khi nhà phát triển quá chú trọng tính năng mà quên trải nghiệm",
      "Lợi ích kinh doanh khi khách hàng cảm thấy dễ sử dụng và thoải mái"
    ],
    sampleAnswer: "From my perspective, intuitive design drives customer loyalty. When a service feels effortless to use, it creates a powerful competitive edge that competitors cannot easily duplicate.",
    keyPhrases: ["from my perspective", "user experience", "competitive edge", "intuitive design", "customer loyalty", "effortless to use"]
  },
  {
    id: "daily-4",
    topic: "Giải Quyết Vấn Đề (Problem Solving)",
    sentence: "Let us break down the complex problem into smaller, manageable action steps.",
    ipa: "/let ʌs breɪk daʊn ðə ˈkɒmpleks ˈprɒbləm ˈɪntuː ˈsmɔːlə ˈmænɪdʒəbl ˈækʃn steps/",
    meaningVi: "Chúng ta hãy chia nhỏ vấn đề phức tạp thành các bước hành động cụ thể, dễ quản lý hơn.",
    audioUrl: null,
    speakingPrompt: "Describe a stressful situation at work or school that you resolved by breaking it down.",
    speakingOutline: [
      "Tình huống quá tải hoặc rắc rối bạn gặp phải lúc đó",
      "Cách bạn giữ bình tĩnh và phân loại các đầu việc ưu tiên",
      "Kết quả cuối cùng và bài học về tư duy giải quyết vấn đề"
    ],
    sampleAnswer: "Whenever I face an overwhelming task, breaking it down into manageable action steps restores momentum. Tackling one discrete challenge at a time keeps anxiety at bay and guarantees steady progress.",
    keyPhrases: ["break down problem", "manageable action steps", "restore momentum", "discrete challenge", "steady progress", "keep anxiety at bay"]
  },
  {
    id: "daily-5",
    topic: "Động Viên & Ghi Nhận Thành Tựu",
    sentence: "Everyone contributed tremendously to this milestone, and your hard work truly paid off.",
    ipa: "/ˈevriwʌn kənˈtrɪbjuːtɪd trəˈmendəsli tuː ðɪs ˈmaɪlstəʊn ænd jɔː hɑːd wɜːk ˈtruːli peɪd ɒf/",
    meaningVi: "Mọi người đều đã đóng góp to lớn cho mốc thành công này, và sự chăm chỉ của các bạn thực sự đã được đền đáp.",
    audioUrl: null,
    speakingPrompt: "Describe how a compliment or words of encouragement from a mentor inspired you to do better.",
    speakingOutline: [
      "Ai đã khen ngợi bạn và vào thời điểm nào",
      "Cảm xúc của bạn khi những nỗ lực được ghi nhận xứng đáng",
      "Sự khích lệ đó đã thúc đẩy bạn tiến bộ ra sao trong những dự án tiếp theo"
    ],
    sampleAnswer: "Recognizing individual contributions fosters psychological safety and boosts team morale. When leaders genuinely appreciate hard work, team members feel motivated to continually deliver their best work.",
    keyPhrases: ["contributed tremendously", "truly paid off", "psychological safety", "boost team morale", "genuine appreciation", "deliver best work"]
  }
];

// Helper: Normalize word for comparison
function cleanWord(w: string): string {
  return w.toLowerCase().replace(/[^a-z0-9']/g, "").trim();
}

// Helper: Levenshtein distance
function levenshteinDistance(s1: string, s2: string): number {
  const m = s1.length;
  const n = s2.length;
  const dp: number[][] = Array.from({ length: m + 1 }, () => Array(n + 1).fill(0));
  for (let i = 0; i <= m; i++) dp[i][0] = i;
  for (let j = 0; j <= n; j++) dp[0][j] = j;
  for (let i = 1; i <= m; i++) {
    for (let j = 1; j <= n; j++) {
      if (s1[i - 1] === s2[j - 1]) {
        dp[i][j] = dp[i - 1][j - 1];
      } else {
        dp[i][j] = 1 + Math.min(dp[i - 1][j], dp[i][j - 1], dp[i - 1][j - 1]);
      }
    }
  }
  return dp[m][n];
}

function buildDefaultWordMap(drill: ExtractedDrill): WordMapData {
  const words = drill.sentence.split(/\s+/).map(cleanWord).filter(w => w.length >= 4);
  const central = words[0] || "collaborate";
  return {
    centralWord: central,
    ipa: drill.ipa || "",
    meaningVi: drill.meaningVi || "Từ vựng trọng tâm trong bài học",
    collocations: (drill.keyPhrases || []).slice(0, 3).map((phrase, i) => ({
      text: phrase,
      meaningVi: `Cụm từ then chốt #${i + 1} cần nhớ`,
      example: drill.sentence
    })),
    phrasalVerbs: [
      { text: `focus on ${central}`, meaningVi: "Tập trung vào", example: `We must focus on ${central} to improve results.` },
      { text: `carry out`, meaningVi: "Tiến hành, thực hiện", example: `We need to carry out effective communication.` }
    ],
    synonyms: [
      { text: words[1] || "develop", meaningVi: "Từ vựng liên quan trong ngữ cảnh", example: drill.sentence }
    ],
    contextExamples: [
      { sentence: drill.sentence, meaningVi: drill.meaningVi }
    ]
  };
}

// Helper: Smart text & audio extractor
function extractPracticeContent(
  markdown: string,
  lessonTitle: string,
  courseTitle: string
): { drills: ExtractedDrill[]; detectedTopic: string } {
  // 1. Detect audio references
  const audioUrls: string[] = [];
  const audioRegex = /(?:https?:\/\/[^\s)"]+|[A-Za-z0-9_./-]+\.(?:wav|mp3))/gi;
  let audioMatch;
  while ((audioMatch = audioRegex.exec(markdown)) !== null) {
    let raw = audioMatch[0].trim().replace(/[)"\]]+$/, "");
    if (raw.startsWith("http://") || raw.startsWith("https://")) {
      audioUrls.push(raw);
    } else if (raw.startsWith("/api/v1/document-assets/")) {
      audioUrls.push(raw);
    } else {
      const cleanPath = raw.replace(/^(\.\/|\/)+/, "");
      audioUrls.push(`/api/v1/document-assets/IELTS/${cleanPath}`);
    }
  }

  // 2. Detect topic from text
  const combinedText = `${courseTitle} ${lessonTitle} ${markdown.slice(0, 1500)}`.toLowerCase();
  let topicCategory: "ielts" | "tech" | "daily" = "daily";
  let topicDisplay = "Giao tiếp công sở & Đời sống";

  if (
    combinedText.includes("ielts") ||
    combinedText.includes("cambridge") ||
    combinedText.includes("academic") ||
    combinedText.includes("listening") ||
    combinedText.includes("reading") ||
    combinedText.includes("writing") ||
    combinedText.includes("speaking")
  ) {
    topicCategory = "ielts";
    topicDisplay = "Luyện thi IELTS & Tiếng Anh Học Thuật";
  } else if (
    combinedText.includes("finance") ||
    combinedText.includes("stock") ||
    combinedText.includes("crypto") ||
    combinedText.includes("trading") ||
    combinedText.includes("tech") ||
    combinedText.includes("code") ||
    combinedText.includes("software") ||
    combinedText.includes("algorithm") ||
    combinedText.includes("data")
  ) {
    topicCategory = "tech";
    topicDisplay = "Tiếng Anh Công Nghệ & Tài Chính";
  }

  // 3. Extract custom sentences and vocabulary from Markdown
  const extractedSentences: Array<{ text: string; meaningVi?: string; ipa?: string }> = [];

  // 3a. Table rows with IPA or vocabulary (e.g. | Chữ | /eɪ/ | word | or | /æ/ | happy, angry | ...)
  const mdLines = markdown.split("\n");
  for (const line of mdLines) {
    const trimmed = line.trim();
    if (!trimmed.startsWith("|") || !trimmed.endsWith("|")) continue;
    const cells = trimmed.split("|").map(c => c.trim()).filter(Boolean);
    if (cells.length < 2) continue;
    if (cells.some(c => /^:?-+:?$/.test(c))) continue;

    const ipaCellIdx = cells.findIndex(c => /\/[^/]+\//.test(c));
    if (ipaCellIdx !== -1) {
      const rawIpa = cells[ipaCellIdx].match(/\/[^/]+\//)?.[0] || "";
      const otherCells = cells.filter((_, idx) => idx !== ipaCellIdx);
      const enCandidates = otherCells.flatMap(c => {
        const clean = c.replace(/[*_`]/g, "").trim();
        return clean.split(/[,;·•\/]/).map(w => w.trim()).filter(w => /^[A-Za-z\s'-]{2,40}$/.test(w));
      });
      const noteCell = otherCells.find(c => /[à-ỹÀ-Ỹ]/.test(c)) || "";
      if (enCandidates.length > 0) {
        const word = enCandidates[0];
        if (!extractedSentences.some(s => s.text.toLowerCase() === word.toLowerCase())) {
          extractedSentences.push({
            text: word,
            meaningVi: noteCell ? `${word}: ${noteCell}` : `Từ vựng phát âm: ${word} (${rawIpa})`,
            ipa: rawIpa,
          });
        }
      }
    }
  }

  // 3b. Minimal pairs: "Minimal pairs ...: bad/bag · pat/pad" or "fat/vat"
  const minimalPairPattern = /(?:minimal\s*pairs?|cặp\s*từ\s*tối\s*thiểu)[^:\n]*:\s*([^\n]+)/gi;
  let mpm;
  while ((mpm = minimalPairPattern.exec(markdown)) !== null) {
    const rawPairs = mpm[1];
    const pairs = rawPairs.split(/[·•,;]/).map(p => p.trim());
    for (const pair of pairs) {
      const cleanPair = pair.replace(/[*_`]/g, "").trim();
      if (/^[A-Za-z]+\s*\/\s*[A-Za-z]+$/.test(cleanPair)) {
        if (!extractedSentences.some(s => s.text === cleanPair)) {
          extractedSentences.push({
            text: cleanPair,
            meaningVi: `Cặp từ tối thiểu phân biệt âm: ${cleanPair}`,
            ipa: "",
          });
        }
      }
    }
  }

  // 3c. Self-test lists: e.g. "đọc to 20 lần: "think, thank, the, them, father""
  const selfTestPattern = /đọc\s*to\s*(?:\d+\s*lần)?\s*:\s*["“']?([^"”\n]{10,200})["”']?/gi;
  let stm;
  while ((stm = selfTestPattern.exec(markdown)) !== null) {
    const listStr = stm[1].trim();
    const items = listStr.split(/[,;·•]/).map(w => w.trim().replace(/[*_`]/g, "")).filter(w => /^[A-Za-z\s'-]{2,40}$/.test(w));
    for (const item of items) {
      if (!extractedSentences.some(s => s.text.toLowerCase() === item.toLowerCase())) {
        extractedSentences.push({
          text: item,
          meaningVi: `Luyện tập phát âm từ: ${item}`,
          ipa: "",
        });
      }
    }
  }

  // 3d. Match bold phrases or vocabulary lines: **Word** (/ipa/): meaning
  const boldVocabPattern = /\*\*([A-Za-z0-9\s'-]{2,40})\*\*\s*(?:\(([^)]+)\))?\s*[-:–—]\s*([^\n]+)/g;
  let vm;
  while ((vm = boldVocabPattern.exec(markdown)) !== null) {
    const word = vm[1].trim();
    const potentialIpa = vm[2]?.trim() || "";
    const meaning = vm[3].trim().replace(/\*+/g, "");
    if (/^[A-Za-z\s'-]+$/.test(word) && word.length > 3) {
      if (!extractedSentences.some(s => s.text.toLowerCase() === word.toLowerCase())) {
        extractedSentences.push({
          text: `The term "${word}" is commonly used to describe ${meaning.toLowerCase().slice(0, 70)}.`,
          meaningVi: `Thuật ngữ "${word}": ${meaning}`,
          ipa: potentialIpa ? (potentialIpa.startsWith("/") ? potentialIpa : `/${potentialIpa}/`) : ""
        });
      }
    }
  }

  // 3e. Match bullet points with full English sentences: - "Sentence." - Nghĩa
  const bulletPattern = /(?:^|\n)[-*•]\s*["“']?([A-Z][a-zA-Z0-9\s,;:'"–—-]{18,180}[.!?])["”']?(?:\s*[-–—:]\s*([^\n]+))?/g;
  let bm;
  while ((bm = bulletPattern.exec(markdown)) !== null) {
    const text = bm[1].trim();
    const meaningVi = bm[2]?.trim();
    if (text.split(" ").length >= 4 && !extractedSentences.some(s => s.text === text)) {
      extractedSentences.push({ text, meaningVi });
    }
  }

  // 4. Assemble drill list (expand up to 20 drills, removing the .slice(0, 3) restriction)
  const baseDefaultPack =
    topicCategory === "ielts"
      ? DEFAULT_IELTS_DRILLS
      : topicCategory === "tech"
      ? DEFAULT_TECH_FINANCE_DRILLS
      : DEFAULT_DAILY_DRILLS;

  const resultDrills: ExtractedDrill[] = [];

  const maxDrills = Math.min(20, Math.max(5, extractedSentences.length));
  extractedSentences.slice(0, maxDrills).forEach((item, idx) => {
    const matchedAudio = audioUrls[idx] || null;
    resultDrills.push({
      id: `extracted-${idx + 1}`,
      topic: `${lessonTitle || topicDisplay} (Bài học)`,
      sentence: item.text,
      ipa: item.ipa || "",
      meaningVi: item.meaningVi || "Nội dung trích xuất trực tiếp từ bài giảng hiện tại.",
      audioUrl: matchedAudio,
      speakingPrompt: `How would you explain the significance or pronunciation of "${item.text.slice(0, 45)}" in this lesson?`,
      speakingOutline: [
        "Ý nghĩa cốt lõi của khái niệm hoặc câu nói trong bài học",
        "Ví dụ minh họa thực tế mà bạn từng quan sát hoặc áp dụng",
        "Góc nhìn cá nhân và bài học rút ra"
      ],
      sampleAnswer: `In the context of ${lessonTitle || "this topic"}, mastering "${item.text.slice(0, 30)}" with accurate pronunciation and intonation is vital. It lays the groundwork for structured thinking and clear expression. Applying it consistently will yield substantial improvements.`,
      keyPhrases: [
        item.text.split(" ").slice(0, 3).join(" ").toLowerCase(),
        "vital concept",
        "structured thinking",
        "clear expression",
        "yield substantial improvements"
      ]
    });
  });

  // Supplement with high-quality defaults so there are always at least 5 drills
  baseDefaultPack.forEach((drill, idx) => {
    if (resultDrills.length < 5) {
      const audioForDefault = audioUrls[resultDrills.length] || drill.audioUrl;
      resultDrills.push({
        ...drill,
        audioUrl: audioForDefault
      });
    }
  });

  return { drills: resultDrills, detectedTopic: topicDisplay };
}

// Generate dynamic Mini-Quiz from drills
function generateQuizFromDrills(drills: ExtractedDrill[]): QuizQuestion[] {
  const quizList: QuizQuestion[] = [];

  drills.forEach((drill, idx) => {
    const words = drill.sentence
      .split(/\s+/)
      .map(w => cleanWord(w))
      .filter(w => w.length >= 5);
    const targetWord = words[Math.min(idx, words.length - 1)] || "essential";

    // Fill-in-the-blank style
    const blankSentence = drill.sentence.replace(
      new RegExp(`\\b${targetWord}\\b`, "i"),
      "______"
    );

    // Generate plausible distractors
    const distractors = [
      targetWord + "ly",
      targetWord + "ing",
      targetWord.endsWith("e") ? targetWord.slice(0, -1) + "tion" : targetWord + "ness"
    ].filter(d => d !== targetWord);

    const rawOptions = Array.from(new Set([targetWord, ...distractors, "effective", "substantial"])).slice(0, 4);
    // Shuffle options
    const options = [...rawOptions].sort(() => 0.5 - Math.random());

    quizList.push({
      id: `quiz-${idx + 1}`,
      question: `Điền từ thích hợp nhất vào chỗ trống:\n"${blankSentence}"`,
      options,
      correctAnswer: targetWord,
      explanation: `Từ chính xác là "${targetWord}". Dịch cả câu: "${drill.meaningVi}". Hãy chú ý ngữ pháp và sự hòa hợp thì/từ loại trong câu.`
    });
  });

  return quizList.slice(0, 5);
}

export function EnglishPracticeLab({
  courseId,
  courseTitle,
  lessonId,
  lessonTitle,
  lessonMarkdown,
  onSaveToNotebook,
  onCreateFlashcardQuick
}: PracticeModuleProps) {
  // Extract adaptive drills
  const { drills, detectedTopic } = useMemo(
    () => extractPracticeContent(lessonMarkdown, lessonTitle, courseTitle),
    [lessonMarkdown, lessonTitle, courseTitle]
  );

  const quizQuestions = useMemo(() => generateQuizFromDrills(drills), [drills]);

  // Backend Practice Activities & Attempts state
  const [dbActivities, setDbActivities] = useState<any[]>([]);
  const [practiceAttempts, setPracticeAttempts] = useState<any[]>([]);
  const [isLoadingBackend, setIsLoadingBackend] = useState(false);
  const [isMicPermissionDenied, setIsMicPermissionDenied] = useState(false);

  // Check SpeechRecognition support on mount
  useEffect(() => {
    if (typeof window !== "undefined") {
      const win = window as any;
      const hasRec = !!(win.SpeechRecognition || win.webkitSpeechRecognition);
      setSpeechRecSupported(hasRec);
    }
  }, []);

  // Load Practice Activities & Attempts from backend
  useEffect(() => {
    let isMounted = true;
    async function loadBackendPractice() {
      setIsLoadingBackend(true);
      try {
        const headers = getClientRequestHeaders();
        const actUrl = `/api/v1/practice/activities?courseId=${encodeURIComponent(courseId)}${lessonId ? `&lessonId=${encodeURIComponent(lessonId)}` : ""}`;
        const attUrl = `/api/v1/practice/attempts?courseId=${encodeURIComponent(courseId)}${lessonId ? `&lessonId=${encodeURIComponent(lessonId)}` : ""}&limit=25`;

        const [actRes, attRes] = await Promise.all([
          fetch(actUrl, { headers }).catch(() => null),
          fetch(attUrl, { headers }).catch(() => null),
        ]);

        if (actRes && actRes.ok) {
          const actData = await actRes.json();
          if (isMounted && Array.isArray(actData)) {
            setDbActivities(actData);
          }
        }
        if (attRes && attRes.ok) {
          const attData = await attRes.json();
          if (isMounted && Array.isArray(attData)) {
            setPracticeAttempts(attData);
          }
        }
      } catch {
        // ignore
      } finally {
        if (isMounted) setIsLoadingBackend(false);
      }
    }
    loadBackendPractice();
    return () => {
      isMounted = false;
    };
  }, [courseId, lessonId]);

  // Merge backend activities if present
  const effectiveDrills: ExtractedDrill[] = useMemo(() => {
    if (dbActivities.length > 0) {
      const shadowingAct = dbActivities.find(a => a.type === "shadowing");
      const dictationAct = dbActivities.find(a => a.type === "dictation");
      const pronunciationAct = dbActivities.find(a => a.type === "pronunciation");
      const wordMapAct = dbActivities.find(a => a.type === "word_map");

      let parsedWordMap: WordMapData | null = null;
      if (wordMapAct?.configJson) {
        try {
          parsedWordMap = JSON.parse(wordMapAct.configJson);
        } catch {}
      }

      const parseCfg = (jsonStr?: string) => {
        try { return jsonStr ? JSON.parse(jsonStr) : {}; } catch { return {}; }
      };

      const shadCfg = parseCfg(shadowingAct?.configJson);
      const dictCfg = parseCfg(dictationAct?.configJson);
      const pronCfg = parseCfg(pronunciationAct?.configJson);

      const dbSentence = shadCfg.sentence || dictCfg.sentence || pronCfg.sentence;
      if (dbSentence) {
        const dbDrill: ExtractedDrill = {
          id: shadowingAct?.id || "db-drill-1",
          topic: `${lessonTitle || courseTitle} (Bài học chuẩn hóa)`,
          sentence: dbSentence,
          ipa: shadCfg.ipa || dictCfg.ipa || pronCfg.ipa || "",
          meaningVi: shadCfg.meaningVi || dictCfg.meaningVi || pronCfg.meaningVi || "Nội dung bài tập giao tiếp chuẩn hóa từ hệ thống.",
          audioUrl: shadCfg.audioUrl || null,
          speakingPrompt: `Luyện tập phát âm và đối thoại phản xạ theo chủ đề: ${lessonTitle || courseTitle}`,
          speakingOutline: [
            "Lắng nghe câu mẫu và nhại theo ngữ điệu chuẩn",
            "Chú ý các từ nối âm và phát âm rõ âm đuôi",
            "Thực hành phản xạ tự nhiên trong đối thoại thực tế"
          ],
          sampleAnswer: dbSentence,
          keyPhrases: shadCfg.keyPhrases || dictCfg.keyPhrases || ["effective communication", "active listening", "mutual respect"],
          wordMapData: parsedWordMap
        };
        return [dbDrill, ...drills.filter(d => d.sentence !== dbDrill.sentence)];
      }
    }
    return drills;
  }, [dbActivities, drills, lessonTitle, courseTitle]);

  // Main UI State
  const [activeMode, setActiveMode] = useState<PracticeMode>("shadowing");
  const [activeDrillIndex, setActiveDrillIndex] = useState(0);
  const activeDrill = effectiveDrills[activeDrillIndex] || effectiveDrills[0];

  const activeWordMap: WordMapData = useMemo(() => {
    if (activeDrill.wordMapData) {
      return activeDrill.wordMapData;
    }
    const dbWordMapAct = dbActivities.find(a => a.type === "word_map");
    if (dbWordMapAct?.configJson) {
      try {
        return JSON.parse(dbWordMapAct.configJson);
      } catch {}
    }
    return buildDefaultWordMap(activeDrill);
  }, [activeDrill, dbActivities]);

  // Global Audio / TTS settings
  const [voiceAccent, setVoiceAccent] = useState<VoiceAccent>("en-US");
  const [ttsSpeed, setTtsSpeed] = useState<number>(1.0);
  const [isPlayingSample, setIsPlayingSample] = useState(false);
  const [audioErrorNotice, setAudioErrorNotice] = useState<string | null>(null);

  // Audio elements
  const sampleAudioRef = useRef<HTMLAudioElement | null>(null);

  // Toast / Status Message
  const [toast, setToast] = useState<{ message: string; type: "success" | "info" | "warning" } | null>(null);
  const showToast = useCallback((message: string, type: "success" | "info" | "warning" = "info") => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3800);
  }, []);

  const getModeLabel = (mode: PracticeMode): string => {
    switch (mode) {
      case "shadowing": return "Nhại giọng (Shadowing)";
      case "dictation": return "Chép chính tả (Dictation)";
      case "speaking": return "Luyện nói & Phát âm";
      case "word_map": return "Sơ đồ từ vựng (Word Map)";
      case "quiz": return "Trắc nghiệm nhanh";
      default: return "Luyện tập";
    }
  };

  // Record practice attempt to Backend Practice Engine
  const recordPracticeAttempt = useCallback(
    async (params: {
      mode: PracticeMode;
      score: number;
      maxScore?: number;
      isPassed?: boolean;
      details?: any;
      durationSeconds?: number;
      activityTitle?: string;
      activityKey?: string;
    }) => {
      try {
        const headers = getClientRequestHeaders();
        const dbAct = dbActivities.find(a => a.type === params.mode || a.id === params.activityKey);
        const passed = params.isPassed ?? (params.score >= (dbAct?.passingScore ?? 70));
        const payload = {
          activityId: dbAct?.id || null,
          courseId,
          lessonId: lessonId || null,
          activityType: params.mode,
          activityKey: params.activityKey || `${courseId}-${lessonId || "general"}-${params.mode}-${activeDrill.id}`,
          activityTitle: params.activityTitle || `${activeDrill.topic} · ${getModeLabel(params.mode)}`,
          score: Math.max(0, Math.min(params.maxScore || 100, Math.round(params.score))),
          maxScore: params.maxScore || 100,
          isPassed: passed,
          detailJson: JSON.stringify(params.details || {}),
          durationSeconds: params.durationSeconds || null,
        };

        const res = await fetch("/api/v1/practice/attempts", {
          method: "POST",
          headers: {
            ...headers,
            "Content-Type": "application/json",
          },
          body: JSON.stringify(payload),
        });

        if (res.ok) {
          const savedAttempt = await res.json();
          setPracticeAttempts(prev => [savedAttempt, ...prev]);
          invalidateDashboardCache();
          showToast(
            `Ghi nhận lượt luyện tập: ${Math.round(params.score)}/100 điểm (${passed ? "Đã đạt" : "Cần luyện lại"})`,
            passed ? "success" : "info"
          );
        }
      } catch {
        // Non-blocking
      }
    },
    [courseId, lessonId, dbActivities, activeDrill, showToast]
  );

  // Save word or phrase directly to FSRS Flashcards
  const handleSaveToFlashcardDirect = useCallback(
    async (params: { front: string; back: string; ipa?: string; example?: string }) => {
      try {
        const cleanFront = `${params.front}${params.ipa ? ` [${params.ipa}]` : ""}`.trim();
        const cleanBack = `${params.back}${params.example ? `\n\nVí dụ: ${params.example}` : ""}`.trim();

        if (onCreateFlashcardQuick) {
          await onCreateFlashcardQuick(cleanFront, cleanBack);
          showToast(`Đã lưu "${params.front}" vào Flashcard FSRS!`, "success");
          return;
        }

        const headers = getClientRequestHeaders();
        const res = await fetch("/api/v1/flashcards", {
          method: "POST",
          headers: {
            ...headers,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            lessonId: lessonId || null,
            courseId: courseId || null,
            cardType: "standard",
            sourceTitleSnapshot: `${courseTitle} · ${lessonTitle}`,
            sourceQuoteSnapshot: cleanFront,
            frontText: cleanFront,
            backText: cleanBack,
            tagNames: ["tieng-anh", "tu-vung", "fsrs"],
          }),
        });

        if (res.ok) {
          invalidateDashboardCache();
          showToast(`Đã lưu "${params.front}" vào Flashcard FSRS!`, "success");
        } else {
          showToast("Không thể tạo thẻ Flashcard lúc này.", "warning");
        }
      } catch {
        showToast("Lỗi kết nối khi lưu thẻ Flashcard.", "warning");
      }
    },
    [onCreateFlashcardQuick, lessonId, courseId, courseTitle, lessonTitle, showToast]
  );

  // ----------------- MODE 1: SHADOWING STATE -----------------
  const [isRecording, setIsRecording] = useState(false);
  const [recordedAudioUrl, setRecordedAudioUrl] = useState<string | null>(null);
  const [spokenTranscript, setSpokenTranscript] = useState("");
  const [speechRecSupported, setSpeechRecSupported] = useState(true);
  const [manualTranscriptInput, setManualTranscriptInput] = useState("");
  const [isSpeakingSlowWord, setIsSpeakingSlowWord] = useState<string | null>(null);

  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const recognitionRef = useRef<any>(null);

  // ----------------- MODE 2: DICTATION STATE -----------------
  const [dictationSubMode, setDictationSubMode] = useState<"type" | "chips">("chips");
  const [dictationTyped, setDictationTyped] = useState("");
  const [dictationChecked, setDictationChecked] = useState(false);
  const [showFirstLetterHint, setShowFirstLetterHint] = useState(false);
  const [showFullAnswer, setShowFullAnswer] = useState(false);

  // Word-Bank Chips
  const [placedChips, setPlacedChips] = useState<Array<{ id: string; word: string }>>([]);
  const [availableChips, setAvailableChips] = useState<Array<{ id: string; word: string }>>([]);

  // ----------------- MODE 3: SPEAKING CUE CARD STATE -----------------
  const [prepSeconds, setPrepSeconds] = useState(60);
  const [prepTimerActive, setPrepTimerActive] = useState(false);
  const [speakSeconds, setSpeakSeconds] = useState(120);
  const [speakTimerActive, setSpeakTimerActive] = useState(false);
  const [speakingNotes, setSpeakingNotes] = useState("");
  const [speakingAiLoading, setSpeakingAiLoading] = useState(false);
  const [speakingAiFeedback, setSpeakingAiFeedback] = useState<string | null>(null);
  const [showSampleAnswer, setShowSampleAnswer] = useState(false);

  // ----------------- MODE 4: MINI-QUIZ STATE -----------------
  const [currentQuizIdx, setCurrentQuizIdx] = useState(0);
  const [selectedQuizChoice, setSelectedQuizChoice] = useState<string | null>(null);
  const [quizAnswerChecked, setQuizAnswerChecked] = useState(false);
  const [quizScore, setQuizScore] = useState(0);
  const [quizCompleted, setQuizCompleted] = useState(false);

  // Reset states when drill changes
  useEffect(() => {
    if (sampleAudioRef.current) {
      sampleAudioRef.current.pause();
      sampleAudioRef.current.currentTime = 0;
    }
    if (typeof window !== "undefined" && "speechSynthesis" in window) {
      window.speechSynthesis.cancel();
    }
    setIsPlayingSample(false);
    setIsRecording(false);
    setRecordedAudioUrl(null);
    setSpokenTranscript("");
    setManualTranscriptInput("");
    setDictationTyped("");
    setDictationChecked(false);
    setShowFirstLetterHint(false);
    setShowFullAnswer(false);
    setSpeakingAiFeedback(null);
    setShowSampleAnswer(false);

    // Setup word bank chips for dictation
    const sentenceWords = activeDrill.sentence
      .replace(/[.,/#!$%^&*;:{}=\-_`~()]/g, "")
      .split(/\s+/)
      .filter(Boolean);

    const distractors = ["however", "therefore", "always", "crucial", "often", "their", "moreover"];
    const chosenDistractors = distractors.slice(0, 3);
    const allWords = [...sentenceWords, ...chosenDistractors];

    const chips = allWords
      .map((w, index) => ({ id: `chip-${index}-${w}`, word: w }))
      .sort(() => 0.5 - Math.random());

    setAvailableChips(chips);
    setPlacedChips([]);
  }, [activeDrillIndex, activeDrill]);

  // Clean up timers & audio URLs on unmount
  useEffect(() => {
    return () => {
      if (sampleAudioRef.current) {
        sampleAudioRef.current.pause();
      }
      if (typeof window !== "undefined" && "speechSynthesis" in window) {
        window.speechSynthesis.cancel();
      }
      if (mediaRecorderRef.current && mediaRecorderRef.current.state !== "inactive") {
        mediaRecorderRef.current.stop();
      }
      if (recognitionRef.current) {
        try {
          recognitionRef.current.stop();
        } catch {
          // ignore
        }
      }
      if (recordedAudioUrl) {
        URL.revokeObjectURL(recordedAudioUrl);
      }
    };
  }, [recordedAudioUrl]);

  // Speaking Timers
  useEffect(() => {
    let interval: NodeJS.Timeout | null = null;
    if (prepTimerActive && prepSeconds > 0) {
      interval = setInterval(() => setPrepSeconds(s => s - 1), 1000);
    } else if (prepSeconds === 0 && prepTimerActive) {
      setPrepTimerActive(false);
      showToast("⏰ Hết 1 phút chuẩn bị! Bạn hãy bật mic và bắt đầu phần nói 2 phút nhé.", "info");
    }
    return () => {
      if (interval) clearInterval(interval);
    };
  }, [prepTimerActive, prepSeconds, showToast]);

  useEffect(() => {
    let interval: NodeJS.Timeout | null = null;
    if (speakTimerActive && speakSeconds > 0) {
      interval = setInterval(() => setSpeakSeconds(s => s - 1), 1000);
    } else if (speakSeconds === 0 && speakTimerActive) {
      setSpeakTimerActive(false);
      showToast("🎉 Chúc mừng bạn đã hoàn thành trọn vẹn 2 phút bài nói Speaking!", "success");
    }
    return () => {
      if (interval) clearInterval(interval);
    };
  }, [speakTimerActive, speakSeconds, showToast]);

  // ----------------- SAMPLE AUDIO PLAYBACK (FILE OR TTS) -----------------
  const handlePlaySampleAudio = useCallback(
    (rate = ttsSpeed) => {
      if (isPlayingSample) {
        if (sampleAudioRef.current) {
          sampleAudioRef.current.pause();
        }
        if (typeof window !== "undefined" && "speechSynthesis" in window) {
          window.speechSynthesis.cancel();
        }
        setIsPlayingSample(false);
        return;
      }

      // Try file audio first if available
      if (activeDrill.audioUrl) {
        try {
          if (!sampleAudioRef.current) {
            sampleAudioRef.current = new Audio(activeDrill.audioUrl);
          } else {
            sampleAudioRef.current.src = activeDrill.audioUrl;
          }
          sampleAudioRef.current.playbackRate = rate;
          sampleAudioRef.current.onended = () => setIsPlayingSample(false);
          sampleAudioRef.current.onerror = () => {
            // Audio file failed to load, fallback seamlessly to SpeechSynthesis
            setAudioErrorNotice("File âm thanh bài học chưa tải được; tự động chuyển sang giọng đọc máy chuẩn.");
            sampleAudioRef.current = null;
            playWebSpeechTts(activeDrill.sentence, rate, voiceAccent);
          };

          setIsPlayingSample(true);
          sampleAudioRef.current.play().catch(() => {
            playWebSpeechTts(activeDrill.sentence, rate, voiceAccent);
          });
          return;
        } catch {
          // fallback to TTS
        }
      }

      // Fallback: Web Speech API
      playWebSpeechTts(activeDrill.sentence, rate, voiceAccent);
    },
    [isPlayingSample, activeDrill, ttsSpeed, voiceAccent]
  );

  const playWebSpeechTts = (text: string, rate: number, lang: VoiceAccent) => {
    if (typeof window === "undefined" || !("speechSynthesis" in window)) {
      showToast("Trình duyệt không hỗ trợ Web Speech API TTS.", "warning");
      return;
    }
    window.speechSynthesis.cancel();
    const utter = new SpeechSynthesisUtterance(text);
    utter.lang = lang;
    utter.rate = rate;
    utter.onstart = () => setIsPlayingSample(true);
    utter.onend = () => setIsPlayingSample(false);
    utter.onerror = () => setIsPlayingSample(false);
    window.speechSynthesis.speak(utter);
  };

  const handleSpeakSlowWord = (word: string) => {
    if (typeof window === "undefined" || !("speechSynthesis" in window)) return;
    window.speechSynthesis.cancel();
    setIsSpeakingSlowWord(word);
    const utter = new SpeechSynthesisUtterance(word);
    utter.lang = voiceAccent;
    utter.rate = 0.65; // Slow & clear pronunciation
    utter.onend = () => setIsSpeakingSlowWord(null);
    utter.onerror = () => setIsSpeakingSlowWord(null);
    window.speechSynthesis.speak(utter);
  };

  // Current mode recent attempts & best status
  const currentModeAttempts = useMemo(() => {
    return practiceAttempts.filter(a => a.activityType === activeMode);
  }, [practiceAttempts, activeMode]);

  const latestModeAttempt = currentModeAttempts[0];

  // ----------------- MIC & RECORDING LOGIC -----------------
  const handleToggleRecording = async () => {
    if (isRecording) {
      // STOP recording
      if (recognitionRef.current) {
        try {
          recognitionRef.current.stop();
        } catch {
          // ignore
        }
      }
      if (mediaRecorderRef.current && mediaRecorderRef.current.state !== "inactive") {
        mediaRecorderRef.current.stop();
      }
      setIsRecording(false);
      showToast("Đã dừng ghi âm.", "info");

      // Record practice attempt if speech was recognized
      if (effectiveSpokenText && (activeMode === "shadowing" || activeMode === "speaking")) {
        recordPracticeAttempt({
          mode: activeMode,
          score: shadowingAccuracy,
          isPassed: shadowingAccuracy >= 70,
          details: {
            sentence: activeDrill.sentence,
            spokenText: effectiveSpokenText,
            accuracy: shadowingAccuracy,
          }
        });
      }
      return;
    }

    // START recording
    setIsMicPermissionDenied(false);
    setSpokenTranscript("");
    if (recordedAudioUrl) {
      URL.revokeObjectURL(recordedAudioUrl);
      setRecordedAudioUrl(null);
    }

    // 1. Web Speech Recognition
    const win = window as any;
    const SpeechRecognition = win.SpeechRecognition || win.webkitSpeechRecognition;

    if (SpeechRecognition) {
      try {
        const recognition = new SpeechRecognition();
        recognition.lang = voiceAccent;
        recognition.interimResults = true;
        recognition.continuous = true;

        recognition.onresult = (event: any) => {
          let currentLine = "";
          for (let i = 0; i < event.results.length; i++) {
            currentLine += event.results[i][0].transcript + " ";
          }
          setSpokenTranscript(currentLine.trim());
        };

        recognition.onerror = () => {
          // Speech recognition may fail quietly; keep MediaRecorder going
        };

        recognition.onend = () => {
          // Recognition ended
        };

        recognition.start();
        recognitionRef.current = recognition;
        setSpeechRecSupported(true);
      } catch {
        setSpeechRecSupported(false);
      }
    } else {
      setSpeechRecSupported(false);
    }

    // 2. MediaRecorder for real audio capture
    if (typeof navigator !== "undefined" && navigator.mediaDevices?.getUserMedia) {
      try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        setIsMicPermissionDenied(false);
        const recorder = new MediaRecorder(stream);
        const chunks: BlobPart[] = [];

        recorder.ondataavailable = e => {
          if (e.data.size > 0) chunks.push(e.data);
        };

        recorder.onstop = () => {
          const blob = new Blob(chunks, { type: "audio/webm" });
          const url = URL.createObjectURL(blob);
          setRecordedAudioUrl(url);
          stream.getTracks().forEach(t => t.stop());
        };

        recorder.start();
        mediaRecorderRef.current = recorder;
      } catch {
        setIsMicPermissionDenied(true);
        showToast("Không truy cập được microphone. Vui lòng cấp quyền micro cho trình duyệt.", "warning");
      }
    }

    setIsRecording(true);
  };

  // ----------------- WORD-BY-WORD SCORING (SHADOWING) -----------------
  const effectiveSpokenText = (spokenTranscript.trim() || manualTranscriptInput.trim()).toLowerCase();

  const shadowingWordResults = useMemo(() => {
    const targetWords = activeDrill.sentence.split(/\s+/).filter(Boolean);
    if (!effectiveSpokenText) {
      return targetWords.map(w => ({ word: w, status: "pending" as const, clean: cleanWord(w) }));
    }

    const spokenTokens = effectiveSpokenText
      .split(/\s+/)
      .map(cleanWord)
      .filter(Boolean);
    const spokenSet = new Set(spokenTokens);

    return targetWords.map((rawWord, idx) => {
      const cleanTarget = cleanWord(rawWord);
      const exactAtPos = spokenTokens[idx] === cleanTarget;
      const inSpokenList = exactAtPos || spokenSet.has(cleanTarget);

      if (inSpokenList) {
        return { word: rawWord, status: "correct" as const, clean: cleanTarget };
      }

      // Check close match with Levenshtein
      const isClose = spokenTokens.some(
        st =>
          levenshteinDistance(st, cleanTarget) <= 2 ||
          (cleanTarget.length > 4 && (st.startsWith(cleanTarget.slice(0, 4)) || cleanTarget.startsWith(st)))
      );

      if (isClose) {
        return { word: rawWord, status: "close" as const, clean: cleanTarget };
      }

      return { word: rawWord, status: "incorrect" as const, clean: cleanTarget };
    });
  }, [activeDrill.sentence, effectiveSpokenText]);

  const shadowingAccuracy = useMemo(() => {
    if (!effectiveSpokenText) return 0;
    const total = shadowingWordResults.length;
    if (total === 0) return 0;
    const correctCount = shadowingWordResults.filter(w => w.status === "correct").length;
    const closeCount = shadowingWordResults.filter(w => w.status === "close").length;
    return Math.round(((correctCount + closeCount * 0.6) / total) * 100);
  }, [shadowingWordResults, effectiveSpokenText]);

  // Complete and record Shadowing attempt
  const handleCompleteShadowing = () => {
    const score = effectiveSpokenText ? shadowingAccuracy : 100;
    recordPracticeAttempt({
      mode: "shadowing",
      score,
      isPassed: score >= 70,
      details: {
        sentence: activeDrill.sentence,
        spokenText: effectiveSpokenText,
        hasAudioRecording: !!recordedAudioUrl,
      }
    });
  };

  // Record Speaking / Pronunciation attempt
  const handleRecordSpeakingScore = () => {
    if (!effectiveSpokenText) {
      showToast("Vui lòng đọc to câu mẫu để nhận dạng giọng nói trước khi ghi nhận điểm.", "warning");
      return;
    }
    recordPracticeAttempt({
      mode: "speaking",
      score: shadowingAccuracy,
      isPassed: shadowingAccuracy >= 70,
      details: {
        sentence: activeDrill.sentence,
        spokenText: effectiveSpokenText,
        recognizedAccuracy: shadowingAccuracy,
      }
    });
  };

  // Save incorrect words to Flashcards
  const handleSaveMissedWordsToFlashcards = async () => {
    const wrongWords = shadowingWordResults.filter(w => w.status === "incorrect");
    if (wrongWords.length === 0) {
      showToast("Không có từ phát âm sai nào cần tạo thẻ!", "info");
      return;
    }

    try {
      for (const item of wrongWords) {
        await handleSaveToFlashcardDirect({
          front: item.word,
          back: `Phát âm mẫu câu: "${activeDrill.sentence}"\nNghĩa tiếng Việt: ${activeDrill.meaningVi}`,
          example: activeDrill.sentence
        });
      }
      showToast(`Đã lưu ${wrongWords.length} từ vào Flashcard FSRS ôn tập!`, "success");
    } catch {
      showToast("Không thể lưu flashcard. Vui lòng thử lại sau.", "warning");
    }
  };

  // Save Shadowing results to Notebook
  const handleSaveShadowingToNotebook = async () => {
    const wrongWords = shadowingWordResults.filter(w => w.status === "incorrect").map(w => w.word);
    const markdownLog = [
      `### 🎙️ Kết Quả Luyện Phát Âm (Shadowing): ${activeDrill.topic}`,
      `- **Câu luyện tập:** ${activeDrill.sentence}`,
      `- **Phiên âm IPA:** \`${activeDrill.ipa}\``,
      `- **Nghĩa tiếng Việt:** ${activeDrill.meaningVi}`,
      `- **Độ chính xác:** **${shadowingAccuracy}%**`,
      `- **Bản ghi âm giọng bạn:** ${effectiveSpokenText || "(Chưa có dữ liệu lời nói)"}`,
      wrongWords.length > 0
        ? `- **Các từ cần chú ý luyện lại:** ${wrongWords.join(", ")}`
        : "- **Nhận xét:** Phát âm rất chuẩn xác và rõ ràng!"
    ].join("\n");

    try {
      await onSaveToNotebook(markdownLog);
      showToast("Đã lưu kết quả bài tập phát âm vào Sổ tay!", "success");
    } catch {
      showToast("Lỗi khi lưu vào sổ tay.", "warning");
    }
  };

  // ----------------- MODE 2: DICTATION EVALUATION -----------------
  const dictationAccuracy = useMemo(() => {
    const targetWords = activeDrill.sentence
      .toLowerCase()
      .replace(/[.,/#!$%^&*;:{}=\-_`~()]/g, "")
      .split(/\s+/)
      .filter(Boolean);

    let userWords: string[] = [];
    if (dictationSubMode === "type") {
      userWords = dictationTyped
        .toLowerCase()
        .replace(/[.,/#!$%^&*;:{}=\-_`~()]/g, "")
        .split(/\s+/)
        .filter(Boolean);
    } else {
      userWords = placedChips.map(c => cleanWord(c.word));
    }

    if (userWords.length === 0) return 0;
    let hits = 0;
    targetWords.forEach((tw, i) => {
      if (userWords[i] === tw) hits++;
    });
    const maxLen = Math.max(targetWords.length, userWords.length);
    return Math.round((hits / maxLen) * 100);
  }, [activeDrill.sentence, dictationSubMode, dictationTyped, placedChips]);

  // Diff comparison items for dictation
  const dictationDiffItems = useMemo(() => {
    const targetWords = activeDrill.sentence
      .replace(/[.,/#!$%^&*;:{}=\-_`~()]/g, "")
      .split(/\s+/)
      .filter(Boolean);

    let userWords: string[] = [];
    if (dictationSubMode === "type") {
      userWords = dictationTyped
        .replace(/[.,/#!$%^&*;:{}=\-_`~()]/g, "")
        .split(/\s+/)
        .filter(Boolean);
    } else {
      userWords = placedChips.map(c => c.word);
    }

    const maxLen = Math.max(targetWords.length, userWords.length);
    const items: Array<{
      targetWord?: string;
      userWord?: string;
      status: "correct" | "typo" | "missing" | "extra";
    }> = [];

    for (let i = 0; i < maxLen; i++) {
      const tw = targetWords[i];
      const uw = userWords[i];
      if (tw && uw) {
        if (cleanWord(tw) === cleanWord(uw)) {
          items.push({ targetWord: tw, userWord: uw, status: "correct" });
        } else {
          items.push({ targetWord: tw, userWord: uw, status: "typo" });
        }
      } else if (tw && !uw) {
        items.push({ targetWord: tw, status: "missing" });
      } else if (!tw && uw) {
        items.push({ userWord: uw, status: "extra" });
      }
    }
    return items;
  }, [activeDrill.sentence, dictationSubMode, dictationTyped, placedChips]);

  const missedDictationWords = useMemo(() => {
    return Array.from(
      new Set(
        dictationDiffItems
          .filter(item => item.status === "typo" || item.status === "missing")
          .map(item => item.targetWord)
          .filter((w): w is string => !!w)
      )
    );
  }, [dictationDiffItems]);

  const handleSaveMissedDictationToFlashcards = async () => {
    if (missedDictationWords.length === 0) {
      showToast("Không có từ gõ sai nào cần lưu!", "info");
      return;
    }
    for (const w of missedDictationWords) {
      await handleSaveToFlashcardDirect({
        front: w,
        back: `Từ cần nhớ trong câu chính tả: "${activeDrill.sentence}"\nNghĩa câu: ${activeDrill.meaningVi}`,
        example: activeDrill.sentence
      });
    }
    showToast(`Đã lưu ${missedDictationWords.length} từ vào Flashcard FSRS!`, "success");
  };

  const handleCheckDictation = () => {
    setDictationChecked(true);
    recordPracticeAttempt({
      mode: "dictation",
      score: dictationAccuracy,
      isPassed: dictationAccuracy >= 70,
      details: {
        targetSentence: activeDrill.sentence,
        typed: dictationTyped,
        placedChips: placedChips.map(c => c.word),
        subMode: dictationSubMode,
        accuracy: dictationAccuracy,
      }
    });
  };

  const handleChipClick = (chip: { id: string; word: string }, from: "available" | "placed") => {
    if (from === "available") {
      setAvailableChips(prev => prev.filter(c => c.id !== chip.id));
      setPlacedChips(prev => [...prev, chip]);
    } else {
      setPlacedChips(prev => prev.filter(c => c.id !== chip.id));
      setAvailableChips(prev => [...prev, chip]);
    }
  };

  const handleResetChips = () => {
    setAvailableChips(prev => [...prev, ...placedChips]);
    setPlacedChips([]);
    setDictationChecked(false);
  };

  // ----------------- MODE 3: SPEAKING AI FEEDBACK -----------------
  // Live target vocabulary detection in Speaking
  const detectedPhrasesStatus = useMemo(() => {
    const fullText = `${spokenTranscript} ${speakingNotes} ${manualTranscriptInput}`.toLowerCase();
    return activeDrill.keyPhrases.map(phrase => {
      const phraseNorm = phrase.toLowerCase().replace(/[^a-z0-9 ]/g, "");
      const words = phraseNorm.split(" ").filter(Boolean);
      const isMatched =
        fullText.includes(phraseNorm) ||
        (words.length >= 2 && words.every(w => fullText.includes(w)));
      return { phrase, isMatched };
    });
  }, [spokenTranscript, speakingNotes, manualTranscriptInput, activeDrill.keyPhrases]);

  const detectedCount = detectedPhrasesStatus.filter(p => p.isMatched).length;

  const handleRequestAiSpeakingReview = async () => {
    const userSpokenContent = spokenTranscript.trim() || speakingNotes.trim() || manualTranscriptInput.trim();
    if (!userSpokenContent) {
      showToast("Vui lòng bật mic nói hoặc gõ dàn ý của bạn trước khi nhận xét AI.", "warning");
      return;
    }

    setSpeakingAiLoading(true);
    try {
      const headers = getClientRequestHeaders({
        "Content-Type": "application/json"
      });

      const promptText = `
Bạn là chuyên gia khảo thí IELTS Speaking Band 8.5+. Hãy đánh giá câu trả lời của người học một cách chi tiết, nhẹ nhàng và mang tính động viên:
- Chủ đề: ${activeDrill.speakingPrompt}
- Dàn ý gợi ý: ${activeDrill.speakingOutline.join(" | ")}
- Từ vựng mục tiêu cần dùng: ${activeDrill.keyPhrases.join(", ")}
- Bài nói của người học: "${userSpokenContent}"

Định dạng nhận xét ngắn gọn bằng Tiếng Việt gồm:
1. Điểm mạnh về ý tưởng và độ trôi chảy (Fluency & Coherence)
2. Nhận xét về độ rộng từ vựng (Lexical Resource) - đã dùng được cụm từ nào tốt?
3. Gợi ý 2 cách sửa ngữ pháp hoặc cách diễn đạt tự nhiên hơn (Pronunciation / Grammar polish).
      `.trim();

      const res = await fetch("/api/v1/ai/explain", {
        method: "POST",
        headers,
        body: JSON.stringify({
          excerpt: promptText,
          contextTitle: `IELTS Speaking Review: ${activeDrill.topic}`,
          actionType: "explain"
        })
      });

      if (res.ok) {
        const data = await res.json();
        setSpeakingAiFeedback(data.explanation || "Đã phân tích xong bài nói của bạn.");
      } else {
        // Fallback rubric review
        setSpeakingAiFeedback(
          `Nhận xét tự động: Bạn đã diễn đạt được nội dung cơ bản. Đã sử dụng thành công ${detectedCount}/${activeDrill.keyPhrases.length} cụm từ mục tiêu. Để nâng band điểm, hãy bổ sung các liên từ (Furthermore, Consequently) và kéo dài câu trả lời bằng ví dụ thực tế.`
        );
      }
    } catch {
      setSpeakingAiFeedback(
        `Nhận xét tự động: Bạn đã diễn đạt được nội dung cơ bản. Đã sử dụng thành công ${detectedCount}/${activeDrill.keyPhrases.length} cụm từ mục tiêu. Tiếp tục duy trì luyện tập mỗi ngày!`
      );
    } finally {
      setSpeakingAiLoading(false);
    }
  };

  const handleSaveSpeakingToNotebook = async () => {
    const textLog = [
      `### 🗣️ Nhật Ký Luyện Nói (Speaking Cue Card): ${activeDrill.topic}`,
      `- **Câu hỏi thảo luận:** ${activeDrill.speakingPrompt}`,
      `- **Cụm từ vựng mục tiêu đã áp dụng:** ${detectedCount}/${activeDrill.keyPhrases.length} cụm (${detectedPhrasesStatus.filter(p => p.isMatched).map(p => p.phrase).join(", ") || "Chưa ghi nhận"})`,
      `- **Bản ghi âm / Ghi chú của bạn:** ${spokenTranscript || speakingNotes || "Không có văn bản"}`,
      speakingAiFeedback ? `- **Nhận xét từ AI / Giảng viên:**\n${speakingAiFeedback}` : ""
    ]
      .filter(Boolean)
      .join("\n");

    try {
      await onSaveToNotebook(textLog);
      showToast("Đã lưu bài luyện nói vào Sổ tay!", "success");
    } catch {
      showToast("Lỗi khi lưu vào sổ tay.", "warning");
    }
  };

  // ----------------- MODE 4: QUIZ ACTIONS -----------------
  const activeQuiz = quizQuestions[currentQuizIdx] || quizQuestions[0];

  const handleSelectQuizOption = (opt: string) => {
    if (quizAnswerChecked) return;
    setSelectedQuizChoice(opt);
  };

  const handleCheckQuizAnswer = () => {
    if (!selectedQuizChoice || quizAnswerChecked) return;
    setQuizAnswerChecked(true);
    if (selectedQuizChoice === activeQuiz.correctAnswer) {
      setQuizScore(s => s + 1);
    }
  };

  const handleNextQuizQuestion = () => {
    if (currentQuizIdx + 1 < quizQuestions.length) {
      setCurrentQuizIdx(i => i + 1);
      setSelectedQuizChoice(null);
      setQuizAnswerChecked(false);
    } else {
      setQuizCompleted(true);
      const isCorrectNow = selectedQuizChoice === activeQuiz.correctAnswer;
      const totalCorrect = quizScore + (isCorrectNow ? 1 : 0);
      const finalScore = Math.round((totalCorrect / quizQuestions.length) * 100);
      recordPracticeAttempt({
        mode: "quiz",
        score: finalScore,
        isPassed: finalScore >= 70,
        details: {
          totalQuestions: quizQuestions.length,
          correctAnswers: totalCorrect,
        }
      });
    }
  };

  const handleRestartQuiz = () => {
    setCurrentQuizIdx(0);
    setSelectedQuizChoice(null);
    setQuizAnswerChecked(false);
    setQuizScore(0);
    setQuizCompleted(false);
  };

  return (
    <div data-testid="english-practice-lab" className="w-full max-w-5xl mx-auto space-y-5 p-3 sm:p-6 transition-colors duration-200">
      {/* Toast Banner */}
      {toast && (
        <div
          role="status"
          aria-live="polite"
          className={`flex items-center justify-between gap-3 px-4 py-3 rounded-xl shadow-md text-sm font-medium animate-fadeIn ${
            toast.type === "success"
              ? "bg-emerald-50 text-emerald-800 dark:bg-emerald-950/70 dark:text-emerald-200 border border-emerald-300 dark:border-emerald-800"
              : toast.type === "warning"
              ? "bg-amber-50 text-amber-800 dark:bg-amber-950/70 dark:text-amber-200 border border-amber-300 dark:border-amber-800"
              : "bg-indigo-50 text-indigo-900 dark:bg-indigo-950/70 dark:text-indigo-200 border border-indigo-200 dark:border-indigo-800"
          }`}
        >
          <div className="flex items-center gap-2">
            {toast.type === "success" && <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />}
            {toast.type === "warning" && <AlertCircle className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0" />}
            {toast.type === "info" && <Sparkles className="w-4 h-4 text-indigo-600 dark:text-indigo-400 shrink-0" />}
            <span>{toast.message}</span>
          </div>
          <button
            onClick={() => setToast(null)}
            className="text-xs opacity-70 hover:opacity-100 p-1 min-h-[36px] min-w-[36px] flex items-center justify-center"
            aria-label="Đóng thông báo"
          >
            ✕
          </button>
        </div>
      )}

      {/* Header & Mode Tabs */}
      <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 shadow-sm p-4 sm:p-6">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 pb-5 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
          <div>
            <div className="flex flex-wrap items-center gap-2 mb-1.5">
              {dbActivities.length > 0 ? (
                <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-700 dark:text-emerald-300 border border-emerald-500/30">
                  <CheckCircle2 className="w-3.5 h-3.5" />
                  Bài học chuẩn hóa ({dbActivities.length} hoạt động)
                </span>
              ) : (
                <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-[#5E51E8]/10 text-[#5E51E8] dark:text-[#818CF8]">
                  <Sparkles className="w-3.5 h-3.5" />
                  100% Miễn Phí & Tự Động Thích Ứng
                </span>
              )}
              <span className="text-xs text-[var(--text-secondary,#71717a)] dark:text-zinc-400">
                Chủ đề: <strong className="text-[var(--text-primary,#18181b)] dark:text-zinc-200">{detectedTopic}</strong>
              </span>
            </div>
            <h2 className="text-xl sm:text-2xl font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100 tracking-tight">
              Luyện Tập Tiếng Anh & IELTS Chuyên Sâu
            </h2>
            <p className="text-sm text-[var(--text-secondary,#52525b)] dark:text-zinc-400 mt-0.5">
              Tích hợp công nghệ nhận diện giọng nói, nghe mẫu chuẩn bản xứ và hỗ trợ tối ưu trên điện thoại.
            </p>
          </div>

          {/* Audio & Accent Settings + Recent Mode Attempt Status */}
          <div className="flex flex-col sm:flex-row sm:items-center gap-2">
            {latestModeAttempt && (
              <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-medium bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700">
                <span className="text-[var(--text-secondary,#71717a)] dark:text-zinc-400">Gần nhất:</span>
                <span className={latestModeAttempt.isPassed ? "text-emerald-600 dark:text-emerald-400 font-bold" : "text-amber-600 dark:text-amber-400 font-bold"}>
                  {Math.round(latestModeAttempt.score)}/100
                </span>
                <span className={`px-2 py-0.5 rounded-full text-[10px] font-semibold ${latestModeAttempt.isPassed ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300" : "bg-amber-500/15 text-amber-800 dark:text-amber-300"}`}>
                  {latestModeAttempt.isPassed ? "Đã đạt" : "Cần luyện lại"}
                </span>
              </div>
            )}
            <div className="flex flex-wrap items-center gap-2">
              <div className="flex items-center rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 p-1 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700">
                <button
                  type="button"
                  onClick={() => setVoiceAccent("en-US")}
                  className={`px-3 py-1.5 rounded-lg text-xs font-semibold min-h-[38px] transition-all ${
                    voiceAccent === "en-US"
                      ? "bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-[#5E51E8] dark:text-[#818CF8] shadow-xs"
                      : "text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:text-[var(--text-primary,#18181b)]"
                  }`}
                >
                  🇺🇸 Anh - Mỹ
                </button>
                <button
                  type="button"
                  onClick={() => setVoiceAccent("en-GB")}
                  className={`px-3 py-1.5 rounded-lg text-xs font-semibold min-h-[38px] transition-all ${
                    voiceAccent === "en-GB"
                      ? "bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-[#5E51E8] dark:text-[#818CF8] shadow-xs"
                      : "text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:text-[var(--text-primary,#18181b)]"
                  }`}
                >
                  🇬🇧 Anh - Anh
                </button>
              </div>

              <div className="flex items-center rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 p-1 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700">
                {[0.75, 1.0, 1.25].map(rate => (
                  <button
                    key={rate}
                    type="button"
                    onClick={() => setTtsSpeed(rate)}
                    className={`px-2.5 py-1.5 rounded-lg text-xs font-medium min-h-[38px] transition-all ${
                      ttsSpeed === rate
                        ? "bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-[#5E51E8] dark:text-[#818CF8] shadow-xs font-bold"
                        : "text-[var(--text-secondary,#52525b)] dark:text-zinc-400"
                    }`}
                  >
                    {rate}x
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>

        {/* 5 Practice Mode Tabs (Optimized for mobile & desktop) */}
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-2 pt-4">
          <button
            type="button"
            data-testid="practice-tab-shadowing"
            onClick={() => setActiveMode("shadowing")}
            className={`flex items-center justify-center gap-2 p-3 rounded-xl border text-sm font-semibold min-h-[48px] transition-all ${
              activeMode === "shadowing"
                ? "bg-[#5E51E8]/10 border-[#5E51E8] text-[#5E51E8] dark:text-[#818CF8] dark:border-[#818CF8]"
                : "border-transparent bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:text-[var(--text-primary,#18181b)]"
            }`}
          >
            <Mic className="w-4 h-4 shrink-0" />
            <span>1. Nhại Giọng</span>
          </button>

          <button
            type="button"
            data-testid="practice-tab-dictation"
            onClick={() => setActiveMode("dictation")}
            className={`flex items-center justify-center gap-2 p-3 rounded-xl border text-sm font-semibold min-h-[48px] transition-all ${
              activeMode === "dictation"
                ? "bg-[#5E51E8]/10 border-[#5E51E8] text-[#5E51E8] dark:text-[#818CF8] dark:border-[#818CF8]"
                : "border-transparent bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:text-[var(--text-primary,#18181b)]"
            }`}
          >
            <Headphones className="w-4 h-4 shrink-0" />
            <span>2. Chép Chính Tả</span>
          </button>

          <button
            type="button"
            data-testid="practice-tab-speaking"
            onClick={() => setActiveMode("speaking")}
            className={`flex items-center justify-center gap-2 p-3 rounded-xl border text-sm font-semibold min-h-[48px] transition-all ${
              activeMode === "speaking"
                ? "bg-[#5E51E8]/10 border-[#5E51E8] text-[#5E51E8] dark:text-[#818CF8] dark:border-[#818CF8]"
                : "border-transparent bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:text-[var(--text-primary,#18181b)]"
            }`}
          >
            <MessageSquare className="w-4 h-4 shrink-0" />
            <span>3. Luyện Nói & Phát Âm</span>
          </button>

          <button
            type="button"
            data-testid="practice-tab-word-map"
            onClick={() => setActiveMode("word_map")}
            className={`flex items-center justify-center gap-2 p-3 rounded-xl border text-sm font-semibold min-h-[48px] transition-all ${
              activeMode === "word_map"
                ? "bg-[#5E51E8]/10 border-[#5E51E8] text-[#5E51E8] dark:text-[#818CF8] dark:border-[#818CF8]"
                : "border-transparent bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:text-[var(--text-primary,#18181b)]"
            }`}
          >
            <BookOpen className="w-4 h-4 shrink-0" />
            <span>4. Sơ Đồ Từ Vựng</span>
          </button>

          <button
            type="button"
            data-testid="practice-tab-quiz"
            onClick={() => setActiveMode("quiz")}
            className={`flex items-center justify-center gap-2 p-3 rounded-xl border text-sm font-semibold min-h-[48px] transition-all ${
              activeMode === "quiz"
                ? "bg-[#5E51E8]/10 border-[#5E51E8] text-[#5E51E8] dark:text-[#818CF8] dark:border-[#818CF8]"
                : "border-transparent bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:text-[var(--text-primary,#18181b)]"
            }`}
          >
            <Layers className="w-4 h-4 shrink-0" />
            <span>5. Trắc Nghiệm</span>
          </button>
        </div>

        {/* Drill Selector Bar (Modes 1, 2, 3) */}
        {activeMode !== "quiz" && (
          <div className="flex items-center justify-between gap-2 mt-4 pt-4 border-t border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
            <button
              type="button"
              disabled={activeDrillIndex === 0}
              onClick={() => setActiveDrillIndex(i => Math.max(0, i - 1))}
              className="flex items-center gap-1 px-3 py-2 rounded-xl text-xs font-medium min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-secondary,#52525b)] dark:text-zinc-300 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-zinc-200 dark:hover:bg-zinc-700"
            >
              <ChevronLeft className="w-4 h-4" />
              <span>Câu trước</span>
            </button>

            <div className="flex items-center gap-1.5 flex-wrap justify-center py-1 max-w-full">
              {drills.map((drill, idx) => (
                <button
                  key={drill.id}
                  type="button"
                  onClick={() => setActiveDrillIndex(idx)}
                  className={`w-8 h-8 rounded-full text-xs font-bold min-h-[36px] min-w-[36px] flex items-center justify-center transition-all ${
                    activeDrillIndex === idx
                      ? "bg-[#5E51E8] text-white shadow-sm ring-2 ring-[#5E51E8]/30"
                      : "bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:bg-zinc-200 dark:hover:bg-zinc-700"
                  }`}
                  aria-label={`Chọn câu ${idx + 1}`}
                >
                  {idx + 1}
                </button>
              ))}
            </div>

            <button
              type="button"
              disabled={activeDrillIndex === drills.length - 1}
              onClick={() => setActiveDrillIndex(i => Math.min(drills.length - 1, i + 1))}
              className="flex items-center gap-1 px-3 py-2 rounded-xl text-xs font-medium min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-secondary,#52525b)] dark:text-zinc-300 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-zinc-200 dark:hover:bg-zinc-700"
            >
              <span>Câu sau</span>
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        )}
      </div>

      {/* Notice when audio file fails to load */}
      {audioErrorNotice && (
        <div className="flex items-center gap-2 p-3 rounded-xl bg-amber-500/10 border border-amber-500/30 text-xs text-amber-800 dark:text-amber-200">
          <AlertCircle className="w-4 h-4 shrink-0 text-amber-600 dark:text-amber-400" />
          <span>{audioErrorNotice}</span>
        </div>
      )}

      {/* ----------------- CHẾ ĐỘ 1: SHADOWING & CHẤM TỪNG TỪ ----------------- */}
      {activeMode === "shadowing" && (
        <div className="space-y-4">
          <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 shadow-sm p-5 sm:p-7 space-y-6">
            {/* Header info */}
            <div className="flex flex-wrap items-center justify-between gap-2 text-xs">
              <span className="font-semibold text-[var(--text-secondary,#71717a)] dark:text-zinc-400 uppercase tracking-wider">
                Mẫu Câu Luyện Phát Âm #{activeDrillIndex + 1}
              </span>
              <span className="px-2.5 py-0.5 rounded-full bg-emerald-500/10 text-emerald-700 dark:text-emerald-300 font-medium">
                {activeDrill.audioUrl ? "Audio bản xứ kèm bài học" : "Giọng đọc Web Speech"}
              </span>
            </div>

            {/* Target Sentence Display */}
            <div className="space-y-2.5">
              <p className="text-xl sm:text-2xl font-semibold text-[var(--text-primary,#18181b)] dark:text-zinc-100 leading-relaxed">
                {activeDrill.sentence}
              </p>
              {activeDrill.ipa && (
                <div className="inline-block px-3 py-1 rounded-lg bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 text-xs font-mono text-[#5E51E8] dark:text-[#818CF8]">
                  {activeDrill.ipa}
                </div>
              )}
              <p className="text-sm text-[var(--text-secondary,#52525b)] dark:text-zinc-400 italic">
                {activeDrill.meaningVi}
              </p>
            </div>

            {/* Listen to native model controls */}
            <div className="flex flex-wrap items-center gap-3 pt-2">
              <button
                type="button"
                onClick={() => handlePlaySampleAudio()}
                className={`flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-sm min-h-[44px] transition-all shadow-xs ${
                  isPlayingSample
                    ? "bg-rose-500 text-white hover:bg-rose-600"
                    : "bg-[#5E51E8] text-white hover:bg-[#4F46E5]"
                }`}
              >
                {isPlayingSample ? <Pause className="w-4 h-4" /> : <Volume2 className="w-4 h-4" />}
                <span>{isPlayingSample ? "Dừng nghe mẫu" : "Nghe mẫu chuẩn"}</span>
              </button>

              <button
                type="button"
                onClick={() => handlePlaySampleAudio(0.75)}
                className="flex items-center gap-1.5 px-3.5 py-2 rounded-xl text-xs font-medium min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-secondary,#52525b)] dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-700"
              >
                <Sliders className="w-3.5 h-3.5" />
                <span>Nghe chậm (0.75x)</span>
              </button>
            </div>

            {/* Main Interactive Mic Button for Recording & Live Grading */}
            <div className="pt-4 border-t border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 flex flex-col items-center justify-center text-center space-y-3">
              <div className="relative">
                {isRecording && (
                  <span className="absolute -inset-2 rounded-full bg-rose-500/20 animate-ping" />
                )}
                <button
                  type="button"
                  onClick={handleToggleRecording}
                  className={`relative w-16 h-16 rounded-full flex items-center justify-center text-white shadow-lg transition-transform active:scale-95 ${
                    isRecording
                      ? "bg-rose-600 ring-4 ring-rose-400/40"
                      : "bg-[#5E51E8] hover:bg-[#4F46E5] ring-4 ring-[#5E51E8]/20"
                  }`}
                  aria-label={isRecording ? "Dừng ghi âm" : "Bắt đầu ghi âm"}
                >
                  {isRecording ? <MicOff className="w-7 h-7" /> : <Mic className="w-7 h-7" />}
                </button>
              </div>

              <div className="space-y-0.5">
                <p className="text-sm font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                  {isRecording ? "Đang thu âm giọng bạn... Hãy đọc to câu mẫu!" : "Chạm nút Mic để đọc & chấm điểm"}
                </p>
                <p className="text-xs text-[var(--text-secondary,#71717a)] dark:text-zinc-400">
                  {isRecording ? "Chạm lại để dừng và nhận kết quả chi tiết từng từ" : "Hệ thống sẽ tô màu trực quan từng từ bạn đọc"}
                </p>
              </div>

              {/* Mic Permission Denied Banner */}
              {isMicPermissionDenied && (
                <div className="p-3.5 rounded-xl bg-rose-500/10 border border-rose-500/30 text-xs text-rose-900 dark:text-rose-200 space-y-1 text-left">
                  <div className="flex items-center gap-2 font-bold">
                    <AlertCircle className="w-4 h-4 text-rose-600 dark:text-rose-400 shrink-0" />
                    <span>Không thể truy cập microphone</span>
                  </div>
                  <p>
                    Trình duyệt đã từ chối quyền microphone hoặc không tìm thấy micro. Vui lòng cấp quyền micro cho trang web trong cài đặt trình duyệt để ghi âm.
                  </p>
                </div>
              )}

              {/* Side-by-side comparison audio player */}
              {recordedAudioUrl && (
                <div className="w-full max-w-md mx-auto p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-3 mt-3 animate-fadeIn">
                  <div className="flex items-center justify-between text-xs font-bold text-[var(--text-secondary,#52525b)] dark:text-zinc-300 uppercase tracking-wider">
                    <span>So sánh trực tiếp: Bản xứ vs Bạn</span>
                    <Sparkles className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
                  </div>

                  <div className="space-y-2 text-left">
                    <div className="flex items-center justify-between p-2 rounded-lg bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700">
                      <span className="text-xs font-semibold text-[var(--text-primary,#18181b)] dark:text-zinc-200">1. Giọng phát âm mẫu:</span>
                      <button
                        type="button"
                        onClick={() => handlePlaySampleAudio()}
                        className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold bg-[#5E51E8] text-white hover:bg-[#4F46E5] min-h-[36px]"
                      >
                        <Volume2 className="w-3.5 h-3.5" />
                        <span>Nghe mẫu</span>
                      </button>
                    </div>

                    <div className="p-2 rounded-lg bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-1">
                      <span className="text-xs font-semibold text-[var(--text-primary,#18181b)] dark:text-zinc-200">2. Bản ghi âm giọng bạn:</span>
                      <audio src={recordedAudioUrl} controls className="w-full h-9 rounded" />
                    </div>
                  </div>

                  <button
                    type="button"
                    onClick={handleCompleteShadowing}
                    className="w-full flex items-center justify-center gap-2 py-2.5 rounded-xl font-bold text-xs bg-emerald-600 text-white hover:bg-emerald-700 shadow-xs transition-all min-h-[44px]"
                  >
                    <CheckCircle2 className="w-4 h-4" />
                    <span>Xác nhận hoàn thành lượt Shadowing</span>
                  </button>
                </div>
              )}
            </div>

            {/* Fallback Notice for SpeechRecognition in Firefox/Safari */}
            {!speechRecSupported && (
              <div className="p-3.5 rounded-xl bg-amber-500/10 border border-amber-500/30 text-xs text-amber-900 dark:text-amber-200 space-y-2">
                <div className="flex items-center gap-2 font-semibold">
                  <AlertCircle className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0" />
                  <span>Trình duyệt của bạn hiện chưa mở tự động Web Speech Recognition.</span>
                </div>
                <p>
                  Bạn vẫn có thể ghi âm thật và bấm <strong>Nghe lại giọng bạn</strong> ở trên để tự đối chiếu với giọng mẫu, hoặc gõ nhanh câu bạn vừa đọc vào ô bên dưới để chấm độ khớp từng từ:
                </p>
                <input
                  type="text"
                  value={manualTranscriptInput}
                  onChange={e => setManualTranscriptInput(e.target.value)}
                  placeholder="Gõ hoặc dán câu tiếng Anh bạn vừa nói vào đây để chấm..."
                  className="w-full px-3 py-2 rounded-lg text-xs bg-[var(--surface-card,#ffffff)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 min-h-[44px]"
                />
              </div>
            )}

            {/* Word-by-Word Colored Grading Display */}
            {effectiveSpokenText && (
              <div className="space-y-4 pt-4 border-t border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 animate-fadeIn">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-bold text-[var(--text-secondary,#52525b)] dark:text-zinc-300 uppercase tracking-wider">
                    Kết quả chấm điểm từng từ:
                  </span>
                  <span
                    className={`text-sm font-extrabold px-3 py-1 rounded-full ${
                      shadowingAccuracy >= 80
                        ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
                        : shadowingAccuracy >= 50
                        ? "bg-amber-500/10 text-amber-600 dark:text-amber-400"
                        : "bg-rose-500/10 text-rose-600 dark:text-rose-400"
                    }`}
                  >
                    Độ chuẩn xác: {shadowingAccuracy}%
                  </span>
                </div>

                {/* Words display */}
                <div className="flex flex-wrap gap-2 p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/60 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700/60">
                  {shadowingWordResults.map((item, idx) => (
                    <button
                      key={idx}
                      type="button"
                      onClick={() => item.status === "incorrect" && handleSpeakSlowWord(item.word)}
                      title={item.status === "incorrect" ? "Bấm vào đây để nghe máy đọc chậm từ này" : undefined}
                      className={`px-3 py-1.5 rounded-lg text-sm font-semibold transition-all min-h-[40px] flex items-center gap-1.5 ${
                        item.status === "correct"
                          ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border border-emerald-500/30"
                          : item.status === "close"
                          ? "bg-amber-500/15 text-amber-800 dark:text-amber-300 border border-amber-500/30"
                          : "bg-rose-500/15 text-rose-700 dark:text-rose-300 border border-rose-500/40 hover:bg-rose-500/25 cursor-pointer shadow-2xs"
                      }`}
                    >
                      <span>{item.word}</span>
                      {item.status === "correct" && <Check className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400" />}
                      {item.status === "close" && <span className="text-xs text-amber-600 dark:text-amber-400">~</span>}
                      {item.status === "incorrect" && (
                        <Volume2 className="w-3.5 h-3.5 text-rose-600 dark:text-rose-400 animate-pulse" />
                      )}
                    </button>
                  ))}
                </div>

                <div className="flex flex-wrap items-center gap-4 text-xs text-[var(--text-secondary,#71717a)] dark:text-zinc-400">
                  <span className="flex items-center gap-1.5">
                    <span className="w-2.5 h-2.5 rounded-full bg-emerald-500" />
                    Xanh: Phát âm chuẩn
                  </span>
                  <span className="flex items-center gap-1.5">
                    <span className="w-2.5 h-2.5 rounded-full bg-amber-500" />
                    Vàng: Gần đúng (sai âm đuôi)
                  </span>
                  <span className="flex items-center gap-1.5">
                    <span className="w-2.5 h-2.5 rounded-full bg-rose-500" />
                    Đỏ: Đọc sai (bấm vào từ để nghe đọc chậm)
                  </span>
                </div>

                {/* Quick 1-touch actions */}
                <div className="flex flex-wrap items-center gap-2 pt-3">
                  {onCreateFlashcardQuick && (
                    <button
                      type="button"
                      onClick={handleSaveMissedWordsToFlashcards}
                      className="flex items-center gap-2 px-3.5 py-2.5 rounded-xl text-xs font-semibold min-h-[44px] bg-amber-500/10 text-amber-800 dark:text-amber-200 border border-amber-500/30 hover:bg-amber-500/20 transition-all"
                    >
                      <BookmarkPlus className="w-4 h-4 text-amber-600 dark:text-amber-400" />
                      <span>Lưu từ đọc sai thành thẻ Flashcard</span>
                    </button>
                  )}

                  <button
                    type="button"
                    onClick={handleSaveShadowingToNotebook}
                    className="flex items-center gap-2 px-3.5 py-2.5 rounded-xl text-xs font-semibold min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-primary,#18181b)] dark:text-zinc-200 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 hover:bg-zinc-200 dark:hover:bg-zinc-700 transition-all"
                  >
                    <NotebookPen className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
                    <span>Lưu kết quả vào Sổ tay</span>
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {/* ----------------- CHẾ ĐỘ 2: NGHE & CHÉP CHÍNH TẢ (DICTATION) ----------------- */}
      {activeMode === "dictation" && (
        <div className="space-y-4">
          <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 shadow-sm p-5 sm:p-7 space-y-6">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-4 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
              <div>
                <span className="text-xs font-semibold text-[var(--text-secondary,#71717a)] dark:text-zinc-400 uppercase tracking-wider">
                  Chép Chính Tả (Dictation) #{activeDrillIndex + 1}
                </span>
                <h3 className="text-base sm:text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                  Lắng nghe câu mẫu và hoàn thành lại nội dung
                </h3>
              </div>

              {/* Toggle 2 Submodes */}
              <div className="flex items-center rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 p-1 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700">
                <button
                  type="button"
                  onClick={() => setDictationSubMode("chips")}
                  className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold min-h-[38px] transition-all ${
                    dictationSubMode === "chips"
                      ? "bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-[#5E51E8] dark:text-[#818CF8] shadow-xs"
                      : "text-[var(--text-secondary,#52525b)] dark:text-zinc-400"
                  }`}
                >
                  <Grid className="w-3.5 h-3.5" />
                  <span>Chạm ghép từ (Điện thoại)</span>
                </button>
                <button
                  type="button"
                  onClick={() => setDictationSubMode("type")}
                  className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold min-h-[38px] transition-all ${
                    dictationSubMode === "type"
                      ? "bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-[#5E51E8] dark:text-[#818CF8] shadow-xs"
                      : "text-[var(--text-secondary,#52525b)] dark:text-zinc-400"
                  }`}
                >
                  <Type className="w-3.5 h-3.5" />
                  <span>Gõ phím (Máy tính)</span>
                </button>
              </div>
            </div>

            {/* Audio Listening Control Panel */}
            <div className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/70 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700/60 flex flex-wrap items-center justify-between gap-3">
              <div className="flex flex-wrap items-center gap-2">
                <button
                  type="button"
                  onClick={() => handlePlaySampleAudio(1.0)}
                  className="flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-sm min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs transition-all"
                >
                  <Volume2 className="w-4 h-4" />
                  <span>Nghe câu mẫu (1.0x)</span>
                </button>

                <button
                  type="button"
                  onClick={() => handlePlaySampleAudio(0.75)}
                  className="flex items-center gap-1.5 px-3.5 py-2.5 rounded-xl text-xs font-medium min-h-[44px] bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 text-[var(--text-primary,#18181b)] dark:text-zinc-200 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 hover:bg-zinc-100 dark:hover:bg-zinc-800"
                >
                  <Sliders className="w-3.5 h-3.5" />
                  <span>Nghe chậm (0.75x)</span>
                </button>
              </div>

              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => setShowFirstLetterHint(!showFirstLetterHint)}
                  className="flex items-center gap-1.5 px-3 py-2 rounded-xl text-xs font-medium min-h-[44px] text-amber-700 dark:text-amber-300 hover:bg-amber-500/10"
                >
                  <Lightbulb className="w-4 h-4 text-amber-500" />
                  <span>{showFirstLetterHint ? "Ẩn gợi ý" : "Xem gợi ý chữ đầu"}</span>
                </button>

                <button
                  type="button"
                  onClick={() => setShowFullAnswer(!showFullAnswer)}
                  className="flex items-center gap-1.5 px-3 py-2 rounded-xl text-xs font-medium min-h-[44px] text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:bg-zinc-200 dark:hover:bg-zinc-800"
                >
                  {showFullAnswer ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                  <span>{showFullAnswer ? "Ẩn đáp án" : "Hiện đáp án"}</span>
                </button>
              </div>
            </div>

            {/* Hint or Answer Reveal */}
            {showFirstLetterHint && (
              <div className="p-3.5 rounded-xl bg-amber-500/10 border border-amber-500/30 text-xs text-amber-800 dark:text-amber-200 space-y-1">
                <span className="font-bold">Gợi ý chữ cái đầu của từng từ:</span>
                <p className="font-mono text-sm tracking-widest">
                  {activeDrill.sentence
                    .split(/\s+/)
                    .map(w => w[0] + "_".repeat(Math.max(1, w.length - 1)))
                    .join(" ")}
                </p>
              </div>
            )}

            {showFullAnswer && (
              <div className="p-3.5 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-xs text-emerald-800 dark:text-emerald-200 space-y-1">
                <span className="font-bold">Đáp án chuẩn:</span>
                <p className="text-sm font-semibold">{activeDrill.sentence}</p>
                <p className="italic text-xs text-emerald-700 dark:text-emerald-300">{activeDrill.meaningVi}</p>
              </div>
            )}

            {/* SUBMODE 1: TYPING ON COMPUTER */}
            {dictationSubMode === "type" && (
              <div className="space-y-4">
                <textarea
                  value={dictationTyped}
                  onChange={e => {
                    setDictationTyped(e.target.value);
                    setDictationChecked(false);
                  }}
                  rows={3}
                  placeholder="Gõ lại câu tiếng Anh bạn nghe được vào đây..."
                  className="w-full p-4 rounded-xl text-base bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-[#5E51E8]"
                />

                <div className="flex items-center gap-3">
                  <button
                    type="button"
                    onClick={handleCheckDictation}
                    className="px-5 py-2.5 rounded-xl font-semibold text-sm min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs"
                  >
                    Kiểm tra đáp án
                  </button>

                  <button
                    type="button"
                    onClick={() => {
                      setDictationTyped("");
                      setDictationChecked(false);
                    }}
                    className="px-4 py-2.5 rounded-xl font-medium text-xs min-h-[44px] text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:bg-zinc-200 dark:hover:bg-zinc-800"
                  >
                    Xóa làm lại
                  </button>
                </div>

                {/* Diff Comparison */}
                {dictationChecked && (
                  <div className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-3">
                    <div className="flex items-center justify-between text-xs font-bold">
                      <span>Đối chiếu chi tiết từng từ:</span>
                      <span
                        className={`text-sm px-2.5 py-0.5 rounded-full ${
                          dictationAccuracy >= 80
                            ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
                            : "bg-amber-500/10 text-amber-600 dark:text-amber-400"
                        }`}
                      >
                        Độ chính xác: {dictationAccuracy}%
                      </span>
                    </div>

                    <div className="flex flex-wrap gap-2 text-sm">
                      {dictationDiffItems.map((item, idx) => (
                        <div
                          key={idx}
                          className={`flex items-center gap-1 px-2.5 py-1 rounded-md font-medium text-xs ${
                            item.status === "correct"
                              ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border border-emerald-500/30"
                              : item.status === "typo"
                              ? "bg-rose-500/15 text-rose-700 dark:text-rose-300 border border-rose-500/30"
                              : item.status === "missing"
                              ? "bg-amber-500/15 text-amber-800 dark:text-amber-300 border border-amber-500/30"
                              : "bg-zinc-500/15 text-zinc-700 dark:text-zinc-300 border border-zinc-500/30"
                          }`}
                        >
                          {item.status === "correct" && <span>{item.targetWord}</span>}
                          {item.status === "typo" && (
                            <>
                              <span className="line-through opacity-70">{item.userWord}</span>
                              <span className="font-bold text-rose-600 dark:text-rose-400">→ {item.targetWord}</span>
                              {item.targetWord && (
                                <button
                                  type="button"
                                  onClick={() => handleSaveToFlashcardDirect({ front: item.targetWord!, back: `Từ đúng trong câu chính tả: "${activeDrill.sentence}"\nNghĩa câu: ${activeDrill.meaningVi}`, example: activeDrill.sentence })}
                                  className="ml-1 p-0.5 text-[10px] bg-amber-500/20 text-amber-800 dark:text-amber-200 rounded hover:bg-amber-500/40"
                                  title="Lưu từ này vào Flashcard FSRS"
                                >
                                  + Thẻ FSRS
                                </button>
                              )}
                            </>
                          )}
                          {item.status === "missing" && (
                            <>
                              <span>[Thiếu: {item.targetWord}]</span>
                              {item.targetWord && (
                                <button
                                  type="button"
                                  onClick={() => handleSaveToFlashcardDirect({ front: item.targetWord!, back: `Từ thiếu trong câu chính tả: "${activeDrill.sentence}"\nNghĩa câu: ${activeDrill.meaningVi}`, example: activeDrill.sentence })}
                                  className="ml-1 p-0.5 text-[10px] bg-amber-500/20 text-amber-800 dark:text-amber-200 rounded hover:bg-amber-500/40"
                                  title="Lưu từ này vào Flashcard FSRS"
                                >
                                  + Thẻ FSRS
                                </button>
                              )}
                            </>
                          )}
                          {item.status === "extra" && <span>[Thừa: {item.userWord}]</span>}
                        </div>
                      ))}
                    </div>

                    {missedDictationWords.length > 0 && (
                      <div className="pt-2 flex items-center gap-2">
                        <button
                          type="button"
                          onClick={handleSaveMissedDictationToFlashcards}
                          className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-amber-500/10 text-amber-800 dark:text-amber-200 border border-amber-500/30 hover:bg-amber-500/20 transition-all min-h-[38px]"
                        >
                          <BookmarkPlus className="w-3.5 h-3.5 text-amber-600 dark:text-amber-400" />
                          <span>Lưu {missedDictationWords.length} từ gõ sai/thiếu vào Flashcard FSRS</span>
                        </button>
                      </div>
                    )}
                  </div>
                )}
              </div>
            )}

            {/* SUBMODE 2: CHIPS ON MOBILE (390px OPTIMIZED) */}
            {dictationSubMode === "chips" && (
              <div className="space-y-4">
                {/* Placed chips container */}
                <div className="min-h-[70px] p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/80 border-2 border-dashed border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 flex flex-wrap gap-2 items-center">
                  {placedChips.length === 0 ? (
                    <span className="text-xs text-[var(--text-secondary,#71717a)] dark:text-zinc-500 italic">
                      Chạm vào các thẻ từ vựng bên dưới theo đúng thứ tự để xếp thành câu...
                    </span>
                  ) : (
                    placedChips.map(chip => (
                      <button
                        key={chip.id}
                        type="button"
                        onClick={() => handleChipClick(chip, "placed")}
                        className="px-3 py-2 rounded-xl text-xs font-bold bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs min-h-[44px] transition-all flex items-center gap-1.5"
                      >
                        <span>{chip.word}</span>
                        <span className="text-white/60 text-[10px]">✕</span>
                      </button>
                    ))
                  )}
                </div>

                {/* Available chips pool */}
                <div className="space-y-1.5">
                  <span className="text-xs font-bold text-[var(--text-secondary,#52525b)] dark:text-zinc-400">
                    Kho từ vựng có sẵn (kèm từ gây nhiễu):
                  </span>
                  <div className="flex flex-wrap gap-2 p-3 rounded-xl bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 min-h-[60px]">
                    {availableChips.map(chip => (
                      <button
                        key={chip.id}
                        type="button"
                        onClick={() => handleChipClick(chip, "available")}
                        className="px-3.5 py-2.5 rounded-xl text-xs font-semibold bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-primary,#18181b)] dark:text-zinc-200 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 hover:border-[#5E51E8] min-h-[44px] active:scale-95 transition-all"
                      >
                        {chip.word}
                      </button>
                    ))}
                  </div>
                </div>

                {/* Chip control actions */}
                <div className="flex items-center gap-3 pt-2">
                  <button
                    type="button"
                    onClick={handleCheckDictation}
                    className="px-5 py-2.5 rounded-xl font-semibold text-sm min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs"
                  >
                    Kiểm tra ghép từ
                  </button>

                  <button
                    type="button"
                    onClick={handleResetChips}
                    className="px-4 py-2.5 rounded-xl font-medium text-xs min-h-[44px] text-[var(--text-secondary,#52525b)] dark:text-zinc-400 hover:bg-zinc-200 dark:hover:bg-zinc-800"
                  >
                    Xếp lại từ đầu
                  </button>
                </div>

                {/* Result check */}
                {dictationChecked && (
                  <div className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 space-y-3 animate-fadeIn">
                    <div className="flex items-center justify-between text-xs font-bold">
                      <div className="flex items-center gap-2">
                        {dictationAccuracy === 100 ? (
                          <CheckCircle2 className="w-5 h-5 text-emerald-600 dark:text-emerald-400 shrink-0" />
                        ) : (
                          <AlertCircle className="w-5 h-5 text-amber-600 dark:text-amber-400 shrink-0" />
                        )}
                        <span>{dictationAccuracy === 100 ? "Xuất sắc! Ghép chính xác 100%!" : "Kết quả đối chiếu ghép từ:"}</span>
                      </div>
                      <span
                        className={`text-sm px-2.5 py-0.5 rounded-full ${
                          dictationAccuracy >= 80
                            ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
                            : "bg-amber-500/10 text-amber-600 dark:text-amber-400"
                        }`}
                      >
                        Độ chính xác: {dictationAccuracy}%
                      </span>
                    </div>

                    <div className="flex flex-wrap gap-2 text-sm">
                      {dictationDiffItems.map((item, idx) => (
                        <div
                          key={idx}
                          className={`flex items-center gap-1 px-2.5 py-1 rounded-md font-medium text-xs ${
                            item.status === "correct"
                              ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border border-emerald-500/30"
                              : item.status === "typo"
                              ? "bg-rose-500/15 text-rose-700 dark:text-rose-300 border border-rose-500/30"
                              : item.status === "missing"
                              ? "bg-amber-500/15 text-amber-800 dark:text-amber-300 border border-amber-500/30"
                              : "bg-zinc-500/15 text-zinc-700 dark:text-zinc-300 border border-zinc-500/30"
                          }`}
                        >
                          {item.status === "correct" && <span>{item.targetWord}</span>}
                          {item.status === "typo" && (
                            <>
                              <span className="line-through opacity-70">{item.userWord}</span>
                              <span className="font-bold text-rose-600 dark:text-rose-400">→ {item.targetWord}</span>
                              {item.targetWord && (
                                <button
                                  type="button"
                                  onClick={() => handleSaveToFlashcardDirect({ front: item.targetWord!, back: `Từ đúng trong câu: "${activeDrill.sentence}"\nNghĩa câu: ${activeDrill.meaningVi}`, example: activeDrill.sentence })}
                                  className="ml-1 p-0.5 text-[10px] bg-amber-500/20 text-amber-800 dark:text-amber-200 rounded hover:bg-amber-500/40"
                                  title="Lưu từ này vào Flashcard FSRS"
                                >
                                  + Thẻ FSRS
                                </button>
                              )}
                            </>
                          )}
                          {item.status === "missing" && (
                            <>
                              <span>[Thiếu: {item.targetWord}]</span>
                              {item.targetWord && (
                                <button
                                  type="button"
                                  onClick={() => handleSaveToFlashcardDirect({ front: item.targetWord!, back: `Từ thiếu trong câu: "${activeDrill.sentence}"\nNghĩa câu: ${activeDrill.meaningVi}`, example: activeDrill.sentence })}
                                  className="ml-1 p-0.5 text-[10px] bg-amber-500/20 text-amber-800 dark:text-amber-200 rounded hover:bg-amber-500/40"
                                  title="Lưu từ này vào Flashcard FSRS"
                                >
                                  + Thẻ FSRS
                                </button>
                              )}
                            </>
                          )}
                          {item.status === "extra" && <span>[Thừa: {item.userWord}]</span>}
                        </div>
                      ))}
                    </div>

                    {missedDictationWords.length > 0 && (
                      <div className="pt-2 flex items-center gap-2">
                        <button
                          type="button"
                          onClick={handleSaveMissedDictationToFlashcards}
                          className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-amber-500/10 text-amber-800 dark:text-amber-200 border border-amber-500/30 hover:bg-amber-500/20 transition-all min-h-[38px]"
                        >
                          <BookmarkPlus className="w-3.5 h-3.5 text-amber-600 dark:text-amber-400" />
                          <span>Lưu {missedDictationWords.length} từ sai/thiếu vào Flashcard FSRS</span>
                        </button>
                      </div>
                    )}
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      )}

      {/* ----------------- CHẾ ĐỘ 3: LUYỆN NÓI THEO CHỦ ĐỀ (SPEAKING CUE CARD) ----------------- */}
      {activeMode === "speaking" && (
        <div className="space-y-4">
          <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 shadow-sm p-5 sm:p-7 space-y-6">
            {/* Header info */}
            <div className="flex flex-wrap items-center justify-between gap-2 pb-4 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
              <div>
                <span className="text-xs font-semibold text-[var(--text-secondary,#71717a)] dark:text-zinc-400 uppercase tracking-wider">
                  IELTS Speaking Part 2 & 3 Cue Card
                </span>
                <h3 className="text-base sm:text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                  {activeDrill.speakingPrompt}
                </h3>
              </div>

              {/* Timers */}
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => setPrepTimerActive(!prepTimerActive)}
                  className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold min-h-[44px] transition-all border ${
                    prepTimerActive
                      ? "bg-amber-500 text-white border-amber-600 animate-pulse"
                      : "bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-amber-700 dark:text-amber-300 border-amber-500/30"
                  }`}
                >
                  <Timer className="w-4 h-4" />
                  <span>Chuẩn bị: {prepSeconds}s</span>
                </button>

                <button
                  type="button"
                  onClick={() => setSpeakTimerActive(!speakTimerActive)}
                  className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold min-h-[44px] transition-all border ${
                    speakTimerActive
                      ? "bg-emerald-500 text-white border-emerald-600 animate-pulse"
                      : "bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-emerald-700 dark:text-emerald-300 border-emerald-500/30"
                  }`}
                >
                  <Mic className="w-4 h-4" />
                  <span>Nói: {speakSeconds}s</span>
                </button>
              </div>
            </div>

            {/* Zero-Facade Warning when SpeechRecognition is not supported */}
            {!speechRecSupported && (
              <div className="p-4 rounded-xl bg-amber-500/10 border border-amber-500/30 text-xs text-amber-900 dark:text-amber-200 space-y-2">
                <div className="flex items-center gap-2 font-bold text-sm">
                  <AlertCircle className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0" />
                  <span>Trình duyệt hiện tại chưa hỗ trợ nhận dạng giọng nói tự động (SpeechRecognition)</span>
                </div>
                <p className="leading-relaxed">
                  Khuyến nghị dùng Google Chrome hoặc Microsoft Edge trên máy tính / Android để trải nghiệm tính năng chấm phát âm trực tiếp. Hệ thống tuân thủ nguyên tắc <strong>Zero-Facade</strong>: KHÔNG tự tạo điểm số giả khi thiếu API nhận diện. Bạn vẫn có thể ghi âm câu nói và tự đối chiếu qua phần ghi chú hoặc chuyển sang các chế độ Shadowing, Dictation, Sơ đồ từ vựng.
                </p>
              </div>
            )}

            {/* Mic Permission Denied Banner */}
            {isMicPermissionDenied && (
              <div className="p-3.5 rounded-xl bg-rose-500/10 border border-rose-500/30 text-xs text-rose-900 dark:text-rose-200 space-y-1">
                <div className="flex items-center gap-2 font-bold">
                  <AlertCircle className="w-4 h-4 text-rose-600 dark:text-rose-400 shrink-0" />
                  <span>Không thể truy cập microphone</span>
                </div>
                <p>
                  Trình duyệt đã từ chối quyền microphone hoặc không tìm thấy micro. Vui lòng cấp quyền micro cho trang web trong cài đặt trình duyệt để ghi âm.
                </p>
              </div>
            )}

            {/* Pronunciation Recognition Score (Zero-Facade Honest Label) */}
            {speechRecSupported && effectiveSpokenText && (
              <div className="p-4 rounded-xl bg-indigo-500/10 border border-indigo-500/30 space-y-3 animate-fadeIn">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <div className="flex items-center gap-2">
                    <Award className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
                    <span className="font-bold text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                      Điểm ước lượng theo nhận dạng giọng nói:{" "}
                      <span className={shadowingAccuracy >= 70 ? "text-emerald-600 dark:text-emerald-400 font-extrabold" : "text-amber-600 dark:text-amber-400 font-extrabold"}>
                        {shadowingAccuracy}%
                      </span>
                    </span>
                  </div>
                  <button
                    type="button"
                    onClick={handleRecordSpeakingScore}
                    className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold bg-[#5E51E8] text-white hover:bg-[#4F46E5] min-h-[38px] shadow-xs"
                  >
                    <Check className="w-3.5 h-3.5" />
                    <span>Lưu kết quả phát âm</span>
                  </button>
                </div>

                <div className="flex flex-wrap gap-2 pt-1">
                  {shadowingWordResults.map((item, idx) => (
                    <div
                      key={idx}
                      className={`flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-semibold ${
                        item.status === "correct"
                          ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border border-emerald-500/30"
                          : item.status === "close"
                          ? "bg-amber-500/15 text-amber-800 dark:text-amber-300 border border-amber-500/30"
                          : "bg-rose-500/15 text-rose-700 dark:text-rose-300 border border-rose-500/30"
                      }`}
                    >
                      <span>{item.word}</span>
                      {item.status === "incorrect" && (
                        <button
                          type="button"
                          onClick={() => handleSaveToFlashcardDirect({ front: item.word, back: `Từ phát âm chưa chuẩn: "${activeDrill.sentence}"\nNghĩa câu: ${activeDrill.meaningVi}`, example: activeDrill.sentence })}
                          className="ml-1 p-0.5 text-[10px] bg-amber-500/20 text-amber-800 dark:text-amber-200 rounded hover:bg-amber-500/40"
                          title="Lưu từ này vào Flashcard FSRS"
                        >
                          + Thẻ FSRS
                        </button>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Outline suggestions */}
            <div className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800/60 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700/60 space-y-2">
              <span className="text-xs font-bold text-[#5E51E8] dark:text-[#818CF8] uppercase tracking-wider">
                Gợi ý 3 ý chính để phát triển bài nói:
              </span>
              <ul className="space-y-1.5 text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
                {activeDrill.speakingOutline.map((point, idx) => (
                  <li key={idx} className="flex items-start gap-2">
                    <span className="w-5 h-5 rounded-full bg-[#5E51E8]/10 text-[#5E51E8] dark:text-[#818CF8] font-bold text-[11px] flex items-center justify-center shrink-0 mt-0.5">
                      {idx + 1}
                    </span>
                    <span>{point}</span>
                  </li>
                ))}
              </ul>
            </div>

            {/* Key collocations tracker with Live Green Checkmark */}
            <div className="space-y-2.5">
              <div className="flex items-center justify-between text-xs">
                <span className="font-bold text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
                  Cụm từ ghi điểm nên dùng (Hệ thống tự động phát hiện khi bạn nói):
                </span>
                <span className="font-semibold text-emerald-600 dark:text-emerald-400">
                  Đã dùng: {detectedCount}/{activeDrill.keyPhrases.length} cụm
                </span>
              </div>

              <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
                {detectedPhrasesStatus.map((item, idx) => (
                  <div
                    key={idx}
                    className={`flex items-center justify-between p-2.5 rounded-xl border text-xs font-semibold transition-all ${
                      item.isMatched
                        ? "bg-emerald-500/15 border-emerald-500/50 text-emerald-800 dark:text-emerald-200"
                        : "bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 text-[var(--text-secondary,#71717a)] dark:text-zinc-400"
                    }`}
                  >
                    <span>{item.phrase}</span>
                    {item.isMatched ? (
                      <Check className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
                    ) : (
                      <span className="w-2 h-2 rounded-full bg-zinc-300 dark:bg-zinc-600 shrink-0" />
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Mic recording / typing notes */}
            <div className="space-y-3 pt-2">
              <div className="flex items-center justify-between">
                <span className="text-xs font-bold text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
                  Ghi chú hoặc Bật Mic để thực hành:
                </span>
                <button
                  type="button"
                  onClick={handleToggleRecording}
                  className={`flex items-center gap-2 px-3.5 py-1.5 rounded-xl text-xs font-bold min-h-[44px] transition-all ${
                    isRecording
                      ? "bg-rose-600 text-white animate-pulse"
                      : "bg-[#5E51E8] text-white hover:bg-[#4F46E5]"
                  }`}
                >
                  <Mic className="w-4 h-4" />
                  <span>{isRecording ? "Đang ghi âm... Chạm để dừng" : "Bật Mic Nói"}</span>
                </button>
              </div>

              <textarea
                value={speakingNotes || spokenTranscript}
                onChange={e => setSpeakingNotes(e.target.value)}
                rows={3}
                placeholder="Ghi chú dàn ý hoặc quan sát lời nói được nhận diện trực tiếp tại đây..."
                className="w-full p-3.5 rounded-xl text-sm bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 text-[var(--text-primary,#18181b)] dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-[#5E51E8]"
              />

              <div className="flex flex-wrap items-center gap-2">
                <button
                  type="button"
                  disabled={speakingAiLoading}
                  onClick={handleRequestAiSpeakingReview}
                  className="flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-xs min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs disabled:opacity-50"
                >
                  <Sparkles className="w-4 h-4" />
                  <span>{speakingAiLoading ? "AI đang đánh giá bài nói..." : "AI Nhận Xét Bài Nói"}</span>
                </button>

                <button
                  type="button"
                  onClick={() => setShowSampleAnswer(!showSampleAnswer)}
                  className="flex items-center gap-1.5 px-3.5 py-2.5 rounded-xl text-xs font-medium min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-secondary,#52525b)] dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-700"
                >
                  <BookOpen className="w-4 h-4" />
                  <span>{showSampleAnswer ? "Ẩn bài nói mẫu" : "Xem bài nói mẫu Band 7.5+"}</span>
                </button>

                <button
                  type="button"
                  onClick={handleSaveSpeakingToNotebook}
                  className="flex items-center gap-1.5 px-3.5 py-2.5 rounded-xl text-xs font-medium min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-primary,#18181b)] dark:text-zinc-200 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 hover:bg-zinc-200 dark:hover:bg-zinc-700 ml-auto"
                >
                  <NotebookPen className="w-4 h-4 text-[#5E51E8] dark:text-[#818CF8]" />
                  <span>Lưu bài nói vào Sổ tay</span>
                </button>
              </div>

              {/* Sample Answer Box */}
              {showSampleAnswer && (
                <div className="p-4 rounded-xl bg-indigo-500/10 border border-indigo-500/30 text-xs text-indigo-950 dark:text-indigo-200 space-y-2 animate-fadeIn">
                  <span className="font-bold text-sm text-[#5E51E8] dark:text-[#818CF8]">
                    Bài Nói Mẫu (Band 7.5 - 8.0 Model Answer):
                  </span>
                  <p className="leading-relaxed text-sm">{activeDrill.sampleAnswer}</p>
                </div>
              )}

              {/* AI Feedback Display */}
              {speakingAiFeedback && (
                <div className="p-4 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-xs text-emerald-950 dark:text-emerald-200 space-y-2 animate-fadeIn">
                  <div className="flex items-center gap-2 font-bold text-sm text-emerald-700 dark:text-emerald-300">
                    <Sparkles className="w-4 h-4" />
                    <span>Đánh Giá Chi Tiết Từ AI:</span>
                  </div>
                  <p className="whitespace-pre-line leading-relaxed text-xs">{speakingAiFeedback}</p>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* ----------------- CHẾ ĐỘ 4: SƠ ĐỒ TỪ VỰNG (WORD MAP / MIND MAP) ----------------- */}
      {activeMode === "word_map" && (
        <div className="space-y-4">
          <EnglishWordMapView
            wordMap={activeWordMap}
            onSpeak={(text, rate) => playWebSpeechTts(text, rate || ttsSpeed, voiceAccent)}
            onSaveToFlashcard={handleSaveToFlashcardDirect}
            onComplete={async () => {
              await recordPracticeAttempt({
                mode: "word_map",
                score: 100,
                isPassed: true,
                activityTitle: `${activeDrill.topic} · Sơ đồ từ vựng (${activeWordMap.centralWord})`,
                details: {
                  centralWord: activeWordMap.centralWord,
                  collocationsCount: activeWordMap.collocations.length,
                  phrasalVerbsCount: activeWordMap.phrasalVerbs.length,
                  synonymsCount: activeWordMap.synonyms.length,
                }
              });
            }}
            isCompleted={currentModeAttempts.some(a => a.isPassed)}
            bestScore={currentModeAttempts.length > 0 ? Math.max(...currentModeAttempts.map(a => a.score)) : null}
          />
        </div>
      )}

      {/* ----------------- CHẾ ĐỘ 5: KIỂM TRA NHANH TỪ VỰNG (MINI-QUIZ) ----------------- */}
      {activeMode === "quiz" && (
        <div className="space-y-4">
          <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 shadow-sm p-5 sm:p-7 space-y-6">
            {!quizCompleted ? (
              <>
                <div className="flex items-center justify-between pb-4 border-b border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800">
                  <div>
                    <span className="text-xs font-semibold text-[var(--text-secondary,#71717a)] dark:text-zinc-400 uppercase tracking-wider">
                      Mini-Quiz Kiểm Tra Nhanh
                    </span>
                    <h3 className="text-base sm:text-lg font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                      Câu hỏi {currentQuizIdx + 1} / {quizQuestions.length}
                    </h3>
                  </div>

                  <span className="px-3 py-1 rounded-full text-xs font-bold bg-[#5E51E8]/10 text-[#5E51E8] dark:text-[#818CF8]">
                    Điểm số: {quizScore}
                  </span>
                </div>

                <div className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-sm sm:text-base font-semibold text-[var(--text-primary,#18181b)] dark:text-zinc-100 whitespace-pre-line">
                  {activeQuiz.question}
                </div>

                {/* 4 Choices */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  {activeQuiz.options.map((opt, idx) => {
                    const isSelected = selectedQuizChoice === opt;
                    const isCorrect = opt === activeQuiz.correctAnswer;

                    let btnStyle =
                      "bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900 border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 text-[var(--text-primary,#18181b)] dark:text-zinc-200 hover:border-[#5E51E8]";

                    if (quizAnswerChecked) {
                      if (isCorrect) {
                        btnStyle = "bg-emerald-500/20 border-emerald-500 text-emerald-800 dark:text-emerald-200 font-bold";
                      } else if (isSelected && !isCorrect) {
                        btnStyle = "bg-rose-500/20 border-rose-500 text-rose-800 dark:text-rose-200";
                      } else {
                        btnStyle = "opacity-50 border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800";
                      }
                    } else if (isSelected) {
                      btnStyle = "bg-[#5E51E8]/10 border-[#5E51E8] text-[#5E51E8] dark:text-[#818CF8] font-bold";
                    }

                    return (
                      <button
                        key={idx}
                        type="button"
                        onClick={() => handleSelectQuizOption(opt)}
                        className={`p-3.5 rounded-xl border text-left text-sm min-h-[48px] flex items-center justify-between transition-all ${btnStyle}`}
                      >
                        <span>{opt}</span>
                        {quizAnswerChecked && isCorrect && (
                          <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
                        )}
                        {quizAnswerChecked && isSelected && !isCorrect && (
                          <XCircle className="w-4 h-4 text-rose-600 dark:text-rose-400 shrink-0" />
                        )}
                      </button>
                    );
                  })}
                </div>

                {/* Explanation */}
                {quizAnswerChecked && (
                  <div className="p-4 rounded-xl bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 text-xs space-y-1.5 animate-fadeIn">
                    <span className="font-bold text-emerald-600 dark:text-emerald-400">Giải thích nghĩa & ngữ pháp:</span>
                    <p className="text-[var(--text-secondary,#52525b)] dark:text-zinc-300 leading-relaxed">
                      {activeQuiz.explanation}
                    </p>
                  </div>
                )}

                {/* Actions */}
                <div className="flex items-center gap-3 pt-2">
                  {!quizAnswerChecked ? (
                    <button
                      type="button"
                      disabled={!selectedQuizChoice}
                      onClick={handleCheckQuizAnswer}
                      className="px-5 py-2.5 rounded-xl font-semibold text-sm min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs disabled:opacity-50"
                    >
                      Kiểm tra
                    </button>
                  ) : (
                    <button
                      type="button"
                      onClick={handleNextQuizQuestion}
                      className="px-5 py-2.5 rounded-xl font-semibold text-sm min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs flex items-center gap-1.5"
                    >
                      <span>{currentQuizIdx + 1 < quizQuestions.length ? "Câu tiếp theo" : "Xem tổng kết"}</span>
                      <ChevronRight className="w-4 h-4" />
                    </button>
                  )}
                </div>
              </>
            ) : (
              /* Quiz Summary Card */
              <div className="text-center py-8 space-y-4 animate-fadeIn">
                <div className="w-16 h-16 rounded-full bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 mx-auto flex items-center justify-center">
                  <Award className="w-8 h-8" />
                </div>
                <h3 className="text-xl font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                  Hoàn Thành Bài Kiểm Tra Nhanh!
                </h3>
                <p className="text-sm text-[var(--text-secondary,#52525b)] dark:text-zinc-400">
                  Bạn đã trả lời đúng <strong className="text-emerald-600 dark:text-emerald-400 font-bold">{quizScore}</strong> trên tổng số {quizQuestions.length} câu hỏi.
                </p>

                <div className="flex flex-wrap items-center justify-center gap-3 pt-4">
                  <button
                    type="button"
                    onClick={handleRestartQuiz}
                    className="flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-xs min-h-[44px] bg-[var(--surface-elevated,#f4f4f5)] dark:bg-zinc-800 text-[var(--text-primary,#18181b)] dark:text-zinc-200 border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700 hover:bg-zinc-200 dark:hover:bg-zinc-700"
                  >
                    <RotateCcw className="w-4 h-4" />
                    <span>Làm lại bài kiểm tra</span>
                  </button>

                  <button
                    type="button"
                    onClick={async () => {
                      await onSaveToNotebook(
                        `### 🧩 Kết Quả Mini-Quiz: ${activeDrill.topic}\n- Điểm số: **${quizScore}/${quizQuestions.length}** câu đúng.\n- Hoàn thành lúc: ${new Date().toLocaleTimeString("vi-VN")}`
                      );
                      showToast("Đã lưu kết quả Mini-Quiz vào Sổ tay!", "success");
                    }}
                    className="flex items-center gap-2 px-4 py-2.5 rounded-xl font-semibold text-xs min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-xs"
                  >
                    <NotebookPen className="w-4 h-4" />
                    <span>Lưu kết quả vào Sổ tay</span>
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default EnglishPracticeLab;
