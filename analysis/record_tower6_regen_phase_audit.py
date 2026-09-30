"""Decompose the measured first-regeneration delay without fitting game constants."""
import json
from pathlib import Path
from datetime import datetime,timezone
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';G=T/'generated';V=G/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
rows=read(V/'tower6-replay-timing.json')['samples'];withdraw=next(r for r in rows if r['outgoing']==0);prior=rows[rows.index(withdraw)-1]
original=read(V/'tower6-outgoing-pixels.json')['changes'];assert len(original)==1 and original[0]['after'] is False
old=read(V/'tower6-capture-audit.json');og=old['originalRegen']['events'][0]['pts'];rg=old['replayRegen']['events'][0]['pts'];ot=original[0]['pts'];rt=withdraw['pts']
baseline=read(V/'full-replay-before-skin102.json');current=read(V/'full-replay-diagnostic.json');assert current==baseline
out=dict(atUtc=datetime.now(timezone.utc).isoformat(),status='regen-phase-delay-decomposed',originalWithdrawal=original[0],replayBeforeWithdrawal=prior,replayAfterWithdrawal=withdraw,
 originalFirstRegen=og,replayFirstRegen=rg,withdrawalDelay=rt-ot,firstRegenDelay=rg-og,postWithdrawalDelayDifference=(rg-rt)-(og-ot),
 replayNumericBaselineIdentical=True,
 interpretation='The2.43391s first-regeneration delay consists of2.36786s later outgoing-line removal and0.06605s difference in time remaining until regeneration. This is timing decomposition, not an independent proof of the cause of the later capture.',
 sourceMechanism='BattleSimulation.RunAI action1 removes outgoing lines and reconnects against incoming enemies. The source list is shuffled. RegenAccumulator is preserved while outgoing lines block regeneration. This diagnostic does not reproduce the original session shuffle state.',
 correction='Original tower6 outgoing disappears while tower7 remains green. Neither original nor replay supports treating capture of tower7 as the immediate trigger for tower6 first regeneration.',
 limitations=['Original withdrawal uses visible capacity-dot change; replay records simulation state after a frame.','Original accumulated regen at withdrawal is not directly observable.','Original source-selection order and earlier incoming-line histories remain unknown.'],
 evidence=['tower6-outgoing-pixels.json','tower6-outgoing-review.png','tower6-replay-timing.json','tower6-capture-audit.json','full-replay-diagnostic.json'])
write(V/'tower6-regen-phase-audit.json',out)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalVideoReference']['tower6RegenPhaseAudit']='generated/video-20260928/tower6-regen-phase-audit.json';write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);m=dict(id='tower6-regen-phase-delay-decomposed',status=out['status'],atUtc=out['atUtc'],evidence=['generated/video-20260928/tower6-regen-phase-audit.json']);v['milestones']=[x for x in v.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,v)
p=W/'analysis/VALIDATION_MANIFEST.json';v=read(p);v['tower6RegenPhaseAudit']=out;write(p,v)
marker='\n\n## 右下塔首次回血延迟分解'
note=marker+"\n\n原片红色出兵圆点在62.88203～62.89878秒消失，接触表确认此时底部塔仍为绿色。本地只读追踪显示65.26664秒才撤去右下塔出兵线，晚2.36786秒。首次回血分别为63.63302与66.06693秒，晚2.43391秒；其中撤线后的等待差仅0.06605秒。原版画面事件和本地模拟事件有显示时差，因此不把最后几帧差异当作公式错误。\n\n本地撤线前回血累计为1.18333秒，出兵期间保持不变，撤线后继续累计至2秒。首次回血延迟主要来自战场/AI撤线时机。底部塔被占领并非立即触发回血的条件；原片在底部塔仍绿时就已撤线。AI保护分支会先撤线，再按敌方入线重新连接，受来源塔洗牌和当时入线影响；本轮未恢复原片的随机状态。\n\n只改了编辑器诊断器，新增逐帧输出，未改生产逻辑。Unity批处理回放成功退出0，12个检查点及完整数值报告与既有基线逐项相同。既有259项机制检查和600帧测试作为此前结果保留，本轮未声称重跑；未修改伤害、速度、回血或随机种子。完整战场验收仍未通过。\n"
for p in [W/'analysis/VIDEO_VALIDATION_REPORT.md',W/'analysis/VALIDATION_REPORT.md',W/'RESTORE_PROGRESS.md',T/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf-8').split(marker)[0]+note,encoding='utf-8')
print(json.dumps({k:out[k] for k in ['withdrawalDelay','firstRegenDelay','postWithdrawalDelayDifference','replayNumericBaselineIdentical']},indent=2))
