"""Register a measured replay mismatch without promoting it to an acceptance pass."""
import hashlib,json
from pathlib import Path
from datetime import datetime,timezone
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';G=T/'generated';V=G/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
d=read(V/'timed-replay-diagnostic.json');regen=read(V/'regen-phase-evidence.json')
assert len(d['checkpoints'])==7 and len(d['inputs'])==10 and all(x['accepted'] for x in d['inputs'])
camp_matches=sum(a==b for c in d['checkpoints'] for a,b in zip(c['camps'],c['originalCamps']))
score_matches=sum(abs(a-b)<.001 for c in d['checkpoints'] for a,b in zip(c['scores'],c['originalScores']))
summary=dict(atUtc=datetime.now(timezone.utc).isoformat(),status='executed-with-mismatches',matchedReplay=False,
 acceptedInputs=10,checkpointCount=7,campMatches=camp_matches,totalTowerObservations=49,scoreMatches=score_matches,
 earliestCheckedMismatchPts=d['checkpoints'][0]['pts'],preRecordingSeconds=d['assumedPreRecordingSeconds'],
 maximumScoreDifference=max(abs(a-b) for c in d['checkpoints'] for a,b in zip(c['scores'],c['originalScores'])),
 conclusion='Initial mismatch predates first player input. Regeneration-phase calibration alone does not recover unknown original soldiers/AI/RNG. Do not retune source mechanics to fit this recording.',
 nextWork='Trace later original player inputs and compare initial opposing-line soldiers/AI dispatch timing; preserve unknown RNG rather than searching seeds for a visual fit.',
 evidence=[])
for name in ['timed-replay-fixture.json','timed-replay-diagnostic.json','timed-replay-initial-9seconds.json','regen-phase-evidence.json','early-input-trace.json']:
 summary['evidence'].append(dict(path=name,sha256=hashlib.sha256((V/name).read_bytes()).hexdigest()))
write(V/'timed-replay-summary.json',summary)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalVideoReference']['timedReplayDiagnostic']=dict(evidence='generated/video-20260928/timed-replay-summary.json',status=summary['status'],matched=False);write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);m=dict(id='original-video-timed-replay-diagnostic',atUtc=summary['atUtc'],status='mismatch-observed',evidence=['generated/video-20260928/timed-replay-summary.json']);v['milestones']=[x for x in v.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,v)
p=W/'analysis/VALIDATION_MANIFEST.json';v=read(p);v['timedReplayDiagnostic']=summary;write(p,v)
p=W/'analysis/GOAL_COMPLETION_AUDIT.json';v=read(p);v['blockerAudit']['timedReplayDiagnostic']=summary;write(p,v)
marker='\n\n## 按录像时间轴的连续回放诊断'
text=marker+f'''\n\n已用生产 BattleView、实际物理连线拓扑和原版配置，执行前 62.32 秒内已确认的 7 次连线操作与 3 次技能。10 次输入均接受，7 个检查点共 49 个塔阵营全部相同；塔数值仅 {score_matches}/49 项相同，最大差值 {summary['maximumScoreDifference']:g}。这是一份**未匹配的诊断结果**，不是整局验收通过。\n\n原片蓝塔 19→20 首帧为 1.26736 秒（上一帧 1.25060），20→21 为 3.26884 秒（上一帧 3.25180）。结合初始 15、每两秒增兵以及约 0.806 秒恢复暂停，条件性推断录像前运行时间为 9.53864–9.55540 秒；本次用中点 {d['assumedPreRecordingSeconds']:.5f} 秒。该推断依赖此前无蓝塔操作，且尚有画面延迟。\n\n先前使用 9 秒假设的基线结果也完整保留。校准增兵相位后并未让整局全部吻合，因此没有选择“误差更小”的时长或搜索随机种子，也没有修改正常存档、生产机制或原始配置。\n\n塔编号顺序：左上、右上、中左、外左、中右、外右、底部。\n\n| 原片秒数 | 原版数值 | 回放数值 | 最大差值 |\n|---|---|---|---|\n'''
for c in d['checkpoints']:
 fmt=lambda arr:', '.join(f'{x:g}' for x in arr)
 delta=max(abs(a-b) for a,b in zip(c['scores'],c['originalScores']))
 text+=f"| {c['pts']:.3f} | {fmt(c['originalScores'])} | {fmt(c['scores'])} | {delta:g} |\n"
text+='\n第一个检查点（1.017 秒）已经有两座塔相差 1，早于 4.103 秒的第一次已知玩家操作。原版未知的已有士兵、AI 历史、兵线时钟和随机状态仍需进一步区分；不能用这份差异表单独认定连线、冰冻或伤害公式错误。\n\n可重复执行：`AreaBattle.EditorTools.OriginalVideoReplayDiagnostic.Run`；输入与结果在 `generated/video-20260928/timed-replay-*.json`，增兵像素测量脚本为 `analysis/video_regen_measure.py`。范围止于雷击之后，尚未完成后半段玩家输入与胜利全过程同步。\n'
for p in [W/'analysis/VIDEO_VALIDATION_REPORT.md',W/'analysis/VALIDATION_REPORT.md',W/'RESTORE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf-8').split(marker)[0]+text,encoding='utf-8')
print(json.dumps({k:v for k,v in summary.items() if k!='evidence'},ensure_ascii=False))
