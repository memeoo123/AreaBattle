import json
from pathlib import Path
p=Path('analysis/targets/wxcf1394487200e48f/43/generated/flow-evidence.json');d=json.loads(p.read_text(encoding='utf-8'))
print('facts',len(d['facts']),'bodies',len(d['disassemblies']),'blank',[x['command'] for x in d['disassemblies'] if not x['text'].strip()])
for x in d['facts']:
 if x['id'] in ('AI-source-and-target-order','pause-and-update-gates','ordinary-wave-scope'):print(x['id'],x['value'])
print('unknowns',d['unknowns'])
