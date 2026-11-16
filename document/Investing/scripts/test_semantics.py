import sys
import os
import re

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

sys.path.insert(0, os.path.dirname(__file__))

import build_module_1
import build_module_2
import build_module_3

captured_weeks = {}

def mock_build(week_num, title, module_num, lesson_slug, questions, review_tip=None):
    captured_weeks[week_num] = questions

build_module_1.build_week_assessment = mock_build
build_module_2.build_week_assessment = mock_build
build_module_3.build_week_assessment = mock_build

build_module_1.build_w01()
build_module_1.build_w02()
build_module_1.build_w03()
build_module_1.build_w04()

build_module_2.build_w05()
build_module_2.build_w06()
build_module_2.build_w07()
build_module_2.build_w08()
build_module_2.build_w09()
build_module_2.build_w10()

build_module_3.build_w11()
build_module_3.build_w12()
build_module_3.build_w13()
build_module_3.build_w14()

print(f"Captured {len(captured_weeks)} weeks of data.")

discrepancies = []

for w, qlist in sorted(captured_weeks.items()):
    for q in qlist:
        qid = q["qid"]
        ans = q["ans"]
        exp = q["explanation"]
        opts = q["options"]
        
        # Check every distractor mentioned individually:
        # e.g., 'A nhầm...', 'B là...', 'C chỉ...', 'D xem...'
        # Find single letter followed by word (not grouped like A, B, C)
        individual_matches = re.finditer(r'(?:^|[.;]\s*)([ABCD])\s+([a-zà-ỹA-ZÀ-Ỹ0-9\s]+?)(?:;|\.|\Z)', exp)
        for m in individual_matches:
            ltr = m.group(1)
            reason = m.group(2).strip()
            if ltr == ans:
                continue
            opt_text = opts.get(ltr, "")
            # Let's inspect what was said
            # If reason mentions a specific term, does opt_text or question relate to it?
            # E.g., if reason mentions 'lãi đơn', does opt_text mention 'lãi đơn' or formula with 'r*n'?
            # We can log any potential mismatch
            # print(f"[{qid}] {ltr}: {reason[:40]} | Opt: {opt_text[:40]}")

print("Individual checks done.")
