"""Publish verified original ObjectPoolManager / Frame ownership milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1581 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/pool-manager-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1581'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('pool-manager-')]);assert len(focused['checks'])==11
native=read(validation/'analysis/pool-manager-native-validation.json');assert native['passed']and len(native['checks'])==16 and native['loadedIcons']==10 and native['returnedIcons']==10
native_log=(validation/'analysis/pool-manager-native.log').read_text();assert 'AREABATTLE_POOL_MANAGER_NATIVE_PASS checks=16'in native_log and 'error CS'not in native_log
focused['limitations']=native['scope']
evidence=read(out/'POOL_MANAGER_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['generic']['methods']),len(evidence['contexts']),len(evidence['usages']))==(11,5,4,4)
for row in evidence['methods']+evidence['generic']['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6384 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==21
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameObjectPoolManager.cs','Scripts/OutgameObjectPool.cs','Editor/OutgameObjectPoolManagerValidation.cs','Editor/OutgamePoolManagerPlayModeValidation.cs','Editor/OutgameEffectControlValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='object-pool-manager-native-frame-ownership';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Complete TopInfoUI account/actions and UIControl production bindings, recover unresolved GuideControl4065 and remaining17 controller lifecycle/business dependencies for real Main/lobby/battle startup. Continue full resource catalog/account/login/platform/all business and final Player/original audiovisual acceptance.'
def diagnostics(s):return {k:s.count(k)for k in ('ShouldRunBehaviour','Curl error 35','Curl error 42','ArgumentOutOfRangeException','error CS')}
record=dict(atUtc=now,scope='Original ObjectPoolManager3672 normal/shared-generic factory/lifecycle plus real Frame70/Logic12/EffectControl/NormalPool ownership',checksPassed=1581,newIntegratedChecks=11,targetedChecks=11,nativeChecks=16,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=21,remainingLifecycleControllers=17,commands=['BattleBuild.ValidateMechanicsOnly','OutgamePoolManagerPlayModeValidation.Run'],logDiagnostics=dict(integrated=diagnostics(log),native=diagnostics(native_log)),
 observedBoundaries=evidence['findings']+['Final integrated/native source compilation and product checks pass; exact environment log diagnostic counts retained above. Initial duplicate framework-exception declaration compile failure retained; final code reuses existing recovered OutgameFrameworkException. Native original task/particle/currency/account path runs through real Frame.Update and verifies unscaled automatic pool expiry, then reverse Frame.Shutdown. No whole-production/Main/Player/original audiovisual claim.'],notClaimed='Complete production module resource acquisition, TopInfo account UI, Main/account/platform, remaining17 controllers/all business, fresh Player/original audiovisual equivalence.')
write(analysis/'pool-manager-validation.json',focused)
for name in ('unity-integrated-validation.json','pool-manager-integrated.log','pool-manager-native-validation.json','pool-manager-native.log','pool-manager-initial-compile.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
shutil.copy2(validation/'analysis/captures/pool-manager-task-native.png',analysis/'captures/pool-manager-task-native.png')
write(out/'POOL_MANAGER_AUDIT.json',dict(status='original-pool-manager-native-frame-verified-full-production-pending',atUtc=now,sourceEvidence='POOL_MANAGER_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,poolManagerAudit='generated/outgame/POOL_MANAGER_AUDIT.json');state['validation'].update(integratedChecksPassed=1581,poolManager=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-pool-manager-native-frame-verified-full-production-pending',integratedChecks=1581,newChecks=11,nativeChecks=16,freshPlayModeRun=True,remainingLifecycleControllers=17));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1581,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for name in ('analysis/pool-manager-validation.json','analysis/pool-manager-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original ObjectPoolManager3672 recovered with11 normal/5shared bodies/4rgctx ranges and real Frame/Logic/EffectControl/NormalPool composition.1581 integrated including11 new,16 native with actual task/account/particle/currency, paused real-time pool expiration and reverse Frame shutdown. {len(fingerprints)} matching inputs;6384 normal methods, controllers21/38 unchanged. Full resource catalog/Main/account/platform/business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1581,newChecks=11,native=16,fingerprints=len(fingerprints),indexedMethods=6384,controllers=21,remaining=17,diagnostics=record['logDiagnostics'])))
