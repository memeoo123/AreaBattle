"""Bounded original out-of-battle disassembly including nested callbacks."""
from pathlib import Path
import json,struct,runpy,contextlib,io,re,sys
class Sink(io.StringIO):
 def reconfigure(self,**kw):pass
base=Path(__file__).parent
requested=sys.argv[1:] or ['Proj_xqzdStartUI','MenuTabUI','MainTabItemUI','CommanderManager','CommanderManagerData','CommanderData','CommanderSkillData','LocalDataManager','LocalData','LocalDataControl','CommanderControl','CommanderUI']
with contextlib.redirect_stdout(Sink()):env=runpy.run_path(str(base/'combat_presentation_disassemble.py'))
meta=env['b'];pairs=env['pairs'];ts=[struct.unpack_from('<16i8H2I',meta,pairs[19][0]+i*88) for i in range(pairs[19][1]//88)];md=[struct.unpack_from('<7i4H',meta,pairs[5][0]+i*36) for i in range(pairs[5][1]//36)]
def name(n):
 a=pairs[2][0]+n;return meta[a:meta.index(0,a)].decode('utf8')
ids={t[2] for t in ts if name(t[0]) in requested}
for _ in range(5):ids.update(t[2] for t in ts if t[3] in ids)
indexes={i for i,t in enumerate(ts) if t[2] in ids}
s=(base/'combat_disassemble.py').read_text(encoding='utf8');ns={'__file__':str(base/'combat_disassemble.py')};exec(s.split('selected=')[0],ns)
out=env['OUT']/'outgame/disassembly';out.mkdir(parents=True,exist_ok=True);ns['out']=out
maps={m:json.loads((env['ROOT']/f'evidence/{m}-function-map.json').read_text()) for m in ['wasmcode','wasmcode1']};selected=[];manifest=[]
for m in env['gm']:
 if md[m['metadata']][1] not in indexes:continue
 mp=maps[m['module']];body=mp['bodies'].get(str(m['function']))
 if not body:continue
 typeindex=md[m['metadata']][1];key='Type'+str(typeindex)
 selected.append(dict(m,**{'class':key,'token':m['metadata'],'body':body,'signature':mp['signatures'][body['typeIndex']]}))
 manifest.append(dict(m,body=body,path='disassembly/'+key+'-'+str(m['metadata'])+'.txt'))
ns['selected']=selected
with contextlib.redirect_stdout(Sink()):exec(s[s.index('for m in selected:'):],ns)
for m in manifest:
 p=out.parent/m['path'];text=p.read_text(encoding='utf8');mod=m['module']
 text=re.sub(r'i32.const \[(\d+)\]',lambda a:a[0]+(' ;'+env['annotation'](int(a[1])) if env['annotation'](int(a[1])) else ''),text)
 text=re.sub(r'call \[(\d+)\] ; [^\n]*',lambda a:a[0].split(' ;')[0]+' ; '+(env['byfn'][(mod,int(a[1]))]['cls']+'.'+env['byfn'][(mod,int(a[1]))]['method'] if (mod,int(a[1])) in env['byfn'] else ''),text)
 p.write_text('; Original class: '+m['cls']+'\n'+text,encoding='utf8')
p=out.parent/'method-map.json';old=json.loads(p.read_text(encoding='utf8')) if p.exists() else []
merged={m['metadata']:m for m in old}
for m in manifest:merged[m['metadata']]=m
p.write_text(json.dumps(list(merged.values()),ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'classesRequested':requested,'functionsExtracted':len(manifest),'totalIndexed':len(merged)}))
