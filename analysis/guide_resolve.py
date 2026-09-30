"""Resolve guide helper identities from original codegen module tables."""
import json,struct
from pathlib import Path
base=Path('analysis/controls_disassemble.py').read_text(encoding='utf8')
ctx={'__file__':str(Path('analysis/controls_disassemble.py').resolve())}
exec(compile(base.split('control_type=')[0],'metadata', 'exec'),ctx)
memory,meta,pairs,md,ts,ms,table=[ctx[k] for k in ('memory','meta','pairs','md','ts','ms','table')]
for t in ts:
    if ms(t[0])=='Vector3':
        print('Vector3 fields',[(ms(struct.unpack_from('<iii',meta,pairs[11][0]+j*12)[0])) for j in range(t[8],t[8]+t[18])])
for a in (1904810,):print(a,memory[a:memory.index(0,a)].decode())
images=[struct.unpack_from('<10i',meta,pairs[20][0]+i*40) for i in range(pairs[20][1]//40)]
wanted={949,10103,7025,7029,1348,3531,5573}
delayParents={t[2] for t in ts if ms(t[0])=='TimeHelper'}
for im in images:
    name=ms(im[0]);start=memory.find(name.encode()+b'\0')
    if start<0:continue
    at=0;candidates=[]
    while (at:=memory.find(struct.pack('<I',start),at))>=0:
        if at%4==0 and at+12<len(memory):
            _,count,ptr=struct.unpack_from('<III',memory,at)
            if 1<count<200000 and ptr>0 and ptr+count*4<len(memory):
                values=struct.unpack_from('<'+str(count)+'I',memory,ptr)
                if max(values)<1000000:candidates.append((count,values))
        at+=1
    for count,values in candidates:
        for m in md:
            if not im[2]<=m[1]<im[2]+im[3]:continue
            rid=m[6]&0xffffff
            if not 1<=rid<=count:continue
            ptr=values[rid-1];loc=table.get(ptr)
            if loc and loc[0]=='wasmcode' and (loc[1] in wanted or ptr==2416 or ts[m[1]][3] in delayParents or ms(ts[m[1]][0]).startswith('U¥') or (ms(ts[m[1]][0])=='Vector3' and ms(m[0])=='.cctor') or (ms(ts[m[1]][0])=='TimeModule' and ms(m[0]) in ('Update','OnUpdate'))):
                print(name,ms(ts[m[1]][0]),ms(m[0]),m[6],loc,ptr)
                if 'Timer' in ms(m[0]):
                    print('method tuple',m)
                    for j in range(m[4],m[4]+m[10]):
                        p=struct.unpack_from('<iii',meta,pairs[10][0]+j*12)
                        print('param',j,p,ms(p[0]))
