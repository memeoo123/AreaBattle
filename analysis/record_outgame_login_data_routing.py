from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==380
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='login-data-routing';s['validation'].update(integratedChecksPassed=380,loginSyncChecksPassed=len(v['checks']));s['nextActions']=['Resolve and implement GameDataVersionMgr reload/upload/download branches and inherited login failure handling.','Trace first LevelID assignment and connect account initialization to menu.','Complete remaining outgame systems and runnable end-to-end build.'];write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['login-data-routing']={'implementationReady':True,'productionConnected':False,'source':['disassembly/Type4612-34985.txt','disassembly/Type4612-34990.txt','disassembly/Type4612-34993.txt','disassembly/Type4612-34994.txt','login-sync-generic-resolution.json','login-sync-fields.json','disassembly/Type4610-34974.txt','login-failure-fields.json','login-failure-generic-resolution.json','login-success-generic-resolution.json','disassembly/Type4611-34978.txt','disassembly/Type4611-34983.txt','disassembly/Type3795-29617.txt'],'implementation':['OutgameLoginSync.cs','OutgameLoginFailure.cs','OutgameLoginSuccess.cs','OutgameLoginDataPlan.cs'],'scope':'Source new-player force-upload priority and same-user/new-device manager-reload routing, with boundary matrix checks. Actual manager reload, version negotiation, HTTP and production entry remain pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='Original login data upload versus reload routing') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original login data upload versus reload routing','checksPassed':380,'outgameChecksPassed':113,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Full scene, aspect adaptation, all skin assets or playable lobby'};write(p,s)
note='\n\n## 登录数据重载决策（380项）\n新增OutgameLoginDataPlan，确认本地同步标志0且新玩家时强制上传并跳过内存重载，优先于新设备/切换账号；其余情况仅不同账号或新设备触发重载。同账号字符串按原版精确比较。新增账号/设备/新玩家/同步标志矩阵验证，集成380项（原267+关外113），Unity退出0。该决策尚未代替完整数据版本与传输实现；整体未完成。\n'
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameLoginDataRouting','--result','pass','--evidence','380 integrated checks; login data routing priority matrix verified; full goal incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded380; full goal remains active.')
