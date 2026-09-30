"""Extract the five used custom skill shaders' original compiled GLES and fixed state."""
import json,re,sys,hashlib
from pathlib import Path
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
from UnityPy.helpers import CompressionHelper
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43';S=R/'generated/resource-snapshots/skill-effects-20260928';O=S/'shader-evidence';O.mkdir(exist_ok=True)
j=json.loads((S/'asset-evidence-incremental.json').read_text(encoding='utf8'));found=set();rows=[]
for o in j['objects']:
 if o['type']!='Shader' or o['name'] not in ('Effect/Alpha Blend','Effect/Alpha Blend UV','Effect/Sword','Effect/Pingzi','Effect/TransformTrails') or o['name'] in found:continue
 found.add(o['name']);env=UnityPy.load(str(R/o['source']));reader=next(x for x in env.objects if x.path_id==o['pathId']);d=reader.read();tree=reader.read_typetree();name=o['name'].replace('/','_').replace(' ','_');dest=O/(name+'.json');dest.write_text(json.dumps(tree,ensure_ascii=False,indent=2),encoding='utf8');segments=[]
 for pi,offs in enumerate(d.offsets):
  for si,off in enumerate(offs if isinstance(offs,list) else [offs]):
   clen=d.compressedLengths[pi];dlen=d.decompressedLengths[pi];clen=clen[si] if isinstance(clen,list) else clen;dlen=dlen[si] if isinstance(dlen,list) else dlen
   raw=CompressionHelper.decompress_lz4(bytes(d.compressedBlob)[off:off+clen],dlen)
   strings=re.findall(rb'[\x09\x0a\x0d\x20-\x7e]{5,}',raw);p=O/f'{name}.{pi}.{si}.txt';p.write_text('\n'.join(s.decode('ascii') for s in strings),encoding='utf8');segments.append(str(p.relative_to(R)).replace('\\','/'))
 rows.append({'name':o['name'],'sourceId':o['id'],'source':o['source'],'tree':str(dest.relative_to(R)).replace('\\','/'),'segments':segments,'platforms':d.platforms})
(O/'manifest.json').write_text(json.dumps(rows,indent=2),encoding='utf8');print(json.dumps(rows))
