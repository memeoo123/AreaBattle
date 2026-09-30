"""Inspect copied Unity assets without executing game code or downloading data."""
import collections
import hashlib
import json
import re
import sys
from pathlib import Path

VENDOR = Path('E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
sys.path.insert(0, str(VENDOR))
import UnityPy

ROOT = Path(__file__).parent / 'targets/wxcf1394487200e48f/43'
CACHE = ROOT / 'work/cache/StreamingAssets/WebGL/Proj_hdzd'
OUT = ROOT / 'generated/assets'
OUT.mkdir(parents=True, exist_ok=True)
reports = []
for p in sorted(CACHE.rglob('*')):
    if not p.is_file() or p.suffix == '.txt':
        continue
    entry = {'source':p.relative_to(ROOT).as_posix(), 'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
    try:
        env = UnityPy.load(str(p))
        entry['types'] = dict(collections.Counter(o.type.name for o in env.objects))
        entry['objects'] = []
        entry['containers'] = list(env.container.keys())
        for obj in env.objects:
            row = {'pathId':obj.path_id, 'type':obj.type.name, 'assetFile':obj.assets_file.name}
            try:
                row['name'] = obj.peek_name()
                if obj.type.name == 'TextAsset':
                    data = obj.read()
                    raw = data.m_Script
                    raw = raw.encode('utf-8', 'surrogateescape') if isinstance(raw, str) else bytes(raw)
                    name = re.sub(r'[^\w.-]', '_', data.m_Name)
                    dest = OUT / p.stem / f'{name}__{obj.path_id}.bytes'
                    dest.parent.mkdir(parents=True, exist_ok=True)
                    dest.write_bytes(raw)
                    row.update(output=dest.relative_to(ROOT).as_posix(),size=len(raw),sha256=hashlib.sha256(raw).hexdigest(),headerHex=raw[:32].hex())
                elif obj.type.name in {'MonoScript', 'MonoBehaviour', 'Transform', 'RectTransform', 'GameObject', 'Camera'}:
                    tree = obj.read_typetree()
                    dest = OUT / p.stem / f'{obj.type.name}__{obj.path_id}.json'
                    dest.parent.mkdir(parents=True, exist_ok=True)
                    dest.write_text(json.dumps(tree,ensure_ascii=False,indent=2),encoding='utf-8')
                    row['output'] = dest.relative_to(ROOT).as_posix()
            except Exception as exc:
                row['readError'] = str(exc)[:300]
            entry['objects'].append(row)
    except Exception as exc:
        entry['error'] = str(exc)[:300]
    reports.append(entry)
(ROOT / 'evidence/asset-inventory.json').write_text(json.dumps({'tool':{'name':'UnityPy','version':UnityPy.__version__,'vendor':str(VENDOR)},'bundles':reports},ensure_ascii=False,indent=2),encoding='utf-8')
text_assets = [o for b in reports for o in b.get('objects',[]) if o['type']=='TextAsset']
print(json.dumps({'bundleCount':len(reports),'failed':[{k:v for k,v in b.items() if k in {'source','error'}} for b in reports if 'error' in b], 'objectCount':sum(len(b.get('objects',[])) for b in reports),'textAssetCount':len(text_assets),'textAssets':[{'name':o.get('name'),'size':o.get('size'),'headerHex':o.get('headerHex')} for o in text_assets if re.search('Level|Soldier|Tower|GlobalValue|Camp|AIConfig',o.get('name') or '')]},ensure_ascii=False,indent=2))
