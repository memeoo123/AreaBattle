from pathlib import Path
source=Path('analysis/asset_incremental_flatten.py').read_text(encoding='utf-8')
source=source.replace('hud-soldier300-20260928','obstacle-visuals-20260928')
source=source.replace("targets=['guideui','proj_xqzdpauseui','soldier_300']", "targets=[x['name'].lower() for x in json.loads((ROOT/'evidence/obstacle-downloads.json').read_text(encoding='utf8'))]")
exec(compile(source,'obstacle_snapshot_flatten','exec'))
