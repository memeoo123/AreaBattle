import runpy,sys,struct
sys.argv=['x','NO'];n=runpy.run_path('analysis/flow_disassemble.py')
for a in range(3953304,3953624,8):
 v=struct.unpack_from('<I',n['memory'],a)[0];print(a,hex(v),v>>29,(v&0x1ffffffe)>>1)
