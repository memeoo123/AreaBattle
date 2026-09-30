from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==564
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Common play-state dispatcher notifications and special-mode bypass','checksPassed':564,'outgameChecksPassed':297,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=564;write(p,s)
audit={'status':'shared_dispatch_verified_concrete_hosts_pending','source':['disassembly/Type4107-31525.txt','disassembly/Type4107-31479.txt','play-state-control-generics.json'],'verified':['Set state then PauseGame(request==7)','Audio/WayLine/AI/Skill receive current stored state in sequence','Event receives original requested state','PVP active short-circuits Dice query; either mode bypasses state branches and adjusts camera','Reentrant notification state changes affect later reads and final branch'],'remaining':['Concrete common-state host and full branch table','Production startup/account/menu ownership','Actual scene loading, persistence and Player E2E'],'checks':564,'freshPlayMode':False}
write(o/'PLAY_STATE_DISPATCH_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 玩法状态公共分发（564项）\n新增OutgamePlayStateDispatcher，按31525先存当前状态、PauseGame(state==7)，依次通知Audio/WayLine/AI/Skill，再广播原请求值。解析ControlBase泛型确认PVPController和DiceGameControl的激活条件，任一成立跳过普通状态分支，仅调整摄像机。验证通知顺序、特殊模式短路、忽略原第二bool参数，以及回调重入后字段读取与原请求消息不同的语义。564项集成通过，未新跑PlayMode或Player。具体宿主、完整状态分支和生产入口仍待完成。\n',encoding='utf8')
print('Recorded main-start binding; production composition remains incomplete.')
