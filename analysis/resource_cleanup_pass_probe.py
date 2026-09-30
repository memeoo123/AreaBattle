"""Locate and inspect source metadata registration candidates for generic UI calls."""
import contextlib,io,runpy,struct,json,hashlib
class Sink(io.StringIO):
    def reconfigure(self,**kw):pass
with contextlib.redirect_stdout(Sink()):
    d=runpy.run_path('analysis/flow_all_module_map.py')
n=d['n'];mem=n['memory'];u=lambda at:struct.unpack_from('<I',mem,at)[0]
print('typeCount',len(n['ts']),'memory',len(mem))
candidates=[]
needle=struct.pack('<I',len(n['ts']));pos=0
while (pos:=mem.find(needle,pos))>=0:
    if pos%4==0 and pos>=40 and pos+24<len(mem) and u(pos+8)==len(n['ts']):
        start=pos-40;row=struct.unpack_from('<16I',mem,start)
        if all(0<row[i]<len(mem) for i in [1,3,5,7,9,11,13]):candidates.append((start,row))
    pos+=1
print('registrationCandidates',candidates)
assert len(candidates)==1,'Metadata registration must resolve uniquely'
start,row=candidates[0]
for i in range(row[8]):
    mdIndex,classInst,methodInst=struct.unpack_from('<3i',mem,row[9]+i*12)
    assert 0<=mdIndex<len(n['md']) and -1<=classInst<row[2] and -1<=methodInst<row[2]
resolved=[]
for at in [3961104,3978900,3978908,3978912]:
    v=u(at);print('usage',at,hex(v),'kind',v>>29,'index',(v&0x1ffffffe)>>1)
for start,row in candidates:
    for at in [3961104,3978900,3978908,3978912]:
        v=u(at);idx=(v&0x1ffffffe)>>1
        if v>>29!=6 or idx>=row[8]:continue
        spec=struct.unpack_from('<3i',mem,row[9]+12*idx)
        md=n['md'][spec[0]];cls=n['ms'](n['ts'][md[1]][0]);method=n['ms'](md[0])
        args=[]
        for inst in spec[1:]:
            if inst<0:args.append(None);continue
            ptr=u(row[3]+4*inst);cnt,types=struct.unpack_from('<II',mem,ptr)
            vals=[]
            for i in range(cnt):
                typ=u(types+4*i);data,bits=struct.unpack_from('<II',mem,typ)
                vals.append(dict(type=hex(bits),data=data,name=n['ms'](n['ts'][data][0]) if data<len(n['ts']) else None))
            args.append(vals)
        print('resolved',at,cls,method,spec,args)
        resolved.append(dict(usageAddress=at,encodedUsage=hex(v),methodSpecIndex=idx,
                             methodSpecAddress=row[9]+12*idx,methodSpec=list(spec),
                             declaringType=cls,method=method,genericArguments=args))
out=dict(target=dict(appId='wxcf1394487200e48f',version='43'),
         source='generated/wasm/wasmcode.static-memory.bin',sha256=hashlib.sha256(mem).hexdigest(),
         registrationAddress=start,registrationWords=list(row),
         validation=dict(uniqueRegistration=True,methodSpecsBoundsChecked=row[8],typeDefinitions=len(n['ts'])),
         calls=resolved,qualification='Read-only static IL2CPP metadata resolution; no target code execution or UI timing inference.')
(n['ROOT']/'generated/outgame/resource-cleanup-pass-generics.json').write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

