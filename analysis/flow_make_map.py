from pathlib import Path
s=Path('analysis/flow_api_names.py').read_text(encoding='utf-8')
s=s.replace("targets={1348,5581,1200,1914,1173,1213,1621}","targets=set(); result=[]")
s=s.replace("if mod=='wasmcode' and f in targets:print(name,ms(ts[m[1]][0]),ms(m[0]),f,'table',tid,'metadata',j)","result.append(dict(image=name,cls=ms(ts[m[1]][0]),method=ms(m[0]),function=f,module=mod,table=tid,metadata=j))")
s += "\nimport json\nfrom pathlib import Path\nPath('analysis/flow_all_map.json').write_text(json.dumps(result,ensure_ascii=False),encoding='utf-8')\nfor x in result:\n if x['image']=='GFRunning.dll' and ('Control' in x['cls'] or 'Process' in x['cls'] or x['cls']=='GameFrameEntry'): print(x)\n"
Path('analysis/flow_all_module_map.py').write_text(s,encoding='utf-8')
