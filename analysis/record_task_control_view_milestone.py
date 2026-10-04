"""Record verified task controller/Achievement subview milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1512 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/task-control-view-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1512'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('task-control-view-')]);assert len(focused['checks'])==12
focused['limitations']='Actual controller4507/AchiTaskSubUI3986, simultaneous Task/Achievement owners and original main/task row assets. Source event/red/filter/projection/claim/disposal verified. Fresh11 native pointer/scaled-slide/award/restart checks recorded separately. Full TaskSingleton/TaskPanel lifecycle/tabs/Main/platform/effect/report hosts and Player remain pending.'
native=read(validation/'analysis/task-control-view-native-validation.json');assert native['passed']and len(native['checks'])==11
native_log=(validation/'analysis/task-control-view-native.log').read_text();assert 'error CS'not in native_log
evidence=read(out/'TASK_CONTROL_VIEW_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(34,30,27)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6317 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==20
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameTaskControl.cs','Scripts/OutgameAchievementTaskView.cs','Scripts/OutgameCoreControllerBindings.cs','Editor/OutgameTaskControlViewValidation.cs','Editor/OutgameTaskControlViewPlayModeValidation.cs','Editor/OutgameTaskRowsValidation.cs','Editor/OutgameTaskActivityValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='task-controller-achievement-subview-and-native-claim';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover TaskSingleton3990 concrete generic ownership and full TaskPanelUI4416 lifecycle/daily-achievement tabs/close/top-info/audio/preview ownership plus original Main task button opening route, over restored actual Task/Achievement runtimes and controller4507/AchiTaskSubUI3986. Continue production Main/account/platform/effect/report hosts, remaining18 controller lifecycles, all business and original audiovisual/Player acceptance.'
record=dict(atUtc=now,scope='Original task controller4507, main task visibility/red and Achievement subview3986 with native claim/restart',checksPassed=1512,newIntegratedChecks=12,targetedChecks=12,nativeChecks=11,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=20,remainingLifecycleControllers=18,commands=['BattleBuild.ValidateMechanicsOnly','OutgameTaskControlViewPlayModeValidation.Run'],
 observedBoundaries=evidence['findings']+['Final integrated retains32 existing ShouldRunBehaviour assertions and one Curl42. Native retains one UnityEditor.Search startup ArgumentOutOfRangeException and one Curl42; no compiler errors. Fresh native11 checks pass after initial layout measurement corrected.'],notClaimed='Full TaskSingleton/TaskPanel lifecycle/tabs/Main startup or opening route, complete production account/platform/effect/report hosts, original audiovisual equivalence or fresh Player.')
write(analysis/'task-control-view-validation.json',focused)
for name in ('unity-integrated-validation.json','task-control-view-integrated.log','task-control-view-native-validation.json','task-control-view-native.log','task-control-view-native-initial-validation.json','task-control-view-native-initial.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'TASK_CONTROL_VIEW_AUDIT.json',dict(status='original-task-controller-achievement-subview-verified-full-page-main-pending',atUtc=now,sourceEvidence='TASK_CONTROL_VIEW_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],nativeChecks=native['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,taskControlViewAudit='generated/outgame/TASK_CONTROL_VIEW_AUDIT.json');state['validation'].update(integratedChecksPassed=1512,taskControlView=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-task-controller-achievement-subview-verified-full-page-main-pending',integratedChecks=1512,newChecks=12,nativeChecks=11,freshPlayModeRun=True,remainingLifecycleControllers=18));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1512,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for name in ('analysis/task-control-view-validation.json','analysis/task-control-view-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Controller4507 lifecycle/real owner cache/events/level10 entrance/red and AchiTaskSubUI3986 type filtering/projection/claim/disposal restored.1512 integrated including12 new,11 fresh native original-pointer/scaled-slide/200-gold/row-reuse/file-restart checks,{len(fingerprints)} matching inputs,6317 normal methods. Lifecycle20/38; TaskSingleton/full TaskPanel tabs/lifecycle/Main opening and complete production hosts/remaining business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1512,newChecks=12,native=11,fingerprints=len(fingerprints),indexedMethods=6317,controllers=20,remaining=18)))
