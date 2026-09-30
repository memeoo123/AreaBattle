from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-baked-animation-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==369
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='native-scene-effects';s['validation'].update(integratedChecksPassed=369,bakedAnimationChecksPassed=len(v['checks']));s['nextActions']=['Connect original idle scene effect assets to production account/menu lifecycle.','Connect scene styles/effects to production menu lifecycle and validate rendering.','Complete remaining account/outgame systems and runnable end-to-end build.'];write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);s['subsystemGates']['native-scene-effects']={'implementationReady':True,'productionConnected':False,'source':['disassembly/Type4064-31260.txt','disassembly/Type4064-31258.txt','disassembly/Type4064-31255.txt','scene-effect-generic-resolution.json','scene-effect-initialize-resolution.json'],'implementation':['OutgameSceneEffects.cs','OutgameSceneEffectAssets.cs','RecoveredBossImporter.cs'],'scope':'Original source config, offset+scene cache, root Spine animation initialization, delayed callback activation and duplicate Add semantics. Three original prefabs and 34 resources imported and rendered; production wiring and original matched-frame comparison pending.'};write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='Original native scene effects, Spine animation and particles') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original native scene effects, Spine animation and particles','checksPassed':369,'outgameChecksPassed':102,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Full scene, aspect adaptation, all skin assets or playable lobby'};write(p,s)
note='\n\n## 三组原始场景附加效果（369项）\n导入古堡、沼泽 Spine 4.1.16 与火山六个原始粒子系统，共3个原生Prefab、34项依赖；保留原始材质、纹理、层级和组件数据。新增实际资源加载器、Spine循环动画/生成网格/缓存复用验证，集成369项（原267+关外102），Unity退出0。已生成并查看 scene7/8/9 隔离预览，火山粒子与沼泽动画可渲染。未声称正式账号接线或原版逐帧对照完成；整体目标继续。\n'
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
subprocess.run(b+['set-check']+a+['--name','outgameNativeSceneEffects','--result','pass','--evidence','369 integrated checks; three native effects and actual Spine geometry verified; full goal incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded369; full goal remains active.')
