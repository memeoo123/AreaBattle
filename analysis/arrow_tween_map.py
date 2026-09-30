import runpy,sys
n=runpy.run_path('analysis/flow_all_module_map.py')
for x in n['result']:
 if (x['cls'] in ('DOTween','Tween','DOTweenComponent','TweenManager') and x['method'] in ('.cctor','Reset','SetUpdate','Update','ApplyTo')) or (x['cls']=='TweenSettingsExtensions' and x['method'] in ('SetUpdate','OnComplete')):print(x)
