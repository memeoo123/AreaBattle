"""Acquire only catalogue-listed level-layout bundles for the authorized full in-level goal."""
import concurrent.futures
import hashlib
import json
import urllib.request
import urllib.error
from datetime import datetime,timezone
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
catalog=json.loads((ROOT/'generated/resource-catalog.json').read_text(encoding='utf-8'))
entries=[x for x in catalog['entries'] if x['name'].startswith('data/levelcfg/')]
base='https://gameoss-hz.entermore.cn/app-3/Release/Proj_hdzd/XYX/weixin/hcrzd/1.36/StreamingAssets/WebGL/Proj_hdzd/'
def download(item):
    dest=(ROOT/'work/remote'/item['file']).resolve();assert ROOT.resolve() in dest.parents
    result={'logicalName':item['name'],'catalogLine':item['line'],'url':base+item['file'],'expectedBytes':item['size'],'expectedMd5':item['md5'],'destination':dest.relative_to(ROOT).as_posix()}
    try:
        if dest.exists():data=dest.read_bytes();result['acquisition']='existing'
        else:
            with urllib.request.urlopen(urllib.request.Request(result['url'],headers={'User-Agent':'AreaBattle-resource-analysis/1.0'}),timeout=20) as response:
                data=response.read(1048577);result.update(httpStatus=response.status,acquisition='downloaded')
        assert len(data)==item['size'] and hashlib.md5(data).hexdigest()==item['md5'],'Catalogue size/MD5 mismatch'
        dest.parent.mkdir(parents=True,exist_ok=True)
        if not dest.exists():dest.write_bytes(data)
        result.update(status='verified',sha256=hashlib.sha256(data).hexdigest(),actualBytes=len(data))
    except Exception as exc:result.update(status='failed',error=str(exc))
    return result
report={'authorization':'User requested autonomous full in-level restoration after approving target resource downloads','atUtc':datetime.now(timezone.utc).isoformat(),'items':[]}
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
    for n,result in enumerate(pool.map(download,entries),1):
        report['items'].append(result)
        if n%100==0:print(f'Processed {n}/{len(entries)}',flush=True)
report['items'].sort(key=lambda x:x['logicalName'])
(ROOT/'evidence/full-level-downloads.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'requested':len(entries),'verified':sum(x['status']=='verified' for x in report['items']),'failed':[x for x in report['items'] if x['status']!='verified'],'bytes':sum(x.get('actualBytes',0) for x in report['items'])},ensure_ascii=False,indent=2))
