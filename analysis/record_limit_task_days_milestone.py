"""Record source selection provider and native original seven-day list integration."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1377 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/limit-task-days-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1377'in log and 'error CS'not in log
native=read(validation/'analysis/limit-task-days-native-validation.json');assert native['passed']and len(native['checks'])==9
native_log=(validation/'analysis/limit-task-days-native.log').read_text();assert 'error CS'not in native_log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith(('dynamic-selection-','limit-task-days-'))]);assert len(focused['checks'])==14
focused['limitations']=native['scope']
evidence=read(out/'LIMIT_TASK_DAYS_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['generic']['methods']),len(evidence['fields']),len(evidence['usages']))==(18,11,15,23)
for row in evidence['methods']+evidence['generic']['methods']+[evidence['generic']['defaultDurationWrapper']]:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==5991 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameDynamicSelection.cs','Scripts/OutgameLimitTaskPageItem.cs','Scripts/OutgameLimitTaskDayListBinding.cs','Scripts/OutgameLimitTaskModels.cs','Scripts/OutgameImportedUiOutlets.cs','Editor/OutgameDynamicSelectionValidation.cs','Editor/OutgameLimitTaskDaysValidation.cs','Editor/OutgameLimitTaskDaysPlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:
 paths.add('UnityProject/Assets/AreaBattle/'+name);paths.add('UnityProject/Assets/AreaBattle/'+name+'.meta')
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='limit-task-selection-and-original-day-list';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore CommonLimitTimeTaskItem task rows, LimitTimeTaskAccItem, SevendayAccPreviewItem and complete CommonLimitTimeTaskUI page lifecycle/refresh/claims over the recovered day selection list, then production seven-day menu/red entry. Continue Task/Achievement, full Main/account/SDK/HTTP/scene hosts, remaining19 controllers/all business/rewards/return, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original shared selection provider/renderer and seven-day list/item binding with source-identity outlet resolution',checksPassed=1377,newIntegratedChecks=14,targetedChecks=14,nativeChecks=9,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly','OutgameLimitTaskDaysPlayModeValidation.Run'],
 observedBoundaries=[
  'Source shared selection provider/renderer11 generic methods plus compiler default-duration wrapper restored; normal page4338 adds13 methods. Evidence18 normal methods,15 fields,23 usages; normal index5991. Selection lives on source task/day data. SetSelect preserves transition-before-callback, count after select, live data scan, GetHashCode-based deselection and optional centering. Refresh replays selection after rendering; hash collisions, callback mutation and failures verified.',
  'CommonLimitTimeTaskPageItem binds actual child1301 and original seven day rows. Normal Button/locked Image/selected Image/name/red outlets, localization side effects before Chinese day-label overwrite, current-day gate, button selection message, single-row hiding, short-array/unbox failure behavior and DynamicBaseItem ownership/listener disposal restored. The page event endpoint is observed; complete page task refresh remains pending.',
  'Two original sibling Scroll Views share a name. Original source-object IDs now determine exported outlet paths; runtime importer adapter walks preserved sibling topology. Exact day-list component6678087315481976053 is bound with its own scroll reference, leading30/spacing30/single fixed column and non-recycling settings. Previous static import layout stays unchanged; prior raw outlet name paths were ambiguous.',
  'Integrated1377 including14 new passes. Native9 proves actual DynamicList LateUpdate, original button pointers and locked day, concrete statistic10015 to original task progress and day red, selected-page replay after native list refresh, common-message red clearing, and persistent native row after DynamicBaseItem disposal with listeners removed.',
  'Integrated log retains32 existing ShouldRunBehaviour assertions and one Curl42; native log one UnityEditor.Search startup indexing ArgumentOutOfRangeException and one Curl42; final runs have no compiler errors. Initial compile attempt failed on a new test variable name collision, corrected and initial log retained. Native-only runner added after integrated pass and then compiled/passed; gameplay and integrated test sources unchanged.',
  'All recorded inputs match isolated project. No new Player build, whole-page task/accumulator/preview/main/menu acceptance, platform success or original audiovisual equivalence claimed. Lifecycle19/38 unchanged.'
 ],notClaimed='Task rows, accumulator/preview/full page, production menu/Main/account/platform/all business, remaining19 controllers, Player and original audiovisual acceptance.')
write(analysis/'limit-task-days-validation.json',focused)
for name in ('unity-integrated-validation.json','limit-task-days-integrated.log','limit-task-days-initial-compile.log','limit-task-days-native-validation.json','limit-task-days-native.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'LIMIT_TASK_DAYS_AUDIT.json',dict(status='selection-and-original-day-list-verified-full-page-pending',atUtc=now,sourceEvidence='LIMIT_TASK_DAYS_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,limitTaskDaysAudit='generated/outgame/LIMIT_TASK_DAYS_AUDIT.json');state['validation'].update(integratedChecksPassed=1377,limitTaskDays=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='selection-and-original-day-list-verified-full-page-pending',integratedChecks=1377,newChecks=14,nativeChecks=9,freshPlayModeRun=True,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1377,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for p in ('analysis/limit-task-days-validation.json','analysis/limit-task-days-native-validation.json'):
 if p not in manifest['reports']:manifest['reports'].append(p)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Shared selection and original seven-day list/item now bound to actual task activity; source-object outlet fix for duplicate Scroll Views.1377 integrated including14 new,9 native checks,{len(fingerprints)} matching inputs,5991 normal method index plus11 selection generic bodies. Lifecycle19/38; task/accumulator/preview/full page/menu/Main/remaining business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1377,newChecks=14,native=9,fingerprints=len(fingerprints),indexedMethods=5991,controllers=19,remaining=19)))
