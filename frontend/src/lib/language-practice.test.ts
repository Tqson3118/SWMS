import assert from "node:assert/strict";
import {
  evaluatePronunciationWords,
  detectVietnamesePronunciationTips,
  evaluateClozeAnswers,
  evaluateTranslationAnswer,
  evaluateGrammarFixAnswer,
  evaluateRoleplayOffline,
  normalizePracticeActivities,
  filterItemsByDifficulty,
  parseClozeTemplate,
  GRADING_LABELS,
  type ClozePracticeItem,
  type TranslatePracticeItem,
  type GrammarFixPracticeItem,
  type RoleplayPracticeItem,
  type RawPracticeActivity,
} from "./language-practice.ts";

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

// 1. Pronunciation word-by-word coloring & accuracy
runTest("Pronunciation: exact sentence returns 100% and all correct with no error tips", () => {
  const res = evaluatePronunciationWords(
    "Making a positive first impression helps build long-term trust.",
    "Making a positive first impression helps build long term trust",
    "en-US"
  );
  assert.equal(res.score, 100);
  assert.equal(res.wrongCount, 0);
  assert.equal(res.nearCount, 0);
  assert.equal(res.correctCount, res.words.length);
  assert.equal(res.tips.length, 0);
});

runTest("Pronunciation: detects correct, near (dropped ending / small typo), and wrong words", () => {
  const res = evaluatePronunciationWords(
    "I think the recent projects build strong relationships.",
    "I tink the recent project strong",
    "en-US"
  );
  const byClean = new Map(res.words.map((w) => [w.cleanTarget, w]));
  assert.equal(byClean.get("i")?.status, "correct");
  assert.equal(byClean.get("think")?.status, "near"); // tink -> think (dist 1)
  assert.equal(byClean.get("projects")?.status, "near"); // project -> projects (dropped -s)
  assert.equal(byClean.get("build")?.status, "wrong"); // missing
  assert.equal(byClean.get("relationships")?.status, "wrong"); // missing
  assert.ok(res.score > 20 && res.score < 90, `Unexpected score ${res.score}`);
});

runTest("Pronunciation: empty transcript returns 0 score and does not fabricate score", () => {
  const res = evaluatePronunciationWords("Hello world", "   ", "en-US");
  assert.equal(res.score, 0);
  assert.equal(res.correctCount, 0);
  assert.equal(res.wrongCount, 2);
});

// 2. Vietnamese pronunciation tips detection
runTest("Vietnamese Tips: detects /θ/-/ð/, ending sounds, -s/-ed, and confusable pairs", () => {
  const target = "I think the weather encouraged walked projects and ship relationships.";
  const evalRes = evaluatePronunciationWords(target, "I and", "en-US");
  const categories = new Set(evalRes.tips.map((t) => t.category));

  assert.ok(categories.has("th_sound"), "Expected th_sound tip");
  assert.ok(categories.has("ending_sounds"), "Expected ending_sounds tip");
  assert.ok(categories.has("s_es_ed_endings"), "Expected s_es_ed_endings tip");
  assert.ok(categories.has("confusable_pairs"), "Expected confusable_pairs tip");

  const thTip = evalRes.tips.find((t) => t.category === "th_sound")!;
  assert.ok(
    thTip.exampleWords.some((w) => w.toLowerCase() === "think" || w.toLowerCase() === "weather"),
    `Expected think/weather in ${thTip.exampleWords.join(",")}`
  );
  assert.ok(thTip.adviceVi.includes("đầu lưỡi"), "Expected Vietnamese advice for th sound");

  const sEdTip = evalRes.tips.find((t) => t.category === "s_es_ed_endings")!;
  assert.ok(
    sEdTip.exampleWords.some((w) => ["encouraged", "walked", "relationships", "projects"].includes(w.toLowerCase())),
    `Expected -s/-ed words in ${sEdTip.exampleWords.join(",")}`
  );
});

runTest("Pronunciation CJK prep: Japanese & Chinese character evaluation and tips", () => {
  const jaRes = evaluatePronunciationWords("おはようございます", "おはようござます", "ja-JP");
  assert.ok(jaRes.score >= 80 && jaRes.score < 100, `Expected high partial score for ja-JP, got ${jaRes.score}`);
  assert.equal(jaRes.tips.length, 1);
  assert.equal(jaRes.tips[0].category, "cjk_tone_pitch");

  const zhRes = evaluatePronunciationWords("你好，很高兴认识你", "你好很高兴认识你", "zh-CN");
  assert.equal(zhRes.score, 100);
  assert.equal(zhRes.tips.length, 0);

  const zhMiss = evaluatePronunciationWords("你好，很高兴认识你", "你好认识你", "zh-CN");
  assert.ok(zhMiss.score > 40 && zhMiss.score < 90);
  assert.equal(zhMiss.tips[0].category, "cjk_tone_pitch");
  assert.ok(detectVietnamesePronunciationTips(zhMiss.words, "你好，很高兴认识你", "zh-CN").length === 1);
});

// 3. Cloze evaluation
runTest("Cloze: parses both {{0}} and ___ templates and grades multi-blank with normalization", () => {
  const segA = parseClozeTemplate("I look {{0}} to {{1}} with you.", 2);
  assert.equal(segA.filter((s) => s.type === "blank").length, 2);
  const segB = parseClozeTemplate("I look ___ to ___ with you.", 2);
  assert.equal(segB.filter((s) => s.type === "blank").length, 2);

  const item: ClozePracticeItem = {
    id: "cloze-1",
    lang: "en-US",
    difficulty: "medium",
    title: "Test Cloze",
    promptVi: "Điền chỗ trống",
    template: "It is a {{0}} to meet you, and I {{1}} agree more.",
    blanks: [
      { acceptedAnswers: ["pleasure"] },
      { acceptedAnswers: ["couldn't", "could not"] },
    ],
    meaningVi: "Rất hân hạnh gặp bạn, tôi hoàn toàn đồng ý.",
  };

  const fullPass = evaluateClozeAnswers(item, ["  Pleasure! ", "could not"]);
  assert.equal(fullPass.score, 100);
  assert.equal(fullPass.isAllCorrect, true);
  assert.equal(fullPass.correctCount, 2);

  const halfPass = evaluateClozeAnswers(item, ["pleasure", "wrong"]);
  assert.equal(halfPass.score, 50);
  assert.equal(halfPass.isAllCorrect, false);
  assert.equal(halfPass.blanks[0].isCorrect, true);
  assert.equal(halfPass.blanks[1].isCorrect, false);
});

// 4. Translation evaluation
runTest("Translation: supports contractions equivalence and multiple acceptedAnswers", () => {
  const item: TranslatePracticeItem = {
    id: "tr-1",
    lang: "en-US",
    difficulty: "easy",
    title: "Dịch mẫu",
    sourceVi: "Tôi không nghĩ cuộc họp này sẽ kéo dài quá lâu.",
    hintKeywords: ["think", "meeting", "long"],
    acceptedAnswers: [
      "I do not think this meeting will take too long.",
      "I don't think the meeting will be too long.",
    ],
    sampleAnswer: "I don't think this meeting will take too long.",
  };

  // Contraction equivalence: "I'm" / "don't" <-> "do not"
  const exact1 = evaluateTranslationAnswer(item, "I'm? No wait: I don't think this meeting will take too long");
  assert.ok(exact1.score < 100); // has extra tokens

  const exact2 = evaluateTranslationAnswer(item, "  I don’t think this meeting will take too long! ");
  assert.equal(exact2.score, 100);
  assert.equal(exact2.isExactMatch, true);

  const altExact = evaluateTranslationAnswer(item, "I do not think the meeting will be too long.");
  assert.equal(altExact.score, 100);
  assert.equal(altExact.isExactMatch, true);

  const partial = evaluateTranslationAnswer(item, "I do not think meeting");
  assert.ok(partial.score > 20 && partial.score < 95);
  assert.ok(partial.missingTokens.includes("long"));
});

// 5. Grammar Fix evaluation
runTest("Grammar Fix: exact fix scores 100, unchanged error flagged, partial fix reports diff", () => {
  const item: GrammarFixPracticeItem = {
    id: "gf-1",
    lang: "en-US",
    difficulty: "medium",
    title: "Fix look forward to",
    incorrectSentence: "I look forward to meet you next week.",
    errorTypeHintVi: "look forward to + V-ing",
    correctedSentence: "I look forward to meeting you next week.",
    acceptedAnswers: ["I am looking forward to meeting you next week."],
    explanationVi: "Sau 'look forward to' cần dùng V-ing.",
  };

  const exact = evaluateGrammarFixAnswer(item, "I look forward to meeting you next week");
  assert.equal(exact.score, 100);
  assert.equal(exact.isExactMatch, true);
  assert.equal(exact.isUnchangedError, false);

  const alt = evaluateGrammarFixAnswer(item, "I'm looking forward to meeting you next week.");
  assert.equal(alt.score, 100);
  assert.equal(alt.isExactMatch, true);

  const unchanged = evaluateGrammarFixAnswer(item, "I look forward to meet you next week.");
  assert.equal(unchanged.isUnchangedError, true);
  assert.ok(unchanged.score <= 30);

  const partial = evaluateGrammarFixAnswer(item, "I look forward meeting you next week.");
  assert.equal(partial.isExactMatch, false);
  assert.ok(partial.missingTokens.includes("to"));
});

// 6. Roleplay Offline Fallback evaluation
runTest("Roleplay Offline: always sets keyword_offline mode, honest label, and deterministic score", () => {
  const item: RoleplayPracticeItem = {
    id: "rp-1",
    lang: "en-US",
    difficulty: "easy",
    title: "Small Talk Roleplay",
    scenarioVi: "Gặp đồng nghiệp mới ở hội thảo.",
    aiRole: "Alex",
    aiPrompt: "Hi! Is this seat taken?",
    aiPromptVi: "Chào bạn! Ghế này có ai ngồi chưa?",
    userGoalVi: "Mời ngồi và giới thiệu bản thân.",
    expectedKeywords: ["go ahead", "pleasure", "meet", "hanoi"],
    sampleResponses: ["No, please go ahead! It is a pleasure to meet you. I am based in Hanoi."],
    minWords: 8,
  };

  const empty = evaluateRoleplayOffline(item, "");
  assert.equal(empty.score, 0);
  assert.equal(empty.gradingMode, "keyword_offline");
  assert.equal(empty.usedLiveEndpoint, false);
  assert.equal(empty.gradingLabelVi, GRADING_LABELS.keyword_offline);

  const strong = evaluateRoleplayOffline(
    item,
    "No, please go ahead! It's a pleasure to meet you, I work as an engineer in Hanoi."
  );
  assert.equal(strong.gradingMode, "keyword_offline");
  assert.equal(strong.usedLiveEndpoint, false);
  assert.equal(strong.gradingLabelVi, "Chấm theo từ khóa (offline)");
  assert.ok(strong.score >= 85, `Expected >= 85, got ${strong.score}`);
  assert.equal(strong.matchedKeywords.length, 4);
  assert.equal(strong.missingKeywords.length, 0);

  const weak = evaluateRoleplayOffline(item, "Yes hello");
  assert.ok(weak.score < 50, `Expected < 50 for weak short answer, got ${weak.score}`);
  assert.ok(weak.missingKeywords.length >= 3);
});

// 7. Schema Normalization (Legacy + Extended + Merging multiple activities + Difficulty filter)
runTest("Schema Normalizer: merges legacy single ConfigJson and extended multi-item activities cleanly", () => {
  const rawActivities: RawPracticeActivity[] = [
    {
      id: "act-legacy-shadow",
      type: "shadowing",
      title: "Legacy Shadowing",
      orderIndex: 1,
      configJson: JSON.stringify({
        sentence: "It is a pleasure to meet you.",
        ipa: "/ɪt ɪz ə ˈpleʒə tuː miːt juː/",
        meaningVi: "Rất hân hạnh gặp bạn.",
        keyPhrases: ["pleasure to meet you"],
      }),
    },
    {
      id: "act-extended-shadow",
      type: "shadowing",
      title: "Extended Shadowing Pack",
      orderIndex: 5,
      configJson: JSON.stringify({
        lang: "en-GB",
        difficulty: "hard",
        items: [
          {
            id: "sh-easy",
            difficulty: "easy",
            sentence: "Thank you for joining us today.",
            meaningVi: "Cảm ơn bạn đã tham gia hôm nay.",
          },
          {
            id: "sh-hard",
            difficulty: "hard",
            sentence: "Cross-disciplinary collaboration accelerates innovation.",
            meaningVi: "Hợp tác liên ngành tăng tốc đổi mới.",
          },
        ],
      }),
    },
    {
      id: "act-wm-1",
      type: "word_map",
      title: "Word Map 1",
      orderIndex: 4,
      configJson: JSON.stringify({
        centralWord: "introduce",
        meaningVi: "Giới thiệu",
        collocations: [{ text: "introduce oneself", meaningVi: "Tự giới thiệu" }],
        phrasalVerbs: [],
        synonyms: [],
        contextExamples: [],
      }),
    },
    {
      id: "act-wm-2",
      type: "word_map",
      title: "Word Map 2",
      orderIndex: 6,
      configJson: JSON.stringify({
        lang: "en-US",
        difficulty: "hard",
        centralWord: "collaborate",
        meaningVi: "Hợp tác",
        collocations: [{ text: "collaborate closely", meaningVi: "Hợp tác chặt chẽ" }],
        phrasalVerbs: [],
        synonyms: [],
        contextExamples: [],
      }),
    },
  ];

  const bank = normalizePracticeActivities(rawActivities, "en-US");
  assert.equal(bank.shadowing.length, 3);
  assert.equal(bank.shadowing[0].difficulty, "medium"); // legacy default
  assert.equal(bank.shadowing[1].lang, "en-GB"); // inherited from activity config
  assert.equal(bank.shadowing[1].difficulty, "easy");
  assert.equal(bank.shadowing[2].difficulty, "hard");

  assert.equal(bank.wordMaps.length, 2);
  assert.equal(bank.wordMaps[0].centralWord, "introduce");
  assert.equal(bank.wordMaps[1].centralWord, "collaborate");

  const easyOnly = filterItemsByDifficulty(bank.shadowing, "easy");
  assert.equal(easyOnly.length, 1);
  assert.equal(easyOnly[0].id, "sh-easy");

  const allItems = filterItemsByDifficulty(bank.shadowing, "all");
  assert.equal(allItems.length, 3);
});

console.log(`\n✅ All ${passed} language-practice unit tests passed!`);
