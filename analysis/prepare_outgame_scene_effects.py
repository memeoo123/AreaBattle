"""Derive original idle-effect native hierarchies using reviewed exporters."""
import json,sys
from pathlib import Path
R=Path('analysis/targets/wxcf1394487200e48f/43');S=R/'generated/resource-snapshots/outgame-scenes-20260929'
read=lambda p:json.loads(p.read_text(encoding='utf8'))
source=Path('analysis/asset_focus.py').read_text(encoding='utf8').split('roots=[]')[0]
ctx={'__file__':str(Path('analysis/asset_focus.py').resolve())};exec(compile(source,'scene_effect_focus','exec'),ctx)
base=ctx['objs'];base.update({o['id']:o for o in read(S/'asset-evidence-incremental.json')['objects']})
hierarchy,walk_refs=ctx['hierarchy'],ctx['walk_refs'];rows=[];(S/'prefabs').mkdir(exist_ok=True)
for name in ['eff_idle_GuBao','idle_eff_HuoShan','eff_idle_ZhaoZe']:
 matches=[o for o in base.values() if o['name']==name and o['type']=='GameObject'];assert len(matches)==1,(name,len(matches))
 o=matches[0];root=hierarchy(o);nodes=[]
 def walk(n,parent=None):
  path=(parent or '')+'/'+n['name'];nodes.append({'path':path,'parent':parent,'object':n['id'],'name':n['name'],'active':n['active'],'layer':n['layer'],'transform':n.get('transform'),'components':n['components']})
  for child in n['children']:walk(child,path)
 walk(root);deps=walk_refs([o['id']]);out={'rootObject':o['id'],'nodes':nodes,'dependencies':[base[id] for id in deps]}
 dest=S/'prefabs'/(name+'.flat.json');dest.write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8');rows.append({'name':name,'flatData':dest.relative_to(R).as_posix()})
 print(name,[(n['name'],[(c['type'],(c.get('script')or{}).get('m_ClassName')) for c in n['components']]) for n in nodes])
(S/'unity-consumption-index.json').write_text(json.dumps({'prefabs':rows},indent=2),encoding='utf8')
for script in ['flow_boss_sources.py','flow_boss_native_prepare.py']:
 source=Path('analysis',script).read_text(encoding='utf8').replace('boss-entities-20260928','outgame-scenes-20260929')
 exec(compile(source,script,'exec'),{'__file__':str(Path('analysis',script).resolve())})
