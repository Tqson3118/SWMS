import os
import re
import sys

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

BASE_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
QUIZZES_DIR = os.path.join(BASE_DIR, "assessments", "quizzes")
ANSWERS_DIR = os.path.join(BASE_DIR, "assessments", "answer-keys")
NOTES_DIR = os.path.join(BASE_DIR, "notes")

def run_deep_audit():
    print("=== DEEP AUDIT OF ALL 18 WEEKS ===")
    
    total_issues = []
    
    for w in range(1, 19):
        week_str = f"W{w:02d}"
        q_file = os.path.join(QUIZZES_DIR, f"week-{w:02d}-quiz.md")
        a_file = os.path.join(ANSWERS_DIR, f"week-{w:02d}-answers.md")
        
        if not os.path.exists(q_file) or not os.path.exists(a_file):
            total_issues.append(f"{week_str}: Files missing")
            continue
            
        with open(q_file, "r", encoding="utf-8") as f:
            q_text = f.read()
        with open(a_file, "r", encoding="utf-8") as f:
            a_text = f.read()
            
        # Parse lesson link and LOs in lesson
        m_link = re.search(r"\*\*Bài học:\*\*\s*\[Mở lesson tương ứng\]\(([^)]+)\)", a_text)
        if m_link:
            lesson_rel = m_link.group(1)
            lesson_path = os.path.normpath(os.path.join(ANSWERS_DIR, lesson_rel))
            if os.path.exists(lesson_path):
                with open(lesson_path, "r", encoding="utf-8") as lf:
                    lesson_text = lf.read()
                lesson_los = set(re.findall(r"\[LO-W\d+-\d+\]", lesson_text))
            else:
                total_issues.append(f"{week_str}: Lesson path does not exist: {lesson_path}")
                lesson_los = set()
        else:
            total_issues.append(f"{week_str}: Lesson link missing")
            lesson_los = set()

        # Parse questions
        q_blocks = re.split(r"###\s*(Q\d+-\d+)", q_text)[1:]
        quiz_data = {}
        for i in range(0, len(q_blocks), 2):
            qid = q_blocks[i]
            qbody = q_blocks[i+1]
            first_line = qbody.strip().splitlines()[0]
            lo_m = re.search(r"(\[LO-W\d+-\d+\])", first_line)
            lo = lo_m.group(1) if lo_m else ""
            q_title = first_line
            opts = dict(re.findall(r"^([ABCD])\.\s+(.+)$", qbody, re.MULTILINE))
            quiz_data[qid] = {
                "lo": lo,
                "title": q_title,
                "options": opts,
                "body": qbody
            }

        # Parse answers table
        ans_rows = {}
        for line in a_text.splitlines():
            m = re.match(r"^\|\s*(Q\d+-\d+)\s*\|\s*\**([ABCD])\**\s*\|\s*(\[[^\]]+\])\s*\|\s*(.+)\s*\|$", line)
            if m:
                qid, ans, lo, exp = m.groups()
                ans_rows[qid] = {
                    "ans": ans,
                    "lo": lo,
                    "explanation": exp.strip()
                }

        # Compare
        for qid, qinfo in quiz_data.items():
            opts = qinfo["options"]
            # Check 4 options exist
            for ltr in ["A", "B", "C", "D"]:
                if ltr not in opts:
                    total_issues.append(f"{week_str} {qid}: Missing option {ltr}")
            
            # Check options unique
            if len(set(opts.values())) != len(opts):
                total_issues.append(f"{week_str} {qid}: Duplicate option values: {opts}")

            # Check LO in lesson
            if lesson_los and qinfo["lo"] not in lesson_los:
                total_issues.append(f"{week_str} {qid}: LO {qinfo['lo']} not defined in lesson {lesson_rel}")

            if qid not in ans_rows:
                total_issues.append(f"{week_str} {qid}: Not in answer key")
                continue

            arow = ans_rows[qid]
            ans = arow["ans"]
            exp = arow["explanation"]

            # Check LO matches
            if qinfo["lo"] != arow["lo"]:
                total_issues.append(f"{week_str} {qid}: LO mismatch: quiz {qinfo['lo']} vs ans {arow['lo']}")

            # Check (X đúng)
            if f"({ans} đúng)" not in exp and f"({ans} là đáp án đúng)" not in exp:
                total_issues.append(f"{week_str} {qid}: Explanation doesn't contain ({ans} đúng)")

            # Check distractors mentioned in explanation
            # e.g., 'A nhầm...', 'B sai...', 'C là...', 'D chỉ...'
            # Find occurrences of 'A ...', 'B ...', 'C ...', 'D ...' where letter is at sentence or clause start
            distractor_patterns = re.findall(r'(?:[.;,]|^)\s*([ABCD])\s+([^.;]+)', exp)
            for d_ltr, d_desc in distractor_patterns:
                opt_txt = opts.get(d_ltr, "")
                # Let's record if there's any mismatch or suspicious reference
                # Specifically if d_ltr is the answer but described as a mistake:
                if d_ltr == ans and any(w in d_desc.lower() for w in ['sai', 'nhầm', 'bỏ qua', 'ngược', 'phi lý']):
                    total_issues.append(f"{week_str} {qid}: Ans {ans} described as error in: '{d_ltr} {d_desc}'")

    print(f"Total issues found: {len(total_issues)}")
    for iss in total_issues:
        print(" - ", iss)

if __name__ == "__main__":
    run_deep_audit()
