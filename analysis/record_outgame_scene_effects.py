from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-baked-animation-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==368
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='scene-effect-cache-logic';s['validation'].update(integratedChecksPassed=368,bakedAnimationChecksPassed=len(v['checks']));s['nextActions']=['Import original three idle scene effects with Spine and particle components.','Connect scene styles/effects to production menu lifecycle and validate rendering.','Complete remaining account/outgame systems and runnable end-to-end build.'];write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['scene-effect-cache-logic']={'implementationReady':True,'productionConnected':False,'source':['disassembly/Type4064-31260.txt','disassembly/Type4064-31258.txt','disassembly/Type4064-31255.txt','scene-effect-generic-resolution.json'],'implementation':['OutgameSceneEffects.cs'],'scope':'Original source config, offset+scene cache, root Spine animation initialization, delayed callback activation and duplicate Add semantics. Actual effect prefabs and production wiring pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='Original scene effect cache and callback behavior') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original scene effect cache and callback behavior','checksPassed':368,'outgameChecksPassed':101,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Full scene, aspect adaptation, all skin assets or playable lobby'};write(p,s)
note='\n\n## 场景附加效果缓存逻辑（368项）\n新增 OutgameSceneEffects，使用原始 SceneEffectConfig，保留 offset+sceneId 缓存、复用只激活不重设父节点、异步回调无选择检查、重复加载完成 Dictionary.Add 失败行为；根 SkeletonAnimation 初始化后播放 animation 循环。新增异步顺序与缓存隔离测试，集成368项（原267+关外101），Unity退出0。三组实际效果资源已下载，Spine/粒子原组件导入与生产场景接线仍待完成；整体目标继续。\n'
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameSceneEffectCacheLogic','--result','pass','--evidence','368 integrated checks; scene effect cache and callback behavior verified; full goal incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded368; full goal remains active.')
