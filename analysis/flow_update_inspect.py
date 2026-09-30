import runpy,sys,struct
sys.argv=['x','NO'];n=runpy.run_path('analysis/flow_disassemble.py')
for t in n['ts']:
 if n['ms'](t[0]) in ('GameControl','ControlMgr','GameEntry','GameManager'):
  print(n['ms'](t[0]),[(i,n['ms'](struct.unpack_from('<i',n['meta'],n['pairs'][11][0]+i*12)[0])) for i in range(t[8],t[8]+t[18])])
for m in n['methods']:
 if m['method'] in ('Update','Updata','OnUpdate') and any(s in m['class'] for s in ('Control','Mgr','Game','Module')): print(m['class'],m['method'],m['function'],m['module'])
print([struct.unpack('<f',struct.pack('<I',v))[0] for v in [1053609165,1048576000]])
