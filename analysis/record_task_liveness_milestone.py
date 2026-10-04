"""Record verified original liveness preview/tier/native animation milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1475 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/task-liveness-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1475'in log and 'error CS'not in log
native=read(validation/'analysis/task-liveness-native-validation.json');assert native['passed']and len(native['checks'])==14
native_log=(validation/'analysis/task-liveness-native.log').read_text();assert 'error CS'not in native_log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('task-liveness-')]);assert len(focused['checks'])==12
focused['limitations']='Original liveness preview/tier source ordering, real daily strategy/economy and native source animation components. Controlled frame endpoints; real Unity frame/selection/animation/save/restart verified separately. Full task page/achievement/controller/Main and production Effect1016/fly/sprite/popup/report hosts, particle effects and original audiovisual/Player acceptance pending.'
evidence=read(out/'TASK_LIVENESS_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(30,63,20)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6218 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
animation=read(validation/'analysis/task-panel-animation-import-report.json');assert animation['passed']and(animation['components'],animation['clips'],animation['curves'],animation['keys'])==(6,2,18,119)
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameLivenessPreviewItem.cs','Scripts/OutgameTaskLivenessBinding.cs','Editor/OutgameTaskPanelAnimationImport.cs','Editor/OutgameTaskLivenessValidation.cs','Editor/OutgameTaskLivenessPlayModeValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
assets=workspace/'UnityProject/Assets/AreaBattle/Resources/Recovered/TaskPanel';paths.update(str(p.relative_to(workspace))for p in assets.rglob('*')if p.is_file());paths.add('analysis/targets/wxcf1394487200e48f/43/generated/outgame/task-panel-animations.json')
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='ordinary-task-liveness-preview-tier-and-native-animation';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore concrete Achievement models/manager/strategy/activity and task controller4507 required by TaskPanelUI daily/achievement tabs. Complete TaskSingleton3990 ownership, TaskPanelUI4416 lifecycle/tab/red-dot/main task entry using recovered rows and liveness preview/tier binding. Continue production Main/account/platform/all remaining19 controllers/business, source Effect1016/particles/visual-audio and final Player acceptance.'
record=dict(atUtc=now,scope='Original task liveness preview/tier interactions and six native Animation components',checksPassed=1475,newIntegratedChecks=12,targetedChecks=12,nativeChecks=14,freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['OutgameTaskPanelAnimationImport.Run','BattleBuild.ValidateMechanicsOnly','OutgameTaskLivenessPlayModeValidation.Run'],
 observedBoundaries=[
  'LivenessPreviewItem4350/async4349 and TaskPanelUI tier methods33877/33881..33883, closures4414/async4415 restored. Effect-close portion33880 restored; tab-content switch/full page still pending. Source30 reviewed methods (24 newly indexed),63 fields,20 usages; ordinary index6218.',
  'Preview initialization registers default visible before Awake deactivates object. Repeated visibility preserves update registration semantics; exact touch/IconBg/Node-substring selection keeps visible, external selection hides by scale. SetData snapshots ItemIcon/atlas/Lang; synchronous render precedes reused end-of-frame await; grow-only name width expands both halves/root. Overlap, reentry, disposed root and failure prefixes preserved.',
  'Tier binding displays actual30/60/100 thresholds, state toggles, initial preview versus eligible claim callbacks, runtime listener replacement and original canvas layer. Refresh below minimum updates red only. Missing item config throws after preview activation/position. Source claimed-state refresh retains old callback/glow and disabled button state.',
  'Claim captures first reward ID/type/low32 count, hides glow, requests Effect1016/stores handle and plays native Animation before new WaitForEndOfFrame. Currency fly uses no economy and top callback; item popup quantity1. Button disables only after effects; actual daily strategy then grants configured rewards/mask and refreshes red. Source allows multiple pending callbacks, so no invented deduplication at unguarded liveness endpoint. Failure/reentry ordering verified.',
  'Six original native Animation components and two shared legacy clips restored: box manual1.7s and glow auto1.5s. Complete18 curves/119 keys roundtrip exact tangents/weights. Original box clip has missing hdzd_eff_bxGlow path on three source owners; unbound tracks retained. ParticleSystems and actual Effect1016 runtime/assets remain pending, no full audiovisual equivalence claim.',
  '1475 integrated including12 new. Fresh visible native14 checks actual EventSystem/UpdateManager preview behavior, pointer detail anchor, real width expansion after render, one-frame actual award while timeScale0 keeps box animation paused, resumed animation/automatic save and independent tier-state restart. Source effects/sprite/localization/popup/report are observed required hosts.',
  'Final integrated log retains32 existing ShouldRunBehaviour assertions and one Curl42; native retains one UnityEditor.Search startup ArgumentOutOfRangeException and one Curl42. No compiler errors. All new focused/native checks pass.',
  'Whole TaskPanelUI lifecycle/tabs/task-controller/Main entry and Achievement remain pending; production Main/account/platform/remaining19 controllers/all business and final Player/original audiovisual acceptance incomplete. Lifecycle19/38 unchanged.'
 ],notClaimed='Whole task page/achievement/task-controller/main entry, complete production effect/account/Main/platform hosts, original particle/visual/audio equivalence or fresh Player.')
write(analysis/'task-liveness-validation.json',focused)
for name in ('unity-integrated-validation.json','task-liveness-integrated.log','task-liveness-native-validation.json','task-liveness-native.log','task-panel-animation-import-report.json','task-panel-animation-import.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'TASK_LIVENESS_AUDIT.json',dict(status='original-liveness-preview-tier-native-animations-verified-page-pending',atUtc=now,sourceEvidence='TASK_LIVENESS_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],nativeChecks=native['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,taskLivenessAudit='generated/outgame/TASK_LIVENESS_AUDIT.json');state['validation'].update(integratedChecksPassed=1475,taskLiveness=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-liveness-preview-tier-native-animations-verified-page-pending',integratedChecks=1475,newChecks=12,nativeChecks=14,freshPlayModeRun=True,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1475,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for name in ('analysis/task-liveness-validation.json','analysis/task-liveness-native-validation.json','analysis/task-panel-animation-import-report.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original liveness preview/tier binding plus six native Animation components/two source clips18 curves119 keys restored.1475 integrated including12 new,14 fresh native EventSystem/frame/animation/reward/save/restart checks,{len(fingerprints)} matching inputs,6218 normal methods. Whole TaskPanelUI/TaskController/Achievement/production effect/Main hosts/particles/Player pending; lifecycle19/38.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1475,newChecks=12,native=14,fingerprints=len(fingerprints),indexedMethods=6218,controllers=19,remaining=19)))
