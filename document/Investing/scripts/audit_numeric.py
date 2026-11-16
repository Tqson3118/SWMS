import re
import os
import sys

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

    # Find numeric questions
    q_blocks = re.split(r"###\s*(Q\d+-\d+)", q_text)[1:]
    quiz_data = {}
    for i in range(0, len(q_blocks), 2):
        qid = q_blocks[i]
        qbody = q_blocks[i+1]
        opts = dict(re.findall(r"^([ABCD])\.\s+(.+)$", qbody, re.MULTILINE))
        quiz_data[qid] = {"body": qbody.strip().splitlines()[0], "opts": opts}

    for line in a_text.splitlines():
        m = re.match(r"^\|\s*(Q\d+-\d+)\s*\|\s*\**([ABCD])\**\s*\|\s*(\[[^\]]+\])\s*\|\s*(.+)\s*\|$", line)
        if m:
            qid, ans, lo, exp = m.groups()
            opts = quiz_data[qid]["opts"]
            ans_text = opts[ans]
            # If the option is purely a numeric / short calculation answer (e.g. <= 30 chars and contains numbers)
            if len(ans_text) < 30 and re.search(r"\d+", ans_text):
                nums_in_ans = re.findall(r"\d+(?:[.,]\d+)?", ans_text)
                for n in nums_in_ans:
                    if n not in exp and n.replace(",", ".") not in exp and n.replace(".", ",") not in exp:
                        print(f"[W{w:02d}][{qid}] Number '{n}' in option '{ans_text}' not found in explanation: '{exp}'")
