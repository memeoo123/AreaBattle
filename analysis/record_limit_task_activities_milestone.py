"""Record verified concrete limit-task activities and native frame/restart evidence."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes();p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed'] and len(full['checks'])==1344 and all(r['result']=='pass' for r in full['checks'])
log=(validation/'analysis/limit-task-activities-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1344' in log and 'error CS' not in log
native=read(validation/'analysis/limit-task-activities-native-validation.json');assert native['passed'] and len(native['checks'])==9
native_log=(validation/'analysis/limit-task-activities-native.log').read_text();assert 'error CS' not in native_log
focused=dict(full,checks=[r for r in full['checks'] if r['id'].startswith('limit-task-activity-')]);assert len(focused['checks'])==21
focused['limitations']='Concrete parent/child/manager activity graph, real49-task config/statistics/dispatch/item engine/file storage.21 new integrated checks plus separate9 native frame/pointer/restart/day checks. Diagnostic button is not original page; SevenDay/preload/Main/account/report delivery/Player and full audiovisual acceptance remain pending.'
assert len(read(out/'method-map.json'))==5962 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==18
evidence=read(out/'LIMIT_TASK_ACTIVITIES_SOURCE_EVIDENCE.json');assert (len(evidence['methods']),len(evidence['fields']),len(evidence['methodUsages']),len(evidence['literals']))==(43,41,11,20)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path'] for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameLimitTimeTaskActivity.cs','Scripts/OutgameLimitTimeTaskRuntime.cs','Editor/OutgameLimitTimeTaskValidation.cs','Editor/OutgameLimitTimeTaskPlayModeValidation.cs')
paths.update('UnityProject/Assets/AreaBattle/'+name for name in files);fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2339
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='limit-task-concrete-activities-and-native-restart';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore SevendayActivityControl.EnterGameInit and ProcedurePreLoad composition with the concrete common activity/task owners; restore original CommonLimitTimeTaskUI/pages/assets and Task/Achievement activities/managers. Continue production Main/account/SDK/HTTP/scene hosts, remaining20 LogicModule controllers/all gameplay/shop/rewards/return and Player/audiovisual acceptance.'
record=dict(atUtc=now,scope='Original ChildLimitTimeTaskActivity4698 and LimitTimeTaskActivity4699 with actual common runtime registration/models/factory and native save/restart/day events',checksPassed=1344,newIntegratedChecks=21,targetedChecks=21,nativeChecks=9,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),commands=['BattleBuild.ValidateMechanicsOnly','OutgameLimitTimeTaskPlayModeValidation.Run'],
 observedBoundaries=[
  '43 source methods,41 fields,11 callback/generic usages and20 literal values retained; method index5962. Actual49-task runtime graph uses concrete registered parent/child, manager4711, real FSM/statistics/common dispatcher and model/factory owners. Runtime binding preserves other registrations and requires actual application storage/report/item endpoints.',
  'Initialization reuses saved objects, retains diagnosed orphan records, creates unsaved tasks, decodes insertion-order64-bit reward flags. Saving only appends records with nonzero progress/state within a condition loop, preserving zero-condition and duplicate-ID behavior. Reset retains cumulative flags/launch time/clicked page; notifications distinguish current-day initialization from group-day reset.',
  'Real common events verify state/lifetime/argument filters, Int64 delta unboxing, clamp, threshold/report slots and caught failure prefixes. Listener add/config versus remove/saved key mismatch and empty-key duplicate clock registration retained. Source owner-ID completion provider and integer-day gap failures retained instead of invented fixes.',
  'Actual global rewards/costs and liveness factory precede task state/flags; failures preserve awarded prefix. Signed32-bit masks extended into Int64 preserve bit31 sign extension and bit32 aliasing versus64-bit decode. Task reward/day/all/cumulative and liveness reports retain source ordering and distinct localization. External report delivery remains explicit host-owned.',
  'Native9 passed in visible GameView: actual49-task graph, statistics eligibility, diagnostic pointer claim via original item config, liveness10, native frame automatic save, fresh file-backed runtime restart, no repeat award, clock calendar-day unlock and next-frame save. Original task page, SevenDay/Main, complete inventory restart and platform delivery are not claimed.',
  'Integrated1344 passed with32 existing ShouldRunBehaviour assertions and one Curl42 shutdown abort. Native passed with one UnityEditor.Search startup ArgumentOutOfRangeException and one Curl42 shutdown abort; zero compiler errors. Initial integrated1339 failed one fixture expectation due to sorted event order; revised isolated failure case plus five boundary checks pass. Native driver added after integrated pass and compiled/run successfully; gameplay and integrated-test source unchanged.2339 source inputs match isolated project.'
 ],notClaimed='Original task UI/page visuals/SevenDay/preload/Main, Task/Achievement owners, remaining20 controllers, account/payment/report delivery, Player build or original audiovisual acceptance.')
write(analysis/'limit-task-activities-validation.json',focused)
for name in ('unity-integrated-validation.json','limit-task-activities-integrated.log','limit-task-activities-initial.log','limit-task-activities-native-validation.json','limit-task-activities-native.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'LIMIT_TASK_ACTIVITIES_AUDIT.json',dict(status='concrete-task-runtime-native-verified-pages-main-pending',atUtc=now,sourceEvidence='LIMIT_TASK_ACTIVITIES_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,limitTaskActivitiesAudit='generated/outgame/LIMIT_TASK_ACTIVITIES_AUDIT.json');state['validation'].update(integratedChecksPassed=1344,limitTaskActivities=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='concrete-task-runtime-native-verified-pages-main-pending',integratedChecks=1344,newChecks=21,nativeChecks=9,freshPlayModeRun=True,remainingLifecycleControllers=20));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path']) for r in manifest['sourceFingerprints'] if r['path'] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1344,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)]);manifest['validationHistory'].append(record);write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append('Concrete limit-task parent/child/manager/model/factory graph restored;1344 integrated including21 new,9 native frame/pointer/restart/day checks,2339 matching inputs,5962 method index. Original task pages/SevenDay/preload/Main and platform delivery pending; controller lifecycle18/38, no Player build.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1344,newChecks=21,native=9,fingerprints=len(fingerprints),indexedMethods=5962,controllers=18,remaining=20)))
