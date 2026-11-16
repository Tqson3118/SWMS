import os
import re

def check_tests():
    print('--- TESTS ---')
    test_files = []
    for root, dirs, files in os.walk('D:/03_Study/Roadmap/IELTS'):
        for file in files:
            if file.startswith('test-') and file.endswith('.md'):
                test_files.append(os.path.join(root, file))
    
    for fpath in test_files:
        with open(fpath, 'r', encoding='utf-8') as f:
            content = f.read()
            match = re.search(r'> \*\*Điểm: ____/(\d+)\.\*\*(.*?)\n', content)
            if match:
                expected_total = int(match.group(1))
                breakdown = match.group(2)
                numbers = [int(n) for n in re.findall(r'\b\d+\b', breakdown)]
                if sum(numbers) != expected_total:
                    print(f'MISMATCH in {fpath}: Expected {expected_total}, Breakdown sums to {sum(numbers)} ({breakdown})')
            
def check_listening():
    print('--- LISTENING ---')
    listen_files = []
    for root, dirs, files in os.walk('D:/03_Study/Roadmap/IELTS'):
        if 'listening' in root:
            for file in files:
                if file.endswith('.md'):
                    listen_files.append(os.path.join(root, file))
    
    for fpath in listen_files:
        with open(fpath, 'r', encoding='utf-8') as f:
            content = f.read()
            # check if it has a youtube link or specific link
            if 'youtube.com' not in content and 'youtu.be' not in content and 'http' not in content:
                print(f'NO LINK in {fpath}')

check_tests()
check_listening()
