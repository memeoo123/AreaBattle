from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');play=read(w/'analysis/outgame-ads-playmode-validation.json')
assert r['passed'] and len(r['checks'])==559 and v['passed'] and play['passed'] and len(play['checks'])==7
sources=['disassembly/Type3135-23916.txt', 'disassembly/Type3135-23917.txt', 'disassembly/Type3135-23918.txt', 'disassembly/Type3135-23919.txt', 'disassembly/Type3135-23920.txt', 'disassembly/Type3135-23921.txt', 'disassembly/Type3135-23922.txt', 'disassembly/Type3135-23923.txt', 'disassembly/Type3135-23924.txt', 'disassembly/Type3135-23925.txt', 'disassembly/Type3135-23926.txt', 'disassembly/Type3135-23927.txt']
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='sdk-function-actions';s['validation'].update(integratedChecksPassed=559,loginSyncChecksPassed=len(v['checks']));write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['sdk-function-actions']={'implementationReady':True,'productionConnected':False,'source':sources,'implementation':['OutgameSdkFunctionActions.cs','OutgameSdkFunctionInitialization.cs','OutgameSdkButtonRegistry.cs'],'scope':'Twelve original action bodies restored against typed manager boundary. CloseAdsTips short-circuits cached feature15 check and localized toast; login true; share empty; remaining source manager routes preserved. Concrete managers and production startup scheduling pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='DBTSDKManager function state cache and SDKExtension source') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'SDK action bodies and exact manager routing','checksPassed':559,'outgameChecksPassed':292,'freshPlayerBuild':False,'freshPlayerSmoke':False,'playModeReport':'analysis/outgame-ads-playmode-validation.json','playModeChecksPassed':7,'freshPlayModeRun':True,'notClaimed':'Complete pooling, production account or full lobby'};write(p,s)
audit={'status':'action_bodies_verified_concrete_managers_pending','source':sources,'verified':['AdsVideoFunction checks CloseAdsTips before cached feature15','No-ad hint uses GameFrameWorkLang key Sdk_NoAdsTips','Share action empty','LoginComStatic receives true','Feedback, GDPR, policy, terms, publication, restore, game center, draw ads and privacy recall route to original manager method'],'fieldEvidence':{'typeIndex':3135,'type':'DBTSDKManager','field':'CloseAdsTips','staticOffset':5,'typeUsageAddress':3930128},'remaining':['Concrete platform manager implementations and startup composition','Production account/menu/persistence/build/E2E'],'checks':559,'freshAdsPlaymodeChecks':7,'priorFlyPlaymodeChecks':8,'dynamicScope':'Action contract tests use explicit manager probe; fresh Ads PlayMode regression does not claim external platform operations.'}
write(o/'SDK_FUNCTION_ACTIONS_AUDIT.json',audit)
note='\n\n## SDK动作宿主（559项+独立7项）\n新增OutgameSdkFunctionActions，恢复原始十二个动作：CloseAdsTips静态偏移5先行阻止提示，随后检查缓存功能15并读取Sdk_NoAdsTips本地化文字；登录固定true，分享为空，其余严格转发原管理器方法。559项集成及新7项Ads PlayMode回归通过，Unity退出0。管理器接口仍是明确边界，尚未接入具体平台服务；不得据此宣称真实登录、内购或广告已完成。下一步恢复具体SDK管理器及启动组合，再继续生产关外全流程与构建。\n'

for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameSdkFunctionActions','--result','pass','--evidence','559 integrated checks and7 fresh actual Ads PlayMode checks; prior8 fly PlayMode not rerun. Concrete platform implementations and production account composition pending.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded559; full goal active.')
