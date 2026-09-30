"""Inventory WASM bodies referencing AppSetting metadata and byte stores at field36."""
from pathlib import Path
import json,contextlib,io,runpy,hashlib
class Sink(io.StringIO):
 def reconfigure(self,**kw):pass
with contextlib.redirect_stdout(Sink()):env=runpy.run_path('analysis/flow_all_module_map.py')
root=env['n']['ROOT'];names={}
for m in env['result']:names.setdefault((m['module'],m['function']),[]).append(m)
def leb(v):
 result=[]
 while True:
  part=v&127;v>>=7
  if v==0 and part<64:result.append(part);return bytes(result)
  result.append(part|128)
const=b'\x41'+leb(3928176);store=b'\x3a\x00\x24';rows=[];hashes={};total=0
for module in ['wasmcode','wasmcode1']:
 blob=(root/f'generated/wasm/{module}.wasm').read_bytes();hashes[module]=hashlib.sha256(blob).hexdigest()
 mapping=json.loads((root/f'evidence/{module}-function-map.json').read_text())
 for fn,body in mapping['bodies'].items():
  total+=1;data=blob[body['offset']:body['offset']+body['size']]
  if const in data and store in data:rows.append({'module':module,'function':int(fn),'body':body,'methods':names.get((module,int(fn)),[])})
out={'scope':'Candidate byte-pattern inventory, requires decoded verification and does not exclude indirect/reflection writes','typeUsage':3928176,'fieldOffset':36,'moduleHashes':hashes,'bodiesScanned':total,'candidates':rows}
(root/'generated/outgame/resource-mode-writer-candidates.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8')
for row in rows:print(row['module'],row['function'],[(m['cls'],m['method'],m['metadata']) for m in row['methods']])
print('bodies',total,'candidates',len(rows))
