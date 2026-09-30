from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==582
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Fest activity late initialization and manager-pool composition','checksPassed':582,'outgameChecksPassed':315,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=582;write(p,s)
import hashlib
source=t/'generated/tables/SummerRewardConfig.json';asset=w/'UnityProject/Assets/AreaBattle/Resources/Data/Outgame/SummerRewardConfig.json';assert source.read_bytes()==asset.read_bytes()
audit={'status':'concrete_activity_components_composed_in_validation_production_owner_pending','source':['disassembly/Type4504-34397.txt','disassembly/Type4504-34396.txt','data-manager-registration-roster.json'],'verified':['Original4505 registration flags create concrete FestActManager through DataManagerPool','Late init disabled branch no-op; enabled config then CheckInit then state refresh then icons','Historical local dates complete with no stat subscription; future dates explicitly fixture-only','Noncomplete init enables updates and subscribes statistic10000','Statistic callback refreshes based on old status before state recomputation; next completed callback removes listener','Pool save/reload preserves shared manager status used by controller and countdown'],'remaining':['Actual CommonMsgDispatcher/GameStatistics publisher binding','Full controller gameplay/WarWin listener and UI ownership','Production startup instantiation of composed components and Player E2E'],'checks':582,'freshPlayMode':False}
write(o/'FEST_INITIALIZATION_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 活动后期初始化与数据池联合验证（582项）\nOutgameFestActivityInitialization恢复OnLateInit34397及统计回调34396。配置、活动周期检查、状态计算、入口刷新、统计订阅按原序执行；实际FestActManager按原始4505注册标记加入DataManagerPool并保存共享状态。原始历史日期进入结束态；另用明确未来时间fixture验证旧状态先刷新入口、再计算到期、下次回调退订的顺序。582项集成通过。真实统计发布器、完整WarWin监听、生产启动owner和Player流程仍未完成，本次无新PlayMode或Player。\n',encoding='utf8')
print('Recorded fest late initialization; production composition remains incomplete.')
