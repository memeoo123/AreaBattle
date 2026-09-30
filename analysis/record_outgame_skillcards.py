from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-commander-view-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==312
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='commander-dynamic-content-binding';s['validation'].update(integratedChecksPassed=312,commanderViewChecksPassed=4);s['nextActions']=['Complete commander skills/3D/acquisition descriptions and bind page to account startup.','Implement real effects consumers and lifecycle saves.','Run actual rendered UI capture and new lobby build after entry wiring.'];write(p,s)
p=o/'golden-cases.json';s=read(p);byid={c['id']:c for c in v['checks']}
for c in s['cases']:
 if c['id'] in byid:c.update(byid[c['id']])
s['cases'] += [dict(c,sourceContract='CommanderItem and CommanderUI source methods plus original prefab bindings') for c in v['checks'] if c['id'] not in {x['id'] for x in s['cases']}];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original dynamic commander skill cards, RealCurLevel gates and next upgrade hint','checksPassed':312,'outgameChecksPassed':45,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Skill/details presentation, real platform/stats consumers, account entry or full lobby'};write(p,s)
note='''

## 指挥官技能卡片（312项）
新增OutgameCommanderSkillCards，克隆原CommanderSkillItem到objSkillContent，来源CommanderSkillData.UnlockLevel/Unlock及CommanderSkillItem.SetData/RefreshData/RefreshUpgradeInfo。解锁使用独立RealCurLevel>=AllSkillConfig.unLockLevel（6/14/21），不是CurLevel或英雄等级；未命中配置返回0。原技能图标来自GameItem[id+2000].ItemIcon，卡片锁与等级背景互斥、等级中文前缀、升级箭头仅owned&&!max且slot==(heroLevel-1)%3。技能点击暴露SkillSelected(slot)，实际详情数据仍未绑定，不宣称完成。
新用例CurLevel999而RealCurLevel6仅第一技能解锁，RealCurLevel14第二解锁第三保持锁，真实英雄升级刷新技能1到等级2/箭头切到slot1。准备脚本补技能图标，共134sprites。实际渲染并查看确认三张卡片出现。渲染发现字面\n，补提取LangModule共21方法，Get f1243原000674be..000674d2明确String.Replace(escaped newline,newline)，OutgameLocalization按源码修复。method-map现370。重新渲染确认换行提示正确。
312集成检查通过，Unity退出0。完整技能详情/技能效果说明、英雄模型、真实启动与所有剩余关外系统仍待完成，无新玩家构建，完整目标active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView','outgameCommanderView']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','312 integrated checks pass including real dynamic commander card and upgrade button/disk integration; full outgame incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded312checks; dynamic commander cards and operations connected; full objective incomplete.')

