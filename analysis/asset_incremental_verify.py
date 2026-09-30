"""Validate snapshot source/exports/references and hash every generated file."""
import json,hashlib,collections,sys
from pathlib import Path
from PIL import Image
sys.stdout.reconfigure(encoding='utf8')
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43';SNAP=ROOT/'generated/resource-snapshots/hud-soldier300-20260928'
index=json.loads((SNAP/'unity-consumption-index.json').read_text(encoding='utf8'));r=json.loads((SNAP/'asset-evidence-incremental.json').read_text(encoding='utf8'))
counts=collections.Counter();errors=[];paths={};meshInfo=[]
for o in r['objects']:
 if o.get('baselineReuse'):continue
 for kind,name in o['outputs'].items():
  p=ROOT/name
  if not p.is_file():errors.append('Missing '+name);continue
  if name in paths:errors.append('Duplicate output '+name)
  paths[name]=o['id'];counts[kind]+=1
  if kind in ('png','pngCanvas'):
   with Image.open(p) as im:im.verify()
  if kind=='meshNative':
   m=json.loads(p.read_text(encoding='utf8'));vs=m['vertices'];tris=[i for sm in m['submeshTriangles'] for t in sm for i in t]
   if not vs or any(i<0 or i>=len(vs) for i in tris):errors.append('Mesh index '+name)
   meshInfo.append({'object':o['id'],'name':o['name'],'vertexCount':len(vs),'triangles':len(tris)//3,'uv1Count':len(m['uv1'] or [])})
summary=[]
for p in index['prefabs']:
 d=json.loads((ROOT/p['flatData']).read_text(encoding='utf8'));ids={n['object'] for n in d['nodes']};deps={o['id']:o for o in d['dependencies']}
 for o in deps.values():
  for ref in o.get('references',[]):
   if not ref['resolved'] or ref.get('target') not in deps:errors.append('Closure missing '+o['id']+' '+str(ref))
  if o.get('resolvedAtlasByRenderDataKey') and o['resolvedAtlasByRenderDataKey'] not in deps:errors.append('Atlas missing '+o['id'])
 for b in d['outletBindings']:
  if not b['path']:errors.append('Unmapped outlet '+str(b))
 fonts=[{'object':o['id'],'name':o['name'],'outputs':o['outputs']} for o in deps.values() if o['type']=='Font']
 sprites=[{'object':o['id'],'name':o['name'],'outputs':o['outputs']} for o in deps.values() if o['type']=='Sprite']
 summary.append({'name':p['name'],'nodes':p['nodeCount'],'bindings':p['bindingCount'],'spriteCount':len(sprites),'fonts':fonts,'spriteReferences':sprites,'animationData':[c['data'] for n in d['nodes'] for c in n['components'] if (c.get('script')or{}).get('m_ClassName')=='SpineAnimator']})
baseline=r['baselineEvidence'];same=hashlib.sha256((ROOT/baseline['path']).read_bytes()).hexdigest()==baseline['sha256'];assert same
assert not errors,errors
report={'status':'passed-export-integrity-only','newOutputs':dict(counts),'meshes':meshInfo,'prefabs':summary,'errors':errors,'baselineAssetEvidenceUnchanged':same,'runtimeImported':False,'matchedVisualValidated':False,'targetCodeExecuted':False}
(SNAP/'export-validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
files=[]
for p in sorted(SNAP.rglob('*')):
 if p.is_file() and p.name!='file-hashes.json':files.append({'path':p.relative_to(ROOT).as_posix(),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
(SNAP/'file-hashes.json').write_text(json.dumps({'files':files,'count':len(files),'totalBytes':sum(x['bytes'] for x in files)},ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'status':report['status'],'newOutputs':report['newOutputs'],'meshes':meshInfo,'hashedFiles':len(files),'baselineUnchanged':same},ensure_ascii=False))
