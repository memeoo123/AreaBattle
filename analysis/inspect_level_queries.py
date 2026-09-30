from pathlib import Path
import json,struct
h=Path('analysis/recover_outgame_manager_registry.py').resolve();n={'__file__':str(h)};exec(h.read_text().split('rows=[]')[0],n);globals().update(n)
for a in [3928280,3953384,3953616,4010604,3937580]:
 v=u(a);k=v>>29;i=(v&0x1ffffffe)>>1
 if k==6:i=struct.unpack_from('<3i',mem,483008+12*i)[0];print(a,k,i,ms(ts[md[i][1]][0]),ms(md[i][0]))
 elif k in [1,2]:
  ptr=u(200288+4*i);code=(u(ptr+4)>>16)&255;data=u(ptr);print(a,k,i,code,data,ms(ts[data][0]) if code in [17,18] else '')
for i,t in enumerate(ts):
 if ms(t[0])=='Tower':
  print('Tower',i)
  for j in range(t[18]):
   f=struct.unpack_from('<3i',b,pairs[11][0]+12*(t[8]+j));print(u(u(3823136+4*i)+4*j),ms(f[0]),f[1])
for i in [31481,31493,31510,31511,31519,31521,31523,31501,31474]:
 m=md[i];print('method',i,ms(m[0]),[(ms(struct.unpack_from('<3i',b,pairs[10][0]+12*(m[4]+j))[0]),struct.unpack_from('<3i',b,pairs[10][0]+12*(m[4]+j))[2]) for j in range(m[10])])
mm=json.loads((p/'generated/outgame/method-map.json').read_text(encoding='utf8'))
print(type(mm));print(str(mm)[:100])
