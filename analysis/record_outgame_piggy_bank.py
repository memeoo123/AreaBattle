from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==575
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Piggy bank state and main-page concrete providers','checksPassed':575,'outgameChecksPassed':308,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=575;write(p,s)
audit={'status':'piggy_inventory_rules_and_main_provider_verified_production_events_pending','source':['disassembly/Type3874-30169.txt','disassembly/Type3874-30172.txt','disassembly/Type3874-30173.txt','disassembly/Type3874-30178.txt'],'verified':['Current level>=30 bank visibility; collectNum>999 full','Win8 adds20 via setter, requested amount reported before upper cap1000; lower bound not invented','Reset setter0 then collectAdNum0 then activity reset report; PaySuccess empty','Uses existing profile inventory fields and survives explicit later save/reload','Main menu level and bank indicators now validated with concrete progression/bank providers'],'remaining':['Production gameplay event subscribe/unsubscribe and owner initialization','ADHelper live reporting provider; tests use explicit report sink','Easter/time service and complete account/bootstrap/Player E2E'],'checks':575,'freshPlayMode':False}
write(o/'PIGGY_BANK_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 存钱罐实际数据与主页服务连接（575项）\n新增OutgamePiggyBank恢复30关门槛、胜利8累积20、数量上限1000、报告请求值后提交、重置数量和广告次数；使用原有profile.inventory.collectNum/collectAdNum，保存重载验证通过。主页原始UI集成改用OutgameLevelProgression与存钱罐真实状态提供器，验证满额及关卡标签。575项集成通过；生产游戏状态事件订阅、报告服务、活动时间与完整启动尚待接通，无新PlayMode或Player。\n',encoding='utf8')
print('Recorded piggy bank and main data providers; production composition remains incomplete.')
