"""Download only the three user-approved catalogued level bundles."""
import hashlib
import json
import urllib.request
import urllib.error
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).parent/'targets/wxcf1394487200e48f/43'
plan=json.loads((ROOT/'generated/next-download-plan.json').read_text(encoding='utf-8'))
manifest=json.loads((ROOT/'manifest.json').read_text(encoding='utf-8'))
manifest['authorization']['networkApproval']={'userResponse':'允许','scope':'Download the three level_0/1/2 resource bundles in generated/next-download-plan.json','recordedAtUtc':datetime.now(timezone.utc).isoformat()}
(ROOT/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
records=[]
for item in plan['items']:
    dest=(ROOT/item['destination']).resolve()
    assert ROOT.resolve() in dest.parents
    record=dict(item)
    try:
        request=urllib.request.Request(item['candidateUrl'],headers={'User-Agent':'AreaBattle-resource-analysis/1.0'})
        with urllib.request.urlopen(request,timeout=20) as response:
            record.update(httpStatus=response.status,finalUrl=response.url,contentType=response.headers.get('Content-Type'))
            data=response.read(1048577)
        if len(data)>1048576: raise ValueError('Unexpected response exceeds 1 MiB')
        record.update(actualBytes=len(data),sha256=hashlib.sha256(data).hexdigest(),md5=hashlib.md5(data).hexdigest(),headerHex=data[:32].hex())
        record['sizeMatches']=len(data)==item['expectedBytes']
        record['md5Matches']=record['md5']==item['catalogMd5']
        if not record['sizeMatches'] or not record['md5Matches']: raise ValueError('Catalogue size or MD5 mismatch')
        dest.parent.mkdir(parents=True,exist_ok=True)
        dest.write_bytes(data)
        record['status']='verified'
    except urllib.error.HTTPError as exc:
        record.update(status='http-error',httpStatus=exc.code,error=str(exc))
    except Exception as exc:
        record.update(status='failed',error=str(exc))
    records.append(record)
report={'atUtc':datetime.now(timezone.utc).isoformat(),'items':records}
(ROOT/'evidence/level-downloads.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=False,indent=2))
