"""Export isolated obstacle dependency evidence, preserving baseline files."""
from pathlib import Path
source=Path('analysis/asset_incremental_export.py').read_text(encoding='utf-8')
source=source.replace('hud-soldier300-20260928','obstacle-visuals-20260928')
exec(compile(source,'obstacle_snapshot_export','exec'))
