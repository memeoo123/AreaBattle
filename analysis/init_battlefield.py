import json
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
p=ROOT/'BATTLEFIELD_RESTORE_STATE.json'
if not p.exists():
    template=Path('E:/Projects/weichatAnalysis/cangshu/skills/wechat-minigame-battlefield-restorer/assets/BATTLEFIELD_RESTORE_STATE.template.json')
    data=json.loads(template.read_text(encoding='utf-8'))
    data['target']={'appId':'wxcf1394487200e48f','version':'43','representativeLevel':'layout_1, followed by layout_2 and coverage of all in-level mechanics'}
    data['sourceArtifacts']['implementation']='../../../../../UnityProject'
    for key,item in data['subsystems'].items():
        item['notes']='Scope is this tower-connection game; template examples such as merge grids are not asserted to exist.'
        if key in ['deployment-and-production','entities-and-attributes','attack-and-damage','targeting-and-movement','phase-flow','presentation-and-feedback']:
            item['evidenceStatus']='partial'
            item['evidence']=['generated/gameplay-symbols.json','generated/gameplay-method-map.json','generated/level-summary.json']
    data['subsystems']['preparation']['unknowns']=['Confirm original level-entry, tower selection, drag-connect/cut and battle-start lifecycle; do not assume grid/merge/shop mechanics.']
    data['subsystems']['waves-and-scaling']['unknowns']=['Determine level variants, special/boss schedules and scaling actually used by this game.']
    data['blockers']=['Complete static and controlled-runtime battle evidence before production implementation.','Original screenshot capture returned SetIsBorderRequired 0x80004002; visual baseline is not available yet.']
    data['nextActions']=['Recover collision, movement, AI and flow consumers','Validate raw mechanics cases and produce RESTORE_SPEC','Build Unity simulation and then restore presentation against original references']
    p.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
print(p)
