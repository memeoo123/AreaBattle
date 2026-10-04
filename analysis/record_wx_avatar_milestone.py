"""Record verified WXAvatar/account/native-restart milestone once; never replay after success."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);validation=p.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 s=json.dumps(v,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if p.exists()and b'\r\n'in p.read_bytes()else s).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1645 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/wx-avatar-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1645'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('wx-avatar-')]);assert len(focused['checks'])==16
native=read(validation/'analysis/wx-avatar-native-validation.json');assert native['passed']and len(native['checks'])==12 and native['restoredNickname']=='头像恢复验证' and native['restoredAuthorization'] and native['restoredPreference']==47
nlog=(validation/'analysis/wx-avatar-native.log').read_text();assert 'AREABATTLE_WX_AVATAR_NATIVE_PASS checks=12'in nlog and 'error CS'not in nlog
assert (validation/'analysis/captures/wx-avatar-native.png').stat().st_size>10000
evidence=read(out/'WX_AVATAR_SOURCE_EVIDENCE.json');assert len(evidence['methods'])==20 and len(evidence['fields'])==14
for r in evidence['methods']:assert hashlib.sha256((out/r['path']).read_bytes()).hexdigest()==r['sha256']
assert len(read(out/'method-map.json'))==6427 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==21
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameWxAvatar.cs','Editor/OutgameAvatarAssetImporter.cs','Editor/OutgameWxAvatarValidation.cs','Editor/OutgameWxAvatarPlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
assets=workspace/'UnityProject/Assets/AreaBattle/Resources/EnterGameUI'
paths.update(str(p.relative_to(workspace))for p in assets.rglob('*')if p.is_file());paths.add(str(assets.with_suffix('.meta').relative_to(workspace)))

fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='wx-avatar-download-default-resource-userdata-native-restart';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover real RankControl score/RanklistTransmitter and full UserInfoUI/general sprite resource destination. Complete WX platform authorization/preference backend and Main coroutine/ownership assembly, Guide and remaining17 roster controller lifecycle/business, real Main/account/platform/all business and final Player/original audiovisual acceptance.'

def diagnostics(s):return {k:s.count(k)for k in ('ShouldRunBehaviour','Curl error 35','Curl error 42','ArgumentOutOfRangeException','error CS')}
record=dict(atUtc=now,scope='Original WXAvatar/default resource/cache/native UnityWebRequest and real TopInfo/UserDataPrefs/Match/account file restart',checksPassed=1645,newIntegratedChecks=16,targetedChecks=16,nativeChecks=12,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=21,remainingLifecycleControllers=17,commands=['BattleBuild.ValidateMechanicsOnly','OutgameWxAvatarPlayModeValidation.Run'],logDiagnostics=dict(integrated=diagnostics(log),native=diagnostics(nlog)),restoredAccount=dict(matchNickname=native['restoredNickname'],matchAvatar=native['restoredAvatar'],matchAuthorization=native['restoredAuthorization'],userDataPreference=native['restoredPreference']),observedBoundaries=evidence['findings'],notClaimed='Actual remote avatar/WeChat authorization, platform preference SDK installation/Main coroutine global composition, full Rank/UserInfoUI/general sprite/Main/platform/remaining17 roster/all business/fresh Player/original audiovisual equivalence; local-file native transport is explicit fixture.')
focused['limitations']=native['scope'];write(analysis/'wx-avatar-validation.json',focused)
for name in ('unity-integrated-validation.json','wx-avatar-integrated.log','wx-avatar-native-validation.json','wx-avatar-native.log','avatar-import.log','avatar-import-initial.log','avatar-import-shape-fixed.log','avatar-import-geometry-diagnostic.log','avatar-import-physics-diagnostic.log','wx-avatar-render-initial-wx-avatar-native.log','wx-avatar-render-initial-wx-avatar-native-validation.json','wx-avatar-layout-initial-wx-avatar-native.log','wx-avatar-layout-initial-wx-avatar-native-validation.json','wx-avatar-overlay-initial-wx-avatar-native.log','wx-avatar-overlay-initial-wx-avatar-native-validation.json'):shutil.copy2(validation/'analysis'/name,analysis/name)
shutil.copy2(validation/'analysis/captures/wx-avatar-native.png',analysis/'captures/wx-avatar-native.png')
write(out/'WX_AVATAR_AUDIT.json',dict(status='wx-avatar-native-download-account-storage-verified-main-platform-pending',atUtc=now,sourceEvidence='WX_AVATAR_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,wxAvatarAudit='generated/outgame/WX_AVATAR_AUDIT.json');state['validation'].update(integratedChecksPassed=1645,wxAvatar=record);state['milestones'].append(dict(id=milestone,atUtc=now,status='wx-avatar-native-download-account-storage-verified-main-platform-pending',integratedChecks=1645,newChecks=16,nativeChecks=12,freshPlayModeRun=True,remainingLifecycleControllers=17));write(target/'OUTGAME_RESTORE_STATE.json',state)
by={r['path']:r for r in fingerprints};ordered=[by.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by];manifest.update(atUtc=now,passed=True,caseCount=1645,latestValidation=record,sourceFingerprints=ordered+[by[n]for n in sorted(by)]);manifest['validationHistory'].append(record)
for name in ('analysis/wx-avatar-validation.json','analysis/wx-avatar-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority);execution['completedThisRun'].append(f'WXAvatar9188 native default128x128/PPU80/7vertices/15indices/25physics restored, cache/download/dedup/callback and Utils_PlayerPrefs string wrapper connected to TopInfo with real UserDataPrefs+Match+shared account file restart;1645 integrated including16 new,12 native checks. {len(fingerprints)} matching inputs;6427 indexed methods,21/38 roster unchanged. Platform/Main/Rank/UserInfoUI/all business/Player still pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1645,newChecks=16,native=12,fingerprints=len(fingerprints),indexedMethods=6427,controllers=21,remaining=17,diagnostics=record['logDiagnostics'])))
