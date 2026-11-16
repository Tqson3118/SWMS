import os
import sys

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

BASE_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
QUIZZES_DIR = os.path.join(BASE_DIR, "assessments", "quizzes")
ANSWERS_DIR = os.path.join(BASE_DIR, "assessments", "answer-keys")


def build_week_assessment(week_num, title, module_num, lesson_slug, questions, review_tip=None):
    if len(questions) != 8:
        raise ValueError(f"Week {week_num} must have exactly 8 questions, got {len(questions)}")

    # Check distribution
    ans_list = [q["ans"] for q in questions]
    from collections import Counter
    c = Counter(ans_list)
    expected = {"A": 2, "B": 2, "C": 2, "D": 2}
    if dict(c) != expected:
        raise ValueError(f"Week {week_num} answer distribution unbalanced: {dict(c)} != {expected}")

    # Build Quiz Markdown
    quiz_lines = [
        "> [!CAUTION]",
        "> Nội dung chỉ mang tính giáo dục, KHÔNG phải lời khuyên đầu tư.",
        "> Mọi quyết định đầu tư là trách nhiệm cá nhân của bạn.",
        "> Hãy tham khảo chuyên gia tài chính phù hợp trước khi đầu tư.",
        "",
        f"# Quiz W{week_num:02d} — {title}",
        "",
        "**Hướng dẫn:** Chọn một đáp án đúng nhất cho mỗi câu hỏi. Không xem trước bảng giải thích đáp án.",
        ""
    ]

    for q in questions:
        qid = q["qid"]
        lo = q["lo"]
        qtext = q["question"]
        quiz_lines.append(f"### {qid} — [{lo}] {qtext}")
        quiz_lines.append(f"A. {q['options']['A']}")
        quiz_lines.append(f"B. {q['options']['B']}")
        quiz_lines.append(f"C. {q['options']['C']}")
        quiz_lines.append(f"D. {q['options']['D']}")
        quiz_lines.append("")

    quiz_content = "\n".join(quiz_lines).strip() + "\n"
    quiz_file = os.path.join(QUIZZES_DIR, f"week-{week_num:02d}-quiz.md")
    with open(quiz_file, "w", encoding="utf-8") as f:
        f.write(quiz_content)

    # Build Answer Key Markdown
    if review_tip is None:
        review_tip = "Nếu làm sai, hãy quay lại mục Core Concepts và Common Mistakes trong bài học để xem xét các cơ chế tự vệ tâm lý, phân tích dòng tiền và thiết lập quy tắc kỷ luật cho bản thân."

    ans_lines = [
        "> [!CAUTION]",
        "> Nội dung chỉ mang tính giáo dục, KHÔNG phải lời khuyên đầu tư.",
        "> Mọi quyết định đầu tư là trách nhiệm cá nhân của bạn.",
        "> Hãy tham khảo chuyên gia tài chính phù hợp trước khi đầu tư.",
        "",
        f"# Đáp án & giải thích W{week_num:02d} — {title}",
        "",
        f"**Bài học:** [Mở lesson tương ứng](../../notes/module-{module_num}/{lesson_slug}.md)",
        "",
        "| Mã câu | Đáp án | LO | Giải thích đúng và phân tích phương án nhiễu |",
        "|---|---:|---|---|"
    ]

    for q in questions:
        qid = q["qid"]
        lo = q["lo"]
        ans = q["ans"]
        explanation = q["explanation"].replace("\n", " ").strip()
        ans_lines.append(f"| {qid} | **{ans}** | [{lo}] | {explanation} |")

    ans_lines.append("")
    ans_lines.append(f"**Cách học lại:** {review_tip}")
    ans_lines.append("")

    ans_content = "\n".join(ans_lines)
    ans_file = os.path.join(ANSWERS_DIR, f"week-{week_num:02d}-answers.md")
    with open(ans_file, "w", encoding="utf-8") as f:
        f.write(ans_content)

    print(f"✅ Generated W{week_num:02d}: {title} (Answers: {ans_list})")
