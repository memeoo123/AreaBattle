import json,struct
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
b=(ROOT/'work/webdata/Il2CppData/Metadata/global-metadata.dat').read_bytes();p=[struct.unpack_from('<II',b,8+8*i) for i in range(21)]
def s(i):return b[p[2][0]+i:b.index(0,p[2][0]+i)].decode('utf8')
memory=(ROOT/'generated/wasm/wasmcode.static-memory.bin').read_bytes(); old=json.loads((ROOT/'generated/gameplay-method-map.json').read_text(encoding='utf8')); module=old['codeGenModule'];maps={n:json.loads((ROOT/f'evidence/{n}-function-map.json').read_text()) for n in ['wasmcode','wasmcode1']};table={}
for n,v in maps.items():
 for e in v['elements']:
  if e['offset'] and e['offset']['opcode']==65:
   for i,f in enumerate(e['functions']):table[e['offset']['value']+i]={'module':n,'function':f}
import runpy,contextlib,io
class Sink(io.StringIO):
 def reconfigure(self,**kwargs):pass
with contextlib.redirect_stdout(Sink()): gm={x["metadata"]:x for x in runpy.run_path(str(Path(__file__).with_name("flow_all_module_map.py")))["result"]}
methods=[];typeinfo=[]
for idx in list(range(4166,4216))+[3406,3407,3408,3409,3470,3471,3472,3473,4224,4232,4233,4234,4269,4272]:
 t=struct.unpack_from('<16i8H2I',b,p[19][0]+idx*88);label='Type'+str(idx);info={'typeIndex':idx,'name':s(t[0]),'fields':[]}
 for fi in range(t[8],t[8]+t[18]):
  f=struct.unpack_from('<3i',b,p[11][0]+fi*12);info['fields'].append({'name':s(f[0]),'typeIndex':f[1],'token':f[2]})
 typeinfo.append(info)
 for mi in range(t[9],t[9]+t[16]):
  m=struct.unpack_from('<7i4H',b,p[5][0]+mi*36);ptr=struct.unpack_from('<I',memory,module['methodPointersAddress']+((m[6]&0xffffff)-1)*4)[0];row={'class':label,'method':s(m[0]),'token':m[6],'tableIndex':ptr};row.update(table.get(ptr,{}))
  if idx<4000 and mi in gm:row.update(tableIndex=gm[mi]['table'],module=gm[mi]['module'],function=gm[mi]['function'])
  if 'module' in row:
   mp=maps[row['module']];body=mp['bodies'].get(str(row['function']))
   if body:row.update(body=body,signature=mp['signatures'][body['typeIndex']])
  methods.append(row)
mp=maps['wasmcode']
for f in [1545,1899,2673,4876,5582,7935,10954,14831,21405,9709,1246,1108,1114,1621,4700,5798,5995,15991,11853,5015,14594,15141,8244,20638,17743,1201,3798,10401,2596,2462,7933,12437,15567,15572,15569,5194,20505,20506,20508,13085,9807,1735,9809,15064,15065,6271,20547,20552,20544,20545,13117,20555,5062,4443]:
 body=mp['bodies'][str(f)]
 methods.append({'class':'Raw','method':str(f),'token':f,'module':'wasmcode','function':f,'body':body,'signature':mp['signatures'][body['typeIndex']]})
for f in [6274,6276,4522,20581,20580,20551,20595,20592,20594,20593,13131,14986,14985,7022,10104,10093]:
 body=mp['bodies'][str(f)]
 methods.append({'class':'Raw','method':str(f),'token':f,'module':'wasmcode','function':f,'body':body,'signature':mp['signatures'][body['typeIndex']]})
mp=maps['wasmcode1']
for f in [44848,20732,23540,42501]:
 body=mp['bodies'][str(f)]
 methods.append({'class':'Raw','method':'wasmcode1_'+str(f),'token':1000000+f,'module':'wasmcode1','function':f,'body':body,'signature':mp['signatures'][body['typeIndex']]})
(ROOT/'generated/combat-extra-map.json').write_text(json.dumps({'types':typeinfo,'methods':methods},ensure_ascii=False,indent=2),encoding='utf8')
src=Path(__file__).with_name('combat_disassemble.py').read_text(encoding='utf8');src=src.replace("names={}","methods += json.loads((ROOT/'generated/combat-extra-map.json').read_text(encoding='utf-8'))['methods']\nnames={}");a=src.index('selected=');z=src.index('\nout=',a);src=src[:a]+"selected=[m for m in methods if m['class'].startswith('Type') or m['class']=='Raw' or m['class']=='ConfigMgr' and m['method'] in ('get_gameTimeScale','get_shipTimeScale','SetLevelSpeed','.ctor') or m['class'] in ('EffectID','Bullet','LineRendererEntity','WayLineCircle') or m['class']=='LevelControl' and m['method'] in ('GetTowerInRange','GetOtherTower','GetEnemyTower')]"+src[z:];exec(compile(src,'combat_disassemble.py','exec'))
