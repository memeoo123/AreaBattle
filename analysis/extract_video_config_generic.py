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
 if spec[0] not in [26612,26619,26622,26631]:continue
 table=u(2623360+methodIndex*4);mod,fn=f['n']['table'][table]
 md=n['md'][spec[0]];typ=md[1];mp=n['maps'][mod];body=mp['bodies'][str(fn)]
 row=dict(metadata=spec[0],cls=n['name'](n['ts'][typ][0]),method=n['name'](md[0]),module=mod,function=fn,table=table,
          genericMethodTableRow=i,methodSpecIndex=specIndex,methodSpec=list(spec),genericMethodPointerIndex=methodIndex,
          path='disassembly/VideoConfigGeneric-'+str(spec[0])+'.txt')
 manifest.append(row);selected.append(dict(row,**{'class':'VideoConfigGeneric','token':spec[0],'body':body,'signature':mp['signatures'][body['typeIndex']]}))
ns=n['ns'];ns['selected']=selected
with contextlib.redirect_stdout(Sink()):exec(n['s'][n['s'].index('for m in selected:'):],ns)
for row in manifest:
 p=n['out'].parent/row['path'];text=p.read_text(encoding='utf8');mod=row['module'];env=n['env']
 text=re.sub(r'i32.const \[(\d+)\]',lambda a:a[0]+(' ;'+env['annotation'](int(a[1])) if env['annotation'](int(a[1])) else ''),text)
 text=re.sub(r'call \[(\d+)\] ; [^\n]*',lambda a:a[0].split(' ;')[0]+' ; '+(env['byfn'][(mod,int(a[1]))]['cls']+'.'+env['byfn'][(mod,int(a[1]))]['method'] if (mod,int(a[1])) in env['byfn'] else ''),text)
 p.write_text('; Original shared generic: '+row['cls']+'.'+row['method']+'\n'+text,encoding='utf8')
(n['out'].parent/'video-config-generic.json').write_text(json.dumps(dict(sourceSha256=hashlib.sha256(mem).hexdigest(),registration=2673556,genericPointers=2623360,methodTable=382304,specTable=483008,methods=manifest,qualification='Shared reference-type generic implementation; runtime generic context supplies concrete pooled type.'),ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print('Extracted',len(manifest),'shared pool methods')


contexts=[]
methodToken=n['md'][26612][6]
for r in range(112):
 token,start,count=struct.unpack_from('<3I',mem,2249520+12*r)
 if token not in [n['md'][i][6] for i in [26612,26619,26622,26631]]:continue
 for i in range(count):
  kind,address=struct.unpack_from('<II',mem,2250864+8*(start+i));index=u(address)
  row=dict(methodToken=token,slot=i,offset=i*4,kind=kind,dataAddress=address,index=index)
  if kind==3:
   spec=struct.unpack_from('<3i',mem,483008+12*index);md=n['md'][spec[0]]
   row.update(method=n['name'](n['ts'][md[1]][0])+'.'+n['name'](md[0]),spec=list(spec))
  contexts.append(row)
(n['out'].parent/'video-config-rgctx.json').write_text(json.dumps(dict(token=methodToken,rows=contexts),ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print(contexts)
