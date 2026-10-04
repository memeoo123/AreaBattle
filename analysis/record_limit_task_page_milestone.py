"""Record complete limited-task page lifecycle and native composed flow/restart."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1410 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/limit-task-page-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1410'in log and 'error CS'not in log
native=read(validation/'analysis/limit-task-page-native-validation.json');assert native['passed']and len(native['checks'])==17
native_log=(validation/'analysis/limit-task-page-native.log').read_text();assert 'error CS'not in native_log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('limit-task-page-')]);assert len(focused['checks'])==9
focused['limitations']=native['scope']
evidence=read(out/'LIMIT_TASK_PAGE_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(31,24,34)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6044 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameLimitTaskPage.cs','Scripts/OutgameLimitTaskListBinding.cs','Scripts/OutgameLimitTaskDayListBinding.cs','Editor/OutgameLimitTaskPageValidation.cs','Editor/OutgameLimitTaskPagePlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:
 paths.add('UnityProject/Assets/AreaBattle/'+name);paths.add('UnityProject/Assets/AreaBattle/'+name+'.meta')
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='limit-task-complete-page-and-native-restart';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore production seven-day menu entry/red/countdown subscriptions, then continue Task/Achievement and full Main/account/SDK/HTTP/scene assembly, remaining19 controllers/all business/rewards/return, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Complete original limited-task page over recovered BaseUI and actual activity notifications, native open/claim/preview/close/save/restart',checksPassed=1410,newIntegratedChecks=9,targetedChecks=9,nativeChecks=17,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly','OutgameLimitTaskPagePlayModeValidation.Run'],
 observedBoundaries=[
  'CommonLimitTimeTaskUI4310 all15 methods restored with day comparator32896 and TimeUtility32655. New16 normal methods make index6044; evidence31 methods,24 fields,34 decoded usages. Original module namespace/resource path, layer2/open0/close0 and recovered BaseUI loading/close used.',
  'Awake preview/mode labels/activity/config/buttons/listeners/countdown/lists/report order retained. Selected renderer drives task and accumulator population; day pointer writes lastClickDayId without Dirty. Real task/accumulator activity notifications drive page listeners without test forwarding. Accumulator callback can continue after its page synchronously disposed/rebuilt it; native deferred destruction preserves source behavior.',
  'Countdown uses actual SevenDay cached interval and live clock, Chinese day+hour or hour+minute+second formatting, unchecked narrowing and original repeated end message without auto-close. Source malformed message guards and otherwise-unused accumulator-mode lookup preserve partial failure. Preview world position/list/frame refresh composed.',
  'Dispose clears BaseUI lifetime, removes four page listeners, deselects days and disposes accumulator values. Providers/dictionary/preview references remain as source. Edit fixture defers root destruction through disposal; native test verifies actual two-frame opening and frame-separated async close/main handle release.',
  'Integrated1410 including9 new passes. Native17 verifies original bootstrap/popup, actual day/task/accumulator pointers, page-owned refresh, real preview await, synchronous accumulator rebuild, duplicate guard, UpdateManager hiding, automatic activity save, ended-but-open page, close callback/registry/hide/dispose/release order, and independent file restart/reopen. Additional claimed tasks are seeded; inventory persistence and full activity playthrough not inferred.',
  'Final logs contain zero compiler errors. Integrated retains32 existing ShouldRunBehaviour assertions and one Curl42; native one UnityEditor.Search startup indexing ArgumentOutOfRangeException and one Curl42. All recorded inputs match isolated project.',
  'Resource acquisition/report/localization/sprite/detail/currency animation/skin UI hosts remain explicit fixture boundaries. Production menu/Main/account/platform/all business and new Player/original audiovisual acceptance not claimed. Lifecycle19/38 unchanged.'
 ],notClaimed='Production menu/Main/account/platform/all business, remaining19 controllers, inventory persistence, new Player and original audiovisual acceptance.')

write(analysis/'limit-task-page-validation.json',focused)
for name in ('unity-integrated-validation.json','limit-task-page-integrated.log','limit-task-page-native-validation.json','limit-task-page-native.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'LIMIT_TASK_PAGE_AUDIT.json',dict(status='original-complete-page-native-restart-verified-menu-main-pending',atUtc=now,sourceEvidence='LIMIT_TASK_PAGE_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,limitTaskPageAudit='generated/outgame/LIMIT_TASK_PAGE_AUDIT.json');state['validation'].update(integratedChecksPassed=1410,limitTaskPage=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-complete-page-native-restart-verified-menu-main-pending',integratedChecks=1410,newChecks=9,nativeChecks=17,freshPlayModeRun=True,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1410,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for p in ('analysis/limit-task-page-validation.json','analysis/limit-task-page-native-validation.json'):
 if p not in manifest['reports']:manifest['reports'].append(p)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Complete original limited-task page with recovered BaseUI, own activity listeners, source countdown/preview/close restored; native actual pointers, automatic activity save and independent full-page restart verified.1410 integrated including9 new,17 native checks,{len(fingerprints)} matching inputs,6044 normal methods. Lifecycle19/38; production menu/Main/remaining business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1410,newChecks=9,native=17,fingerprints=len(fingerprints),indexedMethods=6044,controllers=19,remaining=19)))
