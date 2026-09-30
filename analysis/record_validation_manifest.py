"""Fingerprint the verified representative mechanics build, retaining full-goal gaps."""
import json,hashlib
from pathlib import Path
from datetime import datetime,timezone
P=Path(__file__).resolve().parent.parent
def read(p):return json.loads((P/p).read_text(encoding='utf-8'))
checks=read('analysis/unity-integrated-validation.json');build=read('analysis/unity-build-report.json');smoke=read('analysis/player-smoke.json')
assert checks['passed'] and build['result']=='Succeeded' and smoke['passed']
assert (P/'analysis/player-smoke.json').stat().st_mtime >= (P/'Build/Windows/AreaBattle.exe').stat().st_mtime, 'Smoke report predates latest binary'
assert 'AREABATTLE_PLAYER_SMOKE_PASS' in (P/'analysis/player-smoke.log').read_text(encoding='utf-8',errors='replace'), 'Latest player log lacks pass marker'
time=(P/'analysis/unity-integrated-validation.json').stat().st_mtime
sources=[]
for path in sorted((P/'UnityProject/Assets/AreaBattle').rglob('*')):
 if path.is_file() and path.suffix in ['.cs','.shader','.json','.prefab','.mat','.unity']:
  if path.suffix in ['.cs','.shader']:assert path.stat().st_mtime<=time,f'New unvalidated source: {path}'
  sources.append({'path':path.relative_to(P).as_posix(),'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
manifest={'schemaVersion':'1.0','passed':True,'atUtc':datetime.now(timezone.utc).isoformat(),
 'target':{'appId':'wxcf1394487200e48f','version':'43'},
 'scope':'Controlled representative ordinary layout5 and source-derived subsystem fixtures; not a complete battlefield or matched replay claim.',
 'mechanicsDataScope':['layout5 preparation/input/topology/production/AI/combat/pause/result/retry',
  'Source-derived arrow,guide,18 skills,Boss998/999,PVP and all639-layout initialization/collider fixtures', 'Six original commander animations, font fallback and level871 user screenshot anchors'],
 'excludedByUser':['Lobby and commercialization'],
 'pendingFullGoal':['Matched visual/audio timing and RNG','Unobserved resource-load callback races and daily pool initialization','Single level871 entry frame compared; complete original timed replay remains pending'],
 'commands':['Unity.exe -batchmode -executeMethod AreaBattle.EditorTools.BattleBuild.BuildAndValidate',
  'Build/Windows/AreaBattle.exe -batchmode -nographics -battle-smoke -battle-smoke-report analysis/player-smoke.json -logFile analysis/player-smoke.log'],
 'reports':['analysis/unity-integrated-validation.json','analysis/unity-build-report.json','analysis/player-smoke.json'],
 'caseCount':len(checks['checks']),'sourceFingerprints':sources}
(P/'analysis/VALIDATION_MANIFEST.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Verified representative manifest:',len(checks['checks']),'cases;',len(sources),'source fingerprints; full goal incomplete.')
