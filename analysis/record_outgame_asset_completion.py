from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');play=read(w/'analysis/outgame-fly-playmode-validation.json')
assert r['passed'] and len(r['checks'])==531 and v['passed'] and play['passed'] and len(play['checks'])==8
sources=['disassembly/Type2929-'+str(i)+'.txt' for i in [22899,22900,22907]]+['disassembly/Type2930-22916.txt']
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='asset-handle-completion';s['validation'].update(integratedChecksPassed=531,loginSyncChecksPassed=len(v['checks']));write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['asset-handle-completion']={'implementationReady':True,'productionConnected':False,'source':sources,'implementation':['OutgameAssetHandle.cs'],'scope':'Asset handle Completed add/remove/dispatch restored with validity checks, immediate completed subscription and multicast failure semantics. Provider-triggered dispatch and production loading still pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='Object<T> shared source wrapper') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original pooled object count/lifecycle wrapper','checksPassed':531,'outgameChecksPassed':264,'freshPlayerBuild':False,'freshPlayerSmoke':False,'playModeReport':'analysis/outgame-fly-playmode-validation.json','playModeChecksPassed':8,'notClaimed':'Complete pooling, production account or full lobby'};write(p,s)
audit={'status':'completion_contract_verified_provider_dispatch_pending','source':sources,'verified':['Add checks validity; already-done provider invokes new delegate synchronously without storing it','Pending subscription uses multicast combine; removal uses delegate remove','Dispatch invokes current multicast snapshot, retains subscriptions and does not catch exceptions','Callback removal does not change in-flight snapshot; following invocation sees removal','Released handle removal warns and throws Exception with original message'],'remaining':['Provider done transition and trigger ordering','Original loader mode initialization','Native provider references and pool lifecycle composition','Production pages/account and full build/E2E'],'checks':531,'playmodeChecks':8,'dynamicScope':'8 Play Mode checks cover existing fly flow; completion edge cases are integrated callback tests'}
write(o/'ASSET_HANDLE_COMPLETION_AUDIT.json',audit)
p=o/'ACCOUNT_STARTUP_AUDIT.json';s=read(p);s['confirmed'].append({'fact':'Asset handle Completed event restored from22907/22900/22899 with immediate callbacks and retained multicast semantics. Provider-driven completion still pending.','source':sources,'validation':['analysis/outgame-login-sync-validation.json','analysis/outgame-fly-playmode-validation.json']});write(p,s)
note='\n\n## 资源加载完成事件（531项 + 独立8项动态检查）\n依据22907/22900/22899恢复OutgameAssetHandle.Completed：无效句柄告警后抛原Exception；已完成订阅立即同步调用且不存储；等待状态保存多播委托；完成派发保留列表且不吞异常。验证回调内退订的快照语义、重复派发保留、立即订阅、抛错截断和释放后退订。531项集成通过，8项飞币Play Mode回归通过；后者不代表加载事件动态链已接通。provider完成触发、原加载器/引用计数、生产页面账号及完整构建仍待完成。\n' 
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameAssetHandleCompletion','--result','pass','--evidence','531 integrated checks and8 independent actual Play Mode checks; complete pool and production flows pending.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded531; full goal active.')
