"""Record source runtime activity conditions and the recovered generic FSM event bridge."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,value):
    text=json.dumps(value,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes()
    p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed'] and len(full['checks'])==1246 and all(r['result']=='pass' for r in full['checks'])
log=(validation/'activity-conditions-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1246' in log and 'error CS' not in log
focused=dict(full,checks=[r for r in full['checks'] if r['id'].startswith(('activity-condition-','activity-fsm-'))]);assert len(focused['checks'])==10
focused['limitations']='Source runtime activity condition caches/comparison and generic FSM events. Actual recovered statistics drives framework state transition in an Editor fixture. Full ActivityBase/four states/UI/config owner/control/SevenDay/Main pending; no new native or Player.'
assert len(read(out/'method-map.json'))==5833 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==18
evidence=read(out/'ACTIVITY_CONDITIONS_SOURCE_EVIDENCE.json');generic=read(out/'fsm-event-generics.json');assert len(evidence['methods'])==6 and len(generic['methods'])==13
for row in evidence['methods']+generic['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path'] for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameActivityConditions.cs','Scripts/OutgameFsmState.cs','Editor/OutgameActivityConditionsValidation.cs')
paths.update('UnityProject/Assets/AreaBattle/'+name for name in files);fingerprints=[]
for name in sorted(paths):
    data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
    fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2311
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='activity-runtime-conditions-fsm-events';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Implement extracted ActivityBase/four concrete activity states and ActivityControl/config owner graph using recovered condition caches, concrete offline data and FSM events. Connect SevenDay.EnterGameInit and ProcedurePreLoad; wire real Main/account/SDK/HTTP/scene/config hosts, remaining20 controllers/all business/reward-return, then Player and audiovisual acceptance.'
record=dict(atUtc=now,scope='ActivityItemData lazy config and four runtime condition caches, ActivityStateBase condition comparison, generic Fsm/FsmState event dispatch',
    checksPassed=1246,newIntegratedChecks=10,targetedChecks=10,freshPlayModeRun=False,nativeChecks=0,freshPlayerBuild=False,freshPlayerSmoke=False,
    editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),commands=['BattleBuild.ValidateMechanicsOnly'],
    observedBoundaries=[
        'Runtime condition parsing uses parameter-array length, cached config reference, underscore-separated boxed Int32 arguments and Int64 target; invalid date/numeric becomes0, null date args and partial cache on failure preserved.',
        'Runtime comparison gates on nonzero config.open, uses supplied type IDs and exact cached args, reads target after provider reentry. Actual original local config parses and private caches are excluded from JSON/rebuilt after deserialize.',
        'Recovered Fsm current-state event dispatch and FsmState subscriptions preserve duplicate/remove-one semantics, multicast snapshots, reentry, handler exception propagation, payload identity and OnDestroy cleanup. Generic ChangeState validates null and forwards args.',
        'Existing concrete statistics item10700/8 reaches parsed target4 and changes actual recovered FSM state; test states are explicit Editor fixtures. Full activity states/widgets/listeners/config control/Main remain pending.',
        '76 activity base/state bodies extracted for subsequent work, not claimed implemented. Controller bindings unchanged18/38. No new native PlayMode/Player or audiovisual acceptance.',
        'Integration retains32 existing ShouldRunBehaviour editor assertions and one Unity cloud request timeout, same counts as previous1236 run; no compiler errors and all1246 product checks pass.'
    ],notClaimed='Full ActivityBase/four state UI/listeners/config owner/control, SevenDay, production Main/business assembly, remaining20 controller bindings, native activity UI or Player/audiovisual acceptance.')
write(analysis/'activity-conditions-validation.json',focused);shutil.copy2(validation/'analysis/unity-integrated-validation.json',analysis/'unity-integrated-validation.json');shutil.copy2(validation/'activity-conditions-integrated.log',analysis/'activity-conditions-integrated.log')
write(out/'ACTIVITY_CONDITIONS_AUDIT.json',dict(status='conditions-fsm-events-verified-activity-states-pending',atUtc=now,sourceEvidence='ACTIVITY_CONDITIONS_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,activityConditionsAudit='generated/outgame/ACTIVITY_CONDITIONS_AUDIT.json');state['validation'].update(integratedChecksPassed=1246,activityConditions=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='conditions-fsm-events-verified-activity-states-pending',integratedChecks=1246,newChecks=10,nativeChecks=0,remainingLifecycleControllers=20));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path']) for r in manifest['sourceFingerprints'] if r['path'] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1246,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)]);manifest['validationHistory'].append(record);write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append('Runtime activity condition/cache parsing plus generic FSM state events with actual statistics-driven transition;1246 full checks,2311 matching inputs; full activity states/config/control/SevenDay/Main pending, no new native or Player');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1246,newChecks=10,fingerprints=len(fingerprints),indexedMethods=5833,controllers=18,remaining=20)))
