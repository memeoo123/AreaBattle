"""Inspect compiled local shader blobs with UnityPy's pinned decompressor."""
import json, sys, re
from pathlib import Path
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
from UnityPy.helpers import CompressionHelper
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
OUT=ROOT/'generated/unity-assets/ShaderEvidence';OUT.mkdir(parents=True,exist_ok=True)
src=ROOT/'work/cache/StreamingAssets/WebGL/Proj_hdzd/pack2_3d9a052945c53b0c6660bdb786e5d5b2.unity3d'
env=UnityPy.load(str(src))
for o in env.objects:
    if o.type.name!='Shader' or o.path_id!=-1870220313943124997:continue
    d=o.read();t=o.read_typetree();safe=re.sub(r'[^\w.-]','_',d.m_ParsedForm.m_Name)
    (OUT/(safe+'.typetree.json')).write_text(json.dumps(t,indent=2,default=lambda x:list(x) if isinstance(x,bytes) else str(x)),encoding='utf8')
    print(d.m_ParsedForm.m_Name, 'platforms',d.platforms,'offsets',d.offsets,'compressedLengths',d.compressedLengths,'decompressedLengths',d.decompressedLengths)
    for pi,offs in enumerate(d.offsets):
        if isinstance(offs,int):offs=[offs]
        for si,off in enumerate(offs):
            clen=d.compressedLengths[pi]; dlen=d.decompressedLengths[pi]
            clen=clen[si] if isinstance(clen,list) else clen;dlen=dlen[si] if isinstance(dlen,list) else dlen
            raw=CompressionHelper.decompress_lz4(bytes(d.compressedBlob)[off:off+clen],dlen)
            (OUT/f'{safe}.platform{pi}.segment{si}.bin').write_bytes(raw)
            strings=re.findall(rb'[\x09\x0a\x0d\x20-\x7e]{5,}',raw)
            (OUT/f'{safe}.platform{pi}.segment{si}.strings.txt').write_text('\n'.join(s.decode('ascii') for s in strings),encoding='utf8')
            print('segment',pi,si,len(raw),'strings',len(strings))
