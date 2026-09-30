from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');play=read(w/'analysis/outgame-ads-playmode-validation.json')
assert r['passed'] and len(r['checks'])==560 and v['passed'] and play['passed'] and len(play['checks'])==7
sources=['disassembly/Type7703-56233.txt','disassembly/Type7703-56234.txt','disassembly/Type7567-55276.txt','disassembly/Type7567-55278.txt','disassembly/Type7566-55287.txt']
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='xyx-video-bridge';s['validation'].update(integratedChecksPassed=560,loginSyncChecksPassed=len(v['checks']));write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['xyx-video-bridge']={'implementationReady':True,'productionConnected':False,'source':sources,'implementation':['OutgameXyxVideoBridge.cs','OutgameAdsVideoFlow.cs'],'scope':'XYXADControl readiness forwarding and video callback conversion restored. Per-call captured flag, bool to numeric string JSON, warning-before-platform and repeated callback behavior retained. Native PlayMode now uses this conversion instead of handwritten JSON; external platform remains fixture.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='DBTSDKManager function state cache and SDKExtension source') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'XYX video bridge callback conversion and native reward flow','checksPassed':560,'outgameChecksPassed':293,'freshPlayerBuild':False,'freshPlayerSmoke':False,'playModeReport':'analysis/outgame-ads-playmode-validation.json','playModeChecksPassed':7,'freshPlayModeRun':True,'notClaimed':'Complete pooling, production account or full lobby'};write(p,s)
audit={'status':'xyx_callback_bridge_verified_external_platform_pending','source':sources,'verified':['XYXCommon_DBT_ISDK forwards video readiness and show to XYXADControl','Each ShowVideoStatic request captures its own flag','Warning occurs before platform call','Platform bool success becomes result string0, failure string1','Repeated callbacks forwarded without deduplication','Native Ads PlayMode now enters through restored bool-to-JSON conversion'],'remaining':['BridgeManager concrete XYX implementation selection and external transport','Production SDK owner and manager initialization','Production account/menu/persistence/build/E2E'],'checks':560,'freshAdsPlaymodeChecks':7,'priorFlyPlaymodeChecks':8,'dynamicScope':'Actual native button and real wait then restored XYX callback conversion; platform completion is an explicit bool fixture, not live WeChat ad.'}
write(o/'XYX_VIDEO_BRIDGE_AUDIT.json',audit)
note='\n\n## 小游戏广告回调桥接（560项+独立7项）\n确认WX_DBT_ISDK、XYXCommon_DBT_ISDK与ISDK继承关系，并抽取XYXADControl/XYXLogin相关源证据，索引2885。新增OutgameXyxVideoBridge：广告请求先日志，独立捕获videoFlag，平台bool转换为字符串result（成功0失败1）和原JSON，重复回调继续转发。PlayMode不再手写原始JSON，而由该转换进入AdsVideoFlow和原生按钮完成链。560项集成及新7项Ads PlayMode通过；外部微信广告仍是显式bool fixture，实际BridgeManager平台选择与传输、生产账号菜单、完整构建仍待完成。\n'

for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameXyxVideoBridge','--result','pass','--evidence','560 integrated checks and7 fresh actual Ads PlayMode checks; prior8 fly PlayMode not rerun. Concrete platform implementations and production account composition pending.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded560; full goal active.')
