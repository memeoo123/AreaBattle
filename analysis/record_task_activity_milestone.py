"""Record verified ordinary task activity/strategy runtime and native shared-root behavior once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1451 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/task-activity-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1451'in log and 'error CS'not in log
native=read(validation/'analysis/task-activity-native-validation.json');assert native['passed']and len(native['checks'])==14
native_log=(validation/'analysis/task-activity-native.log').read_text();assert 'error CS'not in native_log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('task-activity-')]);assert len(focused['checks'])==12
focused['limitations']='Concrete ordinary task parent/child/strategies/manager/factories with original8-task config, actual item economy/common events and independent file storage. Joint seven-day root/native frames verified separately. Claim tests call actual activity methods; task UI/pointers, Main/account/platform hosts and Player/original audiovisual acceptance remain pending.'
evidence=read(out/'TASK_ACTIVITY_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']),len(evidence['abstractMethods']))==(73,55,33,4)
for row in evidence['methods']+[evidence['eventWrapper']]:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6161 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameTaskActivity.cs','Scripts/OutgameTaskNetStrategy.cs','Scripts/OutgameChildTaskOffStrategy.cs','Scripts/OutgameTaskRuntime.cs','Scripts/OutgameTaskItemFactory.cs','Editor/OutgameTaskActivityValidation.cs','Editor/OutgameTaskActivityPlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:
 paths.add('UnityProject/Assets/AreaBattle/'+name);paths.add('UnityProject/Assets/AreaBattle/'+name+'.meta')
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='ordinary-task-activity-strategies-and-shared-runtime';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore ordinary task UI/main task entry and native user pointers against concrete TaskActivity; then Achievement models/manager/strategy/activity/UI. Continue full main-page/Main/account/platform and remaining19 controllers/all business/reward return, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original ordinary task activities/strategies/item factory in actual shared ordinary/seven-day runtime',checksPassed=1451,newIntegratedChecks=12,targetedChecks=12,nativeChecks=14,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly','OutgameTaskActivityPlayModeValidation.Run'],
 observedBoundaries=[
  'TaskActivity4685/ChildTaskActivity4676/TaskNetStrategyBase4683/ChildTaskOffNetStrategy4681 and TaskItemFactory4669/TaskIapRefreshItem4670 restored. TaskRuntime installs actual manager/activity/model services and preserves limited-task registration. Source73 normal methods,55 fields,33 usages,4 abstract declarations, boxed event wrapper and concrete weighted generic type; normal index6161.',
  'Source direct ActivityBase parent retains registered seven-day child alongside ordinary child. Ordinary child maps/indexes preserve duplicate failure prefixes; other-view getter deliberately allocates fresh dictionary per call. Display sort uses config.param. Initial/rebound listeners preserve source configured-versus-recorded key semantics and native dispatcher ordering.',
  'Original8 tasks generate via chance, show conditions and weighted no-replacement selection. Source quirks preserved: group count minus remaining TASK count loop bound, first selected config expiry/receive mode for every selected task, zero UID precheck, signed hash UID, condition-first-row-length report indexing and partial mutation on exceptions.',
  'Statistics enforces local launch-state and receiveState gates; lifetime values, boxed Int64 delta/Int32 filters, signed/clamped values and report exceptions verified. Task claim sets state before real rewards/costs/list update; liveness endpoint lacks internal threshold/repeat guard and matches first threshold tier. No invented economy rollback or payment success.',
  'Daily/weekly strict-greater boundary differs from interval equality; reset auto-claims eligible tasks and liveness, clears source counters, regenerates tasks, rebinds events and notifies in original order. Expiry equality retains task and later adjacent expired rows each notify. Save uses previous manager snapshot-before-activity-save ordering.',
  'Parent source factory is TaskFactory4673 category2/3; child usage3939868 is TaskItemFactory4669 category10/4 despite shared constructor annotation. Corrected before final1451 regression and native14. Child item regular Add awards then invokes Use/report; model-only Add omits Use and no task refresh/debit is invented.',
  '1451 integrated including12 new checks;2536 matching inputs. Final visible PlayMode14 checks actual shared graph, native readiness-gated autosave, both families claims, independent disk restart and next-millisecond daily regeneration. Claims use restored methods, not task UI pointers.',
  'Full log retains32 existing ShouldRunBehaviour assertions and one Curl42. Native log retains one UnityEditor.Search startup ArgumentOutOfRangeException and one Curl42. No compiler errors. Earlier type-alias compile failure,1450 pre-child-factory run and native13 remain separate diagnostic artifacts.',
  'Task page/main entry UI, Achievement, full Main/account/platform and all remaining business/19 controllers, final Player and original audiovisual acceptance are incomplete. Controller lifecycle19/38 unchanged.'
 ],notClaimed='Ordinary task UI pointer/whole main-page acceptance, Achievement, complete Main/account/platform/all business, fresh Player and original audiovisual acceptance.')
write(analysis/'task-activity-validation.json',focused)
for name in ('unity-integrated-validation.json','task-activity-integrated.log','task-activity-native-validation.json','task-activity-native.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'TASK_ACTIVITY_AUDIT.json',dict(status='original-task-activities-strategies-shared-runtime-native-verified-ui-pending',atUtc=now,sourceEvidence='TASK_ACTIVITY_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],nativeChecks=native['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,taskActivityAudit='generated/outgame/TASK_ACTIVITY_AUDIT.json');state['validation'].update(integratedChecksPassed=1451,taskActivity=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-task-activities-strategies-shared-runtime-native-verified-ui-pending',integratedChecks=1451,newChecks=12,nativeChecks=14,freshPlayModeRun=True,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1451,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for name in ('analysis/task-activity-validation.json','analysis/task-activity-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original ordinary parent/child task activities, offline/statistics/calendar/reward strategies and distinct child task item factory restored in actual shared ordinary/seven-day graph.1451 integrated including12 new,14 fresh native frames/disk-restart/day-boundary checks,{len(fingerprints)} matching inputs,6161 normal methods. Ordinary task UI/Main/account/platform/Achievement/remaining business and Player pending. Lifecycle19/38.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1451,newChecks=12,native=14,fingerprints=len(fingerprints),indexedMethods=6161,controllers=19,remaining=19)))
