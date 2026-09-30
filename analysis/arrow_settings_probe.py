import runpy,sys,struct
from pathlib import Path
sys.argv=['x','NO'];n=runpy.run_path('analysis/flow_disassemble.py')
for t in n['ts']:
 if n['ms'](t[0]) in ('DOTweenSettings','SafeModeOptions','ModulesSetup'):
  print(n['ms'](t[0]))
  for i in range(t[8],t[8]+t[18]):
   f=struct.unpack_from('<iii',n['meta'],n['pairs'][11][0]+i*12);print(i,n['ms'](f[0]),f[1],n['typenames'].get(f[1]))
for p in Path('analysis/targets/wxcf1394487200e48f/43/generated/unity-assets/MonoBehaviour').glob('DOTweenSettings*.bin'):
 b=p.read_bytes();print(p.name,len(b));print([(hex(i),b[i:i+4].hex(),struct.unpack_from('<I',b,i)[0]) for i in range(0,len(b)-3,4)])
