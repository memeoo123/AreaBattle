"""Locate original UI startup roots in existing local exports; retain partial schema boundaries."""
import json
from pathlib import Path
helper=Path(__file__).with_name('asset_focus.py')
n={'__file__':str(helper.resolve())}
exec(compile(helper.read_text(encoding='utf8').split('roots=[]')[0],str(helper),'exec'),n)
root=n['ROOT']; out=root/'generated/outgame'
container=next(c for c in n['report']['containers'] if c['assetPath']=='assets/gameres/bundleres/ui/uiroot.prefab')
keys=[container['object'],'level0:19','resources.assets:244']
evidence=[]; nodes=[]
def flatten(x,path=''):
 row={'path':path,'name':x['name'],'sourceId':x['id'],'active':bool(x['active']),'transform':x['transform'],'components':[]}
 for c in x['components']:
  kind=(c.get('script') or {}).get('m_ClassName',c['type'])
  if kind in ('Transform','RectTransform'):
   row['transformSourceId']=c['id'];continue
  data=dict(c['data'])
  for k in ['m_RaycastTarget','m_Maskable','m_PreserveAspect','m_FillCenter','m_FillClockwise','m_UseSpriteMesh']:
   if k in data:data[k]=bool(data[k])
  row['components'].append({'className':kind,'sourceId':c['id'],'data':json.dumps(data,ensure_ascii=False)})
 nodes.append(row)
 for ch in x['children']:flatten(ch,path+'/'+ch['name'] if path else ch['name'])
for key in keys:
 obj=n['objs'][key];h=n['hierarchy'](obj)
 evidence.append({'object':key,'source':obj,'root':h})
flatten(evidence[0]['root'])
assert len(nodes)==13 and len(evidence[0]['root']['children'])==9
manifest={'prefabs':[{'name':'UIRoot','rootPath':container['assetPath'],'nodes':nodes,'bindings':[]}],'sprites':[],'fonts':[]}
(out/'ui-root-evidence.json').write_text(json.dumps({'container':container,'roots':evidence},ensure_ascii=False,indent=2),encoding='utf8')
(out/'ui-root-import.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'nodes':len(nodes),'layers':[x['name'] for x in evidence[0]['root']['children']],'source':container,'remaining':['AdaptiveBangs behavior','Bootstrap CanvasScaler/CanvasAdaptive/AtlasLoader partial schemas','LoadUIRootOver lifecycle']},ensure_ascii=False))
