from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==567
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Concrete Unity home scene and camera activation','checksPassed':567,'outgameChecksPassed':300,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=567;write(p,s)
audit={'status':'home_scene_objects_verified_production_owner_pending','source':['disassembly/Type4064-31255.txt','disassembly/Type4064-31256.txt','home-scene-generics.json','disassembly/Type4104-31533.txt'],'verified':['Optional Dictionary<int,GameObject> keyed100+sourceOffset60 deactivated first','Scene_game disabled; Scene_home enabled','HomeCamera and CommanderCamera enabled, GameCamera disabled','Source repeated commander activation retained','Home asset callback31533 overwrites cached config then preloads; it does not directly open menu'],'remaining':['Actual original scene owner references and startup composition','Menu visibility lifecycle and home preload completion ownership','Account/data integration, persistence and Player E2E'],'checks':567,'freshPlayMode':False}
write(o/'HOME_SCENE_PRESENTATION_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 主页场景对象切换（567项）\n新增OutgameHomeScenePresentation直接操作Unity场景对象，复原GameControl31255/31256：可选字典100+offset60对象隐藏，Scene_game关闭，Scene_home/HomeCamera/CommanderCamera开启，GameCamera关闭。泛型证据确认Dictionary<int,GameObject>，测试真实Unity对象及无字典/缺失键路径，567项集成通过。另确认主页配置读取回调31533只覆盖配置并启动预加载，不应在该回调中臆造打开菜单。具体原始场景引用与生产入口仍未接入，未新跑PlayMode或Player。\n',encoding='utf8')
print('Recorded main-start binding; production composition remains incomplete.')
