"""Static, instruction-aware callsite search over recovered Assembly-CSharp bodies."""
import sys,runpy,struct,json
sys.argv=['x','NO'];n=runpy.run_path('analysis/flow_disassemble.py')
targets={int(x) for x in sys.argv[2:]} if len(sys.argv)>2 else {1164,5934,5935,6188,7631,11600}
tables={m['tableIndex']:m['function'] for m in n['methods'] if m.get('module')=='wasmcode' and m.get('function') in targets}
blobs={k:(n['ROOT']/f'generated/wasm/{k}.wasm').read_bytes() for k in ['wasmcode','wasmcode1']}
result=[]
for m in n['methods']:
 if 'body' not in m:continue
 b=blobs[m['module']];r=n['R'](b,m['body']['offset']);end=r.p+m['body']['size']
 for _ in range(r.leb()):r.leb();r.p+=1
 history=[]
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
  history.append([hex(pos),n['ops'][op],a]);history=history[-18:]
  target=a[1] if op in (54,58) and a[1] in (24,56,60) else None
  if target is not None:result.append({'class':m['class'],'method':m['method'],'function':m['function'],'module':m['module'],'callsite':hex(pos),'target':target,'kind':n['ops'][op],'context':list(history)})
print(json.dumps(result,ensure_ascii=False,indent=2))
