"""Record verified account-rewards/shared-subview lifecycle milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1531 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/account-rewards-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1531'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('account-rewards-')]);assert len(focused['checks'])==10
focused['limitations']='Actual account ItemManager/LocalDataManager/ToolControl and Task/Achievement engines share pool/storage/config and statistics.10 new integrated and13 native checks; original daily50/achievement200 restore250 in both currency records with both claims and liveness. Package branches and login/save/report boundaries verified. Explicit login/platform/presentation/report/skin endpoints, full production Main/remaining controllers/Player acceptance pending.'
native=read(validation/'analysis/account-rewards-native-validation.json');assert native['passed']and len(native['checks'])==13
native_log=(validation/'analysis/account-rewards-native.log').read_text();assert 'error CS'not in native_log
evidence=read(out/'ACCOUNT_REWARDS_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['dependencies']),len(evidence['registrations']))==(19,6,2)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6332 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==20
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameAccountItemRuntime.cs','Scripts/OutgameActivityRewardBinding.cs','Scripts/OutgameItemRuntime.cs','Editor/OutgameAccountRewardsValidation.cs','Editor/OutgameAccountRewardsPlayModeValidation.cs','Editor/OutgameTaskActivityValidation.cs','Editor/OutgameTaskRowsValidation.cs','Editor/OutgameTaskControlViewValidation.cs','Editor/OutgameTaskPageValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='account-item-activity-reward-persistence';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover original EffectControl4058 and effect-module resource/handle ownership required by task effect1016 and shared Main/battle effects; connect actual visual/audio/report/skin/platform hosts. Continue full production Main/account/login/data-pool startup composition, remaining18 controller lifecycles and all business, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Actual account item/local economy and activity reward composition, original page claims, source save/failure ordering and independent account restart',checksPassed=1531,newIntegratedChecks=10,targetedChecks=10,nativeChecks=13,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=20,remainingLifecycleControllers=18,commands=['BattleBuild.ValidateMechanicsOnly','OutgameAccountRewardsPlayModeValidation.Run'],
 observedBoundaries=evidence['findings']+['Final integrated retains32 existing ShouldRunBehaviour assertions and one Curl35 certificate failure. Native retains one UnityEditor.Search startup ArgumentOutOfRangeException and one Curl42; product checks and compilation pass. Initial account integration fixture errors (edit-mode static scheduler, late item binding, daily versus achievement order assumptions) are preserved in initial diagnostics, corrected through explicit shared scheduler and before-init composition.'],notClaimed='Full production Main/login/account/platform, complete effect/audio/skin/report hosts, remaining18 controllers, original audiovisual equivalence or fresh Player.')
write(analysis/'account-rewards-validation.json',focused)
for name in ('unity-integrated-validation.json','account-rewards-integrated.log','account-rewards-native-validation.json','account-rewards-native.log','account-rewards-initial-validation.json','account-rewards-initial-integrated.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'ACCOUNT_REWARDS_AUDIT.json',dict(status='account-item-activity-rewards-verified-production-main-pending',atUtc=now,sourceEvidence='ACCOUNT_REWARDS_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],nativeChecks=native['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,accountRewardsAudit='generated/outgame/ACCOUNT_REWARDS_AUDIT.json');state['validation'].update(integratedChecksPassed=1531,accountRewards=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='account-item-activity-rewards-verified-production-main-pending',integratedChecks=1531,newChecks=10,nativeChecks=13,freshPlayModeRun=True,remainingLifecycleControllers=18));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1531,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for name in ('analysis/account-rewards-validation.json','analysis/account-rewards-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Actual account LocalDataManager/ItemManager/ToolControl and Task/Achievement reward composition restored.1531 integrated including10 new,13 native page/reward/restart checks; independent restart restores250 gold in both records with daily/achievement claims and liveness. Original package low/high branches, gates and report failure verified. {len(fingerprints)} matching inputs;6332 normal methods and controller20/38 unchanged. Full Main/login/platform/presentation/effect/skin/report/all business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1531,newChecks=10,native=13,fingerprints=len(fingerprints),indexedMethods=6332,controllers=20,remaining=18)))
