"""Record verified ordinary task row/daily projection milestone exactly once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1463 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/task-rows-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1463'in log and 'error CS'not in log
native=read(validation/'analysis/task-rows-native-validation.json');assert native['passed']and len(native['checks'])==14
native_log=(validation/'analysis/task-rows-native.log').read_text();assert 'error CS'not in native_log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('task-rows-')]);assert len(focused['checks'])==12
focused['limitations']='Original task row and daily-subview projection/claim over actual task activity/config/economy. Original TaskPanelUI static hierarchy with explicit duplicate-Graphic adaptation. Native pointers/scaled slide/save/restart verified separately. Full page lifecycle/tabs/liveness preview, production effect/sprite/report/formatter/Main hosts, Achievement and Player/original audiovisual acceptance pending.'
evidence=read(out/'TASK_ROWS_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(36,53,25)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6194 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameTaskRow.cs','Scripts/OutgameDailyTaskView.cs','Scripts/OutgameScalarTweenRunner.cs','Scripts/OutgameFlyEasing.cs','Editor/OutgameTaskRowsValidation.cs','Editor/OutgameTaskRowsPlayModeValidation.cs','Editor/RecoveredHudImporter.cs','Editor/BattleBuild.cs')
for name in files:
 paths.add('UnityProject/Assets/AreaBattle/'+name);paths.add('UnityProject/Assets/AreaBattle/'+name+'.meta')
assets=workspace/'UnityProject/Assets/AreaBattle/Resources/Recovered/TaskPanel';paths.add(str(assets.relative_to(workspace))+'.meta')
paths.update(str(p.relative_to(workspace))for p in assets.rglob('*')if p.is_file())
paths.add('analysis/targets/wxcf1394487200e48f/43/generated/outgame/task-panel-ui-import.json')
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='ordinary-task-row-and-daily-view-native-claims';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore TaskPanelUI4416 full lifecycle and daily/achievement tab ownership, LivenessPreviewItem4350, liveness tier interactions and main task entry. Recover concrete Achievement runtime and integrate shared rows. Continue full Main/account/platform/remaining19 controllers/all business/reward return, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original ordinary task row/daily-subview and native pointer/slide/claim/save/restart',checksPassed=1463,newIntegratedChecks=12,targetedChecks=12,nativeChecks=14,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['RecoveredHudImporter.ImportTaskPanelBatch','BattleBuild.ValidateMechanicsOnly','OutgameTaskRowsPlayModeValidation.Run'],
 observedBoundaries=[
  'TaskItemItem4367/TaskItemItemData4366 and DailyTaskSubUI3988 restored. Source36 reviewed methods (33 newly indexed),53 fields,25 usages and TaskType compressed default constants. Index6194. Daily singleton creation/full page ownership is not included.',
  'Original12 bundles verified against catalog size/MD5 and SHA256. Two prefabs:TaskPanelUI84 source nodes,TaskItemItem12;40 sprites and1 source font. Two original duplicate Image owners cannot be added directly by Unity; importer explicitly retains first owner and appends full-rect child for second Graphic after source siblings. No audiovisual equivalence claimed; source animation/particles pending.',
  'Shared display projection retains config reward arrays and language object; first condition row element1 supplies target, low32 first saved value supplies progress, empty conditions retain previous display. GameValue900001 queried per row filters only id8 when zero. Claimed rows hide; reused rows not automatically reactivated. Progress before row refresh and actual strategy-before-UI claim order retained.',
  'Task row preserves achievement-only geometry mutation, special localization branches, warning/exception boundaries, captured reward count before sprite reentry, source unchecked absolute/rank binary rendering, button enabled/state order, random substitution and shared reward mutation before callback, current data reporting after reentry. Dispose retains row data/delegate.',
  'Click disables native Button, requests observed non-economic currency effect or award popup, then actual scaled localX1600/.7s/ease3 slide. Completion re-enables, invokes actual daily strategy, updates real economy/liveness, hides and reports. Source IsClaim is not set by row; actual activity guard prevents duplicate inventory award. OutSine float/double order source68862 restored without changing default easing.',
  '1463 integrated including12 new; fresh visible PlayMode14 verifies pointer callback, disabled repeat suppression, timeScale0 delay, actual completion award/liveness, original row hide, automatic task save and independent file restart, retained fields/deferred native destruction. Currency/sprite/report/random reward/format endpoints remain required observed hosts, not full production assembly.',
  'Full log retains32 existing ShouldRunBehaviour assertions and one Curl42. Native retains one UnityEditor.Search startup ArgumentOutOfRangeException and one Curl42. No compiler errors. Initial duplicate-Graphic import failure retained as diagnostic; corrected import and both validations pass.',
  'Full TaskPanelUI tabs/lifecycle, liveness reward preview/buttons, main task entry and Achievement pending. Production Main/account/platform/all business/remaining19 controllers, final Player and original audiovisual acceptance incomplete. Lifecycle19/38 unchanged.'
 ],notClaimed='Whole task page/liveness controls/main entry or Achievement acceptance, complete production Main/platform/effect/report hosts, fresh Player/original audiovisual acceptance.')
write(analysis/'task-rows-validation.json',focused)
for name in ('unity-integrated-validation.json','task-rows-integrated.log','task-rows-native-validation.json','task-rows-native.log','task-panel-import.log','task-panel-import-final.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'TASK_ROWS_AUDIT.json',dict(status='original-task-rows-daily-view-native-claims-verified-page-pending',atUtc=now,sourceEvidence='TASK_ROWS_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],nativeChecks=native['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,taskRowsAudit='generated/outgame/TASK_ROWS_AUDIT.json');state['validation'].update(integratedChecksPassed=1463,taskRows=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-task-rows-daily-view-native-claims-verified-page-pending',integratedChecks=1463,newChecks=12,nativeChecks=14,freshPlayModeRun=True,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1463,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for name in ('analysis/task-rows-validation.json','analysis/task-rows-native-validation.json','analysis/unity-task-panel-import-report.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original task row/daily view plus84/12-node static task prefabs restored; native Button pointer, scaled slide, actual daily claim/economy/liveness, automatic save/independent restart.1463 integrated including12 new,14 fresh native checks,{len(fingerprints)} matching inputs,6194 normal methods. Whole-page tabs/liveness preview/Main entry/Achievement/production hosts/Player still pending; lifecycle19/38.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1463,newChecks=12,native=14,fingerprints=len(fingerprints),indexedMethods=6194,controllers=19,remaining=19)))
