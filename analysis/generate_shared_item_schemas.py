from pathlib import Path
import struct,json
exec(Path('analysis/inspect_shared_item_metadata.py').read_text(encoding='utf-8-sig').split('for t in [')[0])
pending={4527,4529,4531,4533};done={};roots=set(pending)
def name(ptr):
 d=u(ptr);c=u(ptr+4)>>16&255
 if c in {2,8,10,12,13,14}:return {2:'bool',8:'int',10:'long',12:'float',13:'double',14:'string'}[c]
 if c==29:return name(d)+'[]'
 if c in (17,18):pending.add(d);return ms(ts[d][0])
 if c==21:
  inst=u(d+4);assert ms(ts[u(u(d))][0])=='List`1' and u(inst)==1
  return 'List<'+name(u(u(inst+4)))+'>'
 raise ValueError((c,d))
while pending:
 t=pending.pop()
 if t in done:continue
 fs=[]
 for j in range(ts[t][18]):
  fn,ft,tk=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[t][8]+j));fs.append(dict(name=ms(fn),type=name(u(200288+4*ft)),offset=u(u(3823136+4*t)+4*j)))
 done[t]=dict(name=ms(ts[t][0]),fields=fs)
lines=['// Generated from original metadata by analysis/generate_shared_item_schemas.py.','using System;','using System.Collections.Generic;','namespace AreaBattle.SharedItemConfig','{']
for t,r in sorted(done.items()):
 lines+=['    // Original type '+str(t),'    [Serializable] public sealed class '+r['name']+(' : IOutgameConfigRow' if t in roots else ''),'    {']
 lines+=['        public '+f['type']+' '+f['name']+';' for f in r['fields']]
 if t in roots:lines+=['        public object UniqueID => '+r['fields'][0]['name']+';']
 lines+=['    }']
lines+=['}'];Path('UnityProject/Assets/AreaBattle/Scripts/OutgameSharedItemSchemas.cs').write_text('\n'.join(lines)+'\n',encoding='utf8')
Path('analysis/targets/wxcf1394487200e48f/43/generated/outgame/SHARED_ITEM_CONFIG_SCHEMAS.json').write_text(json.dumps(done,indent=2,ensure_ascii=False)+'\n',encoding='utf8')
print('Generated',len(done),'shared types')
