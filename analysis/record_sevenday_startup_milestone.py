"""Record source seven-day controller and native common pre-load/scene integration."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes();p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed'] and len(full['checks'])==1353 and all(r['result']=='pass' for r in full['checks'])
log=(validation/'analysis/sevenday-startup-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1353' in log and 'error CS' not in log
native=read(validation/'analysis/sevenday-startup-native-validation.json');assert native['passed'] and len(native['checks'])==11
native_log=(validation/'analysis/sevenday-startup-native.log').read_text();assert 'error CS' not in native_log
focused=dict(full,checks=[r for r in full['checks'] if r['id'].startswith('sevenday-')]);assert len(focused['checks'])==9
focused['limitations']='Source seven-day controller with concrete limited-task graph/commander sum, dates/red/seed/scene completion semantics. Actual StatisticsRuntime/ActivityRuntime pre-load is verified separately in11 native checks because source singleton objects require PlayMode. Full production Main/account/UI/platform/Player acceptance remains incomplete.'
matrix=read(out/'CONTROLLER_LIFECYCLE_MATRIX.json');assert len(read(out/'method-map.json'))==5962 and matrix['implementedLifecycleControllers']==19
assert all(r['implementation']=='OutgameSevendayActivityControl.cs' for row in matrix['controllers'] if row['typeIndex']==4502 for r in row['lifecycle'])
evidence=read(out/'SEVENDAY_STARTUP_SOURCE_EVIDENCE.json');assert (len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(34,28,13)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path'] for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameSevendayActivityControl.cs','Scripts/OutgameCommonPreLoadHost.cs','Editor/OutgameSevendayValidation.cs','Editor/OutgameCommonStartupValidation.cs','Editor/OutgameCommonStartupPlayModeValidation.cs')
paths.update('UnityProject/Assets/AreaBattle/'+name for name in files);fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2344
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='sevenday-controller-and-common-native-startup';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore original CommonLimitTimeTaskUI and task/day/accumulator/reward page assets, interactions and production seven-day menu/red entry; complete Task/Achievement activities/managers. Continue production Main/account/SDK/HTTP/scene host assembly, remaining19 LogicModule controllers/all gameplay/shop/rewards/return and Player/audiovisual acceptance.'
record=dict(atUtc=now,scope='SevendayActivityControl4502 all methods, concrete commander total level, common pre-load host and seven-day scene-completion binding',checksPassed=1353,newIntegratedChecks=9,targetedChecks=9,nativeChecks=11,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly','OutgameCommonStartupPlayModeValidation.Run'],
 observedBoundaries=[
  'Source34 relevant existing methods,28 fields,13 usages retained; method index5962 unchanged. Controller4502 is now concretely bound; lifecycle matrix19/38. Empty OnInit/Updata preserved, Dispose clears registry only; scene completion separately calls EnterGameInit after level-rank initialization and before play-state1/loading-close/interactive.',
  'Actual child1301 resolves through LimitTimeTaskActivity1301001, itself a child of1300001 in original config. Cached dates derive first over-condition target lowInt32 minus1, unspecified launch-date midnight, and overflowing Int32 millisecond multiplication before Int64 addition. Strict start/end bounds, lazy start0 refresh, explicit reentry refresh, null-child retained dates and exact red state0 verified. Both day/accumulator queries execute when unlocked.',
  'Seeding uses actual skin catalog count, current level-minus1 unchecked/nonnegative clamp, and restored live commander dictionary signedInt32 sum. Callback changes to later level/config keys and failures after date publication retained. Original81-row skin config produces three initial unlocked soldier skins in native startup.',
  'Concrete pre-load host routes original arena provider/statistics/activity endpoints to real StatisticsRuntime/ActivityRuntime. Native11 verifies server response remains pending across frames, activity initialization only after response, actual WaitUntil requires both independently controlled account flags, real FSM transition precedes delayed scene completion, seven-day seed10 and native automatic compressed task save. Required network/account/repair/UI/scene/report hosts remain explicit fixtures.',
  'Integrated1353 including9 new passes; native11 passes. Integrated log retains32 existing ShouldRunBehaviour assertions and one Curl42 abort; native log retains one UnityEditor.Search startup indexing ArgumentOutOfRangeException and one Curl42 abort, zero compiler errors. Initial1355 edit-mode attempt retained: full common runtime needs PlayMode and fixture removal needed ChildActivities lookup. Full pre-load coverage moved to actual native run, not replaced by a synthetic success.',
  '2344 source inputs match isolated project. Native-only fixture/runner skin catalog assertion refined after integrated pass, then compiled and passed native run; gameplay and integrated-test source unchanged. No new Player build, original task-page or complete Main/account/platform acceptance.'
 ],notClaimed='Original common task UI/menu/red entry, complete production Main/SDK/account/HTTP/scene hosts, remaining19 controllers, Task/Achievement and remaining game business, platform success, Player or original audiovisual acceptance.')
write(analysis/'sevenday-startup-validation.json',focused)
for name in ('unity-integrated-validation.json','sevenday-startup-integrated.log','sevenday-startup-editmode-initial.log','sevenday-startup-native-validation.json','sevenday-startup-native.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
shutil.copy2(out/'CONTROLLER_LIFECYCLE_MATRIX.json',validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame/CONTROLLER_LIFECYCLE_MATRIX.json')
write(out/'SEVENDAY_STARTUP_AUDIT.json',dict(status='sevenday-controller-common-native-startup-verified-pages-main-pending',atUtc=now,sourceEvidence='SEVENDAY_STARTUP_SOURCE_EVIDENCE.json',verification=record,implementation=list(files)+['Scripts/OutgameCommanderManager.cs','Scripts/OutgameControllerRegistry.cs','Scripts/OutgameCoreControllerBindings.cs','Scripts/OutgameStartupEntry.cs'],checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,sevendayStartupAudit='generated/outgame/SEVENDAY_STARTUP_AUDIT.json');state['validation'].update(integratedChecksPassed=1353,sevendayStartup=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='sevenday-controller-common-native-startup-verified-pages-main-pending',integratedChecks=1353,newChecks=9,nativeChecks=11,freshPlayModeRun=True,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path']) for r in manifest['sourceFingerprints'] if r['path'] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1353,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)]);manifest['validationHistory'].append(record);write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append('Sevenday controller4502 bound; concrete pre-load StatisticsRuntime/ActivityRuntime and scene callback seven-day seeding verified.1353 integrated including9 new,11 native startup readiness/scene/save checks,2344 matching inputs,5962 method index. Controller lifecycle19/38; task pages/menu and production Main/account/remaining business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1353,newChecks=9,native=11,fingerprints=len(fingerprints),indexedMethods=5962,controllers=19,remaining=19)))
