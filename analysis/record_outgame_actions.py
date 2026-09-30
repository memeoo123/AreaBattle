from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
v=read(w/'analysis/outgame-commander-action-validation.json');r=read(w/'analysis/unity-integrated-validation.json');assert v['passed'] and r['passed'] and len(r['checks'])==302
now=datetime.now(timezone.utc).isoformat()
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['commander-page-actions']={'implementationReady':True,'scope':'Selection and upgrade action flow; required effects injected. Real page rendering, stats manager, platform delivery and lifecycle saving remain pending.','rules':[{'value':'Changed selected item: if level>0 immediately set UsedCommanderId and ReportUse; locked item only previews. Selecting identical item is no-op. No save in this method.','source':'disassembly/Type4307-32832.txt'},{'value':'Locked ClickUpgrade first tests Unlockable(currentLevel>=unlockLevel); then all unlock prices. Owned path checks first-currency nextCost without repeating level gate.','source':['disassembly/Type4307-32834.txt','disassembly/Type4307-32841.txt','disassembly/Type4307-32852.txt']},{'value':'Successful manager transition: Add DailyCommUpgrade1; Set CommanderUpgrade(commanderId,level); Add CommanderUpgradeTotal1; UserDataPrefs.OnSave then DataManagerPool.SaveData. IDs read from original StatisticEventConfig.','source':'disassembly/Type4028-31016.txt','offset':'005e643d..005e64af'},{'value':'Control wrapper reports upgraded ID with CURRENTLY USED commander level, before unlock UI auto-equip. Unlock UI when newlevel1 shows popup then sets usedID, reports unlock/use. No extra save in these methods.','source':['disassembly/Type4027-30995.txt','disassembly/Type4027-30991.txt','disassembly/Type4307-32841.txt'],'offset':'005e58b9..005e592f'}]};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in v['checks']};s['cases']=[c for c in s['cases'] if c['id'] not in ids]+[dict(c,sourceContract='OUTGAME_RESTORE_SPEC.json#subsystemGates.commander-page-actions') for c in v['checks']];write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=now;s['currentStage']='production-page-binding-and-lifecycle';s['validation'].update(integratedChecksPassed=302,commanderActionChecksPassed=5);s['nextActions']=['Bind actual runtime menu/commander views; action controller is ready, page rendering still missing.','Trace lifecycle save after selection/autoequip, then wire concrete commander stats/platform effects and tool effects.','Continue skins/store entity consumers and remaining full outgame scope.'];s['milestones'].append({'id':'commander-page-actions','atUtc':now,'checks':5,'status':'partial-subsystem'})
for x in s['subsystems']:
 if x['id']=='commander-progression':x.update(implementation='core, record initialization and page action controller implemented; visual page and consumers pending',validation='5 new action flow checks; full subsystem incomplete')
write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Commander page action flow, source save/report order','checksPassed':302,'outgameChecksPassed':35,'freshPlayerBuild':False,'freshPlayerSmoke':False,'commands':['BattleBuild.ValidateMechanicsOnly'],'notClaimed':'Actual page rendering, concrete stats/platform consumers, lifecycle save after equip or complete lobby'};write(p,s)
note='''\n\n## 指挥官页面操作顺序（302项）
新增OutgameCommanderActions.cs及5项验证，原始StatisticEventConfig复制到Outgame资源并按字段读取统计ID。选中未解锁英雄只预览，已解锁则立即出战并ReportUse；同一选择不重复操作。锁定英雄点击解锁先关卡门槛后价格；已拥有升级不重验解锁关卡。
成功后按源码顺序统计DailyCommUpgrade/CommanderUpgrade/CommanderUpgradeTotal，再UserPrefs保存与Manager保存，控制器ReportLevelUp使用当前出战英雄等级（不是被升级英雄等级），解锁UI随后弹窗、自动出战、ReportUnlock/ReportUse。真实磁盘回调测试确认保存发生在自动出战之前：磁盘有已扣款和已解锁，但仍为旧usedID，内存为新usedID。因此需要后续生命周期保存；不可悄悄添加即时保存并宣称源码一致。
IOutgameCommanderEffects为必需依赖，测试观测统计/报告顺序，尚无真实统计管理器或平台上报消费者；实际页面也未绑定。Unity退出0，集成302通过，尚无新Windows构建。完整目标active/incomplete。下一步必须推进可操作页面绑定和退出/返回生命周期，避免仅完成模型层。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','302 integrated checks pass including5 commander page action order checks; page/consumer integration and full goal incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded302 checks; complete outgame remains incomplete.')
