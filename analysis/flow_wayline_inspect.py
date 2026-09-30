import json,sys
from pathlib import Path
sys.stdout.reconfigure(encoding='utf-8')
G=Path(__file__).parent/'targets/wxcf1394487200e48f/43/generated'
def walk(n):
 print(n['name'])
 for c in n['components']:
  if c['type']=='Transform':continue
  print(c['type'],json.dumps(c,ensure_ascii=False))
 for x in n.get('children',[]):walk(x)
for p in (G/'unity-assets/prefabs').glob('LineRander*.json'):
 print(p.name);walk(json.loads(p.read_text(encoding='utf-8'))['root'])
