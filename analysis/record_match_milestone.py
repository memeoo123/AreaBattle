"""Record verified Match/account/native-restart milestone once; never replay after success."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);validation=p.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 s=json.dumps(v,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if p.exists()and b'\r\n'in p.read_bytes()else s).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1629 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/match-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1629'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('match-')]);assert len(focused['checks'])==18
native=read(validation/'analysis/match-native-validation.json');assert native['passed']and len(native['checks'])==20 and native['restoredGold']==37 and native['restoredDiamonds']==5 and native['restoredName']=='恢复测试'
nlog=(validation/'analysis/match-native.log').read_text();assert 'AREABATTLE_MATCH_NATIVE_PASS checks=20'in nlog and 'error CS'not in nlog
assert (validation/'analysis/captures/match-native.png').stat().st_size>10000
evidence=read(out/'MATCH_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==29 and len(evidence['fields'])==23
for r in evidence['methods']:assert hashlib.sha256((out/r['path']).read_bytes()).hexdigest()==r['sha256']
assert len(read(out/'method-map.json'))==6407 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==21
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameMatchRuntime.cs','Scripts/GameRandomSource.cs','Editor/OutgameMatchValidation.cs','Editor/OutgameMatchPlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
third=workspace/'UnityProject/Assets/AreaBattle/ThirdParty/LitJson'
paths.update(str(p.relative_to(workspace))for p in third.rglob('*')if p.is_file());paths.add(str(third.with_suffix('.meta').relative_to(workspace)))
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='match-storage-control-top-info-native-restart';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover WXAvatar actual default icon/cache/download and UserDataPrefs authorization composition, RankControl score/full UserInfoUI and real RanklistTransmitter. Complete UI/general sprite/resource assembly, Guide and remaining17 controller lifecycle/business, real Main/account/platform/all business and final Player/original audiovisual acceptance.'
def diagnostics(s):return {k:s.count(k)for k in ('ShouldRunBehaviour','Curl error 35','Curl error 42','ArgumentOutOfRangeException','error CS')}
record=dict(atUtc=now,scope='Original MatchManager/MatchControl and real TopInfo account/shared-pool/file persistence, original rank request/callback ordering',checksPassed=1629,newIntegratedChecks=18,targetedChecks=18,nativeChecks=20,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=21,remainingLifecycleControllers=17,commands=['BattleBuild.ValidateMechanicsOnly','OutgameMatchPlayModeValidation.Run'],logDiagnostics=dict(integrated=diagnostics(log),native=diagnostics(nlog)),restoredAccount=dict(gold=37,diamonds=5,name='恢复测试',matchNickname='platform-nickname',matchAvatar='platform-avatar',matchAuthorization=True,dailyWins=1,weeklyWins=1,totalWins=6,dailyLosses=1,weeklyLosses=1,totalLosses=1,played=2),observedBoundaries=evidence['findings'],notClaimed='Actual WXAvatar/UserDataPrefs platform composition, rank network/server responses, full Rank/UserInfoUI/general sprite/Main/platform/remaining17 roster/all business/fresh Player/original audiovisual equivalence; original bundled LitJSON commit unknown, full parser equivalence unclaimed.')
focused['limitations']=native['scope'];write(analysis/'match-validation.json',focused)
for name in ('unity-integrated-validation.json','match-integrated.log','match-native-validation.json','match-native.log','match-initial-match-integrated.log','match-initial-unity-integrated-validation.json'):shutil.copy2(validation/'analysis'/name,analysis/name)
shutil.copy2(validation/'analysis/captures/match-native.png',analysis/'captures/match-native.png')
write(out/'MATCH_AUDIT.json',dict(status='match-account-runtime-verified-external-avatar-rank-ports-pending',atUtc=now,sourceEvidence='MATCH_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,matchAudit='generated/outgame/MATCH_AUDIT.json');state['validation'].update(integratedChecksPassed=1629,match=record);state['milestones'].append(dict(id=milestone,atUtc=now,status='match-account-runtime-verified-external-avatar-rank-ports-pending',integratedChecks=1629,newChecks=18,nativeChecks=20,freshPlayModeRun=True,remainingLifecycleControllers=17));write(target/'OUTGAME_RESTORE_STATE.json',state)
by={r['path']:r for r in fingerprints};ordered=[by.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by];manifest.update(atUtc=now,passed=True,caseCount=1629,latestValidation=record,sourceFingerprints=ordered+[by[n]for n in sorted(by)]);manifest['validationHistory'].append(record)
for name in ('analysis/match-validation.json','analysis/match-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority);execution['completedThisRun'].append(f'MatchManager4130/MatchControl4129 and real TopInfo/shared account persistence restored;1629 integrated including18 new,20 native checks with actual Match file restart, original three-rank request/response/record replacement ordering. {len(fingerprints)} matching inputs;6407 indexed methods,21/38 roster unchanged. External avatar/rank/platform/Main/all business/Player still pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1629,newChecks=18,native=20,fingerprints=len(fingerprints),indexedMethods=6407,controllers=21,remaining=17,diagnostics=record['logDiagnostics'])))
