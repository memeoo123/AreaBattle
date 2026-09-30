from pathlib import Path
import struct
h=Path('analysis/recover_outgame_manager_registry.py').resolve();n={'__file__':str(h)};exec(h.read_text().split('rows=[]')[0],n);globals().update(n)
for i in [4462]:
 t=ts[i]
 for j in range(t[18]):
  f=struct.unpack_from('<3i',b,pairs[11][0]+12*(t[8]+j));ptr=u(200288+4*f[1]);code=(u(ptr+4)>>16)&255;data=u(ptr);print(u(u(3823136+4*i)+4*j),ms(f[0]),f[1],code,ms(ts[data][0]) if code in [17,18] else data)
for a in [3944712,3956196,3916936,3993048]:
 v=u(a);k=v>>29;i=(v&0x1ffffffe)>>1;print('usage',a,k,i)
 if k==6:
  s=struct.unpack_from('<3i',mem,483008+12*i);m=md[s[0]];print(s,ms(ts[m[1]][0]),ms(m[0]))
 elif k in [1,2]:
  ptr=u(200288+4*i);code=(u(ptr+4)>>16)&255;data=u(ptr);print('type',code,data,ms(ts[data][0]) if code in [17,18] else '')
