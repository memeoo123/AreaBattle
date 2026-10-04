"""Record verified common ActivityControl and actual config/data/FSM composition."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,value):
    text=json.dumps(value,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes()
    p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');native=read(validation/'analysis/activity-control-native-validation.json')
assert full['passed'] and len(full['checks'])==1292 and all(r['result']=='pass' for r in full['checks'])
assert native['passed'] and len(native['checks'])==13
log=(validation/'analysis/activity-control-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1292' in log and 'error CS' not in log
native_log=(validation/'analysis/activity-control-native.log').read_text();assert 'error CS' not in native_log
focused=dict(full,checks=[r for r in full['checks'] if r['id'].startswith('activity-control-')]);assert len(focused['checks'])==16
focused['limitations']=native['scope']
assert len(read(out/'method-map.json'))==5873 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==18
evidence=read(out/'ACTIVITY_CONTROL_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==43 and len(evidence['sharedGenerics'])==2 and len(evidence['fields'])==44
for row in evidence['methods']+evidence['sharedGenerics']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path'] for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameActivityControl.cs','Scripts/OutgameActivityRuntime.cs','Editor/OutgameActivityControlValidation.cs','Editor/OutgameActivityControlPlayModeValidation.cs')
paths.update('UnityProject/Assets/AreaBattle/'+name for name in files);fingerprints=[]
for name in sorted(paths):
    data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
    fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2329
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='activity-control-runtime-composition';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore attributed concrete activity inheritance/manager/factory roster, FatherActivityBase/ChildActivityBase and ChildLimitTimeTaskActivity4698; bind SevenDay.EnterGameInit and ProcedurePreLoad to actual shared activity runtime. Finish production Main/account/SDK/HTTP/scene hosts, remaining20 LogicModule controllers/all gameplay/shop/rewards/return and Player/audiovisual acceptance.'
record=dict(atUtc=now,scope='Complete common ActivityControl registration/data readiness/widgets/popups/update/daily/item hooks/report ordering and real configuration/offline manager/FSM runtime',
    checksPassed=1292,newIntegratedChecks=16,targetedChecks=16,freshPlayModeRun=True,nativeChecks=13,freshPlayerBuild=False,freshPlayerSmoke=False,
    editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),commands=['BattleBuild.ValidateMechanicsOnly','OutgameActivityControlPlayModeValidation.Run'],
    observedBoundaries=[
        'All36 original ActivityControl methods restored through34 normal bodies/two shared generic implementations, plus nested comparators/empty init DTO/attribute/parent-report cache. Source43 normal bodies,2 shared generics,44 fields; metadata Type.Equals slot126 corrected and explicitly tested separately from IsSubclassOf slot21. Method index5873.',
        'Actual full original config roster/offline compressed storage/pool/statistics/FSM graph works through Control.Init. Reflection parent/child/pending override, duplicate/null failures, exact-base child refresh, real UGUI area/sibling/listener arrangement and common item factory hook validated. Source global hook retained on cleanup, remove+append deduplicates repeated Init; fresh manager can differ from earlier pool duplicate.',
        'Popup queues use signed descending priorities and exact UI types. Automatic notice marks before UI call, dirties current owner, removes one item after success; failure retains queue. Forward close removal skips adjacent matches and automatic popup can retain blocking NoticeUi reference, matching original.',
        'Launch/Warm/Over virtual hooks, captured/reread Data timestamps, stats reset, mutable report payload initialization/message/delivery ordering match source. Parent-report cache preserves missing-parent separators. Reports/UI delivery remain explicit required app endpoints; no platform success fabricated.',
        'Minute and daily counters preserve callback-sensitive row rereads, signed truncation and unchecked overflow. Update subtracts60 once and activity dirty clears after successful IO; pool disable and actual changed-payload write failure covered. Initial unchanged-payload failure test corrected for SDK write deduplication; initial compile/runtime logs retained.',
        'Native13 on final code covers real UpdateManager frames, source state transitions, Unity pointer clicks, automatic popup and reentrant OpenUI, source CloseUI retention, compressed save and independent owner restart, cleanup and over/close. Scheduler registration captured explicitly in these fixtures; prior native daily scheduler evidence remains separate.',
        'Integrated1292 pass with32 existing ShouldRunBehaviour assertions and one Curl42 shutdown abort, zero compiler errors. Native log retains one UnityEditor.Search startup ArgumentOutOfRangeException;13 product checks pass. Source2329 inputs match validation copy. Controller lifecycle remains18/38 because shared ActivityControl is outside LogicModule roster.'
    ],notClaimed='Concrete activity subclasses/managers/factories/business UI/rewards and SevenDay/ProcedurePreLoad/Main assembly, remaining20 controllers, real report/UI/platform/account delivery, original-page audiovisual acceptance or new Player build.')
write(analysis/'activity-control-validation.json',focused)
for name in ('unity-integrated-validation.json','activity-control-integrated.log','activity-control-native.log','activity-control-native-validation.json','activity-control-integrated-initial-compile.log','activity-control-integrated-initial-runtime.log','before-type-equality-fix-activity-control-integrated.log','before-type-equality-fix-activity-control-native.log','before-type-equality-fix-activity-control-native-validation.json'):
    shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'ACTIVITY_CONTROL_AUDIT.json',dict(status='common-control-runtime-verified-concrete-gameplay-pending',atUtc=now,sourceEvidence='ACTIVITY_CONTROL_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,activityControlAudit='generated/outgame/ACTIVITY_CONTROL_AUDIT.json');state['validation'].update(integratedChecksPassed=1292,activityControl=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='common-control-runtime-verified-concrete-gameplay-pending',integratedChecks=1292,newChecks=16,nativeChecks=13,remainingLifecycleControllers=20));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path']) for r in manifest['sourceFingerprints'] if r['path'] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1292,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)]);manifest['validationHistory'].append(record);write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append('Common ActivityControl full registration/data/UI queues/update/daily/item hook/report ordering and concrete config/offline/FSM runtime;1292 full plus13 native,2329 matching inputs; concrete gameplay/SevenDay/Main pending,38-roster remains18, no fresh Player');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1292,newChecks=16,native=13,fingerprints=len(fingerprints),indexedMethods=5873,controllers=18,remaining=20)))
