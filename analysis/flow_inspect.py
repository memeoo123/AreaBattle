import sys, runpy
sys.argv=['flow_disassemble.py','NO']
n=runpy.run_path('analysis/flow_disassemble.py')
ms,ts,md,methods,pairs,meta=n['ms'],n['ts'],n['md'],n['methods'],n['pairs'],n['meta']
import struct
print([(x['class'],x['method'],x.get('function'),x.get('module'),x.get('body')) for x in methods if x['class']==n['methodnames'][31535].split('.')[0]])
for t in ts:
 if ms(t[0]) in ('Global','PlayStateEnum'):
  print(ms(t[0]), [(i,ms(struct.unpack_from('<i',meta,pairs[11][0]+i*12)[0])) for i in range(t[8],t[8]+t[18])])
for ti,t in enumerate(ts):
 if ms(t[0]) in ('ConfigMgr','StarInfoCfg','PlayStateEnum'):
  print(ms(t[0]), [(i,ms(struct.unpack_from('<i',meta,pairs[11][0]+i*12)[0])) for i in range(t[8],t[8]+t[18])])
  if ms(t[0])=='PlayStateEnum':
   for j in range(pairs[7][1]//12):
    fi,typ,di=struct.unpack_from('<iii',meta,pairs[7][0]+j*12)
    if t[8]<=fi<t[8]+t[18]:print('enum',fi,typ,di,meta[pairs[8][0]+di:pairs[8][0]+di+4].hex())
for i,m in enumerate(md):
 if ms(ts[m[1]][0])=='Tower' and m[9] in range(4,10):print('Tower slot',m[9],ms(m[0]))
for t in ts:
 if ms(t[0])=='AIActionEnum':print('AI enum',[(i,ms(struct.unpack_from('<i',meta,pairs[11][0]+i*12)[0])) for i in range(t[8],t[8]+t[18])])
