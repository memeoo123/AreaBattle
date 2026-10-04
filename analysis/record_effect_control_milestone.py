"""Publish verified EffectControl registry/pool/sequence/native account milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1570 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/effect-control-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1570'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('effect-control-')]);assert len(focused['checks'])==16
native=read(validation/'analysis/effect-control-native-validation.json');assert native['passed']and len(native['checks'])==14 and native['loadedIcons']==10 and native['returnedIcons']==10
native_log=(validation/'analysis/effect-control-native.log').read_text();assert 'AREABATTLE_EFFECT_CONTROL_NATIVE_PASS checks=14'in native_log and 'error CS'not in native_log
focused['limitations']=native['scope']
evidence=read(out/'EFFECT_CONTROL_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(45,32,9)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6384 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==21
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameEffectControl.cs','Scripts/OutgameNormalPool.cs','Scripts/OutgameFlyToolCleanup.cs','Scripts/OutgameCoreControllerBindings.cs','Scripts/OutgameUiControl.cs','Scripts/OutgameTopInfoPage.cs','Editor/OutgameEffectControlValidation.cs','Editor/OutgameEffectControlPlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='effect-control-pool-sequence-native-account';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover full original ObjectPoolManager module/factory registration and connect actual frame startup/resource ownership in place of local manager acquisition host; complete UIControl/TopInfo account bindings, full resource catalog/Main/account/login/platform, remaining17 controllers/all business and final Player/original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original EffectControl4058 registry/LogicModule/NormalPool creation/sequence/fly cleanup with actual UIControl text caches and native task/account rewards',checksPassed=1570,newIntegratedChecks=16,targetedChecks=16,nativeChecks=14,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=21,remainingLifecycleControllers=17,commands=['BattleBuild.ValidateMechanicsOnly','OutgameEffectControlPlayModeValidation.Run'],
 observedBoundaries=evidence['findings']+['Final integrated retains32 existing ShouldRunBehaviour assertions and one Curl35 certificate failure. Native retains one UnityEditor.Search startup ArgumentOutOfRangeException, one Curl35 and one Curl42; product checks and compilation pass. Initial tween delegate compile error and native incorrect dual-record expectation retained; corrected expected source ItemManager50/LocalData57 and local diamonds3 without changing production reward rules. Original task/TopInfo/currency/particle visible native capture retained; no original-frame audiovisual equivalence claimed.'],notClaimed='Full original ObjectPoolManager startup/resource acquisition, complete TopInfo account UI, production Main/account/platform, remaining17 controllers/all business, fresh Player/original audiovisual equivalence.')
write(analysis/'effect-control-validation.json',focused)
for name in ('unity-integrated-validation.json','effect-control-integrated.log','effect-control-native-validation.json','effect-control-native.log','effect-control-initial-compile.log','effect-control-initial-effect-control-native.log','effect-control-initial-effect-control-native-validation.json'):shutil.copy2(validation/'analysis'/name,analysis/name)
shutil.copy2(validation/'analysis/captures/effect-control-task-native.png',analysis/'captures/effect-control-task-native.png')
write(out/'EFFECT_CONTROL_AUDIT.json',dict(status='effect-controller-lifecycle-native-account-verified-full-manager-production-pending',atUtc=now,sourceEvidence='EFFECT_CONTROL_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,effectControlAudit='generated/outgame/EFFECT_CONTROL_AUDIT.json');state['validation'].update(integratedChecksPassed=1570,effectControl=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='effect-controller-lifecycle-native-account-verified-full-manager-production-pending',integratedChecks=1570,newChecks=16,nativeChecks=14,freshPlayModeRun=True,remainingLifecycleControllers=17));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1570,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for name in ('analysis/effect-control-validation.json','analysis/effect-control-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original EffectControl registry/LogicModule, NormalPool creation, source reinit/dispose/sequence/fly ownership and lazy UI currency Text getters:1570 integrated including16 new,14 native with real task/account/currency/effect1016; source ItemManager50/LocalData57/diamonds3 restart verified. {len(fingerprints)} matching inputs;6384 normal methods, controllers21/38. Full ObjectPoolManager/resource catalog/Main/account/platform/business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1570,newChecks=16,native=14,fingerprints=len(fingerprints),indexedMethods=6384,controllers=21,remaining=17)))
