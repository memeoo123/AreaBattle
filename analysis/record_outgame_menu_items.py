from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==570
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Menu item creation and frame wait order','checksPassed':570,'outgameChecksPassed':303,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=570;write(p,s)
audit={'status':'source_sequence_verified_production_host_pending','source':['disassembly/Type4294-32741.txt','disassembly/Type4296-9802.txt'],'verified':['New Main, Shop and Commander created in source order, each followed by shared WaitForEndOfFrame','Existing pages are made visible without frame waits','ItemInfo created only if absent after the three pages','Close visits source fields32,24,28,36,40 and excludes ItemInfo'],'remaining':['Concrete production page factory and UI lifecycle','Actual frame scheduler PlayMode verification','Account/data and production entry with full Player E2E'],'checks':570,'freshPlayMode':False}
write(o/'MENU_ITEMS_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 菜单页面异步创建与复用（570项）\n新增OutgameMenuItems还原32741/9802，主页、商店、指挥官依次创建，每个新页面后等待共享帧末指令；已有页面直接显示，最后按需创建物品信息页。关闭顺序保留两处旧页面字段且不关闭物品信息页。570项集成通过；真实帧调度、具体页面宿主、生产入口及Player端到端仍待验证，本次未跑PlayMode或Player。\n',encoding='utf8')
print('Recorded menu item creation; production composition remains incomplete.')
