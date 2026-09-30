from pathlib import Path
source=Path('analysis/asset_incremental_export.py').read_text(encoding='utf8').replace('hud-soldier300-20260928','arrow-projectile-20260928')
exec(compile(source,'projectile_export','exec'))
