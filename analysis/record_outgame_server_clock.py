from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==578
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Server clock refresh lifecycle and countdown integration','checksPassed':578,'outgameChecksPassed':311,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=578;write(p,s)
audit={'status':'source_clock_and_countdown_verified_native_sdk_provider_pending','source':['disassembly/Type4034-31057.txt','disassembly/Type4034-31063.txt','disassembly/Type4034-31064.txt','disassembly/Type4034-31067.txt','disassembly/Type4034-31062.txt'],'verified':['Guarded init subscribes GamePause and refreshes twice; positive SDK results are read twice per refresh','Tick uses realtime accumulated strictly>1 seconds, subtracting once; SDK nonpositive falls back to original local epoch','First day observation silent; later day-of-year change emits RefreshNetTime then Time_NewDay','Debug offset applies only long timestamp getter; seconds/calendar use raw timestamp','Dispose removes pause listener and clears owner via supplied callback','Activity countdown validated against concrete restored server clock'],'remaining':['Live SDK clock/network/release adapters and production tick owner','Activity manager/config timestamps and full menu bootstrap','Calendar week helper and full server/platform/Player E2E'],'checks':578,'freshPlayMode':False}
write(o/'SERVER_CLOCK_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 服务器时钟生命周期与倒计时连接（578项）\nOutgameServerClock恢复初始化双刷新、暂停恢复、实时时长严格大于1秒才更新、SDK正值二次读取及非正值本地时间回退。首次日期观察不广播；后续跨日按RefreshNetTime、Time_NewDay顺序发送，调试偏移仅影响长时间戳接口。活动倒计时已验证读取此具体时钟。578项集成通过；SDK时间/网络/发布状态仍通过明确提供器注入，真实平台和生产tick owner未接通，活动数据与完整启动仍待完成；无新PlayMode或Player。\n',encoding='utf8')
print('Recorded server clock lifecycle; production composition remains incomplete.')
