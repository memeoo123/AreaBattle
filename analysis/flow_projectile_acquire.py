from pathlib import Path
source=Path('analysis/asset_incremental.py').read_text(encoding='utf8').replace('hud-soldier300-20260928','arrow-projectile-20260928')
source=source.replace("roots=['ui/mainmenu/guideui.prefab.unity3d','ui/mainmenu/proj_xqzdpauseui.prefab.unity3d','model/soldiercommon/soldier_300.prefab.unity3d']", "roots=['effect/scene/hdzd_eff_spheretrails.prefab.unity3d']")
source=source.replace('these three roots','the ArrowTower projectile root').replace("'rootCount':3","'rootCount':len(roots)")
exec(compile(source,'projectile_acquire','exec'))
