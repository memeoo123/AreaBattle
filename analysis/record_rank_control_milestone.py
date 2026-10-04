"""Record verified full RankControl lifecycle/home/native milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);stage=p.parse_args().validation_root.resolve()
root=Path(__file__).resolve().parent.parent;analysis=root/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 s=json.dumps(v,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if p.exists()and b'\r\n'in p.read_bytes()else s).encode())
full=read(stage/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1688 and all(r['result']=='pass'for r in full['checks'])
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('rank-control-')]);assert len(focused['checks'])==16
log=(stage/'analysis/rank-control-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1688'in log and 'error CS'not in log
native=read(stage/'analysis/rank-control-native-validation.json');assert native['passed']and len(native['checks'])==31 and native['restoredName']=='恢复测试'and native['restoredGold']==37 and native['restoredDiamonds']==5
nlog=(stage/'analysis/rank-control-native.log').read_text();assert 'AREABATTLE_RANK_CONTROL_NATIVE_PASS checks=31'in nlog and 'error CS'not in nlog
assert(stage/'analysis/captures/rank-control-native.png').stat().st_size>10000
evidence=read(out/'RANK_CONTROL_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==26 and len(evidence['fields'])==24
for r in evidence['methods']:assert hashlib.sha256((out/r['path']).read_bytes()).hexdigest()==r['sha256']
assert len(read(out/'method-map.json'))==6443 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==22
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']};files=('Scripts/OutgameRankControl.cs','Scripts/OutgameRankScore.cs','Scripts/OutgameCoreControllerBindings.cs','Editor/OutgameRankControlValidation.cs','Editor/OutgameRankControlPlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
 data=(root/name).read_bytes();assert data==(stage/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='rank-control-full-lifecycle-home-ai-native-restart';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover actual RankUI/OverUI rank rows and original RanklistTransmitter4482 network/callback composition, UserInfoUI/general sprite directory and AppInfoManager country transport. Continue Guide and remaining16 roster controllers, Main/account/platform/all business/final Player/original audiovisual acceptance.'
def diagnostics(s):return {k:s.count(k)for k in ('ShouldRunBehaviour','Curl error 35','Curl error 42','ArgumentOutOfRangeException','error CS')}
record=dict(atUtc=now,scope='Full original RankControl4134 constructor/lifecycle/home100rows/settlement AI generation, actual registry/account/reference pool/native restart',checksPassed=1688,newIntegratedChecks=16,targetedChecks=16,nativeChecks=31,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(stage/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=22,remainingLifecycleControllers=16,commands=['BattleBuild.ValidateMechanicsOnly','OutgameRankControlPlayModeValidation.Run'],logDiagnostics=dict(integrated=diagnostics(log),native=diagnostics(nlog)),observedBoundaries=evidence['findings'],screenshotReviewed=True,notClaimed='Actual RankUI/OverUI rendering, RanklistTransmitter and AppInfoManager transport/Main/platform/all business/fresh Player/original audiovisual equivalence. Native screenshot is original TopInfo;100rank rows are verified runtime data.')
focused['limitations']=record['notClaimed'];write(analysis/'rank-control-validation.json',focused)
for name in ('unity-integrated-validation.json','rank-control-integrated.log','rank-control-native-validation.json','rank-control-native.log'):shutil.copy2(stage/'analysis'/name,analysis/name)
shutil.copy2(stage/'analysis/captures/rank-control-native.png',analysis/'captures/rank-control-native.png')
write(out/'RANK_CONTROL_AUDIT.json',dict(status='rank-control-full-lifecycle-home-data-native-verified-ui-network-pending',atUtc=now,sourceEvidence='RANK_CONTROL_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,rankControlAudit='generated/outgame/RANK_CONTROL_AUDIT.json');state['validation'].update(integratedChecksPassed=1688,rankControl=record);state['milestones'].append(dict(id=milestone,atUtc=now,status='rank-control-full-lifecycle-home-data-native-verified-ui-network-pending',integratedChecks=1688,newChecks=16,nativeChecks=31,freshPlayModeRun=True,implementedLifecycleControllers=22,remainingLifecycleControllers=16));write(target/'OUTGAME_RESTORE_STATE.json',state)
by={r['path']:r for r in fingerprints};ordered=[by.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by];manifest.update(atUtc=now,passed=True,caseCount=1688,latestValidation=record,sourceFingerprints=ordered+[by[n]for n in sorted(by)]);manifest['validationHistory'].append(record)
for name in ('analysis/rank-control-validation.json','analysis/rank-control-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority);execution['completedThisRun'].append(f'RankControl4134 full lifecycle/registry, original100-row home, AI settlement, source config and pooled-row reuse.1688 integrated including16 new,31 native checks;{len(fingerprints)} matching inputs;6443 indexed methods;22/38 roster,16remaining. Native independent account restart rebuilds home rows from actual saved rank/score/UserInfo. RankUI/OverUI rendering/network/platform/Main/all business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1688,newChecks=16,native=31,fingerprints=len(fingerprints),indexedMethods=6443,controllers=22,remaining=16,diagnostics=record['logDiagnostics'])))
