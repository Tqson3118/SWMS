import assert from "node:assert/strict";
import {
  buildPracticeLessonHref,
  extractQuickCheckQuestions,
  getPracticeActivityTypeLabel,
  resolvePracticeSelection,
} from "./practice-helpers.ts";

let passed = 0;

function runTest(name: string, fn: () => void) {
  try {
    fn();
    console.log(`PASS: ${name}`);
    passed++;
  } catch (err) {
    console.error(`FAIL: ${name}`);
    throw err;
  }
}

runTest("resolvePracticeSelection maps English, Coding, Finance, and Psychology activity types", () => {
  assert.deepEqual(resolvePracticeSelection("dictation", "key-1", "Tiếng Anh"), {
    labMode: "english",
    englishTab: "dictation",
  });
  assert.deepEqual(resolvePracticeSelection("pronunciation", "key-2", "IELTS"), {
    labMode: "english",
    englishTab: "speaking",
  });
  assert.deepEqual(resolvePracticeSelection("js_sandbox", "two-sum", "Lập trình"), {
    labMode: "code",
    codingTab: "js-sandbox",
  });
  assert.deepEqual(resolvePracticeSelection("bug_hunt", null, "C# .NET"), {
    labMode: "code",
    codingTab: "bug-hunter",
  });
  assert.deepEqual(resolvePracticeSelection("portfolio_sim", null, "Đầu tư"), {
    labMode: "finance",
    financeTab: "portfolio",
  });
  assert.deepEqual(resolvePracticeSelection("compound_calculator", null, "Tài chính"), {
    labMode: "finance",
    financeTab: "compound",
  });
  assert.deepEqual(resolvePracticeSelection("stroop_test", "psych-stroop-test", "Tâm lý học"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "cognitive-labs",
    psychCogSubTab: "stroop",
  });
  assert.deepEqual(resolvePracticeSelection("memory_span", null, "Tâm lý học"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "cognitive-labs",
    psychCogSubTab: "memory-span",
  });
  assert.deepEqual(resolvePracticeSelection("roleplay", "roleplay-1", "Tiếng Anh Giao Tiếp"), {
    labMode: "english",
    englishTab: "roleplay",
  });
  assert.deepEqual(resolvePracticeSelection("roleplay", "psych-roleplay-1", "Tâm lý học & Kỹ năng sống"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "roleplay",
  });
});

runTest("buildPracticeLessonHref encodes query params cleanly and falls back safely", () => {
  assert.equal(
    buildPracticeLessonHref("course-1", "lesson-2", "dictation", "act key"),
    "/app/practice/course-1/lesson-2?type=dictation&key=act+key"
  );
  assert.equal(buildPracticeLessonHref("course-1", null, "dictation", "k"), "/app/practice");
});

runTest("getPracticeActivityTypeLabel returns natural Vietnamese labels", () => {
  assert.equal(getPracticeActivityTypeLabel("js_sandbox"), "Web Worker Sandbox");
  assert.equal(getPracticeActivityTypeLabel("stroop_test"), "Thí nghiệm Stroop");
  assert.equal(getPracticeActivityTypeLabel("predict_output"), "Đoán Output JS");
  assert.equal(getPracticeActivityTypeLabel("bug_fix"), "Sửa Bug Write-in");
  assert.equal(getPracticeActivityTypeLabel("hand_calc"), "Tự tính tay tài chính");
  assert.equal(getPracticeActivityTypeLabel("decision_journal"), "Nhật ký quyết định");
  assert.equal(getPracticeActivityTypeLabel("bias_spotter"), "Nhận diện Thiên kiến");
  assert.equal(getPracticeActivityTypeLabel("feynman"), "Feynman Tự định nghĩa");
  assert.equal(getPracticeActivityTypeLabel("cbt_exercise"), "Bài tập CBT tình huống");
  assert.equal(getPracticeActivityTypeLabel("apply_journal"), "Nhật ký áp dụng");
});

runTest("resolvePracticeSelection maps all new s3a Write-in activity types accurately", () => {
  assert.deepEqual(resolvePracticeSelection("predict_output", "pred-1", "Lập trình JS"), {
    labMode: "code",
    codingTab: "predict-output",
  });
  assert.deepEqual(resolvePracticeSelection("bug_fix", "bug-1", "Lập trình JS"), {
    labMode: "code",
    codingTab: "bug-hunter",
  });
  assert.deepEqual(resolvePracticeSelection("hand_calc", "calc-1", "Đầu tư"), {
    labMode: "finance",
    financeTab: "hand-calc",
  });
  assert.deepEqual(resolvePracticeSelection("decision_journal", "dj-1", "Đầu tư"), {
    labMode: "finance",
    financeTab: "decision-journal",
  });
  assert.deepEqual(resolvePracticeSelection("bias_spotter", "bs-1", "Tâm lý học"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "bias-spotter",
  });
  assert.deepEqual(resolvePracticeSelection("feynman", "feyn-1", "Tâm lý học Ứng dụng"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "feynman",
  });
  assert.deepEqual(resolvePracticeSelection("cbt_exercise", "cbt-1", "Tâm lý học"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "cbt-exercise",
  });
  assert.deepEqual(resolvePracticeSelection("apply_journal", "app-1", "Tâm lý học"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "apply-journal",
  });
});

runTest("extractQuickCheckQuestions extracts up to 2 grounded multiple-choice questions", () => {
  const md = `
# Bộ nhớ ảo & TLB
- **TLB (Translation Lookaside Buffer)**: Bộ nhớ đệm phần cứng tốc độ cao lưu bản đồ trang ảo sang khung trang vật lý gần nhất.
- **Page Fault**: Ngoại lệ do MMU kích hoạt khi tiến trình truy cập trang chưa được nạp vào RAM vật lý.
- **Working Set**: Tập hợp các trang bộ nhớ mà tiến trình đang sử dụng tích cực trong một khoảng thời gian.
`;
  const qs = extractQuickCheckQuestions(md, "Bài 3.2: Bộ nhớ ảo");
  assert.equal(qs.length, 2);
  for (const q of qs) {
    assert.equal(q.options.length, 4);
    assert.ok(q.options.some((o) => o.key === q.correctKey));
    assert.ok(q.question.includes("Bài 3.2: Bộ nhớ ảo"));
  }
  assert.deepEqual(extractQuickCheckQuestions("", "Empty"), []);
});

runTest("resolvePracticeSelection & extractQuickCheckQuestions work accurately for Japanese N5 and Chinese HSK1", () => {
  // Even when lesson title contains "Giao tiếp", Japanese N5 & Chinese HSK1 roleplay must route to language lab ("english")
  assert.deepEqual(
    resolvePracticeSelection(
      "roleplay",
      "ja-l32-rp-1",
      "Tiếng Nhật Sơ Cấp (N5)",
      "Bài 3.2: Di chuyển & Giao tiếp Đời sống N5"
    ),
    { labMode: "english", englishTab: "roleplay" }
  );
  assert.deepEqual(
    resolvePracticeSelection(
      "roleplay",
      "zh-l11-rp-1",
      "Tiếng Trung Sơ Cấp (HSK1)",
      "Bài 1.1: Hệ thống Pinyin, 4 Thanh điệu & Chào hỏi"
    ),
    { labMode: "english", englishTab: "roleplay" }
  );
  assert.deepEqual(
    resolvePracticeSelection("translate", "ja-l11-tr-1", "Tiếng Nhật Sơ Cấp (N5)"),
    { labMode: "english", englishTab: "translate" }
  );

  const jaMd = `
# Bài 1.1: Hiragana nền tảng
- **Hiragana (ひらがな)**: Bảng chữ mềm cơ bản của tiếng Nhật dùng để viết từ thuần Nhật và trợ từ.
- **Trợ từ は (đọc là wa)**: Đánh dấu chủ đề của câu, dù viết bằng chữ は nhưng luôn đọc là wa.
- **Âm ngắt っ (Sokuon)**: Biểu thị nhịp dừng nén hơi đúng 1 phách trước phụ âm kế tiếp.
`;
  const jaQs = extractQuickCheckQuestions(jaMd, "Bài 1.1: Hiragana nền tảng");
  assert.equal(jaQs.length, 2);
  assert.ok(jaQs.every((q) => q.options.length === 4));

  const zhMd = `
# Bài 1.1: Pinyin & 4 Thanh điệu
- **Pinyin (拼音)**: Hệ thống ký âm Latinh chuẩn của tiếng Trung phổ thông gồm Thanh mẫu, Vận mẫu và 4 thanh điệu.
- **Trợ từ nghi vấn 吗 (ma)**: Đặt cuối câu trần thuật để chuyển ngay thành câu hỏi Có/Không.
- **Phó từ mức độ 很 (hěn)**: Đứng trước tính từ làm vị ngữ để cân bằng nhịp câu trong tiếng Trung.
`;
  const zhQs = extractQuickCheckQuestions(zhMd, "Bài 1.1: Pinyin & 4 Thanh điệu");
  assert.equal(zhQs.length, 2);
  assert.ok(zhQs.every((q) => q.options.length === 4));
});

runTest("extractQuickCheckQuestions ignores structural lesson headings and strips list/bullet prefixes", () => {
  const structureMd = `
# Bài 2: Giao tiếp văn phòng
- **1. Mục tiêu bài học**: Nắm vững các mẫu câu chào hỏi và hẹn gặp đối tác chuyên nghiệp.
- **2. Đoạn hội thoại mẫu**: Lắng nghe và thực hành theo ngữ cảnh thực tế tại cuộc họp.
- **Từ vựng**: Tổng hợp các từ mới liên quan đến đàm phán hợp đồng.
- **Ngữ pháp**: Các thì cơ bản và câu điều kiện loại 1.
- **Lỗi phát âm**: Không nuốt âm đuôi và phát âm chuẩn phụ âm cuối.
- **Ví dụ**: Minh họa trường hợp gọi điện hẹn lịch họp tuần tới.
- **Fruitful Collaboration**: Sự cộng tác mang lại nhiều kết quả tích cực và giá trị chung cho cả hai bên.
- **Follow-up Meeting**: Cuộc họp tiếp nối nhằm rà soát tiến độ và tháo gỡ các vướng mắc phát sinh.
`;
  const qs = extractQuickCheckQuestions(structureMd, "Bài 2: Giao tiếp văn phòng");
  assert.equal(qs.length, 2);
  assert.ok(qs[0].question.includes("Fruitful Collaboration"));
  assert.ok(qs[1].question.includes("Follow-up Meeting"));
  assert.ok(!qs.some((q) => q.question.includes("Mục tiêu bài học")));
  assert.ok(!qs.some((q) => q.question.includes("Đoạn hội thoại mẫu")));
  assert.ok(!qs.some((q) => q.question.includes("Từ vựng")));
});

console.log(`\n✅ All ${passed} practice-helpers unit tests passed!`);
