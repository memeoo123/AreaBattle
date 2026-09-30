import json,struct,sys
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
ROOT=Path('analysis/targets/wxcf1394487200e48f/43')
r=json.loads((ROOT/'generated/asset-evidence.json').read_text(encoding='utf8'));objs={o['id']:o for o in r['objects']}
scripts={o['id'] for o in objs.values() if o['type']=='MonoScript' and o['name'] in ('CanvasScaler','UIControl','UICamera','UIManager')}
for o in objs.values():
 if o['type']!='MonoBehaviour' or not any(x['property']=='.m_Script' and x.get('target') in scripts for x in o.get('references',[])):continue
 go=objs[next(x['target'] for x in o['references'] if x['property']=='.m_GameObject')]
 print(o['id'],go['id'],go['name'],o['outputs'])
 if 'raw' in o.get('outputs',{}):
  b=(ROOT/o['outputs']['raw']).read_bytes();print('FIELDS',struct.unpack_from('<iff2fi f',b,32))
  gd=json.loads((ROOT/go['outputs']['typetree']).read_text(encoding='utf8'))
  print('components',[(objs[x['target']]['type'],objs[x['target']]['id']) for x in go['references'] if x['property'].startswith('.m_Component')])
