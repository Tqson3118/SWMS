"use client";

import React, { useState, useMemo, useEffect, useRef, useCallback } from "react";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";
import { resolvePracticeSelection, type PsychTabMode } from "@/lib/practice-helpers";
import { normalizePsychologyActivities } from "@/lib/writein-practice";
import {
  ApplyJournalWriteInLab,
  BiasSpotterWriteInLab,
  CbtExerciseWriteInLab,
  FeynmanWriteInLab,
} from "./PsychologyWriteInViews";
import { Timer, Activity, ListChecks, CheckSquare, Target, Play, RotateCcw } from "lucide-react";

import {
  Brain,
  MessageSquare,
  Heart,
  Shield,
  Sparkles,
  BookOpen,
  CheckCircle2,
  AlertCircle,
  ArrowRight,
  RefreshCw,
  Send,
  ChevronRight,
  HelpCircle,
  ThumbsUp,
  Compass,
  Award,
  BookmarkCheck,
  Scale,
  Zap,
  Info,
  User,
  Smile,
  Frown,
  Meh,
  Eye,
  Check,
  Flame,
  Volume2,
} from "lucide-react";

export interface PracticeModuleProps {
  courseId: string;
  courseTitle: string;
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  onSaveToNotebook: (textToAppend: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  initialActivityType?: string | null;
  initialActivityKey?: string | null;
}

/* =========================================================================
   1. TYPES & DATA: CHẾ ĐỘ 1 - MÔ PHỎNG HỘI THOẠI THỰC TẾ (ROLEPLAY)
   ========================================================================= */

interface NvcFeedback {
  observation: string; // Nêu sự việc khách quan
  feeling: string;     // Nói cảm xúc thật
  need: string;        // Nêu nhu cầu / giá trị
  request: string;     // Đưa đề nghị cụ thể
}

interface RoleplayOption {
  id: string;
  userReplyText: string;
  styleLabel: "Giao tiếp Trắc ẩn (NVC)" | "Hung hăng / Công kích" | "Thụ động / Né tránh" | "Khéo léo / Giữ ranh giới" | "Phán xét / Áp đặt";
  isOptimal: boolean;
  scoreRespect: number; // 0 - 10
  scoreEmpathy: number; // 0 - 10
  partnerResponse: string;
  partnerMood: "happy" | "neutral" | "angry" | "relieved" | "defensive";
  nvcAnalysis: NvcFeedback;
  coachingTip: string;
  nextTurnId?: string | null; // null if end of branch
}

interface RoleplayTurn {
  id: string;
  partnerPrompt: string;
  partnerContextHint?: string;
  options: RoleplayOption[];
}

interface RoleplayScenario {
  id: string;
  title: string;
  description: string;
  badge: string;
  partnerName: string;
  partnerRole: string;
  initialContext: string;
  turns: Record<string, RoleplayTurn>;
  startTurnId: string;
}

const ROLEPLAY_SCENARIOS: RoleplayScenario[] = [
  {
    id: "feedback-deadline",
    title: "Góp ý đồng đội trễ hạn mà không gây phòng thủ",
    description: "Bạn và đồng đội cùng làm dự án chung. Đã 22h đêm trước hạn nộp nhưng bạn ấy vẫn chưa gửi phần nội dung, và đây là lần thứ hai trong tuần.",
    badge: "Kỹ năng làm việc nhóm",
    partnerName: "Tuấn Anh",
    partnerRole: "Thành viên cùng nhóm thuyết trình",
    initialContext: "Đồng hồ điểm 22h00. Sáng mai 8h nhóm phải thuyết trình trước lớp/khách hàng. Bạn vừa nhận được tin nhắn từ Tuấn Anh.",
    startTurnId: "turn_1",
    turns: {
      turn_1: {
        id: "turn_1",
        partnerPrompt: "Xin lỗi cậu nhiều nhé! Tối nay nhà mình có chút việc đột xuất nên giờ mới mở máy được, slide phần mình chưa xong hẳn. Hay là mai lên lớp mình mở máy mình chiếu chay luôn, khỏi cần ghép slide được không?",
        partnerContextHint: "Tuấn Anh đang cảm thấy hơi áy náy nhưng cũng ngại làm tiếp trong đêm, muốn tìm giải pháp tạm bợ.",
        options: [
          {
            id: "opt_1a",
            userReplyText: "Lúc nào cậu cũng lấy lý do bận việc gia đình hết vậy? Mai thuyết trình rồi mà giờ còn vô trách nhiệm như thế, ai mà gánh nổi bài của cậu mãi!",
            styleLabel: "Hung hăng / Công kích",
            isOptimal: false,
            scoreRespect: 2,
            scoreEmpathy: 1,
            partnerResponse: "Cậu nói chuyện kiểu gì thế? Ai chẳng có lúc kẹt việc đột xuất? Tôi đã xin lỗi rồi còn dán nhãn người khác. Không đợi được thì cậu tự đi mà làm hết đi!",
            partnerMood: "defensive",
            nvcAnalysis: {
              observation: "Bạn dùng từ phóng đại 'Lúc nào cũng' thay vì quan sát khách quan về sự việc tối nay.",
              feeling: "Bạn trút sự bực dọc và bất an bằng cách công kích cá nhân.",
              need: "Nhu cầu về sự an tâm và trách nhiệm chung bị biến thành sự buộc tội.",
              request: "Không có đề nghị cụ thể nào được đưa ra ngoài việc sỉ nhục nhau.",
            },
            coachingTip: "Khi bạn dán nhãn đối phương là 'vô trách nhiệm', não bộ của họ lập tức kích hoạt cơ chế tự vệ (Fight-or-Flight). Cuộc trò chuyện chuyển từ giải quyết công việc sang đối đầu cái tôi.",
            nextTurnId: "turn_2_conflict",
          },
          {
            id: "opt_1b",
            userReplyText: "À... ừ thế cũng được cậu ạ. Miễn mai cậu có bài là được rồi, cố gắng nhé.",
            styleLabel: "Thụ động / Né tránh",
            isOptimal: false,
            scoreRespect: 6,
            scoreEmpathy: 5,
            partnerResponse: "May quá cảm ơn cậu nhé! Vậy mai lên lớp tính tiếp, mình đi ngủ sớm đây mệt quá rồi.",
            partnerMood: "relieved",
            nvcAnalysis: {
              observation: "Bạn né tránh việc đối diện với sự việc 22h đêm chưa có slide.",
              feeling: "Bạn kìm nén sự lo lắng và ức chế của chính mình.",
              need: "Bạn bỏ quên nhu cầu về chất lượng bài thuyết trình và sự công bằng trong nhóm.",
              request: "Không đưa ra bất kỳ thỏa thuận hay giới hạn an toàn nào.",
            },
            coachingTip: "Thụ động né tránh xung đột sẽ tạo ra 'sự bình yên giả tạo'. Sáng mai rủi ro vỡ trận rất cao, và bạn sẽ ôm sự ấm ức trong lòng suốt thời gian dài.",
            nextTurnId: "turn_2_passive",
          },
          {
            id: "opt_1c",
            userReplyText: "Mình hiểu nhà có việc đột xuất thì khó tập trung được. Tuy nhiên hiện tại đã 22h, mai là hạn chót và mình rất lo lắng nếu bài chưa khớp nội dung thì sáng mai cả nhóm sẽ bị trừ điểm phong cách. Nhóm mình cần slide hoàn chỉnh trước 23h30 để duyệt lại lần cuối. Cậu có thể gửi ngay dàn ý các ý chính trong 30 phút nữa không, phần trình bày slide mình sẽ hỗ trợ ráp vào giúp cậu?",
            styleLabel: "Giao tiếp Trắc ẩn (NVC)",
            isOptimal: true,
            scoreRespect: 10,
            scoreEmpathy: 9,
            partnerResponse: "Cảm ơn cậu nhiều vì đã thông cảm và chịu hỗ trợ mình! Mình hiểu cậu đang rất lo. Mình mở file gửi dàn ý và số liệu thô qua Zalo cho cậu ngay trong 20 phút nữa nhé, nhờ cậu format giúp mình!",
            partnerMood: "happy",
            nvcAnalysis: {
              observation: "Nêu rõ sự thật: Hiện tại đã 22h, mai là hạn chót (không suy diễn, không dán nhãn).",
              feeling: "Nói thật cảm xúc của mình: 'mình rất lo lắng nếu bài chưa khớp'.",
              need: "Nêu rõ giá trị: Nhóm cần sự an tâm và đồng bộ trước 23h30.",
              request: "Đề nghị cụ thể & hành động hỗ trợ: Gửi dàn ý trong 30 phút + mình hỗ trợ format.",
            },
            coachingTip: "Khi kết hợp 'Thấu cảm hoàn cảnh' + 'Giữ vững tiêu chuẩn công việc' + 'Đưa giải pháp chia lửa', người nghe sẽ không cảm thấy bị tấn công mà cảm thấy có đồng minh cùng vượt khó.",
            nextTurnId: "turn_2_constructive",
          },
        ],
      },
      turn_2_constructive: {
        id: "turn_2_constructive",
        partnerPrompt: "Tuấn Anh gửi file dàn ý lúc 22h25: 'Mình vừa gửi rồi nhé! Có 2 phần số liệu mục 3 mình chưa kịp tổng hợp biểu đồ, cậu xem qua giúp mình với.'",
        partnerContextHint: "Đồng đội đã hợp tác và tin tưởng bạn hơn sau phản hồi trắc ẩn.",
        options: [
          {
            id: "opt_2ca",
            userReplyText: "Nhận được rồi Tuấn Anh ơi. Phần biểu đồ mục 3 mình sẽ dùng template có sẵn vẽ trong 10 phút là xong. Cậu nghỉ ngơi chút rồi 23h15 mình gửi lại bản tổng hợp cho cậu duyệt trước khi đi ngủ nhé. Mai nhóm mình tự tin làm thật tốt!",
            styleLabel: "Giao tiếp Trắc ẩn (NVC)",
            isOptimal: true,
            scoreRespect: 10,
            scoreEmpathy: 10,
            partnerResponse: "Tuyệt vời quá! Cảm ơn cậu đã làm việc rất chuyên nghiệp và tử tế. Mai mình bao cậu chầu cafe sáng nhé!",
            partnerMood: "happy",
            nvcAnalysis: {
              observation: "Xác nhận đã nhận file và ước lượng thời gian hoàn thành cụ thể (10 phút).",
              feeling: "Tạo không khí tích cực, tự tin và giải tỏa áp lực cho cả hai.",
              need: "Nhu cầu thống nhất phiên bản cuối cùng trước giờ nghỉ.",
              request: "Hẹn mốc 23h15 cùng rà soát lần cuối.",
            },
            coachingTip: "Khen ngợi nỗ lực và chốt hạ mốc thời gian rõ ràng giúp duy trì động lực và củng cố mối quan hệ đồng đội bền vững.",
            nextTurnId: null,
          },
          {
            id: "opt_2cb",
            userReplyText: "Sao số liệu quan trọng nhất lại để sót thế này? Thôi được rồi để tôi tự bơi nốt, cậu đi ngủ đi cho rảnh nợ.",
            styleLabel: "Phán xét / Áp đặt",
            isOptimal: false,
            scoreRespect: 3,
            scoreEmpathy: 2,
            partnerResponse: "Ơ kìa... tôi đã cố gắng gửi nhanh nhất có thể rồi mà cậu lại bảo thế? Lần sau tôi chẳng buồn nhờ cậu nữa.",
            partnerMood: "defensive",
            nvcAnalysis: {
              observation: "Chỉ trích sai sót ngay sau khi đối phương vừa nỗ lực hợp tác.",
              feeling: "Bộc phát cảm xúc chán nản, mỉa mai.",
              need: "Đánh mất cơ hội xây dựng lòng tin.",
              request: "Đuổi khéo đối phương khiến họ cảm thấy nỗ lực của mình bị phủ nhận hoàn toàn.",
            },
            coachingTip: "Giai đoạn về đích rất nhạy cảm. Một câu mỉa mai vào phút chót có thể phá hủy toàn bộ thiện chí vừa gầy dựng được.",
            nextTurnId: null,
          },
        ],
      },
      turn_2_conflict: {
        id: "turn_2_conflict",
        partnerPrompt: "Tuấn Anh nhắn lại trong nhóm: 'Mọi người xem tin nhắn bạn này đi, tôi có việc bận gia đình mà nói chuyện xúc phạm người khác như thế thì tôi rút khỏi nhóm luôn cho nhẹ đầu!'",
        partnerContextHint: "Xung đột leo thang đỉnh điểm. Nguy cơ nhóm tan vỡ ngay trước giờ G.",
        options: [
          {
            id: "opt_2conf_a",
            userReplyText: "Rút thì rút luôn đi, có bạn chỉ tổ kéo điểm nhóm xuống!",
            styleLabel: "Hung hăng / Công kích",
            isOptimal: false,
            scoreRespect: 1,
            scoreEmpathy: 1,
            partnerResponse: "Được, tôi sẽ báo giáo viên ngay bây giờ về việc cậu cô lập và lăng mạ thành viên khác!",
            partnerMood: "angry",
            nvcAnalysis: {
              observation: "Tiếp tục đổ thêm dầu vào lửa, đẩy xung đột lên mức không thể cứu vãn.",
              feeling: "Bốc hỏa và mất kiểm soát cảm xúc.",
              need: "Cả hai đều muốn thắng cái tôi thay vì hướng tới kết quả chung.",
              request: "Cắt đứt hợp tác.",
            },
            coachingTip: "Khi cái tôi thắng, cả nhóm đều thất bại. Đây là cái giá của việc để cơn giận dẫn dắt giao tiếp.",
            nextTurnId: null,
          },
          {
            id: "opt_2conf_b",
            userReplyText: "Tuấn Anh ơi, mình xin lỗi vì lúc nãy quá sốt ruột trước bài thuyết trình sáng mai nên đã dùng lời lẽ gay gắt và thiếu bình tĩnh với cậu. Mình thật sự trân trọng sự đóng góp của cậu cho nhóm. Mình hít một hơi sâu rồi, bây giờ việc quan trọng nhất là sáng mai cả nhóm có bài tốt. Cậu gửi giúp mình những gì cậu đã có nhé, mình cùng hoàn thiện?",
            styleLabel: "Giao tiếp Trắc ẩn (NVC)",
            isOptimal: true,
            scoreRespect: 9,
            scoreEmpathy: 8,
            partnerResponse: "Thôi được rồi... Mình cũng xin lỗi vì không chủ động báo sớm làm cậu lo lắng. Để mình gửi phần đã làm qua cho cậu luôn.",
            partnerMood: "relieved",
            nvcAnalysis: {
              observation: "Dám thừa nhận hành vi thiếu bình tĩnh lúc nãy của mình.",
              feeling: "Thể hiện sự hối lỗi chân thành và mong muốn hàn gắn.",
              need: "Đặt mục tiêu chung của nhóm lên trên cái tôi cá nhân.",
              request: "Mời đối phương quay lại hợp tác trên tinh thần xây dựng.",
            },
            coachingTip: "Biết xin lỗi khi lỡ lời không làm bạn yếu thế đi, mà thể hiện bản lĩnh cảm xúc và sự trưởng thành vượt bậc.",
            nextTurnId: null,
          },
        ],
      },
      turn_2_passive: {
        id: "turn_2_passive",
        partnerPrompt: "7h30 sáng hôm sau tại phòng học: Tuấn Anh hớt hải chạy vào: 'Cậu ơi, máy mình tự nhiên bị lỗi cập nhật Windows không bật lên được, file slide lưu trong máy đó chưa kịp backup lên Drive rồi!'",
        partnerContextHint: "Hậu quả nhãn tiền của việc né tránh thỏa thuận rõ ràng từ đêm hôm trước.",
        options: [
          {
            id: "opt_2pass_a",
            userReplyText: "Trời ơi tôi biết ngay mà! Tối qua tôi đã linh cảm rồi mà tại nể cậu nên không nói, giờ thì cả nhóm ăn 0 điểm nhé!",
            styleLabel: "Hung hăng / Công kích",
            isOptimal: false,
            scoreRespect: 3,
            scoreEmpathy: 2,
            partnerResponse: "Sao tối qua cậu không bảo tôi gửi lên Drive đi? Giờ cậu lại bảo biết ngay với nể tôi?!",
            partnerMood: "defensive",
            nvcAnalysis: {
              observation: "Oán trách và đổ lỗi khi sự việc đã rồi.",
              feeling: "Ấm ức dồn nén từ tối qua bùng phát thành đòn công kích.",
              need: "Không giải quyết được vấn đề thực tế trước mắt.",
              request: "Than thở vô nghĩa.",
            },
            coachingTip: "Né tránh ở hiện tại luôn dẫn đến thảm họa lớn hơn ở tương lai kèm theo sự cay đắng.",
            nextTurnId: null,
          },
          {
            id: "opt_2pass_b",
            userReplyText: "Bình tĩnh nào, còn 25 phút nữa. Trong Drive nhóm mình còn bản phác thảo tuần trước. Tuấn Anh mở điện thoại log in vào tài khoản Google tìm file nháp đó đi, mình sẽ dùng laptop của mình tạo 3 slide tóm tắt ý chính ngay bây giờ. Lần này nhóm mình cùng rút kinh nghiệm sâu sắc về việc đồng bộ file nhé.",
            styleLabel: "Giao tiếp Trắc ẩn (NVC)",
            isOptimal: true,
            scoreRespect: 8,
            scoreEmpathy: 8,
            partnerResponse: "May quá mình tìm thấy link nháp trên Zalo rồi! Cảm ơn cậu đã không bỏ rơi mình, lần sau mình hứa backup đầy đủ.",
            partnerMood: "relieved",
            nvcAnalysis: {
              observation: "Nhận diện thời gian còn lại (25 phút) và tài nguyên sẵn có (file nháp).",
              feeling: "Trấn an sự hoảng loạn, giữ nhịp thở bình tĩnh.",
              need: "Giải quyết sự cố khẩn cấp trước mắt.",
              request: "Phân công việc cấp tốc: bạn tìm file, mình tạo slide tóm tắt.",
            },
            coachingTip: "Khi khủng hoảng xảy ra, người có tư duy giải pháp sẽ tập trung vào 'Bước tiếp theo là gì?' thay vì 'Lỗi của ai?'.",
            nextTurnId: null,
          },
        ],
      },
    },
  },

  {
    id: "refuse-request",
    title: "Từ chối lời nhờ vả quá sức mà vẫn giữ trọn tình cảm & ranh giới",
    description: "Chị đồng nghiệp thân thiết nhờ bạn làm hộ bản báo cáo số liệu phân tích thị trường vào chiều thứ Sáu, trong khi bạn đang ngập đầu với 2 deadline gấp của sếp giao.",
    badge: "Thiết lập ranh giới cá nhân",
    partnerName: "Chị Mai Lan",
    partnerRole: "Đồng nghiệp tiền bối thân thiết",
    initialContext: "14h chiều thứ Sáu. Bàn làm việc của bạn đang ngổn ngang hồ sơ cần nộp trước 17h. Chị Mai Lan bước tới với vẻ mặt lo lắng và ái ngại.",
    startTurnId: "turn_1",
    turns: {
      turn_1: {
        id: "turn_1",
        partnerPrompt: "Em ơi, chị vừa nhận được điện thoại ở trường báo bé Bin bị sốt cao co giật phải nhập viện gấp. Chiều nay phòng mình phải nộp báo cáo thị trường tuần, em phụ trách làm nốt bảng số liệu giúp chị được không? Trong phòng em là người xử lý Excel nhanh nhất!",
        partnerContextHint: "Chị Mai Lan thực sự đang gặp chuyện khẩn cấp gia đình nhưng nhiệm vụ bạn đang gánh cũng cực kỳ rủi ro nếu trễ hạn.",
        options: [
          {
            id: "opt_ref_1a",
            userReplyText: "Dạ vâng... chị cứ về chăm bé đi ạ, để em làm cho. Không sao đâu chị.",
            styleLabel: "Thụ động / Né tránh",
            isOptimal: false,
            scoreRespect: 6,
            scoreEmpathy: 7,
            partnerResponse: "Ôi chị cảm ơn em nhiều lắm, em cứu chị bàn thua trông thấy này! Chị gửi file qua email em rồi nhé!",
            partnerMood: "relieved",
            nvcAnalysis: {
              observation: "Bạn nhận thêm việc mà không cân nhắc năng lực và giới hạn thời gian thực tế.",
              feeling: "Bề ngoài tỏ ra ổn nhưng bên trong bắt đầu hoảng loạn, ấm ức và căng thẳng tột độ.",
              need: "Bỏ rơi nhu cầu an toàn công việc và sức khỏe của chính mình vì sợ mang tiếng từ chối.",
              request: "Không có đề xuất giải pháp bền vững.",
            },
            coachingTip: "Nói 'Có' với người khác trong khi bản thân muốn nói 'Không' là tự phản bội chính mình. Kết quả bạn sẽ phải cày tới nửa đêm, có nguy cơ làm hỏng cả 2 việc quan trọng.",
            nextTurnId: "turn_2_overwhelmed",
          },
          {
            id: "opt_ref_1b",
            userReplyText: "Không được đâu chị ơi! Chiều nay em cũng ngập đầu 2 báo cáo sếp dí sát cổ đây, tự nhiên chị đẩy việc qua cho em sao em gánh nổi?",
            styleLabel: "Hung hăng / Công kích",
            isOptimal: false,
            scoreRespect: 2,
            scoreEmpathy: 1,
            partnerResponse: "Chị biết em bận chị mới phải cắn răng nhờ vì con chị đi viện cấp cứu, chứ em tưởng chị thích đùn đẩy việc à? Em làm gì mà nói chuyện cay nghiệt thế...",
            partnerMood: "defensive",
            nvcAnalysis: {
              observation: "Từ chối thô bạo mà hoàn toàn phớt lờ biến cố nguy hiểm của đối phương (bé sốt co giật).",
              feeling: "Phản ứng phòng vệ gắt gỏng.",
              need: "Bảo vệ ranh giới cá nhân nhưng bằng phương thức làm tổn thương tình người sâu sắc.",
              request: "Cắt đứt cuộc trò chuyện một cách thô lỗ.",
            },
            coachingTip: "Ranh giới không đồng nghĩa với sự vô cảm. Bạn có quyền từ chối công việc nhưng không nên chối bỏ sự đồng cảm với nỗi đau của người khác.",
            nextTurnId: "turn_2_hurt",
          },
          {
            id: "opt_ref_1c",
            userReplyText: "Trời ơi thương bé Bin quá, chị mau thu xếp về với cháu ngay đi ạ! Về phần báo cáo, chiều nay em đang phải nộp gấp 2 hợp đồng cho sếp trước 17h nên em không thể trực tiếp làm toàn bộ số liệu giúp chị được. Nhưng em có sẵn file template tự động tính đã cài công thức tuần trước, em gửi chị chuyển tiếp cho anh Hùng hoặc em báo với Trưởng phòng xin gia hạn báo cáo sang sáng thứ Hai vì lý do bất khả kháng của chị nhé?",
            styleLabel: "Khéo léo / Giữ ranh giới",
            isOptimal: true,
            scoreRespect: 10,
            scoreEmpathy: 10,
            partnerResponse: "Chị cảm ơn em rất nhiều vì đã thấu hiểu cho mẹ con chị! Ý kiến báo Trưởng phòng xin dời sang sáng thứ Hai quá hợp lý, chị nhắn sếp ngay trên đường đây. Cảm ơn em nhiều lắm!",
            partnerMood: "happy",
            nvcAnalysis: {
              observation: "Đồng cảm trọn vẹn với hoàn cảnh khẩn cấp của đồng nghiệp (bé Bin sốt).",
              feeling: "Chân thành quan tâm và giữ sự bình tĩnh.",
              need: "Ranh giới rõ ràng: thừa nhận trung thực mình cũng có deadline gấp của sếp.",
              request: "Đưa ra 2 giải pháp thay thế thông minh: chia sẻ template + hỗ trợ xin gia hạn chính thức.",
            },
            coachingTip: "Công thức từ chối đỉnh cao: 'Đồng cảm ấm áp' + 'Từ chối dứt khoát trung thực' + 'Cung cấp phương án thay thế khả thi'. Bạn bảo vệ được mình mà vẫn là chỗ dựa tinh thần cho người khác.",
            nextTurnId: null,
          },
        ],
      },
      turn_2_overwhelmed: {
        id: "turn_2_overwhelmed",
        partnerPrompt: "17h00 cùng ngày: Sếp đứng trước bàn bạn đòi 2 báo cáo hợp đồng, trong khi bảng số liệu của chị Lan bạn mới làm được 30%. Sếp tỏ ra rất bực mình.",
        partnerContextHint: "Bạn kiệt sức, hai mắt cay xè và bị sếp khiển trách vì trễ deadline quan trọng.",
        options: [
          {
            id: "opt_over_a",
            userReplyText: "Dạ em xin lỗi sếp, tại chị Lan nhờ em làm hộ báo cáo con chị ấy ốm nên em không kịp làm bài của em...",
            styleLabel: "Thụ động / Né tránh",
            isOptimal: false,
            scoreRespect: 4,
            scoreEmpathy: 3,
            partnerResponse: "Sếp lắc đầu: 'Tôi giao việc cho cậu chứ có giao cho Lan đâu? Cậu ôm rơm rặm bụng rồi đổ lỗi à? Tôi cần kết quả trước 18h!'",
            partnerMood: "defensive",
            nvcAnalysis: {
              observation: "Biến bản thân thành nạn nhân và vô tình đẩy đồng nghiệp vào thế khó với sếp.",
              feeling: "Hoảng loạn và né tránh trách nhiệm.",
              need: "Đánh mất uy tín nghề nghiệp.",
              request: "Không có đề xuất giải quyết rủi ro.",
            },
            coachingTip: "Khi bạn không dám từ chối, cái giá phải trả thường là sự sụp đổ uy tín cá nhân.",
            nextTurnId: null,
          },
          {
            id: "opt_over_b",
            userReplyText: "Dạ thưa sếp, em xin nhận trách nhiệm vì đã đánh giá chưa đúng khối lượng công việc chiều nay. Hợp đồng số 1 em đã hoàn thành 100% gửi sếp duyệt ngay bây giờ. Hợp đồng số 2 em xin phép tập trung làm liên tục và gửi sếp trước 18h15. Em cam kết đảm bảo số liệu chuẩn xác ạ.",
            styleLabel: "Khéo léo / Giữ ranh giới",
            isOptimal: true,
            scoreRespect: 9,
            scoreEmpathy: 7,
            partnerResponse: "Sếp gật đầu: 'Được, gửi ngay hợp đồng 1 qua cho tôi. Rút kinh nghiệm việc phân bổ thời gian nhé.'",
            partnerMood: "neutral",
            nvcAnalysis: {
              observation: "Chịu trách nhiệm 100% không viện cớ.",
              feeling: "Bình tĩnh, chuyên nghiệp.",
              need: "Khôi phục niềm tin của lãnh đạo.",
              request: "Đưa ra mốc giờ cụ thể có tính khả thi cao.",
            },
            coachingTip: "Nhận trách nhiệm và đưa ra giải pháp từng phần là cách nhanh nhất để chặn đứng cơn khủng hoảng.",
            nextTurnId: null,
          },
        ],
      },
      turn_2_hurt: {
        id: "turn_2_hurt",
        partnerPrompt: "Chị Lan rơm rớm nước mắt, quay lưng bước vội ra thang máy. Không khí trong phòng làm việc trở nên ngột ngạt kỳ lạ.",
        partnerContextHint: "Mối quan hệ đồng nghiệp thân thiết nhiều năm có nguy cơ rạn nứt hoàn toàn.",
        options: [
          {
            id: "opt_hurt_a",
            userReplyText: "Chị Lan ơi, em chạy theo chị ra thang máy đây. Em xin lỗi vì câu nói vừa rồi, em bị stress công việc nên lỡ lời cộc lốc với chị. Chị lo cho bé trước đi nhé, việc báo cáo em sẽ báo sếp xin lùi sang thứ Hai giúp chị, chị đừng lo!",
            styleLabel: "Giao tiếp Trắc ẩn (NVC)",
            isOptimal: true,
            scoreRespect: 9,
            scoreEmpathy: 10,
            partnerResponse: "Chị Lan xúc động: 'Cảm ơn em đã chạy theo nói với chị... Chị cũng hiểu em đang áp lực. Cảm ơn em nhiều!'",
            partnerMood: "relieved",
            nvcAnalysis: {
              observation: "Hành động ngay lập tức để cứu vãn mối quan hệ trước khi vết nứt quá sâu.",
              feeling: "Chân thành nhận lỗi lỡ lời.",
              need: "Tình đồng nghiệp và sự sẻ chia.",
              request: "Hỗ trợ xin lùi hạn.",
            },
            coachingTip: "Khi nhận ra mình đã làm tổn thương người khác bằng sự thô lỗ, thời điểm xin lỗi tốt nhất là NGAY LẬP TỨC.",
            nextTurnId: null,
          },
          {
            id: "opt_hurt_b",
            userReplyText: "Kệ, mình nói đúng mà. Thân đến mấy thì việc ai nấy làm chứ, hơi đâu mà bận tâm.",
            styleLabel: "Hung hăng / Công kích",
            isOptimal: false,
            scoreRespect: 2,
            scoreEmpathy: 1,
            partnerResponse: "Các đồng nghiệp khác nhìn bạn bằng ánh mắt ái ngại và giữ khoảng cách.",
            partnerMood: "angry",
            nvcAnalysis: {
              observation: "Tự đóng băng cảm xúc để bảo vệ sự tự ái.",
              feeling: "Bất cần nhưng thực chất bên trong cô đơn.",
              need: "Đánh mất kết nối con người.",
              request: "Cô lập bản thân.",
            },
            coachingTip: "Đúng lý nhưng sai tình sẽ biến nơi làm việc thành một nhà tù băng giá.",
            nextTurnId: null,
          },
        ],
      },
    },
  },

  {
    id: "handle-criticism",
    title: "Xử lý khi nhận lời chê bai gay gắt (Tách biệt cảm xúc tức thời)",
    description: "Trong buổi họp đánh giá dự án, một thành viên khác lớn tiếng chê bai ý tưởng của bạn là 'vô dụng, viển vông và làm mất thời gian cả phòng'.",
    badge: "Bản lĩnh cảm xúc & Trí tuệ EQ",
    partnerName: "Hoàng Quân",
    partnerRole: "Thành viên nhóm phản biện",
    initialContext: "Phòng họp 10 người. Hoàng Quân vừa đứng bật dậy, đập bàn và chỉ trích slide đề xuất của bạn.",
    startTurnId: "turn_1",
    turns: {
      turn_1: {
        id: "turn_1",
        partnerPrompt: "Cái kế hoạch truyền thông của bạn vẽ ra toàn bánh vẽ, hoàn toàn vô dụng và viển vông! Triển khai cả tháng trời chẳng mang lại tí chuyển đổi nào, chỉ tổ làm mất thời gian của cả phòng!",
        partnerContextHint: "Hoàng Quân đang công kích trực diện trước mặt mọi người, cố tình dùng từ ngữ nặng nề.",
        options: [
          {
            id: "opt_crit_1a",
            userReplyText: "Bạn nói ai vô dụng? Bạn thì làm được trò trống gì cho dự án mà to mồm? Có giỏi thì nhảy vào mà làm xem có hơn ai không!",
            styleLabel: "Hung hăng / Công kích",
            isOptimal: false,
            scoreRespect: 2,
            scoreEmpathy: 1,
            partnerResponse: "Quân cười khẩy: 'Mọi người thấy chưa, đuối lý rồi quay sang cắn càn cá nhân! Thái độ thế này bảo sao dự án thất bại!'",
            partnerMood: "angry",
            nvcAnalysis: {
              observation: "Tấn công cá nhân đáp trả thay vì bàn về dữ liệu công việc.",
              feeling: "Cơn thịnh nộ bùng phát vì cái tôi bị tổn thương.",
              need: "Thắng thua trong tranh cãi thay vì tìm giải pháp tối ưu cho dự án.",
              request: "Thách thức tiêu cực.",
            },
            coachingTip: "Khi bị công kích, nếu bạn đáp trả bằng sự tức giận, bạn đã trao quyền kiểm soát cảm xúc của mình vào tay đối phương.",
            nextTurnId: "turn_2_escalate",
          },
          {
            id: "opt_crit_1b",
            userReplyText: "Dạ... em xin lỗi mọi người, chắc do em năng lực còn yếu kém nên làm ảnh hưởng dự án ạ...",
            styleLabel: "Thụ động / Né tránh",
            isOptimal: false,
            scoreRespect: 4,
            scoreEmpathy: 3,
            partnerResponse: "Quân đắc thắng: 'Biết mình kém thì lần sau đừng có đứng lên trình bày làm mất thì giờ của người khác!'",
            partnerMood: "defensive",
            nvcAnalysis: {
              observation: "Tự dán nhãn bản thân là 'yếu kém' dù chưa phân tích dữ liệu thực tế.",
              feeling: "Xấu hổ, co cụm và tự ti.",
              need: "Né tránh xung đột bằng cách hạ thấp giá trị bản thân.",
              request: "Đầu hàng vô điều kiện.",
            },
            coachingTip: "Nhận hết lỗi về mình khi chưa có số liệu khách quan sẽ giết chết sự tự tin và khiến người khác coi thường năng lực của bạn.",
            nextTurnId: null,
          },
          {
            id: "opt_crit_1c",
            userReplyText: "Mình ghi nhận là kết quả chuyển đổi tháng vừa qua chưa đạt được kỳ vọng của Quân và cả phòng. Quân có thể chỉ rõ cụ thể chỉ số nào hoặc kênh nào đang kém hiệu quả nhất theo phân tích của bạn không? Chúng ta cùng nhìn vào số liệu thực tế để xem cần cắt giảm kênh nào và tối ưu điểm chạm nào cho đợt tới.",
            styleLabel: "Giao tiếp Trắc ẩn (NVC)",
            isOptimal: true,
            scoreRespect: 10,
            scoreEmpathy: 9,
            partnerResponse: "Quân hơi khựng lại vì thấy bạn không nổi nóng, hạ giọng xuống: 'Ờ thì... tỷ lệ click trên kênh TikTok rất thấp, chỉ đạt 0.8% trong khi chi phí chạy video khá cao...'",
            partnerMood: "neutral",
            nvcAnalysis: {
              observation: "Thừa nhận sự thật khách quan: Kết quả chưa đạt kỳ vọng (kỹ thuật Fogging).",
              feeling: "Giữ tâm thế điềm tĩnh, tách biệt giá trị bản thân khỏi lời chê bai.",
              need: "Hướng cuộc họp về mục tiêu chung: Tối ưu hiệu quả dự án dựa trên dữ liệu.",
              request: "Yêu cầu dẫn chứng số liệu cụ thể và mời gọi hợp tác giải quyết.",
            },
            coachingTip: "Kỹ thuật 'Tách quả bóng khỏi cầu thủ': Coi lời phê bình như một quả bóng dữ liệu. Đừng để nó đập vào ngực bạn, hãy chuyền nó xuống bàn để cả hai cùng nhìn vào mổ xẻ.",
            nextTurnId: "turn_2_deescalated",
          },
        ],
      },
      turn_2_deescalated: {
        id: "turn_2_deescalated",
        partnerPrompt: "Quân ngồi xuống và mở máy tính: 'Nếu điều chỉnh lại, mình nghĩ nên dồn 70% ngân sách TikTok sang cho kênh Email Marketing và SEO bài viết chuyên sâu.'",
        partnerContextHint: "Đối phương đã chuyển hoàn toàn từ trạng thái công kích sang trạng thái đóng góp chuyên môn.",
        options: [
          {
            id: "opt_deesc_a",
            userReplyText: "Góc nhìn này của Quân rất sắc sảo và sát thực tế. Số liệu tuần rồi cũng cho thấy tỷ lệ chuyển đổi từ Email cao gấp 3 lần. Mình thống nhất đề xuất này và sẽ cập nhật lại bảng phân bổ ngân sách gửi cả phòng trong chiều nay nhé.",
            styleLabel: "Giao tiếp Trắc ẩn (NVC)",
            isOptimal: true,
            scoreRespect: 10,
            scoreEmpathy: 10,
            partnerResponse: "Cả phòng thở phào nhẹ nhõm, Trưởng phòng gật đầu tán thưởng bản lĩnh xử lý tình huống xuất sắc của bạn.",
            partnerMood: "happy",
            nvcAnalysis: {
              observation: "Tập trung vào giải pháp tối ưu và công nhận đóng góp của đối phương.",
              feeling: "Cởi mở, cầu thị và hào hiệp.",
              need: "Thành công chung của tập thể.",
              request: "Cam kết cập nhật kế hoạch rõ ràng.",
            },
            coachingTip: "Biến một người đang đối đầu thành người đồng hành là đỉnh cao của trí tuệ cảm xúc trong công việc.",
            nextTurnId: null,
          },
        ],
      },
      turn_2_escalate: {
        id: "turn_2_escalate",
        partnerPrompt: "Trưởng phòng phải đập bàn can thiệp: 'Hai bạn dừng lại ngay! Đây là phòng họp công ty chứ không phải cái chợ!'",
        partnerContextHint: "Cả hai đều bị đánh giá xấu trong biên bản cuộc họp.",
        options: [
          {
            id: "opt_esc_a",
            userReplyText: "Em xin lỗi sếp và mọi người vì đã để cảm xúc cá nhân chi phối. Em xin phép lắng nghe tiếp ý kiến xây dựng từ các anh chị ạ.",
            styleLabel: "Giao tiếp Trắc ẩn (NVC)",
            isOptimal: true,
            scoreRespect: 8,
            scoreEmpathy: 7,
            partnerResponse: "Không khí dịu bớt, cuộc họp được tiếp tục với sự điều phối của Trưởng phòng.",
            partnerMood: "neutral",
            nvcAnalysis: {
              observation: "Kịp thời phanh lại trước khi sự việc đi quá xa.",
              feeling: "Bình tĩnh thừa nhận sai lầm.",
              need: "Giữ gìn trật tự và không gian chuyên nghiệp.",
              request: "Tập trung vào phần thảo luận chung.",
            },
            coachingTip: "Dừng lại kịp thời luôn tốt hơn là cố cãi đến cùng.",
            nextTurnId: null,
          },
        ],
      },
    },
  },

  {
    id: "listen-stressed-person",
    title: "Lắng nghe người thân đang stress (Đồng cảm thay vì vội giảng giải)",
    description: "Người bạn đời / em gái của bạn đi làm về, mắt đỏ hoe, buông túi xách xuống sofa và thở dài: 'Em mệt mỏi quá, hôm nay ở công ty sếp cứ soi mói từng tí, em thấy mình thật vô dụng...'",
    badge: "Lắng nghe thấu cảm & Nâng đỡ tinh thần",
    partnerName: "Hà My",
    partnerRole: "Người thân yêu đang kiệt sức",
    initialContext: "20h tối. Căn phòng ấm áp. Hà My vừa về đến nhà sau 11 tiếng làm việc căng thẳng, vẻ mặt chực khóc.",
    startTurnId: "turn_1",
    turns: {
      turn_1: {
        id: "turn_1",
        partnerPrompt: "Hôm nay em mệt mỏi đến mức không thở nổi nữa... Sếp cứ soi từng dấu chấm phẩy trong báo cáo của em, đồng nghiệp thì chẳng ai thèm phụ một tay. Em thấy mình bất tài và vô tích sự quá...",
        partnerContextHint: "Đối phương đang trải qua cảm xúc kiệt quệ và cảm giác bị cô lập. Não bộ đang ở trạng thái tổn thương sâu sắc.",
        options: [
          {
            id: "opt_stress_1a",
            userReplyText: "Có gì đâu mà phải nghiêm trọng hóa lên thế em? Đi làm ai chẳng có lúc bị sếp mắng. Em phải suy nghĩ tích cực lên, rèn luyện tinh thần thép vào chứ hở tí là khóc lóc thế sao làm lớn được!",
            styleLabel: "Phán xét / Áp đặt",
            isOptimal: false,
            scoreRespect: 3,
            scoreEmpathy: 1,
            partnerResponse: "Hà My gạt nước mắt, đứng bật dậy: 'Anh/chị lúc nào cũng chỉ biết giảng đạo lý! Em không cần ai dạy đời em hết!' rồi đóng sầm cửa phòng ngủ.",
            partnerMood: "angry",
            nvcAnalysis: {
              observation: "Gạt bỏ và xem nhẹ nỗi đau của người khác ('Có gì đâu', 'Đi làm ai chẳng...').",
              feeling: "Thiếu kiên nhẫn, ban phát sự tích cực độc hại (Toxic Positivity).",
              need: "Muốn đối phương hết buồn nhanh để bản thân đỡ khó chịu, thay vì thực sự quan tâm đến họ.",
              request: "Đưa ra lời khuyên sáo rỗng vô tác dụng.",
            },
            coachingTip: "Khi ai đó đang chìm trong đau khổ, điều họ cần là 'Được cảm thấy an toàn và được lắng nghe', chứ không phải một bài học đạo đức hay lời khuyên tích cực gượng gạo.",
            nextTurnId: null,
          },
          {
            id: "opt_stress_1b",
            userReplyText: "Công ty độc hại thế thì nghỉ việc quách đi em! Mai nộp đơn luôn, anh/chị xin cho em sang chỗ khác làm chứ việc gì phải khổ thế!",
            styleLabel: "Khéo léo / Giữ ranh giới",
            isOptimal: false,
            scoreRespect: 6,
            scoreEmpathy: 5,
            partnerResponse: "Hà My lắc đầu thở dài: 'Nói thì dễ lắm... Giờ nghỉ việc lấy tiền đâu trả tiền nhà? Em chỉ muốn kể cho nhẹ lòng thôi mà...'",
            partnerMood: "defensive",
            nvcAnalysis: {
              observation: "Vội vàng đưa ra giải pháp thực tế khi đối phương chưa sẵn sàng tiếp nhận.",
              feeling: "Sốt sắng muốn giải quyết vấn đề hộ người khác.",
              need: "Nhầm lẫn giữa việc 'nghe giãi bày cảm xúc' với 'giải quyết bài toán logic'.",
              request: "Đưa ra hành động cực đoan (nghỉ việc ngay).",
            },
            coachingTip: "Đừng vội sửa chữa (Fixing mode) khi cảm xúc chưa được chữa lành (Feeling mode). Trước khi bật đèn giải pháp, hãy ôm lấy cảm xúc của họ trước.",
            nextTurnId: null,
          },
          {
            id: "opt_stress_1c",
            userReplyText: "Hôm nay chắc hẳn là một ngày rất cô đơn và kiệt sức với em... Nghe em kể thôi mà anh/chị cũng thấy xót xa. Lại đây ngồi uống cốc nước ấm này với anh/chị nhé. Anh/chị ở đây với em, em cứ nói ra hết cho nhẹ lòng, không sao cả.",
            styleLabel: "Giao tiếp Trắc ẩn (NVC)",
            isOptimal: true,
            scoreRespect: 10,
            scoreEmpathy: 10,
            partnerResponse: "Hà My ôm lấy bạn òa khóc nức nở như một đứa trẻ. Sau vài phút giải tỏa hết cơn nghẹn ngào, cô ấy ngước nhìn bạn với ánh mắt nhẹ nhõm: 'Cảm ơn vì đã luôn ở bên em... Giờ em thấy đỡ hơn nhiều rồi.'",
            partnerMood: "happy",
            nvcAnalysis: {
              observation: "Công nhận và gọi tên chính xác cảm xúc của đối phương: 'cô đơn và kiệt sức'.",
              feeling: "Tình yêu thương, sự dịu dàng và lòng trắc ẩn trọn vẹn.",
              need: "Tạo không gian an toàn vô điều kiện (Holding Space) để đối phương xả van áp lực.",
              request: "Đề nghị chăm sóc nhỏ nhẹ: uống cốc nước ấm, ngồi bên nhau.",
            },
            coachingTip: "Sự hiện diện không phán xét là món quà chữa lành quý giá nhất bạn có thể trao tặng cho một tâm hồn đang mệt mỏi.",
            nextTurnId: null,
          },
        ],
      },
    },
  },
];

/* =========================================================================
   2. TYPES & DATA: CHẾ ĐỘ 2 - NHẬN DIỆN BẪY TÂM LÝ (COGNITIVE BIAS DETECTOR)
   ========================================================================= */

interface BiasQuestion {
  id: string;
  scenarioTitle: string;
  scenarioText: string;
  realWorldContext: string;
  correctBiasKey: string;
  options: {
    key: string;
    label: string;
    explanation: string;
  }[];
  neuroscienceInsight: string;
  actionableHack: string;
  flashcardFront: string;
  flashcardBack: string;
}

const COGNITIVE_BIASES_LIST: BiasQuestion[] = [
  {
    id: "bias_confirmation",
    scenarioTitle: "Chỉ tìm kiếm bài viết khen ngợi chế độ ăn Keto",
    scenarioText: "Hoàng quyết định thử chế độ ăn Keto để giảm cân. Cậu lên Google và chỉ bấm đọc những bài viết có tiêu đề 'Keto giúp giảm mỡ thần kỳ' hoặc 'Keto đảo ngược lão hóa'. Khi bạn gửi cho cậu một bài nghiên cứu y khoa về nguy cơ tăng mỡ máu và sỏi thận do ăn quá nhiều chất béo bão hòa, Hoàng gạt phắt đi và bảo 'Nghiên cứu này rác rưởi, tài trợ bởi các hãng thực phẩm!'",
    realWorldContext: "Đời sống sức khỏe & thói quen tiếp nhận thông tin",
    correctBiasKey: "confirmation_bias",
    options: [
      {
        key: "confirmation_bias",
        label: "Thiên Kiến Xác Nhận (Confirmation Bias)",
        explanation: "Chính xác! Bộ não có xu hướng bẩm sinh là chỉ tìm kiếm, ghi nhớ và đánh giá cao những thông tin củng cố cho niềm tin có sẵn của mình, đồng thời phớt lờ hoặc thù địch với bằng chứng trái chiều.",
      },
      {
        key: "sunk_cost",
        label: "Ngụy Biện Chi Phí Chìm (Sunk Cost Fallacy)",
        explanation: "Chưa đúng. Chi phí chìm liên quan đến việc tiếc tiền bạc/công sức đã bỏ ra trong quá khứ nên tiếp tục đâm lao, không phải là việc chọn lọc thông tin ủng hộ niềm tin.",
      },
      {
        key: "anchoring",
        label: "Hiệu Ứng Mỏ Neo (Anchoring Effect)",
        explanation: "Chưa đúng. Mỏ neo là việc bị chi phối quá mức bởi con số hoặc mẩu thông tin đầu tiên được tiếp cận.",
      },
      {
        key: "dunning_kruger",
        label: "Hiệu Ứng Dunning-Kruger",
        explanation: "Chưa đúng. Dunning-Kruger là sự tự tin ảo tưởng của người mới bắt đầu, khác với việc chủ động chọn lọc thông tin.",
      },
    ],
    neuroscienceInsight: "Bộ não tiêu tốn rất nhiều năng lượng (glucose) khi phải xử lý sự bất đồng nhận thức (Cognitive Dissonance). Để tiết kiệm năng lượng sinh tồn, não kích hoạt vùng hạch hạnh nhân (Amygdala) coi thông tin trái chiều như một mối đe dọa thể xác, và tiết ra Dopamine khi đọc được thông tin vuốt ve niềm tin hiện tại.",
    actionableHack: "Mẹo 'Luật sư của Ác quỷ' (Devil's Advocate): Mỗi khi bạn rất tin vào một quan điểm, hãy tự ép mình tìm kiếm trên Google từ khóa ngược lại: 'Tại sao [quan điểm đó] lại sai?' và đọc ít nhất 2 bài phân tích phản biện có số liệu.",
    flashcardFront: "Thiên kiến xác nhận (Confirmation Bias) là gì và làm sao để phòng tránh?",
    flashcardBack: "Là xu hướng chỉ tìm kiếm & tin thông tin củng cố niềm tin có sẵn và phớt lờ bằng chứng trái chiều. Cách tránh: Áp dụng 'Luật sư của Ác quỷ', chủ động tìm kiếm lý do tại sao niềm tin của mình có thể sai.",
  },
  {
    id: "bias_sunk_cost",
    scenarioTitle: "Cố học tiếp ngành không hợp vì tiếc 3 năm đại học",
    scenarioText: "Hương nhận ra mình hoàn toàn không có đam mê và năng khiếu với ngành Kế toán từ cuối năm thứ 2. Sang năm thứ 3, mỗi ngày đến trường với cô là một cực hình gây trầm cảm. Tuy nhiên, khi bố mẹ khuyên Hương có thể chuyển sang học Thiết kế đồ họa mà cô rất thích, Hương khóc và bảo: 'Con đã lỡ học 3 năm tốn mấy trăm triệu tiền học phí rồi, giờ bỏ thì phí hoài công sức bao năm qua, con phải ráng lấy cái bằng này!'",
    realWorldContext: "Quyết định sự nghiệp & cuộc đời",
    correctBiasKey: "sunk_cost",
    options: [
      {
        key: "sunk_cost",
        label: "Ngụy Biện Chi Phí Chìm (Sunk Cost Fallacy)",
        explanation: "Chính xác! Hương đang để thời gian và tiền bạc đã mất trong quá khứ (vốn không thể lấy lại được) định đoạt tương lai, khiến cô tiếp tục lãng phí thêm nhiều năm tháng hạnh phúc phía trước.",
      },
      {
        key: "confirmation_bias",
        label: "Thiên Kiến Xác Nhận (Confirmation Bias)",
        explanation: "Chưa đúng. Đây không phải là việc tìm kiếm thông tin củng cố niềm tin.",
      },
      {
        key: "halo_effect",
        label: "Hiệu Ứng Vầng Hào Quang (Halo Effect)",
        explanation: "Chưa đúng. Halo Effect là việc đánh giá tốt toàn diện một người chỉ vì họ có một đặc điểm nổi bật.",
      },
      {
        key: "bandwagon",
        label: "Tâm Lý Bầy Đàn (Bandwagon Effect)",
        explanation: "Chưa đúng. Đây là cuộc chiến nội tâm về sự tiếc nuối chi phí đã bỏ ra chứ không phải chạy theo đám đông.",
      },
    ],
    neuroscienceInsight: "Não bộ con người tiến hóa với cơ chế 'Nỗi sợ mất mát' (Loss Aversion) mạnh gấp 2-2.5 lần 'Niềm vui nhận được' (theo Daniel Kahneman). Việc thừa nhận một quyết định trong quá khứ là sai lầm kích hoạt vùng não đau đớn (Insula), khiến chúng ta thà tiếp tục chịu đựng đau khổ kéo dài còn hơn là cắt lỗ ngay lập tức.",
    actionableHack: "Mẹo 'Số Không Thuần Khiết' (Clean Slate Test): Hãy tự hỏi: 'Nếu ngay sáng nay thức dậy, tôi chưa từng học 3 năm này và tài khoản vẫn nguyên vẹn, liệu tôi có chọn đăng ký học tiếp ngành này trong năm thứ 4 không?' Nếu câu trả lời là 'KHÔNG', hãy dũng cảm chuyển hướng.",
    flashcardFront: "Ngụy biện chi phí chìm (Sunk Cost Fallacy) là gì?",
    flashcardBack: "Là cái bẫy tiếp tục đổ thêm thời gian, tiền bạc vào một việc vô ích chỉ vì tiếc những gì đã bỏ ra trong quá khứ. Nhớ rằng: Chi phí chìm là tiền đã mất, tương lai mới là thứ bạn còn quyết định được.",
  },
  {
    id: "bias_anchoring",
    scenarioTitle: "Chiếc áo 2.500.000đ gạch giá còn 990.000đ",
    scenarioText: "Bước vào cửa hàng quần áo, Minh thấy một chiếc áo khoác treo biển: 'Giá gốc niêm yết: 2.500.000đ — Giảm sốc Black Friday: chỉ còn 990.000đ!'. Minh lập tức cảm thấy đây là một món hời thế kỷ và mua ngay mà không cần đắn đo, dù chất vải và đường may chỉ tương đương những chiếc áo giá 400.000đ ở chợ.",
    realWorldContext: "Tiêu dùng & đàm phán tài chính",
    correctBiasKey: "anchoring",
    options: [
      {
        key: "anchoring",
        label: "Hiệu Ứng Mỏ Neo (Anchoring Effect)",
        explanation: "Chính xác! Con số 2.500.000đ đóng vai trò là chiếc 'mỏ neo' trong tâm trí Minh, làm sai lệch thước đo giá trị thực của chiếc áo, khiến mức giá 990.000đ trở nên quá rẻ một cách tương đối.",
      },
      {
        key: "dunning_kruger",
        label: "Hiệu Ứng Dunning-Kruger",
        explanation: "Chưa đúng. Đây là hiện tượng tâm lý định giá tiêu dùng, không phải đánh giá năng lực.",
      },
      {
        key: "transparency",
        label: "Ảo Tưởng Trong Suốt (Illusion of Transparency)",
        explanation: "Chưa đúng. Ảo tưởng trong suốt liên quan đến cảm xúc giao tiếp xã hội.",
      },
      {
        key: "system1_system2",
        label: "Thiên Kiến Hệ Thống 1",
        explanation: "Mặc dù Hệ thống 1 có tham gia, nhưng bẫy tâm lý cụ thể và chính xác nhất ở đây là Hiệu Ứng Mỏ Neo.",
      },
    ],
    neuroscienceInsight: "Bộ não rất lười tính toán giá trị tuyệt đối vì việc đó đòi hỏi phân tích chi phí vải, nhân công, chuỗi cung ứng. Não chọn lối tắt so sánh tương đối: lấy con số đầu tiên xuất hiện làm mốc chuẩn (mỏ neo) và so sánh mọi thứ xoay quanh mốc đó.",
    actionableHack: "Mẹo 'Nhổ Mỏ Neo': Che con số giá gốc đi, nhìn thẳng vào sản phẩm và tự hỏi: 'Nếu món này để trần không có nhãn mác, mình sẵn sàng rút bao nhiêu tiền trong ví ra để mua nó?'",
    flashcardFront: "Hiệu ứng mỏ neo (Anchoring Effect) trong định giá hoạt động thế nào?",
    flashcardBack: "Bộ não bị con số đầu tiên nhìn thấy ghim chặt làm thước đo chuẩn mực. Người bán tạo giá gốc cao ngất ngưởng để giá sau giảm trông như một món hời dù giá trị thực thấp hơn nhiều.",
  },
  {
    id: "bias_dunning_kruger",
    scenarioTitle: "Vừa đọc 3 bài viết chứng khoán đã mang tiền đi 'All-in'",
    scenarioText: "Sau khi xem 3 video TikTok và đọc 2 cuốn sách tóm tắt về đầu tư cổ phiếu, Nam cảm thấy thị trường tài chính cực kỳ dễ kiếm tiền. Cậu tự tin khoe với bạn bè: 'Mấy chuyên gia phân tích toàn làm phức tạp hóa vấn đề, chỉ cần canh theo mô hình nến đảo chiều là ăn chắc 30%/tháng!' và đem toàn bộ tiền tiết kiệm cắm vào các mã đầu cơ rủi ro cao.",
    realWorldContext: "Đầu tư tài chính & năng lực cá nhân",
    correctBiasKey: "dunning_kruger",
    options: [
      {
        key: "dunning_kruger",
        label: "Hiệu Ứng Dunning-Kruger",
        explanation: "Chính xác! Nam đang đứng trên đỉnh cao của 'Đỉnh cao ngu dốt' (Mount Stupid) — nơi kiến thức ít ỏi nhưng sự tự tin lại đạt đỉnh vì chưa biết thế giới kiến thức thực tế bao la và phức tạp đến nhường nào.",
      },
      {
        key: "confirmation_bias",
        label: "Thiên Kiến Xác Nhận",
        explanation: "Chưa đúng. Điểm cốt lõi ở đây là sự tương quan lệch lạc giữa kiến thức nông cạn và sự tự tin thái quá.",
      },
      {
        key: "bandwagon",
        label: "Tâm Lý Bầy Đàn",
        explanation: "Chưa đúng. Nam hành động dựa trên ảo tưởng năng lực cá nhân chứ không phải do bị áp lực đám đông ép buộc.",
      },
      {
        key: "sunk_cost",
        label: "Chi Phí Chìm",
        explanation: "Chưa đúng.",
      },
    ],
    neuroscienceInsight: "Để nhận biết được mình kém cỏi trong một lĩnh vực, một người cần phải có đủ kiến thức chuyên môn về lĩnh vực đó. Khi chưa có chuyên môn, họ thiếu luôn cả 'siêu nhận thức' (Metacognition) để tự đánh giá năng lực của chính mình.",
    actionableHack: "Mẹo 'Quy tắc 100 giờ': Trước khi tuyên bố mình hiểu sâu một môn nào đó, hãy dành ít nhất 100 giờ học tập nghiêm túc và trao đổi với một chuyên gia có 10 năm kinh nghiệm để thấy được những 'điểm mù' kiến thức của mình.",
    flashcardFront: "Đường cong Dunning-Kruger phản ánh điều gì?",
    flashcardBack: "Người mới biết chút ít thường cực kỳ tự tin (Đỉnh cao ngu dốt). Khi học sâu hơn, họ rơi vào 'Thung lũng thất vọng' vì nhận ra mình biết quá ít. Chỉ qua rèn luyện lâu dài, sự tự tin mới tăng dần cùng năng lực thực tế.",
  },
  {
    id: "bias_bandwagon",
    scenarioTitle: "Xếp hàng 2 tiếng mua trà sữa 'hot trend' dù không khát",
    scenarioText: "Thấy mạng xã hội rần rần chia sẻ món 'Trà sữa đất nung', trước cửa quán hàng dài người đứng chờ dưới nắng 2 tiếng, Linh cũng nằng nặc rủ nhóm bạn vào xếp hàng cùng. Linh nghĩ thầm: 'Đông người xếp hàng thế này chắc chắn phải ngon tuyệt đỉnh!' nhưng khi uống thì vị rất bình thường và ngọt gắt.",
    realWorldContext: "Tâm lý xã hội & Trào lưu đám đông",
    correctBiasKey: "bandwagon",
    options: [
      {
        key: "bandwagon",
        label: "Tâm Lý Bầy Đàn & Bằng Chứng Xã Hội (Bandwagon / Social Proof)",
        explanation: "Chính xác! Linh mặc định rằng hành động của số đông là bằng chứng cho chất lượng và tính đúng đắn, đồng thời bị thôi thúc bởi nỗi sợ bỏ lỡ (FOMO).",
      },
      {
        key: "anchoring",
        label: "Hiệu Ứng Mỏ Neo",
        explanation: "Chưa đúng. Đây là sự a dua theo hành vi tập thể.",
      },
      {
        key: "halo_effect",
        label: "Hiệu Ứng Hào Quang",
        explanation: "Chưa đúng.",
      },
      {
        key: "confirmation_bias",
        label: "Thiên Kiến Xác Nhận",
        explanation: "Chưa đúng.",
      },
    ],
    neuroscienceInsight: "Trong thời tiền sử, bị tách khỏi bộ lạc đồng nghĩa với cái chết trước thú dữ. Não bộ hình thành phản xạ sinh tồn ăn sâu: 'Làm theo bầy đàn là an toàn nhất'. Vùng não tưởng thưởng sẽ giải phóng cảm giác an tâm khi chúng ta hòa mình vào đám đông.",
    actionableHack: "Mẹo 'Tạm dừng 24 giờ': Trước bất kỳ xu hướng tiêu dùng nào đang sốt trên mạng, hãy đợi ít nhất 24-48 giờ trước khi chi tiền hoặc tham gia. 90% sự hào hứng sẽ tự biến mất khi cơn sốt ảo qua đi.",
    flashcardFront: "Hiệu ứng bầy đàn (Bandwagon Effect) là gì?",
    flashcardBack: "Xu hướng làm theo một hành động hoặc tin theo một trào lưu chỉ vì có nhiều người khác đang làm như vậy. Cách khắc phục: Tạm dừng 24 giờ trước khi mua sắm theo trend.",
  },
  {
    id: "bias_system1_2",
    scenarioTitle: "Phản xạ bốc hỏa gõ phím đáp trả ngay comment khiêu khích",
    scenarioText: "Đang lướt Facebook, thấy một tài khoản lạ để lại bình luận chê bai quê hương mình với lời lẽ xúc phạm, Tuấn lập tức tim đập nhanh, máu dồn lên não và gõ ngay một tràng chửi bới tục tĩu dài 10 dòng gửi đi chỉ trong 15 giây. Sau khi gửi, Tuấn bị bạn bè phản ứng và rơi vào cuộc cãi vã kéo dài thâu đêm đầy độc hại.",
    realWorldContext: "Kiểm soát cảm xúc & Giao tiếp trên không gian số",
    correctBiasKey: "system1_system2",
    options: [
      {
        key: "system1_system2",
        label: "Tư Duy Nhanh (Hệ Thống 1) Áp Đảo Tư Duy Chậm (Hệ Thống 2)",
        explanation: "Chính xác! Hệ thống 1 (trực giác, cảm xúc bốc đồng, tự động, nhanh) đã chiếm quyền điều khiển trước khi Hệ thống 2 (lý tính, cân nhắc hậu quả, logic) kịp thức tỉnh để can thiệp.",
      },
      {
        key: "sunk_cost",
        label: "Ngụy Biện Chi Phí Chìm",
        explanation: "Chưa đúng.",
      },
      {
        key: "dunning_kruger",
        label: "Hiệu Ứng Dunning-Kruger",
        explanation: "Chưa đúng.",
      },
      {
        key: "anchoring",
        label: "Hiệu Ứng Mỏ Neo",
        explanation: "Chưa đúng.",
      },
    ],
    neuroscienceInsight: "Hệ thống 1 nằm ở các cấu trúc não cổ (Hạch hạnh nhân, Não bò sát) phản ứng trong vòng 50 mili-giây. Trong khi đó, Hệ thống 2 nằm ở Vỏ não trước trán (Prefrontal Cortex) cần từ 3 đến 5 giây để kích hoạt và tiêu tốn năng lượng suy nghĩ cao hơn.",
    actionableHack: "Mẹo 'Quy tắc 10 giây & 3 nhịp thở sâu': Khi cảm thấy nhịp tim tăng vọt vì một tin nhắn khiêu khích, hãy đặt điện thoại úp xuống bàn, hít sâu 3 nhịp chậm rãi trong 10 giây. Hãy cho vỏ não trước trán thời gian tỉnh dậy để cứu bạn khỏi rắc rối.",
    flashcardFront: "Sự khác biệt giữa Hệ thống 1 và Hệ thống 2 (Daniel Kahneman)?",
    flashcardBack: "Hệ thống 1: Nhanh, tự động, cảm tính, ít tốn năng lượng (dễ bốc đồng). Hệ thống 2: Chậm, lý tính, đòi hỏi nỗ lực tập trung. Bí quyết tự chủ: Trì hoãn 10 giây để Hệ thống 2 kịp kích hoạt.",
  },
  {
    id: "bias_halo",
    scenarioTitle: "Tin tưởng mù quáng một đối tác chỉ vì ngoại hình bảnh bao",
    scenarioText: "Trong buổi giao lưu khởi nghiệp, Long gặp một đối tác ăn mặc vest hàng hiệu sang trọng, nụ cười tỏa sáng, đồng hồ Rolex và phong thái tự tin như một diễn viên điện ảnh. Dù đối tác không cung cấp được báo cáo tài chính kiểm toán và hồ sơ pháp lý rõ ràng, Long vẫn vội vàng ký hợp đồng góp vốn 500 triệu vì nghĩ rằng: 'Người đàng hoàng, lịch thiệp thế này chắc chắn là doanh nhân chân chính!'",
    realWorldContext: "Đánh giá con người & Hợp tác kinh doanh",
    correctBiasKey: "halo_effect",
    options: [
      {
        key: "halo_effect",
        label: "Hiệu Ứng Vầng Hào Quang (Halo Effect)",
        explanation: "Chính xác! Một đặc điểm nổi bật bên ngoài (ngoại hình đẹp, đồ hiệu sang trọng) đã tạo nên một 'vầng hào quang' bao phủ, khiến Long suy diễn tích cực sang toàn bộ nhân cách và độ tin cậy trong kinh doanh của người đó.",
      },
      {
        key: "bandwagon",
        label: "Tâm Lý Bầy Đàn",
        explanation: "Chưa đúng.",
      },
      {
        key: "confirmation_bias",
        label: "Thiên Kiến Xác Nhận",
        explanation: "Chưa đúng.",
      },
      {
        key: "transparency",
        label: "Ảo Tưởng Trong Suốt",
        explanation: "Chưa đúng.",
      },
    ],
    neuroscienceInsight: "Bộ não con người có xu hướng 'tiết kiệm năng lượng nhận thức' bằng cách giả định rằng những người hấp dẫn về mặt thị giác cũng sở hữu các phẩm chất đạo đức tốt (Đẹp = Tốt / 'What is beautiful is good' stereotype).",
    actionableHack: "Mẹo 'Thẩm định hồ sơ mù' (Blind Review): Trong các quyết định tiền bạc hoặc tuyển dụng, hãy đánh giá con người thuần túy dựa trên các con số số liệu, bằng chứng pháp lý và kết quả xác minh độc lập, gạt bỏ hoàn toàn yếu tố ngoại hình và tài ăn nói.",
    flashcardFront: "Hiệu ứng hào quang (Halo Effect) là gì?",
    flashcardBack: "Xu hướng để một ấn tượng tích cực ban đầu (như vẻ ngoài ưa nhìn, ăn mặc sang trọng) làm lu mờ khả năng đánh giá khách quan về năng lực hay đạo đức thật sự của một người.",
  },
  {
    id: "bias_transparency",
    scenarioTitle: "Nghĩ rằng cả hội trường đều nhìn thấu sự run rẩy của mình",
    scenarioText: "Bước lên bục thuyết trình trước 50 người, Mai cảm thấy tim mình đập thình thịch và lòng bàn tay hơi đổ mồ hôi. Cô đinh ninh rằng tất cả mọi người bên dưới đang nhìn thấu sự sợ hãi của mình, thấy rõ từng nhịp run trong giọng nói của cô, khiến cô càng lúc càng hoảng loạn và quên sạch bài nói.",
    realWorldContext: "Thuyết trình trước đám đông & Lo âu xã hội",
    correctBiasKey: "transparency",
    options: [
      {
        key: "transparency",
        label: "Ảo Tưởng Trong Suốt (Illusion of Transparency)",
        explanation: "Chính xác! Mai đang lầm tưởng rằng trạng thái tâm lý và cảm xúc bên trong của mình hoàn toàn lộ rõ ra ngoài cho người khác nhìn thấy (như thể cô ấy là người bằng kính trong suốt).",
      },
      {
        key: "dunning_kruger",
        label: "Hiệu Ứng Dunning-Kruger",
        explanation: "Chưa đúng.",
      },
      {
        key: "anchoring",
        label: "Hiệu Ứng Mỏ Neo",
        explanation: "Chưa đúng.",
      },
      {
        key: "sunk_cost",
        label: "Ngụy Biện Chi Phí Chìm",
        explanation: "Chưa đúng.",
      },
    ],
    neuroscienceInsight: "Chúng ta trải nghiệm cảm xúc của chính mình một cách mãnh liệt từ bên trong, nên não bộ tự động chiếu cảm giác cực độ đó ra thế giới xung quanh. Thực tế các nghiên cứu tâm lý chứng minh người nghe chỉ nhận biết được chưa tới 15% sự lo âu mà người nói đang cảm nhận.",
    actionableHack: "Mẹo 'Chiếc áo giáp vô hình': Hãy tự nhắc nhở bản thân trước khi lên sân khấu: 'Khán giả không phải là máy quét X-quang. Họ không thể thấy nhịp tim hay sự lo lắng của mình trừ khi mình tự nói ra.'",
    flashcardFront: "Ảo tưởng trong suốt (Illusion of Transparency) là gì?",
    flashcardBack: "Ảo tưởng rằng cảm xúc bên trong của chúng ta (lo âu, run rẩy, bối rối) hoàn toàn lộ rõ ra cho người khác thấy. Thực tế: Người ngoài hầu như không nhận ra sự bồn chồn của bạn.",
  },
];

/* =========================================================================
   3. TYPES & DATA: CHẾ ĐỘ 3 - GỠ RỐI SUY NGHĨ CBT (INTERACTIVE REFLECTION CARD)
   ========================================================================= */

interface CbtPreset {
  title: string;
  badge: string;
  fact: string;
  autoThought: string;
  stressLevel: number;
  emotions: string[];
  evidenceFor: string;
  evidenceAgainst: string;
  friendAdvice: string;
  microAction: string;
}

const CBT_PRESETS: CbtPreset[] = [
  {
    title: "Bị sếp phê bình slide báo cáo chưa đạt chuẩn",
    badge: "Công việc / Áp lực",
    fact: "Trong cuộc họp 10h sáng, sếp nhận xét slide báo cáo của mình còn thiếu phân tích đối thủ cạnh tranh và yêu cầu làm lại trước 15h.",
    autoThought: "Sếp ghét mình rồi. Mình là kẻ kém cỏi vô tích sự, sớm muộn gì cũng bị sa thải.",
    stressLevel: 8,
    emotions: ["Lo âu", "Xấu hổ", "Bất an", "Tự trách"],
    evidenceFor: "Sếp nói giọng khá nghiêm nghị trước mặt cả phòng.",
    evidenceAgainst: "Tuần trước sếp vừa khen đề xuất ngân sách của mình tốt. Sếp chỉ góp ý đúng vào nội dung slide thiếu sót chứ không hề chỉ trích nhân cách hay đe dọa sa thải. Dự án này sếp đang chịu sức ép lớn từ Ban Giám đốc nên sếp cần số liệu chuẩn.",
    friendAdvice: "Sếp chỉ đang cần slide hoàn thiện hơn để bảo vệ dự án thôi cậu ơi. Đây là chuyện công việc bình thường, không định nghĩa con người cậu. Cậu làm việc rất chăm chỉ và có năng lực mà, chỉnh lại một chút là xuất sắc ngay!",
    microAction: "Mở máy tính, gõ khung 3 ý phân tích đối thủ trong 5 phút và đi pha một cốc trà hoa cúc uống cho dịu thần kinh.",
  },
  {
    title: "Gửi tin nhắn cho người yêu / bạn thân 2 tiếng chưa thấy trả lời",
    badge: "Mối quan hệ / Tình cảm",
    fact: "Mình gửi tin nhắn rủ ăn tối lúc 17h, hiện tại đã 19h15, đối phương đã đọc (seen) nhưng chưa nhắn lại.",
    autoThought: "Họ chán mình rồi, họ đang giấu giếm điều gì đó hoặc mình đã làm điều gì sai khiến họ lạnh nhạt.",
    stressLevel: 7,
    emotions: ["Bồn chồn", "Tủi thân", "Bất an"],
    evidenceFor: "Đã hiện chữ 'Đã xem' mà chưa gõ chữ nào.",
    evidenceAgainst: "Hôm nay là cuối tuần, đối phương từng kể phải tăng ca đột xuất hoặc đang trên đường lái xe về nhà giờ cao điểm kẹt xe. Bình thường họ luôn tôn trọng và quan tâm mình. Việc chưa nhắn lại 2 tiếng không thể chứng minh họ hết yêu thương mình.",
    friendAdvice: "Người ta bận việc xíu thôi mà, đừng để cái đầu vẽ kịch bản phim truyền hình bi kịch nữa! Ai cũng có những lúc mở tin nhắn ra xem rồi vướng việc gấp chưa kịp gõ trả lời được.",
    microAction: "Để điện thoại sang phòng khác, bật bài nhạc yêu thích và đi tắm nước ấm thư giãn trong 15 phút.",
  },
  {
    title: "Điểm bài thi thử thấp hơn kỳ vọng",
    badge: "Học tập / Áp lực thành tích",
    fact: "Bài kiểm tra trắc nghiệm thử nghiệm đạt 6.5/10, thấp hơn mục tiêu 8.5 mình đặt ra.",
    autoThought: "Bộ não mình quá chậm chạp, học bao nhiêu cũng đổ sông đổ biển, mình sẽ trượt kỳ thi thật mất.",
    stressLevel: 9,
    emotions: ["Thất vọng", "Bất lực", "Sợ hãi"],
    evidenceFor: "Điểm số 6.5 là thật, mình làm sai 7 câu.",
    evidenceAgainst: "Đây là bài THI THỬ, mục đích của thi thử là phát hiện lỗ hổng kiến thức trước khi thi thật. Trong 7 câu sai, có 4 câu do đọc ẩu đề bài chứ không phải do không hiểu lý thuyết. Mình còn 3 tuần nữa để ôn luyện.",
    friendAdvice: "Sai ở bài thi thử là điều may mắn nhất đấy! Thà sai bây giờ để sửa còn hơn là sai trong phòng thi thật. Cậu biết chính xác mình yếu phần nào rồi, chỉ cần tập trung vá lỗ hổng đó là điểm sẽ nhảy vọt thôi!",
    microAction: "Lấy bút đỏ khoanh lại 4 câu sai do đọc ẩu và viết lại tóm tắt công thức cho 3 câu lý thuyết trong 5 phút.",
  },
];

/* =========================================================================
   MAIN COMPONENT: PsychologyLifeSkillsLab
   ========================================================================= */


// =============================================================================
// CÔNG CỤ 5: PHÒNG THÍ NGHIỆM NHẬN THỨC & TÂM LÝ HỌC THỰC NGHIỆM
// (Thí nghiệm Stroop, Đo lường Working Memory Span & Nhật ký Tự phản chiếu CBT ABCDE)
// =============================================================================

export interface CognitiveLabsShowcaseProps {
  courseTitle: string;
  lessonTitle: string;
  lessonMarkdown?: string;
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
  initialSubLab?: CognitiveSubLab | null;
}

type CognitiveSubLab = "stroop" | "memory-span" | "cbt-journal";

interface StroopTrial {
  word: string;
  wordMeaning: string;
  colorName: "Đỏ" | "Xanh dương" | "Xanh lá" | "Vàng";
  colorClass: string;
  isCongruent: boolean;
}

const STROOP_TRIALS: StroopTrial[] = [
  { word: "ĐỎ", wordMeaning: "Đỏ", colorName: "Đỏ", colorClass: "text-red-600 dark:text-red-400 border-red-500", isCongruent: true },
  { word: "XANH DƯƠNG", wordMeaning: "Xanh dương", colorName: "Đỏ", colorClass: "text-red-600 dark:text-red-400 border-red-500", isCongruent: false },
  { word: "XANH LÁ", wordMeaning: "Xanh lá", colorName: "Xanh lá", colorClass: "text-emerald-600 dark:text-emerald-400 border-emerald-500", isCongruent: true },
  { word: "VÀNG", wordMeaning: "Vàng", colorName: "Xanh dương", colorClass: "text-blue-600 dark:text-blue-400 border-blue-500", isCongruent: false },
  { word: "ĐỎ", wordMeaning: "Đỏ", colorName: "Vàng", colorClass: "text-amber-500 dark:text-amber-300 border-amber-400", isCongruent: false },
  { word: "VÀNG", wordMeaning: "Vàng", colorName: "Vàng", colorClass: "text-amber-500 dark:text-amber-300 border-amber-400", isCongruent: true },
  { word: "XANH LÁ", wordMeaning: "Xanh lá", colorName: "Đỏ", colorClass: "text-red-600 dark:text-red-400 border-red-500", isCongruent: false },
  { word: "XANH DƯƠNG", wordMeaning: "Xanh dương", colorName: "Xanh dương", colorClass: "text-blue-600 dark:text-blue-400 border-blue-500", isCongruent: true },
  { word: "ĐỎ", wordMeaning: "Đỏ", colorName: "Xanh lá", colorClass: "text-emerald-600 dark:text-emerald-400 border-emerald-500", isCongruent: false },
  { word: "XANH LÁ", wordMeaning: "Xanh lá", colorName: "Vàng", colorClass: "text-amber-500 dark:text-amber-300 border-amber-400", isCongruent: false },
];

export function CognitiveLabsShowcase({
  courseTitle,
  lessonTitle,
  lessonMarkdown,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
  initialSubLab,
}: CognitiveLabsShowcaseProps) {
  const [activeSubLab, setActiveSubLab] = useState<CognitiveSubLab>(initialSubLab ?? "stroop");
  useEffect(() => {
    if (initialSubLab) {
      setActiveSubLab(initialSubLab);
    }
  }, [initialSubLab]);
  const [feedback, setFeedback] = useState<string | null>(null);

  // ----------------------------------------------------
  // SUB-LAB 1: STROOP TEST
  // ----------------------------------------------------
  const [stroopTrialIdx, setStroopTrialIdx] = useState<number>(0);
  const [stroopIsRunning, setStroopIsRunning] = useState<boolean>(false);
  const [stroopStartTime, setStroopStartTime] = useState<number>(0);
  const [stroopRecords, setStroopRecords] = useState<Array<{
    trial: number;
    isCongruent: boolean;
    isCorrect: boolean;
    reactionTimeMs: number;
  }>>([]);
  const [stroopCompleted, setStroopCompleted] = useState<boolean>(false);

  const startStroopTest = () => {
    setStroopTrialIdx(0);
    setStroopRecords([]);
    setStroopCompleted(false);
    setStroopIsRunning(true);
    setStroopStartTime(performance.now());
  };

  const handleStroopAnswer = (chosenColor: "Đỏ" | "Xanh dương" | "Xanh lá" | "Vàng") => {
    if (!stroopIsRunning || stroopCompleted) return;
    const now = performance.now();
    const rt = Math.round(now - stroopStartTime);
    const currentTrial = STROOP_TRIALS[stroopTrialIdx];
    const isCorrect = chosenColor === currentTrial.colorName;

    const newRecord = {
      trial: stroopTrialIdx + 1,
      isCongruent: currentTrial.isCongruent,
      isCorrect,
      reactionTimeMs: rt,
    };

    const updatedRecords = [...stroopRecords, newRecord];
    setStroopRecords(updatedRecords);

    if (stroopTrialIdx + 1 < STROOP_TRIALS.length) {
      setStroopTrialIdx(prev => prev + 1);
      setStroopStartTime(performance.now());
    } else {
      setStroopIsRunning(false);
      setStroopCompleted(true);

      const correctCount = updatedRecords.filter(r => r.isCorrect).length;
      const score = Math.round((correctCount / updatedRecords.length) * 100);

      const conRecs = updatedRecords.filter(r => r.isCongruent && r.isCorrect);
      const inconRecs = updatedRecords.filter(r => !r.isCongruent && r.isCorrect);
      const avgCon = conRecs.length > 0 ? Math.round(conRecs.reduce((a, b) => a + b.reactionTimeMs, 0) / conRecs.length) : 0;
      const avgIncon = inconRecs.length > 0 ? Math.round(inconRecs.reduce((a, b) => a + b.reactionTimeMs, 0) / inconRecs.length) : 0;
      const interferenceMs = avgIncon - avgCon;

      if (onRecordAttempt) {
        onRecordAttempt({
          mode: "stroop_test",
          score,
          maxScore: 100,
          isPassed: score >= 70,
          activityTitle: `${lessonTitle || courseTitle} · Thí nghiệm Stroop`,
          activityKey: "psych-stroop-test",
          details: {
            correctCount,
            totalTrials: updatedRecords.length,
            avgCongruentMs: avgCon,
            avgIncongruentMs: avgIncon,
            interferenceMs,
          }
        });
      }
    }
  };

  const stroopStats = useMemo(() => {
    if (stroopRecords.length === 0) return null;
    const correctCount = stroopRecords.filter(r => r.isCorrect).length;
    const conRecs = stroopRecords.filter(r => r.isCongruent && r.isCorrect);
    const inconRecs = stroopRecords.filter(r => !r.isCongruent && r.isCorrect);
    const avgCon = conRecs.length > 0 ? Math.round(conRecs.reduce((a, b) => a + b.reactionTimeMs, 0) / conRecs.length) : 0;
    const avgIncon = inconRecs.length > 0 ? Math.round(inconRecs.reduce((a, b) => a + b.reactionTimeMs, 0) / inconRecs.length) : 0;
    const interferenceMs = avgIncon - avgCon;
    return {
      accuracy: Math.round((correctCount / stroopRecords.length) * 100),
      avgCon,
      avgIncon,
      interferenceMs,
    };
  }, [stroopRecords]);

  // ----------------------------------------------------
  // SUB-LAB 2: MEMORY SPAN & CHUNKING
  // ----------------------------------------------------
  const [spanLevel, setSpanLevel] = useState<number>(4);
  const [currentSequence, setCurrentSequence] = useState<string>("");
  const [showSequence, setShowSequence] = useState<boolean>(false);
  const [userRecallInput, setUserRecallInput] = useState<string>("");
  const [spanResult, setSpanResult] = useState<{ passed: boolean; expected: string; actual: string } | null>(null);
  const [maxSpanAchieved, setMaxSpanAchieved] = useState<number>(0);
  const [isSpanRunning, setIsSpanRunning] = useState<boolean>(false);
  const [chunkingMode, setChunkingMode] = useState<"raw" | "chunked">("raw");

  const startMemoryTest = (level: number, mode: "raw" | "chunked") => {
    setSpanLevel(level);
    setChunkingMode(mode);
    setSpanResult(null);
    setUserRecallInput("");
    setIsSpanRunning(true);

    // Sinh dãy số ngẫu nhiên
    let digits = "";
    for (let i = 0; i < level; i++) {
      digits += Math.floor(Math.random() * 10).toString();
    }
    setCurrentSequence(digits);
    setShowSequence(true);

    // Ẩn số sau thời gian tỉ lệ với độ dài (ví dụ: 1500ms + level * 300ms)
    setTimeout(() => {
      setShowSequence(false);
    }, 1500 + level * 300);
  };

  const handleVerifyRecall = () => {
    const cleanActual = userRecallInput.replace(/\D/g, "");
    const cleanExpected = currentSequence;
    const isOk = cleanActual === cleanExpected;

    setSpanResult({
      passed: isOk,
      expected: cleanExpected,
      actual: cleanActual,
    });
    setIsSpanRunning(false);

    if (isOk && spanLevel > maxSpanAchieved) {
      setMaxSpanAchieved(spanLevel);
    }

    if (onRecordAttempt) {
      const score = isOk ? Math.min(100, Math.round((spanLevel / 8) * 100)) : 40;
      onRecordAttempt({
        mode: "memory_span",
        score,
        maxScore: 100,
        isPassed: isOk && spanLevel >= 5,
        activityTitle: `${lessonTitle || courseTitle} · Memory Span (Level ${spanLevel})`,
        activityKey: `psych-memory-span-${spanLevel}`,
        details: {
          level: spanLevel,
          mode: chunkingMode,
          passed: isOk,
          maxSpan: Math.max(maxSpanAchieved, isOk ? spanLevel : 0),
        }
      });
    }
  };

  // ----------------------------------------------------
  // SUB-LAB 3: GUIDED CBT REFLECTION JOURNAL (MÔ HÌNH ABCDE)
  // ----------------------------------------------------
  const [cbtA, setCbtA] = useState<string>("Nhận được email phản hồi từ khách hàng / cấp trên chỉ ra 3 lỗi trong báo cáo.");
  const [cbtB, setCbtB] = useState<string>("'Mình làm ăn thật tệ hại, chắc chắn họ nghĩ mình vô dụng và mình sắp bị sa thải.' (Bẫy tư duy: Trầm trọng hóa & Đọc suy nghĩ)");
  const [cbtC, setCbtC] = useState<string>("Tim đập nhanh, lo âu 8/10, muốn trì hoãn né tránh mở tiếp email để sửa.");
  const [cbtD, setCbtD] = useState<string>("Bằng chứng thực tế: Họ đã khen phần phân tích dữ liệu tuần trước. 3 lỗi này là lỗi định dạng có thể sửa trong 15 phút. Không có ai đề cập đến sa thải cả.");
  const [cbtE, setCbtE] = useState<string>("'Sai sót là cơ hội hoàn thiện quy trình kiểm tra chéo (checklist). Mình sẽ dành 15 phút sửa ngay và cảm ơn họ đã góp ý kịp thời.' Cảm xúc: Tự tin và bình tĩnh hơn (Lo âu giảm xuống 2/10).");

  const handleSaveCbtJournal = async () => {
    const markdown = `### 🧠 Nhật Ký Phản Chiếu Nhận Thức CBT (Mô hình ABCDE)
- **Bài học:** ${lessonTitle} (${courseTitle})
- **A (Sự kiện kích hoạt):** ${cbtA}
- **B (Niềm tin / Suy nghĩ tự động):** ${cbtB}
- **C (Hệ quả cảm xúc & phản ứng):** ${cbtC}
- **D (Tranh biện & Thách thức):** ${cbtD}
- **E (Niềm tin mới cân bằng):** ${cbtE}`;

    await onSaveToNotebook(markdown);
    setFeedback("Đã lưu Nhật ký CBT vào Sổ tay!");
    setTimeout(() => setFeedback(null), 3000);

    if (onRecordAttempt) {
      onRecordAttempt({
        mode: "reflection_journal",
        score: 100,
        maxScore: 100,
        isPassed: true,
        activityTitle: `${lessonTitle || courseTitle} · Nhật ký Tự phản chiếu CBT ABCDE`,
        activityKey: "psych-cbt-abcde-journal",
        details: {
          hasDisputation: cbtD.length > 20,
          hasEffectiveBelief: cbtE.length > 20,
        }
      });
    }
  };

  const handleSaveFlashcard = async (title: string, concept: string, takeaway: string) => {
    if (!onCreateFlashcardQuick) return;
    const front = `Tâm lý học Nhận thức: ${title}`;
    const back = `**Khái niệm:**\n${concept}\n\n**Ứng dụng thực tiễn:**\n${takeaway}`;
    await onCreateFlashcardQuick(front, back);
    setFeedback("Đã lưu nguyên lý nhận thức vào Flashcard FSRS!");
    setTimeout(() => setFeedback(null), 3000);
  };

  return (
    <div data-testid="cognitive-labs-container" className="space-y-6">
      {/* HEADER & SUB-TABS */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-4 rounded-xl bg-purple-50/50 dark:bg-purple-950/20 border border-purple-200 dark:border-purple-800">
        <div>
          <h2 className="text-sm sm:text-base font-bold flex items-center gap-2 text-purple-950 dark:text-purple-200">
            <Brain className="w-5 h-5 text-purple-600 dark:text-purple-400" />
            Phòng Thí Nghiệm Nhận Thức & Khoa Học Não Bộ (Cognitive Labs)
          </h2>
          <p className="text-xs text-purple-700 dark:text-purple-300 mt-0.5">
            Trực tiếp trải nghiệm các hiện tượng tâm lý học thực nghiệm và rèn luyện năng lực phản chiếu bản thân.
          </p>
        </div>

        <div className="flex items-center flex-wrap gap-1.5 p-1 rounded-xl bg-white dark:bg-zinc-900 border border-purple-200 dark:border-purple-800">
          <button
            type="button"
            data-testid="subtab-stroop"
            onClick={() => setActiveSubLab("stroop")}
            className={`min-h-[38px] px-3 py-1.5 rounded-lg text-xs font-semibold transition cursor-pointer flex items-center gap-1.5 ${
              activeSubLab === "stroop"
                ? "bg-purple-600 text-white shadow-sm"
                : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-100"
            }`}
          >
            <Timer className="w-3.5 h-3.5" />
            1. Thí Nghiệm Stroop
          </button>
          <button
            type="button"
            data-testid="subtab-memory-span"
            onClick={() => setActiveSubLab("memory-span")}
            className={`min-h-[38px] px-3 py-1.5 rounded-lg text-xs font-semibold transition cursor-pointer flex items-center gap-1.5 ${
              activeSubLab === "memory-span"
                ? "bg-purple-600 text-white shadow-sm"
                : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-100"
            }`}
          >
            <Activity className="w-3.5 h-3.5" />
            2. Trí Nhớ & Chunking
          </button>
          <button
            type="button"
            data-testid="subtab-cbt-journal"
            onClick={() => setActiveSubLab("cbt-journal")}
            className={`min-h-[38px] px-3 py-1.5 rounded-lg text-xs font-semibold transition cursor-pointer flex items-center gap-1.5 ${
              activeSubLab === "cbt-journal"
                ? "bg-purple-600 text-white shadow-sm"
                : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-100"
            }`}
          >
            <BookOpen className="w-3.5 h-3.5" />
            3. Nhật Ký CBT (ABCDE)
          </button>
        </div>
      </div>

      {/* FEEDBACK TOAST */}
      {feedback && (
        <div className="p-3 rounded-lg bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-300 dark:border-emerald-800 text-emerald-800 dark:text-emerald-300 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4" />
          <span>{feedback}</span>
        </div>
      )}

      {/* SUB-LAB 1: STROOP TEST */}
      {activeSubLab === "stroop" && (
        <div data-testid="stroop-test-container" className="space-y-6">
          <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                <Target className="w-4 h-4 text-purple-600" />
                Mục tiêu: Đo lường sự xung đột giữa Hệ thống 1 (Tự động đọc chữ) và Hệ thống 2 (Kiểm soát ức chế chọn màu mực)
              </span>
              <span className="text-[11px] font-mono text-zinc-500">
                10 Lượt kiểm tra chuẩn hóa
              </span>
            </div>
            <p className="text-xs text-zinc-600 dark:text-zinc-400 leading-relaxed">
              <strong>Quy tắc quan trọng:</strong> Hãy bấm nút chọn <strong>MÀU MỰC CỦA CHỮ</strong> nhanh nhất có thể, bất kể từ đó đang viết gì! Ví dụ: Từ viết là &ldquo;XANH&rdquo; nhưng màu mực là <strong>ĐỎ</strong> thì bạn phải chọn <strong>ĐỎ</strong>.
            </p>
          </div>

          {!stroopIsRunning && !stroopCompleted && (
            <div className="text-center py-12 rounded-2xl border border-dashed border-zinc-300 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-900 space-y-4">
              <h3 className="text-base font-bold text-zinc-800 dark:text-zinc-200">
                Sẵn sàng thử thách phản xạ não bộ?
              </h3>
              <p className="text-xs text-zinc-500 max-w-md mx-auto">
                Hệ thống đo thời gian phản hồi chính xác đến mili-giây (ms) bằng đồng hồ hiệu năng của trình duyệt.
              </p>
              <button
                type="button"
                data-testid="stroop-start-btn"
                onClick={startStroopTest}
                className="px-6 py-2.5 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold transition cursor-pointer shadow-sm inline-flex items-center gap-2"
              >
                <Play className="w-4 h-4 fill-white" />
                Bắt đầu bài kiểm tra (10 lượt)
              </button>
            </div>
          )}

          {stroopIsRunning && (
            <div className="rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 p-6 space-y-6 text-center">
              <div className="flex items-center justify-between text-xs text-zinc-500 border-b border-zinc-200 dark:border-zinc-800 pb-3">
                <span>Lượt: <strong className="text-purple-600 text-sm">{stroopTrialIdx + 1}</strong> / {STROOP_TRIALS.length}</span>
                <span>Chọn màu mực nhanh nhất có thể!</span>
              </div>

              {/* TARGET WORD DISPLAY */}
              <div className="py-8">
                <div
                  data-testid="stroop-target-word"
                  className={`text-4xl sm:text-5xl font-black tracking-wider transition-all select-none ${STROOP_TRIALS[stroopTrialIdx].colorClass}`}
                >
                  {STROOP_TRIALS[stroopTrialIdx].word}
                </div>
              </div>

              {/* 4 ANSWER BUTTONS */}
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 max-w-lg mx-auto">
                <button
                  type="button"
                  data-testid="stroop-btn-red"
                  onClick={() => handleStroopAnswer("Đỏ")}
                  className="min-h-[48px] px-4 py-2.5 rounded-xl border border-red-300 dark:border-red-800 bg-red-50 dark:bg-red-950/40 text-red-700 dark:text-red-300 font-bold text-xs hover:bg-red-100 dark:hover:bg-red-900/60 transition cursor-pointer"
                >
                  🔴 ĐỎ
                </button>
                <button
                  type="button"
                  data-testid="stroop-btn-blue"
                  onClick={() => handleStroopAnswer("Xanh dương")}
                  className="min-h-[48px] px-4 py-2.5 rounded-xl border border-blue-300 dark:border-blue-800 bg-blue-50 dark:bg-blue-950/40 text-blue-700 dark:text-blue-300 font-bold text-xs hover:bg-blue-100 dark:hover:bg-blue-900/60 transition cursor-pointer"
                >
                  🔵 XANH DƯƠNG
                </button>
                <button
                  type="button"
                  data-testid="stroop-btn-green"
                  onClick={() => handleStroopAnswer("Xanh lá")}
                  className="min-h-[48px] px-4 py-2.5 rounded-xl border border-emerald-300 dark:border-emerald-800 bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300 font-bold text-xs hover:bg-emerald-100 dark:hover:bg-emerald-900/60 transition cursor-pointer"
                >
                  🟢 XANH LÁ
                </button>
                <button
                  type="button"
                  data-testid="stroop-btn-yellow"
                  onClick={() => handleStroopAnswer("Vàng")}
                  className="min-h-[48px] px-4 py-2.5 rounded-xl border border-amber-300 dark:border-amber-800 bg-amber-50 dark:bg-amber-950/40 text-amber-700 dark:text-amber-300 font-bold text-xs hover:bg-amber-100 dark:hover:bg-amber-900/60 transition cursor-pointer"
                >
                  🟡 VÀNG
                </button>
              </div>
            </div>
          )}

          {stroopCompleted && stroopStats && (
            <div data-testid="stroop-results-card" className="p-6 rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 space-y-6">
              <div className="flex items-center justify-between border-b border-zinc-200 dark:border-zinc-800 pb-4">
                <div className="flex items-center gap-2">
                  <CheckCircle2 className="w-5 h-5 text-emerald-600" />
                  <h3 className="text-base font-bold text-zinc-900 dark:text-zinc-100">
                    Báo Cáo Phân Tích Hiệu Ứng Stroop
                  </h3>
                </div>
                <button
                  type="button"
                  onClick={startStroopTest}
                  className="text-xs text-purple-600 dark:text-purple-400 hover:underline flex items-center gap-1 cursor-pointer"
                >
                  <RotateCcw className="w-3.5 h-3.5" />
                  Làm lại bài test
                </button>
              </div>

              {/* STATS TILES */}
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                <div className="p-3.5 rounded-xl bg-purple-50 dark:bg-purple-950/30 border border-purple-200 dark:border-purple-800 text-center">
                  <span className="text-[11px] text-zinc-500 block">Độ chính xác</span>
                  <span className="text-xl font-black text-purple-700 dark:text-purple-300">{stroopStats.accuracy}%</span>
                </div>
                <div className="p-3.5 rounded-xl bg-emerald-50 dark:bg-emerald-950/30 border border-emerald-200 dark:border-emerald-800 text-center">
                  <span className="text-[11px] text-zinc-500 block">Trùng khớp (Congruent)</span>
                  <span className="text-xl font-black text-emerald-700 dark:text-emerald-300">{stroopStats.avgCon} ms</span>
                </div>
                <div className="p-3.5 rounded-xl bg-amber-50 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-800 text-center">
                  <span className="text-[11px] text-zinc-500 block">Bất đồng (Incongruent)</span>
                  <span className="text-xl font-black text-amber-700 dark:text-amber-300">{stroopStats.avgIncon} ms</span>
                </div>
                <div className="p-3.5 rounded-xl bg-blue-50 dark:bg-blue-950/30 border border-blue-200 dark:border-blue-800 text-center">
                  <span className="text-[11px] text-zinc-500 block">Hiệu ứng Nhiễu (Interference)</span>
                  <span className="text-xl font-black text-blue-700 dark:text-blue-300">+{stroopStats.interferenceMs} ms</span>
                </div>
              </div>

              {/* SCIENTIFIC EXPLANATION */}
              <div className="p-4 rounded-xl bg-zinc-50 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 text-xs space-y-2 leading-relaxed">
                <h4 className="font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-1.5">
                  <Sparkles className="w-4 h-4 text-purple-600" />
                  Ý nghĩa khoa học của chỉ số Nhiễu +{stroopStats.interferenceMs}ms:
                </h4>
                <p className="text-zinc-600 dark:text-zinc-400">
                  Khi từ và màu mực bất đồng, Hệ thống 1 (Automatic Reading) tự động giải mã ngữ nghĩa từ trong 150-200ms đầu tiên. Vỏ não tiền trán (Dorsolateral Prefrontal Cortex) của bạn phải kích hoạt cơ chế ức chế phản xạ (Inhibitory Control) để chọn đúng màu mực, khiến thời gian phản ứng bị chậm lại trung bình {stroopStats.interferenceMs}ms.
                </p>
              </div>

              {/* ACTIONS */}
              <div className="flex flex-wrap items-center justify-between gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => handleSaveFlashcard(
                    "Hiệu ứng Stroop (Stroop Interference Effect)",
                    "Hiện tượng não bộ mất nhiều thời gian hơn để xử lý khi màu sắc thực tế của chữ xung đột với ý nghĩa chữ viết.",
                    "Minh họa sự tranh chấp giữa Hệ thống 1 (Tự động, nhanh, bản năng) và Hệ thống 2 (Ý thức, kiểm soát, chậm)."
                  )}
                  className="px-3.5 py-1.5 rounded-lg border border-purple-300 dark:border-purple-700 bg-purple-50 dark:bg-purple-950/50 hover:bg-purple-100 text-purple-800 dark:text-purple-300 text-xs font-semibold inline-flex items-center gap-1.5 transition cursor-pointer"
                >
                  <Sparkles className="w-3.5 h-3.5" />
                  Lưu Khái Niệm Vào Flashcard FSRS
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* SUB-LAB 2: MEMORY SPAN & CHUNKING */}
      {activeSubLab === "memory-span" && (
        <div data-testid="memory-span-container" className="space-y-6">
          <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                <Activity className="w-4 h-4 text-purple-600" />
                Định luật Miller (7 ± 2) & Kỹ thuật Phân nhóm (Chunking)
              </span>
              <span className="text-[11px] font-mono text-zinc-500">
                Kỷ lục cao nhất: {maxSpanAchieved} chữ số
              </span>
            </div>
            <p className="text-xs text-zinc-600 dark:text-zinc-400 leading-relaxed">
              Trí nhớ làm việc (Working Memory) của con người có giới hạn vật lý khoảng 4 đến 7 đơn vị thông tin rời rạc. Tuy nhiên, bằng cách <strong>Chunking (nhóm thông tin)</strong>, chúng ta có thể ghi nhớ chuỗi 10-12 số một cách dễ dàng.
            </p>
          </div>

          {/* LEVEL SELECTOR */}
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-xs font-semibold text-zinc-700 dark:text-zinc-300">Chọn độ dài chuỗi:</span>
            {[4, 5, 6, 7, 8].map((lvl) => (
              <button
                key={lvl}
                type="button"
                data-testid={`span-lvl-btn-${lvl}`}
                disabled={isSpanRunning}
                onClick={() => startMemoryTest(lvl, chunkingMode)}
                className={`px-3 py-1.5 rounded-lg text-xs font-bold border transition cursor-pointer ${
                  spanLevel === lvl
                    ? "bg-purple-600 border-purple-600 text-white"
                    : "bg-white dark:bg-zinc-900 border-zinc-300 dark:border-zinc-700 text-zinc-700 dark:text-zinc-300 hover:border-purple-400"
                }`}
              >
                Level {lvl} ({lvl} số)
              </button>
            ))}

            <div className="ml-auto flex items-center gap-2">
              <span className="text-xs text-zinc-500">Chế độ hiển thị:</span>
              <button
                type="button"
                onClick={() => setChunkingMode(chunkingMode === "raw" ? "chunked" : "raw")}
                className="px-2.5 py-1 rounded border border-zinc-300 dark:border-zinc-700 text-xs font-mono text-zinc-700 dark:text-zinc-300 cursor-pointer"
              >
                {chunkingMode === "raw" ? "Số rời rạc (0 9 1 2)" : "Phân nhóm (0912-345)"}
              </button>
            </div>
          </div>

          {/* ACTIVE TRIAL DISPLAY */}
          <div className="p-8 rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 text-center space-y-4">
            {showSequence ? (
              <div className="space-y-2">
                <span className="text-xs text-zinc-400 uppercase tracking-widest">Hãy tập trung ghi nhớ chuỗi số:</span>
                <div data-testid="memory-sequence-display" className="text-3xl sm:text-4xl font-mono font-black text-purple-600 dark:text-purple-400 tracking-widest">
                  {chunkingMode === "chunked" && currentSequence.length >= 6
                    ? `${currentSequence.slice(0, 3)}-${currentSequence.slice(3, 6)}${currentSequence.slice(6) ? `-${currentSequence.slice(6)}` : ""}`
                    : currentSequence.split("").join(" ")}
                </div>
              </div>
            ) : isSpanRunning ? (
              <div className="space-y-3 max-w-sm mx-auto">
                <span className="text-xs font-semibold text-zinc-700 dark:text-zinc-300 block">
                  Nhập lại chính xác chuỗi số vừa xuất hiện:
                </span>
                <input
                  type="text"
                  data-testid="memory-recall-input"
                  value={userRecallInput}
                  onChange={(e) => setUserRecallInput(e.target.value)}
                  placeholder="Nhập chuỗi số..."
                  className="w-full text-center text-xl font-mono font-bold p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-900 focus:outline-none focus:border-purple-500"
                  autoFocus
                />
                <button
                  type="button"
                  data-testid="memory-verify-btn"
                  onClick={handleVerifyRecall}
                  className="w-full py-2.5 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold transition cursor-pointer"
                >
                  Xác Nhận Kết Quả
                </button>
              </div>
            ) : spanResult ? (
              <div className="space-y-3">
                <div className={`inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-bold ${
                  spanResult.passed
                    ? "bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-300"
                    : "bg-red-100 text-red-800 dark:bg-red-950 dark:text-red-300"
                }`}>
                  {spanResult.passed ? <CheckCircle2 className="w-4 h-4" /> : <AlertCircle className="w-4 h-4" />}
                  {spanResult.passed ? `Chính xác! Bạn đã ghi nhớ trọn vẹn ${spanLevel} chữ số.` : `Chưa chính xác ở chuỗi ${spanLevel} số.`}
                </div>
                <div className="text-xs font-mono text-zinc-500">
                  Chuỗi gốc: <span className="font-bold text-zinc-800 dark:text-zinc-200">{spanResult.expected}</span> | Bạn nhập: <span className="font-bold text-zinc-800 dark:text-zinc-200">{spanResult.actual || "(trống)"}</span>
                </div>
                <button
                  type="button"
                  onClick={() => startMemoryTest(spanLevel, chunkingMode)}
                  className="px-4 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 text-xs font-medium text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition cursor-pointer"
                >
                  Thử lại chuỗi khác
                </button>
              </div>
            ) : (
              <div className="text-xs text-zinc-500 py-4">
                Bấm vào một trong các Level phía trên để bắt đầu thử nghiệm trí nhớ làm việc.
              </div>
            )}
          </div>
        </div>
      )}

      {/* SUB-LAB 3: GUIDED CBT REFLECTION JOURNAL (ABCDE) */}
      {activeSubLab === "cbt-journal" && (
        <div data-testid="cbt-journal-container" className="space-y-6">
          <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 space-y-2">
            <span className="text-xs font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
              <Brain className="w-4 h-4 text-purple-600" />
              Liệu Pháp Nhận Thức - Hành Vi (CBT): Mô Hình ABCDE Tái Cấu Trúc Nhận Thức
            </span>
            <p className="text-xs text-zinc-600 dark:text-zinc-400 leading-relaxed">
              Không phải sự việc bên ngoài (A) gây ra cảm xúc tiêu cực (C), mà chính là niềm tin và diễn giải méo mó của chúng ta (B). Bằng cách tranh biện phản biện (D), bạn tạo ra niềm tin mới tích cực và bền bỉ (E).
            </p>
          </div>

          <div className="grid grid-cols-1 gap-4">
            <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 space-y-2">
              <label className="text-xs font-bold text-zinc-800 dark:text-zinc-200 block">
                A — Activating Event (Sự kiện kích hoạt khách quan):
              </label>
              <textarea
                data-testid="cbt-input-a"
                rows={2}
                value={cbtA}
                onChange={(e) => setCbtA(e.target.value)}
                className="w-full p-2.5 rounded-lg border border-zinc-200 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-900 text-xs text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-purple-500"
              />
            </div>

            <div className="p-4 rounded-xl border border-amber-200 dark:border-amber-800 bg-amber-50/30 dark:bg-amber-950/20 space-y-2">
              <label className="text-xs font-bold text-amber-900 dark:text-amber-300 block">
                B — Beliefs (Niềm tin / Suy nghĩ tự động tiêu cực &amp; Bẫy nhận thức):
              </label>
              <textarea
                data-testid="cbt-input-b"
                rows={2}
                value={cbtB}
                onChange={(e) => setCbtB(e.target.value)}
                className="w-full p-2.5 rounded-lg border border-amber-300 dark:border-amber-700 bg-white dark:bg-zinc-900 text-xs text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-purple-500"
              />
            </div>

            <div className="p-4 rounded-xl border border-red-200 dark:border-red-800 bg-red-50/30 dark:bg-red-950/20 space-y-2">
              <label className="text-xs font-bold text-red-900 dark:text-red-300 block">
                C — Consequences (Hệ quả cảm xúc &amp; Phản ứng sinh lý):
              </label>
              <textarea
                data-testid="cbt-input-c"
                rows={2}
                value={cbtC}
                onChange={(e) => setCbtC(e.target.value)}
                className="w-full p-2.5 rounded-lg border border-red-300 dark:border-red-700 bg-white dark:bg-zinc-900 text-xs text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-purple-500"
              />
            </div>

            <div className="p-4 rounded-xl border border-blue-200 dark:border-blue-800 bg-blue-50/30 dark:bg-blue-950/20 space-y-2">
              <label className="text-xs font-bold text-blue-900 dark:text-blue-300 block">
                D — Disputation (Thách thức &amp; Tranh biện phản biện dựa trên sự thật):
              </label>
              <textarea
                data-testid="cbt-input-d"
                rows={2}
                value={cbtD}
                onChange={(e) => setCbtD(e.target.value)}
                className="w-full p-2.5 rounded-lg border border-blue-300 dark:border-blue-700 bg-white dark:bg-zinc-900 text-xs text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-purple-500"
              />
            </div>

            <div className="p-4 rounded-xl border border-emerald-200 dark:border-emerald-800 bg-emerald-50/30 dark:bg-emerald-950/20 space-y-2">
              <label className="text-xs font-bold text-emerald-900 dark:text-emerald-300 block">
                E — Effective New Belief (Niềm tin mới cân bằng &amp; Hành động kiến tạo):
              </label>
              <textarea
                data-testid="cbt-input-e"
                rows={2}
                value={cbtE}
                onChange={(e) => setCbtE(e.target.value)}
                className="w-full p-2.5 rounded-lg border border-emerald-300 dark:border-emerald-700 bg-white dark:bg-zinc-900 text-xs text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-purple-500"
              />
            </div>
          </div>

          <div className="flex flex-wrap items-center justify-between gap-2 pt-2">
            <button
              type="button"
              onClick={() => handleSaveFlashcard(
                "Mô hình CBT ABCDE Tái Cấu Trúc Nhận Thức",
                "A (Activating Event) -> B (Belief) -> C (Consequence) -> D (Disputation) -> E (Effective New Belief).",
                "Áp dụng khi gặp cảm xúc tiêu cực dâng trào: viết ra suy nghĩ tự động (B) và tìm bằng chứng phản biện (D) để bình tĩnh lại."
              )}
              className="px-3.5 py-2 rounded-lg border border-purple-300 dark:border-purple-700 bg-purple-50 dark:bg-purple-950/50 hover:bg-purple-100 text-purple-800 dark:text-purple-300 text-xs font-semibold inline-flex items-center gap-1.5 transition cursor-pointer"
            >
              <Sparkles className="w-3.5 h-3.5" />
              Lưu Mô Hình Vào Flashcard FSRS
            </button>

            <button
              type="button"
              data-testid="cbt-save-journal-btn"
              onClick={handleSaveCbtJournal}
              className="px-5 py-2.5 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold transition cursor-pointer shadow-sm inline-flex items-center gap-2"
            >
              <BookOpen className="w-4 h-4" />
              Lưu Nhật Ký CBT Vào Sổ Tay &amp; Ghi Nhận Luyện Tập
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export function PsychologyLifeSkillsLab({
  courseId,
  courseTitle,
  lessonId,
  lessonTitle,
  lessonMarkdown,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  initialActivityType,
  initialActivityKey,
}: PracticeModuleProps) {
  const resolvedPsychTarget = useMemo(() => {
    const resolved = resolvePracticeSelection(
      initialActivityType,
      initialActivityKey,
      courseTitle,
      lessonTitle,
      lessonMarkdown
    );
    return resolved?.labMode === "recall" ? resolved : null;
  }, [initialActivityType, initialActivityKey, courseTitle, lessonTitle, lessonMarkdown]);

  // Navigation tabs
  const [activeTab, setActiveTab] = useState<PsychTabMode>(
    resolvedPsychTarget?.psychTab ?? "roleplay"
  );
  useEffect(() => {
    if (resolvedPsychTarget?.psychTab) {
      setActiveTab(resolvedPsychTarget.psychTab);
    }
  }, [resolvedPsychTarget]);

  const [dbActivities, setDbActivities] = useState<any[]>([]);
  const [practiceAttempts, setPracticeAttempts] = useState<any[]>([]);
  const [isLoadingBackend, setIsLoadingBackend] = useState(false);

  const normalizedPsychBank = useMemo(
    () => normalizePsychologyActivities(dbActivities),
    [dbActivities]
  );

  useEffect(() => {
    let isMounted = true;
    async function loadBackendPractice() {
      const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
      const validCourseId = courseId && UUID_REGEX.test(courseId) ? courseId : null;
      const validLessonId = lessonId && UUID_REGEX.test(lessonId) ? lessonId : null;
      if (!validCourseId && !validLessonId) {
        return;
      }
      setIsLoadingBackend(true);
      try {
        const headers = getClientRequestHeaders();
        const queryParams = new URLSearchParams();
        if (validCourseId) queryParams.set("courseId", validCourseId);
        if (validLessonId) queryParams.set("lessonId", validLessonId);
        const actUrl = `/api/v1/practice/activities?${queryParams.toString()}`;
        const attUrl = `/api/v1/practice/attempts?${queryParams.toString()}`;

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

  const recordPracticeAttempt = useCallback(
    async (params: {
      mode?: string;
      activityType?: string;
      activityId?: string | null;
      score: number;
      maxScore?: number;
      isPassed?: boolean;
      details?: any;
      detailJson?: string;
      durationSeconds?: number;
      activityTitle?: string;
      activityKey?: string;
    }) => {
      try {
        const headers = getClientRequestHeaders();
        const effectiveType = params.mode || params.activityType || "practice";
        const dbAct = dbActivities.find(
          (a) =>
            (params.activityId && a.id === params.activityId) ||
            a.type === effectiveType ||
            a.id === params.activityKey
        );
        const passed = params.isPassed ?? (params.score >= (dbAct?.passingScore ?? 70));
        const payload = {
          activityId: params.activityId || dbAct?.id || null,
          courseId,
          lessonId: lessonId || null,
          activityType: effectiveType,
          activityKey: params.activityKey || `${courseId}-${lessonId || "general"}-${effectiveType}`,
          activityTitle: params.activityTitle || `${lessonTitle || courseTitle} · ${effectiveType}`,
          score: Math.max(0, Math.min(params.maxScore || 100, Math.round(params.score))),
          maxScore: params.maxScore || 100,
          isPassed: passed,
          detailJson:
            typeof params.detailJson === "string"
              ? params.detailJson
              : JSON.stringify(params.details || {}),
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
          const saved = await res.json();
          setPracticeAttempts(prev => [saved, ...prev]);
          invalidateDashboardCache();
          if (typeof window !== "undefined") {
            window.dispatchEvent(new Event("swms:data-mutated"));
          }
        }
      } catch {
        // ignore
      }
    },
    [courseId, lessonId, lessonTitle, courseTitle, dbActivities]
  );

  // Extracted action steps & reflections from current lessonMarkdown
  const extractedLessonCases = useMemo(() => {
    const md = lessonMarkdown || "";
    const actions: string[] = [];
    const reflections: string[] = [];

    const bulletRegex = /(?:^|\n)(?:[-*•]|\d+\.)\s+(?:\[\s*\]\s*)?(?:\*\*)?([^*\n]{12,150})/g;
    let bm: RegExpExecArray | null;
    while ((bm = bulletRegex.exec(md)) !== null) {
      const text = bm[1].replace(/\*+/g, "").trim();
      if (!text || text.startsWith("LO-") || text.startsWith("[")) continue;
      if (/\?$/.test(text) || /(câu hỏi|tình huống|suy ngẫm|case|ví dụ|thực hành)/i.test(text)) {
        if (reflections.length < 6 && !reflections.includes(text)) reflections.push(text);
      } else if (actions.length < 8 && !actions.includes(text)) {
        actions.push(text);
      }
    }

    const detailsQRegex = /<summary[^>]*>([^<]{8,140})<\/summary>/gi;
    let dm: RegExpExecArray | null;
    while ((dm = detailsQRegex.exec(md)) !== null) {
      const q = dm[1].replace(/[*_`]/g, "").trim();
      if (q && reflections.length < 8 && !reflections.includes(q)) {
        reflections.push(q);
      }
    }

    if (actions.length === 0) {
      actions.push(
        `Áp dụng 1 nguyên tắc cốt lõi của "${lessonTitle}" trong 24 giờ tới.`,
        "Ghi lại 1 tình huống thực tế bạn đã quan sát được hôm nay mà không phán xét.",
        "Chia sẻ tóm tắt bài học cho một người bạn theo kỹ thuật Feynman."
      );
    }
    if (reflections.length === 0) {
      reflections.push(
        `Điều gì trong bài "${lessonTitle}" thay đổi góc nhìn của bạn nhiều nhất?`,
        "Trong tình huống căng thẳng sắp tới, bạn sẽ phản ứng khác đi như thế nào?"
      );
    }
    return { actions, reflections };
  }, [lessonMarkdown, lessonTitle]);

  const [checkedActions, setCheckedActions] = useState<Record<number, boolean>>({});
  const [lessonReflectionNote, setLessonReflectionNote] = useState("");

  // Notifications feedback toast
  const [toastMessage, setToastMessage] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const showToast = (msg: string) => {
    setToastMessage(msg);
    setTimeout(() => {
      setToastMessage(null);
    }, 3500);
  };

  /* -------------------------------------------------------------------------
     STATE & HANDLERS: CHẾ ĐỘ 1 - ROLEPLAY
     ------------------------------------------------------------------------- */
  const [selectedScenarioId, setSelectedScenarioId] = useState<string>("feedback-deadline");
  const currentScenario = useMemo(() => {
    return (
      ROLEPLAY_SCENARIOS.find((s) => s.id === selectedScenarioId) ||
      ROLEPLAY_SCENARIOS[0]
    );
  }, [selectedScenarioId]);

  const [currentTurnId, setCurrentTurnId] = useState<string>(currentScenario.startTurnId);
  const [chatHistory, setChatHistory] = useState<
    Array<{
      sender: "partner" | "user";
      text: string;
      meta?: {
        scoreRespect?: number;
        scoreEmpathy?: number;
        styleLabel?: string;
        isOptimal?: boolean;
        coachingTip?: string;
        nvcAnalysis?: NvcFeedback;
      };
    }>
  >([]);
  const [selectedOption, setSelectedOption] = useState<RoleplayOption | null>(null);
  const [scenarioCompleted, setScenarioCompleted] = useState<boolean>(false);

  // Initialize or reset scenario chat
  const resetScenario = (scenario: RoleplayScenario) => {
    setCurrentTurnId(scenario.startTurnId);
    setSelectedOption(null);
    setScenarioCompleted(false);
    const startTurn = scenario.turns[scenario.startTurnId];
    setChatHistory([
      {
        sender: "partner",
        text: startTurn?.partnerPrompt || "Xin chào, chúng ta cùng thảo luận nhé!",
      },
    ]);
  };

  useEffect(() => {
    resetScenario(currentScenario);
  }, [currentScenario]);

  const currentTurnData = useMemo(() => {
    return currentScenario.turns[currentTurnId] || null;
  }, [currentScenario, currentTurnId]);

  const handleSelectReplyOption = (opt: RoleplayOption) => {
    setSelectedOption(opt);
    // Push user reply to chat history
    const updatedHistory = [
      ...chatHistory,
      {
        sender: "user" as const,
        text: opt.userReplyText,
        meta: {
          scoreRespect: opt.scoreRespect,
          scoreEmpathy: opt.scoreEmpathy,
          styleLabel: opt.styleLabel,
          isOptimal: opt.isOptimal,
          coachingTip: opt.coachingTip,
          nvcAnalysis: opt.nvcAnalysis,
        },
      },
      {
        sender: "partner" as const,
        text: opt.partnerResponse,
      },
    ];
    setChatHistory(updatedHistory);

    if (opt.nextTurnId && currentScenario.turns[opt.nextTurnId]) {
      setCurrentTurnId(opt.nextTurnId);
      // add next partner turn prompt if available
      const nextTurn = currentScenario.turns[opt.nextTurnId];
      if (nextTurn.partnerPrompt !== opt.partnerResponse) {
        setChatHistory((prev) => [
          ...prev,
          {
            sender: "partner",
            text: nextTurn.partnerPrompt,
          },
        ]);
      }
    } else {
      setScenarioCompleted(true);
    }
  };

  const handleSaveRoleplayToNotebook = async () => {
    if (isSaving) return;
    setIsSaving(true);
    try {
      const summaryMarkdown = `
### 💬 Nhật Ký Thực Hành Hội Thoại: ${currentScenario.title}
- **Khóa học:** ${courseTitle} (${lessonTitle})
- **Tình huống:** ${currentScenario.initialContext}
- **Đối tác hội thoại:** ${currentScenario.partnerName} (${currentScenario.partnerRole})
- **Đánh giá phản xạ:**
${chatHistory
  .filter((m) => m.sender === "user")
  .map(
    (m, idx) => `
**Lượt ${idx + 1}:**
> "${m.text}"
- Phong cách: **${m.meta?.styleLabel}** | Tôn trọng: ${m.meta?.scoreRespect}/10 | Đồng cảm: ${m.meta?.scoreEmpathy}/10
- Bài học cốt lõi: ${m.meta?.coachingTip}
`
  )
  .join("\n")}
- **Ghi nhớ 4 bước Giao tiếp Trắc ẩn (NVC):**
  1. *Quan sát:* Nêu sự việc cụ thể, không dán nhãn hay phóng đại.
  2. *Cảm xúc:* Nói thật cảm giác bên trong của bản thân mà không đổ lỗi.
  3. *Nhu cầu:* Chia sẻ giá trị / điều bạn thật sự cần.
  4. *Đề nghị:* Đưa ra giải pháp hành động khả thi, rõ ràng và cùng hỗ trợ.
`;
      await onSaveToNotebook(summaryMarkdown);
      showToast("Đã lưu bài học hội thoại vào Sổ tay ghi chép!");
    } catch {
      showToast("Có lỗi khi lưu vào Sổ tay, vui lòng thử lại.");
    } finally {
      setIsSaving(false);
    }
  };

  const handleCreateRoleplayFlashcard = async () => {
    if (!onCreateFlashcardQuick || !selectedOption) return;
    const front = `[Giao Tiếp Ứng Xử] Cách phản hồi khi: "${currentScenario.title}"?`;
    const back = `✅ Phản hồi hiệu quả: "${selectedOption.userReplyText}"\n\n💡 Bí quyết NVC:\n- ${selectedOption.coachingTip}`;
    try {
      await onCreateFlashcardQuick(front, back);
      showToast("Đã tạo thẻ ghi nhớ giao tiếp thành công!");
    } catch {
      showToast("Có lỗi khi tạo thẻ ghi nhớ.");
    }
  };

  /* -------------------------------------------------------------------------
     STATE & HANDLERS: CHẾ ĐỘ 2 - COGNITIVE BIAS DETECTOR
     ------------------------------------------------------------------------- */
  const [currentBiasIndex, setCurrentBiasIndex] = useState<number>(0);
  const [selectedBiasKey, setSelectedBiasKey] = useState<string | null>(null);
  const [biasSubmitted, setBiasSubmitted] = useState<boolean>(false);
  const [userScore, setUserScore] = useState<{ correct: number; total: number }>({
    correct: 0,
    total: 0,
  });

  const activeBias = COGNITIVE_BIASES_LIST[currentBiasIndex];

  const handleSelectBiasOption = (key: string) => {
    if (biasSubmitted) return;
    setSelectedBiasKey(key);
  };

  const handleSubmitBiasCheck = () => {
    if (!selectedBiasKey || biasSubmitted) return;
    setBiasSubmitted(true);
    const isRight = selectedBiasKey === activeBias.correctBiasKey;
    setUserScore((prev) => ({
      correct: prev.correct + (isRight ? 1 : 0),
      total: prev.total + 1,
    }));
  };

  const handleNextBiasQuestion = () => {
    if (currentBiasIndex < COGNITIVE_BIASES_LIST.length - 1) {
      setCurrentBiasIndex((prev) => prev + 1);
      setSelectedBiasKey(null);
      setBiasSubmitted(false);
    } else {
      // Loop back or stay at end
      setCurrentBiasIndex(0);
      setSelectedBiasKey(null);
      setBiasSubmitted(false);
    }
  };

  const handleCreateBiasFlashcard = async () => {
    if (!onCreateFlashcardQuick) return;
    try {
      await onCreateFlashcardQuick(activeBias.flashcardFront, activeBias.flashcardBack);
      showToast("Đã tạo thẻ ghi nhớ thiên kiến tâm lý thành công!");
    } catch {
      showToast("Có lỗi khi tạo thẻ ghi nhớ.");
    }
  };

  const handleSaveBiasToNotebook = async () => {
    if (isSaving) return;
    setIsSaving(true);
    try {
      const noteContent = `
### Phân tích thiên kiến tâm lý: ${activeBias.scenarioTitle}
- **Tình huống:** ${activeBias.scenarioText}
- **Thiên kiến tâm lý:** ${activeBias.options.find((o) => o.key === activeBias.correctBiasKey)?.label}
- **Cơ chế não bộ:** ${activeBias.neuroscienceInsight}
- **Hành động ngay (gỡ bẫy tư duy):** ${activeBias.actionableHack}
`;
      await onSaveToNotebook(noteContent);
      showToast("Đã lưu phân tích thiên kiến tâm lý vào Sổ tay!");
    } catch {
      showToast("Có lỗi khi lưu vào Sổ tay.");
    } finally {
      setIsSaving(false);
    }
  };

  /* -------------------------------------------------------------------------
     STATE & HANDLERS: CHẾ ĐỘ 3 - CBT REFLECTION CARD
     ------------------------------------------------------------------------- */
  const [cbtFact, setCbtFact] = useState<string>("");
  const [cbtThought, setCbtThought] = useState<string>("");
  const [cbtStressLevel, setCbtStressLevel] = useState<number>(6);
  const [cbtEmotions, setCbtEmotions] = useState<string[]>(["Lo âu"]);
  const [cbtEvidenceFor, setCbtEvidenceFor] = useState<string>("");
  const [cbtEvidenceAgainst, setCbtEvidenceAgainst] = useState<string>("");
  const [cbtFriendAdvice, setCbtFriendAdvice] = useState<string>("");
  const [cbtMicroAction, setCbtMicroAction] = useState<string>("");

  const EMOTION_CHIPS = [
    "Lo âu",
    "Tức giận",
    "Thất vọng",
    "Xấu hổ",
    "Bất an",
    "Bất lực",
    "Tội lỗi",
    "Tủi thân",
    "Ghen tị",
    "Kiệt sức",
  ];

  const handleToggleEmotion = (emotion: string) => {
    if (cbtEmotions.includes(emotion)) {
      setCbtEmotions(cbtEmotions.filter((e) => e !== emotion));
    } else {
      setCbtEmotions([...cbtEmotions, emotion]);
    }
  };

  const handleLoadCbtPreset = (preset: CbtPreset) => {
    setCbtFact(preset.fact);
    setCbtThought(preset.autoThought);
    setCbtStressLevel(preset.stressLevel);
    setCbtEmotions(preset.emotions);
    setCbtEvidenceFor(preset.evidenceFor);
    setCbtEvidenceAgainst(preset.evidenceAgainst);
    setCbtFriendAdvice(preset.friendAdvice);
    setCbtMicroAction(preset.microAction);
    showToast(`Đã nạp tình huống mẫu: "${preset.title}"`);
  };

  const handleResetCbtForm = () => {
    setCbtFact("");
    setCbtThought("");
    setCbtStressLevel(5);
    setCbtEmotions(["Lo âu"]);
    setCbtEvidenceFor("");
    setCbtEvidenceAgainst("");
    setCbtFriendAdvice("");
    setCbtMicroAction("");
  };

  const handleSaveCbtToNotebook = async () => {
    if (!cbtFact.trim() && !cbtThought.trim()) {
      showToast("Vui lòng ghi lại sự việc hoặc suy nghĩ trước khi lưu.");
      return;
    }
    if (isSaving) return;
    setIsSaving(true);
    try {
      const now = new Date().toLocaleString("vi-VN", {
        hour: "2-digit",
        minute: "2-digit",
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      });
      const cbtMarkdown = `
### 🌿 Bản Gỡ Rối Suy Nghĩ CBT (${now})
- **Khóa học / Bài học:** ${courseTitle} - ${lessonTitle}
- **1. Sự việc khách quan:**
> ${cbtFact || "(Chưa điền)"}
- **2. Suy nghĩ tự động & Cảm xúc:**
> "${cbtThought || "(Chưa điền)"}"
- Cảm xúc trải nghiệm: **${cbtEmotions.join(", ") || "Chưa chọn"}** (Mức độ căng thẳng: **${cbtStressLevel}/10**)
- **3. Cân nhắc bằng chứng:**
  - *Bằng chứng ủng hộ:* ${cbtEvidenceFor || "Không có bằng chứng khách quan xác thực."}
  - *Bằng chứng phản bác:* ${cbtEvidenceAgainst || "Chưa có bằng chứng phủ định."}
- **4. Góc nhìn của người bạn thân trắc ẩn:**
> "${cbtFriendAdvice || "(Chưa điền)"}"
- **5. Hành động vi mô trong 5 phút tới:**
> ✅ **${cbtMicroAction || "(Chưa điền)"}**
`;
      await onSaveToNotebook(cbtMarkdown);
      showToast("Đã xuất và lưu bản gỡ rối suy nghĩ vào Sổ tay!");
    } catch {
      showToast("Có lỗi xảy ra khi lưu vào Sổ tay.");
    } finally {
      setIsSaving(false);
    }
  };

  /* -------------------------------------------------------------------------
     RENDER
     ------------------------------------------------------------------------- */
  return (
    <div data-testid="psychology-practice-lab" className="w-full max-w-5xl mx-auto space-y-6 pb-12 font-sans text-zinc-900 dark:text-zinc-100">
      {/* Toast Feedback Notification */}
      {toastMessage && (
        <div className="fixed bottom-6 right-6 z-50 flex items-center gap-2.5 px-4 py-3 rounded-xl bg-zinc-900/95 dark:bg-zinc-100/95 text-white dark:text-zinc-900 shadow-xl backdrop-blur-md text-sm font-medium border border-zinc-700/50 dark:border-zinc-300 transition-all animate-in fade-in slide-in-from-bottom-3 duration-200">
          <Sparkles className="w-4 h-4 text-amber-400 shrink-0" />
          <span>{toastMessage}</span>
        </div>
      )}

      {/* Main Header Card */}
      <div className="rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-gradient-to-br from-indigo-50/70 via-white to-purple-50/50 dark:from-zinc-900 dark:via-zinc-900 dark:to-indigo-950/30 p-5 sm:p-6 shadow-sm">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div className="space-y-1.5">
            <div className="inline-flex items-center gap-2 px-2.5 py-1 rounded-full text-xs font-semibold bg-indigo-100 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300 border border-indigo-200/60 dark:border-indigo-800/60">
              <Brain className="w-3.5 h-3.5" />
              <span>Phòng Thực Hành Tâm Lý Học & Kỹ Năng Sống</span>
            </div>
            <h2 className="text-xl sm:text-2xl font-bold tracking-tight text-zinc-900 dark:text-zinc-50">
              {lessonTitle || "Giao Tiếp Thấu Cảm & Tự Chủ Tâm Trí"}
            </h2>
            <p className="text-sm text-zinc-600 dark:text-zinc-400 max-w-2xl leading-relaxed">
              Áp dụng khoa học não bộ và liệu pháp CBT vào các tình huống đời sống thực tế:
              lắng nghe không phán xét, từ chối có ranh giới và hóa giải bẫy tâm lý thường ngày.
            </p>
          </div>

          <div className="flex items-center gap-2 self-start sm:self-center shrink-0">
            <div className="px-3 py-1.5 rounded-lg bg-white/80 dark:bg-zinc-800/80 border border-zinc-200 dark:border-zinc-700 text-xs text-zinc-600 dark:text-zinc-400 flex items-center gap-1.5">
              <Compass className="w-3.5 h-3.5 text-indigo-500" />
              <span>Học chậm & Tỉnh thức</span>
            </div>
          </div>
        </div>

        {/* Navigation Tabs */}
        <div className="flex flex-wrap items-center gap-2 mt-6 pt-5 border-t border-zinc-200/80 dark:border-zinc-800">
          <button
            type="button"
            onClick={() => setActiveTab("roleplay")}
            className={`min-h-[44px] px-4 py-2 rounded-xl text-sm font-medium transition-all flex items-center gap-2 cursor-pointer ${
              activeTab === "roleplay"
                ? "bg-indigo-600 text-white shadow-md shadow-indigo-500/20"
                : "bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-700 border border-zinc-200 dark:border-zinc-700"
            }`}
          >
            <MessageSquare className="w-4 h-4" />
            <span>1. Mô Phỏng Hội Thoại Thực Tế</span>
          </button>

          <button
            type="button"
            onClick={() => setActiveTab("biases")}
            className={`min-h-[44px] px-4 py-2 rounded-xl text-sm font-medium transition-all flex items-center gap-2 cursor-pointer ${
              activeTab === "biases"
                ? "bg-indigo-600 text-white shadow-md shadow-indigo-500/20"
                : "bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-700 border border-zinc-200 dark:border-zinc-700"
            }`}
          >
            <Zap className="w-4 h-4" />
            <span>2. Nhận diện thiên kiến tâm lý ({COGNITIVE_BIASES_LIST.length})</span>
          </button>

          <button
            type="button"
            onClick={() => setActiveTab("cbt")}
            className={`min-h-[44px] px-4 py-2 rounded-xl text-sm font-medium transition-all flex items-center gap-2 cursor-pointer ${
              activeTab === "cbt"
                ? "bg-indigo-600 text-white shadow-md shadow-indigo-500/20"
                : "bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-700 border border-zinc-200 dark:border-zinc-700"
            }`}
          >
            <Heart className="w-4 h-4" />
            <span>3. Gỡ Rối Suy Nghĩ CBT (5 Bước)</span>
          </button>

          <button
            type="button"
            data-testid="tab-lesson-cases"
            onClick={() => setActiveTab("lesson-cases")}
            className={`min-h-[44px] px-4 py-2 rounded-xl text-sm font-medium transition-all flex items-center gap-2 cursor-pointer ${
              activeTab === "lesson-cases"
                ? "bg-indigo-600 text-white shadow-md shadow-indigo-500/20"
                : "bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-700 border border-zinc-200 dark:border-zinc-700"
            }`}
          >
            <BookmarkCheck className="w-4 h-4" />
            <span>4. Checklist &amp; Tình Huống Bài Học</span>
          </button>

          <button
            type="button"
            data-testid="tab-cognitive-labs"
            onClick={() => setActiveTab("cognitive-labs")}
            className={`min-h-[44px] px-4 py-2 rounded-xl text-sm font-medium transition-all flex items-center gap-2 cursor-pointer ${
              activeTab === "cognitive-labs"
                ? "bg-purple-600 text-white shadow-md shadow-purple-500/20"
                : "bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-700 border border-zinc-200 dark:border-zinc-700"
            }`}
          >
            <Brain className="w-4 h-4" />
            <span>5. Phòng Thí Nghiệm Nhận Thức (Cognitive Labs)</span>
          </button>

          <button
            type="button"
            data-testid="tab-bias-spotter"
            onClick={() => setActiveTab("bias-spotter")}
            className={`min-h-[44px] px-4 py-2 rounded-xl text-sm font-medium transition-all flex items-center gap-2 cursor-pointer ${
              activeTab === "bias-spotter"
                ? "bg-amber-600 text-white shadow-md shadow-amber-500/20"
                : "bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-700 border border-zinc-200 dark:border-zinc-700"
            }`}
          >
            <Zap className="w-4 h-4" />
            <span>6. Nhận diện Thiên kiến (Tự gõ)</span>
          </button>

          <button
            type="button"
            data-testid="tab-feynman"
            onClick={() => setActiveTab("feynman")}
            className={`min-h-[44px] px-4 py-2 rounded-xl text-sm font-medium transition-all flex items-center gap-2 cursor-pointer ${
              activeTab === "feynman"
                ? "bg-indigo-600 text-white shadow-md shadow-indigo-500/20"
                : "bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-700 border border-zinc-200 dark:border-zinc-700"
            }`}
          >
            <Award className="w-4 h-4" />
            <span>7. Feynman Tự định nghĩa</span>
          </button>

          <button
            type="button"
            data-testid="tab-cbt-exercise"
            onClick={() => setActiveTab("cbt-exercise")}
            className={`min-h-[44px] px-4 py-2 rounded-xl text-sm font-medium transition-all flex items-center gap-2 cursor-pointer ${
              activeTab === "cbt-exercise"
                ? "bg-purple-600 text-white shadow-md shadow-purple-500/20"
                : "bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-700 border border-zinc-200 dark:border-zinc-700"
            }`}
          >
            <Heart className="w-4 h-4" />
            <span>8. Bài tập CBT Tình huống</span>
          </button>

          <button
            type="button"
            data-testid="tab-apply-journal"
            onClick={() => setActiveTab("apply-journal")}
            className={`min-h-[44px] px-4 py-2 rounded-xl text-sm font-medium transition-all flex items-center gap-2 cursor-pointer ${
              activeTab === "apply-journal"
                ? "bg-emerald-600 text-white shadow-md shadow-emerald-500/20"
                : "bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-700 border border-zinc-200 dark:border-zinc-700"
            }`}
          >
            <Compass className="w-4 h-4" />
            <span>9. Nhật ký áp dụng</span>
          </button>
        </div>
      </div>

      {/* =====================================================================
          TAB 1: MÔ PHỎNG HỘI THOẠI THỰC TẾ (ROLEPLAY)
         ===================================================================== */}
      {activeTab === "roleplay" && (
        <div className="space-y-6 animate-in fade-in duration-200">
          {/* Scenario Selector Chips */}
          <div className="space-y-2">
            <span className="text-xs font-semibold uppercase tracking-wider text-zinc-500 dark:text-zinc-400">
              Chọn kịch bản thực tế cần tôi luyện:
            </span>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-2.5">
              {ROLEPLAY_SCENARIOS.map((sc) => {
                const isSelected = sc.id === selectedScenarioId;
                return (
                  <button
                    key={sc.id}
                    type="button"
                    onClick={() => setSelectedScenarioId(sc.id)}
                    className={`min-h-[48px] text-left p-3 rounded-xl border transition-all cursor-pointer flex flex-col justify-between gap-1.5 ${
                      isSelected
                        ? "bg-indigo-50/80 dark:bg-indigo-950/40 border-indigo-400 dark:border-indigo-600 shadow-sm ring-1 ring-indigo-400"
                        : "bg-white dark:bg-zinc-900 border-zinc-200 dark:border-zinc-800 hover:border-zinc-300 dark:hover:border-zinc-700"
                    }`}
                  >
                    <div className="flex items-center justify-between gap-1">
                      <span className="text-[10px] font-semibold uppercase px-2 py-0.5 rounded bg-zinc-100 dark:bg-zinc-800 text-zinc-600 dark:text-zinc-400">
                        {sc.badge}
                      </span>
                      {isSelected && (
                        <CheckCircle2 className="w-3.5 h-3.5 text-indigo-600 dark:text-indigo-400 shrink-0" />
                      )}
                    </div>
                    <span className="text-xs font-medium text-zinc-900 dark:text-zinc-100 line-clamp-2">
                      {sc.title}
                    </span>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Context Banner */}
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 space-y-2">
            <div className="flex items-center justify-between flex-wrap gap-2">
              <div className="flex items-center gap-2">
                <div className="w-7 h-7 rounded-full bg-indigo-100 dark:bg-indigo-900/60 text-indigo-700 dark:text-indigo-300 flex items-center justify-center text-xs font-bold">
                  {currentScenario.partnerName.charAt(0)}
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-zinc-900 dark:text-zinc-100">
                    Đối tác: {currentScenario.partnerName}
                  </h3>
                  <p className="text-xs text-zinc-500 dark:text-zinc-400">
                    {currentScenario.partnerRole}
                  </p>
                </div>
              </div>
              <button
                type="button"
                onClick={() => resetScenario(currentScenario)}
                className="min-h-[36px] px-3 py-1.5 rounded-lg border border-zinc-200 dark:border-zinc-700 text-xs font-medium text-zinc-600 dark:text-zinc-300 hover:bg-zinc-50 dark:hover:bg-zinc-800 flex items-center gap-1.5 transition cursor-pointer"
              >
                <RefreshCw className="w-3.5 h-3.5" />
                <span>Bắt đầu lại</span>
              </button>
            </div>
            <p className="text-xs text-zinc-600 dark:text-zinc-400 bg-zinc-50 dark:bg-zinc-800/60 p-2.5 rounded-lg border border-zinc-200/60 dark:border-zinc-800 italic">
              Bối cảnh: {currentScenario.initialContext}
            </p>
          </div>

          {/* Interactive Chat Arena */}
          <div className="rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50/50 dark:bg-zinc-950 p-4 sm:p-5 space-y-4">
            <span className="text-xs font-semibold uppercase tracking-wider text-zinc-500 dark:text-zinc-400 flex items-center gap-1.5">
              <MessageSquare className="w-3.5 h-3.5 text-indigo-500" />
              Diễn biến cuộc trò chuyện
            </span>

            {/* Message Feed */}
            <div className="space-y-3.5 max-h-[460px] overflow-y-auto pr-1">
              {chatHistory.map((msg, idx) => {
                const isPartner = msg.sender === "partner";
                return (
                  <div
                    key={idx}
                    className={`flex gap-2.5 ${isPartner ? "justify-start" : "justify-end"}`}
                  >
                    {isPartner && (
                      <div className="w-8 h-8 rounded-full bg-purple-100 dark:bg-purple-900/60 text-purple-700 dark:text-purple-300 flex items-center justify-center text-xs font-bold shrink-0 mt-0.5">
                        {currentScenario.partnerName.charAt(0)}
                      </div>
                    )}
                    <div
                      className={`max-w-[85%] sm:max-w-[75%] rounded-2xl p-3.5 space-y-2 text-sm leading-relaxed ${
                        isPartner
                          ? "bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 text-zinc-800 dark:text-zinc-200 rounded-tl-sm shadow-xs"
                          : "bg-indigo-600 text-white rounded-tr-sm shadow-md shadow-indigo-600/10"
                      }`}
                    >
                      <div className="flex items-center justify-between gap-2 text-[11px] opacity-80 mb-0.5">
                        <span className="font-semibold">
                          {isPartner ? currentScenario.partnerName : "Bạn (Người học)"}
                        </span>
                        {msg.meta?.styleLabel && (
                          <span className="px-2 py-0.5 rounded-full bg-white/20 text-[10px] font-medium">
                            {msg.meta.styleLabel}
                          </span>
                        )}
                      </div>
                      <p className="whitespace-pre-wrap">{msg.text}</p>

                      {/* Analysis Badge for User Replies */}
                      {msg.meta && (
                        <div className="pt-2 mt-2 border-t border-white/20 text-xs space-y-1 text-white/90">
                          <div className="flex items-center gap-3 text-[11px] font-semibold">
                            <span>Sự tôn trọng: {msg.meta.scoreRespect}/10</span>
                            <span>•</span>
                            <span>Đồng cảm: {msg.meta.scoreEmpathy}/10</span>
                          </div>
                          {msg.meta.coachingTip && (
                            <p className="text-[11px] opacity-95 italic">
                              Gợi ý: {msg.meta.coachingTip}
                            </p>
                          )}
                        </div>
                      )}
                    </div>
                    {!isPartner && (
                      <div className="w-8 h-8 rounded-full bg-indigo-700 text-white flex items-center justify-center text-xs font-bold shrink-0 mt-0.5">
                        Bạn
                      </div>
                    )}
                  </div>
                );
              })}
            </div>

            {/* Reply Options Drawer */}
            {!scenarioCompleted && currentTurnData && (
              <div className="mt-4 pt-4 border-t border-zinc-200 dark:border-zinc-800 space-y-2.5">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-zinc-700 dark:text-zinc-300">
                    Lượt phản hồi của bạn — Hãy chọn câu trả lời phù hợp:
                  </span>
                  {currentTurnData.partnerContextHint && (
                    <span className="text-[11px] text-zinc-500 italic">
                      Gợi ý: {currentTurnData.partnerContextHint}
                    </span>
                  )}
                </div>

                <div className="space-y-2">
                  {currentTurnData.options.map((opt) => (
                    <button
                      key={opt.id}
                      type="button"
                      onClick={() => handleSelectReplyOption(opt)}
                      className="w-full text-left p-3.5 rounded-xl border border-zinc-200 dark:border-zinc-700 bg-white dark:bg-zinc-900 hover:border-indigo-400 dark:hover:border-indigo-500 hover:bg-indigo-50/40 dark:hover:bg-indigo-950/30 transition-all cursor-pointer min-h-[44px] group flex items-start gap-3"
                    >
                      <div className="w-5 h-5 rounded-full border border-zinc-300 dark:border-zinc-600 group-hover:border-indigo-500 flex items-center justify-center shrink-0 mt-0.5 text-xs text-zinc-400 group-hover:text-indigo-600">
                        <Send className="w-3 h-3" />
                      </div>
                      <div className="space-y-1 flex-1">
                        <div className="flex items-center justify-between gap-2">
                          <span className="text-xs font-semibold text-zinc-900 dark:text-zinc-100 group-hover:text-indigo-600 dark:group-hover:text-indigo-400">
                            {opt.userReplyText}
                          </span>
                        </div>
                        <div className="flex items-center gap-2 text-[10px] text-zinc-500">
                          <span className="px-1.5 py-0.5 rounded bg-zinc-100 dark:bg-zinc-800 font-medium">
                            {opt.styleLabel}
                          </span>
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              </div>
            )}

            {/* Completion Summary & Action Card */}
            {scenarioCompleted && (
              <div className="rounded-xl border border-emerald-200 dark:border-emerald-800 bg-emerald-50/70 dark:bg-emerald-950/40 p-4 space-y-3">
                <div className="flex items-center gap-2 text-emerald-800 dark:text-emerald-300 font-semibold text-sm">
                  <CheckCircle2 className="w-5 h-5 text-emerald-600 dark:text-emerald-400 shrink-0" />
                  <span>Hoàn thành kịch bản hội thoại thực tế!</span>
                </div>
                <p className="text-xs text-emerald-700 dark:text-emerald-300/90 leading-relaxed">
                  Bạn vừa hoàn thành một phiên giao tiếp phân nhánh. Kỹ năng giao tiếp trắc ẩn (NVC) đòi hỏi sự kiên nhẫn: tách biệt việc khách quan khỏi cảm xúc phán xét, và luôn gắn liền mong muốn của mình với một đề nghị hành động cụ thể.
                </p>

                <div className="flex flex-wrap items-center gap-2.5 pt-2">
                  <button
                    type="button"
                    onClick={() => void handleSaveRoleplayToNotebook()}
                    disabled={isSaving}
                    className="min-h-[44px] px-4 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-medium flex items-center gap-1.5 transition cursor-pointer shadow-xs disabled:opacity-60"
                  >
                    <BookOpen className="w-4 h-4" />
                    <span>Lưu bài học hội thoại vào Sổ tay</span>
                  </button>

                  {onCreateFlashcardQuick && selectedOption && (
                    <button
                      type="button"
                      onClick={() => void handleCreateRoleplayFlashcard()}
                      className="min-h-[44px] px-4 py-2 rounded-xl bg-white dark:bg-zinc-900 border border-emerald-300 dark:border-emerald-700 text-emerald-800 dark:text-emerald-300 hover:bg-emerald-100/50 dark:hover:bg-emerald-900/50 text-xs font-medium flex items-center gap-1.5 transition cursor-pointer"
                    >
                      <Sparkles className="w-4 h-4 text-emerald-600" />
                      <span>Tạo thẻ ghi nhớ câu nói mẫu</span>
                    </button>
                  )}

                  <button
                    type="button"
                    onClick={() => resetScenario(currentScenario)}
                    className="min-h-[44px] px-4 py-2 rounded-xl border border-zinc-300 dark:border-zinc-700 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800 text-xs font-medium flex items-center gap-1.5 transition cursor-pointer"
                  >
                    <RefreshCw className="w-3.5 h-3.5" />
                    <span>Luyện lại tình huống này</span>
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* NVC 4-Step Reference Card */}
          <div className="rounded-2xl border border-indigo-100 dark:border-indigo-900/60 bg-gradient-to-r from-indigo-50/40 via-white to-purple-50/30 dark:from-zinc-900 dark:via-zinc-900 dark:to-indigo-950/20 p-4 sm:p-5 space-y-3">
            <div className="flex items-center gap-2">
              <Shield className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
              <h4 className="text-xs font-bold uppercase tracking-wider text-indigo-900 dark:text-indigo-200">
                Khung Chuẩn 4 Bước Giao Tiếp Trắc Ẩn (Nonviolent Communication - NVC)
              </h4>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 text-xs">
              <div className="p-3 rounded-xl bg-white dark:bg-zinc-800/80 border border-zinc-200 dark:border-zinc-700 space-y-1">
                <span className="font-bold text-indigo-600 dark:text-indigo-400">1. Quan sát (Observation)</span>
                <p className="text-zinc-600 dark:text-zinc-400">
                  Ghi lại sự thật như camera quay lại. Tuyệt đối không dán nhãn: &quot;Cậu luôn luôn...&quot; hay &quot;Cậu vô trách nhiệm&quot;.
                </p>
              </div>
              <div className="p-3 rounded-xl bg-white dark:bg-zinc-800/80 border border-zinc-200 dark:border-zinc-700 space-y-1">
                <span className="font-bold text-indigo-600 dark:text-indigo-400">2. Cảm xúc (Feeling)</span>
                <p className="text-zinc-600 dark:text-zinc-400">
                  Bày tỏ cảm xúc thật của chính mình: &quot;Tôi cảm thấy lo lắng, bất an&quot; thay vì đổ lỗi &quot;Cậu làm tôi bực&quot;.
                </p>
              </div>
              <div className="p-3 rounded-xl bg-white dark:bg-zinc-800/80 border border-zinc-200 dark:border-zinc-700 space-y-1">
                <span className="font-bold text-indigo-600 dark:text-indigo-400">3. Nhu cầu (Need)</span>
                <p className="text-zinc-600 dark:text-zinc-400">
                  Nêu rõ giá trị bạn coi trọng: &quot;Vì tôi cần sự an tâm và bài thuyết trình được đồng bộ trước hạn chót&quot;.
                </p>
              </div>
              <div className="p-3 rounded-xl bg-white dark:bg-zinc-800/80 border border-zinc-200 dark:border-zinc-700 space-y-1">
                <span className="font-bold text-indigo-600 dark:text-indigo-400">4. Đề nghị (Request)</span>
                <p className="text-zinc-600 dark:text-zinc-400">
                  Hành động cụ thể, khả thi và tích cực: &quot;Cậu có thể gửi dàn ý trong 30 phút nữa không, mình sẽ format phụ cậu?&quot;.
                </p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* =====================================================================
          TAB 2: NHẬN DIỆN BẪY TÂM LÝ (COGNITIVE BIAS DETECTOR)
         ===================================================================== */}
      {activeTab === "biases" && (
        <div className="space-y-6 animate-in fade-in duration-200">
          {/* Header Stats Bar */}
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            <div className="flex items-center gap-2">
              <span className="text-xs font-semibold px-2.5 py-1 rounded-full bg-amber-100 dark:bg-amber-950/70 text-amber-800 dark:text-amber-300">
                Câu hỏi {currentBiasIndex + 1} / {COGNITIVE_BIASES_LIST.length}
              </span>
              <span className="text-xs text-zinc-500">
                Độ chính xác: {userScore.correct}/{userScore.total || 0}
              </span>
            </div>

            {/* Mini Progress Dots */}
            <div className="flex items-center gap-1.5 overflow-x-auto py-1">
              {COGNITIVE_BIASES_LIST.map((b, idx) => {
                const isCurrent = idx === currentBiasIndex;
                return (
                  <button
                    key={b.id}
                    type="button"
                    onClick={() => {
                      setCurrentBiasIndex(idx);
                      setSelectedBiasKey(null);
                      setBiasSubmitted(false);
                    }}
                    className={`w-6 h-6 rounded-full text-[11px] font-semibold transition cursor-pointer flex items-center justify-center ${
                      isCurrent
                        ? "bg-indigo-600 text-white ring-2 ring-indigo-400"
                        : "bg-zinc-100 dark:bg-zinc-800 text-zinc-600 dark:text-zinc-400 hover:bg-zinc-200"
                    }`}
                  >
                    {idx + 1}
                  </button>
                );
              })}
            </div>
          </div>

          {/* Scenario Card */}
          <div className="rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-5 sm:p-6 space-y-4 shadow-sm">
            <div className="flex items-center justify-between gap-2 flex-wrap">
              <span className="text-xs font-semibold px-2 py-0.5 rounded bg-indigo-50 dark:bg-indigo-950/50 text-indigo-700 dark:text-indigo-300 border border-indigo-200/50 dark:border-indigo-800/50">
                Ngữ cảnh: {activeBias.realWorldContext}
              </span>
              <span className="text-xs text-zinc-500">
                Thiên kiến nhận thức nào đang chi phối?
              </span>
            </div>

            <h3 className="text-base sm:text-lg font-bold text-zinc-900 dark:text-zinc-50">
              {activeBias.scenarioTitle}
            </h3>

            <div className="p-4 rounded-xl bg-zinc-50 dark:bg-zinc-800/60 border border-zinc-200/80 dark:border-zinc-800 text-sm leading-relaxed text-zinc-800 dark:text-zinc-200">
              {activeBias.scenarioText}
            </div>

            {/* Multiple Choice Options */}
            <div className="space-y-2.5 pt-2">
              <span className="text-xs font-semibold uppercase tracking-wider text-zinc-500">
                Chọn thiên kiến tâm lý phù hợp với tình huống trên:
              </span>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
                {activeBias.options.map((opt) => {
                  const isSelected = selectedBiasKey === opt.key;
                  const isCorrect = opt.key === activeBias.correctBiasKey;

                  let cardStyle =
                    "bg-white dark:bg-zinc-900 border-zinc-200 dark:border-zinc-700 hover:border-indigo-300 dark:hover:border-indigo-700";
                  if (isSelected && !biasSubmitted) {
                    cardStyle =
                      "bg-indigo-50 dark:bg-indigo-950/40 border-indigo-500 ring-2 ring-indigo-400";
                  } else if (biasSubmitted) {
                    if (isCorrect) {
                      cardStyle =
                        "bg-emerald-50 dark:bg-emerald-950/40 border-emerald-500 ring-2 ring-emerald-400";
                    } else if (isSelected && !isCorrect) {
                      cardStyle =
                        "bg-rose-50 dark:bg-rose-950/40 border-rose-500 ring-2 ring-rose-400";
                    }
                  }

                  return (
                    <button
                      key={opt.key}
                      type="button"
                      disabled={biasSubmitted}
                      onClick={() => handleSelectBiasOption(opt.key)}
                      className={`min-h-[52px] p-3.5 rounded-xl border text-left transition-all cursor-pointer flex items-center justify-between gap-2 ${cardStyle}`}
                    >
                      <span className="text-xs sm:text-sm font-medium text-zinc-900 dark:text-zinc-100">
                        {opt.label}
                      </span>
                      {biasSubmitted && isCorrect && (
                        <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
                      )}
                      {biasSubmitted && isSelected && !isCorrect && (
                        <AlertCircle className="w-4 h-4 text-rose-600 dark:text-rose-400 shrink-0" />
                      )}
                    </button>
                  );
                })}
              </div>
            </div>

            {/* Check Button */}
            {!biasSubmitted && (
              <div className="pt-3">
                <button
                  type="button"
                  disabled={!selectedBiasKey}
                  onClick={handleSubmitBiasCheck}
                  className="min-h-[44px] px-6 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white font-medium text-sm transition cursor-pointer flex items-center gap-2 shadow-xs"
                >
                  <span>Kiểm tra đáp án</span>
                  <ArrowRight className="w-4 h-4" />
                </button>
              </div>
            )}

            {/* Post-Submit Detailed Coaching Analysis */}
            {biasSubmitted && (
              <div className="space-y-4 pt-4 border-t border-zinc-200 dark:border-zinc-800 animate-in fade-in duration-200">
                {/* Result Notification */}
                <div
                  className={`p-4 rounded-xl border ${
                    selectedBiasKey === activeBias.correctBiasKey
                      ? "bg-emerald-50/80 dark:bg-emerald-950/40 border-emerald-300 dark:border-emerald-800 text-emerald-900 dark:text-emerald-200"
                      : "bg-amber-50/80 dark:bg-amber-950/40 border-amber-300 dark:border-amber-800 text-amber-900 dark:text-amber-200"
                  }`}
                >
                  <div className="flex items-center gap-2 font-bold text-sm mb-1">
                    {selectedBiasKey === activeBias.correctBiasKey ? (
                      <>
                        <CheckCircle2 className="w-4 h-4 text-emerald-600" />
                        <span>Chính xác! Bạn đã nhận diện đúng thiên kiến tâm lý.</span>
                      </>
                    ) : (
                      <>
                        <Info className="w-4 h-4 text-amber-600" />
                        <span>Chưa hoàn toàn chuẩn xác! Hãy xem giải thích chi tiết dưới đây:</span>
                      </>
                    )}
                  </div>
                  <p className="text-xs leading-relaxed">
                    {
                      activeBias.options.find((o) => o.key === activeBias.correctBiasKey)
                        ?.explanation
                    }
                  </p>
                </div>

                {/* Neuroscience Insight & Actionable Hack */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-3.5">
                  <div className="p-4 rounded-xl bg-purple-50/50 dark:bg-purple-950/30 border border-purple-200/70 dark:border-purple-900/50 space-y-2">
                    <span className="text-xs font-bold uppercase tracking-wider text-purple-800 dark:text-purple-300 flex items-center gap-1.5">
                      <Brain className="w-4 h-4" />
                      Cơ chế tiến hóa của bộ não
                    </span>
                    <p className="text-xs text-zinc-700 dark:text-zinc-300 leading-relaxed">
                      {activeBias.neuroscienceInsight}
                    </p>
                  </div>

                  <div className="p-4 rounded-xl bg-amber-50/50 dark:bg-amber-950/30 border border-amber-200/70 dark:border-amber-900/50 space-y-2">
                    <span className="text-xs font-bold uppercase tracking-wider text-amber-800 dark:text-amber-300 flex items-center gap-1.5">
                      <Zap className="w-4 h-4" />
                      Mẹo hành động ngay (Mind-Hack)
                    </span>
                    <p className="text-xs text-zinc-700 dark:text-zinc-300 leading-relaxed font-medium">
                      {activeBias.actionableHack}
                    </p>
                  </div>
                </div>

                {/* Actions Toolbar */}
                <div className="flex flex-wrap items-center justify-between gap-3 pt-2">
                  <div className="flex flex-wrap items-center gap-2">
                    {onCreateFlashcardQuick && (
                      <button
                        type="button"
                        onClick={() => void handleCreateBiasFlashcard()}
                        className="min-h-[44px] px-3.5 py-2 rounded-xl border border-indigo-200 dark:border-indigo-800 bg-indigo-50/80 dark:bg-indigo-950/50 text-indigo-700 dark:text-indigo-300 text-xs font-medium hover:bg-indigo-100 flex items-center gap-1.5 transition cursor-pointer"
                      >
                        <Sparkles className="w-3.5 h-3.5" />
                        <span>Tạo thẻ ghi nhớ thiên kiến này</span>
                      </button>
                    )}

                    <button
                      type="button"
                      onClick={() => void handleSaveBiasToNotebook()}
                      disabled={isSaving}
                      className="min-h-[44px] px-3.5 py-2 rounded-xl border border-zinc-200 dark:border-zinc-700 bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 text-xs font-medium hover:bg-zinc-100 flex items-center gap-1.5 transition cursor-pointer"
                    >
                      <BookOpen className="w-3.5 h-3.5" />
                      <span>Lưu vào Sổ tay</span>
                    </button>
                  </div>

                  <button
                    type="button"
                    onClick={handleNextBiasQuestion}
                    className="min-h-[44px] px-5 py-2 rounded-xl bg-zinc-900 dark:bg-zinc-100 text-white dark:text-zinc-900 text-xs font-medium hover:bg-zinc-800 flex items-center gap-2 transition cursor-pointer shadow-xs ml-auto"
                  >
                    <span>
                      {currentBiasIndex < COGNITIVE_BIASES_LIST.length - 1
                        ? "Câu hỏi tiếp theo"
                        : "Luyện lại từ đầu"}
                    </span>
                    <ChevronRight className="w-4 h-4" />
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {/* =====================================================================
          TAB 3: GỠ RỐI SUY NGHĨ & CÂN BẰNG CẢM XÚC (CBT REFLECTION CARD)
         ===================================================================== */}
      {activeTab === "cbt" && (
        <div className="space-y-6 animate-in fade-in duration-200">
          {/* Quick Presets Drawer */}
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 space-y-2.5">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold uppercase tracking-wider text-zinc-500 flex items-center gap-1.5">
                <Sparkles className="w-3.5 h-3.5 text-amber-500" />
                Nạp nhanh tình huống mẫu để tham khảo:
              </span>
              <button
                type="button"
                onClick={handleResetCbtForm}
                className="text-xs text-zinc-500 hover:text-zinc-800 dark:hover:text-zinc-200 underline cursor-pointer"
              >
                Xóa làm mới
              </button>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-2">
              {CBT_PRESETS.map((p, idx) => (
                <button
                  key={idx}
                  type="button"
                  onClick={() => handleLoadCbtPreset(p)}
                  className="min-h-[44px] text-left p-2.5 rounded-lg border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-800/40 hover:border-indigo-400 dark:hover:border-indigo-600 transition cursor-pointer text-xs"
                >
                  <span className="font-semibold text-zinc-900 dark:text-zinc-100 line-clamp-1">
                    {p.title}
                  </span>
                  <span className="text-[10px] text-zinc-500 block mt-0.5">{p.badge}</span>
                </button>
              ))}
            </div>
          </div>

          {/* 5-Step Interactive Form */}
          <div className="rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-5 sm:p-6 space-y-6 shadow-sm">
            {/* Step 1: Objective Event */}
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <label className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                  <span className="w-5 h-5 rounded-full bg-indigo-600 text-white flex items-center justify-center text-xs">
                    1
                  </span>
                  Sự việc vừa xảy ra là gì? (Sự thật khách quan)
                </label>
                <span className="text-[11px] text-zinc-500 italic">
                  Chỉ ghi như máy quay ghi hình, không phán xét
                </span>
              </div>
              <textarea
                rows={2}
                value={cbtFact}
                onChange={(e) => setCbtFact(e.target.value)}
                placeholder="Ví dụ: Lúc 14h, tin nhắn trong nhóm Zalo của mình chưa có ai thả tim hoặc trả lời..."
                className="w-full p-3 rounded-xl border border-zinc-200 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-800 text-sm text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-indigo-500 transition resize-none"
              />
            </div>

            {/* Step 2: Automatic Negative Thought + Emotions & Stress */}
            <div className="space-y-3 pt-2 border-t border-zinc-200 dark:border-zinc-800">
              <label className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                <span className="w-5 h-5 rounded-full bg-indigo-600 text-white flex items-center justify-center text-xs">
                  2
                </span>
                Suy nghĩ tự động đầu tiên nảy ra & Cảm xúc
              </label>

              <textarea
                rows={2}
                value={cbtThought}
                onChange={(e) => setCbtThought(e.target.value)}
                placeholder="Ví dụ: Mọi người coi thường mình, mình chẳng có tiếng nói gì trong nhóm này..."
                className="w-full p-3 rounded-xl border border-zinc-200 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-800 text-sm text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-indigo-500 transition resize-none"
              />

              {/* Emotion Selector Chips */}
              <div className="space-y-1.5">
                <span className="text-xs font-semibold text-zinc-600 dark:text-zinc-400">
                  Chọn cảm xúc bạn đang trải nghiệm:
                </span>
                <div className="flex flex-wrap gap-1.5">
                  {EMOTION_CHIPS.map((emo) => {
                    const isSelected = cbtEmotions.includes(emo);
                    return (
                      <button
                        key={emo}
                        type="button"
                        onClick={() => handleToggleEmotion(emo)}
                        className={`min-h-[36px] px-3 py-1.5 rounded-lg text-xs font-medium transition cursor-pointer ${
                          isSelected
                            ? "bg-rose-500 text-white shadow-xs"
                            : "bg-zinc-100 dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-700"
                        }`}
                      >
                        {emo}
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* Stress Level Slider */}
              <div className="space-y-1.5 pt-1">
                <div className="flex items-center justify-between text-xs font-semibold">
                  <span className="text-zinc-600 dark:text-zinc-400">
                    Mức độ căng thẳng / áp lực:
                  </span>
                  <span
                    className={`px-2 py-0.5 rounded text-white ${
                      cbtStressLevel >= 8
                        ? "bg-rose-600"
                        : cbtStressLevel >= 5
                        ? "bg-amber-600"
                        : "bg-emerald-600"
                    }`}
                  >
                    {cbtStressLevel} / 10
                  </span>
                </div>
                <input
                  type="range"
                  min={1}
                  max={10}
                  value={cbtStressLevel}
                  onChange={(e) => setCbtStressLevel(Number(e.target.value))}
                  className="w-full h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg appearance-none cursor-pointer accent-indigo-600"
                />
              </div>
            </div>

            {/* Step 3: Evidence Testing (For vs Against) */}
            <div className="space-y-3 pt-2 border-t border-zinc-200 dark:border-zinc-800">
              <label className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                <span className="w-5 h-5 rounded-full bg-indigo-600 text-white flex items-center justify-center text-xs">
                  3
                </span>
                Cân nhắc bằng chứng: Có đúng như bạn nghĩ không?
              </label>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <span className="text-xs font-semibold text-rose-600 dark:text-rose-400 flex items-center gap-1">
                    <AlertCircle className="w-3.5 h-3.5" />
                    Bằng chứng ủng hộ suy nghĩ tiêu cực đó:
                  </span>
                  <textarea
                    rows={3}
                    value={cbtEvidenceFor}
                    onChange={(e) => setCbtEvidenceFor(e.target.value)}
                    placeholder="Những sự kiện thật nào khiến bạn tin vào điều tiêu cực đó?"
                    className="w-full p-2.5 rounded-xl border border-zinc-200 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-800 text-xs text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-rose-400 transition resize-none"
                  />
                </div>

                <div className="space-y-1.5">
                  <span className="text-xs font-semibold text-emerald-600 dark:text-emerald-400 flex items-center gap-1">
                    <CheckCircle2 className="w-3.5 h-3.5" />
                    Bằng chứng phản bác / Sự thật ngược lại:
                  </span>
                  <textarea
                    rows={3}
                    value={cbtEvidenceAgainst}
                    onChange={(e) => setCbtEvidenceAgainst(e.target.value)}
                    placeholder="Sự thật khách quan nào cho thấy bạn đang suy diễn hoặc thổi phồng?"
                    className="w-full p-2.5 rounded-xl border border-zinc-200 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-800 text-xs text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-emerald-400 transition resize-none"
                  />
                </div>
              </div>
            </div>

            {/* Step 4: Compassionate Friend Perspective */}
            <div className="space-y-2 pt-2 border-t border-zinc-200 dark:border-zinc-800">
              <div className="flex items-center justify-between">
                <label className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                  <span className="w-5 h-5 rounded-full bg-indigo-600 text-white flex items-center justify-center text-xs">
                    4
                  </span>
                  Góc nhìn trắc ẩn: Nếu bạn thân gặp chuyện này, bạn sẽ khuyên gì?
                </label>
                <span className="text-[11px] text-zinc-500 italic">
                  Tách chủ thể để kích hoạt lòng trắc ẩn tự thân
                </span>
              </div>
              <textarea
                rows={2}
                value={cbtFriendAdvice}
                onChange={(e) => setCbtFriendAdvice(e.target.value)}
                placeholder="Ví dụ: Ai cũng có lúc bận việc, đừng tự trách mình nữa. Cậu đã làm rất tốt phần của mình rồi..."
                className="w-full p-3 rounded-xl border border-zinc-200 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-800 text-sm text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-indigo-500 transition resize-none"
              />
            </div>

            {/* Step 5: Micro-Action in 5 Minutes */}
            <div className="space-y-2 pt-2 border-t border-zinc-200 dark:border-zinc-800">
              <label className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                <span className="w-5 h-5 rounded-full bg-indigo-600 text-white flex items-center justify-center text-xs">
                  5
                </span>
                1 việc nhỏ làm được ngay trong 5 phút tới
              </label>
              <input
                type="text"
                value={cbtMicroAction}
                onChange={(e) => setCbtMicroAction(e.target.value)}
                placeholder="Ví dụ: Đứng dậy uống 1 cốc nước ấm, đi dạo 3 phút hoặc hít thở sâu 5 nhịp..."
                className="w-full p-3 rounded-xl border border-zinc-200 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-800 text-sm text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-indigo-500 transition"
              />
            </div>

            {/* Form Actions */}
            <div className="pt-4 border-t border-zinc-200 dark:border-zinc-800 flex flex-wrap items-center justify-between gap-3">
              <button
                type="button"
                onClick={handleResetCbtForm}
                className="min-h-[44px] px-4 py-2 rounded-xl border border-zinc-200 dark:border-zinc-700 text-xs font-medium text-zinc-600 dark:text-zinc-400 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition cursor-pointer"
              >
                Làm mới bảng
              </button>

              <button
                type="button"
                onClick={() => void handleSaveCbtToNotebook()}
                disabled={isSaving}
                className="min-h-[44px] px-6 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white font-medium text-sm transition cursor-pointer flex items-center gap-2 shadow-md shadow-indigo-600/15"
              >
                <BookOpen className="w-4 h-4" />
                <span>Xuất & Lưu bản gỡ rối vào Sổ tay</span>
              </button>
            </div>
          </div>
        </div>
      )}

      {/* =====================================================================
          TAB 5: PHÒNG THÍ NGHIỆM NHẬN THỨC (COGNITIVE LABS)
          ===================================================================== */}
      {activeTab === "cognitive-labs" && (
        <CognitiveLabsShowcase
          courseTitle={courseTitle}
          lessonTitle={lessonTitle}
          lessonMarkdown={lessonMarkdown}
          onSaveToNotebook={onSaveToNotebook}
          onCreateFlashcardQuick={onCreateFlashcardQuick}
          onRecordAttempt={recordPracticeAttempt}
          initialSubLab={resolvedPsychTarget?.psychCogSubTab ?? null}
        />
      )}

      {/* =====================================================================
          TAB 6..9: ACTIVE RECALL WRITE-IN MODES (BIAS SPOTTER, FEYNMAN, CBT EXERCISE, APPLY JOURNAL)
          ===================================================================== */}
      {activeTab === "bias-spotter" && (
        <BiasSpotterWriteInLab
          dbItems={normalizedPsychBank.biasSpotters}
          lessonId={lessonId}
          courseTitle={courseTitle}
          lessonTitle={lessonTitle}
          initialActivityKey={initialActivityKey}
          onSaveToNotebook={onSaveToNotebook}
          onCreateFlashcardQuick={onCreateFlashcardQuick}
          onRecordAttempt={recordPracticeAttempt}
        />
      )}

      {activeTab === "feynman" && (
        <FeynmanWriteInLab
          dbItems={normalizedPsychBank.feynmanPrompts}
          lessonId={lessonId}
          courseTitle={courseTitle}
          lessonTitle={lessonTitle}
          initialActivityKey={initialActivityKey}
          onSaveToNotebook={onSaveToNotebook}
          onCreateFlashcardQuick={onCreateFlashcardQuick}
          onRecordAttempt={recordPracticeAttempt}
        />
      )}

      {activeTab === "cbt-exercise" && (
        <CbtExerciseWriteInLab
          dbItems={normalizedPsychBank.cbtExercises}
          lessonId={lessonId}
          courseTitle={courseTitle}
          lessonTitle={lessonTitle}
          initialActivityKey={initialActivityKey}
          onSaveToNotebook={onSaveToNotebook}
          onCreateFlashcardQuick={onCreateFlashcardQuick}
          onRecordAttempt={recordPracticeAttempt}
        />
      )}

      {activeTab === "apply-journal" && (
        <ApplyJournalWriteInLab
          dbPrompts={normalizedPsychBank.applyJournals}
          lessonId={lessonId}
          courseTitle={courseTitle}
          lessonTitle={lessonTitle}
          initialActivityKey={initialActivityKey}
          onSaveToNotebook={onSaveToNotebook}
          onCreateFlashcardQuick={onCreateFlashcardQuick}
          onRecordAttempt={recordPracticeAttempt}
        />
      )}

      {/* =====================================================================
          TAB 4: CHECKLIST HÀNH ĐỘNG & TÌNH HUỐNG TỪ CHÍNH BÀI HỌC
         ===================================================================== */}
      {activeTab === "lesson-cases" && (
        <div className="space-y-5 animate-in fade-in duration-200">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <div className="rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-5 space-y-3">
              <div className="flex items-center justify-between gap-2">
                <h3 className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                  <CheckCircle2 className="w-4 h-4 text-emerald-500" />
                  <span>Checklist Hành Động Của Bài Học</span>
                </h3>
                <span className="text-xs font-semibold text-indigo-600 dark:text-indigo-400">
                  {Object.values(checkedActions).filter(Boolean).length}/{extractedLessonCases.actions.length} đã chọn
                </span>
              </div>
              <div className="space-y-2">
                {extractedLessonCases.actions.map((act, idx) => {
                  const isChecked = Boolean(checkedActions[idx]);
                  return (
                    <label
                      key={idx}
                      className={`flex items-start gap-2.5 p-3 rounded-xl border text-xs leading-relaxed cursor-pointer transition ${
                        isChecked
                          ? "border-emerald-300 dark:border-emerald-800 bg-emerald-50/50 dark:bg-emerald-950/25 text-zinc-900 dark:text-zinc-100"
                          : "border-zinc-200 dark:border-zinc-800 bg-zinc-50/60 dark:bg-zinc-800/40 text-zinc-700 dark:text-zinc-300"
                      }`}
                    >
                      <input
                        type="checkbox"
                        checked={isChecked}
                        onChange={() =>
                          setCheckedActions((prev) => ({ ...prev, [idx]: !prev[idx] }))
                        }
                        className="mt-0.5 accent-emerald-600"
                      />
                      <span className={isChecked ? "line-through opacity-80" : ""}>{act}</span>
                    </label>
                  );
                })}
              </div>
            </div>

            <div className="rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-5 space-y-3">
              <h3 className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                <HelpCircle className="w-4 h-4 text-indigo-500" />
                <span>Câu Hỏi &amp; Tình Huống Suy Ngẫm</span>
              </h3>
              <ul className="space-y-2">
                {extractedLessonCases.reflections.map((q, idx) => (
                  <li
                    key={idx}
                    className="p-3 rounded-xl border border-indigo-200/70 dark:border-indigo-900/50 bg-indigo-50/40 dark:bg-indigo-950/20 text-xs text-zinc-800 dark:text-zinc-200 leading-relaxed"
                  >
                    <span className="font-bold text-indigo-600 dark:text-indigo-400 mr-1.5">
                      #{idx + 1}
                    </span>
                    {q}
                  </li>
                ))}
              </ul>
            </div>
          </div>

          <div className="rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-5 space-y-3">
            <label className="text-sm font-bold text-zinc-900 dark:text-zinc-100 block">
              Kế hoạch áp dụng &amp; Tự phản chiếu cho bài &ldquo;{lessonTitle}&rdquo;:
            </label>
            <textarea
              rows={4}
              value={lessonReflectionNote}
              onChange={(e) => setLessonReflectionNote(e.target.value)}
              placeholder="Ghi lại câu trả lời của bạn cho các tình huống trên và 1 thay đổi cụ thể bạn sẽ áp dụng tuần này..."
              className="w-full p-3 rounded-xl border border-zinc-200 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-800 text-sm text-zinc-900 dark:text-zinc-100 focus:outline-none focus:border-indigo-500"
            />
            <div className="flex flex-wrap items-center justify-end gap-2">
              <button
                type="button"
                disabled={isSaving || (!lessonReflectionNote.trim() && Object.values(checkedActions).filter(Boolean).length === 0)}
                onClick={async () => {
                  setIsSaving(true);
                  try {
                    const doneList = extractedLessonCases.actions
                      .filter((_, i) => checkedActions[i])
                      .map((a) => `- [x] ${a}`)
                      .join("\n");
                    await onSaveToNotebook(
                      `### 🌱 Checklist & Phản chiếu (${lessonTitle})\n${doneList ? `${doneList}\n\n` : ""}${lessonReflectionNote.trim()}`
                    );
                    showToast("Đã lưu checklist & suy ngẫm vào Sổ tay!");
                  } finally {
                    setIsSaving(false);
                  }
                }}
                className="min-h-[42px] px-5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs font-semibold transition cursor-pointer inline-flex items-center gap-2"
              >
                <BookOpen className="w-4 h-4" />
                <span>Lưu Checklist &amp; Suy Ngẫm Vào Sổ Tay</span>
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default PsychologyLifeSkillsLab;
