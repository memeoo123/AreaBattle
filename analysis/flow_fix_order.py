from pathlib import Path
p=Path('analysis/flow_evidence.py');s=p.read_text(encoding='utf-8');s=s.replace("Ordering of physical list is a separate WayLineControl dependency.","WayLineControl initializes the physical list sorted by LineLength ascending (independent asset_pipeline static proof); therefore Reinforce/Occupy prefer shortest eligible physical edges.")
s=s.replace("w('1ff354','1ff3c7')]),","w('1ff354','1ff3c7'),'controls-disassembly/__c-TnyuYV_-100665602.txt: comparer LineLength field+52; CoreProbe-function2297-2297.txt less=-1 greater=1']),")
p.write_text(s,encoding='utf-8')
