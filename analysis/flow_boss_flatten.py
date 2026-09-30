from pathlib import Path
source=Path('analysis/asset_incremental_flatten.py').read_text(encoding='utf8').replace('hud-soldier300-20260928','boss-entities-20260928')
source=source.replace("targets=['guideui','proj_xqzdpauseui','soldier_300']","targets=['boss01','boss02','bosssoldier1','bosssoldier2']")
exec(compile(source,'projectile_flatten','exec'))
