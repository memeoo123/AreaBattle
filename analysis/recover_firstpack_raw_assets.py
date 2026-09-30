"""Preserve firstpack TextAssets and font payloads for native bundle reconstruction."""
from pathlib import Path
import sys,json,hashlib
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
root=Path(__file__).resolve().parent.parent;t=root/'analysis/targets/wxcf1394487200e48f/43';source=next((t/'work/cache').rglob('firstpack_*.unity3d'));env=UnityPy.load(str(source));assets=[]
for key,obj in env.container.items():
 if obj.type.name=='TextAsset' and key.startswith('assets/gameres/bundleres/data/config/'):
  data=obj.read();raw=data.m_Script.encode('utf8',errors='surrogateescape');relative='Recovered/FirstPack/Config/'+data.m_Name;destination=root/'UnityProject/Assets/AreaBattle/Resources'/f'{relative}.bytes'
 elif obj.type.name=='Font' and '/font/' in key:
  data=obj.read();raw=bytes(data.m_FontData);relative='Recovered/Fonts/'+data.m_Name;extension='.otf' if data.m_Name=='defaultNullFont' else '.ttf';destination=root/'UnityProject/Assets/AreaBattle/Resources'/f'{relative}{extension}'
 else:continue
 destination.parent.mkdir(parents=True,exist_ok=True)
 if destination.exists():assert destination.read_bytes()==raw,('Existing asset mismatch',str(destination))
 else:destination.write_bytes(raw)
 assets.append(dict(sourcePath=key,sourceFile=obj.assetsfile.name,pathId=obj.path_id,type=obj.type.name,name=data.m_Name,resourceKey=relative,destination=destination.relative_to(root).as_posix(),bytes=len(raw),sha256=hashlib.sha256(raw).hexdigest()))
assert sum(x['type']=='TextAsset' for x in assets)==85
assert sum(x['type']=='Font' for x in assets)==2
assert len({x['destination'].lower() for x in assets})==87
out=dict(source=source.relative_to(root).as_posix(),sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),assets=assets,qualification='Byte-preserving raw TextAsset and Font data import. Existing HY font matched exactly. Font importer/native rendering settings and firstpack UI/atlas reconstruction tracked separately.')
(t/'generated/outgame/FIRSTPACK_RAW_ASSETS_AUDIT.json').write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print({'textAssets':85,'fonts':2,'bytes':sum(a['bytes'] for a in assets)})
