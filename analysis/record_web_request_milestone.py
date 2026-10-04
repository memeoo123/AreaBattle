"""Record the reviewed HTTP helper/agent/queue and native loopback milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);stage=p.parse_args().validation_root.resolve()
root=Path(__file__).resolve().parent.parent;analysis=root/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
    old=p.read_bytes()if p.exists()else b'';s=json.dumps(v,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if b'\r\n'in old else s).encode())
full=read(stage/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1731 and all(r['result']=='pass'for r in full['checks'])
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith(('web-request-agent-','web-request-pool-'))]);assert len(focused['checks'])==27
log=(stage/'analysis/web-request-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1731'in log and 'error CS'not in log
native=read(stage/'analysis/web-request-native-validation.json');assert native['passed']and len(native['checks'])==21
nlog=(stage/'analysis/web-request-native.log').read_text();assert 'AREABATTLE_WEB_REQUEST_NATIVE_PASS checks=21'in nlog and 'error CS'not in nlog
evidence=read(out/'WEB_REQUEST_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==45 and len(evidence['fields'])==32
for r in evidence['methods']+read(out/'web-request-task-pool-generics.json')['methods']:assert hashlib.sha256((out/r['path']).read_bytes()).hexdigest()==r['sha256']
assert len(read(out/'method-map.json'))==6529 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==22
files=['Scripts/OutgameUnityWebRequestAgentHelper.cs','Scripts/OutgameWebRequestTaskPool.cs','Editor/OutgameWebRequestAgentValidation.cs','Editor/OutgameWebRequestTaskPoolValidation.cs','Editor/OutgameWebRequestPlayModeValidation.cs','Editor/BattleBuild.cs']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
    data=(root/name).read_bytes();assert data==(stage/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='http-helper-agent-task-queue-native-loopback';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover original WebRequestManager singleton/helper creation/header/debug counters/object serialization/callback facade; HttpManager/NetTool/AppInfoManager platform domain-key-report composition. Then RankUI/OverUI, UserInfoUI/general sprites and full Main/account/remaining16 controllers/business/final Player and original audiovisual acceptance.'
def diagnostics(s):return {k:s.count(k)for k in ('ShouldRunBehaviour','Curl error 35','Curl error 42','ArgumentOutOfRangeException','error CS')}
record=dict(atUtc=now,scope='Original UnityWebRequest helper, task/agent and generic queue; actual loopback GET/POST/HTTP503/timeout/AES rank/cancellation/priority/reuse/shutdown',checksPassed=1731,newIntegratedChecks=27,targetedChecks=27,nativeChecks=21,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(stage/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=22,remainingLifecycleControllers=16,commands=['BattleBuild.ValidateMechanicsOnly','OutgameWebRequestPlayModeValidation.Run'],logDiagnostics=dict(integrated=diagnostics(log),native=diagnostics(nlog)),diagnosticExplanation='Native UnityEditor.Search.SearchDatabase startup index exception is outside game stack; retained in log. Curl42 follows cancellation of deliberately stalled loopback request. No compilation errors; all product checks pass.',observedBoundaries=evidence['findings'],notClaimed='Original WebRequestManager/HttpManager/NetTool/platform URL-key/report delivery, production Main/RankUI/OverUI/all business/final Player/original audiovisual equivalence. Local server responses are explicit native test evidence, not external platform success.')
focused['limitations']=record['notClaimed'];write(analysis/'web-request-validation.json',focused)
for name in ['unity-integrated-validation.json','web-request-integrated.log','web-request-native-validation.json','web-request-native.log']:shutil.copy2(stage/'analysis'/name,analysis/name)
write(out/'WEB_REQUEST_AUDIT.json',dict(status='helper-agent-task-queue-native-verified-manager-platform-pending',atUtc=now,sourceEvidence='WEB_REQUEST_SOURCE_EVIDENCE.json',verification=record,implementation=files,checks=focused['checks'],native=native,remaining=next_priority))
state.update(status='active',lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,webRequestAudit='generated/outgame/WEB_REQUEST_AUDIT.json',rankTransmitterAudit='generated/outgame/RANK_TRANSMITTER_AUDIT.json',pauseRequestedByUser=False)
state['validation'].update(integratedChecksPassed=1731,webRequest=record);state['milestones'].append(dict(id=milestone,atUtc=now,status='helper-agent-task-queue-native-verified-manager-platform-pending',integratedChecks=1731,newChecks=27,nativeChecks=21,freshPlayModeRun=True,implementedLifecycleControllers=22,remainingLifecycleControllers=16));write(target/'OUTGAME_RESTORE_STATE.json',state)
by={r['path']:r for r in fingerprints};ordered=[by.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by];manifest.update(atUtc=now,passed=True,caseCount=1731,latestValidation=record,goalStatus='active',sourceFingerprints=ordered+[by[n]for n in sorted(by)]);manifest['validationHistory'].append(record)
for name in ['analysis/web-request-validation.json','analysis/web-request-native-validation.json']:
    if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(status='active',updatedAtUtc=now,resumedByUserAt='2026-10-04',pauseRequestedByUser=False,verification=record,nextPriority=next_priority);execution['completedThisRun'].append(f'Original HTTP helper/task/agent and queue restored;1731 integrated including27 new,21 native actual loopback checks with encrypted rank response, priorities/cancel/reuse/shutdown. {len(fingerprints)} matching inputs;6529 indexed methods plus51 generic bodies/94contexts;22/38 lifecycle unchanged. Published41 preserved rank transmitter methods. Manager/platform/Main/UI/full business/final Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
orchestration=read(root/'ORCHESTRATION_STATE.json');orchestration['updatedAtUtc']=now;orchestration['currentWorkstream'].update(status='active',resumeOnlyOnUserRequest=False);write(root/'ORCHESTRATION_STATE.json',orchestration)
print(json.dumps(dict(integrated=1731,newChecks=27,native=21,fingerprints=len(fingerprints),indexedMethods=6529,controllers=22,remaining=16,diagnostics=record['logDiagnostics'])))
