import os
import re
import glob
from docx import Document
from docx.shared import Pt, RGBColor, Inches

def md_to_docx(md_path, docx_path):
    doc = Document()
    
    with open(md_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        
    in_code_block = False
    code_content = []
    
    for line in lines:
        line = line.strip('\n')
        
        if line.startswith('```'):
            if in_code_block:
                in_code_block = False
                p = doc.add_paragraph('\n'.join(code_content))
                # basic styling for code
                for run in p.runs:
                    run.font.name = 'Consolas'
                    run.font.size = Pt(10)
                    run.font.color.rgb = RGBColor(40, 40, 40)
                code_content = []
            else:
                in_code_block = True
            continue
            
        if in_code_block:
            code_content.append(line)
            continue
            
        if not line.strip():
            continue
            
        if line.startswith('#'):
            match = re.match(r'^(#+)\s+(.*)', line)
            if match:
                level = len(match.group(1))
                text = match.group(2)
                h = doc.add_heading(text, level=min(level, 9))
                for run in h.runs:
                    run.font.color.rgb = RGBColor(0, 51, 102)
            continue
            
        if line.startswith('- ') or line.startswith('* '):
            text = line[2:].strip()
            p = doc.add_paragraph(style='List Bullet')
        elif re.match(r'^\d+\.\s', line):
            text = line[line.find(' ')+1:].strip()
            p = doc.add_paragraph(style='List Number')
        else:
            text = line
            p = doc.add_paragraph()
            
        # parse inline bold
        parts = re.split(r'(\*\*.*?\*\*)', text)
        for part in parts:
            if part.startswith('**') and part.endswith('**'):
                run = p.add_run(part[2:-2])
                run.bold = True
            else:
                p.add_run(part)

    doc.save(docx_path)

if __name__ == '__main__':
    md_files = glob.glob("*.md")
    for md_file in md_files:
        docx_file = md_file.replace('.md', '.docx')
        md_to_docx(md_file, docx_file)
        print(f"Generated {docx_file}")
