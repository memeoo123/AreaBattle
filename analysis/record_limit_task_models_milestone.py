"""Record source task/cumulative reward models and liveness factory verification."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes();p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed'] and len(full['checks'])==1323 and all(r['result']=='pass' for r in full['checks'])
log=(validation/'analysis/limit-task-models-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1323' in log and 'error CS' not in log
focused=dict(full,checks=[r for r in full['checks'] if r['id'].startswith('limit-task-model-') or r['id'].startswith('limit-task-factory-')]);assert len(focused['checks'])==17
focused['limitations']='Original limit-task/cumulative reward/page models and liveness item factory, actual49-task configuration/statistics/item engine. Child graph is explicit required test endpoint; concrete activity initialization/progress listeners/claims/pages/SevenDay/Main pending. No fresh native PlayMode or Player run.'
assert len(read(out/'method-map.json'))==5919 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==18
evidence=read(out/'LIMIT_TASK_MODELS_SOURCE_EVIDENCE.json');assert (len(evidence['methods']),len(evidence['fields']),len(evidence['methodUsages']))==(37,44,15)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path'] for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameLimitTaskModels.cs','Scripts/OutgameLimitTimeTaskFactory.cs','Editor/OutgameLimitTaskModelsValidation.cs')
paths.update('UnityProject/Assets/AreaBattle/'+name for name in files);fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2335
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='limit-task-models-and-liveness-factory';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore actual ChildLimitTimeTaskActivity4698 initialization/reset/statistics listeners/progress/reward-claim/day-unlock behavior and LimitTimeTaskActivity4699 manager/family ownership; bind restored model services and liveness factory. Then SevenDay.EnterGameInit/ProcedurePreLoad, remaining Task/Achievement activities/managers, production Main/account/SDK/HTTP/scene hosts, remaining20 LogicModule controllers/all gameplay/shop/rewards/return and Player/audiovisual acceptance.'
record=dict(atUtc=now,scope='LimitTaskItemData/NoviceAccRewardItemData/Ext/LimitTaskPageItem and ActivityVirtualItemBase/LimitTimeTaskFactory/LivenessPoint original behavior',checksPassed=1323,newIntegratedChecks=17,targetedChecks=17,freshPlayModeRun=False,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),commands=['BattleBuild.ValidateMechanicsOnly'],
 observedBoundaries=[
  '37 source methods,44 field records and15 generic method usages retained; method index5919. All49 original novice task config rows decode recorded/show conditions and rewards. Model singleton bindings require actual owners, with explicit child graph endpoints until concrete activity is restored.',
  'Task eligibility follows saved progress then live GameValue show conditions then unlocked day then state0; button state1 bypasses eligibility as claimed.24 state/record/show/day combinations, callback target reread, signed sorting and config cache behavior verified. Reset clears/rebuilds progress without changing claim/selection/show cache. Published condition/reward cache prefixes remain after malformed rows. JSON includes only original persisted fields.',
  'Cumulative progress counts claimed state1 tasks or matching2/4 reward items, independent of inventory. Reward model sums across matching item types; Ext additionally filters item.paramInt to activityId. Invalid mode is silent0 for reward model but diagnosed0 for Ext. Unchecked Int64 sum/Int32 truncation, missing target999, present empty0 and current-owner rereads covered.',
  'Source factory routes held config2/4 then item constructor rereads current config. Liveness AddItemOnlyModel uses actual item engine before common event with live paramInt and boxed Int32 truncation; normal AddItem and Use follow source base routes. Inventory/report/dirty prefix and event failure boundaries verified. External report delivery remains explicit host-owned.',
  'Integrated1323 checks pass on final code with32 existing ShouldRunBehaviour assertions and one Curl42 shutdown abort, zero compiler errors.2335 source inputs match isolated project. Initial passing log retained before comparator field-read/message-argument order refinement; final full run includes reentrant accumulator comparator test. No new native/Player run; historical ActivityControl native13 remains separate.'
 ],notClaimed='Concrete child/parent task initialization/statistics listeners/reward-claim/day-unlock, actual pages/SevenDay/Main composition, remaining20 controllers, platform/account/payment delivery, native/Player or original audiovisual acceptance.')
write(analysis/'limit-task-models-validation.json',focused)
for name in ('unity-integrated-validation.json','limit-task-models-integrated.log','limit-task-models-before-order-refinement.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'LIMIT_TASK_MODELS_AUDIT.json',dict(status='task-models-liveness-factory-verified-concrete-activity-pending',atUtc=now,sourceEvidence='LIMIT_TASK_MODELS_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,limitTaskModelsAudit='generated/outgame/LIMIT_TASK_MODELS_AUDIT.json');state['validation'].update(integratedChecksPassed=1323,limitTaskModels=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='task-models-liveness-factory-verified-concrete-activity-pending',integratedChecks=1323,newChecks=17,freshPlayModeRun=False,remainingLifecycleControllers=20));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path']) for r in manifest['sourceFingerprints'] if r['path'] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1323,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)]);manifest['validationHistory'].append(record);write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append('Limit task/accumulator/page models and liveness factory/item implemented;1323 integrated including17 new,2335 matching inputs,5919 method index; actual progress-listener/claim/day-unlock/SevenDay/Main pending, controller lifecycle18/38, no fresh native/Player.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1323,newChecks=17,fingerprints=len(fingerprints),indexedMethods=5919,controllers=18,remaining=20)))
