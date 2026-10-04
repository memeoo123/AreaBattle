"""Record verified rank-list prerequisites once; full RankControl remains pending."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);stage=p.parse_args().validation_root.resolve()
root=Path(__file__).resolve().parent.parent;analysis=root/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 s=json.dumps(v,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if p.exists()and b'\r\n'in p.read_bytes()else s).encode())
full=read(stage/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1672 and all(r['result']=='pass'for r in full['checks'])
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('rank-list-support-')]);assert len(focused['checks'])==10
log=(stage/'analysis/rank-list-support-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1672'in log and 'error CS'not in log
evidence=read(out/'RANK_LIST_SUPPORT_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==11 and len(evidence['sharedGenerics'])==5 and len(evidence['fields'])==16
for r in evidence['methods']+evidence['sharedGenerics']:assert hashlib.sha256((out/r['path']).read_bytes()).hexdigest()==r['sha256']
assert len(read(out/'method-map.json'))==6443 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==21
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']};files=('Scripts/OutgameRankListSupport.cs','Editor/OutgameRankListSupportValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
 data=(root/name).read_bytes();assert data==(stage/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2849
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='rank-list-reference-pool-weight-country-prerequisites';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Use verified rank-list support to complete RankControl home/AI list, constructor and full lifecycle with real country/row pool; bind registry only after complete. Then RanklistTransmitter/UserInfoUI/Main/account/platform/remaining17 roster/all business/final Player/original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original RankItemData/ReferencePool4142, weighted selection26201/26203/26206 and country helper32594/32591 prerequisites',checksPassed=1672,newIntegratedChecks=10,targetedChecks=10,nativeChecks=0,freshPlayModeRun=False,freshPlayerBuild=False,freshPlayerSmoke=False,priorNativeChecks=25,priorNativeReport='analysis/rank-score-native-validation.json',editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(stage/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=21,remainingLifecycleControllers=17,commands=['BattleBuild.ValidateMechanicsOnly'],logDiagnostics={k:log.count(k)for k in ('ShouldRunBehaviour','Curl error 35','Curl error 42','ArgumentOutOfRangeException','error CS')},observedBoundaries=evidence['findings'],notClaimed='No new native run or Player. Full RankControl lifecycle/home/AI integration and AppInfoManager actual country transport/Main/platform/all business still pending.')
focused['limitations']=record['notClaimed'];write(analysis/'rank-list-support-validation.json',focused)
for name in ('unity-integrated-validation.json','rank-list-support-integrated.log'):shutil.copy2(stage/'analysis'/name,analysis/name)
write(out/'RANK_LIST_SUPPORT_AUDIT.json',dict(status='rank-list-prerequisites-verified-controller-integration-pending',atUtc=now,sourceEvidence='RANK_LIST_SUPPORT_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],remaining=next_priority))
evidence['status']='rank-list-prerequisites-verified-controller-integration-pending';write(out/'RANK_LIST_SUPPORT_SOURCE_EVIDENCE.json',evidence)
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,rankListSupportAudit='generated/outgame/RANK_LIST_SUPPORT_AUDIT.json');state['validation'].update(integratedChecksPassed=1672,rankListSupport=record);state['milestones'].append(dict(id=milestone,atUtc=now,status='rank-list-prerequisites-verified-controller-integration-pending',integratedChecks=1672,newChecks=10,nativeChecks=0,freshPlayModeRun=False,remainingLifecycleControllers=17));write(target/'OUTGAME_RESTORE_STATE.json',state)
by={r['path']:r for r in fingerprints};ordered=[by.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by];manifest.update(atUtc=now,passed=True,caseCount=1672,latestValidation=record,sourceFingerprints=ordered+[by[n]for n in sorted(by)]);manifest['validationHistory'].append(record)
if 'analysis/rank-list-support-validation.json'not in manifest['reports']:manifest['reports'].append('analysis/rank-list-support-validation.json')
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority);execution['completedThisRun'].append('Rank list prerequisites: actual FIFO reference pool4142/row clearing, weighted subset alias/priority/cumulative random, asynchronous country reset/callback ordering.1672 integrated including10 new;no new native run(prior rank native25 remains);2849 matching inputs,6443 indexed methods,21/38 roster unchanged. Full Rank home/lifecycle/network/Main/platform/all business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1672,newChecks=10,fingerprints=len(fingerprints),indexedMethods=6443,controllers=21,remaining=17,diagnostics=record['logDiagnostics'])))
