"""Verify evidence references and coverage consistency without executing game code."""
import json
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
e=json.loads((ROOT/'generated/combat-evidence.json').read_text(encoding='utf8'))
c=json.loads((ROOT/'generated/combat-skill-coverage.json').read_text(encoding='utf8'))
checked=0
for r in e['rules']:
 for ref in r['evidence']:
  if isinstance(ref,str):assert (ROOT/ref).exists(),ref
  elif 'disassembly' in ref:
   f=ROOT/ref['disassembly'];lines=f.read_text(encoding='utf8').split('\n');a,z=ref['lines']
   assert 1<=a<=z<=len(lines),(f,a,z)
   assert (ROOT/ref['wasm']).stat().st_size>=ref['bodyOffset']+ref['bodySize']
   assert f'function {ref["function"]};' in lines[0],f
   assert f'offset={ref["bodyOffset"]} size={ref["bodySize"]}' in lines[0],f
   if ref['instructionOffsetsHex']:
    assert lines[a-1].startswith(ref['instructionOffsetsHex'][0]),f
    assert lines[z-1].startswith(ref['instructionOffsetsHex'][1]),f
   checked+=1
ids={r['id'] for r in e['rules']}
for case in e['derivedCases']:assert set(case['rules'])<=ids,case['id']
assert len({s['skillId'] for s in c['skills']})==18
assert all(len(s['levels'])==10 and s['ruleId'] in ids for s in c['skills'])
assert c['boss']['scannedLayoutCount']==639
assert c['boss']['serializedActiveStarCount']==2
assert [b['configId'] for b in c['boss']['configs'] if b['status']=='unusedconfig']==[24]
report={'passed':True,'rules':len(e['rules']),'functionReferences':checked,'derivedCases':len(e['derivedCases']),'skills':18,'skillConfigs':sum(len(s['levels']) for s in c['skills']),'layouts':639,'scope':'reference integrity and static coverage consistency only; no target execution or runtime verification'}
(ROOT/'generated/combat-validation.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print(json.dumps(report))
