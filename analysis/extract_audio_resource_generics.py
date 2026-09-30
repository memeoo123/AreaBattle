"""Resolve shared IL2CPP ObjectPool bodies through original registration tables."""
from pathlib import Path
import contextlib,io,runpy,struct,json,re,hashlib
class Sink(io.StringIO):
 def reconfigure(self,**kw):pass
with contextlib.redirect_stdout(Sink()):f=runpy.run_path('analysis/flow_all_module_map.py')
mem=f['memory'];u=lambda at:struct.unpack_from('<I',mem,at)[0]
assert u(2673556)==6294 and u(2673560)==2623360
assert all(u(2623360+4*i)==0 or u(2623360+4*i) in f['n']['table'] for i in range(6294))
n={'__file__':str(Path('analysis/outgame_disassemble.py').resolve())}
exec(Path(n['__file__']).read_text(encoding='utf8').split("for m in env['gm']:")[0],n)
selected=[];manifest=[]
for i in range(6294):
 specIndex,methodIndex,invoker,adjustor=struct.unpack_from('<4i',mem,382304+16*i)
 spec=struct.unpack_from('<3i',mem,483008+12*specIndex)
 if spec[0] not in [26176,26186,26188,26190,26239,26253,22371,22372,22373,22374,22377]:continue
 table=u(2623360+methodIndex*4);mod,fn=f['n']['table'][table]
 md=n['md'][spec[0]];typ=md[1];mp=n['maps'][mod];body=mp['bodies'][str(fn)]
 row=dict(metadata=spec[0],cls=n['name'](n['ts'][typ][0]),method=n['name'](md[0]),module=mod,function=fn,table=table,
          genericMethodTableRow=i,methodSpecIndex=specIndex,methodSpec=list(spec),genericMethodPointerIndex=methodIndex,
          path='disassembly/AudioResourceGeneric-'+str(spec[0])+'.txt')
 manifest.append(row);selected.append(dict(row,**{'class':'AudioResourceGeneric','token':spec[0],'body':body,'signature':mp['signatures'][body['typeIndex']]}))
ns=n['ns'];ns['selected']=selected
with contextlib.redirect_stdout(Sink()):exec(n['s'][n['s'].index('for m in selected:'):],ns)
for row in manifest:
 p=n['out'].parent/row['path'];text=p.read_text(encoding='utf8');mod=row['module'];env=n['env']
 text=re.sub(r'i32.const \[(\d+)\]',lambda a:a[0]+(' ;'+env['annotation'](int(a[1])) if env['annotation'](int(a[1])) else ''),text)
 text=re.sub(r'call \[(\d+)\] ; [^\n]*',lambda a:a[0].split(' ;')[0]+' ; '+(env['byfn'][(mod,int(a[1]))]['cls']+'.'+env['byfn'][(mod,int(a[1]))]['method'] if (mod,int(a[1])) in env['byfn'] else ''),text)
 p.write_text('; Original shared generic: '+row['cls']+'.'+row['method']+'\n'+text,encoding='utf8')
(n['out'].parent/'audio-resource-generics.json').write_text(json.dumps(dict(sourceSha256=hashlib.sha256(mem).hexdigest(),registration=2673556,genericPointers=2623360,methodTable=382304,specTable=483008,methods=manifest,qualification='Shared reference-type implementations for AudioClip asset loading and IEnumerator coroutine wrapper; concrete types supplied by original generic contexts.'),ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print('Extracted',len(manifest),'shared audio resource methods')


