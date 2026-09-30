import json,sys
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
def walk(n,parent=''):
    path=parent+'/'+n['name']
    print(path,n['id'],'active=',n['active'],'transform=',{k:v for k,v in n.get('transform',{}).items() if k in ('m_LocalPosition','m_LocalScale','m_AnchorMin','m_AnchorMax','m_AnchoredPosition','m_SizeDelta','m_Pivot')})
    for c in n['components']:
        if c['type']=='MonoBehaviour':
            script=(c.get('script')or{}).get('m_ClassName')
            print(' ',script,'partial',c.get('schemaPartial'),{k:v for k,v in c['data'].items() if k not in ('m_ObjectHideFlags','m_GameObject','m_Script','m_Name','m_EditorClassIdentifier')})
    for child in n['children']:walk(child,path)
for name in sys.argv[1:]:
    d=json.loads((ROOT/f'generated/unity-assets/prefabs/{name}.hierarchy.json').read_text(encoding='utf8'))
    walk(d['root'])
