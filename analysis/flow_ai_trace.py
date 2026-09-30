import subprocess,sys,re
p=subprocess.run([sys.executable,'analysis/flow_disassemble.py','AICampController','AIGoToAction'],capture_output=True,text=True,encoding='utf-8',check=True)
rows=[]
for line in p.stdout.splitlines():
 m=re.match(r'([0-9a-f]{8})\s+(\S+)\s+\[(.*?)\](.*)',line)
 if m:rows.append(dict(pos=int(m[1],16),op=m[2],args=[int(x) for x in m[3].split(', ') if x],suffix=m[4]))
stack=[];labels={};targets={}
for i,r in enumerate(rows):
 if r['op'] in ('block','loop','if'):stack.append(i)
 elif r['op']=='end':
  if stack:labels[stack.pop()]=i+1
 elif r['op']=='else':pass
 elif r['op'] in ('br','br_if','br_table'):targets[i]=[stack[-1-d] if d<len(stack) else None for d in r['args']]
for i,r in enumerate(rows):
 if len(sys.argv)>1 and not int(sys.argv[1],16)<=r['pos']<=int(sys.argv[2],16):continue
 suffix=r['suffix']
 if i in targets:
  dest=[]
  for t in targets[i]:
   if t is None:dest.append('return');continue
   j=t+1 if rows[t]['op']=='loop' else labels[t]
   dest.append(hex(rows[j]['pos']) if j<len(rows) else 'return')
  suffix+=' -> '+str(dest)
 print(f"{r['pos']:08x} {r['op']} {r['args']} {suffix}")
