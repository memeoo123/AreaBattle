"""Record verified original rank score/account restart milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);validation=p.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 s=json.dumps(v,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if p.exists()and b'\r\n'in p.read_bytes()else s).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1662 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/rank-score-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1662'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('rank-score-')]);assert len(focused['checks'])==17
native=read(validation/'analysis/rank-score-native-validation.json');assert native['passed']and len(native['checks'])==25 and native['restoredName']=='恢复测试'and native['restoredGold']==37 and native['restoredDiamonds']==5
nlog=(validation/'analysis/rank-score-native.log').read_text();assert 'AREABATTLE_RANK_SCORE_NATIVE_PASS checks=25'in nlog and 'error CS'not in nlog
assert(validation/'analysis/captures/rank-score-native.png').stat().st_size>10000
evidence=read(out/'RANK_SCORE_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==23 and len(evidence['fields'])==28
for r in evidence['methods']:assert hashlib.sha256((out/r['path']).read_bytes()).hexdigest()==r['sha256']
assert len(read(out/'method-map.json'))==6435 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==21
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameRankScore.cs','Editor/OutgameRankScoreValidation.cs','Editor/OutgameRankScorePlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
paths.update(('analysis/run_original_rank_float_oracle.js',str((out/'rank-float-oracle.json').relative_to(workspace))))
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2845
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='rank-score-original-float-oracle-account-native-restart';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Complete RankControl home/AI lists, reference pool rows and full lifecycle, then real RanklistTransmitter and UserInfoUI/general sprite destination. Complete Main/account/platform/all remaining17 roster controllers and business, final Player/original audiovisual acceptance.'
def diagnostics(s):return {k:s.count(k)for k in ('ShouldRunBehaviour','Curl error 35','Curl error 42','ArgumentOutOfRangeException','error CS')}
record=dict(atUtc=now,scope='Original RankManager/RankControl score methods, WASM float oracle and real TopInfo/account file restart',checksPassed=1662,newIntegratedChecks=17,targetedChecks=17,nativeChecks=25,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=21,remainingLifecycleControllers=17,commands=['BattleBuild.ValidateMechanicsOnly','OutgameRankScorePlayModeValidation.Run','node analysis/run_original_rank_float_oracle.js'],logDiagnostics=dict(integrated=diagnostics(log),native=diagnostics(nlog)),observedBoundaries=evidence['findings'],notClaimed='Full RankControl lifecycle/home/AI list/RanklistTransmitter; actual remote server/platform/Main/all business/fresh Player/original audiovisual equivalence.')
focused['limitations']=native['scope'];write(analysis/'rank-score-validation.json',focused)
for name in ('unity-integrated-validation.json','rank-score-integrated.log','rank-score-native-validation.json','rank-score-native.log','rank-score-float-initial-rank-score-native.log','rank-score-float-initial-rank-score-native-validation.json'):shutil.copy2(validation/'analysis'/name,analysis/name)
shutil.copy2(validation/'analysis/captures/rank-score-native.png',analysis/'captures/rank-score-native.png')
write(out/'RANK_SCORE_AUDIT.json',dict(status='rank-score-native-restart-verified-full-controller-pending',atUtc=now,sourceEvidence='RANK_SCORE_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,rankScoreAudit='generated/outgame/RANK_SCORE_AUDIT.json');state['validation'].update(integratedChecksPassed=1662,rankScore=record);state['milestones'].append(dict(id=milestone,atUtc=now,status='rank-score-native-restart-verified-full-controller-pending',integratedChecks=1662,newChecks=17,nativeChecks=25,freshPlayModeRun=True,remainingLifecycleControllers=17));write(target/'OUTGAME_RESTORE_STATE.json',state)
by={r['path']:r for r in fingerprints};ordered=[by.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by];manifest.update(atUtc=now,passed=True,caseCount=1662,latestValidation=record,sourceFingerprints=ordered+[by[n]for n in sorted(by)]);manifest['validationHistory'].append(record)
for name in ('analysis/rank-score-validation.json','analysis/rank-score-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority);execution['completedThisRun'].append('RankManager4137 and RankControl score/offline rank methods restored with original config and real TopInfo/account storage. Original WASM float oracle7 cases proves1.28K saves639999 and restart displays6.39K. 1662 integrated including17 new,25 native checks;2845 matching inputs;6435 indexed methods;21/38 roster unchanged. Full Rank home/lifecycle/network/Main/platform/all business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1662,newChecks=17,native=25,fingerprints=len(fingerprints),indexedMethods=6435,controllers=21,remaining=17,diagnostics=record['logDiagnostics'])))
