import sys,json
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
from pathlib import Path
R=Path('analysis/targets/wxcf1394487200e48f/43')
for o in UnityPy.load(str(R/'work/webdata/data.unity3d')).objects:
 if o.type.name=='TagManager':
  d=o.read_typetree();(R/'generated/gesture-visuals/original-TagManager.json').write_text(json.dumps(d,indent=2),encoding='utf8');print(json.dumps(d,ensure_ascii=True))
