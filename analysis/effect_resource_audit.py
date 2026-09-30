"""Bounded async ownership disassembly; never executes target code."""
from pathlib import Path
import json,runpy,contextlib,io,re
class Sink(io.StringIO):
 def reconfigure(self,**kwargs):pass
base=Path(__file__).parent
with contextlib.redirect_stdout(Sink()):env=runpy.run_path(str(base/'combat_presentation_disassemble.py'))
p=base/'combat_disassemble.py';s=p.read_text(encoding='utf8');ns={'__file__':str(p)};exec(s.split('selected=')[0],ns)
ns['out']=env['OUT']/'effect-resource-disassembly';ns['out'].mkdir(exist_ok=True)
selected=[]
for m in env['gm']:
 if m['cls'] not in ['EffectModule','ResLoader','AssetResLoader','ResourcesModule','BaseEffect','AssetbundleModule','NewResLoadHelper']:continue
 mp=json.loads((env['ROOT']/f"evidence/{m['module']}-function-map.json").read_text());body=mp['bodies'][str(m['function'])]
 selected.append(dict(m,**{'class':m['cls'],'token':m['metadata'],'body':body,'signature':mp['signatures'][body['typeIndex']]}))
for fn in [13160]:
 mp=json.loads((env['ROOT']/"evidence/wasmcode-function-map.json").read_text());body=mp['bodies'][str(fn)]
 selected.append(dict(module='wasmcode',function=fn,method='generic-load',**{'class':'Raw','token':fn,'body':body,'signature':mp['signatures'][body['typeIndex']]}))
ns['selected']=selected
with contextlib.redirect_stdout(Sink()):exec(s[s.index('for m in selected:'):],ns)
for p in ns['out'].glob('*.txt'):
 t=p.read_text(encoding='utf8');mod='wasmcode1' if '; wasmcode1 function' in t else 'wasmcode'
 t=re.sub(r'i32.const \[(\d+)\]',lambda m:m[0]+(' ;'+env['annotation'](int(m[1])) if env['annotation'](int(m[1])) else ''),t)
 t=re.sub(r'call \[(\d+)\] ; [^\n]*',lambda m:m[0].split(' ;')[0]+' ; '+(env['byfn'][(mod,int(m[1]))]['cls']+'.'+env['byfn'][(mod,int(m[1]))]['method'] if (mod,int(m[1])) in env['byfn'] else ''),t)
 p.write_text(t,encoding='utf8')
print(json.dumps({'files':len(selected),'unityChanged':False}))
