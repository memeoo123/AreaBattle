from pathlib import Path
import json,struct,hashlib
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';n={'__file__':str(helper)};exec(helper.read_text(encoding='utf8').split('rows=[]')[0],n)
u,b,pairs,ts,ms=[n[k] for k in ('u','b','pairs','ts','ms')]
def pointer(ptr):
 data=u(ptr);code=u(ptr+4)>>16&255;d={'pointer':ptr,'code':code,'data':data}
 if code in (17,18):d['name']=ms(ts[data][0])
 if code==29:d['element']=pointer(data)
 return d
classes=[]
for index in (4064,4219,4277):
 fields=[]
 for j in range(ts[index][18]):
  name,t,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[index][8]+j));fields.append({'offset':u(u(3823136+4*index)+4*j),'name':ms(name),'type':pointer(u(200288+4*t))})
 classes.append({'typeIndex':index,'name':ms(ts[index][0]),'fields':fields})
O=n['p']/'generated/outgame';methods=json.loads((O/'method-map.json').read_text(encoding='utf8'))
result={'classes':classes,'sourceMethods':[{'metadata':m['metadata'],'path':m['path'],'sha256':hashlib.sha256((O/m['path']).read_bytes()).hexdigest()} for m in methods if 31254<=m['metadata']<=31277], 'correction':'arr_homeGroud is Texture2D[], not GameObject[]. GameControl owns one shared current scene field60 for home and battle paths.', 'status':'source-evidence'}
generic=[]
for address in (4001196,3956208,3955636,3956204):
 v=u(address);index=(v&0x1ffffffe)>>1;assert v>>29==6
 spec=struct.unpack_from('<3i',n['mem'],483008+12*index);method=n['md'][spec[0]]
 generic.append({'usageAddress':address,'spec':list(spec),'declaringType':ms(ts[method[1]][0]),'method':ms(method[0])})
result['resolvedGenericCalls']=generic
(O/'GAME_CONTROLLER_FIELD_EVIDENCE.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print('GameControl/GameSceneMono/Tags field evidence saved')
