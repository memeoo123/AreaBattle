import sys,json,hashlib,collections
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
from pathlib import Path
R=Path('analysis/targets/wxcf1394487200e48f/43');S=R/'generated/resource-snapshots/boss-entities-20260928';O=S/'spine';O.mkdir(exist_ok=True)
E=json.loads((S/'asset-evidence-incremental.json').read_text(encoding='utf8'));objs={o['id']:o for o in E['objects']};envs={};found=[]
for o in E['objects']:
 if o['type']!='TextAsset' or o.get('baselineReuse'):continue
 if o['source']not in envs:envs[o['source']]=UnityPy.load(str(R/o['source']))
 r=next(x for x in envs[o['source']].objects if x.path_id==o['pathId']);d=r.read_typetree();blob=d['m_Script'];blob=blob.encode('utf8',errors='surrogateescape') if isinstance(blob,str)else bytes(blob)
 dest=O/(o['name']+'.txt');dest.write_bytes(blob);row={'id':o['id'],'name':o['name'],'source':o['source'],'sourceSha256':hashlib.sha256((R/o['source']).read_bytes()).hexdigest(),'path':str(dest.relative_to(R)).replace('\\','/'),'sha256':hashlib.sha256(blob).hexdigest()}
 try:
  j=json.loads(blob);row['version']=j.get('skeleton',{}).get('spine');row['bones']=len(j.get('bones',[]));row['slots']=len(j.get('slots',[]));row['ik']=j.get('ik');row['transformConstraints']=j.get('transform');row['pathConstraints']=j.get('path');row['animations']={k:list(v)for k,v in j.get('animations',{}).items()}
 except Exception:pass
 found.append(row)
(S/'spine-sources.json').write_text(json.dumps(found,indent=2),encoding='utf8');print(json.dumps(found,ensure_ascii=True,indent=2))
