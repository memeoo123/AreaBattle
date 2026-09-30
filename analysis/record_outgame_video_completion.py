from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');play=read(w/'analysis/outgame-fly-playmode-validation.json')
assert r['passed'] and len(r['checks'])==538 and v['passed'] and play['passed'] and len(play['checks'])==8
sources=['disassembly/Type10741-'+str(i)+'.txt' for i in [74412,74416,74427,74428,74429]]+['video-button-generics.json']
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='shop-video-completion';s['validation'].update(integratedChecksPassed=538,loginSyncChecksPassed=len(v['checks']));write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['shop-video-completion']={'implementationReady':True,'productionConnected':False,'source':sources,'implementation':['OutgameVideoButtonCompletion.cs','OutgameShopLifecycle.cs','OutgameShopFeedback.cs'],'scope':'UIVideoBtn completion and native UnityEvent<bool> registrations wired through ShopEventHost to ShopFeedback and inventory. Click/cooldown, report coroutine and ad platform host remain pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='UIVideoBtn completion/listener source subset') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Video completion source ordering and original ShopUI inventory integration','checksPassed':538,'outgameChecksPassed':271,'freshPlayerBuild':False,'freshPlayerSmoke':False,'playModeReport':'analysis/outgame-fly-playmode-validation.json','playModeChecksPassed':8,'freshPlayModeRun':False,'notClaimed':'Complete pooling, production account or full lobby'};write(p,s)
audit={'status':'video_completion_verified_click_and_platform_pending','source':sources,'verified':['UnityEvent<bool> generic AddListener and Invoke resolved from source metadata','showType2 resets report state and activeSelf controls delayed report restart','Success/failure reporting precedes completion event; errors interrupt rewards','Native ShopUI video components grant50gold/20diamonds through recovered inventory dispatcher','Failed video completion grants nothing','Shop Dispose retains video listeners; explicit RemoveVideoBtnListeners removes runtime callbacks','OnDestroy marks destroyed and clears both events'],'remaining':['Original click gating, cooldown and button state','DelayReport coroutine and report transport','Actual advertisement host','Production account/menu composition, persistence/build/E2E'],'checks':538,'priorPlaymodeChecks':8,'dynamicScope':'Native ShopUI integrated editor verification; previous fly PlayMode not rerun'}
write(o/'SHOP_VIDEO_COMPLETION_AUDIT.json',audit)
p=o/'ACCOUNT_STARTUP_AUDIT.json';s=read(p);s['confirmed'].append({'fact':'UIVideoBtn completion/listener subset restored and connected to native ShopUI reward callbacks. Original click/ad host and production account/menu composition remain pending.','source':sources,'validation':['analysis/outgame-login-sync-validation.json','analysis/outgame-fly-playmode-validation.json']});write(p,s)
note='\n\n## 商店视频完成回调（538项）\n提取UIVideoBtn及嵌套类35方法，索引2185。解析UnityEvent<bool>泛型绑定，恢复完成上报与奖励事件顺序、activeSelf条件、显式移除监听和销毁清理。原ShopUI视频组件经生命周期注册接入库存发奖：成功50金币/20钻石，失败无奖励，上报异常中断发奖。538项集成通过（关外271项），Unity退出0。此前8项飞币PlayMode未重跑。点击、冷却、DelayReport、广告平台及生产账号菜单入口仍待恢复，完整目标保持进行中。\n'

for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameShopVideoCompletion','--result','pass','--evidence','538 integrated checks; prior8 PlayMode checks not rerun. Video click/ad platform and production flows pending.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded538; full goal active.')
