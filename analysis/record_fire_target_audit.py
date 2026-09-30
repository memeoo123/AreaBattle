"""Explain a measured score mismatch without treating seed equality as gameplay fidelity."""
import json,hashlib,subprocess
from pathlib import Path
from datetime import datetime,timezone
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';G=T/'generated';V=G/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
pixel=read(V/'fire-blue2-score-pixels.json');steps=[x for x in pixel['changes'] if x['before'] is not None and x['after'] is not None]
assert [(x['before'],x['after']) for x in steps]==[(32,34),(34,36),(36,38)]
events=read(V/'full-replay-events.json')['events'];scores=[e for e in events if e['kind']=='score' and e['tower']==2 and 52<e['pts']<61]
assert [e['value'] for e in scores]==[34,36,38,40,42,44,46]
assert not any(e['kind']=='arrival' and e['tower']==2 and 52<e['pts']<61 for e in events)
src=V/'fire-target-source';src.mkdir(exist_ok=True)
for function in ['2673','10954']:
 r=subprocess.run(['python',str(W/'analysis/flow_disassemble.py'),'@'+function],capture_output=True,check=True)
 (src/(function+'.txt')).write_bytes(r.stdout)
reference=G/'model-binding/LevelControl-InitEnemySkin.txt';assert reference.exists()
out=dict(atUtc=datetime.now(timezone.utc).isoformat(),status='one-score-divergence-explained',originalScoreTransitions=steps,
 replayScoreTransitions=scores,originalFriendlyHits=3,replayFriendlyHits=7,healPerHit=2,explainedScoreDifference=8,
 evidence=dict(originalPixel='fire-blue2-score-pixels.json',replay='full-replay-events.json',sourceSelector=['fire-target-source/2673.txt','fire-target-source/10954.txt'],sourceEnemySkinInitialization='generated/model-binding/LevelControl-InitEnemySkin.txt'),
 mechanism='Fire coroutine gathers active towers, selects through shared RandomHelper list selector, then heals same-camp targets by2. Tower2 has outgoing lines and no incoming line in this interval, so no regeneration/arrival contribution.',
 originalSelector='Function2673 delegates to10954;10954 reads shared System.Random, obtains list count, calls the virtual count-bound random method, then indexes the list. No camp weighting in this helper.',
 knownSequenceGap='Original InitEnemySkin makes3 random selections for each of3 enemy skin slots. The explicit reconstructed video cosmetic fixture does not reproduce these original selections, prior account/session draws or unknown random state.',
 interpretation='The8-point excess at blue2 is exactly four extra random heals, not evidence of wrong healing strength. This explains this tower only, not tower6 capture delay or the complete random sequence.',
 limitations=['Original full25 target sequence has not been recovered.','Enemy-skin initialization range depends on original config selection; no arbitrary nine-call offset or seed fitting is applied.','Do not infer full gameplay equivalence from one explained score.'])
write(V/'fire-target-audit.json',out)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalVideoReference']['fireTargetAudit']='generated/video-20260928/fire-target-audit.json';write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);m=dict(id='video-fire-blue2-eight-point-explanation',status='bounded-difference-explained',atUtc=out['atUtc'],evidence=['generated/video-20260928/fire-target-audit.json']);v['milestones']=[x for x in v.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,v)
p=W/'analysis/VALIDATION_MANIFEST.json';v=read(p);v['fireTargetAudit']=out;write(p,v)
marker='\n\n## 火球落点导致的右上塔数值差异'
note=marker+'\n\n右上蓝塔2在火球期间无入兵，已有出兵线使其不自行增兵。逐帧数字匹配得到原片三次各加2：\n\n| 原片首个可见帧 | 数值 |\n|---|---|\n'
for e in steps:note+=f"| {e['pts']:.5f}秒 | {e['before']}→{e['after']} |\n"
note+='\n还原回放在53.827、54.027、54.845、55.278、56.529、57.163、58.213秒共发生7次加2，32→46；原片32→38。4次额外治疗恰好解释该塔的8点误差，不需要修改单次治疗量。数字被后续绿色治疗特效遮挡的帧标记为未知，没有当作0或新增治疗。\n\n源码确认火球从活跃塔列表经共享RandomHelper选取；此外原版入关的敌方皮肤初始化有9次随机选择，而本地录像对照使用显式皮肤组合，未复现原始账号/会话随机状态。即使随意给相同种子，也不能据此要求落点逐个相同。本轮未为拟合录像增加虚构随机调用或更换种子。\n\n结论只解释右上塔8点差异。25发火球完整目标序列及右下塔占领偏晚的分项原因仍未全部确认；生产伤害、治疗、随机选择代码均未改动，因此继续沿用已通过的259项检查与构建结果。\n'
for p in [W/'analysis/VIDEO_VALIDATION_REPORT.md',W/'analysis/VALIDATION_REPORT.md',W/'RESTORE_PROGRESS.md',T/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf-8').split(marker)[0]+note,encoding='utf-8')
print('Blue2 original3 vs replay7 heals;8-point discrepancy explained. Production code unchanged.')
