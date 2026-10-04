"""Record original main seven-day entry/red and native entry-to-page flow/restart."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1419 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/sevenday-entry-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1419'in log and 'error CS'not in log
native=read(validation/'analysis/sevenday-entry-native-validation.json');assert native['passed']and len(native['checks'])==14
native_log=(validation/'analysis/sevenday-entry-native.log').read_text();assert 'error CS'not in native_log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('sevenday-entry-')]);assert len(focused['checks'])==9
focused['limitations']=native['scope']
evidence=read(out/'SEVENDAY_ENTRY_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(7,5,10)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6044 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameSevendayEntryBinding.cs','Editor/OutgameSevendayEntryValidation.cs','Editor/OutgameSevendayEntryPlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:
 paths.add('UnityProject/Assets/AreaBattle/'+name);paths.add('UnityProject/Assets/AreaBattle/'+name+'.meta')
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
 fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='sevenday-main-entry-red-and-native-restart';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore ordinary Task/Achievement manager/activity/models/UI, complete Proj_xqzdStartUI and production Main/account/resource/audio/effect/report/SDK/HTTP/scene hosts, remaining19 controllers/all business/rewards/return, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original main seven-day entry/red connected to actual control and complete page registry, native entry-to-claim-to-close/save/restart',checksPassed=1419,newIntegratedChecks=9,targetedChecks=9,nativeChecks=14,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly','OutgameSevendayEntryPlayModeValidation.Run'],
 observedBoundaries=[
  'Proj_xqzdStartUI4399 four complete entry methods33722/33704/33695/33690 restored with corresponding Awake33711/Dispose33692 slices and original Button extension. Evidence7 methods,5 fields,10 usages; ordinary method index6044 unchanged. Full main-page lifecycle is not claimed by this binding.',
  'Original RigthBar/btn_sevenDay and imgReddot connected to actual SevenDay control and typed complete page registry. Initialize captures unlock, conditionally appends button and four message callbacks, refreshes red then applies captured visibility. Locked initialization installs nothing; later unlock message does not invent registration. Click invokes required voice(1,2001), opens page with empty args, then emits button message; failures retain prefix.',
  'Three red messages use actual task/accumulator eligibility; end message only updates entry visibility. Red callback captures managed object before current control query and uses managed null guard, preserving destroyed-reference failure. Dispose removes four callbacks only if currently unlocked and retains button handler; repeated initialization and single-occurrence removal preserved.',
  'Integrated1419 including9 new passes. Native14 verifies original main hierarchy and entry pointer to actual page load, duplicate-open reuse, real task completion red, accumulator red delayed until observed currency completion, automatic activity save, native popup close/resource release, independent main-entry/page reconstruction from disk and end message hiding entry while popup remains open. Claimed prerequisites seeded; global inventory persistence and real sound/animation output not inferred.',
  'Final logs contain zero compiler errors. Integrated retains32 existing ShouldRunBehaviour assertions and one Curl42; native one UnityEditor.Search startup indexing ArgumentOutOfRangeException and one Curl42. Native-only runner added after integrated pass then compiled/passed; runtime and integrated test sources unchanged. All recorded inputs match isolated project.',
  'Full main-page/Main/account/resource/report/audio/effect/platform production assembly, remaining19 controllers/all business and new Player/original audiovisual acceptance remain required. Lifecycle19/38 unchanged.'
 ],notClaimed='Complete Proj_xqzdStartUI/Main/account/platform/all business, remaining19 controllers, inventory persistence, real sound/effect/report transport, new Player and original audiovisual acceptance.')

write(analysis/'sevenday-entry-validation.json',focused)
for name in ('unity-integrated-validation.json','sevenday-entry-integrated.log','sevenday-entry-native-validation.json','sevenday-entry-native.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'SEVENDAY_ENTRY_AUDIT.json',dict(status='original-main-entry-red-native-restart-verified-main-assembly-pending',atUtc=now,sourceEvidence='SEVENDAY_ENTRY_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],native=native,remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,sevendayEntryAudit='generated/outgame/SEVENDAY_ENTRY_AUDIT.json');state['validation'].update(integratedChecksPassed=1419,sevendayEntry=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-main-entry-red-native-restart-verified-main-assembly-pending',integratedChecks=1419,newChecks=9,nativeChecks=14,freshPlayModeRun=True,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1419,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for p in ('analysis/sevenday-entry-validation.json','analysis/sevenday-entry-native-validation.json'):
 if p not in manifest['reports']:manifest['reports'].append(p)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original main seven-day entry/red and lifecycle slices connected to actual control/messages/page registry; native original entry through real claims, red completion, close, automatic activity save and independent restart verified.1419 integrated including9 new,14 native checks,{len(fingerprints)} matching inputs,6044 normal methods. Lifecycle19/38; full main-page/Main/remaining business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1419,newChecks=9,native=14,fingerprints=len(fingerprints),indexedMethods=6044,controllers=19,remaining=19)))
