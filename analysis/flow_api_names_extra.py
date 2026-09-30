import runpy,sys,struct
sys.argv=['x','NO'];n=runpy.run_path('analysis/flow_disassemble.py');memory,ms,ts,md=n['memory'],n['ms'],n['ts'],n['md']; targets={1883,9710}
for im in n['images']:
 name=ms(im[0]);at=memory.find(name.encode()+b'\0')
 if at<0:continue
 addr=struct.pack('<I',at);off=0;c=[]
 while (off:=memory.find(addr,off))>=0:
  if off%4==0 and off+12<len(memory):
   _,cnt,ptr=struct.unpack_from('<III',memory,off)
   if 0<cnt<100000 and 0<ptr and ptr+cnt*4<=len(memory):c.append((cnt,ptr))
  off+=1
 if len(c)!=1:continue
 cnt,ptr=c[0]
 for j,m in enumerate(md):
  if not im[2]<=m[1]<im[2]+im[3]:continue
  rid=m[6]&0xffffff
  if not 1<=rid<=cnt:continue
  tid=struct.unpack_from('<I',memory,ptr+(rid-1)*4)[0]
  if tid not in n['table']:continue
  mod,f=n['table'][tid]
  if mod=='wasmcode' and f in targets:print(name,ms(ts[m[1]][0]),ms(m[0]),f,'table',tid,'metadata',j)

