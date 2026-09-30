from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-level-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==318
p=o/'ACCOUNT_STARTUP_AUDIT.json';s=read(p);s['sourceMethodsIndexed']=640
# Correct field naming after checking getter/setter and metadata: field24 is mode, not SpecialLevel.
s['confirmed'][0]['fact']='LevelControl.RealCurLevel always returns LocalDataManager.data.LevelID. CurLevel returns this only when eSpecialState(field24)==0; otherwise returns SpecialLevel(field16). SpecialLevel setter writes only field16.'
s['confirmed'][0]['source']+=['disassembly/Type4107-31496.txt','disassembly/Type4107-31512.txt']
s['confirmed'][1]['fact']=s['confirmed'][1]['fact'].replace('SpecialLevel!=0','eSpecialState!=0')
s['confirmed'] += [
 {'fact':'ProcedurePreLoad initializes user net module, registers statistics provider then initializes statistics with callback. ProcedureStarGame migrates skin and commander data from registered local string before prefab initialization and GotoGameScene.','source':['disassembly/Type4451-34120.txt','disassembly/Type4451-34123.txt','disassembly/Type4452-34135.txt']},
 {'fact':'GamePlay load callback dispatches event, reports EnterGameHome with current level, initializes rank and seven-day activity, calls SetPlaySate(1), closes loading and reports interactive. State1 initializes channel data then immediately switches state2; state2 loads current level config.','source':['disassembly/Type4452-34141.txt','disassembly/Type4107-31525.txt','disassembly/Type4107-31489.txt']},
 {'fact':'LevelConfig contains id0->SceneId0 and LevelControl normal state2 resolves current level through ConfigMgr.GetLevelConfig. No blanket level0-to1 clamp should be invented. Full first-account login initialization is not yet proven.','source':['tables/LevelConfig.json','disassembly/Type3907-30397.txt','disassembly/Type4107-31489.txt']},
 {'fact':'InitChanelData reads online hcrzd_ABtest, keeps existing value on null/empty and otherwise Int32.Parse assigns field268. Full channel table swap and failure policy need follow-up.','source':'disassembly/Type3907-30419.txt'}]
write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='account-startup-level-provider';s['validation'].update(integratedChecksPassed=318,levelProgressionChecksPassed=3)
s['nextActions']=['Finish first-account/login init and original startup event ordering; retain recovered level0 instead of assuming1.','Bind production UI to held profile and level providers with concrete statistics/report/save consumers.','Complete remaining subsystem coverage and build real lobby; editor fixtures are not completion evidence.'];write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['level-progress-provider']={'implementationReady':True,'status':'confirmed','scope':'Held normal LevelID and runtime eSpecialState/SpecialLevel providers; login, guide initialization and full level entry excluded.','rules':[
{'rule':'RealCurrentLevel=profile.levelID. CurrentLevel=eSpecialState==0?levelID:SpecialLevel. Runtime special level is not persisted as normal progression.','source':['disassembly/Type4107-31480.txt','disassembly/Type4107-31495.txt','disassembly/Type4107-31496.txt']},
{'rule':'Normal CurrentLevel setter writes raw value, calls stat10015 with signed int32(value-1) extended to int64, then local data save. Nonzero eSpecialState returns without effects. No implicit clamping.','source':'disassembly/Type4107-31470.txt'}],
'implementation':['OutgameLevelProgression.cs','OutgameProfileStore.cs'],'limitations':['Concrete statistics consumer and full lifecycle integration pending','Source raw default0 is not a claim about all first-account post-login grants']};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='LevelControl real/current/special getters and normal setter') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Source normal/special level providers, statistic/save ordering and restart persistence','checksPassed':318,'outgameChecksPassed':51,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Full login, first-account grants, production startup/platform/stats or playable lobby'};write(p,s)
note='''

## 关卡提供者与真实启动取证（318项）
新增OutgameLevelProgression，OutgameProfile持久化levelID（原LocalData标量0默认，未宣称完成首次账户初始化）。RealCurrentLevel恒为levelID；CurrentLevel在eSpecialState=0时取levelID，否则取SpecialLevel。核对原getter/setter后修正ACCOUNT_STARTUP_AUDIT：field24是eSpecialState，field16才是SpecialLevel，之前审计命名错误已纠正。正常setter原顺序写值→stat10015(value-1)→SaveLocalData；特殊模式setter无副作用；保留原unchecked int32减法且不Clamp。
新增3项验证覆盖真实隔离磁盘重启/统计顺序、特殊模式不污染普通进度、零值不篡改。原指挥官视图验证改用真实提供者，SpecialState1/SpecialLevel999/持久LevelID6和14验证技能门槛。Unity退出0，318集成全部通过（原267+关外51）。
补提取LoginProcedureBase31、流程51、ConfigMgr56方法，累计640。已证入口先迁移皮肤与指挥官再加载GamePlay，回调初始化rank/七日活动后状态1→2；状态2用CurrentLevel解析关卡。原LevelConfig确有id0/SceneId0，不能擅自把0改1。原登录网络/首次数据/完整入口仍待接通，下一步补完整调用顺序与生产消费者。没有新玩家构建，目标仍active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView','outgameCommanderView','outgameLevelProgression']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','318 integrated checks pass including normal/special level provider and real isolated restart. Complete outgame still incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded 318 checks, source level provider and startup audit.')
