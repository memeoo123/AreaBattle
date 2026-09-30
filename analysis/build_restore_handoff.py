"""Assemble evidence-bearing handoff; readiness is computed, never inferred from time spent."""
import json
from pathlib import Path
from datetime import datetime, timezone

ROOT = Path(__file__).parent / 'targets/wxcf1394487200e48f/43'
def read(p):
    return json.loads((ROOT / p).read_text(encoding='utf-8'))
def write(p, value):
    (ROOT / p).write_text(json.dumps(value, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
def confirmed(value, *evidence):
    return dict(value=value, status='confirmed', evidence=list(evidence))

combat = read('generated/combat-evidence.json')
flow = read('generated/flow-evidence.json')
oracle = read('generated/original-numeric-cases.json')
assets = read('generated/unity-assets/reconstruction-index.json')
controls_path = ROOT / 'generated/controls-evidence.json'
controls = read('generated/controls-evidence.json') if controls_path.exists() else None
layout = read('generated/all-levels/level_5.json')

golden = dict(schemaVersion='1.0', target=oracle['target'],
    numericOracle=oracle, cases=oracle['cases'] + [dict(case, provenance='source-derived; full-game runtime comparison pending') for case in combat['derivedCases']],
    limitations=['Original arithmetic functions are executed; whole-game capture/replay has not yet succeeded.',
                 'Source-derived combat cases are independent expected values, not claimed original runtime observations.'])
write('generated/golden-cases.json', golden)

scene_objects = {o['id']: o for o in read('generated/asset-evidence.json')['objects']}
def tree(key):
    return read(scene_objects[key]['outputs']['typetree'])
camera = tree('BuildPlayer-GamePlay:46')
transforms=[]
key='BuildPlayer-GamePlay:30'
while key in scene_objects:
    data=tree(key)
    transforms.append(dict(id=key,path=scene_objects[key]['outputs']['typetree'],data=data))
    parent=data['m_Father']['m_PathID']
    if not parent: break
    key='BuildPlayer-GamePlay:'+str(parent)

unknowns = [
    dict(id='whole-game-replay', status='unknown', scope='full goal',
         verification='Obtain original screenshots/video and matched input timing; sky capture failed SetIsBorderRequired E_NOINTERFACE.'),
    dict(id='special-content', status='unknown', scope='later slices',
         verification='Resolve Commander/boss/status branches and add separate mechanism goldens before enabling those layouts.'),
    dict(id='presentation-runtime', status='unknown', scope='presentation',
         verification='After mechanicsData passes, compare camera adaptation, animated soldiers, HUD/effects/audio with matched original frames.'),
]
if not controls:
    unknowns.append(dict(id='input-and-topology',status='unknown',scope='initial slice',verification='Complete controls-evidence for drag/cut, eligibility and initial adjacency.'))
spec=dict(schemaVersion='1.0', implementationReady=False,
    target=dict(appId='wxcf1394487200e48f',version='43',engine='unity',engineVersion='2021.3.56f2'),
    scope=dict(fullGoal='Complete in-level mechanics and presentation; no lobby commercialization',
               firstConnectedSlice='Ordinary battle simulation and player controls in layout5, using a controlled unmodified loadout; layout0 numeric fixture',
               acceptance='battlefield-faithful',
               note='A ready connected slice does not mean the complete battlefield goal is met.'),
    design=dict(orientation=confirmed('portrait','generated/modules/game.js'),
                width=dict(value=None,status='unknown',verification='Recover UI scaler/runtime viewport'),
                height=dict(value=None,status='unknown',verification='Recover UI scaler/runtime viewport')),
    representativeLevel=dict(id=confirmed(5,'generated/all-levels/level_5.json'),
        source='generated/all-levels/level_5.json',raw=layout,playerCamp=confirmed(1,'flow:player-camp'),
        initialTowers=confirmed([dict(id=i+1,camp=t['CampID'],score=t['StartScore'],ship=t['ShipID'],
            worldPosition={k:v/100 for k,v in t['pos'].items()}) for i,t in enumerate(layout['StarInfoCfgs'])],
            'flow:normal-level-tower-init','flow:tower-world-position'),
        modifiers=confirmed('No selected commander or applied buffs in this controlled fixture','combat:spawn-multiplier'),
        originalAccountState=dict(status='unknown',verification='Original capture and loadout observation required')),
    scene=dict(anchors=confirmed('Source Unity XYZ; world pos=serialized pos/100','flow:tower-world-position'),
        layers=[],uiHierarchy='generated/unity-assets/prefabs/Proj_xqzdPlayUI.hierarchy.json',
        serializedCamera=confirmed(camera,scene_objects['BuildPlayer-GamePlay:46']['outputs']['typetree']),
        cameraTransformChain=confirmed(transforms,*[t['path'] for t in transforms]),
        runtimeCameraAdaptation=dict(status='unknown',verification='Trace GameSceneControl and observe runtime aspect adaptation')),
    assets=assets['prefabs'],
    entities=confirmed(read('generated/tables/SoldierConfig.json')['Datas'],'generated/tables/SoldierConfig.json','combat:soldier-stats'),
    spawners=confirmed(read('generated/tables/DispatchConfig.json')['Datas'][:3], 'generated/tables/DispatchConfig.json','combat:spawn-clock','original-numeric-cases.json'),
    combat=dict(rules=combat['rules'], enums=combat['enumValues']),
    phases=flow['facts'],controls=controls,
    rounds=[dict(levelSource='generated/all-levels/level_5.json',schedule='Initial fixed tower layout plus directional tower production and configured AI camp actions',
                 status='confirmed',evidence=['generated/all-levels/level_5.json','combat:spawn-clock','flow:AI-timer'])],
    goldenCases='golden-cases.json', unknowns=unknowns,
    presentationFallbacks=[dict(item='HUD and active unit debug shapes',status='explicit temporary fallback',reason='Make mechanics observable before presentation acceptance; original exports retained.')],
    evidenceIndex={'combat':'generated/combat-evidence.json','flow':'generated/flow-evidence.json',
                   'assets':'generated/asset-evidence.json','controls':'generated/controls-evidence.json',
                   'oracle':'generated/original-numeric-cases.json'},
    generatedAt=datetime.now(timezone.utc).isoformat())
# Scope gate is the connected ordinary slice, not the full user goal. Special content,
# guide flow and matched replay remain explicit required work in battlefield state.
required_controls={'adjacency-generation','capsule-blocking','line-direction-states','remove-outgoing-only',
                   'line-count-outgoing','press-player-source','drag-preview-release-commit','cut-player-direction'}
required_flow={'AI-reinforce-and-occupy','AI-protect','AI-defend','pause-and-update-gates','retry-initial-score'}
control_ids={r['id'] for r in (controls or {}).get('rules',[]) if r['status']=='confirmed'}
flow_ids={r['id'] for r in flow['facts'] if r['status']=='confirmed'}
pause=next((x['value'] for x in flow['facts'] if x['id']=='pause-and-update-gates'),{})
ready=required_controls<=control_ids and required_flow<=flow_ids and 'AICampController -> LevelControl' in pause.get('order','')
if ready:
    spec['implementationReady']=True
    spec['remainingFullGoalWork']=unknowns+[
        dict(id='guide-and-special-modes',status='unknown',scope='later slices',verification='Recover guide restrictions and special level controllers before claiming full battlefield fidelity.'),
        dict(id='ai-random-runtime-state',status='unknown',scope='matched replay',verification='Inject a controlled RNG sequence for deterministic tests and recover original seed/sequence for matched replay.')]
    spec['unknowns']=[]
    spec['design']={'orientation':confirmed('portrait','generated/modules/game.js'),
                    'referenceAspect':confirmed(0.5625,'flow:camera-adaptation'),
                    'testViewport':{'width':540,'height':960,'origin':'local test fixture at the recovered reference aspect; not an observed device resolution'}}
    spec['scene']['runtimeCameraAdaptation']=confirmed('orthoSize=2.1*(0.5625/aspect)','flow:camera-adaptation')
    spec['scope']['ordinarySliceBoundary']='Includes fixed layout, controls, line topology, production, ordinary soldier combat, AI, pause, outcomes and retry. Excludes tutorial/commander/boss branches from this first implementation only; full acceptance remains required.'
    # Correct the earlier broad interpretation of camp-change cleanup with the more
    # precise callee evidence. Preserve the correction alongside both source records.
    spec['evidenceCorrections']=[dict(id='camp-change-lines',rule='RemoveStarAllLine removes outgoing only; incoming directions survive capture.',
                                    evidence=['controls:remove-outgoing-only','combat:camp-change'])]
write('generated/RESTORE_SPEC.json', spec)
print(json.dumps(dict(implementationReady=spec['implementationReady'],numericCases=len(golden['cases']),controlsAvailable=bool(controls))))
