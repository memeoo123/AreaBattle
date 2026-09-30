"""Prepare original TipUI hierarchy using the established source UI import contract."""
from pathlib import Path
import json,hashlib
p=Path(__file__).with_name('asset_focus.py');n={'__file__':str(p.resolve())};exec(p.read_text(encoding='utf8').split('roots=[]')[0],n)
t=n['ROOT'];objects=n['objs'];container={'assetPath':'resources.assets:273 (original Resources object)','object':'resources.assets:273'};nodes=[];sprites={};fonts={};idpaths={};outlets=[];pending=[]
# Player resources omit MonoBehaviour field trees. Decode against complete same-class
# schemas from the authorized bundles, requiring full payload consumption.
import sys,struct
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
originalTree=n['tree'];decoded={};decodeEvidence=[];environments={}
def env(path):
 if path not in environments:environments[path]=UnityPy.load(str(t/path))
 return environments[path]
def reader(obj):return next(x for x in env(obj['source']).objects if x.path_id==obj['pathId'] and x.assets_file.name==obj['id'].rsplit(':',1)[0])
def scriptClass(obj):
 script=n['target'](obj,'.m_Script');return originalTree(script).get('m_ClassName') if script else None
def flatten(node):
 yield node
 for child in node['children']:yield from flatten(child)
ids=[c['id'] for row in flatten(n['hierarchy'](objects[container['object']])) for c in row['components'] if c['type']=='MonoBehaviour']
for key in ids:
 obj=objects[key];cls=scriptClass(obj);raw=(t/obj['outputs']['raw']).read_bytes()
 if cls=='Proj_xqzdLoadingUI':
  assert len(raw)==68 and raw[28:32]==bytes(4)
  data=originalTree(obj).copy()
  for offset,field in ((32,'txt_loading'),(44,'MASK'),(56,'anim')):
   fileId,pathId=struct.unpack_from('<iq',raw,offset);data[field]={'m_FileID':fileId,'m_PathID':pathId}
  schemaSource='metadata4387 + exact three PPtr payloads at32/44/56'
 else:
  match=next(o for o in objects.values() if o['type']=='MonoBehaviour' and not o.get('typetreePartial') and scriptClass(o)==cls)
  data=reader(obj).read_typetree(reader(match).serialized_type.node,check_read=True);schemaSource=match['id']
 decoded[key]=data;ownerReader=reader(obj)
 def refs(value,path=''):
  if isinstance(value,dict):
   if set(value)=={'m_FileID','m_PathID'} and value['m_PathID']:
    cabinet=key.rsplit(':',1)[0] if value['m_FileID']==0 else ownerReader.assets_file.externals[value['m_FileID']-1].path.split('/')[-1]
    dest=cabinet+':'+str(value['m_PathID']);yield {'property':path,'target':dest,'resolved':dest in objects}
   else:
    for k,v in value.items():yield from refs(v,path+'.'+k)
  elif isinstance(value,list):
   for i,v in enumerate(value):yield from refs(v,path+'['+str(i)+']')
 obj['references']=list(refs(data));decodeEvidence.append({'id':key,'class':cls,'bytes':len(raw),'sha256':hashlib.sha256(raw).hexdigest(),'schemaSource':schemaSource,'data':data})
n['tree']=lambda obj:decoded.get(obj['id'],originalTree(obj))
root=n['hierarchy'](objects[container['object']])
(t/'generated/outgame/LOADING_UI_DECODED_COMPONENTS.json').write_text(json.dumps(decodeEvidence,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
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
assert root['name']=='Proj_xqzdLoadingUI'
for field in ('txt_loading','MASK','anim'):
 ref=decoded['resources.assets:1605'][field];assert ref['m_FileID']==0
 bindings.append(dict(name=field,path=idpaths['resources.assets:'+str(ref['m_PathID'])],owner=''))
# Original six-sprite loading character: one streamed PPtr curve, one looping state.
import math
clipObject=objects['resources.assets:49'];clip=n['tree'](clipObject);stream=clip['m_MuscleClip']['m_Clip']['data'];assert stream['m_StreamedClip']['curveCount']==1 and stream['m_DenseClip']['m_CurveCount']==0 and not stream['m_ConstantClip']['data']
binding=clip['m_ClipBindingConstant'];assert len(binding['genericBindings'])==1 and binding['genericBindings'][0]['typeID']==212 and binding['genericBindings'][0]['isPPtrCurve']==1
spriteIds=[]
for ptr in binding['pptrCurveMapping']:
 assert ptr['m_FileID']==0
 key='resources.assets:'+str(ptr['m_PathID']);obj=objects[key];tree=n['tree'](obj);png=obj['outputs']['pngCanvas'];spriteIds.append(key)
 sprites[key]=dict(id=key,name=obj['name'],path=png,sha256=hashlib.sha256((t/png).read_bytes()).hexdigest(),pivot=tree['m_Pivot'],ppu=tree['m_PixelsToUnits'],border=tree['m_Border'])
words=stream['m_StreamedClip']['data'];raw=struct.pack('<'+'I'*len(words),*words);pos=0;keys=[]
while pos<len(raw):
 time,count=struct.unpack_from('<fI',raw,pos);pos+=8
 for _ in range(count):
  index,a,b,c,value=struct.unpack_from('<I4f',raw,pos);pos+=20
  assert index==0 and a==b==c==0 and value==int(value)
  keys.append(dict(time=max(time,0),spriteId=spriteIds[int(value)]))
animation=dict(keys=keys,duration=clip['m_MuscleClip']['m_StopTime'],frameRate=clip['m_SampleRate'],loop=clip['m_MuscleClip']['m_LoopTime'],source='resources.assets:49',controllerSource='resources.assets:52',stateName='lodingSoldier',path='objs/LoadingProcess/anim',rendererSource=n['tree'](objects['resources.assets:552']))
assert len(keys)==6 and animation['duration']==.5
manifest=dict(prefabs=[dict(name='Proj_xqzdLoadingUI',rootPath=container['assetPath'],nodes=nodes,bindings=bindings)],sprites=list(sprites.values()),fonts=list(fonts.values()),animation=animation)
(t/'generated/outgame/loading-ui-import.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
(t/'generated/outgame/LOADING_UI_SOURCE_AUDIT.json').write_text(json.dumps(dict(source=container,nodes=len(nodes),bindings=bindings,pending=pending,qualification='Static source hierarchy only. Business behavior, native disabled Animation clips and matched rendering remain separate.'),ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print({'nodes':len(nodes),'bindings':len(bindings),'sprites':len(sprites),'fonts':len(fonts)})
