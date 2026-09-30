import json,sys,re
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
r=json.loads(Path('analysis/targets/wxcf1394487200e48f/43/generated/guide-evidence.json').read_text(encoding='utf8'))
for f in r['functions']:
    if len(sys.argv)>1 and not any(a in (f['class']+'.'+f['method']) or a==str(f['function']) or a==str(f['token']) for a in sys.argv[1:] if not a.startswith('--')):continue
    print(f['class']+'.'+f['method'],f['module'],f['function'],f['token'],f['body'])
    lines=f['disassembly']
    if '--calls' in sys.argv:lines=[x for x in lines if ' ; ' in x or 'f32.const' in x or 'f64.const' in x]
    for x in lines:print(re.sub(r' {2,}',' ',x) if '--compact' in sys.argv else x)
