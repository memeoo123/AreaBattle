from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==566
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Complete source play-state branch table with explicit host boundaries','checksPassed':566,'outgameChecksPassed':299,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=566;write(p,s)
audit={'status':'branch_table_verified_concrete_hosts_pending','source':['disassembly/Type4107-31525.txt','play-state-ui-generics.json'],'verified':['States1/2 home entry ordering','State4 fail reward eligibility and checked flag','State5 guide versus normal start and music ordering','States6/7 boss rain, speed and AudioCompositeBase.Pause(slot7,metadata26386)','States8/9 win/fail UI and side effects','State13 only retrieves play UI','State3 and10/11/12/14/15 delegate recovered transition implementations','Unknown state branch no-op'],'remaining':['Concrete shared-state and branch hosts','Original home-level loading completion and startup/account/menu ownership','Actual scene loading, persistence and Player E2E'],'checks':566,'freshPlayMode':False}
write(o/'LEVEL_STATE_BRANCHES_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 完整玩法状态分支表（566项）\n新增OutgameLevelStateBranches补齐1/2/4/5/6/7/8/9/13，组合现有3和10/11/12/14/15。保留失败奖励资格、教学与普通开战、恢复/暂停音频、胜负界面与统计顺序、未知状态无操作。元数据确认音频虚槽7为AudioCompositeBase.Pause26386。566项集成通过；这里只证明完整分发表及契约，具体宿主仍未连接实际生产场景。下一步集中恢复主页关卡加载完成与实际入口宿主，不再把分支测试当作关外可玩验收。\n',encoding='utf8')
print('Recorded main-start binding; production composition remains incomplete.')
