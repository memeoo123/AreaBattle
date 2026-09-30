"""Export the failure UI snapshot without modifying either earlier resource snapshot."""
from pathlib import Path
for script in ('asset_incremental_export.py','asset_incremental_flatten.py'):
 s=(Path('analysis')/script).read_text(encoding='utf8').replace('hud-soldier300-20260928','result-ui-20260928')
 s=s.replace("targets=['guideui','proj_xqzdpauseui','soldier_300']","targets=['proj_xqzdfailui']")
 exec(compile(s,'result_'+script,'exec'))
