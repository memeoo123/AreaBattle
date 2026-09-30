from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==576
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Piggy bank source message lifecycle','checksPassed':576,'outgameChecksPassed':309,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=576;write(p,s)
audit={'status':'actual_message_bus_subscription_verified_production_owner_pending','source':['disassembly/Type3874-30170.txt','disassembly/Type3874-30173.txt','disassembly/Type3874-30175.txt','disassembly/Type3485-26822.txt','disassembly/Type3485-26830.txt'],'verified':['OnInit sets ActiveUpdate true and adds GamePlayState listener','Message checks current level before reading first boxed state value','OnDispose removes one source delegate; repeated init retains duplicates','Actual OutgameMessageDispatcher victory events change held inventory; ordinary dispose/reinit grants once'],'remaining':['Production owner must instantiate/init bank and publish gameplay states through source host','Source reporting provider and full gameplay/return-home persistence E2E','Easter/time and account bootstrap'],'checks':576,'freshPlayMode':False}
write(o/'PIGGY_EVENTS_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 存钱罐游戏状态消息连接（576项）\nOutgamePiggyBank恢复OnInit/OnDispose并连接现有OutgameMessageDispatcher的GamePlayState通道：初始化启用ActiveUpdate，胜利消息更新实际库存字段，退出解绑。保留原版重复订阅与单次退订语义，以及关卡门槛先于事件参数读取的顺序。576项集成通过。实际生产owner创建与发布状态、真实报告服务和完整进出关存档流程仍待完成；本次无新PlayMode或Player。\n',encoding='utf8')
print('Recorded piggy source message lifecycle; production composition remains incomplete.')
