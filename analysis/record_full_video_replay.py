"""Record full observed-input replay, including rejected input and visual gaps."""
import json,hashlib
from pathlib import Path
from datetime import datetime,timezone
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';G=T/'generated';V=G/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
d=read(V/'full-replay-diagnostic.json');trace=read(V/'full-input-trace.json')
assert len(trace['events'])==22 and len(d['inputs'])==25 and len(d['checkpoints'])==12
rejected=[e for e in d['inputs'] if not e['accepted']]
summary=dict(atUtc=datetime.now(timezone.utc).isoformat(),status='full-observed-input-replay-with-mismatches',matchedReplay=False,
 inputs=len(d['inputs']),acceptedInputs=sum(e['accepted'] for e in d['inputs']),rejectedInputs=rejected,
 campMatches=sum(a==b for c in d['checkpoints'] for a,b in zip(c['camps'],c['originalCamps'])),
 scoreMatches=sum(abs(a-b)<.001 for c in d['checkpoints'] for a,b in zip(c['scores'],c['originalScores'])),
 towerObservations=84,originalVictoryPts=86.0442,replayVictoryPts=d['victoryPts'],victoryDelaySeconds=d['victoryPts']-86.0442,
 finalPhase=d['finalPhase'],visualGaps=['Video comparison now explicitly selects original soldier102 pointed-hat/star-staff asset. Original account and per-camp skin selection remain inferred, not read.',
 'First checkpoint has the same three opposing line pairs3-7,4-7,6-7, but different soldier positions and two differing tower scores. Source line topology alone does not establish matching initial state.'],
 nextWork='Compare initial dispatch, collision and capture timing without fitting RNG seeds or forcing scores; continue matched animation and UI timing review.',
 evidence=[])
for name in ['full-input-trace.json','full-replay-fixture.json','full-replay-diagnostic.json','late-capacity-candidates.json','late-input-review/index.json','late-input-review-extra/index.json','late-second-dot-refinement.json','full-replay-first-checkpoint.png','full-replay-victory.png']:
 summary['evidence'].append(dict(path=name,sha256=hashlib.sha256((V/name).read_bytes()).hexdigest()))
write(V/'full-replay-summary.json',summary)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalVideoReference']['fullInputTrace']='generated/video-20260928/full-input-trace.json';v['originalVideoReference']['fullReplayDiagnostic']='generated/video-20260928/full-replay-summary.json';v['originalVideoReference']['fullReplayMatched']=False;write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);m=dict(id='original-video-full-input-replay',atUtc=summary['atUtc'],status='mismatch-observed',evidence=['generated/video-20260928/full-replay-summary.json']);v['milestones']=[x for x in v.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,v)
for p in [W/'analysis/VALIDATION_MANIFEST.json',W/'analysis/GOAL_COMPLETION_AUDIT.json']:
 v=read(p);v['fullVideoReplayDiagnostic']=summary;write(p,v)
marker='\n\n## 完整录像操作回放（未匹配）'
text=marker+f'''\n\n后半段新增 15 次已提交连线变化，合计 22 次连线变化与 3 次技能；切线滑动中的多个变化可能属于同一个手势。提交时刻由相邻原片帧的箭头、预览消失和容量圆点交叉确认。已识别并排除兵线穿过容量圆点区域导致的像素计数误判。\n\n生产场景沿录像时间轴运行至末尾，自然胜利发生在 **{d['victoryPts']:.3f} 秒**，原片为 **86.044 秒**，晚约 **{summary['victoryDelaySeconds']:.3f} 秒**。没有强行改分、修改阵营或挑选随机种子来实现结算。\n\n但这仍未通过同步验收：25 次输入只接受 {summary['acceptedInputs']} 次；75.639 秒的 6→7 连线在原片有效，回放当时 6 号塔仍属红方，因而正常拒绝。后续 81.942 秒的 6→4 虽接受，容量也因此比原片少 1。\n\n12 个采样时刻，阵营 {summary['campMatches']}/84 项一致、数值 {summary['scoreMatches']}/84 项一致。采样阵营相同不代表两帧之间的占领时刻相同，上述拒绝正是反例；胜利时间接近也不能视作完整机制一致。\n\n| 后半段原片秒数 | 操作 |\n|---|---|\n'''
for e in trace['events'][7:]:text+=f"| {e['pts']:.5f} | {'连接' if e['kind']=='connect' else '切断'} {e['source']} → {e['target']} |\n"
text+='\n首个检查点的三组敌方对向兵线（3↔7、4↔7、6↔7）与原片一致，士兵位置和塔数值仍有差异。已导出回放的兵线时钟、士兵位置与画面，便于继续追查。\n\n皮肤差异已进一步处理：录像对照现显式选择原始 soldier102 的尖帽、星形法杖网格、图集与动画。这个匹配来自画面和资产对照，不是读取原版账号；各阵营的实际存档选择仍未确认。默认通用皮肤选项保留。\n\n本次未更改生产战斗参数或正常存档。执行 `AreaBattle.EditorTools.OriginalVideoReplayDiagnostic.Run` 现在会读取 `full-replay-fixture.json` 与 `full-input-trace.json`；前一轮 62 秒诊断 JSON 保留为历史记录。\n'
for p in [W/'analysis/VIDEO_VALIDATION_REPORT.md',W/'analysis/VALIDATION_REPORT.md',W/'RESTORE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf-8').split(marker)[0]+text,encoding='utf-8')
print(json.dumps({k:v for k,v in summary.items() if k!='evidence'},ensure_ascii=False))
