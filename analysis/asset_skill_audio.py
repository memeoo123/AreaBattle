"""Acquire only statically confirmed in-level skill audio with catalog dependency closure."""
import json,sys
from pathlib import Path
R=Path('analysis/targets/wxcf1394487200e48f/43')
ids={2016,2018,2021,2026,2027,2028,2029,2030,2031,2032,2033,2034,2035,2036,2037,2038,2039,2040,2041,2042,2043,3121,3122,4401,4501}
rows=json.loads((R/'generated/tables/AudioConfig.json').read_text(encoding='utf8'))['Datas']
roots=[r['ResPath'].lower()+'.unity3d' for r in rows if r['id'] in ids]
assert len(roots)==len(ids)
if '--export' in sys.argv:
 source=Path('analysis/asset_incremental_export.py').read_text(encoding='utf8').replace('hud-soldier300-20260928','skill-audio-20260928')
else:
 source=Path('analysis/asset_incremental.py').read_text(encoding='utf8').replace('hud-soldier300-20260928','skill-audio-20260928')
 source=source.replace("roots=['ui/mainmenu/guideui.prefab.unity3d','ui/mainmenu/proj_xqzdpauseui.prefab.unity3d','model/soldiercommon/soldier_300.prefab.unity3d']",'roots='+repr(roots))
 source=source.replace("'rootCount':3","'rootCount':len(roots)").replace('parent explicitly scoped these three roots','parent scoped the25 statically confirmed in-level skill audio IDs')
exec(compile(source,'skill_audio_snapshot','exec'))
