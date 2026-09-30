"""Recover the complete source Assembly-CSharp manager registration roster (metadata v31)."""
from pathlib import Path
import struct,json,hashlib
p=Path(__file__).resolve().parent/'targets/wxcf1394487200e48f/43'
b=(p/'work/webdata/Il2CppData/Metadata/global-metadata.dat').read_bytes();mem=(p/'generated/wasm/wasmcode.static-memory.bin').read_bytes()
assert struct.unpack_from('<II',b)==(0xfab11baf,31)
pairs=[struct.unpack_from('<II',b,8+i*8) for i in range(31)];u=lambda a:struct.unpack_from('<I',mem,a)[0]
ts=[struct.unpack_from('<16i8H2I',b,pairs[19][0]+i*88) for i in range(pairs[19][1]//88)]
md=[struct.unpack_from('<7i4H',b,pairs[5][0]+i*36) for i in range(pairs[5][1]//36)]
ms=lambda off:b[pairs[2][0]+off:b.index(0,pairs[2][0]+off)].decode('utf8')
ims=[struct.unpack_from('<10i',b,pairs[20][0]+i*40) for i in range(pairs[20][1]//40)];im=next(x for x in ims if ms(x[0])=='Assembly-CSharp.dll')
attrs={};start,count=im[-2:]
for i in range(start,start+count):
 tok,off=struct.unpack_from('<II',b,pairs[25][0]+i*8);end=struct.unpack_from('<II',b,pairs[25][0]+(i+1)*8)[1];attrs[tok]=(i,off,end)
def parent(i):
 a=ts[i][4]
 if a<0:return None
 data,bits=struct.unpack_from('<II',mem,u(200288+4*a));return data if bits>>16&255==18 else None
def chain(i):
 seen=[]
 while i is not None and i not in seen:
  seen.append(i);i=parent(i)
 return seen
rows=[]
for i in range(im[2],im[2]+im[3]):
 ancestors=chain(i)
 if 3465 not in ancestors:continue
 attr=None;x=attrs.get(ts[i][-1])
 if x:
  raw=b[pairs[24][0]+x[1]:pairs[24][0]+x[2]]
  # Exact supported ModelRegister constructor shape: one attr, three args, no named args.
  assert raw[:8]==b'\x01'+struct.pack('<I',26704)+b'\x03\x00\x00', (i,raw.hex())
  assert raw[8]==14 and raw[9]<128 and not raw[9]&1
  size=raw[9]>>1;name=raw[10:10+size].decode('utf8');tail=raw[10+size:]
  assert len(tail)==4 and tail[0]==2 and tail[2]==2 and tail[1] in (0,1) and tail[3] in (0,1)
  attr=dict(gameName=name,autoSyn=bool(tail[1]),compressData=bool(tail[3]),attributeRangeIndex=x[0],dataOffset=x[1],dataEnd=x[2],rawHex=raw.hex(),constructorMetadata=26704)
 methods=[dict(metadata=j,name=ms(m[0]),slot=m[-2]) for j,m in enumerate(md) if m[1]==i and ms(m[0]) in ('get_DataKey','OnInit','OnSave','OnRelease','.ctor')]
 rows.append(dict(typeIndex=i,name=ms(ts[i][0]),token=hex(ts[i][-1]),parentChain=ancestors,registration=attr,methods=methods))
out=dict(metadataVersion=31,metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),sourceImage='Assembly-CSharp.dll',sourceRegistration='disassembly/Type3467-26738.txt',sourceAttribute='disassembly/Type3464-26704.txt',managers=rows,qualification='Direct custom attributes decoded from complete Assembly-CSharp DataManagerBase inheritance inventory; registration selected by source game-name equality. No inferred activation of unmarked classes.')
(p/'generated/outgame/data-manager-registration-roster.json').write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print(json.dumps({'derivedManagers':len(rows),'registeredByGame':{g:sum(r['registration'] is not None and r['registration']['gameName']==g for r in rows) for g in sorted({r['registration']['gameName'] for r in rows if r['registration']})}}))

runtime={'registrations':[dict(sourceTypeIndex=r['typeIndex'],sourceClass=r['name'],gameName=r['registration']['gameName'],autoSyn=r['registration']['autoSyn'],compressData=r['registration']['compressData']) for r in rows if r['registration']]}
destination=Path(__file__).resolve().parent.parent/'UnityProject/Assets/AreaBattle/Resources/Data/OutgameManagerRegistration.json'
destination.write_text(json.dumps(runtime,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
