"""Register managed-effect lifetime fix with source evidence and fresh player checks."""
import json,subprocess
from pathlib import Path
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';g=t/'generated';v=g/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf8'))
def write(p,j):p.write_text(json.dumps(j,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
checks=read(w/'analysis/unity-integrated-validation.json');assert checks['passed'] and len(checks['checks'])==267
assert read(w/'analysis/unity-build-report.json')['result']=='Succeeded' and read(w/'analysis/player-smoke.json')['passed']
assert read(g/'inlevel-resource-audit.json')['passedIntegrity']
assert read(v/'full-replay-diagnostic.json')==read(v/'full-replay-before-skin102.json')
now=datetime.now(timezone.utc).isoformat()
audit={'target':'wxcf1394487200e48f/43','atUtc':now,'status':'source-confirmed lifetime fix; resource transport parity incomplete','rules':[
 {'rule':'BaseEffect model loading selects NewResLoadHelper.LoadPrefabAsync or AssetbundleModule.LoadPrefab using GameFrameEntry static field36. These are distinct from a direct ResourcesModule.Load request.','source':'effect-resource-lifecycle-disassembly/f9900.txt','offsets':['0048891f..004889ae','00488ca4..00488caf']},
 {'rule':'Legacy AssetbundleModule checks its path/asset cache before the load-await branch. A non-null cached GameObject bypasses that await; no universal minimum-frame or fixed delay is established.','source':'effect-resource-lifecycle-disassembly/f9946.txt','offsets':['0048fb39..0048fbf9','0048fc38..0048fced','0048fe48..0048fe54']},
 {'rule':'BaseEffect Play awaits readiness and then calls virtual Start. Start sets active and only then creates positive-duration WaitForSeconds.','source':'effect-resource-lifecycle-disassembly/f9898.txt','offsets':['00487ef4..00487f7a'],'related':['effect-resource-lifecycle-disassembly/f9880.txt','effect-resource-lifecycle-disassembly/f13406.txt']},
 {'rule':'BaseEffect readiness predicate accepts loaded(field49) OR disposed(field50). Dispose marks field50 and destroys an existing object; pending-load races need further source-path validation.','source':'effect-resource-lifecycle-disassembly/f20505.txt','related':['effect-resource-lifecycle-disassembly/f13086.txt']}
], 'fix':{'file':'UnityProject/Assets/AreaBattle/Scripts/BattleSkillPresentation.cs','before':'Step flushes a newly created managed effect then immediately subtracts the whole preceding delta from its lifetime. A sufficiently long delta destroys the model in its creation Step.','after':'Record creation Step; managed effects created by this Step do not consume its preceding delta. Already-ready effects still count the next Step. Projectile clocks and battle mechanics are unchanged.','scope':'Lifetime only; Resources.Load remains synchronous.'},'validation':{'checksPassed':267,'newCases':['managed-effect-lifetime-starts-at-model-readiness','managed-effect-already-ready-counts-next-frame'],'build':read(w/'analysis/unity-build-report.json'),'playerSmoke':read(w/'analysis/player-smoke.json'),'numericReplayUnchanged':True,'resourceFiles':240,'guidReferences':1363},'unknowns':['Original session resource cache state and selected new/legacy transport branch are unconfirmed.','Cold-load scheduling/IO and callback delivery cannot be inferred from a single video.','No fixed 0.18378-second delay introduced; local synchronous readiness still differs from a cold original load.','Pending-close/retry resource races and complete original visual/audio parity remain open.']}
write(g/'effect-resource-readiness-audit.json',audit)
p=g/'RESTORE_SPEC.json';s=read(p);s['originalVideoReference']['managedEffectReadinessAudit']='generated/effect-resource-readiness-audit.json';write(p,s)
p=t/'BATTLEFIELD_RESTORE_STATE.json';s=read(p);m={'id':'managed-effect-readiness-lifetime-fixed','status':audit['status'],'atUtc':now,'evidence':['generated/effect-resource-readiness-audit.json']};s['milestones']=[x for x in s.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}))
s['latestValidation']={'scope':'Managed effect lifetime begins after model readiness; source cache-path audit.','checksPassed':267,'freshPlayerBuild':True,'freshPlayerSmoke':True,'smokeFrames':600,'commands':['BattleBuild.BuildAndValidate','AreaBattle.exe -battle-smoke','OriginalVideoReplayDiagnostic.Run','python analysis/asset_inlevel_audit.py'],'numericReplayUnchanged':True,'sourceAudit':'generated/effect-resource-readiness-audit.json','notClaimed':'Original cold resource loading latency, async cancellation races, or complete visual/audio parity.'};s['managedEffectReadinessAudit']=audit;write(p,s)
note='''\n\n## 特效模型就绪与寿命修复（2026-09-29，267项检查）

源码追踪确认：托管特效BaseEffect具有新/旧两条异步资源分支；旧AssetbundleModule先查缓存，命中时绕过加载等待，未证明所有请求必须延迟一帧。故不能把普通ResourcesModule待处理队列直接当作雷击显示规则，也不能加入录像测得的固定0.18378秒延迟。

已修复BattleSkillPresentation.Step的寿命错误：原版BaseEffect.Start在模型就绪后建立WaitForSeconds；本地此前在Flush创建模型后立即把同一Step已经过去的dt计入Age。现在只跳过创建Step的寿命扣除，已经就绪的模型仍正常推进，暂停仍使用scaled delta。未修改伤害、投射物时钟、兵力、种子；资源加载本身仍为同步，尚未复原原版冷加载。

新增两项检查覆盖长帧创建不立即过期、立即扣兵与展示独立、暂停、完整持续时长和已就绪模型正常到期。267项全部通过；Windows构建0错误/5条已有Spine警告，新二进制600帧smoke通过；240文件/1363 GUID检查通过。完整录像数值报告与修复前相同，截图已重生成。

可重跑证据：analysis/effect_resource_audit.py、effect_resource_lifecycle_audit.py；结论generated/effect-resource-readiness-audit.json。还需确认原会话使用的新旧资源分支、缓存状态、冷加载调度和Dispose早于加载完成时的竞态；完整视觉/听觉验收仍未通过。
'''
for p in [w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',w/'analysis/VIDEO_VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md',w/'AREA_BATTLE_HANDOFF.md']:
 s=p.read_text(encoding='utf8');marker='\n\n## 特效模型就绪与寿命修复';p.write_text(s.split(marker)[0]+note,encoding='utf8')
print('Registered267 checks, fresh build/smoke, source readiness evidence.')
