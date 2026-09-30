"""Resolve IL2CPP methods through static CodeGenModule pointers and WASM tables."""
import json
import struct
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
class R:
    def __init__(self,b,p=0):self.b,self.p=b,p
    def u(self):
        v=s=0
        while True:
            c=self.b[self.p];self.p+=1;v|=(c&127)<<s
            if c<128:return v
            s+=7
    def text(self):
        n=self.u();v=self.b[self.p:self.p+n].decode();self.p+=n;return v
    def byte(self):v=self.b[self.p];self.p+=1;return v
    def limits(self):
        flag=self.u();self.u()
        if flag&1:self.u()
    def offset(self):
        op=self.byte();value=self.u();assert self.byte()==11
        return {'opcode':op,'value':value}
maps={}
for name in ['wasmcode','wasmcode1']:
    b=(ROOT/f'generated/wasm/{name}.wasm').read_bytes();r=R(b,8);imports=[];elems=[];bodies={};types=[];signatures=[]
    while r.p<len(b):
        sid=r.u();size=r.u();end=r.p+size
        if sid==1:
            for _ in range(r.u()):
                assert r.byte()==96
                args=[r.byte() for _ in range(r.u())];rets=[r.byte() for _ in range(r.u())]
                signatures.append({'args':args,'returns':rets})
        if sid==2:
            for _ in range(r.u()):
                module,label,kind=r.text(),r.text(),r.byte()
                if kind==0:imports.append({'module':module,'name':label,'type':r.u()})
                elif kind==1:r.byte();r.limits()
                elif kind==2:r.limits()
                elif kind==3:r.byte();r.byte()
                else:raise ValueError(kind)
        if sid==3:types=[r.u() for _ in range(r.u())]
        if sid==9:
            for _ in range(r.u()):
                flag=r.u();assert flag in (0,1,2,3),flag
                table=r.u() if flag==2 else 0
                offset=r.offset() if flag in (0,2) else None
                if flag!=0:assert r.byte()==0
                funcs=[r.u() for _ in range(r.u())]
                elems.append({'flag':flag,'table':table,'offset':offset,'functions':funcs})
        if sid==10:
            for i in range(r.u()):
                size=r.u();off=r.p;r.p+=size
                bodies[i+len(imports)]={'offset':off,'size':size,'typeIndex':types[i]}
        r.p=end
    maps[name]={'imports':imports,'elements':elems,'bodies':bodies,'signatures':signatures}
    (ROOT/f'evidence/{name}-function-map.json').write_text(json.dumps(maps[name],indent=2),encoding='utf-8')
memory=(ROOT/'generated/wasm/wasmcode.static-memory.bin').read_bytes()
needle=b'Assembly-CSharp.dll\x00';starts=[];at=0
while (at:=memory.find(needle,at))>=0:starts.append(at);at+=1
candidates=[]
for start in starts:
    pointer=struct.pack('<I',start);at=0
    while (at:=memory.find(pointer,at))>=0:
        if at%4==0 and at+12<len(memory):
            p,count,ptr=struct.unpack_from('<III',memory,at)
            if 100<count<100000 and ptr>0 and ptr+count*4<len(memory):
                values=struct.unpack_from('<'+str(count)+'I',memory,ptr)
                if max(values)<1000000:candidates.append({'offset':at,'nameAddress':start,'methodCount':count,'methodPointersAddress':ptr})
        at+=1
assert len(candidates)==1,candidates
module=candidates[0]
symbols=json.loads((ROOT/'generated/gameplay-symbols.json').read_text(encoding='utf-8'))
table={}
for name,m in maps.items():
    for e in m['elements']:
        if e['offset'] and e['offset']['opcode']==65:
            for i,f in enumerate(e['functions']):table[e['offset']['value']+i]={'module':name,'function':f}
resolved=[]
for t in symbols['types']:
    if t['assembly']!='Assembly-CSharp.dll':continue
    for f in t['methods']:
        rid=f['token']&0xffffff
        if not 1<=rid<=module['methodCount']:continue
        ptrAddress=module['methodPointersAddress']+(rid-1)*4
        pointer=struct.unpack_from('<I',memory,ptrAddress)[0]
        row={'class':t['name'],'method':f['name'],'token':f['token'],'metadataOffset':f['offset'],'pointerAddress':ptrAddress,'tableIndex':pointer}
        if pointer in table:
            row.update(table[pointer]);m=maps[row['module']]
            if row['function'] in m['bodies']:
                row['body']=m['bodies'][row['function']]
                row['signature']=m['signatures'][row['body']['typeIndex']]
            else:row['import']=m['imports'][row['function']]
        resolved.append(row)
result={'status':'statically-resolved-through-module-and-element-table; runtime-not-executed','codeGenModule':module,'methods':resolved}
(ROOT/'generated/gameplay-method-map.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
interesting=[r for r in resolved if r['class'] in ('BattleControl','Tower','Soldier') and r['method'] in ('GetSpawnTime','GetDispatchLineNum','GetDispatchAddScoreTime','SoldierCollision','ChangeScore','get_SpawnTime','RefreshTowerGrade')]
print(json.dumps({'module':module,'mappedMethods':len(resolved),'withBody':sum('body' in x for x in resolved),'tableSegments':{n:[{'offset':e['offset'],'count':len(e['functions'])} for e in m['elements']][:3] for n,m in maps.items()},'interesting':interesting},ensure_ascii=False,indent=2))
