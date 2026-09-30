"""Flatten recovered hierarchy for Unity JsonUtility's bounded serialization depth."""
import json
from pathlib import Path
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43/generated'
source=json.loads((R/'obstacle-evidence.json').read_text(encoding='utf-8'))
prefabs=[]
for prefab in source['prefabs']:
 nodes=[]
 def flatten(node,parent):
  index=len(nodes)
  nodes.append({k:node[k] for k in ['name','active','layer','transform','components']}|{'parent':parent})
  for child in node['children']:flatten(child,index)
 flatten(prefab['root'],-1)
 prefabs.append({'entityId':prefab['entityId'],'name':prefab['name'],'nodes':nodes})
(R/'obstacle-runtime.json').write_text(json.dumps({'prefabs':prefabs},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Flattened15 source hierarchies without changing geometry.')
