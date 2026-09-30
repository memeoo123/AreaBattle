"""Audit actual build scenes and runtime ownership; textual references are not execution proof."""
from pathlib import Path
import re,json,hashlib
w=Path(__file__).resolve().parent.parent
u=w/'UnityProject';scripts=u/'Assets/AreaBattle/Scripts'
files=list(scripts.glob('*.cs'));texts={p:p.read_text(encoding='utf8') for p in files}
guid_to_file={}
for p in files:
 meta=Path(str(p)+'.meta')
 if meta.exists():
  hit=re.search(r'^guid: (\w+)',meta.read_text(encoding='utf8'),re.M)
  if hit:guid_to_file[hit[1]]=p
settings=u/'ProjectSettings/EditorBuildSettings.asset';setting_text=settings.read_text(encoding='utf8')
scenes=[]
for path in re.findall(r'path: (.+\.unity)',setting_text):
 p=u/path;text=p.read_text(encoding='utf8');refs=[]
 for guid in re.findall(r'm_Script: \{[^}]*guid: (\w+)',text):
  source=guid_to_file.get(guid);refs.append({'guid':guid,'script':str(source.relative_to(w)) if source else None})
 scenes.append({'path':path,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'scripts':refs})
focus=['OutgameStartupEntry','OutgameMenuView','OutgameMenuSceneBinding','OutgameLocalDataManager','OutgameCommanderManager','OutgameSkinManager','OutgameSdkFunctionInitialization','OutgameSdkFunctionActions','OutgameXyxVideoBridge']
rows=[]
for name in focus:
 mentions=[]
 for p,text in texts.items():
  for line,value in enumerate(text.splitlines(),1):
   if re.search(r'\b'+name+r'\b',value):mentions.append({'path':str(p.relative_to(w)),'line':line,'text':value.strip()})
 rows.append({'type':name,'runtimeSourceMentions':mentions})
result={'status':'production_entry_incomplete','classification':'Authoritative serialized scene evidence plus conservative textual runtime reference inventory; references alone do not prove construction or execution.','buildSettingsSha256':hashlib.sha256(settings.read_bytes()).hexdigest(),'scenes':scenes,'focus':rows,'requirements':[{'id':'production-entry','state':'missing','evidence':'Only configured scene is Battle; entry component references recorded above. No recovered outgame startup owner composed in current runtime.'},{'id':'account-data-owner','state':'incomplete','next':'Recover and compose source procedure/login/data lifecycle without inferring authenticated success from local profile.'},{'id':'menu-lifecycle','state':'incomplete','next':'Create and bind original pages before activation; connect source scene-complete/main-page lifecycle.'},{'id':'return-to-lobby','state':'incomplete','next':'Bind battle completion to original outgame lifecycle and persistence; preserve existing standalone battle mode.'},{'id':'player-build-e2e','state':'not-proven','next':'Build and run actual production entry after composition; current isolated mechanical/PlayMode checks are insufficient.'}],'priority':'Complete original startup owner and menu/data composition before extending optional platform API coverage.'}
out=w/'analysis/targets/wxcf1394487200e48f/43/generated/outgame/PRODUCTION_ENTRY_AUDIT.json';out.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print(json.dumps({'scenes':scenes,'focusTypes':len(rows),'status':result['status']},ensure_ascii=False))
