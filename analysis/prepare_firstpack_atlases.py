"""Prepare source sprite payloads for native firstpack atlas reconstruction."""
from pathlib import Path
import json,hashlib,shutil
root=Path(__file__).resolve().parent.parent;t=root/'analysis/targets/wxcf1394487200e48f/43';d=json.loads((t/'generated/asset-evidence.json').read_text(encoding='utf8'));objects={x['id']:x for x in d['objects']};atlases=[]
for name in ['PublicUI','PublicBtn']:
 atlas=next(x for x in objects.values() if x['type']=='SpriteAtlas' and x['name']==name);sprites=[]
 for ref in atlas['references']:
  if not ref['property'].startswith('.m_PackedSprites'):continue
  sprite=objects[ref['target']];tree=json.loads((t/sprite['outputs']['typetree']).read_text(encoding='utf8'));source=t/sprite['outputs']['pngCanvas'];dest=root/'UnityProject/Assets/AreaBattle/Resources/Recovered/FirstPack/Atlases'/name/(sprite['name']+'.png')
  dest.parent.mkdir(parents=True,exist_ok=True)
  if dest.exists():assert dest.read_bytes()==source.read_bytes()
  else:shutil.copyfile(source,dest)
  sprites.append(dict(name=sprite['name'],sourceId=sprite['id'],source=source.relative_to(root).as_posix(),assetPath=dest.relative_to(root/'UnityProject').as_posix(),sha256=hashlib.sha256(source.read_bytes()).hexdigest(),pivot=tree['m_Pivot'],border=tree['m_Border'],pixelsPerUnit=tree['m_PixelsToUnits'],width=tree['m_Rect']['width'],height=tree['m_Rect']['height']))
 assert len({x['name'] for x in sprites})==len(sprites)
 atlases.append(dict(name=name,sourceId=atlas['id'],assetPath='Assets/AreaBattle/Resources/Recovered/FirstPack/Atlases/'+name+'.spriteatlas',sprites=sprites))
manifest=dict(atlases=atlases,qualification='Uses existing evidence-backed untrimmed canvas PNG exports preserving original pivot/border/PPU. Unity repacking layout is not original atlas texture layout; source bundle exposes runtime atlas only, not editor packing settings.')
(t/'generated/outgame/firstpack-atlas-import.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf8');print([(x['name'],len(x['sprites'])) for x in atlases])
