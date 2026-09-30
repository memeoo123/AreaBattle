from pathlib import Path
import json
p=Path('UnityProject/ProjectSettings/TagManager.asset');s=p.read_text(encoding='utf-8-sig');a=s.index('  layers:\n');z=s.index('  m_SortingLayers:',a);old=s[a:z].splitlines()[1:];src=json.loads(Path('analysis/targets/wxcf1394487200e48f/43/generated/gesture-visuals/original-TagManager.json').read_text(encoding='utf8'))['layers'];assert len(old)==len(src)==32
for i,(line,name) in enumerate(zip(old,src)):
 held=line[4:].strip();assert held in ['',name],(i,held,name)
s=s[:a]+'  layers:\n'+''.join('  - '+name+'\n' for name in src)+s[z:];p.write_text(s,encoding='utf8')
