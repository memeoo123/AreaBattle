"""Annotate local skill disassemblies with original strings and cross-module method names."""
import json, struct, re, runpy, contextlib, io
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43';OUT=ROOT/'generated'
class Sink(io.StringIO):
 def reconfigure(self,**kwargs):pass
with contextlib.redirect_stdout(Sink()): gm=runpy.run_path(str(Path(__file__).with_name('flow_all_module_map.py')))['result']
bytable={x['table']:x for x in gm};bymethod={x['metadata']:x for x in gm};byfn={(x['module'],x['function']):x for x in gm}
b=(ROOT/'work/webdata/Il2CppData/Metadata/global-metadata.dat').read_bytes();pairs=[struct.unpack_from('<II',b,8+8*i) for i in range(21)]
mem=(OUT/'wasm/wasmcode.static-memory.bin').read_bytes()
def annotation(a):
 if 3900000<=a<4200000:
  u=struct.unpack_from('<I',mem,a)[0];kind=u>>29;idx=(u&0x1fffffff)>>1
  if kind==5:
   le,off=struct.unpack_from('<II',b,pairs[0][0]+idx*8)
   return ' STRING '+json.dumps(b[pairs[1][0]+off:pairs[1][0]+off+le].decode('utf8'),ensure_ascii=False)
  if kind==3 and idx in bymethod:
   m=bymethod[idx];return ' METHOD '+m['cls']+'.'+m['method']
 if 1000<a<100000 and a in bytable:
  m=bytable[a];return ' TABLE '+m['cls']+'.'+m['method']
 return ''
dest=OUT/'combat-presentation-disassembly';dest.mkdir(exist_ok=True)
selected=[]
for p in (OUT/'combat-disassembly').glob('*.txt'):
 match=re.match(r'Type(\d+)-',p.name)
 if match and (3406<=int(match[1])<=3409 or 3470<=int(match[1])<=3473 or 4166<=int(match[1])<=4205) and 'annotated' not in p.name:selected.append(p)
 elif p.name.startswith(('Tower-','EffectID-','Raw-7935','Raw-4443','Soldier-','Bullet-','SkillBase-')):selected.append(p)
for p in selected:
 s=p.read_text(encoding='utf8');mod='wasmcode1' if '; wasmcode1 function' in s else 'wasmcode'
 s=re.sub(r'i32.const \[(\d+)\]',lambda m:m[0]+(' ;'+annotation(int(m[1])) if annotation(int(m[1])) else ''),s)
 s=re.sub(r'call \[(\d+)\] ; [^\n]*',lambda m:m[0].split(' ;')[0]+' ; '+(byfn[(mod,int(m[1]))]['cls']+'.'+byfn[(mod,int(m[1]))]['method'] if (mod,int(m[1])) in byfn else ''),s)
 (dest/p.name).write_text(s,encoding='utf8')
print(json.dumps({'files':len(selected)}))
