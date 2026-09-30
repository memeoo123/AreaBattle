"""Read-only source map for the commander / retry presentation audit."""
import contextlib, io, json, runpy, struct
class Sink(io.StringIO):
    def reconfigure(self, **kwargs): pass
with contextlib.redirect_stdout(Sink()):
    mapped = runpy.run_path('analysis/flow_all_module_map.py')
n = mapped['n']
selected = [m for m in mapped['result'] if ('closeUI' in m['cls'] or 'CloseAnim' in m['cls'] or m['cls']=='BaseUI')]
Path=None
from pathlib import Path
Path('analysis/video-ui-close-map.json').write_text(json.dumps(selected,indent=2))
print(json.dumps(selected,indent=2))
