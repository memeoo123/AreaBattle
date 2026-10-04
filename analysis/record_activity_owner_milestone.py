"""Record common activity config loading, factory/child API and real data/state restart."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,value):
    text=json.dumps(value,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes()
    p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json')
assert full['passed'] and len(full['checks'])==1266 and all(r['result']=='pass' for r in full['checks'])
log=(validation/'analysis/activity-owner-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1266' in log and 'error CS' not in log
focused=dict(full,checks=[r for r in full['checks'] if r['id'].startswith('activity-owner-')]);assert len(focused['checks'])==10
focused['limitations']='Common activity configuration owner/read lifecycle, base factory/child API, source resources/online JSON and actual activity data/FSM/gzip file restart. Business config dictionaries/groups, ActivityControl/concrete activities/pop queues and Main remain pending. Binary acquisition is an explicit host. No fresh native or Player.'
assert len(read(out/'method-map.json'))==5849 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==18
evidence=read(out/'ACTIVITY_OWNER_SOURCE_EVIDENCE.json');generic=read(out/'activity-owner-generics.json');assert len(evidence['methods'])==25 and len(evidence['fields'])==41 and len(generic['methods'])==4
for row in evidence['methods']+generic['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path'] for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameActivityConfigManager.cs','Scripts/OutgameActivityConfigReader.cs','Scripts/OutgameActivityFactory.cs','Editor/OutgameActivityOwnerValidation.cs')
paths.update('UnityProject/Assets/AreaBattle/'+name for name in files);fingerprints=[]
for name in sorted(paths):
    data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
    fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2320
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='activity-common-config-owner-factory';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore remaining ActivityConfigMgr task/achievement/novice/liveness dictionaries and custom loaders; implement ActivityControl activity registration/state bookkeeping/pop queues/update/daily/item hooks, compose production config/data/states and SevenDay/ProcedurePreLoad. Then finish Main/account/SDK/HTTP/scene hosts, remaining20 controllers/all business/reward-return and Player/audiovisual acceptance.'
record=dict(atUtc=now,scope='Common activity config owner/reader and source JSON resources, ActivityBase factory/child lookup, actual configuration-to-data-to-state file restart',
    checksPassed=1266,newIntegratedChecks=10,targetedChecks=10,freshPlayModeRun=False,nativeChecks=0,freshPlayerBuild=False,freshPlayerSmoke=False,
    editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),commands=['BattleBuild.ValidateMechanicsOnly'],
    observedBoundaries=[
        'Common config loading preserves custom subclass discovery, live lists, repeated-read counters, equality-based callback, current-owner normalization, partial failures, retained fields on Dispose. Nine original local activity rows and original settings loaded; exactly open1 missing close/over conditions normalized to -1/999999.',
        'Online common_ActivityConfig overrides only activity table, keeps first duplicate/logs original message, parses from first brace; settings route rereads binary flag and callback uses captured argument. Actual Unity JSON acquisition provided; binary decoder remains explicit required host.',
        'ActivityBase factory and generic child lookup restored. Base item factory rereads original item catalog through selected subfactory; original currency/exp/package Produce bodies explicitly return null. Missing config and invalid generic cast failures preserved, no invented entity/reward.',
        'Editor integration composes actual config owner, activity data manager/offline strategy, recovered statistics/FSM/widgets and gzip file backend. Independent config/data/owner graphs restore notice state/warm timestamp; ActivityControl/pop/page endpoints remain fixture boundaries.',
        'Source25 bodies/41 fields plus4 generics (two ActivityControl getters extracted but not implemented);16 new indexed bodies, total5849. Business config groups and full control/concrete activity/Main assembly pending. Lifecycle bindings unchanged18/38.',
        'Full integration retains32 existing ShouldRunBehaviour editor assertions and one Curl42 shutdown abort; no compiler errors and all1266 product checks pass. No fresh native/Player; latest native10 remains previous1256 activity state run.'
    ],notClaimed='Full task/achievement/novice/liveness configuration surfaces, actual ActivityControl/pop queue/concrete activities/SevenDay/Main production assembly, remaining20 controller bindings, binary/platform acquisition, fresh native/Player or original-page audiovisual acceptance.')
write(analysis/'activity-owner-validation.json',focused)
for name in ('unity-integrated-validation.json','activity-owner-integrated.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'ACTIVITY_OWNER_AUDIT.json',dict(status='common-config-factory-verified-control-business-config-pending',atUtc=now,sourceEvidence='ACTIVITY_OWNER_SOURCE_EVIDENCE.json',verification=record,implementation=list(files)+['Scripts/OutgameActivityBase.cs'],checks=focused['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,activityOwnerAudit='generated/outgame/ACTIVITY_OWNER_AUDIT.json');state['validation'].update(integratedChecksPassed=1266,activityOwner=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='common-config-factory-verified-control-business-config-pending',integratedChecks=1266,newChecks=10,nativeChecks=0,remainingLifecycleControllers=20));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path']) for r in manifest['sourceFingerprints'] if r['path'] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1266,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)]);manifest['validationHistory'].append(record);write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append('Common ActivityConfigMgr loading/reader with original resources and online JSON, base factory/child API, actual config/data/FSM compressed file restart;1266 full,2320 matching inputs; business config groups/ActivityControl/SevenDay/Main pending, no fresh native/Player');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1266,newChecks=10,fingerprints=len(fingerprints),indexedMethods=5849,controllers=18,remaining=20)))
