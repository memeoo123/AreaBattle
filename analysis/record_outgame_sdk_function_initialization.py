from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');play=read(w/'analysis/outgame-ads-playmode-validation.json')
assert r['passed'] and len(r['checks'])==558 and v['passed'] and play['passed'] and len(play['checks'])==7
sources=['disassembly/Type3135-23911.txt','disassembly/Type3134-23947.txt','sdk-function-initialization-generics.json','disassembly/Type2886-22400.txt']
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='sdk-function-initialization';s['validation'].update(integratedChecksPassed=558,loginSyncChecksPassed=len(v['checks']));write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['sdk-function-initialization']={'implementationReady':True,'productionConnected':False,'source':sources,'implementation':['OutgameSdkFunctionInitialization.cs','OutgameSdkButtonRegistry.cs'],'scope':'Initialization coroutine restored: WaitForEndOfFrame, twelve Dictionary.Add registrations in source order, then WaitForUpdate with keepWaiting=false. Native button bindings consume same action dictionary. Action owner remains explicit interface pending concrete source platform implementations and production startup integration.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='DBTSDKManager function state cache and SDKExtension source') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'SDK action initialization source timing and button dispatch','checksPassed':558,'outgameChecksPassed':291,'freshPlayerBuild':False,'freshPlayerSmoke':False,'playModeReport':'analysis/outgame-ads-playmode-validation.json','playModeChecksPassed':7,'freshPlayModeRun':True,'notClaimed':'Complete pooling, production account or full lobby'};write(p,s)
audit={'status':'action_registration_verified_concrete_owner_pending','source':sources,'verified':['First yield is WaitForEndOfFrame with no premature registration','Twelve actions registered through Add in source order','Second yield is CustomYieldInstruction with keepWaiting=false','Native button consumes registered login action','Duplicate Add fails after earlier registrations without rollback'],'functionIdsInOrder':[5,7,8,9,10,11,12,17,19,20,21,22],'remaining':['Concrete action owner and platform manager implementations','Production SDK startup scheduling','Production account/menu/persistence/build/E2E'],'checks':558,'freshAdsPlaymodeChecks':7,'priorFlyPlaymodeChecks':8,'dynamicScope':'Initialization timing covered by iterator contract tests and actual native button invocation; fresh Ads PlayMode regression does not yet schedule this initialization coroutine.'}
write(o/'SDK_FUNCTION_INITIALIZATION_AUDIT.json',audit)
note='\n\n## SDK动作初始化（558项+独立7项）\n新增OutgameSdkFunctionInitialization，按原始协程先WaitForEndOfFrame，再依次Add十二个功能动作，最后WaitForUpdate（keepWaiting=false）。验证帧边界前无注册、原生按钮消费同一动作字典、对应动作分发、重复键导致部分注册保留后抛异常。558项集成与7项新Ads PlayMode回归通过，Unity退出0；初始化自身的调度时序目前为迭代器契约验证，尚未接入真实生产启动。具体动作宿主、平台实现、完整关外生产与构建仍待完成。\n'

for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameSdkFunctionInitialization','--result','pass','--evidence','558 integrated checks and7 fresh actual Ads PlayMode checks; prior8 fly PlayMode not rerun. Concrete platform implementations and production account composition pending.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded558; full goal active.')
