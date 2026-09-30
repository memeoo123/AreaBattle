from pathlib import Path
p=Path('analysis/flow_all_module_map.py');s=p.read_text(encoding='utf-8').replace("Path('analysis/flow_all_map.json').write_text(json.dumps(result,ensure_ascii=False),encoding='utf-8')", "")
p.write_text(s,encoding='utf-8')
Path('analysis/flow_all_map.json').unlink()
