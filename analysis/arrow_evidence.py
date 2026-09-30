"""Reproduce static ArrowTower evidence and source-derived cases; no target execution."""
import hashlib,json,math,struct,subprocess,sys
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
G=ROOT/'generated'
def w(a,b):return f'generated/wasm/wasmcode.wasm:0x{a}-0x{b}'
def fact(id,value,*refs):return dict(id=id,status='confirmed-static',value=value,evidence=list(refs))
facts=[
 fact('arrow-config',{'shipId':4,'canAddLine':False,'attackRate':[1.8,1.3,.8],'attackRegion':[.7,1,1.4],'regionBaseScale':.44,'minimumDistance':.2},w('8daaa2','8dab54'),'SettingConfig.json','ArrowTower.get_IsCanAddLine f972 constant false'),
 fact('arrow-clock',{'timer':'timer += tower-scaled dt; if timer > attackRate[grade], timer=0 and attempt one attack. No catch-up. Failed target searches also consume the attempt.','order':'Tower.Updata first, then arrow clock; no extra Camp!=0 or Tree gate in ArrowTower override.','priority':'search soldiers first; only when no soldier found search towers'},w('60a281','60a2c5'),w('60a347','60a4ab'),w('8dac03','8dac18')),
 fact('soldier-selection-order',{'choice':'first returned soldier; not nearest or random','lines':'WayLineControl list132 order; nonnull, Active12 and Direction16 !=0 required','withinLine':'large-origin list44 then small-origin list28; original insertion order within each list','campFilter':'classifies the entire directional list using its current origin endpoint camp, not individual soldier.Camp','activeFilter':'no additional soldier Active/death filter in range collector'},w('458fe8','458ff4'),w('1d6ba6','1d6bb9'),w('1d6d90','1d6da5'),w('457736','45778c')),
 fact('soldier-range',{'formula':'min*min <= squaredDistance <= range*range','inclusive':True,'minimum':.2,'zeroRange':'returns camp-filtered list without distance filtering; unused by recovered arrow config'},w('6e1d9c','6e1dae'),w('6e1ed1','6e1f16')),
 fact('tower-range-source-bug',{'formula':'sqrt(minimumParameter) <= squaredDistance <= range*range','minimumParameter':.2,'effectiveMinimumDistance':.2**.25,'inclusive':True,'note':'Preserve original f32.sqrt on minimum, not the soldier collector square. Candidate must have Active8 true and enemy camp.'},w('37a800','37a80a'),w('37a92d','37a939')),
 fact('tower-selection-random',{'collectionOrder':'LevelControl.Towers field56 order','choice':'RandomHelper generic list choice: random.Next(candidate.Count) then indexed element; not nearest','rngQualification':'The caller selects a random index; exact seeded Unity/System random parity is not asserted.'},w('56b084','56b099'),w('50f918','50f94a')),
 fact('projectile-path',{'asset':'effect/scene/hdzd_eff_SphereTrails','duration':.2,'scale':[.5,.5,.5],'euler':[0,90,0],'start':'source.position + Vector3.up * [0.2,0.24,0.28][grade]','soldierEnd':'target transform position + target transform forward * .2, captured when prefab spawn callback runs','towerEnd':'target transform position + Vector3.up * .1','homing':False,'pool':'ArrowEntityRoot / ArrowObjs, initial capacity60, lifetime10'},w('60a3a0','60a3ae'),w('3bd84b','3bd9c6'),w('69fbae','69fc51'),w('72af4b','72aff5'),'global-metadata.dat field21120, default data index80720: cdcc4c3e8fc2753e295c8f3e',w('8dab58','8dabfd')),
 fact('soldier-hit',{'effect':'OnComplete calls retained Soldier.ClearInSkill, independent of HP, distance, camp or battle state','deathGuard':'ClearInSkill skips if byte92 death-in-progress already true; otherwise immediately removes soldier from current line list100 and calls Clear(false), which starts death animation','lifetime':'No generation guard for pooled target reuse. A cleared or reused object can still be referenced by the closure; rendering/pool interleaving remains a live-test requirement.'},w('706a1a','706a24'),w('295a51','295a72'),w('4b5617','4b561b')),
 fact('tower-hit',{'effect':'retained target reference nonnull -> ChangeScore(source CURRENT camp,-1,true)','capture':False,'revalidation':'No target Active/camp/range/play-state recheck in hit closure. Source camp is read at impact, not launch.','projectile':'hidden and returned to pool at callback'},w('70754c','707591')),
 fact('arrow-clock-lifetime',{'clear':'ArrowTower.Clear only calls Tower.Clear and hides range circle; does not reset arrow timer184 or cancel projectile tween','changeCamp':'no arrow timer reset or projectile cancellation','qualification':'Exact cross-retry object reuse and asynchronous prefab-loading interleaving require original runtime validation.'},w('8dadf8','8dae0d'),'ArrowTower.ChangeCamp f20702; ArrowTower.Init inherits Tower.Init'),
 fact('projectile-time-domain',{'DOTweenSettings':'serialized defaultUpdateType0=Normal, defaultTimeScaleIndependent=false, timeScale1, defaultEaseType6=OutQuad','DOMove':'no SetUpdate override; uses scaled Time.deltaTime','pause':'GlobalData.PauseGame sets Time.timeScale0 so flight pauses','result':'non-Pause state sets Time.timeScale1; existing tween callback has no Running guard and can still hit after Victory/Defeat','frameOrder':'relative DOTweenComponent vs MineGameLogicModule update order not established; comparison at identical rendered frame pending'},w('10d565','10d67f'),w('286fce','286fd2'),w('89720e','897288'),w('493377','49339c'),'unity-assets/MonoBehaviour/DOTweenSettings__resources.bin: timeScale@0x3c=1, updateType@0x64=0, independent@0x68=false, ease@0x6c=6','flow:pause-and-update-gates')]
def f32(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def ranged(distance,tower=False,maximum=.7):
 d=f32(distance);d2=f32(d*d);min=f32(.2);r=f32(maximum)
 return d2<=f32(r*r) and d2 >= (f32(math.sqrt(min)) if tower else f32(min*min))
cases=[]
def case(id,pre,expected,rules):cases.append(dict(id=id,status='derived-not-runtime-verified',pre=pre,expected=expected,rules=rules,provenance='source-derived arithmetic and branch interpretation; target WASM not executed'))
for d in [.2,.5,.69,.7,.71]:case('arrow-range-'+str(d),{'distance':d,'grade':0,'minimum':.2},{'soldierAccepted':ranged(d),'towerAccepted':ranged(d,True)},['soldier-range','tower-range-source-bug'])
case('arrow-strict-timer',{'timer':0,'dt':[1.8,.001],'grade':0},{'attemptsAfterEachTick':[0,1],'finalTimer':0},['arrow-clock'])
case('arrow-fixed-flight-end',{'soldierPosition':[1,0,2],'soldierForward':[1,0,0],'sourceGrade':2},{'end':[1.2,0,2],'duration':.2,'sourceHeight':.28},['projectile-path'])
case('arrow-damage-no-capture',{'targetScore':.5,'targetCamp':2,'sourceCampAtLaunch':1,'sourceCampAtHit':3},{'targetScore':0,'targetCamp':2,'delta':-1,'sourceCampPassed':3},['tower-hit'])
case('arrow-first-not-nearest',{'orderedCandidateDistances':[.69,.3,.4],'range':.7},{'selectedIndex':0},['soldier-selection-order'])
case('arrow-soldier-direct-kill',{'targetHP':100000,'targetInLine':True,'deathInProgress':False},{'targetInLine':False,'deathAnimationStarted':True,'hpTestPerformed':False},['soldier-hit'])
selected=[20697,20698,20700,20701,20702,972,8258,13201,9386,5491,14737,9374,11602,7632,2673,10954,14057,15163,15588,15172,7128,2658,10315,3531,1459,9979,6967,20037,20041,9985,4379]
dis=[]
for f in selected:
 p=subprocess.run([sys.executable,str(Path(__file__).parent/'flow_disassemble.py'),'@'+str(f)],capture_output=True,encoding='utf-8',check=True)
 assert 'offset=' in p.stdout
 dis.append({'function':f,'text':p.stdout})
unknowns=[{'id':'async-prefab-and-pool-race','status':'unknown','detail':'Original asynchronous prefab availability and exact reuse identities after retry may change whether a retained callback hits a reused target; no generation guard observed.'},{'id':'engine-component-update-order','status':'unknown','detail':'DOTweenComponent relative order to game module Update not established.'},{'id':'arrow-visual-live-parity','status':'unknown','detail':'Recovered assets/config and static transforms do not establish original live visual parity.'}]
out={'target':{'appId':'wxcf1394487200e48f','version':'43'},'analysisMode':'static decode; no original target code executed','facts':facts,'derivedCases':cases,'unknowns':unknowns,'disassemblies':dis,'sourceHashes':{n:hashlib.sha256((G/'wasm'/n).read_bytes()).hexdigest() for n in ['wasmcode.wasm','wasmcode1.wasm']}}
(G/'arrow-evidence.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf-8')
spec=json.loads((G/'RESTORE_SPEC.json').read_text(encoding='utf-8'))
spec['arrowImplementation']={'ready':True,'scope':'ShipID4 in-level mechanics; static rules and source-derived goldens','evidence':'generated/arrow-evidence.json','rules':[f['id'] for f in facts],'knownGaps':unknowns,'fullGoalComplete':False}
combat=json.loads((G/'combat-evidence.json').read_text(encoding='utf-8'))
spec['skillImplementation']={'ready':True,'evidence':'generated/combat-evidence.json','coverage':'generated/combat-skill-coverage.json','scope':'18 skills confirmed static core branches and Boss998/999; only explicit recovered implementations','ruleCount':len(combat['rules']),'unknowns':combat['unknowns'],'limitations':['Exact asynchronous timing and buff injection provenance remain unknown; does not establish full battlefield coverage.'],'fullGoalComplete':False}
spec['guideImplementation']={'ready':True,'evidence':'generated/guide-evidence.json','scope':'Static tutorial0/1/2 rules, transitions, visibility, retained adjacency and persistent regen stop','fullGoalComplete':False}
(G/'RESTORE_SPEC.json').write_text(json.dumps(spec,ensure_ascii=False,indent=2),encoding='utf-8')
golden=json.loads((G/'golden-cases.json').read_text(encoding='utf-8'))
merged={x['id']:x for x in golden['cases']}
for c in combat['derivedCases']+cases:merged[c['id']]=c
golden['cases']=list(merged.values())
(G/'golden-cases.json').write_text(json.dumps(golden,ensure_ascii=False,indent=2),encoding='utf-8')
print('arrow evidence:',len(facts),'facts,',len(cases),'cases;',len(dis),'disassemblies; spec/golden appended')
