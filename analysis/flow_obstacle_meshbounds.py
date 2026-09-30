import json,sys
from pathlib import Path
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
R=Path('analysis/targets/wxcf1394487200e48f/43');S=R/'generated/resource-snapshots/obstacle-visuals-20260928'
e=json.loads((R/'generated/asset-evidence.json').read_text(encoding='utf8'));m=next(o for o in e['objects'] if o['id']=='CAB-d1e0db418281b7eefd4ea44394873e5d:-4260555571800234629')
env=UnityPy.load(str(R/m['source']));o=next(o for o in env.objects if o.path_id==m['pathId'])
t=o.read_typetree();out={'meshId':m['id'],'localBounds':t['m_LocalAABB'],'submeshes':t['m_SubMeshes'],'indexFormat':t['m_IndexFormat']}
(S/'mesh-original-bounds.json').write_text(json.dumps(out,indent=2),encoding='utf8');print(out['localBounds'])
