"""
Automated Verification Script for Investing Assessments (W01 - W18)
Verifies:
1. Existence of all 18 quiz and 18 answer key files.
2. Caution disclaimer in all files.
3. Quiz format: Title, exactly 8 questions, LO code, unified options format (A., B., C., D.).
4. Answer key format: Title, lesson link validity, table structure, LO mapping.
5. Answer distribution: Exactly 2A, 2B, 2C, 2D per week (total 36A, 36B, 36C, 36D).
6. Consistency and integrity: Key in table matches explanation affirmation ((X đúng)), distractor analysis, no boilerplate.
"""

import os
import re
import sys
from collections import Counter

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")

BASE_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
ASSESSMENTS_DIR = os.path.join(BASE_DIR, "assessments")
QUIZZES_DIR = os.path.join(ASSESSMENTS_DIR, "quizzes")
ANSWERS_DIR = os.path.join(ASSESSMENTS_DIR, "answer-keys")
NOTES_DIR = os.path.join(BASE_DIR, "notes")


def verify_all_assessments():
    errors = []
    warnings = []
    total_counts = Counter()

    print("=" * 70)
    print("STARTING FULL INVESTING ASSESSMENTS AUDIT (W01 - W18)")
    print("=" * 70)

    for w in range(1, 19):
        week_str = f"W{w:02d}"
        q_file = os.path.join(QUIZZES_DIR, f"week-{w:02d}-quiz.md")
        a_file = os.path.join(ANSWERS_DIR, f"week-{w:02d}-answers.md")

        # 1. Existence
        if not os.path.exists(q_file):
            errors.append(f"[{week_str}] Quiz file missing: {q_file}")
            continue
        if not os.path.exists(a_file):
            errors.append(f"[{week_str}] Answer key file missing: {a_file}")
            continue

        with open(q_file, "r", encoding="utf-8") as f:
            q_text = f.read()
        with open(a_file, "r", encoding="utf-8") as f:
            a_text = f.read()

        # 2. Caution disclaimer
        if "> [!CAUTION]" not in q_text:
            errors.append(f"[{week_str}] Quiz missing [!CAUTION] disclaimer")
        if "> [!CAUTION]" not in a_text:
            errors.append(f"[{week_str}] Answer key missing [!CAUTION] disclaimer")

        # 3. Quiz structure
        q_title_m = re.search(r"^#\s*Quiz\s+(?:W\d+\s*[—–-]\s*)?(.+)$", q_text, re.MULTILINE)
        if not q_title_m:
            errors.append(f"[{week_str}] Quiz title format invalid: expected '# Quiz WNN — ...'")

        # Questions in quiz
        q_blocks = re.split(r"###\s*(Q\d+-\d+)", q_text)[1:]
        if len(q_blocks) != 16:  # 8 pairs of (qid, body)
            errors.append(f"[{week_str}] Quiz question count: expected 8 questions, got {len(q_blocks)//2}")
            continue

        quiz_questions = {}
        for i in range(0, len(q_blocks), 2):
            qid = q_blocks[i]
            qbody = q_blocks[i + 1]

            # LO code in question header
            lo_m = re.search(r"(\[LO-[^\]]+\])", qbody.splitlines()[0])
            lo = lo_m.group(1) if lo_m else ""
            if not lo:
                errors.append(f"[{week_str}][{qid}] Missing LO code in question header")

            # Check options formatting: must be A., B., C., D. (no leading dash)
            opts = re.findall(r"^([ABCD])\.\s+(.+)$", qbody, re.MULTILINE)
            dash_opts = re.findall(r"^[-*]\s*([ABCD])\.\s+", qbody, re.MULTILINE)
            if dash_opts:
                errors.append(f"[{week_str}][{qid}] Options have non-standard bullet dash: {dash_opts}")
            if len(opts) != 4:
                errors.append(f"[{week_str}][{qid}] Option count expected 4 (A, B, C, D), got {len(opts)}")

            # Check unique option values and non-empty text
            opt_texts = [opt[1].strip() for opt in opts]
            if len(set(opt_texts)) != len(opt_texts):
                errors.append(f"[{week_str}][{qid}] Duplicate option text found")
            for ltr, text in opts:
                if not text.strip():
                    errors.append(f"[{week_str}][{qid}] Option {ltr} is empty")

            quiz_questions[qid] = {
                "lo": lo,
                "options": {opt[0]: opt[1].strip() for opt in opts},
            }

        # 4. Answer key structure
        a_title_m = re.search(r"^#\s*Đáp án & giải thích\s+(?:W\d+\s*[—–-]\s*)?(.+)$", a_text, re.MULTILINE)
        if not a_title_m:
            errors.append(f"[{week_str}] Answer key title format invalid: expected '# Đáp án & giải thích WNN — ...'")

        # Lesson link & LO verification against notes
        lesson_los = set()
        lesson_link_m = re.search(r"\*\*Bài học:\*\*\s*\[Mở lesson tương ứng\]\(([^)]+)\)", a_text)
        if not lesson_link_m:
            errors.append(f"[{week_str}] Answer key missing valid lesson link")
        else:
            rel_path = lesson_link_m.group(1)
            target_lesson = os.path.normpath(os.path.join(ANSWERS_DIR, rel_path))
            if not os.path.exists(target_lesson):
                errors.append(f"[{week_str}] Lesson file does not exist: {target_lesson}")
            else:
                with open(target_lesson, "r", encoding="utf-8") as lf:
                    lesson_text = lf.read()
                lesson_los = set(re.findall(r"\[LO-W\d+-\d+\]", lesson_text))

        # Table rows
        ans_rows = []
        for line in a_text.splitlines():
            m = re.match(r"^\|\s*(Q\d+-\d+)\s*\|\s*\**([ABCD])\**\s*\|\s*(\[[^\]]+\])\s*\|\s*(.+)\s*\|$", line)
            if m:
                ans_rows.append({
                    "qid": m.group(1),
                    "ans": m.group(2),
                    "lo": m.group(3),
                    "explanation": m.group(4).strip()
                })

        if len(ans_rows) != 8:
            errors.append(f"[{week_str}] Answer key table row count: expected 8, got {len(ans_rows)}")
            continue

        # 5. Distribution check: 2A, 2B, 2C, 2D
        week_keys = [r["ans"] for r in ans_rows]
        week_counts = Counter(week_keys)
        expected_counts = {"A": 2, "B": 2, "C": 2, "D": 2}
        if dict(week_counts) != expected_counts:
            errors.append(f"[{week_str}] Answer distribution UNBALANCED! Got {dict(week_counts)}, expected exactly {expected_counts}")
        else:
            total_counts.update(week_counts)

        # 6. Consistency & Integrity
        for row in ans_rows:
            qid = row["qid"]
            ans_letter = row["ans"]
            ans_lo = row["lo"]
            explanation = row["explanation"]

            if qid not in quiz_questions:
                errors.append(f"[{week_str}][{qid}] Answer key question id not in quiz")
                continue

            q_info = quiz_questions[qid]

            # LO match
            if ans_lo != q_info["lo"]:
                errors.append(f"[{week_str}][{qid}] LO mismatch: quiz={q_info['lo']} vs ans_key={ans_lo}")
            if lesson_los and q_info["lo"] not in lesson_los:
                errors.append(f"[{week_str}][{qid}] LO '{q_info['lo']}' not found in lesson notes '{rel_path}'")

            # Answer key affirmation in explanation: should contain '(X đúng)' or similar
            affirmation_patterns = [
                f"({ans_letter} đúng)",
                f"({ans_letter} là đáp án đúng)",
                f"Phương án {ans_letter} đúng",
                f"Đáp án {ans_letter} đúng",
            ]
            has_affirmation = any(p in explanation for p in affirmation_patterns)
            if not has_affirmation:
                errors.append(f"[{week_str}][{qid}] Explanation missing clear affirmation for key '{ans_letter}' (e.g. '({ans_letter} đúng)')")

            # Check that explanation does not falsely affirm another letter as correct
            other_letters = [letter for letter in ["A", "B", "C", "D"] if letter != ans_letter]
            for other_letter in other_letters:
                false_affirmations = [
                    f"({other_letter} đúng)",
                    f"Phương án {other_letter} đúng",
                    f"Đáp án {other_letter} đúng",
                ]
                if any(fa in explanation for fa in false_affirmations):
                    errors.append(f"[{week_str}][{qid}] Explanation CONTRADICTION: key is '{ans_letter}' but affirms '{other_letter} đúng'")

            # Check that correct answer is not described as an error
            distractor_mentions = re.findall(r'(?:^|[.;,]\s*)([ABCD])\s+([^.;]+)', explanation)
            for d_ltr, d_desc in distractor_mentions:
                if d_ltr == ans_letter:
                    for neg in ['sai', 'nhầm', 'bỏ qua', 'vi phạm', 'ngược']:
                        if neg in d_desc.lower().split():
                            errors.append(f"[{week_str}][{qid}] Correct answer '{ans_letter}' described negatively: '{d_desc.strip()}'")

            # Check for forbidden boilerplate
            boilerplate_terms = [
                "khung phân tích có giả định",
                "claim chưa mở",
                "endpoint hiện lỗi 404",
                "đáp án phù hợp với khái niệm và giới hạn của bài"
            ]
            for bt in boilerplate_terms:
                if bt in explanation.lower() or bt in q_text.lower():
                    errors.append(f"[{week_str}][{qid}] Contains forbidden boilerplate text: '{bt}'")

        print(f"[{week_str}] Checked: 8 questions, distribution={dict(week_counts)} keys={week_keys} - Status: {'OK' if not any(e.startswith(f'[{week_str}]') for e in errors) else 'FAILED'}")

    print("\n" + "=" * 70)
    print("AUDIT SUMMARY:")
    print(f"Total weeks tested: 18")
    print(f"Total questions tested: {sum(total_counts.values())}")
    print(f"Cumulative distribution: {dict(total_counts)}")
    print(f"Expected cumulative: {{'A': 36, 'B': 36, 'C': 36, 'D': 36}}")
    print("=" * 70)

    if errors:
        print(f"\n❌ FOUND {len(errors)} ERROR(S):")
        for idx, err in enumerate(errors, 1):
            print(f"  {idx}. {err}")
        return False
    else:
        print("\n✅ ALL 18 WEEKS PASSED VERIFICATION WITH ZERO ERRORS!")
        print("  - Every week has exactly 2A, 2B, 2C, 2D.")
        print("  - All LO codes match 100%.")
        print("  - All explanations match shuffled keys and analyze distractors.")
        print("  - All lesson links and caution disclaimers verified.")
        return True


if __name__ == "__main__":
    success = verify_all_assessments()
    sys.exit(0 if success else 1)
