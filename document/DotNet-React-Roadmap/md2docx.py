"""Build study documents from the reviewed Markdown files beside this script."""
from pathlib import Path
import re
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.opc.constants import RELATIONSHIP_TYPE as RT

ROOT = Path(__file__).resolve().parent
NAMES = {
 '1.md': '1_CSharp_Co_Ban.docx',
 '2.md': '2_Giai_Bai_Toan_Va_Debug.docx',
 '3.md': '3_CSharp_Cho_Ung_Dung_Nho.docx',
 '4.md': '4_Database_Va_SQL.docx',
 '5.md': '5_HTTP_Va_ASPNET_Core.docx',
 '6.md': '6_Nen_Tang_Frontend.docx',
 '7.md': '7_React_Va_Hoan_Thien.docx',
 '8.md': '8_CSharp_Chuyen_Sau.docx',
 '9.md': '9_Thiet_Ke_Code_Va_Patterns.docx',
 '10.md': '10_SQL_Va_EFCore_Sau.docx',
 '11.md': '11_API_Nhieu_Nguoi_Dung_Va_Bao_Mat.docx',
 '12.md': '12_React_TypeScript_Sau.docx',
 '13.md': '13_Kiem_Thu_Co_He_Thong.docx',
 '14.md': '14_Trien_Khai_Va_Van_Hanh.docx',
 'README.md': '00_Bat_Dau.docx',
}

def link(p, label, target):
    e=OxmlElement('w:hyperlink')
    if not target.startswith(('https://','http://')):
        target=NAMES.get(target,target)
    e.set(qn('r:id'), p.part.relate_to(target, RT.HYPERLINK, is_external=True))
    r=OxmlElement('w:r'); props=OxmlElement('w:rPr')
    color=OxmlElement('w:color'); color.set(qn('w:val'),'225588'); props.append(color)
    r.append(props); t=OxmlElement('w:t'); t.text=label; r.append(t); e.append(r)
    p._p.append(e)

def inline(p,s):
    tokens=re.split(r'(\[[^\]]+\]\([^\)]+\)|\*\*[^*]+\*\*|`[^`]+`)',s)
    for token in tokens:
        m=re.fullmatch(r'\[([^\]]+)\]\(([^\)]+)\)',token)
        if m: link(p,*m.groups())
        elif token.startswith('**') and token.endswith('**'):
            p.add_run(token[2:-2]).bold=True
        elif token.startswith('`') and token.endswith('`'):
            r=p.add_run(token[1:-1]); r.font.name='Consolas'; r.font.size=Pt(10)
        else: p.add_run(token)

def convert(source, destination):
    s=source.read_text(encoding='utf-8-sig')
    if '`@`@`' in s: raise ValueError('Malformed code fences: '+str(source))
    doc=Document(); sec=doc.sections[0]
    sec.page_width=Inches(8.5); sec.page_height=Inches(11)
    sec.top_margin=sec.bottom_margin=Inches(.7)
    sec.left_margin=sec.right_margin=Inches(.75)
    for name in ['Normal','Title','Heading 1','Heading 2','Heading 3','Heading 4']:
        st=doc.styles[name]; st.font.name='Calibri'; st.font.color.rgb=RGBColor(0,0,0)
    normal=doc.styles['Normal']; normal.font.size=Pt(11)
    normal.paragraph_format.space_after=Pt(6); normal.paragraph_format.line_spacing=1.08
    doc.styles['Title'].font.size=Pt(23)
    for name,size in [('Heading 1',17),('Heading 2',14),('Heading 3',12)]:
        doc.styles[name].font.size=Pt(size)
        doc.styles[name].paragraph_format.keep_with_next=True
    code=False
    for line in s.splitlines():
        if line.startswith('```'):
            code=not code
            continue
        if code:
            p=doc.add_paragraph()
            p.paragraph_format.space_after=Pt(0)
            p.paragraph_format.line_spacing=1
            p.paragraph_format.widow_control=False
            r=p.add_run(line or ' '); r.font.name='Consolas'; r.font.size=Pt(9)
            shade=OxmlElement('w:shd'); shade.set(qn('w:fill'),'F3F4F5'); p._p.get_or_add_pPr().append(shade)
            continue
        if not line.strip(): continue
        if re.fullmatch(r'\s*[-*_]{3,}\s*',line): continue
        heading=re.match(r'^(#{1,6})\s+(.*)',line)
        if heading:
            depth,title=heading.groups()
            title=title.replace('**','').replace('`','')
            p=doc.add_paragraph(style='Title' if len(depth)==1 else f'Heading {min(len(depth)-1,4)}')
            inline(p,title)
        elif line.startswith('- '):
            p=doc.add_paragraph(style='List Bullet'); inline(p,line[2:])
        else:
            p=doc.add_paragraph(); inline(p,line)
    if code: raise ValueError('Unclosed code fence: '+str(source))
    doc.core_properties.title=next(x.lstrip('# ') for x in s.splitlines() if x.startswith('#'))
    doc.core_properties.author=''
    doc.save(destination)
    return len(doc.paragraphs)

if __name__=='__main__':
    for md,name in NAMES.items():
        source=ROOT/md
        if source.exists():
            count=convert(source,ROOT/name)
            print(f'{name}: {count} paragraphs')
