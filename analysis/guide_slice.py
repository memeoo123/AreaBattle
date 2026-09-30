import json,sys
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
r=json.loads(Path('analysis/targets/wxcf1394487200e48f/43/generated/guide-evidence.json').read_text(encoding='utf8'))
for f in r['functions']:
 if str(f['function'])!=sys.argv[1]:continue
 print(f['class'],f['method'],f['module'],f['function'])
 for s in f['disassembly']:
  if len(sys.argv)<3 or sys.argv[2]<=s[:8]<=sys.argv[3]:print(s)
