"""Record full activity business config schemas/groups and concrete custom-manager roster."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,value):
    text=json.dumps(value,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes()
    p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json')
assert full['passed'] and len(full['checks'])==1276 and all(r['result']=='pass' for r in full['checks'])
log=(validation/'analysis/activity-business-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1276' in log and 'error CS' not in log
focused=dict(full,checks=[r for r in full['checks'] if r['id'].startswith('activity-business-')]);assert len(focused['checks'])==10
focused['limitations']='Complete configuration DTO/query/group and original three custom managers with actual recovered JSON resources and runtime composition. ActivityControl/concrete activities/UI/Main and binary/platform acquisition remain pending. No new native or Player.'
assert len(read(out/'method-map.json'))==5871 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==18
evidence=read(out/'ACTIVITY_BUSINESS_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==40 and len(evidence['fields'])==102 and len(evidence['schemas'])==9
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path'] for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameActivityBusinessSchemas.cs','Scripts/OutgameActivityBusinessConfig.cs','Scripts/OutgameActivityCustomConfigs.cs','Scripts/OutgameActivityConfigRuntime.cs','Editor/OutgameActivityBusinessValidation.cs')
paths.update('UnityProject/Assets/AreaBattle/'+name for name in files);fingerprints=[]
for name in sorted(paths):
    data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
    fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2325
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='activity-business-config-composition';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Implement ActivityControl registration/state bookkeeping/pop queues/update/daily/item hooks using complete activity config roster plus actual offline data/states; restore concrete activities and SevenDay/ProcedurePreLoad. Finish production Main/account/SDK/HTTP/scene hosts, remaining20 controllers/all business/reward-return and Player/audiovisual acceptance.'
record=dict(atUtc=now,scope='ActivityConfigMgr nine business schemas, remaining query/group methods, NoviceTask/Task/Achievement custom managers and concrete full config runtime',
    checksPassed=1276,newIntegratedChecks=10,targetedChecks=10,freshPlayModeRun=False,nativeChecks=0,freshPlayerBuild=False,freshPlayerSmoke=False,
    editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),commands=['BattleBuild.ValidateMechanicsOnly'],
    observedBoundaries=[
        'Metadata-backed nine DTOs preserve original names, Int64 and nested ListArrayInt/Lang; shared achievement accumulator schema only has id. All remaining config group/query/null/missing diagnostics match source. Complete custom-manager inheritance roster is exactly4668/4675/4713.',
        'Actual source resources load49 novice tasks/8 rewards/1 NT activity,8 tasks/3 liveness/1 task group/1 task activity,127 achievements/3 accumulator rows, then9 common activities/settings. Production config runtime reaches Completed==Expected==5 through real concrete loaders.',
        'Source task list groups append and remain live; novice/liveness inner Add duplicates throw; novice activities obtain empty task groups. Achievement condition value keys group by contentType/content and sort by signed priority. Acquisition and grouping failures preserve prefix and suppress completion.',
        'Novice/task capture binary mode once for their whole read sequence; Achievement always uses Unity JSON. Binary acquisition remains explicit required host. Load/release repeatedly resolve current owner, with captured first destination and source ordered partial failures.',
        'Runtime disposal retains TasksByGroup as original35395; subsequent successful read appends8 new rows to8 retained old rows. Re-read without dispose appends custom instances then fails duplicate novice group after changing expected count. These original behaviors are explicitly tested.',
        'Source40 bodies/102 fields/nine schemas and complete three-manager roster,22 new indexed bodies, total5871. Controller lifecycle bindings remain18/38; ActivityControl/concrete gameplay/Main assembly pending.',
        'Full integration retains32 existing ShouldRunBehaviour editor assertions and one Curl42 shutdown abort; no compiler errors and all1276 product checks pass. No fresh native/Player; native10 remains prior1256 activity state run.'
    ],notClaimed='ActivityControl/concrete activities/pop queues/SevenDay/Main and business UI/rewards production assembly, remaining20 controller bindings, binary/platform implementation, fresh native/Player or original-page audiovisual acceptance.')
write(analysis/'activity-business-validation.json',focused)
for name in ('unity-integrated-validation.json','activity-business-integrated.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'ACTIVITY_BUSINESS_AUDIT.json',dict(status='complete-config-roster-verified-activity-control-pending',atUtc=now,sourceEvidence='ACTIVITY_BUSINESS_SOURCE_EVIDENCE.json',verification=record,implementation=list(files)+['Scripts/OutgameActivityConfigManager.cs'],checks=focused['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,activityBusinessAudit='generated/outgame/ACTIVITY_BUSINESS_AUDIT.json');state['validation'].update(integratedChecksPassed=1276,activityBusiness=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='complete-config-roster-verified-activity-control-pending',integratedChecks=1276,newChecks=10,nativeChecks=0,remainingLifecycleControllers=20));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path']) for r in manifest['sourceFingerprints'] if r['path'] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1276,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)]);manifest['validationHistory'].append(record);write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append('Complete activity config nine schemas/query/groups and original three concrete custom managers/runtime with original resources/release/reread/failures;1276 full,2325 matching inputs; ActivityControl/concrete gameplay/SevenDay/Main pending, no fresh native/Player');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1276,newChecks=10,fingerprints=len(fingerprints),indexedMethods=5871,controllers=18,remaining=20)))
