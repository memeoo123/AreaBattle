"""Evidence-backed presentation events. Reads local extracted data only."""
import json, struct
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
OUT=ROOT/'generated'
def read(p):return json.loads((OUT/p).read_text(encoding='utf8'))
def f32(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def ref(name,offset):return {'path':'generated/combat-disassembly/'+name+'.txt','offset':offset}
audio=[]
for row in read('tables/AudioConfig.json')['Datas']:
 if row['id'] not in [1001,2005,2006,2007,2008,2009,2010,2012,2013,2014,2015]:continue
 stem=Path(row['ResPath']).stem.lower()
 files=[p for p in (OUT/'unity-assets/AudioClip').glob('*') if p.name.lower().startswith(stem+'__')]
 audio.append({**row,'recovered':[{'path':p.relative_to(ROOT).as_posix(),'extension':p.suffix,'headerHex':p.read_bytes()[:16].hex()} for p in files], 'assetStatus':'recovered' if files else 'not-found-in-current-AudioClip-export'})
dead=[]
for name in ['soldier_100','soldier_200','soldier_300']:
 d=read('presentation-prepared/'+name+'.json');clip=next(x for x in d['animationData'] if x['name']=='dead')
 dead.append({'model':name,'clip':clip,'runtimeLoop':False,'removalDuration':f32(clip['lengthSeconds']-f32(.05))})
report={
 'target':{'appid':'wxcf1394487200e48f','version':'43'},'status':'static-confirmed-contract-with-explicit-unknowns',
 'death':{'trigger':'Soldier.Clear(false); ClearInSkill removes from direction list and calls Clear(false) only when Dying92=false.',
  'entryPoints':[
   {'event':'contact death','animated':True,'condition':'Either HP<=0 after simultaneous damage; independently clear each dead soldier and remove it from directional list.','source':ref('WayLine-100665501','00456b1e / 00456bba')},
   {'event':'arrow soldier hit','animated':True,'condition':'Projectile callback invokes ClearInSkill; already dying is ignored.','source':ref('Type4208-100665265','00706a1a..00706a24')},
   {'event':'skill arrow rain / poison','animated':True,'condition':'ClearInSkill on selected soldier, regardless of remaining HP.'},
   {'event':'skill1 enemy removal','animated':False,'condition':'WayLine.RemoveAllEnemySoldier passes immediate=true to table9536 Soldier.Clear.','source':ref('WayLine-100665531','006e1886..006e1890 / 006e1acf')},
   {'event':'tower arrival / invalid voyage','animated':False,'condition':'Clear(true) after damage, reinforcement, return to origin, exhausted voyage or no outgoing forwarding line.','source':ref('Tower-100665389','0072578e / 007254b2 / 0072573d')}],
  'combatEligibility':'Dying model is retained but removed from the directional list immediately. ClearInSkill checks Dying92 to reject repeat hits. Simulation Active=false and Detach represent combat eligibility; do not restore Active for animation.',
  'begin':{'clearAttachedEffects':True,'setDying':True,'animation':'dead','loop':False,'deltaLocalPosition':{'x':'UnityEngine.Random.Range(-0.1f,0.1f)','y':0,'z':'independent UnityEngine.Random.Range(-0.1f,0.1f)'},'tween':'DOBlendableLocalMoveBy','duration':'currentClip.lengthSeconds - 0.05f','clock':'Unity scaled default DOTween; no SetUpdate override in this path','ease':'OutQuad q=1-(1-t)^2; serialized DOTweenSettings.defaultEaseType6, cross-reference generated/arrow-evidence.json; no per-call SetEase','source':ref('Soldier-100665356','004b55e4..004b56b3')},
  'completion':'OnComplete Soldier.f15867 calls Clear(true): hide/null model, dispose animation player, reset Active/Dying and emit SoldierClearEvent. If model is missing at Clear(false), immediately fall back to Clear(true).',
  'clips':dead,
  'integration':'SoldierState.PlayDeathAnimation records the request. View retains only an already existing model until death duration completes; no dead model should be instantiated retrospectively.'},
 'audio':{'assets':audio,'globalGate':{'soundEnabled':'AudioControl.IsSoundEffect must be true','collisionThrottle':'2012 and 2013 each own a separate static int64 deadline. Skip if deadline > nowSeconds; otherwise set deadline=nowSeconds+1, then play. Equality permits playback.','source':ref('Raw-1201','0006631b..000663df')},
  'wallClock':{'formula':'(long)((DateTime.Now - new DateTime(1970,1,1,8,0,0)).Ticks * 1e-7)','clock':'local DateTime.Now, independent of Unity timeScale','baseConstruction':ref('Raw-15567','0072991f..00729941'),'baseInitialization':ref('Raw-15572','00729cbf..00729cc2'),'calculation':ref('Raw-3798','00123e6d..00123ee5')},
  'events':[
   {'ids':[2005,2006],'trigger':'Tower.ChangeCamp with actual oldCamp!=newCamp. Only changes involving PlayerCampID are audible: new player=>2005, lost player=>2006.','towerThrottle':'Require lastAudio168 < nowSeconds (strict); set lastAudio168=nowSeconds+1 within elapsed gate, including nonplayer changes.','callArgument':1,'source':ref('Tower-100665375','00499fd5..0049a094')},
   {'ids':[2012],'trigger':'WayLine arrival processing, if soldier camp or destination tower camp equals PlayerCampID.','callArgument':3,'source':ref('WayLine-100665501','004566af / 004568f2')},
   {'ids':[2013],'trigger':'Soldier contact only if at least one HP<=0 and either soldier camp equals PlayerCampID. One request per pair, not one per death.','callArgument':3,'source':ref('WayLine-100665501','00456a91')},
   {'ids':[2014],'trigger':'Tower grade increases (any camp); not grade reduction.','callArgument':1,'source':ref('Tower-100665420','00725fe5')},
   {'ids':[2015,2007,2008],'trigger':'Press tower / valid pair on release / cut hit, respectively.','source':'generated/controls-evidence.json: audio-feedback'},
   {'ids':[1001,2009,2010],'triggerStatus':'asset mapping provided; exact BGM/result callback placement belongs to flow/HUD evidence, not re-audited here.'}],
  'playMode':{'1':'AudioParallel: requests may overlap; UIAudioManager.parallelNode8 caches this node.','2':'AudioSequence (not used by the listed ordinary effect callsites).','3':'AudioSingle: IDs2012/2013 share singleNode16. If a current action exists, queue the new one in pending20 and call the current action OnStop; completion callback clears current16 and immediately starts pending20. Otherwise start immediately.','binding':ref('Raw-9809','00477af9 / 00477b2f / 00477b65'),'nodeCreation':ref('Raw-6271','00224e3f..00224f12'),'singlePlay':ref('Raw-20547','008cafca..008cb01c'),'singleCompletion':ref('Raw-20544','008cadce..008cae3a'),'onceStop':ref('Raw-20555','008cbacc..008cbb0f'),'parallelPlay':ref('Raw-20552','008cb601..008cb627')},
  'unknown':'Exact asynchronous clip-load ordering is not original-runtime validated; active audio mode and channel exclusivity are static-confirmed.'},
 'effects':[
  {'id':104,'res':'hit','configuredDuration':1,'trigger':'ChangeScore delta!=0 AND sourceCamp!=target camp before change AND Time.time-lastHit160>0.5f (strict). This tests source camp rather than sign of delta.','timeWrite':'Set lastHit160=Time.time after showing effect.','position':'target transform world position + Vector3.up * 0.257999986410141f','rotation':'Show overload supplies Vector3.zero Euler rotation','parent':None,'effectSortingLayer':-1,'effectOrder':-1,'source':ref('Tower-100665425','00499d58..00499e5e')},
  {'id':105,'res':'LevelUp','configuredDuration':1.5,'trigger':'new grade > old grade in CheckGradeChange, any camp.','position':'target transform world position + Vector3.up * 0.257999986410141f','rotation':'Show overload supplies Vector3.zero Euler rotation','parent':None,'effectSortingLayer':-1,'effectOrder':1,'source':ref('Tower-100665420','00725f03..00725fda')},
  {'id':106,'res':'LevelUp_B','configuredDuration':2,'triggerStatus':'no active effect caller found; not a proven grade-down effect','audit':'Opcode-validated i32.const106 scan of all mapped Assembly-CSharp.dll/GFRunning.dll functions found only ADHelper.ReportGuideState and three TextVerticalGradientThreeColor methods. Dynamic/computed IDs remain possible.'}],
 'effectScale':{'rule':'Preserve instantiated prefab localScale. Type3 BaseEffect initialization writes parent(false), localPosition and localEulerAngles but does not write localScale. Root prefab Euler angles are overwritten with zero; child transforms remain serialized.','defaultParent':'EffectModule.WorldEffectRoot from Show12437 @005bef91; caller parent null is replaced here.','initialize':ref('Type3472-100667718','00488e9c..00488fb9'),'show':ref('Raw-12437','005bef77..005bf028')},
 'effectLifetime':{'rule':'BaseEffect.Play after object readiness sets active then, for cfg.duration>0, awaits WaitForSeconds((float)duration) and calls Stop. Thus104 lasts1 scaled second and105 lasts1.5 scaled seconds after model readiness. The sprite clip duration does not replace EffectConfig.duration.','source':ref('Type3471-100667716','00487ef4..00487fa9 / 004880fb')},
 'restartTimers':{'lastHit160':'Tower.Init100665408 writes zero @00725cbd, but Unity Time.time continues across Restart: first post-restart hit is permitted immediately when application scaled time>.5.','lastAudio168':'No reset found in audited Tower Init/Clear bodies; object-pool reuse can retain this field. New allocation initializes zero.'},
 'corePayload':'score event: SourceCamp, PreviousCamp (before mutation), Amount (original delta), Value (result score), Camp (after mutation). grade event EffectId105 only on upward transition.',
 'limitations':['Async initialization/render frame order still requires original runtime validation.','106 absence is a bounded static finding, not a proof against computed/dynamic callers.','2010 configured audio has no matching current AudioClip export; do not substitute an invented clip.']}
(OUT/'combat-presentation-contract.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
md=['# Combat presentation event contract','', 'Static local evidence; target wxcf1394487200e48f/43. Full details, source offsets and asset paths are in `combat-presentation-contract.json`.','',
 'Death: contact deaths, arrow hits and ClearInSkill rain/poison retain the existing model, play dead without looping, drift locally by independent random X/Z in [-0.1,+0.1], and remove after clip length minus 0.05 seconds. Arrival and skill1 removal are immediate. Dying soldiers leave combat lists immediately.','',
 'Audio: every voice request respects IsSoundEffect. IDs2012/2013 separately throttle on integer wall seconds, permitting equality. Capture2005/2006 uses a per-tower strict inequality and only player-involved camp changes. Clock base is local 1970-01-01 08:00:00.','',
 '104: nonzero score change from a different camp, strict Time.time difference >0.5; camp comparison happens before capture. 105: grade increase only. Both are at tower position +up*0.258, zero Euler rotation. 106 has no confirmed effect caller.','',
 'lastHit160 resets during Tower.Init, but Unity Time.time does not reset on restart; no audited reset of lastAudio168. Effect initialization preserves prefab localScale, sets parent(false), localPosition and zero localEulerAngles. Type3 defaults to WorldEffectRoot. Configured duration is a scaled WaitForSeconds after the model is ready.','', '| Model | Dead clip seconds | Removal seconds |','|---|---:|---:|']
md += [f"| {x['model']} | {x['clip']['lengthSeconds']:.7f} | {x['removalDuration']:.7f} |" for x in dead]
md += ['', '| Audio ID | Config path | Export |','|---|---|---|']
md += [f"| {x['id']} | {x['ResPath']} | {x['recovered'][0]['extension'] if x['recovered'] else 'missing'} |" for x in audio]
(OUT/'COMBAT_PRESENTATION_CONTRACT.md').write_text('\n'.join(md)+'\n',encoding='utf8')
print(json.dumps({'audioAssets':len(audio),'recovered':sum(bool(x['recovered']) for x in audio),'deadClips':len(dead)}))
