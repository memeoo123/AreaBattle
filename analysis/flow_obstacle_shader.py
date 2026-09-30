import json,sys,re
from pathlib import Path
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
from UnityPy.helpers import CompressionHelper
R=Path('analysis/targets/wxcf1394487200e48f/43');O=R/'generated/resource-snapshots/obstacle-visuals-20260928/shaders';O.mkdir(exist_ok=True)
env=UnityPy.load(str(R/'work/webdata/data.unity3d'))
for o in env.objects:
 if o.assets_file.name!='unity_builtin_extra' or o.path_id!=10752:continue
 t=o.read_typetree();(O/'builtin10752.json').write_text(json.dumps(t,indent=2,default=lambda x:list(x) if isinstance(x,bytes) else str(x)),encoding='utf8')
 d=o.read();print('Shader',getattr(d,'m_ParsedForm',None));print('tree keys',t.keys())
 for pi,offs in enumerate(d.offsets):
  if isinstance(offs,int):offs=[offs]
  for si,off in enumerate(offs):
   cl=d.compressedLengths[pi];dl=d.decompressedLengths[pi];cl=cl[si] if isinstance(cl,list) else cl;dl=dl[si] if isinstance(dl,list) else dl
   raw=CompressionHelper.decompress_lz4(bytes(d.compressedBlob)[off:off+cl],dl)
   (O/f'builtin10752.{pi}.{si}.bin').write_bytes(raw)
   strings=re.findall(rb'[\x09\x0a\x0d\x20-\x7e]{5,}',raw)
   (O/f'builtin10752.{pi}.{si}.txt').write_text('\n'.join(s.decode('ascii') for s in strings),encoding='utf8')
