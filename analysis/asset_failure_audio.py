"""Acquire/export only catalog AudioConfig 2010 and its original dependency closure."""
from pathlib import Path
import sys

snapshot='failure-audio-2010-20260928'
if '--export' in sys.argv:
    s=Path('analysis/asset_incremental_export.py').read_text(encoding='utf8')
    s=s.replace('hud-soldier300-20260928',snapshot)
    exec(compile(s,'failure_audio_export','exec'))
else:
    s=Path('analysis/asset_incremental.py').read_text(encoding='utf8')
    s=s.replace('hud-soldier300-20260928',snapshot)
    s=s.replace("roots=['ui/mainmenu/guideui.prefab.unity3d','ui/mainmenu/proj_xqzdpauseui.prefab.unity3d','model/soldiercommon/soldier_300.prefab.unity3d']","roots=['audio/once/ui/failui_fail.mp3.unity3d']")
    s=s.replace("'rootCount':3","'rootCount':len(roots)")
    s=s.replace('parent explicitly scoped these three roots','parent explicitly scoped failure result audio 2010')
    exec(compile(s,'failure_audio_acquire','exec'))
