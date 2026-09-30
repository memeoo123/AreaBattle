from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-commander-view-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==315 and len(v['checks'])==7
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='commander-skill-details-bound';s['validation'].update(integratedChecksPassed=315,commanderViewChecksPassed=7)
s['nextActions']=['Recover and bind account/login startup and LevelControl providers before creating a playable lobby entry.','Complete commander 3D/acquisition bindings and concrete statistics/report/save effects.','Implement remaining outgame subsystems listed below; retain partial claim until a new lobby build and end-to-end flows pass.']
for sub in s['subsystems']:
 if sub['id']=='commander-progression':sub.update(evidenceStatus='Core, original page/card/skill detail formatting and click state confirmed; full guide/migration/model lifecycle pending',implementation='Core, initialization, action controller and dynamic original page including prices/skill descriptions/comparison rows implemented with supplied held profile; production consumers pending',validation='7 commander view checks plus existing core/action checks; editor detail capture inspected, no player entry')
 if sub['id']=='entry-navigation':sub['implementation']='Source switch state, commander overlay and imported original menu/page views connected; real account entry/providers pending'
write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['commander-skill-detail']={'implementationReady':True,'status':'confirmed','scope':'Original page skill text/comparison rendering from explicit held profile and RealCurLevel, Chinese locale. Full commander/guide/model/startup remains outside this gate.','rules':[
 {'rule':'Click any skill, including locked skills, opens details and stores slot. CommanderItem.OnClick calls RefreshSelectedSkillInfo(-1): retain selected slot and panel visibility while replacing text/rows for new commander. Close hides panel without resetting slot.','source':['disassembly/Type4335-33134.txt','disassembly/Type4334-33119.txt','disassembly/Type4307-32844.txt','disassembly/Type4307-32845.txt']},
 {'rule':'Name uses Commander.SkillName.id, target Commander.Target.targetType, operating Commander.Operating.useType. Current row SkillConfig[id*100+level], next row key+1 only when level<=9. Description uses describeData indexes over duration/data1/data2/data3, transforms dataType 4/5 to abs(value-1)*100, rounds one decimal ToEven and pads with spaces before original localized format.','source':['disassembly/Type4307-32844.txt','disassembly/Type4307-32847.txt']},
 {'rule':'Grid iterates dataType over duration/data1/data2/data3, skips zero. Type1 duration,2 skill quantity,3 fire quantity,4 slow percent,5 effect percent. Start with all five parents hidden; show configured rows. Next-current absolute delta; types4/5 transform current to abs(value-1)*100 and delta*100. Nonzero delta rounded2 ToEven in original yellow + format; zero/no next shows current only (percent rounded2).','source':['disassembly/Type4307-32829.txt','disassembly/Type4307-32827.txt','disassembly/Type4307-32851.txt']},
 {'rule':'Unlock tip visibility uses RealCurLevel < skill.unLockLevel, localized CommanderUI.UnlockLevel; selected card and background correspond to slot.','source':'disassembly/Type4307-32844.txt'}],
 'limitations':['Invariant number formatting validated for recovered Chinese presentation; other runtime cultures unverified.','Original click audio and full UI/platform lifecycle still incomplete.']};write(p,s)
p=o/'golden-cases.json';s=read(p);byid={c['id']:c for c in v['checks']}
for c in s['cases']:
 if c['id'] in byid:c.update(byid[c['id']])
s['cases'] += [dict(c,sourceContract='CommanderUI f8108/f12846/f12849 and CommanderItem.OnClick f20191; original prefab bindings') for c in v['checks'] if c['id'] not in {x['id'] for x in s['cases']}];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Source commander skill details, comparison rows and retained slot switching','checksPassed':315,'outgameChecksPassed':48,'freshPlayerBuild':False,'freshPlayerSmoke':False,'capture':'analysis/captures/outgame-skill-detail-fixture.png','notClaimed':'Full commander models/guide, production platform/stats/login consumers, account entry or full lobby'};write(p,s)
note='''

## 指挥官技能详情与数值表（315项）
OutgameSkillDescription按原CommanderUI f8108/f12849绑定技能名称、目标、操作、说明和锁定提示。SkillConfig[id*100+skillLevel]与describeData决定说明数值；速度类dataType4/5转为abs(value-1)*100；一位小数ToEven加原空格。
数值表完整恢复f12846：遍历dataType而非describeData，0隐藏，1持续时间/2技能数量/3火力数量/4减速/5增速。level<=9显示下一技能等级差值的绝对值，变化值保留两位ToEven并用原黄色+格式；不变或满级仅当前值。原Awake字段字典与ctor格式串已核对，已移除临时隐藏整表逻辑。
CommanderItem.OnClick f20191调用RefreshSelectedSkillInfo(-1)，故切换英雄保留已选技能槽及面板开关并刷新内容；增加验证防止旧英雄技能残留。Close按f6196只隐藏，保留槽位。
Unity验证退出0，315集成检查全部通过（原267+关外48），指挥官视图7项。实际渲染并查看outgame-skill-detail-fixture.png：时光守护者减速30%+10%、持续10秒、所有敌方塔/点击、第14关解锁提示，原皮肤与布局。此为隔离编辑器fixture，不是生产账户/玩家构建；所有数值测试存档仍在随机隔离目录。
完整关外目标仍active/incomplete，后续优先原登录/账号/关卡提供者与真实入口，及英雄模型、真实事件消费者和余下关外子系统。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView','outgameCommanderView']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','315 integrated checks passed, including source skill description/grid and retained slot switching; full outgame remains incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded 315 checks and source skill-detail contract; complete goal remains active.')
