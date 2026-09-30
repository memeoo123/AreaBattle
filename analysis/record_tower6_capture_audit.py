"""Persist bounded original/replay differences without changing simulation inputs."""
import json
from datetime import datetime, timezone
from pathlib import Path
W=Path(__file__).resolve().parent.parent
T=W/'analysis/targets/wxcf1394487200e48f/43'; G=T/'generated'; V=G/'video-20260928'
def read(p): return json.loads(p.read_text(encoding='utf-8'))
def write(p,v): p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
pixels=read(V/'tower6-score-pixels.json')
events=read(V/'full-replay-events.json')['events']
scores=[e for e in events if e['tower']==6 and e['kind']=='score' and 52<e['pts']<77]
fire=[e for e in scores if e['pts']<61]
assert len(fire)==1 and fire[0]['value']==3
changes=pixels['changes']
def transition(a,b):
    return next(e for e in changes if e['before']==a and e['after']==b)
regen=[transition(0,1),transition(1,2)]
replay_regen=[e for e in scores if 61<e['pts']<68.2]
assert [e['value'] for e in replay_regen]==[4,5]
capture=next(e for e in events if e['tower']==6 and e['kind']=='capture' and e['pts']>60)
out=dict(atUtc=datetime.now(timezone.utc).isoformat(),status='bounded-state-divergence-confirmed',
    originalFire=dict(startScore=5,endScore=0,observedSequence=[5,3,1,0],minimumTwoPointHits=3,
        firstHitBracket=transition(5,3),
        limitations='Particles obscure later digit transitions; zero clamps damage, so score alone cannot count further hits at zero. Attribution to fire uses the input trace (no incoming player line during fire) and source two-point damage.'),
    replayFire=dict(hits=1,startScore=5,endScore=3,events=fire),
    originalRegen=dict(events=regen,interval=regen[1]['pts']-regen[0]['pts']),
    replayRegen=dict(events=replay_regen,interval=replay_regen[1]['pts']-replay_regen[0]['pts']),
    originalLateDamage=transition(2,1),replayScores=scores,
    capture=dict(originalRoofColorBracket=[73.62087,73.63778],originalScoreZeroBracket=[73.6042,73.62087],
        replayPts=capture['pts'],delayAgainstFirstBlueRoof=capture['pts']-73.63778),
    interpretation='Original enters the later attack with2 points after two regenerations, replay with5. Both measured regeneration intervals are2.001s. Original score drops2->1 at69.25234; replay has already received one attack at68.36832 and its second at69.25234. Different health and arrival histories prevent attributing the complete capture delay to movement speed or the regeneration formula.',
    limitations=['First regeneration phase and complete intervening soldier collision histories are not matched.','Three-point excess does not by itself prove an exact three-second capture delay.','No score override, seed search, speed change or production edit was applied.'],
    evidence=['tower6-score-pixels.json','fire-tower6-score-review.png','tower6-unclassified-review.png','tower6-capture-pixels.json','full-replay-events.json','full-input-trace.json'])
write(V/'tower6-capture-audit.json',out)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalVideoReference']['tower6CaptureAudit']='generated/video-20260928/tower6-capture-audit.json';write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);m=dict(id='video-tower6-fire-regen-capture-audit',status=out['status'],atUtc=out['atUtc'],evidence=['generated/video-20260928/tower6-capture-audit.json']);v['milestones']=[x for x in v.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,v)
p=W/'analysis/VALIDATION_MANIFEST.json';v=read(p);v['tower6CaptureAudit']=out;write(p,v)
marker='\n\n## 右下塔火球、回血与占领差异'
note=marker+'''\n\n原片右下塔6在火球期间可见5→3→1→0，本地仅一次扣2，5→3。首个原片5→3位于57.11237～57.12886秒。后两次变化受特效遮挡，只确认数字顺序；零值存在截断，不能据此认定总共恰好三发命中。

原片回血0→1发生在63.61593～63.63302秒，1→2发生在65.61685～65.63439秒，相隔2.00137秒。本地3→4在66.06693秒，4→5在68.06817秒，相隔2.00124秒。两边回血周期一致，但首个回血相位尚未对齐。

后续原片2→1发生在69.23584～69.25234秒；本地68.36832秒已有一次入兵，69.25234秒是第二次。原片数字降零在73.62087秒先可见，蓝色塔顶在73.63778秒先可见，本地占领76.64023秒，按塔顶颜色相差3.00245秒。数字和换色属于不同画面事件，不能把两者当作同一个精确时刻。

已确认火球结束后3点差异、相同回血周期，以及不同入兵历史；尚未证明3点差异能完整解释3秒延迟。没有修改移速、回血、伤害或随机种子来拟合原片。下一步需要核对首次回血相位及双向兵线碰撞历史。此次只新增分析脚本和证据，沿用既有259项集成检查及600帧烟雾测试；没有重新执行或新增生产测试，也没有通过完整画面验收。
'''
for p in [W/'analysis/VIDEO_VALIDATION_REPORT.md',W/'analysis/VALIDATION_REPORT.md',W/'RESTORE_PROGRESS.md',T/'REVERSE_PROGRESS.md']:
    p.write_text(p.read_text(encoding='utf-8').split(marker)[0]+note,encoding='utf-8')
print(json.dumps(dict(originalRegenInterval=out['originalRegen']['interval'],replayRegenInterval=out['replayRegen']['interval'],captureDelay=out['capture']['delayAgainstFirstBlueRoof']),indent=2))
