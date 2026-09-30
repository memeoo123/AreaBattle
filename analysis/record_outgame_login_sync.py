from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==374
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='login-sync-state';s['validation'].update(integratedChecksPassed=374,loginSyncChecksPassed=len(v['checks']));s['nextActions']=['Resolve and implement GameDataVersionMgr reload/upload/download branches and inherited login failure handling.','Trace first LevelID assignment and connect account initialization to menu.','Complete remaining outgame systems and runnable end-to-end build.'];write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['login-sync-state']={'implementationReady':True,'productionConnected':False,'source':['disassembly/Type4612-34985.txt','disassembly/Type4612-34990.txt','disassembly/Type4612-34993.txt','disassembly/Type4612-34994.txt','login-sync-generic-resolution.json','login-sync-fields.json'],'implementation':['OutgameLoginSync.cs'],'scope':'Original LoginSynData entry/leave subscriptions, old-player completion, new-player upload wait, 30-second timeout and prior-user rollback. Inherited FSM and SDK/data services are required host dependencies; production integration pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='Original login sync state, upload wait and timeout') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original login sync state, upload wait and timeout','checksPassed':374,'outgameChecksPassed':107,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Full scene, aspect adaptation, all skin assets or playable lobby'};write(p,s)
note='\n\n## 登录同步状态（374项）\n纠正前轮消息标注：GF_ReceiveDataUploaded是上传完成而非失败。新增OutgameLoginSync，原样保留新玩家等待上传、旧玩家直接进入成功状态、回调读取当前IsNewPlayer、30秒超时回退用户ID/路径/关闭同步和离开时解绑。泛型解析确认ChangeState<LoginSuccess>，源字段确认Uid/IsNewDevice/IsNewPlayer。新增4项边界与顺序验证，集成374项（原267+关外107），Unity退出0。SDK/传输/父级错误处理需真实host接线，未声称登录完成。\n'
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameLoginSyncState','--result','pass','--evidence','374 integrated checks; login sync callbacks and timeout order verified; full goal incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded374; full goal remains active.')
