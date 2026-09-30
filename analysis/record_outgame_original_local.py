from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-original-local-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==322
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='original-local-data-import';s['validation'].update(integratedChecksPassed=322,originalLocalDataChecksPassed=4)
s['nextActions']=['Recover source commander/skin old payload migration and startup account sequencing; raw local JSON is preserved for these consumers.','Bind actual startup UI and concrete statistics/report/save consumers.','Complete all remaining outgame subsystem coverage, player build and end-to-end validation.'];write(p,s)
p=o/'ACCOUNT_STARTUP_AUDIT.json';s=read(p);s['sourceMethodsIndexed']=642;s['confirmed'] += [{'fact':'DealUnsafeStr performs ordinal String.Replace(bank,collect) on the whole local JSON. Existing nonempty toolCounts suppress old-field migration. Empty toolCounts parses OldToolPropertyClass, then initializes tool rows and overwrites only old values !=-1. Six old fields default2 and map2001 iceNum/2002 supportNum/2003 toolnum_4/2004 fireNum/2005 upgradeNum/2006 toolnum_5.','source':['disassembly/Type4119-31618.txt','disassembly/Type4119-31627.txt','disassembly/Type4119-31633.txt','disassembly/Type4119-31626.txt','disassembly/Type4125-31641.txt','disassembly/Type4125-31642.txt']}];write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['original-local-data-import']={'implementationReady':True,'status':'confirmed','scope':'Provided original local JSON to recovered scalar/tool state. Remaining subsystem fields retained verbatim; not a full original save/login replacement.','rules':[
{'rule':'Null/empty original payload initializes scalar0 and missing skill tools2. Nonempty payload replaces bank with collect before parsing local fields. Raw source payload preserved separately for future consumers.','source':['disassembly/Type4119-31618.txt','disassembly/Type4119-31627.txt','disassembly/Type4119-31633.txt']},
{'rule':'Only empty original toolCounts enables legacy tool parsing. Initialize missing AllSkill rows then migrate old six fields when !=-1; absent fields default2. Other negative values are not clamped. Modern nonempty rows, including count0, win.','source':['disassembly/Type4119-31626.txt','disassembly/Type4125-31641.txt','disassembly/Type4125-31642.txt']}],
'implementation':['OutgameOriginalLocalData.cs','OutgameProfileStore.cs','OutgameLocalInventory.cs'], 'limitations':['Limited bag initialization, remaining LocalData fields, skin/commander old migration and full account startup pending','Importer commits only after parsing succeeds to preserve held reconstruction profile; no malformed source payload compatibility claim']};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='LocalDataManager.UpdateDataCallBack/DealUnsafeStr/DeadOldData and OldToolPropertyClass') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original local scalar/tool JSON import with source legacy migration and raw preservation','checksPassed':322,'outgameChecksPassed':55,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Complete original wire format, full legacy migrations, login or playable lobby'};write(p,s)
note='''

## 原本地数据导入与旧道具迁移（322项）
新增OutgameOriginalLocalData.Apply，读取提供的原始LocalData JSON，仅提交已恢复的LevelID、货币、collectNum/collectAdNum、toolCounts。按DealUnsafeStr全串bank→collect替换，sourceLocalDataJson原样留存，包括未知字段，供后续皮肤/指挥官等消费者使用；不对真实用户文件执行导入。
空/缺省数据只初始化原标量0和缺失技能道具2，不添加虚构首关/货币奖励。仅原toolCounts为空启用旧六字段迁移；OldToolPropertyClass ctor所有字段2，2001冰/2002支援/2003toolnum_4/2004火/2005升级/2006toolnum_5，-1跳过覆盖，其他负数按源码保留；现代非空列表优先，保留数量0。解析成功后才提交到重建profile，失败保留当前与磁盘存档，此为导入器保护策略、不声称原坏JSON容错完全相同。
四项新验证通过：六字段+bank迁移、新旧优先级/-1默认、空数据、原文和未知字段持久化/坏JSON不污染。Unity退出0，322集成检查（原267+关外55）全部通过。原方法索引642。完整登录、其他旧数据迁移、主入口与关外其余系统仍未完成，没有新玩家构建，目标保持active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView','outgameCommanderView','outgameLevelProgression','outgameOriginalLocalData']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','322 integrated checks pass including original local scalar/tool import, legacy precedence and isolated restart/failure preservation. Full outgame incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded 322 checks; original local import partial scope explicit.')
