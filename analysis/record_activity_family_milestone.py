"""Record verified shared activity families and novice task module storage (no native rerun)."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes();p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json')
assert full['passed'] and len(full['checks'])==1306 and all(r['result']=='pass' for r in full['checks'])
log=(validation/'analysis/activity-family-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1306' in log and 'error CS' not in log
focused=dict(full,checks=[r for r in full['checks'] if r['id'].startswith('activity-family-')]);assert len(focused['checks'])==14
focused['limitations']='Shared activity father/child and novice manager source reconciliation, real base FSM/config/pool and compressed file restart. Concrete task progress/rewards/pages/SevenDay/Main are pending. Explicit concrete activity fixture endpoints. No fresh native PlayMode or Player run.'
assert len(read(out/'method-map.json'))==5884 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==18
evidence=read(out/'ACTIVITY_FAMILY_SOURCE_EVIDENCE.json');assert (len(evidence['methods']),len(evidence['sharedGenerics']),len(evidence['fields']))==(15,11,28)
for row in evidence['methods']+evidence['sharedGenerics']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
rgctx=read(out/'activity-family-rgctx.json');assert len(rgctx['rows'])==27
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path'] for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameActivityFamily.cs','Scripts/OutgameNoviceTaskManager.cs','Editor/OutgameActivityFamilyValidation.cs')
paths.update('UnityProject/Assets/AreaBattle/'+name for name in files);fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2332
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='activity-family-novice-data';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore actual ChildLimitTimeTaskActivity4698 and LimitTimeTaskActivity4699 progress/day/accumulator rewards, task/item getters/factories and remaining attributed Task/Achievement managers/activities. Bind SevenDay.EnterGameInit and ProcedurePreLoad to shared activity runtime; finish production Main/account/SDK/HTTP/scene hosts, remaining20 LogicModule controllers/all gameplay/shop/rewards/return and Player/audiovisual acceptance.'
record=dict(atUtc=now,scope='Original shared FatherActivityBase/ChildActivityBase and NoviceTaskManager storage/reconciliation; exact concrete activity registration roster',checksPassed=1306,newIntegratedChecks=14,targetedChecks=14,freshPlayModeRun=False,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),commands=['BattleBuild.ValidateMechanicsOnly'],
 observedBoundaries=[
  '11 shared generic bodies,27 runtime context entries,15 normal methods and28 field records retained; normal method index5884. Complete9-type activity ancestry resolves generic parents; only Task1300001,LimitTimeTask1301001,Achievement1601001 carry registration attributes. Concrete seven-day children have no automatic registration attribute.',
  'Family preserves source constructor/discard behavior, module assignment before base Refresh, first parent match, retained data/ownership dictionaries, sequential Add failures and SortDataDic boundary. Actual child base FSM integration and disposal verified, alongside explicit probe checks for failure prefixes.',
  'Novice manager loads original49-task config through real pool OnInit, preserves raw/gzip fallback and uncaught second parse error, publishes Data before reconciliation, removes adjacent orphan activities, retains duplicate saved rows with last-row index, initializes first-insertion config day and diagnoses empty groups. No initial save or fabricated task entries.',
  'Independent file backend/owner restart preserves every source ext field, full signed64 flags, task state and condition value/args. Server wait never creates data; actual callbacks and sync-disabled local path verified. Current-config terminal lookup, callback replacement of live Data, refresh/save failure prefixes and empty release/strategy verified.',
  'Integrated1306 passes with32 existing ShouldRunBehaviour assertions and one Curl42 shutdown abort, zero compiler errors.2332 validation inputs match isolated project. Previous ActivityControl native13 result is retained historical evidence; no fresh native or Player run in this milestone.'
 ],notClaimed='Concrete child/parent task progress or reward logic, task getters/factories, actual pages/SevenDay/Main assembly, remaining20 controllers, real platform/account/payment delivery, original audiovisual acceptance or fresh native/Player validation.')
write(analysis/'activity-family-validation.json',focused)
for name in ('unity-integrated-validation.json','activity-family-integrated.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'ACTIVITY_FAMILY_AUDIT.json',dict(status='shared-family-novice-storage-verified-concrete-task-business-pending',atUtc=now,sourceEvidence='ACTIVITY_FAMILY_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,activityFamilyAudit='generated/outgame/ACTIVITY_FAMILY_AUDIT.json');state['validation'].update(integratedChecksPassed=1306,activityFamily=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='shared-family-novice-storage-verified-concrete-task-business-pending',integratedChecks=1306,newChecks=14,freshPlayModeRun=False,remainingLifecycleControllers=20));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path']) for r in manifest['sourceFingerprints'] if r['path'] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1306,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)]);manifest['validationHistory'].append(record);write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append('Shared activity father/child and original NoviceTaskManager storage/reconciliation;1306 full integrated checks,2332 matching inputs; exact3 attributed activities, concrete task/reward/page/SevenDay/Main still pending; no fresh native or Player, controller lifecycle18/38.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1306,newChecks=14,fingerprints=len(fingerprints),indexedMethods=5884,controllers=18,remaining=20)))
