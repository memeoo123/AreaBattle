import json,sys,re
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
r=json.loads(Path('analysis/targets/wxcf1394487200e48f/43/generated/scene-binding-evidence.json').read_text(encoding='utf8'))
for f in r['functions']:
 if len(sys.argv)>1 and not any(a in f['class']+'.'+f['method'] or a in (str(f['function']),str(f['token'])) for a in sys.argv[1:] if not a.startswith('--')):continue
 print(f['class']+'.'+f['method'],f['module'],f['function'],f['token'],f['body'])
 if '--list' in sys.argv:continue
 for x in f['disassembly']:
  if '--calls' in sys.argv and not ('; literal:' in x or '; method:' in x or re.search(r'call \[\d+\] ; \S',x)):continue
  print(re.sub(r' {2,}',' ',x))

