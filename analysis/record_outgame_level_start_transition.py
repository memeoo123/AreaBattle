from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==563
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Source state3 loading/menu/delayed level transition','checksPassed':563,'outgameChecksPassed':296,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=563;write(p,s)
audit={'status':'state3_branch_verified_full_dispatch_pending','source':['disassembly/Type4107-31525.txt','disassembly/Type4107-31474.txt','disassembly/Type4107-31479.txt'],'verified':['OpenLoading then field48=true and field40=5f','InitializeEnemySkin then LevelControl field53=false','Close existing menu then append0.5second callback','Delayed callback queries current level at invocation, loads it, then initializes special scene','SetPlaySate ignores second bool and returns true after OnGamePlayState'],'remaining':['Shared state notifications, special-mode bypass and complete state dispatch','Production DOTween scheduling and loading view owner','Concrete level loader/scene lifecycle plus account/menu/persistence/build/E2E'],'checks':563,'freshPlayMode':False}
write(o/'LEVEL_START_TRANSITION_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 开始关卡过渡（563项）\n新增OutgameLevelStartTransition还原OnGamePlayState状态3分支：加载界面字段48=true、40=5f，初始化敌方皮肤，重置LevelControl字段53，关闭现有菜单，0.5秒后读取当时的当前关卡、加载关卡并初始化特殊场景。源31479的第二bool参数实际上不参与分发，返回true；不能自行添加该参数的效果。563项集成通过，未新跑PlayMode或Player。此类仅恢复状态3分支；公共状态通知、特殊模式绕过、实际延时宿主与生产入口仍待连接。\n',encoding='utf8')
print('Recorded main-start binding; production composition remains incomplete.')
