"""Recover the dynamic skin-to-model binding that supersedes stale SoldierConfig.EntityID."""
import json, subprocess, sys
from pathlib import Path
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
def read(p):return json.loads((R/p).read_text(encoding='utf-8'))
out=R/'generated/model-binding';out.mkdir(exist_ok=True)
methods=[('Soldier','GetSoldierObj'),('PlayerControl','GetSkinEntityByType'),
         ('PlayerControl','GetSkinEntityByRandomType'),('SkinManager','GetUserSkinId'),('LevelControl','InitEnemySkin')]
for cls,method in methods:
    result=subprocess.run([sys.executable,'analysis/flow_disassemble.py',cls,method],capture_output=True)
    result.check_returncode()
    (out/f'{cls}-{method}.txt').write_bytes(result.stdout)
skin=read('generated/tables/SkinConfig.json')['Datas']
model=read('generated/tables/EntityModelConfig.json')['Datas']
bindings=[]
for sid in [100,200,300]:
    s=next(x for x in skin if x['id']==sid)
    m=next(x for x in model if x['id']==s['prefabId'])
    bindings.append(dict(skin=s,model=m))
facts=[dict(id='dynamic-soldier-model',status='confirmed',rule='For ordinary soldier type<=9, Soldier.GetSoldierObj calls PlayerControl.GetSkinEntityByType for the player camp and GetSkinEntityByRandomType for other camps; SoldierConfig.EntityID is not the ordinary skin binding.',evidence=['generated/model-binding/Soldier-GetSoldierObj.txt:36']),
       dict(id='player-selected-skin',status='confirmed',rule='SkinManager.GetUserSkinId(type) -> SkinConfig.prefabId; missing config fallback type*1000. A game-mode bool at controller offset64 adds3000. Actual saved skin/account state has not been read.',evidence=['generated/model-binding/PlayerControl-GetSkinEntityByType.txt:44']),
       dict(id='enemy-skin',status='confirmed',rule='Types1/2/3 use LevelControl enemy skin arrays at84/88/92, indexed clamp(camp,2,4)-2; skinId=100/200/300 + selected offset. Missing config falls back to type*1000; same game-mode flag may add3000.',evidence=['generated/model-binding/PlayerControl-GetSkinEntityByRandomType.txt:31']),
       dict(id='baseline-skins',status='confirmed',rule='Explicit controlled loadout may choose free, unlocked skin100/200/300; this is a fixture choice, not evidence of original current account selection.',bindings=bindings,evidence=['generated/tables/SkinConfig.json','generated/tables/EntityModelConfig.json'])]
report=dict(target=dict(appId='wxcf1394487200e48f',version='43'),facts=facts,
            unknowns=[dict(item='Original selected skins/random seed and +3000 mode flag',verification='Observe original running loadout and resolve GameControl flag; do not infer from cache membership.')])
(R/'generated/model-binding-evidence.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Recovered five method bodies and dynamic default-skin bindings.')
