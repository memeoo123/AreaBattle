import runpy,sys
sys.argv=['x','NO'];n=runpy.run_path('analysis/flow_disassemble.py')
for i in [10255]: print(i,n['table'][i])
