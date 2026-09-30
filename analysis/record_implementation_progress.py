"""Record verified implementation milestones without passing full-goal gates."""
import json
from pathlib import Path
from datetime import datetime, timezone
P=Path(__file__).resolve().parent.parent
R=P/'analysis/targets/wxcf1394487200e48f/43'
def read(p): return json.loads(p.read_text(encoding='utf-8'))
def write(p,v): p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
stamp=datetime.now(timezone.utc).isoformat()
tests=read(P/'analysis/unity-mechanics-validation.json')
player=read(P/'analysis/player-smoke.json')
build=read(P/'analysis/unity-build-report.json')
assert tests['passed'] and player['passed'] and build['result']=='Succeeded'
count=len(tests['checks'])
integrated=read(P/'analysis/unity-integrated-validation.json')
integrated_count=len(integrated['checks'])
assert integrated['passed']
b=read(R/'BATTLEFIELD_RESTORE_STATE.json')
ordinary=['phase-flow','preparation','deployment-and-production','entities-and-attributes',
 'targeting-and-movement','attack-and-damage','outcomes-and-persistence','presentation-and-feedback']
for key in b['subsystems']:
 s=b['subsystems'][key];s['implementationStatus']='partial';s['validationStatus']='integration-pass'
 s['tests']=list(dict.fromkeys(s.get('tests',[])+['../../../../analysis/unity-mechanics-validation.json','../../../../analysis/player-smoke.json','../../../../analysis/unity-integrated-validation.json']))
 s['notes']='Source-derived production runtime fixtures pass for the listed paths; remaining unknowns and matched original replay prevent full subsystem acceptance.'
updates={
 'phase-flow':'Ordinary/special/PVP entry, running/pause/result/retry and nine tutorial entrances tested. Original replay remains open.',
 'preparation':'Input, skill stock guards, original collider topology and 15 obstacle entities tested. Account inventory remains an explicit local fixture.',
 'deployment-and-production':'Production and player-only Commander multiplier consumers tested, including strict boundaries and retained clocks.',
 'entities-and-attributes':'Original unit data, summoned units and skill attribute mutations tested. Original runtime comparisons pending.',
 'targeting-and-movement':'Ordinary AI, forwarding, arrows and source-derived PVP skill target rules tested. Matched live mode traversal remains pending.',
 'attack-and-damage':'Ordinary collision/arrival, 18 skills, Boss998/999 and PVP outcomes tested against static source fixtures.',
 'skills-buffs-status':'18 skills implemented and traversed through production input. Skill15 source async continuation across retry is tested. No active TowerBuff producer was found; no behavior invented.',
 'waves-and-scaling':'639 layouts and 561 normal-level mappings tested. Daily initial/refill selection is unknown; stored daily pool pop-last and explicit special/PVP entry adapters are tested.',
 'outcomes-and-persistence':'Normal victory advances local saved progress; defeat/special does not. PVP sum-score outcomes and explicit special entry traversal tested; original account comparison is outside the local profile fixture.',
 'presentation-and-feedback':'Original default GPU-animated soldiers, 15 obstacle models, waylines, arrows, gesture resources, UGUI result animations and skill native effects imported. Two original Boss Spines, soldiers, 18 skill resources, embedded ice/speed, dynamic HUD and Guide Spine/hand restored and traversed. Level871 user entry screenshot compared after source-derived background, initial labels, font fallback, commander and camp-highlight corrections. Full synchronized replay remains pending.'}
for key,value in updates.items(): b['subsystems'][key]['unknowns']=[value]
b['blockers']=['Source-derived integration passes within fixture coverage; unresolved resource-load races and daily initial/refill entry remain explicit.',
 'Original level871 entry screenshot compared; full visual/audio replay and other modes lack synchronized original frames.']
b['nextActions']=['Extend the recorded level871 entry comparison to original action and result frames','Resolve original RNG/resource-load timing and daily initial/refill selection against runtime evidence']
b['milestones']=b.get('milestones',[])
milestone={'id':'ordinary-build-smoke','atUtc':stamp,'mechanicsCasesPassed':count,'playerFrames':player['frames'],
 'integratedMechanicsCasesPassed':integrated_count,
 'retryTopologyPreserved':player['retryTopologyPreserved'],'build':'../../../../Build/Windows/AreaBattle.exe',
 'scope':'Ordinary layout5, controlled baseline configuration; full-goal claim remains incomplete'}
b['milestones']=[m for m in b['milestones'] if m.get('id')!=milestone['id']]+[milestone]
write(R/'BATTLEFIELD_RESTORE_STATE.json',b)
status=read(R/'generated/analysis-status.json')
status.update(updatedAtUtc=stamp,stage='source-derived-full-in-level-integration',
 ordinaryMechanicsCases=count,integratedMechanicsCases=integrated_count,standaloneSmoke=player,
 limitations=['Level871 static entry reference compared; full matched visual/audio replay remains unavailable','Source RNG and resource-load latency are not matched to a captured original session','Daily initial/refill selection and unobserved async asset-load races remain explicit'])
write(R/'generated/analysis-status.json',status)
report=f'''# AreaBattle restoration progress

Updated {stamp}. Target: 冲向那座塔 / wxcf1394487200e48f / 43.

The active goal is complete in-level mechanics and presentation. Overall status: incomplete.

## Verified implementation and runtime fixtures

- Editable Unity project: `UnityProject/`, Unity 6000.0.68f1. Original editor was 2021.3.56f2; migration tested by actual compile/build.
- Windows executable: `Build/Windows/AreaBattle.exe` (requires its adjacent Data/runtime files).
- {count} production mechanics cases pass, including 18 direct original WASM numeric oracle cases.
- {integrated_count} integrated cases pass across ordinary, arrow, all nine tutorial entrances, all-layout data/obstacle import, level mapping, local progression, 18 active skills, Boss998/999 and PVP.
- 600-frame actual Windows binary smoke passes, with spawning before/after retry and identical topology.
- Ordinary layout5 and fixtures cover production, AI, damage, capture, forwarding, opposing contacts, pause, victory, defeat and retry.
- All 639 layout bundles verified against the source catalog; 5,752 towers and 2,862 obstacles recovered.

## Remaining full-goal work

- Verify source-derived runtime against the original session, including special modes and asynchronous callback timing; source-proven skill2/12/15 retry fixtures already pass.
- All 18 skills have production input traversal fixtures. Boss Spines, native status/skill particles, guide animation/hand, capacity marks and audio lifecycle are restored; original timing comparison remains open.
- Original default soldiers, Bosses, waylines, all 15 obstacle entities, UGUI and result animations use recovered assets. Async resource-load races and original RNG alignment remain outside proven coverage.
- User confirmed the level871 reference is entry without input. Its recovered layout120, seven towers, startup AI topology, blue highlights, background, UI and commander are now compared in `generated/original871-visual-comparison.json`. Full timed replay remains pending; desktop screenshot helper is still unavailable.

Evidence: `analysis/unity-integrated-validation.json`, `analysis/player-smoke.json`, `analysis/unity-build-report.json`, `analysis/captures`, and `analysis/targets/wxcf1394487200e48f/43/generated/INLEVEL_RESOURCE_AUDIT.md`.
'''
(P/'RESTORE_PROGRESS.md').write_text(report,encoding='utf-8')
(P/'analysis/VALIDATION_REPORT.md').write_text(report,encoding='utf-8')
print(f'Recorded {count} ordinary cases and standalone smoke; full-goal gates remain pending.')
