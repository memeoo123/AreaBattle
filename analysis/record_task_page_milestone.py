"""Record verified task-page/shared-subview lifecycle milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1521 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/task-page-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1521'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('task-page-')]);assert len(focused['checks'])==9
focused['limitations']='Full original TaskPanelUI, shared lazy singleton ownership, actual subviews/TopInfo, source entry route and BaseUI resource close.9 new integrated,12 fresh native checks. Task/claim/liveness records survive independent storage restart; currency uses existing in-memory fixture and account currency persistence is NOT claimed. Full production Main/account/platform/effects/reports/audio/localization/Player pending.'
native=read(validation/'analysis/task-page-native-validation.json');assert native['passed']and len(native['checks'])==12
native_log=(validation/'analysis/task-page-native.log').read_text();assert 'error CS'not in native_log
evidence=read(out/'TASK_PAGE_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(26,38,9)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6332 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==20
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameTaskSubviews.cs','Scripts/OutgameTaskPage.cs','Scripts/OutgameTaskEntryBinding.cs','Scripts/OutgameTaskRow.cs','Editor/OutgameTaskPageValidation.cs','Editor/OutgameTaskPagePlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='task-page-singleton-lifecycle-and-main-entry';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Compose Task/Achievement rewards with actual account ItemManager/LocalDataManager persistent storage and statistics owners, replacing the explicitly in-memory reward fixture for end-to-end currency restart proof. Continue production Main/account/platform/effect/report/audio/localization hosts, remaining18 controller lifecycles/all business and final Player/original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original TaskPanelUI4416, TaskSingleton3990, main task opening route, native tabs/claim/close/reopen and task-record restart',checksPassed=1521,newIntegratedChecks=9,targetedChecks=9,nativeChecks=12,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=20,remainingLifecycleControllers=18,commands=['BattleBuild.ValidateMechanicsOnly','OutgameTaskPagePlayModeValidation.Run'],
 observedBoundaries=evidence['findings']+['Integrated retains32 existing ShouldRunBehaviour assertions, Curl35 certificate verification failure and Unity cloud configuration timeout. Native editor startup Search exception/Curl diagnostics remain separate from product checks. Initial native test wrongly expected in-memory reward fixture currency to persist; final test explicitly verifies disk-backed task claim/liveness and records fresh fixture currency separately.'],notClaimed='Production account currency persistence in this task-page fixture; full production Main/account/platform/effects/reports/audio/localization, original audiovisual equivalence or fresh Player.')
write(analysis/'task-page-validation.json',focused)
for name in ('unity-integrated-validation.json','task-page-integrated.log','task-page-native-validation.json','task-page-native.log','task-page-native-initial-validation.json','task-page-native-initial.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'TASK_PAGE_AUDIT.json',dict(status='original-task-page-main-entry-verified-production-composition-pending',atUtc=now,sourceEvidence='TASK_PAGE_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],nativeChecks=native['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,taskPageAudit='generated/outgame/TASK_PAGE_AUDIT.json');state['validation'].update(integratedChecksPassed=1521,taskPage=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-task-page-main-entry-verified-production-composition-pending',integratedChecks=1521,newChecks=9,nativeChecks=12,freshPlayModeRun=True,remainingLifecycleControllers=18));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1521,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for name in ('analysis/task-page-validation.json','analysis/task-page-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'TaskPanelUI4416, shared TaskSingleton3990 and StartUI task entry restored with original UITip/TopInfo/toggle/close/late-init/failure ordering.1521 integrated including9 new,12 fresh native page/claim/close/reopen/task-record-restart checks,{len(fingerprints)} matching inputs,6332 normal methods. Controller lifecycle20/38 unchanged. Currency fixture is in-memory; actual account reward persistence and full production hosts/remaining business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1521,newChecks=9,native=12,fingerprints=len(fingerprints),indexedMethods=6332,controllers=20,remaining=18)))
