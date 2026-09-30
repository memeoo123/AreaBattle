"""Resolve ConfigMgr initialization calls to original concrete config metadata and payloads."""
from pathlib import Path
import struct,json,re,hashlib
p=Path('analysis/recover_outgame_manager_registry.py');n={'__file__':str(p.resolve())};exec(p.read_text(encoding='utf8').split('rows=[]')[0],n)
u,mem,ts,ms,md,b,pairs=[n[k] for k in ['u','mem','ts','ms','md','b','pairs']]
root=n['p'];out=root/'generated/outgame';source=out/'disassembly/Type3907-30413.txt';lines=source.read_text(encoding='utf8').splitlines()
rows=[]
for i,line in enumerate(lines):
 if 'call [1333]' not in line and 'call [3646]' not in line:continue
 addr=int(re.search(r'i32.const \[(\d+)\]',lines[i-2])[1]);encoded=u(addr);assert encoded>>29==6
 specIndex=(encoded&0x1ffffffe)>>1;spec=struct.unpack_from('<3i',mem,483008+12*specIndex);inst=u(72592+4*spec[2]);count,args=struct.unpack_from('<II',mem,inst);assert count==1
 typ=u(args);data,bits=struct.unpack_from('<II',mem,typ);assert bits>>16&255==18
 name=ms(ts[data][0]);kind='table' if spec[0]==26610 else 'value';assert spec[0] in [26610,26614]
 offset=12 if not rows else int(re.search(r'\[2, (\d+)\]',lines[i-3] if kind=='table' else lines[i+1])[1])
 fields=[]
 for j in range(ts[data][18]):
  fieldIndex=ts[data][8]+j;fname,ftype,token=struct.unpack_from('<3i',b,pairs[11][0]+12*fieldIndex);ptr=u(200288+4*ftype);fd,fb=struct.unpack_from('<II',mem,ptr)
  fields.append(dict(name=ms(fname),typeIndex=ftype,typeData=fd,typeCode=fb>>16&255,attributes=fb&65535,offset=u(u(3823136+4*data)+4*j)))
 payload=Path('UnityProject/Assets/AreaBattle/Resources/Recovered/FirstPack/Config')/(name+'.bytes');raw=payload.read_bytes();text=raw.decode('utf-8-sig');obj=json.loads(text[text.index('{'):]);values=obj['Datas'] if kind=='table' else obj
 rows.append(dict(order=len(rows),ownerFieldOffset=offset,usageAddress=addr,methodSpecIndex=specIndex,methodSpec=list(spec),method=ms(md[spec[0]][0]),sourceTypeIndex=data,sourceName=name,kind=kind,fields=fields,payload=payload.as_posix(),sha256=hashlib.sha256(raw).hexdigest(),rowCount=len(values) if kind=='table' else None,jsonFields=sorted({k for row in values for k in row} if kind=='table' else obj.keys())))
assert len(rows)==64 and len({r['sourceName'] for r in rows})==64
result=dict(source=str(source),sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),tables=sum(r['kind']=='table' for r in rows),values=sum(r['kind']=='value' for r in rows),rows=rows,initializeSemantics=['ConfigRead static bool0=true and AppSetting static bool39=false before repeat guard','When ConfigMgr bool8 is false, load ordered tables into existing dictionaries, then assign value configs','Call postprocessors30439,30406,30398,30426,30428 in order through30401','Set instance bool8=true only after postprocessors return'],remaining=['Exact row dictionary helper and UniqueID overrides','Typed runtime schemas and derived indexes','Modern resource routes and actual production Main/account composition'])
methodmap=json.loads((out/'method-map.json').read_text(encoding='utf8'))
for row in rows:
 row['uniqueIdMethods']=[dict(metadata=x['metadata'],path=x['path']) for x in methodmap if x['cls']==row['sourceName'] and x['method']=='get_UniqueID']
 row['payloadOnlyFields']=sorted(set(row['jsonFields'])-{f['name'] for f in row['fields']})
result['otherFirstPackConfigs']=sorted({p.stem for p in Path('UnityProject/Assets/AreaBattle/Resources/Recovered/FirstPack/Config').glob('*.bytes')}-{r['sourceName'] for r in rows})
result['payloadOnlyQualification']='Fields absent from directly declared metadata; inspect inheritance/stripping and active readers before interpreting as gameplay.'
(out/'CONFIG_INITIALIZATION_ROSTER.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf8');print(json.dumps(dict(tables=result['tables'],values=result['values'],rows=sum(r['rowCount'] or 0 for r in rows))))


