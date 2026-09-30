"""Prepare distinct scene and Resources bootstrap canvas prefabs from source evidence."""
from pathlib import Path
import json
r=Path(__file__).parent/'targets/wxcf1394487200e48f/43/generated'
e=json.loads((r/'outgame/ui-root-evidence.json').read_text(encoding='utf8'))
s=json.loads((r/'outgame/ui-bootstrap-schema.json').read_text(encoding='utf8'))
tags=json.loads((r/'gesture-visuals/original-TagManager.json').read_text(encoding='utf8'))['tags']
rows=[]
for root,name in zip(e['roots'][1:],['SceneUICanvas','ResourceUICanvas']):
 node=root['root'];camera=node['children'][0];assert camera['name']=='UICamera'
 comp=lambda x,kind:next(c for c in x['components'] if c['type']==kind)
 def native(x,kind):
  d=dict(comp(x,kind)['data']);d.pop('m_GameObject',None);d.pop('m_Camera',None);d.pop('m_TargetTexture',None)
  d['serializedVersion']=2 if kind=='Camera' else 3
  if kind=='Camera':
   d['m_NormalizedViewPortRect']['serializedVersion']=2
   d['m_CullingMask']['serializedVersion']=2
  return json.dumps(d)
 values={c['class']:c['fields'] for c in s['components'] if c['owner']==root['object']}
 # Both original m_Tag=20010 map to custom tag index10 (source TagManager).
 assert tags[10]=='GFUICanvas'
 rows.append({'name':name,'sourceId':root['object'],'tag':tags[10],'rootTransform':node['transform'],'canvasJson':native(node,'Canvas'),'cameraTransform':camera['transform'],'cameraJson':native(camera,'Camera'),'scaler':values['CanvasScaler'],'raycaster':values['GraphicRaycaster'],'adaptive':values['CanvasAdaptive'],'aspect':values['AspectRatioFitter'],'aspectEnabled':next(c['data']['m_Enabled']!=0 for c in node['components'] if (c.get('script') or {}).get('m_ClassName')=='AspectRatioFitter'),'atlasAutoLoad':values['AtlasLoader']['isAutoLoaderAtlas']})
(r/'outgame/ui-bootstrap-import.json').write_text(json.dumps({'prefabs':rows,'pending':['AtlasLoader behavior; serialized auto-load value retained in manifest','Production composition and route selection']},ensure_ascii=False,indent=2),encoding='utf8')
print([(x['name'],x['sourceId'],x['tag']) for x in rows])
