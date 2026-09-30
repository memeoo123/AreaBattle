import collections
import hashlib
import json
import re
import sys
from pathlib import Path
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
items=json.loads((ROOT/'evidence/full-level-downloads.json').read_text(encoding='utf-8'))['items']
out=ROOT/'generated/all-levels';out.mkdir(exist_ok=True)
records=[];fields=collections.defaultdict(collections.Counter);ships=collections.Counter();camps=collections.Counter();boss=0
for item in items:
    assert item['status']=='verified'
    path=ROOT/item['destination'];assert hashlib.sha256(path.read_bytes()).hexdigest()==item['sha256']
    env=UnityPy.load(str(path));objects=[x for x in env.objects if x.type.name=='TextAsset'];assert len(objects)==1
    obj=objects[0];asset=obj.read();raw=asset.m_Script.encode('utf-8','surrogateescape')
    data=json.loads(raw.decode('utf-8-sig'));levelid=int(re.fullmatch('data/levelcfg/level_(\d+).json.unity3d',item['logicalName']).group(1))
    output=f'generated/all-levels/level_{levelid}.json'
    (ROOT/output).write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
    for k,rows in data.items():
        for row in rows:
            fields[k].update(row.keys())
    for tower in data['StarInfoCfgs']:
        ships[tower['ShipID']]+=1;camps[tower['CampID']]+=1
        boss+=bool(tower.get('isBoss',False))
    records.append({'levelId':levelid,'source':item['destination'],'sha256':item['sha256'],'pathId':obj.path_id,'assetFile':obj.assets_file.name,'textSha256':hashlib.sha256(raw).hexdigest(),'output':output,'towerCount':len(data['StarInfoCfgs']),'obstacleCount':len(data['ObstacleInfoCfgs'])})
summary={'levelCount':len(records),'totalTowers':sum(ships.values()),'shipCounts':dict(ships),'campCounts':dict(camps),'explicitBossTowers':boss,'fields':{k:dict(v) for k,v in fields.items()},'towerCountRange':[min(r['towerCount'] for r in records),max(r['towerCount'] for r in records)],'maxObstacles':max(r['obstacleCount'] for r in records)}
(ROOT/'evidence/full-level-extraction.json').write_text(json.dumps({'tool':'UnityPy '+UnityPy.__version__,'summary':summary,'levels':sorted(records,key=lambda r:r['levelId'])},ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(summary,ensure_ascii=False,indent=2))
