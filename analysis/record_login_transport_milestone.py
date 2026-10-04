"""Record the verified domain/key-envelope/login upload checkpoint once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);stage=p.parse_args().validation_root.resolve()
root=Path(__file__).resolve().parent.parent;analysis=root/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,value):
    old=p.read_bytes()if p.exists()else b'';s=json.dumps(value,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if b'\r\n'in old else s).encode())
full=read(stage/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1772 and all(r['result']=='pass'for r in full['checks'])
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('login-transport-')]);assert len(focused['checks'])==23
log=(stage/'analysis/login-transport-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1772'in log and 'error CS'not in log
native=read(stage/'analysis/login-transport-native-validation.json');assert native['passed']and len(native['checks'])==8
nlog=(stage/'analysis/login-transport-native.log').read_text();assert 'AREABATTLE_LOGIN_TRANSPORT_NATIVE_PASS checks=8'in nlog and 'error CS'not in nlog
evidence=read(out/'LOGIN_TRANSPORT_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==23 and len(evidence['fields'])==14
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6606 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==22
files=['Scripts/OutgameNetTool.cs','Scripts/OutgameLoginTransmitter.cs','Scripts/OutgameRankTransmitter.cs','Editor/OutgameLoginTransportValidation.cs','Editor/OutgameLoginTransportPlayModeValidation.cs','Editor/OutgameWebRequestPlayModeValidation.cs','Editor/BattleBuild.cs']
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
    data=(root/name).read_bytes();assert data==(stage/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2883
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='login-transmitter-domain-key-envelope-native';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover HttpManager owner/global-key mutation, HttpNetAcion login/server-time/business responses and ServerTimeSync; install actual platform AppInfoManager/domain/report callbacks, then RankUI/OverUI/Main/account/remaining16 controllers/full business/final Player/original audiovisual acceptance.'
def diagnostics(s):return {k:s.count(k)for k in ('ShouldRunBehaviour','Curl error 35','Curl error 42','ArgumentOutOfRangeException','error CS')}
record=dict(atUtc=now,scope='NetTool domain configuration/cache, server-time key envelope decoder and LoginTransmitter protocol/DTO/merged upload lifecycle; actual Unity HTTP encrypted upload and plaintext server-time GET',checksPassed=1772,newIntegratedChecks=23,targetedChecks=23,nativeChecks=8,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(stage/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=22,remainingLifecycleControllers=16,commands=['BattleBuild.ValidateMechanicsOnly','OutgameLoginTransportPlayModeValidation.Run'],logDiagnostics=dict(integrated=diagnostics(log),native=diagnostics(nlog)),diagnosticExplanation='Any UnityEditor.Search startup index exception remains in native log and is outside game stack. Initial native fixture incorrectly expected null-object POST; corrected against existing source manager null->GET and retained failed evidence. Runtime implementation unchanged for that correction.',observedBoundaries=evidence['findings'],notClaimed='Production HttpManager owner/global-key installation, HttpNetAcion business callbacks, ServerTimeSync, real platform SDK domain/login/report delivery, Main/RankUI/OverUI/full business/final Player/original audiovisual equivalence. Native fixture supplies login frame ticks and local server responses explicitly.')
focused['limitations']=record['notClaimed'];write(analysis/'login-transport-validation.json',focused)
for name in ['unity-integrated-validation.json','login-transport-integrated.log','login-transport-native-validation.json','login-transport-native.log']:shutil.copy2(stage/'analysis'/name,analysis/name)
write(out/'LOGIN_TRANSPORT_AUDIT.json',dict(status='login-transport-native-verified-owner-business-platform-pending',atUtc=now,sourceEvidence='LOGIN_TRANSPORT_SOURCE_EVIDENCE.json',verification=record,implementation=files,checks=focused['checks'],native=native,remaining=next_priority))
state.update(status='active',lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,loginTransportAudit='generated/outgame/LOGIN_TRANSPORT_AUDIT.json',pauseRequestedByUser=False)
state['validation'].update(integratedChecksPassed=1772,loginTransport=record);state['milestones'].append(dict(id=milestone,atUtc=now,status='login-transport-native-verified-owner-business-platform-pending',integratedChecks=1772,newChecks=23,nativeChecks=8,freshPlayModeRun=True,implementedLifecycleControllers=22,remainingLifecycleControllers=16));write(target/'OUTGAME_RESTORE_STATE.json',state)
by={r['path']:r for r in fingerprints};ordered=[by.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by];manifest.update(atUtc=now,passed=True,caseCount=1772,latestValidation=record,goalStatus='active',sourceFingerprints=ordered+[by[n]for n in sorted(by)]);manifest['validationHistory'].append(record)
for name in ['analysis/login-transport-validation.json','analysis/login-transport-native-validation.json']:
    if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(status='active',updatedAtUtc=now,resumedByUserAt='2026-10-04',pauseRequestedByUser=False,verification=record,nextPriority=next_priority);execution['completedThisRun'].append(f'NetTool domain cache, key decoder and LoginTransmitter restored;1772 integrated including23 new,8 native actual HTTP checks.2883 matching inputs;6606 indexed methods;22/38 lifecycle unchanged. Actual encrypted merged upload and plaintext server-time GET verified via WebRequestManager. HttpManager/HttpNetAcion/ServerTimeSync/platform/Main/business/final Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
orchestration=read(root/'ORCHESTRATION_STATE.json');orchestration['updatedAtUtc']=now;orchestration['currentWorkstream'].update(status='active',resumeOnlyOnUserRequest=False);write(root/'ORCHESTRATION_STATE.json',orchestration)
print(json.dumps(dict(integrated=1772,newChecks=23,native=8,fingerprints=len(fingerprints),indexedMethods=6606,controllers=22,remaining=16,diagnostics=record['logDiagnostics'])))
