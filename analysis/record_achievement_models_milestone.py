"""Record verified original Achievement models/factory milestone once."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1487 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/achievement-models-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1487'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('achievement-model-')]);assert len(focused['checks'])==12
focused['limitations']='Original Achievement model/storage schemas/factory,127 original achievement configs and3 cumulative configs, actual item economy/report/delivery sequence. Required activity owner is explicit fixture until concrete Achievement activity/manager/strategy assembly. File roundtrip validates schemas only, not automatic persistence. No fresh native/Player run; full UI/production Main/platform/remaining business pending.'
evidence=read(out/'ACHIEVEMENT_MODELS_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['fields']),len(evidence['usages']))==(21,17,13)
for row in evidence['methods']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
assert len(read(out/'method-map.json'))==6239 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==19
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameAchievementModels.cs','Scripts/OutgameAchievementFactory.cs','Editor/OutgameAchievementModelsValidation.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='achievement-models-storage-and-point-factory';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Restore concrete AchievementMgr4722, AchievementStrategyBase4727, AchievementOffStrategy4725 and AchievementActivity4714 with actual ActivityControl, original statistics events, reward failure order, compact/legacy save migration and independent restart. Then complete TaskController4507/AchiTaskSubUI3986/TaskSingleton3990/TaskPanelUI4416 daily-achievement page/Main entry. Continue all remaining19 controller lifecycles, production Main/account/platform, business and original audiovisual/Player acceptance.'
record=dict(atUtc=now,scope='Original Achievement storage/models and point factory',checksPassed=1487,newIntegratedChecks=12,targetedChecks=12,nativeChecks=0,freshPlayModeRun=False,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=19,remainingLifecycleControllers=19,commands=['BattleBuild.ValidateMechanicsOnly'],
 observedBoundaries=[
  'Source4715..4719 models and4627/4628 point/factory restored from21 reviewed methods,17 fields,13 metadata usages; ordinary method index6239. Actual original tables127 achievements and3 cumulative thresholds30/60/90.',
  'Storage public names/Int64 progress,time,statePts and Int32 accPts retained; private config/selection/show cache excluded. CanComplete resolves config before mutable progress, requires signed threshold and exact state0 without added show/time gates. Sort ready/incomplete/nonzero-state then cached config.id.',
  'Rewards freshly allocate every call, keep signed Int32 quantities widened to Int64 and permit retry after malformed row repair. ShowCondition publishes cache first, boxes copied middle arguments, retains partial prefix after failure. First nonnull config caches remain, missing retry.',
  'Cumulative item reads required owner1601001 live, unsigned Int64 bit selection masks index63, threshold short-circuits state query. Owner used in validation is explicit model fixture; concrete Achievement runtime remains pending. Runtime cumulative config declares only id; no rewards/claim rule inferred from extra raw JSON fields.',
  'Allocation usage3927880 resolves AchievementPoint4627 despite shared constructor annotation. Exact category2/2 factory, regular Add applies real Int64 economy/report/delivery before point event with lowInt32 amount; inherited model-only omits point event. Use reports without debit. Delivery and point callback failures preserve source prefix.',
  '1487 integrated checks including12 new; all2662 inputs byte-match isolated stage. No fresh native/Player run for these model changes; previous14 native task-liveness checks remain historical and are not claimed for Achievement.',
  'Final integrated log retains32 existing ShouldRunBehaviour assertions and one Curl42; no compiler errors. Full manager/strategy/activity automatic save/restart, task/achievement page/controller/Main and remaining business are incomplete; lifecycle19/38 unchanged.'
 ],notClaimed='Concrete Achievement manager/strategy/activity or automatic persistence, full task/achievement UI/controller/Main, production account/platform hosts, original audiovisual equivalence or fresh native/Player.')
write(analysis/'achievement-models-validation.json',focused)
for name in ('unity-integrated-validation.json','achievement-models-integrated.log'):shutil.copy2(validation/'analysis'/name,analysis/name)
write(out/'ACHIEVEMENT_MODELS_AUDIT.json',dict(status='original-achievement-models-point-factory-verified-runtime-pending',atUtc=now,sourceEvidence='ACHIEVEMENT_MODELS_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,achievementModelsAudit='generated/outgame/ACHIEVEMENT_MODELS_AUDIT.json');state['validation'].update(integratedChecksPassed=1487,achievementModels=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-achievement-models-point-factory-verified-runtime-pending',integratedChecks=1487,newChecks=12,nativeChecks=0,freshPlayModeRun=False,remainingLifecycleControllers=19));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1487,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
if 'analysis/achievement-models-validation.json'not in manifest['reports']:manifest['reports'].append('analysis/achievement-models-validation.json')
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original Achievement storage/models and category2/2 point factory restored.1487 integrated including12 new,{len(fingerprints)} matching inputs,6239 normal methods; no fresh native/Player. Real127 achievement configs/3 cumulative thresholds plus actual item engine validated. Concrete Achievement manager/strategy/activity automatic save/restart and full task page/controller/Main remain pending; lifecycle19/38.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1487,newChecks=12,native=0,fingerprints=len(fingerprints),indexedMethods=6239,controllers=19,remaining=19)))
