"""Local HUD consumer disassembly and metadata, no Unity writes."""
import json,re,runpy,contextlib,io,struct
from pathlib import Path
class Sink(io.StringIO):
 def reconfigure(self,**kwargs):pass
with contextlib.redirect_stdout(Sink()):env=runpy.run_path(str(Path(__file__).with_name('combat_presentation_disassemble.py')))
ROOT=env['ROOT'];OUT=ROOT/'generated';doc=json.loads((OUT/'hud-evidence.json').read_text(encoding='utf8'));dest=OUT/'combat-hud-disassembly';dest.mkdir(exist_ok=True)
classes=['TowerCanvas','Tower','Proj_xqzdPlayUI']
for f in doc['functions']:
 if f['class'] not in classes:continue
 s='; '+f['class']+'.'+f['method']+' '+f['module']+' function'+str(f['function'])+'\n'+'\n'.join(f['disassembly'])
 s=re.sub(r'call \[(\d+)\] ; [^\n]*',lambda m:m[0].split(' ;')[0]+' ; '+(env['byfn'][(f['module'],int(m[1]))]['cls']+'.'+env['byfn'][(f['module'],int(m[1]))]['method'] if (f['module'],int(m[1])) in env['byfn'] else ''),s)
 s=re.sub(r'i32.const \[(\d+)\](?! ;)',lambda m:m[0]+env['annotation'](int(m[1])),s)
 (dest/(f['class']+'-'+str(f['token'])+'.txt')).write_text(s,encoding='utf8')
b=env['b'];pairs=env['pairs']
def string(i):return b[pairs[2][0]+i:b.index(0,pairs[2][0]+i)].decode('utf8')
types=[]
for idx in range(pairs[19][1]//88):
 t=struct.unpack_from('<16i8H2I',b,pairs[19][0]+idx*88)
 if string(t[0]) not in classes:continue
 fs=[]
 for fi in range(t[8],t[8]+t[18]):
  f=struct.unpack_from('<3i',b,pairs[11][0]+fi*12);fs.append({'name':string(f[0]),'type':f[1],'token':f[2]})
 types.append({'class':string(t[0]),'typeIndex':idx,'fields':fs})
(OUT/'combat-hud-types.json').write_text(json.dumps(types,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'functions':len(list(dest.glob('*.txt'))),'classes':len(types)}))
