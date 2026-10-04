"""Record ordinary-task models/storage schema and liveness factory validation."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1431 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/task-models-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1431'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('task-model-')]);assert len(focused['checks'])==12
focused['limitations']='Original ordinary-task models, storage schema and liveness factory over real config/item reward engine. Temporary file roundtrip only; actual TaskMgr/strategies/activities/UI, automatic persistence/Main/Player remain pending. No fresh native run.'
evidence=read(out/'TASK_MODELS_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(30,42,15)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6074 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameTaskModels.cs','Scripts/OutgameTaskFactory.cs','Editor/OutgameTaskModelsValidation.cs','Editor/BattleBuild.cs')
for name in files:
 paths.add('UnityProject/Assets/AreaBattle/'+name);paths.add('UnityProject/Assets/AreaBattle/'+name+'.meta')
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='ordinary-task-models-and-liveness-factory';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore ordinary TaskMgr4692/TaskOffStrategy4696 and ChildTaskOffNetStrategy4681/TaskNetStrategyBase4683, actual ChildTaskActivity4676/TaskActivity4685 assembly and automatic save/daily refresh/claim, then Achievement managers/strategies/models/UI. Continue complete main-page/Main/account/platform and remaining19 controllers/all business/rewards/return, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original ordinary-task models/storage schema and actual liveness item factory/reward events',checksPassed=1431,newIntegratedChecks=12,targetedChecks=12,nativeChecks=0,freshPlayModeRun=False,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly'],
 observedBoundaries=[
  'TaskData4686/ChildTaskData4687/Ext4688/GroupData4689/TaskItemData4690/Condition4691/LivenessItemData4677 and TaskFactory4673/TaskLivenessPoint4674 restored. New30 normal methods make index6074; evidence30 methods,42 fields including visibility attributes,15 decoded usages. Actual original8 tasks/one activity/one group/three tiers loaded.',
  'Task reset appends copied zero-value conditions, retaining existing progress/state/selection and partial work on malformed input. Eligibility compares recorded Int64 progress to signed last config target and exact state0 without invented receive/expiry/show gates. Description snapshots target ints before language callback. Comparator preserves ready/unreceived/received/claimed branches and source noncanonical-state non-total ordering.',
  'Config caches retain first nonnull row and retry null. Child/group refresh reset publishes a new zero-filled vector before config failure without resetting unrelated counters. Liveness list is lazily published and explicitly refreshed in dictionary order, deriving state from unsigned32-bit mask with shift count modulo32. Reward cache retains negative and partially decoded entries.',
  'TaskData.Clone uses original memberwise wrapper and shares nested graph. Public storage fields and signed Int64 limits survive independent temporary JSON file write/read; private configs/selection/internal liveness cache are excluded. This proves storage schema only, not automatic TaskMgr persistence or full task restart.',
  'Task factory accepts only category2/3. Actual item model/reports execute before CommonGameModule_Task_LivenessPointAdd+current paramInt with boxed lowInt32 count. Event exception preserves applied award/report. Regular Add omits activity-only event; Use reports without implicit debit. Original economic long retained.',
  'Integrated1431 including12 new passes,2518 matching source inputs. No fresh native suite/Player; earlier native results remain historical. Final log has zero compiler errors,32 existing ShouldRunBehaviour assertions and one Curl42.',
  'Actual TaskMgr/TaskOffStrategy/ChildTask strategy/activity/daily refresh/UI and Achievement remain pending. Full main-page/Main/account/platform/all business, remaining19 controllers and final Player/original audiovisual acceptance still required. Lifecycle19/38 unchanged.'
 ],notClaimed='TaskMgr/strategies/activities automatic persistence/daily refresh/claim/UI, Achievement, complete Main/platform/all business, fresh native/Player and original audiovisual acceptance.')

write(analysis/'task-models-validation.json',focused)
for name in ('unity-integrated-validation.json','task-models-integrated.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'TASK_MODELS_AUDIT.json',dict(status='original-task-models-and-liveness-factory-verified-manager-strategy-pending',atUtc=now,sourceEvidence='TASK_MODELS_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,taskModelsAudit='generated/outgame/TASK_MODELS_AUDIT.json');state['validation'].update(integratedChecksPassed=1431,taskModels=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-task-models-and-liveness-factory-verified-manager-strategy-pending',integratedChecks=1431,newChecks=12,nativeChecks=0,freshPlayModeRun=False,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1431,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for p in ('analysis/task-models-validation.json',):
 if p not in manifest['reports']:manifest['reports'].append(p)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original ordinary-task storage/models and liveness factory restored; actual8-task config, source reset/eligibility/sort/caches, independent storage-schema file read and real item event/reward behavior verified.1431 integrated including12 new,{len(fingerprints)} matching inputs,6074 normal methods. No fresh native run; TaskMgr/strategies/activities/UI/Achievement/Main/remaining business/Player pending. Lifecycle19/38.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1431,newChecks=12,native=0,fingerprints=len(fingerprints),indexedMethods=6074,controllers=19,remaining=19)))
