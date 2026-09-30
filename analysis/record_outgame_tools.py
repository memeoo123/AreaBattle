"""Record source tool dispatch without claiming backend/save integration."""
from pathlib import Path
from datetime import datetime,timezone
import json,subprocess
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf8'))
def write(p,j):p.write_text(json.dumps(j,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
now=datetime.now(timezone.utc).isoformat();r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-tool-validation.json');assert r['passed'] and len(r['checks'])==288 and v['passed']
expected=[{'trace':['top','gold-spent:4','changed:1001','cost:1001:1:4:6','save'],'balance':6},{'trace':['top','changed:1001','save'],'balance':3,'accepted':False},{'failedTrace':['changed:2001','changed:2001','save'],'successfulGainBalance':3},{'disabledReportSkillTrace':['changed:2001','save'],'disabledReportGenericTrace':['save']},{'accepted':False,'reportedGain':1,'balance':2147483647},{'id':999999,'type':0,'accepted':True,'trace':['save']},{'id':1003,'type':6,'delegatedSignedDelta':True,'missingEntityWarning':True,'strengthUnchanged':True},{'scene':[1,99],'soldier':[100,399],'skill':[2001,2999],'explicit':{'1001':1,'1002':2,'1005':7}}]
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in v['checks']};s['cases']=[c for c in s['cases'] if c['id'] not in ids]+[{'id':c['id'],'expected':e,'result':c['result'],'sourceContract':'OUTGAME_RESTORE_SPEC.json#subsystemGates.tool-dispatch-core'} for c,e in zip(v['checks'],expected)];s['scope']='Commander, local inventory and tool dispatch source cores; complete account, UI and external integrations remain incomplete.';write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=now;s['currentStage']='commander-profile-defaults-and-durable-store';s['nextActions']=['Finish CommanderManager.InitData/DealOldData and CommanderManagerData constructor before new-account creation.','Implement durable account and connect IOutgameToolEffects save/statistics/notifications to actual local state.','Trace and wire enabled menu/commander entry flows; retain source skin and item entity routing.']
for x in s['subsystems']:
 if x['id']=='currency-items-packages':x.update(evidenceStatus='inventory and ToolChange dispatch confirmed; entity side effects pending',implementation='inventory plus source dispatcher implemented; typed side-effect consumer integration pending',validation='14 inventory/dispatcher cases pass; full subsystem incomplete')
s['validation'].update(integratedChecksPassed=288,toolDispatchChecksPassed=8,newPlayerBuild=False,playableLobby=False);s.setdefault('milestones',[]).append({'id':'tool-dispatch-core','atUtc':now,'checks':8,'status':'partial-subsystem'});write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Source ItemHelper classification and ToolControl side-effect order,8 new checks. Observed typed sink; account store/entity/platform consumers pending.','checksPassed':288,'outgameChecksPassed':21,'freshPlayerBuild':False,'freshPlayerSmoke':False,'commands':['BattleBuild.ValidateMechanicsOnly'],'notClaimed':'Durable account save, actual platform analytics, skin/package entity implementations or playable lobby.'};write(p,s)
note='''\n\n## 关外ToolControl分发完成（2026-09-29，288项）
新增OutgameToolDispatcher.cs，对照ItemHelper.GetGoodsType f2585和ToolControl.ToolChange f1799。复原1001/1002/1005特殊分类、技能2001..2999、士兵皮肤100..399、场景皮肤1..99，其余GameItem字典分类。金币/钻石按flag刷新TopInfo；只有成功的负金币变更记消费统计；技能分支无条件通知，再按统一flag二次通知。正向变更即使库存溢出拒绝也上报Get，负向失败不上报Cost；全部正常返回路径均调用Save，未知type0返回默认true但不变库存。源码特殊行为按实保留，没有擅自去重通知或加成功才保存条件。
IOutgameToolEffects提供实际接入点；当前测试用可观察sink证明顺序，尚无完整账号保存实现，不得把接口调用当作持久化或平台验收。场景配置不存在抛出，士兵配置不存在跳过；皮肤分支忽略数量；普通实体走signed delta，缺实体警告但默认true。缺失消费者不是自由授予奖励的理由。
8项新增检查+原280=288通过，编辑器编译通过。未重建Windows/未宣称大厅可玩。GameItem/SceneSkin/Skin原表复制至Resources/Data/Outgame。下一步真正推进账号默认值和磁盘store，并绑定IOutgameToolEffects，再做关外可操作入口；尤其CommanderManager.InitData/DealOldData仍需追踪，不得猜默认拥有英雄或金币。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name,msg in [('unityCompile','Unity compilation and288 integrated checks pass; no new player build.'),('outgameCommanderCore','7 commander core cases pass.'),('outgameInventoryCore','6 inventory core cases pass.'),('outgameToolDispatchCore','8 dispatch order/source failure cases pass with observed side-effect sink; actual external/account consumers pending.')]:subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence',msg,'--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded288 checks; complete outgame objective remains active and incomplete.')
