import runpy
n=runpy.run_path('analysis/flow_all_module_map.py')
for x in n['result']:
 if x['cls']=='MineGameLogicModule':print(x)
