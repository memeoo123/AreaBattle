"""Record the reviewed HTTP manager/native singleton and actual rank IO milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);stage=p.parse_args().validation_root.resolve()
root=Path(__file__).resolve().parent.parent;analysis=root/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
    old=p.read_bytes()if p.exists()else b'';s=json.dumps(v,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if b'\r\n'in old else s).encode())
full=read(stage/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1749 and all(r['result']=='pass'for r in full['checks'])
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('web-request-manager-')]);assert len(focused['checks'])==18
log=(stage/'analysis/web-request-manager-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1749'in log and 'error CS'not in log
native=read(stage/'analysis/web-request-manager-native-validation.json');assert native['passed']and len(native['checks'])==17
nlog=(stage/'analysis/web-request-manager-native.log').read_text();assert 'AREABATTLE_WEB_REQUEST_MANAGER_NATIVE_PASS checks=17'in nlog and 'error CS'not in nlog
evidence=read(out/'WEB_REQUEST_MANAGER_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==58 and len(evidence['fields'])==13
for r in evidence['methods']:assert hashlib.sha256((out/r['path']).read_bytes()).hexdigest()==r['sha256']
assert len(read(out/'method-map.json'))==6586 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==22
files=['Scripts/OutgameWebRequestManagerHost.cs','Scripts/OutgameRankTransmitter.cs','Scripts/OutgameWebRequestTaskPool.cs','Editor/OutgameWebRequestManagerValidation.cs','Editor/OutgameWebRequestManagerPlayModeValidation.cs','Editor/OutgameWebRequestPlayModeValidation.cs','Editor/BattleBuild.cs']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
    data=(root/name).read_bytes();assert data==(stage/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='http-manager-native-singleton-rank-transport';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover HttpManager/NetTool/AppInfoManager platform domain-key-login-server-time-report composition, then RankUI/OverUI, UserInfoUI/general sprites and full Main/account/remaining16 controllers/business/final Player and original audiovisual acceptance.'
def diagnostics(s):return {k:s.count(k)for k in ('ShouldRunBehaviour','Curl error 35','Curl error 42','ArgumentOutOfRangeException','error CS')}
record=dict(atUtc=now,scope='Original WebRequestManager owner, overload serialization/headers/debug counters/events and native singleton; actual AES rank HTTP, priority/cancel/KeWan/timeout/shutdown/recreation',checksPassed=1749,newIntegratedChecks=18,targetedChecks=18,nativeChecks=17,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(stage/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=22,remainingLifecycleControllers=16,commands=['BattleBuild.ValidateMechanicsOnly','OutgameWebRequestManagerPlayModeValidation.Run'],logDiagnostics=dict(integrated=diagnostics(log),native=diagnostics(nlog)),diagnosticExplanation='Native UnityEditor.Search.SearchDatabase startup index exception is outside game stack; retained in log. Curl42 follows cancellation of deliberately stalled loopback request. No compilation errors; all product checks pass.',observedBoundaries=evidence['findings'],notClaimed='HttpManager/NetTool/platform URL-key/login/server-time/report delivery, production Main/RankUI/OverUI/all business/final Player/original audiovisual equivalence. Local server responses are explicit native test evidence, not external platform success.')
focused['limitations']=record['notClaimed'];write(analysis/'web-request-manager-validation.json',focused)
for name in ['unity-integrated-validation.json','web-request-manager-integrated.log','web-request-manager-native-validation.json','web-request-manager-native.log']:shutil.copy2(stage/'analysis'/name,analysis/name)
write(out/'WEB_REQUEST_MANAGER_AUDIT.json',dict(status='http-manager-owner-native-verified-platform-pending',atUtc=now,sourceEvidence='WEB_REQUEST_MANAGER_SOURCE_EVIDENCE.json',verification=record,implementation=files,checks=focused['checks'],native=native,remaining=next_priority))
state.update(status='active',lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,webRequestManagerAudit='generated/outgame/WEB_REQUEST_MANAGER_AUDIT.json',rankTransmitterAudit='generated/outgame/RANK_TRANSMITTER_AUDIT.json',pauseRequestedByUser=False)
state['validation'].update(integratedChecksPassed=1749,webRequestManager=record);state['milestones'].append(dict(id=milestone,atUtc=now,status='http-manager-owner-native-verified-platform-pending',integratedChecks=1749,newChecks=18,nativeChecks=17,freshPlayModeRun=True,implementedLifecycleControllers=22,remainingLifecycleControllers=16));write(target/'OUTGAME_RESTORE_STATE.json',state)
by={r['path']:r for r in fingerprints};ordered=[by.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by];manifest.update(atUtc=now,passed=True,caseCount=1749,latestValidation=record,goalStatus='active',sourceFingerprints=ordered+[by[n]for n in sorted(by)]);manifest['validationHistory'].append(record)
for name in ['analysis/web-request-manager-validation.json','analysis/web-request-manager-native-validation.json']:
    if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(status='active',updatedAtUtc=now,resumedByUserAt='2026-10-04',pauseRequestedByUser=False,verification=record,nextPriority=next_priority);execution['completedThisRun'].append(f'Original WebRequestManager owner/overloads/header/debug counting/callback events/native singleton restored;1749 integrated including18 new,17 native actual loopback manager checks. {len(fingerprints)} matching inputs;6586 indexed methods;22/38 lifecycle unchanged. Real rank transport uses manager serialization/queue/HTTP; native KeWan gate, timeout, capacity orphan and independent recreation verified. HttpManager/NetTool/platform/Main/UI/full business/final Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
orchestration=read(root/'ORCHESTRATION_STATE.json');orchestration['updatedAtUtc']=now;orchestration['currentWorkstream'].update(status='active',resumeOnlyOnUserRequest=False);write(root/'ORCHESTRATION_STATE.json',orchestration)
print(json.dumps(dict(integrated=1749,newChecks=18,native=17,fingerprints=len(fingerprints),indexedMethods=6586,controllers=22,remaining=16,diagnostics=record['logDiagnostics'])))
