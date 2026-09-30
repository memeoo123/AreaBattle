"""Read-only resource integrity audit; never modifies original/cache evidence or Unity assets."""
from pathlib import Path
import collections,hashlib,json,re
W=Path(__file__).resolve().parent.parent;R=W/'analysis/targets/wxcf1394487200e48f/43';G=R/'generated';A=W/'UnityProject/Assets';D=A/'AreaBattle/Resources/Recovered'
read=lambda p:json.loads(p.read_text(encoding='utf8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
errors=[];checked={};snapshots=[]
for p in sorted((G/'resource-snapshots').glob('*/acquisition-manifest.json')):
 d=read(p);items=d.get('items',[]);count=0
 for item in items:
  if not isinstance(item,dict) or not item.get('path'):continue
  f=R/item['path'];expected=item.get('sha256')
  if not f.is_file():errors.append({'kind':'missing-acquired-bundle','path':str(f)});continue
  if f not in checked:checked[f]=sha(f)
  if expected and checked[f]!=expected:errors.append({'kind':'bundle-sha256','path':str(f),'expected':expected,'actual':checked[f]})
  if item.get('md5') and hashlib.md5(f.read_bytes()).hexdigest()!=item['md5']:errors.append({'kind':'bundle-md5','path':str(f)})
  count+=1
 snapshots.append({'manifest':p.relative_to(W).as_posix(),'checkedItems':count,'sha256':sha(p)})
native=[]
for p in sorted((G/'resource-snapshots').glob('**/native-import.json')):
 d=read(p);n=0
 for resource in d['resources']:
  for field,digest in [('path','sha256'),('sourceMesh','sourceSha256'),('templatePath',None)]:
   if not resource.get(field):continue
   f=R/resource[field]
   if not f.is_file():errors.append({'kind':'missing-prepared-resource','path':str(f)});continue
   if digest and resource.get(digest) and sha(f)!=resource[digest]:errors.append({'kind':'prepared-resource-sha256','path':str(f)})
   n+=1
 for prefab in d['prefabs']:
  f=R/prefab['templatePath']
  if not f.is_file():errors.append({'kind':'missing-native-template','path':str(f)})
 native.append({'manifest':p.relative_to(W).as_posix(),'prefabs':len(d['prefabs']),'resources':len(d['resources']),'checkedFiles':n,'sha256':sha(p)})
guidmap={}
for root in [A,W/'UnityProject/Packages',W/'UnityProject/Library/PackageCache',Path('C:/Program Files/Unity/Hub/Editor/6000.0.68f1/Editor/Data/Resources/PackageManager/BuiltInPackages')]:
 if not root.is_dir():continue
 for p in root.rglob('*.meta'):
  m=re.search(r'^guid: ([a-f0-9]{32})',p.read_text(encoding='utf8',errors='replace'),re.M)
  if m:guidmap[m.group(1)]=str(p)
references=collections.Counter();unknown={}
for p in D.rglob('*'):
 if p.suffix not in ('.prefab','.mat','.asset','.anim','.controller'):continue
 text=p.read_text(encoding='utf8',errors='replace')
 for guid in re.findall(r'guid: ["\']?([a-f0-9]{32})',text):
  references[guid]+=1
  if guid not in guidmap and not guid.startswith('0000000000000000'):unknown.setdefault(guid,[]).append(p.relative_to(W).as_posix())
 if '__GUID_' in text or '__FILEID_' in text:errors.append({'kind':'unresolved-template-token','path':str(p)})
for guid,paths in unknown.items():errors.append({'kind':'unresolved-local-guid','guid':guid,'paths':paths})
audio=read(D/'Audio/audio-runtime.json');required=read(G/'skill-audio-minimum-resources.json')['audioIds'];audioids={r['id'] for r in audio['clips']}
for id in set(required+[1001,2001,2009,2010,2012,2013])-audioids:errors.append({'kind':'missing-required-audio','id':id})
for row in audio['clips']:
 for path,expected in [(R/row['path'],row['sha256']),(D/f"Audio/{row['id']}.wav",row['sha256'])]:
  if not path.is_file() or sha(path)!=expected:errors.append({'kind':'audio-source-or-copy','id':row['id'],'path':str(path)})
minimum=read(G/'skill-presentation-minimum-resources.json')
for row in minimum['resources']:
 name=Path(row['logicalPath']).stem
 if not (D/f'SkillEffects/{name}.prefab').is_file():errors.append({'kind':'missing-required-skill-prefab','name':name})
out={'target':'wxcf1394487200e48f/43','scope':'Current confirmed default in-level consumer closure; no lobby/commerce expansion. Static filesystem/hash/reference checks, not a replacement for Unity imports or original visual baseline.','passedIntegrity':not errors,'errors':errors,'acquisitionSnapshots':snapshots,'uniqueAcquiredFilesChecked':len(checked),'nativePreparedClosures':native,'unityGuidReferencesChecked':sum(references.values()),'uniqueUnityGuids':len(references),'resourceCounts':{'prefabs':dict(collections.Counter(p.parent.name for p in D.rglob('*.prefab'))),'towerSprites':len(list((D/'Towers').glob('*.png'))),'audio':len(audio['clips']),'skills':len(minimum['resources'])},'confirmedUnwired':[{'id':'guide-persistent-tip','source':'GuideUI.textTipAdd source retained; GuideUI stage6/9/10/11/12 setters confirmed in guide-evidence','implementation':'BattleHud.Initialize forces mainbg/textTipAdd false; BattleView.OnGUI renders PersistentTip as temporary IMGUI','resourceMissing':False},{'id':'guide-pitch-stage7-8','source':'GuideUI.ShowUI f7740 has Pitch stage7/8 branch','implementation':'BattleHud.Initialize forces mainbg/guideContent/Pitch false; no later source branch in Synchronize','resourceMissing':False}],
 'unconfirmedConsumers':[{'id':'tower-buff','detail':'Older skill contract has no confirmed TowerBuff producer. This is not an established missing asset consumer.'},{'id':'boss821-unused-children','detail':'Entity821 hero_sanjiao_red/blue, skill_TM, Regions, SphereTrails are not exported as a whole root; active consumers unconfirmed. Known ice/slow/haste and shadow9034 are restored.'},{'id':'nondefault-skins-and-mode3000','detail':'Only source default soldier100/200/300 restored. Current original selected skins and GameControl +3000 flag remain unobserved; optional cosmetic catalog membership alone does not establish a required active consumer.'}],
 'historicalUnknownCorrections':[{'source':'hud-evidence:pause-guide-prefab-not-cached','now':'Resolved by hud-soldier300 snapshot and 7 HUD prefabs.'},{'source':'hud-evidence:tower-grade-offset-field','now':'Resolved by DispatchConfig grade/maxLine binding and TowerCanvas projection implementation.'},{'source':'combat-presentation-contract:2010 unavailable','now':'Resolved by failure-audio snapshot and imported Audio/2010.wav.'},{'source':'result-ui-evidence:result-entrance-animation','now':'Resolved by original default-autoplay curves, 3 components/5clips/28retainedcurves/531keys.'},{'source':'result-ui-evidence:topbar-progress-consumer','now':'Resolved by combat-hud-consumer-contract and production HudPresentationValidation.'},{'source':'boss-presentation-evidence:import/action/shadow pending','now':'Resolved by flow native import, action/rain hooks, shadow and fresh-process production checks; matched original remains open.'}],
 'remainingValidation':['Original synchronized rendered-frame comparison remains pending across all new visuals.','Native TrailRenderer temporal history in Editor-only AdvanceFrame without engine updates is not equivalent to engine runtime emission.','Source ParticleSystem autoRandomSeed retained; pixel-identical random particle frames not established.','Six constant startDelay empty min/max curves normalize to 12 dormant two-key arrays in Unity6; source active numeric values unchanged and strict diff retained.','Unity6 regenerates mipmaps from exact exported base PNGs; original compressed mip bytes remain in bundles.'],
 'artifacts':[{ 'path':p.relative_to(W).as_posix(),'sha256':sha(p)} for p in [G/'skill-presentation-contract.json',G/'guide-presentation-evidence.json',W/'analysis/guide-presentation-probe.json',W/'analysis/embedded-effect-clock-probe.json']]
}
out['completedSinceInitialAudit']=out.pop('confirmedUnwired')
for item in out['completedSinceInitialAudit']:
 item['status']='source-consumer implemented; current unified build validation pending'
 item['implementation']='RecoveredGuidePresentation popup-only tip/alignment and Pitch params; BattleHud main description clearing; IMGUI PersistentTip removed. Source event lifecycle correction recorded in guide-presentation-evidence.json.'
out['confirmedUnwired']=[]
out['pendingImplementations']=[
 {'id':'boss-action2-fireball','status':'flow implementation in progress','sourceConsumer':'Boss action2 original prefab Wy_Fire clone and 404/408 hit effects','owner':'level_flow_evidence','resourceMissing':'Exact source node is local; 404/408 are existing imported skill roots. Await implementation/import validation.'},
 {'id':'boss-ice-actual-node','status':'flow implementation in progress','sourceConsumer':'Boss ice points to entity821 skill_TM, not ordinary icicle. Ordinary tower retains icicle.','owner':'level_flow_evidence','resourceMissing':'Source node is local; new correct subtree import required.'}]
out['unconfirmedConsumers'].append({'id':'show-my-camp-effect','detail':'GuideUI.OnOK f11888@0x58bc61..0x58bd0b calls LevelControl.ShowMyCampEffect f4973 for stages1/4/5/7/8/9/10/11. Internal resource and relationship to current tower markers still need consumer closure. Do not infer from hero_sanjiao names.'})
out['unconfirmedConsumers'].append({'id':'skill14-nondefault-skin','detail':'Current StartRecruits emits EntityId1000/SkinType1, view uses soldier100; source alternative selected-skin producer is not independently confirmed. Default original asset present; nondefault variants not claimed.'})
out['unconfirmedConsumers']=[x for x in out['unconfirmedConsumers']if x['id']not in ['show-my-camp-effect','boss821-unused-children']]
out['unconfirmedConsumers'].append({'id':'boss821-remaining-unused-children','detail':'Regions and SphereTrails still lack a confirmed active consumer. Red/blue markers and skill_TM are now confirmed and handled as pending source-backed implementations, not unknowns.'})
out['pendingImplementations'].append({'id':'ordinary-and-boss-target-markers','status':'ordinary3prefab resources prepared; root/flow integration in progress','sourceConsumer':'Tower mode1 blue player/mode2 red enemy/mode0 hide; separate ShowMyCampEffect107 on guide confirmation','owner':'root + level_flow_evidence','resourceMissing':False,'evidence':'generated/tower-marker-evidence.json','nativeManifest':'generated/resource-snapshots/skill-effects-20260928/prepared-markers/native-import.json'})
out['coverage']=[
 {'area':'Ordinary towers','state':'imported and wired','assets':'72 source sprite canvases with source pivot/PPU; normal/defense/attack/arrow grade/camp variants','runtime':'RecoveredAssetImporter + BattleView'},
 {'area':'Default soldiers','state':'imported and wired','assets':'soldier100/200/300 native mesh, original packed animation textures/material tuples, run/dead ranges, camp tint','runtime':'RecoveredSoldierImporter + RecoveredSoldierVisual'},
 {'area':'Battlefield background','state':'imported and wired','assets':'HD4_CJ_1 original geometry/material plus runtime SceneSkin binding','runtime':'RecoveredAssetImporter + BattleView'},
 {'area':'Obstacles','state':'imported and wired','assets':'15 original visual prefabs and geometry/material textures; independent source collision topology','runtime':'RecoveredObstacleImporter + BattleView/BattleObstacles'},
 {'area':'Way lines and arrow/gestures','state':'imported and wired; Editor-only trail history limitation retained','assets':'2 wayline prefabs; 3 projectile/drag-circle/cut-gesture prefabs','runtime':'RecoveredWayLineImporter/RecoveredGestureImporter + RecoveredGestureVisual'},
 {'area':'18 commander skills','state':'21 roots and 3 ordinary embedded effects imported and wired','assets':'24 editable native prefabs, packed Animation/Animator, particle fields, exact reconstructed compiled custom shader formulas','runtime':'RecoveredSkillEffectImporter + BattleSkillPresentation/RecoveredEffectVisual'},
 {'area':'Bosses and Boss soldiers','state':'4 source Spine models imported/wired; action2 and actual ice subtree pending above','assets':'Boss801/802 and Soldier9001/9002, original atlas/skeleton/IK/path data, cloud/rain; entity821 subset and shadow9034','runtime':'RecoveredBossImporter/RecoveredBossVisual/RecoveredBossEmbeddedImporter'},
 {'area':'HUD, pause and results','state':'7 prefabs imported and wired','assets':'TowerCanvas/SkillUI/PlayTopBar/VictoryUI/DefeatUI/GuideUI/PauseUI; original sprites/fonts/rectangles and source Button/Slider links','runtime':'RecoveredHudImporter + BattleHud; native tower capacity and Boss slider consumers'},
 {'area':'Result entrance animation','state':'source-default autoplay restored and validated','assets':'3 Animation components/5 clips/28 retained curves/531 keys','runtime':'RecoveredUiAnimation explicit scaled clock; no invented secondary autoplay'},
 {'area':'Guide demonstration and hand','state':'Spine/hand fresh-process5checks pass; popup text/Pitch new6thcase pending unified build','assets':'YD_0 4.1.16, 47 attachment frames, 2 atlas materials; original guideHand Image; original tip/Pitch nodes','runtime':'RecoveredGuidePresentation; original scaled timing and ShowUI/OnOK lifecycle'},
 {'area':'Battle/skill/result audio','state':'38 source clips decoded and imported, required ID closure present','assets':'BGM/capture/connect/cut/arrow/skill/results2009+2010/button sounds; source AAC decoded without resampling','runtime':'BattleAudio; source fade/pause/single/parallel routes'}]
out['excludedByScope']=['Lobby, advertising, rankings, purchase and multiplied reward interfaces.','Local victory normal-claim button is an authorized continuation adapter labeled 下一关, not a claim to original currency reward integration.','Optional unused skin/theme catalog entries without an observed current in-level consumer.']
# Refresh from real integrated checks, rather than inferring completion from file counts.
validation_path=W/'analysis/unity-integrated-validation.json'
validation=read(validation_path);case_by_id={c['id']:c for c in validation['checks']}
def passed(*ids):return all(case_by_id.get(i,{}).get('result')=='pass' for i in ids)
guide_ids=['guide-original-spine-persistent-atlas','guide-production-spine-explicit-clock','guide-production-hand-move-loop-pause','guide-production-hand-pulse-completion','guide-production-stage-visibility-and-cut','guide-source-pitch-and-tip-lifecycle']
marker_ids=['skill-drag-original-all-tower-marker-modes','guide-confirm-original-player-camp-effect107','boss-source-skill-tm-ice-and-marker-bindings']
ordinary_report=read(W/'analysis/unity-tower-marker-import-report.json');boss_report=read(W/'analysis/unity-boss-embedded-import-report.json')
assert ordinary_report['passed'] and ordinary_report['prefabCount']==3 and ordinary_report['resourceCount']==8
assert boss_report['passed'] and boss_report['prefabCount']==7 and boss_report['resourceCount']==71
completed=[]
for entry in out['pendingImplementations']:
 if entry['id']=='boss-ice-actual-node' and passed('boss-source-skill-tm-ice-and-marker-bindings'):
  entry.update(status='imported and production binding validated',resourceMissing=False,validationCases=['boss-source-skill-tm-ice-and-marker-bindings']);completed.append(entry)
 elif entry['id']=='ordinary-and-boss-target-markers' and passed(*marker_ids):
  entry.update(status='imported and production consumers validated',validationCases=marker_ids);completed.append(entry)
 elif entry['id']=='boss-action2-fireball':
  fire_ids=['boss102-native-bullet-spawn-and-hit-presentation-order','boss-fire-original-clone-pool','boss-action2-production-flight-hit-retry']
  entry.update(resourceMissing=False,validationCases=fire_ids)
  if passed(*fire_ids):entry['status']='imported and production flight/hit/material/retry validated';completed.append(entry)
  else:entry.update(status='clone/pool and event order passed; production flight material validation failed',blockingCase=case_by_id['boss-action2-production-flight-hit-retry'])
out['pendingImplementations']=[x for x in out['pendingImplementations']if x not in completed]
out['completedSinceInitialAudit'].extend(completed)
if passed(*guide_ids):
 for entry in out['completedSinceInitialAudit']:
  if entry['id'].startswith('guide-'):entry.update(status='source-consumer implemented and integrated validation passed',validationCases=guide_ids)
for row in out['coverage']:
 if row['area']=='Guide demonstration and hand' and passed(*guide_ids):row['state']='source Spine, hand, popup text and Pitch all6 integrated cases passed'
 if row['area']=='Bosses and Boss soldiers':row['state']='4 models and7 embedded prefabs imported; source ice/marker consumers passed; '+('fireball flight/hit/material/retry passed' if passed('boss-action2-production-flight-hit-retry') else 'fireball material supported check failed')
out['coverage'].append({'area':'Tower skill-target markers and guide107','state':'3 ordinary prefabs/8 resources imported; all ordinary/Boss mode and107 production cases passed','assets':'hero_sanjiao_blue/red and hdzd_effect_yindao_03; Boss7-prefab closure includes correct source markers and skill_TM','runtime':'BattleSkillPresentation + source-mode input consumer and guide confirmation'})
for entry in out['unconfirmedConsumers']:
 if entry['id']=='boss821-remaining-unused-children':entry['detail']='Regions and SphereTrails still lack a confirmed active consumer. Red/blue markers and skill_TM now have imported and production-validated source consumers.'
out['integratedValidation']={'path':validation_path.relative_to(W).as_posix(),'sha256':sha(validation_path),'total':len(validation['checks']),'passed':sum(c['result']=='pass' for c in validation['checks']),'failed':[c for c in validation['checks']if c['result']!='pass'],'relevantCases':[case_by_id[i]for i in guide_ids+marker_ids+['boss102-native-bullet-spawn-and-hit-presentation-order','boss-fire-original-clone-pool','boss-action2-production-flight-hit-retry']]}
for p in [validation_path,W/'analysis/unity-tower-marker-import-report.json',W/'analysis/unity-boss-embedded-import-report.json']:
 out['artifacts'].append({'path':p.relative_to(W).as_posix(),'sha256':sha(p)})
(G/'inlevel-resource-audit.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8')
lines=['# In-level resource audit','',f'Integrity: {"PASS" if not errors else "FAIL"}. Checked {len(checked)} acquired files and {sum(references.values())} Unity GUID references. No source bundles, baseline asset-evidence or Unity assets changed by this audit.','', '## Current source-backed coverage','', '| Area | State | Assets |','|---|---|---|']
for row in out['coverage']:lines.append('| '+row['area']+' | '+row['state']+' | '+row['assets']+' |')
lines+=['','## Integrated validation','',f"{out['integratedValidation']['passed']} of {out['integratedValidation']['total']} integrated cases passed. Source reports: analysis/unity-integrated-validation.json, unity-tower-marker-import-report.json, unity-boss-embedded-import-report.json.",'','- All6 Guide cases passed, including original popup text/Pitch lifecycle.','- Ordinary marker import passed:3 prefabs/8 resources. Production all-tower marker modes, guide confirmation107 and retry cases passed.','- Boss embedded import passed:7 prefabs/71 resources,1272 packed curves/160842 curve samples. Correct skill_TM ice and marker bindings passed.','','## Pending confirmed consumers','','- Boss action2 original Wy_Fire clone/pool and spawn/hit event-order cases passed, but boss-action2-production-flight-hit-retry failed: source fire materials supported. Flow is repairing this material/shader failure. Do not equate imported files or passing pool checks with full visual validation.','','## Unconfirmed consumers and validation limits','']
if not out['pendingImplementations']:
 lines=[x for x in lines if not x.startswith('- Boss action2 original')]
 lines.insert(lines.index('## Unconfirmed consumers and validation limits')-1,'No remaining failures among the currently confirmed imported consumers. Original matched-frame visual acceptance remains separate.')
lines += ['- '+x['id']+': '+x['detail'] for x in out['unconfirmedConsumers']]
lines += ['- '+x for x in out['remainingValidation']]
lines += ['','## Historical evidence corrections','','Older standalone evidence files are preserved. Current closure resolves their previous Pause/Guide cache gap, missing audio2010, result entrance animation, topbar/Boss slider, and Boss action/rain/shadow import statements. The machine-readable audit records each correction.','','No matched original-game visual baseline acceptance is granted by this audit.']
(G/'INLEVEL_RESOURCE_AUDIT.md').write_text('\n'.join(lines)+'\n',encoding='utf8')
print(json.dumps({k:out[k] for k in ['passedIntegrity','uniqueAcquiredFilesChecked','unityGuidReferencesChecked','resourceCounts','errors']},ensure_ascii=False))
