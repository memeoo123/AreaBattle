from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==565
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Retry/next-level/return-home source state branches','checksPassed':565,'outgameChecksPassed':298,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=565;write(p,s)
audit={'status':'continuation_branches_verified_concrete_hosts_pending','source':['disassembly/Type4107-31525.txt','disassembly/Type4107-31502.txt','play-state-ui-generics.json'],'verified':['States10 and15 differ in small-index reset','State12 initializes then increments main level and closes pause UI','State14 initializes then increments small level','State11 clears ADHelper flag28 and waits for Hide callback','Return callback closes Play/Guide UI, clears special mode, initializes, changes to2 then Show(null)'],'remaining':['State branches1/2/4/5/6/7/8/9/13 and concrete hosts','Production startup/account/menu ownership','Actual scene loading, persistence and Player E2E'],'checks':565,'freshPlayMode':False}
write(o/'LEVEL_CONTINUATION_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 重试、下一关与返回主页（565项）\n新增OutgameLevelContinuation还原状态10/11/12/14/15及31502回调，保留初始化与主/小关递增顺序、两种重试的索引差异，以及返回主页等待Hide后清理PlayUI/GuideUI、清除特殊状态、切状态2再Show的时序。泛型证据明确UI类型。565项集成通过，未新跑PlayMode或Player。完整分支、具体宿主和生产入口仍待完成，不能视为实际场景已能返回主页。\n',encoding='utf8')
print('Recorded main-start binding; production composition remains incomplete.')
