from pathlib import Path
import struct,json,hashlib
exec(Path('analysis/inspect_shared_item_metadata.py').read_text(encoding='utf-8-sig').split('for t in [')[0])
fields={}
for j in range(ts[4542][18]):
 fn,ft,tk=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[4542][8]+j));off=u(u(3823136+4*4542)+4*j)
 if off not in [32,36,40,44,52,60,68]:continue
 ptr=u(200288+4*ft);data=u(ptr);inst=u(data+4);fields[off]=dict(name=ms(fn),generic=tn(u(data)),arguments=[tn(u(u(inst+4)+4*k)) for k in range(u(inst))])
assert fields[36]['arguments']==fields[44]['arguments'] and 'ItemUserData' in fields[36]['arguments'][1]
assert 'ProductUserData' in fields[32]['arguments'][1]
o=Path('analysis/targets/wxcf1394487200e48f/43/generated/outgame');body=(o/'disassembly/Type4542-34596.txt').read_text(encoding='utf8');assert 'i32.load [2, 36]' in body and 'i32.load [2, 44]' in body and 'i32.load [2, 32]' not in body
(o/'GLOBAL_ITEM_REWARD_FIELD_AUDIT.json').write_text(json.dumps(dict(fields=fields,snapshotMethod=34596,snapshotFunction=12037,snapshotKind='item',snapshotBodySha256=hashlib.sha256(body.encode()).hexdigest(),evidence='Field36 live ItemUserData dictionary and field44 snapshot ItemUserData dictionary; product fields32/40 belong to34583. This corrects preliminary misidentification before final validation.'),ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print(json.dumps(fields,ensure_ascii=False))
