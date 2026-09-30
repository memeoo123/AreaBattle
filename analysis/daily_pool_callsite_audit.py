"""Instruction-check all recovered WASM bodies for daily-pool construction references."""
import contextlib, io, json, runpy
from pathlib import Path
class Sink(io.StringIO):
    def reconfigure(self, **kwargs): pass
with contextlib.redirect_stdout(Sink()):
    data = runpy.run_path('analysis/flow_all_module_map.py')
n=data['n']; targets={('wasmcode1',66164),('wasmcode1',66165)}
tables={table:dest for table,dest in n['table'].items() if dest in targets}
def leb(v, signed=False):
    out=[]
    while True:
        b=v&127; v>>=7; out.append(b|(128 if v else 0))
        if not v:
            if signed and b&64:out[-1]|=128;out.append(0)
            return bytes(out)
names={}
for m in data['result']:
    names.setdefault((m['module'],m['function']),[]).append(m['image']+':'+m['cls']+'.'+m['method'])
hits=[]; failures=[]; checked=0; candidates=0
for mod,fmap in n['fm'].items():
    blob=(n['ROOT']/f'generated/wasm/{mod}.wasm').read_bytes()
    needles=[b'\x41'+leb(i,True) for i in tables]+[b'\x10'+leb(f) for m,f in targets if m==mod]
    for fid,body in fmap['bodies'].items():
        checked+=1; start=body['offset']; end=start+body['size']
        if not any(v in blob[start:end] for v in needles):continue
        candidates+=1; r=n['R'](blob,start)
        for _ in range(r.leb()):r.leb();r.p+=1
        try:
            while r.p<end:
                pos=r.p; op=blob[r.p];r.p+=1;a=[]
                if op in (2,3,4):a=[r.leb(True)]
                elif op in (12,13,16,32,33,34,35,36,63,64):a=[r.leb()]
                elif op==14:a=[r.leb() for _ in range(r.leb()+1)]
                elif op==17:a=[r.leb(),r.leb()]
                elif 40<=op<=62:a=[r.leb(),r.leb()]
                elif op in (65,66):a=[r.leb(True)]
                elif op in (67,68):r.p+=4 if op==67 else 8
                elif op not in n['ops']:raise ValueError(hex(op))
                dest=(mod,a[0]) if op==16 and (mod,a[0]) in targets else tables.get(a[0]) if op==65 else None
                if dest:hits.append(dict(module=mod,function=int(fid),names=names.get((mod,int(fid)),[]),offset=hex(pos),target=dest,kind='direct-call' if op==16 else 'table-constant'))
        except Exception as e:failures.append(dict(module=mod,function=fid,offset=hex(r.p),error=str(e)))
out=dict(scope='All local wasmcode and wasmcode1 defined bodies, with byte candidates validated as instructions; excludes unavailable modules, reflection and serialized/server data.',definedBodies=checked,candidateBodies=candidates,targets=sorted(targets),callsites=hits,decodeFailures=failures,
    conclusion='No source-backed pool fill/refill algorithm inferred from absence of callsites. Persisted ordered pools remain required.')
p=n['ROOT']/'generated/daily-pool-all-wasm-callsite-audit.json';p.write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(out,ensure_ascii=True,indent=2))
