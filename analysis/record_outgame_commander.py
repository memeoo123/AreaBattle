"""Checkpoint the first outgame implementation slice without claiming a runnable lobby."""
from pathlib import Path
import json,subprocess,hashlib
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';g=t/'generated';o=g/'outgame'
def read(p):return json.loads(p.read_text(encoding='utf8'))
def write(p,j):p.write_text(json.dumps(j,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
now=datetime.now(timezone.utc).isoformat();r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==274
sub=read(w/'analysis/outgame-commander-validation.json');assert sub['passed'] and len(sub['checks'])==7
expected=[{'levels':[0,1,5,6,14,15,27,28],'costs':[10000,250,250,500,500,1000,1000,10000],'commander':2},{'commander':3,'levelUnlockThreshold':22,'currencyAffordabilityIndependent':True},{'commander':2,'insufficientCoins':9999,'unlockCost':10000,'resultLevel':1,'changedSkillSlot':-1},{'commander':2,'fromLevel':1,'upgrades':3,'coinCost':750,'resultSkillLevels':[2,2,2]},{'commander':2,'fromLevel':1,'upgrades':27,'totalCoinCost':18750,'resultLevel':28,'resultSkillLevels':[10,10,10],'maxUpgradeRejected':True},{'commander':5,'currency':1002,'unlockCost':500,'firstUpgradeCost':250},{'commander':2,'fromLevel':1,'coins':249,'accepted':False,'mutations':0}]
write(o/'golden-cases.json',{'schemaVersion':'1.0','target':'wxcf1394487200e48f/43','scope':'Commander core; explicitly supplied held state and A configuration, not a fresh account.','cases':[{'id':c['id'],'expected':e,'result':c['result'],'sourceContract':'OUTGAME_RESTORE_SPEC.json#subsystemGates.commander-core'} for c,e in zip(sub['checks'],expected)]})
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=now;s['currentStage']='profile-initialization-and-item-transactions';s['nextActions']=['Recover authoritative first-run local/commander profile initialization and migration.','Recover ToolControl/ItemManager mutation, reward and save/event ordering.','Wire source-confirmed menu navigation and commander UI to shared persistent state.']
for item in s['subsystems']:
 if item['id']=='commander-progression':item.update(evidenceStatus='core-confirmed; initialization/UI gating pending',implementation='core-implemented; integration pending',validation='7 core cases pass; full subsystem incomplete')
s['validation']={'compile':'pass','integratedChecksPassed':274,'commanderCoreChecksPassed':7,'report':'../../../../outgame-commander-validation.json','newPlayerBuild':False,'playableLobby':False};s.setdefault('milestones',[]).append({'id':'commander-core','atUtc':now,'checks':7,'status':'partial-subsystem'});write(p,s)
p=o/'scope-inventory.json';s=read(p);s['sourceExtractedFunctions']=len(read(o/'method-map.json'));write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for key,value in old.items():
 if key not in s:s[key]=value
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'New outgame commander core, explicitly supplied state and config; no lobby integration yet.','checksPassed':274,'outgameCoreChecksPassed':7,'freshPlayerBuild':False,'freshPlayerSmoke':False,'commands':['BattleBuild.ValidateMechanicsOnly'],'notClaimed':'Fresh-profile/save/UI integration, runnable lobby or complete outgame restoration. Existing Windows build predates outgame core.'};s['currentGoal']='Complete out-of-battle logic restoration';s['outgameState']=str(t/'OUTGAME_RESTORE_STATE.json');write(p,s)
note='''\n\n## 当前新目标：关外逻辑完全复原（2026-09-29）
用户明确设定新的自主执行目标，目标工具已激活，无token预算。主任务转为关外完整逻辑；旧战斗复原成果及未完成视觉差异保留。ORCHESTRATION_STATE.currentWorkstream指向目标内OUTGAME_RESTORE_STATE.json，原battlefield phase/验收级别没有篡改。

已建16子系统清单，候选1417个源方法（候选不是全部本版本可达），首批249个函数已反汇编到generated/outgame/disassembly；可复跑analysis/outgame_disassemble.py并附类名。MenuTabConfig中4号HerosDetailUI是isActive=false，不要因为类存在就启用；其他指挥官入口仍需追踪。

首个实现OutgameCommanderProgression.cs：读取显式提供的CommanderConfig/UpgradeConfig及持有状态，复原NextCost、关卡门槛/资源判定分离、解锁和升级的扣款/级别/技能轮转核心。28级封顶，1→28共27次升级消费18750（A表）、技能轮流升到10；满级NextCost查询原版返回解锁费用，但实际升级拒绝满级。没有猜测新账号余额/初始持有，尚未接入UI、统计事件和保存。B表选择待核实，配置文件原样保留在Resources/Data/Outgame。

7项关外核心检查+原267项=274项通过，编辑器编译通过。未重新构建Windows；现有可执行文件仍是之前关内版本，不能宣称已有可玩关外大厅。源码/测试路径见outgame-commander-validation.json与generated/outgame/golden-cases.json。

下一步按OUTGAME_RESTORE_STATE继续：原LocalData/Commander初始化、ToolControl/ItemManager奖励入账和保存事件，再接主菜单及养成界面。不要继续以录像粒子/雷击延迟为主线，也不要把仅7项养成规则通过称为关外完成。保留既有用户profile，先建立兼容迁移契约。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
print('Outgame commander checkpoint recorded; goal remains active and incomplete.')
