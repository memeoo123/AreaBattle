import runpy,sys,contextlib,io
n=runpy.run_path('analysis/flow_all_module_map.py')
for x in n['result']:
 if x['method'] in ('Updata','Update') and x['image']!='Assembly-CSharp.dll':print(x)

