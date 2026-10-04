"""Record original accumulator/preview and native automatic claim save/restart."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1401 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/limit-task-acc-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1401'in log and 'error CS'not in log
native=read(validation/'analysis/limit-task-acc-native-validation.json');assert native['passed']and len(native['checks'])==11
native_log=(validation/'analysis/limit-task-acc-native.log').read_text();assert 'error CS'not in native_log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith(('limit-task-acc-','sevenday-acc-preview-'))]);assert len(focused['checks'])==15
focused['limitations']=native['scope']
evidence=read(out/'LIMIT_TASK_ACC_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(33,46,29)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6028 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameLimitTaskAccItem.cs','Scripts/OutgameSevendayAccPreviewItem.cs','Editor/OutgameLimitTaskAccValidation.cs','Editor/OutgameSevendayAccPreviewValidation.cs','Editor/OutgameLimitTaskAccPlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:
 paths.add('UnityProject/Assets/AreaBattle/'+name);paths.add('UnityProject/Assets/AreaBattle/'+name+'.meta')
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='limit-task-accumulator-preview-and-native-restart';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Complete CommonLimitTimeTaskUI lifecycle/refresh/close/countdown and production seven-day menu/red entry over restored day/task/accumulator/preview components. Continue Task/Achievement, full Main/account/SDK/HTTP/scene hosts, remaining19 controllers/all business/rewards/return, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original accumulator claim/preview and native frame wait/update/automatic activity save/restart',checksPassed=1401,newIntegratedChecks=15,targetedChecks=15,nativeChecks=11,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly','OutgameLimitTaskAccPlayModeValidation.Run'],
 observedBoundaries=[
  'Accumulator4348 all10 methods and preview4361 all12 methods plus async4360 MoveNext/SetStateMachine restored. New24 normal methods make index6028; evidence33 methods,46 fields,29 decoded usages. Original templates/outlets, final-id criterion, odd/even box layout and actual child reward engine retained.',
  'Accumulator callback precedes live eligibility query. Actual reward precedes first live display reward read; visual count truncates to lowInt32 only. Currency effect uses applyInventory=false and sends red event only on completion. Other items use legacy paramInt delta1/CoinCost then skin popup. Secondary items use original zero delta/Activity; missing page still processes them. Failure retains already-applied economic state.',
  'Preview UIObject visibility retains active object and uses scaleone/zero. Every visible call registers update; hidden queues removal. EventSystem selected object permits exact touch name or reward-template/Node substring. Refresh clears immediately, awaits actual WaitForEndOfFrame, then uses live data. Concurrent awaits append both sets; hidden preview still builds. Dispose retains managed child references/update callback as source.',
  'Integrated1401 including15 new passes. Native11 verifies original pointer preview, actual frame await, child detail click, deferred destruction, overlapping refresh, UpdateManager null-selection dismissal, actual gold claim and duplicate guard, delayed effect callback, automatic parent save and independent disk restart. Claimed tasks are seeded prerequisites; no complete activity playthrough or global inventory persistence inferred.',
  'Final logs contain zero compiler errors. Integrated retains32 existing ShouldRunBehaviour assertions and one Curl42; native one UnityEditor.Search startup indexing ArgumentOutOfRangeException and one Curl42. All recorded source inputs match the isolated project.',
  'No new Player, complete page/menu/Main/platform or original audiovisual acceptance claimed. Currency animation/skin UI/sprite/localization/detail endpoints remain observed required hosts. Lifecycle19/38 unchanged.'
 ],notClaimed='Complete activity page/menu/Main/account/platform/all business, remaining19 controllers, inventory persistence, Player and original audiovisual acceptance.')

write(analysis/'limit-task-acc-validation.json',focused)
for name in ('unity-integrated-validation.json','limit-task-acc-integrated.log','limit-task-acc-native-validation.json','limit-task-acc-native.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'LIMIT_TASK_ACC_AUDIT.json',dict(status='original-accumulator-preview-native-restart-verified-full-page-pending',atUtc=now,sourceEvidence='LIMIT_TASK_ACC_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,limitTaskAccAudit='generated/outgame/LIMIT_TASK_ACC_AUDIT.json');state['validation'].update(integratedChecksPassed=1401,limitTaskAcc=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-accumulator-preview-native-restart-verified-full-page-pending',integratedChecks=1401,newChecks=15,nativeChecks=11,freshPlayModeRun=True,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1401,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for p in ('analysis/limit-task-acc-validation.json','analysis/limit-task-acc-native-validation.json'):
 if p not in manifest['reports']:manifest['reports'].append(p)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original accumulator claim and async preview restored; native pointer, actual frame await/update dismissal and automatic activity save/restart verified.1401 integrated including15 new,11 native checks,{len(fingerprints)} matching inputs,6028 normal methods. Lifecycle19/38; full page/menu/Main/remaining business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1401,newChecks=15,native=11,fingerprints=len(fingerprints),indexedMethods=6028,controllers=19,remaining=19)))
