"""Acquire catalog-listed obstacle prefabs and recover exact collider hierarchies."""
import sys,json,hashlib,urllib.request,subprocess
from pathlib import Path
sys.stdout.reconfigure(encoding='utf-8')
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
def read(p):return json.loads((R/p).read_text(encoding='utf-8'))
def save(p,v):
 p=R/p;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
layouts=[json.loads(p.read_text(encoding='utf-8')) for p in (R/'generated/all-levels').glob('*.json')]
ids=sorted({o['EnityID'] for l in layouts for o in l.get('ObstacleInfoCfgs',[])})
models={m['id']:m for m in read('generated/tables/EntityModelConfig.json')['Datas']}
catalog={e['name']:e for e in read('generated/resource-catalog.json')['entries']}
base='https://gameoss-hz.entermore.cn/app-3/Release/Proj_hdzd/XYX/weixin/hcrzd/1.36/StreamingAssets/WebGL/Proj_hdzd/'
paths=[];acquired=[]
for entity in ids:
 name=models[entity]['AssetName'];e=catalog['model/entity/'+name.lower()+'.prefab.unity3d']
 cached=R/'work/cache/StreamingAssets/WebGL/Proj_hdzd'/e['file'];remote=R/'work/remote'/e['file']
 path=cached if cached.exists() else remote
 if path.exists():blob=path.read_bytes();origin='existing'
 else:
  with urllib.request.urlopen(base+e['file'],timeout=25) as response:blob=response.read(e['size']+1)
  origin='downloaded'
 assert len(blob)==e['size'] and hashlib.md5(blob).hexdigest()==e['md5'],name
 path.parent.mkdir(parents=True,exist_ok=True)
 if not path.exists():path.write_bytes(blob)
 paths.append((entity,name,path))
 acquired.append(dict(entityId=entity,name=name,path=path.relative_to(R).as_posix(),acquisition=origin,
  catalogLine=e['line'],size=len(blob),md5=e['md5'],sha256=hashlib.sha256(blob).hexdigest(),url=base+e['file']))
save('evidence/obstacle-downloads.json',acquired)
env=UnityPy.Environment()
for _,_,p in paths:env.load_file(str(p))
def hierarchy(go):
 data=go.read();node={'name':data.m_Name,'active':bool(data.m_IsActive),'layer':data.m_Layer,
   'pathId':go.path_id,'serializedFile':go.assets_file.name,'components':[],'children':[]}
 for item in data.m_Component:
  ptr=item.component if hasattr(item,'component') else item[1]
  obj=ptr.deref();kind=obj.type.name
  if kind=='Transform':
   tf=obj.read();node['transform']=obj.read_typetree()
   for c in tf.m_Children:node['children'].append(hierarchy(c.read().m_GameObject.deref()))
  elif kind.endswith('Collider'):
   node['components'].append({'type':kind,'pathId':obj.path_id,'data':obj.read_typetree()})
 return node
prefabs=[]
for entity,name,path in paths:
 matching=[(asset,p) for asset,p in env.container.items() if asset.endswith('/'+name.lower()+'.prefab')]
 assert len(matching)==1,(name,len(matching))
 asset,ptr=matching[0];node=hierarchy(ptr.deref());prefabs.append(dict(entityId=entity,name=name,assetPath=asset,root=node))
dump=subprocess.run([sys.executable,'analysis/flow_disassemble.py','ObstacleInfoCfg','SetTransform'],capture_output=True,check=True).stdout
dest=R/'generated/obstacle/SetTransform.txt';dest.parent.mkdir(parents=True,exist_ok=True);dest.write_bytes(dump)
save('generated/obstacle-evidence.json',dict(target=dict(appId='wxcf1394487200e48f',version='43'),
 transform={'status':'confirmed-static','positionDivisor':100,'eulerDivisor':100,'scaleDivisor':100,
 'evidence':'generated/obstacle/SetTransform.txt','function':11209,'offset':5519511,'size':534,
 'calls':{'4699':'UnityMathfTool.GetVector3ByVector3Int','1348':'Transform.set_position','5581':'Transform.set_eulerAngles','1200':'Transform.set_localScale'}},
 prefabs=prefabs,unknowns=['Original runtime matched topology/capture not yet available']))
def nodes(n):yield n;yield from (c for child in n['children'] for c in nodes(child))
print(json.dumps({'prefabs':len(prefabs),'colliders':{kind:sum(c['type']==kind for p in prefabs for n in nodes(p['root']) for c in n['components']) for kind in ['BoxCollider','MeshCollider','SphereCollider','CapsuleCollider']},'bytes':sum(x['size'] for x in acquired)},ensure_ascii=False))
