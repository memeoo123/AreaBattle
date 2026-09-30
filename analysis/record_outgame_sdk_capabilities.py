from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');play=read(w/'analysis/outgame-ads-playmode-validation.json')
assert r['passed'] and len(r['checks'])==556 and v['passed'] and play['passed'] and len(play['checks'])==7
sources=['disassembly/Type3135-23915.txt','disassembly/Type3135-23904.txt','sdk-initialization-generics.json']
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='sdk-capability-dispatch';s['validation'].update(integratedChecksPassed=556,loginSyncChecksPassed=len(v['checks']));write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['sdk-capability-dispatch']={'implementationReady':True,'productionConnected':False,'source':sources,'implementation':['OutgameSdkCapabilityResolver.cs','OutgameSdkFunctionStates.cs','OutgameSdkButtonRegistry.cs'],'scope':'Source GetState switch restored for functions0-22 and default, including zero inversion, exact draw-video status1, shared policy query and fixed feature5. Native PlayMode cache uses this resolver with explicit readiness fixture. Concrete manager implementations, full refresh and production initialization remain pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='DBTSDKManager function state cache and SDKExtension source') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'SDK capability dispatch numeric rules and cache integration','checksPassed':556,'outgameChecksPassed':289,'freshPlayerBuild':False,'freshPlayerSmoke':False,'playModeReport':'analysis/outgame-ads-playmode-validation.json','playModeChecksPassed':7,'freshPlayModeRun':True,'notClaimed':'Complete pooling, production account or full lobby'};write(p,s)
audit={'status':'source_dispatch_verified_platform_managers_pending','source':sources,'verified':['GetState functions0-22 plus defaultfalse','Feature5 fixedtrue;6 and17 invert zero;21 requires exactly1','Features11 and12 share policy manager query','Cache refresh15 uses resolver readiness query','InitDBTSDK generic calls resolve Ads,Iap,Report,AppInfo,AppUser,SDKTool in that order'],'remaining':['Concrete platform manager implementations and transport','Full SDK refresh and initialization owner','Production account/menu/persistence/build/E2E'],'checks':556,'freshAdsPlaymodeChecks':7,'priorFlyPlaymodeChecks':8,'dynamicScope':'Fresh native PlayMode; readiness and platform/report transport are explicit fixtures'}
write(o/'SDK_CAPABILITY_DISPATCH_AUDIT.json',audit)
note='\n\n## SDK能力分发（556项+独立7项）\n新增OutgameSdkCapabilityResolver，按原始GetState还原功能0到22及未知值处理；覆盖固定真、零值取反、绘制视频状态必须等于1、共享隐私策略查询和缓存强制刷新。556项集成与重新运行的7项实际Ads PlayMode通过，Unity退出0；历史8项Fly PlayMode未重跑。泛型证据确认SDK初始化依次添加Ads、Iap、Report、AppInfo、AppUser、SDKTool管理器。具体平台实现、完整刷新、生产账号菜单、持久化端到端和构建仍未完成；测试中的平台响应为显式fixture。\n'

for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameSdkCapabilityDispatch','--result','pass','--evidence','556 integrated checks and7 fresh actual Ads PlayMode checks; prior8 fly PlayMode not rerun. Concrete platform implementations and production account composition pending.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded556; full goal active.')
