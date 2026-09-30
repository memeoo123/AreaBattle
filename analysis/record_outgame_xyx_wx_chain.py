from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');play=read(w/'analysis/outgame-ads-playmode-validation.json')
assert r['passed'] and len(r['checks'])==561 and v['passed'] and play['passed'] and len(play['checks'])==7
sources=['disassembly/Type9254-64920.txt','disassembly/Type9254-64921.txt','disassembly/Type7571-55311.txt','disassembly/Type7566-55287.txt']
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='xyx-wx-module-chain';s['validation'].update(integratedChecksPassed=561,loginSyncChecksPassed=len(v['checks']));write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['xyx-wx-module-chain']={'implementationReady':True,'productionConnected':False,'source':sources,'implementation':['OutgameXyxVideoBridge.cs','OutgameAdsVideoFlow.cs','OutgameAdModuleEntry.cs','OutgameRewardVideoControllerHost.cs'],'scope':'Recovered Bridge_WX_XYXFunction forwarding connected directly to existing WX module entry through an explicit constructor. Verified SDK flow -> XYX -> WX module -> request router -> native controller callback -> JSON -> SDK completion for cancellation and success. Production owner still pending; external controller is test fixture.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='DBTSDKManager function state cache and SDKExtension source') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'SDK to WX module and controller callback composition','checksPassed':561,'outgameChecksPassed':294,'freshPlayerBuild':False,'freshPlayerSmoke':False,'playModeReport':'analysis/outgame-ads-playmode-validation.json','playModeChecksPassed':7,'freshPlayModeRun':True,'notClaimed':'Complete pooling, production account or full lobby'};write(p,s)
audit={'status':'sdk_wx_module_composition_verified_production_owner_pending','source':sources,'verified':['Bridge_WX_XYXFunction readiness and show forward to WXFunctionManger','OutgameXyxVideoBridge accepts restored WX module entry directly','SDK async request traverses module and request router to controller','Cancellation reaches raw JSON and resets SDK interval','Success reaches SDK callback and retains four-second interval'],'remaining':['BridgeManager startup selection and production composition','External native transport and configuration','Production account/menu/persistence/build/E2E'],'checks':561,'freshAdsPlaymodeChecks':7,'priorFlyPlaymodeChecks':8,'dynamicScope':'Complete restored local SDK-to-WX-module chain tested with controller fixture. Fresh Ads PlayMode regression still tests direct bool platform fixture; live WeChat is not claimed.'}
write(o/'XYX_WX_MODULE_CHAIN_AUDIT.json',audit)
note='\n\n## SDK至微信模块链路（561项+独立7项）\n确认Bridge_WX_XYXFunction64920/64921转发至WXFunctionManger，复用现有OutgameAdModuleEntry、RewardVideo、RequestRouter和ControllerHost，新增XyxVideoBridge直接连接入口的构造路径。验证SDK请求穿过所有恢复层，控制器取消/完成分别经JSON转换返回原始SDK失败/成功回调，保留门控时间语义。561项集成及新7项Ads PlayMode回归通过；完整链路测试的控制器为fixture，生产启动选择及外部微信原生服务仍未接入。源索引2888，目标未完成。\n'

for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameXyxWxModuleChain','--result','pass','--evidence','561 integrated checks and7 fresh actual Ads PlayMode checks; prior8 fly PlayMode not rerun. Concrete platform implementations and production account composition pending.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded561; full goal active.')
