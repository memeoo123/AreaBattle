"""Acquire explicit catalog assets and manifest dependencies into a separate immutable snapshot."""
import json,sys,hashlib,urllib.request,argparse
from pathlib import Path
from datetime import datetime,timezone
sys.stdout.reconfigure(encoding='utf8');sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
SNAP=ROOT/'generated/resource-snapshots/guide-book-ui-20261003'
BASE='https://gameoss-hz.entermore.cn/app-3/Release/Proj_hdzd/XYX/weixin/hcrzd/1.36/StreamingAssets/WebGL/Proj_hdzd/'
catalog=json.loads((ROOT/'generated/resource-catalog.json').read_text(encoding='utf8'))
entries={x['name']:x for x in catalog['entries']}
manifestpath=ROOT/'work/cache/StreamingAssets/WebGL/Proj_hdzd/Proj_hdzd_531b039b937f5e74d8244c5aeeceb42e'
env=UnityPy.load(str(manifestpath))
if __name__=='__main__':
 args=argparse.ArgumentParser();args.add_argument('--download',action='store_true');args=args.parse_args()
 t=next(o.read_typetree() for o in env.objects if o.type.name=='AssetBundleManifest')
 SNAP.mkdir(parents=True,exist_ok=True)
 (SNAP/'bundle-manifest.json').write_text(json.dumps(t,ensure_ascii=False,indent=2),encoding='utf8')
 names=dict(t['AssetBundleNames']);ids={n:i for i,n in names.items()};infos=dict(t['AssetBundleInfos'])
 roots=['ui/mainmenu/guidebookui.prefab.unity3d','ui/mainmenu/items/guidebookitem.prefab.unity3d','uiatlas/guidebookui.spriteatlas.unity3d','uiatlas/guidesprite.spriteatlas.unity3d']
 found=set();pending=[ids[x] for x in roots]
 while pending:
  ident=pending.pop()
  if ident in found:continue
  found.add(ident);pending.extend(infos[ident]['AssetBundleDependencies'])
 records=[]
 for ident in sorted(found,key=lambda i:names[i]):
  name=names[ident];item=entries[name];dest=SNAP/'source-bundles'/item['file']
  assert ROOT.resolve() in dest.resolve().parents
  local=next((p for p in [ROOT/'work/cache/StreamingAssets/WebGL/Proj_hdzd'/item['file'],ROOT/'work/remote'/item['file'],dest,*sorted((ROOT/'generated/resource-snapshots').glob('*/source-bundles/'+item['file']))] if p.exists()),None)
  rec={'logicalName':name,'manifestIndex':ident,'dependencies':[names[i] for i in infos[ident]['AssetBundleDependencies']],'catalog':item,'url':BASE+item['file'],'path':str((local or dest).relative_to(ROOT)).replace('\\','/'),'acquisition':'existing-local' if local and local!=dest else 'new-snapshot','status':'planned'}
  if local or args.download:
   try:
    if local:data=local.read_bytes()
    else:
     with urllib.request.urlopen(urllib.request.Request(rec['url'],headers={'User-Agent':'AreaBattle-resource-analysis/1.0'}),timeout=30) as response:
      rec['httpStatus']=response.status;rec['finalUrl']=response.url;data=response.read(item['size']+1)
    if len(data)!=item['size'] or hashlib.md5(data).hexdigest()!=item['md5']:raise ValueError('Original catalog size/MD5 mismatch')
    if not local:dest.parent.mkdir(parents=True,exist_ok=True);dest.write_bytes(data)
    rec.update(status='verified',size=len(data),md5=hashlib.md5(data).hexdigest(),sha256=hashlib.sha256(data).hexdigest())
   except Exception as e:rec.update(status='failed',error=repr(e))
  records.append(rec)
 old=ROOT/'generated/asset-evidence.json'
 report={'schemaVersion':1,'target':{'appId':'wxcf1394487200e48f','version':'43'},'snapshot':'guide-book-ui-20261003','atUtc':datetime.now(timezone.utc).isoformat(),'authorization':'User authorized autonomous complete outgame reconstruction; source GuideBookUI and GuideBookItem plus explicit GuideSprite/GuideBookUI atlases with original manifest dependency closure.','roots':roots,'tool':{'UnityPy':UnityPy.__version__},'catalogSource':catalog['source'],'manifestSource':str(manifestpath.relative_to(ROOT)),'manifestSha256':hashlib.sha256(manifestpath.read_bytes()).hexdigest(),'baselineAssetEvidence':{'path':str(old.relative_to(ROOT)),'sha256':hashlib.sha256(old.read_bytes()).hexdigest()},'items':records}
 (SNAP/'acquisition-manifest.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
 print(json.dumps({'rootCount':len(roots),'closureCount':len(records),'newBytes':sum(x['catalog']['size'] for x in records if x['acquisition']=='new-snapshot'),'items':[{'name':x['logicalName'],'status':x['status'],'acquisition':x['acquisition'],'error':x.get('error')} for x in records]},ensure_ascii=False,indent=2))
