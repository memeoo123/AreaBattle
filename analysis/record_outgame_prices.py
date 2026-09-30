from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-commander-view-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==310
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='commander-dynamic-content-binding';s['validation'].update(integratedChecksPassed=310,commanderViewChecksPassed=2);s['nextActions']=['Complete commander skills/3D/acquisition descriptions and bind page to account startup.','Implement real effects consumers and lifecycle saves.','Run actual rendered UI capture and new lobby build after entry wiring.'];write(p,s)
p=o/'golden-cases.json';s=read(p);byid={c['id']:c for c in v['checks']}
for c in s['cases']:
 if c['id'] in byid:c.update(byid[c['id']])
s['cases'] += [dict(c,sourceContract='CommanderItem and CommanderUI source methods plus original prefab bindings') for c in v['checks'] if c['id'] not in {x['id'] for x in s['cases']}];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Commander localized name/level, source unlock/upgrade price display and max-state visibility','checksPassed':310,'outgameChecksPassed':43,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Skill/details presentation, real platform/stats consumers, account entry or full lobby'};write(p,s)
note='''

## 指挥官价格与中文文案（310项）
OutgameCommanderView补齐原textLv、textUnlockName/textUpgradeName/textMaxLevel状态，goUpgrade价格区随满级及unlockType2隐藏；首次解锁显示unlockPrice，已拥有显示NextCost，第二价仅锁定且priceCount>1显示；首价item>=8001按原{owned}/{cost}格式及0.5缩放。当前A表实际仅1001金币/1002钻石，第二价/实体显示分支没有真实A表端到端样本，不能扩大验收。证据CommanderUI.RefreshSelectedCommanderInfo f5148 offsets001aecae..001af312及InitializeComponent字段映射。
新增OutgameLocalization读取原LanguageConfig中文列，复制完整原表未改内容；动态视图测试改用实际中文，验证时光守护者、10000解锁→250升级、500钻石价、满级隐藏价格显示满级。新增1项+原309=310，Unity退出0。UI预处理增加价格物品图标候选，共116sprites；图集同名资源的精确消歧仍待核对，不宣称像素一致。
技能/3D/解锁获取说明、真实启动和生命周期、平台/统计消费者仍未完成，无新Windows构建。目标active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView','outgameCommanderView']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','310 integrated checks pass including real dynamic commander card and upgrade button/disk integration; full outgame incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded310checks; dynamic commander cards and operations connected; full objective incomplete.')
