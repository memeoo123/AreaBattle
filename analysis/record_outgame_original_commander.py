from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-original-commander-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==326
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='original-commander-migration';s['validation'].update(integratedChecksPassed=326,originalCommanderChecksPassed=4)
s['nextActions']=['Recover skin old data and remaining startup migrations before binding account loading to production UI.','Complete concrete statistics/report/save consumers and original entry/lifecycle ordering.','Finish full outgame subsystem coverage, new player build and end-to-end validation.'];write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['original-commander-migration']={'implementationReady':True,'status':'confirmed','scope':'Supplied legacy CommanderManagerData JSON and source InitData; complete login/platform workflow excluded.','rules':[
{'rule':'DealOldData ignores null/decoded-null/empty commanderDatas. A nonempty list replaces held CommanderManagerData wholesale, then InitData indexes configured IDs and appends missing defaults. No max-level merging, ownership normalization, auto-equip or save occurs in this method.','source':['disassembly/Type4028-31018.txt','disassembly/Type4028-31010.txt']},
{'rule':'DTO fields CommanderManagerData.commanderDatas/UsedCommanderId, CommanderData.isNew/Id/curLevel/skillsData and CommanderSkillData.skillId/skillLevel preserved. UsedCommanderId constructor default1. Held skill IDs and order must survive; configuration skills only initialize missing records.','source':['disassembly/Type4029-31022.txt','disassembly/Type4028-31010.txt','../gameplay-symbols.json']}],
'implementation':['OutgameOriginalCommanders.cs','OutgameCommanderProgression.cs','OutgameCommanderSkillCards.cs','OutgameCommanderView.cs'],
'limitations':['Malformed payload handling uses precommit reconstruction validation, not a claim of exact original exceptions','Full skin and account startup migration still pending','Old reconstruction envelopes without skillIds retain configuration fallback; original imported records preserve explicit IDs']};write(p,s)
p=o/'ACCOUNT_STARTUP_AUDIT.json';s=read(p);s['confirmed'].append({'fact':'CommanderManager.DealOldData replaces manager data when commanderDatas is nonempty, then InitData preserves held records and adds missing config rows. Stored skillId/order, curLevel/isNew and UsedCommanderId remain unchanged. This is not a merge-by-highest-level operation.','source':['disassembly/Type4028-31018.txt','disassembly/Type4028-31010.txt','disassembly/Type4029-31022.txt']});write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='CommanderManager.DealOldData/InitData and original record field identities') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original commander migration, held skill identity in view/upgrade and restart persistence','checksPassed':326,'outgameChecksPassed':59,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Full skin/other migrations, original login and real lobby entry'};write(p,s)
note='''

## 原英雄旧存档迁移（326项）
新增OutgameOriginalCommanders.ApplyLegacy，按CommanderManager.DealOldData f12859：null/decoded-null/空列表不改当前数据，非空commanderDatas整体替换，再执行原InitData补齐缺失配置英雄；不按最高等级合并，不篡改已选ID、不额外保存。原isNew/Id/curLevel/skillsData(skillId,skillLevel)全映射，UsedCommanderId默认1。低等级旧数据会按原规则替换高等级当前记录，输入仅隔离fixture、未导入任何用户文件。
修复此前重建仅保存skillLevels的不足：OutgameCommanderState新增skillIds/isNew，缺失记录取配置技能，导入记录保留技能编号与顺序；升级、技能卡片和详情使用held SkillIds。旧重建envelope未存skillIds时兼容回退配置。未命中英雄配置的原记录留在数据列表但不生成页面卡片，与原InitData有效字典范围一致。
新增四项测试：整体替换与记录保留、空旧数据不覆盖、按保留技能槽升级并重启、原UI详情/卡片使用导入技能ID与等级（测试故意调换顺序防止假阳性）。Unity退出0，326集成检查通过（原267+关外59）。完整皮肤/其他迁移、真实登录与关外入口仍待完成，无新玩家构建，目标active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView','outgameCommanderView','outgameLevelProgression','outgameOriginalLocalData','outgameOriginalCommanders']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','326 integrated checks passed including original commander migration, held skill identity and restart/view integration. Full goal incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded 326 checks; original commander migration implemented.')
