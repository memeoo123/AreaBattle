"""Read-only source map for the commander / retry presentation audit."""
import contextlib, io, json, runpy, struct
class Sink(io.StringIO):
    def reconfigure(self, **kwargs): pass
with contextlib.redirect_stdout(Sink()):
    mapped = runpy.run_path('analysis/flow_all_module_map.py')
n = mapped['n']
selected = [m for m in mapped['result'] if 'ObjectAnim' in m['cls'] or (m['cls'] in ['UIModule','UIUtils'] and (m['method']=='.cctor' or 'Anim' in m['method']))]
Path=None
from pathlib import Path
Path('analysis/video-ui-close-map.json').write_text(json.dumps(selected,indent=2))
print(json.dumps(selected,indent=2))

import struct
for cls in ['BaseUI']:
 t=next(t for t in n['ts'] if n['ms'](t[0])==cls)
 print(cls,[(i-t[8],n['ms'](struct.unpack_from('<iii',n['meta'],n['pairs'][11][0]+i*12)[0])) for i in range(t[8],t[8]+t[18])])
