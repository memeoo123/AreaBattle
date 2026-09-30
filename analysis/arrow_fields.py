import runpy,sys,struct
sys.argv=['x','NO'];n=runpy.run_path('analysis/flow_disassemble.py')
for t in n['ts']:
 if n['ms'](t[0]) in ('DOTween','Tween','Soldier','ArrowTower'):
  print(n['ms'](t[0]))
  for i in range(t[8],t[8]+t[18]):print(i,n['ms'](struct.unpack_from('<i',n['meta'],n['pairs'][11][0]+i*12)[0]))
