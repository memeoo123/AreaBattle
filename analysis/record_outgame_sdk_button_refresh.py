from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');play=read(w/'analysis/outgame-ads-playmode-validation.json')
assert r['passed'] and len(r['checks'])==557 and v['passed'] and play['passed'] and len(play['checks'])==7
sources=['disassembly/Type3135-23911.txt','disassembly/Type3134-23947.txt','sdk-function-initialization-generics.json','disassembly/Type2886-22400.txt']
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='sdk-button-refresh';s['validation'].update(integratedChecksPassed=557,loginSyncChecksPassed=len(v['checks']));write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['sdk-button-refresh']={'implementationReady':True,'productionConnected':False,'source':sources,'implementation':['OutgameSdkCapabilityResolver.cs','OutgameSdkFunctionStates.cs','OutgameSdkButtonRegistry.cs'],'scope':'Source ReshsdkFunctionBtnState restored: absent list no-op, skip destroyed buttons, apply cached IsOpen(false) for each live entry including duplicates. No cleanup or forced capability query. Initialization coroutine evidence recovered but not yet implemented; concrete platform managers and production owner pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='DBTSDKManager function state cache and SDKExtension source') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'SDK button activation refresh and cached capability integration','checksPassed':557,'outgameChecksPassed':290,'freshPlayerBuild':False,'freshPlayerSmoke':False,'playModeReport':'analysis/outgame-ads-playmode-validation.json','playModeChecksPassed':7,'freshPlayModeRun':True,'notClaimed':'Complete pooling, production account or full lobby'};write(p,s)
audit={'status':'button_refresh_verified_initialization_pending','source':sources,'verified':['Refresh unregistered function no-op','Skip Unity-null entries without cleaning registry','Each live button uses cached capability, including duplicate entries','Cached false hides and cached true reactivates buttons'],'nextEvidence':{'initializationYield':['WaitForEndOfFrame','WaitForUpdate (keepWaiting=false)'],'registration':'Dictionary<SDKOpenFunction,UnityAction>.Add','functionIdsInOrder':[5,7,8,9,10,11,12,17,19,20,21,22]},'remaining':['Implement source action initialization coroutine and concrete actions','Concrete platform manager implementations and transport','Production account/menu/persistence/build/E2E'],'checks':557,'freshAdsPlaymodeChecks':7,'priorFlyPlaymodeChecks':8,'dynamicScope':'Fresh native PlayMode; readiness and platform/report transport remain explicit fixtures'}
write(o/'SDK_BUTTON_REFRESH_AUDIT.json',audit)
note='\n\n## SDK按钮状态刷新（557项+独立7项）\n按23911还原ReshsdkFunctionBtnState：未注册无操作，逐项跳过Unity已销毁对象，读取缓存能力并设置激活状态；不强制查询平台、不清除列表。验证隐藏及重新激活、重复注册和已销毁对象处理。557项集成和重新运行7项实际Ads PlayMode通过，Unity退出0；历史8项Fly PlayMode未重跑。初始化协程证据确认先WaitForEndOfFrame，再按5/7/8/9/10/11/12/17/19/20/21/22注册动作，最后WaitForUpdate，其keepWaiting恒false。源索引2759；初始化协程实现、具体平台和完整生产链路仍待完成。\n'

for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameSdkButtonRefresh','--result','pass','--evidence','557 integrated checks and7 fresh actual Ads PlayMode checks; prior8 fly PlayMode not rerun. Concrete platform implementations and production account composition pending.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded557; full goal active.')
