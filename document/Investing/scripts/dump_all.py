import sys
import os
import re

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

base = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "assessments"))
q_dir = os.path.join(base, "quizzes")
a_dir = os.path.join(base, "answer-keys")

for w in range(1, 19):
    q_file = os.path.join(q_dir, f"week-{w:02d}-quiz.md")
    a_file = os.path.join(a_dir, f"week-{w:02d}-answers.md")
    with open(q_file, "r", encoding="utf-8") as f:
        q_text = f.read()
    with open(a_file, "r", encoding="utf-8") as f:
        a_text = f.read()

    q_blocks = re.split(r"###\s*(Q\d+-\d+)", q_text)[1:]
    quiz_data = {}
    for i in range(0, len(q_blocks), 2):
        qid = q_blocks[i]
        qbody = q_blocks[i+1]
        lines = qbody.strip().splitlines()
        header = lines[0]
        opts = dict(re.findall(r"^([ABCD])\.\s+(.+)$", qbody, re.MULTILINE))
        quiz_data[qid] = {"header": header, "options": opts}

    ans_data = {}
    for line in a_text.splitlines():
        m = re.match(r"^\|\s*(Q\d+-\d+)\s*\|\s*\**([ABCD])\**\s*\|\s*(\[[^\]]+\])\s*\|\s*(.+)\s*\|$", line)
        if m:
            qid, ans, lo, exp = m.groups()
            ans_data[qid] = {"ans": ans, "lo": lo, "exp": exp}

    print(f"==================== WEEK {w:02d} ====================")
    for qid in sorted(quiz_data.keys()):
        q = quiz_data[qid]
        a = ans_data.get(qid, {})
        ans = a.get("ans", "?")
        exp = a.get("exp", "?")
        print(f"[{qid}] Header: {q['header']}")
        print(f"  Ans: {ans}")
        for ltr in ["A", "B", "C", "D"]:
            marker = " (*) " if ltr == ans else "     "
            print(f"  {marker}{ltr}. {q['options'].get(ltr, '')}")
        print(f"  Exp: {exp}")
        print()
