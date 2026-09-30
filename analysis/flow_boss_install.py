"""Install pinned, unmodified official Spine 4.1 runtime (no Editor or examples)."""
from pathlib import Path
import shutil,hashlib,json
src=Path('analysis/vendor/spine-runtimes-4.1')
dest=Path('UnityProject/Assets/AreaBattle/ThirdParty/Spine41')
for a,b in [('spine-csharp/src','spine-csharp'),('spine-unity/Assets/Spine/Runtime/spine-unity','spine-unity')]:
 shutil.copytree(src/a,dest/b,dirs_exist_ok=True)
for name in ['LICENSE','README.md']:
 shutil.copyfile(src/name,dest/name)
manifest={'repository':'https://github.com/EsotericSoftware/spine-runtimes','branch':'4.1','commit':'77a5db0ec6d16331f5efbaa7662bba9355bd3424','sourceSkeletonVersion':'4.1.16','scope':'Unmodified spine-csharp and spine-unity runtime only; no Editor, samples or original executable game code.','files':[]}
for p in sorted(dest.rglob('*')):
 if p.is_file():manifest['files'].append({'path':str(p.relative_to(dest)).replace('\\','/'),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
(dest/'PROVENANCE.json').write_text(json.dumps(manifest,indent=2),encoding='utf8')
print(len(manifest['files']))
