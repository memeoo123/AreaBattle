"""Compact HUD bindings and original serialized fields, retaining full prefab/bytecode evidence."""
import json,sys,struct
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43';p=ROOT/'generated/hud-evidence.json';r=json.loads(p.read_text(encoding='utf8'))
assets=json.loads((ROOT/'generated/asset-evidence.json').read_text(encoding='utf8'));objs={o['id']:o for o in assets['objects']}
def fn(i):
 f=next(f for f in r['functions'] if f['function']==i)
 return {k:f[k] for k in ('module','function','token','class','method','body')}
def prefab(name):return next(p for p in r['prefabs'] if p['assetPath'].endswith('/'+name+'.prefab'))
play=prefab('proj_xqzdplayui');skill=prefab('skillui');over=prefab('proj_xqzdoverui')
def node(pre,name):return next(n for n in pre['nodes'] if n['path']==name)
score=node(play,'/Proj_xqzdPlayUI/StarInfoRoot/StarCanvas/ScoreNum')
txt=next(c for c in score['components'] if c['class']=='Text');font=next(ref['target'] for ref in txt['references'] if ref['property']=='.m_FontData.m_Font')
r['summary']={
 'towerScore':{'node':score['path'],'object':score['object'],'transform':score['transform'],'textComponent':txt,'outline':next(c for c in score['components'] if c['class']=='Outline'),'font':objs[font]},
 'skillBar':{'root':node(skill,'/SkillUI/main'),'layout':node(skill,'/SkillUI/main/layout'),'template':node(skill,'/SkillUI/SkillItem'),'bindings':skill['outletBindings']},
 'pauseButton':node(play,'/Proj_xqzdPlayUI/TopRoot/LeftContainer/PauseBtn'),
 'playBindings':play['outletBindings'],
 'resultBindings':over['outletBindings'],
 'resultHierarchy':[{'path':n['path'],'object':n['object'],'active':n['active'],'transform':n['transform']} for n in over['nodes']],
 'projectileOrigin':{'formula':'BattleCamera.ScreenToWorldPoint(new Vector3(UIcamera.WorldToScreenPoint(SkillItem[index].transform.position).x, UIcamera.WorldToScreenPoint(SkillItem[index].transform.position).y, 5))','skill2ItemIndex':1,'skill7ItemIndex':0,'skill2Evidence':{'source':'generated/combat-disassembly/Type4169-100665118.txt','offsets':['0070767c','00707687','0070768a','00707711','00707734','00707763','00707782']},'skill7Evidence':{'source':'generated/combat-disassembly/Type4179-100665142.txt','offsets':['003f76aa','003f7700','003f7750']}}}
rows=json.loads((ROOT/'generated/resource-catalog.json').read_text(encoding='utf8'))['entries']
r['missingLocalCatalogRows']=[x for x in rows if any(k in x['name'].lower() for k in ('pauseui','guideui')) and not x['cached']]
canvas=[]
for ident in ('level0:121','resources.assets:1465'):
 o=objs[ident];raw=(ROOT/o['outputs']['raw']).read_bytes()
 scaleMode,ppu,scale,width,height,matchMode,match=struct.unpack_from('<iff2fif',raw,32)
 canvas.append({'object':ident,'source':o['source'],'raw':o['outputs']['raw'],'serializedPayloadOffset':32,'scaleMode':scaleMode,'referencePixelsPerUnit':ppu,'scaleFactor':scale,'referenceResolution':[width,height],'screenMatchMode':matchMode,'matchWidthOrHeight':match,'provenance':'original serialized CanvasScaler payload; field order corroborated by local Unity CanvasScaler.cs and original script reference globalgamemanagers.assets:200'})
r['summary']['canvasScalers']=canvas
r['summary']['skillBar']['derivedThreeItemCenters']={'space':'1080x1920 design coordinates, bottom-left origin; derived from serialized layout, before runtime adaptation','centers':[[465.48915,154],[675.39999,154],[885.31084,154]],'derivation':'x=1080/2+135.399994+(-1,0,1)*629.732544/3; y=29+250/2'}
rule=lambda id,text,*f:dict(id=id,status='confirmed-static',rule=text,evidence=[fn(x) for x in f])
r['rules']=[
 rule('score-rendering','TowerCanvas.SetScoreNum renders Max for max flag; otherwise max(integerScore,0).ToString(). Debug suffix flag defaultsfalse. Source score text uses original HYZhuZiMuTouRenW font, fontsize40,bestfit10..40,alignmentMiddleCenter,white,outlineblack distance(2,-2),UseGraphicAlpha=false. Rect anchors/pivot(.5,.5),size(100,42.85267),anchored(0,85.6). StarCanvas itself is dynamically positioned; do not interpret85.6 as world units.',6994,15524),
 rule('tower-ui-projection','TowerCanvas.RefreshPos stores baseworldpoint+Vector3.up*.1+Vector3.up*(gradeConfigField16-1)*.05, projects with GameSceneControl camera WorldToScreenPoint, converts via RectTransformUtility.ScreenPointToLocalPointInRectangle using UI canvas worldCamera, assigns anchoredPosition. Semantic identity of gradeConfigField16 remains pending.',10060),
 rule('skill-clone-order','SkillUI creates SkillItem[] from ConfigMgr field196 row keyed SkillControl.mode(+108), row field36 skill-id array. Each item clones the offscreen SkillItem template, parents under layout, initializes the skill-id and field184 configrow; SkillControl.AddItem keeps exact arrayindex order. Separate SkillUI is not a child of PlayUI.',10345,10352),
 rule('skill-layout','SkillUI main anchored(0,29),size918x250; child layout anchored(135.4,0),size629.73254x182. HorizontalLayoutGroup center alignment4,zero padding/spacing,ChildForceExpandWidth/Height=true,ChildControlWidth/Height=false. Template size156x156,pivot(.5,.5). Three equal layout slots are ~209.91085 wide; compute actual RectTransform positions after layout, not hardcoded screen percentages.'),
 rule('canvas-reference','Loaded level0 UICanvas CanvasScaler has reference1080x1920,ScaleWithScreenSize1,MatchWidthOrHeight0,match1(height). Resources UICanvas template uses same reference but match0(width). Loaded level0 Canvas is ScreenSpaceCamera1,CameraPathID44. Any runtime scaler override remains unverified; preserve this distinction.'),
 rule('projectile-origin','Skill2 reads item index1 transform.position; skill7 reads item index0. Both project through UIcamera.WorldToScreenPoint, replace screenz with5, then battlecamera.ScreenToWorldPoint. Native helper ids confirmed2198 WorldToScreenPoint,2610 ScreenToWorldPoint,1112 Transform.get_position. Source formula and offsets are in summary.projectileOrigin.'),
 rule('pause-bindings','PauseUI.InitializeComponent binds imageBg,RunBtn,btn_home,btn_sound,btn_shock,btn_again,btn_music,main,img_sound_on/off,img_music_on/off,img_shock_on/off. Exact PauseUI prefab hierarchy is unavailable locally; method bindings alone cannot establish layout.',18068),
 rule('result-prefab','Original Proj_xqzdOverUI locally recovered with210 nodes and125 UIOutlet bindings. Complete serialized active state,RectTransforms,Image/Text/Button components,sprite/font dependencies retained under prefabs and summary.resultHierarchy/resultBindings. Do not assume every prefab branch is visible in every result mode.')]
r['unknowns']=[{'id':'pause-guide-prefab-not-cached','blocking':'exact-ui-appearance','details':'Exact missing catalog rows listed; PauseUI bindings known but no local prefab hierarchy.'},{'id':'live-original-hud','blocking':'visual-acceptance','details':'No matched live screenshot. Prefab defaults may be overridden at runtime; dynamic viewport/canvas reference resolution and all result mode branches remain to validate.'},{'id':'tower-grade-offset-field','blocking':'exact-score-worldplacement','details':'RefreshPos singleton3953328 grade row field16 is not semantically named yet; original arithmetic retained.'}]
r['validation']={'status':'static-prefab-bindings-and-bytecode','targetCodeExecuted':False,'prefabCount':len(r['prefabs']),'functionCount':len(r['functions']),'rules':len(r['rules'])}
p.write_text(json.dumps(r,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'validation':r['validation'],'missing':r['missingLocalCatalogRows'],'resultRootChildren':[n['path'] for n in over['nodes'] if n['path'].count('/')<=3]},ensure_ascii=False,indent=2))
