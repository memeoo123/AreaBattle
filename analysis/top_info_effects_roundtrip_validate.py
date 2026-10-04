"""Compare every original particle scalar/curve field with native Unity Editor readback."""
import json,math,hashlib,sys
from pathlib import Path
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43';S=R/'generated/resource-snapshots/top-info-effects-20261003'
read=lambda p:json.loads(p.read_text(encoding='utf8'))
embedded='--embedded' in sys.argv
m=read(S/('prepared-embedded/native-import.json' if embedded else 'prepared/native-import.json'));objects={o['id']:o for o in read(S/'asset-evidence-incremental.json')['objects']}
for o in read(R/'generated/asset-evidence.json')['objects']:objects.setdefault(o['id'],o)
rows=[];failures=[]
def compare(source,actual,path,stats,diffs):
 if isinstance(source,dict) and 'm_FileID' in source and 'm_PathID' in source:stats['pointerFieldsExcluded']+=1;return
 if isinstance(source,dict):
  if not isinstance(actual,dict):diffs.append({'path':path,'source':source,'actual':actual});return
  for k,v in source.items():
   if isinstance(v,dict) and 'm_FileID' in v and 'm_PathID' in v:stats['pointerFieldsExcluded']+=1;continue
   if k not in actual:diffs.append({'path':path+'.'+k,'source':v,'actual':'MISSING'})
   else:compare(v,actual[k],path+'.'+k,stats,diffs)
 elif isinstance(source,list):
  if not isinstance(actual,list) or len(source)!=len(actual):diffs.append({'path':path,'sourceLength':len(source),'actualLength':len(actual) if isinstance(actual,list) else None});return
  for i,(a,b) in enumerate(zip(source,actual)):compare(a,b,path+f'[{i}]',stats,diffs)
 elif isinstance(source,(float,int,bool)):
  stats['numericScalars']+=1
  if not isinstance(actual,(float,int,bool)):diffs.append({'path':path,'source':source,'actual':actual});return
  if math.isnan(source) and math.isnan(actual):return
  delta=abs(source-actual)
  # EditorJson uses decimal float32 formatting; this tolerance covers its text rounding only.
  limit=max(1e-7,abs(source)*1e-6)
  if source!=actual and (not math.isfinite(delta) or delta>limit):diffs.append({'path':path,'source':source,'actual':actual,'delta':delta})
  if math.isfinite(delta):stats['maxDecimalRoundoff']=max(stats['maxDecimalRoundoff'],delta)
 else:
  stats['otherScalars']+=1
  if source!=actual:diffs.append({'path':path,'source':source,'actual':actual})
for p in m['prefabs']:
 for c in p['components']:
  if c['type']!='ParticleSystem':continue
  actual=Path('analysis/top-info-effects-roundtrip')/p['name']/(c['fileId']+'.json');o=objects[c['id']];source=R/o['outputs']['typetree'];stats={'numericScalars':0,'otherScalars':0,'pointerFieldsExcluded':0,'maxDecimalRoundoff':0};diffs=[]
  if actual.exists():
   a=read(actual);a=a.get('ParticleSystem',a);compare(read(source),a,'',stats,diffs)
  else:diffs.append({'error':'Unity roundtrip file missing','path':str(actual)})
  row={'prefab':p['name'],'component':c['id'],'path':c['path'],'passed':not diffs,'sourceSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'readbackSha256':hashlib.sha256(actual.read_bytes()).hexdigest() if actual.exists() else None,'stats':stats,'differences':diffs};rows.append(row)
  if diffs:failures.append({'prefab':p['name'],'component':c['id'],'differenceCount':len(diffs)})
out={'passed':not failures,'particleSystems':len(rows),'numericScalars':sum(x['stats']['numericScalars'] for x in rows),'otherScalars':sum(x['stats']['otherScalars'] for x in rows),'comparison':'All original fields recursively; PPtr structures excluded because IDs/GUIDs remapped and importer validates actual references. Numeric tolerance max(1e-7,abs(source)*1e-6) only for Editor JSON decimal float32 formatting.','failures':failures,'components':rows}
(S/('embedded-particle-roundtrip-validation.json' if embedded else 'particle-roundtrip-validation.json')).write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8');print(json.dumps({k:v for k,v in out.items() if k not in ('components','failures')}));print('failed components',len(failures));print('first differences',rows[0]['differences'][:6])
