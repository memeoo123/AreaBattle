"""Acquire/export the bounded source-contract skill effect dependency closure."""
from pathlib import Path
import sys
snapshot='skill-effects-20260928'
if '--flatten' in sys.argv:
    s=Path('analysis/asset_incremental_flatten.py').read_text(encoding='utf8').replace('hud-soldier300-20260928',snapshot)
    s=s.replace("targets=['guideui','proj_xqzdpauseui','soldier_300']","targets=[Path(x['logicalPath']).stem for x in json.loads((ROOT/'generated/skill-presentation-minimum-resources.json').read_text(encoding='utf8'))['resources']]")
    exec(compile(s,'skill_effect_flatten','exec'))
elif '--export' in sys.argv:
    s=Path('analysis/asset_incremental_export.py').read_text(encoding='utf8').replace('hud-soldier300-20260928',snapshot)
    exec(compile(s,'skill_effect_export','exec'))
else:
    s=Path('analysis/asset_incremental.py').read_text(encoding='utf8').replace('hud-soldier300-20260928',snapshot)
    s=s.replace("roots=['ui/mainmenu/guideui.prefab.unity3d','ui/mainmenu/proj_xqzdpauseui.prefab.unity3d','model/soldiercommon/soldier_300.prefab.unity3d']","roots=[x['catalog']['name'] for x in json.loads((ROOT/'generated/skill-presentation-minimum-resources.json').read_text(encoding='utf8'))['resources']]; assert len(roots)==21")
    s=s.replace("'rootCount':3","'rootCount':len(roots)").replace('parent explicitly scoped these three roots','parent explicitly scoped these 21 skill effect contract roots')
    s=s.replace('import json,sys,hashlib,urllib.request,argparse','import json,sys,hashlib,urllib.request,urllib.parse,argparse')
    s=s.replace("'url':BASE+item['file']","'url':BASE+urllib.parse.quote(item['file'],safe='/%')")
    exec(compile(s,'skill_effect_acquire','exec'))
