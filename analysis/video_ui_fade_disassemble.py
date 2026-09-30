import contextlib,io,runpy,re,sys,json
from pathlib import Path
class Sink(io.StringIO):
 def reconfigure(self,**kw):pass
with contextlib.redirect_stdout(Sink()):d=runpy.run_path('analysis/flow_all_module_map.py')
byfn={(m['module'],m['function']):m for m in d['result']}
dest=Path('analysis/targets/wxcf1394487200e48f/43/generated/video-20260928/ui-close-source');dest.mkdir(exist_ok=True)
for fn in [15045,9777]:
 sys.argv=['flow_disassemble.py','@'+str(fn)]
 with contextlib.redirect_stdout(Sink()) as out:runpy.run_path('analysis/flow_disassemble.py')
 s=out.getvalue()
 def name(m):
  row=byfn.get(('wasmcode',int(m[1])));return m[0].split(' ;')[0]+' ; '+(row['cls']+'.'+row['method'] if row else '')
 s=re.sub(r'call \[(\d+)\] ; [^\n]*',name,s)
 s=re.sub(r'i32.const \[(\d+)\]',lambda m:m[0]+' ; TABLE '+next((v['cls']+'.'+v['method'] for v in d['result'] if v['table']==int(m[1])),''),s)
 s=re.sub(r'  +',' ',s)
 (dest/f'{fn}.txt').write_text(s,encoding='utf-8')
 print('saved',fn)

