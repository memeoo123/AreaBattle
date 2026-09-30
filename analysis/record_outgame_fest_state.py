from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==579
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Activity state boundaries and original cycle data','checksPassed':579,'outgameChecksPassed':312,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=579;write(p,s)
audit={'status':'source_activity_state_and_cycle_data_verified_config_loader_pending','source':['disassembly/Type4504-34395.txt','disassembly/Type4504-34402.txt','disassembly/Type4505-34420.txt','disassembly/Type4505-34413.txt'],'verified':['SummerData original serialized fields/defaults and cycle reset boundaries','Notice/start/end comparisons and strictly-greater current-level unlock','Warmup/unlock/complete reporting before state writes','Offline states3/4 become6; reconnect from6 does not repeat unlock report','Disabled config masks status5 without replacing saved status','Concrete server clock and progression drive boundary validation; original JSON roundtrip'],'remaining':['ActivityConfigMgr/ConfigHelper103002 loading and FestActManager storage ownership','Controller initialization/listeners and reward collection rules','Production account/menu/bootstrap and Player E2E'],'checks':579,'freshPlayMode':False}
write(o/'FEST_ACTIVITY_STATE_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 活动状态边界与原始周期存档（579项）\nOutgameFestActivityState恢复4504的状态规则，读取具体ServerClock与LevelProgression：notice/start/end精确边界、关卡严格大于解锁级别、预热/解锁/完成报告顺序、断网3/4转6及重连行为。OutgameFestActivityData保留SummerData原始字段名和周期重置边界，JSON往返验证通过。579项集成通过；原始103002活动配置加载、完整FestActManager存储/奖励、事件初始化及生产owner尚未完成，无新PlayMode或Player。\n',encoding='utf8')
print('Recorded activity state and cycle data; production composition remains incomplete.')
