"""Reuse the bounded static decoder for HUD classes; write HUD evidence only."""
from pathlib import Path
source=Path('analysis/guide_disassemble.py').read_text(encoding='utf8')
start=source.index('classes=');end=source.index('functions=[]')
selection="""classes={'Proj_xqzdPlayUI','Proj_xqzdOverUI','Proj_xqzdPauseUI','SkillUI','SkillItem','SkillControl','TowerCanvas'}
type_ids={t[2] for t in ts if ms(t[0]) in classes}
tokens={md[j][6] for t in ts if t[3] in type_ids for j in range(t[9],t[9]+t[16])}
selected=[m for m in methods if m['class'] in classes or m['token'] in tokens or (m['class']=='Tower' and m['method'] in ('RefreshStarCanvas','Init'))]
"""
source=source[:start]+selection+source[end:]
source=source.replace('generated/guide-evidence.json','generated/hud-evidence.json')
exec(compile(source,'hud_static_decoder','exec'),{'__file__':str(Path('analysis/guide_disassemble.py').resolve())})

