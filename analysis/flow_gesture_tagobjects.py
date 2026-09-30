import sys,json
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
from pathlib import Path
R=Path('analysis/targets/wxcf1394487200e48f/43');E=json.loads((R/'generated/asset-evidence.json').read_text(encoding='utf8'));sources=sorted(set(o['source'] for o in E['objects'] if o['type']=='GameObject'));found=[]
for source in sources:
 for o in UnityPy.load(str(R/source)).objects:
  if o.type.name=='GameObject':
   d=o.read_typetree()
   if d['m_Tag']==20001:found.append({'source':source,'id':str(o.assets_file.name)+':'+str(o.path_id),'name':d['m_Name'],'tag':d['m_Tag']})
(R/'generated/gesture-visuals/Tag2-objects.json').write_text(json.dumps(found,indent=2),encoding='utf8');print(json.dumps(found))
