"""Reuse reviewed local exporter with snapshot-only destinations and immutable baseline references."""
import json
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
SNAP=ROOT/'generated/resource-snapshots/task-box-effect-20261003'
acquisition=json.loads((SNAP/'acquisition-manifest.json').read_text(encoding='utf8'))
assert all(x['status']=='verified' for x in acquisition['items'])
baseline=json.loads((ROOT/'generated/asset-evidence.json').read_text(encoding='utf8'))
source=Path('analysis/asset_export.py').read_text(encoding='utf8')
source=source.replace("OUT = ROOT / 'generated/unity-assets'","OUT = ROOT / 'generated/resource-snapshots/task-box-effect-20261003/unity-assets'")
source=source.replace("paths=sorted(CACHE.rglob('*.unity3d'))+[ROOT/'work/webdata/data.unity3d',ROOT/'work/webdata/Resources/unity_default_resources']","paths=[ROOT/x['path'] for x in acquisition['items']]+[ROOT/'work/webdata/data.unity3d',ROOT/'work/webdata/Resources/unity_default_resources']")
source=source.replace('objects=list(env.objects)', '''for _ in range(10):
    try:
        _containers=env.container
        break
    except RuntimeError as e:
        if 'dictionary changed size' not in str(e):raise
else:raise RuntimeError('UnityPy container dependency discovery did not stabilize')
objects=list(env.objects)''')
source=source.replace('for n,o in enumerate(objects):','''for n,o in enumerate(objects):
    if key(o) in baselineObjects:
        records.append({**baselineObjects[key(o)],'baselineReuse':True})
        continue''')
source=source.replace("writejson(ROOT/'generated/asset-evidence.json',report)","report['baselineEvidence']=acquisition['baselineAssetEvidence']; report['acquisitionManifest']='generated/resource-snapshots/task-box-effect-20261003/acquisition-manifest.json'; writejson(ROOT/'generated/resource-snapshots/task-box-effect-20261003/asset-evidence-incremental.json',report)")
ctx={'__file__':str(Path('analysis/asset_export.py').resolve()),'baselineObjects':{o['id']:o for o in baseline['objects']},'acquisition':acquisition}
exec(compile(source,'snapshot_asset_export','exec'),ctx)
# UnityPy maps are tuples before JSON serialization. The original exporter only walked
# lists, so rebuild from persisted JSON (maps have become arrays), preserving baseline.
report=ctx['report'];known={o['id'] for o in baseline['objects']}|{o['id'] for o in report['objects']}
deps={d['serializedFile']:d for d in baseline['dependencies']+report['dependencies']}
def refs(value,owner,path=''):
 if isinstance(value,dict):
  if 'm_FileID' in value and 'm_PathID' in value and value['m_PathID']:
   fid=value['m_FileID'];pid=value['m_PathID'];cab=owner['serializedFile'];ext=None
   if fid:
    ext=deps[cab]['externals'][fid-1];cab=ext['path'].rsplit('/',1)[-1]
   if cab=='unity default resources':cab='unity_default_resources'
   ident=cab+':'+str(pid)
   return [{'property':path,'fileId':fid,'pathId':pid,'serializedFile':cab,'externalGuid':ext['guid'] if ext else None,'resolved':ident in known,'target':ident if ident in known else None}]
  return [r for k,v in value.items() for r in refs(v,owner,path+'.'+k)]
 if isinstance(value,(list,tuple)):return [r for i,v in enumerate(value) for r in refs(v,owner,path+f'[{i}]')]
 return []
repaired=[]
for o in report['objects']:
 if 'typetree' not in o['outputs']:continue
 tree=json.loads((ROOT/o['outputs']['typetree']).read_text(encoding='utf8'));new=refs(tree,o)
 if new!=o.get('references',[]):
  repaired.append({'object':o['id'],'before':len(o.get('references',[])),'after':len(new)})
  o['references']=new;o['referencesRebuiltFromSavedTree']=True
report['referenceCorrection']={'reason':'Original recursive reference scanner omitted UnityPy tuple map entries, including Material texture bindings and SpriteAtlas packed render data. This snapshot rebuilds references from JSON arrays without changing baseline evidence.','objects':repaired}
(SNAP/'asset-evidence-incremental.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
print('Reference correction objects',len(repaired))
