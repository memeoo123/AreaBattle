from pathlib import Path
import struct,json
p=Path('analysis/recover_outgame_manager_registry.py');n={'__file__':str(p.resolve())};exec(p.read_text(encoding='utf8').split('rows=[]')[0],n)
u,b,pairs,ts,ms,md=[n[k] for k in ['u','b','pairs','ts','ms','md']]
def tn(ptr):
 d=u(ptr);c=u(ptr+4)>>16&255
 if c in {2,8,10,12,13,14}:return {2:'bool',8:'int',10:'long',12:'float',13:'double',14:'string'}[c]
 if c in (17,18):return ms(ts[d][0])+'#'+str(d)
 if c==29:return tn(d)+'[]'
 return str((c,d))
for t in [3490,4527,4529,4531,4533,4550]:
 print('TYPE',t,ms(ts[t][0]))
 for j in range(ts[t][18]):
  name,ft,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[t][8]+j))
  print(u(u(3823136+4*t)+4*j),ms(name),tn(u(200288+4*ft)))
for a in [3954552,3955676,3954568,3955688,3954820,3954972]:
 v=u(a);kind=v>>29;i=(v&0x1ffffffe)>>1
 if kind==6:i,ci,mi=struct.unpack_from('<3i',n['mem'],483008+12*i)
 print('METHOD',a,kind,i,ms(ts[md[i][1]][0]),ms(md[i][0]))
