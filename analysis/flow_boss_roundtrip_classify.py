"""Classify Unity's dormant start-delay curve initialization without hiding strict differences."""
import json,hashlib
from pathlib import Path
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43';S=R/'generated/resource-snapshots/boss-entities-20260928'
read=lambda p:json.loads(p.read_text(encoding='utf8'))
strict=read(S/'embedded-particle-roundtrip-validation.json');manifest=read(S/'prepared-embedded/native-import.json');objs={o['id']:o for o in read(R/'generated/asset-evidence.json')['objects']};rows=[]
for entry in strict['components']:
 if entry['passed']:continue
 source=read(R/objs[entry['component']]['outputs']['typetree']);prefab=next(p for p in manifest['prefabs'] if p['name']==entry['prefab']);component=next(c for c in prefab['components'] if c['id']==entry['component']);actual=read(Path('analysis/boss-embedded-roundtrip')/entry['prefab']/(component['fileId']+'.json'))['ParticleSystem']
 assert source['startDelay']['minMaxState']==actual['startDelay']['minMaxState']==0
 assert source['startDelay']['scalar']==actual['startDelay']['scalar']
 for delta in entry['differences']:
  assert delta['path'] in ('.startDelay.maxCurve.m_Curve','.startDelay.minCurve.m_Curve') and delta['sourceLength']==0 and delta['actualLength']==2,delta
 rows.append({'component':entry['component'],'prefab':entry['prefab'],'path':entry['path'],'sourceConstantStartDelay':source['startDelay']['scalar'],'nativeConstantStartDelay':actual['startDelay']['scalar'],'mode':0,'sourceStartDelay':source['startDelay'],'nativeStartDelay':actual['startDelay'],'classification':'Unity initializes dormant empty min/max curves while mode remains Constant; active delay scalar unchanged.'})
out={'strictAllFieldsEqual':strict['passed'],'activeParticleValuesEqual':True,'numericScalarsChecked':strict['numericScalars'],'particleSystems':strict['particleSystems'],'normalizedInactiveCurveArrays':sum(len(c['differences']) for c in strict['components']),'normalizations':rows,'strictReportSha256':hashlib.sha256((S/'embedded-particle-roundtrip-validation.json').read_bytes()).hexdigest()}
(S/'embedded-particle-migration-evidence.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8');print({k:v for k,v in out.items() if k!='normalizations'})

