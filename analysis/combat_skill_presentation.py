"""Build skill presentation handoff from static local source and recovered tables."""
import json
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'; OUT=ROOT/'generated'
def read(p):return json.loads((OUT/p).read_text(encoding='utf8'))
cfg={r['id']:r for r in read('tables/EffectConfig.json')['Datas']}
catalog={r['name'].lower():r for r in read('resource-catalog.json')['entries']}
def src(typ,token,at=None):
 p=f'generated/combat-presentation-disassembly/{typ}-{token}.txt'
 assert (ROOT/p).exists(),p
 return {'path':p,**({'offset':at} if at else {})}
def effect(i,trigger,position,**kw):
 r=cfg[i]
 return {'effectId':i,'logicalPath':f"Effect/Scene/{r['res']}.prefab",'loader':'EffectModule.Show','configuredDurationSeconds':r['duration'],'trigger':trigger,'position':position,'lifetime':f"{r['duration']} scaled seconds after model readiness" if r['duration']>0 else 'No configured timed expiry; explicit cleanup required',**kw}
def pool(name,**kw):return {'logicalPath':'effect/scene/'+name,'loader':'LoadPrefab pool (direct path; append .prefab for catalog lookup)','configuredDurationApplies':False,**kw}
skills=[]
def add(i,refs,visuals,**kw):skills.append({'skillId':i,'status':'static-confirmed; original rendering comparison pending','source':refs,'visuals':visuals,**kw})
add(1,[src('Type4166',100665106),src('Tower',100665429),src('Tower',100665383),src('Tower',100665394)],
 [{'loader':'embedded tower child','node':'objEffectRoot/Hdzd_Effect_icicle','trigger':'Execute on affected enemy towers','begin':'SetActive(true); Animator.SetInteger("skill",0)','secondary':'Skill1.End calls virtual slot8 Tower.PlayIceEffect2 on active enemies still state1, setting skill=1','end':'Tower.Clear helper calls virtual slot14 StopIceEffect to set inactive; End uses melt animation rather than immediate Stop','transform':'Preserve tower prefab child transform'}],unknowns=['Animation-controller exact transition timing is covered by native asset audit; float argument6/cdTime is ignored by PlayIceEffect.'])
add(2,[src('Type4169',100665118,'00707819'),src('Type4167',100665113),src('Type4168',100665115)],
 [pool('Hdzd_Effect_Wy_Fire',trigger='Each coroutine launch chooses an active tower',parent='SkillBase.GetSkillParent()',origin='UI item index1 world position -> UI camera screen -> set screen.z=5 -> battle camera world',endpoint='Target tower position + up*0.2',localScale=[1,1,1],motion={'component':'Bullet','arcHeight':1.5,'duration':'distance(origin, unoffset target position)/10 + 0.3','rotationSpeed':180,'lookAt':True,'autoDisable':False},lifetime='Returned to pool by arrival callback'),
 effect(404,'Arrival when target is currently friendly','Tower transform world position'),effect(408,'Arrival when target is currently hostile','Tower transform world position + up*0.2')],audioIds=[2021,3122],notes=['Arrival presentation and mechanism callback has no Running gate.'])
add(3,[src('Type4171',100665124,'006e75ce')],[effect(413,'Execute, at chosen target','Target cached position fields68/72/76')],audioIds=[4401])
for i,node in [(4,'Hdzd_Effect_Wy_speedUp'),(5,'Hdzd_Effect_Wy_speedDown')]:
 add(i,[src('Type4173' if i==4 else 'Type4174',100665128 if i==4 else 100665132),src('Tower',100665374),src('Tower',100665412)],
 [{'loader':'embedded tower child','node':'objEffectRoot/'+node,'trigger':f'Tower helper polls player SkillControl.currentCommander active({i}) AND skill.IsEffective(thisTower)','lifetime':'SetActive follows that predicate','transform':'Preserve tower prefab child transform'}],audioIds=[2042 if i==4 else 2043],notes=['This visual consumer is player Commander only. No EffectModule.Show in skill Execute.'])
add(6,[src('Type4175',100665134)],[],audioIds=[4501],notes=['No dedicated effect in Execute; score change may trigger ordinary hit/grade feedback. Fresh AI instance has no selected heal target.'])
add(7,[src('Type4179',100665140),src('Type4179',100665142,'003f7840..003f7854'),src('Type4178',100665147),src('Type4179',100665141)],
 [pool('hdzd_eff_ZHG04_01',trigger='One pooled bat for each selected enemy tower',parent='Pool parent retained; no UI SetParent in Execute or spawn callback',origin='UI item index0 projection with screen.z=5; this is only an origin lookup',endpoint='Tower transform world position',localScale='Prefab value; no explicit overwrite found',motion={'component':'Bullet','arcHeight':0,'duration':1.5,'rotationSpeed':0,'lookAt':True,'autoDisable':False},lifetime='Retained at destination; repeated Bullet arrival callbacks are possible. Hide/pool when already state4 and tower becomes friendly, or at SkillEnd.')])
add(8,[src('Type4180',100665149),src('Type4180',100665150),src('Soldier',100665349)],
 [effect(418,'Existing selected friendly soldiers and newly spawned state5 soldiers','Attached to Soldier transform through EffectCollection',parent='Soldier.get_transform()',lifetime='Explicit removal for currently friendly soldiers at SkillEnd; soldier Clear/death also clears attached effects')])
add(9,[src('Type4182',100665152,'00ea6aa3..00ea6adc / 00ea6d0c'),src('Type4182',100665151,'007a8868..007a8886'),src('Type4182',100665153)],
 [effect(421,'Execute before launching absorption projectiles','Local Vector3.zero under selected target transform (Show overload f4443)',parent='Selected target transform'),pool('hdzd_eff_ZHG04_03',trigger='One projectile from each selected active enemy/source tower',parent='SkillBase.GetSkillParent()',origin='Source tower transform world position',endpoint='Selected friendly target transform world position',localScale=[1,1,1],motion={'component':'Bullet','arcHeight':'UnityEngine.Random.value per projectile','duration':0.7,'rotationSpeed':0,'lookAt':True,'autoDisable':True},lifetime='Bullet auto-disables at arrival; heal callback requires Running and target still friendly')])
add(10,[src('Type4186',100665174),src('Type4184',100665169),src('Type4185',100665171)],
 [pool('hdzd_eff_ZHG03_01',trigger='Two pooled visual rain objects per inner iteration; args0*0.5 iterations; interval2/args0',parent='SkillBase.GetSkillParent()',origin='(Random.Range(-20,20)/20, 10, Random.Range(-40,40)/15 + 3)',localScale=[4,4,4],motion={'component':'DOTween.DOMoveY','endpointY':0,'duration':1,'ease':'default OutQuad','clock':'Unity scaled'},lifetime='Land callback removes first enemy soldier within0.15, then hides/pools object',matchingConfigId=415)],notes=['No separate land effect confirmed. Config415 duration5 is bypassed by direct pooling.'])
add(11,[src('Type4189',100665184),src('Type4189',100665185),src('Soldier',100665349)],
 [effect(420,'Existing affected soldiers and newly spawned state6 soldiers','Soldier transform via EffectCollection',parent='Soldier.get_transform()',effectOrder=-9999,lifetime='No configured expiry. SkillEnd resets tower state88 only; no soldier-effect removal in its body. Soldier Clear/death clears attachments.')],notes=['-9999 is effectOrder sentinel, not duration.'])
add(12,[src('Type4190',100665194,'00512d13 / 00512f18'),src('Type4191',100665191),src('Type4191',100665193)],
 [effect(417,'Async Execute starts','Target tower position + up*0.7'),effect(419,'After WaitForSeconds0.8','Target cached position68/72/76')],audioIds=[2032,2033],notes=['SkillEnd clears target/timer but does not close these effects. Reset explicitly closes stored handles52/56.'])
add(13,[src('Type4193',100665201),src('Type4193',100665200)],
 [effect(511,'SoldierInit callback for newly spawned friendly soldier','Soldier transform via EffectCollection',parent='Soldier.get_transform()',lifetime='SkillEnd only unsubscribes; existing attachment persists until soldier Clear/death')])
add(14,[src('Type4194',100665209,'00708169 / 0070838f'),src('Type4198',100665220),src('Type4198',100665219),src('Type4198',100665221)],
 [{'loader':'EntityModelConfig via LoadPrefab.GetEntityNow','entitySelection':'PlayerControl.GetSkinEntityByType(1)','defaultEntityId':1000,'defaultAsset':'soldier_100','pathEnum':10,'trigger':'Recruit coroutine','origin':'Random annulus around target radius0.3..0.5, world y=0','localScale':[0.05,0.05,0.05],'tint':'SetSoldierColor(skillCamp)','animation':{'name':'run','loop':True},'orientation':'Rendering localEuler = (-camera.localEulerAngles.x,180,0) when spawn.x < target.cachedX; otherwise (cameraX,0,0). Movement direction separately uses horizontal normalization.','motion':'Special DeadSoldier moves using0.5*WayLine movement multiplier','lifetime':'Special unit hits target and deactivates; ordinary Soldier dead animation does not govern this unit'},effect(521,'At recruited unit spawn','Spawn world position')],unknowns=['All optional player skin choices are not the minimum resource set; bind the selected skin dynamically.'])
add(15,[src('Type4196',100665216)],
 [effect(532,'Start friendly tower serial score job','Target cached position68/72/76'),effect(531,'Start enemy tower serial score job','Target cached position68/72/76')],notes=['Effect duration5 is independent from repeated +/-1 score task. Old task can survive Restart with its original Tower reference; see lifecycle evidence.'])
add(16,[src('Type4200',100665225),src('Type4200',100665227)],
 [effect(611,'SoldierInit callback for newly spawned enemy soldier','Soldier transform via EffectCollection',parent='Soldier.get_transform()',lifetime='SkillEnd only unsubscribes; attachment persists until soldier Clear/death')])
add(17,[src('Type4202',100665228,'006dbf71'),src('Type4202',100665231),src('Type4201',100665234)],
 [effect(612,'Execute snapshots friendly towers','Each tower cached position68/72/76',lifetime='Configured10 scaled seconds; additionally close when snapshot tower camp changes during a pass and at SkillEnd. Do not extend effect lifetime to skill duration.')])
add(18,[src('Type4205',100665242,'006dd3da..006dd4be / 006dd51e / 006dd532'),src('Type4205',100665245),src('Type4205',100665239,'006dcf66..006dcf8e'),src('Type4205',100665243,'006dcc88..006dccff'),src('Type4203',100665248,'006460ac..006460c1'),src('Type4203',100665249),src('Type4204',100665251),src('Type4205',100665240)],
 [{'effectId':633,'logicalPath':f"Effect/Scene/{cfg[633]['res']}.prefab",'loader':'Manual asset load/instantiate, not EffectModule.Show','trigger':'DragBegin; follows DragMove ground point','localScale':[0.25,0.25,0.25],'lifetime':'DragEnd hides; Reset destroys indicator100'},
 pool('hdzd_eff_wyys04',trigger='Execute',parent='SkillBase.GetSkillParent()',origin='UI item index2 -> UI camera screen; enemy adds Screen.height to screen.y; set screen.z=5 -> battle camera world',endpoint='Chosen skill center',localScale=[150,150,150],motion={'component':'Bullet','arcHeight':1.5,'duration':1,'rotationSpeed':180,'lookAt':True,'autoDisable':False},lifetime='Landing callback hides/pools bottle',matchingConfigId=632),
 {'effectId':631,'logicalPath':f"Effect/Scene/{cfg[631]['res']}.prefab",'loader':'Manual async asset load/instantiate, not EffectModule.Show','configuredDurationApplies':False,'trigger':'Load starts at Execute; instantiate inactive; activate at bottle landing','position':'Chosen skill center','localScale':[1,1,1],'particles':'GetChild(0).GetComponentsInChildren<ParticleSystem>(); each main.simulationSpeed=7/(cdTime-1)','lifetime':'SkillEnd hides ground68; Reset destroys it. Config631 duration12 does not govern this manual object.'}],audioIds=[2039,2040,2041],notes=['Launch2039; landing2040 and parallel2041; stored2041 playback handles stop at SkillEnd. Async ground readiness relative to bottle landing is an original runtime race; do not synthesize ground before load.'])

resources={}
for sk in skills:
 for v in sk['visuals']:
  path=v.get('logicalPath')
  if not path:continue
  canonical=path.lower()
  if not canonical.endswith('.prefab'):canonical+='.prefab'
  cat=catalog.get(canonical+'.unity3d')
  assert cat,canonical
  r=resources.setdefault(canonical,{'logicalPath':canonical,'catalog':cat,'skills':[],'effectIds':[],'note':'Catalog cached flag is original inventory status, not a current presence check. Bundle dependencies must also be resolved.'})
  if sk['skillId'] not in r['skills']:r['skills'].append(sk['skillId'])
  if v.get('effectId') and v['effectId'] not in r['effectIds']:r['effectIds'].append(v['effectId'])
report={'target':{'appid':'wxcf1394487200e48f','version':'43'},'scope':'All18 active ordinary commander skill visual branches; Boss models are separate tower-entity binding work.',
 'common':{'managedEffect':'Type3 defaults parent to WorldEffectRoot, sets localPosition and zero localEulerAngles; preserves prefab localScale unless caller overrides. Positive duration uses scaled WaitForSeconds after model readiness.','attachedEffect':'EffectCollection parent is soldier transform. Preserve source prefab children/materials/particle settings; lifecycle is not inferred from visual loops.','bullet':'Init parameter order(start,end,arcHeight,duration,callback,rotationSpeed,lookAt,autoDisable). See recovered Bullet behavior before rendering trajectory.','clock':'Skill timers and coroutine/particle lifetime are distinct; use specific source clocks. No per-skill unscaled tween override confirmed here.'},'skills':skills,'minimumResources':list(resources.values()),
 'embeddedResources':[{'skills':[1,4,5],'source':'Already required tower prefab family','children':['objEffectRoot/Hdzd_Effect_icicle','objEffectRoot/Hdzd_Effect_Wy_speedUp','objEffectRoot/Hdzd_Effect_Wy_speedDown']}],
 'dynamicEntityResources':[{'skill':14,'table':'EntityModelConfig','lookup':'PlayerControl.GetSkinEntityByType(1)','default':next(r for r in read('tables/EntityModelConfig.json')['Datas'] if r['id']==1000),'status':'Default soldier_100 already recovered; optional skins require their actual selected mapping.'}],
 'excludedConfigOnly':[{'id':i,'reason':'No active direct EffectModule caller confirmed; do not add solely because present in configuration.'} for i in [403,416]],
 'unknowns':['Original runtime visual parity, shaders/material blend behavior and particle-system timing still need validation.','TowerBuff producer is unconfirmed, so no invented buff asset binding. Known skills8/11/13/16 attached effects are explicitly listed.','Boss Ship11..15 model/animation binding is outside this skill-only minimum list and remains a separate resource requirement.']}
(OUT/'skill-presentation-contract.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
report['common']['bulletOrientation']={'init':'Only arcHeight!=0 and lookAt input true calls Transform.LookAt(end), once at Init. Arc0 branches exit before LookAt, even if input true.','update':'When arcHeight!=0, Transform.Rotate(axis * Time.deltaTime * rotationSpeed); Euler-vector overload defaults Space.Self. axis=Quaternion.AngleAxis(-90,Vector3.up)*(end-start); axis is NOT normalized. No per-frame tangent LookAt.','sources':[src('Bullet',100664289,'001729d3 / 00172c6e / 00172c8f / 00172d2c'),src('Bullet',100664290,'005f4dc9..005f4e4e'),src('Bullet',100664285,'008c369a..008c3714')]}
report['presentationApi']={'source':'UnityProject/Assets/AreaBattle/Scripts/BattleSkills.cs','event':'BattleSimulation.SkillVisual : Action<SkillVisualEvent>','coordinateRule':'PositionIsLocal=true for tower/soldier attached effects, otherwise world. Parent conveys attachment.','managedHandles':'Positive VisualId identifies effects/projectiles/recruits; attached effects keyed(SoldierId,EffectId), poison ground keyed(18,Camp).','mechanismOwnership':'Existing SkillProjectiles and RecruitedSoldiers remain sole mechanism entities. Visual positions read existing entities; Skill10 SkillProjectileVisualPosition applies OutQuad without changing mechanical arrival. No extra random draws.','delayedVisuals':'Separate visual wait queue in TickSkillCoroutines emits skill12 hit419 after0.8 and skill18 landing after1; never changes score. Subscriber exceptions isolated.','resetLimitation':'battle-reset is host reconstruction notification. Global managed EffectModule cleanup on ordinary retry not proven; clearall in host is a known unvalidated presentation difference. SkillBase.Reset only clears active/time; skill12.Reset explicitly closes2effects, skill18.Reset destroys manualindicator/ground.','resetAudit':'generated/combat-visual-lifecycle-references.json','validation':'5new production-event cases added to SkillValidation; total50 awaiting root Unity run.'}
if (OUT/'skill-audio-contract.json').exists():
 audio=read('skill-audio-contract.json');report['audioContract']='generated/skill-audio-contract.json'
 for s in report['skills']:
  s['audioIds']=sorted({i for r in audio['events'] if r['skillId']==s['skillId'] for i in r['audioIds']})
  if s['skillId']==18:s['notes']=[x.replace('Launch2039','Launch3121') for x in s.get('notes',[])]
(OUT/'skill-presentation-contract.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
(OUT/'skill-presentation-minimum-resources.json').write_text(json.dumps({'target':report['target'],'resources':report['minimumResources'],'embeddedResources':report['embeddedResources'],'dynamicEntityResources':report['dynamicEntityResources']},ensure_ascii=False,indent=2),encoding='utf8')
entities={r['id']:r for r in read('tables/EntityModelConfig.json')['Datas']}
soldiers={r['id']:r for r in read('tables/SoldierConfig.json')['Datas']}
def entity_binding(i):
 row=entities.get(i)
 if row is None:return {'entityId':i,'status':'missing EntityModelConfig row; no resource name inferred'}
 suffix='/'+row['AssetName'].lower()+'.prefab.unity3d'
 return {'entityId':i,'config':row,'catalogMatches':[c for name,c in catalog.items() if name.endswith(suffix)]}
boss=[]
for c in read('tables/CampConfig.json')['Datas']:
 if not c['bossModelId']:continue
 soldier=soldiers[c['bossSoldierId']]
 boss.append({'camp':c['id'],'tower':entity_binding(c['bossModelId']),'soldierConfig':soldier,'soldierEntity':entity_binding(soldier['EntityID'])})
(OUT/'combat-boss-presentation-bindings.json').write_text(json.dumps({'target':report['target'],'source':'generated/combat-evidence.json rule boss-init-and-soldier; Boss.Init token100665274; CampConfig/SoldierConfig/EntityModelConfig','bindings':boss,'scopeWarning':'Camp configuration links only. Scene reachability/factory selection and fallback for missing rows are not inferred. Catalog matches do not prove actual object load.'},ensure_ascii=False,indent=2),encoding='utf8')
lines=['# Skill presentation contract','','18 ordinary skills: static calls, transforms, lifetimes and minimum asset bindings are in `skill-presentation-contract.json`. The resource-only handoff is `skill-presentation-minimum-resources.json`.','','21 unique external effect prefab bundles; dependencies still required. Skills1/4/5 use embedded tower children. Skill6 has no dedicated visual. Skill14 uses current player type1 skin, default entity1000 soldier_100.','','| Skill | External effect / direct pooled prefab |','|---|---|']
for s in skills:
 names=[str(v.get('effectId') or v.get('logicalPath','').split('/')[-1]) for v in s['visuals'] if v.get('logicalPath')]
 lines.append(f"| {s['skillId']} | {', '.join(names) or ('embedded tower children' if s['skillId'] in [1,4,5] else 'none dedicated')} |")
lines+=['','Direct pooled objects2/7/9/10/18 do not use similarly named EffectConfig expiry. Skill18 ground631 and drag633 are manually instantiated too. Attached skills13/16 effects survive skill end until soldier clear; skill11 End resets tower state but does not clear existing soldier effects.','', 'Boss bindings are separately recorded in `combat-boss-presentation-bindings.json`: camps5/6 models801/802, camps7..9 reference missing entity rows803..805. Soldiers11/12 use entities9001/9002;13..15 use3008. Missing rows are not replaced with invented models.']
(OUT/'SKILL_PRESENTATION_CONTRACT.md').write_text('\n'.join(lines)+'\n',encoding='utf8')
print(json.dumps({'skills':len(skills),'minimumBundles':len(resources),'sourceRefs':sum(len(s['source']) for s in skills)}))
