"""Read two proven dynamic Boss attachments from immutable baseline (no acquisition)."""
import json
from pathlib import Path
R=Path('analysis/targets/wxcf1394487200e48f/43');S=R/'generated/resource-snapshots/boss-entities-20260928'
source=Path('analysis/asset_focus.py').read_text(encoding='utf8').split('roots=[]')[0]
ctx={'__file__':str(Path('analysis/asset_focus.py').resolve())};exec(compile(source,'baseline_boss_auxiliary','exec'),ctx)
report,objs,hierarchy,walk_refs=[ctx[k] for k in ('report','objs','hierarchy','walk_refs')]
rows=[]
for name in ['bossCommonEffectRoot','QBDyShadow']:
 o=next(o for o in objs.values() if o['name']==name and o['type']=='GameObject');root=hierarchy(o);nodes=[]
 def walk(n,parent=None):
  path=(parent or '')+'/'+n['name'];nodes.append({'path':path,'parent':parent,'object':n['id'],'name':n['name'],'active':n['active'],'layer':n['layer'],'transform':n.get('transform'),'components':n['components']})
  for child in n['children']:walk(child,path)
 walk(root);deps=walk_refs([o['id']]);out={'rootObject':o['id'],'nodes':nodes,'dependencies':[objs[id] for id in deps]}
 path=S/'prefabs'/f'{name}.flat.json';path.write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8');rows.append({'name':name,'flatData':str(path.relative_to(R)).replace('\\','/')})
 print(name,[(n['name'],[(c['type'],c.get('script',{}).get('m_ClassName'))for c in n['components']])for n in nodes]);print('resources',[(objs[id]['type'],objs[id]['name'])for id in deps if objs[id]['type'] in ('AnimatorController','AnimationClip')])
(S/'auxiliary-index.json').write_text(json.dumps(rows,indent=2),encoding='utf8')
