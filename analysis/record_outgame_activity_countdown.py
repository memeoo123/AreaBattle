from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==577
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Source activity countdown and original UI formatting','checksPassed':577,'outgameChecksPassed':310,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=577;write(p,s)
audit={'status':'countdown_and_native_label_verified_activity_clock_ownership_pending','source':['disassembly/Type4504-34377.txt','disassembly/Type4504-34374.txt','disassembly/Type4504-34401.txt','easter-controller-generics.json'],'verified':['Source state3 targets startTimeStamp32;4 targets endTimeStamp40','Server milliseconds subtract then signed divide1000 then wrap toint32; negative values retained','Time components preserve signed division/remainder and minute short conversion','Original main Text and LanguageConfig select day/hour, hour/minute, minute/second formats','UI status<2 leaves prior text; other non3/4 states render zero without clock read'],'remaining':['Activity manager status/config/endpoints and authoritative ServerTimeModule implementation','Main refresh owner wiring and periodic update lifecycle','Complete production entry and Player E2E'],'checks':577,'freshPlayMode':False}
write(o/'ACTIVITY_COUNTDOWN_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 原版活动倒计时与主页文字（577项）\n新增OutgameActivityCountdown恢复控制器4504的34377：状态3倒数至startTimeStamp，状态4倒数至endTimeStamp，使用服务器毫秒时间戳，保留负值、整数截断和分钟short转换。OutgameMainActivityTimer在真实主页Text上使用原始语言表按天/小时、小时/分钟、分钟/秒显示；状态小于2不改文字，其他非3/4状态归零且不读时钟。577项集成通过。活动状态与时间戳配置、ServerTimeModule、周期刷新及生产owner仍待完成，无新PlayMode或Player。\n',encoding='utf8')
print('Recorded source activity countdown; production composition remains incomplete.')
