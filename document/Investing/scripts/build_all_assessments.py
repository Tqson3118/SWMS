"""
Build and verify all Investing assessments (W01 - W18)
Executes:
1. build_module_1.py (W01 - W04)
2. build_module_2.py (W05 - W10)
3. build_module_3.py (W11 - W14)
(W15 - W18 are canonical and already standardized)
4. verify_assessments.py (Validates all 18 weeks)
"""

import os
import sys

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")

sys.path.insert(0, os.path.dirname(__file__))

from build_module_1 import build_w01, build_w02, build_w03, build_w04
from build_module_2 import build_w05, build_w06, build_w07, build_w08, build_w09, build_w10
from build_module_3 import build_w11, build_w12, build_w13, build_w14
from verify_assessments import verify_all_assessments


def main():
    print("=== BUILDING MODULE 1 (W01 - W04) ===")
    build_w01()
    build_w02()
    build_w03()
    build_w04()

    print("\n=== BUILDING MODULE 2 (W05 - W10) ===")
    build_w05()
    build_w06()
    build_w07()
    build_w08()
    build_w09()
    build_w10()

    print("\n=== BUILDING MODULE 3 (W11 - W14) ===")
    build_w11()
    build_w12()
    build_w13()
    build_w14()

    print("\n=== RUNNING FULL VERIFICATION SUITE (W01 - W18) ===")
    success = verify_all_assessments()
    if not success:
        sys.exit(1)
    print("\nAll assessments built and verified successfully!")


if __name__ == "__main__":
    main()
