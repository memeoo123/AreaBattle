from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-login-sync-validation.json');play=read(w/'analysis/outgame-fly-playmode-validation.json')
assert r['passed'] and len(r['checks'])==533 and v['passed'] and play['passed'] and len(play['checks'])==8
sources=['disassembly/Type2954-23028.txt','disassembly/Type2939-22969.txt','disassembly/Type2939-22973.txt','bundle-dependency-generics.json']
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='provider-bundle-release';s['validation'].update(integratedChecksPassed=533,loginSyncChecksPassed=len(v['checks']));write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['provider-bundle-release']={'implementationReady':True,'productionConnected':False,'source':sources,'implementation':['OutgameAssetProvider.cs','OutgameBundleReferences.cs'],'scope':'Provider destruction and dependency reference/release restored. Verified duplicate entries, unchecked counts and partial failure. Bundle acquisition/loading and production composition pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='Object<T> shared source wrapper') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original pooled object count/lifecycle wrapper','checksPassed':533,'outgameChecksPassed':266,'freshPlayerBuild':False,'freshPlayerSmoke':False,'playModeReport':'analysis/outgame-fly-playmode-validation.json','playModeChecksPassed':8,'freshPlayModeRun':False,'notClaimed':'Complete pooling, production account or full lobby'};write(p,s)
audit={'status':'provider_bundle_release_verified_acquisition_pending','source':sources,'verified':['Destroy sets IsDestroyed before owner/dependency processing and does not enforce CanDestroy','Owner count decremented then owner cleared','Dependency group traverses original list, including duplicate entries, and adjusts Int32 count unchecked','Dependency group cleared only after successful Release; failures retain earlier effects','Destroy does not clear handle list/reference count'],'remaining':['Bundle acquisition and lifetime manager','Original startup flags and platform loader','Production page/account integration and full build/E2E'],'checks':533,'priorPlaymodeChecks':8,'dynamicScope':'No new Play Mode run required for isolated bundle bookkeeping; prior8-check report does not cover bundle destruction'}
write(o/'PROVIDER_BUNDLE_RELEASE_AUDIT.json',audit)
p=o/'ACCOUNT_STARTUP_AUDIT.json';s=read(p);s['confirmed'].append({'fact':'Provider.Destroy23028 and dependency group Reference22973/Release22969 restored with partial failure semantics. Source bundle acquisition/manager scheduling pending.','source':sources,'validation':['analysis/outgame-login-sync-validation.json','analysis/outgame-fly-playmode-validation.json']});write(p,s)
note='\n\n## Provider资源依赖释放（533项）\n提取依赖组2939的8个方法，索引2122；泛型解析确认List<bundle2938>逐项枚举。新增OutgameBundleReferences，并在provider补齐Destroy23028：先置销毁，再扣Owner并清空，随后逐项释放依赖，成功后清空依赖组。保留重复项、unchecked负数计数、失败前部分修改和重试继续扣减，不擅自检查CanDestroy。533项集成通过，Unity退出0。此前8项Play Mode报告保留，本轮未重跑且不用于证明依赖释放。bundle获取/管理器调度、原平台加载与关外完整页面/账号验收仍待完成。\n' 
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameProviderBundleRelease','--result','pass','--evidence','533 integrated checks and8 independent actual Play Mode checks; complete pool and production flows pending.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded533; full goal active.')
