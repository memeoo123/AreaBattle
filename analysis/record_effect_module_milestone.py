"""Record verified effect-module milestone once; source publisher is separately idempotent."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1546 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/effect-module-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1546'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('effect-module-')]);assert len(focused['checks'])==15
focused['limitations']='Actual recovered EffectModule/Base/UI/Fly/Line lifecycle and real AssetOperationHandle with native GameObject/Canvas.15 new integrated checks and11 native. Explicit resource providers and fixture prefab;1016 original config path/duration verified, original1016 particles/resource acquisition/task binding/full EffectControl/Main/platform/all business/Player pending. Source late-load orphan and repeated line timer disposal preserved and verified.'
native=read(validation/'analysis/effect-module-native-validation.json');assert native['passed']and len(native['checks'])==11
native_log=(validation/'analysis/effect-module-native.log').read_text();assert 'error CS'not in native_log
evidence=read(out/'EFFECT_MODULE_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']),len(evidence['generic']['methods']))==(52,59,24,1)
for row in evidence['methods']+evidence['generic']['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6384 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==20
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameEffectModule.cs','Scripts/OutgameBaseEffect.cs','Scripts/OutgameEffectCollection.cs','Editor/OutgameEffectModuleValidation.cs','Editor/OutgameEffectModulePlayModeValidation.cs','Editor/OutgameLevelReuseValidation.cs','Editor/OutgameLevelReusePlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='effect-module-native-lifecycle';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Acquire and reconstruct original Effect/UI/hdzd_eff_bxGlow effect1016 prefab/particles and dependencies, compose actual modern/legacy effect/config resources with task page/liveness and collection owners. Recover complete EffectControl4058 NormalPool/sequence/fly lifecycle. Continue production Main/account/login/platform/all remaining18 controllers and business, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original EffectModule/Base/UI/Fly/Line configuration, roots, async loading, sorting, scaled timers, native tween, explicit/natural destruction and actual asset handles',checksPassed=1546,newIntegratedChecks=15,targetedChecks=15,nativeChecks=11,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=20,remainingLifecycleControllers=18,commands=['BattleBuild.ValidateMechanicsOnly','OutgameEffectModulePlayModeValidation.Run'],
 observedBoundaries=evidence['findings']+['Final integrated retains32 existing ShouldRunBehaviour assertions and one Curl35 certificate failure. Native retains one UnityEditor.Search startup ArgumentOutOfRangeException and one Curl42; product checks and compilation pass. Initial interface compilation diagnostic and native Canvas fixture assertion failures preserved; final tests use actual parent/overrideSorting premises. No original1016 visual equivalence or full production hosts claimed.'],notClaimed='Original1016 prefab/particle resources and task-page binding, EffectControl4058 lifecycle, complete resource acquisition, production Main/account/platform, remaining18 controllers/all business, fresh Player/original audiovisual equivalence.')
write(analysis/'effect-module-validation.json',focused)
for name in ('unity-integrated-validation.json','effect-module-integrated.log','effect-module-native-validation.json','effect-module-native.log','effect-module-initial-compile.log','effect-module-initial-unity-integrated-validation.json','effect-module-initial-effect-module-integrated.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'EFFECT_MODULE_AUDIT.json',dict(status='effect-module-native-lifecycle-verified-resource-and-controller-composition-pending',atUtc=now,sourceEvidence='EFFECT_MODULE_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],nativeChecks=native['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,effectModuleAudit='generated/outgame/EFFECT_MODULE_AUDIT.json');state['validation'].update(integratedChecksPassed=1546,effectModule=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='effect-module-native-lifecycle-verified-resource-and-controller-composition-pending',integratedChecks=1546,newChecks=15,nativeChecks=11,freshPlayModeRun=True,remainingLifecycleControllers=18));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1546,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for name in ('analysis/effect-module-validation.json','analysis/effect-module-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original EffectModule/Base/UI/Fly/Line source52 ordinary+1shared generic recovered and verified:1546 integrated including15 new,11 native. Actual asset handle and native timers/tween/geometry, late-load orphan and repeated line timer disposal verified. {len(fingerprints)} matching inputs;6384 normal methods, controllers20/38 unchanged. Original1016 visual resources/task binding/EffectControl/full Main/account/platform/business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1546,newChecks=15,native=11,fingerprints=len(fingerprints),indexedMethods=6384,controllers=20,remaining=18)))
