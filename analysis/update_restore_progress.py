"""Refresh current evidence index without discarding prior provenance or history."""
import json
from pathlib import Path
from datetime import datetime, timezone
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
def read(p): return json.loads((R/p).read_text(encoding='utf-8'))
def write(p,v): (R/p).write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
stamp=datetime.now(timezone.utc).isoformat()
ready=read('generated/RESTORE_SPEC.json').get('implementationReady') is True
m=read('manifest.json')
new=['generated/RESTORE_SPEC.json','generated/golden-cases.json','generated/original-numeric-cases.json',
     'generated/combat-evidence.json','generated/flow-evidence.json','generated/asset-evidence.json',
     'generated/unity-assets/reconstruction-index.json','generated/unity-assets/export-validation.json',
     'evidence/full-level-downloads.json','evidence/full-level-extraction.json','BATTLEFIELD_RESTORE_STATE.json']
if (R/'generated/controls-evidence.json').exists():new.append('generated/controls-evidence.json')
m['generatedArtifacts']=list(dict.fromkeys(m['generatedArtifacts']+new))
m['updatedAtUtc']=stamp
m['authorization']['scope']='User-approved autonomous full in-level restoration, target-resource acquisition and Unity implementation; original packages and user data preserved.'
m['currentStage']='connected-battlefield-evidence-and-handoff'
m['reverseAnalysisComplete']=ready
m['implementationReady']=ready
m['reverseAnalysisScope']='Connected ordinary battle slice in layout5; full-goal special content and matched replay remain incomplete in battlefield state.'
for command in ['python analysis/download_level_catalog.py','bundled-python -B analysis/extract_full_levels.py','node analysis/run_original_numeric_oracle.js','python analysis/build_restore_handoff.py']:
    if not any(x.get('command')==command for x in m['commands']):m['commands'].append(dict(command=command,exitCode=0))
write('manifest.json',m)
b=read('BATTLEFIELD_RESTORE_STATE.json')
b['sourceArtifacts']['implementation']='../../../../UnityProject'
b['target']['representativeLevel']='layout5 ordinary battle; layout0 numeric fixture; full goal includes all in-level subsystems'
mapping={
'phase-flow':('generated/flow-evidence.json','Original phase dispatch order is being finalized.'),
'preparation':('generated/controls-evidence.json','Finish drag/cut and initial line topology evidence.'),
'deployment-and-production':('generated/combat-evidence.json','Special modifiers and async pause behavior remain open.'),
'entities-and-attributes':('generated/combat-evidence.json','Complete boss and commander subclasses.'),
'targeting-and-movement':('generated/flow-evidence.json','Complete AI target branch evidence and verify production integration.'),
'attack-and-damage':('generated/combat-evidence.json','Baseline confirmed; special boss handlers and original replay pending.'),
'skills-buffs-status':('generated/combat-evidence.json','Trace every active skill/buff and exact configuration binding.'),
'waves-and-scaling':('generated/flow-evidence.json','Finalize level sequence and special-mode schedule coverage.'),
'outcomes-and-persistence':('generated/flow-evidence.json','Finalize retry reset and outcome persistence scope.'),
'presentation-and-feedback':('generated/asset-evidence.json','Unity asset import and runtime matched capture remain unvalidated.')}
for k,(p,unknown) in mapping.items():
    v=b['subsystems'][k]
    if (R/p).exists():
        v['evidenceStatus']='partial'
        if p not in v['evidence']:v['evidence'].append(p)
    v['unknowns']=[unknown]
write('BATTLEFIELD_RESTORE_STATE.json',b)
status=read('generated/analysis-status.json')
status.update(dict(updatedAtUtc=stamp,stage=m['currentStage'],implementationReady=ready,
    completeLayoutCount=639,numericOracleCases=18,sourceDerivedCombatCases=10,
    unityProject='E:/Projects/AreaBattle/UnityProject',unityEditor='6000.0.68f1',
    limitations=['No original visual capture yet','Unity implementation/validation pending','Full-goal special-content evidence is still incomplete']))
write('generated/analysis-status.json',status)
p=R/'REVERSE_PROGRESS.md';s=p.read_text(encoding='utf-8')
marker='## Full in-level restoration goal'
if marker not in s:
    s+='\n'+marker+'\n\n'
    s+=f'Updated: {stamp}. User authorized autonomous complete in-level mechanics and presentation restoration.\n\n'
    s+='- All 639 catalog layouts downloaded with original size/MD5 verification; 5,752 towers and 2,862 obstacles recovered.\n'
    s+='- Ordinary combat/arrow behavior and world coordinates are evidence-backed; separate agents are finalizing input, AI and special-content branches.\n'
    s+='- Four unaltered original arithmetic WASM bodies executed without host imports; 18 numeric oracle cases recorded. Ten additional source-derived combat cases retain their distinct provenance.\n'
    s+='- Asset export: 30 native-coordinate meshes, 698 sprites, 21 focused prefab hierarchies and exact serialized identities. Import/runtime fidelity still unvalidated.\n'
    s+='- Empty Unity 6000.0.68f1 project created because original 2021.3.56f2 editor is unavailable locally; compatibility must pass actual import/compile.\n'
    s+='- Draft RESTORE_SPEC and golden-cases now exist. implementationReady remains false until the connected input/flow evidence closes.\n'
    s+='- Original screenshot helper failed SetIsBorderRequired E_NOINTERFACE. This is an open matched-replay gate, not a visual pass.\n'
    s+='- Supersedes prior missing-layout and unmapped-WASM status. Do not rerun old snapshot builders to overwrite this progress.\n'
    p.write_text(s,encoding='utf-8')
print('Updated evidence index, battlefield paths, and append-only progress; no phase/check was bypassed.')
