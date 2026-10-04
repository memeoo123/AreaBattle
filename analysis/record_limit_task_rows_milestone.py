"""Record original task rows, actual pointer claim and native task save/restart."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1386 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/limit-task-rows-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1386'in log and 'error CS'not in log
native=read(validation/'analysis/limit-task-rows-native-validation.json');assert native['passed']and len(native['checks'])==10
native_log=(validation/'analysis/limit-task-rows-native.log').read_text();assert 'error CS'not in native_log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('limit-task-rows-')]);assert len(focused['checks'])==9
focused['limitations']=native['scope']
evidence=read(out/'LIMIT_TASK_ROWS_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(21,27,16)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6004 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameLimitTaskItem.cs','Scripts/OutgameLimitTaskListBinding.cs','Editor/OutgameLimitTaskRowsValidation.cs','Editor/OutgameLimitTaskRowsPlayModeValidation.cs','Editor/OutgameLimitTaskDaysValidation.cs','Editor/BattleBuild.cs')
for name in files:
 paths.add('UnityProject/Assets/AreaBattle/'+name);paths.add('UnityProject/Assets/AreaBattle/'+name+'.meta')
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='limit-task-original-rows-claim-and-native-restart';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore LimitTimeTaskAccItem and SevendayAccPreviewItem, then complete CommonLimitTimeTaskUI lifecycle/refresh/close/countdown and production seven-day menu/red entry over recovered day/task lists. Continue Task/Achievement, full Main/account/SDK/HTTP/scene hosts, remaining19 controllers/all business/rewards/return, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original task rows/list rendering, actual task claim and native automatic task save/restart',checksPassed=1386,newIntegratedChecks=9,targetedChecks=9,nativeChecks=10,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly','OutgameLimitTaskRowsPlayModeValidation.Run'],
 observedBoundaries=[
  'CommonLimitTimeTaskItem4337 all11 methods plus source page RenderDay32880 and comparator32897 restored. New13 normal methods make index6004; evidence21 methods,27 fields,16 decoded usages. Task provider follows actual parent1301001 and child dictionary by current task activityId; original templates and layout parents used.',
  'Renderer clears rewards before fetching new data, rebuilds reward/progress clones with original callbacks and partial failures, formats configured target values, then queries button state separately after localization. Goto handler remains empty. Dispose clears row references and rewards only; progress/native root/buttons/data remain as source. No invented cleanup or navigation.',
  'Claim invokes actual Child.TaskComplete then sends SevendayFinishTask from current data after callbacks. Duplicate/rejected claims still notify; thrown report prevents finish after already-applied award/state. Callback rebinding changes final message as source. Native real button delivers liveness10 through original activity factory and reward engine.',
  'Task list preserves source spacing8/leading10/fixed centered column/non-recycling and correct Content scroll. RenderDay preserves live show-condition threshold reads around GameValue, includes claimed rows, sorts BtnState only, centers index0 then updates list. Page lifecycle and automatic production callback registration still pending; test forwarding exercises recovered render method explicitly.',
  'Integrated1386 including9 new passes. Native10 verifies original49-task/seven-day graph, real pointer claim, same-frame duplicate guard, frame-deferred cloned-item destruction, actual UpdateManager automatic parent task save, independent file restart and original reopened claimed row. Global inventory persistence is a separate required host contract, not inferred from task restart.',
  'Final logs contain zero compiler errors. Integrated retains32 existing ShouldRunBehaviour assertions and one Curl42; native one UnityEditor.Search startup indexing ArgumentOutOfRangeException and one Curl42. Native-only runner added after integrated pass then compiled/passed; runtime/integrated test sources unchanged. All recorded inputs match isolated project.',
  'No new Player, complete page/accumulator/preview/menu/Main/platform or original audiovisual acceptance claimed. Required sprite/localization/detail endpoints remain observed test hosts. Lifecycle19/38 unchanged.'
 ],notClaimed='Accumulator/preview/complete page, production menu/Main/account/platform/all business, remaining19 controllers, Player and original audiovisual acceptance.')
write(analysis/'limit-task-rows-validation.json',focused)
for name in ('unity-integrated-validation.json','limit-task-rows-integrated.log','limit-task-rows-native-validation.json','limit-task-rows-native.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'LIMIT_TASK_ROWS_AUDIT.json',dict(status='original-task-rows-claim-and-native-restart-verified-full-page-pending',atUtc=now,sourceEvidence='LIMIT_TASK_ROWS_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,limitTaskRowsAudit='generated/outgame/LIMIT_TASK_ROWS_AUDIT.json');state['validation'].update(integratedChecksPassed=1386,limitTaskRows=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-task-rows-claim-and-native-restart-verified-full-page-pending',integratedChecks=1386,newChecks=9,nativeChecks=10,freshPlayModeRun=True,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1386,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for p in ('analysis/limit-task-rows-validation.json','analysis/limit-task-rows-native-validation.json'):
 if p not in manifest['reports']:manifest['reports'].append(p)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original task rows/render filtering/button-state sorting and actual claim bound; native original pointer to liveness award, automatic task save/restart verified.1386 integrated including9 new,10 native checks,{len(fingerprints)} matching inputs,6004 normal methods. Lifecycle19/38; accumulator/preview/full page/menu/Main/remaining business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1386,newChecks=9,native=10,fingerprints=len(fingerprints),indexedMethods=6004,controllers=19,remaining=19)))
