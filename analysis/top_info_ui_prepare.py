from pathlib import Path
import json
helper=Path(__file__).parent/'outgame_ui_prepare.py'
env={'__file__':str(helper)}
exec(helper.read_text(encoding='utf8').split("for p in read(G/")[0],env)
G=env['G']
for page in env['read'](G/'outgame/top-info-ui-evidence.json')['prefabs']:
 root=page['nodes'][0]['path'];env['add'](root.strip('/'),page['nodes'],page['outletBindings'],root)
result=dict(target=dict(appId='wxcf1394487200e48f',version='43'),prefabs=env['prefabs'],sprites=list(env['sprites'].values()),fonts=list(env['fonts'].values()),unknowns=env['unknown'])
(G/'outgame/top-info-ui-import.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print({'prefabs':len(result['prefabs']),'nodes':sum(len(p['nodes']) for p in result['prefabs']),'sprites':len(result['sprites'])})
