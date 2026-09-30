"""Read-only source map for the commander / retry presentation audit."""
import contextlib, io, json, runpy, struct
class Sink(io.StringIO):
    def reconfigure(self, **kwargs): pass
with contextlib.redirect_stdout(Sink()):
    mapped = runpy.run_path('analysis/flow_all_module_map.py')
n = mapped['n']
selected = [m for m in mapped['result'] if
    (m['module'] == 'wasmcode' and m['function'] in [13291,2622,1032,1877,1091]) or
    (m['cls'] in ['Proj_xqzdOverUI','Proj_xqzdFailUI','Event'] and m['method'] in ['.ctor','.cctor']) or
    (m['module'] == 'wasmcode1' and m['function'] in [66164,66165])]
print(json.dumps(selected, indent=2, ensure_ascii=True))
for cls in ['Event']:
    t = next(t for t in n['ts'] if n['ms'](t[0]) == cls)
    fields = [struct.unpack_from('<iii', n['meta'], n['pairs'][11][0] + i*12) for i in range(t[8], t[8]+t[18])]
    print(cls, [(i,n['ms'](f[0])) for i,f in enumerate(fields)])
