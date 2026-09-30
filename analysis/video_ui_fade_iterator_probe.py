"""Read-only source map for the commander / retry presentation audit."""
import contextlib, io, json, runpy, struct
class Sink(io.StringIO):
    def reconfigure(self, **kwargs): pass
with contextlib.redirect_stdout(Sink()):
    mapped = runpy.run_path('analysis/flow_all_module_map.py')
n = mapped['n']
import struct
usage=struct.unpack_from('<I',n['memory'],3949220)[0];typeIndex=(usage&0x1fffffff)>>1
cls=n['typenames'][typeIndex]
selected=[m for m in mapped['result'] if m['cls']==cls]

Path=None
from pathlib import Path
Path('analysis/video-ui-close-map.json').write_text(json.dumps(selected,indent=2))
print(json.dumps(selected,indent=2))

import struct
for cls in ['BaseUI']:
 t=next(t for t in n['ts'] if n['ms'](t[0])==cls)
 print(cls,[(i-t[8],n['ms'](struct.unpack_from('<iii',n['meta'],n['pairs'][11][0]+i*12)[0])) for i in range(t[8],t[8]+t[18])])
