"""Prepare original TipUI hierarchy using the established source UI import contract."""
from pathlib import Path
import json,hashlib
p=Path(__file__).with_name('asset_focus.py');n={'__file__':str(p.resolve())};exec(p.read_text(encoding='utf8').split('roots=[]')[0],n)
t=n['ROOT'];objects=n['objs'];container=next(c for c in n['report']['containers'] if c['assetPath'].endswith('/tipui.prefab'));root=n['hierarchy'](objects[container['object']]);nodes=[];sprites={};fonts={};idpaths={};outlets=[];pending=[]
bools=set('m_RaycastTarget m_Maskable m_PreserveAspect m_FillCenter m_FillClockwise m_UseSpriteMesh m_UseGraphicAlpha m_Interactable m_ChildForceExpandWidth m_ChildForceExpandHeight m_ChildControlWidth m_ChildControlHeight m_ChildScaleWidth m_ChildScaleHeight m_ReverseArrangement m_BestFit m_AlignByGeometry m_RichText m_WholeNumbers m_WrapAround'.split())
def normalize(v):
 if isinstance(v,dict):return {k:bool(x) if k in bools else normalize(x) for k,x in v.items()}
 if isinstance(v,list):return [normalize(x) for x in v]
 return v
def walk(x,path=''):
 idpaths[x['id']]=path
 row=dict(path=path,name=x['name'],sourceId=x['id'],active=bool(x['active']),transform=x['transform'],components=[])
 for c in x['components']:
  idpaths[c['id']]=path;kind=(c.get('script')or{}).get('m_ClassName',c['type'])
  if kind in ['Transform','RectTransform']:row['transformSourceId']=c['id'];continue
  if kind=='CanvasRenderer':continue
  if kind=='UIOutlet':outlets.extend((c['id'].rsplit(':',1)[0],v) for v in c['data']['OutletInfos']);continue
  if kind=='Animation':pending.append(dict(sourceId=c['id'],path=path,reason='Original disabled Animation references external clips; native clip restoration pending.'));continue
  item=dict(className=kind,sourceId=c['id'],data=json.dumps(normalize(c['data']),ensure_ascii=False))
  for prop,key in [('.m_Sprite','sprite'),('.m_FontData.m_Font','font'),('.m_TargetGraphic','targetGraphic')]:
   ref=next((v for v in c['references'] if v['property']==prop and v.get('target')),None)
   if not ref:continue
   item[key+'Id']=ref['target']
   if key=='targetGraphic':continue
   obj=objects[ref['target']];tree=n['tree'](obj)
   if key=='sprite':
    png=obj['outputs']['pngCanvas'];sprites[obj['id']]=dict(id=obj['id'],name=obj['name'],path=png,sha256=hashlib.sha256((t/png).read_bytes()).hexdigest(),pivot=tree['m_Pivot'],ppu=tree['m_PixelsToUnits'],border=tree['m_Border'])
   else:
    font=obj['outputs']['font'];entry=dict(id=obj['id'],name=obj['name'],path=font,sha256=hashlib.sha256((t/font).read_bytes()).hexdigest());fallback=n['target'](obj,'.m_FallbackFonts[0]')
    if fallback:entry.update(fallbackPath=fallback['outputs']['font'],fallbackSourceId=fallback['id'])
    fonts[obj['id']]=entry
  row['components'].append(item)
 nodes.append(row)
 for child in x['children']:walk(child,(path+'/' if path else '')+child['name'])
walk(root)
bindings=[]
for cabinet,v in outlets:
 assert v['Object']['m_FileID']==0
 bindings.append(dict(name=v['Name'],path=idpaths[cabinet+':'+str(v['Object']['m_PathID'])],owner=''))
assert len(nodes)==13 and len(bindings)==6
manifest=dict(prefabs=[dict(name='TipUI',rootPath=container['assetPath'],nodes=nodes,bindings=bindings)],sprites=list(sprites.values()),fonts=list(fonts.values()))
(t/'generated/outgame/tip-ui-import.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
(t/'generated/outgame/TIP_UI_SOURCE_AUDIT.json').write_text(json.dumps(dict(source=container,nodes=13,bindings=bindings,pending=pending,qualification='Static source hierarchy only. Business behavior, native disabled Animation clips and matched rendering remain separate.'),ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print({'nodes':13,'bindings':6,'sprites':len(sprites),'fonts':len(fonts)})
