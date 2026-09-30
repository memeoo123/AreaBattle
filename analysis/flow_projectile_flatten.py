from pathlib import Path
source=Path('analysis/asset_incremental_flatten.py').read_text(encoding='utf8').replace('hud-soldier300-20260928','arrow-projectile-20260928')
source=source.replace("targets=['guideui','proj_xqzdpauseui','soldier_300']","targets=['hdzd_eff_spheretrails']")
exec(compile(source,'projectile_flatten','exec'))
