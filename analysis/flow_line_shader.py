from pathlib import Path
p=Path('analysis/asset_shader_probe.py').read_text()
p=p.replace('o.path_id!=-1870220313943124997','o.path_id!=6012412777988399977')
exec(compile(p, str(Path('analysis/asset_shader_probe.py').resolve()),'exec'))
