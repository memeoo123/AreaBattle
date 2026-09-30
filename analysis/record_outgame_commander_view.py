from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-commander-view-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==309
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='commander-dynamic-content-binding';s['validation'].update(integratedChecksPassed=309,commanderViewChecksPassed=1);s['nextActions']=['Complete commander detail/skills/price rendering and bind page to account startup.','Implement real effects consumers and lifecycle saves.','Run actual rendered UI capture and new lobby build after entry wiring.'];write(p,s)
p=o/'golden-cases.json';s=read(p);byid={c['id']:c for c in v['checks']}
for c in s['cases']:
 if c['id'] in byid:c.update(byid[c['id']])
s['cases'] += [dict(c,sourceContract='CommanderItem and CommanderUI source methods plus original prefab bindings') for c in v['checks'] if c['id'] not in {x['id'] for x in s['cases']}];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Dynamic CommanderItem clone, original portraits, selection, real upgrade buttons and disk persistence','checksPassed':309,'outgameChecksPassed':42,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Skill/details presentation, real platform/stats consumers, account entry or full lobby'};write(p,s)
note='''

## 原始指挥官动态卡片与操作接线（309项）
OutgameCommanderView.cs克隆原CommanderItem模板到CommanderContent，按传入档案顺序生成卡片，绑定原始独立头像（IconName原配置offset44），选择/锁定/出战/等级状态与源码红点规则，并绑定原btnUpgrade到OutgameCommanderActions。成功解锁/升级后刷新卡片与按钮。原素材准备脚本扩展所有CommanderConfig.IconName，107sprites（原101+6）重新导入。
新集成用例用真实Prefab Button.onClick：锁定卡片预览→10000金币解锁自动出战→250升级→磁盘重载余额0/等级2/used2；验证动态头像不同、锁与出战标记及文字刷新。显式fixture金币10250/level999仅测试，不是账号默认。统计/报告使用观测sink，实际磁盘Save回调执行。309集成通过，编译退出0。
技能详情/3D模型/价格说明仍未接，当前标题/等级由外部localize函数提供；真实启动入口、本地化消费者、统计和平台消费者、退出生命周期仍待完成。暂无新Windows构建或视觉完整验收，目标active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView','outgameCommanderView']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','309 integrated checks pass including real dynamic commander card and upgrade button/disk integration; full outgame incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded309checks; dynamic commander cards and operations connected; full objective incomplete.')
