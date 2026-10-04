"""Record verified ordinary task manager and offline storage; run once per milestone."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1439 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/task-manager-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1439'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('task-manager-')]);assert len(focused['checks'])==8
focused['limitations']='Original TaskMgr and offline strategy over actual config/pool/account-aware storage and independent file restart. TaskActivity endpoints are explicit test callbacks; production parent/child activity, daily refresh, claims/UI/Main remain pending. No fresh native/Player run.'
evidence=read(out/'TASK_MANAGER_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']),len(evidence['runtimeGenericContext']))==(18,10,11,7)
for row in evidence['methods']+evidence['genericFiles']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6088 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameTaskManager.cs','Editor/OutgameTaskManagerValidation.cs','Editor/BattleBuild.cs')
for name in files:
 paths.add('UnityProject/Assets/AreaBattle/'+name);paths.add('UnityProject/Assets/AreaBattle/'+name+'.meta')
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='ordinary-task-manager-and-offline-storage';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore concrete ChildTaskActivity4676/TaskActivity4685 and ChildTaskOffNetStrategy4681/TaskNetStrategyBase4683, install actual TaskMgr/factory in activity runtime and verify daily refresh/claims/automatic save/UI; then Achievement. Continue full Main/account/platform and remaining19 controllers/all business/reward return, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original ordinary TaskMgr and offline strategy with actual storage/migration/file restart',checksPassed=1439,newIntegratedChecks=8,targetedChecks=8,nativeChecks=0,freshPlayModeRun=False,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly'],
 observedBoundaries=[
  'TaskMgr4692/TaskOffStrategy4696 and shared NetStratrgyBase methods recovered. Source evidence18 normal methods (14 newly indexed),5 generic bodies,3 concrete wrappers,10 fields,11 usages,7 RGCTX slots and TaskData/TaskActivity/TaskMgr concrete type arguments. Normal index6088.',
  'Manager OnInit only creates strategy; explicit initialization binds actual pool owner and config-gates UpdateData(true). Server waits for genuine callback. Manager publishes parsed input before Add indexing, retaining duplicate prefix and skipping activity callback on error.',
  'Offline loading preserves gzip/raw JSON fallback, fresh versus existing migration branches, orphan cleanup, refresh sentinel, legacy signed progress and condition-key reconciliation. Migration/logging exceptions preserve original partial updates; UID initialized only after successful repair. New child reset occurs before append in reconciliation but after append in fresh data.',
  'Save captures JSON-roundtrip compressed snapshot before activity SaveData callback, then stores through current manager. Cached nonnull activity retained; null retries; missing owner/throwing callback prevents storage. Independent actual file backend restart verifies signed limits/progress/state and liveness mask.',
  '1439 integrated including8 new checks pass;2522 matching inputs. No fresh native or Player run. Log retains32 existing ShouldRunBehaviour assertions and one Curl42, zero compiler errors.',
  'TaskActivity callbacks are explicit validation endpoints, not actual task business. Concrete parent/child activities, daily refresh/claim/UI and production runtime installation remain pending. Controller lifecycle19/38 unchanged; complete Main/account/platform/all business/Player/original audiovisual acceptance incomplete.'
 ],notClaimed='Production ordinary task activity/automatic daily refresh/claim/UI, Achievement, complete Main/account/platform/all business, fresh native/Player and original audiovisual acceptance.')
write(analysis/'task-manager-validation.json',focused)
for name in ('unity-integrated-validation.json','task-manager-integrated.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'TASK_MANAGER_AUDIT.json',dict(status='original-task-manager-offline-storage-verified-concrete-activity-pending',atUtc=now,sourceEvidence='TASK_MANAGER_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,taskManagerAudit='generated/outgame/TASK_MANAGER_AUDIT.json');state['validation'].update(integratedChecksPassed=1439,taskManager=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-task-manager-offline-storage-verified-concrete-activity-pending',integratedChecks=1439,newChecks=8,nativeChecks=0,freshPlayModeRun=False,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1439,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
if 'analysis/task-manager-validation.json'not in manifest['reports']:manifest['reports'].append('analysis/task-manager-validation.json')
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original ordinary TaskMgr/offline strategy restored with legacy migration, source lifecycle/save ordering, genuine deferred server callback and independent actual file restart.1439 integrated including8 new,{len(fingerprints)} matching inputs,6088 normal methods. No new native/Player; concrete task activity/daily refresh/claim/UI, Achievement/Main/remaining business pending. Lifecycle19/38.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1439,newChecks=8,native=0,fingerprints=len(fingerprints),indexedMethods=6088,controllers=19,remaining=19)))
