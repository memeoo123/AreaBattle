import json,sys
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
ROOT=Path('analysis/targets/wxcf1394487200e48f/43')
r=json.loads((ROOT/'generated/hud-evidence.json').read_text(encoding='utf8'))
for p in r['prefabs']:
 if sys.argv[1]=='bindings':
  print(p['assetPath']);print(json.dumps(p['outletBindings'],ensure_ascii=False));continue
 for n in p['nodes']:
  if not any(k in n['path'] for k in sys.argv[1:]):continue
  print(n['path'],json.dumps(n['transform'],ensure_ascii=False))
  for c in n['components']:
   if c['class'] not in ('UIOutlet','Image','Button','CanvasRenderer'):
    print(c['class'],json.dumps(c['data'],ensure_ascii=False),json.dumps(c['references'],ensure_ascii=False))
