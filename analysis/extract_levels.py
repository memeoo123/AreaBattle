"""Parse the approved level bundles and preserve raw TextAsset provenance."""
import hashlib
import json
import sys
from pathlib import Path

sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
downloads=json.loads((ROOT/'evidence/level-downloads.json').read_text(encoding='utf-8'))
out=ROOT/'generated/levels'
out.mkdir(exist_ok=True)
reports=[]
for index,item in enumerate(downloads['items']):
    assert item['status']=='verified'
    source=ROOT/item['destination']
    assert hashlib.sha256(source.read_bytes()).hexdigest()==item['sha256']
    env=UnityPy.load(str(source))
    texts=[obj for obj in env.objects if obj.type.name=='TextAsset']
    assert len(texts)==1
    obj=texts[0]
    data=obj.read()
    raw=data.m_Script.encode('utf-8','surrogateescape') if isinstance(data.m_Script,str) else data.m_Script
    parsed=json.loads(raw.decode('utf-8-sig'))
    (out/f'level_{index}.raw.json').write_bytes(raw)
    (out/f'level_{index}.json').write_text(json.dumps(parsed,ensure_ascii=False,indent=2),encoding='utf-8')
    reports.append({'levelId':index,'source':item['destination'],'sourceSha256':item['sha256'],'textAssetName':data.m_Name,'assetFile':obj.assets_file.name,'pathId':obj.path_id,'textSha256':hashlib.sha256(raw).hexdigest(),'output':f'generated/levels/level_{index}.json','data':parsed})
(ROOT/'evidence/level-extraction.json').write_text(json.dumps({'tool':'UnityPy '+UnityPy.__version__,'levels':reports},ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(reports,ensure_ascii=False,indent=2))
