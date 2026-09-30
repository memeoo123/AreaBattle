import json
from pathlib import Path
source=Path('analysis/asset_focus.py').read_text(encoding='utf8').split('roots=[]')[0]
ctx={'__file__':str(Path('analysis/asset_focus.py').resolve())};exec(compile(source,'gesture_readonly','exec'),ctx)
R=ctx['ROOT'];O=R/'generated/gesture-visuals';O.mkdir(exist_ok=True)
for c in ctx['report']['containers']:
 if not c['assetPath'].endswith('.prefab') or not any(s in c['assetPath'].lower() for s in ['hdzd_eff_spheretrails','/linerandercut.prefab','/linearrow.prefab']):continue
 root=ctx['hierarchy'](ctx['objs'][c['object']]);deps=ctx['walk_refs']([c['object']]);p=O/(root['name']+'.hierarchy.json')
 p.write_text(json.dumps({'assetPath':c['assetPath'],'root':root,'dependencies':[ctx['objs'][k]for k in deps]},ensure_ascii=False,indent=2),encoding='utf8')
 print(c['assetPath'],root['name'],len(deps))
 def walk(n):
  print(n['name'],[(x['type'],(x.get('script')or{}).get('m_ClassName'))for x in n['components']])
  for x in n['children']:walk(x)
 walk(root)
