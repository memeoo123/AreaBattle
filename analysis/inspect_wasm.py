"""Static WebAssembly section/name/data inspection; no instantiation."""
import json
import struct
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
class Reader:
    def __init__(self,b,pos=0): self.b,self.p=b,pos
    def u(self):
        v,s=0,0
        while True:
            c=self.b[self.p];self.p+=1;v|=(c&127)<<s
            if not c&128:return v
            s+=7
            assert s<70
    def string(self):
        n=self.u();s=self.b[self.p:self.p+n].decode('utf-8');self.p+=n;return s
reports=[]
for name in ['wasmcode','wasmcode1']:
    b=(ROOT/f'generated/wasm/{name}.wasm').read_bytes()
    r=Reader(b,8)
    sections=[];names={};segments=[]
    while r.p<len(b):
        sid=r.u();size=r.u();start=r.p;end=start+size
        sections.append({'id':sid,'offset':start,'size':size})
        if sid==0 and r.string()=='name':
            while r.p<end:
                sub=r.u();length=r.u();subend=r.p+length
                if sub==1:
                    count=r.u()
                    for _ in range(count):
                        idx=r.u();label=r.string();names[idx]=label
                r.p=subend
        if sid==11:
            for i in range(r.u()):
                flags=r.u()
                assert flags in (0,2),flags
                memindex=r.u() if flags==2 else 0
                opcode=b[r.p];r.p+=1;assert opcode==0x41
                address=r.u();assert b[r.p]==0x0b;r.p+=1
                length=r.u();segments.append({'index':i,'address':address,'offset':r.p,'size':length});r.p+=length
        r.p=end
    if segments:
        memory=bytearray(max(x['address']+x['size'] for x in segments))
        for x in segments:memory[x['address']:x['address']+x['size']]=b[x['offset']:x['offset']+x['size']]
        (ROOT/f'generated/wasm/{name}.static-memory.bin').write_bytes(memory)
    report={'file':f'generated/wasm/{name}.wasm','sections':sections,'names':names,'dataSegments':segments}
    (ROOT/f'evidence/{name}-structure.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    hits={i:n for i,n in names.items() if any(x in n.lower() for x in ('spawn','tower','battlecontrol','soldiercollision'))}
    reports.append({'file':name,'nameCount':len(names),'sample':list(names.items())[:8],'interesting':dict(list(hits.items())[:10]),'dataSegments':len(segments)})
print(json.dumps(reports,indent=2))
