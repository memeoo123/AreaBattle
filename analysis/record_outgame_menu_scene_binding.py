from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-menu-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==370
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='menu-scene-request-binding';s['validation'].update(integratedChecksPassed=370,menuChecksPassed=len(v['checks']));s['nextActions']=['Connect original idle scene effect assets to production account/menu lifecycle.','Connect scene styles/effects to production menu lifecycle and validate rendering.','Complete remaining account/outgame systems and runnable end-to-end build.'];write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['menu-scene-request-binding']={'implementationReady':True,'productionConnected':False,'source':['disassembly/Type4379-33464.txt','disassembly/Type4379-33473.txt','disassembly/Type4064-31261.txt'],'implementation':['OutgameMenuView.cs','OutgameMenuSceneBinding.cs'],'scope':'Source accepted request timing connects native menu to shop/home scene presentation before tween completes; rejected requests and commander route do not invoke MoveCamera. Full account/menu initialization still pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='Native menu accepted-request scene binding') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Native menu accepted-request scene binding','checksPassed':370,'outgameChecksPassed':103,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Full scene, aspect adaptation, all skin assets or playable lobby'};write(p,s)
note='\n\n## 菜单请求与场景切换接线（370项）\nOutgameMenuView 发出导航成功事件，OutgameMenuSceneBinding 按 MenuTabUI f5890/f11385 在 CheckUI 成功返回后立即调用 MoveCamera；拒绝请求、重复页面、锁定统帅不产生场景副作用。预览已使用实际绑定替代手动移动。原生页面与模型根联合验证通过，370项（原267+关外103），Unity退出0；正式账号入口与完整生命周期仍未完成。\n'
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameMenuSceneBinding','--result','pass','--evidence','370 integrated checks; native menu and scene binding verified; full goal incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded370; full goal remains active.')
