"""Reuse reviewed downloader for the catalog-listed failure UI and its manifest closure."""
from pathlib import Path
s=Path('analysis/asset_incremental.py').read_text(encoding='utf8')
s=s.replace('hud-soldier300-20260928','result-ui-20260928')
s=s.replace("roots=['ui/mainmenu/guideui.prefab.unity3d','ui/mainmenu/proj_xqzdpauseui.prefab.unity3d','model/soldiercommon/soldier_300.prefab.unity3d']","roots=['ui/mainmenu/proj_xqzdfailui.prefab.unity3d']")
s=s.replace("'rootCount':3","'rootCount':len(roots)")
s=s.replace('parent explicitly scoped these three roots','parent explicitly scoped in-level result UI')
exec(compile(s,'result_ui_acquire','exec'))
