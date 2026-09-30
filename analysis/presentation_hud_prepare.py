"""Flatten recovered UI into a JsonUtility import contract; no target code execution."""
import json,hashlib
from pathlib import Path
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43';G=R/'generated';O=G/'presentation-prepared'
def read(p):return json.loads(p.read_text(encoding='utf8'))
base=read(G/'asset-evidence.json');inc=read(G/'resource-snapshots/hud-soldier300-20260928/asset-evidence-incremental.json')
objects={o['id']:o for o in base['objects']};objects.update({o['id']:o for o in inc['objects']})
resultInc=read(G/'resource-snapshots/result-ui-20260928/asset-evidence-incremental.json');objects.update({o['id']:o for o in resultInc['objects']})
h=read(G/'hud-evidence.json');play=next(p for p in h['prefabs'] if p['assetPath'].endswith('proj_xqzdplayui.prefab'));skill=next(p for p in h['prefabs'] if p['assetPath'].endswith('skillui.prefab'))
language={x['id']:x['zh_cn'] for x in read(G/'tables/LanguageConfig.json')['Datas']}
prefabs=[];sprites={};fonts={};unknown=[]
BOOL_FIELDS=set('m_RaycastTarget m_Maskable m_PreserveAspect m_FillCenter m_FillClockwise m_UseSpriteMesh m_UseGraphicAlpha m_Interactable m_ChildForceExpandWidth m_ChildForceExpandHeight m_ChildControlWidth m_ChildControlHeight m_ChildScaleWidth m_ChildScaleHeight m_ReverseArrangement m_BestFit m_AlignByGeometry m_RichText m_WholeNumbers m_WrapAround'.split())
def normalize(v):
 if isinstance(v,dict):return {k:bool(x) if k in BOOL_FIELDS else normalize(x) for k,x in v.items()}
 if isinstance(v,list):return [normalize(x) for x in v]
 return v
def add(name,nodes,bindings,rootPath):
 out=[];latest={};used=set()
 for n in nodes:
  if not n['path'].startswith(rootPath):continue
  original=n['path'][len(rootPath):].lstrip('/')
  parentOriginal=original.rsplit('/',1)[0] if '/' in original else ''
  parent=latest.get(parentOriginal,'')
  relative=(parent+'/' if parent else '')+original.split('/')[-1] if original else ''
  if relative in used:relative+='__'+n['object'].split(':')[-1].replace('-','n')
  used.add(relative);latest[original]=relative
  row={'path':relative,'name':n['path'].split('/')[-1],'sourceId':n['object'],'active':n['active'],'transform':n['transform'],'components':[]}
  row['transformSourceId']=next(x['target'] for x in objects[n['object']]['references'] if x.get('target') in objects and objects[x['target']]['type'] in ('Transform','RectTransform'))
  langKey=next((c['data'].get('key') for c in n['components'] if c.get('class',(c.get('script')or{}).get('m_ClassName',c.get('type')))=='UILangText'),None)
  for c in n['components']:
   kind=c.get('class',(c.get('script')or{}).get('m_ClassName',c.get('type')))
   if kind in ('Transform','RectTransform','CanvasRenderer'):continue
   data=c['data'];normalized=normalize(data)
   if kind=='Text' and langKey:normalized['m_Text']=language[langKey]
   item={'className':kind,'sourceId':c['id'],'data':json.dumps(normalized,ensure_ascii=False),'localizationKey':langKey if kind=='Text' else None}
   for prop,key in [('.m_TargetGraphic','targetGraphicId'),('.m_FillRect','fillRectId'),('.m_HandleRect','handleRectId')]:
    ref=next((x for x in c['references'] if x['property']==prop),None)
    if ref and ref.get('target'):item[key]=ref['target']
   for prop,key in [('.m_Sprite','sprite'),('.m_FontData.m_Font','font')]:
    ref=next((x for x in c['references'] if x['property']==prop),None)
    if not ref or not ref.get('target'):continue
    o=objects[ref['target']];t=read(R/o['outputs']['typetree']);item[key+'Id']=o['id']
    if key=='sprite':
     png=o['outputs'].get('pngCanvas',o['outputs'].get('png'))
     assert png,(o['id'],o['name'])
     sprites[o['id']]={'id':o['id'],'name':o['name'],'path':png,'sha256':hashlib.sha256((R/png).read_bytes()).hexdigest(),'pivot':t['m_Pivot'],'ppu':t['m_PixelsToUnits'],'border':t['m_Border']}
    else:
     path=o['outputs'].get('font');assert path
     fonts[o['id']]={'id':o['id'],'name':o['name'],'path':path,'sha256':hashlib.sha256((R/path).read_bytes()).hexdigest()}
     fallback=next((objects[x['target']] for x in o['references'] if x['property']=='.m_FallbackFonts[0]' and x.get('target')),None)
     if fallback:fonts[o['id']]['fallbackPath']=fallback['outputs']['font'];fonts[o['id']]['fallbackSourceId']=fallback['id']
   mat=next((x for x in c['references'] if x['property']=='.m_Material' and x.get('target')),None)
   if mat:item['materialId']=mat['target'];unknown.append({'node':n['path'],'material':mat['target'],'detail':'Explicit material binding needs separate shader audit; importer preserves original source ID and uses component default pending that audit.'})
   row['components'].append(item)
  out.append(row)
 prefabs.append({'name':name,'rootPath':rootPath,'nodes':out,'bindings':[{'name':b['name'],'path':b['path'][len(rootPath):].lstrip('/'),'owner':b.get('outletOwner',rootPath)[len(rootPath):].lstrip('/')} for b in bindings if b.get('path') and b['path'].startswith(rootPath)]})
add('TowerCanvas',play['nodes'],play['outletBindings'],'/Proj_xqzdPlayUI/StarInfoRoot/StarCanvas')
add('SkillUI',skill['nodes'],skill['outletBindings'],'/SkillUI')
add('PlayTopBar',play['nodes'],play['outletBindings'],'/Proj_xqzdPlayUI/TopRoot')
over=next(p for p in h['prefabs'] if p['assetPath'].endswith('proj_xqzdoverui.prefab'))
overRoot='/Proj_xqzdOverUI'
keep=[overRoot,overRoot+'/imageBg',overRoot+'/objBtn',overRoot+'/objBtn/go_common']
branches=[overRoot+'/go_victory',overRoot+'/objBtn/go_common/btn_normalGold2']
overNodes=[n for n in over['nodes'] if n['path'] in keep or any(n['path']==b or n['path'].startswith(b+'/') for b in branches)]
add('VictoryUI',overNodes,over['outletBindings'],overRoot)
fail=read(G/'resource-snapshots/result-ui-20260928/prefabs/proj_xqzdfailui.flat.json')
add('DefeatUI',fail['nodes'],fail['outletBindings'],fail['nodes'][0]['path'])
for name,file in [('GuideUI','guideui'),('PauseUI','proj_xqzdpauseui')]:
 p=read(G/f'resource-snapshots/hud-soldier300-20260928/prefabs/{file}.flat.json')
 add(name,p['nodes'],p.get('outletBindings',[]),p['nodes'][0]['path'])
camera=read(R/objects['level0:44']['outputs']['typetree']);camgo=objects['level0:33'];trref=next(x for x in camgo['references'] if objects[x['target']]['type']=='Transform');camtransform=read(R/objects[trref['target']]['outputs']['typetree'])
canvas=read(R/objects['level0:70']['outputs']['typetree'])
items=read(G/'tables/GameItemConfig.json')['Datas'];skillIcons=[{'skill':x['id']-2000,'name':x['ItemIcon']} for x in items if 2001<=x['id']<=2018]
dynamicNames={x['name'] for x in skillIcons}
for o in objects.values():
 if o['type']!='Sprite' or not (o['name'] in dynamicNames or o['name'].startswith('guideUI_icon')):continue
 t=read(R/o['outputs']['typetree']);png=o['outputs'].get('pngCanvas',o['outputs'].get('png'))
 if not png:continue
 sprites[o['id']]={'id':o['id'],'name':o['name'],'path':png,'sha256':hashlib.sha256((R/png).read_bytes()).hexdigest(),'pivot':t['m_Pivot'],'ppu':t['m_PixelsToUnits'],'border':t['m_Border']}
out={'target':{'appId':'wxcf1394487200e48f','version':'43'},'prefabs':prefabs,'sprites':list(sprites.values()),'fonts':list(fonts.values()),'skillIcons':skillIcons,'unknowns':unknown,'canvas':{'referenceResolution':{'x':1080,'y':1920},'match':1,'planeDistance':canvas['m_PlaneDistance'],'cameraSize':camera['orthographic size'],'cameraPosition':camtransform['m_LocalPosition'],'cameraRotation':camtransform['m_LocalRotation'],'cameraDepth':camera['m_Depth'],'nearClip':camera['near clip plane'],'farClip':camera['far clip plane']}}
(O/'hud-import.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'prefabs':[(p['name'],len(p['nodes'])) for p in prefabs],'sprites':len(sprites),'fonts':len(fonts),'explicitMaterialUnknowns':len(unknown),'canvas':out['canvas']},ensure_ascii=False))
