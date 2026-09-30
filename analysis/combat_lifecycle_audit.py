"""Bounded WASM reference audit. Static bytes and metadata only; no target execution."""
import json, struct, bisect, runpy, contextlib, io
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
OUT=ROOT/'generated'
class Sink(io.StringIO):
 def reconfigure(self,**kwargs):pass
with contextlib.redirect_stdout(Sink()):
 global_map=runpy.run_path(str(Path(__file__).with_name('flow_all_module_map.py')))['result']
names={}
for x in global_map:names.setdefault((x['module'],x['function']),[]).append(x)
ns={'__file__':str(Path(__file__).with_name('combat_disassemble.py'))};exec(Path(__file__).with_name('combat_disassemble.py').read_text(encoding='utf8').split('selected=')[0],ns)
R=ns['R'];ops=ns['ops']
def instructions(blob,body):
 r=R(blob,body['offset']);end=r.p+body['size']
 for _ in range(r.leb()):r.leb();r.p+=1
 result=[]
 while r.p<end:
  pos=r.p;op=blob[r.p];r.p+=1
  if op not in ops:raise ValueError((hex(pos),hex(op)))
  args=[]
  if op in (2,3,4):args=[r.leb(True)]
  elif op in (12,13,16,32,33,34,35,36,63,64):args=[r.leb()]
  elif op==14:args=[r.leb() for _ in range(r.leb()+1)]
  elif op==17:args=[r.leb(),r.leb()]
  elif 40<=op<=62:args=[r.leb(),r.leb()]
  elif op in (65,66):args=[r.leb(True)]
  elif op in (67,68):r.p+=4 if op==67 else 8
  result.append((pos,op,args))
 return result
def leb(n,signed=False):
 out=bytearray()
 while True:
  c=n&127;n>>=7
  done=(n==0 and (not signed or not(c&64))) or (signed and n==-1 and c&64)
  if not done:c|=128
  out.append(c)
  if done:return bytes(out)
queries=[('towerbuff-constructor-direct','wasmcode1',16,44849),('towerbuff-constructor-table',None,65,14444),('towerbuff-getter-table',None,65,15528),('async-runner-getter-direct','wasmcode',16,8244),('async-runner-getter-table',None,65,24062)]
for x in global_map:
 if x['cls']=='MonoBehaviour' and x['method']=='StopAllCoroutines':
  queries += [('stop-all-coroutines-direct',x['module'],16,x['function']),('stop-all-coroutines-table',None,65,x['table'])]
queries=list(dict.fromkeys(queries));results=[]
for module in ['wasmcode','wasmcode1']:
 blob=(OUT/f'wasm/{module}.wasm').read_bytes();mp=json.loads((ROOT/f'evidence/{module}-function-map.json').read_text());bodies=sorted((v['offset'],int(f),v) for f,v in mp['bodies'].items());starts=[x[0] for x in bodies];cache={}
 for name,which,opcode,value in queries:
  if which and which!=module:continue
  pattern=bytes([opcode])+leb(value,opcode==65);offset=0;hits=[];rejected=[]
  while True:
   offset=blob.find(pattern,offset)
   if offset<0:break
   at=offset;offset+=1;index=bisect.bisect_right(starts,at)-1
   if index<0:continue
   start,function,body=bodies[index]
   if at>=start+body['size']:continue
   if function not in cache:
    try:cache[function]=instructions(blob,body)
    except Exception as ex:cache[function]=str(ex)
   decoded=cache[function]
   if isinstance(decoded,str):rejected.append({'function':function,'offset':at,'parseError':decoded});continue
   if (at,opcode,[value]) in decoded:hits.append({'function':function,'instructionOffset':hex(at),'body':body,'names':names.get((module,function),[])})
  results.append({'query':name,'module':module,'opcode':opcode,'value':value,'verifiedInstructionHits':hits,'unparsedCandidates':rejected})
b=(ROOT/'work/webdata/Il2CppData/Metadata/global-metadata.dat').read_bytes();p=[struct.unpack_from('<II',b,8+8*i) for i in range(21)]
t=struct.unpack_from('<16i8H2I',b,p[19][0]+4026*88);encoded=0x20000000|(t[2]*2+1);memory=(OUT/'wasm/wasmcode.static-memory.bin').read_bytes();needle=struct.pack('<I',encoded)
usage=[i for i in range(0,len(memory)-3,4) if memory[i:i+4]==needle]
report={'target':{'appid':'wxcf1394487200e48f','version':'43'},'mode':'static-only','queries':results,'towerBuffTypeUsage':{'metadataType':4026,'byvalTypeIndex':t[2],'encodedUsage':hex(encoded),'alignedStaticMemoryHits':usage},'namedAwaiterMethods':[x for x in global_map if x['module']=='wasmcode' and x['function'] in [5995,15991,11853,5015,14594,15141,8244,20638,1111,1227,1637]],'limitations':['No direct-call hit does not prove absence of all reflection/generic/dynamic/inlined creation.','StopAllCoroutines callers require receiver inspection; names alone do not establish the async runner as receiver.']}
(OUT/'combat-lifecycle-xrefs.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'queries':[{k:x[k] for k in ['query','module']}|{'hits':len(x['verifiedInstructionHits']),'unparsed':len(x['unparsedCandidates'])} for x in results],'towerBuffUsageHits':len(usage)}))
