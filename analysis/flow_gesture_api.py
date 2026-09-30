from pathlib import Path
s=Path('analysis/flow_line_api.py').read_text(encoding='utf8');a=s.index('targets=');b=s.index('\nfor im',a);s=s[:a]+'targets={1724,1112,1241,1812,5156,4382}'+s[b:];exec(compile(s,'gesture_api','exec'))
