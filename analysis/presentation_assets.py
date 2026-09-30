import json,sys
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
base=json.loads((ROOT/'generated/asset-evidence.json').read_text(encoding='utf8'))
inc=json.loads((ROOT/'generated/resource-snapshots/hud-soldier300-20260928/asset-evidence-incremental.json').read_text(encoding='utf8'))
objs={o['id']:o for o in base['objects']};objs.update({o['id']:o for o in inc['objects']})
def tree(o):return json.loads((ROOT/o['outputs']['typetree']).read_text(encoding='utf8'))
for name in ('soldier_100','soldier_200','soldier_300'):
 if name=='soldier_300':d=json.loads((ROOT/'generated/resource-snapshots/hud-soldier300-20260928/prefabs/soldier_300.flat.json').read_text(encoding='utf8'));node=d['nodes'][0]
 else:d=json.loads((ROOT/f'generated/unity-assets/prefabs/{name}.hierarchy.json').read_text(encoding='utf8'));node=d['root']
 print('PREFAB',name)
 for c in node['components']:
  if (c.get('script')or{}).get('m_ClassName')=='SpineAnimator':print('ANIMATION',c['data'])
  if c['type']=='MeshRenderer':
   print('RENDERER',{k:v for k,v in c['data'].items() if 'Sort' in k or 'Material' in k})
   for ref in c['references']:
    if not ref['property'].startswith('.m_Materials'):continue
    o=objs[ref['target']];t=tree(o);print('MATERIAL',o['id'],json.dumps(t,ensure_ascii=False))
    for ref in o['references']:
     target=objs[ref['target']]
     if target['type']=='Texture2D':
      tex=json.loads((ROOT/target['outputs']['textureRaw']).read_text(encoding='utf8'))
      print('TEXTURE',target['id'],target['name'],{k:v for k,v in tex.items() if k in ('m_Width','m_Height','m_TextureFormat','m_MipCount','m_TextureSettings','m_ColorSpace','m_LightmapFormat','m_IsReadable')})
