"""Instruction-aware bounded mapped Assembly-CSharp search for daily pool creation."""
import sys,runpy,json
sys.argv=['x','NO'];n=runpy.run_path('analysis/flow_disassemble.py')
targets={('wasmcode1',66164),('wasmcode1',66165)}
tables={key:dest for key,dest in n['table'].items() if dest in targets}
blobs={k:(n['ROOT']/f'generated/wasm/{k}.wasm').read_bytes() for k in ['wasmcode','wasmcode1']}
result=[]
for m in n['methods']:
 if 'body' not in m:continue
 b=blobs[m['module']];r=n['R'](b,m['body']['offset']);end=r.p+m['body']['size']
 for _ in range(r.leb()):r.leb();r.p+=1
 while r.p<end:
  pos=r.p;op=b[r.p];r.p+=1;a=[]
  if op in (2,3,4):a=[r.leb(True)]
  elif op in (12,13,16,32,33,34,35,36,63,64):a=[r.leb()]
  elif op==14:a=[r.leb() for _ in range(r.leb()+1)]
  elif op==17:a=[r.leb(),r.leb()]
  elif 40<=op<=62:a=[r.leb(),r.leb()]
  elif op in (65,66):a=[r.leb(True)]
  elif op in (67,68):r.p+=4 if op==67 else 8
  elif op not in n['ops']:raise Exception((m,hex(pos),op))
  dest=(m['module'],a[0]) if op==16 and (m['module'],a[0]) in targets else tables.get(a[0]) if op==65 else None
  if dest:result.append(dict(className=m['class'],method=m['method'],module=m['module'],function=m['function'],offset=hex(pos),target=dest,kind='call' if op==16 else 'table-constant'))
print(json.dumps(dict(scope='all recovered mapped Assembly-CSharp bodies; direct calls and table constants, not reflection/deserialization',targets=list(targets),callsites=result),ensure_ascii=False,indent=2))
