from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==376
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='login-failure-state';s['validation'].update(integratedChecksPassed=376,loginSyncChecksPassed=len(v['checks']));s['nextActions']=['Resolve and implement GameDataVersionMgr reload/upload/download branches and inherited login failure handling.','Trace first LevelID assignment and connect account initialization to menu.','Complete remaining outgame systems and runnable end-to-end build.'];write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['login-failure-state']={'implementationReady':True,'productionConnected':False,'source':['disassembly/Type4612-34985.txt','disassembly/Type4612-34990.txt','disassembly/Type4612-34993.txt','disassembly/Type4612-34994.txt','login-sync-generic-resolution.json','login-sync-fields.json','disassembly/Type4610-34974.txt','login-failure-fields.json','login-failure-generic-resolution.json'],'implementation':['OutgameLoginSync.cs','OutgameLoginFailure.cs'],'scope':'Original terminal failure ordering, mode0 switch branch and nonzero login branch. Actual inherited HTTP handling and concrete SDK/data services remain pending; no production completion claim.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='Original login failure state ordering') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original login failure state ordering','checksPassed':376,'outgameChecksPassed':109,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Full scene, aspect adaptation, all skin assets or playable lobby'};write(p,s)
note='\n\n## 登录失败分支（376项）\n新增OutgameLoginFailure，保留先发送LoginProgress4、模式0清除成功标志转LoginSuccess后LoginFail5，以及非0重置计时转IdelState后原错误码通知的区别。泛型目标和LoginFail虚槽14均已解析。2项顺序/字段保持测试通过，集成376项（原267+关外109），Unity退出0。另确认HTTP错误经虚槽10，基类只记日志，不能直接套用终止处理；实际FSM/SDK/数据服务接线仍待完成。\n'
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameLoginFailureState','--result','pass','--evidence','376 integrated checks; login failure branching and transition order verified; full goal incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded376; full goal remains active.')
