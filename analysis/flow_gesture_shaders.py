from pathlib import Path
s=Path('analysis/flow_obstacle_shader.py').read_text(encoding='utf8')
s=s.replace("'generated/resource-snapshots/obstacle-visuals-20260928/shaders'","'generated/gesture-visuals/shaders'").replace("O.mkdir(exist_ok=True)","O.mkdir(parents=True,exist_ok=True)")
s=s.replace("o.path_id!=10752","o.path_id not in (200,203)").replace("'builtin10752.json'","f'builtin{o.path_id}.json'").replace("f'builtin10752.","f'builtin{o.path_id}.")
s=s.replace("print('Shader',getattr(d,'m_ParsedForm',None))","print('Shader',o.path_id,d.m_ParsedForm.m_Name)")
exec(compile(s,'gesture_shaders','exec'))
