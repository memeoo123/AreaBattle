"""Bounded static presentation function extraction."""
from pathlib import Path
s=Path('analysis/guide_disassemble.py').read_text(encoding='utf8');a=s.index('classes=');b=s.index('functions=[]')
s=s[:a]+'''classes={'LineRendererEntity','WayLine','WayLineCircle','WayLineControl'}
type_ids={t[2] for t in ts if ms(t[0]) in classes}
tokens={md[j][6] for t in ts if t[3] in type_ids for j in range(t[9],t[9]+t[16])}
selected=[m for m in methods if m['class'] in classes or m['token'] in tokens]
'''+s[b:]
s=s.replace('generated/guide-evidence.json','generated/wayline-presentation-evidence.json')
exec(compile(s,'presentation_static_decoder','exec'),{'__file__':str(Path('analysis/guide_disassemble.py').resolve())})

