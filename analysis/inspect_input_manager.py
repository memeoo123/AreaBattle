from pathlib import Path
import json,struct
h=Path('analysis/recover_outgame_manager_registry.py').resolve();n={'__file__':str(h)};exec(h.read_text().split('rows=[]')[0],n);globals().update(n)
t=ts[3603]
for j in range(t[18]):
 f=struct.unpack_from('<3i',b,pairs[11][0]+12*(t[8]+j));print(u(u(3823136+4*3603)+4*j),ms(f[0]),f[1])
mm=json.loads((p/'generated/outgame/method-map.json').read_text(encoding='utf8'))
for m in mm:
 if m['cls']=='InputManager' and (m['method'].isascii() or m['body']['size']>300):print(m['metadata'],m['method'],m['function'],m['body']['size'])
