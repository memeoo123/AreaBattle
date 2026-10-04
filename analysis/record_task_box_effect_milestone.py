"""Publish original task-box asset/binding milestone once, after verified native rendering."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/'analysis';target=analysis/'targets/wxcf1394487200e48f/43';out=target/'generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists()and b'\r\n'in p.read_bytes();p.write_bytes((text.replace('\n','\r\n')if crlf else text).encode())
full=read(validation/'analysis/unity-integrated-validation.json');assert full['passed']and len(full['checks'])==1554 and all(r['result']=='pass'for r in full['checks'])
log=(validation/'analysis/task-box-effect-integrated.log').read_text();assert 'AREABATTLE_INTEGRATED_PASS cases=1554'in log and 'error CS'not in log
focused=dict(full,checks=[r for r in full['checks']if r['id'].startswith('task-box-effect-')]);assert len(focused['checks'])==8
focused['limitations']='Original1016 prefab/config/materials/textures, recovered resource wrappers and real task/account/EffectModule binding. Local native AssetBundle acquisition host; complete production resource catalog, EffectControl/NormalPool/fly composition, Main/account/platform/all business and Player/original audiovisual equivalence remain pending.'
native=read(validation/'analysis/task-box-effect-native-validation.json');assert native['passed']and len(native['checks'])==14 and native['particlePixels']>0 and native['restoredGold']==150
native_log=(validation/'analysis/task-box-effect-native.log').read_text();assert 'AREABATTLE_TASK_BOX_EFFECT_NATIVE_PASS checks=14'in native_log and 'error CS'not in native_log
evidence=read(out/'TASK_BOX_EFFECT_SOURCE_EVIDENCE.json');assert(len(evidence['methods']),len(evidence['dependencies']))==(8,6)
for row in evidence['methods']+evidence['dependencies']:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
for key in ('acquisition','particleReadback'):
 row=evidence[key];assert hashlib.sha256((target/row['path']).read_bytes()).hexdigest()==row['sha256']
assert evidence['acquisition']['bundles']==10 and evidence['particleReadback']['passed'] and evidence['particleReadback']['numericScalars']==7362
assert len(read(out/'method-map.json'))==6384 and read(out/'CONTROLLER_LIFECYCLE_MATRIX.json')['implementedLifecycleControllers']==20
manifest=read(analysis/'VALIDATION_MANIFEST.json');paths={r['path']for r in manifest['sourceFingerprints']}
files=('Scripts/OutgameEffectResources.cs','Editor/OutgameTaskBoxEffectValidation.cs','Editor/OutgameTaskBoxEffectPlayModeValidation.cs','Editor/RecoveredSkillEffectImporter.cs','Editor/BattleBuild.cs')
for name in files:paths.update(('UnityProject/Assets/AreaBattle/'+name,'UnityProject/Assets/AreaBattle/'+name+'.meta'))
assets=workspace/'UnityProject/Assets/AreaBattle/Resources/Recovered/TaskBoxEffect'
paths.add(str(assets.relative_to(workspace))+'.meta');paths.update(str(p.relative_to(workspace))for p in assets.rglob('*')if p.is_file())
fingerprints=[]
for name in sorted(paths):
 data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name;fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone='task-box-original-effect-native-binding';state=read(target/'OUTGAME_RESTORE_STATE.json');assert not any(r.get('id')==milestone for r in state['milestones'])
next_priority='Recover complete EffectControl4058 NormalPool creation/sequence/fly lifecycle and connect original currency/effect collection owners. Continue full production resource catalog, Main/account/login/platform/all remaining18 controllers and business, final Player and original audiovisual acceptance.'
record=dict(atUtc=now,scope='Original task-box effect1016 assets/resource wrappers and actual task reward/tab/timer/account binding',checksPassed=1554,newIntegratedChecks=8,targetedChecks=8,nativeChecks=14,nativeParticlePixels=native['particlePixels'],freshPlayModeRun=True,freshPlayerBuild=False,freshPlayerSmoke=False,
 editorVersion=full['unityVersion'],platform='OSXEditor',projectVersionPreserved='6000.0.68f1',isolatedProject=str(validation/'UnityProject'),matchingSourceFingerprints=len(fingerprints),implementedLifecycleControllers=20,remainingLifecycleControllers=18,commands=['BattleBuild.ValidateMechanicsOnly','OutgameTaskBoxEffectPlayModeValidation.Run'],
 observedBoundaries=evidence['findings']+['Final integrated retains32 existing ShouldRunBehaviour assertions and one Curl35 certificate failure. Native retains one UnityEditor.Search startup ArgumentOutOfRangeException and one Curl42; product checks and compilation pass. Initial missing Canvas sorting fixture failure preserved. Original UIRoot parenting requires settled host Canvas; final native depth check and same-frame renderer on/off pixel difference verify actual rendered particles, without claiming original-frame visual equivalence.'],notClaimed='Complete EffectControl4058/NormalPool/fly lifecycle and resource catalog, production Main/account/platform, remaining18 controllers/all business, fresh Player/original audiovisual equivalence.')
write(analysis/'task-box-effect-validation.json',focused)
for name in ('unity-integrated-validation.json','task-box-effect-integrated.log','task-box-effect-native-validation.json','task-box-effect-native.log','task-box-effect-render-diagnostics.txt','task-box-effect-initial-task-box-effect-native.log','task-box-effect-initial-task-box-effect-native-validation.json'):shutil.copy2(validation/'analysis'/name,analysis/name)
for name in ('task-box-effect-native.png','task-box-effect-with-particles.png','task-box-effect-without-particles.png'):shutil.copy2(validation/'analysis/captures'/name,analysis/'captures'/name)
write(out/'TASK_BOX_EFFECT_AUDIT.json',dict(status='original-task-box-effect-native-binding-verified-full-controller-composition-pending',atUtc=now,sourceEvidence='TASK_BOX_EFFECT_SOURCE_EVIDENCE.json',verification=record,implementation=list(files),checks=focused['checks'],nativeChecks=native['checks'],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,taskBoxEffectAudit='generated/outgame/TASK_BOX_EFFECT_AUDIT.json');state['validation'].update(integratedChecksPassed=1554,taskBoxEffect=record)
state['milestones'].append(dict(id=milestone,atUtc=now,status='original-task-box-effect-native-binding-verified-full-controller-composition-pending',integratedChecks=1554,newChecks=8,nativeChecks=14,freshPlayModeRun=True,remainingLifecycleControllers=18));write(target/'OUTGAME_RESTORE_STATE.json',state)
by_path={r['path']:r for r in fingerprints};ordered=[by_path.pop(r['path'])for r in manifest['sourceFingerprints']if r['path']in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1554,latestValidation=record,sourceFingerprints=ordered+[by_path[n]for n in sorted(by_path)]);manifest['validationHistory'].append(record)
for name in ('analysis/task-box-effect-validation.json','analysis/task-box-effect-native-validation.json'):
 if name not in manifest['reports']:manifest['reports'].append(name)
write(analysis/'VALIDATION_MANIFEST.json',manifest)
execution=read(analysis/'RESTORATION_EXECUTION_STATE.json');execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution['completedThisRun'].append(f'Original task effect1016 dependency closure10 bundles (7new/82086bytes),4 particles/7362 scalars roundtrip and native task/account binding:1554 integrated including8 new,14 native;486 actual renderer pixels. {len(fingerprints)} matching inputs;6384 normal methods, controllers20/38 unchanged. Complete EffectControl/resource catalog/Main/account/platform/business/Player pending.');write(analysis/'RESTORATION_EXECUTION_STATE.json',execution)
print(json.dumps(dict(integrated=1554,newChecks=8,native=14,fingerprints=len(fingerprints),indexedMethods=6384,controllers=20,remaining=18)))
