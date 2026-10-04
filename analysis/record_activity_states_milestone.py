"""Record activity base/four-state implementation and native widget/clock validation once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,value):
    text=json.dumps(value,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes()
    p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');native=read(validation/'analysis/activity-states-native-validation.json')
assert full['passed'] and len(full['checks'])==1256 and all(r['result']=='pass' for r in full['checks'])
assert native['passed'] and len(native['checks'])==10
log=(validation/'analysis/activity-states-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1256' in log and 'error CS' not in log
focused=dict(full,checks=[r for r in full['checks'] if r['id'].startswith('activity-states-')]);assert len(focused['checks'])==10
focused['limitations']='Recovered ActivityBase lifecycle/four concrete states/widgets with actual FSM/statistics/common messages. ActivityControl/config/pop queue/UI page and Main are explicit required hosts. Factory/child lookup remain pending; native suite is separate, no Player/rendered original-page claim.'
assert len(read(out/'method-map.json'))==5833 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==18
evidence=read(out/'ACTIVITY_STATES_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==77 and len(evidence['fields'])==25
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path'] for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameActivityBase.cs','Scripts/OutgameActivityStateBase.cs','Scripts/OutgameActivityStates.cs','Editor/OutgameActivityStatesValidation.cs','Editor/OutgameActivityStatesPlayModeValidation.cs')
paths.update('UnityProject/Assets/AreaBattle/'+name for name in files);fingerprints=[]
for name in sorted(paths):
    data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
    fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2316
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='activity-base-four-states-widgets';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover ActivityBase factory/child lookup and ActivityControl/config owner/pop queue graph; compose concrete offline activity data with these states and SevenDay.EnterGameInit/ProcedurePreLoad. Wire production Main/account/SDK/HTTP/scene/config hosts, remaining20 controllers/all business/reward-return, then Player and audiovisual acceptance.'
record=dict(atUtc=now,scope='ActivityBase lifecycle, Close/Notice/Launch/Over states, original statistics listeners, UGUI buttons/Text and countdown',
    checksPassed=1256,newIntegratedChecks=10,targetedChecks=10,freshPlayModeRun=True,nativeChecks=10,freshPlayerBuild=False,freshPlayerSmoke=False,
    editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),commands=['BattleBuild.ValidateMechanicsOnly','OutgameActivityStatesPlayModeValidation.Run (non-batch Game view)'],
    observedBoundaries=[
        'Actual statistics changes drive Close/Notice/Launch/Over and reset; source initial priorities, nested hook order, popup flags, unknown saved state, init/dirty failure prefixes and child lifecycle retained.',
        'UGUI Button listeners are replaced using source type-based UI host; original notice event can request launch popup twice. Source launch unsubscribe arrays are crossed, leaving dormant callbacks gated by IsLeave. Countdown preserves three-segment format and sticky cached argument array.',
        'Native10 checks exercise Unity pointer dispatch, actual Text/visibility, complete state cycle, owner destruction/recreation, actual automatic statistics clock updates and date-listener removal across frames. UI/pop/control endpoints remain explicit fixtures, not original pages or full Main.',
        'Initial native run timed out waiting for seconds with a >day countdown that intentionally displays only day/hour/minute; eight preceding checks passed. Set fixture server-clock anchor to one hour before threshold, rerun passed10; no production change. Initial log/report preserved.',
        'Source evidence references77 bodies and25 fields, including one explicitly pending factory. Generic child lookup remains pending. Method index5833 and controller bindings18/38 unchanged.',
        'Full integration retains32 ShouldRunBehaviour editor assertions and one Curl42 shutdown abort; native log has UnityEditor.Search startup ArgumentOutOfRangeException and one Curl42 abort. No compiler errors; product1256 and native10 passed.'
    ],notClaimed='ActivityFactoryBase/generic child lookup, ActivityControl/config/popup owner production graph, concrete SevenDay/Main/business assembly, remaining20 controller bindings, rendered original activity page, Player or audiovisual acceptance.')
write(analysis/'activity-states-validation.json',focused)
for name in ('unity-integrated-validation.json','activity-states-native-validation.json','activity-states-integrated.log','activity-states-native-verified.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'ACTIVITY_STATES_AUDIT.json',dict(status='base-four-states-widgets-verified-control-assembly-pending',atUtc=now,sourceEvidence='ACTIVITY_STATES_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,activityStatesAudit='generated/outgame/ACTIVITY_STATES_AUDIT.json');state['validation'].update(integratedChecksPassed=1256,activityStates=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='base-four-states-widgets-verified-control-assembly-pending',integratedChecks=1256,newChecks=10,nativeChecks=10,remainingLifecycleControllers=20));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path']) for r in manifest['sourceFingerprints'] if r['path'] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1256,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)]);manifest['validationHistory'].append(record);write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append('ActivityBase lifecycle/four states and original UGUI button/description/countdown path;1256 full plus10 native,2316 matching inputs; ActivityControl/config/pop owner/SevenDay/Main and factory/child API remain pending');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1256,newChecks=10,nativeChecks=10,fingerprints=len(fingerprints),indexedMethods=5833,controllers=18,remaining=20)))
