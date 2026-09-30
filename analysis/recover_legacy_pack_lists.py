"""Extract original Resources pack alias lists and verify firstpack container membership."""
import sys,json,hashlib
from pathlib import Path
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
root=Path(__file__).resolve().parent.parent
t=root/'analysis/targets/wxcf1394487200e48f/43'
source=t/'work/webdata/data.unity3d'
env=UnityPy.load(str(source));rows=[]
bundle=next((t/'work/cache').rglob('firstpack_*.unity3d'));container=list(UnityPy.load(str(bundle)).container)
resources={}
for obj in env.objects:
 if obj.type.name=='ResourceManager':
  for key,ptr in obj.read_typetree()['m_Container']:
   if key in ('firstpack','pack2'):resources[key]=dict(serializedFile=obj.assets_file.name,pathId=obj.path_id,pointer=ptr)
for obj in env.objects:
 if obj.type.name!='TextAsset':continue
 d=obj.read()
 if d.m_Name not in ('firstpack','pack2'):continue
 name=d.m_Name;raw=d.m_Script.encode('utf8',errors='surrogateescape');aliases=json.loads(raw)
 assert isinstance(aliases,list) and all(isinstance(a,str) for a in aliases)
 assert resources[name]['pointer']['m_PathID']==obj.path_id
 output=root/'UnityProject/Assets/AreaBattle/Resources'/f'{name}.txt'
 if output.exists():assert output.read_bytes()==raw,'Do not overwrite different existing pack list'
 output.write_bytes(raw)
 matches=[]
 if name=='firstpack':
  for alias in aliases:
   assert alias.endswith('.unity3d')
   prefix='assets/gameres/bundleres/'+alias[:-8]
   paths=[x for x in container if x==prefix or x.startswith(prefix+'/')]
   matches.append(dict(alias=alias,containerPaths=paths))
 rows.append(dict(name=name,serializedFile=obj.assets_file.name,pathId=obj.path_id,resourceManager=resources[name],bytes=len(raw),sha256=hashlib.sha256(raw).hexdigest(),aliases=aliases,output=output.relative_to(root).as_posix(),containerMatches=matches))
assert len(rows)==2
result=dict(source=source.relative_to(root).as_posix(),sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),firstpackBundle=bundle.relative_to(root).as_posix(),firstpackBundleSha256=hashlib.sha256(bundle.read_bytes()).hexdigest(),lists=rows,qualification='Original text preserved byte-for-byte. ResourceManager pointers and container keys inspected; atlas variants or naming differences remain explicit if unmatched.')
(t/'generated/outgame/ORIGINAL_PACK_LISTS_AUDIT.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print([(r['name'],len(r['aliases']),r['sha256']) for r in rows]);print([(m['alias'],len(m['containerPaths'])) for r in rows for m in r['containerMatches']])
