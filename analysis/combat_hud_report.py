"""Publish evidence-backed tower capacity and Boss HUD consumers; no Unity writes."""
import json,struct
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43';OUT=ROOT/'generated'
h=json.loads((OUT/'hud-evidence.json').read_text(encoding='utf8'))
prefab=next(p for p in h['prefabs'] if any('/StarCanvas/' in n['path'] for n in p['nodes']))
nodes=[]
for n in prefab['nodes']:
 if ('/StarCanvas/' in n['path'] and any('/'+s in n['path'] for s in ['normal','defense','attack','LinePress'])) or '/ProgressSlider/' in n['path']:
  nodes.append({k:n[k] for k in ['path','active','layer','transform','components'] if k in n})
color=list(struct.unpack('<4f',struct.pack('<QQ',4533863819123296829,4575657222464717455)))
report={'target':{'appId':'wxcf1394487200e48f','version':'43'},'status':'source-consumer confirmed',
 'towerCapacity':{
  'creation':'PlayUI.f11012 enumerates active towers, excludes objects castable to Boss, then SetTowerCanvas. Boss has no ordinary TowerCanvas.',
  'source':['generated/combat-hud-disassembly/Proj_xqzdPlayUI-100666905.txt @0051516b..00515223','generated/combat-presentation-disassembly/Tower-100665419.txt @007257c8..0072583a','generated/combat-hud-disassembly/TowerCanvas-100665446.txt @0049a676..0049a89a'],
  'kindField':'Tower+60 soldierType, not camp and not grade',
  'kindRoots':{'1':'normal','2':'defense','3':'attack','other':'all three roots inactive; no capacity images registered'},
  'initialization':'Hide normal/defense/attack, register first3 child Images of selected root, additionally first child Image of each defense/attack icon, then show selected root. SetScore(0,false,-1) and SetLineNum(0,0).',
  'position':{'base':'Tower.position(+68/+72/+76) + Tower.canvasOffset(+140/+144/+148)','refresh':'base + Vector3.up * (0.1 + (DispatchConfigForGrade.maxLine - 1)*0.05)','gradeLookup':'grade0/1/else uses dispatch singleton +8/+12/+16, corresponding ordinary DispatchConfig ids1/2/3','projection':'battle camera WorldToScreenPoint, then RectTransformUtility.ScreenPointToLocalPointInRectangle with UI camera, assign canvas RectTransform anchoredPosition','source':'TowerCanvas.RefreshPos f10060 @0049a2c4..0049a3cf'},
  'lineFields':{'used':'Tower+52 outgoingLineCount','capacity':'Tower+56 maxLines'},
  'perSlot':{'range':'i=0..2 in original selected list order','active':'used > i || capacity > i','mainColor':'used > i ? cachedCampColor : Color.white','childColor':'if mainColor approximately white (squared RGBA distance < 9.99999944e-11), use static shadowRGBA; otherwise Color.white','shadowRGBA':color,'source':'TowerCanvas.SetLineNum f6995 @0028a59c..0028a71c'},
  'campColor':'LevelControl.GetCampColor(Tower.Camp). Tower.RefreshStarCanvas(refreshColor=true) calls SetScoreNum then SetLineNum then replaces cachedCampColor; preserve this call-order detail if reproducing frame-level capture transitions.',
  'geometry':'Reuse the original selected root and child rectangles; keep native HorizontalLayoutGroup centered as serialized. All3 roots anchored(0,40). normal/defense root size129.5x27.5; attack141x27.5. normal+defense main images32x32, attack40x40; inset child images22x22.',
  'linePress':'TowerCanvas.Init finds LinePress transform into+20 but no additional active consumer found in the10 TowerCanvas methods. The supplied StarCanvas prefab has no LinePress node; do not invent one.',
  'goldens':[{'used':0,'capacity':1,'active':[True,False,False],'colors':['white','white','white']},{'used':1,'capacity':2,'active':[True,True,False],'colors':['camp','white','white']},{'used':3,'capacity':1,'active':[True,True,True],'colors':['camp','camp','camp']},{'soldierType':4,'expected':'no ordinary capacity root visible'}]
 },
 'normalTopBar':{'path':'TopRoot/ProgressSlider/go_normal','visibility':'false in ordinary Refresh path, including Boss levels','source':'PlayUI.kcWa{ia f11010 @00514eba..00514ec2 unconditionally SetActive(false)','consumerAudit':'All24 PlayUI methods inspected. go_normal+188 is bound in InitializeComponent and read only by this false assignment. No original active width/proportion consumer for Progress0..4 found in this class.','serializedContent':'Five original Slider nodes remain in prefab; their existence does not authorize enabling them or inventing camp-width formulas. Preserve hidden status.'},
 'bossSlider':{
  'path':'TopRoot/ProgressSlider/slider_boss','field':'PlayUI+184','visibility':'LevelControl.isBossLevel byte+53; go_normal stays hidden','visibilitySource':'PlayUI.f11010 @00514eab..00514eea','initialization':'PlayUI.Refresh f18058 if isBossLevel: maxValue=(float)LevelControl.curBoss(+60).maxScore(+44), then value=same maxScore. Does not read current score here.','initializationSource':'generated/combat-hud-disassembly/Proj_xqzdPlayUI-100666895.txt @007a171e..007a178a',
  'updates':'Awake subscribes BossHealthChange. Handler unboxes args[0] as int and assigns Slider.value. No ratio tween, no sender/BossId check, no smoothing shown.',
  'updateSources':['PlayUI.Awake f18064 @007a2844..007a2861','PlayUI handler f54154 @0111aa6e..0111aaae','Boss.ChangeScore f68536 @014ae050..014ae05b => Boss.f31304 @00b88f55..00b88fba'],
  'eventPayload':'(int)Boss.Score(+48), truncation towardzero. Each Boss.ChangeScore sends after base.ChangeScore; shared event has no Boss identity, so any Boss can update the one slider.',
  'serializedSlider':{'min':0,'max':100,'value':100,'wholeNumbers':True,'direction':0,'anchoredPosition':[3,-33],'anchorMin':[0,1],'anchorMax':[1,1],'sizeDelta':[0,90.7242431640625]},
  'goldens':[{'maxScore':1000,'scoreAtRefresh':500,'initialSliderValue':1000},{'scoreEvent':123.9,'eventInt':123},{'scoreEvent':0.5,'eventInt':0}]
 },
 'towerVisualPoll':{'field':108,'clock':'Tower.Update receives scaled game dt; only camp!=0 accumulates','trigger':'timer >0.2, then timer-=0.2 once; no catchup while loop','reset':'Tower.Init @00725ca8 and Clear helper f6992 @00289e41 both zero timer','boss':'Boss.Update f68529 @014ace57 calls base Tower.Update; virtualslot4 dispatches Boss.f68530 instead of Tower.f15512. Boss only polls playerCommander skill5 to set speedDown+156 active; no skill4 speedUp consumer.'},
 'prefabNodes':nodes,
 'unknowns':['No active faction-width formula should be synthesized from disabled ordinary go_normal sliders.','Full native image comparison remains root validation; these rules are static source evidence.']}
(OUT/'combat-hud-consumer-contract.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'prefabNodes':len(nodes),'rules':['capacity','normal-hidden','boss-slider','visual-poll']}))
