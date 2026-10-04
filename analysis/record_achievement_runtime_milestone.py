"""Record verified concrete Achievement runtime milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1500 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/achievement-runtime-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1500'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('achievement-runtime-')]);assert len(focused['checks'])==13
focused['limitations']='Actual Achievement activity/manager/offline strategy over127 original configs, actual common statistics/item engine and account-aware data pool file storage. Compact/legacy/empty-claim restart verified. Report/reward-delivery hosts remain explicit fixtures; full task/achievement UI/controller/Main/platform and final audiovisual/Player remain pending. No fresh native run.'
evidence=read(out/'ACHIEVEMENT_RUNTIME_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(63,17,31)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6298 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameAchievementRuntime.cs','Scripts/OutgameAchievementStrategy.cs','Editor/OutgameAchievementRuntimeValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='achievement-runtime-statistics-claims-and-data-pool-storage';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Complete AchiTaskSubUI3986/TaskController4507/TaskSingleton3990/TaskPanelUI4416 daily-achievement ownership, tabs/red dots and Main entry over actual Task and Achievement runtimes. Continue all remaining19 controller lifecycles, production Main/account/platform/effect/report hosts, business and original audiovisual/Player acceptance.'
record=dict(atUtc=now,scope='Original concrete Achievement activity/manager/strategy, statistics, claims and data pool storage',checksPassed=1500,newIntegratedChecks=13,targetedChecks=13,nativeChecks=0,freshPlayModeRun=False,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly'],
 observedBoundaries=evidence['findings']+['Final integrated log retains32 existing ShouldRunBehaviour assertions and one Curl42; no compiler errors. Initial run exposed an incorrectly chosen test initial value for the source live-sort case; corrected scenario verifies repeated/omitted visits. Final1500 checks pass.'],notClaimed='Full task/achievement UI/controller/Main, production account/platform/effect/report delivery hosts, original audiovisual equivalence or fresh native/Player.')
write(analysis/'achievement-runtime-validation.json',focused)
for name in ('unity-integrated-validation.json','achievement-runtime-integrated.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'ACHIEVEMENT_RUNTIME_AUDIT.json',dict(status='original-achievement-runtime-verified-ui-main-pending',atUtc=now,sourceEvidence='ACHIEVEMENT_RUNTIME_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,achievementRuntimeAudit='generated/outgame/ACHIEVEMENT_RUNTIME_AUDIT.json');state['validation'].update(integratedChecksPassed=1500,achievementRuntime=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-achievement-runtime-verified-ui-main-pending',integratedChecks=1500,newChecks=13,nativeChecks=0,freshPlayModeRun=False,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1500,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
if 'analysis/achievement-runtime-validation.json'not in manifest['reports']:manifest['reports'].append('analysis/achievement-runtime-validation.json')
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Actual Achievement Activity/Manager/OffStrategy/runtime registered before ActivityControl startup; source progress/filter/live-sort, reward failure sequence, cumulative points and data-pool compact/legacy/empty-claim restart verified.1500 integrated including13 new,{len(fingerprints)} matching inputs,6298 normal methods. No fresh native/Player; full task page/controller/Main pending, lifecycle19/38.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1500,newChecks=13,native=0,fingerprints=len(fingerprints),indexedMethods=6298,controllers=19,remaining=19)))
