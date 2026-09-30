"""Bounded original AudioControl disassembly for in-level settings behavior."""
from pathlib import Path
s=Path('analysis/combat_disassemble.py').read_text(encoding='utf8')
start=s.index('selected=');end=s.index('\nout=',start)
s=s[:start]+"selected=[m for m in methods if m['class']=='AudioControl']"+s[end:]
s=s.replace('generated/combat-disassembly','generated/pause-audio-disassembly')
exec(compile(s,'audio static audit','exec'),{'__file__':str(Path('analysis/combat_disassemble.py').resolve())})
