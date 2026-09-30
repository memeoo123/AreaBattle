from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
v=read(w/'analysis/outgame-profile-validation.json');r=read(w/'analysis/unity-integrated-validation.json');assert v['passed'] and r['passed'] and len(r['checks'])==293
now=datetime.now(timezone.utc).isoformat()
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['commander-record-initialization']={'implementationReady':True,'scope':'Missing commander records only; complete account login/guide and migration not covered. Reconstruction envelope is not the original platform wire format.','rules':[{'value':'UsedCommanderId constructor default1; missing commander curLevel0, isNew false; each configured skill seed1. Held valid records retained.','source':['disassembly/Type4029-31022.txt','disassembly/Type4028-31010.txt'],'offsets':['005e6881..005e6885','003ab1f7..003ab210','003ab368..003ab373']}]};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in v['checks']};s['cases']=[c for c in s['cases'] if c['id'] not in ids]+[dict(c,sourceContract='OUTGAME_RESTORE_SPEC.json#subsystemGates.commander-record-initialization',validationScope='Source defaults plus reconstruction disk persistence, not platform persistence parity') for c in v['checks']];write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=now;s['currentStage']='profile-consumers-and-menu-integration';s['validation'].update(integratedChecksPassed=293,profileChecksPassed=5);s['nextActions']=['Trace reachable main menu and commander entry gates; connect profile to production entry.','Implement concrete IOutgameToolEffects consumers after tracing skin/statistics/item entities; do not use no-op rewards.','Trace login/guide initial grants, original save migration and active A/B selection.'];s['milestones'].append({'id':'commander-records-and-disk-store','atUtc':now,'checks':5,'status':'partial-subsystem'})
for x in s['subsystems']:
 if x['id']=='commander-progression':x.update(evidenceStatus='core and missing-record defaults confirmed; UI/guide/migration pending',implementation='core and record initialization implemented',validation='commander core7 plus profile integration5 pass; complete subsystem pending')
 if x['id']=='profile-save-login':x.update(evidenceStatus='commander defaults confirmed; complete login/migration pending',implementation='isolated reconstruction JSON disk envelope with replacement backup; not original wire format',validation='5 profile checks pass; production menu binding pending')
write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Commander record defaults and independent durable reconstruction profile','checksPassed':293,'outgameChecksPassed':26,'freshPlayerBuild':False,'freshPlayerSmoke':False,'commands':['BattleBuild.ValidateMechanicsOnly'],'notClaimed':'Complete login, guide grants, original-platform migration, concrete dispatcher consumers, playable lobby'};write(p,s)
note='''\n\n## 关外指挥官记录初始化与磁盘存档（293项）
OutgameProfileStore.cs新增独立重建版存档封装，显式路径，与已有战斗用户档隔离；完整pending写入flush后replace，保留上一代backup。不是原平台存档格式，不宣称迁移兼容。源码确认CommanderManagerData默认选中1，InitData补建指挥官等级0、每技能等级1；有效已有记录保留，耗尽工具不补回。完整账号的新手奖励/登录授予仍未确定。
OutgameProfileValidation新增5项：默认记录、初始化保留进度、实际磁盘解锁升级扣款/耗尽重载、损坏文件保留、未知版本保留。Unity批处理退出0，集成293通过。测试仅写analysis/outgame-store-tests独立随机目录，没有触碰用户存档。当前尚未接生产大厅，也没有重建玩家程序。
下一步追踪可达MainTab/MenuTab/Commander入口及门槛，再将profile接到真实入口；IOutgameToolEffects的皮肤、实体、统计消费者需有源码依据，不可空实现后宣称完成。完整目标仍active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','293 integrated checks pass including5 profile disk/default checks; full outgame incomplete, no new player build.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded293 passing checks; goal remains incomplete.')

