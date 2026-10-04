"""Flatten new prefabs and resource closure for a later editable Unity import; no runtime changes."""
import json,hashlib,sys,collections
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43';SNAP=ROOT/'generated/resource-snapshots/top-info-effects-20261003'
base=Path('analysis/asset_focus.py').read_text(encoding='utf8').split('roots=[]')[0]
base=base.replace("ROOT/'generated/asset-evidence.json'","ROOT/'generated/resource-snapshots/top-info-effects-20261003/asset-evidence-incremental.json'")
ctx={'__file__':str(Path('analysis/asset_focus.py').resolve())};exec(compile(base,'incremental_readonly','exec'),ctx)
report,objs,hierarchy,walk_refs,tree=[ctx[k] for k in ('report','objs','hierarchy','walk_refs','tree')]
baseline=json.loads((ROOT/'generated/asset-evidence.json').read_text(encoding='utf8'))
for item in baseline['objects']:
 if item['id'] not in objs:objs[item['id']]={**item,'baselineReuse':True}
targets=['hdzd_effect_jinbiglow','hdzd_eff_diamond02'];prefabs=[]
for name in targets:
 c=next(c for c in report['containers'] if c['assetPath'].endswith('/'+name+'.prefab'))
 root=hierarchy(objs[c['object']]);nodes=[];index={}
 def walk(n,parent=None):
  path=(parent or '')+'/'+n['name'];row={'path':path,'parent':parent,'object':n['id'],'name':n['name'],'active':n['active'],'layer':n['layer'],'transform':n.get('transform'),'components':n['components']};nodes.append(row);index[n['id']]=path
  for component in n['components']:index[component['id']]=path
  for child in n['children']:walk(child,path)
 walk(root)
 bindings=[]
 for n in nodes:
  for comp in n['components']:
   if (comp.get('script')or{}).get('m_ClassName')!='UIOutlet':continue
   for i,info in enumerate(comp['data'].get('OutletInfos',[])):
    ref=next((ref for ref in comp['references'] if ref['property']==f'.OutletInfos[{i}].Object'),None)
    bindings.append({'name':info['Name'],'componentType':info['ComponentType'],'object':ref['target'] if ref else None,'path':index.get(ref['target']) if ref else None,'owner':n['path']})
 deps=walk_refs([c['object']]);unresolved=[{'owner':id,**ref} for id in deps for ref in objs[id].get('references',[]) if not ref['resolved'] or ref.get('target') not in objs]
 data={'assetPath':c['assetPath'],'rootObject':c['object'],'nodes':nodes,'outletBindings':bindings,'dependencies':[objs[id] for id in deps],'unresolvedReferences':unresolved,'partialComponents':[comp['id'] for n in nodes for comp in n['components'] if comp.get('schemaPartial')],'coordinateSpace':'Original Unity local transforms and RectTransform anchors; no axis/scale conversions'}
 (SNAP/'prefabs').mkdir(exist_ok=True);dest=SNAP/'prefabs'/f'{name}.flat.json';dest.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf8')
 prefabs.append({'name':name,'assetPath':c['assetPath'],'root':c['object'],'flatData':str(dest.relative_to(ROOT)).replace('\\','/'),'nodeCount':len(nodes),'bindingCount':len(bindings),'dependencyCount':len(deps),'unresolvedReferences':unresolved,'partialComponents':data['partialComponents']})
 print(name,len(nodes),'nodes',len(bindings),'bindings',len(deps),'deps','unresolved',len(unresolved),'partial',len(data['partialComponents']))
new=[o for o in objs.values() if not o.get('baselineReuse')]
resourceRows=[]
for o in new:
 if o['type'] not in ('Sprite','Texture2D','Mesh','Material','Font','AnimationClip'):continue
 row={k:o[k] for k in ('id','type','name','source','outputs')}
 if o['type']=='Sprite':
  t=tree(o);row.update(rect=t.get('m_Rect'),pivot=t.get('m_Pivot'),pixelsToUnits=t.get('m_PixelsToUnits'),border=t.get('m_Border'),trimOffset=o.get('trimOffset'))
 row['files']=[{'path':path,'sha256':hashlib.sha256((ROOT/path).read_bytes()).hexdigest(),'bytes':(ROOT/path).stat().st_size} for path in o['outputs'].values()]
 resourceRows.append(row)
manifest=json.loads((SNAP/'acquisition-manifest.json').read_text(encoding='utf8'))
baseline=manifest['baselineAssetEvidence'];baselineUnchanged=hashlib.sha256((ROOT/baseline['path']).read_bytes()).hexdigest()==baseline['sha256']
assert baselineUnchanged
summary={'schemaVersion':1,'target':manifest['target'],'snapshot':manifest['snapshot'],'sourceManifest':'generated/resource-snapshots/top-info-effects-20261003/acquisition-manifest.json','prefabs':prefabs,'newObjectCounts':dict(collections.Counter(o['type'] for o in new)),'resources':resourceRows,'baselinePreserved':baselineUnchanged,'newObjectCount':len(new),'reusedObjectCount':len(objs)-len(new),'validation':{'sourceBundlesVerified':len(manifest['items']),'newSourceBundles':sum(x['acquisition']=='new-snapshot' for x in manifest['items']),'exportErrors':[{'object':o['id'],'error':o['exportError']} for o in new if 'exportError'in o],'runtimeImported':False,'targetCodeExecuted':False}}
(SNAP/'unity-consumption-index.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'new':len(new),'counts':summary['newObjectCounts'],'baselinePreserved':baselineUnchanged},ensure_ascii=False))
