"""Checkpoint the inventory source contract and current integrated validation."""
from pathlib import Path
from datetime import datetime,timezone
import json,subprocess
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf8'))
def write(p,j):p.write_text(json.dumps(j,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
now=datetime.now(timezone.utc).isoformat();checks=read(w/'analysis/unity-integrated-validation.json');inv=read(w/'analysis/outgame-inventory-validation.json');assert checks['passed'] and len(checks['checks'])==280 and inv['passed'] and len(inv['checks'])==6
p=o/'golden-cases.json';j=read(p);ids={c['id'] for c in inv['checks']};j['cases']=[c for c in j['cases'] if c['id'] not in ids]
expected=[{'missingConfiguredItems':18,'seedCount':2,'scalarInitialGrant':0},{'held2001':0,'held2002':7,'consumed2003':0,'afterRoundtrip':[0,7,0],'rowCount':18},{'scalarIds':[1001,1002,1004,1005],'rejectedAbsentIds':[1003,7999]},{'overdraftAccepted':False,'exactDebitAccepted':True,'partialMutation':False},{'initialGold':2147483647,'delta':1,'accepted':False,'finalGold':2147483647},{'initialGold':10250,'commander2UnlockAndFirstUpgrade':True,'finalGold':0,'commanderLevel':2,'skillLevels':[2,1,1]}]
j['cases'] += [{'id':c['id'],'expected':e,'result':c['result'],'sourceContract':'OUTGAME_RESTORE_SPEC.json#subsystemGates.local-inventory-core'} for c,e in zip(inv['checks'],expected)];j['scope']='Commander and local inventory cores; full account/UI/platform validation incomplete.';write(p,j)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=now;s['currentStage']='tool-reward-dispatch-and-profile-persistence'
for x in s['subsystems']:
 if x['id']=='currency-items-packages':x.update(evidenceStatus='LocalDataManager inventory confirmed; outer ToolControl dispatch under analysis',implementation='inventory core implemented; reward dispatch and packages pending',validation='6 inventory and commander-integration cases pass; subsystem incomplete')
s['validation'].update(integratedChecksPassed=280,localInventoryChecksPassed=6,newPlayerBuild=False,playableLobby=False);s['nextActions']=['Trace ToolControl.ToolChange goods-type dispatch, events/reporting/save ordering.','Finish Commander/LocalData initialization and legacy conversion before creating a shared durable profile.','Wire reachable menu and commander/inventory operations to that profile.'];s.setdefault('milestones',[]).append({'id':'local-inventory-core','atUtc':now,'checks':6,'status':'partial-subsystem'});write(p,s)
p=o/'scope-inventory.json';s=read(p);s['sourceExtractedFunctions']=len(read(o/'method-map.json'));write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,v in old.items():
 if k not in s:s[k]=v
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Outgame LocalDataManager inventory core, six source scenarios, production commander integration and JSON roundtrip.','checksPassed':280,'outgameChecksPassed':13,'freshPlayerBuild':False,'freshPlayerSmoke':False,'commands':['BattleBuild.ValidateMechanicsOnly'],'notClaimed':'Complete profile/account initialization, reward dispatcher, durable save, UI lobby or platform parity.'};write(p,s)
note='''\n\n## 关外库存核心完成，整体仍在进行（2026-09-29，280项）
本轮有实质进展：新增OutgameLocalInventory.cs，对应LocalDataManager.InitToolDictData/GetToolNum/SetToolNum（f11482/f11484/f4958）。AllSkillConfig中18个gameItem缺失记录补count2，已有0或其它余额保留；1001金币、1002钻石、1004体力、1005通用道具走标量字段；其它ID查已有ToolCount记录。未知道具写入失败，不静默新建；i32加法后负值拒绝整次变更，精确扣至0成功。注意这只是原始LocalData库存层，不能绕过ToolControl把1003礼包当体力/币处理。
新增6项验证，覆盖缺失补齐、耗尽后JSON重载不补货、四类标量映射、未知道具、余额不足原子拒绝、整数溢出、指挥官解锁+升级真实库存扣费。总280项通过，无C#错误。现有Windows程序仍是之前关内构建，本轮没有伪称可玩大厅或重建程序。未操作用户已有存档。
源证据OUTGAME_RESTORE_SPEC.json新增local-inventory-core gate；黄金用例追加到generated/outgame/golden-cases.json。首批271个源函数可在outgame/method-map.json查找。继续方向：ToolControl.ToolChange(f1799)分发/统计/事件/SaveData，再做完整本地profile和可操作入口。ItemHelper.GetGoodsType已提取到Type4286-32643.txt，DataManagerSave及ItemModuleControl已提取。初始完整账号、旧数据迁移、平台流程均未验收。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
base=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];args=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,path in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(base+['record-artifact']+args+['--kind',kind,'--path',str(path)],check=True,capture_output=True)
for name,msg in [('unityCompile','Unity compilation and280 integrated checks pass; no new Windows build this checkpoint.'),('outgameCommanderCore','7 commander core checks pass; complete UI/save integration pending.'),('outgameInventoryCore','6 local inventory cases pass, including production commander debit and JSON roundtrip. Full durable account/profile pending.')]:subprocess.run(base+['set-check']+args+['--name',name,'--result','pass','--evidence',msg,'--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded local inventory checkpoint:280 checks; outgame goal incomplete and active.')
