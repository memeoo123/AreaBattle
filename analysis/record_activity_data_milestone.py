"""Record verified source activity manager/offline persistence; no new native/Player claim."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path)
validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,value):
    crlf=p.exists() and b'\r\n' in p.read_bytes();text=json.dumps(value,ensure_ascii=False,indent=2)+'\n'
    p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json')
assert full['passed'] and len(full['checks'])==1236 and all(r['result']=='pass' for r in full['checks'])
log=(validation/'activity-data-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1236' in log and 'error CS' not in log
focused=dict(full,checks=[r for r in full['checks'] if r['id'].startswith('activity-data-')]);assert len(focused['checks'])==12
focused['limitations']='Original activity manager, concrete offline strategy, condition/reconciliation rules and real gzip file restart. Full ActivityControl/config loading/common-manager production factories/SevenDay/Main pending. No fresh native PlayMode or Player.'
assert read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==18
assert len(read(out/'method-map.json'))==5757
evidence=read(out/'ACTIVITY_DATA_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==33
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path'] for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameActivityData.cs','Scripts/OutgameActivityCodec.cs','Scripts/OutgameActivityManager.cs','Scripts/OutgameActivityStrategy.cs','Editor/OutgameActivityDataValidation.cs')
paths.update('UnityProject/Assets/AreaBattle/'+name for name in files)
fingerprints=[]
for name in sorted(paths):
    data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
    fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2308
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='activity-manager-offnet-data'
state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore ActivityControl/ActivityConfigMgr runtime and activity condition/subclass graph using the concrete ActivityManager/offnet data path; connect SevenDay.EnterGameInit and ProcedurePreLoad. Wire real Main/account/SDK/HTTP/transition/scene/config hosts; complete remaining20 controllers/all business/reward-return, then Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='ActivityManager/CommonModuleManagerBase/concrete offline strategy, persisted DTO/codec and source condition/reconciliation rules',
    checksPassed=1236,newIntegratedChecks=12,targetedChecks=12,freshPlayModeRun=False,nativeChecks=0,freshPlayerBuild=False,freshPlayerSmoke=False,
    editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),
    commands=['BattleBuild.ValidateMechanicsOnly'],observedBoundaries=[
        'Twelve new cases include48 launch/notice/over/status combinations, duplicate first-match/orphan retention, actual data pool and gzip file save plus independent backend/manager restart with signed64 extremes.',
        'Source registration callback reread/reentry, constructor before config guard, duplicate strategy initialization, partial failures, null data, release only resetting registration flag and retained dirty on save verified.',
        'Concrete offline strategy delays for actual server callback, handles legacy raw JSON and invalid JSON via original warning/error/recovery sequence; required hosts are explicit, no server result synthesized.',
        'ActivityControl/config selection/production reflected manager factories/condition caches/SevenDay and Main are pending. Existing ActivityConfig local reader gained original fields without changing its query behavior.',
        'Unity integration log retains32 existing ShouldRunBehaviour editor assertions and one Unity cloud request timeout, same counts as prior1224 run; all1236 product checks pass and no compiler errors.',
        'Latest native15 remains the prior frame-launch suite, not rerun in this data-only batch. No new Player or original audiovisual acceptance.'
    ],notClaimed='Full activity owner/configuration lifecycle, SevenDay, remaining20 controller bindings, complete Main/account/business graph, native activity UI, Player or audiovisual acceptance.')
write(analysis/'activity-data-validation.json',focused);shutil.copy2(validation/'analysis/unity-integrated-validation.json',analysis/'unity-integrated-validation.json');shutil.copy2(validation/'activity-data-integrated.log',analysis/'activity-data-integrated.log')
write(out/'ACTIVITY_DATA_AUDIT.json',dict(status='offline-data-verified-activity-owner-pending',atUtc=now,sourceEvidence='ACTIVITY_DATA_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,activityDataAudit='generated/outgame/ACTIVITY_DATA_AUDIT.json');state['validation'].update(integratedChecksPassed=1236,activityData=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='offline-data-verified-activity-owner-pending',integratedChecks=1236,newChecks=12,nativeChecks=0,remainingLifecycleControllers=20));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path']) for r in manifest['sourceFingerprints'] if r['path'] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1236,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)]);manifest['validationHistory'].append(record);write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append('Activity manager/common base/offline persistence, gzip codec, conditions and48 state combinations;1236 full checks,2308 matching inputs; ActivityControl/config/SevenDay/Main production hosts pending; no new native or Player')
write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1236,newChecks=12,fingerprints=len(fingerprints),indexedMethods=5757,controllers=18,remaining=20)))
