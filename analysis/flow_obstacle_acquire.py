"""Minimum manifest closure for the 15 already-confirmed obstacle prefab roots."""
from pathlib import Path
source=Path('analysis/asset_incremental.py').read_text(encoding='utf-8')
source=source.replace('hud-soldier300-20260928','obstacle-visuals-20260928')
source=source.replace("roots=['ui/mainmenu/guideui.prefab.unity3d','ui/mainmenu/proj_xqzdpauseui.prefab.unity3d','model/soldiercommon/soldier_300.prefab.unity3d']", "roots=['model/entity/'+x['name'].lower()+'.prefab.unity3d' for x in json.loads((ROOT/'evidence/obstacle-downloads.json').read_text(encoding='utf8'))]")
source=source.replace('these three roots','these fifteen obstacle roots').replace("'rootCount':3","'rootCount':len(roots)")
exec(compile(source,'obstacle_snapshot_acquire','exec'))
