import json,struct,sys
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
base=Path('analysis/controls_disassemble.py').read_text(encoding='utf8').split('control_type=')[0];ctx={'__file__':str(Path('analysis/controls_disassemble.py').resolve())};exec(compile(base,'metadata_only','exec'),ctx)
ROOT,memory,meta,pairs,ts,md,ms=[ctx[k] for k in ('ROOT','memory','meta','pairs','ts','md','ms')]
for b in (1015222895,1028980212):print('float',b,struct.unpack('<f',struct.pack('<I',b))[0])
for a in (3953328,4032292):print('usage',a,hex(struct.unpack_from('<I',memory,a)[0]),'kind',struct.unpack_from('<I',memory,a)[0]>>29,'index',(struct.unpack_from('<I',memory,a)[0]&0x1fffffff)>>1)
def leb(v):
 b=[]
 while True:
  x=v&127;v>>=7
  if not v and x<64:b.append(x);return bytes(b)
  b.append(x|128)
needle=b'\x41'+leb(3953328)
blobs={n:(ROOT/f'generated/wasm/{n}.wasm').read_bytes() for n in ('wasmcode','wasmcode1')}
for m in ctx['methods']:
 if 'body' not in m:continue
 body=m['body'];b=blobs[m['module']][body['offset']:body['offset']+body['size']]
 if needle in b:print('USES3953328',m['class'],m['method'],m['function'])
for t in ts:
 name=ms(t[0]);fields=[ms(struct.unpack_from('<iii',meta,pairs[11][0]+i*12)[0]) for i in range(t[8],t[8]+t[18])]
 if name in ('TowerCanvas','SpineAnimator','Soldier','DispatchConfig','SettingConfig') or any('grade' in f.lower() for f in fields) and len(fields)<30:
  print('FIELDS',name,t[2],fields)
